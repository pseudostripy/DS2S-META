﻿using DS2S_META.Utils;
using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    public class LinkNGDrop
    {
        public int origID;
        public int ngplusID;

        public LinkNGDrop(int origID, int ngplusID)
        {
            this.origID = origID;
            this.ngplusID = ngplusID;
        }
    }


    internal abstract class ItemSetBase
    {
        // Other Logic related things:
        internal static List<DropInfo> FillerItems = new()
        {
            new DropInfo(0x0393AE10, 3), // Lifegem
            new DropInfo(0x0393D520), // Radiant Lifegem
            new DropInfo(0x0393FC30), // Old Radiant Lifegem
            new DropInfo(0x039413A0), // Dried Root
            new DropInfo(0x03942340), // Amber Herb
            new DropInfo(0x03944A50), // Twilight Herb
            new DropInfo(0x03947160), // Wilted Dusk Herb
            new DropInfo(0x03949870), // Poison Moss
            new DropInfo(0x0394E690), // Monastery Charm
            new DropInfo(0x03950DA0), // Dragon Charm
            new DropInfo(0x03952128), // Divine Blessing
            new DropInfo(0x039534B0), // Rouge Water
            new DropInfo(0x03955BC0), // Crimson Water
            new DropInfo(0x0395D4D8), // Human Effigy
            new DropInfo(0x0395F800), // Small Blue Burr
            new DropInfo(0x03961F10), // Small Yellow Burr
            new DropInfo(0x03964620), // Small Orange Burr
            new DropInfo(0x03966D30), // Dark Troches
            new DropInfo(0x03969440), // Common Fruit
            new DropInfo(0x0396BB50), // Red Leech Troches
            new DropInfo(0x03970970), // Triclops Snake Troches
            new DropInfo(0x03971CF8), // Old Growth Balm
            new DropInfo(0x039720E0), // Vine Balm
            new DropInfo(0x039724C8), // Blackweed Balm
            new DropInfo(0x039728B0), // Goldenfruit Balm
            new DropInfo(0x03973080), // Aromatic Ooze
            new DropInfo(0x03975790), // Gold Pine Resin
            new DropInfo(0x03977EA0), // Charcoal Pine Resin
            new DropInfo(0x0397A5B0), // Dark Pine Resin
            new DropInfo(0x0397CCC0), // Rotten Pine Resin
            new DropInfo(0x0397F3D0), // Bleeding Serum
            new DropInfo(0x039841F0), // Green Blossom
            new DropInfo(0x03986900), // Rusted Coin
            new DropInfo(0x0398DE30), // Homeward Bone
            new DropInfo(0x03992C50), // Silver Talisman
            new DropInfo(0x0399C890), // Repair Powder
            new DropInfo(0x039A16B0), // Flame Butterfly
            new DropInfo(0x039A64D0), // Prism Stone
            new DropInfo(0x039B4F30), // Rubbish
            new DropInfo(0x039B5318), // Petrified Something
            new DropInfo(0x039B9198), // Bonfire Ascetic
            new DropInfo(0x039B9D50), // Alluring Skull
            new DropInfo(0x039BA138), // Lloyd's Talisman
            new DropInfo(0x039BC460, 5), // Throwing Knife
            new DropInfo(0x039BEB70, 3), // Witching Urn
            new DropInfo(0x039C1280, 3), // Lightning Urn
            new DropInfo(0x039C3990, 3), // Firebomb
            new DropInfo(0x039C4D18, 3), // Black Firebomb
            new DropInfo(0x039C60A0, 3), // Hexing Urn
            new DropInfo(0x039C87B0, 5), // Poison Throwing Knife
            new DropInfo(0x039C9B38, 3), // Dung Pie
            new DropInfo(0x039CAEC0, 5), // Lacerating Knife
            new DropInfo(0x039CD5D0, 3), // Corrosive Urn
            new DropInfo(0x039CFCE0, 3), // Holy Water Urn
            new DropInfo(0x039D1068), // Fading Soul
            new DropInfo(0x039D23F0), // Soul of a Lost Undead
            new DropInfo(0x039D4B00), // Large Soul of a Lost Undead
            new DropInfo(0x039D7210), // Soul of a Nameless Soldier
            new DropInfo(0x039D9920), // Large Soul of a Nameless Soldier
            new DropInfo(0x039DC030), // Soul of a Proud Knight
            new DropInfo(0x039DE740), // Large Soul of a Proud Knight
            new DropInfo(0x039E0E50), // Soul of a Brave Warrior
            new DropInfo(0x039E3560), // Large Soul of a Brave Warrior
            new DropInfo(0x039E5C70), // Soul of a Hero
            
            new DropInfo(0x039F1FC0, 5), // Wood Arrow
            new DropInfo(0x039F46D0, 5), // Iron Arrow
            new DropInfo(0x039F6DE0, 5), // Magic Arrow
            new DropInfo(0x039F94F0, 5), // Lightning Arrow
            new DropInfo(0x039FBC00, 5), // Fire Arrow
            new DropInfo(0x039FE310, 5), // Dark Arrow
            new DropInfo(0x03A00A20, 5), // Poison Arrow
            new DropInfo(0x03A03130, 5), // Lacerating Arrow
            new DropInfo(0x03A07F50, 5), // Iron Greatarrow
            new DropInfo(0x03A0CD70, 5), // Lightning Greatarrow
            new DropInfo(0x03A0F480, 5), // Fire Greatarrow
            new DropInfo(0x03A142A0, 5), // Destructive Greatarrow
            new DropInfo(0x03A169B0, 5), // Wood Bolt
            new DropInfo(0x03A190C0, 5), // Heavy Bolt
            new DropInfo(0x03A1B7D0, 5), // Magic Bolt
            new DropInfo(0x03A1DEE0, 5), // Lightning Bolt
            new DropInfo(0x03A205F0, 5), // Fire Bolt
            new DropInfo(0x03A22D00, 5), // Dark Bolt
            
            new DropInfo(0x03A25410), // Titanite Shard
            new DropInfo(0x03A26798), // Large Titanite Shard
            new DropInfo(0x03A27B20), // Titanite Chunk
            new DropInfo(0x03A2C940), // Twinkling Titanite
            new DropInfo(0x03A33E70), // Petrified Dragon Bone
            new DropInfo(0x03A3B3A0), // Faintstone
            new DropInfo(0x03A3DAB0), // Boltstone
            new DropInfo(0x03A401C0), // Firedrake Stone
            new DropInfo(0x03A428D0), // Darknight Stone
            new DropInfo(0x03A44FE0), // Poison Stone
            new DropInfo(0x03A476F0), // Bleed Stone
            new DropInfo(0x03A4C510), // Raw Stone
            new DropInfo(0x03A4EC20), // Magic Stone
            new DropInfo(0x03A51330), // Old Mundane Stone
            new DropInfo(0x03A53A40), // Palestone
        };
        internal static Dictionary<int, int> SoulPriceList = new()
        {
            { 60625000, 50 },       // Fading soul
            { 60630000, 200 },      // Soul of a Lost Undead 
            { 60640000, 400 },      // Large Soul of a Lost Undead
            { 60650000, 800 },      // Soul of a Nameless Soldier
            { 60660000, 1000 },     // Large Soul of a Nameless Soldier
            { 60670000, 2000 },     // Soul of a Proud Knight
            { 60680000, 3000 },     // Large Soul of a Proud Knight
            { 60690000, 5000 },     // Soul of a Brave Warrior
            { 60700000, 8000 },     // Large Soul of a Brave Warrior
            { 60710000, 10000 },    // Soul of a Hero
            { 60720000, 20000 },    // Soul of a Great Hero
            //
            { 64010000, 6000 },     // Soul of the Last Giant
            { 64000000, 8000 },     // Soul of the Pursuer
            { 64020000, 6000 },     // Dragonrider Soul
            { 64030000, 10000 },    // Old Dragonslayer Soul
            { 64040000, 6000 },     // Flexile Sentry Soul
            { 64050000, 6000 },     // Ruin Sentinel Soul
            { 64060000, 25000 },    // Soul of the Lost Sinner
            { 64070000, 8000 },     // Executioner's Chariot Soul
            { 64080000, 6000 },     // Skeleton Lord's Soul
            { 64090000, 6000 },     // Covetous Demon Soul
            { 64100000, 10000 },    // Mytha's Soul
            { 64110000, 8000 },     // Smelter Demon Soul
            { 64120000, 25000 },    // Old Iron King Soul
            { 64130000, 6000 },     // Royal Rat Vanguard Soul
            { 64140000, 25000 },    // Soul of the Rotten
            { 64150000, 8000 },     // Najka Soul
            { 64160000, 6000 },     // Rat Authority Soul
            { 64170000, 25000 },    // Freja Soul
            { 64180000, 18000 },    // Mirror Knight Soul
            { 64190000, 12000 },    // Demon of Song Soul
            { 64200000, 15000 },    // Velstadt Soul
            { 64210000, 50000 },    // Soul of the King
            { 64220000, 14000 },    // Guardian Dragon Soul
            { 64230000, 75000 },    // Ancient Dragon Soul
            { 64240000, 25000 },    // Giant Lord Soul
            { 64250000, 30000 },    // Nashandra Soul
            { 64260000, 16000 },    // Throne Defender Soul
            { 64270000, 16000 },    // Throne Watcher Soul
            { 64280000, 22000 },    // Darklurker Soul
            { 64290000, 6000 },     // Gargoyles Soul
            { 64300000, 60000 },    // Old Witch Soul
            { 64310000, 60000 },    // Old King Soul
            { 64320000, 60000 },    // Old Dead One Soul
            { 64330000, 60000 },    // Old Paledrake Soul
            { 64500000, 16000 },    // Sinh Soul
            { 64510000, 16000 },    // Fume Knight Soul
            { 64520000, 16000 },    // Aava Soul
            { 64530000, 30000 },    // Elana Soul
            { 64540000, 30000 },    // Nadalia Soul
            { 64550000, 30000 },    // Alsanna Soul
            { 64560000, 16000 },    // Sir Alonne Soul
            { 64580000, 30000 },    // Ivory King Soul
            { 64590000, 16000 },    // Zallen Soul
            { 64610000, 16000 },    // Lud Soul
            // { 64600000, 16000 },    // Loyce Soul (nothing)
        };

        // Purely for printing:
        internal static List<KEYID> KeyOutputOrder = new()
        {
            // Actual Key Item IDs:
            KEYID.BLACKSMITH,
            KEYID.MANSION,
            KEYID.SOLDIER,
            KEYID.IRON,
            KEYID.DULLEMBER,
            KEYID.ANTIQUATED,
            KEYID.BASTILLEKEY,
            KEYID.ROTUNDA,
            KEYID.FORGOTTEN,
            KEYID.UNDEADLOCKAWAY,
            KEYID.FANG,
            KEYID.BRIGHTSTONE,
            KEYID.TSELDORADENKEY,
            KEYID.KINGSPASSAGE,
            KEYID.EMBEDDED,
            KEYID.KINGSRING,
            KEYID.ALDIASKEY,
            KEYID.ASHENMIST,
            KEYID.KINSHIP,
            //
            KEYID.PETRIFIEDEGG,
            KEYID.WHISPERS,
            KEYID.CRUSHEDEYEORB,
            KEYID.TOKENOFFIDELITY,
            KEYID.TOKENOFSPITE,
            //
            KEYID.DLC1KEY,
            KEYID.DRAGONSTONE,
            KEYID.ETERNALSANCTUM,
            KEYID.DLC2KEY,
            KEYID.SCEPTER,
            KEYID.TOWER,
            KEYID.DLC3KEY,
            KEYID.GARRISONWARD,
            //
            KEYID.FRAGRANTBRANCH,
            KEYID.SOULOFAGIANT,
            KEYID.SMELTERWEDGE,
            KEYID.NADALIAFRAGMENT,
            KEYID.PHARROSLOCKSTONE,
        };
        
        // Overloads for quick construction, single or no key requirements:
        internal static RandoInfo ShopInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.SHOP, kso);
        }
        internal static RandoInfo EvShopInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // things which require an event
            return new RandoInfo(area, desc, PICKUPTYPE.EVSHOP, kso);
        }
        internal static RandoInfo ShopSustain(MapArea area, string desc, List<KeySet> kso)
        {
            // turn off the disable event
            return new RandoInfo(area, desc, PICKUPTYPE.SHOP, RDZ_TASKTYPE.SHOPSUSTAIN, kso);
        }
        internal static RandoInfo EvShopSustain(MapArea area, string desc, List<KeySet> kso)
        {
            // turn off the disable event
            return new RandoInfo(area, desc, PICKUPTYPE.EVSHOP, RDZ_TASKTYPE.SHOPSUSTAIN, kso);
        }
        internal static RandoInfo TradeShopInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.EVSHOP, RDZ_TASKTYPE.UNLOCKTRADE, kso);
        }
        internal static RandoInfo FreeTradeShopInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.EVSHOP, RDZ_TASKTYPE.FREETRADE, kso);
        }
        internal static RandoInfo TradeShopCopy(MapArea area, string desc, int refid, List<KeySet> kso)
        {
            // awkward Ornifex things
            return new RandoInfo(area, desc, PICKUPTYPE.EVSHOP, RDZ_TASKTYPE.TRADE_SHOP_COPY, refid, kso);
        }
        internal static RandoInfo ShopRemoveInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.SHOP, RDZ_TASKTYPE.SHOPREMOVE, kso);
        }
        internal static RandoInfo ShopCopy(MapArea area, string desc, int refid, List<KeySet> kso)
        {
            // not randomized, just copied from copyfromID
            return new RandoInfo(area, desc, PICKUPTYPE.SHOP, RDZ_TASKTYPE.FILL_BY_COPY, refid, kso);
        }
        internal static RandoInfo NpcInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.NPC, kso);
        }
        internal static RandoInfo NpcSafeInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.NPC, PICKUPTYPE.NONVOLATILE), kso);
        }
        internal static RandoInfo CovInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.COVENANTHARD, kso);
        }
        internal static RandoInfo CovFineInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.COVENANTEASY, kso);
        }
        internal static RandoInfo WChestInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.WOODCHEST, kso);
        }
        internal static RandoInfo MChestInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.METALCHEST, kso);
        }
        internal static RandoInfo NGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.NGPLUS, kso);
        }
        internal static RandoInfo WChestNGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.WOODCHEST, PICKUPTYPE.NGPLUS), kso);
        }
        internal static RandoInfo MChestNGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.METALCHEST, PICKUPTYPE.NGPLUS), kso);
        }
        internal static RandoInfo SafeInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.NONVOLATILE, kso);
        }
        internal static RandoInfo VolInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.VOLATILE, kso);
        }
        internal static RandoInfo CrowsInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.CROWS, RDZ_TASKTYPE.CROWS, kso);
        }
        internal static RandoInfo UnresolvedInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.UNRESOLVED, kso);
        }
        internal static RandoInfo ExoticInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.EXOTIC, kso);
        }
        internal static RandoInfo CrammedInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.CRAMMED, kso);
        }
        internal static RandoInfo RemovedInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, PICKUPTYPE.REMOVED, kso);
        }
        internal static RandoInfo BossInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(area, desc, PICKUPTYPE.BOSS, kso);
        }
        internal static RandoInfo BossVolInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.BOSS, PICKUPTYPE.VOLATILE), kso);
        }
        internal static RandoInfo BossNGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.BOSS, PICKUPTYPE.NGPLUS), kso);
        }
        internal static RandoInfo LinkedSlave(MapArea area, string desc, int refid, PICKUPTYPE pickuptype, List<KeySet> kso)
        {
            // See ShopCopy (but this is for non-shops)
            return new RandoInfo(area, desc, pickuptype, RDZ_TASKTYPE.LINKEDSLAVE, refid, kso);
        }
        //
        internal static RandoInfo EnemyDropInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // these will be converted do GUARANTEEDENEMYDROP during Presanitization if required.
            return new RandoInfo(area, desc, PICKUPTYPE.ENEMYDROP, kso);
        }
        internal static RandoInfo EnemyNGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.WOODCHEST, PICKUPTYPE.NGPLUS), kso);
        }
        internal static RandoInfo EnemyRegistInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // lots of enemies can share these tables
            return new RandoInfo(area, desc, PICKUPTYPE.ENEMYREGISTDROP, kso);
        }
        internal static RandoInfo EnemyRegistNpc(MapArea area, string desc, List<KeySet> kso)
        {
            // lots of enemies can share these tables
            return new RandoInfo(area, desc, PICKUPTYPE.ENEMYREGISTNPC, kso);
        }
        internal static RandoInfo EnemyRegistInvasion(MapArea area, string desc, List<KeySet> kso)
        {
            // lots of enemies can share these tables
            return new RandoInfo(area, desc, PICKUPTYPE.ENEMYREGISTINVASION, kso);
        }
        internal static RandoInfo EnemyRegistSummon(MapArea area, string desc, List<KeySet> kso)
        {
            // lots of enemies can share these tables
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.ENEMYREGISTSUMMON, PICKUPTYPE.BADREGISTDROP), kso);
        }
        internal static RandoInfo EnemyRegistNGPlusInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // lots of enemies can share these tables
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.ENEMYREGISTDROP, PICKUPTYPE.NGPLUS), kso);
        }
        internal static RandoInfo EnemyRegistJourneyPlus(MapArea area, string desc, List<KeySet> kso)
        {
            // For enemies that *only* spawn on second journey, not via ascetic
            return new RandoInfo(area, desc, TypeArray(PICKUPTYPE.ENEMYREGISTDROP, PICKUPTYPE.JOURNEYPLUS), kso);
        }
        internal static RandoInfo UnreachableDropInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // for things the player isn't expected to be able to kill?
            return new RandoInfo(area, desc, PICKUPTYPE.BADENEMYDROP, kso);
        }
        internal static RandoInfo UnreachableRegistInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // for things the player isn't expected to be able to kill?
            return new RandoInfo(area, desc, PICKUPTYPE.BADREGISTDROP, kso);
        }
        internal static RandoInfo MemeCopyDropInfo(MapArea area, string desc, List<KeySet> kso)
        {
            // e.g things with identical enemies with identical drops, but in
            // different itemlot tables.
            return new RandoInfo(area, desc, PICKUPTYPE.BADENEMYDROP, kso);
        }


        // Utility shorthand methods (for common purposes):
        internal static PICKUPTYPE[] TypeArray(params PICKUPTYPE[] types)
        {
            return types;
        }
    }
}
