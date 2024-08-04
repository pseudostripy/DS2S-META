using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook.MemoryMods
{
    internal class JumpInject : Inject
    {
        public IntPtr JmpToAddr { get; set; }
        public enum STDINJTYPE
        {
            R11ABSJUMP,
            RAXABSJUMP
        }

        // Standard constructor for jumping to a place to execute code, and then jumping back
        public JumpInject(DS2SHook hook, IntPtr injaddr, IntPtr jmpToAddr, byte[] origbytes, STDINJTYPE jmptype) : base(hook)
        {
            InjAddr = injaddr;
            JmpToAddr = jmpToAddr;
            OrigBytes = origbytes;
            if (OrigBytes != null)
                NewBytes = GetJmpBytes(jmptype, OrigBytes);
        }
        public JumpInject(DS2SHook hook, IntPtr injaddr, byte[] origbytes, byte[] customJmpBytes) : base(hook) 
        {
            // use this constructor when you've already pre-made the inject asm code
            if (origbytes.Length != customJmpBytes.Length)
            {
                MetaExceptionStaticHandler.Raise("Inject lengths unequal");
                return;
            }
            InjAddr = injaddr;
            OrigBytes = origbytes;
            NewBytes = customJmpBytes;
        }
        

        private byte[] GetJmpBytes(STDINJTYPE jmptype, byte[] origbytes)
        {
            return jmptype switch
            {
                STDINJTYPE.R11ABSJUMP => R11_AbsJumpBytes(origbytes, JmpToAddr),
                STDINJTYPE.RAXABSJUMP => RAX_AbsJumpBytes(origbytes, JmpToAddr),
                _ => throw new NotImplementedException(),
            };
        }

        // Common ASM machine code for injects
        public static readonly byte[] StandardR11Jmp = new byte[] { 0x49, 0xbb, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe3 }; // mov r11 ADDR; jmp r11
        public static readonly byte[] StandardRAXJmp = new byte[] { 0x48, 0xb8, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe0 }; // movabs rax ADDR; jmp rax

        private static byte[] R11_AbsJumpBytes(byte[] bytesToReplace, IntPtr ptrJumpLoc)
        {
            var inj = (byte[])StandardR11Jmp.Clone();   // shallow copy is fine
            inj = inj.NopExtend(bytesToReplace.Length); // fix inject length
            var ptrJumpLoc_asbytes = BitConverter.GetBytes(ptrJumpLoc.ToInt64()); // convert pointer to bytes
            Array.Copy(ptrJumpLoc_asbytes, 0x0, inj, 0x2, sizeof(long)); // update R11 jump location for inj
            return inj;
        }

        // CAREFUL! RAX is often used for calcs
        private static byte[] RAX_AbsJumpBytes(byte[] bytesToReplace, IntPtr ptrJumpLoc)
        {
            var inj = (byte[])StandardRAXJmp.Clone();   // shallow copy is fine
            inj = inj.NopExtend(bytesToReplace.Length); // fix inject length
            var ptrJumpLoc_asbytes = BitConverter.GetBytes(ptrJumpLoc.ToInt64()); // convert pointer to bytes
            Array.Copy(ptrJumpLoc_asbytes, 0x0, inj, 0x2, sizeof(long)); // update RAX jump location for inj
            return inj;
        }
    }
}
