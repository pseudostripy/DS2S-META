using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;

namespace DS2S_META.Utils.Offsets
{
    public abstract class DS2SOffsets : DS2HookOffsets
    {
        public DS2SOffsets()
        {
            // BaseA
            BaseAAob = "48 8B 05 ? ? ? ? 48 8B 58 38 48 85 DB 74 ? F6";
            BaseABabyJumpAoB = "49 BA ? ? ? ? ? ? ? ? 41 FF E2 90 74 2E";
            BasePtrOffset1 = 0x3;
            BasePtrOffset2 = 0x7;

            LoadingState = new int[] { 0x80, 0x8, 0xBB4 };
            GameState = 0x24AC; // from basea

            // Records:
            ForceQuit = new (0x24B1);
            PlayerName = new(0x114);
            PlayerCtrl = new(0x168, 0x16C, 0x170, 0x174, 0x1AC, 0x1B4, 0x2A8, 0x218, 0x1B0);
            PlayerBaseMisc = new(0x64, 0x68, 0x18A8);
            PlayerEquipment = new(0x920, 0x90C, 0x8F8, 0x8E4,
                                  0x880, 0x8A8, 0x8D0,
                                  0x86C, 0x894, 0x8BC);
            PlayerParam = new(0xF4, 0xFC, 0x3C, 0xEC,
                              0x1A4, 0x1AC, 0x1D6, 0x1D7);
            Attributes = new(0xD0, 0x8, 0xA, 0xC, 0xE, 0x10, 0x12, 0x18, 0x14, 0x16);

            Covenants = new()
            {
                CurrentCovenant = 0x1AD,
                HeirsOfTheSunDiscovered = 0x1AF,
                HeirsOfTheSunRank = 0x1B9,
                HeirsOfTheSunProgress = 0x1C4,
                BlueSentinelsDiscovered = 0x1B0,
                BlueSentinelsRank = 0x1BA,
                BlueSentinelsProgress = 0x1C6,
                BrotherhoodOfBloodDiscovered = 0x1B1,
                BrotherhoodOfBloodRank = 0x1BB,
                BrotherhoodOfBloodProgress = 0x1CB,
                WayOfTheBlueDiscovered = 0x1B2,
                WayOfTheBlueRank = 0x1BC,
                WayOfTheBlueProgress = 0x1CA,
                RatKingDiscovered = 0x1B3,
                RatKingRank = 0x1BD,
                RatKingProgress = 0x1CC,
                BellKeepersDiscovered = 0x1B4,
                BellKeepersRank = 0x1BE,
                BellKeepersProgress = 0x1CE,
                DragonRemnantsDiscovered = 0x1B5,
                DragonRemnantsRank = 0x1BF,
                DragonRemnantsProgress = 0x1D0,
                CompanyOfChampionsDiscovered = 0x1B6,
                CompanyOfChampionsRank = 0x1C0,
                CompanyOfChampionsProgress = 0x1D2,
                PilgrimsOfDarknessDiscovered = 0x1B7,
                PilgrimsOfDarknessRank = 0x1C1,
                PilgrimsOfDarknessProgress = 0x1D4
            };

            Gravity = new(0x134);
            PlayerMapData = new()
            {
                WarpBase = 0x1A0,
                WarpY1 = 0x1A0,
                WarpZ1 = 0x1A4,
                WarpX1 = 0x1A8,
                WarpY2 = 0x1B0,
                WarpZ2 = 0x1B4,
                WarpX2 = 0x1B8,
                WarpY3 = 0x1C0,
                WarpZ3 = 0x1C4,
                WarpX3 = 0x1C8
            };
            Bonfire = new(0x164, 0x16C);
            BonfireLevels = new()
            {
                FireKeepersDwelling = 0x2,
                TheFarFire = 0x1A,
                CrestfallensRetreat = 0x62,
                CardinalTower = 0x32,
                SoldiersRest = 0x4A,
                ThePlaceUnbeknownst = 0x7A,
                HeidesRuin = 0x4B2,
                TowerOfFlame = 0x49A,
                TheBlueCathedral = 0x4CA,
                UnseenPathtoHeide = 0x28A,
                ExileHoldingCells = 0x182,
                McDuffsWorkshop = 0x1CA,
                ServantsQuarters = 0x1E2,
                StraidsCell = 0x16A,
                TheTowerApart = 0x19A,
                TheSaltfort = 0x1FA,
                UpperRamparts = 0x1B2,
                UndeadRefuge = 0x362,
                BridgeApproach = 0x37A,
                UndeadLockaway = 0x392,
                UndeadPurgatory = 0x3AA,
                PoisonPool = 0x242,
                TheMines = 0x212,
                LowerEarthenPeak = 0x22A,
                CentralEarthenPeak = 0x25A,
                UpperEarthenPeak = 0x272,
                ThresholdBridge = 0x2BA,
                IronhearthHall = 0x2A2,
                EygilsIdol = 0x2D2,
                BelfrySolApproach = 0x2EA,
                OldAkelarre = 0x482,
                RuinedForkRoad = 0x4E2,
                ShadedRuins = 0x4FA,
                GyrmsRespite = 0x512,
                OrdealsEnd = 0x52A,
                RoyalArmyCampsite = 0x10A,
                ChapelThreshold = 0x122,
                LowerBrightstoneCove = 0xF2,
                HarvalsRestingPlace = 0x55A,
                GraveEntrance = 0x542,
                UpperGutter = 0x43A,
                CentralGutter = 0x40A,
                HiddenChamber = 0x422,
                BlackGulchMouth = 0x3F2,
                KingsGate = 0x302,
                UnderCastleDrangleic = 0x34A,
                ForgottenChamber = 0x332,
                CentralCastleDrangleic = 0x31A,
                TowerOfPrayerAmana = 0x92,
                CrumbledRuins = 0xAA,
                RhoysRestingPlace = 0xC2,
                RiseOfTheDead = 0xDA,
                UndeadCryptEntrance = 0x3DA,
                UndeadDitch = 0x3C2,
                Foregarden = 0x13A,
                RitualSite = 0x152,
                DragonAerie = 0x452,
                ShrineEntrance = 0x46A,
                SanctumWalk = 0x572,
                PriestessChamber = 0x58A,
                HiddenSanctumChamber = 0x5BA,
                LairOfTheImperfect = 0x5D2,
                SanctumInterior = 0x5EA,
                TowerOfPrayerShulva = 0x602,
                SanctumNadir = 0x5A2,
                ThroneFloor = 0x61A,
                UpperFloor = 0x64A,
                Foyer = 0x632,
                LowermostFloor = 0x67A,
                TheSmelterThrone = 0x692,
                IronHallwayEntrance = 0x662,
                OuterWall = 0x6AA,
                AbandonedDwelling = 0x6C2,
                ExpulsionChamber = 0x70A,
                InnerWall = 0x722,
                LowerGarrison = 0x6DA,
                GrandCathedral = 0x6F2
            };
            Connection = new(0x8, UNSET);
            Camera = new()
            {
                CamStart = 0x170,
                CamStart2 = 0x19C,
                CamStart3 = 0x1C,
                CamX = 0x1A0,
                CamZ = 0x1A4,
                CamY = 0x1A8
            };

            // Core structures:
            Core = new()
            {
                PlayerTypeOffset = 0xB0,
                GameDataManagerOffset = 0xA8, // CAREFUL/UNCHECKED
                AvailableItemBagOffset = 0x10,
                ItemGiveWindowPointer = 0x22E0,
                PlayerCtrlOffset = 0xD0,
                NetSvrBloodstainManagerOffset1 = 0x90,
                NetSvrBloodstainManagerOffset2 = 0x28,
                NetSvrBloodstainManagerOffset3 = 0x88,
                PlayerParamOffset = 0x490,
                PlayerPositionOffset1 = 0xF8,
                PlayerPositionOffset2 = 0xF0,
                PlayerDataMapOffset = new int[4] { 0xD0, 0x100, 0x320, 0x20 },
                SpEffectCtrlOffset = 0x3E0,
                CharacterFlagsOffset = 0x490,
                EventManagerOffset = 0x70,
                WarpManagerOffset = 0x70,
                BonfireLevelsOffset1 = 0x58,
                BonfireLevelsOffset2 = 0x20,
                ConnectionOffset = 0x38,
                CameraOffset1 = 0x0,
                CameraOffset2 = 0x20,
                CameraOffset3 = 0x28,
                //SpEfCtrl2 = UNSET,
                ItemGiveGameDataMan = Array.Empty<int>(),
                UnknItemDisplayPtr = Array.Empty<int>(),
                PlayerNameOffset = 0xA8,
                PlayerBaseMiscOffset = new int[2] { 0xA8, 0xC0 },
                NoGrav = new int[2] { 0xD0, 0x100 },
            };

            // Func AoBs:
            Func = new()
            {
                ItemGiveFunc = "48 89 5C 24 18 56 57 41 56 48 83 EC 30 45 8B F1 41",
                RemoveSoulsFuncAoB = "44 8b 81 ec 00 00 00 41 3b d0 73 05 44 2b c2 eb 03",
                ItemStruct2dDisplay = "40 53 48 83 EC 20 45 33 D2 45 8B D8 48 8B D9 44 89 11",
                GiveSoulsFuncAoB = "48 83 EC 28 48 8b 01 48 85 C0 74 23 48 8b 80 b8 00 00 00",
                SetWarpTargetFuncAoB = "48 89 5C 24 08 48 89 74 24 20 57 48 83 EC 60 0F B7 FA",
                WarpFuncAoB = "40 53 48 83 EC 60 8B 02 48 8B D9 89 01 8B 42 04",
                BaseBAoB = "48 8B 0D ? ? ? ? 48 85 C9 74 ? 48 8B 49 18 E8",
                CameraAoB = "60 02 2C F0 F3 7F 00 00",
                SpeedFactorAccelOffset = "F3 0F 59 9F A8 02 00 00 F3 0F 10 16",
                SpeedFactorAnimOffset = "F3 0F 59 99 A8 02 00 00",
                SpeedFactorJumpOffset = "F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E0 00 00 00",
                SpeedFactorBuildupOffset = "F3 0F 59 99 A8 02 00 00 F3 0F 10 12 F3 0F 10 42 04 48 8B 89 E8 03 00 00",
                DisableSkirtAOB = "89 84 8B C4 01 00 00",
                
                


                // sub-versioned
                //DisplayItem = AOB_UNSET, 
                //ApplySpEffectAoB = AOB_UNSET,
            };
        }
    }
}
