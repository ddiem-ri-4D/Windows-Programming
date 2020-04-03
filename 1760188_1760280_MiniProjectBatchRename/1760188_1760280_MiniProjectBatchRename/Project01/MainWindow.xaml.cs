using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Xml;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Project01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	public class Item
    {
        public string filename { get; set; }
        public string newfilename { get; set; }
        public string path { get; set; }
        public string error { get; set; }
    }
    public partial class MainWindow : Window
    {
        string program_path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        HashSet<string> external_preset_path = new HashSet<string>();
        private string path_files;
        private string[] file_extension;
        private string[] name_files;
        private string[] newname_files;
        private string[] error_files;
        int len_files;
        private string path_folders;
        private string[] name_folders;
        private string[] newname_folders;
        private string[] error_folders;
        int len_folders;
        int flag_tab = -1;
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        public MainWindow()
        {
            InitializeComponent();
            Newcase_Selector.SelectedIndex = 0;
            Move_Selector.SelectedIndex = 0;
            loadPresets();
        } 
        private void StartBatchButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult;
            if (ReplaceCheckBox.IsChecked == false && newCaseCheckBox.IsChecked == false &&
            moveCheckBox.IsChecked == false && UniqueNameCheckBox.IsChecked == false)
            {
                messageBoxResult = System.Windows.MessageBox.Show(
                            "This action can't undo.\n" +
                            "You have not selected action or file!",
                            "Please select action and file!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                bool checkError = false;
                for (int i = 0; i < len_files; i++)
                {
                    if (error_files[i] != null)
                        checkError = true;
                }
                messageBoxResult = MessageBoxResult.OK;
                if (checkError == true)
                {

                    messageBoxResult = System.Windows.MessageBox.Show(
                            "This action can't undo.\n" +
                            "Some files/folders maybe duplicated and this action will add a number to the suffix!",
                            "Be Careful!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                }
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    previousName();
                    return;
                }
                rename_action();
                update();
                add();
            }
        }
        
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (len_files > 0)
            {
                Array.Clear(name_files, 0, len_files);
                Array.Clear(newname_files, 0, len_files);
                Array.Clear(error_files, 0, len_files);
            }
            if (len_folders > 0)
            {
                Array.Clear(name_folders, 0, len_folders);
                Array.Clear(newname_folders, 0, len_folders);
                Array.Clear(error_folders, 0, len_folders);
            }
            len_files = 0;
            len_folders = 0;
            path_files = "";
            path_folders = "";
            ReplaceCheckBox.IsChecked = false;
            newCaseCheckBox.IsChecked = false;
            moveCheckBox.IsChecked = false;
            UniqueNameCheckBox.IsChecked = false;
            FindTextBox.Text = "";
            ReplaceTextBox.Text = "";
            flag_tab = 0;
            update();
            flag_tab = 1;
            update();
            flag_tab = tab.SelectedIndex;
        }
       
        private void Save_Presets_Bnt_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML File|*.xml";
            saveFileDialog.Title = "Save an Presets File";
            saveFileDialog.InitialDirectory = program_path + @"\Presets";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                savepreset(saveFileDialog.FileName);
                loadPresets();
            }
        }
        private void Open_Presets_Bnt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML File|*.xml";
            openFileDialog.Title = "Open/Load an Presets File";
            openFileDialog.InitialDirectory = program_path + @"\Presets";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                string soucefile = Path.GetDirectoryName(openFileDialog.FileName);
                loadpreset(openFileDialog.FileName);
                if (soucefile != program_path + @"\Presets")
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(
                        "This preset is not belong to Default Presets Folder\n" +
                        "Do you want copy this preset to Default Presets Folder?",
                        "Copy Preset Confirmation",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (messageBoxResult == MessageBoxResult.No)
                        external_preset_path.Add(openFileDialog.FileName);
                    else
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "XML File|*.xml";
                        saveFileDialog.Title = "Set name for Presets File";
                        saveFileDialog.InitialDirectory = program_path + @"\Presets";
                        saveFileDialog.FileName = Path.GetFileName(openFileDialog.FileName);
                        saveFileDialog.RestoreDirectory = true;
                        saveFileDialog.ShowDialog();
                        if (saveFileDialog.FileName != "")
                        {
                            File.Copy(soucefile, program_path + @"\Presets");
                        }
                    }
                }
                loadPresets();
                Presets.SelectedIndex = Presets.Items.Count - 1;
            }
        }
       
        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            flag_tab = 0;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                add();
            }
        }
        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            flag_tab = 1;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                add();
            }
        }

      
        private void ApplyReplaceButton_Click(object sender, RoutedEventArgs e)
        {

            if (FindTextBox.Text == "" || ReplaceTextBox.Text == "")
            {
                MessageBox.Show("Find field or Replace field can not be empty", "Error!",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ReplaceCheckBox.IsChecked = true;
                UniqueNameCheckBox.IsChecked = false;
                ReplaceExpander.IsExpanded = false;
                replaceName();
                update();
            }
        }
        private void ApplyNewCaseButton_Click(object sender, RoutedEventArgs e)
        {
            newCaseCheckBox.IsChecked = true;
            newCaseExpander.IsExpanded = false;
            UniqueNameCheckBox.IsChecked = false;
            newcaseName();
            update();
        }
        private void ApplyMoveButton_Click(object sender, RoutedEventArgs e)
        {
            moveCheckBox.IsChecked = true;
            moveExpander.IsExpanded = false;
            UniqueNameCheckBox.IsChecked = false;
            moveISBN();
            update();
        }
        private void ApplyUniqueNameButton_Click(object sender, RoutedEventArgs e)
        {
            UniqueNameCheckBox.IsChecked = true;
            uniqueNameExpander.IsExpanded = false;
            ReplaceCheckBox.IsChecked = false;
            newCaseCheckBox.IsChecked = false;
            moveCheckBox.IsChecked = false;
            guid();
            update();
        }
        
        private void ReplaceCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (FindTextBox.Text == "" || ReplaceTextBox.Text == "")
            {
                MessageBox.Show("Find field or Replace field can not be empty", "Error!",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                ReplaceCheckBox.IsChecked = false;
                return;
            }
            ReplaceExpander.IsExpanded = true;
            replaceName();
            update();
        }
        private void NewCaseCheckBox_Click(object sender, RoutedEventArgs e)
        {
            newCaseExpander.IsExpanded = true;
            newcaseName();
            update();

        }
        private void MoveCheckBox_Click(object sender, RoutedEventArgs e)
        {
            moveExpander.IsExpanded = true;
            moveISBN();
            update();
        }
        private void UniqueNameCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ApplyUniqueNameButton_Click(sender, e);
        }

       
        private void rename_action()
        {
            if (flag_tab == 0)
            {

                for (int i = 0; i < len_files; i++)
                {
                    string oldpath = path_files + @"\" + name_files[i];
                    string newpath = path_files + @"\";
                    if (error_files[i] == null)
                    {
                        newpath += newname_files[i];
                    }
                    else
                    {
                        newpath += Path.GetFileNameWithoutExtension(newname_files[i]) + " " + error_files[i].Substring(0, 1) + Path.GetExtension(newname_files[i]);
                    }
                    bool samename = false;
                    if (oldpath.ToLower().Equals(newpath.ToLower()))
                    {
                        samename = true;
                        newpath += "._temp_.";
                    }
                    File.Move(oldpath, newpath);
                    if (samename)
                    {
                        File.Move(newpath, newpath.Substring(0, newpath.Length - 8));
                    }

                }

            }
            else
            {
                for (int i = 0; i < len_folders; i++)
                {
                    string oldpath = path_folders + @"\" + name_folders[i]; ;
                    string newpath = path_folders + @"\" + newname_folders[i];
                    if (error_folders[i] != null)
                    {
                        newpath += error_folders[i].Substring(0, 1);
                    }
                    bool samename = false;
                    if (oldpath.ToLower().Equals(newpath.ToLower()))
                    {
                        samename = true;
                        newpath += "._temp_.";
                    }
                    Directory.Move(oldpath, newpath);
                    if (samename)
                    {
                        string abc = newpath.Substring(0, newpath.Length - 8);
                        Directory.Move(newpath, abc);
                    }
                }
            }
        }
        private void update()
        {
            error_check();
            List<Item> Item = new List<Item>();
            if (flag_tab == 0)
            {
                // fileListView.Items.Clear();
                for (int i = 0; i < len_files; i++)
                {
                    Item.Add(new Item { filename = name_files[i], newfilename = newname_files[i], path = path_files, error = error_files[i] });
                }
                fileListView.ItemsSource = Item;
            }
            else
            {
                //folderListView.Items.Clear();
                for (int i = 0; i < len_folders; i++)
                {
                    Item.Add(new Item { filename = name_folders[i], newfilename = newname_folders[i], path = path_folders, error = error_folders[i] });
                }
                folderListView.ItemsSource = Item;
            }

        }
        private void error_check()
        {
            if (flag_tab == 0)
            {
                if (len_files == 0) return;
                Dictionary<string, int> duplicate = new Dictionary<string, int>();
                string[] folders_name = Directory.GetDirectories(path_files);
                for (int i = 0; i < len_files; i++)
                {
                    for (int j = 0; j < folders_name.Count(); j++)
                    {
                        if (newname_files[i].Equals(folders_name[j]))
                        {
                            if (!duplicate.ContainsKey(newname_files[i]))
                            {
                                duplicate.Add(newname_files[i], 1);
                                error_files[i] = "1 - File name already exists";
                            }
                            else
                            {
                                duplicate[newname_files[i]]++;
                                error_files[i] = duplicate[newname_files[i]].ToString() + " - File name already exists";
                            }
                        }
                    }
                    for (int j = 0; j < len_files; j++)
                    {
                        if (i == j) continue;
                        if (newname_files[i].Equals(newname_files[j]))
                        {
                            if (!duplicate.ContainsKey(newname_files[i]))
                            {
                                duplicate.Add(newname_files[i], 1);
                                error_files[i] = "1 - File name already exists";
                            }
                            else
                            {
                                duplicate[newname_files[i]]++;
                                error_files[i] = duplicate[newname_files[i]].ToString() + " - File name already exists";
                            }
                        }
                    }
                }
            }
            else
            {
                if (len_folders == 0) return;
                Dictionary<string, int> duplicate = new Dictionary<string, int>();
                string[] files_name = Directory.GetFiles(path_folders);
                for (int i = 0; i < len_folders; i++)
                {
                    for (int j = 0; j < files_name.Count(); j++)
                    {
                        if (newname_folders[i].Equals(files_name[j]))
                        {
                            if (!duplicate.ContainsKey(newname_folders[i]))
                            {
                                duplicate.Add(newname_folders[i], 1);
                                error_files[i] = "1 - Folder name already exists";
                            }
                            else
                            {
                                duplicate[newname_folders[i]]++;
                                error_files[i] = duplicate[newname_folders[i]].ToString() + " - Folder name already exists";
                            }
                        }
                    }
                    for (int j = 0; j < len_folders; j++)
                    {
                        if (i == j) continue;
                        if (newname_folders[i].Equals(newname_folders[j]))
                        {
                            if (!duplicate.ContainsKey(newname_folders[i]))
                            {
                                duplicate.Add(newname_folders[i], 1);
                                error_files[i] = "1 - Folder name already exists";
                            }
                            else
                            {
                                duplicate[newname_folders[i]]++;
                                error_files[i] = duplicate[newname_folders[i]].ToString() + " - Folder name already exists";
                            }
                        }
                    }
                }
            }
        }
        private void previousName()
        {
            if (flag_tab == 0)
            {
                for (int i = 0; i < len_files; i++)
                {
                    newname_files[i] = name_files[i];
                }
            }
            else
            {
                for (int i = 0; i < len_folders; i++)
                {
                    newname_folders[i] = name_folders[i];
                }
            }
            update();
        }
        private void savepreset(string Path)
        {
            XmlWriter writer = null;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;
            writer = XmlWriter.Create(Path, settings);
            writer.WriteStartElement("Presets");
            /*------------------------*/
            writer.WriteStartElement("Replace", ReplaceCheckBox.IsChecked.ToString());
            writer.WriteElementString("Find_With", FindTextBox.Text);
            writer.WriteElementString("Replace_With", ReplaceTextBox.Text);
            writer.WriteEndElement();

            /*------------------------*/
            writer.WriteStartElement("NewCase", newCaseCheckBox.IsChecked.ToString());
            writer.WriteElementString("Select", Newcase_Selector.SelectedIndex.ToString());
            writer.WriteEndElement();

            /*------------------------*/
            writer.WriteStartElement("Move", moveCheckBox.IsChecked.ToString());
            writer.WriteElementString("Select", Move_Selector.SelectedIndex.ToString());
            writer.WriteEndElement();

            /*------------------------*/
            writer.WriteStartElement("Unique", UniqueNameCheckBox.IsChecked.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.Flush();
            writer.Close();
        }
        private void loadpreset(string Path)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(Path);
            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.Name == "Replace")
                {
                    if (node.NamespaceURI == "True")
                    {
                        FindTextBox.Text = node.FirstChild.InnerText;
                        ReplaceTextBox.Text = node.LastChild.InnerText;
                        ReplaceCheckBox.IsChecked = true;
                    }
                }
                else if (node.Name == "NewCase")
                {
                    if (node.NamespaceURI == "True")
                    {
                        newCaseCheckBox.IsChecked = true;
                        Newcase_Selector.SelectedIndex = Convert.ToInt32(node.InnerText);
                    }
                }
                else if (node.Name == "Move")
                {
                    if (node.NamespaceURI == "True")
                    {
                        moveCheckBox.IsChecked = true;
                        Move_Selector.SelectedIndex = Convert.ToInt32(node.InnerText);

                    }
                }
                else if (node.Name == "Unique")
                {
                    if (node.NamespaceURI == "True")
                    {
                        UniqueNameCheckBox.IsChecked = true;
                    }
                }
            }
        }
        private void loadPresets()
        {
            if (!Directory.Exists("Presets"))
            {
                Directory.CreateDirectory("Presets");
            }
            Presets.Items.Clear();
            string[] preset = Directory.GetFiles(program_path + @"\Presets");
            foreach (var x in preset)
                Presets.Items.Add(Path.GetFileName(x));
            foreach (var x in external_preset_path)
                Presets.Items.Add(x);
        }

        
        private void add()
        {
            if (flag_tab == 0)
            {
                // fileListView.Items.Clear();
                path_files = fbd.SelectedPath;
                string[] files = Directory.GetFiles(path_files);
                len_files = files.Length;
                name_files = new string[len_files];
                newname_files = new string[len_files];
                error_files = new string[len_files];
                file_extension = new string[len_files];
                for (int i = 0; i < len_files; i++)
                {
                    name_files[i] = Path.GetFileName(files[i]);
                    file_extension[i] = Path.GetExtension(name_files[i]);
                    newname_files[i] = name_files[i];
                }
            }
            if (flag_tab == 1)
            {
                //fileListView.Items.Clear();
                path_folders = fbd.SelectedPath;
                string[] folders = Directory.GetDirectories(path_folders);
                int len_path = path_folders.Length;
                len_folders = folders.Length;
                name_folders = new string[len_folders];
                newname_folders = new string[len_folders];
                error_folders = new string[len_folders];
                for (int i = 0; i < len_folders; i++)
                {
                    string _temp = Path.GetFileName(folders[i]);
                    name_folders[i] = _temp;
                    newname_folders[i] = name_folders[i];
                }
            }
            update();
        }

        
        private void replaceName()
        {
            string _find = FindTextBox.Text;
            string _replace = ReplaceTextBox.Text;
            if (flag_tab == 0)
            {

                for (int i = 0; i < len_files; i++)
                {
                    newname_files[i] = name_files[i].Replace(_find, _replace);
                }
            }
            else
            {
                for (int i = 0; i < len_folders; i++)
                {
                    newname_folders[i] = name_folders[i].Replace(_find, _replace);
                }
            }
        }
        private void newcaseName()
        {
            string UppercaseWords(string value)
            {
                value = value.Trim();
                value = value.ToLower();
                value = Regex.Replace(value, " {2,}", " ");

                char[] array = value.ToCharArray();
                if (array.Length >= 1)
                {
                    if (char.IsLower(array[0]))
                    {
                        array[0] = char.ToUpper(array[0]);
                    }
                }
                for (int i = 1; i < array.Length; i++)
                {
                    if (array[i - 1] == ' ')
                    {
                        if (char.IsLower(array[i]))
                        {
                            array[i] = char.ToUpper(array[i]);
                        }
                    }
                }
                return new string(array);
            }
            string UppercaseFisrtLetter(string value)
            {
                value = value.ToLower();
                if (value == null)
                    return null;

                if (value.Length > 1)
                    return char.ToUpper(value[0]) + value.Substring(1);

                return value.ToUpper();
            }
            if (flag_tab == 0)
            {
                string extension;
                if (Newcase_Selector.SelectedIndex == 0)
                {
                    for (int i = 0; i < len_files; i++)
                    {
                        newname_files[i] = Path.GetFileNameWithoutExtension(newname_files[i]);
                        newname_files[i] = newname_files[i].ToUpper() + file_extension[i];
                    }
                }
                else if (Newcase_Selector.SelectedIndex == 1)
                {
                    for (int i = 0; i < len_files; i++)
                    {
                        extension = Path.GetExtension(newname_files[i]);
                        newname_files[i] = Path.GetFileNameWithoutExtension(newname_files[i]);
                        newname_files[i] = newname_files[i].ToLower() + file_extension[i];
                    }
                }
                else if (Newcase_Selector.SelectedIndex == 2)
                {
                    for (int i = 0; i < len_files; i++)
                    {
                        extension = Path.GetExtension(newname_files[i]);
                        newname_files[i] = Path.GetFileNameWithoutExtension(newname_files[i]);
                        newname_files[i] = UppercaseFisrtLetter(newname_files[i]) + file_extension[i];
                    }
                }
                else
                {
                    for (int i = 0; i < len_files; i++)
                        newname_files[i] = UppercaseWords(newname_files[i]);
                }
            }
            else
            {
                if (Newcase_Selector.SelectedIndex == 0)
                {
                    for (int i = 0; i < len_folders; i++)                      
                        newname_folders[i] = newname_folders[i].ToUpper();
                }
                else if (Newcase_Selector.SelectedIndex == 1)
                {
                    for (int i = 0; i < len_folders; i++)   
                        newname_folders[i] = newname_folders[i].ToLower();
                }
                else if (Newcase_Selector.SelectedIndex == 2)
                {
                    for (int i = 0; i < len_folders; i++)
                    {
                        newname_folders[i] = UppercaseFisrtLetter(newname_folders[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < len_folders; i++)                       
                        newname_folders[i] = UppercaseWords(newname_folders[i]);
                }
            }
        }
        private void moveISBN()
        {
            string[] splitIBSN(string name)
            {
                Regex rx = new Regex(@"(97(?:8|9)([ -]?)(?=\d{1,5}\2?\d{1,7}\2?\d{1,6}\2?\d)(?:\d\2*){9}\d)");
                Match m = rx.Match(name);
                if (!m.Success)
                    return null;
                var ISBN = m.Groups[0].Value;
                var x_name = rx.Replace(name, "");
                x_name = x_name.Trim();
                x_name = Regex.Replace(x_name, " {2,}", " ");
                return new string[] { ISBN, x_name };
            }
            for (int i = 0; i < len_files; i++)
            {

                var splited_name = splitIBSN(newname_files[i]);
                if (splited_name == null) continue;
                if (Move_Selector.SelectedIndex == 0) newname_files[i] = splited_name[0] + " " + splited_name[1].Replace(" " + file_extension[i], file_extension[i]);
                else newname_files[i] = splited_name[1].Replace(file_extension[i], " " + splited_name[0]) + file_extension[i];
            }
        }
        private void guid()
        {
            Guid obj;
            if (flag_tab == 0)
            {
                for (int i = 0; i < len_files; i++)
                {
                    obj = Guid.NewGuid();
                    newname_files[i] = obj.ToString() + file_extension[i];
                }
            }
            else
            {
                for (int i = 0; i < len_folders; i++)
                {
                    obj = Guid.NewGuid();
                    newname_folders[i] = obj.ToString();
                }
            }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (UniqueNameCheckBox.IsChecked == true)
            {
                ApplyUniqueNameButton_Click(sender, e);
            }
            else
            {
                if (flag_tab == 0)
                {
                    if (len_files == 0) return;
                    newname_files = name_files.Clone() as string[];
                    Array.Clear(error_files, 0, len_files);
                    error_files = new string[len_files];
                }
                else
                {
                    if (len_folders == 0) return;
                    newname_folders = name_folders.Clone() as string[];
                    Array.Clear(error_folders, 0, len_folders);
                    error_folders = new string[len_folders];
                }
                if (ReplaceCheckBox.IsChecked == true)
                    ApplyReplaceButton_Click(sender, e);
                if (newCaseCheckBox.IsChecked == true)
                    NewCaseCheckBox_Click(sender, e);
                if (moveCheckBox.IsChecked == true)
                    ApplyMoveButton_Click(sender, e);
            }
            update();
        }
        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                File.Delete(program_path + @"\Presets\" + Presets.SelectedItem);
                Presets.Items.Remove(Presets.SelectedItem);
            }
        }
        private void Presets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Presets.Items.Count == 0) return;
            string name = Presets.SelectedItem.ToString();
            string _path = program_path + @"\Presets\" + name;
            if (File.Exists(_path))
                loadpreset(_path);
            else
            {
                _path = name;
                if (File.Exists(_path))
                    loadpreset(_path);
            }
        }
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            flag_tab = tab.SelectedIndex;
            if (flag_tab == 0)
                moveExpander.Visibility = Visibility.Visible;
            else moveExpander.Visibility = Visibility.Collapsed;
        }
    }
}
