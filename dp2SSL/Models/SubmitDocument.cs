using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;
using Microsoft.VisualStudio.Threading;
using static System.Resources.ResXFileRef;

namespace dp2SSL
{
    // 用于显示借书还书操作结果信息的可视化文档
    public class SubmitDocument : FlowDocument
    {

        public double BaseFontSize { get; set; }
        public string BuildStyle { get; set; }
        // public List<string> DoorNames { get; set; }

        internal List<ActionInfo> _actions = new List<ActionInfo>();

        // 超额语音提醒记载表
        // 姓名 --> 最后一次提醒时间
        Hashtable _overflowSpeakTable = new Hashtable();

        bool HasOverflowSpeaked(string name, TimeSpan period)
        {
            lock (_overflowSpeakTable.SyncRoot)
            {
                // 防止内容太多
                if (_overflowSpeakTable.Count > 1000)
                    _overflowSpeakTable.Clear();

                bool existing = false;
                if (_overflowSpeakTable.ContainsKey(name))
                {
                    DateTime time = (DateTime)_overflowSpeakTable[name];
                    if (DateTime.Now - time < period)
                        existing = true;
                }
                _overflowSpeakTable[name] = DateTime.Now;
                return existing;
            }
        }

#if REMOVED

        // localFilePath --> Image Control
        static Hashtable _imageControlTable = new Hashtable();

        static Image GetImageControl(string localFilePath)
        {
            lock (_imageControlTable.SyncRoot)
            {
                if (_imageControlTable.ContainsKey(localFilePath))
                    return _imageControlTable[localFilePath] as Image;
            }

            var convertor = new StringToBitmapConverter();

            var image = new Image();
            image.Width = 120;
            image.Margin = new Thickness(0, 4, 0, 6);
            image.Source = convertor.Convert(localFilePath, typeof(ImageSource), null, null) as ImageSource;
            lock (_imageControlTable.SyncRoot)
            {
                _imageControlTable.Add(localFilePath, image);
            }
            return image;
        }

#endif

#if REMOVED
        static Image GetImageControl(string localFilePath)
        {
            if (string.IsNullOrEmpty(localFilePath))
                throw new ArgumentException($"{nameof(localFilePath)} 不应为空");

            var convertor = new StringToBitmapConverter();

            var image = new Image();
            image.Width = 120;
            image.Margin = new Thickness(0, 4, 0, 6);
            image.Source = convertor.Convert(localFilePath, typeof(ImageSource), null, null) as ImageSource;
            return image;
        }
#endif

#if REMOVED
        static string GetCoverImageLocalPath(Entity entity)
        {
            if (string.IsNullOrEmpty(entity.CoverImageLocalPath) == false)
                return entity.CoverImageLocalPath;

            var uii = entity.GetOiPii();

            lock (_uiiTable.SyncRoot)
            {
                if (_uiiTable.ContainsKey(uii))
                    return _uiiTable[uii] as string;
            }

            string get_style = "";
            if (ShelfData.LibraryNetworkCondition != "OK")
                get_style += ",localGetEntityInfo";
            if (App.DisplayCoverImage)
                get_style += ",coverImage";
            EntityCollection collection = new EntityCollection { entity };
            _ = Task.Run(async () =>
            {
                var result = await ShelfData.FillBookFieldsAsync(collection, default, get_style);
                if (string.IsNullOrEmpty(entity.CoverImageLocalPath) == false)
                {
                    lock (_uiiTable.SyncRoot)
                    {
                        _uiiTable[uii] = entity.CoverImageLocalPath;
                    }
                }
            });
            return null;
        }
#endif

