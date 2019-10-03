////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using FileSplitAndJoinWin32;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TextFileScanerLib;
using TextFileScanerLib.Matches;
using TextFileScanerLib.scan.matches;

namespace FileSplitAndJoinWPF
{

    /// <summary>
    /// Логика взаимодействия для SplitUC.xaml
    /// </summary>
    public partial class UcSplit : UserControl
    {
        public string currentFormatResult { get; set; } = "";

        public UcSplit()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Установить язык интерфейса
        /// </summary>
        public void SetLng()
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(g.Dict);
        }

        /// <summary>
        /// Установка формата отображения и вызов события клика мышкой по соответсвующему пункту контекстного меню.
        /// </summary>
        public string DisplayFormat
        {
            set
            {
                foreach (MenuItem li in dislayFormatsMenuItem.Items)
                {
                    if (value == li.Tag?.ToString())
                    {
                        SelectFormat_Click(li, new RoutedEventArgs());
                        return;
                    }
                    if (li.Items.Count > 0)
                    {
                        foreach (MenuItem li2 in li.Items)
                        {
                            if (li2.Tag?.ToString() == value)
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

        /// <summary>
        /// Событие переключения режима нарезкий файла: [по размеру]/[по тексту]
        /// </summary>
        public void SelectModeSplit(object sender, RoutedEventArgs e)
        {
            if (SliderSizeManage == null || CutTextBox == null)
                return;
            if (isModeOfSize.IsChecked.Value)
            {
                SliderSizeManage.Visibility = Visibility.Visible;
                SlideFileInfo.Visibility = Visibility.Visible;
                CutTextBox.Visibility = Visibility.Hidden;
                MenuItemFindPointCutText.IsEnabled = false;
            }
            else
            {
                SliderSizeManage.Visibility = Visibility.Hidden;
                SlideFileInfo.Visibility = Visibility.Hidden;
                CutTextBox.Visibility = Visibility.Visible;
                MenuItemFindPointCutText.IsEnabled = true;
            }
        }

        /// <summary>
        /// Событие скрытия/отображения поля для ввода строки нарезкий файла.
        /// Что бы отключать/включать пункт контекстного меню "найти следующее вхождение"
        /// </summary>
        private void MenuItemFindPointCutText_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isModeOfSize.IsChecked.Value || CutTextBox.Text.Trim().Length==0 || SliderSizeManage.Maximum == 0)
                MenuItemFindPointCutText.IsEnabled = false;
            else
                MenuItemFindPointCutText.IsEnabled = true;
        }

        /// <summary>
        /// Событие выбора формата отображение из пункта контекстного меню
        /// </summary>
        private void SelectFormat_Click(object sender, RoutedEventArgs e)
        {
            MenuItem Item = (MenuItem)sender;
            if (e != null)
            {
                currentFormatResult = Item.Tag?.ToString();
                new ModifyRegistry().Write("display_format", currentFormatResult);
                AdapterFileReader.SetEncoding(currentFormatResult);
            }
            Item.Background = Brushes.PaleGoldenrod;
            Item.BorderBrush = Brushes.Red;
            MenuItem ParentItem = (MenuItem)Item.Parent;
            if (ParentItem.Parent is MenuItem)
                SelectFormat_Click(ParentItem, null);

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
            SliderFileValueChanged(null, null);
        }

        /// <summary>
        /// Событие открытия контекстного меню ползунка позиционирования курсора в файле
        /// </summary>
        private void SliderSizeManage_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            foreach (object obj in SliderSizeManage.ContextMenu.Items)
            {
                MenuItem li;
                if (obj is MenuItem && ((MenuItem)obj).Tag != null)
                    li = (MenuItem)obj;
                else
                    continue;

                li.IsEnabled = Convert.ToDouble(li.Tag, CultureInfo.CurrentCulture) < SliderSizeManage.Maximum;
            }
        }

        /// <summary>
        /// событие выбора исходного файла
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                // Set filter options and filter index.
                Filter = "All Files (*.*)|*.*|Text Files (.txt)|*.txt",
                FilterIndex = 1,
                CheckPathExists = true,
                Multiselect = false
            };

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                PathFile.Text = openFileDialog1.FileName;
                new ModifyRegistry().Write("last_read_file", PathFile.Text.Trim());
                AdapterFileReader.SetEncoding(currentFormatResult);
                g.FileManager.OpenFile(openFileDialog1.FileName);
                SliderSizeManage.Maximum = g.FileManager.Length;
                SliderSizeManage.Value = 0;
                SliderFileValueChanged(null, null);
            }
        }

        /// <summary>
        /// Событие перемещения ползунка изменения позиции курсора в файле
        /// </summary>
        public void SliderFileValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(sender is null || e is null))
                SliderSizeManage.Tag = null;

            if (g.FileManager is null)
                return;

            double percent = SliderSizeManage.Value / (SliderSizeManage.Maximum / 100);
            SlideFileInfo.Content = SliderSizeManage.Value.ToString(CultureInfo.CurrentCulture) + "/" + SliderSizeManage.Maximum.ToString(CultureInfo.CurrentCulture) + " byte(s) = " + (SliderSizeManage.Value < 1024 ? "0.0" : (Math.Round(SliderSizeManage.Value / 1048576, 2)).ToString(CultureInfo.CurrentCulture)) + "/" + (Math.Round(SliderSizeManage.Maximum / 1048576, 2)).ToString(CultureInfo.CurrentCulture) + " MB" + " [" + String.Format(CultureInfo.CurrentCulture,"{0:F6}", Math.Round(percent, 6)) + "%" + "]";
            //
            g.FileManager.Position = (long)SliderSizeManage.Value;
            Dictionary<ReadingDirection, byte[]> showData = g.FileManager.ReadDataAboutPosition((long)SliderSizeManage.Value, g.CacheSize);

            string StartData;
            string EndData;
            switch (currentFormatResult?.ToLower(CultureInfo.CurrentCulture))
            {
                case "hex":
                    StartData = AdapterFileReader.BytesToHEX(showData[ReadingDirection.Left]);
                    EndData = AdapterFileReader.BytesToHEX(showData[ReadingDirection.Rifht]);
                    break;
                case "base64":
                    StartData = Convert.ToBase64String(showData[ReadingDirection.Left]);
                    EndData = Convert.ToBase64String(showData[ReadingDirection.Rifht]);
                    break;
                default:
                    StartData = AdapterFileReader.EncodingMode.GetString(showData[ReadingDirection.Left]);
                    EndData = AdapterFileReader.EncodingMode.GetString(showData[ReadingDirection.Rifht]);
                    break;
            }
            //

            ResultTextBox.Document.Blocks.Clear();
            TextRange rangeStart = new TextRange(ResultTextBox.Document.ContentEnd, ResultTextBox.Document.ContentEnd)
            {
                Text = StartData
            };
            rangeStart.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Indigo);

            TextRange rangeEnd = new TextRange(ResultTextBox.Document.ContentEnd, ResultTextBox.Document.ContentEnd)
            {
                Text = EndData
            };
            rangeEnd.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
            if (e == null)
                return;
        }

        public void CacheSizeMenuItemMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e != null)
            {
                if (e.Delta > 0)
                    CacheSizeMenuItem.Tag = (Convert.ToDouble(CacheSizeMenuItem.Tag, CultureInfo.CurrentCulture) + 100).ToString(CultureInfo.CurrentCulture);
                else
                    CacheSizeMenuItem.Tag = (Convert.ToDouble(CacheSizeMenuItem.Tag, CultureInfo.CurrentCulture) - 100).ToString(CultureInfo.CurrentCulture);
                //
            }
            CacheSizeMenuItem.Header = g.Dict["txtBufferByteSize"].ToString() + ": " + CacheSizeMenuItem.Tag.ToString();
            SliderFileValueChanged(null, null);
        }

        private void ResultTextBox_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            new ModifyRegistry().Write("cach_size", CacheSizeMenuItem.Tag.ToString());
            g.CacheSize = Convert.ToInt32(CacheSizeMenuItem.Tag.ToString(), CultureInfo.CurrentCulture);
            SliderFileValueChanged(null, null);
        }

        private void SizeSliderSelectPosition(object sender, RoutedEventArgs e)
        {
            SliderSizeManage.Value = Convert.ToInt64(((MenuItem)sender).Tag.ToString(), CultureInfo.CurrentCulture);
        }

        private void Button_Click_StartSplit(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(PathFile.Text.Trim()))
            {
                MessageBox.Show(g.Dict["txtFileNotExist"].ToString(), g.Dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfSize.IsChecked.Value && (SliderSizeManage.Value <= 0 || SliderSizeManage.Value == SliderSizeManage.Maximum))
            {
                MessageBox.Show(g.Dict["txtSelectCorrectFileSize"].ToString(), g.Dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfText.IsChecked.Value && CutTextBox.Text.Length == 0)
            {
                MessageBox.Show(g.Dict["txtEnterTextCutFile"].ToString(), g.Dict["MessError"].ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (isModeOfText.IsChecked.Value)
                SliderSizeManage.Value = 0;

            if (isModeOfText.IsChecked.Value && currentFormatResult.ToLower(CultureInfo.CurrentCulture) == "hex" && !new System.Text.RegularExpressions.Regex(@"^(\w\w-)+\w\w$").IsMatch(CutTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show(string.Format(CultureInfo.CurrentCulture, this.Resources.MergedDictionaries[0]["txtErrHEXTextCutFile"].ToString().Replace("\\n", Environment.NewLine), CutTextBox.Text, AdapterFileReader.StringToHEX(CutTextBox.Text)), "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    return;
                CutTextBox.Text = AdapterFileReader.StringToHEX(CutTextBox.Text);
            }
            Dictionary<OptionsEnum, string> options = new Dictionary<OptionsEnum, string>()
            {{ OptionsEnum.File, PathFile.Text },
            {OptionsEnum.Mode, (isModeOfSize.IsChecked.Value ? "Size" : "Text")},
            {OptionsEnum.extMode, (isModeOfSize.IsChecked.Value ? SliderSizeManage.Value.ToString(CultureInfo.CurrentCulture) : CutTextBox.Text)}};
            FinalSplitWin doSplitForm = new FinalSplitWin(options)
            {
                //
                Owner = Window.GetWindow(this),
                Tag = this
            };
            doSplitForm.ShowDialog();
        }

        /// <summary>
        /// Событие [найти следующее вхождение в файле]
        /// </summary>
        private void MenuItemFindPointCutText_Click(object sender, RoutedEventArgs e)
        {
            g.FileManager.Scanner.ClearMatchUnits();

            if (currentFormatResult.ToLower(CultureInfo.CurrentCulture) == "hex")
                g.FileManager.Scanner.AddMatchUnit(new MatchUnitBytes(AdapterFileReader.HexToByte(CutTextBox.Text)));
            else
                g.FileManager.Scanner.AddMatchUnit(new MatchUnitText(CutTextBox.Text, false)); //searchData = AdapterFileReader.StringToSearchBytes(CutTextBox.Text);

            long index_detect_find_data;
            if (SliderSizeManage.Tag is null)
            {
                index_detect_find_data = g.FileManager.FindPositionData((long)SliderSizeManage.Value);
            }
            else
            {
                index_detect_find_data = (long)SliderSizeManage.Tag;
                index_detect_find_data = g.FileManager.FindPositionData(index_detect_find_data + g.FileManager.Scanner.ScanResult.MatchUnit.GetDetectedSearchData().Length);
            }

            if (index_detect_find_data < 0 || index_detect_find_data + g.FileManager.Scanner.MinDataLengthBytes >= g.FileManager.Length)
                MessageBox.Show("End of file", "End of file", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            else
            {
                SliderSizeManage.Value = index_detect_find_data;

                if (index_detect_find_data > -1)
                    SliderSizeManage.Tag = index_detect_find_data;
            }
        }

        /// <summary>
        /// Событие двойного клика мышкой по полю пути к исходному файлу чтения. Если файл ещё не выбран, то зарускается событие выбора файла
        /// </summary>
        private void PathFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PathFile.Text.Trim().Length == 0)
                Button_Click(null, null);
        }

        /// <summary>
        /// Событие прокручивания колёсика мышки для ползунка изменения курсора в файле
        /// </summary>
        private void SliderSizeManage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SliderSizeManage.Value += e.Delta * 5;
        }
    }
}
