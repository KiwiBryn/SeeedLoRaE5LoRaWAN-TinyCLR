//---------------------------------------------------------------------------------
// Copyright (c) May 2021, devMobile Software
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
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.SeeedLoRaE5.NetworkJoinOTAA
{
   using System;
   using System.Diagnostics;
   using System.Text;
   using System.Threading;

   using GHIElectronics.TinyCLR.Devices.Uart;
   using GHIElectronics.TinyCLR.Pins;

   public class Program
   {
#if TINYCLR_V2_FEZDUINO
      private static string SerialPortId = SC20100.UartPort.Uart5;
#endif
      private const string AppKey = "................................";

      //txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+ID=AppEui,{AppEui}\r\n"));
      //private const string AppEui = "................";

      //txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+ID=AppEui,\"{AppEui}\"\r\n"));
      private const string AppEui = ".. .. .. .. .. .. .. ..";

      private const byte messagePort = 1;

      //private const string payload = "48656c6c6f204c6f526157414e"; // Hello LoRaWAN
      private const string payload = "01020304"; // AQIDBA==
      //private const string payload = "04030201"; // BAMCAQ==

      public static void Main()
      {
         UartController serialDevice;
         int txByteCount;
         int rxByteCount;

         Debug.WriteLine("devMobile.IoT.SeeedE5.NetworkJoinOTAA starting");

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

            // Set the Region to AS923
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes("AT+DR=AS923\r\n"));
            Debug.WriteLine($"TX: DR {txByteCount} bytes");
            Thread.Sleep(500);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }

            // Set the Join mode
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes("AT+MODE=LWOTAA\r\n"));
            Debug.WriteLine($"TX: MODE {txByteCount} bytes");
            Thread.Sleep(500);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }

            // Set the appEUI
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+ID=AppEui,\"{AppEui}\"\r\n"));
            Debug.WriteLine($"TX: ID=AppEui {txByteCount} bytes");
            Thread.Sleep(500);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }
            
            // Set the appKey
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+KEY=APPKEY,{AppKey}\r\n"));
            Debug.WriteLine($"TX: KEY=APPKEY {txByteCount} bytes");
            Thread.Sleep(500);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }

            // Set the PORT
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+PORT={messagePort}\r\n"));
            Debug.WriteLine($"TX: PORT {txByteCount} bytes");
            Thread.Sleep(500);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }

            // Join the network
            txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes("AT+JOIN\r\n"));
            Debug.WriteLine($"TX: JOIN {txByteCount} bytes");
            Thread.Sleep(10000);

            // Read the response
            rxByteCount = serialDevice.BytesToRead;
            if (rxByteCount > 0)
            {
               byte[] rxBuffer = new byte[rxByteCount];
               serialDevice.Read(rxBuffer);
               Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
            }

            while (true)
            {
               // Unconfirmed message
               txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+MSGHEX=\"{payload}\"\r\n"));
               Debug.WriteLine($"TX: MSGHEX {txByteCount} bytes");

               // Confirmed message
               //txByteCount = serialDevice.Write(UTF8Encoding.UTF8.GetBytes($"AT+CMSGHEX=\"{payload}\"\r\n"));
               //Debug.WriteLine($"TX: CMSGHEX {txByteCount} bytes");

               Thread.Sleep(10000);

               // Read the response
               rxByteCount = serialDevice.BytesToRead;
               if (rxByteCount > 0)
               {
                  byte[] rxBuffer = new byte[rxByteCount];
                  serialDevice.Read(rxBuffer);
                  Debug.WriteLine($"RX :{UTF8Encoding.UTF8.GetString(rxBuffer)}");
               }

               Thread.Sleep(30000);
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
         }
      }
   }
}
