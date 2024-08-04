using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META
{
    public enum DS2VER
    {
        VANILLA_V112,
        VANILLA_V111,
        VANILLA_V102,
        SOTFS_V102,
        SOTFS_V103,
        UNSUPPORTED
    }

    public static class DS2Versions
    {
        public readonly static List<DS2VER> ANYVER = new() { DS2VER.SOTFS_V103, DS2VER.SOTFS_V102, DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> ANYSOTFS = new() { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 };
        public readonly static List<DS2VER> ANYVANILLA = new() { DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> ALLBUTOLDPATCH = new() { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> V102 = new() { DS2VER.VANILLA_V102 };
        public readonly static List<DS2VER> V111 = new() { DS2VER.VANILLA_V111 };
        public readonly static List<DS2VER> V112 = new() { DS2VER.VANILLA_V112 };
        public readonly static List<DS2VER> S102 = new() { DS2VER.SOTFS_V102 };
        public readonly static List<DS2VER> S103 = new() { DS2VER.SOTFS_V103 };
        public readonly static List<DS2VER> V111OR112 = new() { DS2VER.VANILLA_V111, DS2VER.VANILLA_V111 };
    }
}
