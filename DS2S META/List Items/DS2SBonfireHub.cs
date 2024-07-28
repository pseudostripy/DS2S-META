using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DS2S_META.Utils;

namespace DS2S_META
{
    public class DS2SBonfireHub : IComparable<DS2SBonfireHub>
    {
        public string Name;
        public List<string> BonfireNames = new();
        public List<DS2SBonfire> Bonfires = new();
        private readonly bool _isLinked = false;
        public bool IsLinked => _isLinked;

        public DS2SBonfireHub(string hubname, List<string> bfnames)
        {
            Name = hubname;
            BonfireNames = bfnames;
            _isLinked = false; // incomplete class
        }
        public DS2SBonfireHub(string hubname, List<string> bfnames, List<DS2SBonfire> bfs)
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
                    MetaExceptionStaticHandler.Raise("Bonfire Hub cannot be linked to bonfire. Check resources for typos");
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
    }
}
