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


        public SeriesCollection ChartData { get; set; }
        public int currentDisk = 1;
        public int currentPartition = 0;
        bool isPartitionClicked = false;
        MBR mBR = new MBR();
        Tree FolderTree { get; set; } = new Tree();
        public MainWindow()
        {
            InitializeComponent();
            initialization();
        }
        public void deleteParitionFromView()
        {
            PartitionArea.Children.Clear();
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

                printMBRInfo(index);

                //render tree and chart for used/total size
                if (partitionType == "FAT32")
                {
                    clearFolderTree();
                    FAT32 a = new FAT32(mBR.getFirstSectorLBA(currentPartition), currentDisk);
                    FolderTree = a.readRoot();
                  
               
                }
                else if (partitionType == "NTFS")
                {
                    clearFolderTree();
                    NTFS ntfs = new NTFS(mBR.getFirstSectorLBA(currentPartition), mBR.getSectorInPartition(currentPartition), currentDisk);
                    ntfs.printVBRInfo();
                    FolderTree = ntfs.buildTree();
                    FolderTree.IsNTFS = ntfs.sizeOfMFTAndVBR();
                   
                }

                long partitionSize = mBR.getSectorInPartition(currentPartition) * 512;
                UsedTotal.Text = Function.toFileSize((double)FolderTree.sizeOnDiskOfTree()) + " / " + Function.toFileSize((double)partitionSize);
                chart(partitionSize);
                PieChart.Series = ChartData;
                renderRoots();
                TreeView.ScrollToTop();

            };
            PartitionArea.Children.Add(button);
        }

        void printMBRInfo(int index)
        {
            long head = 0, sector = 0, cylinder = 0;
            FileImage.Source = new BitmapImage(new Uri(@"/resources/compact-disk.png", UriKind.RelativeOrAbsolute));
            PartitionType.Text = mBR.getPartitionType(index);
            FileName.Text = "PARTITION " + index.ToString();
            FileSize.Text = "First Sector(LBA): " + mBR.getFirstSectorLBA(index);
            mBR.getStartSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
            DateCreated.Text = "Begin address(CHS): (" + head + "," + sector + "," + cylinder + ")";
            mBR.getLastSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
            TimeCreated.Text = "End address(CHS): (" + head + "," + sector + "," + cylinder + ")";
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
        }

        public void clearFolderTree()
        {
            foreach (var file in FolderTree.ListOfFiles)
            {
                if (this.FindName("n" + file.Key) != null)
                {
                    this.UnregisterName("n" + file.Key);
                }
            }

            //clear all old folder
            FolderTreeContain.Children.Clear();

            //show "PLEASE CHOOSE A DISK AND A PARTITION TO VIEW" in empty treeview area
            TextBlock pleaseChoose = new TextBlock();
            pleaseChoose.Text = "PLEASE CHOOSE A DISK AND A PARTITION TO VIEW";
            pleaseChoose.Style = (Style)FindResource("UbuntuFont");
            pleaseChoose.FontSize = 25;
            pleaseChoose.HorizontalAlignment = HorizontalAlignment.Center;
            pleaseChoose.Margin = new Thickness(0, 150, 0, 0);
            pleaseChoose.Foreground = new SolidColorBrush(Color.FromRgb(201, 201, 201));
            FolderTreeContain.Children.Add(pleaseChoose);
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
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            
            foreach (ManagementObject wmi_HD in searcher.Get())
            {
                HardDrive hd = new HardDrive();
                hd.Model = wmi_HD["Model"].ToString();
                hd.Type = wmi_HD["InterfaceType"].ToString();
       
                if (hd.Type == "USB")
                {
                    string x = wmi_HD["DeviceId"].ToString();
                    createDiskButton((int)x[x.Length - 1] - 48, hd.Model, 1);
                }
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
                resetFileInfoArea();
                currentDisk = index;
                currentPartition = -1;
                deleteParitionFromView();
                clearFolderTree();
                mBR.readMBR(index);
                for (int i = 0; i < 3; i++)
                {
                    if (mBR.isPartitionActive(i) == true)
                        printPartition(i);
                }
                PleaseChoose.Visibility = Visibility.Visible;

            };
            DiskArea.Children.Add(DiskButton1);

        }

        void resetFileInfoArea()
        {
            FileName.Text = "";
            FileImage.Source = null;
            FileSize.Text = "";
            DateCreated.Text = "";
            TimeCreated.Text = "";
            IsHidden.Content = "";
            IsHidden.IsChecked = false;
            IsReadOnly.Content = "";
            IsReadOnly.IsChecked = false;
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
            button.Height = 42;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.BorderThickness = new Thickness(0);
            button.Margin = new Thickness(20 * node.Level, 5, 0, 5);
            button.Tag = node.Info.ID;
            button.Click += infoButtonClick;

            Image image = new Image();
            image.Width = 39;
            image.Height = 39;
            image.Margin = new Thickness(5, 0, 0, 0);
            image.Stretch = Stretch.UniformToFill;


            DockPanel dockPanel = new DockPanel();
            dockPanel.Width = 600;
            dockPanel.HorizontalAlignment = HorizontalAlignment.Left;

            Button expand = new Button();

            expand.Style = (Style)this.FindResource("TreeExpandButton");
            expand.Tag = node.Info.ID;
            
           
            expand.BorderThickness = new Thickness(0);
            expand.Background = Brushes.Transparent;
            expand.FontSize = 12;

            if (node.Info.IsDirectory == true)
                button.MouseDoubleClick += (sender, e) => expandButtonClick(expand, e, image);


            TextBlock textBlock = new TextBlock();
            textBlock.Text = node.Info.FileName;
            textBlock.Margin = new Thickness(12, 0, 0, 0);
            textBlock.Foreground = Brushes.Black;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.FontSize = 15;
            if (node.Info.IsSystem == true)
            {
                image.Source = new BitmapImage(new Uri(@"/resources/system.png", UriKind.RelativeOrAbsolute));

            }
            else if (node.Info.IsDirectory == false)
            {
                string ex = Function.getFilenameExtension(node.Info.FileName);


                if (Function.extension.ContainsKey(ex))
                    image.Source = new BitmapImage(Function.extension[ex]);
                else
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
                expand.Click += (s, e) => expandButtonClick(s, e, image);
                expand.MouseDoubleClick += (s, e) => expandButtonClick(s, e, image);
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
            FolderTreeContain.Children.Clear();
            var rootList = FolderTree.getRootsSortedByName();
            foreach (var root in rootList)
            {
                renderANode(root.Value, FolderTreeContain);
            }
        }

        private void infoButtonClick(object sender, EventArgs e)
        {
            isPartitionClicked = false;
            long id = (long)(sender as Button).Tag;
            FileInfomation file = FolderTree.ListOfFiles[id].Info;

            if (file.IsSystem == true)
            {
                FileImage.Source = new BitmapImage(new Uri(@"/resources/system.png", UriKind.RelativeOrAbsolute));

            }
            else if (file.IsDirectory == false)
            {

                string ex = Function.getFilenameExtension(file.FileName);

                if (Function.extension.ContainsKey(ex))
                    FileImage.Source = new BitmapImage(Function.extension[ex]);
                else
                    FileImage.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));

            }
            else
                FileImage.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));

            FileSize.Text = "Size: " + Function.toFileSize(FolderTree.getSize(id));
            Function.getFilenameExtension(file.FileName);
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

        private void expandButtonClick(object sender, EventArgs e, object image)
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

                (image as Image).Source = new BitmapImage(new Uri(@"/resources/open-folder.png", UriKind.RelativeOrAbsolute));

                var childrenList = FolderTree.getChildrenSortedByName(id);
                foreach (var child in childrenList)
                    renderANode(child.Value, area);

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

                (image as Image).Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));


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

        static public Dictionary<string, Uri> extension = new Dictionary<string, Uri>()
        {
            {"css",new Uri("/resources/icons/css.png",UriKind.RelativeOrAbsolute) },
            {"doc",new Uri("/resources/icons/doc.png",UriKind.RelativeOrAbsolute) },
            {"docx",new Uri("/resources/icons/doc.png",UriKind.RelativeOrAbsolute) },
            {"exe",new Uri("/resources/icons/exe.png",UriKind.RelativeOrAbsolute) },
            {"html",new Uri("/resources/icons/html.png",UriKind.RelativeOrAbsolute) },
            {"jpg",new Uri("/resources/icons/jpg.png",UriKind.RelativeOrAbsolute) },
            {"json",new Uri("/resources/icons/json.png",UriKind.RelativeOrAbsolute) },
            {"mp3",new Uri("/resources/icons/mp3.png",UriKind.RelativeOrAbsolute) },
            {"mp4",new Uri("/resources/icons/mp4.png",UriKind.RelativeOrAbsolute) },
            {"pdf",new Uri("/resources/icons/pdf.png",UriKind.RelativeOrAbsolute) },
            {"ppt",new Uri("/resources/icons/ppt.png",UriKind.RelativeOrAbsolute) },
            {"pptx",new Uri("/resources/icons/ppt.png",UriKind.RelativeOrAbsolute) },
            {"txt",new Uri("/resources/icons/txt.png",UriKind.RelativeOrAbsolute) },
            {"xml",new Uri("/resources/icons/xml.png",UriKind.RelativeOrAbsolute) },
        };

        public static FileStream stream = null;
        static public long littleEndian(byte[] src, int offset, int length)
        {
            long res = 0;
            for (int i = length - 1; i >= 0; --i)
            {
                res <<= 8;
                res += (long)src[offset + i];
            }
            return res;
        }

        static public long littleEndian(byte[] src, long offset, long length)
        {
            long res = 0;
            for (long i = length - 1; i >= 0; --i)
            {
                res <<= 8;
                res += (long)src[offset + i];
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

        static public string getFilenameExtension(string fileName)
        {
            int i = 0;
            string ex = "";
            for (i = fileName.Length - 1; i >= 0; --i)
            {
                if (fileName[i] == '\u002E')
                    break;
            }
            if (i == 0)
                return "";
            ++i;
            for (; i < fileName.Length; ++i)
            {
                ex += fileName[i];
            }
            
            if(ex[ex.Length - 1] < '\u0031' || ex[ex.Length - 1] > '\u007a')
                ex.Remove(ex.Length - 1, 1);

           



            return ex.TrimEnd();
        }

    }
}
