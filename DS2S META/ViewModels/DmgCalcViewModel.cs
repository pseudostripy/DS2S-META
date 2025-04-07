using DS2S_META.Utils;
using DS2S_META.Utils.Offsets.HookGroupObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using DS2S_META.DataClassHelpers.Commands;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;
using System.Collections;
using DS2S_META.Utils.ParamRows;
using Basic.Reference.Assemblies;

namespace DS2S_META.ViewModels
{
    public class DmgCalcViewModel : ViewModelBase
    {
        private ObservableCollection<DS2SItem> _weaponList;
        public ICollectionView? WeaponCollectionView { get; }

        private ObservableCollection<DS2SInfusion> _infusionList;
        public ICollectionView? InfusionCollectionView { get; set; }

        private ScalingBonusHGO? ScalingBonusHGO => Hook?.DS2P?.ScalingBonusHGO;

        // Constructor
        public DmgCalcViewModel()
        {
            var wepCat = DS2Resource.Weapons;
            wepCat.Sort();
            _weaponList = new ObservableCollection<DS2SItem>(wepCat); // alphabetical
            _infusionList = new ObservableCollection<DS2SInfusion>(new List<DS2SInfusion>());

            WeaponCollectionView = CollectionViewSource.GetDefaultView(_weaponList);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);

            WeaponCollectionView.Filter += FilterWeapons;
            SetWeapon = new SetWeaponCommand(this);

            // initialize commands
            QueryExecuteCommand = new RelayCommand(QueryExecuteExecute, QueryExecuteCanExec);


        }
        public ICommand QueryExecuteCommand { get; set; }
        private bool QueryExecuteCanExec(object? parameter) => MetaFeature.FtQueryExecute;
        //private void QueryExecuteExecute(object? parameter) => PopulateItemRowsTest();
        private void QueryExecuteExecute(object? parameter) => TestCustomCodeAttempt2();

        private void TestCustomCodeAttempt2()
        {
            var query = ParamMan.ItemLotOtherRows.Where(ilr => ilr.NumDrops > 2).ToList();
            var test = Hook.DS2P.MapManager.GetLootItemPack();
            int debug = 1;
        }

