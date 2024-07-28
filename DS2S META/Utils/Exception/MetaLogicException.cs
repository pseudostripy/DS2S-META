using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class MetaLogicException : MetaException
    {
        public const string TYPE_LOGIC_EXCEPTION = "Meta Logical Issue";
        private const string FTERR = "";
        public MetaLogicException(string msg) : base(TYPE_LOGIC_EXCEPTION, FTERR + msg, null) { }
        public MetaLogicException(string msg, Exception inner) : base(TYPE_LOGIC_EXCEPTION, msg, inner) { }
    }
}
