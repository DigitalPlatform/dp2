using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Collections;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Text.SectionPropertyString;

namespace DigitalPlatform.rms
{
    // 配置权限
    public class CfgRights
    {
        public XmlNode nodeRoot = null; // 权限定义根元素
        public ArrayList MacroRights = null; // 宏权限数组

        // 初始化
        // parameters:
        //      node   权限定义根节点 
        // return:
        //      -1  出错
        //      0   成功
        public int Initial(XmlNode node,
            out string strError)
        {
            strError = "";

            this.nodeRoot = node;

            // 设宏权限数组
            this.InitialMacroRights();

            return 0;
        }

        // 初始化宏权限数组
        private void InitialMacroRights()
        {
            MacroRights = new ArrayList();

            this.MacroRights.Add(new MacroRightItem(
                "write",
                "overwrite,delete,create"));

            string strManagementRights =
                "list,"
                + "read,"
                + "overwrite,delete,create,"
                + "clear,"
                + "changepassword,";

            this.MacroRights.Add(new MacroRightItem(
                "management",
                strManagementRights));
        }


        // 检查权限
        // parameters:
        //      strPath     资源路径
        //      resType     资源类型
        //      strRights   被查找的权限
        //      strExistRights  out参数,返回已存在的权限
        //      resultType  out参数,返回查找结果
        //                  Minus = -1, // 减
        //                  None = 0,   // 没有定义    
        //                  Plus = 1,   // 加
        //      strError    out参数,返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public int CheckRights(
            string strPath,
            List<string> aOwnerDbName,
            string strUserName,
            ResType resType,
            string strQueryRights,
            out string strExistRights,
            out ResultType resultType,
            out string strError)
        {
            strError = "";
            strExistRights = "";

            resultType = ResultType.None;

            Debug.Assert(resType != ResType.None, "resType参数不能为ResType.None");

            List<string> aRights = null;
            int nRet = this.BuildRightArray(
                strPath,
                aOwnerDbName,
                strUserName,
                out aRights,
                out strError);
            if (nRet == -1)
                return -1;

            string strResType = "";
            if (resType == ResType.Database)
                strResType = "database";
            else if (resType == ResType.Directory)
                strResType = "directory";
            else if (resType == ResType.File)
                strResType = "leaf";
            else if (resType == ResType.Record)
                strResType = "record";
            else
                strResType = "leaf";

            for (int i = aRights.Count - 1; i >= 0; i--)
            {
                string strOneRights = aRights[i];

                string strSectionName = "";


                string strRealRights = "";
                if (i == aRights.Count - 1)
                {
                    strRealRights = strOneRights;
                }
                else if (i == aRights.Count - 2)
                {
                    strSectionName = "children_" + strResType;
                    strRealRights = this.GetSectionRights(strOneRights,
                        strSectionName);

                    if (strRealRights == "" && strResType != "database")
                    {
                        strSectionName = "descendant_" + strResType;
                        strRealRights = this.GetSectionRights(strOneRights,
                            strSectionName);
                    }
                }
                else
                {
                    strSectionName = "descendant_" + strResType;
                    strRealRights = this.GetSectionRights(strOneRights,
                        strSectionName);
                }

                string strPureRights = this.GetSectionRights(strRealRights, "this");

                if (strPureRights != "")
                {
                    if (strExistRights != "")
                        strExistRights = strExistRights + ",";
                    strExistRights += strPureRights;
                }


                // 检查当前权限字符串中是否存在指定的权限,加，减都返回
                resultType = this.CheckRights(strQueryRights,
                    strPureRights);
                if (resultType != ResultType.None)
                    return 0;
            }

            return 0;
        }


