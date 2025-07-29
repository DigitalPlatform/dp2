using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.rms;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.ResultSet;
using DigitalPlatform.Text;
using static DigitalPlatform.ResultSet.DpResultSet;
using System.Data.SqlClient;

namespace DigitalPlatform.rms
{

    public class SearchItem
    {
        public string TargetTables;  // 检索目标表,以逗号分隔
        public string Word = "";     // 检索词
        public string Match = "";    // 匹配方式
        public string Relation = "";  // 关系符
        public string DataType = "";  // 数据类型

        public string IdOrder = "";
        public string KeyOrder = "";

        public string OrderBy = "";     // 排序风格
        public int MaxCount = -1;       // 限制的最大条数 -1:不限
        public string Timeout = "";     // 超时时间长度。例如 00:02:00 表示两分钟

        // 2025/1/23
        // 描述信息
        public string Description = "";
    }

    // 专门的逻辑检索类,并不用考虑加锁的问题，
    // 因为在DatabaseCollection里的DoSearch()函数里做堆栈变量
    public class Query
    {
        DatabaseCollection m_dbColl;  // 1.数据库集合指针
        User m_oUser;                 // 2.帐户指针
        public XmlDocument m_dom;     // 3.检索式dom

        // 处理警告的等级
        //   0:严厉，不宽容警告，直接返回-1
        //   1:宽容，当各项矛盾时，系统自动做出修改后，进行检索
        int m_nWarningLevel = 1;


        //静态成员m_precedenceTable,string数组，存放操作符与对应的优先级
        public static string[] m_precedenceTable = {"NOT","2",
                                                       "OR","1",
                                                       "AND","1",
                                                       "SUB","1",
                                                       "!","2",
                                                       "+","1",
                                                       "-","1"};
        // 构造函数
        // paramter:
        //		dbColl  数据库集合指针
        //		user    帐户对象指针
        //		dom     检索式DOM
        public Query(DatabaseCollection dbColl,
            User user,
            XmlDocument dom)
        {
            m_dbColl = dbColl;
            m_oUser = user;
            m_dom = dom;

            // 从dom里找到warnging的处理级别信息
            string strWarningLevel = "";
            XmlNode nodeWarningLevel = dom.SelectSingleNode("//option");
            if (nodeWarningLevel != null)
                strWarningLevel = DomUtil.GetAttr(nodeWarningLevel, "warning");

            if (StringUtil.RegexCompare(@"\d", strWarningLevel) == true)
                m_nWarningLevel = Convert.ToInt32(strWarningLevel);
        }


        // 得到一个操作符对应的优先级
        // parameter:
        //		strOperator 操作符
        // return: 
        //		!= -1   该操作符的优先级，
        //		== -1   没找到,注意在使用的地方，-1代表没找到，不能参于比较
        public static int GetPrecedence(string strOperator)
        {
            //每次过两个数
            for (int i = 0; i < m_precedenceTable.Length; i += 2)
            {
                if (String.Compare(m_precedenceTable[i], strOperator, true) == 0)
                {
                    //因为任何时候都是两个一组，所以这个循环是安全的，
                    //但更严密是，即使m_precedenceTable没有配正确，此函数也不报错，需要做如下判断
                    if (i + 1 < m_precedenceTable.Length)
                        return Convert.ToInt32(m_precedenceTable[i + 1]);
                    else
                        return -1;
                }
            }
            return -1;
        }


        /*
         The algorithm in detail
        Read character. 
        1.If the character is a number then add it to the output. 
        如果字符是数字，直接加到output队列

        2.If the character is an operator then do something based on the various situations: 
        如果字符是运算符，根据下列的情况处理:

            If the operator has a higher precedence than the operator at the top of the stack or the stack is empty, 
            push the operator onto the stack. 
            如果运算符的的优先级高于栈顶元素，或者栈为空，则将该运算符push到栈里
	
            If the operator's precedence is less than or equal to the precedence of the operator at the top of the stack stack 
            then pop operators off the stack, 
            onto the output until the operator at the top of the stack has less precedence than the current operator or there is no more stack to pop.
            At this point, push the operator onto the stack. 
            如果运算的优先级低于或者等于栈顶运算符，
            则从栈顶pop运算符，加到output队列里，
            直到栈顶元素的优先级小于当前运算符，或者栈变为空时，
            push当前操作到栈里
	
        3.If the character is a left-parenthesis then push it onto the stack. 
        如果运算符是一个左括号，直接push到栈里

        4.If the character is a right-parenthesis 
        then pop an operators off the stack,
        onto the output until the operator read is a left-parenthesis 
        at which point it is popped off the stack but not added to the output. 
        如果运算符是一个右括号，则从栈里pop运算符，加到output队列里，
        直到遇到一个左括号，把左括号pop出栈，但不加到队列里

        If the stack runs out without finding a left-parenthesis 
        then there are mismatched parenthesis. 
        如果从栈里没有找到左括号，则括号不匹配，即少左括号

        5.If there are no more characters after the current,
        pop all the operators off the stack and exit. 
        If a left-parenthesis is popped then there are mismatched parenthesis. 
        最后，从栈里pop所有的操作符，加到output队列里。
        如果遇到一个左括号，则出现括号不匹配错误，即少右括号。

        After doing one or more of the above steps, go back to the start. 
         */

