# GetBiblioInfos() GetBiblioInfo() – 获得书目信息 Get Bibliographic Information

## 用途

获得一条或一批书目记录的多种格式信息。

## 接口定义

```C#
        LibraryServerResult GetBiblioInfos(
            string strBiblioRecPath,
            string strBiblioXml,
            string[] formats,
            out string[] results,
            out byte[] baTimestamp)
```

```C#
        LibraryServerResult GetBiblioInfo(
            string strBiblioRecPath,
            string strBiblioXml,
            string strBiblioType,
            out string strBiblio)
```


## 参数

### strBiblioRecPath

第一种用法: 指定书目记录路径。
指定书目记录路径。
举例: `中文图书/1`

路径应当为一般数据库记录形态，例如 `中文图书/1`。若有延长的部分，比如深入到 object 层级(例如 `中文图书/1/object/0`)，则本 API 会报错拒绝。


第二种用法: 指定书目记录路径，并携带方向信息。
在书目记录路径字符串后面接续"$prev"或"$next"，表示希望获得这个位置的前一条(ID 较小)或后一条(ID 较大)书目记录的信息。
举例: `中文图书/1$next`   可以获得 `中文图书/2` 这条记录的信息

第三种用法: 成批检索式
参数值为 @path-list:xxx 形态，
则表示希望批获取摘要。
其中 xxx 部分是成批检索式，为逗号间隔的检索词列表，每个检索词为 @itemBarcode: 或 @refID: 或 @bibliorecpath 引导的内容，或书目记录路径。

此种用法中，当 formats 参数为唯一一个 "summary" 元素，返回参数 results 中每个元素为一个 summary 内容。
而当 formats 为多种格式，则 results 中会返回 格式个数 X 检索词数量 这么多的元素

举例： 
1) `@path-list:中文图书/1,中文图书/2`  表示利用书目记录路径
2) `@path-list:@bibliorecpath:中文图书/1,@bibliorecpath:中文图书/2`  表示利用书目记录路径
3) `@path-list:中文图书/1$next,中文图书/3$prev`  表示利用书目记录路径，并带有方向标志部分
4) `@path-list:@itemBarcode:B0000001,@itemBarcode:B0000002`  表示利用册条码号
5) `@path-list:@refID:xxx,@refID:xxxx`  表示利用册记录的参考 ID
6) `@path-list:中文图书/1,@itemBarcode:B0000001,@refID:xxx`  以上各种方式的混合

注: 成批检索式中每个检索词后面的 `$prev` 或 `$next` 部分，语义是针对书目记录的前一条或者后一条。
举例来说，`@path-list:@itemBarcode:B0000001$next,@itemBarcode:B0000002$prev` 表示希望获得 B0000001 所从属的书目记录的后一条书目记录，以及 B0000002 所从属的书目记录的前一条书目记录。
注意，上例中的 `@itemBarcode:B0000001$next` 部分，既不是指册条码号为 B0000001 的册记录的后一条册记录，也不是指后一条册记录所从属的书目记录。

(测试注: formats 为唯一的一种 `summary`，和 formats 为多种格式，这两种情况，都要分别针对上述成批检索式例子进行测试)

注: `@path-list:` 成批获取的多条信息会返回在 [out] results 参数中，最多可以返回多少信息存在一个极限，如果即将超过这个极限，API 会限制返回的信息数量。也就是说，返回多少信息是 API 根据当时情况决定的，请求 API 的前端要对 strBiblioRecPath 参数中请求的记录数无法满足有所准备，当返回的数量不足时，应当在下一轮请求中补足不足的部分。
API 会确保按照 strBiblioRecPath 参数中指定的先后顺序来返回结果信息，如果要丢弃一些信息，只会丢弃后面的信息。

### strBiblioXml

指定书目记录的 XML 内容。如果本参数不为空，表示不从数据库检索书目记录，而是直接使用本参数值作为书目记录内容来获得所需的信息格式。

### formats (或 strBiblioType)

(SetBiblioInfo() API 不具备本参数。而以 strBiblioType 参数指定希望返回的格式，用法和 formats 参数中使用的格式相同)

指定希望获得的信息格式，这是一个字符串数组，元素值可以在 xml/html/text/@???/summary/outputpath/metadata/targetrecpath 中选用一个或者多个。

formats参数可用值表
格式名	说明
xml XML字符串。这是记录的原始格式

html    	HTML字符串。如果为MARC格式数据库，根据内核数据库下配置文件cfgs/loan_biblio.fltx创建的HTML字符串

text    	纯文本字符串。如果为MARC格式数据库，根据内核数据库下配置文件cfgs/loan_biblio_text.fltx创建的HTML字符串

@???	    书目局部数据。例如 `@price`，表示希望获得书目记录中的价格。通过library.xml 中 C# 脚本函数 GetBiblioPart() 创建的字符串内容。GetBiblioPart() 函数的原型如下：
```
	public int GetBiblioPart(XmlDocument bibliodom,
		string strPartName,
		out string strResultValue)
```
详情可参考《参考手册》3.2小节。注意 strPartName 中是不包括 ’@’ 字符的局部名称

summary	书目摘要字符串。这个格式是由dp2Kernel层次相关书目库下的cfgs/summary.fltx配置文件定义的。如果这个配置文件不存在，软件会自动探索寻找相关书目库下的cfgs/summary.cs和cfgs/summary.cs.ref配置文件，如果这两个文件存在，则会用它们来创建摘要字符串

outputpath	书目记录的实际路径。这在 strBiblioRecPath 参数值后部包含了 ’$’ 部分时非常必要，因为这时 strBiblioRecPath 中的内容并不是书目记录的实际路径

