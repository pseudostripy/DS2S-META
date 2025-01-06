using DS2S_META.Utils;
using DS2S_META.Utils.DS2Hook;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.DS2Versions;
using static DS2S_META.Utils.DS2Hook.DS2SHook;

namespace DS2S_META
{
    public enum DS2VER
    {
        VANILLA_V112,
        VANILLA_V111,
        VANILLA_V102,
        SOTFS_V102,
        SOTFS_V103,
        UNSUPPORTED,
        UNHOOKED,
    }

    // primary constructor
    public class DS2Versions(DS2SHook hook)
    {
        public readonly static List<DS2VER> ANYVER = [ DS2VER.SOTFS_V103, DS2VER.SOTFS_V102, DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 ];
        public readonly static List<DS2VER> ANYSOTFS = [ DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 ];
        public readonly static List<DS2VER> ANYVANILLA = [ DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 ];
        public readonly static List<DS2VER> ALLBUTOLDPATCH = [ DS2VER.SOTFS_V102, DS2VER.SOTFS_V103, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 ];
        public readonly static List<DS2VER> V102 = [ DS2VER.VANILLA_V102 ];
        public readonly static List<DS2VER> V111 = [ DS2VER.VANILLA_V111 ];
        public readonly static List<DS2VER> V112 = [ DS2VER.VANILLA_V112 ];
        public readonly static List<DS2VER> S102 = [ DS2VER.SOTFS_V102 ];
        public readonly static List<DS2VER> S103 = [ DS2VER.SOTFS_V103 ];
        public readonly static List<DS2VER> V111OR112 = [ DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 ];

        // Module sizes (size of game in bytes)
        private const int ModuleSizeS103 = 0x1D76000;  // _OnlineCPSotfs
        private const int ModuleSizeS102 = 0x20B6000;  // _VulnPatchSotfs
        private const int ModuleSizeV112 = 0x01DB4000; // _OnlineCP
        private const int ModuleSizeV111 = 0x02055000; // _VulnPatch
        private const int ModuleSizeV102 = 0x0133e000; // _OldPatch

        // DS2 & BBJ Process Info Data
        private enum BYTECODES
        {
            // used for sotfs differentiation:
            NOBBJBYTE = 0xF3,
            NEWBBJBYTE = 0x49,

            // used for vanilla differentiation:
            JUMPREL32 = 0xE9,
            MOV_ECX_EAX = 0x8B,
            MOV_EAX_DWORTPTR = 0x8B,
            MOVSS = 0xF3,
        }
        public enum BBJTYPE
        {
            NOBBJ,
            OLDBBJ_VANILLA,
            NEWBBJ_VANILLA,
            OLDBBJ_SOTFS,
            NEWBBJ_SOTFS,
            UNKN_VANILLA,
            TMPUNCHECKED,
        }

        // WIP
        private bool Hooked => hook?.Hooked == true;
        public bool IsOldPatch => Hooked && DS2Ver == DS2VER.VANILLA_V102;
        public bool IsSOTFS_CP => Hooked && DS2Ver == DS2VER.SOTFS_V103;
        public bool IsSOTFS => Hooked && ANYSOTFS.Contains(DS2Ver);
        public bool IsVanilla => ANYVANILLA.Contains(DS2Ver);
        public bool IsValidVer => IsSOTFS || IsVanilla;
        private DS2VER _ver = DS2VER.UNHOOKED;
        public DS2VER DS2Ver => _ver;
        private BBJTYPE _bbjtype;
        public BBJTYPE BbjType => _bbjtype;
        private string? _verInfoString = null;
        public string? VerInfoString => _verInfoString;

        public void OnHooked()
        {
            // Size of running DS2 application
            var moduleSz = hook.Process?.MainModule?.ModuleMemorySize;
            _ver = hook.Is64Bit ? GetSotfsVer(moduleSz) : GetVanillaVer(moduleSz);
            _bbjtype = BBJTYPE.TMPUNCHECKED; // TODO GetBBJType();
            _verInfoString = GetStringVersion();
        }
        