        // 刷新显示
        // 把 actions 中的对象的状态变化更新到当前文档中
        // TODO: 一个办法是整个 Paragraph 替换。一个办法只替换里面的部分 InLine 对象
        // parameters:
        //      actions 发生了状态改变的 action。也就是被执行同步了的 action
        //      skipped 被跳过了(没有处理的) action
        public void Refresh(List<ActionInfo> actions)
        {
            // 更新 _actions 中的对象
            int count = 0;
            foreach (var action in actions)
            {
                var old_action = _actions.Where(o => o.ID == action.ID).FirstOrDefault();
                if (old_action != null)
                {
                    int index = _actions.IndexOf(old_action);
                    Debug.Assert(index != -1, "");
                    if (index != -1)
                    {
                        _actions.RemoveAt(index);
                        _actions.Insert(index, action);
                        count++;
                    }
                }
            }

            if (count == 0)
                return;

            {
                // 检查超额图书
                List<string> overflow_titles = new List<string>();
                _actions.ForEach(item =>
                {
                    if (item.Action == "borrow" && item.SyncErrorCode == "overflow")
                        overflow_titles.Add($"{ShortTitle(item.Entity.Title)} [{ShelfData.GetPiiString(item.Entity)}]");
                });

                // 显示超额的信息
                if (overflow_titles.Count > 0)
                {
                    // 定位 Paragraph
                    var block = this.Blocks.Where(o =>
                    {
                        if (!(o.Tag is string id))
                            return false;
                        return id == OVERFLOW_ID;
                    }).FirstOrDefault();

                    // block 不应为 null。替代方法：
                    if (block == null)
                    {
                        // TODO: 在适当位置插入标志段落
                        throw new Exception("#overflow 标志段落没有找到");
                    }

                    var p = BuildOverflowParagraph(overflow_titles);
                    this.Blocks.InsertBefore(block, p);
                    this.Blocks.Remove(block);

                    // 获得人名
                    List<string> names = new List<string>();
                    {
                        actions.ForEach((o) =>
                        {
                            if (o.Operator != null)
                                names.Add(string.IsNullOrEmpty(o.Operator.PatronNameMasked) ? o.Operator.PatronBarcodeMasked : o.Operator.PatronNameMasked);
                        });
                        StringUtil.RemoveDupNoSort(ref names);
                    }

                    // 语音提醒
                    // 针对同一个人，短时间内密集提醒要注意避免
                    if (HasOverflowSpeaked(StringUtil.MakePathList(names), TimeSpan.FromSeconds(30)) == false)
                    {
                        var speak_count = ShelfData.GetOverdueSpeakCount();
                        for (int i = 0; i < speak_count; i++)
                        {
                            App.CurrentApp.SpeakSequence("警告：借书超额");
                        }
                    }
                }
            }

            foreach (var action in actions)
            {
                // 定位 Paragraph
                var block = this.Blocks.Where(o =>
                    {
                        if (!(o.Tag is ParagraphInfo info))
                            return false;
                        return info.Action.ID == action.ID;
                    })
                    .FirstOrDefault();
                if (block == null)
                    continue;

                // 替换 Paragraph
                {
                    if (!(block.Tag is ParagraphInfo old_info))
                        continue;

                    var old_image = GetImage(block as Paragraph);

                    var new_block = BuildParagraph(action,
                        old_info.Index,
                        this.BaseFontSize,
                        this.BuildStyle,
                        old_image);
                    this.Blocks.InsertBefore(block, new_block);
                    this.Blocks.Remove(block);
                }
            }
        }

#if REMOVED
        static async Task _displayImagesAsync(List<ImageAndEntity> images,
            CancellationToken token)
        {
            foreach (var item in images)
            {
                if (token.IsCancellationRequested)
                    return;

                var entity = item.Entity;
                var image = item.Image;

                string localFilePath = entity.CoverImageLocalPath;

                if (string.IsNullOrEmpty(localFilePath) == false
                    && File.Exists(localFilePath))
                {
                    // var convertor = new StringToBitmapConverter();
                    image.Source = GetBitmap(localFilePath);
                    // convertor.Convert(localFilePath, typeof(ImageSource), null, null) as ImageSource;
                    image.Visibility = Visibility.Visible;
                    continue;
                }

                if (token.IsCancellationRequested)
                    return;

                EntityCollection collection = new EntityCollection { entity };

                string get_style = "";
                if (ShelfData.LibraryNetworkCondition != "OK")
                    get_style += ",localGetEntityInfo";
                if (App.DisplayCoverImage)
                    get_style += ",coverImage";

                var result = await ShelfData.FillBookFieldsAsync(collection, token, get_style);

                if (token.IsCancellationRequested)
                    return;

                if (string.IsNullOrEmpty(entity.CoverImageLocalPath) == false)
                {
                    if (File.Exists(entity.CoverImageLocalPath) == false)
                        throw new Exception($"文件 '{entity.CoverImageLocalPath}' 不存在");

                    App.Invoke(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        image.Source = GetBitmap(entity.CoverImageLocalPath);
                        image.Visibility = Visibility.Visible;
                    });
                }
            }
        }
#endif

