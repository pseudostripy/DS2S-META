using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.Randomizer.RandomizerManager;

namespace DS2S_META
{
    internal static class ExtensionMethods
    {
        public static bool IsKeys(this SetType settype) => settype == SetType.Keys;
        public static bool IsReqs(this SetType settype) => settype == SetType.Reqs;
        public static bool IsGens(this SetType settype) => settype == SetType.Gens;
    }
}
