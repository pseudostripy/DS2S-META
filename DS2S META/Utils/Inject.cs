using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DS2S_META.Utils
{
    internal class Inject
    {
        internal IntPtr InjAddr;
        internal byte[]? OrigBytes;
        internal byte[]? NewBytes;
        internal int InjLen => OrigBytes?.Length ?? 0;
        public const byte NOP = 0x90; // No-Op instruction x64

        public Inject(IntPtr injaddr, byte[] origbytes, byte[] newbytes) 
        {
            if (origbytes.Length != newbytes.Length)
            {
                MetaExceptionStaticHandler.Raise("Inject lengths unequal");
                return;
            }
            InjAddr = injaddr;
            OrigBytes = origbytes;  
            NewBytes = newbytes;
        }

        internal bool Valid => InjAddr != IntPtr.Zero;
        internal IntPtr RetAddr => IntPtr.Add(InjAddr, InjLen);
    }
}
