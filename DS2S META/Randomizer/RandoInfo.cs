using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Net;

namespace DS2S_META.Randomizer
{
    internal enum KEYID : int
    {
        // Area shorthands:
        NONE = 0x0,                 // default
        BELFRYLUNA      = 0xBB0,    // Branch && Pharros
        SINNERSRISE     = 0xBB1,    // Branch || Antiquated 
        DRANGLEIC       = 0xBB2,    // All conditions for drangleic
        AMANA           = 0xBB3,    // Drangleic + King's passage
        ALDIASKEEP      = 0xBB4,    // Branch + King's Ring
        MEMORYJEIGH     = 0xBB5,    // King's Ring + Ashen Mist
        GANKSQUAD       = 0xBB6,    // DLC1 + Eternal Sanctum
        PUZZLINGSWORD   = 0xBB7,    // TO CONSIDER: Might need range
        ELANA           = 0xBB8,    // DLC1 + Dragon Stone
        FUME            = 0xBB9,    // DLC2 + Scorching Sceptor
        BLUESMELTER     = 0xBBA,    // DLC2 + Tower Key
        ALONNE          = 0xBBB,    // DLC2 + Tower Key + Scorching Scepter + Ashen Mist
        DLC3            = 0xBBC,    // DLC3 + Drangleic
        FRIGIDOUTSKIRTS = 0xBBD,    // DLC3 + Garrison Ward Key
        CREDITS         = 0xBBE,    // Everything required to beat Nash
        VENDRICK        = 0xBBF,    // Amana && SoaG x3
        BELFRYSOL       = 0xBC0,    // Rotunda && BigPharros
        DARKLURKER      = 0xBC1,    // Forgotten && Drangleic && Torch && Butterfly x3
        DLC2            = 0xBC2,    // DLC2 key + Rotunda
        MEMORYORRO      = 0xBC3,    // Soldier Key & Ashen Mist
        EARTHERNPEAK    = 0xBC4,    // Rotunda
        IRONKEEP        = 0xBC5,    // Rotunda
        UNDEADCRYPT     = 0xBC6,    // Drangleic + King's passage [see amana]
        THRONEWANT      = 0xBC7,    // Drangleic + King's ring [Duo only]
        HARVESTVALLEY   = 0xBC8,    // Rotunda
        SHADEDWOODS     = 0xBC9,    // Branch
        TSELDORA        = 0xBCA,    // Branch
        UNDEADPURGATORY = 0xBCB,    // Rotunda
        HUNSTMANSCOPSE  = 0xBCC,    // Rotunda
        
        BRANCH          = 0xAA0,    // Require at least 3 branches
        PHARROS         = 0xAA1,    // Enough Pharros Lockstones available
        ALLWEDGES       = 0xAA2,    // Enough Smelter Wedges
        ALLNADSOULS     = 0xAA3,    // Enough Nadalia Soul Fragments
        TENBRANCHLOCK   = 0xAA4,    // Require at least 10 branches
        BIGPHARROS      = 0xAA5,    // Require at least two lockstones
        MIRRORKNIGHTEVENT = 0xAA6,  // Drangleic & King's passage [see also Amana]
        SHRINEOFWINTER  = 0xAA7,    // SinnersRise & IronKeep & Tseldora
        MEMEPHARROS     = 0xAA8,    // 10 Pharros Lock

        // NPC shorthands:
        HEADVENGARL     = 0xCC0,    // [Currently] FragrantBranch
        AGDAYNE         = 0xCC1,    // Drangleic
        GILLIGAN        = 0xCC2,    // Earthern Peak
        WELLAGER        = 0xCC3,    // Drangleic
        GAVLAN          = 0xCC4,    // NO REQUIREMENT
        CREIGHTON       = 0xCC5,    // Rotunda & UndeadLockaway
        STRAID          = 0xCC6,    // [Currently] see SinnerRise
        CHLOANNE        = 0xCC7,    // Rotunda
        MCDUFF          = 0xCC8,    // DullEmber (currently only)
        ORNIFEX         = 0xCC9,    // Branch & FangKey
        TITCHY          = 0xCCA,    // Rotunda & TokenOfSpite
        TARK            = 0xCCB,    // Branch & Whispers
        //TARGRAY         = 0xCCB,    // 

