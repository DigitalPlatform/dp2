using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    internal partial class ReaderInfoFormOptionDlg : Form
    {
        /// <summary>
        /// ¿ò¼Ü´°¿Ú
        /// </summary>
        public MainForm MainForm = null;

        public ReaderInfoFormOptionDlg()
        {
            InitializeComponent();
        }

        private void ReaderInfoFormOptionDlg_Load(object sender, EventArgs e)
        {
            string strError = "";

            string strNewDefault = this.MainForm.AppInfo.GetString(
                "readerinfoform_optiondlg",
                "newreader_default",
                "<root />");
            int nRet = this.readerEditControl_newReaderDefault.SetData(strNewDefault,
                 "",
                 null,
                 out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            this.textBox_cardPhoto_maxWidth.Text = this.MainForm.AppInfo.GetString(
                "readerinfoform_optiondlg",
                "cardphoto_maxwidth",
                "120");

            string strSelection = this.MainForm.AppInfo.GetString(
                "readerinfoform_optiondlg",
                "idcardfield_filter_list",
                "name,gender,nation,dateOfBirth,address,idcardnumber,agency,validaterange,photo");
            SetIdcardFieldSelection(strSelection);

            this.readerEditControl_newReaderDefault.SetReadOnly("librarian");

            this.readerEditControl_newReaderDefault.GetValueTable += new GetValueTableEventHandler(readerEditControl_newReaderDefault_GetValueTable);
        }

        void readerEditControl_newReaderDefault_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void ReaderInfoFormOptionDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.readerEditControl_newReaderDefault.GetValueTable -= new GetValueTableEventHandler(readerEditControl_newReaderDefault_GetValueTable);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strNewDefault = "";
            string strError = "";

            int nRet = this.readerEditControl_newReaderDefault.GetData(
                out strNewDefault,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                this.MainForm.AppInfo.SetString(
                "readerinfoform_optiondlg",
                "newreader_default",
        strNewDefault);
            }

            this.MainForm.AppInfo.SetString(
    "readerinfoform_optiondlg",
    "cardphoto_maxwidth",
    this.textBox_cardPhoto_maxWidth.Text);

            string strSelection = GetIdcardFieldSelection();
            
            this.MainForm.AppInfo.SetString(
     "readerinfoform_optiondlg",
     "idcardfield_filter_list",
     strSelection);


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        void SetIdcardFieldSelection(string strSelection)
        {
            this.checkBox_idCard_selectName.Checked =
                StringUtil.IsInList("name", strSelection);
            this.checkBox_idCard_selectGender.Checked =
    StringUtil.IsInList("gender", strSelection);
            this.checkBox_idCard_selectNation.Checked =
    StringUtil.IsInList("nation", strSelection);
            this.checkBox_idCard_selectDateOfBirth.Checked =
    StringUtil.IsInList("dateOfBirth", strSelection);
            this.checkBox_idCard_selectAddress.Checked =
    StringUtil.IsInList("address", strSelection);
            this.checkBox_idCard_selectIdCardNumber.Checked =
    StringUtil.IsInList("idcardnumber", strSelection);
            this.checkBox_idCard_selectAgency.Checked =
    StringUtil.IsInList("agency", strSelection);
            this.checkBox_idCard_selectValidateRange.Checked =
    StringUtil.IsInList("validaterange", strSelection);
            this.checkBox_idCard_selectPhoto.Checked =
StringUtil.IsInList("photo", strSelection); 

        }

        string GetIdcardFieldSelection()
        {
            string strSelection = "";
            if (this.checkBox_idCard_selectName.Checked == true)
                StringUtil.SetInList(ref strSelection, "name", true);
            if (this.checkBox_idCard_selectGender.Checked == true)
                StringUtil.SetInList(ref strSelection, "gender", true);
            if (this.checkBox_idCard_selectNation.Checked == true)
                StringUtil.SetInList(ref strSelection, "nation", true);
            if (this.checkBox_idCard_selectDateOfBirth.Checked == true)
                StringUtil.SetInList(ref strSelection, "dateOfBirth", true);
            if (this.checkBox_idCard_selectAddress.Checked == true)
                StringUtil.SetInList(ref strSelection, "address", true);
            if (this.checkBox_idCard_selectIdCardNumber.Checked == true)
                StringUtil.SetInList(ref strSelection, "idcardnumber", true);
            if (this.checkBox_idCard_selectAgency.Checked == true)
                StringUtil.SetInList(ref strSelection, "agency", true);
            if (this.checkBox_idCard_selectValidateRange.Checked == true)
                StringUtil.SetInList(ref strSelection, "validaterange", true);
            if (this.checkBox_idCard_selectPhoto.Checked == true)
                StringUtil.SetInList(ref strSelection, "photo", true);
            return strSelection;
        }

        private void button_idCard_selectAll_Click(object sender, EventArgs e)
        {
            IdcardSelectionSelectAll(true);
        }

        void IdcardSelectionSelectAll(bool bOn)
        {
            this.checkBox_idCard_selectName.Checked = bOn;
            this.checkBox_idCard_selectGender.Checked = bOn;
            this.checkBox_idCard_selectNation.Checked = bOn;
            this.checkBox_idCard_selectDateOfBirth.Checked = bOn;
            this.checkBox_idCard_selectAddress.Checked = bOn;
            this.checkBox_idCard_selectIdCardNumber.Checked = bOn;
            this.checkBox_idCard_selectAgency.Checked = bOn;
            this.checkBox_idCard_selectValidateRange.Checked = bOn;
            this.checkBox_idCard_selectPhoto.Checked = bOn;
        }

        private void button_idCard_clearAll_Click_1(object sender, EventArgs e)
        {
            IdcardSelectionSelectAll(false);
        }

        private void readerEditControl_newReaderDefault_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = this.MainForm.GetReaderDbLibraryCode(e.DbName);
        }
    }
}