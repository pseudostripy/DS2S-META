using DS2S_META.Randomizer;
using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
        public static IEnumerable<DropInfo> FilterByItem(this IEnumerable<DropInfo> dropinfos, params ITEMID[] items)
        {
            return dropinfos.Where(di => items.Cast<int>().Contains(di.ItemID));
        }
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
        public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> pool, int count)
        {
            var choices = Enumerable.Range(0, pool.Count()).ToList().Shuffle().Take(count);
            var outlist = new List<T>(); // empty
            foreach (var choice in choices)
                outlist.Add(pool.ElementAt(choice));
            return outlist;
        }

        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rng.Next(n + 1); // 0 <= k <= j
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }

        public static string AsMetaName(this ITEMID itemid) => ((int)itemid).AsMetaName();
        public static string AsMetaName(this int itemid)
        {
            if (DS2Resource.ItemNames.TryGetValue(itemid, out var name))
                return name;
            return string.Empty;
        }

        public static string[] RegexSplit(this string source, string pattern) => Regex.Split(source, pattern);
    }
}
