using System;
using System.IO;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbIndexOutput : BufferedIndexOutput
    {
        private readonly LiteDatabase _db;
        private readonly string _name;

        public LiteDbIndexOutput(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;
        }

        public override long Length => FileHelper.GetContentFileDataLength(_db, _name);

        protected override void FlushBuffer(byte[] b, int offset, int len)
        {
            var segment = new byte[len];
            Buffer.BlockCopy(b, offset, segment, 0, len);

            var fsinfo = _db.FileStorage.FindById(_name);
            if (fsinfo != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //Console.WriteLine($"{fsinfo.Id} already exist with lenth of {fsinfo.GetContentFileDataLength}");
                    fsinfo.CopyTo(memoryStream);
                    //memoryStream.Position = memoryStream.GetContentFileDataLength;
                    memoryStream.Position = this.GetFilePointer() - len;
                    //Console.WriteLine($"MemorySteam lenth: {memoryStream.GetContentFileDataLength} before writing");
                    memoryStream.Write(segment, 0, len);
                    //memoryStream.Flush();
                    //_db.FileStorage.Delete(_name);
                    //Console.WriteLine($"MemorySteam lenth: {memoryStream.GetContentFileDataLength} after writing and flush");
                    memoryStream.Position = 0;
                    fsinfo = _db.FileStorage.Upload(_name, _name, memoryStream);
                    //Console.WriteLine($"{_name} lenth {fsinfo.GetContentFileDataLength} after flush");
                }
            }
            else
            {
                using (var fileStream = _db.FileStorage.OpenWrite(_name, _name))
                {
                    //Console.WriteLine($"Opened a new file:{_name} to write.");
                    fileStream.Write(segment, 0, len);
                    fileStream.Flush();
                    //Console.WriteLine($"{_name} lenth {fileStream.GetContentFileDataLength} after flush");
                }
            }
            GC.Collect();
        }
    }
}