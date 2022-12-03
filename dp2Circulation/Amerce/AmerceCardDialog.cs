using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Interfaces;

namespace dp2Circulation
{
    internal partial class AmerceCardDialog : Form
    {
        public string InterfaceUrl = "";
        public AmerceForm AmerceForm = null;
        public AmerceItem[] AmerceItems = null;
        public List<OverdueItemInfo> OverdueInfos = null;

        bool m_bDone = false;   // 扣款是否完成

#if NO
        IpcClientChannel channel = new IpcClientChannel();
        DkywInterface obj = null;
#endif

        IChannel channel = null;    // new IpcClientChannel();
        ICardCenter obj = null;

        public AmerceCardDialog()
        {
            InitializeComponent();
        }

        private void AmerceCardDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = this.StartChannel(
                this.InterfaceUrl,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            this.timer1.Start();
        }

        void DisplayRest()
        {
            if (obj != null)
            {
                string strError = "";
                string strNewPrice = "";

                try
                {
                    lock (this.obj)
                    {
                        int nRet = obj.Deduct(this.CardNumber,
                            "",
                            "",
                            out strNewPrice,
                            out strError);
                        if (nRet == 1)
                        {
                            this.label_cardInfo.Text = "卡号 " + this.CardNumber + " \r\n的当前余额为 " + strNewPrice;
                            this.timer1.Stop(); // 只查询一次
                            return;
                        }
                    }
                }
                catch
                {
                    this.timer1.Stop();
                }
            }

            this.label_cardInfo.Text = "";
        }

        private void button_writeCard_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 防止重入
            if (this.m_nIn > 0)
            {
                strError = "发生冲突。稍后重试";
                goto ERROR1;
            }

            bool bSucceed = false;
            int nRet = 0;

