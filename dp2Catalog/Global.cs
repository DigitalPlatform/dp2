using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.GUI;
using System.Xml;
using System.IO;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Collections;
using DigitalPlatform.dp2.Statis;

namespace dp2Catalog
{
    public class Global
    {
        public static int ParsePathParam(string strPathParam,
    out int index,
    out string strPath,
    out string strDirection,
    out string strError)
        {
            strError = "";
            index = -1;
            strPath = "";
            strDirection = "";

            Hashtable param_table = StringUtil.ParseParameters(strPathParam, ',', ':');
            if (param_table["index"] != null)
            {
                string strIndex = (string)param_table["index"];
                if (Int32.TryParse(strIndex, out index) == false)
                {
                    strError = "strPath 参数值 '" + strPathParam + "' 格式错误。index 应该为纯数字";
                    return -1;
                }

            }
            if (param_table["path"] != null)
            {
                strPath = (string)param_table["path"];
            }
            if (param_table["direction"] != null)
            {
                strDirection = (string)param_table["direction"];
            }
            return 0;
        }

        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 把 XML 字符串装入一个Web浏览器控件
        // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
        public static void SetXmlToWebbrowser(WebBrowser webBrowser,
            string strDataDir,
            string strTempFileType,
            string strXml)
        {
            if (string.IsNullOrEmpty(strXml) == true)
            {
                ClearHtmlPage(webBrowser,
                    strDataDir);
                return;
            }

            string strTargetFileName = strDataDir + "\\temp_" + strTempFileType + ".xml";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strTargetFileName = strDataDir + "\\xml.txt";
                StreamWriter sw = new StreamWriter(strTargetFileName,
    false,	// append
    System.Text.Encoding.UTF8);
                sw.Write("XML内容装入DOM时出错: " + ex.Message + "\r\n\r\n" + strXml);
                sw.Close();
                webBrowser.Navigate(strTargetFileName);

                return;
            }

            dom.Save(strTargetFileName);
            webBrowser.Navigate(strTargetFileName);
        }

        public static bool StopWebBrowser(WebBrowser webBrowser)
        {
            WebBrowserInfo info = null;
            if (webBrowser.Tag is WebBrowserInfo)
            {
                info = (WebBrowserInfo)webBrowser.Tag;
                if (info != null)
                {
                    if (info.Cleared == true)
                    {
                        webBrowser.Stop();
                        return true;
                    }
                    else
                    {
                        info.Cleared = true;
                        return false;
                    }
                }
            }

            return false;
        }


        public static void ClearHtmlPage(WebBrowser webBrowser,
            string strDataDir)
        {
            StopWebBrowser(webBrowser);

            if (String.IsNullOrEmpty(strDataDir) == true)
            {
                webBrowser.DocumentText = "(空)";
                return;
            }
            string strImageUrl = PathUtil.MergePath(strDataDir, "page_blank_128.png");
            string strHtml = "<html><body><img src='" + strImageUrl + "' width='64' height='64' alt='空'></body></html>";
            webBrowser.DocumentText = strHtml;
            /*
            string strTempFilename = strDataDir + "\\temp_blank_page.html";
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            webBrowser.Navigate(strTempFilename);
             * */
        }

        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", PathUtil.MergePath(strDataDir, "servermapped"));

