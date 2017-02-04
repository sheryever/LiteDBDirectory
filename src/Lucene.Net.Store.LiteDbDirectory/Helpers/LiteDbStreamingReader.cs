using System;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using LiteDB;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    class LiteDbStreamingReader : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly string _name;
        //private LiteFileStream contentFileStream;
        private long _currentPosition;

        public LiteDbStreamingReader(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;

            TryInitialize();
        }

        public void ReadBytes(long position, byte[] b, int offset, int len)
        {
            var fileInfo = _db.FileStorage.FindById(_name);
            if (fileInfo == null)
            {
                return;
            }
            if (position < _currentPosition)
            {
                if (!TryInitialize())
                {
                    return;
                }
            }
            if (fileInfo.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    fileInfo.CopyTo(stream);
                    //contentFileStream = _db.FileStorage.FindById(_name).OpenRead();  // contentFileStream is required to open everytime because after steam.Read it is copying 0 values in memorySteam
                    //contentFileStream.CopyTo(stream);
                    stream.Position = position;
                    stream.Read(b, offset, len);
                }
                GC.Collect();
            }
            _currentPosition = position + len;

        }

        private bool TryInitialize()
        {
            var fileInfo = _db.FileStorage.FindById(_name);
            if (fileInfo == null)
                return false;
            //contentFileStream = _db.FileStorage.FindById(_name).OpenRead();
            return true;
        }

        public void Dispose()
        {
            //contentFileStream.Close();
            //contentFileStream.Dispose();
        }

        public static long Length(LiteDatabase db, string contentName)
        {
            var fileInfo = db.FileStorage.FindById(contentName);
            if (fileInfo == null)
                return 0;

            return fileInfo.Length;
        }
    }
}