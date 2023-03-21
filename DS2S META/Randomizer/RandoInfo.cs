using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        
        BRANCH          = 0xAA0,    // Require at least 3 branches
        PHARROS         = 0xAA1,    // Enough Pharros Lockstones available
        NADALIA         = 0xAA2,    // DLC2 + enough Smelter Wedges 
        TENBRANCHLOCK   = 0xAA4,    // Require at least 10 branches
        BIGPHARROS      = 0xAA5,    // Require at least two lockstones
        MIRRORKNIGHTEVENT = 0xAA6,  // Drangleic & King's passage [see also Amana]
        SHRINEOFWINTER  = 0xAA7,    // SinnersRise & IronKeep & Tseldora

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
        DLC1            = 52000000,
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
        TSELDORADEN     = 50930000,
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

    internal enum PICKUPTYPE : int
    {
        ENEMYDROP,
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
        EXOTIC,     // Legit in game things considered too hard to achieve in rando
        REMOVED,    // Lost content
        CRAMMED,    // Meme stuff regarding edge cases when you're crammed
        UNRESOLVED,
        SHOP,       //
        CROWS,
        LINKEDSLAVE, // Rules are defined by some other drop that was defined and linked
    }

    internal class RandoInfo
    {
        internal MapArea Area;
        internal string? Description;
        internal PICKUPTYPE[] Types;
        internal KeySet[] KeySet;
        internal RDZ_STATUS RandoHandleType;
        internal int RefInfoID = 0;
        
        // Main class constructor
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, params KeySet[] reqkeys)
        {
            Area = area;
            Description = desc;
            Types = new PICKUPTYPE[] { type };
            KeySet = reqkeys;
            RandoHandleType = RDZ_STATUS.STANDARD;
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE[] types, params KeySet[] reqkeys)
        {
            Area = area;
            Description = desc;
            Types = types;
            KeySet = reqkeys;
            RandoHandleType= RDZ_STATUS.STANDARD;
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, RDZ_STATUS handletype, params KeySet[] reqkeys)
        {
            Area = area;
            Description = desc;
            Types = new PICKUPTYPE[] { type };
            KeySet = reqkeys;
            RandoHandleType = handletype;
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE type, RDZ_STATUS handletype, int refID, params KeySet[] reqkeys)
        {
            Area = area;
            Description = desc;
            Types = new PICKUPTYPE[] { type };
            KeySet = reqkeys;
            RandoHandleType = handletype;
            RefInfoID = refID;
        }
        internal RandoInfo(MapArea area, string desc, PICKUPTYPE[] types, RDZ_STATUS handletype, params KeySet[] reqkeys)
        {
            Area = area;
            Description = desc;
            Types = types;
            KeySet = reqkeys;
            RandoHandleType = handletype;
        }


        internal bool HasType(List<PICKUPTYPE> checklist)
        {
            return Types.Any(checklist.Contains);
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
            return Types.Any(pt => pt == checktype);
        }
        internal bool ContainsOnlyTypes(List<PICKUPTYPE> onlytypes)
        {
            return Types.All(onlytypes.Contains);
        }

        internal bool IsSoftlockPlacement(List<int> placedSoFar)
        {
            // Try each different option for key requirements
            foreach (var keyset in KeySet)
            {
                if (keyset.Keys.All(kid => ItemSetBase.IsPlaced(kid, placedSoFar)))
                    return false; // NOT SOFT LOCKED all required keys are placed for at least one Keyset
            }
            return true; // No keyset has all keys placed yet, so this is dangerous; try somewhere else
        }
    };

    internal class KeySet
    {
        internal KEYID[] Keys;
        internal KeySet(params KEYID[] keys)
        {
            Keys = keys;
        }
    }
}