        private static DS2VER GetVanillaVer(int? modulesz)
        {
            return modulesz switch
            {
                ModuleSizeV112 => DS2VER.VANILLA_V112,
                ModuleSizeV111 => DS2VER.VANILLA_V111,
                ModuleSizeV102 => DS2VER.VANILLA_V102,
                _ => DS2VER.UNSUPPORTED,
            };
        }
        private static DS2VER GetSotfsVer(int? modulesz)
        {
            return modulesz switch
            {
                ModuleSizeS102 => DS2VER.SOTFS_V102,
                ModuleSizeS103 => DS2VER.SOTFS_V103,
                _ => DS2VER.UNSUPPORTED,
            };
        }
        internal BBJTYPE GetBBJType(bool isOldBbj)
        {
            // TODO VANILLA
            if (IsVanilla)
                return GetBBJTypeVanilla();

            if (isOldBbj)
                return BBJTYPE.OLDBBJ_SOTFS;


            // check for new bbj
            int jumpfcn_offset_V102 = 0x037B4BC;
            int jumpfcn_offset_V103 = 0x0381E1C;
            var jmpfcn_offset = IsSOTFS_CP ? jumpfcn_offset_V103 : jumpfcn_offset_V102;

            var module_addr = hook.Process?.MainModule?.BaseAddress
                ?? throw new Exception("Unknown DS2 MainModule size");
            
            var jmp_ptr = IntPtr.Add(module_addr, jmpfcn_offset);

            // Read a byte to see if the bbj inject is there:
            var jumpinj = hook.CreateBasePointer(jmp_ptr);
            byte testbyte = jumpinj.ReadByte(0);
            return testbyte switch
            {
                (byte)BYTECODES.NOBBJBYTE => BBJTYPE.NOBBJ,
                (byte)BYTECODES.NEWBBJBYTE => BBJTYPE.NEWBBJ_SOTFS,
                _ => throw new Exception("Probably an issue with setting up the pointers/addresses"),
            };
        }
        internal BBJTYPE GetBBJTypeVanilla()
        {
            var jmpfcn_offset = DS2Ver switch
            {
                DS2VER.VANILLA_V102 => 0x033A424,
                DS2VER.VANILLA_V111 => 0x3A09C4,
                DS2VER.VANILLA_V112 => 0x3A7364,
                _ => throw new Exception("Shouldn't get here")
            };

            var module_addr = hook.Process?.MainModule?.BaseAddress ??
                    throw new Exception("Unknown DS2 MainModule size");
            var jmp_ptr = IntPtr.Add(module_addr, jmpfcn_offset);

            // Read a byte to see if the bbj inject is there:
            var jumpinj = hook.CreateBasePointer(jmp_ptr);
            byte testbyte = jumpinj.ReadByte(0);
            bool isInjected = testbyte switch
            {
                (byte)BYTECODES.MOV_ECX_EAX => false,
                (byte)BYTECODES.JUMPREL32 => true,
                _ => throw new Exception("Shouldn't happen for Vanilla?"),
            };

            // Split out easy bbj types:
            if (!isInjected) return BBJTYPE.NOBBJ;
            if (DS2Ver == DS2VER.VANILLA_V112) return BBJTYPE.NEWBBJ_VANILLA; // only new version available
            if (DS2Ver == DS2VER.VANILLA_V102) return BBJTYPE.NEWBBJ_VANILLA; // only new version available

            // Finally differentiate between V1.11 old/new bbj mods:
            var reljump = jumpinj.ReadInt32(0x1); // read rel_jump (E9 XXXXXXXX LE)
            var addr_inj_code = jumpinj.Resolve() + reljump + 5; // 5 for instruction length
            var inj_code = hook.CreateBasePointer(addr_inj_code);
            var testbyte2 = inj_code.ReadByte(0xE); // first byte that is different between versions

            // Differentiate:
            return testbyte2 switch
            {
                (byte)BYTECODES.MOVSS => BBJTYPE.OLDBBJ_VANILLA,
                (byte)BYTECODES.MOV_EAX_DWORTPTR => BBJTYPE.NEWBBJ_VANILLA,
                _ => throw new Exception("Probably shouldn't get this, unknown bbj inject")
            };
        }

        private string GetStringVersion()
        {
            if (!IsSOTFS && !IsVanilla)
                return "Unknown game or version";

            // get data:
            var gametype = IsSOTFS ? "Sotfs" : "Vanilla";
            var calib = GetCalibrationString();
            var modtype = GetModTypeString();

            // build string:
            StringBuilder sb = new();
            sb.Append(gametype);
            sb.Append(' ');
            sb.Append(calib);
            sb.Append(' ');
            sb.Append(modtype);
            return sb.ToString();
        }
        private string GetModTypeString()
        {
            return BbjType switch
            {
                BBJTYPE.NOBBJ => "(unmodded)",
                BBJTYPE.OLDBBJ_VANILLA or BBJTYPE.OLDBBJ_SOTFS => "(old bbj mod)",
                BBJTYPE.NEWBBJ_VANILLA or BBJTYPE.NEWBBJ_SOTFS => "(bbj mod)",
                _ => "(unknown mod)",
            };
        }
        private string GetCalibrationString()
        {
            return DS2Ver switch
            {
                DS2VER.SOTFS_V102 => "V1.02",
                DS2VER.SOTFS_V103 => "V1.03",
                DS2VER.VANILLA_V102 => "V1.02 Old Patch",
                DS2VER.VANILLA_V111 => "V1.11",
                DS2VER.VANILLA_V112 => "V1.12",
                _ => "Unknown calibration"
            };
        }
    }
}
