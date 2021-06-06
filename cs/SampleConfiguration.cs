//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace GenericBLESensor
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Smart Sensors";

        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title="Generic: Discover ", ClassType=typeof(Scenario1_Discovery) },
            new Scenario() { Title="Generic: Connect ", ClassType=typeof(Scenario2_Client) },
            //new Scenario() { Title="Server: Publish foreground", ClassType=typeof(Scenario3_ServerForeground) },
            new Scenario() { Title="Flow Sensors", ClassType=typeof(Scenario4_DataVisualization) },
        };

        public string SelectedBleDeviceId;
        public string SelectedBleDeviceName = "No device selected";
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
