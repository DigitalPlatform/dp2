using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static dp2SSL.LibraryChannelUtil;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.WPF;
using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// WriteTagWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WriteTagWindow : Window
    {
        // 要执行的任务信息
        public WriteTagTask TaskInfo { get; set; }

        public bool Finished { get; set; }

        public WriteTagWindow()
        {
            InitializeComponent();

            this.booksControl.SetSource(_entities);

            this.Loaded += WriteTagWindow_Loaded;
            this.Unloaded += WriteTagWindow_Unloaded;
        }

        public string Comment
        {
            get
            {
                return this.comment.Text;
            }
            set
            {
                this.comment.Text = value;
            }
        }

        public string TitleText
        {
            get
            {
                return this.title.Text;
            }
            set
            {
                this.title.Text = value;
            }
        }

        private void WriteTagWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            App.PatronTagChanged -= App_PatronTagChanged;
        }

        private void WriteTagWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.TaskInfo != null)
                this.Comment = $"准备写入 RFID 标签。PII={this.TaskInfo.PII}";

            this.booksControl.EmptyComment = "请在读写器上放空白标签 ...";

            App.PatronTagChanged += App_PatronTagChanged;
            _ = InitialEntities();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void App_PatronTagChanged(object sender, NewTagChangedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            // 重置活跃时钟
            PageMenu.MenuPage.ResetActivityTimer();

            /*
            // 在读者证读卡器上扫 ISO15693 的标签可以查看图书内容
            {
                if (e.AddTags?.Count > 0
                    || e.UpdateTags?.Count > 0
                    || e.RemoveTags?.Count > 0)
                    DetectPatron();
            }
            */
            await ChangeEntitiesAsync((BaseChannel<IRfid>)sender, e);
        }

        EntityCollection _entities = new EntityCollection();

        async Task InitialEntities()
        {
            List<Entity> update_entities = new List<Entity>();
            foreach (var tag in ShelfData.PatronTagList.Tags)
            {
                var entity = _entities.Add(tag);
                update_entities.Add(entity);
            }

            if (update_entities.Count > 0)
            {
                BaseChannel<IRfid> channel = RfidManager.GetChannel();
                try
                {
                    await FillBookFieldsAsync(channel, update_entities);
                }
                finally
                {
                    RfidManager.ReturnChannel(channel);
                }
            }

            await TryWriteTagAsync(update_entities,
                this.TaskInfo);
        }

        // 跟随事件动态更新列表
        async Task ChangeEntitiesAsync(BaseChannel<IRfid> channel,
            NewTagChangedEventArgs e)
        {
            if (booksControl.Visibility != Visibility.Visible)
                return;

            bool changed = false;
            List<Entity> update_entities = new List<Entity>();
            App.Invoke(new Action(() =>
            {
                if (e.AddTags != null)
                    foreach (var tag in e.AddTags)
                    {
                        var entity = _entities.Add(tag);
                        update_entities.Add(entity);
                    }
                if (e.RemoveTags != null)
                    foreach (var tag in e.RemoveTags)
                    {
                        _entities.Remove(tag.OneTag.UID);
                        changed = true;
                    }
                if (e.UpdateTags != null)
                    foreach (var tag in e.UpdateTags)
                    {
                        var entity = _entities.Update(tag);
                        if (entity != null)
                            update_entities.Add(entity);
                    }
            }));

            if (update_entities.Count > 0)
            {
                await FillBookFieldsAsync(channel, update_entities);
            }
            else if (changed)
            {
                // 修改 borrowable
                booksControl.SetBorrowable();
            }

            if (update_entities.Count > 0)
                changed = true;

            await TryWriteTagAsync(update_entities,
    this.TaskInfo);
        }

        // 第二阶段：填充图书信息的 PII 和 Title 字段
        async Task FillBookFieldsAsync(BaseChannel<IRfid> channel,
            List<Entity> entities)
        {
#if NO
            RfidChannel channel = RFID.StartRfidChannel(App.RfidUrl,
out string strError);
            if (channel == null)
                throw new Exception(strError);
#endif
            try
            {
                foreach (Entity entity in entities)
                {
                    /*
                    if (_cancel == null
                        || _cancel.IsCancellationRequested)
                        return;
                        */
                    if (entity.FillFinished == true)
                        continue;

                    //if (string.IsNullOrEmpty(entity.Error) == false)
                    //    continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        if (entity.TagInfo == null)
                            continue;

                        Debug.Assert(entity.TagInfo != null);

                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
                        string pii = chip.FindElement(ElementOID.PII)?.Text;
                        entity.PII = GetCaption(pii);

                        // 2021/4/2
                        entity.OI = chip.FindElement(ElementOID.OI)?.Text;
                        entity.AOI = chip.FindElement(ElementOID.AOI)?.Text;
                    }

                    bool clearError = true;

                    // 获得 Title
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        var waiting = entity.Waiting;
                        entity.Waiting = true;
                        try
                        {
                            GetEntityDataResult result = null;
                            if (App.Protocol == "sip")
                                result = await SipChannelUtil.GetEntityDataAsync(entity.PII,
                                    entity.GetOiOrAoi(),
                                    "network");
                            else
                            {
                                // 2021/4/15
                                var strict = ChargingData.GetBookInstitutionStrict();
                                if (strict)
                                {
                                    string oi = entity.GetOiOrAoi();
                                    if (string.IsNullOrEmpty(oi))
                                    {
                                        entity.SetError("标签中没有机构代码，被拒绝使用");
                                        clearError = false;
                                        goto CONTINUE;
                                    }
                                }
                                result = await LibraryChannelUtil.GetEntityDataAsync(entity.GetOiPii(strict), "network"); // 2021/4/2 改为严格模式 OI_PII
                            }

                            if (result.Value == -1)
                            {
                                entity.SetError(result.ErrorInfo);
                                clearError = false;
                                goto CONTINUE;
                            }

                            entity.Title = GetCaption(result.Title);
                            entity.SetData(result.ItemRecPath,
                                result.ItemXml,
                                DateTime.Now);

                            // 2020/7/3
                            // 获得册记录阶段出错，但获得书目摘要成功
                            if (string.IsNullOrEmpty(result.ErrorCode) == false)
                            {
                                entity.SetError(result.ErrorInfo);
                                clearError = false;
                            }
                        }
                        finally
                        {
                            entity.Waiting = waiting;
                        }
                    }

                CONTINUE:
                    if (clearError == true)
                        entity.SetError(null);
                    entity.FillFinished = true;
                    // 2020/9/10
                    entity.Waiting = false;
                }

                booksControl.SetBorrowable();
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"FillBookFields() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                SetGlobalError("current", $"FillBookFields() 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
            }
        }

        public static string GetCaption(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";

            return text;
        }

        // 设置全局区域错误字符串
        void SetGlobalError(string type, string error)
        {
            App.SetError(type, error);
        }

        class FindBlankTagResult : NormalResult
        {
            public Entity ResultEntity { get; set; }
        }

        // 寻找唯一的空标签。如果出现了，而且是唯一一个，则自动向里写入内容
        async Task<FindBlankTagResult> FindBlankTagAsync(
            List<Entity> entities,
            WriteTagTask task_info)
        {
            List<Entity> blank_entities = new List<Entity>();
            List<Entity> pii_entities = new List<Entity>();
            foreach (var entity in entities)
            {
                if (IsBlank(entity))
                    blank_entities.Add(entity);
                if (entity.PII == task_info.PII && entity.GetOiOrAoi() == task_info.OI)
                    pii_entities.Add(entity);
            }

            // 如果空白标签正好是一个
            if (pii_entities.Count == 0 && blank_entities.Count == 1)
                return new FindBlankTagResult { ResultEntity = blank_entities[0] };

            // 如果 PII 对得上的正好是一个
            if (blank_entities.Count == 0 && pii_entities.Count == 1)
                return new FindBlankTagResult { ResultEntity = pii_entities[0] };

            return new FindBlankTagResult();
        }

        async Task<NormalResult> TryWriteTagAsync(List<Entity> entities,
            WriteTagTask task_info)
        {
            if (this.Finished)
                return new NormalResult { Value = 0 };

            var result = await FindBlankTagAsync(entities, task_info);
            if (result.Value == -1)
                return new NormalResult { Value = 0 };

            if (result.ResultEntity != null)
            {
                // 写入
                var chip = BuildChip(task_info);
                int nRet = SaveNewChip(result.ResultEntity.ReaderName,
                    result.ResultEntity.TagInfo,
                    chip,
                    out TagInfo new_tag_info,
                    out string strError);
                if (nRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                // 语音播报成功，自动关闭窗口
                this.Finished = true;
                App.CurrentApp.SpeakSequence($"写入完成");
                App.Invoke(new Action(() =>
                {
                    this.Comment = $"写入完成。PII={result.ResultEntity.PII}, UID={new_tag_info.UID}";
                    this.Background = new SolidColorBrush(Colors.DarkGreen);
                }));

                // 3 秒以后自动关闭对话框
                _ = Task.Run(async ()=> {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    App.Invoke(new Action(() =>
                    {
                        this.Close();
                    }));
                });
                return new NormalResult { Value = 1 };
            }

            return new NormalResult { Value = 0 };
        }

        public static LogicChip BuildChip(WriteTagTask task)
        {
            LogicChip result = new LogicChip();

            /*
            result.AFI = LogicChipItem.DefaultBookAFI;
            result.DSFID = LogicChipItem.DefaultDSFID;
            result.EAS = LogicChipItem.DefaultBookEAS;
            */

            // barcode --> PII
            result.NewElement(ElementOID.PII, task.PII);

            if (IsIsil(task.OI))
                result.NewElement(ElementOID.OwnerInstitution, task.OI);
            else
                result.NewElement(ElementOID.AlternativeOwnerInstitution, task.OI);

            // TypeOfUsage?
            // (十六进制两位数字)
            // 10 一般流通馆藏
            // 20 非流通馆藏。保存本库? 加工中?
            // 70 被剔旧的馆藏。和 state 元素应该有某种对应关系，比如“注销”
            {
                string typeOfUsage = "10";

                result.NewElement(ElementOID.TypeOfUsage, typeOfUsage);
            }

            // AccessNo --> ShelfLocation
            // 注意去掉 {ns} 部分
            result.NewElement(ElementOID.ShelfLocation,
                task.AccessNo);

            return result;
        }

        static bool IsIsil(string text)
        {
            // 所属机构ISIL由拉丁字母、阿拉伯数字（0-9），分隔符（-/:)组成，总长度不超过16个字符。
            if (DigitalPlatform.RFID.Compact.CheckIsil(text, false) == false)
                return false;

            string strError = VerifyOI(text);
            if (strError != null)
                return false;
            return true;
        }

        int SaveNewChip(
            string readerName,
            TagInfo tagInfo,
            LogicChip _right,
    out TagInfo new_tag_info,
    out string strError)
        {
            new_tag_info = null;
            strError = "";

            try
            {
                new_tag_info = ToTagInfo(
                    tagInfo,
                    _right);
                NormalResult result = RfidManager.WriteTagInfo(
    readerName,
    tagInfo,
    new_tag_info);
                TagList.ClearTagTable(new_tag_info.UID);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "SaveNewChip() 出现异常: " + ex.Message;
                return -1;
            }
        }

        public static TagInfo ToTagInfo(TagInfo existing,
    LogicChip chip)
        {
            TagInfo new_tag_info = existing.Clone();
            new_tag_info.Bytes = chip.GetBytes(
                (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                (int)new_tag_info.BlockSize,
                LogicChip.GetBytesStyle.None,
                out string block_map);
            new_tag_info.LockStatus = block_map;

            new_tag_info.AFI = LogicChip.DefaultBookAFI;
            new_tag_info.DSFID = LogicChip.DefaultDSFID;
            new_tag_info.EAS = LogicChip.DefaultBookEAS;
            return new_tag_info;
        }

        static bool IsBlank(Entity entity)
        {
            if (entity.TagInfo == null)
                return false;
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
            return chip.IsBlank();
        }

        #region

        /*
OI的校验，总长度不超过16位。
2位国家代码前缀-6位中国行政区划代码-1位图书馆类型代码-图书馆自定义码（最长4位）
 * */
        public static string VerifyOI(string oi)
        {
            if (string.IsNullOrEmpty(oi))
                return "机构代码不应为空";

            if (oi.Length > 16)
                return $"机构代码 '{oi}' 不合法: 总长度不应超过 16 字符";

            var parts = oi.Split(new char[] { '-' });
            if (parts.Length != 4)
                return $"机构代码 '{oi}' 不合法: 应为 - 间隔的四个部分形态";
            string country = parts[0];
            if (country != "CN")
                return $"机构代码 '{oi}' 不合法: 第一部分国家代码 '{country}' 不正确，应为 'CN'";
            string region = parts[1];
            if (region.Length != 6
                || StringUtil.IsPureNumber(region) == false)
                return $"机构代码 '{oi}' 不合法: 第二部分行政区代码 '{region}' 不正确，应为 6 位数字";
            string type = parts[2];
            if (type.Length != 1
    || VerifyType(type[0]) == false)
                return $"机构代码 '{oi}' 不合法: 第三部分图书馆类型代码 '{type}' 不正确，应为 1 位字符(取值范围为 1-9,A-F)";
            string custom = parts[3];
            if (custom.Length < 1 || custom.Length > 4
                || IsLetterOrDigit(custom) == false)
                return $"机构代码 '{oi}' 不合法: 第四部分图书馆自定义码 '{custom}' 不正确，应为 1-4 位数字或者大写字母";

            return null;
        }

        static bool VerifyType(char ch)
        {
            if (ch >= '1' && ch <= '9')
                return true;
            if (ch >= 'A' && ch <= 'F')
                return true;
            return false;
        }

        static bool IsLetterOrDigit(string text)
        {
            foreach (char ch in text)
            {
                if (char.IsLetter(ch) && char.IsUpper(ch) == false)
                    return false;
                if (char.IsLetterOrDigit(ch) == false)
                    return false;
            }

            return true;
        }


        #endregion
    }

    // 任务信息
    public class WriteTagTask
    {
        // PII
        public string PII { get; set; }
        // 机构代码
        public string OI { get; set; }
        // 索取号
        public string AccessNo { get; set; }

        // 任务完成时是否自动关闭对话框
        public bool AutoCloseDialog { get; set; }
    }
}
