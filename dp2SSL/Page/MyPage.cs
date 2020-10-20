using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace dp2SSL
{
    public class MyPage : Page
    {
        List<Window> _dialogs = new List<Window>();

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

        internal void AddLayer()
        {
            if (_adorner == null || _layer == null)
                return;
            try
            {
                _layer.Add(_adorner);
            }
            catch
            {

            }
        }

        internal void RemoveLayer()
        {
            if (_adorner == null || _layer == null)
                return;
            _layer.Remove(_adorner);
        }
    }
}