        private async void TestCustomCodeAttempt()
        {
            StaticTest.initialize();
            var ns = new Nonstatic(18);

            ParamMan.TestInt = 5;
            // see if we can compile a custom program and run it :O

            string userCode = @"
            using System;
            using DS2S_META.Utils;
            using System.Collections.Generic;
            using DS2S_META;
            using System.Collections;
            using System.Collections.ObjectModel;
            using System.Linq;

            public class UserCode
            {
                public int Execute(Nonstatic ns)
                {
                    MyTestClass testclass = new MyTestClass(9);
                    List<MyTestClass>? testclasses = new List<MyTestClass>();    
                    testclasses.Add(testclass);
                    var test = new List<int>() { 1, 2, 4 };
                    //var test2 = ParamMan.TestListParam[2];
                    var test3 = StaticTest.MyClasses.Where(f => f.MyField < 12).First().MyField;
                    //return test3;
                    var test4 = ns.MyClasses[0].MyField;
                    var test5 = ns.testing;
                    return test3;
                    //return testclasses[0].MyField;
                    //return test[2];
                }
            }";


            //var test3 = StaticTest.MyClasses.Where(f => f.MyField < 12).First().MyField;
            //MyTestClass testclass = new MyTestClass();
            //List<MyTestClass> testclasses = new List<MyTestClass>();
            //testclasses.Add(testclass);

            //return ParamMan.ItemRows?.Where(ir => ir.MetaItemName.ToLower().Contains(""rapier"")).First();
            //ParamMan.ItemRows;
            object? result = null;

            try
            {
                // Compile the user code
                var assembly = await Task.Run(() => CompileCode(userCode));

                // Load the UserCode type
                var type = assembly.GetType("UserCode");

                if (type != null)
                {
                    // Create an instance of the compiled class
                    var instance = Activator.CreateInstance(type);
                    var method = type.GetMethod("Execute");

                    if (method != null)
                    {
                        // Invoke the ProcessData method with AppData as an argument
                        //result = method.Invoke(instance, null);
                        result = method.Invoke(instance, new object[] { ns });
                    }
                    else
                    {
                        Console.WriteLine("Method 'Execute' not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to load the UserCode type.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            var test = (int)(result ?? -1);
            //var test = result as string;
            var debug = 1;

        }

        static Assembly CompileCode(string code)
        {
            // Parse the code into a SyntaxTree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Define assembly and module names
            var assemblyName = "UserCodeAssembly";

            // References to current and system assemblies
            var references = new[]
            {
                //MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // Mscorlib,
                //MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ParamMan).Assembly.Location), // Reference to current application
                //MetadataReference.CreateFromFile(typeof(ItemLotRow).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(ItemRow).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(Param).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MyTestClass).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StaticTest).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Nonstatic).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),

                //MetadataReference.CreateFromFile("System.Collections.dll"),
                //MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(System.Collections).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
                //MetadataReference.CreateFromFile(AppDomain.CurrentDomain
                //    .GetAssemblies()
                //    .Single(a => a.GetName().Name == "System.Runtime")
                //    .Location)
            };

            //List<PortableExecutableReference> testrefall = new();
            //var asms = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (Assembly? assembly in asms)
            //{
            //    if (assembly != null)
            //    {
            //        var test = assembly.Location;
            //        if (test != null && !test.Contains("null"))
            //        {
            //            try
            //            {
            //                testrefall.Add(MetadataReference.CreateFromFile(assembly.Location));

            //            }
            //            catch (Exception ex)
            //            {
            //            }

            //        }

            //    }
            //}
            //references.Concat(testrefall.ToArray());

            //Compile the code
            //var compilation = CSharpCompilation.Create(
            //    assemblyName,
            //    new[] { syntaxTree },
            //    references,
            //    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));


            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references: ReferenceAssemblies.Net80)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug, checkOverflow: true))
                    .AddReferences(references);
            

            // Emit the assembly to memory
            using var ms = new System.IO.MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                // Capture and display errors
                var errors = string.Join(Environment.NewLine, result.Diagnostics);
                throw new InvalidOperationException($"Compilation failed: {errors}");
            }

            // Load the compiled assembly
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }


        public ObservableCollection<ItemRow> ItemRowsTest { get; set; } = [];

        public void PopulateItemRowsTest()
        {
            //var testrows = ParamMan.ItemRows?.Where(ir => ir.MetaItemName.ToLower().Contains("e"))
            //    .ToList() ?? []; // default empty
            //ItemRowsTest.Clear();
            //ItemRowsTest.AddRange(testrows);

            var successItemRows = ParamMan.ItemLotOtherRows?.Where(ilr => 
                    ilr.Items.Where(ir => ir.AsItemRow().MetaItemName.ToLower().Contains("ee")).Count() > 1)
                    .ToList() ?? []; // default empty

            List<ItemRow> testrows = [];
            for (int i = 0; i < successItemRows.Count; i++)
            {
                var ilr = successItemRows[i];
                var eeItems = ilr.Items.Select(i => i.AsItemRow()).Where(ir => ir.MetaItemName.ToLower().Contains("ee")).ToList();
                testrows.AddRange(eeItems);
            }
            
            ItemRowsTest.Clear();
            ItemRowsTest.AddRange(testrows);


        }


        public WeaponRow? WepSel { get; set; }

        public string hModString
        {
            get
            {
                string strhand = LeftHandSelected ? "lMod" : "rMod";
                return $"hMod ({strhand})";
            }
        }
        public string SetWepLabel
        {
            get
            {
                if (SelectedItem == null) 
                    return "UNSELECTED WEAPON";
                return $"{SelectedInfusion} {SelectedItem?.Name} +{UpgradeVal}";
            }
        }
        private int _nudUpgrMax = 5;
        public int NudUpgrMax {
            get => _nudUpgrMax;
            set
            {
                _nudUpgrMax = value;
                OnPropertyChanged(nameof(NudUpgrMax));
            }
        }


