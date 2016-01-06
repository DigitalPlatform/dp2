using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

// using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryClient
{
#if NO
    /// <summary>
    /// 根据册条码号获得册记录路径的枚举器
    /// </summary>
    public class ItemBarcodeLoader : IEnumerable
    {
        List<string> _barcodes = new List<string>();

        public List<string> Barcodes
        {
            get
            {
                return this._barcodes;
            }
            set
            {
                this._barcodes = value;
            }
        }

        public LibraryChannel Channel
        {
            get;
            set;
        }

        public Stop Stop
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            string strError = "";
            List<string> barcodes = new List<string>();
            int i = 0;
            foreach (string strBarcode in this._barcodes)
            {
                barcodes.Add(strBarcode);
                if (barcodes.Count >= 100
                    || (i == this._barcodes.Count - 1 && barcodes.Count > 0))
                {
                    string strBiblio = "";
                    string strResult = "";

                    if (this.Channel == null)
                        throw new ArgumentException("Channel 成员不应为 null", "Channel");

                    long lRet = this.Channel.GetItemInfo(this.Stop,
                        "@barcode-list:" + string.Join(",", barcodes),
                        "get-path-list",
                        out strResult,
                        "", // strBiblioType,
                        out strBiblio,
                        out strError);
                    if (lRet == -1)
                        throw new Exception(strError);

                    List<string> recpaths = strResult.Split(new char[] { ',' }).ToList();
                    //    StringUtil.SplitList(strResult);

                    if (recpaths.Count == 0 && barcodes.Count == 1)
                        recpaths.Add("");
                    else
                    {
                        Debug.Assert(barcodes.Count == recpaths.Count, "");
                    }

                    for (int j = 0; j < recpaths.Count; j++)
                    {
                        string recpath = recpaths[j];
                        ItemBarcodeInfo info = new ItemBarcodeInfo();
                        info.RecPath = recpath;
                        info.ItemBarcode = barcodes[j];
                        yield return info;
                    }

                    barcodes.Clear();
                }

                i++;
            }

            Debug.Assert(barcodes.Count == 0, "");
        }
    }

    public class ItemBarcodeInfo
    {
        public string ItemBarcode { get; set; }
        public string RecPath { get; set; }
        public string ErrorInfo { get; set; }
    }
#endif
}
