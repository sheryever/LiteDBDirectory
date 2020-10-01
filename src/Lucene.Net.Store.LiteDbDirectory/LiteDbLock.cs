using System;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbLock : Lock
    {
        private readonly ILiteCollection<IndexFileLock> _fileLocks;
        private readonly string _lockName;

        public LiteDbLock(ILiteCollection<IndexFileLock> fileLocks, string lockName)
        {
            _fileLocks = fileLocks;
            _lockName = lockName;
        }

        public override bool IsLocked()
        {
            var locks = _fileLocks.FindAll();
            return _fileLocks.Count(fl => fl.Name == _lockName && fl.LockReleaseTimestamp > DateTime.UtcNow) != 0;
        }

        public override bool Obtain()
        {
            ReleaseLocksByReleaseTimestamp();
            if (IsLocked())
                return false;
            if (_fileLocks.FindOne(fl => fl.Name == _lockName) != null)
            {
                return false;
            }
            var fileLock = new IndexFileLock
            {
                Name = _lockName,
                LockReleaseTimestamp = DateTime.UtcNow.AddMinutes(30)
            };
            _fileLocks.Insert(fileLock);
            return true;
        }

        private void ReleaseLocksByReleaseTimestamp()
        {
            _fileLocks.DeleteMany(fl => fl.LockReleaseTimestamp < DateTime.UtcNow);
        }

        protected override void Dispose(bool disposing)
        {
            _fileLocks.DeleteMany(fl => fl.Name == _lockName);
        }
    }
}