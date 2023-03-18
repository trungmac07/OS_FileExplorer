using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    public class FileInfomation
    {
        public bool IsRoot { get; set; }
        public string FileName { get; set; } 
        public long ID { get; set;}
        public long IDParentFolder { get; set; }
        public bool IsReadOnly { get; set;}
        public bool IsHidden { get; set;}
        public bool IsArchive { get; set;}
        public bool IsSystem { get; set;}
        public bool IsDirectory { get; set;}
        public DateTime CreatedTime { get; set;}
        public DateTime LastModifiedTime { get; set;}
        public long Size { get; set;}
        public long SizeOnDisk { get; set;}
        public int Type { get; set;} //0-file/ 1-folder

        public FileInfomation(NTFS.MFTEntry e)
        {
            e.export(this);
            if (IDParentFolder == 5)
                IsRoot = true;
        }
        public FileInfomation(long id)
        {
            ID = id;
        }
        public void showInfo()
        {
            Console.WriteLine("FileName: " + FileName);
            Console.WriteLine("ID: " + ID);
            Console.WriteLine("IDParent: " + IDParentFolder);
        }
    }
}
