
# dp2 图书馆集成系统

An Integrated Library System write in .NET

dp2 图书馆集成系统是 数字平台(北京)软件有限责任公司于 2006 年开始开发的一套图书馆业务系统软件。现在我们决定将它开源，奉献给全社会。

这套系统包含下列模块：

* dp2Kernel - 数据库内核(标准版)
* dp2Library - 图书馆应用服务器(标准版)
* dp2LibraryXE - 图书馆应用服务器(单机版和小型版)
* dp2ZServer - Z39.50 服务器
* dp2OPAC - 读者公共查询
* dp2Circulation - 内务前端
* dp2Catalog - 编目前端
* dp2Batch - 内核批处理前端
* dp2Manager - 内核管理
* dp2rms - 内核资源管理
* [GcatLite](https://github.com/DigitalPlatform/dp2/blob/master/GcatLite) - 通用汉语著者号码表取号示范程序

我们将继续维护和销售原有的企业版，并增加社区版的发行包下载服务。企业版和社区版采用同一套源代码，社区版的功能更新一般会比企业版要稍晚一些。

欢迎有识之士加入我们的开发和推广团队，以便更多更好地为这套系统增添功能。email: xietao@dp2003.com

---

QQ群: 

开发事宜：开源dp2系统开发 163251536

产品使用咨询：数字平台产品 487513826

---

[数字平台(北京)软件有限责任公司](http://dp2003.com)
谢涛
2015.5.7

## 如何编译

1) 应使用 Visual Studio 2017 或 2019

2) 确保安装过 .NET Core SDK 2.6 以上版本

下载地址：
https://dotnet.microsoft.com/download/visual-studio-sdks

3) dp2 Solution 中引用了一个名为 dp-library 的submodule。需要用 Git 命令行执行（先安装一个Git桌面工具，例如GitHub Desktop）：

```
git submodule init
git submodule update
```

以确保获得最新的 dp-library 代码。

Git 命令执行以后，需要重新打开 dp2 Solution 变动才能生效

4) 编译中若出现类似这样的报错：

```
13>C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets(3214,5): warning MSB3327: 无法在当前用户的 Windows 证书存储中找到代码签名证书。若要更正此问题，请禁用 ClickOnce 清单的签名或将证书安装到证书存储中。
13>C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets(3214,5): error MSB3323: 在证书存储区中找不到清单签名证书。
```

需要将一些 .exe 的 Project (例如 dp2Catalog)的“签名”属性页中“为 ClickOnce 清单签名”这个 checkbox 清除选择。
