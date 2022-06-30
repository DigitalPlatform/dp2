using System;
using System.Collections.Generic;
using System.Threading;

namespace DigitalPlatform
{
    /*
     * 1) stop = new Stop() 
     * 2) 窗口load阶段 Register()
     * 3) 窗口closed阶段 Unregister()
     * 4) 每次循环开始 Initial() 关联delegate
     *    循环开始时候，BeginLoop() 循环结束后，EndLoop() Initial()撤离和delegate的关联
     * 5) 在循环执行中，如果用户触发stop button，delegate自然会被调用。或者
     *    在循环中主动观察stop的State状态，也可以得知按钮是否已经被触发了
     * 
     * 
     * 
     * 
     * 
     * 
     */

    //在子窗口中定义
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
        StopManager m_manager = null;

        string m_strMessage = "";

        // bool m_bCancel = false;

        public string Name = "";

        public object Tag = null;   // 用来存放任意对象

        public Stop()
        {
        }

        // parameters:
        //      strMessage  要显示的消息。如果为 null，表示不显示消息
        public string Initial(// Delegate_doStop doStopDelegate,
            string strMessage)
        {
            string strOldMessage = m_strMessage;
            // m_doStopDelegate = doStopDelegate;
            if (strMessage != null)
            {
                m_strMessage = strMessage;
                if (m_manager != null)
                {
                    // TODO: 执行多次更新，可能毁掉原来存储的状态
                    m_manager.ChangeState(this,
                        StateParts.All,
                        true);
                }
            }

            return strOldMessage;
        }

        // 注册，和管理对象建立联系
        // parameters:
        //      bActive 是否需要立即激活
        public void Register(StopManager manager,
            bool bActive)
        {
            m_manager = manager;
            manager.Add(this);

            if (bActive == true)
                manager.Active(this);
        }

        // parameters:
        //      bActive 是否激活改变后的顶层 Stop 对象
        public void Unregister(bool bActive = true)
        {
            if (m_manager != null)
            {
                // var old_top = m_manager.ActiveStop;

                m_manager.Remove(this, true);

                /*
                // 2022/6/29
                if (bActive)
                {
                    var new_top = m_manager.ActiveStop;
                    if (new_top != old_top)
                        m_manager.Active(new_top);
                }
                */

                m_manager = null;
            }
        }

        int _inBeginLoop = 0;

        // 检查当前是否已经处于 BeginLoop() 之中
        public bool IsInLoop
        {
            get
            {
                return _inBeginLoop > 0;
            }
        }

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

        //准备做事情,被循环调，时面了调了Stopmanager的Enable()函数，修改父窗口的按钮状态
        // return:
        //      true 成功
        //      false   失败。BeginLoop() 发生了嵌套
        public void BeginLoop()
        {
#if NO
            if (_inBeginLoop > 0 
                && _allowNest == false)
                throw new Exception("针对同一 Stop 对象，BeginLoop 不能嵌套调用");
#endif

            int nRet = Interlocked.Increment(ref _inBeginLoop);
            // _inBeginLoop++;
            if (nRet == 1)
            {
                nStop = 0;	// 正在处理

                if (m_manager != null)
                {
                    bool bIsActive = m_manager.IsActive(this);

                    if (this.OnBeginLoop != null)
                    {
                        BeginLoopEventArgs e = new BeginLoopEventArgs();
                        e.IsActive = bIsActive;
                        this.OnBeginLoop(this, e);
                    }

                    if (bIsActive == true)
                    {
                        m_manager.ChangeState(this,
                            StateParts.All | StateParts.SaveEnabledState,
                            true);
                    }
                    else
                    {
                        // 不在激活位置的stop，不要记忆原有的reversebutton状态。因为这样会记忆到别人的状态
                        m_manager.ChangeState(this,
                            StateParts.All,
                            true);
                    }
                }
            }
        }

        //事情做完了，被循环调，里面调了StopManager的Enable()函数，修改按钮为发灰状态
        public void EndLoop()
        {
            if (_inBeginLoop == 0)
                throw new Exception("针对同一 Stop 对象，调用 EndLoop() 不应超过 BeginLoop() 调用次数");

            int nRet = Interlocked.Decrement(ref _inBeginLoop);

            if (nRet == 0)
            {
                nStop = 2;	// 转为 已经停止 状态

                if (m_manager != null)
                {
                    bool bIsActive = m_manager.IsActive(this);

                    this.m_strMessage = "";

                    if (this.OnEndLoop != null)
                    {
                        EndLoopEventArgs e = new EndLoopEventArgs();
                        e.IsActive = bIsActive;
                        this.OnEndLoop(this, e);
                    }

                    if (bIsActive == true)
                    {
                        m_manager.ChangeState(this,
                            StateParts.All | StateParts.RestoreEnabledState,
                            true);
                    }
                    else
                    {
                        // 不在激活位置，不要恢复所谓旧状态
                        m_manager.ChangeState(this,
                            StateParts.All,
                            true);
                    }
                }
            }

            // _inBeginLoop--;
        }

        public void SetMessage(string strMessage)
        {
            m_strMessage = strMessage;

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

            if (m_manager != null)
            {
                // TODO: 只应当改变文本的状态，不应当动按钮的状态
                m_manager.ChangeState(this,
                    StateParts.Message,
                    true);
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

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressRange,
                    true);
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

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
            }

            // 2014/1/7
            if (lValue > this.ProgressMax)
            {
                this.ProgressMax = lValue;
                m_manager.ChangeState(this,
    StateParts.ProgressRange,
    true);
            }
        }

        public void HideProgress()
        {
            this.ProgressValue = -1;

            if (m_manager != null)
            {
                m_manager.ChangeState(this,
                    StateParts.ProgressValue,
                    true);
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
                return m_strMessage;
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
        public bool IsActive = false;
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
