using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Web;

using AsyncPluggableProtocol;

using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Common;

namespace DigitalPlatform.Drawing
{
    public class BarcodeProtocol : IProtocol
    {
        public string Name
        {
            get
            {
                return "barcode";
            }
        }

        public BarcodeProtocol()
        {
        }


        public Task<Stream> GetStreamAsync(string url)
        {
            string path = url.Substring(Name.Length + 1);
            return Task.FromResult(BuildQrCodeImage(path));
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText,
            char chSegChar,
            char chEqualChar,
            string strDecodeStyle = "")
        {
            Hashtable results = new Hashtable();

            if (string.IsNullOrEmpty(strText) == true)
                return results;

            string[] parts = strText.Split(new char[] { chSegChar });   // ','
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                string strName = "";
                string strValue = "";
                int nRet = strPart.IndexOf(chEqualChar);    // '='
                if (nRet == -1)
                {
                    strName = strPart;
                    strValue = "";
                }
                else
                {
                    strName = strPart.Substring(0, nRet).Trim();
                    strValue = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strName) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (strDecodeStyle == "url")
                    strValue = HttpUtility.UrlDecode(strValue);

                results[strName] = strValue;
            }

            return results;
        }

        // parameters:
        //      strType 39 / 空 / 
        static Stream BuildQrCodeImage(string path)
        {
            Hashtable param_table = ParseParameters(path, ',', '=', "url");
            string strType = (string)param_table["type"];
            string strCode = (string)param_table["code"];
            string strWidth = (string)param_table["width"];
            string strHeight = (string)param_table["height"];

            int nWidth = 200;
            int nHeight = 200;

            if (string.IsNullOrEmpty(strWidth) == false)
                Int32.TryParse(strWidth, out nWidth);
            if (string.IsNullOrEmpty(strHeight) == false)
                Int32.TryParse(strHeight, out nHeight);

            string strCharset = "ISO-8859-1";
            bool bDisableECI = false;

            BarcodeFormat format = BarcodeFormat.QR_CODE;
            if (strType == "39" || strType == "code_39")
            {
                format = BarcodeFormat.CODE_39;
                strCode = strCode.ToUpper();    // 小写字符会无法编码
            }
            else if (strType == "ean_13")
            {
                format = BarcodeFormat.EAN_13;
                strCode = strCode.ToUpper();
            }

            EncodingOptions options = new QrCodeEncodingOptions
            {
                Height = nWidth,    // 400,
                Width = nHeight,    // 400,
                DisableECI = bDisableECI,
                ErrorCorrection = ErrorCorrectionLevel.L,
                CharacterSet = strCharset // "UTF-8"
            };

            if (strType == "39" || strType == "code_39"
                || strType == "ean_13")
                options = new EncodingOptions
                {
                    Width = nWidth, // 500,
                    Height = nHeight,   // 100,
                    Margin = 10
                };

            var writer = new BarcodeWriter
            {
                // Format = BarcodeFormat.QR_CODE,
                Format = format,
                // Options = new EncodingOptions
                Options = options
            };

            try
            {
                MemoryStream result = new MemoryStream(4096);

                using (var bitmap = writer.Write(strCode))
                {
                    bitmap.Save(result, System.Drawing.Imaging.ImageFormat.Png);
                }
                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
            catch (Exception ex)
            {
                Stream result = BuildTextImage("异常: " + ex.Message, Color.FromArgb(255, Color.DarkRed));
                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }

        static Stream BuildTextImage(string strText,
    Color color,
    int nWidth = 400)
        {
            // 文字图片
            return ArtText.BuildArtText(
                strText,
                "Consolas", // "Microsoft YaHei",
                (float)16,
                FontStyle.Bold,
                color,
                Color.Transparent,
                Color.Gray,
                ArtEffect.None,
                ImageFormat.Png,
                nWidth);
        }


    }
}
