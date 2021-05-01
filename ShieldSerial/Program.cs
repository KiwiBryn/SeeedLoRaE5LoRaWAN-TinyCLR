//---------------------------------------------------------------------------------
// Copyright (c) April 2021, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// must have one of 
//    TINYCLR_V2_FEZDUINO or
//    SERIAL_SYNC_READ or SERIAL_ASYNC_READ defined
//---------------------------------------------------------------------------------
namespace devMobile.IoT.SeeedLoRaE5.ShieldSerial
{
   using System;
   using System.Diagnostics;
   using System.Text;
   using System.Threading;

   using GHIElectronics.TinyCLR.Devices.Uart;
   using GHIElectronics.TinyCLR.Pins;

   public class Program
   {
      private static UartController serialDevice;
      private const string ATCommand = "at+ver\r\n";
#if TINYCLR_V2_FEZDUINO
      private static readonly string SerialPortId = SC20100.UartPort.Uart5;
#endif
#if TINYCLR_V2_SC20100DEV_MIKROBUS_1
      private const string SerialPortId = SC20100.UartPort.Usart2;
#endif
#if TINYCLR_V2_SC20100DEV_MIKROBUS_2
      private const string SerialPortId = SC20100.UartPort.Uart3;
#endif

      public static void Main()
      {
         Debug.WriteLine("devMobile.IoT.SeeedLoRaE5.ShieldSerial starting");

         try
         {
            serialDevice = UartController.FromName(SerialPortId);

            serialDevice.SetActiveSettings(new UartSetting()
            {
               BaudRate = 9600,
               Parity = UartParity.None,
               StopBits = UartStopBitCount.One,
               Handshaking = UartHandshake.None,
               DataBits = 8
            });

            serialDevice.Enable();

#if SERIAL_ASYNC_READ
            serialDevice.DataReceived += SerialDevice_DataReceived;
#endif

            while (true)
            {
               byte[] txBuffer = UTF8Encoding.UTF8.GetBytes(ATCommand);

               int txByteCount = serialDevice.Write(txBuffer);
               Debug.WriteLine($"TX: {txByteCount} bytes");

#if SERIAL_SYNC_READ
               while( serialDevice.BytesToWrite>0)
               {
                  Debug.WriteLine($" BytesToWrite {serialDevice.BytesToWrite}");
                  Thread.Sleep(100);
               }

               int rxByteCount = serialDevice.BytesToRead;
               if (rxByteCount>0)
               {
                  byte[] rxBuffer = new byte[rxByteCount];

                  serialDevice.Read(rxBuffer);

                  Debug.WriteLine($"RX sync:{rxByteCount} bytes read");
                  String response = UTF8Encoding.UTF8.GetString(rxBuffer);
                  Debug.WriteLine($"RX sync:{response}");
               }
#endif

               Thread.Sleep(20000);
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
         }
      }

#if SERIAL_ASYNC_READ
      private static void SerialDevice_DataReceived(UartController sender, DataReceivedEventArgs e)
      {
         byte[] rxBuffer = new byte[e.Count];

         sender.Read(rxBuffer, 0, e.Count);

         Debug.WriteLine($"RX Async:{e.Count} bytes read");
         String response = UTF8Encoding.UTF8.GetString(rxBuffer);
         Debug.WriteLine($"RX Async:{response}");
      }
#endif
   }
}

