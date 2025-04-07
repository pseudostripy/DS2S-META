using DS2S_META.Dialog;
using DS2S_META.Utils.DS2Hook;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.Offsets.HookGroupObjects
{
    public enum BAGTYPE : int
    {
        NORMALLOOT = 0,
        PLAYERDROPPED = 3, // ?????
    }

    public class MapManager
    {
        private readonly DS2SHook Hook;
        public PHPointer? PHMapManager;
        public PHPointer? PHMapItemPackManager;        

        public MapManager(DS2SHook hook, Dictionary<string, PHPointer> PHPDict)
        {
            Hook = hook;

            PHMapManager = HGO.ValOrNull(PHPDict, "MapManager");
            PHMapItemPackManager = HGO.ValOrNull(PHPDict, "MapItemPackManager");
        }

        public List<MapItemEntityStruct> GetLootItemPack()
        {
            return GetItemPackSet(BAGTYPE.NORMALLOOT);
        }
        public List<MapItemEntityStruct> GetItemPackSet(BAGTYPE bagType)
        {
            // This function is following getItemBag at 0x1401e04e0
            if (Hook == null)
                throw new MetaLogicException("Hook is not initialised yet, cannot start using it for maps");

            // get ptr to top of array
            int listOffset = 0x8;
            PHPointer PHItemPackList = Hook.CreateChildPointer(PHMapItemPackManager, listOffset);

            // get ptr at array[i]
            int ptrOffset = (int)bagType * 8;
            PHPointer PHItemPack = Hook.CreateChildPointer(PHItemPackList, ptrOffset);

            // ensure we have at least one entry to follow the linked list
            int offsetMapItemsLinkedList = 0x18;
            PHPointer firstMapItemElement = Hook.CreateChildPointer(PHItemPack, offsetMapItemsLinkedList);

            // initialize loop / preallocate
            PHPointer currMapItemElement = getNextMapItemElement(firstMapItemElement);
            List<MapItemEntityStruct> retList = [];
            
            // follow linked list
            while (currMapItemElement.Resolve() != firstMapItemElement.Resolve())
            {
                // read MapItemEntityStruct object and add:
                int offsetEntityStruct = 0x10;
                PHPointer PHMapItemEntityStruct = Hook.CreateChildPointer(currMapItemElement, offsetEntityStruct);
                var mapEntityStruct = new MapItemEntityStruct(Hook,PHMapItemEntityStruct);
                
                // record data for easy C# querying
                retList.Add(mapEntityStruct);

                // traverse linked list
                currMapItemElement = getNextMapItemElement(currMapItemElement);
            }
            return retList;
        }
        private PHPointer getNextMapItemElement(PHPointer currMapItemLinkedListElement)
        {
            // used to traverse this kind of linked list
            int offsetNext = 0x0;

            // need to make as basePtr to avoid very large child linked list chain ptr depth
            return Hook.CreateBasePointer(currMapItemLinkedListElement.ReadIntPtr(offsetNext)); 
        }
        
    }
}
