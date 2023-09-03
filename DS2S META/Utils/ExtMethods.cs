using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public static class ExtMethods
    {
        public static bool CheckFeature(this DS2SHook? hook, METAFEATURE feat)
        {
            if (hook == null)
                return false;
            return hook.Hooked && hook.IsFeatureCompatible(feat);
        }
        public static bool InGameAndFeature(this DS2SHook? hook, METAFEATURE feat)
        {
            if (hook == null)
                return false;
            return hook.InGame && hook.IsFeatureCompatible(feat);
        }
    }
}