        // 根据上述算法，将传入父亲节点儿子的正常顺利转换成逆波兰表顺利，
        // 得到一个ArrayList
        // 注意过滤掉扩展的节点
        // parameter:
        //		node    父亲节点
        //		output  out参数，返回逆波兰表顺利的ArrayList
        //				注意有可能是空ArrayList。
        //		strErrorInfo    out参数，返回处理信息
        // return:
        //		-1  出错 例如:括号不匹配;找不到某操作符的优先级
        //      0   可用节点数为 0。等于没有任何可检索的必要
        //		1   成功
        public int Infix2RPN(XmlNode node,
            out List<XmlElement> output,
            out string strError)
        {
            strError = "";
            // output = new ArrayList();
            output = new List<XmlElement>();
            if (node == null)
            {
                strError = "node 不能为 null\r\n";
                return -1;
            }
            if (node.ChildNodes.Count == 0)
            {
                strError = "检索式片段 '" + node.OuterXml + "' 中没有任何检索节点";  // "node 的 ChildNodes 个数为 0\r\n";
                return 0;
            }

            //声明的一个栈对象，直接用.net的Stack类
            Stack stackOperator = new Stack();

            foreach (XmlNode nodeChild in node.ChildNodes)
            {
                // 2022/8/11
                if (nodeChild.NodeType != XmlNodeType.Element)
                    continue;

                //用于其它用途的扩展节点，需要跳过
                /*
                if (nodeChild.Name == "lang" || nodeChild.Name == "option")
                    continue;
                */
                // 2025/1/12
                if (nodeChild.Name != "operator" && nodeChild.Name != "item"
                    && nodeChild.Name != "target" && nodeChild.Name != "group")
                    continue;

                // 2010/9/26 add
                if (nodeChild.NodeType == XmlNodeType.Whitespace)
                    continue;

                //操作数时，加到output.
                if (nodeChild.Name != "operator")
                {
                    output.Add(nodeChild as XmlElement);
                }
                else //操作符时
                {
                    string strOperator = DomUtil.GetAttr(nodeChild, "value");

                    //本身有这个节点，但value为空时，按OR算
                    if (strOperator == "")
                        strOperator = "OR";

                    //左括号直接push栈里
                    if (strOperator == "(")
                    {
                        stackOperator.Push(nodeChild);
                        continue;
                    }

                    //右括号时
                    if (strOperator == ")")
                    {
                        bool bFound = false;
                        while (stackOperator.Count != 0)
                        {
                            //首先从栈里pop出一个操作符
                            var nodeTemp = stackOperator.Pop() as XmlElement;

                            string strTemp = DomUtil.GetAttr(node, "value");

                            //如果不等于左括号，加到output里
                            if (strTemp != "(")
                            {
                                output.Add(nodeTemp);
                            }
                            else  //等于则跳出循环
                            {
                                bFound = true;
                                break;
                            }
                        }

                        //没找到左括号时，返回-1
                        if (bFound == false)
                        {
                            strError = "右括号缺少配对的左括号";
                            return -1;
                        }
                    }


                    //如果栈为空，直接push
                    if (stackOperator.Count == 0)
                    {
                        stackOperator.Push(nodeChild);
                        continue;
                    }

                    //得到当前操作符的优先级
                    int nPrecedence;
                    nPrecedence = Query.GetPrecedence(strOperator);

                    //-1，代表没找到
                    if (nPrecedence == -1)
                    {
                        strError = "没有找到操作符" + strOperator + "的优先级<br/>";
                        return -1;
                    }

                    //从栈里peek一个元素（不用pop，否则可能还会加回去）
                    XmlNode nodeFromStack = (XmlNode)stackOperator.Peek();
                    string strOperatorFormStack = "";
                    if (nodeFromStack != null)
                        strOperatorFormStack = DomUtil.GetAttr(nodeFromStack, "value");

                    //得到栈里元素的优先级
                    int nPrecedenceFormStack;
                    nPrecedenceFormStack = Query.GetPrecedence(strOperatorFormStack);

                    if (nPrecedenceFormStack == -1)
                    {
                        strError = "没有找到运操作符" + strOperatorFormStack + "的优先级<br/>";
                        return -1;
                    }

                    //当前操作符的优先级高于栈顶运算符的优先级，
                    //则将当前操作符push到栈里
                    if (nPrecedence > nPrecedenceFormStack)
                    {
                        stackOperator.Push(nodeChild);
                    }

                    //当前低于栈顶时
                    if (nPrecedence <= nPrecedenceFormStack)
                    {
                        //下列代码严密
                        while (true)
                        {
                            //栈空时，将当前操作符push到栈里，跳出循环
                            if (stackOperator.Count == 0)
                            {
                                stackOperator.Push(nodeChild);
                                break;
                            }

                            //得到栈顶操作符(使用peek)，及优先级
                            XmlElement nodeIn = stackOperator.Peek() as XmlElement;
                            string strOperatorIn = "";
                            if (nodeIn != null)
                                strOperatorIn = DomUtil.GetAttr(nodeIn, "value");

                            int nPrecedenceIn;
                            nPrecedenceIn = Query.GetPrecedence(strOperatorIn);

                            if (nPrecedenceIn == -1)
                            {
                                strError = "没有找到运操作符" + strOperatorIn + "的优先级<br/>";
                                return -1;
                            }

                            //当前高于栈顶的，则将当前push到栈里，跳出循环
                            if (nPrecedence > nPrecedenceIn)
                            {
                                stackOperator.Push(nodeChild);
                                break;
                            }

                            //其它从栈里pop，加到output里
                            nodeIn = stackOperator.Pop() as XmlElement;
                            output.Add(nodeIn);
                        }
                    }
                }
            }


            //最后，将栈里剩下的操作符，从栈里pop，加到output里
            while (stackOperator.Count != 0)
            {
                var nodeTemp = stackOperator.Pop() as XmlElement;

                //操作符为左括号，括号不匹配，出错，返回-1
                string strOperator = "";
                if (nodeTemp != null)
                    strOperator = DomUtil.GetAttr(nodeTemp, "value");
                if (strOperator == ")")
                {
                    strError = "左括号多";
                    return -1;
                }

                output.Add(nodeTemp);
            }

            return 1;
        }

