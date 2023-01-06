using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DesktopWPFAppLowLevelKeyboardHook
{
    public class LowLevelKeyboardListener
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public event EventHandler<KeyPressedArgs>? OnKeyPressed;

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        public bool IsHooked = false;

        public LowLevelKeyboardListener()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookID = SetHook(_proc);
            IsHooked= true;
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
            IsHooked= false;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            if (curProcess == null) return default;
            using ProcessModule? curModule = curProcess.MainModule;
            if (curModule?.ModuleName == null) return default;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                OnKeyPressed?.Invoke(this, new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(vkCode)));
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }

    public class KeyPressedArgs : EventArgs
    {
        public Key KeyPressed { get; private set; }

        public KeyPressedArgs(Key key)
        {
            KeyPressed = key;
        }
    }
}