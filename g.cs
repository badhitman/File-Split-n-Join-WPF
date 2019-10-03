////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Windows;
using TextFileScanerLib;

namespace FileSplitAndJoinWPF
{
    public static class g
    {
        public static MainWindow myStartWin { get; set; }
        public static int CacheSize { get; set; } = 1024;
        public static FileSplitAndJoin FileManager { get; set; } = new FileSplitAndJoin();
        public static void OpenFolder(string PathFolder)
        {
            string windir = Environment.GetEnvironmentVariable("WINDIR");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = windir + @"\explorer.exe";
            prc.StartInfo.Arguments = PathFolder;
            prc.Start();
        }
        /////////////////////////////////////////
        public static System.Collections.ArrayList errorListCalculator { get; set; }
        public static string resultCalculator { get; set; }
        public static string last_lang_programm { get; set; } = "eng";
        public static ResourceDictionary Dict { get; set; }
    }
}
