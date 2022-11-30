using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.IO;
using System.Drawing;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using Microsoft.CodeAnalysis.Operations;


namespace dp2Circulation
{
    /// <summary>
    /// 和实体检索有关的基础功能
    /// 被 ItemSearchForm 和 LabelPrintForm 所使用
    /// </summary>
    public class ItemSearchFormBase : SearchFormBase
    {
        // 实体数据库名 --> 册条码号 列号
        // internal Hashtable m_tableBarcodeColIndex = new Hashtable();

        // internal bool m_bBiblioSummaryColumn = true; // 是否在浏览列表中 加入书目摘要列
        internal int m_nBiblioSummaryColumn = 1; // 在浏览列表中要加入的书目信息列数

        internal bool m_bFirstColumnIsKey = false; // 当前listview浏览列的第一列是否应为key

        // 最近使用过的记录路径文件名
        internal string m_strUsedRecPathFilename = "";

        // 最近使用过的条码号文件名
        internal string m_strUsedBarcodeFilename = "";

        // 最近使用过的 XML 文件名
        internal string m_strUsedXmlFilename = "";


        /// <summary>
        /// 当前窗口查询的数据库类型，用于显示的名称形态
        /// </summary>
        public override string DbTypeCaption
        {
            get
            {
                if (this.DbType == "item")
                    return "实体";
                else if (this.DbType == "comment")
                    return "评注";
                else if (this.DbType == "order")
                    return "订购";
                else if (this.DbType == "issue")
                    return "期";
                else if (this.DbType == "arrive")
                    return "预约到书";
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");
            }
        }

        // TODO: 如果册条码号为空，则根据参考 ID 返回 @refID:xxxxxxx 这样形式的内容
        // 根据 ListViewItem 对象，获得册条码号列的内容
        // parameters:
        //      bWarning    是否出现警告对话框
        /// <summary>
        /// 根据浏览行 ListViewItem 对象，获得册条码号列的内容
        /// </summary>
        /// <param name="item">浏览行 ListViewItem 对象</param>
        /// <param name="bWarning">当需要时，是否出现警告对话框</param>
        /// <param name="strBarcode">返回册条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>01: 出错; 0: 成功</returns>
        public int GetItemBarcode(
            ListViewItem item,
            bool bWarning,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从(来自事项第一列的)记录路径 '" + strRecPath + "' 获得库名时出错";
                return -1;
            }

            // 根据数据库名获得 册条码号 列号
            // return:
            //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
            //      其他  返回列 index
            int nCol = GetColumnIndex(strItemDbName, "item_barcode");
            if (nCol == -1)
            {
                // 这个实体库没有在 browse 文件中 册条码号 列
                strError = "警告：实体库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                if (bWarning == true)
                    this.MessageBoxShow(strError);

                nCol = 0;   // 这个大部分情况能奏效

                if (m_nBiblioSummaryColumn == 0)
                    nCol += 1;
                else
                    nCol += m_nBiblioSummaryColumn + 1;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12
            }

            strBarcode = ListViewUtil.GetItemText(item, nCol);
            return 0;
        }

#if NO
        int GetBarcodeColumnIndex(string strItemDbName)
        {
            int nCol = -1;
            object o = m_tableBarcodeColIndex[strItemDbName];
            if (o == null)
            {
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                nCol = temp.FindColumnByType("item_barcode");
                if (nCol == -1)
                {
#if NO
                    // 这个实体库没有在 browse 文件中 册条码号 列
                    strError = "警告：实体库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                    if (bWarning == true)
                        MessageBox.Show(this, strError);
#endif

                    nCol = 0;   // 这个大部分情况能奏效
                }
                if (m_bBiblioSummaryColumn == false)
                    nCol += 1;
                else
                    nCol += 2;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12

                m_tableBarcodeColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");
            return nCol;
        }
#endif

        // 获得一个列 type 对应的列 index，针对特定的数据库
        // return:
        //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
        //      其他  返回列 index
        int GetColumnIndex(string strItemDbName,
            string strColumnType)
        {
            int nCol = -1;

            // 根据数据库名和列 type 去获得 列号
            string strCacheKey = strItemDbName + ":" + strColumnType;

            object o = _columnIndexTable[strCacheKey];
            if (o == null)
            {
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                if (temp == null)
                    return -1;
                nCol = temp.FindColumnByType(strColumnType);
                if (nCol == -1)
                    return -1;

                if (m_nBiblioSummaryColumn == 0)
                    nCol += 1;
                else
                    nCol += m_nBiblioSummaryColumn + 1;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12

                _columnIndexTable[strCacheKey] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");
            return nCol;
        }

        public void ClearColumnIndexCache()
        {
            _columnIndexTable.Clear();
        }

        // 列 type 到 列号 index 的对照表
        // key 格式为 数据库名:type，value 为 列 index
        Hashtable _columnIndexTable = new Hashtable();

        // 2021/5/28
        // 获得特定角色的列的值
        // return:
        //      -2  没有找到列 type
        //      -1  出错
        //      >=0 列号
        public int GetColumnText(
            Record record,
            string strColumnType,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            string strRecPath = record.Path;
            // 根据记录路径获得浏览记录的数据库名。注意，不一定是实体库名
            string strItemDbName = Global.GetDbName(strRecPath);

            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从(来自事项第一列的)记录路径 '" + strRecPath + "' 获得库名时出错";
                return -1;
            }

            // return:
            //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
            //      其他  返回列 index
            int nCol = GetColumnIndex(strItemDbName, strColumnType);
            if (nCol == -1)
            {
                // 这个实体库没有在 browse 文件中 册条码号 列
                strError = "数据库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 " + strColumnType + " 的列。请注意刷新或修改此配置文件";
                return -2;
            }

            Debug.Assert(nCol > 0, "");

            int index = nCol - m_nBiblioSummaryColumn - 1;
            if (index >= 0 && record.Cols != null && index < record.Cols.Length)
                strText = record.Cols[index];
            return nCol;
        }


        // 2015/6/14
        // 获得特定角色的列的值
        // return:
        //      -2  没有找到列 type
        //      -1  出错
        //      >=0 列号
        public int GetColumnText(
            ListViewItem item,
            string strColumnType,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            // 根据记录路径获得浏览记录的数据库名。注意，不一定是实体库名
            string strItemDbName = Global.GetDbName(strRecPath);

            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从(来自事项第一列的)记录路径 '" + strRecPath + "' 获得库名时出错";
                return -1;
            }

            // return:
            //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
            //      其他  返回列 index
            int nCol = GetColumnIndex(strItemDbName, strColumnType);
            if (nCol == -1)
            {
                // 这个实体库没有在 browse 文件中 册条码号 列
                strError = "数据库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 " + strColumnType + " 的列。请注意刷新或修改此配置文件";
                return -2;
            }

            Debug.Assert(nCol > 0, "");

            strText = ListViewUtil.GetItemText(item, nCol);
            return nCol;
        }

        string _element_list = "";

