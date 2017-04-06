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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Marc;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

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
            Task.Factory.StartNew(() => TestCreateAccessNo());
        }


        void TestCreateAccessNo()
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

                // 定义排架体系
                string strGroupFragment =
                    "<group name=\"_测试种次号\" classType=\"中图法\" qufenhaoType=\"种次号\" zhongcihaodb=\"\" callNumberStyle=\"索取类号+区分号\">"
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

            using (EntityForm entity_form = new EntityForm())
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
        }

        // 每创建一个册记录在内存，然后发生索取号，保存。再创建下一个索取号。要求索取号完全相同
        // parameters:
        //      strStyle    save_before 先保存创建的册记录，再给它创建索取号，然后再次保存
        void UiTest2(string strBiblioDbName, string strStyle)
        {
            string strError = "";

            using (EntityForm entity_form = new EntityForm())
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
        }

        // 多个窗口并发创建索取号，然后再统一保存
        void UiTest3(string strBiblioDbName//,
            //string strStyle
            )
        {
            string strError = "";

            using (EntityForm entity_form1 = new EntityForm())
            using (EntityForm entity_form2 = new EntityForm())
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
        }

        static void CreateEntityRecords(EntityForm entity_form, int nCount)
        {
            string strError = "";

            for (int i = 0; i < nCount; i++)
            {
                BookItem bookitem = new BookItem();
                bookitem.Barcode = "";
                bookitem.Location = "_测试阅览室";

                int nRet = entity_form.EntityControl.AppendEntity(bookitem,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

            }
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
}
