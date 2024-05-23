using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets
{
    // Intermediary class used for cleaner looking inputs only
    public class VerOffs
    {
        public List<DS2VER> ValidVersions;
        public int Offset;

        public VerOffs(List<DS2VER> validVers, int offset)
        {
            ValidVersions = validVers;
            Offset = offset;
        }
    }
}
