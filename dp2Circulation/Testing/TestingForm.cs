using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System.Xml;
using DigitalPlatform.Xml;
using System.Collections;
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
            Task.Factory.StartNew(() =>
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
            int nRet = 0;
            // 获得书目数据库From信息
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
            string strError = "";

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


                ///
                if (string.IsNullOrEmpty(strTestDbName) == false)
                {
                    Progress.SetMessage("正在删除" + strTestDbName + " ...");
                    lRet = channel.ManageDatabase(
        stop,
        "delete",
        strTestDbName,    // strDatabaseNames,
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
                        continue;
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
                        continue;
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
                        continue;
                    }
                    if (nRet != 0)
                    {
                        errors.Add("比较针对 '" + strObjectPath + "' 上传和下载的两个文件时出错: 两个文件内容不一致: " + strError);
                        return errors;
                        continue;
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
                LogAndRecover("");
            });
        }

        #region 日志和恢复

        void LogAndRecover(string strStyle)
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
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
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
                    stop,
                    channel,
                    strDate,
                    LogType.OperLog,
                    out strError);
                if (lRet <= -1)
                {
                    goto ERROR1;
                }

                long lStartOffs = lRet; // 日志起点偏移

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
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();

                // *** 恢复测试
                // 从指定偏移位置启动 dp2library 日志恢复后台任务
                string strTaskName = "日志恢复";
                nRet = StartBatchTask(
                    stop,
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
            stop,
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

                // 删除测试用的书目库、排架体系、馆藏地定义
                Progress.SetMessage("正在删除测试用书目库 ...");
                lRet = channel.ManageDatabase(
    stop,
    "delete",
    strBiblioDbName,    // strDatabaseNames,
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 重新获得各种库名、列表
                ReloadDatabaseCfg();
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
            }
        ERROR1:
            this.Invoke((Action)(() => MessageBox.Show(this, strError)));
            this.ShowMessage(strError, "red", true);
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
                    return 1;
                }

                Thread.Sleep(1000);
            }

            return 0;
        }

        #endregion

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
