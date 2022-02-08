using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2LibraryXE")]
[assembly: AssemblyDescription("dp2 图书馆集成系统应用服务器 XE 版")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("dp2LibraryXE")]
[assembly: AssemblyCopyright("Copyright © 2014-2015 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("936bc166-0823-445b-89a7-b9a87acc831b")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“内部版本号”和“修订号”的默认值，
// 方法是按如下所示使用“*”:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.5.*")]
[assembly: AssemblyFileVersion("3.5.0.0")]

// V1.2 (2016/10/11) 在面板上可以为 Windows 启用 MSMQ，可以为 library.xml 配置 MQ 参数
// V3.0 (2018/6/23) 改为用 .NET Framework 4.6.1 编译
// V3.1 (2018/4/12) 采用新的 dp-library submodule 的版本
// V3.2 (2021/9/9) 主菜单增加创建绿色更新包和安装绿色更新包命令
// V3.3 (2022/1/27) 升级 dp2OPAC 时，会自动把安装包中的 web.config 和当前 dp2OPAC 的 web.config 内容合并
// V3.4 (2022/1/29) 升级 dp2OPAC 时，会把虚拟目录 bin 子目录中以前版本残留的 system.*.dll 文件删除
// V3.5 (2022/2/8) 升级 dp2OPAC 时，会观察虚拟目录中是否存在 __filelist.config 文件，如果存在，则按照它删除以前残留的文件；否则会把虚拟目录 bin 子目录中以前版本残留的 system.*.dll 文件删除
