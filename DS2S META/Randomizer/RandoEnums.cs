using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public enum LOGICRES
    {
        SUCCESS,
        SUCCESS_IN_LOGIC,
        SUCCESS_VANPLACE,
        SUCCESS_NO_CONSTRAINTS,
        SUCCESS_DISTCOMPROMISE,
        DELAY_VANLOCKED, // try again later
        DELAY_MAXDIST, // try again later
        FAIL_PICKUPTYPE,
        FAIL_SOFTLOCK,
        FAIL_VAN_WRONGRDZ,
        FAIL_RESERVED,
        FAIL_DIST_TOO_NEAR,
        FAIL_DIST_TOO_FAR,
        FAIL_DIST_NOTAPPLICABLE,
        FAIL_DIST_NODELOCKED,
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
