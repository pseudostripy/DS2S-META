using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public enum ITEMID
    {
        NONE = 0,
        ESTUS           = 0x0395E478,
        ESTUSEMPTY      = 0x0395E860,
        DARKSIGN        = 0x03990540,
        BONEOFORDER     = 0x03B259A0,
        KEYTOEMBEDDED   = 0x001E3660,
        BINOCULARS      = 0x005D1420,
        RAPIER          = 0x16E360,
        TORCH           = 0x0399EFA0,
        RINGOFBINDING   = 0x02689B90,
        CATRING         = 0x0268C2A0,
        AGEDFEATHER     = 0x0398F1B8,
        DULLEMBER       = 0x030A0BB0,
        KINGSRING       = 0x026A2230,
        ASHENMIST       = 0x0308D330,
        LENIGRASTKEY    = 0x030836F0,
        FANGKEY         = 0x0307E8D0,
        BLACKSEPCRYSTAL = 0x03B47C80,
        BASTILLEKEY     = 0x03072580,
        ROTUNDALOCKSTONE = 0x03088510,
        DRAKEWINGUGS    = 0x004FCDB0,
        ELEUMLOYCE      = 0x0018DF30,
        HUMANEFFIGY     = 0x0395D4D8,
        YEARN           = 0x01DC6120,
        FLYNNSRING      = 0x027322E0,
        DARKWEAPON      = 0x0207B6E0,
    }
    public enum ITEMUSAGE
    {
        BOSSSOULUSAGE = 2000,
        ITEMUSAGEKEY = 2700,
        SOULUSAGE = 1900,
    }
    public enum LOGICRES
    {
        SUCCESS,
        SUCCESS_VANPLACE,
        SUCCESS_DISTCOMPROMISE,
        DELAY_VANLOCKED, // try again later
        DELAY_MAXDIST, // try again later
        FAIL_PICKUPTYPE,
        FAIL_SOFTLOCK,
        FAIL_VAN_WRONGRDZ,
        FAIL_RESERVED,
        FAIL_DIST_TOONEAR,
        FAIL_DIST_TOOFAR,
        FAIL_DISTANCE_NA,
    }
    public enum MapArea
    {
        Undefined, // For things we don't know
        Quantum, // Doesn't neatly fit in one area
        ThingsBetwixt,
        Majula,
        FOFG, // Forest of Fallen Giants
        HeidesTowerOfFlame,
        NoMansWharf,
        TheLostBastille,
        BelfryLuna,
        SinnersRise,
        HuntsmansCopse,
        UndeadPurgatory,
        HarvestValley,
        EarthenPeak,
        IronKeep,
        BelfrySol,
        ShadedWoods,
        DoorsOfPharros,
        Tseldora,
        ThePit,
        GraveOfSaints,
        TheGutter,
        BlackGulch,
        DrangleicCastle,
        ShrineOfAmana,
        UndeadCrypt,
        AldiasKeep,
        DragonAerie,
        DragonShrine,
        MemoryOfJeigh,
        MemoryOfOrro,
        MemoryOfVammar,
        //DragonMemories,
        ShulvaSanctumCity,
        DragonsSanctum,
        CaveOfTheDead,
        BrumeTower,
        IronPassage,
        FrozenEleumLoyce,
        FrigidOutskirts,
        MemoryOfTheOldIronKing,
        //ShrineOfWinter,
        //CathedralOfBlue,
        //LordsPrivateChamber,
        //KingsPassage,
        //ThroneOfWant,
        //DragonsRest,
        //GrandCathedral,
        //TheOldChaos,
        //PilgrimsOfDark, // Grandahl's shop, covenant rewards and Darklurker drops
        //RatKingCovenant,
        //BellKeepers,
    };
    public enum DROPPARAMID
    {
        LICIAHEIDES = 76900000,
        DUKETSELDORA = 1140300,
    }
    public enum RestrType
    {
        // Item Restriction Type
        Anywhere,
        Vanilla,
        Distance
    }
    public enum ITEMGROUP
    {
        Specified,
        Staff,
        Pyro,
        Chime,
        BlacksmithKey
    }
    
    public static class DS2Data
    {
        public static readonly Dictionary<ITEMGROUP, List<int>> ItemGroups = new()
        {
            [ITEMGROUP.BlacksmithKey] = new List<ITEMID>() { ITEMID.LENIGRASTKEY, ITEMID.DULLEMBER, ITEMID.FANGKEY }.Cast<int>().ToList(),
            [ITEMGROUP.Pyro] = new() { 05400000, 05410000 },
            [ITEMGROUP.Staff] = new() { 1280000, 3800000, 3810000, 3820000, 3830000, 3850000, 3860000, 3870000,
                                            3880000, 3890000, 3900000, 3910000, 3930000, 3940000, 4150000, 5370000,
                                            5540000, 11150000 },
            [ITEMGROUP.Chime] = new() { 2470000, 4010000, 4020000, 4030000, 4040000, 4050000, 4060000, 4080000,
                                            4090000, 4100000, 4110000, 4120000, 4150000, 11150000 },
        };
    }

}
