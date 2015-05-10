using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.rms.Client;

namespace dp2Manager
{
    public partial class ImportTemplateDlg : Form
    {
        public MainForm MainForm = null;
        public string FileName = "";
        public string Url = "";

        TemplateCollection templates = null;

        public ImportTemplateDlg()
        {
            InitializeComponent();
        }

        private void ImportTemplateDlg_Load(object sender, EventArgs e)
        {
            FillList();

        }

        void FillList()
        {
            this.listView_objects.Items.Clear();


            if (this.FileName == "")
                return;

            try
            {
                templates = TemplateCollection.Load(this.FileName, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            for (int i = 0; i < templates.Count; i++)
            {
                Template t = templates[i];

                ListViewItem item = new ListViewItem(t.Object.Name, t.Object.Type);
                item.Tag = t;

                item.SubItems.Add(this.Url);

                this.listView_objects.Items.Add(item);
            }
        }

        void EnableControls(bool bEnable)
        {
            this.listView_objects.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_create.Enabled = bEnable;
        }
  
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_create_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            for (int i = 0; i < this.listView_objects.Items.Count; i++)
            {
                ListViewItem item = this.listView_objects.Items[i];

                if (item.Checked == false)
                    continue;

                string strUrl = item.SubItems[1].Text;

                Template t = (Template)item.Tag;

                if (t.Object.Type == ResTree.RESTYPE_DB)
                {
                    // 创建数据库
                    DatabaseDlg dlg = new DatabaseDlg();
                    dlg.Text = "根据模板创建新数据库";
                    dlg.IsCreate = true;
                    dlg.BatchMode = true;
                    // dlg.RefDbName = strRefDbName;
                    dlg.MainForm = this.MainForm;
                    dlg.RefObject = t.Object;
                    dlg.Initial(strUrl,
                        "");

                    dlg.DatabaseType = t.Type;
                    dlg.SqlDbName = t.SqlDbName;
                    dlg.KeysDef = t.KeysDef;
                    dlg.BrowseDef = t.BrowseDef;
                    dlg.LogicNames = t.LogicNames;

                    this.MainForm.AppInfo.LinkFormState(dlg, "databasedlg_state");
                    DialogResult result = dlg.ShowDialog(this);
                    this.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (result == DialogResult.Cancel && i < this.listView_objects.Items.Count - 1)
                    {
                        DialogResult resultTemp = MessageBox.Show(this,
                            "是否要中断批处理?",
                            "dp2Manager",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (resultTemp == DialogResult.Yes)
                            break;
                    }
                }

            }

            EnableControls(true);
        }

        private void button_selectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_objects.Items.Count; i++)
            {
                this.listView_objects.Items[i].Checked = true;
            }
        }
    }
}