using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    internal static class DS2ModuleInfo
    {
        
        internal static class ModuleSizes
        {
            internal const int SotfsV103 = 0x1D76000;    // _OnlineCPSotfs
            internal const int SotfsV102 = 0x20B6000;    // _VulnPatchSotfs
            internal const int VanillaV112 = 0x01DB4000; // _OnlineCP
            internal const int VanillaV111 = 0x02055000; // _VulnPatch
            internal const int VanillaV102 = 0x0133e000; // _OldPatch
        }
    }
}
