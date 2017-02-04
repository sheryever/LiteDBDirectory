using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    class ByteWriter
    {
        private readonly long _bufferSize;
        private readonly List<Segment> _internalWriter = new List<Segment>();

        public ByteWriter(long bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public void Add(long position, byte[] array)
        {
            if (_internalWriter.Any())
            {
                var same = _internalWriter.FirstOrDefault(z => z.Buffer.LongLength < _bufferSize && z.Position + z.Buffer.Length == position);
                if (same != null)
                {
                    same.Buffer = ByteHelper.Combine(same.Buffer, array);
                }
                else
                {
                    _internalWriter.Add(new Segment() { Buffer = array, Position = position });
                }
            }
            else
            {
                _internalWriter.Add(new Segment() { Buffer = array, Position = position });
            }
        }

        public IEnumerable<Segment> GetSegments()
        {
            return _internalWriter;
        }
    }
}