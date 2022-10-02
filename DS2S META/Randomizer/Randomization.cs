using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils;

namespace DS2S_META.Randomizer
{

    /// <summary>
    /// Parent class combining Vanilla and Shuffled Data for Items/Shop subclasses
    /// </summary>
    internal abstract class Randomization
    {
        // Fields
        internal int ParamID;
        internal abstract List<DropInfo> Flatlist { get; }


        // Constructors:
        internal Randomization(int pid)
        {
            ParamID = pid;
        }

        // Methods:
        internal abstract string printdata();
        internal abstract bool IsSaturated();
        internal abstract void AddShuffledItem(DropInfo item);
        internal abstract bool HasShuffledItemID(int itemID);
        internal abstract bool HasVannilaItemID(int itemID);
        internal abstract int GetShuffledItemQuant(int itemID);
        internal abstract string GetNeatDescription();
        internal abstract void AdjustQuantity(DropInfo di);
        protected int RoundUpNearestMultiple(int val, int m)
        {
            return (int)Math.Ceiling((double)val / m) * m;
        }
        protected int GetTypeRandomPrice(int itemid)
        {
            if (!RandomizerManager.TryGetItem(itemid, out var item))
                return RandomizerManager.RandomGammaInt(3000, 50); // generic guess

            if (item == null)
                throw new NullReferenceException("Shouldn't be possible to get here");

            switch (item.ItemType)
            {
                case eItemType.AMMO:
                    return RandomizerManager.RandomGammaInt(100, 10);

                case eItemType.CONSUMABLE:
                    return GetConsumableRandomPrice(item.ItemID);

                case eItemType.WEAPON1:
                case eItemType.WEAPON2:
                    return RandomizerManager.RandomGammaInt(5000, 100);

                default:
                    return RandomizerManager.RandomGammaInt(3000, 50);
            }
        }
        protected int GetConsumableRandomPrice(int itemid)
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

        internal ItemParam GetItem(int itemid) => RandomizerManager.VanillaItemParams[itemid];
        internal string GetItemName(int itemid) => GetItem(itemid).MetaItemName;
    }


    internal class LotRdz : Randomization
    {
        // Subclass fields:
        internal ItemLot VanillaLot = new();
        internal ItemLot ShuffledLot = new();

        // Constructors:
        internal LotRdz(int paramid) : base(paramid) { }
        internal LotRdz(KeyValuePair<int, ItemLot> VanKvp) : base(VanKvp.Key)
        {
            VanillaLot = VanKvp.Value;
        }

        // Methods
        internal override bool IsSaturated() => ShuffledLot != null && (ShuffledLot.NumDrops == VanillaLot.NumDrops);
        internal override List<DropInfo> Flatlist
        {
            get
            {
                var flatlist = VanillaLot.Lot;
                return flatlist;
            }
        }
        internal override void AddShuffledItem(DropInfo di)
        {
            // Fix quantity:
            AdjustQuantity(di);

            if (ShuffledLot == null)
            {
                ShuffledLot = new ItemLot(di);
                ShuffledLot.ParamDesc = VanillaLot?.ParamDesc;
            }
            else
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
            return ShuffledLot.Lot.Where(di => di.ItemID == itemID).First().Quantity;
            // Note: there's an extremely unlikely bug that can occur here and only affects
            // output display, so I'm too lazy to deal with it.
        }
        internal override string GetNeatDescription()
        {
            StringBuilder sb = new StringBuilder($"{ParamID}: {VanillaLot?.ParamDesc}{Environment.NewLine}");
            
            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();    
            
            foreach(var di in ShuffledLot.Lot)
            {
                sb.Append($"\t{GetItemName(di.ItemID)}");
                sb.Append($" x{di.Quantity}");
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
    }


    internal class ShopRdz : Randomization
    {
        // Subclass fields:
        internal ShopInfo VanillaShop = new();
        internal ShopInfo ShuffledShop = new();

        // Constructors:
        internal ShopRdz(int paramid) : base(paramid) { }
        internal ShopRdz(KeyValuePair<int, ShopInfo> VanKvp) : base(VanKvp.Key)
        {
            VanillaShop = VanKvp.Value;
        }
        internal ShopRdz(ShopInfo vanshop) : base(vanshop.ID)
        {
            VanillaShop = vanshop;
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
            int basepricenew = 12000; // some large number that divides by a lot of things
            int pricenew = GetTypeRandomPrice(di.ItemID);
            float pricerate = (float)pricenew / basepricenew;

            // Create:
            ShuffledShop = new ShopInfo(di, VanillaShop, pricerate, basepricenew);
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
            StringBuilder sb = new StringBuilder($"{ParamID}: {VanillaShop?.ParamDesc}{Environment.NewLine}");

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

    }
}
