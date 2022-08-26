using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    internal class ItemLot
    {
        // Fields:
        internal List<DropInfo> Lot = new List<DropInfo>();
        internal List<int> Items => Lot.Select(L => L.ItemID).ToList();
        internal List<int> Quantities => Lot.Select(L => L.ItemID).ToList();

        private const int SINGLE = 1;

        // Constructors:
        internal ItemLot() { }
        internal ItemLot(IEnumerable<int> itemIDs)
        {
            // Assumes that all these items are only 1 quantity.
            
            Lot = itemIDs.Select(id => new DropInfo(id, SINGLE)).ToList();
        }
        internal ItemLot(IEnumerable<int> itemIDs, IEnumerable<int> quantities)
        {
            if (itemIDs.Count() != quantities.Count())
                throw new ArgumentException("Mismatch between number of items and number of quantity parameters provided");

            Lot = itemIDs.Zip(quantities, (id, q) => new DropInfo(id, (byte)q)).ToList();
        }

        // Methods:
        internal void AddDrop(int itemID)
        {
            AddDrop(itemID, SINGLE);
        }
        internal void AddDrop(int itemID, int quantity)
        {
            Lot.Add(new DropInfo(itemID, (byte)quantity));
        }

    }

    internal class DropInfo
    {
        // Fields:
        internal int ItemID { get; set; }
        internal byte Quantity { get; set; }


        // Constructors:
        internal DropInfo() { }
        internal DropInfo(int itemID, byte quantity)
        {
            ItemID = itemID;
            Quantity = quantity;
        }
    }
}
