using PropertyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using DS2S_META.Utils;
using System.Reflection;
using DS2S_META.Utils.Offsets;
using Xceed.Wpf.Toolkit;
using Keystone;
using System.Threading.Tasks;
using static DS2S_META.Utils.ItemRow;
using DS2S_META.Randomizer;
using Octokit;
using System.CodeDom;
using System.Security.Cryptography;
using DS2S_META.Utils.Offsets.CodeLocators;
using DS2S_META.Utils.Offsets.HookGroupObjects;
using DS2S_META.Utils.Offsets.OffsetClasses;
using System.Windows.Controls;
using System.Windows.Threading;
using DS2S_META.Dialog;
using DS2S_META.Utils.DS2Hook.MemoryMods;

namespace DS2S_META.Utils.DS2Hook
{
    public class DS2SHook : PHook, INotifyPropertyChanged
    {
        //public static readonly string ExeDir = Environment.CurrentDirectory;
        //private List<Inject> InstalledInjects = new();
        //private bool DmgModInstalled => DmgModInj1 != null;
        //private IntPtr DmgModCodeAddr = IntPtr.Zero;
        //private Inject? DmgModInj1 = null;
        //private Inject? DmgModInj2 = null;

        public MainWindow MW { get; set; }

        // Event Handling:
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }
        public event EventHandler<GameStateEventArgs> OnGameStateHandler;
        public void RaiseGameStateChange(int oldstate, int newstate)
        {
            OnGameStateHandler?.Invoke(this, new GameStateEventArgs(this, oldstate, newstate));
        }

        //public IntPtr ModuleAddress => Process?.MainModule?.BaseAddress ?? IntPtr.Zero;
        public string ID => Process?.Id.ToString() ?? "Not Hooked";

