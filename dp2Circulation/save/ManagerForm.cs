using System;
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

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    public partial class ManagerForm : Form
    {
        const int TYPE_NSTABLE = 0;
        const int TYPE_GROUP = 1;
        const int TYPE_DATABASE = 2;
        const int TYPE_ERROR = 3;


        int m_nRightsTableXmlVersion = 0;
        int m_nRightsTableHtmlVersion = 0;

        // 表示当前全部数据库信息的XML字符串
        public string AllDatabaseInfoXml = "";

        const int WM_INITIAL = API.WM_USER + 201;

        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;

        string [] type_names = new string[] {
            "biblio","书目",
            "entity","实体",
            "order","订购",
            "issue","期",
            "reader","读者",
            "message","消息",
            "arrived","预约到书",
            "amerce","违约金",
            "publisher","出版者",
            "zhongcihao","种次号",
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
        public string GetTypeName(string strTypeString)
        {
            for (int i = 0; i < type_names.Length / 2; i++)
            {
                if (type_names[i * 2] == strTypeString)
                    return type_names[i * 2+1];
            }

            return null;    // not found
        }

        public ManagerForm()
        {
            InitializeComponent();
        }

        private void ManagerForm_Load(object sender, EventArgs e)
        {
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            API.PostMessage(this.Handle, WM_INITIAL, 0, 0);

            this.listView_opacDatabases.SmallImageList = this.imageList_opacDatabaseType;
            this.listView_opacDatabases.LargeImageList = this.imageList_opacDatabaseType;

            this.listView_databases.SmallImageList = this.imageList_opacDatabaseType;
            this.listView_databases.LargeImageList = this.imageList_opacDatabaseType;

            this.treeView_opacBrowseFormats.ImageList = this.imageList_opacBrowseFormatType;

            this.treeView_zhongcihao.ImageList = this.imageList_zhongcihao;
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
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

                        nRet = this.ListRightsTables(out strError);
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

                        // 列出脚本
                        nRet = this.ListScript(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

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
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }

            }

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
                    "当前窗口内读者流通权限定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    this.tabControl_main.SelectedTab = this.tabPage_loanPolicy;
                    return;
                }
            }

            if (this.LocationTypesDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有馆藏地点定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
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
                    "当前窗口内有脚本定义被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
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
        }

        private void ManagerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null)
            {
                stop.Unregister(); // 脱离关联
                stop = null;
            }

        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        /*
        private void button_clearAllDbs_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = ClearAllDbs(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "OK");
        }*/

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
            this.MainForm.Update();

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

        void EnableControls(bool bEnable)
        {
            // this.button_clearAllDbs.Enabled = bEnable;
            this.toolStrip_databases.Enabled = bEnable;
        }

        private void ManagerForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
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

                // 2008/7/2 new add
                // 空的名字将被忽略
                if (String.IsNullOrEmpty(strName) == true)
                    continue;

                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strTypeName);
                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_databases.Items.Add(item);
            }

            return 0;
        }

        // 确定一个数据库是不是书目库类型?
        bool IsDatabaseBiblioType(string strDatabaseName)
        {
            for (int i = 0; i < this.listView_databases.Items.Count; i++)
            {
                ListViewItem item = this.listView_databases.Items[i];
                string strName = item.Text;
                if (strName == strDatabaseName)
                {
                    string strTypeName = ListViewUtil.GetItemText(item, 1);
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
            this.MainForm.Update();

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
            }
            else
            {
                this.toolStripButton_modifyDatabase.Enabled = false;
                this.toolStripButton_deleteDatabase.Enabled = false;
                this.toolStripButton_initializeDatabase.Enabled = false;
            }
        }

        // 创建书目库
        private void ToolStripMenuItem_createBiblioDatabase_Click(object sender, EventArgs e)
        {

            BiblioDatabaseDialog dlg = new BiblioDatabaseDialog();

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
            this.MainForm.StartPrepareNames(false);
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

        // 创建数据库
        public int CreateDatabase(
            string strDatabaseInfo,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建数据库 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "create",
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

        // 删除数据库
        public int DeleteDatabase(
            string strDatabaseNames,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除数据库 "+strDatabaseNames+"...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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

        // 初始化数据库
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
            this.MainForm.Update();

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

        // 修改数据库
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
            this.MainForm.Update();

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

            string strDbNameList = "";
            for (int i = 0; i < this.listView_databases.SelectedItems.Count; i++)
            {
                if (i > 0)
                    strDbNameList += ",";
                strDbNameList += this.listView_databases.SelectedItems[i].Text;
            }

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除数据库 "+strDbNameList+"?\r\n\r\n警告：数据库一旦被删除后，其内的数据记录将全部丢失，并再也无法复原",
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

                // 重新获得各种库名、列表
                this.MainForm.StartPrepareNames(false);
            }
            finally
            {
                EnableControls(true);
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
            string strTypeName = ListViewUtil.GetItemText(item, 1);
            string strName = item.Text;

            string strType = GetTypeString(strTypeName);
            if (strType == null)
                strType = strTypeName;

            if (strType == "biblio")
            {
                BiblioDatabaseDialog dlg = new BiblioDatabaseDialog();

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
                this.MainForm.StartPrepareNames(false);
            }
            else if (strType == "reader")
            {
                ReaderDatabaseDialog dlg = new ReaderDatabaseDialog();

                dlg.Text = "修改读者库特性";
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
                SelectDatabaseLine(dlg.ReaderDatabaseName);

                // 重新获得各种库名、列表
                this.MainForm.StartPrepareNames(false);

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
                this.MainForm.StartPrepareNames(false);
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
                strType = ListViewUtil.GetItemText(this.listView_databases.SelectedItems[0], 1);
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

            menuItem = new MenuItem("创建书目库(&B)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createBiblioDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建读者库(&V)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createReaderDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建违约金库(&A)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createAmerceDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建预约到书库(&R)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createArrivedDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建出版者库(&P)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createPublisherDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建消息库(&M)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createMessageDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建种次号库(&Z)");
            menuItem.Click += new System.EventHandler(this.ToolStripMenuItem_createZhongcihaoDatabase_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            string strText = "";
            if (this.listView_databases.SelectedItems.Count == 1)
                strText = "删除" + strType + "库 '" + strName + "'(&D)";
            else
                strText = "删除所选 " + this.listView_databases.SelectedItems.Count.ToString() + " 个OPAC数据库(&D)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.toolStripButton_deleteDatabase_Click);
            if (this.listView_databases.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("观察所选 "+this.listView_databases.SelectedItems.Count.ToString()+" 个数据库的定义(&D)");
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

            for (int i = 0; i < this.listView_databases.SelectedItems.Count; i++)
            {
                ListViewItem item = this.listView_databases.SelectedItems[i];
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
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_viewXml_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);
            return;
        }

        // 创建读者库
        private void ToolStripMenuItem_createReaderDatabase_Click(object sender, EventArgs e)
        {
            ReaderDatabaseDialog dlg = new ReaderDatabaseDialog();

            dlg.Text = "创建新读者库";
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
            SelectDatabaseLine(dlg.ReaderDatabaseName);

            // 重新获得各种库名、列表
            this.MainForm.StartPrepareNames(false);
        }

        // 创建违约金库
        private void ToolStripMenuItem_createAmerceDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("amerce", "", "");
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
            this.MainForm.StartPrepareNames(false);
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

        private void ToolStripMenuItem_createMessageDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("message", "", "");
        }

        private void ToolStripMenuItem_createZhongcihaoDatabase_Click(object sender, EventArgs e)
        {
            CreateSimpleDatabase("zhongcihao", "", "");
        }

        // 刷新数据库名列表
        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            RefreshDatabaseList();
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
        int ConfirmLogin(out string strError)
        {
            strError = "";

            ConfirmSupervisorDialog login_dlg = new ConfirmSupervisorDialog();
            login_dlg.UserName = this.MainForm.AppInfo.GetString(
                    "default_account",
                    "username",
                    "");
            login_dlg.ServerUrl = this.MainForm.LibraryServerUrl;
            login_dlg.Comment = "重要操作前，需要验证您的身份";

            login_dlg.StartPosition = FormStartPosition.CenterScreen;
            login_dlg.ShowDialog(this);

            if (login_dlg.DialogResult != DialogResult.OK)
                return 0;

            string strLocation = this.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");


            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            long lRet = this.Channel.Login(login_dlg.UserName,
                login_dlg.Password,
                strLocation,
                false,
                out strError);
            if (lRet == -1)
                return -1;


            if (lRet == 0)
            {
                // strError = "";
                return -1;
            }

            return 1;
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

            string strDbNameList = "";
            for (int i = 0; i < this.listView_databases.SelectedItems.Count; i++)
            {
                if (i > 0)
                    strDbNameList += ",";
                strDbNameList += this.listView_databases.SelectedItems[i].Text;
            }

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要初始化数据库 " + strDbNameList + "?\r\n\r\n警告：1) 数据库一旦被初始化后，其内的数据记录将全部丢失，并再也无法复原。\r\n      2) 如果初始化的是书目库，则书目库从属的实体库、订购库、期库也会一并被初始化。",
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
            <caption lang="zh-cn">中文书刊</caption>
            <caption lang="en">Chinese Books and Series</caption>
            <from style="title">
                <caption lang="zh-cn">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from style="author">
                <caption lang="zh-cn">著者</caption>
                <caption lang="en">Author</caption>
            </from>
            <database name="中文图书" />
            <database name="中文期刊" />
        </virtualDatabase>
        <database name="用户">
            <caption lang="zh-cn">用户</caption>
            <caption lang="en">account</caption>
            <from name="用户名">
                <caption lang="zh-cn">用户名</caption>
                <caption lang="en">username</caption>
            </from>
            <from name="__id" />
        </database>
        <database name="中文图书">
            <caption lang="zh-cn">中文图书</caption>
            <caption lang="en">Chinese book</caption>
            <from name="ISBN">
                <caption lang="zh-cn">ISBN</caption>
                <caption lang="en">ISBN</caption>
            </from>
            <from name="ISSN">
                <caption lang="zh-cn">ISSN</caption>
                <caption lang="en">ISSN</caption>
            </from>
            <from name="题名">
                <caption lang="zh-cn">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from name="题名拼音">
                <caption lang="zh-cn">题名拼音</caption>
                <caption lang="en">Title pinyin</caption>
            </from>
            <from name="主题词">
                <caption lang="zh-cn">主题词</caption>
                <caption lang="en">Thesaurus</caption>
            </from>
            <from name="关键词">
                <caption lang="zh-cn">关键词</caption>
                <caption lang="en">Keyword</caption>
            </from>
            <from name="分类号">
                <caption lang="zh-cn">分类号</caption>
                <caption lang="en">Class number</caption>
            </from>
            <from name="责任者">
                <caption lang="zh-cn">责任者</caption>
                <caption lang="en">Contributor</caption>
            </from>
            <from name="责任者拼音">
                <caption lang="zh-cn">责任者拼音</caption>
                <caption lang="en">Contributor pinyin</caption>
            </from>
            <from name="出版者">
                <caption lang="zh-cn">出版者</caption>
                <caption lang="en">Publisher</caption>
            </from>
            <from name="索书号">
                <caption lang="zh">索书号</caption>
                <caption lang="en">Call number</caption>
            </from>
            <from name="收藏单位">
                <caption lang="zh-cn">收藏单位</caption>
                <caption lang="en">Rights holder</caption>
            </from>
            <from name="索书类号">
                <caption lang="zh">索书类号</caption>
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
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];


                string strName = DomUtil.GetAttr(node, "name");
                string strType = node.Name;

                // 对于<virtualDatabase>元素，要选出<caption>里面的中文名称
                if (node.Name == "virtualDatabase")
                    strName = DomUtil.GetCaption("zh", node);

                int nImageIndex = 0;
                if (strType == "virtualDatabase")
                    nImageIndex = 1;

                ListViewItem item = new ListViewItem(strName, nImageIndex);
                item.SubItems.Add(GetOpacDatabaseTypeDisplayString(strType));
                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_opacDatabases.Items.Add(item);
            }

            return 0;
        }

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
            stop.Initial("正在获取全部OPAC数据库定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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
            this.MainForm.Update();

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

        // 获得普通数据库定义
        public int GetDatabaseInfo(
            string strDbName,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取数据库 "+strDbName+" 的定义...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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

            OpacVirtualDatabaseDialog dlg = new OpacVirtualDatabaseDialog();

            dlg.Text = "新增虚拟库定义";
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
            this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacVirtualDatabaseDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

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
        public int DetectVirtualDatabaseNameDup(string strCaptionsXml,
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
                                strError = "语言代码 '" + strOneLang + "' 下的虚拟库名 '" + strOneName + "' 和当前已经存在的列表中第 " + (j + 1).ToString() + " 行的语言 '"+strExistLang+"' 下的虚拟库名 '"+strExistName+"' 发生了重复";
                                return 1;
                            }
                        }
                    }
                    else if (strType == "database")
                    {
                        if (strName == strOneName)
                        {
                            strError = "语言代码 '" + strOneLang + "' 下的虚拟库名 '" + strOneName + "' 和当前已经存在的普通库名(列表中第 "+(j+1).ToString()+" 行)发生了重复";
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

            Debug.Assert(index >= 0 && index <= this.listView_opacDatabases.Items.Count - 1,"");

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

            for (int i = 0; i < this.listView_opacDatabases.SelectedItems.Count; i++)
            {
                ListViewItem item = this.listView_opacDatabases.SelectedItems[i];
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
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strXml;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_viewXml_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

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

                OpacVirtualDatabaseDialog dlg = new OpacVirtualDatabaseDialog();

                dlg.Text = "修改虚拟库定义";
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
                this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacVirtualDatabaseDialog_state");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);


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

                string strXml = (string)item.Tag;

                XmlDocument dom = new XmlDocument();
                try {
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
                this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacNormalDatabaseDialog_state");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);


                if (dlg.DialogResult != DialogResult.OK)
                    return;

                DomUtil.SetAttr(dom.DocumentElement, "name", dlg.DatabaseName);

                item.Text = dlg.DatabaseName;
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
            for (int i = 0; i < this.listView_opacDatabases.Items.Count; i++)
            {
                existing_dbnames.Add(this.listView_opacDatabases.Items[i].Text);
            }

            OpacNormalDatabaseDialog dlg = new OpacNormalDatabaseDialog();

            dlg.Text = "新增普通库定义";
            dlg.ManagerForm = this;
            dlg.ExcludingDbNames = existing_dbnames;

            this.MainForm.AppInfo.LinkFormState(dlg, "ManagerForm_OpacNormalDatabaseDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<database name='' />");


            // 从<virtualDatabase>元素下的若干<caption>中，选出符合当前工作语言的一个名字字符串
            // 从一个元素的下级<caption>元素中, 提取语言符合的文字值
            string strName = dlg.DatabaseName;
            string strType = "database";

            DomUtil.SetAttr(dom.DocumentElement, "name", strName);

            ListViewItem item = new ListViewItem(strName, 0);
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

            // 选定刚刚插入的普通库
            item.Selected = true;
            this.listView_opacDatabases.FocusedItem = item;

            // 如果是书目库，看看这个数据库的显示格式定义是否已经存在？
            // 如果不存在，提示插入建议
            if (IsDatabaseBiblioType(strName) == true
                && HasBrowseFormatDatabaseExist(strName) == false)
            {
                DialogResult result = MessageBox.Show(this,
                    "刚新增的书目库 "+strName+" 还没有OPAC记录显示格式定义。\r\n\r\n要自动给它创建常规的OPAC记录显示格式定义么? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                            // 为书目库插入OPAC显示格式节点(后插)
                    nRet = NewBiblioOpacBrowseFormat(strName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
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

            string strDbNameList = "";
            for (int i = 0; i < this.listView_opacDatabases.SelectedItems.Count; i++)
            {
                if (i > 0)
                    strDbNameList += ",";
                strDbNameList += this.listView_opacDatabases.SelectedItems[i].Text;
            }

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
            RefreshOpacDatabaseList();
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
            int nRet = ClearAllDbs(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, "OK");
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

                    string strFormatName = DomUtil.GetAttr(format_node, "name");
                    string strType = DomUtil.GetAttr(format_node, "type");
                    string strScriptFile = DomUtil.GetAttr(format_node, "scriptfile");

                    string strDisplayText = strFormatName;
                    
                    if (String.IsNullOrEmpty(strType) == false)
                        strDisplayText += " type=" + strType;

                    if (String.IsNullOrEmpty(strScriptFile) == false)
                        strDisplayText += " scriptfile=" + strScriptFile;

                    TreeNode format_treenode = new TreeNode(strDisplayText, 1, 1);
                    format_treenode.Tag = format_node.OuterXml;

                    database_treenode.Nodes.Add(format_treenode);
                }
            }

            this.treeView_opacBrowseFormats.ExpandAll();

            return 0;

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
            this.MainForm.Update();

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
            this.MainForm.Update();

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

            // TODO: 如果数据库为书目库，则type应当预设为"biblio"
            dlg.Text = "请指定显示格式的属性";
            dlg.FormatName = "";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<format />");

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


            TreeNode new_treenode = new TreeNode(strDisplayText, 1, 1);
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


                // 新的显示格式
                OpacBrowseFormatDialog dlg = new OpacBrowseFormatDialog();

                dlg.Text = "请指定显示格式的属性";
                dlg.FormatName = DomUtil.GetAttr(dom.DocumentElement, "name");
                dlg.FormatType = DomUtil.GetAttr(dom.DocumentElement, "type");
                dlg.ScriptFile = DomUtil.GetAttr(dom.DocumentElement, "scriptfile");
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;


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
            RefreshOpacBrowseFormatTree();
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

        int ListRightsTables(out string strError)
        {
            strError = "";

            if (this.LoanPolicyDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内读者流通权限定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            /*
            // 2008/10/12 new add
            if (this.LoanPolicyDefChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有读者流通权限定义被修改后尚未保存。若此时重新装载读者流通权限定义，现有未保存信息将丢失。\r\n\r\n确实要重新装载? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }*/

            string strRightsTableXml = "";
            string strRightsTableHtml = "";

            // 获得流通读者权限相关定义
            int nRet = GetRightsTableInfo(out strRightsTableXml,
                out strRightsTableHtml,
                out strError);
            if (nRet == -1)
                return -1;

            strRightsTableXml = "<rightstable>" + strRightsTableXml + "</rightstable>";

            string strXml = "";
            nRet = DomUtil.GetIndentXml(strRightsTableXml,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.textBox_loanPolicy_rightsTableDef.Text = strXml;
            Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                strRightsTableHtml);

            this.LoanPolicyDefChanged = false;

            this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;

            return 1;
        }

        // 获得流通读者权限相关定义
        int GetRightsTableInfo(out string strRightsTableXml,
            out string strRightsTableHtml,
            out string strError)
        {
            strError = "";
            strRightsTableXml = "";
            strRightsTableHtml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取读者流通权限定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTable",
                    out strRightsTableXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTableHtml",
                    out strRightsTableHtml,
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

        // 保存流通读者权限相关定义
        // parameters:
        //      strRightsTableXml   流通读者权限定义XML。注意，没有根元素
        int SetRightsTableDef(string strRightsTableXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者流通权限定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "rightsTable",
                    strRightsTableXml,
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

        // 同步读者权限XML定义和流通权限表HTML显示
        int SynchronizeRightsTableAndHtml()
        {
            string strError = "";

            if (this.m_nRightsTableXmlVersion == this.m_nRightsTableHtmlVersion)
                return 0;


            string strRightsTableXml = this.textBox_loanPolicy_rightsTableDef.Text;
            string strRightsTableHtml = "";

            if (String.IsNullOrEmpty(strRightsTableXml) == true)
            {
                Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                    "<p>(blank)</p>");
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

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
                Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                    "<p>(blank)</p>");
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;
                return 0;
            }

            strRightsTableXml = dom.DocumentElement.InnerXml;

            // EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取读者流通权限定义HTML ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "instance_rightstable_html",
                    strRightsTableXml,
                    out strRightsTableHtml,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }

                Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                    strRightsTableHtml);
                this.m_nRightsTableHtmlVersion = this.m_nRightsTableXmlVersion;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                // EnableControls(true);
            }

        ERROR1:
            Global.SetHtmlString(this.webBrowser_rightsTableHtml,
                HttpUtility.HtmlEncode(strError));
            return -1;
        }


        bool m_bLoanPolicyDefChanged = false;

        public bool LoanPolicyDefChanged
        {
            get
            {
                return this.m_bLoanPolicyDefChanged;
            }
            set
            {
                this.m_bLoanPolicyDefChanged = value;
                if (value == true)
                    this.toolStripButton_loanPolicy_save.Enabled = true;
                else
                    this.toolStripButton_loanPolicy_save.Enabled = false;
            }
        }

        private void textBox_loanPolicy_rightsTableDef_TextChanged(object sender, EventArgs e)
        {
            // XML编辑器中的版本发生变化
            this.m_nRightsTableXmlVersion++;
            this.LoanPolicyDefChanged = true;
        }

        private void textBox_loanPolicy_rightsTableDef_Enter(object sender, EventArgs e)
        {
            SynchronizeRightsTableAndHtml();
        }

        private void textBox_loanPolicy_rightsTableDef_Leave(object sender, EventArgs e)
        {
            SynchronizeRightsTableAndHtml();
        }

        private void toolStripButton_loanPolicy_save_Click(object sender, EventArgs e)
        {
            string strError = "";
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
                    strRightsTableXml = "";
                }
                else
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

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_loanPolicy_refresh_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = this.ListRightsTables(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

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
            <locationtypes>
                <item canborrow="yes">流通库</item>
                <item>阅览室</item>
            </locationtypes>
            */

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
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

                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                ListViewItem item = new ListViewItem(strText, 0);
                item.SubItems.Add(bCanBorrow == true ? "是" : "否");

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
            stop.Initial("正在获取<locationtypes>配置 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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

        // 修改/设置全部馆藏地定义<locationtypes>
        int SetAllLocationTypesInfo(string strLocationDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置<locationtypes>定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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

        // 构造<locationtypes>定义的XML片段
        // 注意是下级片断定义，没有<locationtypes>元素作为根。
        int BuildLocationTypesDef(out string strLocationDef,
            out string strError)
        {
            strError = "";
            strLocationDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<locationtypes />");

            for (int i = 0; i < this.listView_location_list.Items.Count; i++)
            {
                ListViewItem item = this.listView_location_list.Items[i];
                string strText = item.Text;
                string strCanBorrow = ListViewUtil.GetItemText(item, 1);

                bool bCanBorrow = false;

                if (strCanBorrow == "是" || strCanBorrow == "yes")
                    bCanBorrow = true;

                XmlNode nodeItem = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(nodeItem);

                nodeItem.InnerText = strText;
                DomUtil.SetAttr(nodeItem, "canborrow", bCanBorrow == true ? "yes" : "no");
            }

            strLocationDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 提交<locationtypes>定义修改
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

            return 0;
        }


        private void toolStripButton_location_refresh_Click(object sender, EventArgs e)
        {
            // 在listview中列出所有馆藏地
            string strError = "";
            int nRet = ListAllLocations(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        bool m_bLocationTypesDefChanged = false;

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

        // 新创建馆藏地点事项
        private void toolStripButton_location_new_Click(object sender, EventArgs e)
        {
            LocationItemDialog dlg = new LocationItemDialog();

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewItem item = new ListViewItem(dlg.LocationString, 0);
            item.SubItems.Add(dlg.CanBorrow == true ? "是" : "否");

            this.listView_location_list.Items.Add(item);
            ListViewUtil.SelectLine(item, true);

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

            dlg.LocationString = ListViewUtil.GetItemText(item, 0);
            dlg.CanBorrow = (ListViewUtil.GetItemText(item, 1) == "是") ? true : false;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewUtil.ChangeItemText(item, 0, dlg.LocationString);
            ListViewUtil.ChangeItemText(item, 1, dlg.CanBorrow == true ? "是" : "否");

            ListViewUtil.SelectLine(item, true);
            this.LocationTypesDefChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
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

            string strItemNameList = "";
            for (int i = 0; i < this.listView_location_list.SelectedItems.Count; i++)
            {
                if (i > 0)
                    strItemNameList += ",";
                strItemNameList += this.listView_location_list.SelectedItems[i].Text;
            }

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除馆藏地点事项 " + strItemNameList + "?",
                "ManagerForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            for (int i = this.listView_location_list.SelectedIndices.Count - 1;
                i >= 0;
                i--)
            {
                int index = this.listView_location_list.SelectedIndices[i];
                string strDatabaseName = this.listView_location_list.Items[index].Text;
                this.listView_location_list.Items.RemoveAt(index);
            }

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

            string strName = "";
            string strCanBorrow = "";
            if (this.listView_location_list.SelectedItems.Count > 0)
            {
                strName = this.listView_location_list.SelectedItems[0].Text;
                strCanBorrow = ListViewUtil.GetItemText(this.listView_location_list.SelectedItems[0], 1);
            }


            // 修改馆藏事项
            {
                menuItem = new MenuItem("修改 " + strName + "(&M)");
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
                strText = "删除 " + strName + "(&D)";
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
            this.MainForm.Update();

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
        //      strRightsTableXml   脚本定义XML。注意，没有根元素
        int SetScriptDef(string strScriptXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存脚本定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

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
            string strError = "";

            int nRet = this.ListScript(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
        }

        private void textBox_script_TextChanged(object sender, EventArgs e)
        {
            this.ScriptChanged = true;
        }

        private void textBox_script_comment_DoubleClick(object sender, EventArgs e)
        {
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

        int ListZhongcihao(out string strError)
        {
            strError = "";

            /*
            if (this.ZhongcihaoChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内种次号定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }*/

            this.treeView_zhongcihao.Nodes.Clear();


            string strZhongcihaoXml = "";

            // 获得种次号相关定义
            int nRet = GetZhongcihaoInfo(out strZhongcihaoXml,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<zhogncihao />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strZhongcihaoXml;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
    <zhongcihao>
        <nstable name="nstable">
            <item prefix="marc" uri="http://dp2003.com/UNIMARC" />
        </nstable>
        <group name="中文书目" zhongcihaodb="种次号">
            <database name="中文图书" leftfrom="索书类号" 

rightxpath="//marc:record/marc:datafield[@tag='905']/marc:subfield[@code='e']/text()" 

titlexpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='a']/text()" 

authorxpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f' or @code='g']/text()" 

/>
        </group>
    </zhongcihao>
 * */
            XmlNodeList nstable_nodes = dom.DocumentElement.SelectNodes("nstable");
            for (int i = 0; i < nstable_nodes.Count; i++)
            {
                XmlNode node = nstable_nodes[i];

                string strNstableName = DomUtil.GetAttr(node, "name");

                TreeNode nstable_treenode = new TreeNode(strNstableName,
                    TYPE_NSTABLE, TYPE_NSTABLE);
                nstable_treenode.Tag = node.OuterXml;

                this.treeView_zhongcihao.Nodes.Add(nstable_treenode);
            }

            XmlNodeList group_nodes = dom.DocumentElement.SelectNodes("group");
            for (int i = 0; i < group_nodes.Count; i++)
            {
                XmlNode node = group_nodes[i];

                string strGroupName = DomUtil.GetAttr(node, "name");
                string strZhongcihaoDbName = DomUtil.GetAttr(node, "zhongcihaodb");

                TreeNode group_treenode = new TreeNode(strGroupName + " 种次号库='" + strZhongcihaoDbName + "'",
                    TYPE_GROUP, TYPE_GROUP);
                group_treenode.Tag = node.OuterXml;

                this.treeView_zhongcihao.Nodes.Add(group_treenode);

                // 加入database节点
                XmlNodeList database_nodes = node.SelectNodes("format");
                for (int j = 0; j < database_nodes.Count; j++)
                {
                    XmlNode database_node = database_nodes[j];

                    string strDatabaseName = DomUtil.GetAttr(database_node, "name");

                    string strDisplayText = strDatabaseName;

                    TreeNode database_treenode = new TreeNode(strDisplayText,
                        TYPE_DATABASE, TYPE_DATABASE);
                    database_treenode.Tag = database_node.OuterXml;

                    group_treenode.Nodes.Add(database_treenode);
                }
            }

            this.treeView_zhongcihao.ExpandAll();
            // this.ZhongcihaoChanged = false;

            return 1;
        }

        // 获得种次号相关定义
        int GetZhongcihaoInfo(out string strZhongcihaoXml,
            out string strError)
        {
            strError = "";
            strZhongcihaoXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取种次号定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "zhongcihao",
                    out strZhongcihaoXml,
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

        // 保存种次号定义
        // parameters:
        //      strZhongcihaoXml   脚本定义XML。注意，没有根元素
        int SetZhongcihaoDef(string strZhongcihaoXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存种次号定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "zhongcihao",
                    strZhongcihaoXml,
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

        #endregion

        private void treeView_zhongcihao_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView_zhongcihao_MouseUp(object sender, MouseEventArgs e)
        {

        }

        // 插入<group>类型节点
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

            // 询问<group>名
            ZhongcihaoGroupDialog dlg = new ZhongcihaoGroupDialog();

            dlg.Text = "请指定组特性";
            dlg.AllZhongcihaoDatabaseInfoXml = GetAllZhongcihaoDbInfoXml();
            dlg.ExcludingDbNames = GetAllUsedZhongcihaoDbName();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

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
                string strComment = "种次号库 '" + dlg.ZhongcihaoDbName + "' 尚未创建。请创建它。";
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

            TreeNode new_treenode = new TreeNode(dlg.GroupName, TYPE_GROUP, TYPE_GROUP);
            new_treenode.Tag = dom.OuterXml;
            this.treeView_zhongcihao.Nodes.Insert(index, new_treenode);

            this.treeView_zhongcihao.SelectedNode = new_treenode;

            /*
            // 需要立即向服务器提交修改
            nRet = SubmitZhongcihaoDef(out strError);
            if (nRet == -1)
            {
                new_treenode.ImageIndex = TYPE_ERROR;    // 表示未能提交的新插入节点请求
                goto ERROR1;
            }*/

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检查指定名字的种次号库是否已经创建
        // return:
        //      -2  所指定的种次号库名字，实际上是一个已经存在的其他类型的库名
        //      -1  error
        //      0   还没有创建
        //      1   已经创建
        int CheckZhongcihaoDbCreated(string strZhongcihaoDbName,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                strError = "参数strZhongcihaoDbName的值不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
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

                if ("zhongcihao" == strType)
                {
                    if (strName == strZhongcihaoDbName)
                        return 1;
                }

                if (strType == "biblio")
                {
                    if (strName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的小书目库名 '" + strName + "' 相重了";
                        return -2;
                    }

                    string strEntityDbName = DomUtil.GetAttr(node, "entityDbName");
                    if (strEntityDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的实体库名 '" + strEntityDbName + "' 相重了";
                        return -2;
                    }

                    string strOrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    if (strOrderDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的订购库名 '" + strOrderDbName + "' 相重了";
                        return -2;
                    }

                    string strIssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    if (strIssueDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的期库名 '" + strIssueDbName + "' 相重了";
                        return -2;
                    }

                }

                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;

                if (strName == strZhongcihaoDbName)
                {
                    strError = "所拟定的种次号库名和当前已经存在的" + strTypeName + "库名 '" + strName + "' 相重了";
                    return -2;
                }

            }

            return 0;
        }

        string GetAllZhongcihaoDbInfoXml()
        {
            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
                return null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                // strError = "XML装入DOM时出错: " + ex.Message;
                // return -1;
                Debug.Assert(false, "");
                return "";
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if (StringUtil.IsInList("zhongcihao", strType) == true)
                    continue;

                node.ParentNode.RemoveChild(node);
            }

            return dom.OuterXml;
        }

        // 获得treeview中已经使用过的全部种次号名
        List<string> GetAllUsedZhongcihaoDbName()
        {
            List<string> existing_dbnames = new List<string>();
            for (int i = 0; i < this.treeView_zhongcihao.Nodes.Count; i++)
            {
                TreeNode tree_node = this.treeView_zhongcihao.Nodes[i];
                if (tree_node.ImageIndex != TYPE_GROUP)
                    continue;

                string strXml = (string)tree_node.Tag;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                string strZhongcihaoDbName = DomUtil.GetAttr(dom.DocumentElement, "zhongcihaodb");

                if (String.IsNullOrEmpty(strZhongcihaoDbName) == false)
                    existing_dbnames.Add(strZhongcihaoDbName);
            }

            return existing_dbnames;
        }

        private void toolStripMenuItem_zhongcihao_insert_database_Click(object sender, EventArgs e)
        {

        }

        private void ToolStripMenuItem_zhongcihao_insert_nstable_Click(object sender, EventArgs e)
        {

        }




    }
}