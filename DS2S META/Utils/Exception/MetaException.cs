using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class MetaException : Exception
    {
        public string Etype;

        public MetaException(string etype, string? msg, Exception? innerException) : base(msg, innerException)
        {
            Etype = etype;
        }
    }
}
