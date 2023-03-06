using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void getDirectoryTree()
        {
            DriveInfo curDrive = DriveInfo.GetDrives()[0];
            DirectoryInfo curDirectory = curDrive.RootDirectory;
            DirectoryInfo[] directoryList = curDirectory.GetDirectories();

            foreach (DirectoryInfo directory in directoryList)
            {
                //<Button Height="80" Background="#222831" Foreground="#EEEEEE" BorderThickness="0 0 0 0">O DIA C</Button>
                Button button = new Button();
                button.Height = 50;
                button.Background = new SolidColorBrush(Color.FromRgb(57, 62, 70));
                button.Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                button.BorderThickness = new Thickness(0, 0, 0, 0);
                button.Content = directory.Name;
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Cursor = Cursors.Hand;

                DockPanel dockPanel = new DockPanel();
                TextBlock textBlock = new TextBlock();
                textBlock.Text = directory.Name;
                textBlock.Margin = new Thickness(10, 5, 0, 0);

                BitmapImage myBitmapImage = new BitmapImage();
                myBitmapImage.BeginInit();
                myBitmapImage.UriSource = new Uri("resources/folder.png", UriKind.RelativeOrAbsolute);
                myBitmapImage.DecodePixelWidth = 2048;
                myBitmapImage.EndInit();
                Image image = new Image();
                image.Source = myBitmapImage;
                image.Height = 25;
                image.Width = 25;

                dockPanel.Children.Add(image);
                dockPanel.Children.Add(textBlock);
                button.Content = dockPanel;
                DirectoryTreeContain.Children.Add(button);
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            getDirectoryTree();
            //readFileAttribute();
            ChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(73) },
                    DataLabels = false,
                    StrokeThickness = 0,
                    Fill = new SolidColorBrush(Color.FromRgb(0,173,181)),

                },
                new PieSeries
                {
                    Title = "Free",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(27) },
                    DataLabels = false,
                    StrokeThickness = 0,
                    Fill = new SolidColorBrush(Color.FromRgb(238,238,238)),
                }
            }
                ;
            DataContext = this;
        }

        public void readFileAttribute()
        {
            FileInfo oFileInfo = new FileInfo(@"E:\Download\file.png");
            MessageBox.Show("My File's Name: \"" + oFileInfo.Name + "\"");
            DateTime dtCreationTime = oFileInfo.CreationTime;
            MessageBox.Show("Date and Time File Created: " + dtCreationTime.ToString());
            MessageBox.Show("myFile Extension: " + oFileInfo.Extension);
            MessageBox.Show("myFile total Size: " + oFileInfo.Length.ToString());
            MessageBox.Show("myFile filepath: " + oFileInfo.DirectoryName);
            MessageBox.Show("My File's Full Name: \"" + oFileInfo.FullName + "\"");

        }

        public SeriesCollection ChartData { get; set; }


    }

}
