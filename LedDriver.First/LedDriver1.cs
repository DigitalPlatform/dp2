using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;

namespace LedDriver.First
{
    /// <summary>
    /// LED 具体型号驱动 -- 诣阔led板
    /// </summary>
    public class LedDriver1 : ILedDriver
    {
        LedProperty _ledProperty = new LedProperty();

        public LedProperty LedProperty
        {
            get
            {
                return _ledProperty;
            }
        }

        // 初始化时需要提供端口号、LED 片数量、每片像素宽度高度等参数
        // parameters:
        //      style   附加的子参数 
        public NormalResult InitializeDriver(LedProperty property,
            string style)
        {
            _ledProperty = property;

            var verify_result = VerifyLedProperty(_ledProperty);
            if (verify_result.Value == -1)
                return verify_result;

            return new NormalResult();
        }

        public NormalResult ReleaseDriver()
        {
            return new NormalResult();
        }

        public static int DEFAULT_FONT_SIZE = 32;

        // parameters:
        //      style   附加的子参数 
        public NormalResult Display(string text,
            int x,
            int y,
            DisplayStyle property,
            string style)
        {
            int fontSize = DEFAULT_FONT_SIZE;
            if (string.IsNullOrEmpty(property.FontSize) == false)
            {
                if (Int32.TryParse(property.FontSize, out fontSize) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"fontSize '{property.FontSize}' 格式错误",
                        ErrorCode = "invalidFontSize"
                    };
                // TODO: 检查，是否为 16 24 32 之一
            }

            int totalWidth = _ledProperty.CellWidth * _ledProperty.CellXCount;

            // 01 静止, 02 左移, 49 连续左移
            int actionType = 1; // 默认静止
            if (string.IsNullOrEmpty(property.Effect)
                || property.Effect == "still")
                actionType = 1;
            else if (property.Effect == "moveLeft")
                actionType = 2;
            else if (property.Effect == "moveLeftContinue")
                actionType = 49;
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"effect '{property.Effect}' 格式错误。应为 still moveLeft moveLeftContinue 之一",
                    ErrorCode = "invalidEffect"
                };

            int speed = 0;
            if (string.IsNullOrEmpty(property.MoveSpeed) == false)
            {
                if (Int32.TryParse(property.FontSize, out fontSize) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"moveSpeed '{property.MoveSpeed}' 格式错误，应为 1~99 之间的整数",
                        ErrorCode = "invalidMoveSpeed"
                    };

                // 检查值范围
                if (speed >= 1 && speed <= 99)
                {

                }
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"moveSpeed '{property.MoveSpeed}' 格式错误，应为 1~99 之间的整数",
                        ErrorCode = "invalidFontSize"
                    };
            }

            int duration = 0;
            if (string.IsNullOrEmpty(property.Duration) == false)
            {
                if (Int32.TryParse(property.Duration, out duration) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"duration '{property.Duration}' 格式错误，应为 0~9999 之间的整数",
                        ErrorCode = "invalidDuration"
                    };

                // 检查值范围
                if (duration >= 0 && duration <= 9999)
                {

                }
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"duration '{property.Duration}' 格式错误，应为 0~9999 之间的整数",
                        ErrorCode = "invalidDuration"
                    };
            }

            string command = GetCommand(text,
    fontSize,
    totalWidth,
    actionType,
    speed,
    duration,
    x,
    y,
    _ledProperty.CellHeight);

            // 发送指令串
            bool bRet = sendMsg2Led(_ledProperty.SerialPort,
                command,
                out string error);
            if (bRet == false)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"发送串口命令(端口号 {_ledProperty.SerialPort})出错: {error}",
                    ErrorCode = "sendMessageError"
                };
            }

            return new NormalResult();
        }

        /// <summary>
        /// 发送指令串到led显示屏
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="msg"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool sendMsg2Led(string portName,
            string msg,
            out string error)
        {
            error = "";

            ComModel _comModel = new ComModel();
            try
            {
                // 打开串口
                _comModel.Open(portName,
                "57600",//baudRate,
                "8",//dataBits,
                "One",//stopBits,
                "None",//parity,
                "None"); //handshake);

                // 将字符转为二进制
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("gb2312");
                byte[] data = encoding.GetBytes(msg);

                // 给串口发送二进制
                bool bRet = _comModel.Send(data);
                if (bRet == false)
                {
                    error = "给串口发送消息失败";
                    return false;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                // 关闭串口
                _comModel.Close();
            }

            return true;
        }

        /// <summary>
        /// 得到诣阔LED命令串
        public static string GetCommand(string text,
            int nFontSize,
            int width,
            int actionType,
            int speed,
            int hold,
            int x = 0,
            int y = 0,
            int cellHeight = 32)
        {
            string result = "";

            string textAlign = "1";  //默认左对齐
            // 静止的时间水平居中，移动的时候左对齐
            if (actionType == 1)
            {
                textAlign = "2";
            }

            string strCellHeight = cellHeight.ToString().PadLeft(4, '0');

            result = "!#"       // 起始标志
                   + "001"       // 控制卡号，一般只有一块控制卡，默认填001
                   + "%ZD00"  // 先把以前的分区全部删除
                   + "%ZI01"   // 创建新的区域，默认编号为01
                   + "%ZC" + x.ToString().PadLeft(4, '0') + y.ToString().PadLeft(4, '0') + width.ToString().PadLeft(4, '0') + strCellHeight/*"0032"*/ //创建分区,四段分别代表坐标 “起点 X 、起点 Y、 宽度 、高度”
                   + "%ZA" + actionType.ToString().PadLeft(2, '0')  //设置区域的特技参数由 2 位数字组成(01—立即显示 49—连续左移)
                   + "%ZS" + speed.ToString().PadLeft(2, '0')
                   + "%ZH" + hold.ToString().PadLeft(4, '0')
                   + "%F" + nFontSize.ToString()     // 字体大小，点阵字库支持16,24,32
                   + "%AH" + textAlign   // 水平对齐
                   + "%AV2"    // 垂直居中
                   + "%C1"      // 颜色，单色只有红色
                   + text          // 字符串
                   + "$$";        // 结尾

            return result;
        }

        // 验证 LedProperty 的格式正确性
        public static NormalResult VerifyLedProperty(LedProperty property)
        {
            if (string.IsNullOrEmpty(property.SerialPort))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "SerialPort 尚未指定端口号"
                };
            }

            if (property.CellXCount > 0)
            {

            }
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "CellXCount (LED 片横向数量)参数格式不正确。应 >= 1"
                };

            if (property.CellWidth == 0)
                property.CellWidth = 64;    // 默认 64
            else if (property.CellWidth > 0)
            {

            }
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "CellWidth (LED 单片宽度)参数格式不正确。应 > 0"
                };

            if (property.CellHeight == 0)
                property.CellHeight = 32;    // 默认 32
            else if (property.CellHeight > 0)
            {

            }
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "CellHeight (LED 单片高度)参数格式不正确。应 > 0"
                };

            return new NormalResult();
        }
    }
}
