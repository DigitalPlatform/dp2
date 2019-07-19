using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DigitalPlatform.LibraryServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("digitalplatform")]
[assembly: AssemblyProduct("DigitalPlatform.LibraryServer")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6178e957-a053-4639-b899-f357f0bed006")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("3.15.*")]
[assembly: AssemblyFileVersion("3.15.0.0")]

//      2.1 (2012/4/5) 第一个具有版本号的版本。特点是增加了改造了GetIssueInfo() GetOrderInfo() GetCoomentInfo() 修改了第一参数名，去掉了第二参数
//      2.11 (2012/5/5) 为ListBiblioDbFroms() API增加了 item order issue 几个类型
//      2.12 (2012/5/15) SearchBiblio() API 对“出版时间”检索途径进行了特殊处理
//      2.13 (2012/5/16) SearchBiblio() API 通过strFromStyle中包含_time子串来识别时间检索请求
//      2.14 (2012/8/26) GetRes() API 的 nStart 参数从int修改为long类型
//      2.15 (2012/9/10) 开始进行分馆用户改造
//      2.16 (2012/9/19) Login() API增加out strLibraryCode参数
//      2.17 (2012/11/8) 为ListBiblioDbFroms() API增加了 amerce invoice 几个类型
//      2.18 (2013/2/13) 为了和 dp2Kernel 2.54 配套
//      2.19 (2013/12/21) 增加启动 log 信息
//      2.20 (2013/12/4) 读者 HTML 格式能接受 html:noborrowhistory 这样的用法 (局部还有错)
//      2.21 (2013/12/5) 读者 HTML 格式能接受 html:noborrowhistory 这样的用法
//      2.22 (2013/12/8) GetReaderInfo() 的 strBarcode 可以使用 "@barcode:" 引导，表示仅仅在证条码号中查找
//      2.23 (2013/12/8) GetSysParameters() 增加 cfgs listFileNamesEx; cfgs/get_res_timestamps
//      2.24 (2013/12/15) Borrow() Return() 允许 item 格式返回 xml:noborrowhistory; 读者 格式返回 summary
//      2.25 (2013/12/17)  GetReaderInfo() 允许 xml:noborrowhistory
//      2.26 (2013/12/30) SearchBiblio() 中, 对 strStyle 发来的 "class,__class" 能正确去重
//      2.27 (2014/1/2) SearchBiblbio() 中如果一个 formstyle 没有找到，会返回 ErrorCode.FromNotFound 错误码
//      2.28 (2014/1/15) GetBiblioInfos() API 允许前端发来多条 XML 记录，每条记录之间用 <!--> 间隔
//      2.29 (2014/3/2) GetCalendar() API 无论 strAction 是 list getcount get , strName 参数都发挥作用。为了 获得全部事项，注意 list / getcount 需要使用空值的 strName。以前版本，在 list / getcount 时候忽略 strName 参数，效果是获得全部试想；而 get 的效果是只能获得一个事项  
//      2.30 (2014/3/17) GetBiblioInfo() 和 GetBiblioInfos() API，可以使用 subcount:??? format
//      2.31 (2014/4/29) GetOperLogs() 对于 level-2 的 SetEntity SetOrder SetIssue SetComment 记录中，<oldRecord> 增加了 parent_id 属性
//      2.32 (2014/9/16) 个人图书馆功能，允许读者之间进行借还操作
//      2.33 (2014/9/24) Borrow() Return() API 允许用 @refID: 前缀的册条码号来进行借书还书
//      2.34 (2014/10/23) 允许各种功能使用评估模式
//      2.35 (2014/11/14) Borrow() API 中续借功能允许 strReaderBarcode 为空
//      2.36 (2014/11/15) Login() API 的 mac 参数，允许多个 MAC 地址，用竖线分割
//      2.37 (2014/11/17) Foregift() 和 Hire() 两个 API 都增加了两个 out 参数
//      2.38 (2014/11/26) ManageDatabase() API 的 refresh 功能，可以自动启动重建检索点的批处理任务
//      2.39 (2015/1/21) CopyBiblioInfo() API 增加了 strMergeStyle 和 strOutputBiblio 参数，SetBiblioInfo() API 增加了 onlydeletesubrecord action
//      2.40 (2015/1/25) Login() API 可以返回 token 字符串, VerifyReaderPassword() API 可以验证 token 字符串。dp2OPAC 借此实现了保持用户登录状态的功能，和第三方 SSO 跟随 dp2OPAC 登录的功能
//      2.41 (2015/1/26) Login() API 增加了对试探密码循环攻击的防范功能，每次禁止相关 IP 使用 Login() API 10 分钟
//      2.42 (2015/1/29) GetItemInfo() GetOrderInfo() GetIssueInfo() GetCommentInfo() API 增加了 strItemXml 参数。允许获得记录的检索点信息
//      2.43 (2015/1/30) GetItemInfo() API 进一步增加了 strItemDbType 参数，并包含了原先的 GetItemInfo GetOrderInfo GetIssuInfo GetCommentInfo API 的全部功能。至此，GetItemInfo() API 所取代的其他几个 API 逐渐要废止。为了保持兼容性，暂时保留一段时间这几个 API
//      2.44 (2015/4/30) GetSystemParameter() API 增加了 category=cfgs name=getDataDir 获得数据目录物理路径 
//      2.45 (2015/5/15) 文件上传和 WriteRes() API 都得到了充实，支持 dp2libraryconsole 前端进行文件上传和管理操作了 
//      2.46 (2015/5/18) 增加 API ListFile()
//      2.47 (2015/6/13) GetSystemParameter() API 增加了 category=arrived name=dbname
//      2.48 (2015/6/16) GetVersion() API 增加了 out uid 参数
//      2.49 (2015/8/23) GetRes() API 允许获得违约金库 cfgs 下的配置文件了。以前版本不允许是因为一个 bug 造成的。GetReaderInfo() API 的 strResultTypeList 参数增加了 advancexml_history_bibliosummary 这种用法
//      2.50 (2015/9/3) Return() API 的盘点功能，增加了 为 return_info 中返回信息的功能；WriteRes() 增加了 strStyle "delete" 删除盘点库记录功能(需要权限 inventorydelete)
//      2.51 (2015/9/10) SetReaderInfo() API 增加了一个新的 strAction 值 changereaderbarcode，允许在读者尚有借阅信息的情况下正确修改证件条码号
//      2.52 (2015/9/17) SetReaderInfo() API 允许使用用户定义的扩充字段。扩充字段在 library.xml 的 circulation 元素 patronAdditionalFields 属性中定义
//      2.53 (2015/9/26) Borrow() 和 Return() 增加了对总操作时间超过一秒的情况 memo 日志记录的功能。
//      2.54 (2015/9/28) ManageDatabase() 中刷新检索点定义功能，增加了对读者库选择刷新 keys 为普通状态和适合日志恢复状态的功能
//      2.55 (2015/10/16) SetReaderInfo() API 允许使用用户定义的读者同步扩充字段。扩充字段在 library.xml 的 circulation 元素 patronReplicationFields 属性中定义
//      2.56 (2015/10/18) GetReaderInfo() API 为 html 格式增加了 style_dark 和 style_light 风格。缺省为 style_light。light 对应于 readerhtml.css, dark 对英语 readerhtml_dark.css
//      2.57 (2015/11/8) Borrow() 和 Return() API 利用 dp2kernel 优化的检索式提高了运行速度
//      2.58 (2015/11/14) GetBrowseRecords() API 允许获取对象的 metadata 和 timestamp 了。这个版本要求 dp2kernel 为 V2.62 以上
//      2.59 (2015/11/16) WriteRes() API 允许通过 lTotalLength 为 -1 调用，作用是仅修改 metadata。这个版本要求 dp2kernel 为 V2.63 以上
//      2.60 (2015/11/25) GetOperLogs() 和 GetOperLog() API 开始支持两种日志类型。
//      2.61 (2015/12/9) GetReaderInfo() 允许使用 _testreader 获得测试用的读者记录信息
//      2.62 (2015/12/11) Login() API 增加了检查前端最低版本号的功能。如果用户权限中有 checkclientversion，就进行这项检查
//      2.63 (2015/12/12) Return() API，对于超期违约金因子为空的情况，现在不当作出错处理。这种情况交费信息不会写入读者记录的交费信息字段，但会进入操作日志中(便于以后进行统计)。
//      2.64 (2015/12/27) 借书和还书操作信息会自动写入 mongodb 的日志库。增加后台任务 "创建 MongoDB 日志库"
//      2.65 (2016/1/1) GetSystemParameters() API 增加 circulation/chargingOperDatabase。
//      2.66 (2016/1/2) GetBiblioInfos() API 中当 strBiblioRecPath 参数在使用 @path-list: 引导的时候，其后部允许出现 @itemBarcode:xxxx|@itemBarcode:xxx 这样的内容
//      2.67 (2016/1/6) GetItemInfo() API 的 @barcode-list:" "get-path-list" 功能允许间杂 @refID:前缀的号码。
//      2.68 (2016/1/9) Return() API 增加了 read action。会将动作记入操作日志。ChargingOperDatabase 库也会自动更新
//      2.69 (2016/1/29) 各个 API 都对读者身份加强了检查，防止出现权限漏洞。
//      2.70 (2016/4/10) 增加 MSMQ 消息队列功能。dp2library 失效日期从 5.1 变为 7.1。ReadersMonitor 后台任务会自动给没有 refID 元素的读者记录增加此元素
//      2.71 (2016/4/15) 对各个环节的密码相关功能进行加固。GetReaderInfo() API 不会返回 password 元素；GetOperLog() GetOperLogs() API 会滤除各种密码
//      2.72 (2016/5/14) SearchBiblio() API 支持按照馆代码筛选
//      2.73 (2016/5/18) MaxItemHistoryItems 和 MaxPatronHistoryItems 的默认值都修改为 10
//      2.74 (2016/5/21) GetOperLogs() API 返回的 amerce 操作的日志记录，无论何种详细级别，都不去除 oldReaderRecord 元素
//      2.75 (2016/5/30) GetMessage() API 加入了获取 MSMQ 消息的功能
//      2.76 (2016/6/4) 将读者记录和册记录中的借阅历史缺省个数修改为 10 
//      2.77 (2016/6/7) 读者身份的账户在登录时 dp2library 会给其权限值自动添加一个 patron 值(如果读者记录 state 元素有值则会从权限值中删除可能存在的 patron); 工作人员身份的账户在登录时 dp2library 会给其权限值自动添加一个 librarian 值
//      2.78 (2016/6/8) Return() API 在某些特殊情况下会无法清除册记录 XmlDocument 中的 borrower 和其他元素，新版本在此环节做了多种尝试，如果最后依然无法从 XmlDocument 删除元素，则(API 会)报错，并把错误情况写入 dp2library 错误日志。dp2library 失效期改为 2016.11.1
//      2.79 (2016/6/9) 增加放弃取书通知。优化 Reservation() API 中重复写入册记录和读者记录的情况
//      2.80 (2016/6/13) ChangeReaderPassword() API，即便是工作人员身份，也可以通过 strReaderOldPassword 参数决定是否验证旧密码。null 表示不验证。
//      2.81 (2016/6/25) SetUser() API 在创建新用户的时候允许 binding 字段使用 ip:[current] 表达自动绑定 IP 的要求
//                      GetSystemParameter() API system/outgoingQueue 可以获得 MSMQ 队列路径
//      2.82 (2016/8/31) ManageDatabase() API 可以管理 _biblioSummary 类型的数据库。特殊类型名字改为前方以字符 _ 引导
//      2.83 (2016/9/17) GetSystemParameter() API 增加 category=utility 里面的 getClientAddress 和 getClientIP 两个功能
//      2.84 (2016/9/26) WriteRes() API 允许具备 managedatabase 权限的用户写入任何路径的对象，主要是用于修改内核数据库下属的配置文件
//      2.85 (2016/9/28) GetSystemParameter() API system/version 可以获得 dp2library 版本号。BindPatron() API 可以使用 PQR:xxxx 方式进行绑定
//      2.86 (2016/10/7) GetIssues() API 允许在 strStyle 中使用 query:xxx 参数，实现仅对某一期的期记录的获取。
//      2.87 (2016/10/22) Login() API 允许工作人员代理工作人员登录而不使用 token 字符串。这时通道使用的是代理账户的权限。
//      2.88 (2016/10/30) 为登录过程首次实现 ip: router_ip: 筛选功能。通道显示的 Via 合并了两类 Via 和 IP 地址。
//      2.89 (2016/11/3) dp2library 可以使用 * 作为序列号，这样最大通道数为 255，而且永不失效。
//      2.90 (2016/11/6) 消除 首次初始化 MSMQ 队列文件遇到异常然后挂起，但再也不会重试消除挂起状态 的 Bug。尝试将 Dir() API 和 ListFile() API 连接起来
//      2.91 (2016/11/15) GetBiblioInfos() API 增加了一种 subrecords format，可以用于同时返回下级记录的 XML。返回的最多每种下级记录不超过 10 条
//      2.92 (2016/12/3) Return() API 增加了 boxing 功能
//      2.93 (2016/12/13) SetBiblioInfo() 和 SetEntities() SetOrders() 等 API 支持 simulate 风格，或者增强原有对 simulate 的支持。内务模拟导入 .bdf 文件功能要用到这些新特性
//      2.94 (2016/12/20) 开始支持 997 的查重键和相关功能
//      2.95 (2016/12/21) 修改 CopyBiblio() API 移动书目记录后没有返回正确时间戳的 bug
//      2.96 (2016/12/22) SetBiblioInfo() 增加 strStyle 参数，strStyle 参数可以使用 noeventlog 值
//      2.97 (2017/1/1) 书目记录查重键生成法为 0.02
//      2.98 (2017/1/2) SetBiblioInfo() strAction 增加 checkunique 功能
//      2.99 (2017/1/12) Borrow() 和 Return() API 在读者记录中 borrow 元素超过 10 个的时候，会剪裁了读者记录再写入 OperLog 记录中。此举可以大大缓解借书册数很多的读者记录导致日志文件急剧变大的问题
//      2.100 (2017/1/16) CopyBiblioInfo() API 写入操作日志的时候，增加了 overwritedRecord 元素用于存储被覆盖位置覆盖前的记录内容
//      2.101 (2017/1/17) Login() API 在代理登录的时候，从上一个版本的做法(被代理账户为工作人员账户的时候，登录成功后使用代理账户权限)改为登录成功后使用被代理账户的权限、会自动过滤掉高于代理账户的危险权限
//      2.102 (2017/1/20) locationTypes 定义是否允许 item 元素文本值为空，要看 library.xml 中 <circulation 元素 acceptBlankRoomName 属性，缺省为 false。SetEntities() API 保存册记录时根据 locationTypes 元素对册记录的馆藏地内容进行检查，如果 locationTypes 定义允许 room 部分为空，这个版本也是不会出现(保存时拒绝的) bug 了
//      2.103 (2017/3/14) SetBiblioInfo() 的 strStyle 支持 “bibliotoitem” 在修改记录以前保存旧书目记录到现有册记录的 biblio 元素
//      2.104 (2017/3/29) 为 SetOneClassTailNumber() API 增加 memo unmemo skipmemo 三个新功能
//      2.105 (2017/4/14) 消除 SearchDup() API 中合并算法之前没有对结果集进行排序的 Bug
//      2.106 (2017/4/20) CopyBiblioInfo() API 经过较为严格的测试，修正了一些 Bug，从此前端的移动书目记录功能要求必须使用这个版本
//      2.107 (2017/4/25) 为 VerifyBarcode() API 扩充 strAction 和 out strOutputBarcode 参数，支持变换条码号功能
//      2.108 (2017/5/11) dp2Kernel 新版本 GetBrowse() API 支持 @coldef: 中使用名字空间和(匹配命中多个XmlNode时串接用的)分隔符号
//      2.109 (2017/5/23) 对 ManageDatabase() API 也写入日志了。但日志恢复功能会跳过这个类型的操作日志
//      2.110 (2017/5/30) 消除 CopyBiblioInfo() API 中账户权限不够时会发生移动不完整的 Bug。
//      2.111 (2017/6/7) WriteRes() API 的 strStyle 参数允许使用 simulate。此时不会产生操作日志
//      2.112 (2017/6/14) SetEntities() API 增加了一种 Action 为 verify
//      2.113 (2017/6/16) GetBiblioInfos() API 增加了一种格式 marc。也可以用作 marc:syntax
//      2.114 (2017/9/20) 批处理任务 大备份初步可用。对对象文件的文件指针用法进行了优化(StreamCache 类)
//      2.115 (2017/9/23) ListFile() API 中的删除文件功能，被限定在 dp2library 数据目录的 upload 和 backup 子目录。不再允许前一版本那样的 managedatabase 权限的用户删除数据目录下的任何文件
//      2.116 (2017/9/30) SetUser() API 增加了 closechannel 动作
//      2.117 (2017/10/6) dp2kernel 的 GetRes() 和 WriteRes() API 的 strStyle 增加了 gzip 选项
//      2.118 (2017/10/21) library.xml 中 channel 元素增加了 privilegedIpList 属性，可以定义特权前端 IP，这些前端可以创建 maxChannelsLocalhost 属性定义的那么多个并发通道
//      2.119 (2017/11/13) library.xml 中 circulation 元素增加了 verifyRegisterNoDup 属性，用于定义是否校验册记录登录号的重复情况
//      2.120 (2017/12/16) WriteRes() API 针对上传文件也支持 gzip 风格了。此前只是对 dp2kernel 资源上传的时候支持 gzip
//      2.121 (2018/5/15) GetBiblioInfos() API 中改进了获得 table 格式的功能，允许 table: 后面携带风格列表例如 "table:price|title"，另外 UNIMARC 格式内置了 table 格式发生能力，可以删除数据目录下的 cfgs/table_unimarc.fltx 来使用这个内置的发生模块
//      3.0 (2018/6/23) 改为用 .NET Framework 4.6.1 编译
//      3.1 (2018/7/1) GetSearchResult() API 在返回 -1 的时候，ErrorCode 的错误码不再是 CommonError，而是具体的错误码值。比如 NotFound 表示结果集不存在
//      3.2 (2018/7/17) GetSystemParameter() API 增加了 system/expire 获取 dp2library 失效日期的功能
//      3.3 (2018/7/28) SetBiblioInfo() API 增加了格式 marc(或 marcquery)，机内格式 MARC 字符串
//      3.4 (2018/8/7) SetOrders() API 所保存的订购记录里面增加了 fixedPrice 和 discount 元素。早先版本如果保存时候提交这两个元素，会被 dp2library 过滤掉
//      3.5 (2018/9/20) 这个版本要求 dp2kernel 3.1 配套
//      3.6 (2018/9/26) ListDbFroms() API 的 strDbType 新增类型 "authority"
//      3.7 (2018/9/30) library.xml 中脚本函数 ItemCanBorrow() 和 ItemCanReturn() 都增加了一个 readerdom 参数。原来的参数形态也继续兼容。
/*
ItemCanBorrow()
早期版本里，本函数的类型是这样的：
	public bool ItemCanBorrow(
		bool bRenew,
		Account account, 
		XmlDocument itemdom,
		out string strMessageText)
新版本继续兼容这种类型。但新版本提供了新的类型，能更好地参考读者记录信息：
	public bool ItemCanBorrow(
		bool bRenew,
		Account account, 
                XmlDocument readerdom,  // 这是新增加的参数
		XmlDocument itemdom,
		out string strMessageText)
ItemCanReturn()
早期版本里，本函数的类型是这样的：
	public bool ItemCanReturn(Account account, 
		XmlDocument itemdom,
		out string strMessageText)
新版本继续兼容这种类型。但新版本提供了新的类型，能更好地参考读者记录信息：
	public bool ItemCanReturn(Account account, 
                XmlDocument readerdom,  // 这是新增加的参数
		XmlDocument itemdom,
		out string strMessageText)
 * */
//      (续上) 3.7 版本还为 locationType//item/@canBorrow 属性扩展了用法，除了继续兼容以前的 'yes' 和 'no' 值以外，还可以
//      使用 javascript 脚本定义，如 "javascript:xxxxx"。脚本执行前，宿主给准备好了 account readerRecord itemRecord 三个变量。
//      脚本通过 result 变量返回 'yes' 或 'no' 表示是否允许外借。如果脚本中没有定义这个变量，默认 'no' 的效果。
//      此外，脚本返回前，可以设置变量 message 的值，这是一个字符串，宿主会用于向前端提示。
//      注意脚本函数可能会被 GetEntities() API 调用(当 strStyle 中包含 'opac' 时)，但此种情况下 readerRecord 变量值为 null，注意在 javascript 代码中用 if (readerRecord == null) 来甄别判断处理
//      3.8 (2018/10/14）对象权限开始支持 download:group2,level-2;preview:group1,level-1 这样的形态。原来的 group1,level-1 形态也继续兼容
//      3.9 (2018/11/23) GetDupSearchResult() API 的 DupSearchResult 类增加了一个成员 Detail，描述检索过程，也就是权值是如何加起来的
//      3.10 (2018/12/1) SetEntities() API 中的 info.Style 可以使用 autopostfix 风格，用途是当 info.Action 为 'new' 或 'forcenew' 时，当册条码号或者登录号发生重复的时候，自动给这两类号码后面增加随机的后缀字符串，以保证册记录创建成功
//      3.11 (2019/1/11) GetSystemParameter() API 增加 system/rfid 定义
//              library.xml 中增加 rfid 元素
//      3.12 (2019/6/1) library.xml 中增加 barcodeValidation 元素。和配套的机制
//      3.13 (2019/6/9) RefreshDatabase 在 includeFiles 中包含 "browse" 的时候，会处理 browse_xxx 这样的配置文件模板文件。以前的版本不会处理 browse_xxx 这样的
//              开始支持人脸识别功能
//      3.14 (2019/6/23）GetRes() API 的 strStyle 参数支持使用 "uploadedPartial" 表示操作都是针对已上载临时部分的。比如希望获得这个局部的长度，时间戳，等等。这个特性暂时只支持静态文件
//      3.15 (2019/7/12) 新的 barcodeValidation 机制，会用馆藏地(而不是馆代码)字符串来作为 libraryCode 参数进行条码校验和变换。配套的内务前端也可发来组名，条码校验规则里面用通配符一般可以适应这两种情况