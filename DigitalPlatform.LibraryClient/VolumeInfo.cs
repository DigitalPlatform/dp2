using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryClient
{
    // 期刊业务
    // 一个具体的卷期信息
    public class VolumeInfo
    {
        public string Year = "";
        public string IssueNo = "";
        public string Zong = "";
        public string Volume = "";

        public string GetString(/*bool bIncludeYear*/)
        {
            return BuildItemVolumeString(
                this.Year,
                this.IssueNo,
                this.Zong,
                this.Volume);
        }

        // 构造表达一个册所在的当年期号、总期号、卷号的字符串
        public static string BuildItemVolumeString(
            string strYear,
            string strIssue,
            string strZong,
            string strVolume)
        {
            string strResult = "";
            if (String.IsNullOrEmpty(strYear) == false)
            {
                strResult += strYear;
            }

            if (String.IsNullOrEmpty(strIssue) == false)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ",";
                strResult += "no." + strIssue;
            }

            if (String.IsNullOrEmpty(strZong) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "总." + strZong;
            }

            if (String.IsNullOrEmpty(strVolume) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "v." + strVolume;
            }

            return strResult;
        }

        /*
        // 构造表达一个册所在的当年期号、总期号、卷号的字符串
        public static string BuildItemVolumeString(string strIssue,
            string strZong,
            string strVolume)
        {
            string strResult = "";
            if (String.IsNullOrEmpty(strIssue) == false)
                strResult += "no." + strIssue;

            if (String.IsNullOrEmpty(strZong) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "总." + strZong;
            }

            if (String.IsNullOrEmpty(strVolume) == false)
            {
                if (strResult != "")
                    strResult += ", ";
                strResult += "v." + strVolume;
            }

            return strResult;
        }
         * */

        // 解析no.序列
        public static int ExpandNoString(string strText,
            string strDefaultYear,
            out List<VolumeInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<VolumeInfo>();

            string strCurrentYear = strDefaultYear;

            string[] no_parts = strText.Split(new char[] { ',', ':', ';', '，', '：', '；' });    // ':' ';' 是为了兼容某个阶段的临时用法 2001:no.1-2;2002:no.1-12
            for (int i = 0; i < no_parts.Length; i++)
            {
                string strPart = no_parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                if (StringUtil.IsNumber(strPart) == true
                    && strPart.Length == 4)
                {
                    strCurrentYear = strPart;
                    continue;
                }

                // 去掉"no."部分
                if (StringUtil.HasHead(strPart, "no.") == true)
                {
                    strPart = strPart.Substring(3).Trim();
                }

                // TODO: 没有"no."开头的，是否警告?

                if (String.IsNullOrEmpty(strPart) == true)
                    continue;

                List<string> nos = null;

                try
                {
                    nos = ExpandSequence(strPart);
                }
                catch (Exception ex)
                {
                    strError = "序列 '" + strPart + "' 格式错误:" + ex.Message;
                    return -1;
                }

                for (int j = 0; j < nos.Count; j++)
                {
                    string strNo = nos[j];

                    if (String.IsNullOrEmpty(strCurrentYear) == true)
                    {
                        strError = "当遇到 '" + strNo + "' 的时候，没有必要的年份信息，无法解析no.序列信息";
                        return -1;
                    }

                    VolumeInfo info = new VolumeInfo();
                    info.IssueNo = strNo;
                    info.Year = strCurrentYear;
                    infos.Add(info);
                }

            }

            return 0;
        }

        // 解析卷期范围序列。例如“2001,no.1-12=总.101-112=v.25*12”
        public static int BuildVolumeInfos(string strText,
            out List<VolumeInfo> infos,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            infos = new List<VolumeInfo>();

            string strYearString = "";
            string strNoString = "";
            string strVolumeString = "";
            string strZongString = "";

            List<string> notdef_segments = new List<string>();

            string[] segments = strText.Split(new char[] { '=' });
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();
                if (String.IsNullOrEmpty(strSegment) == true)
                    continue;
                if (strSegment.IndexOf("y.") != -1)
                    strYearString = strSegment;
                else if (strSegment.IndexOf("no.") != -1)
                    strNoString = strSegment;
                else if (strSegment.IndexOf("v.") != -1)
                    strVolumeString = strSegment;
                else if (strSegment.IndexOf("总.") != -1)
                    strZongString = strSegment;
                else
                {
                    notdef_segments.Add(strSegment);
                }
            }

            // 2012/4/25
            // 当年期号序列很重要，如果缺了，光有总期号和卷号是不行的
            if (string.IsNullOrEmpty(strNoString) == true
                && (string.IsNullOrEmpty(strZongString) == false || string.IsNullOrEmpty(strVolumeString) == false))
            {
                strError = "当年期号序列不能省却。'" + strText + "'";
                if (notdef_segments.Count > 0)
                    strError += "。字符串中出现了无法识别的序列: " + StringUtil.MakePathList(notdef_segments, "=");
                return -1;
            }

            if (String.IsNullOrEmpty(strNoString) == false)
            {
                // 去掉"y."部分
                if (StringUtil.HasHead(strYearString, "y.") == true)
                {
                    strYearString = strYearString.Substring(2).Trim();
                }

                // 解析no.序列
                nRet = ExpandNoString(strNoString,
                    strYearString,
                    out infos,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析序列 '" + strNoString + "' (年份'" + strYearString + "')时发生错误: " + strError;
                    return -1;
                }
            }

            // 去掉"总."部分
            if (StringUtil.HasHead(strZongString, "总.") == true)
            {
                strZongString = strZongString.Substring(2).Trim();
            }

            if (String.IsNullOrEmpty(strZongString) == false)
            {
                List<string> zongs = null;

                try
                {
                    zongs = ExpandSequence(strZongString);
                }
                catch (Exception ex)
                {
                    strError = "总. 序列 '" + strZongString + "' 格式错误:" + ex.Message;
                    return -1;
                }

                for (int i = 0; i < infos.Count; i++)
                {
                    VolumeInfo info = infos[i];
                    if (i < zongs.Count)
                        info.Zong = zongs[i];
                    else
                        break;
                }
            }

            // 去掉"v."部分
            if (StringUtil.HasHead(strVolumeString, "v.") == true)
            {
                strVolumeString = strVolumeString.Substring(2).Trim();
            }

            if (String.IsNullOrEmpty(strVolumeString) == false)
            {
                List<string> volumes = null;

                try
                {
                    volumes = ExpandSequence(strVolumeString);
                }
                catch (Exception ex)
                {
                    strError = "v.序列 '" + strVolumeString + "' 格式错误:" + ex.Message;
                    return -1;
                }

                string strLastValue = "";
                for (int i = 0; i < infos.Count; i++)
                {
                    VolumeInfo info = infos[i];
                    if (i < volumes.Count)
                    {
                        info.Volume = volumes[i];
                        strLastValue = info.Volume; // 记忆最后一个
                    }
                    else
                        info.Volume = strLastValue; // 沿用最后一个
                }
            }

            // 2015/5/8 如果 strText 内容为“绿笔采风”之类的，就无法分析出部件
            if (infos.Count == 0 && notdef_segments.Count > 0)
            {
                strError += "卷期范围字符串中出现了无法识别的序列: " + StringUtil.MakePathList(notdef_segments, "=");
                return -1;
            }

            return 0;
        }

        // 展开号码字符串
        // 可能抛出异常
        public static List<string> ExpandSequence(string strText)
        {
            List<string> results = new List<string>();
            string[] parts = strText.Split(new char[] { ',', '，' });
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i];
                if (String.IsNullOrEmpty(strPart) == true)
                {
                    results.Add(strPart);
                    continue;
                }

                // -
                int nRet = strPart.IndexOf("-");
                if (nRet != -1)
                {
                    string strStart = strPart.Substring(0, nRet);
                    string strEnd = strPart.Substring(nRet + 1);

                    int start = Convert.ToInt32(strStart);
                    int end = Convert.ToInt32(strEnd);

                    for (int j = start; j <= end; j++)
                    {
                        results.Add(j.ToString());
                    }

                    continue;
                }

                // *
                nRet = strPart.IndexOf("*");
                if (nRet != -1)
                {
                    string strValue = strPart.Substring(0, nRet);
                    string strCount = strPart.Substring(nRet + 1);

                    int count = Convert.ToInt32(strCount);
                    for (int j = 0; j < count; j++)
                    {
                        results.Add(strValue);
                    }

                    continue;
                }

                results.Add(strPart);
            }

            return results;
        }

        // 解析当年期号、总期号、卷号的字符串
        public static void ParseItemVolumeString(string strVolumeString,
            out string strIssue,
            out string strZong,
            out string strVolume)
        {
            strIssue = "";
            strZong = "";
            strVolume = "";

            string[] segments = strVolumeString.Split(new char[] { ';', ',', '=', '；', '，', '＝' });    // ',','='为2010/2/24新增
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();

                if (StringUtil.HasHead(strSegment, "no.") == true)
                    strIssue = strSegment.Substring(3).Trim();
                else if (StringUtil.HasHead(strSegment, "总.") == true)
                    strZong = strSegment.Substring(2).Trim();
                else if (StringUtil.HasHead(strSegment, "v.") == true)
                    strVolume = strSegment.Substring(2).Trim();
            }
        }

        public static int CheckIssueNo(
            string strName,
            string strIssueNo,
            out string strError)
        {
            strError = "";

            if (strIssueNo.IndexOfAny(new char[] { '-', '*', ',', ';', '=', '?', '－', '＊', '，', '；', '＝', '？' }) != -1)
            {
                strError = strName + "字符串中不能包含下列字符: '-','*',',',';','=','?'";
                return -1;
            }

            return 0;
        }

        // 如果出现等号，优先用等号，逗号就被当作缺期的中断符号了
        // 2016,no.1-3,5=总.100-102,104=v.10*4
        // 需要直接用 BuildVolumeInfos() 函数
        public static void ParseItemVolumeString(string strVolumeString,
            out string strYear,
    out string strIssue,
    out string strZong,
    out string strVolume)
        {
            strYear = "";
            strIssue = "";
            strZong = "";
            strVolume = "";

            string[] segments = strVolumeString.Split(new char[] { ';', ',', '=', '；', '，', '＝' });    // ',','='为2010/2/24新增
            for (int i = 0; i < segments.Length; i++)
            {
                string strSegment = segments[i].Trim();

                if (StringUtil.HasHead(strSegment, "no.") == true)
                    strIssue = strSegment.Substring(3).Trim();
                else if (StringUtil.HasHead(strSegment, "总.") == true)
                    strZong = strSegment.Substring(2).Trim();
                else if (StringUtil.HasHead(strSegment, "v.") == true)
                    strVolume = strSegment.Substring(2).Trim();
                else if (StringUtil.HasHead(strSegment, "y.") == true)
                    strYear = strSegment.Substring(2).Trim();
                else if ( i == 0 )  // 只有第一个没有前缀的字符串才会被当作年份。因为后面还会出现逗号
                    strYear = strSegment.Trim();
            }
        }
    }
}
