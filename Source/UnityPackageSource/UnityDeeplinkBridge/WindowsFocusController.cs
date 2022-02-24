#if UNITY_EDITOR_WIN
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UnityDeeplinkBridging
{
    internal class WindowsFocusController : IFocusController
    {
        private const int ALT = 0xA4;
        private const int EXTENDEDKEY = 0x1;
        private const int KEYUP = 0x2;
        private IntPtr _toOpen;

        public void Initialize()
        {
            _toOpen = GetActiveWindow();
            UnityEngine.Debug.Log("Initialized " + _toOpen);
        }
        
        public void FocusThis()
        {
            ShowWindowAsync(new HandleRef(null, _toOpen), 9);
            // Simulate alt press
            keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);
            // Simulate alt release
            keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);
            SwitchToThisWindow(_toOpen, true);
            SetForegroundWindow(_toOpen);
        }

        [DllImport("User32.dll", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")] 
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
#endif