using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 报表配置参数
    /// 配置了每个分馆的报表概况
    /// </summary>
    public class ReportConfigBuilder
    {
        public string CfgFileName = ""; // XML 配置文件全路径

        XmlDocument CfgDom = null;      // XML 配置文件的 DOM

        public bool Changed
        {
            get;
            set;
        }

        public int LoadCfgFile(string strBaseDir,
            string strFileName,
            out string strError)
        {
            strError = "";

            this.CfgFileName = Path.Combine(strBaseDir, strFileName);  // "report_def.xml"
            this.CfgDom = new XmlDocument();
            try
            {
                this.CfgDom.Load(this.CfgFileName);
            }
            catch (FileNotFoundException)
            {
                this.CfgDom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                this.CfgDom = null;
                strError = "报表配置文件 "+this.CfgFileName+" 打开错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        public void Save()
        {
            if (this.CfgDom != null && string.IsNullOrEmpty(this.CfgFileName) == false)
            {
                this.CfgDom.Save(this.CfgFileName);
            }

            this.Changed = false;
        }

        public void FillList(ListView list)
        {
            list.Items.Clear();

            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return;

            XmlNodeList nodes = this.CfgDom.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");

                strCode = ReportForm.GetDisplayLibraryCode(strCode);

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, strCode);

                list.Items.Add(item);
            }

            if (list.Items.Count > 0)
                list.Items[0].Selected = true;
        }

        // 获得 102 表的部门列表
        public List<string> Get_102_Departments(string strLibraryCode)
        {
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return new List<string>();

            XmlNode node = this.CfgDom.DocumentElement.SelectSingleNode("library[@code='" + strLibraryCode + "']");
            if (node == null)
                return new List<string>();

            return StringUtil.SplitList(DomUtil.GetAttr(node, "table_102_departments"));
        }

        public XmlNode GetLibraryNode(string strLibraryCode)
        {
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return null;

            return this.CfgDom.DocumentElement.SelectSingleNode("library[@code='" + strLibraryCode + "']");
        }

        // 创建一个新的 <library> 元素。要对 code 属性进行查重
        // parameters:
        //      -1  出错
        //      0   成功
        //      1   已经有这个 code 属性的元素了
        public int CreateNewLibraryNode(string strLibraryCode,
            out XmlNode node,
            out string strError)
        {
            strError = "";
            node = null;

            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
            {
                strError = "ReportConfig 对象尚未初始化";
                return -1;
            }

            node = this.CfgDom.DocumentElement.SelectSingleNode("library[@code='"+strLibraryCode+"']");
            if (node != null)
            {
                strError = "已经存在馆代码 '"+strLibraryCode+"' 的配置事项了，不能重复创建";
                return 1;
            }

            node = this.CfgDom.CreateElement("library");
            this.CfgDom.DocumentElement.AppendChild(node);
            DomUtil.SetAttr(node, "code", strLibraryCode);

            this.Changed = true;
            return 0;
        }
    }
}
