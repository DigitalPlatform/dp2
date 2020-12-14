using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("RfidCenter")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("RfidCenter")]
[assembly: AssemblyCopyright("Copyright © 数字平台 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("03a2e9c6-5513-4211-9028-af6f69941039")]

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
[assembly: AssemblyVersion("1.13.0")]   // 1.11.*
[assembly: AssemblyFileVersion("1.13.0.0")]

// V1.1 2019/2/21 支持 32-bit Windows 环境
// V1.2 2019/4/12 采用了最新 dp-library submodule 的版本
// V1.3 2019/9/12 取消了 SendKey 功能
// V1.4 2019/11/27 增加对具有多天线的读写器的支持。RfidCenter API 有若干改动
// V1.5 2019/12/5 对 ListTags() API 做了增强，允许它同时执行 getLockState 动作
// V1.6 2020/4/13 修正了 reader_locks.LockForWrite() 异常未处理的 bug
// V1.7 2020/7/20 增加 LED 显示屏 API
// V1.8 2020/8/19 增加 小票打印 API
// V1.9 2020/9/2 用上了 RFID SDK 2020 年 4 月的 DLL。并且增加了 CompactLog 机制用来记录 inventory() error，当十分钟内这类出错累计超过 10 次，则会自动重启一次 RFID 驱动
// V1.10 2020/9/22 用回 RFID SDK 2019 年底的 DLL
// V1.11 2020/11/21 OpenShelfLock() API 增加了一个新版本。锁被打开后立即关闭情况得到了解决；优化了锁关闭状态下探测状态的速度
//      1.11.1 (2020/11/27) 读取锁状态增加了重试机制
//      1.11.2 去掉了锁操作的 lock()
//      1.11.3 读取锁状态出错重试以后即便解决了，也会返回警告错误代码
//      1.11.4 (2020/12/2) 锁控重构为使用单独的 Driver，解决两块锁控板情景的特定问题
//      1.11.5 (2020/12/7) ShelfLockDriver.First 增加了 Dispose() 接口
// V1.12 2020/12/8 锁控改用独立的 Driver
//      1.12.0 (2020/12/8)
// V1.13 2020/12/14 增加对 UHF 读写器的支持
