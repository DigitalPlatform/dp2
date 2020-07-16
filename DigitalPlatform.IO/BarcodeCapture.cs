using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// 捕捉条码输入
    /// </summary>
    public class BarcodeCapture
    {
        public delegate void Delegate_lineFeed(StringInput input);
        public event Delegate_lineFeed InputLine;

        public delegate void Delegate_charFeed(CharInput input);

        public event Delegate_charFeed InputChar;

        //定义成静态，这样不会抛出回收异常
        private static HookProc hookproc;

        public bool Handled = false;

        public struct StringInput
        {
            /*
            public int VirtKey;//虚拟码
            public int ScanCode;//扫描码
            public string KeyName;//键名
            public uint Ascii;//Ascll
            public char Chr;//字符
            */

            /*
        public string OriginalChrs; //原始 字符
        public string OriginalAsciis;//原始 ASCII

        public string OriginalBarCode; //原始数据条码
        */

            public string Barcode;//条码信息 保存最终的条码
            public bool IsValid;//条码是否有效
            public DateTime Time;//扫描时间,
        }

        public struct CharInput
        {
            public Keys Key;
            public string KeyChar;

            /*
            public int VirtKey;//虚拟码
            public int ScanCode;//扫描码
            public string KeyName;//键名
            public uint Ascii;//Ascll
            public char Chr;//字符
            */

            /*
            public string OriginalChrs; //原始 字符
            public string OriginalAsciis;//原始 ASCII

            public string OriginalBarCode; //原始数据条码

            public string Barcode;//条码信息 保存最终的条码
            public bool IsValid;//条码是否有效
            public DateTime Time;//扫描时间,
            */
        }

        private struct EventMsg
        {
            public int message;
            public int paramL;
            public int paramH;
            public int Time;
            public int hwnd;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        private static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);

        [DllImport("user32", EntryPoint = "GetKeyboardState")]
        private static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32", EntryPoint = "ToAscii")]
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);


        delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        // CharInput _char = new CharInput();
        int hKeyboardHook = 0;

        StringBuilder _barcode = new StringBuilder();

        StringInput _string = new StringInput();

        static int TIME_SHTRESHOLD = 50;

        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        static bool _shift = false;

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));
                if (wParam == WM_KEYUP)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    Keys key = (Keys)vkCode;

                    if (key == Keys.LShiftKey)
                    {
                        _shift = false;
                        goto END1;
                    }
                }

                if (wParam == WM_KEYDOWN)//WM_KEYDOWN=0x100 
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    Keys key = (Keys)vkCode;

                    if (key == Keys.LShiftKey)
                    {
                        _shift = true;
                        // Console.WriteLine($"modi {(System.Windows.Forms.Keys)vkCode}");
                        goto END1;
                    }

                    TimeSpan ts = DateTime.Now.Subtract(_string.Time);
                    if (ts.TotalMilliseconds > TIME_SHTRESHOLD)
                        _barcode.Clear();

                    _string.Time = DateTime.Now;

                    //KeysConverter kc = new KeysConverter();
                    //string keyChar = kc.ConvertToString(key);

                    string keyChar = GetKeyString(key, _shift);
                    Debug.WriteLine($"keyChar='{keyChar}', key='{key.ToString()}'");

                    _barcode.Append(keyChar);

                    // 保护缓冲区
                    if (_barcode.Length > 1000)
                        _barcode.Clear();

                    Debug.WriteLine($"msg.message={(msg.message & 0xff)} _barcode:'{_barcode.ToString()}'");

                    if (InputChar != null)
                    {
                        CharInput input = new CharInput
                        {
                            Key = key,
                            KeyChar = keyChar,
                        };
                        AsyncCallback callback = new AsyncCallback(AsyncBackCharFeed);
                        Delegate[] delArray = InputChar.GetInvocationList();
                        foreach (Delegate_charFeed del in delArray)
                        {
                            try
                            {
                                del.BeginInvoke(input, callback, del);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }

                    if ((msg.message & 0xff) == 13 && _barcode.Length > 3)
                    {
                        // 回车
                        _string.Barcode = _barcode.ToString();// barCode.OriginalBarCode;
                        _string.IsValid = true;
                        _barcode.Remove(0, _barcode.Length);

                        Debug.WriteLine($"isValid = true");
                    }
                    else
                        goto END1;

                    try
                    {
                        if (InputLine != null
                            // && _string.IsValid
                            )
                        {
                            Debug.WriteLine($"trigger callback");

                            AsyncCallback callback = new AsyncCallback(AsyncBackLineFeed);
                            Delegate[] delArray = InputLine.GetInvocationList();
                            foreach (Delegate_lineFeed del in delArray)
                            {
                                try
                                {
                                    del.BeginInvoke(_string, callback, del); // 异步调用防止界面卡死
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            //BarCodeEvent(barCode);//触发事件
                            _string.Barcode = "";
                            //_char.OriginalChrs = "";
                            //_char.OriginalAsciis = "";
                            //_char.OriginalBarCode = "";
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _string.IsValid = false;
                        _string.Time = DateTime.Now;
                    }
                }
            }
        END1:
            if (Handled == true)
            {
                if (wParam == WM_KEYDOWN)
                    Console.Beep();
                return 1;
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

        /*
        static char[] upper_number = new char[] { ')','!','@',
'#','$','%','^','&','*','(',
        };
        */

        static string[] char_def = new string[] {
            "Oemtilde:`~",
            "D1:1!",
            "D2:2@",
            "D3:3#",
            "D4:4$",
            "D5:5%",
            "D6:6^",
            "D7:7&",
            "D8:8*",
            "D9:9(",
            "D0:0)",
            "OemMinus:-_",
            "Oemplus:=+",
            "OemOpenBrackets:[{",
            "Oem6:]}",
            "Oem5:\\|",
            "Oem1:;:",
            "Oem7:'\"",
            "Oemcomma:,<",
            "OemPeriod:.>",
            "OemQuestion:/?",
        };

        static Hashtable _char_table = null;

        static void BuildCharTable()
        {
            if (_char_table != null)
                return;
            _char_table = new Hashtable();
            foreach (string def in char_def)
            {
                int pos = def.IndexOf(':');
                string name = def.Substring(0, pos);
                string chars = def.Substring(pos + 1);
                _char_table[name] = chars;
            }
        }

        static string GetCharDef(string name)
        {
            if (_char_table.ContainsKey(name) == false)
                return null;
            return (string)_char_table[name];
        }

        public static string GetKeyString(Keys key, bool shift)
        {
            if (key == Keys.None)
                return "";

            BuildCharTable();

            string name = key.ToString();
            string keyStr = "?";    //  key.ToString();
            int keyInt = (int)key;
            // Numeric keys
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                //if (shift == false)
                //    return char.ToString((char)(key - Keys.D0 + '0'));

                var def = GetCharDef(name);
                if (def == null)
                    return "?";
                return char.ToString(def[shift ? 1 : 0]);
                // return char.ToString(upper_number[key - Keys.D0]);
            }
            // Char keys are returned directly
            else if (key >= Keys.A && key <= Keys.Z)
            {
                if (shift == false)
                    return char.ToString((char)(key - Keys.A + 'a'));
                return char.ToString((char)(key - Keys.A + 'A'));
            }
            else if (name.StartsWith("Oem"))
            {
                var def = GetCharDef(name);
                if (def == null)
                    return "?";
                return char.ToString(def[shift ? 1 : 0]);

            }
            else if (key == Keys.Return)
            {
                return "\r";
            }
            // If the key is a keypad operation (Add, Multiply, ...) or an 'Oem' key, P/Invoke
            else if ((keyInt >= 84 && keyInt <= 89) || keyInt >= 140)
            {
                // keyStr = KeyCodeToUnicode(key);
                return "?";
            }
            return keyStr;
        }

#if OLD
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));
                if (wParam == 0x100)//WM_KEYDOWN=0x100 
                {
                    _char.VirtKey = msg.message & 0xff;//虚拟吗
                    _char.ScanCode = msg.paramL & 0xff;//扫描码

                    if (_char.VirtKey == (int)Keys.LShiftKey)
                        goto END1;

                    StringBuilder strKeyName = new StringBuilder(225);
                    if (GetKeyNameText(_char.ScanCode * 65536, strKeyName, 255) > 0)
                    {
                        _char.KeyName = strKeyName.ToString().Trim(new char[] { ' ', '\0' });
                    }
                    else
                    {
                        _char.KeyName = "";
                    }
                    byte[] kbArray = new byte[256];
                    uint uKey = 0;
                    GetKeyboardState(kbArray);

                    if (ToAscii(_char.VirtKey, _char.ScanCode, kbArray, ref uKey, 0))
                    {
                        _char.Ascii = uKey;
                        _char.Chr = Convert.ToChar(uKey);
                    }

                    TimeSpan ts = DateTime.Now.Subtract(_string.Time);

                    // Debug.WriteLine($"ts={ts.TotalMilliseconds} char:'{_char.Chr.ToString()}' code:{(int)(_char.Chr)}");
                    Debug.WriteLine($"ts={ts.TotalMilliseconds} code:{(int)(_char.Chr)}");

                    if (_char.Chr == 0)
                        goto END1;

                    if (ts.TotalMilliseconds > TIME_SHTRESHOLD)
                    {
                        //时间戳，大于50 毫秒表示手动输入
                        //strBarCode = barCode.Chr.ToString();
                        _barcode.Remove(0, _barcode.Length);
                        _barcode.Append(_char.Chr.ToString());
                        //_char.OriginalChrs = " " + Convert.ToString(_char.Chr);
                        //_char.OriginalAsciis = " " + Convert.ToString(_char.Ascii);
                        //_char.OriginalBarCode = Convert.ToString(_char.Chr);
                    }
                    else
                    {
                        _barcode.Append(_char.Chr.ToString());

                        Debug.WriteLine($"msg.message={(msg.message & 0xff)} _barcode:'{_barcode.ToString()}'");

                        if ((msg.message & 0xff) == 13 && _barcode.Length > 3)
                        {
                            // 回车
                            _string.Barcode = _barcode.ToString();// barCode.OriginalBarCode;
                            _string.IsValid = true;
                            _barcode.Remove(0, _barcode.Length);

                            Debug.WriteLine($"isValid = true");
                        }
                    }
                    _string.Time = DateTime.Now;
                    try
                    {
                        if (InputLine != null
                            && _string.IsValid
                            )
                        {
                            Debug.WriteLine($"trigger callback");

                            AsyncCallback callback = new AsyncCallback(AsyncBack);
                            Delegate[] delArray = InputLine.GetInvocationList();
                            foreach (Delegate_lineFeed del in delArray)
                            {
                                try
                                {
                                    del.BeginInvoke(_string, callback, del); // 异步调用防止界面卡死
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            //BarCodeEvent(barCode);//触发事件
                            _string.Barcode = "";
                            //_char.OriginalChrs = "";
                            //_char.OriginalAsciis = "";
                            //_char.OriginalBarCode = "";
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _string.IsValid = false;
                        _string.Time = DateTime.Now;
                    }
                }
            }
            END1:
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }

#endif

        //异步返回方法
        public void AsyncBackLineFeed(IAsyncResult ar)
        {
            Delegate_lineFeed del = ar.AsyncState as Delegate_lineFeed;
            del.EndInvoke(ar);
        }

        //异步返回方法
        public void AsyncBackCharFeed(IAsyncResult ar)
        {
            Delegate_charFeed del = ar.AsyncState as Delegate_charFeed;
            del.EndInvoke(ar);
        }

        //安装钩子
        public bool Start()
        {
            if (hKeyboardHook == 0)
            {
                hookproc = new HookProc(KeyboardHookProc);

                //GetModuleHandle 函数 替代 Marshal.GetHINSTANCE
                //防止在 framework4.0中 注册钩子不成功
                IntPtr modulePtr = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);

                //WH_KEYBOARD_LL=13
                //全局钩子 WH_KEYBOARD_LL
                // hKeyboardHook = SetWindowsHookEx(13, hookproc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);

                hKeyboardHook = SetWindowsHookEx(13, hookproc, modulePtr, 0);
            }
            return (hKeyboardHook != 0);
        }

        //卸载钩子
        public bool Stop()
        {
            if (hKeyboardHook != 0)
            {
                return UnhookWindowsHookEx(hKeyboardHook);
            }
            return true;
        }

    }
}
