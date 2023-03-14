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
using System.Windows.Media.Animation;
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
                button.Background = new SolidColorBrush(Colors.White);
                //button.Foreground = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                button.BorderThickness = new Thickness(0, 0, 0, 0);
                button.Content = directory.Name;
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Cursor = Cursors.Hand;

                DockPanel dockPanel = new DockPanel();
                TextBlock textBlock = new TextBlock();
                textBlock.Text = directory.Name;
                textBlock.Margin = new Thickness(10, 5, 0, 0);
                textBlock.Foreground = new SolidColorBrush(Colors.Black);

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
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        public SeriesCollection ChartData { get; set; }
        public int currentDisk = 1;
        public int currentPartition = 1;
        public MainWindow()
        {
            // Khoi's codes
            InitializeComponent();
            getDirectoryTree();
            getDrive();
            chart();

            string drivePath = @"\\.\PhysicalDrive" + currentDisk;
            Function.stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);



            MBR mBR = new MBR();
            mBR.readMBR(currentDisk);

            //mBR.printMBRTable();

           


            mBR.printPartitionInfo(currentPartition);
            if(mBR.getPartitionType(currentPartition) == "NTFS")
            {
                NTFS ntfs = new NTFS(mBR.getFirstSectorLBA(currentPartition), mBR.getSectorInPartition(currentPartition),currentDisk);
                ntfs.printVBRInfo();
                ntfs.readAttribute();
                ntfs.showTree();
            }

            ////////
            /*string diskPath = @"\\.\D:"; // Replace with the path to your disk

            using (FileStream fs = new FileStream(diskPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] mbr = new byte[512];
                int bytesRead = 0;

                bytesRead = fs.Read(mbr, 0, mbr.Length);

                // Process the MBR data here
                // The contents of the 'mbr' array represent the bytes read from the MBR sector

                for (int i = 0; i < mbr.Length; i++)
                {
                    byte b = mbr[i];
                    // Do something with the byte here
                    if(b == 128)
                        Console.WriteLine("HERE:" + i);
                    Console.WriteLine(b);
                }
                Console.WriteLine(mbr[446]);
            }*/

          
            
        }
        public void chart()
        {
            ChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(73) },
                    DataLabels = false,
                    StrokeThickness = 0,
                    Stroke = null,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2EC4B6")),

                },
                new PieSeries
                {
                    Title = "Free",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(27) },
                    DataLabels = false,
                    StrokeThickness = 0,
                    Stroke = null,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CD8D0")),
                }
            };
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

        public void showFileInfo(FileInfo fileinfo)
        {
            FileName.Text = fileinfo.Name;
            FileSize.Text = fileinfo.Length.ToString();
            DateCreated.Text = fileinfo.CreationTime.ToString();
            TimeCreated.Text = fileinfo.CreationTime.ToString();
            getDrive();
        }
        public void getDrive()
        {
            foreach (DriveInfo d in allDrives)
            {
                if (d.DriveType == DriveType.Fixed)
                    createButton(d.Name, 0);
                else if(d.DriveType == DriveType.Removable)
                    createButton(d.Name, 1);
            }
        }
        public void createButton(string s,int kind)
        {
            s += "abcdef name";
            Button DiskButton1 = new Button();

            DockPanel dcPanel1 = new DockPanel();

            TextBlock txt = new TextBlock();
            Image img = new Image();

            img.Height = 40;
            img.Width = 40;
            if(kind == 0)
                img.Source = new BitmapImage(new Uri("/resources/hdd.png",UriKind.RelativeOrAbsolute));
            else if(kind == 1)
                img.Source = new BitmapImage(new Uri("/resources/usb.png", UriKind.RelativeOrAbsolute));

            img.Stretch = Stretch.Fill;
        

            txt.Margin = new Thickness(20, 0, 0, 0);
            txt.Width = 150;    
            txt.Text = s;
            txt.Padding = new Thickness(0, 7, 0, 0);

            dcPanel1.Children.Add(img);
            dcPanel1.Children.Add(txt);
            
            DiskButton1.Height = 70;
            DiskButton1.Content = dcPanel1;
            DiskButton1.Style = (Style)this.FindResource("MenuButton");
            DiskArea.Children.Add(DiskButton1);

            
        }

        public void menuButtonClick(object sender, EventArgs e)
        {
            foreach(var child in DiskArea.Children)
            {
                if(child is Button)
                {
                    (child as Button).Style = (Style)this.FindResource("MenuButton");
                }   
            }
            (sender as Button).Style = (Style)this.FindResource("SelectedMenuButton");
        }

        public void closeApp(object sender, EventArgs e)
        {
            Close();
        }

        private void dragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

       


    }
    public class Function
    {
        public static FileStream stream = null;
        static public long littleEndian(byte[] src, int offset, int length)
        {
            long res = 0;
            for (int i = length - 1; i >= 0; --i)
            {
                res <<= 8;
                res += (int)src[offset + i];
            }
            return res;
        }
       

    }
}
