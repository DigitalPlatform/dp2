using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是 GCAT 相关代码
    /// </summary>
    public partial class LibraryApplication
    {
        static string[] hanzi_number = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二" };
        static string[] luoma_number_upper = new string[] { "Ⅰ", "Ⅱ", "Ⅲ", "Ⅳ", "Ⅴ", "Ⅵ", "Ⅶ", "Ⅷ", "Ⅸ", "Ⅹ", "Ⅺ", "Ⅻ" };
        static string[] luoma_number_lower = new string[] { "ⅰ", "ⅱ", "ⅲ", "ⅳ", "ⅴ", "ⅵ", "ⅶ", "ⅷ", "ⅸ", "ⅹ" };

        static string stopword = " …。，、；：？！…—·ˉˇ¨‘’“”～＂＇｀〃〔〕〈〉《》「」『』〖〗【】（）［］｛｝	.,;:?!'\"`~@#$%^&*()_+-=|\\<>{}[]";

        static string[] fufen_2_shuang = new string[] { "A-P", "Q-Z" };
        static string[] fufen_2_dan = new string[] { "aegim", "noruv" };

        static string[] fufen_3_shuang = new string[] { "A-J", "K-T", "W-Z" };
        static string[] fufen_3_dan = new string[] { "aeg", "im", "noruv" };

        static string[] fufen_5_shuang = new string[] { "A-G", "H-L", "M-R", "S-X", "Y-Z" };
        static string[] fufen_5_dan = new string[] { "ae", "g", "im", "n", "oruv" };

        static string[] fufen_10_shuang = new string[] { "A-E", "F-G", "H-J", "K-L", "M-P", "Q-R", "S-T", "W-X", "Y", "Z" };
        static string[] fufen_10_dan = new string[] { "a", "e", "g", "i", "m", "n", "o", "r", "u", "v" };

        public string PinyinDbName { get; set; }
        public string GcatDbName { get; set; }
        public string WordDbName { get; set; }

        // 汉字名 锁。用于控制创建汉字条目的过程
        public RecordLockCollection hanzi_locks = new RecordLockCollection();

        // return:
        //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
        //		-3	需要回答问题
        //      -1  出错
        //      0   成功
        public int GetNumberInternal(
            SessionInfo sessioninfo,
            ref int nStep,
            string strAuthorParam,
            bool bSelectPinyin,
            bool bSelectEntry,
            bool bOutputDebugInfo,
            ref List<Question> questions,
            out string strNumber,
            out StringBuilder debug_info,
            out string strError)
        {
            strNumber = "";
            strError = "";
            debug_info = new StringBuilder();

            Debug.Assert(questions != null, "");

            if (strAuthorParam == "")
            {
                strError = "著者字符串不能为空";
                return -1;
            }

            if (string.IsNullOrEmpty(this.GcatDbName))
            {
                strError = "当前尚未配置 著者号码 库名";
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strAuthor = ConvertNumberToHanzi(strAuthorParam);

            if (strAuthor == "")
            {
                strError = "著者字符串　'" + strAuthorParam + "' 经加工后为空，无法取著者号";
                return -1;
            }

            if (bOutputDebugInfo == true)
            {
                debug_info.Append("著者字符串 '" + strAuthorParam + "' 经去除非用字、将阿拉伯和罗马数字转换为汉字数字后，为 '" + strAuthor + "'。\r\n");
            }

            string strComfirmPinyin = "";	// 用于在多选中进行确认的拼音

            REDOSEARCH:
            List<string> parts = new List<string>();
            {
                string strPart = strAuthor;
                while (string.IsNullOrEmpty(strPart) == false)
                {
                    parts.Add(strPart);
                    strPart = strPart.Substring(0, strPart.Length - 1);
                }
            }

            string strPath = "";

            long lRet = 0;
            string strMatched = "";
            foreach (string strPart in parts)
            {
                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("以字符串 '" + strPart + "' 对著者号码库 '" + this.GcatDbName + "' 进行试探性检索，");
                }

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.GcatDbName)   // 2007/9/14 new add
                    + ":" + "汉字'><item><word>"
                    + StringUtil.GetXmlStringSimple(strPart)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";

                lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索库 " + this.GcatDbName + " 时出错: " + strError;
                    return -1;
                }
                if (lRet == 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("结果未命中。\r\n");
                    }
                    // goto CONTINUE;	// not found
                    continue;
                }

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("结果命中 " + Convert.ToString(lRet) + " 条。\r\n");
                }

                lRet = channel.DoGetSearchResult(
                    "default",
                    1000,
                    "zh",
                    null,	// stop,
                    out List<string> aPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索著者号码库获取检索结果时出错: " + strError;
                    return -1;
                }

                if (aPath.Count == 0)
                {
                    strError = "检索著者号码库获取的检索结果为空(但是对keys检索时是有结果的)";
                    return -1;
                }

                int nIndex = 0;

                if (aPath.Count > 1 && strComfirmPinyin != "")
                {
                    strPath = SelectOneRecByPinyin(
                        channel,
                        aPath,
                        strComfirmPinyin,
                        out strError);
                    if (strPath == null)
                        return -1;
                    if (strPath != "")
                    {
                        goto ALREADYSELECTED;
                    }
                    else
                    {
                        if (bOutputDebugInfo == true)
                        {
                            string strPaths = "";
                            for (int k = 0; k < aPath.Count; k++)
                            {
                                strPaths += (string)aPath[k] + ";";
                            }
                            debug_info.Append("警告：用于确认多选的拼音 '" + strComfirmPinyin + "' 在命中记录集 '" + strPaths + "' 中没有匹配上。\r\n");
                        }
                    }
                }

                // 如果命中多个记录
                if (aPath.Count > 1 && bSelectEntry == true)
                {
                    Question q = GetQuestion(questions, nStep);
                    if (q == null)
                    {
                        string strNameList = BuildNameList(
                            channel,
                            aPath,
                            out strError);
                        if (strNameList == null)
                            return -1;
                        string strAskText = "名称 '" + strPart + "' 存在多个条目: \r\n---\r\n"
                            + strNameList
                            + "---\r\n\r\n请选择一个。(输入序号，从1开始计数)";
                        q = NewQuestion(questions,
                            nStep,
                            strAskText,
                            "");
                        Debug.Assert(q != null, "");
                        strError = "请回答问题，以便为 '" + strAuthor + "' 确定适当的号码表条目。";
                        return -3;
                    }

                    try
                    {
                        nIndex = Convert.ToInt32(q.Answer);
                    }
                    catch
                    {
                        strError = "答案 '" + q.Answer + "' 格式不正确";
                        q.Answer = "";
                        return -3;
                    }

                    nIndex--;	// 因为答案习惯为从1开始计数

                    if (nIndex < 0 || nIndex >= aPath.Count)
                    {
                        strError = "答案 '" + q.Answer + "' 格式不正确, 值应在1和" + Convert.ToString(aPath.Count) + "之间。";
                        q.Answer = "";
                        return -3;
                    }

                    nStep += 1;
                }

                // SELECTFIRST:
                //strRecID = ResPath.GetRecordId((string)aPath[0]);
                strPath = (string)aPath[nIndex];

                ALREADYSELECTED:

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("获得记录路径 '" + strPath + "' 。\r\n");
                }
                strMatched = strPart;
                break;

#if NO
            CONTINUE:
                if (strPart.Length == 0)
                {
                    strError = "著者 '" + strAuthor + "' 的整体或局部均未检索命中";
                    return -1;
                }
                strPart = strPart.Substring(0, strPart.Length - 1);
