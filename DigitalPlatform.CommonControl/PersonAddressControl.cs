using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.Xml;

namespace DigitalPlatform.CommonControl
{
    public partial class PersonAddressControl : UserControl
    {
        public string DbName = "";  // 数据库名。用于获取valueTable值时作为线索

        XmlDocument RecordDom = null;

        bool m_bChanged = false;

        bool m_bInInitial = true;   // 是否正在初始化过程之中

        Color ColorChanged = Color.Yellow; // 表示内容改变过的颜色
        Color ColorDifference = Color.Blue; // 表示内容有差异的颜色

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        public event ControlKeyPressEventHandler ControlKeyPress = null;

        public event ControlKeyEventHandler ControlKeyDown = null;


        public PersonAddressControl()
        {
            InitializeComponent();
        }

        #region 数据成员

        public string OldRecord = "";
        // public byte[] Timestamp = null;

        public string Zipcode
        {
            get
            {
                return this.textBox_zipcode.Text;
            }
            set
            {
                this.textBox_zipcode.Text = value;
            }
        }

        public string Address
        {
            get
            {
                return this.textBox_address.Text;
            }
            set
            {
                this.textBox_address.Text = value;
            }
        }

        public string PersonName
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        public string Department
        {
            get
            {
                return this.textBox_department.Text;
            }
            set
            {
                this.textBox_department.Text = value;
            }
        }

        public string Tel
        {
            get
            {
                return this.textBox_tel.Text;
            }
            set
            {
                this.textBox_tel.Text = value;
            }
        }

        public string Email
        {
            get
            {
                return this.textBox_email.Text;
            }
            set
            {
                this.textBox_email.Text = value;
            }
        }

        public string Bank
        {
            get
            {
                return this.textBox_bank.Text;
            }
            set
            {
                this.textBox_bank.Text = value;
            }
        }

        public string Accounts
        {
            get
            {
                return this.textBox_accounts.Text;
            }
            set
            {
                this.textBox_accounts.Text = value;
            }
        }

        public string PayStyle
        {
            get
            {
                return this.comboBox_payStyle.Text;
            }
            set
            {
                this.comboBox_payStyle.Text = value;
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

        #endregion  // 数据成员

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                bool bOldValue = this.m_bChanged;

                this.m_bChanged = value;
                if (this.m_bChanged == false)
                    this.ResetColor();

                // 触发事件
                if (bOldValue != value && this.ContentChanged != null)
                {
                    ContentChangedEventArgs e = new ContentChangedEventArgs();
                    e.OldChanged = bOldValue;
                    e.CurrentChanged = value;
                    ContentChanged(this, e);
                }
            }
        }

