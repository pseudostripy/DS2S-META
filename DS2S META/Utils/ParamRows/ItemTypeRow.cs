using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    //// NOTE: I don't think this is a particularly stable form of weapon
    //// discrimination, might want to implement a better form.
    //public enum eItemType : byte
    //{
    //    WEAPON1     = 0,
    //    WEAPON2     = 1, // includes shields
    //    HEADARMOUR  = 2,
    //    CHESTARMOUR = 3,
    //    GAUNTLETS   = 4,
    //    LEGARMOUR   = 5,
    //    AMMO        = 6,
    //    RING        = 7,
    //    CONSUMABLE  = 8, // Includes keys
    //    SPELLS      = 9,
    //}

    /// <summary>
    /// Data Class for storing ItemParam
    /// </summary>
    public class ItemTypeRow : Param.Row
    {
        // Misc Fields:
        internal int ItemID;
        internal string MetaItemName => ItemID.AsMetaName();

        // Behind-fields:
        private int _unk00;
        private float _unk04;
        private float _unk08;
        private float _unk0C;
        private int _unk10;
        private int _unk14;
        private byte _unk18;
        private byte _unk19;
        private byte _unk1A;
        private byte _unk1B;
        
        

        // offsets:
        private enum ITFOFF
        {
            // ItemType Field Offsets (note not byte offsets!)
            UNK00 = 0,
            UNK04 = 1,
            UNK08 = 2,
            UNK0C = 3,
            UNK10 = 4,
            UNK14 = 5,
            UNK18 = 6,
            UNK19 = 7,
            UNK1A = 8,
            UNK1B = 9,
        }

        // Interfaces (Properties):
        internal int Unk00
        {
            get => _unk00;
            set
            {
                _unk00 = value;
                WriteAtField(ITFOFF.UNK00, BitConverter.GetBytes(value));
            }
        }
        internal float Unk04
        {
            get => _unk04;
            set
            {
                _unk04 = value;
                WriteAtField(ITFOFF.UNK04, BitConverter.GetBytes(value));
            }
        }
        internal float Unk08
        {
            get => _unk08;
            set
            {
                _unk08 = value;
                WriteAtField(ITFOFF.UNK08, BitConverter.GetBytes(value));
            }
        }
        internal float Unk0C
        {
            get => _unk0C;
            set
            {
                _unk0C = value;
                WriteAtField(ITFOFF.UNK0C, BitConverter.GetBytes(value));
            }
        }
        internal int Unk10
        {
            get => _unk10;
            set
            {
                _unk10 = value;
                WriteAtField(ITFOFF.UNK10, BitConverter.GetBytes(value));
            }
        }
        internal int Unk14
        {
            get => _unk14;
            set
            {
                _unk14 = value;
                WriteAtField(ITFOFF.UNK14, BitConverter.GetBytes(value));
            }
        }
        internal byte Unk18
        {
            get => _unk18;
            set
            {
                _unk18 = value;
                WriteByteAtField(ITFOFF.UNK18, _unk18);
            }
        }
        internal byte Unk19
        {
            get => _unk19;
            set
            {
                _unk19 = value;
                WriteByteAtField(ITFOFF.UNK19, _unk19);
            }
        }
        internal byte Unk1A
        {
            get => _unk1A;
            set
            {
                _unk1A = value;
                WriteByteAtField(ITFOFF.UNK1A, _unk1A);
            }
        }
        internal byte Unk1B
        {
            get => _unk1B;
            set
            {
                _unk1B = value;
                WriteByteAtField(ITFOFF.UNK1B, _unk1B);
            }
        }

        // Constructor:
        public ItemTypeRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            Unk00 = (int)ReadAtFieldNum(ITFOFF.UNK00);
            Unk04 = (float)ReadAtFieldNum(ITFOFF.UNK04);
            Unk08 = (float)ReadAtFieldNum(ITFOFF.UNK08);
            Unk0C = (float)ReadAtFieldNum(ITFOFF.UNK0C);
            Unk10 = (int)ReadAtFieldNum(ITFOFF.UNK10);
            Unk14 = (int)ReadAtFieldNum(ITFOFF.UNK14);
            Unk18 = (byte)ReadAtFieldNum(ITFOFF.UNK18);
            Unk19 = (byte)ReadAtFieldNum(ITFOFF.UNK19);
            Unk1A = (byte)ReadAtFieldNum(ITFOFF.UNK1A);
            Unk1B = (byte)ReadAtFieldNum(ITFOFF.UNK1B);            
        }
    }
}
