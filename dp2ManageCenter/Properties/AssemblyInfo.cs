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
[assembly: AssemblyVersion("1.4.7")]
[assembly: AssemblyFileVersion("1.4.7.0")]

// v1.1 (2020/2/25) 获取 MD5 采用了新的任务方式。会检查 dp2library 的版本号是否为 3.23 以上
// v1.2 (2020/2/26) 服务器管理对话框里面增加了 UID 列，新增服务器节点时会检查 UID 是否重复，重复的不允许加入
//                  服务器管理对话框里面可以复制 JSON 定义到 Windows 剪贴板；和从 Windows 剪贴板粘贴 JSON 定义创建新的服务器节点
//                  服务器对话框里面复制出去的 JSON 定义，可以被 dp2Circulation 登录对话框粘贴使用
//                  新功能：ListView 对于内容显示不全的列，鼠标停留在上方会出现 tooltips 小窗口
//                  dp2ManageCenter 创建大备份文件时，文件名采用 MC_2020_02_02_图书馆名.dp2bak 这样的格式，在内务批处理任务窗等处查看大备份文件名的时候，便于操作者把内务启动的大备份任务产生的文件和 dp2ManageCenter 产生的区别开
//                  服务器管理对话框，在确定关闭前会对服务器名和 UID 非空，服务器名之间不能重复，UID 之间不能重复进行检查。按住 Ctrl 键点确定按钮可以跳过这种检查
//                  下载文件的最后阶段，进行服务器文件和本地文件 MD5 比对的时候，采用并行的方式进行
// V1.3 (2020/2/28) 优化了大备份下载算法，节省了通道，只为每一个服务器使用一根下载通道。(此前可能要用到三根)
//                  修正了下载日备份文件时“移除”列表事项时任务没有停止的 bug
//      (2020/2/29) LibraryChannelPool 里面 GetChannel() 加锁算法进行了改进。分为两个阶段加锁，第一阶段加了读锁，如果必要在第二阶段再加写锁。这样当 pool 中有闲置通道的时候，只需要加读锁就可以了，尽量不影响其他 GetChannel() 和 ReturnChannel() 的并发性能
//                  大备份和日备份过程中，每当完成针对一个服务器的任务时，会自动释放闲置的 LibraryChannel 通道。这样能减轻 ChannelPool 执行时加锁的压力
// V1.4 (2020/4/23) 增加点对点消息窗，和书架查询窗
//      1.4.1 (2021/8/16) 书柜查询窗的列表上下文菜单增加了 修改 关联 ID 的命令
//      1.4.2 (2021/8/22) 增加获取文件对话框
//                      书柜查询窗的时间范围值可以输入类似 20200101 这样的表示一天范围的值了
//      1.4.3 (2021/9/7) 书柜查询窗增加命令按钮。可以执行 write tag 命令。用的是点对点 SetInfo() API
//      1.4.4 (2021/12/1) GetRes() API 的获得服务器文件 MD5 码的功能，在面对 dp2library 3.99 以上版本时，改用 getTaskResult,dontRemove，并且启动任务也改用 beginTask:xxxx 方式
//      1.4.5 (2021/12/6) 以前版本下载文件时探测 .~state 文件过程逻辑有缺陷。最新版增加了找不到 .~state 文件以后再探测一次原始文件是否存在的步骤
//      1.4.6 (2023/6/9) 增加密集书架伺服功能
//      1.4.7 (2023/7/7) 密集书架 3 区的通道号做了倒转处理
