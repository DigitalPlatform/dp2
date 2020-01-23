using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer.Reporting
{

    public class Patron
    {
        public string RecPath { get; set; }
        public string Barcode { get; set; }
        public string LibraryCode { get; set; }
        public string Department { get; set; }
        public string ReaderType { get; set; }
        public string Name { get; set; }
        public string State { get; set; }

        // 根据 XML 记录建立
        public static int FromXml(XmlDocument dom,
            string strReaderRecPath,
            string strLibraryCode,
            ref Patron line,
            out string strError)
        {
            strError = "";

            if (line == null)
                line = new Patron();

            line.RecPath = strReaderRecPath;
            line.Barcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            line.LibraryCode = strLibraryCode;

            line.Department = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            line.ReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            line.Name = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            line.State = DomUtil.GetElementText(dom.DocumentElement,
    "state");
            return 0;
        }
    }


}