#endif
            }

            if (string.IsNullOrEmpty(strMatched) == true)
            {
                strError = "著者 '" + strAuthor + "' 的整体或局部均未检索命中";
                return -4;  // return -1;
            }

            string strXing = strMatched;	// 姓
            string strMing = strAuthor.Substring(strMatched.Length);	// 名

            if (bOutputDebugInfo == true)
            {
                debug_info.Append("把字符串 '" + strAuthor + "' 切割为姓 '" + strXing + "' 和名 '" + strMing + "' 两部分。\r\n");
            }

            // 取记录
            string strStyle = "content,data";


            lRet = channel.GetRes(strPath,
                strStyle,
                out string strXml,
                out string strMetaData,
                out byte[] baTimeStamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                return -1;
            }

            if (bOutputDebugInfo == true)
            {
                debug_info.Append("取出著者号码表记录 '" + strPath + "' 内容为:\r\n---\r\n" + strXml + "\r\n---\r\n");
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载xml记录时出错: " + ex.Message;
                return -1;
            }

            int nRet = 0;

            // 看看是不是参见情形。
            string strRef = dom.DocumentElement.GetAttribute("r");
            if (strRef != "")
            {
                nRet = strRef.IndexOf(",");
                if (nRet != -1)
                {
                    strAuthor = strRef.Substring(0, nRet).Trim();
                    strComfirmPinyin = strRef.Substring(nRet + 1).Trim();	// 用于确认多选的拼音
                }
                else
                {
                    strAuthor = strRef;
                    strComfirmPinyin = "";
                }
                goto REDOSEARCH;
            }

            string strTempDebugInfo = "";

            if (strMing.Length > 0)	// 有“名”的情况
            {
                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("按照有'名'情况处理。\r\n");
                }

                string strFirst = strMing.Substring(0, 1);

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("取出名中第一字符 '" + strFirst + "'，\r\n");
                }


                // return:
                //		-1	出错
                //		0	没有找到
                //		1	找到
                nRet = GetPinyin(
                    ref questions,
                    ref nStep,
                    strAuthor,
                    channel,
                    strFirst,
                    bSelectPinyin,
                    out string strPinyin,
                    out strError);
                if (nRet == -3)
                    return -3;
                if (nRet == -1)
                {
                    strError = "获取汉字 '" + strFirst + "' 的拼音时出错: " + strError;
                    return -1;
                }
                if (nRet == 0)
                {
                    strError = "获取汉字 '" + strFirst + "' 的拼音时没有找到: " + strError;
                    return -1;
                }

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("获得其拼音为 '" + strPinyin + "'。\r\n");
                }


                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("对著者号码XML记录进行查找，");
                }

                strTempDebugInfo = "";

                // 根据首字母查找范围属性
                // parameters:
                //		strPinyin	一个汉字的拼音
                // return:
                //		-1	出错
                //		0	没有找到
                //		1	找到
                nRet = GetSubRange(dom,
                    strPinyin.ToUpper(),
                    bOutputDebugInfo,
                    out string strValue,
                    out string strFufen,
                    out strTempDebugInfo,
                    out strError);

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("查找的详细过程如下:\r\n---\r\n" + strTempDebugInfo + "---\r\n");
                }

                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    return -1;
                }

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("获得值为 '" + strValue + "' 复分表类型为 '" + strFufen + "'。\r\n");
                }

                if (strFufen != "" && strFufen != "0")
                {
                    // 有复分表

                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("发现有复分表。\r\n");
                    }

                    string strSecond = "";
                    string strSecondPinyin = "";

                    // 看单名还是双名决定是否取名第二字的拼音
                    if (strMing.Length >= 2)
                    {
                        strSecond = strMing.Substring(1, 1);

                        if (bOutputDebugInfo == true)
                        {
                            debug_info.Append("双名要取出名的第二字符 '" + strSecond + "'，\r\n");
                        }

                        // return:
                        //		-1	出错
                        //		0	没有找到
                        //		1	找到
                        nRet = GetPinyin(
                            ref questions,
                            ref nStep,
                            strAuthor,
                            channel,
                            strSecond,
                            bSelectPinyin,
                            out strSecondPinyin,
                            out strError);
                        if (nRet == -3)
                            return -3;
                        if (nRet == -1)
                        {
                            strError = "获取汉字 '" + strSecond + "' 的拼音时出错: " + strError;
                            return -1;
                        }
                        if (nRet == 0)
                        {
                            strError = "获取汉字 '" + strSecond + "' 的拼音时没有找到: " + strError;
                            return -1;
                        }

                        if (bOutputDebugInfo == true)
                        {
                            debug_info.Append("获得其拼音为 '" + strSecondPinyin + "'。\r\n");
                        }

                    }
                    else
                    {

                        Debug.Assert(strMing.Length == 1, ""); // 单名

                        if (bOutputDebugInfo == true)
                        {
                            debug_info.Append("单名情形。\r\n");
                        }
                        // 单名，而且第一字拼音没有尾母。
                        if (strPinyin.Length == 1)
                        {
                            if (bOutputDebugInfo == true)
                            {
                                debug_info.Append("单名，而且第一字拼音没有尾母。\r\n");
                                debug_info.Append("因此值字符串 '" + strValue + "' 就是最终著者号。\r\n");
                            }

                            strNumber = strValue;
                            return 0;
                        }
                    }

                    strTempDebugInfo = "";


                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("用名之第一音　'" + strPinyin + "' 和　第二音　'" + strSecondPinyin + "' 查复分表 '" + strFufen + "' 。\r\n");
                    }

                    // 查找复分表
                    // return:
                    //		-1	出错
                    //		>=0	正常
                    nRet = SearchFufen(
                        strFufen,
                        strPinyin,
                        strSecondPinyin,
                        bOutputDebugInfo,
                        out strTempDebugInfo,
                        out strError);
                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("查找复分表的详细过程如下:\r\n---\r\n" + strTempDebugInfo + "---\r\n");
                    }
                    if (nRet == -1)
                        return -1;

                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("获得增量值 " + Convert.ToString(nRet) + "。\r\n");
                    }

                    int nDelta = nRet;

                    // 给一个著者号增加一个数量。
                    // 例如 B019 + 1 变成 B020
                    nRet = AddFufen(strValue,
                        nDelta,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (bOutputDebugInfo == true)
                    {
                        debug_info.Append("值字符串 '" + strValue + "' 和　增量值 " + Convert.ToString(nDelta) + " 合成，得到最终著者号 '" + strNumber + "'。\r\n");
                    }

                    return 0;
                }

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("发现无复分表。\r\n");
                    debug_info.Append("因此值字符串 '" + strValue + "' 就是最终著者号。\r\n");
                }

                strNumber = strValue;
                return 0;
            }
            else // 只有姓，没有名的情况
            {
                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("按照无'名'情况处理。\r\n");
                }

                string strValue = "";
                string strFufen = "";

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("查复分表XML记录中缺省范围值，");
                }

                strTempDebugInfo = "";

                // 根据首字母查找范围属性
                // parameters:
                //		strPinyin	一个汉字的拼音
                // return:
                //		-1	出错
                //		0	没有找到
                //		1	找到
                nRet = GetSubRange(dom,
                    "",
                    bOutputDebugInfo,
                    out strValue,
                    out strFufen,
                    out strTempDebugInfo,
                    out strError);
                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("查找的详细过程如下:\r\n---\r\n" + strTempDebugInfo + "---\r\n");
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    return -1;
                }

                if (bOutputDebugInfo == true)
                {
                    debug_info.Append("为 '" + strValue + "'。这就是最终的著者号码值。\r\n");
                }

                strNumber = strValue;
                return 0;
            }
        }

        #region 下级函数

        // 将字符串中的阿拉伯数字、罗马数字转换为汉字数字
        string ConvertNumberToHanzi(string strText)
        {
            string strResult = "";
            int nRet = 0;

            for (int i = 0; i < strText.Length; i++)
            {
                string strOne = strText[i].ToString();

                // 英文字母去除
                if (String.Compare(strOne.ToLower(), "a") >= 0
                    && String.Compare(strOne.ToLower(), "z") <= 0)
                    continue;

                // 非用字
                nRet = stopword.IndexOf(strOne);
                if (nRet != -1)
                    continue;

                // 是不是阿拉伯数字
                if (Char.IsDigit(strText, i) == true)
                {
                    int nIndex = Convert.ToInt32(strOne);

                    Debug.Assert(nIndex >= 0 && nIndex <= 9, "");

                    strResult += hanzi_number[nIndex];
                    continue;
                }

                // 是不是大写罗马数字
                nRet = "ⅠⅡⅢⅣⅤⅥⅦⅧⅨⅩⅪⅫ".IndexOf(strOne);
                if (nRet != -1)
                {
                    nRet++;
                    strResult += hanzi_number[nRet];
                    continue;
                }

                // 是不是小写的罗马数字
                nRet = "ⅰⅱⅲⅳⅴⅵⅶⅷⅸⅹ".IndexOf(strOne);
                if (nRet != -1)
                {
                    nRet++;
                    strResult += hanzi_number[nRet];
                    continue;
                }

                strResult += strOne;
            }

            return strResult;
        }

        // 在多个命中记录中利用预先知道的拼音选择其中一个
        // return:
        //		null	出错
        //		""	没有选中
        //		其他	正确
        string SelectOneRecByPinyin(
            RmsChannel channel,
            List<string> aPath,
            string strComfirmPinyin,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < aPath.Count; i++)
            {
                string strPath = (string)aPath[i];
                // 取记录
                string strStyle = "content,data";

                string strMetaData;
                string strOutputPath;
                string strXml = "";
                byte[] baTimeStamp = null;

                long lRet = channel.GetRes(strPath,
                    strStyle,
                    out strXml,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                    return null;
                }


                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                    return null;
                }

                string strPinyin = dom.DocumentElement.GetAttribute("p");
                if (String.Compare(strPinyin, strComfirmPinyin, true) == 0)
                    return strPath;
            }

            return "";	// 没有选中
        }

        // 创建记录选择列表
        string BuildNameList(
            RmsChannel channel,
            List<string> aPath,
            out string strError)
        {
            strError = "";
            string strResult = "";

            for (int i = 0; i < aPath.Count; i++)
            {
                string strPath = (string)aPath[i];
                // 取记录
                string strStyle = "content,data";

                string strMetaData;
                string strOutputPath;
                string strXml = "";
                byte[] baTimeStamp = null;

                long lRet = channel.GetRes(strPath,
                    strStyle,
                    out strXml,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                    return null;
                }


                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                    return null;
                }

                string strHanzi = dom.DocumentElement.GetAttribute("h");
                string strPinyin = dom.DocumentElement.GetAttribute("p");
                string strComment = dom.DocumentElement.GetAttribute("c");

                if (strComment != "")
                    strComment = " (" + strComment + ")";

                strResult += Convert.ToString(i + 1) + ") " + strHanzi + " " + strPinyin + strComment + "\r\n";

            }

            return strResult;
        }

        // return:
        //		-3	需要回答问题
        //		-1	出错
        //		0	没有找到
        //		1	找到
        int GetPinyin(
            ref List<Question> questions,
            ref int nStep,
            string strAuthor,
            RmsChannel channel,
            string strHanzi,
            bool bSelectPinyin,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(this.PinyinDbName)   // 2007/9/14 new add
                + ":" + "汉字'><item><word>"
                + StringUtil.GetXmlStringSimple(strHanzi)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>zh</lang></target>";

#if NO
            // TODO: 最好是连检索带获取命中的第一条记录
            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
            {
                strError = "检索拼音库时出错: " + strError;
                return -1;
            }
            if (lRet == 0)
                return 0;	// not found

            List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                1,
                "zh",
                null,	// stop,
                out aPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索拼音库获取检索结果时出错: " + strError;
                return -1;
            }

            if (aPath.Count == 0)
            {
                strError = "检索拼音库获取的检索结果为空(但是对keys检索时是有结果的)";
                return -1;
            }

            //strRecID = ResPath.GetRecordId((string)aPath[0]);
            string strPath = (string)aPath[0];

            // 取记录
            string strStyle = "content,data";
            string strMetaData;
            string strOutputPath;
            string strXml = "";
            byte[] baTimeStamp = null;

            lRet = channel.GetRes(strPath,
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "检索 '" + strPath + "' 记录体时出错: " + strError;
                return -1;
            }

