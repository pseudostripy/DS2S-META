using DS2S_META.Dialog;
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
        private readonly DS2SHook Hook;
        public PHPointer? BaseA;
        public PHPointer? BaseB;
        private PHPointer? PHBaseA_OldBbj;
        

        public CorePHP(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            BaseA = HGO.ValOrNull(PHPDict, "BaseA");
            BaseB = HGO.ValOrNull(PHPDict, "BaseB");
            PHBaseA_OldBbj = HGO.ValOrNull(PHPDict, "BaseAOldBBJMod");

            // Old BBJ mod installed:
            if (BaseA == null)
                MetaWarningMessages.BasePtrNotFound(Hook.DS2Ver);
            
            // TODO
            //MetaInfoMessages.OldBbjNotImplemented();
            //if (BaseA == null && PHBaseA_OldBbj != null)
            //    BaseA = PHBaseA_OldBbj;
        }

        
    }
}
