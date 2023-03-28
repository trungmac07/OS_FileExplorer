using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    public class Tree
    {
        public long IsNTFS { get; set; } = 0;
        public Dictionary<long, FolderTreeNode> ListOfRoots { get; set; }
        public Dictionary<long, FolderTreeNode> ListOfFiles { get; set; }

        public Tree()
        {
            ListOfRoots = new Dictionary<long, FolderTreeNode>();
            ListOfFiles = new Dictionary<long, FolderTreeNode>();
        }

        //Return size of a file or folder
        //If a file -> just return its size
        //If a folder -> return it owns files
        public long getSize(long id)
        {
            if (ListOfFiles.ContainsKey(id))
            {
                if (ListOfFiles[id].Info.IsDirectory == true)
                {
                    long sum = 0;
                    foreach (var child in ListOfFiles[id].Children)
                        sum += getSize(child);

                    return sum;
                }
                else
                {
                    return ListOfFiles[id].Info.Size;
                }
            }
            return -1;
        }

        //size on disk
        public long getSizeOnDisk(long id)
        {
            if (ListOfFiles.ContainsKey(id))
            {
                if (ListOfFiles[id].Info.IsDirectory == true)
                {
                    long sum = 0;
                    foreach (var child in ListOfFiles[id].Children)
                        sum += getSizeOnDisk(child);
                    return sum;
                }
                else
                {
                    return ListOfFiles[id].Info.SizeOnDisk;
                }
            }
            return -1;
        }

        //used bytes of this tree
        public long sizeOnDiskOfTree()
        {
            long sum = 0;
            //sum += IsNTFS;
            if (IsNTFS != 0)
                foreach (var root in ListOfRoots)
                {
                    if (root.Key!=8)
                        sum += getSizeOnDisk(root.Key);
                }
            else
                foreach (var root in ListOfRoots)
                {
                    sum += getSizeOnDisk(root.Key);
                }
            return sum;
        }

        //add a file to tree
        public void addToTree(FileInfomation src)
        {
            //null
            if (src == null)
                return;
            FolderTreeNode newNode = new FolderTreeNode(src);


            //is root
            if (src.IsRoot == true)
            {
                ListOfRoots.Add(src.ID, newNode);
            }
            else //not a root
            {
                //parent existed
                if (ListOfFiles.ContainsKey(src.IDParentFolder))
                {
                    ListOfFiles[src.IDParentFolder].Children.Add(src.ID);
                }
                else //parent not existed
                {
                    //create new parent with no info but a children list
                    ListOfFiles[src.IDParentFolder] = new FolderTreeNode(src.IDParentFolder);
                    ListOfFiles[src.IDParentFolder].Children.Add(src.ID);
                }
            }
            //if exist this -> only update info
            if (ListOfFiles.ContainsKey(src.ID))
            {
                ListOfFiles[src.ID].Info = src;
                foreach (var child in ListOfFiles[src.ID].Children)
                {
                    ListOfFiles[child].Level = ListOfFiles[src.ID].Level + 1;
                }
            }
            else
                ListOfFiles[src.ID] = newNode;

            //level for margin
            if (src.IDParentFolder != 5)
                ListOfFiles[src.ID].Level = ListOfFiles[src.IDParentFolder].Level + 1;

            //update level
            foreach (var child in ListOfFiles[src.ID].Children)
            {
                ListOfFiles[child].Level = ListOfFiles[src.ID].Level + 1;
            }


        }


        //Sorted Dictionary for building a tree ordered by filename
        public SortedDictionary<FileIdentifier, FolderTreeNode> getRootsSortedByName()
        {
            SortedDictionary<FileIdentifier, FolderTreeNode> rootsSortedByName = new SortedDictionary<FileIdentifier, FolderTreeNode>();

            foreach (var root in ListOfRoots)
            {
                rootsSortedByName.Add(new FileIdentifier(root.Value.Info.ID, root.Value.Info.FileName, root.Value.Info.IsSystem), root.Value);
            }

            return rootsSortedByName;
        }

        public SortedDictionary<FileIdentifier, FolderTreeNode> getChildrenSortedByName(long id)
        {
            SortedDictionary<FileIdentifier, FolderTreeNode> filesSortedByName = new SortedDictionary<FileIdentifier, FolderTreeNode>();

            foreach (var child in ListOfFiles[id].Children)
            {
                filesSortedByName.Add(new FileIdentifier(ListOfFiles[child].Info.ID, ListOfFiles[child].Info.FileName, ListOfFiles[child].Info.IsSystem), ListOfFiles[child]);
            }

            return filesSortedByName;
        }

    }
}
