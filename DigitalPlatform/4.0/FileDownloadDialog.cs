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

        public Button CancelButton
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

        public void SetProgress(long bytesReceived, 
            long totalBytesToReceive)
        {
            if (this.IsDisposed)
                return;

            this.Invoke((Action)(() =>
            {
                double ratio = (double)bytesReceived / (double)totalBytesToReceive;
                //this.progressBar1.Minimum = 0;
                //this.progressBar1.Maximum = 100;
                this.progressBar1.Value = Convert.ToInt32((double)100 * ratio);

                if (ratio == 100)
                    this.progressBar1.Style = ProgressBarStyle.Marquee;
                else
                    this.progressBar1.Style = ProgressBarStyle.Continuous;

                this.label_message.Text = bytesReceived.ToString() + " / " + totalBytesToReceive.ToString() + " " + this.SourceFilePath;
            }));
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
