////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using FileSplitAndJoinWin32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace FileSplitAndJoinWPF
{
    /// <summary>
    /// Логика взаимодействия для JoinUC.xaml
    /// </summary>
    public partial class ucJoin : UserControl
    {
        public ucJoin()
        {
            InitializeComponent();
        }

        public void Set_lng()
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(g.dict);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            string initial_directory_join_file = new ModifyRegistry().Read("initial_directory_join_file");
            if (initial_directory_join_file != null)
                openFileDialog.InitialDirectory = initial_directory_join_file;
            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            if (openFileDialog.FileNames.Length == 0)
                return;
            new ModifyRegistry().Write("initial_directory_join_file", System.IO.Path.GetDirectoryName(openFileDialog.FileNames[0]));
            foreach (string s in openFileDialog.FileNames)
                AddFile(s, new FileInfo(s).Length.ToString());
        }

        private void AddFile(string name, string size)
        {
            foreach (FileForJoin li in ListViewFiles.Items)
            {
                if (li.Name == name && MessageBox.Show("The file [" + name + "] already exists in the list. Add a duplicate?", "Duplicate " + name, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    return;
            }

            GridView gridView = new GridView();
            ListViewFiles.View = gridView;
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                DisplayMemberBinding = new Binding("Name")
            });
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Size",
                DisplayMemberBinding = new Binding("Size")
            });
            
            ListViewFiles.Items.Add(new FileForJoin() { Name = name, Size = size });
        }

        private void ListViewFiles_Drop(object sender, DragEventArgs e)
        {
            foreach (string s in e.Data.GetData(DataFormats.FileDrop, true) as string[])
                AddFile(s, new FileInfo(s).Length.ToString());
        }

        private void ListViewFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void CheckPossibilityEnabledMove(MenuItem itemMenu)
        {
            string headerItem = itemMenu.Header.ToString();
            string stringCommand = headerItem.Substring(headerItem.Length - 1, 1);
            if (itemMenu.Tag.ToString() == "↑")
                MenuItemMoveUp.IsEnabled = !ListViewFiles.SelectedItems.Contains(ListViewFiles.Items[0]);
            if (itemMenu.Tag.ToString() == "↓")
                MenuItemMoveDown.IsEnabled = !ListViewFiles.SelectedItems.Contains(ListViewFiles.Items[ListViewFiles.Items.Count - 1]);
        }

        private void MenuItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (ListViewFiles.SelectedItems.Count > 0)
            {
                item.IsEnabled = true;
                if (item.Name == MenuItemMoveDown.Name || item.Name == MenuItemMoveUp.Name)
                    CheckPossibilityEnabledMove(item);
            }
            else
                item.IsEnabled = false;
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            int totalCounSelectedItems = ListViewFiles.SelectedItems.Count;
            if (ListViewFiles.SelectedItems.Count == 0)
                return;
            if (ListViewFiles.SelectedItems.Count == ListViewFiles.Items.Count)
            {
                ListViewFiles.Items.Clear();
                return;
            }
            int selectedIndex = ListViewFiles.SelectedIndex;
            //
            Stack<FileForJoin> selectedItems = new Stack<FileForJoin>();
            foreach (FileForJoin item in ListViewFiles.SelectedItems)
                selectedItems.Push(item);
            foreach (FileForJoin item in selectedItems)
                ListViewFiles.Items.Remove(item);
            if (selectedIndex < ListViewFiles.Items.Count && totalCounSelectedItems == 1)
                ListViewFiles.SelectedIndex = selectedIndex;
            else if (ListViewFiles.Items.Count > 0 && totalCounSelectedItems == 1)
                ListViewFiles.SelectedIndex = ListViewFiles.Items.Count - 1;
        }

        private void ListViewFiles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && ListViewFiles.SelectedItems.Count > 0)
                MenuItemDelete_Click(null, null);
        }

        private void MenuItemMoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewFiles.SelectedItems.Count == 0 || ListViewFiles.SelectedItems.Count == ListViewFiles.Items.Count)
                return;

            MenuItem itemMenu = (MenuItem)sender;
            string headerItem = itemMenu.Header.ToString();
            string stringCommand = headerItem.Substring(headerItem.Length - 1, 1);
            //
            List<FileForJoin> selectedItems = new List<FileForJoin>();
            foreach (FileForJoin item in ListViewFiles.SelectedItems)
                selectedItems.Add(item);
            //
            List<int> indexesMove = new List<int>();
            int IndexItem = 0;
            foreach (FileForJoin item in ListViewFiles.Items)
            {
                if (selectedItems.Contains(item))
                    indexesMove.Add(IndexItem);
                IndexItem++;
            }
            selectedItems = null;
            ListViewFiles.SelectedItems.Clear();

            indexesMove.Sort();
            if (stringCommand == "↓")
                indexesMove.Reverse();
            foreach (int i in indexesMove)
            {
                var itemToMoveUp = ListViewFiles.Items[i];
                ListViewFiles.Items.RemoveAt(i);
                ListViewFiles.Items.Insert(i + (stringCommand == "↑" ? -1 : 1), itemToMoveUp);
                ListViewFiles.SelectedItems.Add(ListViewFiles.Items[i + (stringCommand == "↑" ? -1 : 1)]);

            }
        }

        private void MenuItem_IsVisibleChangedMenuItem(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((MenuItem)sender).IsEnabled = ListViewFiles.Items.Count > 0;
        }

        private void Button_Click_StartJoin(object sender, RoutedEventArgs e)
        {
            if (ListViewFiles.Items.Count == 0)
                return;
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog1.RestoreDirectory = true;
            
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = new string[ListViewFiles.Items.Count];
                int index = 0;
                foreach (FileForJoin item in ListViewFiles.Items)
                    files[index++] = item.Name;

                FileManager.FileWriter.JoinFiles(files, saveFileDialog1.FileName);
                g.OpenFolder(Path.GetDirectoryName(saveFileDialog1.FileName));
            }
        }
    }

    public class FileForJoin
    {
        public string Name { get; set; }
        public string Size { get; set; }
    }
}
