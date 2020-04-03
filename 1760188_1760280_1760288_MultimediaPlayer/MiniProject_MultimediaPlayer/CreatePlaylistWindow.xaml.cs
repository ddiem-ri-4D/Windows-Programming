using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Collections.ObjectModel;

namespace MiniProject_MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for CreatePlaylistWindow.xaml
    /// </summary>
    public partial class CreatePlaylistWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        public CreatePlaylistWindow()
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
                createButton.IsEnabled = true;
            }
            else
            {
                createButton.IsEnabled = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            namePlaylistTextBox.Text = "Untitled playlist";
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkSameName(namePlaylistTextBox.Text + ".xml") == true)
            {
                System.Windows.MessageBox.Show("Tên đã tồn tại, vui lòng đặt tên khác!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                ((MainWindow)Application.Current.MainWindow)._namePlaylist = namePlaylistTextBox.Text;
                ((MainWindow)Application.Current.MainWindow)._createPlaylist = true;
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
