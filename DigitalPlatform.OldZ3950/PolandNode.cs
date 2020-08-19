using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using DigitalPlatform.Text;

namespace DigitalPlatform.OldZ3950
{
    public class PolandNode : BerNode
    {
        public string m_strOrgExpression = "";
        // public stringLPSTR m_pszOrgExpression;
        public string m_strToken;			// 当前读入的token
        public int m_nType;				// token的类型

        #region 常量

        public const int TYPE_AND	= 0;
        public const int TYPE_OR	= 1;
        public const int TYPE_NOT	= 2;
        public const int TYPE_OPERAND    = 3;
        public const int TYPE_NEAR	= 4;
        public const int TYPE_WITHIN	= 5;
        public const int TYPE_LEFTBRACKET = 6;		// 左括号
        public const int TYPE_RIGHTBRACKET = 7; 	// 右括号


        
        public const ushort z3950_Operand                       = 0;
        public const ushort z3950_Query                         = 1;
        public const ushort z3950_and                           = 0;
        public const ushort z3950_or                            = 1;
        public const ushort z3950_and_not                       = 2;
        public const ushort z3950_prox                          = 3;

/* 相似操作的几种属性 */
        public const ushort z3950_exclusion                     = 1;
        public const ushort z3950_distance                      = 2;
        public const ushort z3950_ordered                       = 3;
        public const ushort z3950_relationType                  = 4;
        public const ushort z3950_proximityUnitCode             = 5;

        public const ushort z3950_lessThan                      = 1;
        public const ushort z3950_lessThanOrEqual               = 2;
        public const ushort z3950_equal                         = 3;
        public const ushort z3950_greaterThanOrEqual            = 4;
        public const ushort z3950_greaterThan                   = 5;
        public const ushort z3950_notEqual                      = 6;
        public const ushort z3950_known                         = 1;
        public const ushort z3950_private                       = 2;

/* 查询类型 */
        public const ushort z3950_type_0                        = 0;
        public const ushort z3950_type_1                        = 1;
        public const ushort z3950_type_2                        = 2;
        public const ushort z3950_type_100                     =100;
        public const ushort z3950_type_101 = 101;

        #endregion


        TokenCollection m_OperatorArray = new TokenCollection();
        TokenCollection m_PolandArray = new TokenCollection();
        BerNodeStack m_StackArray = new BerNodeStack();

        public BerNode m_Subroot = new BerNode();

        public Encoding m_queryTermEncoding = Encoding.GetEncoding(936);

        int m_nOffs = 0;

        public char CurrentChar
        {
            get
            {
                if (this.m_nOffs >= this.m_strOrgExpression.Length)
                    return (char)0; // 兼容原来的习惯

                return this.m_strOrgExpression[this.m_nOffs];
            }
        }

        public bool ReachEnd
        {
            get
            {
                if (this.m_nOffs >= this.m_strOrgExpression.Length)
                    return true;
                return false;
            }
        }

        // 当前位置指针向后移动一个字符
        // return:
        //      true    到达末尾
        //      false   没有到达末尾
        public bool MoveNext()
        {
            this.m_nOffs++;
            return this.ReachEnd;
        }

        public PolandNode(string strOrgExpression)
        {
        	m_strOrgExpression = strOrgExpression;
	        // m_pszOrgExpression = (char *)(LPCSTR)m_strOrgExpression;
            m_nOffs = 0;
        }


        // 将原始表达式转换为逆波兰表达式
        // parameters
        // return:
        //		NULL
        //		其他
        public void ChangeOrgToRPN()
        {
            for (; ; )
            {
                GetAToken();
                HandlingAToken();
                if (m_strToken == "" && m_nType == -1)
                    break;
            }
            ChangeRPNToTree();
        }

        static bool IsWhite(char c)
        {
            if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                return true;
            else
                return false;
        }

        static bool IsDelim(char c)
        {
            if (" +*!()".IndexOf(c) != -1
                || c == 9 || c == '\r' || c == 0 || c == '\n' || c == ' ')
                return true;
            else
                return false;
        }


