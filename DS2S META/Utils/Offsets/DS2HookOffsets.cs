using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    /// <summary>
    /// This is the master list of META functionality interfaces with 
    /// DS2. Is it subclassed by all versions to override the offsets
    /// as appropriate
    /// </summary>
    public abstract class DS2HookOffsets
    {
        // Misc/Global
        public const int UNSET = -1;

        // BaseA
        public const string BaseAAob = "48 8B 05 ? ? ? ? 48 8B 58 38 48 85 DB 74 ? F6";
        public const string BaseABabyJumpAoB = "49 BA ? ? ? ? ? ? ? ? 41 FF E2 90 74 2E";
        public const int BasePtrOffset1 = 0x3;
        public const int BasePtrOffset2 = 0x7;

        // Section 1:
        public PlayerCtrlOffsets PlayerCtrl { get; init; }
        public PlayerName PlayerName { get; init; }
        public ForceQuit ForceQuit { get; init; }
        public PlayerBaseMisc PlayerBaseMisc { get; init; }
        public PlayerEquipment PlayerEquipment { get; init; }
        public PlayerParam PlayerParam { get; init; }
        public Attributes Attributes { get; init; }
        public Covenants Covenants { get; init; }
        public Gravity Gravity { get; init; }
        public PlayerMapData PlayerMapData { get; init; }
        public Bonfire Bonfire { get; init; }
        public BonfireLevels BonfireLevels { get; init; }
        public Connection Connection { get; init; }
        public Camera Camera { get; init; }

        public PlayerType PlayerType = new(0x3C, 0x3D, 0x48);
        public NetSvrBloodstainManager NetSvrBloodstainManager = new(0x38, 0x3C, 0x40);
        public PlayerPosition PlayerPosition = new(0x20, 0x24, 0x28, 0x34, 0x38, 0x3C);

        // Core struct offsets:
        public const int PlayerTypeOffset = 0xB0;
        public const int PlayerNameOffset = 0xA8;
        public const int AvailableItemBagOffset = 0x10;
        public const int ItemGiveWindowPointer = 0x22E0;
        public const int PlayerBaseMiscOffset = 0xC0;
        public const int PlayerCtrlOffset = 0xD0;
        public const int NetSvrBloodstainManagerOffset1 = 0x90;
        public const int NetSvrBloodstainManagerOffset2 = 0x28;
        public const int NetSvrBloodstainManagerOffset3 = 0x88;
        public const int PlayerParamOffset = 0x490;
        public const int PlayerPositionOffset1 = 0xF8;
        public const int PlayerPositionOffset2 = 0xF0;
        public const int PlayerMapDataOffset1 = 0x100;
        public const int PlayerMapDataOffset2 = 0x320;
        public const int PlayerMapDataOffset3 = 0x20;
        public const int SpEffectCtrlOffset = 0x3E0;
        public const int CharacterFlagsOffset = 0x490;
        public const int EventManagerOffset = 0x70;
        public const int WarpManagerOffset = 0x70;
        public const int BonfireLevelsOffset1 = 0x58;
        public const int BonfireLevelsOffset2 = 0x20;
        public const int ConnectionOffset = 0x38;
        public const int CameraOffset1 = 0x0;
        public const int CameraOffset2 = 0x20;
        public const int CameraOffset3 = 0x28;


        // Functions TODO:
        public const string ItemGiveFunc = "48 89 5C 24 18 56 57 41 56 48 83 EC 30 45 8B F1 41";
        public const string ItemStruct2dDisplay = "40 53 48 83 EC 20 45 33 D2 45 8B D8 48 8B D9 44 89 11";
        public const string GiveSoulsFuncAoB = "48 83 ec 28 48 8b 01 48 85 c0 74 23 48 8b 80 b8 00 00 00";
        public const string SetWarpTargetFuncAoB = "48 89 5C 24 08 48 89 74 24 20 57 48 83 EC 60 0F B7 FA";
        public const string WarpFuncAoB = "40 53 48 83 EC 60 8B 02 48 8B D9 89 01 8B 42 04";
        public const string BaseBAoB = "48 8B 0D ? ? ? ? 48 85 C9 74 ? 48 8B 49 18 E8";
        public const string CameraAoB = "60 02 2c f0 f3 7f 00 00";
        public const string SpeedFactorAccelOffset = "F3 0F 59 9F A8 02 00 00 F3 0F 10 16";
        public const string SpeedFactorAnimOffset = "F3 0F 59 99 A8 02 00 00";
        public const string SpeedFactorJumpOffset = "F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E0 00 00 00";
        public const string SpeedFactorBuildupOffset = "F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E8 03 00 00";

        // ---------------------------------------------------
        // ---------------------------------------------------
        // ---------------------------------------------------
        // ---------------------------------------------------

        // TODO (abstract):
        public string? DisplayItem;
        public int[]? PlayerStatsOffsets;
        public string? ApplySpEffectAoB;


        //#region Param

        //public enum Param
        //{
        //    TotalParamLength = 0x0,
        //    ParamName = 0xC,
        //    OffsetsOnlyTableLength = 0x48,
        //    TableLength = 0x50 // total incl. offsets/rows parts (excls. strings)
        //}

        //public const int ParamDataOffset1 = 0x18;
        //public const int ParamDataOffset2 = 0xD8;
        //public const int ParamDataOffset3 = 0xA8;
        //public const int ParamDataOffset4 = 0x50;
        //public const int ParamDataOffset5 = 0xB0;

        //public const int ParamItemLotOtherOffset = 0x1F8;

        //public const int LevelUpSoulsParamOffset = 0x580;
        //public const int WeaponParamOffset = 0x420;
        //public enum WeaponParam
        //{
        //    ReinforceID = 0x8
        //}

        //public const int WeaponReinforceParamOffset = 0x470;
        //public enum WeaponReinforceParam
        //{
        //    MaxUpgrade = 0x48,
        //    CustomAttrID = 0xE8
        //}
        //public const int CustomAttrSpecParamOffset = 0x4F0;

        //public const int ArmorParamOffset = 0x4A0;
        //public enum ArmorParam
        //{
        //    ReinforceID = 0x8
        //}
        //public const int ArmorReinforceParamOffset = 0x4B0;
        //public enum ArmorReinforceParam
        //{
        //    MaxUpgrade = 0x60,
        //}

        //public const int ItemParamOffset = 0x20;
        //public enum ItemParam
        //{
        //    ItemUsageID = 0x44,
        //    MaxHeld = 0x4A,
        //    BaseBuyPrice = 0x30,
        //    ItemType = 0x4F,
        //}

        //public const int ItemUsageParamOffset1 = 0x40;
        //public const int ItemUsageParamOffset2 = 0xD8;
        //public enum ItemUasgeParam
        //{
        //    Bitfield = 0x6
        //}

        //public enum ItemLotOffsets : int
        //{
        //    // The 1 is the first of 10 slots for each of the following
        //    Item1 = 0x2C,
        //    Quantity1 = 0x4,
        //    Reinforcement1 = 0xE,
        //    Infusion1 = 0x18,
        //    Unk3_1 = 0x22, // TODO: investigate (related to item one-per-playthrough)
        //    Chance1 = 0x54
        //}
        //public enum ShopLotOffsets : int
        //{
        //    ItemID = 0x00,
        //    Unk04 = 0x04,
        //    EnableFlag = 0x08,
        //    DisableFlag = 0x0C,
        //    MaterialID = 0x10,
        //    DuplicateItemID = 0x14,
        //    Unk18 = 0x18,
        //    PriceRate = 0x1C,
        //    Quantity = 0x20,
        //}



    }
}
