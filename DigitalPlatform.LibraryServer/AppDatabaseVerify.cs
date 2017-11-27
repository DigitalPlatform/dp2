using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.LibraryServer.Common;

namespace DigitalPlatform.LibraryServer
{
    public partial class LibraryApplication
    {
        // parameters:
        //      cfg_dom 装入了 library.xml 的 XmlDocument 对象
        //      strDbName   被删除的数据库名
        // return:
        //      -1  验证过程出现错误(也就是说验证过程没有来的及完成)
        //      0   验证发现不正确
        //      1   验证发现正确
        public int VerifyDatabaseDelete(
    string strDbType,
    string strDbName,
    out string strError)
        {
            int nRet = ServerDatabaseUtility.VerifyDatabaseDelete(this.LibraryCfgDom,
                strDbType,
                strDbName,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strDbType) == true)
            {
                strError = "strDbType 参数值不应为空";
                return -1;
            }
            if (string.IsNullOrEmpty(strDbName) == true)
            {
                strError = "strDbName 参数值不应为空";
                return -1;
            }

            // 实用库
            // 不用再验证。因为数据库名信息都在 LibraryCfgDom 里面了，上面已经验证过了

            string strDbTypeCaption = ServerDatabaseUtility.GetTypeCaption(strDbType);

            // 单个数据库
            if (ServerDatabaseUtility.IsSingleDbType(strDbType))
            {
                string dbname = GetSingleDbName(strDbType);
                if (string.IsNullOrEmpty(dbname) == false)
                {
                    strError = "删除完成后，" + strDbTypeCaption + "库名 '" + strDbName + "' 在 LibraryApplication 的 相应成员(.???DbName)内没有清理干净";
                    return 0;
                }
            }

            // 书目库
            if (strDbType == "biblio")
            {
                // TODO: 在内存结构里面验证
            }

            // 单个书目库下属库
            if (ServerDatabaseUtility.IsBiblioSubType(strDbType) == true)
            {
                // TODO: 在内存结构里面验证
            }

            return 1;
        }
    }
}