        // 将输入的表达式分割为独立的单元
        // parameters
        int GetAToken()
        {
            this.m_strToken = "";
            this.m_nType = -1;


            while (IsWhite(this.CurrentChar) == true) //去掉空格、\t、\r、\n
            {
                ++this.m_nOffs;
            }

            // **1 文件结尾：
            // 条件：遇到0字符
            // 返回：token内为空串
            //       token_type -1
            if (this.ReachEnd == true)
            {   //	如果遇到结尾。
                m_strToken = "";
                m_nType = -1;
                return 0;
            }

            // **3 括号
            // 条件：遇到"()"之一
            // 返回：token 左括号或右括号
            //       token_type  左括号或右括号
            if (this.CurrentChar == '('
                || this.CurrentChar == ')')
            {
                m_strToken = new string(this.CurrentChar, 1);
                this.MoveNext();

                if (m_strToken == "(")
                    m_nType = TYPE_LEFTBRACKET;
                else
                    m_nType = TYPE_RIGHTBRACKET;
                return 0;
            }

            // **5 字符串
            // 条件：遇到"
            // 返回：token 字符串，表示算子
            //       设置字符串类型为算子
            if (this.CurrentChar == '"')
            {  // quoted string 
                /*
                LPSTR pTemp;
                LPTSTR pStr;
                int nLen = 0;
                 * */

                // 找到结束的'"'
                int nRet = this.m_strOrgExpression.IndexOf('\"', this.m_nOffs + 1);
                if (nRet == -1)
                    throw new Exception("非法字符串常量");

                nRet++;	// 跳过"符号
                // 接着越过"后面接续的非delimeter段落
                for (; nRet < m_strOrgExpression.Length; nRet++)
                {
                    if (IsDelim(m_strOrgExpression[nRet]) == true)
                        break;
                }

                /*
                pTemp ++;	// 跳过"符号
                while (!IsDelim(*pTemp))
                {
                    pTemp++;
                }*/

                this.m_strToken = this.m_strOrgExpression.Substring(this.m_nOffs,
                    nRet - this.m_nOffs);


                m_nType = TYPE_OPERAND;

                this.m_nOffs = nRet + 1;

                return 0;

            }

            // **6 不带引号的字符串
            // 条件：不满足以上所有条件
            // 返回：token 字符串，表示算子或算符
            //      设置字符串类型为算子
            /*
            LPTSTR pStr;
            LPSTR pTemp;

            pTemp = m_pszOrgExpression;
            while (IsDelim(this.m_strOrgExpression[nStart]) == false)
            {
                nLen++;
                pTemp++;
            }
             * */
            int nLen = 0;

            for (int i = this.m_nOffs; i < this.m_strOrgExpression.Length; i++)
            {
                if (IsDelim(this.m_strOrgExpression[i]) == true)
                    break;
                nLen++;
            }

            this.m_strToken = this.m_strOrgExpression.Substring(this.m_nOffs, nLen);
            m_nType = GetReserveType(m_strToken);

            this.m_nOffs += nLen;

            return 0;

        }

        class RESERVEENTRY
        {
            public string m_strName;
            public int m_nType;

            public RESERVEENTRY(string strName, int nType)
            {
                m_strName = strName;
                m_nType = nType;
            }

        }


        RESERVEENTRY [] struReserve = new RESERVEENTRY[] {
	        new RESERVEENTRY("and",TYPE_AND),
	new RESERVEENTRY("or",TYPE_OR),
	new RESERVEENTRY("not",TYPE_NOT),
	new RESERVEENTRY("near",TYPE_NEAR),
	new RESERVEENTRY("within",TYPE_WITHIN),
	new RESERVEENTRY("",-1),
        };

        // 查找保留字表，得到保留字的整数代号
        // return:
        //		-1	not found
        //		其它	保留字的整数代号
        int GetReserveType(string strName)
        {
            int i;
            Debug.Assert(strName != "", "");

            for (i = 0; ; i++)
            {
                if (struReserve[i].m_strName == "")
                    break;
                if (String.Compare(strName, struReserve[i].m_strName, true) == 0)
                    return struReserve[i].m_nType;
            }

            if (String.IsNullOrEmpty(strName) == true)
                return -1;
            else
                return TYPE_OPERAND;
        }


