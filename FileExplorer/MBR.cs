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
        private string[] BinArr = { "0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111", "1000", "1001",
                            "1010", "1011", "1100", "1101", "1110", "1111"};
        private enum Offset 
        {
            status = 446,
            startSectorCHS = 447,
            type = 450,
            lastSectorCHS = 451,
            firstSectorLBA = 454,
            sectorInPartition = 458
        }
        private byte[] byteArr = new byte[512];
        private string byteString;
        public MBR()
        {
            byteString = "";
        }
        private string readLittleEndian(int offset, int numberOfByte)
        {
            string s = "";
            string st = "";
            for (int i = offset * 2; i < offset * 2 + numberOfByte * 2; i++)
                st = st + byteString[i];
            Console.WriteLine(st);
            for (int i = 0; i <  numberOfByte ; i ++)
            {
                s = byteString[(offset + i) * 2 + 1] + s;
                s = byteString[(offset + i) * 2] + s;
            }
            Console.WriteLine(s);
            return s;
        }
        private void readCHS(string CHSstring, ref long head, ref long sector, ref long cylinder)
        {
            string _head = "", _sector = "", _cylinder = "";
            _cylinder = _cylinder + CHSstring[8] ;
            _cylinder = _cylinder + CHSstring[9];
            for (int i = 0; i < 8; i++)
            {
                _head += CHSstring[i];
                _cylinder += CHSstring[i + 16];
            }
            for (int i = 10; i < 16; i++) _sector += CHSstring[i];
            head = converBinStringToInt(_head);
            sector = converBinStringToInt(_sector);
            cylinder = converBinStringToInt(_cylinder);
        }
        private long converBinStringToInt(string binString)
        {
            long value = 0;
            for (int i = binString.Length - 1; i >= 0; i--)
            {
                if (binString[i] == '1')
                {
                    long tmp = 1;
                    for (int j = 0; j < binString.Length - i - 1; j++)
                        tmp *= 2;
                    value += tmp;
                }
            }
            return value;
        }
        public string convertHexStringToBinString(string hexString)
        {
            string binString = "";
            for (int i = 0; i < hexString.Length; i++)
            {
                int binValue = (int)hexString[i] - 48;
                if (binValue >= 0 && binValue <= 9)
                    binString += BinArr[binValue];
                else if (binValue >= 10)
                    binString += BinArr[binValue - 7];
            }
            /*
            string binString = String.Join(String.Empty, hexString.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
              )
            );
            */
            return binString;

            
        }
        public void readMBR(int index)
        {
            string drivePath = @"\\.\PhysicalDrive" + index.ToString() ;
            Function.stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);

            // Open the drive for reading
            try
            {
                
                // Read the MBR (first sector of the drive)
                Function.stream.Read(byteArr, 0, byteArr.Length);
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
        public void getStartSectorInPartitionCHS(int index, ref long head, ref long sector, ref long cylinder)
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
        public void getLastSectorInPartitionCHS(int index, ref long head, ref long sector, ref long cylinder)
        {
            string CHSstring = getLastSectorInPartitionCHS(index);
            readCHS(CHSstring, ref head, ref sector, ref cylinder);
        }
        public long getFirstSectorLBA(int index)
        {
            string LBAstring = readLittleEndian((int)Offset.firstSectorLBA + (16 * index), 4);
            LBAstring = convertHexStringToBinString(LBAstring);
            return converBinStringToInt(LBAstring);
        }
     
        public long getSectorInPartition(int index)
        {
            string s = readLittleEndian((int)Offset.sectorInPartition + (16 * index), 4);
            s = convertHexStringToBinString(s);
            return converBinStringToInt(s);
        }
        public void printPartitionInfo(int index)
        {
            long head = 0, sector = 0, cylinder = 0;
            Console.WriteLine("Partition " + index.ToString() + "'s information:");
            Console.WriteLine("Status: " + getPartitionStatus(index));
            Console.WriteLine("Start sector(CHS): " + getStartSectorInPartitionCHS(index));
            getStartSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
            Console.WriteLine("Head: " + head);
            Console.WriteLine("Sector: " + sector);
            Console.WriteLine("Cylinder: " + cylinder);
            Console.WriteLine("Type: " + getPartitionType(index));
            Console.WriteLine("Last sector(CHS): " + getLastSectorInPartitionCHS(index));
            getLastSectorInPartitionCHS(index, ref head, ref sector, ref cylinder);
            Console.WriteLine("Head: " + head);
            Console.WriteLine("Sector: " + sector);
            Console.WriteLine("Cylinder: " + cylinder);
            Console.WriteLine("First sector(LBA): " + getFirstSectorLBA(index));
            Console.WriteLine("Total sectors: " + getSectorInPartition(index).ToString());
        }

        public bool isPartitionActive(int index)
        {
            long start_head = 0, start_sector = 0, start_cylinder = 0;
            getStartSectorInPartitionCHS(index, ref start_head, ref start_sector, ref start_cylinder);
            long end_head = 0, end_sector = 0, end_cylinder = 0;
            getStartSectorInPartitionCHS(index, ref end_head, ref end_sector, ref end_cylinder);

            if (start_head == end_head && end_head == 0)
            {
                if (start_sector == end_sector && end_sector == 0)
                {
                    if (start_cylinder == end_cylinder && end_cylinder == 0)
                        return false;
                }
            }
            return true;
        }
    }
}
