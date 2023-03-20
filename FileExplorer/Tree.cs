﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    public class Tree
    {
        public Dictionary<long, FolderTreeNode> ListOfRoots { get; set; }
        public Dictionary<long, FolderTreeNode> ListOfFiles { get; set; }
        
        public Tree()
        {
            ListOfRoots = new Dictionary<long, FolderTreeNode>();
            ListOfFiles = new Dictionary<long, FolderTreeNode>();
        }

        public long getSize(long id)
        {
            if(ListOfFiles.ContainsKey(id))
            {
                if(ListOfFiles[id].Info.IsDirectory == true)
                {
                    long sum = 0;
                    foreach(var child in ListOfFiles[id].Children)
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

        public long getSizeOnDisk(long id)
        {
            if (ListOfFiles.ContainsKey(id))
            {
                if (ListOfFiles[id].Info.IsDirectory == true)
                {
                    long sum = 0;
                    foreach (var child in ListOfFiles[id].Children)
                        sum += getSizeOnDisk(child);
                }
                else
                {
                    return ListOfFiles[id].Info.SizeOnDisk;
                }
            }
            return -1;
        }


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

            foreach (var child in ListOfFiles[src.ID].Children)
            {
                ListOfFiles[child].Level = ListOfFiles[src.ID].Level + 1;
            }


        }

    }
}
