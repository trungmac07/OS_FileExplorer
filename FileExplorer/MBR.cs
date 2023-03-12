using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal class MBR
    {
        public enum Offset 
        {
            status = 446,
            startSectorCHS = 447,
            type = 450,
            lastSectorCHS = 451,
            firstSectorLBA = 454,
            sectorInPartition = 458
        }
        Offset mbrOffset;
        byte[] byteArr = new byte[512];
        string byteString;
        public MBR()
        {
            byteString = "";
        }
        public void readMBR(int index)
        {
            string drivePath = @"\\.\PhysicalDrive" + index.ToString() ;

            // Open the drive for reading
            try
            {
                FileStream stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);
                // Read the MBR (first sector of the drive)
                stream.Read(byteArr, 0, byteArr.Length);
                byteString = BitConverter.ToString(byteArr).Replace("-", "");

            }
            catch (FileNotFoundException) { };
            
        }
        public void printMBRTable()
        {
            Console.WriteLine("MBR: " + byteString);
            //Console.WriteLine(byteString.Length);
        }
        public void printMBROffset(int index)
        {
            Console.Write(byteString[index * 2]);
            Console.Write(byteString[index * 2 + 1]);
            Console.WriteLine();
        }
        public string getPartitionStatus(int index)
        {
            int offset = (int)Offset.status + (16 * index);
            if (byteString[offset * 2] == '0' && byteString[offset * 2 + 1] == '0') return "unbootable";
            if (byteString[offset * 2] == '8' && byteString[offset * 2 + 1] == '0') return "bootable";
            return "Unkown partition status.";
        }
        public string getFirstSectorInPartitionCHS(int index)
        {
            string result = "";
            int offset = (int)Offset.startSectorCHS + (16 * index);
            for (int i = 0; i < 5; i += 2)
                result = byteString[(offset + i) * 2] + byteString[(offset + i) * 2 + 1] + result;
            return result;
        }
        public string getPartitionType(int index)
        {
            int offset = (int)Offset.type + (16 * index);
            if (byteString[offset * 2] == '0' && byteString[offset * 2 + 1] == 'C') return "FAT32";
            if (byteString[offset * 2] == '0' && byteString[offset * 2 + 1] == '7') return "NTFS";
            return "Unkown partition type.";
        }
        public void printPartitionInfo(int index)
        {
            
        }
    }
}
