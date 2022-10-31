using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 消息管理窗口
    /// </summary>
    public partial class MessageForm : MyForm
    {
        public MessageForm()
        {
            InitializeComponent();
        }

        private void MessageForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
        }

        private void MessageForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MessageForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        const int COLUMN_ID = 0;
        const int COLUMN_SENDER = 1;
        const int COLUMN_RECIPIENT = 2;
        const int COLUMN_SUBJECT = 3;
        const int COLUMN_DATE = 4;
        const int COLUMN_SIZE = 5;


        // 装入一个信箱的全部消息
        int LoadMessages(string strBox,
            out string strError)
        {
            strError = "";

            this.listView_message.Items.Clear();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在装入消息 ...");
            _stop.BeginLoop();

            EnableControls(false);

            try
            {
                int nTotalCount = 0;

                MessageData[] messages = null;

                long nRet = this.Channel.ListMessage(
                    "search", // true,
                    "message",
                    strBox,
                    MessageLevel.Summary,
                    0,
                    0,
                    out nTotalCount,
                    out messages,
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                int nStart = 0;
                int nMax = -1;
                for (; ; )
                {
                    nRet = this.Channel.ListMessage(
        "", // false,
        "message",
        strBox,
        MessageLevel.Summary,
        nStart,
        nMax,
        out nTotalCount,
        out messages,
        out strError);
                    if (nRet == -1)
                        return -1;

                    // 装入浏览列表
                    foreach (MessageData message in messages)
                    {
                        ListViewItem item = new ListViewItem();
                        ListViewUtil.ChangeItemText(item, COLUMN_ID, message.strRecordID);
                        ListViewUtil.ChangeItemText(item, COLUMN_SENDER, message.strSender);
                        ListViewUtil.ChangeItemText(item, COLUMN_RECIPIENT, message.strRecipient);
                        ListViewUtil.ChangeItemText(item, COLUMN_SUBJECT, message.strSubject);
                        ListViewUtil.ChangeItemText(item, COLUMN_DATE, DateTimeUtil.LocalTime(message.strCreateTime, "yyyy-MM-dd HH:mm:ss"));
                        ListViewUtil.ChangeItemText(item, COLUMN_SIZE, message.strSize);

                        this.listView_message.Items.Add(item);
                    }

                    nStart += messages.Length;
                    if (nStart >= nTotalCount)
                        break;
                }
            }
            finally
            {
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
            }

            return 0;
        }

        private void comboBox_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = LoadMessages(this.comboBox_box.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this.comboBox_box.Enabled = bEnable;
            }));
        }

    }
}
