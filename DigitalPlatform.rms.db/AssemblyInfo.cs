using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//
// 有关程序集的常规信息是通过下列
// 属性集控制的。更改这些属性值可修改与程序集
// 关联的信息。
//
[assembly: AssemblyTitle("DigitalPlatform.rms.db")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("digitalplatform")]
[assembly: AssemblyProduct("DigitalPlatform.rms.db")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4D63C547-5A12-4D19-9835-E166F4D3D069")]

//
// 要对程序集进行签名，必须指定要使用的密钥。有关程序集签名的更多信息，请参考 
// Microsoft .NET Framework 文档。
//
// 使用下面的属性控制用于签名的密钥。
//
// 注意:
//   (*) 如果未指定密钥，则程序集不会被签名。
//   (*) KeyName 是指已经安装在计算机上的
//      加密服务提供程序(CSP)中的密钥。KeyFile 是指包含
//       密钥的文件。
//   (*) 如果 KeyFile 和 KeyName 值都已指定，则 
//       发生下列处理:
//       (1) 如果在 CSP 中可以找到 KeyName，则使用该密钥。
//       (2) 如果 KeyName 不存在而 KeyFile 存在，则 
//           KeyFile 中的密钥安装到 CSP 中并且使用该密钥。
//   (*) 要创建 KeyFile，可以使用 sn.exe(强名称)实用工具。
//       在指定 KeyFile 时，KeyFile 的位置应该相对于
//       项目输出目录，即
//       %Project Directory%\obj\<configuration>。例如，如果 KeyFile 位于
//       该项目目录，应将 AssemblyKeyFile 
//       属性指定为 [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) “延迟签名”是一个高级选项 - 有关它的更多信息，请参阅 Microsoft .NET Framework
//       文档。
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]

//
// 程序集的版本信息由下列 4 个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 您可以指定所有这些值，也可以使用“修订号”和“内部版本号”的默认值，方法是按
// 如下所示使用 '*':

[assembly: AssemblyVersion("3.9.*")]
[assembly: AssemblyFileVersion("3.9.0.0")]

//      2.2 第一个具有版本号的版本。特点是增加了SearchEx() API，另外Record结构也有改动，增加了RecordBody成员
//      2.3 records 表中会自动增加一个列 newdptimestamp
//      2.4 records 表中会自动增加两个列 filename newfilename -- 2012/2/8
//      2.5 支持4种数据库引擎
//      2.51 新增freetime时间类型 2012/5/15
//      2.52 Dir() API 中检索途径节点的 TypeString 对时间检索途径包含 _time _freetime _rfc1123time _utime 子串
//      2.53 将GetRes() API的nStart参数从int修改为long类型 2012/8/26
//      2.54 实现全局结果集 2013/1/4
//      2.55 GetRecords()/GetBrowse()等的 strStyle 新增 format:@coldef:xxx|xxx 功能
//      2.56 大幅度改进 WriteRecords() 等 API，提高了批处理 I/O 的速度 2013/2/21
//      2.57 2015/1/21 改进 CopyRecord() API 增加了 strMergeStyle 和 strIdChangeList 参数。允许源和目标的对象都保留在目标记录中
//      2.58 2015/8/25 修改空值检索的错误。( keycount 和 keyid 风格之下 不正确的 not in ... union 语句)
//      2.59 2015/8/27 GetRecords()/GetBrowse()等 API 中 strStyle 的 format:@coldef:xxx|xxx 格式，其中 xxx 除了原先 xpath 用法外，还可以使用 xpath->convert 格式。
//      2.60 2015/9/26 WriteXml() 对整个操作超过一秒的情况，会将时间构成详情写入错误日志
//      2.61 2015/11/8 Search() 和 SearchEx() 中，XML 检索式的 target 元素增加了 hint 属性。如果 hint 属性包含 first 子串，则当 target 元素的 list 属性包含多个数据库时，顺次检索的过程中只要有一次命中，就立即停止检索返回。此方式能提高检索速度，但不保证能检索全命中结果。比较适合用于册条码号等特定的检索途径进行借书还书操作
//      2.62 2015/11/14 GetBrowse() API 允许获得对象记录的 metadata 和 timestamp
//      2.63 2015/11/16 WriteRes() API WriteRes() API 允许通过 lTotalLength 为 -1 调用，作用是仅修改 metadata
//      2.64 2016/1/6 MySQL 版本在删除和创建检索点的时候所使用的 SQL 语句多了一个分号。此 Bug 已经排除
//      2.65 2016/5/14 WriteRecords() API 支持上载结果集。XML 检索式为 item 元素增加 resultset 属性，允许已有结果集参与逻辑运算。优化 resultset[] 操作符速度。
//      2.66 2016/12/13 若干 API 支持 simulate style
//      2.67 2017/5/11 GetBrowse() API 支持 @coldef: 中使用名字空间和(匹配命中多个XmlNode时串接用的)分隔符号
//                      例如: "id,cols,format:@coldef://marc:record/marc:datafield[@tag='690']/marc:subfield[@code='a']->nl:marc=http://dp2003.com/UNIMARC->dm:\t|//marc:record/marc:datafield[@tag='093']/marc:subfield[@code='a']->nl:marc=http://www.loc.gov/MARC21/slim->dm:\t";
//                      | 分隔多个栏目的定义段落。每个栏目的定义中：
//                      ->nl:表示名字空间列表。多个名字空间之间用分号间隔
//                      ->dm:表示串接用的符号，当 XPath 匹配上多个 XmlNode 时用这种符号拼接结果字符串
//                      ->cv:表示转换方法。以前的方法，这样定义也是可以的 xxxx->cccc 其中 xxxx 是 XPath 部分，cccc 是 convert method 部分。新用法老用法都兼容
//                      '->' 分隔的第一个部分默认就是 XPath。
//      2.68 2017/6/7 为 WriteRes() API 的 strStyle 参数增加 simulate 用法 (当写入对象资源时)
//      2.69 2017/10/7 为 GetRes() 和 WriteRes() API 的 strStyle 参数增加 gzip 用法
//      3.0 2018/6/23 改为用 .NET Framework 4.6.1 编译
//      3.1 2018/9/20 GetRecords() API 中当 strStyle 为 @remove:resultset_name 用法时，resultset_name 部分可以用逗号间隔的多个全局结果集名。单个全局结果集名的第一个字符可以是 '#'，也可以不是
//      3.2 2018/10/12 广泛应用 CancellationToken 提高 down 时候的敏捷度
//      3.3 2019/5/13 RefreshDb() API 增加了 "start_endfastappend" 动作
//      3.4 2021/6/9 MySQL 前端连接库换用 MySqlConnector https://github.com/mysql-net/MySqlConnector
//      3.5 2022/1/25 keycount 方式检索的时候，在 doItem() 函数中增加一步合并 key count(调用新增的 MergeCount() 函数)
//		3.6 2022/10/20 当数据库关闭的时候，增加了关闭 StreamCache 和 PageCache 的语句。早先版本因为缺乏这些语句造成了 bug，在有的情况下 dp2kernel service 停止了还无法删除对象目录中的某些没有被关闭的对象文件
//      3.7 2025/1/16 pgsql 各种表中可能用到 like 'xxx%' 算法的字段，都增加了 collate "C" 定义
//                  检索过程增加了超时中断机制。改进了前端请求中断检索的效果。
//      3.8 2025/1/21 Search() API 中 strOutputStyle 参数可以使用 sortby:key 或者 sortby:id。缺省表示 sortby:id。需和 keyid 或者 keycount 配置使用。
//      3.9 2025/5/16 keys 检索点中 convert 方法增加了 usmarctrim
