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
using System.Diagnostics;

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
            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>
  </libraryCode>
  <borrows>
    <borrow barcode='T0000050' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";

            var app = PrepareApp(_xml);

            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(patron_xml);

            int nRet = app.AdjustOverflow(
            readerdom,
            null,
            out List<ItemModifyInfo> modifies,
            out string strError);

            Assert.AreEqual(0, nRet);
            Assert.AreEqual(0, modifies.Count);
        }

        // 有一个 overflow 册
        [TestMethod]
        public void Test_adjust_2()
        {
            string patron_xml = @"<root>
  <barcode>R0000001</barcode>
  <name>张三</name>
  <refID>be13ecc5-6a9c-4400-9453-a072c50cede1</refID>
  <libraryCode>
  </libraryCode>
  <borrows>
    <borrow barcode='T0000050' overflow='test' recPath='中文图书实体/1268' biblioRecPath='中文图书/614' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='a8d5815e-d8f1-40c9-96be-dd0c83f8e3d1' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
    <borrow barcode='T0000131' recPath='中文图书实体/1349' biblioRecPath='中文图书/623' location='智能书柜' borrowDate='Wed, 02 Sep 2020 00:08:25 +0800' borrowPeriod='31day' borrowID='2eb9902e-3463-4049-a7c5-f65c04c52a90' returningDate='Sat, 03 Oct 2020 12:00:00 +0800' operator='supervisor' type='普通' price='' />
  </borrows>
  <readerType>本科生</readerType>
</root>";

            var app = PrepareApp(_xml);

            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(patron_xml);

            int nRet = app.AdjustOverflow(
            readerdom,
            null,
            out List<ItemModifyInfo> modifies,
            out string strError);

            Assert.AreEqual(0, nRet);
            Assert.AreEqual(1, modifies.Count);
            Assert.AreEqual("T0000050", modifies[0].ItemBarcode);
            Assert.AreEqual("31day", modifies[0].BorrowPeriod);
        }


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
