using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// OperLogStatisForm (日志统计窗) 统计方案的宿主类
    /// </summary>
    public class OperLogStatis : StatisHostBase
    {
        // 基本统计表格(全范围一个表)

        /// <summary>
        /// 本对象所关联的 OperLogStatisForm (日志统计窗)
        /// </summary>
        public OperLogStatisForm OperLogStatisForm = null;	// 引用

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime StartDate = new DateTime(0);

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime EndDate = new DateTime(0);

        /// <summary>
        /// 获得表示日期范围的字符串
        /// </summary>
        /// <returns>日期范围字符串</returns>
        public string GetTimeRangeString()
        {
            string strStart = StartDate.ToLongDateString();
            string strEnd = EndDate.ToLongDateString();

            if (strStart == strEnd)
                return strStart;

            return strStart + "-" + strEnd;
        }

        // 当前日期，在运行中不断变动
        /// <summary>
        /// 当前正在处理的日期
        /// </summary>
        public DateTime CurrentDate = new DateTime(0);

#if NO
        private bool disposed = false;
        public WebBrowser Console = null;
        public string ProjectDir = "";  // 方案源文件所在目录
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件

        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件

        int m_nFileNameSeed = 1;
#endif

        /// <summary>
        /// 当前正在处理的日志文件名。纯文件名
        /// </summary>
        public string CurrentLogFileName = "";    // 当前日志文件名(纯文件名)

        /// <summary>
        /// 当前日志记录在文件中的下标 (从 0 开始计数)
        /// </summary>
        public long CurrentRecordIndex = -1; // 当前日志记录在文件中的偏移量

        string m_strXml = "";    // 日志记录体
        /// <summary>
        /// 当前正在处理的日志记录 XML 记录，字符串类型
        /// </summary>
        public string Xml
        {
            get
            {
                return this.m_strXml;
            }
            set
            {
                this.m_strXml = value;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public OperLogStatis()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.OperLogStatisForm.MainForm.DataDir, "~operlog_statis");
        }

#if NO
        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~OperLogStatis()      
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }


        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // 删除所有输出文件
                if (this.OutputFileNames != null)
                {
                    Global.DeleteFiles(this.OutputFileNames);
                    this.OutputFileNames = null;
                }

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;
                 * */
                try // 2009/10/21
                {
                    this.FreeResources();
                }
                catch
                {
                }
            }
            disposed = true;
        }

        // 2009/10/21
        public virtual void FreeResources()
        {
        }

        // 初始化
        public virtual void OnInitial(object sender, StatisEventArgs e)
        {

        }

        // 开始
        public virtual void OnBegin(object sender, StatisEventArgs e)
        {

        }

        // 每一记录处理
        public virtual void OnRecord(object sender, StatisEventArgs e)
        {

        }

        // 结束
        public virtual void OnEnd(object sender, StatisEventArgs e)
        {

        }

        // 打印输出
        public virtual void OnPrint(object sender, StatisEventArgs e)
        {

        }


        public void ClearConsoleForPureTextOutputing()
        {
            Global.ClearForPureTextOutputing(this.Console);
        }

        public void WriteToConsole(string strText)
        {
            Global.WriteHtml(this.Console, strText);
        }

        public void WriteTextToConsole(string strText)
        {
            Global.WriteHtml(this.Console, HttpUtility.HtmlEncode(strText));
        }

        // 获得一个新的输出文件名
        public string NewOutputFileName()
        {
            string strFileNamePrefix = this.OperLogStatisForm.MainForm.DataDir + "\\~statis";

            string strFileName = strFileNamePrefix + "_" + this.m_nFileNameSeed.ToString() + ".html";

            this.m_nFileNameSeed++;

            this.OutputFileNames.Add(strFileName);

            return strFileName;
        }

        // 将字符串内容写入文本文件
        public void WriteToOutputFile(string strFileName,
            string strText,
            Encoding encoding)
        {
            StreamWriter sw = new StreamWriter(strFileName,
                false,	// append
                encoding);
            sw.Write(strText);
            sw.Close();
        }

        // 删除一个输出文件
        public void DeleteOutputFile(string strFileName)
        {
            int nIndex = this.OutputFileNames.IndexOf(strFileName);
            if (nIndex != -1)
                this.OutputFileNames.RemoveAt(nIndex);

            try
            {
                File.Delete(strFileName);
            }
            catch
            {
            }
        }

