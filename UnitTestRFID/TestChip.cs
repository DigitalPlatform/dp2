using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;

namespace UnitTestRFID
{
    [TestClass]
    public class TestChip
    {
        // 国标第二册最后的完整例子，测试 LogicChip.From()
        [TestMethod]
        public void Test_chip_example_from()
        {
            byte[] data = Element.FromHexString(
                @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
);


            LogicChip chip = LogicChip.From(data, 4, "ll....lll");
            Debug.Write(chip.ToString());

            Assert.AreEqual(chip.FindElement(ElementOID.PrimaryItemIdentifier).Text,
                "123456789012");
            Assert.AreEqual(chip.FindElement(ElementOID.SetInformation).Text,
                "1203");
            Assert.AreEqual(chip.FindElement(ElementOID.ShelfLocation).Text,
                "QA268.L55");
            Assert.AreEqual(chip.FindElement(ElementOID.OwnerInstitution).Text,
                "US-InU-Mu");
        }


        // 测试重组含有 Locked 元素的已有标签内容
        [TestMethod]
        public void Test_chip_layout_1()
        {
            byte[] data = Element.FromHexString(
                @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
);


            LogicChip chip = LogicChip.From(data,4,"ll....lll");
            Debug.Write(chip.ToString());

            chip.Elements[0].SetLocked(true);
            chip.Elements[4].SetLocked(true);

            chip.SetIsNew(false);
            chip.Sort(4 * 9, 4);

            Assert.IsTrue(chip.Elements[0].OID == ElementOID.PrimaryItemIdentifier);
            Assert.IsTrue(chip.Elements[1].OID == ElementOID.ContentParameter);
            Assert.IsTrue(chip.Elements[2].OID == ElementOID.SetInformation);
            Assert.IsTrue(chip.Elements[3].OID == ElementOID.ShelfLocation);
            Assert.IsTrue(chip.Elements[4].OID == ElementOID.OwnerInstitution);
        }

        // 测试写入新标签
        [TestMethod]
        public void Test_chip_layout_2()
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.PII, "123456789012").WillLock = true;
            chip.NewElement(ElementOID.SetInformation, "1203");
            chip.NewElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.NewElement(ElementOID.OwnerInstitution, "US-InU-Mu").WillLock = true;
            Debug.Write(chip.ToString());

            var result = chip.GetBytes(4 * 9, 4, out string block_map);
            string result_string = Element.GetHexString(result, "4");
            byte[] correct = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
);
            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(block_map, "ww....www");

        }

        // 测试写入新标签
        // 最后两个 WillLock 元素，只需要调整边界一次。这两个元素之间不需要调整边界
        [TestMethod]
        public void Test_chip_layout_3()
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.PII, "123456789012").WillLock = true;
            chip.NewElement(ElementOID.SetInformation, "1203");
            chip.NewElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.NewElement(ElementOID.OwnerInstitution, "US-InU-Mu").WillLock = true;
            chip.NewElement(ElementOID.Title, "test").WillLock = true;

            var result = chip.GetBytes(4 * 11, 4, out string block_map);

            Debug.Write(chip.ToString());

            string result_string = Element.GetHexString(result, "4");
            byte[] correct = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 02 d0 02
14 02 04 b3
c6 02 07 44
1c b6 e2 e3
35 d6 00 00
03 07 ac c0
9e ba a0 6f
6b 7f 02 04
74 65 73 74"
);
            Assert.IsTrue(result.SequenceEqual(correct));
            Assert.AreEqual(block_map, "ww.....wwww");
        }

        // 测试复杂布局
        [TestMethod]
        public void Test_chip_complex_layout_1()
        {
            byte[] data = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
);


            LogicChip chip = LogicChip.From(data, 4, "ll....lll");
            Debug.Write(chip.ToString());

            Assert.AreEqual(chip.FindElement(ElementOID.PrimaryItemIdentifier).Text,
    "123456789012");
            Assert.AreEqual(chip.FindElement(ElementOID.SetInformation).Text,
                "1203");
            Assert.AreEqual(chip.FindElement(ElementOID.ShelfLocation).Text,
                "QA268.L55");
            Assert.AreEqual(chip.FindElement(ElementOID.OwnerInstitution).Text,
                "US-InU-Mu");

            // 先固化为写入过的状态

            // 测试在后面继续加入新元素
            chip.NewElement(ElementOID.Title, "test").WillLock = true;
            var result = chip.GetBytes(4 * 20, 4, out string block_map);

            Debug.Write(chip.ToString());
            string result_string = Element.GetHexString(result, "4");

            // 以前的最后一个非锁定元素，跑到了以前最后的锁定元素后面。因为 Content Parameter 字段变长了，顶走了它
            Assert.AreEqual(block_map, "ll....lll..www");

        }
    }
}
