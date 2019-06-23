using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;	// DateTimeUtil
using DigitalPlatform.Text;
// using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;


namespace DigitalPlatform.OPAC.Server
{
    // 一个标题行
    [Serializable()]
    public class Line
    {
        bool m_bInfoInitilized = false;
        // [NonSerialized]
        // public MainPage Container = null;

        // public string m_strRecID = "";
        public string m_strRecPath = "";

        // public string m_strParentID = "";

        // public string m_strArticleState = "";	// 帖子状态：精华、...。
        // 从xml记录中<state>元素获得

        // public string m_strArticleTitle = "";	// 帖子标题
        // 从xml记录中<title>元素获得


        // public string m_strAuthor = "";	// 作者
        // 从xml记录中<author>元素获得

        public DateTime m_timeCreate;	// 顶层帖子创建时间

        // public string m_strLastUpdate = "";	// 所有跟帖中，最后更新时间
        public DateTime m_timeLastUpdate;
        // 从xml记录中，<tree>元素下级所有<rec>元素中的日期属性计算而得

        // public string m_strSummary = "";

        public bool Initialized
        {
            get
            {
                return this.m_bInfoInitilized;
            }
        }

        // 从服务器端得到XML数据，初始化若干变量
        // parameters:
        //		page	如果!=null，允许灵敏中断
        // return:
        //		-1	出错
        //		0	正常结束
        //		1	被用户中断
        public int InitialInfo(
            System.Web.UI.Page page,
            LibraryChannel channel,
            out string strError)
        {
            strError = "";

            Line line = this;

            if (this.m_bInfoInitilized == true)
                return 0;

            if (String.IsNullOrEmpty(this.m_strRecPath) == true)
            {
                strError = "m_strRecPath尚未初始化";
                return -1;
            }

            string strStyle = "content,data";

            string strContent;
            string strMetaData;
            byte[] baTimeStamp;
            string strOutputPath;

            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
                return 1;

            long nRet = channel.GetRes(null,
                this.m_strRecPath,
                strStyle,
                out strContent,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获取记录 '" + this.m_strRecPath + "' 时出错: " + strError;
                return -1;
            }

            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
                return 1;

            // 处理数据
            nRet = line.ProcessXml(
                page,
                strContent,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }

            this.m_bInfoInitilized = true;
            return 0;
        }

        // 析出xml中的数据
        // parameters:
        //		page	如果!=null，允许灵敏中断
        // return:
        //		-1	出错
        //		0	正常结束
        //		1	被用户中断
        public int ProcessXml(
            System.Web.UI.Page page,
            string strXml,
            out string strError)
        {
            strError = "";

            if (page != null
                && page.Response.IsClientConnected == false)	// 灵敏中断
                return 1;


            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // this.m_strParentID = DomUtil.GetElementText(dom.DocumentElement, "parent");

            // 帖子状态：精华、...。
            // this.m_strArticleState = DomUtil.GetElementText(dom.DocumentElement, "state");

            // 帖子标题
            // this.m_strArticleTitle = DomUtil.GetElementText(dom.DocumentElement, "title");

            // 作者
            // this.m_strAuthor = DomUtil.GetElementText(dom.DocumentElement, "creator");

            // 摘要
            // this.m_strSummary = DomUtil.GetElementText(dom.DocumentElement, "description"); // ??

            XmlNode node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
            if (node != null)
            {
                string strCreateTime = DomUtil.GetAttr(node, "time");
                if (String.IsNullOrEmpty(strCreateTime) == false)
                {
                    try
                    {
                        this.m_timeCreate = DateTimeUtil.FromRfc1123DateTimeString(strCreateTime);
                    }
                    catch
                    {
                    }
                }
            }

            node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='lastContentModified']");
            if (node != null)
            {
                string strLastModifiedTime = DomUtil.GetAttr(node, "time");

                if (string.IsNullOrEmpty(strLastModifiedTime) == false)
                {
                    try
                    {
                        this.m_timeLastUpdate = DateTimeUtil.FromRfc1123DateTimeString(strLastModifiedTime);
                    }
                    catch
                    {
                    }
                }
            }
            else
                this.m_timeLastUpdate = this.m_timeCreate;


