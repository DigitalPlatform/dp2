using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
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
    }
}
