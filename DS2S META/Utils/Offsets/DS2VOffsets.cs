using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    internal class DS2VOffsets : DS2HookOffsets
    {
        public DS2VOffsets()
        {
            // BaseA
            BaseAAob = "8B F1 8B 0D ? ? ? 01 8B 01 8B 50 28 FF D2 84 C0 74 0C";
            BaseABabyJumpAoB = "8b 48 6c 53 8b 58 70 8b 40 68 89 45 0c a1 f4 93 54 01 56";
            BasePtrOffset1 = 0x4;
            BasePtrOffset2 = 0x0;


            // Records    
            PlayerName = new(0xA4);
            ForceQuit = new(0xDF1);
            PlayerCtrl = new(HP: 0xFC, HPMin: 0x100, HPMax: 0x104, HPCap: 0x108, SP: 0x140, SPMax: 0x148, SpeedModifier: 0x208);
            PlayerBaseMisc = new(0xE4, 0xE8, UNSET);
            PlayerEquipment = new(0x1F8, 0x1DC, 0x1C0, 0x1A4,
                                  0xC8, 0xF4, 0x120,
                                  0x44, 0x70, 0x9C);
            PlayerParam = new(0xF0, 0xF8, 0x38, 0xE8,
                              0x1A0, 0x1A8, 0x1D2, 0x1D3);
            Attributes = new(0xCC, 0x4, 0x6, 0x8, 0xA, 0xC, 0xE, 0x14, 0x10, 0x12);


            Covenants = new()
            {
                CurrentCovenant = 0x1A9,
                HeirsOfTheSunDiscovered = 0x1AB,
                HeirsOfTheSunRank = 0x1B5,
                HeirsOfTheSunProgress = 0x1C0,
                BlueSentinelsDiscovered = 0x1AC,
                BlueSentinelsRank = 0x1B6,
                BlueSentinelsProgress = 0x1C2,
                BrotherhoodOfBloodDiscovered = 0x1AD, // possibly wrong (0x1AA alternative)
                BrotherhoodOfBloodRank = 0x1B7, // also possibly wrong (0x1B4 alternative)
                BrotherhoodOfBloodProgress = 0x1C4, // right I think (but maybe not) (0x1C7 alternative)
                WayOfTheBlueDiscovered = 0x1AE,
                WayOfTheBlueRank = 0x1B8,
                WayOfTheBlueProgress = 0x1C6,
                RatKingDiscovered = 0x1AF,
                RatKingRank = 0x1B9,
                RatKingProgress = 0x1C8,
                BellKeepersDiscovered = 0x1B0,
                BellKeepersRank = 0x1BA,
                BellKeepersProgress = 0x1CA,
                DragonRemnantsDiscovered = 0x1B1,
                DragonRemnantsRank = 0x1BB,
                DragonRemnantsProgress = 0x1CC,
                CompanyOfChampionsDiscovered = 0x1B2,
                CompanyOfChampionsRank = 0x1BC,
                CompanyOfChampionsProgress = 0x1CE,
                PilgrimsOfDarknessDiscovered = 0x1B3,
                PilgrimsOfDarknessRank = 0x1BD,
                PilgrimsOfDarknessProgress = 0x1D0
            };
            Gravity = new(0xFC);
            
            PlayerMapData = new()
            {
                WarpBase = 0x120,
                WarpY1 = 0x120,
                WarpZ1 = 0x124,
                WarpX1 = 0x128,
                WarpY2 = 0x130,
                WarpZ2 = 0x134,
                WarpX2 = 0x138,
                WarpY3 = 0x140,
                WarpZ3 = 0x144,
                WarpX3 = 0x148
            };
            Bonfire = new(0xB4, 0xBC);
            BonfireLevels = new()
            {
                TheFarFire = 0x12,
                FireKeepersDwelling = 0x2,
                CrestfallensRetreat = 0x42,
                CardinalTower = 0x22,
                SoldiersRest = 0x32,
                ThePlaceUnbeknownst = 0x52,
                HeidesRuin = 0x322,
                TowerofFlame = 0x312,
                TheBlueCathedral = 0x332,
                UnseenPathtoHeide = 0x1B2,
                ExileHoldingCells = 0x102,
                McDuffsWorkshop = 0x132,
                ServantsQuarters = 0x142,
                StraidsCell = 0xF2,
                TheTowerApart = 0x112,
                TheSaltfort = 0x152,
                UpperRamparts = 0x122,
                UndeadRefuge = 0x242,
                BridgeApproach = 0x252,
                UndeadLockaway = 0x262,
                UndeadPurgatory = 0x272,
                PoisonPool = 0x182,
                TheMines = 0x162,
                LowerEarthenPeak = 0x172,
                CentralEarthenPeak = 0x192,
                UpperEarthenPeak = 0x1A2,
                ThresholdBridge = 0x1D2,
                IronhearthHall = 0x1C2,
                EygilsIdol = 0x1E2,
                BelfrySolApproach = 0x1F2,
                OldAkelarre = 0x302,
                RuinedForkRoad = 0x342,
                ShadedRuins = 0x352,
                GyrmsRespite = 0x362,
                OrdealsEnd = 0x372,
                RoyalArmyCampsite = 0xB2,
                ChapelThreshold = 0xC2,
                LowerBrightstoneCove = 0xA2,
                HarvalsRestingPlace = 0x392,
                GraveEntrance = 0x382,
                UpperGutter = 0x2D2,
                CentralGutter = 0x2B2,
                BlackGulchMouth = 0x2A2,
                HiddenChamber = 0x2C2,
                KingsGate = 0x202,
                ForgottenChamber = 0x222,
                UnderCastleDrangleic = 0x232,
                CentralCastleDrangleic = 0x212,
                TowerofPrayer = 0x62,
                CrumbledRuins = 0x72,
                RhoysRestingPlace = 0x82,
                RiseoftheDead = 0x92,
                UndeadCryptEntrance = 0x292,
                UndeadDitch = 0x282,
                Foregarden = 0xD2,
                RitualSite = 0xE2,
                DragonAerie = 0x2E2,
                ShrineEntrance = 0x2F2,
                SanctumWalk = 0x3A2,
                TowerofPrayerDLC = 0x402,
                PriestessChamber = 0x3B2,
                HiddenSanctumChamber = 0x3D2,
                LairoftheImperfect = 0x3E2,
                SanctumInterior = 0x3F2,
                SanctumNadir = 0x3C2,
                ThroneFloor = 0x412,
                UpperFloor = 0x432,
                Foyer = 0x422,
                LowermostFloor = 0x452,
                TheSmelterThrone = 0x462,
                IronHallwayEntrance = 0x442,
                OuterWall = 0x472,
                AbandonedDwelling = 0x482,
                InnerWall = 0x4C2,
                LowerGarrison = 0x492,
                ExpulsionChamber = 0x4B2,
                GrandCathedral = 0x4A2
            };
            Connection = new(UNSET, 0x54);
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
                PlayerTypeOffset = 0x90,
                GameDataManagerOffset = 0x60,
                AvailableItemBagOffset = 0x8,
                ItemGiveWindowPointer = 0xCC4,
                PlayerCtrlOffset = 0x74,
                NetSvrBloodstainManagerOffset1 = 0x90,
                NetSvrBloodstainManagerOffset2 = 0x28,
                NetSvrBloodstainManagerOffset3 = 0x88,
                PlayerParamOffset = 0x378,
                PlayerPositionOffset1 = 0xB4,
                PlayerPositionOffset2 = 0xA8,
                PlayerDataMapOffset = new int[6] { 0x74, 0xB8, 0x8, 0x14, 0x1B0, 0x10 },
                //PlayerMapDataOffset2 = 0x1B0,
                //PlayerMapDataOffset3 = 0x10,
                SpEffectCtrlOffset = 0x308,
                CharacterFlagsOffset = 0x490,
                EventManagerOffset = 0x44,
                WarpManagerOffset = 0x2C,
                BonfireLevelsOffset1 = 0x2c,
                BonfireLevelsOffset2 = 0x10,
                ConnectionOffset = UNSET,
                CameraOffset1 = 0x0,
                CameraOffset2 = 0x20,
                CameraOffset3 = 0x28,
                //GravityOffset1 = 0xB8,
                //GravityOffset2 = 0x8,
                //EquipmentOffset1 = 0x2D4,
                //EquipmentOffset2 = 0x14,
                //GameDataManagerOffset = 0x60,
                //SpEfCtrl2 = 0xF8,
                ItemGiveGameDataMan = new int[5] { 0x60, 0x10, 0x94, 0x298, 0x248 },
                UnknItemDisplayPtr = new int[4] { 0x60, 0x10, 0x94, 0x298 },
                //UnknItemDisplayPtr = new int[6] {0x18, 0x2a8, 0x94, 0xc30, 0x698, 0x38}
                PlayerNameOffset = 0x60,
                PlayerBaseMiscOffset = new int[1] { 0x60 },
                NoGrav = new int[3] { 0x74, 0xB8, 0x8 } // this is really only for gravity
            };

            // Func AOBs
            Func = new()
            {
                ItemGiveFunc = "55 8B EC 83 EC 10 53 8B 5D 0C 56 8B 75 08 57 53 56 8B F9",
                ItemStruct2dDisplay = "55 8B EC 8B 45 08 0F 57 C0 8B 4D 14 53",
                GiveSoulsFuncAoB = "55 8B EC 8B 01 83 EC 08 85 C0 74 20 8B 80 94 00 00 00",
                RemoveSoulsFuncAoB = "55 8b ec 8b 81 e8 00 00 00 8b 55 08 83 ec 08 3b d0 73 04",
                SetWarpTargetFuncAoB = "55 8B EC 83 EC 44 53 56 8B 75 0C 57 56 8D 4D 0C",
                WarpFuncAoB = "55 8B EC 83 EC 40 53 56 8B 75 08 8B D9 57 B9 10 00 00 00",
                BaseBAoB = "89 45 98 A1 ? ? ? ? 89 7D 9C 89 BD ? ? ? ? 85 C0",
                CameraAoB = "60 02 2c f0 f3 7f 00 00",
                SpeedFactorAccelOffset = "F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9",
                SpeedFactorAnimOffset = "F3 0F 10 89 08 02 00 00 8B 89 B4 00 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D F4 0F 5A C0 89",
                SpeedFactorJumpOffset = "F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D F4 0F 5A C0 0F 5A C9 F2 0F 59 C1 66 0F 5A C0 F3 0F 11 45 F4",
                SpeedFactorBuildupOffset = "F3 0F 10 8E 08 02 00 00 0F 5A C0 0F 5A C9 F2 0F 59 C8 0F 57 C0 66 0F 5A C1 F3 0F 10 4D EC",
                DisplayItem = "83 c4 10 8d 95 6c fe ff ff 52 8b ce e8 ? ? ? ?", // rel call!
            };
        }


        // applyspef2: 89 6c 24 fc 8d 64 24 fc 54 5d 8b 45 08

        //#region Param

        //public enum Param
        //{
        //    TotalParamLength = 0x0,
        //    ParamName = 0xC,
        //    TableLength = 0x30
        //}

        //public const int ParamDataOffset1 = 0x18;
        //public const int ParamDataOffset2 = 0x154;
        //public const int ParamDataOffset3 = 0x60;
        //public const int ParamDataOffset4 = 0x94;

        //public const int LevelUpSoulsParamOffset = 0x2B8;
        //public const int WeaponParamOffset = 0x208;
        //public enum WeaponParam
        //{
        //    ReinforceID = 0x8
        //}

        //public const int WeaponReinforceParamOffset = 0x238;
        //public enum WeaponReinforceParam
        //{
        //    MaxUpgrade = 0x48,
        //    CustomAttrID = 0xE8
        //}
        //public const int CustomAttrSpecParamOffset = 0x270;

        //public const int ArmorParamOffset = 0x4A0;
        //public enum ArmorParam
        //{
        //    ReinforceID = 0x8
        //}
        //public const int ArmorReinforceParamOffset = 0x258;
        //public enum ArmorReinforceParam
        //{
        //    MaxUpgrade = 0x60,
        //}

        //public const int ItemParamOffset = 0x10;
        //public enum ItemParam
        //{
        //    ItemUsageID = 0x44,
        //    MaxHeld = 0x4A
        //}

        //public const int ItemUsageParamOffset = 0x20;
        //public enum ItemUasgeParam
        //{
        //    Bitfield = 0x6
        //}


    }
}

