# GetOperLog() – 获取操作日志
```
        LibraryServerResult GetOperLog(
                    string strFileName,
                    long lIndex,
                    long lHint,
                    string strStyle,
                    string strFilter,
                    out string strXml,
                    out long lHintNext,
                    long lAttachmentFragmentStart,
                    int nAttachmentFragmentLength,
                    out byte[] attachment_data,
                    out long lAttachmentTotalLength);

```
## 用途
获得操作日志。

## 参数

### string strFileName

日志文件名

strFileName参数指定了日志文件名。形态如“20120101.log”(日期加.log)

### long lIndex

日志记录序号

lIndex参数指定了日志记录的序号。从0开始计数。

特殊用法：如果lIndex参数值为-1，表示仅希望获得整个日志文件尺寸值(或记录数，参见 strStyle 的 getcount 值解释)，将由 lHintNext 参数返回。

注: 这里所指的日志文件尺寸，是在 dp2library 服务器上日志文件的尺寸，而不是在前端获取到的日志记录内容的尺寸。因为前端获取到的日志记录内容可能是经过过滤或者压缩过的，所以它的尺寸不一定等于服务器上日志文件的尺寸。

### long lHint
暗示参数

lHint参数指定了日志记录所在位置的暗示信息。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。在顺序获取日志记录的时候，上一次API调用返回的lHintNext值正好可用于下一次调用时设置lHint参数值。如果是首次获取日志记录无法具备这个值，或者其他情况下无法指定这个值，可用-1设置lHint参数值。

lHint参数的意义是加快API的执行速度，可避免从文件头部开始逐个获取和计算所要访问的日志记录的位置的操作。如果不想使用这个参数，也可以设置为-1，这样API会从文件头部开始逐个获取和计算所要访问的日志记录的位置，直到找到指定的日志记录位置为止，速度会略慢一点。

### string strStyle
风格

可以包含若干风格值，用逗号连接起来。例如 "accessLog,getcount"。下面列出可用的风格值：   
**accessLog** 表示希望获取访问日志。如果不包含此值，则默认获取普通日志。   
**getcount** 当 lIndex 参数为 -1 时，strStyle 中包含本值表示希望在 lHintNext 中返回当前(文件名由 strFileName 参数表示的)日志文件中的日志的记录条数；否则在 lHintNext 中返回当前日志文件的字节数。   
**dont_return_xml** 表示不要在 strXml 参数中返回日志记录内容。   
**supervisor** 表示希望在返回的日志记录中保留敏感信息。如果不包含此值，则默认不返回敏感信息，即返回的日志记录中的敏感信息都已经被滤除了。如果要使用此值，还要求当前账户的权限中具备 replication 权限，否则会报错。   
**level-n** 表示希望获取的日志记录内容的详细级别。n 代表 0 1 2 之一。数字越小越详细。如果不包含此值，效果为 level-0。   

下面是三个级别的定义：   
**0**   全部   
**1**   滤除 读者记录和册记录 XML 内容   
**2**   滤除 读者记录和册记录中的 borrowHistory 元素   


### string strFilter
过滤参数

表示希望只获取哪些特定操作类型的日志记录。这是一个逗号间隔的字符串，例如 "borrow,return"。
如果 strFilter 的值为空，表示返回所有类型的日志记录。

注: 操作类型是指日志记录中的 operation 元素的值，例如 borrow、return、writeRes 等等。前端可以根据需要设置 strFilter 参数值，以便只获取特定类型的日志记录，从而减少不必要的数据传输和处理。

### out string strXml
返回日志记录XML

strXml参数返回了日志记录。日志记录是XML格式的。有一些日志记录还有附件部分，通过另一参数 attachment_data 可返回附件内容。

### out long lHintNext

返回下一个日志记录的暗示参数

lHintNext参数返回了可用于下一次顺序访问日志记录操作的位置暗示信息。

### long lAttachmentFragmentStart

附件片段开始位置

lAttachmentFragmentStart参数指定了要获取的日志记录附件的起始位置。从0开始计数。

### int nAttachmentFragmentLength

要获取的附件片段长度

nAttachmentFragmentLength参数指定了要获取的日志记录附件的长度。如果不想获取附件，可将此参数设置为0。

### out byte[] attachment_data

返回附件数据

attachment_data参数返回了附件数据，这是一个byte数组。

### out long lAttachmentTotalLength

返回附件的总长度

lAttachmentTotalLength参数返回了附件的总长度。

### 返回值

