using PropertyHook;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using static SoulsFormats.PARAMDEF;
using DS2S_META.Utils;
using System.Linq;
using System.Reflection;
using System.IO;

namespace DS2S_META
{
    public enum RESOURCETYPE { MEMORY, FILE }
    public class ParamInfo
    {
        public int Param { get; init; }
        public int ParamID { get; init; }
        public int Paramoffset { get; init; }
        public int NextParam { get; init; }

        // Previously Param Enum:
        public int TotalParamLength { get; init; }
        public int ParamName { get; init; }

        // possibly this should be 0x30 but I dont think it matters fortunately
        public int OffsetsOnlyTableLength { get; init; }
    }
    public class Param : IComparable<Param>
    {
        public string? FilePath { get; private set; }
        public PHPointer? Pointer { get; private set; }
        public int[]? Offsets { get; private set; }
        public PARAMDEF ParamDef { get; private set; }
        public string Name { get; private set; } // Unique Name
        public string Type { get; private set; } // ParamDef Name
        public string StringsFileName { get; set; } // Param Strings file
        public int? OffsetsTableLength { get; private set; }
        public int? TotalTableLength { get; private set; }
        public byte[]? Bytes { get; private set; }
        public byte[]? NewBytes { get; set; } // after modifications
        public List<Row> Rows { get; private set; } = new();
        public List<Field> Fields { get; private set; } = new();
        private static Regex _paramEntryRx { get; } = new(@"^\s*(?<id>\S+)\s+(?<name>.*)$", RegexOptions.CultureInvariant);
        public Dictionary<int, string> NameDictionary { get; private set; } = new();
        public Dictionary<int, int> OffsetDict { get; private set; } = new();
        public int RowLength { get; private set; }
        public readonly RESOURCETYPE ResourceType;
        private readonly bool Is64;
        private ParamInfo ParamInfo => Is64 ? PInfo64 : PInfo32;


        private readonly ParamInfo PInfo32 = new()
        {
            Param = 0x40,
            ParamID = 0x0,
            Paramoffset = 0x4,
            NextParam = 0xC,

            // Previously Param Enum:
            TotalParamLength = 0x0,
            ParamName = 0xC,
            OffsetsOnlyTableLength = 0x30,
        };
        private readonly ParamInfo PInfo64 = new()
        {
            Param = 0x40,
            ParamID = 0x0,
            Paramoffset = 0x8,
            NextParam = 0x18,

            // Previously Param Enum:
            TotalParamLength = 0x0,
            ParamName = 0xC,
            OffsetsOnlyTableLength = 0x30,
        };
        

        private const string paramfol = "Resources/Paramdex_DS2S_09272022/";

        // Constructors:
        public Param(PHPointer pointer, int[] offsets, PARAMDEF Paramdef, string name, bool isSotfs)
        {
            Is64 = isSotfs;
            ResourceType = RESOURCETYPE.MEMORY;
            Pointer = pointer;
            Offsets = offsets;
            ParamDef = Paramdef;
            Name = name;
            Type = Paramdef.ParamType;
            RowLength = ParamDef.GetRowSize();
        }
        public Param(string filepath, PARAMDEF Paramdef, string name, bool isSotfs)
        {
            Is64 = isSotfs;
            ResourceType = RESOURCETYPE.FILE;
            FilePath = filepath;
            ParamDef = Paramdef;
            Name = name;
            Type = Paramdef.ParamType;
            RowLength = ParamDef.GetRowSize();
        }

