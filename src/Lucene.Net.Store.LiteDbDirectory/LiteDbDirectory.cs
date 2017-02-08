using System;
using System.Collections.Concurrent;
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
    /// <summary>
    /// An implementation of <see cref="Directory"/> to store the lucene indexing files in LiteDb database.
    /// </summary>
    public sealed class LiteDbDirectory : Directory
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<FileMetaData> _fileMetaDataCollection;
        private readonly LiteCollection<FileContent> _fileContenteCollection;

        private readonly ConcurrentDictionary<string, LiteDbIndexInput> _runningInputs = new ConcurrentDictionary<string, LiteDbIndexInput>();
        private readonly ConcurrentDictionary<string, LiteDbIndexOutput> _runningOutputs = new ConcurrentDictionary<string, LiteDbIndexOutput>();

        /// <summary>
        /// Initiate the LiteDbDirectory instance 
        /// </summary>
        /// <param name="db">The LiteDatabase object</param>
        public LiteDbDirectory(LiteDatabase db)
        {
            if(db == null)
                throw new ArgumentNullException(nameof(db));

            _db = db;
            _fileMetaDataCollection = _db.GetCollection<FileMetaData>(LiteDbCollectionsInfo.FileMetaData);
            _fileContenteCollection = db.GetCollection<FileContent>(LiteDbCollectionsInfo.FileContents);

            CheckRequiredCollection();
            SetLockFactory(new LiteDbLockFactory(_db));
        }

        /// <summary>
        /// Check the requried collection in the database
        /// </summary>
        public void CheckRequiredCollection()
        {

            { // Validate if the required database structure is available

                var alltablesAreAvailable =
                    _db.GetCollectionNames()
                        .Count(n => n == "__FileLocks" || n == "__FileContents" || n == "__FileMetaData") == 3;


                if (false == alltablesAreAvailable)
                {
                    throw new ConfigurationErrorsException($"The LiteDbCollections required for the LiteDbDirectory are not available in database");
                }
            }
        }

        /// <summary>
        /// Create the requried tables for lucene.net indexing
        /// </summary>
        /// <param name="db">The LiteDatabase object</param>
        /// <param name="dropExisting">A boolean value determine to drop and recreate the collections if already exist</param>
        public static void CreateRequiredCollections(LiteDatabase db, bool dropExisting = false)
        {
            if (dropExisting)
            {
                LiteDbCollectionsInfo.Collections.ForEach(db.DropTableIfExists);
                db.FileStorage.FindAll().ForEach(f => db.FileStorage.Delete(f.Id));
            }

            var temp1 = db.GetCollection<FileContent>(LiteDbCollectionsInfo.FileContents);
            temp1.Insert(new FileContent {Name = "1"});
            temp1.Delete(f => f.Name == "1");
            var temp2 = db.GetCollection<FileMetaData>(LiteDbCollectionsInfo.FileMetaData);
            temp2.Insert(new FileMetaData { Name = "1" });
            temp2.Delete(f => f.Name == "1");
            var temp3 = db.GetCollection<IndexFileLock>(LiteDbCollectionsInfo.FileLocks);
            temp3.Insert(new IndexFileLock { Name = "1" });
            temp3.Delete(f => f.Name == "1");
        }

        /// <summary>
        /// List all the file names from <see cref="LiteCollection{T}"/> of <see cref="FileMetaData"/>
        /// </summary>
        /// <returns></returns>
        public override string[] ListAll()
        {
            var result =_fileMetaDataCollection.FindAll().Select(n => n.Name);

            if (result.Any())
            {
                return result.ToArray();
            }
            return new string[0];
        }

        /// <summary>
        /// Check the file exist in <see cref="LiteCollection{T}"/> of <see cref="FileMetaData"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="FileMetaData"/></param>
        /// <returns>The boolean value</returns>
        public override bool FileExists(string name)
        {
            return _fileMetaDataCollection.Count(fm => fm.Name == name) > 0;
        }

        /// <summary>
        /// Get the <see cref="FileMetaData.LastTouchedTimestamp"/> of a <see cref="FileMetaData"/> from <see cref="LiteCollection{T}"/> of <see cref="FileMetaData"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="FileMetaData"/></param>
        /// <returns>The <see cref="FileMetaData.LastTouchedTimestamp"/> as ticks</returns>
        public override long FileModified(string name)
        {
            var result = _fileMetaDataCollection.FindOne(fm => fm.Name == name);
            return result.LastTouchedTimestamp.Ticks;
        }

        /// <summary>
        /// Update the value <see cref="FileMetaData.LastTouchedTimestamp"/>
        /// </summary>
        /// <param name="name">The name of the <see cref="FileMetaData"/></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the <see cref="FileMetaData"/></param>
        public override void DeleteFile(string name)
        {
            LiteDbIndexOutput runningOutput;
            if (_runningOutputs.TryRemove(name, out runningOutput))
            {
                runningOutput.Dispose();
            }
            LiteDbIndexInput runningInput;
            if (_runningInputs.TryRemove(name, out runningInput))
            {
                runningInput.Dispose();
            }

            var metaData = _fileMetaDataCollection.FindOne(fm => fm.Name == name);
            metaData.IsDeleted = true;
            metaData.Name = Guid.NewGuid().ToString();
            _fileMetaDataCollection.Update(metaData);

            var contentFile =  _fileContenteCollection.FindOne(fc => fc.Name == name);
            contentFile.IsDeleted = true;
            contentFile.Name = Guid.NewGuid().ToString();
            _fileContenteCollection.Update(contentFile);
        }



        /// <summary>
        /// Get the lenght of the indexing file
        /// </summary>
        /// <param name="name">The name of the <see cref="FileContent"/></param>
        /// <returns></returns>
        public override long FileLength(string name)
        {
            return FileHelper.GetContentFileDataLength(_db, name);
        }

        /// <summary>
        /// Create the instance of an <see cref="IndexOutput"/> and the required <see cref="FileContent"/> and <see cref="FileMetaData"/> objects
        /// </summary>
        /// <param name="name">The name of the indexing file </param>
        /// <returns>The object of <see cref="IndexOutput"/></returns>
        public override IndexOutput CreateOutput(string name)
        {
            LiteDbIndexOutput runningOutput;
            if (_runningOutputs.TryRemove(name, out runningOutput))
            {
                runningOutput.Dispose();
            }


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

            var result = new LiteDbIndexOutput(_db, name);
            _runningOutputs.TryAdd(name, result);

            return result;

        }

        /// <summary>
        /// Create the instance of an <see cref="IndexInput"/> 
        /// </summary>
        /// <param name="name">The name of the indexing file </param>
        /// <returns>The object of <see cref="IndexOutput"/></returns>
        public override IndexInput OpenInput(string name)
        {
            LiteDbIndexInput runningInput;
            if (_runningInputs.TryRemove(name, out runningInput))
            {
                runningInput.Dispose();
            }

            var result = new LiteDbIndexInput(_db, name);
            _runningInputs.TryAdd(name, result);
            return result;
        }

        /// <summary>
        /// Release the memory and flush all the data in the files
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _runningInputs.Values.ForEach(z => z.Dispose());
            _runningOutputs.Values.ForEach(z => z.Dispose());
            _db.Dispose();
        }


    }
}