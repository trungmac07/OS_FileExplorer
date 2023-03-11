using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    
    internal class NTFS
    {
        private long BytesPerSector { get; set; }
        private long SectorsPerCluster { get; set; }
        private long SectorsPerTrack { get; set; }
        private long NumberOfHeads { get; set; }
        private long NumberOfSectors { get; set; }
        private long BeginCluster1 { get; set; }
        private long BeginCluster2 { get; set; }
        private long BytesPerEntry { get; set; }

        private static int[] offset = { 0x0b, 0x0d, 0x18, 0x1a, 0x28, 0x30, 0x38, 0x40 };
        private static int[] length = { 2, 1, 2, 2, 8, 8, 8, 1 };

        public NTFS()
        {
            byte[] vbr = new byte[512];
            
            //TEST
            for (int i = 0; i < 512; i++)
                vbr[i] = (byte) i;
            vbr[offset[7]] = 0xF6;
            //

            BytesPerSector      =   littleEndian(vbr, offset[0], length[0]);
            SectorsPerCluster   =   littleEndian(vbr, offset[1], length[1]);
            SectorsPerTrack     =   littleEndian(vbr, offset[2], length[2]);
            NumberOfHeads       =   littleEndian(vbr, offset[3], length[3]);
            NumberOfSectors     =   littleEndian(vbr, offset[4], length[4]);
            BeginCluster1       =   littleEndian(vbr, offset[5], length[5]);
            BeginCluster2       =   littleEndian(vbr, offset[6], length[6]);
            
            //2's complement 
            int rawValue = vbr[offset[7]];
            int negativeMask = 0x00000080;
            int invertMask = ~0x000000ff;

            if ((rawValue & negativeMask) > 0)          // with a negative number
            {   
                rawValue |= invertMask;                 // change the left 24 bits to 1
                rawValue = ~rawValue; ++rawValue;       // invert bit, +1    
            }

            BytesPerEntry = (long)Math.Pow(2, rawValue); //2^
      
        }

        private long littleEndian(byte[] src, int offset, int length)
        {
            long res = 0;
            for (int i = length - 1; i >= 0; --i)
            {
                res <<= 8;
                res += (int)src[offset + i];
            }
            return res;
        }

        public void printVBRInfo()
        {
            Console.WriteLine(BytesPerSector);
            Console.WriteLine(SectorsPerCluster);
            Console.WriteLine(SectorsPerTrack);
            Console.WriteLine(NumberOfHeads);
            Console.WriteLine(NumberOfSectors);
            Console.WriteLine(BeginCluster1);
            Console.WriteLine(BeginCluster2);
            Console.WriteLine(BytesPerEntry);
        }

       

    }
}
