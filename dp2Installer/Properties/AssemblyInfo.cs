using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2Installer")]
[assembly: AssemblyDescription("dp2 安装实用工具")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("dp2Installer -- dp2 安装实用工具")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("2a8983c2-448b-4776-9167-00727fdbd316")]

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
[assembly: AssemblyVersion("3.5.3")]
[assembly: AssemblyFileVersion("3.5.3.0")]

// 1.1
// 1.2 (2016/11/2) 增加安装 Z39.50 服务器功能
// 1.3 (2017/9/3) 在管理 dp2kernel 和 dp2library 实例的时候，能只停止单个实例
// 3.0 (2018/6/23) 改为用 .NET Framework 4.6.1 编译
// 3.1 (2019/4/12) 采用最新 dp-library submodule 的版本
// 3.2 (2019/4/28) 改为用 .NET Framework 4.7.2 编译的版本
// 3.3 (2021/1/5) 增加 PalmCenter 安装维护功能
//      3.3.1 (2021/1/5)
// 3.4 (2021/7/16) dp2installer 的 dp2library 实例对话框中增加了 checkbox “停用本实例”
// 3.5 (2021/9/12) dp2installer 全面启用 ClientInfo。包括错误日志、 Config 体系(利用 settings.xml 文件保存配置参数)
//      3.5.1 (2021/9/15) dp2kernel 和 dp2library 实例安装对话框里面的实例名做了检查，合法的实例名字符为数字或者字母，或者下划线。实例名也可以为空
//      3.5.2 (2022/1/27) 升级 dp2OPAC 时，会自动把安装包中的 web.config 和当前 dp2OPAC 的 web.config 内容合并
//      3.5.3 (2022/1/29) 升级 dp2OPAC 时，会把虚拟目录 bin 子目录中以前版本残留的 system.*.dll 文件删除

