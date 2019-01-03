using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;
using static DigitalPlatform.RFID.LogicChip;

namespace UnitTestRFID
{
    [TestClass]
    public class TestChip2
    {

        // 测试写入新标签
        [TestMethod]
        public void Test_chip2_layout_1()
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.PII, "B123456");
            chip.NewElement(ElementOID.OMF, "BA");
            Debug.Write(chip.ToString());

            var result = chip.GetBytes(4 * 28,
                4,
                GetBytesStyle.None,
                out string block_map);
            string result_string = Element.GetHexString(result, "4");

            Debug.Write(chip.ToString());

        }

    }
}
