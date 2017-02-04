using System.Data;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbIndexInput : IndexInput
    {
        private readonly LiteDatabase _db;
        private readonly string _name;
        private long _position;

        private LiteDbStreamingReader _dataReader;

        internal LiteDbIndexInput(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;
            _dataReader = new LiteDbStreamingReader(db, name);
        }

        public override byte ReadByte()
        {
            var buffer = new byte[1];
            ReadBytes(buffer, 0, 1);
            return buffer[0];
        }

        public override void ReadBytes(byte[] b, int offset, int len)
        {
            if (b.Length == 0)
                return;

            _dataReader.ReadBytes(_position, b, offset, len);

            _position += len;
        }

        protected override void Dispose(bool disposing)
        {
            
        }

        public override void Seek(long pos)
        {
            _position = pos;
        }

        public override long Length()
        {
            return LiteDbStreamingReader.Length(_db, _name); ;
        }

        public override long FilePointer => _position;
    }
}