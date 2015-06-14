using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    // 在编辑册记录、条码册登记的过程中，发现有重复条码的记录，
    // 本对话框用于显示这些册记录
    internal partial class EntityBarcodeFoundDupDlg : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        Commander commander = null;
        WebExternalHost m_webExternalHostItem = new WebExternalHost();

        // 2012/2/9
        WebExternalHost m_webExternalHostBiblio = new WebExternalHost();

        public string ItemText = "";    // 册的HTML信息
        public string BiblioText = "";  // 种的HTML信息

        public string MessageText
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        public EntityBarcodeFoundDupDlg()
        {
            InitializeComponent();
        }

        private void EntityBarcodeFoundDupDlg_Load(object sender, EventArgs e)
        {
            // webbrowser
            this.m_webExternalHostItem.Initial(this.MainForm, this.webBrowser_item);
            this.webBrowser_item.ObjectForScripting = this.m_webExternalHostItem;

            this.m_webExternalHostBiblio.Initial(this.MainForm, this.webBrowser_biblio);
            this.webBrowser_biblio.ObjectForScripting = this.m_webExternalHostBiblio;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);


            if (String.IsNullOrEmpty(this.ItemText) == false)
            {
#if NO
                Global.SetHtmlString(this.webBrowser_item,
                    this.ItemText,
                    this.MainForm.DataDir,
                    "entitybarcodedup_item");
#endif
                this.m_webExternalHostItem.SetHtmlString(this.ItemText,
                    "entitybarcodedup_item");
            }

            if (String.IsNullOrEmpty(this.BiblioText) == false)
            {
#if NO
                Global.SetHtmlString(this.webBrowser_biblio,
                    this.BiblioText,
                    this.MainForm.DataDir,
                    "entitybarcodedup_item");
#endif
                this.m_webExternalHostBiblio.SetHtmlString(this.BiblioText,
                    "entitybarcodedup_biblio");
            }

        }


        private void EntityBarcodeFoundDupDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHostItem != null)
                this.m_webExternalHostItem.Destroy();

            if (this.m_webExternalHostBiblio != null)
                this.m_webExternalHostBiblio.Destroy();
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHostItem.ChannelInUse | this.m_webExternalHostBiblio.ChannelInUse;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}