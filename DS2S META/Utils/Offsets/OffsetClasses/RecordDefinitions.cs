using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    public readonly record struct PlayerCtrlOffsets(int HP, int HPMin, int HPMax, int HPCap, int SP, int SPMax, int SpeedModifier);
    public readonly record struct PlayerName(int Name);
    public readonly record struct ForceQuit(int Quit);
    public readonly record struct PlayerType(int ChrNetworkPhantomId, int TeamType, int CharType);
    public readonly record struct PlayerBaseMisc(int Class, int NewGame, int SaveSlot);
    public readonly record struct PlayerEquipment(int Legs, int Arms, int Chest, int Head,
                                         int RightHand1, int RightHand2, int RightHand3,
                                         int LeftHand1, int LeftHand2, int LeftHand3);
    public readonly record struct NetSvrBloodstainManager(int BloodstainY, int BloodstainX, int BloodstainZ);
    public readonly record struct PlayerParam(int SoulMemory, int SoulMemory2, int MaxEquipLoad, int Souls,
                                     int TotalDeaths, int HollowLevel, int SinnerLevel, int SinnerPoints);
    public readonly record struct Attributes(int SoulLevel, int VGR, int END, int VIT, int ATN, int STR, int DEX, int ADP, int INT, int FTH);
    public readonly record struct PlayerPosition(int PosY, int PosZ, int PosX, int AngY, int AngZ, int AngX);
    public readonly record struct Gravity(int GravityFlag);
    public readonly record struct PlayerMapData(int WarpBase, int WarpY1, int WarpZ1, int WarpX1,
                                                int WarpY2, int WarpZ2, int WarpX2,
                                                int WarpY3, int WarpZ3, int WarpX3);
    public readonly record struct Bonfire(int LastSetBonfireAreaID, int LastSetBonfire);
    public readonly record struct Connection(int Online, int ConnectionType);
    
    // Larger data structures:
    public readonly record struct BonfireLevels
    (
        int FireKeepersDwelling,
        int TheFarFire,
        int CrestfallensRetreat,
        int CardinalTower,
        int SoldiersRest,
        int ThePlaceUnbeknownst,
        int HeidesRuin,
        int TowerofFlame,
        int TheBlueCathedral,
        int UnseenPathtoHeide,
        int ExileHoldingCells,
        int McDuffsWorkshop,
        int ServantsQuarters,
        int StraidsCell,
        int TheTowerApart,
        int TheSaltfort,
        int UpperRamparts,
        int UndeadRefuge,
        int BridgeApproach,
        int UndeadLockaway,
        int UndeadPurgatory,
        int PoisonPool,
        int TheMines,
        int LowerEarthenPeak,
        int CentralEarthenPeak,
        int UpperEarthenPeak,
        int ThresholdBridge,
        int IronhearthHall,
        int EygilsIdol,
        int BelfrySolApproach,
        int OldAkelarre,
        int RuinedForkRoad,
        int ShadedRuins,
        int GyrmsRespite,
        int OrdealsEnd,
        int RoyalArmyCampsite,
        int ChapelThreshold,
        int LowerBrightstoneCove,
        int HarvalsRestingPlace,
        int GraveEntrance,
        int UpperGutter,
        int CentralGutter,
        int HiddenChamber,
        int BlackGulchMouth,
        int KingsGate,
        int UnderCastleDrangleic,
        int ForgottenChamber,
        int CentralCastleDrangleic,
        int TowerofPrayer,
        int CrumbledRuins,
        int RhoysRestingPlace,
        int RiseoftheDead,
        int UndeadCryptEntrance,
        int UndeadDitch,
        int Foregarden,
        int RitualSite,
        int DragonAerie,
        int ShrineEntrance,
        int SanctumWalk,
        int PriestessChamber,
        int HiddenSanctumChamber,
        int LairoftheImperfect,
        int SanctumInterior,
        int TowerofPrayerDLC,
        int SanctumNadir,
        int ThroneFloor,
        int UpperFloor,
        int Foyer,
        int LowermostFloor,
        int TheSmelterThrone,
        int IronHallwayEntrance,
        int OuterWall,
        int AbandonedDwelling,
        int ExpulsionChamber,
        int InnerWall,
        int LowerGarrison,
        int GrandCathedral
    );
    public readonly record struct Covenants(
        int CurrentCovenant,
        int HeirsOfTheSunDiscovered,
        int HeirsOfTheSunRank,
        int HeirsOfTheSunProgress,
        int BlueSentinelsDiscovered,
        int BlueSentinelsRank,
        int BlueSentinelsProgress,
        int BrotherhoodOfBloodDiscovered,
        int BrotherhoodOfBloodRank,
        int BrotherhoodOfBloodProgress,
        int WayOfTheBlueDiscovered,
        int WayOfTheBlueRank,
        int WayOfTheBlueProgress,
        int RatKingDiscovered,
        int RatKingRank,
        int RatKingProgress,
        int BellKeepersDiscovered,
        int BellKeepersRank,
        int BellKeepersProgress,
        int DragonRemnantsDiscovered,
        int DragonRemnantsRank,
        int DragonRemnantsProgress,
        int CompanyOfChampionsDiscovered,
        int CompanyOfChampionsRank,
        int CompanyOfChampionsProgress,
        int PilgrimsOfDarknessDiscovered,
        int PilgrimsOfDarknessRank,
        int PilgrimsOfDarknessProgress
        );
    public readonly record struct Camera
    (
        int CamStart,
        int CamStart2,
        int CamStart3,
        int CamX,
        int CamZ,
        int CamY
    );
    public readonly record struct Core
    (
        int PlayerTypeOffset,
        int PlayerNameOffset,
        int GameDataManagerOffset,
        int AvailableItemBagOffset,
        int ItemGiveWindowPointer,
        int PlayerBaseMiscOffset,
        int PlayerCtrlOffset,
        int NetSvrBloodstainManagerOffset1,
        int NetSvrBloodstainManagerOffset2,
        int NetSvrBloodstainManagerOffset3,
        int PlayerParamOffset,
        int PlayerPositionOffset1,
        int PlayerPositionOffset2,
        int PlayerMapDataOffset1,
        int PlayerMapDataOffset2,
        int PlayerMapDataOffset3,
        int SpEffectCtrlOffset,
        int CharacterFlagsOffset,
        int EventManagerOffset,
        int WarpManagerOffset,
        int BonfireLevelsOffset1,
        int BonfireLevelsOffset2,
        int ConnectionOffset,
        int CameraOffset1,
        int CameraOffset2,
        int CameraOffset3,
        
        int SpEfCtrl2,
        int[] ItemGiveGameDataMan,
        int[] UnknItemDisplayPtr
    );

    public record class Func
    {
        public string ItemGiveFunc { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string ItemStruct2dDisplay { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string GiveSoulsFuncAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string SetWarpTargetFuncAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string WarpFuncAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string BaseBAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string CameraAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string SpeedFactorAccelOffset { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string SpeedFactorAnimOffset { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string SpeedFactorJumpOffset { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string SpeedFactorBuildupOffset { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string DisplayItem { get; set; } = DS2HookOffsets.AOB_UNSET;
        public string ApplySpEffectAoB { get; set; } = DS2HookOffsets.AOB_UNSET;
    };
}
