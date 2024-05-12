using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SBonfire : IComparable<DS2SBonfire>
    {
        public ushort ID;
        public string Name;
        public int AreaID;
        public DS2SBonfireHub? Hub { get; set; } // parent Hub

        public static readonly DS2SBonfire EmptyBonfire = new(0, 0, "not found");
        public DS2SBonfire(int areaId, ushort id, string name)
        {
            ID = id;
            Name = name;
            AreaID = areaId;
        }
        public override string ToString() => Name;
        public int CompareTo(DS2SBonfire? other) => Name.CompareTo(other?.Name);
    }
}
