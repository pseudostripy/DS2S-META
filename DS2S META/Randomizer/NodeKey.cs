using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    internal struct NodeKey
    {
        internal MapArea Area;
        internal List<KeySet> KSO; // KeySet Options?
        internal bool BadArea => Area == MapArea.Undefined || Area == MapArea.Quantum;

        // TO TIDY!
        internal bool IsKeyless => KSO.Count == 0
                        || (KSO.Count == 1 && KSO[0].HasKey(KEYID.NONE))
                        || KSO.Count == 1 && KSO[0].Keys.Count == 0;

        internal NodeKey(MapArea area, List<KeySet> kso)
        {
            Area = area;
            KSO = kso;
        }



        internal bool HasKeySet(KeySet querykso)
        {
            foreach (KeySet ks in KSO) 
            {
                if (ks == querykso)
                    return true;
            }
            return false;
        }
    }
}
