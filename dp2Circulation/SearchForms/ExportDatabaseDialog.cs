using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows.Forms;

namespace dp2Circulation.SearchForms
{
    public partial class ExportDatabaseDialog : Form
    {
        public List<string> DbNameList = new List<string>();

        public ExportDatabaseDialog()
        {
            InitializeComponent();
        }

        private void ExportDatabaseDialog_Load(object sender, EventArgs e)
        {
            FillDbNameList();
        }

        void FillDbNameList()
        {
            this.comboBox_dbName.Items.Clear();
            foreach (var name in this.DbNameList)
            {
                this.comboBox_dbName.Items.Add(name);
            }
        }

        public static List<string> GetAllBiblioDbNames()
        {
            return Program.MainForm.BiblioDbProperties.Select(o => o.DbName).ToList();
        }

        private void ExportDatabaseDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.SourceName == "database")
            {
                if (string.IsNullOrEmpty(this.DbName))
                {
                    strError = "请选择一个书目库名";
                    goto ERROR1;
                }

                if (this.StartNo > this.EndNo)
                {
                    strError = "起始 ID 号不应大于结束 ID 号";
                    goto ERROR1;
                }
            }

            if (this.checkBox_delete.Checked)
            {
                DialogResult result = MessageBox.Show(this,
    $"确实要删除数据库 {this.DbName} 内 ID 号码范围为 {this.StartNo}-{this.EndNo} 的这些书目记录? ",
    "ExportDatabaseDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
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

        public string DbName
        {
            get
            {
                return this.comboBox_dbName.Text;
            }
            set
            {
                this.comboBox_dbName.Text = value;
            }
        }

        public long StartNo
        {
            get
            {
                return (long)this.numericUpDown_startNo.Value;
            }
            set
            {
                this.numericUpDown_startNo.Value = value;
            }
        }

        public long EndNo
        {
            get
            {
                return (long)this.numericUpDown_endNo.Value;
            }
            set
            {
                this.numericUpDown_endNo.Value = value;
            }
        }

        public bool Deleting
        {
            get
            {
                return this.checkBox_delete.Checked;
            }
            set
            {
                this.checkBox_delete.Checked = value;
            }
        }

        public bool DeletingVisible
        {
            get
            {
                return this.checkBox_delete.Visible;
            }
            set
            {
                this.checkBox_delete.Visible = value;
            }
        }

        public bool NoEventLog
        {
            get
            {
                return this.checkBox_noEventLog.Checked;
            }
            set
            {
                this.checkBox_noEventLog.Checked = value;
            }
        }

        public bool CompressTailNo
        {
            get
            {
                return this.checkBox_compressTailNo.Checked;
            }
            set
            {
                this.checkBox_compressTailNo.Checked = value;
            }
        }

        public string SourceName
        {
            get
            {
                if (this.tabControl_source.SelectedTab == this.tabPage_selected)
                    return "selected";
                return "database";
            }
            set
            {
                if (value == "selected")
                    this.tabControl_source.SelectedTab = this.tabPage_selected;
                else
                    this.tabControl_source.SelectedTab = this.tabPage_database;
            }
        }

        public bool SelectedPageVisible
        {
            get
            {
                return this.tabControl_source.TabPages.IndexOf(this.tabPage_selected) != -1;
            }
            set
            {
                if (value == false)
                {
                    // 去除 page selected
                    this.tabControl_source.TabPages.Remove(this.tabPage_selected);
                }
                else
                {
                    if (this.tabControl_source.TabPages.IndexOf(this.tabPage_selected) == -1)
                        this.tabControl_source.TabPages.Insert(0, this.tabPage_selected);
                }
            }
        }

        public int SelectedItemCount
        {
            set
            {
                this.label_selected_message.Text = $"当前选择了 {value} 个事项";
                if (value == 0)
                    this.SelectedPageVisible = false;
                else
                    this.SelectedPageVisible = true;
            }
        }

        private void checkBox_delete_CheckedChanged(object sender, EventArgs e)
        {
            this.checkBox_compressTailNo.Visible = this.checkBox_delete.Checked;
        }
    }
}
