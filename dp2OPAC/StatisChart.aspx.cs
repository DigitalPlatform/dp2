using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;
using System.Web.UI.DataVisualization.Charting;
using System.Drawing;

using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.OPAC.Server;

public partial class StatisChart : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

    // XmlDataSource TimeDataSource = null;
    // string SelectedDate = "";

    string m_strTempFilename = "";

#if NO
    protected override void InitializeCulture()
    {
        WebUtil.InitLang(this);
        base.InitializeCulture();
    }
#endif

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        this.TreeView1.EventMode = true;

        // 这是已经宏兑现的文件
        string strDataFile = (string)this.Session["__timerange_data_filename__"];  // 只能使用备用值

        if (string.IsNullOrEmpty(strDataFile) == false)
        {
            this.TreeView1.XmlFileName = strDataFile;
        }

        this.TitleBarControl1.CurrentColumn = TitleColumn.Statis;
        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

        this.PanelControl1.Title = (string)this.GetLocalResourceObject("时间范围");
        this.PanelControl2.Title = (string)this.GetLocalResourceObject("统计指标");

        {
            this.m_strTempFilename = PathUtil.MergePath(sessioninfo.GetTempDir(), "~statis_chat_entry");
            if (File.Exists(this.m_strTempFilename) == true)
            {
                this.StatisEntryControl1.XmlFileName = m_strTempFilename;
            }
        }

        this.StatisEntryControl1.CheckedChanged += new CheckedChangedEventHandler(StatisEntryControl1_CheckedChanged);
        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        StatisEntryControl1_CheckedChanged(sender, e);
    }

    void StatisEntryControl1_CheckedChanged(object sender, EventArgs e)
    {
        // 记忆
        // this.HiddenField_entryItems.Value = StringUtil.MakePathList(this.StatisEntryControl1.GetSelectedItems());

        string strDate = this.TreeView1.SelectedNodePath;
        /*
        if (String.IsNullOrEmpty(strDate) == true
            && this.TreeView1.SelectedNode != null)
        {
            XmlDocument dom = (XmlDocument)this.TimeDataSource.GetXmlDocument();

            XmlNode node = dom.SelectSingleNode(this.TreeView1.SelectedNode.DataPath);

            if (node != null)
            {
                strDate = DomUtil.GetAttr(node, "date");
            }
        }
         * */

        string strError = "";
        if (string.IsNullOrEmpty(strDate) == false)
        {
            int nRet = GetResult(strDate,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }
        return;
        ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }

#if NO
    static void FillLibraryCodeList(DropDownList list,
    List<string> items)
    {
        if (list.Items.Count > 0)
            return;

        list.Items.Clear();

        if (items == null)
            return;

        list.Items.Add("<所有分馆>");
        foreach (string s in items)
        {
            list.Items.Add(s);
        }
    }
#endif

    static void FillChartTypeList(DropDownList list)
    {
        if (list.Items.Count > 0)
            return;

        list.Items.Clear();

        var values = Enum.GetValues(typeof(SeriesChartType)).Cast<SeriesChartType>();

        foreach (SeriesChartType s in values)
        {
            if (s == SeriesChartType.Renko
                || s == SeriesChartType.ThreeLineBreak
                || s == SeriesChartType.Kagi
                || s == SeriesChartType.PointAndFigure)
                continue;

            list.Items.Add(s.ToString());
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string strStatisColumnVisible = GetStatisColumnVisible();

        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);
        string strRole = loginstate.ToString().ToLower(); // notlogin public reader librarian

        if (StringUtil.IsInList("all", strStatisColumnVisible) == true
            || (loginstate == LoginState.Librarian && StringUtil.IsInList("librarian", strStatisColumnVisible) == true)
            || (loginstate == LoginState.Reader && StringUtil.IsInList("reader", strStatisColumnVisible) == true)
            || (loginstate == LoginState.Public && StringUtil.IsInList("public", strStatisColumnVisible) == true)
            || (loginstate == LoginState.NotLogin && StringUtil.IsInList("notlogin", strStatisColumnVisible) == true)
            )
        {
            // 2015/4/3
            // 尚未登录的情况下，需要一点额外处理
            if (loginstate == LoginState.NotLogin)
            {
                sessioninfo.UserID = "public";
                sessioninfo.IsReader = false;
            }
        }
        else
        {
            Response.Write("必须是 " + strStatisColumnVisible + " 状态才能使用statischart.aspx");
            Response.End();
            /*
            // 是否登录?
            if (sessioninfo.UserID == "")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                return;
            }*/
        }

        string strSideBarFile = Server.MapPath("./statis_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        this.Chart1.ChartAreas[0].AxisX.IsLabelAutoFit = true;
        this.Chart1.ChartAreas[0].AxisX.Interval = 1;

        if (this.IsPostBack == true)
        {
            if (string.IsNullOrEmpty(this.HiddenField_imageWidth.Value) == false)
            {
                string[] parts = this.HiddenField_imageWidth.Value.Split(new char[] { ',' });
                string strWidth = parts[0];
                string strHeight = "";
                if (parts.Length >= 2)
                    strHeight = parts[1];
                double w = 0;
                if (double.TryParse(strWidth, out w) == false
                    || w == 0)
                {
                    throw new Exception("字符串格式不正确 this.HiddenField_imageWidth.Value = '" + this.HiddenField_imageWidth.Value + "' 左边部分应该为数字");
                }

                if (w <= 0)
                    w = 500;

                w = Math.Min(w, 2000);

                double h = w * 0.46;

                if (string.IsNullOrEmpty(strHeight) == false)
                {
                    if (double.TryParse(strHeight, out h) == false
    || h == 0)
                    {
                        throw new Exception("字符串格式不正确 this.HiddenField_imageWidth.Value = '" + this.HiddenField_imageWidth.Value + "' 逗号右边部分应该为数字");
                    }
                    if (h <= 0)
                        h = 500;

                    h = Math.Min(h, 1000);
                }

                this.Chart1.Width = new Unit(w, UnitType.Pixel);
                this.Chart1.Height = new Unit(h, UnitType.Pixel);
            }
        }

        FillChartTypeList(this.DropDownList_chartType);
        if (this.IsPostBack == false)
        {
            this.DropDownList_chartType.Text = "Line";
        }

        // 这是尚未宏兑现的原始 XML 文件
        string strDataFileName = this.Request["datafile"];

        if (String.IsNullOrEmpty(strDataFileName) == true)
            strDataFileName = "statis_timerange.xml";

        // 进行宏兑现
        MacroDataFile(strDataFileName, strDataFileName + ".1");

        if (string.IsNullOrEmpty(this.TreeView1.XmlFileName) == true || this.IsPostBack == false
    // 如果数据文件名和参数中的不吻合，需要重新设置数据集
    || PathUtil.PureName(this.TreeView1.XmlFileName).ToLower() != (strDataFileName + ".1").ToLower()
    )
        {
            string strSourceFileName = app.DataDir + "/cfgs/" + strDataFileName;
            this.TreeView1.XmlFileName = strSourceFileName + ".1";
            if (File.Exists(this.TreeView1.XmlFileName) == false)
            {
                BuildDefaultDataXmlFile(strSourceFileName);
                MacroDataFile(strSourceFileName, this.TreeView1.XmlFileName);
            }
            this.Session["__timerange_data_filename__"] = this.TreeView1.XmlFileName; // 补充保存在Session中，备用。因为在_Init()阶段是得不到隐藏字段的值的
        }

        string strDate = "";

        if (this.IsPostBack == false)
        {
            strDate = this.Page.Request["date"];
            if (string.IsNullOrEmpty(strDate) == true)
                strDate = DateTimeUtil.DateTimeToString8(DateTime.Now).Substring(0, 6); // 当前月份
        }
        else
            strDate = this.TreeView1.SelectedNodePath;

        /*
        if (this.IsPostBack == false
            && string.IsNullOrEmpty(this.HiddenField_entryItems.Value) == false)
        {
            this.StatisEntryControl1.SelectItems(StringUtil.SplitList(this.HiddenField_entryItems.Value));
        }
         * */

        /*
        if (String.IsNullOrEmpty(strDate) == true
            && this.TreeView1.SelectedNode != null)
        {
            XmlDocument dom = (XmlDocument)this.TimeDataSource.GetXmlDocument();
            XmlNode node = dom.SelectSingleNode(this.TreeView1.SelectedNode.DataPath);
            if (node != null)
            {
                strDate = DomUtil.GetAttr(node, "date");
            }
        }
         * */

        string strError = "";
        if (string.IsNullOrEmpty(strDate) == false)
        {
            int nRet = GetResult(strDate,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }
        return;
        ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }

    // statis_timerange.xml 文件兑现了宏以后写入一个目标文件。给 TreeView 用的是目标文件
    static bool MacroDataFile(string strSourceFilename, string strTargetFilename)
    {
        if (File.Exists(strSourceFilename) == false)
            return false;

        File.Delete(strTargetFilename);

        XmlDocument dom = new XmlDocument();
        dom.Load(strSourceFilename);

        // 兑现宏
        // parameters:
        //      attr_list   要替换的属性名列表。例如 name, command
        int nRet = CacheBuilder.MacroDom(dom,
            new List<string> { "name", "date" },
            out string strError);
        if (nRet == -1)
            throw new Exception(strError);

        dom.Save(strTargetFilename);

        return true;
    }

    // 创建一个表示当年和去年的默认 XML 内容
    // 尽量用宏表示时间
    static void BuildDefaultDataXmlFile(string strFilename)
    {
        XmlDocument dom = new XmlDocument();
        dom.LoadXml("<root />");

        // DateTime now = DateTime.Now;
        string[] macro_names = new string[] { "prevyear", "year" };
        foreach (string macro_name in macro_names)
        {
            XmlNode year = dom.CreateElement("class");
            dom.DocumentElement.AppendChild(year);
            //DomUtil.SetAttr(year, "name", now.Year.ToString() + "年");
            //DomUtil.SetAttr(year, "date", now.Year.ToString().PadLeft(4, '0'));
            DomUtil.SetAttr(year, "name", "%" + macro_name + "%年");
            DomUtil.SetAttr(year, "date", "%" + macro_name + "%");

            // 12个月
            for (int i = 0; i < 12; i++)
            {
                XmlNode month = dom.CreateElement("class");
                year.AppendChild(month);
                //DomUtil.SetAttr(month, "name", now.Year.ToString() + "年" + (i + 1).ToString() + "月");
                //DomUtil.SetAttr(month, "date", now.Year.ToString().PadLeft(4, '0') + (i + 1).ToString().PadLeft(2, '0'));
                DomUtil.SetAttr(month, "name", "%" + macro_name + "%年" + (i + 1).ToString() + "月");
                DomUtil.SetAttr(month, "date", "%" + macro_name + "%" + (i + 1).ToString().PadLeft(2, '0'));
            }
        }

        dom.Save(strFilename);
    }

    string GetStatisColumnVisible()
    {
        // 元素缺乏时的缺省值
        string strStatisColumnVisible = "reader,librarian";
        if (app.WebUiDom == null)
            return strStatisColumnVisible;

        XmlNode nodeStatisColumn = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/statisColumn");
        if (nodeStatisColumn != null)
        {
            // 一旦元素具备，就没有缺省值了
            strStatisColumnVisible = DomUtil.GetAttr(nodeStatisColumn, "visible");
        }

        return strStatisColumnVisible;
    }

    List<XmlNode> m_nodes = null;

    protected void TreeView1_GetNodeData(object sender, GetNodeDataEventArgs e)
    {
        if (e.Node.Name[0] == '_')
            return;

        if (m_nodes == null)
        {
            // 首次初始化
            m_nodes = GetParentNodes(e.Node.OwnerDocument.DocumentElement, this.TreeView1.SelectedNodePath);
        }

        string strName = DomUtil.GetAttr(e.Node, "name");
        if (string.IsNullOrEmpty(strName) == true)
            return;

        string strDate = DomUtil.GetAttr(e.Node, "date");

        // string strDataFile = Path.GetFileName(this.TreeView1.XmlFileName).ToLower();

        e.Name = strName;


        if (strDate == this.TreeView1.SelectedNodePath)
        {
            e.Seletected = true;
        }

        /*
        string strDateParam = "";
        if (string.IsNullOrEmpty(strDate) == false)
            strDateParam = "&date=" + HttpUtility.UrlEncode(strDate);

        e.Url = "./statischart.aspx?" + strDateParam;
         * */
        e.Url = strDate;

        // if (e.Node == e.Node.OwnerDocument.DocumentElement)
        if (this.m_nodes != null && this.m_nodes.Count > 0)
        {
            if (this.m_nodes.IndexOf(e.Node) != -1)
                e.Closed = false;
        }
        else
            e.Closed = false;

    }

    static List<XmlNode> GetParentNodes(XmlNode root, string strDate)
    {
        List<XmlNode> results = new List<XmlNode>();
        XmlNode node = root.OwnerDocument.SelectSingleNode("//*[@date='" + strDate + "']");
        if (node == null)
            return results;
        node = node.ParentNode;
        while (node != null)
        {
            results.Add(node);
            node = node.ParentNode;
        }
        return results;
    }

#if NO
    protected void TreeView1_TreeNodeDataBound(object sender, TreeNodeEventArgs e)
    {

    }
#endif

#if NO
    protected void TreeView1_DataBound(object sender, EventArgs e)
    {
        if (this.TreeView1.Nodes.Count == 0)
            return;

        TreeNode root = this.TreeView1.Nodes[0];

        XmlDocument dom = (XmlDocument)this.TimeDataSource.GetXmlDocument();

        if (String.IsNullOrEmpty(this.SelectingNodePath) == false)
        {
            TreeNode selecting_node = GetTreeNode(root,
    this.SelectingNodePath);
            if (selecting_node != null)
            {
                selecting_node.Selected = true;

                TreeView1_SelectedNodeChanged(null, null);
            }
        }

        // 收缩第二级
        if (this.TreeView1.Nodes.Count > 0)
        {
            foreach (TreeNode node in this.TreeView1.Nodes)
            {
                node.Collapse();
            }
        }

    }
#endif

#if NO
    static TreeNode GetTreeNode(TreeNode root,
string strNodePath)
    {
        string[] path = strNodePath.Split(new char[] { '_' });
        TreeNode current_node = root;
        for (int i = 1; i < path.Length; i++)
        {
            string strNumber = path[i];
            int nNumber = Convert.ToInt32(strNumber);
            current_node = current_node.ChildNodes[nNumber];
        }

        return current_node;
    }
#endif

#if NO
    protected void TreeView1_SelectedNodeChanged(object sender, EventArgs e)
    {
        if (this.TreeView1.SelectedNode == null)
            return;

        XmlDocument dom = (XmlDocument)this.TimeDataSource.GetXmlDocument();

        XmlNode node = dom.SelectSingleNode(this.TreeView1.SelectedNode.DataPath);

        if (node == null)
            return;

        string strDate = DomUtil.GetAttr(node, "date");
        if (string.IsNullOrEmpty(strDate) == true)
            return;

        string strError = "";
        int nRet = GetResult(strDate,
            true,
            out strError);
        if (nRet == -1)
            goto ERROR1;
        return;
    ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }
#endif

    // 设置好统计指标面板
    int SetEntryPanel(string strXml,
        string strTotalXmlFilename,
        out string strError)
    {
        strError = "";

        if (string.IsNullOrEmpty(this.m_strTempFilename) == true)
            this.m_strTempFilename = PathUtil.MergePath(sessioninfo.GetTempDir(), "~statis_chat_entry");

        // Debug.WriteLine("temp file -- " + this.m_strTempFilename);

        // 将当前数据XML中的事项信息和累积文件合并。如果发现有增补，则修改累积文件
        int nRet = PrepareXml(
            strXml,
            strTotalXmlFilename,
            m_strTempFilename,
            out strError);
        if (nRet == -1)
            return -1;

        this.StatisEntryControl1.XmlFileName = m_strTempFilename;

        return 0;
    }

    // 把三段的路径缩减为两段形态
    static string MakePath(string strText)
    {
        string[] parts = strText.Split(new char[] { '/' });
        if (parts.Length <= 2)
            return strText;
        return parts[1] + "/" + parts[2];   // 丢掉第一段
    }

    // 比较路径。如果遇到第一级匹配的，就算匹配上了
    static bool MatchPath(List<string> paths, string strPath)
    {
        foreach (string s in paths)
        {
            if (s == strPath)
                return true;
            string[] s_parts = s.Split(new char[] { '/' });
            string[] c_parts = strPath.Split(new char[] { '/' });
            if (s_parts.Length == 1 && s_parts[0] == c_parts[0])
                return true;
        }

        return false;
    }

    // 结合当天日期，如果时间范围字符串中实际包含的时间只有一天，则在前面多加上一天
    // return:
    //      false   没有改变
    //      true    发生了改变
    static bool ModifyDateString(ref string strDateRangeString)
    {
        string strStartDate = "";
        string strEndDate = "";

        // 将日期字符串解析为起止范围日期
        // throw:
        //      Exception
        DateTimeUtil.ParseDateRange(strDateRangeString,
            out strStartDate,
            out strEndDate);

        DateTime start;
        DateTime end;

        start = DateTimeUtil.Long8ToDateTime(strStartDate);

        if (strEndDate == "")
        {
            end = start;
            strEndDate = strStartDate;
        }
        else
        {
            end = DateTimeUtil.Long8ToDateTime(strEndDate);

            TimeSpan delta = end - start;
            if (delta.Days < 0)
            {
                // 交换两个时间
                DateTime temp = end;
                end = start;
                start = temp;
            }
        }

        DateTime today = DateTimeUtil.Long8ToDateTime(DateTimeUtil.DateTimeToString8(DateTime.Now));

        if (start > today)
            return false;   // 这种情况没有意义(返回不了统计结果)，就不处理了

        int nCount = (int)(today - start).TotalDays + 1; // 实际存在多少天

        if (nCount == 1)
        {
            start -= new TimeSpan(1, 0, 0, 0);
            strDateRangeString = DateTimeUtil.DateTimeToString8(start) + "-" + DateTimeUtil.DateTimeToString8(end);
            return true;
        }

        return false;
    }

    static string GetDateName(string strDate)
    {
        if (string.IsNullOrEmpty(strDate) == true)
            return strDate;

        if (strDate.Length == 4)
            return strDate + "年";

        if (strDate.Length == 6)
            return strDate.Substring(0, 4) + "年" + strDate.Substring(4, 2).Trim(new char[] { '0' }) + "月";

        return strDate;
    }

    int GetResult(string strDate,
        bool bForce,
        out string strError)
    {
        strError = "";
        int nRet = 0;

        if (string.IsNullOrEmpty(strDate) == true)
            strDate = this.TreeView1.SelectedNodePath;

        // 保存起来
        if (string.IsNullOrEmpty(strDate) == false)
            this.TreeView1.SelectedNodePath = strDate;

        this.Page.Title = (string)this.GetLocalResourceObject("统计图") + " - " + GetDateName(strDate);

        SeriesChartType type = SeriesChartType.Line;
        if (string.IsNullOrEmpty(this.DropDownList_chartType.Text) == false)
            type = (SeriesChartType)Enum.Parse(typeof(SeriesChartType), this.DropDownList_chartType.Text, true);

        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);
        string strRole = loginstate.ToString().ToLower(); // notlogin public reader librarian

        string strDataFilename = PathUtil.MergePath(sessioninfo.GetTempDir(), "~statis_chat_data_" + strRole);
        XmlDocument dom = new XmlDocument();

        if (bForce == false && File.Exists(strDataFilename) == true)
        {
            try
            {
                dom.Load(strDataFilename);
            }
            catch (Exception ex)
            {
                strError = "文件装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 日期范围也要对得上才行
            string strFileRange = DomUtil.GetElementText(dom.DocumentElement, "range");
            if (strFileRange != strDate)
            {
                bForce = true;
                dom = new XmlDocument();
            }
            else
                bForce = false;
        }
        else
            bForce = true;


        if (bForce == true)
        {
            RangeStatisInfo info = null;
            string strXml = "";

            ModifyDateString(ref strDate);

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                long lRet = //sessioninfo.Channel.
                    channel.GetStatisInfo(strDate,
                    "list",
                    out info,
                    out strXml,
                    out strError);
                if (lRet == -1)
                    return -1;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            string strTemplateFilename = app.CfgDir + "\\statis_template.xml";
            if (File.Exists(strTemplateFilename) == true)
            {
                string strTargetXml = "";
                nRet = StatisViewControl.FilterXmlFile(
                    strRole, // notlogin public reader librarian
                    strXml,
                    strTemplateFilename,
                    true,
                    out strTargetXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                strXml = strTargetXml;
            }

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement, "range", strDate);

            DomUtil.SetElementText(dom.DocumentElement, "startDate", info.StartDate);
            DomUtil.SetElementText(dom.DocumentElement, "endDate", info.EndDate);

            dom.Save(strDataFilename);
        }


        List<string> selected_itemspaths = this.StatisEntryControl1.GetSelectedItems();

        string strTotalXmlFilename = PathUtil.MergePath(app.DataDir, "~statis_entry_" + strRole + ".xml");

        nRet = SetEntryPanel(dom.DocumentElement.OuterXml,
            strTotalXmlFilename,
            out strError);
        if (nRet == -1)
            return -1;

        // 如果首次访问的时候没有指定统计指标
        if (this.IsPostBack == false && selected_itemspaths.Count == 0)
        {
            selected_itemspaths.Add("出纳");
            this.StatisEntryControl1.SelectItems(selected_itemspaths);
        }


#if NO
        string strLibraryCode = this.DropDownList_libraryCode.Text;
        if (strLibraryCode == "<所有分馆>")
            strLibraryCode = "";
#endif
        string strLibraryCode = (string)this.Page.Session["librarycode"];

        this.Chart1.ChartAreas.Clear();

        this.Chart1.Series.Clear();

        Legend legend = new Legend();
        // legend.Name = strName;
        legend.Docking = Docking.Bottom;
        legend.IsTextAutoFit = true;
        Font default_font = GetDefaultFont();
        if (default_font != null)
            legend.Font = default_font;
        this.Chart1.Legends.Add(legend);

        XmlNodeList nodes = null;

        if (string.IsNullOrEmpty(strLibraryCode) == true)
            nodes = dom.DocumentElement.SelectNodes("category/item");
        else
            nodes = dom.DocumentElement.SelectNodes("library[@code='" + strLibraryCode + "']/category/item");
        int nCount = 0;

        int nArea = 1;
        string strAreaName = "";
        if ((nCount % 100) == 0)
        {
            strAreaName = CreateChartArea(nArea++);
        }

        int nMaxPoints = 0;
        string strStartDate = DomUtil.GetElementText(dom.DocumentElement, "startDate");
        string strEndDate = DomUtil.GetElementText(dom.DocumentElement, "endDate");
        DateTime start_date = DateTimeUtil.Long8ToDateTime(strStartDate);
        DateTime end_date = DateTimeUtil.Long8ToDateTime(strEndDate);
        DateTime now = DateTime.Now;

        foreach (XmlNode node in nodes)
        {
            string strName = GetItemPath(node);

            string strPath = MakePath(strName);
            if (MatchPath(selected_itemspaths, strPath) == false)
                continue;

            string strValue = DomUtil.GetAttr(node, "value");

            Series series = new Series(strName);
            series.ChartType = type;    //  SeriesChartType.Line;
            series.ChartArea = strAreaName;
            series.BorderWidth = 3;
            series.IsVisibleInLegend = true;

            DateTime current_date = start_date;
            string[] values = strValue.Split(new char[] { ',' });
            int i = 0;
            foreach (string v in values)
            {
                // 2012/11/16
                if (current_date <= end_date
    && current_date <= now)
                {
                }
                else
                    break;

                if (i == 0)
                    goto CONTINUE;
                double d = 0;
                if (string.IsNullOrEmpty(v) == false)
                    double.TryParse(v, out d);
                series.Points.AddXY(current_date, d);

                current_date = current_date.AddDays(1);
                CONTINUE:
                i++;
            }

            while (current_date <= end_date
                && current_date <= now)
            {
                series.Points.AddXY(current_date, 0);
                current_date = current_date.AddDays(1);
                i++;
            }

            if (i > nMaxPoints)
                nMaxPoints = i;
            this.Chart1.Series.Add(series);
            nCount++;
        }

        this.Chart1.ChartAreas[0].AxisX.Minimum = start_date.ToOADate();
        this.Chart1.ChartAreas[0].AxisX.Maximum = end_date.ToOADate();

        if (nMaxPoints <= 32 && this.Chart1.ChartAreas.Count > 0)
            this.Chart1.ChartAreas[0].AxisX.Interval = 1;

        if (this.CheckBox_3D.Checked == true)
            this.Chart1.ChartAreas[0].Area3DStyle.Enable3D = true;

        return 0;
    }

    public static Font GetDefaultFont()
    {
        try
        {
            FontFamily family = new FontFamily("微软雅黑");
        }
        catch
        {
            return null;
        }

        return new Font(new FontFamily("微软雅黑"), (float)9.0, GraphicsUnit.Point);
    }

    string CreateChartArea(int i)
    {
        string strName = "ChartArea" + i.ToString();
        ChartArea area = new ChartArea();
        area.Name = strName;

        area.AxisY.LineColor = Color.FromArgb(255, 100, 100, 100);
        area.AxisX.LineColor = Color.FromArgb(255, 100, 100, 100);

        area.AxisY.MajorGrid.LineColor = Color.FromArgb(255, 200, 200, 200);
        area.AxisX.MajorGrid.LineColor = Color.FromArgb(255, 200, 200, 200);

        this.Chart1.ChartAreas.Add(area);
        return strName;
    }

    static string GetItemPath(XmlNode node)
    {
        string strResult = "";
        while (node != null && node != node.OwnerDocument.DocumentElement)
        {
            string strName = "";

            if (node.Name == "item" || node.Name == "category")
                strName = DomUtil.GetAttr(node, "name");
            else
                strName = DomUtil.GetAttr(node, "code");
            if (string.IsNullOrEmpty(strResult) == false)
                strResult = strName + "/" + strResult;
            else
                strResult = strName;

            node = node.ParentNode;
        }

        return strResult;
    }

#if NO
    protected void DropDownList_libraryCode_SelectedIndexChanged(object sender, EventArgs e)
    {
        TreeView1_SelectedNodeChanged(sender, e);
    }
#endif

    protected void DropDownList_chartType_SelectedIndexChanged(object sender, EventArgs e)
    {
        StatisEntryControl1_CheckedChanged(sender, e);
    }


    // 将当前数据XML中的事项信息和累积文件合并。如果发现有增补，则修改累积文件
    int PrepareXml(
        string strXml,
        string strTotalXmlFilename,
        string strCurrentXmlFilename,
        out string strError)
    {
        strError = "";

        XmlDocument total_dom = new XmlDocument();
        try
        {
            total_dom.Load(strTotalXmlFilename);
        }
        catch (FileNotFoundException)
        {
            total_dom.LoadXml("<root />");
        }
        catch (Exception ex)
        {
            strError = "将XML文件 '" + strTotalXmlFilename + "' 装入XMLDOM时出错: " + ex.Message;
            return -1;
        }

        XmlDocument data_dom = new XmlDocument();
        try
        {
            data_dom.LoadXml(strXml);
        }
        catch (Exception ex)
        {
            strError = "数据XML装入DOM时出错: " + ex.Message;
            return -1;
        }

        // return:
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        int nRet = AppendEntry(
            ref total_dom,
            data_dom,
            out strError);
        if (nRet == -1)
            return -1;

        if (nRet == 1)
        {
            lock (this.app)
            {
                total_dom.Save(strTotalXmlFilename);
            }
        }

#if NO
        // 需要保持当前文件中的checked状态
        XmlDocument current_dom = new XmlDocument();
        try
        {
            current_dom.Load(strCurrentXmlFilename);
        }
        catch (FileNotFoundException ex)
        {
            current_dom.LoadXml("<root />");
        }
        catch (Exception ex)
        {
            strError = "将XML文件 '" + strCurrentXmlFilename + "' 装入XMLDOM时出错: " + ex.Message;
            return -1;
        }

        AddChecked(ref total_dom, current_dom);
#endif

        if (File.Exists(strCurrentXmlFilename) == false
            || nRet == 1)
            total_dom.Save(strCurrentXmlFilename);

        return 0;
    }

    static void AddChecked(
        ref XmlDocument target_dom,
        XmlDocument source_dom)
    {
        XmlNodeList source_cats = source_dom.DocumentElement.SelectNodes("category");
        foreach (XmlNode source_cat in source_cats)
        {
            string strValue = DomUtil.GetAttr(source_cat, "checked");
            if (string.IsNullOrEmpty(strValue) == true)
                continue;
            string strCategoryName = DomUtil.GetAttr(source_cat, "name");
            XmlNode target_cat = target_dom.DocumentElement.SelectSingleNode("category[@name='" + strCategoryName + "']");
            if (target_cat == null)
                continue;

            DomUtil.SetAttr(target_cat, "checked", "true");

            XmlNodeList source_items = source_cat.SelectNodes("item");
            foreach (XmlNode source_item in source_items)
            {
                strValue = DomUtil.GetAttr(source_item, "checked");
                if (string.IsNullOrEmpty(strValue) == true)
                    continue;
                string strItemName = DomUtil.GetAttr(source_item, "name");
                XmlNode target_item = target_cat.SelectSingleNode("item[@name='" + strItemName + "']");
                if (target_item != null)
                    DomUtil.SetAttr(target_item, "checked", "true");
            }
        }

    }

    // return:
    //      -1  出错
    //      0   没有发生修改
    //      1   发生了修改
    static int AppendEntry(
        ref XmlDocument total_dom,
        XmlDocument data_dom,
        out string strError)
    {
        strError = "";

        bool bChanged = false;

        XmlNodeList source_nodes = data_dom.DocumentElement.SelectNodes("//category");
        foreach (XmlNode source_node in source_nodes)
        {
            string strCategoryName = DomUtil.GetAttr(source_node, "name");

            // 看看目标中是否有这个<category>元素
            XmlNode target_node = total_dom.DocumentElement.SelectSingleNode("category[@name='" + strCategoryName + "']");
            if (target_node == null)
            {
                target_node = total_dom.CreateElement("category");
                total_dom.DocumentElement.AppendChild(target_node);
                DomUtil.SetAttr(target_node, "name", strCategoryName);
                bChanged = true;
            }

            // 看看此<category>元素下的<item>元素是否齐全
            XmlNodeList source_items = source_node.SelectNodes("item");
            foreach (XmlNode source_item in source_items)
            {
                string strItemName = DomUtil.GetAttr(source_item, "name");
                XmlNode target_item = target_node.SelectSingleNode("item[@name='" + strItemName + "']");
                if (target_item == null)
                {
                    target_item = total_dom.CreateElement("item");
                    target_node.AppendChild(target_item);
                    DomUtil.SetAttr(target_item, "name", strItemName);
                    bChanged = true;
                }
            }
        }

        if (bChanged == true)
            return 1;
        return 0;
    }

    protected void TreeView1_TreeItemClick(object sender, TreeItemClickEventArgs e)
    {
        string strDate = e.Url;

        string strError = "";
        if (string.IsNullOrEmpty(strDate) == false)
        {
            int nRet = GetResult(strDate,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }
        return;
        ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;

    }
    protected void TitleBarControl1_Refreshing(object sender, RefreshingEventArgs e)
    {
        sessioninfo.ClearLoginReaderDomCache();
        e.Cancel = true;

        // 也可以让缺省的动作执行，但等缺省动作完成后，重新创建缓存目录，获得临时文件
    }
}