        static Image GetImage(Paragraph paragraph)
        {
            if (paragraph == null)
                return null;

            var container = paragraph.Inlines.Where(o => o.Name == "cover_image").FirstOrDefault() as InlineUIContainer;
            // .ToList().Cast<InlineUIContainer>();
            if (container == null)
                return null;

            var result = container.Child as Image;
            container.Child = null;

            return result;
        }


        static void MaskDeleteAllImage(Paragraph paragraph)
        {
            if (paragraph == null)
                return;

            var containers = paragraph.Inlines.Where(o => o is InlineUIContainer).ToList().Cast<InlineUIContainer>();
            var images = containers.Select(o => o.Child).ToList().Cast<Image>();
            foreach (var image in images)
            {
                var cancel = image.Tag as CancellationTokenSource;
                if (cancel != null)
                    cancel.Cancel();
            }
        }

        const string OVERFLOW_ID = "#overflow";

        // 构造超额图书列表
        Paragraph BuildOverflowParagraph(List<string> titles)
        {
            var p = new Paragraph();
            p.FontFamily = new FontFamily("微软雅黑");
            p.FontSize = this.BaseFontSize * 1.8;
            // p.FontStyle = FontStyles.Italic;
            p.TextAlignment = TextAlignment.Left;
            p.Foreground = Brushes.White;
            p.Background = Brushes.DarkGoldenrod;
            // p.LineHeight = 18;
            p.TextIndent = 0;   // -20;
            p.Margin = new Thickness(10, this.BaseFontSize * 2.0, 0, this.BaseFontSize * 2.0);
            p.Padding = new Thickness(this.BaseFontSize * 1.4);
            p.Tag = OVERFLOW_ID;

            StringBuilder text = new StringBuilder();
            text.Append($"警告：您取书已经超额了。请将下列 {titles.Count} 册图书放回书柜:");
            int i = 1;
            foreach (string title in titles)
            {
                text.Append($"\r\n{i}) {title}");
                i++;
            }
            p.Inlines.Add(new Run(text.ToString()));
            return p;
        }

        public static string ShortTitle(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            int pos = text.IndexOf(". --");
            if (pos == -1)
                return text;
            return text.Substring(0, pos).Trim();
        }

