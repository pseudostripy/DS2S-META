using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class ItemLot
    {
        // Fields:
        internal string ParamDesc;
        internal List<DropInfo> Lot = new List<DropInfo>();
        internal List<int> Items => Lot.Select(L => L.ItemID).ToList();
        internal List<byte> Quantities => Lot.Select(L => L.Quantity).ToList();
        internal int NumDrops => Lot.Count();

        private const int SINGLE = 1;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumDrops; i++)
            {
                sb.Append($"Item[{i}] x{Quantities[i]}: {Items[i]:X} / {Items[i]}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }

        // Constructors:
        internal ItemLot() { }
        internal ItemLot(DropInfo dropInfo)
        {
            Lot.Add(dropInfo);
        }
        internal ItemLot(List<DropInfo> lots)
        {
            Lot = new List<DropInfo>(lots);
        }

        // Methods:
        internal ItemLot Clone()
        {
            // Performs a deep clone on the Lot object
            var ilclone = new ItemLot();
            foreach (var di in Lot)
            {
                ilclone.AddDrop(di.Clone());
            }
            return ilclone;
        }
        internal void Zeroise()
        {
            // Careful using this method!
            foreach (var di in Lot)
            {
                di.ItemID = 60510000; // rubbish
                di.Quantity = 0;
            }
        }

        // Utility:
        internal void AddDrop(int itemID, int quantity, int reinforce, int infusion)
        {
            AddDrop(new DropInfo(itemID, (byte)quantity, (byte) reinforce, (byte) infusion));
        }
        internal void AddDrop(DropInfo data)
        {
            Lot.Add(data);
        }

    }

    internal class DropInfo
    {
        // Fields:
        internal int ItemID { get; set; }
        internal byte Quantity { get; set; }
        internal byte Infusion { get; set; }
        internal byte Reinforcement { get; set; }

        // Constructors:
        internal DropInfo() { }
        internal DropInfo(int itemID, byte quantity, byte reinforce, byte infusion)
        {
            ItemID = itemID;
            Quantity = quantity;
            Reinforcement = reinforce;
            Infusion = infusion;
        }
        internal DropInfo(int itemID, int quantity, int reinforce, int infusion)
        {
            ItemID = itemID;
            Quantity = (byte)quantity;
            Reinforcement = (byte)reinforce;
            Infusion = (byte)infusion;
        }
        internal DropInfo Clone()
        {
            return (DropInfo)MemberwiseClone();
        }

        // Properties:
        internal bool IsKeyType => Enum.IsDefined(typeof(KEYID), ItemID);
    }
}
