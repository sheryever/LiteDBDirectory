using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.Store.LiteDbDirectory.Entities
{
    public class FileMetaData
    {
        public string Name { get; set; }
        public DateTime LastTouchedTimestamp { get; set; }
    }
}
