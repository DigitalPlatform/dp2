using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using Microsoft.Win32.SafeHandles;

using DigitalPlatform.Xml;
using System.Web;


namespace dp2Circulation
{
    /// <summary>
    /// 凭条打印宿主类
    /// </summary>
    public class PrintHost
    {
        /// <summary>
        /// 统计方案存储目录
        /// </summary>
        public string ProjectDir = "";  // 方案源文件所在目录

        /// <summary>
        /// 当前正在运行的统计方案实例的独占目录。一般用于存储统计过程中的临时文件
        /// </summary>
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 脚本编译后的 Assembly
        /// </summary>
        public Assembly Assembly = null;

        /*
        // 基类主动记忆的借还操作有关信息，便于OnPrint()时使用
        public string CurrentReaderBarcode = "";
        public string CurrentReaderSummary = "";
        public List<BorrowItemInfo> BorrowItems = new List<BorrowItemInfo>();
        public List<ReturnItemInfo> ReturnItems = new List<ReturnItemInfo>();
         * */
        /// <summary>
        /// 打印信息
        /// </summary>
        public PrintInfo PrintInfo = new PrintInfo();

        /// <summary>
        /// 已经打印的信息集合
        /// </summary>
        public List<PrintInfo> PrintedInfos = new List<PrintInfo>();

        /// <summary>
        /// PrintedInfos 集合的最大尺寸。缺省为 100
        /// </summary>
        public int MaxPrintedInfos = 100;  // 队列最大尺寸

        /// <summary>
        /// 尚未打印的信息集合
        /// </summary>
        public List<PrintInfo> UnprintInfos = new List<PrintInfo>();

        /// <summary>
        /// UnprintInfos 集合的最大尺寸。缺省为 100
        /// </summary>
        public int MaxUnprintInfos = 100;    // 队列最大尺寸

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnInitial(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnPrint(object sender, PrintEventArgs e)
        {

        }

        /// <summary>
        /// 测试打印
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnTestPrint(object sender, PrintEventArgs e)
        {

        }

        /// <summary>
        /// 清除打印机配置
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnClearPrinterPreference(object sender, PrintEventArgs e)
        {

        }

        // 对象即将被关闭
        // 常用来看看内部缓冲区是否还有尚未打印输出的内容
        /// <summary>
        /// 对象即将被关闭。常用来处理内部缓冲区是否还有尚未打印输出的内容
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnClose(object sender, EventArgs e)
        {

        }

        // 
        /// <summary>
        /// 每一次扫描读者证条码完成后触发一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnReaderBarcodeScaned(object sender, ReaderBarcodeScanedEventArgs e)
        {

        }

        // 
        /// <summary>
        /// 每一次借完成后触发一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnBorrowed(object sender, BorrowedEventArgs e)
        {
            if (e.ReaderBarcode != this.PrintInfo.CurrentReaderBarcode)
            {
                // 前面积累的打印信息，推送到相应的队列中，便于随时挑选出来进行重新打印
                if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderBarcode) == false
                    && this.PrintInfo.HasContent() == true
                    )
                {
                    if (this.PrintInfo.PrintedCount == 0)
                    {
                        while (this.UnprintInfos.Count >= this.MaxUnprintInfos)
                            this.UnprintInfos.RemoveAt(0);

                        this.UnprintInfos.Add(this.PrintInfo);
                    }
                    else
                    {
                        while (this.PrintedInfos.Count >= this.MaxPrintedInfos)
                            this.PrintedInfos.RemoveAt(0);

                        this.PrintedInfos.Add(this.PrintInfo);
                    }
                }

                this.PrintInfo = new PrintInfo();
            }

