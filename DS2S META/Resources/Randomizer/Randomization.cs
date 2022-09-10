using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{

    /// <summary>
    /// Parent class combining Vanilla and Shuffled Data for Items/Shop subclasses
    /// </summary>
    internal abstract class Randomization
    {
        // Fields
        internal int ParamID;
        //ItemLot VanillaLot;
        //ItemLot ShuffledLot;
        //RandoInfo RandoInfo;
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

        internal ItemParam GetItem(int itemid) => RandomizerManager.VanillaItemParams[itemid];
        internal string GetItemName(int itemid) => GetItem(itemid).MetaItemName;
    }


    internal class LotRdz : Randomization
    {
        // Subclass fields:
        internal ItemLot VanillaLot;
        internal ItemLot ShuffledLot;

        // Constructors:
        internal LotRdz(int paramid) : base(paramid) { }
        internal LotRdz(KeyValuePair<int, ItemLot> VanKvp) : base(VanKvp.Key)
        {
            VanillaLot = VanKvp.Value;
        }

        // Methods
        internal override bool IsSaturated() => ShuffledLot.NumDrops == VanillaLot.NumDrops;
        internal override List<DropInfo> Flatlist => VanillaLot.Lot;
        internal override void AddShuffledItem(DropInfo item)
        {
            if (ShuffledLot == null)
            {
                ShuffledLot = new ItemLot(item);
                ShuffledLot.ParamDesc = VanillaLot.ParamDesc;
            }
            else
                ShuffledLot.AddDrop(item);

            if (ShuffledLot.NumDrops > VanillaLot.NumDrops)
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
            StringBuilder sb = new StringBuilder($"{ParamID}: {VanillaLot.ParamDesc}{Environment.NewLine}");
            
            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();    
            
            foreach(var di in ShuffledLot.Lot)
            {
                sb.Append($"\t{GetItemName(di.ItemID)}");
                if (di.Quantity > 0)
                    sb.Append($" x{di.Quantity}");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r','\n');
        }

        internal override string printdata()
        {
            throw new NotImplementedException();
        }

    }


    internal class ShopRdz : Randomization
    {
        // Subclass fields:
        internal ShopInfo VanillaShop;
        internal ShopInfo ShuffledShop;

        // Constructors:
        internal ShopRdz(int paramid) : base(paramid) { }
        internal ShopRdz(KeyValuePair<int, ShopInfo> VanKvp) : base(VanKvp.Key)
        {
            VanillaShop = VanKvp.Value;
        }

        // Methods:
        internal override bool IsSaturated() => ShuffledShop == null;
        internal override string printdata()
        {
            int itemid = VanillaShop.ItemID;
            return $"{ParamID} [{"<insert_name>"}]: {itemid} ({GetItemName(itemid)})";
        }
        internal override List<DropInfo> Flatlist => new List<DropInfo>() { VanillaShop.ConvertToDropInfo() };
        internal override void AddShuffledItem(DropInfo item)
        {
            int basepricenew = RandomizerManager.RandomGammaInt(3000);
            ShuffledShop = new ShopInfo(item, VanillaShop, 1.00f, basepricenew);
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
            return VanillaShop.ItemID == itemID;
        }
        internal override int GetShuffledItemQuant(int itemID)
        {
            if (ShuffledShop == null)
                return -1;
            return ShuffledShop.AdjQuantity;
        }
        internal override string GetNeatDescription()
        {
            StringBuilder sb = new StringBuilder($"{ParamID}: {VanillaShop.ParamDesc}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledShop == null || ShuffledShop.ItemID == 0)
                return sb.Append("\tEMPTY").ToString();

            return sb.Append($"\t{GetItemName(ShuffledShop.ItemID)}").ToString();
        }
    }
}
