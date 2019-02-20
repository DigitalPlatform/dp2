using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace dp2SSL
{

    public class PasswordEditor : ITypeEditor
    {
        #region fields
        PropertyItem _propertyItem;
        PasswordBox _passwordBox;
        #endregion

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            _propertyItem = propertyItem;

            _passwordBox = new PasswordBox();
            _passwordBox.Password = (string)propertyItem.Value;
            _passwordBox.LostFocus += OnLostFocus;

            return _passwordBox;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // prevent event from bubbeling
            e.Handled = true;

            if (!_passwordBox.Password.Equals((string)_propertyItem.Value))
            {
                _propertyItem.Value = _passwordBox.Password;
            }
        }
    }

#if NO
    //Custom editors that are used as attributes MUST implement the ITypeEditor interface.
    public class PasswordEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            PasswordBox textBox = new PasswordBox();
            textBox.Background = new SolidColorBrush(Colors.Red);

            //create the binding from the bound property item to the editor
            var _binding = new Binding("Value"); //bind to the Value property of the PropertyItem
            _binding.Source = propertyItem;
            _binding.ValidatesOnExceptions = true;
            _binding.ValidatesOnDataErrors = true;
            _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, _binding);
            textBox.PasswordChar = '*';
            return textBox;
        }
    }

#endif
}
