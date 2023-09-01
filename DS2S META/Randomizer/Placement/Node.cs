using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer.Placement
{
    /// <summary>
    /// A group of Rdzs that exactly share MapArea & KeySet
    /// </summary>
    internal class Node
    {
        internal NodeKey NodeKey;
        internal List<Randomization> RdzList;

        // MapAreasMinimally visited nodes for unlock
        internal List<int> SteinerNodes = new();        // nodes stored by integer ID
        internal List<MapArea> SteinerNodesMA = new();  // as above but human readable

        // Properties
        internal bool IsUnlocked => SteinerNodes.Count != 0;
        internal bool IsLocked => !IsUnlocked;

        // Methods:
        internal void AddStein(MapArea area)
        {
            SteinerNodesMA.Add(area);
            if (area == MapArea.Undefined || area == MapArea.Quantum)
                return; // no dist on these!

            SteinerNodes.Add(Steiner.Map2Id[area]);
        }
        internal void AddStein(int ID)
        {
            SteinerNodes.Add(ID);
            SteinerNodesMA.Add(Steiner.Id2Map[ID]);
        }

        // Constructor:
        internal Node(IGrouping<NodeKey, Randomization> grouping)
        {
            NodeKey = grouping.Key;
            RdzList = grouping.ToList();

            // Unlock places that require zero keys
            if (NodeKey.IsKeyless)
            {
                AddStein(MapArea.ThingsBetwixt);

                if (NodeKey.Area == MapArea.ThingsBetwixt)
                    return; // done

                // Start in betwixt, get to item area (by any means):
                AddStein(NodeKey.Area);
            }
        }
    }
}
