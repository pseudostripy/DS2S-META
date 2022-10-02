using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DS2S_META.Utils;




namespace DS2S_META.Randomizer
{
    /// <summary>
    /// This class is not strictly required, but it provides useful
    /// shorthand checks for many of the variables used in the randomizer.
    /// </summary>
    internal class ShopInfo
    {
        // Fields:
        internal Param.Row ParamRow; // Raw DS2 memory
        internal int ID => ParamRow.ID; // shorthand

        //internal string? ParamDesc => Data.Name;
        internal string ParamDesc { get; set; }
        
        // Behind-fields
        private int _itemid;
        private int _enableflag;
        private int _disableflag;
        private int _materialid;
        private int _duplicateid;
        private float _pricerate;
        private int _quantity;

        // Properties
        internal int ItemID 
        { 
            get => _itemid;
            set
            {
                _itemid = value;
                WriteAt(0, BitConverter.GetBytes(value));
            }
        }
        internal int EnableFlag 
        { 
            get => _enableflag;
            set
            {
                _enableflag = value;
                WriteAt(2, BitConverter.GetBytes(value));
            }
        }
        internal int DisableFlag 
        { 
            get => _disableflag;
            set
            {
                _disableflag = value;
                WriteAt(3, BitConverter.GetBytes(value));
            }
        }
        internal int MaterialID 
        { 
            get => _materialid;
            set
            {
                _materialid = value;
                WriteAt(4, BitConverter.GetBytes(value));
            }
        }
        internal int DuplicateItemID 
        { 
            get => _duplicateid;
            set
            {
                _duplicateid = value;
                WriteAt(5, BitConverter.GetBytes(value));
            }
        }
        internal float PriceRate 
        { 
            get => _pricerate;
            set
            {
                _pricerate = value;
                WriteAt(7, BitConverter.GetBytes(value));
            }
        }
        internal int Quantity
        { 
            get => _quantity;
            set
            {
                _quantity = value;
                WriteAt(8, BitConverter.GetBytes(value));
            }
        }

        // Do we need this?
        internal int NewBasePrice { get; set; }
        
        internal ItemParam ItemParam => RandomizerManager.VanillaItemParams[ItemID];
        internal int VanillaBasePrice
        {
            get
            {
                if (!RandomizerManager.TryGetItem(ItemID, out var item))
                    return -1;
                if (item == null)
                    return -1;
                return item.BaseBuyPrice;
            }
        }

        // Constructors:
        internal ShopInfo() { }
        internal ShopInfo(Param.Row shoprow)
        {
            // Unpack data:
            ParamRow = shoprow;
            ParamDesc = ParamRow.Desc;

            // Initial field setting:
            ItemID = (int)ReadAt(0);
            EnableFlag = (int)ReadAt(2);
            DisableFlag = (int)ReadAt(3);
            MaterialID = (int)ReadAt(4);
            DuplicateItemID = (int)ReadAt(5);
            PriceRate = (float)ReadAt(7);
            Quantity = (int)ReadAt(8);
        }

        internal ShopInfo(DropInfo DI, ShopInfo VanShop, float pricerate, int newbaseprice)
        {
            // Collision might be ok?
            ParamRow = VanShop.ParamRow;
            
            // Used to construct things from various information sources:
            ItemID          = DI.ItemID;
            Quantity        = DI.Quantity;
            //
            EnableFlag      = VanShop.EnableFlag;
            DisableFlag     = VanShop.DisableFlag;
            MaterialID      = VanShop.MaterialID;
            DuplicateItemID = VanShop.DuplicateItemID;
            ParamDesc       = VanShop.ParamDesc;
            //
            PriceRate = pricerate;
            //
            NewBasePrice = newbaseprice;
        }
        internal ShopInfo Clone()
        {
            return (ShopInfo)MemberwiseClone();
        }

        // Methods:
        internal List<DropInfo> ConvertToDropInfo()
        {
            // Assume no infusion or reinforcement, to consider later.
            return new List<DropInfo>() { new DropInfo(ItemID, Quantity, 0, 0) };
        }
        //internal void SetBytesOutput()
        //{
        //    // Prepare for write

        //    var fourbytes = new byte[4];

        //    var itemid = BitConverter.GetBytes(ItemID);
        //    var enable = BitConverter.GetBytes(EnableFlag);
        //    var disable = BitConverter.GetBytes(DisableFlag);
        //    var material = BitConverter.GetBytes(MaterialID);
        //    var dup = BitConverter.GetBytes(DuplicateItemID);
        //    var rate = BitConverter.GetBytes(PriceRate);
        //    var quant = BitConverter.GetBytes(Quantity);

        //    var test = Concat(itemid, fourbytes, enable, disable, material,
        //                       dup, fourbytes, rate, quant);
        //    ParamRow.RowBytes = test;
        //}
        public void WriteAt(int fieldindex, byte[] valuebytes)
        {
            // Note: this function isn't generalised properly yet
            int fieldoffset = ParamRow.Param.Fields[fieldindex].FieldOffset;
            Array.Copy(valuebytes, 0, ParamRow.RowBytes, fieldoffset, valuebytes.Length);
        }
        //public byte[] ReadAt(int fieldindex)
        //{
        //    Param.Field F = ParamRow.Param.Fields[fieldindex];
        //    byte[] retbytes = new byte[F.FieldLength];
        //    Array.Copy(ParamRow.RowBytes, F.FieldOffset, retbytes, 0, F.FieldLength);
        //    return retbytes;
        //}
        public object ReadAt(int fieldindex) => ParamRow.Data[fieldindex];
        public void StoreRow()
        {
            // Convenience wrapper
            ParamRow.Param.StoreRowBytes(ParamRow);
        }
        //public static byte[] Concat(params byte[][] arrays)
        //{
        //    return arrays.SelectMany(x => x).ToArray();
        //}
    }
}
