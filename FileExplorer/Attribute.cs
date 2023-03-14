using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
namespace FileExplorer
{

    internal partial class NTFS
    {
        public abstract class Attribute
        {
            public FileStream stream = null;
            public long CurrentDisk { get; }
            public long Size { get; }
            public long Resident { get; }
            public Attribute(long firstByte, long size, long resident, long currentDisk)
            {
                this.Size = size;
                this.Resident = resident;
                this.CurrentDisk = currentDisk;
          
            }

            abstract public void Export(MFTEntry x);

        }

        public class StandardInfoAttribute : Attribute
        {
            private long createdTime;
            private long modifiedTime;
            public enum OffsetTime
            {
                CREATEDTIME = 0x00,
                MODIFIEDTIME = 0x08,
            }
            public enum LengthTime
            {
                CREATEDTIME = 8,
                MODIFIEDTIME = 8,
            }
            public StandardInfoAttribute(long firstByte, long size, long resident, long currentDisk) : base(firstByte, size, resident, currentDisk)
            {
                byte[] attribute = new byte[size];

                Function.stream.Seek(firstByte, SeekOrigin.Begin);
                Function.stream.Read(attribute, 0, (int)size);

                createdTime = Function.littleEndian(attribute, (int)OffsetTime.CREATEDTIME, (int)LengthTime.CREATEDTIME);
                modifiedTime = Function.littleEndian(attribute, (int)OffsetTime.MODIFIEDTIME, (int)LengthTime.MODIFIEDTIME);
            }

            public override void Export(MFTEntry x)
            {
                x.CreatedTime = createdTime;
                x.ModifiedTime = modifiedTime;
            }

        }


        public class FileNameAttribute : Attribute
        {
            private long IDParentFolder;
            private bool IsReadOnly { get; set; } = false;
            private bool IsHidden { get; set; } = false;
            private bool IsSystem { get; set; } = false;
            private bool IsArchive { get; set; } = false;
            private bool IsDirectory { get; set; } = false;
            private string fileName = "";
            public enum FileAttribute
            {
                MASK_READ_ONLY = 0x01,
                MASK_HIDDEN = 0x02,
                MASK_SYSTEM = 0x04,
                MASK_ARCHIVE = 0x20,
                MASK_DIRECTORY = 0x10000000
            }


            public FileNameAttribute(long firstByte, long size, long resident, long currentDisk) : base(firstByte, size, resident, currentDisk)
            {
                byte[] attribute = new byte[size];
     
                Function.stream.Seek(firstByte, SeekOrigin.Begin);
                Function.stream.Read(attribute, 0, (int)size);
           
                int[] offset = { 0x00, 0x38, 0x40, 0x42 };
                int[] length = { 6, 4, 1 };

                IDParentFolder = Function.littleEndian(attribute, offset[0], length[0]);

                int attributes = (int)Function.littleEndian(attribute, offset[1], length[1]);
                if ((attributes & (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsReadOnly = true;
                if ((attributes & (int)FileAttribute.MASK_HIDDEN) != 0)
                    IsHidden = true;
                if ((attributes & (int)FileAttribute.MASK_SYSTEM) != 0)
                    IsSystem = true;
                if ((attributes & (int)FileAttribute.MASK_ARCHIVE) != 0)
                    IsArchive = true;
                if ((attributes & (int)FileAttribute.MASK_DIRECTORY) != 0)
                    IsDirectory = true;

                long nameLength = Function.littleEndian(attribute, offset[2], length[2]);
           
                for (long i = 0;i<2*nameLength;++i)
                {
                    
                    fileName += (char)attribute[offset[3] + i];
                    
                }
                Console.WriteLine("LEN:" + nameLength.ToString() + " - " + fileName);
            }

            public override void Export(MFTEntry x)
            {
                x.FileName = fileName;
                x.IDParentFolder = IDParentFolder;
                x.IsReadOnly = IsReadOnly;
                x.IsHidden = IsHidden;
                x.IsSystem = IsSystem;
                x.IsArchive = IsArchive;
                x.IsDirectory = IsDirectory;
            }
        }
        public class DataAttribute : Attribute
        {
            public DataAttribute(long firstByte, long size, long resident, long currentDisk) : base(firstByte, size, resident, currentDisk)
            {
            }

            public override void Export(MFTEntry x)
            {

            }
        }
    }

}