        public int ImportFromXmlFile(string strFileName,
    string strStyle,
    out string strError)
        {
            strError = "";

            SetStatusMessage("");   // 清除以前残留的显示

            if (string.IsNullOrEmpty(strFileName) == true)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要打开的" + this.DbTypeCaption + "记录 XML 文件名";
                dlg.FileName = this.m_strUsedRecPathFilename;
                dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                this.m_strUsedXmlFilename = dlg.FileName;
            }
            else
                this.m_strUsedXmlFilename = strFileName;

            REDO_INPUT:
            var element_list = InputDlg.GetInput(this,
    "请指定要修改的元素名列表",
    "要修改的元素名列表(空表示全部元素):",
    _element_list);
            if (element_list == null)
                return 0;

            if (string.IsNullOrEmpty(element_list) == false)
            {
                // 检查元素名
                List<string> error_names = new List<string>();
                var names = StringUtil.SplitList(element_list);
                foreach (var name in names)
                {
                    if (Array.IndexOf(_ignore_names, name) != -1)
                        error_names.Add(name);
                }

                if (error_names.Count > 0)
                {
                    strError = $"下列元素禁止修改 {StringUtil.MakePathList(error_names)}。请重新指定元素名";
                    MessageBox.Show(this, strError);
                    goto REDO_INPUT;
                }
            }

