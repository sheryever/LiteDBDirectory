using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;
using Lucene.Net.Store.LiteDbDirectory.Entities;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Get the length of a <see cref="FileContent"/>
        /// </summary>
        /// <param name="db">The LiteDatabase object</param>
        /// <param name="contentFileName">The name of the <see cref="FileContent"/></param>
        /// <returns>The length of the file content</returns>
        public static long GetContentFileDataLength(LiteDatabase db, string contentFileName)
        {
            var fileInfo = db.FileStorage.FindById(contentFileName);
            if (fileInfo == null)
                return 0;

            return fileInfo.Length;
        }
    }
}
