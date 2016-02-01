using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Server
{

    // 多个聊天室栏目的集合
    public class ChatRoomCollection : List<ChatRoom>, IDisposable
    {
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        public string DataDirectory = "";

        public int PicMaxWidth = 800;
        public int PicMaxHeight = 8000;

        public void Dispose()
        {
            this.Clear();
        }

        // 2016/1/23
        public new void Clear()
        {
            foreach(ChatRoom room in this)
            {
                if (room != null)
                    room.Close();
            }
            base.Clear();
        }

        public ChatRoom GetChatRoom(
    string strRights,
    string strName,
    bool bLock = true)
        {
            ChatRoom room = __GetChatRoom(strName, bLock);
            if (room == null)
                return null;
            // 是否属于组
            if (MatchGroup(room.GroupList, strRights) == false)
                return null;
            return room;
        }

        // parameters:
        //      strName 栏目名字。也可以用 @1 这样的形态
        public ChatRoom __GetChatRoom(
            string strName,
            bool bLock = true)
        {
            if (bLock == true)
                m_lock.EnterReadLock();
            try
            {
                if (string.IsNullOrEmpty(strName) == false
                    && strName[0] == '@')
                {
                    int index = 0;
                    if (Int32.TryParse(strName.Substring(1).Trim(), out index) == false)
                        return null;
                    index--;    // 从1开始计数
                    if (index < 0 || index >= this.Count)
                        return null;
                    return this[index];
                }
                foreach (ChatRoom room in this)
                {
                    if (String.Compare(room.Name, strName, true) == 0)
                        return room;
                }
            }
            finally
            {
                if (bLock == true)
                    m_lock.ExitReadLock();
            }

            return null;
        }

        public int CreateChatRoom(
    string strRoomName,
    out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strRoomName) == true)
            {
                strError = "要创建的栏目名不能为空";
                return -1;
            }

            m_lock.EnterWriteLock();
            try
            {
                ChatRoom room = null;
                // 查重
                room = __GetChatRoom(strRoomName, false);
                if (room != null)
                {
                    strError = "名字为 '" + strRoomName + "' 的栏目已经存在...";
                    return -1;
                }

                // 创建一个栏目
                room = new ChatRoom();
                room.Name = strRoomName;
                this.Add(room);

                int nRet = room.Initial(PathUtil.MergePath(this.DataDirectory, room.Name),
        out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                m_lock.ExitWriteLock();
            }

            return 0;
        }

        public int DeleteChatRoom(
string strRoomName,
out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strRoomName) == true)
            {
                strError = "要删除的栏目名不能为空";
                return -1;
            }

            m_lock.EnterWriteLock();
            try
            {
                ChatRoom room = null;
                // 查重
                room = __GetChatRoom(strRoomName, false);
                if (room == null)
                {
                    strError = "名字为 '" + strRoomName + "' 的栏目并不存在...";
                    return -1;
                }

                room.Close();
                Directory.Delete(room.DataDirectory, true);
                this.Remove(room);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }

            return 0;
        }

        public int IsEditor(
            string strRoomName,
            string strUserID,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strRoomName) == true)
            {
                strError = "要观察的栏目名不能为空";
                return -1;
            }

            m_lock.EnterReadLock();
            try
            {
                ChatRoom room = null;
                // 查重
                room = __GetChatRoom(strRoomName, false);
                if (room == null)
                {
                    strError = "名字为 '" + strRoomName + "' 的栏目并不存在...";
                    return -1;
                }

                if (room.EditorList.IndexOf(strUserID) != -1)
                    return 1;   // 是版主身份

                return 0;
            }
            finally
            {
                m_lock.ExitReadLock();
            }

            return 0;
        }

        // 根据XML定义初始化数组
        public int Initial(XmlNode nodeDef,
            string strDirectory,
            out string strError)
        {
            strError = "";

            this.Close();

            PathUtil.CreateDirIfNeed(strDirectory);	// 确保目录创建
            this.DataDirectory = strDirectory;

            m_lock.EnterWriteLock();
            try
            {
                this.Clear();

                // 创建一个缺省的栏目
                if (nodeDef == null)
                {
                    ChatRoom room = new ChatRoom();
                    room.Name = "default";
                    this.Add(room);

                    int nRet = room.Initial(PathUtil.MergePath(strDirectory, room.Name),
            out strError);
                    if (nRet == -1)
                        return -1;

                    return 0;
                }

                int nValue = -1;
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                DomUtil.GetIntegerParam(nodeDef,
                    "picMaxWidth",
                    800,
                    out nValue,
                    out strError);
                this.PicMaxWidth = nValue;

                nValue = -1;
                // return:
                //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                //      0   正常获得明确定义的参数值
                //      1   参数没有定义，因此代替以缺省参数值返回
                DomUtil.GetIntegerParam(nodeDef,
                    "picMaxHeight",
                    8000,
                    out nValue,
                    out strError);
                this.PicMaxHeight = nValue;

                XmlNodeList nodes = nodeDef.SelectNodes("chatRoom");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strName = DomUtil.GetAttr(node, "name");
                    if (string.IsNullOrEmpty(strName) == true)
                        continue;

                    string strEditors = DomUtil.GetAttr(node, "editors");
                    string strGroups = DomUtil.GetAttr(node, "groups");

                    ChatRoom room = null;
                    /*
                    room = GetChatRoom(strName, false);
                    if (room != null)
                    {
                        strError = "名字为 '"+strName+"' 的栏目被重复定义了";
                        return -1;
                    }
                     * */

                    room = new ChatRoom();
                    room.Name = strName;
                    room.EditorList = StringUtil.SplitList(strEditors);
                    room.GroupList = StringUtil.SplitList(strGroups);
                    this.Add(room);

                    int nRet = room.Initial(PathUtil.MergePath(strDirectory, strName),
            out strError);
                    if (nRet == -1)
                        return -1;

                }
            }
            finally
            {
                m_lock.ExitWriteLock();
            }

            return 0;
        }

        // 获得栏目的XML定义
        public int GetDef(
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<chatRoomDef />");

            m_lock.EnterReadLock();
            try
            {
                foreach (ChatRoom room in this)
                {
                    XmlNode node = dom.CreateElement("chatRoom");
                    dom.DocumentElement.AppendChild(node);
                    DomUtil.SetAttr(node, "name", room.Name);
                    DomUtil.SetAttr(node, "editors", StringUtil.MakePathList(room.EditorList));
                    DomUtil.SetAttr(node, "groups", StringUtil.MakePathList(room.GroupList));
                }

                DomUtil.SetAttr(dom.DocumentElement, "picMaxWidth", this.PicMaxWidth.ToString());
                DomUtil.SetAttr(dom.DocumentElement, "picMaxHeight", this.PicMaxHeight.ToString());

            }
            finally
            {
                m_lock.ExitReadLock();
            }

            // strXml = dom.DocumentElement.OuterXml;
            strXml = DomUtil.GetIndentXml(dom.DocumentElement);

            return 0;
        }

        public void Close()
        {
            m_lock.EnterReadLock();
            try
            {
                foreach (ChatRoom room in this)
                {
                    room.Close();
                }
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public void Flush()
        {
            m_lock.EnterReadLock();
            try
            {
                foreach (ChatRoom room in this)
                {
                    room.Flush();
                }
            }
            finally
            {
                m_lock.ExitReadLock();
            }
        }

        public static bool MatchGroup(List<string> groups,
            string strRights)
        {
            if (groups == null || groups.Count == 0)
                return true;    // 如果没有定义group属性，则是公开的

            if (string.IsNullOrEmpty(strRights) == true)
                return false;   // 用户没有任何组定义

            foreach (string s in groups)
            {
                if (StringUtil.IsInList(s, strRights) == true)
                    return true;
            }

            return false;
        }

        public List<string> GetRoomNames(string strRights,
            bool bLock = true)
        {
            if (bLock == true)
                m_lock.EnterReadLock();
            try
            {
                List<string> results = new List<string>();
                foreach (ChatRoom room in this)
                {
                    if (MatchGroup(room.GroupList, strRights) == false)
                        continue;
                    results.Add(room.Name);
                }

                return results;
            }
            finally
            {
                if (bLock == true)
                    m_lock.ExitReadLock();
            }

            return null;
        }
    }

    // 一个聊天室栏目
    public class ChatRoom
    {
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        public string Name = "";    // 栏目的名字

        // 2012/10/13
        public List<string> GroupList = new List<string>();    // 版主列表

        // 2012/5/18
        public List<string> EditorList = new List<string>();    // 版主列表
        // Stream m_stream = null;

        DpResultSet m_resultset = null;

        public string ContentFileName = "";
        public long ContentFileVersion = 0;

        public string DataDirectory = "";
        public string Date = "";    // 当天的日期。8字符

        public string PrepareAttachFileName(string strExtention)
        {
            Debug.Assert(string.IsNullOrEmpty(this.DataDirectory) == false, "");

            string strDate = DateTimeUtil.DateTimeToString8(DateTime.Now);
            string strFileName = PathUtil.MergePath(DataDirectory, strDate);

            return strFileName + "." + Guid.NewGuid().ToString() + strExtention;
        }

        int PrepareContentFileName(out string strError)
        {
            strError = "";

            Debug.Assert(string.IsNullOrEmpty(this.DataDirectory) == false, "");

            string strDate = DateTimeUtil.DateTimeToString8(DateTime.Now);
            string strFileName = PathUtil.MergePath(DataDirectory, strDate);
            if (this.ContentFileName != strFileName)
            {
                this.Close();
                this.ContentFileName = strFileName;

                try
                {
                    m_resultset = new DpResultSet(false, false);

                    // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                    if (File.Exists(this.ContentFileName) == true)
                    {
                        m_resultset.Attach(this.ContentFileName,
            this.ContentFileName + ".index");
                    }
                    else
                    {
                        m_resultset.Create(this.ContentFileName,
            this.ContentFileName + ".index");
                    }

                    this.ContentFileVersion = DateTime.Now.Ticks;
                    this.Date = strDate;
                }
                catch (Exception ex)
                {
                    strError = "打开或创建结果集文件 '" + this.ContentFileName + "' (和.index) 发生错误: " + ex.Message;
                    this.ContentFileName = "";
                    return -1;
                }
            }
            else
            {
                Debug.Assert(m_resultset != null, "");
            }

            return 0;
        }

        // 初始化
        public int Initial(string strDirectory,
            out string strError)
        {
            strError = "";

            this.DataDirectory = strDirectory;

            PathUtil.CreateDirIfNeed(strDirectory);	// 确保目录创建

            // 打开当天的内容文件
            int nRet = PrepareContentFileName(out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        public void Close()
        {
            if (this.m_resultset != null)
            {
                string strTemp1 = "";
                string strTemp2 = "";
                m_resultset.Detach(out strTemp1,
                    out strTemp2);

                this.m_resultset = null;
            }

            this.ContentFileName = "";
        }

        // 关闭文件后重新打开
        public void Flush()
        {
            m_lock.EnterWriteLock();
            try
            {
                if (this.m_resultset != null)
                {
                    m_resultset.Flush();
                }
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        // 追加文本
        // 多线程：安全
        internal void AppendText(string strRefID,
            string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return;
            if (m_resultset == null)
                return;

            m_lock.EnterWriteLock();
            try
            {
                // 打开当天的内容文件
                string strError = "";
                int nRet = PrepareContentFileName(out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                DpRecord record = new DpRecord(strRefID);
                record.BrowseText = strText;
                m_resultset.Add(record);
            }
            finally
            {
                m_lock.ExitWriteLock();
            }
        }

        const int MAX_COUNT_PER_GET = 100;  // 每次最多可以获取的行数

        // 获得任务当前信息
        // 多线程：安全
        // parameters:
        //      bDisplayAllIP   是否显示全部IP地址。如果为false，表示只显示访客的IP地址，并且是掩盖部分的
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        public int GetInfo(
            string strDate,
            long nStart,
            long nCount,
            bool bDisplayAllIP,
            out ChatInfo info,
            out string strError)
        {
            strError = "";
            info = null;

            if (strDate != this.Date)
            {
                // 要获得不是当天的内容
                string strFileName = PathUtil.MergePath(DataDirectory, strDate);
                if (File.Exists(strFileName) == false)
                {
                    strError = "日期为 '" + strDate + "' 的文件不存在";
                    return 0;
                }

                DpResultSet resultset = new DpResultSet(false, false);

                resultset.Attach(strFileName,
    strFileName + ".index");
                try
                {
                    info = GetInfo(
    resultset,
    nStart,
    nCount,
    bDisplayAllIP);
                }
                finally
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }

                FileInfo fi = new FileInfo(strFileName);
                info.ResultVersion = fi.LastWriteTimeUtc.Ticks;
                return 1;
            }
            else
            {
                m_lock.EnterReadLock();
                try
                {
                    // 打开当天的内容文件
                    int nRet = PrepareContentFileName(out strError);
                    if (nRet == -1)
                        return -1;

#if NO
                    ChatInfo info = new ChatInfo();
                    info.Name = this.Name;

                    long nMax = 0;
                    if (nCount == -1)
                        nMax = m_resultset.Count;
                    else
                        nMax = Math.Min(m_resultset.Count, nStart + nCount);

                    if (nMax - nStart > MAX_COUNT_PER_GET)
                        nMax = nStart + MAX_COUNT_PER_GET;

                    string strResult = "";
                    for (long i = nStart; i < nMax; i++)
                    {
                        DpRecord record = m_resultset[i];
                        if (bDisplayAllIP == true)
                            strResult += record.BrowseText;
                        else
                            strResult += RemoveIP(record.BrowseText);
                    }

                    info.ResultText = strResult;
                    info.NextStart = nMax;   // 结束位置
                    info.TotalLines = m_resultset.Count;
                    info.ResultVersion = this.ContentFileVersion;

                    return info;
#endif
                    info = GetInfo(
this.m_resultset,
nStart,
nCount,
bDisplayAllIP);
                    return 1;
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }
        }

        ChatInfo GetInfo(
            DpResultSet resultset,
            long nStart,
            long nCount,
            bool bDisplayAllIP)
        {
            ChatInfo info = new ChatInfo();
            info.Name = this.Name;

            long nMax = 0;
            if (nCount == -1)
                nMax = resultset.Count;
            else
                nMax = Math.Min(resultset.Count, nStart + nCount);

            if (nMax - nStart > MAX_COUNT_PER_GET)
                nMax = nStart + MAX_COUNT_PER_GET;

            string strResult = "";
            for (long i = nStart; i < nMax; i++)
            {
                DpRecord record = resultset[i];
                if (bDisplayAllIP == true)
                    strResult += record.BrowseText;
                else
                    strResult += MaskIP(record.BrowseText);
            }

            info.ResultText = strResult;
            info.NextStart = nMax;   // 结束位置
            info.TotalLines = resultset.Count;
            if (resultset == this.m_resultset)
                info.ResultVersion = this.ContentFileVersion;
            else
            {
                info.ResultVersion = 0; // 暂时无法给出
            }

            return info;
        }

        // 对访客的条目的IP进行加工，隐藏部分内容
        static string MaskIP(string strXml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                return "消息XML装入DOM时出错: " + ex.Message;
            }
            XmlNode node = dom.DocumentElement.SelectSingleNode("div[@class='ip']");
            if (node == null)
                return strXml;

            string strIP = "";
            XmlNode nodeUserID = dom.DocumentElement.SelectSingleNode("div[@class='userid']");
            if (nodeUserID != null)
            {
                string strUserID = nodeUserID.InnerText;
                if (StringUtil.HasHead(strUserID, "(访客)") == true)
                {
                    strIP = node.InnerText;

                    if (strIP == "::1")
                        strIP = "localhost";
                    else
                    {
                        // 加工
                        string[] parts = strIP.Split(new char[] { '.' });
                        if (parts.Length == 4)
                        {
                            strIP = parts[0] + ".*.*." + parts[3];
                        }
                    }
                }

            }

            node.InnerText = strIP;
            return dom.DocumentElement.OuterXml;
        }

        // 删除一个事项
        // 只是索引压缩了,数据文件没有处理。注意,也并没有删除事项所关联的图像文件
        // 多线程：安全
        // parameters:
        //      bChangeVersion  是否修改版本号
        //      lNewVersion 返回修改后的新版本号。如果bChangeVersion==false, 此参数也返回没有发生过变化的版本号
        //      strContent  返回已经删除的记录的内容
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   已经删除
        public int DeleteItem(string strRefID,
            string strDate,
            bool bChangeVersion,
            out long lNewVersion,
            out string strContent,
            out string strError)
        {
            strError = "";
            lNewVersion = 0;
            strContent = "";

            if (strDate != this.Date)
            {
                // 要获得不是当天的内容
                string strFileName = PathUtil.MergePath(DataDirectory, strDate);
                if (File.Exists(strFileName) == false)
                {
                    strError = "日期为 '" + strDate + "' 的文件不存在";
                    return 0;
                }

                int nRet = 0;

                DpResultSet resultset = new DpResultSet(false, false);

                resultset.Attach(strFileName,
    strFileName + ".index");
                try
                {
                    nRet = DeleteItem(resultset,
                strRefID,
                bChangeVersion,
                false,
                out lNewVersion,
                out strContent,
                out strError);
                }
                finally
                {
                    string strTemp1 = "";
                    string strTemp2 = "";
                    resultset.Detach(out strTemp1,
                        out strTemp2);
                }


                FileInfo fi = new FileInfo(strFileName);
                lNewVersion = fi.LastWriteTimeUtc.Ticks;
                return nRet;
            }
            else
            {
                m_lock.EnterReadLock();
                try
                {
                    // 打开当天的内容文件
                    int nRet = PrepareContentFileName(out strError);
                    if (nRet == -1)
                        return -1;

                    return DeleteItem(this.m_resultset,
                strRefID,
                bChangeVersion,
                false,
                out lNewVersion,
                out strContent,
                out strError);
                }
                finally
                {
                    m_lock.ExitReadLock();
                }
            }

#if NO
            /////
            m_lock.EnterWriteLock();
            try
            {
                // 打开当天的内容文件
                int nRet = PrepareContentFileName(out strError);
                if (nRet == -1)
                    return -1;

                return DeleteItem(this.m_resultset,
            strRefID,
            out lNewVersion,
            out strError);


            }
            finally
            {
                m_lock.ExitWriteLock();
            }
#endif
        }

        // parameters:
        //      bChangeVersion  是否修改版本号
        //      lNewVersion 返回修改后的新版本号。如果bChangeVersion==false, 此参数也返回没有发生过变化的版本号
        //      strContent  返回已经删除的记录的内容
        int DeleteItem(DpResultSet resultset,
            string strRefID,
            bool bChangeVersion,
            bool bAllowDeleteNotifyItem,
            out long lNewVersion,
            out string strContent,
            out string strError)
        {
            strError = "";
            lNewVersion = 0;
            strContent = "";

            for (long i = 0; i < resultset.Count; i++)
            {
                DpRecord record = resultset[i];

                if (record.ID == strRefID)
                {
                    strContent = record.BrowseText;
                    if (bAllowDeleteNotifyItem == false
                        && string.IsNullOrEmpty(strContent) == false)
                    {
                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strContent);
                        }
                        catch (Exception ex)
                        {
                            strError = "记录内容装入XMLDOM时出错: " + ex.Message;
                            return -1;
                        }
                        string strClass = DomUtil.GetAttr(dom.DocumentElement, "class");
                        strClass.Replace(" ", ",");
                        if (StringUtil.IsInList("notify", strClass) == true)
                        {
                            strError = "notify事项不允许删除";
                            return -1;
                        }
                    }

                    resultset.RemoveAt((int)i);
                    if (resultset == this.m_resultset)
                    {
                        if (bChangeVersion == true)
                            this.ContentFileVersion = DateTime.Now.Ticks;   // 文件版本发生变化

                        lNewVersion = this.ContentFileVersion;
                    }
                    return 1;   // deleted
                }
            }

            strError = "refid为 '" + strRefID + "' 的事项没有找到";
            return 0;   // not found
        }
    }

    // 批处理任务信息
    [DataContract(Namespace = "http://dp2003.com/dp2opac/")]
    public class ChatInfo
    {
        // 名字
        [DataMember]
        public string Name = "";

        // 状态
        [DataMember]
        public string State = "";

        // 当前进度
        [DataMember]
        public string ProgressText = "";

        // 输出结果
        [DataMember]
        public int MaxLines = 0;
        [DataMember]
        public string ResultText = "";
        [DataMember]
        public long NextStart = 0;   // [in]起点行号 [out]下次继续获得的起点行号
        [DataMember]
        public long TotalLines = 0;  // 整个的行数

        [DataMember]
        public long ResultVersion = 0;  // 信息文件版本
    }
}