#endif
        // 新版本
        // 获得一个modifyprice或者amerce动作型的日志记录中的修改违约金的细节
        // return:
        //      -1  error
        //      0   成功
        /// <summary>
        /// 获得一个modifyprice或者amerce动作型的日志记录中的修改违约金的细节
        /// </summary>
        /// <param name="domOperLog">日志记录 XmlDocument 对象</param>
        /// <param name="prices">金额对的集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 成功</returns>
        public static int ComputeAmerceModifiedPrice(XmlDocument domOperLog,
            out List<PricePair> prices,
            out string strError)
        {
            strError = "";
            prices = new List<PricePair>();

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            if (strAction != "modifyprice"
                && strAction != "amerce")
            {
                strError = "动作类型是 '" + strAction + "', 不能用于调用ComputeAmerceModifiedPrice()函数。应当是modifyprice/amerce类型";
                return -1;
            }

            List<IdPrice> list = null;
            int nRet = 0;

#if NO
            // 根据日志记录中的<oldReaderRecord>元素内嵌文本(当作一个XML记录)中的<overdue>元素创建ID-价格列表
            nRet = BuildIdPriceListFromOverdueTag(
                domOperLog,
                "oldReaderRecord",
                out list,
                out strError);
                        if (nRet == 0)
                return 0;
#endif
            nRet = BuildIdPriceListFromAmerceRecordTag(
    domOperLog,
    true,
    out list,
    out strError);
            if (nRet == -1)
                return -1;
            if (list.Count == 0)
                return 0;


            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");

                // 2012/3/23
                if (string.IsNullOrEmpty(strNewPrice) == true)
                    continue;

                // 
                string strPureNewPrice = PriceUtil.GetPurePrice(strNewPrice);

                // 2012/3/23
                if (string.IsNullOrEmpty(strPureNewPrice) == true)
                    continue;


                // 
                string strDebug = "";
                string strOldPrice = GetPriceByID(list, strID, out strDebug);
                if (strOldPrice == null)
                {
                    strError = "日志文件格式错误: 根据id '" + strID + "' 在日志记录<oldReaderRecord>元素文本内<overdue>元素中没有找到对应的事项。debug: " + strDebug;
                    return -1;
                }

                PricePair pair = new PricePair();
                pair.OldPrice = strOldPrice;
                pair.NewPrice = strNewPrice;

                prices.Add(pair);
            }

            return 0;
        }

        // 包装后的版本，为了兼容以前的版本
        /// <summary>
        /// 计算一个amerce或undo动作类型日志记录中的相关违约金总额。即将废止的版本
        /// </summary>
        /// <param name="domOperLog">日志记录 XmlDocument 对象</param>
        /// <param name="prices">金额字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 成功</returns>
        public static int ComputeAmerceOrUndoPrice(XmlDocument domOperLog,
    out List<string> prices,
    out string strError)
        {
            return ComputeAmerceOrUndoPrice(domOperLog,
                null,
                out prices,
                out strError);
        }

        /*
amerce 交费

<root>
<operation>amerce</operation> 操作类型
<action>amerce</action> 具体动作。有amerce undo modifyprice
<readerBarcode>...</readerBarcode> 读者证条码
<!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
<amerceItems>
<amerceItem id="..." newPrice="..." comment="..." />
...
</amerceItems>
<amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> 在罚款库中创建的新记录。注意<amerceRecord>元素可以重复。<amerceRecord>元素内容里面的<itemBarcode><readerBarcode><id>等具备了足够的信息。
<operator>test</operator> 操作者
<operTime>Fri, 08 Dec 2006 10:09:36 GMT</operTime> 操作时间
  
<readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>

<root>
<operation>amerce</operation> 
<action>undo</action> 
<readerBarcode>...</readerBarcode> 读者证条码
<!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
<amerceItems>
<amerceItem id="..." newPrice="..."/>
...
</amerceItems>
<amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> Undo所去掉的罚款库记录
<operator>test</operator> 
<operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
  
<readerRecord recPath='...'>...</readerRecord>	最新读者记录

</root>

 * */
        // 新版本，返回字符串数组
        // 计算一个amerce或undo动作类型日志记录中的相关违约金总额
        /// <summary>
        /// 计算一个amerce或undo动作类型日志记录中的相关违约金总额
        /// </summary>
        /// <param name="domOperLog">日志记录 XmlDocument 对象</param>
        /// <param name="strReasonHead">表示费用事由的引导字符串</param>
        /// <param name="prices">金额字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 成功</returns>
        public static int ComputeAmerceOrUndoPrice(XmlDocument domOperLog,
            string strReasonHead,
            out List<string> prices,
            out string strError)
        {
            strError = "";
            prices = new List<string>();

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            List<IdPrice> list = null;
            int nRet = 0;

            if (strAction == "modifyprice")
            {
                strError = "动作类型是modifyprice，不能由本函数处理，应当由 ComputeAmerceModifiedPrice() 函数处理。";
                return -1;
            }

            // 根据日志记录中的 <amerceRecord> 元素创建ID-价格列表
            nRet = BuildIdPriceListFromAmerceRecordTag(domOperLog,
                false,
                out list,
                out strError);
            if (nRet == -1)
                return -1;

            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");

                string strDebug = "";

                if (string.IsNullOrEmpty(strReasonHead) == false)
                {
                    string strReason = GetReasonByID(list, strID, out strDebug);
                    if (StringUtil.HasHead(strReason, strReasonHead) == false)
                        continue;
                }

                if (strAction == "amerce")
                {
                    // 如果没有新价格，就找到旧价格
                    if (String.IsNullOrEmpty(strNewPrice) == true)
                    {
                        strNewPrice = GetPriceByID(list, strID, out strDebug);
                        if (strNewPrice == null)
                        {
                            strError = "日志文件格式错误: 根据id '" + strID + "' 在<amerceRecord>元素中没有找到对应的事项。";
                            return -1;
                        }
                    }
                }
                if (strAction == "undo")
                {
                    strNewPrice = GetPriceByID(list, strID, out strDebug);
                    if (strNewPrice == null)
                    {
                        strError = "日志文件格式错误: 根据id '" + strID + "' 在<amerceRecord>元素中没有找到对应的事项。";
                        return -1;
                    }

                    // 找到ID列表

                    // 在读者记录中的<overdues>下找到对应的<overdue>元素，然后在price元素中得到价格

                    // 也可以遍历日志记录中<amerceRecord>元素，找到id匹配的记录。
                }

                prices.Add(strNewPrice);
            }

            return 0;
        }

        /*
amerce 交费

<root>
  <operation>amerce</operation> 操作类型
  <action>amerce</action> 具体动作。有amerce undo modifyprice
  <readerBarcode>...</readerBarcode> 读者证条码
  <!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
  <amerceItems>
	<amerceItem id="..." newPrice="..." comment="..." />
	...
  </amerceItems>
  <amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> 在罚款库中创建的新记录。注意<amerceRecord>元素可以重复。<amerceRecord>元素内容里面的<itemBarcode><readerBarcode><id>等具备了足够的信息。
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 10:09:36 GMT</operTime> 操作时间
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
</root>

<root>
  <operation>amerce</operation> 
  <action>undo</action> 
  <readerBarcode>...</readerBarcode> 读者证条码
  <!-- <idList>...<idList> ID列表，逗号间隔 已废止 -->
  <amerceItems>
	<amerceItem id="..." newPrice="..."/>
	...
  </amerceItems>
  <amerceRecord recPath='...'><root><itemBarcode>0000001</itemBarcode><readerBarcode>R0000002</readerBarcode><state>amerced</state><id>632958375041543888-1</id><over>31day</over><borrowDate>Sat, 07 Oct 2006 09:04:28 GMT</borrowDate><borrowPeriod>30day</borrowPeriod><returnDate>Thu, 07 Dec 2006 09:04:27 GMT</returnDate><returnOperator>test</returnOperator></root></amerceRecord> Undo所去掉的罚款库记录
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录

</root>

         * */
        // 计算一个amerce或undo动作类型日志记录中的相关违约金总额
        /// <summary>
        /// 计算一个amerce或undo动作类型日志记录中的相关违约金总额。即将废止的版本
        /// </summary>
        /// <param name="domOperLog">日志记录 XmlDocument 对象</param>
        /// <param name="nCount">金额个数</param>
        /// <param name="total_price">总金额</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 成功</returns>
        public static int ComputeAmerceOrUndoPrice(XmlDocument domOperLog,
            out int nCount,
            out decimal total_price,
            out string strError)
        {
            nCount = 0;
            total_price = 0;
            strError = "";

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            List<IdPrice> list = null;
            int nRet = 0;

            if (strAction == "modifyprice")
            {
                strError = "动作类型是modifyprice，不能由本函数处理，应当由ComputeAmerceModifiedPrice()函数处理。";
                return -1;
            }



            // 根据日志记录中的<amerceRecord>元素创建ID-价格列表
            nRet = BuildIdPriceListFromAmerceRecordTag(domOperLog,
                false,
                out list,
                out strError);
            if (nRet == -1)
                return -1;

            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");

                string strDebug = "";

                if (strAction == "amerce")
                {
                    // 如果没有新价格，就找到旧价格
                    if (String.IsNullOrEmpty(strNewPrice) == true)
                    {
                        strNewPrice = GetPriceByID(list, strID, out strDebug);
                        if (strNewPrice == null)
                        {
                            strError = "日志文件格式错误: 根据id '" + strID + "' 在<amerceRecord>元素中没有找到对应的事项。";
                            return -1;
                        }
                    }
                }
                if (strAction == "undo")
                {
                    strNewPrice = GetPriceByID(list, strID, out strDebug);
                    if (strNewPrice == null)
                    {
                        strError = "日志文件格式错误: 根据id '" + strID + "' 在<amerceRecord>元素中没有找到对应的事项。";
                        return -1;
                    }

                    // 找到ID列表

                    // 在读者记录中的<overdues>下找到对应的<overdue>元素，然后在price元素中得到价格

                    // 也可以遍历日志记录中<amerceRecord>元素，找到id匹配的记录。
                }

                // 累加strNewPrice
                string strPurePrice = PriceUtil.GetPurePrice(strNewPrice);
                decimal price = 0;
                try
                {
                    price = Convert.ToDecimal(strPurePrice);
                }
                catch
                {
                    strError = "价格字符串 '" + strPurePrice + "' 格式错误1。";
                    return -1;
                }
                total_price += price;
            }

            nCount = nodes.Count;

            return 0;
        }

        // 旧版本
        // 计算一个modifyprice或者amerce动作型的日志记录中的修改违约金总额
        // return:
        //      -1  error
        //      9   没有找到相关元素
        //      1   成功
        /// <summary>
        /// 计算一个modifyprice或者amerce动作型的日志记录中的修改违约金总额。
        /// 这是即将废止的版本
        /// </summary>
        /// <param name="domOperLog">日止记录 XmlDocument 对象</param>
        /// <param name="nCount">返回事项个书</param>
        /// <param name="inc_price">增加的金额</param>
        /// <param name="dec_price">减少的金额</param>
        /// <param name="total_delta_price">总体的变动金额</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到； 1: 找到</returns>
        public static int ComputeAmerceModifiedPrice(XmlDocument domOperLog,
            out int nCount,
            out decimal inc_price,
            out decimal dec_price,
            out decimal total_delta_price,
            out string strError)
        {
            nCount = 0;
            inc_price = 0;
            dec_price = 0;
            total_delta_price = 0;
            strError = "";

            string strAction = DomUtil.GetElementText(domOperLog.DocumentElement,
                "action");

            if (strAction != "modifyprice"
                && strAction != "amerce")
            {
                strError = "动作类型是 '" + strAction + "', 不能用于调用ComputeAmerceModifiedPrice()函数。应当是modifyprice/amerce类型";
                return -1;
            }

            List<IdPrice> list = null;
            int nRet = 0;

            /*
            // 根据日志记录中的<oldReaderRecord>元素内嵌文本(当作一个XML记录)中的<overdue>元素创建ID-价格列表
            nRet = BuildIdPriceListFromOverdueTag(
                domOperLog,
                "oldReaderRecord",
                out list,
                out strError);
            if (nRet == 0)
                return 0;
             * */
            nRet = BuildIdPriceListFromAmerceRecordTag(
domOperLog,
true,
out list,
out strError);
            if (nRet == -1)
                return -1;
            if (list.Count == 0)
                return 0;


            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceItems/amerceItem");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");
                string strNewPrice = DomUtil.GetAttr(node, "newPrice");

                // 2012/3/23
                if (string.IsNullOrEmpty(strNewPrice) == true)
                    continue;

                // 
                string strPureNewPrice = PriceUtil.GetPurePrice(strNewPrice);

                // 2012/3/23
                if (string.IsNullOrEmpty(strPureNewPrice) == true)
                    continue;


                decimal new_price = 0;
                try
                {
                    new_price = Convert.ToDecimal(strPureNewPrice);
                }
                catch
                {
                    strError = "价格字符串 '" + strPureNewPrice + "' 格式错误2。";
                    return -1;
                }

                // 
                string strDebug = "";
                string strOldPrice = GetPriceByID(list, strID, out strDebug);
                if (strOldPrice == null)
                {
                    strError = "日志文件格式错误: 根据id '" + strID + "' 在日志记录<oldReaderRecord>元素文本内<overdue>元素中没有找到对应的事项。debug: " + strDebug;
                    return -1;
                }

                string strOldPurePrice = PriceUtil.GetPurePrice(strOldPrice);

                decimal old_price = 0;
                try
                {
                    old_price = Convert.ToDecimal(strOldPurePrice);
                }
                catch
                {
                    strError = "价格字符串 '" + strOldPurePrice + "' 格式错误3。";
                    return -1;
                }

                decimal delta = new_price - old_price;

                if (delta > 0)
                {
                    inc_price += delta;
                }

                if (delta < 0)
                {
                    dec_price += delta;
                }


                total_delta_price += delta;
            }

            nCount = nodes.Count;

            return 1;
        }

        // 根据日志记录中的<amerceRecord>元素创建ID-价格列表
        // parameters:
        //      bGetOriginPrice 是否要尽量获得 originPrice 元素内容
        static int BuildIdPriceListFromAmerceRecordTag(
            XmlDocument domOperLog,
            bool bGetOriginPrice,
            out List<IdPrice> list,
            out string strError)
        {
            strError = "";
            list = new List<IdPrice>();

            XmlNodeList nodes = domOperLog.DocumentElement.SelectNodes("amerceRecord");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strXml = nodes[i].InnerText;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "日志记录中<amerceRecord>文本装入DOM失败: " + ex.Message;
                    return -1;
                }

                string strOriginPrice = DomUtil.GetElementText(dom.DocumentElement,
                    "originPrice").Trim();


                string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                    "price").Trim();
                string strID = DomUtil.GetElementText(dom.DocumentElement,
                    "id").Trim();
                string strReason = DomUtil.GetElementText(dom.DocumentElement,
                    "reason").Trim();

                IdPrice item = new IdPrice();
                item.ID = strID;
                // 如果有原始金额，尽量使用原始金额
                if (bGetOriginPrice == true && string.IsNullOrEmpty(strOriginPrice) == false)
                    item.Price = strOriginPrice;
                else
                    item.Price = strPrice;
                item.Reason = strReason;

                list.Add(item);
            }

            return 0;
        }