        //	处理一个Token(CString型)
        //	返回值：正确且未结束：0;     出错：-1;    结束：1。
        //	表达式读完后，一定要送一次End 如："#"
        int HandlingAToken()
        {

            //Token为一个左括号
            if (m_nType == TYPE_LEFTBRACKET)
                return DoLeftBracket();

            //Token为一个右括号
            if (m_nType == TYPE_RIGHTBRACKET)
                return DoRightBracket();

            //Token为表达式结束符
            if (m_nType == -1)
                return DoEnd();//vv

            //Token为一个运算因子（变量）
            if (m_nType == TYPE_OPERAND)
                return PutTokenToArray(m_strToken, m_nType);

            //Token为一个运算符
            return DoOperator(m_strToken, m_nType);
        }

        // 处理左括号
        // parameters:
        int DoLeftBracket()
        {
            //	运算符栈中有无运算符？
            if (m_OperatorArray.Count == 0)
            {
                //	无，则先送入"("以示进入新一层表达式
               PutOperatorToArray("(",
                    TYPE_LEFTBRACKET);	//	先送入"("
            }

            PutOperatorToArray("(",
                TYPE_LEFTBRACKET);//	再送入运算符

            return 0;
        }



        // 处理右括号
        // parameters:
        int DoRightBracket()
        {
            int nRet;
            int index;
            for (; ; )
            {
                //	运算符栈顶是否为"("？
                index = m_OperatorArray.Count - 1;
                if (index == -1)
                    return -1;

                Token token = m_OperatorArray[index]; //	运算符栈顶取出

                if (token.m_strToken == "(")
                {
                    m_OperatorArray.RemoveAt(index);//	删除这个运算符栈顶元素
                    break;
                }

                nRet = PutTokenToArray(token.m_strToken,
                    token.m_nType);//	再送入运算符入PL
                if (nRet == -1)
                    return -1;//只要不删数组项，就不必担心delete 问题

                m_OperatorArray.RemoveAt(index);//	删除这个运算符栈顶元素
            }

            return 0;
        }

        // 处理结束符
        // parameters:
        int DoEnd()
        {
            int nRet;
            int index;
            for (; ; )
            {
                //	运算符栈顶是否为"("？

                index = m_OperatorArray.Count - 1;

                if (index == -1)
                {
                    return -1;
                }
                Token token = m_OperatorArray[index]; //	运算符栈顶取出

                if (token.m_strToken == "(")
                {
                    m_OperatorArray.RemoveAt(index);//	删除这个运算符栈顶元素
                    break;
                }

                nRet = PutTokenToArray(token.m_strToken,
                    token.m_nType);//	再送入运算符入PL
                if (nRet == -1)
                    return -1;//此时不必delete token ,因为Array未删除，不必担心

                m_OperatorArray.RemoveAt(index);//	删除这个运算符栈顶元素
            }

            index = m_OperatorArray.Count;
            if (index != -1)
                return -1;

            return 1;  //	返回1，表示结束。
        }

        // 将token加入m_PolandArray
        // parameters:
        int PutTokenToArray(string strToken,
            int nType)
        {
            Token token = null;

            token = new Token();
            m_PolandArray.Add(token);

            token.m_strToken = strToken;
            token.m_nType = nType;

            if (m_OperatorArray.Count == 0)
            {
                //	无，则先送入"("以示进入新一层表达式
                PutOperatorToArray("(",
                    TYPE_LEFTBRACKET);	//	先送入"("
            }

            return 0;
        }


        // 处理operator
        // parameters:
        int DoOperator(string strToken,
            int nType)
        {
            int nRet = 0;

            //	运算符栈中有无运算符？
            if (m_OperatorArray.Count == 0)
            {
                //	无，则先送入"("以示进入新一层表达式
                PutOperatorToArray("(",
                    TYPE_LEFTBRACKET);	//	先送入"("

                PutOperatorToArray(strToken, nType); //	再送入运算符
            }
            else	//	若有(设为OP1)，则判运算符strToken的优先级别：
            //  若YX(op1)大于等于YX(strToken)则先送OP1入PL（PolandArray)
            {
                int index = m_OperatorArray.Count - 1;
                if (index == -1)
                    return -1;

                Token token = m_OperatorArray[index]; //	运算符栈顶取出
                if (token.m_strToken != "(")
                {

                    if (Precedence(token.m_strToken) >= Precedence(strToken))
                    {
                        //	>=时，先送OP1入PL，再送strToken入OP(OperatorArray)
                        nRet = PutTokenToArray(token.m_strToken, token.m_nType);
                        //	运算符栈顶入PL
                        if (nRet == -1)
                            return -1;//此时不必delete token ,因为Array未删除，不必担心

                        m_OperatorArray.RemoveAt(index);//	删除这个运算符栈顶元素
                    }
                }

                PutOperatorToArray(strToken, nType); //	送入运算符
            }

            return 0;
        }

