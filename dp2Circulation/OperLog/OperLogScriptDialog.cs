﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 查找日志记录的对话框
    /// </summary>
    internal partial class OperLogScriptDialog : Form
    {
        public OperLogScriptDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_script);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_script);
                GuiState.SetUiState(controls, value);
            }
        }

        public string Script
        {
            get
            {
                return this.textBox_script.Text;
            }
            set
            {
                this.textBox_script.Text = value;
            }
        }

        void DisplayCurLineNo()
        {
            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_script,
                out x,
                out y);
            label_message.Text = "Ln " + Convert.ToString(y + 1) + "   Ch " + (x >= 0 ? Convert.ToString(x + 1) : "?");
        }

        private void textBox_script_MouseUp(object sender, MouseEventArgs e)
        {
            DisplayCurLineNo();
        }

        private void textBox_script_KeyUp(object sender, KeyEventArgs e)
        {
            DisplayCurLineNo();
        }
    }
}
