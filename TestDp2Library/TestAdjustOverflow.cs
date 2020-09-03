using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;
using static DigitalPlatform.LibraryServer.LibraryApplication;

namespace TestDp2Library
{
    /// <summary>
    /// 测试 dp2library 中调整 Overflow 的功能
    /// </summary>
    [TestClass]
    public class TestTestAdjustOverflow
    {
        static string _xml = @"    <rightsTable>
        <type reader='本科生'>
            <param name='可借总册数' value='10' />
            <param name='可预约册数' value='5' />
            <param name='以停代金因子' value='1.0' />
            <param name='工作日历名' value='基本日历' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教材'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教学参考'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='原版西文'>
                <param name='可借册数' value='2' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='3.0' />
            </type>
        </type>
        <type reader='硕士生'>
            <param name='可借总册数' value='15' />
            <param name='可预约册数' value='5' />
            <param name='以停代金因子' value='1.0' />
            <param name='工作日历名' value='基本日历' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教材'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教学参考'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='原版西文'>
                <param name='可借册数' value='3' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='3.0' />
            </type>
        </type>
        <type reader='博士生'>
            <param name='可借总册数' value='20' />
            <param name='可预约册数' value='5' />
            <param name='以停代金因子' value='1.0' />
            <param name='工作日历名' value='基本日历' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教材'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教学参考'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='原版西文'>
                <param name='可借册数' value='4' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='3.0' />
            </type>
        </type>
        <type reader='讲师'>
            <param name='可借总册数' value='20' />
            <param name='可预约册数' value='5' />
            <param name='以停代金因子' value='1.0' />
            <param name='工作日历名' value='基本日历' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教材'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教学参考'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='原版西文'>
                <param name='可借册数' value='5' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='3.0' />
            </type>
        </type>
        <type reader='教授'>
            <param name='可借总册数' value='1' />
            <param name='可预约册数' value='5' />
            <param name='以停代金因子' value='1.0' />
            <param name='工作日历名' value='基本日历' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教材'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='教学参考'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='1.5' />
            </type>
            <type book='原版西文'>
                <param name='可借册数' value='6' />
                <param name='借期' value='31day,15day' />
                <param name='超期违约金因子' value='CNY1.0/day' />
                <param name='丢失违约金因子' value='3.0' />
            </type>
        </type>
        <readerTypes>
            <item>本科生</item>
            <item>硕士生</item>
            <item>博士生</item>
            <item>讲师</item>
            <item>教授</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
            <item>教材</item>
            <item>教学参考</item>
            <item>原版西文</item>
        </bookTypes>
        <library code='海淀分馆'>
            <type reader='本科生'>
                <param name='可借总册数' value='10' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='2' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='硕士生'>
                <param name='可借总册数' value='15' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='3' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='博士生'>
                <param name='可借总册数' value='20' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='4' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='讲师'>
                <param name='可借总册数' value='20' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='5' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='教授'>
                <param name='可借总册数' value='30' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='6' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <readerTypes>
                <item>本科生</item>
                <item>硕士生</item>
                <item>博士生</item>
                <item>讲师</item>
                <item>教授</item>
            </readerTypes>
            <bookTypes>
                <item>普通</item>
                <item>教材</item>
                <item>教学参考</item>
                <item>原版西文</item>
            </bookTypes>
        </library>
        <library code='西城分馆'>
            <type reader='本科生'>
                <param name='可借总册数' value='10' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='2' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='硕士生'>
                <param name='可借总册数' value='15' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='3' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='博士生'>
                <param name='可借总册数' value='20' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='4' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='讲师'>
                <param name='可借总册数' value='20' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='5' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <type reader='教授'>
                <param name='可借总册数' value='30' />
                <param name='可预约册数' value='5' />
                <param name='以停代金因子' value='1.0' />
                <param name='工作日历名' value='基本日历' />
                <type book='普通'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教材'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='教学参考'>
                    <param name='可借册数' value='10' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='1.5' />
                </type>
                <type book='原版西文'>
                    <param name='可借册数' value='6' />
                    <param name='借期' value='31day,15day' />
                    <param name='超期违约金因子' value='CNY1.0/day' />
                    <param name='丢失违约金因子' value='3.0' />
                </type>
            </type>
            <readerTypes>
                <item>本科生</item>
                <item>硕士生</item>
                <item>博士生</item>
                <item>讲师</item>
                <item>教授</item>
            </readerTypes>
            <bookTypes>
                <item>普通</item>
                <item>教材</item>
                <item>教学参考</item>
                <item>原版西文</item>
            </bookTypes>
        </library>
    </rightsTable>";

