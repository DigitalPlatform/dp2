using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Web;

using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.Drawing;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 全局静态函数
    /// </summary>
    public class Global
    {
        // 检测应还书时间是否超过当前时间
        // parameters:
        //      end 应还书时间。GMT 时间
        //      now 当前时间。GMT 时间
        // return:
        //      1   超期
        //      0   没有超期
        public static int IsOver(string strUnit,
            DateTime end,
            DateTime now,
            out string strError)
        {
            strError = "";
            int nRet = DateTimeUtil.RoundTime(strUnit, ref now, out strError);
            if (nRet == -1)
                return -1;

            if (now > end)
                return 1;   // 超过
            return 0;   // 没有超过
        }

        // 看现在是否已经超期
        // return:
        //      -1  检测过程出错(是否超期则未知)
        //      1   超期
        //      0   没有超期
        public static int IsOverdue(string strBorrowDate,
            string strPeriod,
            out string strError)
        {
            strError = "";

            DateTime now = DateTime.UtcNow;
            DateTime timeEnd = new DateTime(0);
            int nRet = GetReturnDay(
                strBorrowDate,
                strPeriod,
            out timeEnd,
            out strError);
            if (nRet == -1)
                return -1;

            nRet = DateTimeUtil.ParsePeriodUnit(strPeriod,
                "day",
                out long lPeriodValue,
                out string strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "借阅期限值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }

            return IsOver(strPeriodUnit, timeEnd, now, out strError);
        }

        // 测算应还书时间
        public static int GetReturnDay(
            string strBorrowDate,
            string strPeriod,
            out DateTime timeEnd,
            out string strError)
        {
            timeEnd = new DateTime(0);
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = DateTimeUtil.ParsePeriodUnit(strPeriod,
                "day",
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "借阅期限值 '" + strPeriod + "' 格式错误: " + strError;
                return -1;
            }

            DateTime borrowdate = new DateTime((long)0);

            try
            {
                borrowdate = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
            }
            catch
            {
                strError = "借阅日期字符串 '" + strBorrowDate + "' 格式错误";
                return -1;
            }

            return GetReturnDay(
                borrowdate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out strError);
        }

        // 测算应还书日期
        // 这是理论还书时间，不考虑宽限期情况
        // parameters:
        //      timeStart   借阅开始时间。GMT时间
        //      timeEnd     返回应还回的最后时间。GMT时间
        // return:
        //      -1  出错
        //      0   成功
        public static int GetReturnDay(
            DateTime timeStart,
            long lPeriod,
            string strUnit,
            out DateTime timeEnd,
            out string strError)
        {
            strError = "";
            timeEnd = DateTime.MinValue;

            // 正规化时间
            int nRet = DateTimeUtil.RoundTime(strUnit,
                ref timeStart,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta;

            if (strUnit == "day")
                delta = new TimeSpan((int)lPeriod, 0, 0, 0);
            else if (strUnit == "hour")
                delta = new TimeSpan((int)lPeriod, 0, 0);
            else
            {
                strError = "未知的时间单位 '" + strUnit + "'";
                return -1;
            }

            timeEnd = timeStart + delta;

            // 正规化时间
            nRet = DateTimeUtil.RoundTime(strUnit,
                ref timeEnd,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // parameters:
        //      strName 例如，"KB2544514"
        public static bool IsKbInstalled(string strName)
        {
            try
            {
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\" + strName))
                {
                    return (string)baseKey.GetValue("ThisVersionInstalled") == "Y";
                }
            }
            catch
            {
                return false;
            }
        }

        // 在第一列前面插入一个空白列
        public static string[] InsertBlankColumn(string[] cols,
            int nDelta = 1)
        {
            string[] results = new string[cols == null ? nDelta : cols.Length + nDelta];
            for (int i = 0; i < nDelta; i++)
            {
                results[i] = "";
            }
            if (results.Length > 1)
                Array.Copy(cols, 0, results, nDelta, results.Length - nDelta);
            return results;
        }

        public static Control FindFocusedControl(Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        /// <summary>
        /// Activate 一个 Form。包括了处理最小化时候恢复显示的功能
        /// </summary>
        /// <param name="form">Form</param>
        public static void Activate(Form form)
        {
            if (form != null)
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;
                form.Activate();
            }
        }

        /// <summary>
        /// 判断一个字体字符串是否为虚拟的条码字体
        /// </summary>
        /// <param name="strFontString">字体字符串。如果是虚拟条码字体，则函数返回时候已经被修改为具体的条码字体名称了</param>
        /// <returns>是否为虚拟的条码字体。true 表示是</returns>
        public static bool IsVirtualBarcodeFont(ref string strFontString)
        {
            if (string.IsNullOrEmpty(strFontString) == true)
                return false;

            string strFontName = "";
            string strOther = "";
            StringUtil.ParseTwoPart(strFontString,
                ",",
                out strFontName,
                out strOther);
            if (strFontName == "barcode")
            {
                strFontString = "C39HrP24DhTt," + strOther;
                return true;
            }

            return false;
        }

        public static string GetBarcodeFontString(Font font)
        {
            if (font == null)
                return "";

            return "barcode, " + font.SizeInPoints.ToString() + "pt";
        }

        /// <summary>
        /// 根据字符串定义构造 Font 对象
        /// 能用于条码字体 C39HrP24DhTt
        /// </summary>
        /// <param name="strFontString"></param>
        /// <returns></returns>
        public static Font BuildFont(string strFontString)
        {
            if (String.IsNullOrEmpty(strFontString) == true)
                return Control.DefaultFont;

            // Create the FontConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

            Font font = (Font)converter.ConvertFromString(strFontString);

            // GDI+ 新安装字体后不能显现的 BUG 绕过方法
            if (string.IsNullOrEmpty(font.OriginalFontName) == false
                && font.OriginalFontName != font.Name)
            {
                List<FontFamily> families = new List<FontFamily>(GlobalVars.PrivateFonts.Families);

                FontFamily t = families.Find(f => string.Compare(f.Name, font.OriginalFontName, true) == 0);

                // if (families.Exists(f => string.Compare(f.Name, font.OriginalFontName, true) == 0) == true)
                if (t != null)
                {
                    Font new_font = new Font(t, font.Size, font.Style);
                    font.Dispose(); // 2017/2/27
                    return new_font;
                }
            }

            return font;
        }

        // 会自动从 PrivateFonts 中寻找
        public static Font BuildFont(string font_name, float height, GraphicsUnit unit)
        {
            Font font = new Font(
               font_name,    // "OCR-B 10 BT", 
               height, unit);
            if (string.IsNullOrEmpty(font.OriginalFontName) == false
    && font.OriginalFontName != font.Name)
            {
                List<FontFamily> families = new List<FontFamily>(GlobalVars.PrivateFonts.Families);

                FontFamily t = families.Find(f => string.Compare(f.Name, font.OriginalFontName, true) == 0);
                if (t != null)
                {
                    font.Dispose();
                    // return new Font(t, font.Size, font.Style, unit);    // unit 2017/10/19
                    return new Font(t, height, unit);   // 修改成这样观察一下效果 2017/10/19
                }
            }
            return font;
        }

        /// <summary>
        /// 设置一个文本编辑器某行的内容
        /// </summary>
        /// <param name="textbox">文本编辑器</param>
        /// <param name="nLine">行 index</param>
        /// <param name="strValue">要设置的内容</param>
        public static void SetLineText(TextBox textbox,
    int nLine,
    string strValue)
        {
            string strText = textbox.Text.Replace("\r\n", "\r");
            string[] lines = strText.Split(new char[] { '\r' });

            strText = "";
            for (int i = 0; i < Math.Max(nLine, lines.Length); i++)
            {
                if (i != 0)
                    strText += "\r\n";

                if (i == nLine)
                    strText += strValue;
                else
                {
                    if (i < lines.Length)
                        strText += lines[i];
                    else
                        strText += "";
                }

            }

            textbox.Text = strText;
        }

        // 设置或者刷新一个操作记载
        internal static int SetOperation(
            ref XmlDocument dom,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "dom.DocumentElement == null";
                return -1;
            }

            XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
            if (nodeOperations == null)
            {
                nodeOperations = dom.CreateElement("operations");
                dom.DocumentElement.AppendChild(nodeOperations);
            }

            XmlNodeList nodes = nodeOperations.SelectNodes("operation[@name='" + strOperName + "']");
            if (bAppend == true)
            {
                // 删除多余9个的
                if (nodes.Count > 9)
                {
                    for (int i = 0; i < nodes.Count - 9; i++)
                    {
                        XmlNode node = nodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }
            else
            {
                if (nodes.Count > 1)
                {
                    for (int i = 0; i < nodes.Count - 1; i++)
                    {
                        XmlNode node = nodes[i];
                        node.ParentNode.RemoveChild(node);
                    }
                }
            }

            {
                XmlNode node = null;
                if (bAppend == true)
                {
                }
                else
                {
                    node = nodeOperations.SelectSingleNode("operation[@name='" + strOperName + "']");
                }


                if (node == null)
                {
                    node = dom.CreateElement("operation");
                    nodeOperations.AppendChild(node);
                    DomUtil.SetAttr(node, "name", strOperName);
                }


                string strTime = DateTimeUtil.Rfc1123DateTimeString(DateTime.UtcNow);// app.Clock.GetClock();

                DomUtil.SetAttr(node, "time", strTime);
                DomUtil.SetAttr(node, "operator", strOperator);
                if (String.IsNullOrEmpty(strComment) == false)
                    DomUtil.SetAttr(node, "comment", strComment);
            }

            return 0;
        }

        /// <summary>
        /// 修改一个状态字符串
        /// </summary>
        /// <param name="strState">要修改的字符串</param>
        /// <param name="strAddList">要加入的值列表</param>
        /// <param name="strRemoveList">要移除的值列表</param>
        public static void ModifyStateString(ref string strState,
            string strAddList,
            string strRemoveList)
        {
            string[] adds = strAddList.Split(new char[] { ',' });
            for (int i = 0; i < adds.Length; i++)
            {
                StringUtil.SetInList(ref strState, adds[i], true);
            }
            string[] removes = strRemoveList.Split(new char[] { ',' });
            for (int i = 0; i < removes.Length; i++)
            {
                StringUtil.SetInList(ref strState, removes[i], false);
            }
        }

        /// <summary>
        /// 获得 MARC 记录中的全部编目规则字符串
        /// </summary>
        /// <param name="strMARC">MARC字符串。机内格式</param>
        /// <returns>字符串集合</returns>
        public static List<string> GetExistCatalogingRules(string strMARC)
        {
            int nRet = 0;
            List<string> results = new List<string>();
            MarcRecord record = new MarcRecord(strMARC);

            MarcNodeList subfields = record.select("field/subfield");
            foreach (MarcSubfield subfield in subfields)
            {
                if (subfield.Name == "*")
                    results.Add(subfield.Content.Trim());
                else
                {
                    string strCmd = StringUtil.GetLeadingCommand(subfield.Content);
                    if (string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        results.Add(strCmd.Substring(3));
                    }
                }
            }

            foreach (MarcField field in record.ChildNodes)
            {
                string strField = field.Text;

                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;

                {
#if NO
                    // 字段名后(字段指示符后)和第一个子字段符号之间的空白片断
                    string strIndicator = "";
                    string strContent = "";
                    if (MarcUtil.IsControlFieldName(strField.Substring(0, 3)) == true)
                    {
                        strContent = strField.Substring(3);
                    }
                    else
                    {
                        if (strField.Length >= 5)
                        {
                            strIndicator = strField.Substring(3, 2);
                            strContent = strField.Substring(3 + 2);
                        }
                        else
                            strIndicator = strField.Substring(3, 1);
                    }
#endif

                    string strBlank = field.Content;   // .Trim();
                    nRet = strBlank.IndexOf((char)MarcUtil.SUBFLD);
                    if (nRet != -1)
                        strBlank = strBlank.Substring(0, nRet); // .Trim();

                    string strCmd = StringUtil.GetLeadingCommand(strBlank);
                    if (string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        results.Add(strCmd.Substring(3));
                    }
                }
            }
            results.Sort();
            StringUtil.RemoveDup(ref results, true);
            return results;
        }

#if NO
        /// <summary>
        /// 获得 MARC 记录中的全部编目规则字符串
        /// </summary>
        /// <param name="strMARC">MARC字符串。机内格式</param>
        /// <returns>字符串集合</returns>
        public static List<string> GetExistCatalogingRules(string strMARC)
        {
            int nRet = 0;
            List<string> results = new List<string>();

            for (int i = 1; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                nRet = MarcUtil.GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet != 1)
                    break;

                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;

                {
#if NO
                    string strFieldName = strField.Substring(0, 3);
#endif

                    // 字段名后(字段指示符后)和第一个子字段符号之间的空白片断
                    string strIndicator = "";
                    string strContent = "";
                    if (MarcUtil.IsControlFieldName(strField.Substring(0, 3)) == true)
                    {
                        strContent = strField.Substring(3);
                    }
                    else
                    {
                        if (strField.Length >= 5)
                        {
                            strIndicator = strField.Substring(3, 2);
                            strContent = strField.Substring(3 + 2);
                        }
                        else
                            strIndicator = strField.Substring(3, 1);
                    }

                    string strBlank = strContent;   // .Trim();
                    nRet = strBlank.IndexOf((char)MarcUtil.SUBFLD);
                    if (nRet != -1)
                        strBlank = strBlank.Substring(0, nRet); // .Trim();

                    string strCmd = StringUtil.GetLeadingCommand(strBlank);
                    if (string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        results.Add(strCmd.Substring(3));
                    }

#if NO
                    // 后面还是要继续处理，但strField中去掉了 {...} 一段
                    if (string.IsNullOrEmpty(strCmd) == false)
                    {
                        strContent = strContent.Substring(strCmd.Length + 2);
                        strField = strFieldName + strIndicator + strContent;
                    }
#endif
                }

                // 2012/11/6
                // 观察 $* 子字段
                {
                    //
                    string strSubfield = "";
                    string strNextSubfieldName1 = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = MarcUtil.GetSubfield(strField,
                        ItemType.Field,
                        "*",    // "*",
                        0,
                        out strSubfield,
                        out strNextSubfieldName1);
                    if (nRet == 1)
                    {
                        string strCurRule = strSubfield.Substring(1);
                        if (string.IsNullOrEmpty(strCurRule) == false)
                            results.Add(strCurRule);
                    }
                }

                for (int j = 0; ; j++)
                {
                    string strSubfield = "";
                    string strNextSubfieldName = "";
                    // return:
                    //		-1	error
                    //		0	not found
                    //		1	found
                    nRet = MarcUtil.GetSubfield(strField,
                        ItemType.Field,
                        null,
                        j,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (nRet != 1)
                        break;
                    if (strSubfield.Length <= 1)
                        continue;

                    string strSubfieldName = strSubfield.Substring(0, 1);
                    string strContent = strSubfield.Substring(1);

                    if (strSubfieldName == "*")
                        results.Add(strContent);
                    else
                    {
                        string strCmd = StringUtil.GetLeadingCommand(strContent);

                        if (string.IsNullOrEmpty(strCmd) == false
                            && StringUtil.HasHead(strCmd, "cr:") == true)
                            results.Add(strCmd.Substring(3));
                    }
                }
            }
            results.Sort();
            StringUtil.RemoveDup(ref results);
            return results;
        }

#endif

        #region 刷新列表

        delegate void Delegate_filterValue(Control control);

        // 不安全版本
        // 过滤掉 {} 包围的部分
        static void __FilterValue(Control control)
        {
            string strText = StringUtil.GetPureSelectedValue(control.Text);
            if (control.Text != strText)
                control.Text = strText;
        }

#if NO
        // 安全版本
        public static void FilterValue(Control owner, 
            Control control)
        {
            if (owner.InvokeRequired == true)
            {
                Delegate_filterValue d = new Delegate_filterValue(__FilterValue);
                owner.BeginInvoke(d, new object[] { control });
            }
            else
            {
                __FilterValue((Control)control);
            }
        }
#endif
        // 安全版本
        /// <summary>
        /// 过滤控件中的文本值
        /// </summary>
        /// <param name="owner">控件的宿主控件</param>
        /// <param name="control">控件</param>
        public static void FilterValue(Control owner,
            Control control)
        {
            Delegate_filterValue d = new Delegate_filterValue(__FilterValue);

            if (owner.Created == false)
                __FilterValue((Control)control);
            else
                owner.BeginInvoke(d, new object[] { control });
        }

        // 不安全版本
        // 过滤掉 {} 包围的部分
        // 还有列表值去重的功能
        static void __FilterValueList(Control control)
        {
            List<string> results = StringUtil.FromListString(StringUtil.GetPureSelectedValue(control.Text));
            StringUtil.RemoveDupNoSort(ref results);
            string strText = StringUtil.MakePathList(results);
            if (control.Text != strText)
                control.Text = strText;
        }

#if NO
        // 安全版本
        public static void FilterValueList(Control owner, Control control)
        {
            if (owner.InvokeRequired == true)
            {
                Delegate_filterValue d = new Delegate_filterValue(__FilterValueList);
                owner.BeginInvoke(d, new object[] { control });
            }
            else
            {
                __FilterValueList((Control)control);
            }
        }
#endif
        // 安全版本
        /// <summary>
        /// 过滤控件中的列表值
        /// </summary>
        /// <param name="owner">控件的宿主控件</param>
        /// <param name="control">控件</param>
        public static void FilterValueList(Control owner, Control control)
        {

            Delegate_filterValue d = new Delegate_filterValue(__FilterValueList);

            if (owner.Created == false)
                __FilterValueList((Control)control);
            else
                owner.BeginInvoke(d, new object[] { control });

        }

        #endregion

        // 
        // return:
        //      -1  出错。包括出错后重试然后放弃
        //      0   成功
        /// <summary>
        /// 删除一个目录
        /// </summary>
        /// <param name="owner">本函数中 MessageBox 要用到的窗口</param>
        /// <param name="strDataDir">要删除的目录</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错。包括放弃操作; 0: 成功</returns>
        public static int DeleteDataDir(
            IWin32Window owner,
            string strDataDir,
            out string strError)
        {
            strError = "";
        REDO_DELETE_DATADIR:
            try
            {
                Directory.Delete(strDataDir, true);
                return 0;
            }
            catch (Exception ex)
            {
                strError = "删除目录 '" + strDataDir + "' 时出错: " + ex.Message;
            }

            if (owner != null)
            {
                DialogResult temp_result = MessageBox.Show(owner,
        strError + "\r\n\r\n是否重试?",
        "删除目录 '" + strDataDir + "'",
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (temp_result == DialogResult.Retry)
                    goto REDO_DELETE_DATADIR;
            }

            return -1;
        }

        // 
        /// <summary>
        /// 构造馆代码字符串集合。空字符串也包括在内
        /// </summary>
        /// <param name="strList">列表字符串。逗号分隔的多个馆代码</param>
        /// <returns>字符串集合</returns>
        public static List<string> FromLibraryCodeList(string strList)
        {
            List<string> results = new List<string>();
            string[] parts = strList.Split(new char[] { ',' });
            foreach (string s in parts)
            {
                string strText = s.Trim();
                results.Add(strText);
            }

            return results;
        }
#if NO
        // 获得一个馆藏分配字符串里面的所有馆代码
        public static int GetDistributeLibraryCodes(string strDistribute,
            out List<string> library_codes,
            out string strError)
        {
            strError = "";
            library_codes = new List<string>();

            LocationColletion locations = new LocationColletion();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            foreach (Location location in locations)
            {
                if (string.IsNullOrEmpty(location.Name) == true)
                    continue;



                string[] parts = location.RefID.Split(new char[] { '|' });
                foreach (string text in parts)
                {
                    string strRefID = text.Trim();
                    if (string.IsNullOrEmpty(strRefID) == true)
                        continue;
                    library_codes.Add(strRefID);
                }
            }

            return 0;
        }
#endif

        // 
        /// <summary>
        /// 获得一个馆藏分配字符串里面的所有 参考 ID
        /// </summary>
        /// <param name="strDistribute">馆藏分配字符串</param>
        /// <param name="refids">返回参考 ID 字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int GetRefIDs(string strDistribute,
            out List<string> refids,
            out string strError)
        {
            strError = "";
            refids = new List<string>();

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute,
                out strError);
            if (nRet == -1)
                return -1;

            foreach (Location location in locations)
            {
                if (string.IsNullOrEmpty(location.RefID) == true)
                    continue;

                string[] parts = location.RefID.Split(new char[] { '|' });
                foreach (string text in parts)
                {
                    string strRefID = text.Trim();
                    if (string.IsNullOrEmpty(strRefID) == true)
                        continue;
                    refids.Add(strRefID);
                }
            }

            return 0;
        }

        /// <summary>
        /// 根据一个馆代码列表字符串，判断这个字符串是否代表了全局用户
        /// </summary>
        /// <param name="strLibraryCodeList">馆代码列表字符串</param>
        /// <returns>是否</returns>
        public static bool IsGlobalUser(string strLibraryCodeList)
        {
            if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                return true;
            return false;
        }

        // 
        // return:
        //      -1  出错
        //      0   超过管辖范围。strError中有解释
        //      1   在管辖范围内
        /// <summary>
        /// 观察一个馆藏分配字符串，看看是否完全在当前用户管辖范围内
        /// </summary>
        /// <param name="strDistribute">馆藏分配字符串</param>
        /// <param name="strLibraryCodeList">当前用户的馆代码列表字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 超过管辖范围，strError 中有解释; 1: 在管辖范围内</returns>
        public static int DistributeInControlled(string strDistribute,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (IsGlobalUser(strLibraryCodeList) == true)
                return 1;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute, out strError);
            if (nRet == -1)
            {
                strError = "馆藏分配字符串 '" + strDistribute + "' 格式不正确";
                return -1;
            }

            foreach (Location location in locations)
            {
                // 空的馆藏地点被视为不在分馆用户管辖范围内
                if (string.IsNullOrEmpty(location.Name) == true)
                {
                    strError = "馆代码 '' 不在范围 '" + strLibraryCodeList + "' 内";
                    return 0;
                }

                string strLibraryCode = "";
                string strPureName = "";

                // 解析
                ParseCalendarName(location.Name,
            out strLibraryCode,
            out strPureName);

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    strError = "馆代码 '" + strLibraryCode + "' 不在范围 '" + strLibraryCodeList + "' 内";
                    return 0;
                }
            }

            return 1;
        }

        // 
        // return:
        //      -1  出错
        //      0   没有任何部分在管辖范围
        //      1   至少部分在管辖范围内
        /// <summary>
        /// 观察一个馆藏分配字符串，看看是否部分在当前用户管辖范围内
        /// </summary>
        /// <param name="strDistribute">馆藏分配字符串</param>
        /// <param name="strLibraryCodeList">当前用户的馆代码列表字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有任何部分在管辖范围; 1: 至少部分在管辖范围内</returns>
        public static int DistributeCross(string strDistribute,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (IsGlobalUser(strLibraryCodeList) == true)
                return 1;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute, out strError);
            if (nRet == -1)
            {
                strError = "馆藏分配字符串 '" + strDistribute + "' 格式不正确";
                return -1;
            }

            foreach (Location location in locations)
            {
                // 空的馆藏地点被视为不在分馆用户管辖范围内
                if (string.IsNullOrEmpty(location.Name) == true)
                    continue;

                string strLibraryCode = "";
                string strPureName = "";

                // 解析
                ParseCalendarName(location.Name,
            out strLibraryCode,
            out strPureName);

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == true)
                    return 1;
            }

            return 0;
        }

        // 兼容以前的脚本。新的脚本尽量要使用StringUtil.GetPureLocation()
        /// <summary>
        /// 获得纯净的馆藏地点字符串。去掉了其中的 #reservation 部分
        /// 半方法为了兼容以前的脚本。新的脚本尽量要使用StringUtil.GetPureLocation()
        /// </summary>
        /// <param name="strLocation">待加工的馆藏地点字符串</param>
        /// <returns>加工后的馆藏地点字符串</returns>
        public static string GetPureLocation(string strLocation)
        {
            return StringUtil.GetPureLocation(strLocation);
        }

        /// <summary>
        /// 获得馆代码列表中的第一个馆代码
        /// </summary>
        /// <param name="strLibraryCodeList">管代码列表。逗号分隔的字符串列表</param>
        /// <returns>第一个馆代码</returns>
        public static string GetFirstLibraryCode(string strLibraryCodeList)
        {
            if (string.IsNullOrEmpty(strLibraryCodeList) == true)
                return "";

            List<string> librarycodes = StringUtil.SplitList(strLibraryCodeList);

            if (librarycodes.Count > 0)
                return librarycodes[0];

            return "";
        }

        /// <summary>
        /// 从一个馆藏地点字符串中解析出馆代码部分。例如 "海淀分馆/阅览室" 解析出 "海淀分馆"
        /// </summary>
        /// <param name="strLocationString">馆藏地点字符串</param>
        /// <returns>返回馆代码</returns>
        public static string GetLibraryCode(string strLocationString)
        {

            // 解析
            ParseCalendarName(strLocationString,
        out string strLibraryCode,
        out string strPureName);

            return strLibraryCode;
        }

        // 2016/5/5
        public static string GetLocationRoom(string strLocationString)
        {
            string strLibraryCode = "";
            string strPureName = "";

            // 解析
            ParseCalendarName(strLocationString,
        out strLibraryCode,
        out strPureName);

            return strPureName;
        }

        /// <summary>
        /// 合成日历名
        /// </summary>
        /// <param name="strLibraryCode">馆代码</param>
        /// <param name="strPureName">单纯的日历名</param>
        /// <returns></returns>
        public static string BuildCalendarName(string strLibraryCode,
            string strPureName)
        {
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                return strPureName;
            else
                return strLibraryCode + "/" + strPureName;
        }

        // 
        /// <summary>
        /// 解析日历名。例如 "海淀分馆/基本日历"
        /// </summary>
        /// <param name="strName">完整的日历名</param>
        /// <param name="strLibraryCode">返回馆代码部分</param>
        /// <param name="strPureName">返回纯粹日历名部分</param>
        public static void ParseCalendarName(string strName,
            out string strLibraryCode,
            out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }

        /// <summary>
        /// 过滤出符合馆代码列表的那些馆藏地字符串
        /// </summary>
        /// <param name="strLibraryCodeList">馆代码列表字符串</param>
        /// <param name="values">要进行过滤得字符串集合</param>
        /// <returns>过滤后得到的字符串集合</returns>
        public static List<string> FilterLocationsWithLibraryCodeList(string strLibraryCodeList,
    List<string> values)
        {
            List<string> results = new List<string>();
            foreach (string v in values)
            {
                string strLibraryCode = "";
                string strPureValue = "";

                // 解析一个馆藏地点字符串
                // 海淀分馆/教师
                ParseCalendarName(v,
                    out strLibraryCode,
                    out strPureValue);

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == true)
                    results.Add(v);
            }

            return results;
        }

