using DS2S_META.Randomizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.ParamRows
{
    /// <summary>
    /// Underlying row shared between ItemLotRow and ItemDropRow which
    /// look the same but should be interpreted differently
    /// </summary>
    public abstract class ItemLotBaseRow : Param.Row
    {
        internal abstract ItemLotBaseRow CloneBlank();

        // Fields:
        internal string? ParamDesc { get; set; }

        // Behind Fields (explicitly declare that they will be set before usage)
        internal List<int> _items = null!;
        internal List<byte> _quantities = null!;
        internal List<byte> _reinforcements = null!;
        internal List<byte> _infusions = null!;
        internal List<byte> _repeatables = null!;
        internal List<float> _chances = null!;
        internal List<byte>  _unknbytes = null!;

        // Property getters:
        internal List<int> Items => _items;
        internal List<byte> Quantities => _quantities;
        internal List<byte> Reinforcements => _reinforcements;
        internal List<byte> Infusions => _infusions;
        internal List<byte> Repeatables => _repeatables; // currently not edited
        internal List<float> Chances => _chances;
        internal List<byte> UnknBytes => _unknbytes;
        internal int UnknByte0x0 => UnknBytes.ElementAt(0);
        internal int UnknByte0x1 => UnknBytes.ElementAt(1);

        internal abstract int NumDrops { get; }
        internal List<DropInfo> Flatlist => GetFlatlist();
        internal bool IsEmpty => NumDrops == 0;

        internal bool IsDropTable;

        public override string ToString()
        {
            StringBuilder sb = new();
            for (int i = 0; i < NumDrops; i++)
            {
                sb.Append($"Item[{i}] x{Quantities[i]}: {Items[i]:X} / {Items[i]}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }
        public enum MINILOTS {  // Note, these are in #fields not #bytes from start.
                                UNKNBYTE = 0,
                                QUANT = 4, 
                                REINFORCEMENT = 14,
                                INFUSION = 24,
                                REPEATABLE = 34,
                                ITEMID = 44,
                                CHANCES = 54 }

        // Constructors:
        public ItemLotBaseRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            UpdateLists();
        }
        protected void UpdateLists()
        {
            _unknbytes = ReadListOfSizeAt(4, (int)MINILOTS.UNKNBYTE).SelectMany(b => b).ToList();
            _items = ReadListAt((int)MINILOTS.ITEMID).Select(obj => BitConverter.ToInt32(obj)).ToList();
            _quantities = ReadListAt((int)MINILOTS.QUANT).SelectMany(b => b).ToList();
            _reinforcements = ReadListAt((int)MINILOTS.REINFORCEMENT).SelectMany(b => b).ToList();
            _infusions = ReadListAt((int)MINILOTS.INFUSION).SelectMany(b => b).ToList();
            _repeatables = ReadListAt((int)MINILOTS.REPEATABLE).SelectMany(b => b).ToList();
            _chances = ReadListAt((int)MINILOTS.CHANCES).Select(obj => BitConverter.ToSingle(obj)).ToList();
        }

        public List<byte[]> ReadListOfSizeAt(int listsz, int fieldindex)
        {
            // Generalized wrapper to handle non 10-element lists
            // Get 10 at once for this specific param:
            List<byte[]> objout = new();
            var F = Param.Fields[fieldindex];
            for (int i = 0; i < listsz; i++)
            {
                var outbytes = new byte[F.FieldLength];
                Array.Copy(RowBytes, F.FieldOffset + i * F.FieldLength, outbytes, 0, F.FieldLength);
                objout.Add(outbytes);
            }
            return objout;
        }
        public List<byte[]> ReadListAt(int fieldindex) => ReadListOfSizeAt(10, fieldindex);

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

        internal void AddDrop(int itemID, int quantity, int reinforce, int infusion)
        {
            AddDrop(new DropInfo(itemID, (byte)quantity, (byte)reinforce, (byte)infusion));
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
            StoreItem(id, Items[id]);
            StoreQuantity(id, Quantities[id]);
            StoreReinforce(id, Reinforcements[id]);
            StoreInfusion(id, Infusions[id]);
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
            StoreItem(id, Items[id]);
            StoreQuantity(id, Quantities[id]);
            StoreReinforce(id, Reinforcements[id]);
            StoreInfusion(id, Infusions[id]);
        }

        // Storing/Overwriting:
        private Param.Field GetField(MINILOTS fieldindex)
        {
            // Trivial wrapper for convenience
            return Param.Fields[(int)fieldindex];
        }
        private void StoreVal(MINILOTS fenum, int subindex, byte[] bytes)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }

        // Exposed-interfaces
        public void StoreUnkn(int subindex, byte v) => StoreVal(MINILOTS.UNKNBYTE, subindex, v.AsByteArray());
        public void StoreQuantity(int subindex, byte v) => StoreVal(MINILOTS.QUANT, subindex, v.AsByteArray());
        public void StoreReinforce(int subindex, byte v) => StoreVal(MINILOTS.REINFORCEMENT, subindex, v.AsByteArray());
        public void StoreInfusion(int subindex, byte v) => StoreVal(MINILOTS.INFUSION, subindex, v.AsByteArray());
        public void StoreRepeatable(int subindex, byte v) => StoreVal(MINILOTS.REPEATABLE, subindex, v.AsByteArray());
        public void StoreItem(int subindex, int v) => StoreVal(MINILOTS.ITEMID, subindex, BitConverter.GetBytes(v));
        public void StoreChance(int subindex, float v) => StoreVal(MINILOTS.CHANCES, subindex, BitConverter.GetBytes(v));


        // Query Utility
        internal bool HasItem(int itemid) => Items.Contains(itemid);
    }
}