        // 功能: 检查检索单元match,relation,dataType三项的关系
        // 如果存在矛盾:
        // 如果处理警告的级别为0,会给检索单元node加warning信息，不自动更正，返回-1,
        // 如果级别为非0（应该固定为1），则系统自动更加，并在更正的地方加comment信息，返回0
        // 不存在矛盾:返回0
        // parameter:
        //		nodeItem    检索单元节点
        // return:
        //		0   正常（如果有矛盾，但由于处理警告级别为非0，自动更正，也返回0）
        //		-1  存在矛盾，且警告级别为0
        public int ProcessRelation(XmlNode nodeItem)
        {
            //匹配方式
            XmlNode nodeMatch = nodeItem.SelectSingleNode("match");
            string strMatch = "";
            if (nodeMatch != null)
                strMatch = nodeMatch.InnerText.Trim(); // 2012/2/16
            else
            {
                // 2008/11/23
                nodeMatch = nodeItem.OwnerDocument.CreateElement("match");
                nodeItem.AppendChild(nodeMatch);
            }


            if (String.IsNullOrEmpty(strMatch) == true)
            {
                strMatch = "left";  // 2008/11/23
                // DomUtil.SetNodeText(nodeMatch, strMatch);   // 原来是"left"
                nodeMatch.InnerText = strMatch;   // 原来是"left"   // 2012/2/16
                DomUtil.SetAttr(nodeMatch, "comment",
                    "原为" + strMatch + ",修改为缺省的left");
            }

            //关系操作符
            XmlNode nodeRelation = nodeItem.SelectSingleNode("relation");
            string strRelation = "";
            if (nodeRelation != null)
                strRelation = nodeRelation.InnerText.Trim(); // 2012/2/16

            strRelation = QueryUtil.ConvertLetterToOperator(strRelation);

            //数据类型
            XmlNode nodeDataType = nodeItem.SelectSingleNode("dataType");
            string strDataType = "";
            if (nodeDataType != null)
                strDataType = nodeDataType.InnerText.Trim(); // 2012/2/16

            if (strDataType == "number")
            {
                if (strMatch == "left" || strMatch == "right")
                {
                    //当dataType值为number时，match值为left或right或
                    //修改可以自动有两种:
                    //1.将left换成exact;
                    //2.将dataType设为string,
                    //我们先按dataType优先，将match改为exact

                    if (m_nWarningLevel == 0)
                    {
                        DomUtil.SetAttr(nodeItem,
                            "warningInfo",
                            "匹配方式为值‘" + strMatch + "'与数据类型的值'" + strDataType + "'矛盾, 但因处理警告级别为0，不进行自动更正");

                        return -1;
                    }
                    else
                    {
                        nodeMatch.InnerText = "exact";   // 2012/2/16
                        DomUtil.SetAttr(nodeMatch,
                            "comment",
                            "原为" + strMatch + ",由于与数据类型'" + strDataType + "'矛盾，修改为exact");
                    }
                }
            }

            if (strDataType == "string")
            {
                //如果dataType值为string,
                //match值为left或right,且relation值不等于"="

                //出现矛盾，(我们认为match的left或rgith值，只与relation的"="值匹配)
                //有两种裁决办法：
                //1.将relation值改为"="号;
                //2.将match值由left或right改为exact
                //目前按1进行修改

                if ((strMatch == "left" || strMatch == "right") && strRelation != "=")
                {
                    //根据处理警告级别做不同的处理
                    if (m_nWarningLevel == 0)
                    {
                        DomUtil.SetAttr(nodeItem,
                            "warningInfo",
                            "关系操作符'" + strRelation + "'与数据类型" + strDataType + "和匹配方式'" + strMatch + "'不匹配");
                        return -1;
                    }
                    else
                    {
                        nodeRelation.InnerText = "=";   // 2012/2/16
                        DomUtil.SetAttr(nodeRelation,
                            "comment",
                            "原为" + strRelation + ",由于与数据类型'" + strDataType + "和匹配方式'" + strMatch + "'不匹配，修改为'='");
                    }
                }
            }
            return 0;
        }

        public static string GetSortBy(string strOutputStyle,
            bool throw_exception = true)
        {
            // 2025/1/21
            // 排序依据的列 sortby:id 或者 sortby:key
            var sortby = StringUtil.GetParameterByPrefix(strOutputStyle, "sortby");
            if (string.IsNullOrEmpty(sortby))
                sortby = "id";
            else
            {
                // 检查 sortby 值
                if (sortby != "id" && sortby != "key")
                {
                    var error = $"strOutputStyle 参数值 '{strOutputStyle}' 不合法: sortby: 子参数值应为 'id' 或 'key'";
                    if (throw_exception)
                        throw new ArgumentException(error);
                    return "id";
                }
            }

            // 检查 keyid keycount 的缺乏，和 sortby:key 之间的矛盾
            if (sortby == "key"
                && StringUtil.IsInList("keyid", strOutputStyle) == false
                && StringUtil.IsInList("keycount", strOutputStyle) == false)
                throw new ArgumentException($"当 strOutput 参数值 '{strOutputStyle}' 中不具备 keyid 或 keycount 时，不应使用 sortby:key 子参数");

            return sortby;
        }

