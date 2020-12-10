using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("RfidTool")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("RfidTool")]
[assembly: AssemblyCopyright("Copyright ©  2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("ef59437e-87d5-46c9-94cc-59676cb4da93")]

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

[assembly: AssemblyVersion("1.0.1")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// 1.0.1 (2020/12/10) 增加保存“写入历史”列表功能; 感知 USB 插拔、自动重新连接读写器;
//                      读写器连接成功后，会在状态行显示可用读卡器数量;
//                      ScanDialog 中 TagChanged 事件不再和对话框显示、隐藏挂钩，改为一直挂接
//                      
