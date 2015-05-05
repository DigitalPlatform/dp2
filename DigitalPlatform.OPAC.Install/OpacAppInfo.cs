using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using DigitalPlatform.IO;
using DigitalPlatform.Install;

namespace DigitalPlatform.OPAC
{
    /// <summary>
    /// dp2OPAC 虚拟目录和应用的相关信息
    /// </summary>
    public class OpacAppInfo
    {
        public string IisPath = "";

        public string PhysicalPath = "";

        public string DataDir = ""; // 数据目录

        public OpacAppInfo(DirectoryEntry entry)
        {
            this.IisPath = entry.Path;

            this.PhysicalPath = entry.Properties["Path"].Value as string;

            if (string.IsNullOrEmpty(this.PhysicalPath) == false)
                this.DataDir = GetDataDir(this.PhysicalPath);
        }

        // 构造函数：通过 XML 构造
        public OpacAppInfo(XmlElement application)
        {
            string strPath = application.GetAttribute("path");
            string strSiteName = (application.ParentNode as XmlElement).GetAttribute("name");
            this.IisPath = strSiteName + strPath;

            XmlNode node = application.SelectSingleNode("virtualDirectory/@physicalPath");

            if (node != null)
            {
                this.PhysicalPath = node.Value;

                // http://stackoverflow.com/questions/16290282/convert-systemdrive-to-drive-letter
                this.PhysicalPath = Environment.ExpandEnvironmentVariables(this.PhysicalPath);

                if (string.IsNullOrEmpty(this.PhysicalPath) == false)
                    this.DataDir = GetDataDir(this.PhysicalPath);
            }

        }

        static string GetDataDir(string strVirtualDir)
        {
            if (string.IsNullOrEmpty(strVirtualDir) == true)
                return null;

            string strFileName = Path.Combine(strVirtualDir, "start.xml");
            if (File.Exists(strFileName) == false)
                return Path.Combine(strVirtualDir, "data");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch
            {
                return null;
            }

            if (dom.DocumentElement == null)
                return null;

            return dom.DocumentElement.GetAttribute("datadir");
        }

        // 判断是否为 dp2OPAC 虚拟目录。
        // 通过物理目录中是否具有 searchbiblio.aspx 来判断
        public bool IsOPAC()
        {
            if (string.IsNullOrEmpty(this.PhysicalPath) == true)
                return false;
            string strFilePath = Path.Combine(this.PhysicalPath, "searchbiblio.aspx");
            if (File.Exists(strFilePath) == true)
                return true;
            return false;
        }