        class PRETABLE
        {
		    public string strPrec;
		    public int Precedence;

            public PRETABLE(string strPrec,
                int nPrecedence)
            {
                this.strPrec = strPrec;
                this.Precedence = nPrecedence;
            }
	    }

        public const int HUO_PRECLASS   = 1;
        public const int YU_PRECLASS	= 2;
        public const int FEI_PRECLASS	= 3;
        public const int NEAR_PRECLASS  = 1;
        public const int WITHIN_PRECLASS = 1;


	    PRETABLE [] table = 
            new PRETABLE[]
        {
		    new PRETABLE("or",HUO_PRECLASS),
		    new PRETABLE("and",YU_PRECLASS),
		    new PRETABLE("!",FEI_PRECLASS),
		    new PRETABLE("near",NEAR_PRECLASS),
		    new PRETABLE("within",WITHIN_PRECLASS),
		    new PRETABLE("",0),
	    };

        // 获得运算符的优先级数
        // parameters:
        // return:
        //      -1  not found
        //      其他
        int Precedence(string strToken)
        {
            Debug.Assert(String.IsNullOrEmpty(strToken) == false, "");

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i].strPrec == strToken)
                {
                    return table[i].Precedence;
                }
            }

            return -1;  // 未找到
        }

        // 将运算符送入m_OperatorArray
        // parameters:
        void PutOperatorToArray(string strToken,
            int nType)
        {
            Token token = null;

            token = new Token();
            token.m_strToken = strToken;
            token.m_nType = nType;

            m_OperatorArray.Add(token);
        }

        void ReleasePoland()
        {

            m_PolandArray.Clear();

            m_OperatorArray.Clear();

        }

        // 将逆波兰表达式转换为一棵树
        // parameters:
        // return:
        //		NULL
        //		其他
        BerNode ChangeRPNToTree()
        {
            BerNode param = null;
            int nRet;
            Token token = null;

            for (int i = 0; i < this.m_PolandArray.Count; i++)
            {
                token = m_PolandArray[i];
                Debug.Assert(token != null, "");

                if (token.m_nType == TYPE_OPERAND)
                {
                    nRet = HandleOperand(token.m_strToken);
                    if (nRet == -1)
                    {
                        throw new Exception("HandleOperand Fail!");
                        return null;
                    }
                }
                else
                {
                    nRet = HandleOperator(token.m_nType,
                        token.m_strToken);
                    if (nRet == -1)
                        return null;
                }
            }

            PopFromArray(out param);
            /*
            DeleteFile("polandtree.txt");
            pParam->DumpToFile("polandtree.txt");
            */

            // 如果pParam->m_ChildArray.GetSize() > 1
            // 表示不能为21，必须在21下面有一个1
            m_Subroot.AddSubtree(param);

            return param;
        }

        // 处理算子
        // parameters:
        // return:
        //		NULL
        //		其他
        int HandleOperand(string strToken)
        {
            BerNode param = null;
            int nRet = 0;

            param = new BerNode();

            param.m_cClass = ASN1_CONTEXT;
            param.m_cForm = ASN1_CONSTRUCTED;
            param.m_uTag = 0;
            param.m_strDebugInfo = "operand [" + strToken + "]";

            nRet = BuildOperand(param, strToken);
            if (nRet == -1)
            {
                throw new Exception("BuildOperand fail!");
                return -1;
            }

            PushToArray(param);

            return 0;
        }

        // 构造算子子树
        // parameters:
        int BuildOperand(BerNode param,
            string strToken)
        {

            // int nRet;
            /*	// 暂时注释掉
            int nSearches;

            nSearches = g_ptrResultName.GetSize();
            //  判断operand是否为ResultSetName
            for(i=0; i<nSearches; i++) {
                if(strcmp((char *)g_ptrResultName[i], strToken)==0)
                {
                    nRet = BldResultSetId(pParam,strToken);
                    return nRet;
                }
            }
            */

            return BldAttributesPlusTerm(param, strToken);
        }

        // 当算子为ResultSetId时
        // parameters:
        int BldResultSetId(BerNode param,
            string strToken)
        {
            param.NewChildCharNode(BerTree.z3950_ResultSetId,
                ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(strToken));

            return 0;
        }

        // 当算子为AttributesPlusTerm时
        // parameters:
        // return:
        //		NULL
        //		其他
        int BldAttributesPlusTerm(BerNode param,
            string strToken)
        {
            BerNode subparam = null;
            // string strQuery = "";
            int three = 3;
            string strTerm = "";
            string strAttrType = "";
            string strAttrValue = "";

            param = param.NewChildConstructedNode(BerTree.z3950_AttributesPlusTerm,
                ASN1_CONTEXT);
            subparam = param.NewChildConstructedNode(
                BerTree.z3950_AttributeList,
                ASN1_CONTEXT);

            DivideToken(strToken,
                out strTerm,
                out strAttrType,
                out strAttrValue);

            // 缺省值
            if (strAttrType == "")
                strAttrType = "1";
            if (strAttrValue == "")
                strAttrValue = "4";
            /*
            strMessage.Format("term[%s] attrtype[%s] attrvalue[%s]",
                strTerm,
                strAttrType,
                strAttrValue);
            */
            try
            {
                HandleQuery(param,
                    subparam,
                    strTerm,
                    strAttrType,
                    strAttrValue);
            }
            catch(Exception ex)
            {
                throw new Exception("BldAttributesPlusTerm() 处理 token '" + strToken + "' 过程中出现异常", ex);
            }

            if (strToken.IndexOf('/', 0) == -1)
            {
                BerNode seq = null;
                seq = subparam.NewChildConstructedNode(
                    ASN1_SEQUENCE,
                    ASN1_UNIVERSAL);
                // TRACE("pSeq->m_uTag=%d",pSeq->m_uTag);
                seq.NewChildIntegerNode(BerTree.z3950_AttributeType,
                    ASN1_CONTEXT,
                    BitConverter.GetBytes((Int16)three));  /* position */
                // 一样？
                seq.NewChildIntegerNode(BerTree.z3950_AttributeValue,
                    ASN1_CONTEXT,
                    BitConverter.GetBytes((Int16)three));  /* position */
            }

            return 0;
        }

