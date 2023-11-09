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
[assembly: AssemblyVersion("3.5.*")]
[assembly: AssemblyFileVersion("3.5.0.0")]

// 1.1 (2016/6/26） 增加 Session 计数器功能
// 1.2 (2016/10/7) 对期刊，增加显示每期封面的功能
// 1.3 (2017/12/12) 在 opac.xml 中 databaseFilter 元素内 hide 属性可以定义希望隐藏的普通库或者虚拟库名字列表
// 3.0 (2018/6/23) 改用 .NET Framework 4.6.1 编译
// 3.1 (2021/5/28) management.aspx 页面只有具备 manageopac 权限的用户才允许使用
// 3.2 (2021/6/11) column.aspx 创建栏目缓存功能，消除了一个 bug: 当所有书目库都没有评注库的情况下创建栏目缓存会报错
// 3.3 (2021/7/21) book.aspx 中，当用户不具备 getbiblioinfo 权限时(例如为存取定义定义了不适当的值引起)，会直接报错让下级记录(例如评注)也显示不出来。这个问题已经解决。
// 3.4 (2021/9/28) searchbiblio.aspx 中，修正(虚拟库)所选检索途径被程序误识别为“<全部>”的 bug
// 3.5 (2023/11/8) reservationinfo.aspx 增加 URL 参数 barcode，供工作人员管理读者的预约请求。
//                  注: borrowinfo.aspx 原来就已经实现了 barcode 参数
