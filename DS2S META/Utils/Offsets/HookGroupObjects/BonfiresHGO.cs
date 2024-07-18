using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.OffsetClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class BonfiresHGO : HGO
    {
        // Direct Hook properties
        private readonly Dictionary<string, PHLeaf?> PHBonfires;
        private readonly PHLeaf? PHLastBonfireId;
        private readonly PHLeaf? PHLastBonfireAreaId;

        // Interface dictionaries
        public Dictionary<string, int> BonfireLevels { get; set; } = new();

        public BonfiresHGO(DS2SHook hook, Dictionary<string, PHLeaf?> bfLvlsGroup, Dictionary<string, PHLeaf?> lastbfGroup) : base(hook)
        {
            PHBonfires = bfLvlsGroup;
            PHLastBonfireId = lastbfGroup["LastSetBonfire"];
            PHLastBonfireAreaId = lastbfGroup["LastSetBonfireAreaID"];
        }
        public Dictionary<int, string> BfNames = new()
        {
            {  2650, "FireKeepersDwelling" },
            {  4650, "TheFarFire" },
            { 10670, "CrestfallensRetreat" },
            { 10655, "CardinalTower" },
            { 10660, "SoldiersRest" },
            { 10675, "ThePlaceUnbeknownst" },
            { 31655, "HeidesRuin" },
            { 31650, "TowerOfFlame" },
            { 31660, "TheBlueCathedral" },
            { 18650, "UnseenPathToHeide" },
            { 16655, "ExileHoldingCells" },
            { 16670, "McDuffsWorkshop" },
            { 16675, "ServantsQuarters" },
            { 16650, "StraidsCell" },
            { 16660, "TheTowerApart" },
            { 16685, "TheSaltfort" },
            { 16665, "UpperRamparts" },
            { 23650, "UndeadRefuge" },
            { 23655, "BridgeApproach" },
            { 23660, "UndeadLockaway" },
            { 23665, "UndeadPurgatory" },
            { 17665, "PoisonPool" },
            { 17650, "TheMines" },
            { 17655, "LowerEarthenPeak" },
            { 17670, "CentralEarthenPeak" },
            { 17675, "UpperEarthenPeak" },
            { 19655, "ThresholdBridge" },
            { 19650, "IronhearthHall" },
            { 19660, "EygilsIdol" },
            { 19665, "BelfrySolApproach" },
            { 29650, "OldAkelarre" },
            { 32655, "RuinedForkRoad" },
            { 32660, "ShadedRuins" },
            { 33655, "GyrmsRespite" },
            { 33660, "OrdealsEnd" },
            { 14655, "RoyalArmyCampsite" },
            { 14660, "ChapelThreshold" },
            { 14650, "LowerBrightstoneCove" },
            { 34655, "HarvalsRestingPlace" },
            { 34650, "GraveEntrance" },
            { 25665, "UpperGutter" },
            { 25655, "CentralGutter" },
            { 25650, "BlackGulchMouth" },
            { 25660, "HiddenChamber" },
            { 21650, "KingsGate" },
            { 21665, "UnderCastleDrangleic" },
            { 21655, "CentralCastleDrangleic" },
            { 21660, "ForgottenChamber" },
            { 11650, "TowerOfPrayerAmana" },
            { 11655, "CrumbledRuins" },
            { 11660, "RhoysRestingPlace" },
            { 11670, "RiseOfTheDead" },
            { 24655, "UndeadCryptEntrance" },
            { 24650, "UndeadDitch" },
            { 15650, "Foregarden" },
            { 15655, "RitualSite" },
            { 27650, "DragonAerie" },
            { 27655, "ShrineEntrance" },
            { 35650, "SanctumWalk" },
            { 35685, "TowerOfPrayerShulva" },
            { 35655, "PriestessChamber" },
            { 35670, "HiddenSanctumChamber" },
            { 35675, "LairOfTheImperfect" },
            { 35680, "SanctumInterior" },
            { 35665, "SanctumNadir" },
            { 36650, "ThroneFloor" },
            { 36660, "UpperFloor" },
            { 36655, "Foyer" },
            { 36670, "LowermostFloor" },
            { 36675, "TheSmelterThrone" },
            { 36665, "IronHallwayEntrance" },
            { 37650, "OuterWall" },
            { 37660, "AbandonedDwelling" },
            { 37675, "ExpulsionChamber" },
            { 37685, "InnerWall" },
            { 37665, "LowerGarrison" },
            { 37670, "GrandCathedral" },
        };

        public int LastBonfireID
        {
            get => PHLastBonfireId?.ReadUInt16() ?? 0;
            set => PHLastBonfireId?.WriteUInt16((ushort)value);
        }
        public int LastBonfireAreaID
        {
            get => PHLastBonfireAreaId?.ReadInt32() ?? 0;
            set => PHLastBonfireId?.WriteInt32(value);
        }

        // Helpers:
        public int GetBonfireLevel(string bfname)
        {
            var rawlevel = PHBonfires[bfname]?.ReadByte() ?? 0;
            return (rawlevel + 1) / 2;
        } 
        public void SetBonfireLevel(string bfname, int level)
        {
            if (level > 255)
                throw new Exception("Bonfire Level must fit in byte");

            byte rawval = level > 0 ? (byte)(level * 2 - 1) : (byte)0;
            PHBonfires[bfname]?.WriteByte(rawval);
        }
        public void SetBonfireLevelById(int bfid, int level) => SetBonfireLevel(BfNames[bfid], level);
        public void GetBonfireLevelById(int bfid) => GetBonfireLevel(BfNames[bfid]);

        public override void UpdateProperties()
        {
            // update dictionary of data from game
            BonfireLevels = PHBonfires.ToDictionary(kvp => kvp.Key, kvp => GetBonfireLevel(kvp.Key));
            OnPropertyChanged(nameof(LastBonfireID));
            OnPropertyChanged(nameof(LastBonfireAreaID));
        }
    }
}
