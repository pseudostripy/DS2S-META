using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DS2S_META.Utils;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Parent class combining Vanilla and Shuffled Data for Items/Shop subclasses
    /// </summary>
    public abstract class Randomization
    {
        public static double lowestPriceRate = 0.9;

        // Fields
        public int ParamID;
        public abstract List<DropInfo> Flatlist { get; }
        public RDZ_TASKTYPE Type // HandlingType for RandoMan
        {
            get => RandoInfo.RandoHandleType;
            set { RandoInfo.RandoHandleType = value; }
        }
        public bool IsHandled = false;
        public string RandoDesc = string.Empty;
        public string GUID;
        public RandoInfo RandoInfo;
        public int PlaceDist = -1;

        // Constructors:
        public Randomization(int pid, RandoInfo ri, RDZ_TASKTYPE status)
        {
            GUID = Guid.NewGuid().ToString();
            ParamID = pid;
            RandoInfo = ri;
            Type = status;
        }

        // Abstract Methods:
        public abstract string PrintData();
        public abstract bool IsSaturated();
        public abstract void AddShuffledItem(DropInfo item);
        public bool HasShuffledItemId(ITEMID itemid) => HasShuffledItemId((int)itemid);
        public abstract bool HasShuffledItemId(int itemID);
        public abstract bool HasVanillaItemID(int itemID);
        public bool HasVanillaItemID(ITEMID id) => HasVanillaItemID((int)id);
        public bool HasVanillaAnyItemID(IEnumerable<int> itemlist) => itemlist.Any(i => HasVanillaItemID(i)); // true if any are found
        public bool HasVanillaAnyItemID(IEnumerable<ITEMID> itemlist) => itemlist.Any(i => HasVanillaItemID(i)); // true if any are found
        public abstract int GetShuffledItemQuant(int itemID);
        public abstract string GetNeatDescription();
        public abstract string GetNeatDescriptionNoId(int itemId, out string area);
        public abstract void AdjustQuantity(DropInfo di);
        public abstract void ResetShuffled();
        public abstract int UniqueParamID { get; }

        // Common Methods:

        // Wrappers to RandoInfo
        public bool HasPickupType(IEnumerable<PICKUPTYPE> types) => RandoInfo?.HasType(types) == true;
        public bool HasPickupType(PICKUPTYPE type) => RandoInfo?.HasType(type) == true;
        public bool ContainsOnlyTypes(List<PICKUPTYPE> onlytpes)
        {
            if (RandoInfo == null) return false; // TODO?
            return RandoInfo.ContainsOnlyTypes(onlytpes);
        }
        protected static int RoundUpNearestMultiple(int val, int m)
        {
            return (int)Math.Ceiling((double)val / m) * m;
        }
        protected static int GetTypeRandomPrice(int itemid)
        {
            var item = itemid.TryAsItemRow();
            if (item == null)
                return Rng.RandomGammaInt(3000, 50); // generic guess

            return item.ItemType switch
            {
                eItemType.AMMO => Rng.RandomGammaInt(100, 10),
                eItemType.CONSUMABLE => GetConsumableRandomPrice(item.ItemID),
                eItemType.WEAPON1 or eItemType.WEAPON2 => Rng.RandomGammaInt(5000, 100),
                _ => Rng.RandomGammaInt(3000, 50),
            };
        }
        protected static int GetConsumableRandomPrice(int itemid)
        {
            // Add more rules here as appropriate:
            if (ItemSetBase.SoulPriceList.ContainsKey(itemid))
            {
                var souls = ItemSetBase.SoulPriceList[itemid];
                var ranval = Rng.RandomGammaInt(souls, 50);
                return (int)Math.Max(ranval, lowestPriceRate * souls); // Limit to 10% off best sale
            }

            var lowtier = new List<int>() { 60010000, 60040000, 60595000, 60070000 }; // lifegem, amber herb, dung pie, poison moss
            if (lowtier.Contains(itemid))
                return Rng.RandomGammaInt(400, 50);

            // Otherwise:
            return Rng.RandomGammaInt(2000, 50);
        }
        public static void AdjustQuantityParameterized(DropInfo di, int maxconsumquant)
        {
            if (maxconsumquant > 255) throw new Exception("Please use number <= 255");

            var itype = di.AsItemRow().ItemType;
            switch (itype)
            {
                case eItemType.AMMO:
                    if (di.Quantity == 255)
                        di.Quantity = 50; // reset to reasonable value

                    // Otherwise round to nearest 10 ceiling
                    di.Quantity = (byte)RoundUpNearestMultiple(di.Quantity, 10);
                    return;

                case eItemType.CONSUMABLE:
                    if (di.Quantity == 255)
                        di.Quantity = (byte)maxconsumquant;
                    return;

                default:
                    // Everything else:
                    di.Quantity = 1;
                    return;
            }
        }


        

        public bool IsStandardHT => Type == RDZ_TASKTYPE.STANDARD;
        public bool IsExcludedHT => Type == RDZ_TASKTYPE.EXCLUDE;
        public void MarkHandled()
        {
            IsHandled = true;
        }
    }
}
