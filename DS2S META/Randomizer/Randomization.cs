using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils;

namespace DS2S_META.Randomizer
{

    internal enum RDZ_STATUS
    {
        // Enum used to denote required further post-processing situations
        EXCLUDED,
        SHOPREMOVE,
        STANDARD,
        CROWS,
        COMPLETE,
        FILL_BY_COPY,
        UNLOCKTRADE, // enable immediately
        MAKEFREE,
        TRADE_SHOP_COPY,
        INITIALIZING,
    }

    /// <summary>
    /// Parent class combining Vanilla and Shuffled Data for Items/Shop subclasses
    /// </summary>
    internal abstract class Randomization
    {
        // Fields
        internal int ParamID;
        internal abstract List<DropInfo> Flatlist { get; }
        internal RDZ_STATUS Status = RDZ_STATUS.INITIALIZING;
        internal bool IsHandled = false;
        internal string RandoDesc = string.Empty;
        internal string GUID;
        internal RandoInfo? RandoInfo;

        // Constructors:
        internal Randomization(int pid)
        {
            GUID = Guid.NewGuid().ToString();
            ParamID = pid;
        }

        // Abstract Methods:
        internal abstract string printdata();
        internal abstract bool IsSaturated();
        internal abstract void AddShuffledItem(DropInfo item);
        internal abstract bool HasShuffledItemID(int itemID);
        internal abstract bool HasVannilaItemID(int itemID);
        internal abstract int GetShuffledItemQuant(int itemID);
        internal abstract string GetNeatDescription();
        internal abstract void AdjustQuantity(DropInfo di);
        internal abstract void ResetShuffled();
        internal abstract int UniqueParamID { get; }
        
        // Common Methods:

        // Wrappers to RandoInfo
        internal bool HasType(List<PICKUPTYPE> bannedtypes)
        {
            //if (RandoInfo == null) throw new Exception("Shouldn't get here without Logic being set"); // TODO?
            if (RandoInfo == null) return false; // TODO?
            return RandoInfo.HasType(bannedtypes);
        }
        internal bool ContainsOnlyTypes(List<PICKUPTYPE> onlytpes)
        {
            if (RandoInfo == null) return false; // TODO?
            return RandoInfo.ContainsOnlyTypes(onlytpes);
        }
        internal bool IsSoftlockPlacement(List<int> placedSoFar)
        {
            if (RandoInfo == null) throw new Exception("Shouldn't get here without Logic being set");
            return RandoInfo.IsSoftlockPlacement(placedSoFar);
        }
        protected static int RoundUpNearestMultiple(int val, int m)
        {
            return (int)Math.Ceiling((double)val / m) * m;
        }
        protected static int GetTypeRandomPrice(int itemid)
        {
            if (!RandomizerManager.TryGetItem(itemid, out var item) || item == null)
                return RandomizerManager.RandomGammaInt(3000, 50); // generic guess

            return item.ItemType switch
            {
                eItemType.AMMO => RandomizerManager.RandomGammaInt(100, 10),
                eItemType.CONSUMABLE => GetConsumableRandomPrice(item.ItemID),
                eItemType.WEAPON1 or eItemType.WEAPON2 => RandomizerManager.RandomGammaInt(5000, 100),
                _ => RandomizerManager.RandomGammaInt(3000, 50),
            };
        }
        protected static int GetConsumableRandomPrice(int itemid)
        {
            // Add more rules here as appropriate:
            if (ItemSetBase.SoulPriceList.ContainsKey(itemid))
            {
                var souls = ItemSetBase.SoulPriceList[itemid];
                var ranval = RandomizerManager.RandomGammaInt(souls, 50);
                return (int)Math.Max(ranval, 0.9 * souls); // Limit to 10% off best sale
            }

            var lowtier = new List<int>() { 60010000, 60040000, 60595000, 60070000 }; // lifegem, amber herb, dung pie, poison moss
            if (lowtier.Contains(itemid))
                return RandomizerManager.RandomGammaInt(400, 50);

            // Otherwise:
            return RandomizerManager.RandomGammaInt(2000, 50);
        }
        internal static string GetItemName(int itemid)
        {
            if (!RandomizerManager.TryGetItem(itemid, out var item))
                return string.Empty;
            if (item == null)
                return string.Empty;

            return item.MetaItemName;
        }
        internal void MarkHandled()
        {
            IsHandled = true;
        }
    }

