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
        public ObservableCollection<PropertyViewModel> QueryResults { get; set; } = [];
        public string UserQueryText { get; set; } = "";
        public const int QUERYERROR = -1;

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
        private void QueryExecuteExecute(object? parameter) => UserQueryExecute();

        private string CreateUserCode()
        {
            string[] lines = UserQueryText.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string lastline = lines[lines.Length - 1];
            if (!lastline.StartsWith("object query = "))
            {
                lastline = "object query = " + lastline; // prepend fix;
            }
            if (!lastline.Trim().EndsWith(';'))
            {
                lastline = lastline.Trim() + ";"; // ending fix
            }
            lines[lines.Length - 1] = lastline; // last line QOL fix
            string fixed_string = string.Join(Environment.NewLine, lines);

            // Setup template
            string userCode = $@"
            using System;
            using DS2S_META;            
            using DS2S_META.Utils;
            using DS2S_META.Utils.ParamRows;
            using System.Collections.Generic;
            using System.Collections;
            using System.Collections.ObjectModel;
            using System.Linq;
            using DS2S_META.Randomizer; // has ShopRow for some reason

            public class UserCode
            {{
                public object Execute()
                {{
                    List<ItemLotRow> lots = ParamMan.ItemLotOtherRows;
                    List<ItemDropRow> drops = ParamMan.ItemLotChrRows;
                    List<ShopRow> shops = ParamMan.ShopRows;
                    { fixed_string }
                    return query;
                }}
            }}";
            return userCode;

            // e.g.:
            //object query = ParamMan.ItemLotOtherRows.Where(il => il.HasItem((int)ITEMID.FIREBOMB)).ToList();
        }

        private async void UserQueryExecute()
        {
            // see if we can compile a custom program and run it :O
            string userCode = CreateUserCode();
            var queryresult = await Task.Run(() => RunCustomCode(userCode));
            if (queryresult is int iq && iq == QUERYERROR)
                return;
            UpdateResultsOutput(queryresult);
        }

        private void UpdateResultsOutput(object obj)
        {
            QueryResults.Clear();
            if (obj is IEnumerable objvec && obj is not string)
            {
                int i = 0;
                foreach (var obji in objvec)
                {
                    QueryResults.Add(new PropertyViewModel($"{i}", obji));
                    i++;
                }
            }
            else
            {
                QueryResults.Add(new PropertyViewModel($"{obj}", obj));
            }
        }

        private static async Task<object> RunCustomCode(string userCode)
        {
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
                        result = method.Invoke(instance, new object[] { });
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
                var cexp = new MetaException("Compilation Exception", ex.Message,ex);
                MetaExceptionStaticHandler.Handle(cexp);
            }
            return result ?? QUERYERROR;
        } 

        //private ExamplePassingNonStaticVariable()
        //{
        //    public int Execute(Nonstatic ns)
        //    {
        //        MyTestClass testclass = new MyTestClass(9);
        //        List<MyTestClass>? testclasses = new List<MyTestClass>();
        //        testclasses.Add(testclass);
        //        var test = new List<int>() { 1, 2, 4 };
        //        //var test2 = ParamMan.TestListParam[2];
        //        var test3 = StaticTest.MyClasses.Where(f => f.MyField < 12).First().MyField;
        //        //return test3;
        //        var test4 = ns.MyClasses[0].MyField;
        //        var test5 = ns.testing;
        //        return test3;
        //        //return testclasses[0].MyField;
        //        //return test[2];
        //    }
        //}

        static Assembly CompileCode(string code)
        {
            // Parse the code into a SyntaxTree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            // Define assembly and module names
            var assemblyName = "UserCodeAssembly";

            // References to current and system assemblies
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(ParamMan).Assembly.Location), // Reference to current application
                //MetadataReference.CreateFromFile(typeof(MyTestClass).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(StaticTest).Assembly.Location),
                //MetadataReference.CreateFromFile(typeof(Nonstatic).Assembly.Location),
            };

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
