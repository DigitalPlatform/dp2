﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitalPlatform.Text
{
    public class Location
    {
        public string Name = "";    // 馆藏地点名
        public string RefID = "";   // 参考ID。如果为空，就表示没有被checked
    }

    public class LocationCollection : List<Location>
    {
        // 解析馆藏地名字字符串
        // 例如“海淀分馆/阅览室”拆分为左右两个部分。“阅览室”会被认为是第二部分；"海淀分馆/"会被人为是第一部分
        public static void ParseLocationName(string strName,
    out string strLibraryCode,
    out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }

        // 获得用到的馆代码字符串列表。已经归并排序。可以用于互相比较
        public List<string> GetUsedLibraryCodes()
        {
            List<string> librarycodes = new List<string>();
            foreach (Location item in this)
            {
                ParseLocationName(item.Name,
                    out string strLibraryCode,
                    out string strPureName);
                librarycodes.Add(strLibraryCode);
            }

            librarycodes.Sort();
            RemoveDup(ref librarycodes);
            return librarycodes;
        }

        // 获得用到的馆藏地点字符串列表。已经归并排序。可以用于互相比较
        public List<string> GetUsedLocations()
        {
            List<string> locations = new List<string>();
            foreach (Location item in this)
            {
                locations.Add(item.Name);
            }

            locations.Sort();
            RemoveDup(ref locations);
            return locations;
        }

        // 把一个字符串数组去重。调用前，应当已经排序
        static void RemoveDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }
        }

        // 去除字符串末尾的一个逗号
        public static string RemoveTailComma(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            if (strText[strText.Length - 1] == ',')
                return strText.Substring(0, strText.Length - 1);

            return strText;
        }

        // 列表中的id是否全部为空?
        public static bool IsEmptyIDs(string strIDs)
        {
            if (String.IsNullOrEmpty(strIDs) == true)
                return true;

            string[] ids = strIDs.Split(new char[] { ',' });
            for (int i = 0; i < ids.Length; i++)
            {
                string strText = ids[i];
                if (String.IsNullOrEmpty(strText) == false)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return this.ToString(true);
        }

        // 获得已到复本数
        // 也就是refid不为空的事项个数
        // 对于套内多册的验收情况，refid用竖线隔开，形成一组。本函数返回的应该理解为套数，不是册数。但套内可能验收不足
        public virtual int GetArrivedCopy()
        {
            int nValue = 0;
            for (int i = 0; i < this.Count; i++)
            {
                Location item = this[i];

                if (String.IsNullOrEmpty(item.RefID) == false)
                    nValue++;
            }

            return nValue;
        }

        // 统计一个馆藏地分配的册数
        public virtual int GetLocationCopy(string location)
        {
            int nValue = 0;
            foreach (Location item in this)
            {
                if (item.Name == location)
                    nValue++;
            }

            return nValue;
        }


        // 2012/12/22
        // 返回全部 RefID
        public virtual List<string> GetRefIDs()
        {
            List<string> results = new List<string>();
            foreach (Location location in this)
            {
                if (String.IsNullOrEmpty(location.RefID) == true)
                    continue;

                string[] parts = location.RefID.Split(new char[] { '|' });
                results.AddRange(parts);
            }

            return results;
        }

        // paremters:
        //      bOutputID   是否输出ID部分
        public virtual string ToString(bool bOutputID)
        {
            string strResult = "";
            string strPrevLocationString = null;
            int nPartCount = 0;
            string strIDs = "";
            for (int i = 0; i < this.Count; i++)
            {
                Location item = this[i];

                if (item.Name == strPrevLocationString)
                {
                    nPartCount++;
                    strIDs += item.RefID + ",";
                }
                else
                {
                    if (strPrevLocationString != null)
                    {
                        if (strResult != "")
                            strResult += ";";
                        strResult += strPrevLocationString + ":" + nPartCount.ToString();

                        if (bOutputID == true)
                        {
                            if (IsEmptyIDs(strIDs) == false)
                                strResult += "{" + RemoveTailComma(strIDs) + "}";
                        }

                        nPartCount = 0;
                        strIDs = "";
                    }

                    nPartCount++;
                    strIDs += item.RefID + ",";
                }

                strPrevLocationString = item.Name;
            }

            if (nPartCount != 0)
            {
                if (strResult != "")
                    strResult += ";";
                strResult += strPrevLocationString + ":" + nPartCount.ToString();
                if (bOutputID == true)
                {
                    if (IsEmptyIDs(strIDs) == false)
                        strResult += "{" + RemoveTailComma(strIDs) + "}";
                }

            }

            return strResult;
        }

        public int Build(string value,
            out string strError)
        {
            strError = "";
            this.Clear();

            if (string.IsNullOrEmpty(value))    // 2018/5/9
                return 0;

            string[] sections = value.Split(new char[] { ';' });
            for (int i = 0; i < sections.Length; i++)
            {
                string strSection = sections[i].Trim();
                if (String.IsNullOrEmpty(strSection) == true)
                    continue;

                string strIDs = ""; // 已验收id列表

                string strLocationString = "";
                int nCount = 0;
                int nRet = strSection.IndexOf(":");
                if (nRet == -1)
                {
                    strLocationString = strSection;
                    nCount = 1;
                }
                else
                {
                    strLocationString = strSection.Substring(0, nRet).Trim();
                    string strCount = strSection.Substring(nRet + 1);

                    nRet = strCount.IndexOf("{");
                    if (nRet != -1)
                    {
                        strIDs = strCount.Substring(nRet + 1).Trim();

                        if (strIDs.Length > 0 && strIDs[strIDs.Length - 1] == '}')
                            strIDs = strIDs.Substring(0, strIDs.Length - 1);

                        strCount = strCount.Substring(0, nRet).Trim();
                    }

                    try
                    {
                        nCount = Convert.ToInt32(strCount);
                    }
                    catch
                    {
                        strError = "馆藏分配片段 '" + strLocationString + "' 中 '" + strCount + "' 部分应为纯数字";
                        return -1;
                    }

                    if (nCount > 1000)
                    {
                        strError = "数字太大，超过1000";
                        return -1;
                    }
                }

                for (int j = 0; j < nCount; j++)
                {
                    Location item = new Location();
                    item.Name = strLocationString;
                    this.Add(item);
                }

                if (string.IsNullOrEmpty(strIDs) == false)
                {
                    string[] ids = strIDs.Split(new char[] { ',' });

                    int nStartBase = this.Count - nCount;
                    for (int k = 0; k < nCount; k++)
                    {
                        Location item = this[nStartBase + k];

                        if (k >= ids.Length)
                            break;

                        string strID = ids[k];

                        if (String.IsNullOrEmpty(strID) == true)
                        {
                            // item.Arrived = false;
                            continue;
                        }

                        item.RefID = strID;
                    }
                } // end of if
            } // end of i

            return 0;
        }

        // 合并两个馆藏地点字符串
        public static int MergeTwoLocationString(string strLocationString1,
            string strLocationString2,
            bool bOutputID,
            out string strLocationString,
            out string strError)
        {
            strError = "";
            strLocationString = "";

            LocationCollection items_1 = new LocationCollection();
            int nRet = items_1.Build(strLocationString1,
                out strError);
            if (nRet == -1)
                return -1;

            LocationCollection items_2 = new LocationCollection();
            nRet = items_2.Build(strLocationString2,
                out strError);
            if (nRet == -1)
                return -1;

            LocationCollection items = new LocationCollection();
            items.AddRange(items_1);
            items.AddRange(items_2);

            // 归并。让相同的事项靠近。和排序不同，它不改变已有的基本的序。
            // return:
            //      0   unchanged
            //      1   changed
            items.Merge();

            strLocationString = items.ToString(bOutputID);

            return 0;
        }

        // 2008/8/29
        // 归并。让相同的事项靠近。和排序不同，它不改变已有的基本的序。
        // return:
        //      0   unchanged
        //      1   changed
        public int Merge()
        {
            bool bChanged = false;
            for (int i = 0; i < this.Count;)
            {
                Location item = this[i];

                string strLocationString = item.Name;
                int nTop = i + 1;
                for (int j = i + 1; j < this.Count; j++)
                {
                    Location comp_item = this[j];
                    if (comp_item.Name == strLocationString)
                    {
                        // 拉到最近位置(其余被推后)
                        if (j != nTop)
                        {
                            Location temp = this[j];
                            this.RemoveAt(j);
                            this.Insert(nTop, temp);
                            bChanged = true;
                        }

                        nTop++;
                    }

                }

                i = nTop;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 刷新所有 refid
        // parameters:
        //      table   参考 ID 替换表。旧参考 ID --> 新参考 ID
        //              如果在 table 中查到了对照关系，则用它来替换已有的参考 ID；如果没有查到，则替换新的参考 ID 后加入这一个对照关系
        // return:
        //      被替换的参考 ID 个数
        public int RefreshRefIDs(ref Hashtable table)
        {
            int count = 0;
            foreach (Location location in this)
            {
                if (string.IsNullOrEmpty(location.RefID) == false)
                {
                    string old_refid = location.RefID;
                    string new_refid = (string)table[old_refid];
                    if (string.IsNullOrEmpty(new_refid) == false)
                        location.RefID = new_refid;
                    else
                    {
                        location.RefID = Guid.NewGuid().ToString();
                        table[old_refid] = location.RefID;
                    }
                    count++;
                }
            }

            return count;
        }

        // 从 source 中复制 count 个元素构成一个新的 collection
        // 注: 若 source 中元素少于 count，则有多少复制多少
        // 注：本函数为浅复制。即结果集合中的元素也是源集合中的元素
        public static LocationCollection CopyItems(LocationCollection source, int count)
        {
            LocationCollection result = new LocationCollection();
            int i = 0;
            foreach (Location location in source)
            {
                result.Add(location);
                i++;
                if (i >= count)
                    break;
            }

            return result;
        }

        // 确保集合至少有 count 个元素
        // 注：本函数为浅复制。即结果集合中的元素也是源集合中的元素
        public void EnsureItems(
            int count,
            string default_name)
        {
            if (this.Count >= count)
                return;

            int delta = count - this.Count;
            for (int i = 0; i < delta; i++)
            {
                Location location = new Location();
                location.Name = default_name;
                this.Add(location);
            }

        }
    }
}
