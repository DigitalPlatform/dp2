using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Install;
using System.IO;
using System.Xml;

namespace TestDp2Library.Install
{
    /// <summary>
    /// 测试那些和安装有关的函数
    /// </summary>
    [TestClass]
    public class TestInstall
    {
        // 添加原本没有的 dependentAssembly
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_01()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(8, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

        // 添加原本没有的 dependentAssembly。添加前 target 中 runtime 元素不存在
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_02()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
</configuration>";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(8, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

        // 添加原本没有的 dependentAssembly。添加前 target 中 assemblyBinding 元素不存在
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_03()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
  </runtime>
</configuration>";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(8, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }


        // 去掉 target 中的 dependentAssembly
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_05()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(0, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

        // 去掉 target 中的 dependentAssembly。source 中 runtime 不存在
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_06()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(0, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

        // 去掉 target 中的 dependentAssembly。source 中 assemblyBinding 不存在(runtime 存在)
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_07()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
    <runtime/>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(1, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(0, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

        // 没有变化
        [TestMethod]
        public void Test_install_RefreshDependentAssembly_10()
        {
            string sourceXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string targetXml = @"
<configuration>
    <system.webServer>
    <modules runAllManagedModulesForAllRequests='true' />
    <handlers>
      <remove name='ChartImageHandler' />
      <add name='ChartImageHandler' preCondition='integratedMode' verb='GET,HEAD,POST' path='ChartImg.axd' type='System.Web.UI.DataVisualization.Charting.ChartHttpHandler, System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35' />
    </handlers>
        <defaultDocument enabled='true'>
            <files>
                <add value='searchbiblio.aspx' />
            </files>
        </defaultDocument>
    </system.webServer>
  <runtime>
    <assemblyBinding xmlns='urn:schemas-microsoft-com:asm.v1'>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Threading.Tasks' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.6.10.0' newVersion='2.6.10.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='DocumentFormat.OpenXml' publicKeyToken='8fb06cb64d019a17' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-2.9.1.0' newVersion='2.9.1.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Diagnostics.Tracing' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.2.0.0' newVersion='4.2.0.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Reflection' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.1.2.0' newVersion='4.1.2.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='ExcelNumberFormat' publicKeyToken='23c6f5d73be07eca' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-1.0.5.0' newVersion='1.0.5.0' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Memory' publicKeyToken='cc7b13ffcd2ddd51' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-4.0.1.1' newVersion='4.0.1.1' />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name='System.Runtime.CompilerServices.Unsafe' publicKeyToken='b03f5f7f11d50a3a' culture='neutral' />
        <bindingRedirect oldVersion='0.0.0.0-5.0.0.0' newVersion='5.0.0.0' />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
";
            string tempDir = Path.GetTempPath();
            string sourceFileName = Path.Combine(tempDir, "source.xml");
            string targetFileName = Path.Combine(tempDir, "target.xml");

            File.WriteAllText(sourceFileName, sourceXml);
            File.WriteAllText(targetFileName, targetXml);

            int nRet = InstallHelper.RefreshDependentAssembly(sourceFileName,
    targetFileName,
    out string strError);
            Assert.AreEqual(0, nRet);

            // 验证
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(targetFileName);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

                var nodes = dom.SelectNodes("configuration/runtime/asm:assemblyBinding/asm:dependentAssembly", nsmgr);
                Assert.AreEqual(8, nodes.Count);
            }

            File.Delete(sourceFileName);
            File.Delete(targetFileName);
        }

    }
}
