# SetBiblioInfo() – 设置书目信息 Set Bibliographic Information

## 用途

创建、修改、删除书目记录。

## 接口定义

```
       public LibraryServerResult SetBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            string strStyle,
            out string strOutputBiblioRecPath,
            out string strOutputBiblio,
            out byte[] baOutputTimestamp)

```

## 参数

## strAction

指定了具体的操作动作。值为"new" "change" "delete" "onlydeletebiblio" "onlydeletesubrecord" "checkunique" "notifynewbook" 之一。

含义如下：
new 创建一条新的书目记录；
change  修改一条已经存在的书目记录；
delete  删除一条书目记录，同时删除其下属的册记录、期记录
onlydeletebiblio    只删除书目记录，而不删除其下属的册记录、期记录、订购记录、评注记录；
onlydeletesubrecord 只删除书目记录下属的册记录、期记录、订购记录、评注记录，而不删除书目记录本身
checkunique 检查书目记录唯一性
notifynewbook 通知读者新书到达

"delete" 在删除书目记录的同时，会自动删除下属的册记录。
"delete" 和 "onlydeletesubrecord" 均要求册记录不是借出状态才能删除。

“new”表示希望创建一条新的书目记录。调用时将 baTimestamp 参数值设置为 null 即可。API 返回后，baOutputBiblioRecPath 参数中返回了实际创建的书目记录的路径，baOutputTimestamp 参数中返回了刚创建的书目记录的时间戳；
(TODO: 当记录路径为一个已经存在的路径，用 new 调用，会发生什么)

“change”表示希望覆盖保存到一条已经存在书目记录之上。调用时 baTimestamp 参数值须为已经存在的那条书目记录的时间戳。API 返回后，baOutputTimestamp 参数中返回了修改后的书目记录的时间戳；

“delete”表示希望删除一条书目记录。在删除书目记录的同时，会自动删除下属的册记录、期记录、订购记录、评注记录。删除操作前，API 会检查册记录中是否有借阅信息和未了结的费用信息，如果有则拒绝进行删除操作。调用时 baTimestamp 参数值须为已经存在的那条书目记录的时间戳。

“onlydeletebiblio”表示希望只删除一条书目记录，而不要删除其下属的册记录、期记录、订购记录、评注记录。删除后这些下级记录依然存在 parent 元素，保留了原有的从属关系。

“notifynewbook”表示希望对某条书目记录进行新书到达通知。注意使用这个功能时，strBiblioType 参数要设置拟通知的读者所从属的馆代码列表，为空表示只通知全局读者。


## strBiblioRecPath

书目记录路径。

当 strAction 参数值为"new"时，本参数一般为 "中文图书/?" 这样的追加形态，表示让系统在指定的书目库下自动追加创建一条记录(API 执行完成后，会用 strOutputBiblioRecPath 参数返回实际写入的路径)；也可以指定一个路径，表示希望创建在这个路径上(前提是这个路径不存在)。
除此以外，本参数均需要明确指定书目记录路径。

## strBiblioType

书目记录格式类型。其值可以为 "xml" "marcquery" "marc" "iso2709" 之一。
含义如下:
"xml"   MARCXML 格式
"marcquery" 数字平台 dp2 系统的 MARC 机内格式
"marc"  同 "marcquery"
"iso2709"   ISO2709 格式。以 BASE64 方式编码，本质上是 byte[]，已经以特定的编码方式打包。

注: MARC 机内格式为 `头标区24字符+字段区`。字段区格式为 `字段名1+字段内容1+字段结束符1+字段名2+字段内容2+字段结束符2...`。字段内容中可能包含字段指示符两字符。
注: "iso2709" 格式可以指定编码方式，用法如 `iso2709:gb2312`，冒号右侧表示 ISO2709 数据所采用的编码方式，缺省为 utf-8。通常 ISO2709 格式的数据必须知道其打包时的编码方式才能正确解析。

特殊地，当 strAction 参数值为 “notifynewbook” 时，表示希望对某条书目记录进行新书到达通知。注意使用这个功能时，strBiblioType 参数要设置拟通知的读者所从属的馆代码列表，为空表示只通知全局读者。


## strBiblio

书目记录。这是 XML 格式的书目记录内容。(介绍 dplibrary 中使用的两种 MARCXML 名字空间)

