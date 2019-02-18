using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;

namespace File_Split_and_Join
{

    /// <summary>
    /// Логика взаимодействия для SplitUC.xaml
    /// </summary>
    public partial class ucSplit : UserControl
    {
        public string currentFormatResult = "";


        public ucSplit()
        {
            InitializeComponent();
        }

        public void Set_lng()
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(g.dict);
        }

        public string DisplayFormat
        {
            set
            {
                foreach (MenuItem li in dislayFormatsMenuItem.Items)
                {
                    if (li.Tag != null && li.Tag.ToString() == value)
                    {
                        SelectFormat_Click(li, new RoutedEventArgs());
                        return;
                    }
                    if (li.Items.Count > 0)
                    {
                        foreach (MenuItem li2 in li.Items)
                        {
                            if (li2.Tag != null && li2.Tag.ToString() == value)
                            {
                                SelectFormat_Click(li2, new RoutedEventArgs());
                                return;
                            }
                        }
                    }
                }
                SelectFormat_Click(defaultDisplayFormat, null);
            }
            get
            {
                foreach (MenuItem li in dislayFormatsMenuItem.Items)
                {
                    if (li.BorderBrush != null)
                        return li.Tag.ToString();
                    if (li.Items.Count > 0)
                    {
                        foreach (MenuItem li2 in li.Items)
                        {
                            if (li2.BorderBrush != null)
                                return li2.Tag.ToString();
                        }
                    }
                }
                return defaultDisplayFormat.Tag.ToString();
            }
        }

        public string CachSize
        {
            get
            {
                return CacheSizeMenuItem.Tag.ToString();
            }

            set
            {
                CacheSizeMenuItem.Tag = value;
            }
        }

        public void SelectModeSplit(object sender, RoutedEventArgs e)
        {
            if (SliderSizeManage == null || CutTextBox == null)
                return;
            if (isModeOfSize.IsChecked.Value)
            {
                SliderSizeManage.Visibility = System.Windows.Visibility.Visible;
                SlideFileInfo.Visibility = System.Windows.Visibility.Visible;
                CutTextBox.Visibility = System.Windows.Visibility.Hidden;
                MenuItemFindPointCutText.IsEnabled = false;
            }
            else
            {
                SliderSizeManage.Visibility = System.Windows.Visibility.Hidden;
                SlideFileInfo.Visibility = System.Windows.Visibility.Hidden;
                CutTextBox.Visibility = System.Windows.Visibility.Visible;
                MenuItemFindPointCutText.IsEnabled = true;
            }
        }

