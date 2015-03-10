using System;
using System.Collections.Generic;
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

            SerialWriter[] Ports = SerialWriter.InitializeArray<SerialWriter>(sp.Length);

            int i = 0;
            foreach (string s in sp)
            {
                SerialWriter sw = new SerialWriter(s);
                Ports[i] = sw;
                i++;
            }
            Ledwall ld = new Ledwall(Ports[0].LedWidth, Ports[0].LedHeight, Ports);
            ld.readVideo(_path + "Video//test.mp4");
        }
    }
}
