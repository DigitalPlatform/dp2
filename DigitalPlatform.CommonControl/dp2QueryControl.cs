using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;


namespace DigitalPlatform.CommonControl
{
    public partial class dp2QueryControl : UserControl
    {
        internal const int WM_SET_BORDERSYTLE = API.WM_USER + 201;


        internal List<dp2QueryLine> Lines = new List<dp2QueryLine>();

        public event GetListEventHandler GetList = null;

        public event EventHandler ViewXml = null;

        public event ApendMenuEventHandler AppendMenu = null;

        public event EventHandler EnterPress = null;

        public event GetFromStyleHandler GetFromStyle = null;

        public dp2QueryControl()
        {
            InitializeComponent();
        }

        public void OnEnterPress(object sender, EventArgs e)
        {
            this.EnterPress(sender, e);
        }

        public bool HasEnterPressEvent
        {
            get
            {
                if (this.EnterPress != null)
                    return true;
                return false;
            }
        }


        PanelMode m_mode = PanelMode.ServerName;

        [Category("Appearance")]
        [DescriptionAttribute("Panel Mode")]
        [DefaultValue(typeof(PanelMode), "ServerName")]
        public PanelMode PanelMode
        {
            get
            {
                return this.m_mode;
            }
            set
            {
                this.m_mode = value;

                if ((this.m_mode & CommonControl.PanelMode.ServerName) != 0)
                {
                    this.label_serverName.Visible = true;

                }
                else
                {
                    this.label_serverName.Visible = false;
                }

                foreach (dp2QueryLine line in this.Lines)
                {
                    line.RefreshMode();
                }
            }
        }

        public int Count
        {
            get
            {
                return this.Lines.Count;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < this.Lines.Count; i++)
            {
                dp2QueryLine line = this.Lines[i];

                line.textBox_word.Text = "";

                // TODO: 需要把逻辑运算符和from恢复到缺省状态
                // line.comboBox_from.Text = DomUtil.GetAttr(node, "from");
                // line.comboBox_logicOperator.Text = DomUtil.GetAttr(node, "logic");
            }
        }

        public string GetSaveString()
        {
            string strResult = "";
            foreach (dp2QueryLine line in this.Lines)
            {
                if (string.IsNullOrEmpty(strResult) == false)
                    strResult += "^";
                strResult += line.GetSaveString();
            }

            return strResult;
        }

