using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
