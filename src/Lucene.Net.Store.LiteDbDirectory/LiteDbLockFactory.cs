using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbLockFactory : LockFactory
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<IndexFileLock> _fileLocks;


        internal LiteDbLockFactory(LiteDatabase db)
        {
            _db = db;
            _fileLocks = db.GetCollection<IndexFileLock>(LiteDbDatabase.FileLocks);
        }

        public override Lock MakeLock(string lockName)
        {
            return new LiteDbLock(_fileLocks, lockName);
        }

        public override void ClearLock(string lockName)
        {
            _fileLocks.Delete(fl => fl.Name == lockName);
        }
    }
}