#if NO
        // 根据日志记录中的某元素的内嵌文本的<overdue>元素创建ID-价格列表
        // oldReaderRecord 元素 或者 readerRecord 元素
        // parameters:
        // return:
        //      -1  error
        //      0   要找的XML元素不存在
        //      1   成功
        static int BuildIdPriceListFromOverdueTag(
            XmlDocument domOperLog,
            string strElementTag,
            out List<IdPrice> list,
            out string strError)
        {
            strError = "";
            list = new List<IdPrice>();

            XmlNode nodeRecord = domOperLog.DocumentElement.SelectSingleNode(strElementTag);
            if (nodeRecord == null)
            {
                strError = "元素 <" +strElementTag+ "> 在日志记录中不存在";
                return 0;
            }

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(nodeRecord.InnerText);
            }
            catch (Exception ex)
            {
                strError = "元素 <" + strElementTag + "> 内文本装入XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strPrice = DomUtil.GetAttr(node,
                    "price").Trim();
                string strID = DomUtil.GetAttr(node,
                    "id").Trim();
                string strReason = DomUtil.GetAttr(node,
                    "reason").Trim();

                IdPrice item = new IdPrice();
                item.ID = strID;
                item.Price = strPrice;
                item.Reason = strReason;

                list.Add(item);
            }

            return 1;
        }
