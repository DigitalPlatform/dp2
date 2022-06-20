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
[assembly: AssemblyVersion("3.121.*")]
[assembly: AssemblyFileVersion("3.121.0.0")]

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
//      2.65 (2016/1/1) GetSystemParameter() API 增加 circulation/chargingOperDatabase。
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
//      3.16 (2019/7/30) SetEntities() API 新增 transfer 子功能，用于典藏移交操作
//      3.17 (2019/8/29) GetOperLogs() strStyle 参数可包含 "wait"，表示最多等待 30 秒直到有返回结果
//      3.18 (2019/11/6) Borrow() strStyle 参数可包含 overflowable，表示允许超额临时借书，主要是用于智能书柜。此时读者记录中的 borrow 元素里要写入 overflow 属性
//      3.19 (2019/11/9) Borrow() 用 overflowable 方式借阅图书时，册记录中要写入 overflow 元素
//      3.20 (2019/11/11) Return() 中 transfer 功能进行了修正。为 strStyle 中的 location 和 currentLocation 子参数进行了 StringUtil.Unescape()
//      3.21 (2020/2/18) BatchTask() API 启动 “大备份”任务时可以用 backup 权限就够了。backup 权限也能用 ListFile() API 下载和删除 upload 子目录以外的其他子目录，权限比较高
//      3.22 (2020/2/19) GetSystemParameter() API 当 strCategory 为 "library" strName 为 "name" 时不需要登录就可以获取到信息
//		3.23 (2020/2/25) GetRes() API 用于获得 dp2library 文件 MD5 信息的时候，可以使用 beginTask getTaskResult 等子功能，用轮询方式查看任务状态和获得结果。这可适应非常大的物理文件
//		3.24 (2020/3/1) ListFile() API 以前版本有时会出现获取当日操作日志文件尺寸陈旧的问题。现已消除
//		3.25 (2020/3/2) 超期通知批处理任务会自动给所有的读者记录加上 libraryCode 元素。SetReaderInfo() API 无论是 new 还是 change 动作都会自动给读者记录加上 libraryCode 元素
//		3.26 (2020/3/20) 重构代码，使用 MongoDB.Driver 2.10.2。修正了 dp2OPAC 安装包中 opac_app.zip 缺大量子目录和文件的问题。
//		3.27 (2020/3/27) Borrow() 和 Return() API 的 strStyle 参数可以包含 operTime:xxx 子参数，表示本次操作为同步操作，xxx 部分是实际操作时间。程序会对实际操作时间和册记录中遗留的 checkInOutDate 元素里面的时间进行比较，有可能会拒绝同步(返回错误码 ErrorCode.SyncDenied)
//		3.28 (2020/6/21) Borrow() API 的 strStyle 参数可以包含 requestPeriod:xxx 子参数，表示本次借书操作强制使用此借阅期限(不再使用 library.xml 中借阅权限表的相关值)
//		3.29 (2020/7/16) Borrow() Return() GetBiblioSummary() API 针对 strItemBarcode 参数做了增加，允许使用 xxxx.xxxx 的形态，即点的左边是 RFID 标签的 OI 或者 AOI。相关 API 会自动检查册记录的馆藏地关联的 OI 是否和 strItemBarcode 中的一致。以前没有 . 的形态依然兼容，这种情况下不进行 OI 检查
//						GetSystemParameter() API 中增加了 "rfid/getOwnerInstitution" 这一种 catagory，用于获得图书或者读者的 OI。返回的字符串为 "xxx|xxx" 格式，左边是 OI，右边是 AOI
//		3.30 (2020/7/16) 997 字段查重键的构成算法，增加了一个版本项(UNIMARC 205$a/MARC21 150$a)和一个 998$k 子字段。key 算法的版本号(997$v)从 0.03 变为 0.04，以前版本的 997 key 需要重建
//		3.31 (2020/8/27) Borrow() API 要在读者记录和册记录中产生 borrowID 属性或者元素，并且 Borrow() 和 Return() API 的 BorrowInfo 和 ReturnInfo 结构中增加了 BorrowID 成员
//		3.32 (2020/8/27) GetItemInfo() API 返回 XML 记录的时候，记录中会包含一个即时发生的 oi 元素
//		3.33 (2020/8/28) 为 SetSystemParameter() API 增加了操作日志。前端可以通过拉取此类日志记录感知服务器配置参数的变化
//		3.34 (2020/9/4) 增加调整超额这一种操作日志记录类型 adjustOverflow
//		3.35 (2020/9/8) Borrow() API 在读者记录中创建的 borrows/borrow 元素中增加了 oi 属性。GetReaderInfo() API 返回的读者记录 XML 中增加了 oi 元素
//		3.36 (2020/9/11) 配合 dp2ssl 发布正式版，dp2library 专用版本号
//		3.37 (2020/10/12-14) SetEntities() API 的 Action 增加 "setuid" 子功能；Style 增加 "onlyWriteLog" 表示只写入操作日志，不修改册记录(注意操作日志记录中 style 元素里面有请求的 strStyle 值可供判断)
//		3.38 (2020/10/27) SetBiblioInfo() API 的 strStyle 增加 "whenChildEmpty"，表示只有当书目记录没有下级记录时才允许删除，否则会返回错误码 AccessDenied。
//		3.39 (2020/11/10) Return() API 的 "transfer" 功能，strStyle 参数可以包含 shelfNo:xxx 子参数，用于请求修改册记录的永久架位
//		3.40 (2020/12/11) SetEntities() API 和 Return() API 的 "transfer" 功能，新的册记录中 currentLocation 字段内容可以用这样的形态: *:xxx 或者 xxx:*，其中星号表示不变的部分
//		3.41 (2020/12/21) SetEntities() API 的 delete action 功能，style 可以包含使用 "force_clear_keys"，用于删除 XML 结构已经被破坏的册记录，作用是提醒 dp2kernel 层(根据记录 id)强制删除册记录的检索点 key
//		3.42 (2020/12/23) 进一步巩固检索和删除册记录过程中遇到的 XML 部分被破坏(对象文件删除)场景下的功能完整性
//		3.43 (2020/12/29) GetReaderInfo() SetReaderInfo() 等环节支持读者记录中的 palmprint 元素
//		3.44 (2021/2/5) GetBiblioInfos() 获得下级记录功能("subrecords:item")消除一个关于 ErrorCode 的 bug
//		3.45 (2021/3/4) Amerce() VerifyReaderPassword() API 增加了对 OI.barcode 类型条码号的支持
//		3.46 (2021/3/4) Borrow() 和 Return() API 中返回册记录和读者记录的时候，增加了一种格式 oi，可以返回记录的机构代码。另外 xml 格式返回的内容中也确保增加了 oi 元素
//		3.47 (2021/3/8) Borrow() 和 Return() API 中的 strReaderBarcode 参数可以正确使用 xxxx.xxxx 形态了
//		3.48 (2021/3/11) GetReaderInfo() API 中获取 advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary 格式时如果账户不具备 getbibliosummary 权限，会在返回的 XML 记录 summary 中相关 summary 属性值中放入报错信息，方便系统管理员排错
//					另外 GetReaderInfo() API 增加了 json 和 advancejson 两种格式，对应于 xml 和 advancexml。其他配套的 advancexml_xxx 不变
//		3.49 (2021/3/16) GetItemInfo() API 的 strResultType 参数值增加了 uii 一种类型，可以返回册记录的 UII
//		3.50 (2021/4/6) SetEntities() API 中修改和创建册记录的时候，会自动删除其他册记录中重复的 uid 字段。注意要确保所有实体库的 keys 中定义了 RFID UID 检索点
//		3.51 (2021/4/28) Borrow() API 续借时会清除读者记录中的 borrow 元素下先前残留的 notifyHistory 属性，这样避免续借以后(ReadersMonitor)发出超期通知时被这个属性抑制
//		3.52 (2021/5/11) GetBrowseRecords() API 对返回的册记录 XML 自动添加了 oi 元素
//		3.53 (2021/6/8) SearchCharging() API actions 参数增加 noResult 表示不返回 results，只返回 result.Value(totalCount)。用于 dp2OPAC 的 BorrowHistoryControl 中单纯获取读者借阅历史的事项数(而不需要返回事项)
//		3.54 (2021/7/4) ChangeUserPassword() API 在修改密码的时候，不再要求前端先 Login() 成功
//		3.55 (2021/7/7) Return() API 中，如果 strStyle 参数带有 operTime 子参数，会用这个子参数来计算超期时间。以前的版本这里处理有误，是用 dp2library 服务器当前时钟来计算超期时间了
//						Login() API 中，以临时密码登录时，临时密码会自动转为正式密码，最新版正式密码的失效期会根据 library.xml 中 login/@tempPasswordExpireLength 属性值来决定。如果此属性缺省，相当于一个小时的长度。找回密码的手机短信的失效期也受这个参数控制(和以前版本的默认值兼容)
//		3.56 (2021/7/9) 读者密码被设置的时候，是否有失效期属性，会根据 library.xml 中 login/@patronPasswordExpireLength 参数和读者记录中的 rights 中是否包含 neverexpire 权限综合决定
//		3.57 (2021/7/9) SetUser() API 创建账户时同时设置密码，如果密码不符合强密码规则，会出现 account 元素创建成功但密码为空的结果。此 bug 已经修正
//						(发正式版)
//		3.58 (2021/7/9) ReadersMonitor 中对每一条读者记录的处理中，增加了
//						根据 library.xml 中 login/@patronPasswordExpireLength 和读者记录的 rights 元素，
//						添加或者清除读者记录中 password/@expire 属性的步骤。这样当 library.xml 配置发生改变后，后台批处理会自动去修改读者记录
//		3.59 (2021/7/9) 此前版本 SetMessage() API 中发送 dp2 内置邮件给一般收件者，不会发送成功。此 bug 在最新版中已经修正。
//						此前版本 ChangeReaderPassword() API 修改读者密码时，(强密码状态下)检查新密码时不允许和证条码号、读者姓名相同，这项检查没有兑现。此 bug 在最新版中已经修正
//		3.60 (2021/7/15) GetReaderInfo() SetReaderInfo() 初步实现了账户 getreaderinfo:n setreaderinfo:n 分级权限控制
//		3.61 (2021/7/15) GetReaderInfo() API 的返回数据类型增加 structure 类型，用于返回记录结构定义
//		3.62 (2021/7/16) library.xml 中 login/@patronPasswordStyle 属性，和 accounts/passwordStlye 属性，可以用 "style-1,login" 这样的属性值，其中 login 表示工作人员或者读者身份登录的时候 dp2library 会自动检查密码强度，如果密码强度不够，登录会失败，报错信息提示密码强度不够，需要修改密码后重新登录 
//						dp2installer 的 dp2library 实例对话框中增加了 checkbox “停用本实例”
//		3.63 (2021/7/19) GetSearchResult() 和 GetBrowseRecord() API 对书目和读者 XML 记录和 Cols 巩固了按照当前权限进行过滤
//		3.64 (2021/7/20) 继续完善 GetSearchResult() 和 GetBrowseRecord() API 的存取控制
//		3.65 (2021/7/20) SetReaderInfo() API 中(strAction 为 'change' 时) strNewXml 内容的 XML 根元素可以带一个 importantFields 属性，表明保存时候关注的重要字段，如果这些字段被拒绝保存，请求就会整个失败(确保不会发生仅有部分字段被修改的情况)
//		3.66 (2021/7/20) BindPatron() API 的 strStyle 参数增加了一种值 singlestrict 表示如果以前存在同类型号码，本次绑定会失败
//		3.67 (2021/7/22) 账户权限中 getreaderinfo:xxx 和 setreaderinfo:xxx 中，xxx 代表字段定义。字段定义用法如下:
//						数字|g_xxxx|元素名
//						其中，“数字”是先前版本中的 1-9 的数字；“g_xxxx” 是组名；“元素名”即读者 XML 记录中的 XML 元素名，例如 face readerType 这样的
//		3.68 (2021/7/23) SetReaderInfo() API 修改读者记录的时候，若指定了 importantFields，现在是当实际发生的修改超出 importantFields 范围才会报错。(前面版本是只要 importantFields 中列出的元素超过当前用户的可修改元素范围就会报错)
//						账户权限中 getreaderinfo:xxx 和 setreaderinfo:xxx 其中的 xxx 部分可以使用的元素名增加了 dprms.file，代表数字对象 <dprms:file> 元素。名字中用点是为了规避权限字符串中的冒号
//		3.69 (2021/7/28) 发布正式版
//		3.70 (2021/7/29) 当读者权限中包含 getreaderinfo 时, GetReaderInfo() API 以读者身份获得自己的读者记录的时候，structure 里面会返回限定元素 displayName 和 preferrence。此前的版本这里不正确，会返回 [all]
//		3.71 (2021/7/30) 读者身份登录时，根据 reader 账户权限和读者 XML 记录 rights 元素合成最终权限的过程，会正确处理 getreaderinfo:n 和 setreaderinfo:n 的合并
//		3.72 (2021/8/2) ResetPassword() API 做了改进，当读者记录的权限(由 reader 账户权限和读者记录 rights 元素合成而来)中包含 denyresetmypassword 时，不允许重设该读者的密码
//		3.73 (2021/8/3) SetReaderInfo() API 除了原先的具有 setreaderinfo 权限的用户具有删除读者记录的权限外，新增允许具有 setreaderinfo:xxx|r_delete 权限的用户删除读者记录，不过还有一个限定条件是拟删除的读者记录中包含的数据元素不超过此用户的可修改字段范围
//		3.74 (2021/8/4) 对 getreaderinfo:xxx 权限中 xxx 部分元素名序列中，处理好 ?name 和 name 互相覆盖的情况
//		3.75 (2021/8/5) GetReaderInfo() API 的 formats 参数中可以使用 xml:name|department 这样的格式来限定返回的 XML 元素。元素名列表的用法，和权限字符串中 getreaderinfo 冒号后面的用法一样
//						SetReaderInfo() API 提交保存的读者 XML 记录中，根元素的 dataFields 属性可指定元素名列表(逗号间隔)，表示 XML 记录中只有这些元素才是提交保存的内容，此外的其它元素都不会被保存时采用。这样可以提交很少的内容但避免了删除不该删除的字段。如果 dataFields 属性缺省，表示不进行这种限定，那么效果就是请求所提交的 XML 记录中所有元素都会被保存考虑，例如 XML 记录中未包含的元素会被当作删除此元素
//		3.76 (2021/8/6) SetReaderInfo() API 重构。将一些校验数据的过程移到合并好记录以后的阶段进行
//		3.77 (2021/8/8) SetReaderInfo() API 继续重构。完善删除功能
//		3.78 (2021/8/9) SetReaderInfo() API 对于前端提交的 refID 元素，如果最终合成的记录里面没有 refID，会采纳前端提交的 refID。如果这时前端并没有提供 refID，则服务器会随机发生一个 refID
//						GetReaderInfo() API 一般情况下返回的读者 XML 记录中会包含完整的 borrowHistory 元素。(此前的版本会滤除 borrowHistory 元素的 InnerXml)
//		3.79 (2021/8/9) SetReaderInfo() API 兑现了对 refID 的查重；另外修正了各处重复情况下的 ErrorCode 用法
//						前端请求中提交的读者 XML 记录根元素的属性 dataFields="" 等同于 dataFields="[none]" 效果。dataField 属性缺乏才等于缺省效果，逻辑上相当于 dataFields="[all]" 效果，其中 [all] 代表全部可用的元素
//		3.80 (2021/8/12) SetReaderInfo() API 中增加了检查当前账户权限定义的步骤。要求 getreaderinfo:xxx 中的元素集完整包括 setreaderinfo:xxx 中的元素集。如果不满足，就报错提示修改账户权限满足要求。此前版本在 change 动作时基本元素集有一步和 getreaderinfo:xxx 元素集交叉，这一步已经取消。change 动作和 new delete 动作一样，都只认 setreaderinfo:xxx 中的元素集
//		3.81 (2021/8/17) 先前版本的 Return() API 当册有超期情况时不会在日志记录中写入 borrowID。这一 bug 已经修正
//						先前版本 GetReaderInfo() API 在处理 formats 为 xml:noborrowhistory 时有 bug，已经修正
//		3.82 (2021/8/19) library.xml 中增加 circulation/@patronMaskDefinition 定义读者记录马赛克的方法。该属性的缺省值相当于 "name:1|0,tel:3|3,*:2|0"
//						(重申说明)library.xml 中 circulation/@borrowCheckOverdue 属性定义 Borrow() API 借书的时候，是否检查读者记录中未还超期册。这个属性缺省值为 "true"，表示要“检查”，意思就是说如果读者记录中有未还超期册，那么不允许借其它图书。注：如果读者记录中有未了结的交费事项无论如何是不允许外借的，需要先处理这些交费
//		3.83 (2021/8/24) Borrow() API 在续借的时候，以前版本会重设册记录的 borrowID 元素为新值。现在改为不改变 borrowID 原值。但倘若册记录中 borrowID 元素续借时候如果不存在，则会自动创建一个 borrowID 元素赋予一个新的 GUID 值(这种情况会出现在很久以前的版本 Borrow() API 借书产生的册记录中。不过这只是理论推测。一旦时间很久了没有还，可能会超期，就没法续借了，所以这种续借时候发现册记录中 borrowID 元素不存在的情况遇到的概率也不是很大)
//		3.84 (2021/8/25) Borrow() 和 Return() API 支持在 strStyle 参数中使用 ",comment:xxxx"，以便在操作日志记录中写入 clientComment 元素
//		3.85 (2021/8/26) Borrow() API 的 strStyle 参数内可以使用 ",special:dontCheckOverdue|dontCheckAmerce" 特性，效果分别是“不检查潜在超期册”和“不检查待交费信息”。不过这两项特性都需要当前账户具有 specialcharging 权限。操作日志记录中会写入 special 元素，元素文本内容是 strStyle 参数中 ",special:xxx|xxx" 片段的 xxx|xxx 部分
//		3.86 (2021/8/27) 巩固 SetReaderInfo() API。force change 情况下，如果从数据库中读出的 XML 不合法，不会报错。也就是说可以用 force change 来强制修改一条读者记录
//			 (2021/8/28) 继续巩固 SetReaderInfo() API 和 dp2kernel 模块中的相关功能，确保对象文件被损坏或者物理删除的读者记录，可以被 SetReaderInfo() API 的 delete 和 (force)change 功能删除和修改
//		3.87 (2021/8/29) 工作人员账户密码，和读者 XML 记录中的密码，新增了 bcrypt 算法类型。读者 XML 记录中的临时密码继续沿用 SHA1 算法
//		3.88 (2021/8/31) SetEntities() API 中增加了检查保存到实体库的 XML 记录中 parent 元素的步骤，要求 parent 元素内容必须是一个纯数字
//		3.89 (2021/9/2) LibraryApplication.LoadCfg() 如果完整装载没有成功，那么当后面文件系统探测到 library.xml 变化，重新装载的时候，会采用完整装载的方式来装载。
//		3.90 (2021/9/3) 修复 GetOperLogs() 和 GetOperLog() API 中过滤内嵌读者 XML 记录中过滤 password 元素的一处 bug
//		3.91 (2021/9/10) SearchBiblio() 和 SearchItem() API 的 strOutputStyle 中可以使用 desc 来让命中结果集内的 ID 按照降序排序。缺省为升序。
//						注: 检索式 XML 中的 order 元素的作用还需要另外评估
//		3.92 (2021/9/12) SearchBiblio() 和 GetSearchResult() API 支持 dp2library 本地结果集排序。用法是 GetSearchResult() API 的 strBrowseStyle 参数内使用 sort:1|2,sortmaxcount=1000 子参数
//		3.93 (2021/9/13) dp2Kernel 中的 browse 配置文件先前已允许使用 <root filter='marc'> 方式了，最新版扩展了 col/use 元素内容的用法，允许使用逗号间隔的多个名字
//		3.94 (2021/9/15) GetSearchResult() API 中 sort:xxx 命令中列 -0 表示用路径排序的倒序
//		3.95 (2021/10/9) SetSystemParameter() API 中 category 为 "circulation" name 为 "script" 时，改为先编译 C# 脚本代码，如果编译有错则不会改变 LibraryDom
//		3.96 (2021/10/21) ResetPassword() API 中检索读者记录命中限制数从 10 修改为 500。并且，如果达到这个命中数，会在错误日志中记一笔警告信息
//		3.97 (2021/10/25) ReadersMonitor 后台任务增加了专门的错误日志文件类型，文件名以 readersmonitor_ 开头。目前仅实现了 MQ 类型的动作写入此日志，其他类型的动作待添加写入功能
//		3.98 (2021/11/24) library.xml 中增加 messageServer 元素用于定义 dp2mserver 消息服务器参数。当定义了消息服务器参数的情况下，
//							dp2library 写入操作日志的时候会自动向 _dp2library_xxx 群发送一条通知消息，其它前端可以据此即时感知到操作日志的变动
//							SetReaderInfo() API 会对账户权限 getreaderinfo:xxx 和 setreaderinfo 的情况返回“账户定义错误”(因为读字段权限范围小于写字段范围)，防止此时覆盖损坏读者记录
//		3.99 (2021/11/30) GetRes() API 中 strStyle 中 getTaskResult 功能增加 dontRemove 参数，表示不删除 task
//							另外 strStyle 增加了 removeTask 功能
//		3.100 (2022/1/7) MD5 Task 管理模块修正了一个涉及到 FinishTime 的 bug。此 bug 会让任务过早被自动清除
//		3.101 (2022/1/10) dp2library 的 C# 脚本编译改为 Roslyn
//		3.102 (2022/1/18) RepairBorrowInfo() API 优化改进。返回的出错信息里面增加了 XML 标记
//		3.103 (2022/1/28) SetBiblioInfo() API action 为 “checkUnique” 时，增加了对 library.xml 中未配置查重空间和发起记录不在查重空间内两种情况进行了报错(以前版本是不报错)
//		3.104 (2022/2/26) Return() API 会根据一定的规则，还书的同时修改册记录的 currentLocation 元素。
//						规则是: Return() API strStyle 参数中的 currentLocation:xxx 优先；内务前端登录对话框里面的工作台号次之(也就是说最近一次 Login() API 的 parameters 参数中的 location 子参数)；(进行还书操作的)工作人员账户里面“位置”字段最次。按照这个顺序，非空的值会写入册记录的 currentLocation 元素内容。如果全部都是空，则不写入册记录 currentLocation 元素
//		3.105 (2022/3/3) Borrow() 和 Return() API 可以处理跨越分馆的借书还书(还没有测试完成)
//						当 Return() API 根据 strStyle 参数中的 currentLocation:xxx 子参数或者账户当前位置或者 Login() API 的工作台号来自动修改册记录的 currentLocation 字段过程中，遇到检测位置字符串内容格式出错，暂当作警告处理(而不是当作报错处理)
//		3.106 (2022/3/5) library.xml 中 rfid 元素采用新的匹配算法(允许读者 XML 记录中的 department 和 readerType 元素参与匹配)
//		3.107 (2022/3/8) SearchReader() GetReaderInfo() GetSearchResult() GetBrowseRecords() 等 API 均对馆际互借情况下，扩大了 dp2library 账户能查看的读者记录查看范围。
//						ManageDatabase() API "getinfo" 功能对馆际互借情况情况也扩大了可见的读者库范围
//		3.108 (2022/3/12) 最新版中 library.xml 文件内 rfid/@map 属性增加一种默认精确一致(不像以前默认前方一致)的匹配算法，称为 0.02 版算法。以前 0.01 版算法依然支持
//		3.109 (2022/3/18) 当 library.xml 被修改的时候(例如被 Windows 记事本直接修改)，会自动产生一个 configChanged 操作日志动作。dp2library 启动的时候也会产生一个 configChanged 动作
//						原有 setSystemParameter 动作改掉了一个会把日志记录中某些 \t 字符替换为 * 的 bug
//		3.110 (2022/3/30) SetEntities() API 当册记录有 reservations 元素时保存会报错说“超过定义范围”，此 bug 已经修正
//		3.111 (2022/3/31) 修正 SessionInfo::ExpandLibraryCodeList 中的一处 bug。当 library.xml 中 rightsTable 内没有找到指定的 code 属性值的 library 元素时，不会抛出异常，而是返回 SessionInfo::LibraryCodeList
//		3.112 (2022/3/12) WriteRes() 和 GetRes() API 增加了带宽限制的功能。由全局参数 downloadBandwidth 和 uploadBandwidth 定义，缺省为 -1 表示不限制
//		3.113 (2022/4/14) GetChannelInfo() API 返回的行信息中增加了 LastTime 列。改进了 rest.http Session 的激活(Touch)和休眠释放代码
//		3.114 (2022/4/18) MySqlBulkCopy 做了改进，当出现非法 UTF-8 内容的时候，会自动跳过有问题的行继续向后处理
//		3.115 (2022/5/12) 对 SessionTable 中的 _ipTable 做了 lock 保护。为 LibraryServrice 的 Dispose() 函数内增加了捕获异常写入错误日志的代码
//		3.116 (2022/5/17) 为 LibraryService 增加 OperationContext.Current.Channel.Closing 和 Closed 事件处理代码，尝试捕获通道释放动作
//		3.117 (2022/5/18) 消除 SetSystemParameter() API 中一处 sessioninfo = null 的 bug
//		3.118 (2022/5/20) library.xml 增加 fileShare 元素定义共享文件夹
//		3.119 (2022/5/24) Return() API 执行 "transfer" 动作时，如果册记录中没有 currentLocation 元素，本次请求修改的字符串中包含星号(表示使用原有内容)，会报错
//		3.120 (2022/5/27) SetReaderInfo() API 增加 "notifyOverdue" 和 "notifyRecall" 功能
//		3.121 (2022/6/20) 为 LibraryService 去掉 OperationContext.Current.Channel.Closing 和 Closed 事件处理代码，恢复原先用 Dispose() 的方式
//							在 Return() API 中去掉加锁册记录时重新读入册记录验证时间戳这一步骤，节省了一次读册记录的动作
