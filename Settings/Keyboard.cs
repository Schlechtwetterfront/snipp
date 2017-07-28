using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace clipman.Settings
{
    /// <summary>
    /// Monitors global shortcuts.
    /// </summary>
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

            /// <summary>
            /// Hotkey event.
            /// </summary>
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

        /// <summary>
        /// List of registered hotkey ids. Used to unregister on dispose.
        /// </summary>
        private List<int> hotkeyIds = new List<int>();

        public KeyboardMonitor()
        {
            hwndSource.AddHook(WndProc);
        }

        /// <summary>
        /// Add a global hotkey.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="key"></param>
        /// <returns>Returns the id used to identify the hotkey in callbacks.</returns>
        public int AddHotkey(int modifier, int key)
        {
            int id = hotkeyIds.Count + 9000;
            hotkeyIds.Add(id);
            NativeMethods.RegisterHotKey(hwndSource.Handle, id, (uint)modifier, (uint)key);
            return id;
        }

        public void RemoveHotkey(int id)
        {
            NativeMethods.UnregisterHotKey(hwndSource.Handle, id);
        }

        /// <summary>
        /// Listen to window events and fire `KeyPressed` when a registered hotkey is pressed.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
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

        public void Dispose(bool cleanupManaged)
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                hwndSource.Dispose();
                foreach (var id in hotkeyIds) {
                    NativeMethods.UnregisterHotKey(hwndSource.Handle, id);
                }
            }
        }

        /// <summary>
        /// Event fired when a hotkey is pressed.
        /// </summary>
        public event EventHandler<HotkeyEventArgs> KeyPressed;
    }
}
