using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

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

            if (action == "getstatus")
                return GetStatus(style);

            // 初始化打印机
            /*
[名称] 初始化打印机
[格式] ASCII码 ESC @
十六进制码 1B 40
十进制码 27 64
[描述] 清除打印缓冲区中的数据，复位打印机模式到电源打开时打印机的有效模式。
[注意] * Memory Swithc的设置不再被检查
* 接收缓冲区中的数据不被清除。
            * */
            if (action == "init")
            {
                //返回值：0 -- 成功   -1  --  失败
                nRet = Printer.YkInitPrinter();
                if (nRet == 0)
                    return new NormalResult();
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "InitPrinter Fail"
                    };
            }

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

        // 获得打印机当前状态
        public NormalResult GetStatus(string style)
        {
            byte[] status = new byte[4];

            /*
            status[0] = (byte)Printer.YkGetStatus((byte)1);
            status[1] = (byte)Printer.YkGetStatus((byte)2);
            status[2] = (byte)Printer.YkGetStatus((byte)3);
            */
            int ret = Printer.YkGetStatus((byte)4);
            if (ret == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "请求失败"
                };

            status[3] = (byte)ret;

            List<string> errors = new List<string>();

            /*
            if ((status[0] & 0x40) > 0)
            {
                //进纸键接通
            }
            else
            {
                //进纸键断开
            }

            if ((status[0] & 0x20) > 0)
            {
                //等待联机回复错误
            }
            else
            {
                //未等待
            }

            if ((status[0] & 0x08) > 0)
            {
                //脱机
            }
            else
            {
                //联机
            }

            if ((status[1] & 0x40) > 0)
            {
                //发生错误
            }
            else
            {
                //正常
            }

            if ((status[1] & 0x20) > 0)
            {
                //打印机纸用完停止打印
            }
            else
            {
                //正常
            }

            if ((status[1] & 0x08) > 0)
            {
                //通过进纸键进纸
            }
            else
            {
                //不通过进纸键进纸
            }

            if ((status[1] & 0x04) > 0)
            {
                //机头抬杠打开
            }
            else
            {
                //机头抬杠关闭
            }

            if ((status[2] & 0x40) > 0)
            {
                //出现可自动恢复错误
            }
            else
            {
                //正常
            }

            if ((status[2] & 0x20) > 0)
            {
                //出现不可恢复错误
            }
            else
            {
                //正常
            }

            if ((status[2] & 0x08) > 0)
            {
                //发生自动切纸错误
            }
            else
            {
                //正常
            }

            if ((status[2] & 0x04) > 0)
            {
                //发生机械错误
            }
            else
            {
                //正常
            }
            */

            if ((status[3] & 0x60) > 0)
            {
                //纸尽
                errors.Add("paperout");
            }
            else
            {
                //正常
            }

            if ((status[3] & 0x0c) > 0)
            {
                //纸将尽
                errors.Add("paperwillout");
            }
            else
            {
                //正常
            }

            return new NormalResult
            {
                Value = 0,
                ErrorCode = StringUtil.MakePathList(errors)
            };
        }
    }
}
