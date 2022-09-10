using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal abstract class ItemSetBase
    {
        // Other Logic related things:
        List<PICKUPTYPE> BanKeyTypes = new List<PICKUPTYPE>()
        {
            PICKUPTYPE.NPC,
            PICKUPTYPE.VOLATILE,
            PICKUPTYPE.EXOTIC,
            PICKUPTYPE.COVENANTEASY,
            PICKUPTYPE.COVENANTHARD,
            PICKUPTYPE.UNRESOLVED,
            PICKUPTYPE.REMOVED,
            PICKUPTYPE.NGPLUS,
            PICKUPTYPE.CRAMMED,
            PICKUPTYPE.WOODCHEST,
            PICKUPTYPE.SHOP, // For now
        };
        List<PICKUPTYPE> BanGeneralTypes = new List<PICKUPTYPE>()
        {
            PICKUPTYPE.EXOTIC,
            PICKUPTYPE.COVENANTHARD, // To split into cheap/annoying
            PICKUPTYPE.UNRESOLVED,
            PICKUPTYPE.REMOVED,
            PICKUPTYPE.NGPLUS,
            PICKUPTYPE.CRAMMED,
        };

        internal List<PICKUPTYPE> BanFromLoot = new List<PICKUPTYPE>()
        {
            // List of places where loot cannot come from:
            PICKUPTYPE.CRAMMED,
            PICKUPTYPE.UNRESOLVED,
            PICKUPTYPE.REMOVED,
        };

        // Overloads for quick construction, single or no key requirements:
        internal RandoInfo NpcInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.NPC, new KeySet(reqkey));
        }
        internal RandoInfo NpcSafeInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.NPC, PICKUPTYPE.NONVOLATILE), new KeySet(reqkey));
        }
        internal RandoInfo CovInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.COVENANTHARD, new KeySet(reqkey));
        }
        internal RandoInfo CovFineInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.COVENANTEASY, new KeySet(reqkey));
        }
        internal RandoInfo WChestInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.WOODCHEST, new KeySet(reqkey));
        }
        internal RandoInfo MChestInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.METALCHEST, new KeySet(reqkey));
        }
        internal RandoInfo NGPlusInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.NGPLUS, new KeySet(reqkey));
        }
        internal RandoInfo WChestNGPlusInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.WOODCHEST, PICKUPTYPE.NGPLUS), new KeySet(reqkey));
        }
        internal RandoInfo MChestNGPlusInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.METALCHEST, PICKUPTYPE.NGPLUS), new KeySet(reqkey));
        }
        internal RandoInfo SafeInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.NONVOLATILE, new KeySet(reqkey));
        }
        internal RandoInfo VolInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.VOLATILE, new KeySet(reqkey));
        }
        internal RandoInfo UnresolvedInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.UNRESOLVED, new KeySet(reqkey));
        }
        internal RandoInfo ExoticInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.EXOTIC, new KeySet(reqkey));
        }
        internal RandoInfo CrammedInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.CRAMMED, new KeySet(reqkey));
        }
        internal RandoInfo RemovedInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, PICKUPTYPE.REMOVED, new KeySet(reqkey));
        }
        internal RandoInfo BossInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(desc, PICKUPTYPE.BOSS, new KeySet(reqkey));
        }
        internal RandoInfo BossNGPlusInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.BOSS, PICKUPTYPE.NGPLUS), new KeySet(reqkey));
        }
        internal RandoInfo VolBossInfo(string desc, KEYID reqkey = KEYID.NONE)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.BOSS, PICKUPTYPE.VOLATILE), new KeySet(reqkey));
        }

        // Overloads for multiple key options:
        internal RandoInfo NpcInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.NPC, keysets);
        }
        internal RandoInfo NpcSafeInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.NPC, PICKUPTYPE.NONVOLATILE), keysets);
        }
        internal RandoInfo CovInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.COVENANTHARD, keysets);
        }
        internal RandoInfo CovFineInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.COVENANTEASY, keysets);
        }
        internal RandoInfo WChestInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.WOODCHEST, keysets);
        }
        internal RandoInfo MChestInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.METALCHEST, keysets);
        }
        internal RandoInfo NGPlusInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.NGPLUS, keysets);
        }
        internal RandoInfo WChestNGPlusInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.WOODCHEST, PICKUPTYPE.NGPLUS), keysets);
        }
        internal RandoInfo MChestNGPlusInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.METALCHEST, PICKUPTYPE.NGPLUS), keysets);
        }
        internal RandoInfo SafeInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.NONVOLATILE, keysets);
        }
        internal RandoInfo ExoticInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.EXOTIC, keysets);
        }
        internal RandoInfo CrammedInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.CRAMMED, keysets);
        }
        internal RandoInfo VolInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, PICKUPTYPE.VOLATILE, keysets);
        }
        internal RandoInfo BossInfo(string desc, params KeySet[] keysets)
        {
            // This is essentially a flag on top of safeinfo
            return new RandoInfo(desc, PICKUPTYPE.BOSS, keysets);
        }
        internal RandoInfo BossNGPlusInfo(string desc, params KeySet[] keysets)
        {
            return new RandoInfo(desc, TypeArray(PICKUPTYPE.BOSS, PICKUPTYPE.NGPLUS), keysets);
        }

        // Utility shorthand methods (for common purposes):
        internal PICKUPTYPE[] TypeArray(params PICKUPTYPE[] types)
        {
            return types;
        }
        internal KeySet KSO(params KEYID[] keys) // KeySetOption
        {
            return new KeySet(keys);
        }
        internal Dictionary<int, RandoInfo> RemoveBannedTypes(List<PICKUPTYPE> bantypes)
        {
            return D.Where(kvp => IsValidKeyPickup(kvp, bantypes))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        private bool IsValidKeyPickup(KeyValuePair<int, RandoInfo> kvp_pickup, List<PICKUPTYPE> bannedtypes)
        {
            PICKUPTYPE[] PTs = kvp_pickup.Value.Types;
            return !PTs.Any(bannedtypes.Contains);
        }
        internal void AppendKvp(KeyValuePair<int, RandoInfo> kvp) 
        {
            D.Add(kvp.Key, kvp.Value);
        }
        internal bool IsAllowedType(Randomization rdz, RandomizerManager.SetType parentset)
        {
            // Get Logic-Info for this attempted placement:
            var LI = D[rdz.ParamID];

            switch (parentset)
            {
                case RandomizerManager.SetType.Keys:
                    return LI.AvoidsTypes(BanKeyTypes);

                case RandomizerManager.SetType.Reqs:
                    return LI.AvoidsTypes(BanKeyTypes); // todo

                case RandomizerManager.SetType.Gens:
                    return LI.AvoidsTypes(BanGeneralTypes);

                default:
                    throw new Exception("Unexpected value in argument set type");
            }
        }
        internal bool AvoidsTypes(Randomization rdz, List<PICKUPTYPE> bantypes) 
        {
            var LI = D[rdz.ParamID];
            return LI.AvoidsTypes(bantypes);
        }
        internal bool IsBannedType(Randomization rdz, RandomizerManager.SetType parentset)
        {
            return !IsAllowedType(rdz, parentset);
        }
        internal bool IsSoftlockPlacement(Randomization rdz, List<int> placedSoFar)
        {
            // Get the KeySet logic for this attempted placement
            var LI = D[rdz.ParamID];
            
            // Try each different option for key requirements
            foreach (var keyset in LI.KeySet)
            {
                if (keyset.Keys.All(kid => IsPlaced(kid, placedSoFar)))
                    return false; // NOT SOFT LOCKED all required keys are placed for at least one Keyset
            }
            return true; // No keyset has all keys placed yet, so this is dangerous; try somewhere else
        }
        private bool IsPlaced(KEYID kid, List<int> placedSoFar)
        {
            // Function to handle different checks depending on KeyTypes I guess:
            switch (kid)
            {
                case KEYID.NONE:
                    // no condition required:
                    return true;

                case KEYID.BELFRYLUNA:
                    // Branch && Pharros Lockstone x2
                    return condLuna();

                case KEYID.SINNERSRISE:
                    // Branch || Antiquated
                    return condSinner();

                case KEYID.DRANGLEIC:
                    // Branch && Rotunda && Sinner's Rise
                    return condDrangleic();

                case KEYID.AMANA:
                    // Drangleic && King's passage
                    return condAmana();

                case KEYID.ALDIASKEEP:
                    // Branch && King's Ring
                    return condAldias();

                case KEYID.MEMORYJEIGH:
                    // King's Ring && Ashen Mist
                    return condJeigh();

                case KEYID.GANKSQUAD:
                    // DLC1 && Eternal Sanctum
                    return condGankSquad();

                case KEYID.PUZZLINGSWORD:
                    // DLC1 (TODO Bow/Arrow as keys)
                    return condDLC1();

                case KEYID.ELANA:
                    // DLC1 && Dragon Stone
                    return condElana();

                case KEYID.FUME:
                    // DLC2 && Scorching Sceptor
                    return condFume();

                case KEYID.BLUESMELTER:
                    // DLC2 && Tower Key
                    return condBlueSmelter();

                case KEYID.ALONNE:
                    // DLC2 && Tower Key && Scorching Scepter && Ashen Mist
                    return condAlonne();

                case KEYID.DLC3:
                    // DLC3key && Drangleic
                    return condDLC3();

                case KEYID.FRIGIDOUTSKIRTS:
                    // DLC3 && Garrison Ward Key
                    return condFrigid();

                case KEYID.CREDITS:
                    // Drangleic & Memory of Jeigh
                    return condCredits();

                case KEYID.VENDRICK:
                    // Amana + SoaG x3
                    return condVendrick();

                case KEYID.BRANCH:
                    // Three branches available
                    return condBranch();

                case KEYID.TENBRANCHLOCK:
                    // Ten branches available
                    return condTenBranch();

                case KEYID.NADALIA:
                    // DLC2 && Scepter && Tower Key && 12x Smelter Wedge
                    return condNadalia();

                case KEYID.PHARROS:
                    // Eight Pharros lockstones available
                    return condPharros();

                case KEYID.BELFRYSOL:
                    // Rotunda Lockstone && Pharros Lockstone x2
                    return condSol();

                case KEYID.DARKLURKER:
                    // Drangleic && Forgotten key && Torch && Butterfly x3
                    return condDarklurker();

                default:
                    return condKey(kid); // Simple Key check
            }

            // Conditions wrappers:
            int countBranches() => placedSoFar.Where(i => i == (int)KEYID.FRAGRANTBRANCH).Count();
            int countPharros() => placedSoFar.Where(i => i == (int)KEYID.PHARROSLOCKSTONE).Count();
            bool condKey(KEYID keyid) => placedSoFar.Contains((int)keyid);
            bool condBranch() => countBranches() >= 3;
            bool condTenBranch() => countBranches() >= 10;
            bool condRotunda() => condKey(KEYID.ROTUNDA);
            bool condAshen() => condKey(KEYID.ASHENMIST);
            bool condKingsRing() => condKey(KEYID.KINGSRING);
            bool condDLC1() => condKey(KEYID.DLC1);
            bool condDLC2() => condKey(KEYID.DLC2);
            bool condSinner() => condBranch() || condKey(KEYID.ANTIQUATED);
            bool condDrangleic() => condBranch() && condRotunda() && condSinner();
            bool condAmana() => condDrangleic() && condKey(KEYID.KINGSPASSAGE);
            bool condAldias() => condBranch() && condKingsRing();
            bool condJeigh() => condAshen() && condKingsRing();
            bool condGankSquad() => condDLC1() && condKey(KEYID.ETERNALSANCTUM);
            bool condElana() => condDLC1() && condKey(KEYID.DRAGONSTONE);
            bool condFume() => condDLC2() && condKey(KEYID.SCEPTER);
            bool condBlueSmelter() => condDLC2() && condKey(KEYID.TOWER);
            bool condAlonne() => condDLC2() && condKey(KEYID.TOWER) && condKey(KEYID.SCEPTER) && condAshen();
            bool condDLC3() => condKey(KEYID.DLC3KEY) && condDrangleic();
            bool condFrigid() => condDLC3() && condKey(KEYID.GARRISONWARD);
            bool condCredits() => condDrangleic() && condJeigh();
            bool condWedges() => placedSoFar.Where(i => i == (int)KEYID.SMELTERWEDGE).Count() == 12;
            bool condNadalia() => condFume() && condBlueSmelter() && condWedges();
            bool condVendrick() => condAmana() && (placedSoFar.Where(i => i == (int)KEYID.SOULOFAGIANT).Count() >= 3);
            bool condBigPharros() => countPharros() >= 2;
            bool condPharros() => countPharros() >= 8; // surely enough
            bool condLuna() => condBranch() && condBigPharros();
            bool condSol() => condRotunda() && condBigPharros();
            bool condButterflies() => placedSoFar.Where(i => i == (int)KEYID.FLAMEBUTTERFLY).Count() >= 3;
            bool condDarklurker() => condDrangleic() && condKey(KEYID.FORGOTTEN) && condButterflies() && condKey(KEYID.TORCH);
        }

        // To implement:
        internal Dictionary<int, RandoInfo> D = new Dictionary<int, RandoInfo>();
        internal abstract void SetupItemSet();


    }
}
