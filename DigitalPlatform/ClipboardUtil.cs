using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DigitalPlatform
{
    public class ClipboardUtil
    {
        /*
        public static string GetClipboardText()
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                return "";
            return (string)ido.GetData(DataFormats.UnicodeText);
        }
        */

        public static string GetClipboardText()
        {
            string result = "";

            RunClipboard(() =>
            {
                IDataObject ido = Clipboard.GetDataObject();
                if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                {
                    result = "";
                    return;
                }
                result = (string)ido.GetData(DataFormats.UnicodeText);
            });
            return result;
        }

        public static IDataObject GetDataObject()
        {
            IDataObject ido = null;

            RunClipboard(() =>
            {
                ido = Clipboard.GetDataObject();
            });

            return ido;
        }

        public static void SetClipboardText(object data_object)
        {
            RunClipboard(() =>
            {
                Clipboard.SetDataObject(data_object);
            });
        }

        public delegate void Delegate_clipboardFunc();

        public static void RunClipboard(Delegate_clipboardFunc func)
        {
            try
            {
                func();
                return;
            }
            catch
            {

            }

            // https://stackoverflow.com/questions/38421985/why-clipboard-setdataobject-doesnt-copy-object-to-the-clipboard-in-c-sharp
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        func();
                    }
                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            if (threadEx != null)
                throw threadEx;
        }

    }
}