        // 检索单元item的信息，对库进行检索
        // parameter:
        //		nodeItem	    item节点
        //		resultSet	结果集。返回时不能确保结果集已经排序。需要看resultset.Sorted成员
        //		            传进结果集,????????每次清空，既然每次清空，那还不如返回一个结果集呢
        //		isConnected	是否连接
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		-6	无足够的权限
        //		0	成功
        public int doItem(
            SessionInfo sessioninfo,
            string strOutputStyle,
            XmlElement nodeItem,
            ref DpResultSet resultSet,
            ChannelHandle handle,
            StringBuilder explainInfo,
            out string strError)
        {
            strError = "";
            if (nodeItem == null)
            {
                strError = "doItem() nodeItem 参数值不应为 null";
                return -1;
            }

            if (resultSet == null)
            {
                strError = "doItem() resultSet 参数值不应为 null";
                return -1;
            }

            // 2025/1/17
            if (handle != null && handle.CancelToken.IsCancellationRequested)
            {
                strError = "前端中断";
                return -1;
            }

#if REMOVED
            // 2025/1/21
            // 排序依据的列 sortby:id 或者 sortby:key
            var sortby = StringUtil.GetParameterByPrefix(strOutputStyle, "sortby");
            if (string.IsNullOrEmpty(sortby))
                sortby = "id";
            else
            {
                // 检查 sortby 值
                if (sortby != "id" && sortby != "key")
                {
                    strError = $"strOutputStyle 参数值 '{strOutputStyle}' 不合法: sortby: 子参数值应为 'id' 或 'key'";
                    return -1;
                }
            }
#endif
            var sortby_key = Query.GetSortBy(strOutputStyle) == "key";

            string strResultSetName = nodeItem.GetAttribute("resultset");
            if (string.IsNullOrEmpty(strResultSetName) == false)
            {
                resultSet.Close();
                resultSet = null;

                DpResultSet source = null;
                if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                {
                    source = this.m_dbColl.KernelApplication.ResultSets.GetResultSet(strResultSetName.Substring(1), false);
                }
                else
                {
                    source = sessioninfo.GetResultSet(strResultSetName, false);
                }

                if (source == null)
                {
                    strError = "没有找到名为 '" + strResultSetName + "' 的结果集对象";
                    return -1;
                }
                resultSet = source.Clone("handle");

                // 2022/1/16
                // 对即将 ReadOnly 的结果集先排序。因为一旦变成 ReadOnly 以后就不允许排序了
                if (EnsureSorted(resultSet, handle, sortby_key,
                    "克隆后",
                    explainInfo,
                    out strError) == -1)
                {
                    strError = $"在对克隆后的结果集排序时出现错误: {strError}";
                    return -1;
                }

                resultSet.ReadOnly = true;
                return 0;
            }

            //先清空一下
            resultSet.Clear();

            int nRet;

            //调processRelation对检索单元的成员检查是否存在矛盾
            //如果返回0，则可能对item的成员进行了修改，所以后面重新提取内容
            nRet = ProcessRelation(nodeItem);
            if (nRet == -1)
            {
                // strError = "doItem()里调processRelation出错";
                strError = "检索式局部有错: " + nodeItem.OuterXml;
                return -1;
            }

            // 根据nodeItem得到检索信息
            /*
            string strTarget;
            string strWord;
            string strMatch;
            string strRelation;
            string strDataType;
            string strIdOrder;
            string strKeyOrder;
            string strOrderBy;
            string strHint = "";
            int nMaxCount;
            */
            nRet = QueryUtil.GetSearchInfo(nodeItem,
                strOutputStyle,
                out string strTarget,
                out string strWord,
                out string strMatch,
                out string strRelation,
                out string strDataType,
                out string strIdOrder,
                out string strKeyOrder,
                out string strOrderBy,
                out int nMaxCount,
                out string strHint,
                out string timeout,
                out strError);
            if (nRet == -1)
                return -1;

            bool bSearched = false;
            bool bNeedSort = false;

            bool bFirst = StringUtil.IsInList("first", strHint);    // 是否为 命中则停止继续检索

            int hit_database_count = 0; // 检索真正发生命中的数据库数量
            // 将 target 以 ; 号分成多个库
            // TODO: 其实多个库之间也可以考虑实现为 select union
            string[] aDatabase = strTarget.Split(new Char[] { ';' });
            foreach (string strOneDatabase in aDatabase)
            {
                if (strOneDatabase == "")
                    continue;

                string strDbName;
                string strTableList;

                // 拆分库名与途径
                nRet = DatabaseUtil.SplitToDbNameAndForm(strOneDatabase,
                    out strDbName,
                    out strTableList,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 得到库
                Database db = m_dbColl.GetDatabase(strDbName);
                if (db == null)
                {
                    strError = "未找到'" + strDbName + "'库";
                    return -1;
                }

                // 2009/7/19
                if (db.InRebuildingKey == true)
                {
                    strError = "数据库 '" + db.GetCaption(null) + "' 正处在重建检索点状态，不能进行检索...";
                    return -1;
                }

                string strTempRecordPath = db.GetCaption("zh-CN") + "/" + "record";
                string strExistRights = "";
                bool bHasRight = this.m_oUser.HasRights(strTempRecordPath,
                    ResType.Record,
                    "read",
                    out strExistRights);
                if (bHasRight == false)
                {
                    strError = "您的帐户名为'" + m_oUser.Name + "'" +
                        ",对'" + strDbName + "'" +
                        "数据库中的记录没有'读(read)'权限，目前的权限为'" + strExistRights + "'。";
                    return -6;
                }

                SearchItem searchItem = new SearchItem();
                searchItem.Description = $"{strOneDatabase}\r\n{DomUtil.GetIndentXml(nodeItem.OuterXml)}";
                searchItem.TargetTables = strTableList;
                searchItem.Word = strWord;
                searchItem.Match = strMatch;
                searchItem.Relation = strRelation;
                searchItem.DataType = strDataType;
                searchItem.IdOrder = strIdOrder;
                searchItem.KeyOrder = strKeyOrder;
                searchItem.OrderBy = strOrderBy;
                searchItem.MaxCount = nMaxCount;
                searchItem.Timeout = timeout;

                long prev_count = resultSet.Count;

                // 注: SearchByUnion不清空resultSet，从而使多个库的结果集放在一起
                string strWarningInfo = "";
                //		-1	出错
                //		0	成功
                //      1   成功，但resultset需要再行排序一次
                nRet = db.SearchByUnion(
                    strOutputStyle,
                    searchItem,
                    handle,
                    // isConnected,
                    resultSet,
                    this.m_nWarningLevel,
                    explainInfo,
                    out strError,
                    out strWarningInfo);
                if (nRet == -1)
                    return -1;

                bSearched = true;

                if (nRet == 1)
                {
                    bNeedSort = true;
                }

                if (prev_count < resultSet.Count)
                    hit_database_count++;

                if (nRet >= 1 && bFirst == true)
                    break;
            }

            // 2010/5/17
            if (bSearched == true)
            {
                // 2010/5/11
                resultSet.EnsureCreateIndex();   // 确保创建了索引

                // 2022/1/24
                // 把相同的 key 后面的 count 合并
                if (StringUtil.IsInList("keycount", strOutputStyle)
                    && hit_database_count > 1)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    if (DoSort(resultSet,
                        handle/*isConnected*/,
                        (a, b) =>
                        {
                            if (sortby_key)
                                return a.CompareToKey(b);
                            return a.CompareTo(b);
                        }) == true)
                    {
                        strError = "前端中断";
                        return -1;
                    }

                    sw.Stop();
                    explainInfo?.AppendLine($"key+count 合并阶段排序耗费时间 {sw.Elapsed}");
                    sw.Restart();

                    DpResultSet oTargetMiddle = sessioninfo.NewResultSet();   // new DpResultSet();
                    StringBuilder debugInfo = null;

                    // 合并
                    nRet = DpResultSetManager.MergeCount(resultSet,
    oTargetMiddle,
    querystop,
    handle,
    ref debugInfo,
    out strError);
                    if (nRet == -1)
                        return -1;
                    resultSet.Close();
                    resultSet = oTargetMiddle;

                    bNeedSort = false;  // 已经排序了

                    sw.Stop();
                    explainInfo?.AppendLine($"key+count 合并阶段归并耗费时间 {sw.Elapsed}");
                }

                // 排序
                // TODO: 其实可以使用EnsureSorted()函数
                if (bNeedSort == true)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    if (DoSort(resultSet,
                        handle/*isConnected*/,
                        (a, b) =>
                        {
                            if (sortby_key)
                                return a.CompareToKey(b);
                            return a.CompareTo(b);
                        }) == true)
                    {
                        strError = "前端中断";
                        return -1;
                    }

                    sw.Stop();
                    explainInfo?.AppendLine($"doItem 排序耗费时间 {sw.Elapsed}");
                }
                // TODO: 也可以延迟排序，本函数返回一个值表希望排序。等到最后不得不排序时候再排序最好了

                //resultSet.RemoveDup();
            }

