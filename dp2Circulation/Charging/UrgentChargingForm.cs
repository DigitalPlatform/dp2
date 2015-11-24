using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 应急出纳窗
    /// </summary>
    public partial class UrgentChargingForm : MyForm
    {
#if NO
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";
        DigitalPlatform.Stop stop = null;

#endif

        FuncState m_funcstate = FuncState.Borrow;

        // 记载连续输入的册条码号
        List<BarcodeAndTime> m_itemBarcodes = new List<BarcodeAndTime>();

        const int WM_PREPARE = API.WM_USER + 200;
        const int WM_SCROLLTOEND = API.WM_USER + 201;
        const int WM_SWITCH_FOCUS = API.WM_USER + 202;

        Hashtable m_textTable = new Hashtable();
        int m_nTextNumber = 0;


        // 消息WM_SWITCH_FOCUS的wparam参数值
        const int READER_BARCODE = 0;
        // const int READER_PASSWORD = 1;
        const int ITEM_BARCODE = 2;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UrgentChargingForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 是否要自动校验条码号
        /// </summary>
        public bool NeedVerifyBarcode
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "verify_barcode",
                    false);
            }
        }

        /// <summary>
        /// 信息对话框的不透明度
        /// </summary>
        public double InfoDlgOpacity
        {
            get
            {
                return (double)this.MainForm.AppInfo.GetInt(
                    "charging_form",
                    "info_dlg_opacity",
                    100) / (double)100;
            }
        }

        bool DoubleItemInputAsEnd
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "doubleItemInputAsEnd",
                    false);
            }

        }

        private void UrgentChargingForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);


            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.MainForm.Urgent = true;

            this.FuncState = this.FuncState;    // 使"操作"按钮文字显示正确
            Global.WriteHtml(this.webBrowser_operationInfo,
                "<pre>");
            EnableControls(false);

            API.PostMessage(this.Handle, WM_PREPARE, 0, 0);

        }

        private void UrgentChargingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }
