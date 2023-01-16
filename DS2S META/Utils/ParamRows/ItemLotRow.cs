using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace DS2S_META.Randomizer
{
    public class ItemWLotRow : ItemLotRow
    {
        // Constructors:
        public ItemWLotRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            //// Unpack data:
            //Items = ReadListAt((int)MINILOTS.ITEMID).Select(obj => BitConverter.ToInt32(obj)).ToList();
            //Quantities = ReadListAt((int)MINILOTS.QUANT).SelectMany(b => b).ToList();
            //Reinforcements = ReadListAt((int)MINILOTS.REINFORCEMENT).SelectMany(b => b).ToList();
            //Infusions = ReadListAt((int)MINILOTS.INFUSION).SelectMany(b => b).ToList();
            //Chances = ReadListAt((int)MINILOTS.CHANCES).Select(obj => BitConverter.ToSingle(obj)).ToList();

            IsDropTable = false;
            bool MalformedOrder = NumDrops != 0 && Quantities[NumDrops - 1] == 0; // see e.g. Ancient Dragon drop
            if (MalformedOrder)
                FixNonsense();
        }

        internal override int NumDrops => Quantities.Where(q => q != 0).Count();
        internal override ItemLotRow CloneBlank()
        {
            // Performs a deep clone on the Lot object
            var ilclone = new ItemWLotRow(Param, Name, ID, DataOffset)
            {
                _items = new List<int>(new int[10]),
                _quantities = new List<byte>(new byte[10]),
                _reinforcements = new List<byte>(new byte[10]),
                _infusions = new List<byte>(new byte[10])
            };

            return ilclone;
        }
    }
    public class ItemDropRow : ItemLotRow
    {
        // Constructors:
        public ItemDropRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            IsDropTable = true;

            // Order the fields so that only nonzero chance items exist (remove the rest)
            RemoveZeroPercentChance();
        }

        private void RemoveZeroPercentChance()
        {
            int clegit = 0; // counter of "legit"

            // copy info:
            var cpyIt = new List<int>();
            var cpyQ = new List<byte>();
            var cpyR = new List<byte>();
            var cpyI = new List<byte>();
            var cpyC = new List<float>();

            for (int i = 0; i < 10; i++)
            {
                if (Quantities[i] != 0 && Chances[i] != 0)
                {
                    cpyIt.Add(Items[i]);
                    cpyQ.Add(Quantities[i]);
                    cpyR.Add(Reinforcements[i]);
                    cpyI.Add(Infusions[i]);
                    cpyC.Add(Chances[i]);
                    clegit++;
                }
            }

            // Now overwrite the original bytes:
            for (int i = 0; i < cpyIt.Count; i++)
            {
                // Add to backend too (this is like an element-wise array property setter method)
                int id = i;
                StoreDataWrapper(MINILOTS.ITEMID, id, cpyIt[i]);
                StoreDataWrapper(MINILOTS.QUANT, id, cpyQ[i]);
                StoreDataWrapper(MINILOTS.REINFORCEMENT, id, cpyR[i]);
                StoreDataWrapper(MINILOTS.INFUSION, id, cpyI[i]);
                StoreDataWrapper(MINILOTS.CHANCES, id, cpyC[i]);
            }

            // Clear out the rest:
            for (int i = cpyIt.Count; i < 10; i++)
            {
                int id = i;
                StoreDataWrapper(MINILOTS.ITEMID, id, 0);
                StoreDataWrapper(MINILOTS.QUANT, id, 0);
                StoreDataWrapper(MINILOTS.REINFORCEMENT, id, 0);
                StoreDataWrapper(MINILOTS.INFUSION, id, 0);
                StoreDataWrapper(MINILOTS.CHANCES, id, 0);
            }
            updateLists();
        }
        internal override int NumDrops {
            get
            {
                int c = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (Quantities[i] == 0 || Chances[i] == 0)
                        break;    
                    c++;
                }
                return c;
            }
        }
        internal override ItemLotRow CloneBlank()
        {
            // Performs a deep clone on the Lot object
            var ilclone = new ItemDropRow(Param, Name, ID, DataOffset)
            {
                _items = new List<int>(new int[10]),
                _quantities = new List<byte>(new byte[10]),
                _reinforcements = new List<byte>(new byte[10]),
                _infusions = new List<byte>(new byte[10]),
                _chances = new List<float>(_chances) // clone chances from orig
            };

            return ilclone;
        }
    }

    public abstract class ItemLotRow : Param.Row
    {
        internal abstract ItemLotRow CloneBlank();
        
        // Fields:
        internal string? ParamDesc { get; set; }

        // Behind Fields:
        internal List<int> _items;
        internal List<byte> _quantities;
        internal List<byte> _reinforcements;
        internal List<byte> _infusions;
        internal List<float> _chances;

        // Property getters:
        internal List<int> Items => _items;
        internal List<byte> Quantities => _quantities;
        internal List<byte> Reinforcements => _reinforcements;
        internal List<byte> Infusions => _infusions;
        internal List<float> Chances => _chances;

        internal abstract int NumDrops { get; }
        internal List<DropInfo> Flatlist => GetFlatlist();
        internal bool IsEmpty => NumDrops == 0;

        internal bool IsDropTable;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumDrops; i++)
            {
                sb.Append($"Item[{i}] x{Quantities[i]}: {Items[i]:X} / {Items[i]}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }
        protected enum MINILOTS { ITEMID = 44, QUANT = 4, REINFORCEMENT = 14, INFUSION = 24, CHANCES = 54 }

        // Constructors:
        public ItemLotRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            // Unpack data:
            updateLists();
        }
                
        public List<byte[]> ReadListAt(int fieldindex)
        {
            // Get 10 at once for this specific param:
            List<byte[]> objout = new();
            var F = Param.Fields[fieldindex];
            for (int i = 0; i < 10; i++)
            {
                var outbytes = new byte[F.FieldLength];
                Array.Copy(RowBytes, F.FieldOffset + i * F.FieldLength, outbytes, 0, F.FieldLength);
                objout.Add(outbytes);
            }
            return objout;
        }

        // Methods:
        internal List<DropInfo> GetFlatlist()
        {
            List<DropInfo> flatlist = new();
            int legitdrops = 0;
            for (int i = 0; i < 10; i++) // 10 rows in loot tables
            {
                // See ancient dragon etc.
                if (Quantities[i] == 0)
                    continue;
                
                // Drops Only:
                if (IsDropTable && Chances[i] == 0)
                    continue;

                DropInfo di = new(Items[i], Quantities[i], Reinforcements[i], Infusions[i]);
                flatlist.Add(di);
                legitdrops++;

                // Check if finished
                if (legitdrops == NumDrops)
                    break;
            }
            return flatlist;
        }
        
        internal void CloneValuesFrom(ItemLotRow tocopy)
        {
            // be a little careful if you haven't blanked it properly before!
            for (int i = 0; i < 10; i++)
                AddDrop(tocopy.Items[i], tocopy.Quantities[i], tocopy.Reinforcements[i], tocopy.Infusions[i]);
        }
        

        internal void FixNonsense()
        {
            // This might get confusing.
            // We're zeroing out the amount in memory, but the Vanilla shop
            // still has its original copy of the amounts in the Quantities List.
            // So the NumDrops is still correct and we'll only replace (in memory)
            // enough to replicate the vanilla NumDrops. There'll still be one leftover
            // somewhere, but it's amount will be zeroed out in memory so we can 
            // ignore it.
            //
            // Note that all the lot randomization is done with the Quantities
            // list, not the underlying memory data for this class.
            for (int i = NumDrops; i <= 10; i++)
                StoreDataWrapper(MINILOTS.QUANT, i, 0); // force write 0 amount
            StoreRow(); // commit to memory

        }
        internal void AddDrop(int itemID, int quantity, int reinforce, int infusion)
        {
            AddDrop(new DropInfo(itemID, (byte)quantity, (byte) reinforce, (byte) infusion));
        }
        internal void AddDrop(DropInfo DI)
        {
            // This is the main way to adjust the fields in this class,
            // and handles the backend setting of the ParamRow bytes
            int id = NumDrops;
            if (id >= 10)
                throw new Exception("Trying to add too many DropInfos to this lot.");

            // Write to the fields:
            Items[id] = DI.ItemID;
            Quantities[id] = DI.Quantity;
            Reinforcements[id] = DI.Reinforcement;
            Infusions[id] = DI.Infusion;

            // Add to backend too (this is like an element-wise array property setter method)
            StoreDataWrapper(MINILOTS.ITEMID, id, Items[id]);
            StoreDataWrapper(MINILOTS.QUANT, id, Quantities[id]);
            StoreDataWrapper(MINILOTS.REINFORCEMENT, id, Reinforcements[id]);
            StoreDataWrapper(MINILOTS.INFUSION, id, Infusions[id]);
        }
        internal void SetDrop(DropInfo DI, int id)
        {
            // This is the main way to adjust the fields in this class,
            // and handles the backend setting of the ParamRow bytes
            if (id >= 10)
                throw new Exception("Index 'id' must be between 0 and 9");

            // Write to the fields:
            Items[id] = DI.ItemID;
            Quantities[id] = DI.Quantity;
            Reinforcements[id] = DI.Reinforcement;
            Infusions[id] = DI.Infusion;

            // Add to backend too (this is like an element-wise array property setter method)
            StoreDataWrapper(MINILOTS.ITEMID, id, Items[id]);
            StoreDataWrapper(MINILOTS.QUANT, id, Quantities[id]);
            StoreDataWrapper(MINILOTS.REINFORCEMENT, id, Reinforcements[id]);
            StoreDataWrapper(MINILOTS.INFUSION, id, Infusions[id]);
        }
        private Param.Field GetField(MINILOTS fieldindex)
        {
            // Trivial wrapper for convenience
            //return ParamRow.Param.Fields[(int)fieldindex];
            return Param.Fields[(int)fieldindex];
        }
        protected void StoreDataWrapper(MINILOTS fenum, int subindex, int value)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }
        protected void StoreDataWrapper(MINILOTS fenum, int subindex, float value)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }
        protected void StoreDataWrapper(MINILOTS fenum, int subindex, byte value)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }
        internal void updateLists()
        {
            _items = ReadListAt((int)MINILOTS.ITEMID).Select(obj => BitConverter.ToInt32(obj)).ToList();
            _quantities = ReadListAt((int)MINILOTS.QUANT).SelectMany(b => b).ToList();
            _reinforcements = ReadListAt((int)MINILOTS.REINFORCEMENT).SelectMany(b => b).ToList();
            _infusions = ReadListAt((int)MINILOTS.INFUSION).SelectMany(b => b).ToList();
            _chances = ReadListAt((int)MINILOTS.CHANCES).Select(obj => BitConverter.ToSingle(obj)).ToList();
        }

        // Query Utility
        internal bool HasItem(int itemid) => Items.Contains(itemid);
        internal int GetLotIndex(int itemid)
        {
            if (!HasItem(itemid))
                throw new Exception("Shouldn't be using this query when there's no item with this ID in list");

            for (int i = 0; i < 10; i++)
            {
                if (Items[i] == itemid)
                    return i;
            }
            throw new Exception("This should be impossible");
        }

    }
}
