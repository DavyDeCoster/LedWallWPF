using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedWall
{
    class File
    {
        public string Path { get; set; }
        public bool IsVideo { get; set; }
        public string Name { get; set; }
        public List<File> Files { get; set; }
        public int Wait { get; set; }

        static string defaultPathVideo = AppDomain.CurrentDomain.BaseDirectory + "Video";
        static string defaultPathPicture = AppDomain.CurrentDomain.BaseDirectory + "Images";
        static string defaultPathSetting = AppDomain.CurrentDomain.BaseDirectory + "Settings//Files.csv";

        static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

        public File()
        {

        }

        public File(string Name, string Path, bool isVideo, int wait)
        {
            this.Name = Name;
            this.Path = Path;
            this.IsVideo = isVideo;
            this.Wait = wait;
        }

        public File(string Name, List<File> Files)
        {
            this.Name = Name;
            this.Files = Files;
        }

        public static List<File> GetAllFiles()
        {
            List<File> lstFile = GetFiles();

            return lstFile;
        }

        public static void SaveFiles(List<File> lstFiles)
        {
            StreamWriter sw = new StreamWriter(defaultPathSetting);

            sw.WriteLine("Files Ledwall-controller -- © Davy De Coster");
            sw.WriteLine("--------------------------------------------");
            foreach (File f in lstFiles)
            {
                if(f.Path != null)
                {
                    sw.WriteLine(f.Name + ";" + f.Path + ";" + f.IsVideo + ";" + f.Wait);
            
                }
                else
                {
                    sw.WriteLine("START MARQUEE");

                    foreach (File s in f.Files)
                    {
                        sw.WriteLine(s.Name + ";" + s.Path + ";" + s.IsVideo + ";" + s.Wait);
                    }

                    sw.WriteLine("END MARQUEE");
                }
            }
            sw.Close();
        }

        private static List<File> GetFiles()
        {
            List<File> lstFiles = new List<File>();
            try
            {
                StreamReader sr = new StreamReader(defaultPathSetting);
                sr.ReadLine();
                sr.ReadLine();

                string s = sr.ReadLine();

                while (s != null)
                {
                    if (s == "START MARQUEE")
                    {
                        s = sr.ReadLine();
                        File Marq = new File();
                        Marq.Files = new List<File>();
                        while (s != "END MARQUEE")
                        {
                            string[] sSplit = s.Split(';');

                            if (CheckPathForFile(sSplit[1]))
                            {
                                Marq.Files.Add(new File(sSplit[0], sSplit[1], Convert.ToBoolean(sSplit[2]), Convert.ToInt32(sSplit[3])));
                            }

                            Marq.Name = sSplit[0];

                            s = sr.ReadLine();
                        }
                        lstFiles.Add(Marq);
                        s = sr.ReadLine();
                    }
                    else
                    {
                        string[] sSplit = s.Split(';');

                        if (CheckPathForFile(sSplit[1]))
                        {
                            lstFiles.Add(new File(sSplit[0], sSplit[1], Convert.ToBoolean(sSplit[2]), Convert.ToInt32(sSplit[3])));
                        }

                        s = sr.ReadLine();
                    }
                }

                sr.Close();
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Settings");
            }
            catch (FileNotFoundException)
            {
                SaveFiles(lstFiles);
            }


            return lstFiles;
        }

        private static bool CheckPathForFile(string p)
        {
            return System.IO.File.Exists(p);
        }

        public override string ToString()
        {
            if(this.Path==null)
            {
                return Name + " (Marquee, " + this.Files[0].Wait + " sec)";
            }
            else if (this.IsVideo)
            {
                return Name + " (Video, "+Wait+" sec)";
            }
            else
            {
                return Name + " (Picture, " + Wait + " sec)";
            }
        }

        internal static string AddFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Files|*.BMP;*.JPG;*.GIF;*.PNG;*.MP4;*.AVI;*.MOV;*.MKV;*.WMV|All files (*.*)|*.*";
            if ((bool)ofd.ShowDialog())
            {
                return CopyFileToDirectory(ofd.FileName);
            }

            return null;
        }

        private static string CopyFileToDirectory(string p)
        {
            if (ImageExtensions.Contains(System.IO.Path.GetExtension(p).ToUpperInvariant()))
            {
                try
                {
                    System.IO.File.Copy(p, defaultPathPicture + "/" + System.IO.Path.GetFileName(p));
                    return defaultPathPicture + "/" + System.IO.Path.GetFileName(p);
                }
                catch (IOException)
                {
                    System.IO.File.Delete(defaultPathPicture + "/" + System.IO.Path.GetFileName(p));
                    System.IO.File.Copy(p, defaultPathPicture + "/" + System.IO.Path.GetFileName(p));
                    return defaultPathPicture + "/" + System.IO.Path.GetFileName(p);
                }
            }
            else
            {
                try
                {
                    System.IO.File.Copy(p, defaultPathVideo + "/" + System.IO.Path.GetFileName(p));
                    return defaultPathVideo + "/" + System.IO.Path.GetFileName(p);
                }
                catch (IOException)
                {
                    System.IO.File.Delete(defaultPathVideo + "/" + System.IO.Path.GetFileName(p));
                    System.IO.File.Copy(p, defaultPathVideo + "/" + System.IO.Path.GetFileName(p));
                    return defaultPathVideo + "/" + System.IO.Path.GetFileName(p);
                }
            }
        }

        internal static bool CheckIfVideoOrPicture(string NewPath)
        {
            if (ImageExtensions.Contains(System.IO.Path.GetExtension(NewPath).ToUpperInvariant()))
            {
                return false;
            }
            return true;
        }

        internal static void DeleteFile(File p)
        {
            if(p.Path != null)
            {
                System.IO.File.Delete(p.Path);
            }
            else
            {
                foreach (File f in p.Files)
                {
                    System.IO.File.Delete(f.Path);
                }
            }
        }
    }
}