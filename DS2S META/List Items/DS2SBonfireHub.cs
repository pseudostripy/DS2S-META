using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DS2S_META
{
    public class DS2SBonfireHub : IComparable<DS2SBonfireHub>
    {
        // be careful on names with ":" inside them. Regex won't catch it.
        private static readonly Regex BonfireHubEntryRx = new(@"^(?<name>.+?):\s+(?<bonfires>.+)$");

        public string Name;
        public List<string> BonfireNames = new();
        public List<DS2SBonfire> Bonfires = new();
        private readonly bool _isLinked = false;
        public bool IsLinked => _isLinked;

        private DS2SBonfireHub(string hubname, List<string> bfnames)
        {
            Name = hubname;
            BonfireNames = bfnames;
            _isLinked = false; // incomplete class
        }
        private DS2SBonfireHub(string hubname, List<string> bfnames, List<DS2SBonfire> bfs)
        {
            Name = hubname;
            BonfireNames = bfnames;
            Bonfires = bfs;
            _isLinked = true; // complete class
        }

        public static DS2SBonfireHub LinkBonfireObjects(DS2SBonfireHub prehub, List<DS2SBonfire> allbonfires)
        {
            List<DS2SBonfire> bfs = new();
            foreach (var str in prehub.BonfireNames)
            {
                var bf = allbonfires.Where(xbf => xbf.Name == str).FirstOrDefault();
                if (bf == null)
                {
                    MetaException.Raise("Bonfire Hub cannot be linked to bonfire. Check resources for typos");
                    break;
                }
                bfs.Add(bf);
            }
            return new DS2SBonfireHub(prehub.Name, prehub.BonfireNames, bfs);
        }

        public override string ToString() => Name;
        public int CompareTo(DS2SBonfireHub? other)
        {
            return Name.CompareTo(other?.Name);
        }

        public static DS2SBonfireHub ParseNew(string line)
        {
            Match m = BonfireHubEntryRx.Match(line);

            var name = m.Groups["name"].Value;
            var bfnames = m.Groups["bonfires"].Value
                                .Split('-')
                                .Select(x => x.Trim())
                                .ToList();
            return new DS2SBonfireHub(name, bfnames);
        }
    }
}
