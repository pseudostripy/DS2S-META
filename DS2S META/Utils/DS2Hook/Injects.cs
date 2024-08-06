using PropertyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook
{
    /// <summary>
    ///  Class to contain things which directly affect game .text data
    /// </summary>
    public class Injects
    {
        public enum NOPINJECTS
        {
            DISABLESKIRT,
            INFINITESPELLS,
            INFINITEGOODS,
        }
            
        private class VerBytesArrayDef
        {
            public NOPINJECTS ID;
            public VerBytes[] VerbyteArray;
            public VerBytesArrayDef(NOPINJECTS id, params VerBytes[] vbs)
            {
                ID = id;
                VerbyteArray = vbs;
            }
        }
        private class VerBytes
        {
            public List<DS2VER> ValidVers;
            public byte[] Bytes;
            public VerBytes(List<DS2VER> validvers, byte[] bytes)
            { 
                ValidVers = validvers;
                Bytes = bytes;
            }
        }

        public static byte[] GetDefinedBytes(DS2VER ver, NOPINJECTS nm)
        {
            var varbytes = NopByteDefinitions.FirstOrDefault(vb => vb.ID == nm)
                ?? throw new Exception($"Cannot find any bytes under the NOPINJECTS enum: {nm}");

            var vbs = varbytes.VerbyteArray.Where(vb => vb.ValidVers.Contains(ver)).ToList();
            if (vbs.Count == 0)
                throw new Exception($"Cannot find defined bytes for {nm} for DS2 version {ver}");

            if (vbs.Count > 1)
                throw new Exception($"Multipe byte definitions for {nm} for DS2 version {ver}");

            return vbs.First().Bytes;
        }
        private static readonly List<VerBytesArrayDef> NopByteDefinitions = new()
        {
            new VerBytesArrayDef(NOPINJECTS.DISABLESKIRT, new VerBytes(DS2Versions.S103, new byte[] { 0x89, 0x84, 0x8B, 0xC4, 0x01, 0x00, 0x00 })),
            new VerBytesArrayDef(NOPINJECTS.INFINITESPELLS, new VerBytes(DS2Versions.V102, new byte[] {0x88, 0x43, 0x18}),
                                                            new VerBytes(DS2Versions.S103, new byte[] {0x88, 0x4D, 0x20})),
            new VerBytesArrayDef(NOPINJECTS.INFINITEGOODS, new VerBytes(DS2Versions.S103, new byte[] {0x66, 0x29, 0x73, 0x20})),
        };

        //internal bool ApplyBIKP1Skip(bool enable)
        //{
        //    // Last resort graceful failure
        //    if (DS2P.Func.ApplySpEffect == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.ApplySpEffect");
        //    if (DS2P.MiscPtrs.SpEffectCtrl == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.SpEffectCtrl");
        //    if (DS2P.Core.BaseA == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.BaseA");

        //    // Change some constants read by the BIK fight I guess.
        //    // Carbon copy from https://www.nexusmods.com/darksouls2/mods/1043 .
        //    // Haven't bothered to figure out how it works.
        //    byte[] DISABLEMOD = new byte[2] { 0x0, 0x0 };
        //    byte[] ENABLEMOD_VAL1 = new byte[2] { 0x80, 0x9c };
        //    byte[] ENABLEMOD_VAL2 = new byte[2] { 0x0e, 0x3c };
        //    var val1_bytes = enable ? ENABLEMOD_VAL1 : DISABLEMOD;
        //    var val2_bytes = enable ? ENABLEMOD_VAL2 : DISABLEMOD;

        //    // enable/disable phase1
        //    DS2P.MiscPtrs.PHBIKP1Skip_Val1?.WriteBytes(val1_bytes);
        //    DS2P.MiscPtrs.PHBIKP1Skip_Val2?.WriteBytes(val2_bytes);

        //    return enable; // turned on or off now
        //}

        //public MultiStageMod SetupDmgMod(DS2SHook hook, bool affectDealtDmg, bool affectRecvDmg, int dmgFactorDealt, int dmgFactorRecvd)
        //{
        //    // This looks a little complicated because we don't yet know
        //    // where the memory will be allocated.
        //    // So all of the pointers need to be figured out and updated dynamically.


        //    //if (MetaFeature.IsInactive(METAFEATURE.DMGMOD))
        //    //    return false;

        //    //if (DmgModInstalled)
        //    //    UninstallDmgMod(); // start from a fresh inject incase settings change

        //    // Allocate memory for our mod
        //    List<MemoryModification> mods = new();
        //    AllocatedInject allcode = new(hook, 0x1000);
        //    var pmem = allcode.Allocate();

        //    var module_addr = hook?.Process?.MainModule?.BaseAddress ?? throw new MetaMemoryException("Cannot find DS2 Module address for hooks/inject calculations");
            
        //    // First inject (amBeingHit)
        //    var inj1_offset = 0x17aa65; // inject for figuring out if being hit [todo load from SOTFS_v1.03 offsets]
        //    var p_inj1 = IntPtr.Add(module_addr, inj1_offset);
        //    var inj1_ob = new byte[] { 0x48, 0x89, 0x44, 0x24, 0x28, 0x48, 0x8b, 0x44, 0x24, 0x60, 0x48, 0x89, 0x44, 0x24, 0x20 };
        //    var inj1_jmpLoc = IntPtr.Add(pmem, 0x7e); // see assembly script
        //    var inj1_nb = Inject.R11_AbsJumpBytes(inj1_ob, inj1_jmpLoc);
        //    var inj1 = new Inject(p_inj1, inj1_ob, inj1_nb);

        //    // mov r11 ADDR; jmp r11
        //    //var inj1len = 0xf;
        //    //var inj1_nb_st = new byte[] { 0x49, 0xbb, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe3 };
        //    //var inj1_nb = inj1_nb_st.NopExtend(inj1len);
        //    // fix and install
        //    //var inj1code_bytes = BitConverter.GetBytes(p_code1st.ToInt64());        // convert ptrAddr to byte[]
        //    //Array.Copy(inj1code_bytes, 0x0, inj1_nb, 0x2, inj1code_bytes.Length);   // overwrite FFFFFFFF 00000000
        //    //if (!inj1.Valid) return false;


        //    // Setup addresses for my new code:
        //    //var memalloc = Allocate(0x1000, flProtect: Kernel32.PAGE_EXECUTE_READWRITE); // host the assembly in memory
        //    //var p_code2st = IntPtr.Add(memalloc, 0x0);        // first thing
        //    //var p_code1st = p_code2st + 0x7e;                 // see assembly script


        //    // Second inject (dmgCalculation)
        //    var inj2_offset = 0x138F77; // todo as above
        //    var p_inj2 = IntPtr.Add(module_addr, inj2_offset);
        //    var inj2_ob = new byte[] { 0x49, 0x8b, 0x46, 0x08, 0xf3, 0x41, 0x0f, 0x5e, 0xf1, 0xf3, 0x0f, 0x59, 0x70, 0x1c };
        //    var inj2_nb = Inject.RAX_AbsJumpBytes(inj2_ob, pmem);
        //    var inj2 = new Inject(p_inj2, inj2_ob, inj2_nb);

        //    //var inj2len = 0xe;
        //    //var inj2_nb = Enumerable.Repeat(Inject.NOP, inj2len).ToArray();            // prefill as nops
        //    //var hdrinj2bytes = new byte[2] { 0x48, 0xb8 };                      // movabs rax, x
        //    //var inj2code_bytes = BitConverter.GetBytes(p_code2st.ToInt64());    // FFFFFFFF 00000000
        //    //var inj2_jbytes = new byte[2] { 0xff, 0xe0 };                       // jmp rax
        //    //// build & install inj2
        //    //Array.Copy(hdrinj2bytes, 0x0, inj2_nb, 0x0, hdrinj2bytes.Length);
        //    //Array.Copy(inj2code_bytes, 0x0, inj2_nb, 0x2, inj2code_bytes.Length);
        //    //Array.Copy(inj2_jbytes, 0x0, inj2_nb, 0xa, inj2_jbytes.Length);




        //    // Prep assembly substitutions
        //    var MEMSTORE_OFFSET = 0x100;
        //    var pDataStore = IntPtr.Add(pmem, MEMSTORE_OFFSET);
        //    var amDealingHit = IntPtr.Add(pDataStore, 0);
        //    var enDealNoDmg = IntPtr.Add(pDataStore, 0x8);
        //    var enTakeNoDmg = IntPtr.Add(pDataStore, 0x10);
        //    var amDealingHit_bytes = BitConverter.GetBytes(amDealingHit.ToInt64());
        //    var enDealNoDmg_bytes = BitConverter.GetBytes(enDealNoDmg.ToInt64());
        //    var enTakeNoDmg_bytes = BitConverter.GetBytes(enTakeNoDmg.ToInt64());
        //    var inj1ret_bytes = BitConverter.GetBytes(inj1.RetAddr.ToInt64());
        //    var inj2ret_bytes = BitConverter.GetBytes(inj2.RetAddr.ToInt64());
        //    var dmgfacDealt_bytes = BitConverter.GetBytes(dmgFactorDealt);
        //    var dmgfacRecvd_bytes = BitConverter.GetBytes(dmgFactorRecvd);


        //    // Clone reference assembly and populate links
        //    var asm = (byte[])DS2SAssembly.NoDmgMod.Clone();
        //    Array.Copy(amDealingHit_bytes, 0, asm, 0x10, amDealingHit_bytes.Length);
        //    Array.Copy(enDealNoDmg_bytes, 0, asm, 0x23, enDealNoDmg_bytes.Length);
        //    Array.Copy(enTakeNoDmg_bytes, 0, asm, 0x46, enTakeNoDmg_bytes.Length);
        //    Array.Copy(amDealingHit_bytes, 0, asm, 0x64, amDealingHit_bytes.Length);
        //    Array.Copy(inj2ret_bytes, 0, asm, 0x74, inj2ret_bytes.Length);
        //    Array.Copy(amDealingHit_bytes, 0, asm, 0x8f, amDealingHit_bytes.Length);
        //    Array.Copy(inj1ret_bytes, 0, asm, 0x9f, inj1ret_bytes.Length);
        //    Array.Copy(dmgfacDealt_bytes, 0, asm, 0x35, dmgfacDealt_bytes.Length); // dealt dmgfactor if enabled
        //    Array.Copy(dmgfacRecvd_bytes, 0, asm, 0x58, dmgfacRecvd_bytes.Length); // recv dmgfactor if enabled



        //    // Write machine code into memory:
        //    inj1.Install();
        //    inj2.Install();
        //    //InstallInject(inj1);
        //    //InstallInject(inj2);
        //    Kernel32.WriteBytes(hook.Handle, pmem, asm); // install dmgmod code in FIRST HALF of alloc mem

        //    // Populate settings:
        //    byte dealNoDmg_byte = affectDealtDmg ? (byte)1 : (byte)0;
        //    byte takeNoDmg_byte = affectRecvDmg ? (byte)1 : (byte)0;
        //    Kernel32.WriteByte(Handle, enDealNoDmg, dealNoDmg_byte);
        //    Kernel32.WriteByte(Handle, enTakeNoDmg, takeNoDmg_byte);

        //    // done
        //    DmgModInj1 = inj1;
        //    DmgModInj2 = inj2;
        //    DmgModCodeAddr = memalloc;
        //    return true; // success
        //}
    }
}
