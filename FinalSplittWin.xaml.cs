using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace File_Split_and_Join
{
    public enum Options { File, Mode, extMode };
    /// <summary>
    /// Логика взаимодействия для FinalSplitWin.xaml
    /// </summary>
    public partial class FinalSplitWin : Window
    {
        public delegate void SplitFileOfSizeDelegate(FinalSplitWin _win, string destFolder, long size, bool repeat = false, int repeatEvery = 1);
        //public SplitFileOfSizeDelegate SplitFileOfSizeDelegateObj;

        public delegate void SplitFileOfDataDelegate(FinalSplitWin _win, string destFolder, byte[] dataSearch, bool repeat = false, int repeatEvery = 1);
        //public SplitFileOfDataDelegate SplitFileOfDataDelegateObj;

        private Dictionary<Options, string> myOptions = new Dictionary<Options, string>();

        public FinalSplitWin()
        {
            InitializeComponent();
            Set_lng();
        }

        public void Set_lng()
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(g.dict);
        }

        public FinalSplitWin(Dictionary<Options, string> inOptions)
        {
            InitializeComponent();
            Set_lng();
            this.myOptions = inOptions;
            Title += " - " + myOptions[Options.File];
            LabelSplitMode.Content += " " + myOptions[Options.Mode];
            LabelSplitOf.Content += " " + myOptions[Options.extMode];
        }

        private void repeatCut_Click(object sender, RoutedEventArgs e)
        {
            if (everyCut.IsChecked.Value)
            {
                wathRepeat.IsEnabled = true;
                wathRepeat.Text = wathRepeat.Tag.ToString();
            }
            else
            {
                wathRepeat.Tag = wathRepeat.Text;
                wathRepeat.Text = "1";
                wathRepeat.IsEnabled = false;
            }
        }

        private void wathRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(wathRepeat.Text.ToString(), @"^[1-9]+\d*$"))
            {
                wathRepeat.Text = wathRepeat.Tag.ToString();
                wathRepeat.Select(wathRepeat.Text.Length, 0);
            }
            else
            {
                wathRepeat.Tag = wathRepeat.Text;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = System.IO.Path.GetDirectoryName(myOptions[Options.File]);
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DestinationFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (DestinationFolderTextBox.Text.Trim() == "" || !Directory.Exists(DestinationFolderTextBox.Text))
            {
                System.Windows.MessageBox.Show(g.dict["MessSpecifyDestinationFolder"].ToString(), g.dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                DestinationFolderTextBox.Focus();
                return;
            }

            //freezeStatusControls.Add(.Name,.IsEnabled);

            string DestinationFolderTextBoxText = DestinationFolderTextBox.Text;
            ProgressBarSplitFile.Value = 0;
            if (myOptions[Options.Mode].ToLower() == "size")
                new SplitFileOfSizeDelegate(g.FileManager.SplitFile).BeginInvoke(this, DestinationFolderTextBox.Text, Convert.ToInt64(myOptions[Options.extMode]), everyCut.IsChecked.Value, Convert.ToInt32(wathRepeat.Text), delegate { g.OpenFolder(DestinationFolderTextBoxText); }, null);
            else
            {
                byte[] dataSearch;
                if (((ucSplit)this.Tag).currentFormatResult.ToLower() == "hex")
                    dataSearch = SplitAndJoinFile.HexToByte(myOptions[Options.extMode]);
                else
                    dataSearch = SplitAndJoinFile.StringToByte(myOptions[Options.extMode]);
                new SplitFileOfDataDelegate(g.FileManager.SplitFile).BeginInvoke(this, DestinationFolderTextBox.Text, dataSearch, everyCut.IsChecked.Value, Convert.ToInt32(wathRepeat.Text), delegate { g.OpenFolder(DestinationFolderTextBoxText); }, null);
            }

        }

        private void wathRepeat_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!wathRepeat.IsEnabled)
                return;
            if (e.Delta > 0)
                wathRepeat.Text = (Convert.ToInt16(wathRepeat.Text) + 1).ToString();
            else
                wathRepeat.Text = (Convert.ToInt16(wathRepeat.Text) - 1).ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void ProgressBarSplitFile_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ProgressBarSplitFile.Value > 0 && ProgressBarSplitFile.Value < ProgressBarSplitFile.Maximum && ProgressBarSplitFile.Tag == null)
            {
                ProgressBarSplitFile.Tag = new Dictionary<string, bool>();
                ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag)).Add(DestinationButtonSelect.Name, DestinationButtonSelect.IsEnabled);
                DestinationButtonSelect.IsEnabled = false;
                ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag)).Add(everyCut.Name, everyCut.IsEnabled);
                everyCut.IsEnabled = false;
                ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag)).Add(wathRepeat.Name, wathRepeat.IsEnabled);
                wathRepeat.IsEnabled = false;
                ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag)).Add(CanselButton.Name, CanselButton.IsEnabled);
                CanselButton.IsEnabled = false;
                ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag)).Add(StartButton.Name, StartButton.IsEnabled);
                StartButton.IsEnabled = false;
            }
            else if (ProgressBarSplitFile.Value == ProgressBarSplitFile.Maximum)
            {
                Dictionary<string, bool> freeze = ((Dictionary<string, bool>)(ProgressBarSplitFile.Tag));
                DestinationButtonSelect.IsEnabled = freeze[DestinationButtonSelect.Name];
                everyCut.IsEnabled = freeze[everyCut.Name];
                wathRepeat.IsEnabled = freeze[wathRepeat.Name];
                CanselButton.IsEnabled = freeze[CanselButton.Name];
                StartButton.IsEnabled = freeze[StartButton.Name];
                ProgressBarSplitFile.Tag = null;
            }
        }
    }
}
