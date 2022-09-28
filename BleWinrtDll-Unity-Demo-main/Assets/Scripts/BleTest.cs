using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;


public class BleTest : MonoBehaviour
{
    // Change this to match your device.
    string targetDeviceName = "ArduinoIMU";
    string serviceUuid = "{ABF0E000-B597-4BE0-B869-6054B7ED0CE3}";
    string[] characteristicUuids = {      
         "{ABF0E002-B597-4BE0-B869-6054B7ED0CE3}",
         "{ABF0E003-B597-4BE0-B869-6054B7ED0CE3}",
         "{ABF0E004-B597-4BE0-B869-6054B7ED0CE3}",
         "{ABF0E005-B597-4BE0-B869-6054B7ED0CE3}",
         "{ABF0E006-B597-4BE0-B869-6054B7ED0CE3}",
         "{ABF0E007-B597-4BE0-B869-6054B7ED0CE3}"
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
    public Text TextDiscoveredDevices, TextIsScanning, TextTargetDeviceConnection, TextTargetDeviceData1, TextTargetDeviceData2, TextTargetDeviceData3, TextTargetDeviceData4, TextTargetDeviceData5, TextTargetDeviceData6, TextTargetDeviceData7;
    public Button ButtonEstablishConnection, ButtonStartScan;
    float acx, lastAcx, acy, lastAcy, acz, lastAcz, gyrox, lastGyrox, gyroy, lastGyroy, gyroz, lastGyroz, pres1, lastPres1, pres2, lastPres2, pres3, lastPres3; //damhir
    float datos;//damhir
    string Nombre, LastNombre;//damhir
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
        Nombre = BLE.ReadPackage();
        Debug.Log("Nombre" + Nombre);
        packageReceived = BLE.ReadBytes(Nombre);
        datos = BitConverter.ToSingle(packageReceived, 0);
        TextTargetDeviceData7.text = "service: " + Nombre;
        switch (Nombre)
        {
            case "{abf0e002-b597-4be0-b869-6054b7ed0ce3}":
                acx = datos;
                TextTargetDeviceData1.text = "acx: " + acx;
                break;
            case "{abf0e003-b597-4be0-b869-6054b7ed0ce3}":
                acy = datos;
                TextTargetDeviceData2.text = "acy: " + acy;

                break;
            case "{abf0e004-b597-4be0-b869-6054b7ed0ce3}":
                acz = datos;
                TextTargetDeviceData3.text = "acz: " + acz;
                break;
            case "{abf0e005-b597-4be0-b869-6054b7ed0ce3}":

                gyrox = datos;
                TextTargetDeviceData4.text = "gyrox: " + gyrox;

                break;
            case "{abf0e006-b597-4be0-b869-6054b7ed0ce3}":

                gyroy = datos;
                TextTargetDeviceData5.text = "gyroy: " + gyroy;

                break;
            case "{abf0e007-b597-4be0-b869-6054b7ed0ce3}":
                gyroz = datos;
                TextTargetDeviceData6.text = "gyroz: " + gyroz;
                break;
        }
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
                
                 break;

            /*if (acx != lastAcx || acy != lastAcy || acz != lastAcz || gyrox != lastGyrox || gyroz != lastGyroz ||
                gyroy != lastGyroz || LastNombre != Nombre) //&& pres1 != lastPres1 && pres2 != lastPres2 && pres3 != lastPres3)
            {

                        TextTargetDeviceData1.text = "Acx: " + acx;


                        TextTargetDeviceData2.text = "Acy: " + acy;



                        TextTargetDeviceData3.text = "Acz: " + acz;



                        TextTargetDeviceData4.text = "gyrox: " + gyrox;



                        TextTargetDeviceData5.text = "gyroy: " + gyroy;



                        TextTargetDeviceData6.text = "gyroz: " + gyroz; 


                lastAcx = acx;
                lastAcy = acy;
                lastAcz = acz;
                lastGyrox = gyrox;
                lastGyroy = gyroy;
                lastGyroz = gyroz;
                LastNombre = Nombre;


            }*/

             
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
            Debug.Log("no device found!");
            return;
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
