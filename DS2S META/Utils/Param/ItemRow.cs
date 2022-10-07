using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    // NOTE: I don't think this is a particularly stable form of weapon
    // discrimination, might want to implement a better form.
    public enum eItemType : byte
    {
        WEAPON1     = 0,
        WEAPON2     = 1, // includes shields
        HEADARMOUR  = 2,
        CHESTARMOUR = 3,
        GAUNTLETS   = 4,
        LEGARMOUR   = 5,
        AMMO        = 6,
        RING        = 7,
        CONSUMABLE  = 8, // Includes keys
        SPELLS      = 9,
    }

    /// <summary>
    /// Data Class for storing ItemParam
    /// </summary>
    public class ItemRow
    {
        internal Param.Row ParamRow;
        internal int ID => ParamRow.ID;

        internal string MetaItemName;

        internal int IconID;
        internal int ItemID;
        internal int WeaponID;
        internal int ArmourID;
        internal int AmmunitionID;
        internal int RingID;
        internal int SpellID;
        internal int GestureID;
        internal int ItemUsageID;
        internal int MaxHeld;
        internal int _basebuyprice;
        internal int BaseBuyPrice {
            get => _basebuyprice;
            set
            {
                _basebuyprice = value;
                WriteAt(12, BitConverter.GetBytes(value));
            }
        }
        internal eItemType ItemType;

        public enum Offsets
        {
            ItemUsageID  = 0x44,
            MaxHeld      = 0x4A,
            BaseBuyPrice = 0x30,
            ItemType     = 0x4F,
        }

        // Constructor:
        internal ItemRow(Param.Row paramrow)
        {
            // Unpack data:
            ParamRow = paramrow;

            IconID = (int)ReadAt(0);
            WeaponID = (int)ReadAt(5);
            ArmourID = (int)ReadAt(6);
            AmmunitionID = (int)ReadAt(7);
            RingID = (int)ReadAt(8);
            SpellID = (int)ReadAt(9);
            GestureID = (int)ReadAt(10);
            ItemID = GetItemID();
            BaseBuyPrice = (int)ReadAt(12);
            ItemUsageID = (int)ReadAt(17);
            MaxHeld = (int)(short)ReadAt(20);
            ItemType = (eItemType)ReadAt(24);
        }
        private int GetItemID()
        {
            // is this how they do it?
            if (WeaponID != -1)
                return WeaponID;
            if (ArmourID != -1)
                return ArmourID;
            if (AmmunitionID != -1)
                return AmmunitionID;
            if (RingID != -1)
                return RingID;
            if (SpellID != -1)
                return SpellID;
            if (GestureID != -1)
                return GestureID;
            return IconID; // last ditch save
        }

        public object ReadAt(int fieldindex) => ParamRow.Data[fieldindex];
        public void WriteAt(int fieldindex, byte[] valuebytes)
        {
            // Note: this function isn't generalised properly yet
            int fieldoffset = ParamRow.Param.Fields[fieldindex].FieldOffset;
            Array.Copy(valuebytes, 0, ParamRow.RowBytes, fieldoffset, valuebytes.Length);
        }
    }
}
