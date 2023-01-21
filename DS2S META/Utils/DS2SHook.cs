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
using System.Threading;
using System.Threading.Tasks;
using DS2S_META.Randomizer;

using SoulsFormats;
using static SoulsFormats.PARAMDEF;
using DS2S_META.Utils;
using System.Windows.Automation;
using System.Runtime.Serialization;
using Octokit;
using System.Reflection;
using DS2S_META.Utils.Offsets;
using System.CodeDom;
using System.Buffers.Text;

namespace DS2S_META
{
    public class DS2SHook : PHook, INotifyPropertyChanged
    {
        public static readonly string ExeDir = Environment.CurrentDirectory;
        public List<Param> Params = new();


        public MainWindow MW { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }
        public IntPtr BaseAddress => Process?.MainModule?.BaseAddress ?? IntPtr.Zero;
        public string ID => Process?.Id.ToString() ?? "Not Hooked";

        private string _version = "";
        public string Version 
        { 
            get =>_version;
            private set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public DS2VER DS2Ver;
        public BBJTYPE BBJType;
        internal DS2HookOffsets Offsets;

        public static bool Reading { get; set; }

        
        public List<ItemRow> Items = new();
        
        private PHPointer GiveSoulsFunc;
        private PHPointer ItemGiveFunc;
        private PHPointer ItemStruct2dDisplay;
        private PHPointer DisplayItem;
        private PHPointer SetWarpTargetFunc;
        private PHPointer WarpManager;
        private PHPointer WarpFunc;
        private PHPointer? SomePlayerStats;

        public  PHPointer BaseA;

        private PHPointer PlayerName;
        private PHPointer AvailableItemBag;
        private PHPointer ItemGiveWindow;
        private PHPointer PlayerBaseMisc;
        private PHPointer PlayerCtrl;
        private PHPointer PlayerPosition;
        private PHPointer PlayerGravity;
        private PHPointer PlayerParam;
        private PHPointer PlayerType;
        private PHPointer SpEffectCtrl;
        private PHPointer ApplySpEffect;
        private PHPointer PlayerMapData;
        private PHPointer EventManager;
        private PHPointer BonfireLevels;
        private PHPointer NetSvrBloodstainManager;

        
        private PHPointer BaseASetup;
        private PHPointer BaseBSetup;
        private PHPointer BaseB;
        private PHPointer Connection;

        private PHPointer Camera;
        private PHPointer Camera2;
        private PHPointer Camera3;
        private PHPointer Camera4;
        private PHPointer Camera5;

        private PHPointer SpeedFactorAccel;
        private PHPointer SpeedFactorAnim;
        private PHPointer SpeedFactorJump;
        private PHPointer SpeedFactorBuildup;

        public bool Loaded => PlayerCtrl != null && PlayerCtrl.Resolve() != IntPtr.Zero;
        public bool Setup = false;

        public bool Focused => Hooked && User32.GetForegroundProcessID() == Process.Id;

        public DS2SHook(int refreshInterval, int minLifetime) :
            base(refreshInterval, minLifetime, p => p.MainWindowTitle == "DARK SOULS II")
        {
            Version = "Not Hooked";
            
            OnHooked += DS2Hook_OnHooked;
            OnUnhooked += DS2Hook_OnUnhooked;
        }

        public void RegisterAOBs()
        {
            if (Offsets?.Func == null)
                throw new Exception("Func structure null");

            BaseBSetup = RegisterAbsoluteAOB(Offsets.Func.BaseBAoB);
            SpeedFactorAccel = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAccelOffset);
            SpeedFactorAnim = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorAnimOffset);
            SpeedFactorJump = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorJumpOffset);
            SpeedFactorBuildup = RegisterAbsoluteAOB(Offsets.Func.SpeedFactorBuildupOffset);
            GiveSoulsFunc = RegisterAbsoluteAOB(Offsets.Func.GiveSoulsFuncAoB);
            ItemGiveFunc = RegisterAbsoluteAOB(Offsets.Func.ItemGiveFunc);
            ItemStruct2dDisplay = RegisterAbsoluteAOB(Offsets.Func.ItemStruct2dDisplay);
            SetWarpTargetFunc = RegisterAbsoluteAOB(Offsets.Func.SetWarpTargetFuncAoB);
            WarpFunc = RegisterAbsoluteAOB(Offsets.Func.WarpFuncAoB);
            
