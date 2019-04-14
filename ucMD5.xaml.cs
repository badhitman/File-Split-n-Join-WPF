////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;

namespace FileSplitAndJoinWPF
{
    /// <summary>
    /// Логика взаимодействия для ucMD5.xaml
    /// </summary>
    public partial class ucMD5 : UserControl
    {
        public ucMD5()
        {
            InitializeComponent();
        }

        public void Set_lng()
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(g.dict);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            // Set filter options and filter index.
            openFileDialog1.Filter = "All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                FileInfo fInfo = new FileInfo(openFileDialog1.FileName);
                PathFile.Text = "MD5:    " + GetHash(openFileDialog1.FileName);
                PathFile.Text += Environment.NewLine + Environment.NewLine + g.dict["MD5_FileName"] + ": " + openFileDialog1.FileName;
                PathFile.Text += Environment.NewLine + g.dict["MD5_Creation"] + ":  " + fInfo.CreationTime;
                PathFile.Text += Environment.NewLine + g.dict["MD5_LastAccess"] + ":   " + fInfo.LastAccessTime;
                PathFile.Text += Environment.NewLine + g.dict["MD5_LastWrite"] + ":    " + fInfo.LastWriteTime;
                PathFile.Text += Environment.NewLine + g.dict["MD5_LengthFile"] + ": " + fInfo.Length + " b";
                if (fInfo.Length > 1024)
                    PathFile.Text += " = " + Math.Round((double)fInfo.Length / 1024, 2) + " kb";
                if (fInfo.Length > 1024 * 1024)
                    PathFile.Text += " = " + Math.Round((double)fInfo.Length / 1024 / 1024, 2) + " mb";
            }
        }

        public static string GetHash(string pathSrc)
        {
            String md5Result;
            StringBuilder sb = new StringBuilder();
            MD5 md5Hasher = MD5.Create();

            using (FileStream fs = File.OpenRead(pathSrc))
            {
                foreach (Byte b in md5Hasher.ComputeHash(fs))
                    sb.Append(b.ToString("x2").ToLower());
            }

            md5Result = sb.ToString();

            return md5Result;
        }
    }
}
