﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("FingerprintCenter")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("FingerprintCenter")]
[assembly: AssemblyCopyright("Copyright © 数字平台 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("26186394-5c49-412d-a1df-1f9567108eb9")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
// 可以指定所有值，也可以使用以下所示的 "*" 预置版本号和修订号
// 方法是按如下所示使用“*”: :
[assembly: AssemblyVersion("2.1.*")]
[assembly: AssemblyFileVersion("2.1.0.0")]

// V1.1 2019/2/21 第二个版本
// V1.2 2019/4/12 采用最新 dp-library submodule 的版本
// V2.1 2019/7/30 GetVersion() API 返回 2.1，表示从这个版本开始 GetState() API 支持 strStyle 为 "getLibraryServerUID"
//                  而此前 GetVersion() API 返回的是 2.0 (2.0 的意思是相对于 zkfingerprint 的 1.0)