当 strAction 参数值为"delete"或"onlydeletebiblio"或"onlydeletesubrecord"时，本参数可以为空字符串。

## baTimestamp

书目记录时间戳。

指 API 操作前，书目记录原有的时间戳。一般可以先用一次 GetBiblioInfos() API 调用获得已有的时间戳。

## strComment

需要写入操作日志的注释字符串

## strStyle
simulate：是否为模拟操作  
force：强制写入，全局用户并具备 restore 权限   
nocheckdup  
noeventlog  
nooperations  
bibliotoitem 为册记录添加书目信息  
whenChildEmpty 是否仅当没有子记录时才删除书目记录  

## [out] strOutputBiblioRecPath

输出的书目记录路径。

当 strBiblioRecPath 中末级为问号，表示追加保存书目记录的时候，本参数返回实际保存的书目记录路径

此参数也用于，当写入书目记录前自动查重时发现了重复的书目记录，(虽然写入没有成功，但)这里返回这些发生重复的若干书目记录的路径，以逗号分隔，便于调用者进行提示和选择。

## [out] strOutputBiblio

实际保存的 XML 记录内容。

相比提交保存的记录内容，实际保存后可能多了 operation 元素。当前用户权限不够的字段，可能被改变或者没有兑现保存。

注: 当存取定义的 setbiblioinfo 权限不允许保存某些字段时，这些字段不会被保存到数据库中。
例: `中文图书:setbiblioinfo=*(200)|getbiblioinfo=*;` 这个存取定义表示允许对“中文图书”书目库进行 getbiblioinfo 的操作，可以获取到所有 MARC 字段内容；也允许进行 setbiblioinfo 的操作，但限制了 200 之外的字段不允许修改(只允许修改 200 字段)。
注: 当账户缺乏 setbiblioobject 和 setobject 权限时，保存书目记录时，XML 记录中的 dprms:file 元素内容不会被保存到数据库中。
例: `中文图书:setbiblioinfo=*,setbiblioobject=;` 这个存取定义表示允许对“中文图书”书目库进行 setbiblioinfo 的操作，可以保存书目记录的 MARC 字段内容；但不允许保存书目记录下属的对象记录内容(因为 setbiblioobject 和 setobject 均为空)，所以 dprms:file 元素内容不会被保存到数据库中(数据库已有的记录内容中的 dprms:file 元素则不允许被修改或删除)。

## [out] baOutputTimestamp

操作完成后，新的时间戳。

当 strAction 参数值为 "delete" 时，本参数返回 null。


## 返回值

### Result.Value
-1 出错
0 成功
大于0 表示查重发现了重复的书目记录，保存被拒绝。此时 strOutputBiblioRecPath 参数中返回了这些重复书目记录的路径，以逗号分隔。
### Result.ErrorInfo
错误信息
### Result.ErrorCode
错误码


## 权限

需要 setbiblioinfo 或 order 普通权限，或同名存取定义权限。

### 存取定义 setbiblioinfo 权限可用的动作

对应于 API strAction 参数，基本上每个参数值都直接对应一个权限动作名称。

可用的动作名称如下：
new
change
delete
onlydeletebiblio
onlydeletesubrecord
notifynewbook

此外，还有一套 owner... 开头的动作名称可用。表示只有当初创建这条书目记录的拥有者，才有权进行的操作。
如下:
ownerchange
ownerdelete
owneronlydeletebiblio
owneronlydeletesubrecord

注: strAction 参数值为"new"时，没有对应的 owner... 权限动作名称，因为不需要检查拥有者权限，因为创建新记录不涉及到已有记录的拥有者问题。
注: strAction 参数值 "notifynewbook"" 也没有对应的 owner... 权限动作名称。

书目记录中 998$z 子字段表示记录的拥有者账户名。当 strAction 参数值为"change""delete""onlydeletebiblio""onlydeletesubrecord"时，如果存取定义中配置了对应的 owner... 权限动作名称，则系统会检查当前登录账户名和 998$z 子字段内容是否一致，只有一致时才允许进行相应的操作。

### 读写权限完整性限制

