using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbLockFactory : LockFactory
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<IndexFileLock> _fileLocks;

        internal LiteDbLockFactory(LiteDatabase db)
        {
            _db = db;
            _fileLocks = db.GetCollection<IndexFileLock>(LiteDbCollectionsInfo.FileLocks);
        }

        public override Lock MakeLock(string lockName)
        {
            return new LiteDbLock(_fileLocks, lockName);
        }

        public override void ClearLock(string lockName)
        {
            _fileLocks.DeleteMany(fl => fl.Name == lockName);
        }
    }
}