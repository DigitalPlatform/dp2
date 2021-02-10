using DigitalPlatform.RFID;
using DigitalPlatform.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dp2SSL
{
    // 盘点主循环
    public static class InventoryLoop
    {
        volatile static bool _pause = false;

        // 暂停循环
        public static void PauseLoop()
        {
            _pause = true;
        }

        // 继续循环
        public static void ContinueLoop()
        {
            _pause = false;
        }

        public static void BeginLoop(CancellationToken token)
        {
            _ = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        while (token.IsCancellationRequested == false)
                        {
                            if (_pause)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                                continue;
                            }

                            // 语音提示倒计时开始盘点
                            await SpeakCounter(token);

                            string readerNameList = "*";
                            string list_style = $"dont_delay";   // 确保 inventory 并立即返回

                            var result = RfidManager.CallListTags(readerNameList, list_style);

                            if (result.Results == null)
                                result.Results = new List<OneTag>();
                            if (result.Value == -1)
                                ShowMessageBox("inventory", result.ErrorInfo);
                            else
                            {
                                ShowMessageBox("inventory", null);
                            }

                            int cross_count = 0;
                            int process_count = 0;
                            this.Invoke((Action)(() =>
                            {
                                var fill_result = FillTags(result.Results);
                                cross_count = fill_result.CrossCount;
                            }));

                            // TODO: 语音念出交叉的事项个数

                            // testing
                            // await SpeakPauseCounter(token);

                            // 语音或音乐提示正在处理，不要移动天线
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"修改循环出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                    finally
                    {
                        App.CurrentApp.Speak("停止修改");
                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            /*
            void ClearCache(List<OneTag> tags)
            {
                foreach (var tag in tags)
                {
                    ClearCacheTagTable(tag.UID);
                }
            }
            */
        }



        // 语音提示倒计时
        async static Task SpeakCounter(CancellationToken token)
        {
            for (int i = DataModel.BeforeScanSeconds; i > 0; i--)
            {
                App.CurrentApp.Speak($"{i}");
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            Console.Beep();
        }

        // 语音提示间隙时间，方便拿走从读写器上标签
        async static Task SpeakPauseCounter(CancellationToken token)
        {
            // 让上一句话说完
            await Task.Delay(TimeSpan.FromSeconds(3), token);

            App.CurrentApp.Speak($"暂停开始");
            await Task.Delay(TimeSpan.FromSeconds(3), token);
            for (int i = 5; i > 0; i--)
            {
                App.CurrentApp.Speak($"{i}");
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            App.CurrentApp.Speak($"暂停结束");
            await Task.Delay(TimeSpan.FromSeconds(3), token);
        }

        async static Task SpeakAdjust(string text, CancellationToken token)
        {
            App.CurrentApp.Speak(text);
            await Task.Delay(TimeSpan.FromSeconds(4), token);
        }
    }
}
