using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;
using System.Linq;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和中心服务器相关的代码
    /// </summary>
    public partial class LibraryApplication
    {


        // 修改 <center> 内的定义
        // parameters:
        //      strAction   "create" "modify" "delete"
        //      strXml      传入的 XML 内容。注意可能在调用过程中被修改，其中 password 属性值从 plain 变为 hashed 状态
        //      strOldXml   返回被修改前的 server 元素 OuterXml。顶层包含元素 center 作为容器元素。
        //      strSnapshoet    返回被修改前的 center 元素 OuterXml。此内容便于日志恢复阶段进行快照恢复
        // return:
        //      -1  error
        //      0   not change
        //      1   changed
        public int SetCenterDef(string strAction,
            ref string strXml,
            out string strOldXml,
            out string strSnapshot,
            out string strError)
        {
            strError = "";
            strOldXml = "";
            strSnapshot = "";

            // 记载修改前的 center 元素 OuterXml
            {
                var center_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("center");
                strSnapshot = GetXmls(center_nodes.Cast<XmlNode>());
            }

            bool bChanged = false;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM时出错：" + ex.Message;
                return -1;
            }

            // 把 strXml 中的 password 属性值 hashed
            {
                var password_attrs = dom.DocumentElement.SelectNodes("//center/server/@password");
                foreach (XmlAttribute attr in password_attrs)
                {
                    attr.Value = Cryptography.Encrypt(attr.Value, EncryptKey);
                }
                strXml = dom.DocumentElement.OuterXml;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//server");
            foreach (XmlElement node in nodes)
            {
                string strRefID = DomUtil.GetAttr(node, "refid");

                if (strAction == "delete")
                {
                    if (string.IsNullOrEmpty(strRefID) == true)
                    {
                        strError = "无法删除，因为请求中没有使用 refid 属性 : " + node.OuterXml;
                        return -1;
                    }
                    XmlNodeList target_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("center/server[@refid='" + strRefID + "']");
                    if (target_nodes.Count == 0)
                    {
                        strError = "参考 ID 为 '" + strRefID + "' 的 server 元素在 center 元素下没有找到";
                        return -1;
                    }

                    // 2025/4/4
                    // 记忆下 library.xml 相关 server 节点的原始 XML
                    // 注意 center 是返回内容中必要的容器元素名。不意味着 library.xml 中 center 元素下只有这些内容
                    strOldXml += GetXmls(target_nodes.Cast<XmlNode>(), "center");

                    foreach (XmlNode target in target_nodes)
                    {
                        target.ParentNode.RemoveChild(target);
                        bChanged = true;
                    }
                    continue;
                }

                if (strAction == "modify")
                {
                    if (string.IsNullOrEmpty(strRefID) == true)
                    {
                        strError = "无法修改，因为请求中没有使用 refid 属性 : " + node.OuterXml;
                        return -1;
                    }
                    var target = this.LibraryCfgDom.DocumentElement.SelectSingleNode("center/server[@refid='" + strRefID + "']") as XmlElement;
                    if (target == null)
                    {
                        strError = "参考 ID 为 '" + strRefID + "' 的 server 元素在 center 元素下没有找到";
                        return -1;
                    }

                    // 2025/4/4
                    // 记忆下 library.xml 相关 server 节点的原始 XML
                    // 注意 center 是返回内容中必要的容器元素名。不意味着 library.xml 中 center 元素下只有这些内容
                    strOldXml += GetXmls(new List<XmlNode>() { target }, "center");

                    string strName = DomUtil.GetAttr(node, "name");

                    //XmlElement t = target as XmlElement;
                    //XmlElement n = node as XmlElement;
                    if (node.HasAttribute("name") == true
                        && DomUtil.GetAttr(target, "name") != strName)
                    {
                        // name 查重
                        XmlNodeList target_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("center/server[@name='" + strName + "']");
                        if (target_nodes.Count > 0)
                        {
                            strError = "修改失败。名字 为 '" + strName + "' 的 server 元素已经存在，无法把别的 server 元素的 name 属性修改为此值";
                            return -1;
                        }

                        DomUtil.SetAttr(target, "name", strName);
                        bChanged = true;
                    }

                    if (node.HasAttribute("url") == true)
                    {
                        DomUtil.SetAttr(target, "url", DomUtil.GetAttr(node, "url"));
                        bChanged = true;
                    }

                    if (node.HasAttribute("username") == true)
                    {
                        DomUtil.SetAttr(target, "username", DomUtil.GetAttr(node, "username"));
                        bChanged = true;
                    }

                    if (node.HasAttribute("password") == true)
                    {
                        //string strText = Cryptography.Encrypt(DomUtil.GetAttr(node, "password"), EncryptKey);
                        //DomUtil.SetAttr(target, "password", strText);
                        target.SetAttribute("password", node.GetAttribute("password"));
                        bChanged = true;
                    }

                    continue;
                }

                if (strAction == "create")
                {
                    if (string.IsNullOrEmpty(strRefID) == true)
                    {
                        strError = "无法创建，因为请求中没有使用 refid 属性 : " + node.OuterXml;
                        return -1;
                    }

                    // 查重
                    XmlNodeList target_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("center/server[@refid='" + strRefID + "']");
                    if (target_nodes.Count > 0)
                    {
                        strError = "创建失败。参考 ID 为 '" + strRefID + "' 的 server 元素已经存在";
                        return -1;
                    }

                    string strName = DomUtil.GetAttr(node, "name");
                    if (string.IsNullOrEmpty(strName) == true)
                    {
                        strError = "创建失败。name 属性不能为空";
                        return -1;
                    }

                    string strUrl = DomUtil.GetAttr(node, "url");
                    if (string.IsNullOrEmpty(strUrl) == true)
                    {
                        strError = "创建失败。url 属性不能为空";
                        return -1;
                    }

                    string strUserName = DomUtil.GetAttr(node, "username");
                    if (string.IsNullOrEmpty(strUserName) == true)
                    {
                        strError = "创建失败。username 属性不能为空";
                        return -1;
                    }

                    // name 查重
                    target_nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("center/server[@name='" + strName + "']");
                    if (target_nodes.Count > 0)
                    {
                        strError = "创建失败。名字 为 '" + strName + "' 的 server 元素已经存在";
                        return -1;
                    }

                    XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("center");
                    if (root == null)
                    {
                        root = this.LibraryCfgDom.CreateElement("center");
                        this.LibraryCfgDom.DocumentElement.AppendChild(root);
                    }
                    var target = this.LibraryCfgDom.CreateElement("server");
                    root.AppendChild(target);

                    DomUtil.SetAttr(target, "name", strName);
                    DomUtil.SetAttr(target, "url", strUrl);

                    DomUtil.SetAttr(target, "username", strUserName);
                    // string strPassword = DomUtil.GetAttr(node, "password");
                    // DomUtil.SetAttr(target, "password", Cryptography.Encrypt(strPassword, EncryptKey));
                    target.SetAttribute("password", node.GetAttribute("password"));
                    DomUtil.SetAttr(target, "refid", strRefID);

                    bChanged = true;
                    continue;
                }
            }

            if (bChanged == true)
            {
#if NO
                // <itemdbgroup>内容更新，刷新配套的内存结构
                int nRet = this.LoadItemDbGroupParam(this.LibraryCfgDom,
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog(strError);
                    return -1;
                }
#endif
                this.Changed = true;
                return 1;
            }
            return 0;
        }

        // 将远程书目库名替换为本地书目库名
        // return:
        //      -1  出错
        //      0   没有找到对应的本地书目库
        //      1   找到，并已经替换
        public int ReplaceBiblioRecPath(
    string strServer,
    ref string strRecPath,
    out string strError)
        {
            strError = "";

            string strDbName = ResPath.GetDbName(strRecPath);
            string strID = ResPath.GetRecordId(strRecPath);

            foreach (ItemDbCfg cfg in this.ItemDbs)
            {
                if (cfg.ReplicationServer != strServer)
                    continue;
                if (strDbName == cfg.ReplicationDbName)
                {
                    strRecPath = cfg.BiblioDbName + "/" + strID;
                    return 1;
                }
            }

            strRecPath = "";
            return 0;
        }

        // 根据中心服务器名，获得相关的书目库配置
        public List<ItemDbCfg> FindReplicationItems(string strServer)
        {
            List<ItemDbCfg> results = new List<ItemDbCfg>();
            foreach (ItemDbCfg cfg in this.ItemDbs)
            {
                if (cfg.ReplicationServer != strServer)
                    continue;
                results.Add(cfg);
            }

            return results;
        }
    }
}
