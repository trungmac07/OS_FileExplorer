using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
namespace FileExplorer
{
    public partial class NTFS
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

            //Attribute 
            List<Attribute> ListOfAttributes { get; } = new List<Attribute>();


            public MFTEntry(long firstByte, long currentDisk, long bytesPerEntry) 
            {
                CurrentDisk = currentDisk;
                FirstByte = firstByte;
                byte[] entryHeader = new byte[bytesPerEntry];
                try
                {
                    Function.stream.Seek(FirstByte, SeekOrigin.Begin);
                    Function.stream.Read(entryHeader, 0, (int)bytesPerEntry);
       
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
                while((tmp = readAttributeHeader(ref firstByte)) != null)
                {
                    ListOfAttributes.Add(tmp);
                }
                
            }
            public Attribute readAttributeHeader(ref long firstByte)
            {
                
                Attribute res = null;
                byte[] attributeHeader = new byte[32];
          
                Function.stream.Seek(firstByte, SeekOrigin.Begin);
                Function.stream.Read(attributeHeader, 0, 32);

                int[] offset = { 0x00, 0x04, 0x08, 0x10, 0x14};
                int[] length = { 4, 4, 1, 4, 2 };

                byte attributeType = attributeHeader[0];
                
                long sizeOfAttribute = Function.littleEndian(attributeHeader, offset[1], length[1]);
                long resident = Function.littleEndian(attributeHeader, offset[2], length[2]);
                long sizeOfContent = Function.littleEndian(attributeHeader, offset[3], length[3]);
                long positionOfContent = Function.littleEndian(attributeHeader,offset[4], length[4]) + firstByte;

                if (attributeType == 0x10)
                    res = new StandardInfoAttribute(positionOfContent, sizeOfAttribute, resident, CurrentDisk);
                else if (attributeType == 0x30)
                    res = new FileNameAttribute(positionOfContent, sizeOfAttribute, resident, CurrentDisk);
                else if (attributeType == 0x80)
                {
                    //Console.WriteLine(sizeOfAttribute);
                    res = new DataAttribute(firstByte, sizeOfAttribute, resident, CurrentDisk);
                }
                else if (attributeType == 0xFF || attributeType == 0x00)
                    res = null;
                else 
                    res = new OtherAttribute(0,0,0,0);

                firstByte += sizeOfAttribute;
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
                Console.WriteLine("ID: " + ID);
                foreach(Attribute a in ListOfAttributes)
                    a.showInfo();
                Console.WriteLine();
                //Console.WriteLine("Size: " + BytesUsed + "/" + NumberOfBytes);
            }

            public string showType()
            {
                if (Type == 0) 
                    return "Deleted File";
                if (Type == 1)
                    return "File";
                if (Type == 2)
                    return "Deleted Folder";
                if (Type == 3)
                    return "Folder";
                return "";
            }

            public int CompareTo(MFTEntry other)
            {
                return this.ID.CompareTo(other.ID);
            }

            public void export(FileInfomation x)
            {
                x.Type = (int)Type;
                x.ID = ID;
                foreach(Attribute a in ListOfAttributes)
                    a.export(x);
            }
            
            

        }
    }
}

