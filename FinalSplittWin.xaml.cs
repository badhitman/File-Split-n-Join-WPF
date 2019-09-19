////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using DataFileScanerLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace FileSplitAndJoinWPF
{
    public enum Options { File, Mode, extMode };
    /// <summary>
    /// Логика взаимодействия для FinalSplitWin.xaml
    /// </summary>
    public partial class FinalSplitWin : Window
    {
        public delegate void SplitFileOfSizeDelegate(string destFolder, long size, int repeatEvery = 1);
        //public SplitFileOfSizeDelegate SplitFileOfSizeDelegateObj;

        public delegate void SplitFileOfDataDelegate(string destFolder, byte[][] dataSearch, int repeatEvery = 1);
        //public SplitFileOfDataDelegate SplitFileOfDataDelegateObj;

        private Dictionary<Options, string> myOptions = new Dictionary<Options, string>();

        public FinalSplitWin()
        {
            InitializeComponent();
            Set_lng();
        }

        public void Set_lng()
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(g.dict);
        }

        public FinalSplitWin(Dictionary<Options, string> inOptions)
        {
            InitializeComponent();
            Set_lng();
            myOptions = inOptions;
            Title += " - " + myOptions[Options.File];
            LabelSplitMode.Content += " " + myOptions[Options.Mode];
            LabelSplitOf.Content += " " + myOptions[Options.extMode];
        }

        /// <summary>
        /// Событие изменения размера компановки.
        /// </summary>
        private void wathRepeat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(dimension_group.Text.ToString(), @"^[1-9]+\d*$"))
            {
                dimension_group.Text = dimension_group.Tag.ToString();
                dimension_group.Select(dimension_group.Text.Length, 0);
            }
            else
            {
                dimension_group.Tag = dimension_group.Text;
            }
        }

        /// <summary>
        /// Событие выбора каталога назначения/сохранения результата нарезки
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = Path.GetDirectoryName(myOptions[Options.File]);
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DestinationFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// запуск нарезки файла
        /// </summary>
        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            if (DestinationFolderTextBox.Text.Trim() == "" || !Directory.Exists(DestinationFolderTextBox.Text))
            {
                System.Windows.MessageBox.Show(g.dict["MessSpecifyDestinationFolder"].ToString(), g.dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                DestinationFolderTextBox.Focus();
                return;
            }

            string DestinationFolderTextBoxText = DestinationFolderTextBox.Text;
            ProgressBarSplitFile.Value = 0;
            if (myOptions[Options.Mode].ToLower() == "size")
                new SplitFileOfSizeDelegate(g.FileManager.SplitFile).BeginInvoke(DestinationFolderTextBox.Text, Convert.ToInt64(myOptions[Options.extMode]), Convert.ToInt32(dimension_group.Text), delegate { g.OpenFolder(DestinationFolderTextBoxText); }, null);
            else
            {
                byte[][] dataSearch;
                if (((ucSplit)this.Tag).currentFormatResult.ToLower() == "hex")
                    dataSearch = FileScanner.HexToByte(myOptions[Options.extMode]);
                else
                    dataSearch = FileScanner.StringToSearchBytes(myOptions[Options.extMode]);

                g.FileManager.ProgressValueChange += FileManager_ProgressValueChange;
                new SplitFileOfDataDelegate(g.FileManager.SplitFile).BeginInvoke(DestinationFolderTextBox.Text, dataSearch, Convert.ToInt32(dimension_group.Text), delegate { g.OpenFolder(DestinationFolderTextBoxText); }, null);
            }

        }

        /// <summary>
        /// Обработчик события хода выполнения обработки файла
        /// </summary>
        /// <param name="percentage">Процент выполнения в диапазоне 0-100</param>
        private void FileManager_ProgressValueChange(int percentage)
        {
            ProgressBarSplitFile.Value = percentage;
            TaskbarItemInfo.ProgressValue = (double)percentage / (double)100;

            if (TaskbarItemInfo.ProgressValue == 100)
                g.FileManager.ProgressValueChange -= FileManager_ProgressValueChange;
        }

        /// <summary>
        /// событие прокрутки колёсика мышки для увеличения или уменьшения размерности компановки
        /// </summary>
        private void wathRepeat_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!dimension_group.IsEnabled)
                return;

            if (e.Delta > 0)
                dimension_group.Text = (Convert.ToInt16(dimension_group.Text) + 1).ToString();
            else
                dimension_group.Text = (Convert.ToInt16(dimension_group.Text) - 1).ToString();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => g.FileManager.ProgressValueChange -= FileManager_ProgressValueChange;

        private void ProgressBarSplitFile_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ProgressBarSplitFile.Value > 0 && ProgressBarSplitFile.Value < ProgressBarSplitFile.Maximum && ProgressBarSplitFile.Tag == null)
            {
                /////////////////////////////////////////////////////////////
                // Сохраняем состояние элементов формы
                Dictionary<string, bool> save_state_form = new Dictionary<string, bool>();
                save_state_form.Add(DestinationButtonSelect.Name, DestinationButtonSelect.IsEnabled);
                DestinationButtonSelect.IsEnabled = false;
                save_state_form.Add(dimension_group.Name, dimension_group.IsEnabled);
                dimension_group.IsEnabled = false;
                save_state_form.Add(CanselButton.Name, CanselButton.IsEnabled);
                CanselButton.IsEnabled = false;
                save_state_form.Add(StartButton.Name, StartButton.IsEnabled);
                ProgressBarSplitFile.Tag = save_state_form;
                StartButton.IsEnabled = false;
            }
            else if (ProgressBarSplitFile.Value == ProgressBarSplitFile.Maximum)
            {
                /////////////////////////////////////////////////////////////
                // Восстанавливаем состояние элементов формы
                Dictionary<string, bool> restore_state_form = (Dictionary<string, bool>)ProgressBarSplitFile.Tag;
                DestinationButtonSelect.IsEnabled = restore_state_form[DestinationButtonSelect.Name];
                dimension_group.IsEnabled = restore_state_form[dimension_group.Name];
                CanselButton.IsEnabled = restore_state_form[CanselButton.Name];
                StartButton.IsEnabled = restore_state_form[StartButton.Name];
                ProgressBarSplitFile.Tag = null;
            }
        }
    }
}
