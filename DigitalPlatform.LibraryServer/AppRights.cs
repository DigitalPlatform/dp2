using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和账户权限有关的功能
    /// </summary>
    public partial class LibraryApplication
    {
        public static string[] danger_rights = new string[] { 
        "batchtask",
        "clearalldbs",
        "devolvereaderinfo",
        "changeuser",
        "newuser",
        "deleteuser",
        "changeuserpassword",
        "simulatereader",
        "simulateworker",
        "setsystemparameter",
        "urgentrecover",
        "repairborrowinfo",
        "settlement",
        "undosettlement",
        "deletesettlement",
        "writerecord",
        "managedatabase",
        "restore",
        "managecache",
        "managechannel",
        "upload",
        "bindpatron",
        };

        // 对账户权限进行过滤，去掉一些危险的权限
        // parameters:
        //      source  待过滤的权限列表
        //      limit   用于限制的权限列表。即，source 里面的危险权限，要和 limit 里面的权限做交叉运算，共同的才保留下来。即 source 里面的危险权限不能高于 limit 里面的危险权限集合
        //      removed 返回那些被去掉的权限
        public static string LimitRights(string source, 
            string limit,
            out string removed)
        {
            removed = "";

            List<string> results = new List<string>();
            List<string> removed_results = new List<string>();
            List<string> list = StringUtil.SplitList(source);
            foreach(string right in list)
            {
                if (Array.IndexOf(danger_rights, right) != -1)
                {
                    if (StringUtil.IsInList(right, limit) == true)
                        results.Add(right);
                    else
                        removed_results.Add(right);
                    continue;
                }

                results.Add(right);
            }

            removed = StringUtil.MakePathList(removed_results);
            return StringUtil.MakePathList(results);
        }
    }
}
