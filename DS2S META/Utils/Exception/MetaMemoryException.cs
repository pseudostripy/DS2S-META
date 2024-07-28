using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class MetaMemoryException : MetaException
    {
        public const string TYPE_MEMORY_EXCEPTION = "Meta Memory Related Exception";
        private const string FTERR = "";
        public MetaMemoryException(string msg) : base(TYPE_MEMORY_EXCEPTION, FTERR + msg, null) { }
        public MetaMemoryException(string msg, Exception inner) : base(TYPE_MEMORY_EXCEPTION, msg, inner) { }
    }
}
