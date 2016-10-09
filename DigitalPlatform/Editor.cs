using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    class Editor
    {
    }

    // 调用数据加工模块
    public delegate void GenerateDataEventHandler(object sender,
        GenerateDataEventArgs e);

    public class GenerateDataEventArgs : EventArgs
    {
        // 当前焦点所在的子控件
        public object FocusedControl = null;

        // 2009/2/27
        public string ScriptEntry = ""; // 入口函数名。如果为空，则会调用Main(object sender, GenerateDataEventArgs e)

        public bool ShowErrorBox = true;    // [in]是否要显示出错MessageBox
        public string ErrorInfo = "";   // [out]出错信息

        // 2015/5/29
        // 附加的参数对象。具体类型由每个功能规定
        public object Parameter = null;
    }


    // Ctrl+?键被触发
    public delegate void ControlLetterKeyPressEventHandler(object sender,
        ControlLetterKeyPressEventArgs e);

    public class ControlLetterKeyPressEventArgs : EventArgs
    {
        public Keys KeyData = Keys.Control;
        public bool Handled = false;
    }

    //

    // 获得书目信息
    public delegate void GetBiblioEventHandler(object sender,
        GetBiblioEventArgs e);

    public class GetBiblioEventArgs : EventArgs
    {
        // 数据记录
        public string Data { get; set; }
        // 格式。unimarc/usmarc/xml
        public string Syntax { get; set; }
    }

}