            return 0;
        }


        // return:
        //      fasle   正常完成
        //      true    中断
        public static bool DoSort(DpResultSet resultset,
            ChannelHandle handle,
            // Delegate_isConnected isConnected
            delegate_compare func_compare = null)
        {
            resultset.Idle += new IdleEventHandler(sort_Idle);
            resultset.Param = handle;   //  isConnected;
            try
            {
                resultset.Sort(func_compare);
            }
            catch (InterruptException /*ex*/)
            {
                return true;    // 中断
            }
            finally
            {
                resultset.Idle -= new IdleEventHandler(sort_Idle);
                resultset.Param = null;
            }

            return false;
        }

        static void sort_Idle(object sender, IdleEventArgs e)
        {
            DpResultSet resultset = (DpResultSet)sender;
#if NO
            Delegate_isConnected isConnected = (Delegate_isConnected)resultset.Param;

            if (isConnected() == false)
            {
                throw new InterruptException("中断");
            }
#endif

            ChannelHandle handle = (ChannelHandle)resultset.Param;
            if (handle.DoIdle() == false)
                throw new InterruptException("中断");

            // e.bDoEvents = false;
        }


        // 递归函数，得一个节点的集合，外面doSearch调该函数
        // parameter:
        //		nodeRoot	当前根节点
        //		resultSet	结果集。返回时不能确保结果集已经排序。需要看resultset.Sorted成员
        // return:
        //		-1	出错
        //		-6	无权限
        //		0	成功
        public int DoQuery(
            SessionInfo sessioninfo,
            string strOutputStyle,
            XmlElement nodeRoot,
            ref DpResultSet resultSet,
            ChannelHandle handle,
            StringBuilder explainInfo,
            out string strError)
        {
#if DEBUG
            DateTime start_time = DateTime.Now;
            Debug.WriteLine("Begin DoQuery()");
#endif

            try
            {

                strError = "";
                if (nodeRoot == null)
                {
                    strError = "DoQuery() nodeRoot参数不能为null。";
                    return -1;
                }

#if NO
                if (resultSet == null)
                {
                    strError = "DoQuery() resultSet参数不能为null。";
                    return -1;
                }
#endif

                if (resultSet != null)
                {
                    // 先清空一下
                    resultSet.Clear();
                }

                // 到item时不再继续递归
                if (nodeRoot.Name == "item")
                {
                    if (resultSet == null)
                        resultSet = sessioninfo.NewResultSet(); // 延迟创建
                    // return:
                    //		-1	出错
                    //		-6	无足够的权限
                    //		0	成功
                    return doItem(
                        sessioninfo,
                        strOutputStyle,
                        nodeRoot,
                        ref resultSet,
                        handle,
                        explainInfo,
                        out strError);
                }

                //如果为扩展节点，则不递归
                if (nodeRoot.Name == "operator"
                    || nodeRoot.Name == "lang")
                {
                    return 0;
                }

                //将正常顺序变成逆波兰表序
                // return:
                //		-1  出错 例如:括号不匹配;找不到某操作符的优先级
                //      0   可用节点数为 0。等于没有任何可检索的必要
                //		1   成功
                int nRet = Infix2RPN(nodeRoot,
                    out List<XmlElement> rpn,
                    out strError);
                if (nRet == -1)
                {
                    strError = "逆波兰表错误:" + strError;
                    return -1;
                }
                if (nRet == 0)
                    return -1;

                //属于正常情况
                if (rpn.Count == 0)
                    return 0;

                // return:
                //		-1  出错
                //		-6	无足够的权限
                //		0   成功
                nRet = ProceedRPN(
                    sessioninfo,
                    strOutputStyle,
                    rpn,
                    ref resultSet,
                    handle,
                    explainInfo,
                    out strError);
                if (nRet <= -1)
                    return nRet;

                return 0;
            }
            finally
            {
#if DEBUG
                TimeSpan delta = DateTime.Now - start_time;
                Debug.WriteLine("End DoQuery() 耗时 " + delta.ToString());
#endif
            }
        }

        static int EnsureSorted(DpResultSet resultset,
            ChannelHandle handle,
            bool sortby_key,
            string comment,
            StringBuilder explainInfo,
            out string strError)
        {
            strError = "";

            resultset.EnsureCreateIndex();   // 确保创建了索引

            if (resultset.Sorted == false)
            {
                Stopwatch sw = Stopwatch.StartNew();

                if (DoSort(resultset,
                    handle/*isConnected*/,
                    (a, b) =>
                    {
                        if (sortby_key)
                            return a.CompareToKey(b);
                        return a.CompareTo(b);
                    }) == true)
                {
                    strError = "前端中断";
                    return -1;
                }

                Debug.Assert(resultset.Sorted == true, "");

                sw.Stop();
                explainInfo?.AppendLine($"{comment} 排序耗费时间 {sw?.Elapsed}");
            }
            return 0;
        }

