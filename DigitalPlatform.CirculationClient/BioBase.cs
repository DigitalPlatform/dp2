using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 生物识别基础类
    /// </summary>
    public class BioBase
    {
        public event SpeakEventHandler Speak = null;
        public event CapturedEventHandler Captured = null;
        public event ImageReadyEventHandler ImageReady = null;

        public event DownloadProgressChangedEventHandler ProgressChanged = null;

        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        public void SetProgress(long current, long total)
        {
            if (ProgressChanged != null)
            {
                DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(current, total);
                ProgressChanged(null, e);
            }
        }

        public void Speaking(string text, string displayText = null)
        {
            if (Speak != null)
            {
                Speak(null, new SpeakEventArgs
                {
                    Text = text,
                    DisplayText = displayText
                });
            }
        }

        public void ShowMessage(string text)
        {
            if (ProgressChanged != null)
            {
                DownloadProgressChangedEventArgs e = new DownloadProgressChangedEventArgs(text)
                {
                    BytesReceived = -1,
                    TotalBytesToReceive = -1
                };
                ProgressChanged(null, e);
            }
        }

        public void Loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (Prompt != null)
                Prompt(sender, e);
        }

        public bool HasLoaderPrompt()
        {
            return (Prompt != null);
        }

        public bool HasImageReady()
        {
            return (ImageReady != null);
        }

        public void TriggerImageReady(object sender, ImageReadyEventArgs e)
        {
            if (ImageReady != null)
                ImageReady(sender, e);
        }

        public void TriggerCaptured(object sender, CapturedEventArgs e)
        {
            if (Captured != null)
                Captured(sender, e);
        }
    }

    /// <summary>
    /// 捕获完成提示事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void CapturedEventHandler(object sender,
        CapturedEventArgs e);

    /// <summary>
    /// 捕获完成事件的参数
    /// </summary>
    public class CapturedEventArgs : EventArgs
    {
        // 识别出的号码文字
        public string Text { get; set; }
        public int Score { get; set; }
        public string ErrorInfo { get; set; }

        public int Quality { get; set; }    // 指纹图象质量

        // 用于识别消息的 ID
        public string MessageID { get; set; }

        // 2021/3/23
        // 捕获的时间
        public DateTime CreateTime { get; set; }

        // 2021/10/28
        // 图像数据。指纹、掌纹等的图像
        public byte[] ImageData { get; set; }
    }

    public delegate void ImageReadyEventHandler(object sender,
    ImageReadyEventArgs e);

    /// <summary>
    /// 捕获完成事件的参数
    /// </summary>
    public class ImageReadyEventArgs : EventArgs
    {
        public Image Image { get; set; }    // 注意，使用者要负责 Dispose() 这个 Image 对象

        public int Quality { get; set; }    // 指纹图象质量
    }

    public delegate void SpeakEventHandler(object sender,
    SpeakEventArgs e);

    public class SpeakEventArgs : EventArgs
    {
        // 语音文字
        public string Text { get; set; }
        // 用于显示的文字。如果为空，则表示使用 Text 成员
        public string DisplayText { get; set; }
    }
}
