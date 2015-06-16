using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;


namespace dp2Circulation
{

    public partial class EntityRegisterForm : MyForm
    {
        FloatingMessageForm _floatingMessage = null;

        bool _scanMode = false;

        /// <summary>
        /// 是否处在扫描条码的状态？
        /// </summary>
        public bool ScanMode
        {
            get
            {
                return this._scanMode;
            }
            set
            {
                if (this._scanMode == value)
                    return;

                this._scanMode = value;

                // this.button_load_scanBarcode.Enabled = !this._scanMode;

                if (this._scanMode == false)
                {
                    if (this._scanBarcodeForm != null)
                        this._scanBarcodeForm.Close();
                }
                else
                {
                    // button_load_scanBarcode_Click(this, new EventArgs());
                    OpenScanBarcodeForm();
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityRegisterForm()
        {
            InitializeComponent();

            this.entityRegisterControl1.HideSelection = false;

            this.entityRegisterControl1.AutoSize = true;
            this.entityRegisterControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.entityRegisterControl1.AutoScroll = true;

            this.AutoScroll = false;
#if NO
            this.entityRegisterControl1.AutoSize = true;
            this.entityRegisterControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.entityRegisterControl1.AutoScroll = false;

            this.AutoScroll = true;
#endif

            this.entityEditControl_quickRegisterDefault.DisplayMode = "simple_register";
            this.entityEditControl_quickRegisterDefault.BackColor = SystemColors.Control;
            this.entityEditControl_quickRegisterDefault.ForeColor = SystemColors.ControlText;
            this.entityEditControl_quickRegisterDefault.BorderStyle = BorderStyle.None;

            this.entityRegisterControl1.ColorControl = this.colorSummaryControl1;
        }

        private void EntityRegisterForm_Load(object sender, EventArgs e)
        {
            // this.entityRegisterControl1.Channel = this.Channel;
            this.entityRegisterControl1.Progress = this.stop;
            this.entityRegisterControl1.MainForm = this.MainForm;
            string strFileName = Path.Combine(this.MainForm.DataDir, "ajax-loader.gif");
            this.entityRegisterControl1.LoaderImage = Image.FromFile(strFileName);

            // TODO: 异步加快窗口打开速度?
            LoadServerXml();

            OpenScanBarcodeForm();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.Show(this);
            }
        }

        void LoadServerXml()
        {
            // 当前登录的主要服务器不同，则需要的 xml 配置文件是不同的。应当存储在各自的目录中
            string strFileName = Path.Combine(this.MainForm.ServerCfgDir, ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName()) + "\\servers.xml");
            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strFileName));

            if (File.Exists(strFileName) == false
                || MainForm.GetServersCfgFileVersion(strFileName) < MainForm.SERVERSXML_VERSION)
            {
                string strError = "";
                // 创建 servers.xml 配置文件
                int nRet = this.MainForm.BuildServersCfgFile(strFileName,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "文件 '" + strFileName + "' 装入XMLDOM 时出错: " + ex.Message);
                return;
            }

            // TODO: 是否在文件不存在的情况下，给出缺省的几个 server ?

            this.entityRegisterControl1.ServersDom = dom;
        }

        private void EntityRegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.entityRegisterControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "EntityRegisterForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void EntityRegisterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this._scanBarcodeForm != null)
                this._scanBarcodeForm.Close();
            if (_floatingMessage != null)
                _floatingMessage.Close();
        }

        void SetFloatMessage(string strColor,
    string strText)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string, string>(SetFloatMessage), strColor, strText);
                return;
            }

            if (strColor == "waiting")
                this._floatingMessage.RectColor = Color.FromArgb(80, 80, 80);
            else
                this._floatingMessage.RectColor = Color.Purple;

            this._floatingMessage.Text = strText;
        }

        ScanRegisterBarcodeForm _scanBarcodeForm = null;

        void OpenScanBarcodeForm()
        {
            if (this._scanBarcodeForm == null)
            {
                this._scanBarcodeForm = new ScanRegisterBarcodeForm();
                MainForm.SetControlFont(this._scanBarcodeForm, this.Font, false);
                this._scanBarcodeForm.BackColor = SystemColors.ControlDark;
                this._scanBarcodeForm.BarcodeScaned += new ScanedEventHandler(_scanBarcodeForm_BarcodeScaned);
                this._scanBarcodeForm.FormClosed += new FormClosedEventHandler(_scanBarcodeForm_FormClosed);
                this._scanBarcodeForm.Show(this);
            }
            else
            {
                if (this._scanBarcodeForm.WindowState == FormWindowState.Minimized)
                    this._scanBarcodeForm.WindowState = FormWindowState.Normal;
            }

            this.entityRegisterControl1.BeginThread();
        }

        void _scanBarcodeForm_BarcodeScaned(object sender, ScanedEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(e.Barcode) == true)
            {
                Console.Beep();
                return;
            }

            // 自动切换到 登记 属性页，避免操作者看不到扫入了什么内容
            if (this.tabControl_main.SelectedTab != this.tabPage_register)
                this.tabControl_main.SelectedTab = this.tabPage_register;

            // 清除浮动的错误信息
            this._floatingMessage.Text = "";

            // 把册条码号直接加入行中，然后等待专门的线程来装载刷新
            // 要查重
