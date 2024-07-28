using PropertyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Xps.Serialization;
using DS2S_META.Utils.DS2Hook;

namespace DS2S_META.Utils
{
    internal class AllocatedAsm : MemoryModification
    {
        private readonly uint PageType = Kernel32.PAGE_READWRITE;
        private readonly uint AllocSize;
        private bool IsAllocated = false;
        private IntPtr PtrAllocMem;
        public byte[]? Asm;


        public AllocatedAsm(DS2SHook hook, uint sz, bool exec = true) : base(hook)
        {
            if (sz == 0) throw new Exception("Cannot have 0-size inject");
            AllocSize = sz;
            if (exec) PageType = Kernel32.PAGE_EXECUTE_READWRITE;
        }
        public IntPtr Allocate()
        {
            if (IsAllocated) return PtrAllocMem;
            PtrAllocMem = Hook.Allocate(AllocSize, flProtect: PageType);
            if (PtrAllocMem == IntPtr.Zero) throw new Exception("Issue allocating desired memory");
            IsAllocated = true;
            return PtrAllocMem;
        }

        public void SetAsmBytes(byte[] asm)
        {
            // declare the main machine code to place in the allocated memory upon install
            Asm = asm;
        }
        public override void Install()
        {
            if (Asm == null) throw new MetaMemoryException("Cannot install an allocated inject with nothing to write there");
            if (!IsAllocated)
                Allocate();
            Kernel32.WriteBytes(Hook.Handle, PtrAllocMem, Asm);
        }
        public override void Uninstall()
        {
            Hook.Free(PtrAllocMem);
        }
    }
}
