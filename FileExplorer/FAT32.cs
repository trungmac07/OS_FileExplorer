using System;
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
        public long bytesPerSector { get; set; }
        public long sectorsPerCluster { get; set; }
        public long sectorsBeforeFAT { get; set; }
        public long numberOfFATs { get; set; }
        public long volume { get; set; }
        public long sectorsPerFAT { get; set; }
        public long begginCluster { get; set; }

        public FAT32(byte[] BST)
        {
            bytesPerSector = Function.littleEndian(BST, (int)OffsetBST.BYTES_PER_SECTOR, (int)LengthBST.BYTES_PER_SECTOR);
            sectorsPerCluster = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_CLUSTER, (int)LengthBST.SECTORS_PER_CLUSTER);
            sectorsBeforeFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_BEFORE_FAT, (int)LengthBST.SECTORS_BEFORE_FAT);
            numberOfFATs = Function.littleEndian(BST, (int)OffsetBST.NUMBER_OF_FATS, (int)LengthBST.NUMBER_OF_FATS);
            volume = Function.littleEndian(BST, (int)OffsetBST.VOLUME, (int)LengthBST.VOLUME);
            sectorsPerFAT = Function.littleEndian(BST, (int)OffsetBST.SECTORS_PER_FAT, (int)LengthBST.SECTORS_PER_FAT);
            begginCluster = Function.littleEndian(BST, (int)OffsetBST.BEGGIN_CLUSTER, (int)LengthBST.BEGGIN_CLUSTER);
        } 
    }
}
