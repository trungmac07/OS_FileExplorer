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
        private string readLittleEndian(int offset, int numberOfByte)
        {
            string s = "";
            for (int i = 0; i < 2 * numberOfByte - 1; i += 2)
                s = byteString[(offset + i) * 2] + byteString[(offset + i) * 2 + 1] + s;
            return s;
        }
        private void readCHS(string CHSstring, ref string head, ref string sector, ref string cylinder)
        {
            string _head = "", _sector = "", _cylinder = "";

            for (int i = 0; i < 8; i++)
            {
                _head += CHSstring[i];
                _cylinder += CHSstring[i + 16];
            }
            for (int i = 10; i < 16; i++) _sector += CHSstring[i];
            _sector += CHSstring[8];
            _sector += CHSstring[9];
            head = _head;
            sector = _sector;
            cylinder = _cylinder;
        }
        private int converBinStringToInt(string binString)
        {
            int value = 0;
            for (int i = binString.Length - 1; i >= 0; i--)
            {
                int tmp = 1;
                for (int j = 0; j < binString.Length - i - 1; j++)
                    tmp *= 2;
                value += tmp;
            }
            return value;
        }
        public string convertHexStringToBinString(string hexString)
        {
            string binString = String.Join(String.Empty, hexString.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
              )
            );
            return binString;
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
        public string getStartSectorInPartitionCHS(int index)
        {
            string CHSstring = readLittleEndian((int)Offset.startSectorCHS + (16 * index), 3);
            return convertHexStringToBinString(CHSstring);
        }
        public void getStartSectorInPartitionCHS(int index, ref string head, ref string sector, ref string cylinder)
        {
            string CHSstring = getStartSectorInPartitionCHS(index);
            readCHS(CHSstring, ref head, ref sector, ref cylinder);
        }
        public string getPartitionType(int index)
        {
            int offset = (int)Offset.type + (16 * index);
            if (byteString[offset * 2] == '0' && byteString[offset * 2 + 1] == 'C') return "FAT32";
            if (byteString[offset * 2] == '0' && byteString[offset * 2 + 1] == '7') return "NTFS";
            return "Unkown partition type.";
        }
        public string getLastSectorInPartitionCHS(int index)
        {
            string CHSstring = readLittleEndian((int)Offset.lastSectorCHS + (16 * index), 3);
            return convertHexStringToBinString(CHSstring);
        }
        public void getLastSectorInPartitionCHS(int index, ref string head, ref string sector, ref string cylinder)
        {
            string CHSstring = getLastSectorInPartitionCHS(index);
            readCHS(CHSstring, ref head, ref sector, ref cylinder);
        }
        public int getFirstSectorLBA(int index)
        {
            string LBAstring = readLittleEndian((int)Offset.firstSectorLBA + (16 * index), 4);
            return converBinStringToInt(LBAstring);
        }
        public int getSectorInPartition(int index)
        {
            string s = readLittleEndian((int)Offset.sectorInPartition + (16 * index), 4);
            s = convertHexStringToBinString(s);
            return converBinStringToInt(s);
        }
        public void printPartitionInfo(int index)
        {
            Console.WriteLine("Partition " + index.ToString() + "'s information:");
            Console.WriteLine("Status: " + getPartitionStatus(index));
            Console.WriteLine("Start sector(CHS): " + getStartSectorInPartitionCHS(index));
            Console.WriteLine("Type: " + getPartitionType(index));
            Console.WriteLine("Last sector(CHS): " + getLastSectorInPartitionCHS(index));
            Console.WriteLine("First sector(LBA): " + getFirstSectorLBA(index));
            Console.WriteLine("Total sectors: " + getSectorInPartition(index).ToString());
        }
    }
}
