using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Marc;

namespace dp2Circulation
{
    /// <summary>
    /// 旧的种册窗二次开发宿主类。现在已经被 DetailHost 类替代。
    /// 保留Host类的代码是出于兼容以前的脚本考虑，将在一段时间后删除这个类
    /// </summary>
    public class Host
    {
        /// <summary>
        /// 种册窗
        /// </summary>
        public EntityForm DetailForm = null;

        /// <summary>
        /// 脚本编译后的 Assembly
        /// </summary>
        public Assembly Assembly = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Host()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// 调用一个 Ctrl+A 功能
        /// </summary>
        /// <param name="strFuncName">功能名</param>
        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // 调用成员函数
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);
        }

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Main(object sender, HostEventArgs e)
        {

        }
    }

    /// <summary>
    /// Host 统计事件参数
    /// </summary>
    public class HostEventArgs : EventArgs
    {
        /*
        // 从何处启动? MarcEditor EntityEditForm
        public object StartFrom = null;
         * */

        /// <summary>
        /// [in]创建数据的事件参数
        /// </summary>
        public GenerateDataEventArgs e = null;
    }
}
