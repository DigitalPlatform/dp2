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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System.Diagnostics;

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
        //                  copy_enablesubrecord    是否允许下级记录
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
            copy_param.CopyChildRecords = StringUtil.IsInList("copy_copychildrecords",strStyle);
            copy_param.BuildLink = StringUtil.IsInList("copy_buildlink",strStyle);
            copy_param.EnableSubRecord = StringUtil.IsInList("copy_enablesubrecord",strStyle);


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
            foreach(List<int> current in result)
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
