using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.MarcDom;

namespace DigitalPlatform.LibraryServer
{
    public class FilterHost
    {
        public LibraryApplication App = null;
        public string RecPath = "";
        public string ResultString = "";

        // 2018/5/14
        public string Style = "";
    }

    public class LoanFilterDocument : FilterDocument
    {
        public FilterHost FilterHost = null;
    }
}
