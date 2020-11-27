using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;

namespace RfidTool
{
    /// <summary>
    /// 数据模型
    /// </summary>
    public static class DataModel
    {
        static CancellationTokenSource _cancelRfidManager = new CancellationTokenSource();

        public static NewTagList TagList = new NewTagList();

        public static void StartRfidManager(string url)
        {
            _cancelRfidManager?.Cancel();

            _cancelRfidManager = new CancellationTokenSource();
            RfidManager.Base.Name = "RFID 中心";
            RfidManager.Url = url;
            // RfidManager.AntennaList = "1|2|3|4";    // testing
            // RfidManager.SetError += RfidManager_SetError;
            RfidManager.ListTags += RfidManager_ListTags;
            RfidManager.Start(_cancelRfidManager.Token);
        }

        public static event NewTagChangedEventHandler TagChanged = null;

        private static void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 标签总数显示
            if (e.Result.Results != null)
            {
                TagList.Refresh(// sender as BaseChannel<IRfid>,
                    e.ReaderNameList,
                    e.Result.Results,
                    (readerName, uid, antennaID) =>
                    {
                        var channel = sender as BaseChannel<IRfid>;
                        return channel.Object.GetTagInfo(readerName, uid, antennaID);
                    },
                    (add_tags, update_tags, remove_tags) =>
                    {
                        TagChanged?.Invoke(sender, new NewTagChangedEventArgs
                        {
                            AddTags = add_tags,
                            UpdateTags = update_tags,
                            RemoveTags = remove_tags,
                            Source = e.Source,
                        });
                    },
                    (type, text) =>
                    {
                        RfidManager.TriggerSetError(null/*this*/, new SetErrorEventArgs { Error = text });
                    });
            }
        }

        public static void StopRfidManager()
        {
            _cancelRfidManager?.Cancel();
            RfidManager.Url = "";
            RfidManager.ListTags -= RfidManager_ListTags;
        }
    }

    public delegate void NewTagChangedEventHandler(object sender,
NewTagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class NewTagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddTags { get; set; }
        public List<TagAndData> UpdateTags { get; set; }
        public List<TagAndData> RemoveTags { get; set; }
        public string Source { get; set; }   // 触发者
    }
}
