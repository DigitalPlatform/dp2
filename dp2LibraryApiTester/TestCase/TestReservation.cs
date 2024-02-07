using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester.TestCase
{
    // 测试预约相关功能
    public static class TestReservation
    {
        public static NormalResult TestAll(string condition)
        {
            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            result = TestCreateReaderRecord("test_normal", condition);
            if (result.Value == -1) return result;

            result = CreateBiblioRecords("test_normal", condition);
            if (result.Value == -1) return result;


            result = TestReservation_1("test_normal", "");
            if (result.Value == -1) return result;

            /*
            result = Finish();
            if (result.Value == -1) return result;
            */
            return new NormalResult();
        }


        static string _globalPatronDbName = "_总馆读者";
        static string _globalLibraryCode = "";

        static string _itemDbName = "_测试图书实体";
        static string _biblioDbName = "_测试图书";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _biblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在创建测试用书目库 ...");
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                int nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    null,
                    _biblioDbName,
                    "book",
                    "unimarc",
                    "*",
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // *** 创建读者库

                // 如果测试用的读者库以前就存在，要先删除。
                DataModel.SetMessage("正在删除测试用读者库 ...");
                strOutputInfo = "";
                lRet = channel.ManageDatabase(
    null,
    "delete",
    _globalPatronDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                {
                    DataModel.SetMessage($"正在创建测试用读者库 '{_globalPatronDbName}'...");
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateReaderDatabase(
                        channel,
                        null,
                        _globalPatronDbName,
                        _globalLibraryCode,
                        true,
                        "",
                        out strError);
                    if (nRet != 1)
                        goto ERROR1;
                }

                // *** 创建总馆用户若干
                {
                    var user_names = new List<string>() { "test_normal" };
                    DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                    nRet = Utility.DeleteUsers(channel,
                        user_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 创建几个测试用工作人员账户

                    DataModel.SetMessage("正在创建用户 test_normal ...");
                    lRet = channel.SetUser(null,
                        "new",
                        new UserInfo
                        {
                            UserName = "test_normal",
                            Rights = "setsystemparameter,getbiblioinfo,setbiblioinfo,getreaderinfo,setreaderinfo,getiteminfo,setiteminfo,borrow,return,reservation",
                        },
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                }

                DataModel.SetMessage("正确", "green");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "PrepareEnvironment() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static int _recordCount = 10;

        // "册记录缺 borrower"
        // "册记录 borrower 错位"
        // "册记录不存在"
        public static NormalResult CreateBiblioRecords(
            string userName,
            string condition)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在创建书目记录和册记录 ...");

                for (int i = 0; i < _recordCount; i++)
                {
                    // *** 创建书目记录
                    string path = _biblioDbName + "/?";
                    var record = BuildBiblioRecord($"测试题名 {i + 1}", "");

                    string strXml = "";
                    int nRet = MarcUtil.Marc2XmlEx(
        record.Text,
        "unimarc",
        ref strXml,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    long lRet = channel.SetBiblioInfo(null,
                        "new",
                        path,
                        "xml", // strBiblioType
                        strXml,
                        null,
                        null,
                        "", // style
                        out string output_biblio_recpath,
                        out byte[] new_timestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    DataModel.SetMessage($"{output_biblio_recpath}");

                    if (condition != "册记录不存在")
                    {
                        // 创建册记录
                        EntityInfo[] errorinfos = null;

#if REMOVED
                        string borrower = "G" + (i + 1).ToString().PadLeft(7, '0'); // G0000001
                        string borrow_date = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now);

                        if (condition == "册记录缺 borrower")
                            borrower = "";
                        else if (condition == "册记录 borrower 错位")
                            borrower = "G" + (i + 2).ToString().PadLeft(7, '0'); // G0000001
#endif

                        var entities = BuildEntityRecords(
                            "BOOK" + (i + 1).ToString().PadLeft(7, '0'),
                            $"参考ID{(i+1)}");
                        lRet = channel.SetEntities(null,
                            output_biblio_recpath,
                            entities,
                            out errorinfos,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        strError = GetError(errorinfos);
                        if (string.IsNullOrEmpty(strError) == false)
                            goto ERROR1;

                        string itemRecPath = errorinfos[0].NewRecPath;
                    }
                }

                DataModel.SetMessage("书目记录和册记录创建完成", "green");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "CreateBiblioRecords() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"CreateBiblioRecords() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        static string GetError(EntityInfo[] errorinfos)
        {
            if (errorinfos != null)
            {
                List<string> errors = new List<string>();
                foreach (var error in errorinfos)
                {
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        errors.Add(error.ErrorInfo);
                    }
                }

                if (errors.Count > 0)
                    return StringUtil.MakePathList(errors, "; ");
            }

            return null;
        }

        static EntityInfo[] BuildEntityRecords(string barcode,
            string refID)
        {
            // X0000001
            string item_template = @"<root>
<barcode>{barcode}</barcode>
<location>阅览室</location>
<bookType>普通</bookType>
<price>CNY12.00</price>
<refID>{refID}</refID>
</root>";
            string xml = "";
            xml = item_template.Replace("{barcode}", barcode)
                .Replace("{refID}", refID);

            xml = DomUtil.RemoveEmptyElements(xml);

            List<EntityInfo> results = new List<EntityInfo>();
            EntityInfo entity = new EntityInfo
            {
                Action = "new",
                // Style = "force",
                NewRecord = xml,
            };
            results.Add(entity);
            return results.ToArray();
        }

        // parameters:
        //      strStyle    create_objects  表示要创建对象文件
        static MarcRecord BuildBiblioRecord(
            string strTitle,
            string strStyle)
        {
            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "200  $a" + strTitle));
            record.add(new MarcField('$', "690  $aI247.5"));
            record.add(new MarcField('$', "701  $a测试著者"));
            return record;
        }

        // 创建读者记录
        // "读者记录缺 borrow"
        // "读者记录不存在"
        public static NormalResult TestCreateReaderRecord(
            string userName,
            string condition)
        {
            string strError = "";

            if (condition == "读者记录不存在")
                return new NormalResult();

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"正在以用户 {userName} 身份创建读者记录 ...");
                long lRet = 0;

                // 不校验条码号
                lRet = channel.SetSystemParameter(null,
                    "circulation",
                    "?VerifyBarcode",
                    "false",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

#if REMOVED
                // 设置条码号校验规则
                int ret = Utility.SetBarcodeValidation(
                    @"<validator location=',流通库,测试库,智能书柜,阅览室,保存本库'>
        <patron>
            <CMIS />
            <range value='G0000001-G9999999' />
        </patron>
    </validator>
    <validator location='海淀分馆*' >
        <patron>
            <CMIS />
            <range value='H0000001-H9999999' />
        </patron>
    </validator>
    <validator location='西城分馆*' >
        <patron>
            <CMIS />
            <range value='X0000001-X9999999' />
        </patron>
    </validator>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;
#endif

                // 设置借还权限。这样读者类型和图书类型就都定下来了
                int ret = Utility.SetRightsTable(
                    @"    <type reader='本科生'>
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
    </bookTypes>",
                    out strError);
                if (ret == -1)
                    goto ERROR1;

                string path = _globalPatronDbName + "/?";

                string xml_template = @"<root>
<barcode>{barcode}</barcode>
<name>{name}</name>
<readerType>本科生</readerType>
<department>数学系</department>
<refID>{refID}</refID>
</root>";

                for (int i = 0; i < _recordCount; i++)
                {
                    string barcode = "PATRON" + (i + 1).ToString().PadLeft(7, '0'); // G0000001
                    string name = $"读者" + (i + 1).ToString();
#if REMOVED
                    string item_barcode = "TEST" + (i + 1).ToString().PadLeft(7, '0');
                    string item_recpath = _itemDbName + "/" + (i + 1).ToString();
                    string biblio_recpath = _biblioDbName + "/" + (i + 1).ToString();
                    string borrow_date = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now);
                    string returning_date = DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now + TimeSpan.FromDays(31));
#endif
                    string refID = $"读者参考ID{(i+1)}";

                    string xml = xml_template.Replace("{barcode}", barcode)
                        .Replace("{name}", name)
                        .Replace("{refID}", refID);

#if REMOVED
                    if (condition == "读者记录缺 borrow")
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(xml);
                        var nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                        foreach (XmlElement borrow in nodes)
                        {
                            borrow.ParentNode.RemoveChild(borrow);
                        }

                        xml = dom.DocumentElement.OuterXml;
                    }
#endif

                    lRet = channel.SetReaderInfo(null,
                        "new", // "forcenew",
                        path,
                        xml,
                        null,
                        null,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue kernel_errorcode,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                    DataModel.SetMessage($"正在创建读者记录 {saved_recpath}");
                }


                /*
                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                */
                DataModel.SetMessage("读者记录全部创建完成", "green");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult TestReservation_1(
            string userName,
            string condition)
        {
            string strError = "";

            LibraryChannel channel = DataModel.NewChannel(userName, "");
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage($"P1 借书两册 ...");
                long lRet = 0;

                {
                    lRet = channel.Borrow(null,
                        false,
                        "PATRON0000001",
                        "BOOK0000001",
                        "",
                        false,
                        null,
                        "",
                        "xml",
                        out string[] item_records,
                        "xml",
                        out string[] reader_records,
                        "xml",
                        out string[] biblio_records,
                        out string[] dup_path,
                        out string output_reader_barcode,
                        out BorrowInfo borrow_info,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                {
                    lRet = channel.Borrow(null,
        false,
        "PATRON0000001",
        "BOOK0000002",
        "",
        false,
        null,
        "",
        "xml",
        out string[] item_records,
        "xml",
        out string[] reader_records,
        "xml",
        out string[] biblio_records,
        out string[] dup_path,
        out string output_reader_barcode,
        out BorrowInfo borrow_info,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                DataModel.SetMessage($"P2 预约 B2 ...");
                lRet = channel.Reservation(null,
                    "new",
                    "PATRON0000002",
                    "BOOK0000001,BOOK0000002",
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                DataModel.SetMessage($"P1 还书 B2(两册的预约请求同时被满足) ...");

                {
                    lRet = channel.Return(null,
                        "return",
                        "PATRON0000001",
                        "BOOK0000002",
                        "",
                        false,
                        "",
                        "xml",
                        out string[] item_records,
                        "xml",
                        out string[] reader_records,
                        "xml",
                        out string[] biblio_records,
                        out string[] dup_path,
                        out string output_reader_barcode,
                        out ReturnInfo return_info,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // 检查预约到书库记录，第二册到书，可以被 PATRON0000002 借阅(但其他读者去借阅会被拒绝)
                // 检查两条册记录，预约请求都应该被消掉了

                // 尝试用 PATRON0000001 或 PATRON0000003 去借阅 BOOK0000002，应该被拒绝
                DataModel.SetMessage($"正在试探借书 2 ...");

                {
                    // P1 还 B1。普通还回
                    {
                        lRet = channel.Return(null,
                            "return",
                            "PATRON0000001",
                            "BOOK0000001",
                            "",
                            false,
                            "",
                            "xml",
                            out string[] item_records,
                            "xml",
                            out string[] reader_records,
                            "xml",
                            out string[] biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out ReturnInfo return_info,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }

                    // P2 借 B1 相当于普通借阅，因为刚才满足的是 B2
                    // 借阅完成后，P2 读者记录中的预约到书信息应该还在?
                    {
                        lRet = channel.Borrow(null,
            false,
            "PATRON0000002",
            "BOOK0000001",
            "",
            false,
            null,
            "",
            "xml",
            out string[] item_records,
            "xml",
            out string[] reader_records,
            "xml",
            out string[] biblio_records,
            out string[] dup_path,
            out string output_reader_barcode,
            out BorrowInfo borrow_info,
            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                }

                // testing
                //DataModel.SetMessage("预约测试完成", "green");
                //return new NormalResult();

                {
                    lRet = channel.Borrow(null,
                        false,
                        "PATRON0000001",
                        "BOOK0000002",
                        "",
                        false,
                        null,
                        "",
                        "xml",
                        out string[] item_records,
                        "xml",
                        out string[] reader_records,
                        "xml",
                        out string[] biblio_records,
                        out string[] dup_path,
                        out string output_reader_barcode,
                        out BorrowInfo borrow_info,
                        out strError);
                    if (lRet != -1)
                        goto ERROR1;
                    if (channel.ErrorCode != ErrorCode.BorrowReservationDenied)
                    {
                        strError = $"这里期待返回的错误码为 {ErrorCode.BorrowReservationDenied}";
                        goto ERROR1;
                    }
                }

                {
                    lRet = channel.Borrow(null,
        false,
        "PATRON0000003",
        "BOOK0000002",
        "",
        false,
        null,
        "",
        "xml",
        out string[] item_records,
        "xml",
        out string[] reader_records,
        "xml",
        out string[] biblio_records,
        out string[] dup_path,
        out string output_reader_barcode,
        out BorrowInfo borrow_info,
        out strError);
                    if (lRet != -1)
                        goto ERROR1;
                    if (channel.ErrorCode != ErrorCode.BorrowReservationDenied)
                    {
                        strError = $"这里期待返回的错误码为 {ErrorCode.BorrowReservationDenied}";
                        goto ERROR1;
                    }

                }

                // 只有 P2 借 B2 才能成功
                {
                    lRet = channel.Borrow(null,
        false,
        "PATRON0000002",
        "BOOK0000002",
        "",
        false,
        null,
        "",
        "xml",
        out string[] item_records,
        "xml",
        out string[] reader_records,
        "xml",
        out string[] biblio_records,
        out string[] dup_path,
        out string output_reader_barcode,
        out BorrowInfo borrow_info,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }



                /*
                if (errors.Count > 0)
                {
                    Utility.DisplayErrors(errors);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "; ")
                    };
                }
                */
                DataModel.SetMessage("预约测试完成", "green");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "TestCreateReaderRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.DeleteChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"TestCreateReaderRecord() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }



        public static NormalResult Finish()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _biblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                DataModel.SetMessage("正在删除测试用读者库 ...");
                lRet = channel.ManageDatabase(
    null,
    "delete",
    _globalPatronDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                var user_names = new List<string>() { "test_normal" };
                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(user_names, ",")} ...");
                int nRet = Utility.DeleteUsers(channel,
                    user_names,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                DataModel.SetMessage("结束");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "Finish() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


    }
}
