using LibraryStudio.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Marc
{
    public partial class MarcEditor
    {
        // private readonly object _invalidateLock = new object();

        private System.Windows.Forms.Timer _invalidateTimer;
        // 时钟间隔多少时间触发一次检查
        private const int _checkDelayMs = 100; // 可调，30-100ms 常用范围

        void CreateCheckTimer()
        {
            _invalidateTimer = new System.Windows.Forms.Timer { Interval = _checkDelayMs };
            _invalidateTimer.Tick += (s, e) => CheckAndTrigger();
        }

        void DestroyCheckTimer()
        {
            if (_invalidateTimer != null)
            {
                _invalidateTimer.Stop();
                _invalidateTimer.Tick -= (s, e) => CheckAndTrigger(); // optional unsubscribe
                _invalidateTimer.Dispose();
                _invalidateTimer = null;
            }
        }

        private void ScheduleEvent()
        {
            if (!_invalidateTimer.Enabled)
                _invalidateTimer.Start();
        }

        // 上一次探测时插入符所在的字段的字段名
        string _lastCaretFieldName = "";

        private void CheckAndTrigger()
        {
            //if (DateTime.UtcNow < _lastPendingTime + _idleLength)
            //    return;

            //lock (_invalidateLock)
            //{
                _invalidateTimer.Stop();
            //}

            // 检查和触发
            var field = GetDomRecord().GetField(this.CaretFieldIndex, false);
            if (field == null)
                return;

            // TODO: 当前所在的子字段名发生变化，也要触发此事件
            var current_caret_field_name = field.Name;
            if (current_caret_field_name != _lastCaretFieldName)
            {
                this.FireSelectedFieldChanged();
                _lastCaretFieldName = current_caret_field_name;
            }
        }

    }
}
