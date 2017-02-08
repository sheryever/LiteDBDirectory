using System.Collections.Generic;

namespace Lucene.Net.Store.LiteDbDirectory
{
    internal class LiteDbCollectionsInfo
    {
        public const string FileMetaData = "__FileMetaData";
        public const string FileLocks = "__FileLocks";
        public const string FileContents = "__FileContents";

        public static IEnumerable<string> Collections
        {
            get
            {
                yield return "__FileMetaData";
                yield return "__FileLocks";
                yield return "__FileContents";
            }
        }
    }
}