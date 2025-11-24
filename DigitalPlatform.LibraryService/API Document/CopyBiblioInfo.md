
为什么复制书目记录的时候，要检查下属实体库是否具备 setitemobject(目标) 和 getitemobject(源) 权限？
假设不做针对源的 getiteminfo 检查就可以复制。如果这个账户本来对源实体库不具备 getitemobject 权限，他看不到源实体库记录下属的对象记录。
但他可以这样：把书目记录连带下属实体记录复制到目标书目库(和下属实体库)，这样，源实体记录的对象也一并复制过来了，
那复制后，他就可以从目标实体库看到这个原本看不到的对象了，这就绕过原来的限制了。
为了杜绝这种漏洞，那么复制的时候，就要检查他是否具备针对源实体库的 getitemobject 权限。

另外，假设不做针对目标实体库的 setitemobject 检查就可以复制，那突破了目标实体库的原则，
这也是不该被允许的。


# CopyBiblioInfo() – 复制书目记录 Bibliographic Information

## 用途

复制或移动书目记录

```C#

        public LibraryServerResult CopyBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strNewBiblioRecPath,
            string strNewBiblio,
            string strMergeStyle,
            out string strOutputBiblio,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp)
```

## 参数

### strAction
本参数指定了具体的动作。其值应当为 "copy" "move" "onlycopybiblio" "onlymovebiblio" 之一。  

“copy”表示复制书目记录及下属的各类下级记录。不但要复制源书目记录到目标位置，也要一并复制源书目记录下属的册、期、订购、评注记录到属于目标书目记录的位置。
“move”表示移动书目记录及下属的各类下级记录。不但要移动源书目记录到目标位置，也要一并移动源书目记录下属的册、期、订购、评注记录到属于目标书目记录的位置。  
“onlycopybiblio”表示仅复制源书目记录到目标位置，而不复制源书目记录下属的册、期、订购、评注记录。  
“onlymovebiblio”表示仅移动源书目记录到目标位置，而不移动源书目记录下属的册、期、订购、评注记录。  

### strBiblioRecPath
要复制或移动的源记录的路径

### strBiblioType
源书目记录的类型。目前只能用值 "xml"

### strBiblio
源书目记录内容。目前只能用值 null

### baTimestamp
源书目记录的时间戳。当 baTimestamp 参数值为空的时候，baTimestamp 并不参与判断，也就是说，即便在操作移动或者复制的前一瞬间源书目记录被修改了，本API处理过程也不会报错返回，而是会继续执行完 API。

而当 baTimestamp 参数值不为空的时候，baTimestamp参数值就要参与判断了，API处理过程会核实操作的源书目记录的时间戳是否和baTimestamp参数值一致，如果不一致则会报错。

### strNewBiblioRecPath
目标书目记录的路径。值除了可以用确定的路径例如“中文图书/611”这样的以外，还可以用追加形态的路径例如“中文图书/?”
一般常用追加形态的路径进行复制或移动操作。

### strNewBiblio

strNewBiblio参数指定了要额外写入目标书目记录位置的记录内容。如果此参数的值为空，则表示将源书目记录移动或者复制到目标位置就完成任务；如果此参数的值不为空，则表示移动或者复制操作后，还要把此参数的值作为记录内容覆盖写入目标记录位置。也就是连复制/移动带修改的意思。

### strMergeStyle
当目标记录已经存在时，如何合并源和目标这两条书目记录的本体部分和下级记录部分。其值为 reserve_source/reserve_target/missing_source_subrecord/overwrite_target_subrecord 之一或者逗号间隔组合。   
本参数为空等于 “reserve_source,combine_subrecord”效果；   
reserve_source 表示本体采用源书目记录；  
reserve_target 表示本体采用目标书目记录；   
missing_source_subrecord 表示丢失来自源的下级记录(保留目标原本的下级记录)；    
overwrite_target_subrecord 表示采纳来自源的下级记录，删除目标记录原本的下级记录(注：此功能暂时没有实现)；   
combine_subrecord 表示组合来源和目标的下级记录。

loose 表示宽松方式。缺省为严格方式。
严格方式下，要求当前账户权限能读取源记录和写入目标记录的所有实际出现的字段，包括 dprms:file 元素。如果不具备，则会报错(AccessDenied 错误码)。
宽松方式下，拷贝或者复制过程因为读、写权限不足可能会丢失部分字段(file元素)和下属对象，本 API 不会报错。

### [out] strOutputBiblio
返回目标位置实际保存的书目记录 XML 内容。

