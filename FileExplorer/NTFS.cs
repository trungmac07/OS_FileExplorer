using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    
    internal class NTFS
    {
        private long bytesPerSector;
        private long sectorsPerCluster;
        private long sectorsPerTrack;
        private long numberOfHeads;
        private long numberOfSectors;
        private long beginCluster1;
        private long beginCluster2;
        private long bytesPerEntry;

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

            bytesPerSector      =   littleEndian(vbr, offset[0], length[0]);
            sectorsPerCluster   =   littleEndian(vbr, offset[1], length[1]);
            sectorsPerTrack     =   littleEndian(vbr, offset[2], length[2]);
            numberOfHeads       =   littleEndian(vbr, offset[3], length[3]);
            numberOfSectors     =   littleEndian(vbr, offset[4], length[4]);
            beginCluster1       =   littleEndian(vbr, offset[5], length[5]);
            beginCluster2       =   littleEndian(vbr, offset[6], length[6]);
            
            //2's complement 
            int rawValue = vbr[offset[7]];
            int negativeMask = 0x00000080;
            int invertMask = ~0x000000ff;

            if ((rawValue & negativeMask) > 0)          // with a negative number
            {   
                rawValue |= invertMask;                 // change the left 24 bits to 1
                rawValue = ~rawValue; ++rawValue;       // invert bit, +1    
            }

            bytesPerEntry = (long)Math.Pow(2, rawValue); //2^
            Console.WriteLine(bytesPerEntry);
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
            Console.WriteLine(bytesPerSector);
            Console.WriteLine(sectorsPerCluster);
            Console.WriteLine(sectorsPerTrack);
            Console.WriteLine(numberOfHeads);
            Console.WriteLine(numberOfSectors);
            Console.WriteLine(beginCluster1);
            Console.WriteLine(beginCluster2);
            Console.WriteLine(bytesPerEntry);
        }

       

    }
}
