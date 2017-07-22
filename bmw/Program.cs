using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;

namespace bmw
{
    class Program
    {
        static List<byte> bytes = new List<byte>();

        static void Main(string[] args)
        {
            var f = File.OpenRead(@"C:\Users\User\Documents\visual studio 2017\Projects\bmw\bmw\bin\Debug\76e94118-10de-4aec-ab0e-4211d7c3ca6c.bin");
            //var f = File.OpenRead(@"C:\Users\User\Documents\visual studio 2017\Projects\bmw\bmw\bin\Debug\98d79144-06b0-4f96-8298-44db4addf192.bin");

            var z = f.ReadByte();
            var extractor = new BmwBusPacketExtractor();

            while (z > -1)
            {
                extractor.PushByte((byte)z);
                z = f.ReadByte();
            }

            var sourceCounter = new Dictionary<byte, int>();
            var destCounter = new Dictionary<byte, int>();

            Console.WriteLine();
            Console.WriteLine("Sources: ");
            foreach (var p in extractor.OutputPackets)
            {
                if (p.From == 0x50)
                {
                    var z2 = BitConverter.ToString(p.Payload.ToArray());
                    Console.WriteLine($"Steering wheel: {z2}");
                }

                var tobyte = p.Payload[0];

                if (sourceCounter.ContainsKey(p.From))
                {
                    sourceCounter[p.From]++;
                } else
                {
                    sourceCounter[p.From] = 1;
                }

                if (destCounter.ContainsKey(tobyte))
                {
                    destCounter[tobyte]++;
                }
                else
                {
                    destCounter[tobyte] = 1;
                }
            }

            foreach (var src in sourceCounter)
            {
                Console.WriteLine($"Src: {BitConverter.ToString(new[] { src.Key })}, Val: {src.Value}");
                var knownDevice = (Devices)src.Key;
                if (Enum.IsDefined(typeof(Devices), knownDevice))
                {
                    Console.WriteLine(knownDevice);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Destinations: ");
            foreach (var src in destCounter)
            {
                Console.WriteLine($"Src: {BitConverter.ToString(new[] { src.Key })}, Val: {src.Value}");
                var knownDevice = (Devices)src.Key;
                if (Enum.IsDefined(typeof(Devices), knownDevice))
                {
                    Console.WriteLine(knownDevice);
                }
            }

            Debugger.Break();


            /*Console.Out.WriteLine("0x80, 0x05, 0xBF, 0x18, 0x00, 0x00, 0x22");
            Console.Out.WriteLine("0x50, 0x04, 0x68, 0x32, 0x11, 0x1F: Volume Up");

            Console.Out.WriteLine("0x50, 0x04, 0x68, 0x32, 0x11, 0x1F: Volume Up");
            Console.Out.WriteLine("0x50, 0x04, 0x68, 0x32, 0x10, 0x1E: Volume Down");
            */
            
            SerialPort mySerialPort = new SerialPort("COM13");

            mySerialPort.BaudRate = 9600;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Parity = Parity.Even;
            mySerialPort.RtsEnable = true;
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            mySerialPort.Open();

            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            while (true)
            {
                Console.ReadKey();
                // var byts  = new byte[] {     0x80, 0x0C, 0xFF, 0x24, 0x01, 0x00, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)' ', (byte)' '}.ToList();
                var byts = new byte[] { 0xD0, 0x07, 0xBF, 0x5B, 0x60,
0x00, 0x04,
0x00 }.ToList();
                //var byts = new byte[] { 0x68, 0x17, 0xE7, 0x23, 0x62, 0x30, 0x07, 0x20, 0x20, 0x20, 0x20, 0x20, 0x08, 0x43, 0x44, 0x20, 0x32, 0x2D, 0x30, 0x34, 0x20, 0x20, 0xBC, 0x20 }.ToList();
                byte checksum = 0;
                foreach (var b in byts)
                {
                    checksum ^= b;
                }

                byts.Add(checksum);

                var z2 = BitConverter.ToString(new[] { checksum });
                Console.WriteLine("Checksum:" + z2);

                mySerialPort.Write(byts.ToArray(), 0, byts.Count);
                Console.WriteLine("Wrote bytes");

            }

            Console.ReadLine();
            mySerialPort.Close();

        }

        private static void DataReceivedHandler(
                    object sender,
                    SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            for (int i = 0; i < sp.BytesToRead; i++)
            {
                var b = (byte)sp.ReadByte();
                bytes.Add(b);
                var z = BitConverter.ToString(new[] { b });
                Console.Write("0x" + z + ", ");
            }

            if (bytes.Count % 100 == 0)
            {
                File.WriteAllBytes(Guid.NewGuid().ToString() + ".bin", bytes.ToArray());
            }

            Console.WriteLine();
             // Console.WriteLine("Data Received:");
        }
    }
}
