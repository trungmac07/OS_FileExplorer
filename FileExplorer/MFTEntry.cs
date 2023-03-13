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
            public enum AttributeType
            {
                STANDARD_INFOMATION = 0x10,
                FILE_NAME = 0x30,
                DATA = 0x80,
                END = 0xFF
            }
            

            //Define mask value to get attributes from FILE_NAME attribute
            public enum FileAttribute
            {
                MASK_READ_ONLY = 0x01,
                MASK_HIDDEN = 0x02,
                MASK_SYSTEM = 0x04,
                MASK_ARCHIVE = 0x20,
                MASK_DIRECTORY = 0x10000000
            }
            
            public enum OffsetMFTEntryHeader
            {
                SIGN = 0x00,
                BEGIN_FIRST_ATTRIBUTE= 0x14, 
                TYPE =0x16, 
                BYTES_USED = 0x18, 
                NUMBER_OF_BYTES = 0x1c, 
                ID = 0x2c,
            }

            public enum LengthMFTEntryHeader
            {
                SIGN = 4,
                BEGIN_FIRST_ATTRIBUTE = 2,
                TYPE = 2,
                BYTES_USED = 4,
                NUMBER_OF_BYTES = 4,
                ID = 4,
            }

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
                int[] offset = {  };
                int[] length = { 4, 2, 2, 4, 4, 4 };

                Sign = new char[4];
                for (int i = 0; i < 4; ++i)
                    Sign[i] = (char)entryHeader[(int)OffsetMFTEntryHeader.SIGN + (4 - i)];

                BeginFirstAttribute =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE, (int) LengthMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE);
                Type                =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.TYPE,                  (int) LengthMFTEntryHeader.TYPE);
                BytesUsed           =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.BYTES_USED,            (int) LengthMFTEntryHeader.BYTES_USED);
                NumberOfBytes       =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.NUMBER_OF_BYTES,       (int) LengthMFTEntryHeader.NUMBER_OF_BYTES);
                ID                  =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.ID,                    (int) LengthMFTEntryHeader.ID);
            }

            public byte readAttributesHeader()
            {
                enum Offset
                {
                    HAHA,
                    HIHI,
                }
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
                if ((attributes &   (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsReadOnly = true;
                if ((attributes &   (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsHidden = true;
                if((attributes &    (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsSystem = true;
                if((attributes &    (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsArchive = true;
                if ((attributes &   (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsDirectory = true;

                long nameLength = Function.littleEndian(fileNameAttribute, offset[2], length[2]);
                for(long i=nameLength-1; i>=0;--i)
                {
                    FileName += fileNameAttribute[offset[3] + i];
                }
            }

            public void readAttributes(byte type)
            {
                if(type == (int)AttributeType.STANDARD_INFOMATION)
                {
                    readStandardInfo();
                }
                else if (type == (int)AttributeType.FILE_NAME)
                {
                    readFileName();
                }
                else if(type == (int)AttributeType.DATA)
                {

                }
                else if (type == (int)AttributeType.END)
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
