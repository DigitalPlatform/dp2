using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关 RFID 的功能
    /// </summary>
    public partial class MainForm
    {
        public static bool GetOwnerInstitution(
    XmlDocument cfg_dom,
    string strLocation,
    out string isil,
    out string alternative)
        {
            isil = "";
            alternative = "";

            if (cfg_dom == null)
                return false;

            return LibraryServerUtil.GetOwnerInstitution(
                cfg_dom.DocumentElement,
                strLocation,
                "entity",
                out isil,
                out alternative);
        }

        public static bool GetOwnerInstitution(
XmlDocument cfg_dom,
string libraryCode,
XmlDocument readerdom,
out string isil,
out string alternative)
        {
            isil = "";
            alternative = "";

            if (cfg_dom == null)
                return false;

            return LibraryServerUtil.GetOwnerInstitution(
                cfg_dom.DocumentElement,
                libraryCode,
                readerdom,
                out isil,
                out alternative);
        }

#if REMOVED
        /*
<rfid>
	<ownerInstitution>
		<item map="海淀分馆/" isil="test" />
		<item map="西城/" alternative="xc" />
	</ownerInstitution>
</rfid>
    map 为 "/" 或者 "/阅览室" 可以匹配 "图书总库" "阅览室" 这样的 strLocation
    map 为 "海淀分馆/" 可以匹配 "海淀分馆/" "海淀分馆/阅览室" 这样的 strLocation
    最好单元测试一下这个函数
         * */
        // parameters:
        //      cfg_dom 根元素是 rfid
        //      strLocation 纯净的 location 元素内容。
        //      isil    [out] 返回 ISIL 形态的代码
        //      alternative [out] 返回其他形态的代码
        // return:
        //      true    找到。信息在 isil 和 alternative 参数里面返回
        //      false   没有找到
        public static bool GetOwnerInstitution(
            XmlDocument cfg_dom,
            string strLocation,
            out string isil,
            out string alternative)
        {
            isil = "";
            alternative = "";

            if (cfg_dom == null)
                return false;

            if (strLocation != null 
                && strLocation.IndexOfAny(new char[] { '*', '?' }) != -1)
                throw new ArgumentException($"参数 {nameof(strLocation)} 值({strLocation})中不应包含字符 '*' '?'", nameof(strLocation));

            // 分析 strLocation 是否属于总馆形态，比如“阅览室”
            // 如果是总馆形态，则要在前部增加一个 / 字符，以保证可以正确匹配 map 值
            // ‘/’字符可以理解为在馆代码和阅览室名字之间插入的一个必要的符号。这是为了弥补早期做法的兼容性问题
            Global.ParseCalendarName(strLocation,
        out string strLibraryCode,
        out string strRoom);
            if (string.IsNullOrEmpty(strLibraryCode))
                strLocation = "/" + strRoom;

            XmlNodeList items = cfg_dom.DocumentElement.SelectNodes(
                "ownerInstitution/item");
            List<HitItem> results = new List<HitItem>();
            foreach (XmlElement item in items)
            {
                string map = item.GetAttribute("map");

                if (StringUtil.RegexCompare(GetRegex(map), strLocation))
                // if (strLocation.StartsWith(map))
                {
                    HitItem hit = new HitItem { Map = map, Element = item };
                    results.Add(hit);
                }
            }

            if (results.Count == 0)
                return false;

            // 如果命中多个，要选出 map 最长的那一个返回

            // 排序，大在前
            if (results.Count > 0)
                results.Sort((a, b) => { return b.Map.Length - a.Map.Length; });

            var element = results[0].Element;
            isil = element.GetAttribute("isil");
            alternative = element.GetAttribute("alternative");

            // 2021/2/1
            if (string.IsNullOrEmpty(isil) && string.IsNullOrEmpty(alternative))
            {
                throw new Exception($"map 元素不合法，isil 和 alternative 属性均为空");
            }
            return true;
#if NO
            foreach (XmlElement item in items)
            {
                string map = item.GetAttribute("map");
                if (strLocation.StartsWith(map))
                {
                    isil = item.GetAttribute("isil");
                    alternative = item.GetAttribute("alternative");
                    return true;
                }
            }
#endif

            return false;
        }

        class HitItem
        {
            public XmlElement Element { get; set; }
            public string Map { get; set; }
        }

        static string GetRegex(string pattern)
        {
            if (pattern == null)
                pattern = "";
            if (pattern.Length > 0 && pattern[pattern.Length - 1] != '*')
                pattern += "*";
            return "^" + Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".")
            + "$";
        }

