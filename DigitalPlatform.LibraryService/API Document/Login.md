# Login() – 登录
```
        LibraryServerResult Login(
            string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights);
```
用途：登录。dp2Library中大部分的API都要在成功登录后才能使用。

## 参数
   
### string strUserName   
   
登录名
   
如果是工作人员身份登录，则此参数中为工作人员的用户名。
如果是读者身份登录，则此参数中一般是读者的证条码号。
读者登录时，strUserName参数值除了可以用读者证条码号，也可以采用形态为“前缀:值"的方式来指定读者身份。可用的前缀字符串如下表：   
   
前缀	说明   
NB	姓名   
如遇到重复时还可加上生日区分，姓名和生日之间间隔以字符'|'。姓名必须完整，生日为8字符形式   
EM	Email地址   
TP	电话号码   
ID	身份证号   
   
   
### string strPassword   
   
密码   
   
strPassword参数指定了用户的密码。

### string strParameters   
   
登录参数   
   
格式为 `location=???,index=???,type=???,simulate=???,client=???,publicError`

可用的子参数如下:

location    前端所在的工作台号。即前端所在的图书馆位置。比如"一楼出纳台"、"工具书阅览室"等等，可以是具体的馆藏地名称，也可以是代表若干个馆藏地的模糊名称。当后面要调用某些相关的 API 时，这里提供的工作台号能对操作提供线索信息。

index   在登录名发生重名情况下用于选择的下标值，输入的值应为数字，从零开始计数。此子参数可以缺省。   

libraryCode 用于限制读者身份登录时查找读者库范围的馆代码列表。可以为一个或多个馆代码，以竖线分隔。例如 "海淀分馆|西城分馆"。缺省表示不限制范围。当登录名可能在多个分馆的读者库之间出现重复的情况下，通过为这个参数值提供适当的馆代码可以避免登录出现报错。

type    登录者的身份类型，其值"reader"为读者，"worker"为工作人员，缺省为"worker"。   

simulate    是否为代理登录方式。值为"true"表示代理登录，"false"表示不进行代理登录，缺省为"false"。
所谓代理登录，是工作人员代表读者进行登录，请求时提供工作人员的用户名和密码，连带提供读者的登录名进行登录，登录成功后，通道为读者身份。详见后面的说明。

client  必填的参数，代表前端版本，我们一般这样填写“client=xxx|0.01"，其中 xxx 部分为前端模块的名字。此子参数的作用主要是用于 dp2library 服务器判断前端版本是否过低并做出相应的处理。

clientip    前端机器的 IP 地址。便于配合 dp2library 服务器进行多级连接时的白名单处理。

lang    当前通道希望使用的语言。例如 "zh-CN" 或 "en"

gettoken    

publicError 是否将登录报错文字按照 public 普通账户需求加以优化。缺省为不优化。此子参数没有值。
例: `type=worker,client=dp2capo|0.01,publicError`
app.PublicError 也会产生作用(TODO: library.xml 中如何配置?)。对一个新登录的通道，根据 app.PublicError 和本参数中是否包含 publicError，二者有一个就会优化报错信息。

TODO: gettoken 和 token 登录法。

### out string strOutputUserName       
     
返回账户的用户名   
   
参数返回了实际登录成功的的用户名。当工作人员身份登录时，此参数返回工作人员的用户名；当读者身份登录时，此参数返回读者的证条码号。   

### out string strRights   
   
参数返回了登录成功的用户的权限字符串。   

### 返回值


