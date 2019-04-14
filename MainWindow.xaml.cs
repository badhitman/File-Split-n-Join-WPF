////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using FileSplitAndJoinWin32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileSplitAndJoinWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ucJoin my_ucJoin = new ucJoin();
        private ucSplit my_ucSplit = new ucSplit();
        private ucMD5 my_ucMD5 = new ucMD5();

        public MainWindow()
        {
            InitializeComponent();
            g.myStartWin = this;
            //
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            this.Title += " ver." + version + " beta";

            ////////////////////////////////////////////////////////////
            //
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
            //ci.ThreeLetterISOLanguageName
            g.last_lang_programm = new ModifyRegistry().Read("last_lang");
            if (g.last_lang_programm == null || g.last_lang_programm == "")
            {
                g.last_lang_programm = "eng";
                foreach (MenuItem li in lngs_MenuItem.Items)
                {
                    if (li.Name.Substring(0, 3) == ci.ThreeLetterISOLanguageName)
                    {
                        g.last_lang_programm = ci.ThreeLetterISOLanguageName;
                        break;
                    }
                }
            }

            foreach (MenuItem li in lngs_MenuItem.Items)
            {
                if (li.Name == g.last_lang_programm + "_MenuItem")
                {
                    li.IsChecked = true;
                }
            }
            Set_lng(g.last_lang_programm);

            ////////////////////////////////////////////////////////////
            //
            string latest_mode_programm = new ModifyRegistry().Read("last_mode_programm", "");
            if (latest_mode_programm == "")
                latest_mode_programm = menu_item_Split.Name;
            if (latest_mode_programm == menu_item_Split.Name)
                select_mode(menu_item_Split, null);
            else if (latest_mode_programm == menu_item_Join.Name)
                select_mode(menu_item_Join, null);
            else
                select_mode(menu_item_MD5, null);

            ////////////////////////////////////////////////////////////
            //
            my_ucSplit.CachSize = new ModifyRegistry().Read("cach_size", "1024");
            if (my_ucSplit.CachSize == null)
                my_ucSplit.CachSize = "1024";
            my_ucSplit.CacheSizeMenuItem_MouseWheel(null, null);
            g.CacheSize = Convert.ToInt32(my_ucSplit.CachSize);

            ////////////////////////////////////////////////////////////
            //
            my_ucSplit.DisplayFormat = new ModifyRegistry().Read("DISPLAY_FORMAT");

        }

        private void Set_lng(string lng)
        {
            g.dict = new ResourceDictionary();
            try
            {
                // Do not initialize this variable here.
                g.dict.Source = new Uri("..\\Resources\\StringResources-" + lng + ".xaml", UriKind.Relative);
            }
            catch
            {
                g.dict.Source = new Uri("..\\Resources\\StringResources-eng.xaml", UriKind.Relative);
            }
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(g.dict);
            my_ucSplit.Set_lng();
            my_ucJoin.Set_lng();
            my_ucMD5.Set_lng();
        }

        private void b_SnJ_Click(object sender, RoutedEventArgs e)
        {
            int childAmount = VisualTreeHelper.GetChildrenCount((sender as System.Windows.Controls.Primitives.ToggleButton).Parent);
            System.Windows.Controls.Primitives.ToggleButton tb;
            for (int i = 0; i < childAmount; i++)
            {
                tb = null;
                tb = VisualTreeHelper.GetChild((sender as System.Windows.Controls.Primitives.ToggleButton).Parent, i) as System.Windows.Controls.Primitives.ToggleButton;

                if (tb != null)
                    tb.IsChecked = false;
            }

            tb = (sender as System.Windows.Controls.Primitives.ToggleButton);
            tb.IsChecked = true;
            if (e != null)
            {
                if (tb.Content.ToString() == menu_item_Split.Header.ToString())
                {
                    select_mode(menu_item_Split, null);
                }
                else if (tb.Content.ToString() == menu_item_Join.Header.ToString())
                {
                    select_mode(menu_item_Join, null);
                }
                else
                {
                    select_mode(menu_item_MD5, null);
                }
            }
        }

        private void select_Checkable(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            foreach (MenuItem li in ((MenuItem)item.Parent).Items)
            {
                li.IsChecked = false;
            }
            item.IsChecked = true;
            if (item.Tag != null && item.Tag.ToString() == "lng")
                Set_lng(item.Name.Substring(0, 3));
        }

        private void gotourl(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((MenuItem)sender).Tag.ToString());
        }

        private void select_mode(object sender, RoutedEventArgs e)
        {
            testSP.Children.Clear();
            MenuItem item = ((MenuItem)sender);
            if (item.Header.ToString() == b_Join.Content.ToString())
            {
                b_Join.IsChecked = true;
                b_Join.Focus();
                //
                b_Split.IsChecked = false;
                b_MD5.IsChecked = false;
                //
                b_SnJ_Click(b_Join, null);
                testSP.Children.Add(my_ucJoin);
            }
            else if (item.Header.ToString() == b_Split.Content.ToString())
            {
                b_Split.IsChecked = true;
                b_Split.Focus();
                //
                b_Join.IsChecked = false;
                b_MD5.IsChecked = false;
                //
                b_SnJ_Click(b_Split, null);
                //
                my_ucSplit.SelectModeSplit(null, null);
                testSP.Children.Add(my_ucSplit);
            }
            else
            {
                b_MD5.IsChecked = true;
                b_MD5.Focus();
                //
                b_Join.IsChecked = false;
                b_Split.IsChecked = false;
                //
                b_SnJ_Click(b_MD5, null);
                //
                my_ucSplit.SelectModeSplit(null, null);
                testSP.Children.Add(my_ucMD5);
            }
            select_Checkable(sender, null);
            new ModifyRegistry().Write("last_mode_programm", item.Name);
        }

        private void CloseProgramm(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StartEndData_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TextData_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void lngs_MenuItem_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            foreach (MenuItem li in lngs_MenuItem.Items)
            {
                if (li.IsChecked)
                {
                    new ModifyRegistry().Write("last_lang", li.Name.Substring(0, 3));
                    g.last_lang_programm = li.Name.Substring(0, 3);
                }
            }
        }

        private void MenuItem_Click_About(object sender, RoutedEventArgs e)
        {
            AboutWin wind = new AboutWin();
            wind.ShowDialog();
        }
    }
}
