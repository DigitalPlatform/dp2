using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class Field856Dialog : Form
    {
        // $8内容有初始值，要求follow填充其它几个相关子字段内容，则需要在打开对话框前设置该成员为true
        public bool AutoFollowIdSet = false;

        public event GetResInfoEventHandler GetResInfo = null;

        string m_strReserve = "";   // 其他没有被模板定义的子字段内容

        public Field856Dialog()
        {
            InitializeComponent();

            FillTypeList();
        }

        void FillTypeList()
        {
            this.tabComboBox_type.Items.Add("封面图像");
            this.tabComboBox_type.Items.Add("封面图像.小");
            this.tabComboBox_type.Items.Add("封面图像.中");
            this.tabComboBox_type.Items.Add("封面图像.大");
        }

        static string GetTypeString(string strCaption)
        {
            if (strCaption == "封面图像")
                return "type:FrontCover";
            if (strCaption == "封面图像.小")
                return "type:FrontCover.SmallImage";
            if (strCaption == "封面图像.中")
                return "type:FrontCover.MediumImage";
            if (strCaption == "封面图像.大")
                return "type:FrontCover.LargeImage";
            return "";
        }

        public string MessageText
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;

                if (this.textBox_message.Text == "")
                {
                    this.textBox_message.Visible = false;
                    this.splitContainer_main.Panel1Collapsed = true;
                }
                else
                {
                    this.textBox_message.Visible = true;
                    this.splitContainer_main.Panel1Collapsed = false;
                }
            }
        }

        public string Value
        {
            get
            {
                string strError = "";
                string strValue = "";
                int nRet = GetValue(out strValue,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                return strValue;
            }
            set
            {
                string strError = "";
                int nRet = this.SetValue(value,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }
        }

        private void Field856Dialog_Load(object sender, EventArgs e)
        {
            this.comboBox_indicator1.Items.Add("0\tEmail");
            this.comboBox_indicator1.Items.Add("1\tFTP");
            this.comboBox_indicator1.Items.Add("2\tTelnet");
            this.comboBox_indicator1.Items.Add("3\tDial-up");
            this.comboBox_indicator1.Items.Add("4\tHTTP");
            this.comboBox_indicator1.Items.Add("7\t其它方法");

            this.comboBox_indicator2.Items.Add(" \t未指明");
            this.comboBox_indicator2.Items.Add("0\t资源对象");
            this.comboBox_indicator2.Items.Add("1\t资源的其它版本");
            this.comboBox_indicator2.Items.Add("2\t相关资源");
            this.comboBox_indicator2.Items.Add("8\t不产生前导语");

            // 如果$8内容有初始值，并且明确要求了follow填充其它几个相关子字段内容
            if (this.AutoFollowIdSet == true
                && this.comboBox_u.Text != "")
            {
                comboBox_u_SelectedIndexChanged(null, null);
            }

            this.MessageText = this.MessageText;
        }

        private void Field856Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Field856Dialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


        #region 外部可以直接访问每个子字段内容的textbox

        public string Subfield_f
        {
            get
            {
                return this.textBox_f.Text;
            }
            set
            {
                this.textBox_f.Text = value;
            }
        }

        public string Subfield_q
        {
            get
            {
                return this.textBox_q.Text;
            }
            set
            {
                this.textBox_q.Text = value;
            }
        }

        public string Subfield_s
        {
            get
            {
                return this.textBox_s.Text;
            }
            set
            {
                this.textBox_s.Text = value;
            }
        }

        public string Subfield_u
        {
            get
            {
                return this.comboBox_u.Text;
            }
            set
            {
                this.comboBox_u.Text = value;
            }
        }
        public string Subfield_x
        {
            get
            {
                return this.textBox_x.Text;
            }
            set
            {
                this.textBox_x.Text = value;
            }
        }

        public string Subfield_y
        {
            get
            {
                return this.textBox_y.Text;
            }
            set
            {
                this.textBox_y.Text = value;
            }
        }

        public string Subfield_z
        {
            get
            {
                return this.textBox_z.Text;
            }
            set
            {
                this.textBox_z.Text = value;
            }
        }


        public string Subfield_2
        {
            get
            {
                return this.textBox_2.Text;
            }
            set
            {
                this.textBox_2.Text = value;
            }
        }
        public string Subfield_3
        {
            get
            {
                return this.textBox_3.Text;
            }
            set
            {
                this.textBox_3.Text = value;
            }
        }

        public string Subfield_8
        {
            get
            {
                return this.textBox_8.Text;
            }
            set
            {
                this.textBox_8.Text = value;
            }
        }

        #endregion

        void Clear()
        {
            this.comboBox_indicator1.Text = " ";
            this.comboBox_indicator2.Text = " ";

            this.textBox_f.Text = "";
            this.textBox_q.Text = "";
            this.textBox_s.Text = "";
            this.comboBox_u.Text = "";
            this.textBox_x.Text = "";
            this.textBox_y.Text = "";
            this.textBox_z.Text = "";
            this.textBox_2.Text = "";
            this.textBox_3.Text = "";
            this.textBox_8.Text = "";

            this.m_strReserve = "";
        }

        int GetValue(
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            if (this.comboBox_indicator1.Text.Length != 1)
            {
                strError = "指示符1必须为1字符";
                return -1;
            }

            if (this.comboBox_indicator2.Text.Length != 1)
            {
                strError = "指示符2必须为1字符";
                return -1;
            }

            strResult += this.comboBox_indicator1.Text;
            strResult += this.comboBox_indicator2.Text;

            // $f
            if (String.IsNullOrEmpty(this.textBox_f.Text) == false)
                strResult += new string((char)31,1)
                    + "f"
                    + this.textBox_f.Text;

            // $q
            if (String.IsNullOrEmpty(this.textBox_q.Text) == false)
                strResult += new string((char)31, 1)
                    + "q"
                    + this.textBox_q.Text;

            // $s
            if (String.IsNullOrEmpty(this.textBox_s.Text) == false)
                strResult += new string((char)31, 1)
                    + "s"
                    + this.textBox_s.Text;

            // $u
            if (String.IsNullOrEmpty(this.comboBox_u.Text) == false)
                strResult += new string((char)31, 1)
                    + "u"
                    + this.comboBox_u.Text;

            // $x
            if (String.IsNullOrEmpty(this.textBox_x.Text) == false)
                strResult += new string((char)31, 1)
                    + "x"
                    + this.textBox_x.Text;

            // $y
            if (String.IsNullOrEmpty(this.textBox_y.Text) == false)
                strResult += new string((char)31, 1)
                    + "y"
                    + this.textBox_y.Text;

            // $z
            if (String.IsNullOrEmpty(this.textBox_z.Text) == false)
                strResult += new string((char)31, 1)
                    + "z"
                    + this.textBox_z.Text;

            // $2
            if (String.IsNullOrEmpty(this.textBox_2.Text) == false)
                strResult += new string((char)31, 1)
                    + "2"
                    + this.textBox_2.Text;

            // $3
            if (String.IsNullOrEmpty(this.textBox_3.Text) == false)
                strResult += new string((char)31, 1)
                    + "3"
                    + this.textBox_3.Text;

            // $8
            if (String.IsNullOrEmpty(this.textBox_8.Text) == false)
                strResult += new string((char)31, 1)
                    + "8"
                    + this.textBox_8.Text;

            // 其他没有显示的
            if (String.IsNullOrEmpty(this.m_strReserve) == false)
                strResult += m_strReserve;

            return 0;
        }

        // parameters:
        //      strValue    第一、第二字符为指示符，后面为字段内容
        int SetValue(string strValue,
            out string strError)
        {
            strError = "";

            this.Clear();

            char chIndicator1 = ' ';
            char chIndicator2 = ' ';

            if (strValue.Length >= 1)
                chIndicator1 = strValue[0];

            if (strValue.Length >= 2)
                chIndicator2 = strValue[1];

            this.comboBox_indicator1.Text = new string(chIndicator1, 1);
            this.comboBox_indicator2.Text = new string(chIndicator2, 1);

            if (strValue.Length <= 2)
                return 0;

            // 去掉字段指示符2字符
            strValue = strValue.Substring(2);

            this.textBox_f.Text = GetSubfield(ref strValue, 'f');
            this.textBox_q.Text = GetSubfield(ref strValue, 'q');
            this.textBox_s.Text = GetSubfield(ref strValue, 's');
            this.comboBox_u.Text = GetSubfield(ref strValue, 'u');
            this.textBox_x.Text = GetSubfield(ref strValue, 'x');
            this.textBox_y.Text = GetSubfield(ref strValue, 'y');
            this.textBox_z.Text = GetSubfield(ref strValue, 'z');
            this.textBox_2.Text = GetSubfield(ref strValue, '2');
            this.textBox_3.Text = GetSubfield(ref strValue, '3');
            this.textBox_8.Text = GetSubfield(ref strValue, '8');

            this.m_strReserve = strValue;

            return 0;
        }

        // 从字符串中抽取一个子字段内容
        // return:
        //      ""  没有找到
        //      其他    子字段内容，不包括子字段名(一个字符)。
        static string GetSubfield(ref string strValue,
            char chSubfieldName)
        {
            if (String.IsNullOrEmpty(strValue) == true)
                return "";

            bool bOn = false;
            for (int i = 0; i < strValue.Length; i++)
            {
                char ch = strValue[i];

                if (bOn == true)
                {
                    if (chSubfieldName == ch)
                    {
                        int nStart = i - 1;
                        string strResult = strValue.Substring(i + 1);
                        int nRet = strResult.IndexOf((char)31);
                        if (nRet != -1)
                        {
                            strResult = strResult.Substring(0, nRet);
                            strValue = strValue.Remove(nStart, nRet + 2);
                        }
                        else
                        {
                            strValue = strValue.Substring(0, nStart);
                        }
                        return strResult;
                    }
                }

                if (ch == (char)31)
                    bOn = true;
                else
                    bOn = false;
            }

            return "";
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 校验
            string strError = "";

            if (this.comboBox_indicator1.Text.Length != 1)
            {
                strError = "指示符1必须为1字符";
                goto ERROR1;
            }

            if (this.comboBox_indicator2.Text.Length != 1)
            {
                strError = "指示符2必须为1字符";
                goto ERROR1;
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

        private void comboBox_indicator1_Validating(object sender,
            CancelEventArgs e)
        {
            if (this.comboBox_indicator1.Text.Length != 1)
            {
                MessageBox.Show(this, "指示符1内容必须为1字符");
                e.Cancel = true;
            }

        }

        private void comboBox_indicator2_Validating(object sender, CancelEventArgs e)
        {
            if (this.comboBox_indicator2.Text.Length != 1)
            {
                MessageBox.Show(this, "指示符2内容必须为1字符");
                e.Cancel = true;
            }

        }

        int m_nInDropDown = 0;

        private void comboBox_u_DropDown(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            if (this.comboBox_u.Items.Count != 0)
                return;

            if (this.GetResInfo == null)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                GetResInfoEventArgs e1 = new GetResInfoEventArgs();
                e1.ID = "";
                this.GetResInfo(this, e1);

                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    MessageBox.Show(this, e1.ErrorInfo);
                    return;
                }

                if (e1.Results == null)
                    return;

                for (int i = 0; i < e1.Results.Count; i++)
                {
                    this.comboBox_u.Items.Add(e1.Results[i].ID);
                }

            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        private void comboBox_u_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.comboBox_u.Text) == true)
                return;

            GetResInfoEventArgs e1 = new GetResInfoEventArgs();
            e1.ID = this.comboBox_u.Text;
            this.GetResInfo(this, e1);

            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                MessageBox.Show(this, e1.ErrorInfo);
                return;
            }

            if (e1.Results == null || e1.Results.Count == 0)
                return;

            this.textBox_f.Text = e1.Results[0].LocalPath;
            this.textBox_q.Text = e1.Results[0].Mime;
            this.textBox_s.Text = e1.Results[0].Size.ToString();
        }

        private void tabComboBox_type_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.tabComboBox_type.Text) == true)
            {
                this.textBox_x.ReadOnly = false;
                this.textBox_x.Text = "";
            }
            else
            {
                this.textBox_x.ReadOnly = true;
                this.textBox_x.Text = GetTypeString(this.tabComboBox_type.Text);
            }

        }
    }

    public class ResInfo
    {
        public string ID = "";
        public string Mime = "";
        public long Size = 0;
        public string LocalPath = "";
        // public string LastModified = "";    // 最后修改时间
    }

    /// <summary>
    /// 获得资源相关信息
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetResInfoEventHandler(object sender,
        GetResInfoEventArgs e);

    /// <summary>
    /// 获得值列表的参数
    /// </summary>
    public class GetResInfoEventArgs : EventArgs
    {
        public string ID = "";
        public List<ResInfo> Results = null;

        public string ErrorInfo = "";
    }
}