        // Misc combinations:
        EXECUTIONERSET  = 0xDD0,    // Titchy && ShrineOfWinter
        LICIAINVASION   = 0xDD1,    // CrushedEyeOrb && Drangleic
        NADALIAFULL     = 0xDD2,    // NadaliaFragments && BlueSmelter && Fume
        PLACEUNBENKNOWNST = 0xDD3,  // KingsRing && SoldierKey
        SINNERSCELLS    = 0xDD4,    // SinnerRise && BastilleKey
        ROTUNDAPHARROS  = 0xDD5,    // Rotunda && Pharros
        DRAGONCOVENANT  = 0xDD6,    // IronKeep && PetrifiedEgg
        BRANCHMEMEPHARROS = 0xDD7,  // Branch && LowPharros
        TSELDORADEN     = 0xDD8,    // Tseldora && TseldoraDenKey
        TSELDORAVAULT   = 0xDD9,    // Tseldora && BrightstoneKey
        BENHARTFULLQUEST = 0xDDA,   // Ashen && Drangleic
        ORROPHARROS     = 0xDDB,    // Orro && MemePharros
        FUMEIDOL        = 0xDDC,    // Nadalia && Fume
        FUMETOWERIDOL   = 0xDDD,    // FumeIdol && TowerKey
        SMELTERIDOL     = 0xDDE,    // Nadalia && BlueSmelter
        DLC3CAVE        = 0xDDF,    // DLC3 && Torch
        DLC3PHARROS     = 0xDE0,    // DLC3 && MemePharros
        DRAGONMEMORY    = 0xDE1,    // Tseldora && AshenMist
        WELLAGERQUEST   = 0xDE2,    // AshenMist && Drangleic
        BDSM            = 0xDE3,    // KeyToEmbedded && Drangleic
        AMANAPHARROS    = 0xDE4,    // Amana && MemePharros
        AMANABRANCH     = 0xDE5,    // Amana && MemeBranch
        AGDAYNEGIFT     = 0xDE6,    // Agdayne && KingsRing
        LUCATIELFULLQUEST = 0xDE7,  // AldiasKeep
        NAVLANQUEST     = 0xDE8,    // AldiasKeep && Rotunda
        ALDIASLAB       = 0xDE9,    // AldiasKeep && AldiaKey
        FOURFORLORN     = 0xDEA,    // AldiasKeep && Torch
        BELLKEEPERSCOV  = 0xDEB,    // BelfryLuna || BelfrySol

        // Actual Key Item IDs:
        SOLDIER         = 50600000,
        FORGOTTEN       = 50820000,
        TOWER           = 52400000,
        KINSHIP         = 50900000,
        ASHENMIST       = 50910000,
        GARRISONWARD    = 52500000,
        ALDIASKEY       = 51030000,
        SCEPTER         = 53100000,
        KINGSRING       = 40510000,
        DRAGONSTONE     = 52650000,
        DLC1KEY         = 52000000,
        DLC2KEY         = 52100000,
        DLC3KEY         = 52200000,
        EMBEDDED        = 1980000,
        KINGSPASSAGE    = 50610000,
        ETERNALSANCTUM  = 52300000,
        UNDEADLOCKAWAY  = 50970000,
        BASTILLEKEY     = 50800000,
        IRON            = 50810000,
        ANTIQUATED      = 50840000,
        MANSION         = 50860000,
        BRIGHTSTONE     = 50830000,
        FANG            = 50850000,
        ROTUNDA         = 50890000,
        TSELDORADENKEY  = 50930000,
        BLACKSMITH      = 50870000, // (Lenigrast's key)
        DULLEMBER       = 50990000,
        TORCH           = 60420000,
        PHARROSLOCKSTONE = 60536000,
        FRAGRANTBRANCH  = 60537000,
        PETRIFIEDEGG    = 62190000,
        WHISPERS        = 40610000,
        SOULOFAGIANT    = 50920000,
        CRUSHEDEYEORB   = 51000000,
        SMELTERWEDGE    = 53200000,
        NADALIAFRAGMENT = 53300000,
        TOKENOFFIDELITY = 62100000,
        TOKENOFSPITE    = 62110000,
        FLAMEBUTTERFLY  = 60430000,
    }

