using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace FileExplorer
{


    internal partial class NTFS
    {
        public enum OffsetVBR
        {
            BYTES_PER_SECTOR = 0x0b,
            SECTORS_PER_CLUSTER = 0x0d,
            SECTORS_PER_TRACK = 0x18,
            NUMBER_OF_HEADS = 0x1a,
            NUMBER_OF_SECTORS = 0x28,
            BEGIN_CLUSTER_1 = 0x30,
            BEGIN_CLUSTER_2 = 0x38,
            BYTES_PER_ENTRY = 0x40,
        }

        public enum LengthVBR
        {
            BYTES_PER_SECTOR = 2,
            SECTORS_PER_CLUSTER = 1,
            SECTORS_PER_TRACK = 2,
            NUMBER_OF_HEADS = 2,
            NUMBER_OF_SECTORS = 8,
            BEGIN_CLUSTER_1 = 8,
            BEGIN_CLUSTER_2 = 8,
            BYTES_PER_ENTRY = 1,
        }

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
         
            byte[] vbr = new byte[512];


            BytesPerSector      =   Function.littleEndian(vbr,  (int)OffsetVBR.BYTES_PER_SECTOR,     (int)LengthVBR.BYTES_PER_SECTOR);
            SectorsPerCluster   =   Function.littleEndian(vbr,  (int)OffsetVBR.SECTORS_PER_CLUSTER,  (int)LengthVBR.SECTORS_PER_CLUSTER);
            SectorsPerTrack     =   Function.littleEndian(vbr,  (int)OffsetVBR.SECTORS_PER_TRACK,    (int)LengthVBR.SECTORS_PER_TRACK);
            NumberOfHeads       =   Function.littleEndian(vbr,  (int)OffsetVBR.NUMBER_OF_HEADS,      (int)LengthVBR.NUMBER_OF_HEADS);
            NumberOfSectors     =   Function.littleEndian(vbr,  (int)OffsetVBR.NUMBER_OF_SECTORS,    (int)LengthVBR.NUMBER_OF_SECTORS);
            BeginCluster1       =   Function.littleEndian(vbr,  (int)OffsetVBR.BEGIN_CLUSTER_1,      (int)LengthVBR.BEGIN_CLUSTER_1);
            BeginCluster2       =   Function.littleEndian(vbr,  (int)OffsetVBR.BEGIN_CLUSTER_2,      (int)LengthVBR.BEGIN_CLUSTER_2);
            //2's complement 
            int rawValue = vbr[(int)OffsetVBR.BYTES_PER_ENTRY];
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
