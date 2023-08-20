using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Randomizer
{
    /// <summary>
    /// Place to put code that is used in development, or for future
    /// Param file querying
    /// </summary>
    internal class DebugParamQueries
    {
        internal static Dictionary<int, RandoInfo2> ActiveDropDescriptions()
        {
            // An "ActiveDrop" is a character defined in GeneratorParam
            // which can be idendified/linked with an ItemLotChr row
            // and where said ItemLot is non-empty.
            //
            // The output dictionary provides an ItemLotId lookup
            // with the name read from the ParamGenerator table.
            //
            // Only one member of the group is chosen if there are 
            // duplicates Chr pointing to same table.
            if (!ParamMan.IsLoaded)
                throw new Exception("Shouldn't be able to get here until Hook has finished");

            Dictionary<int, RandoInfo2> dropdict = new();
            foreach (var droprow in ParamMan.ItemLotChrRows)
            {
                RandoInfo2 ri2;
                // try find direct:
                if (TryFindGenRow(droprow.ID, out var genrow) && genrow != null)
                {
                    ri2 = new RandoInfo2(genrow.Param.FilePath, genrow.Name, genrow.ID, true);
                    dropdict[droprow.ID] = ri2;
                    continue;
                }

                // Try find indirect
                if (TryFindRegistRow(droprow.ID, out var regrow) && regrow != null)
                {
                    ri2 = new RandoInfo2(regrow.Param.FilePath, regrow.Name, regrow.ID, false);
                    dropdict[droprow.ID] = ri2;
                    continue;
                }

                // not used perhaps?
                ri2 = new RandoInfo2("cantfind", "cantfind", -1, false);
                dropdict[droprow.ID] = ri2;
            }
            return dropdict;
        }
        internal static bool TryFindRegistRow(int itemlotid, out GeneratorRegistRow? registrow)
        {
            // look for matching item via Regist -> Enemy -> ItemLot
            foreach (var genregistparam in ParamMan.GeneratorRegistParams)
            {
                var prows = genregistparam.Rows.OfType<GeneratorRegistRow>();
                foreach (var row in prows)
                {
                    var ilot = row.Enemy?.ItemLot?.ID;
                    if (ilot == itemlotid)
                    {
                        registrow = row;
                        return true;
                    }
                }
            }
            registrow = null;
            return false;
        }
        internal static bool TryFindGenRow(int itemlotid, out GeneratorParamRow? paramrow)
        {
            // look for matching item via Regist -> Enemy -> ItemLot
            foreach (var genparam in ParamMan.GeneratorParams)
            {
                var prows = genparam.Rows.OfType<GeneratorParamRow>();
                foreach (var row in prows)
                {
                    if (row.ItemLotID == itemlotid)
                    {
                        paramrow = row;
                        return true;
                    }
                }
            }
            paramrow = null;
            return false;
        }
    
        internal static void QueryTesting()
        {
            //var testx = AllPtf.OfType<ShopRdz>()
            //                    .Where(shp => shp.HasVanillaItemID(ITEMID.CRYSTALSOULSPEAR)).ToList();
            //var testx2 = okShops.Where(shp => shp.HasVanillaItemID(ITEMID.CRYSTALSOULSPEAR)).ToList();
            // query testing
            //var test = LTR_flatlist.Where(di => di.ItemID == (int)ITEMID.CRYSTALSOULSPEAR).ToList();
            //var test2 = AllP.Where(rdz => rdz.HasVanillaItemID((int)ITEMID.BINOCULARS)).ToList();

            //var test3 = Hook?.CheckLoadedEnemies(CHRID.TARGRAY);

            //var test4 = LTR_flatlist.Where(di => di.Infusion != 0).ToList();
            //var test5 = AllP.Where(rdz => rdz.Flatlist.Any(di => di.Infusion != 0)).ToList();
        }
    }
}
