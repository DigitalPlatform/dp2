using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform.RFID;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DigitalPlatform.RFID.LogicChip;

namespace UnitTestRFID
{
    [TestClass]
    public class TestSetElementPos
    {
        static void SetContent(LogicChip chip)
        {
            chip.SetElement(ElementOID.PII, "1234567890");
            chip.SetElement(ElementOID.SetInformation, "1203");
            chip.SetElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.SetElement(ElementOID.OwnerInstitution, "US-InU-Mu");
            chip.SetElement(ElementOID.LocalDataA, "1234567890");
            chip.SetElement(ElementOID.LocalDataB, "1234567890");
            chip.SetElement(ElementOID.LocalDataC, "1234567890");
            chip.SetElement(ElementOID.Title, "1234567890 1234567890 1234567890");
            chip.SetElement(ElementOID.AOI, "1234567890");
            chip.SetElement(ElementOID.SOI, "1234567890");
            chip.SetElement(ElementOID.AIBI, "1234567890");
        }

        [TestMethod]
        public void Test_setElementPos_1()
        {
            LogicChip chip = new LogicChip();
            SetContent(chip);
            chip.SetIsNew(false);
            chip.Sort(4 * 28,
    4,
    true);
            foreach (var element in chip.Elements)
            {
                int end = element.StartOffs + element.OriginData.Length;
                Assert.IsTrue(end < 4 * 28, "越过末尾");
            }

            // 转换为 bytes。然后重新解析
            var bytes = chip.GetBytes(4 * 28,
                4,
                GetBytesStyle.None,
                out string block_map);

            LogicChip chip1 = LogicChip.From(bytes, 4);
            Debug.Write(chip1.ToString());

            /*
            List<OneArea> free_element_layout = new List<OneArea>();
            List<int> free_segments = new List<int>();
            List<object> anchor_list = new List<object>();

            chip.SetElementsPos(
    4 * 9,
    free_element_layout,
    free_segments,
    anchor_list);
    */
        }
    }
}