        private string _version = "";
        public string Version
        {
            get => _version;
            private set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public DS2VER DS2Ver;
        public BBJTYPE BBJType;

        //internal DS2HookOffsets Offsets;
        public bool InGame => DS2P?.CGS.InGame ?? false; // wrapper (toremove?)

        // -----------------------------------------   WIP   ---------------------------------------
        public DS2Ptrs DS2P;
        private AssemblyScripts ASM;
        private SpeedhackManager? SpeedhackMan;
        public void SetupPointers2()
        {
            DS2P = new(this, DS2Ver);
            ASM = new AssemblyScripts(this, DS2P);
            SpeedhackMan = new SpeedhackManager(this);
        }

        // Exposed interfaces
        public void EnableSpeedhack(bool value) => SpeedhackMan?.Speedhack(value);
        public void SetSpeedhackSpeed(double value) => SpeedhackMan?.SetSpeed(value);

        // Utility Info
        //public static bool Reading { get; set; }
        public NoDmgMod? NoDmgMod;



        public bool IsLoading => DS2P.CGS.IsLoading;

        //private int _gamestate;
        //public int GameState
        //{
        //    get => _gamestate;
        //    set
        //    {
        //        if (value == _gamestate) return;
        //        var oldstate = _gamestate;
        //        _gamestate = value; // needs to be read during event
        //        RaiseGameStateChange(oldstate, value);
        //    }
        //}
        //public bool InGame => GameState == (int)GAMESTATE.LOADEDINGAME;
        //public bool InMainMenu => GameState == (int)GAMESTATE.MAINMENU;

        //public bool 
        public bool Focused => Hooked && User32.GetForegroundProcessID() == Process.Id;
        public bool IsInitialized = false;

        // Pointer Setups
        //private PHPointer GiveSoulsFunc;
        //private PHPointer RemoveSoulsFunc;
        //private PHPointer ItemGiveFunc;
        //private PHPointer ItemStruct2dDisplay;
        //private PHPointer phpDisplayItem;
        //private PHPointer SetWarpTargetFunc;
        //private PHPointer WarpManager;
        //private PHPointer WarpFunc;
        //private PHPointer? SomePlayerStats;

        //public PHPointer BaseA;

        //private PHPointer PlayerName;
        //private PHPointer AvailableItemBag;
        //private PHPointer ItemGiveWindow;
        //private PHPointer PlayerBaseMisc;
        //private PHPointer PlayerCtrl;
        //private PHPointer PlayerPosition;
        //private PHPointer PlayerGravity;
        //private PHPointer PlayerParam;
        //private PHPointer PlayerType;
        //private PHPointer SpEffectCtrl;
        //private PHPointer ApplySpEffect;
        //private PHPointer PlayerMapData;
        //private PHPointer EventManager;
        //private PHPointer BonfireLevels;
        //private PHPointer NetSvrBloodstainManager;

        //private PHPointer BaseASetup;
        //private PHPointer BaseBSetup;
        //private PHPointer BaseB;
        //private PHPointer Connection;

        //private PHPointer Camera;
        //private PHPointer Camera2;
        //private PHPointer Camera3;
        //private PHPointer Camera4;
        //private PHPointer Camera5;

        //private PHPointer SpeedFactorAccel;
        //private PHPointer SpeedFactorAnim;
        //private PHPointer SpeedFactorJump;
        //private PHPointer SpeedFactorBuildup;

        //public PHPointer LoadingState;
        //public PHPointer phDisableAI; // pointer head (missing final offset)
        //public PHPointer phBIKP1SkipVals; // pointer head (missing final offset)


        public DS2SHook(int refreshInterval, int minLifetime) :
            base(refreshInterval, minLifetime, p => p.MainWindowTitle == "DARK SOULS II")
        {
            Version = "Not Hooked";
            OnHooked += DS2Hook_OnHooked;
            OnUnhooked += DS2Hook_OnUnhooked;
        }

        //public void RegisterAOBs()
        //{
        //if (Offsets?.Func == null)
        //    throw new Exception("Func structure null");

        //BaseBSetup = RegisterAbsoluteAOB(Offsets.Func.BaseBAoB);
        //SpeedFactorAccel = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAccelOffset);
        //SpeedFactorAnim = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAnimOffset);
        //SpeedFactorJump = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorJumpOffset);
        //SpeedFactorBuildup = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorBuildupOffset);
        //GiveSoulsFunc = RegisterAbsoluteAOB(Offsets.Func.GiveSoulsFuncAoB);
        //RemoveSoulsFunc = RegisterAbsoluteAOB(Offsets.Func.RemoveSoulsFuncAoB);
        //ItemGiveFunc = RegisterAbsoluteAOB(Offsets.Func.ItemGiveFunc);
        //ItemStruct2dDisplay = RegisterAbsoluteAOB(Offsets.Func.ItemStruct2dDisplay);
        //SetWarpTargetFunc = RegisterAbsoluteAOB(Offsets.Func.SetWarpTargetFuncAoB);
        //WarpFunc = RegisterAbsoluteAOB(Offsets.Func.WarpFuncAoB);

        // Version Specific AOBs:
        //ApplySpEffect = RegisterAbsoluteAOB(Offsets.Func.ApplySpEffectAoB);
        //phpDisplayItem = RegisterAbsoluteAOB(Offsets.Func.DisplayItem); // CAREFUL WITH THIS!
        //}

        public enum WARPOPTIONS
        {
            DEFAULT,
            WARPREST,
            WARPONLY,
        }

        // DS2 & BBJ Process Info Data
        private enum BYTECODES
        {
            // used for sotfs differentiation:
            NOBBJBYTE = 0xF3,
            NEWBBJBYTE = 0x49,

            // used for vanilla differentiation:
            JUMPREL32 = 0xE9,
            MOV_ECX_EAX = 0x8B,
            MOV_EAX_DWORTPTR = 0x8B,
            MOVSS = 0xF3,
        }
        public enum BBJTYPE
        {
            NOBBJ,
            OLDBBJ_VANILLA,
            NEWBBJ_VANILLA,
            OLDBBJ_SOTFS,
            NEWBBJ_SOTFS,
            UNKN_VANILLA,
        }
        private static readonly DS2VER[] ValidVanillaVers = new DS2VER[] { DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112 };
        private static readonly DS2VER[] ValidSotfsVers = new DS2VER[] { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 };
        public bool IsOldPatch => DS2Ver == DS2VER.VANILLA_V102;
        public bool IsSOTFS_CP => DS2Ver == DS2VER.SOTFS_V103;
        public bool IsSOTFS => ValidSotfsVers.Contains(DS2Ver);
        public bool IsVanilla => ValidVanillaVers.Contains(DS2Ver);
        public bool IsValidVer => IsSOTFS || IsVanilla;

        // Hook setup / cleanup
        private void DS2Hook_OnHooked(object? sender, PHEventArgs e)
        {
            DS2Ver = GetDS2Ver();
            //IsValidVer = CheckValidVer();

            // Initial Setup & Version Checks:
            //Offsets = GetOffsets();
            //if (!BasePointerSetup(out bool isOldBbj)) // set BaseA (base pointer)
            //    return; // basepointer setup failure

            // TODO!!
            //BBJType = GetBBJType(isOldBbj);
            BBJType = GetBBJType(false);

            //RegisterAOBs(); // Absolute AoBs
            //RescanAOB();
            //SetupChildPointers();

            // Refactoring:
            SetupPointers2();

            // Slowly migrate to param handling class:
            ParamMan.Initialise(this);
            GetLevelRequirements();

            UpdateStatsProperties();
            Version = GetStringVersion();
            SpeedhackMan?.Setup();

            IsInitialized = true;
            OnPropertyChanged(nameof(Hooked));
        }
        private void DS2Hook_OnUnhooked(object? sender, PHEventArgs e) => Cleanup();
        public void Cleanup()
        {
            Version = "Not Hooked";
            HandleRivaAndSpeedhackUnhooking();
            ParamMan.Uninitialise();
            MW.HKM.ClearHooks();
            OnPropertyChanged(nameof(Hooked));
            IsInitialized = false;
        }
        private enum UNHOOKINGSTYLE
        {
            VANILLA,
            SOTFS_NOSH,
            SOTFS_SH_NORIVA_RESTART,
            SOTFS_SH_RIVA_RESTART
        }
        private void HandleRivaAndSpeedhackUnhooking()
        {
            var unhookStyle = GetUnhookStyle();
            switch (unhookStyle)
            {
                case UNHOOKINGSTYLE.VANILLA:
                    UnhookVanilla();
                    return;

                case UNHOOKINGSTYLE.SOTFS_NOSH:
                    RivaHook.OnUnhooked();
                    return;

                case UNHOOKINGSTYLE.SOTFS_SH_NORIVA_RESTART:
                    RivaUnhookSlow();
                    return;

                case UNHOOKINGSTYLE.SOTFS_SH_RIVA_RESTART:
                    UnhookWithRivaRestart();
                    return;

                default:
                    throw new Exception("Unexpected UNHOOKINGSTYLE enum");
            }
        }
        private UNHOOKINGSTYLE GetUnhookStyle()
        {
            // Logic for deciding which Riva/Speedhack unhooking process to use
            if (IsVanilla) return UNHOOKINGSTYLE.VANILLA;
            if (SpeedhackMan?.SpeedhackEverEnabled != true) return UNHOOKINGSTYLE.SOTFS_NOSH;
            if (!Properties.Settings.Default.RestartRivaOnClose) return UNHOOKINGSTYLE.SOTFS_SH_NORIVA_RESTART;
            return UNHOOKINGSTYLE.SOTFS_SH_RIVA_RESTART;
        }
        private void UnhookVanilla()
        {
            RivaHook.OnUnhooked();
            SpeedhackMan?.ClearSpeedhackInject();
        }
        private void RivaUnhookSlow()
        {
            // Unload and wait for RIVA to refresh itself ~2mins
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => { RivaHook.RefreshEnd(); }));
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SpeedhackMan?.ClearSpeedhackInject();
                RivaHook.OnUnhooked();
            }));
        }
        private void UnhookWithRivaRestart()
        {
            // Try reopen RIVA programatically
            string rivaExePath = Properties.Settings.Default.RivaExePath;
            bool canFindRiva = File.Exists(rivaExePath);


            List<string> rtssProcNames = new() { "RTSS", "RTSSHooksLoader64" };
            var RTSSprocs = Process.GetProcesses().Where(proc => rtssProcNames.Contains(proc.ProcessName)).ToList();

            if (RTSSprocs.Count == 0)
            {
                // RTSS not open (nothing to do)
                SpeedhackMan?.ClearSpeedhackInject();
                return;
            }
            if (!canFindRiva)
            {
                MetaInfoMessages.RivaNotFound(rivaExePath);
                RivaUnhookSlow();
                return;
            }

            // Kill RTSS and request to reopen it
            SpeedhackMan?.ClearSpeedhackInject();
            foreach (var proc in RTSSprocs)
                proc.Kill();
            Util.ExecuteAsAdmin(rivaExePath);
        }

        // Major setup functions:
        internal DS2VER GetDS2Ver()
        {
            // Size of running DS2 application
            var moduleSz = Process?.MainModule?.ModuleMemorySize;

            if (Is64Bit)
                return GetSotfsVer(moduleSz);

            return GetVanillaVer(moduleSz);
        }
        //internal bool CheckValidVer()
        //{
        //    var validvers = new DS2VER[]
        //    {
        //        DS2VER.VANILLA_V102,
        //        DS2VER.VANILLA_V111,
        //        DS2VER.VANILLA_V112,
        //        DS2VER.SOTFS_V102,
        //        DS2VER.SOTFS_V103,
        //    };
        //    return validvers.Contains(DS2Ver);
        //}
        private static DS2VER GetVanillaVer(int? modulesz)
        {
            return modulesz switch
            {
                DS2ModuleInfo.ModuleSizes.VanillaV112 => DS2VER.VANILLA_V112,
                DS2ModuleInfo.ModuleSizes.VanillaV111 => DS2VER.VANILLA_V111,
                DS2ModuleInfo.ModuleSizes.VanillaV102 => DS2VER.VANILLA_V102,
                _ => DS2VER.UNSUPPORTED,
            };
        }
        private static DS2VER GetSotfsVer(int? modulesz)
        {
            return modulesz switch
            {
                DS2ModuleInfo.ModuleSizes.SotfsV102 => DS2VER.SOTFS_V102,
                DS2ModuleInfo.ModuleSizes.SotfsV103 => DS2VER.SOTFS_V103,
                _ => DS2VER.UNSUPPORTED,
            };
        }
        internal BBJTYPE GetBBJType(bool isOldBbj)
        {
            // TODO VANILLA
            if (IsVanilla)
                return GetBBJTypeVanilla();

            if (isOldBbj)
                return BBJTYPE.OLDBBJ_SOTFS;


            // check for new bbj
            int jumpfcn_offset_V102 = 0x037B4BC;
            int jumpfcn_offset_V103 = 0x0381E1C;
            var jmpfcn_offset = IsSOTFS_CP ? jumpfcn_offset_V103 : jumpfcn_offset_V102;

            var module_addr = Process?.MainModule?.BaseAddress;
            if (module_addr == null)
                throw new Exception("Unknown DS2 MainModule size");
            var jmp_ptr = IntPtr.Add((IntPtr)module_addr, jmpfcn_offset);

            // Read a byte to see if the bbj inject is there:
            var jumpinj = CreateBasePointer(jmp_ptr);
            byte testbyte = jumpinj.ReadByte(0);
            return testbyte switch
            {
                (byte)BYTECODES.NOBBJBYTE => BBJTYPE.NOBBJ,
                (byte)BYTECODES.NEWBBJBYTE => BBJTYPE.NEWBBJ_SOTFS,
                _ => throw new Exception("Probably an issue with setting up the pointers/addresses"),
            };
        }
        internal BBJTYPE GetBBJTypeVanilla()
        {
            var jmpfcn_offset = DS2Ver switch
            {
                DS2VER.VANILLA_V102 => 0x033A424,
                DS2VER.VANILLA_V111 => 0x3A09C4,
                DS2VER.VANILLA_V112 => 0x3A7364,
                _ => throw new Exception("Shouldn't get here")
            };

            var module_addr = Process?.MainModule?.BaseAddress;
            if (module_addr == null)
                throw new Exception("Unknown DS2 MainModule size");
            var jmp_ptr = IntPtr.Add((IntPtr)module_addr, jmpfcn_offset);

            // Read a byte to see if the bbj inject is there:
            var jumpinj = CreateBasePointer(jmp_ptr);
            byte testbyte = jumpinj.ReadByte(0);
            bool isInjected = testbyte switch
            {
                (byte)BYTECODES.MOV_ECX_EAX => false,
                (byte)BYTECODES.JUMPREL32 => true,
                _ => throw new Exception("Shouldn't happen for Vanilla?"),
            };

            // Split out easy bbj types:
            if (!isInjected) return BBJTYPE.NOBBJ;
            if (DS2Ver == DS2VER.VANILLA_V112) return BBJTYPE.NEWBBJ_VANILLA; // only new version available
            if (DS2Ver == DS2VER.VANILLA_V102) return BBJTYPE.NEWBBJ_VANILLA; // only new version available

            // Finally differentiate between V1.11 old/new bbj mods:
            var reljump = jumpinj.ReadInt32(0x1); // read rel_jump (E9 XXXXXXXX LE)
            var addr_inj_code = jumpinj.Resolve() + reljump + 5; // 5 for instruction length
            var inj_code = CreateBasePointer(addr_inj_code);
            var testbyte2 = inj_code.ReadByte(0xE); // first byte that is different between versions

            // Differentiate:
            return testbyte2 switch
            {
                (byte)BYTECODES.MOVSS => BBJTYPE.OLDBBJ_VANILLA,
                (byte)BYTECODES.MOV_EAX_DWORTPTR => BBJTYPE.NEWBBJ_VANILLA,
                _ => throw new Exception("Probably shouldn't get this, unknown bbj inject")
            };
        }
        private string GetStringVersion()
        {
            if (!IsSOTFS && !IsVanilla)
                return "Unknown game or version";

            StringBuilder sb = new();

            // get main version:
            if (IsSOTFS)
                sb.Append("Sotfs");
            else
                sb.Append("Vanilla");

            // get sub-version
            sb.Append(' ');
            switch (DS2Ver)
            {
                case DS2VER.SOTFS_V102:
                    sb.Append("V1.02");
                    break;

                case DS2VER.SOTFS_V103:
                    sb.Append("V1.03");
                    break;

                case DS2VER.VANILLA_V102:
                    sb.Append("V1.02 Old Patch");
                    break;

                case DS2VER.VANILLA_V111:
                    sb.Append("V1.11");
                    break;

                case DS2VER.VANILLA_V112:
                    sb.Append("V1.12");
                    break;
            }

            // get mod versions:
            sb.Append(Environment.NewLine);
            switch (BBJType)
            {
                case BBJTYPE.NOBBJ:
                    sb.Append("(unmodded)");
                    break;

                case BBJTYPE.OLDBBJ_VANILLA:
                case BBJTYPE.OLDBBJ_SOTFS:
                    sb.Append("(old bbj mod)");
                    break;

                case BBJTYPE.NEWBBJ_VANILLA:
                case BBJTYPE.NEWBBJ_SOTFS:
                    sb.Append("(bbj mod)");
                    break;

                default:
                    sb.Append("(unknown mod)");
                    break;
            }
            return sb.ToString();
        }

        //internal DS2HookOffsets GetOffsets()
        //{
        //    return DS2Ver switch
        //    {
        //        DS2VER.VANILLA_V102 => new DS2VOffsetsV102(),
        //        DS2VER.VANILLA_V111 => new DS2VOffsetsV111(),
        //        DS2VER.VANILLA_V112 => new DS2VOffsetsV112(),
        //        DS2VER.SOTFS_V102 => new DS2SOffsetsV102(),
        //        DS2VER.SOTFS_V103 => new DS2SOffsetsV103(),
        //        _ => throw new Exception("Unexpected Sotfs Module Size, likely not supported."),
        //    };
        //}
        //internal bool BasePointerSetup(out bool isOldBbj)
        //{
        //    //BaseASetup = RegisterAbsoluteAOB(Offsets.BaseAAob);
        //    //RescanAOB();

        //    //// Attempt "normal" version:
        //    //IntPtr bp_orig = BasePointerFromSetupPointer(BaseASetup);
        //    //BaseA = CreateBasePointer(bp_orig);
        //    //isOldBbj = BaseA.Resolve() == IntPtr.Zero; // cannot find standard basea AoB
        //    //if (!isOldBbj)
        //    //    return true; // normal basea success

        //    //if (DS2Ver != DS2VER.VANILLA_V111 && DS2Ver != DS2VER.SOTFS_V102)
        //    //{
        //    //    MsgBoxCannotFindBasePointer();
        //    //    return false; // failure
        //    //}

        //    // Old BBJ mod BasePointer adjustment:
        //    // Get ptr to aob
        //    var bytestring = Offsets.BaseABabyJumpAoB;
        //    BaseASetup = RegisterAbsoluteAOB(bytestring);
        //    RescanAOB();
        //    BaseA = CreateBasePointer(BasePointerFromSetupBabyJ(BaseASetup));
        //    return true;
        //}
        //public void MsgBoxCannotFindBasePointer()
        //{
        //    // Run on dispatcher thread to avoid STA error
        //    System.Windows.Application.Current.Dispatcher.Invoke(MBBaseErr);
        //}
        //internal void MBBaseErr()
        //{
        //    MessageBox.Show($"Cannot find BasePtr for Version: {DS2Ver}, META likely won't work. Possible alleviations are to " +
        //        $"verify steam game files, check you're using the newest bbj mod dll and check that no other mods are editing " +
        //        $"the game code. If you're still having issues, ping pseudostripy on Discord and we can investigate.");
        //}


        //internal void SetupChildPointers()
        //{
            // Further pointer setup... todo?
            //Core? OC = Offsets.Core; // shorthand
            //if (OC == null) return;

            //PlayerName = CreateChildPointer(BaseA, OC.PlayerNameOffset);
            //AvailableItemBag = CreateChildPointer(BaseA, OC.GameDataManagerOffset, OC.AvailableItemBagOffset, OC.AvailableItemBagOffset);
            //ItemGiveWindow = CreateChildPointer(BaseA, OC.ItemGiveWindowPointer);

            //PlayerBaseMisc = CreateChildPointer(BaseA, OC.PlayerBaseMiscOffset);
            //PlayerCtrl = CreateChildPointer(BaseA, OC.PlayerCtrlOffset);
            //PlayerPosition = CreateChildPointer(PlayerCtrl, OC.PlayerPositionOffset1, OC.PlayerPositionOffset2);
            //PlayerGravity = CreateChildPointer(BaseA, OC.NoGrav);
            //PlayerParam = CreateChildPointer(PlayerCtrl, OC.PlayerParamOffset);
            //PlayerType = CreateChildPointer(PlayerCtrl, OC.PlayerTypeOffset);
            //SpEffectCtrl = CreateChildPointer(PlayerCtrl, OC.SpEffectCtrlOffset);
            //PlayerMapData = CreateChildPointer(BaseA, OC.PlayerDataMapOffset);
            //EventManager = CreateChildPointer(BaseA, OC.EventManagerOffset);
            //BonfireLevels = CreateChildPointer(EventManager, OC.BonfireLevelsOffset1, OC.BonfireLevelsOffset2);
            //WarpManager = CreateChildPointer(EventManager, OC.WarpManagerOffset);
            //NetSvrBloodstainManager = CreateChildPointer(BaseA, OC.NetSvrBloodstainManagerOffset1, OC.NetSvrBloodstainManagerOffset2, OC.NetSvrBloodstainManagerOffset3);


            //BaseB = CreateBasePointer(BasePointerFromSetupPointer(BaseBSetup));
            //Connection = CreateChildPointer(BaseB, OC.ConnectionOffset);

            //Camera = CreateChildPointer(BaseA, OC.CameraOffset1);
            //Camera2 = CreateChildPointer(Camera, OC.CameraOffset2);

            //if (Offsets.PlayerStatsOffsets != null)
            //    SomePlayerStats = CreateChildPointer(BaseA, Offsets.PlayerStatsOffsets);
            //if (Offsets.LoadingState != null)
            //{
            //    LoadingState = CreateChildPointer(BaseA, Offsets.LoadingState[0..^1]);
            //}
            //if (Offsets.DisableAI != null)
            //{
            //    phDisableAI = CreateChildPointer(BaseA, Offsets.DisableAI[0..^1]);
            //}
            //if (Offsets.BIKP1Skip_Val1 != null)
            //    phBIKP1SkipVals = CreateChildPointer(BaseA, Offsets.BIKP1Skip_Val1[0..^1]); // parent for both
        //}
        //public IntPtr BasePointerFromSetupPointer(PHPointer aobpointer)
        //{
        //    if (Offsets.BasePtrOffset1 == null)
        //        throw new Exception("Base pointer offset undefined");
        //    if (Offsets.BasePtrOffset2 == null)
        //        throw new Exception("Base pointer offset 2 undefined");

        //    if (IsVanilla)
        //    {
        //        var addrBaseA = CreateChildPointer(aobpointer, (int)Offsets.BasePtrOffset1);
        //        return addrBaseA.ReadIntPtr((int)Offsets.BasePtrOffset2);
        //    }
        //    else
        //    {
        //        // The instruction seems to be a relative offset in 64-bit?
        //        var readInt = aobpointer.ReadInt32((int)Offsets.BasePtrOffset1);
        //        return aobpointer.ReadIntPtr(readInt + (int)Offsets.BasePtrOffset2);
        //    }

        //}
        //public IntPtr BasePointerFromSetupBabyJ(PHPointer pointer)
        //{
            // Better version that isn't implemented yet:
            //string? bytestring = Offsets?.BaseABabyJumpAoB;
            //if (bytestring == null)
            //    throw new Exception("Cannot look for bbj baseA because no defined AoB string");

            //// Get ptr to aob
            //BaseASetup = RegisterAbsoluteAOB(bytestring);
            //RescanAOB();

            //var aob_sz = bytestring.Trim().Split(" ").Length;
            //var addr_basea = BaseASetup.ReadIntPtr(aob_sz - 4);
            //return CreateBasePointer(addr_basea);

            //var dbug = 1;


            //TODO!
        //    return pointer.ReadIntPtr(0x0121D4D0 + (int)Offsets.BasePtrOffset2);
        //}

        // To improve slowly over time:
        public bool CheckLoadedEnemies(CHRID chrid) => CheckLoadedEnemies((int)chrid);
        public bool CheckLoadedEnemies(int queryChrId)
        {
            if (DS2P.MiscPtrs.PHLoadedEnemiesTable == null)
                throw new Exception("Version error, should be handled in front end");

            int nmax = 70; // I think this is the most?
            int psize = Is64Bit ? 8 : 4;
            for (int i = 0; i < nmax; i++)
            {
                var chrclass = CreateChildPointer(DS2P.MiscPtrs.PHLoadedEnemiesTable, i*psize);
                int chrId = chrclass.ReadInt32(0x28); // to check generality
                if (chrId == queryChrId)
                    return true;
            }

            // not found in whole table
            return false;
        }


        // Player tab stuff:
        //public void UpdateGameState()
        //{
        //    GameState = Hooked ? BaseA.ReadInt32(Offsets.GameState) : -1;
        //}
        public void UpdateMainProperties()
        {
            DS2P?.UpdateProperties();
            //OnPropertyChanged(nameof(CharacterName));
            //OnPropertyChanged(nameof(ID));
            //OnPropertyChanged(nameof(Online));
        }
        public void UpdateStatsProperties()
        {
            //OnPropertyChanged(nameof(SoulLevel));
            //OnPropertyChanged(nameof(Souls));
            //OnPropertyChanged(nameof(SoulMemory));
            //OnPropertyChanged(nameof(SoulMemory2));
            //OnPropertyChanged(nameof(HollowLevel));
            //OnPropertyChanged(nameof(SinnerLevel));
            //OnPropertyChanged(nameof(SinnerPoints));
            //OnPropertyChanged(nameof(Vigor));
            //OnPropertyChanged(nameof(Endurance));
            //OnPropertyChanged(nameof(Vitality));
            //OnPropertyChanged(nameof(Attunement));
            //OnPropertyChanged(nameof(Strength));
            //OnPropertyChanged(nameof(Dexterity));
            //OnPropertyChanged(nameof(Adaptability));
            //OnPropertyChanged(nameof(Intelligence));
            //OnPropertyChanged(nameof(Faith));
        }
        public void UpdatePlayerProperties()
        {
            //OnPropertyChanged(nameof(Health));
            //OnPropertyChanged(nameof(HealthMax));
            //OnPropertyChanged(nameof(HealthCap));
            //OnPropertyChanged(nameof(Stamina));
            //OnPropertyChanged(nameof(MaxStamina));
            //OnPropertyChanged(nameof(TeamType));
            //OnPropertyChanged(nameof(CharType));
            //OnPropertyChanged(nameof(PosX));
            //OnPropertyChanged(nameof(PosY));
            //OnPropertyChanged(nameof(PosZ));
            //OnPropertyChanged(nameof(AngX));
            //OnPropertyChanged(nameof(AngY));
            //OnPropertyChanged(nameof(AngZ));
            //OnPropertyChanged(nameof(Collision));
            //OnPropertyChanged(nameof(Gravity));
            //OnPropertyChanged(nameof(StablePos));
            //OnPropertyChanged(nameof(LastBonfireAreaID));
            //OnPropertyChanged(nameof(Hooked));
        }
        //public void UpdateBonfireProperties()
        //{
        //    OnPropertyChanged(nameof(FireKeepersDwelling));
        //    OnPropertyChanged(nameof(TheFarFire));
        //    OnPropertyChanged(nameof(CrestfallensRetreat));
        //    OnPropertyChanged(nameof(CardinalTower));
        //    OnPropertyChanged(nameof(SoldiersRest));
        //    OnPropertyChanged(nameof(ThePlaceUnbeknownst));
        //    OnPropertyChanged(nameof(HeidesRuin));
        //    OnPropertyChanged(nameof(TowerOfFlame));
        //    OnPropertyChanged(nameof(TheBlueCathedral));
        //    OnPropertyChanged(nameof(UnseenPathtoHeide));
        //    OnPropertyChanged(nameof(ExileHoldingCells));
        //    OnPropertyChanged(nameof(McDuffsWorkshop));
        //    OnPropertyChanged(nameof(ServantsQuarters));
        //    OnPropertyChanged(nameof(StraidsCell));
        //    OnPropertyChanged(nameof(TheTowerApart));
        //    OnPropertyChanged(nameof(TheSaltfort));
        //    OnPropertyChanged(nameof(UpperRamparts));
        //    OnPropertyChanged(nameof(UndeadRefuge));
        //    OnPropertyChanged(nameof(BridgeApproach));
        //    OnPropertyChanged(nameof(UndeadLockaway));
        //    OnPropertyChanged(nameof(UndeadPurgatory));
        //    OnPropertyChanged(nameof(PoisonPool));
        //    OnPropertyChanged(nameof(TheMines));
        //    OnPropertyChanged(nameof(LowerEarthenPeak));
        //    OnPropertyChanged(nameof(CentralEarthenPeak));
        //    OnPropertyChanged(nameof(UpperEarthenPeak));
        //    OnPropertyChanged(nameof(ThresholdBridge));
        //    OnPropertyChanged(nameof(IronhearthHall));
        //    OnPropertyChanged(nameof(EygilsIdol));
        //    OnPropertyChanged(nameof(BelfrySolApproach));
        //    OnPropertyChanged(nameof(OldAkelarre));
        //    OnPropertyChanged(nameof(RuinedForkRoad));
        //    OnPropertyChanged(nameof(ShadedRuins));
        //    OnPropertyChanged(nameof(GyrmsRespite));
        //    OnPropertyChanged(nameof(OrdealsEnd));
        //    OnPropertyChanged(nameof(RoyalArmyCampsite));
        //    OnPropertyChanged(nameof(ChapelThreshold));
        //    OnPropertyChanged(nameof(LowerBrightstoneCove));
        //    OnPropertyChanged(nameof(HarvalsRestingPlace));
        //    OnPropertyChanged(nameof(GraveEntrance));
        //    OnPropertyChanged(nameof(UpperGutter));
        //    OnPropertyChanged(nameof(CentralGutter));
        //    OnPropertyChanged(nameof(HiddenChamber));
        //    OnPropertyChanged(nameof(BlackGulchMouth));
        //    OnPropertyChanged(nameof(KingsGate));
        //    OnPropertyChanged(nameof(UnderCastleDrangleic));
        //    OnPropertyChanged(nameof(ForgottenChamber));
        //    OnPropertyChanged(nameof(CentralCastleDrangleic));
        //    OnPropertyChanged(nameof(TowerOfPrayerAmana));
        //    OnPropertyChanged(nameof(CrumbledRuins));
        //    OnPropertyChanged(nameof(RhoysRestingPlace));
        //    OnPropertyChanged(nameof(RiseOfTheDead));
        //    OnPropertyChanged(nameof(UndeadCryptEntrance));
        //    OnPropertyChanged(nameof(UndeadDitch));
        //    OnPropertyChanged(nameof(Foregarden));
        //    OnPropertyChanged(nameof(RitualSite));
        //    OnPropertyChanged(nameof(DragonAerie));
        //    OnPropertyChanged(nameof(ShrineEntrance));
        //    OnPropertyChanged(nameof(SanctumWalk));
        //    OnPropertyChanged(nameof(PriestessChamber));
        //    OnPropertyChanged(nameof(HiddenSanctumChamber));
        //    OnPropertyChanged(nameof(LairOfTheImperfect));
        //    OnPropertyChanged(nameof(SanctumInterior));
        //    OnPropertyChanged(nameof(TowerOfPrayerAmana));
        //    OnPropertyChanged(nameof(SanctumNadir));
        //    OnPropertyChanged(nameof(ThroneFloor));
        //    OnPropertyChanged(nameof(UpperFloor));
        //    OnPropertyChanged(nameof(Foyer));
        //    OnPropertyChanged(nameof(LowermostFloor));
        //    OnPropertyChanged(nameof(TheSmelterThrone));
        //    OnPropertyChanged(nameof(IronHallwayEntrance));
        //    OnPropertyChanged(nameof(OuterWall));
        //    OnPropertyChanged(nameof(AbandonedDwelling));
        //    OnPropertyChanged(nameof(ExpulsionChamber));
        //    OnPropertyChanged(nameof(InnerWall));
        //    OnPropertyChanged(nameof(LowerGarrison));
        //    OnPropertyChanged(nameof(GrandCathedral));
        //}


        public void UpdateCovenantProperties()
        {
            DS2P.CovenantHGO.UpdateProperties();
            //OnPropertyChanged(nameof(CurrentCovenant));
        }
        public void UpdateInternalProperties()
        {
            //OnPropertyChanged(nameof(Head));
            //OnPropertyChanged(nameof(Chest));
            //OnPropertyChanged(nameof(Arms));
            //OnPropertyChanged(nameof(Legs));
            //OnPropertyChanged(nameof(RightHand1));
            //OnPropertyChanged(nameof(RightHand2));
            //OnPropertyChanged(nameof(RightHand3));
            //OnPropertyChanged(nameof(LeftHand1));
            //OnPropertyChanged(nameof(LeftHand2));
            //OnPropertyChanged(nameof(LeftHand3));
        }


        public void UnlockBonfires()
        {
            foreach (var bf in DS2Resource.Bonfires.Where(bf => bf.ID != DS2SBonfire._GameStartId))
                DS2P.BonfiresHGO.SetBonfireLevelById(bf.ID, 1);
            

            //var BFlvls = Offsets.BonfireLevels;
            //PropertyInfo[] props = typeof(BonfireLevels).GetProperties();
            //foreach (var p in props)
            //{
            //    var fval = (int?)p?.GetValue(BFlvls);

            //    if (fval == null)
            //        continue;
            //    int bfoffset = (int)fval;
            //    if (bfoffset == DS2HookOffsets.UNSET)
            //        continue;

            //    var currentLevel = BonfireLevels.ReadByte(bfoffset);
            //    if (currentLevel == 0)
            //        BonfireLevels.WriteByte(bfoffset, 1);
            //}
        }
        internal bool WarpLast()
        {
            // TO TIDY with bonfire objects

            // Handle betwixt start warps:
            bool PrevBonfireSet = DS2P.BonfiresHGO.LastBonfireAreaID != 0 && DS2P.BonfiresHGO.LastBonfireID != 0;
            if (PrevBonfireSet)
                return Warp((ushort)DS2P.BonfiresHGO.LastBonfireID);

            // Handle first area warp:
            int BETWIXTAREA = 167903232;
            ushort BETWIXTBFID = 2650;
            DS2P.BonfiresHGO.LastBonfireAreaID = BETWIXTAREA;
            return Warp(BETWIXTBFID, true);
        }

        
        internal bool WarpBonfire(DS2SBonfire toBonfire, bool bWrongWarp, bool restAfterWarp) // Events one day surely
        {
            if (toBonfire == null) return false;
            DS2P.BonfiresHGO.LastBonfireAreaID = toBonfire.AreaID;
            var wopt = restAfterWarp ? WARPOPTIONS.WARPREST : WARPOPTIONS.WARPONLY;
            var bfid = toBonfire.ID != 0 ? toBonfire.ID : DS2Resource.GetBonfireByName("Fire Keepers' Dwelling").ID; // fix _Game Start
            return Warp(bfid, bWrongWarp, wopt);
        }
        internal bool Warp(ushort id, bool areadefault = false, WARPOPTIONS wopt = WARPOPTIONS.DEFAULT)
        {
            bool bsuccess;
            if (Is64Bit)
                bsuccess = ASM.Warp64(id, areadefault);
            else
                bsuccess = ASM.Warp32(id, areadefault);

            // Multiplayer mode cannot warp
            if (!bsuccess)
                return false;

            if (!Enum.IsDefined(typeof(WARPOPTIONS), wopt))
            {
                MetaExceptionStaticHandler.Raise($"Unexpected enum type for WARPOPTIONS. Value received: {wopt}");
                return false;
            }

            // Apply rest after warp
            bool do_rest = wopt switch
            {
                WARPOPTIONS.DEFAULT => Properties.Settings.Default.RestAfterWarp,
                WARPOPTIONS.WARPREST => true,
                WARPOPTIONS.WARPONLY => false,
                _ => throw new Exception("Impossible, thrown properly above")
            };
            if (do_rest)
                AwaitBonfireRest();
            return true;
        }
        //internal bool Warp64(ushort id, bool areadefault = false)
        //{
        //    // area default means warp to the 0,0 part of the map (like a wrong warp)
        //    // areadefault = false is a normal "warp to bonfire"
        //    int WARPAREADEFAULT = 2;
        //    int WARPBONFIRE = 3;

        //    var value = Allocate(sizeof(short));
        //    Kernel32.WriteBytes(Handle, value, BitConverter.GetBytes(id));

        //    var asm = (byte[])DS2SAssembly.BonfireWarp64.Clone();
        //    var bytes = BitConverter.GetBytes(value.ToInt64());
        //    Array.Copy(bytes, 0x0, asm, 0x9, bytes.Length);
        //    bytes = BitConverter.GetBytes(SetWarpTargetFunc.Resolve().ToInt64());
        //    Array.Copy(bytes, 0x0, asm, 0x21, bytes.Length);
        //    bytes = BitConverter.GetBytes(WarpManager.Resolve().ToInt64());
        //    Array.Copy(bytes, 0x0, asm, 0x2E, bytes.Length);
        //    bytes = BitConverter.GetBytes(WarpFunc.Resolve().ToInt64());
        //    Array.Copy(bytes, 0x0, asm, 0x3B, bytes.Length);

        //    int flag = areadefault ? WARPAREADEFAULT : WARPBONFIRE;
        //    bytes = BitConverter.GetBytes(flag);
        //    Array.Copy(bytes, 0x0, asm, 0x45, bytes.Length);

        //    var warped = false;
        //    if (!Multiplayer)
        //    {
        //        Execute(asm);
        //        warped = true;
        //    }

        //    Free(value);
        //    return warped;
        //}
        //internal bool Warp32(ushort bfid, bool areadefault = false)
        //{
        //    // area default means warp to the 0,0 part of the map (like a wrong warp)
        //    // areadefault = false is a normal "warp to bonfire"
        //    int WARPAREADEFAULT = 2;
        //    int WARPBONFIRE = 3;
        //    int flag = areadefault ? WARPAREADEFAULT : WARPBONFIRE;

        //    // Get assembly template
        //    var asm = (byte[])DS2SAssembly.BonfireWarp32.Clone();

        //    // Get variables for byte changes
        //    var bfiD_bytes = BitConverter.GetBytes(bfid);
        //    var pWarpTargetFunc = BitConverter.GetBytes(SetWarpTargetFunc.Resolve().ToInt32()); // same as warpman?
        //    var warptypeflag = BitConverter.GetBytes(flag);
        //    var pBaseA = BitConverter.GetBytes(BaseA.Resolve().ToInt32());
        //    var pWarpFun = BitConverter.GetBytes(WarpFunc.Resolve().ToInt32());

        //    // Change bytes
        //    Array.Copy(bfiD_bytes, 0x0, asm, 0xB, bfiD_bytes.Length);
        //    Array.Copy(pWarpTargetFunc, 0x0, asm, 0x14, pWarpTargetFunc.Length);
        //    Array.Copy(warptypeflag, 0x0, asm, 0x1F, warptypeflag.Length);
        //    Array.Copy(pBaseA, 0x0, asm, 0x24, pBaseA.Length);
        //    Array.Copy(pWarpFun, 0x0, asm, 0x36, pWarpFun.Length);

        //    // Safety checks
        //    var warped = false;
        //    if (Multiplayer)
        //        return warped; // No warping in multiplayer!

        //    // Execute:
        //    Execute(asm);
        //    warped = true;
        //    return warped;
        //}

        public void UninstallBIKP1Skip()
        {
            //if (InstalledInjects.Contains(InstallInject))
            BIKP1Skip(false, false);
        } 

        //private bool BIKSkipInstalled
        internal bool BIKP1Skip(bool enable, bool doLoad)
        {
            if (!Hooked) return false;
            //ASM.ApplyBIKP1Skip(enable);
            
            if (doLoad)
                WarpLast(); // force a reload to fix some memes; only on first click
            return enable; // turned on or off now
        }
        //internal bool BIKP1Skip(bool enable, bool doLoad)
        //{
        //    if (!MetaFeature.FtBIKP1Skip)
        //        return false;
        //    if (!Hooked) return false;

        //    // Change some constants read by the BIK fight I guess.
        //    // Carbon copy from https://www.nexusmods.com/darksouls2/mods/1043 .
        //    // Haven't bothered to figure out how it works.
        //    byte[] DISABLEMOD = new byte[2] { 0x0, 0x0 };
        //    byte[] ENABLEMOD_VAL1 = new byte[2] { 0x80, 0x9c };
        //    byte[] ENABLEMOD_VAL2 = new byte[2] { 0x0e, 0x3c };
        //    var val1_bytes = enable ? ENABLEMOD_VAL1 : DISABLEMOD;
        //    var val2_bytes = enable ? ENABLEMOD_VAL2 : DISABLEMOD;

        //    // enable/disable phase1
        //    if (Offsets.BIKP1Skip_Val1 == null || Offsets.BIKP1Skip_Val2 == null)
        //        throw new Exception("Shouldn't get here. Handle via feature enable logic.");
        //    phBIKP1SkipVals.WriteBytes(Offsets.BIKP1Skip_Val1[^1], val1_bytes);
        //    phBIKP1SkipVals.WriteBytes(Offsets.BIKP1Skip_Val2[^1], val2_bytes);
        //    if (doLoad)
        //        WarpLast(); // force a reload to fix some memes; only on first click
        //    return enable; // turned on or off now
        //}



        internal void RestoreHumanity()
        {
            ApplySpecialEffect((int)SPECIAL_EFFECT.RESTOREHUMANITY);
        }
        internal async void AwaitBonfireRest()
        {
            // This is useful to ensure you're at full hp
            // after a load, so that things like lifering+3
            // effects are accounted for before healing

            bool finishedload = await NextLoadComplete();
            if (!finishedload)
                return; // timeout issue

            // Apply bonfire rest in non-loadscreen
            BonfireRest();
        }
        internal async Task<bool> NextLoadComplete()
        {
            Stopwatch sw = Stopwatch.StartNew();
            int loadingTimeout_ms = 15000;
            int dlay = 10;

            // wait for start of load
            while (!IsLoading)
            {
                await Task.Delay(dlay);
                if (sw.ElapsedMilliseconds > loadingTimeout_ms)
                    return false;
            }

            // Now its loading, wait for it to finish:
            while (IsLoading)
            {
                await Task.Delay(dlay);
                if (sw.ElapsedMilliseconds > loadingTimeout_ms)
                    return false;
            }
            return true;

        }
        internal void BonfireRest()
        {
            ApplySpecialEffect((int)SPECIAL_EFFECT.BONFIREREST);
        }
        internal void ApplySpecialEffect(int spEffect)
        {
            if (Is64Bit)
                ASM.ApplySpecialEffect64(spEffect);
            else if (IsOldPatch)
                ASM.ApplySpecialEffect32OP(spEffect);
            else
                ASM.ApplySpecialEffect32(spEffect);
        }
        //internal void ApplySpecialEffect64(int spEffect)
        //{
        //    // Last resort graceful failure
        //    if (DS2P.Func.ApplySpEffect == null)
        //        throw new MetaFeatureException("ApplySpecialEffect64.ApplySpEffect");
        //    if (DS2P.MiscPtrs.SpEffectCtrl == null)
        //        throw new MetaFeatureException("ApplySpecialEffect64.SpEffectCtrl");

        //    // Get assembly template
        //    var asm = (byte[])DS2SAssembly.ApplySpecialEffect64.Clone();

        //    // Prepare inputs:
        //    var effectStruct = Allocate(0x16);
        //    Kernel32.WriteBytes(Handle, effectStruct, BitConverter.GetBytes(spEffect));
        //    Kernel32.WriteBytes(Handle, effectStruct + 0x4, BitConverter.GetBytes(0x1));
        //    Kernel32.WriteBytes(Handle, effectStruct + 0xC, BitConverter.GetBytes(0x219));


        //    var unk = Allocate(sizeof(float));
        //    Kernel32.WriteBytes(Handle, unk, BitConverter.GetBytes(-1f));
        //    var float_m1 = BitConverter.GetBytes(unk.ToInt64());
        //    Array.Copy(float_m1, 0x0, asm, 0x1A, float_m1.Length);

        //    var ptrEffectStruct = BitConverter.GetBytes(effectStruct.ToInt64());
        //    var SpEfCtrl = BitConverter.GetBytes(DS2P.MiscPtrs.SpEffectCtrl.Resolve().ToInt64());
        //    var ptrApplySpEf = BitConverter.GetBytes(DS2P.Func.ApplySpEffect.Resolve().ToInt64());

        //    // Update assembly with variables:
        //    Array.Copy(ptrEffectStruct, 0x0, asm, 0x6, ptrEffectStruct.Length);
        //    Array.Copy(SpEfCtrl, 0x0, asm, 0x10, SpEfCtrl.Length);
        //    Array.Copy(ptrApplySpEf, 0x0, asm, 0x2E, ptrApplySpEf.Length);

        //    // Run and tidy-up
        //    Execute(asm);
        //    Free(effectStruct);
        //    Free(unk);
        //}

        //internal void ApplySpecialEffect32(int spEffectID)
        //{
        //    // Last resort graceful failure
        //    if (DS2P.Func.ApplySpEffect == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32CP.ApplySpEffect");
        //    if (DS2P.MiscPtrs.SpEffectCtrl == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32CP.SpEffectCtrl");

        //    // Assembly template
        //    var asm = (byte[])DS2SAssembly.ApplySpecialEffect32.Clone();

        //    //var ptrEffectStruct = BitConverter.GetBytes(effectStruct.ToInt32());
        //    var spEfId = BitConverter.GetBytes(spEffectID);
        //    var ptrApplySpEf = BitConverter.GetBytes(DS2P.Func.ApplySpEffect.Resolve().ToInt32());
        //    var SpEfCtrl = BitConverter.GetBytes(DS2P.MiscPtrs.SpEffectCtrl.Resolve().ToInt32());

        //    var unk = Allocate(sizeof(float));
        //    Kernel32.WriteBytes(Handle, unk, BitConverter.GetBytes(-1f));
        //    var addr_float_m1 = BitConverter.GetBytes(unk.ToInt32());


        //    // Update assembly with variables:
        //    Array.Copy(spEfId, 0x0, asm, 0x9, spEfId.Length);
        //    Array.Copy(addr_float_m1, 0x0, asm, 0x16, addr_float_m1.Length);
        //    Array.Copy(SpEfCtrl, 0x0, asm, 0x33, SpEfCtrl.Length);
        //    Array.Copy(ptrApplySpEf, 0x0, asm, 0x38, ptrApplySpEf.Length);

        //    // Run and tidy-up
        //    Execute(asm);
        //    Free(unk);
        //}

        //internal void ApplySpecialEffect32OP(int spEffectID)
        //{
        //    // Last resort graceful failure
        //    if (DS2P.Func.ApplySpEffect == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.ApplySpEffect");
        //    if (DS2P.MiscPtrs.SpEffectCtrl == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.SpEffectCtrl");
        //    if (DS2P.Core.BaseA == null)
        //        throw new MetaFeatureException("ApplySpecialEffect32OldPatch.BaseA");

        //    // Assembly template
        //    var asm = (byte[])DS2SAssembly.ApplySpecialEffect32OP.Clone();

        //    //var ptrEffectStruct = BitConverter.GetBytes(effectStruct.ToInt32());
        //    var spEfId = BitConverter.GetBytes(spEffectID);
        //    var ptrApplySpEf = BitConverter.GetBytes(DS2P.Func.ApplySpEffect.Resolve().ToInt32());
        //    var SpEfCtrl = BitConverter.GetBytes(DS2P.MiscPtrs.SpEffectCtrl.Resolve().ToInt32());
        //    var pbasea = BitConverter.GetBytes(DS2P.Core.BaseA.Resolve().ToInt32());


        //    var unk = Allocate(sizeof(float));
        //    Kernel32.WriteBytes(Handle, unk, BitConverter.GetBytes(-1f));
        //    var addr_float_m1 = BitConverter.GetBytes(unk.ToInt32());

        //    // Update assembly with variables:
        //    // Adjust esp offsets?
        //    int z = 0x8;
        //    byte[] Z = new byte[1] { (byte)z };
        //    byte[] Z4 = new byte[1] { (byte)(z + 4) };
        //    byte[] Z8 = new byte[1] { (byte)(z + 8) };
        //    byte[] ZC = new byte[1] { (byte)(z + 0xC) };
        //    Array.Copy(Z, 0x0, asm, 0x8, Z.Length);
        //    Array.Copy(Z4, 0x0, asm, 0x10, Z4.Length);
        //    Array.Copy(Z8, 0x0, asm, 0x23, Z8.Length);
        //    Array.Copy(ZC, 0x0, asm, 0x27, ZC.Length);
        //    Array.Copy(Z, 0x0, asm, 0x2F, Z.Length); // &struct


        //    Array.Copy(spEfId, 0x0, asm, 0x9, spEfId.Length);
        //    Array.Copy(addr_float_m1, 0x0, asm, 0x16, addr_float_m1.Length);
        //    Array.Copy(SpEfCtrl, 0x0, asm, 0x34, SpEfCtrl.Length);
        //    Array.Copy(ptrApplySpEf, 0x0, asm, 0x39, ptrApplySpEf.Length);

        //    // Run and tidy-up
        //    Execute(asm);
        //    Free(unk);
        //}

        internal void AddSouls(int numsouls)
        {
            if (Is64Bit)
                ASM.UpdateSoulCount64(numsouls);
            else
                ASM.UpdateSoulCount32(numsouls);

        }
        //public void UpdateSoulCount64(int souls)
        //{
        //    // Check both as they all come through here
        //    if (DS2P.Func.GiveSouls == null)
        //        throw new MetaFeatureException("GiveSouls64");
        //    if (DS2P.Func.RemoveSouls == null)
        //        throw new MetaFeatureException("RemoveSouls64");

        //    // Assembly template
        //    var asm = (byte[])DS2SAssembly.AddSouls64.Clone();

        //    // Setup dynamic vars:
        //    var playerParam = BitConverter.GetBytes(PlayerParam.Resolve().ToInt64());
        //    GetAddRemSoulFunc(souls, out byte[] numSouls, out byte[] funcChangeSouls);

        //    // Update assembly & execute:
        //    Array.Copy(playerParam, 0, asm, 0x6, playerParam.Length);
        //    Array.Copy(numSouls, 0, asm, 0x11, numSouls.Length);
        //    Array.Copy(funcChangeSouls, 0, asm, 0x17, funcChangeSouls.Length);
        //    Execute(asm);
        //}
        //public void UpdateSoulCount32(int souls_input)
        //{
        //    // Check both as they all come through here
        //    if (DS2P.Func.GiveSouls == null)
        //        throw new MetaFeatureException("GiveSouls32");
        //    if (DS2P.Func.RemoveSouls == null)
        //        throw new MetaFeatureException("RemoveSouls32");

        //    // Assembly template
        //    var asm = (byte[])DS2SAssembly.AddSouls32.Clone();

        //    // Setup dynamic vars:
        //    var playerParam = BitConverter.GetBytes(PlayerParam.Resolve().ToInt32());
        //    GetAddRemSoulFunc(souls_input, out byte[] numSouls, out byte[] funcChangeSouls);

        //    // Update assembly & execute:
        //    Array.Copy(playerParam, 0, asm, 0x4, playerParam.Length);
        //    Array.Copy(numSouls, 0, asm, 0x9, numSouls.Length);
        //    Array.Copy(funcChangeSouls, 0, asm, 0xF, funcChangeSouls.Length);
        //    Execute(asm);
        //}
        //private void GetAddRemSoulFunc(int inSouls, out byte[] outSouls, out byte[] funcChangeSouls)
        //{
        //    bool isAdd = inSouls >= 0;
        //    if (isAdd)
        //    {
        //        outSouls = BitConverter.GetBytes(inSouls); // AddSouls
        //        if (Is64Bit)
        //            funcChangeSouls = BitConverter.GetBytes(DS2P.Func.GiveSouls!.Resolve().ToInt64());
        //        else
        //            funcChangeSouls = BitConverter.GetBytes(DS2P.Func.GiveSouls!.Resolve().ToInt32());
        //    }
        //    else
        //    {
        //        int new_souls = -inSouls; // needs to "subtract a positive number"
        //        outSouls = BitConverter.GetBytes(new_souls); // SubractSouls
        //        if (Is64Bit)
        //            funcChangeSouls = BitConverter.GetBytes(DS2P.Func.RemoveSouls!.Resolve().ToInt64());
        //        else
        //            funcChangeSouls = BitConverter.GetBytes(DS2P.Func.RemoveSouls!.Resolve().ToInt32());
        //    }
        //}

        //private void UpdateSoulLevel()
        //{
        //    var charClass = DS2Resource.Classes.FirstOrDefault(c => c.ID == Class);
        //    if (charClass == null) return;

        //    var soulLevel = GetSoulLevel(charClass);
        //    SoulLevel = soulLevel;
        //    var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);
        //    if (reqSoulMemory > SoulMemory)
        //    {
        //        SoulMemory = reqSoulMemory;
        //        SoulMemory2 = reqSoulMemory;
        //    }
        //}
        private int GetSoulLevel(DS2SClass charClass)
        {
            int sl = charClass.SoulLevel;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.VGR) - charClass.Vigor;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.ATN) - charClass.Attunement;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.VIT) - charClass.Vitality;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.END) - charClass.Endurance;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.STR) - charClass.Strength;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.DEX) - charClass.Dexterity;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.ADP) - charClass.Adaptability;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.INT) - charClass.Intelligence;
            sl += DS2P.PlayerData.GetAttributeLevel(ATTR.FTH) - charClass.Faith;
            return sl;
        }
        public void ResetSoulMemory()
        {
            var charClass = DS2Resource.Classes.FirstOrDefault(c => c.ID == DS2P.PlayerData.Class);
            if (charClass == null) return;

            var soulLevel = GetSoulLevel(charClass);
            var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);

            DS2P.PlayerData.SoulMemory = reqSoulMemory;
            DS2P.PlayerData.SoulMemory2 = reqSoulMemory;
        }
        private int GetRequiredSoulMemory(int SL, int baseSL)
        {
            int soulMemory = 0;
            for (int i = baseSL; i < SL; i++)
            {
                var index = i <= 850 ? i : 850;
                soulMemory += Levels[index];
            }
            return soulMemory;
        }
        private static List<int> Levels = new();
        private void GetLevelRequirements()
        {
            if (ParamMan.PlayerLevelUpSoulsParam == null)
                throw new NullReferenceException("Level up cost param not found");

            foreach (var row in ParamMan.PlayerLevelUpSoulsParam.Rows.Cast<PlayerLevelUpSoulsRow>())
                Levels.Add(row.LevelCost);
        }



        // TODO put somewhere more meaningful
        public enum GIVEOPTIONS
        {
            DEFAULT,
            SHOWDIALOG,
            GIVESILENTLY,
        }

        private void EnsureInstalledNoDmgMod()
        {
            if (NoDmgMod?.IsInstalled == true)
                return;
            NoDmgMod ??= new NoDmgMod(this);
            NoDmgMod.Install();
        }
        public void GeneralizedDmgMod(bool dealFullDmg, bool dealNoDmg, bool recvNoDmg)
        {
            // catch awkward logical bugs
            if (dealFullDmg && dealNoDmg) throw new MetaLogicException("Cannot do zero and full damage together. Should be handled in ViewModel.");

            bool affectDealtDmg = dealFullDmg || dealNoDmg;
            bool affectRcvdDmg = recvNoDmg;
            int HIGHDMG = 0x4b18967f; // float 9999999.0
            var dmgfacDealt = dealFullDmg ? HIGHDMG : 0; // irrelevant if affectDealtDmg == false
            int dmgfacRcvd = recvNoDmg ? 0 : 1;

            EnsureInstalledNoDmgMod();
            NoDmgMod?.SetDmgModSettings(affectDealtDmg, affectRcvdDmg, dmgfacDealt, dmgfacRcvd);
        }
        //private bool InstallDmgMod(bool affectDealtDmg, bool affectRecvDmg, int dmgFactorDealt, int dmgFactorRecvd)
        //{
            //throw new NotImplementedException("should be in its own file now");
            //if (MetaFeature.IsInactive(METAFEATURE.DMGMOD))
            //    return false;

            //if (DmgModInstalled)
            //    UninstallDmgMod(); // start from a fresh inject incase settings change

            //// Setup addresses for my new code:
            //var memalloc = Allocate(0x1000, flProtect: Kernel32.PAGE_EXECUTE_READWRITE); // host the assembly in memory
            //var p_code2st = IntPtr.Add(memalloc, 0x0);        // first thing
            //var p_code1st = p_code2st + 0x7e;                 // see assembly script
            //var module_addr = Process?.MainModule?.BaseAddress;
            //if (module_addr == null)
            //{
            //    MetaExceptionStaticHandler.Raise("Cannot find DS2 Module address for hooks/inject calculations");
            //    return false;
            //}

            //// First inject (amBeingHit)
            //var inj1_offset = 0x17aa65; // inject for figuring out if being hit [todo load from SOTFS_v1.03 offsets]
            //var p_inj1 = IntPtr.Add((IntPtr)module_addr, inj1_offset);
            //var inj1_ob = new byte[] { 0x48, 0x89, 0x44, 0x24, 0x28, 0x48, 0x8b, 0x44, 0x24, 0x60, 0x48, 0x89, 0x44, 0x24, 0x20 };

            
            //byte[] inj1_nb_st = new byte[] { 0x49, 0xbb, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x41, 0xff, 0xe3 }; // mov r11 ADDR; jmp r11
            //var inj1_nb = inj1_nb_st.NopExtend(inj1_ob.Length);
            

            //// fix and install
            //var inj1code_bytes = BitConverter.GetBytes(p_code1st.ToInt64());    // overwrite FFFFFFFF 00000000
            //Array.Copy(inj1code_bytes, 0x0, inj1_nb, 0x2, inj1code_bytes.Length);
            //var inj1 = new Inject(p_inj1, inj1_ob, inj1_nb);
            //if (!inj1.Valid) return false;

            //// Second inject (dmgCalculation)
            //var inj2_offset = 0x138F77; // todo as above
            //var p_inj2 = IntPtr.Add((IntPtr)module_addr, inj2_offset);
            //var inj2_ob = new byte[] { 0x49, 0x8b, 0x46, 0x08, 0xf3, 0x41, 0x0f, 0x5e, 0xf1, 0xf3, 0x0f, 0x59, 0x70, 0x1c };

            //var inj2len = 0xe;
            //var inj2_nb = Enumerable.Repeat(Inject.NOP, inj2len).ToArray();            // prefill as nops
            //var hdrinj2bytes = new byte[2] { 0x48, 0xb8 };                      // movabs rax, x
            //var inj2code_bytes = BitConverter.GetBytes(p_code2st.ToInt64());    // FFFFFFFF 00000000
            //var inj2_jbytes = new byte[2] { 0xff, 0xe0 };                       // jmp rax

            //// build & install inj2
            //Array.Copy(hdrinj2bytes, 0x0, inj2_nb, 0x0, hdrinj2bytes.Length);
            //Array.Copy(inj2code_bytes, 0x0, inj2_nb, 0x2, inj2code_bytes.Length);
            //Array.Copy(inj2_jbytes, 0x0, inj2_nb, 0xa, inj2_jbytes.Length);
            //var inj2 = new Inject(p_inj2, inj2_ob, inj2_nb);
            //if (!inj2.Valid) return false;

            //// Prep assembly substitutions
            //var MEMSTORE_OFFSET = 0x100;
            //var amDealingHit = IntPtr.Add(p_code2st, MEMSTORE_OFFSET);
            //var enDealNoDmg = IntPtr.Add(p_code2st, MEMSTORE_OFFSET + 0x8);
            //var enTakeNoDmg = IntPtr.Add(p_code2st, MEMSTORE_OFFSET + 0x10);
            //var amDealingHit_bytes = BitConverter.GetBytes(amDealingHit.ToInt64());
            //var enDealNoDmg_bytes = BitConverter.GetBytes(enDealNoDmg.ToInt64());
            //var enTakeNoDmg_bytes = BitConverter.GetBytes(enTakeNoDmg.ToInt64());
            //var inj1ret_bytes = BitConverter.GetBytes(inj1.RetAddr.ToInt64());
            //var inj2ret_bytes = BitConverter.GetBytes(inj2.RetAddr.ToInt64());
            //var dmgfacDealt_bytes = BitConverter.GetBytes(dmgFactorDealt);
            //var dmgfacRecvd_bytes = BitConverter.GetBytes(dmgFactorRecvd);


            //// Clone reference assembly and populate links
            //var asm = (byte[])DS2SAssembly.NoDmgMod.Clone();
            //Array.Copy(amDealingHit_bytes, 0, asm, 0x10, amDealingHit_bytes.Length);
            //Array.Copy(enDealNoDmg_bytes, 0, asm, 0x23, enDealNoDmg_bytes.Length);
            //Array.Copy(enTakeNoDmg_bytes, 0, asm, 0x46, enTakeNoDmg_bytes.Length);
            //Array.Copy(amDealingHit_bytes, 0, asm, 0x64, amDealingHit_bytes.Length);
            //Array.Copy(inj2ret_bytes, 0, asm, 0x74, inj2ret_bytes.Length);
            //Array.Copy(amDealingHit_bytes, 0, asm, 0x8f, amDealingHit_bytes.Length);
            //Array.Copy(inj1ret_bytes, 0, asm, 0x9f, inj1ret_bytes.Length);
            //Array.Copy(dmgfacDealt_bytes, 0, asm, 0x35, dmgfacDealt_bytes.Length); // dealt dmgfactor if enabled
            //Array.Copy(dmgfacRecvd_bytes, 0, asm, 0x58, dmgfacRecvd_bytes.Length); // recv dmgfactor if enabled


            //// Write machine code into memory:
            //inj1.Install();
            //inj2.Install();
            ////InstallInject(inj1);
            ////InstallInject(inj2);
            //Kernel32.WriteBytes(Handle, p_code2st, asm); // install dmgmod code

            //// Populate settings:
            //byte dealNoDmg_byte = affectDealtDmg ? (byte)1 : (byte)0;
            //byte takeNoDmg_byte = affectRecvDmg ? (byte)1 : (byte)0;
            //Kernel32.WriteByte(Handle, enDealNoDmg, dealNoDmg_byte);
            //Kernel32.WriteByte(Handle, enTakeNoDmg, takeNoDmg_byte);

            //// done
            //DmgModInj1 = inj1;
            //DmgModInj2 = inj2;
            //DmgModCodeAddr = memalloc;
            //return true; // success
        //}
        public void UninstallDmgMod() => NoDmgMod?.Uninstall();
        
        // QoL Wrappers:
        public void GiveItem(ITEMID itemid, short amount = 1, byte upgrade = 0, byte infusion = 0,
                             GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            GiveItem((int)itemid, amount, upgrade, infusion, opt);
        }
        public void GiveItem(int itemid, short amount = 1,
                             byte upgrade = 0, byte infusion = 0,
                             GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            // Simple wrapper for programmer QoL
            var ids = new int[1] { itemid };
            var amounts = new short[1] { amount };
            var upgrades = new byte[1] { upgrade };
            var infusions = new byte[1] { infusion };
            GiveItems(ids, amounts, upgrades, infusions, opt); // call actual implementation
        }
        public void GiveItems(int[] itemids, short[] amounts, GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            // Fix wrapping for optionals
            var len = itemids.Length;
            var upgrades_list = new List<byte>(len);
            var infusions_list = new List<byte>(len);
            for (int i = 0; i < len; i++)
            {
                upgrades_list.Add(0);
                infusions_list.Add(0);
            }

            byte[] upgrades = upgrades_list.ToArray();
            byte[] infusions = infusions_list.ToArray();

            // Call function
            GiveItems(itemids, amounts, upgrades, infusions, opt);
        }

        // Main outward facing interface wrapper
        public void GiveItems(int[] itemids, short[] amounts, byte[] upgrades, byte[] infusions, GIVEOPTIONS opt = GIVEOPTIONS.DEFAULT)
        {
            var showdialog = opt switch
            {
                GIVEOPTIONS.DEFAULT => !Properties.Settings.Default.SilentItemGive,
                GIVEOPTIONS.SHOWDIALOG => true,
                GIVEOPTIONS.GIVESILENTLY => false,
                _ => throw new Exception("Unexpected flag for Silent Item switch")
            };

            // call actual assembly function:
            if (Is64Bit)
                ASM.GiveItems64(itemids, amounts, upgrades, infusions, showdialog);
            else
                ASM.GiveItems32(itemids, amounts, upgrades, infusions, showdialog);
        }

        


        public void Give17kReward()
        {
            // Add Soul of Pursuer x1 Ring of Blades x1 / 
            var itemids = new int[2] { 0x03D09000, 0x0264CB00 };
            var amounts = new short[2] { 1, 1 };
            GiveItems(itemids, amounts);
            AddSouls(17001);
        }
        public void Give3Chunk1Slab()
        {
            // For the lizard in dlc2
            var items = new ITEMID[2] { ITEMID.TITANITECHUNK, ITEMID.TITANITESLAB };
            var itemids = items.Cast<int>().ToArray();
            var amounts = new short[2] { 3, 1 };
            GiveItems(itemids, amounts);
        }

        public void SetMaxLevels()
        {
            // Possibly to tidy
            DS2P.PlayerData.SetAttributeLevel(ATTR.VGR,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.END,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.VIT,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.ATN,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.STR,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.DEX,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.ADP,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.INT,99);
            DS2P.PlayerData.SetAttributeLevel(ATTR.FTH,99);
        }
        public void NewTestCharacter()
        {
            // shorthand
            GIVEOPTIONS GIVESILENT = GIVEOPTIONS.GIVESILENTLY;

            // Consumables / Multi-items
            var multi_items = new List<ITEMID>() 
                { ITEMID.LIFEGEM, ITEMID.OLDRADIANT, ITEMID.MUSHROOM, ITEMID.DIVINEBLESSING, ITEMID.HUMANEFFIGY,
                  ITEMID.POISONMOSS, ITEMID.WILTEDDUSKHERB, ITEMID.AROMATICOOZE, ITEMID.GOLDPINERESIN, ITEMID.DARKPINERESIN,
                  ITEMID.AGEDFEATHER, ITEMID.FRAGRANTBRANCH, ITEMID.WITCHINGURN, ITEMID.FIREBOMB, ITEMID.BLACKFIREBOMB,
                  ITEMID.DUNGPIE, ITEMID.POISONTHROWINGKNIFE, ITEMID.SOULOFAGREATHERO, ITEMID.OLDDEADONESOUL,
                  ITEMID.ALLURINGSKULL, ITEMID.TORCH, ITEMID.TITANITESHARD, ITEMID.LARGETITANITESHARD,
                  ITEMID.TITANITESLAB, ITEMID.TWINKLINGTITANITE, ITEMID.PETRIFIEDDRAGONBONE, ITEMID.BOLTSTONE,
                  ITEMID.DARKNIGHTSTONE, ITEMID.RAWSTONE, ITEMID.PALESTONE, ITEMID.PHARROSLOCKSTONE,
                  ITEMID.BRIGHTBUG, ITEMID.ASCETIC};
            foreach (int id in multi_items.Cast<int>())
                GiveItem(id, 95, 0, 0, GIVESILENT);

            // Ammo
            var ammo_items = new List<ITEMID>()
                { ITEMID.WOODARROW, ITEMID.IRONARROW, ITEMID.MAGICARROW, ITEMID.FIREARROW, 
                  ITEMID.POISONARROW, ITEMID.HEAVYBOLT };
            foreach (int id in ammo_items.Cast<int>())
                GiveItem(id, 950, 0, 0, GIVESILENT);

            // Common Gear:
            var single_items = new List<ITEMID>() 
                { ITEMID.ESTUS, ITEMID.BINOCULARS, ITEMID.BUCKLER, ITEMID.GOLDENFALCONSHIELD, ITEMID.IRONPARMA,
                  ITEMID.CHLORANTHYRING1, ITEMID.RINGOFBLADES, ITEMID.CATRING, ITEMID.SILVERSERPENTRING1, ITEMID.FLYNNSRING,
                  ITEMID.SORCERERSSTAFF, ITEMID.CLERICSACREDCHIME, ITEMID.PYROFLAME, ITEMID.DARKWEAPON, ITEMID.SUNLIGHTBLADE,
                  ITEMID.BUTTERFLYSKIRT, ITEMID.BUTTERFLYWINGS, ITEMID.TSELDORAHAT, ITEMID.TSELDORAROBE,
                  ITEMID.TSELDORAGLOVES, ITEMID.TSELDORAPANTS};
            foreach (int id in single_items.Cast<int>())
                GiveItem(id, 1, 0, 0, GIVESILENT);

            // Upgraded testing weapons:
            var upgr_weapons = new List<ITEMID>() 
                { ITEMID.DAGGER, ITEMID.RAPIER, ITEMID.MACE, ITEMID.SHORTBOW, ITEMID.LIGHTCROSSBOW, ITEMID.UCHI };
            foreach (int id in upgr_weapons.Cast<int>())
                GiveItem(id, 1, 10, 0, GIVESILENT);

            // Common speedrun weapons:
            GiveItem(ITEMID.RAPIER, 1, 0, 0, GIVESILENT);   // basic rapier
            GiveItem(ITEMID.RAPIER, 1, 10, 3, GIVESILENT);  // lightning rapier
            GiveItem(ITEMID.RAPIER, 1, 10, 4, GIVESILENT);  // dark rapier
            GiveItem(ITEMID.REDIRONTWINBLADE, 1, 10, 3, GIVESILENT);    // lightning RITB
            GiveItem(ITEMID.DBGS, 1, 5, 0, GIVESILENT);
            
            // Keys
            var keyIds = ParamMan.ItemRows?.Where(ir => ir.ItemUsageID == (int)ITEMUSAGE.ITEMUSAGEKEY)
                       .Select(ir => ir.ItemID).ToList() ?? Enumerable.Empty<int>();
            foreach (var id in keyIds)
                GiveItem(id, 1, 0, 0, GIVESILENT);

            // Gestures:
            GiveItem(ITEMID.DECAPITATEGESTURE, 1, 0, 0); // show visibly

            // Used to create a character with commonly useful things
            RestoreHumanity();
            SetMaxLevels();
            AddSouls(9999999);
            UnlockBonfires();

            // to tidy:
            DS2SBonfire majula = new(168034304, 4650, "The Far Fire");
            DS2P.BonfiresHGO.LastBonfireID = majula.ID;
            DS2P.BonfiresHGO.LastBonfireAreaID = majula.AreaID;
            Warp(majula.ID, false, WARPOPTIONS.WARPREST);
        }

        //#endregion

        // Get info requests:
        internal int GetMaxUpgrade(ItemRow item)
        {
            if (!ParamMan.IsLoaded)
                return 0;

            // Until we figure out more about the internal params:
            if (item.ItemID == (int)ITEMID.BINOCULARS || item.ItemID == (int)ITEMID.KEYTOEMBEDDED)
                return 0;


            int? upgr;
            switch (item.ItemType)
            {
                case eItemType.WEAPON1:
                case eItemType.WEAPON2:
                    upgr = item.WeaponRow?.MaxUpgrade;
                    return upgr ?? 0;

                case eItemType.HEADARMOUR:
                case eItemType.LEGARMOUR:
                case eItemType.CHESTARMOUR:
                case eItemType.GAUNTLETS:
                    upgr = item.ArmorRow?.ArmorReinforceRow?.MaxReinforceLevel;
                    return upgr ?? 0;
                default:
                    return 0;
            }
        }
        internal int GetHeld(ItemRow itemrow)
        {
            switch (itemrow.ItemType)
            {
                //case eItemType.AMMO+
                // return GetHeldInInventoryUnstackable(item.ID); // TODO

                case eItemType.CONSUMABLE:
                    return GetHeldInInventoryStackable(itemrow.ID);

                default:
                    return 0;
            }
        }
        private int GetHeldInInventoryStackable(int id)
        {
            var inventorySlot = 0x30;
            var itemOffset = 0x0;
            var boxOffset = 0x4;
            var heldOffset = 0x8;
            var nextOffset = 0x10;

            if (DS2P.MiscPtrs.AvailableItemBag == null) return 0;
            var endPointer = DS2P.MiscPtrs.AvailableItemBag.ReadIntPtr(0x10).ToInt64();
            var bagSize = endPointer - DS2P.MiscPtrs.AvailableItemBag.Resolve().ToInt64();

            var inventory = DS2P.MiscPtrs.AvailableItemBag.ReadBytes(0x0, (uint)bagSize);

            while (inventorySlot < bagSize)
            {
                // Get next item in inventory
                var itemID = BitConverter.ToInt32(inventory, inventorySlot + itemOffset);

                if (itemID == id)
                {
                    var boxValue = BitConverter.ToInt32(inventory, inventorySlot + boxOffset);
                    var held = BitConverter.ToInt32(inventory, inventorySlot + heldOffset);

                    if (boxValue == 0)
                        return held;
                }
                inventorySlot += nextOffset;
            }

            return 0;
        }
        private int GetHeldInInventoryUnstackable(int id)
        {
            throw new NotImplementedException();
        }

        internal bool GetIsDroppable(ItemRow item)
        {
            if (!ParamMan.IsLoaded)
                return false;

            if (!IsInitialized || ParamMan.ItemUsageParam == null)
                return false;

            if (item == null)
                throw new Exception("Cannot find item for GetIsDroppable");

            bool? drp = item.ItemUsageRow?.IsDroppable;
            if (drp == null)
                return false;
            return (bool)drp;
        }

        //#region Internal
        //public byte FastQuit
        //{
        //    set => BaseA.WriteByte(Offsets.ForceQuit.Quit, value);
        //}
        //internal byte DisableAI
        //{
        //    get => InGame && Offsets.DisableAI != null ? phDisableAI.ReadByte(Offsets.DisableAI[^1]) : (byte)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        if (Offsets.DisableAI == null) return;
        //        phDisableAI.WriteByte(Offsets.DisableAI[^1], value);
        //    }
        //}
        //public int Health
        //{
        //    get => InGame ? PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HP) : 0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HP, value);
        //    }
        //}

        // Feature Wrappers
        public void SetNoGravity(bool noGravity)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NOGRAVITY)) return;
            DS2P.CGS.Gravity = !noGravity;
        }
        public void SetNoCollision(bool noCollision)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NOCOLLISION)) return;
            DS2P.CGS.Collision = !noCollision;
        }
        public void SetDisableAI(bool disableAI)
        {
            if (MetaFeature.IsInactive(METAFEATURE.DISABLEAI)) return;
            DS2P.CGS.DisableAI = disableAI;
        }
        public void SetNoDeath(bool noDeath)
        {
            if (MetaFeature.IsInactive(METAFEATURE.NODEATH)) return;
            DS2P.PlayerState.HealthMin = noDeath ? 1 : -99999;
        }
        public void SetRapierOHKO(bool ohko)
        {
            if (MetaFeature.IsInactive(METAFEATURE.OHKO_RAPIER)) return;
            SetWeaponOHKO(ITEMID.RAPIER, ohko);
        }
        public void SetFistOHKO(bool ohko)
        {
            if (MetaFeature.IsInactive(METAFEATURE.OHKO_FIST)) return;
            SetWeaponOHKO(ITEMID.FISTS, ohko);
        }
        private void SetWeaponOHKO(ITEMID wpn, bool ohko)
        {
            if (!Hooked) return;
            float DMGMOD = 50000;

            // Write to memory
            var wpnrow = ParamMan.WeaponParam?.Rows.First(r => r.ID == (int)wpn) as WeaponRow ?? throw new NullReferenceException();
            wpnrow.DamageMultiplier = ohko ? DMGMOD : 1;
            wpnrow.WriteRow();
        }


        //public int HealthMax
        //{
        //    get => InGame ? PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HPMax) : 0;
        //    set => PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HPMax, value);
        //}
        //public int HealthCap
        //{
        //    get
        //    {
        //        if (!InGame) return 0;
        //        var cap = PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HPCap);
        //        return cap < HealthMax ? cap : HealthMax;
        //    }
        //    set => PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HPCap, value);
        //}
        //public int HealthMin
        //{
        //    get => InGame ? PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HPMin) : 0;
        //    set => PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HPMin, value);
        //}
        //public float Stamina
        //{
        //    get => InGame ? PlayerCtrl.ReadSingle(Offsets.PlayerCtrl.SP) : 0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SP, value);
        //    }
        //}
        //public float MaxStamina
        //{
        //    get => InGame ? PlayerCtrl.ReadSingle(Offsets.PlayerCtrl.SPMax) : 0;
        //    set => PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SPMax, value);
        //}
        //public float CurrPoise
        //{
        //    get => InGame ? PlayerCtrl.ReadSingle(Offsets.PlayerCtrl.CurrPoise) : 0;
        //    set => PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.CurrPoise, value);
        //}
        //public byte NetworkPhantomID
        //{
        //    get => InGame ? PlayerType.ReadByte(Offsets.PlayerType.ChrNetworkPhantomId) : (byte)0;
        //    set => PlayerType.WriteByte(Offsets.PlayerType.ChrNetworkPhantomId, value);
        //}
        //public byte TeamType
        //{
        //    get => InGame ? PlayerType.ReadByte(Offsets.PlayerType.TeamType) : (byte)0;
        //    //set => PlayerType.WriteByte(Offsets.PlayerType.TeamType, value);
        //}
        //public byte CharType
        //{
        //    get => InGame ? PlayerType.ReadByte(Offsets.PlayerType.CharType) : (byte)0;
        //    //set => PlayerType.WriteByte(Offsets.PlayerType.CharType, value);
        //}
        //public float[] Pos
        //{
        //    get => new float[3] { PosX, PosY, PosZ };
        //    set
        //    {
        //        PosX = value[0];
        //        PosY = value[1];
        //        PosZ = value[2];
        //    }
        //}
        //public float PosX
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosX) : 0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosX, value);
        //    }
        //}
        //public float PosY
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosY) : 0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosY, value);
        //    }
        //}
        //public float PosZ
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosZ) : 0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosZ, value);
        //    }
        //}
        //public float[] Ang
        //{
        //    get => new float[3] { AngX, AngY, AngZ };
        //    set
        //    {
        //        AngX = value[0];
        //        AngY = value[1];
        //        AngZ = value[2];
        //    }
        //}
        //private float AngX
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngX) : 0;
        //    set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngX, value);
        //}
        //private float AngY
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngY) : 0;
        //    set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngY, value);
        //}
        //private float AngZ
        //{
        //    get => InGame ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngZ) : 0;
        //    set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngZ, value);
        //}
        //public float[] StablePos
        //{
        //    get => new float[3] { StableX, StableY, StableZ };
        //    set
        //    {
        //        StableX = value[0];
        //        StableY = value[1];
        //        StableZ = value[2];
        //    }
        //}
        //private float StableX
        //{
        //    get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpX1) : 0;
        //    set
        //    {
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX1, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX2, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX3, value);
        //    }
        //}
        //private float StableY
        //{
        //    get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpY1) : 0;
        //    set
        //    {
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY1, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY2, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY3, value);
        //    }
        //}
        //private float StableZ
        //{
        //    get => InGame ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpZ1) : 0;
        //    set
        //    {
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ1, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ2, value);
        //        PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ3, value);
        //    }
        //}
        //public float BloodstainX
        //{
        //    get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainX);
        //}
        //public float BloodstainY
        //{
        //    get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainY);
        //}
        //public float BloodstainZ
        //{
        //    get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainZ);
        //}
        //public float CamX
        //{
        //    get => Camera2.ReadSingle(Offsets.Camera.CamX);
        //    set => Camera2.WriteSingle(Offsets.Camera.CamX, value);
        //}
        //public float CamY
        //{
        //    get => Camera2.ReadSingle(Offsets.Camera.CamY);
        //    set => Camera2.WriteSingle(Offsets.Camera.CamY, value);
        //}
        //public float CamZ
        //{
        //    get => Camera2.ReadSingle(Offsets.Camera.CamZ);
        //    set => Camera2.WriteSingle(Offsets.Camera.CamZ, value);
        //}

        //public float Speed
        //{
        //    set
        //    {
        //        if (!InGame) return;
        //        PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SpeedModifier, value);
        //    }
        //}
        //public bool Gravity
        //{
        //    get => InGame ? !PlayerGravity.ReadBoolean(Offsets.Gravity.GravityFlag) : true;
        //    set => PlayerGravity.WriteBoolean(Offsets.Gravity.GravityFlag, !value);
        //}
        //public bool Collision
        //{
        //    get => InGame ? NetworkPhantomID != 18 && NetworkPhantomID != 19 : true;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        if (value)
        //            NetworkPhantomID = 0;
        //        else
        //            NetworkPhantomID = 18;
        //    }
        //}
        //public ushort LastBonfireID
        //{
        //    get => InGame ? EventManager.ReadUInt16(Offsets.Bonfire.LastSetBonfire) : (ushort)0;
        //    set => EventManager.WriteUInt16(Offsets.Bonfire.LastSetBonfire, value);
        //}
        //public int LastBonfireAreaID
        //{
        //    get => InGame ? EventManager.ReadInt32(Offsets.Bonfire.LastSetBonfireAreaID) : 0;
        //    set => EventManager.WriteInt32(Offsets.Bonfire.LastSetBonfireAreaID, value);
        //}
        //public bool Multiplayer => !InGame || ConnectionType > 1;
        //public string Online
        //{
        //    get
        //    {
        //        if (!Hooked) return "Unhooked";
        //        if (!InGame) return "";
        //        if (Offsets.Connection.Online == DS2HookOffsets.UNSET) return "unknown";
        //        return ConnectionType > 0 ? "YES" : "NO";
        //    }
        //}
        //public int ConnectionType => Hooked && Connection != null ? Connection.ReadInt32(Offsets.Connection.Online) : 0;

        // Bonfire Get/Setter related:
        //private byte ReadBfLevel(int bfoffset)
        //{
        //    var rawlevel = BonfireLevels.ReadByte(bfoffset);
        //    return (byte)((rawlevel + 1) / 2);
        //}
        //private void WriteBfLevel(int bfoffset, byte level)
        //{
        //    // converted it into ds2 form first and then write
        //    byte rawval = level > 0 ? (byte)(level * 2 - 1) : (byte)0;
        //    BonfireLevels.WriteByte(bfoffset, rawval);
        //}
        //private BonfireLevels Bfs => Offsets.BonfireLevels; // shorthand
        ////
        //public Dictionary<DS2SBonfire, string> Bfs2Properties = new()
        //{
        //    [DS2Resource.GetBonfireByName("Fire Keepers' Dwelling")] = nameof(FireKeepersDwelling),
        //    [DS2Resource.GetBonfireByName("The Far Fire")] = nameof(TheFarFire),
        //    [DS2Resource.GetBonfireByName("The Crestfallen's Retreat")] = nameof(CrestfallensRetreat),
        //    [DS2Resource.GetBonfireByName("Cardinal Tower")] = nameof(CardinalTower),
        //    [DS2Resource.GetBonfireByName("Soldiers' Rest")] = nameof(SoldiersRest),
        //    [DS2Resource.GetBonfireByName("The Place Unbeknownst")] = nameof(ThePlaceUnbeknownst),
        //    [DS2Resource.GetBonfireByName("Heide's Ruin")] = nameof(HeidesRuin),
        //    [DS2Resource.GetBonfireByName("Tower of Flame")] = nameof(TowerOfFlame),
        //    [DS2Resource.GetBonfireByName("The Blue Cathedral")] = nameof(TheBlueCathedral),
        //    [DS2Resource.GetBonfireByName("Unseen Path to Heide")] = nameof(UnseenPathtoHeide),
        //    [DS2Resource.GetBonfireByName("Exile Holding Cells")] = nameof(ExileHoldingCells),
        //    [DS2Resource.GetBonfireByName("McDuff's Workshop")] = nameof(McDuffsWorkshop),
        //    [DS2Resource.GetBonfireByName("Servants' Quarters")] = nameof(ServantsQuarters),
        //    [DS2Resource.GetBonfireByName("Straid's Cell")] = nameof(StraidsCell),
        //    [DS2Resource.GetBonfireByName("The Tower Apart")] = nameof(TheTowerApart),
        //    [DS2Resource.GetBonfireByName("The Saltfort")] = nameof(TheSaltfort),
        //    [DS2Resource.GetBonfireByName("Upper Ramparts")] = nameof(UpperRamparts),
        //    [DS2Resource.GetBonfireByName("Undead Refuge")] = nameof(UndeadRefuge),
        //    [DS2Resource.GetBonfireByName("Bridge Approach")] = nameof(BridgeApproach),
        //    [DS2Resource.GetBonfireByName("Undead Lockaway")] = nameof(UndeadLockaway),
        //    [DS2Resource.GetBonfireByName("Undead Purgatory")] = nameof(UndeadPurgatory),
        //    [DS2Resource.GetBonfireByName("Poison Pool")] = nameof(PoisonPool),
        //    [DS2Resource.GetBonfireByName("The Mines")] = nameof(TheMines),
        //    [DS2Resource.GetBonfireByName("Lower Earthen Peak")] = nameof(LowerEarthenPeak),
        //    [DS2Resource.GetBonfireByName("Central Earthen Peak")] = nameof(CentralEarthenPeak),
        //    [DS2Resource.GetBonfireByName("Upper Earthen Peak")] = nameof(UpperEarthenPeak),
        //    [DS2Resource.GetBonfireByName("Threshold Bridge")] = nameof(ThresholdBridge),
        //    [DS2Resource.GetBonfireByName("Ironhearth Hall")] = nameof(IronhearthHall),
        //    [DS2Resource.GetBonfireByName("Eygil's Idol")] = nameof(EygilsIdol),
        //    [DS2Resource.GetBonfireByName("Belfry Sol Approach")] = nameof(BelfrySolApproach),
        //    [DS2Resource.GetBonfireByName("Old Akelarre")] = nameof(OldAkelarre),
        //    [DS2Resource.GetBonfireByName("Ruined Fork Road")] = nameof(RuinedForkRoad),
        //    [DS2Resource.GetBonfireByName("Shaded Ruins")] = nameof(ShadedRuins),
        //    [DS2Resource.GetBonfireByName("Gyrm's Respite")] = nameof(GyrmsRespite),
        //    [DS2Resource.GetBonfireByName("Ordeal's End")] = nameof(OrdealsEnd),
        //    [DS2Resource.GetBonfireByName("Royal Army Campsite")] = nameof(RoyalArmyCampsite),
        //    [DS2Resource.GetBonfireByName("Chapel Threshold")] = nameof(ChapelThreshold),
        //    [DS2Resource.GetBonfireByName("Lower Brightstone Cove")] = nameof(LowerBrightstoneCove),
        //    [DS2Resource.GetBonfireByName("Harval's Resting Place")] = nameof(HarvalsRestingPlace),
        //    [DS2Resource.GetBonfireByName("Grave Entrance")] = nameof(GraveEntrance),
        //    [DS2Resource.GetBonfireByName("Upper Gutter")] = nameof(UpperGutter),
        //    [DS2Resource.GetBonfireByName("Central Gutter")] = nameof(CentralGutter),
        //    [DS2Resource.GetBonfireByName("Black Gulch Mouth")] = nameof(BlackGulchMouth),
        //    [DS2Resource.GetBonfireByName("Hidden Chamber")] = nameof(HiddenChamber),
        //    [DS2Resource.GetBonfireByName("King's Gate")] = nameof(KingsGate),
        //    [DS2Resource.GetBonfireByName("Under Castle Drangleic")] = nameof(UnderCastleDrangleic),
        //    [DS2Resource.GetBonfireByName("Central Castle Drangleic")] = nameof(CentralCastleDrangleic),
        //    [DS2Resource.GetBonfireByName("Forgotten Chamber")] = nameof(ForgottenChamber),
        //    [DS2Resource.GetBonfireByName("Tower of Prayer (Amana)")] = nameof(TowerOfPrayerAmana),
        //    [DS2Resource.GetBonfireByName("Crumbled Ruins")] = nameof(CrumbledRuins),
        //    [DS2Resource.GetBonfireByName("Rhoy's Resting Place")] = nameof(RhoysRestingPlace),
        //    [DS2Resource.GetBonfireByName("Rise of the Dead")] = nameof(RiseOfTheDead),
        //    [DS2Resource.GetBonfireByName("Undead Crypt Entrance")] = nameof(UndeadCryptEntrance),
        //    [DS2Resource.GetBonfireByName("Undead Ditch")] = nameof(UndeadDitch),
        //    [DS2Resource.GetBonfireByName("Foregarden")] = nameof(Foregarden),
        //    [DS2Resource.GetBonfireByName("Ritual Site")] = nameof(RitualSite),
        //    [DS2Resource.GetBonfireByName("Dragon Aerie")] = nameof(DragonAerie),
        //    [DS2Resource.GetBonfireByName("Shrine Entrance")] = nameof(ShrineEntrance),
        //    [DS2Resource.GetBonfireByName("Sanctum Walk")] = nameof(SanctumWalk),
        //    [DS2Resource.GetBonfireByName("Tower of Prayer (Shulva)")] = nameof(TowerOfPrayerShulva),
        //    [DS2Resource.GetBonfireByName("Priestess' Chamber")] = nameof(PriestessChamber),
        //    [DS2Resource.GetBonfireByName("Hidden Sanctum Chamber")] = nameof(HiddenSanctumChamber),
        //    [DS2Resource.GetBonfireByName("Lair of the Imperfect")] = nameof(LairOfTheImperfect),
        //    [DS2Resource.GetBonfireByName("Sanctum Interior")] = nameof(SanctumInterior),
        //    [DS2Resource.GetBonfireByName("Sanctum Nadir")] = nameof(SanctumNadir),
        //    [DS2Resource.GetBonfireByName("Throne Floor")] = nameof(ThroneFloor),
        //    [DS2Resource.GetBonfireByName("Upper Floor")] = nameof(UpperFloor),
        //    [DS2Resource.GetBonfireByName("Foyer")] = nameof(Foyer),
        //    [DS2Resource.GetBonfireByName("Lowermost Floor")] = nameof(LowermostFloor),
        //    [DS2Resource.GetBonfireByName("The Smelter Throne")] = nameof(TheSmelterThrone),
        //    [DS2Resource.GetBonfireByName("Iron Hallway Entrance")] = nameof(IronHallwayEntrance),
        //    [DS2Resource.GetBonfireByName("Outer Wall")] = nameof(OuterWall),
        //    [DS2Resource.GetBonfireByName("Abandoned Dwelling")] = nameof(AbandonedDwelling),
        //    [DS2Resource.GetBonfireByName("Expulsion Chamber")] = nameof(ExpulsionChamber),
        //    [DS2Resource.GetBonfireByName("Inner Wall")] = nameof(InnerWall),
        //    [DS2Resource.GetBonfireByName("Lower Garrison")] = nameof(LowerGarrison),
        //    [DS2Resource.GetBonfireByName("Grand Cathedral")] = nameof(GrandCathedral),
        //};

        //public byte FireKeepersDwelling { get => ReadBfLevel(Bfs.FireKeepersDwelling); set { WriteBfLevel(Bfs.FireKeepersDwelling, value); } }
        //public byte TheFarFire { get => ReadBfLevel(Bfs.TheFarFire); set { WriteBfLevel(Bfs.TheFarFire, value); } }
        //public byte CrestfallensRetreat { get => ReadBfLevel(Bfs.CrestfallensRetreat); set { WriteBfLevel(Bfs.CrestfallensRetreat, value); } }
        //public byte CardinalTower { get => ReadBfLevel(Bfs.CardinalTower); set { WriteBfLevel(Bfs.CardinalTower, value); } }
        //public byte SoldiersRest { get => ReadBfLevel(Bfs.SoldiersRest); set { WriteBfLevel(Bfs.SoldiersRest, value); } }
        //public byte ThePlaceUnbeknownst { get => ReadBfLevel(Bfs.ThePlaceUnbeknownst); set { WriteBfLevel(Bfs.ThePlaceUnbeknownst, value); } }
        //public byte HeidesRuin { get => ReadBfLevel(Bfs.HeidesRuin); set { WriteBfLevel(Bfs.HeidesRuin, value); } }
        //public byte TowerOfFlame { get => ReadBfLevel(Bfs.TowerOfFlame); set { WriteBfLevel(Bfs.TowerOfFlame, value); } }
        //public byte TheBlueCathedral { get => ReadBfLevel(Bfs.TheBlueCathedral); set { WriteBfLevel(Bfs.TheBlueCathedral, value); } }
        //public byte UnseenPathtoHeide { get => ReadBfLevel(Bfs.UnseenPathtoHeide); set { WriteBfLevel(Bfs.UnseenPathtoHeide, value); } }
        //public byte ExileHoldingCells { get => ReadBfLevel(Bfs.ExileHoldingCells); set { WriteBfLevel(Bfs.ExileHoldingCells, value); } }
        //public byte McDuffsWorkshop { get => ReadBfLevel(Bfs.McDuffsWorkshop); set { WriteBfLevel(Bfs.McDuffsWorkshop, value); } }
        //public byte ServantsQuarters { get => ReadBfLevel(Bfs.ServantsQuarters); set { WriteBfLevel(Bfs.ServantsQuarters, value); } }
        //public byte StraidsCell { get => ReadBfLevel(Bfs.StraidsCell); set { WriteBfLevel(Bfs.StraidsCell, value); } }
        //public byte TheTowerApart { get => ReadBfLevel(Bfs.TheTowerApart); set { WriteBfLevel(Bfs.TheTowerApart, value); } }
        //public byte TheSaltfort { get => ReadBfLevel(Bfs.TheSaltfort); set { WriteBfLevel(Bfs.TheSaltfort, value); } }
        //public byte UpperRamparts { get => ReadBfLevel(Bfs.UpperRamparts); set { WriteBfLevel(Bfs.UpperRamparts, value); } }
        //public byte UndeadRefuge { get => ReadBfLevel(Bfs.UndeadRefuge); set { WriteBfLevel(Bfs.UndeadRefuge, value); } }
        //public byte BridgeApproach { get => ReadBfLevel(Bfs.BridgeApproach); set { WriteBfLevel(Bfs.BridgeApproach, value); } }
        //public byte UndeadLockaway { get => ReadBfLevel(Bfs.UndeadLockaway); set { WriteBfLevel(Bfs.UndeadLockaway, value); } }
        //public byte UndeadPurgatory { get => ReadBfLevel(Bfs.UndeadPurgatory); set { WriteBfLevel(Bfs.UndeadPurgatory, value); } }
        //public byte PoisonPool { get => ReadBfLevel(Bfs.PoisonPool); set { WriteBfLevel(Bfs.PoisonPool, value); } }
        //public byte TheMines { get => ReadBfLevel(Bfs.TheMines); set { WriteBfLevel(Bfs.TheMines, value); } }
        //public byte LowerEarthenPeak { get => ReadBfLevel(Bfs.LowerEarthenPeak); set { WriteBfLevel(Bfs.LowerEarthenPeak, value); } }
        //public byte CentralEarthenPeak { get => ReadBfLevel(Bfs.CentralEarthenPeak); set { WriteBfLevel(Bfs.CentralEarthenPeak, value); } }
        //public byte UpperEarthenPeak { get => ReadBfLevel(Bfs.UpperEarthenPeak); set { WriteBfLevel(Bfs.UpperEarthenPeak, value); } }
        //public byte ThresholdBridge { get => ReadBfLevel(Bfs.ThresholdBridge); set { WriteBfLevel(Bfs.ThresholdBridge, value); } }
        //public byte IronhearthHall { get => ReadBfLevel(Bfs.IronhearthHall); set { WriteBfLevel(Bfs.IronhearthHall, value); } }
        //public byte EygilsIdol { get => ReadBfLevel(Bfs.EygilsIdol); set { WriteBfLevel(Bfs.EygilsIdol, value); } }
        //public byte BelfrySolApproach { get => ReadBfLevel(Bfs.BelfrySolApproach); set { WriteBfLevel(Bfs.BelfrySolApproach, value); } }
        //public byte OldAkelarre { get => ReadBfLevel(Bfs.OldAkelarre); set { WriteBfLevel(Bfs.OldAkelarre, value); } }
        //public byte RuinedForkRoad { get => ReadBfLevel(Bfs.RuinedForkRoad); set { WriteBfLevel(Bfs.RuinedForkRoad, value); } }
        //public byte ShadedRuins { get => ReadBfLevel(Bfs.ShadedRuins); set { WriteBfLevel(Bfs.ShadedRuins, value); } }
        //public byte GyrmsRespite { get => ReadBfLevel(Bfs.GyrmsRespite); set { WriteBfLevel(Bfs.GyrmsRespite, value); } }
        //public byte OrdealsEnd { get => ReadBfLevel(Bfs.OrdealsEnd); set { WriteBfLevel(Bfs.OrdealsEnd, value); } }
        //public byte RoyalArmyCampsite { get => ReadBfLevel(Bfs.RoyalArmyCampsite); set { WriteBfLevel(Bfs.RoyalArmyCampsite, value); } }
        //public byte ChapelThreshold { get => ReadBfLevel(Bfs.ChapelThreshold); set { WriteBfLevel(Bfs.ChapelThreshold, value); } }
        //public byte LowerBrightstoneCove { get => ReadBfLevel(Bfs.LowerBrightstoneCove); set { WriteBfLevel(Bfs.LowerBrightstoneCove, value); } }
        //public byte HarvalsRestingPlace { get => ReadBfLevel(Bfs.HarvalsRestingPlace); set { WriteBfLevel(Bfs.HarvalsRestingPlace, value); } }
        //public byte GraveEntrance { get => ReadBfLevel(Bfs.GraveEntrance); set { WriteBfLevel(Bfs.GraveEntrance, value); } }
        //public byte UpperGutter { get => ReadBfLevel(Bfs.UpperGutter); set { WriteBfLevel(Bfs.UpperGutter, value); } }
        //public byte CentralGutter { get => ReadBfLevel(Bfs.CentralGutter); set { WriteBfLevel(Bfs.CentralGutter, value); } }
        //public byte HiddenChamber { get => ReadBfLevel(Bfs.HiddenChamber); set { WriteBfLevel(Bfs.HiddenChamber, value); } }
        //public byte BlackGulchMouth { get => ReadBfLevel(Bfs.BlackGulchMouth); set { WriteBfLevel(Bfs.BlackGulchMouth, value); } }
        //public byte KingsGate { get => ReadBfLevel(Bfs.KingsGate); set { WriteBfLevel(Bfs.KingsGate, value); } }
        //public byte UnderCastleDrangleic { get => ReadBfLevel(Bfs.UnderCastleDrangleic); set { WriteBfLevel(Bfs.UnderCastleDrangleic, value); } }
        //public byte ForgottenChamber { get => ReadBfLevel(Bfs.ForgottenChamber); set { WriteBfLevel(Bfs.ForgottenChamber, value); } }
        //public byte CentralCastleDrangleic { get => ReadBfLevel(Bfs.CentralCastleDrangleic); set { WriteBfLevel(Bfs.CentralCastleDrangleic, value); } }
        //public byte TowerOfPrayerAmana { get => ReadBfLevel(Bfs.TowerOfPrayerAmana); set { WriteBfLevel(Bfs.TowerOfPrayerAmana, value); } }
        //public byte CrumbledRuins { get => ReadBfLevel(Bfs.CrumbledRuins); set { WriteBfLevel(Bfs.CrumbledRuins, value); } }
        //public byte RhoysRestingPlace { get => ReadBfLevel(Bfs.RhoysRestingPlace); set { WriteBfLevel(Bfs.RhoysRestingPlace, value); } }
        //public byte RiseOfTheDead { get => ReadBfLevel(Bfs.RiseOfTheDead); set { WriteBfLevel(Bfs.RiseOfTheDead, value); } }
        //public byte UndeadCryptEntrance { get => ReadBfLevel(Bfs.UndeadCryptEntrance); set { WriteBfLevel(Bfs.UndeadCryptEntrance, value); } }
        //public byte UndeadDitch { get => ReadBfLevel(Bfs.UndeadDitch); set { WriteBfLevel(Bfs.UndeadDitch, value); } }
        //public byte Foregarden { get => ReadBfLevel(Bfs.Foregarden); set { WriteBfLevel(Bfs.Foregarden, value); } }
        //public byte RitualSite { get => ReadBfLevel(Bfs.RitualSite); set { WriteBfLevel(Bfs.RitualSite, value); } }
        //public byte DragonAerie { get => ReadBfLevel(Bfs.DragonAerie); set { WriteBfLevel(Bfs.DragonAerie, value); } }
        //public byte ShrineEntrance { get => ReadBfLevel(Bfs.ShrineEntrance); set { WriteBfLevel(Bfs.ShrineEntrance, value); } }
        //public byte SanctumWalk { get => ReadBfLevel(Bfs.SanctumWalk); set { WriteBfLevel(Bfs.SanctumWalk, value); } }
        //public byte PriestessChamber { get => ReadBfLevel(Bfs.PriestessChamber); set { WriteBfLevel(Bfs.PriestessChamber, value); } }
        //public byte HiddenSanctumChamber { get => ReadBfLevel(Bfs.HiddenSanctumChamber); set { WriteBfLevel(Bfs.HiddenSanctumChamber, value); } }
        //public byte LairOfTheImperfect { get => ReadBfLevel(Bfs.LairOfTheImperfect); set { WriteBfLevel(Bfs.LairOfTheImperfect, value); } }
        //public byte SanctumInterior { get => ReadBfLevel(Bfs.SanctumInterior); set { WriteBfLevel(Bfs.SanctumInterior, value); } }
        //public byte TowerOfPrayerShulva { get => ReadBfLevel(Bfs.TowerOfPrayerShulva); set { WriteBfLevel(Bfs.TowerOfPrayerShulva, value); } }
        //public byte SanctumNadir { get => ReadBfLevel(Bfs.SanctumNadir); set { WriteBfLevel(Bfs.SanctumNadir, value); } }
        //public byte ThroneFloor { get => ReadBfLevel(Bfs.ThroneFloor); set { WriteBfLevel(Bfs.ThroneFloor, value); } }
        //public byte UpperFloor { get => ReadBfLevel(Bfs.UpperFloor); set { WriteBfLevel(Bfs.UpperFloor, value); } }
        //public byte Foyer { get => ReadBfLevel(Bfs.Foyer); set { WriteBfLevel(Bfs.Foyer, value); } }
        //public byte LowermostFloor { get => ReadBfLevel(Bfs.LowermostFloor); set { WriteBfLevel(Bfs.LowermostFloor, value); } }
        //public byte TheSmelterThrone { get => ReadBfLevel(Bfs.TheSmelterThrone); set { WriteBfLevel(Bfs.TheSmelterThrone, value); } }
        //public byte IronHallwayEntrance { get => ReadBfLevel(Bfs.IronHallwayEntrance); set { WriteBfLevel(Bfs.IronHallwayEntrance, value); } }
        //public byte OuterWall { get => ReadBfLevel(Bfs.OuterWall); set { WriteBfLevel(Bfs.OuterWall, value); } }
        //public byte AbandonedDwelling { get => ReadBfLevel(Bfs.AbandonedDwelling); set { WriteBfLevel(Bfs.AbandonedDwelling, value); } }
        //public byte ExpulsionChamber { get => ReadBfLevel(Bfs.ExpulsionChamber); set { WriteBfLevel(Bfs.ExpulsionChamber, value); } }
        //public byte InnerWall { get => ReadBfLevel(Bfs.InnerWall); set { WriteBfLevel(Bfs.InnerWall, value); } }
        //public byte LowerGarrison { get => ReadBfLevel(Bfs.LowerGarrison); set { WriteBfLevel(Bfs.LowerGarrison, value); } }
        //public byte GrandCathedral { get => ReadBfLevel(Bfs.GrandCathedral); set { WriteBfLevel(Bfs.GrandCathedral, value); } }

        // Equipped items:
        //private string PlayerCtrlToName(int? offset)
        //{
        //    if (offset == null || !InGame)
        //        return string.Empty;
        //    return PlayerCtrl.ReadInt32((int)offset).AsMetaName();
        //}
        ////
        //public string Head => PlayerCtrlToName(Offsets?.PlayerEquipment.Head);
        //public string Chest => PlayerCtrlToName(Offsets?.PlayerEquipment.Chest);
        //public string Arms => PlayerCtrlToName(Offsets?.PlayerEquipment.Arms);
        //public string Legs => PlayerCtrlToName(Offsets?.PlayerEquipment.Legs);
        //public string RightHand1 => PlayerCtrlToName(Offsets?.PlayerEquipment.RightHand1);
        //public string RightHand2 => PlayerCtrlToName(Offsets?.PlayerEquipment.RightHand2);
        //public string RightHand3 => PlayerCtrlToName(Offsets?.PlayerEquipment.RightHand3);
        //public string LeftHand1 => PlayerCtrlToName(Offsets?.PlayerEquipment.LeftHand1);
        //public string LeftHand2 => PlayerCtrlToName(Offsets?.PlayerEquipment.LeftHand2);
        //public string LeftHand3 => PlayerCtrlToName(Offsets?.PlayerEquipment.LeftHand3);


        //public byte CurrentCovenant
        //{
        //    get => InGame ? PlayerParam.ReadByte(Offsets.Covenants.CurrentCovenant) : (byte)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteByte(Offsets.Covenants.CurrentCovenant, value);
        //    }
        //}

        //public string CharacterName
        //{
        //    get => InGame ? PlayerName.ReadString(Offsets.PlayerName.Name, Encoding.Unicode, 0x22) : "";
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        if (CharacterName == value) return;
        //        PlayerName.WriteString(Offsets.PlayerName.Name, Encoding.Unicode, 0x22, value);
        //        OnPropertyChanged(nameof(CharacterName));
        //    }
        //}
        //public byte Class
        //{
        //    get => InGame ? PlayerBaseMisc.ReadByte(Offsets.PlayerBaseMisc.Class) : (byte)255;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerBaseMisc.WriteByte(Offsets.PlayerBaseMisc.Class, value);
        //    }
        //}
        //public int SoulLevel
        //{
        //    get => InGame ? PlayerParam.ReadInt32(Offsets.Attributes.SoulLevel) : 0;
        //    set => PlayerParam.WriteInt32(Offsets.Attributes.SoulLevel, value);
        //}
        //public int SoulMemory
        //{
        //    get => InGame ? PlayerParam.ReadInt32(Offsets.PlayerParam.SoulMemory) : 0;
        //    set => PlayerParam.WriteInt32(Offsets.PlayerParam.SoulMemory, value);
        //}
        //public int SoulMemory2
        //{
        //    get => InGame ? PlayerParam.ReadInt32(Offsets.PlayerParam.SoulMemory2) : 0;
        //    set => PlayerParam.WriteInt32(Offsets.PlayerParam.SoulMemory2, value);
        //}
        //public byte SinnerLevel
        //{
        //    get => InGame ? PlayerParam.ReadByte(Offsets.PlayerParam.SinnerLevel) : (byte)0;
        //    set => PlayerParam.WriteByte(Offsets.PlayerParam.SinnerLevel, value);
        //}
        //public byte SinnerPoints
        //{
        //    get => InGame ? PlayerParam.ReadByte(Offsets.PlayerParam.SinnerPoints) : (byte)0;
        //    set => PlayerParam.WriteByte(Offsets.PlayerParam.SinnerPoints, value);
        //}
        //public byte HollowLevel
        //{
        //    get => InGame ? PlayerParam.ReadByte(Offsets.PlayerParam.HollowLevel) : (byte)0;
        //    set => PlayerParam.WriteByte(Offsets.PlayerParam.HollowLevel, value);
        //}
        //public int Souls
        //{
        //    get => InGame ? PlayerParam.ReadInt32(Offsets.PlayerParam.Souls) : 0;
        //}
        //public short Vigor
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.VGR) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.VGR, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Endurance
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.END) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.END, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Vitality
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.VIT) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.VIT, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Attunement
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.ATN) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.ATN, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Strength
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.STR) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.STR, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Dexterity
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.DEX) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.DEX, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Adaptability
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.ADP) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.ADP, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Intelligence
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.INT) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.INT, value);
        //        UpdateSoulLevel();
        //    }
        //}
        //public short Faith
        //{
        //    get => InGame ? PlayerParam.ReadInt16(Offsets.Attributes.FTH) : (short)0;
        //    set
        //    {
        //        if (Reading || !InGame) return;
        //        PlayerParam.WriteInt16(Offsets.Attributes.FTH, value);
        //        UpdateSoulLevel();
        //    }
        //}

    }
}
