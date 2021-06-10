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
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GenericBLESensor
{
    // This scenario connects to the device selected in the "Discover
    // GATT Servers" scenario and communicates with it.
    // Note that this scenario is rather artificial because it communicates
    // with an unknown service with unknown characteristics.
    // In practice, your app will be interested in a specific service with
    // a specific characteristic.
    public sealed partial class Scenario4_DataVisualization : Page 
    {
        private CSVHelper CSVHelperObj;
        private MainPage rootPage = MainPage.Current;

        //private BluetoothLEDevice bluetoothLeDevice = null;
        private BluetoothLEDevice bluetoothLeDevice1 = null;
        private BluetoothLEDevice bluetoothLeDevice2 = null;
        private BluetoothLEDevice bluetoothLeDevice3 = null;
        private BluetoothLEDevice bluetoothLeDevice4 = null;
        private BluetoothLEDevice bluetoothLeDevice;
        //private GattCharacteristic selectedCharacteristic;

        GattDeviceService flowService;
        GattCharacteristic flowCharacteristic;

        //GattDeviceService leftFootService;
        //GattCharacteristic leftFootCharacteristic;

        private DeviceWatcher deviceWatcher;

        // Only one registered characteristic at a time.
        //private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;

        List<Int32> calibrationData = new List<Int32>();
        List<Int32> flowDatatoShow = new List<Int32>();
        private Int32 flowDatatoShowMaxNumber = 15;
        private Int32 calibrationVoltage = 0;
        private Int32 calibrationCounter = 0;
        private Int32 calibrationMaxNumber = 100;
        private bool isCalibrated = false;

        Int16[] ValuesToShow;

        private bool subscribedForNotifications = false;
        private bool subscribedForCalibration = false;
        private bool timerEnabled = false;
        //AutoResetEvent autoEvent = new AutoResetEvent(false);
        Timer flowValueTimer;

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        #region UI Code
        public Scenario4_DataVisualization()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //SelectedDeviceRun.Text = rootPage.SelectedBleDeviceName;
            SelectedDeviceRun.Text = "Disconnected";

            if (string.IsNullOrEmpty(rootPage.SelectedBleDeviceId))
            {
                ConnectButton.IsEnabled = true;
            }

            ValuesToShow = new Int16[6] { 0, 0, 0, 0, 0, 0};

        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            var success = await ClearBluetoothLEDeviceAsync();
            if (!success)
            {
                rootPage.NotifyUser("Error: Unable to reset app state", NotifyType.ErrorMessage);
            }
        }
        #endregion

        #region Enumerating Services
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications

                GattCommunicationStatus result = GattCommunicationStatus.Unreachable;
                result = await flowCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    flowCharacteristic.ValueChanged -= FlowCharacteristic_ValueChanged;
                    //subscribedForNotifications = false;
                }

                if (result != GattCommunicationStatus.Success)
                {
                    subscribedForNotifications = false;
                }
                else
                {
                    rootPage.NotifyUser("Error with cleaning.", NotifyType.ErrorMessage);
                    //throw new Exception("Error with cleaning.");
                }


            }
            //bluetoothLeDevice?.Dispose();
            //bluetoothLeDevice = null;

            bluetoothLeDevice1?.Dispose();
            bluetoothLeDevice2?.Dispose();
            bluetoothLeDevice3?.Dispose();
            bluetoothLeDevice4?.Dispose();
            bluetoothLeDevice?.Dispose();

            bluetoothLeDevice1 = null;
            bluetoothLeDevice2 = null;
            bluetoothLeDevice3 = null;
            bluetoothLeDevice4 = null;
            return true;
        }

        private async void ConnectButton_Click()
        {
            ConnectButton.IsEnabled = false;

            if (!await ClearBluetoothLEDeviceAsync())
            {
                rootPage.NotifyUser("Error: Unable to reset state, try again.", NotifyType.ErrorMessage);
                ConnectButton.IsEnabled = true;
                return;
            }
            StartBleDeviceWatcher();

            await Task.Delay(TimeSpan.FromSeconds(1));

            try
            {
                // BT_Code: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                //bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(rootPage.SelectedBleDeviceId);

                // NRF52DK
                // e7:a6:c8:a0:67:41

                // Feather COM5
                // ec:f2:a6:8f:51:d7

                // Feather COM6
                // c1:81:6b:98:16:1f

                // Feather 
                // 0x0000dea8d0bd1a47
                //

                //bluetoothLeDevice1 = await BluetoothLEDevice.FromBluetoothAddressAsync(0xe7a6c8a06741);
                //bluetoothLeDevice2 = await BluetoothLEDevice.FromBluetoothAddressAsync(0xecf2a68f51d7);
                //bluetoothLeDevice3 = await BluetoothLEDevice.FromBluetoothAddressAsync(0xc1816b98161f);
                bluetoothLeDevice4 = await BluetoothLEDevice.FromBluetoothAddressAsync(0xdea8d0bd1a47);


                if (bluetoothLeDevice1 != null)
                {
                    if (bluetoothLeDevice1.Name == "Flow Sensor")
                    {
                        bluetoothLeDevice = bluetoothLeDevice1;
                    }
                }
                if (bluetoothLeDevice2 != null)
                {
                    if (bluetoothLeDevice2.Name == "Flow Sensor")
                    {
                        bluetoothLeDevice = bluetoothLeDevice2;
                    }
                }

                if (bluetoothLeDevice3 != null)
                {
                    if (bluetoothLeDevice3.Name == "Flow Sensor")
                    {
                        bluetoothLeDevice = bluetoothLeDevice3;
                    }
                }
                if (bluetoothLeDevice4 != null)
                {
                    if (bluetoothLeDevice4.Name == "Flow Sensor")
                    {
                        bluetoothLeDevice = bluetoothLeDevice4;
                    }
                }

                if ( bluetoothLeDevice == null )
                {
                    rootPage.NotifyUser("Failed to connect to sensor.", NotifyType.ErrorMessage);
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                rootPage.NotifyUser("Bluetooth radio is not on.", NotifyType.ErrorMessage);
            }

            StopBleDeviceWatcher();

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    //rootPage.NotifyUser("Successfully connected", NotifyType.StatusMessage);
                    foreach (var service in services)
                    {
                        ServiceList.Items.Add(new ComboBoxItem { Content = DisplayHelpers.GetServiceName(service), Tag = service });
                        if (service.Uuid == Constants.flowSensorServiceUuid)
                        {
                            flowService = service;
                            break;
                        }
                    }

                    // From characteristics listing:
                    IReadOnlyList<GattCharacteristic> characteristics = null;
                    var accessStatus = await flowService.RequestAccessAsync();
                    if (accessStatus == DeviceAccessStatus.Allowed)
                    {
                        // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                        // and the new Async functions to get the characteristics of unpaired devices as well. 
                        var result2 = await flowService.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                        if (result2.Status == GattCommunicationStatus.Success)
                        {
                            characteristics = result2.Characteristics;
                            foreach (GattCharacteristic c in characteristics)
                            {
                                CharacteristicList.Items.Add(new ComboBoxItem { Content = DisplayHelpers.GetCharacteristicName(c), Tag = c });
                                if (c.Uuid == Constants.flowSensorCharacteristicUuid)
                                {
                                    flowCharacteristic = c;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            rootPage.NotifyUser("Error accessing service.", NotifyType.ErrorMessage);
                        }
                    }
                }
                else
                {
                    rootPage.NotifyUser("Device unreachable", NotifyType.ErrorMessage);
                }
            }

           
            if (flowCharacteristic != null)
            {
                ValueChangedSubscribeToggle.IsEnabled = true;
                ValueChangedSubscribeToggle.Visibility = Visibility.Visible;
                ValueChangedSubscribeToggle.Content = "Start";
                rootPage.NotifyUser("Connected to sensor.", NotifyType.StatusMessage);
                SelectedDeviceRun.Text = "Connected";
                Calibration.IsEnabled = true;
                Calibration.Visibility = Visibility.Visible;
            }
            ConnectButton.IsEnabled = true;
        }
        #endregion

        #region Calibration
        private async void FlowCharacteristic_Calibration(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            Debug.WriteLine(String.Format("Received {0}  {1}  {2}", args.Timestamp, sender, args.CharacteristicValue));
            byte[] data;
            Int16 newValue;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            ValuesToShow[0] = newValue = BitConverter.ToInt16(data, 0);
            calibrationData.Add(newValue);

            string strValues = ValuesToShow[0].ToString();

            //_ = await Task.Run(() => _ = CSVHelperObj.SaveData(newValue, args.Timestamp));
            //var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            var message = $"Value: {newValue}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CharacteristicLatestValue.Text = message);
            calibrationCounter++;
            if (calibrationCounter > calibrationMaxNumber)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Calibration_Click());
            }
        }

        private async void Calibration_Click()
        {
            Calibration.IsEnabled = false;
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            if (!subscribedForCalibration) {
                try {
                    status = await flowCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (status == GattCommunicationStatus.Success) {
                        flowCharacteristic.ValueChanged += FlowCharacteristic_Calibration;
                    }
                    else {
                        rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                    }
                    if (status == GattCommunicationStatus.Success)   {
                        calibrationData = new List<Int32>();
                        subscribedForCalibration = true;
                        rootPage.NotifyUser("Calibration started", NotifyType.StatusMessage);
                    }
                    else {
                        rootPage.NotifyUser("Error with registration.", NotifyType.StatusMessage);
                    }
                }
                catch (UnauthorizedAccessException ex) {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            else {
                try {
                    status = await flowCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (status == GattCommunicationStatus.Success) {
                        flowCharacteristic.ValueChanged -= FlowCharacteristic_Calibration;
                    }
                    else {
                        rootPage.NotifyUser($"Error: {status}", NotifyType.ErrorMessage);
                    }
                    if (status == GattCommunicationStatus.Success)  {
                        subscribedForCalibration = false;
                        calibrationCounter = 0;
                        double sum = 0;
                        for (int i = 0; i < calibrationData.Count; i++)
                        {
                            sum += calibrationData[i];
                        }
                        calibrationVoltage = (Int32) (sum / (float)calibrationData.Count);
                        Calibration.IsEnabled = true;
                        rootPage.NotifyUser($"Calibration value: {calibrationVoltage} μV", NotifyType.StatusMessage);
                        isCalibrated = true;
                    }
                    else {
                        rootPage.NotifyUser("Error with deregistration.", NotifyType.ErrorMessage);
                    }
                }
                catch (UnauthorizedAccessException ex) {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            
        }
        #endregion

        #region Enumerating Characteristics
        private async void ServiceList_SelectionChanged()
        {
            var service = (GattDeviceService)((ComboBoxItem)ServiceList.SelectedItem)?.Tag;

            CharacteristicList.Items.Clear();
            //RemoveValueChangedHandler();

            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        rootPage.NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                        // On error, act as if there are no characteristics.
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    rootPage.NotifyUser("Error accessing service.", NotifyType.ErrorMessage);

                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();

                }
            }
            catch (Exception ex)
            {
                rootPage.NotifyUser("Restricted service. Can't read characteristics: " + ex.Message,
                    NotifyType.ErrorMessage);
                // On error, act as if there are no characteristics.
                characteristics = new List<GattCharacteristic>();
            }

            foreach (GattCharacteristic c in characteristics)
            {
                CharacteristicList.Items.Add(new ComboBoxItem { Content = DisplayHelpers.GetCharacteristicName(c), Tag = c });
            }
            CharacteristicList.Visibility = Visibility.Visible;
        }
        #endregion

        private async void CharacteristicList_SelectionChanged()
        {
            //    selectedCharacteristic = (GattCharacteristic)((ComboBoxItem)CharacteristicList.SelectedItem)?.Tag;

            //    var zzz = ((ComboBoxItem)CharacteristicList.SelectedItem);
            //    var zzz2tag = zzz.Tag;

            //    if (selectedCharacteristic == null)
            //    {
            //        EnableCharacteristicPanels(GattCharacteristicProperties.None);
            //        rootPage.NotifyUser("No characteristic selected", NotifyType.ErrorMessage);
            //        return;
            //    }

            //    // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            //    // and the new Async functions to get the descriptors of unpaired devices as well. 
            //    var result = await selectedCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            //    if (result.Status != GattCommunicationStatus.Success)
            //    {
            //        rootPage.NotifyUser("Descriptor read failure: " + result.Status.ToString(), NotifyType.ErrorMessage);
            //    }

            //    // BT_Code: There's no need to access presentation format unless there's at least one. 
            //    presentationFormat = null;
            //    if (selectedCharacteristic.PresentationFormats.Count > 0)
            //    {

            //        if (selectedCharacteristic.PresentationFormats.Count.Equals(1))
            //        {
            //            // Get the presentation format since there's only one way of presenting it
            //            presentationFormat = selectedCharacteristic.PresentationFormats[0];
            //        }
            //        else
            //        {
            //            // It's difficult to figure out how to split up a characteristic and encode its different parts properly.
            //            // In this case, we'll just encode the whole thing to a string to make it easy to print out.
            //        }
            //    }

            //    // Enable/disable operations based on the GattCharacteristicProperties.
            //    EnableCharacteristicPanels(selectedCharacteristic.CharacteristicProperties);
        }

        private void SetVisibility(UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        
        private async void CharacteristicReadButton_Click()
        {
            //    // BT_Code: Read the actual value from the device by using Uncached.
            //    GattReadResult result = await selectedCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            //    if (result.Status == GattCommunicationStatus.Success)
            //    {
            //        string formattedResult = FormatValueByPresentation(result.Value, presentationFormat);
            //        rootPage.NotifyUser($"Read result: {formattedResult}", NotifyType.StatusMessage);
            //    }
            //    else
            //    {
            //        rootPage.NotifyUser($"Read failed: {result.Status}", NotifyType.ErrorMessage);
            //    }
        }

        private async void CharacteristicWriteButton_Click()
        {
            //    if (!String.IsNullOrEmpty(CharacteristicWriteValue.Text))
            //    {
            //        var writeBuffer = CryptographicBuffer.ConvertStringToBinary(CharacteristicWriteValue.Text,
            //            BinaryStringEncoding.Utf8);

            //        var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writeBuffer);
            //    }
            //    else
            //    {
            //        rootPage.NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            //    }
        }

        private async void CharacteristicWriteButtonInt_Click()
        {
            //    if (!String.IsNullOrEmpty(CharacteristicWriteValue.Text))
            //    {
            //        var isValidValue = Int32.TryParse(CharacteristicWriteValue.Text, out int readValue);
            //        if (isValidValue)
            //        {
            //            var writer = new DataWriter();
            //            writer.ByteOrder = ByteOrder.LittleEndian;
            //            writer.WriteInt32(readValue);
            //            var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writer.DetachBuffer());
            //        }
            //        else
            //        {
            //            rootPage.NotifyUser("Data to write has to be an int32", NotifyType.ErrorMessage);
            //        }
            //    }
            //    else
            //    {
            //        rootPage.NotifyUser("No data to write to device", NotifyType.ErrorMessage);
            //    }
        }

        private async void UpdateFlowValue(object state)
        {
            if (flowDatatoShow.Count < 10) {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FlowValue.Text = "0");
            }
            else {
                Int32 tempValue;
                double sum = 0;
                for (int i = 0; i < flowDatatoShow.Count; i++) {
                    sum += flowDatatoShow[i];
                }
                tempValue = (Int32)(sum / (float)flowDatatoShow.Count);
                tempValue -= calibrationVoltage;



                // Calibration curve untill 100mL/h
                //tempValue = (int)((float)(Math.Abs(tempValue) - 854) / 12.992);
                tempValue = (int)( 2.3991*Math.Exp(0.0018*Math.Abs((float)tempValue)) );

                //if (tempValue < 10)
                //{
                //    tempValue = 10;
                //}
                //if (tempValue > 100)
                //{
                //    tempValue = 100;
                //}



                if (flowDatatoShow.Count > 20)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FlowValue. Text = "0000");
                }
                else
                {
                    string strValue = tempValue.ToString();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => FlowValue.Text = strValue);
                }

            }
        }

        private async void ValueChangedSubscribeToggle_Click()
        {
            ValueChangedSubscribeToggle.IsEnabled = false;
            //if (!(timerEnabled))
            //{
            //    stateTimer = new Timer(UpdateFlowValue, null, 0, 1000);
            //    ValueChangedSubscribeToggle.Content = "Stop";
            //    timerEnabled = true;
            //    CSVHelperObj = new CSVHelper(JustRightFoot);
            //}
            //else
            //{
            //    stateTimer.Dispose();
            //    ValueChangedSubscribeToggle.Content = "Start";
            //    timerEnabled = false;
            //    await CSVHelperObj.SaveTempCSVAsync();
            //}
            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            if (!subscribedForNotifications)
            {
                CSVHelperObj = new CSVHelper();
                // initialize status 
                //var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                ////selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.)
                //if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                //{
                //    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                //}
                //else if (selectedCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                //{
                //    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                //}
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    //// We receive them in the ValueChanged event handler.
                    //if (!(JustRightFoot))
                    //{
                    //    statusLeft = await leftFootCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    //    //bluetoothLeDeviceLeft.ConnectionStatusChanged
                    //    if (statusLeft == GattCommunicationStatus.Success)
                    //    {
                    //        //rootPage.NotifyUser("Receiving data from sensor", NotifyType.StatusMessage);
                    //        //_ = await Task.Run(() => _ = CSVHelperObj.CreateCSVFileAsync());
                    //        leftFootCharacteristic.ValueChanged += LeftCharacteristic_ValueChanged;
                    //        //ValueChangedSubscribeToggle.Content = "Stop";
                    //        //subscribedForNotifications = true;
                    //    }
                    //    else
                    //    {
                    //        rootPage.NotifyUser($"Error registering for value changes: {statusLeft}", NotifyType.ErrorMessage);
                    //    }
                    //}
                    status = await flowCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (status == GattCommunicationStatus.Success)
                    {
                        //rootPage.NotifyUser("Receiving data from sensor", NotifyType.StatusMessage);
                        //_ = await Task.Run(() => _ = CSVHelperObj.CreateCSVFileAsync());
                        flowCharacteristic.ValueChanged += FlowCharacteristic_ValueChanged;
                        //ValueChangedSubscribeToggle.Content = "Stop";
                        //subscribedForNotifications = true;
                    }
                    else
                    {
                        rootPage.NotifyUser($"Error registering for value changes: {status}", NotifyType.ErrorMessage);
                    }
                    if (status == GattCommunicationStatus.Success)
                    {
                        ValueChangedSubscribeToggle.Content = "Stop";
                        subscribedForNotifications = true;
                        rootPage.NotifyUser("Receiving data from sensor", NotifyType.StatusMessage);
                        flowValueTimer = new Timer(UpdateFlowValue, null, 0, 1000);
                    }
                    else
                    {
                        rootPage.NotifyUser("Error with registration.", NotifyType.StatusMessage);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    
                    status = await flowCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (status == GattCommunicationStatus.Success)
                    {
                        flowCharacteristic.ValueChanged -= FlowCharacteristic_ValueChanged;
                        //rightFootCharacteristic.ValueChanged -= RightCharacteristic_ValueChanged;
                        //ValueChangedSubscribeToggle.Content = "Start";
                        //subscribedForNotifications = false;
                        //await CSVHelperObj.SaveTempCSVAsync();
                        //rootPage.NotifyUser("Successfully saved file", NotifyType.StatusMessage);
                    }
                    else
                    {
                        rootPage.NotifyUser($"Error: {status}", NotifyType.ErrorMessage);
                    }
                    
                    
                    if (status == GattCommunicationStatus.Success)
                    {
                        ValueChangedSubscribeToggle.Content = "Start";
                        subscribedForNotifications = false;
                        await CSVHelperObj.SaveTempCSVAsync();
                        flowValueTimer.Dispose();
                        //rootPage.NotifyUser("Successfully saved file", NotifyType.StatusMessage);
                    }
                    else
                    {
                        rootPage.NotifyUser("Error with deregistration.", NotifyType.ErrorMessage);
                        //throw new Exception("");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    rootPage.NotifyUser(ex.Message, NotifyType.ErrorMessage);
                }
            }
            ValueChangedSubscribeToggle.IsEnabled = true;
        }

        private async void FlowCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            Debug.WriteLine(String.Format("Received {0}  {1}  {2}", args.Timestamp, sender, args.CharacteristicValue));
            byte[] data;
            Int16 newValue;
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out data);
            ValuesToShow[0] = newValue = BitConverter.ToInt16(data, 0);

            flowDatatoShow.Add(newValue);
            if (flowDatatoShow.Count > flowDatatoShowMaxNumber)
            {
                flowDatatoShow.RemoveAt(0);
            }

            string strValues = ValuesToShow[0].ToString();

            _ = await Task.Run(() => _ = CSVHelperObj.SaveData(newValue, args.Timestamp));
            //var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            var message = $"Value: {newValue}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = message);
        }

        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            //if (format != null)
            //{
            //    if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
            //    {
            //        return BitConverter.ToInt32(data, 0).ToString();
            //    }
            //    else if (format.FormatType == GattPresentationFormatTypes.Utf8)
            //    {
            //        try
            //        {
            //            return Encoding.UTF8.GetString(data);
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "(error: Invalid UTF-8 string)";
            //        }
            //    }
            //    else
            //    {
            //        // Add support for other format types as needed.
            //        return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
            //    }
            //}
            //else if (data != null)
            //{

            //    // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
            //    if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.HeartRateMeasurement))
            //    {
            //        try
            //        {
            //            return "Heart Rate: " + ParseHeartRateValue(data).ToString();
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Heart Rate: (unable to parse)";
            //        }
            //    }
            //    else if (selectedCharacteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
            //    {
            //        try
            //        {
            //            // battery level is encoded as a percentage value in the first byte according to
            //            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
            //            return "Battery Level: " + data[0].ToString() + "%";
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Battery Level: (unable to parse)";
            //        }
            //    }

            //    // This is the custom service Right of Left Foot Sensor UUID. Format it like an Int16
            //    else if (selectedCharacteristic.Uuid.Equals(Constants.RightFootSensorCharacteristicUuid) || selectedCharacteristic.Uuid.Equals(Constants.LeftFootSensorCharacteristicUuid))
            //    {
            //        try
            //        {
            //            return BitConverter.ToInt16(data, 0).ToString() + ", " +
            //                   BitConverter.ToInt16(data, 2).ToString() + ", " +
            //                   BitConverter.ToInt16(data, 4).ToString();
            //        }
            //        catch (ArgumentException)
            //        {
            //            return BitConverter.ToInt16(data, 0).ToString();
            //        }
            //    }
            //    // This is the custom calc service Result UUID. Format it like an Int
            //    else if (selectedCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
            //    {
            //        return BitConverter.ToInt32(data, 0).ToString();
            //    }
            //    // No guarantees on if a characteristic is registered for notifications.
            //    //else if (registeredCharacteristic != null)
            //    //{
            //    //    // This is our custom calc service Result UUID. Format it like an Int
            //    //    if (registeredCharacteristic.Uuid.Equals(Constants.ResultCharacteristicUuid))
            //    //    {
            //    //        return BitConverter.ToInt32(data, 0).ToString();
            //    //    }
            //    //}
            //    else
            //    {
            //        try
            //        {
            //            return "Unknown format: " + Encoding.UTF8.GetString(data);
            //        }
            //        catch (ArgumentException)
            //        {
            //            return "Unknown format";
            //        }
            //    }
            //}
            try
            {
                return BitConverter.ToInt16(data, 0).ToString() + ", " +
                       BitConverter.ToInt16(data, 2).ToString() + ", " +
                       BitConverter.ToInt16(data, 4).ToString();
            }
            catch (ArgumentException)
            {
                return BitConverter.ToInt16(data, 0).ToString();
            }
            //else
            //{
            //    return "Empty data received";
            //}
            //return "Unknown format";
        }

        /// <summary>
        /// Process the raw data received from the device into application usable data,
        /// according the the Bluetooth Heart Rate Profile.
        /// https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.heart_rate_measurement.xml&u=org.bluetooth.characteristic.heart_rate_measurement.xml
        /// This function throws an exception if the data cannot be parsed.
        /// </summary>
        /// <param name="data">Raw data received from the heart rate monitor.</param>
        /// <returns>The heart rate measurement value.</returns>
        private static ushort ParseHeartRateValue(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte heartRateValueFormat = 0x01;

            byte flags = data[0];
            bool isHeartRateValueSizeLong = ((flags & heartRateValueFormat) != 0);

            if (isHeartRateValueSizeLong)
            {
                return BitConverter.ToUInt16(data, 1);
            }
            else
            {
                return data[1];
            }
        }

        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            //deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            //deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            //KnownDevices.Clear();

            // Start the watcher. Active enumeration is limited to approximately 30 seconds.
            // This limits power usage and reduces interference with other Bluetooth activities.
            // To monitor for the presence of Bluetooth LE devices for an extended period,
            // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
            // sample for an example.
            deviceWatcher.Start();
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure device isn't already present in the list.
                        //if (FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                        //{

                        //    if (deviceInfo.Name != string.Empty)
                        //    {
                        //        // If device has a friendly name display it immediately.
                        //        KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                        //    }
                        //    else
                        //    {
                        //        // Add it to a list in case the name gets updated later. 
                        //        UnknownDevices.Add(deviceInfo);
                        //    }
                        //}

                    }
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        //BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        //if (bleDeviceDisplay != null)
                        //{
                        //    // Device is already being displayed - update UX.
                        //    bleDeviceDisplay.Update(deviceInfoUpdate);
                        //    return;
                        //}

                        //DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        //if (deviceInfo != null)
                        //{
                        //    deviceInfo.Update(deviceInfoUpdate);
                        //    // If device has been updated with a friendly name it's no longer unknown.
                        //    if (deviceInfo.Name != String.Empty)
                        //    {
                        //        KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                        //        UnknownDevices.Remove(deviceInfo);
                        //    }
                        //}
                    }
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Find the corresponding DeviceInformation in the collection and remove it.
                        //BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        //if (bleDeviceDisplay != null)
                        //{
                        //    KnownDevices.Remove(bleDeviceDisplay);
                        //}

                        //DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        //if (deviceInfo != null)
                        //{
                        //    UnknownDevices.Remove(deviceInfo);
                        //}
                    }
                }
            });
        }

        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                //deviceWatcher.Added -= DeviceWatcher_Added;
                //deviceWatcher.Updated -= DeviceWatcher_Updated;
                //deviceWatcher.Removed -= DeviceWatcher_Removed;
                //deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                //deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }
    }
}
