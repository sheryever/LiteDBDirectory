using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using LiteDB;
using Lucene.Net.Store;
using Lucene.Net.Store.LiteDbDirectory.Entities;
using Lucene.Net.Store.LiteDbDirectory.Helpers;

namespace Lucene.Net.Store.LiteDbDirectory
{
    public sealed class LiteDbDirectory : Directory
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<FileMetaData> _fileMetaDataCollection;
        private readonly LiteCollection<FileContent> _fileContenteCollection;
           

        public LiteDbDirectory(LiteDatabase db)
        {
            if(db == null)
                throw new ArgumentNullException(nameof(db));

            _db = db;
            _fileMetaDataCollection = _db.GetCollection<FileMetaData>(LiteDbDatabase.FileMetaData);
            _fileContenteCollection = db.GetCollection<FileContent>(LiteDbDatabase.FileContents);

            ValidateConfiguration();
            SetLockFactory(new LiteDbLockFactory(_db));
        }

        private void ValidateConfiguration()
        {

            { // Validate if the required database structure is available

                var alltablesAreAvailable =
                    _db.GetCollectionNames()
                        .Count(n => n == "FileLocks" || n == "FileContents" || n == "FileMetaData") == 3;


                if (false == alltablesAreAvailable)
                {
                    throw new ConfigurationErrorsException($"The database structure required for the LiteDbDirectory are not available in database");
                }
            }
        }

        public static void ProvisionDatabase(LiteDatabase db, bool dropExisting = false)
        {
            if (dropExisting)
            {
                LiteDbDatabase.Collections.ForEach(db.DropTableIfExists);
                db.FileStorage.FindAll().ForEach(f => db.FileStorage.Delete(f.Id));
            }
            var temp1 = db.GetCollection<FileContent>(LiteDbDatabase.FileContents);
            temp1.Insert(new FileContent {Name = "1"});
            temp1.Delete(f => f.Name == "1");
            var temp2 = db.GetCollection<FileMetaData>(LiteDbDatabase.FileMetaData);
            temp2.Insert(new FileMetaData { Name = "1" });
            temp2.Delete(f => f.Name == "1");
            var temp3 = db.GetCollection<IndexFileLock>(LiteDbDatabase.FileLocks);
            temp3.Insert(new IndexFileLock { Name = "1" });
            temp3.Delete(f => f.Name == "1");
        }

        public override string[] ListAll()
        {
            var result =_fileMetaDataCollection.FindAll().Select(n => n.Name);

            if (result.Any())
            {
                return result.ToArray();
            }
            return new string[0];
        }

        public override bool FileExists(string name)
        {
            return _fileMetaDataCollection.Count(fm => fm.Name == name) > 0;
        }

        public override long FileModified(string name)
        {
            var result = _fileMetaDataCollection.FindOne(fm => fm.Name == name);
            return result.LastTouchedTimestamp.Ticks;
        }

        public override void TouchFile(string name)
        {
            var result = _fileMetaDataCollection.FindOne(fm => fm.Name == name);
            if (result != null)
            {
                result.LastTouchedTimestamp = DateTime.UtcNow;
                _fileMetaDataCollection.Update(result);
            }

            GC.Collect();
        }

        public override void DeleteFile(string name)
        {
            _fileMetaDataCollection.Delete(fm => fm.Name == name);
            _fileContenteCollection.Delete(fc => fc.Name == name);

        }

        public override long FileLength(string name)
        {
            return LiteDbStreamingReader.Length(_db, name);
        }

        public override IndexOutput CreateOutput(string name)
        {
            if (0 == _fileContenteCollection.Count(fc => fc.Name == name))
            {
                var fileContent = new FileContent {Name = name};
                _fileContenteCollection.Insert(fileContent);
            }
            if (0 == _fileMetaDataCollection.Count(fm => fm.Name == name))
            {
                var fileMetaData = new FileMetaData { Name = name, LastTouchedTimestamp = DateTime.UtcNow };
                _fileMetaDataCollection.Insert(fileMetaData);
            }
            GC.Collect();

            return new LiteDbIndexOutput(_db, name);
        }

        public override IndexInput OpenInput(string name)
        {
            return new LiteDbIndexInput(_db, name);
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
        }


    }
}