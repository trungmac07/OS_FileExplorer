using System;
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
        public long sectorsPerFAT { get; set; }

        public FAT12_16(byte[] BST)
        {
            bytesPerSector = Function.littleEndian(BST, (int)OffsetBST.BYTES_PER_SECTOR, (int)LengthBST.BYTES_PER_SECTOR);
            sectorsPerCluster = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_CLUSTER, (int)LengthBST.SECTORS_PER_CLUSTER);
            sectorsBeforeFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_BEFORE_FAT, (int)LengthBST.SECTORS_BEFORE_FAT);
            numberOfFATs = Function.littleEndian(BST, (int)OffsetBST.NUMBER_OF_FATS, (int)LengthBST.NUMBER_OF_FATS);
            sectorsPerFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_FAT, (int)LengthBST.SECTORS_PER_FAT);
            long a, b;
            a = Function.littleEndian(BST, (int)OffsetBST.VOLUME_1, (int)LengthBST.VOLUME_1);
            b = Function.littleEndian(BST, (int)OffsetBST.VOLUME_2, (int)LengthBST.VOLUME_2);
            if (a == 0) volume = b;
            else volume = a; 
        }
    }
}
