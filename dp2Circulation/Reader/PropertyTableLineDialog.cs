using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation.Reader
{
    public partial class PropertyTableLineDialog : Form
    {
        // 合法的参数名列表
        // 如果集合不为空，则 OK 按钮按下以后会自动校验参数名是否合法
        List<string> _propertyNameList = new List<string>();
        public List<string> PropertyNameList
        {
            get
            {
                return _propertyNameList;
            }
            set
            {
                this._propertyNameList.Clear();
                if (value != null)
                    this._propertyNameList.AddRange(value);

                _default_caption = PropertyTableDialog.BuildDefaultCaption(this._propertyNameList);

                // this.comboBox_name.Items.AddRange(this._propertyNameList.Cast<object>().ToArray());
                foreach(var s in this._propertyNameList)
                {
                    this.comboBox_name.Items.Add(GetDisplayName(s));
                }
            }
        }

        string _default_caption = "";

        public PropertyTableLineDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 按下 Ctrl 键则不会检查参数名、参数值
            var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            string name = GetPureName(this.comboBox_name.Text);

            if (control == false
                && string.IsNullOrEmpty(name))
            {
                if (AllowEmptyName(this._propertyNameList) == false)
                {
                    strError = "请输入参数名";
                    goto ERROR1;
                }
            }

            if (control == false
                && string.IsNullOrEmpty(this.textBox_value.Text))
            {
                strError = "请输入参数值";
                goto ERROR1;
            }

            // 检查参数名是否合法
            if (control == false
                && this._propertyNameList != null && this._propertyNameList.Count > 0)
            {
                if (this._propertyNameList.IndexOf(name) == -1)
                {
                    strError = $"参数名 '{this.comboBox_name.Text}' 不合法，请重新输入";
                    goto ERROR1;
                }
            }

            strError = CheckString(name);
            if (string.IsNullOrEmpty(strError) == false)
                goto ERROR1;

            strError = CheckString(this.textBox_value.Text);
            if (string.IsNullOrEmpty(strError) == false)
                goto ERROR1;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        static string CheckString(string text)
        {
            if (text.IndexOfAny(new char[] { ':',',','[',']'}) != -1)
                return $"'{text}' 中出现了非法字符。不允许出现逗号、冒号、方括号";
            return null;
        }

        public static bool AllowEmptyName(List<string> nameList)
        {
            if (nameList != null && nameList.Count > 0)
            {
                if (nameList.IndexOf("") != -1
                    || nameList.IndexOf(null) != -1)
                    return true;
            }

            return false;
        }

        public string ParameterName
        {
            get
            {
                return GetPureName(this.comboBox_name.Text);
            }
            set
            {
                this.comboBox_name.Text = GetDisplayName(value);
            }
        }

        public string ParameterValue
        {
            get
            {
                return this.textBox_value.Text;
            }
            set
            {
                this.textBox_value.Text = value;
            }
        }

        string GetDisplayName(string name)
        {
            return name;
            // return string.IsNullOrEmpty(name) ? _default_caption : name;
        }

        string GetPureName(string caption)
        {
            return caption;
            /*
            if (caption == _default_caption)
                return "";
            return caption;
            */
        }

        private void comboBox_name_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_name.Text == _default_caption
                || string.IsNullOrEmpty(this.comboBox_name.Text))
                this.label_nameComment.Text = _default_caption;
            else
                this.label_nameComment.Text = "";
        }

        private void PropertyTableLineDialog_Load(object sender, EventArgs e)
        {
            comboBox_name_TextChanged(sender, e);
        }
    }
}
