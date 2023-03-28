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
        FAIL_PICKUPTYPE,
        FAIL_SOFTLOCK,
        FAIL_VAN_WRONGRDZ,
        FAIL_RESERVED,
        FAIL_DISTANCE_MIN,
        FAIL_DISTANCE_MAX,
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
}
