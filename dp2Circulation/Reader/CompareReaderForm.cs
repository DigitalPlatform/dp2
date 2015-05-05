using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 比较两条读者记录异同的对话框
    /// </summary>
    internal partial class CompareReaderForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public string ExistingXml = "";
        public byte [] ExistingTimestamp = null;

        public string UnsavedXml = "";
        public byte[] UnsavedTimestamp = null;

        public string RecPath = "";

        public CompareReaderForm()
        {
            InitializeComponent();
        }

        public void Initial(
            MainForm mainform,
            string strRecPath,
            string strExistingXml,
            byte [] baExistingTimestamp,
            string strUnsavedXml,
            byte [] baUnsaveTimestamp,
            string strMessage)
        {
            this.MainForm = mainform;

            this.RecPath = strRecPath;

            this.ExistingXml = strExistingXml;
            this.ExistingTimestamp = baExistingTimestamp;

            this.UnsavedXml = strUnsavedXml;
            this.UnsavedTimestamp = baUnsaveTimestamp;

            this.textBox_message.Text = strMessage;
        }

        private void CompareReaderForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            string strError = "";
            int nRet = this.readerEditControl_existing.SetData(
                this.ExistingXml,
                this.RecPath,
                this.ExistingTimestamp,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            this.readerEditControl_existing.SetReadOnly("all");

            nRet = this.readerEditControl_unSaved.SetData(
                this.UnsavedXml,
                this.RecPath,
                this.UnsavedTimestamp,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            this.readerEditControl_unSaved.SetReadOnly("librarian");

            this.readerEditControl_unSaved.HighlightDifferences(this.readerEditControl_existing);

            this.readerEditControl_unSaved.GetValueTable += new GetValueTableEventHandler(readerEditControl_unSaved_GetValueTable);
        }

        void readerEditControl_unSaved_GetValueTable(object sender, GetValueTableEventArgs e)
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

        private void CompareReaderForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.readerEditControl_unSaved.GetValueTable -= new GetValueTableEventHandler(readerEditControl_unSaved_GetValueTable);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";
            int nRet = this.readerEditControl_unSaved.GetData(out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            this.UnsavedXml = strXml;
            this.UnsavedTimestamp = this.ExistingTimestamp;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void readerEditControl_existing_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = this.MainForm.GetReaderDbLibraryCode(e.DbName);
        }

        private void readerEditControl_unSaved_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = this.MainForm.GetReaderDbLibraryCode(e.DbName);
        }


    }
}