#if NO
        // 过滤出符合馆代码列表的那些值字符串
        public static List<string> FilterValuesWithLibraryCodeList(string strLibraryCodeList,
    List<string> values)
        {
            List<string> results = new List<string>();
            foreach (string v in values)
            {
                string strCode = "";
                string strPureValue = "";

                // 解析一个来自dp2library的列表值字符串
                // {海淀分馆} 教师
                ParseValueString(v,
                    out strCode,
                    out strPureValue);

                if (StringUtil.IsInList(strCode, strLibraryCodeList) == true)
                    results.Add(v);
            }

            return results;
        }
#endif

        // 
        /// <summary>
        /// 过滤出符合馆代码的那些值字符串
        /// 值字符串的格式为：{海淀分馆} 教师
        /// </summary>
        /// <param name="strLibraryCode">馆代码列表字符串</param>
        /// <param name="values">要进行过滤的值字符串集合</param>
        /// <returns>过滤后得到的值字符串集合</returns>
        public static List<string> FilterValuesWithLibraryCode(string strLibraryCode,
            List<string> values)
        {
            List<string> results = new List<string>();
            foreach (string v in values)
            {
                string strCode = "";
                string strPureValue = "";

                // 解析一个来自dp2library的列表值字符串
                // {海淀分馆} 教师
                ParseValueString(v,
                    out strCode,
                    out strPureValue);

                if (strCode == strLibraryCode)
                    results.Add(v);
            }

            return results;
        }

        // 解析一个来自dp2library的列表值字符串
        // {海淀分馆} 教师
        /// <summary>
        /// 解析一个值字符串
        /// 值字符串的格式：{海淀分馆} 教师
        /// </summary>
        /// <param name="strText">待解析的字符串</param>
        /// <param name="strLibraryCode">返回馆代码部分</param>
        /// <param name="strPureValue">返回纯粹的值部分</param>
        public static void ParseValueString(string strText,
            out string strLibraryCode,
            out string strPureValue)
        {
            strLibraryCode = "";
            strPureValue = "";

            int nRet = strText.IndexOf("{");
            if (nRet == -1)
            {
                strPureValue = strText;
                return;
            }
            int nStart = nRet;
            nRet = strText.IndexOf("}", nStart + 1);
            if (nRet == -1)
            {
                strPureValue = strText;
                return;
            }
            int nEnd = nRet;

            strLibraryCode = strText.Substring(nStart + 1, nEnd - nStart - 1).Trim();
            strPureValue = strText.Remove(nStart, nEnd - nStart + 1).Trim();
        }


        // 
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 获得 Encoding 对象。本函数不支持MARC-8编码名
        /// </summary>
        /// <param name="strName">编码名称。可以是代码页数字形式</param>
        /// <param name="encoding">Encoding 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int GetEncoding(string strName,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            encoding = null;

            try
            {

                if (StringUtil.IsNumber(strName) == true)
                {
                    try
                    {
                        Int32 nCodePage = Convert.ToInt32(strName);
                        encoding = Encoding.GetEncoding(nCodePage);
                    }
                    catch (Exception ex)
                    {
                        strError = "构造编码方式过程出错: " + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    encoding = Encoding.GetEncoding(strName);
                }
            }
            catch (Exception ex)
            {
                strError = "GetEncoding() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

        // 
        /// <summary>
        /// 获得一个编码的信息。
        /// 注意，本函数不能处理扩充的Marc8Encoding类
        /// </summary>
        /// <param name="encoding">Encoding 对象</param>
        /// <returns>EncodingInfo 对象。如果没有找到，则返回 null</returns>
        static EncodingInfo GetEncodingInfo(Encoding encoding)
        {
            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                if (encoding.Equals(infos[i].GetEncoding()))
                    return infos[i];
            }

            return null;    // not found
        }

        // 
        /// <summary>
        /// 获得encoding的正式名字。本函数不能识别Marc8Encoding类
        /// </summary>
        /// <param name="encoding">Encoding 对象</param>
        /// <returns>正式名字</returns>
        public static string GetEncodingName(Encoding encoding)
        {
            EncodingInfo info = GetEncodingInfo(encoding);
            if (info != null)
            {
                return info.Name;
            }
            else
            {
                return "Unknown encoding";
            }
        }

        // 列出encoding名列表
        // 需要把gb2312 utf-8等常用的提前
        /// <summary>
        /// 获得全部可用的编码名列表
        /// </summary>
        /// <param name="bHasMarc8">是否包括 MARC-8</param>
        /// <returns>字符串集合</returns>
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

        #region 号码相关函数

        // 号码之间是否为增量的关系
        static bool IsNextNo(string strPrevNo,
            string strNextNo)
        {
            int nPrevNo = 0;
            try
            {
                nPrevNo = Convert.ToInt32(strPrevNo);
            }
            catch
            {
                return false;
            }
            int nNextNo = 0;
            try
            {
                nNextNo = Convert.ToInt32(strNextNo);
            }
            catch
            {
                return false;
            }

            if (nPrevNo + 1 == nNextNo)
                return true;

            return false;
        }

        // return:
        //      -1  相等
        //      0   未定
        //      1   增量
        static int GetNumberListStyle(List<string> strings)
        {
            if (strings.Count <= 1)
                return 0;
            Debug.Assert(strings.Count >= 2, "");
            if (strings[0] != strings[1])
                return 1;
            return -1;
        }

        static string OutputNumberList(List<string> strings)
        {
            Debug.Assert(strings != null, "");
            Debug.Assert(strings.Count >= 1, "");
            if (strings.Count == 1)
            {
                string strHead = strings[0];
                if (String.IsNullOrEmpty(strHead) == true)
                    strHead = "(空)";
                return strHead;
            }
            if (strings.Count >= 2)
            {
                string strHead = strings[0];
                string strTail = strings[strings.Count - 1];
                if (strHead == strTail)
                {
                    if (String.IsNullOrEmpty(strHead) == true)
                        strHead = "(空)";
                    return strHead + "*" + strings.Count.ToString();
                }

                return strHead + "-" + strTail;
            }
            Debug.Assert(false, "");
            return null;
        }

        // 是否为全部空字符串的数组?
        static bool IsNullList(List<string> parts)
        {
            if (parts.Count == 0)
                return true;
            for (int i = 0; i < parts.Count; i++)
            {
                if (String.IsNullOrEmpty(parts[i]) == false)
                    return false;
            }

            return true;
        }

        // 
        /// <summary>
        /// 将独立的序号字符串组合为一个整个的范围字符串
        /// </summary>
        /// <param name="parts">徐浩字符串集合</param>
        /// <returns>组合后的字符串</returns>
        public static string BuildNumberRangeString(List<string> parts)
        {
            if (IsNullList(parts) == true)
                return "";

            string strResult = "";

            List<string> temp_list = new List<string>();

            string strNo = null;
            for (int i = 0; i < parts.Count; i++)
            {
                strNo = parts[i];

                if (temp_list.Count == 0)
                {
                    // 推入当前
                    temp_list.Add(strNo);
                    continue;
                }

                Debug.Assert(temp_list.Count > 0, "");

                // return:
                //      -1  相等
                //      0   未定
                //      1   增量
                int nNumberStyle = GetNumberListStyle(temp_list);

                string strPrevNo = temp_list[temp_list.Count - 1];

                if (nNumberStyle == 1)
                {
                    if (IsNextNo(strPrevNo, strNo) == false)
                    {
                        // 输出
                        goto OUTPUT;
                    }
                    // 推入当前
                    temp_list.Add(strNo);
                    continue;
                }

                if (nNumberStyle == -1)
                {
                    if (strPrevNo != strNo)
                    {
                        // 输出
                        goto OUTPUT;
                    }
                    // 推入当前
                    temp_list.Add(strNo);
                    continue;
                }

                Debug.Assert(nNumberStyle == 0, "");

                if (IsNextNo(strPrevNo, strNo) == true
                    || strPrevNo == strNo)
                {
                    // 推入当前
                    temp_list.Add(strNo);
                    continue;
                }
                else
                {
                    goto OUTPUT;
                }

            OUTPUT:
                // 输出，然后推入当前
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ",";
                strResult += OutputNumberList(temp_list);
                temp_list.Clear();
                temp_list.Add(strNo);
            }

            if (temp_list.Count > 0)
            {
                // 输出
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ",";
                strResult += OutputNumberList(temp_list);
            }

            return strResult;
        }

        #endregion
        /*
        // 一个字符串的头部?
        public static bool HasHead(string strText,
            string strHead)
        {
            if (strText.Length < strHead.Length)
                return false;
            string strPart = strText.Substring(0, strHead.Length);
            if (strPart == strHead)
                return true;
            return false;
        }
         * */

        /// <summary>
        /// 强制 GC 进行垃圾回收
        /// </summary>
        public static void ForceGarbageCollection()
        {
            //Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            // Without this call to GC.WaitForPendingFinalizers, 
            // the worker loop below might execute at the same time 
            // as the finalizers.
            // With this call, the worker loop executes only after
            // all finalizers have been called.
            GC.WaitForPendingFinalizers();
        }

        // 
        /// <summary>
        /// 看看状态字符串是否包含了“加工中”
        /// </summary>
        /// <param name="strState">状态字符串</param>
        /// <returns>是否包含了“加工中”部分</returns>
        public static bool IncludeStateProcessing(string strState)
        {
            if (StringUtil.IsInList("加工中", strState) == true)
                return true;
            return false;
        }

        // 
        /// <summary>
        /// 为状态字符串增加(子)值“加工中”
        /// </summary>
        /// <param name="strState">待处理的字符串</param>
        /// <returns>返回处理后的字符串</returns>
        public static string AddStateProcessing(string strState)
        {
            string strResult = strState;
            StringUtil.SetInList(ref strResult, "加工中", true);
            return strResult;
        }

        // 
        /// <summary>
        /// 为状态字符串去除(子)值“加工中”
        /// </summary>
        /// <param name="strState">待处理的字符串</param>
        /// <returns>返回处理后的字符串</returns>
        public static string RemoveStateProcessing(string strState)
        {
            string strResult = strState;
            StringUtil.SetInList(ref strResult, "加工中", false);
            return strResult;
        }

        // 构造时间范围字符串
        // 返回的时间范围字符串格式：形态为 “19980101-19991231”
        /// <summary>
        /// 构造时间范围字符串
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>返回的时间范围字符串格式：形态为 “19980101-19991231”</returns>
        public static string MakeTimeRangeString(DateTime start,
            DateTime end)
        {
            string strStart = "";
            if (start != new DateTime(0))
                strStart = DateTimeUtil.DateTimeToString8(start);
            string strEnd = "";
            if (end != new DateTime(0))
                strEnd = DateTimeUtil.DateTimeToString8(end);

            return strStart + " - " + strEnd;
        }

        // 解析时间范围字符串
        // 注：如果end == new DateTime(0)表示无限靠后的时间。
        // parameters:
        //      bAdjustEnd  是否调整末尾时间。调整是指加上一天
        //      strText 日期范围字符串。形态为 “19980101-19991231”
        /// <summary>
        /// 解析时间范围字符串
        /// 注：如果end == new DateTime(0)表示无限靠后的时间
        /// </summary>
        /// <param name="strText">时间范围字符串</param>
        /// <param name="bAdjustEnd">是否调整末尾时间。调整是指加上一天</param>
        /// <param name="start">返回开始时间</param>
        /// <param name="end">返回结束时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int ParseTimeRangeString(string strText,
            bool bAdjustEnd,
            out DateTime start,
            out DateTime end,
            out string strError)
        {
            strError = "";
            start = new DateTime((long)0);
            end = new DateTime((long)0);

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                strError = "'" + strText + "' 中缺乏破折号 '-'";
                return -1;
            }

            string strStart = strText.Substring(0, nRet).Trim();
            string strEnd = strText.Substring(nRet + 1).Trim();

            if (String.IsNullOrEmpty(strStart) == true)
                start = new DateTime(0);
            else
            {
                if (strStart.Length != 8)
                {
                    strError = "破折号左边的部分 '" + strStart + "' 不是8字符";
                    return -1;
                }
                start = DateTimeUtil.Long8ToDateTime(strStart);
            }

            if (String.IsNullOrEmpty(strEnd) == true)
                end = new DateTime(0);
            else
            {
                if (strEnd.Length != 8)
                {
                    strError = "破折号右边的部分 '" + strEnd + "' 不是8字符";
                    return -1;
                }
                end = DateTimeUtil.Long8ToDateTime(strEnd);

                if (bAdjustEnd == true)
                {
                    // 修正一天
                    end += new TimeSpan(24, 0, 0);
                }
            }

            return 0;
        }

        // 
        /// <summary>
        /// 看看一个时间范围内包含多少年
        /// </summary>
        /// <param name="strRange">时间范围字符串</param>
        /// <returns>返回数字，表示年数</returns>
        public static float Years(string strRange)
        {
            int nRet = strRange.IndexOf("-");
            if (nRet == -1)
                return 0;
            string strStart = strRange.Substring(0, nRet).Trim();
            string strEnd = strRange.Substring(nRet + 1).Trim();

            if (strStart.Length != 8)
                return 0;
            if (strEnd.Length != 8)
                return 0;

            // 2012/5/9
            // 如果是整年
            if (strStart.Substring(0, 4) == strEnd.Substring(0, 4)
                && strStart.Substring(4, 4) == "0101"
                && strEnd.Substring(4, 4) == "1231")
                return 1;

            DateTime start = DateTimeUtil.Long8ToDateTime(strStart);
            DateTime end = DateTimeUtil.Long8ToDateTime(strEnd);

            // 这里有点小问题，末尾日期应该是下一日的前一秒
            // 7天以后
            end += new TimeSpan(1, 0, 0, 0);
            end -= new TimeSpan(0, 0, 0, 1);

            TimeSpan delta = end - start;

            return ((float)delta.TotalDays / (float)365);
        }

        // 检测一个出版时间是否处在特定的时间范围内?
        // Exception: 有可能抛出异常
        // parameters:
        //      strPublishTime  4/6/8字符
        //      strRange    格式为"20080101-20081231"
        /// <summary>
        /// 检测一个出版时间是否处在特定的时间范围内?
        /// 可能会抛出异常
        /// </summary>
        /// <param name="strPublishTime">出版时间字符串。可以为 4/6/8 字符</param>
        /// <param name="strRange">时间范围字符串。格式为"20080101-20081231"</param>
        /// <returns>是否处在范围内</returns>
        public static bool InRange(string strPublishTime,
            string strRange)
        {
            if (strPublishTime.Length == 4)
                strPublishTime += "0101";
            else if (strPublishTime.Length == 6)
                strPublishTime += "01";

            int nRet = strRange.IndexOf("-");

            string strStart = strRange.Substring(0, nRet).Trim();
            string strEnd = strRange.Substring(nRet + 1).Trim();

            if (strStart.Length != 8)
                throw new Exception("时间范围字符串 '" + strRange + "' 的左边部分 '" + strStart + "' 格式错误，应为8字符");

            if (strEnd.Length != 8)
                throw new Exception("时间范围字符串 '" + strRange + "' 的右边部分 '" + strEnd + "' 格式错误，应为8字符");

            if (String.Compare(strPublishTime, strStart) < 0)
                return false;

            if (String.Compare(strPublishTime, strEnd) > 0)
                return false;

            return true;
        }

        // 从剪贴板中Paste行插入到ListView中当前选定的位置
        // 注：本函数并不删除调用前已经选定的若干行
        // parameters:
        //      bInsertBefore   是否前插? 如果==true前插，否则后插
        /// <summary>
        /// 从剪贴板中Paste行插入到ListView中当前选定的位置
        /// 注：本函数并不删除调用前已经选定的若干行
        /// </summary>
        /// <param name="form">宿主 Form</param>
        /// <param name="list">ListView</param>
        /// <param name="bInsertBefore">是否前插? 如果==true前插，否则后插</param>
        public static void PasteLinesFromClipboard(Form form,
            ListView list,
            bool bInsertBefore)
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
            {
                MessageBox.Show(form, "剪贴板中没有内容");
                return;
            }
            string strWhole = (string)ido.GetData(DataFormats.UnicodeText);

            int index = -1;

            if (list.SelectedIndices.Count > 0)
                index = list.SelectedIndices[0];

            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            list.SelectedItems.Clear();

            int nMaxColumns = 0;
            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            list.BeginUpdate();
            for (int i = 0; i < lines.Length; i++)
            {
                ListViewItem item = Global.BuildListViewItem(
                    list,
                    lines[i],
                    false);
                // 这里单独计算可能速度要快些
                if (item.SubItems.Count > nMaxColumns)
                    nMaxColumns = item.SubItems.Count;

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
            // 确保列标题数目够
            ListViewUtil.EnsureColumns(list, nMaxColumns, 100);

            list.EndUpdate();

            form.Cursor = oldCursor;
        }

        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        /// <summary>
        /// 复制或者剪切 ListView 中选定的事项的某列到 Clipboard
        /// </summary>
        /// <param name="form">宿主 Form</param>
        /// <param name="nColumnIndex">要复制的列的下标</param>
        /// <param name="list">ListView</param>
        /// <param name="bCut">是否为剪切</param>
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
                strLine = strLine.Replace("\r", "\\r").Replace("\n", "\\n");    // 避免内容中的回车换行干扰 paste 进入 Excel 等的行数
                strTotal.Append(strLine + "\r\n");
            }

            // Clipboard.SetDataObject(strTotal.ToString(), true);
            StringUtil.RunClipboard(() =>
            {
                // https://stackoverflow.com/questions/930219/how-to-handle-a-blocked-clipboard-and-other-oddities
                Clipboard.Clear();
                Clipboard.SetDataObject(strTotal.ToString(), true, 5, 200);
            });

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

