using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal class FAT12_16
    {
        public enum OffsetBST
        {
            BYTES_PER_SECTOR = 0x0b,
            SECTORS_PER_CLUSTER = 0x0d,
            SECTORS_BEFORE_FAT = 0x0e,
            NUMBER_OF_FATS = 0x10,
            ENTRYS_OF_RDET = 0x11,
            VOLUME_1 = 0x13,
            SECTORS_PER_FAT = 0x16,
            VOLUME_2 = 0x20,
        }
        public enum LengthBST
        {
            BYTES_PER_SECTOR = 0x0b,
            SECTORS_PER_CLUSTER = 0x0d,
            SECTORS_BEFORE_FAT = 0x0e,
            NUMBER_OF_FATS = 0x10,
            ENTRYS_OF_RDET = 0x11,
            VOLUME_1 = 0x13,
            SECTORS_PER_FAT = 0x16,
            VOLUME_2 = 0x20,
        }
        public long bytesPerSector { get; set; }
        public long sectorsPerCluster { get; set; }
        public long sectorsBeforeFAT { get; set; }
        public long numberOfFATs { get; set; }
        public long volume { get; set; }
        public long entrysOfRDET { get; set; }
        public long sectorsPerFAT { get; set; }
        public string diskPath { get; set; }

        public FAT12_16(byte[] BST)
        {
            
        }
        public FAT12_16(string disk)
        {
            diskPath = disk;
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] BST = new byte[512];
            fs.Read(BST, 0, BST.Length);
            bytesPerSector = Function.littleEndian(BST, (int)OffsetBST.BYTES_PER_SECTOR, (int)LengthBST.BYTES_PER_SECTOR);
            sectorsPerCluster = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_CLUSTER, (int)LengthBST.SECTORS_PER_CLUSTER);
            sectorsBeforeFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_BEFORE_FAT, (int)LengthBST.SECTORS_BEFORE_FAT);
            numberOfFATs = Function.littleEndian(BST, (int)OffsetBST.NUMBER_OF_FATS, (int)LengthBST.NUMBER_OF_FATS);
            sectorsPerFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_FAT, (int)LengthBST.SECTORS_PER_FAT);
            entrysOfRDET = Function.littleEndian(BST, (int)OffsetBST.ENTRYS_OF_RDET, (int)LengthBST.ENTRYS_OF_RDET);
            long a, b;
            a = Function.littleEndian(BST, (int)OffsetBST.VOLUME_1, (int)LengthBST.VOLUME_1);
            b = Function.littleEndian(BST, (int)OffsetBST.VOLUME_2, (int)LengthBST.VOLUME_2);
            if (a == 0) volume = b;
            else volume = a;

            fs.Close();
        }
        public string NameDisk()
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            long FirstSectorInRDET = 0;
            FirstSectorInRDET += this.sectorsBeforeFAT;
            FirstSectorInRDET += this.sectorsPerFAT * this.numberOfFATs;
            byte[] a = new byte[this.bytesPerSector];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Seek(FirstSectorInRDET * this.bytesPerSector, SeekOrigin.Begin);
            fs.Read(a, 0, 192);

            string s = "";
            for (int i = 0; i <= 12; i++)
            {
                s += (char)a[i];
            }
            fs.Close();
            return s;
        }
        public void readFileFromRDET()
        {
            FileStream fs = new FileStream(this.diskPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            long FirstSectorInRDET = 0;
            FirstSectorInRDET += this.sectorsBeforeFAT;
            FirstSectorInRDET += this.sectorsPerFAT * this.numberOfFATs;
            byte[] a = new byte[this.bytesPerSector];
            fs.Seek(FirstSectorInRDET * this.bytesPerSector, SeekOrigin.Begin);
            fs.Read(a, 0, 160);
            fs.Seek(FirstSectorInRDET * this.bytesPerSector + 128, SeekOrigin.Begin);
            fs.Read(a, 0, 32);
            int subEntry = 0;
            long pos1 = fs.Position;
            long pos2 = 0;
            while (a[0] != 0)
            {
                if (a[0] == 0xE5)
                {
                    fs.Read(a, 0, 32);
                    continue;
                }
                else
                {
                    if (a[0x0B] == 0x0F)
                    {
                        //Luu lai vi tri cua sub entry dau tien
                        if (subEntry == 0) pos1 = fs.Position - 32 * 2;
                        subEntry++;
                        fs.Read(a, 0, 32);
                        continue;
                    }
                    else
                    {
                        pos2 = fs.Position - 32;
                        int check = 0;
                        string name = "";
                        for (long i = pos2; i > pos1; i -= 32)
                        {
                            fs.Seek(i, SeekOrigin.Begin);
                            fs.Read(a, 0, 32);
                            if (check == 1)
                            {
                                //cong them cac ten cua entry phu
                                name += subEntryName(a);
                            }
                            else
                            {
                                if (a[0x06] == 0x7E)
                                {
                                    check = 1;
                                    continue;
                                }
                                else
                                {
                                    //La entry chinh
                                    name += mainEntryName(a);
                                    break;
                                }
                            }
                        }
                        Console.WriteLine(name);
                        fs.Seek(pos2 + 32, SeekOrigin.Begin);
                        fs.Read(a, 0, 32);
                        //reset position
                        subEntry = 0;
                    }
                }
            }
            fs.Close();
        }
        public string subEntryName(byte[] arr)
        {
            string s = "";
            for (int i = 0x01; i < 0x01 + 10; i++)
            {
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
                s += (char)arr[i];
            }
            return s;
        }
    }
}
