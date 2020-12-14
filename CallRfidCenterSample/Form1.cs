using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;

namespace CallRfidCenterSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button_tagListForm_Click(object sender, EventArgs e)
        {
            TagListForm dlg = new TagListForm();
            dlg.ShowDialog(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartRfidManager("ipc://RfidChannel/RfidServer");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopRfidManager();
        }

        CancellationTokenSource _cancelRfidManager = new CancellationTokenSource();

        void StartRfidManager(string url)
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

        public static event TagChangedEventHandler TagChanged = null;

        public static NewTagList TagList = new NewTagList();

        private void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            // 标签总数显示
            if (e.Result.Results != null)
            {
                TagList.Refresh(
                    e.ReaderNameList,
                    e.Result.Results,
                    (readerName, uid, antennaID, protocol) =>
                    {
                        var channel = sender as BaseChannel<IRfid>;
                        return channel.Object.GetTagInfo(readerName, uid, antennaID);
                    },
                    (add_books, update_books, remove_books) =>
                    {
                        TagChanged?.Invoke(sender, new TagChangedEventArgs
                        {
                            AddBooks = add_books,
                            UpdateBooks = update_books,
                            RemoveBooks = remove_books,
                        });
                    },
                    (type, text) =>
                    {
                        RfidManager.TriggerSetError(this, new SetErrorEventArgs { Error = text });
                        // TagSetError?.Invoke(this, new SetErrorEventArgs { Error = text });
                    });
#if REMOVED
                TagList.Refresh(sender as BaseChannel<IRfid>,
                    e.ReaderNameList,
                    e.Result.Results,
                        (add_books, update_books, remove_books, add_patrons, update_patrons, remove_patrons) =>
                        {
                            TagChanged?.Invoke(sender, new TagChangedEventArgs
                            {
                                AddBooks = add_books,
                                UpdateBooks = update_books,
                                RemoveBooks = remove_books,
                                AddPatrons = add_patrons,
                                UpdatePatrons = update_patrons,
                                RemovePatrons = remove_patrons
                            });
                        },
                        (type, text) =>
                        {
                            RfidManager.TriggerSetError(this, new SetErrorEventArgs { Error = text });
                            // TagSetError?.Invoke(this, new SetErrorEventArgs { Error = text });
                        });
#endif

                // 标签总数显示 图书+读者卡
                // this.Number = $"{TagList.Books.Count}:{TagList.Patrons.Count}";
            }
        }

        void StopRfidManager()
        {
            _cancelRfidManager?.Cancel();
            RfidManager.Url = "";
            RfidManager.ListTags -= RfidManager_ListTags;
        }
    }

    public delegate void TagChangedEventHandler(object sender,
TagChangedEventArgs e);

    /// <summary>
    /// 设置标签变化事件的参数
    /// </summary>
    public class TagChangedEventArgs : EventArgs
    {
        public List<TagAndData> AddBooks { get; set; }
        public List<TagAndData> UpdateBooks { get; set; }
        public List<TagAndData> RemoveBooks { get; set; }

        public List<TagAndData> AddPatrons { get; set; }
        public List<TagAndData> UpdatePatrons { get; set; }
        public List<TagAndData> RemovePatrons { get; set; }
    }
}
