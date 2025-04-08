using DS2S_META.Utils.ParamRows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Generalized Lot Randomization
    /// </summary>
    public abstract class GLotRdz<T> : Randomization where T : ItemLotBaseRow
    {
        // Subclass fields:
        public T VanillaLot { get; set; }
        public T ShuffledLot { get; set; }
        public bool IsDropTable = false; // by default

        // Constructors:
        public GLotRdz(T vanlot, RandoInfo ri, RDZ_TASKTYPE status) : base(vanlot.ID, ri, status)
        {
            VanillaLot = vanlot;
            ShuffledLot = (T)VanillaLot.CloneBlank();
        }

        // Methods
        public override bool IsSaturated() => ShuffledLot != null && ShuffledLot.NumDrops == VanillaLot.NumDrops;
        public override List<DropInfo> Flatlist
        {
            get
            {
                var flatlist = VanillaLot.GetFlatlist();
                return flatlist;
            }
        }
        public override void AddShuffledItem(DropInfo di)
        {
            // Fix quantity:
            AdjustQuantity(di);

            if (ShuffledLot == null)
                throw new NullReferenceException("Shuffled lot should have been cloned from vanilla!");

            ShuffledLot.AddDrop(di);

            if (ShuffledLot.NumDrops > VanillaLot?.NumDrops)
                throw new Exception("Shouldn't be able to get here!");
        }
        public override bool HasShuffledItemId(int itemID)
        {
            if (ShuffledLot == null)
                return false;
            return ShuffledLot.Items.Contains(itemID);
        }
        public override bool HasVanillaItemID(int itemID)
        {
            if (VanillaLot == null)
                return false;
            return VanillaLot.Items.Contains(itemID);
        }
        public override int GetShuffledItemQuant(int itemID)
        {
            if (ShuffledLot == null)
                return -1;

            // get full total
            return ShuffledLot.Flatlist.FilterByItem(itemID)
                                .Select(di => (int)di.Quantity)
                                .Sum();
        }
        public override string GetNeatDescription()
        {
            StringBuilder sb = new($"{ParamID}: {VanillaLot?.ParamDesc}{Environment.NewLine}");

            // TODO: (distances of interest are not -1)
            if (PlaceDist != -1)
                sb.Append($"Placement Distance: {PlaceDist}{Environment.NewLine}");

            // Display empty lots
            if (ShuffledLot == null || ShuffledLot.NumDrops == 0)
                return sb.Append("\tEMPTY").ToString();

            for (int i = 0; i < ShuffledLot.NumDrops; i++)
            {
                sb.Append($"\t{ShuffledLot.Items[i].AsMetaName()}");
                sb.Append($" x{ShuffledLot.Quantities[i]}");
                sb.Append(Environment.NewLine);
            }

            // Remove final newline:
            return sb.ToString().TrimEnd('\r', '\n');
        }
                
        public override void AdjustQuantity(DropInfo di) => AdjustQuantityParameterized(di, 5);
        public override string PrintData()
        {
            throw new NotImplementedException();
        }
        public override void ResetShuffled()
        {
            ShuffledLot = (T)VanillaLot.CloneBlank();
            PlaceDist = -1;
            IsHandled = false;
        }
        public override int UniqueParamID => IsDropTable ? ParamID + 80000000 : ParamID;


        public List<DropInfo> GetUniqueFlatlist(List<DropInfo> avoid_these)
        {
            // Return a flat list of drops that do not overlap with the supplied ones.
            // This is a way to remove the NGPlus duplicates which are unchanged.
            List<DropInfo> res = new();
            foreach (var di in Flatlist)
            {
                if (avoid_these.Any(di2 => di2.IsEqualTo(di)))
                    continue;
                res.Add(di);
            }
            return res;
        }

        // Extra Utility:
        public bool IsEmpty => VanillaLot.IsEmpty; // Vanilla Lot has 0 drops
    }
}

