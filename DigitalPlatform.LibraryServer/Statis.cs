using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

using System.IO;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 统计对象。负责内存缓冲统计信息
    /// </summary>
    public class Statis
    {
        public LibraryApplication App = null;

        // 当日统计记录的内存DOM
        public XmlDocument TodayDom = new XmlDocument();
        // 当前DOM所对应的日期
        public string CurrentDate = "";

        int m_nUnsavedCount = 0;

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        // 看看这一天是否存在对应的统计文件
        public bool ExistStatisFile(DateTime date)
        {
            string strFilename = this.App.StatisDir + "\\" + DateTimeUtil.DateTimeToString8(date) + ".xml";

            // 2008/11/24 changed
            FileInfo fi = new FileInfo(strFilename);

            if (fi.Exists == true && fi.Length > 0)
                return true;

            return false;
            /*
            if (File.Exists(strFilename) == true)
                return true;

            return false;
             * */
        }

        public int Initial(
            LibraryApplication app,
            out string strError)
        {
            strError = "";

            this.App = app;

            // 2013/5/11
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            { 
                LoadCurrentFile();
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            } 
            return 0;
        }

        public void Close()
        {
            if (this.TodayDom != null
                && this.CurrentDate != null)
            {
                SaveDom(true);
            }

            this.TodayDom = null;
            this.CurrentDate = "";
        }

        public static string GetCurrentDate()
        {
            DateTime now = DateTime.Now;

            return now.Year.ToString().PadLeft(4, '0')
            + now.Month.ToString().PadLeft(2, '0')
            + now.Day.ToString().PadLeft(2, '0');
        }

        void LoadCurrentFile()
        {
            DateTime now = DateTime.Now;

            this.CurrentDate = GetCurrentDate();

            string strStatisFileName = this.App.StatisDir + "\\"
                + this.CurrentDate + ".xml";

            if (this.TodayDom == null)
            {
                this.TodayDom = new XmlDocument();
            }

            try
            {
                this.TodayDom.Load(strStatisFileName);
            }
            catch(FileNotFoundException)
            {
                this.TodayDom.LoadXml("<root />");
                // 设置起始时间
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "startTime", DateTime.Now.ToString());
                // 设置结束时间
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "endTime", DateTime.Now.ToString());
            }
            catch(Exception ex) // 2013/5/11
            {
                this.TodayDom.LoadXml("<root />");
                // 设置起始时间
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "startTime", DateTime.Now.ToString());
                // 设置结束时间
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "endTime", DateTime.Now.ToString());

                string strErrorText = "Statis::LoadCurrentFile() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "error", DateTime.Now.ToString() + " " + strErrorText);
                if (this.App != null)
                    this.App.WriteErrorLog(strErrorText);
            }
        }

        void SaveDom(bool bForce)
        {
            if (bForce || m_nUnsavedCount > 100)
            {
                m_nUnsavedCount = 0;

                Debug.Assert(this.CurrentDate != "", "");

                string strStatisFileName = this.App.StatisDir + "\\"
                + this.CurrentDate + ".xml";

                this.TodayDom.Save(strStatisFileName);
                return;
            }

            m_nUnsavedCount++;
        }

        // 在指定的<category>元素下写入<item>元素
        double WriteItem(XmlNode nodeCategory,
            string strName,
            double fValue)
        {
            XmlNode nodeItem = nodeCategory.SelectSingleNode("item[@name='" + strName + "']");
            if (nodeItem == null)
            {
                nodeItem = this.TodayDom.CreateElement("item");
                nodeCategory.AppendChild(nodeItem);
                DomUtil.SetAttr(nodeItem, "name", strName);
            }

            string strOldValue = DomUtil.GetAttr(nodeItem, "value");
            double fOldValue = 0;

            if (string.IsNullOrEmpty(strOldValue) == false)
            {
                try
                {
                    fOldValue = Convert.ToDouble(strOldValue);
                }
                catch
                {
                }
            }

            double fNewValue = fOldValue + fValue;

            DomUtil.SetAttr(nodeItem, "value", fNewValue.ToString());

            return fNewValue;
        }

        // 在统计文件中写入一个值
        // 在根下写入<category>，然后在<library>下写入<category>
        // parameters:
        //      strLibraryCode  如果为空，则只在根下写入<category>；如果为非空，则还要在<library>元素下写入<category>
        // return:
        //      如果strLibraryCode为空，则返回唯一累加值；如果strLibraryCode为非空，则返回<library>元素下的<category>中的累加值
        public double IncreaseEntryValue(
            string strLibraryCode,
            string strCategory,
            string strName,
            double fValue)
        {
            if (this.TodayDom.DocumentElement == null)
            {
                throw new Exception("Statis dom not initialized");
            }

            if (this.CurrentDate == "")
            {
                throw new Exception("Statis CurrentDate not initialized");
            }

            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {
                if (GetCurrentDate() != this.CurrentDate)
                {
                    SaveDom(true);
                    this.CurrentDate = GetCurrentDate();
                    this.TodayDom.LoadXml("<root />");  // 清除原来的内容
                    // 设置起始时间
                    DomUtil.SetElementText(this.TodayDom.DocumentElement,
                        "startTime", DateTime.Now.ToString());
                    // 设置结束时间
                    DomUtil.SetElementText(this.TodayDom.DocumentElement,
                        "endTime", DateTime.Now.ToString());
                }

                if (String.IsNullOrEmpty(strCategory) == true)
                    strCategory = "default";

                XmlNode nodeCategory = this.TodayDom.DocumentElement.SelectSingleNode("category[@name='" + strCategory + "']");
                if (nodeCategory == null)
                {
                    nodeCategory = this.TodayDom.CreateElement("category");
                    this.TodayDom.DocumentElement.AppendChild(nodeCategory);
                    DomUtil.SetAttr(nodeCategory, "name", strCategory);
                }

                // 在指定的<category>元素下写入<item>元素
                double fNewValue = WriteItem(nodeCategory,
                    strName,
                    fValue);
#if NO
                XmlNode nodeItem = nodeCategory.SelectSingleNode("item[@name='" + strName + "']");
                if (nodeItem == null)
                {
                    nodeItem = this.TodayDom.CreateElement("item");
                    nodeCategory.AppendChild(nodeItem);
                    DomUtil.SetAttr(nodeItem, "name", strName);
                }

                string strOldValue = DomUtil.GetAttr(nodeItem, "value");
                double fOldValue = 0;

                if (string.IsNullOrEmpty(strOldValue) == false)
                {
                    try
                    {
                        fOldValue = Convert.ToDouble(strOldValue);
                    }
                    catch
                    {
                    }
                }

                double fNewValue = fOldValue + fValue;

                DomUtil.SetAttr(nodeItem, "value", fNewValue.ToString());

#endif
                if (string.IsNullOrEmpty(strLibraryCode) == false)
                {
                    XmlNode nodeLibrary = this.TodayDom.DocumentElement.SelectSingleNode("library[@code='" + strLibraryCode + "']");
                    if (nodeLibrary == null)
                    {
                        nodeLibrary = this.TodayDom.CreateElement("library");
                        this.TodayDom.DocumentElement.AppendChild(nodeLibrary);
                        DomUtil.SetAttr(nodeLibrary, "code", strLibraryCode);

                        nodeCategory = null;
                    }
                    else
                    {
                        nodeCategory = nodeLibrary.SelectSingleNode("category[@name='" + strCategory + "']");
                    }

                    if (nodeCategory == null)
                    {
                        nodeCategory = this.TodayDom.CreateElement("category");
                        nodeLibrary.AppendChild(nodeCategory);
                        DomUtil.SetAttr(nodeCategory, "name", strCategory);
                    }

                    // 在指定的<category>元素下写入<item>元素
                    fNewValue = WriteItem(nodeCategory,
                        strName,
                        fValue);
                }

                // 设置结束时间
                DomUtil.SetElementText(this.TodayDom.DocumentElement,
                    "endTime", DateTime.Now.ToString());

                SaveDom(false);

                return fNewValue;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        public int IncreaseEntryValue(
            string strLibraryCode,
            string strCategory,
            string strName,
            int nValue)
        {
            double fNewValue = IncreaseEntryValue(
                strLibraryCode,
                strCategory,
                strName,
                (double)nValue);

            return (int)fNewValue;
        }

        // 异常：可能会抛出异常
        public void Flush()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);

            try
            {
                if (this.TodayDom != null
                    && this.CurrentDate != null)
                {
                    SaveDom(true);
                }
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

        }

    }

    /*
     * 目前用到的统计指标名称。不包括动态的名称。
     * 
出纳
	借书遇册条码号重复次数
	读者数
	借册
	还书遇册条码号重复次数
	还书遇册条码号重复并无读者证条码号辅助判断次数
	借书遇册条码号重复并读者证条码号也无法去重次数
	借书遇册条码号重复但根据读者证条码号成功去重次数
	读者数
	还册
	声明丢失
	还超期册
	预约到书册
	以停代金事项启动
	以停代金事项到期
	当日内立即还册
	预约次
	预约到书册
违约金
	取消次
	取消元
	修改次
	给付次
	给付元
违约金之注释
	修改次
修复借阅信息
	读者侧次数
	实体侧次数
入馆人次
	所有门之总量
押金
	创建交费请求次
租金
	创建交费请求次
修改读者信息
	创建新记录数
	修改记录数
	删除记录数
修改读者信息之状态
	
修改读者信息之押金
	次数
消息监控
	删除过期消息条数
超期通知
	dpmail超期通知人数
	email超期通知人数
跟踪DTLP
	初始化数据库次数
	覆盖记录条数
	删除记录条数
     * * */

}