#endif

            Record[] records = null;
            long lRet = channel.DoSearchEx(strQueryXml,
                "default",
                "", // strOutputStyle
                1,
                "zh",
                "id,xml",
                out records,
                out strError);
            if (lRet == -1)
            {
                strError = "检索拼音库时出错: " + strError;
                return -1;
            }
            if (lRet == 0)
                return 0;	// not found

            string strXml = records[0].RecordBody.Xml;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载拼音记录xml时出错: " + ex.Message;
                return -1;
            }

            strPinyin = dom.DocumentElement.GetAttribute("p");
            int nRet = strPinyin.IndexOf(";");
            if (nRet != -1)
            {
                if (bSelectPinyin == false) // 强制选多音的第一个
                {
                    strPinyin = strPinyin.Substring(0, nRet).Trim();
                    return 1;
                }
                Question q = GetQuestion(questions, nStep);
                if (q == null)
                {
#if NO
                    string strAskText = "汉字 '" + strHanzi + "' 的拼音如下: \r\n---\r\n"
                        + BuildPinyinList(strPinyin)
                        + "---\r\n\r\n请选择一个。(输入序号，从1开始计数)";
#endif
                    BuildAsk(
    strHanzi,
    strPinyin,
    out string strAskText,
    out string strAskXml);

                    q = NewQuestion(questions,
                        nStep,
                        strAskText,
                        strAskXml);
                    Debug.Assert(q != null, "");
                    strError = "请回答问题，以便为 '" + strAuthor + "' 中的多音字确定读音。";
                    return -3;
                }

                int nIndex = 0;

                try
                {
                    nIndex = Convert.ToInt32(q.Answer);
                }
                catch
                {
                    strError = "答案 '" + q.Answer + "' 格式不正确";
                    q.Answer = "";
                    return -3;
                }

                nIndex--;	// 因为答案习惯为从1开始计数

                string strOnePinyin = "";

                nRet = SelectPinyin(strPinyin,
                    nIndex,
                    out strOnePinyin,
                    out strError);
                if (nRet == -1)
                {
                    strError = "答案 '" + q.Answer + "' 格式不正确: " + strError;
                    q.Answer = "";
                    return -3;
                }

                nStep += 1;
                strPinyin = strOnePinyin.Trim();
            }

            return 1;
        }

        // 构造问题
        static void BuildAsk(
            string strHanzi,
            string strPinyin,
            out string strAskText, 
            out string strAskXml)
        {
            strAskText = "汉字 '" + strHanzi + "' 的拼音如下: \r\n---\r\n"
    + BuildPinyinList(strPinyin)
    + "---\r\n\r\n请选择一个。(输入序号，从1开始计数)";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<question />");
            dom.DocumentElement.AppendChild(dom.CreateTextNode("汉字 '"));

            XmlElement hanzi = dom.CreateElement("hanzi");
            dom.DocumentElement.AppendChild(hanzi);
            hanzi.InnerText = strHanzi;

            dom.DocumentElement.AppendChild(dom.CreateTextNode("' 的拼音如下:\r\n---\r\n"));

            int i = 0;
            foreach (string strOnePinyin in StringUtil.SplitList(strPinyin, ';'))
            {
                dom.DocumentElement.AppendChild(dom.CreateTextNode($"{i + 1}) "));

                XmlElement pinyin = dom.CreateElement("pinyin");
                dom.DocumentElement.AppendChild(pinyin);
                pinyin.InnerText = strOnePinyin;

                dom.DocumentElement.AppendChild(dom.CreateTextNode("\r\n"));

                i++;
            }

            dom.DocumentElement.AppendChild(dom.CreateTextNode("---\r\n\r\n请选择一个。(输入序号，从1开始计数)"));

            strAskXml = dom.DocumentElement.OuterXml;
        }

        // 2018/11/16 新作此函数。XML 文件内每个范围，其尾部实际上是前方一致描述法
        // return:
        //		负数	在范围左边
        //		0	落入范围
        //		正数	在范围右边
        static int LocateRange(Range range,
            string strPinyin)
        {
            // 范围
            int nRet = CompareTwo(range.Start, strPinyin);
            if (nRet > 0)
                return -1;

            // 虽然和 Start 相等，但因为要排除 Start 本身，所以就当作落入左方处理了
            if (nRet == 0 && range.IncludeStart == false)
                return -1;

            // End 表示前方一致匹配。比如 ZHA-ZO 是应该匹配 ZONG 的
            if (strPinyin.StartsWith(range.End))
                nRet = 0;
            else
            {
                // 如果不是前方一致，再用传统比较法
                nRet = CompareTwo(range.End, strPinyin);
                if (nRet < 0)
                    return 1;
            }

            // 虽然和 End 相等，但因为要排除 End 本身，所以就当作落入右方处理了
            if (nRet == 0 && range.IncludeEnd == false)
                return 1;

            return 0;
        }

        // return:
        //		负数	在范围左边
        //		0	落入范围
        //		正数	在范围右边
        static int LocateRange(string strStart,
            string strTail,
            string strPinyin)
        {
            if (strTail == "" || strStart == strTail)
            {
                return CompareTwo(strPinyin, strStart);
            }

            // 范围
            int nRet = CompareTwo(strStart, strPinyin);
            if (nRet > 0)
                return -1;

            nRet = CompareTwo(strTail, strPinyin);
            if (nRet < 0)
                return 1;

            return 0;
        }

        static int CompareTwo(string strLeft,
    string strRight)
        {
            return String.CompareOrdinal(strLeft, strRight);
#if NO
            if (strLeft.Length < strRight.Length)
            {
                strRight = strRight.Substring(0, strLeft.Length);
                return String.Compare(strLeft, strRight);
            }
            else
            {
                strLeft = strLeft.Substring(0, strRight.Length);
                return String.Compare(strLeft, strRight);
            }
#endif
        }

        // 在精确范围中进行匹配
        // return:
        //		负数	在范围左边
        //		0	落入范围
        //		正数	在范围右边
        static int LocateExactRange(string strStart,
            string strTail,
            string strPinyin)
        {
            if (strStart == "")
                throw (new Exception("LocateExactRange()函数的strStart参数不能为空"));
            if (strTail == "")
                throw (new Exception("LocateExactRange()函数的strTail参数不能为空"));

            if (String.Compare(strPinyin, strStart) < 0)
                return -1;

            if (String.Compare(strPinyin, strTail) > 0)
                return 1;

            return 0;
        }

        class Range
        {
            public string Start { get; set; }
            // 是否包含 Start 本身？
            public bool IncludeStart { get; set; }

            public string End { get; set; }
            // 是否包含 End 本身？
            public bool IncludeEnd { get; set; }

            // [] 表示包含首尾。<> 表示不包含首尾
            public override string ToString()
            {
                StringBuilder text = new StringBuilder();
                if (this.IncludeStart)
                    text.Append("[");
                else
                    text.Append("<");
                text.Append(this.Start);
                text.Append("-");
                text.Append(this.End);
                if (this.IncludeEnd)
                    text.Append("]");
                else
                    text.Append(">");

                return text.ToString();
            }
        }

        static int IndexOf(XmlNodeList list, XmlNode node)
        {
            int i = 0;
            foreach (XmlNode current in list)
            {
                if (current == node)
                    return i;
                i++;
            }
            return -1;
        }

        static string GetStart(XmlElement node)
        {
            string range = node.GetAttribute("n");
            return StringUtil.ParseTwoPart(range, "-")[0];
        }

        static string GetEndOrStart(XmlElement node)
        {
            string range = node.GetAttribute("n");
            List<string> parts = StringUtil.ParseTwoPart(range, "-");
            if (string.IsNullOrEmpty(parts[1]) == false)
                return parts[1];
            return parts[0];
        }

        static string GetPrevText(XmlElement node)
        {
            string range = node.GetAttribute("n");
            if (range.IndexOf("-") == -1)
                return GetNextString(range);

            List<string> parts = StringUtil.ParseTwoPart(range, "-");
            if (string.IsNullOrEmpty(parts[1]) == false)
                return GetNextString(parts[1]);
            return GetNextString(parts[0]);
        }

        // 根据首字母查找范围属性
        // parameters:
        //		strPinyin	一个汉字的拼音。如果==""，表示找第一个r元素
        // return:
        //		-1	出错
        //		0	没有找到
        //		1	找到
        public static int GetSubRange(XmlDocument dom,
            string strPinyin,
            bool bOutputDebugInfo,
            out string strValue,
            out string strFufen,
            out string strDebugInfo,
            out string strError)
        {
            strValue = "";
            strFufen = "";
            strError = "";
            strDebugInfo = "";

            string strLast = "A";

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "缺省的第一个范围的起始字母为 '" + strLast + "'。\r\n";
            }

            int nElementCount = 0;
            XmlNode nodeDefault = null;

            string strHitValue = "";
            string strHitFufen = "";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("r");
            foreach (XmlElement node in nodes)
            {
                nElementCount++;

                string strRange = DomUtil.GetAttr(node, "n");

                if (strRange == "")
                    nodeDefault = node;

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "范围字符串 '" + strRange + "' \r\n";
                }

                if (strPinyin != "")
                {
                    if (strRange == "")
                    {
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "用非空拼音查找，则遇空范围要越过。\r\n";
                        }
                        continue;
                    }
                }

                strValue = DomUtil.GetAttr(node, "v");
                strFufen = DomUtil.GetAttr(node, "f");

                if (strPinyin == "")
                    return 1;

                Range range = new Range();
                //string strStart = "";
                //string strTail = "";

                int nRet = strRange.IndexOf("-");
                if (nRet != -1)
                {

                    range.Start = strRange.Substring(0, nRet).Trim();
                    range.IncludeStart = true;

                    range.End = strRange.Substring(nRet + 1).Trim();
                    range.IncludeEnd = true;
                }
                else
                {
                    // 一个号码的情况。需要转换为一个范围

                    range.End = strRange;
                    range.IncludeEnd = true;

                    int index = IndexOf(nodes, node) - 1;
                    if (index < 0)
                    {
                        range.Start = "A";    // 最小的一个字符
                        range.IncludeStart = true;  // 2019/12/12 应该包含 'A'。比如 胡阿祥
                        /*
                         * 2019/12/12 以前是下面几句：
                        if (range.Start == range.End)
                            range.IncludeStart = range.IncludeEnd;
                        else
                            range.IncludeStart = false;
                            */
                    }
                    else
                    {
                        range.Start = GetPrevText(nodes[index] as XmlElement);
                        range.IncludeStart = true;
                    }
#if NO
                    range.Start = strRange;
                    range.IncludeStart = true;

                    int index = IndexOf(nodes, node) + 1;
                    if (index >= nodes.Count)
                    {
                        range.End = "[";    // 找到字符 'Z' 后面一个字符。或者 '{' 更保险
                        range.IncludeEnd = false;
                    }
                    else
                    {
                        range.End = GetStart(nodes[index] as XmlElement);
                        range.IncludeEnd = false;
                    }
#endif
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += $"范围字符串被处理为 {range.ToString()}\r\n";
                }

                // 做事

                // return:
                //		负数	在范围左边
                //		0	落入范围
                //		正数	在范围右边
