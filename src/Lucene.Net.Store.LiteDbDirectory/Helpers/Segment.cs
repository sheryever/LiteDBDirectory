namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    class Segment
    {
        public long Position { get; set; }
        public byte[] Buffer { get; set; }
    }
}