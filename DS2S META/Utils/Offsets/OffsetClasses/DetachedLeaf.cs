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
    public class DetachedLeaf
    {
        public string Id; // fieldname
        public List<VerOffs> VerOffs;
        
        public DetachedLeaf(string id, params VerOffs[] veroffs)
        {
            Id = id;
            VerOffs = veroffs.ToList();
        }

        // Even shorter hand
        public DetachedLeaf(string id, params (List<DS2VER>, int)[] veroffs)
        {
            Id = id;
            VerOffs = veroffs.Select(vo => new VerOffs(vo.Item1, vo.Item2)).ToList();
        }

    }
}
