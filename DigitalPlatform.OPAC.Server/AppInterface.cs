using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;

using DigitalPlatform;	// Stop类
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Interfaces;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 本部分是和外部接口有关的代码
    /// </summary>
    public partial class OpacApplication
    {
        public List<SsoInterface> m_externalSsoInterfaces = null;

        // 初始化单点登录接口
        /*
	<externalSsoInterface>
 		<interface type="nanchang" assemblyName="nanchangsso"/>
	</externalSsoInterface>
         */
        // parameters:
        // return:
        //      -1  出错
        //      0   当前没有配置任何扩展接口
        //      1   成功初始化
        public int InitialExternalSsoInterfaces(out string strError)
        {
            strError = "";

            this.m_externalSsoInterfaces = null;

            XmlNode root = this.OpacCfgDom.DocumentElement.SelectSingleNode(
    "externalSsoInterface");
            if (root == null)
            {
                strError = "在opac.xml中没有找到<externalSsoInterface>元素";
                return 0;
            }

            this.m_externalSsoInterfaces = new List<SsoInterface>();

            XmlNodeList nodes = root.SelectNodes("interface");
            foreach (XmlNode node in nodes)
            {
                string strType = DomUtil.GetAttr(node, "type");
                if (String.IsNullOrEmpty(strType) == true)
                {
                    strError = "<interface>元素未配置type属性值";
                    return -1;
                }

                string strAssemblyName = DomUtil.GetAttr(node, "assemblyName");
                if (String.IsNullOrEmpty(strAssemblyName) == true)
                {
                    strError = "<interface>元素未配置assemblyName属性值";
                    return -1;
                }

                SsoInterface sso_interface = new SsoInterface();
                sso_interface.Type = strType;
                sso_interface.Assembly = Assembly.Load(strAssemblyName);
                if (sso_interface.Assembly == null)
                {
                    strError = "名字为 '" + strAssemblyName + "' 的Assembly加载失败...";
                    return -1;
                }

                Type hostEntryClassType = ScriptManager.GetDerivedClassType(
        sso_interface.Assembly,
        "DigitalPlatform.Interfaces.ExternalSsoHost");
                if (hostEntryClassType == null)
                {
                    strError = "名字为 '" + strAssemblyName + "' 的Assembly中未找到 DigitalPlatform.Interfaces.ExternalSsoHost类的派生类，初始化扩展SSO接口失败...";
                    return -1;
                }

                sso_interface.HostObj = (ExternalSsoHost)hostEntryClassType.InvokeMember(null,
        BindingFlags.DeclaredOnly |
        BindingFlags.Public | BindingFlags.NonPublic |
        BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
        null);
                if (sso_interface.HostObj == null)
                {
                    strError = "创建 type 为 '" + strType + "' 的 DigitalPlatform.Interfaces.ExternalSsoHost 类的派生类的对象（构造函数）失败，初始化扩展消息接口失败...";
                    return -1;
                }

                // message_interface.HostObj.App = this;

                this.m_externalSsoInterfaces.Add(sso_interface);
            }

            return 1;
        }

        public SsoInterface GetSsoInterface(string strType)
        {
            if (this.m_externalSsoInterfaces == null)
                return null;

            foreach (SsoInterface sso_interface in this.m_externalSsoInterfaces)
            {
                if (sso_interface.Type == strType)
                    return sso_interface;
            }

            return null;
        }
    }

    // 一个扩展SSO接口
    public class SsoInterface
    {
        public string Type = "";
        public Assembly Assembly = null;
        public ExternalSsoHost HostObj = null;
    }
}
