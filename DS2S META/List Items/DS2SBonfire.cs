using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    // Enum is simply for easier-to-read code inside meta
    public enum BF : int
    {
        GAMESTART = 0,
        BETWIXT = 2650,
        MAJULA = 4650,
        CRESTFALLENSRETREAT = 10670,
        CARDINALTOWER = 10655,
        SOLDIERSREST = 10660,
        THEPLACEUNBEKNOWNST = 10675,
        HEIDERUIN = 31655,
        TOWEROFFLAME = 31650,
        BLUECATHEDRAL = 31660,
        UNSEENPATHTOHEIDE = 18650,
        EXILEHOLDINGCELLS = 16655,
        MCDUFFSWORKSHOP = 16670,
        SERVANTSQUARTERS = 16675,
        STRAIDSCELL = 16650,
        THETOWERAPART = 16660,
        THESALTFORT = 16685,
        UPPERRAMPARTS = 16665,
        UNDEADREFUGE = 23650,
        BRIDGEAPPROACH = 23655,
        UNDEADLOCKAWAY = 23660,
        UNDEADPURGATORY = 23665,
        POISONPOOL = 17665,
        THEMINES = 17650,
        LOWEREARTHERNPEAK = 17655,
        CENTRALEARTHERNPEAK = 17670,
        UPPEREARTHERNPEAK = 17675,
        THRESHOLDBRIDGE = 19655,
        IRONHEARTHHALL = 19650,
        EYGILSIDOL = 19660,
        BELFRYSOLAPPROACH = 19665,
        OLDAKELARRE = 29650,
        RUINEDFORKROAD = 32655,
        SHADEDRUINS = 32660,
        GYRMSRESPITE = 33655,
        ORDEALSEND = 33660,
        ROYALARMYCAMPSITE = 14655,
        CHAPELTHRESHOLD = 14660,
        LOWERBRIGHTSTONECOVE = 14650,
        HARVALSRESTINGPLACE = 34655,
        GRAVEENTRANCE = 34650,
        UPPERGUTTER = 25665,
        CENTRALGUTTER = 25655,
        BLACKGULCHMOUTH = 25650,
        HIDDENCHAMBER = 25660,
        KINGSGATE = 21650,
        UNDERDRANGLEIC = 21665,
        CENTRALDRANGLEIC = 21655,
        FORGOTTENCHAMBER = 21660,
        TOWEROFPRAYER_AMANA = 11650,
        CRUMBLEDRUINS = 11655,
        RHOYSRESTINGPLACE = 11660,
        RISEOFTHEDEAD = 11670,
        UNDEADCRYPTENTRANCE = 24655,
        UNDEADDITCH = 24650,
        FOREGARDEN = 15650,
        RITUALSITE = 15655,
        DRAGONAERIE = 27650,
        SHRINEENTRANCE = 27655,
        SANCTUMWALK = 35650,
        TOWEROFPRAYER_SHULVA = 35685,
        PRIESTESSCHAMBER = 35655,
        HIDDENSANCTUMCHAMBER = 35670,
        LAIROFTHEIMPERFECT = 35675,
        SANCTUMINTERIOR = 35680,
        SANCTUMNADIR = 35665,
        THRONEFLOOR = 36650,
        UPPERFLOOR = 36660,
        FOYER = 36655,
        LOWERMOSTFLOOR = 36670,
        THESMELTERTHRONE = 36675,
        IRONHALLWAYENTRANCE = 36665,
        OUTERWALL = 37650,
        ABANDONEDDWELLING = 37660,
        EXPULSIONCHAMBER = 37675,
        INNERWALL = 37685,
        LOWERGARRISON = 37665,
        GRANDCATHEDRAL = 37670,
    }

    public class DS2SBonfire : IComparable<DS2SBonfire>
    {
        public ushort ID;
        public string Name;
        public int AreaID;
        public DS2SBonfireHub? Hub { get; set; } // parent Hub

        public static readonly int _GameStartId = 0;
        public static readonly DS2SBonfire EmptyBonfire = new(0, 0, "not found");
        public DS2SBonfire(int areaId, ushort id, string name)
        {
            ID = id;
            Name = name;
            AreaID = areaId;
        }
        public override string ToString() => Name;
        public int CompareTo(DS2SBonfire? other) => Name.CompareTo(other?.Name);
    }
}
