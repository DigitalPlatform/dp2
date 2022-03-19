using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using DigitalPlatform;
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
            _layerCount++;
        }

        internal void RemoveLayer()
        {
            if (_adorner == null || _layer == null)
                return;
            _layerCount--;
            if (_layerCount == 0)
                _layer.Remove(_adorner);
        }
    }
}
