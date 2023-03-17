using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;


namespace FileExplorer
{


    internal partial class NTFS
    {

        public enum OffsetVBR
        {
            BYTES_PER_SECTOR = 0x0b,
            SECTORS_PER_CLUSTER = 0x0d,
            SECTORS_PER_TRACK = 0x18,
            NUMBER_OF_HEADS = 0x1a,
            NUMBER_OF_SECTORS = 0x28,
            BEGIN_CLUSTER_1 = 0x30,
            BEGIN_CLUSTER_2 = 0x38,
            BYTES_PER_ENTRY = 0x40,
        }

        public enum LengthVBR
        {
            BYTES_PER_SECTOR = 2,
            SECTORS_PER_CLUSTER = 1,
            SECTORS_PER_TRACK = 2,
            NUMBER_OF_HEADS = 2,
            NUMBER_OF_SECTORS = 8,
            BEGIN_CLUSTER_1 = 8,
            BEGIN_CLUSTER_2 = 8,
            BYTES_PER_ENTRY = 1,
        }

        private long CurrentDisk { get; set; }

        private long BytesPerSector { get; set; }
        private long SectorsPerCluster { get; set; }
        private long SectorsPerTrack { get; set; }
        private long NumberOfHeads { get; set; }
        private long NumberOfSectors { get; set; }
        private long BeginCluster1 { get; set; }
        private long BeginCluster2 { get; set; }
        private long BytesPerEntry { get; set; }
        SortedSet<MFTEntry> MFTEntries;

        public Dictionary<long, FolderTreeNode> ListOfRoots { get; set; }
        public Dictionary<long,FolderTreeNode> ListOfFiles { get; set; }


        FileStream stream = null;

        private long FirstByte { get; set; }
        public NTFS(long firstSector, long totalSector, int currentDisk)
        {
            CurrentDisk = currentDisk;
            FirstByte = firstSector * 512;
            long length = totalSector * 512;
            byte[] vbr = new byte[512];
            
            //string drivePath = @"\\.\PhysicalDrive" + CurrentDisk.ToString();

            // Open the drive for reading
            try
            {
                //stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                // Read the VBR
                Function.stream.Seek(FirstByte, SeekOrigin.Begin);
                Function.stream.Read(vbr, 0, 512);
               
            }
            catch (FileNotFoundException) { };

            BytesPerSector      =   Function.littleEndian(vbr,  (int)OffsetVBR.BYTES_PER_SECTOR,     (int)LengthVBR.BYTES_PER_SECTOR);
            SectorsPerCluster   =   Function.littleEndian(vbr,  (int)OffsetVBR.SECTORS_PER_CLUSTER,  (int)LengthVBR.SECTORS_PER_CLUSTER);
            SectorsPerTrack     =   Function.littleEndian(vbr,  (int)OffsetVBR.SECTORS_PER_TRACK,    (int)LengthVBR.SECTORS_PER_TRACK);
            NumberOfHeads       =   Function.littleEndian(vbr,  (int)OffsetVBR.NUMBER_OF_HEADS,      (int)LengthVBR.NUMBER_OF_HEADS);
            NumberOfSectors     =   Function.littleEndian(vbr,  (int)OffsetVBR.NUMBER_OF_SECTORS,    (int)LengthVBR.NUMBER_OF_SECTORS);
           
            BeginCluster1       =   Function.littleEndian(vbr,  (int)OffsetVBR.BEGIN_CLUSTER_1,      (int)LengthVBR.BEGIN_CLUSTER_1);
            BeginCluster2       =   Function.littleEndian(vbr,  (int)OffsetVBR.BEGIN_CLUSTER_2,      (int)LengthVBR.BEGIN_CLUSTER_2);
            //2's complement 

            int rawValue = vbr[(int)OffsetVBR.BYTES_PER_ENTRY];
            int negativeMask = 0x00000080;
            int invertMask = ~0x000000ff;

            if ((rawValue & negativeMask) > 0)          // with a negative number
            {
                rawValue |= invertMask;                 // change the left 24 bits to 1
                rawValue = ~rawValue; ++rawValue;       // invert bit, +1    
            }
            BytesPerEntry = (long)Math.Pow(2, rawValue); //2^
            MFTEntries = new SortedSet<MFTEntry>();
            ListOfRoots = new Dictionary<long, FolderTreeNode> ();
            ListOfFiles = new Dictionary<long, FolderTreeNode> ();

        }
       
        public void readAttribute()
        {
            long beginByte = (FirstByte + BeginCluster1 * SectorsPerCluster * BytesPerSector);
            int j = 0;
            for(long i=beginByte; ;i+=BytesPerEntry)
            {
                ++j;
                MFTEntry mFTEntry = new MFTEntry(i, CurrentDisk, BytesPerEntry);
                mFTEntry.print();
                if(mFTEntry.Sign == "FILE")
                {
                    
                    MFTEntries.Add(mFTEntry);
                    addToTree(new FileInfomation(mFTEntry));
                }
                else if (mFTEntry.Sign != "BAAD" && j >= 32)
                {
                    break;
                }
            }
        }

        public void addToTree(FileInfomation src)
        {
            if (src == null)
                return;
            FolderTreeNode newNode = new FolderTreeNode(src);
      
            if (src.IDParentFolder == 5)
            {
                ListOfRoots.Add(src.ID,newNode);
            }
            else
            {
                if(ListOfFiles.ContainsKey(src.IDParentFolder))
                {
                    if (ListOfRoots.ContainsKey(src.IDParentFolder))
                        ListOfRoots[src.IDParentFolder].Children.Add(src.ID);
                    ListOfFiles[src.IDParentFolder].Children.Add(src.ID);
                }
                else
                {
                    ListOfFiles[src.IDParentFolder] = new FolderTreeNode(src.IDParentFolder);
                    ListOfFiles[src.IDParentFolder].Children.Add(src.ID);
                }
                
            }
            if (ListOfFiles.ContainsKey(src.ID))
                ListOfFiles[src.ID].Info = src;
            else
                ListOfFiles[src.ID] = newNode;
            if(src.IDParentFolder != 5)
                ListOfFiles[src.ID].Level = ListOfFiles[src.IDParentFolder].Level + 1;

        }

        public void showTree()
        {
            Console.WriteLine("___________________________________");
            foreach (MFTEntry mFTEntry in MFTEntries)
                mFTEntry.printInfo();

            /*foreach (var node in ListOfFiles)
                node.Value.showInfo();*/
           
        }



        public void printVBRInfo()
        {
            Console.WriteLine("Bytes per sector: " + BytesPerSector);
            Console.WriteLine("Sector per cluster: " + SectorsPerCluster);
            Console.WriteLine("Sector per track " + SectorsPerTrack);
            Console.WriteLine("Number of heads: " + NumberOfHeads);
            Console.WriteLine("Number of sectors: " + NumberOfSectors);
            Console.WriteLine("Begin cluster 1: " + BeginCluster1);
            Console.WriteLine("Begin cluster 2: " + BeginCluster2);
            Console.WriteLine("Bytes per entry: " + BytesPerEntry);
        }



    }

}
