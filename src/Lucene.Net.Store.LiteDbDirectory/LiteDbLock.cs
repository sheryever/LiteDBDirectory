using System;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbLock : Lock
    {
        private readonly LiteCollection<IndexFileLock> _fileLocks;
        private readonly string _lockName;

        public LiteDbLock(LiteCollection<IndexFileLock> fileLocks, string lockName)
        {
            _fileLocks = fileLocks; 
            _lockName = lockName;
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
            _fileLocks.Delete(fl => fl.LockReleaseTimestamp < DateTime.UtcNow);
        }

        public override void Release()
        {
            _fileLocks.Delete(fl => fl.Name == _lockName);
            ReleaseLocksByReleaseTimestamp();
        }

        public override bool IsLocked()
        {
            return _fileLocks.Count(fl => fl.Name == _lockName && fl.LockReleaseTimestamp > DateTime.UtcNow) != 0;
        }
    }
}