        // 运算逆波兰表，得到结果集
        // parameter:
        //		rpn         逆波兰表
        //		resultSet  结果集
        // return:
        //		0   成功
        //		-1  出错 原因可能如下:
        //			1)rpn参数为null
        //			2)oResultSet参数为null
        //			3)栈里的某成员出错（node和result都为null）
        //			4)从栈中pop()或peek()元素时，出现栈空
        //			5)pop的类型，不是实际存在的类型
        //			6)通过一个节点，得到结果集，即调DoQuery()函数出错
        //			7)做运算时，调DpResultSetManager.Merge()函数出错
        //			8)最后栈里的元素多于1，则逆波兰表出错
        //			9)最后结果集为空
        //		-6	无足够的权限
        public int ProceedRPN(
            SessionInfo sessioninfo,
            string strOutputStyle,
            List<XmlElement> rpn,
            ref DpResultSet resultSet,
            ChannelHandle handle,
            // Delegate_isConnected isConnected,
            StringBuilder explainInfo,
            out string strError)
        {
#if DEBUG
            DateTime start_time = DateTime.Now;
            Debug.WriteLine("Begin ProceedRPN()");
#endif

            try
            {
                strError = "";
                //???要搞清楚用不用清空
                //应该清空，后面的运算使用的结果集是堆栈变量，最后把运算结果拷贝到该结果集
                //DoQuery处，也应该先清空
                //doItem处，一进去先清空，但再对数据库循环检索时，千万不清空

                if (resultSet != null)
                    resultSet.Clear();

                if (rpn == null)
                {
                    strError = "rpn不能为null";
                    return -1;
                }
#if NO
                if (resultSet == null)
                {
                    strError = "resultSet不能为null";
                    return -1;
                }
#endif

                if (rpn.Count == 0)
                    return 0;

                var sortby_key = Query.GetSortBy(strOutputStyle) == "key";

                int ret;

                // 声明栈,ReversePolishStack栈是自定义的类
                // 决定用一个栈做运算，如果遇到遇到操作数，就直接push到栈里
                // 遇到操作符，如果是双目，从栈里pop两项，进行运算
                // 注意SUB运算是，用后一次pop的对象减前一次pop的对象
                //
                // oReversePolandStack的成员为ReversePolishItem,
                // ReversePolishItem是一个复杂对象，
                // 包含m_int(类型),m_node(节点),m_resultSet.
                // 实际运用中，m_node和m_resultSet只有一项值有效，另一顶是null
                // m_int用于判断哪个值有效，
                // 0表示node有效，1表示resultSet有效
                ReversePolishStack oReversePolandStack =
                    new ReversePolishStack();

                // for (int i = 0; i < rpn.Count; i++)
                foreach (XmlElement node in rpn)
                {
                    // XmlElement node = (XmlElement)rpn[i];

                    if (node.Name != "operator")  //操作数直接push到栈里
                    {
                        oReversePolandStack.PushNode(node);
                    }
                    else
                    {
                        string strOpreator = DomUtil.GetAttr(node, "value");

#if NO
                        //三个输出用于输入的参数，因为是指针，所以不用out
                        DpResultSet oTargetLeft = sessioninfo.NewResultSet();   // new DpResultSet();
                        DpResultSet oTargetMiddle = sessioninfo.NewResultSet();   // new DpResultSet();
                        DpResultSet oTargetRight = sessioninfo.NewResultSet();   // new DpResultSet();
#endif

                        //做一个两个成员的ArrayList，
                        //成员类型为DpResultSet，
                        //存放从栈里pop出的（如果是node，需要进行计算）的结果集
                        List<DpResultSet> oSource = new List<DpResultSet>();
                        oSource.Add(sessioninfo.NewResultSet());   // new DpResultSet()
                        oSource.Add(sessioninfo.NewResultSet());   // new DpResultSet()
                        try
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                //类型为-1，表示node和resultSet都为null，出现错误
                                if (oReversePolandStack.PeekType() == -1)
                                {
                                    strError = strOpreator + "时,PeekType()等于-1，则表示两项都是null，出错，返回-1<br/>";
                                    return -1;
                                }

                                //表示放得是node
                                if (oReversePolandStack.PeekType() == 0)
                                {
                                    XmlElement nodePop;
                                    nodePop = oReversePolandStack.PopNode();
                                    if (nodePop == null)
                                    {
                                        strError = "nodePop不为又能为null";
                                        return -1;
                                    }

                                    DpResultSet temp = oSource[j];
                                    // return:
                                    //		-1	出错
                                    //		-6	无权限
                                    //		0	成功
                                    ret = this.DoQuery(
                                        sessioninfo,
                                        strOutputStyle,
                                        nodePop,
                                        ref temp,
                                        handle,
                                        explainInfo,
                                        out strError);
                                    if (ret <= -1)
                                        return ret;
                                    if (temp != oSource[j])
                                    {
                                        // 2014/3/11
                                        if (oSource[j] != null)
                                            oSource[j].Close();
                                        oSource[j] = temp;
                                    }
                                }
                                else
                                {
                                    DpResultSet temp = oReversePolandStack.PopResultSet();
                                    Debug.Assert(temp != oSource[j], "");

                                    // 2014/3/11
                                    if (oSource[j] != null)
                                        oSource[j].Close();
                                    oSource[j] = temp;

                                    if (oSource[j] == null)
                                    {
                                        strError = "PopResultSet() return null";
                                        return -1;
                                    }
                                }
                            }
                        }
                        catch (StackUnderflowException /*ex*/)
                        {
                            // 2008/12/4
                            string strOutXml = "";
                            if (node.ParentNode != null)
                            {
                                int nRet = DomUtil.GetIndentXml(node.ParentNode.OuterXml,
                                    out strOutXml,
                                    out strError);
                            }
                            strError = strOpreator + " 是二元操作符，它缺乏操作数。";

                            if (String.IsNullOrEmpty(strOutXml) == false)
                                strError += "\r\n" + strOutXml;

                            // strError = "StackUnderflowException :" + ex.Message;
                            return -1;
                        }

                        // string strDebugInfo;

                        //OR,AND,SUB运算都是调的DpResultSetManager.Merge()函数，
                        //注意参数的使用
                        if (strOpreator == "OR")
                        {
                            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
                            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

                            DpResultSet left = oSource[1];
                            DpResultSet right = oSource[0];

#if DEBUG
                            Debug.Assert(left.IsClosed == false, "");
                            Debug.Assert(right.IsClosed == false, "");
#endif

                            {
                                // 直接相加
                                if (left.Count == 0)
                                {
                                    oReversePolandStack.PushResultSet(
                                        right
                                        );
                                    // 2014/3/11
                                    Debug.Assert(left != right, "");
                                    left.Close();
                                }
                                else if (right.Count == 0)
                                {
                                    oReversePolandStack.PushResultSet(
                                        left
                                        );
                                    // 2014/3/11
                                    Debug.Assert(left != right, "");
                                    right.Close();
                                }
                                else
                                {
                                    if (EnsureSorted(left, handle, sortby_key,
                                        "OR (1)运算前对左侧",
                                        explainInfo,
                                        out strError) == -1)
                                        return -1;
                                    if (EnsureSorted(right, handle, sortby_key,
                                        "OR (1)运算前对右侧",
                                        explainInfo,
                                        out strError) == -1)
                                        return -1;
                                    // return:
                                    //      -1  出错
                                    //      0   没有交叉部分
                                    //      1   有交叉部分
                                    ret = DpResultSetManager.IsCross(left,
                                        right,
                                        out strError);
                                    if (ret == -1)
                                        return -1;
                                    if (ret == 0)
                                    {
                                        DpResultSet left_save = left;
                                        // 注意：函数执行过程，可能交换 left 和 right。也就是说返回后， left == right
                                        ret = DpResultSetManager.AddResults(ref left,
                                            right,
                                            out strError);
                                        if (ret == -1)
                                            return -1;

                                        oReversePolandStack.PushResultSet(
                                            left
                                            );
                                        // 2014/3/11
                                        if (left != right)
                                        {
                                            Debug.Assert(left_save == left, "");
                                            right.Close();
                                        }
                                        else
                                        {
                                            Debug.Assert(left_save != left, "");
                                            left_save.Close();
                                        }
                                    }
                                    else
                                    {
                                        if (left.Asc != right.Asc)
                                        {
                                            right.Asc = left.Asc;
                                            right.Sorted = false;
                                        }

                                        if (EnsureSorted(left, handle, sortby_key,
                                            "OR (2)运算前对左侧",
                                            explainInfo,
                                            out strError) == -1)
                                            return -1;
                                        if (EnsureSorted(right, handle, sortby_key,
                                            "OR (2)运算前对右侧",
                                            explainInfo,
                                            out strError) == -1)
                                            return -1;

                                        {
                                            DpResultSet oTargetMiddle = sessioninfo.NewResultSet();   // new DpResultSet();
                                            StringBuilder debugInfo = null;
                                            ret = DpResultSetManager.Merge(LogicOper.OR,
        left,
        right,
        strOutputStyle,
        null,
        oTargetMiddle,
        null,
        // false,
        querystop,
        handle,
        ref debugInfo,
        out strError);
                                            if (ret == -1)
                                                return -1;

                                            oReversePolandStack.PushResultSet(oTargetMiddle);
                                            // 2014/3/11
                                            Debug.Assert(left != oTargetMiddle, "");
                                            Debug.Assert(right != oTargetMiddle, "");
                                            left.Close();
                                            right.Close();
                                        }
                                    }
                                }
                            }

                            continue;
                        }

                        if (strOpreator == "AND")
                        {
                            DpResultSet left = oSource[1];
                            DpResultSet right = oSource[0];
#if DEBUG
                            Debug.Assert(left.IsClosed == false, "");
                            Debug.Assert(right.IsClosed == false, "");
#endif

                            if (left.Asc != right.Asc)
                            {
                                right.Asc = left.Asc;
                                right.Sorted = false;
                            }

                            if (EnsureSorted(left, handle, sortby_key,
                                "AND 运算前对左侧",
                                explainInfo,
                                out strError) == -1)
                                return -1;
                            if (EnsureSorted(right, handle, sortby_key,
                                "AND 运算前对右侧",
                                explainInfo,
                                out strError) == -1)
                                return -1;

                            // 优化
                            if (left.Count == 0)
                            {
                                oReversePolandStack.PushResultSet(
                                    left
                                    );
                                // 2014/3/11
                                Debug.Assert(left != right, "");
                                right.Close();
                            }
                            else if (right.Count == 0)
                            {
                                oReversePolandStack.PushResultSet(
                                    right
                                    );
                                // 2014/3/11
                                Debug.Assert(left != right, "");
                                left.Close();
                            }
                            else
                            {
                                DpResultSet oTargetMiddle = sessioninfo.NewResultSet();   // new DpResultSet();

                                StringBuilder debugInfo = null;

                                ret = DpResultSetManager.Merge(LogicOper.AND,
                                    left,
                                    right,
                                    strOutputStyle,
                                    null,    //oTargetLeft
                                    oTargetMiddle,
                                    null,   //oTargetRight
                                            // false,
                                    querystop,
                                    handle,
                                    ref debugInfo,
                                    out strError);
                                if (ret == -1)
                                    return -1;

                                oReversePolandStack.PushResultSet(oTargetMiddle);
                                // 2014/3/11
                                Debug.Assert(left != oTargetMiddle, "");
                                Debug.Assert(right != oTargetMiddle, "");
                                left.Close();
                                right.Close();
                            }

                            continue;
                        }

                        if (strOpreator == "SUB")
                        {
                            //因为使用从栈里pop，所以第0个是后面的，第1个是前面的
                            DpResultSet left = oSource[1];
                            DpResultSet right = oSource[0];
#if DEBUG
                            Debug.Assert(left.IsClosed == false, "");
                            Debug.Assert(right.IsClosed == false, "");
#endif

                            if (left.Asc != right.Asc)
                            {
                                right.Asc = left.Asc;
                                right.Sorted = false;
                            }

                            if (EnsureSorted(left,
                                handle,
                                sortby_key,
                                "SUB 运算前对左侧",
                                explainInfo,
                                out strError) == -1)
                                return -1;
                            if (EnsureSorted(right,
                                handle,
                                sortby_key,
                                "SUB 运算前对右侧",
                                explainInfo,
                                out strError) == -1)
                                return -1;

                            // 优化
                            if (left.Count == 0)
                            {
                                oReversePolandStack.PushResultSet(
                                    left
                                    );
                                // 2014/3/11
                                Debug.Assert(left != right, "");
                                right.Close();
                            }
                            else if (right.Count == 0)
                            {
                                oReversePolandStack.PushResultSet(
                                    left
                                    );
                                // 2014/3/11
                                Debug.Assert(left != right, "");
                                right.Close();
                            }
                            else
                            {
                                DpResultSet oTargetLeft = sessioninfo.NewResultSet();   // new DpResultSet();

                                StringBuilder debugInfo = null;

                                ret = DpResultSetManager.Merge(LogicOper.SUB,
                                    left,
                                    right,
                                    strOutputStyle,
                                    oTargetLeft,
                                    null, //oTargetMiddle
                                    null, //oTargetRight
                                          // false,
                                    querystop,
                                    handle,
                                    ref debugInfo,
                                    out strError);
                                if (ret == -1)
                                {
                                    return -1;
                                }

                                oReversePolandStack.PushResultSet(oTargetLeft);
                                // 2014/3/11
                                Debug.Assert(left != oTargetLeft, "");
                                Debug.Assert(right != oTargetLeft, "");
                                left.Close();
                                right.Close();
                            }

                            continue;
                        }
                    }
                }
                if (oReversePolandStack.Count > 1)
                {
                    strError = "逆波兰出错";
                    return -1;
                }
                try
                {
                    int nType = oReversePolandStack.PeekType();
                    //如果类型为0,表示存放的是节点
                    if (nType == 0)
                    {
                        XmlElement node = oReversePolandStack.PopNode();

                        // return:
                        //		-1	出错
                        //		-6	无权限
                        //		0	成功
                        ret = this.DoQuery(
                            sessioninfo,
                            strOutputStyle,
                            node,
                            ref resultSet,
                            handle,
                            explainInfo,
                            out strError);
                        if (ret <= -1)
                            return ret;
                        // 2022/1/24
                        // TODO: 需要把里面的 count 值归并
                        if (StringUtil.IsInList("keycount", strOutputStyle))
                        {

                        }
                    }
                    else if (nType == 1)
                    {
                        // 调DpResultSet的copy函数

                        // TODO: 测算这个Copy所花费的时间。
                        // resultSet.Copy((DpResultSet)(oReversePolandStack.PopResultSet()));
                        resultSet = (DpResultSet)(oReversePolandStack.PopResultSet());
                    }
                    else
                    {
                        strError = "oReversePolandStack的类型不可能为" + Convert.ToString(nType);
                        return -1;
                    }
                }
                catch (StackUnderflowException)
                {
                    strError = "peek或pop时，抛出StackUnderflowException异常";
                    return -1;
                }

