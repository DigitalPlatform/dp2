using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// 导入数据的对话框
    /// </summary>
    public partial class ImportDataDialog : Form
    {
        public ImportDataDialog()
        {
            InitializeComponent();
        }

        private void button_findFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要导入的数据文件";
            dlg.FileName = this.textBox_fileName.Text;
            dlg.Filter = "备份文件 (*.dp2bak)|*.dp2bak|XML文件 (*.xml)|*.xml|ISO2709文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            dlg.Multiselect = true; // 2024/6/4 允许多选

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // this.textBox_fileName.Text = dlg.FileName;
            this.textBox_fileName.Text = string.Join("\r\n", dlg.FileNames);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_fileName.Text) == true)
            {
                strError = "尚未指定要导入的文件名";
                goto ERROR1;
            }

            if (this.FileNames.Length == 0)
            {
                strError = "尚未指定要导入的文件名";
                goto ERROR1;
            }

            // 如果 data 和 object 两个 checkbox 都是 false，要警告一下，但允许这样做，作用就是验证一下过程
            if (this.checkBox_importDataRecord.Checked == false
                && this.checkBox_importObject.Checked == false)
            {
                DialogResult result = MessageBox.Show(this,
    "您选择了既不导入数据记录，也不导入数字对象，本次处理将不会写入任何数据。\r\n\r\n确实要这样继续进行?",
    "导入数据",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            if (this.checkBox_importDataRecord.Checked == true
                && this.checkBox_insertMissing.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
"您选择了在导入数据记录的时候，采用“仅当数据记录不存在的时候才写入”的方式，这样对于数据库中存在数据记录的位置，将不会写入数据记录。\r\n\r\n确实要这样继续进行?",
"导入数据",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        /*
        public string FileName
        {
            get
            {
                return this.textBox_fileName.Text;
            }
            set
            {
                this.textBox_fileName.Text = value;
            }
        }
        */

        // 文件名集合
        public string[] FileNames
        {
            get
            {
                var values = StringUtil.SplitList(this.textBox_fileName.Text.Replace("\r\n", "\n"), '\n');
                StringUtil.RemoveDupNoSort(ref values);
                StringUtil.RemoveBlank(ref values);
                return values.ToArray();
            }
            set
            {
                if (value == null)
                    this.textBox_fileName.Text = "";
                else
                    this.textBox_fileName.Text = StringUtil.MakePathList(value.ToList(), "\r\n");
            }
        }

        public bool ImportDataRecord
        {
            get
            {
                return this.checkBox_importDataRecord.Checked;
            }
            set
            {
                this.checkBox_importDataRecord.Checked = value;
            }
        }

        public bool ImportObject
        {
            get
            {
                return this.checkBox_importObject.Checked;
            }
            set
            {
                this.checkBox_importObject.Checked = value;
            }
        }

        public bool InsertMissing
        {
            get
            {
                return this.checkBox_insertMissing.Checked;
            }
            set
            {
                this.checkBox_insertMissing.Checked = value;
            }
        }

        private void checkBox_importDataRecord_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_importDataRecord.Checked == true)
                this.checkBox_insertMissing.Enabled = true;
            else
                this.checkBox_insertMissing.Enabled = false;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_fileName);
                controls.Add(this.checkBox_importDataRecord);
                controls.Add(this.checkBox_importObject);
                controls.Add(this.checkBox_insertMissing);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_fileName);
                controls.Add(this.checkBox_importDataRecord);
                controls.Add(this.checkBox_importObject);
                controls.Add(this.checkBox_insertMissing);
                GuiState.SetUiState(controls, value);
            }
        }
    }
}
