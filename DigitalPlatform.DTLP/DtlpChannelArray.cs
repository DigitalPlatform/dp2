using System;
using System.Collections;
using System.Windows.Forms;

using DigitalPlatform.Xml;

namespace DigitalPlatform.DTLP
{
    /*
	// 通讯休闲时刻
	// return:
	//		0	继续
	//		1	停止
	public delegate int Delegate_Idle(HostEntry entry);
    */

    /*
	// 获得缺省帐户信息
	// return:
	//		2	already login succeed
	//		1	dialog return OK
	//		0	dialog return Cancel
	//		-1	other error
	public delegate int Delegate_AskAccountInfo(DtlpChannel channel, 
	string strPath,
	out IWin32Window owner,	// 如果需要出现对话框，这里返回对话框的宿主Form
	out string strUserName,
	out string strPassword);
     * */

	/// <summary>
	/// Summary description for ChannelArray.
	/// </summary>
	public class DtlpChannelArray : ArrayList
	{
		public ApplicationInfo appInfo = null;

		// 获得缺省账户名等信息的回调函数地址
		// public Delegate_AskAccountInfo procAskAccountInfo = null;

        public event AskDtlpAccountInfoEventHandle AskAccountInfo = null;


		// 通讯休闲状态回调函数
		// public Delegate_Idle procIdle = null;
        public event DtlpIdleEventHandler Idle = null;

        public bool GUI = true;

		public DtlpChannelArray()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        public bool DoIdle(object sender)
        {
            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            bool bDoEvents = true;
            if (this.Idle != null)
            {
                DtlpIdleEventArgs e = new DtlpIdleEventArgs();
                this.Idle(sender, e);
                if (e.Stop == true)
                    return true;
                bDoEvents = e.bDoEvents;
            }

            if (bDoEvents == true)
            {
                try
                {
                    Application.DoEvents();	// 出让界面控制权
                }
                catch
                {
                }
            }

            System.Threading.Thread.Sleep(1);	// 避免CPU资源过度耗费

            return false;
        }

		// 创建一个新通道
		public DtlpChannel CreateChannel(int usrid)
		{
			DtlpChannel channel = new DtlpChannel();

			channel.Container = this;
			channel.m_lUsrID = usrid;
			channel.InitialHostArray();
	
			this.Add(channel);

			return channel;
		}

		public bool DestroyChannel(DtlpChannel channel)
		{
			channel.Cancel();
			this.Remove(channel);
			return true;
		}

        // 是否已经挂接了事件？
        public bool HasAskAccountInfoEventHandler
        {
            get
            {
                return (this.AskAccountInfo != null);
            }
        }

        // 调用事件
        public void CallAskAccountInfo(object obj,
            AskDtlpAccountInfoEventArgs e)
        {
            if (this.AskAccountInfo == null)
                return;

            this.AskAccountInfo(obj, e);
        }

	}

    // 事件: 询问帐户信息
    public delegate void AskDtlpAccountInfoEventHandle(object sender,
    AskDtlpAccountInfoEventArgs e);

    public class AskDtlpAccountInfoEventArgs : EventArgs
    {
        // 输入参数
        public DtlpChannel Channel = null;
        public string Path = "";

        // 输出参数
        public IWin32Window Owner = null;	// 如果需要出现对话框，这里返回对话框的宿主Form
        public string UserName = "";
        public string Password = "";

        public int Result = 0;
        // Result:
        //		2	already login succeed
        //		1	dialog return OK
        //		0	dialog return Cancel
        //		-1	other error
        public string ErrorInfo = "";
    }

    public delegate void DtlpIdleEventHandler(object sender,
DtlpIdleEventArgs e);


    public class DtlpIdleEventArgs : EventArgs
    {
        public bool bDoEvents = true;
        public bool Stop = false; 
    }
}
