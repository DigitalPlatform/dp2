# Return() - 还书 Checkin

## 用途

还书，丢失，盘点，读过，配书，调拨。

## 接口定义

```C#

        public LibraryServerResult Return(
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            string strComfirmItemRecPath,
            bool bForce,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,
            string strBiblioFormatList,
            out string[] biblio_records,
            out string[] aDupPath,
            out string strOutputReaderBarcode,
            out ReturnInfo return_info)

```

## 参数

### strAction

动作。取值为 return/lost/inventory/read/boxing/transfer 之一

分别是 还书/丢失/盘点/读过/配书/典藏移交

配书，是指图书馆员为读者预先准备好被预约的图书，从书库取出放到特定的书架或者盒子中，等读者来了立刻可以取走

#### 盘点

strAction 为 "inventory" 时，功能是盘点册。

此种功能下，API 参数 strReaderBarcode 被用作批次号。(TODO: 将来可以考虑转移到 strStyle 参数中)

盘点功能主要分成两步：第一步通过给定的批次号和册条码号从盘点库中检索出盘点记录。
如果没有命中，则新创建一条盘点记录；如果有命中的，则修改这条已有的盘点记录。
会把一些基本信息，和本次动作前端通过 strStyle 参数发过来的 location shelfNo currentLocation
信息一并写入盘点记录。待盘点结束后，前端可以统一根据盘点记录进行相关的调拨操作。所谓调拨操作就是修改
册记录的 location(永久馆藏地) shelfNo(永久架位) currentLocation(当前馆藏地及架位)

为何不在写入盘点记录的同时就直接修改册记录实现需要的调拨动作？这是因为盘点过程中，
可能反复扫到同一册图书。比如第一次扫完才发现这一册图书不该在当前地点，然后会被放入一个书车，
等后面集中处理。集中处理时，可能会把这一册图书搬到另外一个、和一开始盘点发现的地点不同的书库，
上架，上架前执行调拨操作。从这个流程可以看出，盘点扫入当时瞬间直接修改册记录并不一定合适。

(TODO: 后面也可以考虑把立即写入册记录作为 strStyle 的一个子参数来实现，供前端选用)

盘点功能会受到账户 return 存取定义的 inventory 动作的范围参数的限制。
例 `中文图书实体:return=inventory(阅览室,基本书库),return,lost|borrow`
表示限制这个账户只能针对当前永久馆藏地已经是 阅览室 或 基本书库 的册记录进行盘点。
星号表示不限制。缺乏圆括号范围定义的时候就相当于星号效果。

#### 配书

strAction 为 "boxing" 时，功能是配书。
配书的意思是，当读者在 OPAC 中预约了在架的图书，图书马上进入到书状态，其他读者暂时被禁止办理这一册图书的借书手续。
图书馆工作人员在工作界面上看到这一在架到书信息后，到书库中为读者取书，放到出纳台的特定位置，这就是“配书”操作，然后触发对读者的通知，
读者来图书馆出纳台办理借书手续，借走图书。

Return() API 的 boxing 功能，会对相关读者记录和册记录进行修改标记，还会对相关预约到书记录进行修改标记。

对读者记录的修改如下:
根据 arrivedItemBarcode 或 arrivedItemRefID 属性值定位到 reservations/request 元素，
为此元素添加 box boxingOperator boxingDate 属性。

对册记录的修改如下:
根据 reader 属性值定位到 reservations/request 元素，
为此元素添加 box boxingOperator boxingDate 属性。

对预约到书记录的修改如下:
通过 notifyID 检索命中相关的预约到书记录，为 state 元素添加一个值 "box"；
在根元素下添加 box boxingOperator boxingDate 三个元素。

#### 调拨

strAction 为 "transfer" 时，功能是调拨。操作会修改册记录的
永久馆藏地(location)、架位号(shelfNo)等等相关字段。

本功能用到 strItemBarcode 参数，用于指明要调拨的册记录；strConfirmItemRecPath 参数确认用的册记录路径（可选），用于避免条码歧义时的二次确认。

strStyle 参数用于传递调拨的额外控制项（可采用 key=value;key2=value2 格式），常见键：
 batchNo：批次号，用于把本次调拨归入某一批次（可为空）。
 location：目标永久馆藏地（必需之一）。
 shelfNo：目标架位号（可选）。
 currentLocation：目标当前馆藏地（可选）。
 forceLog：(不用等号)是否要在册记录没有实质性修改的情况下也写入(setEntity-transfer)操作日志。

### strReaderBarcode
读者证条码号。


### strItemBarcode
册条码号。


### strComfirmItemRecPath
用于确认的册记录路径。

### bForce
是否强制执行还书操作。用于某些配置参数和数据结构不正确的特殊情况。
### strStyle
附加要求。

"reader" 表示希望返回处理完后的读者记录，在参数 record_records 中。


