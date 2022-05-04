using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace sipApiTester.TestCase
{
    public static class TestPipeline
    {
        // Connect 然后 Login。Login 并发完成
        public static async Task<NormalResult> TestLoginPipeline(int amount,
            string style)
        {
            DataModel.SetMessage("TestLoginPipeline() 开始");

            if (Int32.TryParse(DataModel.sipServerPort, out int port) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"端口号 '{DataModel.sipServerPort}' 格式错误。应为整数"
                };

            // 创建通道
            var channel = new SipChannel(Encoding.UTF8);

            var result = await channel.ConnectAsync(DataModel.sipServerAddr, port);
            if (result.Value == -1)
                DataModel.SetMessage($"Connect 出错: {result.ErrorInfo}", "error");

            var use_error_password = StringUtil.IsInList("useErrorPassword", style);

            // 负责发送的线程
            var send_task = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < amount; i++)
                {
                    string password = DataModel.sipPassword;
                    if (use_error_password)
                        password += "1";
                    // return.Value
                    //      -1  出错
                    //      0   登录失败
                    //      1   登录成功
                    var login_result = await channel.SendLoginAsync(DataModel.sipUserName, password);
                    if (login_result.Value != 1)
                    {
                        DataModel.SetMessage($"Login 发送出错: {login_result.ErrorInfo}", "error");
                    }
                    else
                        DataModel.SetMessage("Login 发送成功");
                }
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default).Unwrap();

            // 负责接收的线程
            var recv_task = Task.Factory.StartNew(async () =>
            {
                for (int i = 0; i < amount; i++)
                {
                    // return.Value
                    //      -1  出错
                    //      0   登录失败
                    //      1   登录成功
                    var login_result = await channel.RecvLoginAsync();
                    if (login_result.Value != 1)
                    {
                        DataModel.SetMessage($"Login 接收出错: {login_result.ErrorInfo}", "error");
                    }
                    else
                        DataModel.SetMessage("Login 接收成功");
                }
            },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default).Unwrap();

            // 等待全部 task 完成
            await Task.WhenAll(new Task[] { send_task, recv_task });

            DataModel.SetMessage("释放通道");
            channel.Close();

            DataModel.SetMessage("TestLoginPipeline() 结束");
            return new NormalResult();
        }
    }
}
