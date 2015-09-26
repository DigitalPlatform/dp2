using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 状态修改动作控件
    /// </summary>
    public partial class ChangeStateActionControl : UserControl
    {
        /// <summary>
        /// 增、减 List DropDown 事件
        /// </summary>
        public event EventHandler AddOrRemoveListDropDown = null;

        /// <summary>
        /// “是否修改”状态修改 事件
        /// </summary>
        public event EventHandler ActionChanged = null;


        /// <summary>
        /// 构造函数
        /// </summary>
        public ChangeStateActionControl()
        {
            InitializeComponent();
        }

        private void checkedComboBox_stateAdd_DropDown(object sender, EventArgs e)
        {
            if (this.AddOrRemoveListDropDown != null)
            {
                this.AddOrRemoveListDropDown(this.checkedComboBox_stateAdd, e);
            }
        }

        private void checkedComboBox_stateRemove_DropDown(object sender, EventArgs e)
        {
            if (this.AddOrRemoveListDropDown != null)
            {
                this.AddOrRemoveListDropDown(this.checkedComboBox_stateRemove, e);
            }
        }

        private void comboBox_state_TextChanged(object sender, EventArgs e)
        {
            string strText = this.comboBox_state.Text;

            if (strText == "<增、减>")
            {
                this.checkedComboBox_stateAdd.Enabled = true;
                this.checkedComboBox_stateRemove.Enabled = true;
            }
            else
            {
                this.checkedComboBox_stateAdd.Text = "";
                this.checkedComboBox_stateAdd.Enabled = false;

                this.checkedComboBox_stateRemove.Text = "";
                this.checkedComboBox_stateRemove.Enabled = false;
            }

            if (this.ActionChanged != null)
            {
                this.ActionChanged(this, e);
            }
        }

#if NO
        bool m_bChanged = false;

        /// <summary>
        /// 内容是否被修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;

                if (this.ChangedChanged != null)
                {
                    this.ChangedChanged(this, new EventArgs());
                }
            }
        }
#endif

        private void comboBox_state_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_state.Invalidate();
        }

        /// <summary>
        /// 当前动作是否为要改变
        /// </summary>
        public bool IsActionChange
        {
            get
            {
                if (this.ActionString == "<不改变>")
                    return false;
                return true;
            }
        }

        /// <summary>
        /// 动作组合框里面的值
        /// </summary>
        public string ActionString
        {
            get
            {
                return this.comboBox_state.Text;
            }
            set
            {
                this.comboBox_state.Text = value;

                comboBox_state_TextChanged(this, new EventArgs());

                if (this.ActionChanged != null)
                {
                    this.ActionChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// “增”组合框里面的值
        /// </summary>
        public string AddString
        {
            get
            {
                return this.checkedComboBox_stateAdd.Text;
            }
            set
            {
                this.checkedComboBox_stateAdd.Text = value;
            }
        }

        /// <summary>
        /// “减”组合框里面的值
        /// </summary>
        public string RemoveString
        {
            get
            {
                return this.checkedComboBox_stateRemove.Text;
            }
            set
            {
                this.checkedComboBox_stateRemove.Text = value;
            }
        }

        private void checkedComboBox_stateAdd_TextChanged(object sender, EventArgs e)
        {
            Global.FilterValueList(this, (Control)sender);
        }

        private void checkedComboBox_stateRemove_TextChanged(object sender, EventArgs e)
        {
            Global.FilterValueList(this, (Control)sender);
        }
    }
}
