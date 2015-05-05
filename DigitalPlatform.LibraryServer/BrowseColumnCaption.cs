using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;

using System.Resources;
using System.Globalization;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;
// using DigitalPlatform.Drawing;  // ShrinkPic()

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是流通业务相关的代码
    /// </summary>
    public partial class LibraryApplication
    {

        // OPAC所用的浏览列定义缓存
        // 库名 --> List<BrowseColumnCaption>
        Hashtable BrowsColumnTable = new Hashtable();

        // 获得一个库的浏览列标题
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetBrowseColumnCaptions(
            SessionInfo sessioninfo,
            string strDbName,
            string strLang,
            out List<string> captions,
            out string strError)
        {
            strError = "";
            captions = null;

            REDO:
            List<BrowseColumnCaption> results = (List<BrowseColumnCaption>)this.BrowsColumnTable[strDbName];
            if (results != null && results.Count > 0)
            {
                if (results.Count == 1)
                {
                    captions = results[0].ColumnCaptions;
                    return 1;
                }

                string strLangLeft = "";
                string strLangRight = "";

                DomUtil.SplitLang(strLang,
                   out strLangLeft,
                   out strLangRight);

                for (int i = 0; i < results.Count; i++)
                {
                    BrowseColumnCaption cur_captions = results[i];

                    string strThisLang = cur_captions.Lang;

                    string strThisLangLeft = "";
                    string strThisLangRight = "";

                    DomUtil.SplitLang(strThisLang,
                       out strThisLangLeft,
                       out strThisLangRight);

                    // 是不是左右都匹配则更好?如果不行才是第一个左边匹配的
                    if (strThisLangLeft == strLangLeft)
                    {
                        captions = cur_captions.ColumnCaptions;
                        return 1;
                    }
                }

                captions = results[0].ColumnCaptions;
                return 1;
            }

            string strRemotePath = strDbName + "/cfgs/browse";
            string strLocalPath = "";

            int nRet = this.CfgsMap.MapFileToLocal(
                sessioninfo.Channels,
                strRemotePath,
                out strLocalPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获得配置文件 '" + strRemotePath + "' 时发生错误：" + strError;
                return -1;
            }


            if (nRet == 0)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strLocalPath);
            }
            catch (Exception ex)
            {
                strError = "数据库 " + strDbName + " 的browse配置文件 '" + strLocalPath + "'  装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            List<BrowseColumnCaption> defs = new List<BrowseColumnCaption>();

            XmlNodeList lang_nodes = dom.DocumentElement.SelectNodes("//col/title/caption/@lang");
            if (lang_nodes.Count > 0)
            {
                List<string> langs = new List<string>();
                foreach (XmlNode node in lang_nodes)
                {
                    string strValue = node.Value;

                    if (langs.IndexOf(strValue) == -1)
                        langs.Add(strValue);
                }

                XmlNodeList col_nodes = dom.DocumentElement.SelectNodes("//col");
                foreach (string lang in langs)
                {
                    BrowseColumnCaption one = new BrowseColumnCaption();
                    one.Lang = lang;
                    one.ColumnCaptions = new List<string>();
                    defs.Add(one);

                    foreach (XmlNode node in col_nodes)
                    {
                        XmlNode nodeTitle = node.SelectSingleNode("title");
                        if (nodeTitle != null)
                        {
                            string strCaption = DomUtil.GetLangedNodeText(
              lang,
              nodeTitle,
              "caption",
              true);
                            one.ColumnCaptions.Add(strCaption);
                        }
                        else
                        {
                            // 被迫使用<col>元素的title属性
                            one.ColumnCaptions.Add(DomUtil.GetAttr(node, "title"));
                        }

                    }
                }

                this.BrowsColumnTable[strDbName] = defs;
                goto REDO;
            }

            {
                BrowseColumnCaption one = new BrowseColumnCaption();
                one.Lang = "zh";    // 缺省为zh
                one.ColumnCaptions = new List<string>();
                defs.Add(one);


                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                for (int j = 0; j < nodes.Count; j++)
                {
                    string strColumnTitle = DomUtil.GetAttr(nodes[j], "title");

                    one.ColumnCaptions.Add(strColumnTitle);
                }

                this.BrowsColumnTable[strDbName] = defs;
                goto REDO;
            }
        }

    }

    public class BrowseColumnCaption
    {
        public string Lang = "";
        public List<string> ColumnCaptions = null;  // null表示尚未初始化
    }
}
