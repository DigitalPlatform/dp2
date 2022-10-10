
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

1) 应使用 Visual Studio 2019 或 2022

2) 确保安装过 .NET Core SDK 2.6 以上版本

下载地址：
https://dotnet.microsoft.com/download/visual-studio-sdks

3) dp2 Solution 中引用了一个名为 dp-library 的submodule。需要用 Git 命令行执行下面命令：
（如何打开Git 命令行？可以先安装一个Git桌面工具，例如GitHub Desktop，然后通过菜单Repository/open in command prompt 打开命令行。 或者下载 [Git For Windows](https://github.com/waylau/git-for-win)，然后启动Git Shell）。

```
git submodule init
git submodule update
cd dp-library
git pull
```

注1：或者可用 `git submodule update --init --recursive`
注2：如果 git pull 命令报错说 `You are not currently on a branch.`，可以用 `git pull origin master`
以确保获得最新的 dp-library 代码。

第3步详细执行结果
```
D:\code\dp2>git submodule init

D:\code\dp2>git submodule update

D:\code\dp2>cd dp-library

D:\code\dp2\dp-library>git pull
You are not currently on a branch.
Please specify which branch you want to merge with.
See git-pull(1) for details.

    git pull <remote> <branch>


D:\code\dp2\dp-library>git pull origin master
fatal: unable to access 'https://github.com/DigitalPlatform/dp-library.git/': OpenSSL SSL_read: Connection was reset, errno 10054

D:\code\dp2\dp-library>git pull origin master
From https://github.com/DigitalPlatform/dp-library
 * branch            master     -> FETCH_HEAD
Updating 7fbbf14..e22781d
Fast-forward
 DigitalPlatform.Core/CompactLog.cs                 |  34 ++-
 DigitalPlatform.Core/ConfigSetting.cs              | 286 +++++++++++++++++-
 DigitalPlatform.Core/StringUtil.cs                 |  11 +
 DigitalPlatform.Core/deleted.txt                   | 333 +++++++++++++++++++++
 DigitalPlatform.SIP/BaseMessage.cs                 |   2 +-
 DigitalPlatform.SIP/Request/Checkin_09.cs          |   2 +-
 DigitalPlatform.SIP/Request/Checkout_11.cs         |   2 +-
 DigitalPlatform.SIP/Request/Login_93.cs            |   7 +-
 .../Request/PatronInformation_63.cs                |   2 +-
 DigitalPlatform.SIP/Request/Renew_29.cs            |   2 +-
 DigitalPlatform.SIP/SCHelper.cs                    |  12 +-
 DigitalPlatform.Z3950/BerNode.cs                   |  16 +-
 UnitTestCompactLog/UnitTest1.cs                    |   2 -
 UnitTestCompactLog/UnitTestCompactData.cs          |  31 ++
 UnitTestCompactLog/UnitTestCompactLog.csproj       |  16 +-
 15 files changed, 730 insertions(+), 28 deletions(-)
 create mode 100644 DigitalPlatform.Core/deleted.txt
 create mode 100644 UnitTestCompactLog/UnitTestCompactData.cs

D:\code\dp2\dp-library>
```

如果在dp2代码中修改的dp-library中的代码，那么同步dp-library的代码会报错，如下
```
D:\code\chord\dp-library>git pull origin master
remote: Enumerating objects: 17, done.
remote: Counting objects: 100% (17/17), done.
remote: Compressing objects: 100% (3/3), done.
remote: Total 11 (delta 8), reused 11 (delta 8), pack-reused 0
Unpacking objects: 100% (11/11), 1.08 KiB | 27.00 KiB/s, done.
From https://github.com/DigitalPlatform/dp-library
 * branch            master     -> FETCH_HEAD
   7c720d9..aa89e52  master     -> origin/master
error: Your local changes to the following files would be overwritten by merge:
        DigitalPlatform.SIP/Request/PatronInformation_63.cs
Please commit your changes or stash them before you merge.
Aborting
Updating 7c720d9..aa89e52
```
此时可以先用git reset --hard放弃本地代码，再拉取dp-library。如下：
```
D:\code\chord\dp-library>git reset --hard
HEAD is now at 7c720d9 修改提示

D:\code\chord\dp-library>git pull origin master
From https://github.com/DigitalPlatform/dp-library
 * branch            master     -> FETCH_HEAD
Updating 7c720d9..aa89e52
Fast-forward
 DigitalPlatform.SIP/Request/PatronInformation_63.cs          | 2 +-
 DigitalPlatform.SIP/Response/PatronInformationResponse_64.cs | 2 +-
 2 files changed, 2 insertions(+), 2 deletions(-)
```
 

Git 命令执行以后，需要重新打开 dp2 Solution 变动才能生效

4) 编译中若出现类似这样的报错：

```
13>C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets(3214,5): warning MSB3327: 无法在当前用户的 Windows 证书存储中找到代码签名证书。若要更正此问题，请禁用 ClickOnce 清单的签名或将证书安装到证书存储中。
13>C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Microsoft.Common.CurrentVersion.targets(3214,5): error MSB3323: 在证书存储区中找不到清单签名证书。
```

需要将一些 .exe 的 Project (例如 dp2Catalog)的“签名”属性页中“为 ClickOnce 清单签名”这个 checkbox 清除选择。


