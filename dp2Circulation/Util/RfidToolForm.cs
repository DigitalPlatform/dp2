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
using DigitalPlatform.RFID.UI;

namespace dp2Circulation
{
    public partial class RfidToolForm : MyForm
    {
        // [in][out] 当前选中的事项的 PII
        public string SelectedPII { get; set; }

        const int COLUMN_READERNAME = 0;
        const int COLUMN_UID = 1;
        const int COLUMN_PII = 2;

        public RfidToolForm()
        {
            InitializeComponent();
        }

        private void RfidToolForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            Task.Run(() => { UpdateChipList(); });
        }

        private void RfidToolForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void RfidToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {

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
                    is_empty = this.listView_tags.Items.Count == 0;

                    List<ListViewItem> items = new List<ListViewItem>();
                    foreach (OneTag tag in result.Results)
                    {
                        ListViewItem item = FindItem(this.listView_tags,
                            tag.ReaderName,
                            tag.UID);
                        if (item == null)
                        {
                            item = new ListViewItem(tag.ReaderName);
                            ListViewUtil.ChangeItemText(item, 1, tag.UID);
                            item.Tag = new ItemInfo { OneTag = tag };
                            this.listView_tags.Items.Add(item);

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
                    foreach (ListViewItem item in this.listView_tags.Items)
                    {
                        if (items.IndexOf(item) == -1)
                            delete_items.Add(item);
                    }

                    foreach (ListViewItem item in delete_items)
                    {
                        this.listView_tags.Items.Remove(item);
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
                            }
                        }));
                        FillEntityInfo();
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

        private void toolStripButton_loadRfid_Click(object sender, EventArgs e)
        {
            Task.Run(() => { UpdateChipList(); });
        }

        bool SelectItem(string pii)
        {
            foreach (ListViewItem item in this.listView_tags.Items)
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
                ItemInfo item_info = (ItemInfo)item.Tag;
                OneTag tag = item_info.OneTag;
                if (tag.ReaderName == reader_name && tag.UID == uid)
                    return item;
            }

            return null;
        }

        void GetTagInfo(ListViewItem item)
        {
            ItemInfo item_info = (ItemInfo)item.Tag;
            OneTag tag = item_info.OneTag;

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

        private void listView_tags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_tags.SelectedItems.Count > 0)
            {
                ItemInfo item_info = (ItemInfo)this.listView_tags.SelectedItems[0].Tag;
                OneTag tag = item_info.OneTag;
                var tag_info = tag.TagInfo;

                var chip = LogicChipItem.FromTagInfo(tag_info);
                this.chipEditor1.LogicChipItem = chip;

                if (string.IsNullOrEmpty(item_info.Xml) == false)
                {
                    BookItem book_item = new BookItem();
                    int nRet = book_item.SetData("",
                        item_info.Xml,
                        null, 
                        out string strError);
                    if (nRet == -1)
                    {
                        // 如何报错?
                    }
                    else
                        this.propertyGrid_record.SelectedObject = book_item;
                }
                else
                    this.propertyGrid_record.SelectedObject = null;
            }
            else
            {
                this.chipEditor1.LogicChipItem = null;
                this.propertyGrid_record.SelectedObject = null;
            }
        }

        // 填充所有的册记录信息
        void FillEntityInfo()
        {
            LibraryChannel channel = this.GetChannel();
            try
            {
                foreach (ListViewItem item in this.listView_tags.Items)
                {
                    ItemInfo item_info = (ItemInfo)item.Tag;
                    var tag_info = item_info.OneTag.TagInfo;
                    if (tag_info == null)
                        continue;
                    LogicChip chip = LogicChip.From(tag_info.Bytes,
                        (int)tag_info.BlockSize);
                    string pii = chip.FindElement(ElementOID.PII)?.Text;
                    if (string.IsNullOrEmpty(pii))
                        continue;
                    long lRet = channel.GetItemInfo(null,
                        pii,
                        "xml",
                        out string xml,
                        "",
                        out string biblio,
                        out string strError);
                    if (lRet == -1)
                    {
                        // TODO: 给 item 设置出错状态
                        continue;
                    }

                    item_info.Xml = xml;


                }
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        class ItemInfo
        {
            public OneTag OneTag { get; set; }
            public string Xml { get; set; }
        }
    }
}
