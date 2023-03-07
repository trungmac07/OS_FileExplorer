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

namespace FileExplorer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        public MainWindow()
        {
            InitializeComponent();
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
            Button DiskButton1 = new Button();

            DockPanel dcPanel1 = new DockPanel();

            TextBlock txt = new TextBlock();
            Image img = new Image();

            img.Height = 50;
            img.Width = 50;
            if(kind == 0)
                img.Source = new BitmapImage(new Uri("/resources/hdd.png",UriKind.RelativeOrAbsolute));
            else if(kind == 1)
                img.Source = new BitmapImage(new Uri("/resources/usb.png", UriKind.RelativeOrAbsolute));

            img.Stretch = Stretch.Fill;
        

            txt.Margin = new Thickness(50, 0, 0, 0);
            txt.Width = 160;
            txt.FontSize = 30;
            txt.Text = s;

            dcPanel1.Children.Add(img);
            dcPanel1.Children.Add(txt);
            
            DiskButton1.Height = 80;
            DiskButton1.Content = dcPanel1;
            DiskButton1.Style = (Style)this.FindResource("MenuButton");
            DiskArea.Children.Add(DiskButton1);
        }
    }
    
}
