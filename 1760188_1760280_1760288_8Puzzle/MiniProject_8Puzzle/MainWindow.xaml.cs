using Microsoft.Win32;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MiniProject_8Puzzle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Local Variable

        List<Image> images = null;
        List<int> matrix = new List<int>();
        int tickImg = 1;
        bool _isDragging = false; // trạng thái kéo ảnh
        Point _lastPos; // vị trí cũ của con trỏ chuột
        Point _selectedBoxImageIndexPoint = new Point(); // vị trí hộp của ảnh đang kéo
        int _imgWidth = 100;  // độ dài ảnh nhỏ
        int _imgHeight = 100; // chiều cao ảnh nhỏ
        int _selectedImageIndex = -1; // số thứ tự của ảnh đang kéo trong List image
        int _numBoxIsEmpty = 8; // số thứ tự vị trí hộp rỗng
        bool _selectedFileImage = false; // trạng thái đã chọn file hình ảnh chưa
        OpenFileDialog screen = new OpenFileDialog();
        DispatcherTimer _timer;
        TimeSpan _time;

        //Tọa độ các hộp
        List<Point> listPointBox = null;
        Point b1 = new Point();
        Point b2 = new Point();
        Point b3 = new Point();
        Point b4 = new Point();
        Point b5 = new Point();
        Point b6 = new Point();
        Point b7 = new Point();
        Point b8 = new Point();
        Point b9 = new Point();

        #endregion

        #region Constructor

        public MainWindow()
        {

            InitializeComponent();

            b1.X = Canvas.GetLeft(box1);
            b1.Y = Canvas.GetTop(box1);
            b2.X = Canvas.GetLeft(box2);
            b2.Y = Canvas.GetTop(box2);
            b3.X = Canvas.GetLeft(box3);
            b3.Y = Canvas.GetTop(box3);
            b4.X = Canvas.GetLeft(box4);
            b4.Y = Canvas.GetTop(box4);
            b5.X = Canvas.GetLeft(box5);
            b5.Y = Canvas.GetTop(box5);
            b6.X = Canvas.GetLeft(box6);
            b6.Y = Canvas.GetTop(box6);
            b7.X = Canvas.GetLeft(box7);
            b7.Y = Canvas.GetTop(box7);
            b8.X = Canvas.GetLeft(box8);
            b8.Y = Canvas.GetTop(box8);
            b9.X = Canvas.GetLeft(box9);
            b9.Y = Canvas.GetTop(box9);

            listPointBox = new List<Point>();
            listPointBox.Add(b1); listPointBox.Add(b2);
            listPointBox.Add(b3); listPointBox.Add(b4);
            listPointBox.Add(b5); listPointBox.Add(b6);
            listPointBox.Add(b7); listPointBox.Add(b8);
            listPointBox.Add(b9);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timeTextBlock.Text = "00:03:00";
            time_Tick();
        }
        #endregion

        #region Events

        // Chọn ảnh từ máy
        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            screen.Multiselect = false;
            screen.Title = "Chọn hình";
            screen.Filter = " JPG(*.JPG)|*.jpg|PNG(*.PNG)|*.png|JPEG(*.JPEG)|*.jpeg";
            if (screen.ShowDialog() == true)
            {
                if (_selectedFileImage == true)
                {
                    reset();
                }
                else
                {
                    shuffleButton.IsEnabled = true;
                    _selectedFileImage = true;
                    shuffle();
                }
            }
        }
        // Lưu lại game đang chơi
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFileImage == true &&
                checkWin() == false &&
                timeTextBlock.Text != "00:03:00")
            {
                _timer.Stop();
                pauseButton.Content = "Resume";
                var writer = new StreamWriter("Save8Puzzle.data");
                writer.WriteLine(screen.FileName); // lưu lại đường dẫn hình ảnh

                for (int i = 0; i < listPointBox.Count; i++) // lưu lại vị trí hình ảnh
                {
                    for (int j = 0; j < images.Count - 1; j++)
                    {
                        var xBox = listPointBox[i].X;
                        var yBox = listPointBox[i].Y;
                        var xImage = Canvas.GetLeft(images[j]);
                        var yImage = Canvas.GetTop(images[j]);
                        if (xBox == xImage && yBox == yImage)
                        {
                            if (i < listPointBox.Count - 1)
                            {
                                writer.Write(j);
                                writer.Write(" ");
                            }
                            else
                            {
                                writer.WriteLine(j);
                            }
                        }
                    }
                }
                writer.WriteLine(_numBoxIsEmpty); // lưu lại vị trí ô trống
                writer.WriteLine(_time.Minutes);  // lưu lại số phút còn lại
                writer.WriteLine(_time.Seconds);  // lưu lại số giây còn lại
                writer.WriteLine(_time); // lưu lại định dạng thời gian
                writer.Close();
                System.Windows.MessageBox.Show("Game saved!", "Success",
                   MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Save Game Failed!\n" + "No Games Are In Progress.",
                     "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Load lại game đã lưu
        private void loadGameButton_Click(object sender, RoutedEventArgs e)
        {
            loadGame();
        }


        //các thao tác nhấp, kéo, thả chuột 
        private void board_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectedFileImage == true)
            {
                _lastPos = e.GetPosition(this);
                var curPos = _lastPos;
                for (int i = 0; i < images.Count; i++)
                {
                    if ((hitTest(images[i], _lastPos) == true && ImageNearEmptyBox(images[i]) == false) ||
                        (curPos.X < 46 || curPos.X > _imgWidth * 3 + 54 ||
                          curPos.Y < 66 || curPos.Y > _imgHeight * 3 + 74))
                    {
                        DragDrop.DoDragDrop(images[i], sender, DragDropEffects.Move);
                    }
                    if (hitTest(images[i], _lastPos) == true && ImageNearEmptyBox(images[i]) == true)
                    {
                        starting();
                        _selectedImageIndex = i;
                        _isDragging = true;
                        _selectedBoxImageIndexPoint.X = Canvas.GetLeft(images[_selectedImageIndex]);
                        _selectedBoxImageIndexPoint.Y = Canvas.GetTop(images[_selectedImageIndex]);
                        Canvas.SetZIndex(images[_selectedImageIndex], tickImg++);
                        return;
                    }
                }
            }
        }
        private void board_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging == true)
            {
                var curPos = e.GetPosition(this);
                var dx = curPos.X - _lastPos.X; // độ lệch X
                var dy = curPos.Y - _lastPos.Y; // độ lệch Y


                var oldLeft = Canvas.GetLeft(images[_selectedImageIndex]);
                var oldTop = Canvas.GetTop(images[_selectedImageIndex]);

                var curWidthImage = oldLeft + dx;
                var curHeightImage = oldTop + dy;


                if (curWidthImage < 4)
                {
                    curWidthImage = 4;
                }
                else if (curWidthImage + _imgWidth > 312)
                {
                    curWidthImage = 312 - _imgWidth;
                }
                if (curHeightImage < 4)
                {
                    curHeightImage = 4;
                }
                else if (curHeightImage + _imgHeight > _imgHeight * 3 + 12)
                {
                    curHeightImage = _imgHeight * 2 + 12;
                }


                if (curPos.X < 26 || curPos.X > _imgWidth * 3 + 74 ||
                      curPos.Y < 49 || curPos.Y > _imgHeight * 3 + 89)
                {
                    Canvas.SetLeft(images[_selectedImageIndex], _selectedBoxImageIndexPoint.X);
                    Canvas.SetTop(images[_selectedImageIndex], _selectedBoxImageIndexPoint.Y);
                    _isDragging = false;
                    System.Windows.MessageBox.Show("You Cannot Drag The Image Out Of The Box!",
                        "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                }
                else
                {
                    Canvas.SetLeft(images[_selectedImageIndex], curWidthImage);
                    Canvas.SetTop(images[_selectedImageIndex], curHeightImage);
                }
                _lastPos = curPos;
            }
        }
        private void board_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var curPos = e.GetPosition(this);

            if (_isDragging == true)
            {
                _isDragging = false;
                var xCurPos = curPos.X - 42;
                var yCurPos = curPos.Y - 62;

                var xEmptyBox = listPointBox[_numBoxIsEmpty].X;
                var yEmptyBox = listPointBox[_numBoxIsEmpty].Y;

                if (xCurPos >= xEmptyBox &&
                    xCurPos <= xEmptyBox + _imgWidth &&
                    yCurPos >= yEmptyBox &&
                    yCurPos <= yEmptyBox + _imgHeight)
                {
                    Canvas.SetLeft(images[_selectedImageIndex], xEmptyBox);
                    Canvas.SetTop(images[_selectedImageIndex], yEmptyBox);
                    _numBoxIsEmpty = newEmptyBoxPoint(images[_selectedImageIndex]);
                }
                else
                {
                    Canvas.SetLeft(images[_selectedImageIndex], _selectedBoxImageIndexPoint.X);
                    Canvas.SetTop(images[_selectedImageIndex], _selectedBoxImageIndexPoint.Y);
                }
                Win();
            }
        }

        // Các thao tác Button shuffle, pause, quit
        private void shuffleGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (timeTextBlock.Text != "00:03:00")
            {
                MessageBoxResult DialogResult;
                DialogResult = System.Windows.MessageBox.Show("Are You Sure To Reset ?", "Puzzle",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (DialogResult == MessageBoxResult.Yes)
                {
                    reset();
                    _timer.Stop();
                    pauseButton.IsEnabled = false;
                    shuffleButton.Content = "Shuffle";
                }

            }
            else
            {
                shuffle();
            }

        }
        private void pauseGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (pauseButton.IsEnabled == true)
            {
                if (_timer.IsEnabled == true)
                {
                    _timer.Stop();
                    pauseButton.Content = "Resume";
                }
                else
                {
                    _timer.Start();
                    pauseButton.Content = "Pause";
                }
            }
        }
        private void quitGameButton_Click(object sender, RoutedEventArgs e)
        {

            MessageBoxResult DialogResult;
            DialogResult = System.Windows.MessageBox.Show("Are You Sure To Quit ?", "Puzzle",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (DialogResult == MessageBoxResult.Yes) Environment.Exit(0);

        }

        // 4 nút điều khiển
        private void leftArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFileImage == true)
            {
                if (_numBoxIsEmpty <= 5 && _numBoxIsEmpty >= 0)
                {
                    starting();
                    for (int i = 0; i < 8; i++)
                    {
                        var xImage = Canvas.GetLeft(images[i]);
                        var yImage = Canvas.GetTop(images[i]);
                        var xBox = listPointBox[_numBoxIsEmpty + 3].X;
                        var yBox = listPointBox[_numBoxIsEmpty + 3].Y;
                        if (xImage == xBox && yImage == yBox)
                        {
                            Canvas.SetLeft(images[i], listPointBox[_numBoxIsEmpty].X);
                            Canvas.SetTop(images[i], listPointBox[_numBoxIsEmpty].Y);
                            _numBoxIsEmpty += 3;
                            Win();
                            return;
                        }
                    }
                }
            }
        }
        private void rightArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFileImage == true)
            {

                if (_numBoxIsEmpty >= 3 && _numBoxIsEmpty <= 8)
                {
                    starting();
                    for (int i = 0; i < 8; i++)
                    {
                        var xImage = Canvas.GetLeft(images[i]);
                        var yImage = Canvas.GetTop(images[i]);
                        var xBox = listPointBox[_numBoxIsEmpty - 3].X;
                        var yBox = listPointBox[_numBoxIsEmpty - 3].Y;
                        if (xImage == xBox && yImage == yBox)
                        {
                            Canvas.SetLeft(images[i], listPointBox[_numBoxIsEmpty].X);
                            Canvas.SetTop(images[i], listPointBox[_numBoxIsEmpty].Y);
                            _numBoxIsEmpty -= 3;
                            Win();
                            return;
                        }
                    }
                }
            }
        }
        private void upArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFileImage == true)
            {

                if (_numBoxIsEmpty != 2 && _numBoxIsEmpty != 5 &&
                    _numBoxIsEmpty != 8 && _numBoxIsEmpty != -1)
                {
                    starting();
                    for (int i = 0; i < 8; i++)
                    {
                        var xImage = Canvas.GetLeft(images[i]);
                        var yImage = Canvas.GetTop(images[i]);
                        var xBox = listPointBox[_numBoxIsEmpty + 1].X;
                        var yBox = listPointBox[_numBoxIsEmpty + 1].Y;
                        if (xImage == xBox && yImage == yBox)
                        {
                            Canvas.SetLeft(images[i], listPointBox[_numBoxIsEmpty].X);
                            Canvas.SetTop(images[i], listPointBox[_numBoxIsEmpty].Y);
                            _numBoxIsEmpty += 1;
                            Win();
                            return;
                        }
                    }
                }
            }
        }
        private void downArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFileImage == true)
            {

                if (_numBoxIsEmpty != 0 && _numBoxIsEmpty != 3 &&
                    _numBoxIsEmpty != 6 && _numBoxIsEmpty != -1)
                {
                    starting();
                    for (int i = 0; i < 8; i++)
                    {
                        var xImage = Canvas.GetLeft(images[i]);
                        var yImage = Canvas.GetTop(images[i]);
                        var xBox = listPointBox[_numBoxIsEmpty - 1].X;
                        var yBox = listPointBox[_numBoxIsEmpty - 1].Y;
                        if (xImage == xBox && yImage == yBox)
                        {
                            Canvas.SetLeft(images[i], listPointBox[_numBoxIsEmpty].X);
                            Canvas.SetTop(images[i], listPointBox[_numBoxIsEmpty].Y);
                            _numBoxIsEmpty -= 1;
                            Win();
                            return;
                        }
                    }
                }
            }
        }
        // điều khiển bằng 4 nút mũi tên trên bàn phím
        private void Window_Key(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Left")
            {
                leftArrowButton_Click(sender, e);
            }
            if (e.Key.ToString() == "Right")
            {
                rightArrowButton_Click(sender, e);
            }
            if (e.Key.ToString() == "Up")
            {
                upArrowButton_Click(sender, e);
            }
            if (e.Key.ToString() == "Down")
            {
                downArrowButton_Click(sender, e);
            }
        }

        #endregion

        #region Methods

        private bool hitTest(Image image, Point pos)
        {
            var oldLeft = Canvas.GetLeft(image);
            var oldTop = Canvas.GetTop(image);

            if (pos.X >= oldLeft + 42 &&
                pos.X <= oldLeft + _imgWidth + 42 &&
                pos.Y >= oldTop + 62 &&
                pos.Y <= oldTop + _imgHeight + 62)
            {
                return true;
            }
            return false;
        }
        private bool ImageNearEmptyBox(Image img)
        {
            if (_numBoxIsEmpty >= 0 && _numBoxIsEmpty <= 8)
            {
                var xImage = Canvas.GetLeft(img);
                var yImage = Canvas.GetTop(img);
                var xBoxIsEmpty = listPointBox[_numBoxIsEmpty].X;
                var yBoxIsEmpty = listPointBox[_numBoxIsEmpty].Y;

                if ((xImage == xBoxIsEmpty && yImage + 104 == yBoxIsEmpty) ||
                     (yImage == yBoxIsEmpty && xImage + 104 == xBoxIsEmpty) ||
                     (xImage == xBoxIsEmpty && yImage - 104 == yBoxIsEmpty) ||
                     (yImage == yBoxIsEmpty && xImage - 104 == xBoxIsEmpty))
                {
                    return true;
                }
            }
            return false;

        }
        private int newEmptyBoxPoint(Image img)
        {
            for (int i = 0; i <= 8; i++)
            {
                var xbox = listPointBox[i].X;
                var ybox = listPointBox[i].Y;
                var xImage = _selectedBoxImageIndexPoint.X;
                var yImage = _selectedBoxImageIndexPoint.Y;

                if (xbox == xImage && ybox == yImage)
                {
                    return i;
                }
            }
            return _numBoxIsEmpty;
        }
        private void time_Tick()
        {
            _time = TimeSpan.FromSeconds(179);
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 1), DispatcherPriority.Normal, delegate
            {
                timeTextBlock.Text = _time.ToString();

                if (_time == TimeSpan.Zero)
                {
                    _isDragging = false;
                    System.Windows.MessageBox.Show("You Lost, Let's Play Again!",
                                "Puzzle",
                                MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    reset();
                    _time = TimeSpan.FromSeconds(180);
                }
                if (_time.Minutes == 0 && _time.Seconds <= 10)
                {
                    timeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
                _time = _time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);
            _timer.Stop();
        }
        private void reset()
        {
            timeTextBlock.Text = "00:03:00";
            timeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            _time = TimeSpan.FromSeconds(179);
            _timer.Stop();
            shuffle();
            pauseButton.IsEnabled = false;
            shuffleButton.Content = "Shuffle";
        }
        private void shuffle()
        {
            board.Children.Clear();
            images = new List<Image>();
            _numBoxIsEmpty = 8;

            var filename = screen.FileName;
            var image = new BitmapImage(new Uri(filename));

            var width = image.PixelWidth / 3;
            var height = image.PixelHeight / 3;

            if (height > width)
            {
                height = width;
            }
            else
            {
                width = height;
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var cropped = new CroppedBitmap(image,
                            new Int32Rect(i * width, j * height, width, height));

                    var img = new Image();
                    img.Source = cropped;
                    img.Width = _imgWidth;
                    img.Height = _imgHeight;
                    img.Tag = i * 3 + j + 1; // 1 - 2 - 3 ... - 9                 
                    images.Add(img);
                }
            }


            var rd = new Random();
            var indices = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!(i == 2 && j == 2))
                    {
                        var index = rd.Next(indices.Count);
                        var index2 = indices[index];

                        board.Children.Add(images[index2]);
                        Canvas.SetLeft(images[index2], i * _imgWidth + (i + 1) * 4);
                        Canvas.SetTop(images[index2], j * _imgHeight + (j + 1) * 4);

                        indices.RemoveAt(index);
                    }
                }
            }
            var oriImg = new CroppedBitmap(image,
                            new Int32Rect(0, 0, width * 3, height * 3));
            originalImage.Source = oriImg;
        }
        private void starting()
        {
            if (pauseButton.IsEnabled == false)
            {
                pauseButton.IsEnabled = true;
            }
            if (_timer.IsEnabled == false)
            {
                pauseButton.Content = "Pause";
            }
            if (timeTextBlock.Text == "00:03:00")
            {
                shuffleButton.Content = "Reset";
            }
            _timer.Start();
        }
        private bool checkWin()
        {
            int i;
            for (i = 0; i < 8; i++)
            {
                var xImage = Canvas.GetLeft(images[i]);
                var yImage = Canvas.GetTop(images[i]);
                var xBox = listPointBox[i].X;
                var yBox = listPointBox[i].Y;
                if (xBox != xImage || yBox != yImage)
                {
                    return false;
                }
            }

            return true;
        }
        private void Win()
        {
            if (checkWin() == true)
            {
                board.Children.Add(images[8]);
                Canvas.SetLeft(images[8], listPointBox[8].X);
                Canvas.SetTop(images[8], listPointBox[8].Y);
                _numBoxIsEmpty = -1;
                _timer.Stop();
                pauseButton.IsEnabled = false;
                System.Windows.MessageBox.Show("Congratulations, You Won!", "Puzzle",
                        MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void loadGame()
        {
            StreamReader reader;
            var index = new List<int>();
            string strIndex;
            string numBoxIsEmpty;
            string minutes;
            string seconds;
            string timetextblock;
            try
            {
                reader = new StreamReader("Save8Puzzle.data");
                screen.FileName = reader.ReadLine(); // lấy ra đường dẫn file hình ảnh đã lưu
                strIndex = reader.ReadLine(); // lấy ra số thứ tự ảnh là chuỗi
                numBoxIsEmpty = reader.ReadLine(); // lấy ra vị trí ô trống đã lưu
                minutes = reader.ReadLine(); // lấy ra số phút đã lưu
                seconds = reader.ReadLine(); // lấy ra sô giây đã lưu
                timetextblock = reader.ReadLine(); // lấy ra định dạng thời gian đã lưu

            }
            catch
            {
                System.Windows.MessageBox.Show("Load Game Failed!\n" + "No Games Available To Load.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
         
            
             // lấy ra bị trí hình ảnh đã lưu
            string[] arrListStr = strIndex.Split(' ');
            for (int i = 0; i < arrListStr.Length; i++)
            {
                index.Add(Convert.ToInt32(arrListStr[i]));
            }

            shuffle();
            _numBoxIsEmpty = Convert.ToInt32(numBoxIsEmpty);

            int sec = Convert.ToInt32(seconds);
            int min = Convert.ToInt32(minutes);
            int timePlay = min * 60 + sec;
            timeTextBlock.Text = timetextblock;

            if (timePlay > 10)
            {
                timeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                timeTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            _time = TimeSpan.FromSeconds(timePlay);


            _selectedFileImage = true;

            int j = 0;
            for (int i = 0; i < index.Count; i++)
            {

                if (j == _numBoxIsEmpty)
                {
                    j++;
                }
                Canvas.SetLeft(images[index[i]], listPointBox[j].X);
                Canvas.SetTop(images[index[i]], listPointBox[j].Y);
                j++;
            }
            _timer.Stop();
            shuffleButton.IsEnabled = true;
            shuffleButton.Content = "Reset";
            pauseButton.IsEnabled = false;
            _selectedFileImage = true;

            System.Windows.MessageBox.Show("Load Game Successfully!", "Success",
              MessageBoxButton.OK, MessageBoxImage.Information);

        }

        #endregion
    }
}
