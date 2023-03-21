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

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for PopUp.xaml
    /// </summary>
    public partial class PopUp : Window
    {
        public PopUp()
        {
            InitializeComponent();
        }

        public PopUp(FileInfomation file)
        {
            InitializeComponent();
            this.Title = file.FileName;

            if (file.IsDirectory == false)
                FileImage.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));
            else
                FileImage.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));

            long id = file.ID;

            FileSize.Text = Function.toFileSize(file.Size) + " (" + file.Size +" Bytes)";
            OnDiskSize.Text = Function.toFileSize(file.SizeOnDisk) + " (" + file.SizeOnDisk + " Bytes)";

            

            FileName.Text = file.FileName;


            DateCreated.Text = file.CreatedTime.ToLocalTime().ToString("dd/MM/yyyy");
            TimeCreated.Text = file.CreatedTime.ToLocalTime().ToString("HH:mm:ss");

            LastModifyDate.Text = file.LastModifiedTime.ToLocalTime().ToString("dd/MM/yyyy");
            LastModifyTime.Text = file.LastModifiedTime.ToLocalTime().ToString("HH:mm:ss");


            IsHidden.IsChecked = file.IsHidden;
            IsReadOnly.IsChecked = file.IsReadOnly;
            IsSystem.IsChecked = file.IsSystem;
            IsArchive.IsChecked = file.IsArchive;
            IsDirectory.IsChecked = file.IsDirectory;
        }

    }
}
