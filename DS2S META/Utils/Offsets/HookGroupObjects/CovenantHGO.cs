using DS2S_META.Utils.DS2Hook;
using DS2S_META.Utils.Offsets.OffsetClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public class CovenantHGO : HGO
    {
        public Dictionary<COV, Covenant> GameCovenantData { get; set; } = new();

        // Direct Hook properties
        private readonly Dictionary<COV, string> REDataNames = new()
        {
            { COV.HEIRSOFTHESUN, "HeirsOfTheSun" },
            { COV.BLUESENTINELS, "BlueSentinels" },
            { COV.BROTHERHOODOFBLOOD, "BrotherhoodOfBlood" },
            { COV.WAYOFTHEBLUE, "WayOfTheBlue" },
            { COV.RATKING, "RatKing" },
            { COV.BELLKEEPERS, "BellKeepers" },
            { COV.DRAGONREMNANTS, "DragonRemnants" },
            { COV.COVENANTOFCHAMPIONS, "CompanyOfChampions" },
            { COV.PILGRIMSOFDARKNESS, "PilgrimsOfDarkness" },
        };
        public Dictionary<COV, PHLeaf?> DiscovDict = new();
        public Dictionary<COV, PHLeaf?> RankDict = new();
        public Dictionary<COV, PHLeaf?> ProgressDict = new();
        public PHLeaf? PHCurrentCovenant;

        public CovenantHGO(DS2SHook hook, Dictionary<string, PHLeaf?> covGroup, Dictionary<string, PHLeaf?> leafdict) : base(hook)
        {
            foreach (var kvp in REDataNames)
            {
                covGroup.TryGetValue($"{kvp.Value}Discovered", out var phleaf);
                DiscovDict.Add(kvp.Key, phleaf);
                covGroup.TryGetValue($"{kvp.Value}Rank", out var phleafrank);
                RankDict.Add(kvp.Key, phleafrank);
                covGroup.TryGetValue($"{kvp.Value}Progress", out var phleafprog);
                ProgressDict.Add(kvp.Key, phleafprog);
            }
            PHCurrentCovenant = leafdict["CurrentCovenant"];
        }

        public byte CurrentCovenant
        {
            get => PHCurrentCovenant?.ReadByte() ?? 0;
            set => PHCurrentCovenant?.WriteByte(value);
        }

        // Helpers:
        public bool GetCovenantDiscov(COV id) => DiscovDict[id]?.ReadBoolean() ?? false;
        public byte GetCovenantRank(COV id) => RankDict[id]?.ReadByte() ?? 0;
        public short GetCovenantProgress(COV id) => ProgressDict[id]?.ReadInt16() ?? 0;
        //
        public void SetCovenantDiscov(COV id, bool discov) => DiscovDict[id]?.WriteBoolean(discov);
        public void SetCovenantRank(COV id, int rank) => RankDict[id]?.WriteByte((byte)rank);
        public void SetCovenantProgress(COV id, int prog) => ProgressDict[id]?.WriteInt16((short)prog);
        
        public Covenant GetCovenantData(COV id)
        {
            var disc =  GetCovenantDiscov(id);
            var rank = GetCovenantRank(id);
            var progress = GetCovenantProgress(id);
            return new Covenant(id, disc, rank, progress);
        }

        public override void UpdateProperties()
        {
            // update dictionary of data from game
            GameCovenantData = REDataNames.ToDictionary(kvp => kvp.Key, kvp => GetCovenantData(kvp.Key));
            OnPropertyChanged(nameof(CurrentCovenant));
        }
    }
}
