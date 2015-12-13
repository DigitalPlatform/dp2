using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;

using DigitalPlatform;	// Stop类
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
// using DigitalPlatform.Range;

// using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和栏目存储管理有关的代码
    /// </summary>
    public partial class OpacApplication
    {
        // CommentColumn专用锁。控制创建、更新、显示时间对存储结构的宏观存取操作
        public ReaderWriterLock m_lockCommentColumn = new ReaderWriterLock();
        public int m_nCommentColumnLockTimeout = 5000;	// 5000=5秒

        // 存储结构
        public ColumnStorage CommentColumn = null;

        // 存储文件名
        string StorageFileName = "";

        private void CloseCommentColumn()
        {
            if (this.CommentColumn != null)
            {
                string strTemp1;
                string strTemp2;
                this.CommentColumn.Detach(out strTemp1,
                    out strTemp2);
                this.CommentColumn = null;
            }
        }

        // 装载栏目存储
        private int LoadCommentColumn(
            string strStorageFileName,
            out string strError)
        {
            strError = "";

            this.StorageFileName = strStorageFileName;

            try
            {
                if (this.CommentColumn == null)
                    this.CommentColumn = new ColumnStorage();
                else
                {
                    // 2006/7/6
                    string strTemp1;
                    string strTemp2;
                    this.CommentColumn.Detach(out strTemp1,
                        out strTemp2);
                    this.CommentColumn = null;

                    this.CommentColumn = new ColumnStorage();
                }

                try
                {
                    this.CommentColumn.Attach(strStorageFileName,
                        strStorageFileName + ".index");
                }
                catch (Exception ex)
                {
                    strError = "Attach 文件 " + strStorageFileName + " 和索引失败 :" + ex.Message;
                    return -1;
                }
                return 0;
            }
            catch /*(System.ApplicationException ex)*/
            {
                strError = "栏目暂时被锁定。请稍后再试。";
                return -1;
            }
        }

        // [外部调用]
        // 创建内存和物理存储对象
        public int CreateCommentColumn(
            SessionInfo sessioninfo,
            System.Web.UI.Page page,
            out string strError)
        {
            this.m_lockCommentColumn.AcquireWriterLock(m_nCommentColumnLockTimeout);
            try
            {
                strError = "";
                int nRet = 0;

                // TODO: 如何获得应用服务器帐户的权限字符串?

                if (String.IsNullOrEmpty(sessioninfo.UserID) == true
    || StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
                {
                    strError = "当前帐户不具备 managecache 权限，不能创建栏目缓存";
                    return -1;
                }

                this.CloseCommentColumn();

                if (page != null
                    && page.Response.IsClientConnected == false)	// 灵敏中断
                {
                    strError = "中断";
                    return -1;
                }

                if (this.CommentColumn == null)
                    this.CommentColumn = new ColumnStorage();

                this.CommentColumn.ReadOnly = false;
                this.CommentColumn.m_strBigFileName = this.StorageFileName;
                this.CommentColumn.m_strSmallFileName = this.StorageFileName + ".index";

                this.CommentColumn.Open(true);
                this.CommentColumn.Clear();
                // 检索
                nRet = SearchTopLevelArticles(
                    sessioninfo,
                    page,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 排序
                if (page != null)
                {
                    page.Response.Write("--- begin sort ...<br/>");
                    page.Response.Flush();
                }

                DateTime time = DateTime.Now;

                this.CommentColumn.Sort();

                if (page != null)
                {
                    TimeSpan delta = DateTime.Now - time;
                    page.Response.Write("sort end. time=" + delta.ToString() + "<br/>");
                    page.Response.Flush();
                }

                // 保存物理文件
                string strTemp1;
                string strTemp2;
                this.CommentColumn.Detach(out strTemp1,
                    out strTemp2);

                this.CommentColumn.ReadOnly = true;

                this.CloseCommentColumn();

                // 重新装载
                nRet = LoadCommentColumn(
                    this.StorageFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                this.m_lockCommentColumn.ReleaseWriterLock();
            }
        }

        // 检索顶层文章
        // return:
        //		-1	error
        //		其他 命中数
        private int SearchTopLevelArticles(
            SessionInfo sessioninfo,
            System.Web.UI.Page page,
            out string strError)
        {
            strError = "";

            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
            {
                strError = "中断";
                return -1;
            }

            // 检索全部评注文章 一定时间范围内的?
            List<string> dbnames = new List<string>();
            for (int i = 0; i < this.ItemDbs.Count; i++)
            {
                ItemDbCfg cfg = this.ItemDbs[i];
                string strDbName = cfg.CommentDbName;
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;
                dbnames.Add(strDbName);
            }

            DateTime now = DateTime.Now;
            DateTime oneyearbefore = now - new TimeSpan(365, 0, 0, 0);
            string strTime = DateTimeUtil.Rfc1123DateTimeString(oneyearbefore.ToUniversalTime());

            string strQueryXml = "";
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];
                string strOneQueryXml = "<target list='" + strDbName + ":" + "最后修改时间'><item><word>"    // <order>DESC</order>
                    + strTime + "</word><match>exact</match><relation>" + StringUtil.GetXmlStringSimple(">=") + "</relation><dataType>number</dataType><maxCount>"
                    + "-1"// Convert.ToString(m_nMaxLineCount)
                    + "</maxCount></item><lang>zh</lang></target>";

                if (i > 0)
                    strQueryXml += "<operator value='OR' />";
                strQueryXml += strOneQueryXml;
            }

            if (dbnames.Count > 0)
                strQueryXml = "<group>" + strQueryXml + "</group>";

            if (page != null)
            {
                page.Response.Write("--- begin search ...<br/>");
                page.Response.Flush();
            }

            DateTime time = DateTime.Now;

            long nRet = sessioninfo.Channel.Search(
                null,
                strQueryXml,
                "default",
                "", // outputstyle
                out strError);
            if (nRet == -1)
            {
                strError = "检索时出错: " + strError;
                return -1;
            }

            TimeSpan delta = DateTime.Now - time;
            if (page != null)
            {
                page.Response.Write("search end. hitcount=" + nRet.ToString() + ", time=" + delta.ToString() + "<br/>");
                page.Response.Flush();
            }


            if (nRet == 0)
                return 0;	// not found



            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
            {
                strError = "中断";
                return -1;
            }
            if (page != null)
            {
                page.Response.Write("--- begin get search result ...<br/>");
                page.Response.Flush();
            }

            time = DateTime.Now;

            List<string> aPath = null;
            nRet = sessioninfo.Channel.GetSearchResult(
                null,
                "default",
                0,
                -1,
                "zh",
                out aPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获得检索结果时出错: " + strError;
                return -1;
            }

            if (page != null)
            {
                delta = DateTime.Now - time;
                page.Response.Write("get search result end. lines=" + aPath.Count.ToString() + ", time=" + delta.ToString() + "<br/>");
                page.Response.Flush();
            }

            if (aPath.Count == 0)
            {
                strError = "获取的检索结果为空";
                return -1;
            }

            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
            {
                strError = "中断";
                return -1;
            }


            if (page != null)
            {
                page.Response.Write("--- begin build storage ...<br/>");
                page.Response.Flush();
            }

            time = DateTime.Now;


            this.CommentColumn.Clear();	// 清空集合

            // 加入新行对象。新行对象中，只初始化了m_strRecPath参数
            for (int i = 0; i < Math.Min(aPath.Count, 1000000); i++)	// <Math.Min(aPath.Count, 10)
            {
                Line line = new Line();
                // line.Container = this;
                line.m_strRecPath = aPath[i];

                nRet = line.InitialInfo(
                    page,
                    sessioninfo.Channel,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return -1;	// 灵敏中断


                TopArticleItem item = new TopArticleItem();
                item.Line = line;
                this.CommentColumn.Add(item);

                if (page != null
                    && (i % 100) == 0)
                {
                    page.Response.Write("process " + Convert.ToString(i) + "<br/>");
                    page.Response.Flush();
                }

            }

            if (page != null)
            {
                delta = DateTime.Now - time;
                page.Response.Write("build storage end. time=" + delta.ToString() + "<br/>");
                page.Response.Flush();
            }

            return 1;
        }

        // [外部调用]
        // 修改评注记录后，更新栏目存储结构
        // parameters:
        //      strAction   动作。change/delete/new
        // return:
        //      -2   栏目缓存尚未创建,因此无从更新
        //		-1	error
        //		0	not found line object
        //		1	succeed
        public int UpdateLine(
            System.Web.UI.Page page,
            string strAction,
            string strRecPath,
            string strXml,
            out string strError)
        {
            strError = "";

            if (this.CommentColumn == null
    || this.CommentColumn.Opened == false)
            {
                strError = "尚未创建栏目缓存...";
                return -2;
            }

            this.m_lockCommentColumn.AcquireWriterLock(m_nCommentColumnLockTimeout);
            try
            {


                int nIndex = -1;
                int i = 0;
                Line line = null;

                if (strAction == "change" || strAction == "delete")
                {
                    // 在Storage中找
                    // 需要写锁定
                    for (i = 0; i < this.CommentColumn.Count; i++)
                    {
                        string strCurrentRecPath = this.CommentColumn.GetItemRecPath(i);

                        // 判断两个下属库路径是否等同
                        if (this.IsSameItemRecPath(
                            "comment",
                            strCurrentRecPath,
                            strRecPath) == true)
                        // if (strCurrentRecPath == strRecPath)
                        {
                            nIndex = i;
                            line = ((TopArticleItem)this.CommentColumn[nIndex]).Line;
                            Debug.Assert(line.m_strRecPath == strRecPath, "");

                            this.CommentColumn.RemoveAt(nIndex);

                            if (strAction == "delete")
                                return 1;
                            break;  //  goto FOUND;
                        }
                    }

                    if (strAction == "delete")
                        return 0;

                    if (nIndex == -1)
                    {
                        strError = "路径 '" + strRecPath + "' 在当前 CommentColumn 数组中没有找到";
                        return -1;
                    }
                }
                else if (strAction == "new")
                {
                    line = new Line();
                    line.m_strRecPath = strRecPath;
                }
                else
                {
                    strError = "未知的strAction值 '" + strAction + "'";
                    return -1;
                }

                // FOUND:
                if (strAction == "delete")
                    return 1;

                int nRet = line.ProcessXml(
                    null,	// page,	这里至关重要，不能允许灵敏中断
                    strXml,
                    out strError);
                if (nRet == -1)
                {
                    // 将此情况写入错误日志
                    this.WriteErrorLog("在UpdateLine()函数中，调用line.ProcessXml()发生错误, 记录路径=" + line.m_strRecPath + ")。这将导致栏目主页面中，该记录从显示中丢失。详细原因：" + strError);
                    return -1;
                }

                {
                    // 如果插入位置在Storage范围内
                    TopArticleItem item = new TopArticleItem();
                    item.Line = line;
                    this.CommentColumn.Insert(0,
                        item);
                }

                return 1;
            }
            finally
            {
                this.m_lockCommentColumn.ReleaseWriterLock();
            }
        }
    }
}
