using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dp2Catalog
{
    public class WebClientEx : WebClient
    {
        public static async Task<DowloadStringResult> DownloadStringAsync(string url)
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add(HttpRequestHeader.AcceptEncoding, "utf-8");
            byte[] result = await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
            var encoding = GetEncodingFrom(webClient.ResponseHeaders, Encoding.UTF8);
            return new DowloadStringResult
            {
                String = encoding.GetString(result),
                Encoding = encoding
            };
        }

        public static Encoding GetEncodingFrom(
NameValueCollection responseHeaders,
Encoding defaultEncoding = null)
        {
            if (responseHeaders == null)
                throw new ArgumentNullException("responseHeaders");

            //Note that key lookup is case-insensitive
            var contentType = responseHeaders["Content-Type"];
            if (contentType == null)
                return defaultEncoding;

            var contentTypeParts = contentType.Split(';');
            if (contentTypeParts.Length <= 1)
                return defaultEncoding;

            var charsetPart =
                contentTypeParts.Skip(1).FirstOrDefault(
                    p => p.TrimStart().StartsWith("charset", StringComparison.InvariantCultureIgnoreCase));
            if (charsetPart == null)
                return defaultEncoding;

            var charsetPartParts = charsetPart.Split('=');
            if (charsetPartParts.Length != 2)
                return defaultEncoding;

            var charsetName = charsetPartParts[1].Trim();
            if (charsetName == "")
                return defaultEncoding;

            try
            {
                return Encoding.GetEncoding(charsetName);
            }
            catch (ArgumentException ex)
            {
                throw new UnknownEncodingException(
                    charsetName,
                    "The server returned data in an unknown encoding: " + charsetName,
                    ex);
            }
        }

    }

    public class DowloadStringResult : NormalResult
    {
        public string String { get; set; }
        public Encoding Encoding { get; set; }
    }

    public class UnknownEncodingException : Exception
    {
        public string CharsetName { get; set; }

        public UnknownEncodingException(string charsetName, string text, Exception innerException) : base(text, innerException)
        {
            CharsetName = charsetName;
        }
    }
}
