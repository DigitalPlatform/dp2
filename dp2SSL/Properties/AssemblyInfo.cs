using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// 有关程序集的一般信息由以下
// 控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("dp2SSL")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("dp2SSL")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 会使此程序集中的类型
//对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型
//请将此类型的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

//若要开始生成可本地化的应用程序，请设置
//.csproj 文件中的 <UICulture>CultureYouAreCodingWith</UICulture>
//例如，如果您在源文件中使用的是美国英语，
//使用的是美国英语，请将 <UICulture> 设置为 en-US。  然后取消
//对以下 NeutralResourceLanguage 特性的注释。  更新
//以下行中的“en-US”以匹配项目文件中的 UICulture 设置。

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //主题特定资源词典所处位置
                                     //(未在页面中找到资源时使用，
                                     //或应用程序资源字典中找到时使用)
    ResourceDictionaryLocation.SourceAssembly //常规资源词典所处位置
                                              //(未在页面中找到资源时使用，
                                              //、应用程序或任何主题专用资源字典中找到时使用)
)]


// 程序集的版本信息由下列四个值组成: 
//
//      主版本
//      次版本
//      生成号
//      修订号
//
// 可以指定所有值，也可以使用以下所示的 "*" 预置版本号和修订号
// 方法是按如下所示使用“*”: :
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.5.22")]    // 1.5.*
[assembly: AssemblyFileVersion("1.5.0.0")]  // 1.5.0.0

// 1.0 2019/2/21 第一个版本
// 1.1 2019/2/26 可以显示版本号了
// 1.2 2019/6/14 代码重构以后的版本，并具备人脸识别功能
// 1.3 2019/11/27 这个版本的 dp2ssl 要和 RfidCenter 1.4 版配套使用才行。基本实现了智能书柜功能
// 1.4 2020/4/2 重构算法。书柜 SaveActions() 阶段即刷新门控件上的数字，和与 dp2library 同步无关。具有本地数据库保存操作
// 1.5 2020/9/8 新增加 greensetup.exe 绿色安装，并测试完成
//      1.5.18 (2020/10/14) 消除“清掉前一个同天线号的门的图书数字 bug”
//      1.5.19 (2020/11/5) 增加 “盘点”功能
//      1.5.20 (2020/11/24) 智能书柜：修正密集开关门时的 bug；解决开门后立刻关门情形的状态可靠问题。自助借还：增加自动返回主菜单的配置参数
//      1.5.21 (2020/11/25) 智能书柜的 NewTagList 算法重构，彻底分离图书和读者证读卡器的线程和标签内存列表，以获得任何时候刷读者卡的敏捷性。增加了处理过程中防范返回主菜单的机制
