using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// "DropInfo Set" container for groups of items which need their own logic
    /// </summary>
    internal class Diset
    {
        // Enums:
        internal enum SetType : byte { Keys, Reqs, Gens }

        // Fields
        public SetType Type { get; set; }
        public IEnumerable<DropInfo> Data { get; set; }

        public bool IsKeys => Type == SetType.Keys;

        // Constructors
        public Diset(SetType type, IEnumerable<DropInfo> data) 
        { 
            Data = data;
            Type = type;
        }

        // Handy wrapper factory methods to wrap enum
        public static Diset FromKeys(IEnumerable<DropInfo> data) => new(SetType.Keys, data);
        public static Diset FromReqs(IEnumerable<DropInfo> data) => new(SetType.Reqs, data);
        public static Diset FromGens(IEnumerable<DropInfo> data) => new(SetType.Gens, data);
    }
}
