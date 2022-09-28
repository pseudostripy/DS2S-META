using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    class DS2SBonfireHub : IComparable<DS2SBonfireHub>
    {
        private static Regex BonfireHubEntryRx = new Regex(@"^(?<name>(.*:)+)\s+(?<bonfires>.+)$");

        public string Name;
        public List<string> Bonfires = new List<string>();

        private DS2SBonfireHub(string config)
        {
            Match bonfireEntry = BonfireHubEntryRx.Match(config);
            Name = bonfireEntry.Groups["name"].Value.Replace(":", "");

            foreach (var bonfire in bonfireEntry.Groups["bonfires"].Value.Split('-'))
            {
                Bonfires.Add(bonfire.Trim());
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(DS2SBonfireHub? other)
        {
            return Name.CompareTo(other?.Name);
        }

        public static List<DS2SBonfireHub> All = new List<DS2SBonfireHub>();

        static DS2SBonfireHub()
        {
            foreach (string line in Regex.Split(GetTxtResourceClass.GetTxtResource("Resources/Systems/BonfireHubs.txt"), "[\r\n]+"))
            {
                if (GetTxtResourceClass.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    All.Add(new DS2SBonfireHub(line));
            };
        }
    }
}
