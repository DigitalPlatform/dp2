using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MARC 编辑器属性 对话框
    /// </summary>
    internal partial class PropertyDlg : Form
    {
        /// <summary>
        /// MARC 编辑器
        /// </summary>
        public MarcEditor MarcEditor = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PropertyDlg()
        {
            InitializeComponent();
        }

        private void PropertyDlg_Load(object sender, EventArgs e)
        {
            this.checkBox_enterAsAutoGenerate.Checked = this.MarcEditor.EnterAsAutoGenerate;

            AddLangCodeIfNeed(this.MarcEditor.Lang);

            this.comboBox_uiLanguage.Text = this.MarcEditor.Lang;
        }

        // 获得纯粹的语言代码
        static string GetPureLangCode(string strText)
        {
            int nRet = strText.IndexOf("\t");
            if (nRet == -1)
                return strText;
            return strText.Substring(0, nRet);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.MarcEditor.EnterAsAutoGenerate = this.checkBox_enterAsAutoGenerate.Checked;

            string strNewLang = GetPureLangCode(this.comboBox_uiLanguage.Text);

            if (strNewLang != this.MarcEditor.Lang)
            {
                this.MarcEditor.Lang = strNewLang;
                this.MarcEditor.RefreshNameCaption();
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 将现有的语言代码和combobox中已有的代码比较，如果没有，加入它
        void AddLangCodeIfNeed(string strLang)
        {
            for (int i = 0; i < this.comboBox_uiLanguage.Items.Count; i++)
            {
                string strExistLang = GetPureLangCode((string)this.comboBox_uiLanguage.Items[i]);
                if (strLang == strExistLang)
                    return;
            }

            this.comboBox_uiLanguage.Items.Add(strLang);
        }
    }
}