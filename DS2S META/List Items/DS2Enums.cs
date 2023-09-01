using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    public enum ITEMID
    {
        NONE = 0,
        ESTUS           = 0x0395E478,
        ESTUSSHARD      = 0x039B89C8,
        ESTUSEMPTY      = 0x0395E860,
        BONEDUST        = 0x039B8DB0,
        DARKSIGN        = 0x03990540,
        AGEDFEATHER     = 0x0398F1B8,
        BONEOFORDER     = 0x03B259A0,
        KEYTOEMBEDDED   = 0x001E3660,
        BINOCULARS      = 0x005D1420,
        RAPIER          = 0x16E360,
        RINGOFBINDING   = 0x02689B90,
        CATRING         = 0x0268C2A0,
        BLACKSEPCRYSTAL = 0x03B47C80,
        DRAKEWINGUGS    = 0x004FCDB0,
        ELEUMLOYCE      = 0x0018DF30,
        HUMANEFFIGY     = 0x0395D4D8,
        YEARN           = 0x01DC6120,
        FLYNNSRING      = 0x027322E0,
        DARKWEAPON      = 0x0207B6E0,
        TITANITECHUNK   = 0x03A27B20,
        TITANITESLAB    = 0x03A2A230,
        CRYSTALSOULSPEAR = 0x01DA8C60,
        TORCH           = 0x0399EFA0,
        FLAMEBUTTERFLY  = 0x39A16B0,
        DULLEMBER       = 0x030A0BB0,
        LENIGRASTKEY    = 0x030836F0,
        FANGKEY         = 0x0307E8D0,
        FRAGRANTBRANCH  = 0x039BB8A8,
        ASHENMIST       = 0x0308D330,
        KINGSRING       = 0x026A2230,
        ROTUNDALOCKSTONE = 0x03088510,
        BASTILLEKEY     = 0x03072580,
        LADDERMINIATURE = 0x03096F70,
        TOKENOFFIDELITY = 0x03B39220,
        TOKENOFSPITE    = 0x03B3B930,
        SOLDIERKEY      = 0x3041840,
        FORGOTTENKEY    = 0x30773A0,
        TOWERKEY        = 0x31F8F80,
        GIANTSKINSHIP   = 0x308AC20,
        GARRISONWARDKEY = 0x3211620,
        ALDIASKEY       = 0x30AA7F0,
        SCORCHINGSCEPTER = 0x32A3DE0,
        DRAGONSTONE     = 0x3236010,
        DRAGONTALON     = 0x3197500,
        HEAVYIRONKEY    = 0x31AFBA0,
        FROZENFLOWER    = 0x31C8240,
        KINGSPASSAGEKEY = 0x3043F50,
        ETERNALSANCTUMKEY = 0x31E08E0,
        UNDEADLOCKAWAYKEY = 0x309BD90,
        IRONKEY         = 0x3074C90,
        ANTIQUATEDKEY   = 0x307C1C0,
        HOUSEKEY        = 0x3080FE0,
        BRIGHTSTONEKEY  = 0x3079AB0,
        TSELDORADENKEY  = 0x3092150,
        PHARROSLOCKSTONE = 0x39BB4C0,
        PETRIFIEDEGG    = 0x3B4F1B0,
        RINGOFWHISPERS  = 0x26BA8D0,
        SOULOFAGIANT    = 0x308FA40,
        CRUSHEDEYEORB   = 0x30A32C0,
        SMELTERWEDGE    = 0x32BC480,
        NADALIAFRAGMENT = 0x32D4B20,
        FISTS           = 0x33E140,
    }
    public enum ITEMUSAGE
    {
        BOSSSOULUSAGE = 2000,
        ITEMUSAGEKEY = 2700,
        SOULUSAGE = 1900,
    }
    public enum CHRID
    {
        PLAYER = 100,
        TARGRAY = 785000,
        MADWARRIOR = 836000,
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
    public enum SPECIAL_EFFECT
    {
        RESTOREHUMANITY = 100000010,
        BONFIREREST = 110000010,
        AREALOAD_POSSIBLY = 130000010,
    }

}
