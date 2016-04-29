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

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    internal partial class DkywAmerceCardDialog : Form
    {
        public AmerceForm AmerceForm = null;
        public AmerceItem[] AmerceItems = null;
        public List<OverdueItemInfo> OverdueInfos = null;

        bool m_bDone = false;   // 扣款是否完成

        IpcClientChannel channel = new IpcClientChannel();

        DkywInterface obj = null;

        public DkywAmerceCardDialog()
        {
            InitializeComponent();
        }

        private void AmerceCardDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = this.StartChannel(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                this.timer1.Start();
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


                if (this.m_bBegined == false)
                {
                    obj.DisableSendKey();
                    this.m_bBegined = true;
                }

                string strUsedCardNumber = "";
                string strNewPrice = "";
                int nErrorCode = 0;
                string strPassword = "";


                int nRedoCount = 0;
                REDO:
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

                if (nRet == -1)
                {
                    if (nErrorCode == -7)
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
        bool m_bBegined = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nIn > 0)
                return;

            this.m_nIn++;
            try
            {
                string strError = "";

                if (this.m_bBegined == false)
                {
                    obj.DisableSendKey();
                    this.m_bBegined = true;
                }

                string strCardNumber = "";
                string strRest = "";
                string strLimitMoney = "";
                int nErrorCode = 0;

                // return:
                //      -1  出错
                //      0   没有卡
                //      1   成功获得信息
                int nRet = obj.GetCardInfo(out strCardNumber,
                    out strRest,
                    out strLimitMoney,
                    out nErrorCode,
                    out strError);
                if (nRet == 0)
                {
                    this.label_cardInfo.Text = "请放上IC卡...";
                    this.SetColor(2);
                    return;
                }

                if (nRet == -1)
                {
                    this.label_cardInfo.Text = "读卡错误:" + strError;
                    this.SetColor(1);
                    return;
                }


                this.label_cardInfo.Text = "卡号: " + strCardNumber + "\r\n" + "卡上金额: " + strRest;

                // 和this.CardNumber比较
                if (this.CardNumber != strCardNumber)
                {
                    this.label_cardInfo.Text += "\r\n!!!警告：交费者的卡号应当为 '" + this.CardNumber + "'。不是当前在读卡器上的卡";
                    this.SetColor(1);
                }
                else
                {
                    // 看看余额是否够?
                    try
                    {
                        Decimal rest = Convert.ToDecimal(strRest);
                        Decimal sub = Convert.ToDecimal(this.SubmitPrice);
                        if (rest < sub)
                        {
                            this.label_cardInfo.Text += "\r\n!!!警告：余额不足";
                            this.SetColor(1);
                        }
                        else
                        {
                            this.SetColor(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.label_cardInfo.Text = ex.Message;
                        this.SetColor(1);
                    }
                }
            }
            catch (System.Runtime.Remoting.RemotingException ex)
            {
                this.label_cardInfo.Text = "IC卡监控模块 DkywCardReader.exe 尚未启动 (错误信息:" + ex.Message + ")";
                this.SetColor(1);
            }
            catch (Exception ex)
            {
                this.label_cardInfo.Text = "错误:" + ex.Message;
                this.SetColor(1);
            }
            finally
            {
                this.m_nIn--;
            }
        }

        void StopChannel()
        {
            this.timer1.Stop();

            if (this.m_bBegined == true)
            {
                obj.EnableSendKey();
                this.m_bBegined = false;
            }

            this.EndChannel();
        }

        private void AmerceCardDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.StopChannel();
        }

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
            if (this.channel != null)
            {
                ChannelServices.UnregisterChannel(this.channel);
                this.channel = null;
            }
        }

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


    }
}