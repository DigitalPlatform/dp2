using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;

using DigitalPlatform.Xml;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// XmlStatisForm (XML统计窗) 统计方案的宿主类
    /// </summary>
    public class XmlStatis : StatisHostBase
    {
        /// <summary>
        /// 输入文件名
        /// </summary>
        public string InputFilename = "";

        /// <summary>
        /// 本对象所关联的 XmlStatisForm (XML统计窗)
        /// </summary>
        public XmlStatisForm XmlStatisForm = null;	// 引用

        /// <summary>
        /// 当前正在处理的 XML 记录 在整批中的下标。从 0 开始计数。如果为 -1，表示尚未开始处理
        /// </summary>
        public long CurrentRecordIndex = -1; // 当前XML记录在整批中的偏移量


        /// <summary>
        /// 当前正在处理的 XML 记录，XmlDocument 类型
        /// </summary>
        public XmlDocument RecordDom = null;    // Xml装入XmlDocument

        string m_strXml = "";    // XML记录体

        /// <summary>
        /// 当前正在处理的 XML 记录，字符串类型
        /// </summary>
        public string Xml
        {
            get
            {
                return this.m_strXml;
            }
            set
            {
                this.m_strXml = value;
            }
        }

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.XmlStatisForm.MainForm.DataDir, "~xml_statis");
        }

    }
}