        public static Version GetIisVersion()
        {
            using (RegistryKey componentsKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\InetStp", false))
            {
                if (componentsKey != null)
                {
                    int majorVersion = (int)componentsKey.GetValue("MajorVersion", -1);
                    int minorVersion = (int)componentsKey.GetValue("MinorVersion", -1);

                    if (majorVersion != -1 && minorVersion != -1)
                    {
                        return new Version(majorVersion, minorVersion);
                    }
                }

                return new Version(0, 0);
            }
        }

        // 查找 dp2OPAC 路径
        // return:
        //      -1  出错
        //      其他  返回找到的路径个数
        public static int GetOpacInfo(out List<OpacAppInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<OpacAppInfo>();

            Version version = OpacAppInfo.GetIisVersion();

            if (version.Major >= 7)
            {
                // 使用 appcmd 获得信息
                // appcmd list apps
                return GetOpacInfoByAppCmd(out infos,
                    out strError);
            }

            // 用 Active Directory 方法
            for (int i = 0; ; i++)
            {
                string strSitePath = "IIS://localhost/W3SVC/" + (i + 1);

                try
                {
                    if (DirectoryEntry.Exists(strSitePath) == false)
                        break;
                }
                catch (COMException ex)
                {
                    if ((uint)ex.ErrorCode == 0x80005000)
                    {
                        strError = "IIS 尚未启用";
                        return -1;
                    }
                }

                string strRootPath = strSitePath + "/ROOT";
                if (DirectoryEntry.Exists(strRootPath) == false)
                    continue;

                DirectoryEntry entry = new DirectoryEntry(strRootPath);

                foreach (DirectoryEntry child in entry.Children)
                {
                    if (child.SchemaClassName != "IIsWebVirtualDir")
                        continue;

                    OpacAppInfo info = new OpacAppInfo(child);

                    if (string.IsNullOrEmpty(info.PhysicalPath) == false   // 如果不是应用目录，则 PhysicalPath 可能为空
                        && info.IsOPAC() == true)
                        infos.Add(info);
                }
            }

            return infos.Count;
        }

#if NO
        // 查找 dp2OPAC 路径
        // return:
        //      -1  出错
        //      其他  返回找到的路径个数
        int FindOpacPath(out List<OpacAppInfo> infos,
            out string strError)
        {
            strError = "";
            infos = new List<OpacAppInfo>();

            for (int i = 0; ; i++)
            {
                string strSitePath = "IIS://localhost/W3SVC/" + (i + 1);

                try
                {
                    if (DirectoryEntry.Exists(strSitePath) == false)
                        break;
                }
                catch (COMException ex)
                {
                    if ((uint)ex.ErrorCode == 0x80005000)
                    {
                        strError = "IIS 尚未启用";
                        return -1;
                    }
                }

                string strOpacPath = strSitePath + "/ROOT/dp2OPAC";
                if (DirectoryEntry.Exists(strOpacPath) == false)
                    continue;

                DirectoryEntry entry = new DirectoryEntry(strOpacPath);

                OpacAppInfo info = new OpacAppInfo(entry);

                if (string.IsNullOrEmpty(info.PhysicalPath) == false)   // 如果不是应用目录，则 PhysicalPath 可能为空
                    infos.Add(info);
            }

            return infos.Count;
        }
#endif

        // 用 appcmd 方式获得 sites 信息
        public static int GetSitesByAppCmd(out List<string> results,
            out string strError)
        {
            strError = "";

            results = new List<string>();

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            if (File.Exists(fileName) == false)
            {
                strError = "IIS appcmd 尚未安装";
                return -1;
            }

            List<string> lines = new List<string>();

            // list apps
            // list config /section:Sites

            lines.Add("list config /section:Sites");

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                lines,
                false,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            // List<string> results = new List<string>();
            StringBuilder result = new StringBuilder();
            string strErrorInfo = "";

            try
            {
                int i = 0;
                foreach (string arguments in lines)
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using (Process process = Process.Start(info))
                    {

                        process.OutputDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            // results.Add(e1.Data);
                            if (string.IsNullOrEmpty(e1.Data) == false)
                                result.Append(e1.Data + "\r\n");
                        }
                        );
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            strErrorInfo = e1.Data;
                        }
                        );
                        process.BeginOutputReadLine();
                        while (true)
                        {
                            Application.DoEvents();
                            if (process.WaitForExit(500) == true)
                                break;
                        }
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }

                    i++;
                }
            }
            finally
            {
            }

            if (string.IsNullOrEmpty(strErrorInfo) == false)
            {
                strError = strErrorInfo;
                return -1;
            }
