﻿using System.Reflection;
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
[assembly: AssemblyVersion("3.10.*")]
[assembly: AssemblyFileVersion("3.10.0.0")]

// 2.5 (2015/12/11) 调用 dp2library Login() API 的时候发送了 client 参数
// 3.0 (2018/6/23) 改用 .NET Framework 4.6.1 编译
// 3.1 (2018/8/25) Z39.50 服务器属性中增加了 “ISSN 自动规整为 8 位” 功能
// 3.2 (2019/5/13) 改用 .NET Framework 4.7.2 编译
// 3.3 (2021/10/25) MARC 编辑器的定长字段模板支持最新 marcdef marcvaluelist 配置文件语法改进
// 3.4 (2022/6/17) dp2 检索窗增加了导出 MARCXML 文件功能。目前 UNIMARC 采用 dp2003 UNIMARC 名字空间，MARC21 采用国会图书馆 slim 名字空间
// 3.5 (2022/6/29) dp2 检索窗增加了导入 MARCXML 文件功能。和 880/平行模式转换功能
// 3.6 (2022/6/29) 重构 stop.BeginLoop() 为 var looping = BeginLoop()
//                  重构了下列代码文件: dp2SearchForm MarcDetailHost ZhongcihaoForm MyForm
//                  dp2 检索窗在检索装入浏览框的中途点浏览行，现在可以在固定面板区“属性”属性页看到书目记录详细信息(此前版本检索中途是无法看到的)
// 3.7 (2022/7/11) dp2 检索窗追加保存记录到指定的书目库，如果书目库是 dp2catalog 启动以后在内务里面新创建的一个书目库，则追加保存的时候有可能把书目库的 MARC 格式搞错。这个 bug 已经修复
// 3.8 (2022/7/18) Marc8Encoding 类里面增加了处理 Kf}ujxlm&#x03ac; kf&#x03ac;xury 这样的在 MARC-8 中无法表达的字符的处理能力
// 3.9 (2023/8/10) dp2 检索窗的单行属性页的一个 textbox 和 combobox 控件在 Windows 7 下尺寸不正确；多行属性页的一个 combobox 也有这个问题。这两个问题已经修正
//                  记录窗的一个 textbox 的尺寸在 Windows 7 下，反复修改记录窗尺寸的时候会导致 textbox 宽度只能收缩不能重新扩大。此问题已经修正
//                  MarcQueryHost 类增加几个从 MarcQuery 类复制过来的 static 定义
// 3.10 (2024/5/30) 为 DTLP 检索窗的浏览列表上下文菜单增加几个命令。主要是导出批控文件命令，和发生批控文件命令。
