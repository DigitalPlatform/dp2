using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /*
        <virtualDatabase>
            <caption lang="zh-CN">中文书刊</caption>
            <caption lang="en">Chinese Books and Series</caption>
            <from style="title">
                <caption lang="zh-CN">题名</caption>
                <caption lang="en">Title</caption>
            </from>
            <from style="author">
                <caption lang="zh-CN">著者</caption>
                <caption lang="en">Author</caption>
            </from>
            <database name="中文图书" />
            <database name="中文期刊" />
        </virtualDatabase>
     * * */
    /// <summary>
    /// 负责配置 OPAC 参与检索库中 virtualDatabase 元素的对话框
    /// </summary>
    internal partial class OpacVirtualDatabaseDialog : Form
    {
        /// <summary>
        /// 是否为创建模式?
        /// true: 创建模式; false: 修改模式
        /// </summary>
        public bool CreateMode = false;

        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        public string Xml = ""; // 窗口打开前用于装载初始化定义，窗口关闭后，用返回修改后的定义

        public List<string> ExistingOpacNormalDbNames = new List<string>(); // 已经存在的普通库名。添加成员库的时候，应当从这个范围内挑选

        public OpacVirtualDatabaseDialog()
        {
            InitializeComponent();
        }

        public int Initial(ManagerForm managerform,
            bool bCreateMode,
            string strXml,
            out string strError)
        {
            strError = "";

            this.ManagerForm = managerform;
            this.CreateMode = bCreateMode;

            this.Xml = strXml;

            // 填充窗口内容
            if (String.IsNullOrEmpty(strXml) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装载到DOM时出错: " + ex.Message;
                    return -1;
                }

                // 虚拟库名captions
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("caption");
                string strCaptionsXml = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    strCaptionsXml += nodes[i].OuterXml;
                }

                if (String.IsNullOrEmpty(strCaptionsXml) == false)
                    this.captionEditControl_virtualDatabaseName.Xml = strCaptionsXml;

                // 成员库
                nodes = dom.DocumentElement.SelectNodes("database");
                string strMemberDatabaseNames = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    strMemberDatabaseNames += DomUtil.GetAttr(nodes[i], "name") + "\r\n";
                }
                this.textBox_memberDatabases.Text = strMemberDatabaseNames;

                // froms定义
                nodes = dom.DocumentElement.SelectNodes("from");
                string strFromsXml = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    strFromsXml += nodes[i].OuterXml;
                }
                if (String.IsNullOrEmpty(strFromsXml) == false)
                    this.fromEditControl1.Xml = strFromsXml;
            }

            return 0;
        }

        public List<string> MemberDatabaseNames
        {
            get
            {
                return this.GetMemberDatabaseNames();
            }
            set
            {
                string strText = "";
                for (int i = 0; i < value.Count; i++)
                {
                    string strOne = value[i];
                    if (String.IsNullOrEmpty(strOne) == true)
                        continue;
                    strText += strOne + "\r\n";
                }

                this.textBox_memberDatabases.Text = strText;
            }
        }

        private void OpacVirtualDatabaseDialog_Load(object sender, EventArgs e)
        {
            if (this.CreateMode == true)
            {
                if (String.IsNullOrEmpty(this.captionEditControl_virtualDatabaseName.Xml) == true)
                    this.captionEditControl_virtualDatabaseName.Xml = "<caption lang='zh'></caption><caption lang='en'></caption>";
            }
            else
            {

            }

            /*
            this.PerformAutoScale();
            this.PerformLayout();
            */
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 进行检查
            string strError = "";

            // 检查虚拟库名

            // 检查当前内容形式上是否合法
            // return:
            //      -1  检查过程本身出错
            //      0   格式有错误
            //      1   格式没有错误
            int nRet = this.captionEditControl_virtualDatabaseName.Verify(out strError);
            if (nRet <= 0)
            {
                strError = "虚拟库名有问题: " + strError;
                this.tabControl_main.SelectedTab = this.tabPage_virtualDatabaseName;
                this.captionEditControl_virtualDatabaseName.Focus();
                goto ERROR1;
            }

            // 看看新增的虚拟库名和已有的数据库名是否重复
            if (this.CreateMode == true)
            {
                // 检查新增的虚拟库名是否和当前已经存在的虚拟库名重复
                // return:
                //      -1  检查的过程发生错误
                //      0   没有重复
                //      1   有重复
                nRet = this.ManagerForm.DetectVirtualDatabaseNameDup(this.captionEditControl_virtualDatabaseName.Xml,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;
            }

            // 检查成员库名
            List<string> dbnames = GetMemberDatabaseNames();
            if (dbnames.Count == 0)
            {
                strError = "尚未指定成员库名: " + strError;
                this.tabControl_main.SelectedTab = this.tabPage_memberDatabases;
                this.textBox_memberDatabases.Focus();
                goto ERROR1;

            }

            // 检查检索途径定义
            nRet = this.fromEditControl1.Verify(out strError);
            if (nRet <= 0)
            {
                strError = "检索途径定义有问题: " + strError;
                this.tabControl_main.SelectedTab = this.tabPage_froms;
                this.fromEditControl1.Focus();
                goto ERROR1;
            }

            // 构造可以发送给服务器的XML定义
            string strXml = "";
            nRet = BuildXml(out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.Xml = strXml;

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

        // 构造可以发送给服务器的XML定义
        int BuildXml(out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<virtualDatabase />");

            // 加入表示虚拟库名的captions
            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = this.captionEditControl_virtualDatabaseName.Xml;
            }
            catch (Exception ex)
            {
                strError = "virtual database name captions fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            // froms
            fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = this.fromEditControl1.Xml;
            }
            catch (Exception ex)
            {
                strError = "froms fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            // member databases
            List<string> dbnames = GetMemberDatabaseNames();
            for (int i = 0; i < dbnames.Count; i++)
            {
                XmlNode node = dom.CreateElement("database");
                dom.DocumentElement.AppendChild(node);
                DomUtil.SetAttr(node, "name", dbnames[i]);
            }

            strXml = dom.DocumentElement.OuterXml;

            return 0;
        }

        // 插入一个成员库名
        private void button_insertMemberDatabaseName_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";
            /*
            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_memberDatabases,
                out x,
                out y);

            string strLine = "";

            if (this.textBox_memberDatabases.Lines.Length > 0)
                strLine = this.textBox_memberDatabases.Lines[y];
             * */

            // 要排除的数据库名
            // 两类情况：一类为已经作为成员库名使用了的；一类为尚未定义为OPAC普通库的
            List<string> exclude_dbnames = new List<string>();
            for (int i = 0; i < this.textBox_memberDatabases.Lines.Length; i++)
            {
                string strLine = this.textBox_memberDatabases.Lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                exclude_dbnames.Add(strLine);
            }

                // 不在OPAC已经定义的普通库名之列，要排除
            List<string> exclude1 = null;
            nRet = GetExcludeDbNames(this.ManagerForm.AllDatabaseInfoXml,
                this.ExistingOpacNormalDbNames,
                out exclude1,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            exclude_dbnames.AddRange(exclude1);

            GetOpacMemberDatabaseNameDialog dlg = new GetOpacMemberDatabaseNameDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.SelectedDatabaseName = this.textBox_memberDatabases.SelectedText;
            dlg.ManagerForm = this.ManagerForm;
            dlg.AllDatabaseInfoXml = this.ManagerForm.AllDatabaseInfoXml;
            dlg.ExcludingDbNames = exclude_dbnames;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strNewText = dlg.SelectedDatabaseName.Replace(",", "\r\n");

            // 如果先前没有选定文字范围的话，就要在新插入的内容末尾增加回车换行符号
            if (String.IsNullOrEmpty(this.textBox_memberDatabases.SelectedText) == true)
                strNewText += "\r\n";

            this.textBox_memberDatabases.Paste(strNewText);
            this.textBox_memberDatabases.Focus();

            // SetLineText(this.textBox_memberDatabases, y, dlg.SelectedDatabaseName);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得要排除的、尚未定义为OPAC普通库的数据库名
        int GetExcludeDbNames(string strAllDatbaseInfo,
            List<string> opac_normal_dbnames,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            if (String.IsNullOrEmpty(strAllDatbaseInfo) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strAllDatbaseInfo);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");

                if (opac_normal_dbnames.IndexOf(strName) == -1)
                    results.Add(strName);
            }

            return 0;
        }

#if NO
        public static void SetLineText(TextBox textbox,
    int nLine,
    string strValue)
        {
            string strText = textbox.Text.Replace("\r\n", "\r");
            string[] lines = strText.Split(new char[] { '\r' });

            strText = "";
            for (int i = 0; i < Math.Max(nLine, lines.Length); i++)
            {
                if (i != 0)
                    strText += "\r\n";

                if (i == nLine)
                    strText += strValue;
                else
                {
                    if (i < lines.Length)
                        strText += lines[i];
                    else
                        strText += "";
                }

            }

            textbox.Text = strText;
        }
#endif

        // 为虚拟库名captions编辑器末尾新增一行
        private void button_virtualDatabaseName_newLine_Click(object sender, EventArgs e)
        {
            this.captionEditControl_virtualDatabaseName.NewElement();
        }

        // 在检索途径定义列表中，新增一个空行，插入到最后
        private void button_froms_newBlankLine_Click(object sender, EventArgs e)
        {
            this.fromEditControl1.NewElement(true);
        }

        // 获得成员数据库名列表
        // 会自动去掉空行
        List<string> GetMemberDatabaseNames()
        {
            List<string> results = new List<string>();
            for (int i = 0; i < this.textBox_memberDatabases.Lines.Length; i++)
            {
                string strLine = this.textBox_memberDatabases.Lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                results.Add(strLine);
            }

            return results;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.tabControl_main.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
        }

        // 导入成员库的全部检索途径(显示时已去重合并)到当前检索途径定义中。
        // 导入当前定义窗时，发现重复的style要警告。对于重复style的<from>允许用户选择保留以前的还是用新的冲入
        private void button_from_import_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            List<string> dbnames = GetMemberDatabaseNames();
            if (dbnames.Count == 0)
            {
                strError = "尚未定义成员库名，因此无法导入成员库的检索途径定义";
                goto ERROR1;
            }

            ImportFromsDialog dlg = new ImportFromsDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            this.EnableControls(false);
            nRet = dlg.Initial(this.ManagerForm,
                dbnames,
                out strError);
            this.EnableControls(true);
            if (nRet == -1)
                goto ERROR1;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (this.fromEditControl1.Elements.Count == 0)
                this.fromEditControl1.Xml = dlg.SelectedFromsXml;
            else
            {
                // 如果当前已经有内容，提醒合并还是替代
                DialogResult result = MessageBox.Show(this,
"当前已存在检索途径配置信息。是否将要导入的内容要合并到当前窗口?\r\n\r\n(Yes: 合并; No: 覆盖; Cancel: 放弃)",
"OpacVirtuslDatabaseDialog",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    // 合并要导入的检索途径定义
                    nRet = MergeImportFroms(dlg.SelectedFromsXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                }
                if (result == DialogResult.No)
                {
                    // 覆盖
                    this.fromEditControl1.Xml = dlg.SelectedFromsXml;
                }
                if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 合并要导入的检索途径定义
        int MergeImportFroms(string strXml,
            out string strError)
        {
            strError = "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            try
            {
                dom.DocumentElement.InnerXml = strXml;
            }
            catch (Exception ex)
            {
                strError = "Set InnerXml error: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("from");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strFromStyle = DomUtil.GetAttr(node, "style");

                bool bFound = false;
                for (int j = 0; j < this.fromEditControl1.Elements.Count; j++)
                {
                    FromElement element = this.fromEditControl1.Elements[j];

                    if (element.Style == strFromStyle)
                    {
                        bFound = true;

                        // TODO: 对于找到的事项，有没有必要对captions进行合并?
                        break;
                    }
                }

                if (bFound == true)
                {
                    continue;
                }

                FromElement new_element = this.fromEditControl1.AppendNewElement();
                new_element.Style = strFromStyle;
                new_element.CaptionsXml = node.InnerXml;
            }

            return 0;
        }
    }
}