using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 从评注记录加入主题词到 610 字段 的 对话框
    /// </summary>
    internal partial class AddSubjectDialog : Form
    {
        // 初始时候不显示的，备用的新增主题词。一般用于评注记录状态中有了“已处理”值的情况
        public List<string> HiddenNewSubjects = new List<string>();


        public AddSubjectDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public List<string> ReserveSubjects
        {
            get
            {
                return StringUtil.SplitList(this.textBox_reserve.Text.Replace("\r\n", "\n"), '\n');
            }
            set
            {
                this.textBox_reserve.Text = StringUtil.MakePathList(value, "\r\n");
            }
        }

        public List<string> ExistSubjects
        {
            get
            {
                return StringUtil.SplitList(this.textBox_exist.Text.Replace("\r\n", "\n"), '\n');
            }
            set
            {
                this.textBox_exist.Text = StringUtil.MakePathList(value, "\r\n");
            }
        }

        public List<string> NewSubjects
        {
            get
            {
                return StringUtil.SplitList(this.textBox_new.Text.Replace("\r\n", "\n"), '\n');
            }
            set
            {
                this.textBox_new.Text = StringUtil.MakePathList(value, "\r\n");
            }
        }

        private void toolStripButton_splitExist_Click(object sender, EventArgs e)
        {
            this.textBox_exist.Text = this.textBox_exist.Text.Replace(" ", "\r\n");

        }

        // 将空格间隔的词拆为每行一个词
        private void toolStripButton_splitNew_Click(object sender, EventArgs e)
        {
            this.textBox_new.Text = this.textBox_new.Text.Replace(" ", "\r\n");

        }

        private void toolStripButton_importNewSubjects_Click(object sender, EventArgs e)
        {
            this.NewSubjects = this.HiddenNewSubjects;
        }
    }
}
