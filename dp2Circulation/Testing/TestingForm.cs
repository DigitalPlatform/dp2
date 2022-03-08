using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Web;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    public partial class TestingForm : MyForm
    {
        public TestingForm()
        {
            InitializeComponent();
        }

        private void TestingForm_Load(object sender, EventArgs e)
        {

        }

        private void TestingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();
        }

        private void TestingForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.menuStrip1.Enabled = bEnable;
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        // 测试创建索取号
        private void ToolStripMenuItem_createAccessNo_Click(object sender, EventArgs e)
        {
            _ = Task.Factory.StartNew(() =>
              {
                  TestCreateAccessNo("seed");
                  TestCreateAccessNo("");
              });
        }

        // parameters:
        //      strStyle    风格。空/seed。seed 表示排架体系有种次号库
        void TestCreateAccessNo(string strStyle)
        {
            string strError = "";
            int nRet = 0;

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));
            try
            {
                // 创建测试所需的书目库

                string strBiblioDbName = "_测试用中文图书";

                // 如果测试用的书目库以前就存在，要先删除。删除前最好警告一下
                Progress.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    stop,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                Progress.SetMessage("正在创建测试用书目库 ...");
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    this.Progress,
                    strBiblioDbName,
                    "book",
                    "unimarc",
                    "*",
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strSeedDbName = "";
                if (StringUtil.IsInList("seed", strStyle))
                {
                    strSeedDbName = "_测试用种次号库";

                    Progress.SetMessage("正在删除测试用种次号库 ...");
                    lRet = channel.ManageDatabase(
        stop,
        "delete",
        strSeedDbName,    // strDatabaseNames,
        "",
        "",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                            goto ERROR1;
                    }

                    Progress.SetMessage("正在创建测试用种次号库 ...");

                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateSimpleDatabase(
                        channel,
                        this.Progress,
                        strSeedDbName,
                        "zhongcihao",
                        "",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // *** 定义测试所需的馆藏地
                List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem> items = new List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem>();
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试阅览室", true, true));
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试流通库", true, true));

                // 为系统添加新的馆藏地定义
                nRet = ManageHelper.AddLocationTypes(
                    channel,
                    this.Progress,
                    "add",
                    items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 定义排架体系
                string strGroupFragment =
                    "<group name=\"_测试种次号\" classType=\"中图法\" qufenhaoType=\"种次号\" zhongcihaodb=\"" + strSeedDbName + "\" callNumberStyle=\"索取类号+区分号\">"
                    + "<location name=\"_测试阅览室\" />"
                    + "<location name=\"_测试流通库\" />"
                    + "</group>";
                string strOldCallNumberDef = "";
                nRet = ManageHelper.ChangeCallNumberDef(
                    channel,
                    this.Progress,
                    strGroupFragment,
                    out strOldCallNumberDef,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();

                // *** 创建书目记录
                // 创建册记录和索取号

                this.Invoke((Action)(() =>
    UiTest1(strBiblioDbName, "")
    ));

                this.Invoke((Action)(() =>
UiTest1(strBiblioDbName, "save_before")
));


                this.Invoke((Action)(() =>
UiTest2(strBiblioDbName, "")
));

                this.Invoke((Action)(() =>
UiTest2(strBiblioDbName, "save_before")
));

                this.Invoke((Action)(() =>
UiTest3(strBiblioDbName)
));

                // 删除测试用的书目库、排架体系、馆藏地定义
                Progress.SetMessage("正在删除测试用书目库 ...");
                lRet = channel.ManageDatabase(
    stop,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(strSeedDbName) == false)
                {
                    Progress.SetMessage("正在删除测试用种次号库 ...");
                    lRet = channel.ManageDatabase(
        stop,
        "delete",
        strSeedDbName,    // strDatabaseNames,
        "",
        "",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                Progress.SetMessage("正在复原测试前的排架体系 ...");
                nRet = ManageHelper.RestoreCallNumberDef(
    channel,
    this.Progress,
    strOldCallNumberDef,
    out strError);
                if (nRet == -1)
                    goto ERROR1;


                Progress.SetMessage("正在删除测试用的馆藏地 ...");
                nRet = ManageHelper.AddLocationTypes(
    channel,
    this.Progress,
    "remove",
    items,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();

                return;
            }
            catch (Exception ex)
            {
                strError = "TestCreateAccessNo() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
        }

        void ReloadDatabaseCfg()
        {
            this.Invoke((Action)(() =>
            {
                int nRet = 0;
                // 获得各种类型的数据库的检索途径
                nRet = Program.MainForm.GetDbFromInfos(false);
                if (nRet == -1)
                    goto END1;

                // 获得全部数据库的定义
                nRet = Program.MainForm.GetAllDatabaseInfo(false);
                if (nRet == -1)
                    goto END1;

                // 获得书目库属性列表
                nRet = Program.MainForm.InitialBiblioDbProperties(false);
                if (nRet == -1)
                    goto END1;

                // 获得索取号配置信息
                // 2009/2/24 
                nRet = Program.MainForm.GetCallNumberInfo(false);
                if (nRet == -1)
                    goto END1;
                return;
                END1:
                return;
            }
            ));
        }

        // 新创建十个册记录在内存，然后发生索取号，然后统一保存。要求索取号完全相同
        // parameters:
        //      strStyle    save_before 先保存创建的册记录，再给它创建索取号，然后再次保存
        void UiTest1(string strBiblioDbName, string strStyle)
        {
            string strError = "";

            EntityForm entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                MarcRecord record = new MarcRecord();
                record.add(new MarcField('$', "200  $atest title"));
                record.add(new MarcField('$', "690  $aI247.5"));
                record.add(new MarcField('$', "701  $a测试著者"));

                // 将 MARC 机内格式记录赋予窗口
                entity_form.MarcEditor.Marc = record.Text;
                entity_form.BiblioRecPath = strBiblioDbName + "/?";
                entity_form.InitialPages();
                entity_form.ActivateItemsPage();

                // 保存
                entity_form.DoSaveAll("");

                string strBiblioRecPath = entity_form.BiblioRecPath;

                // 创建册记录
                CreateEntityRecords(entity_form, 10);

                if (StringUtil.IsInList("save_before", strStyle) == true)
                    entity_form.DoSaveAll("");

                ListViewUtil.SelectAllLines(entity_form.EntityControl.ListView);

                // 为当前选定的事项创建索取号
                // return:
                //      -1  出错
                //      0   放弃处理
                //      1   已经处理
                int nRet = entity_form.EntityControl.CreateCallNumber(true, out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                // 保存
                entity_form.DoSaveAll("");

                // MessageBox.Show(this, "暂停");

                // 验证
                string strSave = null;
                foreach (ListViewItem item in entity_form.EntityControl.ListView.Items)
                {
                    string strAccessNo = ListViewUtil.GetItemText(item, BookItem.COLUMN_ACCESSNO);
                    if (strSave != null)
                    {
                        if (strAccessNo != strSave)
                            throw new VerifyException("UiTest1() 中验证阶段发现错误：两个册的索取号创建得不一样 '" + strAccessNo + "' '" + strSave + "'");
                    }

                    strSave = strAccessNo;
                }
            }
            finally
            {
                entity_form.Close();
            }

        }

        // 每创建一个册记录在内存，然后发生索取号，保存。再创建下一个索取号。要求索取号完全相同
        // parameters:
        //      strStyle    save_before 先保存创建的册记录，再给它创建索取号，然后再次保存
        void UiTest2(string strBiblioDbName, string strStyle)
        {
            string strError = "";

            EntityForm entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                MarcRecord record = new MarcRecord();
                record.add(new MarcField('$', "200  $atest title"));
                record.add(new MarcField('$', "690  $aI247.5"));
                record.add(new MarcField('$', "701  $a测试著者"));

                // 将 MARC 机内格式记录赋予窗口
                entity_form.MarcEditor.Marc = record.Text;
                entity_form.BiblioRecPath = strBiblioDbName + "/?";
                entity_form.InitialPages();
                entity_form.ActivateItemsPage();

                // 保存
                entity_form.DoSaveAll("");

                string strBiblioRecPath = entity_form.BiblioRecPath;

                for (int i = 0; i < 10; i++)
                {
                    // 创建册记录
                    CreateEntityRecords(entity_form, 1);

                    if (StringUtil.IsInList("save_before", strStyle) == true)
                        entity_form.DoSaveAll("");

                    ListViewUtil.ClearSelection(entity_form.EntityControl.ListView);
                    // 只选定最后一行
                    entity_form.EntityControl.ListView.Items[entity_form.EntityControl.ListView.Items.Count - 1].Selected = true;

                    // 为当前选定的事项创建索取号
                    // return:
                    //      -1  出错
                    //      0   放弃处理
                    //      1   已经处理
                    int nRet = entity_form.EntityControl.CreateCallNumber(false, out strError);
                    if (nRet == -1)
                        throw new Exception(strError);

                    // 保存
                    entity_form.DoSaveAll("");
                }

                // 验证
                string strSave = null;
                foreach (ListViewItem item in entity_form.EntityControl.ListView.Items)
                {
                    string strAccessNo = ListViewUtil.GetItemText(item, BookItem.COLUMN_ACCESSNO);
                    if (strSave != null)
                    {
                        if (strAccessNo != strSave)
                            throw new VerifyException("UiTest() 中验证阶段发现错误：两个册的索取号创建得不一样 '" + strAccessNo + "' '" + strSave + "'");
                    }

                    strSave = strAccessNo;
                }
            }
            finally
            {
                entity_form.Close();
            }
        }

        // 多个窗口并发创建索取号，然后再统一保存
        void UiTest3(string strBiblioDbName//,
                                           //string strStyle
            )
        {
            string strError = "";

            EntityForm entity_form1 = new EntityForm();
            EntityForm entity_form2 = new EntityForm();
            try
            {
                entity_form1.MainForm = Program.MainForm;
                entity_form1.MdiParent = Program.MainForm;
                entity_form1.Show();

                entity_form2.MainForm = Program.MainForm;
                entity_form2.MdiParent = Program.MainForm;
                entity_form2.Show();

                {
                    entity_form1.Activate();

                    MarcRecord record = new MarcRecord();
                    record.add(new MarcField('$', "200  $atest title 1"));
                    record.add(new MarcField('$', "690  $aI247.5"));
                    record.add(new MarcField('$', "701  $a测试著者1"));

                    // 将 MARC 机内格式记录赋予窗口
                    entity_form1.MarcEditor.Marc = record.Text;
                    entity_form1.BiblioRecPath = strBiblioDbName + "/?";
                    entity_form1.InitialPages();
                    entity_form1.ActivateItemsPage();

                    // 创建册记录
                    CreateEntityRecords(entity_form1, 10);

                    ListViewUtil.SelectAllLines(entity_form1.EntityControl.ListView);

                    // 为当前选定的事项创建索取号
                    // return:
                    //      -1  出错
                    //      0   放弃处理
                    //      1   已经处理
                    int nRet = entity_form1.EntityControl.CreateCallNumber(true, out strError);
                    if (nRet == -1)
                        throw new Exception(strError);

                }

                {
                    entity_form2.Activate();

                    MarcRecord record = new MarcRecord();
                    record.add(new MarcField('$', "200  $atest title 2"));
                    record.add(new MarcField('$', "690  $aI247.5"));
                    record.add(new MarcField('$', "701  $a测试著者2"));

                    // 将 MARC 机内格式记录赋予窗口
                    entity_form2.MarcEditor.Marc = record.Text;
                    entity_form2.BiblioRecPath = strBiblioDbName + "/?";
                    entity_form2.InitialPages();
                    entity_form2.ActivateItemsPage();

                    // 创建册记录
                    CreateEntityRecords(entity_form2, 10);

                    ListViewUtil.SelectAllLines(entity_form2.EntityControl.ListView);

                    // 为当前选定的事项创建索取号
                    // return:
                    //      -1  出错
                    //      0   放弃处理
                    //      1   已经处理
                    int nRet = entity_form2.EntityControl.CreateCallNumber(true, out strError);
                    if (nRet == -1)
                        throw new Exception(strError);

                }

                // 保存
                entity_form1.DoSaveAll("");
                entity_form2.DoSaveAll("");

                // 验证
                string strNumber1 = "";
                {
                    string strSave = null;
                    foreach (ListViewItem item in entity_form1.EntityControl.ListView.Items)
                    {
                        string strAccessNo = ListViewUtil.GetItemText(item, BookItem.COLUMN_ACCESSNO);
                        if (strSave != null)
                        {
                            if (strAccessNo != strSave)
                                throw new VerifyException("UiTest3() 中验证阶段 1 发现错误：两个册的索取号创建得不一样 '" + strAccessNo + "' '" + strSave + "'");
                        }

                        strSave = strAccessNo;
                    }

                    List<string> parts = StringUtil.ParseTwoPart(strSave, "/");
                    strNumber1 = parts[1];
                }

                string strNumber2 = "";
                {
                    string strSave = null;
                    foreach (ListViewItem item in entity_form2.EntityControl.ListView.Items)
                    {
                        string strAccessNo = ListViewUtil.GetItemText(item, BookItem.COLUMN_ACCESSNO);
                        if (strSave != null)
                        {
                            if (strAccessNo != strSave)
                                throw new VerifyException("UiTest3() 中验证阶段 2 发现错误：两个册的索取号创建得不一样 '" + strAccessNo + "' '" + strSave + "'");
                        }

                        strSave = strAccessNo;
                    }

                    List<string> parts = StringUtil.ParseTwoPart(strSave, "/");
                    strNumber2 = parts[1];
                }

                // 2 应该比 1 大 1
                {
                    Int32 v1 = Convert.ToInt32(strNumber1);
                    Int32 v2 = Convert.ToInt32(strNumber2);

                    if (v2 != v1 + 1)
                        throw new VerifyException("UiTest3() 中验证阶段 3 发现错误：两个书目记录的种次号不是差 1 (strNumber1='" + strNumber1 + "' '" + strNumber2 + "')");
                }

            }
            finally
            {
                entity_form1.Close();
                entity_form2.Close();
            }
        }

        // 创建若干册记录
        // 返回参考 ID 列表
        static List<string> CreateEntityRecords(EntityForm entity_form, int nCount)
        {
            string strError = "";

            List<string> refids = new List<string>();
            for (int i = 0; i < nCount; i++)
            {
                BookItem bookitem = new BookItem();
                bookitem.Barcode = "";
                bookitem.Location = "_测试阅览室";

                int nRet = entity_form.EntityControl.AppendEntity(bookitem,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                int index = entity_form.EntityControl.ListView.Items.Count - 1;
                ListViewItem new_item = entity_form.EntityControl.ListView.Items[index];
                string refid = ListViewUtil.GetItemText(new_item, BookItem.COLUMN_REFID);
                refids.Add(refid);
            }

            return refids;
        }

        // 移动书目记录
        private void ToolStripMenuItem_moveBiblioRecord_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                MoveBiblioRecord("");
            });

        }

        #region 移动书目记录

        void MoveBiblioRecord(string strStyle)
        {
            string strError = "";
            int nRet = 0;

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));
            try
            {
                // 创建测试所需的书目库

                string strBiblioDbName = "_测试用中文图书";

                // 如果测试用的书目库以前就存在，要先删除。删除前最好警告一下
                Progress.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    stop,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                Progress.SetMessage("正在创建测试用书目库 ...");
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    this.Progress,
                    strBiblioDbName,
                    "book",
                    "unimarc",
                    "*",
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // *** 定义测试所需的馆藏地
                List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem> items = new List<DigitalPlatform.CirculationClient.ManageHelper.LocationItem>();
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试阅览室", true, true));
                items.Add(new DigitalPlatform.CirculationClient.ManageHelper.LocationItem("", "_测试流通库", true, true));

                // 为系统添加新的馆藏地定义
                nRet = ManageHelper.AddLocationTypes(
                    channel,
                    this.Progress,
                    "add",
                    items,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();

                // *** 界面测试
#if NO
                this.Invoke((Action)(() =>
UiTest_copyBiblioRecord_1(strBiblioDbName, "reserve_source,child_reserve_source,copy_copychildrecords")
));

                this.Invoke((Action)(() =>
UiTest_copyBiblioRecord_1(strBiblioDbName, "reserve_target,child_reserve_target,copy_copychildrecords")
));
#endif
                {
                    List<string> styles = Build_copyBiblioRecord_styleCombination();
                    styles.Sort();
                    foreach (string style in styles)
                    {
                        this.Invoke((Action)(() =>
    UiTest_copyBiblioRecord_1(strBiblioDbName, strStyle)
    ));
                    }
                }



#if NO
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,child_reserve_source")
));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_target,child_reserve_target")
));
#endif

#if NO
                // 追加方式移动
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,create_objects,append_target")
));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,create_objects,append_target,change_biblio_before")
));

                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,append_target")
));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,append_targe,change_biblio_before")
));

                // 已有目标记录的方式移动
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,create_objects")
));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_target,create_objects")
));

                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source,change_biblio_before")
));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_target,change_biblio_before")
));


                this.Invoke((Action)(() =>
    UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_source")
    ));
                this.Invoke((Action)(() =>
UiTest_moveBiblioRecord_1(strBiblioDbName, "reserve_target")
));
#endif

                {
                    List<string> styles = Build_moveBiblioRecord_styleCombination();
                    styles.Sort();
                    foreach (string style in styles)
                    {
                        this.Invoke((Action)(() =>
    UiTest_moveBiblioRecord_1(strBiblioDbName, strStyle)
    ));
                    }
                }

                // 删除测试用的书目库、排架体系、馆藏地定义
                Progress.SetMessage("正在删除测试用书目库 ...");
                lRet = channel.ManageDatabase(
    stop,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Progress.SetMessage("正在删除测试用的馆藏地 ...");
                nRet = ManageHelper.AddLocationTypes(
    channel,
    this.Progress,
    "remove",
    items,
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();

                return;
            }
            catch (Exception ex)
            {
                strError = "MoveBiblioRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
        }

        static string[] copy_faces = new string[] {
            "reserve_source",
            "reserve_target",
            "create_objects",
            "append_target",
            "change_biblio_before",
        "child_reserve_source",
        "child_reserve_target"};

        // 构造出用于测试 复制书目记录 功能的全部可用 style 字符串
        static List<string> Build_copyBiblioRecord_styleCombination()
        {
            List<string> styles = new List<string>();
            int length = copy_faces.Length;
#if NO
            IEnumerable<IEnumerable<int>> result =
    GetPermutations(Enumerable.Range(0, length - 1), length - 1);
#endif
            List<int> base_array = new List<int>();
            for (int i = 0; i < length; i++)
            {
                base_array.Add(i);
            }
            List<List<int>> result = Permutation.GetPermuation(base_array);

            foreach (IEnumerable<int> indices in result)
            {
                List<string> list = new List<string>();
                foreach (int i in indices)
                {
                    list.Add(move_faces[i]);
                }

                styles.Add(StringUtil.MakePathList(list));
            }

            List<string> styles1 = new List<string>();
            // 去掉一些会冲突的组合
            foreach (string style in styles)
            {
                // 去掉 reserve_target reserve_source 一个都没有的组合
                if (StringUtil.IsInList("reserve_source", style) == false
    && StringUtil.IsInList("reserve_target", style) == false)
                    continue;

                // 去掉 reserve_target reserve_source 两个同时具有的组合
                if (StringUtil.IsInList("reserve_source", style) == true
    && StringUtil.IsInList("reserve_target", style) == true)
                    continue;

                // 去掉会冲突的组合
                if (StringUtil.IsInList("reserve_target", style) == true
                    && StringUtil.IsInList("append_target", style) == true)
                    continue;

                // 确保
                string strStyle = style;
                if (StringUtil.IsInList("child_reserve_source", strStyle) == true)
                    StringUtil.SetInList(ref strStyle, "copy_copychildrecords", true);

                styles1.Add(strStyle);
            }

            return styles1;
        }

        static string[] move_faces = new string[] {
            "reserve_source",
            "reserve_target",
            "create_objects",
            "append_target",
            "change_biblio_before",
        "child_reserve_source",
        "child_reserve_target"};

        static IEnumerable<IEnumerable<T>>
    GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        // 构造出用于测试 移动书目记录 功能的全部可用 style 字符串
        static List<string> Build_moveBiblioRecord_styleCombination()
        {
            List<string> styles = new List<string>();
            int length = move_faces.Length;
#if NO
            IEnumerable<IEnumerable<int>> result =
    GetPermutations(Enumerable.Range(0, length - 1), length - 1);
#endif
            List<int> base_array = new List<int>();
            for (int i = 0; i < length; i++)
            {
                base_array.Add(i);
            }
            List<List<int>> result = Permutation.GetPermuation(base_array);

            foreach (IEnumerable<int> indices in result)
            {
                List<string> list = new List<string>();
                foreach (int i in indices)
                {
                    list.Add(move_faces[i]);
                }

                styles.Add(StringUtil.MakePathList(list));
            }

            List<string> styles1 = new List<string>();
            // 去掉一些会冲突的组合
            foreach (string style in styles)
            {
                // 去掉 reserve_target reserve_source 一个都没有的组合
                if (StringUtil.IsInList("reserve_source", style) == false
    && StringUtil.IsInList("reserve_target", style) == false)
                    continue;

                // 去掉 reserve_target reserve_source 两个同时具有的组合
                if (StringUtil.IsInList("reserve_source", style) == true
    && StringUtil.IsInList("reserve_target", style) == true)
                    continue;

                // 去掉会冲突的组合
                if (StringUtil.IsInList("reserve_target", style) == true
                    && StringUtil.IsInList("append_target", style) == true)
                    continue;

                styles1.Add(style);
            }

            return styles1;
        }

        // 创建两条书目记录，然后移动一条去覆盖另外一条
        // parameters:
        //      strStyle    reserve_source reserve_target 分别表示保留源和保留目标书目记录(指移动之后的目标记录)
        //                  change_biblio_before 表示在移动操作前故意修改一下 MARC 编辑器中的书目记录的题名字段，而且不做保存
        //                  create_objects 表示要为参与测试的书目记录创建对象文件，这样移动的时候就是带着对象移动了
        //                  append_target 表示移动源书目记录到书目库尾部，以追加的方式。这时也没有必要先创建测试用的目标书目记录了
        //                          注意，append_target 只能和 reserve_source 配合使用，不允许和 reserve_target 配合使用。因为移动前，目标记录是不存在的，也就无所谓保留目标记录了
        //                  child_reserve_source child_reserve_target 表示下属的册记录等要如何保留。如果这两个值同时都缺，默认为 "child_reserve_source,child_reserve_target" 效果，即合并下级记录
        void UiTest_moveBiblioRecord_1(string strBiblioDbName, string strStyle)
        {
            string strError = "";

            if (StringUtil.IsInList("reserve_source", strStyle) == true
    && StringUtil.IsInList("reserve_target", strStyle) == true)
                throw new ArgumentException("strStyle 中 reserve_source 和 reserve_target 不应该同时具备。只能使用其中一个");

            // 如果都缺，则默认 reserve_source
            if (StringUtil.IsInList("reserve_source", strStyle) == false
                && StringUtil.IsInList("reserve_target", strStyle) == false)
                StringUtil.SetInList(ref strStyle, "reserve_source", true);

            // 如果都缺，则默认 两个组合的情况
            if (StringUtil.IsInList("child_reserve_source", strStyle) == false
                && StringUtil.IsInList("child_reserve_target", strStyle) == false)
            {
                StringUtil.SetInList(ref strStyle, "child_reserve_source", true);
                StringUtil.SetInList(ref strStyle, "child_reserve_target", true);
            }


            // 检查参数
            if (StringUtil.IsInList("append_target", strStyle)
                && StringUtil.IsInList("reserve_target", strStyle))
                throw new ArgumentException("strStyle 中 append_target 只能和 reserve_source 配套使用，不允许和 reserve_target 配套使用");

            BiblioCreationInfo info1 = CreateBiblioRecord(strBiblioDbName,
                "源记录",
                strStyle);
            BiblioCreationInfo info2 = null;
            if (StringUtil.IsInList("append_target", strStyle))
            {
                info2 = new BiblioCreationInfo();
                info2.BiblioRecPath = strBiblioDbName + "/?";
            }
            else
            {
                info2 = CreateBiblioRecord(strBiblioDbName,
         "目标记录",
         strStyle);
            }

            EntityForm entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                entity_form.SafeLoadRecord(info1.BiblioRecPath, "");

                if (StringUtil.IsInList("change_biblio_before", strStyle))
                {
                    ChangeBiblioTitle(entity_form, "源记录_changed");
                }

                MergeStyle autoMergeStyle = MergeStyle.None;
                if (StringUtil.IsInList("reserve_source", strStyle))
                {
                    //MessageBox.Show(this, "请注意在后面对话框中选择 采用源 书目记录");
                    autoMergeStyle = MergeStyle.ReserveSourceBiblio | MergeStyle.CombineSubrecord;
                }
                if (StringUtil.IsInList("reserve_target", strStyle))
                {
                    //MessageBox.Show(this, "请注意在后面对话框中选择 采用目标 书目记录");
                    autoMergeStyle = MergeStyle.ReserveTargetBiblio | MergeStyle.CombineSubrecord;
                }

                if (StringUtil.IsInList("child_reserve_source", strStyle)
                    && StringUtil.IsInList("child_reserve_target", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.CombineSubrecord;
                }
                else if (StringUtil.IsInList("child_reserve_source", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.OverwriteSubrecord;
                }
                else if (StringUtil.IsInList("child_reserve_target", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.MissingSourceSubrecord;
                }
                else
                    throw new ArgumentException("child_xxx 使用不正确");

                int nRet = entity_form.MoveTo(
                    "move",
                    info2.BiblioRecPath,
                    null,
                    autoMergeStyle,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                //
                if (StringUtil.IsInList("append_target", strStyle))
                {
                    // 移动完成后才知道目标记录的路径
                    info2.BiblioRecPath = entity_form.BiblioRecPath;
                }

                {
                    // 检查窗口中的书目记录是否符合要求
                    if (StringUtil.IsInList("reserve_source", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form,
                            StringUtil.IsInList("change_biblio_before", strStyle) ? "源记录_changed" : "源记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }
                    if (StringUtil.IsInList("reserve_target", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form, "目标记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }

                    // 检查窗口中的册记录是否符合要求
                    // 主要检查 refid 列表
                    strError = VerifyItems(entity_form, info1.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_source", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);
                    strError = VerifyItems(entity_form, info2.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_target", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);

                    // 检查窗口中的对象记录是否符合要求
                    if (StringUtil.IsInList("create_objects", strStyle))
                    {
                        if (StringUtil.IsInList("reserve_source", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                        if (StringUtil.IsInList("reserve_target", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                    }

                }

            }
            finally
            {
                entity_form.Close();
                entity_form = null;
            }

            // 重新打开种册窗装载目标记录，再次检测一次。因为有时候第一次在窗口没有关闭的时候看起来对了，但重新打开又不对了
            entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                entity_form.SafeLoadRecord(info2.BiblioRecPath, "");

                {
                    // 检查窗口中的书目记录是否符合要求
                    if (StringUtil.IsInList("reserve_source", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form,
                            StringUtil.IsInList("change_biblio_before", strStyle) ? "源记录_changed" : "源记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }
                    if (StringUtil.IsInList("reserve_target", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form, "目标记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }

                    // 检查窗口中的册记录是否符合要求
                    // 主要检查 refid 列表
                    strError = VerifyItems(entity_form, info1.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_source", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);
                    strError = VerifyItems(entity_form, info2.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_target", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);

                    // 检查窗口中的对象记录是否符合要求
                    if (StringUtil.IsInList("create_objects", strStyle))
                    {
                        if (StringUtil.IsInList("reserve_source", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                        if (StringUtil.IsInList("reserve_target", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                    }
                }

            }
            finally
            {
                entity_form.Close();
            }

            // 检查源书目记录，应该已经不存在
            VerifyBiblioRecordExisting(info1.BiblioRecPath);
        }


        // 创建两条书目记录，然后复制一条去覆盖另外一条
        // parameters:
        //      strStyle    reserve_source reserve_target 分别表示保留源和保留目标书目记录(指移动之后的目标记录)
        //                  change_biblio_before 表示在移动操作前故意修改一下 MARC 编辑器中的书目记录的题名字段，而且不做保存
        //                  create_objects 表示要为参与测试的书目记录创建对象文件，这样移动的时候就是带着对象移动了
        //                  append_target 表示移动源书目记录到书目库尾部，以追加的方式。这时也没有必要先创建测试用的目标书目记录了
        //                          注意，append_target 只能和 reserve_source 配合使用，不允许和 reserve_target 配合使用。因为移动前，目标记录是不存在的，也就无所谓保留目标记录了
        //                  child_reserve_source child_reserve_target 表示下属的册记录等要如何保留。如果这两个值同时都缺，默认为 "child_reserve_source,child_reserve_target" 效果，即合并下级记录
        //                  copy_copychildrecords 是否复制下级记录?
        //                  copy_buildlink          是否创建新记录到旧记录的 link 字段?
        //                  //copy_enablesubrecord    是否允许下级记录
        void UiTest_copyBiblioRecord_1(string strBiblioDbName, string strStyle)
        {
            string strError = "";

            if (StringUtil.IsInList("reserve_source", strStyle) == true
    && StringUtil.IsInList("reserve_target", strStyle) == true)
                throw new ArgumentException("strStyle 中 reserve_source 和 reserve_target 不应该同时具备。只能使用其中一个");

            // 如果都缺，则默认 reserve_source
            if (StringUtil.IsInList("reserve_source", strStyle) == false
                && StringUtil.IsInList("reserve_target", strStyle) == false)
                StringUtil.SetInList(ref strStyle, "reserve_source", true);

            // 如果都缺，则默认 两个组合的情况
            if (StringUtil.IsInList("child_reserve_source", strStyle) == false
                && StringUtil.IsInList("child_reserve_target", strStyle) == false)
            {
                StringUtil.SetInList(ref strStyle, "child_reserve_source", true);
                StringUtil.SetInList(ref strStyle, "copy_copychildrecords", true);
                StringUtil.SetInList(ref strStyle, "child_reserve_target", true);
            }

            if (StringUtil.IsInList("child_reserve_source", strStyle) == true
&& StringUtil.IsInList("copy_copychildrecords", strStyle) == false)
                throw new ArgumentException("strStyle 中 child_reserve_source 具备时，也应该具备 copy_copychildrecords");

            // 检查参数
            if (StringUtil.IsInList("append_target", strStyle)
                && StringUtil.IsInList("reserve_target", strStyle))
                throw new ArgumentException("strStyle 中 append_target 只能和 reserve_source 配套使用，不允许和 reserve_target 配套使用");

            BiblioCreationInfo info1 = CreateBiblioRecord(strBiblioDbName,
                "源记录",
                strStyle);
            BiblioCreationInfo info2 = null;
            if (StringUtil.IsInList("append_target", strStyle))
            {
                info2 = new BiblioCreationInfo();
                info2.BiblioRecPath = strBiblioDbName + "/?";
            }
            else
            {
                info2 = CreateBiblioRecord(strBiblioDbName,
         "目标记录",
         strStyle);
            }

            EntityForm.CopyParam copy_param = new EntityForm.CopyParam();
            copy_param.CopyChildRecords = StringUtil.IsInList("copy_copychildrecords", strStyle);
            copy_param.BuildLink = StringUtil.IsInList("copy_buildlink", strStyle);
            // copy_param.EnableSubRecord = StringUtil.IsInList("copy_enablesubrecord", strStyle);


            EntityForm entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                entity_form.SafeLoadRecord(info1.BiblioRecPath, "");

                if (StringUtil.IsInList("change_biblio_before", strStyle))
                {
                    ChangeBiblioTitle(entity_form, "源记录_changed");
                }

                MergeStyle autoMergeStyle = MergeStyle.None;
                if (StringUtil.IsInList("reserve_source", strStyle))
                {
                    //MessageBox.Show(this, "请注意在后面对话框中选择 采用源 书目记录");
                    autoMergeStyle = MergeStyle.ReserveSourceBiblio | MergeStyle.CombineSubrecord;
                }
                if (StringUtil.IsInList("reserve_target", strStyle))
                {
                    //MessageBox.Show(this, "请注意在后面对话框中选择 采用目标 书目记录");
                    autoMergeStyle = MergeStyle.ReserveTargetBiblio | MergeStyle.CombineSubrecord;
                }

                // 目前 child_xxx 这几个值主要是控制 MoveTo() 函数的 autoMergeStyle 之用。以达到自动测试的目的。
                // 而在种册窗的真实用户操作界面中，是靠用户通过鼠标选择对话框上的合并方式来实现功能的，和这里的原理不同，特此说明
                if (StringUtil.IsInList("child_reserve_source", strStyle)
                    && StringUtil.IsInList("child_reserve_target", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.CombineSubrecord;
                }
                else if (StringUtil.IsInList("child_reserve_source", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.OverwriteSubrecord;
                }
                else if (StringUtil.IsInList("child_reserve_target", strStyle))
                {
                    autoMergeStyle -= (autoMergeStyle & MergeStyle.SubRecordMask);
                    autoMergeStyle |= MergeStyle.MissingSourceSubrecord;
                }
                else
                    throw new ArgumentException("child_xxx 使用不正确");

                int nRet = entity_form.MoveTo(
                    "copy",
                    info2.BiblioRecPath,
                    copy_param,
                    autoMergeStyle,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                //
                if (StringUtil.IsInList("append_target", strStyle))
                {
                    // 移动完成后才知道目标记录的路径
                    info2.BiblioRecPath = entity_form.BiblioRecPath;
                }

                {
                    // 检查窗口中的书目记录是否符合要求
                    if (StringUtil.IsInList("reserve_source", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form,
                            StringUtil.IsInList("change_biblio_before", strStyle) ? "源记录_changed" : "源记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }
                    if (StringUtil.IsInList("reserve_target", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form, "目标记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }

                    // 检查窗口中的册记录是否符合要求
                    // 主要检查 refid 列表
                    strError = VerifyItems_useOldRefID(entity_form, info1.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_source", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);
                    strError = VerifyItems(entity_form, info2.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_target", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);

                    // 检查窗口中的对象记录是否符合要求
                    if (StringUtil.IsInList("create_objects", strStyle))
                    {
                        if (StringUtil.IsInList("reserve_source", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                        if (StringUtil.IsInList("reserve_target", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                    }

                }

            }
            finally
            {
                entity_form.Close();
                entity_form = null;
            }

            // 重新打开种册窗装载目标记录，再次检测一次。因为有时候第一次在窗口没有关闭的时候看起来对了，但重新打开又不对了
            entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                entity_form.SafeLoadRecord(info2.BiblioRecPath, "");

                {
                    // 检查窗口中的书目记录是否符合要求
                    if (StringUtil.IsInList("reserve_source", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form,
                            StringUtil.IsInList("change_biblio_before", strStyle) ? "源记录_changed" : "源记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }
                    if (StringUtil.IsInList("reserve_target", strStyle))
                    {
                        strError = VerifyBiblioTitle(entity_form, "目标记录");
                        if (string.IsNullOrEmpty(strError) == false)
                            throw new Exception(strError);
                    }

                    // 检查窗口中的册记录是否符合要求
                    // 主要检查 refid 列表
                    strError = VerifyItems_useOldRefID(entity_form, info1.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_source", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);
                    strError = VerifyItems(entity_form, info2.ItemRefIDs,
                        StringUtil.IsInList("child_reserve_target", strStyle));
                    if (string.IsNullOrEmpty(strError) == false)
                        throw new Exception(strError);

                    // 检查窗口中的对象记录是否符合要求
                    if (StringUtil.IsInList("create_objects", strStyle))
                    {
                        if (StringUtil.IsInList("reserve_source", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                        if (StringUtil.IsInList("reserve_target", strStyle))
                        {
                            strError = VerifyObjects(entity_form, info1.Objects, false);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);

                            strError = VerifyObjects(entity_form, info2.Objects, true);
                            if (string.IsNullOrEmpty(strError) == false)
                                throw new Exception(strError);
                        }
                    }
                }

            }
            finally
            {
                entity_form.Close();
            }

            // 检查源书目记录，应该已经不存在
            VerifyBiblioRecordExisting(info1.BiblioRecPath);
        }


        string VerifyBiblioRecordExisting(string strBiblioRecPath)
        {
            LibraryChannel channel = this.GetChannel();

            try
            {
                string[] results = null;
                byte[] baTimestamp = null;
                string strError = "";
                List<string> format_list = new List<string>();
                format_list.Add("xml");
                long lRet = channel.GetBiblioInfos(
                    Progress,
                    strBiblioRecPath,
                    "",
                    format_list.ToArray(),
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                    return null;
                if (lRet == -1)
                    throw new Exception(strError);
                return "书目记录 '" + strBiblioRecPath + "' 尚存在，不符合测试要求";
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 验证种册窗里面的 MARC 编辑器中的题名是否符合要求
        static string VerifyBiblioTitle(EntityForm entity_form, string strTitle)
        {
            MarcRecord record = new MarcRecord(entity_form.MarcEditor.Marc);
            string strContent = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
            if (strContent != strTitle)
            {
                return "种册窗中的题名 '" + strContent + "' 和期望的题名 '" + strTitle + "' 不一致";
            }

            return null;    // 表示正确
        }

        // 所创建的对象记录信息。用于最后核对验证
        class ObjectCreationInfo
        {
            public string LocalFilePath { get; set; }   // 本地物理文件全路径
            public long Size { get; set; }  // 文件尺寸
            public string MD5 { get; set; }
        }

        // 验证种册窗中是否具备特定特征的对象
        // parameters:
        //      bExist  验证方向。如果 == true，表示要验证这些对象存在；== false，表示要验证这些对象不存在
        static string VerifyObjects(EntityForm entity_form,
            List<ObjectCreationInfo> infos,
            bool bExist)
        {
            if (infos == null)
                return null;
            foreach (ObjectCreationInfo info in infos)
            {
                string strMD5 = info.MD5;
                List<ListViewItem> items = entity_form.BinaryResControl.FindItemByUsage(strMD5);
                if (items == null || items.Count == 0)
                {
                    if (bExist == true)
                        return "特征 '" + strMD5 + "' 没有找到对应的对象";
                    continue;
                }

                if (bExist == false)
                    return "特征为 '" + strMD5 + "' 的对象不应该存在";

                if (bExist == true && items.Count != 1)
                {
                    return "特征 '" + strMD5 + "' 找到的对象不唯一 (" + items.Count + ")";
                }
            }

            return null;
        }

        // 为种册窗添加若干对象文件
        static List<ObjectCreationInfo> AddObjects(EntityForm entity_form,
            string strTitle,
            int count)
        {
            List<ObjectCreationInfo> infos = new List<ObjectCreationInfo>();

            for (int i = 0; i < count; i++)
            {
                string strError = "";

                ObjectCreationInfo info = new ObjectCreationInfo();
                infos.Add(info);

                // 发生一个本地文件
                info.LocalFilePath = Program.MainForm.GetTempFileName("testing");
                using (StreamWriter sw = new StreamWriter(info.LocalFilePath, false, Encoding.UTF8))
                {
                    for (int j = 0; j < i + 1; j++)
                    {
                        sw.WriteLine(strTitle + " " + j);
                    }
                }

                using (StreamReader sr = new StreamReader(info.LocalFilePath, Encoding.UTF8))
                {
                    info.MD5 = StringUtil.GetMd5(sr.ReadToEnd());
                }

                ListViewItem new_item = null;
                int nRet = entity_form.BinaryResControl.AppendNewItem(
                info.LocalFilePath,
                info.MD5,
            out new_item,
            out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                info.Size = (new FileInfo(info.LocalFilePath)).Length;
            }

            return infos;
        }

        // 修改种册窗里面的 MARC 编辑器中的题名
        static void ChangeBiblioTitle(EntityForm entity_form, string strNewTitle)
        {
            MarcRecord record = new MarcRecord(entity_form.MarcEditor.Marc);
            record.setFirstSubfield("200", "a", strNewTitle);

            entity_form.MarcEditor.Marc = record.Text;
        }

        // 检查 refids 中的参考 ID 是否都存在
        // parameters:
        //      bExist  验证方向。如果 == true，表示要验证这些册存在；== false，表示要验证这些册不存在
        static string VerifyItems_useOldRefID(EntityForm entity_form,
            List<string> refids,
            bool bExist)
        {
            if (refids == null)
                return null;
            foreach (string refid in refids)
            {
                BookItemBase item = entity_form.EntityControl.Items.GetItemByOldRefID(refid);
                // TODO: 如果 item 找到，要进一步验证其 oldRefID 成员是否符合查找参数
                if (bExist == true)
                {
                    if (item == null)
                        return "(oldRefID)参考 ID 为 '" + refid + "' 的册记录事项在种册窗的“册”属性页中没有找到";
                }
                else
                {
                    if (item != null)
                        return "(oldRefID)参考 ID 为 '" + refid + "' 的册记录事项在种册窗的“册”属性页中不应该存在";
                }
            }

            return null;
        }


        // 检查 refids 中的参考 ID 是否都存在
        // parameters:
        //      bExist  验证方向。如果 == true，表示要验证这些册存在；== false，表示要验证这些册不存在
        static string VerifyItems(EntityForm entity_form,
            List<string> refids,
            bool bExist)
        {
            if (refids == null)
                return null;
            foreach (string refid in refids)
            {
                BookItemBase item = entity_form.EntityControl.Items.GetItemByRefID(refid);
                // TODO: 如果 item 找到，要进一步验证其 refid 成员是否符合查找参数
                if (bExist == true)
                {
                    if (item == null)
                        return "参考 ID 为 '" + refid + "' 的册记录事项在种册窗的“册”属性页中没有找到";
                }
                else
                {
                    if (item != null)
                        return "参考 ID 为 '" + refid + "' 的册记录事项在种册窗的“册”属性页中不应该存在";
                }
            }

            return null;
        }

        // 所创建的书目记录和下属册记录信息。用于最后核对验证
        class BiblioCreationInfo
        {
            public string BiblioRecPath { get; set; }
            public List<string> ItemRefIDs { get; set; }
            public List<ObjectCreationInfo> Objects { get; set; }
        }

        // parameters:
        //      strStyle    create_objects  表示要创建对象文件
        BiblioCreationInfo CreateBiblioRecord(string strBiblioDbName,
            string strTitle,
            string strStyle)
        {
            // string strError = "";

            EntityForm entity_form = new EntityForm();
            try
            {
                entity_form.MainForm = Program.MainForm;
                entity_form.MdiParent = Program.MainForm;
                entity_form.Show();

                MarcRecord record = new MarcRecord();
                record.add(new MarcField('$', "200  $a" + strTitle));
                record.add(new MarcField('$', "690  $aI247.5"));
                record.add(new MarcField('$', "701  $a测试著者"));

                // 将 MARC 机内格式记录赋予窗口
                entity_form.MarcEditor.Marc = record.Text;
                entity_form.BiblioRecPath = strBiblioDbName + "/?";
                entity_form.InitialPages();
                entity_form.ActivateItemsPage();

                // 保存
                if (entity_form.DoSaveAll("") == -1)
                    throw new Exception("种册窗保存记录时出错");

                string strBiblioRecPath = entity_form.BiblioRecPath;

                // 创建册记录
                List<string> refids = CreateEntityRecords(entity_form, 10);

                List<ObjectCreationInfo> objects = null;
                if (StringUtil.IsInList("create_objects", strStyle))
                {
                    // 为种册窗添加若干对象文件
                    objects = AddObjects(entity_form, strTitle, 10);
                }


                // 保存
                if (entity_form.DoSaveAll("") == -1)
                    throw new Exception("种册窗保存记录时出错");

                BiblioCreationInfo info = new BiblioCreationInfo();
                info.BiblioRecPath = entity_form.BiblioRecPath;
                info.ItemRefIDs = refids;
                info.Objects = objects;

                return info;
            }
            finally
            {
                entity_form.Close();
            }
        }

        #endregion

        // 测试编译所有统计方案
        private void ToolStripMenuItem_compileAllProjects_Click(object sender, EventArgs e)
        {
            CompileStatisProjects();
        }

        void CompileStatisProjects()
        {
            string strError = "";
            int nCompileCount = 0;
            int nRet = 0;

            //bool bHideMessageBox = false;
            //bool bDontUpdate = false;

            List<Type> types = new List<Type>();
            types.Add(typeof(Iso2709StatisForm));
            types.Add(typeof(OperLogStatisForm));
            types.Add(typeof(ReaderStatisForm));
            types.Add(typeof(ItemStatisForm));
            types.Add(typeof(OrderStatisForm));
            types.Add(typeof(BiblioStatisForm));
            types.Add(typeof(XmlStatisForm));
            // types.Add(typeof(PrintOrderForm));

            foreach (Type type in types)
            {
                var form = (Form)Activator.CreateInstance(type);
                form.MdiParent = Program.MainForm;
                // form.WindowState = FormWindowState.Minimized;
                form.Show();

                Application.DoEvents();

                try
                {
                    // return:
                    //      -2  全部放弃
                    //      -1  出错
                    //      >=0 更新数
                    nRet = CompileProjects(form,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;
                    nCompileCount += nRet;
                }
                finally
                {
                    form.Close();
                }
            }

            // 凭条打印
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = CompileProjects(Program.MainForm.OperHistory,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nCompileCount += nRet;
            }

            // MainForm
            {
                // return:
                //      -2  全部放弃
                //      -1  出错
                //      >=0 更新数
                nRet = CompileProjects(Program.MainForm,
                        out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
                nCompileCount += nRet;
            }

            if (nCompileCount > 0)
                MessageBox.Show(this, "共编译了 " + nCompileCount.ToString() + " 个方案");
            else
                MessageBox.Show(this, "没有编译任何方案");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 更新一个窗口拥有的全部方案
        // parameters:
        //      strSource   "!url"或者磁盘目录。分别表示从网络检查更新，或者从磁盘检查更新
        // return:
        //      -2  全部放弃
        //      -1  出错
        //      >=0 更新数
        int CompileProjects(
            object form,
            out string strError)
        {
            strError = "";
            int nCompileCount = 0;

            dynamic o = form;

            o.EnableControls(false);
            try
            {
                List<string> names = o.ScriptManager.GetAllProjectNames(out strError);
                if (names == null)
                    return -1;

                foreach (string name in names)
                {
                    o.TestCompile(name);
                    nCompileCount++;
                }
            }
            finally
            {
                o.EnableControls(true);
            }
            return nCompileCount;
        }

        static string[] _prices = new string[] {
            "CNY|12.00",
            "^CNY|12.00.00",
            "人民币|12.00|元",
        };

        // 测试 PriceUtil.ParsePriceUnit
        private void ToolStripMenuItem_parsePriceUnit_Click(object sender, EventArgs e)
        {
            foreach (string s in _prices)
            {
                string strText = s;

                bool bError = false;
                if (strText[0] == '^')
                {
                    bError = true;
                    strText = strText.Substring(1);
                }

                string strSamplePrefix = "";
                string strSampleValue = "";
                string strSamplePostfix = "";
                string strError = "";

                string[] parts = s.Split(new char[] { '|' });
                if (parts.Length > 0)
                    strSamplePrefix = parts[0];
                if (parts.Length > 1)
                    strSampleValue = parts[1];
                if (parts.Length > 2)
                    strSamplePostfix = parts[2];

                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";

                strText = strText.Replace("|", "");
                // 分析价格参数
                // 允许前面出现+ -号
                // return:
                //      -1  出错
                //      0   成功
                int nRet = PriceUtil.ParsePriceUnit(strText,
            out strPrefix,
            out strValue,
            out strPostfix,
            out strError);

                if (bError)
                {
                    if (nRet != -1)
                    {
                        MessageBox.Show(this, "样例字符串 '" + strText + "' 应该返回 -1，但返回了 " + nRet);
                    }
                }
                else
                {
                    List<string> errors = new List<string>();
                    if (strPrefix != strSamplePrefix)
                        errors.Add("解析出的前缀应该为 '" + strSamplePrefix + "'，但却是 '" + strPrefix + "'");
                    if (strValue != strSampleValue)
                        errors.Add("解析出的值应该为 '" + strSampleValue + "'，但却是 '" + strValue + "'");
                    if (strPostfix != strSamplePostfix)
                        errors.Add("解析出的后缀应该为 '" + strSamplePostfix + "'，但却是 '" + strPostfix + "'");

                    if (errors.Count > 0)
                        MessageBox.Show(this, "样例字符串 '" + strText + "' 解析结果不符合预期。" + StringUtil.MakePathList(errors));
                }


            }

            MessageBox.Show(this, "OK");
        }

        private void ToolStripMenuItem_objectWriteRead_Click(object sender, EventArgs e)
        {
            TestObjectWriteRead("");
        }

        void TestObjectWriteRead(string strStyle)
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));
            try
            {
                string strTestDbName = "";
                string strOutputInfo = "";

                {
                    strTestDbName = "_测试用盘点库";
                    Progress.SetMessage("正在删除" + strTestDbName + " ...");
                    lRet = channel.ManageDatabase(
        stop,
        "delete",
        strTestDbName,    // strDatabaseNames,
        "",
        "",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                            goto ERROR1;
                    }

                    Progress.SetMessage("正在创建" + strTestDbName + " ...");

                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateSimpleDatabase(
                        channel,
                        this.Progress,
                        strTestDbName,
                        "inventory",
                        "",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }


                {
                    List<string> errors = TestWriteXmlRecords(
stop,
channel,
strTestDbName,
"delete", // "delete,fix",
1,
100);
                    if (errors.Count > 0)
                    {
                        strError = "测试过程发生问题：" + StringUtil.MakePathList(errors);
                        goto ERROR1;
                    }
                }

                // 进行测试
                {
                    List<string> errors = TestUploadObjectFiles(
                stop,
                channel,
                strTestDbName,
                "delete,fix", // "delete,fix",
                1,
                1);
                    if (errors.Count > 0)
                    {
                        strError = "测试过程发生问题：" + StringUtil.MakePathList(errors);
                        goto ERROR1;
                    }
                }

                {
                    List<string> errors = TestUploadObjectFiles(
    stop,
    channel,
    strTestDbName,
    "delete", // "delete",
    100 * 1024,
    100);
                    if (errors.Count > 0)
                    {
                        strError = "测试过程发生问题：" + StringUtil.MakePathList(errors);
                        goto ERROR1;
                    }
                }

                {
                    List<string> errors = TestUploadObjectFiles(
    stop,
    channel,
    strTestDbName,
    "delete,reuse_id", // "delete",
    100 * 1024,
    100);
                    if (errors.Count > 0)
                    {
                        strError = "测试过程发生问题：" + StringUtil.MakePathList(errors);
                        goto ERROR1;
                    }
                }


                if (string.IsNullOrEmpty(strTestDbName) == false)
                {
                    Progress.SetMessage("正在删除" + strTestDbName + " ...");
                    lRet = channel.ManageDatabase(
        stop,
        "delete",
        strTestDbName,    // strDatabaseNames,
        "",
        "",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                return;
            }
            catch (Exception ex)
            {
                strError = "TestObjectWriteRead() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
        }

        // parameters:
        //      strStyle    fix 固定样例
        //                  delete  创建的记录后面要自动删除
        //                  reuse_id    重复使用记录 ID，这样可以测试覆盖重用 ID 写入记录的情况
        List<string> TestUploadObjectFiles(
            Stop stop,
            LibraryChannel channel,
            string strDbName,
            string strStyle,
            int nChunkSize,
            int nObjectCount)
        {
            string strError = "";

            Random random = new Random();

            // fix 表示固定样例。即不是随机的样例。便于跟踪调试排错
            bool bFix = StringUtil.IsInList("fix", strStyle);
            bool bReuseID = StringUtil.IsInList("reuse_id", strStyle);

            List<string> errors = new List<string>();

            string strRecordPath = strDbName + "/?";
            byte[] record_timestamp = null;

            // 先写入一条元数据记录
            {
                int length = random.Next(0, 100 * 1024);
                string strXml = BuildXmlString(nObjectCount, length);
                string strWriteStyle = "content,data";
                string strOutputPath = "";
                long lRet = channel.WriteRes(stop,
                    strRecordPath,
                    strXml,
                    false,
                    strWriteStyle,
                    null,   // timestamp,
                    out record_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    errors.Add("创建记录 '" + strRecordPath + "' 时出错: " + strError);
                    return errors;
                }
                strRecordPath = strOutputPath;
            }


            string strTempDir = Program.MainForm.UserTempDir;

            for (int i = 0; i < nObjectCount; i++)
            {
                if (stop != null && stop.State == 1)
                {
                    errors.Add("用户中断");
                    return errors;
                }

                int delta = random.Next(0, 1024);
                if (bFix)
                    delta = 10;    // 固定算法
                string strClientFilePath = "";

                // 创建一个临时的本地对象文件
                strClientFilePath = Path.Combine(strTempDir, "~" + Guid.NewGuid().ToString());
                CreateObjectFile(strClientFilePath, i * 1024 + delta, strStyle);

                string strObjectPath = strRecordPath + "/object/" + i;
                if (bReuseID)
                    strObjectPath = strRecordPath + "/object/0";

                string strOutputFileName = Path.Combine(strTempDir, "~" + Guid.NewGuid().ToString());

                try
                {
                    channel.UploadResChunkSize = nChunkSize;

                    byte[] temp_timestamp = null;
                    int nRet = channel.UploadObject(
                stop,
                strClientFilePath,
                strObjectPath,
                "", // strStyle,
                null,   // timestamp,
                true,
                out temp_timestamp,
                out strError);
                    if (nRet == -1)
                    {
                        errors.Add("上传对象 '" + strObjectPath + "' 时出错: " + strError);
                        return errors;
                        //continue;
                    }

                    // TODO: 要测试用多种不同尺寸的 chunksize 来下载
                    // 下载
                    string strMetadata = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";
                    long lRet = channel.GetRes(stop,
                        strObjectPath,
                        strOutputFileName,
                        "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
                        out strMetadata,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        errors.Add("下载对象 '" + strObjectPath + "' 时出错:" + strError);
                        return errors;
                        //continue;
                    }

                    // 检查 metadata 中的 localfilepath 和 mime
                    string strMime = PathUtil.MimeTypeFrom(strClientFilePath);
                    List<string> temp_errors = VerifyMetadata(strMetadata,
            Path.GetFileName(strClientFilePath),
            strMime);
                    if (temp_errors.Count > 0)
                    {
                        errors.AddRange(temp_errors);
                        return errors;
                    }

                    nRet = CompareFile(strClientFilePath,
                        strOutputFileName,
                        out strError);
                    if (nRet == -1)
                    {
                        errors.Add("比较针对 '" + strObjectPath + "' 上传和下载的两个文件时出错:" + strError);
                        return errors;
                        //continue;
                    }
                    if (nRet != 0)
                    {
                        errors.Add("比较针对 '" + strObjectPath + "' 上传和下载的两个文件时出错: 两个文件内容不一致: " + strError);
                        return errors;
                        //continue;
                    }

                    if (StringUtil.IsInList("delete", strStyle)
                        && bReuseID == false)
                    {
                        // 删除对象
                        byte[] output_timestamp = null;
                        lRet = channel.WriteRes(stop,
                            strObjectPath,
                            "",
                            0,
                            null,
                            "",
                            "delete",
                            timestamp,
                            out strOutputPath,
                            out output_timestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            errors.Add("删除 '" + strObjectPath + "' 时出错:" + strError);
                            continue;
                        }
                    }
                }
                finally
                {
                    File.Delete(strClientFilePath);
                    File.Delete(strOutputFileName);
                }
            }

            // 删除元数据记录
            if (StringUtil.IsInList("delete", strStyle))
            {
                // 删除对象
                byte[] output_timestamp = null;
                string strOutputPath = "";
                long lRet = channel.WriteRes(stop,
                    strRecordPath,
                    "",
                    0,
                    null,
                    "",
                    "delete",
                    record_timestamp,
                    out strOutputPath,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    errors.Add("删除元数据记录 '" + strRecordPath + "' 时出错:" + strError);
                    return errors;
                }
            }

            return errors;
        }

        List<string> TestWriteXmlRecords(
    Stop stop,
    LibraryChannel channel,
    string strDbName,
    string strStyle,
    int nChunkSize,
    int nObjectCount)
        {
            string strError = "";

            Random random = new Random();

            // fix 表示固定样例。即不是随机的样例。便于跟踪调试排错
            bool bFix = StringUtil.IsInList("fix", strStyle);
            bool bReuseID = StringUtil.IsInList("reuse_id", strStyle);

            List<string> errors = new List<string>();

            byte[] timestamp = null;

            // string strTempDir = Program.MainForm.UserTempDir;
            string strRecordPath = strDbName + "/?";

            for (int i = 0; i < nObjectCount; i++)
            {
                if (stop != null && stop.State == 1)
                {
                    errors.Add("用户中断");
                    return errors;
                }

                int delta = random.Next(0, 1024);
                if (bFix)
                    delta = 10;    // 固定算法

                //    channel.UploadResChunkSize = nChunkSize;

                int length = i * 1024 + delta;
                string strXml = BuildXmlString(0, length);
                string strWriteStyle = "content,data";
                string strOutputPath = "";
                //if (bFix)
                //    strRecordPath = strDbName + "/0";

                byte[] output_timestamp = null;
                long lRet = channel.WriteRes(stop,
                    strRecordPath,
                    strXml,
                    false,
                    strWriteStyle,
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    errors.Add("创建记录 '" + strRecordPath + "' 时出错: " + strError);
                    return errors;
                }

                strRecordPath = strOutputPath;
                timestamp = output_timestamp;

                stop.SetMessage("正在测试写入 XML 记录 " + strRecordPath + " length=" + length);

                // TODO: 要测试用多种不同尺寸的 chunksize 来下载
                // 下载
                string strGetStyle = "content,data,metadata,timestamp,outputpath";
                string strMetadata = "";

                string strResult = "";
                lRet = channel.GetRes(stop,
                    strRecordPath,
                    strGetStyle,
                    out strResult,
                    out strMetadata,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    errors.Add("下载 XML 记录 '" + strRecordPath + "' 时出错:" + strError);
                    return errors;
                    continue;
                }

#if NO
                    // 检查 metadata 中的 localfilepath 和 mime
                    string strMime = PathUtil.MimeTypeFrom(strClientFilePath);
                    List<string> temp_errors = VerifyMetadata(strMetadata,
            Path.GetFileName(strClientFilePath),
            strMime);
                    if (temp_errors.Count > 0)
                    {
                        errors.AddRange(temp_errors);
                        return errors;
                    }
#endif

                if (strResult != strXml)
                    errors.Add("返回的 XML 和预期的不一致");

                if (StringUtil.IsInList("delete", strStyle)
                    && bReuseID == false)
                {
                    // 删除对象
                    // byte[] output_timestamp = null;
                    lRet = channel.WriteRes(stop,
                        strRecordPath,
                        "",
                        0,
                        null,
                        "",
                        "delete",
                        timestamp,
                        out strOutputPath,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        errors.Add("删除 '" + strRecordPath + "' 时出错:" + strError);
                        continue;
                    }
                }
            }

#if NO
            // 删除元数据记录
            if (StringUtil.IsInList("delete", strStyle))
            {
                // 删除对象
                byte[] output_timestamp = null;
                string strOutputPath = "";
                long lRet = channel.WriteRes(stop,
                    strRecordPath,
                    "",
                    0,
                    null,
                    "",
                    "delete",
                    timestamp,
                    out strOutputPath,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    errors.Add("删除 XML 记录 '" + strRecordPath + "' 时出错:" + strError);
                    return errors;
                }
            }
#endif

            return errors;
        }


        static List<string> VerifyMetadata(string strMetadata,
            string strLocalFileName,
            string strMime)
        {
            List<string> errors = new List<string>();
            string strError = "";
            Hashtable table = StringUtil.ParseMetaDataXml(strMetadata,
    out strError);
            if (table == null)
            {
                errors.Add("解析 metadata 时出错: " + strError);
                return errors;
            }

            string localfilepath = (string)table["localpath"];
            if (localfilepath != strLocalFileName)
                errors.Add("metadata 中 localpath ‘" + localfilepath + "’ 和期望值 '" + strLocalFileName + "' 不吻合");

            string mime = (string)table["mimetype"];
            if (mime != strMime)
                errors.Add("metadata 中 mimetype '" + mime + "' 和期望值 '" + strMime + "' 不吻合");

            return errors;
        }

        static string MakeRandomText(int length)
        {
            Random random = new Random();
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char ch = (char)random.Next('A', 'z');
                text.Append(ch);
            }

            return text.ToString();
        }

        // 创造一个包含若干 dprms:file 元素的元数据记录
        // parameters:
        //      length  XML 记录的内容长度
        static string BuildXmlString(int object_count, int length)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            for (int i = 0; i < object_count; i++)
            {
                XmlElement new_file = dom.CreateElement("dprms", "file", DpNs.dprms);
                dom.DocumentElement.AppendChild(new_file);
                new_file.SetAttribute("id", i.ToString());
            }

            {
                XmlElement text = dom.CreateElement("text");
                dom.DocumentElement.AppendChild(text);
                text.InnerText = MakeRandomText(length);
            }

            return dom.OuterXml;
        }

        // 逐字节比较两个文件的异同
        // return:
        //      -1  出错
        //      0   两个文件内容完全相同
        //      1   两个文件内容不同
        static int CompareFile(
            string strFileName1,
            string strFileName2,
            out string strError)
        {
            strError = "";
            try
            {
                using (FileStream stream1 = File.OpenRead(strFileName1))
                using (FileStream stream2 = File.OpenRead(strFileName2))
                {
                    if (stream1.Length != stream2.Length)
                    {
                        strError = "两个文件长度不同。(" + stream1.Length + " 和 " + stream2.Length + ")";
                        return 1;
                    }

                    // 准备从流中读出

                    stream1.Seek(0, SeekOrigin.Begin);
                    stream2.Seek(0, SeekOrigin.Begin);
                    while (true)
                    {
                        int nRet = stream1.ReadByte();
                        if (nRet == -1)
                            return 0;

                        int nRet2 = stream2.ReadByte();
                        if (nRet != nRet2)
                        {
                            strError = "偏移 " + (stream1.Position - 1) + " 处两个 byte 值不同。(" + nRet + " 和 " + nRet2 + ")";
                            return 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "比较过程出现异常: " + ex.Message;
                return -1;
            }
        }

        static void CreateObjectFile(string strFileName,
            long length,
            string strStyle)
        {

            // fix 表示固定样例。即不是随机的样例。便于跟踪调试排错
            bool bFix = StringUtil.IsInList("fix", strStyle);

            Random random = new Random();
            using (FileStream stream = File.Create(strFileName))
            {
                for (long i = 0; i < length; i++)
                {
                    char ch = (char)random.Next(0, char.MaxValue);
                    if (bFix)
                        ch = (char)(i % 256);  // 固定算法
                    stream.WriteByte((byte)ch);
                }
            }
        }

        private void ToolStripMenuItem_logAndRecover_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                //LogAndRecover("createDatabase", "type:*");
                LogAndRecover("deleteBiblioDatabase", "");
                //LogAndRecover("deleteSimpleDatabase", "type:*");   // type:* type:amerce
            });
        }

        #region 日志和恢复

        void LogAndRecover(string strAction,
            string strStyle)
        {
            string strError = "";
            int nRet = 0;

            // TODO: 对话框警告，测试过程可能会导致一些单个数据库无法改回原名，需要用 dp2manager 改名善后。不要在生产环境用这个测试功能。会创建一些测试用的操作日志并无法删除

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始进行日志和恢复的测试</div>");

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));
            try
            {
                if (strAction == "createDatabase")
                {
                    nRet = TestRecoverCreatingDatabases(
this.Progress,
channel,
strStyle,
out strError);
                }
                else if (strAction == "deleteBiblioDatabase")
                {
                    nRet = TestRecoverDeletingBiblioDatabases(
this.Progress,
channel,
strStyle,
out strError);
                }
                else if (strAction == "deleteSimpleDatabase")
                {
                    nRet = TestRecoverDeletingSimpleDatabases(
this.Progress,
channel,
strStyle,
out strError);
                }
                else
                {
                    strError = "无法识别的 strAction '" + strAction + "'";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;
                return;
            }
            catch (Exception ex)
            {
                strError = "LogAndRecord() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束日志和恢复的测试</div>");
            }
            ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.DisplayError(strError);
            this.ShowMessage(strError, "red", true);
        }

        // 测试恢复创建数据库。各种数据库类型
        // parameters:
        //      strStyle    type:xxx
        int TestRecoverCreatingDatabases(
Stop stop,
LibraryChannel channel,
string strStyle,
out string strError)
        {
            strError = "";
            int nRet = 0;

            string strType = StringUtil.GetParameterByPrefix(strStyle, "type", ":");
            if (string.IsNullOrEmpty(strType) == true)
            {
                strError = "strStyle 中没有包含任何 type:xxx 内容";
                return -1;
            }

            if (strType == "*")
            {
                strType = "reader|biblio|arrived|amerce|message|pinyin|gcat|word|publisher|inventory|dictionary|zhongcihao"; // invoice 暂时没有支持
                // strType = "publisher"; // invoice 暂时没有支持
            }

            List<string> types = StringUtil.SplitList(strType, "|");
            foreach (string type in types)
            {
                if (type == "biblio")
                {
                    nRet = TestRecoverCreatingOneBiblioDatabase(
stop,
channel,
out strError);
                    if (nRet == -1)
                        return -1;
                    continue;
                }
                string strCurrentStyle = StringUtil.SetParameterByPrefix(strStyle,
                    "type",
                    ":",
                    type);
                if (type == "zhongcihao" || type == "biblio" || type == "reader")
                    strCurrentStyle += ",dont_protect";
                nRet = TestRecoverCreatingOneDatabase(
            stop,
            channel,
            strCurrentStyle,
            out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        static string[] _roles = new string[] { "catalogTarget",
                "catalogWork",
                "orderRecommendStore",
                "biblioSource",
                "orderWork" };

        // 构造出用于测试 复制书目记录 功能的全部可用 style 字符串
        static List<string> Build_roles_styleCombination()
        {
            List<string> styles = new List<string>();
            int length = _roles.Length;

            List<int> base_array = new List<int>();
            for (int i = 0; i < length; i++)
            {
                base_array.Add(i);
            }
            List<List<int>> result = Permutation.GetPermuation(base_array);

            foreach (IEnumerable<int> indices in result)
            {
                List<string> list = new List<string>();
                foreach (int i in indices)
                {
                    list.Add(_roles[i]);
                }

                styles.Add(StringUtil.MakePathList(list));
            }

            return styles;
        }


        int TestRecoverCreatingOneBiblioDatabase(
    Stop stop,
    LibraryChannel channel,
    out string strError)
        {
            strError = "";

            string[] usages = new string[] { "book", "series" };
            string[] syntaxs = new string[] { "unimarc", "usmarc" };
            string[] subtypes = new string[] { "*", "",
                "entity",
                "entity|order",
                "entity|order|issue",
                "entity|order|issue|comment",
                "order",
                "order|issue",
                "order|issue|comment",
                "issue",
                "issue|comment",
                "comment",
            };

            // 单独测试一下 role 和 unioncatalogstyle 的组合情况
            List<string> roles_list = Build_roles_styleCombination();
            foreach (string roles in roles_list)
            {
                string strCurrentStyle = "type:biblio,usage:book,syntax:unimarc,role:" + roles.Replace(",", "|") + ",subtype:entity|order|issue|comment,dont_protect";

                int nRet = TestRecoverCreatingOneDatabase(
stop,
channel,
strCurrentStyle,
out strError);
                if (nRet == -1)
                    return -1;
            }

            foreach (string usage in usages)
            {
                foreach (string syntax in syntaxs)
                {
                    foreach (string subtype in subtypes)
                    {
                        string strCurrentStyle = "type:biblio,usage:" + usage + ",syntax:" + syntax + ",subtype:" + subtype
                            + ",dont_protect";

                        int nRet = TestRecoverCreatingOneDatabase(
        stop,
        channel,
        strCurrentStyle,
        out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

            return 0;
        }

        // parameters:
        //      strStyle    usage:xxx syntax:xxx dont_protect type:xxx role:xxx
        int TestRecoverCreatingOneDatabase(
            Stop stop,
            LibraryChannel channel,
            string strStyle,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            bool bProtect = !StringUtil.IsInList("dont_protect", strStyle);

            string strType = StringUtil.GetParameterByPrefix(strStyle, "type", ":");
            if (string.IsNullOrEmpty(strType) == true)
            {
                strError = "strStyle 中没有包含任何 type:xxx 内容";
                return -1;
            }

            if (bProtect == true && strType == "biblio")
            {
                strError = "书目库无法进行保护，请在 strStyle 中加上 'dont_protect,' 参数";
                return -1;
            }

            string strUsage = StringUtil.GetParameterByPrefix(strStyle, "usage", ":");
            if (string.IsNullOrEmpty(strUsage) == true)
                strUsage = "book";

            string strRole = StringUtil.GetParameterByPrefix(strStyle, "role", ":");

            string strSyntax = StringUtil.GetParameterByPrefix(strStyle, "syntax", ":");
            if (string.IsNullOrEmpty(strSyntax) == true)
                strSyntax = "unimarc";

            string strSubType = StringUtil.GetParameterByPrefix(strStyle, "subtype", ":");
            if (string.IsNullOrEmpty(strSubType) == true)
                strSubType = "";    // 完全可能创建没有任何下级数据库的书目库

            DisplayTitle("=== 创建一个 " + strType + " 类型的数据库 ===");

            string strSingleDbName = "test_" + strType;

            // *** 预先准备阶段
            // 如果测试用的书目库以前就存在，要先保护起来

            // 得到当前此类型的库名
            string strOldDbName = "";
            string strDetachOldDbName = "";

            string strOutputInfo = "";
            if (bProtect)
            {
                nRet = ManageHelper.GetDatabaseInfo(
            stop,
            channel,
            out List<ManageHelper.DatabaseInfo> infos,
            out strError);
                if (nRet == -1)
                    return -1;
                List<DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo> results =
                    infos.FindAll((DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo info) =>
                    {
                        if (info.Type == strType) return true;
                        return false;
                    });
                if (results.Count > 0)
                {
                    strOldDbName = results[0].Name;

                    // 如果当前存在的此类型数据库正好和临时数据库重名，应认为是上次测试残留的结果，直接删除即可
                    if (strSingleDbName == strOldDbName
                        && String.IsNullOrEmpty(strSingleDbName) == false)
                    {
                        DisplayTitle("因当前已经存在的同类数据库名正好和测试用临时文件名重复，所以改为不用保护它");

                        /*
                        DisplayTitle("拟用于测试的数据库名已经存在，需要删除残留的数据库 '" + strSingleDbName + "'");
                        lRet = channel.ManageDatabase(
                            this.Progress,
            "delete",
            strSingleDbName,    // strDatabaseNames,
            "",
            "skipOperLog",
            out strOutputInfo,
            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ErrorCode.NotFound)
                                DisplayOK(strError);
                            else
                                goto ERROR1;
                        }
                         * */
                        strOldDbName = "";  // 旧数据库这时就不存在了
                    }
                    else
                        strDetachOldDbName = "_saved_" + strOldDbName;
                }
            }
            else
                DisplayTitle("不保护以前的同类数据库。因为此类数据库是可以重复创建的");

            {
                // 删除遗留的临时库
                {
                    DisplayTitle("尝试删除以前遗留的数据库 '" + strSingleDbName + "'");

                    lRet = channel.ManageDatabase(
                        this.Progress,
        "delete",
        strSingleDbName,    // strDatabaseNames,
        "",
        "skipOperLog",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.NotFound)
                            DisplayOK(strError);
                        else
                            goto ERROR1;
                    }
                }
            }

            if (string.IsNullOrEmpty(strOldDbName) == false)
            {
                DisplayTitle("保护当前的同类数据库 '" + strOldDbName + "' --> '" + strDetachOldDbName + "'");

                // 修改一个简单库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有找到源数据库定义
                //      1   成功修改
                nRet = ManageHelper.ChangeSimpleDatabase(
            channel,
            stop,
            strOldDbName,
            strType,
            strDetachOldDbName,
            "detach",
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    strOldDbName = "";
            }

            try
            {

#if NO
            // string strBiblioDbName = "_测试用中文图书";

            Progress.SetMessage("正在删除以前残留的测试用" + strType + "库 ...");
            if (strType == "biblio")
            {
                DisplayTitle("尝试删除以前残留的测试用书目库");
                // 如果测试用的书目库以前就存在，要先删除。删除前最好警告一下
                lRet = channel.ManageDatabase(
                    this.Progress,
    "delete",
    strSingleDbName,    // strDatabaseNames,
    "",
    "skipOperLog",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();
            }
            else
            {
                DisplayTitle("尝试删除以前残留的测试用" + strType + "库 '" + strSingleDbName + "'");
                lRet = channel.ManageDatabase(
    this.Progress,
"delete",
strSingleDbName,    // strDatabaseNames,
"",
"skipOperLog,verify",
out strOutputInfo,
out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
#endif

                // 记载当前操作日志偏移
                DateTime now = DateTime.Now;
                string strDate = DateTimeUtil.DateTimeToString8(now);

                // return:
                //      -2  此类型的日志在 dp2library 端尚未启用
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                lRet = OperLogLoader.GetOperLogCount(
                    this.Progress,
                    channel,
                    strDate,
                    LogType.OperLog,
                    out strError);
                if (lRet <= -1)
                {
                    goto ERROR1;
                }

                long lStartOffs = lRet; // 日志起点偏移
                string strRequestXml = "";
                {


                    if (strType == "biblio")
                    {
                        // TODO: 图书/连续出版物 MARC格式 都应该组合探索
                        DisplayTitle("创建关键日志动作：创建数据库 '" + strSingleDbName + "' usage=" + strUsage + " syntax=" + strSyntax + " subtype=" + strSubType + "。创建动作带有校验功能");
                        // 创建一个书目库
                        // parameters:
                        // return:
                        //      -1  出错
                        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                        //      1   成功创建
                        nRet = ManageHelper.CreateBiblioDatabase(
                            channel,
                            this.Progress,
                            strSingleDbName,
                            strUsage,    // "book",
                            strRole?.Replace("|", ","),
                            strSyntax,  // "unimarc",
                            strSubType?.Replace("|", ","),
                            "verify",
                            out strRequestXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strType == "reader")
                    {
                        DisplayTitle("创建关键日志动作：创建数据库 '" + strSingleDbName + "'。创建动作带有校验功能");
                        nRet = ManageHelper.CreateReaderDatabase(channel,
            this.Progress,
            strSingleDbName,
            "_测试分馆", // strLibraryCode,
            true, // bInCirculation,
            "verify",
            out strRequestXml,
            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        DisplayTitle("创建关键日志动作：创建数据库 '" + strSingleDbName + "'。创建动作带有校验功能");
                        nRet = ManageHelper.CreateSimpleDatabase(
        channel,
        this.Progress,
        strSingleDbName,
        strType,
        "verify",
        out strRequestXml,
        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 重新获得各种库名、列表
                    ReloadDatabaseCfg();
                }

                // TODO: 此处即可进行一次验证
                {
                    DisplayTitle("验证数据库 '" + strSingleDbName + "' 是否确实被成功创建了");

                    // 验证数据库操作的结果状态
                    nRet = VerifyDatabaseState(this.Progress,
                        channel,
                        strSingleDbName + "#" + strType,
                        "create",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // TODO: 要验证数据库创建后在 library.xml 中的参数是否完全正确；验证数据库 dp2kernel 的全部配置文件内容和数量是否和操作日志中完全吻合

                    // 比较创建数据库的操作日志，和当前 dp2library 一端的实际数据库和配置文件状态是否符合
                    // return:
                    //      -1  出错
                    //      0   比较发现完全一致
                    //      1   比较中发现(至少一处)不一致
                    nRet = CompareLogOfCreatingDatabase(
                        this.Progress,
                        channel,
                        strDate,
                        lStartOffs,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        goto ERROR1;
                }

                // *** 恢复测试
                // 从指定偏移位置启动 dp2library 日志恢复后台任务
                DisplayTitle("启动 日志恢复 dp2library后台任务");
                string strTaskName = "日志恢复";
                nRet = StartBatchTask(
                    this.Progress,
                    channel,
                    strTaskName,
                    strDate,
                    lStartOffs,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                DisplayTitle("等待后台任务结束 ...");
                // 等待，直到 dp2library 后台任务结束
                string strErrorInfo = "";
                nRet = WaitBatchTaskFinish(
                    this.Progress,
            channel,
            strTaskName,
            out strErrorInfo,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (string.IsNullOrEmpty(strErrorInfo) == false)
                {
                    strError = "在等待后台任务 '" + strTaskName + "' 结束的过程中发生错误: " + strErrorInfo;
                    goto ERROR1;
                }

                // 验证恢复情况
                {
                    DisplayTitle("验证数据库 '" + strSingleDbName + "' 是否确实被成功创建了");

                    // 验证数据库操作的结果状态
                    nRet = VerifyDatabaseState(this.Progress,
                        channel,
                        strSingleDbName + "#" + strType,
                        "create",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // TODO: 要验证数据库创建后在 library.xml 中的参数是否完全正确；验证数据库 dp2kernel 的全部配置文件内容和数量是否和操作日志中完全吻合

                    // 比较创建数据库的操作日志，和当前 dp2library 一端的实际数据库和配置文件状态是否符合
                    // return:
                    //      -1  出错
                    //      0   比较发现完全一致
                    //      1   比较中发现(至少一处)不一致
                    nRet = CompareLogOfCreatingDatabase(
                        this.Progress,
                        channel,
                        strDate,
                        lStartOffs,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        goto ERROR1;
                }

                // 清除残留信息
                {
                    // 删除测试用的书目库
                    DisplayTitle("正在删除测试用的" + strType + "库 '" + strSingleDbName + "'");
                    lRet = channel.ManageDatabase(
                        this.Progress,
        "delete",
        strSingleDbName,    // strDatabaseNames,
        "",
        "skipOperLog",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    // 重新获得各种库名、列表
                    ReloadDatabaseCfg();
                }
            }
            finally
            {
                // 还原最初的数据库

                if (string.IsNullOrEmpty(strOldDbName) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strDetachOldDbName) == false, "");

                    DisplayTitle("(测试结束。还原最初保存的数据库 '" + strDetachOldDbName + "' --> '" + strOldDbName + "')");
                    string strError1 = "";
                    // 修改一个简单库
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有找到源数据库定义
                    //      1   成功修改
                    nRet = ManageHelper.ChangeSimpleDatabase(
                channel,
                stop,
                strDetachOldDbName,
                strType,
                strOldDbName,
                "attach",
                out strError1);
                    if (nRet == -1)
                    {
                        string strText = "还原时出错: " + strError1;
                        DisplayError(strText);
                        // 如何报错?
                        this.Invoke((Action)(() => MessageBox.Show(this, strText)));
                    }
                }
            }

            return 0;
            ERROR1:
            return -1;
        }

        // 比较删除数据库的操作日志，和当前 dp2library 一端的实际数据库和配置文件状态是否符合
        // return:
        //      -1  出错
        //      0   比较发现完全一致
        //      1   比较中发现(至少一处)不一致
        int CompareLogOfDeletingDatabase(
            Stop stop,
            LibraryChannel channel,
            string strDate,
            long lOffset,
            out string strError)
        {
            strError = "";

            string strTempDir = Path.Combine(Program.MainForm.UserTempDir, "~comp_db");
            PathUtil.CreateDirIfNeed(strTempDir);

            string strAttachmentFileName = Program.MainForm.GetTempFileName("comp");

            try
            {

                string strXml = "";
                // 获得指定位置的一条操作日志

                // return:
                //      -2  日志记录不存在
                //      -1  出错
                //      >=0 附件文件的长度
                long lRet = ManageHelper.GetOneOperLog(
                    channel,
                    stop,
                    strDate,
                    lOffset,
                    strAttachmentFileName,
                    out strXml,
                    out strError);
                if (lRet == -1 || lRet == -2)
                    return -1;
                if (lRet == 0)
                {
                    strError = "日志文件 '" + strDate + "' 偏移 '" + lOffset + "' 的日志记录中没有附件部分";
                    return -1;
                }

                // 根据 .zip 定义文件，比较当前数据库状态
                // return:
                //      -1  出错
                //      0   比较发现完全一致
                //      1   比较中发现(至少一处)不一致
                return ManageHelper.CompareDefinition(
         stop,
         channel,
         strAttachmentFileName,
         strTempDir,
         "deleting",
         out strError);
            }
            finally
            {
                if (string.IsNullOrEmpty(strTempDir) == false)
                {
                    PathUtil.RemoveReadOnlyAttr(strTempDir);    // 避免 .zip 文件中有有只读文件妨碍删除
                    PathUtil.DeleteDirectory(strTempDir);
                }

                File.Delete(strAttachmentFileName);
            }
        }

        // 比较创建数据库的操作日志，和当前 dp2library 一端的实际数据库和配置文件状态是否符合
        // return:
        //      -1  出错
        //      0   比较发现完全一致
        //      1   比较中发现(至少一处)不一致
        int CompareLogOfCreatingDatabase(
            Stop stop,
            LibraryChannel channel,
            string strDate,
            long lOffset,
            out string strError)
        {
            strError = "";

            string strTempDir = Path.Combine(Program.MainForm.UserTempDir, "~comp_db");
            PathUtil.CreateDirIfNeed(strTempDir);

            string strAttachmentFileName = Program.MainForm.GetTempFileName("comp");

            try
            {

                string strXml = "";
                // 获得指定位置的一条操作日志

                // return:
                //      -2  日志记录不存在
                //      -1  出错
                //      >=0 附件文件的长度
                long lRet = ManageHelper.GetOneOperLog(
                    channel,
                    stop,
                    strDate,
                    lOffset,
                    strAttachmentFileName,
                    out strXml,
                    out strError);
                if (lRet == -1 || lRet == -2)
                    return -1;

                List<string> dbnames = null;
                // 根据日志记录 XML，比较当前数据库状态
                // parameters:
                //      strFunction creating/deleting 之一
                //      dbnames 顺便从日志 XML 中提取数据库名返回。注意，当函数返回值不是 0 时，列表中内容可能不全
                // return:
                //      -1  出错
                //      0   比较发现完全一致
                //      1   比较中发现(至少一处)不一致
                int nRet = ManageHelper.CompareOperLog(stop,
             channel,
             strXml,
             "creating",
             out dbnames,
             out strError);
                if (nRet != 0)
                    return -1;

                // 根据 .zip 定义文件，比较当前数据库状态
                // return:
                //      -1  出错
                //      0   比较发现完全一致
                //      1   比较中发现(至少一处)不一致
                return ManageHelper.CompareDefinition(
         stop,
         channel,
         strAttachmentFileName,
         strTempDir,
         "creating",
         out strError);
            }
            finally
            {
                if (string.IsNullOrEmpty(strTempDir) == false)
                {
                    PathUtil.RemoveReadOnlyAttr(strTempDir);    // 避免 .zip 文件中有有只读文件妨碍删除
                    PathUtil.DeleteDirectory(strTempDir);
                }

                File.Delete(strAttachmentFileName);
            }
        }

        int TestRecoverDeletingBiblioDatabases(
Stop stop,
LibraryChannel channel,
string strStyle,
out string strError)
        {
            strError = "";

            string[] usages = new string[] { "book", "series" };
            string[] syntaxs = new string[] { "unimarc", "usmarc" };
            string[] subtypes = new string[] { "*", "",
                "entity",
                "entity|order",
                "entity|order|issue",
                "entity|order|issue|comment",
                "order",
                "order|issue",
                "order|issue|comment",
                "issue",
                "issue|comment",
                "comment",
            };

            // 单独测试一下 role 的不同
            List<string> roles_list = Build_roles_styleCombination();
            foreach (string roles in roles_list)
            {
                string strCurrentStyle = "type:biblio,usage:book,syntax:unimarc,role:" + roles.Replace(",", "|") + ",subtype:entity|order|issue|comment,dont_protect";

                int nRet = TestRecoverDeletingOneBiblioDatabase(
stop,
channel,
strCurrentStyle,
out strError);
                if (nRet == -1)
                    return -1;
            }

            // usage syntax subtype 的组合
            foreach (string usage in usages)
            {
                foreach (string syntax in syntaxs)
                {
                    foreach (string subtype in subtypes)
                    {
                        string strCurrentStyle = "type:biblio,usage:" + usage + ",syntax:" + syntax + ",subtype:" + subtype
                            + ",dont_protect";

                        int nRet = TestRecoverDeletingOneBiblioDatabase(
        stop,
        channel,
        strCurrentStyle,
        out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

#if NO

            ////

            string strType = StringUtil.GetParameterByPrefix(strStyle, "type", ":");
            if (string.IsNullOrEmpty(strType) == true)
            {
                strError = "strStyle 中没有包含任何 type:xxx 内容";
                return -1;
            }

            if (strType == "*")
            {
                strType = "arrived|amerce|message|pinyin|gcat|word|publisher|inventory|dictionary|zhongcihao"; // invoice 暂时没有支持
                // strType = "publisher"; // invoice 暂时没有支持
            }

            List<string> types = StringUtil.SplitList(strType, "|");
            foreach (string type in types)
            {
                string strCurrentStyle = StringUtil.SetParameterByPrefix(strStyle,
                    "type",
                    ":",
                    type);
                if (type == "zhongcihao")
                    strCurrentStyle += ",dont_protect";
                nRet = TestRecoverDeletingOneBiblioDatabase(
            stop,
            channel,
            strCurrentStyle,
            out strError);
                if (nRet == -1)
                    return -1;
            }
#endif
            return 0;
        }

        // 测试恢复删除 书目数据库 (注：“恢复删除”的意思是“重做删除动作”)
        // TODO: 要增加测试删除一个书目库下个别下属库的功能
        // 要测试两种情况:1) 恢复操作前，数据库是存在的。这是正常情况; 2) 恢复操作前，数据库并不存在
        int TestRecoverDeletingOneBiblioDatabase(
            Stop stop,
            LibraryChannel channel,
            string strStyle,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            DisplayTitle("=== 删除一个书目库 ===");

            string strBiblioDbName = "_测试用中文图书";

            // *** 预先准备阶段
            // 如果测试用的书目库以前就存在，要先删除。删除前最好警告一下
            DisplayTitle("正在尝试删除以前残留的测试用书目库 ...");
            long lRet = channel.ManageDatabase(
                this.Progress,
"delete",
strBiblioDbName,    // strDatabaseNames,
"",
"skipOperLog",
out string strOutputInfo,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                    goto ERROR1;
            }

            string strUsage = StringUtil.GetParameterByPrefix(strStyle, "usage", ":");
            if (string.IsNullOrEmpty(strUsage) == true)
                strUsage = "book";

            string strRole = StringUtil.GetParameterByPrefix(strStyle, "role", ":");

            string strSyntax = StringUtil.GetParameterByPrefix(strStyle, "syntax", ":");
            if (string.IsNullOrEmpty(strSyntax) == true)
                strSyntax = "unimarc";

            string strSubType = StringUtil.GetParameterByPrefix(strStyle, "subtype", ":");
            if (string.IsNullOrEmpty(strSubType) == true)
                strSubType = "*";    // 完全可能创建没有任何下级数据库的书目库

            string strStyleCaption = "usage=" + strUsage + " syntax=" + strSyntax + " subtype=" + strSubType + " role=" + strRole;

            // 创建一个书目库，以便后面进行删除操作
            {
                DisplayTitle("创建一个书目库 '" + strBiblioDbName + "'，以前后面能模拟出测试动作(删除) " + strStyleCaption);

                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    this.Progress,
                    strBiblioDbName,
                    strUsage,
                    strRole?.Replace("|", ","),
                    strSyntax,
                    strSubType?.Replace("|", ","),
                    "skipOperLog,verify",
                    out string strRequestXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();
            }

            // TODO: 记载当前操作日志偏移
            DateTime now = DateTime.Now;
            string strDate = DateTimeUtil.DateTimeToString8(now);

            // return:
            //      -2  此类型的日志在 dp2library 端尚未启用
            //      -1  出错
            //      0   日志文件不存在，或者记录数为 0
            //      >0  记录数
            lRet = OperLogLoader.GetOperLogCount(
                this.Progress,
                channel,
                strDate,
                LogType.OperLog,
                out strError);
            if (lRet <= -1)
            {
                goto ERROR1;
            }

            long lStartOffs = lRet; // 日志起点偏移

            // 进行删除操作，并记入日志
            {
                DisplayTitle("创建关键日志动作：删除数据库 '" + strBiblioDbName + "'。创建动作带有校验功能");

                lRet = channel.ManageDatabase(
                    this.Progress,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    "verify",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }

            if (StringUtil.IsInList("db_not_exist", strStyle))
            {
                // 在恢复操作前的环境中，不具有这个数据库
            }
            else
            {
                DisplayTitle("准备进行恢复模拟。先创建临时书目库 '" + strBiblioDbName + "' 以作为恢复的准备条件 " + strStyleCaption);

                // 创建书目库，以便在恢复操作前的环境中，先具有这个数据库
                // 创建一个书目库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                //      1   成功创建
                nRet = ManageHelper.CreateBiblioDatabase(
                    channel,
                    this.Progress,
                    strBiblioDbName,
                    strUsage,
                    strRole?.Replace("|", ","),
                    strSyntax,
                    strSubType?.Replace("|", ","),
                    "skipOperLog,verify",
                    out string strRequestXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // *** 恢复测试
            // 从指定偏移位置启动 dp2library 日志恢复后台任务
            string strTaskName = "日志恢复";
            nRet = StartBatchTask(
                this.Progress,
                channel,
                strTaskName,
                strDate,
                lStartOffs,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 等待，直到 dp2library 后台任务结束
            string strErrorInfo = "";
            nRet = WaitBatchTaskFinish(
                this.Progress,
        channel,
        strTaskName,
        out strErrorInfo,
        out strError);
            if (nRet == -1)
                goto ERROR1;
            if (string.IsNullOrEmpty(strErrorInfo) == false)
            {
                strError = "在等待后台任务 '" + strTaskName + "' 结束的过程中发生错误: " + strErrorInfo;
                goto ERROR1;
            }

            // 验证删除情况
            {
                DisplayTitle("验证数据库 '" + strBiblioDbName + "' 是否确实被删除");

                // 验证数据库操作的结果状态
                nRet = VerifyDatabaseState(this.Progress,
                    channel,
                    strBiblioDbName + "#" + "biblio",
                    "delete",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   比较发现完全一致
                //      1   比较中发现(至少一处)不一致
                nRet = CompareLogOfDeletingDatabase(
                    this.Progress,
                    channel,
                    strDate,
                    lStartOffs,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    goto ERROR1;
            }

            return 0;
            ERROR1:
            return -1;
        }

        // 测试恢复动作：删除各种类型的简单数据库
        // 这里的简单数据库，是指书目库和下属库以外的所有类型的数据库。把这些类型集中在一起编写测试代码，是为了在测试外围 detach 和 attach
        int TestRecoverDeletingSimpleDatabases(
    Stop stop,
    LibraryChannel channel,
    string strStyle,
    out string strError)
        {
            strError = "";
            int nRet = 0;
            //long lRet = 0;

            string strType = StringUtil.GetParameterByPrefix(strStyle, "type", ":");
            if (string.IsNullOrEmpty(strType) == true)
            {
                strError = "strStyle 中没有包含任何 type:xxx 内容";
                return -1;
            }

            if (strType == "*")
            {
                strType = "arrived|amerce|message|pinyin|gcat|word|publisher|inventory|dictionary|zhongcihao"; // invoice 暂时没有支持
                // strType = "publisher"; // invoice 暂时没有支持
            }

            List<string> types = StringUtil.SplitList(strType, "|");
            foreach (string type in types)
            {
                string strCurrentStyle = StringUtil.SetParameterByPrefix(strStyle,
                    "type",
                    ":",
                    type);
                if (type == "zhongcihao")
                    strCurrentStyle += ",dont_protect";
                nRet = TestRecoverDeletingOneSimpleDatabase(
            stop,
            channel,
            strCurrentStyle,
            out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // TODO: 测试过程要显示在 OperHistory 中
        // 测试恢复删除 一个特定类型的 简单库
        // 要测试两种情况:1) 恢复操作前，数据库是存在的。这是正常情况; 2) 恢复操作前，数据库并不存在
        // parameters:
        //      strStyle    type:amerce 等等
        //                  dont_protect 表示不做 detach attach 保护。缺省为要保护
        int TestRecoverDeletingOneSimpleDatabase(
            Stop stop,
            LibraryChannel channel,
            string strStyle,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            DisplayTitle("=== 删除一个简单库 ===");

            bool bProtect = !StringUtil.IsInList("dont_protect", strStyle);

            string strType = StringUtil.GetParameterByPrefix(strStyle, "type", ":");
            if (string.IsNullOrEmpty(strType) == true)
            {
                strError = "strStyle 中没有包含任何 type:xxx 内容";
                return -1;
            }

            string strSingleDbName = "test_" + strType;

            DisplayTitle("数据库类型 '" + strType + "', 用于测试的数据库名为 '" + strSingleDbName + "'");

            // *** 预先准备阶段
            // 如果测试用的书目库以前就存在，要先保护起来
            Progress.SetMessage("正在保护测试前的单个库 ...");

            // 得到当前此类型的简单库名
            string strOldDbName = "";
            string strDetachOldDbName = "";

            if (bProtect)
            {
                List<DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo> infos = null;
                nRet = ManageHelper.GetDatabaseInfo(
            stop,
            channel,
            out infos,
            out strError);
                if (nRet == -1)
                    return -1;
                List<DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo> results =
                    infos.FindAll((DigitalPlatform.CirculationClient.ManageHelper.DatabaseInfo info) =>
                    {
                        if (info.Type == strType) return true;
                        return false;
                    });
                if (results.Count > 0)
                {
                    strOldDbName = results[0].Name;

                    // 如果当前存在的此类型数据库正好和临时数据库重名，应认为是上次测试残留的结果，直接删除即可
                    if (strSingleDbName == strOldDbName
                        && String.IsNullOrEmpty(strSingleDbName) == false)
                    {
                        DisplayTitle("拟用于测试的数据库名已经存在，需要删除残留的数据库 '" + strSingleDbName + "'");

                        string strOutputInfo = "";
                        lRet = channel.ManageDatabase(
                            this.Progress,
            "delete",
            strSingleDbName,    // strDatabaseNames,
            "",
            "skipOperLog",
            out strOutputInfo,
            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ErrorCode.NotFound)
                                DisplayOK(strError);
                            else
                                goto ERROR1;
                        }
                        strOldDbName = "";  // 旧数据库这时就不存在了
                    }
                    else
                        strDetachOldDbName = "_saved_" + strOldDbName;
                }
            }
            else
            {
                DisplayTitle("不保护以前的同类数据库。因为此类数据库是可以重复创建的");
                // 删除遗留的临时库
                {
                    DisplayTitle("尝试删除以前遗留的数据库 '" + strSingleDbName + "'");

                    string strOutputInfo = "";
                    lRet = channel.ManageDatabase(
                        this.Progress,
        "delete",
        strSingleDbName,    // strDatabaseNames,
        "",
        "skipOperLog",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.NotFound)
                            DisplayOK(strError);
                        else
                            goto ERROR1;
                    }
                }
            }

            if (string.IsNullOrEmpty(strOldDbName) == false)
            {
                DisplayTitle("保护当前的同类数据库 '" + strOldDbName + "' --> '" + strDetachOldDbName + "'");

                // 修改一个简单库
                // parameters:
                // return:
                //      -1  出错
                //      0   没有找到源数据库定义
                //      1   成功修改
                nRet = ManageHelper.ChangeSimpleDatabase(
            channel,
            stop,
            strOldDbName,
            strType,
            strDetachOldDbName,
            "detach",
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    strOldDbName = "";
            }

            try
            {
                string strOutputInfo = "";

                // 创建一个简单库，以便后面进行删除操作
                {
                    DisplayTitle("创建一个临时的简单库 '" + strSingleDbName + "'，以前后面能模拟出测试动作(删除)");

                    Progress.SetMessage("正在创建测试用简单库 ...");
                    // 创建一个书目库
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateSimpleDatabase(
                        channel,
                        this.Progress,
                        strSingleDbName,
                        strType,
                        "skipOperLog",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 重新获得各种库名、列表
                    ReloadDatabaseCfg();
                }

                // TODO: 记载当前操作日志偏移
                DateTime now = DateTime.Now;
                string strDate = DateTimeUtil.DateTimeToString8(now);

                // return:
                //      -2  此类型的日志在 dp2library 端尚未启用
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                lRet = OperLogLoader.GetOperLogCount(
                    this.Progress,
                    channel,
                    strDate,
                    LogType.OperLog,
                    out strError);
                if (lRet <= -1)
                {
                    goto ERROR1;
                }

                long lStartOffs = lRet; // 日志起点偏移

                // 进行删除操作，并记入日志
                {
                    DisplayTitle("创建关键日志动作：删除数据库 '" + strSingleDbName + "'。创建动作带有校验功能");

                    lRet = channel.ManageDatabase(
                        this.Progress,
        "delete",
        strSingleDbName,    // strDatabaseNames,
        "",
        "verify",
        out strOutputInfo,
        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                if (StringUtil.IsInList("db_not_exist", strStyle))
                {
                    // 在恢复操作前的环境中，不具有这个数据库
                }
                else
                {
                    DisplayTitle("准备进行恢复模拟。先创建临时数据库 '" + strSingleDbName + "' 以作为恢复的准备条件");
                    // 创建简单库，以便在恢复操作前的环境中，先具有这个数据库
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                    //      1   成功创建
                    nRet = ManageHelper.CreateSimpleDatabase(
                        channel,
                        this.Progress,
                        strSingleDbName,
                        strType,
                        "skipOperLog,verify",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                DisplayTitle("启动 日志恢复 dp2library后台任务");
                // *** 恢复测试
                // 从指定偏移位置启动 dp2library 日志恢复后台任务
                string strTaskName = "日志恢复";
                nRet = StartBatchTask(
                    this.Progress,
                    channel,
                    strTaskName,
                    strDate,
                    lStartOffs,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                DisplayTitle("等待后台任务结束 ...");
                // 等待，直到 dp2library 后台任务结束
                string strErrorInfo = "";
                nRet = WaitBatchTaskFinish(
                    this.Progress,
            channel,
            strTaskName,
            out strErrorInfo,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (string.IsNullOrEmpty(strErrorInfo) == false)
                {
                    strError = "在等待后台任务 '" + strTaskName + "' 结束的过程中发生错误: " + strErrorInfo;
                    goto ERROR1;
                }

                // 验证删除情况
                {
                    DisplayTitle("验证数据库 '" + strSingleDbName + "' 是否确实被删除");

                    // 验证数据库操作的结果状态
                    nRet = VerifyDatabaseState(this.Progress,
                        channel,
                        strSingleDbName + "#" + strType,
                        "delete",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // return:
                    //      -1  出错
                    //      0   比较发现完全一致
                    //      1   比较中发现(至少一处)不一致
                    nRet = CompareLogOfDeletingDatabase(
                        this.Progress,
                        channel,
                        strDate,
                        lStartOffs,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        goto ERROR1;
                }

            }
            finally
            {
                // 还原最初的数据库

                if (string.IsNullOrEmpty(strOldDbName) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strDetachOldDbName) == false, "");

                    DisplayTitle("(测试结束。还原最初保存的数据库 '" + strDetachOldDbName + "' --> '" + strOldDbName + "')");
                    string strError1 = "";
                    // 修改一个简单库
                    // parameters:
                    // return:
                    //      -1  出错
                    //      0   没有找到源数据库定义
                    //      1   成功修改
                    nRet = ManageHelper.ChangeSimpleDatabase(
                channel,
                stop,
                strDetachOldDbName,
                strType,
                strOldDbName,
                "attach",
                out strError1);
                    if (nRet == -1)
                    {
                        string strText = "还原时出错: " + strError1;
                        DisplayError(strText);
                        // 如何报错?
                        this.Invoke((Action)(() => MessageBox.Show(this, strText)));
                    }
                }
            }

            return 0;
            ERROR1:
            return -1;
        }

        // 验证恢复“创建数据库”情况
        /*
         * 1) 验证每个 dp2kernel 数据库是否已经创建。方法是获取关键的几个配置文件看看
         * 2) 往 dp2kernel 数据库中写入记录，然后尝试读回
         * 3) 验证 library.xml 中 itemdbgroup 里面的 database 元素是否正确
         * */

        // 验证恢复“删除数据库”情况
        /*
         * 1) 验证每个 dp2kernel 数据库是否已经删除。方法是获取关键的几个配置文件看看
         * 2) 如果是书目库，验证 library.xml 中 itemdbgroup 里面的 database 元素是否正确删除或者修改了
         *      如果是其他单个数据库，验证 library.xml 中相应位置是否兑现了修改
         * */
        int VerifyDeletingDatabase(Stop stop,
            LibraryChannel channel,
            string strDbType,
            string strDbName,
            out string strError)
        {
            strError = "";

            return 0;
        }

        static int StartBatchTask(
            Stop stop,
            LibraryChannel channel,
            string strTaskName,
            string strDate,
            long lStartOffs,
            out string strError)
        {
            strError = "";

            BatchTaskInfo param = new BatchTaskInfo();
            if (param.StartInfo == null)
                param.StartInfo = new BatchTaskStartInfo();
            param.StartInfo.Start = lStartOffs.ToString() + "@" + strDate;
            param.StartInfo.Param = "<root style='verify' />";  // 执行的时候自动进行校验

            BatchTaskInfo resultInfo = null;

            // return:
            //      -1  出错
            //      0   启动成功
            //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
            long lRet = channel.BatchTask(
                stop,
                strTaskName,
                "start",
                param,
                out resultInfo,
                out strError);
            if (lRet == -1 || lRet == 1)
                return -1;

            return 0;
        }

        static List<string> GetErrors(string strResult)
        {
            List<string> results = new List<string>();
            List<string> lines = StringUtil.SplitList(strResult, "\r\n");
            foreach (string line in lines)
            {
                if (line.IndexOf("{error}") != -1)
                    results.Add(line);
            }

            return results;
        }

        // 获得后台任务执行过程中累积的出错信息
        // return:
        //      -1   出错
        //      0   正常
        static int GetBatchTaskErrorResult(Stop stop,
            LibraryChannel channel,
            string strTaskName,
            out List<string> errors,
            out string strError)
        {
            string strResult = "";
            string strErrorInfo = "";
            errors = new List<string>();

            int nRet = GetBatchTaskResult(stop,
            channel,
            strTaskName,
            out strResult,
            out strErrorInfo,
            out strError);
            if (nRet == -1)
                return -1;
            errors = GetErrors(strResult);
            if (string.IsNullOrEmpty(strErrorInfo) == false)
                errors.Add(strErrorInfo);
            return 0;
        }

        // 验证数据库操作的结果状态
        // parameters:
        //      strDatabaseNameList 数据库名列表。"预约到书#arrived,中文图书,#biblio"
        int VerifyDatabaseState(Stop stop,
            LibraryChannel channel,
            string strDatabaseNameList,
            string strFunction,
            out string strError)
        {
            strError = "";

            long lRet = channel.ManageDatabase(
stop,
"getinfo",
strDatabaseNameList,    // strDatabaseNames,
"",
"verify:" + strFunction,
out string strOutputInfo,
out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // 获得后台任务执行过程中累积的结果信息
        // return:
        //      -1   出错
        //      0   批处理任务尚未完全结束
        //      1   批处理任务已经结束
        static int GetBatchTaskResult(Stop stop,
            LibraryChannel channel,
            string strTaskName,
            out string strResult,
            out string strErrorInfo,
            out string strError)
        {
            strError = "";
            strResult = "";
            strErrorInfo = "";

            long currentResultOffs = 0;

            Decoder resultTextDecoder = Encoding.UTF8.GetDecoder();

            for (int i = 0; ; i++)
            {
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }

                BatchTaskInfo param = new BatchTaskInfo();
                BatchTaskInfo resultInfo = null;

                // param.MaxResultBytes = 0;

                param.MaxResultBytes = 4096;
                if (i >= 5)  // 如果发现尚未来得及获取的内容太多，就及时扩大“窗口”尺寸
                    param.MaxResultBytes = 100 * 1024;

                param.ResultOffset = currentResultOffs;

                stop.SetMessage("正在获取任务 '" + strTaskName + "' 的最新信息 (第 " + (i + 1).ToString() + " 批)...");

                long lRet = channel.BatchTask(
                    stop,
                    strTaskName,
                    "getinfo",
                    param,
                    out resultInfo,
                    out strError);
                if (lRet == -1)
                    return -1;

                strResult += GetResultText(resultTextDecoder, resultInfo.ResultText);

                // 存储用于下次
                currentResultOffs = resultInfo.ResultOffset;

                // 如果本次并没有“触底”，需要立即循环获取新的信息。但是循环有一个最大次数，以应对服务器疯狂发生信息的情形。
                if (resultInfo.ResultOffset >= resultInfo.ResultTotalLength)
                {
                    List<string> parts = StringUtil.ParseTwoPart(resultInfo.ProgressText, ":");
                    if (parts[0] == "批处理任务结束")
                    {
                        strErrorInfo = parts[1];
                        return 1;
                    }

                    return 0;
                }
            }
        }

        static string GetResultText(Decoder decoder, byte[] baResult)
        {
            if (baResult == null)
                return "";
            if (baResult.Length == 0)
                return "";

            // Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder;
            char[] chars = new char[baResult.Length];

            int nCharCount = decoder.GetChars(
                baResult,
                    0,
                    baResult.Length,
                    chars,
                    0);
            Debug.Assert(nCharCount <= baResult.Length, "");

            return new string(chars, 0, nCharCount);
        }

        // 等待后台批处理任务结束
        // parameters:
        //      strErrorInfo    返回后台任务出错信息。后台任务启动阶段的报错会在这里出现，但执行阶段的报错不在这里
        //      strError        返回本函数的出错信息
        static int WaitBatchTaskFinish(
            Stop stop,
            LibraryChannel channel,
            string strTaskName,
            out string strErrorInfo,
            out string strError)
        {
            strError = "";
            strErrorInfo = "";

            BatchTaskInfo param = new BatchTaskInfo();
            BatchTaskInfo resultInfo = null;

            param.MaxResultBytes = 0;
            param.ResultOffset = -1;

            while (true)
            {
                long lRet = channel.BatchTask(
        stop,
        strTaskName,
        "getinfo",
        param,
        out resultInfo,
        out strError);
                if (lRet == -1)
                    return -1;

                List<string> parts = StringUtil.ParseTwoPart(resultInfo.ProgressText, ":");
                if (parts[0] == "批处理任务结束")
                {
                    strErrorInfo = parts[1];
                    {
                        // 获得后台任务执行过程中累积的出错信息
                        List<string> errors = null;
                        // return:
                        //      -1   出错
                        //      0   正常
                        int nRet = GetBatchTaskErrorResult(stop,
                            channel,
                            strTaskName,
                            out errors,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (errors.Count > 0)
                        {
                            strError = StringUtil.MakePathList(errors, "\r\n");
                            return -1;
                        }
                    }
                    return 1;
                }

                Thread.Sleep(1000);
            }
        }

        #endregion

        void DisplayTitle(string strText)
        {
            Progress.SetMessage(strText);
            Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(strText) + "</div>");
        }
        void DisplayOK(string strText)
        {
            Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(strText) + "</div>");
        }
        void DisplayError(string strText)
        {
            Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strText) + "</div>");
        }

        // 测试获取 PDF 文件单页
        private void ToolStripMenuItem_getPdfSinglePage_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                GetPdfSinglePage();
            });
            Task.Factory.StartNew(() =>
            {
                GetPdfSinglePage();
            });
        }

        void GetPdfSinglePage()
        {
            DownloadFile("中文图书/612/object/0/page:1,format:png,dpi:300");
        }

        // 一次函数运行中，连续做十次下载
        void DownloadFile(string strServerFilePath)
        {
            string strError = "";

            List<string> filenames = new List<string>();

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));

            try
            {
                string strPrevFileName = "";
                for (int i = 0; i < 10; i++)
                {
                    string strLocalFilePath = Program.MainForm.GetTempFileName("test");
                    Progress.SetMessage(strServerFilePath + "-->" + strLocalFilePath);
                    // parameters:
                    //		strOutputFileName	输出文件名。可以为null。如果调用前文件已经存在, 会被覆盖。
                    // return:
                    //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                    //		0	成功
                    long lRet = this.Channel.GetRes(
                        this.stop,
                        strServerFilePath,
                        strLocalFilePath,
                        "content,data,metadata,timestamp,outputpath,gzip",
                        out string strMetaData,
                        out byte[] baOutputTimeStamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    filenames.Add(strLocalFilePath);

                    // TODO: 将文件内容和标准文件进行比较
                    if (string.IsNullOrEmpty(strPrevFileName) == false)
                    {
                        int nRet = CompareFile(strLocalFilePath, strPrevFileName, out strError);
                        if (nRet != 0)
                            goto ERROR1;
                    }

                    strPrevFileName = strLocalFilePath;

                    // 根据返回的时间戳设置文件最后修改时间
                    // FileUtil.SetFileLastWriteTimeByTimestamp(strLocalFilePath, baOutputTimeStamp);
                }

                // 最后删除测试用的本地文件
                foreach (string filename in filenames)
                {
                    File.Delete(filename);
                }
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 测试 GCAT 通用汉语著者号码表
        private void ToolStripMenuItem_testGCAT_Click(object sender, EventArgs e)
        {
            string strError = "";

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的专用测试 XML 文件名";
            // dlg.FileName = this.RecPathFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "测试文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            dom.Load(dlg.FileName);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行测试 " + dlg.FileName + "</div>");

            List<string> skips = new List<string>();
            List<string> errors = new List<string>();
            try
            {
                string strGcatWebServiceUrl = Program.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
                int i = 0;
                foreach (XmlElement item in nodes)
                {
                    if (Progress != null && Progress.IsStopped)
                    {
                        strError = "中断";
                        break;
                    }
                    string strAuthor = item.GetAttribute("author");
                    string strPinyin = item.GetAttribute("pinyin");
                    string strRecPath = item.GetAttribute("recPath");
                    string strNumber = item.GetAttribute("number");
                    string strQuestions = item.GetAttribute("questions");

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode((i + 1).ToString() + " " + strAuthor + " " + strPinyin) + "</div>");

                    Hashtable question_table = new Hashtable();

                    if (string.IsNullOrEmpty(strQuestions) == false)
                    {
                        Question[] questions = JsonConvert.DeserializeObject<Question[]>(strQuestions);
                        question_table[strAuthor] = questions;
                    }

                    // return:
                    //      -4  著者字符串没有检索命中
                    //      -2  strID验证失败
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = BiblioItemsHost.GetAuthorNumber(
                    ref question_table,
                    Progress,
                    this,
                    strGcatWebServiceUrl,
                    strAuthor,
                    strPinyin,
                    true,   // bSelectPinyin,
                    true, // bSelectEntry,
                    true, // bOutputDebugInfo,
                    out string strOutputNumber,
                    out string strDebugInfo,
                    out strError);
                    if (nRet == -1)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"著者 {strAuthor} 取著者号过程出错: {strError}") + "</div>");
                        continue;
                        // goto ERROR1;
                    }
                    if (nRet == 0)
                    {
                        strError = "放弃选择答案";
                        break;
                    }

                    // 把 Questions 保存起来
                    if (question_table.Count > 0)
                    {
                        string value = "";
                        foreach (string key in question_table.Keys)
                        {
                            Question[] questions = (Question[])question_table[key];
                            value = JsonConvert.SerializeObject(questions);
                            break;
                        }

                        item.SetAttribute("questions", value);
                    }
                    else
                        item.RemoveAttribute("questions");

                    if (strNumber != strOutputNumber)
                    {
                        // 输出到操作历史
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"著者 {strAuthor} 产生的著者号 {strOutputNumber} 和期望的著者号 {strNumber} 不一致") + "</div>");

                        // 为节省精力，只显示首个字母相同(其他字符不同)的情况
                        if (string.IsNullOrEmpty(strOutputNumber) == false
                            && string.IsNullOrEmpty(strNumber) == false
                            && strOutputNumber.Substring(0, 1) == strNumber.Substring(0, 1))
                            errors.Add($" '{strAuthor}'({strPinyin}) --> '{strOutputNumber}'，和 '{strNumber}' 不一致。记录路径 {strRecPath}");
                        else
                            skips.Add($" '{strAuthor}'({strPinyin}) --> '{strOutputNumber}'，和 '{strNumber}' 不一致。记录路径 {strRecPath}");
                    }
                    else
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode($"著者 {strAuthor} 产生的著者号 {strOutputNumber} 和期望的著者号 {strNumber} 一致") + "</div>");

                    i++;
                }

                dom.Save(dlg.FileName);
                return;
            }
            finally
            {
                if (errors.Count > 0)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode($"=== 共发现 {errors.Count} 个问题 ===") + "</div>");
                    int i = 1;
                    foreach (string error in errors)
                    {
                        Program.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode($"{i++}) {error}") + "</div>");
                    }
                }

                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行测试 " + dlg.FileName + "</div>");
            }

            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void ToolStripMenuItem_borrowAndReturn_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                TestBorrowAndReturn("", "R0000001", "0000001");
            });

            Task.Factory.StartNew(() =>
            {
                TestBorrowAndReturn("", "R0000002", "0000002");
            });
        }

        int TestChangeReaderRecord(LibraryChannel channel,
            string strReaderBarcode,
            out string strError)
        {
            strError = "";

            long lRet = channel.GetReaderInfo(stop,
                strReaderBarcode,
                "xml",
                out string[] results,
                out string strRecPath,
                out byte[] timestamp,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return -1;

            string strXml = results[0];

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            string comment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            if (comment == null)
                comment = "";
            if (comment.Length > 200 * 1024)
            {
                comment = "";
                DisplayError($"comment 已经清除");
            }

            DomUtil.SetElementText(dom.DocumentElement, "comment", comment + new string('c', 1000));

            string strNewXml = dom.DocumentElement.OuterXml;

            lRet = channel.SetReaderInfo(stop,
    "change",
    strRecPath,
    strNewXml,
    strXml,
    timestamp,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedRecPath,
    out byte[] new_timestamp,
    out ErrorCodeValue kernel_errorcode,
    out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // parameters:
        //      strStyle    风格
        void TestBorrowAndReturn(string strStyle,
            string strReaderBarcode,
            string strItemBarcode)
        {
            string strError = "";
            int nRet = 0;

            LibraryChannel channel = this.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在进行测试 ...");
            Progress.BeginLoop();

            this.Invoke((Action)(() =>
                EnableControls(false)
                ));
            try
            {
                Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始进行密集借书还书测试</div>");

                for (; ; )
                {
                    if (stop?.State != 0)
                        break;

                    long lRet = 0;

                    // 如果测试用的书目库以前就存在，要先删除。删除前最好警告一下
                    Progress.SetMessage("正在借 ...");
                    {
                        lRet = channel.Borrow(
                            stop,
                            false,
                            strReaderBarcode,
                            strItemBarcode,
                            "", // strConfirmItemRecPath,
                            false,
                            null,   // saBorrowedItemBarcode,
                            "", // strStyle,
                            "", // strItemFormatList,
                            out string[] item_records,
                            "", // strReaderFormatList,
                            out string[] reader_records,
                            "", // strBiblioFormatList,
                            out string[] biblio_records,
                            out string[] aDupPath,
                            out string strOutputReaderBarcode,
                            out BorrowInfo borrow_info,
                            out strError);
                    }
                    if (lRet == -1)
                    {
                        //if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        //    goto ERROR1;
                        DisplayError($"读者 {strReaderBarcode} 借书 {strItemBarcode} 失败: {strError}");
                    }
                    else
                        DisplayOK($"读者 {strReaderBarcode} 借书 {strItemBarcode} 成功");

                    {
                        nRet = TestChangeReaderRecord(channel,
                            strReaderBarcode,
                            out strError);
                        if (nRet == -1)
                            DisplayError($"读者 {strReaderBarcode} 记录修改失败: {strError}");
                        else
                            DisplayOK($"读者 {strReaderBarcode} 记录修改成功");

                    }

                    Progress.SetMessage("正在还 ...");
                    {
                        lRet = channel.Return(
                            stop,
                            "return",
                            strReaderBarcode,
                            strItemBarcode,
                            null,   // strConfirmItemRecPath,
                            false,
                            "", // strStyle,
                            "", // strItemFormatList,
                            out string[] item_records,
                            "", // strReaderFormatList,
                            out string[] reader_records,
                            "", // strBiblioFormatList,
                            out string[] biblio_records,
                            out string[] aDupPath,
                            out string strOutputReaderBarcode,
                            out ReturnInfo return_info,
                            out strError);
                    }

                    if (lRet == -1)
                        DisplayError($"读者 {strReaderBarcode} 还书 {strItemBarcode} 失败: {strError}");
                    else
                        DisplayOK($"读者 {strReaderBarcode} 还书 {strItemBarcode} 成功");

                }
                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束密集借书还书测试</div>");
                return;
            }
            catch (Exception ex)
            {
                strError = "TestBorrowAndReturn() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                this.Invoke((Action)(() =>
                    EnableControls(true)
                    ));

                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
            ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
        }

    }

    // 验证异常
    public class VerifyException : Exception
    {

        public VerifyException(string s)
            : base(s)
        {
        }

    }

    public static class Permutation
    {
        public static List<List<int>> GetPermuation(List<int> base_array)
        {
            List<List<int>> results = new List<List<int>>();

            List<List<int>> prev = new List<List<int>>();   // 上一步骤，上一长度的所有组合
            prev.AddRange(GetNextArray(base_array, new List<int>()));
            results.AddRange(prev);
            for (int i = 0; i < base_array.Count; i++)
            {
                List<List<int>> next = new List<List<int>>();   // 下一长度的所有组合

                foreach (List<int> one in prev)
                {
                    List<List<int>> temp = GetNextArray(base_array, one);

                    AddResults(results, temp);
                    // results.AddRange(temp);

                    AddResults(next, temp);
                    // next.AddRange(temp);
                }

                prev = next;
            }

            return results;
        }

        static void AddResults(List<List<int>> results, List<List<int>> result)
        {
            foreach (List<int> current in result)
            {
                AddResult(results, current);
            }
        }

        // 将 result 内部元素排序之后，加入 results 数组。加入以前要查重，重复的不会加入
        static void AddResult(List<List<int>> results, List<int> result)
        {
            // result.Sort();
            if (IsContained(results, result) == true)
                return;
            results.Add(result);
        }

        static bool IsContained(List<List<int>> results, List<int> result)
        {
            foreach (List<int> current in results)
            {
                if (IsEqual(current, result) == true)
                    return true;
            }
            return false;
        }

        static bool IsEqual(List<int> result1, List<int> result2)
        {
            if (result1.Count != result2.Count)
                return false;
            for (int i = 0; i < result1.Count; i++)
            {
                if (result1[i] != result2[i])
                    return false;
            }
            return true;
        }

        static List<List<int>> GetNextArray(List<int> base_array, List<int> source_array)
        {
            List<List<int>> results = new List<List<int>>();

            // 多一个数字的所有可能
            foreach (int v in base_array)
            {
                if (source_array.IndexOf(v) != -1)
                    continue;
                List<int> result = new List<int>(source_array);
                result.Add(v);

                result.Sort();

                results.Add(result);
            }

            return results;
        }
    }
}
