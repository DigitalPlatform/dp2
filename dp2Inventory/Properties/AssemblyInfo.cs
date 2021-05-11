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
[assembly: AssemblyVersion("0.0.15")]
[assembly: AssemblyFileVersion("0.0.15.0")]

// 0.0.1 (2021/4/22) 首个发布版本
// 0.0.2 (2021/4/22)
//                      1) dp2Inventory.exe 只允许启动一个实例
//                      2) 当一个图书标签被处理一次以后，后来切换过层架标，再次扫描这个图书标签的时候应该得到重新处理(而不是被标为“交叉”)
//                      3) 当设置对话框中 dp2library URL 和 SIP Server Address 都设置的时候不会报错，这是正常情况。此时会自动优先使用 SIP 协议
//                      4) 当 RfidCenter 没有启动的时候，到盘点对话框开始盘点时才会报错。报错后自动停止盘点
// 0.0.3 (2021/4/23)
//                      1) 盘点对话框增加暂停按钮
//                      2) 主窗口菜单增加导入 UID 对照关系和清除对照关系的命令
// 0.0.4 (2021/4/23)
//                      1) 主窗口启动时候会禁用一段直到初始化完成
//                      2) 设置对话框中按下确定按钮时会自动检查 dp2library URL 正确性，并检查它和 SIP 服务器地址之中是否至少填入了一个
// 0.0.5 (2021/4/25)
//                      1) 原先在 inventory.xml 中的配置事项全部移到设置对话框配置
//                      2) 改善 SIP 用户名和密码不正确时候的报错。会自动暂停盘点循环。等用户名和密码修改正确后可以继续盘点循环
//                      3) dp2inventory.exe 启动时候会自动带起来 rfidcenter.exe
// 0.0.6
//                      4) 设置对话框确定关闭的时候，会自动检查 SIP 服务器(地址和用户名密码等)参数是否正确
// 0.0.7
//                      5) SIP 全功能状态时，如果开始盘点所选的馆藏地位置不合法，盘点时会报错。注意 dp2capo 服务器只检查合法的馆代码下属的馆藏地是否合法，而对于无法识别是否合法的馆代码，对其下属的馆藏地无法检查和报错，也就是说任意馆藏地字符串都会写入成功
//                      6) SIP 半功能状态，不会检查开始盘点所选的馆藏地是否合法
// 0.0.8
//                      7) 设置对话框中增加 条码号校验规则 参数。对 dp2library 和 SIP2 模式都起作用
//                      8) 设置对话框中增加 启用标签信息缓存 参数
//                      9) 增加 ClickOnce 方式下后台自动更新的功能
// 0.0.9 (2021/4/26)
//                      1) 开始盘点对话框中的馆藏地可以记忆了
//                      2) (当选择了更新当前位置和永久位置后)在开始盘点对话框确定关闭时，会检查馆藏地是否输入。但如果按住 Ctrl 键点确定按钮，则不做此检查
//                      3) (当选择了更新当前位置和永久位置后)在盘点过程中，还会检查当前馆藏地(由开始盘点对话框设置)是否为空，如果为空则会报错
// 0.0.10
//                      4) 对于 SIP2 协议也支持自动还书和校验 EAS 了
//                      5) 修正设置对话框点确定后报错，再点右上角关闭按钮，无法撤销在对话框打开期间对参数的变动，的 bug
// 0.0.11 (2021/4/27)
//                      1) 扫到 ISO25693 读者证，会显示“读者证被滤除”
// 0.0.12
//                      2) 导入 UID-->UII 对照关系的时候，如果遇到源文件行格式不合法，会直接报错中断处理返回
// 0.0.13 (2021/5/6)
//                      1) 增加书架窗，可以按照每个书架集中显示盘点过的最新册信息
// 0.0.14
//                      2) 开始盘点对话框增加“总是写入操作日志”checkbox。用途是在册记录没有实质性修改的情况下也要写入 transfer 动作日志
// 0.0.15 (2021/5/10)
//                      1) 排除了盘点过程中遇到空白标签出现异常的 bug