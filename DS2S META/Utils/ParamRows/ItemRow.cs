using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    public class ItemRow : Param.Row
    {
        // Misc Fields:
        internal string MetaItemName;
        internal int ItemID;

        // Behind-fields:
        private int _iconID;
        private int _weaponID;
        private int _armorID;
        private int _ammunitionID;
        private int _ringID;
        private int _spellID;
        private int _gestureID;
        internal int _basebuyprice;
        private int _itemUsageID;
        private int _maxheld;
        private eItemType _itemtype;

        // Properties:
        internal int IconID
        {
            get => _iconID;
            set
            {
                _iconID = value;
                WriteAt(0, BitConverter.GetBytes(value));
            }
        }
        internal int WeaponID
        {
            get => _weaponID;
            set
            {
                _weaponID = value;
                WriteAt(5, BitConverter.GetBytes(value));
            }
        }
        internal int ArmorID
        {
            get => _armorID;
            set
            {
                _armorID = value;
                WriteAt(6, BitConverter.GetBytes(value));
            }
        }
        internal int AmmunitionID
        {
            get => _ammunitionID;
            set
            {
                _ammunitionID = value;
                WriteAt(7, BitConverter.GetBytes(value));
            }
        }
        internal int RingID
        {
            get => _ringID;
            set
            {
                _ringID = value;
                WriteAt(8, BitConverter.GetBytes(value));
            }
        }
        internal int SpellID
        {
            get => _spellID;
            set
            {
                _spellID = value;
                WriteAt(9, BitConverter.GetBytes(value));
            }
        }
        internal int GestureID
        {
            get => _gestureID;
            set
            {
                _gestureID = value;
                WriteAt(10, BitConverter.GetBytes(value));
            }
        }
        internal int BaseBuyPrice
        {
            get => _basebuyprice;
            set
            {
                _basebuyprice = value;
                WriteAt(12, BitConverter.GetBytes(value));
            }
        }
        public int ItemUsageID
        {
            get => _itemUsageID;
            set
            {
                _itemUsageID = value;
                WriteAt(17, BitConverter.GetBytes(value));
            }
        }
        internal int MaxHeld
        {
            get => _maxheld;
            set
            {
                _maxheld = value;
                WriteAt(20, BitConverter.GetBytes(value));
            }
        }
        internal eItemType ItemType
        {
            get => _itemtype;
            set
            {
                _itemtype = value;
                WriteAt(24, BitConverter.GetBytes((byte)value));
            }
        }
        internal bool HasName => MetaItemName != string.Empty;

        // Linked Params:
        internal WeaponRow? WeaponRow => ParamMan.GetLink<WeaponRow>(ParamMan.WeaponParam, WeaponID);
        internal ArmorRow? ArmorRow => ParamMan.GetLink<ArmorRow>(ParamMan.ArmorParam, ArmorID);
        internal ItemUsageRow? ItemUsageRow => ParamMan.GetLink<ItemUsageRow>(ParamMan.ItemUsageParam, ItemUsageID);
        internal ArrowRow? ArrowRow => ParamMan.GetLink<ArrowRow>(ParamMan.ArrowParam, AmmunitionID);


        // Useful properties:
        private List<eItemType> WepSpellsArmour = new() 
        { eItemType.WEAPON1, 
          eItemType.WEAPON2, 
          eItemType.HEADARMOUR,
          eItemType.CHESTARMOUR,
          eItemType.GAUNTLETS,
          eItemType.LEGARMOUR,
          eItemType.SPELLS,
        };
        internal bool NeedsMadeUnique => WepSpellsArmour.Contains(ItemType);

        public enum Offsets
        {
            ItemUsageID  = 0x44,
            MaxHeld      = 0x4A,
            BaseBuyPrice = 0x30,
            ItemType     = 0x4F,
        }

        // Constructor:
        public ItemRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            IconID = (int)ReadAt(0);
            WeaponID = (int)ReadAt(5);
            ArmorID = (int)ReadAt(6);
            AmmunitionID = (int)ReadAt(7);
            RingID = (int)ReadAt(8);
            SpellID = (int)ReadAt(9);
            GestureID = (int)ReadAt(10);
            BaseBuyPrice = (int)ReadAt(12);
            ItemUsageID = (int)ReadAt(17);
            MaxHeld = (int)(short)ReadAt(20);
            ItemType = (eItemType)ReadAt(24);

            ItemID = GetItemID();
        }
        private int GetItemID()
        {
            // is this how they do it?
            if (WeaponID != -1)
                return WeaponID;
            if (ArmorID != -1)
                return ArmorID;
            if (AmmunitionID != -1)
                return AmmunitionID;
            if (RingID != -1)
                return RingID;
            if (SpellID != -1)
                return SpellID;
            if (GestureID != -1)
                return GestureID;
            return ID; // consumables are here I think
        }
        public List<DS2SInfusion> GetInfusionList()
        {
            // Method needs updating for armour etc
            if (WeaponID == -1 || WeaponRow == null)
                return new List<DS2SInfusion>() { DS2SInfusion.Infusions[0] };
            return WeaponRow.GetInfusionList();
        }
    }
}
