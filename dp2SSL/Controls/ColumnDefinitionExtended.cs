using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace dp2SSL
{
    // https://www.codeproject.com/Articles/437237/WPF-Grid-Column-and-Row-Hiding#:~:text=WPF%20Grid%20Column%20and%20Row%20Hiding%201%20Introduction,example%20of%20how%20to%20use%20the%20code...%20
    public class ColumnDefinitionExtended : ColumnDefinition
    {
        // Variables
        public static DependencyProperty VisibleProperty;

        // Properties
        public Boolean Visible
        {
            get { return (Boolean)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        // Constructors
        static ColumnDefinitionExtended()
        {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(Boolean),
                typeof(ColumnDefinitionExtended),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));

            ColumnDefinition.WidthProperty.OverrideMetadata(typeof(ColumnDefinitionExtended),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceWidth)));

            ColumnDefinition.MinWidthProperty.OverrideMetadata(typeof(ColumnDefinitionExtended),
                new FrameworkPropertyMetadata((Double)0, null,
                    new CoerceValueCallback(CoerceMinWidth)));
        }

        // Get/Set
        public static void SetVisible(DependencyObject obj, Boolean nVisible)
        {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static Boolean GetVisible(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            obj.CoerceValue(ColumnDefinition.WidthProperty);
            obj.CoerceValue(ColumnDefinition.MinWidthProperty);
        }
        static Object CoerceWidth(DependencyObject obj, Object nValue)
        {
            return (((ColumnDefinitionExtended)obj).Visible) ? nValue : new GridLength(0);
        }
        static Object CoerceMinWidth(DependencyObject obj, Object nValue)
        {
            return (((ColumnDefinitionExtended)obj).Visible) ? nValue : (Double)0;
        }
    }
}
