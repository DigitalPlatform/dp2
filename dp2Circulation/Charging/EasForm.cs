
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2Circulation.Charging
{
    // 用于配合快捷出纳窗的，能显示 EAS 修改操作出错，和允许用户重试修改的窗口
    public partial class EasForm : MyForm
    {
        public event EasChangedEventHandler EasChanged = null;

        public EasForm()
        {
            InitializeComponent();

            this.SuppressSizeSetting = true;  // 不需要基类 MyForm 的尺寸设定功能
        }

        // https://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /*
        private const int WS_EX_TOPMOST = 0x00000008;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TOPMOST;
                return createParams;
            }
        }
        */

        // result.Value:
        //      -1  没有找到
        //      其他 天线编号
        NormalResult GetAntennaByUID(string uid)
        {
            foreach (var data in TagList.Books)
            {
                if (data.OneTag.UID != uid)
                    continue;
                if (data.OneTag.TagInfo == null)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"UID 为 '{uid}' 的标签暂时 TagInfo == null，无法获得天线编号"
                    };
                return new NormalResult { Value = (int)data.OneTag.AntennaID };
            }

            // TODO: 此时可否发起一次 GetTagInfo 请求？
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = $"UID 为 '{uid}' 的标签不在读卡器上，无法获得其天线编号"
            };
        }

        // 从 TagList.Books 中获得一个标签的 EAS 状态
        // result.Value:
        //      -1  出错
        //      0   Off
        //      1   On
        GetEasStateResult GetEasStateByUID(string uid)
        {
            foreach (var data in TagList.Books)
            {
                if (data.OneTag.UID != uid)
                    continue;
                if (data.OneTag.TagInfo == null)
                    return new GetEasStateResult
                    {
                        Value = -1,
                        ErrorInfo = $"UID 为 '{uid}' 的标签暂时 TagInfo == null，无法获得 EAS 状态"
                    };
                return new GetEasStateResult
                {
                    Value = data.OneTag.TagInfo.EAS ? 1 : 0,
                    AntennaID = data.OneTag.AntennaID,
                    ReaderName = data.OneTag.ReaderName
                };
            }

            // TODO: 此时可否发起一次 GetTagInfo 请求？
            return new GetEasStateResult
            {
                Value = -1,
                ErrorInfo = $"UID 为 '{uid}' 的标签不在读卡器上，无法获得其 EAS 状态"
            };
        }

        // 从 TagList.Books 中获得一个标签的 EAS 状态
        // result.Value:
        //      -1  出错
        //      0   Off
        //      1   On
        GetEasStateResult GetEasStateByPII(string pii)
        {
            foreach (var data in TagList.Books)
            {
                TagInfo tag_info = null;
                if (data.OneTag.TagInfo == null)
                {
                    // TODO: 出错了考虑重试一下
                    // result.Value
                    //      -1
                    //      0
                    var result = RfidManager.GetTagInfo("*",
                        data.OneTag.UID,
                        data.OneTag.AntennaID);
                    if (result.Value == -1)
                        continue;
                    tag_info = result.TagInfo;
                }
                else
                    tag_info = data.OneTag.TagInfo;

                string current_pii = QuickChargingForm.GetPII(tag_info);
                if (current_pii == pii)
                {
                    return new GetEasStateResult
                    {
                        Value = tag_info.EAS ? 1 : 0,
                        AntennaID = tag_info.AntennaID,
                        ReaderName = tag_info.ReaderName
                    };
                }
            }

            return new GetEasStateResult
            {
                Value = -1,
                ErrorInfo = $"PII 为 '{pii}' 的标签不在读卡器上，无法获得其 EAS 状态"
            };
        }

        // result.Value:
        //      -1  出错
        //      0   Off
        //      1   On
        internal GetEasStateResult GetEAS(string reader_name,
            string tag_name)
        {
            Parse(tag_name, out string uid, out string pii);
            if (string.IsNullOrEmpty(uid) == false)
                return GetEasStateByUID(uid);
            if (string.IsNullOrEmpty(pii) == false)
                return GetEasStateByPII(pii);

            return new GetEasStateResult
            {
                Value = -1,
                ErrorInfo = $"tag_name '{tag_name}' 不合法"
            };
        }

        // 设置 EAS 状态。如果失败，会在 ListView 里面自动添加一行，以备后面用户在读卡器上放标签时候自动修正
        internal NormalResult SetEAS(
            object task,
            string reader_name,
            string tag_name,
            bool enable)
        {
            // 解析 tag_name 里面的 UID 或者 PII
            string uid = "";
            string pii = "";
            List<string> parts = StringUtil.ParseTwoPart(tag_name, ":");
            if (parts[0] == "pii")
                pii = parts[1];
            else if (parts[0] == "uid" || string.IsNullOrEmpty(parts[0]))
                uid = parts[1];
            else
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"未知的 tag_name 前缀 '{parts[0]}'",
                    ErrorCode = "unknownPrefix"
                };

            // 尝试改变 tag_name 形态，优化后面请求 EAS 写入的速度
            {
                tag_name = ConvertTagNameString(tag_name);
                if (string.IsNullOrEmpty(uid))
                {
                    Parse(tag_name, out string temp_uid, out string temp_pii);
                    uid = temp_uid;
                }
            }

            // 2019/9/29
            // 获得标签的天线编号
            // result.Value:
            //      -1  没有找到
            //      其他 天线编号
            var antenna_result = GetAntennaByUID(uid);
            if (antenna_result.Value == -1)
            {
                // 2020/6/28
                // 尝试找天线编号时候失败，这时候也要加入一个新行，便于工作人员后面放上去图书修复 EAS
                this.Invoke((Action)(() =>
                {
                    // 加入一个新行
                    AddLine(uid, pii, task);
                }));
                return antenna_result;
            }

            NormalResult result = null;
            for (int i = 0; i < 2; i++)
            {
                result = RfidManager.SetEAS(reader_name,
    tag_name,
    (uint)antenna_result.Value,
    enable);
                if (result.Value == 1)
                    break;
            }
            TagList.ClearTagTable(uid);
            TaskList.FillTagList();

            if (result.Value != 1)
            {
                this.Invoke((Action)(() =>
                {
                    // 加入一个新行
                    AddLine(uid, pii, task);
                }));
            }
            else
            {
                /*
                EasChanged?.Invoke(this,
                    new EasChangedEventArgs
                    {
                        UID = uid,
                        PII = pii,
                        Param = task
                    });
                    */
            }

            return result;
        }



        // TODO: 注意查重。避免插入重复的行
        ListViewItem AddLine(string uid, string pii, object o)
        {
            ListViewItem item = new ListViewItem();
            item.Tag = new ItemInfo { Param = o };
            ListViewUtil.ChangeItemText(item, 0, uid);
            ListViewUtil.ChangeItemText(item, 1, pii);
            this.listView1.Items.Add(item);
            OnItemChanged();

            // 自动填充书目摘要列
            Task.Run(() =>
            {
                FillBiblioSummary(new List<ListViewItem>() { item });
            });

            return item;
        }

        // 限制获取摘要时候可以并发使用的 LibraryChannel 通道数
        static Semaphore _limit = new Semaphore(1, 1);

        void FillBiblioSummary(List<ListViewItem> items_param)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            List<string> piis = new List<string>();

            // 第一阶段，准备 ListViewItem 和 PII 集合
            this.Invoke((Action)(() =>
            {
                foreach (ListViewItem item in items_param)
                {
                    string summary = ListViewUtil.GetItemText(item, 2);
                    if (string.IsNullOrEmpty(summary) == false)
                        continue;
                    items.Add(item);
                    piis.Add(ListViewUtil.GetItemText(item, 1));
                }
            }));

            if (piis.Count == 0)
                return;

            // 第二阶段，从服务器获得摘要
            List<string> summarys = new List<string>();

            foreach (string pii in piis)
            {
                // 获得缓存中的bibliosummary
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = Program.MainForm.GetCachedBiblioSummary(pii,
null,
out string strSummary,
out string strError);
                if (nRet == -1)
                    strSummary = $"GetCachedBiblioSummary() 出错: {strError}";
                else if (nRet == 1)
                {

                }
                else
                {
                    try
                    {
                        // TODO: 需要处理 WaitOne() 返回 false 的情况
                        // true if the current instance receives a signal; otherwise, false.
                        _limit.WaitOne(TimeSpan.FromSeconds(10));

                        LibraryChannel channel = Program.MainForm.GetChannel();
                        TimeSpan old_timeout = channel.Timeout;
                        channel.Timeout = new TimeSpan(0, 0, 5);

                        try
                        {
                            long lRet = channel.GetBiblioSummary(
        null,
        pii,
        null,
        null,
        out string strBiblioRecPath,
        out strSummary,
        out strError);
                            if (lRet == -1)
                            {
                                strSummary = $"出错: {strError}";
                            }
                            else
                            {
                                Program.MainForm.SetBiblioSummaryCache(pii,
                                     null,
                                     strSummary);
                            }

                        }
                        finally
                        {
                            channel.Timeout = old_timeout;
                            Program.MainForm.ReturnChannel(channel);

                            _limit.Release();
                        }
                    }
                    catch (Exception ex)
                    {
                        strSummary = $"异常: {ex.Message}";
                    }
                }

                summarys.Add(strSummary);
            }

            this.Invoke((Action)(() =>
            {
                // 第三阶段填充
                int i = 0;
                foreach (string summary in summarys)
                {
                    ListViewItem item = items[i];

                    ListViewUtil.ChangeItemText(item, 2, summary);
                    i++;
                }
            }));
        }

        // uid 和 pii 调用时都尽量提供，匹配上一个就算数
        // parameters:
        //      uid
        //      pii
        ListViewItem FindLine(string uid, string pii)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                string current_uid = ListViewUtil.GetItemText(item, 0);
                string current_pii = ListViewUtil.GetItemText(item, 1);
                if (string.IsNullOrEmpty(uid) == false && string.IsNullOrEmpty(current_uid) == false)
                {
                    if (uid == current_uid)
                        return item;
                }
                if (string.IsNullOrEmpty(pii) == false && string.IsNullOrEmpty(current_pii) == false)
                {
                    if (pii == current_pii)
                        return item;
                }
            }

            return null;
        }

        public NormalResult TryCorrectEas(string uid,
            uint antenna_id,
            string pii)
        {
            var result = _tryCorrectEas(uid, antenna_id, pii);
            if (result.Value == -1)
                this.ShowMessage($"尝试自动修正册 '{pii}' EAS 时出错 '{result.ErrorInfo}'", "red", true);
            else if (result.Value == 1)
                this.ShowMessage($"册 '{pii}' EAS 修正成功", "green", true);

            return result;
        }


        // 尝试自动修正 EAS
        // result.Value
        //      -1  出错
        //      0   ListsView 中没有找到事项
        //      1   发生了修改
        public NormalResult _tryCorrectEas(string uid,
            uint antenna_id,
            string pii)
        {
            // 先检查 uid 和 pii 是否在列表中？
            ListViewItem item = (ListViewItem)this.Invoke((Func<ListViewItem>)(() =>
            {
                return FindLine(uid, pii);
            }));

            if (item == null)
                return new NormalResult { Value = 0 };

            // 获得册记录 XML
            var result = GetItemXml(pii);
            if (result.Value == -1)
            {
                return result;
            }

            // 获得册记录的外借状态。
            // return:
            //      -2  册记录为空，无法判断状态
            //      -1  出错
            //      0   没有被外借
            //      1   在外借状态
            int nRet = RfidToolForm.GetCirculationState(result.ItemXml,
                out string strError);
            if (nRet == -1 || nRet == -2)
            {
                return new NormalResult { Value = -1, ErrorInfo = strError };
            }

            var gettag_result = RfidManager.GetTagInfo("*", uid, antenna_id);
            if (gettag_result.Value == -1)
            {
                return gettag_result;
            }

            var tag_info = gettag_result.TagInfo;

            // 检查册记录外借状态和 EAS 状态是否符合
            // 检测 EAS 是否正确
            NormalResult seteas_result = new NormalResult { Value = 1 };

            if (nRet == 1 && tag_info.EAS == true)
            {
                seteas_result = RfidManager.SetEAS("*", "uid:" + tag_info.UID, tag_info.AntennaID, false);
                TagList.SetEasData(tag_info.UID, false);
            }
            else if (nRet == 0 && tag_info.EAS == false)
            {
                seteas_result = RfidManager.SetEAS("*", "uid:" + tag_info.UID, tag_info.AntennaID, true);
                TagList.SetEasData(tag_info.UID, true);
            }
            else
            {
                // 没有发生修改
            }

            // set_result
            // return result.Value:
            //      -1  出错
            //      0   没有找到指定的标签
            //      1   找到，并成功修改 EAS
            if (seteas_result.Value != 1)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = seteas_result.ErrorInfo
                };
            }

            ItemInfo info = (ItemInfo)item.Tag;

            // 触发事件
            this.EasChanged?.Invoke(this, new EasChangedEventArgs
            {
                PII = pii,
                UID = uid,
                Param = info.Param
            });

            // 从 ListView 中删除这一行
            this.Invoke((Action)(() =>
            {
                this.listView1.Items.Remove(item);
                OnItemChanged();
            }));

            return seteas_result;
        }

        public int ErrorCount
        {
            get
            {
                return this.listView1.Items.Count;
            }
        }

        class GetItemXmlResult : NormalResult
        {
            public string ItemXml { get; set; }
        }

        GetItemXmlResult GetItemXml(string pii)
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                long lRet = channel.GetItemInfo(null,
                    pii,
                    "xml",
                    out string strItemXml,
                    "xml",
                    out string strBiblio,
                    out string strError);
                if (lRet == -1)
                    return new GetItemXmlResult
                    {
                        Value = -1,
                        ErrorCode = channel.ErrorCode.ToString(),
                        ErrorInfo = strError
                    };
                return new GetItemXmlResult
                {
                    Value = 0,
                    ItemXml = strItemXml
                };
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        class ItemInfo
        {
            // 册记录 XML
            public string Xml { get; set; }
            public object Param { get; set; }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Visible = false;
                e.Cancel = true;
            }
        }

        private void EasForm_Load(object sender, EventArgs e)
        {
            _floatingMessage.AutoHide = false;
        }

        // 解析 tag_name 里面的 UID 或者 PII
        public static void Parse(string tag_name, out string uid, out string pii)
        {
            uid = "";
            pii = "";
            List<string> parts = StringUtil.ParseTwoPart(tag_name, ":");
            if (parts[0] == "pii")
                pii = parts[1];
            else if (parts[0] == "uid" || string.IsNullOrEmpty(parts[0]))
                uid = parts[1];
        }


        #region PII-->UID 对照表

        // PII --> UID 对照表
        Hashtable _piiTable = new Hashtable();

        // 保存对照关系
        public void SetUID(string pii, string uid)
        {
            lock (_piiTable.SyncRoot)
            {
                if (_piiTable.Count > 100)
                    _piiTable.Clear();

                _piiTable[pii] = uid;
            }
        }

        // 根据 PII 获得对应的 UID
        string GetUID(string pii)
        {
            lock (_piiTable.SyncRoot)
            {
                if (_piiTable.ContainsKey(pii) == false)
                    return null;
                return (string)_piiTable[pii];
            }
        }

        // 把 tag_name 字符串尽可能转换为 uid: 形态
        string ConvertTagNameString(string tag_name)
        {
            EasForm.Parse(tag_name, out string uid, out string pii);
            if (string.IsNullOrEmpty(uid) == false)
                return tag_name;
            if (string.IsNullOrEmpty(pii))
                return tag_name;

            var cache_uid = GetUID(pii);
            if (string.IsNullOrEmpty(cache_uid))
                return tag_name;

            return $"uid:{cache_uid}";
        }

        #endregion

        private void EasForm_Activated(object sender, EventArgs e)
        {
            //RfidManager.Pause = false;
        }

        // 移除全部事项
        private void toolStripButton_clearAll_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
            OnItemChanged();
        }

        // 切换为详细模式
        private void ToolStripMenuItem_detailMode_Click(object sender, EventArgs e)
        {
            this.DisplayMode = "detail";
        }

        // 切换为数字模式
        private void ToolStripMenuItem_numberMode_Click(object sender, EventArgs e)
        {
            this.DisplayMode = "number";
        }

        string _displayMode = "detail";

        // detail/number 之一
        public string DisplayMode
        {
            get
            {
                return _displayMode;
            }
            set
            {
                _displayMode = value;
                // 兑现显示格式变化
                if (value == "detail")
                {
                    this.tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Percent;
                    this.tableLayoutPanel1.RowStyles[0].Height = 100;

                    this.tableLayoutPanel1.RowStyles[2].SizeType = SizeType.AutoSize;
                    this.tableLayoutPanel1.RowStyles[2].Height = 100;


                    this.listView1.Visible = true;
                    this.label_message.Visible = true;
                    this.label_number.Visible = false;

                    this.ToolStripMenuItem_detailMode.Checked = true;
                    this.ToolStripMenuItem_numberMode.Checked = false;
                }
                else
                {
                    this.tableLayoutPanel1.RowStyles[0].SizeType = SizeType.AutoSize;
                    this.tableLayoutPanel1.RowStyles[0].Height = 100;

                    this.tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
                    this.tableLayoutPanel1.RowStyles[2].Height = 100;

                    this.listView1.Visible = false;
                    this.label_message.Visible = false;
                    this.label_number.Visible = true;

                    this.ToolStripMenuItem_detailMode.Checked = false;
                    this.ToolStripMenuItem_numberMode.Checked = true;

                }
            }
        }

        void OnItemChanged()
        {
            this.label_number.Text = this.listView1.Items.Count.ToString();
        }

        private void EasForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _limit.Dispose();
        }
    }

    /// <summary>
    /// 空闲事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void EasChangedEventHandler(object sender,
    EasChangedEventArgs e);

    /// <summary>
    /// 修改成功事件的参数
    /// </summary>
    public class EasChangedEventArgs : EventArgs
    {
        public string UID { get; set; }
        public string PII { get; set; }

        public object Param { get; set; }
    }

    public class GetEasStateResult : NormalResult
    {
        public string ReaderName { get; set; }
        public uint AntennaID { get; set; }
    }
}
