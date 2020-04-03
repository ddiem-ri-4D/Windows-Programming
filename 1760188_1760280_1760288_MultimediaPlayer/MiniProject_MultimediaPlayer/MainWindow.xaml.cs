using Gma.System.MouseKeyHook;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace MiniProject_MultimediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<MusicItem> playlist = new ObservableCollection<MusicItem>();
        public ObservableCollection<PlaylistItem> playlists = new ObservableCollection<PlaylistItem>();
        MediaPlayer _player = new MediaPlayer();
        DispatcherTimer _timer = new DispatcherTimer();
        private int _locationMusic = -1; // vị trí nhạc đang phát
        private int _locationPlaylist = -1; // vị trí playlist đang phát
        //private bool _mediaPlayerIsPlaying = false;
        private bool _userIsDraggingSlider = false;
        private IKeyboardMouseEvents _hook;
        private int _numberOfPlays = 0;
        private bool _playlistIsPlay = false;

        // đường dẫn đến thư mục lưu Playlist
        private string _playlistPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Playlists\";
        public bool _savePlaylist = false; // trạng thái save playlist
        public bool _createPlaylist = false; // trạng thái create playlist
        public string _namePlaylist; // tên playlist

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window Load And Close

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;

            subcriber();
            playlistMusicListView.ItemsSource = playlist; // Truyền giá trị (là music) cho ListView
            playlistLibaryListView.ItemsSource = playlists; // Truyền giá trị (là playlist) cho ListView
            deleteMusicButton.Visibility = Visibility.Collapsed; // Ẩn Button Delete music
            selectAllButton.Visibility = Visibility.Collapsed; // Ẩn nút Button Select All Music
            deletePlaylist.Visibility = Visibility.Collapsed; // Ẩn Button Delete Playlist
            loadPlaylist();
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            if ((_player.Source != null) &&
                _player.NaturalDuration.HasTimeSpan == true &&
                _userIsDraggingSlider == false && playlist.Count != 0)
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = _player.Position.TotalSeconds;
                musicNaturalDurationTextBlock.Text = String.Format(_player.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                nameMusicTextblock.Text = playlist[_locationMusic].fileName;
                _player.MediaEnded += Player_MediaEnded;
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            unSubcriber();

        }
        #endregion

        #region Play, Pause Music And Next, Previous Music With "Key Hook"

        public void subcriber()
        {
            _hook = Hook.GlobalEvents();
            _hook.KeyUp += _hook_KeyUp;
        }
        public void unSubcriber()
        {
            _hook.KeyUp -= _hook_KeyUp;
            _hook.Dispose();
        }
        private void _hook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (e.Control && (e.KeyCode == System.Windows.Forms.Keys.P))
            {
                Play();
            }
            else if (e.Control && e.Alt && (e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                next();
            }
            else if (e.Control && e.Alt && (e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                previous();
            }
        }

        #endregion

        #region Add Music To Playlist And Choose Music To Play

        private void openFileMusicButton_Click(object sender, RoutedEventArgs e)
        {

            var screen = new OpenFileDialog();
            screen.Title = "Choose Music";
            screen.Filter = " MP3 | *.mp3";
            screen.Multiselect = true;

            if (screen.ShowDialog() == true)
            {
                foreach (var path in screen.FileNames)
                {
                    var item = new MusicItem();
                    item.filePath = path;
                    item.fileName = Path.GetFileName(item.filePath);
                    if (checkSameMusic(item.fileName) == false)
                    {
                        playlist.Add(item);

                        if (_playlistIsPlay == true)
                        {
                            updateAfterAdd();
                            savePlaylistButton.IsEnabled = false;
                        }
                        else
                        {
                            savePlaylistButton.IsEnabled = true;
                        }
                    }
                }
            }
        }
        private bool checkSameMusic(string name)
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                if (name == playlist[i].fileName)
                {
                    return true;
                }
            }
            return false;
        }

        //Chọn Music để play
        private void listViewItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            _locationMusic = playlistMusicListView.SelectedIndex;
            playMusic();
            if (_playlistIsPlay == true)
            {
                updatePlayPosition();
            }
        }
        #endregion

        #region Slider Progress

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            _player.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            musicProgressStatusTextBlock.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"mm\:ss");
        }
        #endregion

        #region Phát Một Lần Hoặc Vô Tận Và Phát Tuần Tự Hoặc Ngẫu Nhiên

        private void repeatMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (repeatMusic.Kind.ToString() == "RepeatOnce")
            {
                repeatMusic.Kind = PackIconKind.Repeat;
                repeatMusicButton.ToolTip = "Phát Vô Tận";
            }
            else
            {
                repeatMusic.Kind = PackIconKind.RepeatOnce;
                repeatMusicButton.ToolTip = "Phát Một Lần";
            }

        }
        private void shuffleMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleMusic.Kind.ToString() == "ShuffleDisabled")
            {
                shuffleMusic.Kind = PackIconKind.ShuffleVariant;
                shuffleMusicButton.ToolTip = "Phát Ngẫu Nhiên";
            }
            else
            {
                shuffleMusic.Kind = PackIconKind.ShuffleDisabled;
                shuffleMusicButton.ToolTip = "Phát Tuần Tự";
            }
        }
        private void Player_MediaEnded(object sender, EventArgs e) // bắt sự kiện khi Music kết thúc
        {
            if (repeatMusic.Kind == PackIconKind.RepeatOnce) // phát lặp lại vô tận
            {
                if (shuffleMusic.Kind == PackIconKind.ShuffleDisabled)
                {
                    random();
                    playlistMusicListView.SelectedIndex = _locationMusic;
                } // phát ngẫu nhiên
                else
                {
                    sequentially();
                    playlistMusicListView.SelectedIndex = _locationMusic;
                } // phát tuần tự
            }
            else // phát lặp lại 1 lần
            {
                if (shuffleMusic.Kind == PackIconKind.ShuffleDisabled)
                {
                    if (_numberOfPlays >= playlist.Count - 1)
                    {
                        _locationMusic = 0;
                        playlistMusicListView.SelectedIndex = _locationMusic;
                        _player.Stop();
                        play.Kind = PackIconKind.Play;
                        _numberOfPlays = 0;
                    }
                    else
                    {
                        random();
                        playlistMusicListView.SelectedIndex = _locationMusic;
                        _numberOfPlays++;
                    }
                } // phát ngẫu nhiên
                else
                {
                    if (_locationMusic == playlist.Count - 1)
                    {
                        sequentially();
                        playlistMusicListView.SelectedIndex = _locationMusic;
                        _player.Stop();
                        play.Kind = PackIconKind.Play;
                    }
                    else
                    {
                        sequentially();
                        playlistMusicListView.SelectedIndex = _locationMusic;
                    }
                } // phát tuần tự              
            }

            updatePlayPosition();
        }
        private void random()
        {
            Random rd = new Random();
            int ird;
            do
            {
                ird = rd.Next(playlist.Count);
            } while (ird == _locationMusic);
            _locationMusic = ird;
        }
        private void sequentially()
        {
            if (_locationMusic == playlist.Count - 1)
            {
                _locationMusic = 0;
            }
            else
            {
                _locationMusic++;
            }
        }
        #endregion

        #region Next And Previous Music

        private void previousMusicButton_Click(object sender, RoutedEventArgs e)
        {
            previous();
            //updatePlayPosition();
        }
        private void nextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            next();
            //updatePlayPosition();
        }
        private void next()
        {
            if (_locationMusic == playlist.Count - 1 && _locationMusic != -1)
            {
                playlistMusicListView.SelectedIndex = 0;
            }
            else if (_locationMusic != -1)
            {
                playlistMusicListView.SelectedIndex = _locationMusic + 1;
            }
        }
        private void previous()
        {
            if (_locationMusic == 0)
            {
                playlistMusicListView.SelectedIndex = playlist.Count - 1;
            }
            else if (_locationMusic != -1)
            {

                playlistMusicListView.SelectedIndex = _locationMusic - 1;
            }
        }
        #endregion

        #region Play, Pause And Stop Music

        private void playMusicButton_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }
        private void stopMusicButton_Click(object sender, RoutedEventArgs e)
        {
            _player.Stop();
            play.Kind = PackIconKind.Play;
        }
        private void Play()
        {
            if (play.Kind == PackIconKind.Play)
            {
                play.Kind = PackIconKind.Pause;

                _player.Play();
            }
            else
            {
                play.Kind = PackIconKind.Play;
                _player.Pause();
            }
        }
        private void playMusic()
        {
            if (playlist.Count != 0)
            {
                string path = playlist[_locationMusic].filePath;
                _player.Open(new Uri(path));
                _player.Play();
                _timer.Start();
                play.Kind = PackIconKind.Pause;
                playButton.IsEnabled = true;
                playlistMusicListView.SelectedIndex = _locationMusic;
            }
        }
        #endregion

        #region Delete Music And Select All Music

        private void deleteMusicButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                if (playlist[i].isChecked == true)
                {
                    if (i != _locationMusic)
                    {
                        if (i < _locationMusic)
                        {
                            _locationMusic--;
                        }
                        playlist.RemoveAt(i);
                        if (playlist.Count == 0)
                        {
                            savePlaylistButton.IsEnabled = false;
                        }
                    }
                    else
                    {
                        if (playlist.Count == 1)
                        {
                            _locationMusic = -1;
                            playlist.RemoveAt(i);
                            savePlaylistButton.IsEnabled = false;
                            defaultStatus();
                        }
                        else
                        {
                            if (i == playlist.Count - 1)
                            {
                                _locationMusic--;
                                playMusic();
                                playlist.RemoveAt(i);
                            }
                            else
                            {
                                _locationMusic++;
                                playMusic();
                                playlist.RemoveAt(i);
                                _locationMusic = i;
                            }
                        }
                    }

                    updateAfterDelete(i);
                    i--;
                }
            }
            CheckBoxChanged(sender, e);

        }
        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (checkNotSelected_Music() == false)
            {
                deleteMusicButton.Visibility = Visibility.Visible;
                selectAllButton.Visibility = Visibility.Visible;
            }
            else
            {
                deleteMusicButton.Visibility = Visibility.Collapsed;
                selectAllButton.Visibility = Visibility.Collapsed;
            }
        }
        private bool checkNotSelected_Music()
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                if (playlist[i].isChecked == true)
                {
                    return false;
                }
            }
            return true;
        }
        private bool checkSelected_Music()
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                if (playlist[i].isChecked == false)
                {
                    return false;
                }
            }
            return true;
        }
        private void defaultStatus()
        {
            _player.Stop();
            _player.Close();
            _locationMusic = -1;
            nameMusicTextblock.Text = "Untitled Music";
            musicProgressStatusTextBlock.Text = "00:00";
            musicNaturalDurationTextBlock.Text = "00:00";
            sliProgress.Minimum = 0;
            sliProgress.Maximum = 10;
            sliProgress.Value = 0;
            playButton.IsEnabled = false;
        }
        // Chọn tất cả Music
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkSelected_Music() == false)
            {
                foreach (MusicItem item in playlistMusicListView.ItemsSource)
                {
                    item.isChecked = true;
                }
            }
            else
            {
                foreach (MusicItem item in playlistMusicListView.ItemsSource)
                {
                    item.isChecked = false;
                }
            }
            CheckBoxChanged(sender, e);
            System.ComponentModel.ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(playlist);
            view.Refresh();
        }
        #endregion

        #region Save, Load And Play Playlist

        private void savePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (_playlistIsPlay == false)
            {
                SavePlayListWinDow savePlayListWinDow = new SavePlayListWinDow();
                savePlayListWinDow.Owner = this;
                savePlayListWinDow.ShowDialog();
                if (_savePlaylist == true)
                {
                    savePlaylistButton.IsEnabled = false;
                }
            }
            else
            {
                _savePlaylist = true;
                _namePlaylist = Path.GetFileNameWithoutExtension(playlists[_locationPlaylist].fileName);

            }
            if (_savePlaylist == true)
            {
                XmlWriter writer = null;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("\t");

                writer = XmlWriter.Create(_playlistPath + _namePlaylist + ".xml", settings);

                writer.WriteStartElement("Playlist");

                // lưu vị trí Play
                writer.WriteStartElement("PositionPlay");
                writer.WriteAttributeString("Position", _locationMusic.ToString());
                writer.WriteEndElement();

                // lưu danh sách bài hát
                for (int i = 0; i < playlist.Count; i++)
                {
                    writer.WriteStartElement("Entry");
                    writer.WriteAttributeString("Source", playlist[i].filePath);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();

                namePlaylistTextBlock.Text = _namePlaylist;

                if (checkSameName(_namePlaylist + ".xml") == false)
                {
                    PlaylistItem item = new PlaylistItem();
                    item.filePath = _playlistPath + _namePlaylist + ".xml";
                    item.fileName = Path.GetFileName(item.filePath);
                    playlists.Add(item);
                    _locationPlaylist = playlists.Count - 1;
                }

                _playlistIsPlay = true;
                _savePlaylist = false;
            }
        }
        private bool checkSameName(string name)
        {
            for (int i = 0; i < playlists.Count; i++)
            {
                if (playlists[i].fileName == name)
                {
                    _locationPlaylist = i;
                    return true;
                }
            }
            return false;
        }

        //Load Playlist
        private void loadPlaylist()
        {
            if (!Directory.Exists("Playlists"))
            {
                Directory.CreateDirectory("Playlists");
            }
            string[] list = Directory.GetFiles(_playlistPath, "*.xml"); // lấy ra toàn file playlist
            for (int i = 0; i < list.Length; i++)
            {
                PlaylistItem item = new PlaylistItem();
                item.filePath = list[i];
                item.fileName = Path.GetFileName(item.filePath);
                playlists.Add(item);
            }
        }

        // Play Playlist
        private void PlaylistLibary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playlistLibaryListView.SelectedIndex != -1)
            {
                _playlistIsPlay = true;
                _locationPlaylist = playlistLibaryListView.SelectedIndex;
                namePlaylistTextBlock.Text = Path.GetFileNameWithoutExtension(playlists[_locationPlaylist].fileName);

                var doc = new XmlDocument();
                doc.Load(playlists[_locationPlaylist].filePath);
                var childs = doc.DocumentElement.ChildNodes;

                _player.Stop();
                _player.Close();
                playlist.Clear();

                if (childs.Count == 1)
                {
                    defaultStatus();
                }
                else
                {
                    _locationMusic = 0;
                    foreach (XmlNode child in childs)
                    {
                        if (child.Name == "PositionPlay")
                        {
                            if (child.Attributes["Position"].Value == "-1")
                            {
                                _locationMusic = 0;
                            }
                            else
                            {
                                _locationMusic = Int32.Parse(child.Attributes["Position"].Value);
                            }
                        }
                        else
                        {
                            var item = new MusicItem();
                            item.filePath = child.Attributes["Source"].Value;
                            item.fileName = Path.GetFileName(item.filePath);
                            playlist.Add(item);
                        }
                    }
                    playMusic();
                }
            }
        }

        #endregion

        #region Delete Playlist

        private void DeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < playlists.Count; i++)
            {
                if (playlists[i].isChecked == true)
                {
                    File.Delete(playlists[i].filePath);
                    if (_playlistIsPlay == true &&
                        (namePlaylistTextBlock.Text + ".xml") == playlists[i].fileName)
                    {
                        _playlistIsPlay = false;
                        defaultStatus();
                        namePlaylistTextBlock.Text = "Untitled playlist";
                        _locationMusic = -1;
                        _locationPlaylist = -1;
                        playlist.Clear();
                        CheckBoxChanged(sender, e);
                    }
                    playlists.RemoveAt(i);
                    i--;
                }
            }
            CheckBoxPlaylistChanged(sender, e);
        }
        private void CheckBoxPlaylistChanged(object sender, RoutedEventArgs e)
        {

            if (checkNotSelected_Playlist() == false)
            {
                deletePlaylist.Visibility = Visibility.Visible;
            }
            else if (checkNotSelected_Playlist() == true)
            {
                deletePlaylist.Visibility = Visibility.Collapsed;
            }
        }
        private bool checkNotSelected_Playlist()
        {
            for (int i = 0; i < playlists.Count; i++)
            {
                if (playlists[i].isChecked == true)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Automatic Updates Method

        // Cập nhật trạng thái vị trí phát trong Playlist
        private void updatePlayPosition()
        {
            if (_playlistIsPlay == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(playlists[_locationPlaylist].filePath);
                XmlNodeList Childs = doc.DocumentElement.ChildNodes;

                Childs[0].Attributes["Position"].Value = _locationMusic.ToString();

                doc.Save(playlists[_locationPlaylist].filePath);
            }
        }
        // Cập nhật khi Add Music
        private void updateAfterAdd()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(playlists[_locationPlaylist].filePath);

            var newNode = doc.CreateNode("element", "Entry", "");
            ((XmlElement)newNode).SetAttribute("Source", playlist[playlist.Count - 1].filePath);
            doc.DocumentElement.AppendChild(newNode);

            doc.Save(playlists[_locationPlaylist].filePath);
        }
        // Cập nhật khi Delete Music
        private void updateAfterDelete(int index)
        {
            if (_playlistIsPlay == true)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(playlists[_locationPlaylist].filePath);
                XmlNodeList Childs = doc.DocumentElement.ChildNodes;
                doc.DocumentElement.RemoveChild(Childs[index + 1]);

                Childs[0].Attributes["Position"].Value = _locationMusic.ToString();

                doc.Save(playlists[_locationPlaylist].filePath);
            }
        }
        #endregion

        // Create Playlist
        private void createNewPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            CreatePlaylistWindow createPlaylistWindow = new CreatePlaylistWindow();
            createPlaylistWindow.Owner = this;
            createPlaylistWindow.ShowDialog();

            if (_createPlaylist == true)
            {
                XmlWriter writer = null;
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = ("\t");

                writer = XmlWriter.Create(_playlistPath + _namePlaylist + ".xml", settings);

                writer.WriteStartElement("Playlist");

                // lưu vị trí Play
                writer.WriteStartElement("PositionPlay");
                writer.WriteAttributeString("Position", "-1");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.Flush();
                writer.Close();

                PlaylistItem item = new PlaylistItem();
                item.filePath = _playlistPath + _namePlaylist + ".xml";
                item.fileName = Path.GetFileName(item.filePath);
                playlists.Add(item);
                _createPlaylist = false;
            }
        }

        // Thông tin ứng dụng
        private void InforButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show(
                "01.  Hook key: Play / Pause (Ctrl + p); Previous / Next(Ctrl + Alt + (ArrowLeft / ArrowRight)). \n" +
                "02.  Lê chuột vào Icon để biết chức năng của nó. \n" +
                "03.  Muốn xóa Music hay Playlist thì Click vào Checkbox của Music hay Playlist đó sẽ hiện ra nút Delete. \n" +
                "04.  Click vào Icon Menu góc trái trên cùng để hiển thị danh sách Playlist. \n" +
                "05.  Khi click vào 1 Music hay 1 Playlist bất kì thì ngay lập tức Play nó. \n" +
                "06.  Nếu Playlist được chọn không có dữ liệu vị trí Music chơi trước đó thì sẽ mặc định phát bài đầu tiên trong Playlist. \n" +
                "07.  Khi 1 Playlist đang phát thì mọi thao tác(thêm Music, xóa Music,...) sẽ tự động cập nhật dữ liệu.\n" +
                "08.  Không thể tạo Playlist rỗng hoặc Save Playlist với tên đã tồn tại trước đó.\n" + 
                "09.  Không thể thêm Music đã tồn tại trong Playlist\n" + 
                "10.  Nếu Icon đang hiển thị là 'Phát Vô tận' thì đang 'Phát Một Lần', và ngược lại.\n" +
                "11.  Nếu Icon đang hiển thị là 'Phát Ngẫu Nhiên' thì đang 'Phát Tuần Tự' , và ngược lại",
                "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

