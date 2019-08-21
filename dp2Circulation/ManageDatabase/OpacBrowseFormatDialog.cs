using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// OPAC记录浏览格式定义对话框
    /// </summary>
    internal partial class OpacBrowseFormatDialog : Form
    {
        XmlDocument dom = null;

        public OpacBrowseFormatDialog()
        {
            InitializeComponent();
        }

        private void OpacBrowseFormatDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = LoadCaptionsXml(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 检查当前内容形式上是否合法
            // return:
            //      -1  检查过程本身出错
            //      0   格式有错误
            //      1   格式没有错误
            int nRet = this.captionEditControl_formatName.Verify(out strError);
            if (nRet <= 0)
            {
                strError = "格式名有问题: " + strError;
                this.captionEditControl_formatName.Focus();
                goto ERROR1;
            }

            if (this.dom == null)
            {
                this.dom = new XmlDocument();
                this.dom.LoadXml("<format />");
            }
            this.dom.DocumentElement.InnerXml = this.captionEditControl_formatName.Xml;

            if (String.IsNullOrEmpty(this.FormatName) == true)
            {
                strError = "缺乏语言代码为zh的格式名";
                goto ERROR1;
            }

            // 2019/8/21
            DomUtil.SetAttr(this.dom.DocumentElement, "name", Nulltify(this.FormatName));
            DomUtil.SetAttr(this.dom.DocumentElement, "type", Nulltify(this.FormatType));
            DomUtil.SetAttr(this.dom.DocumentElement, "style", Nulltify(this.FormatStyle));

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        static string Nulltify(string value)
        {
            if (value == "")
                return null;
            return value;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 将dom中的caption片断用来初始化CaptionEditControl
        int LoadCaptionsXml(out string strError)
        {
            strError = "";
            if (this.dom == null)
                return 0;

            try
            {
                this.captionEditControl_formatName.Xml = this.dom.DocumentElement.InnerXml;
            }
            catch (Exception ex)
            {
                strError = "OpacBrowseFormatDialog LoadCaptionsXml() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 1;
        }

        public string FormatName
        {
            get
            {
                if (this.dom == null)
                    return "";

                return DomUtil.GetCaptionExt("zh", this.dom.DocumentElement);
            }
        }


        public string FormatType
        {
            get
            {
                return this.comboBox_type.Text;
            }
            set
            {
                this.comboBox_type.Text = value;
            }
        }

        public string FormatStyle
        {
            get
            {
                return this.textBox_style.Text;
            }
            set
            {
                this.textBox_style.Text = value;
            }
        }

        public string ScriptFile
        {
            get
            {
                return this.textBox_scriptFile.Text;
            }
            set
            {
                this.textBox_scriptFile.Text = value;
            }
        }

        /*
            <format name="详细" type="biblio">
                <caption lang="zh-CN">详细</caption>
                <caption lang="en">Detail</caption>
		    </format>
         * * */

        public string FormatXml
        {
            get
            {
                if (this.dom == null)
                    return "<format />";

                return this.dom.DocumentElement.OuterXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.dom = new XmlDocument();
                    this.dom.LoadXml("<format />");
                    return;
                }

                this.dom = new XmlDocument();
                this.dom.LoadXml(value);
            }
        }

#if NO
        public string CaptionsXml
        {
            get
            {
                if (this.dom == null)
                    return "";

                return this.dom.DocumentElement.InnerXml;
            }
            set
            {
                if (this.dom == null)
                {
                    this.dom = new XmlDocument();
                    this.dom.LoadXml("<format />");
                }

                this.dom.DocumentElement.InnerXml = value;
            }
        }
#endif
        private void button_virtualDatabaseName_newBlankLine_Click(object sender, EventArgs e)
        {
            this.captionEditControl_formatName.NewElement();
        }
    }
}