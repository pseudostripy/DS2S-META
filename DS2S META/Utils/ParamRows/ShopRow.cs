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
    public class ShopRow : Param.Row
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
        public int ItemID 
        { 
            get => _itemid;
            set
            {
                _itemid = value;
                WriteAtField(0, BitConverter.GetBytes(value));
            }
        }
        public int EnableFlag 
        { 
            get => _enableflag;
            set
            {
                _enableflag = value;
                WriteAtField(2, BitConverter.GetBytes(value));
            }
        }
        public int DisableFlag 
        { 
            get => _disableflag;
            set
            {
                _disableflag = value;
                WriteAtField(3, BitConverter.GetBytes(value));
            }
        }
        public int MaterialID 
        { 
            get => _materialid;
            set
            {
                _materialid = value;
                WriteAtField(4, BitConverter.GetBytes(value));
            }
        }
        public int DuplicateItemID 
        { 
            get => _duplicateid;
            set
            {
                _duplicateid = value;
                WriteAtField(5, BitConverter.GetBytes(value));
            }
        }
        public float PriceRate 
        { 
            get => _pricerate;
            set
            {
                _pricerate = value;
                WriteAtField(7, BitConverter.GetBytes(value));
            }
        }
        public int Quantity
        { 
            get => _quantity;
            set
            {
                _quantity = value;
                WriteAtField(8, BitConverter.GetBytes(value));
            }
        }

        public string? ParamDesc => Desc;
        public string MetaDescription => GetMetaDescription();

        public string GetMetaDescription()
        {
            CasualItemSet.ShopData.TryGetValue(ID, out var rinfo);
            if (rinfo == null)
                return "Description not found";
            return rinfo.Description ?? string.Empty;
        }

        public override string ToString() => MetaDescription;

        public int CopyShopFromParamID = 0;

        // Constructors:
        public ShopRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            // Initial field setting:
            ItemID = (int)ReadAtFieldNum(0);
            EnableFlag = (int)ReadAtFieldNum(2);
            DisableFlag = (int)ReadAtFieldNum(3);
            MaterialID = (int)ReadAtFieldNum(4);
            DuplicateItemID = (int)ReadAtFieldNum(5);
            PriceRate = (float)ReadAtFieldNum(7);
            Quantity = (int)ReadAtFieldNum(8);
        }

        // Methods:
        public ShopRow Clone()
        {
            return (ShopRow)MemberwiseClone();
        }
        public ShopRow CloneBlank()
        {
            var cl = Clone();
            cl.ItemID = 0;
            cl.Quantity = 0;
            return cl;
        }
        public void SetValues(DropInfo DI, ShopRow VanShop, float pricerate)
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
        public List<DropInfo> ConvertToDropInfo()
        {
            // Assume no infusion or reinforcement, to consider later.
            return new List<DropInfo>() { new DropInfo(ItemID, Quantity, 0, 0) };
        }
        public void CopyValuesFrom(ShopRow tocopy)
        {
            // Apply the data of tocopy to this Row, but don't change the row pointer or ParamID fields
            CopyCoreValuesFrom(tocopy); // Item/Material/Price/Quantity
            
            EnableFlag = tocopy.EnableFlag;
            DisableFlag = tocopy.DisableFlag;
            DuplicateItemID = tocopy.DuplicateItemID;
        }
        public void CopyCoreValuesFrom(ShopRow tocopy)
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