        // 没有任何 overflow 册
        [TestMethod]
        public void Test_adjust_1()
        {
            string rights_xml = @"<rightsTable>
       <library code='新分馆'>
        <type reader='本科生'>
            <param name='可借总册数' value='10' />
            <param name='可预约册数' value='10' />
            <param name='以停代金因子' value='1' />
            <param name='工作日历名' value='' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day' />
                <param name='超期违约金因子' value='CNY1.00/day' />
                <param name='丢失违约金因子' value='1' />
            </type>
        </type>
        <readerTypes>
            <item>本科生</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
        </bookTypes>
    </library>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>新分馆</libraryCode>
  <borrows>
    <borrow barcode='T0000050' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";


            /*
            int nRet = CallAdjustOverflow1(rights_xml,
                patron_xml,
                out List<ItemModifyInfo> modifies);

            Assert.AreEqual(0, nRet);
            Assert.AreEqual(0, modifies.Count);
            */
            var result = CallAdjustOverflow1(rights_xml,
                patron_xml);
            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(0, result.Modifies.Count);

        }

        // 有一个 overflow 册。调整后不受数量限制
        [TestMethod]
        public void Test_adjust_2()
        {
            string rights_xml = @"<rightsTable>
       <library code='新分馆'>
        <type reader='本科生'>
            <param name='可借总册数' value='10' />
            <param name='可预约册数' value='10' />
            <param name='以停代金因子' value='1' />
            <param name='工作日历名' value='' />
            <type book='普通'>
                <param name='可借册数' value='10' />
                <param name='借期' value='31day' />
                <param name='超期违约金因子' value='CNY1.00/day' />
                <param name='丢失违约金因子' value='1' />
            </type>
        </type>
        <readerTypes>
            <item>本科生</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
        </bookTypes>
    </library>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>新分馆</libraryCode>
  <borrows>
    <borrow barcode='T0000050' overflow='test' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";

            /*
            int nRet = CallAdjustOverflow1(rights_xml,
                patron_xml,
                out List<ItemModifyInfo> modifies);

            Assert.AreEqual(0, nRet);
            Assert.AreEqual(1, modifies.Count);
            Assert.AreEqual("T0000050", modifies[0].ItemBarcode);
            Assert.AreEqual("31day", modifies[0].BorrowPeriod);
            */

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(1, result.Modifies.Count);
            Assert.AreEqual("T0000050", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
        }

        // 有一个 overflow 册。调整后会碰到类型数量限制，因此无法调整成功
        [TestMethod]
        public void Test_adjust_3()
        {
            string rights_xml = @"<rightsTable>
       <library code='新分馆'>
        <type reader='本科生'>
            <param name='可借总册数' value='10' />
            <param name='可预约册数' value='10' />
            <param name='以停代金因子' value='1' />
            <param name='工作日历名' value='' />
            <type book='普通'>
                <param name='可借册数' value='2' />
                <param name='借期' value='31day' />
                <param name='超期违约金因子' value='CNY1.00/day' />
                <param name='丢失违约金因子' value='1' />
            </type>
        </type>
        <readerTypes>
            <item>本科生</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
        </bookTypes>
    </library>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>新分馆</libraryCode>
  <borrows>
    <borrow barcode='T0000050' overflow='test' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000132' recPath='中文图书实体/1350' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='xxx' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(0, result.Modifies.Count);
        }

        // 有一个 overflow 册。调整后会碰到总数数量限制，因此无法调整成功
        [TestMethod]
        public void Test_adjust_4()
        {
            string rights_xml = @"<rightsTable>
       <library code='新分馆'>
        <type reader='本科生'>
            <param name='可借总册数' value='2' />
            <param name='可预约册数' value='10' />
            <param name='以停代金因子' value='1' />
            <param name='工作日历名' value='' />
            <type book='普通'>
                <param name='可借册数' value='3' />
                <param name='借期' value='31day' />
                <param name='超期违约金因子' value='CNY1.00/day' />
                <param name='丢失违约金因子' value='1' />
            </type>
        </type>
        <readerTypes>
            <item>本科生</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
        </bookTypes>
    </library>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>新分馆</libraryCode>
  <borrows>
    <borrow barcode='T0000050' overflow='test' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000132' recPath='中文图书实体/1350' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='xxx' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(0, result.Modifies.Count);
        }

        // https://github.com/DigitalPlatform/dp2/issues/731#issuecomment-686243283
        // 测试用例1-借5本，超额2本，还非超额的册
        [TestMethod]
        public void Test_adjust_5()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"<?xml version='1.0' encoding='utf-8'?>
    <root>
        <barcode>001</barcode>
        <name>小明</name>
        <readerType>本科生</readerType>
        <borrows>
            <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='f3387cc4-4402-4e7f-91bb-fc5965c86473' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
            <borrow barcode='B005' recPath='中文图书实体/5' biblioRecPath='中文图书/5' location='流通库' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='31day' borrowID='8712719b-f1a8-4127-85d6-76fb454263ce' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
            <borrow barcode='B003' recPath='中文图书实体/3' biblioRecPath='中文图书/3' location='流通库' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='31day' borrowID='4ba34033-9422-499d-90d4-885ce759fa9e' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
            
            <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='4b7f81c2-9a0c-4ee6-88f6-82373b7f012f' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
        </borrows>        
    </root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(1, result.Modifies.Count);
            Assert.AreEqual("B002", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
        }

        // case1.2 借5本，超2本，还超额其中的1本，消掉该本的超额，还剩另一本超额
        [TestMethod]
        public void Test_adjust_6()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>001</barcode>
  <name>小明</name>
  <readerType>本科生</readerType>
  <refID>ffd2b2c2-4a86-464e-b5d4-de61d766953b</refID>
  <libraryCode>
  </libraryCode>
  <borrows>
    <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='31day' borrowID='07ad70da-81f7-4721-a308-ff47ab497fc1' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
    <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='31day' borrowID='7c7476bc-f69f-49c0-bcb7-93167e4b9bef' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
    <borrow barcode='B003' recPath='中文图书实体/3' biblioRecPath='中文图书/3' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='1day' borrowID='ff338003-62f1-47f1-bd4e-47ed8c862768' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
  </borrows>
</root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(1, result.Modifies.Count);
            Assert.AreEqual("B003", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
        }

        // case1.1 借4本，超1本，还其它非超额册，应消掉超额册
        [TestMethod]
        public void Test_adjust_7()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>001</barcode>
  <name>小明</name>
  <readerType>本科生</readerType>
  <refID>ffd2b2c2-4a86-464e-b5d4-de61d766953b</refID>
  <libraryCode>
  </libraryCode>
  <borrows>
    <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='31day' borrowID='07ad70da-81f7-4721-a308-ff47ab497fc1' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
    <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='31day' borrowID='7c7476bc-f69f-49c0-bcb7-93167e4b9bef' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
    <borrow barcode='B003' recPath='中文图书实体/3' biblioRecPath='中文图书/3' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 13:08:12 +0800' borrowPeriod='1day' borrowID='ff338003-62f1-47f1-bd4e-47ed8c862768' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
  </borrows>
</root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(1, result.Modifies.Count);
            Assert.AreEqual("B003", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
        }

        // case1.2 借5本，超2本，还超额其中的1本，消掉该本的超额，还剩另一本超额
        [TestMethod]
        public void Test_adjust_8()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"<root>
  <barcode>001</barcode>
  <name>小明</name>
  <readerType>本科生</readerType>
  <borrows>
    <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' borrowDate='Thu, 03 Sep 2020 13:29:29 +0800' borrowPeriod='31day' borrowID='dfd02e63-3ca2-4ff9-b3e9-a0951b7e7f9d' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
    <borrow barcode='B005' recPath='中文图书实体/5' biblioRecPath='中文图书/5' location='流通库' borrowDate='Thu, 03 Sep 2020 13:29:29 +0800' borrowPeriod='31day' borrowID='dccb5296-2862-48f3-b662-e37eedee324c' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />

    <borrow barcode='B001' recPath='中文图书实体/1' biblioRecPath='中文图书/1' location='流通库' borrowDate='Thu, 03 Sep 2020 13:29:29 +0800' borrowPeriod='31day' borrowID='30f5f4c8-1459-4f79-91b6-d4b8a29b5044' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 13:29:29 +0800' borrowPeriod='1day' borrowID='0b747c0a-907d-42bf-b446-0dd735690c00' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
  </borrows>
</root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(0, result.Modifies.Count);
            /*
            Assert.AreEqual("B003", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
            */
        }

