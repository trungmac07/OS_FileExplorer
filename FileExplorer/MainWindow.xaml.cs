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
using System.Management;
using System.Collections;


namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class HardDrive
        {
            private string model = null;
            private string type = null;
            private string serialNo = null;
            public string Model
            {
                get { return model; }
                set { model = value; }
            }
            public string Type
            {
                get { return type; }
                set { type = value; }
            }
            public string SerialNo
            {
                get { return serialNo; }
                set { serialNo = value; }
            }
        }
        public void initialization()
        {
            getDrive();
            
        }
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

                FolderTreeContain.Children.Add(button);
            }
        }
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        public SeriesCollection ChartData { get; set; }
        public int currentDisk = 1;
        public int currentPartition = 0;
        bool isPartitionClicked = false;
        MBR mBR = new MBR();
        Tree FolderTree { get; set; } = new Tree();
        public MainWindow()
        {
            // Khoi's codes
            InitializeComponent();
            initialization();
            /*

            string drivePath = @"\\.\PhysicalDrive" + currentDisk;
            Function.stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);

            MBR mBR = new MBR();
            mBR.readMBR(currentDisk);
            //mBR.printMBRTable();

            mBR.printPartitionInfo(currentPartition);
            if (mBR.getPartitionType(currentPartition) == "NTFS")
            {
                NTFS ntfs = new NTFS(mBR.getFirstSectorLBA(currentPartition), mBR.getSectorInPartition(currentPartition), currentDisk);
                ntfs.printVBRInfo();
                FolderTree = ntfs.buildTree();
                renderRoots();
            }
            */
        }
        public void deleteParitionFromView()
        {
            while (PartitionArea.Children.Count > 0)
            {
                PartitionArea.Children.RemoveAt(0);
            }
        }
        public void printPartition(int index)
        {
            Button button = new Button();
            TextBlock textBlock = new TextBlock();

            textBlock.Margin = new Thickness(20, 0, 0, 0);
            textBlock.Text = "PARTITION " + index.ToString();
            textBlock.FontSize = 22;

            button.Content = textBlock;
            button.Style = (Style)this.FindResource("PartitionButton");

            button.Click += (sender, e) =>
            {
                isPartitionClicked = true;
                currentPartition = index;
                string partitionType = mBR.getPartitionType(index);
                long head = 0, sector = 0, cylinder = 0;
                FileImage.Source = new BitmapImage(new Uri(@"/resources/compact-disk.png", UriKind.RelativeOrAbsolute));
                PartitionType.Text = partitionType;
                FileName.Text = "PARTITION " + index.ToString();
                FileSize.Text = "First Sector(LBA): " + mBR.getFirstSectorLBA(index);
                mBR.getStartSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
                DateCreated.Text = "Begin address(CHS): " + head + "(Head)" + sector + "(Sector)" + cylinder + "(Cylinder)";
                mBR.getLastSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
                TimeCreated.Text = "End address(CHS): " + head + "(Head)" + sector + "(Sector)" + cylinder + "(Cylinder)";
                Attribute.Text = "Status";
                IsHidden.Content = "Bootable";
                IsReadOnly.Content = "Unbootable";
                if (mBR.getPartitionStatus(index) == "bootable")
                {
                    IsHidden.IsChecked = true;
                    IsReadOnly.IsChecked = false;
                }
                else if (mBR.getPartitionStatus(index) == "unbootable")
                {
                    IsHidden.IsChecked = false;
                    IsReadOnly.IsChecked = true;
                }

                if (partitionType == "FAT32")
                {
                    clearFolderTree();
                    //chart();
                }
                else if (partitionType == "NTFS")
                {
                    clearFolderTree();
                    NTFS ntfs = new NTFS(mBR.getFirstSectorLBA(currentPartition), mBR.getSectorInPartition(currentPartition), currentDisk);
                    ntfs.printVBRInfo();
                    FolderTree = ntfs.buildTree();
                    chart(mBR.getSectorInPartition(currentPartition) * 512);
                    renderRoots();
                }

            };
            PartitionArea.Children.Add(button);
        }

        public void clearFolderTree()
        {
            foreach(var file in FolderTree.ListOfFiles)
            {
                if(this.FindName("n" + file.Key) != null)
                {
                    this.UnregisterName("n" + file.Key);
                }
            }
            FolderTreeContain.Children.Clear();

        }

        public void chart(long paritionSize)
        {
            ChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(FolderTree.sizeOnDiskOfTree()) },
                    DataLabels = false,
                    StrokeThickness = 0,
                    Stroke = null,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2EC4B6")),

                },
                new PieSeries
                {
                    Title = "Free",
                    Values = new ChartValues<ObservableValue>{new ObservableValue(paritionSize - FolderTree.sizeOnDiskOfTree()) },
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
            //DateTime dtCreationTime = oFileInfo.CreationTime;
            //MessageBox.Show("Date and Time File Created: " + dtCreationTime.ToString());
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
            ManagementObjectSearcher searcher = new
            ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            ArrayList hdCollection = new ArrayList();
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                HardDrive hd = new HardDrive();
                hd.Model = wmi_HD["Model"].ToString();
                hd.Type = wmi_HD["InterfaceType"].ToString();
                hdCollection.Add(hd);
            }
            searcher = new
            ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

            int i = 0;
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                // get the hard drive from collection
                // using index
                HardDrive hd = (HardDrive)hdCollection[i];

                // get the hardware serial no.
                if (wmi_HD["SerialNumber"] == null)
                    hd.SerialNo = "None";
                else
                    hd.SerialNo = wmi_HD["SerialNumber"].ToString();

                ++i;
            }
            
            int index = 0;
            foreach (HardDrive hd in hdCollection)
            {
                while (hd.Model[0] == ' ') hd.Model = hd.Model.Remove(0, 1);
                if (hd.Type == "USB")
                    createDiskButton(index, hd.Model, 1);
                else createDiskButton(index, hd.Model, 0);
                index++;
            }
        }

        public void createDiskButton(int index, string s, int kind)
        {
            Button DiskButton1 = new Button();

            DockPanel dcPanel1 = new DockPanel();

            TextBlock txt = new TextBlock();
            Image img = new Image();

            img.Height = 30;
            img.Width = 30;
            if (kind == 0)
                img.Source = new BitmapImage(new Uri("/resources/hdd.png", UriKind.RelativeOrAbsolute));
            else if (kind == 1)
                img.Source = new BitmapImage(new Uri("/resources/usb.png", UriKind.RelativeOrAbsolute));

            img.Stretch = Stretch.Fill;


            txt.Margin = new Thickness(20, 0, 0, 0);
            txt.Width = 150;
            txt.Text = s;
            txt.Padding = new Thickness(0, 7, 0, 0);
            txt.FontSize = 13;

            dcPanel1.Children.Add(img);
            dcPanel1.Children.Add(txt);

            DiskButton1.Height = 50;
            DiskButton1.Content = dcPanel1;
            DiskButton1.Style = (Style)this.FindResource("MenuButton");
            DiskButton1.Click += (sender, e) =>
            {
                currentDisk = index;
                currentPartition = -1;
                deleteParitionFromView();
                mBR.readMBR(index);
                for (int i = 0; i < 3; i++)
                {
                    if (mBR.isPartitionActive(i) == true)
                        printPartition(i);
                }

            };
            DiskArea.Children.Add(DiskButton1);

           
        }

        public void menuButtonClick(object sender, EventArgs e)
        {
            foreach (var child in DiskArea.Children)
            {
                if (child is Button)
                {
                    (child as Button).Style = (Style)this.FindResource("MenuButton");
                }
            }
            (sender as Button).Style = (Style)this.FindResource("SelectedMenuButton");
        }

        public void PartitionButtonClick(object sender, EventArgs e)
        {
            foreach (var child in PartitionArea.Children)
            {
                if (child is Button)
                {
                    (child as Button).Style = (Style)this.FindResource("PartitionButton");
                }
            }
           (sender as Button).Style = (Style)this.FindResource("SelectedPartitionButton");
          
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

        private void renderANode(FolderTreeNode node, StackPanel area)
        {
            Button button = new Button();
            button.Background = Brushes.Transparent;
            button.Width = 600;
            button.Height = 30;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.BorderThickness = new Thickness(0);
            button.Margin = new Thickness(37 * node.Level, 5, 0, 5);
            button.Tag = node.Info.ID;
            button.Click += infoButtonClick;

            DockPanel dockPanel = new DockPanel();
            dockPanel.Width = 600;
            dockPanel.HorizontalAlignment = HorizontalAlignment.Left;

            Button expand = new Button();

            expand.Style = (Style)this.FindResource("TreeExpandButton");
            expand.Tag = node.Info.ID;
            expand.Click += expandButtonClick;
            expand.BorderThickness = new Thickness(0);
            expand.Background = Brushes.Transparent;
            expand.FontSize = 12;

            Image image = new Image();
            image.Width = 25;
            image.Margin = new Thickness(5, 0, 0, 0);

            TextBlock textBlock = new TextBlock();
            textBlock.Text = node.Info.FileName;
            textBlock.Margin = new Thickness(12, 0, 0, 0);
            textBlock.Foreground = Brushes.Black;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontSize = 15;

            if (node.Info.IsDirectory == false)
            {
                image.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));
                expand.Background = Brushes.Transparent;
                expand.BorderThickness = new Thickness(0);
            }
            else
            {
                image.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));
                Image modeImage = new Image();
                modeImage.Width = 12;
                modeImage.Source = new BitmapImage(new Uri(@"/resources/colapse.png", UriKind.RelativeOrAbsolute));
                expand.Content = modeImage;
            }


            dockPanel.Children.Add(expand);
            dockPanel.Children.Add(image);
            dockPanel.Children.Add(textBlock);

            button.Content = dockPanel;

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(0, 3, 0, 0);
            stackPanel.Name = "n" + node.Info.ID;
            stackPanel.Children.Add(button);

            this.RegisterName(stackPanel.Name, stackPanel);

            area.Children.Add(stackPanel);

        }

        private void renderRoots()
        {
            foreach (var root in FolderTree.ListOfRoots)
            {
                renderANode(root.Value, FolderTreeContain);
            }
        }

        private void infoButtonClick(object sender, EventArgs e)
        {
            isPartitionClicked = false;
            long id = (long)(sender as Button).Tag;
            FileInfomation file = FolderTree.ListOfFiles[id].Info;

            if (file.IsDirectory == false)
                FileImage.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));
            else
                FileImage.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));

            FileSize.Text = "Size: " + Function.toFileSize(FolderTree.getSize(id));
            FolderTree.getSizeOnDisk(id);
            string name = "";
            if (file.FileName.Length >= 41)
            {
                for (int i = 0; i < 37; ++i)
                    name += file.FileName[i];
                name += " . . .";
                FileName.Text = name;
            }
            else
            {
                FileName.Text = file.FileName;
            }
            MoreInfoButton.Tag = id;
            Attribute.Text = "Attributes";

            DateCreated.Text = "Created Date: " + file.CreatedTime.ToLocalTime().ToString("dd/MM/yyyy");
            TimeCreated.Text = "Created Time: " + file.CreatedTime.ToLocalTime().ToString("HH:mm:ss");

            IsHidden.Content = "Is Hidden";
            IsReadOnly.Content = "Is ReadOnly";

            IsHidden.IsChecked = file.IsHidden;
            IsReadOnly.IsChecked = file.IsReadOnly;

        }

        private void expandButtonClick(object sender, EventArgs e)
        {
            if ((sender as Button).FontSize == 12) //expand command
            {
                long id = (long)(sender as Button).Tag;

                Image modeImage = new Image();
                modeImage.Source = new BitmapImage(new Uri(@"/resources/expand.png", UriKind.RelativeOrAbsolute));
                modeImage.Width = 12;
                (sender as Button).Content = modeImage;
                (sender as Button).FontSize = 15;   //change to open


                StackPanel area = (this.FindName("n" + id) as StackPanel);

                foreach (var child in FolderTree.ListOfFiles[id].Children)
                {
                    Console.WriteLine("IDDDD :" + FolderTree.ListOfFiles[child].Info.FileName);
                    Console.WriteLine("Level :" + FolderTree.ListOfFiles[child].Level);
                    Console.WriteLine();
                    FolderTreeNode node = FolderTree.ListOfFiles[child];
                    renderANode(node, area);
                }
            }
            else //collapse command
            {
                long id = (long)(sender as Button).Tag;
                Image modeImage = new Image();
                modeImage.Source = new BitmapImage(new Uri(@"/resources/colapse.png", UriKind.RelativeOrAbsolute));
                modeImage.Width = 12;
                (sender as Button).Content = modeImage;
                (sender as Button).FontSize = 12;   //change to close 


                StackPanel area = (this.FindName("n" + id) as StackPanel);

                foreach (var child in FolderTree.ListOfFiles[id].Children)
                {
                    area.Children.Remove((this.FindName("n" + child) as StackPanel));
                    unregisterChildren(child);
                }
            }
        }

        //
        private void unregisterChildren(long parent)
        {
            if (this.FindName("n" + parent) == null)
                return;
            foreach (var child in FolderTree.ListOfFiles[parent].Children)
                unregisterChildren(child);

            this.UnregisterName("n" + parent);
        }

        private void moreInfoButtonClick(object sender, EventArgs e)
        {
            long id = Int64.Parse((sender as Button).Tag.ToString());
            if (isPartitionClicked == false)
            {
                if (FolderTree.ListOfFiles.ContainsKey(id) == true)
                {
                    FileInfomation file = FolderTree.ListOfFiles[id].Info;

                    PopUp popup = new PopUp(FolderTree, id);
                    popup.Show();
                }
            }
            else
            {
                if (currentPartition != -1)
                {
                    PopUp popup = new PopUp(currentDisk, currentPartition);
                    popup.Show();

                }
            }
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
                res += (long) src[offset + i];
            }
            return res;
        }

        static public long littleEndian(byte[] src, long offset, long length)
        {
            long res = 0;
            for (long i = length - 1; i >= 0; --i)
            {
                res <<= 8;
                res += (long) src[offset + i];
            }
            return res;
        }

        static public string toFileSize(double size)
        {
            string unit = " ";
            if (size > 1000)
            {
                size /= 1024;
                unit = " K";
            }
            if (size > 1000)
            {
                size /= 1024;
                unit = " M";
            }
            if (size > 1000)
            {
                size /= 1024;
                unit = " G";
            }

            return size.ToString("F2") + unit + "B";
        }

    }
}