#if NO
            ListViewItem dup = ListViewUtil.FindItem(this.listView_in, e.Barcode, COLUMN_BARCODE);
            if (dup != null)
            {
                Console.Beep();
                ListViewUtil.SelectLine(dup, true);
                MessageBox.Show(this, "您扫入的册条码号 ‘" + e.Barcode + "’ 在列表中已经存在了，请注意不要重复扫入");
                this._scanBarcodeForm.Activate();
                return;
            }

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, e.Barcode);
            this.listView_in.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();
#endif
            string strText = e.Barcode;

            // 如果是 ISBN，则新装入一个书目记录
            // TODO: 装入前需要对整个 list 进行查重，如果前面已经有同样 ISBN 的书目，要提醒
            if (QuickChargingForm.IsISBN(ref strText) == true)
            {
#if NO
                RegisterLine line = new RegisterLine(this.entityRegisterControl1);
                line.BiblioBarcode = strText;

                this.entityRegisterControl1.InsertNewLine(0, line, true);
                line.label_color.Focus();

                this.entityRegisterControl1.SetColorList();

                this.entityRegisterControl1.AddTask(line, "search_biblio");

                // 选定刚新增的事项
                this.entityRegisterControl1.SelectItem(line, true);
                // 确保事项可见
                this.entityRegisterControl1.EnsureVisible(line);
#endif
                this.entityRegisterControl1.AddNewBiblio(strText);
            }
            else
            {
                // 当作册条码号进入
                nRet = this.entityRegisterControl1.AddNewEntity(strText,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // this.entityRegisterControl1.ActivateThread();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            this._scanBarcodeForm.Focus();
        }

        void _scanBarcodeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._scanBarcodeForm = null;
            this.entityRegisterControl1.StopThread();
        }

        private void entityRegisterControl1_SizeChanged(object sender, EventArgs e)
        {

#if NO
            if (this.ClientSize.Width > 100)
            {
                if (this.entityRegisterControl1.Width > this.ClientSize.Width)
                    this.entityRegisterControl1.Width = this.ClientSize.Width;
            }
#endif

            // this.AutoScrollMinSize = this.entityRegisterControl1.Size;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

#if NO
            if (this.ClientSize.Width > 100)
            {
                if (this.entityRegisterControl1.Width > this.ClientSize.Width - 50)
                    this.entityRegisterControl1.Width = this.ClientSize.Width - 50;
            }
#endif
        }

        private void tabPage_defaultTemplate_Enter(object sender, EventArgs e)
        {
            // 册快速登记
            {
                string strError = "";
                string strQuickDefault = this.MainForm.AppInfo.GetString(
                    "entityform_optiondlg",
                    "quickRegister_default",
                    "<root />");
                int nRet = this.entityEditControl_quickRegisterDefault.SetData(strQuickDefault,
                     "",
                     null,
                     out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                this.entityEditControl_quickRegisterDefault.SetReadOnly("librarian");
                // this.entityEditControl_quickRegisterDefault.GetValueTable += new GetValueTableEventHandler(entityEditControl_GetValueTable);
            }
        }

        private void tabPage_defaultTemplate_Leave(object sender, EventArgs e)
        {
            // 册快速登记
            {
                string strError = "";
                string strQuickDefault = "";

                this.entityEditControl_quickRegisterDefault.ParentId = "?";
                int nRet = this.entityEditControl_quickRegisterDefault.GetData(
                    true,
                    out strQuickDefault,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                {
                    this.MainForm.AppInfo.SetString(
                        "entityform_optiondlg",
                        "quickRegister_default",
                        strQuickDefault);
                }
            }
        }

        private void entityEditControl_quickRegisterDefault_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void entityRegisterControl1_DisplayError(object sender, DisplayErrorEventArgs e)
        {
            SetFloatMessage(e.Color, e.Text);
        }

        private void tabPage_register_SizeChanged(object sender, EventArgs e)
        {
        }

        private void tabPage_register_Resize(object sender, EventArgs e)
        {
            this.panel1.Size = new Size(this.tabPage_register.ClientSize.Width, this.colorSummaryControl1.Location.Y - this.panel1.Location.Y);
        }

        private void colorSummaryControl1_Click(object sender, EventArgs e)
        {
            Point pt = Control.MousePosition;
            pt = this.colorSummaryControl1.PointToClient(pt);
            int index = this.colorSummaryControl1.HitTest(pt.X, pt.Y);
            // MessageBox.Show(this, index.ToString());
            this.entityRegisterControl1.SelectLine(index);
        }

        private void button_start_createCfgFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // string strCfgFileName = Path.Combine(this.MainForm.DataDir, "servers.xml");
            // 当前登录的主要服务器不同，则需要的 xml 配置文件是不同的。应当存储在各自的目录中
            string strFileName = Path.Combine(this.MainForm.ServerCfgDir, ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName()) + "\\servers.xml");
            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strFileName));

            if (File.Exists(strFileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
"当前已经存在配置文件 '" + strFileName + "'。若重新创建配置文件，以前的内容将被覆盖。\r\n\r\n确实要重新创建配置文件? ",
"EntityRegisterForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            // 创建 servers.xml 配置文件
            int nRet = this.MainForm.BuildServersCfgFile(strFileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            LoadServerXml();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region 显示属性



        #endregion // 显示属性
    }

}
