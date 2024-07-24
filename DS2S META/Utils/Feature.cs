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
        MANAGEBFS,
        GIVESOULS,
        RESTOREHUMANITY,
        NEWTESTCHARACTER,
        INFINITESTAMINA,
        DISABLESKIRT
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
                METAFEATURE.INFINITESTAMINA => Hook.IsSOTFS_CP && Hook.InGame,
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
                METAFEATURE.MANAGEBFS => Hook.InGame,
                METAFEATURE.GIVESOULS => Hook.IsValidVer && Hook.InGame,        // should work on all supported versions
                METAFEATURE.RESTOREHUMANITY => Hook.IsValidVer && Hook.InGame,  // should work on all supported versions
                METAFEATURE.NEWTESTCHARACTER => Hook.IsValidVer && Hook.InGame, // should work on all supported versions
                METAFEATURE.DISABLESKIRT => Hook.IsValidVer && Hook.InGame,
                _ => throw new NotImplementedException("Add many more here!")
            };
        }
        public static bool IsInactive(METAFEATURE feat) => !IsActive(feat);

        // Specific feature shorthands
        public static bool FtGive17kReward => IsActive(METAFEATURE.GIVE17KREWARD);
        public static bool FtGive3Chunk1Slab => IsActive(METAFEATURE.GIVE3CHUNK1SLAB);
        public static bool FtMadWarrior => IsActive(METAFEATURE.MADWARRIOR);
        public static bool FtRubbishChallenge => IsActive(METAFEATURE.RUBBISHCHALLENGE);
        public static bool FtBIKP1Skip => IsActive(METAFEATURE.BIKP1SKIP);
        public static bool FtDmgMod => IsActive(METAFEATURE.DMGMOD);
        public static bool FtNoDeath => IsActive(METAFEATURE.NODEATH);
        public static bool FtInfiniteStamina => IsActive(METAFEATURE.INFINITESTAMINA);
        public static bool FtRapierOHKO => IsActive(METAFEATURE.OHKO_RAPIER);
        public static bool FtFistOHKO => IsActive(METAFEATURE.OHKO_FIST);
        public static bool FtSpeedhack => IsActive(METAFEATURE.SPEEDHACK);
        public static bool FtNoGravity => IsActive(METAFEATURE.NOGRAVITY);
        public static bool FtNoCollision => IsActive(METAFEATURE.NOCOLLISION);
        public static bool FtDisableAi => IsActive(METAFEATURE.DISABLEAI);
        public static bool FtStorePosition => IsActive(METAFEATURE.STOREPOSITION);
        public static bool FtRestorePosition => IsActive(METAFEATURE.RESTOREPOSITION);
        public static bool FtWarp => IsActive(METAFEATURE.WARP);
        public static bool FtManageBfs => IsActive(METAFEATURE.MANAGEBFS);
        public static bool FtGiveSouls => IsActive(METAFEATURE.GIVESOULS);
        public static bool FtRestoreHumanity => IsActive(METAFEATURE.RESTOREHUMANITY);
        public static bool FtNewTestCharacter => IsActive(METAFEATURE.NEWTESTCHARACTER);

        public static bool FtDisableSkirt = IsActive(METAFEATURE.DISABLESKIRT);
    }
}
