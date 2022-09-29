

// based on https://github.com/Seeed-Studio/Seeed_Arduino_LSM6DS3/blob/master/examples/HighLevelExample/HighLevelExample.ino
// BLE   UUID: https://btprodspecificationrefs.blob.core.windows.net/assigned-values/16-bit%20UUID%20Numbers%20Document.pdf
#include <Arduino.h>
#include <ArduinoBLE.h>
#include <LSM6DS3.h>
#include "Wire.h"

// Create a instance of class LSM6DS3
LSM6DS3 my_imu(I2C_MODE, 0x6A); // I2C device address 0x6A
//push button variable
int pushButton = 2;// asignar el pin al que se conecta el boton
int buttonState = 0;         // variable for reading the pushbutton status
//pines sensor presion
int PresSensor1 = 3;
int PresSensor2 = 4;
int PresSensor3 = 5;

// BLE Variables
// Physical Activity Monitor: 0x183E
BLEService imu_service("ABF0E000-B597-4BE0-B869-6054B7ED0CE3");
// ACC
// acceleration unit = m/s**2
BLEByteCharacteristic acc_unit_characteristic("2713", BLERead);
// xiaoble is 32bit chip: 64bit,4 byte

BLEFloatCharacteristic acc_x_characteristic("ABF0E002-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEFloatCharacteristic acc_y_characteristic("ABF0E003-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEFloatCharacteristic acc_z_characteristic("ABF0E004-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEDescriptor acc_x_descriptor("2901", "accX");
BLEDescriptor acc_y_descriptor("2901", "accY");
BLEDescriptor acc_z_descriptor("2901", "accZ");

// Gyro
BLEFloatCharacteristic gyro_x_characteristic("ABF0E005-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEFloatCharacteristic gyro_y_characteristic("ABF0E006-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEFloatCharacteristic gyro_z_characteristic("ABF0E007-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
BLEDescriptor gyro_x_descriptor("2901", "gyroX");
BLEDescriptor gyro_y_descriptor("2901", "gyroY");
BLEDescriptor gyro_z_descriptor("2901", "gyroZ");

// Presion
//BLEFloatCharacteristic pres1_characteristic("ABF0E008-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
//BLEDescriptor pres1_descriptor("2901", "Pres1");
//BLEFloatCharacteristic pres2_characteristic("ABF0E009-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
//BLEDescriptor pres2_descriptor("2901", "Pres2");
//BLEFloatCharacteristic pres3_characteristic("ABF0E010-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify);
//BLEDescriptor pres3_descriptor("2901", "Pres3");

void setup()
{
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(pushButton, INPUT);
  // build-in leds
  pinMode(LEDR, OUTPUT);
  pinMode(LEDG, OUTPUT);
  pinMode(LEDB, OUTPUT);
  // turn off the all build-in leds
  digitalWrite(LEDR, HIGH);
  digitalWrite(LEDG, HIGH);
  digitalWrite(LEDB, HIGH);

  // Call .begin() to configure the IMUs
  if (my_imu.begin() != 0)
  {
    Serial.println("Device error");
    digitalWrite(LEDR, LOW);
  }
  else
  {
    Serial.println("Device OK!");
  }

  // ble
  // begin initialization
  if (!BLE.begin())
  {
    Serial.println("starting BLE failed!");

    while (1)
    {
      digitalWrite(LEDR, !digitalRead(LEDR));
      delay(500);
    };
  }

  BLE.setDeviceName("ArduinoIMU");
  BLE.setLocalName("ArduinoIMU");
  BLE.setAdvertisedService(imu_service);


  // acc
  imu_service.addCharacteristic(acc_x_characteristic);
  imu_service.addCharacteristic(acc_y_characteristic);
  imu_service.addCharacteristic(acc_z_characteristic);
  acc_x_characteristic.addDescriptor(acc_x_descriptor);
  acc_y_characteristic.addDescriptor(acc_y_descriptor);
  acc_z_characteristic.addDescriptor(acc_z_descriptor);

  // gyro
  imu_service.addCharacteristic(gyro_x_characteristic);
  imu_service.addCharacteristic(gyro_y_characteristic);
  imu_service.addCharacteristic(gyro_z_characteristic);
  gyro_x_characteristic.addDescriptor(gyro_x_descriptor);
  gyro_y_characteristic.addDescriptor(gyro_y_descriptor);
  gyro_z_characteristic.addDescriptor(gyro_z_descriptor);
//  // Presion
//  imu_service.addCharacteristic(pres1_characteristic);
//  imu_service.addCharacteristic(pres2_characteristic);
//  imu_service.addCharacteristic(pres3_characteristic);
//  pres1_characteristic.addDescriptor(pres1_descriptor);
//  pres2_characteristic.addDescriptor(pres2_descriptor);
//  pres3_characteristic.addDescriptor(pres3_descriptor);
  
  BLE.addService(imu_service);


  acc_x_characteristic.writeValueLE(0);
  acc_y_characteristic.writeValueLE(0);
  acc_z_characteristic.writeValueLE(0);
  gyro_x_characteristic.writeValueLE(0);
  gyro_y_characteristic.writeValueLE(0);
  gyro_z_characteristic.writeValueLE(0);
//  pres1_characteristic.writeValueLE(0);
//  pres2_characteristic.writeValueLE(0);
//  pres3_characteristic.writeValueLE(0);

  BLE.advertise();
}

void loop()
{
  buttonState = digitalRead(pushButton);
  //if(buttonState == 0){
    
        BLEDevice central = BLE.central();
  
    if (central)
    {
      while (central.connected())
      {
        
  
        float x = my_imu.readFloatAccelX();
        float y = my_imu.readFloatAccelY();
        float z = my_imu.readFloatAccelZ();
        acc_x_characteristic.writeValueLE(x);
        acc_y_characteristic.writeValueLE(y);
        acc_z_characteristic.writeValueLE(z);
        float gx = my_imu.readFloatGyroX();
        float gy = my_imu.readFloatGyroY();
        float gz = my_imu.readFloatGyroZ();
        gyro_x_characteristic.writeValueLE(gx);
        gyro_y_characteristic.writeValueLE(gy);
        gyro_z_characteristic.writeValueLE(gz);
        Serial.println("acx");
        Serial.println(y);
        Serial.println("acy");
        Serial.println(y);
        Serial.println("acz");
        Serial.println(y);
        Serial.println("gx");
        Serial.println(gx);
        Serial.println("gy");
        Serial.println(gy);
        Serial.println("gz");
        Serial.println(gz);
        //Serial.println(gyro_z_characteristic);
//        int presion1 = digitalRead(PresSensor1);
//        int presion2 = digitalRead(PresSensor2);
//        int presion3 = digitalRead(PresSensor3);
//        pres1_characteristic.writeValueLE(presion1);
//        pres2_characteristic.writeValueLE(presion2);
//        pres3_characteristic.writeValueLE(presion3);
        delay(200);
        digitalWrite(LEDB, !digitalRead(LEDB)); // ble heartbeat
        
      }
    }
  //}
  Serial.println("esperando");
  digitalWrite(LEDG, !digitalRead(LEDG)); // waiting ble connection
  delay(200);
}
