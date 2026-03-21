# Search() – 检索数据库

## 用途

利用 XML 检索式对一个或多个数据库进行检索。

注: 和 SearchEx() API 的差异是，本 API 无法直接返回命中记录，而 Search() API 可以直接返回命中记录。


## 接口定义

```csharp

        public Result Search(string strQuery,
            string strResultSetName,
            string strOutputStyle,
            out string explain)
```

## 参数

### strQuery

XML检索式。XML检索式的定义请参见 QueryXml.md。

### strResultSetName

结果集名。检索命中的结果被自动放入这个结果集当中。
后继可通过调用 GetRecords() API 获得结果集中的记录。

### strOutputStyle

检索风格。

如果包含"keycount"，表示输出 key + count形式
如果包含"keyid"，表示输出 key + id 形式
如果 keycount 和 keyid 都不具备，则表示为一般输出 id 形式
当处于 keycount 或 keyid 状态时，可以包含 sortby:key 或 sortby:id 分别代表按照命中 key 排序和按照命中 id 排序。(缺省为 sortby:id)
可以包含 desc，表示对命中结果排序采用降序；缺省为升序。


## 返回值
### Result.Value
-1  出错
其它  命中的记录数
### Result.ErrorString
错误信息
### Result.ErrorCode
错误码

