using System;
using System.IO;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    class LiteDbIndexOutput : BufferedIndexOutput
    {
        private readonly LiteDatabase _db;
        private readonly string _name;

        public LiteDbIndexOutput(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;
        }

        public override void FlushBuffer(byte[] b, int offset, int len)
        {
            var segment = new byte[len];
            Buffer.BlockCopy(b, offset, segment, 0, len);

            var fsinfo = _db.FileStorage.FindById(_name);
            if (fsinfo != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //Console.WriteLine($"{fsinfo.Id} already exist with lenth of {fsinfo.Length}");
                    fsinfo.CopyTo(memoryStream);
                    //memoryStream.Position = memoryStream.Length;
                    memoryStream.Position = FilePointer - len;
                    //Console.WriteLine($"MemorySteam lenth: {memoryStream.Length} before writing");
                    memoryStream.Write(segment, 0, len);
                    //memoryStream.Flush();
                    //_db.FileStorage.Delete(_name);
                    //Console.WriteLine($"MemorySteam lenth: {memoryStream.Length} after writing and flush");
                    memoryStream.Position = 0;
                    fsinfo = _db.FileStorage.Upload(_name, _name, memoryStream);
                    //Console.WriteLine($"{_name} lenth {fsinfo.Length} after flush");
                }
            }
            else
            {
                using (LiteFileStream fileStream = _db.FileStorage.OpenWrite(_name, _name))
                {
                    //Console.WriteLine($"Opened a new file:{_name} to write.");
                    fileStream.Write(segment, 0, len);
                    fileStream.Flush();
                    //Console.WriteLine($"{_name} lenth {fileStream.Length} after flush");
                }
            }
            GC.Collect();
        }

        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            //Flush();
            //if (disposing)
            //    _writer.Dispose();
        }
        
        public override long Length => FileHelper.Length(_db, _name);
    }
}