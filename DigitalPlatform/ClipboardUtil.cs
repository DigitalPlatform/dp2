using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public class ClipboardUtil
    {
        public static string GetClipboardText()
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                return "";
            return (string)ido.GetData(DataFormats.UnicodeText);
        }
    }
}
