using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;

using DigitalPlatform.Interfaces;
using DigitalPlatform.Xml;

namespace DongshifangMessageInterface
{
    public class DongshifangMessageHost : ExternalMessageHost
    {
        // return:
        //      -1  发送失败
        //      0   没有必要发送
        //      1   发送成功
        public override int SendMessage(
            string strReaderBarcode,
            string strReaderXml,
            string strMessageText,
            string strLibraryCode,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strReaderXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者记录 XML 装入 DOM 出错: " + ex.Message;
                return -1;
            }

            // 获得电话号码
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            if (string.IsNullOrEmpty(strTel) == true)
                return 0;

            // 提取出手机号
            string[] tels = strTel.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> mobiles = new List<string>();
            foreach (string tel in tels)
            {
                string strText = tel.Trim();
                if (strText.Length == 11)
                    mobiles.Add(strTel);
            }

            if (mobiles.Count == 0)
                return 0;

            foreach (string tel in mobiles)
            {
                int nRet = SendMessage(tel,
                    strMessageText,
                    strLibraryCode,
                    out strError);
                if (nRet <= 0)
                {
                    strError = "发送出错，错误码 [" + nRet.ToString() + "]";
                    return -1;
                }

                break;
            }

            return 1;
        }

        // 发送一条消息。可以大于 70 字，分为多条发送
        public static int SendMessage(string strTel,
            string strMessage,
            string strLibraryCode,
            out string strError)
        {
            if (string.IsNullOrEmpty(strTel) == true
                || string.IsNullOrEmpty(strMessage) == true)
            {
                strError = "strMessage 和 strTel 参数不能为空";
                return -1;
            }

            ConfigParam param = null;

            try
            {
                param = ConfigParam.LoadConfig(strLibraryCode);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }


            if (strMessage.Length <= param.nMaxChars)
            {
                return SendOneMessage(
                    param,
                    strTel,
                    strMessage,
                    strLibraryCode,
                    out strError);
            }

            int nCount = 0;
            for (; ; )
            {
                int nLength = strMessage.Length;
                if (nLength > param.nMaxChars)
                    nLength = param.nMaxChars;
                string strPart = strMessage.Substring(0, nLength);
                int nRet = SendOneMessage(
                    param,
                    strTel,
                    strPart,
                    strLibraryCode,
                    out strError);
                if (nRet <= 0)
                    return nRet;
                nCount += nRet;

                if (strMessage.Length <= nLength)
                    break;
                strMessage = strMessage.Substring(nLength);
            }

            return nCount;
        }

        /*
 * app.config 需要这样配置
<?xml version="1.0"?>
<configuration>
    <library 
         code="" 
         eid="企业编号" 
         uid="用户名" 
         pwd="密码"
         max_chars='62'/>

</configuration>         * 
 * */
        /* 旧用法，已经废止
         * app.config 需要这样配置
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <add key="eid" value="企业编号"/>
    <add key="uid" value="用户名"/>
    <add key="pwd" value="密码"/>
  </appSettings>
</configuration>         * 
         * */
        /*
返回值参对表

编号              值                         说明 
1               大于0              发送成功,此次发送成功条数 
2                -1                 参数无效 
3                -2                 通道不存在或者当前业务不支持此通道 
4                -3                 定时格式错误 
5                -4                 接收号码无效 
6                -5                 提交号码个数超过上限,最多100条
7                -6                 短信内容长度不符合要求,普通短信70字 
8                -7                 当前账户余额不足 
9                -8                 网关发送短信时出现异常 
10               -9                 用户或者密码没输入 
11               -10                企业ID或者会员账号不存在 
12               -11                密码错误 
13               -12                账户锁定 
14               -13                网关状态关闭 
15               -14                验证用户时执行异常 
16               -15                网关初始化失败 
17               -16                当前IP已被系统屏蔽,密码失败次数太多 
18               -17                发送异常 
19               -18                账号未审核 
20               -19               当前时间不允许发送短信 
21               -20               传输密钥未设置，请登陆平台设置 
22               -21               提取密钥异常 
23               -22               签名验证失败 
24               -23               发现屏蔽关键字 
25               -100到-199       运营商返回失败代码
         * */
        static int SendOneMessage(
            ConfigParam param,
            string strTel,
            string strMessage,
            string strLibraryCode,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strTel) == true
    || string.IsNullOrEmpty(strMessage) == true)
            {
                strError = "strMessage 和 strTel 参数不能为空";
                return -1;
            }

#if NO
            string strDepCode = System.Configuration.ConfigurationManager.AppSettings["eid"].ToString();
            string strUserID = System.Configuration.ConfigurationManager.AppSettings["uid"].ToString();
            string strPassword = System.Configuration.ConfigurationManager.AppSettings["pwd"].ToString();
#endif

