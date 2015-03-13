using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedWall
{
    class SerialWriter
    {
        public SerialPort Port { get; set; }
        public int LedHeight { get; set; }
        public int LedWidth { get; set; }
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double WriterHeight { get; set; }
        public double WriterWidth { get; set; }
        public int LedLayout { get; set; }
        public sbyte[] data { get; set; }
        
        public SerialWriter(string com)
	    {
            this.Port = new SerialPort(com);
            GetSetting();
	    }

        public SerialWriter()
        {

        }

        public static T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }

            return array;
        }

        private void GetSetting()
        {
            PortChecker();

            Port.Write("?");
            ParseSetting(Port.ReadLine());
        }

        private void ParseSetting(string p)
        {
            string[] setting = p.Split(',');

            LedWidth = Convert.ToInt32(setting[0]);
            LedHeight = Convert.ToInt32(setting[1]);
            LedLayout = Convert.ToInt32(setting[2]);
            XOffset = Convert.ToDouble(setting[5])/100;
            YOffset = Convert.ToDouble(setting[6])/100;
            WriterWidth = Convert.ToDouble(setting[7])/100;
            WriterHeight = Convert.ToDouble(setting[8])/100;
        }

        public void SendData(sbyte[] data)
        {
            PortChecker();

            byte[] uData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, uData, 0, data.Length);

            Port.Write(uData, 0, uData.Length);
        }

        public void SendData()
        {
            PortChecker();

            byte[] uData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, uData, 0, data.Length);
            Port.Write(uData, 0, uData.Length);
        }

        private void PortChecker()
        {
            if (!Port.IsOpen)
            {
                Port.Open();
            }
        }

        public override string ToString()
        {
            return "Width: " + this.LedWidth + " Height: " + LedHeight + " Offsets: X :" + XOffset + " Y: "+YOffset + "WriterWidth: "+ WriterWidth + "WriterHeight: "+ WriterHeight;
        }
    }
}