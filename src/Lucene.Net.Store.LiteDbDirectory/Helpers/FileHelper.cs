using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    public class FileHelper
    {
        public static long Length(LiteDatabase db, string contentName)
        {
            var fileInfo = db.FileStorage.FindById(contentName);
            if (fileInfo == null)
                return 0;

            return fileInfo.Length;
        }
    }
}
