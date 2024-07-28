using DS2S_META.Utils.DS2Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public abstract class MemoryModification
    {
        protected DS2SHook Hook;
        public bool IsInstalled = false;

        abstract public void Install();
        abstract public void Uninstall();

        public MemoryModification(DS2SHook hook)
        {
            Hook = hook; 
        }
    }
}