#endif
        }

        private void UrgentChargingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null) 
                this.MainForm.Urgent = false;
        }

        string LogFileName
        {
            get
            {
                return this.MainForm.DataDir + "\\urgent_charging.txt";
            }
        }

        // 写入日志文件
        // 格式：功能 读者证条码号 册条码号 操作时间
        int WriteLogFile(string strFunc,
            string strReaderBarcode,
            string strItemBarcode,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strFunc) == true)
            {
                strError = "strFunc不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(strReaderBarcode) == true
                && String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "strReaderBarcode和strItemBarcode不能同时为空";
                return -1;
            }

            string strTime = DateTimeUtil.Rfc1123DateTimeString(DateTime.UtcNow);

            string strLine = strFunc + "\t" + strReaderBarcode + "\t" + strItemBarcode + "\t" + strTime + "\r\n";

            StreamUtil.WriteText(this.LogFileName,
                strLine);

            Global.WriteHtml(this.webBrowser_operationInfo,
                strLine);
            Global.ScrollToEnd(this.webBrowser_operationInfo);


            return 0;
        }

        private void button_loadReader_Click(object sender, EventArgs e)
        {
            if (this.textBox_readerBarcode.Text == "")
            {
                MessageBox.Show(this, "读者证条码号为空。");

                this.textBox_readerBarcode.SelectAll();
                this.textBox_readerBarcode.Focus();

                return;
            }

            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus();
        }

        private void button_itemAction_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.textBox_itemBarcode.Text == "")
            {
                strError = "册条码号不能为空。";
                goto ERROR1;
            }

            if (this.DoubleItemInputAsEnd == true)
            {

                // 取出上次输入的最后一个条码，和目前输入的条码比较，看是否一样。
                if (this.m_itemBarcodes.Count > 0)
                {
                    string strLastItemBarcode = this.m_itemBarcodes[m_itemBarcodes.Count - 1].Barcode;
                    TimeSpan delta = DateTime.Now - this.m_itemBarcodes[m_itemBarcodes.Count - 1].Time;
                    // MessageBox.Show(this, delta.TotalMilliseconds.ToString());
                    if (strLastItemBarcode == this.textBox_itemBarcode.Text
                        && delta.TotalMilliseconds < 5000) // 5秒以内
                    {
                        // 清除册条码号输入域
                        this.textBox_itemBarcode.Text = "";
                        // 清除读者证条码号输入域
                        this.textBox_readerBarcode.Text = "请输入下一个读者的证条码号...";
                        this.SwitchFocus(READER_BARCODE, null);
                        return;
                    }
                }


                BarcodeAndTime barcodetime = new BarcodeAndTime();
                barcodetime.Barcode = this.textBox_itemBarcode.Text;
                barcodetime.Time = DateTime.Now;

                this.m_itemBarcodes.Add(barcodetime);
                // 仅仅保持一个条码就可以了
                while (this.m_itemBarcodes.Count > 1)
                    this.m_itemBarcodes.RemoveAt(0);
            }

            if (this.FuncState == FuncState.Borrow)
            {
                if (this.textBox_readerBarcode.Text == "")
                {
                    strError = "读者证条码号不能为空。";
                    goto ERROR1;
                }


                nRet = this.WriteLogFile("borrow",
                    this.textBox_readerBarcode.Text,
                    this.textBox_itemBarcode.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strFastInputText = ChargingInfoDlg.Show(this.CharingInfoHost,
    "读者 " + this.textBox_readerBarcode.Text
    + " 借阅册 "
    + this.textBox_itemBarcode.Text + " 成功。",
    InfoColor.Green,
    "caption",
    this.InfoDlgOpacity,
                        this.MainForm.DefaultFont);
                this.SwitchFocus(ITEM_BARCODE, strFastInputText);
            }

            if (this.FuncState == FuncState.Return)
            {
                nRet = this.WriteLogFile("return",
    this.textBox_readerBarcode.Text,
    this.textBox_itemBarcode.Text,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strFastInputText = ChargingInfoDlg.Show(this.CharingInfoHost,
" 册 "
+ this.textBox_itemBarcode.Text + " 还回成功。",
InfoColor.Green,
"caption",
this.InfoDlgOpacity,
                        this.MainForm.DefaultFont);
                this.SwitchFocus(ITEM_BARCODE, strFastInputText);

            }

            if (this.FuncState == FuncState.VerifyReturn)
            {
                if (this.textBox_readerBarcode.Text == "")
                {
                    strError = "读者证条码号不能为空。";
                    goto ERROR1;
                }

                nRet = this.WriteLogFile("return",
    this.textBox_readerBarcode.Text,
    this.textBox_itemBarcode.Text,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strFastInputText = ChargingInfoDlg.Show(this.CharingInfoHost,
"读者 " + this.textBox_readerBarcode.Text
+ " 还回册 "
+ this.textBox_itemBarcode.Text + " 成功。",
InfoColor.Green,
"caption",
this.InfoDlgOpacity,
                        this.MainForm.DefaultFont);
                this.SwitchFocus(ITEM_BARCODE, strFastInputText);

            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);


        }

        // 带有焦点切换功能的
        /// <summary>
        /// 当前功能状态
        /// </summary>
        public FuncState SmartFuncState
        {
            get
            {
                return m_funcstate;
            }
            set
            {
                FuncState old_funcstate = this.m_funcstate;

                this.FuncState = value;

                // 切换为不同的功能的时候，定位焦点
                if (old_funcstate != this.m_funcstate)
                {
                    if (this.m_funcstate != FuncState.Return)
                    {
                        this.textBox_readerBarcode.SelectAll();
                        this.textBox_itemBarcode.SelectAll();

                        this.textBox_readerBarcode.Focus();
                    }
                    else
                    {
                        this.textBox_itemBarcode.SelectAll();

                        this.textBox_itemBarcode.Focus();
                    }
                }
                else // 重复设置为同样功能，当作清除功能
                {
                    this.textBox_readerBarcode.Text = "";
                    this.textBox_itemBarcode.Text = "";

                    if (this.m_funcstate != FuncState.Return)
                    {
                        this.textBox_readerBarcode.Focus();
                    }
                    else
                    {
                        this.textBox_itemBarcode.Focus();
                    }

                }
            }
        }

        FuncState FuncState
        {
            get
            {
                return m_funcstate;
            }
            set
            {
                // 清除记忆的册条码号
                this.m_itemBarcodes.Clear();

                if (this.m_funcstate != value
                    && value == FuncState.Return)
                    MessageBox.Show(this, "警告：使用不带验证读者证条码号的还回功能，会影响日后数据库恢复到数据库时的容错能力。请慎用此功能。");


                this.m_funcstate = value;

                this.toolStripMenuItem_borrow.Checked = false;
                this.toolStripMenuItem_return.Checked = false;
                this.toolStripMenuItem_verifyReturn.Checked = false;

                if (m_funcstate == FuncState.Borrow)
                {
                    this.button_itemAction.Text = "借";
                    this.toolStripMenuItem_borrow.Checked = true;
                    this.textBox_readerBarcode.Enabled = true;
                }
                if (m_funcstate == FuncState.Return)
                {
                    this.button_itemAction.Text = "还";
                    this.toolStripMenuItem_return.Checked = true;
                    this.textBox_readerBarcode.Text = "";
                    this.textBox_readerBarcode.Enabled = false;
                }
                if (m_funcstate == FuncState.VerifyReturn)
                {
                    this.button_itemAction.Text = "验证还";
                    this.toolStripMenuItem_verifyReturn.Checked = true;
                    this.textBox_readerBarcode.Enabled = true;
                }

            }
        }

        private void toolStripMenuItem_borrow_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Borrow;
        }

        private void toolStripMenuItem_return_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Return;
        }

        private void toolStripMenuItem_verifyReturn_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.VerifyReturn;
        }


        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_loadReader;
        }

        private void textBox_readerPassword_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_verifyReaderPassword;
        }

        private void textBox_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_itemAction;
        }


        // 从文件中装入内容到浏览器
        int LoadLogFileContentToBrowser(out string strError)
        {
            strError = "";

            string strLogFileName = this.LogFileName;
            int nLineCount = 0;
            try
            {
                using (StreamReader sr = new StreamReader(strLogFileName, true))
                {
                    for (; ; )
                    {
                        string strLine = sr.ReadLine();
                        if (strLine == null)
                            break;
                        Global.WriteHtml(this.webBrowser_operationInfo,
            strLine + "\r\n");
                        nLineCount++;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "读取文件过程出错: " + ex.Message;
                return -1;
            }

            Global.WriteHtml(this.webBrowser_operationInfo,
                "--- 此前 " + nLineCount + " 行为应急日志文件 " + this.LogFileName + " 中已经存储的内容 ---\r\n");

            // Global.ScrollToEnd(this.webBrowser_operationInfo);
            API.PostMessage(this.Handle, WM_SCROLLTOEND, 0, 0);


            return 0;
        }

        void SwitchFocus(int target,
    string strFastInput)
        {
            // 提防hashtable越来越大
            if (this.m_textTable.Count > 5)
            {
                Debug.Assert(false, "");
                this.m_textTable.Clear();
            }

            int nNumber = -1;   // -1表示不需要传递字符串参数

            // 如果需要传递字符串参数
            if (String.IsNullOrEmpty(strFastInput) == false)
            {
                string strNumber = this.m_nTextNumber.ToString();
                nNumber = this.m_nTextNumber;
                this.m_nTextNumber++;
                if (this.m_nTextNumber == -1)   // 避开-1
                    this.m_nTextNumber++;

                this.m_textTable[strNumber] = strFastInput;
            }

            API.PostMessage(this.Handle, WM_SWITCH_FOCUS,
                target, nNumber);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PREPARE:
                    {
                        string strError = "";

                        int nRet = LoadLogFileContentToBrowser(out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);

                        // 然后许可界面
                        EnableControls(true);
                        return;
                    }
                //break;
                case WM_SCROLLTOEND:
                    Global.ScrollToEnd(this.webBrowser_operationInfo);
                    break;
                case WM_SWITCH_FOCUS:
                    {
                        string strFastInputText = "";
                        int nNumber = (int)m.LParam;

                        if (nNumber != -1)
                        {
                            string strNumber = nNumber.ToString();
                            strFastInputText = (string)this.m_textTable[strNumber];
                            this.m_textTable.Remove(strNumber);
                        }

                        if (String.IsNullOrEmpty(strFastInputText) == false)
                        {
                            if ((int)m.WParam == READER_BARCODE)
                            {
                                if (this.FuncState == FuncState.Return)
                                    this.FuncState = FuncState.Borrow;

                                this.textBox_readerBarcode.Text = strFastInputText;
                                this.button_loadReader_Click(this, null);
                            }
                            if ((int)m.WParam == ITEM_BARCODE)
                            {
                                this.textBox_itemBarcode.Text = strFastInputText;
                                this.button_itemAction_Click(this, null);
                            }

                            return;
                        }

                        if ((int)m.WParam == READER_BARCODE)
                        {
                            if (this.FuncState == FuncState.Return)
                                this.FuncState = FuncState.Borrow;

                            this.textBox_readerBarcode.SelectAll();
                            this.textBox_readerBarcode.Focus();
                        }

                        if ((int)m.WParam == ITEM_BARCODE)
                        {
                            this.textBox_itemBarcode.SelectAll();
                            this.textBox_itemBarcode.Focus();
                        }

                        return;
                    }
                // break;

            }
            base.DefWndProc(ref m);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_itemBarcode.Enabled = bEnable;
            this.textBox_readerBarcode.Enabled = bEnable;

            this.button_itemAction.Enabled = bEnable;
            this.button_loadReader.Enabled = bEnable;
        }

        private void UrgentChargingForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);

            this.MainForm.toolButton_amerce.Enabled = false;
            /*
            this.toolButton_borrow.Enabled = true;
            this.toolButton_return.Enabled = true;
            this.MainForm.toolButton_verifyReturn.Enabled = true;
             * */
            this.MainForm.toolButton_lost.Enabled = false;
            this.MainForm.toolButton_readerManage.Enabled = false;
            this.MainForm.toolButton_renew.Enabled = false;

            this.MainForm.toolStripDropDownButton_barcodeLoadStyle.Enabled = false;
            this.MainForm.toolStripTextBox_barcode.Enabled = false;

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = true;

            this.MainForm.Urgent = true;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        // 
        /// <summary>
        /// 恢复应急日志文件到服务器
        /// </summary>
        public void Recover()
        {
            string strError = "";
            int nRet = 0;

            string strLogFileName = this.LogFileName;
            int nLineCount = 0;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            EnableControls(false);

            Global.WriteHtml(this.webBrowser_operationInfo,
    "开始恢复。\r\n");


            try
            {
                using (StreamReader sr = new StreamReader(strLogFileName, true))
                {
                    for (; ; )
                    {
                        string strLine = sr.ReadLine();
                        if (strLine == null)
                            break;

                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        string strXml = "";
                        nRet = BuildRecoverXml(
                            strLine,
                            out strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        long lRet = this.Channel.UrgentRecover(
                            stop,
                            strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
    "行\r\n" + strLine + "\r\n恢复到数据库时出错：" + strError + "。\r\n\r\n要中断处理么? ",
    "UrgentChargingForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.Yes)
                                goto ERROR1;

                            Global.WriteHtml(this.webBrowser_operationInfo,
    strLine + " *** error: " + strError + "\r\n");
                            goto CONTINUE_1;
                        }

                        Global.WriteHtml(this.webBrowser_operationInfo,
            strLine + "\r\n");
                    CONTINUE_1:
                        Global.ScrollToEnd(this.webBrowser_operationInfo);

                        nLineCount++;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strLogFileName + "不存在。";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "读取文件过程出错: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            Global.WriteHtml(this.webBrowser_operationInfo,
                "恢复完成。共处理记录 " + nLineCount + " 个。 \r\n");
            Global.WriteHtml(this.webBrowser_operationInfo,
    "注意打开数据目录，改名保存 " + this.LogFileName + " 文件，避免将来不小心重复恢复。\r\n");

            API.PostMessage(this.Handle, WM_SCROLLTOEND, 0, 0);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        /*
<root>
  <operation>borrow</operation> 操作类型
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <itemBarcode>0000001</itemBarcode>  册条码号
  <borrowDate>Fri, 08 Dec 2006 04:17:31 GMT</borrowDate> 借阅日期
  <borrowPeriod>30day</borrowPeriod> 借阅期限
  <no>0</no> 续借次数。0为首次普通借阅，1开始为续借
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:31 GMT</operTime> 操作时间
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
</root>
         * 
         * 
 <root>
  <operation>return</operation> 操作类型
  <action>...</action> 具体动作 有return lost两种
  <itemBarcode>0000001</itemBarcode> 册条码号
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
  <overdues>...</overdues> 超期或丢失赔款信息 通常内容为一个字符串，为一个或多个<overdue>元素XML文本片断
  
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
  <lostComment>...</lostComment> 关于丢失情况的附注(追加写入册记录<comment>的信息)
</root>
         * * */
        int BuildRecoverXml(
            string strLine,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            string[] cols = strLine.Split(new char[] { '\t' });


            if (cols.Length < 4)
            {
                strError = "strLine[" + strLine + "]格式不正确，应为4栏内容。";
                return -1;
            }

            string strFunction = cols[0];
            string strReaderBarcode = cols[1];
            string strItemBarcode = cols[2];
            string strOperTime = cols[3];

            string strUserName =
this.MainForm.AppInfo.GetString(
"default_account",
"username",
"");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            if (strFunction == "borrow")
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    "operation",
                    "borrow");

                DomUtil.SetElementText(dom.DocumentElement,
    "readerBarcode",
    strReaderBarcode);

                DomUtil.SetElementText(dom.DocumentElement,
    "itemBarcode",
    strItemBarcode);

                // no
                DomUtil.SetElementText(dom.DocumentElement,
"no",
"0");

                // borrowDate
                DomUtil.SetElementText(dom.DocumentElement,
"borrowDate",
strOperTime);

                // defaultBorrowPeriod
                DomUtil.SetElementText(dom.DocumentElement,
"defaultBorrowPeriod",
"60day");
                // 注：不能鲁莽写入<borrowPeriod>，而是写入<defaulBorrowPeriod>
                // 因为先要给服务器机会，让它探测读者类型针对册类型的借期参数，实在不行才采用这里给出的缺省参数。


                // operTime
                DomUtil.SetElementText(dom.DocumentElement,
"operTime",
strOperTime);



                // operator
                DomUtil.SetElementText(dom.DocumentElement,
"operator",
strUserName);

            }
            else if (strFunction == "return")
            {
                DomUtil.SetElementText(dom.DocumentElement,
    "operation",
    "return");
                DomUtil.SetElementText(dom.DocumentElement,
"action",
"return");

                // 2006/12/30 
                if (String.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    DomUtil.SetElementText(dom.DocumentElement,
    "readerBarcode",
    strReaderBarcode);
                }


                DomUtil.SetElementText(dom.DocumentElement,
    "itemBarcode",
    strItemBarcode);

                // operTime
                DomUtil.SetElementText(dom.DocumentElement,
"operTime",
strOperTime);

                // operator
                DomUtil.SetElementText(dom.DocumentElement,
"operator",
strUserName);

            }
            else
            {
                strError = "不能识别的function '" + strFunction + "'";
                return -1;
            }

            strXml = dom.OuterXml;

            return 0;
        }

        /// <summary>
        /// 获得出纳信息的宿主
        /// </summary>
        internal ChargingInfoHost CharingInfoHost
        {
            get
            {
                ChargingInfoHost host = new ChargingInfoHost();
                host.ap = MainForm.AppInfo;
                host.window = this;
                return host;
            }
        }
    }
}