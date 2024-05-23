using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class MiscPtrs
    {
        private DS2SHook Hook;
        public PHPointer? AvailableItemBag;
        

        public MiscPtrs(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            AvailableItemBag = ValOrNull(PHPDict, "AvailableItemBag");
        }

        private static PHPointer? ValOrNull(Dictionary<string, PHPointer> PHPDict, string key)
        {
            PHPDict.TryGetValue(key, out var value);
            return value;
        }
    }
}
