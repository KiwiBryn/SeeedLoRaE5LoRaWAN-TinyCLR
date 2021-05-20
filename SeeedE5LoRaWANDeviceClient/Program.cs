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
// must have one of 
//    TINYCLR_V2_FEZDUINO or
//    PAYLOAD_BCD or PAYLOAD_BYTES defined
//    OTAA or ABP
//
// For confirmed messages define CONFIRMED
//---------------------------------------------------------------------------------
namespace devMobile.IoT.SeeedE5LoRaWANDeviceClient
{
   using System;
   using System.Threading;
   using System.Diagnostics;

   using GHIElectronics.TinyCLR.Pins;
   using GHIElectronics.TinyCLR.Devices.Uart;

   using devMobile.IoT.LoRaWan;

   public class Program
   {
#if TINYCLR_V2_FEZDUINO
      private const string SerialPortId = SC20100.UartPort.Uart5;
#endif
      private const string Region = "AS923";
      private static readonly TimeSpan JoinTimeOut = new TimeSpan(0, 0, 25);
      private static readonly TimeSpan SendTimeout = new TimeSpan(0, 0, 10);
      private const byte MessagePort = 15;
#if PAYLOAD_BCD
      private const string PayloadBcd = "010203040506070809";
#endif
#if PAYLOAD_BYTES
      private static readonly byte[] PayloadBytes = { 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
#endif

      public static void Main()
      {
         Result result;

         Debug.WriteLine("devMobile.IoT.SeeedE5LoRaWANDeviceClient starting");

         try
         {
            using (SeeedE5LoRaWANDevice device = new SeeedE5LoRaWANDevice())
            {
               result = device.Initialise(SerialPortId, 9600, UartParity.None, 8, UartStopBitCount.One);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"Initialise failed {result}");
                  return;
               }

#if CONFIRMED
               device.OnMessageConfirmation += OnMessageConfirmationHandler;
#endif
               device.OnReceiveMessage += OnReceiveMessageHandler;

#if RESET
               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Reset");
               result = device.Reset();
               if (result != Result.Success)
               {
                  Debug.WriteLine($"Reset failed {result}");
                  return;
               }
#endif

               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Region {Region}");
               result = device.Region(Region);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"Region failed {result}");
                  return;
               }

               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} ADR On");
               result = device.AdrOn();
               if (result != Result.Success)
               {
                  Debug.WriteLine($"ADR on failed {result}");
                  return;
               }

               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Port");
               result = device.Port(MessagePort);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"Port on failed {result}");
                  return;
               }

#if OTAA
               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} OTAA");
               result = device.OtaaInitialise(Config.AppEui, Config.AppKey);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"OTAA Initialise failed {result}");
                  return;
               }
#endif

#if ABP
               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} ABP");
               result = device.AbpInitialise(DevAddress, NwksKey, AppsKey);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"ABP Initialise failed {result}");
                  return;
               }
#endif

               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Join start Timeout:{JoinTimeOut.TotalSeconds} Seconds");
               result = device.Join(true, JoinTimeOut);
               if (result != Result.Success)
               {
                  Debug.WriteLine($"Join failed {result}");
                  return;
               }
               Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Join finish");

               while (true)
               {
#if PAYLOAD_BCD
                  Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Send Timeout:{SendTimeout.TotalSeconds} Seconds payload BCD:{PayloadBcd}");
#if CONFIRMED
                  result = device.Send(PayloadBcd, true, SendTimeout);
#else
                  result = device.Send(PayloadBcd, false, SendTimeout);
#endif
#endif

#if PAYLOAD_BYTES
                  Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Send Timeout:{SendTimeout.TotalSeconds} Seconds payload Bytes:{BitConverter.ToString(PayloadBytes)}");
#if CONFIRMED
                  result = device.Send(PayloadBytes, true, SendTimeout);
#else
                  result = device.Send(PayloadBytes, false, SendTimeout);
#endif
#endif
                  if (result != Result.Success)
                  {
                     Debug.WriteLine($"Send failed {result}");
                  }

                  Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Sleep");
                  result = device.Sleep();
                  if (result != Result.Success)
                  {
                     Debug.WriteLine($"Sleep failed {result}");
                     return;
                  }

                  Thread.Sleep(30000);

                  Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Wakeup");
                  result = device.Wakeup();
                  if (result != Result.Success)
                  {
                     Debug.WriteLine($"Wakeup failed {result}");
                     return;
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
         }
      }

      static void OnMessageConfirmationHandler(int rssi, double snr)
      {
         Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Send Confirm RSSI:{rssi} SNR:{snr}");
      }

      static void OnReceiveMessageHandler(int port, int rssi, double snr, string payloadBcd)
      {
         byte[] payloadBytes = SeeedE5LoRaWANDevice.BcdToByes(payloadBcd);

         Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} Receive Message RSSI:{rssi} SNR:{snr} Port:{port} Payload:{payloadBcd} PayLoadBytes:{BitConverter.ToString(payloadBytes)}");
      }
   }
}
