using LedWall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LedWall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string _path = AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] sp = SerialPort.GetPortNames();

            foreach (string s in sp)
            {
                Console.WriteLine(s);
            }

            SerialWriter[] Ports = SerialWriter.InitializeArray<SerialWriter>(sp.Length);

            int Height = 0;
            int Width = 0;

            int i = 0;
            foreach (string s in sp)
            {
                SerialWriter sw = new SerialWriter(s);
                if (sw.WriterHeight != 1)
                {
                    Height += sw.LedHeight;
                }
                else
                {
                    Height = sw.LedHeight;
                }
                if (sw.WriterWidth != 1)
                {
                    Width += sw.LedWidth;
                }
                else
                {
                    Width = sw.LedWidth;
                }
                Ports[i] = sw;
                i++;
            }
            Ledwall ld = new Ledwall(Width, Height, Ports);
            //ld.ReadImage("Images//red.jpg");
            ld.readVideo(_path + "Video//test.mp4");
        }
    }
}