    internal static class RandoLogicHelper
    {
        internal static List<KeySet> AddToKSO(List<KeySet> kso, KEYID keyid)
        {
            // Somewhat recursive.
            // Wrapper to make building this up a bit faster.
            switch (keyid)
            {
                case KEYID.NONE:
                    return kso;

                // Single Key requests:
                case KEYID.SOLDIER:
                case KEYID.FORGOTTEN:
                case KEYID.TOWER:
                case KEYID.KINSHIP:
                case KEYID.ASHENMIST:
                case KEYID.GARRISONWARD:
                case KEYID.ALDIASKEY:
                case KEYID.SCEPTER:
                case KEYID.KINGSRING:
                case KEYID.DRAGONSTONE:
                case KEYID.DLC1KEY:
                case KEYID.DLC2KEY:
                case KEYID.DLC3KEY:
                case KEYID.EMBEDDED:
                case KEYID.KINGSPASSAGE:
                case KEYID.ETERNALSANCTUM:
                case KEYID.UNDEADLOCKAWAY:
                case KEYID.BASTILLEKEY:
                case KEYID.IRON:
                case KEYID.ANTIQUATED:
                case KEYID.MANSION:
                case KEYID.BRIGHTSTONE:
                case KEYID.FANG:
                case KEYID.ROTUNDA:
                case KEYID.TSELDORADENKEY:
                case KEYID.BLACKSMITH: // (Lenigrast's key)
                case KEYID.DULLEMBER:
                case KEYID.TORCH:
                case KEYID.PHARROSLOCKSTONE:
                case KEYID.FRAGRANTBRANCH:
                case KEYID.PETRIFIEDEGG:
                case KEYID.WHISPERS:
                case KEYID.SOULOFAGIANT:
                case KEYID.CRUSHEDEYEORB:
                case KEYID.SMELTERWEDGE:
                case KEYID.NADALIAFRAGMENT:
                case KEYID.TOKENOFFIDELITY:
                case KEYID.TOKENOFSPITE:
                case KEYID.FLAMEBUTTERFLY:
                case KEYID.PUZZLINGSWORD:
                    foreach (var ks in kso)
                        ks.Add(keyid);
                    return kso;

                // might change these:
                case KEYID.BRANCH:
                case KEYID.TENBRANCHLOCK:
                case KEYID.PHARROS:
                case KEYID.ALLWEDGES:
                case KEYID.ALLNADSOULS:
                    foreach (var ks in kso)
                        ks.Add(keyid);
                    return kso;


                case KEYID.BELFRYLUNA:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRANCH, KEYID.BIGPHARROS);
                    return kso;

                case KEYID.HUNSTMANSCOPSE:
                case KEYID.UNDEADPURGATORY:
                case KEYID.HARVESTVALLEY:
                case KEYID.CHLOANNE:
                case KEYID.EARTHERNPEAK:
                case KEYID.GILLIGAN:
                case KEYID.IRONKEEP:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ROTUNDA);
                    return kso;

                case KEYID.SHADEDWOODS:
                case KEYID.TSELDORA:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRANCH);
                    return kso;

