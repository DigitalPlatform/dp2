using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform.Interfaces;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Core;
using Aliyun.Acs.Sms.Model.V20160927;
using Aliyun.Acs.Core.Exceptions;

namespace AliMessageInterface
{
    public class AliMessageHost : ExternalMessageHost
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
            string strTel = GetElementText(dom.DocumentElement, "tel");
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
                    strError = "发送出错，错误码 [" + nRet.ToString() + "]，错误信息:" + strError;
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

            {
                IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou",
                    param.AccessKey, 
                    param.AccessSecret);
                IAcsClient client = new DefaultAcsClient(profile);
                SingleSendSmsRequest request = new SingleSendSmsRequest();
                try
                {
                    request.SignName = "管理控制台中配置的短信签名（状态必须是验证通过）";
                    request.TemplateCode = "管理控制台中配置的审核通过的短信模板的模板CODE（状态必须是验证通过）";
                    request.RecNum = "接收号码，多个号码可以逗号分隔";
                    request.ParamString = "短信模板中的变量；数字需要转换为字符串；个人用户每个变量长度必须小于15个字符。";
                    SingleSendSmsResponse httpResponse = client.GetAcsResponse(request);
                    return 0;
                }
                catch (ServerException e)
                {
                    // e.printStackTrace();
                    strError = e.Message;
                    return -1;
                }
                catch (ClientException e)
                {
                    // e.printStackTrace();
                    strError = e.Message;
                    return -1;
                }
            }
        }

        public static string GetElementText(XmlNode nodeRoot,
    string strXpath)
        {
            XmlNode node = nodeRoot.SelectSingleNode(strXpath);
            if (node == null)
                return "";

            XmlNode nodeText;
            nodeText = node.SelectSingleNode("text()");

            if (nodeText == null)
                return "";
            else
                return nodeText.Value;
        }


    }

    /*
* AliMessageInterface.dll.config 需要这样配置
<?xml version="1.0"?>
<configuration>
<library 
 code="*" 
 accessKey="Access Key" 
 accessSecret="Access Secret"/>

</configuration>         * 
* */
    class ConfigParam
    {
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
        public int nMaxChars = 70;  // 每条短消息的字符数。短信服务商可能给文字加上前缀字符串，这样实际能用的可能少于 70 个字符，需要用参数配置一下

        public static ConfigParam LoadConfig(string strLibraryCode)
        {
            string strDir = AppDomain.CurrentDomain.BaseDirectory;
            string strCfgFile = Path.Combine(strDir, "AliMessageInterface.dll.config");

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

            cfg.AccessKey = node.GetAttribute("accessKey");
            if (string.IsNullOrEmpty(cfg.AccessKey) == true)
                throw new Exception("配置文件中尚未为馆代码 '" + strLibraryCode + "'配置 accessKey 参数");

            cfg.AccessSecret = node.GetAttribute("accessSecret");
            if (string.IsNullOrEmpty(cfg.AccessSecret) == true)
                throw new Exception("配置文件中尚未为馆代码 '" + strLibraryCode + "'配置 accessSecret 参数");

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
    }

}