        // ????目前不支持数据库的多语言版本
        // 为CheckRights()服务的底层函数
        // 根据资源路径创建权限数组
        // parameters:
        //      strPath     资源路径
        //      aRights     out参数,返回权限数组成
        //      strError    out参数,返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        private int BuildRightArray(
            string strPath,
            List<string> aOwnerDbName,
            string strUserName,
            out List<string> aRights,
            out string strError)
        {
            strError = "";

            aRights = new List<string>();

            string strRights = "";

            // 把根定义的权限加到数组里
            strRights = DomUtil.GetAttr(this.nodeRoot, "rights");
            aRights.Add(strRights);

            if (strPath == "")
                return 0;

            string[] paths = strPath.Split(new char[] { '/' });
            Debug.Assert(paths.Length > 0, "此时数组长度不可能为0。");
            if (paths[0] == "" || paths[paths.Length - 1] == "")
            {
                strError = "路径'" + strPath + "'不合法，首尾不能为'/'。";
                return -1;
            }

            XmlNode nodeCurrent = this.nodeRoot;
            // 循环下级
            for (int i = 0; i < paths.Length; i++)
            {
                string strName = paths[i];
                bool bFound = false;

                if (nodeCurrent == null)
                {
                    aRights.Add("");
                    continue;
                }

                foreach (XmlNode child in nodeCurrent.ChildNodes)
                {
                    if (child.NodeType != XmlNodeType.Element)
                        continue;

                    string strChildName = DomUtil.GetAttr(child, "name");

                    if (String.Compare(strName, strChildName, true) == 0)
                    {
                        bFound = true;
                        nodeCurrent = child;
                        break;
                    }
                }

                bool bDbo = false;
                if (i == 0)   // 数据库层次
                {
                    if (aOwnerDbName.IndexOf(strName) != -1)
                        bDbo = true;
                }

                strRights = "";

                // 为dbo增加特殊权限
                if (bDbo == true)
                {
                    strRights += "this:management;children_database:management;children_directory:management;children_leaf:management;descendant_directory:management;descendant_record:management;descendant_leaf:management";
                }

                if (bFound == false)
                {
                    aRights.Add(strRights);
                    nodeCurrent = null;
                    continue;
                }

                // 实际定义的权限
                if (nodeCurrent != null)
                {
                    string strTemp = DomUtil.GetAttr(nodeCurrent, "rights");
                    if (String.IsNullOrEmpty(strTemp) == false)
                    {
                        if (strRights != "")
                            strRights += ";";
                        strRights += strTemp;
                    }
                }

                aRights.Add(strRights);
            }
            return 0;
        }

        // 为CheckRights()服务的底层函数
        // parameters:
        //      strRights   被查找的权限
        //      strAllRights    已存在的全部权限
        // return:
        //      ResultType对象
        //          Minus = -1, // 减
        //          None = 0,   // 没有定义    
        //          Plus = 1,   // 加
        private ResultType CheckRights(string strRights,
            string strAllRights)
        {
            if (strAllRights == "")
                return ResultType.None;

            strAllRights = this.CanonicalizeRightString(strAllRights);

            string[] rights = strAllRights.Split(new char[] { ',' });
            for (int i = rights.Length - 1; i >= 0; i--)
            {
                string strOneRight = rights[i];
                if (strOneRight == "")
                    continue;

                string strFirstChar = strOneRight.Substring(0, 1);

                // 前面有+ , - 号的情况
                if (strFirstChar == "+" || strFirstChar == "-")
                {
                    strOneRight = strOneRight.Substring(1);
                }

                if (String.Compare(strRights, strOneRight, true) == 0
                    || strOneRight == "*")
                {
                    if (strFirstChar == "-")
                        return ResultType.Minus;
                    else
                        return ResultType.Plus;
                }
            }

            return ResultType.None;
        }

        // 规范化权限字符串
        private string CanonicalizeRightString(string strRights)
        {
            for (int i = 0; i < this.MacroRights.Count; i++)
            {
                MacroRightItem item = (MacroRightItem)this.MacroRights[i];

                strRights = strRights.Replace(item.MacroRight, item.RealRight);
            }
            return strRights;
        }

        // 得到指定小节的权限
        private string GetSectionRights(string strRights,
            string strCategory)
        {
            DigitalPlatform.Text.SectionPropertyString.PropertyCollection propertyColl =
                new DigitalPlatform.Text.SectionPropertyString.PropertyCollection("this",
                strRights,
                DelimiterFormat.Semicolon);
            Section section = propertyColl[strCategory];
            if (section == null)
                return "";

            return section.Value;
        }

    }

    public enum ResultType
    {
        Minus = -1, // 减
        None = 0,   // 没有定义    
        Plus = 1,   // 加
    }

    // 资源类型
    public enum ResType
    {
        None = 0,
        Server = 1,
        Database = 2,
        Record = 3,
        Directory = 4,
        File = 5,
    }

    // 宏权限对象
    public class MacroRightItem
    {
        public string MacroRight = "";
        public string RealRight = "";

        public MacroRightItem(string strMacroRight,
            string strRealRight)
        {
            this.MacroRight = strMacroRight;
            this.RealRight = strRealRight;
        }
    }
}
