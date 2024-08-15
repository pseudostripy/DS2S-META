using DS2S_META.Utils.DS2Hook;
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
        COVENANTINFO,
        MAXLEVELS,
        RESETTOCLASSLEVELS,
        RESETSOULMEMORY,
        DISABLEPOISONBUILDUP,
        INFINITESTAMINA,
        INFINITESPELLS,
        DISABLEPARTYWALKTIMER,
        INFINITEGOODS,
        DISABLESKIRTPOISON,
    }
    public static class MetaFeature
    {
        private static DS2SHook? Hook;
        public static void Initialize(DS2SHook hook)
        {
            Hook = hook;
        }

        // shorthand wrappers
        private static bool IsValidVer => Hook?.VerMan.IsValidVer == true;
        private static bool InGame => Hook?.InGame == true;
        private static bool IsSOTFS_CP => Hook?.VerMan.IsSOTFS_CP == true;
        private static bool IsSOTFS => Hook?.VerMan.IsSOTFS == true;
        private static bool IsOldPatch => Hook?.VerMan.IsOldPatch == true;

        // Helpful query:
        public static bool IsActive(METAFEATURE feat)
        {
            if (Hook?.Hooked != true) return false; // no way of figuring out versions

            // can add extra specific switches on Versions too soon...
            // aka if the Offsets are defined but it's not working
            // in certain versions, can veto here.
            return feat switch
            {
                METAFEATURE.OHKO_FIST => IsValidVer && InGame,
                METAFEATURE.OHKO_RAPIER => IsValidVer && InGame,
                METAFEATURE.NOGRAVITY => IsValidVer && InGame,
                METAFEATURE.NOCOLLISION => IsValidVer && InGame,
                METAFEATURE.NODEATH => IsValidVer && InGame,
                METAFEATURE.DISABLEAI => InGame && (IsSOTFS || IsOldPatch),
                METAFEATURE.GIVE17KREWARD => IsValidVer && InGame,
                METAFEATURE.GIVE3CHUNK1SLAB => IsValidVer && InGame,
                METAFEATURE.MADWARRIOR => IsSOTFS_CP, // sotfs 1.03 only
                METAFEATURE.RUBBISHCHALLENGE => false, // not working in any versions atm
                METAFEATURE.BIKP1SKIP => IsSOTFS_CP,
                METAFEATURE.SPEEDHACK => IsValidVer,
                METAFEATURE.STOREPOSITION => IsValidVer && InGame,
                METAFEATURE.RESTOREPOSITION => IsValidVer && InGame,
                METAFEATURE.WARP => IsValidVer && Hook.InGame,
                METAFEATURE.DMGMOD => IsSOTFS_CP,
                METAFEATURE.MANAGEBFS => InGame,
                METAFEATURE.GIVESOULS => IsValidVer && InGame,
                METAFEATURE.RESETSOULMEMORY => IsValidVer && InGame,
                METAFEATURE.RESTOREHUMANITY => IsValidVer && InGame,
                METAFEATURE.NEWTESTCHARACTER => IsValidVer && InGame,
                METAFEATURE.COVENANTINFO => IsSOTFS && InGame,
                METAFEATURE.MAXLEVELS => IsValidVer && InGame,
                METAFEATURE.RESETTOCLASSLEVELS => IsValidVer && InGame,
                METAFEATURE.DISABLEPOISONBUILDUP => IsSOTFS && InGame,
                METAFEATURE.DISABLESKIRTPOISON => IsSOTFS && InGame,
                METAFEATURE.INFINITESTAMINA => InGame && (IsSOTFS || IsOldPatch),
                METAFEATURE.INFINITESPELLS => InGame && (IsSOTFS || IsOldPatch),
                METAFEATURE.DISABLEPARTYWALKTIMER => InGame && IsSOTFS_CP,
                METAFEATURE.INFINITEGOODS => InGame && IsSOTFS_CP,
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
        public static bool FtResetSoulMemory => IsActive(METAFEATURE.RESETSOULMEMORY);
        public static bool FtRestoreHumanity => IsActive(METAFEATURE.RESTOREHUMANITY);
        public static bool FtNewTestCharacter => IsActive(METAFEATURE.NEWTESTCHARACTER);
        public static bool FtCovenantInfo => IsActive(METAFEATURE.COVENANTINFO);
        public static bool FtMaxLevels => IsActive(METAFEATURE.MAXLEVELS);
        public static bool FtResetToClassLevels => IsActive(METAFEATURE.RESETTOCLASSLEVELS);
        public static bool FtDisablePoisonBuildup => IsActive(METAFEATURE.DISABLEPOISONBUILDUP);
        public static bool FtDisableSkirtPoison => IsActive(METAFEATURE.DISABLESKIRTPOISON);
        public static bool FtInfiniteStamina => IsActive(METAFEATURE.INFINITESTAMINA);
        public static bool FtInfiniteSpells => IsActive(METAFEATURE.INFINITESPELLS);
        public static bool FtDisablePartyWalkTimer => IsActive(METAFEATURE.DISABLEPARTYWALKTIMER);
        public static bool FtInfiniteGoods => IsActive(METAFEATURE.INFINITEGOODS);
    }
}
