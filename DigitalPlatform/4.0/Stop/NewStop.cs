using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public class Stop
    {
        public long ProgressMin = -1;
        public long ProgressMax = -1;
        public long ProgressValue = -1;

        public StopStyle Style = StopStyle.None;
        public ReaderWriterLock m_stoplock = new ReaderWriterLock();
        public static int m_nLockTimeout = 5000;	// 5000=5秒

        public event StopEventHandler OnStop = null;

        public event BeginLoopEventHandler OnBeginLoop = null;
        public event EndLoopEventHandler OnEndLoop = null;
        public event ProgressChangedEventHandler OnProgressChanged = null;
        // public event MessageChangedEventHandler MessageChanged = null;

        volatile int nStop = -1;	// -1: 尚未使用 0:正在处理 1:希望停止 2:已经停止，EndLoop()已经调用
        StopManager _manager = null;

        string _message = "";

        public string Name = "";

        public object Tag = null;   // 用来存放任意对象

        public Stop()
        {
        }

        // parameters:
        //      strMessage  要显示的消息。如果为 null，表示不显示消息
        public string Initial(string strMessage)
        {
            string strOldMessage = _message;
            // m_doStopDelegate = doStopDelegate;
            if (strMessage != null)
            {
                _message = strMessage;

                _manager.UpdateMessage(this);
                /*
                // TODO: 执行多次更新，可能毁掉原来存储的状态
                _manager?.ChangeState(this,
                    StateParts.All,
                    true);
                */
            }

            return strOldMessage;
        }

        #region new

        private StopGroup _group = null;

        public StopGroup Group
        {
            get
            {
                return _group;
            }
            set
            {
                _group = value;
            }
        }

        // 新版本
        public void Register(StopManager manager,
    string groupName)
        {
            _manager = manager;
            manager.RegisterStop(this, groupName);

            /*
            // TODO: 为 manager.Add() 增加一个 activate 参数，尽量在同一步完成，这样便于减少加锁时间
            if (bActive == true)
                manager.Active(this);
            */
        }

        // parameters:
        //      bActive (此参数已经废止，因为 Unregister 后必定要把某个对象变为激活)是否激活改变后的顶层 Stop 对象
        public void Unregister()
        {
            if (_manager == null)
                throw new Exception("Stop 对象未曾注册过，所以无法注销");
            if (_manager != null)
            {
                _manager.UnregisterStop(this);
                _manager = null;
            }
        }

        // 当前 Stop 是否处在 Active 状态？
        // 注意，只有同时 Active 和属于 active group，才会影响显示
        public bool IsActive()
        {
            if (_group == null || _manager == null)
                return false;
            return this == _group.GetActiveStop();
        }

        #endregion

        /* 激活概念：
         * 每个 MDI 子窗口可以拥有多个 Stop 对象，这些对象在 MDI 子窗口处在激活状态的时候，也应该是激活的。
         * 也就是说一组一组的 Stop 对象，可以随时激活其中任何一组，其它组就变为非激活状态。
         * 工具条的 buttons 和状态条，只显示激活的这一组的动态。非激活的不显示
         * 
         * 激活状态和 looping 状态叠加，才会被视觉显示。
         * 只有 looping 状态，那就是后台的被遮挡的 MDI 子窗口的循环操作；
         * 只有激活状态，说明是前台 MDI 子窗口的 Stop 对象，但还没有开始循环或者循环已经停止
         * 激活也可以称为“前台”
         * 
         * 那当一个 MDI 子窗口新注册 Stop 对象的时候，如果要查询一次自己是否属于前台才能注册 Stop 对象，
         * 比较麻烦。可以为每个子窗口设定一个“组名”，Stop 对象具有组名，一组 Stop 对象是否应该处在前台，只需要查看组名的激活状态即可。
         */

        // 旧版本，计划逐渐弃用
        // 注册，和管理对象建立联系
        // parameters:
        //      bActive 是否需要立即激活
        public void Register(StopManager manager,
            bool bActive)
        {
            _manager = manager;
            manager.RegisterStop(this);

            // TODO: 为 manager.Add() 增加一个 activate 参数，尽量在同一步完成，这样便于减少加锁时间
            if (bActive == true)
                manager.Active(this);
        }

        // 在循环中的深度
        int _inLoopingLevel = 0;

        // 检查当前是否已经处于 BeginLoop() 之中
        public bool IsInLoop
        {
            get
            {
                return _inLoopingLevel > 0;
            }
        }

        // 允许或者禁止嵌套 BeginLoop()
        bool _allowNest = false;

        public bool AllowNest
        {
            get
            {
                return _allowNest;
            }
            set
            {
                this._allowNest = value;
            }
        }

        // 允许或者禁止嵌套 BeginLoop()
        public bool SetAllowNest(bool bAllow)
        {
            bool bOldValue = this._allowNest;
            this._allowNest = bAllow;
            return bOldValue;
        }

        /*
         * BeginLoop() 要做的事情
         * 1) (因为当前 Stop 对象加入活动状态数组，导致)改变 stop button 和 reverse button 的 enabled 状态
         *      注意这些 buttons 是数组共有的
         * 2) (progressbar 为当前 Stop 私有)接管 progress bar 的显示。初始化视觉状态，一般是隐藏 progress bar
         * 
         * 这里说明一下，Stop 对象要分为 looping 和 unlooping 两种数组存储，looping 的才会影响到视觉状态，而 unlooping 的相当于隐藏了不显示
         * */

        //准备做事情,被循环调，时面了调了Stopmanager的Enable()函数，修改父窗口的按钮状态
        // return:
        //      true 成功
        //      false   失败。BeginLoop() 发生了嵌套
        public void BeginLoop()
        {
#if NO
            if (_inLoopingLevel > 0 
                && _allowNest == false)
                throw new Exception("针对同一 Stop 对象，BeginLoop 不能嵌套调用");
#endif

            int nRet = Interlocked.Increment(ref _inLoopingLevel);
            // _inBeginLoop++;
            if (nRet == 1)
            {
                this.m_stoplock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    nStop = 0;  // 正在处理

                    if (_manager != null)
                    {
                        // bool bIsActive = this.IsActive();

                        if (this.OnBeginLoop != null)
                        {
                            BeginLoopEventArgs e = new BeginLoopEventArgs();
                            // TODO: 可以考虑取消 e.IsActive。因为使用者可以从 stop.IsActive 自己探测
                            // e.IsActive = bIsActive;
                            this.OnBeginLoop(this, e);
                        }

                        /*
                        // 只要 Stop 对象属于 active group，就有可能改变显示状态
                        var belong_active_group = this.Group == _manager.GetActiveGroup();
                        if (belong_active_group)
                            _manager.UpdateDisplay();
                        */
                        _manager.UpdateDisplay();

#if REMOVED
                    if (bIsActive == true)
                    {
                        _manager.ChangeState(this,
                            StateParts.All | StateParts.SaveEnabledState,
                            true);
                    }
                    else
                    {
                        // 不在激活位置的stop，不要记忆原有的reversebutton状态。因为这样会记忆到别人的状态
                        _manager.ChangeState(this,
                            StateParts.All,
                            true);
                    }
#endif
                    }
                }
                finally
                {
                    this.m_stoplock.ReleaseWriterLock();
                }
            }
        }

        //事情做完了，被循环调，里面调了StopManager的Enable()函数，修改按钮为发灰状态
        public void EndLoop()
        {
            if (_inLoopingLevel == 0)
                throw new Exception("针对同一 Stop 对象，调用 EndLoop() 不应超过 BeginLoop() 调用次数");

            int nRet = Interlocked.Decrement(ref _inLoopingLevel);

            if (nRet == 0)
            {
                this.m_stoplock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    nStop = 2;  // 转为 已经停止 状态
                    this._message = "";

                    if (_manager != null)
                    {
                        // bool bIsActive = this.IsActive();

                        if (this.OnEndLoop != null)
                        {
                            EndLoopEventArgs e = new EndLoopEventArgs();
                            // TODO: 可以考虑取消 e.IsActive。因为使用者可以从 stop.IsActive 自己探测
                            // e.IsActive = bIsActive;
                            this.OnEndLoop(this, e);
                        }

                        /*
                        // 只要 Stop 对象属于 active group，就有可能改变显示状态
                        var belong_active_group = this.Group == _manager.GetActiveGroup();
                        if (belong_active_group)
                        {
                            _manager.UpdateDisplay();
                        }
                        */
                        _manager.UpdateDisplay();

                        /*
                        // TODO: (2023/10/10)这里 Assert() 需要仔细测试一下
                        // EndLoop() 以后当前 Stop 对象一定不能是活动的状态了
                        Debug.Assert(_manager.SurfaceStop != this);
                        */
#if REMOVED
                    if (bIsActive == true)
                    {
                        _manager.ChangeState(this,
                            StateParts.All | StateParts.RestoreEnabledState,
                            true);
                    }
                    else
                    {
                        // 不在激活位置，不要恢复所谓旧状态
                        _manager.ChangeState(this,
                            StateParts.All,
                            true);
                    }
#endif
                    }
                }
                finally
                {
                    this.m_stoplock.ReleaseWriterLock();
                }
            }

            // _inBeginLoop--;
        }

        public void SetMessage(string strMessage)
        {
            _message = strMessage;

#if REMOVED
            // 2019/6/23
            if (this.MessageChanged != null)
            {
                this.MessageChanged(this, new MessageChangedEventArgs { Message = strMessage });
            }
#endif

            this.OnProgressChanged?.Invoke(
    this,
    new ProgressChangedEventArgs
    {
        Message = strMessage,
        Start = -1,
        End = -1,
        Value = -1
    }
    );

            if (_manager != null)
            {
                _manager.UpdateMessage(this);
                /*
                // TODO: 只应当改变文本的状态，不应当动按钮的状态
                _manager.ChangeState(this,
                    StateParts.Message,
                    true);
                */
            }
        }

        public void SetProgressRange(long lStart, long lEnd)
        {
            this.ProgressMin = lStart;
            this.ProgressMax = lEnd;
            this.ProgressValue = lStart;

            // 2017/12/16
            this.OnProgressChanged?.Invoke(
    this,
    new ProgressChangedEventArgs
    {
        Start = lStart,
        End = lEnd,
        Value = lStart
    }
    );

            if (_manager != null)
            {
                _manager.UpdateProgressRange(this);
                /*
                _manager.ChangeState(this,
                    StateParts.ProgressRange,
                    true);
                */
            }
        }

        public void SetProgressValue(long lValue, object tag = null)
        {
            this.ProgressValue = lValue;

            // 2017/12/16
            this.OnProgressChanged?.Invoke(
                this,
                new ProgressChangedEventArgs
                {
                    Start = this.ProgressMin,
                    End = this.ProgressMax,
                    Value = lValue,
                    Tag = tag,
                }
                );

            if (lValue > this.ProgressMax)
            {
                this.ProgressMax = lValue;
                _manager.UpdateProgressRange(this);

                /*
                _manager.ChangeState(this,
    StateParts.ProgressRange,
    true);
                */
            }

            if (_manager != null)
            {
                _manager.UpdateProgressValue(this);
                /*
                _manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
                */
            }
        }

        // 在所属的 group 中激活。
        // 注：只有当所属的 group 是 active group 时，才会作用到显示
        public bool MoveToTop()
        {
            if (this.Group == null)
                return false;
            return this.Group.MoveToTop(this);
            // 注意本函数的效果是不完满的，建议用 StopManager.Activate(Stop stop) 处理
        }

        public void HideProgress()
        {
            this.ProgressValue = -1;

            if (_manager != null)
            {
                _manager.UpdateProgressValue(this);
                /*
                _manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
                */
            }
        }

        //查看是否结束,被StopManager调
        public virtual int State
        {
            get
            {
                return nStop;
            }
        }

        public virtual bool IsStopped
        {
            get
            {
                if (this.State != 0)
                    return true;
                return false;
            }
        }

        // TODO: 处理中是否要加锁?
        public virtual void Continue()
        {
            nStop = 0;
        }

        public virtual void SetState(int nState)
        {
            this.nStop = nState;
        }

        // 停止,被StopManager调
        // locks: 写锁定
        // parameters:
        //      bHalfStop   是否为一半中断。所谓一般中断，就是不触发Stop事件，而只修改Stop状态。
        public void DoStop(object sender = null)
        {
            this.m_stoplock.AcquireWriterLock(Stop.m_nLockTimeout);
            try
            {
                bool bHalfStop = false;

                if ((this.Style & StopStyle.EnableHalfStop) != 0
                    && this.nStop == 0)
                {
                    bHalfStop = true;
                }

                if (bHalfStop == false)
                {
                    if (this.OnStop != null)
                    {
                        // OnStop()是在已经锁定的情况下调用的
                        StopEventArgs e = new StopEventArgs();
                        this.OnStop(sender == null ? this : sender, e);
                    }
                }

                nStop = 1;
            }
            finally
            {
                this.m_stoplock.ReleaseWriterLock();
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public string DisplayMessage
        {
            get
            {
                return /*Group.Name + " " +*/ _message;
            }
        }
    }


    // Stop事件
    public delegate void StopEventHandler(object sender,
        StopEventArgs e);

    public class StopEventArgs : EventArgs
    {

    }

    // BeginLoop事件
    public delegate void BeginLoopEventHandler(object sender,
        BeginLoopEventArgs e);

    public class BeginLoopEventArgs : EventArgs
    {
        // public bool IsActive = false;
    }

    // EndLoop事件
    public delegate void EndLoopEventHandler(object sender,
        EndLoopEventArgs e);

    public class EndLoopEventArgs : EventArgs
    {
        public bool IsActive = false;
    }

#if REMOVED
    // 文字信息发生改变 事件
    public delegate void MessageChangedEventHandler(object sender,
    MessageChangedEventArgs e);

    public class MessageChangedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
#endif

    // 进度条发生改变 事件
    public delegate void ProgressChangedEventHandler(object sender,
    ProgressChangedEventArgs e);

    public class ProgressChangedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long Value { get; set; }
        // 2022/4/12
        // 扩展的参数
        public object Tag { get; set; }
    }

    // 定义一个Delegate_doStop()
    // public delegate void Delegate_doStop();

    // 哪些部件
    [Flags]
    public enum StateParts
    {
        None = 0x00,
        All = 0xff, // 全部 (除了StoreEnabledState以外)
        StopButton = 0x01,  // Stop按钮
        ReverseButtons = 0x02,  // Stop以外的其他按钮
        Message = 0x10, // 消息文本
        ProgressRange = 0x20,   // 进度条范围
        ProgressValue = 0x40,   // 进度条值

        SaveEnabledState = 0x0100,   // 存储Enabled状态
        RestoreEnabledState = 0x0200,   // 恢复以前存储Enabled状态
    }

    // 增补菜单
    public delegate void AskReverseButtonStateEventHandle(object sender,
    AskReverseButtonStateEventArgs e);

    public class AskReverseButtonStateEventArgs : EventArgs
    {
        public object Button = null;
        public bool EnableQuestion = true; // 问题：希望Enabled? == true指希望使能。一般不会询问使不能，因为这个软件底层可以自行决定
        public bool EnableResult = true;   // 答案：是否同意Enabled.
    }

    [Flags]
    public enum StopStyle
    {
        None = 0x00,
        EnableHalfStop = 0x01,  // 允许第一次“半中断”
    }

    public delegate void DisplayMessageEventHandler(object sender,
DisplayMessageEventArgs e);

    public class DisplayMessageEventArgs : EventArgs
    {
        public string Message = "";
    }
}
