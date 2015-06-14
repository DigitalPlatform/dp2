using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    internal partial class StartZhengyuanReplicationDlg : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartZhengyuanReplicationDlg()
        {
            InitializeComponent();
        }

        private void StartZhengyuanReplicationDlg_Load(object sender, EventArgs e)
        {

            string strError = "";
            int nRet = 0;


            // 通用启动参数
            bool bForceDumpAll = false;
            bool bForceDumpDay = false;
            bool bAutoDumpDay = false;
            bool bClearFirst = false;
            bool bLoop = true;

            nRet = ParseZhengyuanReplicationParam(this.StartInfo.Param,
                out bForceDumpAll,
                out bForceDumpDay,
                out bAutoDumpDay,
                out bClearFirst,
                out bLoop,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.checkBox_forceDumpComplete.Checked = bForceDumpAll;
            this.checkBox_forceDumpDay.Checked = bForceDumpDay;
            this.checkBox_autoDumpDayChange.Checked = bAutoDumpDay;
            this.checkBox_clearFirst.Checked = bClearFirst;
            this.checkBox_loop.Checked = bLoop;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartZhengyuanReplicationDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {

            // 通用启动参数
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");


                DomUtil.SetAttr(dom.DocumentElement,
                    "forceDumpAll",
                    (this.checkBox_forceDumpComplete.Checked == true ? "yes" : "no"));

                DomUtil.SetAttr(dom.DocumentElement,
        "forceDumpDay",
        (this.checkBox_forceDumpDay.Checked == true ? "yes" : "no"));

                DomUtil.SetAttr(dom.DocumentElement,
                    "autoDumpDay",
                    (this.checkBox_autoDumpDayChange.Checked == true ? "yes" : "no"));

                DomUtil.SetAttr(dom.DocumentElement,
                    "clearFirst",
                    (this.checkBox_clearFirst.Checked == true ? "yes" : "no"));

            DomUtil.SetAttr(dom.DocumentElement,
                "loop",
                (this.checkBox_loop.Checked == true ? "yes" : "no"));

            this.StartInfo.Param = dom.OuterXml;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        // 解析通用启动参数
        // 格式
        /*
         * <root dump='...' clearFirst='...' loop='...'/>
         * dump缺省为false
         * clearFirst缺省为false
         * loop缺省为true
         * 
         * 
         * */
        public static int ParseZhengyuanReplicationParam(
            string strParam,
            out bool bForceDumpAll,
            out bool bForceDumpDay,
            out bool bAutoDumpDay,
            out bool bClearFirst,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bForceDumpAll = false;
            bForceDumpDay = false;
            bAutoDumpDay = false;
            bLoop = true;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数装入XML DOM时出错: " + ex.Message;
                return -1;
            }


            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            string strForceDumpAll = DomUtil.GetAttr(dom.DocumentElement,
    "forceDumpAll");
            if (strForceDumpAll.ToLower() == "yes"
                || strForceDumpAll.ToLower() == "true")
                bForceDumpAll = true;
            else
                bForceDumpAll = false;

            string strForceDumpDay = DomUtil.GetAttr(dom.DocumentElement,
"forceDumpDay");
            if (strForceDumpDay.ToLower() == "yes"
                || strForceDumpDay.ToLower() == "true")
                bForceDumpDay = true;
            else
                bForceDumpDay = false;

            string strAutoDumpDay = DomUtil.GetAttr(dom.DocumentElement,
"autoDumpDay");
            if (strAutoDumpDay.ToLower() == "yes"
                || strAutoDumpDay.ToLower() == "true")
                bAutoDumpDay = true;
            else
                bAutoDumpDay = false;


            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
"loop");
            if (strLoop.ToLower() == "yes"
                || strLoop.ToLower() == "true")
                bLoop = true;
            else
                bLoop = false;

            return 0;
        }
    }
}