#if NO
                if ((strStart.Length > 1 || strTail.Length > 1)
                    && strStart.Length == strTail.Length && strPinyin.Length == strStart.Length)
                {
                    nRet = LocateExactRange(strStart,
                        strTail,
                        strPinyin);
                }
                else
#endif
                {
                    nRet = LocateRange(range,
                        strPinyin);
                }
                if (nRet < 0)
                {
                    if (strHitValue != "")
                    {
                        strValue = strHitValue;
                        strFufen = strHitFufen;
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "当前条目范围已偏向测试值左方。但先前条目命中过。所以返回value='" + strValue + "' fufen='" + strFufen + "'\r\n";
                        }
                        return 1;
                    }
                    strError = "拼音 '" + strPinyin + "' 没有找到对应的范围\r\n\r\n" + strDebugInfo;
                    return 0;
                }

                if (nRet == 0)
                {
                    // 记下曾经命中
                    strHitValue = strValue;
                    strHitFufen = strFufen;

                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "命中。但是继续向后探测。\r\n";
                    }

                    // 继续向后探测

                }


                // ? Debug.Assert(nRet > 0, "");


                // 把strTail的第一字母加一
                if (range.End.Length == 0)
                {
                    strError = "range '" + strRange + "' 时tail为空";
                    return -1;
                }
            }

            if (strHitValue != "")	// 曾经命中过
            {
                strValue = strHitValue;
                strFufen = strHitFufen;
                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "测试完全部条目。先前条目命中过。所以返回value='" + strValue + "' fufen='" + strFufen + "'\r\n";
                }
                return 1;
            }

            if (nElementCount == 1)
            {
                if (nodeDefault != null)
                {
                    strValue = DomUtil.GetAttr(nodeDefault, "v");
                    strFufen = DomUtil.GetAttr(nodeDefault, "f");
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "著者号码XML记录中只有一个范围，n参数为空，那么无论什么拼音都能匹配";
                        strDebugInfo += "命中。返回value='" + strValue + "' fufen='" + strFufen + "'\r\n";
                    }
                    return 1;
                }
            }

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "没有找到。\r\n";
            }

            strError = $"拼音 '{strPinyin}' 没有找到对应的范围。XML 定义如下: {dom.DocumentElement.OuterXml}";
            return 0;
        }

        // 2017/3/14
        // 获得下一个范围的起点
        static string GetNextString(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return "A";

            for (int i = strText.Length - 1; i >= 0; i--)
            {
                char ch = strText[i];
                ch = (char)((int)ch + 1);
                strText = strText.Substring(0, i) + ch; // +strText.Substring(i + 1);
                if ((char.IsUpper(ch) && ch <= 'Z')
                    || (char.IsLower(ch) && ch <= 'z'))
                    break;
            }

            return strText;
        }

        // 在复分表中进行查找
        // return:
        //		-1	出错
        //		>=0	正常
        int SearchFufen(
            string strFufenName,
            string strFirstPinyin,
            string strSecondPinyin,
            bool bOutputDebugInfo,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            if (strFufenName != "2"
                && strFufenName != "3"
                && strFufenName != "5"
                && strFufenName != "10")
            {
                strError = "复分表名 '" + strFufenName + "' 错误，必须是2 3 5 10之一";
                return -1;
            }

            // 2019/10/31
            // 拼音中“女”为 nu^，而复分表用了 v 表示 u^。需要预处理一下
            strFirstPinyin = strFirstPinyin.Replace("u^", "v");


            if (strFirstPinyin.Length == 0)
            {
                strError = "strFirstPinyin内容不能为空";
                return -1;
            }

            string[] range = null;
            int i;
            int nRet;

            // 单名
            if (strSecondPinyin == "")
            {
                if (strFirstPinyin.Length < 2)
                {
                    strError = "单名情形下，如果名之第一字拼音只有一个字母，不应调用本函数。(需作特殊处理)";
                    return -1;
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "单名情形。\r\n";
                }

                // 取得第一字的末尾字母
                string strTail = strFirstPinyin.Substring(strFirstPinyin.Length - 1, 1);
                range = null;

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "取名之第一字拼音 '" + strFirstPinyin + "'的末尾字母 '" + strTail + "'。\r\n";
                }

                if (strFufenName == "2")
                    range = fufen_2_dan;
                else if (strFufenName == "3")
                    range = fufen_3_dan;
                else if (strFufenName == "5")
                    range = fufen_5_dan;
                else if (strFufenName == "10")
                    range = fufen_10_dan;
                else
                {
                    Debug.Assert(false, "");
                }

                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "匹配 '" + strFufenName + "' 型单名复分表。\r\n";
                }

                strTail = strTail.ToLower();	// 因为复分表中的是小写

                for (i = 0; i < range.Length; i++)
                {
                    Debug.Assert(range[i].ToLower() == range[i], "单名复分表中定义的必须是小写字母");

                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "和 '" + range[i] + "' 进行比对。\r\n";
                    }

                    nRet = range[i].IndexOf(strTail);
                    if (nRet != -1)
                    {
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "命中，获得增量为 '" + Convert.ToString(i) + "' 。\r\n";
                        }
                        return i;
                    }
                }

                strError = "尾字母 '" + strTail + "' 在复分表 " + strFufenName + " 单名中没有命中";
                return -1;
            }

            // 双名
            // 取得第二字的首字母
            string strHead = strSecondPinyin.Substring(0, 1);

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "取名之第二字拼音 '" + strSecondPinyin + "'的首字母 '" + strHead + "'。\r\n";
            }

            if (strFufenName == "2")
                range = fufen_2_shuang;
            else if (strFufenName == "3")
                range = fufen_3_shuang;
            else if (strFufenName == "5")
                range = fufen_5_shuang;
            else if (strFufenName == "10")
                range = fufen_10_shuang;
            else
            {
                Debug.Assert(false, "");
            }

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "匹配 '" + strFufenName + "' 型双名复分表。\r\n";
            }

            for (i = 0; i < range.Length; i++)
            {
                string strRange = range[i];

                string strStart = "";
                string strTail = "";

                nRet = strRange.IndexOf("-");
                if (nRet == -1)
                {
                    strStart = strRange;
                    strTail = strRange;
                }
                else
                {
                    strStart = strRange.Substring(0, nRet).Trim();
                    strTail = strRange.Substring(nRet + 1).Trim();
                }

                Debug.Assert(strStart != "", "");
                Debug.Assert(strTail != "", "");
                Debug.Assert(String.Compare(strStart, strTail) <= 0, "");


                if (bOutputDebugInfo == true)
                {
                    strDebugInfo += "和 '" + strRange + "' 进行比对。\r\n";
                }

                // return:
                //		负数	在范围左边
                //		0	落入范围
                //		正数	在范围右边
                nRet = LocateRange(strStart.ToUpper(),
                    strTail.ToUpper(),
                    strHead.ToUpper());
                if (nRet < 0)
                    break;
                if (nRet == 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "命中，获得增量为 '" + Convert.ToString(i) + "' 。\r\n";
                    }
                    return i;
                }
            }

            strError = "首字母 '" + strHead + "' 在复分表 " + strFufenName + " 双名中没有命中";
            return -1;
        }

        // 给一个著者号增加一个数量。
        // 例如 B019 + 1 变成 B020
        static int AddFufen(string strText,
            int nNumber,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = strText;

            string strHead = strText;
            string strNumber = "";

            // 定位第一个数字

            for (int i = 0; i < strText.Length; i++)
            {
                if (Char.IsDigit(strText, i) == true)
                {
                    strHead = strText.Substring(0, i);
                    strNumber = strText.Substring(i);
                    break;
                }

            }

            if (strNumber == "")
                strNumber = "0";

            int nWidth = strNumber.Length;

            long nValue = 0;
            try
            {
                nValue = Convert.ToInt64(strNumber);
            }
            catch (Exception ex)
            {
                strError = "数字 '" + strNumber + "' 格式不正常: " + ex.Message;
                return -1;
            }

            strNumber = Convert.ToString(nValue + nNumber).PadLeft(nWidth, '0');
            strResult = strHead + strNumber;

            return 0;
        }

        // 建立供选择的多音字列表文本
        static string BuildPinyinList(string strMultiPinyin)
        {
            string strResult = "";

            string[] pinyins = strMultiPinyin.Split(new char[] { ';' });
            for (int i = 0; i < pinyins.Length; i++)
            {
                strResult += Convert.ToString(i + 1) + ") " + pinyins[i] + "\r\n";
            }

            return strResult;
        }

        // 从多个拼音中选择一个
        int SelectPinyin(string strMultiPinyin,
            int nIndex,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            string[] pinyins = strMultiPinyin.Split(new char[] { ';' });
            if (nIndex >= pinyins.Length || nIndex < 0)
            {
                strError = "所给出的序号越界";
                return -1;
            }
            strPinyin = pinyins[nIndex];

            return 0;
        }

