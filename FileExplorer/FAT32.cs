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
        public FAT32(long firstSector,int CurrentDisk)
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
        public Tree readMainFileFromRDET()
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Tree rs = new Tree();

            long FirstSectorInRDET = 0;
            FirstSectorInRDET += this.sectorsBeforeFAT;
            FirstSectorInRDET += this.sectorsPerFAT * this.numberOfFATs;
            byte[] a = new byte[this.bytesPerSector];
            fs.Seek(FirstSectorInRDET * this.bytesPerSector + firstSectorOfPartition, SeekOrigin.Begin);
            fs.Read(a, 0, 160);
            fs.Seek(FirstSectorInRDET * this.bytesPerSector + 128 + firstSectorOfPartition, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            int level = 0;
            long pos = 0;
            int dem = 0;
            while (a[0] != 0)
            {
                if(a[0x00] != 0xE5 && a[0x0B] != 0x0F)
                {
                    dem++;
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
                else fs.Read(a, 0, 32);
            }
            fs.Close();
            return rs;
        }
        public void GetAllFiles(FolderTreeNode node, Dictionary<long, FolderTreeNode> Files , int level)
        {
            Files.Add(node.Info.ID, node);
            if (node.Children.Count == 0) return;
            else
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    GetAllFiles(ReadFile(node.Children[i], node.Info.ID, level++), Files, level++);
                }
            }
        }
        public string subEntryName(byte[] arr)
        {
            string s = "";
            for (int i = 0x01; i < 0x01 + 10; i++)
            {
                if (arr[i] == 0xFF) break;
                s += (char)arr[i];
            }
            for (int i = 0x0E; i < 0x0E + 12; i++)
            {
                if (arr[i] == 0xFF) break;
                s += (char)arr[i];
            }
            for (int i = 0x1C; i < 0x1C + 4; i++)
            {
                if (arr[i] == 0xFF) break;
                s += (char)arr[i];
            }
            return s;
        }
        public string mainEntryName(byte[] arr)
        {
            string s = "";
            for (int i = 0x00; i < 0x00 + 11; i++)
            {
                if (arr[i] == 0xFF) break;
                s += (char)arr[i];
            }
            return s;
        }

        public DateTime createTime(byte a, byte b, byte c, byte d, byte e)
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
            DateTime temp = new DateTime(nam,thang,ngay,gio,phut,giay);
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
        public FolderTreeNode ReadFile(long FirstByte, long Parent,int level)
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //get the sector--------------------------------
            byte[] a = new byte[512];
            long offset = FirstByte;
            long x = offset / (long)512;
            long y = x * 512;
            int z = (int)(offset - y);
            fs.Seek(y, SeekOrigin.Begin);
            fs.Read(a, 0, z);
            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            //information------------------------------------
            long pos1 = offset - 32;
            long pos2 = offset;
            string name = "";
            DateTime time = new DateTime();
            long s = 0;
            long cluster = 0;

            if (a[0x06] == 0x7E)
            {
                fs.Seek(pos1, SeekOrigin.Begin);
                fs.Read(a, 0, 32);
                while (a[0x0B] == 0x0F && a[0x00] != 0xE5 && a[0x00] != 0x00)
                {
                    pos1 -= 32;
                    fs.Seek(pos1 , SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                }

                for (long i = pos2 - 32; i > pos1; i -= 32)
                {
                    fs.Seek(i, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    name += subEntryName(a);
                }
            }
            else
            {
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(a, 0, 32);
                name += mainEntryName(a);
            }
            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            time = createTime(a[0x0F], a[0x0E], a[0x0D], a[0x11], a[0x10]);
            s = size(a[0x1C], a[0x1D], a[0x1E], a[0x1F]);
            cluster = Function.littleEndian(a, 0x1A, 2);

            FileInfomation FileTemp = new FileInfomation(fs.Position - 32);
            FileTemp.FileName = name;
            if(Parent == 0) FileTemp.IsRoot = true;
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

            if (FileTemp.IsArchive == true) FileTemp.Type = 0;
            else FileTemp.Type = 1;
            //Children-------------------------
            List<long> Children = new List<long>();
            if (FileTemp.IsDirectory == true)
            {
                long FirstSectorInRDET = 0;
                FirstSectorInRDET += this.sectorsBeforeFAT;
                FirstSectorInRDET += this.sectorsPerFAT * this.numberOfFATs;
                offset = (FirstSectorInRDET + (cluster - 2) * sectorsPerCluster) * bytesPerSector + firstSectorOfPartition;
                x = offset / bytesPerSector;
                y = x * bytesPerSector;


                fs.Seek(y, SeekOrigin.Begin);
                fs.Read(a, 0, 64);
                //fs.Read(a, 0, 32);

                while (a[0x00] != 0x00)
                {
                    offset = fs.Position + 32;
                    x = offset / bytesPerSector;
                    y = x * bytesPerSector;
                    z = (int)(offset - y);
                    fs.Seek(y, SeekOrigin.Begin);
                    fs.Read(a, 0, z);
                    fs.Seek(offset - 32, SeekOrigin.Begin);
                    fs.Read(a, 0, 32);
                    if (a[0x00] != 0xE5 && a[0x0B] != 0x0F && a[0x00] != 0x00) Children.Add(fs.Position - 32);
                }
            }
            //return
            FolderTreeNode res = new FolderTreeNode(FileTemp);
            res.Children = Children;
            res.Level = level;
            fs.Close();
            return res;
        }
    }
    
}
