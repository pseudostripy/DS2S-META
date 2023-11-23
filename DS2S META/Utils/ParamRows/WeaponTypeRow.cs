using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    /// <summary>
    /// Data Class for storing Weapons
    /// </summary>
    public class WeaponTypeRow : Param.Row
    {
        private enum OFFS
        {
            ISCASTSORCERIES = 0x0,
            ISCASTMIRACLES = 0x1,
            ISCASTPYROMANCIES = 0x2,
            ISCASTHEXESSTAVES = 0x3,
            UNK00 = 0x4,
            UNK08 = 0x8,
            GRIPSTYLE_2H_RIGHT = 0xc,
            GRIPSTYLE_2H_LEFT = 0x10,
            GRIPSTYLE_1H_RIGHT = 0x14,
            GRIPSTYLE_1H_LEFT = 0x18,
            GRIPSTYLE_POWERSTANCE_RIGHT = 0x1c,
            GRIPSTYLE_POWERSTANCE_LEFT = 0x20,
            POWERSTANCE_SWAP_TYPE = 0x24,
            POSITION_1H_RIGHT = 0x25,
            RELATIVE_POSITION_1H_RIGHT = 0x26,
            POSITION_1H_LEFT = 0x27,
            RELATIVE_POSITION_1H_LEFT = 0x28,
            POSITION_2H_RIGHT = 0x29,
            RELATIVE_POSITION_2H_RIGHT = 0x2a,
            POSITION_UNKNOWN1 = 0x2b,
            RELATIVE_POSITION_UNKNOWN1 = 0x2c,
            POSITION_2H_LEFT = 0x2d,
            RELATIVE_POSITION_2H_LEFT = 0x2e,
            POSITION_UNKNOWN2 = 0x2f,
            RELATIVE_POSITION_UNKNOWN2 = 0x30,
            HOLSTERPOSITION_RIGHT = 0x31,
            HOLSTERRELATIVE_POSITION_RIGHT = 0x32,
            HOLSTERPOSITION_LEFT = 0x33,
            HOLSTERRELATIVE_POSITION_LEFT = 0x34,
            CASTTYPE = 0x35,
            WEAPONCATEGORYNAME = 0x36,
            SWINGSOUND = 0x37,
            POWERSTANCECATEGORY = 0x38,
            UNK3C = 0x3c,
            UNK40 = 0x40,
            UNK44 = 0x44,
            UNK48 = 0x48,
            UNK4C = 0x4c,
            UNK50 = 0x50,
            UNK51 = 0x51,
            UNK52 = 0x52,
            ISCASTHEXESCHIME = 0x53,
            LMOD = 0x54,
            UNK58 = 0x58,
            LEFTHANDSTAMINADAMAGEMULT = 0x5c,
            RMOD = 0x60,
            UNK13 = 0x64,
            RIGHTHANDSTAMINADAMAGEMULT = 0x68,
            COUNTERDAMAGE = 0x6c,
            UNK70 = 0x70,
            UNK74 = 0x74,
            UNK78 = 0x78,
            UNK7C = 0x7c,
            UNK80 = 0x80,
            UNK84 = 0x84,
            UNK88 = 0x88,
            UNK8C = 0x8c,
            UNK90 = 0x90,
            UNK94 = 0x94,
            UNK98 = 0x98,
            UNK9C = 0x9c,
            UNKA0 = 0xa0,
            UNKA1 = 0xa1,
            BOWDISTANCE = 0xa2,
            BACKSTABCHECKANIM = 0xa4,
            GUARDBREAKCHECKANIM = 0xa8,
            UNKAC = 0xac,
            UNKB0 = 0xb0,
            UNKB4 = 0xb4,
            UNKB8 = 0xb8,
            UNKBC = 0xbc,
            UNKC0 = 0xc0,
            UNKC4 = 0xc4,
            UNKC8 = 0xc8,
            GUARDSTAMINAREGEN = 0xcc,
            UNKD0 = 0xd0,
            UNKD4 = 0xd4,
            UNKD8 = 0xd8,
            UNKDC = 0xdc,
            UNKE0 = 0xe0,
            UNKE4 = 0xe4,
            UNKE8 = 0xe8,
            UNKEC = 0xec,
            UNKF0 = 0xf0,
            UNKF4 = 0xf4,
            UNKF8 = 0xf8,
            UNKFC = 0xfc,
            UNK100 = 0x100,
            UNK104 = 0x104,
            UNK108 = 0x108,
            CASTSPEED = 0x10c,
            UNK110 = 0x110,
            UNK114 = 0x114,
            UNK118 = 0x118,
            UNK11C = 0x11c,
            UNK120 = 0x120,
            UNK124 = 0x124,
            UNK128 = 0x128,
            UNK12C = 0x12c,
            MENUCATPARAMID = 0x130,
            UNK134 = 0x134,
            UNK138 = 0x138,
            UNK139 = 0x139,
            MUNDANESCALINGMULT = 0x13a,
            UNK13B = 0x13b,
        }

        // Behind-fields
        private byte _isCastSorceries;
        private byte _isCastMiracles;
        private byte _isCastPyromancies;
        private byte _isCastHexesStaves;
        private int _unk00;
        private int _unk08;
        private int _gripStyle_2h_right;
        private int _gripStyle_2h_left;
        private int _gripStyle_1h_right;
        private int _gripStyle_1h_left;
        private int _gripStyle_powerstance_right;
        private int _gripStyle_powerstance_left;
        private byte _powerstance_swap_type;
        private byte _position_1h_right;
        private byte _relative_position_1h_right;
        private byte _position_1h_left;
        private byte _relative_position_1h_left;
        private byte _position_2h_right;
        private byte _relative_position_2h_right;
        private byte _position_unknown1;
        private byte _relative_position_unknown1;
        private byte _position_2h_left;
        private byte _relative_position_2h_left;
        private byte _position_unknown2;
        private byte _relative_position_unknown2;
        private byte _holsterPosition_right;
        private byte _holsterrelative_position_right;
        private byte _holsterPosition_left;
        private byte _holsterrelative_position_left;
        private byte _castType;
        private byte _weaponCategoryName;
        private byte _swingSound;
        private int _powerstanceCategory;
        private float _unk3c;
        private float _unk40;
        private float _unk44;
        private float _unk48;
        private float _unk4c;
        private byte _unk50;
        private byte _unk51;
        private byte _unk52;
        private byte _isCastHexesChime;
        private float _lMod;
        private float _unk58;
        private float _leftHandStaminaDamageMult;
        private float _rMod;
        private float _unk13;
        private float _rightHandStaminaDamageMult;
        private float _counterDamage;
        private float _unk70;
        private float _unk74;
        private float _unk78;
        private float _unk7c;
        private float _unk80;
        private float _unk84;
        private float _unk88;
        private float _unk8c;
        private float _unk90;
        private float _unk94;
        private float _unk98;
        private float _unk9c;
        private byte _unka0;
        private byte _unka1;
        private short _bowDistance;
        private int _backstabCheckAnim;
        private int _guardbreakCheckAnim;
        private float _unkac;
        private float _unkb0;
        private float _unkb4;
        private float _unkb8;
        private float _unkbc;
        private float _unkc0;
        private float _unkc4;
        private float _unkc8;
        private float _guardStaminaRegen;
        private float _unkd0;
        private float _unkd4;
        private float _unkd8;
        private float _unkdc;
        private float _unke0;
        private float _unke4;
        private float _unke8;
        private float _unkec;
        private float _unkf0;
        private float _unkf4;
        private float _unkf8;
        private float _unkfc;
        private int _unk100;
        private float _unk104;
        private float _unk108;
        private float _castSpeed;
        private float _unk110;
        private float _unk114;
        private float _unk118;
        private float _unk11c;
        private float _unk120;
        private float _unk124;
        private float _unk128;
        private float _unk12c;
        private int _menuCategoryParamId;
        private int _unk134;
        private byte _unk138;
        private byte _unk139;
        private byte _mundaneScalingMult;
        private byte _unk13b;

        // Properties:
        public byte IsCastSorceries
        {
            get => _isCastSorceries;
            set
            {
                _isCastSorceries = value;
                WriteByteAt(OFFS.ISCASTSORCERIES, value);
            }
        }
        public byte IsCastMiracles
        {
            get => _isCastMiracles;
            set
            {
                _isCastMiracles = value;
                WriteByteAt(OFFS.ISCASTMIRACLES, value);
            }
        }
        public byte IsCastPyromancies
        {
            get => _isCastPyromancies;
            set
            {
                _isCastPyromancies = value;
                WriteByteAt(OFFS.ISCASTPYROMANCIES, value);
            }
        }
        public byte IsCastHexesStaves
        {
            get => _isCastHexesStaves;
            set
            {
                _isCastHexesStaves = value;
                WriteByteAt(OFFS.ISCASTHEXESSTAVES, value);
            }
        }
        public int Unk00
        {
            get => _unk00;
            set
            {
                _unk00 = value;
                WriteIntAt(OFFS.UNK00, value);
            }
        }
        public int Unk08
        {
            get => _unk08;
            set
            {
                _unk08 = value;
                WriteIntAt(OFFS.UNK08, value);
            }
        }
        public int GripStyle_2h_right
        {
            get => _gripStyle_2h_right;
            set
            {
                _gripStyle_2h_right = value;
                WriteIntAt(OFFS.GRIPSTYLE_2H_RIGHT, value);
            }
        }
        public int GripStyle_2h_left
        {
            get => _gripStyle_2h_left;
            set
            {
                _gripStyle_2h_left = value;
                WriteIntAt(OFFS.GRIPSTYLE_2H_LEFT, value);
            }
        }
        public int GripStyle_1h_right
        {
            get => _gripStyle_1h_right;
            set
            {
                _gripStyle_1h_right = value;
                WriteIntAt(OFFS.GRIPSTYLE_1H_RIGHT, value);
            }
        }
        public int GripStyle_1h_left
        {
            get => _gripStyle_1h_left;
            set
            {
                _gripStyle_1h_left = value;
                WriteIntAt(OFFS.GRIPSTYLE_1H_LEFT, value);
            }
        }
        public int GripStyle_powerstance_right
        {
            get => _gripStyle_powerstance_right;
            set
            {
                _gripStyle_powerstance_right = value;
                WriteIntAt(OFFS.GRIPSTYLE_POWERSTANCE_RIGHT, value);
            }
        }
        public int GripStyle_powerstance_left
        {
            get => _gripStyle_powerstance_left;
            set
            {
                _gripStyle_powerstance_left = value;
                WriteIntAt(OFFS.GRIPSTYLE_POWERSTANCE_LEFT, value);
            }
        }
        public byte Powerstance_swap_type
        {
            get => _powerstance_swap_type;
            set
            {
                _powerstance_swap_type = value;
                WriteByteAt(OFFS.POWERSTANCE_SWAP_TYPE, value);
            }
        }
        public byte Position_1h_right
        {
            get => _position_1h_right;
            set
            {
                _position_1h_right = value;
                WriteByteAt(OFFS.POSITION_1H_RIGHT, value);
            }
        }
        public byte Relative_position_1h_right
        {
            get => _relative_position_1h_right;
            set
            {
                _relative_position_1h_right = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_1H_RIGHT, value);
            }
        }
        public byte Position_1h_left
        {
            get => _position_1h_left;
            set
            {
                _position_1h_left = value;
                WriteByteAt(OFFS.POSITION_1H_LEFT, value);
            }
        }
        public byte Relative_position_1h_left
        {
            get => _relative_position_1h_left;
            set
            {
                _relative_position_1h_left = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_1H_LEFT, value);
            }
        }
        public byte Position_2h_right
        {
            get => _position_2h_right;
            set
            {
                _position_2h_right = value;
                WriteByteAt(OFFS.POSITION_2H_RIGHT, value);
            }
        }
        public byte Relative_position_2h_right
        {
            get => _relative_position_2h_right;
            set
            {
                _relative_position_2h_right = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_2H_RIGHT, value);
            }
        }
        public byte Position_unknown1
        {
            get => _position_unknown1;
            set
            {
                _position_unknown1 = value;
                WriteByteAt(OFFS.POSITION_UNKNOWN1, value);
            }
        }
        public byte Relative_position_unknown1
        {
            get => _relative_position_unknown1;
            set
            {
                _relative_position_unknown1 = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_UNKNOWN1, value);
            }
        }
        public byte Position_2h_left
        {
            get => _position_2h_left;
            set
            {
                _position_2h_left = value;
                WriteByteAt(OFFS.POSITION_2H_LEFT, value);
            }
        }
        public byte Relative_position_2h_left
        {
            get => _relative_position_2h_left;
            set
            {
                _relative_position_2h_left = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_2H_LEFT, value);
            }
        }
        public byte Position_unknown2
        {
            get => _position_unknown2;
            set
            {
                _position_unknown2 = value;
                WriteByteAt(OFFS.POSITION_UNKNOWN2, value);
            }
        }
        public byte Relative_position_unknown2
        {
            get => _relative_position_unknown2;
            set
            {
                _relative_position_unknown2 = value;
                WriteByteAt(OFFS.RELATIVE_POSITION_UNKNOWN2, value);
            }
        }
        public byte HolsterPosition_right
        {
            get => _holsterPosition_right;
            set
            {
                _holsterPosition_right = value;
                WriteByteAt(OFFS.HOLSTERPOSITION_RIGHT, value);
            }
        }
        public byte Holsterrelative_position_right
        {
            get => _holsterrelative_position_right;
            set
            {
                _holsterrelative_position_right = value;
                WriteByteAt(OFFS.HOLSTERRELATIVE_POSITION_RIGHT, value);
            }
        }
        public byte HolsterPosition_left
        {
            get => _holsterPosition_left;
            set
            {
                _holsterPosition_left = value;
                WriteByteAt(OFFS.HOLSTERPOSITION_LEFT, value);
            }
        }
        public byte Holsterrelative_position_left
        {
            get => _holsterrelative_position_left;
            set
            {
                _holsterrelative_position_left = value;
                WriteByteAt(OFFS.HOLSTERRELATIVE_POSITION_LEFT, value);
            }
        }
        public byte CastType
        {
            get => _castType;
            set
            {
                _castType = value;
                WriteByteAt(OFFS.CASTTYPE, value);
            }
        }
        public byte WeaponCategoryName
        {
            get => _weaponCategoryName;
            set
            {
                _weaponCategoryName = value;
                WriteByteAt(OFFS.WEAPONCATEGORYNAME, value);
            }
        }
        public byte SwingSound
        {
            get => _swingSound;
            set
            {
                _swingSound = value;
                WriteByteAt(OFFS.SWINGSOUND, value);
            }
        }
        public int PowerstanceCategory
        {
            get => _powerstanceCategory;
            set
            {
                _powerstanceCategory = value;
                WriteIntAt(OFFS.POWERSTANCECATEGORY, value);
            }
        }
        public float Unk3c
        {
            get => _unk3c;
            set
            {
                _unk3c = value;
                WriteFloatAt(OFFS.UNK3C, value);
            }
        }
        public float Unk40
        {
            get => _unk40;
            set
            {
                _unk40 = value;
                WriteFloatAt(OFFS.UNK40, value);
            }
        }
        public float Unk44
        {
            get => _unk44;
            set
            {
                _unk44 = value;
                WriteFloatAt(OFFS.UNK44, value);
            }
        }
        public float Unk48
        {
            get => _unk48;
            set
            {
                _unk48 = value;
                WriteFloatAt(OFFS.UNK48, value);
            }
        }
        public float Unk4c
        {
            get => _unk4c;
            set
            {
                _unk4c = value;
                WriteFloatAt(OFFS.UNK4C, value);
            }
        }
        public byte Unk50
        {
            get => _unk50;
            set
            {
                _unk50 = value;
                WriteByteAt(OFFS.UNK50, value);
            }
        }
        public byte Unk51
        {
            get => _unk51;
            set
            {
                _unk51 = value;
                WriteByteAt(OFFS.UNK51, value);
            }
        }
        public byte Unk52
        {
            get => _unk52;
            set
            {
                _unk52 = value;
                WriteByteAt(OFFS.UNK52, value);
            }
        }
        public byte IsCastHexesChime
        {
            get => _isCastHexesChime;
            set
            {
                _isCastHexesChime = value;
                WriteByteAt(OFFS.ISCASTHEXESCHIME, value);
            }
        }
        public float LMod
        {
            get => _lMod;
            set
            {
                _lMod = value;
                WriteFloatAt(OFFS.LMOD, value);
            }
        }
        public float Unk58
        {
            get => _unk58;
            set
            {
                _unk58 = value;
                WriteFloatAt(OFFS.UNK58, value);
            }
        }
        public float LeftHandStaminaDamageMult
        {
            get => _leftHandStaminaDamageMult;
            set
            {
                _leftHandStaminaDamageMult = value;
                WriteFloatAt(OFFS.LEFTHANDSTAMINADAMAGEMULT, value);
            }
        }
        public float RMod
        {
            get => _rMod;
            set
            {
                _rMod = value;
                WriteFloatAt(OFFS.RMOD, value);
            }
        }
        public float Unk13
        {
            get => _unk13;
            set
            {
                _unk13 = value;
                WriteFloatAt(OFFS.UNK13, value);
            }
        }
        public float RightHandStaminaDamageMult
        {
            get => _rightHandStaminaDamageMult;
            set
            {
                _rightHandStaminaDamageMult = value;
                WriteFloatAt(OFFS.RIGHTHANDSTAMINADAMAGEMULT, value);
            }
        }
        public float CounterDamage
        {
            get => _counterDamage;
            set
            {
                _counterDamage = value;
                WriteFloatAt(OFFS.COUNTERDAMAGE, value);
            }
        }
        public float Unk70
        {
            get => _unk70;
            set
            {
                _unk70 = value;
                WriteFloatAt(OFFS.UNK70, value);
            }
        }
        public float Unk74
        {
            get => _unk74;
            set
            {
                _unk74 = value;
                WriteFloatAt(OFFS.UNK74, value);
            }
        }
        public float Unk78
        {
            get => _unk78;
            set
            {
                _unk78 = value;
                WriteFloatAt(OFFS.UNK78, value);
            }
        }
        public float Unk7c
        {
            get => _unk7c;
            set
            {
                _unk7c = value;
                WriteFloatAt(OFFS.UNK7C, value);
            }
        }
        public float Unk80
        {
            get => _unk80;
            set
            {
                _unk80 = value;
                WriteFloatAt(OFFS.UNK80, value);
            }
        }
        public float Unk84
        {
            get => _unk84;
            set
            {
                _unk84 = value;
                WriteFloatAt(OFFS.UNK84, value);
            }
        }
        public float Unk88
        {
            get => _unk88;
            set
            {
                _unk88 = value;
                WriteFloatAt(OFFS.UNK88, value);
            }
        }
        public float Unk8c
        {
            get => _unk8c;
            set
            {
                _unk8c = value;
                WriteFloatAt(OFFS.UNK8C, value);
            }
        }
        public float Unk90
        {
            get => _unk90;
            set
            {
                _unk90 = value;
                WriteFloatAt(OFFS.UNK90, value);
            }
        }
        public float Unk94
        {
            get => _unk94;
            set
            {
                _unk94 = value;
                WriteFloatAt(OFFS.UNK94, value);
            }
        }
        public float Unk98
        {
            get => _unk98;
            set
            {
                _unk98 = value;
                WriteFloatAt(OFFS.UNK98, value);
            }
        }
        public float Unk9c
        {
            get => _unk9c;
            set
            {
                _unk9c = value;
                WriteFloatAt(OFFS.UNK9C, value);
            }
        }
        public byte Unka0
        {
            get => _unka0;
            set
            {
                _unka0 = value;
                WriteByteAt(OFFS.UNKA0, value);
            }
        }
        public byte Unka1
        {
            get => _unka1;
            set
            {
                _unka1 = value;
                WriteByteAt(OFFS.UNKA1, value);
            }
        }
        public short BowDistance
        {
            get => _bowDistance;
            set
            {
                _bowDistance = value;
                WriteShortAt(OFFS.BOWDISTANCE, value);
            }
        }
        public int BackstabCheckAnim
        {
            get => _backstabCheckAnim;
            set
            {
                _backstabCheckAnim = value;
                WriteIntAt(OFFS.BACKSTABCHECKANIM, value);
            }
        }
        public int GuardbreakCheckAnim
        {
            get => _guardbreakCheckAnim;
            set
            {
                _guardbreakCheckAnim = value;
                WriteIntAt(OFFS.GUARDBREAKCHECKANIM, value);
            }
        }
        public float Unkac
        {
            get => _unkac;
            set
            {
                _unkac = value;
                WriteFloatAt(OFFS.UNKAC, value);
            }
        }
        public float Unkb0
        {
            get => _unkb0;
            set
            {
                _unkb0 = value;
                WriteFloatAt(OFFS.UNKB0, value);
            }
        }
        public float Unkb4
        {
            get => _unkb4;
            set
            {
                _unkb4 = value;
                WriteFloatAt(OFFS.UNKB4, value);
            }
        }
        public float Unkb8
        {
            get => _unkb8;
            set
            {
                _unkb8 = value;
                WriteFloatAt(OFFS.UNKB8, value);
            }
        }
        public float Unkbc
        {
            get => _unkbc;
            set
            {
                _unkbc = value;
                WriteFloatAt(OFFS.UNKBC, value);
            }
        }
        public float Unkc0
        {
            get => _unkc0;
            set
            {
                _unkc0 = value;
                WriteFloatAt(OFFS.UNKC0, value);
            }
        }
        public float Unkc4
        {
            get => _unkc4;
            set
            {
                _unkc4 = value;
                WriteFloatAt(OFFS.UNKC4, value);
            }
        }
        public float Unkc8
        {
            get => _unkc8;
            set
            {
                _unkc8 = value;
                WriteFloatAt(OFFS.UNKC8, value);
            }
        }
        public float GuardStaminaRegen
        {
            get => _guardStaminaRegen;
            set
            {
                _guardStaminaRegen = value;
                WriteFloatAt(OFFS.GUARDSTAMINAREGEN, value);
            }
        }
        public float Unkd0
        {
            get => _unkd0;
            set
            {
                _unkd0 = value;
                WriteFloatAt(OFFS.UNKD0, value);
            }
        }
        public float Unkd4
        {
            get => _unkd4;
            set
            {
                _unkd4 = value;
                WriteFloatAt(OFFS.UNKD4, value);
            }
        }
        public float Unkd8
        {
            get => _unkd8;
            set
            {
                _unkd8 = value;
                WriteFloatAt(OFFS.UNKD8, value);
            }
        }
        public float Unkdc
        {
            get => _unkdc;
            set
            {
                _unkdc = value;
                WriteFloatAt(OFFS.UNKDC, value);
            }
        }
        public float Unke0
        {
            get => _unke0;
            set
            {
                _unke0 = value;
                WriteFloatAt(OFFS.UNKE0, value);
            }
        }
        public float Unke4
        {
            get => _unke4;
            set
            {
                _unke4 = value;
                WriteFloatAt(OFFS.UNKE4, value);
            }
        }
        public float Unke8
        {
            get => _unke8;
            set
            {
                _unke8 = value;
                WriteFloatAt(OFFS.UNKE8, value);
            }
        }
        public float Unkec
        {
            get => _unkec;
            set
            {
                _unkec = value;
                WriteFloatAt(OFFS.UNKEC, value);
            }
        }
        public float Unkf0
        {
            get => _unkf0;
            set
            {
                _unkf0 = value;
                WriteFloatAt(OFFS.UNKF0, value);
            }
        }
        public float Unkf4
        {
            get => _unkf4;
            set
            {
                _unkf4 = value;
                WriteFloatAt(OFFS.UNKF4, value);
            }
        }
        public float Unkf8
        {
            get => _unkf8;
            set
            {
                _unkf8 = value;
                WriteFloatAt(OFFS.UNKF8, value);
            }
        }
        public float Unkfc
        {
            get => _unkfc;
            set
            {
                _unkfc = value;
                WriteFloatAt(OFFS.UNKFC, value);
            }
        }
        public int Unk100
        {
            get => _unk100;
            set
            {
                _unk100 = value;
                WriteIntAt(OFFS.UNK100, value);
            }
        }
        public float Unk104
        {
            get => _unk104;
            set
            {
                _unk104 = value;
                WriteFloatAt(OFFS.UNK104, value);
            }
        }
        public float Unk108
        {
            get => _unk108;
            set
            {
                _unk108 = value;
                WriteFloatAt(OFFS.UNK108, value);
            }
        }
        public float CastSpeed
        {
            get => _castSpeed;
            set
            {
                _castSpeed = value;
                WriteFloatAt(OFFS.CASTSPEED, value);
            }
        }
        public float Unk110
        {
            get => _unk110;
            set
            {
                _unk110 = value;
                WriteFloatAt(OFFS.UNK110, value);
            }
        }
        public float Unk114
        {
            get => _unk114;
            set
            {
                _unk114 = value;
                WriteFloatAt(OFFS.UNK114, value);
            }
        }
        public float Unk118
        {
            get => _unk118;
            set
            {
                _unk118 = value;
                WriteFloatAt(OFFS.UNK118, value);
            }
        }
        public float Unk11c
        {
            get => _unk11c;
            set
            {
                _unk11c = value;
                WriteFloatAt(OFFS.UNK11C, value);
            }
        }
        public float Unk120
        {
            get => _unk120;
            set
            {
                _unk120 = value;
                WriteFloatAt(OFFS.UNK120, value);
            }
        }
        public float Unk124
        {
            get => _unk124;
            set
            {
                _unk124 = value;
                WriteFloatAt(OFFS.UNK124, value);
            }
        }
        public float Unk128
        {
            get => _unk128;
            set
            {
                _unk128 = value;
                WriteFloatAt(OFFS.UNK128, value);
            }
        }
        public float Unk12c
        {
            get => _unk12c;
            set
            {
                _unk12c = value;
                WriteFloatAt(OFFS.UNK12C, value);
            }
        }
        public int MenuCategoryParamId
        {
            get => _menuCategoryParamId;
            set
            {
                _menuCategoryParamId = value;
                WriteIntAt(OFFS.MENUCATPARAMID, value);
            }
        }
        public int Unk134
        {
            get => _unk134;
            set
            {
                _unk134 = value;
                WriteIntAt(OFFS.UNK134, value);
            }
        }
        public byte Unk138
        {
            get => _unk138;
            set
            {
                _unk138 = value;
                WriteByteAt(OFFS.UNK138, value);
            }
        }
        public byte Unk139
        {
            get => _unk139;
            set
            {
                _unk139 = value;
                WriteByteAt(OFFS.UNK139, value);
            }
        }
        public byte MundaneScalingMult
        {
            get => _mundaneScalingMult;
            set
            {
                _mundaneScalingMult = value;
                WriteByteAt(OFFS.MUNDANESCALINGMULT, value);
            }
        }
        public byte Unk13b
        {
            get => _unk13b;
            set
            {
                _unk13b = value;
                WriteByteAt(OFFS.UNK13B, value);
            }
        }

        // Constructor:
        public WeaponTypeRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            _isCastSorceries = ReadByteAt(OFFS.ISCASTSORCERIES);
            _isCastMiracles = ReadByteAt(OFFS.ISCASTMIRACLES);
            _isCastPyromancies = ReadByteAt(OFFS.ISCASTPYROMANCIES);
            _isCastHexesStaves = ReadByteAt(OFFS.ISCASTHEXESSTAVES);
            _unk00 = ReadIntAt(OFFS.UNK00);
            _unk08 = ReadIntAt(OFFS.UNK08);
            _gripStyle_2h_right = ReadIntAt(OFFS.GRIPSTYLE_2H_RIGHT);
            _gripStyle_2h_left = ReadIntAt(OFFS.GRIPSTYLE_2H_LEFT);
            _gripStyle_1h_right = ReadIntAt(OFFS.GRIPSTYLE_1H_RIGHT);
            _gripStyle_1h_left = ReadIntAt(OFFS.GRIPSTYLE_1H_LEFT);
            _gripStyle_powerstance_right = ReadIntAt(OFFS.GRIPSTYLE_POWERSTANCE_RIGHT);
            _gripStyle_powerstance_left = ReadIntAt(OFFS.GRIPSTYLE_POWERSTANCE_LEFT);
            _powerstance_swap_type = ReadByteAt(OFFS.POWERSTANCE_SWAP_TYPE);
            _position_1h_right = ReadByteAt(OFFS.POSITION_1H_RIGHT);
            _relative_position_1h_right = ReadByteAt(OFFS.RELATIVE_POSITION_1H_RIGHT);
            _position_1h_left = ReadByteAt(OFFS.POSITION_1H_LEFT);
            _relative_position_1h_left = ReadByteAt(OFFS.RELATIVE_POSITION_1H_LEFT);
            _position_2h_right = ReadByteAt(OFFS.POSITION_2H_RIGHT);
            _relative_position_2h_right = ReadByteAt(OFFS.RELATIVE_POSITION_2H_RIGHT);
            _position_unknown1 = ReadByteAt(OFFS.POSITION_UNKNOWN1);
            _relative_position_unknown1 = ReadByteAt(OFFS.RELATIVE_POSITION_UNKNOWN1);
            _position_2h_left = ReadByteAt(OFFS.POSITION_2H_LEFT);
            _relative_position_2h_left = ReadByteAt(OFFS.RELATIVE_POSITION_2H_LEFT);
            _position_unknown2 = ReadByteAt(OFFS.POSITION_UNKNOWN2);
            _relative_position_unknown2 = ReadByteAt(OFFS.RELATIVE_POSITION_UNKNOWN2);
            _holsterPosition_right = ReadByteAt(OFFS.HOLSTERPOSITION_RIGHT);
            _holsterrelative_position_right = ReadByteAt(OFFS.HOLSTERRELATIVE_POSITION_RIGHT);
            _holsterPosition_left = ReadByteAt(OFFS.HOLSTERPOSITION_LEFT);
            _holsterrelative_position_left = ReadByteAt(OFFS.HOLSTERRELATIVE_POSITION_LEFT);
            _castType = ReadByteAt(OFFS.CASTTYPE);
            _weaponCategoryName = ReadByteAt(OFFS.WEAPONCATEGORYNAME);
            _swingSound = ReadByteAt(OFFS.SWINGSOUND);
            _powerstanceCategory = ReadIntAt(OFFS.POWERSTANCECATEGORY);
            _unk3c = ReadFloatAt(OFFS.UNK3C);
            _unk40 = ReadFloatAt(OFFS.UNK40);
            _lMod = ReadFloatAt(OFFS.LMOD);
            _unk48 = ReadFloatAt(OFFS.UNK48);
            _unk4c = ReadFloatAt(OFFS.UNK4C);
            _unk50 = ReadByteAt(OFFS.UNK50);
            _unk51 = ReadByteAt(OFFS.UNK51);
            _unk52 = ReadByteAt(OFFS.UNK52);
            _isCastHexesChime = ReadByteAt(OFFS.ISCASTHEXESCHIME);
            _lMod = ReadFloatAt(OFFS.LMOD);
            _unk58 = ReadFloatAt(OFFS.UNK58);
            _leftHandStaminaDamageMult = ReadFloatAt(OFFS.LEFTHANDSTAMINADAMAGEMULT);
            _rMod = ReadFloatAt(OFFS.RMOD);
            _unk13 = ReadFloatAt(OFFS.UNK13);
            _rightHandStaminaDamageMult = ReadFloatAt(OFFS.RIGHTHANDSTAMINADAMAGEMULT);
            _counterDamage = ReadFloatAt(OFFS.COUNTERDAMAGE);
            _unk70 = ReadFloatAt(OFFS.UNK70);
            _unk74 = ReadFloatAt(OFFS.UNK74);
            _unk78 = ReadFloatAt(OFFS.UNK78);
            _unk7c = ReadFloatAt(OFFS.UNK7C);
            _unk80 = ReadFloatAt(OFFS.UNK80);
            _unk84 = ReadFloatAt(OFFS.UNK84);
            _unk88 = ReadFloatAt(OFFS.UNK88);
            _unk8c = ReadFloatAt(OFFS.UNK8C);
            _unk90 = ReadFloatAt(OFFS.UNK90);
            _unk94 = ReadFloatAt(OFFS.UNK94);
            _unk98 = ReadFloatAt(OFFS.UNK98);
            _unk9c = ReadFloatAt(OFFS.UNK9C);
            _unka0 = ReadByteAt(OFFS.UNKA0);
            _unka1 = ReadByteAt(OFFS.UNKA1);
            _bowDistance = ReadShortAt(OFFS.BOWDISTANCE);
            _backstabCheckAnim = ReadIntAt(OFFS.BACKSTABCHECKANIM);
            _guardbreakCheckAnim = ReadIntAt(OFFS.GUARDBREAKCHECKANIM);
            _unkac = ReadFloatAt(OFFS.UNKAC);
            _unkb0 = ReadFloatAt(OFFS.UNKB0);
            _unkb4 = ReadFloatAt(OFFS.UNKB4);
            _unkb8 = ReadFloatAt(OFFS.UNKB8);
            _unkbc = ReadFloatAt(OFFS.UNKBC);
            _unkc0 = ReadFloatAt(OFFS.UNKC0);
            _unkc4 = ReadFloatAt(OFFS.UNKC4);
            _unkc8 = ReadFloatAt(OFFS.UNKC8);
            _guardStaminaRegen = ReadFloatAt(OFFS.GUARDSTAMINAREGEN);
            _unkd0 = ReadFloatAt(OFFS.UNKD0);
            _unkd4 = ReadFloatAt(OFFS.UNKD4);
            _unkd8 = ReadFloatAt(OFFS.UNKD8);
            _unkdc = ReadFloatAt(OFFS.UNKDC);
            _unke0 = ReadFloatAt(OFFS.UNKE0);
            _unke4 = ReadFloatAt(OFFS.UNKE4);
            _unke8 = ReadFloatAt(OFFS.UNKE8);
            _unkec = ReadFloatAt(OFFS.UNKEC);
            _unkf0 = ReadFloatAt(OFFS.UNKF0);
            _unkf4 = ReadFloatAt(OFFS.UNKF4);
            _unkf8 = ReadFloatAt(OFFS.UNKF8);
            _unkfc = ReadFloatAt(OFFS.UNKFC);
            _unk100 = ReadIntAt(OFFS.UNK100);
            _unk104 = ReadFloatAt(OFFS.UNK104);
            _unk108 = ReadFloatAt(OFFS.UNK108);
            _castSpeed = ReadFloatAt(OFFS.CASTSPEED);
            _unk110 = ReadFloatAt(OFFS.UNK110);
            _unk114 = ReadFloatAt(OFFS.UNK114);
            _unk118 = ReadFloatAt(OFFS.UNK118);
            _unk11c = ReadFloatAt(OFFS.UNK11C);
            _unk120 = ReadFloatAt(OFFS.UNK120);
            _unk124 = ReadFloatAt(OFFS.UNK124);
            _unk128 = ReadFloatAt(OFFS.UNK128);
            _unk12c = ReadFloatAt(OFFS.UNK12C);
            _menuCategoryParamId = ReadIntAt(OFFS.MENUCATPARAMID);
            _unk134 = ReadIntAt(OFFS.UNK134);
            _unk138 = ReadByteAt(OFFS.UNK138);
            _unk139 = ReadByteAt(OFFS.UNK139);
            _mundaneScalingMult = ReadByteAt(OFFS.MUNDANESCALINGMULT);
            _unk13b = ReadByteAt(OFFS.UNK13B);
        }
    }
}