### strItemFormatList
指明要在 item_records 参数中返回的册记录数据格式。为 "xml" "html" 之一。

### [out] item_records
返回册记录信息。

### strReaderFormatList
指明要在 reader_records 参数中返回的读者记录数据格式。为"xml" "html"之一。

### [out] reader_records
返回读者记录信息。

### strBiblioFormatList
指明要在 biblio_records 参数中返回的书目记录数据格式，为 "xml" "html" 之一。

### [out] biblio_records
返回书目记录信息。

### [out] aDupPath
返回发生重复的册记录路径。这是因为 strItemBarcode 参数值指明的册命中了多条。

### [out] strOutputReaderBarcode
返回读者证条码号。

### [out] return_info
返回还书成功的信息。为 ReturnInfo 类型。

结构 ReturnInfo 的定义如下：

```C#
    public class ReturnInfo
    {
        // 借阅日期/时间
        // RFC1123格式，GMT时间
        public string BorrowTime;

        // 应还日期/时间
        // RFC1123格式，GMT时间
        public string LatestReturnTime;

        // 原借书期限。例如“20day”
        public string Period;

        // 当前为续借的第几次？0表示初次借阅
        public long BorrowCount;

        // 违约金描述字符串。XML格式
        public string OverdueString;

        // 借书操作者
        public string BorrowOperator;

        // 还书操作者
        public string ReturnOperator;

        // 所还的册的图书类型
        public string BookType;

        // 所还的册的馆藏地点
        public string Location;

        /// 所还的册的卷册
        public string Volume;

        // 实际用到的册条码号
        // 可能是 @refID:xxxx 形态
        public string ItemBarcode;

        // 本次还书所针对的借书事务 ID
        public string BorrowID;

        // 所还图书的借者
        // 可能是 @refID:xxx 形态
        public string Borrower;
    }

```



## 返回值

### Result.Value
 -1  出错
 0 操作成功 
 1 操作成功，但有值得操作人员留意的情况：例如有超期情况；发现条码号重复；(属于已被人预约的图书)需要放入预约架等。Result.ErrorInfo 中有详细提示信息。
### Result.ErrorInfo
错误信息
### Result.ErrorCode
错误码

## 权限

可以用普通权限或存取定义控制操作权限。当账户中有存取定义时，依存取定义；没有存取定义，则依普通权限。


### 普通权限

各种操作需要的普通权限，随 strAction 参数值有所不同。具体如下：
"return"    需要 return 权限
"lost"  需要 lost 权限
"inventory" 需要 inventory 权限
"transfer" 需要 setiteminfo 或 writerecord 权限 (注: 调拨功能实际上是底层调用 SetEntities() API 实现的，在后者那里检查的就是这两个权限值)
"read" 需要 read 权限
"boxing" 需要 boxing 权限

### 存取定义

各种操作需要的存取定义权限，随 strAction 参数值有所不同。具体如下：
"return"    circulation 操作的 return 动作
"lost"  circulation 操作的 lost 动作
"inventory" circulation 操作的 inventory 动作
"transfer" circulation 操作的 transfer 动作 // setiteminfo 或 writerecord 权限 (注: 调拨功能实际上是底层调用 SetEntities() API 实现的，在后者那里检查的就是这两个权限值)
"read" circulation 操作的 read 动作
"boxing" circulation 操作的 boxing 动作

例: `中文图书:circulation=return` 允许针对 中文图书 书目库下属的实体库中的册记录，执行 circulation 操作的 return 动作类型
例: `中文图书:circulation=*`  允许针对 中文图书 书目库下属的实体库中的册记录，执行 circulation 操作的所有动作类型
例: `*:circulation=*`  允许针对所有实体库中的册记录，执行 circulation 操作的所有动作类型
例: `*:circulation`  允许针对所有实体库中的册记录，执行 circulation 操作的所有动作类型
例: `*:circulation=`  禁止针对所有实体库中的册记录，执行 circulation 操作的所有动作类型

#### 限定操作的读者范围

在存取定义中还可以利用 reader 操作名来进一步按照读者记录维度进行限定。
(注: reader 操作名和 circulation 操作名是两个平行的操作名)

例: `*:circulation;*读者库:reader=barcode(@^\\d{7}$)`  表示操作只能针对名为读者库中的读者记录，XML 中 barcode 元素内容符合 7 位数字格式的，来进行
例: `*:circulation;*读者库:reader=name(张三)`  表示操作只能针对名为读者库中的读者记录，XML 中 name 元素内容等于 "张三" 的，来进行
例: `*:circulation;*读者库:reader=name(张三),barcode(1*)`  表示操作只能针对名为读者库中的读者记录，XML 中 name 元素内容等于 "张三" 的，并且 barcode 内容以数字 1 开头的，来进行

圆括号中可以使用通配符(即星号或问号，和 DOS dir 命令中通配符用法类似)，或者正则表达式。正则表达式需要用一个 '@' 字符引导。