[LibraryServerResult.Value](https://jihulab.com/DigitalPlatform/dp2doc/-/issues/98)   	
   
表示请求是否成功，或者其它状态。为如下值:    	
	**-1**	错误   
	**0**	用户不存在，或者密码错误   
	**1**	登录成功   
	**>1**	有多个账户符合strUserName的条件。返回值数字正是符合条件的账户个数。这种情况下表明前端登录并未成功，需要进一步处理后重新登录 
      
### LibraryServerResult.ErrorInfo
出错信息。表示出错的具体情况，或者其它需要提醒请求者留意的文字信息。

权限：不需要特定的权限。   

## 说明
除了明确的工作人员身份和读者身份登录外，本API还提供了一种“模拟登录"方式。

模拟登录的时候，前端通过提供一个工作人员(管理员)的用户名和密码，来对一个读者账户进行登录，登录成功后的身份是读者。也就是说，这种登录方式下，登录者不必知道读者账户的密码就可以登录读者的账户，不过，登录时必须提供一个工作人员的用户名和密码。这是为了管理操作的目的而登录。

模拟登录时，strParameters中type子参数值应当为"reader"，并且simulate子参数值为"true"。这时，strPassword中应当设置形态为“管理员用户名,管理员密码"的字符串内容(而不是平时的单纯密码内容)。登录时，dp2Library会通过验证strPassword参数中所提供的管理员身份信息，来决定是否为strUserName所标志的读者执行登录。
dp2OPAC在一些情况下使用了模拟登录方式，登录到dp2Library服务器。

还有一种“访客登录"的形式。此时指定strUserName参数值为"public"；strParameters中的type子参数值为"worker"，不应是"reader"(因为"reader"情况下dp2Library会把strUserName参数的内容当作证条码号而不是用户ID)。这种方式下登录成功后，当前账户的身份是读者身份。
 
dp2Library在安装的时候，确有一个用户名为"public"的用户，它定义了访客登录的权限。
   
## 实例演示1 使用工作人员身份登录   
   
在Login接口，我们可选择的登录身份是多样的，本条实例展示了怎样以工作人员身份登录。   
    
### 请求参数

请求包要用到的参数解释：   
      
strUserName 这边使用了用户名登录   
strPassword 用户密码   
strParameters 登录参数。type填写登录者的身份，worker代表登录者为工作人员，同时也是缺省值；client表示填写的前端版本，如果不填写前端版本的话，返回结果就会报错
 

请求包的数据部分如下：
   
```
{
  "strUserName":"t",
  "strPassword":"1",
  "strParameters":"type=worker,client=practice|0.01"
}
```
   
### 响应结果

响应结果的返回信息详解：   
  
ErrorCode 错误代码为0，也就是没有错误   
ErrorInfo 错误信息为空，也就是没有错误   
Value 返回值为1，代表成功   
strOutputUserName 显示登录成功的账户名  
strRights 登录成功的账号所拥有的权限   
strLibraryCode 馆代码
   
响应信息如下：   
   
```
{
    "LoginResult": {
        "ErrorCode": 0,
        "ErrorInfo": null,
        "Value": 1
    },
    "strOutputUserName": "t",
    "strRights": "getbiblioinfo,setbiblioinfo,setbiblioobject,getbiblioobject,setiteminfo,order,setcommentinfo,librarian,checkclientversion,clientscanvirus",
    "strLibraryCode": ""
}
```
## 实例演示2 模拟登录   
   
模拟登录是通过使用管理员账号来登录读者账号，需要拥有权限simulatereader，否则就会模拟登录失败，下面实例就是怎样进行模拟登录的实例。   
   
    
### 请求参数

请求包要用到的参数解释：   
      
strUserName 输入了读者证条码号
strPassword 输入了管理员账户和密码，在一个双引号中用逗号分隔账号密码   
strParameters 登录参数。type填写登录者的身份，reader代表登录者为读者；simulate表示是否模拟登录，true表示是；client表示填写的前端版本，如果不填写前端版本的话，返回结果就会报错
 

请求包的数据部分如下：
   
```
{
  "strUserName":"P1",
  "strPassword":"supervisor,1",
  "strParameters":"type=reader,simulate=true,client=practice|0.01"
}
``` 
   
### 响应结果

响应结果的返回信息详解：   
  
ErrorCode 错误代码为0，也就是没有错误   
ErrorInfo 错误信息为空，也就是没有错误   
Value 返回值为1，代表成功   
strOutputUserName 显示登录成功的账户名  
strRights 登录成功的账号所拥有的权限   
strLibraryCode 馆代码
   
响应信息如下：   
   
```
{
    "LoginResult": {
        "ErrorCode": 0,
        "ErrorInfo": "",
        "Value": 1
    },
    "strOutputUserName": "P1",
    "strRights": "getreaderinfo,getbibliosummary,renew,reservation,search,getbiblioinfo,getbiblioobject,getiteminfo,getcommentinfo,getissueinfo,getorderinfo,setcommentinfo,setreaderinfo,changereaderpassword,getsystemparameter,listdbfroms,getobject,searchbiblio,searchcharging,searchcomment,searchissue,searchitem,searchorder,getcommentobject,setcommentobject,setreaderobject,getreaderobject,patron",
    "strLibraryCode": ""
}
```

## 说明

代理登录时如何使用 strUserName 和 strPassword 参数?