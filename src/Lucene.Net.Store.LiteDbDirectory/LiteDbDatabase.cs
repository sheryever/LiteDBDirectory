using System.Collections.Generic;

namespace Lucene.Net.Store.LiteDbDirectory
{
    public class LiteDbDatabase
    {
        public const string FileMetaData = "FileMetaData";
        public const string FileLocks = "FileLocks";
        public const string FileContents = "FileContents";
        public static IEnumerable<string> Collections
        {
            get
            {
                yield return "FileMetaData";
                yield return "FileLocks";
                yield return "FileContents";
            }
        }

        //public static IEnumerable<string> Structure()
        //{
        //    //yield return "CREATE TABLE [FileMetaData] ( [Name] TEXT NOT NULL  PRIMARY KEY ASC, LastTouchedTimestamp DATETIME NOT NULL)";
        //    yield return "CREATE TABLE [FileMetaData] ([Name] TEXT NOT NULL PRIMARY KEY ASC, LastTouchedTimestamp DATETIME NOT NULL)";
        //    //yield return "ALTER TABLE [FileMetaData] ADD CONSTRAINT PK_FileMetaData PRIMARY KEY NONCLUSTERED ([Name] ASC)";
        //    yield return "CREATE TABLE [Locks] ( [Name] TEXT NOT NULL  PRIMARY KEY ASC, LockReleaseTimestamp DATETIME NOT NULL)";
        //    //yield return "ALTER TABLE [Locks] ADD CONSTRAINT PK_Locks PRIMARY KEY NONCLUSTERED ([Name] ASC)";
        //    yield return "CREATE TABLE [FileContents] ( Id INTEGER PRIMARY KEY AUTOINCREMENT, [Name] TEXT NOT NULL  CONSTRAINT UniqueFileName UNIQUE,[Content] BLOB DEFAULT NULL)";
        //    //yield return "ALTER TABLE [FileContents] ADD CONSTRAINT PK_FileContents PRIMARY KEY NONCLUSTERED ([Name] ASC)";
        //}
    }
}