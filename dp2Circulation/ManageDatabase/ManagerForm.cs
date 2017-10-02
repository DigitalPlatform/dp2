using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 系统管理窗
    /// </summary>
    public partial class ManagerForm : MyForm
    {
        const int TYPE_ZHONGCIHAO_NSTABLE = 0;
        const int TYPE_ZHONGCIHAO_GROUP = 1;
        const int TYPE_ZHONGCIHAO_DATABASE = 2;
        const int TYPE_ZHONGCIHAO_ERROR = 3;

        const int TYPE_ARRANGEMENT_GROUP = 0;
        const int TYPE_ARRANGEMENT_LOCATION = 1;
        const int TYPE_ARRANGEMENT_ERROR = 2;

        // 
        /// <summary>
        /// 表示当前全部数据库信息的XML字符串
        /// </summary>
        public string AllDatabaseInfoXml = "";

#if NO
        const int WM_INITIAL = API.WM_USER + 201;
        const int WM_LOADSIZE = API.WM_USER + 202;
#endif

        string[] type_names = new string[] {
            "biblio","书目",
            "entity","实体",
            "order","订购",
            "issue","期",
            "reader","读者",
            "message","消息",
            "arrived","预约到书",
            "amerce","违约金",
            "invoice","发票",
            "publisher","出版者",
            "zhongcihao","种次号",
            "dictionary","词典",
            "inventory","盘点",
            "pinyin","拼音",
            "gcat","著者号码",
            "word","词",
        };

        // 根据类型汉字名得到类型字符串
        string GetTypeString(string strName)
        {
            for (int i = 0; i < type_names.Length / 2; i++)
            {
                if (type_names[i * 2 + 1] == strName)
                    return type_names[i * 2];
            }

            return null;    // not found
        }

        // 根据类型字符串得到类型汉字名
        internal string GetTypeName(string strTypeString)
        {
            for (int i = 0; i < type_names.Length / 2; i++)
            {
                if (type_names[i * 2] == strTypeString)
                    return type_names[i * 2 + 1];
            }

            return null;    // not found
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ManagerForm()
        {
            InitializeComponent();

            this.listView_opacDatabases.SmallImageList = this.imageList_opacDatabaseType;
            this.listView_opacDatabases.LargeImageList = this.imageList_opacDatabaseType;

            this.listView_databases.SmallImageList = this.imageList_opacDatabaseType;
            this.listView_databases.LargeImageList = this.imageList_opacDatabaseType;

            this.treeView_opacBrowseFormats.ImageList = this.imageList_opacBrowseFormatType;

            this.treeView_zhongcihao.ImageList = this.imageList_zhongcihao;

            this.treeView_arrangement.ImageList = this.imageList_arrangement;

            ListViewProperty prop = new ListViewProperty();
            this.listView_calendar.Tag = prop;

            ColumnSortStyle style = new ColumnSortStyle("calendar_name");
            style.CompareFunc += new CompareEventHandler(prop_CompareColumn);

            // 第一列特殊，记录路径
            prop.SetSortStyle(0, style);
            //prop.CompareColumn -= new CompareEventHandler(prop_CompareColumn);
            //prop.CompareColumn += new CompareEventHandler(prop_CompareColumn); 

            this.loanPolicyControlWrapper1.LoanPolicyControl.Owner = this;
            this.loanPolicyControlWrapper1.PropertyChanged += new PropertyChangedEventHandler(loanPolicyControlWrapper1_PropertyChanged);

            ListViewProperty prop1 = new ListViewProperty();
            this.listView_location_list.Tag = prop1;

            this.kernelResTree1.AppInfo = Program.MainForm.AppInfo;
            this.kernelResTree1.HideIndices = new int[] { KernelResTree.RESTYPE_FROM };
            this.kernelResTree1.DownloadFiles += kernelResTree1_DownloadFiles;
        }

        void kernelResTree1_DownloadFiles(object sender, DownloadFilesEventArgs e)
        {
            string strError = "";
            string strOutputFolder = "";

            List<dp2Circulation.MainForm.DownloadFileInfo> infos = MainForm.BuildDownloadInfoList(e.FileNames);

            bool bAppend = false;
            // 询问是否覆盖已有的目标下载文件。整体询问
            // return:
            //      -1  出错
            //      0   放弃下载
            //      1   同意启动下载
            int nRet = Program.MainForm.AskOverwriteFiles(infos,    // e.FileNames,
ref strOutputFolder,
out bAppend,
out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            // return:
            //      -1  出错
            //      0   放弃下载
            //      1   成功启动了下载
            nRet = Program.MainForm.BeginDownloadFiles(infos,   // e.FileNames,
                bAppend ? "append" : "overwrite",
                null,
                ref strOutputFolder,
                out strError);
            if (nRet == -1)
                e.ErrorInfo = strError;
        }

        void prop_CompareColumn(object sender, CompareEventArgs e)
        {
            if (e.Column.SortStyle.Name == "calendar_name")
            {
                // 比较两个日历名的大小
                // return:
                //      <0  s1 < s2
                //      ==0 s1 == s2
                //      >0  s1 > s2
                e.Result = CompareCalendarName(e.String1, e.String2);
            }
            else
                e.Result = string.Compare(e.String1, e.String2);
        }

        static int CompareCalendarName(string s1, string s2)
        {
            string strLeft1;
            string strRight1;
            string strLeft2;
            string strRight2;
            Global.ParseCalendarName(s1, out strLeft1, out strRight1);
            Global.ParseCalendarName(s2, out strLeft2, out strRight2);

            int nRet = String.Compare(strLeft1, strLeft2);
            if (nRet != 0)
                return nRet;

            return String.Compare(strRight1, strRight2);
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

#if NO
            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
#endif
            this.BeginInvoke(new Action(Initial));
        }

        void Initial()
        {
            string strError = "";
            int nRet = ListAllDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            nRet = ListAllOpacDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            nRet = this.ListAllOpacBrowseFormats(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 列出所有日历
            // 需要在 NewListRightsTables() 以前调用
            nRet = this.ListCalendars(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

#if NO
                        nRet = this.ListRightsTables(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }
#endif

            nRet = this.NewListRightsTables(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 在listview中列出所有馆藏地
            nRet = this.ListAllLocations(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 列出种次号定义
            nRet = this.ListZhongcihao(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            treeView_zhongcihao_AfterSelect(this, null);


            // 列出排架体系定义
            nRet = this.ListArrangement(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            treeView_arrangement_AfterSelect(this, null);


            // 列出脚本
            nRet = this.ListScript(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            nRet = this.ListDup(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 列出值列表
            nRet = this.ListValueTables(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 列出中心服务器
            nRet = this.ListCenter(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

#if NO
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
#if NO
                case WM_LOADSIZE:
                    LoadSize();
                    return;
#endif
#if NO
                case WM_INITIAL:
                    {
                        string strError = "";
                        int nRet = ListAllDatabases(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        nRet = ListAllOpacDatabases(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        nRet = this.ListAllOpacBrowseFormats(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        // 列出所有日历
                        // 需要在 NewListRightsTables() 以前调用
                        nRet = this.ListCalendars(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

#if NO
                        nRet = this.ListRightsTables(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }
#endif

                        nRet = this.NewListRightsTables(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        // 在listview中列出所有馆藏地
                        nRet = this.ListAllLocations(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        // 列出种次号定义
                        nRet = this.ListZhongcihao(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        treeView_zhongcihao_AfterSelect(this, null);


                        // 列出排架体系定义
                        nRet = this.ListArrangement(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        treeView_arrangement_AfterSelect(this, null);


                        // 列出脚本
                        nRet = this.ListScript(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        nRet = this.ListDup(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        // 列出值列表
                        nRet = this.ListValueTables(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                        // 列出中心服务器
                        nRet = this.ListCenter(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }

                    }
                    return;
#endif
            }
            base.DefWndProc(ref m);
        }
#endif

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                for (int i = 0; i < this.listView_opacDatabases.Items.Count; i++)
                {
                    ListViewItem item = this.listView_opacDatabases.Items[i];
                    if (item.ImageIndex == 2)
                        return true;    // 有尚未提交的、先前曾报错的OPAC数据库定义事项
                }

                // TODO: 尚未提交的tree请求 i j 两层循环
                for (int i = 0; i < this.treeView_opacBrowseFormats.Nodes.Count; i++)
                {
                    TreeNode node = this.treeView_opacBrowseFormats.Nodes[i];
                    if (node.ImageIndex == 2)
                        return true;

                    for (int j = 0; j < node.Nodes.Count; j++)
                    {
                        TreeNode sub_node = node.Nodes[j];
                        if (sub_node.ImageIndex == 2)
                            return true;
                    }
                }

                return false;
            }
        }

        private void ManagerForm_FormClosing(object sender, FormClosingEventArgs e)
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

            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (this.LoanPolicyDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内 读者流通权限 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_newLoanPolicy;  // this.tabPage_loanPolicy;
                    return;
                }
            }

            if (this.LocationTypesDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 馆藏地点 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_locations;
                    return;
                }
            }

            if (this.ScriptChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 脚本 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_script;
                    return;
                }
            }

            if (this.ValueTableChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 值列表 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_valueTable;
                    return;
                }
            }

            if (this.ZhongcihaoChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 种次号 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_zhongcihaoDatabases;
                    return;
                }
            }

            if (this.ArrangementChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 排架体系 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_bookshelf;
                    return;
                }
            }

            if (this.DupChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 查重 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_dup;
                    return;
                }
            }

            if (this.CalendarDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内 日历 定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_calendar;
                    return;
                }
            }
        }

        private void ManagerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null)
            {
                stop.Unregister(); // 脱离关联
                stop = null;
            }
#endif

            // SaveSize();
        }

        // 
        int ClearAllDbs(
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在清除所有数据库内数据 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ClearAllDbs(
                    stop,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return 1;
        ERROR1:
            return -1;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // this.button_clearAllDbs.Enabled = bEnable;
            this.toolStrip_databases.Enabled = bEnable;
        }

        private void ManagerForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        // 从服务器获得最新的关于全部数据库的 XML 定义。注意，不刷新界面。
        int RefreshAllDatabaseXml(out string strError)
        {
            strError = "";

            string strOutputInfo = "";
            int nRet = GetAllDatabaseInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            this.AllDatabaseInfoXml = strOutputInfo;
            return 0;
        }

        // 在listview中列出所有数据库
        int ListAllDatabases(out string strError)
        {
            strError = "";

            this.listView_databases.Items.Clear();

            string strOutputInfo = "";
            int nRet = GetAllDatabaseInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            this.AllDatabaseInfoXml = strOutputInfo;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strOutputInfo);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");
                string strRole = DomUtil.GetAttr(node, "role");
                string strLibraryCode = DomUtil.GetAttr(node, "libraryCode");

                // 2008/7/2
                // 空的名字将被忽略
                if (String.IsNullOrEmpty(strName) == true)
                    continue;

                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;

                string strShuoming = "";
                if (string.IsNullOrEmpty(strRole) == false)
                    strShuoming += "角色: " + strRole;
                {
                    if (string.IsNullOrEmpty(strLibraryCode) == false)
                    {
                        if (string.IsNullOrEmpty(strShuoming) == false)
                            strShuoming += "; ";
                        strShuoming += "图书馆代码: " + strLibraryCode;
                    }
                }


                ListViewItem item = new ListViewItem(strName, 0);
                //item.SubItems.Add(strTypeName);
                //item.SubItems.Add(strShuoming);
                ListViewUtil.ChangeItemText(item, DATABASE_COLUMN_TYPENAME, strTypeName);
                ListViewUtil.ChangeItemText(item, DATABASE_COLUMN_SHUOMING, strShuoming);

                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_databases.Items.Add(item);
            }

            return 0;
        }

        const int DATABASE_COLUMN_NAME = 0;
        const int DATABASE_COLUMN_TYPENAME = 1;
        const int DATABASE_COLUMN_SHUOMING = 2;

        // 确定一个数据库是不是书目库类型?
        bool IsDatabaseBiblioType(string strDatabaseName)
        {
            for (int i = 0; i < this.listView_databases.Items.Count; i++)
            {
                ListViewItem item = this.listView_databases.Items[i];
                string strName = item.Text;
                if (strName == strDatabaseName)
                {
                    string strTypeName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_TYPENAME);
                    string strTypeString = GetTypeString(strTypeName);

                    if (strTypeString == "biblio")
                        return true;
                }
            }

            return false;
        }

        int GetAllDatabaseInfo(out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取全部数据库名 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "getinfo",
                    "",
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        private void listView_databases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedItems.Count > 0)
            {
                this.toolStripButton_modifyDatabase.Enabled = true;
                this.toolStripButton_deleteDatabase.Enabled = true;
                this.toolStripButton_initializeDatabase.Enabled = true;
                this.toolStripButton_refreshDatabaseDef.Enabled = true;
            }
            else
            {
                this.toolStripButton_modifyDatabase.Enabled = false;
                this.toolStripButton_deleteDatabase.Enabled = false;
                this.toolStripButton_initializeDatabase.Enabled = false;
                this.toolStripButton_refreshDatabaseDef.Enabled = false;
            }
        }

        // 创建书目库
        private void ToolStripMenuItem_createBiblioDatabase_Click(object sender, EventArgs e)
        {
            BiblioDatabaseDialog dlg = new BiblioDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "创建新书目库";
            dlg.ManagerForm = this;
            dlg.CreateMode = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 刷新库名列表
            string strError = "";
            int nRet = ListAllDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 选定刚创建的数据库
            SelectDatabaseLine(dlg.BiblioDatabaseName);

            // 重新获得各种库名、列表
            Program.MainForm.StartPrepareNames(false, false);
        }

        void SelectDatabaseLine(string strDatabaseName)
        {
            for (int i = 0; i < this.listView_databases.Items.Count; i++)
            {
                ListViewItem item = this.listView_databases.Items[i];

                if (item.Text == strDatabaseName)
                    item.Selected = true;
                else
                    item.Selected = false;
            }
        }

        // 
        /// <summary>
        /// 创建数据库。
        /// 请参考 dp2Library API ManageDatabase() 的详细说明，尤其是 strAction 参数为 "create" 和 "recreate" 时的功能
        /// </summary>
        /// <param name="strDatabaseInfo">数据库定义 XML</param>
        /// <param name="bRecreate">是否为重新创建</param>
        /// <param name="strOutputInfo">返回操作结果信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int CreateDatabase(
            string strDatabaseInfo,
            bool bRecreate,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建数据库 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    bRecreate == false ? "create" : "recreate",
                    "",
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 删除数据库。
        /// 请参考 dp2Library API ManageDatabase() 的详细说明，尤其是 strAction 参数为 "delete" 时的功能
        /// </summary>
        /// <param name="strDatabaseNames">要删除的数据库名列表</param>
        /// <param name="strOutputInfo">返回操作结果信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int DeleteDatabase(
            string strDatabaseNames,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除数据库 " + strDatabaseNames + "...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "delete",
                    strDatabaseNames,
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 2008/11/16
        //      strDatabaseInfo 要刷新的下属文件特性。<refreshStyle include="keys,browse" exclude="">(表示只刷新keys和browse两个重要配置文件)或者<refreshStyle include="*" exclude="template">(表示刷新全部文件，但是不要刷新template) 如果参数值为空，表示全部刷新
        // 
        /// <summary>
        /// 刷新数据库定义。
        /// 请参考 dp2Library API ManageDatabase() 的详细说明，尤其是 strAction 参数为 "refresh" 时的功能
        /// </summary>
        /// <param name="strDatabaseNames">要刷新定义的数据库名列表</param>
        /// <param name="strDatabaseInfo">数据库定义 XML</param>
        /// <param name="strOutputInfo">返回操作结果信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RefreshDatabasesDefs(
            string strDatabaseNames,
            string strDatabaseInfo,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在刷新数据库 " + strDatabaseNames + " 的定义...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "refresh",
                    strDatabaseNames,
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 初始化数据库。
        /// 请参考 dp2Library API ManageDatabase() 的详细说明，尤其是 strAction 参数为 "initialize" 时的功能
        /// </summary>
        /// <param name="strDatabaseNames">要初始化的数据库名列表</param>
        /// <param name="strOutputInfo">返回操作结果信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int InitializeDatabase(
            string strDatabaseNames,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化数据库 " + strDatabaseNames + "...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "initialize",
                    strDatabaseNames,
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 修改数据库定义。
        /// 请参考 dp2Library API ManageDatabase() 的详细说明，尤其是 strAction 参数为 "change" 时的功能
        /// </summary>
        /// <param name="strDatabaseNames">要修改定义的数据库名列表</param>
        /// <param name="strDatabaseInfo">数据库定义 XML</param>
        /// <param name="strOutputInfo">返回操作结果信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int ChangeDatabase(
            string strDatabaseNames,
            string strDatabaseInfo,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在修改数据库 " + strDatabaseNames + "...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "change",
                    strDatabaseNames,
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 删除数据库
        private void toolStripButton_deleteDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_databases.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要删除的数据库事项";
                goto ERROR1;
            }

            string strLibraryCodeList = this.GetLibraryCodeList();

            string strDbNameList = ListViewUtil.GetItemNameList(this.listView_databases.SelectedItems);
            /*
            foreach (ListViewItem item in this.listView_databases.SelectedItems)
            {
                if (string.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += item.Text;
            }
             * */

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除数据库 " + strDbNameList + "?\r\n\r\n警告：数据库一旦被删除后，其内的数据记录将全部丢失，并再也无法复原",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            // 为确认身份而登录
            // return:
            //      -1  出错
            //      0   放弃登录
            //      1   登录成功
            nRet = ConfirmLogin(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "删除数据库操作被放弃";
                goto ERROR1;
            }

            /*
            // 为更新AllDatabaseInfoXml
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "AllDatabaseInfoXml装入XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }
             * */

            EnableControls(false);

            try
            {

                for (int i = this.listView_databases.SelectedIndices.Count - 1;
                    i >= 0;
                    i--)
                {
                    int index = this.listView_databases.SelectedIndices[i];

                    string strDatabaseName = this.listView_databases.Items[index].Text;

                    string strOutputInfo = "";
                    nRet = DeleteDatabase(strDatabaseName,
                        out strOutputInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.listView_databases.Items.RemoveAt(index);

                    /*
                    // 删除DOM中定义
                    XmlNode nodeDatabase = dom.DocumentElement.SelectSingleNode("database[@name='" + strDatabaseName + "']");
                    if (nodeDatabase == null)
                    {
                        strError = "AllDatabaseInfoXml中居然没有找到名为 '"+strDatabaseName+"' 的数据库定义";
                        goto ERROR1;
                    }
                    dom.DocumentElement.RemoveChild(nodeDatabase);
                     * */

                }

                /*
                // 刷新定义
                this.AllDatabaseInfoXml = dom.OuterXml;
                 * */
                nRet = RefreshAllDatabaseXml(out strError);
                if (nRet == -1)
                    goto ERROR1;

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 如果馆代码列表发生了变化
                if (this.GetLibraryCodeList() != strLibraryCodeList)
                    UpdateLoanPolicyLibraryCode();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }
            finally
            {
                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 重新创建多个数据库
        // return:
        //      -1  出错
        //      0   放弃
        //      其他  重新创建的个数
        int recreateDatabases(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.listView_databases.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要重新创建的数据库事项";
                return -1;
            }

            string strLibraryCodeList = this.GetLibraryCodeList();

            string strDbNameList = ListViewUtil.GetItemNameList(this.listView_databases.SelectedItems);

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要重新创建数据库 " + strDbNameList + "?",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return 0;

            // 为确认身份而登录
            // return:
            //      -1  出错
            //      0   放弃登录
            //      1   登录成功
            nRet = ConfirmLogin(out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "重新创建数据库操作被放弃";
                return -1;
            }

            EnableControls(false);
            try
            {
                int nCount = 0;
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                foreach (ListViewItem item in this.listView_databases.SelectedItems)
                {
                    XmlNode nodeDatabase = dom.CreateElement("database");
                    dom.DocumentElement.AppendChild(nodeDatabase);

                    string strName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_NAME);
                    string strTypeName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_TYPENAME);

                    string strXml = (string)item.Tag;
                    XmlDocument node_dom = new XmlDocument();
                    node_dom.LoadXml(strXml);

                    string strType = node_dom.DocumentElement.GetAttribute("type");

                    DomUtil.SetAttr(nodeDatabase, "name", strName);
                    // type
                    DomUtil.SetAttr(nodeDatabase, "type", strType);
                    DomUtil.SetAttr(nodeDatabase, "info", "existing");

                    nCount++;
                }

                string strDatabaseInfo = dom.OuterXml;
                string strOutputInfo = "";
                // 创建数据库
                nRet = this.CreateDatabase(
                    strDatabaseInfo,
                    true,
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                    return -1;
#if NO
                nRet = RefreshAllDatabaseXml(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 如果馆代码列表发生了变化
                if (this.GetLibraryCodeList() != strLibraryCodeList)
                    UpdateLoanPolicyLibraryCode();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
                return nCount;
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 重新创建数据库
        private void menu_recreateDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_databases.SelectedItems.Count == 0)
            {
                strError = "尚未选定要重新创建的数据库";
                goto ERROR1;
            }

            if (this.listView_databases.SelectedItems.Count > 1)
            {
                // 重新创建多个数据库
                // return:
                //      -1  出错
                //      0   放弃
                //      其他  重新创建的个数
                if (recreateDatabases(out strError) == -1)
                    goto ERROR1;
                return;
            }

            // 重新创建单个数据库。可以在创建前观察到数据库的参数
            ListViewItem item = this.listView_databases.SelectedItems[0];
            string strTypeName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_TYPENAME);
            string strName = item.Text;

            string strType = GetTypeString(strTypeName);
            if (strType == null)
                strType = strTypeName;

            if (strType == "biblio")
            {
                BiblioDatabaseDialog dlg = new BiblioDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "重新创建书目库";
                dlg.ManagerForm = this;
                dlg.CreateMode = true;
                dlg.Recreate = true;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial((string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.BiblioDatabaseName);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }
            else if (strType == "reader")
            {
                ReaderDatabaseDialog dlg = new ReaderDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "重新创建读者库";
                dlg.ManagerForm = this;
                dlg.LibraryCodeList = this.GetLibraryCodeList();
                dlg.CreateMode = true;
                dlg.Recreate = true;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial((string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.ReaderDatabaseName);

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();
            }
            else if (strType == "publisher"
                || strType == "amerce"
                || strType == "arrived"
                || strType == "zhongcihao"
                || strType == "message")
            {
                SimpleDatabaseDialog dlg = new SimpleDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                /*
                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;
                 * */

                dlg.Text = "重新创建" + strTypeName + "库";
                dlg.ManagerForm = this;
                dlg.CreateMode = true;
                dlg.Recreate = true;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial(
                    strType,
                    (string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.DatabaseName);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改数据库特性
        private void toolStripButton_modifyDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_databases.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的数据库";
                goto ERROR1;
            }
            ListViewItem item = this.listView_databases.SelectedItems[0];
            string strTypeName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_TYPENAME);
            string strName = item.Text;

            string strType = GetTypeString(strTypeName);
            if (strType == null)
                strType = strTypeName;

            if (strType == "biblio")
            {
                BiblioDatabaseDialog dlg = new BiblioDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改书目库特性";
                dlg.ManagerForm = this;
                dlg.CreateMode = false;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial((string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.BiblioDatabaseName);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }
            else if (strType == "reader")
            {
                ReaderDatabaseDialog dlg = new ReaderDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改读者库特性";
                dlg.ManagerForm = this;
                dlg.LibraryCodeList = this.GetLibraryCodeList();
                dlg.CreateMode = false;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial((string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.ReaderDatabaseName);

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();
            }
            else if (strType == "message"
                || strType == "amerce"
                || strType == "invoice"
                || strType == "arrived"
                || strType == "zhongcihao"
                || strType == "publisher"
                || strType == "dictionary"
                || strType == "inventory")
            {
                SimpleDatabaseDialog dlg = new SimpleDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                /*
                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;
                 * */

                dlg.Text = "修改" + strTypeName + "库特性";
                dlg.ManagerForm = this;
                dlg.CreateMode = false;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                nRet = dlg.Initial(
                    strType,
                    (string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 刷新库名列表
                nRet = ListAllDatabases(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }

                // 选定刚修改的数据库
                SelectDatabaseLine(dlg.DatabaseName);

                RefreshOpacDatabaseList();
                RefreshOpacBrowseFormatTree();

                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
        static string GetTypeName(string strType)
        {
            if (strType == "publisher")
                return "出版者库";
            if (strType == "amerce")
                return "违约金库";
            if (strType == "arrived")
                return "预约到书库";
            if (strType == "biblio")
                return "书目库";
            if (strType == "entity")
                return "实体库";
            if (strType == "order")
                return "订购库";
            if (strType == "issue")
                return "期库";
            if (strType == "message")
                return "消息";

            return strType;
        }
         * */

        private void listView_databases_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_modifyDatabase_Click(sender, e);
        }

        private void listView_databases_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            /*
            ListViewItem item = null;
            
            if (this.listView_databases.SelectedItems.Count > 0)
                this.listView_databases.SelectedItems[0];
             * */

            string strName = "";
            string strType = "";
            if (this.listView_databases.SelectedItems.Count > 0)
            {
                strName = this.listView_databases.SelectedItems[0].Text;
                strType = ListViewUtil.GetItemText(this.listView_databases.SelectedItems[0], DATABASE_COLUMN_TYPENAME);
            }

            // 修改数据库
            {
                menuItem = new MenuItem("修改" + strType + "库 '" + strName + "'(&M)");
                menuItem.Click += new System.EventHandler(this.toolStripButton_modifyDatabase_Click);
                if (this.listView_databases.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                // 缺省命令
                menuItem.DefaultItem = true;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            string strText = "";

            // 重新创建数据库
            {
                if (this.listView_databases.SelectedItems.Count == 1)
                    strText = "重新创建" + strType + "库 '" + strName + "'(&M)";
                else
                    strText = "重新创建所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个数据库(&M)";

                menuItem = new MenuItem(strText);
                menuItem.Click += new System.EventHandler(this.menu_recreateDatabase_Click);
                if (this.listView_databases.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建书目库(&B)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createBiblioDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建读者库(&V)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createReaderDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建违约金库(&A)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createAmerceDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建发票库(&I)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createInvoiceDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("创建预约到书库(&R)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createArrivedDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建消息库(&M)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createMessageDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建拼音库(&P)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createPinyinDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建著者号码库(&G)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createGcatDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建词库(&G)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createWordDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建种次号库(&Z)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createZhongcihaoDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建出版者库(&P)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createPublisherDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建词典库(&D)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createDictionaryDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建盘点库(&I)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createInventoryDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            if (this.listView_databases.SelectedItems.Count == 1)
                strText = "删除" + strType + "库 '" + strName + "'(&D)";
            else
                strText = "删除所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个数据库(&D)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_deleteDatabase_Click);
            if (this.listView_databases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            if (this.listView_databases.SelectedItems.Count == 1)
                strText = "初始化" + strType + "库 '" + strName + "'(&I)";
            else
                strText = "初始化所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个数据库(&I)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_initializeDatabase_Click);
            if (this.listView_databases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            if (this.listView_databases.SelectedItems.Count == 1)
                strText = "刷新" + strType + "库 '" + strName + "' 的定义(&R)";
            else
                strText = "刷新所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个数据库的定义(&R)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_refreshDatabaseDef_Click);
            if (this.listView_databases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);




            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("观察所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个数据库的定义(&D)");
            menuItem.Click += new System.EventHandler(this.menu_viewDatabaseDefine_Click);
            if (this.listView_databases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_databases, new Point(e.X, e.Y));
        }

        // 观察数据库定义XML
        void menu_viewDatabaseDefine_Click(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要观察其定义的数据库事项");
                return;
            }

            string strXml = "";
            string strDbNameList = "";

            foreach (ListViewItem item in this.listView_databases.SelectedItems)
            {
                string strName = item.Text;
                strXml += "<!-- 数据库 " + strName + " 的定义 -->";
                strXml += (string)item.Tag;

                if (String.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += strName;
            }

            if (this.listView_databases.SelectedItems.Count > 1)
                strXml = "<root>" + strXml + "</root>";

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "数据库  " + strDbNameList + " 的定义";
            // dlg.MainForm = Program.MainForm;
            dlg.XmlString = strXml;
            Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_viewXml_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            return;
        }

        // 创建读者库
        private void ToolStripMenuItem_createReaderDatabase_Click(object sender, EventArgs e)
        {
            string strLibraryCodeList = this.GetLibraryCodeList();

            ReaderDatabaseDialog dlg = new ReaderDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "创建新读者库";
            dlg.ManagerForm = this;
            dlg.LibraryCodeList = this.GetLibraryCodeList();
            dlg.CreateMode = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 刷新库名列表
            string strError = "";
            int nRet = ListAllDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            // 选定刚创建的数据库
            SelectDatabaseLine(dlg.ReaderDatabaseName);

            // 如果馆代码列表发生了变化
            if (this.GetLibraryCodeList() != strLibraryCodeList)
                UpdateLoanPolicyLibraryCode();

            // 重新获得各种库名、列表
            Program.MainForm.StartPrepareNames(false, false);


        }

        // 创建违约金库
        private void ToolStripMenuItem_createAmerceDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("amerce", "", "");
        }

        // 创建发票库
        private void ToolStripMenuItem_createInvoiceDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("invoice", "", "");
        }

        // parameters:
        //      strDatabaseName 数据库名。如果不为空，则对话框会填写此名，但不让修改了
        // return:
        //      -1  errpr
        //      0   cancel
        //      1   created
        int CreateSimpleDatabase(string strType,
            string strDatabaseName,
            string strComment)
        {
            SimpleDatabaseDialog dlg = new SimpleDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            string strTypeName = GetTypeName(strType);
            if (strTypeName == null)
                strTypeName = strType;

            if (String.IsNullOrEmpty(strDatabaseName) == false)
            {
                dlg.DatabaseName = strDatabaseName;
                dlg.DatabaseNameReadOnly = true;
            }

            if (String.IsNullOrEmpty(strComment) == false)
                dlg.Comment = strComment;

            dlg.DatabaseType = strType;
            dlg.Text = "创建新" + strTypeName + "库";
            dlg.ManagerForm = this;
            dlg.CreateMode = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            // 刷新库名列表
            string strError = "";
            int nRet = ListAllDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return -1;
            }

            // 选定刚创建的数据库
            SelectDatabaseLine(dlg.DatabaseName);

            // 重新获得各种库名、列表
            Program.MainForm.StartPrepareNames(false, false);
            return 1;
        }

        // 创建预约到书库
        private void ToolStripMenuItem_createArrivedDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("arrived", "", "");
        }

        private void ToolStripMenuItem_createPublisherDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("publisher", "", "");
        }

        private void ToolStripMenuItem_createDictionaryDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("dictionary", "", "");
        }

        private void ToolStripMenuItem_createInventoryDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("inventory", "", "");
        }

        private void ToolStripMenuItem_createMessageDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("message", "", "");
        }

        private void ToolStripMenuItem_createPinyinDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("pinyin", "", "");
        }

        private void ToolStripMenuItem_createGcatDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("gcat", "", "");
        }

        private void ToolStripMenuItem_createWordDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("word", "", "");
        }

        private void ToolStripMenuItem_createZhongcihaoDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("zhongcihao", "", "");
        }

        // 刷新数据库名列表
        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_refresh.Enabled = false;
            try
            {
                RefreshDatabaseList();
            }
            finally
            {
                this.toolStripButton_refresh.Enabled = true;
            }
        }

        void RefreshDatabaseList()
        {
            string strError = "";
            int nRet = ListAllDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        // 为确认身份而登录
        // return:
        //      -1  出错
        //      0   放弃登录
        //      1   登录成功
        internal int ConfirmLogin(out string strError)
        {
            strError = "";

            ConfirmSupervisorDialog login_dlg = new ConfirmSupervisorDialog();
            MainForm.SetControlFont(login_dlg, Program.MainForm.DefaultFont);
            login_dlg.UserName = Program.MainForm.AppInfo.GetString(
                    "default_account",
                    "username",
                    "");
            login_dlg.ServerUrl = Program.MainForm.LibraryServerUrl;
            login_dlg.Comment = "重要操作前，需要验证您的身份";

            login_dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO_LOGIN:
            login_dlg.ShowDialog(this);

            if (login_dlg.DialogResult != DialogResult.OK)
                return 0;

            string strLocation = Program.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");
            string strParameters = "location=" + strLocation + ",type=worker";
            strParameters += ",client=dp2circulation|" + Program.ClientVersion;

            if (string.IsNullOrEmpty(login_dlg.TempCode) == false)
                strParameters += ",tempCode=" + login_dlg.TempCode;

            // 以手机短信验证方式登录
            if (string.IsNullOrEmpty(login_dlg.PhoneNumber) == false)
                strParameters += ",phoneNumber=" + login_dlg.PhoneNumber;

            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            long lRet = this.Channel.Login(login_dlg.UserName,
                login_dlg.Password,
                strParameters,
                out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                if (this.Channel.ErrorCode == ErrorCode.TempCodeMismatch
                    || this.Channel.ErrorCode == ErrorCode.RetryLogin)
                {
                    login_dlg.ActivateTempCode();
                    login_dlg.RetryLogin = true;  // 尝试再次登录
                    login_dlg.Comment = strError;
                    goto REDO_LOGIN;
                }
                if (this.Channel.ErrorCode == ErrorCode.NeedSmsLogin)
                {
                    login_dlg.ActivatePhoneNumber();
                    login_dlg.Comment = strError;
                    goto REDO_LOGIN;
                }

                // strError = "";
                return -1;
            }

            return 1;
        }

        // parameters:
        //      strDbPaths  分号分割的数据库全路径列表
        void ReplaceHostName(ref string strDbPaths)
        {
            Uri library_uri = new Uri(Program.MainForm.LibraryServerDir1);
            if (library_uri.IsLoopback == true)
                return; // 说明前端和图书馆服务器同在一台机器，就不用替换了

            string[] parts = strDbPaths.Split(new char[] { ';' });
            string strResult = "";
            for (int i = 0; i < parts.Length; i++)
            {
                string strDbPath = parts[i].Trim();
                if (String.IsNullOrEmpty(strDbPaths) == true)
                    continue;

                Uri uri = new Uri(strDbPath);
                if (uri.IsLoopback == true)
                {
                    string strQuery = "";  // 如果有，已经包含前面的问号
                    int nRet = strDbPath.LastIndexOf("?");
                    if (nRet != -1)
                        strQuery = strDbPath.Substring(nRet);

                    strDbPath = uri.Scheme + Uri.SchemeDelimiter + library_uri.Host
                        + (uri.IsDefaultPort == true ? "" : ":" + uri.Port.ToString())  // 2012/3/30 增加冒号
                        // + uri.PathAndQuery 本来可以这样用的，但是汉字的数据库名被escape了
                        + uri.LocalPath
                        + strQuery;
                }

                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ";";

                strResult += strDbPath;
            }

            strDbPaths = strResult;
        }

        // 刷新数据库定义
        private void toolStripButton_refreshDatabaseDef_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            if (this.listView_databases.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要刷新定义的数据库事项";
                goto ERROR1;
            }

            string strDbNameList = ListViewUtil.GetItemNameList(this.listView_databases.SelectedItems);
            /*
            foreach (ListViewItem item in this.listView_databases.SelectedItems)
            {
                if (string.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += item.Text;
            }
             * */

            // 观察里面是否有至少一个读者库。目前只对读者库有区分 keys 版本的必要
            bool bHasReaderDatabase = false;
            foreach (ListViewItem item in this.listView_databases.SelectedItems)
            {
                string strTypeName = ListViewUtil.GetItemText(item, DATABASE_COLUMN_TYPENAME);
                string strTypeString = GetTypeString(strTypeName);
                if (strTypeString == "reader")
                    bHasReaderDatabase = true;
            }
            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要刷新数据库 " + strDbNameList + " 的定义?\r\n\r\n说明：1) 数据库被刷新定义后，根据情况可能需要进行刷新数据库内记录的检索点的操作(否则现有记录的检索点可能会不全)。\r\n      2) 如果刷新的是(大)书目库的定义，则(大)书目库从属的实体库、订购库、期库也会一并被刷新定义。",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            RefreshStyleDialog style_dlg = new RefreshStyleDialog();
            MainForm.SetControlFont(style_dlg, this.Font, false);

            // 2.38
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.38") >= 0)
                style_dlg.AutoRebuildKeysVisible = true;

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.54") >= 0)
                style_dlg.RecoverStateVisible = bHasReaderDatabase;

            style_dlg.StartPosition = FormStartPosition.CenterScreen;
            style_dlg.ShowDialog(this);

            if (style_dlg.DialogResult == DialogResult.Cancel)
                return;

            // 为确认身份而登录
            // return:
            //      -1  出错
            //      0   放弃登录
            //      1   登录成功
            nRet = ConfirmLogin(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "刷新数据库定义的操作被放弃";
                goto ERROR1;
            }

            EnableControls(false);

            try
            {
                /*
                List<string> dbnames = new List<string>();

                for (int i = this.listView_databases.SelectedIndices.Count - 1;
                    i >= 0;
                    i--)
                {
                    int index = this.listView_databases.SelectedIndices[i];

                    string strDatabaseName = this.listView_databases.Items[index].Text;

                    dbnames.Add(strDatabaseName);
                }
                string strDbNameList = StringUtil.MakePathList(dbnames);
                 * */

                XmlDocument style_dom = new XmlDocument();
                style_dom.LoadXml("<refreshStyle />");
                DomUtil.SetAttr(style_dom.DocumentElement,
                    "include", style_dlg.IncludeFilenames);
                DomUtil.SetAttr(style_dom.DocumentElement,
                     "exclude", style_dlg.ExcludeFilenames);
                DomUtil.SetAttr(style_dom.DocumentElement,
                    "autoRebuildKeys", style_dlg.AutoRebuildKeys == true ? "true" : "false");

                // 2015/9/28
                DomUtil.SetAttr(style_dom.DocumentElement,
                    "recoverModeKeys", style_dlg.RecoverState == true ? "true" : "false");

                string strKeysChangedDbpaths = "";

                string strOutputInfo = "";

                //      strDatabaseInfo 要刷新的下属文件特性。<refreshStyle include="keys,browse" exclude="">(表示只刷新keys和browse两个重要配置文件)或者<refreshStyle include="*" exclude="template">(表示刷新全部文件，但是不要刷新template) 如果参数值为空，表示全部刷新
                nRet = RefreshDatabasesDefs(strDbNameList,
                    style_dom.DocumentElement.OuterXml,
                    out strOutputInfo,
                    out strError);
                if (String.IsNullOrEmpty(strOutputInfo) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strOutputInfo);
                    }
                    catch (Exception ex)
                    {
                        strError = "RefreshDatabasesDefs()所返回的strOutputInfo装入XMLDOM时出错: " + ex.Message;
                        goto ERROR1;
                    }
                    strKeysChangedDbpaths = DomUtil.GetAttr(dom.DocumentElement, "dbpaths");

                    ReplaceHostName(ref strKeysChangedDbpaths);
                }

                string strOtherComment = "";
                if (style_dlg.AutoRebuildKeys == true)
                {
                    strOtherComment = "\r\n---\r\ndp2library 已经自动启动批处理任务来重建这些数据库的检索点了，请使用 批处理任务窗 来观察任务的执行情况";
                }
                else
                {
                    strOtherComment = "\r\n---\r\n需要调用dp2Batch(即dp2批处理)来重建这些内核数据库检索点(这些内核数据库的路径已经放入Windows剪贴板中了)";
                }

                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strKeysChangedDbpaths) == false)
                        strError += "。不过下列内核数据库的检索点定义已经发生修改:\r\n---\r\n" + strKeysChangedDbpaths.Replace(";", "\r\n")
                            + strOtherComment;
                    if (style_dlg.AutoRebuildKeys == false)
                        Clipboard.SetDataObject(strKeysChangedDbpaths);
                    goto ERROR1;
                }

                // TODO: 提醒哪些数据库需要刷新检索点
                if (String.IsNullOrEmpty(strKeysChangedDbpaths) == false)
                {
                    /*
                    string strPathList = "";
                    string strNameList = "";

                    string[] dbnames = strKeysChangedDbpaths.Split(new char[] {';'});
                    for (int i = 0; i < dbnames.Length; i++)
                    {
                        string strPath = dbnames[i].Trim();
                        if (String.IsNullOrEmpty(strPath) == true)
                            continue;

                        string strDbName = "";
                        nRet = strPath.IndexOf("?");
                        if (nRet != -1)
                            strDbName = strPath.Substring(nRet+1).Trim();
                        else
                            strDbName = strPath;

                        if (String.IsNullOrEmpty(strPathList) == false)
                            strPathList += ";";

                        strPathList += strPath;

                        if (String.IsNullOrEmpty(strNameList) == false)
                            strNameList += ",";
                        strNameList += strDbName;
                    }
                     * */

                    strError = "下列内核数据库的检索点定义已经发生修改: \r\n---\r\n" + strKeysChangedDbpaths.Replace(";", "\r\n") + strOtherComment;
                    if (style_dlg.AutoRebuildKeys == false)
                        Clipboard.SetDataObject(strKeysChangedDbpaths);
                }
                else
                    strError = "";
            }
            finally
            {
                EnableControls(true);
            }

            if (String.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);

            // 2015/6/13
            // 重新获得各种库名、列表
            Program.MainForm.StartPrepareNames(false, false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 初始化
        private void toolStripButton_initializeDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            if (this.listView_databases.SelectedIndices.Count == 0)
            {
                strError = "尚未选择要初始化的数据库事项";
                goto ERROR1;
            }

            string strDbNameList = ListViewUtil.GetItemNameList(this.listView_databases.SelectedItems);
            /*
            foreach (ListViewItem item in this.listView_databases.SelectedItems)
            {
                if (string.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += item.Text;
            }
             * */

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要初始化数据库 " + strDbNameList + "?\r\n\r\n警告：\r\n1) 数据库一旦被初始化后，其内的数据记录将全部丢失，并再也无法复原。\r\n2) 如果初始化的是书目库，则书目库从属的实体库、订购库、期库也会一并被初始化。",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            // 为确认身份而登录
            // return:
            //      -1  出错
            //      0   放弃登录
            //      1   登录成功
            nRet = ConfirmLogin(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "初始化数据库操作被放弃";
                goto ERROR1;
            }

            EnableControls(false);

            try
            {

                for (int i = this.listView_databases.SelectedIndices.Count - 1;
                    i >= 0;
                    i--)
                {
                    int index = this.listView_databases.SelectedIndices[i];

                    string strDatabaseName = this.listView_databases.Items[index].Text;

                    string strOutputInfo = "";
                    nRet = InitializeDatabase(strDatabaseName,
                        out strOutputInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
            }
            finally
            {
                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        #region OPAC数据库配置管理


        // 在listview中列出所有参与OPAC的数据库
        int ListAllOpacDatabases(out string strError)
        {
            strError = "";

            this.listView_opacDatabases.Items.Clear();

            string strOutputInfo = "";
            int nRet = GetAllOpacDatabaseInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
        <virtualDatabase>
            <caption lang="zh-CN">中文书刊</caption>
            <caption lang="en">Chinese Books and Series</caption>
            <from style="title">
                <caption lang="zh-CN">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from style="author">
                <caption lang="zh-CN">著者</caption>
                <caption lang="en">Author</caption>
            </from>
            <database name="中文图书" />
            <database name="中文期刊" />
        </virtualDatabase>
        <database name="用户">
            <caption lang="zh-CN">用户</caption>
            <caption lang="en">account</caption>
            <from name="用户名">
                <caption lang="zh-CN">用户名</caption>
                <caption lang="en">username</caption>
            </from>
            <from name="__id" />
        </database>
        <database name="中文图书">
            <caption lang="zh-CN">中文图书</caption>
            <caption lang="en">Chinese book</caption>
            <from name="ISBN">
                <caption lang="zh-CN">ISBN</caption>
                <caption lang="en">ISBN</caption>
            </from>
            <from name="ISSN">
                <caption lang="zh-CN">ISSN</caption>
                <caption lang="en">ISSN</caption>
            </from>
            <from name="题名">
                <caption lang="zh-CN">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from name="题名拼音">
                <caption lang="zh-CN">题名拼音</caption>
                <caption lang="en">Title pinyin</caption>
            </from>
            <from name="主题词">
                <caption lang="zh-CN">主题词</caption>
                <caption lang="en">Thesaurus</caption>
            </from>
            <from name="关键词">
                <caption lang="zh-CN">关键词</caption>
                <caption lang="en">Keyword</caption>
            </from>
            <from name="分类号">
                <caption lang="zh-CN">分类号</caption>
                <caption lang="en">Class number</caption>
            </from>
            <from name="责任者">
                <caption lang="zh-CN">责任者</caption>
                <caption lang="en">Contributor</caption>
            </from>
            <from name="责任者拼音">
                <caption lang="zh-CN">责任者拼音</caption>
                <caption lang="en">Contributor pinyin</caption>
            </from>
            <from name="出版者">
                <caption lang="zh-CN">出版者</caption>
                <caption lang="en">Publisher</caption>
            </from>
            <from name="索取号">
                <caption lang="zh">索取号</caption>
                <caption lang="en">Call number</caption>
            </from>
            <from name="收藏单位">
                <caption lang="zh-CN">收藏单位</caption>
                <caption lang="en">Rights holder</caption>
            </from>
            <from name="索取类号">
                <caption lang="zh">索取类号</caption>
                <caption lang="en">Class of call number</caption>
            </from>
            <from name="批次号">
                <caption lang="zh">批次号</caption>
                <caption lang="en">Batch number</caption>
            </from>
            <from name="__id" />
        </database>
             * */

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database | virtualDatabase");
            foreach (XmlElement node in nodes)
            {
                string strName = node.GetAttribute("name");
                string strAlias = node.GetAttribute("alias");
                string strType = node.Name;

                // 对于<virtualDatabase>元素，要选出<caption>里面的中文名称
                if (node.Name == "virtualDatabase")
                    strName = DomUtil.GetCaption("zh", node);

                int nImageIndex = 0;
                if (strType == "virtualDatabase")
                    nImageIndex = 1;

                ListViewItem item = new ListViewItem(strName, nImageIndex);
                ListViewUtil.ChangeItemText(item, COLUMN_OPAC_TYPE, GetOpacDatabaseTypeDisplayString(strType));
                // item.SubItems.Add(GetOpacDatabaseTypeDisplayString(strType));
                ListViewUtil.ChangeItemText(item, COLUMN_OPAC_ALIAS, strAlias);
                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_opacDatabases.Items.Add(item);
            }

            return 0;
        }

        const int COLUMN_OPAC_NAME = 0;
        const int COLUMN_OPAC_TYPE = 1;
        const int COLUMN_OPAC_ALIAS = 2;
        const int COLUMN_OPAC_COMMENT = 3;

        // 获得OPAC数据库类型的显示字符串
        // 所谓显示字符串，就是“虚拟库” “普通库”
        static string GetOpacDatabaseTypeDisplayString(string strType)
        {
            if (strType == "virtualDatabase")
                return "虚拟库";

            if (strType == "database")
                return "普通库";

            return strType;
        }

        // 获得OPAC数据库类型的内部使用字符串
        // 所谓内部使用字符串，就是"virtualDatabase" "database"
        static string GetOpacDatabaseTypeString(string strDisplayString)
        {
            if (strDisplayString == "虚拟库")
                return "virtualDatabase";

            if (strDisplayString == "普通库")
                return "database";

            return strDisplayString;
        }

        // 获得全部OPAC数据库定义
        int GetAllOpacDatabaseInfo(out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取全部 OPAC 数据库定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "opac",
                    "databases",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 修改/设置全部OPAC数据库定义
        int SetAllOpacDatabaseInfo(string strDatabaseDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置全部OPAC数据库定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "opac",
                    "databases",
                    strDatabaseDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 获得普通数据库定义。
        /// 请参考 dp2Library API GetSystemParameter()的详细说明，尤其是 strCategory 参数为 "database_def" 时的功能
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <param name="strOutputInfo">返回数据库定义 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有得到所要求的信息; 1: 得到所要求的信息</returns>
        public int GetDatabaseInfo(
            string strDbName,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取数据库 " + strDbName + " 的定义...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "database_def",
                    strDbName,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 插入虚拟库定义
        private void toolStripMenuItem_insertOpacDatabase_virtual_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 已经存在的库名
            List<string> existing_opac_normal_dbnames = new List<string>();
            for (int i = 0; i < this.listView_opacDatabases.Items.Count; i++)
            {
                ListViewItem current_item = this.listView_opacDatabases.Items[i];
                string strCurrentName = current_item.Text;
                string strCurrentType = ListViewUtil.GetItemText(current_item, 1);

                if (strCurrentType == "普通库")
                    existing_opac_normal_dbnames.Add(strCurrentName);
            }

            OpacVirtualDatabaseDialog dlg = new OpacVirtualDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "新增虚拟库定义";
            dlg.ExistingOpacNormalDbNames = existing_opac_normal_dbnames;
            /*
            dlg.ManagerForm = this;
            dlg.CreateMode = true;
             * */
            int nRet = dlg.Initial(this,
                true,
                "",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // dlg.StartPosition = FormStartPosition.CenterScreen;
            Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacVirtualDatabaseDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(dlg.Xml);
            }
            catch (Exception ex)
            {
                strError = "从对话框中获得的XML装入DOM时出错: " + ex.Message;
                goto ERROR1;
            }

            // 从<virtualDatabase>元素下的若干<caption>中，选出符合当前工作语言的一个名字字符串
            // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
            string strName = DomUtil.GetCaption("zh",
                dom.DocumentElement);
            string strType = dom.DocumentElement.Name;

            ListViewItem item = new ListViewItem(strName, 1);
            item.SubItems.Add(GetOpacDatabaseTypeDisplayString(strType));
            item.Tag = dom.DocumentElement.OuterXml;   // 记载XML定义片断

            this.listView_opacDatabases.Items.Add(item);

            // 需要立即向服务器提交修改
            nRet = SubmitOpacDatabaseDef(out strError);
            if (nRet == -1)
            {
                item.ImageIndex = 2;    // 表示未能提交的新增请求
                goto ERROR1;
            }

            // 选定刚刚插入的虚拟库
            item.Selected = true;
            this.listView_opacDatabases.FocusedItem = item;

            // 观察这个刚插入的虚拟库的成员库，如果还没有具备OPAC显示格式定义，则提醒自动加入
            List<string> newly_biblio_dbnames = new List<string>();
            List<string> member_dbnames = dlg.MemberDatabaseNames;
            for (int i = 0; i < member_dbnames.Count; i++)
            {
                string strMemberDbName = member_dbnames[i];

                if (IsDatabaseBiblioType(strMemberDbName) == false)
                    continue;

                if (HasBrowseFormatDatabaseExist(strMemberDbName) == true)
                    continue;

                newly_biblio_dbnames.Add(strMemberDbName);
            }

            if (newly_biblio_dbnames.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "刚新增的虚拟库 " + strName + " 其成员库中，库 " + StringUtil.MakePathList(newly_biblio_dbnames) + " 还没有OPAC记录显示格式定义。\r\n\r\n要自动给它(们)创建常规的OPAC记录显示格式定义么? ",
    "ManagerForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    for (int i = 0; i < newly_biblio_dbnames.Count; i++)
                    {

                        // 为书目库插入OPAC显示格式节点(后插)
                        nRet = NewBiblioOpacBrowseFormat(newly_biblio_dbnames[i],
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检查新增的虚拟库名是否和当前已经存在的虚拟库名重复
        // return:
        //      -1  检查的过程发生错误
        //      0   没有重复
        //      1   有重复
        internal int DetectVirtualDatabaseNameDup(string strCaptionsXml,
            out string strError)
        {
            strError = "";

            XmlDocument domCaptions = new XmlDocument();
            domCaptions.LoadXml("<root />");
            domCaptions.DocumentElement.InnerXml = strCaptionsXml;

            XmlNodeList nodes = domCaptions.DocumentElement.SelectNodes("caption");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strOneLang = DomUtil.GetAttr(node, "lang");
                string strOneName = node.InnerText;

                for (int j = 0; j < this.listView_opacDatabases.Items.Count; j++)
                {
                    ListViewItem item = this.listView_opacDatabases.Items[j];

                    string strName = ListViewUtil.GetItemText(item, 0);

                    string strXml = (string)item.Tag;
                    string strType = GetOpacDatabaseTypeString(ListViewUtil.GetItemText(item, 1));
                    if (strType == "virtualDatabase")
                    {
                        XmlDocument temp = new XmlDocument();
                        try
                        {
                            temp.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "虚拟库 '" + strName + "' 的XML定义装入DOM过程中出错: " + ex.Message;
                            return -1;
                        }
                        XmlNodeList exist_nodes = temp.DocumentElement.SelectNodes("caption");
                        for (int k = 0; k < exist_nodes.Count; k++)
                        {
                            string strExistLang = DomUtil.GetAttr(exist_nodes[k], "lang");
                            string strExistName = exist_nodes[k].InnerText;

                            if (strExistName == strOneName)
                            {
                                strError = "语言代码 '" + strOneLang + "' 下的虚拟库名 '" + strOneName + "' 和当前已经存在的列表中第 " + (j + 1).ToString() + " 行的语言 '" + strExistLang + "' 下的虚拟库名 '" + strExistName + "' 发生了重复";
                                return 1;
                            }
                        }
                    }
                    else if (strType == "database")
                    {
                        if (strName == strOneName)
                        {
                            strError = "语言代码 '" + strOneLang + "' 下的虚拟库名 '" + strOneName + "' 和当前已经存在的普通库名(列表中第 " + (j + 1).ToString() + " 行)发生了重复";
                            return 1;
                        }
                    }
                }
            }

            return 0;
        }

        private void listView_opacDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_opacDatabases.SelectedItems.Count > 0)
            {
                this.toolStripButton_modifyOpacDatabase.Enabled = true;
                this.toolStripButton_removeOpacDatabase.Enabled = true;
            }
            else
            {
                this.toolStripButton_modifyOpacDatabase.Enabled = false;
                this.toolStripButton_removeOpacDatabase.Enabled = false;
            }
        }

        private void listView_opacDatabases_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strName = "";
            string strType = "";
            if (this.listView_opacDatabases.SelectedItems.Count > 0)
            {
                strName = this.listView_opacDatabases.SelectedItems[0].Text;
                strType = ListViewUtil.GetItemText(this.listView_opacDatabases.SelectedItems[0], 1);
            }

            // 修改OPAC数据库
            {
                menuItem = new MenuItem("修改" + strType + " " + strName + "(&M)");
                menuItem.Click += new System.EventHandler(this.toolStripButton_modifyOpacDatabase_Click);
                if (this.listView_opacDatabases.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                // 缺省命令
                menuItem.DefaultItem = true;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("插入普通库(&N)");
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_insertOpacDatabase_normal_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("插入虚拟库(&V)");
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_insertOpacDatabase_virtual_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            string strText = "";
            if (this.listView_opacDatabases.SelectedItems.Count == 1)
                strText = "移除" + strType + " " + strName + "(&D)";
            else
                strText = "移除所选 " + this.listView_opacDatabases.SelectedItems.Count.ToString() + " 个OPAC数据库(&D)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_removeOpacDatabase_Click);
            if (this.listView_opacDatabases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("观察所选 " + this.listView_opacDatabases.SelectedItems.Count.ToString() + " 个OPAC数据库的定义(&D)");
            menuItem.Click += new System.EventHandler(this.menu_viewOpacDatabaseDefine_Click);
            if (this.listView_opacDatabases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_opacDatabase_up_Click);
            if (this.listView_opacDatabases.SelectedItems.Count == 0
                || this.listView_opacDatabases.Items.IndexOf(this.listView_opacDatabases.SelectedItems[0]) == 0)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.menu_opacDatabase_down_Click);
            if (this.listView_opacDatabases.SelectedItems.Count == 0
                || this.listView_opacDatabases.Items.IndexOf(this.listView_opacDatabases.SelectedItems[0]) >= this.listView_opacDatabases.Items.Count - 1)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_refreshOpacDatabaseList_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_opacDatabases, new Point(e.X, e.Y));
        }

        void menu_opacDatabase_up_Click(object sender, EventArgs e)
        {
            MoveOpacDatabaseItemUpDown(true);
        }

        void menu_opacDatabase_down_Click(object sender, EventArgs e)
        {
            MoveOpacDatabaseItemUpDown(false);
        }

        void MoveOpacDatabaseItemUpDown(bool bUp)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_opacDatabases.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要进行上下移动的OPAC数据库事项");
                return;
            }

            ListViewItem item = this.listView_opacDatabases.SelectedItems[0];
            int index = this.listView_opacDatabases.Items.IndexOf(item);

            Debug.Assert(index >= 0 && index <= this.listView_opacDatabases.Items.Count - 1, "");

            bool bChanged = false;

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "到头";
                    goto ERROR1;
                }

                this.listView_opacDatabases.Items.RemoveAt(index);
                index--;
                this.listView_opacDatabases.Items.Insert(index, item);
                this.listView_opacDatabases.FocusedItem = item;

                bChanged = true;
            }

            if (bUp == false)
            {
                if (index >= this.listView_opacDatabases.Items.Count - 1)
                {
                    strError = "到尾";
                    goto ERROR1;
                }
                this.listView_opacDatabases.Items.RemoveAt(index);
                index++;
                this.listView_opacDatabases.Items.Insert(index, item);
                this.listView_opacDatabases.FocusedItem = item;

                bChanged = true;
            }

            // TODO: 是否可以延迟提交?
            if (bChanged == true)
            {
                // 需要立即向服务器提交修改
                nRet = SubmitOpacDatabaseDef(out strError);
                if (nRet == -1)
                {
                    // TODO: 如何表示未能提交的上下位置移动请求?
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 观察OPAC数据库定义XML
        void menu_viewOpacDatabaseDefine_Click(object sender, EventArgs e)
        {
            if (this.listView_opacDatabases.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要观察其定义的OPAC数据库事项");
                return;
            }

            string strXml = "";
            string strDbNameList = "";

            foreach (ListViewItem item in this.listView_opacDatabases.SelectedItems)
            {
                string strName = item.Text;
                strXml += "<!-- OPAC数据库 " + strName + " 的定义 -->";
                strXml += (string)item.Tag;

                if (String.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += strName;
            }

            if (this.listView_opacDatabases.SelectedItems.Count > 1)
                strXml = "<virtualDatabases>" + strXml + "</virtualDatabases>";

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "OPAC数据库  " + strDbNameList + " 的定义";
            // dlg.MainForm = Program.MainForm;
            dlg.XmlString = strXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_viewXml_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
            return;
        }

        // 修改OPAC数据库定义
        private void toolStripButton_modifyOpacDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_opacDatabases.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的OPAC数据库事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_opacDatabases.SelectedItems[0];

            string strType = GetOpacDatabaseTypeString(ListViewUtil.GetItemText(item, 1));

            if (strType == "virtualDatabase")
            {
                // 已经存在的库名
                List<string> existing_opac_normal_dbnames = new List<string>();
                for (int i = 0; i < this.listView_opacDatabases.Items.Count; i++)
                {
                    ListViewItem current_item = this.listView_opacDatabases.Items[i];
                    string strCurrentName = current_item.Text;
                    string strCurrentType = ListViewUtil.GetItemText(current_item, 1);

                    if (strCurrentType == "普通库")
                        existing_opac_normal_dbnames.Add(strCurrentName);
                }

                OpacVirtualDatabaseDialog dlg = new OpacVirtualDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改虚拟库定义";
                dlg.ExistingOpacNormalDbNames = existing_opac_normal_dbnames;
                /*
                dlg.ManagerForm = this;
                dlg.CreateMode = false;
                 * */

                nRet = dlg.Initial(this,
                    false,
                    (string)item.Tag,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // dlg.StartPosition = FormStartPosition.CenterScreen;
                Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacVirtualDatabaseDialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);


                if (dlg.DialogResult != DialogResult.OK)
                    return;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(dlg.Xml);
                }
                catch (Exception ex)
                {
                    strError = "从对话框中获得的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                // 从<virtualDatabase>元素下的若干<caption>中，选出符合当前工作语言的一个名字字符串
                // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
                string strName = DomUtil.GetCaption("zh",
                    dom.DocumentElement);

                strType = dom.DocumentElement.Name;

                item.Text = strName;
                ListViewUtil.ChangeItemText(item, 1, GetOpacDatabaseTypeDisplayString(strType));
                item.Tag = dlg.Xml;   // 记载XML定义片断

                // 需要立即向服务器提交修改
                nRet = SubmitOpacDatabaseDef(out strError);
                if (nRet == -1)
                {
                    item.ImageIndex = 2;    // 表示未能提交的修改请求
                    goto ERROR1;
                }

                // 观察这个刚修改的虚拟库的成员库，如果还没有具备OPAC显示格式定义，则提醒自动加入
                List<string> newly_biblio_dbnames = new List<string>();
                List<string> member_dbnames = dlg.MemberDatabaseNames;
                for (int i = 0; i < member_dbnames.Count; i++)
                {
                    string strMemberDbName = member_dbnames[i];

                    if (IsDatabaseBiblioType(strMemberDbName) == false)
                        continue;

                    if (HasBrowseFormatDatabaseExist(strMemberDbName) == true)
                        continue;

                    newly_biblio_dbnames.Add(strMemberDbName);
                }

                if (newly_biblio_dbnames.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
        "刚被修改的虚拟库 " + strName + " 其成员库中，库 " + StringUtil.MakePathList(newly_biblio_dbnames) + " 还没有OPAC记录显示格式定义。\r\n\r\n要自动给它(们)创建常规的OPAC记录显示格式定义么? ",
        "ManagerForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        for (int i = 0; i < newly_biblio_dbnames.Count; i++)
                        {

                            // 为书目库插入OPAC显示格式节点(后插)
                            nRet = NewBiblioOpacBrowseFormat(newly_biblio_dbnames[i],
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                    }
                }
            }
            else if (strType == "database")
            {
                OpacNormalDatabaseDialog dlg = new OpacNormalDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                string strXml = (string)item.Tag;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                dlg.Text = "普通库名";
                dlg.ManagerForm = this;
                dlg.DatabaseName = DomUtil.GetAttr(dom.DocumentElement, "name");
                dlg.DatabaseAlias = dom.DocumentElement.GetAttribute("alias");

                Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacNormalDatabaseDialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.DatabaseName);

                ListViewUtil.ChangeItemText(item, COLUMN_OPAC_NAME, dlg.DatabaseName);
                // 注: type 不会修改
                if (string.IsNullOrEmpty(dlg.DatabaseAlias))
                    dom.DocumentElement.RemoveAttribute("alias");
                else
                    dom.DocumentElement.SetAttribute("alias", dlg.DatabaseAlias);
                ListViewUtil.ChangeItemText(item, COLUMN_OPAC_ALIAS, dlg.DatabaseAlias);
                item.Tag = dom.DocumentElement.OuterXml;   // 记载XML定义片断

                // 需要立即向服务器提交修改
                nRet = SubmitOpacDatabaseDef(out strError);
                if (nRet == -1)
                {
                    item.ImageIndex = 2;    // 表示未能提交的修改请求
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改OPAC数据库定义
        private void listView_opacDatabases_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_modifyOpacDatabase_Click(sender, e);
        }

        // 插入OPAC普通库
        private void toolStripMenuItem_insertOpacDatabase_normal_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 已经存在的库名
            List<string> existing_dbnames = new List<string>();
            foreach (ListViewItem item in this.listView_opacDatabases.Items)
            {
                existing_dbnames.Add(item.Text);
            }

            OpacNormalDatabaseDialog dlg = new OpacNormalDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "新增普通库定义";
            dlg.ManagerForm = this;
            dlg.ExcludingDbNames = existing_dbnames;

            Program.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacNormalDatabaseDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            List<string> dbnames = StringUtil.SplitList(dlg.DatabaseName);
            List<string> aliases = StringUtil.SplitList(dlg.DatabaseAlias);

            {
                int i = 0;
                foreach (string strName in dbnames)
                {
                    // 别名
                    string alias = "";
                    if (i < aliases.Count)
                        alias = aliases[i];

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<database name='' />");
                    // 从<virtualDatabase>元素下的若干<caption>中，选出符合当前工作语言的一个名字字符串
                    // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
                    //// string strName = dlg.DatabaseName;
                    string strType = "database";

                    DomUtil.SetAttr(dom.DocumentElement, "name", strName);
                    ListViewItem item = new ListViewItem(strName, 0);
                    ListViewUtil.ChangeItemText(item, COLUMN_OPAC_TYPE, GetOpacDatabaseTypeDisplayString(strType));

                    if (string.IsNullOrEmpty(alias) == false)
                        dom.DocumentElement.SetAttribute("alias", alias);
                    ListViewUtil.ChangeItemText(item, COLUMN_OPAC_ALIAS, alias);

                    item.Tag = dom.DocumentElement.OuterXml;   // 记载XML定义片断

                    this.listView_opacDatabases.Items.Add(item);

                    items.Add(item);

                    i++;
                }
            }

            // 需要立即向服务器提交修改
            nRet = SubmitOpacDatabaseDef(out strError);
            if (nRet == -1)
            {
                foreach (ListViewItem item in items)
                {
                    item.ImageIndex = 2;    // 表示未能提交的新增请求
                }
                goto ERROR1;
            }

            // 选定刚刚插入的普通库
            foreach (ListViewItem item in items)
            {
                item.Selected = true;
                this.listView_opacDatabases.FocusedItem = item;
            }

            DialogResult result = System.Windows.Forms.DialogResult.Cancel;
            foreach (string strName in dbnames)
            {
                // 如果是书目库，看看这个数据库的显示格式定义是否已经存在？
                // 如果不存在，提示插入建议
                if (IsDatabaseBiblioType(strName) == true
                    && HasBrowseFormatDatabaseExist(strName) == false)
                {
                    if (result == System.Windows.Forms.DialogResult.Cancel)
                    {
                        result = MessageBox.Show(this,
                            "刚新增的书目库 " + strName + " 还没有OPAC记录显示格式定义。\r\n\r\n要自动给它创建常规的OPAC记录显示格式定义么? ",
                            "ManagerForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                    }
                    if (result == DialogResult.Yes)
                    {
                        // 为书目库插入OPAC显示格式节点(后插)
                        nRet = NewBiblioOpacBrowseFormat(strName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 提交OPAC数据库定义修改
        int SubmitOpacDatabaseDef(out string strError)
        {
            strError = "";
            string strDatabaseDef = "";
            int nRet = BuildOpacDatabaseDef(out strDatabaseDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SetAllOpacDatabaseInfo(strDatabaseDef,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 构造OPAC数据库定义的XML片段
        // 注意是下级片断定义，没有<virtualDatabases>元素作为根。
        int BuildOpacDatabaseDef(out string strDatabaseDef,
            out string strError)
        {
            strError = "";
            strDatabaseDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<virtualDatabases />");

            for (int i = 0; i < this.listView_opacDatabases.Items.Count; i++)
            {
                ListViewItem item = this.listView_opacDatabases.Items[i];
                string strName = item.Text;
                string strType = ListViewUtil.GetItemText(item, 1);

                string strXmlFragment = (string)item.Tag;

                XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = strXmlFragment;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                dom.DocumentElement.AppendChild(fragment);
            }

            strDatabaseDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 移除一个OPAC数据库
        private void toolStripButton_removeOpacDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_opacDatabases.SelectedItems.Count == 0)
            {
                strError = "尚未选定要移除的OPAC数据库事项";
                goto ERROR1;
            }

            string strDbNameList = ListViewUtil.GetItemNameList(this.listView_opacDatabases.SelectedItems);
            /*
            foreach (ListViewItem item in this.listView_opacDatabases.SelectedItems)
            {
                if (string.IsNullOrEmpty(strDbNameList) == false)
                    strDbNameList += ",";
                strDbNameList += item.Text;
            }
             * */

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要移除OPAC数据库 " + strDbNameList + "?\r\n\r\n注：移除数据库不是删除数据库，只是使这些数据库不能被OPAC检索而已",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            for (int i = this.listView_opacDatabases.SelectedIndices.Count - 1;
                i >= 0;
                i--)
            {
                int index = this.listView_opacDatabases.SelectedIndices[i];
                string strDatabaseName = this.listView_opacDatabases.Items[index].Text;
                this.listView_opacDatabases.Items.RemoveAt(index);
            }


            // 需要立即向服务器提交修改
            nRet = SubmitOpacDatabaseDef(out strError);
            if (nRet == -1)
            {
                // TODO: 是否需要把刚才删除的事项插入回去？
                // item.ImageIndex = 2;    // 表示未能提交的修改请求
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void toolStripButton_refreshOpacDatabaseList_Click(object sender, EventArgs e)
        {
            this.toolStripButton_refreshOpacDatabaseList.Enabled = false;
            try
            {
                RefreshOpacDatabaseList();
            }
            finally
            {
                this.toolStripButton_refreshOpacDatabaseList.Enabled = true;
            }
        }

        void RefreshOpacDatabaseList()
        {
            string strError = "";
            int nRet = ListAllOpacDatabases(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        #endregion // of OPAC数据库配置管理

        // 清除所有数据库内的记录。也就是初始化所有数据库的意思。
        private void toolStripButton_initialAllDatabases_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要初始化***所有***数据库 ?\r\n\r\n警告：\r\n1) 数据库一旦被初始化后，其内的数据记录将全部丢失，并再也无法复原。\r\n2) 如果初始化的是书目库，则书目库从属的实体库、订购库、期库也会一并被初始化。",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            // 为确认身份而登录
            // return:
            //      -1  出错
            //      0   放弃登录
            //      1   登录成功
            nRet = ConfirmLogin(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "初始化所有数据库操作被放弃";
                goto ERROR1;
            }

            EnableControls(false);

            try
            {
                nRet = ClearAllDbs(out strError);
                if (nRet == -1)
                    goto ERROR1;
                else
                    MessageBox.Show(this, "OK");
            }
            finally
            {
                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region OPAC记录显示格式

        // 在treeview中列出所有OPAC数据显示格式
        int ListAllOpacBrowseFormats(out string strError)
        {
            strError = "";

            this.treeView_opacBrowseFormats.Nodes.Clear();

            string strOutputInfo = "";
            int nRet = GetAllOpacBrowseFormats(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
    <browseformats>
        <database name="中文图书">
            <format name="详细" type="biblio" />
        </database>
    	<database name="特色资源">
	    	<format name="详细" scriptfile="./cfgs/opac_detail.fltx" />
	    </database>
        <database name="读者">
            <format name="详细" scriptfile="./cfgs/opac_detail.cs" />
        </database>
    </browseformats>
             * */


            XmlNodeList database_nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < database_nodes.Count; i++)
            {
                XmlNode node = database_nodes[i];

                string strDatabaseName = DomUtil.GetAttr(node, "name");

                TreeNode database_treenode = new TreeNode(strDatabaseName, 0, 0);

                this.treeView_opacBrowseFormats.Nodes.Add(database_treenode);

                // 加入格式节点
                XmlNodeList format_nodes = node.SelectNodes("format");
                for (int j = 0; j < format_nodes.Count; j++)
                {
                    XmlNode format_node = format_nodes[j];

                    /*
                    string strFormatName = DomUtil.GetAttr(format_node, "name");
                    string strType = DomUtil.GetAttr(format_node, "type");
                    string strScriptFile = DomUtil.GetAttr(format_node, "scriptfile");
                    string strStyle = DomUtil.GetAttr(format_node, "style");

                    string strDisplayText = strFormatName;
                    
                    if (String.IsNullOrEmpty(strType) == false)
                        strDisplayText += " type=" + strType;

                    if (String.IsNullOrEmpty(strScriptFile) == false)
                        strDisplayText += " scriptfile=" + strScriptFile;
                     * */

                    TreeNode format_treenode = new TreeNode(GetFormatDisplayString(format_node), 1, 1);
                    format_treenode.Tag = format_node.OuterXml;

                    database_treenode.Nodes.Add(format_treenode);
                }
            }

            this.treeView_opacBrowseFormats.ExpandAll();

            return 0;
        }

        static string GetFormatDisplayString(XmlNode format_node)
        {
            string strFormatName = DomUtil.GetAttr(format_node, "name");
            string strType = DomUtil.GetAttr(format_node, "type");
            string strScriptFile = DomUtil.GetAttr(format_node, "scriptfile");
            string strStyle = DomUtil.GetAttr(format_node, "style");

            string strDisplayText = strFormatName;

            if (String.IsNullOrEmpty(strType) == false)
                strDisplayText += " type=" + strType;

            if (String.IsNullOrEmpty(strScriptFile) == false)
                strDisplayText += " scriptfile=" + strScriptFile;

            if (String.IsNullOrEmpty(strStyle) == false)
                strDisplayText += " style=" + strStyle;

            return strDisplayText;
        }

        // 获得全部OPAC浏览格式定义
        int GetAllOpacBrowseFormats(out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取全部OPAC记录显示格式定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "opac",
                    "browseformats",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 修改/设置全部OPAC记录显示格式定义
        int SetAllOpacBrowseFormatsDef(string strDatabaseDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置全部OPAC记录显示格式定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "opac",
                    "browseformats",
                    strDatabaseDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 插入库名节点(后插)
        private void toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_opacBrowseFormats.SelectedNode;

            // 如果不是根级的节点，则向上找到根级别
            if (current_treenode != null && current_treenode.Parent != null)
            {
                current_treenode = current_treenode.Parent;
            }

            // 插入点
            int index = this.treeView_opacBrowseFormats.Nodes.IndexOf(current_treenode);
            if (index == -1)
                index = this.treeView_opacBrowseFormats.Nodes.Count;
            else
                index++;


            // 当前已经存在的数据库名都是需要排除的
            List<string> existing_dbnames = new List<string>();
            for (int i = 0; i < this.treeView_opacBrowseFormats.Nodes.Count; i++)
            {
                string strDatabaseName = this.treeView_opacBrowseFormats.Nodes[i].Text;
                existing_dbnames.Add(strDatabaseName);
            }

            // 询问数据库名
            OpacNormalDatabaseDialog dlg = new OpacNormalDatabaseDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "请指定数据库名";
            dlg.ManagerForm = this;
            dlg.DatabaseName = "";
            dlg.ExcludingDbNames = existing_dbnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            TreeNode new_treenode = new TreeNode(dlg.DatabaseName, 0, 0);

            this.treeView_opacBrowseFormats.Nodes.Insert(index, new_treenode);

            this.treeView_opacBrowseFormats.SelectedNode = new_treenode;

            // 需要立即向服务器提交修改
            nRet = SubmitOpacBrowseFormatDef(out strError);
            if (nRet == -1)
            {
                new_treenode.ImageIndex = 2;    // 表示未能提交的新插入节点请求
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void treeView_opacBrowseFormats_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode current_treenode = this.treeView_opacBrowseFormats.SelectedNode;

            // 插入格式节点的菜单项，只有在当前节点为数据库类型或者格式类型时才能enabled

            if (current_treenode == null)
            {
                this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Enabled = false;
                this.toolStripButton_opacBrowseFormats_modify.Enabled = false;
                this.toolStripButton_opacBrowseFormats_remove.Enabled = false;
            }
            else
            {
                this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Enabled = true;
                this.toolStripButton_opacBrowseFormats_modify.Enabled = true;
                this.toolStripButton_opacBrowseFormats_remove.Enabled = true;
            }
        }

        // 插入显示格式节点(后插)
        private void toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_opacBrowseFormats.SelectedNode;

            if (current_treenode == null)
            {
                MessageBox.Show(this, "尚未选定库名或格式名节点，因此无法插入新的显示格式节点");
                return;
            }

            int index = -1;

            Debug.Assert(current_treenode != null, "");

            // 如果是第一级的节点，则理解为插入到它的儿子的尾部
            if (current_treenode.Parent == null)
            {
                Debug.Assert(current_treenode != null, "");

                index = current_treenode.Nodes.Count;
            }
            else
            {
                index = current_treenode.Parent.Nodes.IndexOf(current_treenode);

                Debug.Assert(index != -1, "");

                index++;

                current_treenode = current_treenode.Parent;
            }

            // 至此，current_treenode为数据库类型的节点了


            // 新的显示格式
            OpacBrowseFormatDialog dlg = new OpacBrowseFormatDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            // TODO: 如果数据库为书目库，则type应当预设为"biblio"
            dlg.Text = "请指定显示格式的属性";
            // dlg.FormatName = "";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<format />");

            /*
            string strDisplayText = dlg.FormatName;
            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.FormatName);

            if (String.IsNullOrEmpty(dlg.FormatType) == false)
            {
                strDisplayText += " type=" + dlg.FormatType;
                DomUtil.SetAttr(dom.DocumentElement, "type", dlg.FormatType);
            }

            if (String.IsNullOrEmpty(dlg.ScriptFile) == false)
            {
                strDisplayText += " scriptfile=" + dlg.ScriptFile;
                DomUtil.SetAttr(dom.DocumentElement, "scriptfile", dlg.ScriptFile);
            }

            if (String.IsNullOrEmpty(dlg.FormatStyle) == false)
            {
                strDisplayText += " style=" + dlg.FormatStyle;
                DomUtil.SetAttr(dom.DocumentElement, "style", dlg.FormatStyle);
            }
             * */

            // 2009/6/27
            if (String.IsNullOrEmpty(dlg.CaptionsXml) == false)
                dom.DocumentElement.InnerXml = dlg.CaptionsXml;

            TreeNode new_treenode = new TreeNode(GetFormatDisplayString(dom.DocumentElement), 1, 1);
            new_treenode.Tag = dom.DocumentElement.OuterXml;

            current_treenode.Nodes.Insert(index, new_treenode);

            this.treeView_opacBrowseFormats.SelectedNode = new_treenode;

            // 需要立即向服务器提交修改
            nRet = SubmitOpacBrowseFormatDef(out strError);
            if (nRet == -1)
            {
                new_treenode.ImageIndex = 2;    // 表示未能提交的新插入节点请求
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改一个节点的定义
        private void toolStripButton_opacBrowseFormats_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_opacBrowseFormats.SelectedNode;

            if (current_treenode == null)
            {
                MessageBox.Show(this, "尚未选定要修改的库名或格式节点");
                return;
            }

            if (current_treenode.Parent == null)
            {
                // 库名节点


                // 当前已经存在的数据库名都是需要排除的
                List<string> existing_dbnames = new List<string>();
                for (int i = 0; i < this.treeView_opacBrowseFormats.Nodes.Count; i++)
                {
                    string strDatabaseName = this.treeView_opacBrowseFormats.Nodes[i].Text;
                    existing_dbnames.Add(strDatabaseName);
                }

                OpacNormalDatabaseDialog dlg = new OpacNormalDatabaseDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改数据库名";
                dlg.ManagerForm = this;
                dlg.DatabaseName = current_treenode.Text;
                dlg.ExcludingDbNames = existing_dbnames;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                current_treenode.Text = dlg.DatabaseName;

                // 确保展开
                if (current_treenode.Parent != null)
                    current_treenode.Parent.Expand();

                // 需要立即向服务器提交修改
                nRet = SubmitOpacBrowseFormatDef(out strError);
                if (nRet == -1)
                {
                    current_treenode.ImageIndex = 2;    // 表示未能提交的定义变化请求
                    goto ERROR1;
                }
            }
            else
            {
                // 格式节点

                string strXml = (string)current_treenode.Tag;

                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }


                // 显示格式
                OpacBrowseFormatDialog dlg = new OpacBrowseFormatDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "请指定显示格式的属性";
                // dlg.FormatName = DomUtil.GetAttr(dom.DocumentElement, "name");
                dlg.CaptionsXml = dom.DocumentElement.InnerXml; // 2009/6/27
                dlg.FormatType = DomUtil.GetAttr(dom.DocumentElement, "type");
                dlg.ScriptFile = DomUtil.GetAttr(dom.DocumentElement, "scriptfile");
                dlg.FormatStyle = DomUtil.GetAttr(dom.DocumentElement, "style");
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                /*
                string strDisplayText = dlg.FormatName;
                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.FormatName);

                if (String.IsNullOrEmpty(dlg.FormatType) == false)
                {
                    strDisplayText += " type=" + dlg.FormatType;
                    DomUtil.SetAttr(dom.DocumentElement, "type", dlg.FormatType);
                }

                if (String.IsNullOrEmpty(dlg.ScriptFile) == false)
                {
                    strDisplayText += " scriptfile=" + dlg.ScriptFile;
                    DomUtil.SetAttr(dom.DocumentElement, "scriptfile", dlg.ScriptFile);
                }

                if (String.IsNullOrEmpty(dlg.FormatStyle) == false)
                {
                    strDisplayText += " style=" + dlg.FormatStyle;
                    DomUtil.SetAttr(dom.DocumentElement, "style", dlg.FormatStyle);
                }
                 * */

                // 2009/6/27
                if (String.IsNullOrEmpty(dlg.CaptionsXml) == false)
                    dom.DocumentElement.InnerXml = dlg.CaptionsXml;

                // 2009/6/27
                current_treenode.Text = GetFormatDisplayString(dom.DocumentElement);

                current_treenode.Tag = dom.DocumentElement.OuterXml;

                // 确保展开
                current_treenode.Parent.Expand();


                // 需要立即向服务器提交修改
                nRet = SubmitOpacBrowseFormatDef(out strError);
                if (nRet == -1)
                {
                    current_treenode.ImageIndex = 2;    // 表示未能提交的定义变化请求
                    goto ERROR1;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // popup menu
        private void treeView_opacBrowseFormats_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            TreeNode node = this.treeView_opacBrowseFormats.SelectedNode;

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_modify_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }

            // 缺省命令
            if (node != null && node.Parent != null)
                menuItem.DefaultItem = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 插入库名节点
            string strText = "";
            if (node == null)
                strText = "[追加到第一级末尾]";
            else if (node.Parent == null)
                strText = "[同级后插]";
            else
                strText = "[追加到第一级末尾]";

            menuItem = new MenuItem("新增库名节点(&N) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode_Click);
            contextMenu.MenuItems.Add(menuItem);



            // 插入显示格式节点
            if (node == null)
                strText = "";   // 这种情况不允许操作
            else if (node.Parent == null)
                strText = "[追加到下级末尾]";
            else
                strText = "[同级后插]";

            menuItem = new MenuItem("新增显示格式节点(&F) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode_Click);
            if (node == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_opacBrowseFormatNode_up_Click);
            if (this.treeView_opacBrowseFormats.SelectedNode == null
                || this.treeView_opacBrowseFormats.SelectedNode.PrevNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.menu_opacBrowseFormatNode_down_Click);
            if (treeView_opacBrowseFormats.SelectedNode == null
                || treeView_opacBrowseFormats.SelectedNode.NextNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("移除(&E)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_remove_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(treeView_opacBrowseFormats, new Point(e.X, e.Y));

        }

        void menu_opacBrowseFormatNode_up_Click(object sender, EventArgs e)
        {
            MoveUpDown(true);
        }

        void menu_opacBrowseFormatNode_down_Click(object sender, EventArgs e)
        {
            MoveUpDown(false);
        }

        void MoveUpDown(bool bUp)
        {
            string strError = "";
            int nRet = 0;

            // 当前已选择的node
            if (this.treeView_opacBrowseFormats.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要进行上下移动的节点");
                return;
            }

            TreeNodeCollection nodes = null;

            TreeNode parent = treeView_opacBrowseFormats.SelectedNode.Parent;

            if (parent == null)
                nodes = this.treeView_opacBrowseFormats.Nodes;
            else
                nodes = parent.Nodes;

            TreeNode node = treeView_opacBrowseFormats.SelectedNode;

            int index = nodes.IndexOf(node);

            Debug.Assert(index != -1, "");

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "已经到头";
                    goto ERROR1;
                }

                nodes.Remove(node);
                index--;
                nodes.Insert(index, node);
            }
            if (bUp == false)
            {
                if (index >= nodes.Count - 1)
                {
                    strError = "已经到尾";
                    goto ERROR1;
                }

                nodes.Remove(node);
                index++;
                nodes.Insert(index, node);

            }

            this.treeView_opacBrowseFormats.SelectedNode = node;

            // 需要立即向服务器提交修改
            nRet = SubmitOpacBrowseFormatDef(out strError);
            if (nRet == -1)
            {
                // TODO: 如何表示未能提交的位置变化请求
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_opacBrowseFormats_remove_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_opacBrowseFormats.SelectedNode;

            if (current_treenode == null)
            {
                strError = "尚未选定要删除的库名或格式名节点";
                goto ERROR1;
            }

            // 警告
            string strText = "确实要移除";

            if (current_treenode.Parent == null)
                strText += "库名节点";
            else
                strText += "显示格式节点";

            strText += " " + current_treenode.Text + " ";

            if (current_treenode.Parent == null)
                strText += "和其下属节点";

            strText += "?";

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                strText,
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            if (current_treenode.Parent != null)
                current_treenode.Parent.Nodes.Remove(current_treenode);
            else
            {
                Debug.Assert(current_treenode.Parent == null, "");
                this.treeView_opacBrowseFormats.Nodes.Remove(current_treenode);
            }

            // 需要立即向服务器提交修改
            nRet = SubmitOpacBrowseFormatDef(out strError);
            if (nRet == -1)
            {
                // TODO: 如何表示未能提交的移除请求
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // treeview上双击，对于库名节点依然是展开或者收缩的作用；而对格式节点是打开修改对话框的作用
        private void treeView_opacBrowseFormats_DoubleClick(object sender, EventArgs e)
        {
            // 当前已选择的node
            TreeNode node = treeView_opacBrowseFormats.SelectedNode;

            if (node == null)
                return;

            if (node.Parent == null) // 库名节点
                return;

            toolStripButton_opacBrowseFormats_modify_Click(sender, e);

        }

        // treeview中的右鼠标键。让右鼠标键也能定位
        private void treeView_opacBrowseFormats_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode curSelectedNode = this.treeView_opacBrowseFormats.GetNodeAt(e.X, e.Y);

                if (treeView_opacBrowseFormats.SelectedNode != curSelectedNode)
                {
                    treeView_opacBrowseFormats.SelectedNode = curSelectedNode;

                    if (treeView_opacBrowseFormats.SelectedNode == null)
                        treeView_opacBrowseFormats_AfterSelect(null, null);	// 补丁
                }

            }
        }

        // 刷新
        private void toolStripButton_opacBrowseFormats_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_opacBrowseFormats_refresh.Enabled = false;
            try
            {
                RefreshOpacBrowseFormatTree();
            }
            finally
            {
                this.toolStripButton_opacBrowseFormats_refresh.Enabled = true;
            }
        }

        void RefreshOpacBrowseFormatTree()
        {
            string strError = "";
            int nRet = this.ListAllOpacBrowseFormats(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        // 提交OPAC记录显示格式定义修改
        int SubmitOpacBrowseFormatDef(out string strError)
        {
            strError = "";
            string strFormatDef = "";
            int nRet = BuildOpacBrowseFormatDef(out strFormatDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.SetAllOpacBrowseFormatsDef(strFormatDef,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 构造OPAC记录显示格式定义的XML片段
        // 注意是下级片断定义，没有<browseformats>元素作为根。
        int BuildOpacBrowseFormatDef(out string strFormatDef,
            out string strError)
        {
            strError = "";
            strFormatDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<browseformats />");

            for (int i = 0; i < this.treeView_opacBrowseFormats.Nodes.Count; i++)
            {
                TreeNode item = this.treeView_opacBrowseFormats.Nodes[i];

                string strDatabaseName = item.Text;

                XmlNode database_node = dom.CreateElement("database");
                DomUtil.SetAttr(database_node, "name", strDatabaseName);

                dom.DocumentElement.AppendChild(database_node);

                for (int j = 0; j < item.Nodes.Count; j++)
                {
                    TreeNode format_treenode = item.Nodes[j];

                    string strXmlFragment = (string)format_treenode.Tag;

                    XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                    try
                    {
                        fragment.InnerXml = strXmlFragment;
                    }
                    catch (Exception ex)
                    {
                        strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                        return -1;
                    }

                    database_node.AppendChild(fragment);
                }
            }

            strFormatDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 看看一个数据库的显示格式是否存在？
        bool HasBrowseFormatDatabaseExist(string strDatabaseName)
        {
            for (int i = 0; i < this.treeView_opacBrowseFormats.Nodes.Count; i++)
            {
                if (this.treeView_opacBrowseFormats.Nodes[i].Text == strDatabaseName)
                    return true;
            }

            return false;
        }

        // 为书目库插入OPAC显示格式节点(后插)
        int NewBiblioOpacBrowseFormat(string strDatabaseName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 插入点
            int index = this.treeView_opacBrowseFormats.Nodes.Count;

            // 插入库名节点
            TreeNode new_database_treenode = new TreeNode(strDatabaseName, 0, 0);
            this.treeView_opacBrowseFormats.Nodes.Insert(index, new_database_treenode);

            // 插入格式节点
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<format />");

            string strDisplayText = "详细";
            DomUtil.SetAttr(dom.DocumentElement, "name", "详细");

            strDisplayText += " type=" + "biblio";
            DomUtil.SetAttr(dom.DocumentElement, "type", "biblio");

            // 2009/6/27
            dom.DocumentElement.InnerXml = "<caption lang=\"zh-CN\">详细</caption><caption lang=\"en\">Detail</caption>";

            TreeNode new_format_treenode = new TreeNode(strDisplayText, 1, 1);
            new_format_treenode.Tag = dom.DocumentElement.OuterXml;

            new_database_treenode.Nodes.Insert(index, new_format_treenode);

            this.treeView_opacBrowseFormats.SelectedNode = new_format_treenode;


            // 需要立即向服务器提交修改
            nRet = SubmitOpacBrowseFormatDef(out strError);
            if (nRet == -1)
            {
                new_format_treenode.ImageIndex = 2;    // 表示未能提交的新插入节点请求
                return -1;
            }

            return 0;
        }

        #endregion // of OPAC记录显示格式

        #region 读者流通权限

#if NO
        // 创建读者和册类型列表
        private void toolStripButton_loanPolicy_createTypes_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<string> booktypes = null;
            List<string> readertypes = null;

            // 从XML中搜索
            // 获得读者和图书类型列表
            int nRet = GetReaderAndBookTypes(out readertypes,
                out booktypes,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 从library.xml中<valueTables>取得

            // 

            if (booktypes.Count > 0)
            {
                if (this.textBox_loanPolicy_bookTypes.Text != "")
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前图书类型文本框内已经有内容。\r\n\r\n是否要追加新值到其中?\r\n\r\n(Yes 追加; No 覆盖; Cancel 放弃)",
                        "ManagerForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        // 追加
                        List<string> old = MakeStringList(this.textBox_loanPolicy_bookTypes.Text);
                        old.AddRange(booktypes);
                        StringUtil.RemoveDupNoSort(ref old);
                        this.textBox_loanPolicy_bookTypes.Text = StringUtil.MakePathList(old,
                            "\r\n");
                    }
                    else if (result == DialogResult.No)
                    {
                        // 覆盖
                        this.textBox_loanPolicy_bookTypes.Text = StringUtil.MakePathList(booktypes,
                            "\r\n");
                    }
                }
                else
                {
                    // 覆盖
                    this.textBox_loanPolicy_bookTypes.Text = StringUtil.MakePathList(booktypes,
                        "\r\n");
                }
            }
            else
            {
                MessageBox.Show(this, "没有发现任何图书类型");
            }

            if (readertypes.Count > 0)
            {
                if (this.textBox_loanPolicy_readerTypes.Text != "")
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前读者类型文本框内已经有内容。\r\n\r\n是否要追加新值到其中?\r\n\r\n(Yes 追加; No 覆盖; Cancel 放弃)",
                        "ManagerForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        // 追加
                        List<string> old = MakeStringList(this.textBox_loanPolicy_readerTypes.Text);
                        old.AddRange(readertypes);
                        StringUtil.RemoveDupNoSort(ref old);
                        this.textBox_loanPolicy_readerTypes.Text = StringUtil.MakePathList(old,
                            "\r\n");
                    }
                    else if (result == DialogResult.No)
                    {
                        // 覆盖
                        this.textBox_loanPolicy_readerTypes.Text = StringUtil.MakePathList(readertypes,
                            "\r\n");
                    }
                }
                else
                {
                    // 覆盖
                    this.textBox_loanPolicy_readerTypes.Text = StringUtil.MakePathList(readertypes,
                        "\r\n");
                }
            }
            else
            {
                MessageBox.Show(this, "没有发现任何读者类型");
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#endif
#if NO
        private void tabControl_loanPolicy_down_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_loanPolicy_down.SelectedTab == this.tabPage_loanPolicy_html)
            {
                this.toolStripButton_loanPolicy_createTypes.Enabled = false;

                // HTML表格属性页激活，需要同步
                SynchronizeLoanPolicy();
            }
            else
            {
                Debug.Assert(this.tabControl_loanPolicy_down.SelectedTab == this.tabPage_loanPolicy_types, "");

                this.toolStripButton_loanPolicy_createTypes.Enabled = true;

                // types属性页激活，需要同步
                SynchronizeRightsTableAndTypes();
            }

        }

        private void textBox_loanPolicy_rightsTableDef_TextChanged(object sender, EventArgs e)
        {
            // XML编辑器中的版本发生变化
            this.m_nRightsTableXmlVersion++;
            this.LoanPolicyDefChanged = true;
        }

        private void textBox_loanPolicy_readerTypes_TextChanged(object sender, EventArgs e)
        {
            // 读者类型发生变化
            this.m_nRightsTableTypesVersion++;
            this.LoanPolicyDefChanged = true;
        }

        private void textBox_loanPolicy_bookTypes_TextChanged(object sender, EventArgs e)
        {
            // 图书类型发生变化
            this.m_nRightsTableTypesVersion++;
            this.LoanPolicyDefChanged = true;
        }

        private void textBox_loanPolicy_rightsTableDef_Enter(object sender, EventArgs e)
        {
            SynchronizeLoanPolicy();
        }

        private void textBox_loanPolicy_rightsTableDef_Leave(object sender, EventArgs e)
        {
            if (this.Disposing == false)
                SynchronizeLoanPolicy();
        }

        private void toolStripButton_loanPolicy_save_Click(object sender, EventArgs e)
        {
            string strError = "";

            SynchronizeLoanPolicy();

            string strRightsTableXml = this.textBox_loanPolicy_rightsTableDef.Text;

            if (String.IsNullOrEmpty(strRightsTableXml) == true)
            {
                strRightsTableXml = "";
            }
            else
            {

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRightsTableXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串装入XMLDOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (dom.DocumentElement == null)
                {
                    dom.LoadXml("<rightsTable />");
                }

                /*
                string strReaderTypesXml = BuildTypesXml(this.textBox_loanPolicy_readerTypes);
                string strBookTypesXml = BuildTypesXml(this.textBox_loanPolicy_bookTypes);

                {
                    XmlNode nodeReaderTypes = dom.DocumentElement.SelectSingleNode("readerTypes");
                    if (nodeReaderTypes == null)
                    {
                        nodeReaderTypes = dom.CreateElement("readerTypes");
                        dom.DocumentElement.AppendChild(nodeReaderTypes);
                    }

                    nodeReaderTypes.InnerXml = strReaderTypesXml;
                }

                {
                    XmlNode nodeBookTypes = dom.DocumentElement.SelectSingleNode("bookTypes");
                    if (nodeBookTypes == null)
                    {
                        nodeBookTypes = dom.CreateElement("bookTypes");
                        dom.DocumentElement.AppendChild(nodeBookTypes);
                    }

                    nodeBookTypes.InnerXml = strBookTypesXml;
                }
                 * */

                // 似乎没有必要多做一次
                // types编辑界面 --> DOM中的<readerTypes>和<bookTypes>部分
                // 调用前dom中应当已经装入了权限XML代码
                TypesToRightsXml(ref dom);

                strRightsTableXml = dom.DocumentElement.InnerXml;
            }


            // 保存流通读者权限相关定义
            // parameters:
            //      strRightsTableXml   流通读者权限定义XML。注意，没有根元素
            int nRet = SetRightsTableDef(strRightsTableXml,
                //strReaderTypesXml,
                //strBookTypesXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.LoanPolicyDefChanged = false;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        private void toolStripButton_loanPolicy_refresh_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = this.ListRightsTables(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }
#endif

        #endregion

        #region 馆藏地配置

        // 在listview中列出所有馆藏地
        int ListAllLocations(out string strError)
        {
            strError = "";

            if (this.LocationTypesDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有馆藏地定义被修改后尚未保存。若此时重新装载馆藏地定义，现有未保存信息将丢失。\r\n\r\n确实要重新装载? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            this.listView_location_list.Items.Clear();

            string strOutputInfo = "";
            int nRet = GetAllLocationInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
            <locationTypes>
                <item canborrow="yes" itembarcodeNullable="yes">流通库</item>
                <item>阅览室</item>
                <library code="分馆1">
                    <item canborrow="yes">流通库</item>
                    <item>阅览室</item>
                </library>
            </locationTypes>
            */

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                bool bCanBorrow = false;

                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                nRet = DomUtil.GetBooleanParam(node,
                     "canborrow",
                     false,
                     out bCanBorrow,
                     out strError);
                if (nRet == -1)
                    return -1;

                string strText = node.InnerText;

#if NO
                if (String.IsNullOrEmpty(strText) == true)
                    continue;
#endif

                // 
                string strLibraryCode = "";
                XmlNode parent = node.ParentNode;
                if (parent.Name == "library")
                {
                    strLibraryCode = DomUtil.GetAttr(parent, "code");
                }

                bool bNullable = true;

                // 获得布尔型的属性参数值
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                nRet = DomUtil.GetBooleanParam(node,
                     "itemBarcodeNullable",
                     true,
                     out bNullable,
                     out strError);
                if (nRet == -1)
                    return -1;

#if NO
                ListViewItem item = new ListViewItem(strLibraryCode, 0);
                item.SubItems.Add(strText);
                item.SubItems.Add(bCanBorrow == true ? "是" : "否");
#endif
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_LIBRARYCODE, strLibraryCode);
                ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ROOM, strText);
                ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_CANBORROW, bCanBorrow == true ? "是" : "否");
                ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE, bNullable == true ? "是" : "否");

                this.listView_location_list.Items.Add(item);
            }

            this.LocationTypesDefChanged = false;

            return 1;
        }

        // <locationtypes>
        int GetAllLocationInfo(out string strOutputInfo,
    out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取<locationTypes>配置 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "locationTypes",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 修改/设置全部馆藏地定义<locationTypes>
        int SetAllLocationTypesInfo(string strLocationDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置<locationTypes>定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "locationTypes",
                    strLocationDef,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 构造<locationTypes>定义的XML片段
        // 注意是下级片断定义，没有<locationTypes>元素作为根。
        int BuildLocationTypesDef(out string strLocationDef,
            out string strError)
        {
            strError = "";
            strLocationDef = "";

            Hashtable table = new Hashtable();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<locationTypes />");

            for (int i = 0; i < this.listView_location_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_location_list.Items[i];
                string strLibraryCode = ListViewUtil.GetItemText(item, LOCATION_COLUMN_LIBRARYCODE);
                string strLocation = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ROOM);
                string strCanBorrow = ListViewUtil.GetItemText(item, LOCATION_COLUMN_CANBORROW);
                string strNullable = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE);

                bool bCanBorrow = false;
                if (strCanBorrow == "是" || strCanBorrow == "yes")
                    bCanBorrow = true;

                bool bNullable = true;
                if (strNullable == "是" || strNullable == "yes")
                    bNullable = true;
                else
                    bNullable = false;

                if (string.IsNullOrEmpty(strLibraryCode) == true)
                {
                    XmlNode nodeItem = dom.CreateElement("item");
                    dom.DocumentElement.AppendChild(nodeItem);

                    nodeItem.InnerText = strLocation;
                    DomUtil.SetAttr(nodeItem, "canborrow", bCanBorrow == true ? "yes" : "no");
                    DomUtil.SetAttr(nodeItem, "itemBarcodeNullable", bNullable == true ? "yes" : "no");
                }
                else
                {
                    // 按照馆代码分类聚集
                    List<ListViewItem> items = (List<ListViewItem>)table[strLibraryCode];
                    if (items == null)
                    {
                        items = new List<ListViewItem>();
                        table[strLibraryCode] = items;
                    }
                    items.Add(item);
                }
            }

            if (table.Count > 0)
            {
                string[] keys = new string[table.Count];
                table.Keys.CopyTo(keys, 0);
                Array.Sort(keys);

                foreach (string key in keys)
                {
                    List<ListViewItem> items = (List<ListViewItem>)table[key];

                    XmlNode nodeLibrary = dom.CreateElement("library");
                    dom.DocumentElement.AppendChild(nodeLibrary);
                    DomUtil.SetAttr(nodeLibrary, "code", key);

                    foreach (ListViewItem item in items)
                    {
                        string strLocation = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ROOM);
                        string strCanBorrow = ListViewUtil.GetItemText(item, LOCATION_COLUMN_CANBORROW);
                        string strNullable = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE);

                        bool bCanBorrow = false;
                        if (strCanBorrow == "是" || strCanBorrow == "yes")
                            bCanBorrow = true;

                        bool bNullable = true;
                        if (strNullable == "是" || strNullable == "yes")
                            bNullable = true;
                        else
                            bNullable = false;

                        XmlNode nodeItem = dom.CreateElement("item");
                        nodeLibrary.AppendChild(nodeItem);

                        nodeItem.InnerText = strLocation;
                        DomUtil.SetAttr(nodeItem, "canborrow", bCanBorrow == true ? "yes" : "no");
                        DomUtil.SetAttr(nodeItem, "itemBarcodeNullable", bNullable == true ? "yes" : "no");
                    }
                }
            }

            strLocationDef = dom.DocumentElement.InnerXml;
            return 0;
        }

        // 提交<locationTypes>定义修改
        int SubmitLocationTypesDef(out string strError)
        {
            strError = "";
            string strLocationTypesDef = "";
            int nRet = BuildLocationTypesDef(out strLocationTypesDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SetAllLocationTypesInfo(strLocationTypesDef,
                out strError);
            if (nRet == -1)
                return -1;

            // 2014/9/4
            // library.xml 中 <locationTypes> 内容或可能与 GetValueTable() API 相关，每当修改和保存了<locationTypes> 信息，都要刷新一下值列表缓存，以便界面可以重新获得 location 列表值
            Program.MainForm.ClearValueTableCache();
            return 0;
        }

        private void toolStripButton_location_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_location_refresh.Enabled = false;  // 防止快速点按钮时候多重装入列表内容
            try
            {
                // 在listview中列出所有馆藏地
                string strError = "";
                int nRet = ListAllLocations(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_location_refresh.Enabled = true;
            }
        }

        bool m_bLocationTypesDefChanged = false;

        /// <summary>
        /// 馆藏地定义是否被修改
        /// </summary>
        public bool LocationTypesDefChanged
        {
            get
            {
                return this.m_bLocationTypesDefChanged;
            }
            set
            {
                this.m_bLocationTypesDefChanged = value;
                if (value == true)
                    this.toolStripButton_location_save.Enabled = true;
                else
                    this.toolStripButton_location_save.Enabled = false;
            }
        }

        // 保存 馆藏地 配置
        private void toolStripButton_location_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = SubmitLocationTypesDef(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.LocationTypesDefChanged = false;
            }
        }

        // 查找一个馆藏地事项
        static ListViewItem FindLocationItem(ListView listview,
            string strLibraryCode,
            string strRoom)
        {
            foreach (ListViewItem item in listview.Items)
            {
                string strCurrentLibraryCode = ListViewUtil.GetItemText(item, LOCATION_COLUMN_LIBRARYCODE);
                string strCurrentRoom = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ROOM);
                if (strLibraryCode == strCurrentLibraryCode
                    && strRoom == strCurrentRoom)
                    return item;
            }

            return null;
        }

        // 新创建馆藏地点事项
        private void toolStripButton_location_new_Click(object sender, EventArgs e)
        {
            LocationItemDialog dlg = new LocationItemDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CreateMode = true;
            dlg.LibraryCodeList = this.GetLibraryCodeList();
            dlg.ItemBarcodeNullable = true;
            dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 查重
            ListViewItem dup = FindLocationItem(this.listView_location_list,
                dlg.LibraryCode,
                dlg.LocationString);
            if (dup != null)
            {
                ListViewUtil.SelectLine(dup, true);
                dup.EnsureVisible();
                MessageBox.Show(this, "拟新增的馆藏地事项 '" + GetLocationItemName(dup) + "' 已经存在了，请修改后重新创建");
                dlg.CreateMode = false; // 避免 _load 再次填充空白值
                goto REDO;
            }

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_LIBRARYCODE, dlg.LibraryCode);
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ROOM, dlg.LocationString);
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_CANBORROW, dlg.CanBorrow == true ? "是" : "否");
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE, dlg.ItemBarcodeNullable == true ? "是" : "否");

            this.listView_location_list.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            this.LocationTypesDefChanged = true;
        }

        // 修改馆藏地点事项
        private void toolStripButton_location_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_location_list.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的馆藏地点事项";
                goto ERROR1;
            }
            ListViewItem item = this.listView_location_list.SelectedItems[0];

            LocationItemDialog dlg = new LocationItemDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.LibraryCodeList = this.GetLibraryCodeList();
            dlg.LibraryCode = ListViewUtil.GetItemText(item, LOCATION_COLUMN_LIBRARYCODE);
            dlg.LocationString = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ROOM);
            dlg.CanBorrow = (ListViewUtil.GetItemText(item, LOCATION_COLUMN_CANBORROW) == "是") ? true : false;
            dlg.ItemBarcodeNullable = (ListViewUtil.GetItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE) == "是") ? true : false;
            dlg.StartPosition = FormStartPosition.CenterScreen;

        REDO:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 查重
            ListViewItem dup = FindLocationItem(this.listView_location_list,
                dlg.LibraryCode,
                dlg.LocationString);
            if (dup != null && dup != item)
            {
                ListViewUtil.SelectLine(dup, true);
                dup.EnsureVisible();
                MessageBox.Show(this, "拟修改的馆藏地事项 '" + GetLocationItemName(dup) + "' 已经存在了，请修改后重新创建");
                goto REDO;
            }

            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_LIBRARYCODE, dlg.LibraryCode);
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ROOM, dlg.LocationString);
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_CANBORROW, dlg.CanBorrow == true ? "是" : "否");
            ListViewUtil.ChangeItemText(item, LOCATION_COLUMN_ITEMBARCODENULLABLE, dlg.ItemBarcodeNullable == true ? "是" : "否");

            ListViewUtil.SelectLine(item, true);
            this.LocationTypesDefChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        const int LOCATION_COLUMN_LIBRARYCODE = 0;
        const int LOCATION_COLUMN_ROOM = 1;
        const int LOCATION_COLUMN_CANBORROW = 2;
        const int LOCATION_COLUMN_ITEMBARCODENULLABLE = 3;

        // 2014/9/6
        static string GetLocationItemName(ListViewItem item)
        {
            string strLibraryCode = ListViewUtil.GetItemText(item, LOCATION_COLUMN_LIBRARYCODE);
            string strRoom = ListViewUtil.GetItemText(item, LOCATION_COLUMN_ROOM);
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                return strRoom;

            return strLibraryCode + "/" + strRoom;
        }

        // 2014/9/6
        static string GetLocationItemNameList(ListView.SelectedListViewItemCollection items,
    string strSep = ",")
        {
            StringBuilder strItemNameList = new StringBuilder(4096);
            foreach (ListViewItem item in items)
            {
                if (strItemNameList.Length > 0)
                    strItemNameList.Append(strSep);
                strItemNameList.Append(GetLocationItemName(item));
            }

            return strItemNameList.ToString();
        }

        // 删除所选定的馆藏地点事项
        private void toolStripButton_location_delete_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_location_list.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的馆藏地点事项";
                goto ERROR1;
            }

            string strItemNameList = GetLocationItemNameList(this.listView_location_list.SelectedItems);

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除馆藏地点事项 " + strItemNameList + "?",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

#if NO
            for (int i = this.listView_location_list.SelectedIndices.Count - 1;
                i >= 0;
                i--)
            {
                int index = this.listView_location_list.SelectedIndices[i];
                string strDatabaseName = this.listView_location_list.Items[index].Text;
                this.listView_location_list.Items.RemoveAt(index);
            }
#endif
            // 2012/3/11
            ListViewUtil.DeleteSelectedItems(listView_location_list);

            this.LocationTypesDefChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // listview选择发生变动
        private void listView_location_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_location_list.SelectedItems.Count > 0)
            {
                this.toolStripButton_location_modify.Enabled = true;
                this.toolStripButton_location_delete.Enabled = true;
            }
            else
            {
                this.toolStripButton_location_modify.Enabled = false;
                this.toolStripButton_location_delete.Enabled = false;
            }

            if (this.listView_location_list.SelectedItems.Count == 0
                || this.listView_location_list.Items.IndexOf(this.listView_location_list.SelectedItems[0]) == 0)
                this.toolStripButton_location_up.Enabled = false;
            else
                this.toolStripButton_location_up.Enabled = true;

            if (this.listView_location_list.SelectedItems.Count == 0
                || this.listView_location_list.Items.IndexOf(this.listView_location_list.SelectedItems[0]) >= this.listView_location_list.Items.Count - 1)
                this.toolStripButton_location_down.Enabled = false;
            else
                this.toolStripButton_location_down.Enabled = true;
        }

        private void listView_location_list_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_location_list, e, false);
        }

        // listview事项双击
        private void listView_location_list_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_location_modify_Click(sender, e);
        }

        private void listView_location_list_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strItemName = "";
            //string strCanBorrow = "";
            if (this.listView_location_list.SelectedItems.Count > 0)
            {
                strItemName = GetLocationItemName(this.listView_location_list.SelectedItems[0]);
                //strCanBorrow = ListViewUtil.GetItemText(this.listView_location_list.SelectedItems[0], LOCATION_COLUMN_CANBORROW);
            }


            // 修改馆藏事项
            {
                menuItem = new MenuItem("修改 " + strItemName + "(&M)");
                menuItem.Click += new System.EventHandler(this.toolStripButton_location_modify_Click);
                if (this.listView_location_list.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                // 缺省命令
                menuItem.DefaultItem = true;
                contextMenu.MenuItems.Add(menuItem);
            }


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_location_new_Click);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);



            string strText = "";
            if (this.listView_location_list.SelectedItems.Count == 1)
                strText = "删除 " + strItemName + "(&D)";
            else
                strText = "删除所选 " + this.listView_location_list.SelectedItems.Count.ToString() + " 个馆藏地点事项(&D)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_location_delete_Click);
            if (this.listView_location_list.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存(&S)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_location_save_Click);
            if (this.LocationTypesDefChanged == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);



            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*
            menuItem = new MenuItem("观察所选 " + this.listView_location_list.SelectedItems.Count.ToString() + " 个馆藏事项的定义(&D)");
            menuItem.Click += new System.EventHandler(this.menu_viewOpacDatabaseDefine_Click);
            if (this.listView_location_list.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
             * */

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_location_up_Click);
            if (this.listView_location_list.SelectedItems.Count == 0
                || this.listView_location_list.Items.IndexOf(this.listView_location_list.SelectedItems[0]) == 0)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.menu_location_down_Click);
            if (this.listView_location_list.SelectedItems.Count == 0
                || this.listView_location_list.Items.IndexOf(this.listView_location_list.SelectedItems[0]) >= this.listView_location_list.Items.Count - 1)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_location_refresh_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_location_list, new Point(e.X, e.Y));

        }


        void menu_location_up_Click(object sender, EventArgs e)
        {
            MoveLocationItemUpDown(true);
        }

        void menu_location_down_Click(object sender, EventArgs e)
        {
            MoveLocationItemUpDown(false);
        }

        void MoveLocationItemUpDown(bool bUp)
        {
            string strError = "";
            // int nRet = 0;

            if (this.listView_location_list.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要进行上下移动的馆藏地点事项");
                return;
            }

            ListViewItem item = this.listView_location_list.SelectedItems[0];
            int index = this.listView_location_list.Items.IndexOf(item);

            Debug.Assert(index >= 0 && index <= this.listView_location_list.Items.Count - 1, "");

            bool bChanged = false;

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "到头";
                    goto ERROR1;
                }

                this.listView_location_list.Items.RemoveAt(index);
                index--;
                this.listView_location_list.Items.Insert(index, item);
                this.listView_location_list.FocusedItem = item;

                bChanged = true;
            }

            if (bUp == false)
            {
                if (index >= this.listView_location_list.Items.Count - 1)
                {
                    strError = "到尾";
                    goto ERROR1;
                }
                this.listView_location_list.Items.RemoveAt(index);
                index++;
                this.listView_location_list.Items.Insert(index, item);
                this.listView_location_list.FocusedItem = item;

                bChanged = true;
            }


            // TODO: 是否可以延迟提交?
            if (bChanged == true)
            {
                /*
                // 需要立即向服务器提交修改
                nRet = this.SubmitLocationTypesDef(out strError);
                if (nRet == -1)
                {
                    // TODO: 如何表示未能提交的上下位置移动请求?
                    goto ERROR1;
                }
                 * */
                this.LocationTypesDefChanged = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_location_up_Click(object sender, EventArgs e)
        {
            MoveLocationItemUpDown(true);
        }

        private void toolStripButton_location_down_Click(object sender, EventArgs e)
        {
            MoveLocationItemUpDown(false);
        }

        #endregion

        #region 值列表

        int ListValueTables(out string strError)
        {
            strError = "";

            if (this.ValueTableChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内值列表定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            string strValueTableXml = "";

            // 获得脚本相关定义
            int nRet = GetValueTableInfo(out strValueTableXml,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                // 兼容旧版本
                this.textBox_valueTables.Text = "";
                this.ValueTableChanged = false;
                this.textBox_valueTables.Enabled = false;
                return 0;
            }

            strValueTableXml = "<valueTables>" + strValueTableXml + "</valueTables>";

            string strXml = "";
            nRet = DomUtil.GetIndentXml(strValueTableXml,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.textBox_valueTables.Enabled = true;
            this.textBox_valueTables.Text = strXml;
            this.ValueTableChanged = false;

            return 1;
        }

        // 获得脚本相关定义
        int GetValueTableInfo(out string strValueTableXml,
            out string strError)
        {
            strError = "";
            strValueTableXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取值列表定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "valueTables",
                    out strValueTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 保存值列表定义
        // parameters:
        //      strValueTableXml   值列表定义XML。注意，没有根元素
        int SetValueTableDef(string strValueTableXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存值列表定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "valueTables",
                    strValueTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 清除内容中缓存的值列表定义
                Program.MainForm.ClearValueTableCache();

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }


        ERROR1:
            return -1;
        }

        bool m_bValueTableChanged = false;

        /// <summary>
        /// 值列表定义是否被修改
        /// </summary>
        public bool ValueTableChanged
        {
            get
            {
                return this.m_bValueTableChanged;
            }
            set
            {
                this.m_bValueTableChanged = value;
                if (value == true)
                    this.toolStripButton_valueTable_save.Enabled = true;
                else
                    this.toolStripButton_valueTable_save.Enabled = false;
            }
        }

        private void toolStripButton_valueTable_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strValueTableXml = this.textBox_valueTables.Text;

            if (String.IsNullOrEmpty(strValueTableXml) == true)
            {
                strValueTableXml = "";
            }
            else
            {

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strValueTableXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串装入XMLDOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (dom.DocumentElement == null)
                {
                    strValueTableXml = "";
                }
                else
                    strValueTableXml = dom.DocumentElement.InnerXml;
            }

            int nRet = SetValueTableDef(strValueTableXml,
                out strError);
            if (nRet == -1)
            {
                this.ScriptChanged = false;
                goto ERROR1;
            }

            this.ValueTableChanged = false;
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void toolStripButton_valueTable_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_valueTable_refresh.Enabled = false;
            try
            {
                string strError = "";
                int nRet = this.ListValueTables(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_valueTable_refresh.Enabled = true;
            }
        }

        private void textBox_valueTables_TextChanged(object sender, EventArgs e)
        {
            this.ValueTableChanged = true;
        }

        #endregion

        #region 脚本

        int ListScript(out string strError)
        {
            strError = "";

            if (this.ScriptChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内脚本定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            string strScriptXml = "";

            // 获得脚本相关定义
            int nRet = GetScriptInfo(out strScriptXml,
                out strError);
            if (nRet == -1)
                return -1;

            strScriptXml = "<script>" + strScriptXml + "</script>";

            string strXml = "";
            nRet = DomUtil.GetIndentXml(strScriptXml,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            // 为了显示<script>元素中的脚本的回行
            strXml = strXml.Replace("\r\n", "\n");
            strXml = strXml.Replace("\n", "\r\n");

            this.textBox_script.Text = strXml;
            this.ScriptChanged = false;

            return 1;
        }

        // 获得脚本相关定义
        int GetScriptInfo(out string strScriptXml,
            out string strError)
        {
            strError = "";
            strScriptXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取脚本定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "script",
                    out strScriptXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 保存脚本定义
        // parameters:
        //      strScriptXml   脚本定义XML。注意，没有根元素
        int SetScriptDef(string strScriptXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存脚本定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "script",
                    strScriptXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        bool m_bScriptChanged = false;

        /// <summary>
        /// 脚本程序定义是否被修改
        /// </summary>
        public bool ScriptChanged
        {
            get
            {
                return this.m_bScriptChanged;
            }
            set
            {
                this.m_bScriptChanged = value;
                if (value == true)
                    this.toolStripButton_script_save.Enabled = true;
                else
                    this.toolStripButton_script_save.Enabled = false;
            }
        }

        private void toolStripButton_script_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strScriptXml = this.textBox_script.Text;

            if (String.IsNullOrEmpty(strScriptXml) == true)
            {
                strScriptXml = "";
            }
            else
            {

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strScriptXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串装入XMLDOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (dom.DocumentElement == null)
                {
                    strScriptXml = "";
                }
                else
                    strScriptXml = dom.DocumentElement.InnerXml;
            }

            int nRet = SetScriptDef(strScriptXml,
                out strError);
            if (nRet == -1)
            {
                this.textBox_script_comment.Text = strError;
                this.ScriptChanged = false;
                goto ERROR1;
            }
            else
            {
                this.textBox_script_comment.Text = "";
            }

            this.ScriptChanged = false;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_script_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_script_refresh.Enabled = false;
            try
            {
                string strError = "";

                int nRet = this.ListScript(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_script_refresh.Enabled = true;
            }
        }

        private void textBox_script_TextChanged(object sender, EventArgs e)
        {
            this.ScriptChanged = true;
        }

        private void textBox_script_comment_DoubleClick(object sender, EventArgs e)
        {
            if (textBox_script_comment.Lines.Length == 0)
                return;

            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_script_comment,
                out x,
                out y);

            string strLine = textBox_script_comment.Lines[y];

            // 析出"(行，列)"值

            int nRet = strLine.IndexOf("(");
            if (nRet == -1)
                goto ERROR1;

            strLine = strLine.Substring(nRet + 1);
            nRet = strLine.IndexOf(")");
            if (nRet != -1)
                strLine = strLine.Substring(0, nRet);
            strLine = strLine.Trim();

            // 找到','
            nRet = strLine.IndexOf(",");
            if (nRet == -1)
                goto ERROR1;
            y = Convert.ToInt32(strLine.Substring(0, nRet).Trim()) - 1;
            x = Convert.ToInt32(strLine.Substring(nRet + 1).Trim()) - 1;

            // MessageBox.Show(Convert.ToString(x) + " , "+Convert.ToString(y));

            this.textBox_script.Focus();
            this.textBox_script.DisableEmSetSelMsg = false;
            API.SetEditCurrentCaretPos(
                textBox_script,
                x,
                y,
                true);
            this.textBox_script.DisableEmSetSelMsg = true;
            OnScriptTextCaretChanged();
            return;
        ERROR1:
            // 发出警告性的响声
            Console.Beep();
        }

        void OnScriptTextCaretChanged()
        {
            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                textBox_script,
                out x,
                out y);
            toolStripLabel_script_caretPos.Text = Convert.ToString(y + 1) + ", " + Convert.ToString(x + 1);
        }

        private void textBox_script_KeyDown(object sender, KeyEventArgs e)
        {
            OnScriptTextCaretChanged();

        }

        private void textBox_script_MouseUp(object sender, MouseEventArgs e)
        {
            OnScriptTextCaretChanged();

        }

        #endregion

        #region 种次号

        private void treeView_zhongcihao_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            // 插入格式节点的菜单项，只有在当前节点为数据库类型或者格式类型时才能enabled

            if (current_treenode == null)
            {
                this.ToolStripMenuItem_zhongcihao_insert_nstable.Enabled = true;
                this.toolStripMenuItem_zhongcihao_insert_database.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_group.Enabled = true;

                this.toolStripButton_zhongcihao_modify.Enabled = false;
                this.toolStripButton_zhongcihao_remove.Enabled = false;
            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
            {
                // nstable节点
                this.ToolStripMenuItem_zhongcihao_insert_nstable.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_database.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_group.Enabled = true;

                this.toolStripButton_zhongcihao_modify.Enabled = true;
                this.toolStripButton_zhongcihao_remove.Enabled = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_GROUP)
            {
                // group节点
                this.ToolStripMenuItem_zhongcihao_insert_nstable.Enabled = true;
                this.toolStripMenuItem_zhongcihao_insert_database.Enabled = true;
                this.toolStripMenuItem_zhongcihao_insert_group.Enabled = true;

                this.toolStripButton_zhongcihao_modify.Enabled = true;
                this.toolStripButton_zhongcihao_remove.Enabled = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_DATABASE)
            {
                // database节点
                this.ToolStripMenuItem_zhongcihao_insert_nstable.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_database.Enabled = true;
                this.toolStripMenuItem_zhongcihao_insert_group.Enabled = false;

                this.toolStripButton_zhongcihao_modify.Enabled = true;
                this.toolStripButton_zhongcihao_remove.Enabled = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_ERROR)
            {
                // error节点
                this.ToolStripMenuItem_zhongcihao_insert_nstable.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_database.Enabled = false;
                this.toolStripMenuItem_zhongcihao_insert_group.Enabled = false;

                this.toolStripButton_zhongcihao_modify.Enabled = false;
                this.toolStripButton_zhongcihao_remove.Enabled = true;
            }
        }

        // popup menu
        private void treeView_zhongcihao_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            TreeNode node = this.treeView_zhongcihao.SelectedNode;

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_zhongcihao_modify_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }

            // 缺省命令
            if (node != null && node.Parent != null)
                menuItem.DefaultItem = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 插入group节点
            string strText = "";
            if (node == null)
                strText = "[追加到第一级末尾]";
            else if (node.Parent == null)
                strText = "[同级后插]";
            else
                strText = "[追加到第一级末尾]";

            menuItem = new MenuItem("新增组节点(&N) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_group_Click);
            if (node != null && node.ImageIndex == TYPE_ZHONGCIHAO_DATABASE)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // 插入书目库节点
            if (node == null)
                strText = "";   // 这种情况不允许操作
            else if (node.Parent == null)
                strText = "[追加到下级末尾]";
            else
                strText = "[同级后插]";

            menuItem = new MenuItem("新增书目库节点(&F) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_database_Click);
            if (node == null
                || (node != null && node.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE))
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // 插入nstable节点
            strText = "";
            if (node == null)
                strText = "[追加到第一级末尾]";
            else if (node.Parent == null)
                strText = "[同级后插]";
            else if (node.Parent != null)
                strText = "";
            else
                strText = "[追加到第一级末尾]";

            menuItem = new MenuItem("新增名字表节点(&N) " + strText);
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_zhongcihao_insert_nstable_Click);
            if (node != null
                && (node.Parent != null || node.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE))
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_zhongcihao_up_Click);
            if (this.treeView_zhongcihao.SelectedNode == null
                || this.treeView_zhongcihao.SelectedNode.PrevNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.menu_zhongcihao_down_Click);
            if (treeView_zhongcihao.SelectedNode == null
                || treeView_zhongcihao.SelectedNode.NextNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("移除(&E)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_zhongcihao_remove_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(treeView_zhongcihao, new Point(e.X, e.Y));

        }

        // 插入<group>类型节点。一级节点。
        private void toolStripMenuItem_zhongcihao_insert_group_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            // 如果不是根级的节点，则向上找到根级别
            if (current_treenode != null && current_treenode.Parent != null)
            {
                current_treenode = current_treenode.Parent;
            }

            // 插入点
            int index = this.treeView_zhongcihao.Nodes.IndexOf(current_treenode);
            if (index == -1)
                index = this.treeView_zhongcihao.Nodes.Count;
            else
                index++;

            List<string> used_dbnames = GetAllUsedZhongcihaoDbName(null);

            // 询问<group>名
            ZhongcihaoGroupDialog dlg = new ZhongcihaoGroupDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "请指定组特性";
            dlg.AllZhongcihaoDatabaseInfoXml = GetAllZhongcihaoDbInfoXml();
            dlg.ExcludingDbNames = used_dbnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            /* 已经在ZhongcihaoGroupDialog对话框中检查过了
            // 检查对话框中得到的种次号库，是不是被别处用过的种次号库？
            if (used_dbnames.IndexOf(dlg.ZhongcihaoDbName) != -1)
            {
                strError = "您所指定的种次号库 '" + dlg.ZhongcihaoDbName + "' 已经被其他组使用过了。放弃创建组。";
                goto ERROR1;
            }
             * */


            // 检查所指定的种次号库是否存在。如果不存在，提醒创建它

            // 检查指定名字的种次号库是否已经创建
            // return:
            //      -2  所指定的种次号库名字，实际上是一个已经存在的其他类型的库名
            //      -1  error
            //      0   还没有创建
            //      1   已经创建
            nRet = CheckZhongcihaoDbCreated(dlg.ZhongcihaoDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
                goto ERROR1;
            if (nRet == 0)
            {
                string strComment = "种次号库 '" + dlg.ZhongcihaoDbName + "' 尚未创建。按确定按钮可创建它。";
                // return:
                //      -1  errpr
                //      0   cancel
                //      1   created
                nRet = CreateSimpleDatabase("zhongcihao",
                    dlg.ZhongcihaoDbName,
                    strComment);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                    return;
                Debug.Assert(nRet == 1, "");
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<group />");
            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.GroupName);
            DomUtil.SetAttr(dom.DocumentElement, "zhongcihaodb", dlg.ZhongcihaoDbName);

            string strGroupCaption = MakeZhongcihaoGroupNodeName(dlg.GroupName, dlg.ZhongcihaoDbName);

            TreeNode new_treenode = new TreeNode(strGroupCaption, TYPE_ZHONGCIHAO_GROUP, TYPE_ZHONGCIHAO_GROUP);
            new_treenode.Tag = dom.OuterXml;
            this.treeView_zhongcihao.Nodes.Insert(index, new_treenode);

            this.treeView_zhongcihao.SelectedNode = new_treenode;

            this.ZhongcihaoChanged = true;


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 插入<database>类型节点。二级节点
        private void toolStripMenuItem_zhongcihao_insert_database_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            if (current_treenode == null)
            {
                strError = "尚未选定组或数据库名节点，因此无法插入新的数据库名节点";
                goto ERROR1;
            }

            if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
            {
                strError = "选定的节点不能为名字表节点，而必须是组或数据库名节点，才能插入新的数据库名节点";
                goto ERROR1;
            }

            int index = -1;

            Debug.Assert(current_treenode != null, "");

            // 如果是第一级的节点，则理解为插入到它的儿子的尾部
            if (current_treenode.Parent == null)
            {
                Debug.Assert(current_treenode != null, "");

                index = current_treenode.Nodes.Count;
            }
            else
            {
                index = current_treenode.Parent.Nodes.IndexOf(current_treenode);

                Debug.Assert(index != -1, "");

                index++;

                current_treenode = current_treenode.Parent;
            }

            // 至此，current_treenode为<group>类型的节点了

            List<string> used_dbnames = Zhongcihao_GetAllUsedBiblioDbName(null);

            // 新的数据库名
            GetOpacMemberDatabaseNameDialog dlg = new GetOpacMemberDatabaseNameDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "请选择一个要参与组的书目库";
            dlg.AllDatabaseInfoXml = GetAllBiblioDbInfoXml();
            dlg.ExcludingDbNames = used_dbnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 检查对话框中得到的书目库名，是不是被别处用过的书目库名？
            if (used_dbnames.IndexOf(dlg.SelectedDatabaseName) != -1)
            {
                strError = "您所指定的书目库 '" + dlg.SelectedDatabaseName + "' 已经被其他数据库节点使用过了。放弃本次创建数据库节点操作。";
                goto ERROR1;
            }

            // 检查指定名字的书目库是否已经创建
            // return:
            //      -2  所指定的书目库名字，实际上是一个已经存在的其他类型的库名
            //      -1  error
            //      0   还没有创建
            //      1   已经创建
            nRet = CheckBiblioDbCreated(dlg.SelectedDatabaseName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库 '" + dlg.SelectedDatabaseName + "' 尚未创建。请先创建它，再来创建数据库节点。";
                goto ERROR1;
            }

            // 获得数据库syntax
            string strSyntax = "";
            // 获得书目库的syntax
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetBiblioSyntax(dlg.SelectedDatabaseName,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "在调用GetBiblioSyntax()过程中，发现并不存在书目库 '" + dlg.SelectedDatabaseName + "' 的定义";
                goto ERROR1;
            }

            string strPrefix = "";
            string strUri = "";
            if (strSyntax == "unimarc")
                strUri = Ns.unimarcxml;
            else if (strSyntax == "usmarc")
                strUri = Ns.usmarcxml;
            else
            {
                nRet = ExistingPrefix(strSyntax, out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strPrefix = strSyntax;
                Debug.Assert(nRet == 0, "");
                if (nRet == 0)
                {
                    strError = "目前名字表中尚未定义书目库格式 '" + strSyntax + "' 所对应的namespace URI，所以无法创建该格式的书目库节点";
                    goto ERROR1;
                }
            }


            if (String.IsNullOrEmpty(strPrefix) == true)
            {
                // 根据名字空间URI查找对应的prefix
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = FindNamespacePrefix(strUri,
                    out strPrefix,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "在名字表中没有找到与namespace URI '" + strUri + "' (来源于书目库格式 '" + strSyntax + "') 对应的prefix，无法创建该格式的书目库节点";
                    goto ERROR1;
                }
                Debug.Assert(nRet == 1, "");
            }


            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<database />");

            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.SelectedDatabaseName);
            DomUtil.SetAttr(dom.DocumentElement, "leftfrom", "索取类号");

            if (strSyntax == "unimarc")
            {
                DomUtil.SetAttr(dom.DocumentElement,
                                    "rightxpath",
                                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                DomUtil.SetAttr(dom.DocumentElement,
                    "titlexpath",
                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='a']/text()");
                DomUtil.SetAttr(dom.DocumentElement,
                    "authorxpath",
                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='f' or @code='g']/text()");
            }
            else if (strSyntax == "usmarc")
            {
                DomUtil.SetAttr(dom.DocumentElement,
                    "rightxpath",
                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                DomUtil.SetAttr(dom.DocumentElement,
                    "titlexpath",
                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='a']/text()");
                DomUtil.SetAttr(dom.DocumentElement,
                    "authorxpath",
                    "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='c']/text()");
            }
            else
            {
                strError = "目前暂时不能处理syntax为 '" + strSyntax + "' 的书目库节点创建...";
                goto ERROR1;
            }

            string strDatabaseCaption = MakeZhongcihaoDatabaseNodeName(dlg.SelectedDatabaseName);

            TreeNode new_treenode = new TreeNode(strDatabaseCaption,
                TYPE_ZHONGCIHAO_DATABASE, TYPE_ZHONGCIHAO_DATABASE);
            new_treenode.Tag = dom.DocumentElement.OuterXml;

            current_treenode.Nodes.Insert(index, new_treenode);

            this.treeView_zhongcihao.SelectedNode = new_treenode;

            this.ZhongcihaoChanged = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 插入<nstable>类型节点。一级节点
        private void ToolStripMenuItem_zhongcihao_insert_nstable_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            // 看看当前是否已经有了nstable节点
            TreeNode existing_node = FindExistNstableNode();
            if (existing_node != null)
            {
                this.treeView_zhongcihao.SelectedNode = existing_node;
                strError = "名字表节点已经存在。不能重复创建。";
                goto ERROR1;
            }

            // 当前节点
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            // 如果不是根级的节点，则向上找到根级别
            if (current_treenode != null && current_treenode.Parent != null)
            {
                current_treenode = current_treenode.Parent;
            }

            // 插入点
            int index = this.treeView_zhongcihao.Nodes.IndexOf(current_treenode);
            if (index == -1)
                index = this.treeView_zhongcihao.Nodes.Count;
            else
                index++;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<nstable />");
            DomUtil.SetAttr(dom.DocumentElement, "name", "nstable");

            // unimarc
            XmlNode item_node = dom.CreateElement("item");
            dom.DocumentElement.AppendChild(item_node);

            DomUtil.SetAttr(item_node, "prefix", "unimarc");
            DomUtil.SetAttr(item_node, "uri", Ns.unimarcxml);

            // usmarc
            item_node = dom.CreateElement("item");
            dom.DocumentElement.AppendChild(item_node);

            DomUtil.SetAttr(item_node, "prefix", "usmarc");
            DomUtil.SetAttr(item_node, "uri", Ns.usmarcxml);

            string strNstableCaption = MakeZhongcihaoNstableNodeName("nstable");

            TreeNode new_treenode = new TreeNode(strNstableCaption,
                TYPE_ZHONGCIHAO_NSTABLE, TYPE_ZHONGCIHAO_NSTABLE);
            new_treenode.Tag = dom.OuterXml;
            this.treeView_zhongcihao.Nodes.Insert(index, new_treenode);

            this.treeView_zhongcihao.SelectedNode = new_treenode;

            this.ZhongcihaoChanged = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存
        private void toolStripButton_zhongcihao_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = SubmitZhongcihaoDef(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.ZhongcihaoChanged = false;
            }

        }

        // 修改一个节点的定义
        private void toolStripButton_zhongcihao_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            if (current_treenode == null)
            {
                MessageBox.Show(this, "尚未选定要修改的节点");
                return;
            }

            if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_GROUP)
            {
                // 组节点


                string strXml = (string)current_treenode.Tag;
                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "<group>节点的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strGroupName = DomUtil.GetAttr(dom.DocumentElement,
                    "name");
                string strZhongcihaoDbName = DomUtil.GetAttr(dom.DocumentElement,
                    "zhongcihaodb");

                List<string> used_dbnames = GetAllUsedZhongcihaoDbName(current_treenode);

                ZhongcihaoGroupDialog dlg = new ZhongcihaoGroupDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改组特性";
                dlg.ZhongcihaoDbName = strZhongcihaoDbName;
                dlg.GroupName = strGroupName;
                dlg.AllZhongcihaoDatabaseInfoXml = GetAllZhongcihaoDbInfoXml();
                dlg.ExcludingDbNames = used_dbnames;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.GroupName);
                DomUtil.SetAttr(dom.DocumentElement, "zhongcihaodb", dlg.ZhongcihaoDbName);

                current_treenode.Text = MakeZhongcihaoGroupNodeName(dlg.GroupName, dlg.ZhongcihaoDbName);
                current_treenode.Tag = dom.DocumentElement.OuterXml;    // 2009/3/3

                // 确保展开
                // current_treenode.Parent.Expand();

                this.ZhongcihaoChanged = true;

            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_DATABASE)
            {
                // 书目库节点

                string strXml = (string)current_treenode.Tag;

                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "<database>节点的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strDatabaseName = DomUtil.GetAttr(dom.DocumentElement,
                    "name");

                List<string> used_dbnames = Zhongcihao_GetAllUsedBiblioDbName(current_treenode);

                // 新的书目库名
                GetOpacMemberDatabaseNameDialog dlg = new GetOpacMemberDatabaseNameDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改书目库名";
                dlg.SelectedDatabaseName = strDatabaseName;
                dlg.AllDatabaseInfoXml = GetAllBiblioDbInfoXml();
                dlg.ExcludingDbNames = used_dbnames;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 检查对话框中得到的书目库名，是不是被别处用过的书目库名？
                if (used_dbnames.IndexOf(dlg.SelectedDatabaseName) != -1)
                {
                    strError = "您所指定的书目库 '" + dlg.SelectedDatabaseName + "' 已经被其他数据库节点使用过了。放弃本次修改数据库节点操作。";
                    goto ERROR1;
                }

                // 检查指定名字的书目库是否已经创建
                // return:
                //      -2  所指定的书目库名字，实际上是一个已经存在的其他类型的库名
                //      -1  error
                //      0   还没有创建
                //      1   已经创建
                nRet = CheckBiblioDbCreated(dlg.SelectedDatabaseName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "书目库 '" + dlg.SelectedDatabaseName + "' 尚未创建。请先创建它，再来修改数据库节点。";
                    goto ERROR1;
                }

                // 获得数据库syntax
                string strSyntax = "";
                // 获得书目库的syntax
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetBiblioSyntax(dlg.SelectedDatabaseName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "在调用GetBiblioSyntax()过程中，发现并不存在书目库 '" + dlg.SelectedDatabaseName + "' 的定义";
                    goto ERROR1;
                }

                string strPrefix = "";
                string strUri = "";
                if (strSyntax == "unimarc")
                    strUri = Ns.unimarcxml;
                else if (strSyntax == "usmarc")
                    strUri = Ns.usmarcxml;
                else
                {
                    nRet = ExistingPrefix(strSyntax, out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        strPrefix = strSyntax;
                    Debug.Assert(nRet == 0, "");
                    if (nRet == 0)
                    {
                        strError = "目前名字表中尚未定义书目库格式 '" + strSyntax + "' 所对应的namespace URI，所以无法创建该格式的书目库节点";
                        goto ERROR1;
                    }
                }


                if (String.IsNullOrEmpty(strPrefix) == true)
                {
                    // 根据名字空间URI查找对应的prefix
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = FindNamespacePrefix(strUri,
                        out strPrefix,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "在名字表中没有找到与namespace URI '" + strUri + "' (来源于书目库格式 '" + strSyntax + "') 对应的prefix，无法创建该格式的书目库节点";
                        goto ERROR1;
                    }
                    Debug.Assert(nRet == 1, "");
                }

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.SelectedDatabaseName);
                DomUtil.SetAttr(dom.DocumentElement, "leftfrom", "索取类号");

                if (strSyntax == "unimarc")
                {
                    DomUtil.SetAttr(dom.DocumentElement,
                                        "rightxpath",
                                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                    DomUtil.SetAttr(dom.DocumentElement,
                        "titlexpath",
                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='a']/text()");
                    DomUtil.SetAttr(dom.DocumentElement,
                        "authorxpath",
                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='200']/" + strPrefix + ":subfield[@code='f' or @code='g']/text()");
                }
                else if (strSyntax == "usmarc")
                {
                    DomUtil.SetAttr(dom.DocumentElement,
                        "rightxpath",
                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='905']/" + strPrefix + ":subfield[@code='e']/text()");
                    DomUtil.SetAttr(dom.DocumentElement,
                        "titlexpath",
                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='a']/text()");
                    DomUtil.SetAttr(dom.DocumentElement,
                        "authorxpath",
                        "//" + strPrefix + ":record/" + strPrefix + ":datafield[@tag='245']/" + strPrefix + ":subfield[@code='c']/text()");
                }
                else
                {
                    strError = "目前暂时不能处理syntax为 '" + strSyntax + "' 的书目库节点修改...";
                    goto ERROR1;
                }


                string strDisplayText = MakeZhongcihaoDatabaseNodeName(dlg.SelectedDatabaseName);

                current_treenode.Text = strDisplayText;
                current_treenode.Tag = dom.DocumentElement.OuterXml;

                // 确保展开
                current_treenode.Parent.Expand();

                this.ZhongcihaoChanged = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
            {
                // 名字表节点
                string strXml = (string)current_treenode.Tag;
                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "<nstable>节点的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                ZhongcihaoNstableDialog dlg = new ZhongcihaoNstableDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.XmlString = strXml;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                dom = new XmlDocument();
                try
                {
                    dom.LoadXml(dlg.XmlString);
                }
                catch (Exception ex)
                {
                    strError = "修改后的的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strNstableName = DomUtil.GetAttr(dom.DocumentElement, "name");

                current_treenode.Text = MakeZhongcihaoNstableNodeName(strNstableName);
                current_treenode.Tag = dlg.XmlString;

                // 确保展开
                current_treenode.Parent.Expand();

                this.ZhongcihaoChanged = true;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 移除一个节点
        private void toolStripButton_zhongcihao_remove_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_zhongcihao.SelectedNode;

            if (current_treenode == null)
            {
                strError = "尚未选定要删除的节点";
                goto ERROR1;
            }

            // 警告
            string strText = "确实要移除";

            if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_DATABASE)
                strText += "库名节点";
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_GROUP)
                strText += "组节点";
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
                strText += "名字表节点";
            else if (current_treenode.ImageIndex == TYPE_ZHONGCIHAO_ERROR)
                strText += "错误节点";
            else
            {
                strError = "未知的节点类型 " + current_treenode.ImageIndex.ToString();
                goto ERROR1;
            }

            strText += " " + current_treenode.Text + " ";

            if (current_treenode.Nodes.Count > 0)
                strText += "和其下属节点";

            strText += "?";

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                strText,
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            if (current_treenode.Parent != null)
                current_treenode.Parent.Nodes.Remove(current_treenode);
            else
            {
                Debug.Assert(current_treenode.Parent == null, "");
                this.treeView_zhongcihao.Nodes.Remove(current_treenode);
            }

            this.ZhongcihaoChanged = true;

            treeView_zhongcihao_AfterSelect(this, null);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void treeView_zhongcihao_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode curSelectedNode = this.treeView_zhongcihao.GetNodeAt(e.X, e.Y);

                if (treeView_zhongcihao.SelectedNode != curSelectedNode)
                {
                    treeView_zhongcihao.SelectedNode = curSelectedNode;

                    if (treeView_zhongcihao.SelectedNode == null)
                        treeView_zhongcihao_AfterSelect(null, null);	// 补丁
                }

            }
        }

        private void toolStripButton_zhongcihao_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_zhongcihao_refresh.Enabled = false;
            try
            {
                string strError = "";
                int nRet = this.ListZhongcihao(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_zhongcihao_refresh.Enabled = true;
            }
        }


        #endregion // 种次号

        #region 排架体系


        private void treeView_arrangement_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode current_treenode = this.treeView_arrangement.SelectedNode;

            // 插入格式节点的菜单项，只有在当前节点为数据库类型或者格式类型时才能enabled

            if (current_treenode == null)
            {
                this.toolStripMenuItem_arrangement_insert_location.Enabled = false;
                this.toolStripMenuItem_arrangement_insert_group.Enabled = true;

                this.toolStripButton_arrangement_modify.Enabled = false;
                this.toolStripButton_arrangement_remove.Enabled = false;
            }
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_GROUP)
            {
                // group节点
                this.toolStripMenuItem_arrangement_insert_location.Enabled = true;
                this.toolStripMenuItem_arrangement_insert_group.Enabled = true;

                this.toolStripButton_arrangement_modify.Enabled = true;
                this.toolStripButton_arrangement_remove.Enabled = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_LOCATION)
            {
                // location节点
                this.toolStripMenuItem_arrangement_insert_location.Enabled = true;
                this.toolStripMenuItem_arrangement_insert_group.Enabled = false;

                this.toolStripButton_arrangement_modify.Enabled = true;
                this.toolStripButton_arrangement_remove.Enabled = true;
            }
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_ERROR)
            {
                // error节点
                this.toolStripMenuItem_arrangement_insert_location.Enabled = false;
                this.toolStripMenuItem_arrangement_insert_group.Enabled = false;

                this.toolStripButton_arrangement_modify.Enabled = false;
                this.toolStripButton_arrangement_remove.Enabled = true;
            }
        }

        // 观察XML定义代码
        private void toolStripButton_arrangement_viewXml_Click(object sender, EventArgs e)
        {
            if (Program.MainForm.CallNumberCfgDom == null
                || Program.MainForm.CallNumberCfgDom.DocumentElement == null)
            {
                MessageBox.Show(this, "当前内存中尚未具备排架体系XML定义代码");
                return;
            }

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "当前内存中的排架体系XML定义代码";
            // dlg.MainForm = Program.MainForm;
            dlg.XmlString = Program.MainForm.CallNumberCfgDom.DocumentElement.OuterXml;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog();
            return;
        }

        private void toolStripButton_arrangement_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = SubmitArrangementDef(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.ArrangementChanged = false;
            }

        }

        private void toolStripButton_arrangement_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_arrangement_refresh.Enabled = false;
            try
            {
                string strError = "";
                int nRet = this.ListArrangement(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_arrangement_refresh.Enabled = true;
            }
        }

        private void toolStripMenuItem_arrangement_insert_group_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_arrangement.SelectedNode;

            // 如果不是根级的节点，则向上找到根级别
            if (current_treenode != null && current_treenode.Parent != null)
            {
                current_treenode = current_treenode.Parent;
            }

            // 插入点
            int index = this.treeView_arrangement.Nodes.IndexOf(current_treenode);
            if (index == -1)
                index = this.treeView_arrangement.Nodes.Count;
            else
                index++;

            List<string> used_dbnames = GetArrangementAllUsedZhongcihaoDbName(null);

            // 询问<group>名
            ArrangementGroupDialog dlg = new ArrangementGroupDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "请指定排架体系特性";
            dlg.AllZhongcihaoDatabaseInfoXml = GetAllZhongcihaoDbInfoXml();
            dlg.ExcludingDbNames = used_dbnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
        REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 检查排架体系节点名是否重复
            nRet = CheckArrangementNameDup(dlg.ArrangementName,
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                MessageBox.Show(this, "排架体系名 '" + dlg.ArrangementName + "' 已经存在，无法重复创建，请修改");
                goto REDO_INPUT;
            }

            // 检查所指定的种次号库是否存在。如果不存在，提醒创建它
            if (String.IsNullOrEmpty(dlg.ZhongcihaoDbName) == false)
            {
                // 检查指定名字的种次号库是否已经创建
                // return:
                //      -2  所指定的种次号库名字，实际上是一个已经存在的其他类型的库名
                //      -1  error
                //      0   还没有创建
                //      1   已经创建
                nRet = CheckZhongcihaoDbCreated(dlg.ZhongcihaoDbName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                    goto ERROR1;
                if (nRet == 0)
                {
                    string strComment = "种次号库 '" + dlg.ZhongcihaoDbName + "' 尚未创建。按确定按钮可创建它。";
                    // return:
                    //      -1  errpr
                    //      0   cancel
                    //      1   created
                    nRet = CreateSimpleDatabase("zhongcihao",
                        dlg.ZhongcihaoDbName,
                        strComment);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                        return;
                    Debug.Assert(nRet == 1, "");
                }
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<group />");
            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.ArrangementName);
            DomUtil.SetAttr(dom.DocumentElement, "classType", dlg.ClassType);
            DomUtil.SetAttr(dom.DocumentElement, "qufenhaoType", dlg.QufenhaoType);
            DomUtil.SetAttr(dom.DocumentElement, "zhongcihaodb", dlg.ZhongcihaoDbName);
            DomUtil.SetAttr(dom.DocumentElement, "callNumberStyle", dlg.CallNumberStyle);

            string strGroupCaption = MakeArrangementGroupNodeName(
                dlg.ArrangementName,
                dlg.ClassType,
                dlg.QufenhaoType,
                dlg.ZhongcihaoDbName,
                dlg.CallNumberStyle);

            TreeNode new_treenode = new TreeNode(strGroupCaption, TYPE_ARRANGEMENT_GROUP, TYPE_ARRANGEMENT_GROUP);
            new_treenode.Tag = dom.OuterXml;
            this.treeView_arrangement.Nodes.Insert(index, new_treenode);

            this.treeView_arrangement.SelectedNode = new_treenode;

            this.ArrangementChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检查排架体系名是否重复
        int CheckArrangementNameDup(string strNewName,
            TreeNode exclude,
            out string strError)
        {
            strError = "";
            foreach (TreeNode node in this.treeView_arrangement.Nodes)
            {
                if (node == exclude)
                    continue;

                string strXml = (string)node.Tag;
                if (string.IsNullOrEmpty(strXml) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串装入DOM时出错: " + ex.Message;
                    return -1;
                }

                string strName = DomUtil.GetAttr(dom.DocumentElement,
                    "name");

                if (strNewName == strName)
                    return 1;   // 发现重复
            }

            return 0;
        }

        private void toolStripButton_arrangement_remove_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_arrangement.SelectedNode;

            if (current_treenode == null)
            {
                strError = "尚未选定要移除的节点";
                goto ERROR1;
            }

            // 警告
            string strText = "确实要移除";

            if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_LOCATION)
                strText += "馆藏地点名节点";
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_GROUP)
                strText += "排架体系节点";
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_ERROR)
                strText += "错误节点";
            else
            {
                strError = "未知的节点类型 " + current_treenode.ImageIndex.ToString();
                goto ERROR1;
            }

            strText += " " + current_treenode.Text + " ";

            if (current_treenode.Nodes.Count > 0)
                strText += "和其下属节点";

            strText += "?";

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                strText,
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            if (current_treenode.Parent != null)
                current_treenode.Parent.Nodes.Remove(current_treenode);
            else
            {
                Debug.Assert(current_treenode.Parent == null, "");
                this.treeView_arrangement.Nodes.Remove(current_treenode);
            }

            this.ArrangementChanged = true;

            treeView_arrangement_AfterSelect(this, null);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripMenuItem_arrangement_insert_location_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_arrangement.SelectedNode;

            if (current_treenode == null)
            {
                strError = "尚未选定排架体系节点，因此无法插入新的馆藏地点名节点";
                goto ERROR1;
            }

            int index = -1;

            Debug.Assert(current_treenode != null, "");

            // 如果是第一级的节点，则理解为插入到它的儿子的尾部
            if (current_treenode.Parent == null)
            {
                Debug.Assert(current_treenode != null, "");

                index = current_treenode.Nodes.Count;
            }
            else
            {
                index = current_treenode.Parent.Nodes.IndexOf(current_treenode);

                Debug.Assert(index != -1, "");

                index++;

                current_treenode = current_treenode.Parent;
            }

            // 至此，current_treenode为<group>类型的节点了

            List<string> used_locationnames = GetArrangementAllUsedLocationName(null);

            // 新的馆藏地名
            ArrangementLocationDialog dlg = new ArrangementLocationDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "请指定馆藏地点";
            if (this.Channel != null)
                dlg.LibraryCodeList = this.Channel.LibraryCodeList;
            dlg.ExcludingLocationNames = used_locationnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            /*
            // 检查对话框中得到的书目库名，是不是被别处用过的书目库名？
            if (used_locationnames.IndexOf(dlg.SelectedDatabaseName) != -1)
            {
                strError = "您所指定的书目库 '" + dlg.SelectedDatabaseName + "' 已经被其他数据库节点使用过了。放弃本次创建数据库节点操作。";
                goto ERROR1;
            }*/

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<location />");

            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.LocationString);

            string strLocationCaption = MakeArrangementLocationNodeName(dlg.LocationString);

            TreeNode new_treenode = new TreeNode(strLocationCaption,
                TYPE_ARRANGEMENT_LOCATION, TYPE_ARRANGEMENT_LOCATION);
            new_treenode.Tag = dom.DocumentElement.OuterXml;

            current_treenode.Nodes.Insert(index, new_treenode);

            this.treeView_arrangement.SelectedNode = new_treenode;

            this.ArrangementChanged = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
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

        private void toolStripButton_arrangement_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 当前节点
            TreeNode current_treenode = this.treeView_arrangement.SelectedNode;

            if (current_treenode == null)
            {
                MessageBox.Show(this, "尚未选定要修改的节点");
                return;
            }

            if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_GROUP)
            {
                // 组节点
                string strXml = (string)current_treenode.Tag;
                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "<group>节点的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strArrangementName = DomUtil.GetAttr(dom.DocumentElement,
                    "name");
                string strClassType = DomUtil.GetAttr(dom.DocumentElement,
                    "classType");
                string strQufenhaoType = DomUtil.GetAttr(dom.DocumentElement,
                    "qufenhaoType");
                string strZhongcihaoDbName = DomUtil.GetAttr(dom.DocumentElement,
                    "zhongcihaodb");
                string strCallNumberStyle = DomUtil.GetAttr(dom.DocumentElement,
    "callNumberStyle");

                List<string> used_dbnames = GetArrangementAllUsedZhongcihaoDbName(current_treenode);

                ArrangementGroupDialog dlg = new ArrangementGroupDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改排架体系特性";
                dlg.ArrangementName = strArrangementName;
                dlg.ClassType = strClassType;
                dlg.QufenhaoType = strQufenhaoType;
                dlg.ZhongcihaoDbName = strZhongcihaoDbName;
                dlg.CallNumberStyle = strCallNumberStyle;
                dlg.AllZhongcihaoDatabaseInfoXml = GetAllZhongcihaoDbInfoXml();
                dlg.ExcludingDbNames = used_dbnames;
                dlg.StartPosition = FormStartPosition.CenterScreen;
            REDO_INPUT:
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // 检查排架体系节点名是否重复
                nRet = CheckArrangementNameDup(dlg.ArrangementName,
                    current_treenode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    MessageBox.Show(this, "排架体系名 '" + dlg.ArrangementName + "' 已经存在，无法重复创建，请修改");
                    goto REDO_INPUT;
                }

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.ArrangementName);
                DomUtil.SetAttr(dom.DocumentElement, "classType", dlg.ClassType);
                DomUtil.SetAttr(dom.DocumentElement, "qufenhaoType", dlg.QufenhaoType);
                DomUtil.SetAttr(dom.DocumentElement, "zhongcihaodb", dlg.ZhongcihaoDbName);
                DomUtil.SetAttr(dom.DocumentElement, "callNumberStyle", dlg.CallNumberStyle);

                current_treenode.Text = MakeArrangementGroupNodeName(
                    dlg.ArrangementName,
                    dlg.ClassType,
                    dlg.QufenhaoType,
                    dlg.ZhongcihaoDbName,
                    dlg.CallNumberStyle);
                current_treenode.Tag = dom.DocumentElement.OuterXml;

                // 确保展开
                // current_treenode.Parent.Expand();

                this.ArrangementChanged = true;

            }
            else if (current_treenode.ImageIndex == TYPE_ARRANGEMENT_LOCATION)
            {
                // 馆藏地点节点
                string strXml = (string)current_treenode.Tag;

                if (String.IsNullOrEmpty(strXml) == true)
                {
                    strError = "节点 " + current_treenode.Text + " 没有Tag定义";
                    goto ERROR1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "<location>节点的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strLocationName = DomUtil.GetAttr(dom.DocumentElement,
                    "name");

                List<string> used_locationnames = this.GetArrangementAllUsedLocationName(current_treenode);

                ArrangementLocationDialog dlg = new ArrangementLocationDialog();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.Text = "修改馆藏地点名";
                if (this.Channel != null)
                    dlg.LibraryCodeList = this.Channel.LibraryCodeList;
                dlg.LocationString = strLocationName;
                dlg.ExcludingLocationNames = used_locationnames;
                dlg.StartPosition = FormStartPosition.CenterScreen;

                dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
                dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                /*
                // 检查对话框中得到的书目库名，是不是被别处用过的书目库名？
                if (used_locationnames.IndexOf(dlg.SelectedDatabaseName) != -1)
                {
                    strError = "您所指定的书目库 '" + dlg.SelectedDatabaseName + "' 已经被其他数据库节点使用过了。放弃本次修改数据库节点操作。";
                    goto ERROR1;
                }*/

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.LocationString);

                string strDisplayText = MakeArrangementLocationNodeName(dlg.LocationString);

                current_treenode.Text = strDisplayText;
                current_treenode.Tag = dom.DocumentElement.OuterXml;

                // 确保展开
                current_treenode.Parent.Expand();

                this.ArrangementChanged = true;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 右鼠标键也能选定树节点
        private void treeView_arrangement_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode curSelectedNode = this.treeView_arrangement.GetNodeAt(e.X, e.Y);

                if (treeView_arrangement.SelectedNode != curSelectedNode)
                {
                    treeView_arrangement.SelectedNode = curSelectedNode;

                    if (treeView_arrangement.SelectedNode == null)
                        treeView_arrangement_AfterSelect(null, null);	// 补丁
                }
            }
        }

        // context menu
        private void treeView_arrangement_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            TreeNode node = this.treeView_arrangement.SelectedNode;

            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_arrangement_modify_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }

            // 缺省命令
            if (node != null && node.Parent != null)
                menuItem.DefaultItem = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 插入group节点
            string strText = "";
            if (node == null)
                strText = "[追加到第一级末尾]";
            else if (node.Parent == null)
                strText = "[同级后插]";
            else
                strText = "[追加到第一级末尾]";

            menuItem = new MenuItem("新增排架体系节点(&N) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_arrangement_insert_group_Click);
            if (node != null && node.ImageIndex == TYPE_ARRANGEMENT_LOCATION)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // 插入馆藏地点节点
            if (node == null)
                strText = "";   // 这种情况不允许操作
            else if (node.Parent == null)
                strText = "[追加到下级末尾]";
            else
                strText = "[同级后插]";

            menuItem = new MenuItem("新增馆藏地点节点(&F) " + strText);
            menuItem.Click += new System.EventHandler(this.toolStripMenuItem_arrangement_insert_location_Click);
            if (node == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.menu_arrangement_up_Click);
            if (this.treeView_arrangement.SelectedNode == null
                || this.treeView_arrangement.SelectedNode.PrevNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);



            // 
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.menu_arrangement_down_Click);
            if (treeView_arrangement.SelectedNode == null
                || treeView_arrangement.SelectedNode.NextNode == null)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("移除(&E)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_arrangement_remove_Click);
            if (node == null)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(treeView_arrangement, new Point(e.X, e.Y));

        }

        #endregion // 排架体系


        #region 查重

        private void listView_dup_projects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_dup_projects.SelectedIndices.Count == 0)
            {
                this.toolStripButton_dup_project_modify.Enabled = false;
                this.toolStripButton_dup_project_delete.Enabled = false;
            }
            else
            {
                this.toolStripButton_dup_project_modify.Enabled = true;
                this.toolStripButton_dup_project_delete.Enabled = true;
            }
        }

        // 新增一个方案
        private void toolStripButton_dup_project_new_Click(object sender, EventArgs e)
        {
            // 复制出一个新的DOM
            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(this.m_dup_dom.OuterXml);

            ProjectDialog dlg = new ProjectDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CreateMode = true;
            //dlg.DupCfgDialog = this;
            dlg.DbFromInfos = Program.MainForm.BiblioDbFromInfos;
            dlg.BiblioDbNames = this.GetAllBiblioDbNames();
            dlg.ProjectName = "新的查重方案";
            dlg.ProjectComment = "";
            dlg.dom = new_dom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.m_dup_dom = new_dom;

            // 刷新列表
            FillProjectNameList(this.m_dup_dom);

            // 选定刚插入的事项
            SelectProjectItem(dlg.ProjectName);

            this.DupChanged = true;

            FillDefaultList(this.m_dup_dom);  // 库名的集合可能发生改变
        }

        private void toolStripButton_dup_project_modify_Click(object sender, EventArgs e)
        {
            if (this.listView_dup_projects.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的查重方案事项");
                return;
            }

            ListViewItem item = this.listView_dup_projects.SelectedItems[0];

            string strProjectName = item.Text;
            string strProjectComment = ListViewUtil.GetItemText(item, 1);

            // 复制出一个新的DOM
            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(this.m_dup_dom.OuterXml);

            ProjectDialog dlg = new ProjectDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.CreateMode = false;
            // dlg.DupCfgDialog = this;
            dlg.DbFromInfos = Program.MainForm.BiblioDbFromInfos;
            dlg.BiblioDbNames = this.GetAllBiblioDbNames();
            dlg.ProjectName = strProjectName;
            dlg.ProjectComment = strProjectComment;
            dlg.dom = new_dom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.m_dup_dom = new_dom;

            item.Text = dlg.ProjectName;
            ListViewUtil.ChangeItemText(item,
                1, dlg.ProjectComment);

            this.DupChanged = true;

            FillDefaultList(this.m_dup_dom); // 库名的集合可能发生改变

            if (strProjectName != dlg.ProjectName)
            {
                // 方案名发生改变后，兑现到下方的缺省关系列表中
                ChangeDefaultProjectName(strProjectName,
                    dlg.ProjectName);
            }

        }

        private void listView_dup_projects_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_dup_project_modify_Click(sender, e);
        }

        private void toolStripButton_dup_project_delete_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_dup_projects.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的查重方案事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除所选定的 " + this.listView_dup_projects.SelectedIndices.Count.ToString() + " 个查重方案?",
                "DupCfgDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            for (int i = this.listView_dup_projects.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = this.listView_dup_projects.SelectedIndices[i];

                ListViewItem item = this.listView_dup_projects.Items[index];

                string strProjectName = item.Text;

                XmlNode nodeProject = this.m_dup_dom.DocumentElement.SelectSingleNode("//project[@name='" + strProjectName + "']");
                if (nodeProject == null)
                {
                    strError = "不存在name属性值为 '" + strProjectName + "' 的<project>元素";
                    goto ERROR1;
                }

                nodeProject.ParentNode.RemoveChild(nodeProject);

                this.listView_dup_projects.Items.RemoveAt(index);

                // 方案名删除，兑现到下方的缺省关系列表中，也删除相应的列
                ChangeDefaultProjectName(strProjectName,
                        null);
            }

            this.DupChanged = true;

            FillDefaultList(this.m_dup_dom); // 库名的集合可能发生改变

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_dup_default_new_Click(object sender, EventArgs e)
        {
            string strError = "";

            DefaultProjectDialog dlg = new DefaultProjectDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "新增缺省关系事项";
            // dlg.DupCfgDialog = this;
            dlg.BiblioDbNames = this.GetAllBiblioDbNames();
            dlg.dom = this.m_dup_dom;
            dlg.DatabaseName = "";  // 让填入内容
            dlg.DefaultProjectName = "";

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 兑现到DOM中
            XmlNode nodeDefault = this.m_dup_dom.DocumentElement.SelectSingleNode("//default[@origin='" + dlg.DatabaseName + "']");
            if (nodeDefault != null)
            {
                // 查重
                strError = "发起路径为 '" + dlg.DatabaseName + "' 的缺省关系事项已经存在，不能再次新增。可编辑已经存在的该事项。";
                goto ERROR1;
            }

            {
                nodeDefault = this.m_dup_dom.CreateElement("default");

                this.m_dup_dom.DocumentElement.AppendChild(nodeDefault);
            }
            DomUtil.SetAttr(nodeDefault, "origin", dlg.DatabaseName);
            DomUtil.SetAttr(nodeDefault, "project", dlg.DefaultProjectName);

            // 兑现到listview中
            ListViewItem item = new ListViewItem(dlg.DatabaseName, 0);
            item.SubItems.Add(dlg.DefaultProjectName);
            this.listView_dup_defaults.Items.Add(item);

            // 看看数据库名字是否在已经用到的数据库名集合中？如果是，为实在颜色；如果不是，为发虚颜色
            List<string> database_names = GetAllBiblioDbNames();
            if (database_names.IndexOf(dlg.DatabaseName) == -1)
            {
                item.ForeColor = SystemColors.GrayText;
                item.Tag = null;
            }
            else
            {
                item.Tag = 1;
            }

            this.DupChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_dup_default_modify_Click(object sender, EventArgs e)
        {
            if (this.listView_dup_defaults.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的缺省关系事项");
                return;
            }

            ListViewItem item = this.listView_dup_defaults.SelectedItems[0];

            DefaultProjectDialog dlg = new DefaultProjectDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.dom = this.m_dup_dom;
            dlg.DatabaseName = item.Text;
            dlg.DefaultProjectName = ListViewUtil.GetItemText(item, 1);

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 兑现到DOM中
            XmlNode nodeDefault = this.m_dup_dom.DocumentElement.SelectSingleNode("//default[@origin='" + item.Text + "']");
            if (nodeDefault == null)
            {
                nodeDefault = this.m_dup_dom.CreateElement("default");
                this.m_dup_dom.DocumentElement.AppendChild(nodeDefault);
            }

            DomUtil.SetAttr(nodeDefault, "origin", item.Text);
            DomUtil.SetAttr(nodeDefault, "project", dlg.DefaultProjectName);


            // 兑现到listview中
            Debug.Assert(dlg.DatabaseName == item.Text, "");
            ListViewUtil.ChangeItemText(item, 1, dlg.DefaultProjectName);

            this.DupChanged = true;
        }

        private void listView_dup_defaults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_dup_defaults.SelectedIndices.Count == 0)
            {
                this.toolStripButton_dup_default_modify.Enabled = false;
                this.toolStripButton_dup_default_delete.Enabled = false;
            }
            else
            {
                this.toolStripButton_dup_default_modify.Enabled = true;
                this.toolStripButton_dup_default_delete.Enabled = true;
            }
        }

        private void listView_dup_defaults_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_dup_default_modify_Click(sender, e);
        }

        // 删除缺省关系事项
        // TODO: 允许一次可以删除多个事项
        private void toolStripButton_dup_default_delete_Click(object sender, EventArgs e)
        {
            // string strError = "";

            if (this.listView_dup_defaults.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的缺省关系事项");
                return;
            }

            ListViewItem item = this.listView_dup_defaults.SelectedItems[0];
            string strText = item.Text + " -- " + ListViewUtil.GetItemText(item, 1);
            if (item.Tag == null)
            {
                DialogResult result = MessageBox.Show(this,
                    "确实要删除缺省关系事项 " + strText + " ?",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;

                // 发虚的事项可以删除
                this.listView_dup_defaults.Items.Remove(item);
            }
            else
            {
                // TODO: 如果方案名列本来就是空，就没有必要进行删除了

                DialogResult result = MessageBox.Show(this,
                    "确实要清除缺省关系事项 " + strText + " ?",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;

                // 实在的事项，只能抹除方案名栏内容
                ListViewUtil.ChangeItemText(item, 1, "");
            }

            // 兑现到DOM中
            XmlNode nodeDefault = this.m_dup_dom.DocumentElement.SelectSingleNode("//default[@origin='" + item.Text + "']");
            /*
            if (nodeDefault == null)
            {
                strError = "发起路径为 '" + item.Text + "' 的缺省关系事项居然在DOM中不存在";
                goto ERROR1;
            }*/
            if (nodeDefault != null)
            {
                nodeDefault.ParentNode.RemoveChild(nodeDefault);
                this.DupChanged = true;
            }
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        private void toolStripButton_dup_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_dup_refresh.Enabled = false;
            try
            {
                string strError = "";
                int nRet = this.ListDup(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_dup_refresh.Enabled = true;
            }
        }

        private void toolStripButton_dup_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = this.SubmitDupDef(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.DupChanged = false;
            }

        }

#if NO
        ListViewItem m_currentLibraryCodeItem = null;

        // 馆代码列表发生了选择改变
        private void listView_loanPolicy_libraryCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_loanPolicy_libraryCodes.SelectedItems.Count != 1)
            {
                FinishLibraryCodeTextbox();

                int nSave = this.m_nRightsTableTypesVersion;

                this.textBox_loanPolicy_bookTypes.Text = "";
                this.textBox_loanPolicy_readerTypes.Text = "";

                this.m_nRightsTableTypesVersion = nSave;    // 恢复计数器变量的变化。因为这种变化不是真正的击键引起的

                this.textBox_loanPolicy_bookTypes.Enabled = false;
                this.textBox_loanPolicy_readerTypes.Enabled = false;

                this.m_currentLibraryCodeItem = null;
            }
            else
            {
                // 从.tag到textbox
                LibraryCodeInfo info = (LibraryCodeInfo)this.listView_loanPolicy_libraryCodes.SelectedItems[0].Tag;

                int nSave = this.m_nRightsTableTypesVersion;

                this.textBox_loanPolicy_bookTypes.Text = info.BookTypeList;
                this.textBox_loanPolicy_readerTypes.Text = info.ReaderTypeList;

                this.m_nRightsTableTypesVersion = nSave;    // 恢复计数器变量的变化。因为这种变化不是真正的击键引起的

                if (this.textBox_loanPolicy_bookTypes.Enabled == false)
                    this.textBox_loanPolicy_bookTypes.Enabled = true;
                if (this.textBox_loanPolicy_readerTypes.Enabled == false)
                    this.textBox_loanPolicy_readerTypes.Enabled = true;

                this.m_currentLibraryCodeItem = this.listView_loanPolicy_libraryCodes.SelectedItems[0];
            }
        }

