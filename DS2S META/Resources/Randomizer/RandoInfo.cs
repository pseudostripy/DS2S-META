using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Resources.Randomizer
{
    internal enum KEYID : int
    {
        NONE = 0x0,                 // default
        BELFRYLUNA      = 0xBB0,    // Shorthand: Branch && Pharros
        SINNERSRISE     = 0xBB1,    // Shorthand: Branch || Antiquated 
        DRANGLEIC       = 0xBB2,    // Shorthand: All conditions for drangleic
        AMANA           = 0xBB3,    // Shorthand: Drangleic + King's passage
        ALDIASKEEP      = 0xBB4,    // Shorthand: Branch + King's Ring
        MEMORYJEIGH     = 0xBB5,    // Shorthand: King's Ring + Ashen Mist
        GANKSQUAD       = 0xBB6,    // Shorthand: DLC1 + Eternal Sanctum
        PUZZLINGSWORD   = 0xBB7,    // Shorthand TO CONSIDER: Might need range
        ELANA           = 0xBB8,    // Shorthand: DLC1 + Dragon Stone
        FUME            = 0xBB9,    // Shorthand: DLC2 + Scorching Sceptor
        BLUESMELTER     = 0xBBA,    // Shorthand: DLC2 + Tower Key
        ALONNE          = 0xBBB,    // Shorthand: DLC2 + Tower Key + Scorching Scepter + Ashen Mist
        DLC3            = 0xBBC,    // Shorthand: DLC3 + Drangleic
        FRIGIDOUTSKIRTS = 0xBBD,    // Shorthand: DLC3 + Garrison Ward Key
        CREDITS         = 0xBBE,    // Shorthand: Everything required to beat Nash

        BRANCH          = 0xAA0,    // Shorthand: Enough Branches available
        PHARROS         = 0xAA1,    // Shorthand: Enough Pharros Lockstones available
        NADALIA         = 0xAA2,    // Shorthand: DLC2 + enough Smelter Wedges 
        

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
        DLC2            = 52100000,
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

    }

    internal enum PICKUPTYPE : int
    {
        COVENANT,
        NPC,
        WOODCHEST,
        METALCHEST,
        VOLATILE, // misc volatile
        NONVOLATILE, // this is basically corpse pickups now
        BOSS,
        NGPLUS,
        EXOTIC,     // Cannot possibly expect a casual to figure these out
        REMOVED,    // Lost content
        UNRESOLVED,
    }

    internal class RandoInfo
    {
        // Main class constructor
        internal RandoInfo(string desc, PICKUPTYPE type, params KeySet[] reqkeys)
        {
        }
        internal RandoInfo(string desc, PICKUPTYPE[] flags, params KeySet[] reqkeys)
        {
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
