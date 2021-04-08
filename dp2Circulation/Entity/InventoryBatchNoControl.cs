using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 盘点操作中用到的批次号控件
    /// </summary>
    public partial class InventoryBatchNoControl : UserControl
    {
        public InventoryBatchNoControl()
        {
            InitializeComponent();
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler TextChanged
        {
            // http://stackoverflow.com/questions/9370448/add-attribute-to-base-event
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        public override string Text
        {
            get
            {
#if NO
                if (string.IsNullOrEmpty(this.comboBox_libraryCode.Text) == true)
                    return this.textBox_number.Text;
#endif
                return this.comboBox_libraryCode.Text + "-" + this.textBox_number.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value)== true)
                {
                    this.comboBox_libraryCode.Text = "";
                    this.textBox_number.Text = "";
                    return;
                }
#if NO
                if (value.IndexOf("-") == -1)
                {
                    this.comboBox_libraryCode.Text = "";
                    this.textBox_number.Text = value;
                    return;
                }
#endif
                string strPart1 = "";
                string strPart2 = "";
                StringUtil.ParseTwoPart(value, "-", out strPart1, out strPart2);
                this.comboBox_libraryCode.Text = strPart1;
                this.textBox_number.Text = strPart2;

                // 2021/4/8
                // 如果设定到第一部分的值无效，则转为都设定到第二部分
                if (this.comboBox_libraryCode.Text != strPart1)
                    this.textBox_number.Text = value;
            }
        }

        private void comboBox_libraryCode_TextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);
        }

        private void textBox_number_TextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);
        }

        public bool LibaryCodeEanbled
        {
            get
            {
                return this.comboBox_libraryCode.Enabled;
            }
            set
            {
                this.comboBox_libraryCode.Enabled = value;
            }
        }

        public string LibraryCodeText
        {
            get
            {
                return this.comboBox_libraryCode.Text;
            }
            set
            {
                this.comboBox_libraryCode.Text = value;
            }
        }

        // 馆代码列表
        public List<string> LibraryCodeList
        {
            get
            {
                List<string> results = new List<string>();
                foreach (string s in this.comboBox_libraryCode.Items)
                {
                    results.Add(s);
                }
                return results;
            }
            set
            {
                this.comboBox_libraryCode.Items.Clear();
                foreach(string s in value)
                {
                    this.comboBox_libraryCode.Items.Add(s);
                }

                if (this.comboBox_libraryCode.Items.Count == 1 && string.IsNullOrEmpty(this.comboBox_libraryCode.Text) == true)
                    this.comboBox_libraryCode.Text = (string)this.comboBox_libraryCode.Items[0];
            }
        }
    }
}
