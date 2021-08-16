using System;
using System.Text;
using MelonLoader;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VRC.SDK3.Avatars.ScriptableObjects;
using System.IO;
using System.Reflection;
using System.Diagnostics;


namespace BLE_HeartRate
{
    public class BLEHeartRate : MelonMod
    {

        public static void ExtractResFile(string resFileName, string outputFile)
        {
            BufferedStream inStream = null;
            FileStream outStream = null;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly(); 
                inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
                outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[1024];
                int length;

                while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, length);
                }
                outStream.Flush();
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Close();
                }
                if (inStream != null)
                {
                    inStream.Close();
                }
            }
        }

        private static UdpClient udpcRecv = null;
        private static IPEndPoint localIpep = null;
        Thread recvThread;
        public override void OnApplicationStart()
        {
            Console.WriteLine("HR init");
            Console.WriteLine(Path.GetTempPath() + "/BLE_HeartRateCore.exe");
            ExtractResFile("VRCHeartRate.BLE_HeartRateCore.exe", Path.GetTempPath() + "/BLE_HeartRateCore.exe");
            Process.Start(Path.GetTempPath() + "/BLE_HeartRateCore.exe");
            try
            {
                localIpep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15738);
                udpcRecv = new UdpClient(localIpep);
                recvThread = new Thread(RecvThreadFun);
                recvThread.Start();
            }
            catch { }
            base.OnApplicationStart();
        }

        void RecvThreadFun()
        {
            for (; ; )
            {
                byte[] bytRecv = udpcRecv.Receive(ref localIpep);
                string message = Encoding.UTF8.GetString(bytRecv, 0, bytRecv.Length);
                var index = ParamLib.ParamLib.FindParam("HeartRate", VRCExpressionParameters.ValueType.Float);
                if (index.Item1 != -1)
                    ParamLib.ParamLib.SetParameter(index.Item1, (float.Parse(message)+1) / 200.0f);
            }
        }

        public override void OnApplicationQuit()
        {
            recvThread.Abort();
            udpcRecv.Close();
            base.OnApplicationQuit();
        }
    }
}
