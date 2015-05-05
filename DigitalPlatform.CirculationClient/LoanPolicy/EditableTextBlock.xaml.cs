using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// EditableTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new UIPropertyMetadata());

        private void textBoxName_LostFocus(object sender, RoutedEventArgs e)
        {
#if NO
            var txtBlock = (TextBlock)((Grid)((TextBox)sender).Parent).Children[0];

            txtBlock.Visibility = Visibility.Visible;
            ((TextBox)sender).Visibility = Visibility.Collapsed;
#endif
            Editable = false;
        }

#if NO
        private void textBlockName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var txtBox = (TextBox)((Grid)((TextBlock)sender).Parent).Children[1];
            txtBox.Visibility = Visibility.Visible;
            ((TextBlock)sender).Visibility = Visibility.Collapsed;
        }
#endif
        public TextBlock TextBlock
        {
            get
            {
                foreach(UIElement control in this._grid.Children)
                {
                    if (control is TextBlock)
                        return (TextBlock)control;
                }

                return null;
            }
        }

        public TextBox TextBox
        {
            get
            {
                foreach (UIElement control in this._grid.Children)
                {
                    if (control is TextBox)
                        return (TextBox)control;
                }

                return null;
            }
        }

        public bool Editable
        {
            get
            {
                return this.TextBox.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value == true)
                {
                    this.TextBox.Visibility = System.Windows.Visibility.Visible;
                    this.TextBlock.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    this.TextBox.Visibility = System.Windows.Visibility.Collapsed;
                    this.TextBlock.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }
    }
}
