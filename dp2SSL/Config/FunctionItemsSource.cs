using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace dp2SSL
{
    public class FunctionItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("自助借还", "自助借还");
            sizes.Add("智能书柜", "智能书柜");
            return sizes;
        }
    }
}
