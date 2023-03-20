using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
namespace FileExplorer
{

    public partial class NTFS
    {
        public abstract class Attribute
        {
            public long CurrentDisk { get; }
            public long Size { get; }
            public long Resident { get; }
            public Attribute(long firstByte, long size, long resident, long currentDisk)
            {
                this.Size = size;
                this.Resident = resident;
                this.CurrentDisk = currentDisk;

            }
            abstract public void showInfo();

            abstract public void export(FileInfomation x);

        }

        public class StandardInfoAttribute : Attribute
        {
            private long createdTime;
            private long modifiedTime;
            private static DateTime baseTime = new DateTime(1601, 1, 1);
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

            public DateTime CreateTime()
            {
                DateTime time = new DateTime(createdTime + baseTime.Ticks);
                return time;
            }

            public DateTime ModifiedTime()
            {
                DateTime time = new DateTime(modifiedTime + baseTime.Ticks);
                return time;
                /* String test = time.ToLocalTime().ToString("dd/MM/yyyy - HH:mm:ss");
                 return test;*/
            }

            public override void export(FileInfomation x)
            {
                x.CreatedTime = CreateTime();
                x.LastModifiedTime = ModifiedTime();
            }

            public override void showInfo()
            {
                String create = CreateTime().ToLocalTime().ToString("dd/MM/yyyy - HH:mm:ss");
                String modify = ModifiedTime().ToLocalTime().ToString("dd/MM/yyyy - HH:mm:ss");

                Console.WriteLine("Created: " + create);
                Console.WriteLine("Modified: " + modify);
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


                byte[] name = new byte[nameLength * 2];
                for (int i = 0; i < nameLength * 2; ++i)
                {
                    name[i] = attribute[offset[3] + i];
                }

                fileName = Encoding.Unicode.GetString(name);


            }

            public override void export(FileInfomation x)
            {
                x.FileName = fileName;
                x.IDParentFolder = IDParentFolder;
                x.IsReadOnly = IsReadOnly;
                x.IsHidden = IsHidden;
                x.IsSystem = IsSystem;
                x.IsArchive = IsArchive;
                x.IsDirectory = IsDirectory;
            }

            public override void showInfo()
            {
                Console.WriteLine("Filename: " + fileName);
                Console.WriteLine("IDParent: " + IDParentFolder);
            }

        }
        public class DataAttribute : Attribute
        {
            private long dataSize = 0;
            private long sizeOnDisk = 0;
            public DataAttribute(long firstByte, long size, long resident, long currentDisk) : base(firstByte, size, resident, currentDisk)
            {
                if (resident == 0 ) //is resident
                {
                    byte[] attribute = new byte[4];
                    Function.stream.Seek(firstByte + 16, SeekOrigin.Begin);
                    Function.stream.Read(attribute, 0, 4);
                    dataSize = Function.littleEndian(attribute, 0, 4);
                    sizeOnDisk = 0;
                }
                else
                {
                    byte[] attribute = new byte[8];
                    Function.stream.Seek(firstByte + 40, SeekOrigin.Begin);
                    Function.stream.Read(attribute, 0, 8);
                    sizeOnDisk = Function.littleEndian(attribute, 0, 8);

                    byte[] attribute2 = new byte[8];
                    Function.stream.Seek(firstByte + 48, SeekOrigin.Begin);
                    Function.stream.Read(attribute2, 0, 8);
                    dataSize = Function.littleEndian(attribute2, 0, 8);
                }
            }

            public override void export(FileInfomation x)
            {
                if(Resident == 0) //is resident
                {
                    if(x.Size == 0)
                    {
                        x.Size = dataSize;
                        x.SizeOnDisk = 0;
                    }
                }
                else
                {
                    x.Size = dataSize;
                    x.SizeOnDisk = sizeOnDisk;
                }
              
                //Console.WriteLine("Size: " + sizeOnDisk);
            }

            public override void showInfo()
            {
                //Console.WriteLine("Size: " + sizeOnDisk);
            }
        }

        public class OtherAttribute : Attribute
        {
            public OtherAttribute(long firstByte, long size, long resident, long currentDisk) : base(firstByte, size, resident, currentDisk)
            {

            }

            public override void export(FileInfomation x)
            {
               
            }
            public override void showInfo()
            {

            }
        }

    }

}
