using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedWall
{
    class SerialClass
    {
        public SerialPort[] SerialPorts { get; set; }
        
        public SerialClass ()
	    {
            String[] Coms = SerialPort.GetPortNames();
            SerialPorts = InitializeArray<SerialPort>(Coms.Length);

            int i = 0;
            foreach (string s in Coms)
            {
                SerialPort sp = new SerialPort(s);
                SerialPorts[i] = sp;
                i++;
            }
	    }

        T[] InitializeArray<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = new T();
            }

            return array;
        }

        public void SendData(sbyte[] data, SerialPort s)
        {
            if(!s.IsOpen)
            {
                s.Open();
            }

            byte[] uData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, uData, 0, data.Length);

            s.Write(uData, 0, uData.Length);
        }
    }
}
