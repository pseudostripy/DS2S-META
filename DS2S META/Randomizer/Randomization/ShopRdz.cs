using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class ShopRdz : Randomization
    {
        // Subclass fields:
        internal ShopRow VanillaShop;
        internal ShopRow ShuffledShop;

        // Constructors:
        internal ShopRdz(ShopRow vanshop, RandoInfo ri, RDZ_TASKTYPE status) : base(vanshop.ID, ri, status)
        {
            VanillaShop = vanshop;
            ShuffledShop = VanillaShop.CloneBlank();
        }

        // Methods:
        internal override bool IsSaturated() => ShuffledShop != null && ShuffledShop.ItemID != 0;
        internal override string PrintData()
        {
            if (VanillaShop == null)
                return "BLANK";
            int itemid = VanillaShop.ItemID;
            return $"{ParamID} [{"<insert_name>"}]: {itemid} ({itemid.AsMetaName()})";
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
            var item = di.AsItemRow();

            // Fix "unsellable items"
            var baseprice = item.BaseBuyPrice;
            if (baseprice <= 1)
            {
                item.BaseBuyPrice = 12000;
                item.StoreRow();
            }


            int pricenew = GetTypeRandomPrice(di.ItemID);
            float pricerate = (float)pricenew / item.BaseBuyPrice;

            // "Supermarket price" fix - small enough to not increase the price unintenionally, big enough to cause the price to be fixed
            pricerate += 1e-6f;
            // This abomination would do the job more precisely, but would require compiling with /unsafe: unsafe { ++*(int*)&pricerate; }

            // Update:
            ShuffledShop.SetValues(di, VanillaShop, pricerate);
        }
        internal override bool HasShuffledItemId(int itemID)
        {
            if (ShuffledShop == null)
                return false;
            return ShuffledShop.ItemID == itemID;
        }
        internal override bool HasVanillaItemID(int itemID)
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

            return sb.Append($"\t{ShuffledShop.ItemID.AsMetaName()} x{ShuffledShop.Quantity}").ToString();
        }
        internal override void AdjustQuantity(DropInfo di) => AdjustQuantityParameterized(di, 15);
        internal override void ResetShuffled()
        {
            ShuffledShop = VanillaShop.CloneBlank();
            PlaceDist = -1;
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
