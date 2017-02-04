using System;
using System.Data;
using System.IO;
using System.Linq;
using LiteDB;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    class LiteDbStreamingWriter : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly string _name;
        private ByteWriter _buffer = new ByteWriter(4096);

        public LiteDbStreamingWriter(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;
        }

        private void Write(byte[] buffer, int index, int len, bool isFirstWrite)
        {
            var fsinfo = _db.FileStorage.FindById(_name);
            if (fsinfo != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //Console.WriteLine($"{fsinfo.Id} already exist with lenth of {fsinfo.Length}");
                    fsinfo.CopyTo(memoryStream);
                    //memoryStream.Position = memoryStream.Length;
                    memoryStream.Position = index;
                    //Console.WriteLine($"MemorySteam lenth: {memoryStream.Length} before writing");
                    memoryStream.Write(buffer, 0, len);
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
                    fileStream.Write(buffer, 0, len);
                    fileStream.Flush();
                    //Console.WriteLine($"{_name} lenth {fileStream.Length} after flush");
                }
            }
            GC.Collect();


            //Console.WriteLine($"---------------------------------------");
        }

        public void Add(long pointer, byte[] segment)
        {
            _buffer.Add(pointer, segment);
        }

        public void Write(Func<long> length)
        {
            var segments = _buffer.GetSegments();
            if (segments.Any())
            {
                var isFirst = length() == 0;
                foreach (var segment in segments)
                {
                    Write(segment.Buffer, (int)segment.Position, segment.Buffer.Length, isFirst);
                    isFirst = false;
                }
                _buffer = new ByteWriter(4082);
            }
        }

        public void Dispose()
        {
            //_updateCommand.Dispose();
        }
    }
}