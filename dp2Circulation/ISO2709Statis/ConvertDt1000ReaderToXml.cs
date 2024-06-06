﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation.ISO2709Statis
{
    // 2021/12/17
    /// <summary>
    /// #将dt1000读者MARC转换为dp2的XML格式
    /// </summary>
    public class ConvertDt1000ReaderToXml : Iso2709Statis
    {
        string _outputFileName = "";
        XmlWriter _writer = null;

        // 来源。库名(可能包括 IP 地址部分)，用于定位 dt1000 MARC 中 -01 字段
        string _source = null;

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的 XML 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            dlg.Filter = "XML 文件 (*.xml)|*.xml|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                e.Continue = ContinueType.Error;
                e.ParamString = "放弃";
                return;
            }

            _outputFileName = dlg.FileName;

            _writer = XmlWriter.Create(_outputFileName,
                            new XmlWriterSettings
                            {
                                Indent = true,
                                OmitXmlDeclaration = true,
                                CheckCharacters = false
                            });
            _writer.WriteStartDocument();
            _writer.WriteStartElement("collection");
            // dprms名字空间
            _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + ReaderInfoForm.HtmlEncode(DateTime.Now.ToLongTimeString())
        + " 开始转换读者 MARC 记录</div>");
        }

        public override void FreeResources()
        {
            base.FreeResources();
            if (_writer != null)
                _writer.Close();
        }

        public override void OnRecord(object sender, StatisEventArgs e)
        {
            string strError = "";

            MarcRecord record = new MarcRecord(this.MARC);
            /*
            var g01 = record.select("field[@name='-01']").FirstContent;
            var parts = StringUtil.ParseTwoPart(g01, "|");
            string path = ToDp2Path(parts[0]);
            string timestamp = parts[1];
            */
            /*
            ReaderInfoForm.ParseDt1000G01(record,
out string path,
out string timestamp);
            */

            if (_source == null)
            {
                // 从 dt1000 MARC 记录中的若干 -01 字段中选择一个来源数据库
                // /132.147.160.100/图书总库/ctlno/0000001
                int ret = ReaderInfoForm.SelectDt1000G01Source(
                    this.MainForm,
                    record,
                    out string source,
                    out string _);
                if (ret == -1)
                {
                    strError = "第一条 MARC 记录中缺乏 -01 字段，无法获得来源";
                    goto ERROR1;
                }
                if (ret == 0)
                {
                    strError = "用户放弃";
                    goto ERROR1;
                }
                _source = source;
            }

            if (ReaderInfoForm.GetDt1000G01Path(
                record,
                _source,
                "dp2",
                out string path,
                out string timestamp) != 1)
            {
                strError = $"MARC 记录中没有找到匹配 '{_source}' 的 -01 字段";
                goto ERROR1;
            }

            /*
            var text_110_d = record.select("field[@name='110']/subfield[@name='d']").FirstContent;
            if (string.IsNullOrEmpty(text_110_d) == false)
            {
                // this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + ReaderInfoForm.HtmlEncode("110$d='" + text_110_d + "'") + "</div>");
                if (text_110_d == "在职")
                {
                    // 追加在原有 110$a 内容后面
                    var text_110_a = record.select("field[@name='110']/subfield[@name='a']").FirstContent;
                    record.setFirstSubfield("110", "a", text_110_a + text_110_d);
                    record.setFirstSubfield("110", "d", null);
                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + ReaderInfoForm.HtmlEncode("110$a 做了 '在职' 微调") + "</div>");
                }
            }
            */

            int nRet = ReaderInfoForm.ConvertDt1000ReaderMarcToXml(record,
                path,
                timestamp,
                out XmlDocument dom,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            dom.WriteTo(_writer);

            if (string.IsNullOrEmpty(strError) == false)
            {
                this.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + ReaderInfoForm.HtmlEncode(path + ": " + strError).Replace("; ", "<br/>") + "</div>");
            }
            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + ReaderInfoForm.HtmlEncode(DateTime.Now.ToLongTimeString())
        + " 结束转换读者 MARC 记录</div>");

            if (_writer != null)
            {
                // writer 收尾
                _writer.WriteEndElement();
                _writer.WriteEndDocument();

                _writer.Close();
                _writer = null;
            }

            /*
            if (string.IsNullOrEmpty(_outputFileName) == false
                && File.Exists(_outputFileName))
            {
                try
                {
                    System.Diagnostics.Process.Start(_outputFileName);
                }
                catch
                {

                }
            }
            */
        }
    }
}
