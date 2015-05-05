using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Catalog
{
    public interface IZSearchForm
    {
        MainForm MainForm
        {
            get;
            set;
        }

        void EnableQueryControl(ZConnection connection, bool bEnable);

        bool ShowMessageBox(ZConnection connection, string strText);

        bool ShowQueryResultInfo(ZConnection connection, string strText);

        bool DisplayBrowseItems(ZConnection connection, bool bTriggerSelChanged);

        int BuildBrowseText(
    ZConnection connection,
    DigitalPlatform.Z3950.Record record,
    string strStyle,
    out string strBrowseText,
    out int nImageIndex,
    out string strError);



    }
}
