
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

            NormalResult result = RfidManager.SetEAS(reader_name,
tag_name,
enable);
            TagList.ClearTagTable("");

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
            return item;
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

        public NormalResult TryCorrectEas(string uid, string pii)
        {
            var result = _tryCorrectEas(uid, pii);
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
        public NormalResult _tryCorrectEas(string uid, string pii)
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

            var gettag_result = RfidManager.GetTagInfo("*", uid);
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
                seteas_result = RfidManager.SetEAS("*", "uid:" + tag_info.UID, false);
                TagList.SetEasData(tag_info.UID, false);
            }
            else if (nRet == 0 && tag_info.EAS == false)
            {
                seteas_result = RfidManager.SetEAS("*", "uid:" + tag_info.UID, true);
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
}
