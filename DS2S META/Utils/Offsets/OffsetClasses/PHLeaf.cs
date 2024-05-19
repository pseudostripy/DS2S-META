using PropertyHook;
using System;
using System.Collections;
using System.Text;

namespace DS2S_META.Utils.Offsets.OffsetClasses
{
    /// <summary>
    /// A dynamic pointer starting from an AOB, fixed address, or other pointer. Provides functions for reading and writing memory.
    /// </summary>
    public class PHLeaf
    {
        private readonly PHPointer Parent;
        public int ReadOffset { get; set; }

        public PHLeaf(PHPointer parent, int offset)
        {
            Parent = parent;
            ReadOffset = offset;
        }

        // Wrapper interfaces
        public byte[] ReadBytes(uint length) => Parent.ReadBytes(ReadOffset, length);
        public IntPtr ReadIntPtr() => Parent.ReadIntPtr(ReadOffset);
        public bool ReadFlag32(uint mask) => Parent.ReadFlag32(ReadOffset, mask);
        public sbyte ReadSByte() => Parent.ReadSByte(ReadOffset);
        public byte ReadByte() => Parent.ReadByte(ReadOffset);
        public bool ReadBoolean() => Parent.ReadBoolean(ReadOffset);
        public short ReadInt16() => Parent.ReadInt16(ReadOffset);
        public ushort ReadUInt16() => Parent.ReadUInt16(ReadOffset);
        public int ReadInt32() => Parent.ReadInt32(ReadOffset);
        public uint ReadUInt32() => Parent.ReadUInt32(ReadOffset);
        public long ReadInt64() => Parent.ReadInt64(ReadOffset);
        public ulong ReadUInt64() => Parent.ReadUInt64(ReadOffset);
        public float ReadSingle() => Parent.ReadSingle(ReadOffset);
        public double ReadDouble() => Parent.ReadDouble(ReadOffset);
        public string ReadString(Encoding encoding, uint byteCount, bool trim = true) => Parent.ReadString(ReadOffset, encoding, byteCount, trim);
        //
        public bool WriteBytes(byte[] bytes) => Parent.WriteBytes(ReadOffset, bytes);
        public bool WriteFlag32(uint mask, bool state) => Parent.WriteFlag32(ReadOffset, mask, state);
        public bool WriteSByte(sbyte value) => Parent.WriteSByte(ReadOffset, value);
        public bool WriteByte(byte value) => Parent.WriteByte(ReadOffset, value);
        public bool WriteBoolean(bool value) => Parent.WriteBoolean(ReadOffset, value);
        public bool WriteInt16(short value) => Parent.WriteInt16(ReadOffset, value);
        public bool WriteUInt16(ushort value) => Parent.WriteUInt16(ReadOffset, value);
        public bool WriteInt32(int value) => Parent.WriteInt32(ReadOffset, value);
        public bool WriteUInt32(uint value) => Parent.WriteUInt32(ReadOffset, value);
        public bool WriteInt64(long value) => Parent.WriteInt64(ReadOffset, value);
        public bool WriteUInt64(ulong value) => Parent.WriteUInt64(ReadOffset, value);
        public bool WriteSingle(float value) => Parent.WriteSingle(ReadOffset, value);
        public bool WriteDouble(double value) => Parent.WriteDouble(ReadOffset, value);
        public bool WriteString(Encoding encoding, uint byteCount, string value) => Parent.WriteString(ReadOffset, encoding, byteCount, value);
    }
}