为了保障同一账户交替请求 GetBiblioInfos() 和 SetBiblioInfo() API 过程中的数据记录完整安全，系统要求账户中定义的读书目记录的权限必须大于等于写的权限。
因为，倘若读的权限小于写的权限，则可能出现这样的情况：账户用 GetBiblioInfos() API 读到的记录内容是不完整的(因为读权限不够)，但用 SetBiblioInfo() API 写入的时候，却把不完整的内容写回去了，导致原有的完整记录被破坏了。
SetBiblioInfo() API 写入书目记录时，系统会检查账户的 getbiblioinfo 权限和 setbiblioinfo 权限，如果发现前者小于后者，则拒绝写入操作，并报错。
这里所说的大于或小于，一方面是指是否具备全字段的读写权限。有的大于没有的。例如，账户的 getbiblioinfo 权限为 `getbiblioinfo=**`，setbiblioinfo 权限为 `setbiblioinfo=`，则前者大于后者；如果账户的 getbiblioinfo 权限为 `getbiblioinfo=`，setbiblioinfo 权限为 `setbiblioinfo=*`，则前者小于后者，违反了数据安全原则，不允许写入。
另一方面，是指字段范围的大小。例如，账户的 getbiblioinfo 权限为 `getbiblioinfo=*(200,300)`，setbiblioinfo 权限为 `setbiblioinfo=*(200)`，则前者大于后者，允许写入操作；如果账户的 getbiblioinfo 权限为 `getbiblioinfo=*(200)`，setbiblioinfo 权限为 `setbiblioinfo=*(200,300)`，则前者小于后者，违反了数据安全原则，不允许写入操作。


### setbiblioinfo 和 order 权限的关系

当用户只有 order 权限而没有 setbiblioinfo 权限的时候，对于具有 orderWork 角色的书目库可以修改其中的记录，但对于没有 orderWork 角色的书目库就只能在其中创建新的记录而不能修改(删除)已经存在的记录。
当存取定义中 setbiblioinfo 和 order 权限同时存在的时候，SetBiblioInfo() API 优先使用 setbiblioinfo 权限。
例: `中文编目:order|setbiblioinfo|getbiblioinfo;` 检查权限的时候，会先尝试匹配 setbiblioinfo，如果没有这个权限，再尝试匹配 `order` 权限。即便账户存取定义中 order 写在 setbiblioinfo 的前面，也会优先匹配 `setbiblioinfo`。这样设计的目的是因为 `setbiblioinfo` 权限更强大一些，拥有它的用户可以进行更多的写操作。

### 存取定义一般知识

如果账户配置了“存取定义”参数，则存取定义优先于用户的“权限”参数起作用。例如，一个用户配置了这样的存取定义参数：
`中文图书:setbiblioinfo=new,change,delete|getbiblioinfo=*;英文图书:setbiblioinfo=new|getbiblioinfo=*`
这个定义字符串首先被分号区隔为描述不同数据库的段落，然后在每个段落中，冒号左边是数据库名，冒号右边用竖线’|’符号区隔为描述不同API的小段。这样就容易理解这个格式的结构了。

表示这个用户被允许对“中文图书”这个书目库进行SetBiblioInfo() API的strAction参数为new/change/delete的操作，onlydeletebiblio则不被允许；这个用户被允许对“英文图书”这个书目库进行SetBiblioInfo() API的strAction参数值为new的操作，不允许其他操作。也就是说，这个用户可以对中文图书库进行创建记录/修改记录/删除记录的操作，仅删除书目记录的操作不被允许；可以对英文图书库进行创建记录的操作，修改和删除和仅删除书目记录都不被允许。从存取定义代码中还可以看出，这个用户可以对两个库进行GetBiblioInfo() API的“所有”操作(其实，GetBiblioInfo() API并没有strAction参数，所以只有一种类型的操作，这里的符号’*’是为了一种格式上的整齐需要)。

如果要对一个书目库针对SetBiblioInfo() API许可所有strAction参数值指定的操作，可以定义为：
中文图书:setbiblioinfo=*

如果存取定义中没有配置针对GetBiblioInfo()或SetBiblioInfo() API的操作权限，但用户的存取定义是有的(即存取定义并不是全部为空)，那就意味着该用户不能对相应的书目库进行GetBiblioInfo()或SetBiblioInfo() API操作。

需要说明的是，用户账户的“存取定义”参数适用于联合编目等对不同书目库权限要求不同的复杂场合；而对于一般本馆业务而言，把全部书目库视为等同的“权限”角色定义体系，只用账户的普通权限，则更简单，可以有效降低维护管理的复杂性。

