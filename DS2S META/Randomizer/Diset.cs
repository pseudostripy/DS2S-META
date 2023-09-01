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
        internal enum SetType : byte { TrueKeys, TrashKeys, Reqs, Gens }

        // Fields
        public SetType Type { get; set; }
        public List<DropInfo> Data { get; set; }

        public bool IsKeys => Type == SetType.TrueKeys || Type == SetType.TrashKeys;
        public bool IsTrueKeys => Type == SetType.TrueKeys;
        public bool IsTrashKeys => Type == SetType.TrashKeys;
        public bool IsReqs => Type == SetType.Reqs;
        public bool IsGens => Type == SetType.Gens;

        // Constructors
        public Diset(SetType type, List<DropInfo> data) 
        { 
            Data = data;
            Type = type;
        }

        // Handy wrapper factory methods to wrap enum
        public static Diset FromTrueKeys(List<DropInfo> data) => new(SetType.TrueKeys, data);
        public static Diset FromTrashKeys(List<DropInfo> data) => new(SetType.TrashKeys, data);
        public static Diset FromReqs(List<DropInfo> data) => new(SetType.Reqs, data);
        public static Diset FromGens(List<DropInfo> data) => new(SetType.Gens, data);
    }
}
