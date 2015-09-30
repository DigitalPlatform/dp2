using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 宏处理实用类。因为同时需要使用DigitalPlatform.Text和DigitalPlatform.Xml中的功能，所以不适合放在原来的DigitalPlatform.Text项目中，而提升到了这里
    /// </summary>
    public class MacroUtil
    {
        public event ParseOneMacroEventHandler ParseOneMacro = null;

        // 在配置文件中找带有加减号的名字
        static XmlNode FindMacroItem(XmlNode root,
            string strName)
        {
            string strPureName = strName.Replace("+", "").Replace("-", "");

            XmlNodeList nodes = root.SelectNodes("item");
            if (nodes.Count == 0)
                return null;
            foreach (XmlNode node in nodes)
            {
                string strCurrentName = DomUtil.GetAttr(node, "name");
                strCurrentName = strCurrentName.Replace("+", "").Replace("-", "");
                if (strCurrentName == strPureName)
                    return node;
            }

            return null;
        }

        static bool HasIncDecChar(string strName,
            out int nDelta,
            out bool bOperFirst)
        {
            nDelta = 0;
            bOperFirst = false;
            if (strName.IndexOf("+") != -1
    || strName.IndexOf("-") != -1)
            {
                int nRet = strName.IndexOf("+");
                if (nRet == -1)
                    nRet = strName.IndexOf("-");

                Debug.Assert(nRet != -1, "");
                // 是否先操作、然后返回值。否则就是先返回值，后操作
                if (nRet == 0)
                    bOperFirst = true;

                if (strName.IndexOf("+") != -1)
                    nDelta = 1;
                else
                    nDelta = -1;

                return true;
            }
            return false;
        }

        // 从marceditor_macrotable.xml文件中解析宏
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public static int GetFromLocalMacroTable(string strFilename,
            string strName,
            bool bSimulate,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFilename);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFilename + "' 装载到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            // 全名找一次
            XmlNode node = dom.DocumentElement.SelectSingleNode("item[@name='" + strName + "']");
            if (node == null)
            {
                // (不干净名字)再按照去掉了+-符号的名字来找一次
                if (strName.IndexOf("+") != -1
    || strName.IndexOf("-") != -1)
                {
                    string strPureName = strName.Replace("+", "").Replace("-", "");
                    node = dom.DocumentElement.SelectSingleNode("item[@name='" + strPureName + "']");
                }
                else
                {
                    // 干净的名字再找一次dom中的不干净名字
                    if (node == null)
                    {
                        node = FindMacroItem(dom.DocumentElement,
                            strName);
                    }
                }

                if (node == null)
                    return 0;
            }
            string strOldValue = node.InnerText;
            string strNodeName = DomUtil.GetAttr(node, "name");

            int nDelta = 0;
            bool bOperFirst = false;    // 是否先操作、然后返回值。否则就是先返回值，后操作
            // 如果有增/减量要求
            if (HasIncDecChar(strName, out nDelta, out bOperFirst) == true
                || HasIncDecChar(strNodeName, out nDelta, out bOperFirst) == true)
            {
                if (string.IsNullOrEmpty(strOldValue) == true)
                    strOldValue = "0";

                if (bOperFirst == false)
                    strValue = strOldValue;

                string strNewValue = "";
                // 给一个被字符引导的数字增加一个数量。
                // 例如 B019X + 1 变成 B020X
                nRet = StringUtil.IncreaseNumber(strOldValue,
                    nDelta,
            out strNewValue,
            out strError);
                if (nRet == -1)
                {
                    strError = "IncreaseNumber() error :" + strError;
                    return -1;
                }

                if (bOperFirst == true)
                    strValue = strNewValue;

                if (bSimulate == false)
                {
                    node.InnerText = strNewValue;
                    dom.Save(strFilename);
                }
                return 1;
            }

            strValue = strOldValue;
            return 1;
        }


        // 解析宏
        public int Parse(
            bool bSimulate,
            string strMacro,
            out string strResult,
            out string strError)
        {
            strError = "";

            int nCurPos = 0;
            string strPart = "";

            strResult = "";

            for (; ; )
            {
                try
                {
                    strPart = NextMacro(strMacro, ref nCurPos);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
                if (strPart == "")
                    break;

                if (strPart[0] == '%')
                {
                    // 兑现宏的内容
                    ParseOneMacroEventArgs e = new ParseOneMacroEventArgs();
                    e.Macro = strPart;
                    e.Simulate = bSimulate;

                    this.ParseOneMacro(this, e);

                    if (e.Canceled == true)
                    {
                        // 不能处理的宏
                        strResult += strPart;
                        continue;
                    }

                    if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                    {
                        strError = "解析宏'" + strPart + "' 时发生错误: '" + e.ErrorInfo + "'";
                        return -1;
                        //    strResult += "(解析宏'" + strPart + "' 时发生错误: '"+e.ErrorInfo+"')" ;
                        //    continue;
                    }

                    strResult += e.Value;
                }
                else
                {
                    strResult += strPart;
                }
            }

            return 1;
        }

        // 本函数为UnMacroPath()的服务函数
        // 顺次得到下一个部分
        // nCurPos在第一次调用前其值必须设置为0，然后，调主不要改变其值
        // Exception:
        //	MacroNameException
        static string NextMacro(string strText,
            ref int nCurPos)
        {
            if (nCurPos >= strText.Length)
                return "";

            string strResult = "";
            bool bMacro = false;	// 本次是否在macro上

            if (strText[nCurPos] == '%')
                bMacro = true;

            int nRet = -1;

            if (bMacro == false)
                nRet = strText.IndexOf("%", nCurPos);
            else
                nRet = strText.IndexOf("%", nCurPos + 1);

            if (nRet == -1)
            {
                strResult = strText.Substring(nCurPos);
                nCurPos = strText.Length;
                if (bMacro == true)
                {
                    // 这是异常情况，表明%只有头部一个
                    throw (new Exception("macro " + strResult + " format error"));
                }
                return strResult;
            }

            if (bMacro == true)
            {
                strResult = strText.Substring(nCurPos, nRet - nCurPos + 1);
                nCurPos = nRet + 1;
                return strResult;
            }
            else
            {
                Debug.Assert(strText[nRet] == '%', "当前位置不是%，异常");
                strResult = strText.Substring(nCurPos, nRet - nCurPos);
                nCurPos = nRet;
                return strResult;
            }
        }

        // return:
        //      -1  出错。错误信息在 strError 中
        //      0   不能处理的宏
        //      1   成功处理，返回结果在 strValue 中
        public delegate int delegate_parseOneMacro(bool bSimulate,
            string strText,
            out string strValue,
            out string strError);

        // 解析宏
        public static int Parse(
            bool bSimulate,
            string strMacro,
            delegate_parseOneMacro procParseMacro,
            out string strResult,
            out string strError)
        {
            strError = "";

            int nCurPos = 0;
            string strPart = "";

            strResult = "";

            for (; ; )
            {
                try
                {
                    strPart = NextMacro(strMacro, ref nCurPos);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
                if (strPart == "")
                    break;

                if (strPart[0] == '%')
                {
                    string strValue = "";
                    // return:
                    //      -1  出错。错误信息在 strError 中
                    //      0   不能处理的宏
                    //      1   成功处理，返回结果在 strValue 中
                    int nRet = procParseMacro(bSimulate, strPart, out strValue, out strError);
                    if (nRet == -1)
                    {
                        strError = "解析宏'" + strPart + "' 时发生错误: '" + strError + "'";
                        return -1;
                    }

                    if (nRet == 0)
                    {
                        // 不能处理的宏
                        strResult += strPart;
                        continue;
                    }

                    strResult += strValue;
                }
                else
                {
                    strResult += strPart;
                }
            }

            return 1;
        }

        public static string Unquote(string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            if (strValue[0] == '%')
                strValue = strValue.Substring(1);
            if (strValue.Length == 0)
                return "";
            if (strValue[strValue.Length - 1] == '%')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
    }


    // 获得一个宏的实际值
    public delegate void ParseOneMacroEventHandler(object sender,
        ParseOneMacroEventArgs e);

    public class ParseOneMacroEventArgs : EventArgs
    {
        public string Macro = "";   // 宏
        public bool Simulate = false;   // 是否为模拟方式? 在模拟方式下, 种子号增量将变为获得种子号来执行,也就是不会改变种子值
        public string Value = "";   // [out]兑现后的值
        public string ErrorInfo = "";   // [out]出错信息。如果为空，表示没有出错
        public bool Canceled = false;   // [out]是否属于不能处理的宏
    }

}
