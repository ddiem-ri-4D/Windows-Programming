using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MiniProject_MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for SavePlayListWinDow.xaml
    /// </summary>
    public partial class SavePlayListWinDow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        public SavePlayListWinDow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            if (namePlaylistTextBox.Text != "")
            {
                saveButton.IsEnabled = true;
            }
            else
            {
                saveButton.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {         
            namePlaylistTextBox.Text = "Untitled playlist";
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkSameName(namePlaylistTextBox.Text + ".xml") == true)
            {
                System.Windows.MessageBox.Show("Tên đã tồn tại, vui lòng đặt tên khác!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow)._savePlaylist = true;
                ((MainWindow)Application.Current.MainWindow)._namePlaylist = namePlaylistTextBox.Text;

                this.Close();
            }
        }

        private bool checkSameName(string name)
        {
            for (int i = 0; i < ((MainWindow)Application.Current.MainWindow).playlists.Count; i++)
            {
                if (((MainWindow)Application.Current.MainWindow).playlists[i].fileName == name)
                {
                    return true;
                }
            }
            return false;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

     
    }
}