#endif

            /*
C:\Windows\System32\inetsrv>appcmd list config /section:Sites
<system.applicationHost>
  <sites>
    <siteDefaults>
      <bindings>
      </bindings>
      <limits />
      <logFile logFormat="W3C" directory="%SystemDrive%\inetpub\logs\LogFiles">
        <customFields>
        </customFields>
      </logFile>
      <traceFailedRequestsLogging directory="%SystemDrive%\inetpub\logs\FailedRe
qLogFiles" />
      <ftpServer>
        <connections />
        <security>
          <dataChannelSecurity />
          <commandFiltering>
          </commandFiltering>
          <ssl />
          <sslClientCertificates />
          <authentication>
            <anonymousAuthentication />
            <basicAuthentication />
            <clientCertAuthentication />
            <customAuthentication>
              <providers>
              </providers>
            </customAuthentication>
          </authentication>
          <customAuthorization>
            <provider />
          </customAuthorization>
        </security>
        <customFeatures>
          <providers>
          </providers>
        </customFeatures>
        <messages />
        <fileHandling />
        <firewallSupport />
        <userIsolation>
          <activeDirectory />
        </userIsolation>
        <directoryBrowse />
        <logFile />
      </ftpServer>
    </siteDefaults>
    <applicationDefaults applicationPool="DefaultAppPool" />
    <virtualDirectoryDefaults allowSubDirConfig="true" />
    <site name="Default Web Site" id="1">
      <bindings>
        <binding protocol="http" bindingInformation="*:80:" />
      </bindings>
      <limits />
      <logFile>
        <customFields>
        </customFields>
      </logFile>
      <traceFailedRequestsLogging />
      <applicationDefaults />
      <virtualDirectoryDefaults />
      <ftpServer>
        <connections />
        <security>
          <dataChannelSecurity />
          <commandFiltering>
          </commandFiltering>
          <ssl />
          <sslClientCertificates />
          <authentication>
            <anonymousAuthentication />
            <basicAuthentication />
            <clientCertAuthentication />
            <customAuthentication>
              <providers>
              </providers>
            </customAuthentication>
          </authentication>
          <customAuthorization>
            <provider />
          </customAuthorization>
        </security>
        <customFeatures>
          <providers>
          </providers>
        </customFeatures>
        <messages />
        <fileHandling />
        <firewallSupport />
        <userIsolation>
          <activeDirectory />
        </userIsolation>
        <directoryBrowse />
        <logFile />
      </ftpServer>
      <application path="/">
        <virtualDirectoryDefaults />
        <virtualDirectory path="/" physicalPath="%SystemDrive%\inetpub\wwwroot"
/>
      </application>
      <application path="/dp2OPAC">
        <virtualDirectoryDefaults />
        <virtualDirectory path="/" physicalPath="c:\test" />
      </application>
    </site>
  </sites>
</system.applicationHost>

             * 
             * */

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strError);
            }
            catch (Exception ex)
            {
                strError = "Sites 描述 '"+strError+"' 装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//site");
            foreach (XmlElement site in nodes)
            {
                results.Add(site.GetAttribute("name"));

            }

            return results.Count;
        }

        // 用 appcmd 方式获得 dp2OPAC 实例信息
        public static int GetOpacInfoByAppCmd(out List<OpacAppInfo> infos,
            out string strError)
        {
            strError = "";

            infos = new List<OpacAppInfo>();

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            if (File.Exists(fileName) == false)
            {
                strError = "IIS appcmd 尚未安装";
                return -1;
            }

            List<string> lines = new List<string>();

            // list apps
            // list config /section:Sites

            lines.Add("list config /section:Sites");

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                lines,
                false,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            // List<string> results = new List<string>();
            StringBuilder result = new StringBuilder();
            string strErrorInfo = "";

            try
            {
                int i = 0;
                foreach (string arguments in lines)
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using (Process process = Process.Start(info))
                    {

                        process.OutputDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            // results.Add(e1.Data);
                            if (string.IsNullOrEmpty(e1.Data) == false)
                                result.Append(e1.Data + "\r\n");
                        }
                        );
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            strErrorInfo = e1.Data;
                        }
                        );
                        process.BeginOutputReadLine();
                        while (true)
                        {
                            Application.DoEvents();
                            if (process.WaitForExit(500) == true)
                                break;
                        }
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }

                    i++;
                }
            }
            finally
            {
            }

            if (string.IsNullOrEmpty(strErrorInfo) == false)
            {
                strError = strErrorInfo;
                return -1;
            }

