using DigitalPlatform.RFID;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestRFID
{
    [TestClass]
    public class TestBlockRange
    {
        // 测试 3 个 range
        [TestMethod]
        public void Test_blockRange_1()
        {
            // 准备好一个芯片内容
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

            // 测试 BlockRange.GetBlockRanges()
            List<BlockRange> ranges = BlockRange.GetBlockRanges(
                4,
                data,
                "ll....lll",
                'l');
            Assert.IsTrue(ranges[0].BlockCount == 2);
            Assert.IsTrue(ranges[0].Locked == true);
            Assert.IsTrue(ranges[0].Bytes.SequenceEqual(
                Element.FromHexString(
                @"91 00 05 1c
                be 99 1a 14"
                )
            ));

            Assert.IsTrue(ranges[1].BlockCount == 4);
            Assert.IsTrue(ranges[1].Locked == false);
            Assert.IsTrue(ranges[1].Bytes.SequenceEqual(
                Element.FromHexString(
                @"02 01 d0 14
                02 04 b3 46
                07 44 1c b6
                e2 e3 35 d6"
                )
            ));
            Assert.IsTrue(ranges[2].BlockCount == 3);
            Assert.IsTrue(ranges[2].Locked == true);
            Assert.IsTrue(ranges[2].Bytes.SequenceEqual(
                Element.FromHexString(
                @"83 02 07 ac
                c0 9e ba a0
                6f 6b 00 00"
                )
            ));
        }

        // 测试只有一个 range 的情况
        [TestMethod]
        public void Test_blockRange_2()
        {
            // 准备好一个芯片内容
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

            // 测试 BlockRange.GetBlockRanges()
            List<BlockRange> ranges = BlockRange.GetBlockRanges(
                4,
                data,
                ".........",
                'l');
            Assert.IsTrue(ranges.Count == 1);

            Assert.IsTrue(ranges[0].BlockCount == 9);
            Assert.IsTrue(ranges[0].Locked == false);
            Assert.IsTrue(ranges[0].Bytes.SequenceEqual(
                Element.FromHexString(
                @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
                )
            ));

        }

        // 测试只有一个 range 的情况。block_map 字符串用 "" 代替 "........."
        [TestMethod]
        public void Test_blockRange_3()
        {
            // 准备好一个芯片内容
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

            // 测试 BlockRange.GetBlockRanges()
            List<BlockRange> ranges = BlockRange.GetBlockRanges(
                4,
                data,
                "",
                'l');
            Assert.IsTrue(ranges.Count == 1);

            Assert.IsTrue(ranges[0].BlockCount == 9);
            Assert.IsTrue(ranges[0].Locked == false);
            Assert.IsTrue(ranges[0].Bytes.SequenceEqual(
                Element.FromHexString(
                @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
                )
            ));

        }

    }
}