        public void Restore(string strSaveString)
        {
            string[] lines = strSaveString.Split(new char[] {'^'});
            for (int i = 0; i < lines.Length; i++)
            {
                if (i >= this.Lines.Count)
                    this.AppendLine();

                this.Lines[i].Restore(lines[i]);
            }
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SET_BORDERSYTLE:
                    {
                        int index = m.WParam.ToInt32();
                        dp2QueryLine line = this.Lines[index];
                        line.ChangeWordBorder(m.LParam.ToInt32());
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        static string BuildFromList(string strDbNameList,
            string strFromList)
        {
            if (strDbNameList.IndexOf(",") == -1)
                return strDbNameList + ":" + strFromList;

            string[] dbname_list = strDbNameList.Split(new char[] { ',' });
            StringBuilder result = new StringBuilder(4096);
            foreach (string dbname in dbname_list)
            {
                string strDbName = dbname.Trim();
                if (string.IsNullOrEmpty(strDbName) == true)
                    continue;
                if (result.Length > 0)
                    result.Append(";");
                result.Append(strDbName + ":" + strFromList);
            }

            return result.ToString();
        }

        // 创建针对同一个服务器的查询 XML
        public int BuildQueryXml(
    int nPerMax,
    string strLang,
    out string strQueryXml,
    out string strError)
        {
            strError = "";
            strQueryXml = "";

            int nQueryCount = 0;
            for (int i = 0; i < this.Lines.Count; i++)
            {
                dp2QueryLine line = this.Lines[i];

                if (i != 0
                    && string.IsNullOrEmpty(line.WordString) == true
                    && line.MatchStyleString != "空值")
                    continue;

                if (string.IsNullOrEmpty(line.DbNameString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定数据库名";
                    return -1;
                }
                if (string.IsNullOrEmpty(line.FromString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定检索途径";
                    return -1;
                }
                if (string.IsNullOrEmpty(line.MatchStyleString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定匹配方式";
                    return -1;
                }

                string strMatchStyle = GetMatchStyle(line.MatchStyleString);
                if (strMatchStyle == "null")
                {
                    if (string.IsNullOrEmpty(line.WordString) == false)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行：要进行(匹配方式为)空值的检索，请保持该行的检索词为空";
                        return -1;
                    }

                    strMatchStyle = "exact";
                }

                string strFromStyles = "";
                if (this.GetFromStyle != null)
                {
                    GetFromStyleArgs e = new GetFromStyleArgs();
                    e.FromCaption = line.FromString;
                    this.GetFromStyle(line, e);
                    strFromStyles = e.FromStyles;
                }

                string strRelation = "=";
                string strDataType = "string";

                if (line.FromString == "__id")
                {
                    // 如果为范围式
                    if (line.WordString.IndexOfAny(new char [] {'-','~'}) != -1)
                    {
                        strRelation = "draw";
                        strDataType = "number";
                    }
                    else if (String.IsNullOrEmpty(line.WordString) == false)
                    {
                        strDataType = "number";
                    }
                }
                else if (StringUtil.IsInList("_time", strFromStyles) == true)
                {
                    // 如果为范围式
                    if (line.WordString.IndexOf("~") != -1)
                    {
                        strRelation = "range";
                        strDataType = "number";
                    }
                    else
                    {
                        strDataType = "number";

                        // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                        // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                        if (strMatchStyle != "exact" && string.IsNullOrEmpty(line.WordString) == true)
                        {
                            strMatchStyle = "exact";
                            strRelation = "range";
                            line.WordString = "~";
                        }
                    }

                    // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                    strMatchStyle = "exact";
                }

                string strFromList = BuildFromList(line.DbNameString, line.FromString);

                string strOneQueryXml = "<target list='"
    + StringUtil.GetXmlStringSimple(strFromList)
    + "'><item>"
    + "<word>"
    + StringUtil.GetXmlStringSimple(line.WordString)
    + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                if (string.IsNullOrEmpty(strQueryXml) == false)
                    strQueryXml += "<operator value='" + line.LogicOperator + "'/>"; ;

                strQueryXml += strOneQueryXml;
                nQueryCount++;
            }

            if (nQueryCount > 1)
                strQueryXml = "<group>" + strQueryXml + "</group>";

            return 0;
        }

        // 创建针对多个服务器的查询XML
        public int BuildQueryXml(
            int nPerMax,
            string strLang,
            out List<QueryItem> items,
            out string strError)
        {
            strError = "";
            items = new List<QueryItem>();

            QueryItem item = new QueryItem();
            items.Add(item);
            int nQueryCount = 0;
            for(int i=0;i<this.Lines.Count;i++)
            {
                dp2QueryLine line = this.Lines[i];

                if (i != 0
                    && string.IsNullOrEmpty(line.WordString) == true
                    && line.MatchStyleString != "空值")
                    continue;

                if (string.IsNullOrEmpty(line.ServerNameString) == true)
                {
                    strError = "第 "+(i+1).ToString()+" 行：尚未指定服务器名";
                    return -1;
                }
                if (line.ServerNameString.IndexOf(",") != -1)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：服务器名中不能有逗号";
                    return -1;
                } 
                if (string.IsNullOrEmpty(line.DbNameString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定数据库名";
                    return -1;
                }
                if (string.IsNullOrEmpty(line.FromString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定检索途径";
                    return -1;
                }
                if (string.IsNullOrEmpty(line.MatchStyleString) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行：尚未指定匹配方式";
                    return -1;
                }

                string strMatchStyle = GetMatchStyle(line.MatchStyleString);
                if (strMatchStyle == "null")
                {
                    if (string.IsNullOrEmpty(line.WordString) == false)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行：要进行(匹配方式为)空值的检索，请保持该行的检索词为空";
                        return -1;
                    }

                    strMatchStyle = "exact";
                }

                string strRelation = "=";
                string strDataType = "string";

                if (line.FromString == "__id")
                {
                    // 如果为范围式
                    if (line.WordString.IndexOf("~") != -1)
                    {
                        strRelation = "draw";
                        strDataType = "number";
                    }
                    else if (String.IsNullOrEmpty(line.WordString) == false)
                    {
                        strDataType = "number";
                    }
                }
                string strFromList = BuildFromList(line.DbNameString, line.FromString);

                if (String.IsNullOrEmpty(item.ServerName) == true)
                    item.ServerName = line.ServerNameString;
                else
                {
                    if (item.ServerName != line.ServerNameString)
                    {
                        if (line.LogicOperator == "AND"
                            || line.LogicOperator == "SUB")
                        {
                            strError = "第 " + (i + 1).ToString() + " 行： 不同服务器之间不能进行 AND 和 SUB 逻辑运算。(可以进行 OR 逻辑运算)";
                            return -1;
                        }

                        if (nQueryCount > 1)
                            item.QueryXml = "<group>" + item.QueryXml + "</group>";

                        item = new QueryItem();
                        items.Add(item);
                        item.ServerName = line.ServerNameString;
                        nQueryCount = 0;
                    }
                }



                string strOneQueryXml = "<target list='"
    + StringUtil.GetXmlStringSimple(strFromList)
    + "'><item>"
    + "<word>"
    + StringUtil.GetXmlStringSimple(line.WordString)
    + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                if (string.IsNullOrEmpty(item.QueryXml) == false)
                    item.QueryXml += "<operator value='" + line.LogicOperator + "'/>"; ;

                item.QueryXml += strOneQueryXml;
                nQueryCount++;
            }

            if (nQueryCount > 1)
                item.QueryXml = "<group>" + item.QueryXml + "</group>";

            return 0;
        }

        public static string GetMatchStyle(string strText)
        {
            // string strText = this.comboBox_matchStyle.Text;

            // 2009/8/6
            if (strText == "空值")
                return "null";

            if (String.IsNullOrEmpty(strText) == true)
                return "left"; // 缺省时认为是 前方一致

            if (strText == "前方一致")
                return "left";
            if (strText == "中间一致")
                return "middle";
            if (strText == "后方一致")
                return "right";
            if (strText == "精确一致")
                return "exact";

            return strText; // 直接返回原文
        }

        public void DoGetList(object sender, GetListEventArgs e)
        {
            if (this.GetList != null)
                this.GetList(sender, e);
        }

        // 新增加一行
        public void AppendLine()
        {
            this.DisableUpdate();
            try
            {
                int nLastRow = this.tableLayoutPanel_main.RowCount - 1;

                this.tableLayoutPanel_main.RowCount += 1;
                this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());

                dp2QueryLine line = new dp2QueryLine(this);

                line.AddToTable(this.tableLayoutPanel_main, nLastRow);

                this.Lines.Add(line);
                line.RefreshState();
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public void RemoveTailLine()
        {
            if (this.Lines.Count == 0)
                return;

            this.DisableUpdate();
            try
            {
                dp2QueryLine line = this.Lines[this.Lines.Count - 1];
                line.RemoveFromTable(this.tableLayoutPanel_main);

                this.tableLayoutPanel_main.RowCount -= 1;

                this.Lines.RemoveAt(this.Lines.Count - 1);
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public override bool Focused
        {
            get
            {
                if (base.Focused == true)
                    return true;

                for (int i = 0; i < this.Lines.Count; i++)
                {
                    dp2QueryLine line = this.Lines[i];

                    if (line.textBox_word.Focused == true)
                        return true;
                    if (line.comboBox_from.Focused == true)
                        return true;
                    if (line.comboBox_logicOperator.Focused == true)
                        return true;
                    if (line.comboBox_server.Focused == true)
                        return true;
                    if (line.comboBox_dbName.Focused == true)
                        return true;
                    if (line.comboBox_matchStyle.Focused == true)
                        return true;
                }

                return false;
            }
        }

        private void tableLayoutPanel_main_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            if (this.AppendMenu != null)
            {
                AppendMenuEventArgs e1 = new AppendMenuEventArgs();
                e1.ContextMenu = contextMenu;
                this.AppendMenu(this, e1);
            }

            if (this.ViewXml != null)
            {
                //
                menuItem = new MenuItem("察看XML检索式(&X)");
                menuItem.Click += new System.EventHandler(this.menu_viewQueryXml_Click);
                contextMenu.MenuItems.Add(menuItem);

                // ---
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);
            }


            //
            menuItem = new MenuItem("增补新行(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendLine_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("删除末尾行(&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeTailLine_Click);
            if (this.Lines.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show((Control)sender, new Point(e.X, e.Y));
        }


        void menu_viewQueryXml_Click(object sender, EventArgs e)
        {
            if (this.ViewXml != null)
                this.ViewXml(sender, e);
        }

        void menu_appendLine_Click(object sender, EventArgs e)
        {
            this.AppendLine();
        }

        void menu_removeTailLine_Click(object sender, EventArgs e)
        {
            this.RemoveTailLine();
        }

        int m_nInSuspend = 0;
        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_main.SuspendLayout();
            }

            this.m_nInSuspend++;
        }

        public void EnableUpdate()
        {
            this.m_nInSuspend--;

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_main.ResumeLayout(false);
                this.tableLayoutPanel_main.PerformLayout();
            }
        }

        private void label_logic_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);
        }

        private void label_serverName_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);

        }

