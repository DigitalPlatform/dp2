using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public class Commander
    {
        public event IsBusyEventHandler IsBusy = null;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        List<int> messages = new List<int>();

        Form form = null;

        public Commander(Form form)
        {
            this.form = form;

            this.timer.Tick -= new EventHandler(timer_Tick);
            this.timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (this.IsBusy != null)
            {
                IsBusyEventArgs e1 = new IsBusyEventArgs();
                this.IsBusy(this, e1);
                if (e1.IsBusy == true)
                    return;

                lock (this.messages)
                {
                    for (int i = 0; i < this.messages.Count; i++)
                    {
                        API.PostMessage(this.form.Handle, this.messages[i], 0, 0);
                    }

                    this.messages.Clear();
                }
                this.timer.Stop();
            }
        }

        public void AddMessage(int value)
        {
            lock (this.messages)
            {
                this.messages.Add(value);
            }
            this.timer.Start();
        }

        public void Destroy()
        {
            lock (this.messages)
            {
                this.messages.Clear();
            }
            this.timer.Stop();
            this.timer.Tick -= new EventHandler(timer_Tick);
        }
    }

    /// <summary>
    /// 是否繁忙?
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void IsBusyEventHandler(object sender,
        IsBusyEventArgs e);

    /// <summary>
    /// 是否繁忙事件的参数
    /// </summary>
    public class IsBusyEventArgs : EventArgs
    {
        public bool IsBusy = false; // [out]
    }

}