        private void UpdateWepStats()
        {
            WepSel = ParamMan.GetWeaponFromID(SelectedItem?.ItemId);
            NudUpgrMax = WepSel?.MaxUpgrade ?? 0;

            var inflist = WepSel?.GetInfusionList();
            if (inflist == null) return;
            _infusionList = new ObservableCollection<DS2SInfusion>(inflist);
            InfusionCollectionView = CollectionViewSource.GetDefaultView(_infusionList);
            OnPropertyChanged(nameof(InfusionCollectionView));
            OnPropertyChanged(nameof(SetWepLabel));
        }

        private bool FilterWeapons(object obj)
        {
            return true; // todo?
        }

        // Update (called on mainwindow update interval)
        public override void UpdateViewModel()
        {
            if (!ParamMan.IsLoaded)
                return;

            // Update the ones we care about:
            LMod = WepSel?.WTypeRow?.LMod ?? 0;
            RMod = WepSel?.WTypeRow?.RMod ?? 0;

            // Calc scaling:
            pScale = (int)Math.Floor( CalcScaling() );
            pBase = (int)Math.Floor( WepSel?.ReinforceRow?.GetPhysDmg(UpgradeVal) ?? 0 );
            OnPropertyChanged(nameof(pAR));

            CounterDmg = WepSel?.CounterDamage ?? 0;
        }

        public float CalcScaling()
        {
            // Always do str/dex:
            var strbonus = ScalingBonusHGO?.GetBonus(BNSTYPE.STR);
            var strsf = WepSel?.ReinforceRow?.WeaponStatsAffectRow?.ReadScalingValue(WeaponStatsAffectRow.SCTYPE.STR, UpgradeVal); // scale factor

            var dexbonus = ScalingBonusHGO?.GetBonus(BNSTYPE.DEX);
            var dexsf = WepSel?.ReinforceRow?.WeaponStatsAffectRow?.ReadScalingValue(WeaponStatsAffectRow.SCTYPE.DEX, UpgradeVal);

            var scaling = strsf * strbonus + dexsf * dexbonus;
            if (scaling == null) return 0;
            return (float)scaling;
        }

        // Commands:
        public ICommand? SetWeapon { get; private set; }

        // Properties:
        private float _lMod = 0;
        public float LMod
        {
            get => _lMod;
            set
            {
                if (SetField(ref _lMod, value))
                    OnPropertyChanged(nameof(hMod));
            }
        }
        private float _rMod = 0;
        public float RMod
        {
            get => _rMod;
            set
            {
                if (SetField(ref _rMod, value))
                    OnPropertyChanged(nameof(hMod));
            }
        }
        private int _pBase = 0;
        public int pBase
        {
            get => _pBase;
            set => SetField(ref _pBase, value);
        }
        private int _pScale = 0;
        public int pScale
        {
            get => _pScale;
            set => SetField(ref _pScale, value);
        }
        public short _counterDmg = 0;
        public short CounterDmg
        {
            get => _counterDmg;
            set => SetField(ref _counterDmg, value);
        }

        public int pAR => pBase + pScale;

        public float hMod { 
            get
            {
                if (LeftHandSelected)
                    return LMod;
                return RMod;
            } 
        }

        private bool _leftHandSelected = false;
        public bool LeftHandSelected 
        {
            get => _leftHandSelected;
            set
            {
                _leftHandSelected = value;
                OnPropertyChanged(nameof(hModString));
                OnPropertyChanged(nameof(hMod));
            }
        }

        // UI properties:
        private DS2SItem? _selectedItem;
        public DS2SItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetField(ref _selectedItem, value);
                OnPropertyChanged();
                UpdateWepStats();
            }
        }

        public DS2SInfusion _selectedInfusion;
        public DS2SInfusion SelectedInfusion
        {
            get => _selectedInfusion;
            set
            {
                _selectedInfusion = value;
                OnPropertyChanged(nameof(SetWepLabel));
            }
        }

        private int _upgradeval = 0;
        public int UpgradeVal {
            get => _upgradeval;
            set
            {
                _upgradeval = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SetWepLabel));
            }
        }

    }
}
