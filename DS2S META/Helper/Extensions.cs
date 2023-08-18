using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DS2S_META.Randomizer.RandomizerManager;

namespace DS2S_META
{
    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int seed = 1;
            Random rng = new Random(seed);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static bool isKeys(this SetType settype)
        {
            return settype == SetType.Keys;
        }
    }
}
