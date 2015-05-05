using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient.localhost;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 书目检索 检索式输入界面
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:StatisEntryControl runat=server></{0}:StatisEntryControl>")]
    public class StatisEntryControl : WebControl, INamingContainer
    {
        public string XmlFileName = "";

        public event CheckedChangedEventHandler CheckedChanged = null;

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }

        List<Control> GetChildControls(Control parent)
        {
            List<Control> results = new List<Control>();
            if (parent != null)
            {
                results.Add(parent);
                foreach (Control control in parent.Controls)
                {
                    results.AddRange(GetChildControls(control));
                }
            }
            else
            {
                foreach (Control control in this.Controls)
                {
                    results.AddRange(GetChildControls(control));
                } 
            }
            return results;
        }

        public void SelectItems(List<string> names)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(this.XmlFileName);
            }
            catch (Exception ex)
            {
                return;
            }

            foreach (string name in names)
            {
                string strID = GetID(dom.DocumentElement, name);
                if (string.IsNullOrEmpty(strID) == true)
                    continue;
                CheckBox check = (CheckBox)this.FindControl(strID);
                if (check != null)
                    check.Checked = true;
                if (strID.IndexOf("_") == -1)
                {
                    // 如果第一级选定了，也要选定它的下级
                    List<Control> controls = GetChildControls(null);
                    foreach (Control control in controls)
                    {
                        if (!(control is CheckBox))
                            continue;
                        CheckBox cur_check = (CheckBox)control;
                        string strCurID = cur_check.ID;
                        if (StringUtil.HasHead(strCurID, strID + "_") == true)
                            cur_check.Checked = true;
                    }
                }
            }
        }

        static string GetID(XmlNode root,
            string strName)
        {
            string[] parts = strName.Split(new char[] {'/'});
            XmlNode node = null;
            if (parts.Length == 1)
            {
                node = root.SelectSingleNode("category[@name='" + parts[0] + "']");
                if (node == null)
                    return null;

                for (int index = 0; index < node.ParentNode.ChildNodes.Count; index++)
                {
                    if (node == node.ParentNode.ChildNodes[index])
                        return index.ToString();
                }
                return null;
            }
            else if (parts.Length == 2)
            {
                node = root.SelectSingleNode("category[@name='" + parts[0] + "']/item[@name='" + parts[1] + "']");
                if (node == null)
                    return null;

                string strResult = "";
                for (int index2 = 0; index2 < node.ParentNode.ChildNodes.Count; index2++)
                {
                    if (node == node.ParentNode.ChildNodes[index2])
                    {
                        strResult = index2.ToString();
                        node = node.ParentNode;
                        goto FOUND;
                    }
                }
                return null;
            FOUND:
                for (int index = 0; index < node.ParentNode.ChildNodes.Count; index++)
                {
                    if (node == node.ParentNode.ChildNodes[index])
                        return index.ToString() + "_" + strResult;
                }
            return null;
            }
            else
                return null;
        }

        public List<string> GetSelectedItems()
        {
            List<string> results = new List<string>();

            if (string.IsNullOrEmpty(this.XmlFileName) == true)
                return results;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(this.XmlFileName);
            }
            catch (Exception ex)
            {
                return results;
            }

            this.EnsureChildControls();

            List<string> ids = new List<string>();
            List<Control> controls = GetChildControls(null);
            foreach (Control control in controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox check = (CheckBox)control;
                if (check.Checked == false)
                    continue;
                ids.Add(check.ID);
            }

            foreach (string id in ids)
            {
                string strCategoryIndex = "";
                string strItemIndex = "";
                string[] parts = id.Split(new char[] { '_' });
                if (parts.Length > 0)
                    strCategoryIndex = parts[0];
                if (parts.Length > 1)
                    strItemIndex = parts[1];

                int nCategoryIndex = -1;
                int nItemIndex = -1;
                Int32.TryParse(strCategoryIndex, out nCategoryIndex);
                if (string.IsNullOrEmpty(strItemIndex) == false)
                    Int32.TryParse(strItemIndex, out nItemIndex);

                XmlNodeList categorys = dom.DocumentElement.SelectNodes("category");
                if (categorys.Count <= nCategoryIndex)
                    continue;
                string strCategoryName = DomUtil.GetAttr(categorys[nCategoryIndex], "name");

                if (nItemIndex != -1)
                {
                    XmlNodeList items = categorys[nCategoryIndex].SelectNodes("item");
                    if (items.Count <= nItemIndex)
                        continue;
                    string strItemName = DomUtil.GetAttr(items[nItemIndex], "name");
                    results.Add(strCategoryName + "/" + strItemName);
                    continue;
                }
                results.Add(strCategoryName);
            }

            return results;
        }

        protected override void CreateChildControls()
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (string.IsNullOrEmpty(this.XmlFileName) == true)
                return;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(this.XmlFileName);
            }
            catch (Exception ex)
            {
                return;
            }

            Panel main = new Panel();
            main.CssClass = "statisentry";
            this.Controls.Add(main);

            // 全选按钮
            Panel panel = new Panel();
            panel.CssClass = "buttons";
            main.Controls.Add(panel);

            Button selectall = new Button();
            selectall.ID = "selectall";
            selectall.Text = "全选";
            selectall.CssClass = "selectall classical";
            selectall.Click += new EventHandler(selectall_Click);
            panel.Controls.Add(selectall);

            Button unselectall = new Button();
            unselectall.ID = "unselectall";
            unselectall.Text = "全清除";
            unselectall.CssClass = "unselectall classical";
            unselectall.Click += new EventHandler(unselectall_Click);
            panel.Controls.Add(unselectall);

            int i=0;
            XmlNodeList categorys = dom.DocumentElement.SelectNodes("category");
            foreach (XmlNode category in categorys)
            {
                string strCategoryName = DomUtil.GetAttr(category, "name");

                panel = new Panel();
                panel.CssClass = "category";
                main.Controls.Add(panel);

                CheckBox checkbox = new CheckBox();
                checkbox.ID = i.ToString();
                checkbox.Text = strCategoryName;
                checkbox.CssClass = "category";
                if (this.CheckedChanged != null)
                {
                    checkbox.AutoPostBack = true;
                    checkbox.CheckedChanged += new EventHandler(checkbox_CheckedChanged);
                }
                panel.Controls.Add(checkbox);

                XmlNodeList items = category.SelectNodes("item");
                int j = 0;
                foreach (XmlNode item in items)
                {
                    string strItemName = DomUtil.GetAttr(item, "name");

                    panel = new Panel();
                    panel.CssClass = "item";
                    main.Controls.Add(panel);

                    checkbox = new CheckBox();
                    checkbox.ID = i.ToString() + "_" + j.ToString();
                    checkbox.Text = strItemName;
                    checkbox.CssClass = "item";
                    if (this.CheckedChanged != null)
                    {
                        checkbox.AutoPostBack = true;
                        checkbox.CheckedChanged += new EventHandler(checkbox_CheckedChanged);
                    }
                    panel.Controls.Add(checkbox);
                    j++;
                }

                i++;
            }

        }

        void AfterCategoryChecked(string strCategoryID,
            bool bCheck)
        {
            List<Control> controls = GetChildControls(null);
            foreach (Control control in controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox check = (CheckBox)control;
                string[] parts = check.ID.Split(new char[] { '_' });
                if (parts.Length == 2 && parts[0] == strCategoryID)
                {
                    SetCheck(check, bCheck);
                }
            }
        }

        void AfterItemChecked(string strCategoryID,
            string strItemID,
            bool bCheck)
        {
            if (bCheck == false)
            {
                List<Control> controls = GetChildControls(null);
                foreach (Control control in controls)
                {
                    if (!(control is CheckBox))
                        continue;
                    CheckBox check = (CheckBox)control;
                    string[] parts = check.ID.Split(new char[] { '_' });

                    // 如果uncheck item, 则需要uncheck category
                    if (bCheck == false && parts.Length == 1 && parts[0] == strCategoryID)
                    {
                        SetCheck(check, false);
                        break;
                    }
                }
            }
        }

        static void SetCheck(CheckBox check,
            bool bChecked)
        {
            check.Checked = bChecked;
            /*
            if (bChecked == true)
                check.CssClass += " checked";
            else
                check.CssClass = check.CssClass.Replace(" checked", "");
             * */
        }

        void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            string[] parts = check.ID.Split(new char[] { '_' });
            if (parts.Length == 1)
            {
                AfterCategoryChecked(parts[0], check.Checked);
            }
            else if (parts.Length == 2)
            {
                AfterItemChecked(parts[0], parts[1], check.Checked);
            }

            if (this.CheckedChanged != null)
                this.CheckedChanged(this, e);
        }

        void selectall_Click(object sender, EventArgs e)
        {
            this.EnsureChildControls();

            List<Control> controls = GetChildControls(null);
            foreach (Control control in controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox check = (CheckBox)control;
                if (check.Checked == true)
                    continue;
                SetCheck(check, true);
            }
            if (this.CheckedChanged != null)
                this.CheckedChanged(this, e);
        }

        void unselectall_Click(object sender, EventArgs e)
        {
            this.EnsureChildControls();

            List<Control> controls = GetChildControls(null);
            foreach (Control control in controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox check = (CheckBox)control;
                if (check.Checked == false)
                    continue;
                SetCheck(check, false);
            }
            if (this.CheckedChanged != null)
                this.CheckedChanged(this, e);
        }


        protected override void Render(HtmlTextWriter writer)
        {
            List<Control> controls = GetChildControls(null);
            foreach (Control control in controls)
            {
                if (!(control is CheckBox))
                    continue;
                CheckBox check = (CheckBox)control;
                if (check.Checked == true)
                {
                    check.CssClass += " checked";
                    Panel parent = (Panel)check.Parent;
                    parent.CssClass += " checked";
                }
            }

            base.Render(writer);
        }
    }

    public delegate void CheckedChangedEventHandler(object sender,
        EventArgs e);
}
