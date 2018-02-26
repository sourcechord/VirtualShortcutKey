using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualShortcutKey
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnCut(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^x");
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^c");
        }

        private void OnPaste(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^v");
        }

        private void OnUndo(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^z");
        }

        private void OnRedo(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^+z");
        }

        private void OnSelectAll(object sender, RoutedEventArgs e)
        {
            SendKeys.SendWait("^a");
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EnableBlur();

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);



        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            // ウィンドウ背景のぼかしを行うのはWindows10の場合のみ
            accent.AccentState = IsWin10() ? AccentState.ACCENT_ENABLE_BLURBEHIND : AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
            accent.AccentFlags = 2;
            accent.GradientColor = 0x80111111;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        /// <summary>
        /// 実行環境のOSがWindows10か否かを判定
        /// </summary>
        /// <returns></returns>
        internal static bool IsWin10()
        {
            var isWin10 = false;
            using (var mc = new System.Management.ManagementClass("Win32_OperatingSystem"))
            using (var moc = mc.GetInstances())
            {
                foreach (System.Management.ManagementObject mo in moc)
                {
                    var version = mo["Version"] as string;
                    var majar = version.Split('.')
                                       .FirstOrDefault();
                    isWin10 = majar == "10";
                }
            }

            return isWin10;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
