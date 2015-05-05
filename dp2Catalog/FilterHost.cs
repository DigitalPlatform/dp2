using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.MarcDom;

namespace dp2Catalog
{
    public class FilterHost
    {
        public MainForm MainForm = null;
        public string ID = "";
        public string ResultString = "";
    }

    public class BrowseFilterDocument : FilterDocument
    {
        public FilterHost FilterHost = null;
    }
}