#endif

            /*
C:\Windows\System32\inetsrv>appcmd list apps
APP "Default Web Site/" (applicationPool:DefaultAppPool)
APP "Default Web Site/dp2OPAC" (applicationPool:DefaultAppPool)
             * * */

            /*
C:\Windows\System32\inetsrv>appcmd list config /section:Sites
<system.applicationHost>
  <sites>
    <siteDefaults>
      <bindings>
      </bindings>
      <limits />
      <logFile logFormat="W3C" directory="%SystemDrive%\inetpub\logs\LogFiles">
        <customFields>
        </customFields>
      </logFile>
      <traceFailedRequestsLogging directory="%SystemDrive%\inetpub\logs\FailedRe
qLogFiles" />
      <ftpServer>
        <connections />
        <security>
          <dataChannelSecurity />
          <commandFiltering>
          </commandFiltering>
          <ssl />
          <sslClientCertificates />
          <authentication>
            <anonymousAuthentication />
            <basicAuthentication />
            <clientCertAuthentication />
            <customAuthentication>
              <providers>
              </providers>
            </customAuthentication>
          </authentication>
          <customAuthorization>
            <provider />
          </customAuthorization>
        </security>
        <customFeatures>
          <providers>
          </providers>
        </customFeatures>
        <messages />
        <fileHandling />
        <firewallSupport />
        <userIsolation>
          <activeDirectory />
        </userIsolation>
        <directoryBrowse />
        <logFile />
      </ftpServer>
    </siteDefaults>
    <applicationDefaults applicationPool="DefaultAppPool" />
    <virtualDirectoryDefaults allowSubDirConfig="true" />
    <site name="Default Web Site" id="1">
      <bindings>
        <binding protocol="http" bindingInformation="*:80:" />
      </bindings>
      <limits />
      <logFile>
        <customFields>
        </customFields>
      </logFile>
      <traceFailedRequestsLogging />
      <applicationDefaults />
      <virtualDirectoryDefaults />
      <ftpServer>
        <connections />
        <security>
          <dataChannelSecurity />
          <commandFiltering>
          </commandFiltering>
          <ssl />
          <sslClientCertificates />
          <authentication>
            <anonymousAuthentication />
            <basicAuthentication />
            <clientCertAuthentication />
            <customAuthentication>
              <providers>
              </providers>
            </customAuthentication>
          </authentication>
          <customAuthorization>
            <provider />
          </customAuthorization>
        </security>
        <customFeatures>
          <providers>
          </providers>
        </customFeatures>
        <messages />
        <fileHandling />
        <firewallSupport />
        <userIsolation>
          <activeDirectory />
        </userIsolation>
        <directoryBrowse />
        <logFile />
      </ftpServer>
      <application path="/">
        <virtualDirectoryDefaults />
        <virtualDirectory path="/" physicalPath="%SystemDrive%\inetpub\wwwroot"
/>
      </application>
      <application path="/dp2OPAC">
        <virtualDirectoryDefaults />
        <virtualDirectory path="/" physicalPath="c:\test" />
      </application>
    </site>
  </sites>
</system.applicationHost>

             * 
             * */

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strError);
            }
            catch (Exception ex)
            {
                strError = "Sites 描述 '"+strError+"' 装入 XMLDOM 时出错: " +ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//application");
            foreach (XmlElement application in nodes)
            {
                OpacAppInfo info = new OpacAppInfo(application);

                if (string.IsNullOrEmpty(info.PhysicalPath) == false   // 如果不是应用目录，则 PhysicalPath 可能为空
    && info.IsOPAC() == true)
                    infos.Add(info);

            }

            return infos.Count;
        }

        // 用 appcmd 方式获得 所有虚拟目录的信息 (不仅仅是 dp2OPAC 虚拟目录)
        public static int GetAllVirtualInfoByAppCmd(out List<OpacAppInfo> infos,
            out string strError)
        {
            strError = "";

            infos = new List<OpacAppInfo>();

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            if (File.Exists(fileName) == false)
            {
                strError = "IIS appcmd 尚未安装";
                return -1;
            }

            List<string> lines = new List<string>();

            // list apps
            // list config /section:Sites

            lines.Add("list config /section:Sites");

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                lines,
                false,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strError);
            }
            catch (Exception ex)
            {
                strError = "Sites 描述 '" + strError + "' 装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//application");
            foreach (XmlElement application in nodes)
            {
                OpacAppInfo info = new OpacAppInfo(application);

                infos.Add(info);
            }

            return infos.Count;
        }

        // 注册 Web App
        // 只能用于 IIS 7 以上版本
        public static int RegisterWebApp(
            string strSiteName,
            string strVirtualDirPath,
            string strAppDir,
            out string strError)
        {
            strError = "";

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            List<string> lines = new List<string>();

            // 创建新的 Site
            // lines.Add("add site /name:dp2Site /bindings:\"http/:8081:\" /physicalPath:\"" + this.dp2SiteDir + "\"");

            // 允许任何 IP 域名访问本站点
            // lines.Add("set site \"WebSite1\" /bindings:http/:8080:");

            // 创建应用程序
            // TODO: 是否当探测到 apppool 已经存在的时候不必重新创建?
            lines.Add("delete app \"" + strSiteName + strVirtualDirPath + "\"");
            lines.Add("add app /site.name:\"" + strSiteName + "\" /path:" + strVirtualDirPath + " /physicalPath:" + strAppDir);

            // 创建 AppPool
            // lines.Add("delete apppool \"dp2OPAC\"");
            lines.Add("add apppool /name:dp2OPAC");
            // 修改 AppPool 特性： .NET 4.0
            lines.Add("set apppool \"dp2OPAC\" /managedRuntimeVersion:v4.0");
            // 修改 AppPool 特性： Integrated
            lines.Add("set apppool \"dp2OPAC\" /managedPipelineMode:Integrated");

            // 修改 AppPool 特性： disallowOverlappingRotation
            lines.Add("set apppool \"dp2OPAC\" /recycling.disallowOverlappingRotation:true");

            // 使用这个 AppPool
            lines.Add("set app \"" + strSiteName + strVirtualDirPath + "\" /applicationPool:dp2OPAC");

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                lines,
                true,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            string strErrorInfo = "";
            StringBuilder result = new StringBuilder();

            // AppendSectionTitle("开始注册");
            try
            {
                int i = 0;
                foreach (string arguments in lines)
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    result.Append("\r\n" + (i + 1).ToString() + ")\r\n" + fileName + " " + arguments + "\r\n");

                    // Process.Start(fileName, arguments).WaitForExit();
                    using (Process process = Process.Start(info))
                    {

                        process.OutputDataReceived += new DataReceivedEventHandler(
            (s, e1) =>
            {
                result.Append(e1.Data + "\r\n");
            }
        );
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            strErrorInfo = e1.Data;
                        }
                        );
                        process.BeginOutputReadLine();
                        while (true)
                        {
                            Application.DoEvents();
                            if (process.WaitForExit(500) == true)
                                break;
                        }
                        // 显示残余的文字
#if NO
                        while (!process.StandardOutput.EndOfStream)
                        {
                            Application.DoEvents();
                            Thread.Sleep(1);
                        }
#endif
                        // process.CancelOutputRead();
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }

                    i++;
                }
            }
            finally
            {
                // AppendSectionTitle("结束注册");
            }

            if (string.IsNullOrEmpty(strErrorInfo) == false)
            {
                strError = strErrorInfo;
                return -1;
            }

            // 过程信息
            strError = result.ToString();
