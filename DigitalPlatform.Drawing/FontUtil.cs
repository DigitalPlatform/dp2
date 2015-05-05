using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{
    public static class FontUtil
    {
        // 根据一个字体字符串获得一个 Font 对象
        // 不能处理动态加载的条码字体
        public static Font BuildFont(string strFontString)
        {
            if (String.IsNullOrEmpty(strFontString) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strFontString);
            }
            else
            {
                return Control.DefaultFont;
            }
        }

        // 获得一个描述字体的字符串
        public static string GetFontString(Font font)
        {
            if (font == null)
                return "";  // Convertor 会返回 (无)
            // Create the FontConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

            return converter.ConvertToString(font);
        }
    }
}
