using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关各种数据库名和属性的功能
    /// </summary>
    public partial class MainForm
    {
        /*
* 检查两条记录是否适合建立目标关系
* 1) 源库，应当是采购工作库
* 2) 源库和目标库，不是一个库
* 3) 源库的syntax和目标库的syntax，是一样的
* 4) 目标库不应该是外源角色
* */
        // parameters:
        //      strSourceRecPath    记录ID可以为问号
        //      strTargetRecPath    记录ID可以为问号，仅当bCheckTargetWenhao==false时
        // return:
        //      -1  出错
        //      0   不适合建立目标关系 (这种情况是没有什么错，但是不适合建立)
        //      1   适合建立目标关系
        internal int CheckBuildLinkCondition(string strSourceBiblioRecPath,
            string strTargetBiblioRecPath,
            bool bCheckTargetWenhao,
            out string strError)
        {
            strError = "";

            // TODO: 最好检查一下这个路径的格式。合法的书目库名可以在MainForm中找到

            // 检查是不是书目库名。MARC格式是否和当前数据库一致。不能是当前记录自己
            string strTargetDbName = Global.GetDbName(strTargetBiblioRecPath);
            string strTargetRecordID = Global.GetRecordID(strTargetBiblioRecPath);

            if (String.IsNullOrEmpty(strTargetDbName) == true
                || String.IsNullOrEmpty(strTargetRecordID) == true)
            {
                strError = "目标记录路径 '" + strTargetBiblioRecPath + "' 不是合法的记录路径";
                goto ERROR1;
            }

            // 2009/11/25 
            if (this.IsBiblioSourceDb(strTargetDbName) == true)
            {
                strError = "库 '" + strTargetDbName + "' 是 外源书目库 角色，不能作为目标库";
                return 0;
            }

            // 2011/11/29
            if (this.IsOrderWorkDb(strTargetDbName) == true)
            {
                strError = "库 '" + strTargetDbName + "' 是 采购工作库 角色，不能作为目标库";
                return 0;
            }

            string strSourceDbName = Global.GetDbName(strSourceBiblioRecPath);
            string strSourceRecordID = Global.GetRecordID(strSourceBiblioRecPath);

            if (String.IsNullOrEmpty(strSourceDbName) == true
                || String.IsNullOrEmpty(strSourceRecordID) == true)
            {
                strError = "源记录路径 '" + strSourceBiblioRecPath + "' 不是合法的记录路径";
                goto ERROR1;
            }

            /*
            if (this.IsOrderWorkDb(strSourceDbName) == false)
            {
                strError = "源库 '" + strSourceDbName + "' 不具备 采购工作库 角色";
                return 0;
            }*/

            // 根据书目库名获得MARC格式语法名
            // return:
            //      null    没有找到指定的书目库名
            string strSourceSyntax = this.GetBiblioSyntax(strSourceDbName);
            if (String.IsNullOrEmpty(strSourceSyntax) == true)
                strSourceSyntax = "unimarc";
            string strSourceIssueDbName = this.GetIssueDbName(strSourceDbName);


            bool bFound = false;
            if (this.BiblioDbProperties != null)
            {
                foreach (var prop in this.BiblioDbProperties)
                {
                    // BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strTargetDbName)
                    {
                        bFound = true;

                        string strTargetSyntax = prop.Syntax;
                        if (String.IsNullOrEmpty(strTargetSyntax) == true)
                            strTargetSyntax = "unimarc";

                        if (strTargetSyntax != strSourceSyntax)
                        {
                            strError = "拟设置的目标记录因其书目数据格式为 '" + strTargetSyntax + "'，与源记录的书目数据格式 '" + strSourceSyntax + "' 不一致，因此操作被拒绝";
                            return 0;
                        }

                        if (String.IsNullOrEmpty(prop.IssueDbName)
                            != String.IsNullOrEmpty(strSourceIssueDbName))
                        {
                            strError = "拟设置的目标记录因其书目库 '" + strTargetDbName + "' 文献类型(期刊还是图书)和源记录的书目库 '" + strSourceDbName + "' 不一致，因此操作被拒绝";
                            return 0;
                        }
                    }
                }
            }

            if (bFound == false)
            {
                strError = "'" + strTargetDbName + "' 不是合法的书目库名";
                goto ERROR1;
            }

            // source

            if (this.IsBiblioDbName(strSourceDbName) == false)
            {
                strError = "'" + strSourceDbName + "' 不是合法的书目库名";
                goto ERROR1;
            }

            if (strSourceRecordID == "?")
            {
                /* 源记录ID可以为问号，因为这不妨碍建立连接关系
                strError = "源记录 '"+strSourceBiblioRecPath+"' 路径中ID不能为问号";
                return 0;
                 * */
            }
            else
            {
                // 如果不是问号，就要检查一下了，没坏处
                if (Global.IsPureNumber(strSourceRecordID) == false)
                {
                    strError = "源记录  '" + strSourceBiblioRecPath + "' 路径中ID部分必须为纯数字";
                    goto ERROR1;
                }
            }

            // target
            if (strTargetRecordID == "?")
            {
                if (bCheckTargetWenhao == true)
                {
                    strError = "目标记录 '" + strTargetBiblioRecPath + "' 路径中ID不能为问号";
                    return 0;
                }
            }
            else
            {
                if (Global.IsPureNumber(strTargetRecordID) == false)
                {
                    strError = "目标记录 '" + strTargetBiblioRecPath + "' 路径中ID部分必须为纯数字";
                    goto ERROR1;
                }
            }

            if (strTargetDbName == strSourceDbName)
            {
                strError = "目标记录和源记录不能属于同一个书目库 '" + strTargetBiblioRecPath + "'";
                return 0;
                // 注：这样就不用检查目标是否源记录了
            }

            return 1;
        ERROR1:
            return -1;
        }


        // 
        // return:
        //      null    没有找到指定的书目库名
        //      其他      MARC 格式语法名
        /// <summary>
        /// 根据书目库名获得 MARC 格式语法名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>语法名。如果为 null 表示没有找到</returns>
        public string GetBiblioSyntax(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                        return property.Syntax;
                }
            }

            return null;
        }

        // return:
        //      null    没有找到指定的规范库名
        //      其他      MARC 格式语法名
        public string GetAuthoritySyntax(string strBiblioDbName)
        {
            if (this.AuthorityDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.AuthorityDbProperties)
                {
                    if (prop.DbName == strBiblioDbName)
                        return prop.Syntax;
                }
            }

            return null;
        }

        // 
        // return:
        //      null    没有找到指定的书目库名
        /// <summary>
        /// 根据读者库名获得馆代码
        /// </summary>
        /// <param name="strReaderDbName">读者库名</param>
        /// <returns>管代码</returns>
        public string GetReaderDbLibraryCode(string strReaderDbName)
        {
            if (this.ReaderDbProperties != null)
            {
                foreach (ReaderDbProperty prop in this.ReaderDbProperties)
                {
                    if (prop.DbName == strReaderDbName)
                        return prop.LibraryCode;
                }
            }

            return null;
        }

        // 2013/6/15
        // 
        /// <summary>
        /// 获得全部可用的馆代码
        /// </summary>
        /// <returns>字符串集合</returns>
        public List<string> GetAllLibraryCode()
        {
            List<string> results = new List<string>();
            if (this.ReaderDbProperties != null)
            {
                foreach (ReaderDbProperty prop in this.ReaderDbProperties)
                {
                    results.Add(prop.LibraryCode);
                }

                /*
                results.Sort();
                StringUtil.RemoveDup(ref results, true);
                */
                StringUtil.RemoveDupNoSort(ref results);
            }

            return results;
        }

