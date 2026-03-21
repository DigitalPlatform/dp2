# SearchEx() – 检索数据库(并立即返回命中记录)

## 用途

利用 XML 检索式对一个或多个数据库进行检索，并立即返回命中记录。

注: 和 Search() API 的差异是，本 API 可以直接返回命中记录，而 Search() API 无法直接返回命中记录。

## 接口定义

```csharp
        public Result SearchEx(string strQuery,
            string strResultSetName,
            string strSearchStyle,
            long lRecordCount,
            string strLang,
            string strRecordStyle,
            out Record[] records,
            out string explain)
```

## 参数

### strQuery

XML检索式。XML检索式的定义请参见 QueryXml.md。

### strResultSetName

结果集名。检索命中的结果除了立即返回以外，也被自动放入这个结果集当中。
后继可通过调用 GetRecords() API 获得结果集中的记录。

### strSearchStyle

检索风格。

如果包含"keycount"，表示输出 key + count形式
如果包含"keyid"，表示输出 key + id 形式
如果 keycount 和 keyid 都不具备，则表示为一般输出 id 形式
当处于 keycount 或 keyid 状态时，可以包含 sortby:key 或 sortby:id 分别代表按照命中 key 排序和按照命中 id 排序。(缺省为 sortby:id)
可以包含 desc，表示对命中结果排序采用降序；缺省为升序。

### lRecordCount

希望返回的命中记录最大数。

值为 -1 表示尽可能多地获得记录。如果为 0，表示不想获得任何记录。
注: 本 API 总是从命中结果集的偏移量 0 开始获得记录，无法灵活指定开始偏移。

### strLang

语言。

### strRecordStyle

返回命中记录的数据格式。参数值为逗号分隔的形态，多种值可以并列和叠加使用。

其中，id 表示返回记录 ID。在 Record.Path 中。
cols 表示返回记录的浏览格式，多列。在 Record.Cols 中。
xml 表示返回记录体的 XML 格式内容。在 Record.RecordBody.Xml 中。
timestamp 表示返回记录的时间戳。在 Record.RecordBody.Timestamp 中。
metadata 表示返回记录的元数据。在 Record.RecordBody.Metadata 中。
keycount 表示返回检索命中的 key + count (检索点+命中数)形式。key 在 Record.Path 中; count 在 Record.Cols 的第一列中。
keyid 表示返回检索命中的 key + id (检索点+记录路径)形式。key 在 Record.Keys 结构中(当 strRecordStyle 中包含 "key" 时); id 在 Record.Path 中。

如果 keycount 和 keyid 都不具备，则表示为一般输出 ID 形式

注: 如果包含 "keycount"，表示返回命中的检索点和数量。这意味着不可能返回 cols 和 xml 记录体，为了节省数据成员，借用了别的成员返回数据，这时在 Record.Path 中返回命中的检索点，在 Record.Cols 的第一列返回命中数量。



### [out] records

返回命中的记录。见下文相关数据结构 Record 的定义。

### [out] explain

返回检索过程解释信息。

## 返回值
### Result.Value
-1  出错
其它  命中的记录数
### Result.ErrorString
错误信息
### Result.ErrorCode
错误码

## 相关数据结构

```csharp

    public class Record
    {
        // 记录路径，包含数据库名和记录 ID
        public string Path;
        // 相关检索点。检索命中的 Key+From 字符串集合。
        public KeyFrom[] Keys;
        // 浏览列
        public string[] Cols;
        // 记录体
        public RecordBody RecordBody;
    }

    public class KeyFrom
    {
        // 逻辑运算符
        public string Logic;
        // 检索键
        public string Key;
        // 来源字段名
        public string From;
    }

    public class RecordBody
    {
        // 记录路径
        public string Path;
        // 记录 XML 格式内容
        public string Xml;
        // 时间戳
        public byte[] Timestamp;
        // 元数据
        public string Metadata;

        // 获取记录时的返回值。可能包含报错信息。
        public Result Result;
    }

```