# 相关说明

## 联合编目 905 字段过滤

新创建书目记录的时候，如果目标书目库配置了联合编目特性值”905”(参见library.xml的<itemdbgroup>元素)，则 dp2Library 会对拟创建的书目记录 MARC 格式内容进行过滤，滤除当前用户的馆代码以外的 905 字段，然后再用于创建。
“滤除当前用户的馆代码以外的905字段”的意思是，如果MARC记录中有多个905字段，则根据这些字段的$a子字段内容，如果不符合当前用户所配置的馆代码字符串，则这些905字段会被过滤掉。


## 新书到达通知

此功能的原理是，检查指定的书目记录下是否具有评注记录。如果有评注记录，则表示有读者对这条书目记录进行了订购推荐。系统会把这些订购的读者找出来，然后对他们进行新书到达通知。

具体检查的算法是，观察评注记录中 type 元素值是否为“订购征询”，并且 orderSuggestion 元素值是否为“yes”。只有符合这两个条件的评注记录，才表示是订购推荐的评注记录。
然后还要检查读者所在的馆代码，是否符合 strBiblioType 参数中指定的馆代码列表。如果符合，才进行通知。

在总分馆模式下，各个分馆是相对独立的，而书目记录是所有分馆共享的。因此，一种常见的场景是，当分馆 A 到书了，馆员决定对当初推荐订购的读者发出到书通知的时候，肯定只希望分馆 A 的读者收到通知，分馆 B 的读者即便是当初也推荐过，但不应该收到通知。通知哪些分馆的读者，是由 strBiblioType 参数值决定的，可以提供功能的灵活性。

strBiblioType 参数值如果为空，表示只通知总馆(这其实也算一个“分馆”的读者)。如果为一个逗号间隔的列表，表示希望通知的若干个分馆的馆代码，注意列表中也可以包含空，比如 `,海淀分馆,西城分馆` 表示一共三个分馆，第一个是空。

注: 当前账户管辖的馆代码范围，必须包含 strBiblioType 参数中指定的所有馆代码，否则会报错拒绝操作。
注: 账户信息中的“馆代码”如果是空，实际上管辖所有分馆包括总馆。注意这一点语义和刚才的 strBiblioType 中的馆代码语义不同。

消息是通过站内信(dpmail)，email 和 library.xml 中定义的扩展消息接口发出。

## 测试

以请求希望匹配 setbiblioinfo:change 为例：

下列存取定义应该匹配 setbiblioinfo:change 注: 因为 ownerchange 和 change 同时具备，理解为 change
`中文图书:getbiblioinfo|order|setbiblioinfo=ownerchange,change`

下列存取定义应该匹配 setbiblioinfo:change 注: 因为 * 理解为 ownerchange 和 change 同时具备，理解为 change
`中文图书:getbiblioinfo|order|setbiblioinfo=*`

下列存取定义应该匹配 setbiblioinfo:change 注: 这是和 =* 等同的形态
`中文图书:getbiblioinfo|order|setbiblioinfo`

下列存取定义应该匹配 setbiblioinfo:ownerchange 
`中文图书:getbiblioinfo|order|setbiblioinfo=ownerchange`

下列存取定义应该匹配 order:change 注: 因为 setbiblioinfo 为否定方式。先匹配的 setbiblioinfo，后匹配的 order，因为 setbiblioinfo 被否定了，所以命中了 order 
`中文图书:getbiblioinfo|order|setbiblioinfo=`

下列存取定义应该匹配 writerecord:change 注: 因为 setbiblioinfo 为否定方式。先匹配的 setbiblioinfo，后匹配的 writerecord，因为 setbiblioinfo 被否定了，所以命中了 writerecord 
`中文图书:getbiblioinfo|writerecord|setbiblioinfo=`

下列存取定义应该匹配 writerecord:change 注: 因为 setbiblioinfo 为否定方式。先匹配的 setbiblioinfo，后匹配的 writerecord，因为 setbiblioinfo 被否定了，所以命中了 writerecord。另外，和存取定义中 setbiblioinfo 和 writerecord 的顺序无关
`中文图书:getbiblioinfo|setbiblioinfo=|writerecord`

指定增删改都被限定了字段范围
`中文图书:setbiblioinfo=*(200-300)`

## TODO list

