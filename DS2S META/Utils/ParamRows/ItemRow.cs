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
        internal int ItemID;
        internal string MetaItemName => ItemID.AsMetaName();

        // Behind-fields:
        private int _iconID;
        private int _effectId;
        private int _unk08;
        private int _unk0C;
        private int _unk10;
        private int _weaponID;
        private int _armorID;
        private int _ammunitionID;
        private int _ringID;
        private int _spellID;
        private int _gestureID;
        private int _sortId;
        internal int _basebuyprice;
        private int _sellPrice;
        private float _animationSpeed;
        private int _unk3C;
        private int _itemTypeParamId;
        private int _itemUsageID;
        private byte _unk48;
        private byte _unk49;
        private short _maxheld;
        private byte _unk4C;
        private byte _unk4D;
        private byte _unk4E;
        private byte _itemtype;
        private byte _unk50;
        private byte _spellSchool;
        private byte _itemState;
        private byte _unk53;

        // offsets:
        private enum IFOFF
        {
            // Item Field Offsets (note not byte offsets!)
            ICONID = 0,
            EFFECTID = 1,
            UNK08 = 2,
            UNK0C = 3,
            UNK10 = 4,
            WEAPONID = 5,
            ARMORID = 6,
            AMMUNITIONID = 7,
            RINGID = 8,
            SPELLID = 9,
            GESTUREID = 10,
            SORTID = 11,
            BASEBUY = 12,
            SELLPRICE = 13,
            ANIMSPEED = 14,
            UNK3C = 15,
            ITEMTYPEPARAMID = 16,
            ITEMUSAGEID = 17,
            UNK48 = 18,
            UNK49 = 19,
            MAXHELD = 20,
            UNK4C = 21,
            UNK4D = 22,
            UNK4E = 23,
            ITEMTYPE = 24,
            UNK50 = 25,
            SPELLSCHOOL = 26,
            ITEMSTATE = 27,
            UNK53 = 28,
        }

        // Interfaces (Properties):
        internal int IconID
        {
            get => _iconID;
            set
            {
                _iconID = value;
                WriteAtField(IFOFF.ICONID, BitConverter.GetBytes(value));
            }
        }
        internal int EffectId
        {
            get => _effectId;
            set
            {
                _effectId = value;
                WriteAtField(IFOFF.EFFECTID, BitConverter.GetBytes(value));
            }
        }
        internal int Unk08
        {
            get => _unk08;
            set
            {
                _unk08 = value;
                WriteAtField(IFOFF.UNK08, BitConverter.GetBytes(value));
            }
        }
        internal int Unk0C
        {
            get => _unk0C;
            set
            {
                _unk0C = value;
                WriteAtField(IFOFF.UNK0C, BitConverter.GetBytes(value));
            }
        }
        internal int Unk10
        {
            get => _unk10;
            set
            {
                _unk10 = value;
                WriteAtField(IFOFF.UNK10, BitConverter.GetBytes(value));
            }
        }
        internal int WeaponID
        {
            get => _weaponID;
            set
            {
                _weaponID = value;
                WriteAtField(IFOFF.WEAPONID, BitConverter.GetBytes(value));
            }
        }
        internal int ArmorID
        {
            get => _armorID;
            set
            {
                _armorID = value;
                WriteAtField(IFOFF.ARMORID, BitConverter.GetBytes(value));
            }
        }
        internal int AmmunitionID
        {
            get => _ammunitionID;
            set
            {
                _ammunitionID = value;
                WriteAtField(IFOFF.AMMUNITIONID, BitConverter.GetBytes(value));
            }
        }
        internal int RingID
        {
            get => _ringID;
            set
            {
                _ringID = value;
                WriteAtField(IFOFF.RINGID, BitConverter.GetBytes(value));
            }
        }
        internal int SpellID
        {
            get => _spellID;
            set
            {
                _spellID = value;
                WriteAtField(IFOFF.SPELLID, BitConverter.GetBytes(value));
            }
        }
        internal int GestureID
        {
            get => _gestureID;
            set
            {
                _gestureID = value;
                WriteAtField(IFOFF.GESTUREID, BitConverter.GetBytes(value));
            }
        }
        internal int SortId
        {
            get => _sortId;
            set
            {
                _sortId = value;
                WriteAtField(IFOFF.SORTID, BitConverter.GetBytes(value));
            }
        }
        internal int BaseBuyPrice
        {
            get => _basebuyprice;
            set
            {
                _basebuyprice = value;
                WriteAtField(IFOFF.BASEBUY, BitConverter.GetBytes(value));
            }
        }
        internal int SellPrice
        {
            get => _sellPrice;
            set
            {
                _sellPrice = value;
                WriteAtField(IFOFF.SELLPRICE, BitConverter.GetBytes(value));
            }
        }
        internal float AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                _animationSpeed = value;
                WriteAtField(IFOFF.ANIMSPEED, BitConverter.GetBytes(value));
            }
        }
        internal int Unk3C
        {
            get => _unk3C;
            set
            {
                _unk3C = value;
                WriteAtField(IFOFF.UNK3C, BitConverter.GetBytes(value));
            }
        }
        internal int ItemTypeParamId
        {
            get => _itemTypeParamId;
            set
            {
                _itemTypeParamId = value;
                WriteAtField(IFOFF.ITEMTYPEPARAMID, BitConverter.GetBytes(value));
            }
        }
        public int ItemUsageID
        {
            get => _itemUsageID;
            set
            {
                _itemUsageID = value;
                WriteAtField(IFOFF.ITEMUSAGEID, BitConverter.GetBytes(value));
            }
        }
        internal int Unk48
        {
            get => _unk48;
            set
            {
                _unk48 = (byte)value;
                WriteByteAtField(IFOFF.UNK48, _unk48);
            }
        }
        internal int Unk49
        {
            get => _unk49;
            set
            {
                _unk49 = (byte)value;
                WriteByteAtField(IFOFF.UNK49, _unk49);
            }
        }
        internal int MaxHeld
        {
            get => _maxheld;
            set
            {
                _maxheld = (short)value;
                WriteAtField(IFOFF.MAXHELD, BitConverter.GetBytes(value));
            }
        }
        internal int Unk4C
        {
            get => _unk4C;
            set
            {
                _unk4C = (byte)value;
                WriteByteAtField(IFOFF.UNK4C, _unk4C);
            }
        }
        internal int Unk4D
        {
            get => _unk4D;
            set
            {
                _unk4D = (byte)value;
                WriteByteAtField(IFOFF.UNK4D, _unk4D);
            }
        }
        internal int Unk4E
        {
            get => _unk4E;
            set
            {
                _unk4E = (byte)value;
                WriteByteAtField(IFOFF.UNK4E, _unk4E);
            }
        }
        internal eItemType ItemType
        {
            get => (eItemType)_itemtype;
            set
            {
                _itemtype = (byte)value;
                WriteByteAtField(IFOFF.ITEMTYPE, _itemtype);
            }
        }
        internal int Unk50
        {
            get => _unk50;
            set
            {
                _unk50 = (byte)value;
                WriteByteAtField(IFOFF.UNK50, _unk50);
            }
        }
        internal int SpellSchool
        {
            get => _spellSchool;
            set
            {
                _spellSchool = (byte)value;
                WriteByteAtField(IFOFF.SPELLSCHOOL, _spellSchool);
            }
        }
        internal int ItemState
        {
            get => _itemState;
            set
            {
                _itemState = (byte)value;
                WriteByteAtField(IFOFF.ITEMSTATE, _itemState);
            }
        }
        internal int Unk53
        {
            get => _unk53;
            set
            {
                _unk53 = (byte)value;
                WriteByteAtField(IFOFF.UNK53, _unk53);
            }
        }

        // Helpful properties
        internal bool HasName => MetaItemName != string.Empty;
        internal bool IsWeaponType => ItemType == eItemType.WEAPON1 || ItemType == eItemType.WEAPON2;

        // Linked Params:
        internal WeaponRow? WeaponRow => ParamMan.GetLink<WeaponRow>(ParamMan.WeaponParam, WeaponID);
        internal ArmorRow? ArmorRow => ParamMan.GetLink<ArmorRow>(ParamMan.ArmorParam, ArmorID);
        internal ItemTypeRow? ItemTypeRow => ParamMan.GetLink<ItemTypeRow>(ParamMan.ItemTypeParam, ItemTypeParamId);
        internal ItemUsageRow? ItemUsageRow => ParamMan.GetLink<ItemUsageRow>(ParamMan.ItemUsageParam, ItemUsageID);
        internal ArrowRow? ArrowRow => ParamMan.GetLink<ArrowRow>(ParamMan.ArrowParam, AmmunitionID);


        // Constructor:
        public ItemRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            IconID = (int)ReadAtFieldNum(IFOFF.ICONID);
            EffectId = (int)ReadAtFieldNum(IFOFF.EFFECTID);
            Unk08 = (int)ReadAtFieldNum(IFOFF.UNK08);
            Unk0C = (int)ReadAtFieldNum(IFOFF.UNK0C);
            Unk10 = (int)ReadAtFieldNum(IFOFF.UNK10);
            WeaponID = (int)ReadAtFieldNum(IFOFF.WEAPONID);
            ArmorID = (int)ReadAtFieldNum(IFOFF.ARMORID);
            AmmunitionID = (int)ReadAtFieldNum(IFOFF.AMMUNITIONID);
            RingID = (int)ReadAtFieldNum(IFOFF.RINGID);
            SpellID = (int)ReadAtFieldNum(IFOFF.SPELLID);
            GestureID = (int)ReadAtFieldNum(IFOFF.GESTUREID);
            SortId = (int)ReadAtFieldNum(IFOFF.SORTID);
            BaseBuyPrice = (int)ReadAtFieldNum(IFOFF.BASEBUY);
            SellPrice = (int)ReadAtFieldNum(IFOFF.SELLPRICE);
            AnimationSpeed = (float)ReadAtFieldNum(IFOFF.ANIMSPEED);
            Unk3C = (int)ReadAtFieldNum(IFOFF.UNK3C);
            ItemTypeParamId = (int)ReadAtFieldNum(IFOFF.ITEMTYPEPARAMID);
            ItemUsageID = (int)ReadAtFieldNum(IFOFF.ITEMUSAGEID);
            Unk48 = (byte)ReadAtFieldNum(IFOFF.UNK48);
            Unk49 = (byte)ReadAtFieldNum(IFOFF.UNK49);
            MaxHeld = (short)ReadAtFieldNum(IFOFF.MAXHELD);
            Unk4C = (byte)ReadAtFieldNum(IFOFF.UNK4C);
            Unk4D = (byte)ReadAtFieldNum(IFOFF.UNK4D);
            Unk4E = (byte)ReadAtFieldNum(IFOFF.UNK4E);
            ItemType = (eItemType)ReadAtFieldNum(IFOFF.ITEMTYPE);
            SpellSchool = (byte)ReadAtFieldNum(IFOFF.SPELLSCHOOL);
            ItemState = (byte)ReadAtFieldNum(IFOFF.ITEMSTATE);
            Unk53 = (byte)ReadAtFieldNum(IFOFF.UNK53);

            ItemID = GetItemID();
        }
        private int GetItemID()
        {
            return ID;
            //// is this how they do it?
            //if (WeaponID != -1)
            //    return WeaponID;
            //if (ArmorID != -1)
            //    return ArmorID;
            //if (AmmunitionID != -1)
            //    return AmmunitionID;
            //if (RingID != -1)
            //    return RingID;
            //if (SpellID != -1)
            //    return SpellID;
            //if (GestureID != -1)
            //    return GestureID;
            //return ID; // consumables are here I think
        }
        public List<DS2SInfusion> GetInfusionList()
        {
            // Method needs updating for armour etc
            if (WeaponID == -1 || WeaponRow == null)
                return new List<DS2SInfusion>() { DS2SInfusion.Infusions[0] };
            return WeaponRow.GetInfusionList();
        }

        // To improve
        internal int GetItemMaxUpgrade()
        {
            // Wrapper similar to the DS2Item class call in Hook.
            return ItemType switch
            {
                eItemType.WEAPON1 or eItemType.WEAPON2 => WeaponRow?.MaxUpgrade ?? 0,
                eItemType.HEADARMOUR or eItemType.CHESTARMOUR or eItemType.GAUNTLETS or eItemType.LEGARMOUR => ArmorRow?.ArmorReinforceRow?.MaxReinforceLevel ?? 0,
                _ => 0
            };
        }
    }
}
