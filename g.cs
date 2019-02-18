////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Windows;

namespace File_Split_and_Join
{
    public static class g
    {
        public static MainWindow myStartWin;
        public static int CacheSize = 1024;
        public static SplitAndJoinFile FileManager;
        public static void OpenFolder(string PathFolder)
        {
            string windir = Environment.GetEnvironmentVariable("WINDIR");
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = windir + @"\explorer.exe";
            prc.StartInfo.Arguments = PathFolder;
            prc.Start();
        }
        /////////////////////////////////////////
        public static System.Collections.ArrayList errorListCalculator;
        public static string resultCalculator;
        public static string last_lang_programm = "eng";
        public static ResourceDictionary dict;
    }
}
