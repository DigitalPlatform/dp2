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
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.Text;
using static dp2Circulation.MyForm;

namespace dp2Circulation
{
    public partial class RfidPatronCardDialog : Form
    {
        string _protocolFilter = InventoryInfo.ISO15693 + "," + InventoryInfo.ISO18000P6C;

        public string ProtocolFilter
        {
            get
            {
                return _protocolFilter;
            }
            set
            {
                _protocolFilter = value;

                SetTitle();
            }
        }

        void SetTitle()
        {
            this.TryInvoke(() =>
            {
                this.Text = $"RFID 读者卡 ({_protocolFilter})";
            });
        }

        // 左侧编辑器是否成功装载过
        bool _leftLoaded = false;

        public RfidPatronCardDialog()
        {
            InitializeComponent();

            this.chipEditor_existing.Title = "读者卡中原有内容";
            this.chipEditor_editing.Title = "即将写入的内容";
        }

        string _barcode = null;

        public void SetData(ReaderEditControl patron,
            string library_code,
            XmlDocument readerdom,
            out string strWarning)
        {
            strWarning = "";
            try
            {
                this._barcode = patron.Barcode;
                this.chipEditor_editing.LogicChipItem = BuildChip(patron,
                    library_code,
                    readerdom,
                    out strWarning);
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message);
            }
        }

        void SetMessage(string text)
        {
            //this.textBox_message.Text = text;
            //this.textBox_message.Visible = text != null;
        }

        // 根据 BookItem 对象构造一个 LogicChipItem 对象
        public static LogicChipItem BuildChip(ReaderEditControl patron,
            string library_code,
            XmlDocument readerdom,
            out string strWarning)
        {
            strWarning = "";

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.11") < 0)
                throw new Exception("当前连接的 dp2library 必须为 3.11 或以上版本，才能使用 RFID 有关功能");

            LogicChipItem result = new LogicChipItem();

            result.AFI = LogicChipItem.DefaultPatronAFI;
            result.DSFID = LogicChipItem.DefaultDSFID;
            result.EAS = LogicChipItem.DefaultPatronEAS;

            // barcode --> PII
            result.NewElement(ElementOID.PII, patron.Barcode);

            // location --> OwnerInstitution 要配置映射关系
            // 定义一系列前缀对应的 ISIL 编码。如果 location 和前缀前方一致比对成功，则得到 ISIL 编码
            var ret = MainForm.GetOwnerInstitution(
                Program.MainForm.RfidCfgDom,
                // library_code + "/", // 2020/7/17 增加 "/"
                library_code,
                readerdom,
                out string isil,
                out string alternative);
            if (string.IsNullOrEmpty(isil) == false)
            {
                result.NewElement(ElementOID.OwnerInstitution, isil);
            }
            else if (string.IsNullOrEmpty(alternative) == false)
            {
                result.NewElement(ElementOID.AlternativeOwnerInstitution, alternative);
            }
            else
            {
                strWarning = $"当前 library.xml 中没有为馆代码 '{library_code}' 配置机构代码，这样创建的(没有包含机构代码的) 读者卡容易和其他机构的发生混淆";
            }

            // TypeOfUsage?
            // (十六进制两位数字)
            {
                string typeOfUsage = "80";
                result.NewElement(ElementOID.TypeOfUsage, typeOfUsage);
            }

            // TODO: 验证码，用扩展元素

            return result;
        }

