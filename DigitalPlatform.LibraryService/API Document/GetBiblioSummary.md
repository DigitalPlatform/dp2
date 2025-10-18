# GetBiblioSummary -- 获得书目摘要 Retrieve Bibliographic Summary Information

## 用途

获得一条书目记录的书目摘要信息。


## 权限

需要 getbibliosummary 或 order 普通权限，或同名存取定义权限。

## 接口定义
```csharp
        // 从册条码号(+册记录路径)获得种记录摘要，或者从订购记录路径、期记录路径、评注记录路径获得种记录摘要
        // parameters:
        //      strItemBarcode  册条码号。可以使用 @refID: @bibliorecpath: 前缀
        // Result.Value -1出错 0没有找到 1找到
        // 权限:   需要具备 getbibliosummary 或 order 权限
        public LibraryServerResult GetBiblioSummary(
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary)
```

## 参数

### strItemBarcode
册条码号。可为 "xxxx.xxxxx" 形态，点的左侧表示机构代码，右侧表示册条码号
可以使用 @refID: @bibliorecpath: 前缀。
- @refID:xxxx 表示册记录的参考 ID 为 xxxx
- @bibliorecpath:xxxx 表示书目记录路径为 xxxx

### strConfirmItemRecPath
册记录路径，或订购记录路径，或期记录路径，或评注记录路径。

如果 strConfirmItemRecPath 内容形态为 xxx|xxx，右边部分表示书目记录路径，API 会直接使用这个书目记录路径，而不从 strItemBarcode 进行检索了。

如果 strItemBarcode 参数中使用了 @bibliorecpath: 前缀，则本参数无效。

如果 strItemBarcode 参数值命中多于一条(或 strItemBarcode 参数值为空)，并且 strConfirmItemRecPath 参数不为空，则用本参数值进行检索

### strBiblioRecPathExclude
希望排除掉的书目记录路径。可以是一个路径，也可以是多个路径，中间用逗号分隔。
若果命中的书目记录路径在这些路径之列，则直接返回 result.Value = 1。但 strBiblioRecPath 和 strSummary 均返回空。这样设计的目的是为了方便调用者进行排除判断：当前端有缓存的书目记录摘要时(但不确定册条码号)，以册条码号请求本 API，当正巧这一册所从属的书目记录匹配请求时提交的 strBiblioRecPathExclude 参数值，则免去从数据库中检索书目记录的动作。

特殊地，如果 strBiblioRecPathExclude 参数值包含 "coverimage"，表示希望在 strSummary 中包含封面图片 URL。

### strBiblioRecPath (out)
返回书目记录路径。

### strSummary (out)
返回书目摘要信息。



## 返回值
### Result.Value
-1 出错
0 没有找到
1 找到
### Result.ErrorInfo
错误信息
### Result.ErrorCode
错误码

## 实时统计

若书目摘要是从缓存存储中获得的，对指标 "获取书目摘要/存储命中次" 增量一次

若书目摘要是从书目库中获得和构造的，对实时统计指标 "获取书目摘要/构造次" 增量一次

## 附注

参数 strItemBarcode 和 strConfirmItemRecPath 参数值不应同时为空。

API 创建书目摘要是利用相关书目库下的 /cfgs/summary.fltx 配置文件进行的。如果书目库下缺乏此配置文件，则改用书目库下 /cfgs/summary.cs 和 /cfgs/summary.cs.ref 配置文件。