    // GeneralizedLot
    internal abstract class GLotRdz : Randomization
    {
        // Subclass fields:
        internal ItemLotRow VanillaLot;
        internal ItemLotRow ShuffledLot;
        internal bool IsDropTable = false; // by default

        // Constructors:
        internal GLotRdz(ItemLotRow vanlot) : base(vanlot.ID)
        {
            VanillaLot = vanlot;
            ShuffledLot = VanillaLot.CloneBlank();
        }

        // Methods
        internal override bool IsSaturated() => ShuffledLot != null && (ShuffledLot.NumDrops == VanillaLot.NumDrops);
        internal override List<DropInfo> Flatlist
        {
            get
            {
                var flatlist = VanillaLot.GetFlatlist();
                return flatlist;
            }
        }
        internal override void AddShuffledItem(DropInfo di)
        {
            // Fix quantity:
            AdjustQuantity(di);

            if (ShuffledLot == null)
                throw new NullReferenceException("Shuffled lot should have been cloned from vanilla!");

            ShuffledLot.AddDrop(di);

            if (ShuffledLot.NumDrops > VanillaLot?.NumDrops)
                throw new Exception("Shouldn't be able to get here!");
        }
        internal override bool HasShuffledItemID(int itemID)
        {
            if (ShuffledLot == null)
                return false;
            return ShuffledLot.Items.Contains(itemID);
        }
        internal override bool HasVannilaItemID(int itemID)
        {
            if (VanillaLot == null)
                return false;
            return VanillaLot.Items.Contains(itemID);
        }
        internal override int GetShuffledItemQuant(int itemID)
        {
            if (ShuffledLot == null)
                return -1;
            int ind = ShuffledLot.GetLotIndex(itemID);
            return ShuffledLot.Quantities[ind];
            // Note: there's an extremely unlikely bug that can occur here and only affects
            // output display, so I'm too lazy to deal with it.
        }
        internal override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {VanillaLot?.ParamDesc}{Environment.NewLine}");
            
            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();    
            
            for(int i = 0; i < ShuffledLot.NumDrops; i++)
            {
                sb.Append($"\t{GetItemName(ShuffledLot.Items[i])}");
                sb.Append($" x{ShuffledLot.Quantities[i]}");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r','\n');
        }
        internal override void AdjustQuantity(DropInfo di)
        {
            if (!RandomizerManager.TryGetItem(di.ItemID, out var item))
                return;
            if (item == null)
                return;

            var itype = item.ItemType;
            switch (itype)
            {
                case eItemType.AMMO:
                    if (di.Quantity == 255)
                        di.Quantity = 50; // reset to reasonable value

                    // Otherwise round to nearest 10 ceiling
                    di.Quantity = (byte)RoundUpNearestMultiple(di.Quantity, 10);
                    return;

                case eItemType.CONSUMABLE:
                    if (di.Quantity == 255)
                        di.Quantity = 5;
                    return;

                default:
                    // Everything else:
                    di.Quantity = 1;
                    return;
            }
        }
        internal override string printdata()
        {
            throw new NotImplementedException();
        }
        internal override void ResetShuffled()
        {
            ShuffledLot = VanillaLot.CloneBlank();
            IsHandled = false;
        }
        internal override int UniqueParamID => IsDropTable ? ParamID + 80000000 : ParamID;


        internal List<DropInfo> GetUniqueFlatlist(List<DropInfo> avoid_these)
        {
            // Return a flat list of drops that do not overlap with the supplied ones.
            // This is a way to remove the NGPlus duplicates which are unchanged.
            List<DropInfo> res = new();
            foreach (var di in Flatlist)
            {
                if (avoid_these.Any(di2 => di2.IsEqualTo(di)))
                    continue;
                res.Add(di);
            }
            return res;
        }

        // Extra Utility:
        internal bool IsEmpty => VanillaLot.IsEmpty; // Vanilla Lot has 0 drops
    }
    internal class LotRdz : GLotRdz
    {
        // Constructors:
        internal LotRdz(ItemLotRow vanlot) : base(vanlot)
        {
            IsDropTable = false;
        }
    }
    internal class DropRdz : GLotRdz
    {
        // Constructors:
        internal DropRdz(ItemLotRow vanlot) : base(vanlot)
        {
            IsDropTable = true;
        }

