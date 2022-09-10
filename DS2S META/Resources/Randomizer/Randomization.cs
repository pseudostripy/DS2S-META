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
                ShuffledLot = new ItemLot(item);
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
    }
}
