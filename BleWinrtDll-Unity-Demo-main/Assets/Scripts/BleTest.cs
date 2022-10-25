using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class BleTest : MonoBehaviour
{
    // Change this to match your device.
    string targetDeviceName = "ProxIMU";
    string serviceUuid = "{ABF0E000-B597-4BE0-B869-6054B7ED0CE3}";
    string[] characteristicUuids = {      
         "{ABF0E002-B597-4BE0-B869-6054B7ED0CE3}"
    };

    BLE ble;
    BLE.BLEScan scan;
    bool isScanning = false, isConnected = false;
    string deviceId = null;  
    IDictionary<string, string> discoveredDevices = new Dictionary<string, string>();
    int devicesCount = 0;

    // BLE Threads 
    Thread scanningThread, connectionThread, readingThread;

    // GUI elements
    public Text TextDiscoveredDevices, TextIsScanning, TextTargetDeviceConnection, TextTargetDeviceData1, TextTargetDeviceData2, TextTargetDeviceData3, TextTargetDeviceData4, TextTargetDeviceData5, TextTargetDeviceData6, TextTargetDeviceData7, 
        InputText, TextTargetDeviceData8, TextTargetDeviceData9, TextTargetDeviceData10;
    public Button ButtonEstablishConnection, ButtonStartScan;
    float acx, lastAcx, acy, lastAcy, acz, lastAcz, gyrox, lastGyrox, gyroy, lastGyroy, gyroz, lastGyroz, pres1, lastPres1, pres2, lastPres2, pres3, lastPres3; //damhi
    string datos, LastDato, TextInput;//damhir
    public string input;
    public InputField FileEnter;
    // Start is called before the first frame update
    void Start()
    {
        ble = new BLE();
        ButtonEstablishConnection.enabled = false;
        TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        readingThread = new Thread(ReadBleData);
        
    }

    // Update is called once per frame
    void Update()
    {  
        if (isScanning)
        {
            if (ButtonStartScan.enabled)
                ButtonStartScan.enabled = false;

            if (discoveredDevices.Count > devicesCount)
            {
                UpdateGuiText("scan");
                devicesCount = discoveredDevices.Count;
            }                
        } else
        {
            /* Restart scan in same play session not supported yet.
            if (!ButtonStartScan.enabled)
                ButtonStartScan.enabled = true;
            */

            if (TextIsScanning.text != "Not scanning.")
            {
                TextIsScanning.color = Color.white;
                TextIsScanning.text = "Not scanning.";
            }
        }

        // The target device was found.
        if (deviceId != null && deviceId != "-1")
        {
            // Target device is connected and GUI knows.
            if (ble.isConnected && isConnected)
            {
                UpdateGuiText("writeData");
                
            }
            // Target device is connected, but GUI hasn't updated yet.
            else if (ble.isConnected && !isConnected)
            {
                UpdateGuiText("connected");
                isConnected = true;
            // Device was found, but not connected yet. 
            } else if (!ButtonEstablishConnection.enabled && !isConnected)
            {
                ButtonEstablishConnection.enabled = true;
                TextTargetDeviceConnection.text = "Found target device:\n" + targetDeviceName;
            } 
        } 
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    // Prevent threading issues and free BLE stack.
    // Can cause Unity to freeze and lead
    // to errors when omitted.
    private void CleanUp()
    {
        try
        {
            scan.Cancel();
            ble.Close();
            scanningThread.Abort();
            connectionThread.Abort();
        } catch(NullReferenceException e)
        {
            Debug.Log("Thread or object never initialized.\n" + e);
        }        
    }

    public void StartScanHandler()
    {
        devicesCount = 0;
        isScanning = true;
        discoveredDevices.Clear();
        scanningThread = new Thread(ScanBleDevices);
        scanningThread.Start();
        TextIsScanning.color = new Color(244, 180, 26);
        TextIsScanning.text = "Scanning...";
        TextDiscoveredDevices.text = "";
    }

    public void ResetHandler()
    {
        TextTargetDeviceData1.text = "";
        TextTargetDeviceData2.text = "";
        TextTargetDeviceData3.text = "";
        TextTargetDeviceData4.text = "";
        TextTargetDeviceData5.text = "";
        TextTargetDeviceData6.text = "";
        TextTargetDeviceConnection.text = targetDeviceName + " not found.";
        // Reset previous discovered devices
        discoveredDevices.Clear();
        TextDiscoveredDevices.text = "No devices.";
        deviceId = null;
        CleanUp();
    }

    private void ReadBleData(object obj)
    {
        byte[] packageReceived;
        packageReceived = BLE.ReadBytes();
        datos = Encoding.UTF8.GetString(packageReceived);


    }

    private static void addRecord(string ACX, string ACY, string ACZ, string GX, string GY, string GZ, string PRESS1, string PRESS2, string PRESS3, string DATE, string filepath)
    {

        try
        {
            var firstWrite = !File.Exists(filepath);
            
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                if (firstWrite)
                {
                    file.Write("ACX" + "," + "ACY" + "," + "ACZ" + "," + "GX" + "," + "GY" + "," + "GZ" + "," + "PRESS1" + "," + "PRESS2" + "," + "PRESS3" + "," + "DATE");
                }
                else
                {
                    file.WriteLine();
                    file.Write(ACX + "," + ACY + "," + ACZ + "," + GX + "," + GY + "," + GZ + "," + PRESS1 + "," + PRESS2 + "," + PRESS3 + "," + DATE);
                }

            }

        }
        catch(Exception ex)
        {
            throw new ApplicationException("error Saving:", ex);
        }
    }

    public void ReadInput(string s)
    {
        input = s;
        
    }


    // If the system architecture is little-endian (that is, little end first),
    // reverse the byte array.
    //acx = packageReceived;
    // Output: int: 25
    // Convert little Endian.
    // In this example we're interested about an angle
    // value on the first field of our package.
    //acx =  packageReceived[0];

    //acz = packageReceived[2];
    //gyrox = packageReceived[3];
    //gyroy = packageReceived[4];
    //gyroz = packageReceived[5];
    //pres1 = packageReceived[6];
    //pres2 = packageReceived[7];
    //pres3 = packageReceived[8];
    //Debug.Log("Angle: " + remoteAngle);



    void UpdateGuiText(string action)
    {
        switch(action) {
            case "scan":
                TextDiscoveredDevices.text = "";
                foreach (KeyValuePair<string, string> entry in discoveredDevices)
                {
                    TextDiscoveredDevices.text += "DeviceID: " + entry.Key + "\nDeviceName: " + entry.Value + "\n\n";
                    Debug.Log("Added device: " + entry.Key);

                }
                break;
            case "connected":
                ButtonEstablishConnection.enabled = false;
                TextTargetDeviceConnection.text = "Connected to target device:\n" + targetDeviceName;
                break;
            case "writeData":
                if (!readingThread.IsAlive)
                {

                    
                    
                    readingThread = new Thread(ReadBleData);
                    readingThread.Start();



                }

                if (datos != LastDato) //&& pres1 != lastPres1 && pres2 != lastPres2 && pres3 != lastPres3)
                {


                    char delimitador = ',';

                    string[] valores = datos.Split(delimitador);

                    TextTargetDeviceData1.text = "Acx: " + valores[0];


                    TextTargetDeviceData2.text = "Acy: " + valores[1];



                    TextTargetDeviceData3.text = "Acz: " + valores[2];



                    TextTargetDeviceData4.text = "gyrox: " + valores[3];



                    TextTargetDeviceData5.text = "gyroy: " + valores[4];



                    TextTargetDeviceData6.text = "gyroz: " + valores[5];

                    TextTargetDeviceData8.text = "press 1: " + valores[6];

                    TextTargetDeviceData9.text = "press 2: " + valores[7];

                    TextTargetDeviceData10.text = "press 3: " + valores[8];
                    LastDato = datos;
                    String dateNow = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                    TextInput = input;
                    addRecord(valores[0], valores[1], valores[2], valores[3], valores[4], valores[5], valores[6], valores[7], valores[8], dateNow, TextInput);

                }
                break;


             
        }
    }

    void ScanBleDevices()
    {
        scan = BLE.ScanDevices();
        Debug.Log("BLE.ScanDevices() started.");
        scan.Found = (_deviceId, deviceName) =>
        {
            Debug.Log("found device with name: " + deviceName);
            discoveredDevices.Add(_deviceId, deviceName);

            if (deviceId == null && deviceName == targetDeviceName)
                deviceId = _deviceId;
        };

        scan.Finished = () =>
        {
            isScanning = false;
            Debug.Log("scan finished");
            if (deviceId == null)
                deviceId = "-1";
        };
        while (deviceId == null)
        Thread.Sleep(500);
        scan.Cancel();
        scanningThread = null;
        isScanning = false;

        if (deviceId == "-1")
        {
            Thread.Sleep(500);
            scan.Cancel();
            scanningThread = null;
            isScanning = false;
            StartScanHandler();
            Debug.Log("no device found!");

        }
    }

    // Start establish BLE connection with
    // target device in dedicated thread.
    public void StartConHandler()
    {
        connectionThread = new Thread(ConnectBleDevice);
        connectionThread.Start();
    }

    void ConnectBleDevice()
    {
        if (deviceId != null)
        {
            try
            {
                ble.Connect(deviceId,
                serviceUuid,
                characteristicUuids);
            } catch(Exception e)
            {
                Debug.Log("Could not establish connection to device with ID " + deviceId + "\n" + e);
            }
        }
        if (ble.isConnected)
            Debug.Log("Connected to: " + targetDeviceName);
    }

    ulong ConvertLittleEndian(byte[] array)
    {
        int pos = 0;
        ulong result = 0;
        foreach (byte by in array)
        {
            result |= ((ulong)by) << pos;
            pos += 8;
        }
        return result;
    }
}
