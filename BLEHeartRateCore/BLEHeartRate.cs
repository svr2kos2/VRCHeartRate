using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using System.Threading;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using System.Net;
using System.Net.Sockets;

namespace BLE_HeartBeat
{
    public class BLEHeartRate
    {
        private static T AsyncResult<T>(IAsyncOperation<T> async)
        {
            while (true)
            {
                switch (async.Status)
                {
                    case AsyncStatus.Started:
                        Thread.Sleep(10);
                        continue;
                    case AsyncStatus.Completed:
                        return async.GetResults();
                    case AsyncStatus.Error:
                        throw async.ErrorCode;
                    case AsyncStatus.Canceled:
                        throw new TaskCanceledException();
                }
            }
        }

        public enum Status {STOPED,STARTING,ERROR,RUNNING };

        static DeviceInformation device;
        static GattDeviceService service;
        static GattCharacteristic heartrate;
        static public void close()
        {
            try
            {
                if (CurrentStatus == Status.RUNNING)
                    service.Dispose();
                CurrentStatus = Status.STOPED;
            }
            catch
            {
            }
        }

        ~BLEHeartRate()
        {
            if (CurrentStatus == Status.RUNNING)
            {
                close();
            }
        }

        static public Status CurrentStatus = Status.STOPED;
        static public int heartrate_val = 0;
        static public DateTime lastupdate = DateTime.Now;

        static public void checkIfTimedout()
        {
            if(CurrentStatus == Status.RUNNING && DateTime.Now.Second - lastupdate.Second > 5)
            {
                close();
                init();
            }
        }

        static public void init()
        {
            CurrentStatus = Status.STARTING;
            localIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15737);
            udpcSend = new UdpClient(localIpep);
            try
            {
                var heartrateSelector = GattDeviceService
                    .GetDeviceSelectorFromUuid(GattServiceUuids.HeartRate);
                var devices = AsyncResult(DeviceInformation
                    .FindAllAsync(heartrateSelector));
                device = devices.FirstOrDefault();
                service = AsyncResult(GattDeviceService.FromIdAsync(device.Id));
                const int _heartRateMeasurementCharacteristicId = 0x2A37;

                heartrate = AsyncResult(service.GetCharacteristicsForUuidAsync(BluetoothUuidHelper.FromShortId(
                        _heartRateMeasurementCharacteristicId))).Characteristics.FirstOrDefault();

                var status = AsyncResult(
                    heartrate.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify));

                heartrate.ValueChanged += HeartRate_ValueChanged;
                CurrentStatus = Status.RUNNING;
                lastupdate = DateTime.Now;
            }
            catch
            {
                CurrentStatus = Status.ERROR;
            }
        }
        enum ContactSensorStatus
        {
            NotSupported,
            NotSupported2,
            NoContact,
            Contact
        }

        static BLEHeartRate hr = new BLEHeartRate();
        static UdpClient udpcSend = null;
        static IPEndPoint localIpep;

        static void SendData(int hr)
        {
            IPEndPoint remotelpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15738);
            byte[] sendbytes = Encoding.UTF8.GetBytes(hr.ToString());
            udpcSend.Send(sendbytes, sendbytes.Length, remotelpep);
        }

        static void HeartRate_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                var value = args.CharacteristicValue;
                if (value.Length == 0)
                {
                    return;
                }

                using (var reader = DataReader.FromBuffer(value))
                {
                    var bpm = -1;
                    var flags = reader.ReadByte();
                    var isshort = (flags & 1) == 1;
                    var contactSensor = (ContactSensorStatus)((flags >> 1) & 3);
                    var minLength = isshort ? 3 : 2;

                    if (value.Length < minLength)
                    {
                        //Console.WriteLine($"Buffer was too small. Got {value.Length}, expected {minLength}.");
                        return;
                    }

                    if (value.Length > 1)
                    {
                        bpm = isshort
                            ? reader.ReadUInt16()
                            : reader.ReadByte();
                    }
                    heartrate_val = bpm;
                    lastupdate = DateTime.Now;
                    SendData(heartrate_val);
                }
            }
            catch
            {
                //Console.WriteLine("BLEHR read faild");
            }
        }
    }
}
