using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.LibraryClient;
using Newtonsoft.Json;

namespace WebApplication1
{
    public partial class gcat : MyWebPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if (WebUtil.PrepareEnvironment(this,
    ref app,
    ref sessioninfo) == false)
                return;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
                return;

            if (this.IsPostBack == false)
                this.Clear();
        }

        protected void Button_get_Click(object sender, System.EventArgs e)
        {
            string strNumber = "";
            string strDebugInfo = "";
            string strError = "";

            long nRet = 0;

            this.TextBox_number.Text = "";
            this.Label_debugInfo.Text = "";
            this.Label_debugInfo.Visible = false;

            Question[] questions = JsonConvert.DeserializeObject<Question[]>(this.hidden_questions.Value);

            if (questions != null && string.IsNullOrEmpty(this.TextBox_answer.Text) == false)
            {
                Question question = questions[questions.Length - 1];
                question.Answer = this.TextBox_answer.Text;
            }

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                // return:
                //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
                //		-3	需要回答问题
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                nRet = channel.GetAuthorNumber(this.TextBox_author.Text,
                    this.CheckBox_selectPinyin.Checked,
                    this.CheckBox_selectEntry.Checked,
                    this.CheckBox_outputDebugInfo.Checked,
                    ref questions,
                    out strNumber,
                    out strDebugInfo,
                    out strError);
                if (nRet == 0)
                {
                    this.Clear();

                    this.TextBox_number.Text = strNumber;
                    this.Panel_debuginfo.Visible = this.CheckBox_outputDebugInfo.Checked;
                    this.Label_debugInfo.Text = "<b>调试信息:</b><br/>" + GetHtmlString(strDebugInfo);
                    this.Label_debugInfo.Visible = true;
                }
                else if (nRet == -3)
                {
                    Debug.Assert(nRet == -3, "");	// 需要回答问题

                    EnableEdit(false);
                    this.Panel_debuginfo.Visible = false;

                    this.question_frame.Visible = true;

                    this.hidden_questions.Value = JsonConvert.SerializeObject(questions);

                    Question question = questions[questions.Length - 1];
                    this.Label_questionText.Text = GetHtmlString(question.Text);
                }
                else
                {
                    this.Clear();
                    this.Label_errorInfo.Text = strError;
                    this.Panel_debuginfo.Visible = false;
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }
        }

        void EnableEdit(bool bEnable)
        {
            this.TextBox_number.Enabled = bEnable;
            this.TextBox_number.ReadOnly = !bEnable;
            this.Button_get.Enabled = bEnable;
            this.CheckBox_outputDebugInfo.Enabled = bEnable;
            this.CheckBox_selectEntry.Enabled = bEnable;
            this.CheckBox_selectPinyin.Enabled = bEnable;
        }

        void Clear()
        {
            EnableEdit(true);
            this.Label_errorInfo.Text = "";
            this.TextBox_answer.Text = "";
            this.question_frame.Visible = false;
            this.hidden_questions.Value = "";
            this.TextBox_number.Text = "";
            this.TextBox_number.ReadOnly = true;
        }

        public static string GetHtmlString(string strText)
        {
            string[] lines = strText.Split(new char[] { '\n' });

            string strResult = "";

            for (int i = 0; i < lines.Length; i++)
            {
                strResult += HttpUtility.HtmlEncode(lines[i]) + "<br/>";
            }

            return strResult;
        }
    }
}