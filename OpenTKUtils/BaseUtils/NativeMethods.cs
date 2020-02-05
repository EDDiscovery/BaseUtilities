using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKUtils
{
    public class NativeMethods
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public class EVENTMSG
        {
            public int message;
            public int paramL;
            public int paramH;
            public int time;
            public IntPtr hwnd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public INPUTUNION inputUnion;
        }

        // We need to split the field offset out into a union struct to avoid
        // silent problems in 64 bit
        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        public const int INPUT_KEYBOARD = 1;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;

        public const int VIEW_E_DRAW = unchecked((int)0x80040140),

        VK_LBUTTON        = 0x01,
        VK_RBUTTON        = 0x02,
        VK_CANCEL         = 0x03,
        VK_MBUTTON        = 0x04,   /* NOT contiguous with L & RBUTTON */
        VK_XBUTTON1       = 0x05,   /* NOT contiguous with L & RBUTTON */
        VK_XBUTTON2       = 0x06,   /* NOT contiguous with L & RBUTTON */
        VK_BACK           = 0x08,
        VK_TAB            = 0x09,
        VK_CLEAR          = 0x0C,
        VK_RETURN         = 0x0D,
        VK_SHIFT          = 0x10,
        VK_CONTROL        = 0x11,
        VK_MENU           = 0x12,
        VK_PAUSE          = 0x13,
        VK_CAPITAL        = 0x14,
        VK_KANA           = 0x15,
        VK_HANGEUL        = 0x15, /* old name - should be here for compatibility */
        VK_HANGUL         = 0x15,
        VK_JUNJA          = 0x17,
        VK_FINAL          = 0x18,
        VK_HANJA          = 0x19,
        VK_KANJI          = 0x19,
        VK_ESCAPE         = 0x1B,
        VK_CONVERT        = 0x1C,
        VK_NONCONVERT     = 0x1D,
        VK_ACCEPT         = 0x1E,
        VK_MODECHANGE     = 0x1F,
        VK_SPACE          = 0x20,
        VK_PRIOR          = 0x21,
        VK_NEXT           = 0x22,
        VK_END            = 0x23,
        VK_HOME           = 0x24,
        VK_LEFT           = 0x25,
        VK_UP             = 0x26,
        VK_RIGHT          = 0x27,
        VK_DOWN           = 0x28,
        VK_SELECT         = 0x29,
        VK_PRINT          = 0x2A,
        VK_EXECUTE        = 0x2B,
        VK_SNAPSHOT       = 0x2C,
        VK_INSERT         = 0x2D,
        VK_DELETE         = 0x2E,
        VK_HELP           = 0x2F,
        VK_LWIN           = 0x5B,
        VK_RWIN           = 0x5C,
        VK_APPS           = 0x5D,
        VK_SLEEP          = 0x5F,
        VK_NUMPAD0        = 0x60,
        VK_NUMPAD1        = 0x61,
        VK_NUMPAD2        = 0x62,
        VK_NUMPAD3        = 0x63,
        VK_NUMPAD4        = 0x64,
        VK_NUMPAD5        = 0x65,
        VK_NUMPAD6        = 0x66,
        VK_NUMPAD7        = 0x67,
        VK_NUMPAD8        = 0x68,
        VK_NUMPAD9        = 0x69,
        VK_MULTIPLY       = 0x6A,
        VK_ADD            = 0x6B,
        VK_SEPARATOR      = 0x6C,
        VK_SUBTRACT       = 0x6D,
        VK_DECIMAL        = 0x6E,
        VK_DIVIDE         = 0x6F,
        VK_F1             = 0x70,
        VK_F2             = 0x71,
        VK_F3             = 0x72,
        VK_F4             = 0x73,
        VK_F5             = 0x74,
        VK_F6             = 0x75,
        VK_F7             = 0x76,
        VK_F8             = 0x77,
        VK_F9             = 0x78,
        VK_F10            = 0x79,
        VK_F11            = 0x7A,
        VK_F12            = 0x7B,
        VK_F13            = 0x7C,
        VK_F14            = 0x7D,
        VK_F15            = 0x7E,
        VK_F16            = 0x7F,
        VK_F17            = 0x80,
        VK_F18            = 0x81,
        VK_F19            = 0x82,
        VK_F20            = 0x83,
        VK_F21            = 0x84,
        VK_F22            = 0x85,
        VK_F23            = 0x86,
        VK_F24            = 0x87,
        VK_NAVIGATION_VIEW     = 0x88,// reserved
        VK_NAVIGATION_MENU     = 0x89,// reserved
        VK_NAVIGATION_UP       = 0x8A,// reserved
        VK_NAVIGATION_DOWN     = 0x8B,// reserved
        VK_NAVIGATION_LEFT     = 0x8C,// reserved
        VK_NAVIGATION_RIGHT    = 0x8D,// reserved
        VK_NAVIGATION_ACCEPT   = 0x8E,// reserved
        VK_NAVIGATION_CANCEL   = 0x8F,// reserved
        VK_NUMLOCK        = 0x90,
        VK_SCROLL         = 0x91,
        VK_OEM_NEC_EQUAL  = 0x92,  // '=' key on numpad
        VK_OEM_FJ_JISHO   = 0x92,  // 'Dictionary' key
        VK_OEM_FJ_MASSHOU = 0x93,  // 'Unregister word' key
        VK_OEM_FJ_TOUROKU = 0x94,  // 'Register word' key
        VK_OEM_FJ_LOYA    = 0x95,  // 'Left OYAYUBI' key
        VK_OEM_FJ_ROYA    = 0x96,  // 'Right OYAYUBI' key
/*
    * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
    * Used only as parameters to GetAsyncKeyState() and GetKeyState().
    * No other API or message will distinguish left and right keys in this way.
    */
        VK_LSHIFT         = 0xA0,
        VK_RSHIFT         = 0xA1,
        VK_LCONTROL       = 0xA2,
        VK_RCONTROL       = 0xA3,
        VK_LMENU          = 0xA4,
        VK_RMENU          = 0xA5,
        VK_BROWSER_BACK        = 0xA6,
        VK_BROWSER_FORWARD     = 0xA7,
        VK_BROWSER_REFRESH     = 0xA8,
        VK_BROWSER_STOP        = 0xA9,
        VK_BROWSER_SEARCH      = 0xAA,
        VK_BROWSER_FAVORITES   = 0xAB,
        VK_BROWSER_HOME        = 0xAC,
        VK_VOLUME_MUTE         = 0xAD,
        VK_VOLUME_DOWN         = 0xAE,
        VK_VOLUME_UP           = 0xAF,
        VK_MEDIA_NEXT_TRACK    = 0xB0,
        VK_MEDIA_PREV_TRACK    = 0xB1,
        VK_MEDIA_STOP          = 0xB2,
        VK_MEDIA_PLAY_PAUSE    = 0xB3,
        VK_LAUNCH_MAIL         = 0xB4,
        VK_LAUNCH_MEDIA_SELECT = 0xB5,
        VK_LAUNCH_APP1         = 0xB6,
        VK_LAUNCH_APP2         = 0xB7,
        VK_OEM_1          = 0xBA,  // ';:' for US
        VK_OEM_PLUS       = 0xBB,  // '+' any country
        VK_OEM_COMMA      = 0xBC,  // ',' any country
        VK_OEM_MINUS      = 0xBD,  // '-' any country
        VK_OEM_PERIOD     = 0xBE,  // '.' any country
        VK_OEM_2          = 0xBF,  // '/?' for US
        VK_OEM_3          = 0xC0,  // '`~' for US
        VK_GAMEPAD_A                         = 0xC3,// reserved
        VK_GAMEPAD_B                         = 0xC4,// reserved
        VK_GAMEPAD_X                         = 0xC5,// reserved
        VK_GAMEPAD_Y                         = 0xC6,// reserved
        VK_GAMEPAD_RIGHT_SHOULDER            = 0xC7,// reserved
        VK_GAMEPAD_LEFT_SHOULDER             = 0xC8,// reserved
        VK_GAMEPAD_LEFT_TRIGGER              = 0xC9,// reserved
        VK_GAMEPAD_RIGHT_TRIGGER             = 0xCA,// reserved
        VK_GAMEPAD_DPAD_UP                   = 0xCB,// reserved
        VK_GAMEPAD_DPAD_DOWN                 = 0xCC,// reserved
        VK_GAMEPAD_DPAD_LEFT                 = 0xCD,// reserved
        VK_GAMEPAD_DPAD_RIGHT                = 0xCE,// reserved
        VK_GAMEPAD_MENU                      = 0xCF,// reserved
        VK_GAMEPAD_VIEW                      = 0xD0,// reserved
        VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON    = 0xD1,// reserved
        VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON   = 0xD2,// reserved
        VK_GAMEPAD_LEFT_THUMBSTICK_UP        = 0xD3,// reserved
        VK_GAMEPAD_LEFT_THUMBSTICK_DOWN      = 0xD4,// reserved
        VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT     = 0xD5,// reserved
        VK_GAMEPAD_LEFT_THUMBSTICK_LEFT      = 0xD6,// reserved
        VK_GAMEPAD_RIGHT_THUMBSTICK_UP       = 0xD7,// reserved
        VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN     = 0xD8,// reserved
        VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT    = 0xD9,// reserved
        VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT     = 0xDA,// reserved
        VK_OEM_4          = 0xDB, //  '[{' for US
        VK_OEM_5          = 0xDC, //  '\|' for US
        VK_OEM_6          = 0xDD, //  ']}' for US
        VK_OEM_7          = 0xDE, //  ''"' for US
        VK_OEM_8          = 0xDF,
        VK_OEM_AX         = 0xE1, //  'AX' key on Japanese AX kbd
        VK_OEM_102        = 0xE2, //  "<>" or "\|" on RT 102-key kbd.
        VK_ICO_HELP       = 0xE3, //  Help key on ICO
        VK_ICO_00         = 0xE4, //  00 key on ICO
        VK_PROCESSKEY     = 0xE5,
        VK_ICO_CLEAR      = 0xE6,
        VK_PACKET         = 0xE7,
        VK_OEM_RESET      = 0xE9,
        VK_OEM_JUMP       = 0xEA,
        VK_OEM_PA1        = 0xEB,
        VK_OEM_PA2        = 0xEC,
        VK_OEM_PA3        = 0xED,
        VK_OEM_WSCTRL     = 0xEE,
        VK_OEM_CUSEL      = 0xEF,
        VK_OEM_ATTN       = 0xF0,
        VK_OEM_FINISH     = 0xF1,
        VK_OEM_COPY       = 0xF2,
        VK_OEM_AUTO       = 0xF3,
        VK_OEM_ENLW       = 0xF4,
        VK_OEM_BACKTAB    = 0xF5,
        VK_ATTN           = 0xF6,
        VK_CRSEL          = 0xF7,
        VK_EXSEL          = 0xF8,
        VK_EREOF          = 0xF9,
        VK_PLAY           = 0xFA,
        VK_ZOOM           = 0xFB,
        VK_NONAME         = 0xFC,
        VK_PA1            = 0xFD,
        VK_OEM_CLEAR      = 0xFE;
    }

    public enum HookType
    {
        WH_JOURNALRECORD = 0,
        WH_JOURNALPLAYBACK = 1,
        WH_KEYBOARD = 2,
        WH_GETMESSAGE = 3,
        WH_CALLWNDPROC = 4,
        WH_CBT = 5,
        WH_SYSMSGFILTER = 6,
        WH_MOUSE = 7,
        WH_HARDWARE = 8,
        WH_DEBUG = 9,
        WH_SHELL = 10,
        WH_FOREGROUNDIDLE = 11,
        WH_CALLWNDPROCRET = 12,
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    public class UnsafeNativeMethods
    {
        public enum GWL
        {
            ExStyle = -20,
            Style = -16
        }

        public enum GCL
        {
            Style = -26
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern short VkKeyScan(char key);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern short VkKeyScanEx(char key, IntPtr dwhkl);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetKeyNameText(int lParam, [Out] StringBuilder str, int len);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(HookType hk, NativeMethods.HookProc pfnhook, IntPtr hinst, uint threadid);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhook);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetClassLongA")]
        public static extern int GetClassLongA(IntPtr hWnd, GCL nIndex);

        // examples
        //var x1 = UnsafeNativeMethods.GetWindowLong(this.Handle, UnsafeNativeMethods.GWL.ExStyle);
        //System.Diagnostics.Debug.WriteLine("Extstyle = " + x1.ToString("x"));
        //                    var x2 = UnsafeNativeMethods.GetWindowLong(this.Handle, UnsafeNativeMethods.GWL.Style);
        //System.Diagnostics.Debug.WriteLine("style = " + x2.ToString("x"));
        //                    var x3 = UnsafeNativeMethods.GetClassLongA(this.Handle, UnsafeNativeMethods.GCL.Style);
        //System.Diagnostics.Debug.WriteLine("class = " + x3.ToString("x"));
        
        public static int ChangeWindowLong(IntPtr hWnd, GWL nIndex, int mask , int set)     // HELPER!
        {
            int cur = (GetWindowLong(hWnd, nIndex) & ~mask) | set;
            SetWindowLong(hWnd, nIndex, cur);
            //System.Diagnostics.Debug.WriteLine("set exstyle " + cur.ToString("X"));
            return cur;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetKeyboardState(byte[] keystate);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetKeyboardState(byte[] keystate);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string modName);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BlockInput([In, MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SendInput(uint nInputs, NativeMethods.INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern short GetAsyncKeyState(int vkey);

        public static object PtrToStructure(IntPtr lparam, Type cls)
        {
            return Marshal.PtrToStructure(lparam, cls);
        }

        public static void PtrToStructure(IntPtr lparam, object data)
        {
            Marshal.PtrToStructure(lparam, data);
        }


        [DllImport("User32.dll")]
        public static extern int SetForegroundWindow(IntPtr point);
        [DllImport("User32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public static IntPtr GetForegroundWindowOf(string pname)
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetProcessesByName(pname).FirstOrDefault();
            if (p != null)
                return p.MainWindowHandle;
            else
                return (IntPtr)0;
        }


        [DllImport("User32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        #region Menu API (user32.dll, win2k+) (MF_*) (TPM_*)

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647616(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647624(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateMenu();

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647636(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern bool EnableMenuItem(IntPtr hMenu, int uIDEnableItem, int uEnable);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647985(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647987(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647993(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void ModifyMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms647996(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetMenuDefaultItem(IntPtr hMenu, int uItem, uint fByPos);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms648003(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int TrackPopupMenuEx(IntPtr hmenu, int fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        #endregion

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644944(v=vs.85).aspx
        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("Shell32.dll")]
        public static extern uint SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr pszPath  // API uses CoTaskMemAlloc
        );

        [Flags]
        public enum AssocF
        {
            None = 0,
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(AssocF flags, AssocStr str,
           string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, ref uint
           pcchOut);

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms632605.aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            /// <summary>Reserved. DO NOT USE.</summary>
            public System.Drawing.Point ptReserved;
            /// <summary>The size of the window if it were maximized without being moved. This value defaults to the size of the primary monitor.</summary>
            public System.Drawing.Point ptMaxSize;
            /// <summary>The position of the window if it were to be maximized without being moved.</summary>
            public System.Drawing.Point ptMaxPosition;
            /// <summary>The minimum tracking size of the window.</summary>
            public System.Drawing.Point ptMinTrackSize;
            /// <summary>The maximum tracking size of the window. This value defaults to slighter larger than the size of the virtual screen.</summary>
            public System.Drawing.Point ptMaxTrackSize;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    public class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int OemKeyScan(short wAsciiVal);
        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetTickCount();
    }


}
