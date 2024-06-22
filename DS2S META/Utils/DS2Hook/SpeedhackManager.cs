using mrousavy;
using PropertyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DS2S_META.Utils.DS2Hook
{
    public class SpeedhackManager
    {
        private enum INJECTOR_ERRCODE
        {
            PROCESS_NOT_START = -5,
            INCORRECT_NUMARGS = -2,
            HOOK_FAILED = -1,
            FILENOTFOUND = -6,
            NONE = 0,
        }

        private readonly DS2SHook Hook;
        
        // Constructor
        public SpeedhackManager(DS2SHook hook)
        {
            Hook = hook;
        }
        
        // Interface
        public bool SpeedhackInitialised = false;
        public bool SpeedhackEverEnabled = false;

        // Directory
        public static readonly string ExeDir = Environment.CurrentDirectory;
        private static readonly string SpeedhackDllPathX64 = $"{GetTxtResourceClass.ExeDir}/Resources/DLLs/x64/Speedhack.dll";
        private static readonly string SpeedhackDllPathX86 = $"{GetTxtResourceClass.ExeDir}/Resources/DLLs/x86/Speedhack.dll";
        private static readonly string TempDir = $"{ExeDir}\\Resources\\temp";

        // Pointers to populate
        private IntPtr SpeedhackDllPtr;
        private IntPtr SetupPtr;
        private IntPtr SetSpeedPtr;
        private IntPtr DetachPtr;

        public void Setup()
        {
            if (!Hook.Hooked)
                return; // should be impossible
            GetSpeedhackOffsets();
            ClearupDuplicateSpeedhacks();
        }

        internal void Speedhack(bool enable)
        {
            if (enable)
                EnableSpeedhack();
            else
                DisableSpeedhack();
        }
        internal void ClearSpeedhackInject()
        {
            if (!SpeedhackInitialised)
                return;

            DetachSpeedhack(); // this is still very slow!
            SpeedhackDllPtr = IntPtr.Zero;
            SpeedhackInitialised = false;
        }
        public void DisableSpeedhack()
        {
            if (!SpeedhackInitialised)
                return;
            if (!Hook.Hooked)
                return; // game already closed
            SetSpeed(1.0d);
        }
        
        public static void ClearupDuplicateSpeedhacks()
        {
            // Try running on startup before injects where possible
            if (!Directory.Exists(TempDir))
                return;

            var files = Directory.GetFiles(TempDir);
            foreach (var file in files)
            {
                var fname = Path.GetFileNameWithoutExtension(file);
                bool endswithdigit = char.IsDigit(fname[^1]);
                if (endswithdigit)
                {
                    try
                    {
                        var fpath = $"{TempDir}/{fname}.dll";
                        File.Delete(fpath);
                    }
                    catch (IOException)
                    {
                        continue; // in use probably
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue; // probably ok too?
                    }
                }
            }
        }
        private void EnableSpeedhack()
        {
            if (SpeedhackDllPtr == IntPtr.Zero)
                SpeedhackDllPtr = GetSpeedhackPtr();

            // Exit gracefully on error:
            if (SpeedhackDllPtr == IntPtr.Zero)
                return;

            SpeedhackEverEnabled = true;
            if (!SpeedhackInitialised)
                SetupSpeedhack();

            // Update speed:
            SetSpeed((double)Properties.Settings.Default.SpeedValue);
        }
        private void SetupSpeedhack()
        {
            // Initialise Speedhack (one-time)
            IntPtr setup = (IntPtr)(SpeedhackDllPtr.ToInt64() + SetupPtr.ToInt64());
            IntPtr thread = Kernel32.CreateRemoteThread(Hook.Handle, IntPtr.Zero, 0, setup, IntPtr.Zero, 0, IntPtr.Zero);
            _ = Kernel32.WaitForSingleObject(thread, uint.MaxValue);

            SpeedhackInitialised = true;
        }
        private IntPtr GetSpeedhackPtr()
        {
            if (Hook.Is64Bit)
                return Hook.InjectDLL(SpeedhackDllPathX64);
            else
            {
                var injloc = (IntPtr)Run32BitInjector(SpeedhackDllPathX86, out INJECTOR_ERRCODE errcode);
                if (errcode != 0)
                    return IntPtr.Zero;
                return injloc;
            }
        }

        
        private static int Run32BitInjector(string dllfile, out INJECTOR_ERRCODE err)
        {
            string newpath;
            int fid = Properties.Settings.Default.SH32FID; // get freefile()
            Properties.Settings.Default.SH32FID++; // update it for next time

            // copy dll to new file before injecting
            string dllfilename = Path.GetFileNameWithoutExtension(dllfile);
            string newname = $"{dllfilename}{fid}.dll";
            newpath = $"{TempDir}\\{newname}";

            if (!Directory.Exists(TempDir))
                Directory.CreateDirectory(TempDir);

            File.Copy(dllfile, newpath, true);


            err = INJECTOR_ERRCODE.NONE;
            string TRIPQUOT = "\"\"\"";

            // Run the above batch file in new thread
            ProcessStartInfo PSI = new()
            {
                FileName = $"{ExeDir}\\Resources\\Tools\\SpeedInjector32\\SpeedInjector.exe",
                Arguments = $"{TRIPQUOT}{newpath}{TRIPQUOT}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using Process? exeProcess = Process.Start(PSI);
                exeProcess?.WaitForExit();
                if (exeProcess?.ExitCode == null)
                {
                    err = INJECTOR_ERRCODE.PROCESS_NOT_START;
                    return 0;
                }
                return exeProcess.ExitCode; // -1 on null?
            }
            catch
            {
                // Log error.
                MetaExceptionStaticHandler.Raise("Probably cannot find .exe");
                return (int)INJECTOR_ERRCODE.FILENOTFOUND;
            }
        }

        public void SetSpeed(double value)
        {
            IntPtr setSpeed = (IntPtr)(SpeedhackDllPtr.ToInt64() + SetSpeedPtr.ToInt64());
            IntPtr valueAddress = Hook.GetPrefferedIntPtr(sizeof(double), SpeedhackDllPtr);
            Kernel32.WriteBytes(Hook.Handle, valueAddress, BitConverter.GetBytes(value));
            IntPtr thread = Kernel32.CreateRemoteThread(Hook.Handle, IntPtr.Zero, 0, setSpeed, valueAddress, 0, IntPtr.Zero);
            _ = Kernel32.WaitForSingleObject(thread, uint.MaxValue);
            Hook.Free(valueAddress);
        }
        private void DetachSpeedhack()
        {
            if (!Hook.Is64Bit)
            {
                DisableSpeedhack();
                return; // unattach at game close in Vanilla
            }

            // avoid sotfs Meta-reload crash:
            IntPtr detach = (IntPtr)(SpeedhackDllPtr.ToInt64() + DetachPtr.ToInt64());
            IntPtr thread = Kernel32.CreateRemoteThread(Hook.Handle, IntPtr.Zero, 0, detach, IntPtr.Zero, 0, IntPtr.Zero);
            _ = Kernel32.WaitForSingleObject(thread, uint.MaxValue);
            Hook.Free(SpeedhackDllPtr);
        }

        // Currently hardcoded to avoid hassle. Shouldn't change often :/
        private const int SpeedHack32_SetupOffset = 0x1180;
        private const int SpeedHack32_SpeedOffset = 0x1280;
        private const int SpeedHack32_DetachOffset = 0x1230;

        private void GetSpeedhackOffsets()
        {
            if (Hook.Is64Bit)
            {
                var lib = Kernel32.LoadLibrary(SpeedhackDllPathX64);
                var setupOffset = Kernel32.GetProcAddress(lib, "Setup").ToInt64() - lib.ToInt64();
                var setSpeedOffset = Kernel32.GetProcAddress(lib, "SetSpeed").ToInt64() - lib.ToInt64();
                var detachOffset = Kernel32.GetProcAddress(lib, "Detach").ToInt64() - lib.ToInt64();
                SetupPtr = (IntPtr)setupOffset; // 0x1180
                SetSpeedPtr = (IntPtr)setSpeedOffset; // 0x1280
                DetachPtr = (IntPtr)detachOffset; // 0x1230
                Hook.Free(lib);
            }
            else
            {
                SetupPtr = (IntPtr)SpeedHack32_SetupOffset;
                SetSpeedPtr = (IntPtr)SpeedHack32_SpeedOffset;
                DetachPtr = (IntPtr)SpeedHack32_DetachOffset;
            }

        }
    }
}
