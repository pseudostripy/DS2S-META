using DS2S_META.Utils.Offsets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace DS2S_META.Utils.Subhook
namespace DS2S_META.Utils
{
    public class Covenant
    {
        public Covenant(COV id)
        {
            ID = id;
        }

        public Covenant(COV id, bool discov, byte rank, short progress) 
        {
            Discovered = discov;
            Rank = rank;
            Progress = progress;
            ID = id;
        }

        public COV ID;
        public bool Discovered { get; set; }
        public int Rank { get; set; }
        public int Progress { get; set; }
    }
}
