using System;
using System.Collections.Generic;
using System.Text;

namespace Lucene.Net.Store.LiteDbDirectory
{
    public class ConfigurationErrorsException : Exception
    {
        public ConfigurationErrorsException(string message) : base(message)
        {
        }
    }
}