            // Version Specific AOBs:
            ApplySpEffect = RegisterAbsoluteAOB(Offsets.Func.ApplySpEffectAoB);
            DisplayItem = RegisterAbsoluteAOB(Offsets.Func.DisplayItem);
        }

        // DS2 & BBJ Process Info Data
        private const byte NOBBJBYTE = 0xF3;
        private const byte NEWBBJBYTE = 0x49;
        public enum DS2VER 
        { 
            VANILLA_V112,
            VANILLA_V111, 
            VANILLA_V102, 
            SOTFS_V102, 
            SOTFS_V103, 
            UNSUPPORTED 
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
        public bool IsSOTFS_CP => DS2Ver == DS2VER.SOTFS_V103;
        public bool IsSOTFS => new DS2VER[] { DS2VER.SOTFS_V102, DS2VER.SOTFS_V103 }.Contains(DS2Ver);
        public bool IsVanilla => new DS2VER[] {DS2VER.VANILLA_V102, DS2VER.VANILLA_V111, DS2VER.VANILLA_V112}.Contains(DS2Ver);
        public bool IsValidVer;

        private void DS2Hook_OnHooked(object? sender, PHEventArgs e)
        {
            DS2Ver = GetDS2Ver();
            IsValidVer = CheckValidVer();
            
            // Initial Setup & Version Checks:
            Offsets = GetOffsets();
            BasePointerSetup(out bool isOldBbj); // set BaseA (base pointer)
            BBJType = GetBBJType(isOldBbj);
            RegisterAOBs(); // Absolute AoBs
            RescanAOB();
            SetupChildPointers();

            
            // Slowly migrate to param handling class:
            ParamMan.Initialise(this);
            GetVanillaItems();
            GetLevelRequirements();

            UpdateStatsProperties();
            GetSpeedhackOffsets(SpeedhackDllPath);
            Version = GetStringVersion();
            Setup = true;
        }
        private void DS2Hook_OnUnhooked(object? sender, PHEventArgs e)
        {
            Version = "Not Hooked";
            Setup = false;
            ClearSpeedhackInject();
            ParamMan.Uninitialise();
            MW.HKM.ClearHooks();
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
        internal bool CheckValidVer()
        {
            var validvers = new DS2VER[]
            {
                DS2VER.VANILLA_V111,
                DS2VER.VANILLA_V112,
                DS2VER.SOTFS_V102,
                DS2VER.SOTFS_V103,
            };
            return validvers.Contains(DS2Ver);
        }
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
                return BBJTYPE.UNKN_VANILLA;

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
            switch (testbyte)
            {
                case NOBBJBYTE:
                    return BBJTYPE.NOBBJ;
                case NEWBBJBYTE:
                    return BBJTYPE.NEWBBJ_SOTFS;
                default:
                    throw new Exception("Probably an issue with setting up the pointers/addresses");
            }
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
            switch (DS2Ver)
            {
                case DS2VER.SOTFS_V102:
                    sb.Append(" V1.02");
                    break;

                case DS2VER.SOTFS_V103:
                    sb.Append(" V1.03");
                    break;

                case DS2VER.VANILLA_V102:
                    sb.Append(" V1.02 Old Patch");
                    break;

                case DS2VER.VANILLA_V111:
                    sb.Append(" V1.11");
                    break;

                case DS2VER.VANILLA_V112:
                    sb.Append(" V1.12");
                    break;
            }

            // get mod versions:
            switch (BBJType)
            {
                case BBJTYPE.NOBBJ:
                    sb.Append(" (unmodded)");
                    break;

                case BBJTYPE.OLDBBJ_SOTFS:
                    sb.Append(" (old bbj mod)");
                    break;

                case BBJTYPE.NEWBBJ_SOTFS:
                    sb.Append(" (bbj mod)");
                    break;

                default:
                    sb.Append(" (unknown mod)");
                    break;
            }
            return sb.ToString();
        }

        internal DS2HookOffsets GetOffsets()
        {
            return DS2Ver switch
            {
                DS2VER.VANILLA_V102 => throw new Exception("Not implemented yet"),
                DS2VER.VANILLA_V111 => new DS2VOffsetsV111(),
                DS2VER.VANILLA_V112 => new DS2VOffsetsV112(),
                DS2VER.SOTFS_V102 => new DS2SOffsetsV102(),
                DS2VER.SOTFS_V103 => new DS2SOffsetsV103(),
                _ => throw new Exception("Unexpected Sotfs Module Size, likely not supported."),
            };
        }
        internal void BasePointerSetup(out bool isOldBbj)
        {
            BaseASetup = RegisterAbsoluteAOB(Offsets.BaseAAob);
            RescanAOB();

            // Attempt "normal" version:
            IntPtr bp_orig = BasePointerFromSetupPointer(BaseASetup);
            BaseA = CreateBasePointer(bp_orig);
            isOldBbj = BaseA.Resolve() == IntPtr.Zero;
            if (!isOldBbj)
                return;

            // Old BBJ mod BasePointer adjustment:
            BaseASetup = RegisterAbsoluteAOB(Offsets.BaseABabyJumpAoB);
            RescanAOB();
            BaseA = CreateBasePointer(BasePointerFromSetupBabyJ(BaseASetup));
        }
        internal void SetupChildPointers()
        {
            // Further pointer setup... todo?
            Core OC = Offsets.Core; // shorthand
            PlayerName = CreateChildPointer(BaseA, OC.PlayerNameOffset);
            AvailableItemBag = CreateChildPointer(PlayerName, OC.AvailableItemBagOffset, OC.AvailableItemBagOffset);
            ItemGiveWindow = CreateChildPointer(BaseA, OC.ItemGiveWindowPointer);
            PlayerBaseMisc = CreateChildPointer(PlayerName, OC.PlayerBaseMiscOffset);
            PlayerCtrl = CreateChildPointer(BaseA, OC.PlayerCtrlOffset);
            PlayerPosition = CreateChildPointer(PlayerCtrl, OC.PlayerPositionOffset1, OC.PlayerPositionOffset2);
            PlayerGravity = CreateChildPointer(PlayerCtrl, OC.PlayerMapDataOffset1);
            PlayerParam = CreateChildPointer(PlayerCtrl, OC.PlayerParamOffset);
            PlayerType = CreateChildPointer(PlayerCtrl, OC.PlayerTypeOffset);
            SpEffectCtrl = CreateChildPointer(PlayerCtrl, OC.SpEffectCtrlOffset);
            PlayerMapData = CreateChildPointer(PlayerGravity, OC.PlayerMapDataOffset2, OC.PlayerMapDataOffset3);
            EventManager = CreateChildPointer(BaseA, OC.EventManagerOffset);
            BonfireLevels = CreateChildPointer(EventManager, OC.BonfireLevelsOffset1, OC.BonfireLevelsOffset2);
            WarpManager = CreateChildPointer(EventManager, OC.WarpManagerOffset);
            NetSvrBloodstainManager = CreateChildPointer(BaseA, OC.NetSvrBloodstainManagerOffset1, OC.NetSvrBloodstainManagerOffset2, OC.NetSvrBloodstainManagerOffset3);


            BaseB = CreateBasePointer(BasePointerFromSetupPointer(BaseBSetup));
            Connection = CreateChildPointer(BaseB, OC.ConnectionOffset);

            Camera = CreateChildPointer(BaseA, OC.CameraOffset1);
            Camera2 = CreateChildPointer(Camera, OC.CameraOffset2);
            Camera3 = CreateChildPointer(BaseA, OC.CameraOffset2, OC.CameraOffset2);
            Camera4 = CreateChildPointer(BaseA, OC.CameraOffset2, OC.CameraOffset3);
            Camera5 = CreateChildPointer(BaseA, OC.CameraOffset2);

            if (Offsets.PlayerStatsOffsets != null)
                SomePlayerStats = CreateChildPointer(BaseA, Offsets.PlayerStatsOffsets);
        }
        public IntPtr BasePointerFromSetupPointer(PHPointer aobpointer)
        {
            if (Offsets.BasePtrOffset1 == null)
                throw new Exception("Base pointer offset undefined");
            if (Offsets.BasePtrOffset2 == null)
                throw new Exception("Base pointer offset 2 undefined");

            if (IsVanilla)
            {
                var addrBaseA = CreateChildPointer(aobpointer, (int)Offsets.BasePtrOffset1);
                return addrBaseA.ReadIntPtr((int)Offsets.BasePtrOffset2);
            }
            else
            {
                // The instruction seems to be a relative offset in 64-bit?
                var readInt = aobpointer.ReadInt32((int)Offsets.BasePtrOffset1);
                return aobpointer.ReadIntPtr(readInt + (int)Offsets.BasePtrOffset2);
            }
            
        }
        public IntPtr BasePointerFromSetupBabyJ(PHPointer pointer)
        {

            // TODO!
            return pointer.ReadIntPtr(0x0121D4D0 + (int)Offsets.BasePtrOffset2);
        }

        

        public void UpdateName()
        {
            OnPropertyChanged(nameof(Name));
        }
        public void UpdateMainProperties()
        {
            OnPropertyChanged(nameof(ID));
            OnPropertyChanged(nameof(Online));
        }
        public void UpdateStatsProperties()
        {
            OnPropertyChanged(nameof(SoulLevel));
            OnPropertyChanged(nameof(Souls));
            OnPropertyChanged(nameof(SoulMemory));
            OnPropertyChanged(nameof(SoulMemory2));
            OnPropertyChanged(nameof(HollowLevel));
            OnPropertyChanged(nameof(SinnerLevel));
            OnPropertyChanged(nameof(SinnerPoints));
            OnPropertyChanged(nameof(Vigor));
            OnPropertyChanged(nameof(Endurance));
            OnPropertyChanged(nameof(Vitality));
            OnPropertyChanged(nameof(Attunement));
            OnPropertyChanged(nameof(Strength));
            OnPropertyChanged(nameof(Dexterity));
            OnPropertyChanged(nameof(Adaptability));
            OnPropertyChanged(nameof(Intelligence));
            OnPropertyChanged(nameof(Faith));
        }
        public void UpdatePlayerProperties()
        {
            OnPropertyChanged(nameof(Health));
            OnPropertyChanged(nameof(HealthMax));
            OnPropertyChanged(nameof(HealthCap));
            OnPropertyChanged(nameof(Stamina));
            OnPropertyChanged(nameof(MaxStamina));
            OnPropertyChanged(nameof(TeamType));
            OnPropertyChanged(nameof(CharType));
            OnPropertyChanged(nameof(PosX));
            OnPropertyChanged(nameof(PosY));
            OnPropertyChanged(nameof(PosZ));
            OnPropertyChanged(nameof(AngX));
            OnPropertyChanged(nameof(AngY));
            OnPropertyChanged(nameof(AngZ));
            OnPropertyChanged(nameof(Collision));
            OnPropertyChanged(nameof(Gravity));
            OnPropertyChanged(nameof(StableX));
            OnPropertyChanged(nameof(StableY));
            OnPropertyChanged(nameof(StableZ));
            OnPropertyChanged(nameof(LastBonfireAreaID));
        }
        public void UpdateBonfireProperties()
        {
            OnPropertyChanged(nameof(FireKeepersDwelling));
            OnPropertyChanged(nameof(TheFarFire));
            OnPropertyChanged(nameof(TheCrestfallensRetreat));
            OnPropertyChanged(nameof(CardinalTower));
            OnPropertyChanged(nameof(SoldiersRest));
            OnPropertyChanged(nameof(ThePlaceUnbeknownst));
            OnPropertyChanged(nameof(HeidesRuin));
            OnPropertyChanged(nameof(TowerofFlame));
            OnPropertyChanged(nameof(TheBlueCathedral));
            OnPropertyChanged(nameof(UnseenPathtoHeide));
            OnPropertyChanged(nameof(ExileHoldingCells));
            OnPropertyChanged(nameof(McDuffsWorkshop));
            OnPropertyChanged(nameof(ServantsQuarters));
            OnPropertyChanged(nameof(StraidsCell));
            OnPropertyChanged(nameof(TheTowerApart));
            OnPropertyChanged(nameof(TheSaltfort));
            OnPropertyChanged(nameof(UpperRamparts));
            OnPropertyChanged(nameof(UndeadRefuge));
            OnPropertyChanged(nameof(BridgeApproach));
            OnPropertyChanged(nameof(UndeadLockaway));
            OnPropertyChanged(nameof(UndeadPurgatory));
            OnPropertyChanged(nameof(PoisonPool));
            OnPropertyChanged(nameof(TheMines));
            OnPropertyChanged(nameof(LowerEarthenPeak));
            OnPropertyChanged(nameof(CentralEarthenPeak));
            OnPropertyChanged(nameof(UpperEarthenPeak));
            OnPropertyChanged(nameof(ThresholdBridge));
            OnPropertyChanged(nameof(IronhearthHall));
            OnPropertyChanged(nameof(EygilsIdol));
            OnPropertyChanged(nameof(BelfrySolApproach));
            OnPropertyChanged(nameof(OldAkelarre));
            OnPropertyChanged(nameof(RuinedForkRoad));
            OnPropertyChanged(nameof(ShadedRuins));
            OnPropertyChanged(nameof(GyrmsRespite));
            OnPropertyChanged(nameof(OrdealsEnd));
            OnPropertyChanged(nameof(RoyalArmyCampsite));
            OnPropertyChanged(nameof(ChapelThreshold));
            OnPropertyChanged(nameof(LowerBrightstoneCove));
            OnPropertyChanged(nameof(HarvalsRestingPlace));
            OnPropertyChanged(nameof(GraveEntrance));
            OnPropertyChanged(nameof(UpperGutter));
            OnPropertyChanged(nameof(CentralGutter));
            OnPropertyChanged(nameof(HiddenChamber));
            OnPropertyChanged(nameof(BlackGulchMouth));
            OnPropertyChanged(nameof(KingsGate));
            OnPropertyChanged(nameof(UnderCastleDrangleic));
            OnPropertyChanged(nameof(ForgottenChamber));
            OnPropertyChanged(nameof(CentralCastleDrangleic));
            OnPropertyChanged(nameof(TowerofPrayer));
            OnPropertyChanged(nameof(CrumbledRuins));
            OnPropertyChanged(nameof(RhoysRestingPlace));
            OnPropertyChanged(nameof(RiseoftheDead));
            OnPropertyChanged(nameof(UndeadCryptEntrance));
            OnPropertyChanged(nameof(UndeadDitch));
            OnPropertyChanged(nameof(Foregarden));
            OnPropertyChanged(nameof(RitualSite));
            OnPropertyChanged(nameof(DragonAerie));
            OnPropertyChanged(nameof(ShrineEntrance));
            OnPropertyChanged(nameof(SanctumWalk));
            OnPropertyChanged(nameof(PriestessChamber));
            OnPropertyChanged(nameof(HiddenSanctumChamber));
            OnPropertyChanged(nameof(LairoftheImperfect));
            OnPropertyChanged(nameof(SanctumInterior));
            OnPropertyChanged(nameof(TowerofPrayer));
            OnPropertyChanged(nameof(SanctumNadir));
            OnPropertyChanged(nameof(ThroneFloor));
            OnPropertyChanged(nameof(UpperFloor));
            OnPropertyChanged(nameof(Foyer));
            OnPropertyChanged(nameof(LowermostFloor));
            OnPropertyChanged(nameof(TheSmelterThrone));
            OnPropertyChanged(nameof(IronHallwayEntrance));
            OnPropertyChanged(nameof(OuterWall));
            OnPropertyChanged(nameof(AbandonedDwelling));
            OnPropertyChanged(nameof(ExpulsionChamber));
            OnPropertyChanged(nameof(InnerWall));
            OnPropertyChanged(nameof(LowerGarrison));
            OnPropertyChanged(nameof(GrandCathedral));
        }
        public void UpdateCovenantProperties()
        {
            OnPropertyChanged(nameof(CurrentCovenant));
            OnPropertyChanged(nameof(CurrentCovenantName));
            OnPropertyChanged(nameof(HeirsOfTheSunDiscovered));
            OnPropertyChanged(nameof(HeirsOfTheSunRank));
            OnPropertyChanged(nameof(HeirsOfTheSunProgress));
            OnPropertyChanged(nameof(BlueSentinelsDiscovered));
            OnPropertyChanged(nameof(BlueSentinelsRank));
            OnPropertyChanged(nameof(BlueSentinelsProgress));
            OnPropertyChanged(nameof(BrotherhoodOfBloodDiscovered));
            OnPropertyChanged(nameof(BrotherhoodOfBloodRank));
            OnPropertyChanged(nameof(BrotherhoodOfBloodProgress));
            OnPropertyChanged(nameof(WayOfTheBlueDiscovered));
            OnPropertyChanged(nameof(WayOfTheBlueRank));
            OnPropertyChanged(nameof(WayOfTheBlueProgress));
            OnPropertyChanged(nameof(RatKingDiscovered));
            OnPropertyChanged(nameof(RatKingRank));
            OnPropertyChanged(nameof(RatKingProgress));
            OnPropertyChanged(nameof(BellKeepersDiscovered));
            OnPropertyChanged(nameof(BellKeepersRank));
            OnPropertyChanged(nameof(BellKeepersProgress));
            OnPropertyChanged(nameof(DragonRemnantsDiscovered));
            OnPropertyChanged(nameof(DragonRemnantsRank));
            OnPropertyChanged(nameof(DragonRemnantsProgress));
            OnPropertyChanged(nameof(CompanyOfChampionsDiscovered));
            OnPropertyChanged(nameof(CompanyOfChampionsRank));
            OnPropertyChanged(nameof(CompanyOfChampionsProgress));
            OnPropertyChanged(nameof(PilgrimsOfDarknessDiscovered));
            OnPropertyChanged(nameof(PilgrimsOfDarknessRank));
            OnPropertyChanged(nameof(PilgrimsOfDarknessProgress));
        }
        public void UpdateInternalProperties()
        {
            OnPropertyChanged(nameof(Head));
            OnPropertyChanged(nameof(Chest));
            OnPropertyChanged(nameof(Arms));
            OnPropertyChanged(nameof(Legs));
            OnPropertyChanged(nameof(RightHand1));
            OnPropertyChanged(nameof(RightHand2));
            OnPropertyChanged(nameof(RightHand3));
            OnPropertyChanged(nameof(LeftHand1));
            OnPropertyChanged(nameof(LeftHand2));
            OnPropertyChanged(nameof(LeftHand3));
            OnPropertyChanged(nameof(EnableSpeedFactors));
        }

        

        public byte FastQuit
        {
            set
            {
                BaseA.WriteByte(Offsets.ForceQuit.Quit, value);
            }
        }

        #region Player
        public int Health
        {
            get => Loaded ? PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HP) : 0;
            set 
            {
                if (Reading || !Loaded) return;
                PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HP, value);
            }
        }
        public int HealthMax
        {
            get => Loaded ? PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HPMax) : 0;
            set => PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HPMax, value);
        }
        public int HealthCap
        {
            get 
            {
                if (!Loaded) return 0;
                var cap = PlayerCtrl.ReadInt32(Offsets.PlayerCtrl.HPCap);
                return cap < HealthMax ? cap : HealthMax;
            }
            set => PlayerCtrl.WriteInt32(Offsets.PlayerCtrl.HPCap, value);
        }
        public float Stamina
        {
            get => Loaded ? PlayerCtrl.ReadSingle(Offsets.PlayerCtrl.SP) : 0;
            set 
            { 
                if (Reading || !Loaded) return;
                PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SP, value);
            }
        }
        public float MaxStamina
        {
            get => Loaded ? PlayerCtrl.ReadSingle(Offsets.PlayerCtrl.SPMax) : 0;
            set => PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SPMax, value);
        }
        public byte NetworkPhantomID
        {
            get => Loaded ? PlayerType.ReadByte(Offsets.PlayerType.ChrNetworkPhantomId) : (byte)0;
            set => PlayerType.WriteByte(Offsets.PlayerType.ChrNetworkPhantomId, value);
        }
        public byte TeamType
        {
            get => Loaded ? PlayerType.ReadByte(Offsets.PlayerType.TeamType) : (byte)0;
            //set => PlayerType.WriteByte(Offsets.PlayerType.TeamType, value);
        }
        public byte CharType
        {
            get => Loaded ? PlayerType.ReadByte(Offsets.PlayerType.CharType) : (byte)0;
            //set => PlayerType.WriteByte(Offsets.PlayerType.CharType, value);
        }
        public float PosX
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosX) : 0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosX, value);
            }
        }
        public float PosY
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosY) : 0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosY, value);
            }
        }
        public float PosZ
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.PosZ) : 0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerPosition.WriteSingle(Offsets.PlayerPosition.PosZ, value);
            }
        }
        public float AngX
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngX) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngX, value);
        }
        public float AngY
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngY) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngY, value);
        }
        public float AngZ
        {
            get => Loaded ? PlayerPosition.ReadSingle(Offsets.PlayerPosition.AngZ) : 0;
            set => PlayerPosition.WriteSingle(Offsets.PlayerPosition.AngZ, value);
        }
        public float StableX
        {
            get => Loaded ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpX1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpX3, value);
            }
        }
        public float StableY
        {
            get => Loaded ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpY1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpY3, value);
            }
        }
        public float StableZ
        {
            get => Loaded ? PlayerMapData.ReadSingle(Offsets.PlayerMapData.WarpZ1) : 0;
            set
            {
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ1, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ2, value);
                PlayerMapData.WriteSingle(Offsets.PlayerMapData.WarpZ3, value);
            }
        }
        public float BloodstainX
        {
            get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainX);
        }
        public float BloodstainY
        {
            get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainY);
        }
        public float BloodstainZ
        {
            get => NetSvrBloodstainManager.ReadSingle(Offsets.NetSvrBloodstainManager.BloodstainZ);
        }
        public byte[] CameraData
        {
            get => Camera5.ReadBytes((int)0xE9C, 64);
            set => Camera5.WriteBytes((int)0xE9C, value);
        }
        public byte[] CameraData2
        {
            get => Camera4.ReadBytes(Offsets.Camera.CamStart3, 512);
            set => Camera4.WriteBytes(Offsets.Camera.CamStart3, value);
        }
        public float CamX
        {
            get => Camera2.ReadSingle(Offsets.Camera.CamX);
            set => Camera2.WriteSingle(Offsets.Camera.CamX, value);
        }
        public float CamY
        {
            get => Camera2.ReadSingle(Offsets.Camera.CamY);
            set => Camera2.WriteSingle(Offsets.Camera.CamY, value);
        }
        public float CamZ
        {
            get => Camera2.ReadSingle(Offsets.Camera.CamZ);
            set => Camera2.WriteSingle(Offsets.Camera.CamZ, value);
        }
        public float Speed
        {
            set 
            {
                if (!Loaded) return;
                PlayerCtrl.WriteSingle(Offsets.PlayerCtrl.SpeedModifier, value); 
            }
        }
        public bool Gravity
        {
            get => Loaded ? !PlayerGravity.ReadBoolean(Offsets.Gravity.GravityFlag) : true;
            set => PlayerGravity.WriteBoolean(Offsets.Gravity.GravityFlag, !value);
        }
        public bool Collision
        {
            get => Loaded ? NetworkPhantomID != 18 && NetworkPhantomID != 19 : true;
            set
            {
                if (Reading || !Loaded) return;
                if (value)
                    NetworkPhantomID = 0;
                else
                    NetworkPhantomID = 18;
            }
        }
        public ushort LastBonfireID
        {
            get => Loaded ? EventManager.ReadUInt16(Offsets.Bonfire.LastSetBonfire) : (ushort)0;
            set => EventManager.WriteUInt16(Offsets.Bonfire.LastSetBonfire, value);
        }
        public int LastBonfireAreaID
        {
            get => Loaded ? EventManager.ReadInt32(Offsets.Bonfire.LastSetBonfireAreaID) : 0;
            set => EventManager.WriteInt32(Offsets.Bonfire.LastSetBonfireAreaID, value);
        }
        public bool Multiplayer => Loaded ? ConnectionType > 1 : true;
        public bool Online => Loaded ? ConnectionType > 0 : true;
        public int ConnectionType
        {
            get
            {
                var test = Hooked;
                return Hooked && Connection != null ? Connection.ReadInt32(Offsets.Connection.Online) : 0;
            }
        }
        
        internal bool WarpLast()
        {
            // TO TIDY with bonfire objects

            // Handle betwixt start warps:
            bool PrevBonfireSet = (LastBonfireAreaID != 0 && LastBonfireID != 0);
            if (PrevBonfireSet)
                return Warp(LastBonfireID);

            // Handle first area warp:
            int BETWIXTAREA = 167903232;
            ushort BETWIXTBFID = 2650;
            LastBonfireAreaID = BETWIXTAREA;
            return Warp(BETWIXTBFID, true);
        }
        internal bool Warp(ushort id, bool areadefault = false)
        {
            // area default means warp to the 0,0 part of the map (like a wrong warp)
            // areadefault = false is a normal "warp to bonfire"
            int WARPAREADEFAULT = 2;
            int WARPBONFIRE = 3;

            var value = Allocate(sizeof(short));
            Kernel32.WriteBytes(Handle, value, BitConverter.GetBytes(id));

            var asm = (byte[])DS2SAssembly.BonfireWarp.Clone();
            var bytes = BitConverter.GetBytes(value.ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x9, bytes.Length);
            bytes = BitConverter.GetBytes(SetWarpTargetFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x21, bytes.Length);
            bytes = BitConverter.GetBytes(WarpManager.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x2E, bytes.Length);
            bytes = BitConverter.GetBytes(WarpFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0x0, asm, 0x3B, bytes.Length);

            int flag = areadefault ? WARPAREADEFAULT : WARPBONFIRE;
            bytes = BitConverter.GetBytes(flag);
            Array.Copy(bytes, 0x0, asm, 0x45, bytes.Length);

            var warped = false;
            if (!Multiplayer)
            {
                Execute(asm);
                warped = true;
            }

            Free(value);
            return warped;
        }


        internal void ApplySpecialEffect(int spEffect)
        {
            if (Is64Bit)
                ApplySpecialEffect64(spEffect);
            else
                ApplySpecialEffect32(spEffect);
        }
        internal void ApplySpecialEffect64(int spEffect)
        {
            // Get assembly template
            var asm = (byte[])DS2SAssembly.ApplySpecialEffect64.Clone();

            // Prepare inputs:
            var effectStruct = Allocate(0x16);
            Kernel32.WriteBytes(Handle, effectStruct, BitConverter.GetBytes(spEffect));
            Kernel32.WriteBytes(Handle, effectStruct + 0x4, BitConverter.GetBytes(0x1));
            Kernel32.WriteBytes(Handle, effectStruct + 0xC, BitConverter.GetBytes(0x219));
            
            
            var unk = Allocate(sizeof(float));
            Kernel32.WriteBytes(Handle, unk, BitConverter.GetBytes(-1f));
            var float_m1 = BitConverter.GetBytes(unk.ToInt64());
            Array.Copy(float_m1, 0x0, asm, 0x1A, float_m1.Length);

            var ptrEffectStruct = BitConverter.GetBytes(effectStruct.ToInt64());
            var SpEfCtrl = BitConverter.GetBytes(SpEffectCtrl.Resolve().ToInt64());
            var ptrApplySpEf = BitConverter.GetBytes(ApplySpEffect.Resolve().ToInt64());

            // Update assembly with variables:
            Array.Copy(ptrEffectStruct, 0x0, asm, 0x6, ptrEffectStruct.Length);
            Array.Copy(SpEfCtrl, 0x0, asm, 0x10, SpEfCtrl.Length);
            Array.Copy(ptrApplySpEf, 0x0, asm, 0x2E, ptrApplySpEf.Length);

            // Run and tidy-up
            Execute(asm);
            Free(effectStruct);
            Free(unk);
        }
        internal void ApplySpecialEffect32(int spEffectID)
        {
            // Assembly template
            var asm = (byte[])DS2SAssembly.ApplySpecialEffect32.Clone();

            //var ptrEffectStruct = BitConverter.GetBytes(effectStruct.ToInt32());
            var spEfId = BitConverter.GetBytes(spEffectID);
            var ptrApplySpEf = BitConverter.GetBytes(ApplySpEffect.Resolve().ToInt32());
            var SpEfCtrl = BitConverter.GetBytes(SpEffectCtrl.Resolve().ToInt32());
            
            var unk = Allocate(sizeof(float));
            Kernel32.WriteBytes(Handle, unk, BitConverter.GetBytes(-1f));
            var addr_float_m1 = BitConverter.GetBytes(unk.ToInt32());


            // Update assembly with variables:
            Array.Copy(spEfId, 0x0, asm, 0x9, spEfId.Length);
            Array.Copy(addr_float_m1, 0x0, asm, 0x16, addr_float_m1.Length);
            Array.Copy(SpEfCtrl, 0x0, asm, 0x33, SpEfCtrl.Length);
            Array.Copy(ptrApplySpEf, 0x0, asm, 0x38, ptrApplySpEf.Length);

            // Run and tidy-up
            //File.WriteAllBytes("./TESTBYTES64.txt", asm); // debugging
            Execute(asm);
            Free(unk);
        }
        #endregion

        #region Speedhack
        static string SpeedhackDllPath = $"{GetTxtResourceClass.ExeDir}/Resources/DLLs/Speedhack.dll";
        public IntPtr SpeedhackDllPtr;
        IntPtr SetupPtr;
        IntPtr SetSpeedPtr;
        IntPtr DetachPtr;
        internal void Speedhack(bool enable)
        {
            if (enable)
                EnableSpeedhack();
            else
                DisableSpeedhack();
        }
        internal void ClearSpeedhackInject()
        {
            SpeedhackDllPtr = IntPtr.Zero;
        }
        public void DisableSpeedhack()
        {
            IntPtr detach = (IntPtr)(SpeedhackDllPtr.ToInt64() + DetachPtr.ToInt64());
            Kernel32.CreateRemoteThread(Handle, IntPtr.Zero, 0, detach, IntPtr.Zero, 0, IntPtr.Zero);
        }
        private void EnableSpeedhack()
        {
            IntPtr thread = IntPtr.Zero;
            if (SpeedhackDllPtr == IntPtr.Zero)
            {
                SpeedhackDllPtr = InjectDLL(SpeedhackDllPath);
            }

            IntPtr setup = (IntPtr)(SpeedhackDllPtr.ToInt64() + SetupPtr.ToInt64());
            thread = Kernel32.CreateRemoteThread(Handle, IntPtr.Zero, 0, setup, IntPtr.Zero, 0, IntPtr.Zero);
            _ = Kernel32.WaitForSingleObject(thread, uint.MaxValue);
            SetSpeed((float)Properties.Settings.Default.SpeedValue);
        }
        public void SetSpeed(float value)
        {
            IntPtr setSpeed = (IntPtr)(SpeedhackDllPtr.ToInt64() + SetSpeedPtr.ToInt64());
            IntPtr valueAddress = GetPrefferedIntPtr(sizeof(float), SpeedhackDllPtr);
            Kernel32.WriteBytes(Handle, valueAddress, BitConverter.GetBytes(value));
            var thread = Kernel32.CreateRemoteThread(Handle, IntPtr.Zero, 0, setSpeed, valueAddress, 0, IntPtr.Zero);
            Kernel32.WaitForSingleObject(thread, uint.MaxValue);
            Free(valueAddress);
        }
        private void GetSpeedhackOffsets(string path)
        {
            var lib = Kernel32.LoadLibrary(path);
            var setupOffset = Kernel32.GetProcAddress(lib, "Setup").ToInt64() - lib.ToInt64();
            var setSpeedOffset = Kernel32.GetProcAddress(lib, "SetSpeed").ToInt64() - lib.ToInt64();
            var detachOffset = Kernel32.GetProcAddress(lib, "Detach").ToInt64() - lib.ToInt64();
            SetupPtr = (IntPtr)setupOffset;
            SetSpeedPtr = (IntPtr)setSpeedOffset;
            DetachPtr = (IntPtr)detachOffset;
            Free(lib);
        }
        #endregion

        #region Stats
        public string Name
        {
            get => Loaded ? PlayerName.ReadString(Offsets.PlayerName.Name, Encoding.Unicode, 0x22) : "";
            set
            {
                if (Reading || !Loaded) return;
                if (Name == value) return;
                PlayerName.WriteString(Offsets.PlayerName.Name, Encoding.Unicode, 0x22, value);
                OnPropertyChanged(nameof(Name));
            }
        }
        public byte Class
        {
            get => Loaded ? PlayerBaseMisc.ReadByte(Offsets.PlayerBaseMisc.Class) : (byte)255;
            set
            {
                if (Reading || !Loaded) return;
                PlayerBaseMisc.WriteByte(Offsets.PlayerBaseMisc.Class, value);
            }
        }
        public int SoulLevel
        {
            get => Loaded ? PlayerParam.ReadInt32(Offsets.Attributes.SoulLevel) : 0;
            set => PlayerParam.WriteInt32(Offsets.Attributes.SoulLevel, value);
        }
        public int SoulMemory
        {
            get => Loaded ? PlayerParam.ReadInt32(Offsets.PlayerParam.SoulMemory) : 0;
            set => PlayerParam.WriteInt32(Offsets.PlayerParam.SoulMemory, value);
        }
        public int SoulMemory2
        {
            get => Loaded ? PlayerParam.ReadInt32(Offsets.PlayerParam.SoulMemory2) : 0;
            set => PlayerParam.WriteInt32(Offsets.PlayerParam.SoulMemory2, value);
        }
        public byte SinnerLevel
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.PlayerParam.SinnerLevel) : (byte)0;
            set => PlayerParam.WriteByte(Offsets.PlayerParam.SinnerLevel, value);
        }
        public byte SinnerPoints
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.PlayerParam.SinnerPoints) : (byte)0;
            set => PlayerParam.WriteByte(Offsets.PlayerParam.SinnerPoints, value);
        }
        public byte HollowLevel
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.PlayerParam.HollowLevel) : (byte)0;
            set => PlayerParam.WriteByte(Offsets.PlayerParam.HollowLevel, value);
        }
        public int Souls
        {
            get => Loaded ? PlayerParam.ReadInt32(Offsets.PlayerParam.Souls) : 0;
        }
        public short Vigor
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.VGR) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.VGR, value);
                UpdateSoulLevel();
            }
        }
        public short Endurance
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.END) : (short)0;
            set 
            { 
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.END, value);
                UpdateSoulLevel();
            }
        }
        public short Vitality
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.VIT) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.VIT, value);
                UpdateSoulLevel();
            }
        }
        public short Attunement
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.ATN) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.ATN, value);
                UpdateSoulLevel();
            }
        }
        public short Strength
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.STR) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.STR, value);
                UpdateSoulLevel();
            }
        }
        public short Dexterity
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.DEX) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.DEX, value);
                UpdateSoulLevel();
            }
        }
        public short Adaptability
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.ADP) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.ADP, value);
                UpdateSoulLevel();
            }
        }
        public short Intelligence
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.INT) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.INT, value);
                UpdateSoulLevel();
            }
        }
        public short Faith
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Attributes.FTH) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Attributes.FTH, value);
                UpdateSoulLevel();
            }
        }
        #endregion

        #region SoulsFunctions
        public void GiveSouls(int souls)
        {
            var asm = (byte[])DS2SAssembly.AddSouls.Clone();

            var bytes = BitConverter.GetBytes(PlayerParam.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x6, 8);
            bytes = BitConverter.GetBytes(souls);
            Array.Copy(bytes, 0, asm, 0x11, 4);
            bytes = BitConverter.GetBytes(GiveSoulsFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x17, 8);
            Execute(asm);
        }
        private void UpdateSoulLevel()
        {
            var charClass = DS2SClass.All.FirstOrDefault(c => c.ID == Class);
            if (charClass == null) return;

            var soulLevel = GetSoulLevel(charClass);
            SoulLevel = soulLevel;
            var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);
            if (reqSoulMemory > SoulMemory)
            {
                SoulMemory = reqSoulMemory;
                SoulMemory2 = reqSoulMemory;
            }
        }
        private int GetSoulLevel(DS2SClass charClass)
        {
            int sl = charClass.SoulLevel;
            sl += Vigor - charClass.Vigor;
            sl += Attunement - charClass.Attunement;
            sl += Vitality - charClass.Vitality;
            sl += Endurance - charClass.Endurance;
            sl += Strength - charClass.Strength;
            sl += Dexterity - charClass.Dexterity;
            sl += Adaptability - charClass.Adaptability;
            sl += Intelligence - charClass.Intelligence;
            sl += Faith - charClass.Faith;
            return sl;
        }
        public void ResetSoulMemory()
        {
            var charClass = DS2SClass.All.FirstOrDefault(c => c.ID == Class);
            if (charClass == null) return;

            var soulLevel = GetSoulLevel(charClass);
            var reqSoulMemory = GetRequiredSoulMemory(soulLevel, charClass.SoulLevel);

            SoulMemory = reqSoulMemory;
            SoulMemory2 = reqSoulMemory;
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

        public static List<int> Levels = new();
        private void GetLevelRequirements()
        {
            if (ParamMan.PlayerLevelUpSoulsParam == null)
                throw new NullReferenceException("Level up cost param not found");

            foreach (var row in ParamMan.PlayerLevelUpSoulsParam.Rows.Cast<PlayerLevelUpSoulsRow>())
                Levels.Add(row.LevelCost);
        }

        #endregion

        #region Items
        public void GiveItem_wrapper(int item, short amount, byte upgrade, byte infusion)
        {
            if (Properties.Settings.Default.SilentItemGive)
                GiveItemSilently(item, amount, upgrade, infusion);
            else
                GiveItem(item, amount, upgrade, infusion);
        }
        public void GiveItems(int[] itemids, short[] amounts)
        {
            // Fix wrapping for optionals
            var len = itemids.Length;
            var upgrades_list = new List<byte>(len);
            var infusions_list = new List<byte>(len);
            for (int i = 0; i< len; i++)
            {
                upgrades_list.Add(0);
                infusions_list.Add(0);
            }

            byte[] upgrades = upgrades_list.ToArray();
            byte[] infusions = infusions_list.ToArray();

            // Call function
            GiveItems(itemids, amounts, upgrades, infusions);
        }
        public void GiveItems(int[] itemids, short[] amounts, byte[] upgrades, byte[] infusions)
        {
            int numitems = itemids.Length;
            if (numitems > 8)
                throw new Exception("Item Give function in DS2 can only handle 8 items at a time");
            
            // Create item structure to pass to DS2
            var itemStruct = Allocate(0x8A); // should this be d128?
            for (int i = 0; i< itemids.Length; i++)
            {
                var offi = i * 16; // length of one itemStruct
                Kernel32.WriteBytes(Handle, itemStruct + offi + 0x4, BitConverter.GetBytes(itemids[i]));
                Kernel32.WriteBytes(Handle, itemStruct + offi + 0x8, BitConverter.GetBytes(float.MaxValue));
                Kernel32.WriteBytes(Handle, itemStruct + offi + 0xC, BitConverter.GetBytes(amounts[i]));
                Kernel32.WriteByte(Handle, itemStruct + offi + 0xE, upgrades[i]);
                Kernel32.WriteByte(Handle, itemStruct + offi + 0xF, infusions[i]);
            }

            // Fix assembly
            var asm = (byte[])DS2SAssembly.GetItem.Clone();

            var bytes = BitConverter.GetBytes(numitems);
            Array.Copy(bytes, 0, asm, 0x9, 4);
            bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            Array.Copy(bytes, 0, asm, 0xF, 8);
            bytes = BitConverter.GetBytes(AvailableItemBag.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x1C, 8);
            bytes = BitConverter.GetBytes(ItemGiveFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x29, 8);
            bytes = BitConverter.GetBytes(numitems);
            Array.Copy(bytes, 0, asm, 0x36, 4);
            bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            Array.Copy(bytes, 0, asm, 0x3C, 8);
            bytes = BitConverter.GetBytes(ItemStruct2dDisplay.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x54, 8);
            bytes = BitConverter.GetBytes(ItemGiveWindow.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x66, 8);
            bytes = BitConverter.GetBytes(DisplayItem.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x70, 8);

            Execute(asm);
            Free(itemStruct);
        }

        private void GiveItem(int item, short amount, byte upgrade, byte infusion)
        {
            var itemStruct = Allocate(0x8A); // should this be d128?

            Kernel32.WriteBytes(Handle, itemStruct + 0x4, BitConverter.GetBytes(item));
            Kernel32.WriteBytes(Handle, itemStruct + 0x8, BitConverter.GetBytes(float.MaxValue));
            Kernel32.WriteBytes(Handle, itemStruct + 0xC, BitConverter.GetBytes(amount));
            Kernel32.WriteByte(Handle, itemStruct + 0xE, upgrade);
            Kernel32.WriteByte(Handle, itemStruct + 0xF, infusion);

            var asm = (byte[])DS2SAssembly.GetItem.Clone();

            var bytes = BitConverter.GetBytes(0x1);
            Array.Copy(bytes, 0, asm, 0x9, 4);
            bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            Array.Copy(bytes, 0, asm, 0xF, 8);
            bytes = BitConverter.GetBytes(AvailableItemBag.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x1C, 8);
            bytes = BitConverter.GetBytes(ItemGiveFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x29, 8);
            bytes = BitConverter.GetBytes(0x1);
            Array.Copy(bytes, 0, asm, 0x36, 4);
            bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            Array.Copy(bytes, 0, asm, 0x3C, 8);
            bytes = BitConverter.GetBytes(ItemStruct2dDisplay.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x54, 8);
            bytes = BitConverter.GetBytes(ItemGiveWindow.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x66, 8);
            bytes = BitConverter.GetBytes(DisplayItem.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x70, 8);

            Execute(asm);
            Free(itemStruct);
        }
        public void GiveItemSilently(int item, short amount, byte upgrade, byte infusion)
        {
            var itemStruct = Allocate(0x8A);
            Kernel32.WriteBytes(Handle, itemStruct + 0x4, BitConverter.GetBytes(item));
            Kernel32.WriteBytes(Handle, itemStruct + 0x8, BitConverter.GetBytes(float.MaxValue));
            Kernel32.WriteBytes(Handle, itemStruct + 0xC, BitConverter.GetBytes(amount));
            Kernel32.WriteByte(Handle, itemStruct + 0xE, upgrade);
            Kernel32.WriteByte(Handle, itemStruct + 0xF, infusion);

            var asm = (byte[])DS2SAssembly.GetItemNoMenu.Clone();

            var bytes = BitConverter.GetBytes(0x1);
            Array.Copy(bytes, 0, asm, 0x6, 4);
            bytes = BitConverter.GetBytes(itemStruct.ToInt64());
            Array.Copy(bytes, 0, asm, 0xC, 8);
            bytes = BitConverter.GetBytes(AvailableItemBag.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x19, 8);
            bytes = BitConverter.GetBytes(ItemGiveFunc.Resolve().ToInt64());
            Array.Copy(bytes, 0, asm, 0x26, 8);

            Execute(asm);
            Free(itemStruct);
        }
        #endregion

        #region Params
        
        internal int GetMaxUpgrade(ItemRow item)
        {
            if (!ParamMan.IsLoaded)
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

            var endPointer = AvailableItemBag.ReadIntPtr(0x10).ToInt64();
            var bagSize = endPointer - AvailableItemBag.Resolve().ToInt64();

            var inventory = AvailableItemBag.ReadBytes(0x0, (uint)bagSize);

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
            return 0;
        }

        private const int tbo = 0x7A8; // table bonus offset
        public enum BNSTYPE
        {
            STR = 0,
            DEX = 1,
            MAGIC = 2,
            FIRE = 3,
            LIGHTNING = 4,
            DARK = 5,
            POISON = 6,
            BLEED = 7,
        }
        public int GetBonus(BNSTYPE bnstype)
        {
            if (!Hooked) return 0;
            if (SomePlayerStats == null) return 0;
            return SomePlayerStats.ReadInt32(tbo + 36*(int)bnstype);
        }

        
        internal void GetVanillaItems()
        {
            if (ParamMan.ItemParam == null)
                throw new NullReferenceException("Should be loaded by this point I think");
            Items = ParamMan.ItemParam.Rows.OfType<ItemRow>().ToList();
            
            foreach(var item in Items)
            {
                var temp = DS2SItemCategory.AllItems.Where(ds2item => ds2item.ID == item.ID).FirstOrDefault();
                if (temp == null)
                    continue;
                item.MetaItemName = temp.Name;
            }
            return;
        }
        

        // TODO ARCHAIC
        internal bool GetIsDroppable(ItemRow item)
        {
            if (!ParamMan.IsLoaded) 
                return false;

            if (!Setup || ParamMan.ItemUsageParam == null) 
                return false;
            
            if (item == null)
                throw new Exception("Cannot find item for GetIsDroppable");

            bool? drp = item.ItemUsageRow?.IsDroppable;
            if (drp == null)
                return false;
            return (bool)drp;
        }
        
        #endregion

        #region Bonfires
        public byte FireKeepersDwelling
        {
            get 
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.FireKeepersDwelling);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.FireKeepersDwelling, level);
            }
        }

        public byte TheFarFire
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheFarFire);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheFarFire, level);
            }
        }
        public byte TheCrestfallensRetreat
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CrestfallensRetreat);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CrestfallensRetreat, level);
            }
        }
        public byte CardinalTower
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CardinalTower);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CardinalTower, level);
            }
        }
        public byte SoldiersRest
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.SoldiersRest);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.SoldiersRest, level);
            }
        }
        public byte ThePlaceUnbeknownst
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ThePlaceUnbeknownst);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ThePlaceUnbeknownst, level);
            }
        }
        public byte HeidesRuin
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.HeidesRuin);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.HeidesRuin, level);
            }
        }
        public byte TowerofFlame
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TowerofFlame);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TowerofFlame, level);
            }
        }
        public byte TheBlueCathedral
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheBlueCathedral);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheBlueCathedral, level);
            }
        }
        public byte UnseenPathtoHeide
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UnseenPathtoHeide);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UnseenPathtoHeide, level);
            }
        }
        public byte ExileHoldingCells
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ExileHoldingCells);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ExileHoldingCells, level);
            }
        }
        public byte McDuffsWorkshop
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.McDuffsWorkshop);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.McDuffsWorkshop, level);
            }
        }
        public byte ServantsQuarters
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ServantsQuarters);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ServantsQuarters, level);
            }
        }
        public byte StraidsCell
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.StraidsCell);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.StraidsCell, level);
            }
        }
        public byte TheTowerApart
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheTowerApart);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheTowerApart, level);
            }
        }
        public byte TheSaltfort
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheSaltfort);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheSaltfort, level);
            }
        }
        public byte UpperRamparts
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UpperRamparts);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UpperRamparts, level);
            }
        }
        public byte UndeadRefuge
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UndeadRefuge);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UndeadRefuge, level);
            }
        }
        public byte BridgeApproach
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.BridgeApproach);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.BridgeApproach, level);
            }
        }
        public byte UndeadLockaway
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UndeadLockaway);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UndeadLockaway, level);
            }
        }
        public byte UndeadPurgatory
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UndeadPurgatory);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UndeadPurgatory, level);
            }
        }
        public byte PoisonPool
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.PoisonPool);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.PoisonPool, level);
            }
        }
        public byte TheMines
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheMines);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheMines, level);
            }
        }
        public byte LowerEarthenPeak
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.LowerEarthenPeak);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.LowerEarthenPeak, level);
            }
        }
        public byte CentralEarthenPeak
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CentralEarthenPeak);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CentralEarthenPeak, level);
            }
        }
        public byte UpperEarthenPeak
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UpperEarthenPeak);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UpperEarthenPeak, level);
            }
        }
        public byte ThresholdBridge
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ThresholdBridge);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ThresholdBridge, level);
            }
        }
        public byte IronhearthHall
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.IronhearthHall);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.IronhearthHall, level);
            }
        }
        public byte EygilsIdol
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.EygilsIdol);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.EygilsIdol, level);
            }
        }
        public byte BelfrySolApproach
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.BelfrySolApproach);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.BelfrySolApproach, level);
            }
        }
        public byte OldAkelarre
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.OldAkelarre);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.OldAkelarre, level);
            }
        }
        public byte RuinedForkRoad
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.RuinedForkRoad);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.RuinedForkRoad, level);
            }
        }
        public byte ShadedRuins
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ShadedRuins);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ShadedRuins, level);
            }
        }
        public byte GyrmsRespite
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.GyrmsRespite);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.GyrmsRespite, level);
            }
        }
        public byte OrdealsEnd
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.OrdealsEnd);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.OrdealsEnd, level);
            }
        }
        public byte RoyalArmyCampsite
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.RoyalArmyCampsite);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.RoyalArmyCampsite, level);
            }
        }
        public byte ChapelThreshold
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ChapelThreshold);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ChapelThreshold, level);
            }
        }
        public byte LowerBrightstoneCove
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.LowerBrightstoneCove);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.LowerBrightstoneCove, level);
            }
        }
        public byte HarvalsRestingPlace
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.HarvalsRestingPlace);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.HarvalsRestingPlace, level);
            }
        }
        public byte GraveEntrance
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.GraveEntrance);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.GraveEntrance, level);
            }
        }
        public byte UpperGutter
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UpperGutter);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UpperGutter, level);
            }
        }
        public byte CentralGutter
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CentralGutter);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CentralGutter, level);
            }
        }
        public byte HiddenChamber
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.HiddenChamber);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.HiddenChamber, level);
            }
        }
        public byte BlackGulchMouth
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.BlackGulchMouth);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.BlackGulchMouth, level);
            }
        }
        public byte KingsGate
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.KingsGate);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.KingsGate, level);
            }
        }
        public byte UnderCastleDrangleic
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UnderCastleDrangleic);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UnderCastleDrangleic, level);
            }
        }
        public byte ForgottenChamber
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ForgottenChamber);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ForgottenChamber, level);
            }
        }
        public byte CentralCastleDrangleic
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CentralCastleDrangleic);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CentralCastleDrangleic, level);
            }
        }
        public byte TowerofPrayer
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TowerofPrayer);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TowerofPrayer, level);
            }
        }
        public byte CrumbledRuins
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.CrumbledRuins);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.CrumbledRuins, level);
            }
        }
        public byte RhoysRestingPlace
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.RhoysRestingPlace);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.RhoysRestingPlace, level);
            }
        }
        public byte RiseoftheDead
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.RiseoftheDead);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.RiseoftheDead, level);
            }
        }
        public byte UndeadCryptEntrance
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UndeadCryptEntrance);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UndeadCryptEntrance, level);
            }
        }
        public byte UndeadDitch
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UndeadDitch);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UndeadDitch, level);
            }
        }
        public byte Foregarden
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.Foregarden);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.Foregarden, level);
            }
        }
        public byte RitualSite
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.RitualSite);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.RitualSite, level);
            }
        }
        public byte DragonAerie
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.DragonAerie);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.DragonAerie, level);
            }
        }
        public byte ShrineEntrance
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ShrineEntrance);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ShrineEntrance, level);
            }
        }
        public byte SanctumWalk
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.SanctumWalk);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.SanctumWalk, level);
            }
        }
        public byte PriestessChamber
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.PriestessChamber);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.PriestessChamber, level);
            }
        }
        public byte HiddenSanctumChamber
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.HiddenSanctumChamber);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.HiddenSanctumChamber, level);
            }
        }
        public byte LairoftheImperfect
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.LairoftheImperfect);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.LairoftheImperfect, level);
            }
        }
        public byte SanctumInterior
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.SanctumInterior);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.SanctumInterior, level);
            }
        }
        public byte TowerofPrayerDLC
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TowerofPrayerDLC);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TowerofPrayerDLC, level);
            }
        }
        public byte SanctumNadir
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.SanctumNadir);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.SanctumNadir, level);
            }
        }
        public byte ThroneFloor
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ThroneFloor);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ThroneFloor, level);
            }
        }
        public byte UpperFloor
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.UpperFloor);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.UpperFloor, level);
            }
        }
        public byte Foyer
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.Foyer);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.Foyer, level);
            }
        }
        public byte LowermostFloor
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.LowermostFloor);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.LowermostFloor, level);
            }
        }
        public byte TheSmelterThrone
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.TheSmelterThrone);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.TheSmelterThrone, level);
            }
        }
        public byte IronHallwayEntrance
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.IronHallwayEntrance);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.IronHallwayEntrance, level);
            }
        }
        public byte OuterWall
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.OuterWall);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.OuterWall, level);
            }
        }
        public byte AbandonedDwelling
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.AbandonedDwelling);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.AbandonedDwelling, level);
            }
        }
        public byte ExpulsionChamber
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.ExpulsionChamber);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.ExpulsionChamber, level);
            }
        }
        public byte InnerWall
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.InnerWall);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.InnerWall, level);
            }
        }
        public byte LowerGarrison
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.LowerGarrison);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.LowerGarrison, level);
            }
        }
        public byte GrandCathedral
        {
            get
            {
                byte level = 0;

                if (Loaded)
                {
                    level = BonfireLevels.ReadByte(Offsets.BonfireLevels.GrandCathedral);
                    level = (byte)((level + 1) / 2);
                }

                return level;
            }
            set
            {
                byte level = 0;
                if (value > 0)
                    level = (byte)(value * 2 - 1);
                BonfireLevels.WriteByte(Offsets.BonfireLevels.GrandCathedral, level);
            }
        }

        public void UnlockBonfires()
        {
            //foreach (DS2SOffsets.BonfireLevels bonfire in Enum.GetValues(typeof(DS2SOffsets.BonfireLevels)))
            var BFlvls = Offsets.BonfireLevels;
            foreach (var f in BFlvls.GetType().GetFields())
            {
                var fval = f.GetValue(BFlvls);
                if (fval == null) 
                    continue;
                int bfoffset = (int)fval;
                if (bfoffset == DS2HookOffsets.UNSET)
                    continue;

                var currentLevel = BonfireLevels.ReadByte((int)bfoffset);

                //if (bonfire == DS2SOffsets.BonfireLevels.FireKeepersDwelling)
                //        continue;

                if (currentLevel == 0)
                    BonfireLevels.WriteByte((int)bfoffset, 1);
            }
        }

        #endregion

        #region Internal

        public string Head
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.Head);

                if (DS2SItem.Items.ContainsKey(itemID + 10000000))
                    return DS2SItem.Items[itemID + 10000000];

                return itemID.ToString();
            }
        }
        public string Chest
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.Chest);

                if (DS2SItem.Items.ContainsKey(itemID + 10000000))
                    return DS2SItem.Items[itemID + 10000000];

                return itemID.ToString();
            }
        }
        public string Arms
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.Arms);

                if (DS2SItem.Items.ContainsKey(itemID + 10000000))
                    return DS2SItem.Items[itemID + 10000000];

                return itemID.ToString();
            }
        }
        public string Legs
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.Legs);

                if (DS2SItem.Items.ContainsKey(itemID + 10000000))
                    return DS2SItem.Items[itemID + 10000000];

                return itemID.ToString();
            }
        }
        public string RightHand1
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.RightHand1);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        public string RightHand2
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.RightHand2);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        public string RightHand3
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.RightHand3);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        public string LeftHand1
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.LeftHand1);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        public string LeftHand2
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.LeftHand2);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        public string LeftHand3
        {
            get
            {
                if (!Loaded) return "";
                var itemID = PlayerCtrl.ReadInt32(Offsets.PlayerEquipment.LeftHand3);

                if (DS2SItem.Items.ContainsKey(itemID))
                    return DS2SItem.Items[itemID];

                return itemID.ToString();
            }
        }
        private bool _speedFactors;
        public bool EnableSpeedFactors
        {
            get => _speedFactors;
            set
            {
                _speedFactors = value;
                AccelerationStamina = value;
                AnimationSpeed = value;
                JumpSpeed = value;
                BuildupSpeed = value;
            }
        }

        private IntPtr AccelSpeedPtr;
        private IntPtr AccelSpeedCodePtr;
        public float AccelSpeed
        {
            get => AccelSpeedPtr != IntPtr.Zero ? BitConverter.ToSingle(Kernel32.ReadBytes(Handle, AccelSpeedPtr, 0x4), 0x0) : Properties.Settings.Default.AccelSpeed;
            set
            {
                if (AccelSpeedPtr != IntPtr.Zero)
                    Kernel32.WriteBytes(Handle, AccelSpeedPtr, BitConverter.GetBytes(value));

                Properties.Settings.Default.AccelSpeed = value;
            }
        }
        private bool _accelerationStamina;
        public bool AccelerationStamina
        {
            get => _accelerationStamina;
            set
            {
                _accelerationStamina = value;
                if (_accelerationStamina)
                    InjectSpeedFactor(SpeedFactorAccel, ref AccelSpeedPtr, ref AccelSpeedCodePtr, (byte[])DS2SAssembly.SpeedFactorAccel.Clone(), Properties.Settings.Default.AccelSpeed);
                else
                {
                    RepairSpeedFactor(SpeedFactorAccel, AccelSpeedPtr, AccelSpeedCodePtr, (byte[])DS2SAssembly.OgSpeedFactorAccel.Clone());
                    AccelSpeedPtr = IntPtr.Zero;
                    AccelSpeedCodePtr = IntPtr.Zero;
                }
            }
        }

        private IntPtr AnimSpeedPtr;
        private IntPtr AnimSpeedCodePtr;
        public float AnimSpeed
        {
            get => AnimSpeedPtr != IntPtr.Zero ? BitConverter.ToSingle(Kernel32.ReadBytes(Handle, AnimSpeedPtr, 0x4), 0x0) : Properties.Settings.Default.AnimSpeed;
            set
            {
                if (AnimSpeedPtr != IntPtr.Zero)
                    Kernel32.WriteBytes(Handle, AnimSpeedPtr, BitConverter.GetBytes(value));

                Properties.Settings.Default.AnimSpeed = value;
            }
        }
        private bool _animationSpeed;
        public bool AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                _animationSpeed = value;
                if (_animationSpeed)
                    InjectSpeedFactor(SpeedFactorAnim, ref AnimSpeedPtr, ref AnimSpeedCodePtr, (byte[])DS2SAssembly.SpeedFactor.Clone(), Properties.Settings.Default.AnimSpeed);
                else
                {
                    RepairSpeedFactor(SpeedFactorAnim, AnimSpeedPtr, AnimSpeedCodePtr, (byte[])DS2SAssembly.OgSpeedFactor.Clone());
                    AnimSpeedPtr = IntPtr.Zero;
                    AnimSpeedCodePtr = IntPtr.Zero;
                }
            }
        }

        private IntPtr JumpSpeedPtr;
        private IntPtr JumpSpeedCodePtr;
        public float JumpSpeedValue
        {
            get => JumpSpeedPtr != IntPtr.Zero ? BitConverter.ToSingle(Kernel32.ReadBytes(Handle, JumpSpeedPtr, 0x4), 0x0) : Properties.Settings.Default.JumpSpeed;
            set
            {
                if (JumpSpeedPtr != IntPtr.Zero)
                    Kernel32.WriteBytes(Handle, JumpSpeedPtr, BitConverter.GetBytes(value));

                Properties.Settings.Default.JumpSpeed = value;
            }
        }
        private bool _jumpSpeed;
        public bool JumpSpeed
        {
            get => _jumpSpeed;
            set
            {
                _jumpSpeed = value;
                if (_jumpSpeed)
                    InjectSpeedFactor(SpeedFactorJump, ref JumpSpeedPtr, ref JumpSpeedCodePtr, (byte[])DS2SAssembly.SpeedFactor.Clone(), Properties.Settings.Default.JumpSpeed);
                else
                {
                    RepairSpeedFactor(SpeedFactorJump, JumpSpeedPtr, JumpSpeedCodePtr, (byte[])DS2SAssembly.OgSpeedFactor.Clone());
                    JumpSpeedPtr = IntPtr.Zero;
                    JumpSpeedCodePtr = IntPtr.Zero;
                }
            }
        }

        private IntPtr BuildupSpeedPtr;
        private IntPtr BuildupSpeedCodePtr;
        public float BuildupSpeedValue
        {
            get => BuildupSpeedPtr != IntPtr.Zero ? BitConverter.ToSingle(Kernel32.ReadBytes(Handle, BuildupSpeedPtr, 0x4), 0x0) : Properties.Settings.Default.BuildupSpeed;
            set
            {
                if (BuildupSpeedPtr != IntPtr.Zero)
                    Kernel32.WriteBytes(Handle, BuildupSpeedPtr, BitConverter.GetBytes(value));

                Properties.Settings.Default.BuildupSpeed = value;
            }
        }
        private bool _buildupSpeed;
        public bool BuildupSpeed
        {
            get => _buildupSpeed;
            set
            {
                _buildupSpeed = value;
                if (_buildupSpeed)
                    InjectSpeedFactor(SpeedFactorBuildup, ref BuildupSpeedPtr, ref BuildupSpeedCodePtr, (byte[])DS2SAssembly.SpeedFactor.Clone(), Properties.Settings.Default.BuildupSpeed);
                else
                {
                    RepairSpeedFactor(SpeedFactorBuildup, BuildupSpeedPtr, BuildupSpeedCodePtr, (byte[])DS2SAssembly.OgSpeedFactor.Clone());
                    BuildupSpeedPtr = IntPtr.Zero;
                    BuildupSpeedCodePtr = IntPtr.Zero;
                }
            }
        }

        private void RepairSpeedFactor(PHPointer speedFactorPointer, IntPtr valuePointer, IntPtr codePointer, byte[] asm)
        {
            speedFactorPointer.WriteBytes(0x0, asm);
            Free(valuePointer);
            Free(codePointer);
        }
        private void InjectSpeedFactor(PHPointer speedFactorPointer, ref IntPtr valuePointer, ref IntPtr codePointer, byte[] asm, float value)
        {
            var inject = new byte[0x11];
            Array.Copy(asm, inject, inject.Length);
            var newCode = new byte[0x18];
            Array.Copy(asm, inject.Length, newCode, 0x0, newCode.Length);

            valuePointer = Allocate(sizeof(float));
            var valuePointerBytes = BitConverter.GetBytes(valuePointer.ToInt64());
            codePointer = Allocate(sizeof(float), Kernel32.PAGE_EXECUTE_READWRITE);
            var codePointerBytes = BitConverter.GetBytes(codePointer.ToInt64());

            Array.Copy(valuePointerBytes, 0x0, newCode, 0x2, valuePointerBytes.Length);
            Array.Copy(codePointerBytes, 0x0, inject, 0x3, valuePointerBytes.Length);

            Kernel32.WriteBytes(Handle, valuePointer, BitConverter.GetBytes(value));
            Kernel32.WriteBytes(Handle, codePointer, newCode);
            speedFactorPointer.WriteBytes(0x0, inject);
        }

        #endregion

        #region Covenant

        public byte CurrentCovenant
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.CurrentCovenant) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.CurrentCovenant, value);
            }
        }
        public string? CurrentCovenantName
        {
            get => Loaded ? DS2SCovenant.All.FirstOrDefault(x => x.ID == CurrentCovenant)?.Name : "";
        }
        public bool HeirsOfTheSunDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.HeirsOfTheSunDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.HeirsOfTheSunDiscovered, value);
            }
        }
        public byte HeirsOfTheSunRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.HeirsOfTheSunRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.HeirsOfTheSunRank, value);
            }
        }
        public short HeirsOfTheSunProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.HeirsOfTheSunProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.HeirsOfTheSunProgress, value);
            }
        }
        public bool BlueSentinelsDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.BlueSentinelsDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.BlueSentinelsDiscovered, value);
            }
        }
        public byte BlueSentinelsRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.BlueSentinelsRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.BlueSentinelsRank, value);
            }
        }
        public short BlueSentinelsProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.BlueSentinelsProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.BlueSentinelsProgress, value);
            }
        }
        public bool BrotherhoodOfBloodDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.BrotherhoodOfBloodDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.BrotherhoodOfBloodDiscovered, value);
            }
        }
        public byte BrotherhoodOfBloodRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.BrotherhoodOfBloodRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.BrotherhoodOfBloodRank, value);
            }
        }
        public short BrotherhoodOfBloodProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.BrotherhoodOfBloodProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.BrotherhoodOfBloodProgress, value);
            }
        }
        public bool WayOfTheBlueDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.WayOfTheBlueDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.WayOfTheBlueDiscovered, value);
            }
        }
        public byte WayOfTheBlueRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.WayOfTheBlueRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.WayOfTheBlueRank, value);
            }
        }
        public short WayOfTheBlueProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.WayOfTheBlueProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.WayOfTheBlueProgress, value);
            }
        }
        public bool RatKingDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.RatKingDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.RatKingDiscovered, value);
            }
        }
        public byte RatKingRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.RatKingRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.RatKingRank, value);
            }
        }
        public short RatKingProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.RatKingProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.RatKingProgress, value);
            }
        }
        public bool BellKeepersDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.BellKeepersDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.BellKeepersDiscovered, value);
            }
        }
        public byte BellKeepersRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.BellKeepersRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.BellKeepersRank, value);
            }
        }
        public short BellKeepersProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.BellKeepersProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.BellKeepersProgress, value);
            }
        }
        public bool DragonRemnantsDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.DragonRemnantsDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.DragonRemnantsDiscovered, value);
            }
        }
        public byte DragonRemnantsRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.DragonRemnantsRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.DragonRemnantsRank, value);
            }
        }
        public short DragonRemnantsProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.DragonRemnantsProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.DragonRemnantsProgress, value);
            }
        }
        public bool CompanyOfChampionsDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.CompanyOfChampionsDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.CompanyOfChampionsDiscovered, value);
            }
        }
        public byte CompanyOfChampionsRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.CompanyOfChampionsRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.CompanyOfChampionsRank, value);
            }
        }
        public short CompanyOfChampionsProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.CompanyOfChampionsProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.CompanyOfChampionsProgress, value);
            }
        }
        public bool PilgrimsOfDarknessDiscovered
        {
            get => Loaded ? PlayerParam.ReadBoolean(Offsets.Covenants.PilgrimsOfDarknessDiscovered) : false;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteBoolean(Offsets.Covenants.PilgrimsOfDarknessDiscovered, value);
            }
        }
        public byte PilgrimsOfDarknessRank
        {
            get => Loaded ? PlayerParam.ReadByte(Offsets.Covenants.PilgrimsOfDarknessRank) : (byte)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteByte(Offsets.Covenants.PilgrimsOfDarknessRank, value);
            }
        }
        public short PilgrimsOfDarknessProgress
        {
            get => Loaded ? PlayerParam.ReadInt16(Offsets.Covenants.PilgrimsOfDarknessProgress) : (short)0;
            set
            {
                if (Reading || !Loaded) return;
                PlayerParam.WriteInt16(Offsets.Covenants.PilgrimsOfDarknessProgress, value);
            }
        }

        #endregion

    }
}
