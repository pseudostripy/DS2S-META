using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public class DropInfo
    {
        // Fields:
        public int ItemID { get; set; }
        public byte Quantity { get; set; }
        public byte Infusion { get; set; }
        public byte Reinforcement { get; set; }
        public bool IsPlaced { get; private set; } = false;

        // Constructors:
        public DropInfo() { }
        public DropInfo(int itemID)
        {
            ItemID = itemID;
            Quantity = 1;
            Reinforcement = 0;
            Infusion = 0;
        }
        public DropInfo(int itemID, int quantity)
        {
            ItemID = itemID;
            Quantity = (byte)quantity;
            Reinforcement = 0;
            Infusion = 0;
        }
        public DropInfo(int itemID, byte quantity, byte reinforce, byte infusion)
        {
            ItemID = itemID;
            Quantity = quantity;
            Reinforcement = reinforce;
            Infusion = infusion;
        }
        public DropInfo(int itemID, int quantity, int reinforce, int infusion)
        {
            ItemID = itemID;
            Quantity = (byte)quantity;
            Reinforcement = (byte)reinforce;
            Infusion = (byte)infusion;
        }

        public DropInfo Clone()
        {
            return (DropInfo)MemberwiseClone();
        }
        public void MarkPlaced() { IsPlaced = true; }



        // Properties:
        public bool IsKeyType => Enum.IsDefined(typeof(KEYID), ItemID);
                
        // Query Utility
        public bool HasItem(int itemid) => ItemID == itemid;
        public bool HasItem(ITEMID itemid) => HasItem((int)itemid);
        
        // Todo, improve properly
        public bool IsEqualTo(DropInfo di2)
        {
            return ItemID == di2.ItemID &&
                    Quantity == di2.Quantity &&
                    Infusion == di2.Infusion &&
                    Reinforcement == di2.Reinforcement;
        }
    }
}
