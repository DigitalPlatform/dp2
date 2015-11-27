using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;

namespace DigitalPlatform.rms
{
    // KeysCfg 的摘要说明。
    public class KeysBrowseBase
    {
        public XmlDocument dom = null;

        // <xpath>元素 和 XmlNamespaceManager对象的对照表 
        internal Hashtable tableNsClient = new Hashtable();

        // <nstable>元素的xpath路径 和 XmlNamespaceManager对象的对照表
        internal Hashtable tableNsServer = new Hashtable();

        public string BinDir = "";

        // 2015/8/24
        // 配置文件全路径
        internal string CfgFileName
        {
            get;
            set;
        }


        // 初始化KeysBrowseBase对象，把dom准备好，把两个Hashtable准备好
        public virtual int Initial(string strCfgFileName,
            string strBinDir,
            out string strError)
        {
            strError = "";

            this.BinDir = strBinDir;

            // 清空
            this.Clear();

            if (File.Exists(strCfgFileName) == false)
            {
                strError = "配置文件'" + strCfgFileName + "'在本地不存在";
                return -1;
            }

#if NO
            string strText = "";
            // 如果keys文件的内容为空，则不创建检索，正常结束
            StreamReader sw = new StreamReader(strCfgFileName, Encoding.UTF8);
            try
            {
                strText = sw.ReadToEnd();
            }
            finally
            {
                sw.Close();
            }

            if (strText == "")
                return 0;
#endif
            // 2012/2/17
            FileInfo fi = new FileInfo(strCfgFileName);
            if (fi.Length == 0)
                return 0;

            this.CfgFileName = strCfgFileName;

            dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFileName);
            }
            catch (Exception ex)
            {
                strError = "加载配置文件 '" + strCfgFileName + "' 到 XMLDOM 时出错：" + ex.Message;
                return -1;
            }

            // 创建NsTable缓存,被Initial调
            // return:
            //		-1	出错
            //		0	成功
            int nRet = this.CreateNsTableCache(out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 创建NsTable缓存,被Initial调
        // return:
        //		-1	出错
        //		0	成功
        private int CreateNsTableCache(out string strError)
        {
            strError = "";
            int nRet = 0;

            // 没有配置文件时
            if (this.dom == null)
                return 0;

            // 找到所有的<xpath>元素
            XmlNodeList xpathNodeList = dom.DocumentElement.SelectNodes("//xpath[@nstable]");
            for (int i = 0; i < xpathNodeList.Count; i++)
            {
                XmlNode nodeXpath = xpathNodeList[i];
                XmlNode nodeNstable = null;
                // return:
                //		-1	出错
                //		0	没找到	strError里面有出错信息
                //		1	找到
                //		2	根本不使用nstable
                nRet = FindNsTable(nodeXpath,
                    out nodeNstable,
                    out strError);
                if (nRet == 2)
                    Debug.Assert(false, "不可能找不到了");
                if (nRet != 1)
                    return -1;

                // 可能确实不用nstable
                if (nodeNstable != null)
                {
                    // 取出nodeNstable的路径
                    string strPath = "";
                    nRet = DomUtil.Node2Path(dom.DocumentElement,
                        nodeNstable,
                        out strPath,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    XmlNamespaceManager nsmgr = (XmlNamespaceManager)this.tableNsServer[strPath];
                    if (nsmgr == null)
                    {
                        nRet = GetNsManager(nodeNstable,
                            out nsmgr,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        this.tableNsServer[strPath] = nsmgr;
                    }

                    // 加到客户端表
                    this.tableNsClient[nodeXpath] = nsmgr;
                }
            }
            return 0;
        }

        // 根据<nstable>定义的内容得到XmlNamespaceManager对象
        // return
        //      -1  出错
        //      0   成功
        private int GetNsManager(XmlNode nodeNstable,
            out XmlNamespaceManager nsmgr,
            out string strError)
        {
            strError = "";
            nsmgr = new XmlNamespaceManager(nodeNstable.OwnerDocument.NameTable);

            XmlNodeList nodeListItem = nodeNstable.SelectNodes("item");
            for (int i = 0; i < nodeListItem.Count; i++)
            {
                XmlNode nodeItem = nodeListItem[i];

                if (nodeItem.ChildNodes.Count > 0)
                {
                    strError = "配置文件是旧版本。<item>元素不支持下级元素。";
                    return -1;
                }

                string strPrefix = DomUtil.GetAttr(nodeItem, "prefix");
                string strUrl = DomUtil.GetAttr(nodeItem, "url");

                //???如果前缀为空是什么情况，url为空是什么情况。
                if (strPrefix == "" && strUrl == "")
                    continue;

                nsmgr.AddNamespace(strPrefix, strUrl);
            }

            return 0;
        }

        // 查找<xpath>对应的<nstable>
        // return:
        //		-1	出错
        //		0	没找到	strError里面有出错信息
        //		1	找到
        //		2	不使用
        private static int FindNsTable(XmlNode nodeXpath,
            out XmlNode nodeNstable,
            out string strError)
        {
            nodeNstable = null;
            strError = "";

            string strNstableName = DomUtil.GetAttrDiff(nodeXpath, "nstable");
            if (strNstableName == null)
                return 2;

            string strXPath = "";
            // 向内找
            if (strNstableName == "")
                strXPath = ".//nstable";
            else
                strXPath = ".//nstable[@name='" + strNstableName + "']";

            nodeNstable = nodeXpath.SelectSingleNode(strXPath);
            if (nodeNstable != null)
            {
                return 1;
            }

            // 向外找
            if (strNstableName == "")
                strXPath = "//nstable[@name=''] | //nstable[not(@name)]";  //???找属性值为空，或未定义该属性
            else
                strXPath = "//nstable[@name='" + strNstableName + "']";

            nodeNstable = nodeXpath.SelectSingleNode(strXPath);
            if (nodeNstable != null)
                return 1;

            strError = "没找到名字叫'" + strNstableName + "'的<nstable>节点。";
            return 0;
        }

        // 清空对象
        public virtual void Clear()
        {
            this.tableNsClient.Clear();
            this.tableNsServer.Clear();
        }
    }

}