        private void label_database_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);

        }

        private void label_word_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);

        }

        private void label_from_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);

        }

        private void label_matchStyle_MouseUp(object sender, MouseEventArgs e)
        {
            tableLayoutPanel_main_MouseUp(sender, e);

        }
    }

    public class dp2QueryLine : IDisposable
    {
        public dp2QueryControl Container = null;

        public Label label_state = null;
        public ComboBox comboBox_logicOperator = null;
        public ComboBox comboBox_server = null;
        public ComboBox comboBox_dbName = null;
        public TextBox textBox_word = null;
        public ComboBox comboBox_from = null;
        public ComboBox comboBox_matchStyle = null;

        void DisposeChildControls()
        {
            label_state.Dispose();
            comboBox_logicOperator.Dispose();
            comboBox_server.Dispose();
            comboBox_dbName.Dispose();
            textBox_word.Dispose();
            comboBox_from.Dispose();
            comboBox_matchStyle.Dispose();
            Container = null;
        }

        #region 释放资源

        ~dp2QueryLine()
        {
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // release managed resources if any
                    AddEvents(false);
                    DisposeChildControls();
                }

                // release unmanaged resource

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            disposed = true;
        }

        #endregion


        public string LogicOperatorString
        {
            get
            {
                return this.comboBox_logicOperator.Text;
            }
            set
            {
                this.comboBox_logicOperator.Text = value;
            }
        }

        // 纯粹的逻辑操作符号 AND OR SUB
        public string LogicOperator
        {
            get
            {
                string strTemp = this.comboBox_logicOperator.Text;
                int nRet = strTemp.IndexOf(" ");
                if (nRet != -1)
                    return strTemp.Substring(0, nRet);
                return strTemp;
            }
        }

        public string ServerNameString
        {
            get
            {
                return this.comboBox_server.Text;
            }
            set
            {
                this.comboBox_server.Text = value;
            }
        }

        public string DbNameString
        {
            get
            {
                return this.comboBox_dbName.Text;
            }
            set
            {
                this.comboBox_dbName.Text = value;
            }
        }

        public string WordString
        {
            get
            {
                return this.textBox_word.Text;
            }
            set
            {
                this.textBox_word.Text = value;
            }
        }

        public string FromString
        {
            get
            {
                return this.comboBox_from.Text;
            }
            set
            {
                this.comboBox_from.Text = value;
            }
        }

        public string MatchStyleString
        {
            get
            {
                return this.comboBox_matchStyle.Text;
            }
            set
            {
                this.comboBox_matchStyle.Text = value;

                this.RefreshState();
            }
        }

        public string GetSaveString()
        {
            return this.comboBox_logicOperator.Text
                + "|" + this.comboBox_server.Text
                + "|" + this.comboBox_dbName.Text
                + "|" + this.textBox_word.Text.Replace("|", "").Replace("^","")
                + "|" + this.comboBox_from.Text
                + "|" + this.comboBox_matchStyle.Text;
        }

        int m_nDisableEvents = 0;

        public void Restore(string strSaveString)
        {
            m_nDisableEvents++;
            try
            {
                string[] parts = strSaveString.Split(new char[] { '|' });

                if (parts.Length > 0)
                    this.comboBox_logicOperator.Text = parts[0];

                if (parts.Length > 1)
                    this.comboBox_server.Text = parts[1];
                if (parts.Length > 2)
                    this.comboBox_dbName.Text = parts[2];
                if (parts.Length > 3)
                    this.textBox_word.Text = parts[3];
                if (parts.Length > 4)
                    this.comboBox_from.Text = parts[4];
                if (parts.Length > 5)
                    this.comboBox_matchStyle.Text = parts[5];

                this.RefreshState();
            }
            finally
            {
                m_nDisableEvents--;
            }
        }

        public dp2QueryLine(dp2QueryControl control)
        {
            this.Container = control;

            label_state = new Label();
            label_state.Dock = DockStyle.Fill;
            label_state.AutoSize = true;
            label_state.Image = this.Container.imageList_states.Images[0];
            label_state.ImageAlign = ContentAlignment.MiddleCenter;

            comboBox_logicOperator = new ComboBox();
            comboBox_logicOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_logicOperator.FlatStyle = FlatStyle.Flat;
            comboBox_logicOperator.Dock = DockStyle.Fill;
            comboBox_logicOperator.MaximumSize = new Size(150, 28);
            comboBox_logicOperator.Size = new Size(80, 28);
            comboBox_logicOperator.MinimumSize = new Size(50, 28);
            comboBox_logicOperator.Items.AddRange(new object[] {
                "AND 与",
                "OR  或",
                "SUB 减",
            });
            comboBox_logicOperator.Text = "AND 与";

            // servers
            comboBox_server = new ComboBox();
            // comboBox_server.DropDownStyle = ComboBoxStyle.DropDownList;
            if ((this.Container.PanelMode & PanelMode.ServerName) == 0)
                comboBox_server.Visible = false;
            comboBox_server.FlatStyle = FlatStyle.Flat;
            comboBox_server.Dock = DockStyle.Fill;
            comboBox_server.MaximumSize = new Size(150, 28);
            comboBox_server.Size = new Size(120, 28);
            comboBox_server.MinimumSize = new Size(100, 28);
            comboBox_server.Text = "";
            comboBox_server.DropDownWidth = 150;

            // dbname
            comboBox_dbName = new ComboBox();
            // comboBox_dbName.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_dbName.FlatStyle = FlatStyle.Flat;
            comboBox_dbName.Dock = DockStyle.Fill;
            comboBox_dbName.MaximumSize = new Size(150, 28);
            comboBox_dbName.Size = new Size(120, 28);
            comboBox_dbName.MinimumSize = new Size(100, 28);
            comboBox_dbName.Text = "";
            comboBox_dbName.DropDownWidth = 150;

            //
            textBox_word = new TextBox();
            textBox_word.BorderStyle = BorderStyle.None;
            textBox_word.Font = new Font(this.Container.Font, FontStyle.Bold);
            textBox_word.Dock = DockStyle.Fill;
            textBox_word.MaximumSize = new Size(200, 28);
            textBox_word.Size = new Size(150, 28);
            textBox_word.MinimumSize = new Size(100, 28);
            // textBox_word.Margin = new Padding(-1, -1, -1, -1);

            //
            comboBox_from = new ComboBox();
            // comboBox_from.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_from.FlatStyle = FlatStyle.Flat;
            comboBox_from.DropDownHeight = 300;
            comboBox_from.DropDownWidth = 200;
            comboBox_from.Dock = DockStyle.Fill;
            comboBox_from.MaximumSize = new Size(150, 28);
            comboBox_from.Size = new Size(100, 28);
            comboBox_from.MinimumSize = new Size(50, 28);

            // matchstyle
            comboBox_matchStyle = new ComboBox();
            comboBox_matchStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_matchStyle.FlatStyle = FlatStyle.Flat;
            comboBox_matchStyle.Dock = DockStyle.Fill;
            comboBox_matchStyle.MaximumSize = new Size(150, 28);
            comboBox_matchStyle.Size = new Size(80, 28);
            comboBox_matchStyle.MinimumSize = new Size(50, 28);
            comboBox_matchStyle.Items.AddRange(new object[] {
                "前方一致",
                "中间一致",
                "后方一致",
                "精确一致",
                "空值",
            });
            comboBox_matchStyle.Text = "前方一致";

            AddEvents(true);
        }

        void AddEvents(bool bAdd)
        {
            if (bAdd)
            {
                label_state.MouseUp += new MouseEventHandler(label_state_MouseUp);

                label_state.MouseHover += new EventHandler(label_state_MouseHover);
                comboBox_logicOperator.SizeChanged += new EventHandler(comboBox_SizeChanged);
                comboBox_server.DropDown += new EventHandler(comboBox_server_DropDown);
                comboBox_server.TextChanged += new EventHandler(comboBox_server_TextChanged);
                comboBox_server.DropDownClosed += new EventHandler(comboBox_server_TextChanged);
                comboBox_server.SizeChanged += new EventHandler(comboBox_SizeChanged);
                comboBox_dbName.DropDown += new EventHandler(comboBox_dbName_DropDown);
                comboBox_dbName.TextChanged += new EventHandler(comboBox_dbName_TextChanged);
                comboBox_dbName.SizeChanged += new EventHandler(comboBox_SizeChanged);
                textBox_word.TextChanged += new EventHandler(textBox_word_TextChanged);
                textBox_word.KeyDown += new KeyEventHandler(textBox_word_KeyDown);
                comboBox_from.DropDown += new EventHandler(comboBox_from_DropDown);
                comboBox_from.SizeChanged += new EventHandler(comboBox_SizeChanged);
                comboBox_matchStyle.SizeChanged += new EventHandler(comboBox_SizeChanged);
                comboBox_matchStyle.DropDownClosed += new EventHandler(comboBox_matchStyle_DropDownClosed);
            }
            else
            {
                if (label_state != null)
                {
                    label_state.MouseUp -= new MouseEventHandler(label_state_MouseUp);
                    label_state.MouseHover -= new EventHandler(label_state_MouseHover);
                }
                if (comboBox_logicOperator != null)
                    comboBox_logicOperator.SizeChanged -= new EventHandler(comboBox_SizeChanged);
                if (comboBox_server != null)
                {
                    comboBox_server.DropDown -= new EventHandler(comboBox_server_DropDown);
                    comboBox_server.TextChanged -= new EventHandler(comboBox_server_TextChanged);
                    comboBox_server.DropDownClosed -= new EventHandler(comboBox_server_TextChanged);
                    comboBox_server.SizeChanged -= new EventHandler(comboBox_SizeChanged);
                }
                if (comboBox_dbName != null)
                {
                    comboBox_dbName.DropDown -= new EventHandler(comboBox_dbName_DropDown);
                    comboBox_dbName.TextChanged -= new EventHandler(comboBox_dbName_TextChanged);
                    comboBox_dbName.SizeChanged -= new EventHandler(comboBox_SizeChanged);
                }
                if (textBox_word != null)
                {
                    textBox_word.TextChanged -= new EventHandler(textBox_word_TextChanged);
                    textBox_word.KeyDown -= new KeyEventHandler(textBox_word_KeyDown);
                }
                if (comboBox_from != null)
                {
                    comboBox_from.DropDown -= new EventHandler(comboBox_from_DropDown);
                    comboBox_from.SizeChanged -= new EventHandler(comboBox_SizeChanged);
                }
                if (comboBox_matchStyle != null)
                {
                    comboBox_matchStyle.SizeChanged -= new EventHandler(comboBox_SizeChanged);
                    comboBox_matchStyle.DropDownClosed -= new EventHandler(comboBox_matchStyle_DropDownClosed);
                }
            }
        }

        void textBox_word_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        int index = this.Container.Lines.IndexOf(this);
                        if (index > 0)
                        {
                            index--;
                            this.Container.Lines[index].textBox_word.Focus();
                            e.Handled = true;
                        }
                    }
                    break;
                case Keys.Down:
                     {
                        int index = this.Container.Lines.IndexOf(this);
                        if (index < this.Container.Lines.Count - 1)
                        {
                            index++;
                            this.Container.Lines[index].textBox_word.Focus();
                            e.Handled = true;
                        }
                    }
                    break;
                case Keys.Enter:
                    if (this.Container.HasEnterPressEvent == true)
                    {
                        this.Container.OnEnterPress(this, e);
                        // e.Handled = true;
                    }
                    break;

            }
        }

        void comboBox_matchStyle_DropDownClosed(object sender, EventArgs e)
        {
            RefreshState();
        }

        void label_state_MouseHover(object sender, EventArgs e)
        {
            int index = this.Container.Lines.IndexOf(this);

            bool bHasWord = false;
            if (index == 0
                || this.textBox_word.Text != ""
                || this.MatchStyleString == "空值")
            {
                bHasWord = true;
            }
            else
            {
                bHasWord = false;
            }

            string strText = "";
            if (bHasWord == true)
            {
                strText = "只有输入了检索词的行(或者匹配方式为“空值”的行)才会在检索中起作用";
            }
            else
            {
                strText = "没有输入检索词的行在检索时会被忽略";
            }

            if (index == 0)
                strText = "第一行无论是否输入了检索词，都会在检索中起作用";

            this.Container.toolTip1.Show(strText, this.label_state, 5000);
        }

        void label_state_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            //
            menuItem = new MenuItem("输入时间值");
            contextMenu.MenuItems.Add(menuItem);

            MenuItem subMenuItem;
            subMenuItem = new MenuItem("RFC1123时间值...");
            subMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("u时间值...");
            subMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_uSingle_Click);
            menuItem.MenuItems.Add(subMenuItem);

            // ---
            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("RFC1123时间值范围...");
            subMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Range_Click);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("u时间值范围...");
            subMenuItem.Click += new System.EventHandler(this.ToolStripMenuItem_uRange_Click);
            menuItem.MenuItems.Add(subMenuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);




            //
            menuItem = new MenuItem("增补新行(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendLine_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("删除末尾行(&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeTailLine_Click);
            if (this.Container.Lines.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.label_state, new Point(e.X, e.Y));
        }


        private void ToolStripMenuItem_rfc1123Single_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.RangeMode = false;
            try
            {
                dlg.Rfc1123String = this.textBox_word.Text;
            }
            catch
            {
                this.textBox_word.Text = "";
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this.Container);


            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_word.Text = dlg.Rfc1123String;
        }

        private void ToolStripMenuItem_uSingle_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.RangeMode = false;
            try
            {
                dlg.uString = this.textBox_word.Text;
            }
            catch
            {
                this.textBox_word.Text = "";
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this.Container);


            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_word.Text = dlg.uString;

        }

        private void ToolStripMenuItem_rfc1123Range_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.RangeMode = true;
            // 分割为两个字符串
            try
            {
                dlg.Rfc1123String = this.textBox_word.Text;
            }
            catch
            {
                this.textBox_word.Text = "";
            }
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this.Container);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_word.Text = dlg.Rfc1123String;

        }

        private void ToolStripMenuItem_uRange_Click(object sender, EventArgs e)
        {
            GetTimeDialog dlg = new GetTimeDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.RangeMode = true;
            try
            {
                dlg.uString = this.textBox_word.Text;
            }
            catch
            {
                this.textBox_word.Text = "";
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this.Container);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_word.Text = dlg.uString;
        }

        void menu_appendLine_Click(object sender, EventArgs e)
        {
            this.Container.AppendLine();
        }

        void menu_removeTailLine_Click(object sender, EventArgs e)
        {
            this.Container.RemoveTailLine();
        }

        public void RefreshMode()
        {
            if ((this.Container.PanelMode & PanelMode.ServerName) != 0)
                this.comboBox_server.Visible = true;
            else
                this.comboBox_server.Visible = false;
        }

        public void RefreshState()
        {
            int index = this.Container.Lines.IndexOf(this);

            if (index == 0
                || this.textBox_word.Text != ""
                || this.comboBox_matchStyle.Text == "空值")
            {
                // 如果不是这样,会造成输入汉字词组的时候只能上去一个字
                API.PostMessage(this.Container.Handle, dp2QueryControl.WM_SET_BORDERSYTLE, index, 1);
                // this.textBox_word.BorderStyle = BorderStyle.FixedSingle;
                label_state.Image = this.Container.imageList_states.Images[1];
            }
            else
            {
                API.PostMessage(this.Container.Handle, dp2QueryControl.WM_SET_BORDERSYTLE, index, 0);
                // this.textBox_word.BorderStyle = BorderStyle.None;
                label_state.Image = this.Container.imageList_states.Images[0];
            }
        }

        public void ChangeWordBorder(int on)
        {
            if (on != 0)
            {
                this.textBox_word.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                this.textBox_word.BorderStyle = BorderStyle.None;
            }
        }

        void textBox_word_TextChanged(object sender, EventArgs e)
        {
            RefreshState();
#if NO
            TextBox textbox = (TextBox)sender;

            int index = this.Container.Lines.IndexOf(this);

            if (index == 0 || textbox.Text != "")
            {
                textbox.BorderStyle = BorderStyle.FixedSingle;
                label_state.Image = this.Container.imageList_states.Images[1];
            }
            else
            {
                textbox.BorderStyle = BorderStyle.None;
                label_state.Image = this.Container.imageList_states.Images[0];
            }
#endif
        }

        void comboBox_SizeChanged(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;
            combobox.Invalidate();
        }

        public bool HasServerColumn
        {
            get
            {
                return ((this.Container.PanelMode & PanelMode.ServerName) != 0);
            }
        }

        void FillFromList(bool bMessageBox = true)
        {
            if (this.comboBox_from.Items.Count > 0)
                return;

            // TODO: 如果Container没有挂接GetList事件，则可以尽早返回

            if (HasServerColumn == true)
            {
                if (string.IsNullOrEmpty(this.comboBox_server.Text) == true)
                {
                    if (bMessageBox == true)
                        MessageBox.Show(this.Container, "请先选定服务器");
                    return;
                }
            }

            if (string.IsNullOrEmpty(this.comboBox_dbName.Text) == true)
            {
                if (bMessageBox == true)
                    MessageBox.Show(this.Container, "请先选定数据库");
                return;
            }

            GetListEventArgs e1 = new GetListEventArgs();
            e1.Path = (HasServerColumn == true ? this.comboBox_server.Text + "/" : "")
                + this.comboBox_dbName.Text;
            this.Container.DoGetList(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                if (bMessageBox == true)
                    MessageBox.Show(this.Container, e1.ErrorInfo);
                return;
            }

            foreach (string value in e1.Values)
            {
                comboBox_from.Items.Add(value);
            }
        }

        void comboBox_from_DropDown(object sender, EventArgs e)
        {
            // ComboBox combobox = (ComboBox)sender;

            FillFromList();

        }

        void comboBox_dbName_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableEvents > 0)
            {
                this.comboBox_dbName.Tag = this.comboBox_dbName.Text;
                return;
            }

            // 避免两次触发事件
            string strOldValue = (string)this.comboBox_dbName.Tag;
            if (this.comboBox_dbName.Text == strOldValue)
                return;
            this.comboBox_dbName.Tag = this.comboBox_dbName.Text;

            this.comboBox_from.Items.Clear();
            this.FillFromList(false);

            if (string.IsNullOrEmpty(this.comboBox_from.Text) == true
               && this.comboBox_from.Items.Count > 0)
            {
                this.comboBox_from.Text = (string)this.comboBox_from.Items[0];
            }
            else if (this.comboBox_from.Items.IndexOf(this.comboBox_from.Text) == -1)
                this.comboBox_from.Text = "";
        }

        void FillDbNameList(bool bMessageBox = true)
        {
            if (this.comboBox_dbName.Items.Count > 0)
                return;

            if (HasServerColumn == true)
            {
                if (string.IsNullOrEmpty(this.comboBox_server.Text) == true)
                {
                    if (bMessageBox == true)
                        MessageBox.Show(this.Container, "请先选定服务器");
                    return;
                }
            }

            GetListEventArgs e1 = new GetListEventArgs();
            if (HasServerColumn == true)
                e1.Path = this.comboBox_server.Text;
            else
                e1.Path = "";
            this.Container.DoGetList(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                if (bMessageBox == true)
                    MessageBox.Show(this.Container, e1.ErrorInfo);
                return;
            }

            foreach (string value in e1.Values)
            {
                comboBox_dbName.Items.Add(value);
            }
        }

        void comboBox_dbName_DropDown(object sender, EventArgs e)
        {
            // ComboBox combobox = (ComboBox)sender;

            FillDbNameList();
        }

        void comboBox_server_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nDisableEvents > 0)
            {
                this.comboBox_server.Tag = this.comboBox_server.Text;
                return;
            }

            // 避免两次触发事件
            string strOldValue = (string)this.comboBox_server.Tag;
            if (this.comboBox_server.Text == strOldValue)
                return;
            this.comboBox_server.Tag = this.comboBox_server.Text;

            this.comboBox_dbName.Items.Clear();
            this.FillDbNameList(false);

            if (string.IsNullOrEmpty(this.comboBox_dbName.Text) == true
                && this.comboBox_dbName.Items.Count > 0)
            {
                this.comboBox_dbName.Text = (string)this.comboBox_dbName.Items[0];
            }
            else if (this.comboBox_dbName.Items.IndexOf(this.comboBox_dbName.Text) == -1)
                this.comboBox_dbName.Text = "";

            this.comboBox_from.Items.Clear();
            this.FillFromList(false);

            if (string.IsNullOrEmpty(this.comboBox_from.Text) == true
               && this.comboBox_from.Items.Count > 0)
            {
                this.comboBox_from.Text = (string)this.comboBox_from.Items[0];
            }
            else if (this.comboBox_from.Items.IndexOf(this.comboBox_from.Text) == -1)
                this.comboBox_from.Text = "";
        }

        void FillServerNameList()
        {
            if (this.comboBox_server.Items.Count > 0)
                return;

            GetListEventArgs e1 = new GetListEventArgs();
            this.Container.DoGetList(this, e1);
            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                MessageBox.Show(this.Container, e1.ErrorInfo);
                return;
            }

            foreach (string value in e1.Values)
            {
                comboBox_server.Items.Add(value);
            }
        }

        void comboBox_server_DropDown(object sender, EventArgs e)
        {
            // ComboBox combobox = (ComboBox)sender;

            FillServerNameList();
        }

        public void AddToTable(TableLayoutPanel table,
            int nRow)
        {
            table.Controls.Add(this.label_state, 0, nRow);
            table.Controls.Add(this.comboBox_logicOperator, 1, nRow);
            table.Controls.Add(this.comboBox_server, 2, nRow);
            table.Controls.Add(this.comboBox_dbName, 3, nRow);
            table.Controls.Add(this.textBox_word, 4, nRow);
            table.Controls.Add(this.comboBox_from, 5, nRow);
            table.Controls.Add(this.comboBox_matchStyle, 6, nRow);

            if (nRow == 1)
            {
                this.comboBox_logicOperator.Enabled = false;
            }
        }

        public void RemoveFromTable(TableLayoutPanel table)
        {
            table.Controls.Remove(this.label_state);
            table.Controls.Remove(this.comboBox_logicOperator);
            table.Controls.Remove(this.comboBox_server);
            table.Controls.Remove(this.comboBox_dbName);
            table.Controls.Remove(this.textBox_word);
            table.Controls.Remove(this.comboBox_from);
            table.Controls.Remove(this.comboBox_matchStyle);
        }
    }

    // 获得值列表
    public delegate void GetListEventHandler(object sender,
GetListEventArgs e);

    /// <summary>
    /// 获得值列表的参数
    /// </summary>
    public class GetListEventArgs : EventArgs
    {
        public string Path = "";    // [in]
        public List<string> Values = new List<string>();    // [out]
        public string ErrorInfo = "";   // [out]
    }

    public class QueryItem
    {
        public string ServerName = "";
        public string QueryXml = "";
    }

    [Flags]
    public enum PanelMode
    {
        None = 0,
        ServerName = 0x01,  // 显示 服务器名 列
    }

    // 获得检索途径的style字符串
    public delegate void GetFromStyleHandler(object sender,
GetFromStyleArgs e);

    public class GetFromStyleArgs : EventArgs
    {
        public string FromCaption = ""; // [in]
        public string FromStyles = "";  // [out]
    }
}
