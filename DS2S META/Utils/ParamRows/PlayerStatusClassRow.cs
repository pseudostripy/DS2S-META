using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils
{
    /// <summary>
    /// Data Class for storing Weapons
    /// </summary>
    public class PlayerStatusClassRow : Param.Row
    {
        public const int SL_OFFSET = 53; // all stats 6 = SL1

        // Behind-fields
        private short _soulLevel;
        private short _vigor;
        private short _endurance;
        private short _attunement;
        private short _vitality;
        private short _strength;
        private short _dexterity;
        private short _intelligence;
        private short _faith;
        private short _adaptability;
        //
        private int _headArmor;
        private int _bodyArmor; // two quick ones
        private int _armsArmor;
        private int _legsArmor;

        internal short SoulLevel
        {
            get => _soulLevel;
            set
            {
                _soulLevel = value;
                WriteAt(1, BitConverter.GetBytes(value));
            }
        }
        internal short Vigor
        {
            get => _vigor;
            set
            {
                _vigor = value;
                WriteAt(2, BitConverter.GetBytes(value));
            }
        }
        internal short Endurance
        {
            get => _endurance;
            set
            {
                _endurance = value;
                WriteAt(4, BitConverter.GetBytes(value));
            }
        }
        internal short Attunement
        {
            get => _attunement;
            set
            {
                _attunement = value;
                WriteAt(5, BitConverter.GetBytes(value));
            }
        }
        internal short Vitality
        {
            get => _vitality;
            set
            {
                _vitality = value;
                WriteAt(6, BitConverter.GetBytes(value));
            }
        }
        internal short Strength
        {
            get => _strength;
            set
            {
                _strength = value;
                WriteAt(7, BitConverter.GetBytes(value));
            }
        }
        internal short Dexterity
        {
            get => _dexterity;
            set
            {
                _dexterity = value;
                WriteAt(8, BitConverter.GetBytes(value));
            }
        }
        internal short Intelligence
        {
            get => _intelligence;
            set
            {
                _intelligence = value;
                WriteAt(9, BitConverter.GetBytes(value));
            }
        }
        internal short Faith
        {
            get => _faith;
            set
            {
                _faith = value;
                WriteAt(10, BitConverter.GetBytes(value));
            }
        }
        internal short Adaptability
        {
            get => _adaptability;
            set
            {
                _adaptability = value;
                WriteAt(11, BitConverter.GetBytes(value));
            }
        }
        //
        internal int HeadArmor
        {
            get => _headArmor;
            set
            {
                _headArmor = value;
                WriteAt(52, BitConverter.GetBytes(value));
            }
        }
        internal int BodyArmor 
        {
            get => _bodyArmor;
            set
            {
                _bodyArmor = value;
                WriteAt(53, BitConverter.GetBytes(value));
            }
        }
        internal int ArmsArmor
        {
            get => _armsArmor;
            set
            {
                _armsArmor = value;
                WriteAt(54, BitConverter.GetBytes(value));
            }
        }
        internal int LegsArmor
        {
            get => _legsArmor;
            set
            {
                _legsArmor = value;
                WriteAt(55, BitConverter.GetBytes(value));
            }
        }

        // Constructor:
        public PlayerStatusClassRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            HeadArmor = (int)ReadAt(52);
            BodyArmor = (int)ReadAt(53);
            ArmsArmor = (int)ReadAt(54);
            LegsArmor = (int)ReadAt(55);
        }

        public void Wipe()
        {
            // Clears all previously written ids to this class:
            
            // Items/ItemQuant/Spells
            for (int i = 0; i < 10; i++)
            {
                WriteAtItemArray(i, -1);
                WriteAtItemQuantArray(i, 0);
                WriteAtSpellArray(i, -1);
            }

            // Rings
            for (int i = 0; i < 4; i++)
                WriteAtRingArray(i, -1);

            // Weapons:
            int emptywep = 3400000; // not sure why its this value
            for (int i = 0; i < 3; i++)
            {
                WriteAtRHWepArray(i, emptywep);
                WriteAtLHWepArray(i, emptywep);
            }

            // Arrows/Bolts & their quantities:
            for (int i = 0; i < 2; i++)
            {
                WriteAtArrowArray(i, -1);
                WriteAtArrowAmountArray(i, 0);
                WriteAtBoltArray(i, -1);
                WriteAtBoltAmountArray(i, 0);
            }

            // Starting gear:
            HeadArmor = -1;
            BodyArmor = -1;
            ArmsArmor = -1;
            LegsArmor = -1;
        }
        public void SetSoulLevel()
        {
            var sumlevel = Vigor + Endurance + Attunement + Vitality + Strength
                                + Dexterity + Intelligence + Faith + Adaptability;
            SoulLevel = (short)((short)sumlevel - SL_OFFSET);
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
        
        internal int CountItems()
        {
            var indItem1 = GetFieldIndex("Item 1 ID");
            var items = ReadListAt(indItem1).Select(obj => BitConverter.ToInt32(obj)).ToList();

            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                if (items[i] != -1)
                    count++;
            }
            return count;
        }

        // giga-wrappers
        public void WriteRings(IEnumerable<int> ids)
        {
            for (int i = 0; i < ids.Count(); i++)
                WriteAtRingArray(i, ids.ElementAt(i));
        }
        public void WriteRHWeps(IEnumerable<int> ids, IEnumerable<int> reinfs)
        {
            if (ids.Count() != reinfs.Count()) throw new Exception("Argument size mismatch");
            for (int i = 0; i < ids.Count(); i++)
            {
                WriteAtRHWepArray(i, ids.ElementAt(i));
                WriteAtRHWepReinforceArray(i, reinfs.ElementAt(i));
            }
        }
        public void WriteLHWeps(IEnumerable<int> ids, IEnumerable<int> reinfs)
        {
            if (ids.Count() != reinfs.Count()) throw new Exception("Argument size mismatch");
            for (int i = 0; i < ids.Count(); i++)
            {
                WriteAtLHWepArray(i, ids.ElementAt(i));
                WriteAtLHWepReinforceArray(i, reinfs.ElementAt(i));
            }
        }
        public void WriteItems(IEnumerable<int> ids, IEnumerable<short> quants)
        {
            if (ids.Count() != quants.Count()) throw new Exception("Argument size mismatch");
            for (int i = 0; i < ids.Count(); i++)
            {
                WriteAtItemArray(i, ids.ElementAt(i));
                WriteAtItemQuantArray(i, quants.ElementAt(i));
            }
        }
        public void WriteArrows(IEnumerable<int> ids, IEnumerable<short> quants)
        {
            if (ids.Count() != quants.Count()) throw new Exception("Argument size mismatch");
            for (int i = 0; i < ids.Count(); i++)
            {
                WriteAtArrowArray(i, ids.ElementAt(i));
                WriteAtArrowAmountArray(i, quants.ElementAt(i));
            }
        }
        public void WriteBolts(IEnumerable<int> ids, IEnumerable<short> quants)
        {
            if (ids.Count() != quants.Count()) throw new Exception("Argument size mismatch");
            for (int i = 0; i < ids.Count(); i++)
            {
                WriteAtBoltArray(i, ids.ElementAt(i));
                WriteAtBoltAmountArray(i, quants.ElementAt(i));
            }
        }

        // wrappers
        private Param.Field GetField(int indexst)
        {
            // Trivial wrapper for convenience
            return Param.Fields[indexst];
        }
        public void WriteAtArrayWrapperInt(int indexst, int subindex, int value)
        {
            var F = GetField(indexst);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }
        public void WriteAtArrayWrapperShort(int indexst, int subindex, short value)
        {
            var F = GetField(indexst);
            int ind = F.FieldOffset + subindex * F.FieldLength;
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, RowBytes, ind, bytes.Length);
        }
        public void WriteAtItemArray(int subindex, int value)
        {
            WriteAtArrayWrapperInt(16, subindex, value);
        }
        public void WriteAtItemQuantArray(int subindex, short value)
        {
            WriteAtArrayWrapperShort(26, subindex, value);
        }
        public void WriteAtSpellArray(int subindex, int value)
        {
            WriteAtArrayWrapperInt(36, subindex, value);
        }
        public void WriteAtRHWepArray(int subindex, int value)
        {
            // subindex 0..2
            WriteAtArrayWrapperInt(46, subindex, value); 
        }
        public void WriteAtLHWepArray(int subindex, int value)
        {
            // subindex 0..2
            WriteAtArrayWrapperInt(49, subindex, value);
        }
        public void WriteAtRingArray(int subindex, int value)
        {
            // subindex 0..3
            WriteAtArrayWrapperInt(56, subindex, value);
        }
        public void WriteAtRHWepReinforceArray(int subindex, int value)
        {
            // subindex 0..3
            WriteAtArrayWrapperInt(60, subindex, value);
        }
        public void WriteAtLHWepReinforceArray(int subindex, int value)
        {
            // subindex 0..3
            WriteAtArrayWrapperInt(63, subindex, value);
        }
        public void WriteAtArrowArray(int subindex, int value)
        {
            // subindex 0..1
            WriteAtArrayWrapperInt(71, subindex, value);
        }
        public void WriteAtBoltArray(int subindex, int value)
        {
            // subindex 0..2
            WriteAtArrayWrapperInt(73, subindex, value);
        }
        public void WriteAtArrowAmountArray(int subindex, short value)
        {
            // subindex 0..2
            WriteAtArrayWrapperShort(75, subindex, value);
        }
        public void WriteAtBoltAmountArray(int subindex, short value)
        {
            // subindex 0..2
            WriteAtArrayWrapperShort(77, subindex, value);
        }

    }
}
