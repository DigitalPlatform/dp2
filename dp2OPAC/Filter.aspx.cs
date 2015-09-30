using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;
using System.Xml;
using System.Globalization;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;

public partial class Filter : System.Web.UI.Page
{
    OpacApplication app = null;
    SessionInfo sessioninfo = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        string strAction = this.Request["action"];
        if (strAction == "getfilterinfo")
        {
            // resultset 参数是基本结果集的名字
            // selected 参数是希望选定的节点的名字
            string strResultsetName = this.Request["resultset"];
            string strSelected = this.Request["selected"];
            string strLang = this.Request["lang"];
            GetFilterInfo(strResultsetName, strSelected, strLang);
            return;
        }
    }

    protected void Page_Unload(object sender, EventArgs e)
    {
        sessioninfo.Channel.Close();
    }

    // 根据记录数量的多少，少的时候可以立即返回结果，多的时候用线程后台处理，然后可以随时查询状态
    // 线程用线程池，避免过多耗用线程数目

    // 根据结果集名，提取全部书目XML记录，然后批处理经过MarcFilter过滤，创建若干个子结果集
    // 最基本的功能是返回子结果集的显示名，文件名，包含记录数量，供前端显示在界面上
    // 较为深入的功能是，将子结果集按照key排序归并，而显示出二级条目和数量。二级结果集是子结果集的子结果集

    // TODO: 如何及时清理Task对象，避免内存过度膨胀? 是否仅保存最新10个Task对象?
    void GetFilterInfo(
        string strResultsetName,
        string strSelected,
        string strLang)
    {
        string strError = "";
        GetFilterInfo result_info = new GetFilterInfo();

        if (string.IsNullOrEmpty(strResultsetName) == true)
        {
            strError = "结果集名不应为空";
            goto ERROR1;
        }
#if NO
        Hashtable result_table = null;
        string strFilterFileName = PathUtil.MergePath(app.DataDir, "cfgs/facet.fltx");
        int nRet = ResultsetFilter.DoFilter(
            app,
            sessioninfo.Channel,
            strResultsetName,
            strFilterFileName,
            1000,
            ref result_table,
            out strError);
        if (nRet == -1)
            goto ERROR1;
#endif
        FilterTask t = sessioninfo.FindFilterTask(strResultsetName);    // Task对象是利用Session内结果集名来进行管理的
        if (t == null)
        {
            // 如果一个结果集还没有被后台任务处理，就立即启动一个后台任务
            t = new FilterTask();
            sessioninfo.SetFilterTask(strResultsetName, t);

            string strGlobalResultSetName = "";
            bool bShare = false;

            if (strResultsetName[0] == '#')
                strGlobalResultSetName = strResultsetName.Substring(1);
            else
            {
                // 构造全局结果集名
                strGlobalResultSetName = sessioninfo.GetHashCode() + "_" + strResultsetName;

                // 先把结果集共享
                // 管理结果集
                // parameters:
                //      strAction   share/remove 分别表示共享为全局结果集对象/删除全局结果集对象
                long lRet = sessioninfo.Channel.ManageSearchResult(
                    null,
                    "share",
                    strResultsetName,
                    strGlobalResultSetName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                bShare = true;
            }

            FilterTaskInput i = new FilterTaskInput();
            i.App = app;
            i.FilterFileName = PathUtil.MergePath(app.DataDir, "cfgs/facet.fltx");
            i.ResultSetName = strGlobalResultSetName;
            i.ShareResultSet = bShare;
            i.SessionInfo = sessioninfo;
            i.TaskName = strResultsetName;  // Task对象是利用Session内结果集名来进行管理的
            i.MaxCount = 1000;
            // i.aggregation_names = new List<string>() {"author"};

            XmlDocument def_dom = GetFacetDefDom(out strError);
            if (def_dom == null)
                goto ERROR1;
            i.DefDom = def_dom;

            ThreadPool.QueueUserWorkItem(t.ThreadPoolCallBack, i);
            strError = "#pending";  // 表示正在处理，希望前端稍后重新访问
            goto ERROR1;
        }
        else
        {
            if (t.TaskState == TaskState.Processing)
            {
                if (t.ProgressRange != 0)
                    result_info.ProgressValue = (int)(((double)t.ProgressValue / (double)t.ProgressRange) * 100);
                strError = "#pending";  // 表示正在处理，希望前端稍后重新访问
                goto ERROR1;
            }
            if (string.IsNullOrEmpty(t.ErrorInfo) == false)
            {
                strError = t.ErrorInfo;
                goto ERROR1;
            }

#if NO
            string[] names = new string[t.ResultTable.Keys.Count];
            t.ResultTable.Keys.CopyTo(names, 0);
            Array.Sort(names);

            List<FilterInfo> infos = new List<FilterInfo>();
            foreach (string strName in names)
            {
                KeyValueCollection items = (KeyValueCollection)t.ResultTable[strName];
                FilterInfo info = new FilterInfo();
                info.Name = strName;
                if (items != null)
                    info.Count = items.Count.ToString();
                else
                    info.Count = "0";

                infos.Add(info);
            }
            result_info.Items = new FilterInfo[infos.Count];
            infos.CopyTo(result_info.Items);
#endif

#if NO
            Hashtable keyname_table = null;
            // 获得名称对照表
            // parameters:
            //      keyname_table   keyname --> 当前语言的名称
            int nRet = ResultsetFilter.GetKeyNameTable(
                app,
                PathUtil.MergePath(app.DataDir, "cfgs/facet.fltx"),
                t.ResultItems,
                string.IsNullOrEmpty(strLang) == true ? Thread.CurrentThread.CurrentUICulture.Name : strLang,
                out keyname_table,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            long lHitCount = MyWebPage.GetServerResultCount(sessioninfo, strResultsetName);

            XmlDocument def_dom = GetFacetDefDom(out strError);
            if (def_dom == null)
                goto ERROR1;
            this.m_facetDom = def_dom;

            this.m_strLang = strLang;

            try
            {
                // 创建FilterInfo数组
                result_info.Items = ResultsetFilter.BuildFilterInfos(
                    strResultsetName,
                    lHitCount,
                    strSelected,
                    GetKeyNameCaption,
                    t.ResultItems,
                    sessioninfo.GetTempDir(),
                    10);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            if (t.HitCount > 1000)
            {
                result_info.Comment = "分面导航只提取了当前结果集的前 1000 条记录";
            }
        }


        // 返回一级节点的名字和包含记录数量
        this.Response.Write(MyWebPage.GetResultString(result_info));
        this.Response.End();
        return;
    ERROR1:
        result_info.ErrorString = strError;
        this.Response.Write(MyWebPage.GetResultString(result_info));
        this.Response.End();
    }

    XmlDocument GetFacetDefDom(out string strError)
    {
        strError = "";

        XmlDocument def_dom = (XmlDocument)app.GetParam("facetdef");
        if (def_dom == null)
        {
            def_dom = new XmlDocument();
            try
            {
                def_dom.Load(PathUtil.MergePath(app.DataDir, "cfgs/facetdef.xml"));
            }
            catch (Exception ex)
            {
                strError = "facetdef.xml 装入 XMLDOM 出错: " + ex.Message;
                return null;
            }
            app.SetParam("facetdef", def_dom);
        }

        return def_dom;
    }

    XmlDocument m_facetDom = null;
    string m_strLang = "";

    // 回调函数，获得名称的语言字符串
    string GetKeyNameCaption(string strID)
    {
        if (this.m_facetDom == null)
            goto NOTFOUND;
        XmlNode node = m_facetDom.DocumentElement.SelectSingleNode("facet[@id='" + strID + "']");
        if (node == null)
            goto NOTFOUND;
        int index = IndexOf(node.ParentNode.ChildNodes, node);

        string strValue = DomUtil.GetCaption(this.m_strLang, node);
        if (string.IsNullOrEmpty(strValue) == true)
            goto NOTFOUND;

        return "{"+index.ToString().PadLeft(2, '0')+"}" + strValue;
    NOTFOUND:
        return strID;
    }

    static int IndexOf(XmlNodeList nodes, XmlNode node)
    {
        int i = 0;
        foreach (XmlNode cur in nodes)
        {
            if (cur == node)
                return i;
            i++;
        }

        return -1;
    }
}

public class GetFilterInfo
{
    public FilterInfo[] Items = null;

    public string Comment = "";
    public int PageSize = 10;   // 每页记录数

    public int ProgressValue = 0;   // 0-100
    public string ErrorString = "";
}

