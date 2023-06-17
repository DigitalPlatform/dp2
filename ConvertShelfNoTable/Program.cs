using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// 将 Excel 形态的架位对照表文件，转换为 XML 形态
/*
<root>
    <shelf accessNoRange="A11~A22" shelfNo="101A12" />
    <shelf accessNoRange="I712.45/1~I88" shelfNo="777" />
</root>
*/

namespace ConvertShelfNoTable
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("usage: convertShelfNo sourceFileName targetFileName regionNumber");
                return;
            }

            string sourceFileName = args[0];
            string targetFileName = args[1];
            var region = args[2];

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            var doc = new XLWorkbook(sourceFileName);
            var sheet = doc.Worksheets.FirstOrDefault();
            var sheet_rows = sheet.Rows();
            int i = 0;
            foreach ( var row in sheet_rows )
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }
                // 架编号 "01" "99"
                var shelfNumber = row.Cell("B").Value.ToString().Trim().PadLeft(2, '0');
                // 面 "A" "B"
                var side = row.Cell("C").Value.ToString().Trim();

                if (side != "A" && side != "B")
                    throw new Exception($"面必须为 A 或者 B (但为 '{side}')");

                // 列 "1" "7"
                var column = row.Cell("D").Value.ToString().Trim();

                // 行 "1" "6"
                var line = row.Cell("E").Value.ToString().Trim();

                // 索书号起
                var accessNoStart = row.Cell("F").Value.ToString().Trim();

                if (accessNoStart == "\\" || accessNoStart == "空")
                    accessNoStart = "";

                // 索书号止
                var accessNoEnd = row.Cell("G").Value.ToString().Trim();

                var range = accessNoStart + "~" + accessNoEnd;
                if (string.IsNullOrEmpty(accessNoStart))
                    range = "";

                var shelfNo = region + shelfNumber + side + column + line;

                XmlElement shelf = dom.CreateElement("shelf");
                dom.DocumentElement.AppendChild(shelf);


                shelf.SetAttribute("shelfNo", shelfNo);
                shelf.SetAttribute("accessNoRange", range);

                Console.WriteLine(shelf.OuterXml);
                i++;
            }

            dom.Save(targetFileName);
            Console.WriteLine("complete");
        }
    }
}
