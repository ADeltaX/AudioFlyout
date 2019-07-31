using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioFlyout.Classes
{
    delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    public class WindowInBandWrapper
    {
        #region Enums

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        const UInt32 CS_USEDEFAULT = 0x80000000;
        const UInt32 CS_DBLCLKS = 8;
        const UInt32 CS_VREDRAW = 1;
        const UInt32 CS_HREDRAW = 2;
        const UInt32 COLOR_WINDOW = 5;
        const UInt32 COLOR_BACKGROUND = 1;
        const UInt32 IDC_CROSS = 32515;
        const UInt32 WM_DESTROY = 2;
        const UInt32 WM_PAINT = 0x0f;
        const UInt32 WM_DPICHANGED = 0x02E0;
        const UInt32 WM_LBUTTONUP = 0x0202;
        const UInt32 WM_LBUTTONDBLCLK = 0x0203;

        public static class SWP
        {
            public static readonly int
            NOSIZE = 0x0001,
            NOMOVE = 0x0002,
            NOZORDER = 0x0004,
            NOREDRAW = 0x0008,
            NOACTIVATE = 0x0010,
            DRAWFRAME = 0x0020,
            FRAMECHANGED = 0x0020,
            SHOWWINDOW = 0x0040,
            HIDEWINDOW = 0x0080,
            NOCOPYBITS = 0x0100,
            NOOWNERZORDER = 0x0200,
            NOREPOSITION = 0x0200,
            NOSENDCHANGING = 0x0400,
            DEFERERASE = 0x2000,
            ASYNCWINDOWPOS = 0x4000;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        public enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        public enum ZBID
        {
            ZBID_DEFAULT = 0x0,
            ZBID_DESKTOP = 0x1,
            ZBID_UIACCESS = 0x2,
            ZBID_IMMERSIVE_IHM = 0x3,
            ZBID_IMMERSIVE_NOTIFICATION = 0x4,
            ZBID_IMMERSIVE_APPCHROME = 0x5,
            ZBID_IMMERSIVE_MOGO = 0x6,
            ZBID_IMMERSIVE_EDGY = 0x7,
            ZBID_IMMERSIVE_INACTIVEMOBODY = 0x8,
            ZBID_IMMERSIVE_INACTIVEDOCK = 0x9,
            ZBID_IMMERSIVE_ACTIVEMOBODY = 0xA,
            ZBID_IMMERSIVE_ACTIVEDOCK = 0xB,
            ZBID_IMMERSIVE_BACKGROUND = 0xC,
            ZBID_IMMERSIVE_SEARCH = 0xD,
            ZBID_GENUINE_WINDOWS = 0xE,
            ZBID_IMMERSIVE_RESTRICTED = 0xF,
            ZBID_SYSTEM_TOOLS = 0x10,
            ZBID_LOCK = 0x11,
            ZBID_ABOVELOCK_UX = 0x12,
        };

        [Flags]
        public enum WindowStylesEx : uint
        {
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_NOACTIVATE = 0x08000000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_WINDOWEDGE = 0x00000100
        }

        [Flags]
        public enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000u,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000
        }

        #endregion

        #region Platform invoke

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        internal static extern int GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ShowWindowAsync(IntPtr windowHandle, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyWindow(IntPtr hWnd);


        [DllImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowEx")]
        public static extern IntPtr CreateWindowEx(int dwExStyle,
               //UInt16 regResult,
               [MarshalAs(UnmanagedType.LPWStr)] string lpClassName,
               [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
               UInt32 dwStyle,
               int x,
               int y,
               int nWidth,
               int nHeight,
               IntPtr hWndParent,
               IntPtr hMenu,
               IntPtr hInstance,
               IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowInBand")]
        public static extern IntPtr CreateWindowInBand(int dwExStyle,
                    ushort atomBomb,
                    [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName,
                    uint dwStyle,
                    int x,
                    int y,
                    int nWidth,
                    int nHeight,
                    IntPtr hWndParent,
                    IntPtr hMenu,
                    IntPtr hInstance,
                    IntPtr lpParam,
                    int dwBand);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassEx")]
        static extern System.UInt16 RegisterClassEx([In] ref WNDCLASSEX lpWndClass);


        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        #endregion

        private double _DPI;

        private DispatcherTimer _elapsedTimer;
        private bool _isVisible;

        Window _window;
        IntPtr _inBandWindowHandle;
        private readonly WndProc delegWndProc;

        public WindowInBandWrapper(Window window)
        {
            _window = window;
            delegWndProc = myWndProc;

            _DPI = 1;

            _window.PreviewMouseUp += Window_PreviewMouseUp;
            _window.PreviewStylusUp += Window_PreviewStylusUp;
            _window.PreviewMouseDown += Window_PreviewMouseDown;
            _window.PreviewStylusDown += Window_PreviewStylusDown;
            _window.DpiChanged += _window_DpiChanged;

            if (_window.SizeToContent != SizeToContent.Manual)
            {
                (_window.Content as FrameworkElement).SizeChanged += _window_SizeChanged;
            }
            else
            {
                _window.SizeChanged += _window_SizeChanged;
            }

            SetupHideTimer();
        }

        private void _window_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            _DPI = e.NewDpi.DpiScaleX;
            UpdateSize();
        }

        public void SetWindowPosition(int x, int y)
        {
            SetWindowPos(_inBandWindowHandle, IntPtr.Zero, x, y,
                0,
                0,
                SWP.NOZORDER | SWP.NOSIZE);

            UpdateWindow(_inBandWindowHandle);
        }

        public void CreateWindowInBand()
        {
            WNDCLASSEX wind_class = new WNDCLASSEX();
            wind_class.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wind_class.hbrBackground = (IntPtr)1 + 1;
            wind_class.cbClsExtra = 0;
            wind_class.cbWndExtra = 0;
            wind_class.hInstance = Process.GetCurrentProcess().Handle;
            wind_class.hIcon = IntPtr.Zero;
            wind_class.lpszMenuName = _window.Title;
            wind_class.lpszClassName = "WIB_" + Guid.NewGuid();
            wind_class.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegWndProc);
            wind_class.hIconSm = IntPtr.Zero;
            ushort regResult = RegisterClassEx(ref wind_class);

            if (regResult == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            _window.ShowActivated = true;

#if RELEASE_UIACCESS
            //Remember to change uiaccess to "true" in app.manifest

            IntPtr hWnd = CreateWindowInBand(
                    (int)(WindowStylesEx.WS_EX_TOPMOST | WindowStylesEx.WS_EX_TRANSPARENT | WindowStylesEx.WS_EX_NOACTIVATE | WindowStylesEx.WS_EX_TOOLWINDOW),
                    regResult,
                    _window.Title,
                    (uint)WindowStyles.WS_POPUP,
                    0,
                    0,
                    (int)0,
                    (int)0,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    wind_class.hInstance,
                    IntPtr.Zero,
                    (int)ZBID.ZBID_UIACCESS);

#else
            //Remember to change uiaccess to "false" in app.manifest

            IntPtr hWnd = CreateWindowInBand(
                    (int)(WindowStylesEx.WS_EX_TOPMOST | WindowStylesEx.WS_EX_TRANSPARENT | WindowStylesEx.WS_EX_NOACTIVATE | WindowStylesEx.WS_EX_TOOLWINDOW),
                    regResult,
                    _window.Title,
                    (uint)WindowStyles.WS_POPUP,
                    0,
                    0,
                    (int)0,
                    (int)0,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    wind_class.hInstance,
                    IntPtr.Zero,
                    (int)ZBID.ZBID_DEFAULT);

#endif

            if (hWnd == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            new AcrylicBlur(hWnd).EnableBlur();

            HwndSourceParameters param = new HwndSourceParameters
            {
                WindowStyle = 0x10000000 | 0x40000000,
                ParentWindow = hWnd,
                UsesPerPixelOpacity = true
            };

            HwndSource src = new HwndSource(param)
            {
                RootVisual = (Visual)_window.Content
            };
            src.CompositionTarget.BackgroundColor = Colors.Transparent;

            src.ContentRendered += Src_ContentRendered;

            UpdateWindow(hWnd);

            _inBandWindowHandle = hWnd;
        }

        public void DragMove()
        {
            //Hide window will stop working
            //Also there is a bug in Windows 10 18362 that makes the window slooooow to move (same as emoji panel)

            //ReleaseCapture();
            //SendMessage(_inBandWindowHandle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        public void MouseEnter() => _elapsedTimer.Stop();

        public void MouseLeave() => _elapsedTimer.Start();


        private void Src_ContentRendered(object sender, EventArgs e)
        {
            _DPI = GetDpiForWindow(_inBandWindowHandle) / 96D;
            UpdateSize();
        }

        private IntPtr myWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_PAINT:
                    break;

                case WM_LBUTTONDBLCLK:
                    break;

                case WM_DESTROY:
                    DestroyWindow(hWnd);
                    break;

                case WM_DPICHANGED:
                    _DPI = (wParam.ToInt32() & 0xFFFF) / 96D;
                    UpdateSize();
                    break;

                default:
                    break;
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void UpdateSize()
        {
            SetWindowPos(_inBandWindowHandle, IntPtr.Zero, 0, 0,
                (int)((_window.Content as FrameworkElement).ActualWidth * _DPI),
                (int)((_window.Content as FrameworkElement).ActualHeight * _DPI),
                SWP.NOZORDER | SWP.NOMOVE);

            UpdateWindow(_inBandWindowHandle);
        }

        private void _window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateSize();

        #region Custom integration

        private void SetupHideTimer()
        {
#if DEBUG
            _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
#else
            _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
#endif

            _elapsedTimer.Tick += (_, __) =>
            {
                _elapsedTimer.Stop();
                Hide();
            };
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _elapsedTimer.Stop();
            _elapsedTimer.Start();
        }

        private void Window_PreviewStylusUp(object sender, StylusEventArgs e)
        {
            _elapsedTimer.Stop();
            _elapsedTimer.Start();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) => _elapsedTimer.Stop();

        private void Window_PreviewStylusDown(object sender, StylusDownEventArgs e) => _elapsedTimer.Stop();

        public void Show()
        {
            ShowWindowAsync(_inBandWindowHandle, (int)ShowWindowCommands.Show);
            _elapsedTimer.Stop();
            _elapsedTimer.Start();
            _isVisible = true;
        }

        public void Hide()
        {
            ShowWindowAsync(_inBandWindowHandle, (int)ShowWindowCommands.Hide);
            _isVisible = false;
        }

        #endregion
    }
}
