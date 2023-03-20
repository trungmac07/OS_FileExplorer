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
        public class MFTEntry
        {
           
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

            public enum OffsetAttributeHeader
            {
                ATTRIBUTE_TYPE = 0x00,
                SIZE_OF_ATTRIBUTE = 0x04,
                RESIDENT = 0x08,
                SIZE_OF_CONTENT = 0x10,
                POSITION_OF_CONTENT = 0x14,

            }

            public enum LengthAttributeHeader
            {
                ATTRIBUTE_TYPE = 4,
                SIZE_OF_ATTRIBUTE = 4,
                RESIDENT = 1,
                SIZE_OF_CONTENT = 4,
                POSITION_OF_CONTENT = 2,
            }

            //Define type of attribute
            public enum AttributeType
            {
                STANDARD_INFO = 0x10,
                FILE_NAME = 0x30,
                DATA = 0x80,
                END = 0xFF,
                EMPTY = 0x00,
            }


            // entry byte array
            private byte[] Info { get; set; }
            //first byte of entry
            private long FirstByte { get; set; }    
      
            

            //In MFT Entry header
            public string Sign { get; }
            public long BeginFirstAttribute { get; }
            public long Type { get; }
            public long BytesUsed { get; }
            public long NumberOfBytes { get; }
            public long ID { get; }

            //Attribute 
            List<Attribute> ListOfAttributes { get; } = new List<Attribute>();


            public MFTEntry(long firstByte, long bytesPerEntry) 
            {
                FirstByte = firstByte;
                Info = new byte[bytesPerEntry];
                try
                {
                    Function.stream.Seek(FirstByte, SeekOrigin.Begin);
                    Function.stream.Read(Info, 0, (int)bytesPerEntry);
       
                }
                catch (FileNotFoundException) { };

                Sign = "";
                for (int i = 0; i < 4; ++i)
                    Sign += (char)Info[(int)OffsetMFTEntryHeader.SIGN + i];

                BeginFirstAttribute =   Function.littleEndian(Info,  (int) OffsetMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE, (int) LengthMFTEntryHeader.BEGIN_FIRST_ATTRIBUTE);
                Type                =   Function.littleEndian(Info,  (int) OffsetMFTEntryHeader.TYPE,                  (int) LengthMFTEntryHeader.TYPE);
                BytesUsed           =   Function.littleEndian(Info,  (int) OffsetMFTEntryHeader.BYTES_USED,            (int) LengthMFTEntryHeader.BYTES_USED);
                NumberOfBytes       =   Function.littleEndian(Info,  (int) OffsetMFTEntryHeader.NUMBER_OF_BYTES,       (int) LengthMFTEntryHeader.NUMBER_OF_BYTES);
                ID                  =   Function.littleEndian(Info,  (int) OffsetMFTEntryHeader.ID,                    (int) LengthMFTEntryHeader.ID);

               
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
                int position = (int) firstByte;

                
               /*Function.stream.Seek(firstByte, SeekOrigin.Begin);
                Function.stream.Read(attributeHeader, 0, 32);*/

                byte attributeType = Info[position + (int) OffsetAttributeHeader.ATTRIBUTE_TYPE];

                if (attributeType == (int)AttributeType.END || attributeType == (int)AttributeType.EMPTY)
                    return null;

                long sizeOfAttribute    =   Function.littleEndian(Info,  position + (int)OffsetAttributeHeader.SIZE_OF_ATTRIBUTE,   (int) LengthAttributeHeader.SIZE_OF_ATTRIBUTE);
                long resident           =   Function.littleEndian(Info,  position + (int)OffsetAttributeHeader.RESIDENT,            (int) LengthAttributeHeader.RESIDENT);
                long sizeOfContent      =   Function.littleEndian(Info,  position + (int)OffsetAttributeHeader.SIZE_OF_CONTENT,     (int) LengthAttributeHeader.SIZE_OF_CONTENT);
                long positionOfContent  =   Function.littleEndian(Info,  position + (int)OffsetAttributeHeader.POSITION_OF_CONTENT, (int) LengthAttributeHeader.POSITION_OF_CONTENT) + firstByte;

                if(FirstByte == 694306816)
                    Console.WriteLine("kkkk");

                if (attributeType == (int) AttributeType.STANDARD_INFO)
                    res = new StandardInfoAttribute(positionOfContent, sizeOfAttribute, resident, Info);
                else if (attributeType == (int) AttributeType.FILE_NAME)
                    res = new FileNameAttribute(positionOfContent, sizeOfAttribute, resident, Info);
                else if (attributeType == (int) AttributeType.DATA)
                    res = new DataAttribute(firstByte, sizeOfAttribute, resident, Info);
                else if (attributeType == (int) AttributeType.END || attributeType == (int)AttributeType.EMPTY)
                    res = null;
                else 
                    res = new OtherAttribute(0,0,0,Info);
                firstByte += sizeOfAttribute;
                return res;

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

