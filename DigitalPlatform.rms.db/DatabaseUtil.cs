using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms
{
    public class DatabaseUtil
    {
        // 得到newdata字段对应的文件名
        public static string GetNewFileName(string strFilePath)
        {
            return strFilePath + ".new";
        }

        // 得到range字段对应的文件名
        public static string GetRangeFileName(string strFilePath)
        {
            return strFilePath + ".range";
        }

        // 得到range字段对应的文件名
        public static string GetTimestampFileName(string strFilePath)
        {
            return strFilePath + ".timestamp";
        }

        // 得到metadata字段对应的文件名
        public static string GetMetadataFileName(string strFilePath)
        {
            return strFilePath + ".metadata";
        }


        public static string GetLocalDir(XmlNode rootNode,
            XmlNode startNode)
        {
            if (startNode == null)
                return "";

            string strDir = "";
            XmlNode node = startNode;
            while (true)
            {
                if (node == null)
                    break;

                // 如果startNode比root还高级????????????


                string strOneDir = DomUtil.GetAttr(node, "localdir");
                strOneDir = strOneDir.Trim();
                if (strOneDir != "")
                {
                    if (strDir != "")
                        strDir = "\\" + strDir;

                    strDir = strOneDir + strDir;
                }

                if (node == rootNode)
                {
                    break;
                }

                node = node.ParentNode;

            }

            return strDir;
        }


        // 为配置文件创建时间戳
        public static byte[] CreateTimestampForCfg(string strFilePath)
        {
            byte[] baTimestamp = null;
            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return baTimestamp;

            long lTicks = fileInfo.LastWriteTimeUtc.Ticks;
            byte[] baTime = BitConverter.GetBytes(lTicks);

            byte[] baLength = BitConverter.GetBytes((long)fileInfo.Length);
            //Array.Reverse(baLength);

            baTimestamp = new byte[baTime.Length + baLength.Length];
            Array.Copy(baTime,
                0,
                baTimestamp,
                0,
                baTime.Length);
            Array.Copy(baLength,
                0,
                baTimestamp,
                baTime.Length,
                baLength.Length);

            // return ByteArray.GetHexTimeStampString(baTimestamp);
            return baTimestamp;
        }

        // 将检索目标"数据库名1:标题,姓名"格式，
        // 分成单独的数据名和table列表
        // parameter:
        //		strdbComplete	传入的完整格式
        //		strDbName	out参数，返回数据库名
        //		strTableList	out参数，返回table列表
        // return:
        //		-1	出错
        //		0	成功
        public static int SplitToDbNameAndForm(string strTarget,
            out string strDbName,
            out string strTableList,
            out string strError)
        {
            strDbName = "";
            strTableList = "";
            strError = "";

            if (strTarget == null
                || strTarget == "")
            {
                strError = "SplitToDbNameAndForm() strTarget参数不能为null或空字符串";
                return -1;
            }

            int nIndex = strTarget.IndexOf(":");
            if (nIndex != -1)
            {
                strDbName = strTarget.Substring(0, nIndex);
                strTableList = strTarget.Substring(nIndex + 1);
            }
            else  //没有':'时，全按数据库名称算
            {
                strDbName = strTarget;
            }

            return 0;
        }

        // 得到10位的记录ID
        // return:
        //      -1  出错
        //      0   成功
        public static int CheckAndGet10RecordID(ref string strRecordID,
            out string strError)
        {
            strError = "";

            if (strRecordID == null)
            {
                strError = "记录ID不能为null";
                return -1;
            }

            if (strRecordID == "")
            {
                strError = "记录ID不能为空字符串";
                return -1;
            }

            // 大于10不合法
            if (strRecordID.Length > 10)
            {
                strError = "记录ID '" + strRecordID + "' 不合法，不能大于10位";
                return -1;
            }

            //把'?'换成'-1'  因为原来系统都认得'-1'
            if (strRecordID == "?")
                strRecordID = "-1";

#if NO
            if (strRecordID == "?")
            {
                strError = "记录 ID 不能为 '?'。必须是一个明确的数字";
                return -1;
            }
#endif

            // 不能转换成数据不合法
            try
            {
                long nId = Convert.ToInt64(strRecordID);
                // 除-1外，负数不合法
                if (nId < -1)
                {
                    strError = "记录ID '" + strRecordID + "' 不合法";
                    return -1;
                }
            }
            catch
            {
                strError = "记录ID '" + strRecordID + "' 不合法";
                return -1;
            }

            if (strRecordID != "-1")    // 2015/11/16
                strRecordID = DbPath.GetID10(strRecordID);

            return 0;
        }

        public static int AddInteger(string strNewValue,
            string strOldValue,
            out string strLastValue,
            out string strError)
        {
            strLastValue = "";
            strError = "";

            long lNewValue = 0;
            if (strNewValue != "")
            {
                try
                {
                    lNewValue = Convert.ToInt64(strNewValue);
                }
                catch (Exception ex)
                {
                    strError = "当action为AddInteger时，输入的值'" + strNewValue + "'不合法，必须是数字型," + ex.Message;
                    return -1;
                }
            }

            long lOldValue = 0;
            if (strOldValue != "")
            {
                try
                {
                    lOldValue = Convert.ToInt64(strOldValue);
                }
                catch
                {
                    strError = "当action为AddInteger时,原数据中的值'" + strOldValue + "'不合法，必须是数字型";
                    return -1;
                }
            }
            long lLastValue = lNewValue + lOldValue;
            strLastValue = Convert.ToString(lLastValue);

            return 0;
        }

        public static string AppendString(string strNewValue,
            string strOldValue)
        {
            return strOldValue + strNewValue;
        }

        // 解析资源路径中的xpath部分
        // parameters:
        //		strOrigin	资源路径中传来的xpath路径
        //		strLocateXpath	out参数，返回定位的节点路径
        //		strCreatePath	out参数，返回创建的节点路径
        //		strNewRecordTemplate	新数据模板
        //		strAction	行为
        //		strError	出错信息
        // return:
        //		-1	出错
        //		0	成功
        public static int ParseXPathParameter(string strOrigin,
            out string strLocateXPath,
            out string strCreatePath,
            out string strNewRecordTemplate,
            out string strAction,
            out string strError)
        {
            strLocateXPath = "";
            strCreatePath = "";
            strNewRecordTemplate = "";
            strAction = "";
            strError = "";

            if (strOrigin.Length == 0)
            {
                strError = "strOrigin不能为空字符串";
                return -1;
            }

            // 开后门，xpath可以直接用简单的式子
            if (strOrigin[0] == '@')
            {
                strLocateXPath = strOrigin.Substring(1);
                return 0;
            }

            strOrigin = "<root>" + strOrigin + "</root>";
            XmlDocument tempDom = new XmlDocument();
            tempDom.PreserveWhitespace = true; //设PreserveWhitespace为true

            try
            {
                tempDom.LoadXml(strOrigin);
            }
            catch (Exception ex)
            {
                strError = "PaseXPathParameter() 解析strOrigin到dom出错,原因:" + ex.Message;
                return -1;
            }

            // locate
            XmlNode node = tempDom.DocumentElement.SelectSingleNode("//locate");
            if (node != null)
                strLocateXPath = node.InnerText.Trim(); // 2012/2/16

            // create
            node = tempDom.DocumentElement.SelectSingleNode("//create");
            if (node != null)
                strCreatePath = node.InnerText.Trim(); // 2012/2/16

            // template		
            node = tempDom.DocumentElement.SelectSingleNode("//template");
            if (node != null)
            {
                if (node.ChildNodes.Count != 1)
                {
                    strError = "template下级有且只能有一个儿子节点";
                    return -1;
                }
                if (node.ChildNodes[0].NodeType != XmlNodeType.Element)
                {
                    strError = "template的下级必须是元素类型";
                    return -1;
                }
                strNewRecordTemplate = node.InnerXml;
            }

            // action
            node = tempDom.DocumentElement.SelectSingleNode("//action");
            if (node != null)
                strAction = node.InnerText.Trim(); // 2012/2/16

            return 0;
        }

        public static byte[] StringToByteArray(string strText,
            byte[] baPreamble)
        {
            byte[] baText = Encoding.UTF8.GetBytes(strText);
            if (baPreamble != null
                && baPreamble.Length > 0)
            {
                baText = ByteArray.Add(baText, baPreamble);
            }
            return baText;
        }


        // 将一个字节数组转换成字符串，内部会自动检查preamble
        //		bHasPreamble	返回bytes是否带preamble
        public static string ByteArrayToString(byte[] bytes,
            out byte[] baOutputPreamble)
        {
            baOutputPreamble = new byte[0];

            int nIndex = 0;
            int nCount = bytes.Length;

            byte[] baPreamble = Encoding.UTF8.GetPreamble();
            if (bytes.Length > baPreamble.Length)
            {
                if (baPreamble != null
                    && baPreamble.Length != 0)
                {
                    byte[] temp = new byte[baPreamble.Length];
                    Array.Copy(bytes,
                        0,
                        temp,
                        0,
                        temp.Length);

                    bool bEqual = true;
                    for (int i = 0; i < temp.Length; i++)
                    {
                        if (temp[i] != baPreamble[i])
                        {
                            bEqual = false;
                            break;
                        }
                    }

                    if (bEqual == true)
                    {
                        baOutputPreamble = baPreamble;
                        nIndex = temp.Length;
                        nCount = bytes.Length - temp.Length;
                    }
                }
            }
            return Encoding.UTF8.GetString(bytes,
                nIndex,
                nCount);
        }

        // 合并元数据信息
        // parameters:
        //		strOldMetadata	旧元数据
        //		strNewMetadata	新元数据
        //		lLength	长度 -1表示长度未知 -2表示长度不变
        //      strReadCount  读取次数。"" 表示不变(即不修改 readCount 属性内容), "+??"表示增加数量，"-??"表示减少数量，"??"表示直接修改为此数量
        //		strResult	out参数，返回合并后的元数据
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public static int MergeMetadata(string strOldMetadata,
            string strNewMetadata,
            long lLength,
            string strReadCount,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            XmlDocument oldDom = new XmlDocument();
            oldDom.PreserveWhitespace = true; //设PreserveWhitespace为true
            if (string.IsNullOrEmpty(strOldMetadata) == true)
            {
                // strOldMetadata = "<file/>";
                XmlNode root = oldDom.CreateElement("file");
                oldDom.AppendChild(root);
            }
            else
            {
                try
                {
                    oldDom.LoadXml(strOldMetadata);
                }
                catch (Exception ex)
                {
                    strError = "库中的元数据不合法\r\n" + ex.Message;
                    return -1;
                }
            }

            XmlElement oldRoot = oldDom.DocumentElement;

            XmlDocument newDom = new XmlDocument();
            newDom.PreserveWhitespace = true; //设PreserveWhitespace为true
            if (string.IsNullOrEmpty(strNewMetadata) == true)
            {
                // strNewMetadata = "<file/>";
                XmlNode root = newDom.CreateElement("file");
                newDom.AppendChild(root);
            }
            else
            {
                try
                {
                    newDom.LoadXml(strNewMetadata);
                }
                catch (Exception ex)
                {
                    strError = "传来的资源元数据不合法\r\n" + ex.Message;
                    return -1;
                }
            }
            XmlElement newRoot = newDom.DocumentElement;

            for (int i = 0; i < newRoot.Attributes.Count; i++)
            {
                XmlAttribute attr = newRoot.Attributes[i];
                oldRoot.SetAttribute(attr.Name, attr.Value);
                // DomUtil.SetAttr(oldRoot, attr.Name, attr.Value);
            }

            /*
            string strMimetype = DomUtil.GetAttr(newRoot, "mimetype");
            if (strMimetype != "")
                DomUtil.SetAttr(oldRoot, "mimetype", strMimetype);

            string strLocalPath = DomUtil.GetAttr(newRoot, "localpath");
            if (strLocalPath != "")
                DomUtil.SetAttr(oldRoot, "localpath", strLocalPath);
             * */

            // -1表示长度未知(-1本身要被写进去)
            // -2表示保持不变
            if (lLength != -2)
            {
                oldRoot.SetAttribute("size", Convert.ToString(lLength));
                // DomUtil.SetAttr(oldRoot, "size", Convert.ToString(lLength));
            }

            // 2015/8/18
            if (string.IsNullOrEmpty(strReadCount) == false)
            {
                if (strReadCount[0] == '+' || strReadCount[0] == '-')
                {
                    string strValue = strReadCount.Substring(1);
                    if (StringUtil.IsPureNumber(strValue) == false)
                    {
                        strError = "strReadCount 参数值 '"+strReadCount+"' 应该为正负号引导的纯数字";
                        return -1;
                    }
                    long v = 0;
                    if (long.TryParse(strValue, out v) == false)
                    {
                        strError = "strReadCount 参数值 '" + strReadCount + "' 应该为正负号引导的纯数字 ...";
                        return -1;
                    }
                    string strOld = oldRoot.GetAttribute("readCount");
                    long old = 0;
                    if (string.IsNullOrEmpty(strOld) == false)
                        long.TryParse(strOld, out old);

                    oldRoot.SetAttribute("readCount", (old + v).ToString());
                }
                else 
                {
                    long v = 0;
                    if (long.TryParse(strReadCount, out v) == false)
                    {
                        strError = "strReadCount 参数值 '" + strReadCount + "' 应该为纯数字 ...";
                        return -1;
                    }
                    oldRoot.SetAttribute("readCount", strReadCount);
                }
            }

            // 只有当新的没有传过来最后修改时间，才主动设置为当前时间
            // string strTempLastModified = DomUtil.GetAttr(newRoot, "lastmodified");
            string strTempLastModified = newRoot.GetAttribute("lastmodified");
            if (String.IsNullOrEmpty(strTempLastModified) == true)
            {
                oldRoot.SetAttribute("lastmodified", System.DateTime.Now.ToString());
                // DomUtil.SetAttr(oldRoot, "lastmodified", System.DateTime.Now.ToString());
            }

            strResult = oldRoot.OuterXml;
            return 0;
        }


#if NOOOOOOOOOOOOOOOO
        // 检索范围是否合法,并返回真正能够取的长度
        // parameter:
        //		nStart          起始位置 不能小于0
        //		nNeedLength     需要的长度	不能小于-1，-1表示从nStart-(nTotalLength-1)
        //		nTotalLength    数据实际总长度 不能小于0
        //		nMaxLength      限制的最大长度	等于-1，表示不限制
        //		nOutputLength   out参数，返回的可以用的长度
        //		strError        out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        public static int GetRealLength(int nStart,
            int nNeedLength,
            int nTotalLength,
            int nMaxLength,
            out int nOutputLength,
            out string strError)
        {
            nOutputLength = 0;
            strError = "";

            // 起始值,或者总长度不合法
            if (nStart < 0
                || nTotalLength < 0)
            {
                strError = "范围错误:nStart < 0 或 nTotalLength <0 \r\n";
                return -1;
            }
            if (nStart != 0
                && nStart >= nTotalLength)
            {
                strError = "范围错误:起始值大于总长度\r\n";
                return -1;
            }

            nOutputLength = nNeedLength;
            if (nOutputLength == 0)
            {
                return 0;
            }

            if (nOutputLength == -1)  // 从开始到全部
                nOutputLength = nTotalLength - nStart;

            if (nStart + nOutputLength > nTotalLength)
                nOutputLength = nTotalLength - nStart;

            // 限制了最大长度
            if (nMaxLength != -1 && nMaxLength >= 0)
            {
                if (nOutputLength > nMaxLength)
                    nOutputLength = nMaxLength;
            }
            return 0;
        }
#endif



        // 创建xml文件
        // parameters:
        //      strFileName 文件名
        //      strXml  xml字符串
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // strXml参数值只能为空（null或空字符串）,或者合法xml
        // 如果为空，则创建一个空文件
        public static int CreateXmlFile(string strFileName,
            string strXml,
            out string strError)
        {
            strError = "";
            if (String.IsNullOrEmpty(strXml) == true)
            {
                using(Stream s = File.Create(strFileName))
                {

                }
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "CreateXmlFile()，加载字符串到dom时出错：" + ex.Message;
                return -1;
            }

            using (XmlTextWriter w = new XmlTextWriter(strFileName,
                System.Text.Encoding.UTF8))
            {
                dom.Save(w);
                w.Close();
            }
            return 0;
        }


        // 得到
        public static string GetAllCaption(XmlNode node)
        {
            string strXpath = "property/logicname/caption";
            XmlNodeList list = node.SelectNodes(strXpath);

            string strAllCaption = "";
            foreach (XmlNode oneNode in list)
            {
                string strCaption = oneNode.InnerText.Trim();
                if (strCaption == "")
                    continue;

                if (strAllCaption != "")
                    strAllCaption += ",";

                strAllCaption += strCaption;
            }

            return strAllCaption;
        }



        public static List<XmlNode> GetNodes(XmlNode root,
            string strCfgItemPath)
        {
            Debug.Assert(root != null, "GetNodes()调用错误，root参数值不能为null。");
            Debug.Assert(strCfgItemPath != null && strCfgItemPath != "", "GetNodes()调用错误，strCfgItemPath不能为null。");


            List<XmlNode> nodes = new List<XmlNode>();

            //把strpath用'/'分开
            string[] paths = strCfgItemPath.Split(new char[] { '/' });
            if (paths.Length == 0)
                return nodes;

            int i = 0;
            if (paths[0] == "")
                i = 1;
            XmlNode nodeCurrent = root;
            for (; i < paths.Length; i++)
            {
                string strName = paths[i];

                bool bFound = false;
                foreach (XmlNode child in nodeCurrent.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element)
                        continue;

                    bool bThisFound = false;

                    if (String.Compare(child.Name, "database", true) == 0)
                    {
                        string strAllCaption = DatabaseUtil.GetAllCaption(child);
                        if (StringUtil.IsInList(strName, strAllCaption) == true)
                        {
                            bFound = true;
                            bThisFound = true;
                        }
                        else
                        {
                            bThisFound = false;
                        }
                    }
                    else
                    {
                        string strChildName = DomUtil.GetAttr(child, "name");
                        if (String.Compare(strName, strChildName, true) == 0)
                        {
                            bFound = true;
                            bThisFound = true;
                        }
                        else
                        {
                            bThisFound = false;
                        }
                    }

                    if (bThisFound == true)
                    {
                        if (i == paths.Length - 1)
                        {
                            nodes.Add(child);
                        }
                        else
                        {
                            nodeCurrent = child;
                            break;
                        }
                    }
                }

                // 本级未找到，跳出循环
                if (bFound == false)
                    break;
            }

            return nodes;

        }

    }
}
