using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DS2S_META.Utils;
using DS2S_META.Utils.Offsets.OffsetClasses;

namespace DS2S_META.Utils.Offsets
{
    public class Covenant3
    {
        // Version shorthands
        public readonly static List<DS2VER> ANYVER = new() { DS2VER.SOTFS_V103, DS2VER.SOTFS_V102, DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> ANYSOTFS = new() { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 };
        public readonly static List<DS2VER> ANYVANILLA = new() { DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> V102 = new() { DS2VER.VANILLA_V102 };
        public readonly static List<DS2VER> V111 = new() { DS2VER.VANILLA_V111 };
        public readonly static List<DS2VER> V112 = new() { DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> V111OR112 = new() { DS2VER.VANILLA_V111, DS2VER.VANILLA_V111 };
        public readonly static string STRBASEA = "BaseA";

        // BaseA, BaseB
        public PointerDefn BaseA = new(STRBASEA, new List<LocatorDefn>()
        {
            new(ANYSOTFS, new RelInstructionAOBCL("48 8B 05 ? ? ? ? 48 8B 58 38 48 85 DB 74 ? F6",3,7)),
            new(ANYVANILLA, new RelModuleAOBCL("8B F1 8B 0D ? ? ? ? 8B 01 8B 50 28 FF D2 84 C0 74 0C",3))
        });
        public PointerDefn BaseB = new("BaseB", new List<LocatorDefn>()
        {
            new(ANYSOTFS, new RelInstructionAOBCL("48 8B 0D ? ? ? ? 48 85 C9 74 ? 48 8B 49 18 E8",3,7)),
            new(ANYVANILLA, new RelModuleAOBCL("89 45 98 A1 ? ? ? ? 89 7D 9C 89 BD ? ? ? ? 85 C0",3))
        });

        // BaseA ChildPointers:
        public PointerDefn PlayerTypeOffset = new("PlayerTypeOffset",
            new OFLD(ANYSOTFS, STRBASEA, 0xb0),
            new OFLD(ANYVANILLA, STRBASEA, 0x90));
        public PointerDefn AvailableItemBag = new("AvailableItemBag",
            new OFLD(ANYSOTFS, STRBASEA, 0xa8, 0x10, 0x10),
            new OFLD(ANYVANILLA, STRBASEA, 0x60, 0x8, 0x8));
        public PointerDefn ItemGiveWindowPointer = new("ItemGiveWindowPointer",
            new OFLD(ANYSOTFS, STRBASEA, 0x22e0),
            new OFLD(ANYVANILLA, STRBASEA, 0xcc4));
        public PointerDefn PlayerCtrlOffset = new("PlayerCtrl",
            new OFLD(ANYSOTFS, STRBASEA, 0xd0),
            new OFLD(ANYVANILLA, STRBASEA, 0x74));
        public PointerDefn NetSvrBloodstainManager = new("NetSvrBloodstainManager",
            new OFLD(ANYVER, STRBASEA, 0x90, 0x28, 0x88));
        public PointerDefn PlayerParamOffset = new("PlayerParam",
            new OFLD(ANYSOTFS, "PlayerCtrl", 0x490),
            new OFLD(ANYVANILLA, "PlayerCtrl", 0x378));
        public PointerDefn PlayerPosition = new("PlayerPosition",
            new OFLD(ANYSOTFS, "PlayerCtrl", 0xf8, 0xf0),
            new OFLD(ANYVANILLA, "PlayerCtrl", 0xb4, 0xa8));
        public PointerDefn PlayerDataMapPtr = new("PlayerDataMapPtr",
            new OFLD(ANYSOTFS, "PlayerCtrl", 0x100, 0x320, 0x20),
            new OFLD(ANYVANILLA, "PlayerCtrl", 0xB8, 0x8, 0x14, 0x1B0, 0x10));
        public PointerDefn SpEffectCtrl = new("SpEffectCtrl",
            new OFLD(ANYSOTFS, "PlayerCtrl", 0x3e0),
            new OFLD(ANYVANILLA, "PlayerCtrl", 0x308));
        public PointerDefn EventManager = new("EventManager",
            new OFLD(ANYSOTFS, STRBASEA, 0x70),
            new OFLD(ANYVANILLA, STRBASEA, 0x44));
        public PointerDefn WarpManager = new("WarpManager",
            new OFLD(ANYSOTFS, "EventManager", 0x70),
            new OFLD(ANYVANILLA, "EventManager", 0x2c));
        public PointerDefn BonfireLevelsPtr = new("BonfireLevelsPtr",
            new OFLD(ANYSOTFS, "EventManager", 0x58, 0x20),
            new OFLD(ANYVANILLA, "EventManager", 0x2c, 0x10));
        public PointerDefn Connection = new("Connection",
            new OFLD(ANYSOTFS, "BaseB", 0x38));
        public PointerDefn Camera2 = new("Camera2",
            new OFLD(ANYVER, STRBASEA, 0x0, 0x20));
        public PointerDefn PlayerBaseMisc = new("PlayerBaseMisc",
            new OFLD(ANYSOTFS, STRBASEA, 0xa8, 0xc0),
            new OFLD(ANYVANILLA, STRBASEA, 0x60)); // should probably align these?

        // Leaves:
        public LeafDefn PlayerName = new("PlayerName",
            new OFLD(ANYSOTFS, STRBASEA, 0xa8, 0x114),
            new OFLD(ANYVANILLA, STRBASEA, 0x60, 0xa4));
        public LeafDefn Gravity = new("Gravity",
            new OFLD(ANYSOTFS, STRBASEA, 0xd0, 0x100, 0x134),
            new OFLD(ANYVANILLA, STRBASEA, 0x74, 0xb4, 0x8, 0xfc));
        public LeafDefn GameState = new("GameState",
            new OFLD(ANYSOTFS, STRBASEA, 0x24ac),
            new OFLD(ANYVANILLA, STRBASEA, 0xdec));
        public LeafDefn LoadingState = new("LoadingState",
            new OFLD(ANYSOTFS, STRBASEA, 0x80, 0x8, 0xbb4),
            new OFLD(V102, STRBASEA, 0x24, 0x19c, 0xa94, 0x14, 0x4, 0x4c, 0x730),
            new OFLD(V111, STRBASEA, 0x24, 0x14, 0x2fc, 0x54, 0x870),
            new OFLD(V112, STRBASEA, 0xB0, 0x7C, 0x44, 0xac8, 0x0, 0x4c, 0x730));
        public LeafDefn ForceQuit = new("ForceQuit",
            new OFLD(ANYSOTFS, STRBASEA, 0x24b1),
            new OFLD(ANYVANILLA, STRBASEA, 0xdf1));


        public LeafLocatorGroup BonfireLevelsGroup = new("BonfireLevelsPtr",
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
            new DetachedLeaf("GrandCathedral", (ANYSOTFS, 0x6f2), (ANYVANILLA, 0x4a2))
        );

        public LeafLocatorGroup BonfireGroup = new("EventManager",
            new DetachedLeaf("LastSetBonfireAreaID", (ANYSOTFS, 0x164), (ANYVANILLA, 0xb4)),
            new DetachedLeaf("LastSetBonfire", (ANYSOTFS, 0x16c), (ANYVANILLA, 0xbc))
        );

        public LeafLocatorGroup PlayerMapDataGroup = new("PlayerDataMapPtr",
            new DetachedLeaf("WarpY1", (ANYSOTFS, 0x1a0), (ANYVANILLA, 0x120)),
            new DetachedLeaf("WarpZ1", (ANYSOTFS, 0x1a4), (ANYVANILLA, 0x124)),
            new DetachedLeaf("WarpX1", (ANYSOTFS, 0x1a8), (ANYVANILLA, 0x128)),
            new DetachedLeaf("WarpY2", (ANYSOTFS, 0x1b0), (ANYVANILLA, 0x130)),
            new DetachedLeaf("WarpZ2", (ANYSOTFS, 0x1b4), (ANYVANILLA, 0x134)),
            new DetachedLeaf("WarpX2", (ANYSOTFS, 0x1b8), (ANYVANILLA, 0x138)),
            new DetachedLeaf("WarpY3", (ANYSOTFS, 0x1c0), (ANYVANILLA, 0x140)),
            new DetachedLeaf("WarpZ3", (ANYSOTFS, 0x1c4), (ANYVANILLA, 0x144)),
            new DetachedLeaf("WarpX3", (ANYSOTFS, 0x1c8), (ANYVANILLA, 0x148))
        );

        public LeafLocatorGroup PlayerEquipmentGroup = new("PlayerCtrl",
            new DetachedLeaf("SoulLevel", (ANYSOTFS, 0xd0), (ANYVANILLA, 0xcc)),
            new DetachedLeaf("VGR", (ANYSOTFS, 0x8), (ANYVANILLA, 0x4)),
            new DetachedLeaf("END", (ANYSOTFS, 0xa), (ANYVANILLA, 0x6)),
            new DetachedLeaf("VIT", (ANYSOTFS, 0xc), (ANYVANILLA, 0x8)),
            new DetachedLeaf("ATN", (ANYSOTFS, 0xe), (ANYVANILLA, 0xa)),
            new DetachedLeaf("STR", (ANYSOTFS, 0x10), (ANYVANILLA, 0xc)),
            new DetachedLeaf("DEX", (ANYSOTFS, 0x12), (ANYVANILLA, 0xe)),
            new DetachedLeaf("ADP", (ANYSOTFS, 0x18), (ANYVANILLA, 0x14)),
            new DetachedLeaf("INT", (ANYSOTFS, 0x14), (ANYVANILLA, 0x10)),
            new DetachedLeaf("FTH", (ANYSOTFS, 0x16), (ANYVANILLA, 0x12))
        );

        public LeafLocatorGroup PlayerParamGroup = new("PlayerParam",
            new DetachedLeaf("SoulMemory", (ANYSOTFS, 0xf4), (ANYVANILLA, 0xf0)),
            new DetachedLeaf("SoulMemory2", (ANYSOTFS, 0xfc), (ANYVANILLA, 0xf8)),
            new DetachedLeaf("MaxEquipLoad", (ANYSOTFS, 0x3c), (ANYVANILLA, 0x38)),
            new DetachedLeaf("Souls", (ANYSOTFS, 0xec), (ANYVANILLA, 0xe8)),
            new DetachedLeaf("TotalDeaths", (ANYSOTFS, 0x1a4), (ANYVANILLA, 0x1a0)),
            new DetachedLeaf("HollowLevel", (ANYSOTFS, 0x1ac), (ANYVANILLA, 0x1a8)),
            new DetachedLeaf("SinnerLevel", (ANYSOTFS, 0x1d6), (ANYVANILLA, 0x1d2)),
            new DetachedLeaf("SinnerPoints", (ANYSOTFS, 0x1d7), (ANYVANILLA, 0x1d3))
        );

        public LeafLocatorGroup PlayerEquipment = new("PlayerCtrl",
            new DetachedLeaf("Legs", (ANYSOTFS, 0x920), (ANYVANILLA, 0x1f8)),
            new DetachedLeaf("Arms", (ANYSOTFS, 0x90c), (ANYVANILLA, 0x1dc)),
            new DetachedLeaf("Chest", (ANYSOTFS, 0x8f8), (ANYVANILLA, 0x1c0)),
            new DetachedLeaf("Head", (ANYSOTFS, 0x8e4), (ANYVANILLA, 0x1a4)),
            new DetachedLeaf("RightHand1", (ANYSOTFS, 0x880), (ANYVANILLA, 0xc8)),
            new DetachedLeaf("RightHand2", (ANYSOTFS, 0x8a8), (ANYVANILLA, 0xf4)),
            new DetachedLeaf("RightHand3", (ANYSOTFS, 0x8d0), (ANYVANILLA, 0x120)),
            new DetachedLeaf("LeftHand1", (ANYSOTFS, 0x86c), (ANYVANILLA, 0x44)),
            new DetachedLeaf("LeftHand2", (ANYSOTFS, 0x894), (ANYVANILLA, 0x70)),
            new DetachedLeaf("LeftHand3", (ANYSOTFS, 0x8bc), (ANYVANILLA, 0x9c))
        );

        public LeafLocatorGroup PlayerBaseMiscGroup = new("PlayerBaseMisc",
            new DetachedLeaf("Class", (ANYSOTFS, 0x64), (ANYVANILLA, 0xe4)),
            new DetachedLeaf("NewGame", (ANYSOTFS, 0x68), (ANYVANILLA, 0xe8)),
            new DetachedLeaf("SaveSlot", (ANYSOTFS, 0x18a8))
        );

        public LeafLocatorGroup PlayerGroup = new("PlayerCtrl",
            new DetachedLeaf("HP", (ANYSOTFS, 0x168), (ANYVANILLA, 0xfc)),
            new DetachedLeaf("HPMin", (ANYSOTFS, 0x16c), (ANYVANILLA, 0x100)),
            new DetachedLeaf("HPMax", (ANYSOTFS, 0x170), (ANYVANILLA, 0x104)),
            new DetachedLeaf("HPCap", (ANYSOTFS, 0x174), (ANYVANILLA, 0x108)),
            new DetachedLeaf("SP", (ANYSOTFS, 0x1ac), (ANYVANILLA, 0x140)),
            new DetachedLeaf("SPMax", (ANYSOTFS, 0x1b4), (ANYVANILLA, 0x148)),
            new DetachedLeaf("SpeedModifier", (ANYSOTFS, 0x2a8), (ANYVANILLA, 0x208)),
            new DetachedLeaf("CurrPoise", (ANYSOTFS, 0x218), (ANYVANILLA, 0x1ac))
        );

        public LeafLocatorGroup CovenantsGroup = new("PlayerParam",
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
            new DetachedLeaf("PilgrimsOfDarknessProgress", (ANYSOTFS, 0x1d4), (ANYVANILLA, 0x1d0))
        );

    }

}
