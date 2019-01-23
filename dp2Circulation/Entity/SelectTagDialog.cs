using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.RFID;
using static dp2Circulation.MyForm;

// TODO: 1) ListTags 动作移动到对话框里面来做；2)自动不停刷新列表；3)自动填充 PII 等栏
namespace dp2Circulation
{
    public partial class SelectTagDialog : Form
    {
        // [in][out] 当前选中的事项的 PII
        // 注：null 表示不使用它。"" 表示要定位到一个空标签
        public string SelectedPII { get; set; }

        // 是否自动关闭对话框。条件是 SelectedPII 事项被自动选定了
        public bool AutoCloseDialog { get; set; }

        public SelectTagDialog()
        {
            InitializeComponent();
        }

        private void SelectTagDialog_Load(object sender, EventArgs e)
        {
#if NO
            foreach (OneTag tag in _tags)
            {
                ListViewItem item = new ListViewItem(tag.ReaderName);
                ListViewUtil.ChangeItemText(item, 1, tag.UID);
                item.Tag = tag;
                this.listView1.Items.Add(item);
            }

            if (this.listView1.Items.Count > 0)
                ListViewUtil.SelectLine(this.listView1, 0, true);
#endif
            Task.Run(() => { UpdateChipList(); });
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView1.SelectedItems.Count == 0)
            {
                strError = "请选择一个标签";
                goto ERROR1;
            }

#if NO
            if (this.listView1.SelectedItems.Count > 0)
                this.SelectedTag = (OneTag)this.listView1.SelectedItems[0].Tag;
            else
                this.SelectedTag = null;
#endif
            if (this.listView1.SelectedItems.Count > 0)
                this.SelectedPII = ListViewUtil.GetItemText(this.listView1.SelectedItems[0], COLUMN_PII);
            else
                this.SelectedPII = null;

            Debug.Assert(this.SelectedTag != null);
            if (this.SelectedTag != null
                && this.SelectedTag.TagInfo == null
                && this.listView1.SelectedItems.Count > 0)
            {
                Debug.Assert(this.listView1.SelectedItems.Count > 0);
                GetTagInfo(this.listView1.SelectedItems[0]);
                strError = "您选择的行尚未获得 TagInfo。请稍候重试";
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

#if NO
        List<OneTag> _tags = new List<OneTag>();

        public List<OneTag> Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                _tags = value;
            }
        }
#endif

        public OneTag SelectedTag = null;

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                this.SelectedTag = (OneTag)this.listView1.SelectedItems[0].Tag;
                this.button_OK.Enabled = true;
            }
            else
            {
                this.SelectedTag = null;
                this.button_OK.Enabled = false;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            this.button_OK_Click(sender, e);
        }

        // 更新标签列表
        bool UpdateChipList()
        {
            string strError = "";
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
            {
                strError = "尚未配置 RFID 中心 URL";
                goto ERROR1;
            }
            RfidChannel channel = StartRfidChannel(
                Program.MainForm.RfidCenterUrl,
                out strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                goto ERROR1;
            }
            try
            {
                ListTagsResult result = channel.Object.ListTags("*");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                List<Task> tasks = new List<Task>();
                bool is_empty = false;

                this.Invoke((Action)(() =>
                {
                    is_empty = this.listView1.Items.Count == 0;

                    List<ListViewItem> items = new List<ListViewItem>();
                    foreach (OneTag tag in result.Results)
                    {
                        ListViewItem item = FindItem(this.listView1,
                            tag.ReaderName,
                            tag.UID);
                        if (item == null)
                        {
                            item = new ListViewItem(tag.ReaderName);
                            ListViewUtil.ChangeItemText(item, 1, tag.UID);
                            item.Tag = tag;
                            this.listView1.Items.Add(item);

                            if (tag.TagInfo == null)
                            {
                                // 启动单独的线程去填充 .TagInfo
                                tasks.Add(Task.Run(() => { GetTagInfo(item); }));
                            }
                        }

                        items.Add(item);
                    }

                    // 交叉运算得到比 items 中多出来的 ListViewItem，删除它们
                    List<ListViewItem> delete_items = new List<ListViewItem>();
                    foreach (ListViewItem item in this.listView1.Items)
                    {
                        if (items.IndexOf(item) == -1)
                            delete_items.Add(item);
                    }

                    foreach (ListViewItem item in delete_items)
                    {
                        this.listView1.Items.Remove(item);
                    }
                }));

                // 再建立一个 task，等待 tasks 执行完以后，自动选定一个 item
                if (tasks.Count > 0)
                {
                    Task.Run(() =>
                    {
                        Task.WaitAll(tasks.ToArray());
                        this.Invoke((Action)(() =>
                        {
                            // 首次填充，自动设好选定状态
                            if (is_empty)
                            {
                                SelectItem(this.SelectedPII);

                                if (string.IsNullOrEmpty(this.SelectedPII) == false
                                    && this.AutoCloseDialog)
                                    this.button_OK_Click(this, new EventArgs());
                            }
                        }));
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                strError = "UpdateChipList() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                EndRfidChannel(channel);
            }
            ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        const int COLUMN_READERNAME = 0;
        const int COLUMN_UID = 1;
        const int COLUMN_PII = 2;

        bool SelectItem(string pii)
        {
            if (pii == null)
                return false;
            foreach (ListViewItem item in this.listView1.Items)
            {
                string current_pii = ListViewUtil.GetItemText(item, COLUMN_PII);
                if (current_pii == pii)
                {
                    ListViewUtil.SelectLine(item, true);
                    return true;
                }
            }

            return false;
        }

        // 根据读卡器名字和标签 UID 找到已有的 ListViewItem 对象
        static ListViewItem FindItem(ListView list,
            string reader_name,
            string uid)
        {
            foreach (ListViewItem item in list.Items)
            {
                OneTag tag = (OneTag)item.Tag;
                if (tag.ReaderName == reader_name && tag.UID == uid)
                    return item;
            }

            return null;
        }

        void GetTagInfo(ListViewItem item)
        {
            OneTag tag = (OneTag)item.Tag;

            RfidChannel channel = StartRfidChannel(
    Program.MainForm.RfidCenterUrl,
    out string strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                goto ERROR1;
            }
            try
            {
                GetTagInfoResult result = channel.Object.GetTagInfo("*", tag.UID);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                tag.TagInfo = result.TagInfo;

                LogicChip chip = LogicChip.From(result.TagInfo.Bytes,
                    (int)result.TagInfo.BlockSize);

                this.Invoke((Action)(() =>
                {
                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    ListViewUtil.ChangeItemText(item, COLUMN_PII, pii);
                    if (pii == this.SelectedPII)
                        item.Font = new Font(item.Font, FontStyle.Bold);
                }));
                return;
            }
            catch (Exception ex)
            {
                strError = "ListTags() 出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                EndRfidChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, COLUMN_PII, "error:" + strError);
                // TODO: 把 item 修改为红色背景，表示出错的状态
            }));
        }

        private void button_refresh1_Click(object sender, EventArgs e)
        {
            Task.Run(() => { UpdateChipList(); });
        }
    }
}
