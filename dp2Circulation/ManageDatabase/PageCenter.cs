using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 中心服务器 属性页
    /// </summary>
    public partial class ManagerForm
    {
        int SetCenterInfo(
            string strAction,
            string strCenterDef,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在设置中心服务器定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "center",
                    strAction,
                    strCenterDef,
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

        internal List<string> GetCenterServerNames()
        {
            List<string> results = new List<string>();
            foreach (ListViewItem item in this.listView_center.Items)
            {
                results.Add(item.Text);
            }

            return results;
        }

        internal int GetRemoteBiblioDbNames(
    string strRemoveServer,
    out List<string> dbnames,
    out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            // 根据中心服务器名，找到 URL 用户名 密码
            ListViewItem item = ListViewUtil.FindItem(this.listView_center, strRemoveServer, 0);
            if (item == null)
            {
                strError = "中心服务器 '" + strRemoveServer + "' 尚未定义";
                return -1;
            }

            string strUrl = ListViewUtil.GetItemText(item, 1);
            string strUserName = ListViewUtil.GetItemText(item, 2);
            string strPassword = (string)item.Tag;

            return GetRemoteBiblioDbNames(
                strUrl,
                strUserName,
                strPassword,
                out dbnames,
                out strError);
        }

        internal static int GetRemoteBiblioDbNames(
            string strUrl,
            string strUserName,
            string strPassword,
            out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            string strValue = "";

            LibraryChannel channel = new LibraryChannel();
            channel.Url = strUrl;

            try
            {
                long lRet = channel.Login(strUserName,
    strPassword,
    "type=worker",
    out strError);
                if (lRet != 1)
                {
                    strError = "对服务器 '" + channel.Url + "' 以用户 '" + strUserName + "' 进行登录时发生错误: " + strError;
                    return -1;
                }

                lRet = channel.GetSystemParameter(null,
                    "system",
                    "biblioDbGroup",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得书目库信息过程发生错误：" + strError;
                    return -1;
                }


            }
            finally
            {
                channel.Close();
            }

            {
                // 解析 XML
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");

                try
                {
                    dom.DocumentElement.InnerXml = strValue;
                }
                catch (Exception ex)
                {
                    strError = "category=system,name=biblioDbGroup 所返回的 XML 片段在装入 InnerXml 时出错: " + ex.Message;
                    return -1;
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");

                foreach (XmlNode node in nodes)
                {
                    string strDbName = DomUtil.GetAttr(node, "biblioDbName");
                    dbnames.Add(strDbName);
                }
            }

            return 0;
        }

        int GetCenterInfo(out string strOutputInfo,
    out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取中心服务器信息 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "center",
                    "def",
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

        string CenterInfoXml = "";

        // 在 listview 中列出所有中心服务器
        int ListCenter(out string strError)
        {
            strError = "";

            this.listView_center.Items.Clear();

            string strOutputInfo = "";
            int nRet = GetCenterInfo(out strOutputInfo,
                    out strError);
            if (nRet == -1)
                return -1;

            this.CenterInfoXml = strOutputInfo;

            if (string.IsNullOrEmpty(this.CenterInfoXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strOutputInfo);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("server");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strUrl = DomUtil.GetAttr(node, "url");
                string strUserName = DomUtil.GetAttr(node, "username");
                string strRefID = DomUtil.GetAttr(node, "refid");
                string strPassword = DomUtil.GetAttr(node, "password");

                ListViewItem item = new ListViewItem(strName, 0);
                ListViewUtil.ChangeItemText(item, 1, strUrl);
                ListViewUtil.ChangeItemText(item, 2, strUserName);
                ListViewUtil.ChangeItemText(item, 3, strRefID);
                // item.Tag = node.OuterXml;   // 记载XML定义片断
                item.Tag = strPassword;
                this.listView_center.Items.Add(item);
            }

            listView_center_SelectedIndexChanged(this, null);
            return 0;
        }


        private void toolStripButton_center_modify_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_center.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的服务器事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_center.SelectedItems[0];

            string strRefID = ListViewUtil.GetItemText(item, 3);
            if (string.IsNullOrEmpty(strRefID) == true)
            {
                strError = "所选定的事项缺乏参考 ID 值，无法请求服务器端进行修改";
                goto ERROR1;
            }

            CenterServerDialog dlg = new CenterServerDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "修改中心服务器";
            dlg.CreateMode = false;
            dlg.ServerName = ListViewUtil.GetItemText(item, 0);
            dlg.ServerUrl = ListViewUtil.GetItemText(item, 1);
            dlg.UserName = ListViewUtil.GetItemText(item, 2);
            dlg.RefID = ListViewUtil.GetItemText(item, 3);
            dlg.Password = (string)item.Tag;

            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 修改
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<server />");
            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.ServerName);
            DomUtil.SetAttr(dom.DocumentElement, "url", dlg.ServerUrl);
            DomUtil.SetAttr(dom.DocumentElement, "username", dlg.UserName);
            if (dlg.ChangePassword == true)
                DomUtil.SetAttr(dom.DocumentElement, "password", dlg.Password);
            DomUtil.SetAttr(dom.DocumentElement, "refid", dlg.RefID);

            int nRet = SetCenterInfo(
                "modify",
                dom.DocumentElement.OuterXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新显示
            nRet = ListCenter(out strError);
            if (nRet == -1)
                goto ERROR1;

            // 选中刚刚修改的行
            item = ListViewUtil.FindItem(this.listView_center, dlg.RefID, 3);
            if (item != null)
                item.Selected = true;


            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_center_add_Click(object sender, EventArgs e)
        {
            string strError = "";

            int index = -1;
            if (this.listView_center.SelectedIndices.Count > 0)
                index = this.listView_center.SelectedIndices[0];

            string strRefID = Guid.NewGuid().ToString();

            CenterServerDialog dlg = new CenterServerDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "添加中心服务器";
            dlg.CreateMode = true;
            dlg.RefID = strRefID;

            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 创建
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<server />");
            DomUtil.SetAttr(dom.DocumentElement, "name", dlg.ServerName);
            DomUtil.SetAttr(dom.DocumentElement, "url", dlg.ServerUrl);
            DomUtil.SetAttr(dom.DocumentElement, "username", dlg.UserName);
            DomUtil.SetAttr(dom.DocumentElement, "password", dlg.Password);
            DomUtil.SetAttr(dom.DocumentElement, "refid", dlg.RefID);

            int nRet = SetCenterInfo(
                "create",
                dom.DocumentElement.OuterXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新显示
            nRet = ListCenter(out strError);
            if (nRet == -1)
                goto ERROR1;

            // 选中新创建的行
            ListViewItem item = ListViewUtil.FindItem(this.listView_center, dlg.RefID, 3);
            if (item != null)
                item.Selected = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_center_delete_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_center.SelectedItems.Count == 0)
            {
                strError = "尚未选定要移除的服务器事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要移除选定的 " + this.listView_center.SelectedItems.Count.ToString() + " 个服务器事项?",
"dp2Circulation",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            foreach (ListViewItem item in this.listView_center.SelectedItems)
            {
                string strName = ListViewUtil.GetItemText(item, 0);
                string strRefID = ListViewUtil.GetItemText(item, 3);
                if (string.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "事项 " + strName + " 缺乏参考 ID 值，无法请求服务器端进行移除";
                    goto ERROR1;
                }

                XmlNode new_node = dom.CreateElement("server");
                dom.DocumentElement.AppendChild(new_node);

                DomUtil.SetAttr(new_node, "refid", strRefID);
            }

            int nRet = SetCenterInfo(
                "delete",
                dom.DocumentElement.OuterXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新显示
            nRet = ListCenter(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_center_refresh_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 刷新显示
            int nRet = ListCenter(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
