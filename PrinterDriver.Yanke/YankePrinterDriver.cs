using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;

namespace PrinterDriver.Yanke
{
    /// <summary>
    /// 研科 POS 打印机接口
    /// </summary>
    public class YankePrinterDriver : IPosPrinterDriver
    {
        // parameters:
        //      style   附加的子参数 
        public NormalResult InitializeDriver(string port,
            string style)
        {
            try
            {
                int iport = 0;

                if (port.ToLower() == "usb")
                {
                    StringBuilder sb = new StringBuilder();

                    int count = Printer.EnumUSBDeviceSerials(sb);

                    if (count > 0)
                    {
                        // String serial = sb.ToString();
                        iport = 13;
                    }
                    else
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"连接打印机 '{port}' 失败: 没有发现 USB 设备"
                        };
                }
                else if (port.ToLower().StartsWith("com") == true)
                {
                    string number = port.Substring("com".Length).Trim();
                    if (Int32.TryParse(number, out iport) == false)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"端口号字符串 '{port}' 不合法: 数字部分 '{number}' 不合法"
                        };
                }
                else
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"端口号字符串 '{port}' 不合法"
                    };
                }

                //函数功能：连接打印机设备
                //返回值：0 -- 成功   -1  --  失败
                int ret = Printer.YkOpenDevice(iport, 0, 0);
                if (ret == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"连接打印机 '{port}' 失败(iport={iport})"
                    };
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"InitializeDriver() 出现异常: {ExceptionUtil.GetDebugText(ex)}"
                };
            }
        }

        public NormalResult ReleaseDriver()
        {
            try
            {
                int ret = Printer.YkCloseDevice();
                return new NormalResult();
            }
            catch(Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"ReleaseDriver() 出现异常: {ExceptionUtil.GetDebugText(ex)}"
                };
            }
        }

        // parameters:
        //      action  动作。为 feed/cut/cuthalf/printline/print/空 之一。空等于 print(不自动换行)
        //      style   附加的子参数 
        public NormalResult Print(
            string action,
            string text,
            string style)
        {
            int nRet = 0;
            if (action == "feed")
            {
                //返回值：0 -- 成功   -1  --  失败
                nRet = Printer.YkFeedPaper();
                if (nRet == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "FeedPaper Fail"
                    };
            }

            if (action == "cut")
            {
                //返回值：0:成功 1:失败
                nRet = Printer.CutPaper(0);
                if (nRet == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "CutPaper Fail"
                    };
            }

            if (action == "cuthalf")
            {
                //返回值：0:成功 1:失败
                nRet = Printer.CutPaper(1);
                if (nRet == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "CutPaper Fail"
                    };
            }

            if (action == "printline")
            {
                //返回值：0:成功 1:失败
                nRet = Printer.PrintLine(text);
                if (nRet == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "PrintLine Fail"
                    };
            }

            // action 为空，或者 "print"
            //返回值：0:成功 1:失败
            nRet = Printer.PrintString(text);
            if (nRet == 0)
                return new NormalResult();
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "PrintString Fail"
                };
        }
    }
}
