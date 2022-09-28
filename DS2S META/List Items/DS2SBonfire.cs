using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    class DS2SBonfire : IComparable<DS2SBonfire>
    {
        private static Regex BonfireEntryRx = new Regex(@"^(?<area>\S+) (?<id>\S+) (?<name>.+)$");

        public string Name;
        public ushort ID;
        public int AreaID;

        private DS2SBonfire(string config)
        {
            Match bonfireEntry = BonfireEntryRx.Match(config);

            Name = bonfireEntry.Groups["name"].Value;
            ID = Convert.ToUInt16(bonfireEntry.Groups["id"].Value);
            AreaID = Convert.ToInt32(bonfireEntry.Groups["area"].Value);
        }
        public DS2SBonfire(int areaId, ushort id, string name)
        {
            ID = id;
            Name = name;
            AreaID = areaId;
        }
        public override string ToString()
        {
            return Name;
        }
        public int CompareTo(DS2SBonfire? other)
        {
            return Name.CompareTo(other?.Name);
        }

        public static List<DS2SBonfire> All = new List<DS2SBonfire>();

        static DS2SBonfire()
        {
            foreach (string line in Regex.Split(GetTxtResourceClass.GetTxtResource("Resources/Systems/Bonfires.txt"), "[\r\n]+"))
            {
                if (GetTxtResourceClass.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    All.Add(new DS2SBonfire(line));
            };
            All.Sort();
        }
    }
}
