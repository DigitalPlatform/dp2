using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace dp2KernelApiTester.TestCase
{
    // 测试多个通道并发检索
    public static class TestMultiChannel
    {
        static string _strDatabaseName = "中文图书";

        public static NormalResult TestAll(
            CancellationToken token,
            string style = null)
        {
            {
                var result = PrepareEnvironment();
                if (result.Value == -1)
                    return result;
            }

            {
                var result = MultiChannelSearch(token);
                if (result.Value == -1)
                    return result;
            }

            {
                var result = Finish();
                if (result.Value == -1)
                    return result;
            }

            return new NormalResult();
        }

        static string[] database_names = new string[] {
                _strDatabaseName };

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            DataModel.SetMessage("准备环境成功", "green");
            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult Finish()
        {
            string strError = "";

            var channel = DataModel.GetChannel();

            DataModel.SetMessage("清理环境成功", "green");
            return new NormalResult();
        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }

        public static NormalResult MultiChannelSearch(CancellationToken token)
        {
            int COUNT = 100;
            List<RmsChannel> channels = new List<RmsChannel>();
            for (int i = 0; i < COUNT; i++)
            {
                var channel = DataModel.NewChannel();
                channels.Add(channel);
            }

            token.ThrowIfCancellationRequested();

            try
            {

                List<Task> tasks = new List<Task>();
                int j = 0;
                foreach (var channel in channels)
                {
                    string word = "ttttt";
                    string strQueryXml = $"<target list='{_strDatabaseName}:题名'><item><word>{word}</word><match>middle</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                    var task = Task.Factory.StartNew((index) =>
                    {
                        var start = DateTime.UtcNow;
                        var ret = channel.DoSearch(strQueryXml, "default", out string strError);
                        if (ret == -1)
                        {
                            DataModel.SetMessage($"{index} DoSearch() 出错: {strError}", "error");
                        }
                        else
                            DataModel.SetMessage($"线程 {index} 已经结束，耗时 {DateTime.UtcNow - start}", "green");
                    },
                    j,
                    token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                    tasks.Add(task);
                    j++;
                }

                Task.WaitAll(tasks.ToArray(), token);
                DataModel.SetMessage("所有线程已经结束", "green");
            }
            finally
            {
                foreach(var channel in channels)
                {
                    channel.Dispose();
                }
            }

            return new NormalResult();
        }

    }

}