        public static SubmitDocument Build(List<ActionInfo> actions,
            double baseFontSize,
            string style)
        {
            SubmitDocument doc = new SubmitDocument();
            doc.BaseFontSize = baseFontSize;
            doc.BuildStyle = style;

            // 保存起来
            doc._actions.Clear();
            doc._actions.AddRange(actions);

            bool display_transfer = StringUtil.IsInList("transfer", style);

            // 第一部分，总结信息
            List<string> names = new List<string>();
            {
                actions.ForEach((o) =>
                {
                    if (o.Operator != null)
                        names.Add(string.IsNullOrEmpty(o.Operator.PatronNameMasked) ? o.Operator.PatronBarcodeMasked : o.Operator.PatronNameMasked);
                });
                StringUtil.RemoveDupNoSort(ref names);
            }

            int return_count = actions.FindAll((o) => { return o.Action == "return"; }).Count;
            int borrow_count = actions.FindAll((o) => { return o.Action == "borrow"; }).Count;

            // 修改了 currentLocation 并且向内转移的数量
            int change_currentLocation_count = actions
                .FindAll((o) =>
                {
                    return o.Action.StartsWith("transfer")
      && string.IsNullOrEmpty(o.CurrentShelfNo) == false
      && o.TransferDirection == "in";
                })
                .Count;

            // 修改了 location 的数量。这个意味着发生了调拨
            int change_location_count = actions
                .FindAll(o => { return o.Action.StartsWith("transfer") && string.IsNullOrEmpty(o.Location) == false; })
                .Count;

            // (工作人员)普通下架。特点是去向不明，但至少知道这些图书是离开书柜了
            int transferout_count = actions
    .FindAll(o => { return o.Action.StartsWith("transfer") && o.TransferDirection == "out"; })
    .Count;

            // 总结一下涉及到的门
            var door_names = ShelfData.GetDoorName(actions);

            /*
            int succeed_count = actions.FindAll((o) => { return o.ResultType == "succeed" || string.IsNullOrEmpty(o.ResultType); }).Count;
            int error_count = items.FindAll((o) => { return o.ResultType == "error"; }).Count;
            int warning_count = items.FindAll((o) => { return o.ResultType == "warning"; }).Count;
            int information_count = 0;
            if (display_transfer == false)
                information_count = items.FindAll((o) => { return o.ResultType == "information" && o.Operation != "transfer"; }).Count;
            else
                information_count = items.FindAll((o) => { return o.ResultType == "information"; }).Count;

            // 检查超额图书
            List<string> overflow_titles = new List<string>();
            items.ForEach(item =>
            {
                if (item.Operation == "borrow" && item.ErrorCode == "overflow")
                    overflow_titles.Add($"{ShortTitle(item.Entity.Title)} [{item.Entity.PII}]");
            });
            */
            // 提醒断网模式
            if (ShelfData.LibraryNetworkCondition == "Bad")
            {
                var p = new Paragraph();
                p.FontFamily = new FontFamily("微软雅黑");
                p.FontSize = baseFontSize;
                p.TextAlignment = TextAlignment.Center;
                // p.TextIndent = -20;
                p.Margin = new Thickness(0, 0, 0, baseFontSize/*18*/);
                doc.Blocks.Add(p);

                p.Inlines.Add(new Run
                {
                    Text = "温馨提示: 当前设备处于断网模式，读者通知消息暂时无法发送。待网络恢复后会自动补发",
                    Foreground = Brushes.Yellow,
                });
            }


            {
                var p = new Paragraph();
                p.FontFamily = new FontFamily("微软雅黑");
                p.FontSize = baseFontSize;
                p.TextAlignment = TextAlignment.Center;
                p.Foreground = Brushes.Gray;
                // p.TextIndent = -20;
                p.Margin = new Thickness(0, 0, 0, baseFontSize/*18*/);
                doc.Blocks.Add(p);

                if (borrow_count
                    + return_count
                    + change_currentLocation_count
                    + change_location_count
                    + transferout_count > 0)
                {
                    List<string> lines = new List<string>();
                    if (return_count > 0)
                        lines.Add($"还书 {return_count}");
                    if (borrow_count > 0)
                        lines.Add($"借书 {borrow_count}");

                    REDO:
                    if (display_transfer && change_currentLocation_count > 0)
                        lines.Add($"上架 {change_currentLocation_count}");
                    if (display_transfer && change_location_count > 0)
                        lines.Add($"调拨 {change_location_count}");
                    if (display_transfer && transferout_count > 0)
                        lines.Add($"下架 {transferout_count}");

                    if (lines.Count == 0 && display_transfer == false)
                    {
                        display_transfer = true;

                        // 修正 style，便于 Refresh() 时候使用新的 style
                        StringUtil.SetInList(ref style, "transfer", true);
                        doc.BuildStyle = style;
                        goto REDO;
                    }

                    p.Inlines.Add(new Run
                    {
                        Text = $"{StringUtil.MakePathList(names)}",
                        //Background = Brushes.DarkRed,
                        //Foreground = Brushes.White
                        FontFamily = new FontFamily("楷体"),
                        FontSize = baseFontSize * 5.5,  // 3.5,
                        // FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                    });

                    if (door_names.Count > 0)
                    {
                        p.Inlines.Add(new Run
                        {
                            Text = $"\r\n({StringUtil.MakePathList(door_names)})",
                            //Background = Brushes.DarkRed,
                            Foreground = Brushes.Green,
                            // FontFamily = new FontFamily("楷体"),
                            FontSize = baseFontSize * 2.0,    // 1.0
                            // FontWeight = FontWeights.Bold,
                        });
                    }

                    p.Inlines.Add(new Run
                    {
                        Text = $"\r\n{StringUtil.MakePathList(lines, ", ")}\r\n",
                        //Background = Brushes.DarkRed,
                        //Foreground = Brushes.White
                        FontSize = baseFontSize * 3.0,  // * 1.2,
                        Foreground = Brushes.White,
                    });

                    App.CurrentApp.SpeakSequence(StringUtil.MakePathList(lines, ", "));
                }

#if NO
                if (error_count > 0)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = $" 错误 {error_count} ",
                        Background = Brushes.DarkRed,
                        Foreground = Brushes.White
                    });
                }
                if (warning_count > 0)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = $" 警告 {warning_count} ",
                        Background = Brushes.DarkGoldenrod,
                        Foreground = Brushes.White
                    });
                }
                if (information_count > 0)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = $" 信息 {information_count} ",
                        Background = Brushes.Gray,
                        Foreground = Brushes.White
                    });
                }
                if (succeed_count > 0)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = $" 成功 {succeed_count} ",
                        Background = Brushes.DarkGreen,
                        Foreground = Brushes.White
                    });
                }
