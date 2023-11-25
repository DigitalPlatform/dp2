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
[assembly: AssemblyVersion("1.14.27")]   // 1.11.*
[assembly: AssemblyFileVersion("1.14.0.0")]

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
// V1.13 2020/12/14 增加对 UHF 读写器的支持，首先支持高校联盟数据格式
//      1.13.1 (2020/12/16) 增加对 UHF 国标数据格式的支持
//      1.13.2 (2020/12/17) SetEAS() API 增加对 UHF 的支持
// V1.14 2021/1/18
//      1.14.1 (2021/1/18) 用蓝牙方式打开盘点读写器(原先是用串口方式)。可自动感知蓝牙变化，重新初始化读写器
//      1.14.2 (2021/1/27) 改进锁控板开门和探测的可靠性
//      1.14.3 (2021/1/28) 改进锁控板开门和探测可靠性，激进一点的版本
//      1.14.4 (2021/8/10) RfidDriver 中增加 URL105 型号数据
//      1.14.5 (2021/8/13) 为 LED 驱动的 Display() API 增加了 uninitialized 错误码
//      1.14.6 (2021/12/24) 修改 M22 型号 XML 信息
//      1.14.7
//      1.14.8 (2022/1/20) 安装后 Windows 程序组 DigitalPlatform 里面的名字从“dp2-RFID中心”改为“RFID中心”
//      1.14.9 (2022/1/20) 增加 RD5200 型号的元数据 (<sub_id>680601</sub_id>)
//      1.14.10
//      1.14.16 NET 类型在 driverName 为空时候尝试用 RD5100 探测
//      1.14.17 (2023/11/2) RfidCenter 增加了一个 API SetEAS1()，在原来 SetEAS() 基础上修改了返回对象的结构，强加了一个 ChangedUID 字段。用于 UHF 标签修改 EAS 等以后引起 UID 变化时方便前端获知新的 UID
//                          增加了 UM200 这个 UHF 读写器型号的驱动(注: 厂家说 UM200 的驱动也可以用来打开原来的 M60 型号的读写器)
//              (2023/11/9) RfidDriver.First 中 GetTagInfo() 有一处优化读取 EPC 中 UMI 为 off 的 UHF 标签的 User Bank 的情形出现了 bug，把原本 HF 标签的 UID 当作 EPC Bank 进行了判断。此 bug 已经修正
//              (2023/11/13) OneTag 结构增加了 RSSI 成员。TagInfo 增加了 RSSI 成员
//                          新增加两种型号的 UHF 读写器的 XML 元数据。product id 对应于 900003 和 900007
//              (2023/11/21) SetEAS() API 返回的结构中增加了一个 OldUID 成员。(此前版本已经增加了 ChangedUI 成员)
//              (2023/11/22) SetEAS() API 中增加对 UHF EPC 的 CRC-16 校验，跳过一些不必要的尝试动作
//                          对一些地方的 PII 改为适应 UII
//              (2023/11/24) SetEAS() 修改高校联盟的 EPC 的时候优化为仅仅改变一个 word


