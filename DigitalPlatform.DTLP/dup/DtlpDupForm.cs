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

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace DigitalPlatform.DTLP
{
  
    public partial class DtlpDupForm : Form
    {
        DigitalPlatform.StopManager stopManager = null; // 引用
        DigitalPlatform.Stop stop = null;   // 独立使用

        public event LoadDetailEventHandler LoadDetail = null;

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        bool m_bInSearch = false;
        public bool AutoBeginSearch = false;

        public string CfgFilename = ""; // dup.xml配置文件名
        XmlDocument dom = null;

        XmlNode m_nodeProject = null;   // 当前关注的<project>元素节点

        public DtlpChannel DtlpChannel = null;

        const int WM_INITIAL = API.WM_USER + 201;

        const int ITEMTYPE_NORMAL = 0;  // 普通事项
        const int ITEMTYPE_OVERTHRESHOLD = 1; // 权值超过阈值的事项

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        string m_strMarcRecord = "";

        // MARC记录
        public string MarcRecord
        {
            get
            {
                return this.m_strMarcRecord;
            }
            set
            {
                this.m_strMarcRecord = value;
            }
        }

        public string ProjectName
        {
            get
            {
                return this.comboBox_projectName.Text;
            }
            set
            {
                this.comboBox_projectName.Text = value;
            }
        }

        // 记录路径。包含服务器名部分
        public string RecordPath
        {
            get
            {
                return this.textBox_recordPath.Text;
            }
            set
            {
                this.textBox_recordPath.Text = value;

                this.Text = "DTLP查重: " + value;
            }
        }

        public DtlpDupForm()
        {
            InitializeComponent();
        }

        // return:
        //      -1  出错
        //      0   配置文件不存在
        //      1   成功
        public int Initial(string strCfgFilename,
            StopManager stopManager,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();
            try
            {
                this.dom.Load(strCfgFilename);
                this.CfgFilename = strCfgFilename;
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "配置文件 '" + strCfgFilename + "' 不存在";
                this.dom.LoadXml("<root />");   // 便于后面建立新内容
                this.CfgFilename = strCfgFilename;
                return 0;
            }
            catch (Exception ex)
            {
                strError = "配置文件 '" + strCfgFilename + "' 装载到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            this.stopManager = stopManager;

            return 1;
        }

        private void DupForm_Load(object sender, EventArgs e)
        {
            Debug.Assert(this.dom != null, "尚未Initial()");

            stop = new DigitalPlatform.Stop();
            if (this.stopManager != null)
                stop.Register(this.stopManager, true);	// 和容器关联

                        // 自动启动查重
            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        string strError = "";
                        int nRet = DoSearch(out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        return;
                    ERROR1:
                        MessageBox.Show(this, strError);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }


        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = DoSearch(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 析出路径中的servername部分
        static string GetServerName(string strPath)
        {
            string[] parts = strPath.Split(new char [] {'/'} );

            if (parts.Length == 0)
                return "";

            Debug.Assert(parts.Length >= 1, "");

            return parts[0];
        }

        void EnableControls(bool bEnable)
        {
            this.button_search.Enabled = bEnable;

            this.button_viewMarcRecord.Enabled = bEnable;

            this.comboBox_projectName.Enabled = bEnable;
            this.textBox_recordPath.Enabled = bEnable;

            // this.textBox_serverName.Enabled = bEnable;
            // this.button_findServerName.Enabled = bEnable;
        }

        int DoSearch(out string strError)
        {
            strError = "";

            /*
            string strServerName = "";

            strServerName = this.textBox_serverName.Text;

            if (String.IsNullOrEmpty(strServerName) == true)
            {
                // 如果servername text为空，则从发起路径中获得servername
                strServerName = GetServerName(this.RecordPath);
                if (String.IsNullOrEmpty(strServerName) == true)
                {
                    strError = "在没有指定服务器名的情况下，必须指定发起路径";
                    return -1;
                }
            }*/

            string strProjectName = "";

            if (this.comboBox_projectName.Text == ""
                || this.comboBox_projectName.Text == "{default}"
                || this.comboBox_projectName.Text == "<default>"
                || this.comboBox_projectName.Text == "<默认>")
            {
                // 获得和一个来源路径相关的缺省查重方案
                string strDatabasePath = "";
                strProjectName = GetDefaultProjectName(this.RecordPath,
                    out strDatabasePath);
                if (String.IsNullOrEmpty(strProjectName) == true)
                {
                    strError = "没有定义和发起数据库 '" + strDatabasePath + "' 关联的缺省查重方案，因此无法进行查重...";
                    return -1;
                }

                this.comboBox_projectName.Text = strProjectName;    // 将实际用到的查重方案名显示出来
            }
            else
            {
                strProjectName = this.comboBox_projectName.Text;
            }

            this.m_nodeProject = this.dom.DocumentElement.SelectSingleNode("//project[@name='"+strProjectName+"']");
            if (this.m_nodeProject == null)
            {
                strError = "没有定义名字为 '" + strProjectName + "' 的查重方案";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("开始检索 ...");
            stop.BeginLoop();

            this.Update();

            this.EnableControls(false);

            this.EventFinish.Reset();
            this.m_bInSearch = true;
            try
            {
                this.ClearDupState(true);
                this.listView_browse.Items.Clear();

                // 列出<project>元素下的所有<database>元素
                XmlNodeList databases = this.m_nodeProject.SelectNodes("database");
                for (int i = 0; i < databases.Count; i++)
                {
                    XmlNode nodeDatabase = databases[i];

                    int nRet = DoOneDatabase(
                        nodeDatabase,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 排序
                {
                    ColumnSortStyle sortStyle = ColumnSortStyle.RightAlign;

                    this.SortColumns.SetFirstColumn(1,
                        sortStyle,
                        this.listView_browse.Columns,
                        true);

                    this.SortColumns[0].Asc = false;    // 大的在前
                    this.SortColumns.RefreshColumnDisplay(this.listView_browse.Columns);

                    // 排序
                    this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);
                    this.listView_browse.ListViewItemSorter = null;
                }

                // 显示查重状态
                this.SetDupState();
            }
            finally
            {
                this.EventFinish.Set();
                this.m_bInSearch = false;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);

            }

            // 命中数
            return this.listView_browse.Items.Count;
        }

        // 获得配置文件中所有的方案名
        List<string> GetAllProjectName()
        {
            List<string> results = new List<string>();

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(DomUtil.GetAttr(nodes[i], "name"));
            }

            return results;
        }

        // 通过发起数据库，得到查重方案名
        // parameters:
        //      strStartPath    发起记录路径。不一定要数据库路径，可以深于数据库
        //      strDatabasePath [out]顺便返回数据库路径
        string GetDefaultProjectName(string strStartPath,
            out string strDatabasePath)
        { 
            // 把记录路径加工为两段形式的(服务器名/数据库名)
            strDatabasePath = GetDbNameStylePath(strStartPath);

            XmlNode node = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + strDatabasePath + "']");
            if (node == null)
                return null;

            return DomUtil.GetAttr(node, "defaultProject");
        }

        // 把记录路径加工为两段形态
        string GetDbNameStylePath(string strPath)
        {
            string[] parts = strPath.Split(new char [] {'/'} );

            if (parts.Length == 0)
                return "";

            if (parts.Length == 1)
                return parts[0];

            Debug.Assert(parts.Length >= 2, "");

            return parts[0] + "/" + parts[1];
        }

        // 做一个数据库内的检索
        // parameters:
        //      strServerName   目标服务器名。这个和<database>元素name属性中的服务器名部分可不一定一样
        //      strDatabaseName 数据库路径
        int DoOneDatabase(
            XmlNode nodeDatabase,
            out string strError)
        {
            strError = "";

            string strDatabasePath = DomUtil.GetAttr(nodeDatabase, "name");
            string strThreshold = DomUtil.GetAttr(nodeDatabase, "threshold");

            int nThreshold = 0;

            try
            {
                nThreshold = Convert.ToInt32(strThreshold);
            }
            catch
            {
                strError = "threshold值 '" + strThreshold + "' 格式不正确，应当为纯数字";
                return -1;
            }

            // string strServerName = GetServerName(strDatabasePath);

            List<string> results = null;

            // 获得一条记录的关于这个数据库的检索点
            int nRet = this.DtlpChannel.GetAccessPoint(strDatabasePath + "/ctlno/?", // 全路径
                this.MarcRecord,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            // 将results加工为合用的数组
            List<KeyFromItem> items = BuildKeyFromList(results);

            // 遍历数据中实际存在的每个key+from
            for (int i = 0; i < items.Count; i++)
            {
                KeyFromItem item = items[i];

                string strWeight = "";
                string strSearchStyle = "";

                // 获得这个from有关的weight/searchstyle
                // 获得关于一个检索点的定义信息 weight / searchstyle
                // parameters:
                //      strFromName from名。注意里面的31字符应当替换为'$'
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetAccessPointDef(nodeDatabase,
                    item.From,
                    out strWeight,
                    out strSearchStyle,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                Debug.Assert(strWeight != "", "");

                // 进行检索
                nRet = DoOneSearch(
                    strDatabasePath,
                    item.From,
                    item.Key,
                    strWeight,
                    strSearchStyle,
                    nThreshold,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // TODO: 将所有<accessPoint>节点存储到一个hashtable中，以便提高运行速度？便于查检，而不用xpath来选择?


            return 0;
        }

        // 获得关于一个检索点的定义信息 weight / searchstyle
        // parameters:
        //      strFromName from名。注意里面的31字符应当替换为'$'
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        static int GetAccessPointDef(XmlNode nodeDatabase,
            string strFromName,
            out string strWeight,
            out string strSearchStyle,
            out string strError)
        {
            strWeight = "";
            strSearchStyle = "";
            strError = "";

            XmlNode node = nodeDatabase.SelectSingleNode("accessPoint[@name='"+strFromName+"']");
            if (node == null)
                return 0;

            strWeight = DomUtil.GetAttr(node, "weight");
            strSearchStyle = DomUtil.GetAttr(node, "searchStyle");

            return 1;
        }

        // 针对一个检索词的检索
        int DoOneSearch(
            string strDatabasePath,
            string strFromName,
            string strWord,
            string strWeight,
            string strSearchStyle,
            int nThreshold,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            byte[] baNext = null;
            int nStyle = DtlpChannel.CTRLNO_STYLE;

            if (strSearchStyle.ToLower() == "exact")
                nStyle |= DtlpChannel.EXACT_RECORD;

            string strPath = "";
            strPath = strDatabasePath + "/" + strFromName.Replace('$', (char)31) + "/" + strWord;

            // int nDupCount = 0;

            List<string> path_list = new List<string>();

            bool bFirst = true;       // 第一次检索
            while (true)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }
                }

                Encoding encoding = this.DtlpChannel.GetPathEncoding(strPath);

                byte[] baPackage;
                if (bFirst == true)
                {
                    nRet = this.DtlpChannel.Search(strPath,
                        nStyle,
                        out baPackage);
                }
                else
                {
                    nRet = this.DtlpChannel.Search(strPath,
                        baNext,
                        nStyle,
                        out baPackage);
                }
                if (nRet == -1)
                {
                    int errorcode = this.DtlpChannel.GetLastErrno();
                    if (errorcode == DtlpChannel.GL_NOTEXIST)
                    {
                        break;
                    }
                    strError = this.DtlpChannel.GetErrorString();
                    goto ERROR1;
                }

                bFirst = false;

                Package package = new Package();
                package.LoadPackage(baPackage,
                    encoding);

                nRet = package.Parse(PackageFormat.String);
                if (nRet == -1)
                {
                    strError = "Package::Parse() error";
                    goto ERROR1;
                }

                // TODO: 检索索引号先进入一个内存结构，去重以后，再合并进入listview
                AddToPathList(package,
                    ref path_list);

                if (package.ContinueString != "")
                {
                    nStyle |= DtlpChannel.CONT_RECORD;
                    baNext = package.ContinueBytes;
                }
                else
                {
                    break;
                }
            }

            // 排序去重
            path_list.Sort();
            Global.RemoveDup(ref path_list);

            nRet = FillBrowseList(path_list,
                strWeight,
                nThreshold,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            return 0;
        ERROR1:
            return -1;
        }



        // 加入命中路径list
        void AddToPathList(Package package,
            ref List<string> list)
        {
            for (int i = 0; i < package.Count; i++)
            {
                Cell cell = (Cell)package[i];

                string strShortPath = Global.ModifyDtlpRecPath(cell.Path,
    "");
                list.Add(strShortPath);
            }
        }

        int FillBrowseList(List<string> path_list,
            string strWeight,
            int nThreshold,
            out string strError)
        {
            strError = "";

            int nWeight = 0;
            try
            {
                nWeight = Convert.ToInt32(strWeight);
            }
            catch
            {
                strError = "weight字符串 '" + strWeight + "' 格式不正确，应该为纯数字";
                return -1;
            }


            string strShorterStartRecordPath = Global.ModifyDtlpRecPath(this.RecordPath, "");

            // int nDupCount = 0;

            // 处理每条记录
            for (int i = 0; i < path_list.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                string strPath = path_list[i];


                // 查重
                ListViewItem item = DetectDup(strPath);

                int nCurrentWeight = 0;

                if (item == null)
                {
                    string strCompletePath = Global.ModifyDtlpRecPath(strPath,
                        "ctlno");

                    item = new ListViewItem();
                    item.Text = strPath;
                    item.SubItems.Add(strWeight);
                    this.listView_browse.Items.Add(item);

                    nCurrentWeight = nWeight;

                    string[] cols = null;

                    int nRet = GetOneBrowseRecord(
                        strCompletePath,
                        out cols,
                        out strError);
                    if (nRet == -1)
                    {
                        item.SubItems.Add(strError);
                        continue;
                    }
                    if (cols != null)
                    {
                        // 确保列标题数量足够
                        ListViewUtil.EnsureColumns(this.listView_browse,
                            cols.Length,
                            200);
                        for (int j = 0; j < cols.Length; j++)
                        {
                            item.SubItems.Add(cols[j]);
                        }
                    }
                }
                else
                {
                    // 先获得已经有的分数
                    string strExistWeight = item.SubItems[1].Text;

                    int nExistWeight = 0;
                    try
                    {
                        nExistWeight = Convert.ToInt32(strExistWeight);
                    }
                    catch
                    {
                        strError = "原有的weight字符串 '" +strExistWeight + "' 格式不正确，应该为纯数字";
                        return -1;
                    }

                    nCurrentWeight = nExistWeight + nWeight;

                    item.SubItems[1].Text = nCurrentWeight.ToString();
                }

                if (strPath == strShorterStartRecordPath)
                {
                    // 如果就是发起记录自己  2008/2/29
                    item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                    item.BackColor = Color.LightGoldenrodYellow;
                    item.ForeColor = SystemColors.GrayText; // 表示就是发起记录自己
                }
                else if (nCurrentWeight >= nThreshold)
                {
                    item.ImageIndex = ITEMTYPE_OVERTHRESHOLD;
                    item.BackColor = Color.LightYellow;
                    item.Font = new Font(item.Font, FontStyle.Bold);
                }
                else
                {
                    item.ImageIndex = ITEMTYPE_NORMAL;
                }

                // this.listView_browse.UpdateItem(this.listView_browse.Items.Count - 1);
            }

            return 0;
        }

        // 获得浏览记录内容
        // parameters:
        //      strPath 记录路径。应当为完全形态
        int GetOneBrowseRecord(
            string strPath,
            out string[] cols,
            out string strError)
        {
            strError = "";
            cols = null;

            int nRet = 0;

            int nStyle = DtlpChannel.JH_STYLE; // 获得简化记录

            byte[] baPackage;
            nRet = this.DtlpChannel.Search(strPath,
                nStyle,
                out baPackage);
            if (nRet == -1)
            {
                strError = "Search() path '" + strPath + "' 时发生错误: " + this.DtlpChannel.GetErrorString();
                goto ERROR1;
            }

            Package package = new Package();
            package.LoadPackage(baPackage,
                this.DtlpChannel.GetPathEncoding(strPath));
            nRet = package.Parse(PackageFormat.String);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            string strContent = package.GetFirstContent();

            if (String.IsNullOrEmpty(strContent) == false)
            {
                cols = strContent.Split(new char[] { '\t' });
            }

            return 0;
        ERROR1:
            return -1;
        }

        // TODO: 遇到多次之间重复的，需要累加分数。一次之内，不允许重复
        // 检查是否重了
        // TODO: 如果可能，用Hashtable来提高速度
        ListViewItem DetectDup(string strPath)
        {
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];
                if (item.Text == strPath)
                    return item;
            }

            return null;
        }

        /*
        // 把路径加工为缩略或者正规形态
        // parameters:
        //      strCtlnoPart    如果为""，表示加工为缩略的、只用于前端显示形态，如果为"ctlno"或者"记录索引号"，表示加工为正规形态
        string ModifyDtlpRecPath(string strPath,
            string strCtlnoPart)
        {
            int nRet = strPath.LastIndexOf("/");

            if (nRet == -1)
                return strPath;

            string strNumber = strPath.Substring(nRet + 1).Trim();

            nRet = strPath.LastIndexOf("/", nRet - 1);
            if (nRet == -1)
                return strPath;

            string strPrevPart = strPath.Substring(0, nRet).Trim();

            return strPrevPart + "/" + strCtlnoPart + "/" + strNumber;
        }*/

        private void comboBox_projectName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_projectName.Items.Count == 0)
            {
                List<string> projectnames = GetAllProjectName();
                for (int i = 0; i < projectnames.Count; i++)
                {
                    this.comboBox_projectName.Items.Add(projectnames[i]);
                }

                this.comboBox_projectName.Items.Add("{default}");
            }
        }

        static List<KeyFromItem> BuildKeyFromList(List<string> results)
        {
            List<KeyFromItem> items = new List<KeyFromItem>();
            for (int i = 0; i < results.Count / 2; i++)
            {
                KeyFromItem item = new KeyFromItem();
                item.Key = results[i * 2];
                item.From = results[i * 2 + 1].Replace((char)31, '$');
                items.Add(item);
            }

            return items;
        }

        private void DtlpDupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            EventFinish.Set();

            if (stop != null) // 脱离关联
            {
                stop.Style = StopStyle.None;    // 需要强制中断
                stop.DoStop();

                stop.Unregister();	// 和容器脱离关联
                stop = null;
            }
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.DtlpChannel != null)
                this.DtlpChannel.Cancel();
        }

        void ClearDupState(bool bSearching)
        {
            if (bSearching == true)
                this.label_dupMessage.Text = "正在查重...";
            else
                this.label_dupMessage.Text = "尚未查重";
        }

        // 设置查重状态
        void SetDupState()
        {
            int nCount = 0;

            string strShorterStartRecordPath = Global.ModifyDtlpRecPath(this.RecordPath,
                "");

            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];

                if (item.Text == strShorterStartRecordPath)
                    continue;   // 不包含发起记录自己 2008/2/29

                if (item.ImageIndex == ITEMTYPE_OVERTHRESHOLD)
                    nCount++;
                else
                    break;  // 假定超过权值的事项都在前部，一旦发现一个不是的事项，循环就结束
            }

            if (nCount > 0)
                this.label_dupMessage.Text = "有 " + Convert.ToString(nCount) + " 条重复记录。";
            else
                this.label_dupMessage.Text = "没有重复记录。";

        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 第一列为记录路径，排序风格特殊
            if (nClickColumn == 0)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView_browse.Columns,
                true);

            // 排序
            this.listView_browse.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_browse.ListViewItemSorter = null;

        }

        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            // 防止重入
            if (m_bInSearch == true)
            {
                MessageBox.Show(this, "当前窗口正在被一个未结束的长操作使用，无法装载记录。请稍后再试。");
                return;
            }

            if (this.LoadDetail != null)
            {
                int nIndex = -1;
                if (this.listView_browse.SelectedIndices.Count > 0)
                    nIndex = this.listView_browse.SelectedIndices[0];
                else
                {
                    if (this.listView_browse.FocusedItem == null)
                    {
                        MessageBox.Show(this, "尚未选定要装入详细窗的事项");
                        return;
                    }
                    nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
                }

                LoadDetailEventArgs e1 = new LoadDetailEventArgs();
                e1.RecordPath = Global.ModifyDtlpRecPath(this.listView_browse.Items[nIndex].Text, "ctlno");
                this.LoadDetail(this, e1);
            }
        }
    }

    class KeyFromItem
    {
        public string Key = "";
        public string From = "";
    }

    // 调用数据加工模块
    public delegate void LoadDetailEventHandler(object sender,
        LoadDetailEventArgs e);

    public class LoadDetailEventArgs : EventArgs
    {
        public string ServerName = "";
        public string RecordPath = "";
        public bool OpenNewWindow = true;
    }
}