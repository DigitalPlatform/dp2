using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 和(同步及)创建报表有关的功能
    /// </summary>
    public partial class LibraryApplication
    {
        // 用于存储报表数据库的 MySQL Server 账户
        internal string _reportStorageServerType = "";
        internal string _reportStorageServerName = "";
        internal string _reportStorageUserId = "";
        internal string _reportStoragePassword = "";
        internal string _reportStorageDatabaseName = "";

        // 用于同步的 dp2library 账户
        internal string _reportReplicationServer = "";
        internal string _reportReplicationUserName = "";
        internal string _reportReplicationPassword = "";

        internal string DecryptReportPassword(string password)
        {
            // password可能为空
            try
            {
                return Cryptography.Decrypt(password,
                        "dp2003");
            }
            catch
            {
                /*
                strError = "服务器配置文件不合法，根元素下级的<datasource>定义'password'属性值不合法。";
                return -1;
                */
                return "errorpassword";
            }
        }

        internal string EncryptReportPassword(string text)
        {
            return Cryptography.Encrypt(text, "dp2003");
        }
    }
}
