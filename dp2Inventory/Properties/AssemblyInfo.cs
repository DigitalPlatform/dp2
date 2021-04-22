using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2Inventory")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2Inventory")]
[assembly: AssemblyCopyright("Copyright ©  2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("37202531-e7d6-48cf-b2d3-bab8ffc6713c")]

// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
//可以指定所有这些值，也可以使用“生成号”和“修订号”的默认值
//通过使用 "*"，如下所示:
// [assembly: AssemblyVersion("1.0.*")]
// [assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyVersion("0.0.2")]
[assembly: AssemblyFileVersion("0.0.2.0")]

// 0.0.1 (2021/4/22) 首个发布版本
// 0.0.2 (2021/4/22)
//                      1) dp2Inventory.exe 只允许启动一个实例
//                      2) 当一个图书标签被处理一次以后，后来切换过层架标，再次扫描这个图书标签的时候应该得到重新处理(而不是被标为“交叉”)
//                      3) 当设置对话框中 dp2library URL 和 SIP Server Address 都设置的时候不会报错，这是正常情况。此时会自动优先使用 SIP 协议
//                      4) 当 RfidCenter 没有启动的时候，到盘点对话框开始盘点时才会报错。报错后自动停止盘点
