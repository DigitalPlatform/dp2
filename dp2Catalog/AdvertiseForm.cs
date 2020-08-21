using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Catalog
{
    public partial class AdvertiseForm : Form
    {
        public MainForm MainForm = null;

        public AdvertiseForm()
        {
            InitializeComponent();
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
        }

        public void SaveSize()
        {
            if (this.MainForm!= null && this.MainForm.AppInfo != null
                && this.WindowState != FormWindowState.Minimized)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
        "mdi_form_state");
            }
        }

        private void AdvertiseForm_Load(object sender, EventArgs e)
        {
            LoadSize();

            this.Invoke(new Action(LoadPage));
        }
        
        void LoadPage()
        {
            this.webBrowser1.Url = new Uri("http://dp2003.com/dp2portal/view.aspx?link=opensource.xml");
            // this.webBrowser1.Url = new Uri("http://www.ilovelibrary.cn/");
        }

        private void AdvertiseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void AdvertiseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSize();

        }
    }
}