        private void MenuItemFindPointCutText_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isModeOfSize.IsChecked.Value || CutTextBox.Text.Trim() == "" || SliderSizeManage.Maximum == 0)
            {
                MenuItemFindPointCutText.IsEnabled = false;
            }
            else
            {
                MenuItemFindPointCutText.IsEnabled = true;
            }
        }

        private void SelectFormat_Click(object sender, RoutedEventArgs e)
        {
            MenuItem Item = (MenuItem)sender;
            if (e != null)
            {
                currentFormatResult = Item.Tag.ToString();
                new ModifyRegistry().Write("display_format", currentFormatResult);
            }
            Item.Background = Brushes.PaleGoldenrod;
            Item.BorderBrush = Brushes.Red;
            MenuItem ParentItem = (MenuItem)Item.Parent;
            if (ParentItem.Parent is MenuItem)
            {
                SelectFormat_Click(ParentItem, null);
            }
            foreach (MenuItem li in ((MenuItem)Item.Parent).Items)
            {
                if (Item.Header.ToString() != li.Header.ToString())
                {
                    li.Background = Brushes.White;
                    li.BorderBrush = null;
                    foreach (MenuItem subLi in li.Items)
                    {
                        subLi.Background = Brushes.White;
                        subLi.BorderBrush = null;
                    }
                }
            }
            SliderFile_ValueChanged(null, null);
        }

        private void SliderSizeManage_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (Object obj in SliderSizeManage.ContextMenu.Items)
            {
                MenuItem li;
                if (obj is MenuItem && ((MenuItem)obj).Tag != null)
                    li = (MenuItem)obj;
                else
                    continue;
                li.IsEnabled = Convert.ToDouble(li.Tag) < SliderSizeManage.Maximum;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            // Set filter options and filter index.
            openFileDialog1.Filter = "All Files (*.*)|*.*|Text Files (.txt)|*.txt";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                PathFile.Text = openFileDialog1.FileName;
                new ModifyRegistry().Write("last_read_file", PathFile.Text.Trim());
                g.FileManager = new SplitAndJoinFile();
                g.FileManager.OpenFile(openFileDialog1.FileName);
                SliderSizeManage.Maximum = g.FileManager.Length;
                SliderSizeManage.Value = 0;
                SliderFile_ValueChanged(null, null);
            }
        }

        public void SliderFile_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (g.FileManager == null)
                return;
            double percent = SliderSizeManage.Value / (SliderSizeManage.Maximum / 100);
            SlideFileInfo.Content = SliderSizeManage.Value.ToString() + "/" + SliderSizeManage.Maximum.ToString() + " byte(s) = " + (SliderSizeManage.Value < 1024 ? "0.0" : (Math.Round(SliderSizeManage.Value / 1048576, 2)).ToString()) + "/" + (Math.Round(SliderSizeManage.Maximum / 1048576, 2)).ToString() + " MB" + " [" + String.Format("{0:F6}", Math.Round(percent, 6)) + "%" + "]";
            //
            g.FileManager.Position = (long)SliderSizeManage.Value;
            Dictionary<ReadingDirection, byte[]> showData = g.FileManager.ReadDataAboutPosition((long)SliderSizeManage.Value, g.CacheSize, currentFormatResult);
            string StartData;
            string EndData;

            switch (currentFormatResult.ToLower())
            {
                case "utf8":
                    StartData = System.Text.Encoding.UTF8.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.UTF8.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "ascii":
                    StartData = System.Text.Encoding.ASCII.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.ASCII.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "unicode":
                    StartData = System.Text.Encoding.Unicode.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.Unicode.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "bigendianunicode":
                    StartData = System.Text.Encoding.BigEndianUnicode.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.BigEndianUnicode.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "default":
                    StartData = System.Text.Encoding.Default.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.Default.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "utf32":
                    StartData = System.Text.Encoding.UTF32.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.UTF32.GetString(showData[ReadingDirection.Rifht]);
                    break;
                case "utf7":
                    StartData = System.Text.Encoding.UTF7.GetString(showData[ReadingDirection.Left]);
                    EndData = System.Text.Encoding.UTF7.GetString(showData[ReadingDirection.Rifht]);
                    break;
                default:
                    StartData = BitConverter.ToString(showData[ReadingDirection.Left]);
                    EndData = "-" + BitConverter.ToString(showData[ReadingDirection.Rifht]);
                    break;
            }

            //
            ResultTextBox.Document.Blocks.Clear();
            TextRange rangeStart = new TextRange(ResultTextBox.Document.ContentEnd, ResultTextBox.Document.ContentEnd);
            rangeStart.Text = StartData;
            rangeStart.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Indigo);
            

            
            //ResultTextBox.ScrollToVerticalOffset(ResultTextBox.VerticalOffset);

            TextRange rangeEnd = new TextRange(ResultTextBox.Document.ContentEnd, ResultTextBox.Document.ContentEnd);
            rangeEnd.Text = EndData;
            rangeEnd.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            if (e == null)
                return;
        }

        public void CacheSizeMenuItem_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e != null)
            {
                if (e.Delta > 0)
                    CacheSizeMenuItem.Tag = (Convert.ToDouble(CacheSizeMenuItem.Tag) + 1).ToString();
                else
                    CacheSizeMenuItem.Tag = (Convert.ToDouble(CacheSizeMenuItem.Tag) - 1).ToString();
                //
                if (CacheSizeMenuItem.Tag.ToString() == "549")
                    CacheSizeMenuItem.Tag = "550";
                if (CacheSizeMenuItem.Tag.ToString() == "5556")
                    CacheSizeMenuItem.Tag = "5555";
            }
            CacheSizeMenuItem.Header = g.dict["txtBufferByteSize"].ToString() + ": " + CacheSizeMenuItem.Tag.ToString();
            SliderFile_ValueChanged(null, null);
        }

        private void ResultTextBox_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            new ModifyRegistry().Write("cach_size", CacheSizeMenuItem.Tag.ToString());
            g.CacheSize = Convert.ToInt32(CacheSizeMenuItem.Tag.ToString());
            SliderFile_ValueChanged(null, null);
        }

        private void SizeSliderSlectPosition(object sender, RoutedEventArgs e)
        {
            SliderSizeManage.Value = Convert.ToInt64(((MenuItem)sender).Tag.ToString());
        }

        private void Button_Click_StartSplit(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(PathFile.Text.Trim()))
            {
                MessageBox.Show(g.dict["txtFileNotExist"].ToString(), g.dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfSize.IsChecked.Value && (SliderSizeManage.Value <= 0 || SliderSizeManage.Value == SliderSizeManage.Maximum))
            {
                MessageBox.Show(g.dict["txtSelectCorrectFileSize"].ToString(), g.dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfText.IsChecked.Value && CutTextBox.Text == "")
            {
                MessageBox.Show(g.dict["txtEnterTextCutFile"].ToString(), g.dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfText.IsChecked.Value)
                SliderSizeManage.Value = 0;
            //(currentFormatResult.ToLower() == "hex"?SplitAndJoinFile.StringToHEX(CutTextBox.Text):CutTextBox.Text)
            //System.Text.RegularExpressions.Regex my_reg_ex = new System.Text.RegularExpressions.Regex(@"");
            if (isModeOfText.IsChecked.Value && currentFormatResult.ToLower() == "hex" && !new System.Text.RegularExpressions.Regex(@"^(\w\w-)+\w\w$").IsMatch(CutTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show(string.Format(this.Resources.MergedDictionaries[0]["txtErrHEXTextCutFile"].ToString().Replace("\\n",Environment.NewLine), CutTextBox.Text, SplitAndJoinFile.StringToHEX(CutTextBox.Text)), "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;
                CutTextBox.Text = SplitAndJoinFile.StringToHEX(CutTextBox.Text);
            }
            Dictionary<Options, string> options = new Dictionary<Options, string>() 
            {{ Options.File, PathFile.Text },
            {Options.Mode, (isModeOfSize.IsChecked.Value ? "Size" : "Text")},
            {Options.extMode, (isModeOfSize.IsChecked.Value ? SliderSizeManage.Value.ToString() : CutTextBox.Text)}};
            FinalSplitWin doSplitForm = new FinalSplitWin(options);
            //doSplitForm.Tag = options;
            doSplitForm.Owner = Window.GetWindow(this);
            doSplitForm.Tag = this;
            doSplitForm.ShowDialog();
        }

        private void MenuItemFindPointCutText_Click(object sender, RoutedEventArgs e)
        {
            byte[] searchData;
            if (currentFormatResult == "HEX")
            {
                searchData = SplitAndJoinFile.HexToByte(CutTextBox.Text);
            }
            else
            {
                searchData = SplitAndJoinFile.StringToByte(CutTextBox.Text);
            }
            SliderSizeManage.Value++;
            g.FileManager.FindData(searchData, (long)SliderSizeManage.Value, g.myStartWin);
        }

        private void PathFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PathFile.Text.Trim() == "")
                Button_Click(null, null);
        }

        private void SliderSizeManage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SliderSizeManage.Value += e.Delta;
        }
    }
}