                //最后结果集为null，返回出错
                if (resultSet == null)
                {
                    strError = "运算结束后PopResultSet为null" + Convert.ToString(oReversePolandStack.PeekType());
                    return -1;
                }

                return 0;
            }
            finally
            {
#if DEBUG
                TimeSpan delta = DateTime.Now - start_time;
                Debug.WriteLine("End ProceedRPN() 耗时 " + delta.ToString());
#endif
            }
        }

        bool querystop(object param)
        {
            ChannelHandle handle = (ChannelHandle)param;
            if (handle.DoIdle() == true)
                return false;
            return true;
        }

    }  //  end of class Query


    // 逆波兰对象类，是ReversePolishStack的成员
    public class ReversePolishItem
    {
        public int m_int;               // 类型 0:node 1:结果集
        public XmlElement m_node;          // node节点
        public DpResultSet m_resultSet; // 结果集

        // 构造函数
        // parameter:
        //		node        节点
        //		oResultSet  结果集
        public ReversePolishItem(XmlElement node,
            DpResultSet resultSet)
        {
            m_node = node;
            m_resultSet = resultSet;

            if (m_node != null)
            {
                m_int = 0; //0表示XmlNode
            }
            else if (m_resultSet != null)
            {
                m_int = 1; //1表示resultSet
            }
            else
            {
                m_int = -1;
            }
        }

    } //  end of class ReversePolishItem


    // 在ProceedRPN函数里，用到的逆波兰栈，注意这个栈只存放值，成员是ReversePolishItem
    public class ReversePolishStack : ArrayList
    {
        // 只创建带节点的ReversePolishItem对象，并push到栈里
        // parameter:
        //		node    node节点
        // return:
        //      void
        public void PushNode(XmlElement node)
        {
            ReversePolishItem oItem = new ReversePolishItem(node,
                null);
            Add(oItem);
        }

        // 只创建带结果集的ReversePolishItem对象，并push到栈里
        // parameter:
        //		oResult 结果集
        // return:
        //      void
        public void PushResultSet(DpResultSet oResult)
        {
            ReversePolishItem oItem = new ReversePolishItem(null,
                oResult);
            Add(oItem);
        }

        // 创建同时带节点和结果集的ReversePolishItem对象,并push到栈里
        // parameter:
        //		node    节点
        //		oResult 结果集
        // return:
        //      void
        public void Push(XmlElement node,
            DpResultSet oResult)
        {
            ReversePolishItem oItem = new ReversePolishItem(node,
                oResult);
            Add(oItem);
        }

        // pop一个对象，只返回节点
        // return:
        //		node节点
        public XmlElement PopNode()
        {
            //栈为空，抛出StackUnderflowException异常
            if (this.Count == 0)
            {
                StackUnderflowException ex = new StackUnderflowException("Pop前,堆栈已经空");
                throw (ex);
            }

            ReversePolishItem oTemp = (ReversePolishItem)this[this.Count - 1];
            this.RemoveAt(this.Count - 1);

            //可能返回空，调用者错用了类型，应在调用处做判断。
            return oTemp.m_node;
        }

        // pop一个对象，只返回结果集
        // return:
        //		结果集
        public DpResultSet PopResultSet()
        {
            if (this.Count == 0)
            {
                StackUnderflowException ex = new StackUnderflowException("Pop前,堆栈已经空");
                throw (ex);
            }

            ReversePolishItem oTemp = (ReversePolishItem)this[this.Count - 1];
            this.RemoveAt(this.Count - 1);

            //可能返回空，调用者错用了类型，应在调用处做判断。
            return oTemp.m_resultSet;
        }


        // pop一个对象
        // return:
        //		ReversePolishItem对象
        public ReversePolishItem Pop()
        {
            if (this.Count == 0)
            {
                StackUnderflowException ex = new StackUnderflowException("Pop前,堆栈已经空");
                throw (ex);
            }

            ReversePolishItem oTemp = (ReversePolishItem)this[this.Count - 1];
            this.RemoveAt(this.Count - 1);

            //可能返回空，调用者错用了类型，应在调用处做判断。
            return oTemp;
        }

        // 返回栈顶元素的类型，以便于是pop节点带是pop结果集
        // return:
        //		仅返回栈顶对象的的类型
        public int PeekType()
        {
            if (this.Count == 0)
            {
                StackUnderflowException ex = new StackUnderflowException("Pop前,堆栈已经空");
                throw (ex);
            }

            ReversePolishItem oTemp = (ReversePolishItem)this[this.Count - 1];
            return oTemp.m_int;
        }

        // 返回栈顶元素
        // return:
        //      返回ReversePolishItem对象
        public ReversePolishItem Peek()
        {
            if (this.Count == 0)
            {
                StackUnderflowException ex = new StackUnderflowException("Pop前,堆栈已经空");
                throw (ex);
            }

            ReversePolishItem oTemp = (ReversePolishItem)this[this.Count - 1];
            return oTemp;
        }

    } //  end of class ReversePolishStack


}