            string strTempFilename = strDataDir + "\\temp_" + strTempFileType + ".html";
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            webBrowser.Navigate(strTempFilename);
        }

        // 把ListViewItem文本内容构造为tab字符分割的字符串
        public static string BuildLine(ListViewItem item)
        {
            string strLine = "";
            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (i != 0)
                    strLine += "\t";
                strLine += item.SubItems[i].Text;
            }

            return strLine;
        }

        // 根据字符串构造ListViewItem。
        // 字符串的格式为\t间隔的
        // parameters:
        //      list    可以为null。如果为null，就没有自动扩展列标题数目的功能
        public static ListViewItem BuildListViewItem(
            ListView list,
            string strLine)
        {
            ListViewItem item = new ListViewItem();
            string[] parts = strLine.Split(new char[] { '\t' });
            for (int i = 0; i < parts.Length; i++)
            {
                ListViewUtil.ChangeItemText(item, i, parts[i]);

                // 确保列标题数目够
                if (list != null)
                    ListViewUtil.EnsureColumns(list, parts.Length, 100);
            }

            return item;
        }

        // 从剪贴板中Paste行插入到ListView中当前选定的位置
        // parameters:
        //      bInsertBefore   是否前插? 如果==true前插，否则后插
        public static void PasteLinesFromClipboard(Form form,
            string strFormatList,
            ListView list,
            bool bInsertBefore)
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false
                && ido.GetDataPresent(typeof(MemLineCollection)) == false)
            {
                MessageBox.Show(form, "剪贴板中没有内容");
                return;
            }

            // 优先使用专用格式
            if (ido.GetDataPresent(typeof(MemLineCollection)) == true)
            {
                MemLineCollection mem_lines = (MemLineCollection)ido.GetData(typeof(MemLineCollection));

                if (StringUtil.IsInList(mem_lines.Format, strFormatList) == false)
                {
                    MessageBox.Show(form, "剪贴板中的内容格式不符合当前窗口要求，无法粘贴");
                    return;
                }

                int index = -1;

                if (list.SelectedIndices.Count > 0)
                    index = list.SelectedIndices[0];

                Cursor oldCursor = form.Cursor;
                form.Cursor = Cursors.WaitCursor;

                list.SelectedItems.Clear();

                foreach (MemLine line in mem_lines)
                {
                    /*
                    ListViewItem item = Global.BuildListViewItem(
                        list,
                        line.Line);
                    item.Tag = line.Tag;
                     * */
                    line.Item.Tag = line.Tag;

                    if (index == -1)
                        list.Items.Add(line.Item);
                    else
                    {
                        if (bInsertBefore == true)
                            list.Items.Insert(index, line.Item);
                        else
                            list.Items.Insert(index + 1, line.Item);

                        index++;
                    }

                    line.Item.Selected = true;
                }

                form.Cursor = oldCursor;
                return;
            }

            if (ido.GetDataPresent(DataFormats.UnicodeText) == true)
            {
                string strWhole = (string)ido.GetData(DataFormats.UnicodeText);

                int index = -1;

                if (list.SelectedIndices.Count > 0)
                    index = list.SelectedIndices[0];

                Cursor oldCursor = form.Cursor;
                form.Cursor = Cursors.WaitCursor;

                list.SelectedItems.Clear();

                string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    ListViewItem item = Global.BuildListViewItem(
                        list,
                        lines[i]);

                    if (index == -1)
                        list.Items.Add(item);
                    else
                    {
                        if (bInsertBefore == true)
                            list.Items.Insert(index, item);
                        else
                            list.Items.Insert(index + 1, item);

                        index++;
                    }

                    item.Selected = true;
                }

                form.Cursor = oldCursor;
            }
        }

        public static void ExportLinesToExcel(
    Form form,
    ListView list)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            // dlg.FileName = this.ExportExcelFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            ExcelDocument doc = null;
            // Sheet sheet = null;
            int _lineIndex = 0;

            doc = ExcelDocument.Create(dlg.FileName);

            doc.NewSheet("Sheet1");

            int nColIndex = 0;
            foreach (ColumnHeader header in list.Columns)
            {
                doc.WriteExcelCell(
                    _lineIndex,
                    nColIndex++,
                    header.Text,
                    true);
            }
            _lineIndex++;

            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < list.SelectedIndices.Count; i++)
            {
                int index = list.SelectedIndices[i];

                ListViewItem item = list.Items[index];

                List<CellData> cells = new List<CellData>();

                nColIndex = 0;
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    cells.Add(
                        new CellData(
                                    nColIndex++,
                                    subitem.Text,
                                    true,
                                    0)
                    );
                }

                doc.WriteExcelLine(
    _lineIndex,
    cells,
    WriteExcelLineStyle.None);

                _lineIndex++;
            }

            doc.SaveWorksheet();
            if (doc != null)
            {
                doc.Close();
                doc = null;
            }

            form.Cursor = oldCursor;
        }

        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        public static void CopyLinesToClipboard(
            Form form,
            string strFormat,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            MemLineCollection mem_lines = new MemLineCollection();
            mem_lines.Format = strFormat;

            List<int> indices = new List<int>();
            string strTotal = "";
            for (int i = 0; i < list.SelectedIndices.Count; i++)
            {
                int index = list.SelectedIndices[i];

                ListViewItem item = list.Items[index];
                string strLine = Global.BuildLine(item);
                strTotal += strLine + "\r\n";

                MemLine mem_line = new MemLine();
                mem_line.Item = item;
                mem_line.Tag = item.Tag;
                mem_lines.Add(mem_line);

                if (bCut == true)
                    indices.Add(index);
            }

            // Clipboard.SetDataObject(strTotal, true);

            DataObject obj = new DataObject();
            obj.SetData(typeof(MemLineCollection), mem_lines);
            obj.SetData(strTotal);
            Clipboard.SetDataObject(obj, true);

            if (bCut == true)
            {
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    list.Items.RemoveAt(index);
                }
            }

            form.Cursor = oldCursor;
        }

        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        public static void CopyLinesToClipboard(
            Form form,
            int nColumnIndex,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (ListViewItem item in list.SelectedItems)
            {
                string strLine = nColumnIndex >= item.SubItems.Count ? "" : item.SubItems[nColumnIndex].Text;
                strTotal.Append(strLine + "\r\n");
            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            if (bCut == true)
            {
                list.BeginUpdate();
                foreach (ListViewItem item in list.SelectedItems)
                {
                    list.Items.Remove(item);
                }
                /*
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    list.Items.RemoveAt(index);
                }
                 * */
                list.EndUpdate();
            }

            form.Cursor = oldCursor;
        }

        // 把正装的dp2library路径变换为倒装形态
        // 例如：从“本地服务器/中文图书/1”变换为"中文图书/1@本地服务器"
        public static string GetBackStyleDp2Path(string strPath)
        {
            int nRet = strPath.IndexOf("/");
            if (nRet == -1)
                return strPath;

            string strServerName = strPath.Substring(0, nRet).Trim();
            string strPurePath = strPath.Substring(nRet + 1).Trim();

            return strPurePath + "@" + strServerName;
        }

        // 把倒装的dp2library路径变换为正装形态
        // 例如：从"中文图书/1@本地服务器"变换为“本地服务器/中文图书/1”
        public static string GetForwardStyleDp2Path(string strPath)
        {
            int nRet = strPath.IndexOf("@");
            if (nRet == -1)
                return strPath;

            string strServerName = strPath.Substring(nRet + 1).Trim();
            string strPurePath = strPath.Substring(0, nRet).Trim();

            return strServerName + "/" + strPurePath;
        }

#if NO
        public static string ConvertSinglePinyinByStyle(string strPinyin,
    PinyinStyle style)
        {
            if (style == PinyinStyle.None)
                return strPinyin;
            if (style == PinyinStyle.Upper)
                return strPinyin.ToUpper();
            if (style == PinyinStyle.Lower)
                return strPinyin.ToLower();
            if (style == PinyinStyle.UpperFirst)
            {
                if (strPinyin.Length > 1)
                {
                    return strPinyin.Substring(0, 1).ToUpper() + strPinyin.Substring(1).ToLower();
                }

                return strPinyin;
            }

            Debug.Assert(false, "未定义的拼音风格");
            return strPinyin;
        }
#endif

        // 从ISBN号中取得出版社号部分
        // 本函数可以自动适应有978前缀的新型ISBN号
        // 注意ISBN号中必须有横杠
        // parameters:
        //      strPublisherNumber  出版社号码。不包含978-部分
        public static int GetPublisherNumber(string strISBN,
            out string strPublisherNumber,
            out string strError)
        {
            strError = "";
            strPublisherNumber = "";
            int nRet = 0;

            if (strISBN == null)
            {
                strError = "ISBN为空";
                return -1;
            }

            strISBN = strISBN.Trim();

            if (String.IsNullOrEmpty(strISBN) == true)
            {
                strError = "ISBN为空";
                return -1;
            }

            // 试探前面是不是978
            nRet = strISBN.IndexOf("-");
            if (nRet == -1)
            {
                strError = "ISBN字符串 '" + strISBN + "' 中没有横杠符号， 因此无法抽取出版社号码部分";
                return -1;
            }

            int nStart = 0; // 开始取号的位置
            string strFirstPart = strISBN.Substring(0, nRet);

            if (strFirstPart == "978")
            {
                nStart = nRet + 1;

                nRet = strISBN.IndexOf("-", nStart);

                if (nRet == -1)
                {
                    strError = "ISBN号中缺乏第二个横杠，因此无法抽取出版社号";
                    return -1;
                }

                // 此时nRet在978-7-的第二个横杠上面
            }
            else
            {
                nStart = 0;

                // 此时nRet在7-的横杠上面
            }

            nRet = strISBN.IndexOf("-", nRet + 1);
            if (nRet != -1)
            {
                strPublisherNumber = strISBN.Substring(nStart, nRet - nStart).Trim();
            }
            else
            {
                strPublisherNumber = strISBN.Substring(nStart).Trim();
            }

            return 1;
        }

        /*
        // 分析dp2library路径
        public static int ParseDp2Path(string strFullPath,
            out string strServerUrl,
            out string strLocalPath,
            out string strError)
        {
            strError = "";
            strServerUrl = "";
            strLocalPath = "";

            int nRet = strFullPath.IndexOf("@");
            if (nRet == -1)
            {
                strError = "缺乏@";
                return -1;
            }

            strServerUrl = strFullPath.Substring(nRet + 1);

            strLocalPath = strFullPath.Substring(0, nRet);

            return 0;
        }*/

        // 分析路径
        public static int ParsePath(string strFullPath,
            out string strProtocol,
            out string strPath,
            out string strError)
        {
            strError = "";
            strProtocol = "";
            strPath = "";

            int nRet = strFullPath.IndexOf(":");
            if (nRet == -1)
            {
                strError = "缺乏':'";
                return -1;
            }

            strProtocol = strFullPath.Substring(0, nRet).ToLower(); // 协议名规范为小写字符形态
            // 去掉":"
            strPath = strFullPath.Substring(nRet + 1);

            return 0;
        }


        public static void FillEncodingList(ComboBox list,
            bool bHasMarc8)
        {
            list.Items.Clear();

            List<string> encodings = Global.GetEncodingList(bHasMarc8);
            for (int i = 0; i < encodings.Count; i++)
            {
                list.Items.Add(encodings[i]);
            }

            /*
            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                list.Items.Add(infos[i].Name);
            }
             * */
        }

        // 列出encoding名列表
        // 需要把gb2312 utf-8等常用的提前
        public static List<string> GetEncodingList(bool bHasMarc8)
        {
            List<string> result = new List<string>();

            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].GetEncoding().Equals(Encoding.GetEncoding(936)) == true)
                    result.Insert(0, infos[i].Name);
                else if (infos[i].GetEncoding().Equals(Encoding.UTF8) == true)
                    result.Insert(0, infos[i].Name);
                else
                    result.Add(infos[i].Name);
            }

            if (bHasMarc8 == true)
                result.Add("MARC-8");

            return result;
        }
    }

    public class WebBrowserInfo
    {
        public bool Cleared = false;    // 是否被使用过
    }

    [Serializable]
    public class MemLineCollection : List<MemLine>
    {
        public string Format = "";
    }

    [Serializable]
    public class MemLine
    {
        public ListViewItem Item = null;
        public object Tag = null;
    }
}
