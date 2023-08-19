using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal class DropInfo
    {
        // Fields:
        internal int ItemID { get; set; }
        internal byte Quantity { get; set; }
        internal byte Infusion { get; set; }
        internal byte Reinforcement { get; set; }
        internal bool IsPlaced { get; private set; } = false;

        // Constructors:
        internal DropInfo() { }
        internal DropInfo(int itemID)
        {
            ItemID = itemID;
            Quantity = 1;
            Reinforcement = 0;
            Infusion = 0;
        }
        internal DropInfo(int itemID, int quantity)
        {
            ItemID = itemID;
            Quantity = (byte)quantity;
            Reinforcement = 0;
            Infusion = 0;
        }
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
        internal void MarkPlaced() { IsPlaced = true; }



        // Properties:
        internal bool IsKeyType => Enum.IsDefined(typeof(KEYID), ItemID);
        internal static List<eItemType> ReqTypes = new(){ eItemType.RING, eItemType.SPELLS };
        internal bool IsReqType { 
            get
            {
                var item = ParamMan.GetItemFromID(ItemID);
                if (item == null) return false;

                if (ReqTypes.Contains(item.ItemType)) // To generisize
                    return true;
                if (ItemSetBase.ManuallyRequiredItemsTypeRules.ContainsKey(ItemID))
                    return true;
                return false;
            } 
        }

        // Query Utility
        internal bool HasItem(int itemid) => ItemID == itemid;
        internal bool HasItem(ITEMID itemid) => HasItem((int)itemid);
        
        // Todo, improve properly
        internal bool IsEqualTo(DropInfo di2)
        {
            return ItemID == di2.ItemID &&
                    Quantity == di2.Quantity &&
                    Infusion == di2.Infusion &&
                    Reinforcement == di2.Reinforcement;
        }
    }
}
