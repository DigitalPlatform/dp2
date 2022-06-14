using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// 空闲事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void IdleEventHandler(object sender,
    IdleEventArgs e);

    /// <summary>
    /// 空闲事件的参数
    /// </summary>
    public class IdleEventArgs : EventArgs
    {
        // public bool bDoEvents = true;
    }

    /// <summary>
    /// 内容发生改变
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ContentChangedEventHandler(object sender,
    ContentChangedEventArgs e);

    /// <summary>
    /// 获得值列表的参数
    /// </summary>
    public class ContentChangedEventArgs : EventArgs
    {
        public bool OldChanged = false;
        public bool CurrentChanged = false;
    }

    /// <summary>
    /// 获得值列表
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetValueTableEventHandler(object sender,
    GetValueTableEventArgs e);

    /// <summary>
    /// 获得值列表的参数
    /// </summary>
    public class GetValueTableEventArgs : EventArgs
    {
        public string TableName = "";

        public string DbName = "";


        /// <summary>
        /// 值列表
        /// </summary>
        public string[] values = null;

    }

    ///
    /// <summary>
    /// 按键
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ControlKeyPressEventHandler(object sender,
        ControlKeyPressEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class ControlKeyPressEventArgs : EventArgs
    {
        public KeyPressEventArgs e = null;

        // 焦点所在事项名
        public string Name = "";
    }



    ///
    /// <summary>
    /// 按键
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ControlKeyEventHandler(object sender,
        ControlKeyEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class ControlKeyEventArgs : EventArgs
    {
        public KeyEventArgs e = null;

        // 焦点所在事项名
        public string Name = "";

        /*
        // 触发所在的子控件
        // 2009/2/24
        public object SenderControl = null;
         * */
    }

    public delegate void ApendMenuEventHandler(object sender,
    AppendMenuEventArgs e);

    public class AppendMenuEventArgs : EventArgs
    {
        public ContextMenu ContextMenu = null;  // [in]
    }

    public class LockException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public LockException(string strText)
            : base(strText)
        {
        }
    }

    // https://codereview.stackexchange.com/questions/261396/asynchronous-event-handler
    public static class DelegateExtensions
    {
        public static Task InvokeAsync<TArgs>(this Func<object, TArgs, Task> func, object sender, TArgs e)
        {
            return func == null ? Task.CompletedTask
                : Task.WhenAll(func.GetInvocationList().Cast<Func<object, TArgs, Task>>().Select(f => f(sender, e)));
        }
    }
}
