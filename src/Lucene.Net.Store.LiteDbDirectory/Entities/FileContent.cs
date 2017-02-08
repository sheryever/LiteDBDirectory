using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.Store.LiteDbDirectory.Entities
{
    public class FileContent
    {
        public FileContent()
        {
            Id = DateTime.Now.Ticks;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        //public DateTime LastTouchedTimestamp { get; set; }
        public bool IsDeleted { get; set; }
    }
}
