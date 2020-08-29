using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using DigitalPlatform.Text;

namespace dp2SSL
{
    public class PersonDataTemplateSelector : DataTemplateSelector
    {
        /*
        private EntityListControl parent;
        public PersonDataTemplateSelector(EntityListControl parent)
        {
            this.parent = parent;
        }*/

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            EntityCollection entities = (item as Entity).Container;
            string template = StringUtil.GetParameterByPrefix(entities.Style, "template", ":");
            if (string.IsNullOrEmpty(template))
                template = "LargeTemplate";

            return ((FrameworkElement)container).FindResource(template) as DataTemplate;

            /*
            if (item != null && item is Person)
            {
                Person person = item as Person;
                Window window = System.Windows.Application.Current.MainWindow;
                ListBox list = window.FindName("PeopleListBox") as ListBox;

                Person selectedPerson = list.SelectedItem as Person;
                if (selectedPerson != null && selectedPerson.FullName() == person.FullName())
                {
                    return window.FindResource("PeopleTemplateComplex") as DataTemplate;
                }
                else
                {
                    return window.FindResource("PeopleTemplateSimple") as DataTemplate;
                }
            }
            */

            return null;
        }

        /*
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        */
    }
}
