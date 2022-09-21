using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal static class ShopRules
    {
        internal static List<LinkedShopEvent> GetLinkedEvents() 
        {
            List<LinkedShopEvent> LinkedEvents = new List<LinkedShopEvent>()
            {
                new LinkedShopEvent(70400000, 70400004), // Gilligan club
                new LinkedShopEvent(70400001, 70400005), // Gilligan claws
                new LinkedShopEvent(70400002, 70400006), // Gilligan whip
                new LinkedShopEvent(70400003, 70400007), // Gilligan wooden shield
                new LinkedShopEvent(70400200, 70400204), // Gilligan thief mask
                new LinkedShopEvent(70400201, 70400205), // Gilligan black leather armour
                new LinkedShopEvent(70400202, 70400206), // Gilligan black leather gloves
                new LinkedShopEvent(70400203, 70400207), // Gilligan black leather boots
                new LinkedShopEvent(70400500, 70400501), // Gilligan ladder miniature
                new LinkedShopEvent(70400600, 70400603), // Gilligan ooze
                new LinkedShopEvent(70400601, 70400604), // Gilligan lacerating knife
                new LinkedShopEvent(70400602, 70400605), // Gilligan bleeding serum
                //
                new LinkedShopEvent(72600400, 72600401, 72600402), // Gavlan Ring of Giants
                new LinkedShopEvent(72600600, 72600604), // Gavlan poison moss
                new LinkedShopEvent(72600601, 72600605), // Gavlan rotten pine resin
                new LinkedShopEvent(72600602, 72600606), // Gavlan poison throwing knife
                new LinkedShopEvent(72600603, 72600607), // Gavlan poison arrow
                //
                new LinkedShopEvent(75400600, 75400601), // Melentia lifegems
                new LinkedShopEvent(75400609, 75400610, 75400611, 75400612, 75400613, 75400614), // Melentia brightbugs
                //
                new LinkedShopEvent(76100211, 76100219), // Maughlin royal sodlier helm
                new LinkedShopEvent(76100212, 76100220), // Maughlin royal sodlier armour
                new LinkedShopEvent(76100213, 76100221), // Maughlin royal sodlier gauntlets
                new LinkedShopEvent(76100214, 76100222), // Maughlin royal sodlier leggings
                new LinkedShopEvent(76100215, 76100223), // Maughlin elite knight helm
                new LinkedShopEvent(76100216, 76100224), // Maughlin elite knight armour
                new LinkedShopEvent(76100217, 76100225), // Maughlin elite knight gauntlets
                new LinkedShopEvent(76100218, 76100226), // Maughlin elite knight leggings
            
                // Note: Chloanne's descriptions are messed up:
                new LinkedShopEvent(76200600, 76200601), // Chloanne bonfire ascetic
                new LinkedShopEvent(76200602, 76200603), // Chloanne large titanite shard
                new LinkedShopEvent(76200604, 76200605), // Chloanne titanite chunk

                // Note: McDuff's descriptions are messed up:
                new LinkedShopEvent(76430000, 76430007), // McDuff Uchi
                new LinkedShopEvent(76430001, 76430008), // McDuff Bastard Sword
                new LinkedShopEvent(76430002, 76430009), // McDuff Greataxe
                new LinkedShopEvent(76430003, 76430010), // Mcduff Winged Spear
                new LinkedShopEvent(76430004, 76430011), // McDuff Scythe
                new LinkedShopEvent(76430005, 76430012), // McDuff Longbow
                new LinkedShopEvent(76430006, 76430013), // McDuff Light Crossbow
                new LinkedShopEvent(76430100, 76430102), // McDuff Royal Kite Shield
                //
                new LinkedShopEvent(76600300, 76600308), // Carhillion Soul Arrow
                new LinkedShopEvent(76600301, 76600309), // Carhillion Great Soul Arrow
                new LinkedShopEvent(76600302, 76600310), // Carhillion Heavy Soul Arrow
                new LinkedShopEvent(76600303, 76600311), // Carhillion Great Heavy Soul Arrow
                new LinkedShopEvent(76600304, 76600312), // Carhillion Shockwave
                new LinkedShopEvent(76600305, 76600313), // Carhillion Soul Spear Barrage
                new LinkedShopEvent(76600306, 76600314), // Carhillion Magic Weapon
                new LinkedShopEvent(76600307, 76600315), // Carhillion Yearn
                //
                new LinkedShopEvent(76801000, 76801200), // Straid Pursuer UGS
                new LinkedShopEvent(76801001, 76801201), // Straid Pursuer Greatshield
                new LinkedShopEvent(76801002, 76801202), // Straid Giant Stone Axe
                new LinkedShopEvent(76801003, 76801203), // Straid Dragonrider Halberd
                new LinkedShopEvent(76801004, 76801204), // Straid Dragonrider Bow
                new LinkedShopEvent(76801005, 76801205), // Straid Dragonrider Twinblade
                new LinkedShopEvent(76801006, 76801206), // Straid Dragonrider Greatshield
                new LinkedShopEvent(76801007, 76801207), // Straid Warped Sword
                new LinkedShopEvent(76801008, 76801208), // Straid Barbed Club
                new LinkedShopEvent(76801009, 76801209), // Straid Arced Sword
                new LinkedShopEvent(76801010, 76801210), // Straid Chariot Lance
                new LinkedShopEvent(76801011, 76801211), // Straid Shield Crossbow
                new LinkedShopEvent(76801012, 76801212), // Straid Roaring Halberd
                new LinkedShopEvent(76801013, 76801213), // Straid Bone Scythe
                new LinkedShopEvent(76801014, 76801214), // Straid Mytha's Bent Blade
                new LinkedShopEvent(76801015, 76801215), // Straid Smelter Sword
                new LinkedShopEvent(76801016, 76801216), // Straid Spotted Whip
                new LinkedShopEvent(76801017, 76801217), // Straid Gargoyles Bident
            
                new LinkedShopEvent(76801100, 76801300), // Straid Heavy Homing Soul Arrow
                new LinkedShopEvent(76801101, 76801301), // Straid Toxic Mist
                new LinkedShopEvent(76801102, 76801302), // Straid Soul Shower
                new LinkedShopEvent(76801103, 76801303), // Straid Acid Surge
                new LinkedShopEvent(76801104, 76801304), // Straid Sacred Oath
                new LinkedShopEvent(76801105, 76801305), // Straid Repel
                new LinkedShopEvent(76801106, 76801306), // Straid Lifedrain Patch
                //
                new LinkedShopEvent(76900300, 76900310), // Licia Heal
                new LinkedShopEvent(76900301, 76900311), // Licia Med Heal
                new LinkedShopEvent(76900302, 76900312), // Licia Great Heal Excerpt
                new LinkedShopEvent(76900303, 76900313), // Licia Replenishment
                new LinkedShopEvent(76900304, 76900314), // Licia Respelendent Life
                new LinkedShopEvent(76900305, 76900315), // Licia Caressing Prayer
                new LinkedShopEvent(76900306, 76900316), // Licia Force
                new LinkedShopEvent(76900307, 76900317), // Licia Lightning Spear
                new LinkedShopEvent(76900308, 76900318), // Licia Homeward
                new LinkedShopEvent(76900309, 76900319), // Licia Guidance
            
                // 1000s = Normal 1100 = ??, 2000 = free, 2100 = ??
                new LinkedShopEvent(77601000, 77601100, 77602000, 77602100), // Ornifex Dragonslayer Spear
                new LinkedShopEvent(77601001, 77601101, 77602001, 77602101), // Ornifex Lost Sinner Sword
                new LinkedShopEvent(77601002, 77601102, 77602002, 77602102), // Ornifex Iron King Hammer
                new LinkedShopEvent(77601003, 77601103, 77602003, 77602103), // Ornifex Butcher's Knife
                new LinkedShopEvent(77601004, 77601104, 77602004, 77602104), // Ornifex Spider's Silk
                new LinkedShopEvent(77601005, 77601105, 77602005, 77602105), // Ornifex Spider's Fang
                new LinkedShopEvent(77601006, 77601106, 77602006, 77602106), // Ornifex Thorned Greatsword
                new LinkedShopEvent(77601007, 77601107, 77602007, 77602107), // Ornifex King's Mirror
                new LinkedShopEvent(77601008, 77601108, 77602008, 77602108), // Ornifex Sacred Chime Hammer
                new LinkedShopEvent(77601009, 77601119, 77602009, 77602109), // Ornifex Ruler's Sword
                new LinkedShopEvent(77601010, 77601110, 77602010, 77602110), // Ornifex King's UGS
                new LinkedShopEvent(77601011, 77601111, 77602011, 77602111), // Ornifex King's Shield
                new LinkedShopEvent(77601012, 77601112, 77602012, 77602112), // Ornifex Spitfire Spear
                new LinkedShopEvent(77601013, 77601113, 77602013, 77602113), // Ornifex Drakewing UGS
                new LinkedShopEvent(77601014, 77601114, 77602014, 77602114), // Ornifex Curved Dragon Greatsword
                new LinkedShopEvent(77601015, 77601115, 77602015, 77602115), // Ornifex Scythe of Want
                new LinkedShopEvent(77601016, 77601116, 77602016, 77602116), // Ornifex Chimne of Want
                new LinkedShopEvent(77601017, 77601117, 77602017, 77602117), // Ornifex Bow of Want
                new LinkedShopEvent(77601018, 77601118, 77602018, 77602118), // Ornifex Defender's Greatsword
                new LinkedShopEvent(77601019, 77601119, 77602019, 77602119), // Ornifex Defender's Shield
                new LinkedShopEvent(77601020, 77601120, 77602020, 77602120), // Ornifex Watcher's Greatsword
                new LinkedShopEvent(77601021, 77601121, 77602021, 77602121), // Ornifex Watcher's Shield

                new LinkedShopEvent(77601022, 77602022), // Ornifex Moonlight Greatsword
                new LinkedShopEvent(77601023, 77602023), // Ornifex Crypt Blacksword
                new LinkedShopEvent(77601024, 77602024), // Ornifex Chaos Blade
                new LinkedShopEvent(77601025, 77602025), // Ornifex Dragonslayer Greatbow
                new LinkedShopEvent(77601026, 77602026), // Ornifex Yorgh's Spear
                new LinkedShopEvent(77601027, 77602027), // Ornifex Wrathful Axe
                new LinkedShopEvent(77601028, 77602028), // Ornifex Chime Of Screams
                new LinkedShopEvent(77601029, 77602029), // Ornifex Fume Sword
                new LinkedShopEvent(77601030, 77602030), // Ornifex Fume UGS
                new LinkedShopEvent(77601031, 77602031), // Ornifex Bewitched Alonne Sword
                new LinkedShopEvent(77601032, 77602032), // Ornifex Aged Smelter Sword
                new LinkedShopEvent(77601033, 77602033), // Ornifex Ivory Straight Sword
                new LinkedShopEvent(77601034, 77602034), // Ornifex Loyce Shield
                new LinkedShopEvent(77601035, 77602035), // Ornifex Loyce Greatsword
                new LinkedShopEvent(77601036, 77602036), // Ornifex Ivory King UGS
                new LinkedShopEvent(77601037, 77602037), // Ornifex Eleum Loyce
                //
                new LinkedShopEvent(77700600, 77700604), // Shalquoir Prism Stones
                new LinkedShopEvent(77700601, 77700605), // Shalquoir Alluring Skulls
                new LinkedShopEvent(77700602, 77700606), // Shalquoir Lloyd's Amulet
                new LinkedShopEvent(77700603, 77700607), // Shalquoir Homeward Bone
                //
                new LinkedShopEvent(78400300, 78400308), // Cromwell Great Heal
                new LinkedShopEvent(78400301, 78400309), // Cromwell Replenishment
                new LinkedShopEvent(78400302, 78400310), // Cromwell Caressing Prayer
                new LinkedShopEvent(78400303, 78400311), // Cromwell Force
                new LinkedShopEvent(78400304, 78400312), // Cromwell Emit Force
                new LinkedShopEvent(78400305, 78400313), // Cromwell Heavenly Thunder
                new LinkedShopEvent(78400306, 78400314), // Cromwell Perserverance
                new LinkedShopEvent(78400307, 78400315), // Cromwell Scraps of Life
            };
            return LinkedEvents;
        }

        internal static List<int> Exclusions = new List<int>()
        {
            // List of things to exclude because duplicates:
            1,              // Blank
            75400601,       // Missing content
        };
    }
    internal class LinkedShopEvent
    {
        // Setup to handle enabling/disabling of shop logic a bit more cleanly
        // Cannot do it programatically very easily because the disable events
        // can spread across all sorts of scenarios
        internal int KeepID;
        internal List<int> RemoveIDs;

        internal LinkedShopEvent(int keepID, params int[] removeIDs)
        {
            KeepID = keepID;
            RemoveIDs = removeIDs.ToList();
        }
    }
}