#endregion

        // return:
        //      -1  出错
        //      0   成功
        public int SetPinyinInternal(
            SessionInfo sessioninfo,
            string strPinyinXml,
            out string strError)
        {
            strError = "";

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strPinyinXml);
            }
            catch (Exception ex)
            {
                strError = "strPinyinXml装载到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                    continue;

                string strHanzi = "";
                string strSelectedPinyin = "";
                int nRet = BuildHanziAndSelPinyin(node,
            out strHanzi,
            out strSelectedPinyin,
            out strError);
                if (nRet == -1)
                    return -1;

                if (string.IsNullOrEmpty(strHanzi) == true)
                {
                    strError = "XML片断 '" + node.OuterXml + "' 格式不正确：没有包含汉字";
                    return -1;
                }

                // 单字
                if (strHanzi.Length == 1)
                    continue;

                nRet = SaveHanzi(channel,
        strHanzi,
        strSelectedPinyin,
        out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // return:
        //      -1  出错
        //      0   成功
        public int GetSjhmInternal(
            SessionInfo sessioninfo,
string strText,
out string strSjhmXml,
out string strError)
        {
            strSjhmXml = "";
            strError = "";

            // string strPinyin = "";
            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (char ch in strText)
            {
                // 汉字
                XmlElement nodeChar = dom.CreateElement("char");
                dom.DocumentElement.AppendChild(nodeChar);
                nodeChar.InnerText = new string(ch, 1);

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                    continue;

                nRet = SearchHanzi(channel,
        new string(ch, 1),
        out string strPinyin,
        out string strSjhm,
        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    nodeChar.SetAttribute("p", strPinyin);
            }

            strSjhmXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        // return:
        //      -1  出错
        //      0   成功
        public int GetPinyinInternal(
            SessionInfo sessioninfo,
string strText,
out string strPinyinXml,
out string strError)
        {
            strPinyinXml = "";
            strError = "";

            string strPinyin = "";
            int nRet = 0;

            List<string> parts = null;

            try
            {
                parts = SplitText(strText);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            foreach (string text in parts)
            {
                if (ContainHanzi(text, out string strHanzi) == false
                    || IsAllLetter(text) == true)
                {
                    XmlNode nodeText = dom.CreateTextNode(text);
                    dom.DocumentElement.AppendChild(nodeText);
                    continue;
                }

                XmlNode nodeWord = dom.CreateElement("word");
                dom.DocumentElement.AppendChild(nodeWord);

                // 创建<char>元素
                string strChars = "";
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    if (StringUtil.IsHanzi(ch) == false)
                    {
                        strChars += ch;
                        continue;
                    }

                    // 看看是否特殊符号
                    if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                    {
                        strChars += ch;
                        continue;
                    }

                    // 结束积累的
                    if (string.IsNullOrEmpty(strChars) == false)
                    {
                        XmlNode nodeText = dom.CreateTextNode(strChars);
                        nodeWord.AppendChild(nodeText);
                        strChars = "";
                    }

                    // 汉字
                    XmlNode nodeChar = dom.CreateElement("char");
                    nodeWord.AppendChild(nodeChar);
                    nodeChar.InnerText = new string(ch, 1);

                    strPinyin = "";
                    nRet = SearchHanzi(channel,
            new string(ch, 1),
            out strPinyin,
            out string strSjhm,
            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        DomUtil.SetAttr(nodeChar, "p", strPinyin);
                    }
                }

                // 结束积累的
                if (string.IsNullOrEmpty(strChars) == false)
                {
                    XmlNode nodeText = dom.CreateTextNode(strChars);
                    nodeWord.AppendChild(nodeText);
                    strChars = "";
                }
            }

            // 填充<word>元素的拼音属性p
            // 用每个<char>内的汉字构成的字符串来查找词库,这样得到的拼音可以和<char>元素一一对应
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                    continue;

                string strHanzi = BuildHanzi(node);
                if (strHanzi.Length == 1)
                    continue;   // 当<word>元素内仅有一个<char>元素时，则<word>元素没有必要包含p属性。因为这个p属性值会和下级<char>元素的p属性值一样
                strPinyin = "";
                nRet = SearchHanzi(channel,
        strHanzi,
        out strPinyin,
        out string strSjhm,
        out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    DomUtil.SetAttr(node, "p", strPinyin);
                }
            }

            strPinyinXml = dom.DocumentElement.OuterXml;
            return 0;
        }

#region 加拼音有关的下级函数

        static string BuildHanzi(XmlNode nodeWord)
        {
            string strResult = "";
            foreach (XmlNode node in nodeWord.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                    continue;
                strResult += node.InnerText;
            }

            return strResult;
        }

        static int BuildHanziAndSelPinyin(XmlNode nodeWord,
            out string strHanzi,
            out string strSelectedPinyin,
            out string strError)
        {
            strError = "";
            strHanzi = "";
            strSelectedPinyin = "";
            foreach (XmlNode node in nodeWord.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                if (node.Name != "char")
                    continue;
                if (string.IsNullOrEmpty(node.InnerText) == true)
                {
                    strError = "有<char>元素不具备文本内容";
                    return -1;
                }
                if (node.InnerText.Length != 1)
                {
                    strError = "<char>元素文本内容必须为1字符(然而'" + node.InnerText + "'却为 " + node.InnerText.Length.ToString() + " 字符)";
                    return -1;
                }
                string strTempPinyin = DomUtil.GetAttr(node, "sel");
                if (string.IsNullOrEmpty(strTempPinyin) == true)
                {
                    strError = "汉字 '" + node.InnerText + "' 没有给出拼音(sel属性)";
                    return -1;
                }
                strHanzi += node.InnerText;
                if (string.IsNullOrEmpty(strSelectedPinyin) == false)
                    strSelectedPinyin += " ";
                strSelectedPinyin += strTempPinyin;
            }

            return 0;
        }

        // 检索获得汉字的拼音和四角号码
        // 注：只有当 strHanzi 中包含一个汉字时，才能获得其四角号码
        int SearchHanzi(RmsChannel channel,
            string strHanzi,
            out string strPinyin,
            out string strSjhm,
            out string strError)
        {
            this.hanzi_locks.LockForRead(strHanzi);
            try
            {
                strError = "";
                strPinyin = "";
                strSjhm = "";
                string strQueryXml = "";

                if (strHanzi.Length == 1)
                {
                    strQueryXml = "<target list='"
                         + StringUtil.GetXmlStringSimple(this.PinyinDbName)
                         + ":" + "汉字'><item><word>"
                         + StringUtil.GetXmlStringSimple(strHanzi)
                         + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                }
                else
                {
                    strQueryXml = "<target list='"
                         + StringUtil.GetXmlStringSimple(this.WordDbName)   // 2007/9/14 new add
                         + ":" + "汉字'><item><word>"
                         + StringUtil.GetXmlStringSimple(strHanzi)
                         + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                }

#if NO
                long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                {
                    if (strHanzi.Length == 1)
                    {
                        strError = "检索库 " + this.PinyinDbName + " 时出错: " + strError;
                    }
                    else
                    {
                        strError = "检索库 " + this.WordDbName + " 时出错: " + strError;
                    }
                    return -1;
                }
                if (lRet == 0)
                {
                    return 0;
                }

                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "default",
                    1000,
                    "zh",
                    null,	// stop,
                    out aPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索汉字的拼音获取检索结果时出错: " + strError;
                    return -1;
                }

                                for (int i = 0; i < aPath.Count; i++)
                {
                    string strPath = (string)aPath[i];
                    // 取记录
                    string strStyle = "content,data";

                    string strMetaData;
                    string strOutputPath;
                    string strXml = "";
                    byte[] baTimeStamp = null;

                    lRet = channel.GetRes(strPath,
                        strStyle,
                        out strXml,
                        out strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得 '" + strPath + "' 记录体时出错: " + strError;
                        return -1;
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                        return -1;
                    }

                    if (strHanzi.Length == 1)
                        strPinyin = DomUtil.GetAttr(dom.DocumentElement, "p");
                    else
                    {
                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("pinyin");

                        // 排序。使用频率高的在前
                        List<XmlNode> node_array = new List<XmlNode>();
                        foreach (XmlNode node in nodes)
                        {
                            node_array.Add(node);
                        }
                        node_array.Sort(new HitCountComparer());

                        foreach (XmlNode node in node_array)
                        {
                            if (string.IsNullOrEmpty(strPinyin) == false)
                                strPinyin += ";";
                            strPinyin += node.InnerText;
                        }
                    }
                    return 1;
                }

#endif
                long lRet = channel.DoSearchEx(strQueryXml,
    "default",
    "", // strOutputStyle
    1,
    "zh",
    "id,xml",
    out Record[] records,
    out strError);
                if (lRet == -1)
                {
                    if (strHanzi.Length == 1)
                    {
                        strError = "检索库 " + this.PinyinDbName + " 时出错: " + strError;
                    }
                    else
                    {
                        strError = "检索库 " + this.WordDbName + " 时出错: " + strError;
                    }
                    return -1;
                }
                if (lRet == 0)
                    return 0;

                string strXml = records[0].RecordBody.Xml;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "装载路径为 '" + records[0].Path + "' 的 XML 记录到 DOM 时出错: " + ex.Message;
                    return -1;
                }

                if (strHanzi.Length == 1)
                {
                    strPinyin = DomUtil.GetAttr(dom.DocumentElement, "p");
                    strSjhm = dom.DocumentElement.GetAttribute("s");
                }
                else
                {
                    XmlNodeList nodes = dom.DocumentElement.SelectNodes("pinyin");

                    // 排序。使用频率高的在前
                    List<XmlNode> node_array = new List<XmlNode>();
                    foreach (XmlNode node in nodes)
                    {
                        node_array.Add(node);
                    }
                    node_array.Sort(new HitCountComparer());

                    // TODO: 大小写不同的，都应该转换为小写形态，然后合并
                    foreach (XmlNode node in node_array)
                    {
                        if (string.IsNullOrEmpty(strPinyin) == false)
                            strPinyin += ";";
                        strPinyin += node.InnerText;
                    }
                }
                strPinyin = ToLower(strPinyin);
                return 1;
            }
            finally
            {
                this.hanzi_locks.UnlockForRead(strHanzi);
            }
        }

        int SaveHanzi(RmsChannel channel,
            string strHanzi,
            string strPinyin,
            out string strError)
        {
            this.hanzi_locks.LockForWrite(strHanzi);
            try
            {
                // 2018/10/25
                strPinyin = ToLower(strPinyin);

                strError = "";
                string strQueryXml = "";
                string strCreatePath = "";

                if (strHanzi.Length == 1)
                {
                    strCreatePath = this.PinyinDbName + "/?";
                    strQueryXml = "<target list='"
                         + StringUtil.GetXmlStringSimple(this.PinyinDbName)
                         + ":" + "汉字'><item><word>"
                         + StringUtil.GetXmlStringSimple(strHanzi)
                         + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                }
                else
                {
                    strCreatePath = this.WordDbName + "/?";
                    strQueryXml = "<target list='"
                         + StringUtil.GetXmlStringSimple(this.WordDbName)   // 2007/9/14 new add
                         + ":" + "汉字'><item><word>"
                         + StringUtil.GetXmlStringSimple(strHanzi)
                         + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>1000</maxCount></item><lang>zh</lang></target>";
                }

                long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                {
                    if (strHanzi.Length == 1)
                    {
                        strError = "检索库 " + this.PinyinDbName + " 时出错: " + strError;
                    }
                    else
                    {
                        strError = "检索库 " + this.WordDbName + " 时出错: " + strError;
                    }
                    return -1;
                }
                if (lRet == 0)
                {
                    // 不存在，需要创建记录
                    // TODO: 锁定
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<p />");
                    DomUtil.SetAttr(dom.DocumentElement, "h", strHanzi);
                    if (strHanzi.Length == 1)
                        DomUtil.SetAttr(dom.DocumentElement, "p", strPinyin);
                    else
                    {
                        XmlNode node = dom.CreateElement("pinyin");
                        dom.DocumentElement.AppendChild(node);
                        node.InnerText = strPinyin;
                    }
                    lRet = channel.DoSaveTextRes(strCreatePath,
                        dom.DocumentElement.OuterXml,
                        false,
                        "", // strStyle,
                        null,   // timestamp
                        out byte[] output_timestamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    return 0;
                }

                lRet = channel.DoGetSearchResult(
                    "default",
                    1000,
                    "zh",
                    null,	// stop,
                    out List<string> aPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "检索汉字的拼音获取检索结果时出错: " + strError;
                    return -1;
                }

                for (int i = 0; i < aPath.Count; i++)
                {
                    string strPath = (string)aPath[i];
                    // 取记录
                    string strStyle = "content,data,timestamp";

                    lRet = channel.GetRes(strPath,
                        strStyle,
                        out string strXml,
                        out string strMetaData,
                        out byte[] baTimeStamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得 '" + strPath + "' 记录体时出错: " + strError;
                        return -1;
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载路径为'" + strPath + "'的xml记录时出错: " + ex.Message;
                        return -1;
                    }

                    bool bChanged = false;

                    if (strHanzi.Length == 1)
                    {
                        // 老格式
                        string strExistPinyin = "";
                        strExistPinyin = DomUtil.GetAttr(dom.DocumentElement, "p");
                        // 2018/10/25
                        strExistPinyin = ToLower(strExistPinyin);
                        if (string.IsNullOrEmpty(strExistPinyin) == false)
                        {
                            string[] parts = strExistPinyin.Split(new char[] { ';' });
                            foreach (string strPart in parts)
                            {
                                if (strPart == strPinyin)
                                    return 0;   // 已经存在
                            }
                        }

                        string strNewValue = strExistPinyin;
                        if (string.IsNullOrEmpty(strNewValue) == false)
                            strNewValue += ";";
                        DomUtil.SetAttr(dom.DocumentElement, "p", strNewValue);
                        bChanged = true;
                    }
                    else
                    {
                        // 新格式
                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("pinyin");
                        foreach (XmlNode node in nodes)
                        {
                            // existing_pinyins.Add(node.InnerText);
                            if (ToLower(node.InnerText) == strPinyin)
                            {
                                // 累加计数器
                                string strCount = DomUtil.GetAttr(node, "c");
                                Int64.TryParse(strCount, out long nCount);
                                nCount++;
                                DomUtil.SetAttr(node, "c", nCount.ToString());
                                bChanged = true;
                                goto DO_SAVE;
                            }
                        }

                        // 增补新项
                        XmlNode new_node = dom.CreateElement("pinyin");
                        dom.DocumentElement.AppendChild(new_node);
                        new_node.InnerText = strPinyin;
                        bChanged = true;
                    }

                    DO_SAVE:
                    if (bChanged == true)
                    {
                        byte[] output_timestamp = null;
                        lRet = channel.DoSaveTextRes(strPath,
                            dom.DocumentElement.OuterXml,
                            false,
                            "", // strStyle,
                            baTimeStamp,   // timestamp
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                            return -1;
                        return 1;
                    }

                    return 0;
                }

                return 0;
            }
            finally
            {
                this.hanzi_locks.UnlockForWrite(strHanzi);
            }
        }

        static string ToLower(string text)
        {
            if (text == null)
                return null;
            return text.ToLower();
        }

        // static string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’•";

        static bool ContainHanzi(string strText,
            out string strHanzi)
        {
            strHanzi = "";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                {
                    continue;
                }

                // 汉字
                strHanzi += ch;
            }

            if (string.IsNullOrEmpty(strHanzi) == false)
                return true;

            return false;
        }

        // 观察一段文字是否为空白
        static bool IsBlank(string strText,
            int nStart,
            int nLength)
        {
            if (nLength == 0)
                return true;

            for (int i = 0; i < nLength; i++)
            {
                if (strText[nStart + i] != ' '
                    && strText[nStart + i] != '　')
                    return false;
            }

            return true;
        }

        static int IndexOf(string strText,
            string strWord,
            int nStart,
            out int nLength)
        {
            nLength = 0;
            int nFirstPos = -1;
            int nPrevChar = -1;
            for (int i = 0; i < strWord.Length; i++)
            {
                int nPos = strText.IndexOf(strWord[i], nStart);
                if (nPos == -1)
                    return -1;
                if (nFirstPos == -1)
                    nFirstPos = nPos;

                if (nPrevChar != -1
                    && nPos - nPrevChar - 1 > 0)
                {
                    if (IsBlank(strText, nPrevChar + 1, nPos - nPrevChar - 1) == false)
                        return -1;
                }
                nPrevChar = nPos;
                nStart = nPos + 1;
            }
            nLength = nPrevChar - nFirstPos + 1;
            return nFirstPos;
        }

        static bool IsAllLetter(string strText)
        {
            foreach (char ch in strText)
            {
                if ((ch >= 'a' && ch <= 'z')
                    || (ch >= 'A' && ch <= 'Z'))
                {
                }
                else if (ch >= '0' && ch <= '9')
                {
                }
                else if (ch >= '０' && ch <= '９')
                {
                }
                else
                    return false;
            }
            return true;
        }

        static string BuildDisplayFormat(List<string> tokens)
        {
            string strResult = "";
            foreach (string text in tokens)
            {
                strResult += "{" + text + "}";
            }
            return strResult;
        }

        // 可能会抛出异常
        static void Match(List<string> tokens,
            string strOriginText,
            out List<string> results)
        {
            results = new List<string>();
            int nStart = 0;
            foreach (string token in tokens)
            {
                if (token == ",")
                    continue;
                if (IsAllLetter(token) == true)
                    continue;

                int nLength = 0;
                int nRet = IndexOf(strOriginText, token, nStart, out nLength);
                if (nRet == -1)
                {
                    throw new Exception("字符串 '" + strOriginText + "' 无法和分词后的数组 '" + BuildDisplayFormat(tokens) + "' 匹配");
                    //results.Add("?" + token);
                    //continue;
                }
                if (nRet > nStart)
                {
                    results.Add(strOriginText.Substring(nStart, nRet - nStart));
                    nStart = nRet;
                }
                results.Add(strOriginText.Substring(nStart, nLength));
                nStart += nLength;
            }

            if (nStart < strOriginText.Length)
            {
                results.Add(strOriginText.Substring(nStart));
            }
        }

        // 可能会抛出异常
        public static List<string> SplitText(string strText)
        {
            Analyzer ca = new SmartChineseAnalyzer(false);  // true
            StringReader sentence = new StringReader(
                strText
                    );
            TokenStream ts = ca.TokenStream("sentence", sentence);
            TermAttribute termAttr = (TermAttribute)ts.GetAttribute(typeof(TermAttribute));

            List<string> tokens = new List<string>();
            while (ts.IncrementToken())
            {
                tokens.Add(termAttr.Term());
            }
            ts.Close();

            List<string> results = new List<string>();
            Match(tokens,
                strText,
                out results);
            return results;
        }

#endregion

        public static Question GetQuestion(List<Question> questions, int index)
        {
            if (index >= questions.Count)
                return null;
            return questions[index];
        }

        public static Question NewQuestion(List<Question> questions,
            int index,
            string strText,
            string strXml)
        {
            Question result = null;

            for (; ; )
            {
                if (index >= questions.Count)
                {
                    result = new Question();
                    questions.Add(result);
                }
                else
                    break;
            }

            if (index < questions.Count)
            {
                result = questions[index];
                result.Text = strText;
                result.Xml = strXml;
                result.Answer = "";
                return result;
            }

            Debug.Assert(false, "");	// 不可能走到这里
            return null;
        }
    }

#if NO
    public class QuestionCollection : List<Question>
    {
        // public string Name = "";

        public Question GetQuestion(int index)
        {
            if (index >= this.Count)
                return null;
            return this[index];
        }

        public Question NewQuestion(int index,
            string strText)
        {
            Question result = null;

            for (; ; )
            {
                if (index >= this.Count)
                {
                    result = new Question();
                    this.Add(result);
                }
                else
                    break;
            }

            if (index < this.Count)
            {
                result = this[index];
                result.Text = strText;
                result.Answer = "";
                return result;
            }

            Debug.Assert(false, "");	// 不可能走到这里
            return null;
        }
    }
#endif

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class Question
    {
        [DataMember]
        public string Text = "";	// 问题正文

        // 2018/11/20
        [DataMember]
        public string Xml = "";     // 用 XML 格式描述的问题正文

        [DataMember]
        public string Answer = "";	// 问题答案
    }

    // 按照拼音条目的使用频率排序
    public class HitCountComparer : IComparer<XmlNode>
    {
        int IComparer<XmlNode>.Compare(XmlNode x, XmlNode y)
        {
            string s1 = DomUtil.GetAttr(x, "c");
            string s2 = DomUtil.GetAttr(y, "c");

            Int64.TryParse(s1, out long c1);
            Int64.TryParse(s2, out long c2);

            // 大在前
            return -1 * (int)(c1 - c2);
        }
    }

}
