using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 和 整理流通信息 有关的代码
    /// </summary>
    public partial class MainForm : Form
    {
        public string VerifyLoanErrorFileName = "";

        void GetReaderAndBiblioDbNames(out List<string> reader_dbnames,
            out List<string> biblio_dbnames)
        {
            reader_dbnames = new List<string>();
            biblio_dbnames = new List<string>();

            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strDatabaseName = dtlp_item.Text;
                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                if (StringUtil.IsInList("读者库", strCreatingType) == true)
                {
                    reader_dbnames.Add(strDatabaseName);
                }
                else if (StringUtil.IsInList("书目库", strCreatingType) == true)
                {
                    biblio_dbnames.Add(strDatabaseName);
                }
            }
        }

        // 检索获得所有读者证条码
        // return:
        //      -1  error
        //      其他    文本文件的行数
        int SearchAllReaderBarcode(string strBarcodeFilename,
            out string strError)
        {
            strError = "";

            int nTotalCount = 0;

            // 创建文件
            StreamWriter sw = new StreamWriter(strBarcodeFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索 ...");
                stop.BeginLoop();

                EnableControls(false);

                try
                {
                    long lRet = Channel.SearchReader(stop,
                        "<全部>",
                        "",
                        -1,
                        "证条码",
                        "left",
                        this.Lang,
                        null,   // strResultSetName
                        "", // strOutputStyle
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        return 0;   // 2008/10/7 new add

                    long lHitCount = lRet;

                    long lStart = 0;
                    long lCount = lHitCount;

                    stop.SetProgressRange(0, lCount);

                    AppendHtml(
        "共有 " + lHitCount.ToString() + " 条读者记录。<br/>");

                    Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
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


                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lCount,
                            "id,cols",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        Debug.Assert(searchresults != null, "");

                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            //barcodes.Add(searchresults[i].Cols[0]);
                            sw.Write(searchresults[i].Cols[0] + "\r\n");
                            nTotalCount++;
                        }


                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        stop.SetMessage("共有条码 " + lHitCount.ToString() + " 个。已获得条码 " + lStart.ToString() + " 个");
                        stop.SetProgressValue(lStart);

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }


                    /*
                    // 排序、去重
                    stop.SetMessage("正在排序和去重");

                    // 排序
                    barcodes.Sort();

                    // 去重
                    int nRemoved = 0;
                    for (int i = 0; i < barcodes.Count; i++)
                    {
                        string strBarcode = barcodes[i];

                        for (int j = i + 1; j < barcodes.Count; j++)
                        {
                            if (strBarcode == barcodes[j])
                            {
                                barcodes.RemoveAt(j);
                                nRemoved++;
                                j--;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                     * */

                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }


            return nTotalCount;

        ERROR1:
            return -1;
        }

        // return:
        //      -1  error
        //      0   没有命中的读者记录
        //      1   正常处理完成
        public int VerifyLoan(out string strError)
        {
            strError = "";
            int nRet = 0;

            string strBarcodeFilename = this.DataDir + "\\~reader_barcode.tmp";

            /*
            Global.Clear(this.webBrowser_info);
            Global.WriteHtml(this.webBrowser_info,
                "<html><head></head><body>");
             * */

            AppendHtml(
"====================<br/>"
+ "整理流通信息<br/>"
+ "====================<br/><br/>");


            // 检索获得所有读者证条码
            nRet = SearchAllReaderBarcode(strBarcodeFilename,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            int nTotalCount = nRet;

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(strBarcodeFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strBarcodeFilename + " 失败: " + ex.Message;
                return -1;
            }

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在整理流通信息 ...");
            stop.BeginLoop();

            this.Update();

            try
            {

                stop.SetProgressRange(0, (long)nTotalCount);

                /*
                Global.AppendHtml(this.webBrowser_copyDatabaseSummary,
                    "准备升级 " + this.listView_dtlpDatabases.CheckedItems.Count.ToString() + " 个数据库内的全部记录...<br/>");
                 * */
                int nCount = 0;
                for (int i = 0; ; i++)
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


                    string strReaderBarcode = sr.ReadLine();

                    if (strReaderBarcode == null)
                        break;

                    if (String.IsNullOrEmpty(strReaderBarcode) == true)
                        continue;

                    stop.SetMessage("正在处理第 " + (i + 1).ToString() + " 条读者记录，条码为 " + strReaderBarcode);
                    stop.SetProgressValue(i + 1);

                    int nStart = 0;
                    int nPerCount = -1;
                    int nProcessedBorrowItems = 0;
                    int nTotalBorrowItems = 0;

                    for (; ; )
                    {

                        string strOutputReaderBarcode = "";
                        string[] aDupPath = null;


                        long lRet = Channel.RepairBorrowInfo(
                            stop,
                            "upgradefromdt1000_crossref",
                            strReaderBarcode,
                            "",
                            "",
                            nStart,
                            nPerCount,
                            out nProcessedBorrowItems,
                            out nTotalBorrowItems,
                            out strOutputReaderBarcode,
                            out aDupPath,
                            out strError);

                        string strOffsComment = "";
                        if (nStart > 0)
                        {
                            strOffsComment = "(偏移量" + nStart.ToString() + "开始的" + nProcessedBorrowItems + "个借阅册)";
                        }


                        if (lRet == -1)
                        {
                            AppendHtml(
                                "检查读者记录 " + strReaderBarcode + " 时" + strOffsComment + "出错: " + strError + "<br/>");
                        }
                        if (lRet == 1)
                        {
                            AppendHtml(
                                "检查读者记录 " + strReaderBarcode + " 时" + strOffsComment + "发现问题: " + strError + "<br/>");
                        }

                        if (nTotalBorrowItems > 0 && nProcessedBorrowItems == 0)
                        {
                            Debug.Assert(false, "当nTotalBorrowItems值大于0的时候(为" + nTotalBorrowItems.ToString() + ")，nProcessedBorrowItems值不能为0");
                            break;
                        }

                        nStart += nProcessedBorrowItems;
                        if (nStart >= nTotalBorrowItems)
                            break;

                    }

                    nCount++;
                }


                /*
                Global.AppendHtml(this.webBrowser_copyDatabaseSummary,
                    "升级 " + this.listView_dtlpDatabases.CheckedItems.Count.ToString() + " 个数据库内记录全部完成。<br/>");
                 * */
                AppendHtml(
                    "整理流通信息全部完成。<br/>");

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                stop.HideProgress();

                this.EnableControls(true);

                if (sr != null)
                    sr.Close();

                try
                {
                    File.Delete(strBarcodeFilename);
                }
                catch
                {
                }

            }
            return 1;
        }


    }
}
