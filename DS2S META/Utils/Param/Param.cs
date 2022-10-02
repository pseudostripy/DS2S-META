using PropertyHook;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

using static SoulsFormats.PARAMDEF;
using DS2S_META.Utils;
using System.Security.Cryptography;
using System.Printing.IndexedProperties;
using System.Linq;

namespace DS2S_META
{
    public class Param : IComparable<Param>
    {
        public PHPointer Pointer { get; private set; }
        public int[] Offsets { get; private set; }
        public PARAMDEF ParamDef { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public int OffsetsTableLength { get; private set; }
        public int TotalTableLength { get; private set; }
        public byte[]? Bytes { get; private set; }
        public byte[]? NewBytes { get; set; } // after modifications
        public List<Row> Rows { get; private set; } = new();
        public List<Field> Fields { get; private set; } = new();
        private static Regex _paramEntryRx { get; } = new(@"^\s*(?<id>\S+)\s+(?<name>.*)$", RegexOptions.CultureInvariant);
        public Dictionary<int, string> NameDictionary { get; private set; } = new();
        public Dictionary<int, int> OffsetDict { get; private set; } = new();
        public int RowLength { get; private set; }

        private const string paramfol = "Resources/Paramdex_DS2S_09272022/";

        public Param(PHPointer pointer, int[] offsets, PARAMDEF Paramdef, string name)
        {
            Pointer = pointer;
            Offsets = offsets;
            ParamDef = Paramdef;
            Name = name;
            Type = Paramdef.ParamType;
            RowLength = ParamDef.GetRowSize();
            BuildNameDictionary();
            BuildOffsetDictionary();
            BuildCells();
        }
        private void BuildNameDictionary()
        {
            // Why don't they just use the same names :/
            string names_filename = Name.Replace("_", string.Empty);
            string[] result = Util.GetListResource(@$"{paramfol}Names/{names_filename}.txt");
            if (result.Length == 0)
                return;

            foreach (string line in result)
            {
                if (!Util.IsValidTxtResource(line)) //determine if line is a valid resource or not
                    continue;

                Match itemEntry = _paramEntryRx.Match(line);
                string name = itemEntry.Groups["name"].Value;//.Replace("\r", "");
                int id = Convert.ToInt32(itemEntry.Groups["id"].Value);
                if (NameDictionary.ContainsKey(id))
                    continue;

                NameDictionary.Add(id, name);
            };
        }
        private void BuildOffsetDictionary()
        {
            string paramType = Pointer.ReadString((int)DS2SOffsets.Param.ParamName, Encoding.UTF8, 0x20);
            if (paramType != Type)
                throw new InvalidOperationException($"Incorrect Param Pointer: {paramType} should be {Type}");

            TotalTableLength = Pointer.ReadInt32((int)DS2SOffsets.Param.TableLength);
            OffsetsTableLength = Pointer.ReadInt32((int)DS2SOffsets.Param.OffsetsOnlyTableLength);
            Bytes = Pointer.ReadBytes(0x0, (uint)TotalTableLength);

            
            int param = 0x40;
            int paramID = 0x0;
            int paramoffset = 0x8;
            int nextParam = 0x18;

            while (param < OffsetsTableLength)
            {
                int itemID = BitConverter.ToInt32(Bytes, param + paramID);
                int itemParamOffset = BitConverter.ToInt32(Bytes, param + paramoffset);
                string name = $"{itemID} - ";
                if (NameDictionary.ContainsKey(itemID))
                    name += $"{NameDictionary[itemID]}";

                if (!OffsetDict.ContainsKey(itemID))
                    OffsetDict.Add(itemID, itemParamOffset);

                Rows.Add(new(this, name, itemID, itemParamOffset));

                param += nextParam;
            }
        }
        public void StoreRowBytes(Row row)
        {
            if (Bytes == null)
                throw new Exception("Param Bytes are not set, cannot be modified");

            // Initial copy
            if (NewBytes == null)
                NewBytes = (byte[])Bytes.Clone();

            if (row.RowBytes.Length != RowLength)
                throw new ArrayTypeMismatchException("Row bytes size does not match expected length for param row size");

            Array.Copy(row.RowBytes, 0, NewBytes, row.DataOffset, RowLength);
        }

        public override string ToString()
        {
            return Name;
        }
        public int CompareTo(Param? otherParam)
        {
            if (otherParam == null)
                throw new ArgumentNullException(nameof(otherParam));

            return string.Compare(Name, otherParam.Name, StringComparison.Ordinal);
        }
        public void RestoreParam()
        {
            Pointer.WriteBytes(0, Bytes);
        }
        public void WriteModifiedParam()
        {
            Pointer.WriteBytes(0, NewBytes);
        }
        private void BuildCells()
        {
            int totalSize = 0;
            for (int i = 0; i < ParamDef.Fields.Count; i++)
            {
                PARAMDEF.Field field = ParamDef.Fields[i];
                DefType type = field.DisplayType;
                int size = ParamUtil.IsArrayType(type) ? ParamUtil.GetValueSize(type) * field.ArrayLength : ParamUtil.GetValueSize(type);
                if (ParamUtil.IsArrayType(type))
                    totalSize += ParamUtil.GetValueSize(type) * field.ArrayLength;
                else
                    totalSize += ParamUtil.GetValueSize(type);

                if (ParamUtil.IsBitType(type) && field.BitSize != -1)
                {
                    int bitOffset = field.BitSize;
                    DefType bitType = type == DefType.dummy8 ? DefType.u8 : type;
                    int bitLimit = ParamUtil.GetBitLimit(bitType);
                    Fields.Add(GetBitField(field, totalSize, size, bitOffset));


                    for (; i < ParamDef.Fields.Count - 1; i++)
                    {
                        PARAMDEF.Field nextField = ParamDef.Fields[i + 1];
                        DefType nextType = nextField.DisplayType;
                        if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || bitOffset + nextField.BitSize > bitLimit
                            || (nextType == DefType.dummy8 ? DefType.u8 : nextType) != bitType)
                            break;
                        bitOffset += nextField.BitSize;
                        Fields.Add(GetBitField(nextField, totalSize, size, bitOffset));
                    }
                    continue;
                }

                switch (field.DisplayType)
                {
                    case DefType.s8:
                    case DefType.s16:
                    case DefType.s32:
                        Fields.Add(new NumericField(field, totalSize - size, true));
                        break;
                    case DefType.u8:
                    case DefType.dummy8:
                    case DefType.u16:
                    case DefType.u32:
                        Fields.Add(new NumericField(field, totalSize - size, false));
                        break;
                    case DefType.f32:
                        Fields.Add(new SingleField(field, totalSize - size));
                        break;
                    case DefType.fixstr:
                        Fields.Add(new FixedStr(field, totalSize - size, Encoding.ASCII));
                        break;
                    case DefType.fixstrW:
                        Fields.Add(new FixedStr(field, totalSize - size, Encoding.Unicode));
                        break;
                    default:
                        throw new($"Unknown type: {field.DisplayType}");
                }
            }
        }
        private static BitField GetBitField(PARAMDEF.Field field, int totalSize, int size, int bitOffset)
        {

            switch (field.DisplayType)
            {
                case DefType.u8:
                case DefType.dummy8:
                    return field.BitSize > 1
                        ? new PartialByteField(field, totalSize - size, bitOffset - field.BitSize, field.BitSize)
                        : new BitField(field, totalSize - size, bitOffset - 1);
                case DefType.u16:
                    return field.BitSize > 1 
                        ? new PartialUShortField(field, totalSize - size, bitOffset - field.BitSize, field.BitSize) 
                        : new BitField(field, totalSize - size, bitOffset - 1);
                case DefType.u32:
                    return field.BitSize > 1
                        ? new PartialUIntField(field, totalSize - size, bitOffset - field.BitSize, field.BitSize)
                        : new BitField(field, totalSize - size, bitOffset - 1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Each "section" of a Param table (aka each ID) associates to a "row"
        /// </summary>
        public class Row
        {
            public Param Param { get; private set; }
            public string Name { get; private set; }
            public int ID { get; private set; }
            public int DataOffset { get; private set; }
            public byte[] RowBytes { get; set;} = Array.Empty<byte>();
            public object[] Data => ReadRow();
            public string Desc => Name.Substring(Name.LastIndexOf('-') + 2);

            public Row(Param param, string name, int id, int offset)
            {
                Param = param;
                Name = name;
                ID = id;
                DataOffset = offset;

                if (Param.Bytes == null)
                    return;
                RowBytes = Param.Bytes.Skip(DataOffset).Take(Param.RowLength).ToArray();
            }
            public override string ToString()
            {
                return Name;
            }

            public object[] ReadRow()
            {
                if (Param == null)
                    return Array.Empty<object>();
                
                return Param.Fields.Select(f => f.GetFieldValue(RowBytes)).ToArray();
            }
            public void WriteRow()
            {
                Param.Pointer.WriteBytes(DataOffset, RowBytes);
            }

        }


        /// <summary>
        /// These are defined in the ParamDef xml
        /// </summary>
        public abstract class Field
        {
            private PARAMDEF.Field _paramdefField { get; }
            public DefType Type => _paramdefField.DisplayType;
            public string InternalName => _paramdefField.InternalName;
            public string DisplayName => _paramdefField.DisplayName;
            public string Description => _paramdefField.Description;
            public int ArrayLength => _paramdefField.ArrayLength;
            public object Increment => _paramdefField.Increment;
            public int FieldOffset { get; }
            public int FieldLength { get; }

            public Field(PARAMDEF.Field field, int fieldOffset)
            {
                _paramdefField = field;
                FieldOffset = fieldOffset;
                FieldLength = GetFieldLength();
            }
            private int GetFieldLength()
            {
                // returns field length in bytes
                switch (Type)
                {
                    case DefType.s8:
                    case DefType.u8:
                    case DefType.s16:
                    case DefType.u16:
                    case DefType.s32:
                    case DefType.u32:
                    case DefType.b32:
                    case DefType.f32:
                    case DefType.angle32:
                        return 4;

                    case DefType.f64:
                        return 8;

                    // Given that there are 8 bytes available, these could possibly be offsets
                    case DefType.dummy8:
                    case DefType.fixstr:
                    case DefType.fixstrW:
                        throw new NotImplementedException($"TODO read arraylength subproperty! : {Type}");

                    default:
                        throw new NotImplementedException($"Unknown field type: {Type}");
                }
            }
            public override string ToString()
            {
                return InternalName;
            }

            // Override for different field returns
            public virtual object GetFieldValue(byte[] rowbytes)
            {
                // Strings overriden in subclass
                var outputarray = new byte[4];
                Array.Copy(rowbytes, FieldOffset, outputarray, 0, 4);

                switch (Type)
                {
                    case DefType.s8:
                    case DefType.u8:
                    case DefType.s16:
                    case DefType.u16:
                    case DefType.s32:
                    case DefType.u32:
                    case DefType.b32:
                        return BitConverter.ToInt32(outputarray);

                    case DefType.f32:
                    case DefType.angle32:
                        return BitConverter.ToSingle(outputarray);

                    case DefType.f64:
                        // 8 bytes...
                        var outputarray2 = new byte[8];
                        Array.Copy(rowbytes, FieldOffset, outputarray2, 0, 8);
                        return BitConverter.ToDouble(outputarray2);

                    // Given that there are 8 bytes available, these could possibly be offsets
                    case DefType.dummy8:
                    case DefType.fixstr:
                    case DefType.fixstrW:
                        //value = null;
                        //br.AssertInt64(0);
                        //break;
                        throw new NotImplementedException($"Not Checked! : {Type}");

                    default:
                        throw new NotImplementedException($"Missing variable read for type: {Type}");
                }
            }
        }

        public class FixedStr : Field
        {
            public Encoding Encoding;
            public FixedStr(PARAMDEF.Field field, int fieldOffset, Encoding encoding) : base(field, fieldOffset)
            {
                Encoding = encoding;
            }
            public override string GetFieldValue(byte[] bytes)
            {
                return Encoding.GetString(bytes, FieldOffset, ArrayLength);
            }
        }
        public class NumericField : Field
        {
            public bool IsSigned;
            public NumericField(PARAMDEF.Field field, int fieldOffset, bool isSigned) : base(field, fieldOffset)
            {
                IsSigned = isSigned;
            }
        }
        public class BitField : Field
        {
            public int BitPosition;
            public BitField(PARAMDEF.Field field, int fieldOffset, int bitPosition) : base(field, fieldOffset)
            {
                BitPosition = bitPosition;
            }
        }
        public class PartialByteField : BitField
        {
            public int Width;
            public PartialByteField(PARAMDEF.Field field, int fieldOffset, int bitPosition, int width) : base(field, fieldOffset, bitPosition)
            {
                Width = width;
            }
        }
        public class PartialUShortField : BitField
        {
            public int Width;
            public PartialUShortField(PARAMDEF.Field field, int fieldOffset, int bitPosition, int width) : base(field, fieldOffset, bitPosition)
            {
                Width = width;
            }
        }
        public class PartialUIntField : BitField
        {
            public int Width;
            public PartialUIntField(PARAMDEF.Field field, int fieldOffset, int bitPosition, int width) : base(field, fieldOffset, bitPosition)
            {
                Width = width;
            }
        }
        public class SingleField : Field
        {
            public SingleField(PARAMDEF.Field field, int fieldOffset) : base(field, fieldOffset)
            {
            }
        }
    }
}