            this.PrintInfo.CurrentReaderBarcode = e.ReaderBarcode;
            if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderSummary) == true)
                this.PrintInfo.CurrentReaderSummary = e.ReaderSummary;

            this.PrintInfo.PrintedCount = 0;    // 把打印次数清为0，表示内容已经变化，新版本(增补后的)内容尚未打印

            BorrowItemInfo info = new BorrowItemInfo();
            info.OperName = e.OperName;
            info.ItemBarcode = e.ItemBarcode;
            info.BiblioSummary = e.BiblioSummary;
            info.LatestReturnDate = e.LatestReturnDate;
            info.Period = e.Period;
            info.BorrowCount = e.BorrowCount;
            info.TimeSpan = e.TimeSpan;

            info.BorrowOperator = e.BorrowOperator;
            this.PrintInfo.BorrowItems.Add(info);
        }

        // return:
        //      -1  error
        //      0   未能推送
        //      1    已经推送
        internal int PushCurrentToQueue(out string strError)
        {
            strError = "";

            if (this.PrintInfo.HasContent() == false)
            {
                strError = "当前没有内容，不必推送";
                return 0; // 当前没有内容的就不必推送
            }

            if (this.PrintInfo.PrintedCount != 0)
            {
                strError = "当前内容已存在于“已打印队列”中";
                return 0;
            }


            while (this.UnprintInfos.Count >= this.MaxUnprintInfos)
                this.UnprintInfos.RemoveAt(0);

            this.UnprintInfos.Add(this.PrintInfo);

            this.PrintInfo = new PrintInfo();
            strError = "当前内容被推送到“未打印队列”中";
            return 1;
        }

        // 
        /// <summary>
        /// 每一次还完成后触发一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnReturned(object sender, 
            ReturnedEventArgs e)
        {
            if (e.ReaderBarcode != this.PrintInfo.CurrentReaderBarcode)
            {
                // 前面积累的打印信息，推送到相应的队列中，便于随时挑选出来进行重新打印
                if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderBarcode) == false
                    && this.PrintInfo.HasContent() == true
                    )
                {
                    if (this.PrintInfo.PrintedCount == 0)
                    {
                        while (this.UnprintInfos.Count >= this.MaxUnprintInfos)
                            this.UnprintInfos.RemoveAt(0);

                        this.UnprintInfos.Add(this.PrintInfo);
                    }
                    else
                    {
                        while (this.PrintedInfos.Count >= this.MaxPrintedInfos)
                            this.PrintedInfos.RemoveAt(0);

                        this.PrintedInfos.Add(this.PrintInfo);
                    }
                }

                this.PrintInfo = new PrintInfo();
            }

            this.PrintInfo.CurrentReaderBarcode = e.ReaderBarcode;
            if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderSummary) == true)
                this.PrintInfo.CurrentReaderSummary = e.ReaderSummary;

            this.PrintInfo.PrintedCount = 0;    // 把打印次数清为0，表示内容已经变化，新版本(增补后的)内容尚未打印

            // 对借阅事项进行查重
            bool bFoundDup = false;
            for (int i = 0; i < this.PrintInfo.BorrowItems.Count; i++)
            {
                BorrowItemInfo borrow = this.PrintInfo.BorrowItems[i];
                if (borrow.ItemBarcode == e.ItemBarcode)
                {
                    this.PrintInfo.BorrowItems.RemoveAt(i);
                    bFoundDup = true;
                    break;
                }
            }

            if (bFoundDup == true)
                return;

            ReturnItemInfo info = new ReturnItemInfo();
            info.OperName = e.OperName;
            info.ItemBarcode = e.ItemBarcode;
            info.BiblioSummary = e.BiblioSummary;
            info.BorrowDate = e.BorrowDate;
            info.LatestReturnDate = e.LatestReturnDate;
            info.Period = e.Period;
            info.BorrowCount = e.BorrowCount;
            info.TimeSpan = e.TimeSpan;

            info.BorrowOperator = e.BorrowOperator;
            info.ReturnOperator = e.ReturnOperator;
            string strError = "";
            int nRet = info.BuildOverdueItems(e.OverdueString, out strError);
            if (nRet == -1)
                throw new Exception("BuildOverdueItems error: " + strError);

            this.PrintInfo.ReturnItems.Add(info);
        }

        // 
        /// <summary>
        /// 每一次交罚金完成后触发一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnAmerced(object sender, AmercedEventArgs e)
        {
            if (e.ReaderBarcode != this.PrintInfo.CurrentReaderBarcode)
            {
                // 前面积累的打印信息，推送到相应的队列中，便于随时挑选出来进行重新打印
                if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderBarcode) == false
                    && this.PrintInfo.HasContent() == true
                    )
                {
                    if (this.PrintInfo.PrintedCount == 0)
                    {
                        while (this.UnprintInfos.Count >= this.MaxUnprintInfos)
                            this.UnprintInfos.RemoveAt(0);

                        this.UnprintInfos.Add(this.PrintInfo);
                    }
                    else
                    {
                        while (this.PrintedInfos.Count >= this.MaxPrintedInfos)
                            this.PrintedInfos.RemoveAt(0);

                        this.PrintedInfos.Add(this.PrintInfo);
                    }
                }

                this.PrintInfo = new PrintInfo();
            }

            this.PrintInfo.CurrentReaderBarcode = e.ReaderBarcode;
            if (String.IsNullOrEmpty(this.PrintInfo.CurrentReaderSummary) == true)
                this.PrintInfo.CurrentReaderSummary = e.ReaderSummary;

            this.PrintInfo.PrintedCount = 0;    // 把打印次数清为0，表示内容已经变化，新版本(增补后的)内容尚未打印

            this.PrintInfo.OverdueItems.AddRange(e.OverdueInfos);
        }


        // 将字符串按照规定的最大长度截断
        // parameters:
        //      nMaxBytes   最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍
        //      strTruncatedMask    发生截断时在尾部添加的标志。例如"..."
        /// <summary>
        /// 将字符串按照规定的最大长度截断。假定一个汉字等于两个西文字符宽度
        /// </summary>
        /// <param name="strText">原始字符串</param>
        /// <param name="nMaxBytes">最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍</param>
        /// <param name="strTruncatedMask">发生截断时在尾部添加的标志。例如"..."</param>
        /// <returns>返回的字符串</returns>
        public static string LimitByteWidth(string strText,
            int nMaxBytes,
            string strTruncatedMask)
        {
            string strResult = "";
            int nByteCount = 0;
            for (int i = 0; i < strText.Length; i++)
            {
                char c = strText[i];

                int v = (int)c;

                if (v < 256)
                {
                    nByteCount++;
                }
                else
                {
                    nByteCount += 2;
                }

                strResult += c;
                if (nByteCount >= nMaxBytes)
                {
                    if (String.IsNullOrEmpty(strTruncatedMask) == false)
                        strResult += strTruncatedMask;
                    break;
                }
            }

            return strResult;
        }

        // 获得一个右对齐的字符串
        /// <summary>
        /// 获得一个右对齐的字符串。假定一个汉字等于两个西文字符宽度
        /// </summary>
        /// <param name="strText">原始字符串</param>
        /// <param name="nLineBytes">一行内的总字符数</param>
        /// <returns>返回的字符串</returns>
        public static string RightAlignString(string strText,
            int nLineBytes)
        {
            int nRet = GetBytesWidth(strText);

            if (nRet >= nLineBytes)
                return strText; // 长度已经超过，无法进行右对齐

            int nDelta = nLineBytes - nRet;
            return new string(' ', nDelta) + strText;
        }

        // 获得一个居中的字符串
        /// <summary>
        /// 获得一个居中的字符串。假定一个汉字等于两个西文字符宽度
        /// </summary>
        /// <param name="strText">原始字符串</param>
        /// <param name="nLineBytes">一行内的总字符数</param>
        /// <returns>返回的字符串</returns>
        public static string CenterAlignString(string strText,
            int nLineBytes)
        {
            int nRet = GetBytesWidth(strText);

            if (nRet >= nLineBytes)
                return strText; // 长度已经超过，无法进行居中对齐

            int nDelta = nLineBytes - nRet;
            return new string(' ', nDelta/2) + strText;
        }

        // 数一数字符串内的相当于西文字符宽度bytes数
        /// <summary>
        /// 获得字符串内的相当于西文字符的字符数。假定一个汉字等于两个西文字符宽度
        /// </summary>
        /// <param name="strText">字符串</param>
        /// <returns>字符数</returns>
        public static int GetBytesWidth(string strText)
        {
            int nByteCount = 0;
            for (int i = 0; i < strText.Length; i++)
            {
                char c = strText[i];

                int v = (int)c;

                if (v < 256)
                {
                    nByteCount++;
                }
                else
                {
                    nByteCount += 2;
                }
            }

            return nByteCount;
        }

        // 将字符串规格化为固定行长，不超过限定行数字符串
        // 注：如果内容不足nMaxLines定义的行数，则有多少行就是多少行
        // 注：每一行，包括最后一行末尾都带有回车换行
        // parameters:
        //      nFirstLineMaxBytes  首行最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍
        //      nOtherLineMaxBytes  其余行的每行西文字符数极限
        /// <summary>
        /// 将字符串规格化为固定行长，不超过限定行数字符串
        /// 注：如果内容不足nMaxLines定义的行数，则有多少行就是多少行
        /// 注：每一行，包括最后一行末尾都带有回车换行
        /// </summary>
        /// <param name="strText">原始字符粗汉</param>
        /// <param name="nFirstLineMaxBytes">首行最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍</param>
        /// <param name="nOtherLineMaxBytes">其余行的每行西文字符数极限</param>
        /// <param name="strOtherLinePrefix">添加在其余行前面的引导字符串</param>
        /// <param name="nMaxLines">最多行数</param>
        /// <returns>返回的字符串</returns>
        public static string SplitLines(string strText,
            int nFirstLineMaxBytes,
            int nOtherLineMaxBytes,
            string strOtherLinePrefix,
            int nMaxLines)
        {
            string strResult = "";
            int nByteCount = 0;
            int nLineCount = 0;


            for (int i = 0; i < strText.Length; i++)
            {
                char c = strText[i];

                int v = (int)c;

                /*
                if (Char.IsLetterOrDigit(c) == true
                    || Char.IsSymbol(c) == true)
                 * */
                if (v < 256)
                {
                    nByteCount++;
                }
                else
                {
                    nByteCount += 2;
                }

                strResult += c;
                if (nLineCount == 0)    // 第一行
                {
                    if (nByteCount >= nFirstLineMaxBytes)
                    {
                        nLineCount++;
                        if (nLineCount >= nMaxLines)
                            break;

                        if (i == strText.Length - 1)
                        {
                            strResult += "\r\n";    // 最末一行后，不需要前缀
                        }
                        else
                        {
                            strResult += "\r\n" + strOtherLinePrefix;
                        }

                        nByteCount = 0;

                    }
                }
                else
                {
                    // 其余行
                    if (nByteCount >= nOtherLineMaxBytes)
                    {
                        nLineCount++;
                        if (nLineCount >= nMaxLines)
                            break;

                        if (i == strText.Length - 1)
                        {
                            strResult += "\r\n";    // 最末一行后，不需要前缀
                        }
                        else
                        {
                            strResult += "\r\n" + strOtherLinePrefix;
                        }
                        nByteCount = 0;
                    }
                }
            }

            // 如果最后末尾没有回车换行，补充
            if (strResult.Length > 0)
            {
                if (strResult[strResult.Length - 1] != '\n')
                    strResult += "\r\n";
            }

            return strResult;
        }

        // 将字符串规格化为固定行长，固定行数的字符串
        // 注：如果内容不足nFixLines定义的行数，则后面添空行直到这个行数。本函数主要是为了适应固定高度打印的情形
        // 注：每一行，包括最后一行末尾都带有回车换行
        // parameters:
        //      nLineMaxBytes   每行最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍
        /// <summary>
        /// 将字符串规格化为固定行长，固定行数的字符串
        /// 注：如果内容不足nFixLines定义的行数，则后面添空行直到这个行数。本函数主要是为了适应固定高度打印的情形
        /// 注：每一行，包括最后一行末尾都带有回车换行
        /// </summary>
        /// <param name="strText">原始字符串</param>
        /// <param name="nLineMaxBytes">每行最大的西文字符数。这里假定每个中文字符的显示宽度为西文字符的2倍</param>
        /// <param name="nFixLines">固定的行数</param>
        /// <returns>返回的字符串</returns>
        public static string FixLines(string strText,
            int nLineMaxBytes,
            int nFixLines)
        {
            string strResult = "";
            int nByteCount = 0;
            int nLineCount = 0;
            for (int i = 0; i < strText.Length; i++)
            {
                char c = strText[i];

                int v = (int)c;

                /*
                if (Char.IsLetterOrDigit(c) == true
                    || Char.IsSymbol(c) == true)
                 * */
                if (v < 256)
                {
                    nByteCount++;
                }
                else
                {
                    nByteCount += 2;
                }

                strResult += c;
                if (nByteCount >= nLineMaxBytes)
                {
                    nLineCount++;
                    if (nLineCount >= nFixLines)
                        break;
                    strResult += "\r\n";
                    nByteCount = 0;
                }
            }

            // 补足剩余的空行
            if (nLineCount < nFixLines)
            {
                for (; nLineCount >= nFixLines; nLineCount++)
                {
                    strResult += "\r\n";
                }
            }

            return strResult;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string lpFileName,
            int dwDesiredAccess, 
            int dwShareMode, 
            int lpSecurityAttributes,
            int dwCreationDisposition ,
            int dwFlagsAndAttributes ,
            int hTemplateFile);

        const int OPEN_EXISTING = 3;

        // parameters:
        //      strPrinterName  "LPT1"
        /// <summary>
        /// 获得代表打印机的 StreamWriter 对象
        /// </summary>
        /// <param name="strPrinterName">打印机名字</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="stream">返回 StreamWriter 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错 0: 成功</returns>
        public int GetPrinterStream(string strPrinterName,
            Encoding encoding,
            out StreamWriter stream,
            out string strError)
        {
            strError = "";

            SafeFileHandle iHandle = CreateFile(strPrinterName,
                0x40000000, 0, 0, OPEN_EXISTING, 0, 0);
            // If the handle is invalid,
            // get the last Win32 error 
            // and throw a Win32Exception.
            if (iHandle.IsInvalid)
            {
                // Marshal.ThrowExceptionForHR(Marshal.GetLastWin32Error());
                stream = null;
                strError = "没有连接打印机或者打印机端口不是" + strPrinterName + "。错误码: " + Marshal.GetLastWin32Error().ToString();
                return -1;
            }

            // TODO: 据说这个构造函数被废除了？
            FileStream fs = new FileStream(iHandle, FileAccess.ReadWrite);
            stream = new StreamWriter(fs, encoding); // 用于写文本

            return 0;
        }

        // 
        /// <summary>
        /// 获得打印机端口号。缺省为 LPT1
        /// </summary>
        public string PrinterName
        {
            get
            {
                return this.MainForm.AppInfo.GetString(
                    "charging_print",
                    "prnPort",
                    "LPT1");
            }
        }

        // 
        /// <summary>
        /// 当前是否为暂停打印状态
        /// </summary>
        public bool PausePrint
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "charging_print",
                    "pausePrint",
                    false);

            }
        }
    }

    /// <summary>
    /// 借书事件的参数
    /// </summary>
    public class BorrowedEventArgs : EventArgs
    {
        /// <summary>
        /// 用于显示的操作名称。为 借阅 续借 之一
        /// </summary>
        public string OperName = "";    // 显示操作名 借阅 续借 还回 丢失
        
        /// <summary>
        /// 书目摘要
        /// </summary>
        public string BiblioSummary = "";   // 书目摘要

        /// <summary>
        /// 读者摘要
        /// </summary>
        public string ReaderSummary = "";   // 读者摘要

        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = ""; // 册条码号

        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";   // 读者证条码号

        // --- 以下为和借书有关的操作信息
        /// <summary>
        /// 应还日期
        /// </summary>
        public DateTime LatestReturnDate = new DateTime(0);  // 应还日期

        // 借书期限。例如“20day”
        /// <summary>
        /// 期限。例如“20day”
        /// </summary>
        public string Period = "";

        // 当前为续借的第几次？0表示初次借阅
        /// <summary>
        /// 当前为续借的第几次？0表示初次借阅
        /// </summary>
        public long BorrowCount = 0;

        /// <summary>
        /// 操作耗费的时间
        /// </summary>
        public TimeSpan TimeSpan = new TimeSpan(0); // 操作耗费的时间

        // 2008/5/9 new add
        /// <summary>
        /// 册记录 XML 字符串
        /// </summary>
        public string ItemXml = ""; // 册记录XML。根据OnInitial()中是否设置了this.MainForm.ChargingNeedReturnItemXml = true，这个值可能为空

        // 2011/6/26
        /// <summary>
        /// 借书操作者
        /// </summary>
        public string BorrowOperator = "";

        /// <summary>
        /// 出纳窗
        /// </summary>
        public IChargingForm ChargingForm = null;
    }

    /// <summary>
    /// 还书事件的参数
    /// </summary>
    public class ReturnedEventArgs : EventArgs
    {
        /// <summary>
        /// 用于显示的操作名称。为 还回 丢失 之一
        /// </summary>
        public string OperName = "";    // 显示操作名 借阅 续借 还回 丢失

        /// <summary>
        /// 书目摘要
        /// </summary>
        public string BiblioSummary = "";   // 书目摘要

        /// <summary>
        /// 读者摘要
        /// </summary>
        public string ReaderSummary = "";   // 读者摘要

        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = ""; // 册条码

        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";   // 读者证条码

        /// <summary>
        /// 操作耗费的时间
        /// </summary>
        public TimeSpan TimeSpan = new TimeSpan(0); // 操作耗费的时间

        // --- 以下为和还书有关的操作信息
        // 借书日期
        /// <summary>
        /// 借书日期
        /// </summary>
        public DateTime BorrowDate = new DateTime(0);

        // 应还日期
        /// <summary>
        /// 应还日期
        /// </summary>
        public DateTime LatestReturnDate = new DateTime(0); 

        // 借书期限。例如“20day”
        /// <summary>
        /// 借书期限。例如“20day”
        /// </summary>
        public string Period = "";

        // 为续借的第几次？0表示初次借阅
        /// <summary>
        /// 为续借的第几次？0表示初次借阅
        /// </summary>
        public long BorrowCount = 0;

        // 违约金描述字符串。XML格式
        /// <summary>
        /// 违约金描述 XML 字符串
        /// </summary>
        public string OverdueString = "";

        // 2008/5/9 new add
        /// <summary>
        /// 册记录 XML 字符串
        /// </summary>
        public string ItemXml = ""; // 册记录XML。根据OnInitial()中是否设置了this.MainForm.ChargingNeedReturnItemXml = true，这个值可能为空

        // 2011/6/26
        /// <summary>
        /// 借书操作者
        /// </summary>
        public string BorrowOperator = "";

        /// <summary>
        /// 还书操作者
        /// </summary>
        public string ReturnOperator = "";

        // 2013/4/2
        /// <summary>
        /// 馆藏地点
        /// </summary>
        public string Location = "";

        /// <summary>
        /// 图书类型
        /// </summary>
        public string BookType = "";

        /// <summary>
        /// 出纳窗
        /// </summary>
        public IChargingForm ChargingForm = null;
    }

    /// <summary>
    /// 交费事件的参数
    /// </summary>
    public class AmercedEventArgs : EventArgs
    {
        /// <summary>
        /// 用于显示的操作名称。为“交费”
        /// </summary>
        public string OperName = "";    // 显示操作名 交费

        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";   // 读者证条码

        /// <summary>
        /// 读者摘要
        /// </summary>
        public string ReaderSummary = "";   // 读者摘要

        /// <summary>
        /// 操作耗费的时间
        /// </summary>
        public TimeSpan TimeSpan = new TimeSpan(0); // 操作耗费的时间

        // --- 以下为和交罚金有关的操作信息
        /// <summary>
        /// 交费信息集合
        /// </summary>
        public List<OverdueItemInfo> OverdueInfos = new List<OverdueItemInfo>();

        // 2011/6/26
        /// <summary>
        /// 交费操作者
        /// </summary>
        public string AmerceOperator = "";  // 本次操作者
    }

    /// <summary>
    /// 读者证条码号扫入事件的参数
    /// </summary>
    public class ReaderBarcodeScanedEventArgs : EventArgs
    {
        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";
    }

    /// <summary>
    /// 打印事件的参数
    /// </summary>
    public class PrintEventArgs : EventArgs
    {
        /// <summary>
        /// 打印信息
        /// </summary>
        public PrintInfo PrintInfo = null;  // [in]

        /// <summary>
        /// 打印动作类型。为 print/create 之一
        /// </summary>
        public string Action = "print"; // [in] 动作类型 print--打印 create-仅仅创建内容

        /// <summary>
        /// 返回打印结果字符串
        /// </summary>
        public string ResultString = "";    // [out] 打印结果字符串

        /// <summary>
        /// 返回打印结果字符串的格式。为 text html 之一
        /// </summary>
        public string ResultFormat = "text";    // [out] 打印结果字符串的格式 text html
    }

    // 借阅一个册的有关信息
    /// <summary>
    /// 借阅一个册的有关信息
    /// </summary>
    public class BorrowItemInfo
    {
        /// <summary>
        /// 用于显示的操作名。为 借阅 续借 之一
        /// </summary>
        public string OperName = "";    // 显示操作名 借阅 续借 还回 丢失

        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = ""; // 册条码号

        /// <summary>
        /// 书目摘要
        /// </summary>
        public string BiblioSummary = "";   // 书目摘要

        // 应还日期
        /// <summary>
        /// 应还日期
        /// </summary>
        public DateTime LatestReturnDate = new DateTime(0);

        // 借书期限。例如“20day”
        /// <summary>
        /// 借阅期限。例如“20day”
        /// </summary>
        public string Period = "";

        // 当前为续借的第几次？0表示初次借阅
        /// <summary>
        /// 当前为续借的第几次？0表示初次借阅
        /// </summary>
        public long BorrowCount = 0;

        /// <summary>
        /// 操作耗费的时间
        /// </summary>
        public TimeSpan TimeSpan = new TimeSpan(0); // 操作耗费的时间

        /// <summary>
        /// 借书操作者
        /// </summary>
        public string BorrowOperator = "";  // 2011/6/27

    }

    // 还回一个册的有关信息
    /// <summary>
    /// 还回一个册的有关信息
    /// </summary>
    public class ReturnItemInfo
    {
        /// <summary>
        /// 用于显示的操作名。为 还回 丢失 之一
        /// </summary>
        public string OperName = "";    // 显示操作名 借阅 续借 还回 丢失

        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = ""; // 册条码号

        /// <summary>
        /// 书目摘要
        /// </summary>
        public string BiblioSummary = "";   // 书目摘要


        // --- 以下为和还书有关的操作信息
        // 借书日期
        /// <summary>
        /// 借书日期
        /// </summary>
        public DateTime BorrowDate = new DateTime(0);

        // 应还日期
        /// <summary>
        /// 应还日期
        /// </summary>
        public DateTime LatestReturnDate = new DateTime(0);


        // 借书期限。例如“20day”
        /// <summary>
        /// 借阅期限。例如“20day”
        /// </summary>
        public string Period = "";

        // 为续借的第几次？0表示初次借阅
        /// <summary>
        /// 为续借的第几次？0表示初次借阅
        /// </summary>
        public long BorrowCount = 0;

        /// <summary>
        /// 操作耗费的时间
        /// </summary>
        public TimeSpan TimeSpan = new TimeSpan(0); // 操作耗费的时间
        /*
        // 违约金描述字符串。XML格式
        public string OverdueString = "";
         * */

        /// <summary>
        /// 借书操作者
        /// </summary>
        public string BorrowOperator = "";  // 2011/6/27

        /// <summary>
        /// 还书操作者
        /// </summary>
        public string ReturnOperator = "";  // 2011/6/27

        /// <summary>
        /// 交费事项集合
        /// </summary>
        public List<OverdueItemInfo> OverdueItems = new List<OverdueItemInfo>();

        // 根据XML片段，创建OverdueItems对象数组
        /// <summary>
        /// 根据 XML 片断，创建超期事项集合
        /// </summary>
        /// <param name="strOverdueString">XML片断</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错 0: 成功</returns>
        public int BuildOverdueItems(string strOverdueString,
            out string strError)
        {
            strError = "";
            this.OverdueItems.Clear();

            if (String.IsNullOrEmpty(strOverdueString) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root/>");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOverdueString;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            // 插入到最前面
            DomUtil.InsertFirstChild(dom.DocumentElement, fragment);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdue");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                OverdueItemInfo info = new OverdueItemInfo();
                info.ItemBarcode = DomUtil.GetAttr(node, "barcode");
                info.RecPath = DomUtil.GetAttr(node, "recPath");
                info.Reason = DomUtil.GetAttr(node, "reason");
                info.Price = DomUtil.GetAttr(node, "price");
                info.BorrowDate = DomUtil.GetAttr(node, "borrowDate");
                info.BorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                info.ReturnDate = DomUtil.GetAttr(node, "returnDate");
                info.BorrowOperator = DomUtil.GetAttr(node, "borrowOperator");
                info.ReturnOperator = DomUtil.GetAttr(node, "operator");
                info.ID = DomUtil.GetAttr(node, "id");

                // 2008/11/15 new add
                info.Comment = DomUtil.GetAttr(node, "comment");

                this.OverdueItems.Add(info);
            }

            return 0;
        }

    }

    // 超期事项信息
    /// <summary>
    /// 交费事项信息
    /// </summary>
    public class OverdueItemInfo
    {
        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = ""; // barcode

        /// <summary>
        /// 册记录路径
        /// </summary>
        public string RecPath = ""; // recPath

        /// <summary>
        /// 事由
        /// </summary>
        public string Reason = "";  // reason

        /// <summary>
        /// 金额
        /// </summary>
        public string Price = "";   // price

        /// <summary>
        /// 开始时间
        /// </summary>
        public string BorrowDate = "";  // borrowDate

        /// <summary>
        /// 期限
        /// </summary>
        public string BorrowPeriod = "";    // borrowPeriod

        /// <summary>
        /// 结束时间
        /// </summary>
        public string ReturnDate = "";  // returnDate

        /// <summary>
        /// 借书操作者
        /// </summary>
        public string BorrowOperator = "";  // borrowOperator

        /// <summary>
        /// 还书操作者
        /// </summary>
        public string ReturnOperator = "";    // operator

        /// <summary>
        /// 交费事项 ID
        /// </summary>
        public string ID = "";  // id

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment = ""; // comment 2008/11/15 new add

        /// <summary>
        /// 交费操作者
        /// </summary>
        public string AmerceOperator = "";  // 只在C#脚本中使用

        static string GetLineText(string strCaption,
            string strValue,
            string strLink = "")
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            StringBuilder text = new StringBuilder(4096);
            if (string.IsNullOrEmpty(strValue) == false)
            {
                text.Append("<tr>");
                text.Append("<td class='name'>" + HttpUtility.HtmlEncode(strCaption) + "</td><td class='value'>"
                    + (string.IsNullOrEmpty(strLink) == true ? HttpUtility.HtmlEncode(strValue) : strLink)
                    + "</td>");
                text.Append("</tr>");
            }

            return text.ToString();
        }

        public string ToHtmlString(string strItemLink = "")
        {
            StringBuilder text = new StringBuilder(4096);
            text.Append("<table class='amerce_item'>");

            text.Append(GetLineText("事由", this.Reason));
            text.Append(GetLineText("金额", this.Price));
            text.Append(GetLineText("册条码号", this.ItemBarcode, strItemLink));

            text.Append(GetLineText("开始时间", this.BorrowDate));
            text.Append(GetLineText("开始操作者", this.BorrowOperator));
            text.Append(GetLineText("期限", this.BorrowPeriod));
            text.Append(GetLineText("结束时间", this.ReturnDate));
            text.Append(GetLineText("结束操作者", this.ReturnOperator));
            text.Append(GetLineText("交费事项 ID", this.ID));
            text.Append(GetLineText("注释", this.Comment));
            text.Append(GetLineText("交费操作者", this.AmerceOperator));

            text.Append("</table>");

            return text.ToString();
        }
    }

    // 基类主动记忆的借还操作有关信息，便于OnPrint()时使用
    /// <summary>
    /// 打印信息
    /// </summary>
    public class PrintInfo
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime = DateTime.Now;

        /// <summary>
        /// 当前读者证条码号
        /// </summary>
        public string CurrentReaderBarcode = "";

        /// <summary>
        /// 当前读者摘要
        /// </summary>
        public string CurrentReaderSummary = "";

        /// <summary>
        /// 借阅信息集合
        /// </summary>
        public List<BorrowItemInfo> BorrowItems = new List<BorrowItemInfo>();

        /// <summary>
        /// 还书信息集合
        /// </summary>
        public List<ReturnItemInfo> ReturnItems = new List<ReturnItemInfo>();

        /// <summary>
        /// 交费信息集合
        /// </summary>
        public List<OverdueItemInfo> OverdueItems = new List<OverdueItemInfo>();

        /// <summary>
        /// 已经打印过的次数
        /// </summary>
        public int PrintedCount = 0;    // 已经打印过的次数

        /// <summary>
        /// 清除全部数据
        /// </summary>
        public void Clear()
        {
            this.CurrentReaderBarcode = "";
            this.CurrentReaderSummary = "";
            this.BorrowItems.Clear();
            this.ReturnItems.Clear();
            this.OverdueItems.Clear();
            this.PrintedCount = 0;
        }

        /// <summary>
        /// 当前是否有可打印的内容
        /// </summary>
        /// <returns>是否</returns>
        public bool HasContent()
        {
            if (this.BorrowItems.Count > 0
                || this.ReturnItems.Count > 0
                || this.OverdueItems.Count > 0)
                return true;

            return false;
        }
    }
}
