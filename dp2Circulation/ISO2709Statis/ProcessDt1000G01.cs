using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Marc;

namespace dp2Circulation.ISO2709Statis
{
    // 2024/6/6
    /// <summary>
    /// #对dt1000导出的ISO2709文件整理-01字段
    /// </summary>
    public class ProcessDt1000G01 : Iso2709Statis
    {
        Stream _writer = null;

        public Stream Writer
        {
            get
            {
                return _writer;
            }
        }

        // 来源。库名(可能包括 IP 地址部分)，用于定位 dt1000 MARC 中 -01 字段
        string _source = null;

        public override void FreeResources()
        {
            if (_writer != null)
                _writer.Close();
        }

        public int PrepareSource(
            MarcRecord record,
            out string path,
            out string timestamp,
            out string strError)
        {
            strError = "";
            path = "";
            timestamp = "";

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
                    return -1;
                }
                if (ret == 0)
                {
                    strError = "用户放弃";
                    return -1;
                }
                _source = source;
            }

            if (ReaderInfoForm.GetDt1000G01Path(
                record,
                _source,
                "", // 保持 path 中返回的内容就是 dt1000 原样的
                out path,
                out timestamp) != 1)
            {
                strError = $"MARC 记录中没有找到匹配 '{_source}' 的 -01 字段";
                return -1;
            }

            // 进行一些整理：
            // 1) 如果第一个字符是 '/'，则删除这个 '/'; (这是 dt1000 导出来的 -01)
            // 2) 如果前方一致 "TCPIP网络/", 则替换为 ""; (这是 dt1000 插件模式导致的，比较啰嗦，需要缩短一点)
            // 3) 中间的 "/ctlno/" 替换为 "/"。 
            if (path.StartsWith("/") == true)
                path = path.Substring(1);
            if (path.StartsWith("TCPIP网络/"))
                path = path.Replace("TCPIP网络/", "");
            path = path.Replace("/ctlno/", "/");
            return 0;
        }

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            string strError = "";

            string suggest_output_filename =
                Path.GetFileNameWithoutExtension(this.InputFilename)
                + "_output"
                + Path.GetExtension(this.InputFilename);

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要创建的 ISO2709 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = suggest_output_filename;
            dlg.Filter = "ISO2709文件 (*.iso)|*.iso|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                e.Continue = ContinueType.SkipAll;
                return;
            }

            try
            {
                _writer = File.Create(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "创建文件 " + dlg.FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnRecord(object sender, StatisEventArgs e)
        {
            string strError = "";

            MarcRecord record = new MarcRecord(this.MARC);

            int nRet = PrepareSource(
                record,
                out string path,
                out string timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            string strXml = "";
            nRet = MarcUtil.Marc2XmlEx(this.MARC,
    this.Syntax,
    ref strXml,
    out strError);
            if (nRet == -1)
                goto ERROR1;
            */

            record.select("field[@name='-01']").detach();
            record.ChildNodes.insertSequence(
                new MarcField($"-01{path}|{timestamp}"),
                InsertSequenceStyle.PreferHead);

            // 将MARC机内格式转换为ISO2709格式
            // parameters:
            //      strSourceMARC   [in]机内格式MARC记录。
            //      strMarcSyntax   [in]为"unimarc"或"usmarc"
            //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
            //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
            // return:
            //      -1  出错
            //      0   成功
            nRet = MarcUtil.CvtJineiToISO2709(
                record.Text,
                this.Syntax,
                Encoding.UTF8,
                "", // unimarc_modify_100 ? "unimarc_100" : "",
                out byte[] baTarget,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            _writer.Write(baTarget,
                0,
                baTarget.Length);

            /*
            if (dlg.CrLf == true)
            {
                byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                s.Write(baCrLf, 0,
                    baCrLf.Length);
            }
            */

            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }
    }

}
