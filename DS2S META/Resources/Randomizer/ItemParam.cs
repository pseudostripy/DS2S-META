using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    public enum eItemType : byte
    {
        STAFFCHIME  = 0,
        WEAPON      = 1, // includes shields
        HEADARMOUR  = 2,
        CHESTARMOUR = 3,
        GAUNTLETS   = 4,
        LEGARMOUR   = 5,
        RING        = 7,
        CONSUMABLE  = 8, // Includes keys
    }

    /// <summary>
    /// Data Class for storing ItemParam
    /// </summary>
    internal class ItemParam
    {
        internal string MetaItemName;
        internal int ItemID;
        internal int ItemUsageID;
        internal int MaxHeld;
        internal int BaseBuyPrice;
        internal eItemType ItemType;

        // Constructor:
        internal ItemParam(string metaItemName, int itemID, int itemUsageID, int maxHeld, int baseBuyPrice, byte itemType)
        {
            MetaItemName = metaItemName;
            ItemID = itemID;
            ItemUsageID = itemUsageID;
            MaxHeld = maxHeld;
            BaseBuyPrice = baseBuyPrice;
            ItemType = (eItemType)itemType;
        }
    }
}
