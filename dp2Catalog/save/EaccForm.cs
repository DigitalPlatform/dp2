using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;

namespace dp2Catalog
{
    public partial class EaccForm : Form
    {
        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;

        public EaccForm()
        {
            InitializeComponent();
        }

        private void EaccForm_Load(object sender, EventArgs e)
        {
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager);	// 和容器关联


            this.textBox_unihanFilenames.Text = MainForm.applicationInfo.GetString(
                "eacc_form",
                "unihan_filename",
                "");
            this.textBox_e2uFilename.Text = MainForm.applicationInfo.GetString(
                "eacc_form",
                "e2u_filename",
                "");
            this.textBox_u2eFilename.Text = MainForm.applicationInfo.GetString(
                "eacc_form",
                "u2e_filename",
                "");

        }

        private void EaccForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.DoStop();

                stop.Unregister();	// 和容器关联
                stop = null;
            }


            MainForm.applicationInfo.SetString(
                "eacc_form",
                "unihan_filename",
                this.textBox_unihanFilenames.Text);
            MainForm.applicationInfo.SetString(
                "eacc_form",
                "e2u_filename",
                this.textBox_e2uFilename.Text);
            MainForm.applicationInfo.SetString(
                "eacc_form",
                "u2e_filename",
                this.textBox_u2eFilename.Text);

        }

        private void button_begin_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";
            nRet = BuildCharsetTable(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "OK");
            }
        }

        int BuildCharsetTable(out string strError)
        {
            strError = "";


            CharsetTable charsettable_e2u = new CharsetTable();
            CharsetTable charsettable_u2e = new CharsetTable();

            charsettable_e2u.Open(true);
            charsettable_u2e.Open(true);


            if (this.textBox_unihanFilenames.Text == "")
            {
                strError = "尚未指定输入文件名";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在创建码表文件 ...");
            stop.BeginLoop();

            EnableControls(false);

            this.Update();
            this.MainForm.Update();

            try
            {

                for (int i = 0; i < this.textBox_unihanFilenames.Lines.Length; i++)
                {

                    StreamReader sr = null;
                    try
                    {
                        sr = new StreamReader(this.textBox_unihanFilenames.Lines[i]);
                    }
                    catch (Exception ex)
                    {
                        strError = "文件 " + this.textBox_unihanFilenames.Lines[i] + " 打开失败: " + ex.Message;
                        return -1;
                    }

                    this.MainForm.ToolStripProgressBar.Minimum = 0;
                    this.MainForm.ToolStripProgressBar.Maximum = (int)sr.BaseStream.Length;
                    this.MainForm.ToolStripProgressBar.Value = 0;



                    try
                    {
                        for (; ; )
                        {
                            Application.DoEvents();	// 出让界面控制权

                            if (stop != null)
                            {
                                if (stop.State != 0)
                                {
                                    strError = "用户中断";
                                    return -1;
                                }
                            }

                            string strLine = sr.ReadLine();
                            if (strLine == null)
                                break;

                            if (strLine.Length < 1)
                                goto CONTINUE;

                            // 注释行
                            if (strLine[0] == '#')
                                goto CONTINUE;

                            int nRet = strLine.IndexOf("\t", 0);
                            if (nRet == -1)
                                goto CONTINUE;	// 格式有问题

                            string strPart1 = strLine.Substring(0, nRet).Trim();

                            strLine = strLine.Substring(nRet + 1);

                            nRet = strLine.IndexOf("\t", 0);
                            if (nRet == -1)
                                goto CONTINUE;	// 格式有问题

                            string strPart2 = strLine.Substring(0, nRet).Trim();
                            string strPart3 = strLine.Substring(nRet + 1).Trim();

                            strPart1 = strPart1.Substring(2);	// 去掉'U+'

                            if (strPart2 != "kEACC")
                                goto CONTINUE;	// 不相关的行

                            strLine = strPart1 + "\t" + strPart3;

                            CharsetItem item = new CharsetItem();
                            item.Content = strLine;
                            charsettable_u2e.Add(item);

                            strLine = strPart3 + "\t" + strPart1;

                            item = new CharsetItem();
                            item.Content = strLine;
                            charsettable_e2u.Add(item);	// ANSI字符集

                            stop.SetMessage(strLine);

                        CONTINUE:
                            // 显示进度条
                            this.MainForm.ToolStripProgressBar.Value = (int)sr.BaseStream.Position;
                        }

                    }
                    finally
                    {
                        sr.Close();
                    }

                }


                stop.SetMessage("正在复制和排序...");

            string strDataFileName = "";
            string strIndexFileName = "";

            if (String.IsNullOrEmpty(this.textBox_e2uFilename.Text) == false)
            {

                charsettable_e2u.Sort();
                charsettable_e2u.Detach(out strDataFileName,
                    out strIndexFileName);

                File.Delete(this.textBox_e2uFilename.Text);
                File.Move(strDataFileName,
                    this.textBox_e2uFilename.Text);

                File.Delete(this.textBox_e2uFilename.Text + ".index");
                File.Move(strIndexFileName,
                    this.textBox_e2uFilename.Text + ".index");
            }

            //

            if (String.IsNullOrEmpty(this.textBox_u2eFilename.Text) == false)
            {
                charsettable_u2e.Sort();
                charsettable_u2e.Detach(out strDataFileName,
                    out strIndexFileName);

                File.Delete(this.textBox_u2eFilename.Text);
                File.Move(strDataFileName,
                    this.textBox_u2eFilename.Text);

                File.Delete(this.textBox_u2eFilename.Text + ".index");
                File.Move(strIndexFileName,
                    this.textBox_u2eFilename.Text + ".index");
            }


        }
        finally
        {
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControls(true);

        }

            return 0;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            /*
            if (this.Channel != null)
                this.Channel.Cancel();
             * */
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_e2uFilename.Enabled = bEnable;
            this.textBox_u2eFilename.Enabled = bEnable;
            this.textBox_unihanFilenames.Enabled = bEnable;

            this.button_findE2uFilename.Enabled = bEnable;
            this.button_findOriginFilename.Enabled = bEnable;
            this.button_findU2eFilename.Enabled = bEnable;
            this.button_begin.Enabled = bEnable;
        }

        // 根据Eacc编码查出Unicode编码
        private void button_searchE2u_Click(object sender, EventArgs e)
        {
            if (this.textBox_e2uFilename.Text == "")
            {
                MessageBox.Show(this, "尚未指定 e2u 码表文件");
                return;
            }

            this.textBox_unicodeCode.Text = "";

            CharsetTable charsettable_e2u = new CharsetTable();

            charsettable_e2u.Attach(this.textBox_e2uFilename.Text,
                this.textBox_e2uFilename.Text + ".index");

            try
            {

                string strValue = "";
                // return:
                //      -1  not found
                int nRet = charsettable_e2u.Search(
                    this.textBox_eaccCode.Text.ToUpper(),
                    out strValue);

                if (nRet == -1)
                {
                    MessageBox.Show(this, "没有找到");
                }
                else
                {
                    this.textBox_unicodeCode.Text = strValue;
                }
            }
            finally
            {
                string strTemp1;
                string strTemp2;

                charsettable_e2u.Detach(out strTemp1,
                    out strTemp2);
            }


        }

        // 根据Unicode编码查出Eacc编码
        private void button_searchU2e_Click(object sender, EventArgs e)
        {
            if (this.textBox_u2eFilename.Text == "")
            {
                MessageBox.Show(this, "尚未指定 u2e 码表文件");
                return;
            }

            this.textBox_eaccCode.Text = "";

            CharsetTable charsettable_u2e = new CharsetTable();

            charsettable_u2e.Attach(this.textBox_u2eFilename.Text,
                this.textBox_u2eFilename.Text + ".index");

            try
            {

                string strValue = "";
                // return:
                //      -1  not found
                int nRet = charsettable_u2e.Search(
                    this.textBox_unicodeCode.Text.ToUpper(),
                    out strValue);

                if (nRet == -1)
                {
                    MessageBox.Show(this, "没有找到");
                }
                else
                {
                    this.textBox_eaccCode.Text = strValue;
                }
            }
            finally
            {
                string strTemp1;
                string strTemp2;

                charsettable_u2e.Detach(out strTemp1,
                    out strTemp2);
            }
        }

        private void button_e2uStringConvert_Click(object sender, EventArgs e)
        {
            if (this.textBox_e2uFilename.Text == "")
            {
                MessageBox.Show(this, "尚未指定 e2u 码表文件");
                return;
            }

            this.textBox_unicodeCode.Text = "";

            CharsetTable charsettable_e2u = new CharsetTable();

            charsettable_e2u.Attach(this.textBox_e2uFilename.Text,
                this.textBox_e2uFilename.Text + ".index");

            try
            {
                string strText = "";
                charsettable_e2u.Text_e2u(this.textBox_eaccString.Text,
                    out strText);

                this.textBox_unicodeString.Text = strText;
            }
            finally
            {
                string strTemp1;
                string strTemp2;

                charsettable_e2u.Detach(out strTemp1,
                    out strTemp2);
            }


        }

        private void button_u2eStringConvert_Click(object sender, EventArgs e)
        {

        }



    }
}