using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook.MemoryMods
{
    public class NoDmgMod : MemoryModification
    {
        private Inject? Inj1;
        private Inject? Inj2;
        private IntPtr? ModuleAddr;
        private IntPtr AllocMemAddr;
        private AllocatedAsm? AllocMem;

        private const int MEMSTORE_OFFSET = 0x100;

        public NoDmgMod(DS2SHook hook) : base(hook)
        {
        }

        public override void Install()
        {
            // This looks a little complicated because we don't yet know
            // where the memory will be allocated.
            // So all of the pointers need to be figured out and updated dynamically.
            
            // Allocate memory for our mod
            ModuleAddr = Hook?.Process?.MainModule?.BaseAddress ?? throw new MetaMemoryException("Cannot find DS2 Module address for hooks/inject calculations");
            AllocMem = new AllocatedAsm(Hook, 0x1000);  // Create the assembly alloc object
            AllocMemAddr = AllocMem.Allocate();         // Actually do the allocation to get the ptr

            // Create Injects and main asm
            Inj1 = SetupFirstInject();
            Inj2 = SetupSecondInject();
            AllocMem.SetAsmBytes(CreateCoreModAsm());
            
            // Install them
            Inj1.Install();
            Inj2.Install();
            AllocMem.Install();
            IsInstalled = true;
        }
        public override void Uninstall() 
        {
            if (!IsInstalled) return;
            Inj1?.Uninstall();
            Inj2?.Uninstall();
        }

        private Inject SetupFirstInject()
        {
            if (ModuleAddr == null) throw new Exception("Null Ptr apparently");

            // First inject (amBeingHit)
            var inj1_offset = 0x17aa65; // inject for figuring out if being hit [todo load from SOTFS_v1.03 offsets]
            var p_inj1 = IntPtr.Add((IntPtr)ModuleAddr, inj1_offset);
            var inj1_ob = new byte[] { 0x48, 0x89, 0x44, 0x24, 0x28, 0x48, 0x8b, 0x44, 0x24, 0x60, 0x48, 0x89, 0x44, 0x24, 0x20 };
            var inj1_jmpLoc = IntPtr.Add(AllocMemAddr, 0x7e); // see assembly script
            var inj1_nb = Inject.R11_AbsJumpBytes(inj1_ob, inj1_jmpLoc);
            return new Inject(Hook, p_inj1, inj1_ob, inj1_nb);
        }
        private Inject SetupSecondInject()
        {
            if (ModuleAddr == null) throw new Exception("Null Ptr apparently");

            // Second inject (dmgCalculation)
            var inj2_offset = 0x138F77; // todo as above
            var p_inj2 = IntPtr.Add((IntPtr)ModuleAddr, inj2_offset);
            var inj2_ob = new byte[] { 0x49, 0x8b, 0x46, 0x08, 0xf3, 0x41, 0x0f, 0x5e, 0xf1, 0xf3, 0x0f, 0x59, 0x70, 0x1c };
            var inj2_nb = Inject.RAX_AbsJumpBytes(inj2_ob, AllocMemAddr);
            return new Inject(Hook, p_inj2, inj2_ob, inj2_nb);
        }
        private byte[] CreateCoreModAsm()
        {
            int TMP0 = 0;

            // Prep assembly substitutions
            var pDataStore = IntPtr.Add(AllocMemAddr, MEMSTORE_OFFSET);
            var amDealingHit = IntPtr.Add(pDataStore, 0);
            var enDealNoDmg = IntPtr.Add(pDataStore, 0x8);
            var enTakeNoDmg = IntPtr.Add(pDataStore, 0x10);
            var amDealingHit_bytes = BitConverter.GetBytes(amDealingHit.ToInt64());
            var enDealNoDmg_bytes = BitConverter.GetBytes(enDealNoDmg.ToInt64());
            var enTakeNoDmg_bytes = BitConverter.GetBytes(enTakeNoDmg.ToInt64());
            var inj1ret_bytes = BitConverter.GetBytes(Inj1.RetAddr.ToInt64());
            var inj2ret_bytes = BitConverter.GetBytes(Inj2.RetAddr.ToInt64());
            var dmgfacDealt_bytes = BitConverter.GetBytes(TMP0); // dmgFactorDealt (init)
            var dmgfacRecvd_bytes = BitConverter.GetBytes(TMP0); // dmgFactorRecvd (init)


            // Clone reference assembly and populate links
            var asm = (byte[])DS2SAssembly.NoDmgMod.Clone();
            Array.Copy(amDealingHit_bytes, 0, asm, 0x10, amDealingHit_bytes.Length);
            Array.Copy(enDealNoDmg_bytes, 0, asm, 0x23, enDealNoDmg_bytes.Length);
            Array.Copy(enTakeNoDmg_bytes, 0, asm, 0x46, enTakeNoDmg_bytes.Length);
            Array.Copy(amDealingHit_bytes, 0, asm, 0x64, amDealingHit_bytes.Length);
            Array.Copy(inj2ret_bytes, 0, asm, 0x74, inj2ret_bytes.Length);
            Array.Copy(amDealingHit_bytes, 0, asm, 0x8f, amDealingHit_bytes.Length);
            Array.Copy(inj1ret_bytes, 0, asm, 0x9f, inj1ret_bytes.Length);
            Array.Copy(dmgfacDealt_bytes, 0, asm, 0x35, dmgfacDealt_bytes.Length); // dealt dmgfactor if enabled
            Array.Copy(dmgfacRecvd_bytes, 0, asm, 0x58, dmgfacRecvd_bytes.Length); // recv dmgfactor if enabled
            return asm;
        }
    
        public void SetDmgModSettings(bool affectDealtDmg, bool affectRecvDmg, int dmgFactorDealt, int dmgFactorRecvd)
        {
            if (!IsInstalled) throw new MetaMemoryException("Trying to update NoDmgMod memory before it is installed");

            // Convert settings to bytes:
            byte dealNoDmg_byte = affectDealtDmg ? (byte)1 : (byte)0;
            byte takeNoDmg_byte = affectRecvDmg ? (byte)1 : (byte)0;
            var dmgfacDealt_bytes = BitConverter.GetBytes(dmgFactorDealt);
            var dmgfacRecvd_bytes = BitConverter.GetBytes(dmgFactorRecvd);

            // Re-find pointers used during setup
            var pDataStore = IntPtr.Add(AllocMemAddr, MEMSTORE_OFFSET);
            var amDealingHit = IntPtr.Add(pDataStore, 0);
            var pDealNoDmg = IntPtr.Add(pDataStore, 0x8);
            var pTakeNoDmg = IntPtr.Add(pDataStore, 0x10);
            var pDmgDealtFac = IntPtr.Add(AllocMemAddr, 0x35);
            var pDmgRcvdFac = IntPtr.Add(AllocMemAddr, 0x58);

            // Update memory
            Kernel32.WriteByte(Hook.Handle, pDealNoDmg, dealNoDmg_byte);
            Kernel32.WriteByte(Hook.Handle, pTakeNoDmg, takeNoDmg_byte);
            Kernel32.WriteBytes(Hook.Handle, pDmgDealtFac, dmgfacDealt_bytes);
            Kernel32.WriteBytes(Hook.Handle, pDmgRcvdFac, dmgfacRecvd_bytes);
        }
    }
}
