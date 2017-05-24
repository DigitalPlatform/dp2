using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// 批订购中用于浏览订单的窗口
    /// </summary>
    public partial class OrderListViewerForm : Form
    {
        public bool Docked = false;

        public event EventHandler DockChanged = null;

        public BatchOrderForm BatchOrderForm = null;

        public OrderListViewerForm()
        {
            InitializeComponent();
        }

        List<Control> _freeControls = new List<Control>();

        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            if (Program.MainForm.CurrentPropertyControl != this.tabControl_main)
                Program.MainForm.CurrentPropertyControl = this.tabControl_main;

            // 防止内存泄漏
            ControlExtention.AddFreeControl(_freeControls, this.tabControl_main);

            if (bShowFixedPanel == true
                && Program.MainForm.PanelFixedVisible == false)
            {
                Program.MainForm.PanelFixedVisible = true;

                // 把 propertypage 翻出来
                Program.MainForm.ActivatePropertyPage();
            }

            this.Docked = true;
            this.Visible = false;

            if (this.DockChanged != null)
            {
                this.DockChanged(this, new EventArgs());
            }
        }

        public void UnDock()
        {
            this.Docked = false;
            this.Visible = true;

            if (Program.MainForm.CurrentPropertyControl == this.MainControl)
                Program.MainForm.CurrentPropertyControl = null;

            if (this.DockChanged != null)
            {
                this.DockChanged(this, new EventArgs());
            }
        }

#if NO
        /// <summary>
        /// 停靠
        /// </summary>
        public event DoDockEventHandler DoDockEvent = null;

        public void DoDock(bool bShowFixedPanel)
        {
            if (this.DoDockEvent != null)
            {
                DoDockEventArgs e = new DoDockEventArgs();
                e.ShowFixedPanel = bShowFixedPanel;
                this.DoDockEvent(this, e);

                //this.tableLayoutPanel1.BackColor = Color.DimGray;
                //this.tableLayoutPanel1.ForeColor = Color.White;
            }
        }
#endif

        public Control MainControl
        {
            get
            {
                return this.tabControl_main;
            }
        }

        void DisposeFreeControls()
        {
            if (this.tabControl_main != null && Program.MainForm != null)
            {
                // 如果当前固定面板拥有 tabControl_main，则要先解除它的拥有关系，否则怕本 Form 摧毁的时候无法 Dispose() 它
                if (Program.MainForm.CurrentPropertyControl == this.tabControl_main)
                    Program.MainForm.CurrentPropertyControl = null;
            }

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);
        }

        public class Sheet
        {
            public TabPage TabPage { get; set; }
            public WebBrowser WebBrowser { get; set; }

#if NO
            public void OnSelectionChanged()
            {
                MessageBox.Show("test");
            }
#endif
        }

        public Sheet ActiveSheet
        {
            get
            {
                if (this.tabControl_main.TabPages.Count == 0)
                    return null;
                string name = this.tabControl_main.SelectedTab.Text;
                return (Sheet)_sheetTable[name];
            }
        }

        public List<string> GetSheetNames()
        {
            List<string> results = new List<string>();
            foreach(TabPage page in this.tabControl_main.TabPages)
            {
                results.Add(page.Text);
            }

            return results;
        }

        public Sheet GetSheet(string name)
        {
            return (Sheet)_sheetTable[name];
        }

        public void RemoveSheet(string name)
        {
            Sheet sheet = (Sheet)_sheetTable[name];
            if (sheet == null)
                return;
            this.tabControl_main.TabPages.Remove(sheet.TabPage);
            sheet.TabPage.Dispose();
            _sheetTable.Remove(name);
        }

        Hashtable _sheetTable = new Hashtable();    // name --> Sheet

        public Sheet CreateSheet(string name)
        {
            Sheet sheet = (Sheet)_sheetTable[name];
            if (sheet != null)
                return sheet;

            sheet = new Sheet();
            sheet.TabPage = new TabPage();
            sheet.TabPage.Text = name;
            sheet.WebBrowser = new WebBrowser();
            sheet.WebBrowser.Dock = DockStyle.Fill;
            sheet.TabPage.Controls.Add(sheet.WebBrowser);
            this.tabControl_main.TabPages.Add(sheet.TabPage);

            OrderListScript script = new OrderListScript();
            script.Sheet = sheet;
            script.BatchOrderForm = this.BatchOrderForm;
            sheet.WebBrowser.ObjectForScripting = script;

            _sheetTable[name] = sheet;
            return sheet;
        }

    }
}
