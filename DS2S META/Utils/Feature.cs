using mrousavy;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    public enum METAFEATURE
    {
        MADWARRIOR,
        OHKO_FIST,
        OHKO_RAPIER,
        NODEATH,
        DISABLEAI,
        GIVE17KREWARD,
        GIVE3CHUNK1SLAB,
        RUBBISHCHALLENGE,
        BIKP1SKIP,
        NOGRAVITY,
        NOCOLLISION,
        SPEEDHACK,
        STOREPOSITION,
        RESTOREPOSITION,
        WARP,
        DMGMOD,
    }
    public static class MetaFeature
    {
        private static DS2SHook? Hook;
        public static void Initialize(DS2SHook hook)
        {
            Hook = hook;
        }

        // Helpful query:
        public static bool IsActive(METAFEATURE feat)
        {
            if (Hook?.Hooked != true) return false; // no way of figuring out versions

            // can add extra specific switches on Versions too soon...
            // aka if the Offsets are defined but it's not working
            // in certain versions, can veto here.
            return feat switch
            {
                METAFEATURE.OHKO_FIST => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.OHKO_RAPIER => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.NOGRAVITY => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.NOCOLLISION => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.NODEATH => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.DISABLEAI => Hook.IsSOTFS_CP && Hook.InGame,
                METAFEATURE.GIVE17KREWARD => Hook.IsValidVer && Hook.InGame, // should be fine for all versions
                METAFEATURE.GIVE3CHUNK1SLAB => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.MADWARRIOR => Hook.IsSOTFS_CP, // sotfs 1.03 only
                METAFEATURE.RUBBISHCHALLENGE => false, // not working in any versions atm
                METAFEATURE.BIKP1SKIP => Hook.IsSOTFS_CP,
                METAFEATURE.SPEEDHACK => Hook.IsValidVer,
                METAFEATURE.STOREPOSITION => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.RESTOREPOSITION => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.WARP => Hook.IsValidVer && Hook.InGame,
                METAFEATURE.DMGMOD => Hook.IsSOTFS_CP,
                _ => throw new NotImplementedException("Add many more here!")
            };
        }
        public static bool IsInactive(METAFEATURE feat) => !IsActive(feat);
    }
}
