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
        public int[] Offsets;

        public VerOffs(List<DS2VER> validVers, params int[] offsets)
        {
            ValidVersions = validVers;
            Offsets = offsets;
        }



        public override PHPointer Register()
        {
            return null;
        }
    }
}
