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
        public static List<DropInfo> FilterByItem(this List<DropInfo> dropinfos, params ITEMID[] items)
        {
            return dropinfos.Where(di => items.Cast<int>().Contains(di.ItemID)).ToList();
        }
        public static List<DropInfo> FilterByItem(this List<DropInfo> dropinfos, params int[] itemids)
        {
            return dropinfos.Where(di => itemids.Contains(di.ItemID)).ToList();
        }
        public static List<DropInfo> FilterByItemType(this List<DropInfo> dropinfos, params eItemType[] goodtypes)
        {
            return dropinfos.FilterByItemType(goodtypes);
        }
        public static List<DropInfo> FilterByItemType(this List<DropInfo> dropinfos, List<eItemType> goodtypes)
        {
            return dropinfos.Where(di => goodtypes.Contains(di.AsItemRow().ItemType)).ToList();
        }
        public static List<DropInfo> FilterOutItemType(this List<DropInfo> dropinfos, List<eItemType> goodtypes)
        {
            return dropinfos.Where(di => !goodtypes.Contains(di.AsItemRow().ItemType)).ToList();
        }
        public static List<ItemRow> FilterByType(this List<ItemRow> itemrows, params eItemType[] types)
        {
            return itemrows.Where(it => types.Contains(it.ItemType)).ToList();
        }
        public static List<ItemRow> FilterOutId(this List<ItemRow> itemrows, List<ITEMID> badids)
        {
            return itemrows.Where(it => !badids.Cast<int>().Contains(it.ItemID)).ToList();
        }
        public static List<ItemRow> FilterOutUsage(this List<ItemRow> itemrows, params ITEMUSAGE[] baduses)
        {
            return itemrows.Where(it => !baduses.Cast<int>().Contains(it.ItemUsageID)).ToList();
        }
        public static List<Randomization> FilterByVanillaItem(this List<Randomization> rdzs, int itemid)
        {
            return rdzs.Where(rdz => rdz.HasVanillaItemID(itemid)).ToList();
        }
        public static List<Randomization> FilterByTaskType(this List<Randomization> rdzs, params RDZ_TASKTYPE[] allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<ShopRdz> FilterByTaskType(this List<ShopRdz> rdzs, params RDZ_TASKTYPE[] allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<ShopRdz> FilterByTaskType(this List<ShopRdz> rdzs, List<RDZ_TASKTYPE> allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<LotRdz> FilterByTaskType(this List<LotRdz> rdzs, params RDZ_TASKTYPE[] allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<LotRdz> FilterByTaskType(this List<LotRdz> rdzs, List<RDZ_TASKTYPE> allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<Randomization> FilterByTaskType(this List<Randomization> rdzs, List<RDZ_TASKTYPE> allowtasks)
        {
            // return those that match any of the allowtasks types
            return rdzs.Where(rdz => allowtasks.Contains(rdz.Type)).ToList();
        }
        public static List<Randomization> FilterOutTaskType(this List<Randomization> rdzs, params RDZ_TASKTYPE[] badtasks)
        {
            return rdzs.Where(rdz => !badtasks.Contains(rdz.Type)).ToList();
        }
        public static List<Randomization> FilterOutPickupType(this List<Randomization> rdzs, params PICKUPTYPE[] badtypes)
        {
            return rdzs.FilterOutPickupType(badtypes);
        }
        public static List<Randomization> FilterOutPickupType(this List<Randomization> rdzs, List<PICKUPTYPE> badtypes)
        {
            // check the associated RandoInfo and filter out if it contains any of the badtypes
            return rdzs.Where(rdz => !rdz.RandoInfo.PickupTypes.Any(badtypes.Contains)).ToList();
        }
        public static List<Randomization> FilterByPickupType(this List<Randomization> rdzs, params PICKUPTYPE[] allowtypes)
        {
            return rdzs.Where(rdz => rdz.RandoInfo.PickupTypes.Any(allowtypes.Contains)).ToList();
        }
        public static List<Randomization> FilterByPickupType(this List<Randomization> rdzs, List<PICKUPTYPE> allowtypes)
        {
            return rdzs.Where(rdz => rdz.RandoInfo.PickupTypes.Any(allowtypes.Contains)).ToList();
        }
        public static List<Restriction> FilterByType(this List<Restriction> restrs, RestrType allowtype)
        {
            return restrs.Where(r => r.Type == allowtype).ToList();
        }
        public static List<LotRdz> FilterByPickupType(this List<LotRdz> rdzs, params PICKUPTYPE[] allowtypes)
        {
            return rdzs.Where(rdz => rdz.RandoInfo.PickupTypes.Any(allowtypes.Contains)).ToList();
        }

        // Randomness helpers
        public static T RandomElement<T>(this List<T> pool) => pool.ElementAt(Rng.Next(pool.Count()));
        public static List<T> RandomElements<T>(this List<T> pool, int count)
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
        public static ItemRow AsItemRow(this int itemid) => ParamMan.ItemRows[itemid];
        public static ItemRow? TryAsItemRow(this int itemid)
        {
            ParamMan.ItemRows.TryGetValue(itemid, out var item);
            return item;
        }
        public static ItemRow AsItemRow(this DropInfo di) => ParamMan.ItemRows[di.ItemID];
        public static IList<T> AsRows<T>(this Param? param)
        {
            if (param == null) throw new ArgumentNullException(nameof(param));
            return param.Rows.OfType<T>().ToList();
        }

        public static RandoInfo GetGlotRandoInfo(this int paramid) => CasualItemSet.LotData[paramid];
        public static RandoInfo GetDropRandoInfo(this int paramid)
        {
            return CasualItemSet.DropData?[paramid] ?? throw new Exception("Not initialized");
        }
        public static RandoInfo GetShopRandoInfo(this int paramid) => CasualItemSet.ShopData[paramid];

        // More general methods:
        public static string[] RegexSplit(this string source, string pattern) => Regex.Split(source, pattern);
        public static void AddRange<T>(this ICollection<T> target, IList<T> source)
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
