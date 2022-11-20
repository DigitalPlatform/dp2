using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Web;


using DigitalPlatform.Script;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 各种类型的统计窗，统计方案的宿主类的基础类
    /// 定义了过程 virtual 函数
    /// </summary>
    public class StatisHostBase : StatisHostBase0
    {


        /// <summary>
        /// 初始化。在统计方案执行的第一阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnInitial(object sender, StatisEventArgs e)
        {

        }


        // 开始
        /// <summary>
        /// 开始。在统计方案执行的第二阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnBegin(object sender, StatisEventArgs e)
        {

        }

        // 每一记录处理
        /// <summary>
        /// 处理一条记录。在统计方案执行中，第三阶段，针对每条记录被调用一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnRecord(object sender, StatisEventArgs e)
        {

        }

        // 结束
        /// <summary>
        /// 结束。在统计方案执行的第四阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnEnd(object sender, StatisEventArgs e)
        {

        }

        // 打印输出
        /// <summary>
        /// 打印输出。在统计方案执行结束后，需要打印的时候被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnPrint(object sender, StatisEventArgs e)
        {

        }
    }

    /// <summary>
    /// 流程控制类型
    /// </summary>
    public enum ContinueType
    {
        /// <summary>
        /// 要继续
        /// </summary>
        Yes = 0,

        /// <summary>
        /// 跳过所有
        /// </summary>
        SkipAll = 1,

        /// <summary>
        /// 报错后结束
        /// </summary>
        Error = 2,
    }

    /// <summary>
    /// 统计事件参数
    /// </summary>
    [Serializable]
    public class StatisEventArgs : EventArgs
    {
        /// <summary>
        /// 是否继续循环
        /// </summary>
        public ContinueType Continue = ContinueType.Yes;	// 是否继续循环

        /// <summary>
        /// 输入、输出字符串
        /// </summary>
        public string ParamString = ""; // [in][out]输入参数
    }
}
