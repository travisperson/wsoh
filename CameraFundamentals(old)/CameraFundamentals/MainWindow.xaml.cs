/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do a basic green screen.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using Emgu.CV;
using Emgu.CV.Structure;

namespace CameraFundamentals
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Kinect Runtime
        Runtime nui = new Runtime(); 

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            byte[] ipArr = new byte[4];
            ipArr[0] = (byte)10;
            ipArr[1] = (byte)2;
            ipArr[2] = (byte)4;
            ipArr[3] = (byte)26;
            System.Net.IPAddress tIP = new System.Net.IPAddress(ipArr);
            System.Net.IPEndPoint travis = new System.Net.IPEndPoint(tIP,3890);
            myClient.Connect(travis);
            //setup event handlers
            nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
            nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
            
            //Initialize to return both Color & Depth images
            nui.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking);

            //open both streams
            nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            DateTime lastTime = DateTime.Now;
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;

            //get the first tracked skeleton
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     select s).FirstOrDefault();
            //textBlock2.Text ="W: " + e.SkeletonFrame.FloorClipPlane.W.ToString() + "\tX: " + e.SkeletonFrame.FloorClipPlane.X.ToString() + "\tY: " + e.SkeletonFrame.FloorClipPlane.X.ToString() + "\tZ: " + e.SkeletonFrame.FloorClipPlane.X.ToString();
            //textBlock2.Text = "X: " + skeleton.Joints[JointID.HandLeft].Position.X.ToString() + "\tY: " + skeleton.Joints[JointID.HandLeft].Position.Y.ToString() + "\tZ: " + skeleton.Joints[JointID.HandLeft].Position.Z.ToString();
        }
        UdpClient myClient = new UdpClient(3890);
        DateTime lastTime;
        int totalFrames = 0;
        int lastFrames;
        int ballframes = 0;
        int listSize = 0;
        int forwardCount = 0;
        int realBallsSent = 0;
        int virtualBallsSent = 0;
        List<Microsoft.Research.Kinect.Nui.Vector> pointList = new List<Microsoft.Research.Kinect.Nui.Vector>();
        List<long> numList = new List<long>();
        List<TimeSpan> timeList = new List<TimeSpan>();
        TimeSpan frameSpan = new TimeSpan();
        bool virtualBall = false;
        float xVel, yVel, zVel,xDisplace,yDisplace,zDisplace;

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            int i16=0,x,y;
            long dx = 0, dy = 0;
            double bx=0, by=0, bz=0;
            short depth;
            int count = 0;
            Microsoft.Research.Kinect.Nui.Vector point;
            DateTime cur = DateTime.Now;
            frameSpan = cur.Subtract(lastTime);
            //textBox1.Text += cur.Subtract(lastTime).ToString() + '\n';

            //work on virtual ball
            if (virtualBall)
            {
                xDisplace = xDisplace + (xVel * frameSpan.Milliseconds)/1000;
                yDisplace = yDisplace + (yVel * frameSpan.Milliseconds)/1000;
                zDisplace = zDisplace + (zVel * frameSpan.Milliseconds)/1000;
                //send to server here!!
                string dataString = (xDisplace+0.3f).ToString() + " " + (yDisplace-0.1f).ToString() + " " + zDisplace.ToString();
                Byte[] sendBytes = Encoding.ASCII.GetBytes(dataString);
                myClient.Send(sendBytes, sendBytes.Length); //virtual ball pos sent to server  
                virtualBallsSent++;
                textBlock10.Text = "Virtual balls sent: " + virtualBallsSent.ToString();

                textBlock9.Text = xDisplace.ToString() + " " + yDisplace.ToString() + " " + zDisplace.ToString();
                if(yDisplace < -1.5 || Math.Abs(xDisplace) > 2 || zDisplace > 8) //out of play bounds
                    virtualBall = false;
                return;
            }

            //process current depth frame
            PlanarImage depthData = e.ImageFrame.Image;
            for (y=0;y<depthData.Height;y++)
            {
                for(x=0;x<depthData.Width;x++)
                {
                    if ((depthData.Bits[i16] & 0x07) != 0)
                    {
                        depthData.Bits[i16 + 1] = (byte)0;
                        depthData.Bits[i16] = (byte)0;
                        i16 += 2;
                        continue;
                    }
                    depth = (short)((depthData.Bits[i16+1]<<8) | depthData.Bits[i16]);
                    point = nui.SkeletonEngine.DepthImageToSkeleton(((float)x) / ((float)depthData.Width), ((float)y) / ((float)depthData.Height),depth);
                    if (point.Z < 1 || point.Z > 3.8 || point.Y < -1 || point.Y > 1.2)
                    {
                        //not ball
                        depthData.Bits[i16 + 1] = (byte)0;
                        depthData.Bits[i16] = (byte)0;
                    }
                    else
                    {
                        //ball
                        count++;
                        dx += x;
                        dy += y;
                        bx += point.X;
                        by += point.Y;
                        bz += point.Z;
                        depthData.Bits[i16 + 1] = (byte)60;
                        depthData.Bits[i16] = (byte)60;
                    }
                    i16 += 2;
                }
            }
            if (count > 80 && count < 4000)
            {
                //ball detected in frame
                ballframes++;
                dx /= count;
                dy /= count;
                bx /= count;
                by /= count;
                bz /= count;
                i16 = (int)dy * depthData.Width * depthData.BytesPerPixel + (int)dx * depthData.BytesPerPixel;
                depthData.Bits[i16] = (byte)255;
                depthData.Bits[i16 + 1] = (byte)255;
                textBlock1.Text = "X: " + bx.ToString();
                textBlock2.Text = "Y: " + by.ToString();
                textBlock3.Text = "Z: " + bz.ToString();
                textBlock4.Text = "FrameNumber: " + e.ImageFrame.FrameNumber.ToString();
                textBlock5.Text = "Ball Frames: " + ballframes.ToString();
                //textBox1.Text += "FrameNumber: " + e.ImageFrame.FrameNumber.ToString() + " X: " + bx.ToString() + " Y: " + by.ToString() + " Z: " + bz.ToString() + " T: " + frameSpan.ToString() + '\n';
                lastTime = cur;
                Microsoft.Research.Kinect.Nui.Vector tempPoint = new Microsoft.Research.Kinect.Nui.Vector();
                tempPoint.X = (float)bx;
                tempPoint.Y = (float)by;
                tempPoint.Z = (float)bz;
                if (listSize == 0) //first in sequence
                {
                    pointList.Add(tempPoint);
                    numList.Add(e.ImageFrame.FrameNumber);
                    timeList.Add(frameSpan);
                    listSize++;
                }
                else //not first
                {
                    if ((e.ImageFrame.FrameNumber != (numList[listSize - 1] + 2)) || ((tempPoint.Z - 0.05f) < pointList[listSize - 1].Z)) //end of sequence
                    {
                        if (listSize > 2)
                        {
                            textBox1.Text += "Last " + listSize.ToString() + " records are forward movement\n";
                            forwardCount++;
                            textBlock7.Text = forwardCount.ToString();

                            Microsoft.Research.Kinect.Nui.Vector sPoint = pointList[listSize - 3];
                            Microsoft.Research.Kinect.Nui.Vector ePoint = pointList[listSize - 1];
                            //do tracking schtuff here
                            xDisplace = ePoint.X - sPoint.X;
                            yDisplace = ePoint.Y - sPoint.Y;
                            zDisplace = ePoint.Z - sPoint.Z;
                            TimeSpan displaceTime = timeList[listSize - 1] + timeList[listSize - 2];
                            xVel = (xDisplace / displaceTime.Milliseconds) * 1000;
                            yVel = (yDisplace / displaceTime.Milliseconds) * 1000;
                            zVel = (zDisplace / displaceTime.Milliseconds) * 1000;

                            virtualBall = true;
                        }
                        pointList.Clear();
                        numList.Clear();
                        timeList.Clear();
                        listSize = 0;
                    }
                    else //still moving forward!
                    {
                        //float[] fltArr = {tempPoint.X,tempPoint.Y,tempPoint.Z};
                        //Byte[] sendBytes = ToByteArray(fltArr);
                        string dataString = (xDisplace + 0.3f).ToString() + " " + (yDisplace - 0.1f).ToString() + " " + zDisplace.ToString();
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(dataString);
                        myClient.Send(sendBytes, sendBytes.Length); //real ball pos sent to server  

                        realBallsSent++;
                        textBlock8.Text = "Real Balls Sent: " + realBallsSent.ToString();
                        pointList.Add(tempPoint);
                        numList.Add(e.ImageFrame.FrameNumber);
                        timeList.Add(frameSpan);
                        listSize++;
                    }
                }
            }
            else //not a ball detected in this frame
            {
                if (listSize == 0) //first in sequence
                {

                }
                else //not first
                {
                    if (listSize > 2)
                    {
                        textBox1.Text += "Last " + listSize.ToString() + " records are forward movement\n";
                        forwardCount++;
                        textBlock7.Text = forwardCount.ToString();

                        Microsoft.Research.Kinect.Nui.Vector sPoint = pointList[listSize - 3];
                        Microsoft.Research.Kinect.Nui.Vector ePoint = pointList[listSize - 1];
                        //do tracking schtuff here
                        xDisplace = ePoint.X - sPoint.X;
                        yDisplace = ePoint.Y - sPoint.Y;
                        zDisplace = ePoint.Z - sPoint.Z;
                        TimeSpan displaceTime = timeList[listSize - 1] + timeList[listSize - 2];
                        xVel = (xDisplace / displaceTime.Milliseconds)*1000;
                        yVel = (yDisplace / displaceTime.Milliseconds)*1000;
                        zVel = (zDisplace / displaceTime.Milliseconds)*1000;
                        virtualBall = true;
                    }
                    pointList.Clear();
                    numList.Clear();
                    timeList.Clear();
                    listSize = 0;
                }
            }
            ++totalFrames;
            //timeList.Add(cur.Subtract(lastTime));
            /*
            if (cur.Subtract(lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = totalFrames - lastFrames;
                lastFrames = totalFrames;
                lastTime = cur;
                textBlock6.Text = "FPS: " + frameDiff.ToString();
            }*/
            //Use Coding4Fun extension method on ImageFrame class for Depth
            image2.Source = BitmapSource.Create(depthData.Width, depthData.Height, 96, 96, PixelFormats.Gray16, null, depthData.Bits, depthData.Width * depthData.BytesPerPixel);
        }

        const int RED_IDX = 2;
        const int GREEN_IDX = 1;
        const int BLUE_IDX = 0;

        void nui_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            //Manually create BitmapSource for Video
            PlanarImage imageData = e.ImageFrame.Image;/*
            byte[] imageData2 = new byte[imageData.Bits.Length];
            int stride = imageData.Width * imageData.BytesPerPixel;
            long x=0;
            long y=0;
            long hitCount=0;
            for(int i32=0;i32<imageData.Bits.Length;i32+=4)
            {
                //int AveRed = (imageData.Bits[i32 + RED_IDX] + imageData.Bits[i32 + RED_IDX + 4] + imageData.Bits[i32 + RED_IDX - 4] + imageData.Bits[i32 + RED_IDX + imageData.Width * imageData.BytesPerPixel] + imageData.Bits[i32 + RED_IDX - imageData.Width * imageData.BytesPerPixel]) / 5;
                //int AveGreen = (imageData.Bits[i32 + GREEN_IDX] + imageData.Bits[i32 + GREEN_IDX + 4] + imageData.Bits[i32 + GREEN_IDX - 4] + imageData.Bits[i32 + GREEN_IDX + imageData.Width * imageData.BytesPerPixel] + imageData.Bits[i32 + GREEN_IDX - imageData.Width * imageData.BytesPerPixel]) / 5;
                //int AveBlue = (imageData.Bits[i32 + BLUE_IDX] + imageData.Bits[i32 + BLUE_IDX + 4] + imageData.Bits[i32 + BLUE_IDX - 4] + imageData.Bits[i32 + BLUE_IDX + imageData.Width * imageData.BytesPerPixel] + imageData.Bits[i32 + BLUE_IDX - imageData.Width * imageData.BytesPerPixel]) / 5;
                
                int BOX_BLUR_SIZE = 0;
                int AveRed = 0;
                int AveGreen = 0;
                int AveBlue = 0;
                int count = 0;
                for (int k = -BOX_BLUR_SIZE; k <= BOX_BLUR_SIZE; k++)
                {
                    for (int l = -BOX_BLUR_SIZE; l <= BOX_BLUR_SIZE; l++)
                    {
                        if (((i32 + k*imageData.BytesPerPixel + l*stride) >= 0) && ((i32 + k*imageData.BytesPerPixel + l*stride) < imageData.Bits.Length))
                        {
                            count++;
                            AveRed += imageData.Bits[i32 + k * imageData.BytesPerPixel + l * stride + RED_IDX];
                            AveGreen += imageData.Bits[i32 + k * imageData.BytesPerPixel + l * stride + GREEN_IDX];
                            AveBlue += imageData.Bits[i32 + k * imageData.BytesPerPixel + l * stride + BLUE_IDX];
                        }
                    }
                }
                AveRed /= count;
                AveGreen /= count;
                AveBlue /= count;

                if (withinTolerance((byte)AveRed,(byte)AveGreen,(byte)AveBlue))
                {
                    hitCount++;
                    x += ((i32 / 4) % (imageData.Width));
                    y += ((i32 / 4) / (imageData.Width));
                    imageData2[i32 + RED_IDX] = (byte)255;
                    imageData2[i32 + GREEN_IDX] = (byte)255;
                    imageData2[i32 + BLUE_IDX] = (byte)255;
                }
                else
                {
                    imageData2[i32 + RED_IDX] = (byte)0;
                    imageData2[i32 + GREEN_IDX] = (byte)0;
                    imageData2[i32 + BLUE_IDX] = (byte)0;
                }
            }
            if (hitCount > 0)
            {
                x /= hitCount;
                y /= hitCount;
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {
                        if ((x + k) * imageData.BytesPerPixel + (y + l) * stride < imageData.Bits.Length)
                        {
                            imageData2[(x + k) * imageData.BytesPerPixel + (y + l) * stride + RED_IDX] = (byte)255;
                            imageData2[(x + k) * imageData.BytesPerPixel + (y + l) * stride + GREEN_IDX] = (byte)0;
                            imageData2[(x + k) * imageData.BytesPerPixel + (y + l) * stride + BLUE_IDX] = (byte)0;
                        }
                    }
                }
            }*/
            //image1.Source = BitmapSource.Create(imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr32, null, imageData2, imageData.Width * imageData.BytesPerPixel);
            image1.Source = BitmapSource.Create(imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr32, null, imageData.Bits, imageData.Width * imageData.BytesPerPixel);
            
            //section for opencv computations
            
            //Image<Bgr, Byte> currentFrame = new Image<Bgr, Byte>(imageData.Width, imageData.Height, imageData.BytesPerPixel * imageData.Width, imageData.Bits);
            //Image<Bgr, Byte> cvImg = new Image<Bgr, byte>(imageData.Width, imageData.Height);
            //Image<Bgr, Byte> myimage = new Image<Bgr, byte>(400, 100, new Bgr(255, 255, 255));
            //cvImg.Bytes = imageData.Bits;

        }

        private bool withinTolerance(byte red, byte green, byte blue)
        {
            //abs threshold
            if (red < (byte)15 || red > (byte)80)
                return false;
            if (green < (byte)30 || green > (byte)140)
                return false;
            if (blue < (byte)0 || blue > (byte)90)
                return false;
            //rel threshold
            if ((red - blue) < -15)
                return false;
            if ((float)red / (float)green > 0.6f)
                return false;
            return true;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize(); 
        }

        private byte[] ToByteArray(float[] floatArray) 
        { int len = floatArray.Length * 4;
            byte[] byteArray = new byte[len];
            int pos = 0;
            foreach (float f in floatArray) 
            {
                byte[] data = BitConverter.GetBytes(f);
                Array.Copy(data, 0, byteArray, pos, 4);
                pos += 4;
            }
            return byteArray; 
        }
    }
}
