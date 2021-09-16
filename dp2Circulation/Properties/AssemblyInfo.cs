using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dp2Circulation")]
[assembly: AssemblyDescription("dp2图书馆集成系统的内务前端")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("dp2Circulation -- dp2图书馆集成系统的内务前端")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("59b4f729-e045-47f0-9afe-d02f30b94e44")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("3.37.*")]
[assembly: AssemblyFileVersion("3.37.0.0")]

// V2.6 2015/11/7 MainForm BiblioSearchForm ChannelForm 采用 ChannelPool。注意观察有无通讯通道方面的故障
// V2.7 2015/11/30 EntityForm 大幅度改造，采用 ChannelPool。Stop 类的 BeginLoop() 不再允许嵌套，注意观察是否会抛出异常。固定面板区属性页的显示很多已经改造为 PropertyTaskList 实现
// V2.8 2015/12/4 绿色安装版本启动运行的时候，如果没有按住 Ctrl 键，则优先用 ClickOnce 安装方式启动运行。不过，如果当前电脑从来没有安装过 ClickOnce 版本，就缺 开始/所有程序/DigitalPlatform/dp2内务 V2 菜单项，此时依然会以绿色方式启动
//                  启动阶段，框架窗口背景色会体现当前运行的版本，如果是绿色安装版，会显示为深绿色
// V2.9 2015/12/11 调用 dp2library Login() API 的时候发送了 client 参数
// V2.10 2015/12/14 DigitalPlatform.CirculationClient.dll 中剥离部分纯粹通讯功能到 DigitalPlatform.LibraryClient.dll。有可能部分统计方案编译报错，需要修改后发布
//      2.10.5829.27554 2015/12/17 修改了 DigitalPlatform.Drawing 和 DigitalPlatform 的 AssemblyInfo.cs 文件
// V2.11 2016/1/4 读者查询窗和读者窗在导出读者详情 Excel 文件时，可以选择输出借阅历史了。“关于”窗口里面标识了开源的情况。
// V2.12 2016/1/22 clientcfgs 子目录从数据目录移动到用户目录中了。*projects.xml 也移动了。
// V2.13 2016/3/31 启用按照每个分馆进行条码号校验的功能
// V2.14 2016/4/20 dp2library 最新版本的 Login() API 会强制检查，要求 dp2circulation 前端至少达到这个版本号
// V2.15 2016/4/27 适应 HiDPI
// V2.16 2016/5/6 报表窗 operlogxxx 表结构修改，增加 librarycode 字段。212 表统计时候可以显示没有分类号的事项，这可能是册记录删除造成的结果。
// V2.17 2016/5/8 报表窗的长操作改为用单独的线程实现。修正 202 表创建时的一个 Bug
// V2.18 2016/6/7 将 dp2circulation.xml 配置文件从绿色安装目录或者 ClickOnce 数据目录挪动到用户目录了
// V2.19 2016/9/8 为实体查询窗增加强制保存全部修改的功能(按住 Ctrl 键)。比如一册图书有借阅信息的时候一般是不让修改馆藏地的，强制修改则允许这样
// V2.20 2016/9/26 系统管理窗增加“内核”属性页，允许管理内核配置文件。报表窗改掉了没有册条码号的册记录在首次创建本地存储和以后同步修改过程中的 bug
// V2.21 2016/10/7 期刊记到界面可以从摄像头获取封面图像
// 2.22 2016/10/15 增加从龙源期刊获取封面的功能。ClickOnce 安装包中包含了 microsoft.mshtml.dll 文件
// 2.23 2016/11/3 系统管理窗 OPAC 属性页，可以为每个数据库定义一个别名。这样 dp2installer 安装 dp2ZServer 的时候可以从这里导入数据库定义。
// 2.24 2016/11/16 种册窗采用新版 GetBiblioInfos() API 一次性同时获得十条以内的册、订购、期、评注记录。此功能需要和 dp2library 2.91 版和以上版本配套使用才具备
// 2.25 2016/11/22 加拼音和著者号码都改用 http://dp2003.com/dp2library 服务器了。不过著者号码如果配置了以前的 URL http://dp2003.com/gcatserver 依然还可以兼容使用
// 2.26 2016/12/9 报表窗本地存储 operlogamerce 表解决了 'undo' action 行的 price 为空的问题
// 2.27 2017/1/14 client.cs 允许定义 TransformBarcode() 函数，对输入的条码号进行变换
// 2.28 2017/2/23 MARC编辑器中Ctrl+V复制时候盲目去除空格 bug 消除，只去除 $a 以后的一个空格; 批订购窗在固定面板区显示即时订单信息的功能基本完成。读者窗、册窗 jquery 文件目录不对的 bug 得到修正。实体查询窗用 Shift 检索也有了 parent_id 列。
// 2.29 2017/9/19 IsbnSplitter 类里面增加了处理 ISSN 的函数，中文期刊库的 dp2circulation_marc_autogen.cs 里面增加了 ISSN 13/8 转换的功能
// 2.30 2017/9/20 修正日志文件本地缓存功能中的几个 bug
// 3.0 2018/6/23 改为用 .NET Framework 4.6.1 编译
// 3.1 2018/8/13 修正种册窗对象属性页单独修改权限时，保存以后会清除对象内容的 bug
// 3.2 2019/1/26 增加 RFID 功能。快捷出纳窗可以用 RFID 标签进行借还；种册窗的册登记对话框可以写入 RFID 标签
// 3.3 2019/4/12 采用最新 dp-library submodule 的版本
// 3.4 2019/4/30 为书目查询窗和种册窗增加 Z39.50 检索功能
// 3.5 2019/9/3 消除了 rest.http 存在的通道泄露 bug
// 3.6 2019/11/27 这个版本要和 RfidCenter 1.4 版配套使用才行
// 3.7 2019/12/2 实体查询窗增加定义浏览列中的书目列功能
// 3.8 2020/1/6 实体查询窗定义浏览列中书目列功能确认正常
// 3.9 2020/2/26 登录对话框可以从 dp2ManageCenter 的服务器管理对话框里面复制到 Windows 剪贴板的 JSON 定义粘贴
//              下载和上传文件的功能中，针对最新版 dp2library 服务器时，改用任务式的获取 MD5 功能
// 3.10 2020/4/28 快捷出纳窗的 Return() API 会携带当前时间的 operTime 子参数。这样，当遇到“没有被借出”报错时候，工作人员应当收下这一册图书(如果读者坚持要拿走，则需要重新办理一次借出操作)
// 3.11 2020/9/13 种册窗“采购”属性页的上下文菜单增加“为新验收的册设置‘加工中’状态”命令。和种册窗“期”属性页的同名命令效果相同，并且关联同一个系统参数。订购窗里面的同名 checkbox 也关联同一个系统参数。相比以前版本，减少了记忆和理解维护的难度
// 3.12 2021/1/5 增加掌纹识别功能
// 3.13 2021/2/1 种册窗的册登记对话框，如果册记录的馆藏地没有在 library.xml 中定义对应的机构代码，则会报错无法创建 RFID 标签。批修改册窗也会在册记录没有定义机构代码的情况下报错。
// 3.14 2021/2/5 书目查询窗固定面板区“属性”属性页可以显示书目记录的子记录。要求配套 dp2library 3.44 使用
// 3.15 2021/3/3 修正 种册窗的册属性页中 复制列 和 删除列功能的错位的 bug
// 3.16 2021/4/8 书目查询窗和实体查询窗导出到 MARC 文件功能增加“添加-01字段”功能；“批处理/从MARC文件导入”窗增加按照 -01 覆盖书目库记录的功能
// 3.17 2021/4/13 摄像头(DigitalPlatform.Drawing)功能从原来的 AForge.NET 改用 Accord.NET 库，支持高分辨率摄像头
// 3.18 2021/5/28 实体查询窗增加根据当前账户馆代码对册记录进行过滤的功能
// 3.19 2021/6/6 读者查询窗为导出读者 XML 文件增加同时导出对象的功能。类似导出 .bdf 文件功能
// 3.20 2021/6/15
//                  书目查询窗增加“筛选”功能和“在列表中查找”功能
//                  修正 Z39.50 服务器列表对话框中刚导入一个服务器节点然后马上删除时会出现的 bug。
//                  导入 .xml 文件中服务器的功能有改进，可以识别任意位置的 server 元素(此前只能识别根元素下的 server 元素)
// 3.21 2021/6/29 实体查询窗的上下文菜单命令“设置书目栏目”在当前连接的 dp2library 是 V3 以前的版本时会处于 disabled 状态。此时只能用书目摘要方式显示书目部分一列。
// 3.22 2021/7/6 系统参数对话框“全局”属性页增加“禁用朗读”checkbox
//      2021/7/7 登录对话框工具条增加“改密码”按钮。可以修改工作人员或读者的密码
// 3.23 2021/7/22 读者窗内的编辑控件实现了根据从 GetReaderInfo() 获得格式为 "structure" 的字段列表，显示“禁止/可编辑”字段外观
// 3.24 2021/7/22 读者窗登记人脸、指纹、掌纹，或者删除的时候，SetReaderInfo() 请求里 XML 记录的根元素包含了 importantFields 属性，这样当用户(读者记录字段)权限不够的时候，保存记录会明确报错
// 3.25 2021/7/23 读者窗保存 XML 记录的时候，必要时会发送 importantFields 参数，这样当 <dprms:file> 元素发生了修改，但当前账户不具备修改 file 元素的权限时会明确报错。(以前版本这里实际上保存不会成功，但没有明确报错，会让操作者以为操作成功了)
//                  用户窗增加指定私有账户的功能，可以方便那些需要频繁修改不同账户权限然而又不想退出内务的操作
//                  读者窗装载空白内容按钮功能，增加了从 dp2library 服务器获取当前用户可修改的元素的能力，能让读者编辑器呈现出合适的字段 enable/disable 状态
// 3.26 2021/8/13 MessageHub::dp2MServerUrl 会自动将 "http://dp2003.com:8083/dp2MServer" 处理为 https 开头
// 3.27 2021/8/16 聊天窗口增加“查看更早的消息”按钮。每个群组独立保存消息起点时间。窗口刚打开的时候，每个群组的消息起点时间是当天
//                  改善断网以后重新联网，再切换群组时的确保重连 dp2mserver
// 3.28 2021/8/18 日志窗增加智能筛选功能。可以用临时输入的 C# 脚本代码来筛选日志记录
// 3.29 2021/8/25 实体查询窗上下文菜单批处理删除选择的册记录功能，增加了一个按住 Ctrl 键使用时是强制删除记录的新能力
// 3.30 2021/8/25 快捷出纳窗增加复选还书和续借的功能。要求和 dp2library 3.84 以上配套使用，并且当前账户具有 client_multiplecharging 权限(注意这个权限仅对内务前端有限制作用)
// 3.31 2021/8/26 快捷出纳窗增加特殊借阅功能。要求和 dp2library 3.85 以上配套使用，并且当前账户具有 specialcharging 权限
//                  快捷出纳窗增加复选特殊续借功能。特殊的意思是即便当前读者有未还超期册，也允许续借。值得注意的是，如果要续借的册本来就在已经超期状态了，那么续借后反而就变得不超期了，等于把前一次借书的超期给抹了
// 3.32 2021/8/28 改进日志窗里面的恢复记录属性页的“存回”功能。把原来的功能分为两种，除了原来的不弹出对话框(自动选择最后一个 XML 记录)的一种外，另外增加一种要弹出对话框选择 XML 记录的命令。
//                  读者查询窗里面的删除读者记录功能，增加了按住 Ctrl 键变为强制删除的特性。如果一般删除时遇到报错说对象文件不存在无法删除，操作者可以考虑改为尝试强制删除
// 3.33 2021/9/2 改进日志窗恢复读者记录的功能。选择 XML 记录的时候，列表界面增加了原始来源位置的列(日期:序号)
// 3.34 2021/9/9 读者窗打印读者账簿里面的失效期，日期格式从 d 改为 yyyy-MM-dd，避免某些机器上出现的 “星期三”字样
// 3.35 2021/9/10 书目查询窗和实体查询窗增加了“升序/降序”菜单命令
// 3.36 2021/9/13 导入 .bdf 修正“导入模式”的 bug
// 3.37 2021/9/16 读者统计窗当输入了办证日期和失效期筛选范围的时候，原来版本对于读者记录中的空值时间是全部通过筛选，最新版改为通不过筛选。(注：当没有输入这两项筛选日期的时候能通过筛选)