                case KEYID.SINNERSRISE:
                case KEYID.STRAID:
                    // Need to separate into options:
                    List<KeySet> newkso = new();
                    foreach (var ks in kso)
                    {
                        var ks2 = KeySet.Clone(ks);
                        ks.Add(KEYID.BRANCH);
                        ks2.Add(KEYID.ANTIQUATED);
                        
                        newkso.Add(ks);
                        newkso.Add(ks2);
                    }
                    return newkso;

                case KEYID.SHRINEOFWINTER:
                case KEYID.DRANGLEIC:
                case KEYID.WELLAGER:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ROTUNDA, KEYID.BRANCH);

                    // officially correct:
                    //AddToKSO(kso, KEYID.SINNERSRISE); 

                    // take a shortcut because as a branch is essential, we can
                    // already kill sinner, so its redundant. Chops off a lot 
                    // of KSO duplications. From a Steiner point of view, it'll
                    // never be shorter distance to get to BRANCH + ANTIQUATED
                    // than just BRANCH
                    return kso;

                case KEYID.AMANA:
                case KEYID.MIRRORKNIGHTEVENT:
                case KEYID.UNDEADCRYPT:
                case KEYID.AGDAYNE:
                    foreach (var ks in kso)
                        ks.Add(KEYID.KINGSPASSAGE);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.ALDIASKEEP:
                case KEYID.LUCATIELFULLQUEST:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRANCH, KEYID.KINGSRING);
                    return kso;

                case KEYID.MEMORYJEIGH:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ASHENMIST, KEYID.KINGSRING);
                    return kso;

                case KEYID.GANKSQUAD:
                    foreach (var ks in kso)
                        ks.Add(KEYID.DLC1KEY, KEYID.ETERNALSANCTUM);
                    return kso;

                case KEYID.ELANA:
                    foreach (var ks in kso)
                        ks.Add(KEYID.DLC1KEY, KEYID.DRAGONSTONE);
                    return kso;

                case KEYID.DLC2:
                    foreach (var ks in kso)
                        ks.Add(KEYID.DLC2KEY, KEYID.ROTUNDA);
                    return kso;

                case KEYID.FUME:
                    foreach (var ks in kso)
                        ks.Add(KEYID.SCEPTER);
                    return AddToKSO(kso, KEYID.DLC2);

                case KEYID.BLUESMELTER:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TOWER);
                    return AddToKSO(kso, KEYID.DLC2);

                case KEYID.ALONNE:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TOWER, KEYID.SCEPTER, KEYID.ASHENMIST);
                    return AddToKSO(kso, KEYID.DLC2);

                case KEYID.DLC3:
                    foreach (var ks in kso)
                        ks.Add(KEYID.DLC3KEY);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.FRIGIDOUTSKIRTS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.GARRISONWARD);
                    return AddToKSO(kso, KEYID.DLC3);

                case KEYID.VENDRICK:
                    foreach (var ks in kso)
                        ks.Add(KEYID.SOULOFAGIANT);
                    return AddToKSO(kso, KEYID.UNDEADCRYPT);

                case KEYID.BELFRYSOL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BIGPHARROS);
                    return AddToKSO(kso, KEYID.IRONKEEP);

                case KEYID.DARKLURKER:
                    foreach (var ks in kso)
                        ks.Add(KEYID.FORGOTTEN);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.MEMORYORRO:
                    foreach (var ks in kso)
                        ks.Add(KEYID.SOLDIER, KEYID.ASHENMIST);
                    return kso;

                case KEYID.THRONEWANT:
                    foreach (var ks in kso)
                        ks.Add(KEYID.KINGSRING);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.CREDITS:
                    var kso2 = AddToKSO(kso, KEYID.DRANGLEIC);
                    return AddToKSO(kso2, KEYID.MEMORYJEIGH); // has king's ring


                // NPC shorthands:
                case KEYID.HEADVENGARL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRANCH);
                    return kso;

                case KEYID.GAVLAN:
                    return kso;

                case KEYID.CREIGHTON:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ROTUNDA, KEYID.UNDEADLOCKAWAY);
                    return kso;

                case KEYID.MCDUFF:
                    foreach (var ks in kso)
                        ks.Add(KEYID.DULLEMBER);
                    return kso;

                case KEYID.ORNIFEX:
                    foreach (var ks in kso)
                        ks.Add(KEYID.FANG);
                    return AddToKSO(kso, KEYID.TSELDORA);

                case KEYID.TITCHY:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TOKENOFSPITE);
                    return AddToKSO(kso, KEYID.UNDEADPURGATORY);

                case KEYID.EXECUTIONERSET:
                    var kso3 = AddToKSO(kso, KEYID.TITCHY);
                    return AddToKSO(kso3, KEYID.SHRINEOFWINTER);

                case KEYID.LICIAINVASION:
                    foreach (var ks in kso)
                        ks.Add(KEYID.CRUSHEDEYEORB);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.NADALIAFULL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ALLWEDGES, KEYID.ALLNADSOULS);
                    var kso4 = AddToKSO(kso, KEYID.FUME);
                    return AddToKSO(kso4, KEYID.BLUESMELTER);

                case KEYID.PLACEUNBENKNOWNST:
                    foreach (var ks in kso)
                        ks.Add(KEYID.SOLDIER, KEYID.KINGSRING);
                    return kso;

                case KEYID.SINNERSCELLS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BASTILLEKEY);
                    return AddToKSO(kso, KEYID.SINNERSRISE);

                case KEYID.ROTUNDAPHARROS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ROTUNDA, KEYID.PHARROS);
                    return kso;

                case KEYID.DRAGONCOVENANT:
                    foreach (var ks in kso)
                        ks.Add(KEYID.PETRIFIEDEGG);
                    return AddToKSO(kso, KEYID.IRONKEEP);

                case KEYID.TARK:
                    foreach (var ks in kso)
                        ks.Add(KEYID.WHISPERS);
                    return AddToKSO(kso, KEYID.SHADEDWOODS);

                case KEYID.BRANCHMEMEPHARROS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRANCH, KEYID.BRANCHMEMEPHARROS);
                    return kso;

                case KEYID.TSELDORADEN:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TSELDORADENKEY);
                    return AddToKSO(kso, KEYID.TSELDORA);

                case KEYID.TSELDORAVAULT:
                    foreach (var ks in kso)
                        ks.Add(KEYID.BRIGHTSTONE);
                    return AddToKSO(kso, KEYID.TSELDORA);

                case KEYID.BENHARTFULLQUEST:
                    var kso5 = AddToKSO(kso, KEYID.MEMORYORRO);
                    return AddToKSO(kso5, KEYID.DRANGLEIC);

                case KEYID.ORROPHARROS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.MEMEPHARROS);
                    return AddToKSO(kso, KEYID.MEMORYORRO);

                case KEYID.FUMEIDOL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ALLWEDGES);
                    return AddToKSO(kso, KEYID.FUME);

                case KEYID.FUMETOWERIDOL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TOWER);
                    return AddToKSO(kso, KEYID.FUMEIDOL);

                case KEYID.SMELTERIDOL:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ALLWEDGES);
                    return AddToKSO(kso, KEYID.BLUESMELTER);

                case KEYID.DLC3CAVE:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TORCH);
                    return AddToKSO(kso, KEYID.DLC3);

                case KEYID.DLC3PHARROS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.MEMEPHARROS);
                    return AddToKSO(kso, KEYID.DLC3);

                case KEYID.DRAGONMEMORY:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ASHENMIST);
                    return AddToKSO(kso, KEYID.TSELDORA);

                case KEYID.WELLAGERQUEST:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ASHENMIST);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.BDSM:
                    foreach (var ks in kso)
                        ks.Add(KEYID.EMBEDDED);
                    return AddToKSO(kso, KEYID.DRANGLEIC);

                case KEYID.AMANAPHARROS:
                    foreach (var ks in kso)
                        ks.Add(KEYID.MEMEPHARROS);
                    return AddToKSO(kso, KEYID.AMANA);

                case KEYID.AMANABRANCH:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TENBRANCHLOCK);
                    return AddToKSO(kso, KEYID.AMANA);

                case KEYID.AGDAYNEGIFT:
                    foreach (var ks in kso)
                        ks.Add(KEYID.KINGSRING);
                    return AddToKSO(kso, KEYID.AGDAYNE);

                case KEYID.NAVLANQUEST:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ROTUNDA);
                    return AddToKSO(kso, KEYID.ALDIASKEEP);

                case KEYID.ALDIASLAB:
                    foreach (var ks in kso)
                        ks.Add(KEYID.ALDIASKEY);
                    return AddToKSO(kso, KEYID.ALDIASKEEP);

                case KEYID.FOURFORLORN:
                    foreach (var ks in kso)
                        ks.Add(KEYID.TORCH);
                    return AddToKSO(kso, KEYID.ALDIASKEEP);

                case KEYID.BELLKEEPERSCOV:
                    // Need to separate into options:
                    var newkso2 = new List<KeySet>();
                    foreach (var ks in kso)
                    {
                        var ks3 = KeySet.Clone(ks);
                        ks.Add(KEYID.BRANCH, KEYID.BIGPHARROS); // LUNA
                        ks3.Add(KEYID.ROTUNDA, KEYID.BIGPHARROS); // SOL

                        newkso2.Add(ks);
                        newkso2.Add(ks3);
                    }
                    return newkso2;

                default:
                    throw new Exception("Not handled");
            }
        }

        internal static List<KeySet> KeyLogic(KEYID keyid)
        {
            // Monster wrapper for defining key combinations as shorthand
            var kso = new List<KeySet>() { new KeySet() }; // initialise for filling
            return AddToKSO(kso, keyid);
        }
    }
    

    

    internal enum PICKUPTYPE : int
    {
        ENEMYDROP,
        ENEMYREGISTDROP,
        ENEMYREGISTNPC,
        ENEMYREGISTINVASION,
        ENEMYREGISTSUMMON,
        BADENEMYDROP,
        BADREGISTDROP,
        GUARANTEEDENEMYDROP,

        COVENANTEASY,
        COVENANTHARD,
        NPC,
        WOODCHEST,
        METALCHEST,
        VOLATILE, // misc volatile
        NONVOLATILE, // this is basically corpse pickups now
        BOSS,
        NGPLUS,
        JOURNEYPLUS, // memes
        EXOTIC,     // Legit in game things considered too hard to achieve in rando
        REMOVED,    // Lost content
        CRAMMED,    // Meme stuff regarding edge cases when you're crammed
        UNRESOLVED,
        SHOP,       //
        EVSHOP,       //
        CROWS,
    }

    internal class RandoInfo2
    {
        internal string AreaString;
        internal string EnemyName;
        internal int ID;
        internal bool IsDirect;

        internal RandoInfo2(string? areastring, string enemyname, int paramid, bool directlot)
        {
            AreaString = areastring ?? "cantfindmap";
            EnemyName = enemyname;
            ID = paramid;
            IsDirect = directlot;
        }
    }

    internal class RandoInfo
    {
        internal MapArea Area;
        internal string? Description;
        internal PICKUPTYPE[] PickupTypes;
        internal List<KeySet> KSO; // KeySet Options
        internal RDZ_TASKTYPE RandoHandleType { get; set; }
        internal int RefInfoID = 0;
        internal readonly NodeKey NodeKey;
        internal bool IsKeyless => KSO.Count == 0 
                        || (KSO.Count == 1 && KSO[0].HasKey(KEYID.NONE))
                        || KSO.Count == 1 && KSO[0].Keys.Count == 0;

        // Main class constructor
        internal RandoInfo()
        {
            // Default "Empty" RandoInfo
            Area = MapArea.Undefined;
            Description = "<EmptyRandoInfoDefaultString>";
            KSO = new List<KeySet>();
            PickupTypes = Array.Empty<PICKUPTYPE>();
            RandoHandleType = RDZ_TASKTYPE.UNDEFINED;
            NodeKey = new NodeKey(Area, KSO);
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, List<KeySet> kso)
        {
            Area = area;
            Description = desc;
            PickupTypes = new PICKUPTYPE[] { type };
            KSO = kso;
            RandoHandleType = RDZ_TASKTYPE.STANDARD;
            NodeKey = new NodeKey(Area, KSO);
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE[] types, List<KeySet> kso)
        {
            Area = area;
            Description = desc;
            PickupTypes = types;
            KSO = kso;
            RandoHandleType = RDZ_TASKTYPE.STANDARD;
            NodeKey = new NodeKey(Area, KSO);
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, RDZ_TASKTYPE handletype, List<KeySet> kso)
        {
            Area = area;
            Description = desc;
            PickupTypes = new PICKUPTYPE[] { type };
            KSO = kso;
            RandoHandleType = handletype;
            NodeKey = new NodeKey(Area, KSO);
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, RDZ_TASKTYPE handletype, int refID, List<KeySet> kso)
        {
            Area = area;
            Description = desc;
            PickupTypes = new PICKUPTYPE[] { type };
            KSO = kso;
            RandoHandleType = handletype;
            RefInfoID = refID;
            NodeKey = new NodeKey(Area, KSO);
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE[] types, RDZ_TASKTYPE handletype, List<KeySet> kso)
        {
            Area = area;
            Description = desc;
            PickupTypes = types;
            KSO = kso;
            RandoHandleType = handletype;
            NodeKey = new NodeKey(Area, KSO);
        }


        internal bool HasType(IEnumerable<PICKUPTYPE> checklist)
        {
            return PickupTypes.Any(checklist.Contains);
        }
        internal bool AvoidsType(PICKUPTYPE bantype)
        {
            return !HasType(bantype);
        }
        internal bool AvoidsTypes(List<PICKUPTYPE> bannedtypes)
        {
            return !HasType(bannedtypes);
        }
        internal bool HasType(PICKUPTYPE checktype)
        {
            return PickupTypes.Any(pt => pt == checktype);
        }
        internal bool ContainsOnlyTypes(List<PICKUPTYPE> onlytypes)
        {
            return PickupTypes.All(onlytypes.Contains);
        }
    };

    internal struct KeySet
    {
        internal List<KEYID> Keys = new();

        public KeySet()
        {
        }
        public KeySet(KEYID key)
        {
            Keys.Add(key);
        }

        internal static KeySet Clone(KeySet ks)
        {
            return new KeySet
            {
                Keys = new List<KEYID>(ks.Keys)
            };
        }
        internal bool HasKey(KEYID keyid)
        {
            return HasKey((int)keyid);
        }
        internal bool HasKey(int itemid)
        {
            foreach (var key in Keys)
            {
                if ((int)key == itemid) return true;
            }
            return false;
        }
        internal void Add(params KEYID[] newkeys)
        {
            foreach (var key in newkeys)
            {
                // only add unique new ones
                if (!Keys.Contains(key))
                    Keys.Add(key);

            }
        }

        public static bool operator ==(KeySet lhs, KeySet rhs) { return lhs.Keys.SequenceEqual(rhs.Keys); }
        public static bool operator !=(KeySet lhs, KeySet rhs) { return !(lhs == rhs); }


        public bool Equals(KeySet other)
        {
            return Keys.SequenceEqual(other.Keys);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            return Equals((KeySet)obj);
        }
        public override int GetHashCode()
        {
            return Keys.GetHashCode();
        }
    }
}
