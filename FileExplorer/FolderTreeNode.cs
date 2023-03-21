using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
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
