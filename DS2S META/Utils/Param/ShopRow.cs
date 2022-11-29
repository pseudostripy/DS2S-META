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
    /// This class provides easier access to the shop substructure fields
    /// of ShopLineupParam
    /// </summary>
    internal class ShopRow : Param.Row
    {
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

        internal ItemRow ItemParam => RandomizerManager.VanillaItemParams[ItemID];
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
        internal string? ParamDesc => Desc;

        internal int CopyShopFromParamID = 0;

        // Constructors:
        public ShopRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            // Initial field setting:
            ItemID = (int)ReadAt(0);
            EnableFlag = (int)ReadAt(2);
            DisableFlag = (int)ReadAt(3);
            MaterialID = (int)ReadAt(4);
            DuplicateItemID = (int)ReadAt(5);
            PriceRate = (float)ReadAt(7);
            Quantity = (int)ReadAt(8);
        }

        // Methods:
        internal ShopRow Clone()
        {
            return (ShopRow)MemberwiseClone();
        }
        internal void SetValues(DropInfo DI, ShopRow VanShop, float pricerate)
        {
            // Used to construct things from various information sources:
            ItemID          = DI.ItemID;
            Quantity        = DI.Quantity;
            //
            EnableFlag      = VanShop.EnableFlag;
            DisableFlag     = VanShop.DisableFlag;
            MaterialID      = VanShop.MaterialID;
            DuplicateItemID = VanShop.DuplicateItemID;
            //
            PriceRate = pricerate;
        }
        internal List<DropInfo> ConvertToDropInfo()
        {
            // Assume no infusion or reinforcement, to consider later.
            return new List<DropInfo>() { new DropInfo(ItemID, Quantity, 0, 0) };
        }
        internal void CopyValuesFrom(ShopRow tocopy)
        {
            // Apply the data of tocopy to this Row, but don't change the row pointer or ParamID fields
            CopyCoreValuesFrom(tocopy); // Item/Material/Price/Quantity
            
            EnableFlag = tocopy.EnableFlag;
            DisableFlag = tocopy.DisableFlag;
            DuplicateItemID = tocopy.DuplicateItemID;
        }
        internal void CopyCoreValuesFrom(ShopRow tocopy)
        {
            // Apply the data of tocopy to this Row, but don't change the row pointer or ParamID fields
            ItemID = tocopy.ItemID;
            MaterialID = tocopy.MaterialID;
            PriceRate = tocopy.PriceRate;
            Quantity = tocopy.Quantity;
        }
        public void ClearShop()
        {
            ItemID = 0;
            Quantity = 0;
            StoreRow();
        }
    }
}
