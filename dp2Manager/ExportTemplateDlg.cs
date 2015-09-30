using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.rms.Client;
using DigitalPlatform;

namespace dp2Manager
{
    public partial class ExportTemplateDlg : Form
    {
        public MainForm MainForm = null;
        public List<ObjectInfo> Objects = null; // 输入的对象数组
        public List<ObjectInfo> SelectedObjects = null; // 输出的对象数组


        public ExportTemplateDlg()
        {
            InitializeComponent();
        }

        private void ExportTemplateDlg_Load(object sender, EventArgs e)
        {
            FillList();
        }

        private void FillList()
        {
            this.listView_objects.Items.Clear();

            for (int i = 0; i < this.Objects.Count; i++)
            {
                ObjectInfo objectinfo = this.Objects[i];

                ListViewItem item = new ListViewItem(
                    objectinfo.Path,
                    objectinfo.ImageIndex);

                item.SubItems.Add(objectinfo.Url);

                this.listView_objects.Items.Add(item);
                item.Checked = true;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_exportFileName.Text == "")
            {
                MessageBox.Show(this, "尚未指定输出文件名");
                return;
            }

            string strError = "";
            TemplateCollection templates = null;

            this.Cursor = Cursors.WaitCursor;
            int nRet = BuildTemplates(out templates,
                out strError);
            this.Cursor = Cursors.Arrow;
            if (nRet == -1)
                goto ERROR1;

            try
            {
                templates.Save(this.textBox_exportFileName.Text);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        int BuildTemplates(out TemplateCollection templates,
            out string strError)
        {
            strError = "";

            templates = new TemplateCollection();

            for (int i = 0; i < this.listView_objects.Items.Count; i++)
            {
                ListViewItem item = this.listView_objects.Items[i];
                if (item.Checked == false)
                    continue;

                if (item.ImageIndex == ResTree.RESTYPE_FOLDER
                    || item.ImageIndex == ResTree.RESTYPE_FILE)
                {
                    continue;
                }

                string strDbName = item.Text;
                string strUrl = item.SubItems[1].Text;

                DatabaseObjectTree tree = new DatabaseObjectTree();
                tree.Initial(MainForm.Servers,
                    MainForm.Channels,
                    MainForm.stopManager,
                    strUrl,
                    strDbName);
                // 

                RmsChannel channel = MainForm.Channels.GetChannel(strUrl);
                if (channel == null)
                    goto ERROR1;
                List<string[]> logicNames = null;
                string strType;
                string strSqlDbName;
                string strKeysDef;
                string strBrowseDef;

                long nRet = channel.DoGetDBInfo(
                    strDbName,
                    "all",
                    out logicNames,
                    out strType,
                    out strSqlDbName,
                    out strKeysDef,
                    out strBrowseDef,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                Template template = new Template();
                template.LogicNames = logicNames;
                template.Type = strType;
                template.SqlDbName = strSqlDbName;
                template.KeysDef = strKeysDef;
                template.BrowseDef = strBrowseDef;

                template.Object = tree.Root;

                templates.Add(template);
            }

            return 0;
            ERROR1:
            return -1;
        }

        private void button_findExportFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.CreatePrompt = false;
            dlg.FileName = this.textBox_exportFileName.Text;
            dlg.Filter = "模板文件 (*.template)|*.template|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.textBox_exportFileName.Text = dlg.FileName;
        }
    }
}