#endif

        // 从册记录的卷册信息字符串中，获得符合 RFID 标准的 SetInformation 信息
        public static string GetSetInformation(string strVolume)
        {
            if (strVolume.IndexOf("(") == -1)
                return null;
            int offs = strVolume.IndexOf("(");
            if (offs == -1)
                return null;
            strVolume = strVolume.Substring(offs + 1).Trim();
            offs = strVolume.IndexOf(")");
            if (offs != -1)
                strVolume = strVolume.Substring(0, offs).Trim();

            strVolume = StringUtil.Unquote(strVolume, "()");
            offs = strVolume.IndexOf(",");
            if (offs == -1)
                return null;
            List<string> parts = StringUtil.ParseTwoPart(strVolume, ",");
            // 2 4 6 字符
            string left = parts[0].Trim(' ').TrimStart('0');
            string right = parts[1].Trim(' ').TrimStart('0');
            if (StringUtil.IsNumber(left) == false
                || StringUtil.IsNumber(right) == false)
                return null;

            // 看值是否超过 0-255
            if (int.TryParse(left, out int v) == false)
                return null;
            if (v < 0 || v > 255)
                return null;
            if (int.TryParse(right, out v) == false)
                return null;
            if (v < 0 || v > 255)
                return null;

            int max_length = Math.Max(left.Length, right.Length);
            if (max_length == 0 || max_length > 3)
                return null;
            return left.PadLeft(max_length, '0') + right.PadLeft(max_length, '0');
        }


        #region 独立线程写入统计日志

        public class StatisLog
        {
            public BookItem BookItem { get; set; }
            public string ReaderName { get; set; }
            public TagInfo NewTagInfo { get; set; }
            public string Xml { get; set; }

            // 写入出错次数
            public int ErrorCount { get; set; }
        }

        static object _lockStatis = new object();

        static List<StatisLog> _statisLogs = new List<StatisLog>();

        void StartStatisLogWorker(CancellationToken token)
        {
            bool _hide_dialog = false;
            int _hide_dialog_count = 0;

            _ = Task.Factory.StartNew(async () =>
            {
                await WriteStatisLogsAsync(token,
                    (c, m, buttons, sec) =>
                    {
                        DialogResult result = DialogResult.Yes;
                        if (_hide_dialog == false)
                        {
                            this.Invoke((Action)(() =>
                            {
                                result = MessageDialog.Show(this,
                            m,
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxDefaultButton.Button1,
                            "此后不再出现本对话框",
                            ref _hide_dialog,
                            buttons,
                            sec);
                            }));
                            _hide_dialog_count = 0;
                        }
                        else
                        {
                            _hide_dialog_count++;
                            if (_hide_dialog_count > 10)
                                _hide_dialog = false;
                        }

                        if (result == DialogResult.Yes)
                            return buttons[0];
                        else if (result == DialogResult.No)
                            return buttons[1];
                        return buttons[2];
                    }
                    );
            },
            token,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // TODO: 退出 MainForm 的时候，如果发现有没有来得及写入的事项，要写入临时文件，等下载启动时候重新装入队列
        // 循环写入统计日志的过程
        async Task WriteStatisLogsAsync(CancellationToken token,
            LibraryChannelExtension.delegate_prompt prompt)
        {
            // TODO: 需要捕获异常，写入错误日志
            try
            {
                while (token.IsCancellationRequested == false)
                {
                    List<StatisLog> error_items = new List<StatisLog>();
                    // 循环过程不怕 _statisLogs 数组后面被追加新内容
                    int count = _statisLogs.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var log = _statisLogs[i];
                        Program.MainForm.OperHistory.AppendHtml($"<div class='debug recpath'>写册 '{HttpUtility.HtmlEncode(log.BookItem.Barcode)}' 的 RFID 标签，记入统计日志</div>");
                        // parameters:
                        //      prompt_action   [out] 重试/中断
                        // return:
                        //      -2  UID 已经存在
                        //      -1  出错。注意 prompt_action 中有返回值，表明已经提示和得到了用户反馈
                        //      其他  成功
                        int nRet = WriteStatisLog("sender",
                            "subject",
                            log.Xml,
                            prompt,
                            out string prompt_action,
                            out string strError);
                        if (nRet == -2)
                        {
                            // 如果 UID 重复了，跳过这一条
                            _statisLogs.RemoveAt(i);
                            i--;
                            Program.MainForm.OperHistory.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode(strError)}</div>");
                            continue;
                        }
                        else if (nRet == -1)
                        {
                            if (prompt_action == "skip" || prompt_action == "取消")
                            {
                                // 跳过这一条
                                _statisLogs.RemoveAt(i);
                                i--;
                                Program.MainForm.OperHistory.AppendHtml($"<div class='debug error'>遇到错误 {HttpUtility.HtmlEncode(strError)} 后用户选择跳过</div>");
                                continue;
                            }

                            log.ErrorCount++;
                            error_items.Add(log);
                            // this.ShowMessage(strError, "red", true);
                            // TODO: 输出到操作历史
                            Program.MainForm.OperHistory.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode(strError)}</div>");
                        }
                        else
                            Program.MainForm.OperHistory.AppendHtml($"<div class='debug green'>写入成功</div>");
                    }

                    lock (_lockStatis)
                    {
                        _statisLogs.RemoveRange(0, count);
                        _statisLogs.AddRange(error_items);  // 准备重做
                    }

                    if (error_items.Count > 0)
                        await Task.Delay(TimeSpan.FromMinutes(1), token);
                    else
                        await Task.Delay(500, token);
                }
            }
            catch(TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                // this.ShowMessage($"后台线程出现异常: {ex.Message}", "red", true);
                Program.MainForm.OperHistory.AppendHtml($"<div class='debug error'>{HttpUtility.HtmlEncode($"RFID 统计日志后台线程出现异常: {ex.Message}")}</div>");
                WriteErrorLog($"RFID 统计日志后台线程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                /*
                this.Invoke((Action)(() =>
                {
                    this.Enabled = false;   // 禁用界面，迫使操作者关闭窗口重新打开
                }));
                */
            }
        }

        public static void AddWritingLog(StatisLog log)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            DomUtil.SetElementText(dom.DocumentElement,
                "action", "writeRfidTag");

            DomUtil.SetElementText(dom.DocumentElement,
    "uid", Guid.NewGuid().ToString());

            DomUtil.SetElementText(dom.DocumentElement,
    "type", "item");

            DomUtil.SetElementText(dom.DocumentElement,
                "itemBarcode", log.BookItem.Barcode);
            DomUtil.SetElementText(dom.DocumentElement,
                "itemLocation", log.BookItem.Location);
            // 2019/6/28
            DomUtil.SetElementText(dom.DocumentElement,
    "itemRefID", log.BookItem.RefID);

            DomUtil.SetElementText(dom.DocumentElement,
"tagProtocol", "ISO15693");
            DomUtil.SetElementText(dom.DocumentElement,
"tagReaderName", log.ReaderName);
            DomUtil.SetElementText(dom.DocumentElement,
    "tagAFI", Element.GetHexString(log.NewTagInfo.AFI));
            DomUtil.SetElementText(dom.DocumentElement,
    "tagBlockSize", log.NewTagInfo.BlockSize.ToString());
            DomUtil.SetElementText(dom.DocumentElement,
"tagMaxBlockCount", log.NewTagInfo.MaxBlockCount.ToString());
            DomUtil.SetElementText(dom.DocumentElement,
    "tagDSFID", Element.GetHexString(log.NewTagInfo.DSFID));
            DomUtil.SetElementText(dom.DocumentElement,
    "tagUID", log.NewTagInfo.UID);
            DomUtil.SetElementText(dom.DocumentElement,
    "tagBytes", Convert.ToBase64String(log.NewTagInfo.Bytes));

            log.Xml = dom.OuterXml;
            lock (_lockStatis)
            {
                _statisLogs.Add(log);
            }
        }

        // 写入统计日志
        // parameters:
        //      prompt_action   [out] 重试/取消
        // return:
        //      -2  UID 已经存在
        //      -1  出错。注意 prompt_action 中有返回值，表明已经提示和得到了用户反馈
        //      其他  成功
        public int WriteStatisLog(
            string strSender,
            string strSubject,
            string strXml,
            LibraryChannelExtension.delegate_prompt prompt,
            out string prompt_action,
            out string strError)
        {
            prompt_action = "";
            strError = "";

            LibraryChannel channel = this.GetChannel();
            try
            {
                var message = new MessageData
                {
                    strRecipient = "!statis",
                    strSender = strSender,
                    strSubject = strSubject,
                    strMime = "text/xml",
                    strBody = strXml
                };
                MessageData[] messages = new MessageData[]
                {
                    message
                };

            REDO:
                long lRet = channel.SetMessage(
                    "send",
                    "",
                    messages,
                    out MessageData[] output_messages,
                    out strError);
                if (lRet == -1)
                {
                    // 不使用 prompt
                    if (channel.ErrorCode == ErrorCode.AlreadyExist)
                        return -2;
                    if (prompt == null)
                        return -1;
                    // TODO: 遇到出错，提示人工介入处理
                    if (prompt != null)
                    {
                        var result = prompt(channel,
                            strError + "\r\n\r\n(重试) 重试写入; (取消) 取消写入",
                            new string[] { "重试", "取消" },
                            10);
                        if (result == "重试")
                            goto REDO;
                        prompt_action = result;
                        return -1;
                    }
                }

                return (int)lRet;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }


        #endregion

    }
}