metadata	    书目记录的元数据XML字符串

targetrecpath   书目记录 MARC 格式中 998$t 子字段的内容，即该书目记录的“目标记录路径”

iso2709:utf-8|backup	    返回 ISO2709 格式，默认 `utf-8`，不带 `|backup` 时，按照登录账号权限返回对应信息，带 `|backup` 时返回完整信息（账户需具备 backup 权限）
和 marc:syntax|backup    返回 MARC 机内格式，`syntax` 表示返回编码方式，不带 `|backup` 时，按照登录账号权限返回对应信息，带 `|backup` 时返回完整信息（账户需具备 backup 权限）

===

注: 
`marc` 格式返回的是机内格式的 MARC 记录字符串；
`marc:syntax` 返回的内容形态如 `unimarc|xxxx` 或 `usmarc|xxxx`，其中竖线左侧表示具体的 MARC 格式名称，为 `unimarc``usmarc` 之一；竖线右侧为机内格式内容。
`|backup` 会影响到返回内容的详略程度。带 `|backup` 时，返回完整信息；不带 `|backup` 时，返回的信息会依据当前登录用户的权限而有所限制。

关于 `|backup` 影响返回内容详略的情形，举例如下：
假设一个账户的存取定义权限如
`中文图书:getbiblioinfo=*(200)`，表示只允许返回中文图书这个书目库中的记录的 200 字段，其它字段不会返回。(星号表示任何动作都允许，其后的圆括号中是字段列表)
用这个账户身份请求本 API 时，formats 参数为 `marc` 或 `iso2709:utf-8`，则返回的 MARC 记录中只会包含 200 字段，其他字段会被省略掉。
然后，给这个账户增加 `backup` 权限后，再次请求本 API 时，
如果 formats 参数为 `marc:syntax|backup` 或 `iso2709:utf-8|backup`，则返回的 MARC 记录会是完整的，不会被省略任何字段。

TODO: iso2709|backup 用法为何不支持

### [out] results

用于返回结果的字符串数组。数组中每个元素和 formats 参数中列出的格式顺序是对应的。

当前用户的是否具备某些特定权限，可能会影响某些格式的数据能否获得。
当请求 API 时，formats 参数中列出了多个格式，其中某些格式的数据因不具备权限而无法获得(例如因不具备 getbibliosummary 权限无法获得书目摘要)，则 results 中对应的元素会返回空，并且 API 返回值的 .ErrorInfo 中会有关于缺乏权限的报错信息。
当请求 API 时，formats 参数中只列出了一种格式，这种格式的数据因不具备权限而无法获得，则 API 会整体返回出错。


## 返回值
### Result.Value
-1 出错
0 没有找到
1 找到
### Result.ErrorInfo
错误信息
### Result.ErrorCode
错误码

TODO: 当 formats 参数中包含多种格式时，如果某些格式因为权限不够而无法获得，应该让 Result.ErrorCode 返回 PartialDenied

## 权限

需要 getbiblioinfo 或 getrecord 或 order 普通权限，或同名存取定义权限。

举例如下：
1) 普通权限: `getbiblioinfo`
2) 存取定义: `中文图书:getbiblioinfo`
3) 存取定义，定义多个数据库：`中文图书,中文期刊:getbiblioinfo` 或等同形态 `中文图书:getbiblioinfo;中文期刊:getbiblioinfo`
4) 存取定义，控制到 MARC 字段级别: `中文图书:getbiblioinfo=*(###,200-300)` 只返回头标区和 200-300 之间的字段。
5) 存取定义，使用否定方式: `中文图书:getbiblioinfo=;*:getbiblioinfo` 表示除了“中文图书”以外其余书目库都允许获取书目信息。
注: `(###,200-300)` 中的 `###` 表示头标区。
注: 系统检查权限的时候，先检查存取定义中的 getbiblioinfo 权限，如果没有，再检查 order 权限。这是为了尽量获得 getbiblioinfo 权限中包括的字段列表，对返回的数据进行字段级精细控制。如果匹配上了 order 权限，则通常 order 权限不会携带字段列表信息，也就无法进行字段级精细控制了。

特殊地，如果 formats 参数中包含 `summary`，则要具备 getbibliosummary 或 order 权限才允许获取。
例: `中文图书:getbiblioinfo|getbibliosummary` 既允许获得书目记录的一般格式数据，也允许获得此书目记录的书目摘要格式

### getbiblioinfo 和 order 权限的关系

当存取定义中 getbiblioinfo 和 order 权限同时存在的时候，GetBiblioInfo() API 优先使用 getbiblioinfo 权限。即便账户存取定义中 order 写在 getbiblioinfo 的前面，也会优先匹配 `getbiblioinfo`。这样设计的目的是因为 `getbiblioinfo` 可以定义可读的字段范围，比 order 用起来更细致灵活，优先匹配 getbiblioinfo 通常更符合用户的权限定义习惯。
API 检查权限的时候，如果没有 getbiblioinfo 这个权限，才会尝试匹配 `order` 权限。(注: 实际上还要考虑 getrecord 权限。匹配的先后顺序是 getbiblioinfo getrecord order)
例: `中文图书:order|getbiblioinfo;` 会匹配上 getbiblioinfo 权限
例: `中文图书:order|getrecord;` 会匹配上 getrecord 权限
例: `中文图书:order` 会匹配上 order 权限
例: `中文图书:order|getbiblioinfo=*(200);` 会匹配上 getbiblioinfo 权限，并且限定只能读出书目记录中的 200 字段


