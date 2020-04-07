using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace dp2SSL
{
    public class StateToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((string)value == "borrowed")
                return new SolidColorBrush(Colors.Transparent);

            if ((string)value == "onshelf")
                return new SolidColorBrush(Colors.DarkGreen);

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*
应用程序: dp2SSL.exe
Framework 版本: v4.0.30319
说明: 由于未经处理的异常，进程终止。
异常信息: System.IO.FileLoadException
   在 dp2SSL.StateToIconConverter.Convert(System.Object, System.Type, System.Object, System.Globalization.CultureInfo)
   在 System.Windows.Data.BindingExpression.TransferValue(System.Object, Boolean)
   在 System.Windows.Data.BindingExpression.Activate(System.Object)
   在 System.Windows.Data.BindingExpression.AttachToContext(AttachAttempt)
   在 System.Windows.Data.BindingExpression.AttachOverride(System.Windows.DependencyObject, System.Windows.DependencyProperty)
   在 System.Windows.Data.BindingExpressionBase.OnAttach(System.Windows.DependencyObject, System.Windows.DependencyProperty)
   在 System.Windows.StyleHelper.GetInstanceValue(System.Windows.UncommonField`1&lt;System.Collections.Specialized.HybridDictionary[]&gt;, System.Windows.DependencyObject, System.Windows.FrameworkElement, System.Windows.FrameworkContentElement, Int32, System.Windows.DependencyProperty, Int32, System.Windows.EffectiveValueEntry ByRef)
   在 System.Windows.FrameworkTemplate.ReceivePropertySet(System.Object, System.Xaml.XamlMember, System.Object, System.Windows.DependencyObject)
   在 System.Windows.FrameworkTemplate+&lt;&gt;c__DisplayClass45_0.&lt;LoadOptimizedTemplateContent&gt;b__3(System.Object, System.Windows.Markup.XamlSetValueEventArgs)
   在 System.Xaml.XamlObjectWriter.OnSetValue(System.Object, System.Xaml.XamlMember, System.Object)
   在 System.Xaml.XamlObjectWriter.Logic_ApplyPropertyValue(MS.Internal.Xaml.Context.ObjectWriterContext, System.Xaml.XamlMember, System.Object, Boolean)
   在 System.Xaml.XamlObjectWriter.Logic_DoAssignmentToParentProperty(MS.Internal.Xaml.Context.ObjectWriterContext)
   在 System.Xaml.XamlObjectWriter.Logic_AssignProvidedValue(MS.Internal.Xaml.Context.ObjectWriterContext)
   在 System.Xaml.XamlObjectWriter.WriteEndObject()
   在 System.Windows.FrameworkTemplate.LoadTemplateXaml(System.Xaml.XamlReader, System.Xaml.XamlObjectWriter)

异常信息: System.Windows.Markup.XamlParseException
   在 System.Windows.Markup.XamlReader.RewrapException(System.Exception, System.Xaml.IXamlLineInfo, System.Uri)
   在 System.Windows.FrameworkTemplate.LoadTemplateXaml(System.Xaml.XamlReader, System.Xaml.XamlObjectWriter)
   在 System.Windows.FrameworkTemplate.LoadTemplateXaml(System.Xaml.XamlObjectWriter)
   在 System.Windows.FrameworkTemplate.LoadOptimizedTemplateContent(System.Windows.DependencyObject, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector, System.Collections.Generic.List`1&lt;System.Windows.DependencyObject&gt;, System.Windows.UncommonField`1&lt;System.Collections.Hashtable&gt;)
   在 System.Windows.FrameworkTemplate.LoadContent(System.Windows.DependencyObject, System.Collections.Generic.List`1&lt;System.Windows.DependencyObject&gt;)
   在 System.Windows.StyleHelper.ApplyTemplateContent(System.Windows.UncommonField`1&lt;System.Collections.Specialized.HybridDictionary[]&gt;, System.Windows.DependencyObject, System.Windows.FrameworkElementFactory, Int32, System.Collections.Specialized.HybridDictionary, System.Windows.FrameworkTemplate)
   在 System.Windows.FrameworkTemplate.ApplyTemplateContent(System.Windows.UncommonField`1&lt;System.Collections.Specialized.HybridDictionary[]&gt;, System.Windows.FrameworkElement)
   在 System.Windows.FrameworkElement.ApplyTemplate()
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.Grid.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.Control.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.Grid.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.Border.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.Control.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.Controls.WrapPanel.MeasureOverride(System.Windows.Size)
   在 System.Windows.FrameworkElement.MeasureCore(System.Windows.Size)
   在 System.Windows.UIElement.Measure(System.Windows.Size)
   在 System.Windows.ContextLayoutManager.UpdateLayout()
   在 System.Windows.ContextLayoutManager.UpdateLayoutCallback(System.Object)
   在 System.Windows.Media.MediaContext.FireInvokeOnRenderCallbacks()
   在 System.Windows.Media.MediaContext.RenderMessageHandlerCore(System.Object)
   在 System.Windows.Media.MediaContext.RenderMessageHandler(System.Object)
   在 System.Windows.Threading.ExceptionWrapper.InternalRealCall(System.Delegate, System.Object, Int32)
   在 System.Windows.Threading.ExceptionWrapper.TryCatchWhen(System.Object, System.Delegate, System.Object, Int32, System.Delegate)
   在 System.Windows.Threading.DispatcherOperation.InvokeImpl()
   在 MS.Internal.CulturePreservingExecutionContext.CallbackWrapper(System.Object)
   在 System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   在 System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object, Boolean)
   在 System.Threading.ExecutionContext.Run(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)
   在 MS.Internal.CulturePreservingExecutionContext.Run(MS.Internal.CulturePreservingExecutionContext, System.Threading.ContextCallback, System.Object)
   在 System.Windows.Threading.DispatcherOperation.Invoke()
   在 System.Windows.Threading.Dispatcher.ProcessQueue()
   在 System.Windows.Threading.Dispatcher.WndProcHook(IntPtr, Int32, IntPtr, IntPtr, Boolean ByRef)
   在 MS.Win32.HwndWrapper.WndProc(IntPtr, Int32, IntPtr, IntPtr, Boolean ByRef)
   在 MS.Win32.HwndSubclass.DispatcherCallbackOperation(System.Object)
   在 System.Windows.Threading.ExceptionWrapper.InternalRealCall(System.Delegate, System.Object, Int32)
   在 System.Windows.Threading.ExceptionWrapper.TryCatchWhen(System.Object, System.Delegate, System.Object, Int32, System.Delegate)
   在 System.Windows.Threading.Dispatcher.LegacyInvokeImpl(System.Windows.Threading.DispatcherPriority, System.TimeSpan, System.Delegate, System.Object, Int32)
   在 MS.Win32.HwndSubclass.SubclassWndProc(IntPtr, Int32, IntPtr, IntPtr)
   在 MS.Win32.UnsafeNativeMethods.DispatchMessage(System.Windows.Interop.MSG ByRef)
   在 System.Windows.Threading.Dispatcher.PushFrameImpl(System.Windows.Threading.DispatcherFrame)
   在 System.Windows.Application.RunDispatcher(System.Object)
   在 System.Windows.Application.RunInternal(System.Windows.Window)
   在 dp2SSL.App.Main()
   * */
    public class StateToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (StringUtil.IsInList("borrowed", (string)value))
                    return FontAwesome.WPF.FontAwesomeIcon.AddressBook;

                // return FontAwesome.WPF.FontAwesomeIcon.HandPaperOutline;

                return FontAwesome.WPF.FontAwesomeIcon.Cube;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> results = new List<string>();
            var list = StringUtil.SplitList((string)value);
            foreach(string s in list)
            {
                if (s == "overflow")
                    results.Add("超额");
                if (s == "overdue")
                    results.Add("超期");
            }

            return StringUtil.MakePathList(results);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
