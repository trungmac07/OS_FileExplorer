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

            if (file.Type <= 1)
                FileImage.Source = new BitmapImage(new Uri(@"/resources/file.png", UriKind.RelativeOrAbsolute));
            else
                FileImage.Source = new BitmapImage(new Uri(@"/resources/folder.png", UriKind.RelativeOrAbsolute));


            FileName.Text = file.FileName;


            DateCreated.Text = "Created Date: " + file.CreatedTime.ToLocalTime().ToString("dd/MM/yyyy");
            TimeCreated.Text = "Created Time: " + file.CreatedTime.ToLocalTime().ToString("HH:mm:ss");

            LastModifyDate.Text = "Last Modified Date: " + file.LastModifiedTime.ToLocalTime().ToString("dd/MM/yyyy");
            LastModifyTime.Text = "Last Modified Time: " + file.LastModifiedTime.ToLocalTime().ToString("HH:mm:ss");


            IsHidden.IsChecked = file.IsHidden;
            IsReadOnly.IsChecked = file.IsReadOnly;
            IsSystem.IsChecked = file.IsSystem;
            IsArchive.IsChecked = file.IsArchive;
            IsDirectory.IsChecked = file.IsDirectory;
        }

    }
}
