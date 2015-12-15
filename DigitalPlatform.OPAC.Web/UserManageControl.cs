using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;
//using DigitalPlatform.CirculationClient;


namespace DigitalPlatform.OPAC.Web
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:UserManageControl runat=server></{0}:UserManageControl>")]
    public class UserManageControl : WebControl, INamingContainer
    {
        public XmlDocument ReaderDom = null;

        public OpacApplication app = null;
        public SessionInfo sessioninfo = null;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.UserManageControl.cs",
                typeof(UserManageControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }


    }
}
