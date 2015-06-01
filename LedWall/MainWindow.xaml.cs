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

        public string NewPath { get; set; }
        Ledwall ld;
        Thread PlaylistThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillingComboboxes();
            GettingSerialports();
            btnStop.IsEnabled = false;
            EnableDisableAdd();
        }

        private void FillingComboboxes()
        {
            List<File> Files = File.GetAllFiles();

            foreach (File f in Files)
            {
                lstFiles.Items.Add(f);
            }
        }

        private void GettingSerialports()
        {
            string[] sp = SerialPort.GetPortNames();

            if (sp.Length != 0)
            {
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
                        Height += Convert.ToInt32(sw.LedHeight*sw.WriterHeight);
                    }
                    else
                    {
                        Height = sw.LedHeight;
                    }
                    if (sw.WriterWidth != 1)
                    {
                        Width += Convert.ToInt32(sw.LedWidth * sw.WriterWidth);
                    }
                    else
                    {
                        Width = sw.LedWidth;
                    }
                    Ports[i] = sw;
                    i++;
                }
                ld = new Ledwall(Width, Height, Ports);
            }
            else
            {
                MessageBox.Show("No Serial Connection, you can only add files");
                btnPlayPlaylist.IsEnabled = false;
                btnSendOne.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            List<File> lstSaveFiles = new List<File>();

            foreach (File f in lstFiles.Items)
            {
                lstSaveFiles.Add(f);
            }

            File.SaveFiles(lstSaveFiles);
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            File f = new File(txtName.Text, NewPath, File.CheckIfVideoOrPicture(NewPath), (int)slTime.Value);
            lstFiles.Items.Add(f);

            NewPath = "";
            txtName.Text = "";
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            NewPath = File.AddFile();
            EnableDisableAdd();
        }

        private void btnSendOne_Click(object sender, RoutedEventArgs e)
        {
            File f = (File)lstFiles.SelectedItem;

            if(f.Files==null)
            {
                if (f.IsVideo)
                {
                    ld.ReadVideo(f.Path);
                }
                else
                {
                    ld.ReadImage(f.Path);
                }
            }

            else
            {
                foreach (File s in f.Files)
                {
                    ld.ReadImage(s.Path);
                }
            }
        }

        private void btnDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            File.DeleteFile((File)lstFiles.SelectedItem);
            lstFiles.Items.Remove(lstFiles.SelectedItem);
        }

        private void btnShiftUp_Click(object sender, RoutedEventArgs e)
        {
            ShiftUp();
        }

        private void ShiftUp()
        {
            int index = lstFiles.SelectedIndex;
            if (index > 0)
            {
                File f = (File)lstFiles.Items.GetItemAt(index - 1);
                lstFiles.Items.Insert(index - 1, lstFiles.SelectedItem);
                lstFiles.Items.RemoveAt(index);

                lstFiles.Items.Insert(index, f);
                lstFiles.Items.RemoveAt(index + 1);

                lstFiles.SelectedIndex = index - 1;
            }
        }

        private void btnShiftDown_Click(object sender, RoutedEventArgs e)
        {
            ShiftDown();
        }

        private void ShiftDown()
        {
            int index = lstFiles.SelectedIndex;
            if (index != lstFiles.Items.Count - 1)
            {
                File f = (File)lstFiles.Items.GetItemAt(index + 1);
                lstFiles.Items.Insert(index + 1, lstFiles.SelectedItem);
                lstFiles.Items.RemoveAt(index);

                lstFiles.Items.Insert(index, f);
                lstFiles.Items.RemoveAt(index + 2);

                lstFiles.SelectedIndex = index + 1;
            }
        }

        private void btnPlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            SendPlaylist();
            btnPlayPlaylist.IsEnabled = false;
            btnShiftDown.IsEnabled = false;
            btnShiftUp.IsEnabled = false;
        }

        private void SendPlaylist()
        {
            List<File> Files = new List<File>();

            foreach (File f in lstFiles.Items)
            {
                Files.Add(f);
            }

            if (ld != null)
            {
                ld.Playlist = Files;
                ld.Stop = false;
                PlaylistThread = new Thread(new ThreadStart(ld.SendPlaylist));

                PlaylistThread.Start();
                btnStop.IsEnabled = true;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            PlaylistThread.Abort();
            ld.Stop = true;
            btnStop.IsEnabled = false;
            btnPlayPlaylist.IsEnabled = true;
            btnShiftUp.IsEnabled = true;
            btnShiftDown.IsEnabled = true;
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableDisableAdd();
        }

        private void EnableDisableAdd()
        {
            if (txtName.Text == "" || NewPath == null)
            {
                btnAddFile.IsEnabled = false;
            }
            else
            {
                btnAddFile.IsEnabled = true;
            }
        }

        private void btnAddText_Click(object sender, RoutedEventArgs e)
        {
            if(!(bool)chkMarquee.IsChecked)
            {
                SaveTextToImage();
            }
            else
            {
                SaveTextToMarquee();
            }
        }

        private void SaveTextToMarquee()
        {
            string text = txtText.Text;
            int width;
            int height;
            if (ld != null)
            {
                width = ld.Width;
                height = ld.Height;
            }
            else
            {
                width = 107;
                height = 48;
            }

            int marginText = -1*(1+text.Length * 25);
            int startText = 100;
            int i = 0;
            List<string> lstPaths = new List<string>();
            List<File> lstFilesMarquee = new List<File>();

            while(startText>marginText)
            {
                Bitmap bm = TxtToImage.ConvertTextToImage(text.ToUpper(), "Lucida Console", 25, System.Drawing.Color.Black, System.Drawing.Color.White, width, height, startText, 0);
                string Path = _path + "Marquee\\" + text + i.ToString() + ".bmp";
                bm.Save(Path);
                startText -= 1;
                i++;
                lstPaths.Add(Path);
                File f = new File(text, Path, false, (int)slTimeText.Value);
                lstFilesMarquee.Add(f);
            }

            File fMarquee = new File(text, lstFilesMarquee);
            lstFiles.Items.Add(fMarquee);
            txtText.Text = "";
        }

        private void SaveTextToImage()
        {
            string text = txtText.Text;
            int width;
            int height;
            if (ld != null)
            {
                width = ld.Width;
                height = ld.Height;
            }
            else
            {
                width = 107;
                height = 48;
            }

            Bitmap bm = TxtToImage.ConvertTextToImage(text.ToUpper(), "Courier", 25, System.Drawing.Color.Black, System.Drawing.Color.White, width, height);
            string Path = _path + "Text\\" + text + ".bmp";
            bm.Save(Path);

            File f = new File(text, Path, false, (int)slTimeText.Value);
            lstFiles.Items.Add(f);

            txtText.Text = "";
        }

        private void chkLoop_Checked(object sender, RoutedEventArgs e)
        {
            ld.Loop = true;
        }

        private void chkLoop_Unchecked(object sender, RoutedEventArgs e)
        {
            ld.Loop = false;
        }
    }
}