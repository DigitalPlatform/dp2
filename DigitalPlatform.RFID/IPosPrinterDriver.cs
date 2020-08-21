using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public interface IPosPrinterDriver
    {
        // 初始化
        NormalResult InitializeDriver(string port, string style);

        // 释放
        NormalResult ReleaseDriver();

        // 打印
        // parameters:
        //      style   附加的子参数 
        NormalResult Print(
            string action,
            string text,
            string style);

        // 获得打印机当前状态
        NormalResult GetStatus(string style);
    }
}
