using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform;

namespace sipApiTester
{
    public static class TestConnect
    {
        public static NormalResult TestAll()
        {
            return new NormalResult();
        }

        // 单纯 Connect() 连接 SIP Server
        public static async Task<NormalResult> TestConnect1(int amount)
        {
            DataModel.SetMessage("TestConnect1() 开始");

            if (Int32.TryParse(DataModel.sipServerPort, out int port) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"端口号 '{DataModel.sipServerPort}' 格式错误。应为整数"
                };

            List<SipChannel> channels = new List<SipChannel>();
            for (int i = 0; i < amount; i++)
            {
                var channel = new SipChannel(Encoding.UTF8);
                channels.Add(channel);

                var result = await channel.ConnectAsync(DataModel.sipServerAddr, port);
                if (result.Value == -1)
                    DataModel.SetMessage($"Connect 出错: {result.ErrorInfo}", "error");
            }

            foreach (var channel in channels)
            {
                channel.Close();
            }

            DataModel.SetMessage("TestConnect1() 结束");
            return new NormalResult();
        }

        // Connect 然后 Login。顺序完成
        public static async Task<NormalResult> TestConnectAndLogin(int amount)
        {
            DataModel.SetMessage("TestConnectAndLogin() 开始");

            if (Int32.TryParse(DataModel.sipServerPort, out int port) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"端口号 '{DataModel.sipServerPort}' 格式错误。应为整数"
                };

            List<SipChannel> channels = new List<SipChannel>();
            for (int i = 0; i < amount; i++)
            {
                var channel = new SipChannel(Encoding.UTF8);
                channels.Add(channel);

                var result = await channel.ConnectAsync(DataModel.sipServerAddr, port);
                if (result.Value == -1)
                    DataModel.SetMessage($"Connect 出错: {result.ErrorInfo}", "error");
            }

            foreach (var channel in channels)
            {
                // return.Value
                //      -1  出错
                //      0   登录失败
                //      1   登录成功
                var login_result = await channel.LoginAsync(DataModel.sipUserName, DataModel.sipPassword);
                if (login_result.Value != 1)
                {
                    DataModel.SetMessage($"Login 出错: {login_result.ErrorInfo}", "error");
                    /*
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = login_result.ErrorInfo
                    };
                    */
                }
                else
                    DataModel.SetMessage("Login 成功");
            }

            foreach (var channel in channels)
            {
                channel.Close();
            }

            DataModel.SetMessage("TestConnectAndLogin() 结束");
            return new NormalResult();
        }

        // Connect 然后 Login。Login 并发完成
        public static async Task<NormalResult> TestConnectAndLoginConcurrent(int amount)
        {
            DataModel.SetMessage("TestConnectAndLogin() 开始");

            if (Int32.TryParse(DataModel.sipServerPort, out int port) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"端口号 '{DataModel.sipServerPort}' 格式错误。应为整数"
                };

            List<SipChannel> channels = new List<SipChannel>();
            for (int i = 0; i < amount; i++)
            {
                var channel = new SipChannel(Encoding.UTF8);
                channels.Add(channel);

                var result = await channel.ConnectAsync(DataModel.sipServerAddr, port);
                if (result.Value == -1)
                    DataModel.SetMessage($"Connect 出错: {result.ErrorInfo}", "error");
            }

            List<Task> tasks = new List<Task>();
            foreach (var channel in channels)
            {
                var task = Task.Factory.StartNew(async () =>
                {
                    // return.Value
                    //      -1  出错
                    //      0   登录失败
                    //      1   登录成功
                    var login_result = await channel.LoginAsync(DataModel.sipUserName, DataModel.sipPassword);
                    if (login_result.Value != 1)
                    {
                        DataModel.SetMessage($"Login 出错: {login_result.ErrorInfo}", "error");
                        /*
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = login_result.ErrorInfo
                        };
                        */
                    }
                    else
                        DataModel.SetMessage("Login 成功");
                },
default,
TaskCreationOptions.LongRunning,
TaskScheduler.Default).Unwrap();
                tasks.Add(task);
            }

            // 等待全部 task 完成
            await Task.WhenAll(tasks.ToArray());

            DataModel.SetMessage("释放所有通道");

            foreach (var channel in channels)
            {
                channel.Close();
            }

            DataModel.SetMessage("TestConnectAndLogin() 结束");
            return new NormalResult();
        }

    }
}
