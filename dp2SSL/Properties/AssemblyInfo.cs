﻿using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2SSL")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2SSL")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

//若要开始生成可本地化的应用程序，请设置
//.csproj 文件中的 <UICulture>CultureYouAreCodingWith</UICulture>
//例如，如果您在源文件中使用的是美国英语，
//使用的是美国英语，请将 <UICulture> 设置为 en-US。  然后取消
//对以下 NeutralResourceLanguage 特性的注释。  更新
//以下行中的“en-US”以匹配项目文件中的 UICulture 设置。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //主题特定资源词典所处位置
                                     //(未在页面中找到资源时使用，
                                     //或应用程序资源字典中找到时使用)
    ResourceDictionaryLocation.SourceAssembly //常规资源词典所处位置
                                              //(未在页面中找到资源时使用，
                                              //、应用程序或任何主题专用资源字典中找到时使用)
)]


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
[assembly: AssemblyVersion("1.9.10")]    // 1.5.*
[assembly: AssemblyFileVersion("1.9.10.0")]  // 1.5.0.0

// 1.0 2019/2/21 第一个版本
// 1.1 2019/2/26 可以显示版本号了
// 1.2 2019/6/14 代码重构以后的版本，并具备人脸识别功能
// 1.3 2019/11/27 这个版本的 dp2ssl 要和 RfidCenter 1.4 版配套使用才行。基本实现了智能书柜功能
// 1.4 2020/4/2 重构算法。书柜 SaveActions() 阶段即刷新门控件上的数字，和与 dp2library 同步无关。具有本地数据库保存操作
// 1.5 2020/9/8 新增加 greensetup.exe 绿色安装，并测试完成
//      1.5.18 (2020/10/14) 消除“清掉前一个同天线号的门的图书数字 bug”
//      1.5.19 (2020/11/5) 增加 “盘点”功能
//      1.5.20 (2020/11/24) 智能书柜：修正密集开关门时的 bug；解决开门后立刻关门情形的状态可靠问题。自助借还：增加自动返回主菜单的配置参数
//      1.5.21 (2020/11/25) 智能书柜的 NewTagList 算法重构，彻底分离图书和读者证读卡器的线程和标签内存列表，以获得任何时候刷读者卡的敏捷性。增加了处理过程中防范返回主菜单的机制
//      1.5.22 (2020/11/30) 预备 1.6 版本
//      1.5.23 (2020/12/1) 简化 PageShelf 中的 PatronTags 算法，直接用 ShelfData.PatronTagList.Tags
//      1.5.24 (2020/12/4) 增加物理开关灯错误日志记载
//      1.5.25 (2020/12/7) 预备 1.6 版本，回归测试自助借还功能
//      1.5.26 (2020/12/8) RestoreRealTags() 不显示错误信息，而是直接写入错误日志文件
// 1.6 2020/12/8 增加了盘点功能，重构了开关门相关算法
//      1.6.0 (2020/12/8) 新版本
//      1.6.1 (2020/12/9) (设置为身份读卡器竖放情况下)自助借还界面返回主菜单时会残留读者信息区的红色报错信息谓语清除。这个问题已经解决
//      1.6.2 (2021/1/3) 远程控制命令增加 check tag 命令
// 1.7 2021/1/16 盘点功能增加对 SIP2 协议的支持
//      1.7.0 (2021/1/16) 增加 inventory.xml 配置文件。开始盘点对话框暂不支持“校验 EAS”功能
//      1.7.1 (2021/1/17) 增加导入 UID 对照表和清除本地 UID 缓存的功能
//      1.7.2 (2021/1/21) 书柜增加 set sterilamp time 6:00,22:40 命令
//      1.7.3 (2021/1/22)
//      1.7.4 (2021/1/25)
//      1.7.5 (2021/1/27)
//      1.7.6 (2021/1/31) 盘点功能严格检查标签 OI
// 1.8 2021/2/4 盘点功能累积改进阶段测试完成后后发布的正式版
//      1.8.0 (2021/2/4) 发布正式版
//      1.8.1 (2021/2/4) 智能书柜界面可以设置为在休眠一定时间后自动返回主菜单
//      1.8.2 (2021/2/5) 智能书柜界面返回主菜单以前会自动检查当前是否有正在处理中的后台任务(门状态变化引起的处理)，当后台任务全部完成后才允许返回主菜单
//      1.8.3 (2021/2/5) 智能书柜界面可以接受形如 ~supervisor 的工作人员身份条码扫入，设置画面增加了针对工作人员扫入条码的配置参数
//      1.8.4 (2021/2/8) 智能书柜界面如果固定读者信息的时间超过十分钟，软件会弹出对话框确认读者是否还在机器前面，如果读者没有点一次按钮则会自动清除读者信息
//      1.8.5 (2021/2/9) 增加了智能书柜界面固定工作人员信息时间太长，然后自动清除功能；还对自助借还界面也增加了读者信息固定时间太长然后自动清除的功能，只对设置为读者身份读卡器竖放的状态起作用
//      1.8.6 (2021/2/18)
//      1.8.7 (2021/2/18) shelf.xml 中增加 settings/key/@name="菜单页面显示图书馆名" 配置参数，定义在菜单页面是否显示图书馆名字
//      1.8.8 (2021/3/8) 增加下载全部和自动同步册记录和书目摘要到本地缓存功能。断网情况下连接 dp2MServer 的错误日志条目改为紧凑日志形态
//      1.8.9 (2021/3/9) 下载全部册记录功能和同步册记录功能做了改进，只下载和同步当前 dp2library 账户所管辖的馆代码内的馆藏地对应的册记录。shelf.xml 中增加配置参数 <key name="断网模式下开门前检查读者是否超期" value="true"/>
//      1.8.10 (2021/3/10) 盘点功能增加上传外部接口。在 inventory.xml 中增配参数 <uploadInterface protocol='' baseUrl='http://localhost:62022/'/>
//      1.8.11 (2021/4/1) 盘点功能增加导入 UID-->UII 对照表到 dp2library 实体库的功能。原先只能导入到 sip 方式下的本地对照库
//                          自助借还功能的 SIP 模式根据最新版 dp2capo 做了修改，能按照 dp2capo 要求在请求中包含正确的 AO 字段
//      1.8.12 (2021/4/2) 自助借还功能中，ISO15693 读者证严格要求具备 OI 字段，图书标签也严格要求具备 OI 字段
//      1.8.13 (2021/4/8) 自助借还 SIP Login() 消除了一处 bug (返回 -1 和 0 都不是登录成功，1 才是登录成功)
//      1.8.14 (2021/4/14) 发布正式版
//      1.8.15 (2021/4/15) 增加 charging.xml，提供配置事项 <key name="图书标签严格要求机构代码" value="true"/>，用于定义自助借还功能是否严格要求图书标签具备机构代码
//      1.8.16 (2021/4/15) charging.xml 中增加了 verify 属性
//      1.8.17 (2021/4/15) inventory.xml 中增加 <key name="RPAN图书标签和层架标状态切换" value="true"/> 参数，启用了新版 R-PAN 手柄切换两类标签状态功能
//      1.8.18 (2021/5/11) 设置页面增加了导入脱机册信息功能
//      1.8.19 (2021/5/11) 书柜在断网模式下，点门控件上的数字查看图书信息，新导入的册信息有时候会没有当前位置显示。这一 bug 已经修正
//                          书柜在断网模式下借出以后，读者信息里面的在借册列表信息中期限显示为空。这一 bug 已经修正
//      1.8.20 (2021/5/12) 书柜启动的时候，对书柜中现存的图书对应的、上次暂时没有来得及同步的 action 记录，会修改 State 为 "dontsync"，然后本次启动又会新增必要的 action 记录。最新版在修改 State 的同时，也会在 SyncErrorCode 字段中添加 removeRetry 值，表明原因
//      1.8.21 (2021/5/13) shelf.xml 中增加 <key name="休眠关闭提交对话框秒数" value="0"/> 参数，可配置书柜借书还书时“提交对话框”休眠多少秒以后自动关闭。另外刷读者卡会自动关闭开着的“提交对话框”
//      1.8.22 (2021/5/13) 取消 ShelfData.RemoveRetryActionsFromDatabaseAsync()。这样以前积累的尚未同步的动作无论如何后面都会尝试同步
//      1.8.23 (2021/5/14) 当书柜启动的时候，原先版本在网络良好的情况下会直接向 dp2library 服务器提交 return 和 inventory 动作，如果先前还有断网或者网络不良阶段积累的未同步动作，后面就会造成同步顺序颠倒。新版本在这里，改为无论网络是否良好，都先把请求放入队列，等后台同步线程去处理同步
//      1.8.24 (2021/5/17) 当书柜启动的时候，原先版本在网络良好的情况下会直接向 dp2library 服务器提交 return 和 inventory 动作，并直接修改本地 action 数据库记录中的 State 和 SyncOperTime 等字段。新版本去掉了这些修改动作，改为无修改地写入本地 action 库
//      1.8.25 (2021/5/17) 断网模式下，新放入书柜的图书，详细信息不会再出现“通讯失败”的报错，而是会出现“本机没有此册信息”的报错
//      1.8.26 (2021/5/18) 当书柜功能跟踪 dp2library 操作日志的时候，如果发现 new 一个册记录的日志记录，会在本地动作库中搜寻这个创建册记录操作时间以后的所有动作，把 State 修改为空，这样可以促使这些动作重新开始同步
//      1.8.27 (2021/5/19) 书柜界面刷指纹、一维码、二维码成功显示读者信息时，会自动关闭以前残留的提交对话框
//      1.8.28 (2021/5/24) GetChannel() 增加了检查并清理通道、防止通道数量过多的代码
//      1.8.29 (2021/5/25) 消除自助借还和书柜界面以读者二维码登入然后登记人脸成功后自动刷新右侧读者信息报错的 bug
//      1.8.30 (2021/6/9) 消除自助借还界面以二维码借书以后自动刷新右侧读者信息报错的 bug
//      1.8.31 (2021/6/17) 优化自助借还功能中绑定副卡时候的文字提示
//      1.8.32 (2021/6/30) shelf.xml 中可以用参数 <key name="语音提醒关门延迟秒数" value="15,10"/> 定义提醒关门的延迟时间
//      1.8.33 (2021/7/2) 智能书柜的动作同步到 dp2library 服务器，这些动作中的时间信息采用了本地软时钟。dp2ssl 启动的时候，和每隔一个小时，会自动请求 dp2library GetClock() 获得服务器时间，形成一种本地软时钟，可以逼近服务器时间
//      1.8.34 (2021/7/2) 设置页面增加临时菜单命令“* 清除动作记录的 dontsync 状态”
//      1.8.35 (2021/7/15) dp2ssl 启动时，选择断网模式的对话框，增加了延时 5 分钟后自动选择继续以断网模式启动的功能。另外如果在这个对话框选择了“以联网模式继续启动”，则后续显示的报错信息里面增加了注释文字，注明什么时间曾经弹出过对话框，人工选择了用什么模式继续启动，便于管理员诊断分析
//      1.8.36 (2021/7/21) 针对 getreaderinfo:n 和 setreaderinfo:n 权限做了适配
//      1.8.37 (2021/7/27) shelf.xml 中可以用参数 <key name="读者信息屏蔽" value="barcode:1|0,name,department"/> 定义读者信息区如何屏蔽字段文字
//      1.8.38 (2021/8/2) 新版本
//      1.8.39 (2021/8/4) 远程命令 list book xxx 其中 xxx 部分支持带有星号的通配符用法。如果没有通配符则要求精确一致匹配
//      1.8.40 (2021/8/4) dp2ssl 自动发送到点对点群中的读者刷卡开门的信息字符串中的姓名、证条码号和单位都根据 shelf.xml 中 “读者信息屏蔽” 参数，发送前做了脱敏处理
//                          dp2ssl 首次启动时向 LED 文字屏发送文字的时候如果遇到报错，会自动重试最多 5 次，一共耗费 10 秒。这是因为 RfidCenter 初始化 LED 驱动可能需要一定时间，如果 dp2ssl 启动过快会遇到报错
//      1.8.41 (2021/8/17) 远程查询功能增加修改 LinkID 字段的子功能
//      1.8.42 (2021/8/19) 远程查询功能结果发送前对 RequestItem.OperatorID 和 OperatorString 进行了 dp2ssl 的本地脱敏(按照shelf.xml 中“读者信息屏蔽”参数)
//      1.8.43 (2021/8/20) 设置画面增加"导入本地动作库"功能，可以从备份的 XML 文件中导入记录插入到当前动作库记录的前面，并且自动增量原有动作库记录的 ID 号码。
//                          “备份本地动作库”和“恢复本地动作库”功能做了改进
//      1.8.44 (2021/8/20) 修复一处和 CompactLog 有关的 bug
//      1.8.45 (2021/8/22) 远程命令 check book 和 check patron 的功能改为检查本地动作记录的合法性
//                          书柜查询命令中的时间范围可以用 8 字符的单独时间(会被自动扩展为长度一天的时间范围检索)
//                          WpfClientInfo.WriteErrorLog() 会在配置了 dp2mserver url 的情况下也自动发给 robot 聊天群
//                          dp2managecenter 增加获取文件对话框。是用点对点 getRes() API 实现的
//                          当重新启动 dp2ssl 过程中书柜内的图书标签没有发生变化的情况下，初始化过程不会向本地动作库写入盘点(和尝试还书)动作。但每 30 天之外重启 dp2ssl 会至少有一轮启动时向本地动作库中写入盘点动作
//      1.8.46 (2021/8/23) 设置页面增加菜单命令“修复已还 borrow 动作的 LinkID”
//      1.8.47 (2021/8/30) 书柜界面，读者刷卡时候右侧读者信息区显示的在借册，册行的“超期”状态原来版本是由本地缓存的册记录决定的，会不准确，新版本改为由读者 XML 记录中的 borrow 元素(的 returningDate 属性)决定
//      1.8.48 (2021/8/31) 书柜界面，增加 UI 线程未捕获的异常集中处理功能，会显示在底部错误条上
//      1.8.49 (2021/9/4) 简化书柜写入错误日志文件的信息。把启动时候的 tag 信息，和动作信息，写入到另外一个 init_xxx.txt 日志文件
//      1.8.50 (2021/9/7) 远程 API 增加 SetInfo() 的 command 操作。远程聊天命令增加 write tag。设置界面增加菜单命令可以打开写入 RFID 标签对话框
//      1.8.51 (2021/9/27) 当书柜功能中收到柜门打开信号的时候，要查看一下 door.Waiting 是否 > 0，只有 > 0 才 DecWaiting()。
//                          在非常少见的情况下，柜门关闭时会重新弹开，这时候会产生一个关闭信号和一个打开信号。其中打开信号是比较令人意外的
//      1.8.52 (2021/9/28) 书柜意外收到开门信号(指不是屏幕触发)时，这时候门没有开门者信息，dp2ssl 软件会自动把最近一次开此门的操作者当作本次操作者，并继续进行后面的操作，这一情况会写入错误日志
//      1.8.53 (2021/10/8) 远程命令增加 shutdown 命令。要求两个操作者先后在五分钟内发出 shutdown 命令，dp2ssl 才会真正进行关机操作
//                          写入标签的对话框现在改为占满全部屏幕显示(以前版本是随机的显示位置)
//      1.8.54 (2021/10/12) 针对 DisplayCount() 出现异常: “Type: System.InvalidOperationException, Message: 集合已修改；可能无法执行枚举操作。”加固了代码
//      1.8.55 (2021/10/20) 减少了各种通讯探测更新的频率。书柜远程查询支持 SyncCount 用范围 0- 或 -100 这样的范围检索式
//      1.8.56 (2021/11/15) 远程命令增加 set shutdown time 命令。配置参数画面增加了每日关机参数。配置参数对话框增加了放弃修改的“取消”按钮
//      1.8.57 (2021/11/15) dp2ssl 更新 entiryframework 和 sqlite nuget 包版本(从 3.1.5)到 3.1.21
//      1.8.58 (2021/11/17) 参数配置画面增加“每日自动更新壁纸”选项，缺省为 false。如果这里勾选了，则 dp2ssl 会每天都尝试更新壁纸，壁纸图像是存储在用户文件夹内 daily_wallpaper 文件中。
//                          如果要显示固定的壁纸，需要先清除“每日自动更新壁纸”，然后拷贝壁纸文件到用户文件夹里面，文件名为 wallpaper
//      1.8.59 (2021/11/24) dp2ssl 启动时会自动探测消息账户是否属于 _dp2library_xxx 群组，如果属于，则自动拉上操作日志轮询时间间隔，主要靠接受该群组里面的变动消息来感知操作日志变化
//      1.8.60 (2021/11/29) 在已经启用消息感知操作日志变动的情况下，当消息通讯断开后，会自动尝试重连，但如果 1) 重连失败，会把同步 dp2library 轮询间隔恢复为短时间(10 分钟)
//                          2) 重连成功，则自动补一次轮询操作日志动作(因为断开期间有可能 dp2library 日志发生过变动)
//      1.8.61 (2021/11/30) dp2ssl 会在错误日志中写入本次运行期间的网络流量统计数
//      1.8.62 (2021/12/2) 同步本地动作到 dp2library 服务器的时候，如果一个 group 的第一个动作的 State == "normalerror" 并且 SyncCount 大于一个阈值，则会延长这种 group 同步的频率，以避免通讯流量耗费过大
//      1.8.63 (2022/1/20) 自动带起来 RfidCenter 的程序组事项名从“dp2-RFID中心”改为“RFID中心”
//      1.8.64 (2022/3/12) library.xml 中 rfid/ownerInstitution/@map 增加新算法 0.02 支持
//      1.8.65 (2022/3/18) dp2ssl 书柜功能，可以自动通过同步 dp2library 操作日志感知到 library.xml 中 rfid (和 rightsTable) 元素的变化。并自动触发一次全量读者和册记录同步下载
//      1.8.66 (2022/3/19) dp2ssl 中馆员从书柜上架和下架图书的时候，如果选择同时“调拨”，软件会检查馆藏地的修改是否会导致图书所属机构代码发生变化，如果会发生变化，则自动放弃调拨(但上架下架继续执行)，并在操作结束时弹出黄色对话框提示
//      1.8.66 (2022/3/21) 全量下载册记录之前，会自动清除以前残留的全部本地缓存册记录和书目记录
//      1.8.67 (2022/3/22) 改进 dp2ssl 退出时自动保存全量下载册记录断点的功能，修正一个 bug
//      1.8.68 (2022/9/9) dp2ssl 主窗口 Activated 和 Deactivated 的时候，会关闭和打开指纹、RFID 的 SendKey。早先版本只会在 Activated 的时候关闭，不会主动去打开
//      1.8.69 (2023/1/10) dp2ssl 用到的 dp2library 账户的必备权限检查中，去掉了 setreaderobject 和 setobject 权限，因为人脸登记是依靠 facecenter 的账户进行的
// 1.9 2023/12/4 增加 UHF 自助借还功能
//      1.9.0   (2023/12/4) 对“望湖洞庭”增加专门支持。支持缺乏 User Bank 内容的“望湖洞庭”标签，并为此单独放开了 strict 模式，允许没有机构代码的 PII 成功进行借还
//      1.9.1   (2023/12/6) 当 dp2ssl 主窗口 deactivated 以后，会暂停 RfidManager 的后台盘点进程
//      1.9.2   (2023/12/7) 对空标签跳过借还操作
//      1.9.3   (2023/12/18) 自助借还界面增加图书封面显示; 改变布局算法
//      1.9.4   (2023/12/19) 暗色和亮色两种 Skin
//      1.9.5   (2023/12/22) 图书列表和读者信息 ScrollBar 样式改进为 Modern Style。这两个区域可以用触摸方式卷动。
//                          超高频国标格式标签“不存在 PII” bug 已经修正
//      1.9.6   (2023/12/28) 图书显示区域和读者信息显示区域支持触摸屏手指卷动内容
//      1.9.7   (2024/1/2) 人脸识别增加命中多个记录的功能
//      1.9.8   (2024/1/3) 人脸识别命中多个，输入密码过滤的时候，遇到密码攻击，增加了保护机制。对 PageBorrow 和 PageShelf 两处都做到了
//                          PageShelf 界面整理。右侧读者信息显示区的卷滚条正确显示了，区域内可以用手指触摸卷动
//                          PageBorrow 和 PageShelf 两个页面中的读者信息区填充处理过程以单独的线程实现，增加了敏捷度
