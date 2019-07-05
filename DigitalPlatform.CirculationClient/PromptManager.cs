using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    // 用于处理 Prompt 交互的类
    public class PromptManager
    {
        bool _hide_dialog = false;
        int _hide_dialog_count = 0;

        public PromptManager(int max_retry)
        {
            _max_retry = max_retry;
        }

        int _max_retry = 2;
        public int MaxRetry
        {
            get
            {
                return _max_retry;
            }
            set
            {
                _max_retry = value;
            }
        }

        // 弹出对话框并自动重试的次数
        int _retry_count = 0;

        public void Prompt(Form owner, MessagePromptEventArgs e)
        {
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = DialogResult.Yes;
                if (_hide_dialog == false)
                {
                    owner.Invoke((Action)(() =>
                    {
                        result = MessageDialog.Show(owner,
                            e.MessageText +
                            (e.IncludeOperText == false ? "\r\n\r\n(重试) 重试操作;(跳过) 跳过本条继续处理后面的书目记录; (中断) 中断处理" : ""),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxDefaultButton.Button1,
                    "此后不再出现本对话框",
                    ref _hide_dialog,
                    new string[] { "重试", "跳过", "中断" },
                    10);
                    }));
                    _hide_dialog_count = 0;

                    if (result == DialogResult.Yes)
                    {
                        _retry_count++;
                        if (_retry_count >= _max_retry)
                        {
                            result = DialogResult.Cancel;
                            _retry_count = 0;
                        }
                    }
                }
                else
                {
                    _hide_dialog_count++;
                    if (_hide_dialog_count > 10)
                        _hide_dialog = false;
                }

                if (result == DialogResult.Yes)
                    e.ResultAction = "yes";
                else if (result == DialogResult.Cancel)
                    e.ResultAction = "cancel";
                else
                    e.ResultAction = "no";
            }

            if (e.Actions == "yes,cancel")
            {
                DialogResult result = DialogResult.Yes;
                if (_hide_dialog == false)
                {
                    owner.Invoke((Action)(() =>
                    {
                        result = MessageDialog.Show(owner,
                                    e.MessageText +
        (e.IncludeOperText == false ? "\r\n\r\n是否跳过本条继续后面操作?\r\n\r\n(跳过: 跳过并继续; 中断: 停止全部操作)" : ""),
                    MessageBoxButtons.OKCancel,
                    MessageBoxDefaultButton.Button1,
                    "此后不再出现本对话框",
                    ref _hide_dialog,
                    new string[] { "跳过", "中断" },
                    10);
                    }));
                    _hide_dialog_count = 0;

                    if (result == DialogResult.OK)
                    {
                        _retry_count++;
                        if (_retry_count >= _max_retry)
                        {
                            result = DialogResult.Cancel;
                            _retry_count = 0;
                        }
                    }
                }
                else
                {
                    _hide_dialog_count++;
                    if (_hide_dialog_count > 10)
                        _hide_dialog = false;
                }

                if (result == DialogResult.OK)
                    e.ResultAction = "yes";
                else
                    e.ResultAction = "cancel";
            }

            /*
            // TODO: 自动延时以后重试
            this.Invoke((Action)(() =>
            {
                if (e.Actions == "yes,cancel")
                {
                    DialogResult result = MessageBox.Show(this,
        e.MessageText +
        (e.IncludeOperText == false ? "\r\n\r\n是否跳过本条继续后面操作?\r\n\r\n(确定: 跳过并继续; 取消: 停止全部操作)" : ""),
        "MainForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.OK)
                        e.ResultAction = "yes";
                    else // if (result == DialogResult.Cancel)
                        e.ResultAction = "cancel";
                    return;
                }

                // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
                if (e.Actions == "yes,no,cancel")
                {
                    DialogResult result = MessageBox.Show(this,
        e.MessageText +
        (e.IncludeOperText == false ? "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 取消: 停止全部操作)" : ""),
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                        e.ResultAction = "yes";
                    else if (result == DialogResult.Cancel)
                        e.ResultAction = "cancel";
                    else
                        e.ResultAction = "no";
                }
            }));
            */
        }


    }
}
