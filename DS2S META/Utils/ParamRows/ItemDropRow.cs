using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.ParamRows
{
    public class ItemDropRow : ItemLotBaseRow
    {
        // Constructors:
        public ItemDropRow(Param param, string name, int id, int offset) : base(param, name, id, offset)
        {
            IsDropTable = true;

            // Remove fields so that only nonzero chance items remain (and update lists)
            RemoveZeroPercentChance();
        }

        private void RemoveZeroPercentChance()
        {
            int clegit = 0; // counter of "legit"

            // copy info:
            var cpyIt = new List<int>();
            var cpyQ = new List<byte>();
            var cpyR = new List<byte>();
            var cpyI = new List<byte>();
            var cpyC = new List<float>();

            for (int i = 0; i < 10; i++)
            {
                if (Quantities[i] != 0 && Chances[i] != 0)
                {
                    cpyIt.Add(Items[i]);
                    cpyQ.Add(Quantities[i]);
                    cpyR.Add(Reinforcements[i]);
                    cpyI.Add(Infusions[i]);
                    cpyC.Add(Chances[i]);
                    clegit++;
                }
            }

            // Now overwrite the original bytes:
            for (int i = 0; i < cpyIt.Count; i++)
            {
                // Add to backend too (this is like an element-wise array property setter method)
                int id = i;
                StoreItem(id, cpyIt[i]);
                StoreQuantity(id, cpyQ[i]);
                StoreReinforce(id, cpyR[i]);
                StoreInfusion(id, cpyI[i]);
                StoreChance(id, cpyC[i]);
            }

            // Clear out the rest:
            for (int i = cpyIt.Count; i < 10; i++)
            {
                int id = i;
                StoreItem(id, 0);
                StoreQuantity(id, 0);
                StoreReinforce(id, 0);
                StoreInfusion(id, 0);
                StoreChance(id, 0);
            }
            UpdateLists();
        }
        internal override int NumDrops
        {
            get
            {
                int c = 0;
                for (int i = 0; i < 10; i++)
                {
                    if (Quantities[i] == 0 || Chances[i] == 0)
                        break;
                    c++;
                }
                return c;
            }
        }
        internal bool IsGuaranteedDrops()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Quantities[i] > 0 && Chances[i] < 99.9)
                    return false;
            }
            return true;
        }
        
        internal override ItemDropRow CloneBlank()
        {
            // Performs a deep clone on the Lot object
            var chancesnew = new List<float>(_chances);
            var ilclone = new ItemDropRow(Param, Name, ID, DataOffset)
            {
                _items = new List<int>(new int[10]),
                _quantities = new List<byte>(new byte[10]),
                _reinforcements = new List<byte>(new byte[10]),
                _infusions = new List<byte>(new byte[10]),
                _chances = chancesnew // clone chances from orig [todo chances aren't randomized yet]
            };

            return ilclone;
        }
    }
}
