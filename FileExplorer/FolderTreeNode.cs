using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    public class FileIdentifier : IComparable<FileIdentifier>
    {
        public long ID { get; set; }
        public string FileName { get; set; }
        public bool IsSystem { get; set; }

        public FileIdentifier(long id, string name, bool system)
        {
            ID = id;
            FileName = name;
            IsSystem = system;
        }

        public int CompareTo(FileIdentifier other)
        {
            if (this.IsSystem ^ other.IsSystem == true)
            {
                if (this.IsSystem == true)
                    return (int)2e9;
                else
                    return (int)-2e9;
            }
            else
            {
                if (this.FileName == other.FileName)
                    return this.ID.CompareTo(other.ID);
                else
                    return this.FileName.CompareTo(other.FileName);
            }

        }

        public int Compare(FileIdentifier node2)
        {
            return this.CompareTo(node2);
        }

        


    }
    public class FolderTreeNode
    {
        
        public List<long> Children { get; set; } = null;
        public FileInfomation Info { get; set; } = null;
        public int Level { get; set; } = 0;

        //contructor with info
        public FolderTreeNode(FileInfomation src)
        {
            Children = new List<long>();
            Info = src;
        }

        //contructor only with file id
        public FolderTreeNode(long ID)
        {
            Children = new List<long>();
            Info = new FileInfomation(ID);
        }

        //show info for testing 
        public void showInfo()
        {
            Info.showInfo();
            foreach (var child in Children)
                Console.Write(child + " ");
            Console.WriteLine();
        }

       

    }
}