[LibraryServerResult.Value](https://jihulab.com/DigitalPlatform/dp2doc/-/issues/98)
表示请求是否成功，或者其它状态。为如下值:   
      		
**-1** 错误   
**0** 指定的日志文件没有找到   
**1** 成功   
**2** 超过范围   

#### LibraryServerResult.ErrorInfo
出错信息。表示出错的具体情况，或者其它需要提醒请求者留意的文字信息。   
   
### 返回结果说明   
      
在postman中使用json的格式返回的响应结果，这个响应结果只是json取的名字，实际上它是LibraryServerResult结构类型，这个类型有三个返回值：ErrorCode、ErrorInfo、Value 。我们所有的接口返回的响应结果都是LibraryServerResult类型。   
例如下面这个返回结果，这个GetOperLogsResult，就是json为这个响应结果取的名字，并不是它的类型    
  
```
{
    "GetOperLogsResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 1
    },
```

### 权限
需要 getoperlog 权限。

特殊情况下，当 strStyle 参数中包含 supervisor 值，表示希望返回的日志记录中保留敏感信息时，要求当前账户具备 replication 权限。


### 说明

如果一个日志记录的附件尺寸很大，需要多次调用本API，每次获取一个片段的内容，最后组装为完整的内容。获取附件的第一个或者后继各片段的时候，可以用 -1 作为 nAttachmentFragmentLength 参数的值进行调用，这样 dp2library 服务器会尽可能多地返回附件内容(在 attachment_data 参数返回)，前端根据实际每次实际得到的 attachment_data 中的字节数，决定下一次请求的 lAttachmentFragmentStart 参数值。   

## 实例演示1 仅获得一条日志记录

本实例演示了如何通过调用GetOperLog接口来获得一条日志记录

### 请求参数

请求包要用到的参数解释：   
         
strFileName 日志名称为20230327.log   
lIndex 日志名称为20230327.log的第十条操作日志   
lHint 无暗示参数可填-1但是不可为空   
lAttachmentFragmentStart 附件开始获取的字段位置，填0代表从第一个字段开始获取   
nAttachmentFragmentLength 返回附件的长度，填-1服务器会尽力返回这个附件尽可能多的内容   

请求包的数据部分如下：   
```
{
  "strFileName":"20230327.log",
  "lIndex":"9",
  "lHint":"-1",
  "lAttachmentFragmentStart":"0",
  "nAttachmentFragmentLength":"-1"
}
```
### 响应结果
   
响应结果的返回信息详解：   
     
ErrorCode 错误代码为0，也就是没有错误   
ErrorInfo 错误信息为空，也就是没有错误   
Value 返回值为1，代表成功   
strXml 返回日志记录的Xml信息   
lHintNext 返回的暗示参数，可用于下一次的接口调用，作用于"lHint"的参数，能够加快下一次接口的执行速度   
attachment_data 返回的附件数据，是一个byte数组   
lAttachmentTotalLength 返回附件的总长度   

响应信息如下：   
   
```
{
    "GetOperLogResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 1
    },
    "strXml": "<root><operation>writeRes</operation><operator>supervisor</operator><operTime>Mon, 27 Mar 2023 19:12:04 +0800</operTime><requestResPath>中文图书/1/object/1</requestResPath><resPath>中文图书/0000000001/object/1</resPath><ranges>3584000-3662714</ranges><totalLength>3662715</totalLength><metadata>&lt;file mimetype=\"image/jpeg\" localpath=\"C:\\Users\\klein\\Pictures\\FLAMING MOUNTAIN.JPG\" /&gt;</metadata><style>gzip</style><clientAddress via=\"net.pipe://localhost/dp2library/XE\">localhost</clientAddress><version>1.06</version></root>",
    "lHintNext": 3698349,
    "attachment_data": [
        31,
        139,
        8,
        0,
        /*由于返回的byte数组太多，所以这里展示省略大部分的返回附件*/
        51,
        1,
        0
    ],
    "lAttachmentTotalLength": 78758
}
```

## 实例演示2 停止循环

在GetOperLog接口，我们获取操作日志是用日志记录序号来表达我们要获取哪个操作日志。要获取全部日志的话，通常是日志序号从零开始，每次获取递增序号，直到获取完一天内所有的日志记录，这样循环获取。
那么我们怎么停止循环呢，只要填写的日志序号超过日志记录中的最后一位日志序号，那么返回结果中的返回值（Value）就会返回一个”2“（2表示超出范围），这样的话循环就停止了。
需要注意的是，如果是当天日期的操作日志，则后续停止循环时需要我们查看日志序号的最后一条是多少，因为当天的日志记录会随着我们的操作不断增加，所以最后一条日志记录在当天结束之前不能确定，它会随着系统操作的增加而变化。往期的日志记录则不会变化。

### 请求参数

请求包要用到的参数解释：   
   
strFileName 日志名称为20230327.log   
lIndex 日志名称为20230328.log的第14条操作日志；lindex中填写的日志序号超过了原有的日志序号（原有日志序号最后一条为13）   
lHint 无暗示参数可填-1但是不可为空   
lAttachmentFragmentStart 附件开始获取的字段位置，填0代表从第一个字段开始获取   
nAttachmentFragmentLength 返回附件的长度，填-1服务器会尽力返回这个附件尽可能多的内容   

请求包的数据部分如下：
   
```
{
  "strFileName":"20230328.log",
  "lIndex":"14",
  "strStyle":"",
  "strFilter":"",
  "lHint":"-1",
  "lAttachmentFragmentStart":"0",
  "nAttachmentFragmentLength":"-1"

}
```
### 响应结果

响应结果的返回信息详解：   
  
ErrorCode 错误代码为0，也就是没有错误   
ErrorInfo 错误信息为空，也就是没有错误   
Value 返回值为2，代表超出范围   
strXml 返回日志记录的xml信息，这里返回为空是因为访问的日志记录超出范围，日志记录不存在，所以没有   
lHintNext 返回的暗示参数，可用于下一次的接口调用，作用于"lHint"的参数，能够加快下一次接口的执行速度   
attachment_data 返回的附件数据，是一个byte数组   
lAttachmentTotalLength 返回附件的总长度    

响应信息如下：        
   
```
{
    "GetOperLogResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 2
    },
    "strXml": "",
    "lHintNext": -1,
    "attachment_data": null,
    "lAttachmentTotalLength": 0
}
```
## 实例演示3 完整获得(一个日志记录的)唯一附件内容

在我们获取一个日志记录的唯一附件内容时，可能会由于附件内容太大，所以不能获取完整，这个时候，我们就需要运用到分段获取，这个实例就向大家展示了如何分段获取。例如有一个附件的大小为500kb，那么它就有五十多万个byte数组（500x1024），服务器最多一次给我们返回十万个byte数组，那么我们就需要分段获取五到六次一个日志记录的唯一附件内容
   
### 请求参数1   

请求包要用到的参数解释：
      
strFileName 日志名称为20230327.log   
lIndex 日志名称为20230327.log的第五条操作日志   
lHint 无暗示参数可填-1但是不可为空   
strStyle 获取日志记录的风格，这边我们填"dont_return_Xml"在返回结果中就能够不返回“strXml”的内容   
lAttachmentFragmentStart 附件开始获取的字段位置，填0代表从第一个字段开始获取      
nAttachmentFragmentLength 返回附件的长度，填-1服务器会尽力返回这个附件尽可能多的内容     

请求包的数据部分如下：   

```
{
  "strFileName":"20230327.log",
  "lIndex":"5",
  "lHint":"-1",
  "strStyle":"dont_return_xml",
  "lAttachmentFragmentStart":"0",
  "nAttachmentFragmentLength":"-1"
}
```

### 响应结果1   

这里响应结果我们要注意它返回了多少byte数组，记录好byte数组的数量，下一次分段获取日志记录的唯一附件时，就在输入参数“lAttachmentFragmentStart”中，填写我们记录的byte数组数量   

响应结果的返回信息详解：   
  
ErrorCode 错误代码为0，也就是没有错误   
ErrorInfo 错误信息为空，也就是没有错误   
Value 返回值为2，代表超出范围   
strXml 返回日志记录的Xml信息  
lHintNext 返回的暗示参数，可用于下一次的接口调用，作用于"lHint"的参数，能够加快下一次接口的执行速度   
attachment_data 返回的附件数据，是一个byte数组   
lAttachmentTotalLength 返回附件的总长度  

响应信息如下：   

```
{
    "GetOperLogResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 1
    },
    "strXml": "",
    "lHintNext": 2080964,
    "attachment_data": [
        31,
        139,
        8,
        /*由于返回的byte数组太多，所以这里展示省略大部分的返回附件*/
        210,
        4,
        65,
        206
    ],
    "lAttachmentTotalLength": 512178
```
### 请求参数2 

这边"lAttachmentFragmentStart"填写了上一次调用接口时返回的byte数组数量

```
{
  "strFileName":"20230327.log",
  "lIndex":"5",
  "lHint":"-1",
  "strStyle":"dont_return_xml",
  "lAttachmentFragmentStart":"102400",
  "nAttachmentFragmentLength":"-1"
}
```

### 响应结果2 

可以看到返回的"attachment_data"附件数据，已经和上一次调用接口时返回的不一样了，这是接着返回上次没有全部返回的附件数据，但是依然没有返回完这个附件的全部内容，这里我们需要记录好这次byte数组返回的数量，并且加上上一次调用接口返回的数组数量，它们的结果将用于下一次调用接口时，填写于获取这条日志唯一附件的开始位置（lAttachmentFragmentStart）。只要这样每调用接口一次，下一次获取附件位置就增加一次（上一次使用的byte数组数量+这次获得的byte数组数量），这个大小为500kb（512178/1024），只需要分段获取五次，就能够全部获取完

```
{
    "GetOperLogResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 1
    },
    "strXml": "",
    "lHintNext": 2080964,
    "attachment_data": [
        197,
        81,
        196,
        /*由于返回的byte数组太多，所以这里展示省略大部分的返回附件*/
        31,
        140,
        11
    ],
    "lAttachmentTotalLength": 512178
```