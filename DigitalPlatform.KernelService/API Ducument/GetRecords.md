# GetRecords() – 从结果集获得记录

## 用途

从一个结果集当中获得记录。

结果集当中包含的记录是通过 Search() 或 SearchEx() API 放入的。

## 接口定义
```csharp
        public Result GetRecords(
            string strResultSetName,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out Record[] records)
```

## 参数

### strResultSetName

结果集名。检索命中的结果被自动放入这个结果集当中。

### lStart

记录的起始位置。第一条记录的 lStart 值为 0。

### lLength

希望获得的记录数。

值为 -1 表示尽可能多地获得记录。如果为 0，表示不想获得任何记录。

### strLang

语言。会对返回的数据中的路径中的数据库名部分产生影响。比如路径为 "中文图书/1"，当 strLang 为 "zh" 时，返回的路径仍然是 "中文图书/1"；当 strLang 为 "en" 时，返回的路径变为 "Chinese Books/1"。

### strStyle

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

## 返回值
### Result.Value
-1  出错
其它  获得的记录数
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
