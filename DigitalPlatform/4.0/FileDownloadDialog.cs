using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public partial class FileDownloadDialog : Form
    {
        public string SourceFilePath { get; set; }
        public string TargetFilePath { get; set; }

        public FileDownloadDialog()
        {
            InitializeComponent();
        }

        public void SetMessage(string strText)
        {
            if (this.IsDisposed)
                return;

            this.Invoke((Action)(() =>
            {
                this.label_message.Text = strText;
            }));
        }

        public Button MyCancelButton
        {
            get
            {
                return this.button_cancel;
            }
        }

        public ProgressBar ProgressBar
        {
            get
            {
                return this.progressBar1;
            }
        }

        public Label MessageLabel
        {
            get
            {
                return this.label_message;
            }
        }

#if NO
        string _text = "";

        public void SetText(string strTitle, string strText)
        {
            this.Text = strTitle;
            this._text = strText;
        }
#endif
        Bandwidth _bandwidth = new Bandwidth();

        // 设置进度条信息
        // parameters:
        //      strText 文字。当 strText 为空的时候，函数用 bytesReceived 和 totalBytesToReceive 刷新设置进度条比例显示。否则只刷新显示 strText 文字
        //      bytesReceived   接收到多少 bytes
        //      totalBytesToReceive 总共计划接收多少 bytes
        public void SetProgress(
            string strText,
            long bytesReceived,
            long totalBytesToReceive,
            long transferReceived = 0)
        {
            if (this.IsDisposed)
                return;

            try
            {
                this.Invoke((Action)(() =>
                {
                    if (totalBytesToReceive > 0)
                    {
                        double ratio = (double)bytesReceived / (double)totalBytesToReceive;
                        //this.progressBar1.Minimum = 0;
                        //this.progressBar1.Maximum = 100;
                        this.progressBar1.Value = Convert.ToInt32((double)100 * ratio);

                        if (ratio == 100)
                            this.progressBar1.Style = ProgressBarStyle.Marquee;
                        else
                            this.progressBar1.Style = ProgressBarStyle.Continuous;

                        // this.label_message.Text = bytesReceived.ToString() + " / " + totalBytesToReceive.ToString() + " " + this.SourceFilePath;
                        this.label_message.Text = GetLengthText(bytesReceived) + " / " + GetLengthText(totalBytesToReceive) + " " + this.SourceFilePath;

                        // 2022/4/11
                        double band_width = 0;
                        if (transferReceived != 0)
                            band_width = _bandwidth.GetWidth(transferReceived);
                        else
                            band_width = _bandwidth.GetWidth(bytesReceived);
                        if (band_width != -1)
                        {
                            this.label_bandwidth.Text = GetLengthText((long)band_width * 8) + " bit/秒";
                        }
                    }

                    if (strText != null)
                        this.label_message.Text = strText;
                }));
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string[] units = new string[] { "K", "M", "G", "T" };
        public static string GetLengthText(long length)
        {
            decimal v = length;
            int i = 0;
            foreach (string strUnit in units)
            {
                v = decimal.Round(v / 1024, 2);
                if (v < 1024 || i >= units.Length - 1)
                    return v.ToString() + strUnit;

                i++;
            }

            return length.ToString();
        }


        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void StartMarquee()
        {
            this.progressBar1.Style = ProgressBarStyle.Marquee;
        }

        public void StopMarquee()
        {
            this.progressBar1.Style = ProgressBarStyle.Continuous;
        }
    }

    // 带宽计算
    public class Bandwidth
    {
        public long Width { get; set; }

        DateTime _lastTime = DateTime.MinValue;
        double _lastWidth = 0;
        long _lastReceived = 0; // 上次接收到的总数量

        // 平滑间隔
        TimeSpan _period = TimeSpan.FromSeconds(5);

        // 计算瞬时带宽
        public double GetWidth(long bytesReceived)
        {
            var now = DateTime.Now;
            if (_lastTime == DateTime.MinValue)
            {
                _lastTime = now;
                _lastReceived = 0;
                return 0;
            }

            var delta_length = now - _lastTime;

            if (delta_length < _period)
            {
                return -1;  // 表示累积的时间还不够，没有进行计算
            }

            _lastTime = now;

            long delta_bytes = bytesReceived - _lastReceived;
            _lastReceived = bytesReceived;


            if (delta_length.TotalSeconds == 0)
                return 0;

            _lastWidth = (double)delta_bytes / delta_length.TotalSeconds;
            return _lastWidth;
        }
    }
}
