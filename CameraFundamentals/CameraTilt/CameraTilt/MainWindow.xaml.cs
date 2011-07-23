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
using Microsoft.Research.Kinect.Nui; 

namespace CameraTilt
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Set angle to slider1 value
            nui.NuiCamera.ElevationAngle = (int)slider1.Value; 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Need to initialize before adjusting angle
            nui.Initialize(RuntimeOptions.UseColor); 
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //cleanup
            nui.Uninitialize(); 
        }
    }
}
