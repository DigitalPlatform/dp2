using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace DigitalPlatform.CommonControl
{
    public partial class DateControl : UserControl
    {
        string m_strCaption = "";

        int IngoreTextChange = 0;

        [Category("New Event")]
        public event EventHandler DateTextChanged = null;

        public DateControl()
        {
            InitializeComponent();
        }

        [Category("Appearance")]
        [DescriptionAttribute("Caption")]
        [DefaultValue(typeof(string), "")]
        public string Caption
        {
            get
            {
                return this.m_strCaption;
            }
            set
            {
                this.m_strCaption = value;
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "None")]
        public new BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        private void maskedTextBox_date_TextChanged(object sender, EventArgs e)
        {
            if (IngoreTextChange > 0)
                return;

            if (this.DateTextChanged != null)
            {
                this.DateTextChanged(sender, e);
            }

            // this.OnTextChanged(e);
        }

        public override string Text
        {
            get
            {
                return this.maskedTextBox_date.Text;
            }
            set
            {
                this.maskedTextBox_date.Text = value;
            }
        }


        // 当前value是否为空值
        public bool IsValueNull()
        {
            if (this.Value == new DateTime((long)0))
                return true;
            return false;
        }


        public DateTime Value
        {
            get
            {
                // get pure text
                string strPureText = GetPureDateText();

                if (strPureText.Trim() == "")
                    return new DateTime((long)0);

                try
                {
                    DateTime date = DateTime.Parse(this.maskedTextBox_date.Text,
                        this.maskedTextBox_date.Culture,
                        DateTimeStyles.NoCurrentDateDefault);
                    return date;
                }
                catch
                {
                    return new DateTime((long)0);
                }

            }
            set
            {
                if (value == new DateTime((long)0))
                {
                    this.maskedTextBox_date.Text = "";
                    return;
                }

                this.maskedTextBox_date.Text = GetDateString(value);
            }
        }

        static string GetDateString(DateTime date)
        {
            return date.Year.ToString().PadLeft(4, '0') + "年"
                + date.Month.ToString().PadLeft(2, '0') + "月"
                + date.Day.ToString().PadLeft(2, '0') + "日";
        }

        string GetPureDateText()
        {
            // get pure text
            MaskFormat oldformat = this.maskedTextBox_date.TextMaskFormat;

            this.IngoreTextChange++;

            this.maskedTextBox_date.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            string strPureText = this.maskedTextBox_date.Text;
            this.maskedTextBox_date.TextMaskFormat = oldformat;

            this.IngoreTextChange--;

            return strPureText;
        }

        private void maskedTextBox_date_Validating(object sender, CancelEventArgs e)
        {

            string strPureText = GetPureDateText();

            if (strPureText.Trim() == "")
                return; // blank value

            try
            {
                DateTime date = DateTime.Parse(this.maskedTextBox_date.Text,
                    this.maskedTextBox_date.Culture,
                    DateTimeStyles.NoCurrentDateDefault);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, this.Text + " error : " + ex.Message);
                e.Cancel = true;
            }

        }

        private void button_findDate_Click(object sender, EventArgs e)
        {
            GetDateDlg dlg = new GetDateDlg();

            dlg.Text = this.Caption;
            dlg.DateTime = this.Value;
            // dlg.StartLocation = Control.MousePosition;

            dlg.StartPosition = FormStartPosition.Manual;
            dlg.Location = this.PointToScreen(
                new Point(0,
                0 + this.Size.Height)
                );
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.Value = dlg.DateTime;
        }

    }
}
