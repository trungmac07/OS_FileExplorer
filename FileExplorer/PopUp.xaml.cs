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

        public PopUp(Tree folderTree, long id)
        {
            InitializeComponent();

            FileInfomation file = folderTree.ListOfFiles[id].Info;

            this.Title = file.FileName;

            if (file.IsDirectory == false)
                FileImage.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));
            else
                FileImage.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));

            long size = folderTree.getSize(id);
            long sizeOnDisk = folderTree.getSizeOnDisk(id);


            FileSize.Text = Function.toFileSize(size) + " (" + size + " Bytes)";
            OnDiskSize.Text = Function.toFileSize(sizeOnDisk) + " (" + sizeOnDisk + " Bytes)";

            

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
