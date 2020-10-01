using System;
using System.Data;
using System.IO;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbIndexInput : BufferedIndexInput
    {
        private readonly LiteDatabase _db;
        private readonly string _name;
        private long _position;


        internal LiteDbIndexInput(LiteDatabase db, string name) : base(name, 1024)
        {
            _db = db;
            _name = name;
        }

        public override long Length => FileHelper.GetContentFileDataLength(_db, _name);

        protected override void Dispose(bool disposing)
        {
        }

        protected override void ReadInternal(byte[] b, int offset, int length)
        {
            if (b.Length == 0)
                return;

            var fileInfo = _db.FileStorage.FindById(_name);

            if (offset < _position)
            {
                fileInfo = _db.FileStorage.FindById(_name);
            }
            if (fileInfo == null)
            {
                return;
            }
            if (fileInfo.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    fileInfo.CopyTo(stream);
                    stream.Position = _position;
                    stream.Read(b, offset, length);
                }
                GC.Collect();
            }
            _position += length;
        }

        protected override void SeekInternal(long pos)
        {
            _position = pos;
        }
    }
}