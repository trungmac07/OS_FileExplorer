using System;
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
        //abstract
        public abstract class Attribute
        {
            public long CurrentDisk { get; }
            public long Size { get; }
            public long Resident { get; }
            public Attribute(long firstByte, long size, long resident, byte[] info)
            {
                this.Size = size;
                this.Resident = resident;

            }
            //show info for testing
            abstract public void showInfo();

            //export info to a file
            abstract public void export(FileInfomation x);

        }

        //derived
        public class StandardInfoAttribute : Attribute
        {
            private long createdTime;
            private long modifiedTime;

            private bool IsReadOnly { get; set; } = false;
            private bool IsHidden { get; set; } = false;
            private bool IsSystem { get; set; } = false;


            private static DateTime baseTime = new DateTime(1601, 1, 1);

            public enum FileAttribute
            {
                MASK_READ_ONLY = 0x01,
                MASK_HIDDEN = 0x02,
                MASK_SYSTEM = 0x04,

            }

            public enum OffsetTime
            {
                CREATEDTIME = 0x00,
                MODIFIEDTIME = 0x08,
                ATTRIBUTE = 0x20,
            }
            public enum LengthTime
            {
                CREATEDTIME = 8,
                MODIFIEDTIME = 8,
                ATTRIBUTE = 4,
            }
            public StandardInfoAttribute(long firstByte, long size, long resident, byte[] info) : base(firstByte, size, resident, info)
            {
                createdTime = Function.littleEndian(info, firstByte + (long)OffsetTime.CREATEDTIME, (int)LengthTime.CREATEDTIME);
                modifiedTime = Function.littleEndian(info, firstByte + (long)OffsetTime.MODIFIEDTIME, (int)LengthTime.MODIFIEDTIME);

                int attributes = (int)Function.littleEndian(info, firstByte + (long)OffsetTime.ATTRIBUTE, (long)LengthTime.ATTRIBUTE);

                if ((attributes & (int)FileAttribute.MASK_READ_ONLY) != 0)
                    IsReadOnly = true;
                if ((attributes & (int)FileAttribute.MASK_HIDDEN) != 0)
                    IsHidden = true;
                if ((attributes & (int)FileAttribute.MASK_SYSTEM) != 0)
                    IsSystem = true;


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
                x.IsReadOnly = IsReadOnly;
                x.IsHidden = IsHidden;
                x.IsSystem = IsSystem;

            }

            public override void showInfo()
            {
                String create = CreateTime().ToLocalTime().ToString("dd/MM/yyyy - HH:mm:ss");
                String modify = ModifiedTime().ToLocalTime().ToString("dd/MM/yyyy - HH:mm:ss");

            }
        }
        public class FileNameAttribute : Attribute
        {
            private long IDParentFolder;

            private string fileName = "";
            private bool IsArchive { get; set; } = false;
            private bool IsDirectory { get; set; } = false;
            public enum FileAttribute
            {

                MASK_ARCHIVE = 0x20,
                MASK_DIRECTORY = 0x10000000
            }
            public FileNameAttribute(long firstByte, long size, long resident, byte[] info) : base(firstByte, size, resident, info)
            {

                long[] offset = { 0x00, 0x38, 0x40, 0x42 };
                long[] length = { 6, 4, 1 };

                IDParentFolder = Function.littleEndian(info, firstByte + offset[0], length[0]);
                int attributes = (int)Function.littleEndian(info, firstByte + offset[1], length[1]);

                if ((attributes & (int)FileAttribute.MASK_ARCHIVE) != 0)
                    IsArchive = true;
                if ((attributes & (int)FileAttribute.MASK_DIRECTORY) != 0)
                    IsDirectory = true;

                long nameLength = Function.littleEndian(info, firstByte + offset[2], length[2]);


                byte[] name = new byte[nameLength * 2];
                for (int i = 0; i < nameLength * 2; ++i)
                {
                    name[i] = info[firstByte + offset[3] + i];
                }

                fileName = Encoding.Unicode.GetString(name);



            }

            public override void export(FileInfomation x)
            {
                x.FileName = fileName;
                x.IDParentFolder = IDParentFolder;
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
            public DataAttribute(long firstByte, long size, long resident, byte[] info) : base(firstByte, size, resident, info)
            {
                if (isZoneIdentifier(firstByte, info) == true)
                    return;
                if (resident == 0) //is resident
                {
                    dataSize = Function.littleEndian(info, firstByte + 16, 4);
                    sizeOnDisk = 0;
                }
                else
                {
                    sizeOnDisk = Function.littleEndian(info, firstByte + 40, 8);
                    dataSize = Function.littleEndian(info, firstByte + 48, 8);
                }
            }

            bool isZoneIdentifier(long firstByte, byte[] info)
            {
                int nameLength = info[firstByte + 0x09];
   
                if (nameLength != 0)
                {
                    return true;
                }
                else
                    return false;
            }

            public override void export(FileInfomation x)
            {
                if (Resident == 0) //is resident
                {
                    if (x.Size == 0)
                    {
                        x.Size = dataSize;
                        x.SizeOnDisk = 0;
                    }
                }
                else
                {
                    if (dataSize != 0)
                        x.Size = dataSize;

                    if (sizeOnDisk != 0)
                        x.SizeOnDisk = sizeOnDisk;
                }

            }

            public override void showInfo()
            {
                //Console.WriteLine("Size: " + sizeOnDisk);
            }
        }
        public class OtherAttribute : Attribute
        {
            public OtherAttribute(long firstByte, long size, long resident, byte[] info) : base(firstByte, size, resident, info)
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
