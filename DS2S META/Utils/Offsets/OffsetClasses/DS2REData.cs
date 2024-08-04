using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS2S_META.Utils;
using DS2S_META.Utils.Offsets.OffsetClasses;
using DS2S_META.Utils.Offsets.CodeLocators;
using DS2S_META.Utils.DS2Hook;

namespace DS2S_META.Utils.Offsets
{
    public static class DS2REData
    {
        // Version shorthands
        private readonly static List<DS2VER> ANYVER = new() { DS2VER.SOTFS_V103, DS2VER.SOTFS_V102, DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        private readonly static List<DS2VER> ANYSOTFS = new() { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 };
        private readonly static List<DS2VER> ANYVANILLA = new() { DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        private readonly static List<DS2VER> ALLBUTOLDPATCH = new() { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112};
        private readonly static List<DS2VER> V102 = new() { DS2VER.VANILLA_V102 };
        private readonly static List<DS2VER> V111 = new() { DS2VER.VANILLA_V111 };
        private readonly static List<DS2VER> V112 = new() { DS2VER.VANILLA_V112 };
        private readonly static List<DS2VER> S102 = new() { DS2VER.SOTFS_V102 };
        private readonly static List<DS2VER> S103 = new() { DS2VER.SOTFS_V103 };
        private readonly static List<DS2VER> V111OR112 = new() { DS2VER.VANILLA_V111, DS2VER.VANILLA_V111 };
        private readonly static string STRBASEA = "BaseA";

        public static LocatorDefn OFLD(List<DS2VER> validVersions, string parentId, params int[] offsets)
        {
            return new LocatorDefn(validVersions, new LeafCL(parentId, offsets.ToList()));
        }
        public static LocatorDefn CPLD(List<DS2VER> validVersions, string parentId, params int[] offsets)
        {
            return new LocatorDefn(validVersions, new ChildPointerCL(parentId, offsets.ToList()));
        }


        public static readonly List<PointerDefn> PointerDefns = new()
        {
            new(STRBASEA, new List<LocatorDefn>()
            {
                new(ANYSOTFS, new RelInstructionAOBCL("48 8B 05 ? ? ? ? 48 8B 58 38 48 85 DB 74 ? F6",3,7,0)),
                new(ANYVANILLA, new RelModuleAOBCL("8B F1 8B 0D ? ? ? ? 8B 01 8B 50 28 FF D2 84 C0 74 0C",4))
            }),
            new("BaseB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new RelInstructionAOBCL("48 8B 0D ? ? ? ? 48 85 C9 74 ? 48 8B 49 18 E8",3,7,0)),
                new(ANYVANILLA, new RelModuleAOBCL("89 45 98 A1 ? ? ? ? 89 7D 9C 89 BD ? ? ? ? 85 C0",3))
            }),
            new("BaseAOldBBJMod", new List<LocatorDefn>()
            {
                new(S102, new RelInstructionAOBCL("49 BA ? ? ? ? ? ? ? ? 41 FF E2 90 74 2E",3,7,0)),
                new(V111, new RelModuleAOBCL("8b 48 6c 53 8b 58 70 8b 40 68 89 45 0c a1 f4 93 54 01 56",4))
            }),

            new("ItemGiveFuncAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("48 89 5C 24 18 56 57 41 56 48 83 EC 30 45 8B F1 41")),
                new(ANYVANILLA, new AbsoluteAOBCL("55 8B EC 83 EC 10 53 8B 5D 0C 56 8B 75 08 57 53 56 8B F9"))
            }),
            new("ItemStruct2dDisplayAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("40 53 48 83 EC 20 45 33 D2 45 8B D8 48 8B D9 44 89 11")),
                new(V102, new AbsoluteAOBCL("55 8b ec 8b 45 08 8b 4d 14 53 8b 5d 10 56 33 f6")),
                new(V111OR112, new AbsoluteAOBCL("55 8B EC 8B 45 08 0F 57 C0 8B 4D 14 53"))
            }),
            new("GiveSoulsFuncAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("48 83 EC 28 48 8b 01 48 85 C0 74 23 48 8b 80 b8 00 00 00")),
                new(ANYVANILLA, new AbsoluteAOBCL("55 8B EC 8B 01 83 EC 08 85 C0 74 20 8B 80 94 00 00 00"))
            }),
            new("RemoveSoulsFuncAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("44 8b 81 ec 00 00 00 41 3b d0 73 05 44 2b c2 eb 03")),
                new(ANYVANILLA, new AbsoluteAOBCL("55 8b ec 8b 81 e8 00 00 00 8b 55 08 83 ec 08 3b d0 73 04"))
            }),
            new("SetWarpTargetFuncAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("48 89 5C 24 08 48 89 74 24 20 57 48 83 EC 60 0F B7 FA")),
                new(ANYVANILLA, new AbsoluteAOBCL("55 8B EC 83 EC 44 53 56 8B 75 0C 57 56 8D 4D 0C"))
            }),
            new("WarpFuncAoB", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("40 53 48 83 EC 60 8B 02 48 8B D9 89 01 8B 42 04")),
                new(ANYVANILLA, new AbsoluteAOBCL("55 8B EC 83 EC 40 53 56 8B 75 08 8B D9 57 B9 10 00 00 00"))
            }),
            new("DisplayItemFuncAoB", new List<LocatorDefn>()
            {
                new(S102, new AbsoluteAOBCL("48 8B 89 D8 00 00 00 48 85 C9 0F 85 40 5E 00 00")),
                new(S103, new AbsoluteAOBCL("48 8B 89 D8 00 00 00 48 85 C9 0F 85 20 5E 00 00")),
                new(ANYVANILLA, new RelInstructionAOBCL("83 c4 10 8d 95 6c fe ff ff 52 8b ce e8 ? ? ? ?", 13, 17))
            }),
            new("ApplySpEffectAoB", new List<LocatorDefn>()
            {
                new(S102, new AbsoluteAOBCL("E9 ? ? ? ? E9 ? ? ? ? 50 5A 41 51 59")),
                new(S103, new AbsoluteAOBCL("48 89 6C 24 f8 48 8d 64 24 f8 48 8D 2d 33 A7 0A 00")),
                new(V102, new AbsoluteAOBCL("55 8b ec 8b 45 08 83 ec 10 56 8b f1")),
                new(V111, new AbsoluteAOBCL("E9 ? ? ? ? 8B 45 F4 83 C0 01 89 45 F4 E9 ? ? ? ?")),
                new(V112, new AbsoluteAOBCL("89 6c 24 fc 8d 64 24 fc 54 5d 8b 45 08 83 ec 10 89 74 24 fc 8d 64")),
            }),
            new("DisableSkirtDamageAoB", new List<LocatorDefn> ()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("89 84 8B C4 01 00 00")),
            }),

            // Deprecated. Kept as reference.
            new("SpeedFactorAccelOffset", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("F3 0F 59 9F A8 02 00 00 F3 0F 10 16")),
                new(ANYVANILLA, new AbsoluteAOBCL("F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9"))
            }),
            new("SpeedFactorAnimOffset", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("F3 0F 59 99 A8 02 00 00")),
                new(ANYVANILLA, new AbsoluteAOBCL("F3 0F 10 89 08 02 00 00 8B 89 B4 00 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D F4 0F 5A C0 89"))
            }),
            new("SpeedFactorJumpOffset", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E0 00 00 00")),
                new(ANYVANILLA, new AbsoluteAOBCL("F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D F4 0F 5A C0 0F 5A C9 F2 0F 59 C1 66 0F 5A C0 F3 0F 11 45 F4"))
            }),
            new("SpeedFactorBuildupOffset", new List<LocatorDefn>()
            {
                new(ANYSOTFS, new AbsoluteAOBCL("F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E8 03 00 00")),
                new(ANYVANILLA, new AbsoluteAOBCL("F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D EC"))
            }),
        
            // BaseA ChildPointers:    
            new("PlayerTypeOffset", CPLD(ANYSOTFS, STRBASEA, 0xb0),
                                    CPLD(ANYVANILLA, STRBASEA, 0x90)),
            new("AvailableItemBag", CPLD(ANYSOTFS, STRBASEA, 0xa8, 0x10, 0x10),
                                    CPLD(ANYVANILLA, STRBASEA, 0x60, 0x8, 0x8)),
            new("ItemGiveWindowPointer",    CPLD(ANYSOTFS, STRBASEA, 0x22e0),
                                            CPLD(ANYVANILLA, STRBASEA, 0xcc4)),
            new("PlayerCtrl",   CPLD(ANYSOTFS, STRBASEA, 0xd0),
                                CPLD(ANYVANILLA, STRBASEA, 0x74)),
            new("NetSvrBloodstainManager", CPLD(ANYVER, STRBASEA, 0x90, 0x28, 0x88)),
            new("PlayerParam",  CPLD(ANYSOTFS, "PlayerCtrl", 0x490),
                                CPLD(ANYVANILLA, "PlayerCtrl", 0x378)),
            new("PlayerPosition",   CPLD(ANYSOTFS, "PlayerCtrl", 0xf8, 0xf0),
                                    CPLD(ANYVANILLA, "PlayerCtrl", 0xb4, 0xa8)),
            new("PlayerDataMapPtr", CPLD(ANYSOTFS, "PlayerCtrl", 0x100, 0x320, 0x20),
                                    CPLD(ANYVANILLA, "PlayerCtrl", 0xB8, 0x8, 0x14, 0x1B0, 0x10)),
            new("SpEffectCtrl", CPLD(ANYSOTFS, "PlayerCtrl", 0x3e0),
                                CPLD(ANYVANILLA, "PlayerCtrl", 0x308)),
            new("EventManager", CPLD(ANYSOTFS, STRBASEA, 0x70),
                                CPLD(ANYVANILLA, STRBASEA, 0x44)),
            new("WarpManager",  CPLD(ANYSOTFS, "EventManager", 0x70),
                                CPLD(ANYVANILLA, "EventManager", 0x2c)),
            new("BonfireLevelsPtr", CPLD(ANYSOTFS, "EventManager", 0x58, 0x20),
                                    CPLD(ANYVANILLA, "EventManager", 0x2c, 0x10)),
            new("Camera2", CPLD(ANYVER, STRBASEA, 0x0, 0x20)),
            new("PlayerBaseMisc",   CPLD(ANYSOTFS, STRBASEA, 0xa8, 0xc0),
                                    CPLD(ANYVANILLA, STRBASEA, 0x60)),
            new("LoadedEnemiesTable", CPLD(S103, STRBASEA, 0x18)),
            new("ScalingBonusTableCtrl",    CPLD(S102, STRBASEA, 0x20, 0x28, 0x110, 0x70, 0xA0, 0x170, 0x718),
                                            CPLD(S103, STRBASEA, 0x20, 0x28, 0x110, 0x70, 0xA0, 0x170)),
        };


        public static readonly List<LeafDefn> LeafDefns = new()
        {
            new("PlayerName",   OFLD(ANYSOTFS, STRBASEA, 0xa8, 0x114),
                                OFLD(ANYVANILLA, STRBASEA, 0x60, 0xa4)),
            new("Gravity",  OFLD(ANYSOTFS, STRBASEA, 0xd0, 0x100, 0x134),
                            OFLD(ANYVANILLA, STRBASEA, 0x74, 0xb4, 0x8, 0xfc)),
            new("GameState",    OFLD(ANYSOTFS, STRBASEA, 0x24ac),
                                OFLD(ANYVANILLA, STRBASEA, 0xdec)),
            new("LoadingState", OFLD(ANYSOTFS, STRBASEA, 0x80, 0x8, 0xbb4),
                                OFLD(V102, STRBASEA, 0x24, 0x19c, 0xa94, 0x14, 0x4, 0x4c, 0x730),
                                OFLD(V111, STRBASEA, 0x24, 0x14, 0x2fc, 0x54, 0x870),
                                OFLD(V112, STRBASEA, 0xb0, 0x7c, 0x44, 0xac8, 0x0, 0x4c, 0x730)),
            new("ForceQuit",    OFLD(ANYSOTFS, STRBASEA, 0x24b1),
                                OFLD(ANYVANILLA, STRBASEA, 0xdf1)),
            new("DisableAI",    OFLD(S103, STRBASEA, 0x28, 0x18)),
            new("BIKP1Skip_Val1", OFLD(S103, STRBASEA, 0x70, 0x20, 0x18, 0xe34)),
            new("BIKP1Skip_Val2", OFLD(S103, STRBASEA, 0x70, 0x20, 0x18, 0xd52)),
            new("ConnectionType", OFLD(ANYSOTFS, "BaseB", 0x38, 0x8)),
            new("NetworkPhantomID", OFLD(ALLBUTOLDPATCH, "PlayerTypeOffset", 0x3C),
                                    OFLD(V102, "PlayerTypeOffset", 0x38)),
            new("CurrentCovenant", OFLD(ANYSOTFS, "PlayerParam", 0x1AD),
                                   OFLD(ANYVANILLA, "PlayerParam", 0x1A9)),
            new("SoulLevel", OFLD(ANYSOTFS, "PlayerParam", 0xd0),
                            OFLD(ANYVANILLA, "PlayerParam", 0xcc)),
            new("Class",    OFLD(ANYSOTFS, "PlayerBaseMisc", 0x64),
                            OFLD(ANYVANILLA, "PlayerBaseMisc", 0xe4)),
        };

        public static readonly List<LeafLocatorGroup> LeafGroupDefns = new()
        {
            new("CameraGroup", "Camera2",
                new DetachedLeaf("CamX", (ANYVER, 0x1a0)),
                new DetachedLeaf("CamZ", (ANYVER, 0x1a4)),
                new DetachedLeaf("CamY", (ANYVER, 0x1a8))),

            new("BonusScalingTableGroup", "ScalingBonusTableCtrl",
                new DetachedLeaf("STR", (ANYSOTFS, 0x7a8)),
                new DetachedLeaf("DEX", (ANYSOTFS, 0x7cc)),
                new DetachedLeaf("MAGIC", (ANYSOTFS, 0x7f0)),
                new DetachedLeaf("FIRE", (ANYSOTFS, 0x814)),
                new DetachedLeaf("LIGHTNING", (ANYSOTFS, 0x838)),
                new DetachedLeaf("DARK", (ANYSOTFS, 0x85c)),
                new DetachedLeaf("POISON", (ANYSOTFS, 0x880)),
                new DetachedLeaf("BLEED", (ANYSOTFS, 0x8a4))),
            
        new ("BonfireLevelsGroup", "BonfireLevelsPtr",
                new DetachedLeaf("TheFarFire", (ANYSOTFS, 0x1a), (ANYVANILLA, 0x12)),
                new DetachedLeaf("FireKeepersDwelling", (ANYSOTFS, 0x2), (ANYVANILLA, 0x2)),
                new DetachedLeaf("CrestfallensRetreat", (ANYSOTFS, 0x62), (ANYVANILLA, 0x42)),
                new DetachedLeaf("CardinalTower", (ANYSOTFS, 0x32), (ANYVANILLA, 0x22)),
                new DetachedLeaf("SoldiersRest", (ANYSOTFS, 0x4a), (ANYVANILLA, 0x32)),
                new DetachedLeaf("ThePlaceUnbeknownst", (ANYSOTFS, 0x7a), (ANYVANILLA, 0x52)),
                new DetachedLeaf("HeidesRuin", (ANYSOTFS, 0x4b2), (ANYVANILLA, 0x322)),
                new DetachedLeaf("TowerOfFlame", (ANYSOTFS, 0x49a), (ANYVANILLA, 0x312)),
                new DetachedLeaf("TheBlueCathedral", (ANYSOTFS, 0x4ca), (ANYVANILLA, 0x332)),
                new DetachedLeaf("UnseenPathToHeide", (ANYSOTFS, 0x28a), (ANYVANILLA, 0x1b2)),
                new DetachedLeaf("ExileHoldingCells", (ANYSOTFS, 0x182), (ANYVANILLA, 0x102)),
                new DetachedLeaf("McDuffsWorkshop", (ANYSOTFS, 0x1ca), (ANYVANILLA, 0x132)),
                new DetachedLeaf("ServantsQuarters", (ANYSOTFS, 0x1e2), (ANYVANILLA, 0x142)),
                new DetachedLeaf("StraidsCell", (ANYSOTFS, 0x16a), (ANYVANILLA, 0xf2)),
                new DetachedLeaf("TheTowerApart", (ANYSOTFS, 0x19a), (ANYVANILLA, 0x112)),
                new DetachedLeaf("TheSaltfort", (ANYSOTFS, 0x1fa), (ANYVANILLA, 0x152)),
                new DetachedLeaf("UpperRamparts", (ANYSOTFS, 0x1b2), (ANYVANILLA, 0x122)),
                new DetachedLeaf("UndeadRefuge", (ANYSOTFS, 0x362), (ANYVANILLA, 0x242)),
                new DetachedLeaf("BridgeApproach", (ANYSOTFS, 0x37a), (ANYVANILLA, 0x252)),
                new DetachedLeaf("UndeadLockaway", (ANYSOTFS, 0x392), (ANYVANILLA, 0x262)),
                new DetachedLeaf("UndeadPurgatory", (ANYSOTFS, 0x3aa), (ANYVANILLA, 0x272)),
                new DetachedLeaf("PoisonPool", (ANYSOTFS, 0x242), (ANYVANILLA, 0x182)),
                new DetachedLeaf("TheMines", (ANYSOTFS, 0x212), (ANYVANILLA, 0x162)),
                new DetachedLeaf("LowerEarthenPeak", (ANYSOTFS, 0x22a), (ANYVANILLA, 0x172)),
                new DetachedLeaf("CentralEarthenPeak", (ANYSOTFS, 0x25a), (ANYVANILLA, 0x192)),
                new DetachedLeaf("UpperEarthenPeak", (ANYSOTFS, 0x272), (ANYVANILLA, 0x1a2)),
                new DetachedLeaf("ThresholdBridge", (ANYSOTFS, 0x2ba), (ANYVANILLA, 0x1d2)),
                new DetachedLeaf("IronhearthHall", (ANYSOTFS, 0x2a2), (ANYVANILLA, 0x1c2)),
                new DetachedLeaf("EygilsIdol", (ANYSOTFS, 0x2d2), (ANYVANILLA, 0x1e2)),
                new DetachedLeaf("BelfrySolApproach", (ANYSOTFS, 0x2ea), (ANYVANILLA, 0x1f2)),
                new DetachedLeaf("OldAkelarre", (ANYSOTFS, 0x482), (ANYVANILLA, 0x302)),
                new DetachedLeaf("RuinedForkRoad", (ANYSOTFS, 0x4e2), (ANYVANILLA, 0x342)),
                new DetachedLeaf("ShadedRuins", (ANYSOTFS, 0x4fa), (ANYVANILLA, 0x352)),
                new DetachedLeaf("GyrmsRespite", (ANYSOTFS, 0x512), (ANYVANILLA, 0x362)),
                new DetachedLeaf("OrdealsEnd", (ANYSOTFS, 0x52a), (ANYVANILLA, 0x372)),
                new DetachedLeaf("RoyalArmyCampsite", (ANYSOTFS, 0x10a), (ANYVANILLA, 0xb2)),
                new DetachedLeaf("ChapelThreshold", (ANYSOTFS, 0x122), (ANYVANILLA, 0xc2)),
                new DetachedLeaf("LowerBrightstoneCove", (ANYSOTFS, 0xf2), (ANYVANILLA, 0xa2)),
                new DetachedLeaf("HarvalsRestingPlace", (ANYSOTFS, 0x55a), (ANYVANILLA, 0x392)),
                new DetachedLeaf("GraveEntrance", (ANYSOTFS, 0x542), (ANYVANILLA, 0x382)),
                new DetachedLeaf("UpperGutter", (ANYSOTFS, 0x43a), (ANYVANILLA, 0x2d2)),
                new DetachedLeaf("CentralGutter", (ANYSOTFS, 0x40a), (ANYVANILLA, 0x2b2)),
                new DetachedLeaf("BlackGulchMouth", (ANYSOTFS, 0x3f2), (ANYVANILLA, 0x2a2)),
                new DetachedLeaf("HiddenChamber", (ANYSOTFS, 0x422), (ANYVANILLA, 0x2c2)),
                new DetachedLeaf("KingsGate", (ANYSOTFS, 0x302), (ANYVANILLA, 0x202)),
                new DetachedLeaf("ForgottenChamber", (ANYSOTFS, 0x332), (ANYVANILLA, 0x222)),
                new DetachedLeaf("UnderCastleDrangleic", (ANYSOTFS, 0x34a), (ANYVANILLA, 0x232)),
                new DetachedLeaf("CentralCastleDrangleic", (ANYSOTFS, 0x31a), (ANYVANILLA, 0x212)),
                new DetachedLeaf("TowerOfPrayerAmana", (ANYSOTFS, 0x92), (ANYVANILLA, 0x62)),
                new DetachedLeaf("CrumbledRuins", (ANYSOTFS, 0xaa), (ANYVANILLA, 0x72)),
                new DetachedLeaf("RhoysRestingPlace", (ANYSOTFS, 0xc2), (ANYVANILLA, 0x82)),
                new DetachedLeaf("RiseOfTheDead", (ANYSOTFS, 0xda), (ANYVANILLA, 0x92)),
                new DetachedLeaf("UndeadCryptEntrance", (ANYSOTFS, 0x3da), (ANYVANILLA, 0x292)),
                new DetachedLeaf("UndeadDitch", (ANYSOTFS, 0x3c2), (ANYVANILLA, 0x282)),
                new DetachedLeaf("Foregarden", (ANYSOTFS, 0x13a), (ANYVANILLA, 0xd2)),
                new DetachedLeaf("RitualSite", (ANYSOTFS, 0x152), (ANYVANILLA, 0xe2)),
                new DetachedLeaf("DragonAerie", (ANYSOTFS, 0x452), (ANYVANILLA, 0x2e2)),
                new DetachedLeaf("ShrineEntrance", (ANYSOTFS, 0x46a), (ANYVANILLA, 0x2f2)),
                new DetachedLeaf("SanctumWalk", (ANYSOTFS, 0x572), (ANYVANILLA, 0x3a2)),
                new DetachedLeaf("TowerOfPrayerShulva", (ANYSOTFS, 0x602), (ANYVANILLA, 0x402)),
                new DetachedLeaf("PriestessChamber", (ANYSOTFS, 0x58a), (ANYVANILLA, 0x3b2)),
                new DetachedLeaf("HiddenSanctumChamber", (ANYSOTFS, 0x5ba), (ANYVANILLA, 0x3d2)),
                new DetachedLeaf("LairOfTheImperfect", (ANYSOTFS, 0x5d2), (ANYVANILLA, 0x3e2)),
                new DetachedLeaf("SanctumInterior", (ANYSOTFS, 0x5ea), (ANYVANILLA, 0x3f2)),
                new DetachedLeaf("SanctumNadir", (ANYSOTFS, 0x5a2), (ANYVANILLA, 0x3c2)),
                new DetachedLeaf("ThroneFloor", (ANYSOTFS, 0x61a), (ANYVANILLA, 0x412)),
                new DetachedLeaf("UpperFloor", (ANYSOTFS, 0x64a), (ANYVANILLA, 0x432)),
                new DetachedLeaf("Foyer", (ANYSOTFS, 0x632), (ANYVANILLA, 0x422)),
                new DetachedLeaf("LowermostFloor", (ANYSOTFS, 0x67a), (ANYVANILLA, 0x452)),
                new DetachedLeaf("TheSmelterThrone", (ANYSOTFS, 0x692), (ANYVANILLA, 0x462)),
                new DetachedLeaf("IronHallwayEntrance", (ANYSOTFS, 0x662), (ANYVANILLA, 0x442)),
                new DetachedLeaf("OuterWall", (ANYSOTFS, 0x6aa), (ANYVANILLA, 0x472)),
                new DetachedLeaf("AbandonedDwelling", (ANYSOTFS, 0x6c2), (ANYVANILLA, 0x482)),
                new DetachedLeaf("InnerWall", (ANYSOTFS, 0x722), (ANYVANILLA, 0x4c2)),
                new DetachedLeaf("LowerGarrison", (ANYSOTFS, 0x6da), (ANYVANILLA, 0x492)),
                new DetachedLeaf("ExpulsionChamber", (ANYSOTFS, 0x70a), (ANYVANILLA, 0x4b2)),
                new DetachedLeaf("GrandCathedral", (ANYSOTFS, 0x6f2), (ANYVANILLA, 0x4a2))),

            new("LastBonfireGroup", "EventManager",
                new DetachedLeaf("LastSetBonfireAreaID", (ANYSOTFS, 0x164), (ANYVANILLA, 0xb4)),
                new DetachedLeaf("LastSetBonfire", (ANYSOTFS, 0x16c), (ANYVANILLA, 0xbc))),

            new("WarpGroup", "PlayerDataMapPtr",
                new DetachedLeaf("WarpY1", (ANYSOTFS, 0x1a0), (ANYVANILLA, 0x120)),
                new DetachedLeaf("WarpZ1", (ANYSOTFS, 0x1a4), (ANYVANILLA, 0x124)),
                new DetachedLeaf("WarpX1", (ANYSOTFS, 0x1a8), (ANYVANILLA, 0x128)),
                new DetachedLeaf("WarpY2", (ANYSOTFS, 0x1b0), (ANYVANILLA, 0x130)),
                new DetachedLeaf("WarpZ2", (ANYSOTFS, 0x1b4), (ANYVANILLA, 0x134)),
                new DetachedLeaf("WarpX2", (ANYSOTFS, 0x1b8), (ANYVANILLA, 0x138)),
                new DetachedLeaf("WarpY3", (ANYSOTFS, 0x1c0), (ANYVANILLA, 0x140)),
                new DetachedLeaf("WarpZ3", (ANYSOTFS, 0x1c4), (ANYVANILLA, 0x144)),
                new DetachedLeaf("WarpX3", (ANYSOTFS, 0x1c8), (ANYVANILLA, 0x148))),

            new("AttributeGroup", "PlayerParam",
                new DetachedLeaf("VGR", (ANYSOTFS, 0x8), (ANYVANILLA, 0x4)),
                new DetachedLeaf("END", (ANYSOTFS, 0xa), (ANYVANILLA, 0x6)),
                new DetachedLeaf("VIT", (ANYSOTFS, 0xc), (ANYVANILLA, 0x8)),
                new DetachedLeaf("ATN", (ANYSOTFS, 0xe), (ANYVANILLA, 0xa)),
                new DetachedLeaf("STR", (ANYSOTFS, 0x10), (ANYVANILLA, 0xc)),
                new DetachedLeaf("DEX", (ANYSOTFS, 0x12), (ANYVANILLA, 0xe)),
                new DetachedLeaf("ADP", (ANYSOTFS, 0x18), (ANYVANILLA, 0x14)),
                new DetachedLeaf("INT", (ANYSOTFS, 0x14), (ANYVANILLA, 0x10)),
                new DetachedLeaf("FTH", (ANYSOTFS, 0x16), (ANYVANILLA, 0x12))),

            new("PlayerParamGroup", "PlayerParam",
                new DetachedLeaf("SoulMemory", (ANYSOTFS, 0xf4), (ANYVANILLA, 0xf0)),
                new DetachedLeaf("SoulMemory2", (ANYSOTFS, 0xfc), (ANYVANILLA, 0xf8)),
                new DetachedLeaf("MaxEquipLoad", (ANYSOTFS, 0x3c), (ANYVANILLA, 0x38)),
                new DetachedLeaf("Souls", (ANYSOTFS, 0xec), (ANYVANILLA, 0xe8)),
                new DetachedLeaf("TotalDeaths", (ANYSOTFS, 0x1a4), (ANYVANILLA, 0x1a0)),
                new DetachedLeaf("HollowLevel", (ANYSOTFS, 0x1ac), (ANYVANILLA, 0x1a8)),
                new DetachedLeaf("SinnerLevel", (ANYSOTFS, 0x1d6), (ANYVANILLA, 0x1d2)),
                new DetachedLeaf("SinnerPoints", (ANYSOTFS, 0x1d7), (ANYVANILLA, 0x1d3))),

            new("PlayerEquipmentGroup", "PlayerCtrl",
                new DetachedLeaf("Legs", (ANYSOTFS, 0x920), (ANYVANILLA, 0x1f8)),
                new DetachedLeaf("Arms", (ANYSOTFS, 0x90c), (ANYVANILLA, 0x1dc)),
                new DetachedLeaf("Chest", (ANYSOTFS, 0x8f8), (ANYVANILLA, 0x1c0)),
                new DetachedLeaf("Head", (ANYSOTFS, 0x8e4), (ANYVANILLA, 0x1a4)),
                new DetachedLeaf("RightHand1", (ANYSOTFS, 0x880), (ANYVANILLA, 0xc8)),
                new DetachedLeaf("RightHand2", (ANYSOTFS, 0x8a8), (ANYVANILLA, 0xf4)),
                new DetachedLeaf("RightHand3", (ANYSOTFS, 0x8d0), (ANYVANILLA, 0x120)),
                new DetachedLeaf("LeftHand1", (ANYSOTFS, 0x86c), (ANYVANILLA, 0x44)),
                new DetachedLeaf("LeftHand2", (ANYSOTFS, 0x894), (ANYVANILLA, 0x70)),
                new DetachedLeaf("LeftHand3", (ANYSOTFS, 0x8bc), (ANYVANILLA, 0x9c))),

            new("PlayerBaseMiscGroup", "PlayerBaseMisc",
                new DetachedLeaf("NewGame", (ANYSOTFS, 0x68), (ANYVANILLA, 0xe8)),
                new DetachedLeaf("SaveSlot", (ANYSOTFS, 0x18a8))),

            new("PlayerGroup", "PlayerCtrl",
                new DetachedLeaf("HP", (ANYSOTFS, 0x168), (ANYVANILLA, 0xfc)),
                new DetachedLeaf("HPMin", (ANYSOTFS, 0x16c), (ANYVANILLA, 0x100)),
                new DetachedLeaf("HPMax", (ANYSOTFS, 0x170), (ANYVANILLA, 0x104)),
                new DetachedLeaf("HPCap", (ANYSOTFS, 0x174), (ANYVANILLA, 0x108)),
                new DetachedLeaf("SP", (ANYSOTFS, 0x1ac), (ANYVANILLA, 0x140)),
                new DetachedLeaf("SPMax", (ANYSOTFS, 0x1b4), (ANYVANILLA, 0x148)),
                new DetachedLeaf("SPMin", (ANYSOTFS, 0x1b0), (ANYVANILLA, 0x144)),
                new DetachedLeaf("SpeedModifier", (ANYSOTFS, 0x2a8), (ANYVANILLA, 0x208)),
                new DetachedLeaf("CurrPoise", (ANYSOTFS, 0x218), (ANYVANILLA, 0x1ac))),

            new("CovenantsGroup", "PlayerParam",
                new DetachedLeaf("HeirsOfTheSunDiscovered", (ANYSOTFS, 0x1af), (ANYVANILLA, 0x1ab)),
                new DetachedLeaf("HeirsOfTheSunRank", (ANYSOTFS, 0x1b9), (ANYVANILLA, 0x1b5)),
                new DetachedLeaf("HeirsOfTheSunProgress", (ANYSOTFS, 0x1c4), (ANYVANILLA, 0x1c0)),
                new DetachedLeaf("BlueSentinelsDiscovered", (ANYSOTFS, 0x1b0), (ANYVANILLA, 0x1ac)),
                new DetachedLeaf("BlueSentinelsRank", (ANYSOTFS, 0x1ba), (ANYVANILLA, 0x1b6)),
                new DetachedLeaf("BlueSentinelsProgress", (ANYSOTFS, 0x1c6), (ANYVANILLA, 0x1c2)),
                new DetachedLeaf("BrotherhoodOfBloodDiscovered", (ANYSOTFS, 0x1b1), (ANYVANILLA, 0x1ad)),
                new DetachedLeaf("BrotherhoodOfBloodRank", (ANYSOTFS, 0x1bb), (ANYVANILLA, 0x1b7)),
                new DetachedLeaf("BrotherhoodOfBloodProgress", (ANYSOTFS, 0x1cb), (ANYVANILLA, 0x1c4)),
                new DetachedLeaf("WayOfTheBlueDiscovered", (ANYSOTFS, 0x1b2), (ANYVANILLA, 0x1ae)),
                new DetachedLeaf("WayOfTheBlueRank", (ANYSOTFS, 0x1bc), (ANYVANILLA, 0x1b8)),
                new DetachedLeaf("WayOfTheBlueProgress", (ANYSOTFS, 0x1ca), (ANYVANILLA, 0x1c6)),
                new DetachedLeaf("RatKingDiscovered", (ANYSOTFS, 0x1b3), (ANYVANILLA, 0x1af)),
                new DetachedLeaf("RatKingRank", (ANYSOTFS, 0x1bd), (ANYVANILLA, 0x1b9)),
                new DetachedLeaf("RatKingProgress", (ANYSOTFS, 0x1cc), (ANYVANILLA, 0x1c8)),
                new DetachedLeaf("BellKeepersDiscovered", (ANYSOTFS, 0x1b4), (ANYVANILLA, 0x1b0)),
                new DetachedLeaf("BellKeepersRank", (ANYSOTFS, 0x1be), (ANYVANILLA, 0x1ba)),
                new DetachedLeaf("BellKeepersProgress", (ANYSOTFS, 0x1ce), (ANYVANILLA, 0x1ca)),
                new DetachedLeaf("DragonRemnantsDiscovered", (ANYSOTFS, 0x1b5), (ANYVANILLA, 0x1b1)),
                new DetachedLeaf("DragonRemnantsRank", (ANYSOTFS, 0x1bf), (ANYVANILLA, 0x1bb)),
                new DetachedLeaf("DragonRemnantsProgress", (ANYSOTFS, 0x1d0), (ANYVANILLA, 0x1cc)),
                new DetachedLeaf("CompanyOfChampionsDiscovered", (ANYSOTFS, 0x1b6), (ANYVANILLA, 0x1b2)),
                new DetachedLeaf("CompanyOfChampionsRank", (ANYSOTFS, 0x1c0), (ANYVANILLA, 0x1bc)),
                new DetachedLeaf("CompanyOfChampionsProgress", (ANYSOTFS, 0x1d2), (ANYVANILLA, 0x1ce)),
                new DetachedLeaf("PilgrimsOfDarknessDiscovered", (ANYSOTFS, 0x1b7), (ANYVANILLA, 0x1b3)),
                new DetachedLeaf("PilgrimsOfDarknessRank", (ANYSOTFS, 0x1c1), (ANYVANILLA, 0x1bd)),
                new DetachedLeaf("PilgrimsOfDarknessProgress", (ANYSOTFS, 0x1d4), (ANYVANILLA, 0x1d0)))
        };

    }

}
