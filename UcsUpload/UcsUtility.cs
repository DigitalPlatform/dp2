using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace UcsUpload
{
    // 一些实用工具函数
    public static class UcsUtility
    {
        /*
校验码 =MD5(USER_NAME+StaticKey+Date)
其中：USER_NAME 为工作人员代码 (<=10
位)
StaticKey=” UnionC#ata@2012(11)07”
(21 位长度)
Date=YYYYMMDD (8 位数字)
10 XML_FULL_REQ 必备 MARCXML 或 OAIXML 格式的书目数据
        * */
        // 构造序列号
        public static string BuildSerialNumber(string userName)
        {
            string staticKey = "UnionC#ata@2012(11)07";
            string date = DateTimeUtil.DateTimeToString8(DateTime.Now);
            return StringUtil.GetMd5(userName + staticKey + date).ToUpper();
        }

        // parameters:
        //      url             http://202.96.31.28/X
        //      databaseName    数据库名。"UCS01"。测试时临时用 "UCS03"
        //      lang            语言代码。"chi"
        //      action          动作。"N" 或者 "U"，分别表示新建和更新
        //      schema          可选 MARCXML 或 OAIXML
        //      format          可选 UCS 记录上载的记录格式(FMT)定义，默认 BK
        //      serialNumber    校验码 =MD5(USER_NAME+StaticKey+Date)
        public static async Task<UploadResult> Upload(
            string url,
            string databaseName,
            string lang,
            string userName,
            string password,
            string action,
            string serialNumber,
            string record,
            string schema = null,
            string format = null)
        {
            if (serialNumber == null)
                serialNumber = BuildSerialNumber(userName);

            var httpClient = new HttpClient();
            var table = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(
                "op", "update-doc-ucs"
                ),
                new KeyValuePair<string, string>(
                "LIBRARY", databaseName
                ),
                new KeyValuePair<string, string>(
                "CON_LNG", lang
                ),
                new KeyValuePair<string, string>(
                "USER_NAME", userName
                ),
                new KeyValuePair<string, string>(
                "USER_PASSWORD", password
                ),
                new KeyValuePair<string, string>(
                "ACTION", action
                ),
                new KeyValuePair<string, string>(
                "SN", serialNumber
                ),
                new KeyValuePair<string, string>(
                "XML_FULL_REQ", record
                )};

            if (schema != null)
                table.Add(new KeyValuePair<string, string>(
                    "XML_SCHEMA", schema
                    ));
            if (format != null)
                table.Add(new KeyValuePair<string, string>(
                    "DOC_FORMAT", format
                    ));

            var formContent = new FormUrlEncodedContent(table);

            if (url == null)
                url = "http://202.96.31.28/X";

            try
            {
                string resultXml = null;
                using (HttpResponseMessage response = await httpClient.PostAsync(
                    url,
                    formContent))
                {
                    response.EnsureSuccessStatusCode();

                    resultXml = await response.Content.ReadAsStringAsync();
                }

                /*
    书目数据上载成功后的输出项：
    <?xml version = "1.0" encoding = "UTF-8"?>
    <update-doc-ucs>
    <olcc>$$aA420000HBT$$bUCS01005467903$$c002157898</olcc>
    <session-id>JBTDH8NHA5DT854EP8FCS2V6QVKN99CPU9UNPNHTU49G6GVH8U</sessi
    on-id>
    </update-doc-ucs>
    书目数据上载失败的错误提示：
    <?xml version = "1.0" encoding = "UTF-8"?>
    <update-doc-ucs>
    <error>27:上传记录与 UCS 系统记录重复</error>
    <session-id>CV288J9YG7ME79224SJXLQCT8RH4JDVA8BJ7D2FVSC7IENTHY8</sessi
    on-id>
    </update-doc-ucs>

    <?xml version = "1.0" encoding = "UTF-8"?>
    <login>
    <error>User name WWW-X does not exist.</error>
    </login>
                * 
                 * */

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(resultXml);

                var result = new UploadResult();
                result.login = DomUtil.GetElementText(dom.DocumentElement,
                    "login");
                result.error = DomUtil.GetElementText(dom.DocumentElement,
                    "error");
                result.olcc = DomUtil.GetElementText(dom.DocumentElement,
                    "olcc");
                result.sessionid = DomUtil.GetElementText(dom.DocumentElement,
                    "session-id");
                return result;
            }
            catch (Exception ex)
            {
                return new UploadResult
                {
                    Value = -1,
                    ErrorCode = ex.GetType().ToString(),
                    ErrorInfo = ex.Message
                };
            }
        }

        public class UploadResult : NormalResult
        {
            // 当接口出现账号权限的问题时，错误反馈在这个子元素中
            public string login { get; set; }

            // 当上载失败时显示，显示内容为“错误代码:错误信息”
            /*
错误代码与错误内容列表
01:用户名 $1 不存在。
02:用户名错误 $1.
03:用户被拒绝使用'$1/$2'功能.
04:用户被拒绝使用$3 子库中的$1/$2 功能.
05:库 $1 未在服务器上定义。
06:库无法访问。
07:库未在服务器上定义.房
08:你的 GUI 配置不支持 Aleph 登录
09:用户和口令与指定库不匹配
10:可退回的
11:不可退回的
12:没有提供者
13:没有请求者的 ID 名称
46:本地用户更新成功 - 无法更新中枢用户($1).
98:$1 秒后服务器执行请求失败
99:服务 $1 未定义
87:GUI 组件 $1 未被许可
88:超出许可限制
89:警告: 许可将在$1 天内过期.
92:记录由另一用户锁定
第 8 页
97:$1 模块临时性关闭了
11:上传操作参数错误(N/U)
12:获取上传数据出错
15:验证码参数错误
81:上传库代码必备
21:获取上传记录数据出错
22:新增上传记录数据含有 049 字段的$a 或$b 字段
23:更新记录数据没有 049 字段的$c 字段
24:上传记录数据含有多个 049 字段
25:上传记录数据含有多个 910 字段
26:上传记录数据 049 字段对应的记录不存在
27:上传记录与 UCS 系统记录重复
28:910 字段$$a 为必备子字段
29:记录更新权限核查未通过
30:机构权限核查未通过或缺少 200b 字段
31:上传记录数据质量核查未通过，请着重检查 010$a 的 ISBN 号是否正确
81:必须指定上传库代码
82:获取上传记录数据出错
            * */
            public string error { get; set; }

            // 当上载成功时显示，显示内容中心为上传书目数据赋予的联编中心唯一控制号
            public string olcc { get; set; }
            public string sessionid { get; set; }

            public bool AreUcsSucceed
            {
                get
                {
                    if (string.IsNullOrEmpty(olcc) == false)
                        return true;
                    return false;
                }
            }

            public string UcsErrorInfo
            {
                get
                {
                    List<string> errors = new List<string>();
                    if (string.IsNullOrEmpty(login) == false)
                        errors.Add(login);
                    if (string.IsNullOrEmpty(error) == false)
                        errors.Add(error);
                    return StringUtil.MakePathList(errors, "; ");
                }
            }
        }


        static string[] _price_units = new string[] { 
        "CNY",
        "USD",
        };

        // 校验 MARC 记录内容是否合法
        public static NormalResult VerifyRecord(MarcRecord record)
        {
            List<string> errors = new List<string>();

            var subfields_010d = record.select("field[@name='010']/subfield[@name='d']");
            foreach (MarcSubfield subfield in subfields_010d)
            {
                var content = subfield.Content;

                // 010$d 中货币单位应该是 CNY
                int nRet = PriceUtil.ParsePriceUnit(content,
                    out string prefix,
                    out string value,
                    out string postfix,
                    out string strError);
                if (nRet == -1)
                {
                    errors.Add($"拆分金额字符串 '{content}' 的过程中出错: {strError}");
                    continue;
                }

                if (_price_units.Contains(prefix) == false)
                    errors.Add($"货币名称 '{prefix}' 不合法");
            }

            // 606$a 中不应该有 -
            var subfields_606a = record.select("field[@name='606']/subfield[@name='a']");
            foreach (MarcSubfield subfield in subfields_606a)
            {
                if (subfield.Content.Contains("-"))
                    errors.Add($"606$a 中不应该包含横杠字符");
            }

            if (errors.Count > 0)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = StringUtil.MakePathList(errors, "; ")
                };
            }

            return new NormalResult();
        }
    }
}
