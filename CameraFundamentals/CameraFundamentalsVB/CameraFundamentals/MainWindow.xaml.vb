'///////////////////////////////////////////////////////////////////////
'
' This module provides sample code used to demonstrate the use
' of the KinectAudioSource for audio capture and beam tracking
'
' Copyright © Microsoft Corporation.  All rights reserved.  
' This code is licensed under the terms of the 
' Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
' License Agreement: http://research.microsoft.com/KinectSDK-ToU
'
'///////////////////////////////////////////////////////////////////////

Imports System.Text
Imports Microsoft.Research.Kinect.Nui
Imports Coding4Fun.Kinect.Wpf

Namespace CameraFundamentals

	''' <summary>
	''' Interaction logic for MainWindow.xaml
	''' </summary>
	Partial Public Class MainWindow
		Inherits Window

		Public Sub New()

			InitializeComponent()

		End Sub

		'Kinect Runtime
        Private nui As New Runtime

		Private Sub Window_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)

			'setup event handlers
			AddHandler nui.VideoFrameReady, AddressOf nui_VideoFrameReady
			AddHandler nui.DepthFrameReady, AddressOf nui_DepthFrameReady

			'Initialize to return both Color & Depth images
			nui.Initialize(RuntimeOptions.UseColor Or RuntimeOptions.UseDepth)

			'open both streams
			nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color)
			nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.Depth)

		End Sub

		Private Sub nui_DepthFrameReady(ByVal sender As Object, ByVal e As ImageFrameReadyEventArgs)

			'Use Coding4Fun extension method on ImageFrame class for Depth
			image2.Source = e.ImageFrame.ToBitmapSource()

		End Sub

		Private Sub nui_VideoFrameReady(ByVal sender As Object, ByVal e As ImageFrameReadyEventArgs)

			'Manually create BitmapSource for Video
			Dim imageData As PlanarImage = e.ImageFrame.Image
			image1.Source = BitmapSource.Create(imageData.Width, imageData.Height, 96, 96, PixelFormats.Bgr32, Nothing, imageData.Bits, imageData.Width * imageData.BytesPerPixel)


		End Sub

		Private Sub Window_Closed(ByVal sender As Object, ByVal e As EventArgs)

			nui.Uninitialize()

		End Sub

	End Class

End Namespace
