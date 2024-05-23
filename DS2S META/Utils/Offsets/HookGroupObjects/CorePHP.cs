using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class CorePHP
    {
        private DS2SHook Hook;
        public PHPointer? BaseA;
        public PHPointer? BaseB;
        

        public CorePHP(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            BaseA = HGO.ValOrNull(PHPDict, "BaseA");
            BaseB = HGO.ValOrNull(PHPDict, "BaseB");
        }

        
    }
}