            _element_list = element_list;

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导入记录 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导入记录 ...", "halfstop");

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始" + this.DbTypeCaption + " XML 记录导入覆盖</div>");

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this._listviewRecords);

                List<ListViewItem> items = new List<ListViewItem>();

                if (this._listviewRecords.Items.Count > 0
                    && StringUtil.IsInList("clear", strStyle) == true)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this._listviewRecords.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        this.DbType + "SearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return 0;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                // recpath --> xml
                Hashtable xml_table = new Hashtable();

                this._listviewRecords.BeginUpdate();
                try
                {
                    using (var stream = File.OpenRead(m_strUsedXmlFilename))
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        looping.Progress.SetProgressRange(0, stream.Length);

                        // 定位到 collection 元素
                        while (true)
                        {
                            bool bRet = reader.Read();
                            if (bRet == false)
                            {
                                strError = $"文件 {m_strUsedXmlFilename} 没有根元素";
                                goto ERROR1;
                            }
                            if (reader.NodeType == XmlNodeType.Element)
                                break;
                        }

                        int j = 0;
                        while (reader.Read())
                        {
                            bool display = (j % 1000) == 0;

                            if (display)
                                Application.DoEvents(); // 出让界面控制权

                            if (looping.Stopped)
                            {
                                MessageBox.Show(this, "用户中断");
                                return 0;
                            }

                            if (reader.NodeType != XmlNodeType.Element)
                                continue;

                            string record_xml = reader.ReadOuterXml();
                            XmlDocument dom = new XmlDocument();
                            dom.LoadXml(record_xml);

                            if (dom.DocumentElement == null)
                                continue;

                            string strRecPath = dom.DocumentElement.GetAttribute("path");
                            if (string.IsNullOrEmpty(strRecPath))
                            {
                                strError = $"{dom.DocumentElement.Name} 元素缺乏 path 属性";
                                goto ERROR1;
                            }
                            /*
                            if (strRecPath == null)
                                break;
                            */

                            if (display)
                                looping.Progress.SetProgressValue(stream.Position);

                            // 检查路径的正确性，检查数据库是否为实体库之一
                            string strDbName = Global.GetDbName(strRecPath);
                            if (string.IsNullOrEmpty(strDbName) == true)
                            {
                                strError = "'" + strRecPath + "' 不是合法的记录路径";
                                goto ERROR1;
                            }

                            if (this.DbType == "item")
                            {
                                if (Program.MainForm.IsItemDbName(strDbName) == false)
                                {
                                    strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的实体库名。很可能所指定的文件不是实体库的记录路径文件";
                                    goto ERROR1;
                                }
                            }
                            else if (this.DbType == "comment")
                            {
                                if (Program.MainForm.IsCommentDbName(strDbName) == false)
                                {
                                    strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的评注库名。很可能所指定的文件不是评注库的记录路径文件";
                                    goto ERROR1;
                                }
                            }
                            else if (this.DbType == "order")
                            {
                                if (Program.MainForm.IsOrderDbName(strDbName) == false)
                                {
                                    strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的订购库名。很可能所指定的文件不是订购库的记录路径文件";
                                    goto ERROR1;
                                }
                            }
                            else if (this.DbType == "issue")
                            {
                                if (Program.MainForm.IsIssueDbName(strDbName) == false)
                                {
                                    strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的期库名。很可能所指定的文件不是期库的记录路径文件";
                                    goto ERROR1;
                                }
                            }
                            else
                                throw new Exception("未知的DbType '" + this.DbType + "'");

                            xml_table[strRecPath] = record_xml;

                            ListViewItem item = new ListViewItem();
                            item.Text = strRecPath;

                            this._listviewRecords.Items.Add(item);

                            items.Add(item);
                            j++;
                        }
                    }
                }
                finally
                {
                    this._listviewRecords.EndUpdate();
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    looping.Progress,
                channel,
                items,
                "",
                //false,
                true,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 修改记录的 XML
                ListViewPatronLoader loader = new ListViewPatronLoader(channel,
    looping.Progress,
    items,
    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

                looping.Progress.SetProgressRange(0, items.Count);

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);
                List<ListViewItem> changed_items = new List<ListViewItem>();
                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    BiblioInfo info = item.BiblioInfo;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    string old_xml = info.OldXml;
                    string new_xml = xml_table[info.RecPath] as string;

                    bool reloaded = false;
                    // 如果通过记录路径找不到册记录，需要尝试用 refID 找一找
                    if (string.IsNullOrEmpty(old_xml) && string.IsNullOrEmpty(new_xml) == false)
                    {
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到了
                        nRet = TryLoadRecord(
                            channel,
                            new_xml,
                            out string xml,
                            out string item_recpath,
                            out byte[] timestamp,
                            out strError);
                        if (nRet != 1)
                        {
                            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"(用 refID)重试装载记录 '{info.RecPath}' 失败: {strError}。跳过处理") + "</div>");
                            goto CONTINUE;
                        }
                        old_xml = xml;
                        info.OldXml = old_xml;
                        ChangeInfoRecPath(this.m_biblioTable,
info,
item_recpath);
                        info.Timestamp = timestamp;
                        ListViewUtil.ChangeItemText(item.ListViewItem, 0, info.RecPath);
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode($"(用 refID)重试装载记录成功，记录路径被改变为 '{info.RecPath}'") + "</div>");
                        reloaded = true;
                    }

                    if (string.IsNullOrEmpty(old_xml)
                        || string.IsNullOrEmpty(new_xml))
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"记录中 old_xml 或 new_xml 为空，已跳过修改") + "</div>");
                        goto CONTINUE;
                    }

                    XmlDocument old_dom = new XmlDocument();
                    old_dom.LoadXml(old_xml);
                    XmlDocument new_dom = new XmlDocument();
                    new_dom.LoadXml(new_xml);

                    if (reloaded == false)
                    {
                        // 对比 parent 元素不应该有变化
                        string old_parent = DomUtil.GetElementText(old_dom.DocumentElement, "parent");
                        string new_parent = DomUtil.GetElementText(new_dom.DocumentElement, "parent");

                        // parent 元素发生了变化
                        if (old_parent != new_parent)
                        {
                            // 尝试用 refID 重新装载记录
                            {
                                // return:
                                //      -1  出错
                                //      0   没有找到
                                //      1   找到了
                                nRet = TryLoadRecord(
                                    channel,
                                    new_xml,
                                    out string xml,
                                    out string item_recpath,
                                    out byte[] timestamp,
                                    out strError);
                                if (nRet != 1)
                                {
                                    Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"(记录中 parent 元素发生了变动 '{old_parent}'-->'{new_parent}')(用 refID)重试装载记录 '{info.RecPath}' 失败: {strError}。跳过处理") + "</div>");
                                    goto CONTINUE;
                                }
                                old_xml = xml;
                                old_dom.LoadXml(old_xml);
                                info.OldXml = old_xml;
                                ChangeInfoRecPath(this.m_biblioTable,
    info,
    item_recpath);
                                info.Timestamp = timestamp;
                                ListViewUtil.ChangeItemText(item.ListViewItem, 0, info.RecPath);
                                Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode($"(记录中 parent 元素发生了变动 '{old_parent}'-->'{new_parent}')(用 refID)重试装载记录成功，记录路径被改变为 '{info.RecPath}'") + "</div>");
                                reloaded = true;
                            }
                        }
                    }

                    string changed_xml = ChangeElements(element_list,
    old_xml,
    new_xml,
    "append_comment");
                    if (changed_xml == old_xml)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug warning'>&nbsp;&nbsp;&nbsp;&nbsp;" + HttpUtility.HtmlEncode("没有发生修改") + "</div>");
                        goto CONTINUE;
                    }

                    info.NewXml = changed_xml;

                    this.m_nChangedCount++;
                    item.ListViewItem.BackColor = SystemColors.Info;
                    item.ListViewItem.ForeColor = SystemColors.InfoText;

                CONTINUE:
                    i++;
                    looping.Progress.SetProgressValue(i);
                }
            }
            catch (System.Xml.XPath.XPathException ex)
            {
                strError = $"元素名列表 '{element_list}' 不合法: {ex.Message}";
                return -1;
            }
            catch (Exception ex)
            {
                strError = $"ImportFromXmlFile() 出现异常: {ExceptionUtil.GetDebugText(ex)}";
                return -1;
            }
            finally
            {
                this.EnableControls(true);

                this.ReturnChannel(channel);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束" + this.DbTypeCaption + " XML 记录导入覆盖</div>");
            }

            DoViewComment(false);
            return 1;
        ERROR1:
            return -1;
            // MessageBox.Show(this, strError);
        }

        static void ChangeInfoRecPath(Hashtable table,
            BiblioInfo info,
            string new_recpath)
        {
            if (info.RecPath == new_recpath)
                return;
            if (table.ContainsKey(info.RecPath))
                table.Remove(info.RecPath);

            info.RecPath = new_recpath;
            table[new_recpath] = info;
        }

        // parameters:
        //      ref_xml 用于参考的 XML 记录
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到了
        int TryLoadRecord(
            LibraryChannel channel,
            string ref_xml,
            out string xml,
            out string item_recpath,
            out byte[] timestamp,
            out string strError)
        {
            strError = "";
            xml = "";
            item_recpath = "";
            timestamp = null;

            XmlDocument item_dom = new XmlDocument();
            item_dom.LoadXml(ref_xml);

            string refID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(refID))
            {
                strError = "参考记录中不存在 refID 元素";
                return 0;
            }

            // 尝试用 refID 查找
            long lRet = channel.GetItemInfo(null,
                "item",
                $"@refID:{refID}",
                "",
                "xml",
                out xml,
                out item_recpath,
                out timestamp,
                "",
                out _,
                out _,
                out strError);
            if (lRet == 0)
            {
                string barcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");

                // 再尝试用 barcode 找一次
                if (string.IsNullOrEmpty(barcode) == false)
                {
                    lRet = channel.GetItemInfo(null,
        "item",
        barcode,
        "",
        "xml",
        out xml,
        out item_recpath,
        out timestamp,
        "",
        out _,
        out _,
        out strError);
                    if (lRet == 0)
                    {
                        strError = $"用参考 ID '{refID}' 和册条码号 '{barcode}' 都没有找到记录";
                        return 0;
                    }
                    if (lRet != 1)
                    {
                        strError = $"用参考 ID '{refID}' 没有找到记录，然后再用册条码号 '{barcode}' 查找时出错: {strError}";
                        return -1;
                    }
                }
                else
                {
                    strError = $"用参考 ID '{refID}' 没有找到记录";
                    return 0;
                }
            }
            if (lRet != 1)
            {
                Debug.Assert(string.IsNullOrEmpty(strError) == false);
                return -1;
            }
            Debug.Assert(string.IsNullOrEmpty(xml) == false);
            Debug.Assert(string.IsNullOrEmpty(item_recpath) == false);
            return 1;
        }

        static string[] _ignore_names = new string[] {
            "refID",
            "parent",
            "operations",
            "checkInOutDate",
            "borrowHistory",
            "borrower",
            "borrowDate",
            "borrowPeriod",
            "returningDate",
            "oi",
            "operator",
        };

        static List<string> GetElementNames(XmlDocument new_dom)
        {
            List<string> names = new List<string>();
            var nodes = new_dom.DocumentElement.SelectNodes("*");
            foreach (XmlElement node in nodes)
            {
                if (string.IsNullOrEmpty(node.Prefix) == false)
                    continue;
                if (Array.IndexOf(_ignore_names, node.Name) != -1)
                    continue;
                names.Add(node.Name);
            }

            return names;
        }

        // Exceptions:
        //      可能会抛出异常。select() 时
        static string ChangeElements(string element_list,
            string old_xml,
            string new_xml,
            string style)
        {
            XmlDocument old_dom = new XmlDocument();
            old_dom.LoadXml(old_xml);

            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(new_xml);

            bool append_comment = StringUtil.IsInList("append_comment", style);

            List<string> names = StringUtil.SplitList(element_list);
            if (string.IsNullOrEmpty(element_list))
            {
                /*
                var nodes = new_dom.DocumentElement.SelectNodes("*");
                foreach (XmlElement node in nodes)
                {
                    if (string.IsNullOrEmpty(node.Prefix) == false)
                        continue;
                    if (Array.IndexOf(_ignore_names, node.Name) != -1)
                        continue;
                    names.Add(node.Name);
                }
                */
                // 从两个 DOM 里面抽取元素名
                names.AddRange(GetElementNames(new_dom));
                names.AddRange(GetElementNames(old_dom));
                StringUtil.RemoveDup(ref names, false);
            }
            else
            {
                // 检查元素名
                List<string> error_names = new List<string>();
                foreach (var name in names)
                {
                    if (Array.IndexOf(_ignore_names, name) != -1)
                        error_names.Add(name);
                }

                if (error_names.Count > 0)
                    throw new ArgumentException($"下列元素禁止修改 {StringUtil.MakePathList(error_names)}");
            }

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中
            foreach (string name in names)
            {
                /*
                string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                    core_entity_element_names[i]);

                DomUtil.SetElementText(domExist.DocumentElement,
                    core_entity_element_names[i], strTextNew);
                 * */
                {
                    XmlElement node_new = new_dom.DocumentElement.SelectSingleNode(name) as XmlElement;
                    if (node_new != null)
                    {
                        // 看看 dprms:missing 属性是否存在
                        if (node_new.GetAttributeNode("missing", DpNs.dprms) != null)
                            continue;
                    }
                }

                string strTextNew = DomUtil.GetElementOuterXml(new_dom.DocumentElement,
                    name);
                string strTextOld = DomUtil.GetElementOuterXml(old_dom.DocumentElement,
                    name);
                // 2022/8/9
                if (IsEmpty(strTextNew) && IsEmpty(strTextOld))
                    continue;
                if (strTextNew == strTextOld)
                    continue;
                // 在 comment 元素中记载修改前的内容
                if (append_comment)
                {
                    string old_content = DomUtil.GetElementInnerText(old_dom.DocumentElement, name);
                    string new_content = DomUtil.GetElementInnerText(new_dom.DocumentElement, name);

                    string old_comment = DomUtil.GetElementText(old_dom.DocumentElement, "comment");
                    string new_comment = old_comment;
                    if (string.IsNullOrEmpty(new_comment) == false)
                        new_comment += "; ";
                    new_comment += $"{DateTime.Now.ToString()} 修改 {name}:'{old_content}'--'{new_content}'";

                    DomUtil.SetElementText(old_dom.DocumentElement, "comment", new_comment);
                }
                DomUtil.SetElementOuterXml(old_dom.DocumentElement,
                    name, strTextNew);
            }

            return old_dom.OuterXml;
        }

        static bool IsEmpty(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return true;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            if (dom.DocumentElement.HasAttributes == false
                && string.IsNullOrEmpty(dom.DocumentElement.InnerXml.Trim()))
                return true;
            return false;
        }

        /// <summary>
        /// 从记录路径文件导入
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="strStyle">风格。"clear" 表示加入新内容前试图清除 list 中已有的内容，会出现对话框警告。如果没有这个值，则追加到现有内容后面</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错 0: 放弃处理 1:正常结束</returns>
        public int ImportFromRecPathFile(string strFileName,
            string strStyle,
            out string strError)
        {
            strError = "";

            SetStatusMessage("");   // 清除以前残留的显示

            if (string.IsNullOrEmpty(strFileName) == true)
            {
                var ret = (int)this.Invoke((Func<int>)(() =>
                {
                    OpenFileDialog dlg = new OpenFileDialog();

                    dlg.Title = "请指定要打开的" + this.DbTypeCaption + "记录路径文件名";
                    dlg.FileName = this.m_strUsedRecPathFilename;
                    dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
                    dlg.RestoreDirectory = true;

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return 0;

                    this.m_strUsedRecPathFilename = dlg.FileName;
                    return 1;
                }));
                if (ret == 0)
                    return 0;
            }
            else
                this.m_strUsedRecPathFilename = strFileName;

            StreamReader sr = null;
            // bool bSkipBrowse = false;

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(this.m_strUsedRecPathFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + this.m_strUsedRecPathFilename + " 失败: " + ex.Message;
                goto ERROR1;
            }

            /*
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导入记录路径 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导入记录路径 ...", "halfstop");

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this._listviewRecords);
                looping.Progress.SetProgressRange(0, sr.BaseStream.Length);

                List<ListViewItem> items = new List<ListViewItem>();

                if (this._listviewRecords.Items.Count > 0
                    && StringUtil.IsInList("clear", strStyle) == true)
                {
                    DialogResult result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this._listviewRecords.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        this.DbType + "SearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    });
                    if (result == DialogResult.Cancel)
                        return 0;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                this.Invoke((Action)(() =>
                {
                    this._listviewRecords.BeginUpdate();
                }));
                try
                {
                    int i = 0;
                    for (; ; )
                    {
                        bool display = (i % 1000) == 0;

                        /*
                        if (display)
                            Application.DoEvents(); // 出让界面控制权
                        */

                        if (looping.Stopped)
                        {
                            this.MessageBoxShow("用户中断");
                            return 0;
                        }

                        string strRecPath = sr.ReadLine();

                        if (display)
                            looping.Progress.SetProgressValue(sr.BaseStream.Position);

                        if (strRecPath == null)
                            break;

                        // 检查路径的正确性，检查数据库是否为实体库之一
                        string strDbName = Global.GetDbName(strRecPath);
                        if (string.IsNullOrEmpty(strDbName) == true)
                        {
                            strError = "'" + strRecPath + "' 不是合法的记录路径";
                            goto ERROR1;
                        }

                        if (this.DbType == "item")
                        {
                            if (Program.MainForm.IsItemDbName(strDbName) == false)
                            {
                                strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的实体库名。很可能所指定的文件不是实体库的记录路径文件";
                                goto ERROR1;
                            }
                        }
                        else if (this.DbType == "comment")
                        {
                            if (Program.MainForm.IsCommentDbName(strDbName) == false)
                            {
                                strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的评注库名。很可能所指定的文件不是评注库的记录路径文件";
                                goto ERROR1;
                            }
                        }
                        else if (this.DbType == "order")
                        {
                            if (Program.MainForm.IsOrderDbName(strDbName) == false)
                            {
                                strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的订购库名。很可能所指定的文件不是订购库的记录路径文件";
                                goto ERROR1;
                            }
                        }
                        else if (this.DbType == "issue")
                        {
                            if (Program.MainForm.IsIssueDbName(strDbName) == false)
                            {
                                strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的期库名。很可能所指定的文件不是期库的记录路径文件";
                                goto ERROR1;
                            }
                        }
                        else if (this.DbType == "arrive")
                        {
                            if (Program.MainForm.ArrivedDbName != strDbName)
                            {
                                strError = $"路径 '{ strRecPath}' 中的数据库名 '{ strDbName}' 不是合法的预约到书库名({Program.MainForm.ArrivedDbName})。很可能所指定的文件不是预约到书库的记录路径文件";
                                goto ERROR1;
                            }
                        }
                        else
                            throw new Exception("未知的DbType '" + this.DbType + "'");

                        ListViewItem item = new ListViewItem();
                        item.Text = strRecPath;

                        this.Invoke((Action)(() =>
                        {
                            this._listviewRecords.Items.Add(item);
                        }));

                        items.Add(item);

#if NO
                    if (bSkipBrowse == false
                        && !(Control.ModifierKeys == Keys.Control))
                    {
                        int nRet = RefreshBrowseLine(item,
                out strError);
                        if (nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
                                "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
                                this.DbType + "SearchForm",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                bSkipBrowse = true;
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                            {
                                strError = "已中断";
                                break;
                            }
                        }
                    }
#endif

                        i++;
                    }
                }
                finally
                {
                    this.Invoke((Action)(() =>
                    {
                        this._listviewRecords.EndUpdate();
                    }));
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    looping.Progress,
                    channel,
                    items,
                    "",
                    // false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    looping.Progress,   // false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);

                this.ReturnChannel(channel);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();
                _stop.Style = StopStyle.None;
                */
                EndLoop(looping);

                if (sr != null)
                    sr.Close();
            }
            return 1;
        ERROR1:
            return -1;
            // MessageBox.Show(this, strError);
        }

        internal void ClearListViewItems()
        {
            this.Invoke((Action)(() =>
            {

                this._listviewRecords.Items.Clear();

                /*
                // 2008/11/22 
                this.SortColumns.Clear();
                SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);
                 * */
                ListViewUtil.ClearSortColumns(this._listviewRecords);

                // 清除所有需要确定的栏标题
                for (int i = 1; i < this._listviewRecords.Columns.Count; i++)
                {
                    this._listviewRecords.Columns[i].Text = i.ToString();
                }
            }));

            ClearBiblioTable();
            ClearCommentViewer();
        }

        // 刷新一行内除了记录路径外的其他列
        // parameters:
        //      cols    检索结果中的浏览列
        internal override void RefreshOneLine(ListViewItem item,
            string[] cols,
            bool bClearRestColumns)
        {
            if (cols == null)
            {
                int c = 0;
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    if (c != 0)
                        subitem.Text = "";
                    c++;
                }
            }
            else
            {
                /*
                int max_length = 0;

                // 2021/9/1
                if (cols != null)
                    max_length = cols.Length + 1;
                */

                int key_cols = (m_bFirstColumnIsKey ? 1 : 0);

                int c = 0;
                for (; c < cols.Length; c++)
                {
                    int index = 0;
                    if (this.m_nBiblioSummaryColumn == 0)
                        index = c + 1 + key_cols;
                    else
                        index = c + key_cols + (m_nBiblioSummaryColumn + 1);

                    ListViewUtil.ChangeItemText(item,
                    index,
                    cols[c]);

                    /*
                    // 2021/9/28
                    // 计算最大列数
                    if (max_length < index + 1)
                        max_length = index + 1;
                    */
                }

                if (bClearRestColumns)
                {
                    // 清除余下的列内容
                    if (this.m_nBiblioSummaryColumn == 0)
                        c += 1 + key_cols;
                    else
                        c += key_cols + m_nBiblioSummaryColumn + 1;

                    for (; c < item.SubItems.Count; c++)
                    {
                        item.SubItems[c].Text = "";
                    }
                }

                // 2021/9/28
                // 确保列标题列数足够
                /*
                if (max_length > 0)
                    ListViewUtil.EnsureColumns(item.ListView, max_length);
                */
                ListViewUtil.EnsureColumns(item.ListView, item.SubItems.Count);

            }
        }

        // 包装后的版本
        internal int FillBiblioSummaryColumn(
    LibraryChannel channel,
    List<ListViewItem> items,
    bool bBeginLoop,
    out string strError)
        {
            if (bBeginLoop)
            {
                var looping = BeginLoop(this.DoStop, "正在刷新浏览行的书目摘要 ...", "halfstop");
                this.EnableControls(false);

                looping.Progress.SetProgressRange(0, items.Count);
                try
                {
                    return FillBiblioSummaryColumn(
    channel,
    items,
    looping.Progress,
    out strError);
                }
                finally
                {
                    EndLoop(looping);
                    this.EnableControls(true);
                }
            }
            return FillBiblioSummaryColumn(
    channel,
    items,
    null,
    out strError);
        }

        // 实体数据库名 --> parent id 列号
        internal Hashtable m_tableSummaryColIndex = new Hashtable();

        // TODO: 用 CacheableBiblioLoader 改造
        // parameters:
        //      bBeginLoop  是否要 BeginLoop()。在大循环中使用本函数，要用 false    
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal int FillBiblioSummaryColumn(
            LibraryChannel channel,
            List<ListViewItem> items,
            // bool bBeginLoop,
            Stop stop,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment"
                || this.DbType == "arrive")
            {

            }
            else
            {
                strError = $"暂不支持数据库类型 {this.DbType} 调用 FillBiblioSummaryColumn()";
                return -1;
            }

            Debug.Assert(this.DbType == "item"
    || this.DbType == "order"
    || this.DbType == "issue"
    || this.DbType == "comment"
    || this.DbType == "arrive",
    "");

            if (m_nBiblioSummaryColumn == 0)
                return 0;



