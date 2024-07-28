using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS2S_META.Utils;
using DS2S_META.Utils.DS2Hook;

namespace DS2S_META.Utils
{
    public class Inject : MemoryModification
    {
        internal IntPtr InjAddr;
        internal byte[]? OrigBytes;
        internal byte[]? NewBytes;
        internal int InjLen => OrigBytes?.Length ?? 0;
        public const byte NOP = 0x90; // No-Op instruction x64

        // Common ASM machine code for injects
        public static readonly byte[] StandardR11Jmp = new byte[] { 0x49, 0xbb, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe3 }; // mov r11 ADDR; jmp r11
        public static readonly byte[] StandardRAXJmp = new byte[] { 0x48, 0xb8, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe0 }; // movabs rax ADDR; jmp rax
        
        public static byte[] R11_AbsJumpBytes(byte[] bytesToReplace, IntPtr ptrJumpLoc)
        {
            var inj = (byte[])StandardR11Jmp.Clone();   // shallow copy is fine
            inj = inj.NopExtend(bytesToReplace.Length); // fix inject length
            var ptrJumpLoc_asbytes = BitConverter.GetBytes(ptrJumpLoc.ToInt64()); // convert pointer to bytes
            Array.Copy(ptrJumpLoc_asbytes, 0x0, inj, 0x2, sizeof(Int64)); // update R11 jump location for inj
            return inj;
        }

        // CAREFUL! RAX is often used for calcs
        public static byte[] RAX_AbsJumpBytes(byte[] bytesToReplace, IntPtr ptrJumpLoc)
        {
            var inj = (byte[])StandardRAXJmp.Clone();   // shallow copy is fine
            inj = inj.NopExtend(bytesToReplace.Length); // fix inject length
            var ptrJumpLoc_asbytes = BitConverter.GetBytes(ptrJumpLoc.ToInt64()); // convert pointer to bytes
            Array.Copy(ptrJumpLoc_asbytes, 0x0, inj, 0x2, sizeof(Int64)); // update RAX jump location for inj
            return inj;
        }

        public Inject(DS2SHook hook, IntPtr injaddr, byte[] origbytes, byte[] newbytes) : base(hook)
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


        public override void Install()
        {
            throw new NotImplementedException();
        }
        public override void Uninstall() { throw new NotImplementedException(); }
    }
}