注: 虽然 strNewBiblio 提交了希望保存到目标位置的书目记录内容，似乎并不需要 API 返回实际保存的书目记录内容。但因为 API 执行时根据当前账户的实际情况，可能拒绝一些数据字段但保存成功，这时候请求者若要了解到底保存成功了什么内容，就需要利用版参数了。

### [out] strOutputBiblioRecPath
返回目标位置实际写入的书目记录路径。

当参数 strNewBiblioRecPath 值的路径中记录 ID 部分为问号，表示追加保存书目记录的时候，本参数让请求者可以了解实际保存的书目记录路径(ID)

### [out] baOutputTimestamp
如果本 API 成功，则本参数返回目标记录新的时间戳。
如果本 API 以错误码 TimestampMismatch 返回错误(表明 baTimestamp 参数提交的时间戳和当前源记录的时间戳不匹配)，则本参数返回数据库中的源位置的记录的实际时间戳(注意：此时不是目标记录的时间戳)。也就是说，后继再次用这个时间戳填入 baTimestamp 参数进行本 API 请求，应该会成功，不会出现 TimestampMismatch 报错。  

受到册记录中册条码号字段内容不允许重复的规则制约，在strAction参数值为"copy"的操作中，源书目记录下属的册记录被复制到目标位置的同时，API 还自动对册记录中的册条码号内容附加一个随机的字符串，以避免出现册条码号重复的现象。这样，操作者就应该意识到在 API 调用完成后，需要继续额外在前端软件界面上操作，修改这些复制出来的新册记录的册条码号字段内容，修改为实际需要的，才算完满。移动操作不会有这种问题。
另外，复制到目标位置的册、期、订购、评注记录中的参考ID字段内容都会被重新赋值，以避免出现重复现象。

无论是复制还是移动创建的书目记录，册记录、订购记录、期记录、评注记录，都将完整复制或者移动源记录本体所包含的对象资源到目标位置。

目标书目库若配置了“联合编目”特性也会对本API的行为发生影响。详细情况请参考SetBiblioInfo() API的相关说明。



## 返回值

### Result.Value
-1  出错
0   成功，没有警告信息。
1   成功，有警告信息。警告信息在 Result.ErrorInfo 中返回
### Result.ErrorInfo
错误信息
### Result.ErrorCode
错误码

## 权限：
需要 setbiblioinfo 或 writerecord 或 order 普通权限，或同名存取定义权限。
如果源书目记录中包含 dprms:file 元素和对象记录，则额外需要 setbiblioobject 或 setobject 权限。


如果移动或者复制操作涉及到在目标位置创建下级记录，则额外需要一些权限，如下:

册记录，setiteminfo 或 writerecord 权限。若包含 dprms:file 元素和对象记录，则额外需要 setitemobject 或 setobject 权限；
订购记录，setorderinfo 或 writerecord 或 order权限。若包含 dprms:file 元素和对象记录，则额外需要 setorderobject 或 setobject 权限； 
期记录，setissueinfo 或 writerecord 权限。若包含 dprms:file 元素和对象记录，则额外需要 setissueobject 或 setobject 权限；
评注记录，需要setcommentinfo 或 writerecord 权限。若包含 dprms:file 元素和对象记录，则额外需要 setcommentobject 或 setobject 权限；

注: 当 strMergeStyle 参数中包含 "loose" 部分时，为宽松方式，复制或移动过程因为读、写权限不足可能会丢失部分字段(file元素)和下属对象，本 API 不会报错。

# 相关说明

## 数据库缺乏

当源书目库具有下属实体库，而目标书目库下缺乏实体库时，若进行复制操作，
源书目记录下属的册记录会被丢失。这种情况 API 会报错。

其它下级库缺乏时类似。

## 关于通过存取定义设置书目权限的说明

TODO: 
当帐户被配置了存取定义，通过存取定义来设置对数据库的权限，这时将一个数据库中的记录拷贝到另一个库，例如A库到B库，那么需要操作帐户有A库的读权限，有B库的写权限。有可能帐户对A库的读权限范围比较小（例如只有读某些字段），但对B库的写权限比较大（例如可写所有字段），那么还是否校验写权限的字段范围要小于等于读权限的字段范围呢？  
思考：针对复制或移动，这种临时组合的源库和目标库，两个库的读写范围之间没有必然联系，可以不校验get和set字段权限匹配。
但是，如果针对同一个书目库的读写权限(而不是复制或移动时针对的两个书目库的分别读写权限情形)，写越过读，会有往返处理时的数据完整性问题。这种情况需要报错。

示例
```
中文图书:getbiblioinfo=*(200);
测试中文:setbiblioinfo=new 
```