#if NO
            ConfigParam param = null;

            try
            {
                param = ConfigParam.LoadConfig(strLibraryCode);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
#endif

            // http://api.xhsms.com/gb2312/web_api/?x_eid=企业ID&x_uid=账号&x_pwd_md5=登陆密码MD5值&x_ac=10&x_gate_id=300&x_target_no=手机号码&x_memo=短信内容
            string strUrl = "http://api.xhsms.com/gb2312/web_api/?x_eid="
                + param.strDepCode
                + "&x_uid=" + param.strUserID
                + "&x_pwd_md5=" + GetMd5(param.strPassword)
                + "&x_ac=10&x_gate_id=300&x_target_no=" + strTel
                + "&x_memo=" + HttpUtility.UrlEncode(strMessage, Encoding.GetEncoding(936));
            try
            {
                HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(strUrl);
                hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                hr.Method = "GET";
                hr.Timeout = 1 * 60 * 1000;     // 30 * 60 * 1000;
                WebResponse hs = hr.GetResponse();
                Stream sr = hs.GetResponseStream();
                StreamReader ser = new StreamReader(sr, Encoding.Default);
                string strResult = ser.ReadToEnd();
                int v = 0;
                if (int.TryParse(strResult, out v) == false)
                {
                    strError = "HTTP 请求返回字符串 '" + strResult + "'";
                    return -1;
                }
                return v;
            }
            catch (Exception ex)
            {
                strError = "HTTP 调用失败: " + ex.Message;
                return -1;
            }
        }

        static string GetMd5(string strText)
        {
            MD5 hasher = MD5.Create();
            byte[] buffer = Encoding.ASCII.GetBytes(strText);
            byte[] target = hasher.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in target)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }
    }

    class ConfigParam
    {
        public string strDepCode = "";
        public string strUserID = "";
        public string strPassword = "";
        public int nMaxChars = 70;  // 每条短消息的字符数。短信服务商可能给文字加上前缀字符串，这样实际能用的可能少于 70 个字符，需要用参数配置一下

        public static ConfigParam LoadConfig(string strLibraryCode)
        {
            string strDir = AppDomain.CurrentDomain.BaseDirectory;
            string strCfgFile = Path.Combine(strDir, "DongshifangMessageInterface.dll.config");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch
            {
                throw new Exception("配置文件 '" + strCfgFile + "' 装载错误");
            }

            if (dom.DocumentElement == null)
                throw new Exception("配置文件 '" + strCfgFile + "' 缺乏根元素");

            XmlElement node = dom.DocumentElement.SelectSingleNode("library[@code='" + strLibraryCode + "']") as XmlElement;
            if (node == null)
            {
                // 如果找不到特定馆代码的事项，就找 '*' 的事项，这是负责所有未明确配置的馆代码的
                node = dom.DocumentElement.SelectSingleNode("library[@code='*']") as XmlElement;
                if (node == null)
                    throw new Exception("配置文件 '" + strCfgFile + "' 中没有配置馆代码为 '" + strLibraryCode + "' 或者为 '*' 的参数 (library 元素)");
            }

            ConfigParam cfg = new ConfigParam();

            cfg.strDepCode = DomUtil.GetAttr(node, "eid");
            if (string.IsNullOrEmpty(cfg.strDepCode) == true)
                throw new Exception("配置文件中尚未为馆代码 '" + strLibraryCode + "'配置 eid 参数");

            cfg.strUserID = DomUtil.GetAttr(node, "uid");
            if (string.IsNullOrEmpty(cfg.strUserID) == true)
                throw new Exception("配置文件中尚未为馆代码 '" + strLibraryCode + "'配置 uid 参数");

            cfg.strPassword = DomUtil.GetAttr(node, "pwd");
            if (string.IsNullOrEmpty(cfg.strPassword) == true)
                throw new Exception("配置文件中尚未为馆代码 '" + strLibraryCode + "'配置 pwd 参数");

            int nMaxChars = 70;
            string strMaxChars = node.GetAttribute("max_chars");
            if (string.IsNullOrEmpty(strMaxChars) == false)
            {
                if (Int32.TryParse(strMaxChars, out nMaxChars) == false)
                    throw new Exception("配置文件中馆代码 '" + strLibraryCode + "' 所在元素的 max_chars 属性值 '" + strMaxChars + "' 格式错误。应该为纯数字");
            }
            cfg.nMaxChars = nMaxChars;

            return cfg;
        }


#if NO
        public static ConfigParam LoadConfig()
        {

            string strDir = AppDomain.CurrentDomain.BaseDirectory;
            string strExePath = Path.Combine(strDir, "DongshifangMessageInterface.dll");
            Configuration config = null;

            try
            {
                config = ConfigurationManager.OpenExeConfiguration(strExePath);
            }
            catch
            {
                throw new Exception("配置文件 '"+strExePath+".config' 装载错误");
            }

            ConfigParam cfg = new ConfigParam();

            if (config.AppSettings.Settings["eid"] == null)
                throw new Exception("配置文件中尚未配置 eid 参数");
            cfg.strDepCode = config.AppSettings.Settings["eid"].Value;

            if (config.AppSettings.Settings["uid"] == null)
                throw new Exception("配置文件中尚未配置 uid 参数");
            cfg.strUserID = config.AppSettings.Settings["uid"].Value;

            if (config.AppSettings.Settings["pwd"] == null)
                throw new Exception("配置文件中尚未配置 pwd 参数");
            cfg.strPassword = config.AppSettings.Settings["pwd"].Value;

            return cfg;
        }
#endif

    }
}
