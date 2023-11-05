using DS2S_META.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Shapes;

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

            // dump to file
            List<string> lines = new();
            foreach (var kvp in dropdict)
            {
                var rr = kvp.Value;
                lines.Add($"{kvp.Key} => {rr.ID}, {rr.EnemyName}, {rr.AreaString}, {rr.IsDirect}");
            }

            // Write file:
            File.WriteAllLines("./dropdesc.txt", lines.ToArray());


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
                    var ilot = row.Enemy?.ItemLotID;
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

        internal static Dictionary<int, string> ReadShopNames()
        {
            Dictionary<int, string> shopnames = new();

            // Read all:
            var lines = File.ReadAllLines("./Resources/Paramdex_DS2S_09272022/ShopLineupParam.txt");

            // Setup parser:
            Regex re = new(@"(?<paramid>\d+) (?<desc>.*)");
            foreach (var line in lines)
            {
                var match = re.Match(line);
                int paramid = int.Parse(match.Groups["paramid"].Value);
                string desc = match.Groups["desc"].Value;
                shopnames.Add(paramid, desc);
            }
            return shopnames;

        }

        internal static void QueryTesting()
        {
            //var temp = ParamMan.ItemRows?.Where(it => it.ItemTypeParamId == 1300).ToList();
            var temp = ParamMan.ItemRows?.Where(it => ((it.ItemState >> 3) & 1) == 1).ToList(); // & 8

            var test = ParamMan.ItemTypeParam;
            QuickPrint();
            var debug = 1;

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
        internal static void QuickPrint()
        {
            var itemTypes18 = ParamMan.ItemRows?.GroupBy(it => it.ItemTypeRow?.Unk18);

            // Prep:
            List<string> lines = new()
            {
                // Intro line
                $"Printing ItemType18",
                "---------------------------------------------",
            };

            foreach (var grp in itemTypes18)
            {
                List<string> grplines = new()
                {
                    "",
                    "-------------------------------------",
                    $"Printing items in group: {grp.Key}"
                };

                foreach (var itemrow in grp)
                {
                    grplines.Add($"{itemrow.MetaItemName}");
                }
                lines.AddRange( grplines );
            }

            // Write file:
            File.WriteAllLines("./itemtype18_testing.txt", lines.ToArray());
        }

        internal class RandoInfo2
        {
            internal string AreaString;
            internal string EnemyName;
            internal int ID;
            internal bool IsDirect;

            internal RandoInfo2(string? areastring, string enemyname, int paramid, bool directlot)
            {
                AreaString = areastring ?? "cantfindmap";
                EnemyName = enemyname;
                ID = paramid;
                IsDirect = directlot;
            }
        }
    }
}