#if NO
        // 
        /// <summary>
        /// 判断一个库名是否为合法的书目库名
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为合法的书目库名</returns>
        public bool IsValidBiblioDbName(string strDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.BiblioDbProperties.Count; i++)
                {
                    if (this.BiblioDbProperties[i].DbName == strDbName)
                        return true;
                }
            }

            return false;
        }
#endif

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为书目库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为书目库名</returns>
        public bool IsBiblioDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strDbName)
                        return true;
                }
            }

            return false;
        }


        // 
        // 如果返回""，表示该书目库的下属库没有定义
        /// <summary>
        /// 根据书目库名获得对应的下属库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <param name="strDbType">下属库的类型</param>
        /// <returns>下属库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备此类型的下属库</returns>
        public string GetItemDbName(string strBiblioDbName,
            string strDbType)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                    {
                        if (strDbType == "item")
                            return property.ItemDbName;
                        else if (strDbType == "order")
                            return property.OrderDbName;
                        else if (strDbType == "issue")
                            return property.IssueDbName;
                        else if (strDbType == "comment")
                            return property.CommentDbName;
                        else
                            return "";
                    }
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的实体库没有定义
        /// <summary>
        /// 根据书目库名获得对应的实体库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>实体库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备实体库</returns>
        public string GetItemDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                        return property.ItemDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的期库没有定义
        /// <summary>
        /// 根据书目库名获得对应的期库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>期库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备期库</returns>
        public string GetIssueDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                        return property.IssueDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的订购库没有定义
        /// <summary>
        /// 根据书目库名获得对应的订购库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>订购库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备订购库</returns>
        public string GetOrderDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                        return property.OrderDbName;
                }
            }

            return null;
        }

        // 
        // 如果返回""，表示该书目库的评注库没有定义
        /// <summary>
        /// 根据书目库名获得对应的评注库名
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>评注库名。如果为 null，表示书目库没有找到；如果为 ""，表示该书目库不具备评注库</returns>
        public string GetCommentDbName(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.DbName == strBiblioDbName)
                        return property.CommentDbName;
                }
            }

            return null;
        }

        // 实体库名 --> 期刊/图书类型 对照表
        Hashtable itemdb_type_table = new Hashtable();

        // 
        // return:
        //      -1  不是实体库
        //      0   图书类型
        //      1   期刊类型
        /// <summary>
        /// 观察实体库是不是期刊库类型
        /// </summary>
        /// <param name="strItemDbName">实体库名</param>
        /// <returns>-1: 不是实体库; 0: 图书类型; 1: 期刊类型</returns>
        public int IsSeriesTypeFromItemDbName(string strItemDbName)
        {
            int nRet = 0;
            object o = itemdb_type_table[strItemDbName];
            if (o != null)
            {
                nRet = (int)o;
                return nRet;
            }

            string strBiblioDbName = GetBiblioDbNameFromItemDbName(strItemDbName);
            if (strBiblioDbName == null)
                return -1;
            string strIssueDbName = GetIssueDbName(strBiblioDbName);
            if (string.IsNullOrEmpty(strIssueDbName) == true)
                nRet = 0;
            else
                nRet = 1;

            itemdb_type_table[strItemDbName] = nRet;
            return nRet;
        }

        // 
        /// <summary>
        /// 根据实体库名获得对应的书目库名
        /// </summary>
        /// <param name="strItemDbName">实体库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromItemDbName(string strItemDbName)
        {
            // 2008/11/28 
            // 实体库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.ItemDbName == strItemDbName)
                        return property.DbName;
                }
            }

            return null;
        }

        // 根据 册/订购/期/评注 记录路径和 parentid 构造所从属的书目记录路径
        public string BuildBiblioRecPath(string strDbType,
            string strItemRecPath,
            string strParentID)
        {
            if (string.IsNullOrEmpty(strParentID) == true)
                return null;

            string strItemDbName = Global.GetDbName(strItemRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
                return null;

            string strBiblioDbName = this.GetBiblioDbNameFromItemDbName(strDbType, strItemDbName);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return null;

            return strBiblioDbName + "/" + strParentID;
        }

        /// <summary>
        /// 根据实体(期/订购/评注)库名获得对应的书目库名
        /// </summary>
        /// <param name="strDbType">数据库类型</param>
        /// <param name="strItemDbName">数据库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromItemDbName(string strDbType,
            string strItemDbName)
        {
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                if (strDbType == "item")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.ItemDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "order")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.OrderDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "issue")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.IssueDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else if (strDbType == "comment")
                {
                    foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                    {
                        if (prop.CommentDbName == strItemDbName)
                            return prop.DbName;
                    }
                }
                else
                    throw new Exception("无法处理数据库类型 '" + strDbType + "'");
            }

            return null;
        }

        // 订购名 --> 期刊/图书类型 对照表
        Hashtable orderdb_type_table = new Hashtable();

        // 
        // return:
        //      -1  不是订购库
        //      0   图书类型
        //      1   期刊类型
        /// <summary>
        /// 观察订购库是不是期刊库类型
        /// </summary>
        /// <param name="strOrderDbName">订购库名</param>
        /// <returns>-1: 不是订购库; 0: 图书类型; 1: 期刊类型</returns>
        public int IsSeriesTypeFromOrderDbName(string strOrderDbName)
        {
            int nRet = 0;
            object o = orderdb_type_table[strOrderDbName];
            if (o != null)
            {
                nRet = (int)o;
                return nRet;
            }

            string strBiblioDbName = GetBiblioDbNameFromOrderDbName(strOrderDbName);
            if (strBiblioDbName == null)
                return -1;
            string strIssueDbName = GetIssueDbName(strBiblioDbName);
            if (string.IsNullOrEmpty(strIssueDbName) == true)
                nRet = 0;
            else
                nRet = 1;

            orderdb_type_table[strOrderDbName] = nRet;
            return nRet;
        }

        // 
        /// <summary>
        /// 根据期库名获得对应的书目库名
        /// </summary>
        /// <param name="strIssueDbName">期库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromIssueDbName(string strIssueDbName)
        {
            // 2008/11/28 
            // 期库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strIssueDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.IssueDbName == strIssueDbName)
                        return property.DbName;
                }
            }

            return null;
        }

        // 2017/3/5
        // 获得出版物类型
        // return:
        //      null    不清楚
        //      "book"
        //      "series"
        public string GetPubTypeFromOrderDbName(string strOrderDbName)
        {
            // 订购库名为空，无法找书目库名。
            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                {
                    if (prop.OrderDbName == strOrderDbName)
                        return string.IsNullOrEmpty(prop.IssueDbName) ? "book" : "series";
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 根据订购库名获得对应的书目库名
        /// </summary>
        /// <param name="strOrderDbName">订购库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromOrderDbName(string strOrderDbName)
        {
            // 2008/11/28 
            // 订购库名为空，无法找书目库名。
            // 其实也可以找，不过找出来的就不是唯一的了，所以干脆不找
            if (String.IsNullOrEmpty(strOrderDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.OrderDbName == strOrderDbName)
                        return property.DbName;
                }
            }

            return null;
        }

        // 
        /// <summary>
        /// 根据评注库名获得对应的书目库名
        /// </summary>
        /// <param name="strCommentDbName">评注库名</param>
        /// <returns>书目库名</returns>
        public string GetBiblioDbNameFromCommentDbName(string strCommentDbName)
        {
            if (String.IsNullOrEmpty(strCommentDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.CommentDbName == strCommentDbName)
                        return property.DbName;
                }
            }

            return null;
        }


        // 2009/11/25
        // 
        /// <summary>
        /// 是否为外源书目库?
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>是否为外源书目库</returns>
        public bool IsBiblioSourceDb(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var prop in this.BiblioDbProperties)
                {
                    // BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strBiblioDbName)
                    {
                        if (StringUtil.IsInList("biblioSource", prop.Role) == true)
                            return true;
                    }
                }
            }

            return false;
        }

        // 2009/10/24
        // 
        /// <summary>
        /// 是否为采购工作库?
        /// </summary>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <returns>是否为采购工作裤</returns>
        public bool IsOrderWorkDb(string strBiblioDbName)
        {
            if (this.BiblioDbProperties != null)
            {
                foreach (var prop in this.BiblioDbProperties)
                {
                    // BiblioDbProperty prop = this.BiblioDbProperties[i];

                    if (prop.DbName == strBiblioDbName)
                    {
                        if (StringUtil.IsInList("orderWork", prop.Role) == true)
                            return true;
                    }
                }
            }

            return false;
        }

        // 2025/10/22
        public bool HasRole(string biblio_dbname, string role)
        {
            return this.BiblioDbProperties.Where(p => p.DbName == biblio_dbname && StringUtil.IsInList(role, p.Role)).Any();
        }

        public bool IsCatalogWorkDb(string biblio_dbname)
        {
            return HasRole(biblio_dbname, "catalogWork");
        }

        public bool IsCatalogTargetDb(string biblio_dbname)
        {
            return HasRole(biblio_dbname, "catalogWork");
        }

        // 2012/9/1
        /// <summary>
        /// 获得一个书目库的属性信息
        /// </summary>
        /// <param name="strDbName">书目库名</param>
        /// <returns>属性信息对象</returns>
        public BiblioDbProperty GetBiblioDbProperty(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return null;

            if (this.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.BiblioDbProperties)
                {
                    if (prop.DbName == strDbName)
                        return prop;
                }
            }

            return null;
        }

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为实体库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为实体库名</returns>
        public bool IsItemDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                foreach(var property in this.BiblioDbProperties)
                {
                    if (property.ItemDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 2008/12/15
        // 
        /// <summary>
        /// 是否为读者库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为读者库名</returns>
        public bool IsReaderDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.ReaderDbNames != null)    // 2009/3/29 
            {
                for (int i = 0; i < this.ReaderDbNames.Length; i++)
                {
                    if (this.ReaderDbNames[i] == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为订购库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为订购库名</returns>
        public bool IsOrderDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.OrderDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为期库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为期库名</returns>
        public bool IsIssueDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                foreach(var property in this.BiblioDbProperties)
                {
                    if (property.IssueDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        // 
        /// <summary>
        /// 是否为评注库名?
        /// </summary>
        /// <param name="strDbName">数据库名</param>
        /// <returns>是否为评注库名</returns>
        public bool IsCommentDbName(string strDbName)
        {
            if (String.IsNullOrEmpty(strDbName) == true)
                return false;

            if (this.BiblioDbProperties != null)
            {
                foreach (var property in this.BiblioDbProperties)
                {
                    if (property.CommentDbName == strDbName)
                        return true;
                }
            }

            return false;
        }

        public string CompareTwoBiblioDbDef(string strSourceDbName, string strTargetDbName)
        {
            BiblioDbProperty source = this.GetBiblioDbProperty(strSourceDbName);
            BiblioDbProperty target = this.GetBiblioDbProperty(strTargetDbName);

            if (source == null)
                return "源书目库名 '" + strSourceDbName + "' 不存在";
            if (target == null)
                return "目标书目库名 '" + strTargetDbName + "' 不存在";

            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(source.ItemDbName) == false
&& string.IsNullOrEmpty(target.ItemDbName) == true)
                errors.Add("实体库 '" + source.ItemDbName + "' ");   // 左边大于右边

            if (string.IsNullOrEmpty(source.OrderDbName) == false
&& string.IsNullOrEmpty(target.OrderDbName) == true)
                errors.Add("订购库 '" + source.OrderDbName + "' ");   // 左边大于右边

            if (string.IsNullOrEmpty(source.IssueDbName) == false
                && string.IsNullOrEmpty(target.IssueDbName) == true)
                errors.Add("期库 '" + source.IssueDbName + "' ");   // 左边大于右边

            if (string.IsNullOrEmpty(source.CommentDbName) == false
&& string.IsNullOrEmpty(target.CommentDbName) == true)
                errors.Add("评注库 '" + source.CommentDbName + "' ");   // 左边大于右边

            if (errors.Count == 0)
                return null;

            return "源书目库 '" + strSourceDbName + "' 具有下属的" + StringUtil.MakePathList(errors, "、") + "，而目标书目库 '" + strTargetDbName + "' 缺乏";
        }

    }
}
