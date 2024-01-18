using DigitalPlatform.RFID;
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
            ItemCollection items = new ItemCollection();
            items.Add("自助借还");
            items.Add("智能书柜");
            items.Add("盘点");
            return items;
        }
    }

    public class CardNumberConvertItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("十进制");
            items.Add("十六进制");
            return items;
        }
    }

    public class PatronBarcodeStyleSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("禁用");
            items.Add("一维码+二维码");
            items.Add("一维码");
            items.Add("二维码");
            return items;
        }
    }

    public class PosPrintStyleSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("不打印");
            items.Add("借书");
            items.Add("借书+还书");
            return items;
        }
    }

    public class CachePasswordLengthSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("无");
            items.Add("1min");
            items.Add("5min");
            items.Add("10min");
            return items;
        }
    }

    public class EncodingItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("utf-8");
            items.Add("gb2312");
            return items;
        }
    }

    public class SkinItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("亮色");
            items.Add("暗色");
            return items;
        }
    }

    public class FaceInputMultipleItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            items.Add("使用第一个");
            items.Add("列表选择+密码筛选");
            items.Add("列表选择");
            items.Add("密码筛选");
            return items;
        }
    }

    // 当前全部读写器名字列表
    public class ReaderNameItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection items = new ItemCollection();
            var result = RfidManager.ListReaders();
            if (result.Value == -1)
                return items;
            items.Add("");
            foreach (var reader in result.Readers)
            {
                items.Add(reader);
            }

            return items;
        }
    }
}
