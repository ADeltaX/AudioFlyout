using AudioFlyout.Classes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AudioFlyout
{
    internal static class VolumeSMTC
    {
        public const int MAX_TRIES = 6;

        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        #region P/invoke

        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong));
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ShowWindowAsync(IntPtr windowHandle, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        internal static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyWindow(IntPtr hWnd);

        #endregion

        public static void ForceFindSMTCAndHide()
        {
            int tries = 0;

            while (FindSMTCAndHide() == false && tries++ < MAX_TRIES)
            {
                //Sometimes the volume flyout isn't created yet, so we need to send some a key stroke.
                keybd_event((byte)Keys.VolumeUp, 0, 0, 0);
                keybd_event((byte)Keys.VolumeDown, 0, 0, 0);

                Thread.Sleep(500);
            }
        }

        public static bool FindSMTCAndHide()
        {
            IntPtr hWndHost = IntPtr.Zero;

            while ((hWndHost = FindWindowEx(IntPtr.Zero, hWndHost, "NativeHWNDHost", "")) != IntPtr.Zero)
            {
                IntPtr hWndDUI;
                if ((hWndDUI = FindWindowEx(hWndHost, IntPtr.Zero, "DirectUIHWND", "")) != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hWndHost, out int pid);
                    if (Process.GetProcessById(pid).ProcessName.ToLower() == "explorer")
                    {
                        ShowWindowAsync(hWndDUI, 6);
                        ShowWindowAsync(hWndHost, 11);
                        ShowWindowAsync(hWndDUI, 11);

                        unchecked
                        {
                            int extendedStyle = GetWindowLong(hWndDUI, GWL_STYLE);
                            SetWindowLongPtr(hWndDUI, GWL_STYLE, extendedStyle | (int)WindowInBandWrapper.WindowStyles.WS_POPUP | (int)WindowInBandWrapper.WindowStyles.WS_CLIPCHILDREN);

                            int extendedStyle2 = GetWindowLong(hWndHost, GWL_STYLE);
                            SetWindowLongPtr(hWndHost, GWL_STYLE, extendedStyle2 | (int)WindowInBandWrapper.WindowStyles.WS_POPUP | (int)WindowInBandWrapper.WindowStyles.WS_CLIPCHILDREN);
                        }

                        SetWindowPos(hWndHost, IntPtr.Zero, 0, 0,
                                0,
                                0, 0x0010);

                        SetWindowPos(hWndDUI, IntPtr.Zero, 0, 0,
                                0,
                                0, 0x0010);

                        return true;
                    }
                }
            }

            return false;
        }

        public static void FindSMTCAndShow()
        {
            IntPtr hWndHost = IntPtr.Zero;

            while ((hWndHost = FindWindowEx(IntPtr.Zero, hWndHost, "NativeHWNDHost", "")) != IntPtr.Zero)
            {
                IntPtr hWndDUI;
                if ((hWndDUI = FindWindowEx(hWndHost, IntPtr.Zero, "DirectUIHWND", "")) != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(hWndHost, out int pid);
                    if (Process.GetProcessById(pid).ProcessName.ToLower() == "explorer")
                    {
                        //TODO

                        ShowWindowAsync(hWndHost, 9);
                        ShowWindowAsync(hWndDUI, 9);

                        break;
                    }
                }
            }
        }
    }
}
