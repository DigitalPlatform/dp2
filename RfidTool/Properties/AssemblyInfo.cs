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

[assembly: AssemblyVersion("1.0.18")]
[assembly: AssemblyFileVersion("1.0.18.0")]

// 1.0.1 (2020/12/10) 增加保存“写入历史”列表功能; 感知 USB 插拔、自动重新连接读写器;
//                      读写器连接成功后，会在状态行显示可用读卡器数量;
//                      ScanDialog 中 TagChanged 事件不再和对话框显示、隐藏挂钩，改为一直挂接
// 1.0.2 (2020/12/10) 写入层架标和读者卡时 EAS 为 Off   
// 1.0.3 (2020/12/11) 弹出“OI 和 AOI 尚未配置” MessageBox 的时候暂时禁用条码处理模块，避免这时候放标签到读卡器上引起再次弹出 MessageBox
// 1.0.4 (2020/12/14) 增加写入 UHF 高校联盟格式标签的功能(使用 M60 读写器)
// 1.0.5 (2020/12/16) 增加写入 UHF 国标格式标签的功能(使用 M60 读写器)
// 1.0.6 (2021/1/11) 对蓝牙读写器增加专用的打开方式，并能感知到蓝牙读写器打开和关闭电源、做出重新初始化设备的响应
// 1.0.7 (2021/1/14) 增加批处理修改标签功能。增加 readers.xml 支持
// 1.0.8 (2021/1/15)
// 1.0.9 (2021/1/16)
// 1.0.10 (2021/1/16) 修改标签对话框的开始对话框增加了“写入 UID PII 对照日志”这个事项。可以只选择这个动作进行批处理修改
// 1.0.11 (2021/1/18) 设置对话框增加校验条码号规则文本框；写入标签和修改标签功能均加入了可选的校验条码功能
// 1.0.12 (2021/4/9) 扫入对话框和修改对话框增加序列号
// 1.0.13 (2021/4/27) 扫入对话框内上下文菜单修改 EAS 的功能由序列号许可
// 1.0.14 (2021/4/29) 加强了对 OI 和 AOI 中空格字符的检查
// 1.0.15 (2021/5/12) 设置对话框中增加了检查，勾选“校验条码号”要求同时设置了条码号校验规则
// 1.0.16 (2021/6/17) 扫入对话框增加了利用脱机册信息验证的状态
// 1.0.17 (2021/8/10) RfidDriver 中增加 URL105 型号数据
// 1.0.18 (2022/7/24) 扫入对话框增加了一个命令“测试创建错误的标签内容”，用于写入会导致解析错误的内容
//                      设置对话框的“写入标签”属性页增加了一个 checkbox “写入时遇到解析错误的标签要当作空白标签覆盖”
// 1.0.19 (2023/11/2) 改进 UHF 标签相关功能