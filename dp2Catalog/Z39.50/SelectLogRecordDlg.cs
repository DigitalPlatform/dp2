using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace dp2Catalog
{
	public partial class SelectLogRecordDlg: Form
	{
        // 最后选定日志记录的数据
        public byte[] Package = null;

		public SelectLogRecordDlg()
		{
			InitializeComponent();
		}

        private void SelectLogRecordDlg_Load(object sender, EventArgs e)
        {

        }

        private void SelectLogRecordDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_index.SelectedItems.Count == 0)
            {
                strError = "尚未选定日志记录";
                goto ERROR1;
            }

            IndexInfo info = (IndexInfo)this.listView_index.SelectedItems[0].Tag;

            string strLogFilename = this.textBox_logFilename.Text;

            try
            {
                using (Stream stream = File.OpenRead(strLogFilename))
                {
                    if (info.Offs > stream.Length)
                    {
                        strError = "info.Offs " + info.Offs.ToString() + " > stream.Length " + stream.Length.ToString();
                        goto ERROR1;
                    }
                    stream.Seek(info.Offs, SeekOrigin.Begin);

                    this.Package = new byte[info.Length];

                    int nRet = stream.Read(this.Package, 0, (int)info.Length);
                }
            }
            catch (Exception ex)
            {
                strError = "read file '" + strLogFilename + "' error: " + ex.Message;
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

        private void button_findLogFilename_Click(object sender, EventArgs e)
        {
            // 询问原始文件全路径
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.FileName = this.textBox_logFilename.Text;
            //dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.textBox_logFilename.Text = dlg.FileName;

            // 装入listview
            string strError = "";
            int nRet = LoadIndex(this.listView_index,
                this.textBox_logFilename.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }



        // 装载通讯包日志文件的记录索引
        int LoadIndex(
            ListView list,
            string strLogFilename,
            out string strError)
        {
            strError = "";

            Stream stream = null;

            try
            {
                stream = File.OpenRead(strLogFilename);
            }
            catch (Exception ex)
            {
                strError = "file '" + strLogFilename + "'open error: " + ex.Message;
                return -1;
            }

            try
            {
                list.Items.Clear();
                for (long i = 0; ; i++)
                {
                    byte[] len_buffer = new byte[8];

                    int nRet = stream.Read(len_buffer, 0, 8);
                    if (nRet == 0)
                        break;
                    if (nRet != 8)
                    {
                        strError = "file format error";
                        return -1;
                    }

                    long length = BitConverter.ToInt64(len_buffer, 0);

                    if (length == -1)
                    {
                        // 出现了一个未收尾的事项
                        // 把文件最后都算作它的内容
                        length = stream.Length - stream.Position;
                    }

                    IndexInfo info = new IndexInfo();

                    info.index = i;

                    ListViewItem item = new ListViewItem();

                    if (length == 0)
                    {
                        // 特殊处理
                        item.Text = i.ToString();
                        item.ImageIndex = 0;
                        item.Tag = info;
                        list.Items.Add(item);
                        continue;
                    }

                    int direction = stream.ReadByte();

                    long lStartOffs = stream.Position;

                    length--;   // 这是内容长度

                    item.Text = i.ToString();
                    // 方向'
                    string strDirection = "";
                    int nImageIndex = 0;
                    if (direction == 0)
                    {
                        strDirection = "none";
                        nImageIndex = 0;
                    }
                    else if (direction == 1)
                    {
                        strDirection = "client";
                        nImageIndex = 1;
                    }
                    else if (direction == 2)
                    {
                        strDirection = "server";
                        nImageIndex = 2;
                    }
                    else
                    {
                        strDirection = "error direction value: " + ((int)direction).ToString();
                        nImageIndex = 0;
                    }

                    item.Text += " " + strDirection;
                    item.Text += " len:" + length.ToString();
                    item.Text += " offs:" + lStartOffs.ToString();
                    item.ImageIndex = nImageIndex;

                    info.Offs = lStartOffs;
                    info.Length = length;

                    item.Tag = info;
                    list.Items.Add(item);


                    if (length >= 100 * 1024)
                    {
                        stream.Seek(length, SeekOrigin.Current);
                        info.BerTree = null;
                    }
                    else
                    {
                        byte[] baPackage = new byte[(int)length];
                        stream.Read(baPackage, 0, (int)length);
                    }
                }

            }
            finally
            {
                stream.Close();
            }

            return 0;
        }

        private void button_loadFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

                    // 装载通讯包日志文件的记录索引
            nRet = LoadIndex(
                this.listView_index,
                this.textBox_logFilename.Text,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

        }

        public string Filename
        {
            get
            {
                return this.textBox_logFilename.Text;
            }
            set
            {
                this.textBox_logFilename.Text = value;
            }
        }

        private void textBox_logFilename_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_logFilename.Text != "")
                this.button_loadFile.Enabled = true;
            else
                this.button_loadFile.Enabled = false;
        }
	}
}