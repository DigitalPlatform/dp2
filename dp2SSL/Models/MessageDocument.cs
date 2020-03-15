using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using DigitalPlatform.Text;

namespace dp2SSL
{
    public class MessageDocument
    {
        List<MessageItem> _items = new List<MessageItem>();
        public List<MessageItem> Items
        {
            get
            {
                return _items;
            }
        }

        public void Add(MessageItem item)
        {
            _items.Add(item);
        }

        public void AddRange(List<MessageItem> items)
        {
            _items.AddRange(items);
        }

        public void Remove(MessageItem item)
        {
            _items.Remove(item);
        }

        public void Add(
            Operator patron,
            string operation,
            string resultType,
            string errorInfo,
            string errorCode,
            Entity entity)
        {
            Items.Add(new MessageItem
            {
                Operator = patron,
                Operation = operation,
                ResultType = resultType,
                ErrorInfo = errorInfo,
                ErrorCode = errorCode,
                Entity = entity
            });
        }

        public int ErrorCount
        {
            get
            {
                int count = 0;
                foreach (var item in _items)
                {
                    if (item.ResultType == "error")
                        count++;
                }
                return count;
            }
        }

        public FlowDocument BuildDocument(// string patron_name,
            double baseFontSize,
            out string speak)
        {
            speak = "";
            List<MessageItem> items = new List<MessageItem>();
            items.AddRange(this._items);

            /*
            // 按照 Operation 排序
            items.Sort((a, b) =>
            {
                return -1*string.CompareOrdinal(a.Operation, b.Operation);
            });
            */

            FlowDocument doc = new FlowDocument();

            // 第一部分，总结信息
            List<string> names = new List<string>();
            {
                items.ForEach((o) =>
                {
                    if (o.Operator != null)
                        names.Add(string.IsNullOrEmpty(o.Operator.PatronName) ? o.Operator.PatronBarcode : o.Operator.PatronName);
                });
                StringUtil.RemoveDupNoSort(ref names);
            }

            int return_count = items.FindAll((o) => { return o.Operation == "return"; }).Count;
            int borrow_count = items.FindAll((o) => { return o.Operation == "borrow"; }).Count;
            int transfer_count = items.FindAll((o) => { return o.Operation == "transfer"; }).Count;

            int succeed_count = items.FindAll((o) => { return o.ResultType == "succeed" || string.IsNullOrEmpty(o.ResultType); }).Count;
            int error_count = items.FindAll((o) => { return o.ResultType == "error"; }).Count;
            int warning_count = items.FindAll((o) => { return o.ResultType == "warning"; }).Count;
            int information_count = items.FindAll((o) => { return o.ResultType == "information"; }).Count;

            {
                var p = new Paragraph();
                p.FontFamily = new FontFamily("微软雅黑");
                p.FontSize = baseFontSize;
                p.TextAlignment = TextAlignment.Left;
                p.Foreground = Brushes.Gray;
                // p.TextIndent = -20;
                p.Margin = new Thickness(0, 0, 0, 18);
                doc.Blocks.Add(p);

                if (borrow_count + return_count > 0)
                {
                    List<string> lines = new List<string>();
                    if (return_count > 0)
                        lines.Add($"还书请求 {return_count}");
                    if (borrow_count > 0)
                        lines.Add($"借书请求 {borrow_count}");
                    if (transfer_count > 0)
                        lines.Add($"转移请求 {transfer_count}");

                    p.Inlines.Add(new Run
                    {
                        Text = $"{StringUtil.MakePathList(names)} ",
                        //Background = Brushes.DarkRed,
                        //Foreground = Brushes.White
                        FontFamily = new FontFamily("楷体"),
                        FontSize = baseFontSize * 2.5,
                        // FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                    });

                    p.Inlines.Add(new Run
                    {
                        Text = $"{StringUtil.MakePathList(lines, ", ")}\r\n",
                        //Background = Brushes.DarkRed,
                        //Foreground = Brushes.White
                        FontSize = baseFontSize * 1.2,
                        Foreground = Brushes.White,
                    });
                }

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
            }

            // 第二部分，列出每一笔操作
            int index = 0;
            foreach (var item in items)
            {
                var p = item.BuildParagraph(index++, baseFontSize);
                doc.Blocks.Add(p);
            }

            // 构造提示语音
            List<string> speaks = new List<string>();
            int overflow_count = 0;
            foreach (var item in items)
            {
                if (item.Operation == "borrow" && item.ErrorCode == "overflow")
                    overflow_count++;
            }

            if (overflow_count > 0)
            {
                speaks.Add($"警告：有 {overflow_count} 册图书超越许可册数，请放回书柜，谢谢");
            }

            if (speaks.Count == 0)
                speaks.Add("操作完成");

            speak = StringUtil.MakePathList(speaks, "; ");

            /*
            Paragraph p = new Paragraph(new Run("Hello, world!"));
            p.FontSize = 36;
            doc.Blocks.Add(p);

            p = new Paragraph(new Run("The ultimate programming greeting!"));
            p.FontSize = 14;
            p.FontStyle = FontStyles.Italic;
            p.TextAlignment = TextAlignment.Left;
            p.Foreground = Brushes.Gray;
            doc.Blocks.Add(p);
            */
            return doc;
        }

#if NO
        public delegate bool Deletage_shouldVisible(MessageItem item);

        public static void HideSomething(FlowDocument doc, 
            Deletage_shouldVisible func_shouldVisible)
        {
            foreach(var paragraph in doc.Blocks)
            {
                MessageItem item = paragraph.Tag as MessageItem;
                if (item == null)
                    continue;
                bool visible = (bool)func_shouldVisible?.Invoke(item);
                paragraph.
            }
        }

#endif
    }

    public class MessageItem
    {
        public Operator Operator { get; set; }
        public string Operation { get; set; }   // borrow 或 return 或 transfer
        public string ResultType { get; set; }  // 结果类型。succeed/error/warning/information
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }   // 错误码

        public string Direction { get; set; }   // 转移操作的方向。只有转移操作才用到这个字段

        // 消息所涉及到的实体
        public Entity Entity { get; set; }

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

        public Paragraph BuildParagraph(int index, double baseFontSize)
        {
            var p = new Paragraph();
            p.FontFamily = new FontFamily("微软雅黑");
            p.FontSize = baseFontSize;
            // p.FontStyle = FontStyles.Italic;
            p.TextAlignment = TextAlignment.Left;
            p.Foreground = Brushes.Gray;
            // p.LineHeight = 18;
            p.TextIndent = -20;
            p.Margin = new Thickness(10, 0, 0, 8);
            p.Tag = this;   // 记忆下来后面隐藏事项的时候可以用到

            // 序号
            p.Inlines.Add(new Run($"{(index + 1).ToString()}) "));

            Brush back = Brushes.Transparent;
            // 成功和失败状态
            if (ResultType == "error")
            {
                back = Brushes.DarkRed;
                p.Inlines.Add(new Run
                {
                    Text = " 失败 ",
                    Background = back,
                    Foreground = Brushes.White
                });
            }
            else if (ResultType == "warning")
            {
                back = Brushes.DarkGoldenrod;
                p.Inlines.Add(new Run
                {
                    Text = " 警告 ",
                    Background = back,
                    Foreground = Brushes.White
                });
            }
            else if (ResultType == "information")
            {
                back = Brushes.Gray;
                p.Inlines.Add(new Run
                {
                    Text = " 信息 ",
                    Background = back,
                    Foreground = Brushes.White
                });
            }
            else
            {
                back = Brushes.DarkGreen;
                p.Inlines.Add(new Run
                {
                    Text = " 成功 ",
                    Background = back,
                    Foreground = Brushes.White
                });
            }

            // 操作名称
            p.Inlines.Add(new Run
            {
                Text = GetOperationCaption(Operation) + " ",
                Foreground = Brushes.White
            });

            // 转移方向
            if (Operation == "transfer" && string.IsNullOrEmpty(Direction) == false)
            {
                p.Inlines.Add(new Run
                {
                    Text = GetOperationCaption(Direction) + " ",
                    Foreground = Brushes.White
                });
            }

            // 书目摘要
            if (Entity != null && string.IsNullOrEmpty(Entity.Title) == false)
            {
                Run run = new Run(Entity.Title);
                /*
                run.FontSize = 14;
                run.FontStyle = FontStyles.Normal;
                run.Background = Brushes.DarkRed;
                run.Foreground = Brushes.White;
                */

                p.Inlines.Add(run);
            }

            // 错误码和错误信息
            if (string.IsNullOrEmpty(ErrorInfo) == false)
            {
                p.Inlines.Add(new Run
                {
                    Text = "\r\n" + ErrorInfo,
                    Background = back,
                    Foreground = Brushes.White
                });
            }

            return p;
        }
    }
}
