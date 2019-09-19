using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Interfaces;

namespace CallFaceCenterSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // 人脸中心 .NET Remoting URL
        static string facecenter_url = "ipc://FaceChannel/FaceServer";

        // 人脸识别任务对象
        Task _recognitionTask = null;

        // 第一种方法：利用 facecenter 窗口自动翻起来显示视频
        private void button_faceRecognition1_Click(object sender, EventArgs e)
        {
            // 用单独任务进行人脸识别，这样可以不阻塞界面线程
            _recognitionTask = Task.Run(() =>
            {
                var result = Recognition(facecenter_url, "ui");
                ShowMessageBox(result.ToString());
                _recognitionTask = null;
            });
        }

        void ShowMessageBox(string text)
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));
        }

        // 人脸识别 API
        RecognitionFaceResult Recognition(string url, string style)
        {
            FaceChannel channel = StartFaceChannel(
    url,
    out string strError);
            if (channel == null)
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                return channel.Object.RecognitionFace(style);
            }
            catch (Exception ex)
            {
                strError = $"针对 {url} 的 RecongitionFace() 操作失败: { ex.Message}";
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFaceChannel(channel);
            }
        }

        // 取消人脸识别任务 API
        NormalResult CancelRecognition(string url)
        {
            FaceChannel channel = StartFaceChannel(
    url,
    out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                return channel.Object.CancelRecognitionFace();
            }
            catch (Exception ex)
            {
                strError = $"针对 {url} 的 CancelRecognitionFace() 操作失败: { ex.Message}";
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFaceChannel(channel);
            }
        }

        NormalResult DisplayVideo(string url, CancellationToken token)
        {
            FaceChannel channel = StartFaceChannel(
    url,
    out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                while (token.IsCancellationRequested == false)
                {
                    var result = channel.Object.GetImage("");
                    if (result.Value == -1)
                        return result;
                    using (MemoryStream stream = new MemoryStream(result.ImageData))
                    {
                        this.pictureBox1.Image = Image.FromStream(stream);
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = $"针对 {url} 的 GetImage() 请求失败: { ex.Message}";
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFaceChannel(channel);
            }
        }


        #region 人脸识别有关功能

        public class FaceChannel
        {
            public IpcClientChannel Channel { get; set; }
            public IBioRecognition Object { get; set; }
        }

        public FaceChannel StartFaceChannel(
    string strUrl,
    out string strError)
        {
            strError = "";

            FaceChannel result = new FaceChannel();

            result.Channel = new IpcClientChannel(Guid.NewGuid().ToString(), // 随机的名字，令多个 Channel 对象可以并存 
                    new BinaryClientFormatterSinkProvider());

            ChannelServices.RegisterChannel(result.Channel, true);
            bool bDone = false;
            try
            {
                result.Object = (IBioRecognition)Activator.GetObject(typeof(IBioRecognition),
                    strUrl);
                if (result.Object == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return null;
                }
                bDone = true;
                return result;
            }
            finally
            {
                if (bDone == false)
                    EndFaceChannel(result);
            }
        }

        public void EndFaceChannel(FaceChannel channel)
        {
            if (channel != null && channel.Channel != null)
            {
                ChannelServices.UnregisterChannel(channel.Channel);
                channel.Channel = null;
            }
        }

#if NO
        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        public async Task<RecognitionFaceResult> RecognitionFace(string strStyle)
        {
            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息"
                };
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out string strError);
            if (channel == null)
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                return await Task.Factory.StartNew<RecognitionFaceResult>(
                    () =>
                    {
                        return channel.Object.RecognitionFace(strStyle);
                    });
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 RecongitionFace() 操作失败: " + ex.Message;
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFaceChannel(channel);
            }
        }

        public async Task<NormalResult> FaceGetState(string strStyle)
        {
            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == true)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "尚未配置 人脸识别接口URL 系统参数，无法读取人脸信息"
                };
            }

            FaceChannel channel = StartFaceChannel(
                Program.MainForm.FaceReaderUrl,
                out string strError);
            if (channel == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            try
            {
                return await Task.Factory.StartNew<NormalResult>(
                    () =>
                    {
                        return channel.Object.GetState(strStyle);
                    });
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FaceReaderUrl + " 的 GetState() 操作失败: " + ex.Message;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }
            finally
            {
                EndFaceChannel(channel);
            }
        }

#endif

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 记得取消没有完成的人脸识别任务
            if (_recognitionTask != null)
            {
                CancelRecognition(facecenter_url);
            }

            CancelDislayVideo();
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        Task _taskDisplayVideo = null;

        void CancelDislayVideo()
        {
            if (_cancel != null)
            {
                _cancel.Cancel();
                _cancel.Dispose();
                _cancel = null;
            }
        }

        private void button_startVideo_Click(object sender, EventArgs e)
        {
            CancelDislayVideo();

            _cancel = new CancellationTokenSource();
            _taskDisplayVideo = Task.Run(()=> {
                var result = DisplayVideo(facecenter_url, _cancel.Token);
                if (_cancel != null && _cancel.IsCancellationRequested == false)
                    ShowMessageBox(result.ToString());
            });
        }

        private void button_stopVideo_Click(object sender, EventArgs e)
        {
            CancelDislayVideo();
        }
    }
}
