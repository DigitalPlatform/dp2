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
            sizes.Add("盘点");
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

    public class CachePasswordLengthSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("无");
            sizes.Add("1min");
            sizes.Add("5min");
            sizes.Add("10min");
            return sizes;
        }
    }

    public class EncodingItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("utf-8");
            sizes.Add("gb2312");
            return sizes;
        }
    }

    public class SkinItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add("亮色");
            sizes.Add("暗色");
            return sizes;
        }
    }
}