#endif

        // 在list中根据id找到对应的价格字符串
        // return:
        //      null    没有找到
        //      其他    找到的价格字符串
        static string GetPriceByID(List<IdPrice> list,
            string strID,
            out string strDebug)
        {
            strDebug = "";

            strID = strID.Trim();

            for (int i = 0; i < list.Count; i++)
            {
                IdPrice item = list[i];

                strDebug += "item[" + i.ToString() + "] id=" + item.ID + ", price=" + item.Price + ", reason="+item.Reason+";\r\n";

                if (strID == item.ID)
                    return item.Price;
            }

            return null;    // not found
        }

        // 在list中根据id找到对应的 Reason 字符串
        // return:
        //      null    没有找到
        //      其他    找到的价格字符串
        static string GetReasonByID(List<IdPrice> list,
            string strID,
            out string strDebug)
        {
            strDebug = "";

            strID = strID.Trim();

            for (int i = 0; i < list.Count; i++)
            {
                IdPrice item = list[i];

                strDebug += "item[" + i.ToString() + "] id=" + item.ID + ", price=" + item.Price + ", reason=" + item.Reason + ";\r\n";

                if (strID == item.ID)
                    return item.Reason;
            }

            return null;    // not found
        }

        // 汇总价格对
        // NewPrice 减去 OldPrice
        /// <summary>
        /// 汇总金额对。算法是 NewPrice 减去 OldPrice
        /// </summary>
        /// <param name="pairs">要汇总的金额对集合</param>
        /// <param name="strResult">结果字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 成功</returns>
        public static int TotalPricePair(
            List<PricePair> pairs,
            out string strResult,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            strResult = "";

            List<string> prices = new List<string>();

            foreach (PricePair pair in pairs)
            {
                if (string.IsNullOrEmpty(pair.NewPrice) == false)
                    prices.Add(pair.NewPrice);

                if (string.IsNullOrEmpty(pair.OldPrice) == false)
                {
                    string strResultPrice = "";
                    // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
                    // parameters:
                    //      bSum    是否要顺便汇总? true表示要汇总
                    nRet = PriceUtil.NegativePrices(pair.OldPrice,
                        true,
                        out strResultPrice,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    prices.Add(strResultPrice);
                }

            }

            nRet = PriceUtil.TotalPrice(prices,
                out strResult,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }
    }

    // ID-价格二元组
    internal class IdPrice
    {
        public string ID = "";
        public string Price = "";
        public string Reason = "";  // 2013/6/14
    }

    /// <summary>
    /// 具有时间范围特性的表格的集合
    /// </summary>
    public class TimeRangedStatisTableCollection : List<TimeRangedStatisTable>
    {
        bool m_bAllInOne = false;
        bool m_bYear = false;
        bool m_bMonth = false;
        bool m_bDay = false;
        int m_nColumnsHint = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nColumnHint">栏目数暗示</param>
        /// <param name="bAllInOne">按 起止范围 </param>
        /// <param name="bYear">按 每一年</param>
        /// <param name="bMonth">按 每一月</param>
        /// <param name="bDay">按 每一日</param>
        public TimeRangedStatisTableCollection(
            int nColumnHint,
            bool bAllInOne,
            bool bYear,
            bool bMonth,
            bool bDay)
        {
            this.m_nColumnsHint = nColumnHint;
            this.m_bAllInOne = bAllInOne;
            this.m_bYear = bYear;
            this.m_bMonth = bMonth;
            this.m_bDay = bDay;
        }

        /// <summary>
        /// 写入一个单元的值
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="value">值</param>
        public void SetValue(
            DateTime currentTime,
            string strEntry,
            int nColumn,
            object value)
        {
            if (this.m_bAllInOne == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "", this.m_nColumnsHint);
                table.Table.SetValue(strEntry,
                    nColumn, value);
            }
            if (this.m_bYear == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "year", this.m_nColumnsHint);
                table.Table.SetValue(strEntry,
                    nColumn, value);
            }
            if (this.m_bMonth == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "month", this.m_nColumnsHint);
                table.Table.SetValue(strEntry,
                    nColumn, value);
            }
            if (this.m_bDay == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "day", this.m_nColumnsHint);
                table.Table.SetValue(strEntry,
                    nColumn, value);
            }
        }

        static void Inc(TimeRangedStatisTable table, 
            string strEntry,
            int nColumn,
            string strPrice)
        {
            Line line = table.Table.EnsureLine(strEntry);
            string strOldValue = (string)line[nColumn];
            if (string.IsNullOrEmpty(strOldValue) == true)
            {
                line.SetValue(nColumn, strPrice);
                return;
            }

            // 连接两个价格字符串
            string strPrices = PriceUtil.JoinPriceString(strOldValue,
                    strPrice);

            string strError = "";
            List<string> prices = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
            // return:
            //      -1  error
            //      0   succeed
            int nRet = PriceUtil.SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            string strResult = "";
            nRet = PriceUtil.TotalPrice(prices,
out strResult,
out strError);
            if (nRet == -1)
                throw new Exception(strError);

            line.SetValue(nColumn, strResult);
        }

        /// <summary>
        /// 增量一个单元的金额
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="strPrice">金额字符串</param>
        public void IncPrice(
            DateTime currentTime,
            string strEntry,
            int nColumn,
            string strPrice)
        {
            // 2013/6/14
            if (string.IsNullOrEmpty(strPrice) == true)
                return;

            TimeRangedStatisTable table = null;
            if (this.m_bAllInOne == true)
            {
                table = GetTable(currentTime, "", this.m_nColumnsHint);
                Inc(table, strEntry, nColumn, strPrice);
            }
            if (this.m_bYear == true)
            {
                table = GetTable(currentTime, "year", this.m_nColumnsHint);
                Inc(table, strEntry, nColumn, strPrice);
            }
            if (this.m_bMonth == true)
            {
                table = GetTable(currentTime, "month", this.m_nColumnsHint);
                Inc(table, strEntry, nColumn, strPrice);
            }
            if (this.m_bDay == true)
            {
                table = GetTable(currentTime, "day", this.m_nColumnsHint);
                Inc(table, strEntry, nColumn, strPrice);
            }
        }

        /// <summary>
        /// 增量一个单元的整数值
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="createValue">创建值</param>
        /// <param name="incValue">增量值</param>
        public void IncValue(
            DateTime currentTime,
            string strEntry,
            int nColumn,
            Int64 createValue,
            Int64 incValue)
        {
            if (this.m_bAllInOne == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "", this.m_nColumnsHint);
                table.Table.IncValue(strEntry,
                    nColumn, createValue, incValue);
            }
            if (this.m_bYear == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "year", this.m_nColumnsHint);
                table.Table.IncValue(strEntry,
                    nColumn, createValue, incValue);
            }
            if (this.m_bMonth == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "month", this.m_nColumnsHint);
                table.Table.IncValue(strEntry,
                    nColumn, createValue, incValue);
            }
            if (this.m_bDay == true)
            {
                TimeRangedStatisTable table = GetTable(currentTime, "day", this.m_nColumnsHint);
                table.Table.IncValue(strEntry,
                    nColumn, createValue, incValue);
            }

        }

        // 
        /// <summary>
        /// 获得一个适当的表格。如果当前不存在，会自动创建
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="strStyle">风格。内容为空字符串或者 year/month/day</param>
        /// <param name="nColumnsHint">栏目数暗示</param>
        /// <returns>TimeRangedStatisTable 类型的表格对象</returns>
        public TimeRangedStatisTable GetTable(DateTime currentTime,
            string strStyle,
            int nColumnsHint)
        {
            int nCurYear = currentTime.Year;

            if (strStyle != ""
                && strStyle != "year"
                && strStyle != "month"
                && strStyle != "day")
            {
                throw new Exception("无法识别的strStyle参数值'" + strStyle + "'");
            }


            for (int i = 0; i < this.Count; i++)
            {
                TimeRangedStatisTable table = this[i];

                if (strStyle == "year")
                {
                    if (table.Style == "year"
                        && table.StartTime.Year == currentTime.Year)
                        return table;
                }
                else if (strStyle == "month")
                {
                    if (table.Style == "month"
                        && table.StartTime.Year == currentTime.Year
                        && table.StartTime.Month == currentTime.Month)
                        return table;
                }
                else if (strStyle == "day")
                {
                    if (table.Style == "day"
                        && table.StartTime.Year == currentTime.Year
                        && table.StartTime.Month == currentTime.Month
                        && table.StartTime.Day == currentTime.Day)
                        return table;
                }
                else if (strStyle == "")
                {
                    if (table.Style == "")
                    {
                        if (currentTime > table.EndTime)
                            table.EndTime = currentTime;
                        return table;
                        // 注：如果只经历一天的统计，则EndTime就会为空值
                    }
                }

            }

            // 没有找到。创建一个新的表
            TimeRangedStatisTable newTable = new TimeRangedStatisTable();
            newTable.StartTime = currentTime;
            newTable.Table = new Table(nColumnsHint);
            newTable.Style = strStyle;

            this.Add(newTable);
            return newTable;
        }

    }

    // 
    /// <summary>
    /// 具有时间范围特性的表格
    /// </summary>
    public class TimeRangedStatisTable
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime = new DateTime(0);

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime = new DateTime(0);

        /// <summary>
        /// 时间切割风格。有"year" "month" "day" "" 4种
        /// </summary>
        public string Style = "";   // 时间切割风格。有"year" "month" "day" "" 4种

        // 
        /// <summary>
        /// 获得时间范围名
        /// </summary>
        public string TimeRangeName
        {
            get
            {
                if (this.Style == "year")
                    return StartTime.Year.ToString().PadLeft(4, '0') + "年";

                if (this.Style == "month")
                    return StartTime.Year.ToString().PadLeft(4, '0') + "年"
                        + StartTime.Month.ToString() + "月";

                if (this.Style == "day")
                    return StartTime.Year.ToString().PadLeft(4, '0') + "年"
                        + StartTime.Month.ToString() + "月"
                        + StartTime.Day.ToString() + "日";

                if (this.EndTime == new DateTime(0))
                {
                    return StartTime.Year.ToString().PadLeft(4, '0') + "年"
        + StartTime.Month.ToString() + "月"
        + StartTime.Day.ToString() + "日";
                }

                return StartTime.Year.ToString().PadLeft(4, '0') + "年"
                        + StartTime.Month.ToString() + "月"
                        + StartTime.Day.ToString() + "日 - "
                        + EndTime.Year.ToString().PadLeft(4, '0') + "年"
                        + EndTime.Month.ToString() + "月"
                        + EndTime.Day.ToString() + "日";
            }
        }

        /// <summary>
        /// 内含的 Table 类型对象
        /// </summary>
        public Table Table = null;

        // 
        /// <summary>
        /// 是否为空的时间范围?
        /// </summary>
        public bool IsNullTimeRange
        {
            get
            {
                if (this.StartTime == new DateTime(0)
                    && this.EndTime == new DateTime(0))
                    return true;
                return false;
            }
        }

        // 
        /// <summary>
        /// 是否仅仅包含一天的时间范围?
        /// </summary>
        public bool IsOneDayRange
        {
            get
            {
                if (this.StartTime == new DateTime(0)
                    && this.EndTime == new DateTime(0))
                    return false;

                if (this.EndTime == new DateTime(0))
                    return true;
                return false;
            }
        }
    }

    /// <summary>
    /// 一对金额
    /// </summary>
    public class PricePair
    {
        /// <summary>
        /// 新金额
        /// </summary>
        public string NewPrice = "";
        /// <summary>
        /// 旧金额
        /// </summary>
        public string OldPrice = "";
    }
}
