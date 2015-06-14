using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.LibraryServer;

namespace dp2Circulation
{
    internal partial class IssueDialog : Form
    {
        // public object Tag = null;   // 携带任何对象
        /// <summary>
        /// 查重事件
        /// </summary>
        public event CheckDupEventHandler CheckDup = null;

        public IssueDialog()
        {
            InitializeComponent();
        }

        private void IssueDialog_Load(object sender, EventArgs e)
        {

        }

        private void IssueDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IssueDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_publishTime.Text == "")
            {
                strError = "尚未输入出版时间";
                goto ERROR1;
            }

            // 检查出版时间格式是否正确
            int nRet = LibraryServerUtil.CheckSinglePublishTime(this.textBox_publishTime.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.textBox_issue.Text == "")
            {
                strError = "尚未输入期号";
                goto ERROR1;
            }

            // 检查期号格式是否正确
            nRet = VolumeInfo.CheckIssueNo(
                "期号",
                this.textBox_issue.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查总期号格式是否正确
            if (String.IsNullOrEmpty(this.textBox_zong.Text) == false)
            {
                nRet = VolumeInfo.CheckIssueNo(
                    "总期号",
                    this.textBox_zong.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 检查卷号格式是否正确
            if (String.IsNullOrEmpty(this.textBox_volume.Text) == false)
            {
                nRet = VolumeInfo.CheckIssueNo(
                    "卷号",
                    this.textBox_volume.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            if (this.CheckDup != null)
            {
                CheckDupEventArgs e1 = new CheckDupEventArgs();
                e1.PublishTime = this.PublishTime;
                e1.Issue = this.Issue;
                e1.Zong = this.Zong;
                e1.Volume = this.Volume;
                e1.EnsureVisible = true;
                this.CheckDup(this, e1);

                if (e1.DupIssues.Count > 0)
                {
                    // 将重复的期滚入视野

                    MessageBox.Show(this, e1.DupInfo);
                    return;
                }

                if (e1.WarningIssues.Count > 0)
                {
                    // 将警告的的期滚入视野

                    DialogResult dialog_result = MessageBox.Show(this,
            "警告: " + e1.WarningInfo + "\r\n\r\n是否继续?\r\n\r\n(OK: 不理会警告，继续进行后续操作; Cancel: 返回对话框进行修改",
            "BindingControls",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                    if (dialog_result == DialogResult.Cancel)
                        return;
                }
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

        public string PublishTime
        {
            get
            {
                return this.textBox_publishTime.Text;
            }
            set
            {
                this.textBox_publishTime.Text = value;
            }
        }

        public string Issue
        {
            get
            {
                return this.textBox_issue.Text;
            }
            set
            {
                this.textBox_issue.Text = value;
            }
        }

        public string Zong
        {
            get
            {
                return this.textBox_zong.Text;
            }
            set
            {
                this.textBox_zong.Text = value;
            }
        }

        public string Volume
        {
            get
            {
                return this.textBox_volume.Text;
            }
            set
            {
                this.textBox_volume.Text = value;
            }
        }

        public string EditComment
        {
            get
            {
                return this.textBox_editComment.Text;
            }
            set
            {
                this.textBox_editComment.Text = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }
    }

    /// <summary>
    /// 查重事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    internal delegate void CheckDupEventHandler(object sender,
        CheckDupEventArgs e);

    /// <summary>
    /// 查重事件的参数
    /// </summary>
    internal class CheckDupEventArgs : EventArgs
    {
        // [in]
        /// <summary>
        /// [in] 出版时间
        /// </summary>
        public string PublishTime = "";
        /// <summary>
        /// [in] 当年期号
        /// </summary>
        public string Issue = "";
        /// <summary>
        /// [in] 总期号
        /// </summary>
        public string Zong = "";
        /// <summary>
        /// [in] 卷号
        /// </summary>
        public string Volume = "";

        /// <summary>
        /// [in] 是否要确保选定的事项可见
        /// </summary>
        public bool EnsureVisible = false;

        // [out]
        /// <summary>
        /// [out] 返回重复信息
        /// </summary>
        public string DupInfo = "";
        /// <summary>
        /// [out] 返回发生重复的期对象集合
        /// </summary>
        public List<IssueBindingItem> DupIssues = new List<IssueBindingItem>();
        /// <summary>
        /// [out] 返回警告信息
        /// </summary>
        public string WarningInfo = "";
        /// <summary>
        /// [out] 返回发生警告的期对象集合
        /// </summary>
        public List<IssueBindingItem> WarningIssues = new List<IssueBindingItem>();
    }
}