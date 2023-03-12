using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal partial class NTFS
    {
        public class MFTEntry : IComparable<MFTEntry>
        {
            //Define type of attribute
            const byte STANDARD_INFOMATION = 0x10;
            const byte FILE_NAME = 0x30;
            const byte DATA = 0x80;
            const byte END = 0xFF;

            //Define mask value to get attributes from FILE_NAME attribute
            const int MASK_READ_ONLY = 0x01;
            const int MASK_HIDDEN = 0x02;
            const int MASK_SYSTEM = 0x04;
            const int MASK_ARCHIVE = 0x20;
            const int MASK_DIRECTORY = 0x10000000;

            //In MFT Entry header
            private char[] Sign { get; set; }
            private long BeginFirstAttribute { get; set; }
            private long Type { get; set; }
            private long BytesUsed { get; set; }
            private long NumberOfBytes { get; set; }
            private long ID { get; set; }

            //In attributes
            private long CreatedTime { get; set; }
            private long ModifiedTime { get; set; }
            private long IDParentFolder { get; set; } = 0;
            private string FileName { get; set; } = "";

            private bool IsReadOnly { get; set; } = false;
            private bool IsHidden { get; set; } = false;
            private bool IsSystem { get; set; } = false;
            private bool IsArchive { get; set; } = false;
            private bool IsDirectory { get; set; } = false;

            public MFTEntry() 
            {
                byte[] entryHeader = new byte[512];
                int[] offset = { 0x00, 0x14, 0x16, 0x18, 0x1c, 0x2c };
                int[] length = { 4, 2, 2, 4, 4, 4 };

                Sign = new char[4];
                for (int i = 0; i < 4; ++i)
                    Sign[i] = (char)entryHeader[offset[0] + (4 - i)];

                BeginFirstAttribute = Function.littleEndian(entryHeader, offset[1], length[1]);
                Type = Function.littleEndian(entryHeader, offset[2], length[2]);
                BytesUsed = Function.littleEndian(entryHeader, offset[3], length[3]);
                NumberOfBytes = Function.littleEndian(entryHeader, offset[4], length[4]);
                ID = Function.littleEndian(entryHeader, offset[5], length[5]);
            }

            public byte readAttributesHeader()
            {
                byte[] attributeHeader = new byte[512];
                int[] offset = { 0x00, 0x04, 0x08};
                int[] length = { 4, 4, 1 };

                byte attributeType = attributeHeader[3];
                long sizeOfAttribute = Function.littleEndian(attributeHeader, offset[1], length[1]);
                long resident = Function.littleEndian(attributeHeader, offset[2], length[2]);

                return attributeType;
            }

            public void readStandardInfo()
            {
                byte[] standardInfoAttribute = new byte[512];
                int[] offset = { 0x00, 0x08 };
                int[] length = { 8, 8 };
                CreatedTime = Function.littleEndian(standardInfoAttribute, offset[0], length[0]);
                ModifiedTime = Function.littleEndian(standardInfoAttribute, offset[1], length[1]);
            }
            
            public void readFileName()
            {
                byte[] fileNameAttribute = new byte[512];
                int[] offset = { 0x00, 0x38, 0x40, 0x42 };
                int[] length = { 6, 4, 1 };

                IDParentFolder = Function.littleEndian(fileNameAttribute, offset[0], length[0]);

                int attributes = (int)Function.littleEndian(fileNameAttribute, offset[1], length[1]);
                if ((attributes & MASK_READ_ONLY) != 0)
                    IsReadOnly = true;
                if ((attributes & MASK_HIDDEN) != 0)
                    IsHidden = true;
                if((attributes & MASK_SYSTEM) != 0)
                    IsSystem = true;
                if((attributes & MASK_ARCHIVE) != 0)
                    IsArchive = true;
                if ((attributes & MASK_DIRECTORY) != 0)
                    IsDirectory = true;

                long nameLength = Function.littleEndian(fileNameAttribute, offset[2], length[2]);
                for(long i=nameLength-1; i>=0;--i)
                {
                    FileName += fileNameAttribute[offset[3] + i];
                }
            }

            public void readAttributes(byte type)
            {
                if(type == STANDARD_INFOMATION)
                {
                    readStandardInfo();
                }
                else if (type == FILE_NAME)
                {
                    readFileName();
                }
                else if(type == DATA)
                {

                }
                else if (type == END)
                {

                }
            }

            public int CompareTo(MFTEntry other)
            {
                return this.ID.CompareTo(other.ID);
            }

        }
    }
}
