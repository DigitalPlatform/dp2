using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    // 内核数据库信息数组
    public class KernelDbInfoCollection : List<KernelDbInfo>
    {
        // return:
        //      -1  出错
        //      0   成功
        public int Initial(RmsChannelCollection Channels,
            string strServerUrl,
            string strLang,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ResInfoItem[] root_dir_results = null;

            RmsChannel channel = Channels.GetChannel(strServerUrl);

            // 列出所有数据库
            root_dir_results = null;

            long lRet = channel.DoDir("",
                strLang,
                "alllang",
                out root_dir_results,
                out strError);
            if (lRet == -1)
                return -1;

            // 针对数据库的循环
            for (int i = 0; i < root_dir_results.Length; i++)
            {
                ResInfoItem info = root_dir_results[i];
                if (info.Type != ResTree.RESTYPE_DB)
                    continue;

                ResInfoItem[] db_dir_result = null;

                lRet = channel.DoDir(info.Name,
                       strLang,
                       "alllang",
                       out db_dir_result,
                       out strError);
                if (lRet == -1)
                    return -1;

                KernelDbInfo db = new KernelDbInfo();
                nRet = db.Initial(info.Names, db_dir_result,
                    out strError);
                if (nRet == -1)
                    return -1;

                this.Add(db);
            }

            return 0;
        }

        // 根据数据库名找到一个KernelDbInfo数据库对象
        // return:
        //      null   not found
        //      others  found
        public KernelDbInfo FindDb(string strCaption)
        {
            for (int i = 0; i < this.Count; i++)
            {
                KernelDbInfo db = this[i];

                if (db.MatchCaption(strCaption) == true)
                    return db;
            }
            return null;
        }

        // 从特定的数据库中, 匹配出满足特定风格列表的from列表
        // parameters:
        //      strFromStyle    from style的列表, 以逗号分割。
        //                      如果为空，表示全部途径(2007/9/13 new add)
        // return:
        //      null    没有找到
        //      以逗号分割的from名列表
        public string BuildCaptionListByStyleList(string strDbName,
            string strFromStyles,
            string strLang)
        {
            KernelDbInfo db = this.FindDb(strDbName);

            if (db == null)
                return null;

            string strResult = "";

            // 2007/9/13 new add
            if (String.IsNullOrEmpty(strFromStyles) == true
                || strFromStyles == "<全部>" || strFromStyles.ToLower() == "<all>")
            {
                return "<all>";
                // strFromStyles = "<all>";
            }

            List<string> results = new List<string>();

            // 拆分出单独的style字符串
            string[] styles = strFromStyles.Split(new char[] {','});

            for (int i = 0; i < styles.Length; i++)
            {
                string strStyle = styles[i].Trim();
                if (String.IsNullOrEmpty(strStyle) == true)
                    continue;

                // 2012/5/16
                // 忽略 _time/_freetime,_rfc1123time/_utime等表示检索特性的style
                if (StringUtil.HasHead(strStyle, "_") == true
                    && StringUtil.HasHead(strStyle, "__") == false) // 但是 __ 引导的要参与匹配
                    continue;

                // 遍历当前数据库的所有form的styles
                for (int j = 0; j < db.Froms.Count; j++)
                {
                    string strStyles = db.Froms[j].Styles;

                    if (StringUtil.IsInList(strStyle, strStyles) == true
                        || strStyle == "<all>") // 2007/9/13 new add // 注：后来发现内核本来就支持<all>的from后，这里就没有必要了，但是代码仍保留
                    {
                        Caption tempCaption = db.Froms[j].GetCaption(strLang);
                        if (tempCaption == null)
                        {
                            // 防范没有找到的情形
                            tempCaption = db.Froms[j].GetCaption(null); // 获得中立语言的caption
                            if (tempCaption == null)
                            {
                                throw new Exception("数据库 '" + db.Captions[0].Value + "' 中没有找到下标为 " + j.ToString() + " 的From事项的任何Caption");
                            }
                        }

                        // 全部路径情况下，要不包含"__id"途径
                        if (strStyle == "<all>"
                            && tempCaption.Value == "__id")
                            continue;

#if NO
                        if (strResult != "")
                            strResult += ",";

                        strResult += tempCaption.Value;
#endif
                        results.Add(tempCaption.Value);
                    }
                }

            }

            // return strResult;

            StringUtil.RemoveDupNoSort(ref results);
            return StringUtil.MakePathList(results);
        }

        // 根据style值去重
        // 没有style值的from事项要丢弃
        public static void RemoveDupByStyle(ref List<From> target)
        {
            for (int i = 0; i < target.Count; i++)
            {
                From from1 = target[i];

                // 把styles为空的事项丢弃
                if (String.IsNullOrEmpty(from1.Styles) == true)
                {
                    target.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < target.Count; j++)
                {
                    From from2 = target[j];

                    if (from1.Styles == from2.Styles)
                    {
                        target.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        // 根据caption去重(按照特定语种caption来去重)
        public static void RemoveDupByCaption(ref List<From> target,
            string strLang = "zh")
        {
            if (string.IsNullOrEmpty(strLang) == true)
                strLang = "zh";

            for (int i = 0; i < target.Count; i++)
            {
                From from1 = target[i];

                List<Caption> captions1 = from1.GetCaptions(strLang);
                // 把caption(特定语种)为空的事项丢弃
                if (captions1 == null || captions1.Count == 0)
                {
                    target.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < target.Count; j++)
                {
                    From from2 = target[j];
                    List<Caption> captions2 = from2.GetCaptions(strLang);

                    if (IsSame(captions1, captions2) == true)
                    {
                        target.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        // 判断两个caption和集中是否有共同的值
        static bool IsSame(List<Caption> captions1, List<Caption> captions2)
        {
            foreach (Caption caption1 in captions1)
            {
                foreach (Caption caption2 in captions2)
                {
                    if (caption1.Value == caption2.Value)
                        return true;
                }
            }

            return false;
        }

        // 2014/8/29
        // 获得一个 from 的 stylelist
        public string GetFromStyles(string strDbName,
    string strFrom,
    string strLang)
        {
            KernelDbInfo db = this.FindDb(strDbName);

            if (db == null)
                return null;

            // 遍历当前数据库的所有form
            foreach (From from in db.Froms)
            {
                // 注: 同一个语言代码的 caption，也可能有不止一个
                List<Caption> captions = from.GetCaptions(strLang);
                if (captions == null || captions.Count == 0)
                {
                    // 防范没有找到的情形
                    Caption tempCaption = from.GetCaption(null); // 获得中立语言的caption
                    if (tempCaption == null)
                        throw new Exception("数据库 '" + db.Captions[0].Value + "' 中没有找到From事项 " + from.ToString() + " 的任何Caption");

                    captions.Add(tempCaption);
                }

                foreach (Caption caption in captions)
                {
                    if (caption.Value == strFrom)
                        return from.Styles;
                }
#if NO
                Caption tempCaption = from.GetCaption(strLang);
                if (tempCaption == null)
                {
                    // 防范没有找到的情形
                    tempCaption = from.GetCaption(null); // 获得中立语言的caption
                    if (tempCaption == null)
                    {
                        throw new Exception("数据库 '" + db.Captions[0].Value + "' 中没有找到From事项 "+from.ToString()+" 的任何Caption");
                    }
                }

                if (tempCaption.Value == strFrom)
                    return from.Styles;
#endif
            }

            return null;
        }
    }


    // 描述了内核数据库的必要信息
    public class KernelDbInfo
    {
        // 和语言有关的名称
        public List<Caption> Captions = null;

        public List<From> Froms = null;

        public ResInfoItem[] db_dir_result = null;

        // 从db_dir_result数组找到针对特定数据库名的事项
        public static ResInfoItem GetDbItem(
    ResInfoItem[] root_dir_results,
    string strDbName)
        {
            for (int i = 0; i < root_dir_results.Length; i++)
            {
                ResInfoItem info = root_dir_results[i];

                if (info.Type != ResTree.RESTYPE_DB)
                    continue;

                if (info.Name == strDbName)
                    return info;

            }

            return null;
        }

        // 找到一个caption值(不论语言类型)
        public bool MatchCaption(string strCaption)
        {
            if (this.Captions == null)
                return false;

            for (int i = 0; i < this.Captions.Count; i++)
            {
                if (strCaption == this.Captions[i].Value)
                    return true;
            }

            return false;
        }

        public int Initial(
            string [] names,
            ResInfoItem[] db_dir_result,
            out string strError)
        {
            strError = "";

            List<Caption> captions = null;
            int nRet = BuildCaptions(names,
                out captions,
                out strError);
            if (nRet == -1)
                return -1;

            this.Captions = captions;


            this.Froms = new List<From>();

            for (int i = 0; i < db_dir_result.Length; i++)
            {
                ResInfoItem info = db_dir_result[i];
                if (info.Type != ResTree.RESTYPE_FROM)
                    continue;

                From from = new From();

                if (info.Names != null)
                {
                    captions = null;
                    nRet = BuildCaptions(info.Names,
                        out captions,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    from.Captions = captions;
                }
                else
                {
                    if (String.IsNullOrEmpty(info.Name) == true)
                    {
                        strError = "出现了一个ResInfoItem事项，Names和Name均为空，这是不合法的";
                        return -1;
                    }
                    // 加入一个语言中立的名字
                    from.Captions = new List<Caption>();
                    from.Captions.Add(new Caption(null, info.Name));
                }

                from.Styles = info.TypeString;

                this.Froms.Add(from);

            }

            return 0;
        }

        // 根据从服务器获得的名称数组，构造为本类可以使用的List<Caption>类型
        public static int BuildCaptions(string[] names,
            out List<Caption> captions,
            out string strError)
        {
            strError = "";
            captions = new List<Caption>();

            for (int i = 0; i < names.Length; i++)
            {
                string strName = names[i];

                int nRet = strName.IndexOf(':');
                string strLang = "";
                string strValue = "";

                if (nRet != -1)
                {
                    strLang = strName.Substring(0, nRet);
                    strValue = strName.Substring(nRet + 1);
                }
                else
                {
                    strLang = strName;
                    strValue = "";
                    strError = "出现错误的事项 '" + strName + "'，中间缺乏冒号 ";
                    return -1;
                }

                Caption caption = new Caption(strLang,
                    strValue);

                captions.Add(caption);
            }

            return 0;
        }
    }


    // 一个带有语种标志的名称
    public class Caption
    {
        public string Lang = "";
        public string Value = "";

        public Caption(string strLang,
            string strValue)
        {
            this.Lang = strLang;
            this.Value = strValue;
        }
    }

    public class From
    {
        public List<Caption> Captions = null;

        // 风格
        public string Styles = "";

        // 2012/2/8
        // 返回左端匹配或者完全匹配的所有Caption
        public List<Caption> GetCaptions(string strLang)
        {
            List<Caption> results = new List<Caption>();
            for (int i = 0; i < this.Captions.Count; i++)
            {
                Caption caption = this.Captions[i];

                if (String.IsNullOrEmpty(strLang) == true)  // 如果给出的语言为未知, 则直接返回第一个caption
                {
                    results.Add(caption);
                    return results;
                }

                int nRet = CompareLang(caption.Lang, strLang);

                if (nRet == 2 // 匹配得2分
                    || String.IsNullOrEmpty(caption.Lang) == true)  // 或者为中立语言
                    results.Add(caption);
                else if (nRet == 1)
                    results.Add(caption);
            }

            return results; 
        }


        public Caption GetCaption(string strLang)
        {
            Caption OneCaption = null;  // 匹配中仅仅得1分的第一个对象
            for (int i = 0; i < this.Captions.Count; i++)
            {
                Caption caption = this.Captions[i];

                if (String.IsNullOrEmpty(strLang) == true)  // 如果给出的语言为未知, 则直接返回第一个caption
                    return caption;

                int nRet = CompareLang(caption.Lang, strLang);

                if (nRet == 2 // 匹配得2分
                    || String.IsNullOrEmpty(caption.Lang) == true)  // 或者为中立语言
                    return caption;

                if (nRet == 1 && OneCaption == null)
                    OneCaption = caption;
            }

            return OneCaption;  // 如果有1分匹配的话
        }

        // 比较两个语言代码
        // 所谓语言代码, 是类似"zh-cn"这样的字符串。后面可以省略。
        // return:
        //      0   不匹配
        //      1   左段匹配，但是右段不匹配
        //      2   两段均匹配
        static int CompareLang(string strRequest,
            string strValue)
        {
            if (String.IsNullOrEmpty(strRequest) == true
                && String.IsNullOrEmpty(strValue) == true)
                return 2;

            if (String.IsNullOrEmpty(strRequest) == true
                || String.IsNullOrEmpty(strValue) == true)
                return 0;

            string strRequestLeft = "";
            string strRequestRight = "";

            SplitLang(strRequest,
                out strRequestLeft,
                out strRequestRight);

            string strValueLeft = "";
            string strValueRight = "";

            SplitLang(strValue,
                out strValueLeft,
                out strValueRight);

            if (strRequestLeft == strValueLeft
                && strRequestRight == strValueRight)
                return 2;
            if (strRequestLeft == strValueLeft)
                return 1;

            return 0;
        }

        static void SplitLang(string strLang,
            out string strLangLeft,
            out string strLangRight)
        {
            strLangLeft = "";
            strLangRight = "";

            int nRet = strLang.IndexOf("-");
            if (nRet == -1)
                strLangLeft = strLang;
            else
            {
                strLangLeft = strLang.Substring(0, nRet);
                strLangRight = strLang.Substring(nRet + 1);
            }
        }
    }
}