            return 0;
        }
    }


    public class TopArticleItem : Item
    {
        int m_nLength = 0;

        byte[] m_buffer = null;

        long m_ticks = 0;	// 专门从m_line成员中提出，便于排序
        // internal long m_id = 0;
        internal string m_strRecPath = "";

        long m_bColumnTop = 0;


        Line m_line = null;	// Line对象?
        public Line Line
        {
            get
            {
                return m_line;
            }
            set
            {
                m_line = value;

                // 初始化二进制内容
                using (MemoryStream s = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(s, m_line);

                    this.Length = (int)s.Length + 8 * 2 + (4 + Encoding.UTF8.GetByteCount(m_line.m_strRecPath));

                    m_buffer = new byte[(int)s.Length];
                    s.Seek(0, SeekOrigin.Begin);
                    s.Read(m_buffer, 0, m_buffer.Length);
                }

                m_ticks = m_line.m_timeLastUpdate.Ticks;

                /*
                if (StringUtil.IsInList("columntop", m_line.m_strArticleState) == true)
                    m_bColumnTop = 1;
                else
                    m_bColumnTop = 0;
                 * */
                m_bColumnTop = 0;

                if (this.m_ticks == 0)
                    throw (new Exception("ticks不能为0"));

                this.m_strRecPath = m_line.m_strRecPath;
            }
        }

        public override int Length
        {
            get
            {
                return m_nLength;
            }
            set
            {
                m_nLength = value;
            }
        }

        public override void ReadData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");

            // 读入ticks
            byte[] ticksbuffer = new byte[8];
            stream.Read(ticksbuffer, 0, 8);
            this.m_ticks = BitConverter.ToInt64(ticksbuffer, 0);

            if (this.m_ticks == 0)
                throw (new Exception("ticks不能为0"));

            // 
            stream.Read(ticksbuffer, 0, 8);
            this.m_bColumnTop = BitConverter.ToInt64(ticksbuffer, 0);


            // length of path
            byte[] lengthbuffer = new byte[4];
            stream.Read(lengthbuffer, 0, 4);
            int nLength = BitConverter.ToInt32(lengthbuffer, 0);

            Debug.Assert(nLength >= 0 && nLength < 100, "");
            byte[] textbuffer = new byte[nLength];
            stream.Read(textbuffer, 0, nLength);

            this.m_strRecPath = System.Text.Encoding.UTF8.GetString(textbuffer);


            // 读入Length个bytes的内容
            byte[] buffer = new byte[this.Length - 8 * 2 - (nLength + 4)];
            stream.Read(buffer, 0, buffer.Length);

            // 还原内存对象
            using (MemoryStream s = new MemoryStream(buffer))
            {
                BinaryFormatter formatter = new BinaryFormatter();

                m_line = (Line)formatter.Deserialize(s);
            }
        }


        public override void ReadCompareData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");

            // 读入ticks
            byte[] ticksbuffer = new byte[8];
            stream.Read(ticksbuffer, 0, 8);
            this.m_ticks = BitConverter.ToInt64(ticksbuffer, 0);

            if (this.m_ticks == 0)
                throw (new Exception("ticks不能为0"));

            // 
            stream.Read(ticksbuffer, 0, 8);
            this.m_bColumnTop = BitConverter.ToInt64(ticksbuffer, 0);


            // length of path
            byte[] lengthbuffer = new byte[4];
            stream.Read(lengthbuffer, 0, 4);
            int nLength = BitConverter.ToInt32(lengthbuffer, 0);

            Debug.Assert(nLength >= 0 && nLength < 100, "");
            byte[] textbuffer = new byte[nLength];
            stream.Read(textbuffer, 0, nLength);

            this.m_strRecPath = System.Text.Encoding.UTF8.GetString(textbuffer);

            m_line = null;	// 表示line对象不可用
        }

        public override void WriteData(Stream stream)
        {
            if (m_line == null)
            {
                throw (new Exception("m_line尚未初始化"));
            }

            if (m_buffer == null)
            {
                throw (new Exception("m_buffer尚未初始化"));
            }

            if (this.m_ticks == 0)
                throw (new Exception("ticks不能为0"));


            // 单独写入时间ticks
            byte[] buffer = BitConverter.GetBytes(this.m_ticks);
            stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(this.m_bColumnTop);
            stream.Write(buffer, 0, buffer.Length);

            /*
            buffer = BitConverter.GetBytes(this.m_id);
            stream.Write(buffer, 0, buffer.Length);
             * */
            byte[] bufferLength = new byte[4];
            byte[] bufferText = Encoding.UTF8.GetBytes(this.m_strRecPath);
            bufferLength = System.BitConverter.GetBytes((Int32)bufferText.Length);
            Debug.Assert(bufferLength.Length == 4, "");
            stream.Write(bufferLength, 0, 4);
            stream.Write(bufferText, 0, bufferText.Length);

            // 写入Length个bytes的内容
            stream.Write(m_buffer, 0, this.Length - 8 * 2 - (bufferText.Length + 4));
        }

        // 实现IComparable接口的CompareTo()方法,
        // 根据ID比较两个对象的大小，以便排序，
        // 按右对齐方式比较
        // obj: An object to compare with this instance
        // 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
        // Less than zero: This instance is less than obj.
        // Zero: This instance is equal to obj.
        // Greater than zero: This instance is greater than obj.
        // 异常: ArgumentException,obj is not the same type as this instance.
        public override int CompareTo(object obj)
        {
            TopArticleItem item = (TopArticleItem)obj;

            if (this.m_ticks == 0)
                throw (new Exception("this.ticks不能为0"));

            if (item.m_ticks == 0)
                throw (new Exception("item.ticks不能为0"));

            if (this.m_bColumnTop != item.m_bColumnTop)
            {
                if (this.m_bColumnTop != 0)
                    return (-1) * 1;
                return (-1) * (-1);
            }


            long delta = this.m_ticks - item.m_ticks;

            if (delta != 0)
            {
                if (delta < 0)
                    return (-1) * (-1);
                else
                    return (-1) * 1;
            }

            /*
            delta = this.m_id - item.m_id;
            if (delta != 0)
            {
                if (delta < 0)
                    return (-1) * (-1);
                else
                    return (-1) * 1;
            }
             * */

            return 0;
        }
    }



    /// <summary>
    /// 一个栏目的磁盘物理存储结构
    /// </summary>
    public class ColumnStorage : ItemFileBase
    {

        public ColumnStorage()
        {
            this.ReadOnly = true;
        }

        public override Item NewItem()
        {
            return new TopArticleItem();
        }

        // 是否被成功打开?
        public bool Opened
        {
            get
            {
                if (this.m_streamSmall != null
                    && this.m_streamBig != null)
                    return true;
                return false;
            }
        }

        // 快速获得事项的记录路径
        public string GetItemRecPath(Int64 nIndex)
        {
            // 加读锁
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {

                TopArticleItem item = (TopArticleItem)this.GetCompareItem(nIndex, false);

                if (item == null)
                    return "";	// error

                return item.m_strRecPath;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }
        }

    }


}
