using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DS2S_META.Utils
{
    /// <summary>
    /// Data Class for storing starting items / perhaps test characters
    /// </summary>
    public class PlayerStatusItemRow : Param.Row
    {
        // Fields:
        internal FieldList<int> Items;
        internal FieldList<short> Amounts;
        

        // Constructor:
        public PlayerStatusItemRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            Items = new FieldList<int>(20, "Item ID 0", this);
            Amounts = new FieldList<short>(20, "Item Amount 0", this);
        }

        public void Wipe()
        {
            // Items
            Items.Wipe(-1); // set all to -1
            Amounts.Wipe(0);
        }
    }



    public class FieldList<T>
    {
        public int N;               // Number of items in list
        public Param.Row Row;       // Parent Param.Row
        public int FirstFieldInd;   // Index of Field of first element of list
        public Param.Field F;       // Underlying field type
        public List<T> Data;        // Underlying data from memory

        public FieldList(int n, string field1name, Param.Row prow)
        {
            N = n;
            Row = prow;
            FirstFieldInd = Row.GetFieldIndex(field1name);
            F = prow.Param.Fields[FirstFieldInd];
            
            // Populate list:
            Data = new List<T>();
            for (int i = 0; i < N; i++)
                Data.Add( (T)Row.ReadAtFieldNum(FirstFieldInd + i) );
            
        }

        public byte[] GetTBytes(T value)
        {
            // Switch case to avoid using reflection invocation
            // for only my common types

            return value switch
            {
                int i32 => BitConverter.GetBytes(i32),
                uint u32 => BitConverter.GetBytes(u32),
                byte b => BitConverter.GetBytes((short)b),
                short s16 => BitConverter.GetBytes(s16),
                ushort u16 => BitConverter.GetBytes(u16),
                _ => throw new Exception("Type not handled"),
            };
        }
        public void Wipe(T memsetval)
        {
            for (int i = 0; i < N; i++)
                SetIndex(i, memsetval);
        }

        public void SetIndex(int index, T value)
        {
            Data[index] = value; // update local list

            int FInd = FirstFieldInd + index;
            byte[] valbytes = GetTBytes(value);
            Row.WriteAtField(FInd, valbytes);
            Row.StoreRow();
        }

    }
}