        internal override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {RandoDesc}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();

            for (int i = 0; i < ShuffledLot.NumDrops; i++)
            {
                sb.Append($"\t{GetItemName(ShuffledLot.Items[i])}");
                sb.Append($" x{ShuffledLot.Quantities[i]}");
                sb.Append($" ({ShuffledLot.Chances[i]}%)");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r', '\n');
        }
        internal bool IsGuaranteedDrop 
        { 
            get
            {
                if (VanillaLot == null)
                    return false;
                for (int i = 0; i < VanillaLot.NumDrops; i++)
                {
                    if (VanillaLot.Chances[i] != 100F)
                        return false;
                }
                return true;
            } 
        }
    }


    internal class ShopRdz : Randomization
    {
        // Subclass fields:
        internal ShopRow VanillaShop;
        internal ShopRow ShuffledShop;

        // Constructors:
        internal ShopRdz(ShopRow vanshop) : base(vanshop.ID)
        {
            VanillaShop = vanshop;
            ShuffledShop = VanillaShop.Clone();
        }

        // Methods:
        internal override bool IsSaturated() => ShuffledShop != null;
        internal override string printdata()
        {
            if (VanillaShop == null)
                return "BLANK";
            int itemid = VanillaShop.ItemID;
            return $"{ParamID} [{"<insert_name>"}]: {itemid} ({GetItemName(itemid)})";
        }
        internal override List<DropInfo> Flatlist
        {
            get
            {
                var flatlist = VanillaShop.ConvertToDropInfo();
                return flatlist ?? new();
            }
        }
        internal override void AddShuffledItem(DropInfo di)
        {
            // Fix quantity:
            AdjustQuantity(di);

            // Fix price:
            if (!RandomizerManager.TryGetItem(di.ItemID, out var item))
                throw new Exception("I'm curious whether any of them are not defined?");
            if (item == null)
                throw new NullReferenceException("Item shouldn't be null");

            // Fix "unsellable items"
            var baseprice = item.BaseBuyPrice;
            if (baseprice <= 1)
            {
                item.BaseBuyPrice = 12000;
                item.StoreRow();
            }
                

            int pricenew = GetTypeRandomPrice(di.ItemID);
            float pricerate = (float)pricenew / item.BaseBuyPrice;

            // Update:
            ShuffledShop.SetValues(di, VanillaShop, pricerate);
        }
        internal override bool HasShuffledItemID(int itemID)
        {
            if (ShuffledShop == null)
                return false;
            return ShuffledShop.ItemID == itemID;
        }
        internal override bool HasVannilaItemID(int itemID)
        {
            if (ShuffledShop == null)
                return false;
            return VanillaShop?.ItemID == itemID;
        }
        internal override int GetShuffledItemQuant(int itemID)
        {
            if (ShuffledShop == null)
                return -1;
            return ShuffledShop.Quantity;
        }
        internal override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {VanillaShop?.ParamDesc}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledShop == null || ShuffledShop.ItemID == 0)
                return sb.Append("\tEMPTY").ToString();

            return sb.Append($"\t{GetItemName(ShuffledShop.ItemID)} x{ShuffledShop.Quantity}").ToString();
        }
        internal override void AdjustQuantity(DropInfo di)
        {
            if (!RandomizerManager.TryGetItem(di.ItemID, out var item))
                return;
            if (item == null)
                return;

            var itype = item.ItemType;
            switch (itype)
            {
                case eItemType.AMMO:
                    if (di.Quantity == 255)
                        di.Quantity = 50; // reset to reasonable value

                    // Otherwise round to nearest 10 ceiling
                    di.Quantity = (byte)RoundUpNearestMultiple(di.Quantity, 10);
                    return;

                case eItemType.CONSUMABLE:
                    if (di.Quantity == 255)
                        di.Quantity = 15;
                    return;

                default:
                    // Everything else set to one for now. Still deciding on this:
                    di.Quantity = 1;
                    return;
            }
        }
        internal override void ResetShuffled()
        {
            ShuffledShop = VanillaShop.Clone();
            IsHandled = false;
        }
        internal void ZeroiseShuffledShop()
        {
            // Sets things so that the shop is removed from game
            ShuffledShop.ItemID = 0;
            ShuffledShop.Quantity = 0;
        }
        internal override int UniqueParamID => ParamID;
    }
}
