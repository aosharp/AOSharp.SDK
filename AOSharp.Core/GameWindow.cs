using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using AOSharp.Core.UI;

namespace AOSharp.Core
{
    /// <summary>
    /// The client's own window: whether it is in front, minimized, and where
    /// its game area is on screen.
    ///
    /// Read once per frame on the game thread and raised as <see cref="Changed"/>
    /// only when something differs, so plugins subscribe instead of polling.
    /// A WinEvent hook would avoid the per-frame read, but only the thread that
    /// set a hook can remove it, and a plugin reload tears Core down from the
    /// IPC thread; the hook would outlive the assembly it calls into.
    /// </summary>
    public static class GameWindow
    {
        public static IntPtr Handle { get; private set; }
        public static bool IsForeground { get; private set; }
        public static bool IsMinimized { get; private set; }

        /// <summary>
        /// The game area, without frame or title bar, in physical screen pixels
        /// whatever the client's DPI awareness.
        /// </summary>
        public static Rectangle ClientBounds { get; private set; }

        /// <summary>Raised when any of the above changes.</summary>
        public static EventHandler Changed;

        private const uint GW_OWNER = 4;
        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        /// <summary>Called every frame from the game thread.</summary>
        internal static void Update()
        {
            // Found on the first frame, and again if switching between windowed
            // and full screen replaced the window.
            if (!IsWindow(Handle))
                Handle = FindHandle();

            bool foreground = Handle != IntPtr.Zero && GetForegroundWindow() == Handle;
            bool minimized = Handle != IntPtr.Zero && IsIconic(Handle);
            Rectangle bounds = minimized ? ClientBounds : ReadClientBounds(Handle);

            if (foreground == IsForeground && minimized == IsMinimized && bounds == ClientBounds)
                return;

            IsForeground = foreground;
            IsMinimized = minimized;
            ClientBounds = bounds;

            try
            {
                Changed?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private static Rectangle ReadClientBounds(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return Rectangle.Empty;

            // The client may not be DPI aware, and then Windows hands it scaled
            // coordinates. Asking as a per-monitor aware thread gets real pixels.
            IntPtr previous = SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            try
            {
                if (!GetClientRect(hwnd, out RECT rect))
                    return Rectangle.Empty;

                POINT origin = new POINT();
                ClientToScreen(hwnd, ref origin);
                return new Rectangle(origin.X, origin.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            finally
            {
                if (previous != IntPtr.Zero)
                    SetThreadDpiAwarenessContext(previous);
            }
        }

        /// <summary>
        /// The game thread's largest visible, unowned top-level window. Core
        /// starts on the game thread, which is the thread that made the window.
        /// </summary>
        private static IntPtr FindHandle()
        {
            IntPtr best = IntPtr.Zero;
            long bestArea = 0;

            EnumThreadWindows(GetCurrentThreadId(), (hwnd, _) =>
            {
                if (!IsWindowVisible(hwnd) || GetWindow(hwnd, GW_OWNER) != IntPtr.Zero)
                    return true;

                if (GetClientRect(hwnd, out RECT rect))
                {
                    long area = (long)(rect.Right - rect.Left) * (rect.Bottom - rect.Top);
                    if (area > bestArea)
                    {
                        best = hwnd;
                        bestArea = area;
                    }
                }

                return true;
            }, IntPtr.Zero);

            return best != IntPtr.Zero ? best : Process.GetCurrentProcess().MainWindowHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X, Y;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hwnd, uint cmd);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(uint threadId, EnumWindowsProc proc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
