using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Runtime;
using System.Timers;
using System.Threading;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.IO;
using Windows.Storage.Streams;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;

namespace BLE_HeartBeat
{
    class Program
    {

        static void Main(string[] args)
        {
            for (; Process.GetProcessesByName("vrchat").Length != 0; )
            {
                if (BLEHeartRate.CurrentStatus == BLEHeartRate.Status.STOPED)
                {
                    Thread childThread = new Thread(BLEHeartRate.init);
                    childThread.Start();
                    //Console.WriteLine("Starting severce");
                }
                else if (BLEHeartRate.CurrentStatus == BLEHeartRate.Status.ERROR)
                {
                    Thread childThread = new Thread(BLEHeartRate.init);
                    childThread.Start();
                    //Console.WriteLine("Initialization faild, reconnecting");
                }
                else if (BLEHeartRate.CurrentStatus == BLEHeartRate.Status.RUNNING)
                {
                    BLEHeartRate.checkIfTimedout();
                }
                Thread.Sleep(500);
            }
        }


    }
}
