using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace dp2Circulation
{
    public class ArrivedSearchForm : ItemSearchForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ArrivedSearchForm()
            : base()
        {
            this.DbType = "arrive";
        }

        // 册条码号列 index
        int _itemBarcodeCol = -1;

        // 获得事项所从属的书目记录的路径
        // parameters:
        //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
        // return:
        //      -1  出错
        //      0   相关数据库没有配置 parent id 浏览列
        //      1   找到
        public override int GetBiblioRecPath(
            Stop stop,
            LibraryChannel channel,
            ListViewItem item,
            bool bAutoSearch,
            out int nCol,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            nCol = -1;
            strBiblioRecPath = "";

            if (_itemBarcodeCol == -1)
            {
                if (Program.MainForm.NormalDbProperties == null
                    || Program.MainForm.NormalDbProperties.Count == 0)
                {
                    strError = "普通数据库属性尚未初始化。这通常是因为刚进入内务时候初始化阶段出现错误导致的。请退出内务重新进入，并注意正确登录";
                    return -1;
                }
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(/*"预约到书"*/Program.MainForm.ArrivedDbName);
                if (temp == null)
                {
                    strError = "没有找到列定义 预约到书";
                    return -1;
                }

                nCol = temp.FindColumnByType("item_barcode");
                if (nCol == -1)
                {
                    strError = "预约到书的浏览格式没有配置 item_barcode 列";
                    return 0;
                }

                nCol += 1 + m_nBiblioSummaryColumn;
                /*
                if (this.m_bFirstColumnIsKey == true)
                    nCol++;
                */
                _itemBarcodeCol = nCol;   // 储存起来
            }
            else
                nCol = _itemBarcodeCol;

            Debug.Assert(nCol > 0, "");

            // 获得 item barcode
            string itemBarcode = ListViewUtil.GetItemText(item, nCol);
            if (string.IsNullOrEmpty(itemBarcode))
                return 1;
            if (itemBarcode.StartsWith("@") == false)
                strBiblioRecPath = $"@itemBarcode:{itemBarcode}";
            else
                strBiblioRecPath = itemBarcode;
            return 1;
        }

    }
}