#endif
            }

#if NO
            // 第二部分，列出超额的信息
            if (overflow_titles.Count > 0)
            {
                var p = BuildOverflowParagraph(overflow_titles);
                doc.Blocks.Add(p);
            }
#endif
            // 检查超额图书
            List<string> overflow_titles = new List<string>();
            actions.ForEach(item =>
            {
                if (item.Action == "borrow" && item.SyncErrorCode == "overflow")
                    overflow_titles.Add($"{ShortTitle(item.Entity.Title)} [{ShelfData.GetPiiString(item.Entity)}]");
            });

            // 显示超额的信息
            if (overflow_titles.Count > 0)
            {
                var p = doc.BuildOverflowParagraph(overflow_titles);
                // p.Tag = "#overflow";
                doc.Blocks.Add(p);

                // 语音提醒
                var speak_count = ShelfData.GetOverdueSpeakCount();
                for (int i = 0; i < speak_count; i++)
                {
                    App.CurrentApp.SpeakSequence($"警告：借书超额 {overflow_titles.Count} 册");
                }
            }
            else
            {
                // 超额信息的占位符
                var p = new Paragraph();
                p.FontSize = 0.1;
                p.Margin = new Thickness();
                p.Padding = new Thickness();
                p.Tag = OVERFLOW_ID;    // "#overflow";
                doc.Blocks.Add(p);
            }

            // 第三部分，列出每一笔操作
            int index = 0;
            foreach (var action in actions)
            {
                var p = BuildParagraph(action,
                    index,
                    baseFontSize,
                    style,
                    null);
                if (p != null)
                {
                    doc.Blocks.Add(p);
                    index++;
                }
            }

#if NO
            // 构造提示语音
            List<string> speaks = new List<string>();
            if (overflow_titles.Count > 0)
            {
                speaks.Add($"警告：有 {overflow_titles.Count} 册图书超越许可册数，请放回书柜，谢谢");
            }

            if (speaks.Count == 0)
                speaks.Add("操作完成"); // TODO：可否增加姓名和借还册数？例如 王立文借书 5 册成功

            speak = StringUtil.MakePathList(speaks, "; ");