        // case1.3 借5本，超2本，还其它非超额的1册，应从2本超额中消掉1本。
        [TestMethod]
        public void Test_adjust_9()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"    <root >
        <barcode>001</barcode>
        <name>小明</name>
        <readerType>本科生</readerType>
        <borrows>
            <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='f3387cc4-4402-4e7f-91bb-fc5965c86473' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
            <borrow barcode='B005' recPath='中文图书实体/5' biblioRecPath='中文图书/5' location='流通库' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='31day' borrowID='8712719b-f1a8-4127-85d6-76fb454263ce' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
            <borrow barcode='B003' recPath='中文图书实体/3' biblioRecPath='中文图书/3' location='流通库' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='31day' borrowID='4ba34033-9422-499d-90d4-885ce759fa9e' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />

            <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='4b7f81c2-9a0c-4ee6-88f6-82373b7f012f' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
        </borrows>        
    </root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(1, result.Modifies.Count);
            Assert.AreEqual("B002", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
        }

        // case1.4 借5本，超2本，还其它非超额的2册，应把超额的2册消掉超额
        [TestMethod]
        public void Test_adjust_10()
        {
            string rights_xml = @"<rightsTable>
    <type reader='本科生'>
        <param name='可借总册数' value='3' />
        <param name='可预约册数' value='5' />
        <param name='以停代金因子' value='1.0' />
        <param name='工作日历名' value='基本日历' />
        <type book='普通'>
            <param name='可借册数' value='5' />
            <param name='借期' value='31day,15day' />
            <param name='超期违约金因子' value='CNY1.0/day' />
            <param name='丢失违约金因子' value='1.5' />
        </type>
    </type>
    <readerTypes>
        <item>本科生</item>
    </readerTypes>
    <bookTypes>
        <item>普通</item>
    </bookTypes>
</rightsTable>";

            string patron_xml = @"    <root >
        <barcode>001</barcode>
        <name>小明</name>
        <readerType>本科生</readerType>
        <borrows>
            <borrow barcode='B002' recPath='中文图书实体/2' biblioRecPath='中文图书/2' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='f3387cc4-4402-4e7f-91bb-fc5965c86473' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY10' />
            <borrow barcode='B005' recPath='中文图书实体/5' biblioRecPath='中文图书/5' location='流通库' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='31day' borrowID='8712719b-f1a8-4127-85d6-76fb454263ce' returningDate='Sun, 04 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
            <borrow barcode='B004' recPath='中文图书实体/4' biblioRecPath='中文图书/4' location='流通库' overflow='读者 001 所借图书数量将超过 馆代码 [] 中 该读者类型 本科生 对所有图书类型的最多 可借册数 值 3' borrowDate='Thu, 03 Sep 2020 11:06:36 +0800' borrowPeriod='1day' borrowID='4b7f81c2-9a0c-4ee6-88f6-82373b7f012f' returningDate='Fri, 04 Sep 2020 12:00:00 +0800' operator='supervisor' type='普通' price='CNY16.80' />
        </borrows>        
    </root>";

            var result = CallAdjustOverflow1(rights_xml,
    patron_xml);

            Assert.AreEqual(0, result.Value);
            Assert.AreEqual(2, result.Modifies.Count);
            Assert.AreEqual("B002", result.Modifies[0].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[0].BorrowPeriod);
            Assert.AreEqual("B004", result.Modifies[1].ItemBarcode);
            Assert.AreEqual("31day", result.Modifies[1].BorrowPeriod);
        }


