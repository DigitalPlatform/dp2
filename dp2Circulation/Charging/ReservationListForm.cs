
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 列出待办预约请求的窗口
    /// 主要针对读者个人藏书管理而设计
    /// </summary>
    public partial class ReservationListForm : MyForm
    {
        public ReservationListForm()
        {
            InitializeComponent();
        }

        private void ReservationListForm_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(ListReservations));
        }

        private void ReservationListForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReservationListForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.toolStrip1.Enabled = bEnable;
        }

        class MyReaderInfo
        {
            public string ReaderXml = "";
            public string LibraryCode = "";
            public string PersonalLibrary = "";
            public string Name = "";
        }

        // 获得登录者的读者信息。登录者应为读者身份
        // return:
        //      -1  出错
        //      0   读者记录不存在
        //      1   成功
        int GetMyReaderInfo(out MyReaderInfo info,
            out string strError)
        {
            strError = "";
            info = new MyReaderInfo();

            string strUserName = this.MainForm.AppInfo.GetString(
    "default_account",
    "username",
    "");
            bool bIsReader = this.MainForm.AppInfo.GetBoolean(
    "default_account",
    "isreader",
    false);
            if (bIsReader == false)
            {
                strError = "当前用户不是读者身份，无法使用本窗口";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得读者记录 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                stop.SetMessage("正在装入读者记录 " + strUserName + " ...");

                string[] results = null;
                byte[] baTimestamp = null;
                string strRecPath = "";
                long lRet = Channel.GetReaderInfo(
                    stop,
                    strUserName,
                    "xml",   // this.RenderFormat, // "html",
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    strError = "证条码号为 '" + strUserName + "' 的读者记录没有找到 ...";
                    return 0;   // not found
                }
                if (lRet == -1)
                    return -1;

                if (results == null || results.Length == 0)
                {
                    strError = "返回的results不正常。";
                    return -1;
                }
                string strResult = "";
                strResult = results[0];

                if (lRet > 1)
                {
                    strError = "证条码号为 '" + strUserName + "' 的读者记录命中多条 ("+lRet+") ...";
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strResult);
                }
                catch(Exception ex)
                {
                    strError = "XML 装入 DOM 时出错" + ex.Message;
                    return -1;
                }

                info.ReaderXml = strResult;
                info.Name = DomUtil.GetElementText(dom.DocumentElement, "name");
                // 根据读者库查到馆代码
                info.LibraryCode = this.MainForm.GetReaderDbLibraryCode(Global.GetDbName(strRecPath));
                info.PersonalLibrary = DomUtil.GetElementText(dom.DocumentElement, "personalLibrary");

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        }

        // 列出所有预约到书信息
        int ListReservations(MyReaderInfo info,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(info.PersonalLibrary) == true)
            {
                strError = "当前读者 '"+info.Name+"' 没有个人书斋";
                return -1;
            }

            if (string.IsNullOrEmpty(this.MainForm.ArrivedDbName) == true)
            {
                strError = "当前服务器尚未配置预约到书库名";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索预约到书记录 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strQueryWord = "";
                if(string.IsNullOrEmpty(info.LibraryCode) == true)
                    strQueryWord = info.PersonalLibrary;
                else
                    strQueryWord = info.LibraryCode + "/" + info.PersonalLibrary;
                string strFrom = "馆藏地点";
                string strMatchStyle = "exact";
                string strQueryXml = "<target list='" + this.MainForm.ArrivedDbName + ":"
                    + strFrom + "'><item><word>"
    + StringUtil.GetXmlStringSimple(strQueryWord)
    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";

                string strOutputStyle = "";
                long lRet = Channel.Search(stop,
                    strQueryXml,
                    "",
                    strOutputStyle,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "中断";
                        return -1;
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,xml", // bOutputKeyCount == true ? "keycount" : "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error";
                        return -1;
                    }

                    if (lRet == 0)
                        return 0;

                    List<string> paths = new List<string>();

                    int i = 0;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                    {
                        // record.RecordBody.Xml;
                        paths.Add(record.Path);
                        stop.SetProgressValue(lStart + i);
                        i++;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    stop.SetProgressValue(lStart);
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        }

        void ListReservations()
        {
            string strError = "";

            MyReaderInfo info = null;
            // 获得登录者的读者信息。登录者应为读者身份
            // return:
            //      -1  出错
            //      0   读者记录不存在
            //      1   成功
            int nRet = GetMyReaderInfo(out info,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 列出所有预约到书信息
            nRet = ListReservations(info,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
