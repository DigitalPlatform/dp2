using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    public class MyPage : Page
    {
        List<Window> _dialogs = new List<Window>();

        public int GetDialogCount()
        {
            return _dialogs.Count;
        }

        internal void CloseDialogs()
        {
            // 确保 page 关闭时对话框能自动关闭
            App.Invoke(new Action(() =>
            {
                foreach (var window in _dialogs)
                {
                    window.Close();
                }
                _dialogs.Clear();
            }));
        }

        internal void MemoryDialog(Window dialog)
        {
            _dialogs.Add(dialog);
        }

        internal void ForgetDialog(Window dialog)
        {
            _dialogs.Remove(dialog);
        }

        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;

        internal void InitializeLayer(Visual visual)
        {
            _layer = AdornerLayer.GetAdornerLayer(visual);
            _adorner = new LayoutAdorner(this);
        }

        int _layerCount = 0;

        internal void AddLayer()
        {
            if (_adorner == null || _layer == null)
                return;
            if (_layerCount == 0)
            {
                try
                {
                    _layer.Add(_adorner);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"AddLayer() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            }
            App.PauseBarcodeMonitor();
            _layerCount++;
        }

        internal void RemoveLayer()
        {
            if (_adorner == null || _layer == null)
                return;
            App.ContinueBarcodeMonitor();
            _layerCount--;
            if (_layerCount == 0)
            {
                _layer.Remove(_adorner);
            }
        }

        #region 封面图像

        public class CoverItem
        {
            public string ObjectPath { get; set; }
            public Entity Entity { get; set; }
        }


        public static string CoverImagesDirectory
        {
            get
            {
                string cacheDir = Path.Combine(WpfClientInfo.UserDir, "coverImages");
                PathUtil.CreateDirIfNeed(cacheDir);
                return cacheDir;
            }
        }

        public static void BeginCleanCoverImagesDirectory(DateTime time)
        {
            _ = Task.Factory.StartNew(() =>
            {
                var dir = CoverImagesDirectory;
                try
                {
                    ClearDir(dir, time, App.CancelToken);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"ClearDir({dir}) 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            },
App.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // 删除一个目录内的所有文件和目录
        // 可能会抛出异常
        // parameters:
        //      time    要清除这个时间点以前创建、修改过的文件和子目录
        static void ClearDir(string strDir,
            DateTime time,
            CancellationToken token)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
                return;

            // 如果 strDir 是根目录，拒绝进行删除
            if (PathUtil.IsEqual(di.Root.FullName, di.FullName))
                return;

            // 删除所有的下级目录
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo childDir in dirs)
            {
                token.ThrowIfCancellationRequested();

                if (childDir.LastWriteTime < time)
                {
                    try
                    {
                        Directory.Delete(childDir.FullName, true);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"ClearDir(删除子目录 '{childDir.FullName}') 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }
            }

            // 删除所有文件
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                token.ThrowIfCancellationRequested();

                if (fi.LastWriteTime < time)
                {
                    try
                    {
                        File.Delete(fi.FullName);
                    }
                    catch (Exception ex)
                    {
                        WpfClientInfo.WriteErrorLog($"ClearDir(删除文件 '{fi.FullName}') 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                }
            }
        }

        // 计算磁盘剩余空间
        public static long GetUserDiskFreeSpace()
        {
            try
            {
                var root = Path.GetPathRoot(CoverImagesDirectory);
                DriveInfo driveInfo = new DriveInfo(root);
                return driveInfo.AvailableFreeSpace;
            }
            catch (System.IO.IOException ex)
            {
                return -1;
            }
        }

        public static string GetImageFilePath(string text)
        {
            if (StringUtil.IsHttpUrl(text))
                return StringUtil.GetMd5(text);
            return text.Replace("/", "_").Replace("\\", "_");
        }

        #endregion
    }
}
