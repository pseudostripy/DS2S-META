using DS2S_META.Randomizer;
using DS2S_META.Utils;
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

        // common/handy queries for code cleanliness
        public static IEnumerable<ItemRow> FilterByType(this IEnumerable<ItemRow> itemrows, params eItemType[] types)
        {
            return itemrows.Where(it => types.Contains(it.ItemType));
        }
        public static IEnumerable<ItemRow> FilterOutId(this IEnumerable<ItemRow> itemrows, IEnumerable<ITEMID> badids)
        {
            return itemrows.Where(it => !badids.Cast<int>().Contains(it.ItemID));
        }
        public static IEnumerable<ItemRow> FilterOutUsage(this IEnumerable<ItemRow> itemrows, params ITEMUSAGE[] baduses)
        {
            return itemrows.Where(it => !baduses.Cast<int>().Contains(it.ItemUsageID));
        }
        public static IEnumerable<T> AsRows<T>(this Param? param)
        {
            if (param == null) throw new ArgumentNullException(nameof(param));
            return param.Rows.OfType<T>();
        }
        public static T RandomElement<T>(this IEnumerable<T> pool) => pool.ElementAt(Rng.Next(pool.Count()));
    }
}
