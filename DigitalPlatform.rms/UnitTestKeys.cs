using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.rms
{
    [TestClass]
    public class UnitTestKeys
    {
        [TestMethod]
        public void test_keys_01()
        {
            KeysCfg keys_cfg = new KeysCfg();

			string content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
	<nstable>
		<item prefix=""marc"" url=""http://dp2003.com/UNIMARC"" />
	</nstable>

	<key>
		<xpath nstable="""">//marc:record/marc:datafield[@tag='700' or @tag='701' or @tag='702' or @tag='710' or @tag='711' or @tag='712']/marc:subfield[@code='9' or @code='A']</xpath>
		<from>contributorPinyin</from>
		<table ref=""contributorPinyin"" />
	</key>
	<table name=""contributorPinyin"" id=""8"" type=""pinyin_contributor"">
		<convert>
			<string style=""upper,pinyinab"" />
		</convert>
		<convertquery>
			<string style=""upper,pinyinab"" />
		</convertquery>
		<caption lang=""zh-CN"">责任者拼音</caption>
		<caption lang=""en"">Contributor pinyin</caption>
	</table>

	<key>
		<xpath nstable="""">//marc:record/marc:datafield[@tag='700' or @tag='701' or @tag='702' or @tag='710' or @tag='711' or @tag='712']/marc:subfield[@code='9' or @code='A']</xpath>
		<from>contributorPinyin</from>
		<table name=""contributorPinyin"" id=""811"">
			<convert>
				<string style=""stopword,upper,removeblank"" stopwordTable=""pinyin""/>
			</convert>
		</table>
	</key>
</root>
";

            var strKeysCfgFileName = Path.GetTempFileName();
			File.WriteAllText(strKeysCfgFileName, content);

            try
            {
                var ret = keys_cfg.Initial(strKeysCfgFileName,
                    Environment.CurrentDirectory,
                    "test_",
                    out string strError);
				Assert.AreEqual(0, ret);
            }
            finally
            {
                File.Delete(strKeysCfgFileName);
            }
        }

        // "s" 时间的字面值是用本地时间表示的
        [TestMethod]
		public void test_parseUTimeString_01()
		{
			string time = "2025-01-01T00:00:00";
            var correct = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Local);
            correct = correct.ToUniversalTime();

            var ret = KeysCfg.TryParseUTimeString(time,
			out DateTime value);
            Assert.AreEqual(true, ret);
            Console.WriteLine(value.Ticks);
            Assert.AreEqual(correct, value);
        }

        // "u" 时间的字面值是用 UTC 时间表示的
        [TestMethod]
        public void test_parseUTimeString_02()
        {
            string time = "2025-01-01 00:00:00Z";
            var correct = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var ret = KeysCfg.TryParseUTimeString(time,
            out DateTime value);
            Assert.AreEqual(true, ret);
            Console.WriteLine(value.Ticks);
            Assert.AreEqual(correct, value);
        }

        // RFC1123 时间的字面值可以同时包含本地时间和时区信息
        [TestMethod]
        public void test_parseUTimeString_03()
        {
            string time = "Wed, 01 Jan 2025 00:00:00 +0800";
            var correct = new DateTime(2025, 1, 1, 0, 0, 0);
            correct = correct.ToUniversalTime();

            // UTC 时间
            var value = DateTimeUtil.FromRfc1123DateTimeString(time);
            Console.WriteLine(value.Ticks);
            Assert.AreEqual(correct, value);
        }
    }
}