#endif

        private void listView_center_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strName = "";
            if (this.listView_center.SelectedItems.Count > 0)
            {
                strName = this.listView_center.SelectedItems[0].Text;
            }

            // 修改数据库
            {
                menuItem = new MenuItem("修改服务器 '" + strName + "'(&M)");
                menuItem.Click += new System.EventHandler(this.toolStripButton_center_modify_Click);
                if (this.listView_center.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                // 缺省命令
                menuItem.DefaultItem = true;
                contextMenu.MenuItems.Add(menuItem);
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("添加服务器(&A)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_center_add_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            string strText = "";
            if (this.listView_databases.SelectedItems.Count == 1)
                strText = "移除服务器 '" + strName + "'(&R)";
            else
                strText = "移除所选 " + this.listView_center.SelectedItems.Count.ToString() + " 个服务器(&R)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_center_delete_Click);
            if (this.listView_center.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_center_refresh_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_center, new Point(e.X, e.Y));

        }

        private void listView_center_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_center.SelectedItems.Count == 0)
            {
                this.toolStripButton_center_delete.Enabled = false;
                this.toolStripButton_center_modify.Enabled = false;
            }
            else
            {
                this.toolStripButton_center_delete.Enabled = true;
                this.toolStripButton_center_modify.Enabled = true;
            }
        }

        private void listView_center_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_center_modify_Click(sender, e);
        }

        private void textBox_newLoanPolicy_xml_TextChanged(object sender, EventArgs e)
        {
            // XML编辑器中的版本发生变化
            this.m_nRightsTableXmlVersion++;
            this.LoanPolicyDefChanged = true;
        }

        void loanPolicyControlWrapper1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.m_nRightsTableTypesVersion++;
            this.LoanPolicyDefChanged = true;
        }

        private void tabControl_newLoanPolicy_SelectedIndexChanged(object sender, EventArgs e)
        {
            SynchronizeRightsTableAndXml();
        }

        private void toolStripButton_newLoanPolicy_save_Click(object sender, EventArgs e)
        {
            string strError = "";

            SynchronizeRightsTableAndXml();

            string strRightsTableXml = this.textBox_newLoanPolicy_xml.Text;

            // 去掉外围的 <rightsTable> ... </rightsTable>
            if (String.IsNullOrEmpty(strRightsTableXml) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strRightsTableXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串装入XMLDOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (dom.DocumentElement == null)
                {
                    dom.LoadXml("<rightsTable />");
                }

                strRightsTableXml = dom.DocumentElement.InnerXml;
            }

            // 保存流通读者权限相关定义
            // parameters:
            //      strRightsTableXml   流通读者权限定义XML。注意，没有根元素
            int nRet = SetRightsTableDef(strRightsTableXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.LoanPolicyDefChanged = false;
            Program.MainForm.ClearValueTableCache();   // 因为 <rightsTable> 里面的 <readerTypes> 和 <bookTypes> 元素可能发生变化，影响着值列表缓存
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_newLoanPolicy_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_newLoanPolicy_refresh.Enabled = false;
            try
            {
                string strError = "";

                int nRet = this.NewListRightsTables(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                this.toolStripButton_newLoanPolicy_refresh.Enabled = true;
            }
        }

        #region 日历有关功能

        private void listView_calendar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strName = "";
            if (this.listView_calendar.SelectedItems.Count > 0)
            {
                strName = this.listView_calendar.SelectedItems[0].Text;
            }


            // 修改
            {
                menuItem = new MenuItem("修改 " + strName + " (&M)");
                menuItem.Click += new System.EventHandler(this.toolStripButton_calendar_modify_Click);
                if (this.listView_calendar.SelectedItems.Count == 0
                || StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.29") < 0)
                    menuItem.Enabled = false;
                // 缺省命令
                menuItem.DefaultItem = true;
                contextMenu.MenuItems.Add(menuItem);
            }


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_calendar_new_Click);
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.29") < 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);



            string strText = "";
            if (this.listView_calendar.SelectedItems.Count == 1)
                strText = "删除 " + strName + "(&D)";
            else
                strText = "删除所选 " + this.listView_calendar.SelectedItems.Count.ToString() + " 个日历(&D)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_calendar_delete_Click);
            if (this.listView_calendar.SelectedItems.Count == 0
                || StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.29") < 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存(&S)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_calendar_save_Click);
            if (this.CalendarDefChanged == false
                || StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.29") < 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_calendar_refresh_Click);
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_calendar, new Point(e.X, e.Y));
        }

        private void toolStripButton_calendar_modify_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_calendar.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的日历事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_calendar.SelectedItems[0];

            if (item.ImageIndex == ITEMTYPE_DELETED)
            {
                DialogResult result = MessageBox.Show(this,
    "此日历事项当前为已删除状态。\r\n\r\n需要恢复它么？\r\n\r\n(Yes: 恢复并编辑; No: 不恢复，以只读状态观察; Cancel: 放弃本次编辑操作)",
    "ManagerForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    SetCalendarItemState(item, ITEMTYPE_CHANGED);
                    m_nCalendarVersion++;
                }
            }

            string strCalendarName = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_NAME);
            // 如果不是当前用户管辖的分馆，则对话框为只读状态
            bool bOwner = true;
            {
                string strLibraryCode = "";
                string strPureName = "";

                Global.ParseCalendarName(strCalendarName,
            out strLibraryCode,
            out strPureName);

                if (Global.IsGlobalUser(this.Channel.LibraryCodeList) == false
                    && StringUtil.IsInList(strLibraryCode, this.Channel.LibraryCodeList) == false)
                    bOwner = false;
            }

            CalendarDialog dlg = new CalendarDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.OwnerLibraryCodes = GetOwnerLibraryCodes();
            dlg.IsGlobalUser = Global.IsGlobalUser(this.Channel.LibraryCodeList);
            dlg.CreateMode = false;
            dlg.ReadOnly = item.ImageIndex == ITEMTYPE_DELETED || bOwner == false;
            if (dlg.ReadOnly)
                dlg.Text = "日历";
            else
                dlg.Text = "修改日历";
            dlg.CalendarName = strCalendarName;
            dlg.Range = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_RANGE);
            dlg.Comment = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_COMMENT);
            dlg.Content = ListViewUtil.GetItemText(item, COLUMN_CALENDAR_CONTENT);
            Program.MainForm.AppInfo.LinkFormState(dlg, "CalendarDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_NAME, dlg.CalendarName);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_RANGE, dlg.Range);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_COMMENT, dlg.Comment);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_CONTENT, dlg.Content);

            if (item.ImageIndex != ITEMTYPE_NEW)
                SetCalendarItemState(item, ITEMTYPE_CHANGED);

            this.CalendarDefChanged = true;

            // 一般编辑的时候，不可能修改日历名，所以 m_nCalendarVersion++; 就不用了

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_calendar_new_Click(object sender, EventArgs e)
        {
            // string strError = "";

            CalendarDialog dlg = new CalendarDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            DateTime now = DateTime.Now;

            dlg.Text = "创建日历";
            dlg.IsGlobalUser = Global.IsGlobalUser(this.Channel.LibraryCodeList);
            dlg.OwnerLibraryCodes = GetOwnerLibraryCodes();
            dlg.CreateMode = true;
            dlg.CalendarName = Global.BuildCalendarName(Global.GetFirstLibraryCode(this.Channel.LibraryCodeList), "新建日历");    // 当前用户所在分馆
            dlg.Range = DateTimeUtil.DateTimeToString8(new DateTime(now.Year, 1, 1)) + "-" + DateTimeUtil.DateTimeToString8(new DateTime(now.Year + 2, 12, 31));   // 最近两年
            dlg.Comment = "";
            dlg.Content = "";

        REDO:
            Program.MainForm.AppInfo.LinkFormState(dlg, "CalendarDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 日历名查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_calendar, dlg.CalendarName, COLUMN_CALENDAR_NAME);
            if (dup != null)
            {
                ListViewUtil.SelectLine(dup, true);
                MessageBox.Show(this, "日历名 '" + dlg.CalendarName + "' 和现有的日历名重复了，请修改");
                goto REDO;
            }

            ListViewItem item = new ListViewItem();

            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_NAME, dlg.CalendarName);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_RANGE, dlg.Range);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_COMMENT, dlg.Comment);
            ListViewUtil.ChangeItemText(item, COLUMN_CALENDAR_CONTENT, dlg.Content);

            SetCalendarItemState(item, ITEMTYPE_NEW);

            this.listView_calendar.Items.Add(item);
            ListViewUtil.SelectLine(item, true);

            this.CalendarDefChanged = true;
            m_nCalendarVersion++;

            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void toolStripButton_calendar_delete_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_calendar.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的日历事项";
                goto ERROR1;
            }

            string strItemNameList = ListViewUtil.GetItemNameList(this.listView_calendar.SelectedItems);

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除日历事项 " + strItemNameList + "?",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<ListViewItem> delete_items = new List<ListViewItem>();
            bool bChanged = false;
            foreach (ListViewItem item in this.listView_calendar.SelectedItems)
            {
                // 如果本来就是拟新增的事项，删除它即可
                if (item.ImageIndex == ITEMTYPE_NEW)
                    delete_items.Add(item);
                else if (item.ImageIndex != ITEMTYPE_DELETED)
                {
                    SetCalendarItemState(item, ITEMTYPE_DELETED);
                    bChanged = true;
                }
            }

            foreach (ListViewItem item in delete_items)
            {
                this.listView_calendar.Items.Remove(item);
            }

            if (bChanged == true)
            {
                this.CalendarDefChanged = true;
                MessageBox.Show(this, "此时删除的日历事项还会显示在列表中。直到保存的时候，删除操作才会提交到服务器");
            }

            m_nCalendarVersion++;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_calendar_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = SaveCalendars(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_calendar_refresh_Click(object sender, EventArgs e)
        {
            this.toolStripButton_calendar_refresh.Enabled = false;
            try
            {
                string strError = "";
                int nRet = ListCalendars(out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            ERROR1:
                MessageBox.Show(this, strError);
            }
            finally
            {
                this.toolStripButton_calendar_refresh.Enabled = true;
            }
        }

        private void listView_calendar_DoubleClick(object sender, EventArgs e)
        {
            toolStripButton_calendar_modify_Click(sender, e);
        }



        private void listView_calendar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_calendar.SelectedItems.Count > 0)
            {
                this.toolStripButton_calendar_modify.Enabled = true;
                this.toolStripButton_calendar_delete.Enabled = true;
            }
            else
            {
                this.toolStripButton_calendar_modify.Enabled = false;
                this.toolStripButton_calendar_delete.Enabled = false;
            }
        }

        bool _kernelInitialized = false;

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_kernel
                && _kernelInitialized == false)
            {
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.115") < 0)
                {
                    this.ShowMessage("内核管理功能要求当前连接的 dp2library 服务器版本为 2.115 或以上 (而现在是 " + Program.MainForm.ServerVersion + ")", "red", true);
                    return;
                }

                if (this.kernelResTree1.Fill() == true)
                    _kernelInitialized = true;
            }

            if (this.tabControl_main.SelectedTab == this.tabPage_newLoanPolicy)
            {
                // 同步日历名列表
                if (m_nCalendarVersion > 0)
                {
                    List<string> calendar_Names = null;
                    string strError = "";
                    // 装入全部日历名
                    int nRet = GetCalendarNames(out calendar_Names,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                    this.loanPolicyControlWrapper1.LoanPolicyControl.CalendarList = calendar_Names;
                    m_nCalendarVersion = 0;
                }
            }
        }

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();


        private void listView_calendar_ColumnClick(object sender, ColumnClickEventArgs e)
        {
#if NO
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_calendar.Columns,
                true);

            // 排序
            this.listView_calendar.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);
            // this.listView_calendar.ListViewItemSorter = null;
#endif
            ListViewUtil.OnColumnClick(this.listView_calendar, e, false);
        }

        #endregion

        private void kernelResTree1_GetChannel(object sender, DigitalPlatform.LibraryClient.GetChannelEventArgs e)
        {
            e.Channel = Program.MainForm.GetChannel();
            if (e.BeginLoop == true)
            {
                this.ShowMessage("正在访问服务器 ...");
                stop.OnStop += new StopEventHandler(this.DoStop);
                // stop.Initial(" ...");
                stop.BeginLoop();
            }
        }

        private void kernelResTree1_ReturnChannel(object sender, DigitalPlatform.LibraryClient.ReturnChannelEventArgs e)
        {
            Program.MainForm.ReturnChannel(e.Channel);
            if (e.EndLoop == true)
            {
                if (stop != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
                this.ClearMessage();
            }
        }


    }

        #endregion // 查重
}