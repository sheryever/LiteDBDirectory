using System;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    class LiteDbIndexOutput : IndexOutput
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<FileContent> _fileContents;
        private readonly string _name;
        private long _pointer;
        private LiteDbStreamingWriter _writer;

        public LiteDbIndexOutput(LiteDatabase db, string name)
        {
            _db = db;
            _name = name;
            _writer = new LiteDbStreamingWriter(_db, name);
        }

        public override void WriteByte(byte b)
        {
            WriteBytes(new[] { b }, 0, 1);
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            var segment = new byte[length];
            Buffer.BlockCopy(b, offset, segment, 0, length);
            _writer.Add(_pointer, segment);
            _pointer += length;
        }

        public override void Flush()
        {
            _writer.Write(() => Length);
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            if (disposing)
                _writer.Dispose();
        }

        public override void Seek(long pos)
        {
            _pointer = pos;
        }

        public override long FilePointer => _pointer;

        public override long Length => LiteDbStreamingReader.Length(_db, _name);
    }
}