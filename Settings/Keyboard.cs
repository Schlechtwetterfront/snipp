using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace clipman.Settings
{
    public static class Keyboard
    {
        public static Key ClearTextboxKey = Key.Escape;
    }

    public class KeyboardMonitor : IDisposable
    {
        private bool disposed = false;

        private static class NativeMethods
        {
            [DllImport("User32.dll")]
            public static extern bool RegisterHotKey(
                [In] IntPtr hWnd,
                [In] int id,
                [In] uint fsModifiers,
                [In] uint vk
            );

            [DllImport("User32.dll")]
            public static extern bool UnregisterHotKey(
                [In] IntPtr hWnd,
                [In] int id
            );

            public const int WM_HOTKEY = 0x0312;

            /// <summary>
            /// To find message-only windows, specify HWND_MESSAGE in the hwndParent parameter of the FindWindowEx function.
            /// </summary>
            public static IntPtr HWND_MESSAGE = new IntPtr(-3);
        }

        public class HotkeyEventArgs : EventArgs
        {
            public int HotkeyId
            {
                get;
                set;
            }
        }

        private HwndSource hwndSource = new HwndSource(0, 0, 0, 0, 0, 0, 0, null, NativeMethods.HWND_MESSAGE);

        private List<int> hotkeyIds = new List<int>();

        public KeyboardMonitor()
        {
            hwndSource.AddHook(WndProc);
        }

        public int AddHotkey(int modifier, int key)
        {
            int id = hotkeyIds.Count + 9000;
            hotkeyIds.Add(id);
            NativeMethods.RegisterHotKey(hwndSource.Handle, id, (uint)modifier, (uint)key);
            return id;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY)
            {
                Utility.Logging.Log("WM_HOTKEY");
                if (hotkeyIds.Contains(wParam.ToInt32()))
                {
                    handled = true;
                    var args = new HotkeyEventArgs();
                    args.HotkeyId = wParam.ToInt32();
                    KeyPressed?.Invoke(this, args);
                }
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var id in hotkeyIds) {
                    NativeMethods.UnregisterHotKey(hwndSource.Handle, id);
                }
            }
        }

        public event EventHandler<HotkeyEventArgs> KeyPressed;
    }
}
