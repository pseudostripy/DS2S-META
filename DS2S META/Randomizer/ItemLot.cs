using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace DS2S_META.Randomizer
{
    internal class ItemLot
    {
        // Fields:
        internal Param.Row ParamRow; // Raw DS2 memory
        internal int ID => ParamRow.ID; // shorthand

        // Fields:
        internal string? ParamDesc { get; set; }
        //internal List<DropInfo> Lot = new();
        
        // Properties:
        internal List<int> Items = new();
        internal List<byte> Quantities = new();
        internal List<byte> Reinforcements = new();
        internal List<byte> Infusions = new();

        internal int NumDrops => Quantities.Where(q => q != 0).Count();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumDrops; i++)
            {
                sb.Append($"Item[{i}] x{Quantities[i]}: {Items[i]:X} / {Items[i]}\n");
            }
            return sb.ToString().TrimEnd('\n');
        }
        private enum MINILOTS { ITEMID = 44, QUANT = 4, REINFORCEMENT = 14, INFUSION = 24}

        // Constructors:
        internal ItemLot() { }
        internal ItemLot(Param.Row itemlotrow)
        {
            // Unpack data:
            ParamRow = itemlotrow;

            Items = ReadListAt((int)MINILOTS.ITEMID).Select(obj => BitConverter.ToInt32(obj)).ToList();
            Quantities = ReadListAt((int)MINILOTS.QUANT).SelectMany(b => b).ToList();
            Reinforcements = ReadListAt((int)MINILOTS.REINFORCEMENT).SelectMany(b => b).ToList();
            Infusions = ReadListAt((int)MINILOTS.INFUSION).SelectMany(b => b).ToList();

            bool MalformedOrder = NumDrops != 0 && Quantities[NumDrops - 1] == 0; // see e.g. Ancient Dragon drop
            if (MalformedOrder)
                FixNonsense();

        }
                
        public List<byte[]> ReadListAt(int fieldindex)
        {
            // Get 10 at once for this specific param:
            List<byte[]> objout = new();
            var F = ParamRow.Param.Fields[fieldindex];
            for (int i = 0; i < 10; i++)
            {
                var outbytes = new byte[F.FieldLength];
                Array.Copy(ParamRow.RowBytes, F.FieldOffset + i * F.FieldLength, outbytes, 0, F.FieldLength);
                objout.Add(outbytes);
            }
            return objout;
        }

        // Methods:
        internal List<DropInfo> GetFlatlist()
        {
            List<DropInfo> flatlist = new();
            for (int i = 0; i < NumDrops; i++)
            {
                DropInfo di = new DropInfo(Items[i], Quantities[i], Reinforcements[i], Infusions[i]);
                flatlist.Add(di);
            }
            return flatlist;
        }
        internal ItemLot CloneBlank()
        {
            // Performs a deep clone on the Lot object
            var ilclone = new ItemLot(ParamRow);
            ilclone.Items = new List<int>( new int[10] );
            ilclone.Quantities = new List<byte>(new byte[10]);
            ilclone.Reinforcements = new List<byte>(new byte[10]);
            ilclone.Infusions = new List<byte>(new byte[10]);

            return ilclone;
        }
        

        internal void FixNonsense()
        {
            // This might get confusing.
            // We're zeroing out the amount in memory, but the Vanilla shop
            // still has its original copy of the amounts in the Quantities List.
            // So the NumDrops is still correct and we'll only replace (in memory)
            // enough to replicate the vanilla NumDrops. There's still be one leftover
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
            // and handled the backend setting of the ParamRow bytes
            int id = NumDrops;
            if (id > 10)
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
        private Param.Field GetField(MINILOTS fieldindex)
        {
            // Trivial wrapper for convenience
            return ParamRow.Param.Fields[(int)fieldindex];
        }
        private void StoreDataWrapper(MINILOTS fenum, int subindex, int value)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, ParamRow.RowBytes, ind, bytes.Length);
        }
        private void StoreDataWrapper(MINILOTS fenum, int subindex, byte value)
        {
            var F = GetField(fenum);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, ParamRow.RowBytes, ind, bytes.Length);
        }
        public void StoreRow()
        {
            // Convenience wrapper
            ParamRow.Param.StoreRowBytes(ParamRow);
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