        // 调用私有函数 AdjustOverflow()
        // https://stackoverflow.com/questions/9122708/unit-testing-private-methods-in-c-sharp
        // 示范如何测试私有函数。返回类型也是私有的
        static int CallAdjustOverflow(string rightsTableXml,
            string patron_xml,
            out List<ItemModifyInfo> modifies)
        {
            var app = PrepareApp(rightsTableXml);

            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(patron_xml);

            PrivateObject obj = new PrivateObject(app);

            object result = obj.Invoke("AdjustOverflow",
            readerdom,
            null);

            PrivateObject obj1 = new PrivateObject(result);
            int nRet = (int)obj1.GetFieldOrProperty("Value");
            modifies = (List<ItemModifyInfo>)obj1.GetFieldOrProperty("Modifies");

            return nRet;
        }

        // 调用私有函数 AdjustOverflow()
        // https://stackoverflow.com/questions/9122708/unit-testing-private-methods-in-c-sharp
        // 示范如何使用 [assembly: InternalsVisibleTo("TestDp2Library")] 访问私有方法和类型
        static AdjustOverflowResult CallAdjustOverflow1(string rightsTableXml,
    string patron_xml)
        {
            var app = PrepareApp(rightsTableXml);

            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(patron_xml);

            return app.AdjustOverflow(readerdom,
            null);
        }

        // 准备 LibraryApplication 对象
        static LibraryApplication PrepareApp(string rightsTableXml)
        {
            LibraryApplication app = new LibraryApplication();

            app.LibraryCfgDom = new XmlDocument();
            app.LibraryCfgDom.LoadXml("<root ><rightsTable /></root>");
            XmlElement tableRoot = app.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable") as XmlElement;
            DomUtil.SetElementOuterXml(tableRoot, rightsTableXml);
            return app;
        }
    }
}