#endif

            return doc;
        }

        public class ParagraphInfo
        {
            public ActionInfo Action { get; set; }
            public int Index { get; set; }
        }

        public static Paragraph BuildParagraph(
            ActionInfo action,
            int index,
            double baseFontSize,
            string style,
            Image old_image)
        {
            // 是否显示 transfer (in) 条目。注意，即便 false, 也要显示 transfer (out) 条目的
            bool display_transfer = StringUtil.IsInList("transfer", style);
            if (action.Action.StartsWith("transfer") && action.TransferDirection == "in"
                && display_transfer == false)
                return null;

            var p = new Paragraph();
            p.FontFamily = new FontFamily("微软雅黑");
            p.FontSize = baseFontSize;
            // p.FontStyle = FontStyles.Italic;
            p.TextAlignment = TextAlignment.Left;
            p.Foreground = Brushes.Gray;
            // p.LineHeight = 18;
            p.TextIndent = -20;
            p.Margin = new Thickness(10, 0, 0, 8);  // 10,0,0,8
            p.Tag = new ParagraphInfo { Action = action, Index = index };   // 记忆下来后面刷新事项的时候可以用到

            // 序号
            p.Inlines.Add(new Run($"{(index + 1).ToString()}) "));

            Brush back = Brushes.Transparent;
            // 状态
            {
                // 等待动画
                if (string.IsNullOrEmpty(action.State))
                {
                    var image = new FontAwesome.WPF.ImageAwesome();
                    image.Icon = FontAwesome.WPF.FontAwesomeIcon.Spinner;
                    image.Spin = true;
                    image.SpinDuration = 5;
                    image.Height = baseFontSize * 2.0;
                    image.Foreground = Brushes.DarkGray;
                    var container = new InlineUIContainer(image);
                    container.Name = "image_id";
                    p.Inlines.Add(container);
                }
                else if (action.SyncErrorCode == "overflow")
                {
                    back = Brushes.DarkRed;
                    p.Inlines.Add(new Run
                    {
                        Text = " 超额 ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
                else if (action.State == "sync")
                {
                    back = Brushes.DarkGreen;
                    p.Inlines.Add(new Run
                    {
                        Text = " 成功 ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
                else if (action.SyncErrorCode == "skipped")
                {
                    back = Brushes.DeepSkyBlue;
                    p.Inlines.Add(new Run
                    {
                        Text = $" 暂时跳过同步 ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
                else if (action.State == "commerror" || action.State == "normalerror")
                {
                    if (ShelfData.LibraryNetworkCondition == "Bad")
                        back = Brushes.DeepSkyBlue;
                    else
                        back = Brushes.DarkRed;
                    p.Inlines.Add(new Run
                    {
                        Text = $" 同步失败({action.State}) ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
                else if (action.State == "dontsync")
                {
                    back = Brushes.DarkBlue;
                    p.Inlines.Add(new Run
                    {
                        Text = $" 不再同步 ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
                else
                {
                    back = Brushes.DarkRed;
                    p.Inlines.Add(new Run
                    {
                        Text = $" {action.State} ",
                        Background = back,
                        Foreground = Brushes.White
                    });
                }
            }

            // 转移方向
            if (action.Action.StartsWith("transfer")
                /*&& string.IsNullOrEmpty(action.TransferDirection) == false*/)
            {
                p.Inlines.Add(new Run
                {
                    Text = GetTransferDirCaption(action.TransferDirection, action.Location) + " ",
                    Foreground = Brushes.White
                });
            }
            else
            {
                // 操作名称
                p.Inlines.Add(new Run
                {
                    Text = GetOperationCaption(action.Action) + " ",
                    Foreground = Brushes.White
                });
            }

#if REMOVED
            // 获取 title 和 ImageUrl
            if (string.IsNullOrEmpty(action.Entity.Title)
                || (App.DisplayCoverImage && string.IsNullOrEmpty(action.Entity.CoverImageLocalPath) == true))
            {
                string get_style = "";
                if (ShelfData.LibraryNetworkCondition != "OK")
                    get_style += ",localGetEntityInfo";
                if (App.DisplayCoverImage)
                    get_style += ",coverImage";
                EntityCollection collection = new EntityCollection { action.Entity };
                var result = Task.Run(async () =>
                {
                    return await ShelfData.FillBookFieldsAsync(collection, default, get_style);
                }).Result;
            }
#endif

            string title = "";

            if (action.Entity != null)
            {
                title = MessageDocument.ShortTitle(action.Entity.Title);

                // TODO: 这里可以尝试用 LibraryChannelUtil.GetEntityDataAsync() 获得册和书目摘要信息，顺便可以进入本地缓存

                // 2020/5/6
                // 尝试从本地缓存中获取书目摘要
                if (string.IsNullOrEmpty(title))
                    title = LibraryChannelUtil.GetBiblioSummaryFromLocal(action.Entity.GetOiPii(true));
                if (string.IsNullOrEmpty(title))
                {
                    title = ShelfData.GetPiiString(action.Entity);
                }
                else
                {
                    // 2020/7/22
                    title = $"[{ShelfData.GetPiiString(action.Entity)}] {title}";
                }
            }
            else
                title = "(action.Entity 为空)";

            // 书目摘要
            if (string.IsNullOrEmpty(title) == false)
            {
                Run run = new Run(title);
                /*
                run.FontSize = 14;
                run.FontStyle = FontStyles.Normal;
                run.Background = Brushes.DarkRed;
                run.Foreground = Brushes.White;
                */

                p.Inlines.Add(run);
            }




            // 对于上架/下架来说，还要补充显示一些细节信息：location 去向；和 currentLocation 去向
            if (action.Action.StartsWith("transfer"))
            {
                List<string> details = new List<string>();
                if (string.IsNullOrEmpty(action.Location) == false)
                    details.Add($"调拨到:{action.Location}");
                if (string.IsNullOrEmpty(action.CurrentShelfNo) == false)
                    details.Add($"新架位:{action.CurrentShelfNo}");
                p.Inlines.Add(new Run
                {
                    Text = " " + StringUtil.MakePathList(details, " ") + " ",
                    Foreground = Brushes.Green
                });
            }

            // 错误码和错误信息
            if (string.IsNullOrEmpty(action.SyncErrorInfo) == false
                && (action.State != "sync" || action.SyncErrorCode == "overflow"))
            {
                p.Inlines.Add(new Run
                {
                    Text = "\r\n" + action.SyncErrorInfo,
                    Background = back,
                    Foreground = Brushes.White
                });
            }

            // 2024/2/1
            // 封面图片
            if (App.DisplayCoverImage
                && action.Entity != null)
            {
                //var path = GetCoverImageLocalPath(action.Entity);
                //if (string.IsNullOrEmpty(path) == false)
                {
                    p.Inlines.Add(new Run
                    {
                        Text = "\r\n",
                        Background = back,
                        Foreground = Brushes.White
                    });

#if REMOVED
                var convertor = new StringToBitmapConverter();

                var image = new System.Windows.Controls.Image();
                image.Width = 120;
                image.Margin = new Thickness(0, 4, 0, 6);
                image.Source = convertor.Convert(action.Entity.CoverImageLocalPath, typeof(ImageSource), null, null) as ImageSource;
#endif
                    if (old_image == null)
                    {
                        var image = new Image();
                        image.Width = 120;
                        image.Margin = new Thickness(0, 4, 0, 6);
                        image.Visibility = Visibility.Collapsed;

                        var container = new InlineUIContainer(image);
                        container.Name = "cover_image";
                        p.Inlines.Add(container);

                        BeginImageTask(image, action.Entity);
                    }
                    else
                    {
                        var container = new InlineUIContainer(old_image);
                        container.Name = "cover_image";
                        p.Inlines.Add(container);
                    }
                }
            }

            return p;
        }

        #region 封面图像显示

        public class ImageAndEntity
        {
            public Image Image { get; set; }
            public Entity Entity { get; set; }
        }

        /*
        // 已经处理过下载封面图片文件的的 UII 集合
        // UII --> localFilePath
        static Hashtable _uiiTable = new Hashtable();
        */

        // 令 BeginImageTask() 中的线程有序进行
        static AsyncSemaphore _imageTaskLimit = new AsyncSemaphore(1);

        // 启动一个获得图像并显示的任务
        static void BeginImageTask(Image image, Entity entity_param)
        {
            Entity entity = entity_param.Clone();

            // var uii = entity.GetOiPii();
            string localFilePath = entity.CoverImageLocalPath;
            /*
            if (string.IsNullOrEmpty(localFilePath))
            {
                lock (_uiiTable.SyncRoot)
                {
                    localFilePath = _uiiTable[uii] as string;
                }
            }
            */

            if (string.IsNullOrEmpty(localFilePath) == false
                && File.Exists(localFilePath))
            {
                App.Invoke(() =>
                {
                    // var convertor = new StringToBitmapConverter();
                    image.Source = GetBitmap(localFilePath);
                    // convertor.Convert(localFilePath, typeof(ImageSource), null, null) as ImageSource;
                    image.Visibility = Visibility.Visible;
                });
                return;
            }

            CancellationTokenSource cancel = new CancellationTokenSource();
            image.Tag = cancel;
            string get_style = "";
            if (ShelfData.LibraryNetworkCondition != "OK")
                get_style += ",localGetEntityInfo";
            if (App.DisplayCoverImage)
                get_style += ",coverImage";
            EntityCollection collection = new EntityCollection { entity };
            _ = Task.Run(async () =>
            {
                ShelfData.FillBookFieldsResult result = null;
                using (var releaser = await _imageTaskLimit.EnterAsync())
                {
                    result = await ShelfData.FillBookFieldsAsync(collection, cancel.Token, get_style);
                }

                if (string.IsNullOrEmpty(entity.CoverImageLocalPath) == false)
                {
                    /*
                    lock (_uiiTable.SyncRoot)
                    {
                        _uiiTable[uii] = entity.CoverImageLocalPath;
                    }
                    */
                    if (File.Exists(entity.CoverImageLocalPath) == false)
                    {
                        var error = $"BeginImageTask() 中文件 '{entity.CoverImageLocalPath}' 不存在。先前 ShelfData.FillBookFieldsAsync() 的返回值 result={result.ToString()}";
                        WpfClientInfo.WriteErrorLog(error);
                        return;
                    }

                    App.Invoke(() =>
                    {
                        if (cancel.Token.IsCancellationRequested)
                            return;

                        // var convertor = new StringToBitmapConverter();
                        image.Source = GetBitmap(entity.CoverImageLocalPath);
                        //convertor.Convert(entity.CoverImageLocalPath, typeof(ImageSource), null, null) as ImageSource;
                        image.Visibility = Visibility.Visible;
                    });
                }
                else
                {
                    // 图像文件本地路径并没有得到
                    int i = 0;
                    i++;
                }
            });
        }

        public static BitmapImage GetBitmap(string fileName,
            bool use_memory_stream = true)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            Stream s = null;
            bool succeed = false;
            try
            {
                if (use_memory_stream)
                {
                    byte[] bytes = null;
                    using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        int length = (int)stream.Length;
                        if (length > 5 * 1024 * 1024)
                            throw new Exception($"图像文件 {fileName} 尺寸大于 5M，放弃显示");
                        bytes = new byte[length];
                        stream.Read(bytes, 0, length);
                    }
                    s = new MemoryStream(bytes);
                }
                else
                {
                    s = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                }

                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = s;
                imageSource.CacheOption = BitmapCacheOption.OnLoad; // 2023/12/15
                imageSource.EndInit();

                succeed = true;
                return imageSource;
            }
            catch (Exception ex)
            {
                string error = "图像错误: " + ex.Message + " " + fileName;
                WpfClientInfo.WriteErrorLog(error);

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = StringToBitmapConverter.BuildTextImage(error, System.Drawing.Color.Transparent, 400);
                image.CacheOption = BitmapCacheOption.OnLoad;   // (注意这一句必须放在 .UriSource = ... 之后) 防止 WPF 一直锁定这个文件(即便 Image 都消失了还在锁定)
                image.EndInit();

                /*
                // 尝试删除这个图像文件，以便后面还有重试下载的机会
                // 集中通知处理
                _ = Task.Run(() =>
                {
                    App.TryDeleteBrokenImageFile(fileName);
                });
                */
                return image;
            }
            finally
            {
                if (succeed == false && s != null)
                    s?.Close();
            }
        }

        #endregion

        // TODO: 最好把 下架 和 典藏移交(出) 区别开。典藏移交是有明确 location 目的地的操作
        // TODO: 也把上架 和典藏移交(入) 区别开。典藏移交是有明确 location 目的地的操作
        static string GetTransferDirCaption(string transferDirection, string location)
        {
            string result = "";
            string direction = "";
            if (transferDirection == "in")
            {
                result = "上架";
                direction = "入";
            }
            else if (transferDirection == "out")
            {
                result = "下架";
                direction = "出";
            }
            else if (string.IsNullOrEmpty(transferDirection))
                result = "转移(方向不明)";
            else
                result = transferDirection;

            if (string.IsNullOrEmpty(location) == false)
            {
                if (string.IsNullOrEmpty(direction) == false)
                    return result + "+调" + direction;
                return result + "+调拨";
            }
            return result;
        }

        static string GetOperationCaption(string operation)
        {
            if (operation == "borrow")
                return "借";
            if (operation == "return")
                return "还";
            if (operation == "transfer")
                return "转移";
            if (operation == "changeEAS")
                return "修改EAS";

            return operation;
        }
    }
}