#endif
            return 0;
        }

        // 注销 Web App。调用后，程序物理目录没有删除。apppool 没有删除。
        // 只能用于 IIS 7 以上版本
        public static int UnregisterWebApp(
            string strSiteName,
            string strVirtualDirPath,
            out string strError)
        {
            strError = "";

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            List<string> lines = new List<string>();

            // 创建应用程序
            lines.Add("delete app \"" + strSiteName + strVirtualDirPath + "\"");

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                lines,
                true,
                out strError);
            if (nRet == -1)
                return -1;
#if NO
            string strErrorInfo = "";
            StringBuilder result = new StringBuilder();

            try
            {
                int i = 0;
                foreach (string arguments in lines)
                {
                    ProcessStartInfo info = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    result.Append("\r\n" + (i + 1).ToString() + ")\r\n" + fileName + " " + arguments + "\r\n");

                    // Process.Start(fileName, arguments).WaitForExit();
                    using (Process process = Process.Start(info))
                    {

                        process.OutputDataReceived += new DataReceivedEventHandler(
            (s, e1) =>
            {
                result.Append(e1.Data + "\r\n");
            }
        );
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e1) =>
                        {
                            strErrorInfo = e1.Data;
                        }
                        );
                        process.BeginOutputReadLine();
                        while (true)
                        {
                            Application.DoEvents();
                            if (process.WaitForExit(500) == true)
                                break;
                        }
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }

                    i++;
                }
            }
            finally
            {
                // AppendSectionTitle("结束注册");
            }

            if (string.IsNullOrEmpty(strErrorInfo) == false)
            {
                strError = strErrorInfo;
                return -1;
            }

            // 过程信息
            strError = result.ToString();
#endif

            return 0;
        }

        // 查找一个虚拟目录是否存在
        // return:
        //      -1  不存在
        //      其他  数组元素的下标
        public static int Find(List<OpacAppInfo> infos,
            string strSiteName,
            string strVirtualDirPath)
        {
            int i = 0;
            foreach (OpacAppInfo info in infos)
            {
                if (info.IisPath == strSiteName + strVirtualDirPath)
                    return i;

                i++;
            }

            return -1;
        }

        // 启动或者停止 AppPool
        public static int StartAppPool(string strAppPoolName,
            bool bStart,
            out string strError)
        {
            strError = "";

            // appcmd.exe 路径
            string fileName = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.System),
"inetsrv/appcmd.exe");

            string strLine = "start apppool /apppool.name:" + strAppPoolName;
            if (bStart == false)
                strLine = "stop apppool /apppool.name:" + strAppPoolName;

            // parameters:
            //      lines   若干行参数。每行执行一次
            // return:
            //      -1  出错
            //      0   成功。strError 里面有运行输出的信息
            int nRet = InstallHelper.RunCmd(
                fileName,
                new List<string>() { strLine },
                true,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }
    }
}