        private void toolStripButton_saveRfid_Click(object sender, EventArgs e)
        {
            // 写入以前，装载标签内容到左侧，然后调整右侧(中间可能会警告)。然后再保存
            string strError = "";

            string pii = this._barcode;   // 从修改前的

            // 看左侧是否装载过。如果没有装载过则自动装载
            if (_leftLoaded == false)
            {
                // return:
                //      -1  出错
                //      0   放弃装载
                //      1   成功装载
                int nRet = LoadOldChip(pii,
                    "adjust_right,saving",
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
$"装载标签原有内容发生错误: {strError}。\r\n\r\n是否继续保存新内容到此标签?",
"RfidPatronCardDialog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "已放弃保存 RFID 读者卡内容";
                    goto ERROR1;
                }
            }

            // 然后保存
            {
                int nRet = SaveNewChip(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            MessageBox.Show(this, "保存成功");

            // 刷新左侧显示
            {
                // 用保存后的确定了的 UID 重新装载
                int nRet = LoadChipByUID(
                    _tagExisting.ReaderName,
                    _tagExisting.TagInfo.UID,
                    _tagExisting.AntennaID,
    out TagInfo tag_info,
    out strError);
                if (nRet == -1)
                {
                    _leftLoaded = false;
                    strError = "保存 RFID 读者卡内容已经成功。但刷新左侧显示时候出错: " + strError;
                    goto ERROR1;
                }
                _tagExisting.TagInfo = tag_info;
                var chip = LogicChipItem.FromTagInfo(tag_info);
                this.chipEditor_existing.LogicChipItem = chip;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_loadRfid_Click(object sender, EventArgs e)
        {
            // 如果装入的元素里面有锁定状态的元素，要警告以后，覆盖右侧编辑器中的同名元素(右侧这些元素也要显示为只读状态)
            _leftLoaded = false;
            string pii = this._barcode;   // 从修改前的
            // return:
            //      -1  出错
            //      0   放弃装载
            //      1   成功装载
            int nRet = LoadOldChip(pii,
                "adjust_right,auto_close_dialog",
                out string strError);
            if (nRet != 1)
                goto ERROR1;
            _leftLoaded = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        OneTag _tagExisting = null;

        // 装入以前的标签信息
        // 如果读卡器上有多个标签，则出现对话框让从中选择一个。列表中和右侧 PII 相同的，优先被选定
        // parameters:
        //      strStyle    操作方式
        //                  auto_close_dialog  是否要自动关闭选择对话框。条件是选中了 auto_select_pii 事项
        //                  adjust_right    是否自动调整右侧元素。即，把左侧的锁定状态元素覆盖到右侧。调整前要询问。如果不同意调整，可以放弃，然后改为放一个空白标签并装载保存
        //                  saving  是否为了保存而装载？如果是，有些提示要改变
        // return:
        //      -1  出错
        //      0   放弃装载
        //      1   成功装载
        int LoadOldChip(
            string auto_select_pii,
            string strStyle,
            out string strError)
        {
            strError = "";

            bool auto_close_dialog = StringUtil.IsInList("auto_close_dialog", strStyle);
            bool adjust_right = StringUtil.IsInList("adjust_right", strStyle);
            bool saving = StringUtil.IsInList("saving", strStyle);

            try
            {
            REDO:
                // 出现对话框让选择一个
                // SelectTagDialog dialog = new SelectTagDialog();
                using (RfidToolForm dialog = new RfidToolForm())
                {
                    dialog.Text = "选择 RFID 读者卡";
                    dialog.OkCancelVisible = true;
                    dialog.LayoutVertical = false;
                    dialog.AutoCloseDialog = auto_close_dialog;
                    dialog.SelectedPII = auto_select_pii;
                    dialog.AutoSelectCondition = "auto_or_blankPII";    // 2019/1/30
                    dialog.ProtocolFilter = this.ProtocolFilter;
                    Program.MainForm.AppInfo.LinkFormState(dialog, "selectTagDialog_formstate");
                    dialog.ShowDialog(this);

                    if (dialog.DialogResult == DialogResult.Cancel)
                    {
                        strError = "放弃装载 RFID 读者卡内容";
                        return 0;
                    }

                    _tagExisting = dialog.SelectedTag;

                    {
                        // 可能会抛出异常
                        var old_chip = LogicChipItem.FromTagInfo(dialog.SelectedTag.TagInfo);

                        // 首先检查 typeOfUsage 是否为 8X
                        if (old_chip.IsBlank() == false && IsPatronUsage(old_chip) == false)
                        {
                            strError = "当前 RFID 标签是图书类型，不是读者卡类型。请小心不要弄混这两种类型";
                            string message = $"您所选择的 RFID 标签是图书类型，不是读者卡类型。请小心不要弄混这两种类型。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 读者卡;\r\n[否]将这一种不吻合的 RFID 标签装载进来\r\n[取消]放弃装载";
                            if (saving)
                                message = $"您所选择的 RFID 标签是图书类型，不是读者卡类型。请小心不要弄混这两种类型。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 读者卡;\r\n[否]将信息覆盖保存到这一种不吻合的 RFID 标签中(危险)\r\n[取消]放弃保存";

                            DialogResult temp_result = MessageBox.Show(this,
    message,
    "RfidPatronCardDialog",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (temp_result == DialogResult.Yes)
                                goto REDO;
                            if (temp_result == DialogResult.Cancel)
                            {
                                strError = "放弃装载 RFID 读者卡内容";
                                return 0;
                            }
                            if (saving == false)
                                MessageBox.Show(this, "警告：您刚装入了一个可疑的读者卡，极有可能不是读者卡而是图书 RFID 标签。待会儿保存读者卡内容的时候，有可能会张冠李戴覆盖了它。保存读者卡内容前，请务必反复仔细检查");
                        }
                    }

                    if (auto_close_dialog == false
                        // && string.IsNullOrEmpty(auto_select_pii) == false
                        && dialog.SelectedPII != auto_select_pii
                        && string.IsNullOrEmpty(dialog.SelectedPII) == false
                        )
                    {
                        string message = $"您所选择的读者卡其 PII 为 '{dialog.SelectedPII}'，和期待的 '{auto_select_pii}' 不吻合。请小心检查是否正确。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 读者卡;\r\n[否]将这一种不吻合的 RFID 读者卡装载进来\r\n[取消]放弃装载";
                        if (saving)
                            message = $"您所选择的读者卡其 PII 为 '{dialog.SelectedPII}'，和期待的 '{auto_select_pii}' 不吻合。请小心检查是否正确。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 读者卡;\r\n[否]将信息覆盖保存到这一种不吻合的 RFID 读者卡中(危险)\r\n[取消]放弃保存";

                        DialogResult temp_result = MessageBox.Show(this,
        message,
        "RfidPatronCardDialog",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Yes)
                            goto REDO;
                        if (temp_result == DialogResult.Cancel)
                        {
                            strError = "放弃装载 RFID 读者卡内容";
                            return 0;
                        }
                        if (saving == false)
                            MessageBox.Show(this, "警告：您刚装入了一个可疑的读者卡，极有可能不是当前读者记录对应的读者卡。待会儿保存读者卡内容的时候，有可能会张冠李戴覆盖了它。保存读者卡内容前，请务必反复仔细检查");
                    }

                    var tag_info = dialog.SelectedTag.TagInfo;
                    _tagExisting = dialog.SelectedTag;

                    var chip = LogicChipItem.FromTagInfo(tag_info);
                    this.chipEditor_existing.LogicChipItem = chip;

                    if (adjust_right)
                    {
                        int nRet = Merge(this.chipEditor_existing.LogicChipItem,
        this.chipEditor_editing.LogicChipItem,
        out strError);
                        if (nRet == -1)
                            return -1;

                        // 让右侧编辑器感受到 readonly 和 text 的变化
                        var save = this.chipEditor_editing.LogicChipItem;
                        this.chipEditor_editing.LogicChipItem = null;
                        this.chipEditor_editing.LogicChipItem = save;
                    }

                    return 1;
                }
            }
            catch (Exception ex)
            {
                this.chipEditor_existing.LogicChipItem = null;
                strError = "出现异常: " + ex.Message;
                return -1;
            }
        }

        static bool IsPatronUsage(LogicChipItem chip)
        {
            var typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage);
            if (typeOfUsage == null || string.IsNullOrEmpty(typeOfUsage.Text))
                return false;
            if (typeOfUsage.Text.StartsWith("8"))
                return true;
            return false;
        }

        // 把新旧芯片内容合并。即，新芯片中不应修改旧芯片中已经锁定的元素
        public static int Merge(LogicChipItem old_chip,
            LogicChipItem new_chip,
            out string strError)
        {
            strError = "";

            // 首先检查 typeOfUsage 是否为 8X
            if (old_chip.IsBlank() == false && IsPatronUsage(old_chip) == false)
            {
                strError = "当前 RFID 标签是图书类型，不是读者卡类型。请小心不要弄混这两种类型";
                return -1;
            }

            List<string> errors = new List<string>();
            // 检查一遍
            foreach (Element element in old_chip.Elements)
            {
                if (element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(element.OID);
                if (new_element != null)
                {
                    if (new_element.Text != element.Text)
                        errors.Add($"当前读者卡中元素 {element.OID} 已经被锁定，无法进行内容合并(从 '{element.Text}' 修改为 '{new_element.Text}')。");
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                strError += "\r\n\r\n强烈建议废弃此读者卡，启用一个新(空白)读者卡";
                return -1;
            }

            foreach (Element element in old_chip.Elements)
            {
                if (element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(element.OID);
                if (new_element != null)
                {
                    // 修改新元素
                    int index = new_chip.Elements.IndexOf(new_element);
                    Debug.Assert(index != -1);
                    new_chip.Elements.RemoveAt(index);
                    new_chip.Elements.Insert(index, element.Clone());
                }
            }

            return 0;
        }

        int SaveNewChip(out string strError)
        {
            strError = "";

#if OLD_CODE
            RfidChannel channel = StartRfidChannel(
Program.MainForm.RfidCenterUrl,
out strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                return -1;
            }
#endif
            try
            {
                TagInfo new_tag_info = null;
                if (this.chipEditor_editing.LogicChipItem != null)
                {
                    if (_tagExisting.Protocol == InventoryInfo.ISO15693)
                    {
                        new_tag_info = LogicChipItem.ToTagInfo(
                        _tagExisting.TagInfo,
                        this.chipEditor_editing.LogicChipItem);
                    }
                    else if (_tagExisting.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        new_tag_info = RfidToolForm.BuildWritingTagInfo(_tagExisting.TagInfo,
                            this.chipEditor_editing.LogicChipItem,
                            false,   // 这是读者卡，EAS 应该为 false
                            Program.MainForm.UhfDataFormat, // gb/gxlm/auto
                            (initial_format) =>
                            {
                                throw new Exception("意外触发格式选择回调函数");
                                // 如果是空白标签，需要弹出对话框提醒选择格式
                            },
                            (new_format, old_format) =>
                            {
                                string warning = $"警告：即将用{RfidToolForm.GetUhfFormatCaption(new_format)}格式覆盖原有{RfidToolForm.GetUhfFormatCaption(old_format)}格式";
                                DialogResult dialog_result = MessageBox.Show(this,
        $"{warning}\r\n\r\n确实要覆盖？",
        $"RfidPatronCardDialog",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                                if (dialog_result == DialogResult.Yes)
                                    return true;
                                return false;
                            },
                            true);
                    }
                    else
                    {
                        strError = $"无法识别的 RFID 格式 '{_tagExisting.Protocol}'";
                        return -1;
                    }
                }
                else
                    new_tag_info = _tagExisting.TagInfo.Clone();
#if OLD_CODE
                NormalResult result = channel.Object.WriteTagInfo(
                    _tagExisting.ReaderName,
                    _tagExisting.TagInfo,
                    new_tag_info);
#else
                NormalResult result = RfidManager.WriteTagInfo(
    _tagExisting.ReaderName,
    _tagExisting.TagInfo,
    new_tag_info);
                RfidTagList.ClearTagTable(_tagExisting.UID);
                // 2023/10/31
                if (_tagExisting.Protocol == InventoryInfo.ISO18000P6C)
                    RfidTagList.ClearTagTable(new_tag_info.UID);
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo + $" errorCode={result.ErrorCode}";
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "SaveNewChip() 出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
#if OLD_CODE
                EndRfidChannel(channel);
#endif
            }
        }

        int LoadChipByUID(
            string reader_name,
            string uid,
            uint antenna_id,
            out TagInfo tag_info,
            out string strError)
        {
            strError = "";
            tag_info = null;

#if OLD_CODE
            RfidChannel channel = StartRfidChannel(
Program.MainForm.RfidCenterUrl,
out strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                return -1;
            }
#endif
            try
            {
#if OLD_CODE
                var result = channel.Object.GetTagInfo(
                    reader_name,
                    uid);
#else
                var result = RfidManager.GetTagInfo(
    reader_name,
    uid,
    antenna_id);
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                tag_info = result.TagInfo;
                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetTagInfo() 出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
#if OLD_CODE
                EndRfidChannel(channel);
#endif
            }
        }

        private void RfidPatronCardDialog_Load(object sender, EventArgs e)
        {
            SetTitle();
        }
    }
}
