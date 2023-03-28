using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal partial class FAT32
    {
        public enum OffsetBST
        {
            BYTES_PER_SECTOR = 0x0b,
            SECTORS_PER_CLUSTER = 0x0d,
            SECTORS_BEFORE_FAT = 0x0e,
            NUMBER_OF_FATS = 0x10,
            VOLUME = 0x20,
            SECTORS_PER_FAT = 0x24,
            BEGGIN_CLUSTER = 0x2c,
        }
        public enum LengthBST
        {
            BYTES_PER_SECTOR = 2,
            SECTORS_PER_CLUSTER = 1,
            SECTORS_BEFORE_FAT = 2,
            NUMBER_OF_FATS = 1,
            VOLUME = 4,
            SECTORS_PER_FAT = 4,
            BEGGIN_CLUSTER = 4,
        }
        public long firstSectorOfPartition { get; set; }
        public long bytesPerSector { get; set; }
        public long sectorsPerCluster { get; set; }
        public long sectorsBeforeFAT { get; set; }
        public long numberOfFATs { get; set; }
        public long volume { get; set; }
        public long sectorsPerFAT { get; set; }
        public long begginCluster { get; set; }
        public string diskPath { get; set; }
        public Dictionary<long ,long > FAT {get; set;}
        public FAT32(long firstSector, int CurrentDisk)
        {
            diskPath = @"\\.\PhysicalDrive" + CurrentDisk.ToString();
            firstSectorOfPartition = firstSector * 512;
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fs.Seek(firstSectorOfPartition, SeekOrigin.Begin);
            byte[] BST = new byte[512];
            fs.Read(BST, 0, BST.Length);
            bytesPerSector = Function.littleEndian(BST, (int)OffsetBST.BYTES_PER_SECTOR, (int)LengthBST.BYTES_PER_SECTOR);
            sectorsPerCluster = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_CLUSTER, (int)LengthBST.SECTORS_PER_CLUSTER);
            sectorsBeforeFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_BEFORE_FAT, (int)LengthBST.SECTORS_BEFORE_FAT);
            numberOfFATs = Function.littleEndian(BST, (int)OffsetBST.NUMBER_OF_FATS, (int)LengthBST.NUMBER_OF_FATS);
            volume = Function.littleEndian(BST, (int)OffsetBST.VOLUME, (int)LengthBST.VOLUME);
            sectorsPerFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_FAT, (int)LengthBST.SECTORS_PER_FAT);
            begginCluster = Function.littleEndian(BST, (int)OffsetBST.BEGGIN_CLUSTER, (int)LengthBST.BEGGIN_CLUSTER);
            FAT = new Dictionary<long, long>();
            fs.Close();
        }
        
        public string NameDisk()
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            long FirstSectorInRDET = 0;
            FirstSectorInRDET += this.sectorsBeforeFAT;
            FirstSectorInRDET += this.sectorsPerFAT * this.numberOfFATs;
            byte[] a = new byte[this.bytesPerSector];
            fs.Seek(FirstSectorInRDET * this.bytesPerSector + firstSectorOfPartition, SeekOrigin.Begin);
            fs.Read(a, 0, 192);

            string s = "";
            for (int i = 0; i <= 12; i++)
            {
                s += (char)a[i];
            }
            fs.Close();
            return s;
        }
        public void GetAllFiles(FolderTreeNode node, Dictionary<long, FolderTreeNode> Files, int level)
        {
            Files.Add(node.Info.ID, node);
            if (node.Children.Count == 0) return;
            else
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    int a = level + 1;
                    GetAllFiles(ReadFile(node.Children[i], node.Info.ID, a), Files, a);
                }
            }
        }
        public string subEntryName(byte[] arr)
        {
            string s = "";
            byte[] a = new byte[32];
            int index = 0;
            for (int i = 0x01; i < 0x01 + 10; i++)
            {
                if (arr[i] == 0xFF) break;
                a[index] = arr[i];
                index++;
            }

            for (int i = 0x0E; i < 0x0E + 12; i++)
            {
                if (arr[i] == 0xFF) break;
                a[index] = arr[i];
                index++;
            }


            for (int i = 0x1C; i < 0x1C + 4; i++)
            {
                if (arr[i] == 0xFF) break;
                a[index] = arr[i];
                index++;
            }

            s = Encoding.Unicode.GetString(a);
            return s;
        }
        public string mainEntryName(byte[] arr)
        {
            string s = "";

            string s1 = "";
            for (int i = 0x00; i < 0x00 + 8; i++)
            {
                if (arr[i] == 0xFF) break;
                s += (char)arr[i];
            }
            while (true)
            {
                if (s[s.Length - 1] == 0x20)
                {
                    s = s.Substring(0, s.Length - 1);
                }
                else break;
            }

            for (int i = 0x08; i < 0x08 + 3; i++)
            {
                if (arr[i] == 0xFF) break;
                s1 += (char)arr[i];
            }
            if (arr[0x0B] == 0x10) s = s + s1.ToLower();
            else s = s + "." + s1.ToLower();
            return s;
        }

        public DateTime createTime(byte a, byte b, byte c, byte d, byte e,long pos)
        {

            string s1 = Convert.ToString(a, 2);
            string s2 = Convert.ToString(b, 2);
            string s3 = Convert.ToString(c, 2);
            while (s1.Length < 8)
            {
                s1 = "0" + s1;
            }
            while (s2.Length < 8)
            {
                s2 = "0" + s2;
            }
            while (s3.Length < 8)
            {
                s3 = "0" + s3;
            }
            string s = s1 + s2 + s3;

            int gio, phut, giay, miligiay;
            gio = Convert.ToInt32(s.Substring(0, 5), 2);
            phut = Convert.ToInt32(s.Substring(5, 6), 2);
            giay = Convert.ToInt32(s.Substring(11, 6), 2);
            miligiay = Convert.ToInt32(s.Substring(17, 7), 2);

            string s4 = Convert.ToString(d, 2);
            string s5 = Convert.ToString(e, 2);

            while (s4.Length < 8)
            {
                s4 = "0" + s4;
            }
            while (s5.Length < 8)
            {
                s5 = "0" + s5;
            }

            s = s4 + s5;

            int nam, thang, ngay;
            nam = Convert.ToInt32(s.Substring(0, 7), 2) + 1980;
            thang = Convert.ToInt32(s.Substring(7, 4), 2);
            ngay = Convert.ToInt32(s.Substring(11, 5), 2);
            DateTime temp = new DateTime(nam, thang, ngay, gio, phut, giay);
            return temp;

        }
        public string createDate(byte a, byte b)
        {

            string s1 = Convert.ToString(a, 2);
            string s2 = Convert.ToString(b, 2);

            while (s1.Length < 8)
            {
                s1 = "0" + s1;
            }
            while (s2.Length < 8)
            {
                s2 = "0" + s2;
            }

            string s = s1 + s2;

            int nam, thang, ngay;
            nam = Convert.ToInt32(s.Substring(0, 7), 2) + 1980;
            thang = Convert.ToInt32(s.Substring(7, 4), 2);
            ngay = Convert.ToInt32(s.Substring(11, 5), 2);
            s1 = Convert.ToString(thang);
            s2 = Convert.ToString(ngay);
            if (s1.Length < 2) s1 = "0" + s1;
            if (s2.Length < 2) s2 = "0" + s2;

            s = Convert.ToString(nam) + "/" + s1 + "/" + s2;
            return s;
        }
        public long size(byte a, byte b, byte c, byte d)
        {
            byte[] arr = { a, b, c, d };
            long s = Function.littleEndian(arr, 0, 4);
            return s;
        }
        public Tree readRoot()
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Tree rs = new Tree();
            //Get list of cluster need to use
            long[] clusterArr = new long[10000];
            long dem = 0;
            long clusterData = 2;
            byte[] a = new byte[this.bytesPerSector];
            while (clusterData > 0)
            {
                //2 - 473 - 875
                clusterArr[dem] = clusterData;
                dem++;
                clusterData = nextClusterFromFAT(clusterData);
            }
            for(int i = 1; i < dem; i++)
            {
                FAT.Add(clusterArr[i], clusterArr[i - 1]);
            }
            //
            int level = 0;
            long pos = 0;
            for(long i = 0; i < dem; i++)
            {
                fs.Seek(getFirstByteOfCluster(clusterArr[i]), SeekOrigin.Begin);
                fs.Read(a, 0, 512);
                long count = bytesPerSector * sectorsPerCluster;
                fs.Seek(getFirstByteOfCluster(clusterArr[i]), SeekOrigin.Begin);
                if (i == 0)
                {
                    fs.Seek(getFirstByteOfCluster(clusterArr[i]) + 128, SeekOrigin.Begin);
                    count -= 128;
                }
                fs.Read(a, 0, 32);
                //count -= 32;
                while (a[0] != 0 && a[0] != 0x2E && count > 0)
                {
                    if (a[0x00] != 0xE5 && a[0x0B] != 0x0F)
                    {
                        pos = fs.Position - 32;
                        fs.Close();
                        FolderTreeNode temp = ReadFile(pos, 0, level);
                        rs.ListOfRoots.Add(temp.Info.ID, temp);
                        GetAllFiles(temp, rs.ListOfFiles, level);
                        fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        fs.Seek(pos + 32, SeekOrigin.Begin);
                        long offset = pos + 32;
                        long x = offset / bytesPerSector;
                        long y = x * bytesPerSector;
                        int z = (int)(offset - y);
                        fs.Seek(y, SeekOrigin.Begin);
                        fs.Read(a, 0, z);
                        fs.Seek(offset, SeekOrigin.Begin);
                        fs.Read(a, 0, 32);
                    }
                    else
                    {
                        fs.Read(a, 0, 32);
                    }
                    count -= 32;
                }
            }
            return rs;
        }
        public FolderTreeNode ReadFile(long FirstByte, long Parent, int level)
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long realPOS = 0;
            long soDu = 0;
            long sectorPOS = 0;
            long bytesPerCluster = bytesPerSector * sectorsPerCluster;
            byte[] a = new byte[bytesPerCluster];
            FileInfomation FileTemp = new FileInfomation(FirstByte);
            //get the sector--------------------------------

            realPOS = FirstByte;
            soDu = realPOS / 512;
            sectorPOS = soDu * 512;
            fs.Seek(sectorPOS, SeekOrigin.Begin);
            fs.Read(a, 0, 512);
            fs.Seek(realPOS, SeekOrigin.Begin);
            fs.Read(a, 0, 32);


            fs.Seek(FirstByte, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            DateTime time = createTime(a[0x0F], a[0x0E], a[0x0D], a[0x11], a[0x10],FirstByte);
            long s = size(a[0x1C], a[0x1D], a[0x1E], a[0x1F]);
            long cluster = Function.littleEndian(a, 0x1A, 2);

            if (Parent == 0) FileTemp.IsRoot = true;
            else FileTemp.IsRoot = false;
            FileTemp.IDParentFolder = Parent;
            if (a[0x0B] == 0x01) FileTemp.IsReadOnly = true;
            else FileTemp.IsReadOnly = false;
            if (a[0x0B] == 0x02) FileTemp.IsHidden = true;
            else FileTemp.IsHidden = false;
            if (a[0x0B] == 0x04) FileTemp.IsSystem = true;
            else FileTemp.IsSystem = false;
            if (a[0x0B] == 0x10) FileTemp.IsDirectory = true;
            else FileTemp.IsDirectory = false;
            if (a[0x0B] == 0x20) FileTemp.IsArchive = true;
            else FileTemp.IsArchive = false;

            FileTemp.CreatedTime = time;
            FileTemp.Size = s;

            if (FileTemp.IsArchive == true)
            {
                FileTemp.Type = 0;
                if (FileTemp.Size % bytesPerCluster != 0)
                    FileTemp.SizeOnDisk = (FileTemp.Size / bytesPerCluster + 1) * bytesPerCluster;
                else FileTemp.SizeOnDisk = (FileTemp.Size / bytesPerCluster) * bytesPerCluster;
            }
            else FileTemp.Type = 1;
            fs.Close();
            //Tim cach doc ten
            fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileTemp.FileName = readName(FirstByte);
            fs.Close();
            //Children-------------------------
            fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<long> Children = new List<long>();
            if (FileTemp.IsDirectory == true)
            {
                long[] clusterArr = new long[10000];
                long dem = 0;
                long clusterData = cluster; //cluster;
                while (clusterData > 0)
                {
                    clusterArr[dem] = clusterData;
                    dem++;
                    clusterData = nextClusterFromFAT(clusterData);
                }
                for (int i = 1; i < dem; i++)
                {
                    FAT.Add(clusterArr[i], clusterArr[i - 1]);
                }
                for (long i = 0; i < dem; i++)
                {
                    fs.Seek(getFirstByteOfCluster(clusterArr[i]), SeekOrigin.Begin);
                    fs.Read(a, 0, 512);
                    if (i == 0)
                    {
                        fs.Seek(getFirstByteOfCluster(clusterArr[i]) + 64, SeekOrigin.Begin);
                    }
                    long count = bytesPerCluster;
                    while(count > 0)
                    {
                        fs.Read(a, 0, 32);
                        if (a[0] == 0x00) break;
                        if (a[0x00] != 0xE5 && a[0x0B] != 0x0F && a[0x00] != 0x00) Children.Add(fs.Position - 32);
                        count -= 32;
                    }
                }
            }
            //return
            FolderTreeNode res = new FolderTreeNode(FileTemp);
            res.Children = Children;
            res.Level = level;
            fs.Close();
            return res;
        }
        public string readName(long FirstByte)
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long bytesPerCluster = bytesPerSector * sectorsPerCluster;
            long firstByteOfDATA = (sectorsBeforeFAT + sectorsPerFAT * numberOfFATs) * bytesPerSector + firstSectorOfPartition;
            long currentCluster = (FirstByte - firstByteOfDATA) / bytesPerCluster + 2;
            long clusterBefore = -1;
            if(FAT.ContainsKey(currentCluster))
            {
                clusterBefore = FAT[currentCluster];
            }
            string name = "";
            long realPOS = 0;
            long soDu = 0;
            long sectorPOS = 0;
            byte[] a = new byte[512];
            //Agorithm
            long pos1 = FirstByte - 32;
            realPOS = pos1;
            soDu = realPOS / 512;
            sectorPOS = soDu * 512;
            fs.Seek(sectorPOS, SeekOrigin.Begin);
            fs.Read(a, 0, 512);
            fs.Seek(pos1, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            int check = 0;
            if (a[0] != 0 || a[0x0B] == 0x0F) check = 1;
            if (check == 0) return mainEntryName(a);
            else
            {
                check = 0;
                pos1 = FirstByte - 32;
                long pos2 = FirstByte;
                realPOS = pos1;
                soDu = realPOS / 512;
                sectorPOS = soDu * 512;
                fs.Seek(sectorPOS, SeekOrigin.Begin);
                fs.Read(a, 0, 512);
                fs.Seek(pos1, SeekOrigin.Begin);
                fs.Read(a, 0, 32);
                int checkEnough = 0;
                while (a[0x0B] == 0x0F && a[0x00] != 0xE5 && a[0x00] != 0x00)
                {
                    check = 1;
                    pos1 -= 32;
                    realPOS = pos1;
                    soDu = realPOS / 512;
                    sectorPOS = soDu * 512;
                    fs.Seek(sectorPOS, SeekOrigin.Begin);
                    fs.Read(a, 0, 512);
                    fs.Seek(pos1, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    if(a[0x0B] == 0x10 || a[0x0B] == 0x20)
                    {
                        checkEnough = 1; 
                    }
                }
                for (long i = pos2 - 32; i > pos1; i -= 32)
                {
                    check = 1;
                    realPOS = i;
                    soDu = realPOS / 512;
                    sectorPOS = soDu * 512;
                    fs.Seek(sectorPOS, SeekOrigin.Begin);
                    fs.Read(a, 0, 512);
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    name += subEntryName(a);
                }
                if(clusterBefore > 0 && checkEnough == 0)
                {
                    pos1 = getFirstByteOfCluster(clusterBefore) + bytesPerCluster - 32;
                    pos2 = getFirstByteOfCluster(clusterBefore) + bytesPerCluster;
                    realPOS = pos1;
                    soDu = realPOS / 512;
                    sectorPOS = soDu * 512;
                    fs.Seek(sectorPOS, SeekOrigin.Begin);
                    fs.Read(a, 0, 512);
                    fs.Seek(pos1, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    while (a[0x0B] == 0x0F && a[0x00] != 0xE5 && a[0x00] != 0x00)
                    {
                        check = 1;
                        pos1 -= 32;
                        realPOS = pos1;
                        soDu = realPOS / 512;
                        sectorPOS = soDu * 512;
                        fs.Seek(sectorPOS, SeekOrigin.Begin);
                        fs.Read(a, 0, 512);
                        fs.Seek(pos1, SeekOrigin.Begin);
                        fs.Read(a, 0, 32);
                        if (a[0x0B] == 0x10 || a[0x0B] == 0x20)
                        {
                            checkEnough = 1;
                        }
                    }
                    for (long i = pos2 - 32; i > pos1; i -= 32)
                    {
                        check = 1;
                        realPOS = i;
                        soDu = realPOS / 512;
                        sectorPOS = soDu * 512;
                        fs.Seek(sectorPOS, SeekOrigin.Begin);
                        fs.Read(a, 0, 512);
                        fs.Seek(i, SeekOrigin.Begin);
                        fs.Read(a, 0, 32);
                        name += subEntryName(a);
                    }
                }
                if (check == 0)
                {
                    realPOS = FirstByte;
                    soDu = realPOS / 512;
                    sectorPOS = soDu * 512;
                    fs.Seek(sectorPOS, SeekOrigin.Begin);
                    fs.Read(a, 0, 512);
                    fs.Seek(FirstByte, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    name += mainEntryName(a);
                }
            }
            return name;
        }
        public long nextClusterFromFAT(long cluster)
        {
            long firstByteOfFAT = sectorsBeforeFAT * bytesPerSector + firstSectorOfPartition;
            long currentCluster = firstByteOfFAT + cluster * 4;
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long realPOS = 0;
            long soDu = 0;
            long sectorPOS = 0;
            byte[] arr = new byte[512];
            realPOS = currentCluster;
            soDu = realPOS / 512;
            sectorPOS = soDu * 512;
            fs.Seek(sectorPOS, SeekOrigin.Begin);
            fs.Read(arr, 0, 512);
            fs.Seek(currentCluster, SeekOrigin.Begin);
            fs.Read(arr, 0, 4);
            if (arr[0] == 0xFF || arr[0] == 0xF8) return -1;
            return Function.littleEndian(arr, 0, 4);
        }
        public long getFirstByteOfCluster(long cluster)
        {
            long firstByteOfDATA = (sectorsBeforeFAT + sectorsPerFAT * numberOfFATs) * bytesPerSector + firstSectorOfPartition;
            return firstByteOfDATA + (cluster - begginCluster) * sectorsPerCluster * bytesPerSector;
        }
    }

}
