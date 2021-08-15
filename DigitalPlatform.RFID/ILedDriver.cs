using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public interface ILedDriver
    {
        // 初始化时需要提供端口号、LED 片数量、每片像素宽度高度等参数
        // parameters:
        //      style   附加的子参数 
        NormalResult InitializeDriver(LedProperty property, string style);

        NormalResult ReleaseDriver();

        // parameters:
        //      style   附加的子参数
        // result.ErrorCode
        //      "uninitialized" LED 驱动尚未初始化
        NormalResult Display(
            string ledName,
            string text, 
            int x,
            int y,
            DisplayStyle property, 
            string style);
    }

    // 初始化 LED 环境参数
    public class LedProperty
    {
        public string SerialPort { get; set; }  // 串口端口号
        public int CellXCount { get; set; } // LED 片数量
        public int CellWidth { get; set; }  // LED 单片宽度，像素数
        public int CellHeight { get; set; } // LED 单片高度，像素数
    }

    // 文字显示特性
    [Serializable]
    public class DisplayStyle
    {
        public string FontSize { get; set; }    // 数字，像素数 16 24 32

        public string HorzAlign { get; set; }   // left/center/right 默认 left
        public string VertAlign { get; set; }   // top/center/bottom 默认 top

        public string Effect { get; set; }  // moveLeft still moveLeftContinue
        public string MoveSpeed { get; set; }   // 移动速度 01~99
        public string Duration { get; set; }    // 持续时间 0~9999 (0.1 秒单位)
    }
}
