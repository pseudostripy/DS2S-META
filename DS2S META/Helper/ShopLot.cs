using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    internal class ShopLot
    {
        ////Fields:
        //internal List<ShopInfo> Lot = new List<ShopInfo>();
        
        //// Constructor:
        //internal ShopLot()
        //{



        //}

        //public override string ToString()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < NumDrops; i++)
        //    {
        //        sb.Append($"Item[{i}] x{Quantities[i]}: {Items[i]:X} / {Items[i]}\n");
        //    }
        //    return sb.ToString().TrimEnd('\n');
        //}

        
        //    // Constructors:
        //    internal ItemLot() { }
        //    internal ItemLot(DropInfo dropInfo)
        //    {
        //        Lot.Add(dropInfo);
        //    }

        //    // Utility:
        //    internal void AddDrop(int itemID, int quantity, int reinforce, int infusion)
        //    {
        //        AddDrop(new DropInfo(itemID, (byte)quantity, (byte) reinforce, (byte) infusion));
        //    }
        //    internal void AddDrop(DropInfo data)
        //    {
        //        Lot.Add(data);
        //    }

        //}

    }

    internal class ShopInfo
    {
        // Fields:
        internal int ItemID { get; set; }
        internal int EnableFlag { get; set; }
        internal int DisableFlag { get; set; }
        internal int MaterialID { get; set; }
        internal int DuplicateItemID { get; set; }
        internal float PriceRate { get; set; }
        internal int RawQuantity { get; set; }
        internal int BasePrice { get; set; } // Note: comes from ItemParam table
        internal int Quantity { get; set; } // adjusted for inf shop sells

        // Constructors:
        internal ShopInfo(int itemID, int en, int dis, int mat, int dup, float rate, int quant, int baseprice)
        {
            ItemID = itemID;
            EnableFlag = en;
            DisableFlag = dis;
            MaterialID = mat;
            DuplicateItemID = dup;
            PriceRate = rate;
            RawQuantity = quant;
            BasePrice = baseprice;
        }
        internal ShopInfo(int itemID, int en, int dis, int mat, int dup, float rate, int quant)
        {
            ItemID = itemID;
            EnableFlag = en;
            DisableFlag = dis;
            MaterialID = mat;
            DuplicateItemID = dup;
            PriceRate = rate;
            Quantity = quant;
        }
    }
}
