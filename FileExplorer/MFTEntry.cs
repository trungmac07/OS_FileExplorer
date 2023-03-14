﻿using System;
using System.IO;
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
                END = 0xFF,
                NULL,
            }
            

            //Define mask value to get attributes from FILE_NAME attribute
           
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

            private long FirstByte { get; set; }    
            private long CurrentDisk { get; set; }

            //In MFT Entry header
            public string Sign { get; }
            public long BeginFirstAttribute { get; }
            public long Type { get; }
            public long BytesUsed { get; }
            public long NumberOfBytes { get; }
            public long ID { get; }

            //In attributes
            public long CreatedTime { get; set; }
            public long ModifiedTime { get; set; }
            public long IDParentFolder { get; set; } = 0;
            public string FileName { get; set; } = "";

            public bool IsReadOnly { get; set; } = false;
            public bool IsHidden { get; set; } = false;
            public bool IsSystem { get; set; } = false;
            public bool IsArchive { get; set; } = false;
            public bool IsDirectory { get; set; } = false;

            FileStream stream = null;

            public MFTEntry(long firstByte, long currentDisk, long bytesPerEntry) 
            {
                CurrentDisk = currentDisk;
                FirstByte = firstByte;
                byte[] entryHeader = new byte[bytesPerEntry];
                try
                {
                    string drivePath = @"\\.\PhysicalDrive" + CurrentDisk.ToString();
                    
                    stream = new FileStream(drivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    // Read the VBR
                    stream.Seek(FirstByte, SeekOrigin.Begin);
                    stream.Read(entryHeader, 0, (int)bytesPerEntry);
       
                }
                catch (FileNotFoundException) { };

                Sign = "";
                for (int i = 0; i < 4; ++i)
                    Sign += (char)entryHeader[(int)OffsetMFTEntryHeader.SIGN + i];

                BeginFirstAttribute =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE, (int) LengthMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE) + FirstByte;
                Type                =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.TYPE,                  (int) LengthMFTEntryHeader.TYPE);
                BytesUsed           =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.BYTES_USED,            (int) LengthMFTEntryHeader.BYTES_USED);
                NumberOfBytes       =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.NUMBER_OF_BYTES,       (int) LengthMFTEntryHeader.NUMBER_OF_BYTES);
                ID                  =   Function.littleEndian(entryHeader,  (int) OffsetMFTEntryHeader.ID,                    (int) LengthMFTEntryHeader.ID);
                readAttributes();
            }

            public void readAttributes()
            {
                long firstByte = BeginFirstAttribute;
                Attribute tmp = null;
                while ((tmp = readAttributeHeader(firstByte)) != null)
                {
                    firstByte += tmp.Size;
                    tmp.Export(this);
                }
                
            }
            public Attribute readAttributeHeader(long firstByte)
            {
                Attribute res = null;
                byte[] attributeHeader = new byte[512];
       
                stream.Seek(firstByte, SeekOrigin.Begin);
                stream.Read(attributeHeader, 0, 512);

                int[] offset = { 0x00, 0x04, 0x08, 0x10, 0x14};
                int[] length = { 4, 4, 1, 4, 2 };

                byte attributeType = attributeHeader[0];
                //Console.WriteLine("byte: " + attributeHeader[0] + " " + attributeHeader[1] + " " + attributeHeader[2] + " " + attributeHeader[3] + " " + attributeHeader[4] + " " + attributeHeader[5] + " " + attributeHeader[6]);
                long sizeOfAttribute = Function.littleEndian(attributeHeader, offset[1], length[1]);
                long resident = Function.littleEndian(attributeHeader, offset[2], length[2]);
                long offsetByte = BeginFirstAttribute;
                long sizeOfContent = Function.littleEndian(attributeHeader, offset[3], length[3]);
                long positionOfContent = Function.littleEndian(attributeHeader,offset[4], length[4]); 



                if (attributeType == 0x10)
                    res = new StandardInfoAttribute(positionOfContent, sizeOfContent, resident, CurrentDisk);
                else if (attributeType == 0x30)
                    res = new FileNameAttribute(positionOfContent, sizeOfContent, resident, CurrentDisk);
                else if (attributeType == 0x80)
                    res = new DataAttribute(positionOfContent, sizeOfContent, resident, CurrentDisk);
                else res = null;

                return res;

            }
            public void print()
            {
                Console.WriteLine("Sign: " + Sign);
              /*Console.WriteLine("BeginFirstAttribute: " + BeginFirstAttribute);
                Console.WriteLine("Type: " + Type);
                Console.WriteLine("BytesUsed: " + BytesUsed);
                Console.WriteLine("NumberOfBytes: " + NumberOfBytes);
                Console.WriteLine("ID: " + ID); */
            }

            public void printInfo()
            {
                Console.WriteLine("Sign: " + Sign);
                Console.WriteLine("FileName:" + FileName);
            }


            public void readStandardInfo()
            {
                
            }
            
            public void readFileName()
            {
               
            }

            public AttributeType readAttributes(byte type, long sizeOfAttribute, int attributeFirstByte)
            {
                if(type == (int)AttributeType.STANDARD_INFOMATION)
                {
                    readStandardInfo();
                    return AttributeType.STANDARD_INFOMATION;
                }
                else if (type == (int)AttributeType.FILE_NAME)
                {
                    readFileName();
                    return AttributeType.FILE_NAME;
                }
                else if(type == (int)AttributeType.DATA)
                {
                    return AttributeType.DATA;
                }
                else if (type == (int)AttributeType.END)
                {
                    return AttributeType.END;
                }
                else
                {
                    return AttributeType.NULL;
                }
            }


            public int CompareTo(MFTEntry other)
            {
                return this.ID.CompareTo(other.ID);
            }

        }
    }
}
