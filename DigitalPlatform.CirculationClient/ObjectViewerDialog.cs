using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace DigitalPlatform.CirculationClient
{
    public partial class ObjectViewerDialog : Form
    {
        public ObjectViewerDialog()
        {
            InitializeComponent();
        }

        public string HtmlString
        {
            get
            {
                return webBrowser1.DocumentText;
            }
            set
            {
                webBrowser1.DocumentText = value;
            }
        }

        public string Url
        {
            get;
            set;
        }

        private void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
        }

        public void TriggerStreamProgressChanged(string path, long current, long length)
        {
            DisplayProgress(path, current, length);
        }

        void DisplayProgress(string path, long current, long length)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, long, long>(DisplayProgress), path, current, length);
                return;
            }

            SetProgress(this.toolStripProgressBar1, current, length);
            // TODO: 显示为多少 K，多少 M
            if (current == length)
                this.toolStripStatusLabel1.Text = "";
            else
                this.toolStripStatusLabel1.Text = path + " - " + current.ToString() + " / " + length;
        }

        static void SetProgress(ToolStripProgressBar progress,
            long current,
            long length)
        {
            if (current == length)
            {
                progress.Value = 0; // 结束
                return;
            }
            double progress_ratio = (double)64000 / (double)length;
            if (progress_ratio > 1.0)
                progress_ratio = 1.0;

            int maximum = (int)(length * progress_ratio);

            if (progress.Maximum != maximum)
            {
                progress.Minimum = 0;
                progress.Maximum = maximum;
            }
            progress.Value = (int)(current * progress_ratio);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this);
                GuiState.SetUiState(controls, value);
            }
        }

        private void ObjectViewerDialog_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.Url) == false)
                this.BeginInvoke(new Action<string>(Navigate), this.Url);
        }

        void Navigate(string url)
        {
            this.webBrowser1.Navigate(url, true);
        }
    }
}
