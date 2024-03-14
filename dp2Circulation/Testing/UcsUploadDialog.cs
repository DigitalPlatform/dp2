using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using UcsUpload;

namespace dp2Circulation.Testing
{
    public partial class UcsUploadDialog : Form
    {
        public UcsUploadDialog()
        {
            InitializeComponent();
        }

        private void UcsUploadDialog_Load(object sender, EventArgs e)
        {

        }

        private void UcsUploadDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private async void button_upload_Click(object sender, EventArgs e)
        {
            string strError = "";
            string action = this.comboBox_action.Text;
            var parts = StringUtil.ParseTwoPart(action, " ");
            action = parts[0];

            if (action != "N" && action != "U")
            {
                strError = "动作不合法。应为 N 或 U";
                goto ERROR1;
            }

            this.button_upload.Enabled = false;
            try
            {
                var context = Program.MainForm;
                var result = await UcsUtility.Upload(context.UcsApiUrl,
                    context.UcsDatabaseName,
                    "chi",
                    context.UcsUserName,
                    context.UcsPassword,
                    action,
                    null,
                    this.textBox_record.Text,
                    "MARCXML",
                    "BK");
                if (result.Value == -1)
                    MessageBox.Show(this, result.ErrorInfo);
                else
                {
                    if (result.AreUcsSucceed)
                    {
                        MessageBox.Show(this, result.olcc);
                    }
                    else
                        MessageBox.Show(this, result.UcsErrorInfo);
                }

                return;
            }
            finally
            {
                this.button_upload.Enabled = true;
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_pasteFromJinei_Click(object sender, EventArgs e)
        {
            string strError = "";

            var strMARC = Clipboard.GetText(TextDataFormat.UnicodeText);
            strMARC = strMARC
                .Replace('_', '^')
                .Replace('$', (char)Record.SUBFLD)
                .Replace('#', (char)Record.FLDEND)
                .Replace('*', (char)Record.RECEND);

            int nRet = MarcUtil.Marc2Xml(strMARC,
                "usmarc",
                out string xml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_record.Text = DomUtil.GetIndentXml(xml);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 更新上载前记录的 049 字段
        // parameters:
        //      olcc_string 内容举例 $$aA000001TES$$bUCS03000000007$$c004471482$$dNLC01
        void Update049(string olcc_string)
        {
            string strError = "";

            var xml = this.textBox_record.Text;
            int nRet = MarcUtil.Xml2Marc(xml,
                true,
                "usmarc",
                out string syntax,
                out string strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MarcRecord record = new MarcRecord(strMARC);
            var fields = record.select("field[@name='049']");
            if (fields.count > 1)
            {
                // 删除多余的 049
                for (int i = 1; i < fields.count; i++)
                {
                    var field = fields[i];
                    field.detach();
                }
            }

            if (string.IsNullOrEmpty(olcc_string) == false)
                record.setFirstField("049", "  ", olcc_string.Replace("$$", MarcQuery.SUBFLD));

            nRet = MarcUtil.Marc2Xml(record.Text,
                "usmarc",
                out string changed_xml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            this.textBox_record.Text = DomUtil.GetIndentXml(changed_xml);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
