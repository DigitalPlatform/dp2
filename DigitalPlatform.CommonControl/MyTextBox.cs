using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 具有一些特性的 TextBox
    /// 提供延迟发出的 DelayTextChanged 事件
    /// </summary>
    public class MyTextBox : TextBox
    {
        public event EventHandler DelayTextChanged = null;

        Timer _timer = null;

        int _strokeCount = 0;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            if (this.DelayTextChanged != null)
            {
                // 如果这一段没有累积的动作，触发事件
                if (this._strokeCount == 0)
                {
                    this.DelayTextChanged(this, new EventArgs());

                    this._strokeCount = 0;
                    if (this._timer != null)
                        this._timer.Stop();
                }
                else
                {
                    if (this._timer == null && this.DelayTextChanged != null)
                    {
                        this._timer = new Timer();
                        this._timer.Interval = 1000;
                        this._timer.Tick -= new EventHandler(_timer_Tick);
                        this._timer.Tick += new EventHandler(_timer_Tick);
                    }

                    this._timer.Start();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            this._strokeCount++;
        }

        void _timer_Tick(object sender, EventArgs e)
        {

            if (this.DelayTextChanged != null)
            {
                if (this._strokeCount > 0)
                {
                    // 如果有累积的 keydown 动作，则清除积累数字，然后返回
                    this._strokeCount = 0;
                    return;
                }

                if (this._strokeCount == 0)
                {
                    // 如果这一段没有累积的动作了，才真正触发事件
                    this.DelayTextChanged(this, new EventArgs());

                    this._strokeCount = 0;
                    if (this._timer != null)
                        this._timer.Stop();
                }
            }
        }
    }
}
