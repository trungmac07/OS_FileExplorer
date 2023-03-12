using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FileExplorer
{


    internal partial class NTFS
    {
        private long BytesPerSector { get; set; }
        private long SectorsPerCluster { get; set; }
        private long SectorsPerTrack { get; set; }
        private long NumberOfHeads { get; set; }
        private long NumberOfSectors { get; set; }
        private long BeginCluster1 { get; set; }
        private long BeginCluster2 { get; set; }
        private long BytesPerEntry { get; set; }
        SortedSet<MFTEntry> MFTEntries; 

        public NTFS()
        {
            int[] offset = { 0x0b, 0x0d, 0x18, 0x1a, 0x28, 0x30, 0x38, 0x40 };
            int[] length = { 2, 1, 2, 2, 8, 8, 8, 1 };

            byte[] vbr = new byte[512];

            //TEST
            for (int i = 0; i < 512; i++)
                vbr[i] = (byte)i;
            vbr[offset[7]] = 0xF6;
            //

            BytesPerSector = Function.littleEndian(vbr, offset[0], length[0]);
            SectorsPerCluster = Function.littleEndian(vbr, offset[1], length[1]);
            SectorsPerTrack = Function.littleEndian(vbr, offset[2], length[2]);
            NumberOfHeads = Function.littleEndian(vbr, offset[3], length[3]);
            NumberOfSectors = Function.littleEndian(vbr, offset[4], length[4]);
            BeginCluster1 = Function.littleEndian(vbr, offset[5], length[5]);
            BeginCluster2 = Function.littleEndian(vbr, offset[6], length[6]);
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
            MFTEntries = new SortedSet<MFTEntry>();
        }
       
        public void readAttribute()
        {
            long beginByte = BeginCluster1 * SectorsPerCluster * BytesPerSector;
            MFTEntry mFTEntry = new MFTEntry(); //pass in Begin
            /*
             * Add the file to sorted set 
             * Check if the file has parent or not
             * If yes -> find the parent by ID in set and add a child
             * 
             * 
             */
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
