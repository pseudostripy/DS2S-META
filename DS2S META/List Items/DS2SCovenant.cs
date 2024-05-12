using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DS2S_META
{
    public enum COV : int
    {
        NONE = 0,
        HEIRSOFTHESUN = 1,
        BLUESENTINELS = 2,
        BROTHERHOODOFBLOOD = 3,
        WAYOFTHEBLUE = 4,
        RATKING = 5,
        BELLKEEPERS = 6,
        DRAGONREMNANTS = 7,
        COVENANTOFCHAMPIONS = 8,
        PILGRIMSOFDARKNESS = 9,
    }

    public class DS2SCovenant
    {
        public COV ID;
        public string Name;
        public Dictionary<int,int> RankLevels;

        public DS2SCovenant(COV iD, string name, Dictionary<int, int> rankLevels)
        {
            ID = iD;
            Name = name;
            RankLevels = rankLevels;
        }

        public override string ToString() => Name;
    }
}
