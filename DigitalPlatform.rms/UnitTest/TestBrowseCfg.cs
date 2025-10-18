using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.Marc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DigitalPlatform.rms
{
    [TestClass]
    public class TestBrowseCfg
    {
        [TestMethod]
        public void Test_empty()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
</root>";
            string worksheet = @"0123456789012345678901234";
            string correct = "";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 测试经典用法
        [TestMethod]
        public void USE_normal()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col title='书名'>
        <title>
            <caption lang='zh-CN'>书名</caption>
            <caption lang='en'>Title</caption>
        </title>
        <use>title</use>
    </col>
    <col title='作者'>
        <title>
            <caption lang='zh-CN'>作者</caption>
            <caption lang='en'>Author</caption>
        </title>
        <use>author</use>
    </col>
    <col title='出版者'>
        <title>
            <caption lang='zh-CN'>出版者</caption>
            <caption lang='en'>Publisher</caption>
        </title>
        <use>publisher</use>
    </col>
    <col title='出版时间'>
        <title>
            <caption lang='zh-CN'>出版时间</caption>
            <caption lang='en'>Publish time</caption>
        </title>
        <use>publishtime</use>
    </col>
    <col title='中图法分类号'>
        <title>
            <caption lang='zh-CN'>中图法分类号</caption>
            <caption lang='en'>CLC classification</caption>
        </title>
        <use>clc</use>
    </col>
    <col title='主题词'>
        <title>
            <caption lang='zh-CN'>主题词</caption>
            <caption lang='en'>Subject</caption>
        </title>
        <use>subject</use>
    </col>
    <col title='关键词' convert='join(; )'>
        <title>
            <caption lang='zh-CN'>关键词</caption>
            <caption lang='en'>Keyword</caption>
        </title>
        <xpath nstable=''>//marc:record/marc:datafield[@tag='610']/marc:subfield[@code='a']</xpath>
    </col>
    <col title='ISBN'>
        <title>
            <caption lang='zh-CN'>ISBN</caption>
            <caption lang='en'>ISBN</caption>
        </title>
        <use>isbn</use>
    </col>
</root>";
            string worksheet = @"0123456789012345678901234
001T0000001
010  $aISBN
200  $a题名$f责任者
210  $a北京$c人民出版社$d2005
606  $a主题词
610  $a自由词
690  $aTP312
701  $a责任者检索点";
            string correct = "题名\t责任者\t人民出版社\t2005\tTP312\t主题词\t自由词\tISBN";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet, 
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // use 和 xpath 联用
        [TestMethod]
        public void USE_use_and_xpath()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col title='书名'>
        <title>
            <caption lang='zh-CN'>书名</caption>
            <caption lang='en'>Title</caption>
        </title>
        <use>title</use>
        <xpath nstable=''>//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f']</xpath>
    </col>
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "题名: 副题名责任者";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // use 和 xpath 联用，中间加上连接符号
        [TestMethod]
        public void USE_use_and_xpath_with_sep()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col title='书名' convert='join(; )'>
        <title>
            <caption lang='zh-CN'>书名</caption>
            <caption lang='en'>Title</caption>
        </title>
        <use>title</use>
        <xpath nstable=''>//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f']</xpath>
    </col>
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "题名: 副题名; 责任者";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // col/@prefix 测试
        [TestMethod]
        public void USE_col_prefix_postfix()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col title='书名' convert='join(; )' prefix='prefix' postfix='postfix'>
        <title>
            <caption lang='zh-CN'>书名</caption>
            <caption lang='en'>Title</caption>
        </title>
        <use>title</use>
        <xpath nstable=''>//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f']</xpath>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\tprefix题名: 副题名; 责任者postfix\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // col/@text 属性。静态文字
        [TestMethod]
        public void USE_col_static_text()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='11' />
    <col text='22' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "11\t22";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 连接字符串在 col/@text 内容中 title 事项没有出现重复的时候无用
        [TestMethod]
        public void USE_sep_hidden()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col>
        <use convert='join(; )'>title</use>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名: 副题名\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 一个 use 中两栏
        [TestMethod]
        public void USE_use_multi_col()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col>
        <use>title,author</use>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名: 副题名责任者\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 一个 use 中两栏，中间加连接符号
        [TestMethod]
        public void USE_use_multi_col_with_sep()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col>
        <use convert='join(; )'>title,author</use>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名: 副题名; 责任者\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }


        // 两个 use 平行使用
        [TestMethod]
        public void USE_multi_use()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col>
        <use>title</use>
        <use>author</use>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名: 副题名责任者\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 两个 use 平行使用，中间加连接符号
        [TestMethod]
        public void USE_multi_use_with_sep()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col convert='join(; )'>
        <use>title</use>
        <use>author</use>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名: 副题名; 责任者\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 简单返回 MARC 机内格式字符串
        [TestMethod]
        public void javascript_01()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col convert='join(; )'>
        <script>biblio.Text;</script>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t012345678901234567890123200  \u001fa题名\u001fe副题名\u001ff责任者\u001e\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 简单返回 syntax
        [TestMethod]
        public void javascript_02()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col convert='join(; )'>
        <script>syntax;</script>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\tunimarc\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 使用 MarcQuery 函数
        [TestMethod]
        public void javascript_03()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col convert='join(; )'>
        <script>result = syntax + ':' + biblio.select(""field[@name='200']/subfield[@name='a']"").FirstContent;</script>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\tunimarc:题名\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }

        // 使用 MarcQuery 函数
        [TestMethod]
        public void javascript_04()
        {
            string cfg_file_content = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
    <nstable>
        <item prefix='marc' url='http://dp2003.com/UNIMARC' />
    </nstable>
    <col text='|' />
    <col>
        <script>
        <![CDATA[
            result = """";
            var subfields = biblio.select(""field[@name='200']/subfield"");
            for(var i=0; i<subfields.Count; i++)
            {
                result += subfields[i].Content;
                result += "";"";
            }
        ]]>
        </script>
    </col>
    <col text='|' />
</root>";
            string worksheet = @"0123456789012345678901234
200  $a题名$e副题名$f责任者";
            string correct = "|\t题名;副题名;责任者;\t|";
            string marc_syntax = "unimarc";

            string[] cols = Process(cfg_file_content,
                worksheet,
                marc_syntax);
            Assert.AreEqual(correct, string.Join("\t", cols));
        }


        string[] Process(
            string cfg_file_content,
            string worksheet,
            string marc_syntax)
        {
            int nRet = 0;
            string strError = "";

            var bin_dir = "c:\\test";

            var browseCfg = new BrowseCfg();
            string temp_filename = Path.GetTempFileName();
            try
            {
                File.WriteAllText(temp_filename, cfg_file_content, Encoding.UTF8);
                nRet = browseCfg.Initial(temp_filename,
                    bin_dir,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }
            finally
            {
                File.Delete(temp_filename);
            }

            MarcRecord record = MarcRecord.FromWorksheet(worksheet.Replace("$", "ǂ"));
            nRet = MarcUtil.Marc2Xml(record.Text,
                marc_syntax,
                out string xml,
                out strError);
            if (nRet == -1)
            {
                throw new Exception(strError);
            }
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            nRet = browseCfg.BuildCols(dom,
                "",
                out string[] cols,
                out strError);
            if (nRet == -1)
            {
                throw new Exception(strError);
            }
            Assert.IsNotNull(cols);
            Assert.AreEqual(cols.Sum(o=>o.Length), nRet);
            return cols;
        }
    }
}
