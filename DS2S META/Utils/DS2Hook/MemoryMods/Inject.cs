using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS2S_META.Utils.DS2Hook;
using PropertyHook;

namespace DS2S_META.Utils.DS2Hook.MemoryMods
{
    public abstract class Inject : MemoryModification
    {
        internal IntPtr InjAddr;
        internal byte[]? OrigBytes;
        internal byte[]? NewBytes;
        internal int InjLen => OrigBytes?.Length ?? 0;
        public const byte NOP = 0x90; // No-Op instruction x64

        public Inject(DS2SHook hook) : base(hook) { } // for subclassing
        
        internal bool Valid => InjAddr != IntPtr.Zero;
        internal IntPtr RetAddr => IntPtr.Add(InjAddr, InjLen);


        public override void Install()
        {
            // Wrapper for slightly tidier handling of injects
            Kernel32.WriteBytes(Hook.Handle, InjAddr, NewBytes); // install
        }
        public override void Uninstall()
        {
            Kernel32.WriteBytes(Hook.Handle, InjAddr, OrigBytes); // revert to original
        }
    }
}
