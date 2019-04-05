using System;
using System.Collections;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.IO;

// using Microsoft.Web.Services3;
// using Microsoft.Web.Services3.Attachments;

using System.Web.Services.Protocols;	// 为了WebClientAsyncResult

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// 通讯通道集合
	/// </summary>
	public class RmsChannelCollection : Hashtable, IDisposable
	{
		// 获得缺省账户名等信息的回调函数地址
		// public Delegate_AskAccountInfo procAskAccountInfo = null;

        public event AskAccountInfoEventHandle AskAccountInfo = null;

		public bool GUI = true;

		public RmsChannelCollection()
		{
			//
			// TODO: Add constructor logic here
			//
		}

         // 2011/1/19
        public void Dispose()
        {
            foreach (string key in this.Keys)
            {
                RmsChannel channel = (RmsChannel)this[key];
                if (channel != null)
                {
                    try
                    {
                        channel.Close();
                    }
                    catch
                    {
                    }
                }
            }

            this.Clear();
            this.AskAccountInfo = null;
        }

        public void OnAskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            if (this.AskAccountInfo == null)
            {
                e.ErrorInfo = "AskAccountInfo事件函数未设置";
                e.Result = -1;
                return;
            }

            if (this.AskAccountInfo != null)
                this.AskAccountInfo(sender, e);
        }

		// 获得一个Channel对象
		// 如果集合中已经存在这个对象，则直接返回；否则创建一个新对象
		public RmsChannel GetChannel(string strUrl)
		{
			string strRegularUrl = strUrl.ToUpper();

			RmsChannel channel = (RmsChannel)this[strRegularUrl];

			if (channel != null)
				return channel;

			// 创建
			channel = new RmsChannel();
			channel.Url = strUrl;
			channel.Container = this;

			this.Add(strRegularUrl, channel);
			return channel;
		}
 
        // TPPD: 需要把这些临时对象管理起来，在必要的时候进行Close()
        // 创建一个临时Channel对象
        public RmsChannel CreateTempChannel(string strUrl)
        {
            string strRegularUrl = strUrl.ToUpper();

            // 创建
            RmsChannel channel = new RmsChannel();
            channel.Url = strUrl;
            channel.Container = this;

            // this.Add(strRegularUrl, channel);

            return channel;
        }

        // 2011/1/19
        public void Close()
        {
            this.Dispose();
        }
	}

    /*
	// 获得缺省帐户信息
	// return:
	//		2	already login succeed
	//		1	dialog return OK
	//		0	dialog return Cancel
	//		-1	other error
	public delegate int Delegate_AskAccountInfo(
	ChannelCollection Channels, 
	string strComment,
	string strUrl,
	string strPath,
	LoginStyle loginStyle,
	out IWin32Window owner,	// 如果需要出现对话框，这里返回对话框的宿主Form
	out string strUserName,
	out string strPassword);
     */

	public class AccessKeyInfo
	{
		public string Key = "";
		public string KeyNoProcess = "";
		public string Num = "";
		public string FromName = "";	// 检索途径名
		public string FromValue = "";	// key中from字段值
		public string ID = "";
	}

    // 事件: 询问帐户信息
    public delegate void AskAccountInfoEventHandle(object sender,
    AskAccountInfoEventArgs e);

    public class AskAccountInfoEventArgs : EventArgs
    {
        // 输入参数
	    public RmsChannelCollection Channels = null;
        public RmsChannel Channel = null;   // [in] 请求的Channel。  如果 == null，才从 Channels里面根据 Url 来找 2013/2/14
	    public string Comment = "";
	    public string Url = "";
	    public string Path = "";
	    public LoginStyle LoginStyle;
        // 输出参数
	    public IWin32Window Owner = null;	// 如果需要出现对话框，这里返回对话框的宿主Form
	    public string UserName = "";
	    public string Password = "";

        public int Result = 0;
        public string ErrorInfo = "";
        // return:
        //		2	already login succeed
        //		1	dialog return OK
        //		0	dialog return Cancel
        //		-1	other error
    }


}
