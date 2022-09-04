using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Resources.Randomizer
{
    internal abstract class ItemSetBase
    {
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

        // To implement:
        internal Dictionary<int, RandoInfo> D = new Dictionary<int, RandoInfo>();
        internal abstract void SetupItemSet();
    }
}
