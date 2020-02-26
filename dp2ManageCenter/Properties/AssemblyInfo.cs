using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2ManageCenter")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2ManageCenter")]
[assembly: AssemblyCopyright("Copyright © 2020 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("4dd7602b-fba3-4172-9542-dd59b9c7677d")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
// 可以指定所有值，也可以使用以下所示的 "*" 预置版本号和修订号
// 方法是按如下所示使用“*”: :
// [assembly: AssemblyVersion("1.0.*")]

// https://stackoverflow.com/questions/53782085/visual-studio-assemblyversion-with-dont-work
[assembly: AssemblyVersion("1.2.*")]
[assembly: AssemblyFileVersion("1.2.0.0")]

// v1.1 (2020/2/25) 获取 MD5 采用了新的任务方式。会检查 dp2library 的版本号是否为 3.23 以上
// v1.2 (2020/2/26) 服务器管理对话框里面增加了 UID 列，新增服务器节点时会检查 UID 是否重复，重复的不允许加入
//                  服务器管理对话框里面可以复制 JSON 定义到 Windows 剪贴板；和从 Windows 剪贴板粘贴 JSON 定义创建新的服务器节点
//                  服务器对话框里面复制出去的 JSON 定义，可以被 dp2Circulation 登录对话框粘贴使用