#if NOOOOOOOOOOOOO
// 处理检索词得到一个term或AttributesList
// parameters:
//		strQuery	[out]
int DivideToken(LPSTR &pszQuery,
						 CString &strQuery)
{
	BOOL bInQuote = FALSE;
	_TCHAR ch;

	strQuery.Empty();
	
	
	while(IsWhite(*pszQuery)) //去掉空格、\t、\r、\n
		++(pszQuery);


// **1 文件结尾：
// 条件：遇到0字符
	if (*pszQuery == 0) {   //	如果遇到结尾。
		strQuery = "";
		return 0;
	}

// **3 '/'或'='
// 条件：遇到"/="之一
	/*
	if (strchr(" /=",*pszQuery))  { 
		strQuery = *pszQuery++;
	}
	*/
	bInQuote = FALSE;
	while(1) {
		ch = *pszQuery;
		if (ch == '\"') {
			if (bInQuote == TRUE)
				bInQuote = FALSE;
			else
				bInQuote = TRUE;
			pszQuery ++;
			continue;
		}
		if (ch == ' ' && bInQuote == FALSE)
			break;
		if (ch == '/' || ch == '=')
			break;
		pszQuery ++;
	}
	strQuery = *pszQuery ++;
	return 0;
			
// **5 term
// 条件：不满足以上所有条件
	LPTSTR pStr;
	LPSTR  pTemp;
	int nLen = 0;
	
	/*
	pTemp = pszQuery;
	while ((strchr(" /=",*pTemp) ==NULL) 
			&& (*pTemp!=0))
	{
//		printf("pTemp=%c\n",*pTemp);
		nLen++;
		pTemp++;
	}
	*/
	pStr = strQuery.GetBuffer(nLen + 1);
	memmove(pStr,pszQuery,nLen);

	strQuery.ReleaseBuffer(nLen);

	pszQuery = pTemp;

	return 0;

}
#endif 

        // new version
        // 将连续的字符串内容剖析分解为3个部分
        // 例如:	中国/1=4
        //			或者 "中国"/1=4
        // parameters:
        //		strToken	待剖析的字符串
        //		strTerm		[out]检索词
        //		strAttrType	[out]属性类型
        //		strAttrValue [out]属性值
        int DivideToken(string strToken,
                    out string strTerm,
                    out string strAttrType,
                    out string strAttrValue)
        {
            strTerm = "";
            strAttrType = "";
            strAttrValue = "";

            bool bInQuote;
            int nLen;
            //LPTSTR pszQuery;
            char ch;
            int nRet;

            int nOffs = 0;

            // 查找term末尾
            bInQuote = false;
            // pszQuery = (LPTSTR)(LPCTSTR)strToken;
            while (nOffs < strToken.Length)
            {
                ch = strToken[nOffs];
                if (ch == '\"')
                {
                    if (bInQuote == true)
                        bInQuote = false;
                    else
                        bInQuote = true;
                    nOffs++;
                    continue;
                }
                if (ch == ' ' && bInQuote == false)
                    break;
                if (ch == '/' || ch == '=')
                    break;
                nOffs++;
            }

            nLen = nOffs;
            strTerm = strToken.Substring(0, nLen);
            UnQuoteString(ref strTerm);
            strTerm = StringUtil.UnescapeString(strTerm);   // 2015/10/21

            if (nLen >= strToken.Length)
                return 0;   // 2006/11/2 add

            nRet = strToken.IndexOf("=", nLen + 1);
            if (nRet == -1)
                return 0;

            strAttrType = strToken.Substring(nLen + 1, nRet - (nLen + 1));
            strAttrValue = strToken.Substring(nRet + 1);
            return 0;
        }

        // 将常量字符串外面的"去除
        // return:
        //		TRUE	成功
        //		FALSE	不是常量字符串("没有或者不配对)
        bool UnQuoteString(ref string strString)
        {
            if (strString.Length < 2)
                return false;	// 如果长度小于2字符(不是byte!)，则直接返回

            char first = (char)0;
            if (strString[0] == '\"')
            {
                first = strString[0];
            }
            else
                return false;

            if (strString[strString.Length - 1] != first)
                return false;

            strString = strString.Substring(1);
            strString = strString.Substring(0, strString.Length - 1);
            return true;
        }

        /*
发生未捕获的界面线程异常: 
Type: System.FormatException
Message: 輸入字串格式不正確。
Stack:
於 System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer& number, NumberFormatInfo info, Boolean parseDecimal)
於 System.Number.ParseInt32(String s, NumberStyles style, NumberFormatInfo info)
於 System.Int16.Parse(String s, NumberStyles style, NumberFormatInfo info)
於 System.Convert.ToInt16(String value)
於 DigitalPlatform.Z3950.PolandNode.HandleQuery(BerNode param, BerNode subparam, String strTerm, String strAttrType, String strAttrValue)
於 DigitalPlatform.Z3950.PolandNode.BldAttributesPlusTerm(BerNode param, String strToken)
於 DigitalPlatform.Z3950.PolandNode.BuildOperand(BerNode param, String strToken)
於 DigitalPlatform.Z3950.PolandNode.HandleOperand(String strToken)
於 DigitalPlatform.Z3950.PolandNode.ChangeRPNToTree()
於 DigitalPlatform.Z3950.PolandNode.ChangeOrgToRPN()
於 DigitalPlatform.Z3950.BerTree.make_type_1(String strQuery, Encoding queryTermEncoding, BerNode subroot)
於 DigitalPlatform.Z3950.BerTree.SearchRequest(SEARCH_REQUEST struSearch_request, Byte[]& baPackage)
於 dp2Catalog.ZConnection.DoSearchAsync()
於 dp2Catalog.ZConnection.ZConnection_InitialComplete(Object sender, EventArgs e)
於 dp2Catalog.ZConnection.BeginCommands(List`1 commands)
於 dp2Catalog.ZSearchForm.DoSearchOneServer(TreeNode nodeServerOrDatabase, String& strError)
於 dp2Catalog.ZSearchForm.DoSearch()
於 dp2Catalog.ZSearchForm.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.SplitContainer.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.Control.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.SplitContainer.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.Control.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.SplitContainer.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.Control.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.Control.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.TextBoxBase.ProcessDialogKey(Keys keyData)
於 System.Windows.Forms.Control.PreProcessMessage(Message& msg)
於 System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message& msg)
於 System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG& msg)

         * */
        // new version
        // 处理term或AttributesList
        // parameters:
        void HandleQuery(BerNode param,
            BerNode subparam,
            string strTerm,
            string strAttrType,
            string strAttrValue)
        {
            BerNode seq = null;

            seq = subparam.NewChildConstructedNode(
                ASN1_SEQUENCE,
                ASN1_UNIVERSAL);

            // 处理term、attributeType或attributeValue

            //    处理attributeType
            try
            {
                Int16 i = Convert.ToInt16(strAttrType);

                seq.NewChildIntegerNode(BerTree.z3950_AttributeType,
                    ASN1_CONTEXT,
                    BitConverter.GetBytes(i));
            }
            catch(Exception ex)
            {
                throw new Exception("strAttrType = '"+strAttrType+"' 应为数字。", ex);
            }

            //		处理attributeValue 
            try
            {
                Int16 i = Convert.ToInt16(strAttrValue);
                seq.NewChildIntegerNode(BerTree.z3950_AttributeValue,
                    ASN1_CONTEXT,
                    BitConverter.GetBytes(i));
            }
            catch (Exception ex)
            {
                throw new Exception("strAttrValue = '" + strAttrValue + "' 应为数字。", ex);
            }
            // TODO: 为何这里被调用了两次?

            // term
            {
                BerNode tempnode = param.NewChildCharNode(BerTree.z3950_Term,		//	处理term
                    ASN1_CONTEXT,
                    //Encoding.GetEncoding(936).GetBytes(strTerm));
                    this.m_queryTermEncoding.GetBytes(strTerm));
                tempnode.m_strDebugInfo = "term [" + strTerm + "]";
            }

            // return 0;
        }

        int PushToArray(BerNode param)
        {
            this.m_StackArray.Add(param);
            return 0;
        }

        int PopFromArray(out BerNode subparam)
        {
            subparam = null;

            if (m_StackArray.Count == 0)
            {
                Debug.Assert(false, "堆栈已空");
                return -1;
            }

            subparam = m_StackArray[m_StackArray.Count - 1];
            m_StackArray.RemoveAt(m_StackArray.Count - 1);

            return 0;
        }

        // 处理operator
        // parameters:
        // return:
        //		NULL
        //		其他
        int HandleOperator(int nType,
            string strToken)
        {
            BerNode param = null;
            BerNode subparam = null;
            int nRet;
            int i;

            param = new BerNode();

            param.m_cClass = ASN1_CONTEXT;
            param.m_cForm = ASN1_CONSTRUCTED;
            param.m_uTag = 1;
            param.m_strDebugInfo = "operator [" + strToken + "]";

            // 从堆栈中弹出两个操作数对象
            for (i = 0; i < 2; i++)
            {
                nRet = PopFromArray(out subparam);
                if (nRet == -1)
                    return -1;
                param.AddSubtree(subparam);
            }

            // 创建op
            BldOperator(param,
                nType,
                strToken);

            // 结果又入栈
            PushToArray(param);

            return 0;
        }


        // 构造operator子树
        // parameters:
        int BldOperator(BerNode param,
                                 int nType,
                                 string strToken)
        {
            BerNode subparam = null;

            // pSubparam = pParam->NewChildconstructedNode(1,ASN1_CONTEXT);
            // 此层似乎为多余的，试试此语句注释掉以后的效果
            // 2000/11/26 changed


            subparam = param/*pSubparam*/.NewChildConstructedNode(BerTree.z3950_Operator,
                ASN1_CONTEXT);

            if (nType == TYPE_WITHIN)
                BuildWithin(subparam, strToken);
            else if (nType == TYPE_NEAR)
                BuildNear(subparam, strToken);
            else
                BuildGeneral(subparam, strToken, nType);

            return 0;

        }


        // 构造within运算符的子树
        // parameters:
        // return:
        //		NULL
        //		其他
        int BuildWithin(BerNode param,
            string strToken)
        {
            Debug.Assert(false, "尚未实现");
            /*
            BerNode seq = null;
            BerNode subparam = null;
            int distance = 1 , unit = 2;
            char *ind;
            int zero=0, one=1, two=2, three=3, four=4, five=5;
	
            if ((ind = (char *)strstr(strToken,"/")) != NULL)
            {
                sscanf(ind+1,"%d", &distance);
                if ((ind = strstr(ind+1, "/"))!=NULL)
                    sscanf(ind+1,"%d", &unit);
            }
	
            pSeq = pParam->NewChildconstructedNode(z3950_prox,
                ASN1_CONTEXT);
	
            pSeq->NewChildintegerNode(z3950_exclusion, ASN1_CONTEXT,
                (CHAR*)&zero,sizeof(zero));   // 不排除 
	
            pSeq->NewChildintegerNode(z3950_distance, ASN1_CONTEXT,
                (CHAR*)&distance,sizeof(distance));   
	
            pSeq->NewChildintegerNode(z3950_ordered, ASN1_CONTEXT,
                (CHAR*)&one,sizeof(one));   // ordered 

            pSeq->NewChildintegerNode(z3950_relationType, 
                ASN1_CONTEXT,(CHAR*)&two,
                sizeof(two));   // z3950_lessThanOrEqual 
	
            pSubparam = pSeq->NewChildconstructedNode(
                z3950_proximityUnitCode,
                ASN1_CONTEXT);
	
            pSeq->NewChildintegerNode(z3950_known, ASN1_CONTEXT,
                (CHAR*)&unit,sizeof(unit));  

            */

            return 0;

        }


        // 构造near运算符的子树
        // parameters:
        // return:
        //		NULL
        //		其他
        int BuildNear(BerNode param,
            string strToken)
        {
            Debug.Assert(false, "尚未实现");
            /*
            CBERNode *pSeq;
            CBERNode *pSubparam;
            int distance = 1, unit = 2;
            char *ind;
            int zero=0, two=2;
            //int one=1, three=3, four=4, five=5;


            if ((ind = (char *)strstr(strToken,"/"))!=NULL)
            {
                sscanf(ind+1,"%d", &distance);
                if ((ind = strstr(ind+1, "/"))!=NULL)
                    sscanf(ind+1,"%d", &unit);
            }
	
            pSeq = pParam->NewChildconstructedNode(z3950_prox,
                ASN1_CONTEXT);

            pSeq->NewChildintegerNode(z3950_exclusion, 
                ASN1_CONTEXT,
                (CHAR*)&zero,sizeof(zero));   // 不排除 

            pSeq->NewChildintegerNode(z3950_distance, 
                ASN1_CONTEXT,
                (CHAR*)&distance,sizeof(distance)); 
	
            pSeq->NewChildintegerNode(z3950_ordered, 
                ASN1_CONTEXT,
                (CHAR*)&zero,sizeof(zero));   // not ordered 

            pSeq->NewChildintegerNode(z3950_relationType, 
                ASN1_CONTEXT,
                (CHAR*)&two,sizeof(two));   // z3950_lessThanOrEqual 
	
            pSubparam = pSeq->NewChildconstructedNode(
                z3950_proximityUnitCode,
                ASN1_CONTEXT);
            pSeq->NewChildintegerNode(z3950_known, 
                ASN1_CONTEXT,
                (CHAR*)&unit,sizeof(unit)); 
            */
            return 0;

        }


        // 构造非near、within运算符的子树
        // parameters:
        int BuildGeneral(BerNode param,
            string strToken,
            int nType)
        {
            Debug.Assert(nType <= 0xffff, "");  // 避免转换为unsigned short int时候出问题

            /*
            param.NewChildCharNode((ushort)nType,
                ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(strToken)); // ,0);?????
            */
            // 2020/8/20
            param.NewChildCharNode((ushort)nType,
    ASN1_CONTEXT,
    new byte[0]);

            return 0;
        }




    }

    public class Token
    {
        public string m_strToken = "";
        public int m_nType = 0;
    }

    public class TokenCollection : List<Token>
    {
    }

    public class BerNodeStack : List<BerNode>
    {

    }
}
