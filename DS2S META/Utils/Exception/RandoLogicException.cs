using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public class RandoLogicException : MetaException
    {
        public const string TYPE_LOGIC_EXCEPTION = "Randomizer Logic Issue";
        private const string FTERR = "";
        public RandoLogicException(string msg) : base(TYPE_LOGIC_EXCEPTION, FTERR + msg, null) { }
        public RandoLogicException(string msg, Exception inner) : base(TYPE_LOGIC_EXCEPTION, msg, inner) { }
    }
}
