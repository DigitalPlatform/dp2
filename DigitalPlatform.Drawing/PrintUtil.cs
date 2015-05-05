using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Printing;
using System.ComponentModel.Design.Serialization;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace DigitalPlatform.Drawing
{
    public static class PrintUtil
    {
        // 根据描述字符串构造一个 PaperSize 对象
        public static PaperSize BuildPaperSize(string strFontString)
        {
            if (String.IsNullOrEmpty(strFontString) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter = new PageSizeConverter();
                    // System.ComponentModel.TypeDescriptor.GetConverter(typeof(PaperSize));

                return (PaperSize)converter.ConvertFromString(strFontString);
            }
            else
            {
                return null;
            }
        }

        // 获得一个描述纸张尺寸的字符串
        public static string GetPaperSizeString(PaperSize paper_size)
        {
            System.ComponentModel.TypeConverter converter = new PageSizeConverter();
                // System.ComponentModel.TypeDescriptor.GetConverter(typeof(PaperSize));

            return converter.ConvertToString(paper_size);
        }

    }

    public class PageSizeConverter : TypeConverter
    {
        // Overrides the CanConvertFrom method of TypeConverter.
        // The ITypeDescriptorContext interface provides the context for the
        // conversion. Typically, this interface is used at design time to 
        // provide information about the design-time container.
        public override bool CanConvertFrom(ITypeDescriptorContext context,
           Type sourceType)
        {

            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context,
           CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] v = ((string)value).Split(new char[] { ',' });
                if (v.Length < 3)
                    return null;
                return new PaperSize(v[0], int.Parse(v[1]), int.Parse(v[2]));
            }
            return base.ConvertFrom(context, culture, value);
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
           CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ((PaperSize)value).PaperName + "," + ((PaperSize)value).Width + "," + ((PaperSize)value).Height;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

#if NO
    public class PageSizeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
        CultureInfo culture, object value, Type destinationType)
        {
            // Insert other ConvertTo operations here.
            //
            if (destinationType == typeof(InstanceDescriptor) &&
      value is PaperSize)
            {
                PaperSize pt = (PaperSize)value;

                ConstructorInfo ctor = typeof(PaperSize).GetConstructor(
          new Type[] { typeof(string), typeof(int), typeof(int) });
                if (ctor != null)
                {
                    return new InstanceDescriptor(ctor, new object[] { pt.PaperName, pt.Width, pt.Height });
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

#endif
}

