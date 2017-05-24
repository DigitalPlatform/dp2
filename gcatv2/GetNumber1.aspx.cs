using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GcatClient;
using DigitalPlatform.GcatClient.gcat_new_ws;

namespace gcatv2
{
    public partial class GetNumber : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                if (this.Request.Cookies["id"] != null)
                    this.HiddenField_id.Value = this.Request.Cookies["id"].Value;
                if (this.Request.Cookies["memo_id"] != null)
                    this.HiddenField_memoID.Value = this.Request.Cookies["memo_id"].Value;

                string strAction = this.Request["action"];
                if (string.Compare(strAction, "clearid", true) == 0)
                {
                    this.Response.Cookies["id"].Value = "";
                    this.Response.Cookies["memo_id"].Value = "no";
                    this.Response.Cookies["id"].Expires = DateTime.Now.AddDays(30);
                    this.Response.Cookies["memo_id"].Expires = DateTime.Now.AddDays(30);

                    this.HiddenField_id.Value = "";
                    this.HiddenField_memoID.Value = "no";

                }

                string strUrl = System.Configuration.ConfigurationManager.AppSettings["gcatserverurl"];
                if (string.IsNullOrEmpty(strUrl) == true)
                {
                    this.Label_errorInfo.Text = HttpUtility.HtmlEncode("web.config中<configuration>/<appSessings>/<add key='gcatserverurl' value='???'>尚未配置");
                }

            }

            // this.Button_continue.OnClientClick = "alert('test'); $( 'questiondialogform' ).parent().appendTo($('form:first'));$( '#questiondialogform' ).dialog('close');alert('test 2');";
            // this.Button_continue.OnClientClick = "alert('test'); $( 'questiondialogform' ).parent().appendTo($(\"form:first\"));$( '#questiondialogform' ).dialog('close');";

            /*
            LiteralControl literal = new LiteralControl();
            literal.Text = "<script type=\"text/javascript\" language=\"javascript\"> function OpenDialog() {; } </script>";
            while (this.PlaceHolder_script.Controls.Count > 0)
                this.PlaceHolder_script.Controls.RemoveAt(0);
            this.PlaceHolder_script.Controls.Add(literal);
             * */

            /*
            if (string.IsNullOrEmpty(this.TextBox_answer.Text) == false)
                Button_continue_Click(null, null);
             * */
        }

        protected void Button_get_Click(object sender, System.EventArgs e)
        {
            this.TextBox_number.Text = "";
            this.Label_debugInfo.Text = "";
            if (this.Label_debugInfo.Text == "")
                this.Panel_debuginfo.Visible = false;
            else
                this.Panel_debuginfo.Visible = true;

            this.HiddenField_questions.Value = "";
            this.TextBox_answer.Text = "";
            this.Label_question.Text = "";

            DoGetNumber();
        }

        void PrepareQustionDialog(Question[] questions,
            string strError)
        {
            this.PlaceHolder_questionDialog.Visible = true;

            this.PlaceHolder_loginDialog.Visible = false;   // 避免另外一个对话框出现在背景

            string strQuestion = questions[questions.Length - 1].Text;

            this.HiddenField_questions.Value = SaveQuestions(questions);

            string strTitle = strError;
            this.Label_question.Text = strQuestion.Replace("\r\n", "<br/>");

            this.questiondialogform.ToolTip = strError;

            // 令对话框自动启动
            LiteralControl literal = new LiteralControl();
            literal.Text = "<script type=\"text/javascript\" language=\"javascript\">"
            + "$(document).ready( function () { $( '#questiondialogform' ).dialog({ modal: true }); $( '#questiondialogform' ).parent().appendTo($('form:first')); })"
                //+" function cancelClick() { if (window.event) window.event.cancelBubble = true; return false; }  "
            + "</script>";
            while (this.PlaceHolder_script.Controls.Count > 0)
                this.PlaceHolder_script.Controls.RemoveAt(0);

            this.PlaceHolder_script.Controls.Add(literal);
        }

        protected void Button_continue_Click(object sender, EventArgs e)
        {
            DoGetNumber();
        }

        void DoGetNumber()
        {
            if (string.IsNullOrEmpty(this.TextBox_author.Text) == true)
            {
                this.Label_errorInfo.Text = "请输入著者";
                return;
            }

            Question[] questions = RestoreQuestions(this.HiddenField_questions.Value);
            if (questions.Length > 0)
            {
                Question q = questions[questions.Length - 1];
                q.Answer = this.TextBox_answer.Text;
            }

            if (this.HiddenField_memoID.Value == "yes")
            {
                this.Response.Cookies["id"].Value = this.HiddenField_id.Value;
                this.Response.Cookies["memo_id"].Value = this.HiddenField_memoID.Value;
            }
            else
            {
                this.Response.Cookies["id"].Value = "";
                this.Response.Cookies["memo_id"].Value = "no";
            }
            this.Response.Cookies["id"].Expires = DateTime.Now.AddDays(30);
            this.Response.Cookies["memo_id"].Expires = DateTime.Now.AddDays(30);

            string strNumber = "";
            string strDebugInfo = "";
            string strError = "";

            int nRet = 0;

            string strUrl = System.Configuration.ConfigurationManager.AppSettings["gcatserverurl"];

            // return:
            //		-3	需要回答问题
            //      -2  strID验证失败
            //      -1  出错
            //      0   成功
            nRet = GcatNew.GetNumber(null,
                strUrl, // "http://localhost/gcatserver/",
                this.HiddenField_id.Value, // strID,
                this.TextBox_author.Text,
                this.CheckBox_selectPinyin.Checked,
                this.CheckBox_selectEntry.Checked,
                this.CheckBox_outputDebugInfo.Checked,
            ref questions,
            out strNumber,
            out strDebugInfo,
            out strError);
            if (nRet == -3)
            {
                PrepareQustionDialog(questions, strError);
                return;
            }

            this.TextBox_answer.Text = "";
            this.PlaceHolder_questionDialog.Visible = false;

            if (nRet == -2)
            {
                PrepareLoginDialog(strError);
                return;
            }

            this.PlaceHolder_loginDialog.Visible = false;


            if (nRet == 0)
            {
                this.TextBox_number.Text = strNumber;

                if (string.IsNullOrEmpty(strDebugInfo) == false)
                    this.Label_debugInfo.Text = "<b>调试信息:</b><br/>" + GetHtmlString(strDebugInfo);
                else
                    this.Label_debugInfo.Text = "";

                if (this.Label_debugInfo.Text == "")
                    this.Panel_debuginfo.Visible = false;
                else
                    this.Panel_debuginfo.Visible = true;

                this.Label_debugInfo.Visible = true;
                return;
            }

            if (nRet == -1)
            {
                this.Label_errorInfo.Text = strError;
                return;
            }

            if (nRet == -2)
            {
                // id验证失败
            }

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

        // 从字符串中还原Question数组
        static Question[] RestoreQuestions(string strText)
        {
            string[] parts = strText.Split(new char[] { '|' });
            Question[] results = new Question[parts.Length / 2];
            for (int i = 0; i < parts.Length / 2; i++)
            {
                Question question = new Question();
                question.Text = parts[i * 2];
                question.Answer = parts[(i * 2) + 1];
                results[i] = (question);
            }

            return results;
        }

        // 将Question数组变换为字符串形态
        static string SaveQuestions(Question[] questions)
        {
            string strResult = "";
            foreach (Question q in questions)
            {
                if (string.IsNullOrEmpty(strResult) == false)
                    strResult += "|";

                strResult += q.Text + "|" + q.Answer;
            }

            return strResult;
        }

        void PrepareLoginDialog(string strError)
        {
            this.PlaceHolder_loginDialog.Visible = true;

            this.PlaceHolder_questionDialog.Visible = false;   // 避免另外一个对话框出现在背景

            this.logindialogform.ToolTip = strError + "。请重新输入ID，然后继续";
            this.TextBox_id.Text = this.HiddenField_id.Value;
            this.CheckBox_memoID.Checked = (this.HiddenField_memoID.Value == "yes" ? true : false);

            // 令对话框自动启动
            LiteralControl literal = new LiteralControl();
            literal.Text = "<script type=\"text/javascript\" language=\"javascript\">"
            + "$(document).ready( function () { $( '#logindialogform' ).dialog({ modal: true }); $( '#logindialogform' ).parent().appendTo($('form:first')); })"
            + "</script>";
            while (this.PlaceHolder_script.Controls.Count > 0)
                this.PlaceHolder_script.Controls.RemoveAt(0);

            this.PlaceHolder_script.Controls.Add(literal);
        }

        protected void Button_login_Click(object sender, EventArgs e)
        {
            this.HiddenField_id.Value = this.TextBox_id.Text;
            this.HiddenField_memoID.Value = (this.CheckBox_memoID.Checked == true ? "yes" : "no");
            DoGetNumber();
        }
    }
}