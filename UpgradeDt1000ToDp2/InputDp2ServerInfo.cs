using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace UpgradeDt1000ToDp2
{
    /// <summary>
    /// 和 输入dp2服务器信息 有关的代码
    /// </summary>
    public partial class MainForm : Form
    {
        public void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this.textBox_dp2UserName.Text;
                e.Password = this.textBox_dp2Password.Text;
                e.Parameters = "location=#upgrade,type=worker";

                e.Parameters += ",client=upgradedt1000todp2|0.01";

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // 
            IWin32Window owner = null;

            if (sender is IWin32Window)
                owner = (IWin32Window)sender;
            else
                owner = this;

            e.UserName = this.textBox_dp2UserName.Text;
            e.Password = this.textBox_dp2Password.Text;
            e.SavePasswordShort = true;
            e.Parameters = "location=#upgrade,type=worker";

            e.Parameters += ",client=upgradedt1000todp2|0.01";

            e.SavePasswordLong = true;
            e.LibraryServerUrl = this.textBox_dp2AsUrl.Text;
        }

        // return:
        //      1   登录成功
        //      <=0 登录失败 strError中有原因
        int DetectLoginToDp2Server(out string strError)
        {
            strError = "";

            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            long lRet = this.Channel.Login(this.textBox_dp2UserName.Text,
                this.textBox_dp2Password.Text,
                "location=#upgrade,type=worker",
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            if (lRet == 0)
            {
                strError = "密码不正确";
                return 0;
            }

            return 1;
        }
    }
}
