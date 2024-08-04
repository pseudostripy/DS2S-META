using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.Utils.DS2Hook.MemoryMods.JumpInject;

namespace DS2S_META.Utils.DS2Hook.MemoryMods
{
    public class NopableInject : Inject
    {
        public NopableInject(DS2SHook hook, IntPtr injaddr, byte[] origbytes) : base(hook)
        {
            InjAddr = injaddr;
            OrigBytes = origbytes;
            NewBytes = Enumerable.Repeat(NOP, origbytes.Length).ToArray();
        }


    }
}