#if NO
        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        public static void CopyLinesToClipboard(
            Form form,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (ListViewItem item in list.SelectedItems)
            {
                string strLine = Global.BuildLine(item);
                strTotal.Append(strLine + "\r\n");
            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            if (bCut == true)
            {
                list.BeginUpdate();

                foreach (ListViewItem item in list.SelectedItems)
                {
                    // list.Items.Remove(item);
                    item.Remove();
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
#endif

#if NO
        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        public static void CopyLinesToClipboard(
            Form form,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            if (bCut == true)
                list.BeginUpdate();

            foreach (ListViewItem item in list.SelectedItems)
            {
                string strLine = Global.BuildLine(item);
                strTotal.Append(strLine + "\r\n");

                if (bCut == true)
                    item.Remove();
            }

            if (bCut == true)
                list.EndUpdate();

            Clipboard.SetDataObject(strTotal.ToString(), true);

            form.Cursor = oldCursor;
        }
#endif

#if NO
        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        public static void CopyLinesToClipboard(
            Form form,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (ListViewItem item in list.SelectedItems)
            {
                string strLine = Global.BuildLine(item);
                strTotal.Append(strLine + "\r\n");
            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            if (bCut == true)
            {
                ListViewItem [] items = new ListViewItem[list.SelectedItems.Count];
                list.SelectedItems.CopyTo(items, 0);

                list.BeginUpdate();

                foreach (ListViewItem item in items)
                {
                    // list.Items.Remove(item);
                    item.Remove();
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
#endif

        // 复制或者剪切ListView中选定的事项到Clipboard
        // parameters:
        //      bCut    是否为剪切
        /// <summary>
        /// 复制或者剪切 ListView 中选定的事项到 Clipboard
        /// </summary>
        /// <param name="form">宿主 Form</param>
        /// <param name="list">ListView</param>
        /// <param name="bCut">是否为剪切</param>
        public static void CopyLinesToClipboard(
            Form form,
            ListView list,
            bool bCut)
        {
            Cursor oldCursor = form.Cursor;
            form.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (ListViewItem item in list.SelectedItems)
            {
                string strLine = Global.BuildLine(item);
                strTotal.Append(strLine + "\r\n");
            }

            // Clipboard.SetDataObject(strTotal.ToString(), true);
            StringUtil.RunClipboard(() =>
            {
                // https://stackoverflow.com/questions/930219/how-to-handle-a-blocked-clipboard-and-other-oddities
                Clipboard.Clear();
                Clipboard.SetDataObject(strTotal.ToString(), true, 5, 200);
            });

            if (bCut == true)
            {
                int[] indices = new int[list.SelectedItems.Count];
                list.SelectedIndices.CopyTo(indices, 0);

                list.BeginUpdate();

                for (int i = indices.Length - 1; i >= 0; i--)
                {
                    list.Items.RemoveAt(indices[i]);
                }

                list.EndUpdate();
            }

            form.Cursor = oldCursor;
        }

        /*
        // 把一个字符串数组去重。调用前，应当已经排序
        public static void RemoveDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }

        }
         * */

        // 
        /// <summary>
        /// 把 ListViewItem 文本内容构造为 tab 字符分割的字符串
        /// </summary>
        /// <param name="item">ListViewItem</param>
        /// <returns>字符串</returns>
        public static string BuildLine(ListViewItem item)
        {
            StringBuilder strLine = new StringBuilder(4096);
            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (i != 0)
                    strLine.Append("\t");

                string strText = item.SubItems[i].Text.Replace("\r", "\\r").Replace("\n", "\\n");    // 避免内容中的回车换行干扰 paste 进入 Excel 等的行数
                strLine.Append(strText);
            }

            return strLine.ToString();
        }

        // 根据字符串构造ListViewItem。
        // 字符串的格式为\t间隔的
        // parameters:
        //      list    可以为null。如果为null，就没有自动扩展列标题数目的功能
        /// <summary>
        /// 根据字符串构造ListViewItem
        /// 字符串格式为 tab 字符分割的字符串
        /// </summary>
        /// <param name="list">ListView。可以为null。如果为null，就没有自动扩展列标题数目的功能</param>
        /// <param name="strLine">字符串</param>
        /// <param name="AutoExpandColumnCount">是否自动扩展列数</param>
        /// <returns>构造好的 ListViewItem 对象</returns>
        public static ListViewItem BuildListViewItem(
            ListView list,
            string strLine,
            bool AutoExpandColumnCount = true)
        {
            ListViewItem item = new ListViewItem();
            string[] parts = strLine.Split(new char[] { '\t' });
            for (int i = 0; i < parts.Length; i++)
            {
                ListViewUtil.ChangeItemText(item, i, parts[i]);
            }

            // 确保列标题数目够
            if (AutoExpandColumnCount == true)
            {
                if (list != null)
                    ListViewUtil.EnsureColumns(list, parts.Length, 100);
            }

            return item;
        }

        // 记录路径是否为追加型？
        // 所谓追加型，就是记录ID部分为'?'，或者没有记录ID部分
        /// <summary>
        /// 记录路径是否为追加型？
        /// 所谓追加型，就是记录ID部分为'?'，或者没有记录ID部分
        /// </summary>
        /// <param name="strPath">记录路径字符串。例如 "中文图书/120"</param>
        /// <returns>是否为追加型</returns>
        public static bool IsAppendRecPath(string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return true;

            string strRecordID = Global.GetRecordID(strPath);
            if (String.IsNullOrEmpty(strRecordID) == true
                || strRecordID == "?")
                return true;

            return false;
        }

#if NO
        // 是否为新增记录的路径
        /// <summary>
        /// 记录路径是否为追加型。建议废止此函数
        /// </summary>
        /// <param name="strPath">记录路径字符串。例如 "中文图书/120"</param>
        /// <returns>是否为追加型</returns>
        public static bool IsNewPath(string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return true;    //???? 空路径当作新路径?

            string strID = Global.GetRecordID(strPath);

            if (strID == "?"
                || String.IsNullOrEmpty(strID) == true) // 2008/11/28 
                return true;

            return false;
        }
#endif

        /// <summary>
        /// 从文本文件中读入内容
        /// </summary>
        /// <param name="strFilePath">文件全路径</param>
        /// <param name="strContent">返回内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int ReadTextFileContent(string strFilePath,
    out string strContent,
    out string strError)
        {
            return ReadTextFileContent(strFilePath,
                -1,
                out strContent,
                out strError);
        }

        /// <summary>
        /// 从文本文件中读入内容
        /// </summary>
        /// <param name="strFilePath">文件全路径</param>
        /// <param name="lMaxLength">限定最大字符数。-1 为不限制</param>
        /// <param name="strContent">返回内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int ReadTextFileContent(string strFilePath,
    long lMaxLength,
    out string strContent,
    out string strError)
        {
            Encoding encoding = null;
            return FileUtil.ReadTextFileContent(strFilePath,
                lMaxLength,
                out strContent,
                out encoding,
                out strError);
        }

        // 获得批次号表
        // parameters:
        //      strPubType  出版物类型。为 图书/连续出版物/(空) 之一
        internal static void GetBatchNoTable(GetKeyCountListEventArgs e,
            IWin32Window owner,
            string strPubType,  // 出版物类型
            string strType,
            Stop stop,
            LibraryChannel channel)
        {
            string strError = "";
            long lRet = 0;


            if (e.KeyCounts == null)
                e.KeyCounts = new List<KeyCount>();

            string strName = "";
            if (strType == "order")
                strName = "订购";
            else if (strType == "item")
                strName = "册";
            else if (strType == "biblio")
                strName = "编目";
            else
                throw new Exception("未知的strType '" + strType + "' 值");

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);

            // EnableControls(false);
            stop.OnStop += new StopEventHandler(channel.DoStop);
            stop.Initial("正在列出全部" + strName + "批次号 ...");
            stop.BeginLoop();

            try
            {
                int nPerMax = 2000; // 一次检索命中的最大条数限制
                string strLang = "zh";

                string strDbName = "<all>";
                if (strPubType == "图书")
                    strDbName = "<all book>";
                else if (strPubType == "连续出版物")
                    strDbName = "<all series>";
                else
                    strDbName = "<all>";

                if (strType == "order")
                {
                    lRet = channel.SearchOrder(
                        stop,
                        strDbName,  // "<all>",
                        "", // strBatchNo
                        nPerMax,   // -1,
                        "批次号",
                        "left",
                        strLang,
                        "batchno",   // strResultSetName
                        "desc",
                        "keycount", // strOutputStyle
                        out strError);
                }
                else if (strType == "biblio")
                {
                    string strQueryXml = "";

                    lRet = channel.SearchBiblio(
                        stop,
                        strDbName,  // "<all>",    // 尽管可以用 this.comboBox_inputBiblioDbName.Text, 以便获得和少数书目库相关的批次号实例，但是容易造成误会：因为数据库名列表刷新后，这里却不会刷新？
                        "", // strBatchNo,
                        nPerMax,   // -1,    // nPerMax
                        "batchno",
                        "left",
                        strLang,
                        "batchno",   // strResultSetName
                        "desc",
                        "keycount", // strOutputStyle
                        "",
                        out strQueryXml,
                        out strError);
                }
                else if (strType == "item")
                {

                    lRet = channel.SearchItem(
                        stop,
                        strDbName,   // "<all>",
                        "", // strBatchNo
                        nPerMax,  // -1,
                        "批次号",
                        "left",
                        strLang,
                        "batchno",   // strResultSetName
                        "desc",
                        "keycount", // strOutputStyle
                        out strError);
                }
                else
                {
                    Debug.Assert(false, "");
                }


                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                {
                    strError = "没有找到任何" + strName + "批次号检索点";
                    return;
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    lRet = channel.GetSearchResult(
                        stop,
                        "batchno",   // strResultSetName
                        lStart,
                        lCount,
                        "keycount",
                        strLang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error: " + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        // MessageBox.Show(this, "未命中");
                        return;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        if (searchresults[i].Cols == null)
                        {
                            strError = "请更新应用服务器和数据库内核到最新版本，才能使用列出" + strName + "批次号的功能";
                            goto ERROR1;
                        }

                        KeyCount keycount = new KeyCount();
                        keycount.Key = searchresults[i].Path;
                        keycount.Count = Convert.ToInt32(searchresults[i].Cols[0]);
                        e.KeyCounts.Add(keycount);
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(channel.DoStop);
                stop.Initial("");

                // EnableControls(true);

                channel.Timeout = old_timeout;
            }
            return;
        ERROR1:
            MessageBox.Show(owner, strError);
        }

        /// <summary>
        /// 把一个眼色按照比例变暗
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="percent">比例</param>
        /// <returns>返回变化后的颜色</returns>
        public static Color Dark(Color color, float percent)
        {
            int r = color.R - (int)((float)color.R * percent);
            int g = color.G - (int)((float)color.G * percent);
            int b = color.B - (int)((float)color.B * percent);

            if (r < 0)
                r = 0;
            if (g < 0)
                g = 0;
            if (b < 0)
                b = 0;

            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// 把一个颜色按照比例变亮
        /// </summary>
        /// <param name="color">原始颜色</param>
        /// <param name="percent">比例</param>
        /// <returns>返回变化后的颜色</returns>
        public static Color Light(Color color, float percent)
        {
            int r = color.R + (int)((float)color.R * percent);
            int g = color.G + (int)((float)color.G * percent);
            int b = color.B + (int)((float)color.B * percent);

            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// 递归 Invalidate 一个 Control 和它的全部子 Control
        /// </summary>
        /// <param name="control">Control 对象</param>
        public static void InvalidateAllControls(Control control)
        {
            control.Invalidate();
            for (int i = 0; i < control.Controls.Count; i++)
            {
                InvalidateAllControls(control.Controls[i]);    // 递归
            }
        }

        // 检查数据库名
        // return:
        //      -1  有错
        //      0   无错
        internal static int CheckDbName(string strDbName,
            out string strError)
        {
            strError = "";
            if (strDbName.IndexOf("#") != -1)
            {
                strError = "数据库名 '" + strDbName + "' 格式错误。不能有#号";
                return -1;
            }

            return 0;
        }

        // 在listviewcontrol最前面插入一行
        /// <summary>
        /// 在 ListViewControl1 最前面插入一行
        /// </summary>
        /// <param name="list">ListViewControl1 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <param name="insert_pos">插入位置</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem InsertNewLine(
            ListViewControl1 list,
            string strID,
            string[] others,
            int insert_pos = 0)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Insert(insert_pos, item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            list.UpdateItem(insert_pos);
            return item;
        }

        // 在listviewcontrol最后追加一行
        /// <summary>
        /// 在 ListViewControl1 最后追加一行
        /// </summary>
        /// <param name="list">ListViewControl1 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem AppendNewLine(
            ListViewControl1 list,
            string strID,
            string[] others)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Add(item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            list.UpdateItem(list.Items.Count - 1);

            return item;
        }


        // 在listview最前面插入一行
        /// <summary>
        /// 在 ListView 最前面插入一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <param name="insert_pos"></param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem InsertNewLine(
            ListView list,
            string strID,
            string[] others,
            int insert_pos = 0)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Insert(insert_pos, item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            return item;
        }


        // 在listview最后追加一行
        /// <summary>
        /// 在 ListView 最后追加一行
        /// </summary>
        /// <param name="list">ListView 对象</param>
        /// <param name="strID">左边第一列内容</param>
        /// <param name="others">其余列内容</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public static ListViewItem AppendNewLine(
            ListView list,
            string strID,
            string[] others)
        {
            if (others != null)
                ListViewUtil.EnsureColumns(list, others.Length + 1);

            ListViewItem item = new ListViewItem(strID, 0);

            list.Items.Add(item);

            if (others != null)
            {
                for (int i = 0; i < others.Length; i++)
                {
                    item.SubItems.Add(others[i]);
                }
            }

            return item;
        }

        /*
        // 确保列标题数量足够
        public static void EnsureColumns(ListView list,
            int nCount)
        {
            if (list.Columns.Count >= nCount)
                return;

            for (int i = list.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                if (i == 0)
                {
                    strText = "记录路径";
                }
                else
                {
                    strText = Convert.ToString(i);
                }

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = 200;
                list.Columns.Add(col);
            }

        }
         * */

        // 兼容以前版本
        public static string GetReaderSummary(string strReaderXml)
        {
            return GetReaderSummary(strReaderXml, out string _);
        }

        // 
        /// <summary>
        /// 获得读者摘要信息
        /// </summary>
        /// <param name="strReaderXml">读者记录 XML</param>
        /// <param name="strReaderBarcode">返回读者证条码号</param>
        /// <returns>读者摘要</returns>
        public static string GetReaderSummary(string strReaderXml,
            out string strReaderBarcode)
        {
            strReaderBarcode = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                return "读者记录XML装入DOM时出错: " + ex.Message;
            }

            // 2021/9/1
            strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            if (string.IsNullOrEmpty(strReaderBarcode))
            {
                var refID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(refID) == false)
                    strReaderBarcode = $"@refID:{refID}";
            }
            return DomUtil.GetElementText(dom.DocumentElement,
                "name");
        }

        /// <summary>
        /// 对浏览器控件设置 HTML 字符串
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="strTempFileType">临时文件类型。用于构造临时文件名</param>
        public static void SetHtmlString(WebBrowser webBrowser,
            string strHtml,
            string strDataDir,
            string strTempFileType)
        {
            StopWebBrowser(webBrowser);

            // 2021/2/4
            if (strHtml == null)
                strHtml = "";

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", PathUtil.MergePath(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        // 有问题，不要用
        internal static void SetXmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", PathUtil.MergePath(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".xml");

            // TODO: 要能适应"<root ... />"这样的没有prolog的XML内容
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        // 把 XML 字符串装入一个Web浏览器控件
        // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
        /// <summary>
        /// 把 XML 字符串装入一个Web浏览器控件
        /// 本方法能够适应"&lt;root ... /&gt;"这样的没有 prolog 的 XML 内容
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="strTempFileType">临时文件类型。用于构造临时文件名</param>
        /// <param name="strXml">XML 字符串</param>
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
                strTargetFileName = Path.Combine(strDataDir, "xml.txt");
                using (StreamWriter sw = new StreamWriter(strTargetFileName,
    false,	// append
    System.Text.Encoding.UTF8))
                {
                    sw.Write("XML内容装入DOM时出错: " + ex.Message + "\r\n\r\n" + strXml);
                }
                // webBrowser.Navigate(strTargetFileName);
                Navigate(webBrowser, strTargetFileName);  // 2015/7/28
                return;
            }

            dom.Save(strTargetFileName);
            // webBrowser.Navigate(strTargetFileName);
            Navigate(webBrowser, strTargetFileName);  // 2015/7/28
        }

        internal static void PrepareStop(WebBrowser webBrowser)
        {
            webBrowser.Tag = new WebBrowserInfo();
        }

        internal static bool StopWebBrowser(WebBrowser webBrowser)
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

        // 包装后的版本
        public static void ClearHtmlPage(WebBrowser webBrowser,
    string strDataDir)
        {
            ClearHtmlPage(webBrowser, strDataDir, SystemColors.Window);
        }

        /// <summary>
        /// 清空浏览器控件内容
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strDataDir">数据目录。本函数将在其中创建一个临时文件</param>
        /// <param name="backColor">背景色</param>
        public static void ClearHtmlPage(WebBrowser webBrowser,
            string strDataDir,
            Color backColor)
        {
            StopWebBrowser(webBrowser);

            if (String.IsNullOrEmpty(strDataDir) == true)
            {
                webBrowser.DocumentText = "(空)";
                return;
            }
            string strImageUrl = PathUtil.MergePath(strDataDir, "page_blank_128.png");
            string strHtml = "<html><body style='background-color:" + ColorUtil.Color2String(backColor) + ";'><img src='" + strImageUrl + "' width='64' height='64' alt='空'></body></html>";
            webBrowser.DocumentText = strHtml;
        }

        /// <summary>
        /// 对浏览器控件设置 HTML 字符串
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml)
        {
            /*
            // 警告 这样调用，不会自动<body onload='...'>事件
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "doc不应该为null");
            }

            doc = doc.OpenNew(true);
            doc.Write(strHtml);
             * */

            webBrowser.DocumentText = strHtml;
        }

        // 不支持异步调用
        /// <summary>
        /// 向一个浏览器控件中追加写入 HTML 字符串
        /// 不支持异步调用
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <param name="strHtml">HTML 字符串</param>
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");  // 2015/7/28

                doc = webBrowser.Document;
