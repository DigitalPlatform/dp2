using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 和 创建dp2数据库 有关的代码
    /// </summary>
    public partial class MainForm : Form
    {
        // List<XmlDocument> existing_database_doms = new List<XmlDocument>();

        // 创建新的数据库。创建之前，还要删除已经存在的同名数据库
        // return:
        //      -1  出错
        //      0   放弃删除和创建
        //      1   成功
        int CreateNewDp2Databases(out string strError)
        {
            int nRet = 0;
            strError = "";

            string strDatabaseInfo = "";
            string strOutputInfo = "";

            // 删除的数据库名
            List<string> delete_dbnames = new List<string>();
            // 创建的数据库名
            List<string> create_dbnames = new List<string>();

            /*
            // 用于删除命令的DOM
            XmlDocument delete_dom = new XmlDocument();
            delete_dom.LoadXml("<root />");
             * */

            // 用于创建命令的DOM
            XmlDocument create_dom = new XmlDocument();
            create_dom.LoadXml("<root />");

            for (int i = 0; i < this.listView_creatingDp2DatabaseList.Items.Count; i++)
            {
                ListViewItem item = this.listView_creatingDp2DatabaseList.Items[i];

                if (item.ImageIndex == 1)
                    continue;

                string strType = ListViewUtil.GetItemText(item, 1);

                create_dbnames.Add(item.Text);

                // 需要新创建的
                if (item.ImageIndex == 2 || item.ImageIndex == 0)
                {
                    XmlNode nodeCreateDatabase = create_dom.CreateElement("database");
                    create_dom.DocumentElement.AppendChild(nodeCreateDatabase);

                    // name
                    DomUtil.SetAttr(nodeCreateDatabase,
                        "name",
                        item.Text);

                    if (StringUtil.IsInList("书目库", strType) == true)
                    {
                        // 需要创建书目库

                        // type
                        DomUtil.SetAttr(nodeCreateDatabase, "type", "biblio");

                        // syntax
                        string strSyntax = "unimarc";
                        if (StringUtil.IsInList("unimarc", strType, true) == true)
                            strSyntax = "unimarc";
                        else if (StringUtil.IsInList("usmarc", strType, true) == true)
                            strSyntax = "usmarc";

                        DomUtil.SetAttr(nodeCreateDatabase, "syntax", strSyntax);

                        // usage
                        string strUsage = "book";
                        if (StringUtil.IsInList("图书", strType, true) == true)
                            strUsage = "book";
                        else if (StringUtil.IsInList("期刊", strType, true) == true)    // BUG !!! 2009/1/1 changed
                            strUsage = "series";
                        DomUtil.SetAttr(nodeCreateDatabase, "usage", strUsage);


                        // 实体库名
                        if (StringUtil.IsInList("实体", strType, true) == true) // 2008/10/15 new add
                        {
                            DomUtil.SetAttr(nodeCreateDatabase,
                                "entityDbName",
                                item.Text + "实体");
                        }

                        // 订购库名
                        if (StringUtil.IsInList("采购", strType, true) == true)
                        {
                            DomUtil.SetAttr(nodeCreateDatabase,
                                "orderDbName",
                                item.Text + "订购");
                        }

                        // 检查期库名的具备和usage是否矛盾
                        if (strUsage == "series")
                        {
                            DomUtil.SetAttr(nodeCreateDatabase,
                                "issueDbName",
                                item.Text + "期");
                        }

                    } // end of if 书目库
                    else if (StringUtil.IsInList("读者库", strType) == true)
                    {
                        // type
                        DomUtil.SetAttr(nodeCreateDatabase, "type", "reader");
                    }

                    if (StringUtil.IsInList("参与流通", strType) == true)
                    {
                        DomUtil.SetAttr(nodeCreateDatabase, "inCirculation", "true");
                    }
                    else
                    {
                        DomUtil.SetAttr(nodeCreateDatabase, "inCirculation", "false");
                    }
                }

                // 需要先加以删除的
                if (item.ImageIndex == 2)
                {
                    /*
                    XmlNode nodeDeleteDatabase = delete_dom.CreateElement("database");
                    delete_dom.DocumentElement.AppendChild(nodeDeleteDatabase);

                    // name
                    DomUtil.SetAttr(nodeDeleteDatabase,
                        "name",
                        item.Text);
                     * */

                    delete_dbnames.Add(item.Text);
                }

            }

            // 提示修改的数据库名，要删除的数据库，要创建的数据库
            string strText = "";
            if (delete_dbnames.Count > 0)
            {
                strText += "要删除下列数据库:\r\n---\r\n";
                strText += Global.MakeListString(delete_dbnames, "\r\n");
                strText += "\r\n\r\n***警告: 数据库被删除后，其中的数据再也无法复原！\r\n";
                strText += "\r\n";
            }

            if (create_dbnames.Count > 0)
            {
                strText += "要创建下列数据库:\r\n---\r\n";
                strText += Global.MakeListString(create_dbnames, "\r\n");
                strText += "\r\n";
            }

            // 对话框警告
            DialogResult result = MessageBox.Show(this,
                strText + "\r\n确实要继续?",
                "UpgradeDt1000ToDp2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return 0;

            AppendHtml(
"====================<br/>"
+ "创建数据库<br/>"
+ "====================<br/><br/>");


            string strInfo = "";
            if (delete_dbnames.Count > 0)
            {
                strInfo += "在创建前，删除了下列和即将创建数据库重名的数据库:\r\n---\r\n";
                strInfo += Global.MakeListString(delete_dbnames, ",");
                strInfo += "\r\n\r\n";
            }

            if (create_dbnames.Count > 0)
            {
                strInfo += "创建了下列数据库:\r\n---\r\n";
                strInfo += Global.MakeListString(create_dbnames, ",");
                strInfo += "\r\n(注：大书目库可能包含下属的实体库、采购库、期库，在这里仅列出了书目库名)\r\n";
                strInfo += "\r\n\r\n";
            }

            // 删除数据库
            if (delete_dbnames.Count > 0)
            {
                // 删除数据库
                nRet = this.DeleteDatabase(
                    Global.MakeListString(delete_dbnames, ","),
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 创建数据库
            XmlNodeList nodes = create_dom.DocumentElement.SelectNodes("database");
            if (nodes.Count > 0)
            {
                strDatabaseInfo = create_dom.OuterXml;

                // 创建数据库
                nRet = this.CreateDatabase(
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            this.textBox_createDp2DatabaseSummary.Text = strInfo;

            AppendHtml(strInfo.Replace("\r\n", "<br/>"));

            return 1;
        ERROR1:
            return -1;
        }

        // 创建 dp2 系统的简单库(除种次号库外)
        int CreateDp2SimpleDatabases(out string strError)
        {
            strError = "";
            int nRet = 0;

            AppendHtml(
"====================<br/>"
+ "创建辅助数据库<br/>"
+ "====================<br/><br/>");


            // 创建的数据库名
            List<string> creating_dbnames = new List<string>();
            List<string> dbtypes = new List<string>();

            creating_dbnames.Add("违约金");
            dbtypes.Add("amerce");

            creating_dbnames.Add("预约到书");
            dbtypes.Add("arrived");

            creating_dbnames.Add("出版者");
            dbtypes.Add("publisher");

            creating_dbnames.Add("消息");
            dbtypes.Add("message");

            /*
             * 种次号库则要根据需要创建。还需要创建<zhongcihao>配置小节
            creating_dbnames.Add("种次号库");
            dbtypes.Add("zhongcihao");
             * */

            string strDatabaseInfo = "";
            string strOutputInfo = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            List<string> created_dbnames = new List<string>();

            for (int i = 0; i < creating_dbnames.Count; i++)
            {
                string strDatabaseName = creating_dbnames[i];
                string strDatabaseType = dbtypes[i];

                // 如果已经存在那个类型的辅助库
                if (ExistingDp2DatabaseType(strDatabaseType) == true)
                    continue;

                XmlNode nodeDatabase = dom.CreateElement("database");
                dom.DocumentElement.AppendChild(nodeDatabase);

                // type
                DomUtil.SetAttr(nodeDatabase, "type", strDatabaseType);

                nRet = Global.CheckDbName(strDatabaseName,
                    out strError);
                if (nRet == -1)
                    return -1;

                DomUtil.SetAttr(nodeDatabase,
                    "name",
                    strDatabaseName);

                created_dbnames.Add(strDatabaseName);
            }

            if (created_dbnames.Count > 0)  // 2017/8/11
            {
                strDatabaseInfo = dom.OuterXml;

                // 创建数据库
                nRet = this.CreateDatabase(
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            string strInfo = "";
            if (created_dbnames.Count > 0)
            {
                strInfo += "新创建了下列辅助数据库:\r\n---\r\n";
                strInfo += Global.MakeListString(created_dbnames, ",");
                strInfo += "\r\n\r\n";
            }
            else
            {
                strInfo += "拟创建的辅助数据库都已经存在了\r\n";
                strInfo += "\r\n\r\n";
            }

            AppendHtml(strInfo.Replace("\r\n", "<br/>"));

            return 0;
        }

        // 创建dp2系统的种次号库
        // parameters:
        //      error_databasename  类型不符合的、已经存在重名的数据库
        // return:
        //      -1  error
        //      0   suceed。不过error_databasename中可能返回因重名(并且类型不同)而未能创建的数据库名
        int CreateDp2ZhongcihaoDatabases(List<string> create_dbnames,
            out List<string> error_databasename,
            out string strError)
        {
            int nRet = 0;
            error_databasename = new List<string>();

            AppendHtml(
"====================<br/>"
+ "创建种次号库<br/>"
+ "====================<br/><br/>");


            string strDatabaseInfo = "";
            string strOutputInfo = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");


            for (int i = 0; i < create_dbnames.Count; i++)
            {
                string strDatabaseName = create_dbnames[i];
                string strDatabaseType = "zhongcihao";

                // 是否已经存在特定类型、特定名字的dp2数据库?
                // return:
                //      -1  数据库存在，但是类型不符合预期
                //      0   不存在
                //      1   存在
                nRet = ExistingDp2Database(strDatabaseName,
                    "zhongcihao",
                    out strError);
                if (nRet == -1)
                {
                    error_databasename.Add(strDatabaseName);
                    continue;
                }
                if (nRet == 1)
                    continue;

                XmlNode nodeDatabase = dom.CreateElement("database");
                dom.DocumentElement.AppendChild(nodeDatabase);

                // type
                DomUtil.SetAttr(nodeDatabase, "type", strDatabaseType);

                nRet = Global.CheckDbName(strDatabaseName,
                    out strError);
                if (nRet == -1)
                    return -1;

                DomUtil.SetAttr(nodeDatabase,
                    "name",
                    strDatabaseName);
            }

            strDatabaseInfo = dom.OuterXml;

            // 创建数据库
            nRet = this.CreateDatabase(
                strDatabaseInfo,
                out strOutputInfo,
                out strError);
            if (nRet == -1)
                return -1;

            string strInfo = "";
            if (create_dbnames.Count > 0)
            {
                strInfo += "新创建了下列种次号库:\r\n---\r\n";
                strInfo += Global.MakeListString(create_dbnames, ",");
                strInfo += "\r\n\r\n";
            }

            AppendHtml(strInfo.Replace("\r\n", "<br/>"));

            return 0;
        }

        // 列出要创建的dp2数据库。对其中已经存在的，要进行警告
        // 执行本函数前，请先执行ListAllExistingDp2Databases()
        int ListCreatingDp2Databases(out string strError)
        {
            strError = "";

            int index = 0;  // 插入的位置

            for (int i = 0; i < listView_dtlpDatabases.CheckedItems.Count; i++)
            {
                ListViewItem dtlp_item = listView_dtlpDatabases.CheckedItems[i];

                string strCreatingType = ListViewUtil.GetItemText(dtlp_item, 1);

                // 2008/11/30 new add
                string strInCirculation = ListViewUtil.GetItemText(dtlp_item, 2);
                if (strInCirculation == "是")
                {
                    if (String.IsNullOrEmpty(strCreatingType) == false)
                        strCreatingType += ",";
                    strCreatingType += "参与流通";
                }

                // 看看名字是否在dp2中已经存在
                ListViewItem dp2_item = ListViewUtil.FindItem(this.listView_creatingDp2DatabaseList,
                    dtlp_item.Text, 0);
                if (dp2_item != null)
                {
                    dp2_item.ImageIndex = 2;    // 2表示原来已经存在，又再次要求创建的

                    // 设置新的type。TODO: 原有type值怎么办？是否放在另外一列来参考?
                    ListViewUtil.ChangeItemText(dp2_item, 1, strCreatingType);

                    // 把位置提前
                    this.listView_creatingDp2DatabaseList.Items.Remove(dp2_item);
                    this.listView_creatingDp2DatabaseList.Items.Insert(index, dp2_item);
                }
                else
                {
                    dp2_item = new ListViewItem(dtlp_item.Text, 0); // 0表示不存在，而需要新创建的

                    // 设置新的type。
                    ListViewUtil.ChangeItemText(dp2_item, 1, strCreatingType);

                    this.listView_creatingDp2DatabaseList.Items.Insert(index, dp2_item);
                }

                index++;
            }

            // 设置好事项的颜色
            for (int i = 0; i < this.listView_creatingDp2DatabaseList.Items.Count; i++)
            {
                ListViewItem item = this.listView_creatingDp2DatabaseList.Items[i];

                if (item.ImageIndex == 0)
                {
                    // 需要新创建的，已经并不存在，黄色底
                    item.BackColor = Color.LightYellow;
                }
                if (item.ImageIndex == 1)
                {
                    // 以前已经存在的，和本次创建无关的，灰色文字
                    item.ForeColor = SystemColors.GrayText;
                }
                if (item.ImageIndex == 2)
                {
                    // 需要新创建的，但是已经存在的，红色底，白色字
                    item.BackColor = Color.Red;
                    item.ForeColor = Color.White;
                }
            }

            return 0;
        }

        // 是否已经存在特定类型的dp2数据库?
        bool ExistingDp2DatabaseType(string strSingleType)
        {
            XmlNodeList nodes = this.Dp2DatabaseDom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if (StringUtil.IsInList(strSingleType, strType) == true)
                    return true;
            }

            return false;
        }

        // 是否已经存在特定类型、特定名字的dp2数据库?
        // return:
        //      -1  数据库存在，但是类型不符合预期
        //      0   不存在
        //      1   存在
        int ExistingDp2Database(string strDatabaseName,
            string strSingleType,
            out string strError)
        {
            strError = "";

            XmlNodeList nodes = this.Dp2DatabaseDom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if (strName == strDatabaseName)
                {
                    if (StringUtil.IsInList(strSingleType, strType) == true)
                        return 1;

                    strError = "dp2数据库 '" + strName + "' 已经存在，但不是 '" + strSingleType + "' 类型，而是 '" + strType + "' 类型";
                    return -1;
                }
            }

            return 0;
        }

        // 获得dp2中所有已经存在的数据库
        int ListAllExistingDp2Databases(out string strError)
        {
            strError = "";

            string strOutputInfo = "";
            int nRet = GetAllDatabaseInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            this.Dp2DatabaseDom = new XmlDocument();
            try
            {
                this.Dp2DatabaseDom.LoadXml(strOutputInfo);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = Dp2DatabaseDom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                ListViewItem item = new ListViewItem(strName, 1);   // 1表示已经存在的
                item.SubItems.Add(strType);
                item.SubItems.Add("是");
                item.Tag = node.OuterXml;   // 记载XML定义片断

                this.listView_creatingDp2DatabaseList.Items.Add(item);
            }

            return 0;
        }

        // 创建数据库
        public int CreateDatabase(
            string strDatabaseInfo,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建数据库 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "create",
                    "",
                    strDatabaseInfo,
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 删除数据库
        public int DeleteDatabase(
            string strDatabaseNames,
            out string strOutputInfo,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除数据库 " + strDatabaseNames + "...");
            stop.BeginLoop();

            this.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "delete",
                    strDatabaseNames,
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        int GetAllDatabaseInfo(out string strOutputInfo,
    out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取全部dp2数据库的XML定义 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                long lRet = Channel.ManageDatabase(
                    stop,
                    "getinfo",
                    "",
                    "",
                    out strOutputInfo,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }
    }
}
