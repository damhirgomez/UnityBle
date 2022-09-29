

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

BLEStringCharacteristic acc_x_characteristic("ABF0E002-B597-4BE0-B869-6054B7ED0CE3", BLERead | BLENotify,20);
BLEDescriptor acc_x_descriptor("2901", "accX");



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

//  imu_service.addCharacteristic(pres1_characteristic);
//  imu_service.addCharacteristic(pres2_characteristic);
//  imu_service.addCharacteristic(pres3_characteristic);
//  pres1_characteristic.addDescriptor(pres1_descriptor);
//  pres2_characteristic.addDescriptor(pres2_descriptor);
//  pres3_characteristic.addDescriptor(pres3_descriptor);
  
  BLE.addService(imu_service);


  acc_x_characteristic.writeValue("");

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
        String ValueFinal = ""; 
        String Coma = ",";
        ValueFinal.concat(x);
        ValueFinal.concat(Coma);
        ValueFinal.concat(y);
        ValueFinal.concat(Coma);
        ValueFinal.concat(z);


        //const unsigned char dataValue = stringToBytes(ValueFinal);
        acc_x_characteristic.writeValue(ValueFinal);

        Serial.println("String");
        Serial.println(ValueFinal);


        delay(200);
        digitalWrite(LEDB, !digitalRead(LEDB)); // ble heartbeat
        
      }
    }
  //}
  Serial.println("esperando");
  digitalWrite(LEDG, !digitalRead(LEDG)); // waiting ble connection
  delay(200);
}
