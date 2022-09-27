using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Util
{
    internal abstract class DS2Param
    {
        // Fields
        private DS2SHook Hook;

        // Methods
        public abstract void Read(); // read param element
    }
}
