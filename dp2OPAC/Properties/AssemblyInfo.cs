using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下特性集 
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2OPAC")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2OPAC")]
[assembly: AssemblyCopyright("2015 开源的 dp2 图书馆系统")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型 
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型， 
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("aecf761c-7b6f-4cd9-8d19-d56d9ae7fee6")]

// 程序集的版本信息由下列四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 您可以指定所有这些值，也可以使用“修订号”和“内部版本号”的默认值， 
// 方法是按如下所示使用“*”:
[assembly: AssemblyVersion("1.3.*")]
[assembly: AssemblyFileVersion("1.3.0.0")]

// 1.1 (2016/6/26） 增加 Session 计数器功能
// 1.2 (2016/10/7) 对期刊，增加显示每期封面的功能
// 1.3 (2017/12/12) 在 opac.xml 中 databaseFilter 元素内 hide 属性可以定义希望隐藏的普通库或者虚拟库名字列表
