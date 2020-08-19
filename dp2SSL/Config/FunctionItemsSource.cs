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
            sizes.Add("自助借还");
            sizes.Add("智能书柜");
            return sizes;
        }
    }

    public class CardNumberConvertItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("十进制");
            sizes.Add("十六进制");
            return sizes;
        }
    }

    public class PatronBarcodeStyleSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("禁用");
            sizes.Add("一维码+二维码");
            sizes.Add("一维码");
            sizes.Add("二维码");
            return sizes;
        }
    }

    public class PosPrintStyleSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("不打印");
            sizes.Add("借书");
            sizes.Add("借书+还书");
            return sizes;
        }
    }
}
