﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace LedDriver.First
{
    /// <summary>
    /// LED 具体型号驱动 -- 诣阔led板
    /// </summary>
    public class LedDriver1 : ILedDriver
    {
        LedProperty _ledProperty = new LedProperty();

        // 是否完成了初始化
        bool _initialized = false;

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
            _initialized = false;

            _ledProperty = property;

            var verify_result = VerifyLedProperty(_ledProperty);
            if (verify_result.Value == -1)
                return verify_result;

            _initialized = true;
            return new NormalResult();
        }

        public NormalResult ReleaseDriver()
        {
            _initialized = false;
            return new NormalResult();
        }

        public static int DEFAULT_FONT_SIZE = 32;

        // parameters:
        //      style   附加的子参数 
        public NormalResult Display(
            string ledName,
            string text,
            int x,
            int y,
            DisplayStyle property,
            string style)
        {
            // 2021/8/13
            if (_initialized == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "LED 驱动尚未初始化",
                    ErrorCode = "uninitialized"
                };

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

            if (IsNumber(property.Effect))
            {
                if (Int32.TryParse(property.Effect, out actionType) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"effect '{property.Effect}' 格式错误，应为 0~99 之间的整数",
                        ErrorCode = "invalidMoveSpeed"
                    };
            }
            else
            {
                /*
                if (string.IsNullOrEmpty(property.Effect)
                || property.Effect == "still")
                    actionType = 1;
                else if (property.Effect == "moveLeft")
                    actionType = 2;
                else if (property.Effect == "moveLeftCompact")  // 左移，紧凑形态
                    actionType = 49;
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"effect '{property.Effect}' 格式错误。应为 still moveLeft moveLeftContinue 之一",
                        ErrorCode = "invalidEffect"
                    };
                */
                actionType = GetEffectNumber(property.Effect);
                if (actionType == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"effect '{property.Effect}' 格式错误。应为 still moveLeft moveLeftContinue 等等之一",
                        ErrorCode = "invalidEffect"
                    };
            }

            // 速度。数字，或者 slow/normal/fast 三者之一。数字 0~99，越大越快
            int speed = 0;
            if (IsNumber(property.MoveSpeed))
            {
                if (string.IsNullOrEmpty(property.MoveSpeed) == false)
                {
                    if (Int32.TryParse(property.MoveSpeed, out speed) == false)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"moveSpeed '{property.MoveSpeed}' 格式错误，应为 1~99 之间的整数",
                            ErrorCode = "invalidMoveSpeed"
                        };

                    // 检查值范围
                    if (speed >= 0 && speed <= 99)
                    {

                    }
                    else
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"moveSpeed '{property.MoveSpeed}' 格式错误，应为 0~99 之间的整数",
                            ErrorCode = "invalidFontSize"
                        };

                    speed = 99 - speed;
                }
            }
            else
            {
                if (property.MoveSpeed == "slow")
                    speed = 20;
                else if (property.MoveSpeed == "normal")
                    speed = 70;
                else if (property.MoveSpeed == "fast")
                    speed = 90;
                else
                    speed = 70;

                speed = 99 - speed;
            }

            // TODO: 可以做成默认 板子数量 * 1 秒
            int duration_value = Math.Max(10, _ledProperty.CellXCount * 10);    // 默认 1 秒
            // 浮点数，单位为秒
            if (string.IsNullOrEmpty(property.Duration) == false)
            {
                float duration = 0;
                if (float.TryParse(property.Duration, out duration) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"duration '{property.Duration}' 格式错误，应为 0~9999 之间的整数",
                        ErrorCode = "invalidDuration"
                    };

                // 变换为 0~9999 整数(单位为 0.1 秒)
                duration_value = Convert.ToInt32(duration * 10);

                // 检查值范围
                if (duration_value >= 0 && duration_value <= 9999)
                {

                }
                else
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"duration '{property.Duration}' 格式错误，应为 0~999 之间秒数(可以是小数)",
                        ErrorCode = "invalidDuration"
                    };
            }

            string command = GetCommand(text,
    fontSize,
    totalWidth,
    actionType,
    speed,
    duration_value,
    property.HorzAlign,
    property.VertAlign,
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

        static string[] _effectTable = new string[] {
            "00:随机,random",
            "01:立即显示,still",
            "02:左移,moveLeft",
            "03:右移,moveRight",
            "04:上移,moveUp",
            "05:下移,moveDown",
            "06:飘雪",
            "07:冒泡",
            "08:分散拉伸",
            "09:画卷打开",
            "10:画卷闭合",
            "11:向左拉伸",
            "12:向右拉伸",
            "13:向上拉伸",
            "14:向下拉伸",
            "15:向左镭射",
            "16:向右镭射",
            "17:向上镭射",
            "18:向下镭射",
            "19:水平百叶",
            "20:垂直百叶",
            "21:左覆盖",
            "22:右覆盖",
            "23:上覆盖",
            "24:下覆盖",
            "25:左上角覆盖(斜线)",
            "26:右上角覆盖(斜线)",
            "27:左下角覆盖(斜线)",
            "28:右下角覆盖(斜线)",
            "29:左上角覆盖(直线)",
            "30:右上角覆盖(直线)",
            "31:左下角覆盖(直线)",
            "32:右下角覆盖(直线)",
            "33:左右对开",
            "34:上下对开",
            "35:左右闭合",
            "36:上下闭合",
            "37:中间向四周(矩形)",
            "38:四周向中间(矩形)",
            "39:中间向四周(十字)",
            "40:四周向中间(十字)",
            "41:中间向四周(菱形)",
            "42:四周向中间(菱形)",
            "43:闪烁",
            "44:中间移出",
            "45:左右移入",
            "46:左右交叉移动",
            "47:左右交叉覆盖",
            "48:上下交叉覆盖",
            "49:连续左移,紧凑左移,moveLeftCompact",
        };

        // 把特效名字转换为内部特效编号
        static int GetEffectNumber(string name)
        {
            if (string.IsNullOrEmpty(name))
                return 1;   // 默认 still
            foreach (string line in _effectTable)
            {
                if (line == null || line.Length <= 3)
                    throw new Exception($"_effectTable 中行 '{line}' 格式错误");
                string list = line.Substring(3);
                if (StringUtil.IsInList(name, list))
                {
                    string number = line.Substring(0, 2);
                    if (Int32.TryParse(number, out int value) == false)
                        throw new Exception($"_effectTable 中行 '{line}' 格式错误: 前两个字符应该为数字");
                    return value;
                }
            }

            return -1;
        }

        static bool IsNumber(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return false;

            for (int i = 0; i < strText.Length; i++)
            {
                if (strText[0] < '0' || strText[0] > '9')
                    return false;
            }

            return true;
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
                var ret = _comModel.Open(portName,
                    "57600",//baudRate,
                    "8",//dataBits,
                    "One",//stopBits,
                    "None",//parity,
                    "None",//handshake
                    out error);
                if (ret == false)
                    return false;

                // 将字符转为二进制
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("gb2312");
                byte[] data = encoding.GetBytes(msg);

                // 给串口发送二进制
                bool bRet = _comModel.Send(data, out string strError);
                if (bRet == false)
                {
                    error = $"给串口({portName})发送消息失败: {strError}";
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
            string horzAlign,
            string vertAlign,
            int x = 0,
            int y = 0,
            int cellHeight = 32)
        {
            string result = "";

            // 水平方向，1—左对齐 2—居中 3—右对齐
            if (horzAlign == "center")
                horzAlign = "2";
            else if (horzAlign == "right")
                horzAlign = "3";
            else
                horzAlign = "1";
            /*
            string textAlign = "1";  //默认左对齐
            // 静止的时间水平居中，移动的时候左对齐
            if (actionType == 1)
            {
                textAlign = "2";
            }
            */
            // 垂直方向，1--上对齐 2—居中 3—下对齐
            if (vertAlign == "center")
                vertAlign = "2";
            else if (vertAlign == "bottom")
                vertAlign = "3";
            else
                vertAlign = "1";

            string strHeight = (cellHeight - y).ToString().PadLeft(4, '0');
            string strWidth = (width - x).ToString().PadLeft(4, '0');

            result = "!#"       // 起始标志
                   + "001"       // 控制卡号，一般只有一块控制卡，默认填001
                   + "%ZD00"  // 先把以前的分区全部删除
                   + "%ZI01"   // 创建新的区域，默认编号为01
                   + "%ZC" + x.ToString().PadLeft(4, '0') + y.ToString().PadLeft(4, '0') + strWidth + strHeight/*"0032"*/ //创建分区,四段分别代表坐标 “起点 X 、起点 Y、 宽度 、高度”
                   + "%ZA" + actionType.ToString().PadLeft(2, '0')  //设置区域的特技参数由 2 位数字组成(01—立即显示 49—连续左移)
                   + "%ZS" + speed.ToString().PadLeft(2, '0')
                   + "%ZH" + hold.ToString().PadLeft(4, '0')
                   + "%F" + nFontSize.ToString()     // 字体大小，点阵字库支持16,24,32
                   + "%AH" + horzAlign   // 水平对齐
                   + "%AV" + vertAlign  // 垂直居中
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
