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
        public FolderTreeNode(FileInfomation src)
        {
            Children = new List<long>();
            Info = src;
        }
        public FolderTreeNode(long ID)
        {
            Children = new List<long>();
            Info = new FileInfomation(ID);
        }

        public void showInfo()
        {
            Info.showInfo();
            foreach (var child in Children)
                Console.Write(child + " ");
            Console.WriteLine();
        }


    }
}