        public bool Initializing
        {
            get
            {
                return this.m_bInInitial;
            }
            set
            {
                this.m_bInInitial = value;
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="strXml">地址 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SetData(string strXml,
            out string strError)
        {
            strError = "";

            this.OldRecord = strXml;
            this.RecordDom = new XmlDocument();

            try
            {
                this.RecordDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装载到DOM时出错" + ex.Message;
                return -1;
            }

            this.Initializing = true;

            this.Zipcode = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "zipcode");
            this.Address = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "address");
            this.Department = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "department");
            this.PersonName = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "name");
            this.Tel = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "tel");
            this.Email = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "email");
            this.Bank = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "bank");
            this.Accounts = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "accounts");
            this.PayStyle = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "payStyle");
            this.Comment = DomUtil.GetElementText(this.RecordDom.DocumentElement,
                "comment");

            this.Initializing = false;

            this.Changed = false;

            return 0;
        }

        public void Clear()
        {
            this.Zipcode = "";
            this.Address = "";
            this.PersonName = "";
            this.Department = "";
            this.Tel = "";
            this.Email = "";
            this.Bank = "";
            this.Accounts = "";
            this.PayStyle = "";
            this.Comment = "";

            this.ResetColor();

            this.Changed = false;
        }

        public XmlDocument DataDom
        {
            get
            {
                // 2009/2/13
                if (this.RecordDom == null)
                {
                    this.RecordDom = new XmlDocument();
                    this.RecordDom.LoadXml("<root />");
                }
                this.RefreshDom();
                return this.RecordDom;
            }
        }

        public void RefreshDom()
        {
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "zipcode", this.Zipcode);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "address", this.Address);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "department", this.Department);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "name", this.PersonName);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "tel", this.Tel);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "email", this.Email);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "bank", this.Bank);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "accounts", this.Accounts);
            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "payStyle", this.PayStyle);

            DomUtil.SetElementText(this.RecordDom.DocumentElement,
                "comment", this.Comment);
        }

        /// <summary>
        /// 创建好适合于保存的记录信息
        /// </summary>
        /// <param name="strXml">返回构造好的地址 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetData(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (this.RecordDom == null)
            {
                this.RecordDom = new XmlDocument();
                this.RecordDom.LoadXml("<root />");
            }

            this.RefreshDom();

            strXml = this.RecordDom.OuterXml;

            return 0;
        }

        public void FocusZipcode(bool bSelectAll)
        {
            if (bSelectAll == true)
                this.textBox_zipcode.SelectAll();

            this.textBox_zipcode.Focus();
        }

        public void ResetColor()
        {
            Color color = this.tableLayoutPanel_main.BackColor;
            this.label_zipcode_color.BackColor = color;    // 和背景一致
            this.label_address_color.BackColor = color;
            this.label_name_color.BackColor = color;
            this.label_department_color.BackColor = color;
            this.label_tel_color.BackColor = color;
            this.label_email_color.BackColor = color;
            this.label_bank_color.BackColor = color;
            this.label_accounts_color.BackColor = color;
            this.label_payStyle_color.BackColor = color;
            this.label_comment_color.BackColor = color;
        }

        private void valueTextChanged(object sender, EventArgs e)
        {
            this.Changed = true;

            string strControlName = "";

            if (sender is TextBox)
            {
                TextBox textbox = (TextBox)sender;
                strControlName = textbox.Name;
            }
            else if (sender is ComboBox)
            {
                ComboBox combobox = (ComboBox)sender;
                strControlName = combobox.Name;
            }
            else
            {
                Debug.Assert(false, "未处理的类型 " + sender.GetType().ToString());
                return;
            }

            int nRet = strControlName.IndexOf("_");
            if (nRet == -1)
            {
                Debug.Assert(false, "textbox名字中没有下划线");
                return;
            }

            string strLabelName = "label_" + strControlName.Substring(nRet + 1) + "_color";

            Label label = (Label)this.tableLayoutPanel_main.Controls[strLabelName];
            if (label == null)
            {
                Debug.Assert(false, "没有找到名字为 '" + strLabelName + "' 的Label控件");
                return;
            }

            label.BackColor = this.ColorChanged;
        }

        /*
        // 从路径中取出库名部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }
        */

        // 防止重入 2009/7/19
        int m_nInDropDown = 0;


        private void comboBox_payStyle_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/1/15
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {

                ComboBox combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.DbName;

                    if (combobox == this.comboBox_payStyle)
                        e1.TableName = "payStyle";
                    else
                    {
                        Debug.Assert(false, "不支持的sender");
                        return;
                    }

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        // 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// <summary>
        /// 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// </summary>
        /// <param name="refControl">要和自己进行比较的控件对象</param>
        public void HighlightDifferences(PersonAddressControl refControl)
        {
            if (this.Zipcode != refControl.Zipcode)
                this.label_zipcode_color.BackColor = this.ColorDifference;

            if (this.Address != refControl.Address)
                this.label_address_color.BackColor = this.ColorDifference;

            if (this.PersonName != refControl.PersonName)
                this.label_name_color.BackColor = this.ColorDifference;

            if (this.Department != refControl.Department)
                this.label_department_color.BackColor = this.ColorDifference;

            if (this.Tel != refControl.Tel)
                this.label_tel_color.BackColor = this.ColorDifference;

            if (this.Email != refControl.Email)
                this.label_email_color.BackColor = this.ColorDifference;

            if (this.Bank != refControl.Bank)
                this.label_bank_color.BackColor = this.ColorDifference;

            if (this.Accounts != refControl.Accounts)
                this.label_accounts_color.BackColor = this.ColorDifference;

            if (this.PayStyle != refControl.PayStyle)
                this.label_payStyle_color.BackColor = this.ColorDifference;

            if (this.Comment != refControl.Comment)
                this.label_comment_color.BackColor = this.ColorDifference;
        }

        private void DoKeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.ControlKeyPress != null)
            {
                ControlKeyPressEventArgs e1 = new ControlKeyPressEventArgs();
                e1.e = e;
                if (sender == (object)this.textBox_zipcode)
                    e1.Name = "Zipcode";
                else if (sender == (object)this.textBox_address)
                    e1.Name = "Address";
                else if (sender == (object)this.textBox_name)
                    e1.Name = "Name";
                else if (sender == (object)this.textBox_department)
                    e1.Name = "Department";
                else if (sender == (object)this.textBox_tel)
                    e1.Name = "Tel";
                else if (sender == (object)this.textBox_email)
                    e1.Name = "Email";
                else if (sender == (object)this.textBox_bank)
                    e1.Name = "Bank";
                else if (sender == (object)this.textBox_accounts)
                    e1.Name = "Accounts";
                else if (sender == (object)this.comboBox_payStyle)
                    e1.Name = "PayStyle";
                else if (sender == (object)this.textBox_comment)
                    e1.Name = "Comment";
                else
                {
                    Debug.Assert(false, "未知的部件");
                    return;
                }

                this.ControlKeyPress(this, e1);
            }

        }

        private void DoKeyDown(object sender, KeyEventArgs e)
        {
            if (this.ControlKeyDown != null)
            {
                ControlKeyEventArgs e1 = new ControlKeyEventArgs();
                e1.e = e;
                if (sender == (object)this.textBox_zipcode)
                    e1.Name = "Zipcode";
                else if (sender == (object)this.textBox_address)
                    e1.Name = "Address";
                else if (sender == (object)this.textBox_name)
                    e1.Name = "Name";
                else if (sender == (object)this.textBox_department)
                    e1.Name = "Department";
                else if (sender == (object)this.textBox_tel)
                    e1.Name = "Tel";
                else if (sender == (object)this.textBox_email)
                    e1.Name = "Email";
                else if (sender == (object)this.textBox_bank)
                    e1.Name = "Bank";
                else if (sender == (object)this.textBox_accounts)
                    e1.Name = "Accounts";
                else if (sender == (object)this.comboBox_payStyle)
                    e1.Name = "PayStyle";
                else if (sender == (object)this.textBox_comment)
                    e1.Name = "Comment";
                else
                {
                    Debug.Assert(false, "未知的部件");
                    return;
                }

                this.ControlKeyDown(this, e1);
            }

        }
    }
}
