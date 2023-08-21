using DS2S_META.Randomizer;
using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.Interop;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using static DS2S_META.Randomizer.RandomizerManager;

namespace DS2S_META
{
    internal static class ExtensionMethods
    {
        // Common filtering queries:
        public static IEnumerable<DropInfo> FilterByItem(this IEnumerable<DropInfo> dropinfos, params ITEMID[] items)
        {
            return dropinfos.Where(di => items.Cast<int>().Contains(di.ItemID));
        }
        public static IEnumerable<DropInfo> FilterByItemType(this IEnumerable<DropInfo> dropinfos, params eItemType[] goodtypes)
        {
            return dropinfos.FilterByItemType(goodtypes);
        }
        public static IEnumerable<DropInfo> FilterByItemType(this IEnumerable<DropInfo> dropinfos, IEnumerable<eItemType> goodtypes)
        {
            return dropinfos.Where(di => goodtypes.Contains(di.AsItemRow().ItemType));
        }
        public static IEnumerable<DropInfo> FilterOutItemType(this IEnumerable<DropInfo> dropinfos, IEnumerable<eItemType> goodtypes)
        {
            return dropinfos.Where(di => !goodtypes.Contains(di.AsItemRow().ItemType));
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
        public static IEnumerable<Randomization> FilterByVanillaItem(this IEnumerable<Randomization> rdzs, int itemid)
        {
            return rdzs.Where(rdz => rdz.HasVanillaItemID(itemid));
        }
        public static IEnumerable<Randomization> FilterByTaskType(this IEnumerable<Randomization> rdzs, params RDZ_TASKTYPE[] allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.FilterByTaskType(allowtasks);
        }
        public static IEnumerable<Randomization> FilterByTaskType(this IEnumerable<Randomization> rdzs, IEnumerable<RDZ_TASKTYPE> allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type));
        }
        public static IEnumerable<Randomization> FilterOutTaskType(this IEnumerable<Randomization> rdzs, params RDZ_TASKTYPE[] badtasks)
        {
            return rdzs.Where(rdz => !badtasks.Contains(rdz.Type));
        }
        public static IEnumerable<Randomization> FilterOutPickupType(this IEnumerable<Randomization> rdzs, params PICKUPTYPE[] badtypes)
        {
            return rdzs.FilterOutPickupType(badtypes);
        }
        public static IEnumerable<Randomization> FilterOutPickupType(this IEnumerable<Randomization> rdzs, IEnumerable<PICKUPTYPE> badtypes)
        {
            // check the associated RandoInfo and filter out if it contains any of the badtypes
            return rdzs.Where(rdz => !rdz.RandoInfo.PickupTypes.Any(badtypes.Contains));
        }
        public static IEnumerable<Randomization> FilterByPickupType(this IEnumerable<Randomization> rdzs, params PICKUPTYPE[] allowtypes)
        {
            return rdzs.Where(rdz => rdz.RandoInfo.PickupTypes.Any(allowtypes.Contains));
        }
        public static IEnumerable<LotRdz> FilterByPickupType(this IEnumerable<LotRdz> rdzs, params PICKUPTYPE[] allowtypes)
        {
            return rdzs.Where(rdz => rdz.RandoInfo.PickupTypes.Any(allowtypes.Contains));
        }

        // Randomness helpers
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

        // Recasting helpers:
        public static string AsMetaName(this ITEMID itemid) => ((int)itemid).AsMetaName();
        public static string AsMetaName(this int itemid)
        {
            if (DS2Resource.ItemNames.TryGetValue(itemid, out var name))
                return name;
            return string.Empty;
        }
        public static ItemRow AsItemRow(this int itemid) => ParamMan.ItemRows.Where(ir => ir.ItemID == itemid).First();
        public static ItemRow? TryAsItemRow(this int itemid) => ParamMan.ItemRows.Where(ir => ir.ItemID == itemid).FirstOrDefault();
        public static ItemRow AsItemRow(this DropInfo di) => ParamMan.ItemRows.Where(ir => ir.ItemID == di.ItemID).First();
        public static IEnumerable<T> AsRows<T>(this Param? param)
        {
            if (param == null) throw new ArgumentNullException(nameof(param));
            return param.Rows.OfType<T>();
        }

        public static RandoInfo GetGlotRandoInfo(this int paramid) => CasualItemSet.GlotData[paramid];
        public static RandoInfo2 GetDropRandoInfo(this int paramid)
        {
            return CasualItemSet.DropData?[paramid] ?? throw new Exception("Not initialized");
        }
        public static RandoInfo GetShopRandoInfo(this int paramid) => CasualItemSet.ShopData[paramid];

        // More general methods:
        public static string[] RegexSplit(this string source, string pattern) => Regex.Split(source, pattern);
        public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> source)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            foreach (var element in source)
                target.Add(element);
        }
    }
}
