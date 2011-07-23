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

Namespace CameraTilt

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

		Private Sub button1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

			'Set angle to slider1 value
			nui.NuiCamera.ElevationAngle = CInt(Fix(slider1.Value))

		End Sub

		Private Sub Window_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)

			'Need to initialize before adjusting angle
			nui.Initialize(RuntimeOptions.UseColor)

		End Sub

		Private Sub Window_Closed(ByVal sender As Object, ByVal e As EventArgs)

			'cleanup
			nui.Uninitialize()

		End Sub

	End Class

End Namespace
