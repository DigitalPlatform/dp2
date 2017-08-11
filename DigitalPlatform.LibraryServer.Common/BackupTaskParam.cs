using DigitalPlatform.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryServer.Common
{
    public class BackupTaskStart
    {
        // 数据库名列表。逗号分隔的字符串
        public string DbNameList { get; set; }
        // 备份文件名。没有路径部分的纯文件名
        public string BackupFileName { get; set; }

        public static BackupTaskStart FromString(string strText)
        {
            BackupTaskStart param = new BackupTaskStart();

            if (String.IsNullOrEmpty(strText) == true)
                return param;

            Hashtable table = StringUtil.ParseParameters(strText);

            param.BackupFileName = (string)table["backupfilename"];

            if (strText == "continue")
            {
                if (string.IsNullOrEmpty(param.BackupFileName) == false)
                    throw new Exception("当 strText 为 'continue' 时，不应该同时使用 backupfilename 参数");

                param.DbNameList = "continue";
            }
            else
                param.DbNameList = (string)table["dbnamelist"];
            if (string.IsNullOrEmpty(param.DbNameList) == false)
                param.DbNameList = param.DbNameList.Replace("|", ",");

            return param;
        }

        public string ToString()
        {
            string strDbNameList = this.DbNameList;

            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace(",", "|");

            Hashtable table = new Hashtable();
            table["backupfilename"] = this.BackupFileName;
            table["dbnamelist"] = strDbNameList;

            return StringUtil.BuildParameterString(table);
        }

        public static string GetDefaultBackupFileName()
        {
            return DateTime.Now.ToString("u").Replace(":", "_").Replace(" ", "_") + ".dp2bak";
        }
    }
}
