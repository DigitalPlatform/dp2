using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("dp2Circulation")]
[assembly: AssemblyDescription("dp2图书馆集成系统的内务前端")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyProduct("dp2Circulation -- dp2图书馆集成系统的内务前端")]
[assembly: AssemblyCopyright("Copyright © 2006-2015 DigitalPlatform 数字平台(北京)软件有限责任公司")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("59b4f729-e045-47f0-9afe-d02f30b94e44")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("2.11.*")]  // "2.4.*"
[assembly: AssemblyFileVersion("2.11.0.0")]

// V2.6 2015/11/7 MainForm BiblioSearchForm ChannelForm 采用 ChannelPool。注意观察有无通讯通道方面的故障
// V2.7 2015/11/30 EntityForm 大幅度改造，采用 ChannelPool。Stop 类的 BeginLoop() 不再允许嵌套，注意观察是否会抛出异常。固定面板区属性页的显示很多已经改造为 PropertyTaskList 实现
// V2.8 2015/12/4 绿色安装版本启动运行的时候，如果没有按住 Ctrl 键，则优先用 ClickOnce 安装方式启动运行。不过，如果当前电脑从来没有安装过 ClickOnce 版本，就缺 开始/所有程序/DigitalPlatform/dp2内务 V2 菜单项，此时依然会以绿色方式启动
//                  启动阶段，框架窗口背景色会体现当前运行的版本，如果是绿色安装版，会显示为深绿色
// V2.9 2015/12/11 调用 dp2library Login() API 的时候发送了 client 参数
// V2.10 2015/12/14 DigitalPlatform.CirculationClient.dll 中剥离部分纯粹通讯功能到 DigitalPlatform.LibraryClient.dll。有可能部分统计方案编译报错，需要修改后发布
//      2.10.5829.27554 2015/12/17 修改了 DigitalPlatform.Drawing 和 DigitalPlatform 的 AssemblyInfo.cs 文件
// V2.14 2016/1/4 读者查询窗和读者窗在导出读者详情 Excel 文件时，可以选择输出借阅历史了。“关于”窗口里面标识了开源的情况。
