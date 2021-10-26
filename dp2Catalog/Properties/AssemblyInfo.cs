using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dp2Catalog")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DigitalPlatform")]
[assembly: AssemblyProduct("dp2Catalog")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6428cd58-357c-4881-85a0-08647646f283")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("3.3.*")]
[assembly: AssemblyFileVersion("3.3.0.0")]

// 2.5 (2015/12/11) 调用 dp2library Login() API 的时候发送了 client 参数
// 3.0 (2018/6/23) 改用 .NET Framework 4.6.1 编译
// 3.1 (2018/8/25) Z39.50 服务器属性中增加了 “ISSN 自动规整为 8 位” 功能
// 3.2 (2019/5/13) 改用 .NET Framework 4.7.2 编译
// 3.3 (2021/10/25) MARC 编辑器的定长字段模板支持最新 marcdef marcvaluelist 配置文件语法改进
