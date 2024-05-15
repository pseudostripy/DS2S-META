using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    public static class OffLocFactory
    {
        public static List<OffsetLocator> Create(params Tuple<string,int>[] defns)
        {
            return defns.Select(tup => new OffsetLocator(tup.Item1, tup.Item2)).ToList();
        }
    }
}
