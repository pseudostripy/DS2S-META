using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SBonfire : IComparable<DS2SBonfire>
    {
        private static readonly Regex BonfireEntryRx = new(@"^(?<area>\S+) (?<id>\S+) (?<name>.+)$");

        public string Name;
        public ushort ID;
        public int AreaID;
        public DS2SBonfireHub? Hub { get; set; } // parent Hub

        public DS2SBonfire(int areaId, ushort id, string name)
        {
            ID = id;
            Name = name;
            AreaID = areaId;
        }
        public override string ToString() => Name;
        public int CompareTo(DS2SBonfire? other)
        {
            return Name.CompareTo(other?.Name);
        }

        public static DS2SBonfire ParseNew(string line)
        {
            Match m = BonfireEntryRx.Match(line);
            var areaId = Convert.ToInt32(m.Groups["area"].Value);
            var id = Convert.ToUInt16(m.Groups["id"].Value);
            var name = m.Groups["name"].Value;
            return new DS2SBonfire(areaId, id, name);
        }
    }
}