            this.m_nIn++;
            this.button_writeCard.Enabled = false;
            try
            {
                // 先完成数据库操作
                nRet = this.AmerceForm.Submit(
                    this.AmerceItems,
                    this.OverdueInfos,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // string strUsedCardNumber = "";
                string strNewPrice = "";
                // int nErrorCode = 0;
                string strPassword = "";

                int nRedoCount = 0;
            REDO:

                lock (this.obj)
                {
                    // 从卡中心扣款
                    // parameters:
                    //      strRecord   读者XML记录。里面包含足够的表示字段即可
                    //      strPriceString  金额字符串。一般为类似“CNY12.00”这样的形式
                    //      strRest    扣款后的余额
                    // return:
                    //      -2  密码不正确
                    //      -1  出错(调用出错等特殊原因)
                    //      0   扣款不成功(因为余额不足等普通原因)。注意strError中应当返回不成功的原因
                    //      1   扣款成功
                    nRet = obj.Deduct(this.CardNumber,
                        this.SubmitPrice,
                        strPassword, // new
                        out strNewPrice, // new
                        out strError);
                }

#if NO
                // 扣款
                // parameters:
                //      strCardNumber   要求的卡号。如果为空，则表示不要求卡号，直接从当前卡上扣款
                //      strSubMoney 要扣的款额。例如："0.01"
                //      strUsedCardNumber   实际扣款的卡号
                //      strPrice    扣款后的余额
                //      nErrorCode 原始错误码
                //          -1:连接串口错误;
                //          -2:没有发现卡片;
                //          -3:无法读取卡的唯一序列号; 
                //          -4:装入密钥错误;
                //          -5:读卡错误;
                //          -6:卡已过有有效期;
                //          -7:密码错误
                //          -8:输入的金额太大;
                //          -9:写卡失败;
                // return:
                //      -1  出错
                //      0   没有卡
                //      1   成功扣款和获得信息
                //      2   虽然扣款成功，但是上传流水失败
                nRet = obj.SubCardMoney(this.CardNumber,
                    this.SubmitPrice,
                    strPassword,
                    out strUsedCardNumber,
                    out strNewPrice,
                    out nErrorCode,
                    out strError);
                if (nRet == 0)
                {
                    strError = "请放上IC卡，否则无法扣款";
                    goto ERROR1;
                }
#endif

                if (nRet == -2)
                {
                    CardPasswordDialog dlg = new CardPasswordDialog();
                    MainForm.SetControlFont(dlg, this.Font, false);

                    if (nRedoCount == 0)
                        dlg.MessageText = "请(持卡者)输入IC卡密码";
                    else
                        dlg.MessageText = strError;

                    dlg.CardNumber = this.CardNumber;
                    dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                        return; // 放弃扣款

                    strPassword = dlg.Password;
                    nRedoCount++;
                    goto REDO;
                }

                if (nRet != 1)
                {
                    strError = "扣款错误:" + strError;
                    goto ERROR1;
                }

                // this.label_cardInfo.Text = "卡号: " + strCardNumber + "\r\n" + "卡上金额: " + strNewPrice;

                this.m_bDone = true;
                this.button_writeCard.Enabled = false;  // 避免再次扣款
                bSucceed = true;
                MessageBox.Show(this, "扣款 " + this.SubmitPrice + " 成功，新余额 " + strNewPrice);

                if (nRet == 2)
                {
                    MessageBox.Show(this, strError);
                }
            }
            catch (Exception ex)
            {
                strError = "错误:" + ex.Message;
                goto ERROR1;
            }
            finally
            {
                if (bSucceed == false)
                {
                    string strError_1 = "";
                    nRet = this.AmerceForm.RollBack(out strError_1);
                    if (nRet == -1)
                    {
                        strError_1 = "针对交费操作的Rollback失败: " + strError_1 + "\r\n请系统管理员进行手动清理";
                        MessageBox.Show(this, strError_1);
                    }
                }

                this.m_nIn--;

                if (this.m_bDone == false)
                    this.button_writeCard.Enabled = true;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        int m_nIn = 0;

        void StopChannel()
        {
            this.EndChannel();
        }

        private void AmerceCardDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.timer1.Stop();

            this.StopChannel();
        }

        int StartChannel(
    string strUrl,
    out string strError)
        {
            strError = "";

            Uri uri = new Uri(strUrl);
            string strScheme = uri.Scheme.ToLower();

            if (strScheme == "ipc")
                channel = new IpcClientChannel();
            else if (strScheme == "tcp")
                channel = new TcpClientChannel();
            else if (strScheme == "http")
                channel = new HttpClientChannel();
            else
            {
                strError = "URL '" + strUrl + "' 中包含了无法识别的Scheme '" + strScheme + "'。只能使用 ipc tcp http 之一";
                return -1;
            }

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(channel, false);

            try
            {
                this.obj = (ICardCenter)Activator.GetObject(typeof(ICardCenter),
                    strUrl);
                if (this.obj == null)
                {
                    strError = "无法连接到 Remoting 服务器 " + strUrl;
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndChannel()
        {
            if (this.channel != null)
            {
                ChannelServices.UnregisterChannel(this.channel);
                this.channel = null;
            }
        }

#if NO
        int StartChannel(out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(channel, false);

            try
            {
                obj = (DkywInterface)Activator.GetObject(typeof(DkywInterface),
                    "ipc://CardChannel/DkywInterface");
                if (obj == null)
                {
                    strError = "could not locate Card Server";
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndChannel()
        {
            ChannelServices.UnregisterChannel(channel);
        }
#endif
        string m_strSubmitPrice = "";

        // 本次要扣款的值。纯数字，没有货币单位。如果为正数，表示扣款
        public string SubmitPrice
        {
            get
            {
                return this.m_strSubmitPrice;
            }
            set
            {
                this.m_strSubmitPrice = value;
                this.label_thisPrice.Text = "本次拟扣款: " + value;
            }
        }

        // 确定的卡号，当前交款者的卡

        string m_strCardNumber = "";

        public string CardNumber
        {
            get
            {
                return this.m_strCardNumber;
            }
            set
            {
                this.m_strCardNumber = value;
            }
        }

        // 设置卡片显示空间的颜色
        // parameters:
        //      nState  0 正常 1 读卡出错 2 没有卡
        void SetColor(int nState)
        {
            if (nState == 0)
            {
                this.label_cardInfo.BackColor = Color.LightYellow;
                this.label_cardInfo.ForeColor = Color.Black;
                if (this.m_bDone == true)
                    this.button_writeCard.Enabled = false;
                else
                    this.button_writeCard.Enabled = true;
                return;
            }

            if (nState == 1)
            {
                this.label_cardInfo.BackColor = Color.LightYellow;
                this.label_cardInfo.ForeColor = Color.Red;
                this.button_writeCard.Enabled = false;
                return;
            }

            if (nState == 2)
            {
                this.label_cardInfo.BackColor = Color.LightGray;
                this.label_cardInfo.ForeColor = Color.Black;
                this.button_writeCard.Enabled = false;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DisplayRest();
        }


    }
}