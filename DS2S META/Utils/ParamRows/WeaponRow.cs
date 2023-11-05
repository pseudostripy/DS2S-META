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
    public class WeaponRow : Param.Row
    {
        // Fields/Properties/Enums auto-transcribed from Python
        private enum OFFS
        {
            WEAPONMOTIONID = 0x0,
            WEAPONMODELID = 0x4,
            WEAPONREINFORCEID = 0x8,
            ACTIONCATEGORYID = 0xc,
            WEAPONTYPEID = 0x10,
            UNK14 = 0x14,
            STRREQ = 0x18,
            DEXREQ = 0x1a,
            INTREQ = 0x1c,
            FTHREQ = 0x1e,
            WEIGHT = 0x20,
            RECOVANIMATIONWEIGHT = 0x24,
            MAXDURABILITY = 0x28,
            BASEREPAIRCOST = 0x2c,
            UNK30 = 0x30,
            STAMINACONSUMPTION = 0x34,
            STAMINADAMAGE = 0x38,
            STAMINACOSTID = 0x3c,
            UNK40 = 0x40,
            UNK44 = 0x44,
            UNK48 = 0x48,
            UNK4C = 0x4c,
            PLAYERDMGCRITID1 = 0x50,
            PLAYERDMGCRITID2 = 0x54,
            PLAYERDMGCRITID3 = 0x58,
            PLAYERDMGCRITID4 = 0x5c,
            PLAYERDMGCRITID5 = 0x60,
            PLAYERDMGCRITID6 = 0x64,
            PLAYERDMGCRITID7 = 0x68,
            PLAYERDMGCRITID8 = 0x6c,
            PLAYERDMGCRITID9 = 0x70,
            UNK74 = 0x74,
            UNK78 = 0x78,
            UNK7C = 0x7c,
            DAMAGEMULTIPLIER = 0x80,
            EQUIPDMGMULT = 0x84,
            GUARDBREAKRELATED1 = 0x88,
            GUARDBREAKRELATED2 = 0x8c,
            STATUSEFFECTAMOUNT = 0x90,
            UNK94 = 0x94,
            HITBOXSIZE1 = 0x98,
            HITBOXSIZE2 = 0x9c,
            HITBOXWALLCANCEL1 = 0xa0,
            HITBOXWALLCANCEL2 = 0xa4,
            DAMAGETYPE = 0xa8,
            POISEDAMAGE = 0xaa,
            COUNTERDAMAGE = 0xac,
            CASTINGSPEED = 0xae,
            UNKB0 = 0xb0,
            POISEDMGFLOAT = 0xb4,
            UNKB8 = 0xb8,
        }

        // Behind-fields
        private int _weaponMotionId;
        private int _weaponModelId;
        private int _weaponReinforceId;
        private int _actionCategoryId;
        private int _weaponTypeId;
        private int _unk14;
        private short _strReq;
        private short _dexReq;
        private short _intReq;
        private short _fthReq;
        private float _weight;
        private float _recovAnimationWeight;
        private float _maxDurability;
        private int _baseRepairCost;
        private int _unk30;
        private float _staminaConsumption;
        private float _staminaDamage;
        private int _staminaCostId;
        private float _unk40;
        private float _unk44;
        private float _unk48;
        private float _unk4c;
        private int _playerDmgCritId1;
        private int _playerDmgCritId2;
        private int _playerDmgCritId3;
        private int _playerDmgCritId4;
        private int _playerDmgCritId5;
        private int _playerDmgCritId6;
        private int _playerDmgCritId7;
        private int _playerDmgCritId8;
        private int _playerDmgCritId9;
        private int _unk74;
        private int _unk78;
        private int _unk7c;
        private float _damageMultiplier;
        private float _equipDmgMult;
        private float _guardBreakRelated1;
        private float _guardBreakRelated2;
        private float _statusEffectAmount;
        private float _unk94;
        private float _hitboxSize1;
        private float _hitboxSize2;
        private float _hitboxWallCancel1;
        private float _hitboxWallCancel2;
        private short _damageType;
        private short _poiseDamage;
        private short _counterDamage;
        private short _castingSpeed;
        private float _unkb0;
        private float _poiseDmgFloat;
        private float _unkb8;

        // Properties:
        public int WeaponMotionId
        {
            get => _weaponMotionId;
            set
            {
                _weaponMotionId = value;
                WriteIntAt(OFFS.WEAPONMOTIONID, value);
            }
        }
        public int WeaponModelId
        {
            get => _weaponModelId;
            set
            {
                _weaponModelId = value;
                WriteIntAt(OFFS.WEAPONMODELID, value);
            }
        }
        public int WeaponReinforceId
        {
            get => _weaponReinforceId;
            set
            {
                _weaponReinforceId = value;
                WriteIntAt(OFFS.WEAPONREINFORCEID, value);
            }
        }
        public int ActionCategoryId
        {
            get => _actionCategoryId;
            set
            {
                _actionCategoryId = value;
                WriteIntAt(OFFS.ACTIONCATEGORYID, value);
            }
        }
        public int WeaponTypeId
        {
            get => _weaponTypeId;
            set
            {
                _weaponTypeId = value;
                WriteIntAt(OFFS.WEAPONTYPEID, value);
            }
        }
        public int Unk14
        {
            get => _unk14;
            set
            {
                _unk14 = value;
                WriteIntAt(OFFS.UNK14, value);
            }
        }
        public short StrReq
        {
            get => _strReq;
            set
            {
                _strReq = value;
                WriteShortAt(OFFS.STRREQ, value);
            }
        }
        public short DexReq
        {
            get => _dexReq;
            set
            {
                _dexReq = value;
                WriteShortAt(OFFS.DEXREQ, value);
            }
        }
        public short IntReq
        {
            get => _intReq;
            set
            {
                _intReq = value;
                WriteShortAt(OFFS.INTREQ, value);
            }
        }
        public short FthReq
        {
            get => _fthReq;
            set
            {
                _fthReq = value;
                WriteShortAt(OFFS.FTHREQ, value);
            }
        }
        public float Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                WriteFloatAt(OFFS.WEIGHT, value);
            }
        }
        public float RecovAnimationWeight
        {
            get => _recovAnimationWeight;
            set
            {
                _recovAnimationWeight = value;
                WriteFloatAt(OFFS.RECOVANIMATIONWEIGHT, value);
            }
        }
        public float MaxDurability
        {
            get => _maxDurability;
            set
            {
                _maxDurability = value;
                WriteFloatAt(OFFS.MAXDURABILITY, value);
            }
        }
        public int BaseRepairCost
        {
            get => _baseRepairCost;
            set
            {
                _baseRepairCost = value;
                WriteIntAt(OFFS.BASEREPAIRCOST, value);
            }
        }
        public int Unk30
        {
            get => _unk30;
            set
            {
                _unk30 = value;
                WriteIntAt(OFFS.UNK30, value);
            }
        }
        public float StaminaConsumption
        {
            get => _staminaConsumption;
            set
            {
                _staminaConsumption = value;
                WriteFloatAt(OFFS.STAMINACONSUMPTION, value);
            }
        }
        public float StaminaDamage
        {
            get => _staminaDamage;
            set
            {
                _staminaDamage = value;
                WriteFloatAt(OFFS.STAMINADAMAGE, value);
            }
        }
        public int StaminaCostId
        {
            get => _staminaCostId;
            set
            {
                _staminaCostId = value;
                WriteIntAt(OFFS.STAMINACOSTID, value);
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
        public int PlayerDmgCritId1
        {
            get => _playerDmgCritId1;
            set
            {
                _playerDmgCritId1 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID1, value);
            }
        }
        public int PlayerDmgCritId2
        {
            get => _playerDmgCritId2;
            set
            {
                _playerDmgCritId2 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID2, value);
            }
        }
        public int PlayerDmgCritId3
        {
            get => _playerDmgCritId3;
            set
            {
                _playerDmgCritId3 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID3, value);
            }
        }
        public int PlayerDmgCritId4
        {
            get => _playerDmgCritId4;
            set
            {
                _playerDmgCritId4 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID4, value);
            }
        }
        public int PlayerDmgCritId5
        {
            get => _playerDmgCritId5;
            set
            {
                _playerDmgCritId5 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID5, value);
            }
        }
        public int PlayerDmgCritId6
        {
            get => _playerDmgCritId6;
            set
            {
                _playerDmgCritId6 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID6, value);
            }
        }
        public int PlayerDmgCritId7
        {
            get => _playerDmgCritId7;
            set
            {
                _playerDmgCritId7 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID7, value);
            }
        }
        public int PlayerDmgCritId8
        {
            get => _playerDmgCritId8;
            set
            {
                _playerDmgCritId8 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID8, value);
            }
        }
        public int PlayerDmgCritId9
        {
            get => _playerDmgCritId9;
            set
            {
                _playerDmgCritId9 = value;
                WriteIntAt(OFFS.PLAYERDMGCRITID9, value);
            }
        }
        public int Unk74
        {
            get => _unk74;
            set
            {
                _unk74 = value;
                WriteIntAt(OFFS.UNK74, value);
            }
        }
        public int Unk78
        {
            get => _unk78;
            set
            {
                _unk78 = value;
                WriteIntAt(OFFS.UNK78, value);
            }
        }
        public int Unk7c
        {
            get => _unk7c;
            set
            {
                _unk7c = value;
                WriteIntAt(OFFS.UNK7C, value);
            }
        }
        public float DamageMultiplier
        {
            get => _damageMultiplier;
            set
            {
                _damageMultiplier = value;
                WriteFloatAt(OFFS.DAMAGEMULTIPLIER, value);
            }
        }
        public float EquipDmgMult
        {
            get => _equipDmgMult;
            set
            {
                _equipDmgMult = value;
                WriteFloatAt(OFFS.EQUIPDMGMULT, value);
            }
        }
        public float GuardBreakRelated1
        {
            get => _guardBreakRelated1;
            set
            {
                _guardBreakRelated1 = value;
                WriteFloatAt(OFFS.GUARDBREAKRELATED1, value);
            }
        }
        public float GuardBreakRelated2
        {
            get => _guardBreakRelated2;
            set
            {
                _guardBreakRelated2 = value;
                WriteFloatAt(OFFS.GUARDBREAKRELATED2, value);
            }
        }
        public float StatusEffectAmount
        {
            get => _statusEffectAmount;
            set
            {
                _statusEffectAmount = value;
                WriteFloatAt(OFFS.STATUSEFFECTAMOUNT, value);
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
        public float HitboxSize1
        {
            get => _hitboxSize1;
            set
            {
                _hitboxSize1 = value;
                WriteFloatAt(OFFS.HITBOXSIZE1, value);
            }
        }
        public float HitboxSize2
        {
            get => _hitboxSize2;
            set
            {
                _hitboxSize2 = value;
                WriteFloatAt(OFFS.HITBOXSIZE2, value);
            }
        }
        public float HitboxWallCancel1
        {
            get => _hitboxWallCancel1;
            set
            {
                _hitboxWallCancel1 = value;
                WriteFloatAt(OFFS.HITBOXWALLCANCEL1, value);
            }
        }
        public float HitboxWallCancel2
        {
            get => _hitboxWallCancel2;
            set
            {
                _hitboxWallCancel2 = value;
                WriteFloatAt(OFFS.HITBOXWALLCANCEL2, value);
            }
        }
        public short DamageType
        {
            get => _damageType;
            set
            {
                _damageType = value;
                WriteShortAt(OFFS.DAMAGETYPE, value);
            }
        }
        public short PoiseDamage
        {
            get => _poiseDamage;
            set
            {
                _poiseDamage = value;
                WriteShortAt(OFFS.POISEDAMAGE, value);
            }
        }
        public short CounterDamage
        {
            get => _counterDamage;
            set
            {
                _counterDamage = value;
                WriteShortAt(OFFS.COUNTERDAMAGE, value);
            }
        }
        public short CastingSpeed
        {
            get => _castingSpeed;
            set
            {
                _castingSpeed = value;
                WriteShortAt(OFFS.CASTINGSPEED, value);
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
        public float PoiseDmgFloat
        {
            get => _poiseDmgFloat;
            set
            {
                _poiseDmgFloat = value;
                WriteFloatAt(OFFS.POISEDMGFLOAT, value);
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

        // Linked param:
        internal WeaponReinforceRow? ReinforceRow => ParamMan.GetLink<WeaponReinforceRow>(ParamMan.WeaponReinforceParam, WeaponReinforceId);
        internal WeaponTypeRow? WTypeRow => ParamMan.GetLink<WeaponTypeRow>(ParamMan.WeaponTypeParam, WeaponTypeId);

        // Wrappers
        internal int MaxUpgrade => ReinforceRow == null ? 0 : ReinforceRow.MaxReinforce;
        public List<DS2SInfusion> GetInfusionList()
        {
            if (ReinforceRow == null)
                return new List<DS2SInfusion>() { DS2SInfusion.Infusions[0] };
            return ReinforceRow.GetInfusionList();
        }
            

        // Constructor:
        public WeaponRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            _weaponMotionId = ReadIntAt(OFFS.WEAPONMOTIONID);
            _weaponModelId = ReadIntAt(OFFS.WEAPONMODELID);
            _weaponReinforceId = ReadIntAt(OFFS.WEAPONREINFORCEID);
            _actionCategoryId = ReadIntAt(OFFS.ACTIONCATEGORYID);
            _weaponTypeId = ReadIntAt(OFFS.WEAPONTYPEID);
            _unk14 = ReadIntAt(OFFS.UNK14);
            _strReq = ReadShortAt(OFFS.STRREQ);
            _dexReq = ReadShortAt(OFFS.DEXREQ);
            _intReq = ReadShortAt(OFFS.INTREQ);
            _fthReq = ReadShortAt(OFFS.FTHREQ);
            _weight = ReadFloatAt(OFFS.WEIGHT);
            _recovAnimationWeight = ReadFloatAt(OFFS.RECOVANIMATIONWEIGHT);
            _maxDurability = ReadFloatAt(OFFS.MAXDURABILITY);
            _baseRepairCost = ReadIntAt(OFFS.BASEREPAIRCOST);
            _unk30 = ReadIntAt(OFFS.UNK30);
            _staminaConsumption = ReadFloatAt(OFFS.STAMINACONSUMPTION);
            _staminaDamage = ReadFloatAt(OFFS.STAMINADAMAGE);
            _staminaCostId = ReadIntAt(OFFS.STAMINACOSTID);
            _unk40 = ReadFloatAt(OFFS.UNK40);
            _unk44 = ReadFloatAt(OFFS.UNK44);
            _unk48 = ReadFloatAt(OFFS.UNK48);
            _unk4c = ReadFloatAt(OFFS.UNK4C);
            _playerDmgCritId1 = ReadIntAt(OFFS.PLAYERDMGCRITID1);
            _playerDmgCritId2 = ReadIntAt(OFFS.PLAYERDMGCRITID2);
            _playerDmgCritId3 = ReadIntAt(OFFS.PLAYERDMGCRITID3);
            _playerDmgCritId4 = ReadIntAt(OFFS.PLAYERDMGCRITID4);
            _playerDmgCritId5 = ReadIntAt(OFFS.PLAYERDMGCRITID5);
            _playerDmgCritId6 = ReadIntAt(OFFS.PLAYERDMGCRITID6);
            _playerDmgCritId7 = ReadIntAt(OFFS.PLAYERDMGCRITID7);
            _playerDmgCritId8 = ReadIntAt(OFFS.PLAYERDMGCRITID8);
            _playerDmgCritId9 = ReadIntAt(OFFS.PLAYERDMGCRITID9);
            _unk74 = ReadIntAt(OFFS.UNK74);
            _unk78 = ReadIntAt(OFFS.UNK78);
            _unk7c = ReadIntAt(OFFS.UNK7C);
            _damageMultiplier = ReadFloatAt(OFFS.DAMAGEMULTIPLIER);
            _equipDmgMult = ReadFloatAt(OFFS.EQUIPDMGMULT);
            _guardBreakRelated1 = ReadFloatAt(OFFS.GUARDBREAKRELATED1);
            _guardBreakRelated2 = ReadFloatAt(OFFS.GUARDBREAKRELATED2);
            _statusEffectAmount = ReadFloatAt(OFFS.STATUSEFFECTAMOUNT);
            _unk94 = ReadFloatAt(OFFS.UNK94);
            _hitboxSize1 = ReadFloatAt(OFFS.HITBOXSIZE1);
            _hitboxSize2 = ReadFloatAt(OFFS.HITBOXSIZE2);
            _hitboxWallCancel1 = ReadFloatAt(OFFS.HITBOXWALLCANCEL1);
            _hitboxWallCancel2 = ReadFloatAt(OFFS.HITBOXWALLCANCEL2);
            _damageType = ReadShortAt(OFFS.DAMAGETYPE);
            _poiseDamage = ReadShortAt(OFFS.POISEDAMAGE);
            _counterDamage = ReadShortAt(OFFS.COUNTERDAMAGE);
            _castingSpeed = ReadShortAt(OFFS.CASTINGSPEED);
            _unkb0 = ReadFloatAt(OFFS.UNKB0);
            _poiseDmgFloat = ReadFloatAt(OFFS.POISEDMGFLOAT);
            _unkb8 = ReadFloatAt(OFFS.UNKB8);
        }
    }
}
