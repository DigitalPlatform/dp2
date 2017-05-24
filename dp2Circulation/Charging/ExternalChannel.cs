using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 额外的通道。QuickChargingForm 用到了这个类
    /// </summary>
    public class ExternalChannel
    {
        // public MainForm MainForm = null;

        public DigitalPlatform.Stop stop = null;

        public LibraryChannel Channel = new LibraryChannel();

        bool _doEvents = false;

        public void Initial(// MainForm main_form,
            bool bDoEvents = false)
        {
            this._doEvents = bDoEvents;
            // this.MainForm = main_form;

            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this.Channel.Idle -= new IdleEventHandler(Channel_Idle);
            this.Channel.Idle += new IdleEventHandler(Channel_Idle);

            stop = new DigitalPlatform.Stop();
            stop.Register(Program.MainForm.stopManager, true);	// 和容器关联

            return;
        }

        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            Program.MainForm.Channel_AfterLogin(sender, e);    // 2015/11/8
        }

        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // e.bDoEvents = this._doEvents;

            // 2016/1/26
            if (this._doEvents)
                Application.DoEvents();
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            Program.MainForm.Channel_BeforeLogin(sender, e);    // 2015/11/8
        }

        public void Close()
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            if (this.Channel != null)
            {
                this.Channel.Close();
                this.Channel = null;
            }
        }

        public void PrepareSearch(string strText)
        {
            if (stop != null)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial(strText);
                stop.BeginLoop();
            }
        }

        public void EndSearch()
        {
            if (stop != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
    }

}