#if NO
                webBrowser.DocumentText = "<h1>hello</h1>";
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "");
#endif
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 用 WebBrowserExtension 中的ScrollToEnd() 替代
        /// <summary>
        /// 把浏览器控件内容卷滚到尾部
        /// </summary>
        /// <param name="webBrowser">浏览器控件对象</param>
        public static void ScrollToEnd(WebBrowser webBrowser)
        {
#if NO
            /*
            API.SendMessage(window.Handle,
                API.WM_VSCROLL,
                API.SB_BOTTOM,  // (int)API.MakeLParam(API.SB_BOTTOM, 0),
                0);
             * */
            HtmlDocument doc = webBrowser.Document;
            doc.Window.ScrollTo(0, 0x7fffffff);

            /*
            webBrowser.Invalidate();
            webBrowser.Update();
             * */
#endif
            webBrowser.ScrollToEnd();   // 2016/4/22
        }

#if NO
        public static void ScrollToEnd(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;
            doc.Body.ScrollIntoView(false);
        }
#endif

        // 
        /// <summary>
        /// 检测字符串是否为纯数字(不包含'-','.'号)
        /// </summary>
        /// <param name="s">字符串</param>
        /// <returns>是否为纯数字</returns>
        public static bool IsPureNumber(string s)
        {
            if (s == null)
                return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > '9' || s[i] < '0')
                    return false;
            }
            return true;
        }

        static string source_chars = "０１２３４５６７８９．。ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
        static string target_chars = "0123456789..abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 把字符串里面的全角字符转换为对应的半角字符
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <returns>处理后的字符串</returns>
        public static string ConvertQuanjiaoToBanjiao(string strText)
        {
            Debug.Assert(source_chars.Length == target_chars.Length, "");
            string strTarget = "";
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                int nRet = source_chars.IndexOf(ch);
                if (nRet != -1)
                    ch = target_chars[nRet];

                strTarget += ch;
            }

            return strTarget;
        }

        // 
        /// <summary>
        /// 检测一个字符串是否包含了全角字符
        /// </summary>
        /// <param name="strText">要检测的字符串</param>
        /// <returns>是否包含了全角字符</returns>
        public static bool HasQuanjiaoChars(string strText)
        {
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                int nRet = source_chars.IndexOf(ch);
                if (nRet != -1)
                    return true;
            }

            return false;
        }

        // 
        /// <summary>
        /// 将浏览器控件中已有的内容清除，并为后面输出的纯文本显示做好准备
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        public static void ClearForPureTextOutputing(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");  // 2015/7/28
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<pre>");
        }

        /// <summary>
        /// 删除若干文件
        /// </summary>
        /// <param name="filenames">文件名集合</param>
        public static void DeleteFiles(List<string> filenames)
        {
            if (filenames == null)
                return;

            for (int i = 0; i < filenames.Count; i++)
            {
                try
                {
                    File.Delete(filenames[i]);
                }
                catch
                {
                }
            }
        }

        // 
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        /// <summary>
        /// 从路径中取出库名部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回库名部分</returns>
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }

        // 
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        /// <summary>
        /// 从路径中取出记录号部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回记录号部分</returns>
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

#if NO
        // 从路径中取出id部分
        // 原来在entityform.cs中
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(nRet + 1).Trim();
        }
#endif


        // 从ISBN号中取得出版社号部分
        // 本函数可以自动适应有978前缀的新型ISBN号
        // 注意ISBN号中必须有横杠
        // parameters:
        //      strPublisherNumber  出版社号码。不包含978-部分
        /// <summary>
        /// 从 ISBN 号中取得出版社号部分
        /// 本函数可以自动适应有 978 前缀的新型 ISBN 号
        /// 注意 ISBN 号中必须有横杠
        /// </summary>
        /// <param name="strISBN">ISBN 号字符串</param>
        /// <param name="strPublisherNumber">返回出版社号码部分。不包含 978- 部分</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
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

            if (strFirstPart == "978" || strFirstPart == "979")
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
    }

    /// <summary>
    /// 出版物类型
    /// </summary>
    public enum PublicationType
    {
        Book = 0,
        Series = 1,
    }

    /// <summary>
    /// 浏览器控件信息
    /// </summary>
    internal class WebBrowserInfo
    {
        /// <summary>
        /// 是否至少使用过一次
        /// </summary>
        public bool Cleared = false;    // 是否被使用过
    }
}
