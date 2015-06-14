using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;

namespace dp2Circulation
{
    /// <summary>
    /// 暂未使用
    /// </summary>
    internal class NewPrintController : PrintController
    {
        PrintController OriginController = null;

        public NewPrintController(PrintController origin_controller)
        {
            this.OriginController = origin_controller;
        }

        public override bool IsPreview
        {
            get
            {
                return this.OriginController.IsPreview;
            }
        }


        /*
        public override void OnStartPrint(
            PrintDocument document,
            PrintEventArgs e
            )
        {
            this.OriginController.OnStartPrint(document, e);
        }

        public override void OnEndPrint(
            PrintDocument document,
            PrintEventArgs e
        )
        {
            this.OriginController.OnEndPrint(document, e);
        }
         * */


        public override Graphics OnStartPage(
            PrintDocument document,
            PrintPageEventArgs e
            )
        {
            /*
            if (document is PrintLabelDocument)
            {
                PrintLabelDocument doc = (PrintLabelDocument)document;
                if (doc.Ignor == true)
                    return null;
            }
             * */

            return this.OriginController.OnStartPage(document, e);
        }

        public override void OnEndPage(
            PrintDocument document,
            PrintPageEventArgs e
            )
        {
            this.OriginController.OnEndPage(document, e);
        }

    }
}