        public void initialise<T>() where T : Row
        {
            ReadParamBytes(); // Get "Bytes"
            BuildNameDictionary();
            BuildCells();
            BuildOffsetDictionary<T>();
        }
        private void BuildNameDictionary()
        {
            // Why don't they just use the same names :/
            string[] result = Util.GetListResource(@$"{paramfol}Names/{StringsFileName}.txt");
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
        private void ReadParamBytes()
        {
            if (ResourceType == RESOURCETYPE.MEMORY)
                ReadParamBytesPtr();
            else
                ReadParamBytesFile();
        }
        private void ReadParamBytesPtr()
        {
            string? paramType = Pointer?.ReadString(ParamInfo.ParamName, Encoding.UTF8, 0x20);
            if (paramType != Type)
                throw new InvalidOperationException($"Incorrect Param Pointer: {paramType} should be {Type}");

            OffsetsTableLength = Pointer?.ReadInt32(ParamInfo.OffsetsOnlyTableLength);

            //var offsetBytes = Pointer.ReadBytes(0x0, (uint)OffsetsTableLength);
            var nparams = (OffsetsTableLength - ParamInfo.Param) / ParamInfo.NextParam;

            if ((OffsetsTableLength - ParamInfo.Param) % ParamInfo.NextParam != 0)
                throw new Exception("Potential mismatch in param total bytes");
            TotalTableLength = OffsetsTableLength + nparams * RowLength;
            if (TotalTableLength == null)
                throw new Exception("Error reading table byte size");
            Bytes = Pointer?.ReadBytes(0x0, (uint)TotalTableLength);
        }
        private void ReadParamBytesFile()
        {
            if (FilePath == null) throw new Exception("Null file path");
            var filebytes = File.ReadAllBytes(FilePath);
            if (filebytes == null) throw new Exception("Error returning file bytes");

            string paramType = Encoding.UTF8.GetString(filebytes, ParamInfo.ParamName, 0x20).TrimEnd('\0');

            //string paramType = Pointer.ReadString((int)DS2SOffsets.Param.ParamName, Encoding.UTF8, 0x20);
            if (paramType != Type)
                throw new InvalidOperationException($"Incorrect Param Pointer: {paramType} should be {Type}");

            OffsetsTableLength = BitConverter.ToInt32(filebytes, ParamInfo.OffsetsOnlyTableLength);
            //OffsetsTableLength = Pointer.ReadInt32((int)DS2SOffsets.Param.OffsetsOnlyTableLength);

            //var offsetBytes = Pointer.ReadBytes(0x0, (uint)OffsetsTableLength);
            var nparams = (OffsetsTableLength - ParamInfo.Param) / ParamInfo.NextParam;

            if ((OffsetsTableLength - ParamInfo.Param) % ParamInfo.NextParam != 0)
                throw new Exception("Potential mismatch in param total bytes");
            TotalTableLength = OffsetsTableLength + nparams * RowLength;

            Bytes = filebytes.Take((int)TotalTableLength).ToArray();
        }
        private void BuildOffsetDictionary<T>() where T : Row
        {
            // Setup constructor for new row data
            Type[] argtypes = new Type[] { typeof(Param), typeof(string), typeof(int), typeof(int) };
            ConstructorInfo? ctor = typeof(T).GetConstructor(argtypes);
            if (ctor == null)
                throw new NullReferenceException("Cannot find appropriate row constructor");

            int currparam = ParamInfo.Param; // 0x40
            while (currparam < OffsetsTableLength)
            {
                int itemID = BitConverter.ToInt32(Bytes, currparam + ParamInfo.ParamID);
                int itemParamOffset = BitConverter.ToInt32(Bytes, currparam + ParamInfo.Paramoffset);
                string name = $"{itemID} - ";
                if (NameDictionary.ContainsKey(itemID))
                    name += $"{NameDictionary[itemID]}";

                if (!OffsetDict.ContainsKey(itemID))
                    OffsetDict.Add(itemID, itemParamOffset);

                // Create the new object of type Row (or class "T" inheriting from Row)
                T row = (T)ctor.Invoke(new object[] { this, name, itemID, itemParamOffset });
                Rows.Add(row);
                currparam += ParamInfo.NextParam;
            }
        }
        public void StoreRowBytes(Row row)
        {
            if (Bytes == null)
                throw new Exception("Param Bytes are not set, cannot be modified");

            // New copy (first call only)
            NewBytes ??= (byte[])Bytes.Clone();

            if (row.RowBytes.Length != RowLength)
                throw new ArrayTypeMismatchException("Row bytes size does not match expected length for param row size");

            Array.Copy(row.RowBytes, 0, NewBytes, row.ParamRowOffset, RowLength);
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
            Pointer?.WriteBytes(0, Bytes);
        }
        public void WriteModifiedParam()
        {
            // On the offchance no data has been stored at all, just replace as is:
            if (Bytes == null) throw new Exception("Param Bytes are not set, cannot be modified");
            NewBytes ??= (byte[])Bytes.Clone();

            // Memory write
            Pointer?.WriteBytes(0, NewBytes);
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
            public int ParamRowOffset { get; private set; }
            public byte[] RowBytes { get; set;} = Array.Empty<byte>();
            private static readonly Regex re = new(@"\w+ -?(?<desc>.*)");
            public object[] Data => ReadRow();
            public string Desc => GetName();

            public Row(Param param, string name, int id, int offset)
            {
                Param = param;
                Name = name;
                ID = id;
                ParamRowOffset = offset;

                if (Param.Bytes == null)
                    return;
                RowBytes = Param.Bytes.Skip(ParamRowOffset).Take(Param.RowLength).ToArray();
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
                if (Param.Pointer == null) throw new Exception("Missing table pointer");
                Param.Pointer.WriteBytes(ParamRowOffset, RowBytes);
            }
            public string GetName()
            {
                var match = re.Match(Name);
                if (match == null)
                    throw new NullReferenceException("Catch this if they ever don't abide by this scheme");
                return match.Groups["desc"].Value.Trim();
            }

            // Utility read/write
            // byte offsets:
            public byte[] ReadAt(int offset, uint len) {
                var bytes = new byte[len];
                Array.Copy(RowBytes, offset, bytes, 0, len);
                return bytes;
            }
            public byte ReadByteAt(object offset) => ReadAt((int)offset, 1).First();
            public short ReadShortAt(object offset) => BitConverter.ToInt16(ReadAt((int)offset, 2), 0);
            public int ReadIntAt(object offset) => BitConverter.ToInt32(ReadAt((int)offset, 4),0);
            public float ReadFloatAt(object offset) => BitConverter.ToSingle(ReadAt((int)offset, 4),0);
            //
            public void WriteAt(int offset, byte[] valuebytes) => Array.Copy(valuebytes, 0, RowBytes, offset, valuebytes.Length);
            public void WriteByteAt(object offset, byte val) => WriteAt((int)offset, val.AsByteArray());
            public void WriteShortAt(object offset, short val) => WriteAt((int)offset, BitConverter.GetBytes(val));
            public void WriteIntAt(object offset, int val) => WriteAt((int)offset, BitConverter.GetBytes(val));
            public void WriteFloatAt(object offset, float val) => WriteAt((int)offset, BitConverter.GetBytes(val));
            
            // defs Field index:
            public object ReadAtFieldNum(object fieldindex) => Data[(int)fieldindex]; // allow enum indices
            public void WriteByteAtField(object fieldind_orenum, object value) => WriteAtField(fieldind_orenum, ((byte)value).AsByteArray());
            public void WriteAtField(object fieldind_orenum, byte[] valuebytes)
            {
                int fieldindex = (int)fieldind_orenum; // allow enum for easier code
                // Note: this function isn't generalised properly yet
                int fieldoffset = Param.Fields[fieldindex].FieldOffset;
                Array.Copy(valuebytes, 0, RowBytes, fieldoffset, valuebytes.Length);
            }

            public void StoreRow()
            {
                // Convenience wrapper
                Param.StoreRowBytes(this);
            }
            public int GetFieldIndex(string fieldname)
            {
                for (int i = 0; i < Param.ParamDef.Fields.Count; i++)
                {
                    if (Param.ParamDef.Fields[i].DisplayName == fieldname)
                        return i;
                }
                throw new Exception($"Cannot find field with name: {fieldname} in ParamType {Param.Type}");
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
                        return 1;

                    case DefType.s16:
                    case DefType.u16:
                        return 2;

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

                var outputarray = new byte[FieldLength];
                Array.Copy(rowbytes, FieldOffset, outputarray, 0, FieldLength);
                switch (Type)
                {
                    case DefType.s8:
                    case DefType.u8:
                        return (byte)outputarray[0];
                        
                    case DefType.s16:
                    case DefType.u16:
                        return BitConverter.ToInt16(outputarray);

                    case DefType.s32:
                    case DefType.u32:
                    case DefType.b32:
                        return BitConverter.ToInt32(outputarray);

                    case DefType.f32:
                    case DefType.angle32:
                        return BitConverter.ToSingle(outputarray);

                    case DefType.f64:
                        // 8 bytes...
                        return BitConverter.ToDouble(outputarray);

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