#if REMOEVD
            Looping looping = null;
            if (/*_stop != null && */bBeginLoop == true)
            {
                /*
                _stop.Style = StopStyle.EnableHalfStop;
                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.Initial("正在刷新浏览行的书目摘要 ...");
                _stop.BeginLoop();
                */
                looping = BeginLoop(this.DoStop, "正在刷新浏览行的书目摘要 ...", "halfstop");

                this.EnableControls(false);

                // if (_stop != null)
                looping.stop.SetProgressRange(0, items.Count);
            }
#endif
            try
            {
                List<ListViewItem> batch = new List<ListViewItem>();
                long lStartIndex = 0;
                // 每 1000 个事项利用一次枚举器，避免 Hashtable 膨胀太大
                foreach (ListViewItem item in items)
                {
                    batch.Add(item);
                    if (batch.Count >= 1000)
                    {
                        // return:
                        //      -2  获得书目摘要的权限不够
                        //      -1  出错
                        //      0   用户中断
                        //      1   完成
                        nRet = _fillBiblioSummaryColumn(
                            stop,
                            channel,
                            batch,
                            lStartIndex,
                            true,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                            return -1;
                        if (nRet == 0)
                            return 0;
                        lStartIndex += batch.Count;
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    // return:
                    //      -2  获得书目摘要的权限不够
                    //      -1  出错
                    //      0   用户中断
                    //      1   完成
                    nRet = _fillBiblioSummaryColumn(
                        stop,
                        channel,
                        batch,
                        lStartIndex,
                        // bBeginLoop,
                        true,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        return -1;
                    if (nRet == 0)
                        return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "FillBiblioSummaryColumn() 出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
#if REMOVED
                if (/*_stop != null &&*/ bBeginLoop == true)
                {
                    /*
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");
                    _stop.HideProgress();
                    _stop.Style = StopStyle.None;
                    */
                    EndLoop(looping);

                    this.EnableControls(true);
                }
#endif
            }
        }

        public virtual List<Order.ColumnProperty> GetBiblioColumns()
        {
            return null;
        }

        public static List<string> GetCaptionList(List<Order.ColumnProperty> defs)
        {
            List<string> results = new List<string>();
            foreach (var def in defs)
            {
                string type = def.Type;
                if (type.StartsWith("biblio_") == false)
                    continue;

                results.Add(def.Caption);
            }

            return results;
        }

        static List<string> GetTypeList(List<Order.ColumnProperty> defs)
        {
            List<string> results = new List<string>();
            foreach (var def in defs)
            {
                string type = def.Type;
                if (type.StartsWith("biblio_") == false)
                    continue;

                type = type.Substring("biblio_".Length);

                if (type == "recpath")
                    continue;

                if (type == "isbd")
                {
                    // 要求所有带有 "_area" 的 type
                    type = "areas";
                }

                results.Add(type);
            }

            return results;
        }

        // TODO: 用 CacheableBiblioLoader 改造
        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress。
        //                  如果为负数(例如 -1)，表示不触发 SetProgressValue
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal int _fillBiblioSummaryColumn(
        Stop stop,
        LibraryChannel channel,
        List<ListViewItem> items,
        long lStartIndex,
        // bool bDisplayMessage,
        bool bAutoSearch,
        out string strError)
        {
            strError = "";
            int nRet = 0;

            if (m_nBiblioSummaryColumn == 0)
                return 0;

            Debug.Assert(this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment"
                || this.DbType == "arrive",
                "");

            var column_def = GetBiblioColumns();

            // testing
            // column_def = null;
            // m_nBiblioSummaryColumn = 1;

            bool bShowed = false;

            List<string> biblio_recpaths = new List<string>();  // 尺寸可能比 items 数组小，没有包含里面不具有 parent id 列的事项
            List<int> colindex_list = new List<int>();  // 存储每个 item 对应的 parent id colindex。数组大小等于 items 数组大小

            foreach (ListViewItem item in items)
            {
#if NO
                string strRecPath = ListViewUtil.GetItemText(item, 0);
                // 根据记录路径获得数据库名
                string strItemDbName = Global.GetDbName(strRecPath);
                // 根据数据库名获得 parent id 列号

                int nCol = -1;
                object o = m_tableSummaryColIndex[strItemDbName];
                if (o == null)
                {
                    ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                    nCol = temp.FindColumnByType("parent_id");
                    if (nCol == -1)
                    {
                        colindex_list.Add(-1);
                        continue;   // 这个实体库没有 parent id 列
                    }
                    nCol += 2;
                    if (this.m_bFirstColumnIsKey == true)
                        nCol++; // 2013/11/12
                    m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
                }
                else
                    nCol = (int)o;

                Debug.Assert(nCol > 0, "");

                colindex_list.Add(nCol);

                // 获得 parent id
                string strText = ListViewUtil.GetItemText(item, nCol);

                string strBiblioRecPath = "";
                // 看看是否已经是路径 ?
                if (strText.IndexOf("/") == -1)
                {
                    // 获得对应的书目库名
                    strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strError = "数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        return -1;
                    }
                    strBiblioRecPath = strBiblioRecPath + "/" + strText;

                    ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
                }
                else
                    strBiblioRecPath = strText;
#endif
                // 2021/10/18
                if (item.Text.StartsWith("error:"))
                {
                    colindex_list.Add(-1);
                    continue;
                }

                int nCol = -1;

                // TODO: 如果没有 parent id 列，而有 item barcode 列也可以
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(
                    stop,
                    channel,
                    item,
                    bAutoSearch,   // true 如果遇到没有 parent id 列的时候速度较慢
                    out nCol,
                    out string strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    if (bShowed == false)
                    {
                        this.MessageBoxShow(strError);
                        bShowed = true;
                    }
                    colindex_list.Add(-1);
                    continue;
                    // return -1;
                }
                if (nRet == 0)
                {
                    colindex_list.Add(-1);
                    continue;
                }

                // 2017/7/5
                if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    colindex_list.Add(-1);
                    continue;
                }

                if (string.IsNullOrEmpty(strBiblioRecPath) == false
                    && nCol == -1)
                    colindex_list.Add(0);
                else
                    colindex_list.Add(nCol);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                    biblio_recpaths.Add(strBiblioRecPath);
                else
                {
                    Debug.Assert(false, "");
                }
            }

            string format = "summary";
            if (column_def != null)
                format = "table:slim|" + StringUtil.MakePathList(GetTypeList(column_def), "|");

            CacheableBiblioLoader loader = new CacheableBiblioLoader
            {
                Channel = channel,
                Stop = stop,    // this._stop,
                Format = format,    // "summary", "table",
                GetBiblioInfoStyle = GetBiblioInfoStyle.None,
                RecPaths = biblio_recpaths
            };

            loader.Prompt += loader_Prompt;

            var enumerator = loader.GetEnumerator();

            int i = 0;
            foreach (ListViewItem item in items)
            {
                // Application.DoEvents();	// 出让界面控制权

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                string strRecPath = ListViewUtil.GetItemText(item, 0);
                if (stop != null /*&& bDisplayMessage == true*/)
                {
                    if ((i % 10) == 0)  // 2022/11/6
                        stop?.SetMessage("正在获取 " + strRecPath + " 的书目摘要 ...");
                    if (lStartIndex >= 0)   // 2022/11/6
                        stop?.SetProgressValue(lStartIndex + i);
                }

                int nCol = colindex_list[i];
                if (nCol == -1)
                {
                    ListViewUtil.ChangeItemText(item,
                    this.m_bFirstColumnIsKey == false ? 1 : 2,
                    "");
                    ClearOneChange(item, true); // 清除内存中的修改
                    i++;
                    continue;
                }

                try
                {
                    bool bRet = enumerator.MoveNext();
                    if (bRet == false)
                    {
                        Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                        // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                        strError = "error 1";
                        return -1;
                    }
                }
                catch (ChannelException ex)
                {
                    strError = ex.Message;
                    if (ex.ErrorCode == ErrorCode.AccessDenied)
                        return -2;
                    return -1;
                }

                BiblioItem biblio = (BiblioItem)enumerator.Current;
                // Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                if (column_def == null)
                {
                    ListViewUtil.ChangeItemText(item,
                    this.m_bFirstColumnIsKey == false ? 1 : 2,
                    biblio.Content);
                }
                else
                {
                    var columns = GetBiblioColumnText(column_def,
biblio.Content, biblio.RecPath);
                    int start = this.m_bFirstColumnIsKey == false ? 1 : 2;
                    foreach (string text in columns)
                    {
                        ListViewUtil.ChangeItemText(item,
    start++,
    text);
                    }
                }

                ClearOneChange(item, true); // 清除内存中的修改
                i++;
            }

            return 1;
        }

        static List<string> GetBiblioColumnText(List<Order.ColumnProperty> columns,
            string xml,
            string biblio_recpath)
        {
            if (xml == "{null}")
                return new List<string> { xml };
            List<string> results = new List<string>();
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                results.Add($"!error:书目表格数据出错:{ex.Message}");
                return results;
            }

            foreach (var column in columns)
            {
                string type = column.Type;

                if (type.StartsWith("biblio_") == false)
                {
                    results.Add("");
                    continue;
                }

                type = type.Substring("biblio_".Length);

                if (type == "recpath")
                {
                    results.Add(biblio_recpath);
                    continue;
                }

                if (type == "isbd")
                {
                    results.Add(BuildIsbd(dom));
                    continue;
                }

                /*
                XmlElement line = dom.DocumentElement.SelectSingleNode($"line[@type='{type}']") as XmlElement;
                if (line == null)
                {
                    results.Add("");
                    continue;
                }

                results.Add(line.GetAttribute("value")?.Replace("\n", " "));
                */
                results.Add(GetValue(dom, type));
            }

            return results;
        }

        static string[] area_types = new string[] {
"title_area",
"edition_area",
"material_specific_area",
"publication_area",
"material_description_area",
"series_area",
"notes_area",
"resource_identifier_area",
        };

        static string BuildIsbd(XmlDocument table_dom)
        {
            /*
* content_form_area
* title_area
* edition_area
* material_specific_area
* publication_area
* material_description_area
* series_area
* notes_area
* resource_identifier_area
* * */
            StringBuilder text = new StringBuilder();
            foreach (var type in area_types)
            {
                string value = GetValue(table_dom, type);
                if (string.IsNullOrEmpty(value) == false)
                {
                    if (text.Length > 0)
                        text.Append(". -- ");
                    text.Append(value);
                }
            }

            return text.ToString();
        }

        static string GetValue(XmlDocument dom, string type)
        {
            // 先尝试用 XPath 直接获得
            {
                XmlElement line = dom.DocumentElement.SelectSingleNode($"line[@type='{type}']") as XmlElement;
                if (line != null)
                    return line.GetAttribute("value")?.Replace("\n", " ");
            }

            // 然后尝试从所有 line 元素中遍历 type 属性中局部包含所需值的
            {
                XmlNodeList lines = dom.DocumentElement.SelectNodes("line");
                foreach (XmlElement line in lines)
                {
                    string current_type = line.GetAttribute("type");
                    if (StringUtil.IsInList(type, current_type))
                        return line.GetAttribute("value")?.Replace("\n", " ");
                }

                return "";  // 没有找到
            }
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "ItemSearchForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "cancel";
                else if (result == System.Windows.Forms.DialogResult.No)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

        // 2021/10/15
        // 获得可以粗略描述一行的文字
        public static string GetSummaryText(ListViewItem item)
        {
            List<string> results = new List<string>();
            foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
            {
                if (string.IsNullOrEmpty(subitem.Text) == false)
                    results.Add(subitem.Text);
            }

            return StringUtil.MakePathList(results, "; ");
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 renyh@xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentException
Message: RecPaths 中不应包含空元素
Stack:
在 dp2Circulation.CacheableBiblioLoader.<GetEnumerator>d__0.MoveNext()
在 dp2Circulation.ItemSearchFormBase._fillBiblioSummaryColumn(LibraryChannel channel, List`1 items, Int64 lStartIndex, Boolean bDisplayMessage, Boolean bAutoSearch, String& strError)
在 dp2Circulation.ItemSearchForm.FillBrowseList(LibraryChannel channel, ItemQueryParam query, Int64 lHitCount, Boolean bOutputKeyCount, Boolean bOutputKeyID, Boolean bQuickLoad, String& strError)
在 dp2Circulation.ItemSearchForm.DoSearch(Boolean bOutputKeyCount, Boolean bOutputKeyID, ItemQueryParam input_query, Boolean bClearList)
在 dp2Circulation.ItemSearchForm.toolStripButton_search_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)
在 System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.28.6347.382, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.2.9200.0
操作时间 2017/5/18 12:30:01 (Thu, 18 May 2017 12:30:01 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * * */
        // 获得事项所从属的书目记录的路径
        // parameters:
        //      channel 一般情况不会用到 channel 来请求 API
        //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
        // return:
        //      -1  出错
        //      0   相关数据库没有配置 parent id 浏览列
        //      1   找到
        public virtual int GetBiblioRecPath(
            Stop stop,
            LibraryChannel channel,
            ListViewItem item,
            bool bAutoSearch,
            out int nCol,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            nCol = -1;
            strBiblioRecPath = "";
            int nRet = 0;

            // 这是事项记录路径
            string strRecPath = ListViewUtil.GetItemText(item, 0);

            // 2021/10/18
            if (strRecPath.StartsWith("error:"))
            {
                strError = $"浏览行 '{GetSummaryText(item)}' 记录路径列内容为错误信息('{strRecPath}')，无法获得书目记录路径";
                return -1;
            }

            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = $"浏览行 '{GetSummaryText(item)}' 记录路径列没有内容，无法获得书目记录路径";
                return -1;
            }

            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            // 根据数据库名获得 parent id 列号

            object o = m_tableSummaryColIndex[strItemDbName];
            if (o == null)
            {
                if (Program.MainForm.NormalDbProperties == null
                    || Program.MainForm.NormalDbProperties.Count == 0)
                {
                    strError = "普通数据库属性尚未初始化。这通常是因为刚进入内务时候初始化阶段出现错误导致的。请退出内务重新进入，并注意正确登录";
                    return -1;
                }
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                if (temp == null)
                {
                    strError = "实体库 '" + strItemDbName + "' 没有找到列定义";
                    return -1;
                }

                nCol = temp.FindColumnByType("parent_id");
                if (nCol == -1)
                {
                    if (bAutoSearch == false)
                    {
                        strError = "实体库 " + strItemDbName + " 的浏览格式没有配置 parent id 列";
                        return 0;   // 这个实体库没有 parent id 列
                    }

                    // TODO: 这里关于浏览列 type 的判断应该通盘考虑，设计为通用功能，减少对特定库类型的判断依赖

                    string strQueryString = "";
                    if (this.DbType == "item")
                    {
                        Debug.Assert(this.DbType == "item", "");

                        strQueryString = "@path:" + strRecPath;
                    }
                    else if (this.DbType == "arrive")
                    {
                        int nRefIDCol = temp.FindColumnByType("item_refid");
                        if (nRefIDCol == -1)
                        {
                            strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_refid 列";
                            return 0;
                        }

                        strQueryString = ListViewUtil.GetItemText(item, nRefIDCol + 2);

                        if (string.IsNullOrEmpty(strQueryString) == false)
                        {
                            strQueryString = "@refID:" + strQueryString;
                        }
                        else
                        {
                            int nItemBarcodeCol = temp.FindColumnByType("item_barcode");
                            if (nRefIDCol == -1)
                            {
                                strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_barcode 列";
                                return 0;
                            }
                            strQueryString = ListViewUtil.GetItemText(item, nItemBarcodeCol + 2);
                            if (string.IsNullOrEmpty(strQueryString) == true)
                            {
                                strError = "册条码号栏为空，无法获得书目记录路径";
                                return 0;
                            }
                        }
                    }

                    string strItemRecPath = "";
                    if (string.IsNullOrEmpty(strQueryString) == false)
                    {
                        Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");

                        nRet = SearchTwoRecPathByBarcode(
                            stop,   // this._stop,
                            channel,
                            strQueryString,    // "@path:" + strRecPath,
                            out strItemRecPath,
                            out strBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "获得书目记录路径时出错: " + strError;
                            return -1;
                        }
                        else if (nRet == 0)
                        {
                            strError = "检索词 '" + strQueryString + "' 没有找到记录";
                            return -1;
                        }
                        else if (nRet > 1) // 命中发生重复
                        {
                            strError = "检索词 '" + strQueryString + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                            return -1;
                        }
                    }
                    else
                    {
                        nRet = SearchBiblioRecPath(
                            stop,   // this._stop,
                            channel,
                            this.DbType,
                            strRecPath,
                            out strBiblioRecPath,
                            out strError);
                    }
                    if (nRet == -1)
                    {
                        strError = "获得书目记录路径时出错: " + strError;
                        return -1;
                    }
                    else if (nRet == 0)
                    {
                        strError = "记录路径 '" + strRecPath + "' 没有找到记录";
                        return -1;
                    }
                    else if (nRet > 1) // 命中发生重复
                    {
                        strError = "记录路径 '" + strRecPath + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                        return -1;
                    }

                    return 1;
                }

                nCol += (m_nBiblioSummaryColumn + 1);
                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12
                m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");

            // 获得 parent id
            string strText = ListViewUtil.GetItemText(item, nCol);
            // 2022/11/30
            if (strText.StartsWith("error:"))
            {
                strError = $"浏览行 '{GetSummaryText(item)}' 记录路径列内容为错误信息('{strRecPath}')，无法获得书目记录路径";
                return -1;
            }

            if (string.IsNullOrEmpty(strText) == false)
            {
                // 看看是否已经是路径 ?
                if (strText.IndexOf("/") == -1)
                {
                    // 获得对应的书目库名
                    strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strError = this.DbTypeCaption + "类型的数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        return -1;
                    }
                    strBiblioRecPath = strBiblioRecPath + "/" + strText;

                    ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
                }
                else
                    strBiblioRecPath = strText;
            }
            else
            {
                // 2017/5/18
                // 装载浏览格式的中途如果修改服务器相关数据库的 browse 配置文件可能会走到这里
                strError = "parent id 列 (index=" + nCol + ") 内容为空";
                return 1;   // 找到了列号，但是该列内容为空
            }
            return 1;
        }

        const int COLUMN_ERRORINFO = 0;

        // return:
        //      false   出现错误
        //      true    成功
        internal bool FillLineByBarcode(
            Stop stop,
            LibraryChannel channel,
            string strBarcode,
            ListViewItem item)
        {
            string strError = "";
            string strBiblioRecPath = "";
            string strItemRecPath = "";

            Debug.Assert(this.DbType == "item", "");

            // 检索册条码号，检索出其从属的书目记录路径。
            int nRet = SearchTwoRecPathByBarcode(
                stop,   // this._stop,
                channel,
                strBarcode,
                out strItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "error: " + strError);
                return false;
            }
            else if (nRet == 0)
            {
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "error: 册条码号 '" + strBarcode + "' 没有找到记录"); // 2
                return false;
            }
            else if (nRet == 1)
            {
                item.Text = strItemRecPath;
            }
            else if (nRet > 1) // 命中发生重复
            {
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, "error: 册条码号 '" + strBarcode + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误");
                return false;
            }

            string strItemDbName = Global.GetDbName(strItemRecPath);
            int index = GetColumnIndex(strItemDbName, "item_barcode");
            if (index == -1)
            {
                index = 0;   // 这个大部分情况能奏效

                /*
                // 直接报错返回
                // 2021/10/18
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, $"error: 实体库 '{strItemDbName}' 的 item_barcode 角色列定义没有找到");
                return false;
                */
            }

            // 注：如果不知道 index，就没法设置条码号列内容
            ListViewUtil.ChangeItemText(item, index, strBarcode);

            // TODO: 将书目记录路径放入item.Tag中备用
            return true;
        }

        // 根据册条码号，检索出其册记录路径和从属的书目记录路径，以及馆藏地点信息。
        public static int SearchTwoRecPathByBarcode(
            Stop stop,
            LibraryChannel channel,
            string strBarcode,
            out string strItemRecPath,
            out string strLocation,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strItemRecPath = "";
            strLocation = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            long lRet = 0;

#if NO
            Debug.Assert(this.DbType == "item", "");

            if (this.DbType == "item")
                lRet = Channel.GetItemInfo(
                     stop,
                     strBarcode,
                     "xml",
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("SearchTwoRecPathByBarcode()只能在DbType=='item'时使用");
#endif
            lRet = channel.GetItemInfo(
     stop,
     strBarcode,
     "xml",
     out strItemText,
     out strItemRecPath,
     out item_timestamp,
     "recpath",
     out strBiblioText,
     out strBiblioRecPath,
     out strError);
            if (lRet == -1)
                return -1;  // error

            if (lRet == 0)
                return 0;   // not found

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemText);
            }
            catch (Exception ex)
            {
                strError = "item XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");

            return (int)lRet;
        }

        // 根据册条码号，检索出其册记录路径和从属的书目记录路径。
        public static int SearchTwoRecPathByBarcode(
            Stop stop,
            LibraryChannel channel,
            string strBarcode,
            out string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strItemRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            long lRet = 0;

#if NO
            Debug.Assert(this.DbType == "item", "");

            if (this.DbType == "item")
                lRet = Channel.GetItemInfo(
                     stop,
                     strBarcode,
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("SearchTwoRecPathByBarcode()只能在DbType=='item'时使用");
#endif
            lRet = channel.GetItemInfo(
     stop,
     strBarcode,
     null,
     out strItemText,
     out strItemRecPath,
     out item_timestamp,
     "recpath",
     out strBiblioText,
     out strBiblioRecPath,
     out strError);
            if (lRet == -1)
                return -1;  // error

            // TODO: 需要检查一下服务器代码，看看什么情况 strBiblioRecPath 会返回空？
            return (int)lRet;   // not found
        }

        // 根据册记录路径，检索出从属的书目记录路径。
        public static int SearchBiblioRecPath(
            Stop stop,
            LibraryChannel channel,
            string strDbType,
            string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            long lRet = 0;

            if (strDbType == "item")
                lRet = channel.GetItemInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "comment")
                lRet = channel.GetCommentInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "order")
                lRet = channel.GetOrderInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "issue")
                lRet = channel.GetIssueInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("未知的DbType '" + strDbType + "'");

            if (lRet == -1)
                return -1;  // error

            // TODO: 需要检查一下服务器代码，看什么时候 strBiblioRecPath 会返回空
            return (int)lRet;   // not found
        }
    }
}
