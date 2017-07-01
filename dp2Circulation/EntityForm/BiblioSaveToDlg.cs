using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 书目记录另存为 对话框
    /// </summary>
    internal partial class BiblioSaveToDlg : Form
    {
        // 是否越过 从剪贴板自动复制记录路径 的动作
        public bool SuppressAutoClipboard = false;

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        public string CurrentBiblioRecPath = "";    // 当前书目记录的路径

        bool m_bSavedBuildLink = false; // 最初显示的BuildLink值

        int m_nManual = 0;  // 如果为0，表示界面手动勾选。否则就是程序内部去改变checked值

        // 过滤书目库列表用的 MARC 格式
        // 如果为空，则表示不过滤
        public string MarcSyntax
        {
            get;
            set;
        }

        public BiblioSaveToDlg()
        {
            InitializeComponent();
        }

        private void BiblioSaveToDlg_Load(object sender, EventArgs e)
        {
            this.m_bSavedBuildLink = this.BuildLink;

            comboBox_biblioDbName_TextChanged(null, null);

            if (this.SuppressAutoClipboard == false)
            TrySetRecPathFromClipboard();
        }

        private void BiblioSaveToDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.comboBox_biblioDbName.Text) == true)
            {
                MessageBox.Show(this, "尚未指定书目库名");
                return;
            }

            if (String.IsNullOrEmpty(this.textBox_recordID.Text) == true)
            {
                MessageBox.Show(this, "尚未指定记录ID");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        public string RecPath
        {
            get
            {
                return this.comboBox_biblioDbName.Text + "/" + this.textBox_recordID.Text;
            }
            set
            {
                int nRet = value.IndexOf("/");
                if (nRet == -1)
                {
                    this.comboBox_biblioDbName.Text = value;
                }
                else
                {
                    this.comboBox_biblioDbName.Text = value.Substring(0, nRet);
                    this.textBox_recordID.Text = value.Substring(nRet+1);
                }
            }
        }

        public string RecID
        {
            get
            {
                return this.textBox_recordID.Text;
            }
            set
            {
                this.textBox_recordID.Text = value;
            }
        }

        public bool BuildLink
        {
            get
            {
                return this.checkBox_buildLink.Checked;
            }
            set
            {
                this.checkBox_buildLink.Checked = value;
            }
        }

        public bool CopyChildRecords
        {
            get
            {
                return this.checkBox_copyChildRecords.Checked;
            }
            set
            {
                this.checkBox_copyChildRecords.Checked = value;
            }
        }

        public bool EnableCopyChildRecords
        {
            get
            {
                return this.checkBox_copyChildRecords.Enabled;
            }
            set
            {
                this.checkBox_copyChildRecords.Enabled = value;
            }
        }

        private void comboBox_biblioDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_biblioDbName.Items.Count > 0)
                return;

            if (Program.MainForm.BiblioDbProperties == null)
                return;

            for (int i = 0; i < Program.MainForm.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];
                // 只允许特定的 MARC 格式
                if (string.IsNullOrEmpty(this.MarcSyntax) == false
                    && property.Syntax != this.MarcSyntax)
                    continue;
                this.comboBox_biblioDbName.Items.Add(property.DbName);
            }
        }

        // combobox内的库名选择变化后，记录ID变化为"?"
        private void comboBox_biblioDbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.textBox_recordID.Text = "?";
        }

        private void textBox_recordID_Validating(object sender, CancelEventArgs e)
        {
            if (this.textBox_recordID.Text != "?"
                && StringUtil.IsPureNumber(this.textBox_recordID.Text) == false)
            {
                MessageBox.Show(this, "记录ID '"+this.textBox_recordID.Text+"' 不合法。必须为'?'或纯数字(注:均需为半角)");
                e.Cancel = true;
                return;
            }
        }

        private void comboBox_biblioDbName_TextChanged(object sender, EventArgs e)
        {
            string strError = "";

            // 如果另存前的记录路径中ID为问号，意味着不知道目标路径，因此无法创建目标关系
            // 所以要检查目标路径，ID应不为问号
            int nRet = Program.MainForm.CheckBuildLinkCondition(
                    this.RecPath,   // 要另存去的那条
                    this.CurrentBiblioRecPath,  // 另存前的那条
                    true,
                    out strError);
            // 看看是否需要Enable/Diable连接checkbox
            if (nRet == 1)
            {
                // 尽量恢复最初的勾选，如果可能的话
                if (this.m_bSavedBuildLink == true)
                {
                    this.m_nManual++;
                    this.checkBox_buildLink.Checked = true;
                    this.m_nManual--;
                }

                this.checkBox_buildLink.Enabled = true;

                this.label_buildLinkMessage.Text = "";
            }
            else
            {
                this.m_nManual++;
                this.checkBox_buildLink.Checked = false;
                this.m_nManual--;

                this.checkBox_buildLink.Enabled = false;

                this.label_buildLinkMessage.Text = strError;
            }

            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        private void checkBox_buildLink_CheckedChanged(object sender, EventArgs e)
        {
            if (this.m_nManual == 0)
            {
                // 如果手动在界面明确设置了off
                if (this.checkBox_buildLink.Checked == false
                    && this.m_bSavedBuildLink == true)
                    this.m_bSavedBuildLink = false; // 不再坚持恢复
            }
        }

        void TrySetRecPathFromClipboard()
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                return;

            string strText = (string)ido.GetData(DataFormats.UnicodeText);
            if (string.IsNullOrEmpty(strText) == true)
                return;

            if (IsRecPath(strText) == false)
                return;

            this.RecPath = strText;
        }

        // 判断一个字符串是否为记录路径
        static bool IsRecPath(string strText)
        {
            if (strText.IndexOf("/") == -1)
                return false;
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "/", out strLeft, out strRight);
            if (string.IsNullOrEmpty(strLeft) == true)
                return false;
            if (strRight == "?")
                return true;
            if (StringUtil.IsPureNumber(strRight) == false)
                return false;
            return true;
        }
    }
}