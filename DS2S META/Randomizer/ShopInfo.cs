using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS2S_META.Utils;

using DS2S_META;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// This class is not strictly required, but it provides useful
    /// shorthand checks for many of the variables used in the randomizer.
    /// </summary>
    internal class ShopInfo
    {
        // Fields:
        internal Param.Row ParamRow; // Raw DS2 memory
        internal int ID => ParamRow.ID; // shorthand

        //internal string? ParamDesc => Data.Name;
        internal string ParamDesc { get; set; }
        internal int ItemID { get; set; }
        internal int EnableFlag { get; set; }
        internal int DisableFlag { get; set; }
        internal int MaterialID { get; set; }
        internal int DuplicateItemID { get; set; }
        internal float PriceRate { get; set; }
        internal int Quantity { get; set; }

        // Do we need this?
        internal int NewBasePrice { get; set; }
        
        internal ItemParam ItemParam => RandomizerManager.VanillaItemParams[ItemID];
        internal int VanillaBasePrice
        {
            get
            {
                if (!RandomizerManager.TryGetItem(ItemID, out var item))
                    return -1;
                if (item == null)
                    return -1;
                return item.BaseBuyPrice;
            }
        }

        // Constructors:
        internal ShopInfo() { }
        internal ShopInfo(Param.Row shoprow)
        {
            // Unpack data:
            ParamRow = shoprow;

            ParamDesc = UnpackName();
            ItemID = UnpackItemID();
            EnableFlag = UnpackEnable();
            DisableFlag = UnpackDisable();
            MaterialID = UnpackMaterialID();
            DuplicateItemID = UnpackDuplicateID();
            PriceRate = UnpackPriceRate();
            Quantity = UnpackQuantity();
        }

        // Think of a way to not hardcode this?
        private string UnpackName() => ParamRow.Desc;
        private int UnpackItemID() => (int)ParamRow.Data[0];
        private int UnpackEnable() => (int)ParamRow.Data[2];
        private int UnpackDisable() => (int)ParamRow.Data[3];
        private int UnpackMaterialID() => (int)ParamRow.Data[4];
        private int UnpackDuplicateID() => (int)ParamRow.Data[5];
        private float UnpackPriceRate() => (float)ParamRow.Data[7];
        private int UnpackQuantity() => (int)ParamRow.Data[8];

        internal ShopInfo(DropInfo DI, ShopInfo VanShop, float pricerate, int newbaseprice)
        {
            ParamRow = VanShop.ParamRow;
            
            // Used to construct things from various information sources:
            ItemID          = DI.ItemID;
            Quantity        = DI.Quantity;
            //
            EnableFlag      = VanShop.EnableFlag;
            DisableFlag     = VanShop.DisableFlag;
            MaterialID      = VanShop.MaterialID;
            DuplicateItemID = VanShop.DuplicateItemID;
            ParamDesc       = VanShop.ParamDesc;
            //
            PriceRate = pricerate;
            //
            NewBasePrice = newbaseprice;
        }
        internal ShopInfo Clone()
        {
            return (ShopInfo)MemberwiseClone();
        }

        // Methods:
        internal List<DropInfo> ConvertToDropInfo()
        {
            // Assume no infusion or reinforcement, to consider later.
            return new List<DropInfo>() { new DropInfo(ItemID, Quantity, 0, 0) };
        }
        internal void SetBytesOutput()
        {
            // Prepare for write

            var fourbytes = new byte[4];

            var itemid = BitConverter.GetBytes(ItemID);
            var enable = BitConverter.GetBytes(EnableFlag);
            var disable = BitConverter.GetBytes(DisableFlag);
            var material = BitConverter.GetBytes(MaterialID);
            var dup = BitConverter.GetBytes(DuplicateItemID);
            var rate = BitConverter.GetBytes(PriceRate);
            var quant = BitConverter.GetBytes(Quantity);

            var test = Concat(itemid, fourbytes, enable, disable, material,
                               dup, fourbytes, rate, quant);
            ParamRow.RowBytes = test;
        }
        public static byte[] Concat(params byte[][] arrays)
        {
            return arrays.SelectMany(x => x).ToArray();
        }
    }
}
