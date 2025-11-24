using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using PdfSharp.Drawing;
using PdfSharp.Pdf;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace dp2LibraryApiTester.TestCase
{
    public static class TestSetBiblioInfo
    {
        static string _null_marc_header = null;

        public static NormalResult TestAll(CancellationToken token)
        {
            NormalResult result = null;

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            token.ThrowIfCancellationRequested();

            // *** Create
            {
                string[] names = new string[] {
                        "权限不足",
        "普通权限 只有 setbiblioinfo",
        "普通权限 setbiblioinfo getbiblioinfo",
        "存取定义 只有 setbiblioinfo",
        "存取定义 setbiblioinfo getbiblioinfo",
        "存取定义 setbiblioinfo getbiblioinfo 字段限制",
        "存取定义 order",
        "存取定义 order 数据库 orderWork 角色",
                };
                string[] styles = new string[] {
                    "",
                    "file",
                    "file,objectright:setobject|getobject",
                    "file,objectright:setbiblioobject|getbiblioobject",
                };

                Utility.RunMany(
TestCreate,
names,
styles,
token);
            }


            token.ThrowIfCancellationRequested();

            // *** Change
            {
                string[] names = new string[]
                {
                            "权限不足",
        "普通权限 只有 setbiblioinfo",
        "普通权限 setbiblioinfo getbiblioinfo",
        "存取定义 只有 setbiblioinfo",
        "存取定义 setbiblioinfo getbiblioinfo",
        "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向未修改字段)",
        "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向修改字段)",
        "存取定义 order",
        "存取定义 order 数据库 orderWork 角色",
        "存取定义 setbiblioinfo=ownerchange",
        "存取定义 setbiblioinfo=ownerchange 998$z相同",
        "存取定义 setbiblioinfo=ownerchange 998$z不相同",
                };

                string[] styles = new string[] {
                    "",
                    "file",
                    "file,objectright:setobject|getobject",
                    "file,objectright:setbiblioobject|getbiblioobject",
                };

                Utility.RunMany(
TestChange,
names,
styles,
token);
            }


            token.ThrowIfCancellationRequested();

            // *** Delete
            {
                string[] names = new string[] {
                    "权限不足",
                    "普通权限 只有 setbiblioinfo",
                    "普通权限 setbiblioinfo getbiblioinfo",
                    "存取定义 只有 setbiblioinfo",
                    "存取定义 setbiblioinfo getbiblioinfo",

                    "存取定义 setbiblioinfo=change getbiblioinfo",
                    "存取定义 setbiblioinfo=(否定) getbiblioinfo",

                    "存取定义 setbiblioinfo getbiblioinfo 字段限制",
                    "存取定义 order",
                    "存取定义 order 数据库 orderWork 角色",
                    "存取定义 setbiblioinfo=ownerdelete",
                    "存取定义 setbiblioinfo=ownerdelete 998$z相同",
                    "存取定义 setbiblioinfo=ownerdelete 998$z不相同",
                    "存取定义 order|setbiblioinfo 后者优先",
                };

                string[] styles = new string[] {
                    "",
                    "file",
                    "file,objectright:setobject|getobject",
                    "file,objectright:setbiblioobject|getbiblioobject",
                };

                Utility.RunMany(
TestDelete,
names,
styles,
token);
            }


            token.ThrowIfCancellationRequested();

            // *** Get
            {
                string[] names = new string[] {
        "权限不足",
        "普通权限 getbiblioinfo",
        "存取定义 getbiblioinfo",
        "存取定义 order",
        "存取定义 getbiblio=(否定)",
        "存取定义 getbiblioinfo 字段限制",
        "存取定义 order 数据库 orderWork 角色",
        "存取定义 order|getbiblioinfo 后者优先",
            };

                string[] styles = new string[] {
    "",
    "file",
    "file,objectright:getobject",
    "file,objectright:getbiblioobject"
    };
                Utility.RunMany(
        TestGet,
        names,
        styles,
        token);
            }

        END1:
            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }




        #region 下级函数


        #region 对象文件测试

        static string _sample_object_filename = null;

        static void PrepareObject(LibraryChannel channel,
            string recpath,
            string object_id)
        {
            // 准备样本文件
            if (string.IsNullOrEmpty(_sample_object_filename))
            {
                var dir = Environment.CurrentDirectory;
                var file_name = Path.Combine(dir, "test.bin");
                    CreatePdfFile(file_name, default);
                _sample_object_filename = file_name;
            }

            // 写入 recpath 之下 /object/1 位置
            {
                var object_path = recpath + "/object/1";
                var ret = channel.UploadObject(null,
                    _sample_object_filename,
                    object_path,
                    "",
                    null,
                    true,
                    out _,
                    out string error);
                if (ret == -1)
                    throw new Exception($"将本地文件 '{_sample_object_filename}' 写入对象 '{object_path}' 发生错误: {error}");
            }
        }

        public static void CreatePdfFile(string fileName, CancellationToken token)
        {
            PdfDocument document = new PdfDocument();

            for (int i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();

                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Verdana", 40, XFontStyle.Bold);
                gfx.DrawString($"测试 Page {i + 1}", font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
            }
            document.Save(fileName);
        }


        static void TryVerifyObject(LibraryChannel channel,
            string recpath,
            string xml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            var ids = XmlUtility.GetDprmsFileIds(dom);
            if (ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    string path = recpath + "/object/" + id;
                    var output_filename = Path.GetTempFileName();
                    try
                    {
                        var ret = channel.GetRes(null,
                            path,
                            output_filename,
                            "data,content",
                            out _,
                            out _,
                            out string output_path,
                            out string error);
                        if (ret == -1)
                            throw new Exception($"获取对象 '{path}' 时出错: {error}");

                        Debug.Assert(_sample_object_filename != null);

                        // 和样本文件对照
                        if (FileUtil.CompareTwoFile(_sample_object_filename, output_filename) != 0)
                            throw new Exception($"获取对象 '{path}' 时出错: {error}");
                    }
                    finally
                    {
                        File.Delete(output_filename);
                    }
                }
            }
        }

        #endregion


        static bool IsXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return false;
            if (xml.StartsWith("<") == false)
                return false;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

#if REMOVED
        // 比较两个 XML 字符串是否等同
        static bool IsSame(string xml1, string xml2)
        {
            // 先判断两个文件是否都是 XML 格式
            if (IsXml(xml1) == false || IsXml(xml2) == false)
                return string.Equals(xml1, xml2);

            return DomUtil.GetIndentXml(xml1) == DomUtil.GetIndentXml(xml2);
            // return XNode.DeepEquals(XElement.Parse(xml1), XElement.Parse(xml2));
        }
#endif


        #endregion

        // TODO: 增加 style 为 "file,objectright:setobject" (缺乏 |getobject) 的测试能力。ErrorCode.SystemError (权限违反数据完整性安全原则)
        // 创建书目记录
        // parameters:
        //      name    为如下值
        //              "权限不足"
        //              "普通权限 只有 setbiblioinfo"
        //              "普通权限 setbiblioinfo getbiblioinfo"
        //              "存取定义 只有 setbiblioinfo"
        //              "存取定义 setbiblioinfo getbiblioinfo"
        //              "存取定义 setbiblioinfo getbiblioinfo 字段限制"
        //              "存取定义 order"
        //              "存取定义 order 数据库 orderWork 角色"
        static NormalResult TestCreate(
            string name,
            string style,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DataModel.SetMessage($"===> 项目 创建书目记录 {name} 情形, style={style} ...");

            var file = StringUtil.IsInList("file", style);
            var objectright = StringUtil.GetParameterByPrefix(style, "objectright");
            string append_o_right()
            {
                if (objectright == null)
                    return "";
                return "," + objectright.Replace("|", ",");
            }

            string append_o_access()
            {
                if (objectright == null)
                    return "";
                return "|" + objectright;
            }

            string path = "";
            byte[] origin_timestamp = null;
            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }
            long DoCreate(LibraryChannel c, out string error)
            {
                return c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    GetXml(),
                    null,
                    "",
                    out path,
                    out origin_timestamp,
                    out error);
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path))
                    return;
                int redo_count = 0;
            REDO:
                var ret = c.SetBiblioInfo(null,
                    "delete",
                    path,
                    "xml",
                    "",
                    origin_timestamp,
                    "",
                    out _,
                    out origin_timestamp,
                    out string error);
                if (ret == -1)
                {
                    if (redo_count < 2
                        && c.ErrorCode == ErrorCode.TimestampMismatch)
                    {
                        redo_count++;
                        goto REDO;
                    }
                    throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                }
            }

            switch (name)
            {
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCreate(channel, out string error);
                        {
                            Utility.AssertResult(
                "SetBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied,
        error);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCreate(channel, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo,getbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var xml = GetXml();
                        var ret = channel.SetBiblioInfo(null,
                            "new",
                            _strBiblioDbName + "/?",
                            "xml",
                            xml,
                            null,
                            "",
                            out path,
                            out byte[] timestamp,
                            out string error);
                        {
                            if (file && objectright == null)
                                Utility.AssertResult(
"SetBiblioInfo()",
ret,
0,
channel.ErrorCode,
ErrorCode.PartialDenied,
        error,
        "被拒绝");   // 创建成功，但 dprms:file 元素被拒绝写入
                            else
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
        error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("创建书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCreate(channel, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "权限安全性规则");
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var xml = GetXml();
                        var ret = channel.SetBiblioInfo(null,
                            "new",
                            _strBiblioDbName + "/?",
                            "xml",
                            xml,
                            null,
                            "",
                            out path,
                            out byte[] timestamp,
                            out string error);
                        {
                            if (file && objectright == null)
                                Utility.AssertResult(
                                    "SetBiblioInfo()",
                                    ret,
                                    0,
                                    channel.ErrorCode,
                                    ErrorCode.PartialDenied,
                                    error,
                                    "被拒绝");
                            else
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("创建书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo getbiblioinfo 字段限制":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo=*(200)|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var xml = GetXml();
                        var ret = channel.SetBiblioInfo(null,
                            "new",
                            _strBiblioDbName + "/?",
                            "xml",
                            xml,
                            null,
                            "",
                            out path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.PartialDenied,
        error,
        "被拒绝");

                            // 预期的 XML 中只有 200 字段
                            // 头标区要替换为 null 内容
                            {
                                Debug.Assert(_null_marc_header != null);
                                xml = Utility.ChangeElementText(xml,
                                    "//{http://dp2003.com/UNIMARC}:leader",
                                    _null_marc_header);
                                xml = Utility.RemoveElements(xml,
                                    new string[] {
                                        "//{http://dp2003.com/UNIMARC}:datafield[@tag='690']",
                                        "//{http://dp2003.com/UNIMARC}:datafield[@tag='701']",
                                    });
                            }

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                                },
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("创建书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var xml = GetXml();
                        var ret = channel.SetBiblioInfo(null,
                            "new",
                            _strBiblioDbName + "/?",
                            "xml",
                            xml,
                            null,
                            "",
                            out path,
                            out byte[] timestamp,
                            out string error);
                        {
                            if (file && objectright == null)
                                Utility.AssertResult(
"SetBiblioInfo()",
ret,
0,
channel.ErrorCode,
ErrorCode.PartialDenied,
        error);
                            else
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
        error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("创建书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        var change_role_ret = ManageHelper.ChangeDatabaseRole(
                            super_channel,
                            _strBiblioDbName,
                            (old_role) =>
                            {
                                StringUtil.SetInList(ref old_role, "orderWork", true);
                                DataModel.SetMessage($"为书目库 {_strBiblioDbName} 添加 orderWork 角色");
                                return old_role;
                            });
                        if (change_role_ret.Value == -1)
                            throw new Exception($"修改书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var xml = GetXml();
                        var ret = channel.SetBiblioInfo(null,
                            "new",
                            _strBiblioDbName + "/?",
                            "xml",
                            xml,
                            null,
                            "",
                            out path,
                            out byte[] timestamp,
                            out string error);
                        {
                            if (file && objectright == null)
                                Utility.AssertResult(
                                    "SetBiblioInfo()",
                                    ret,
                                    0,
                                    channel.ErrorCode,
                                    ErrorCode.PartialDenied,
                                    error,
                                    "不具备");
                            else
                                Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("创建书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);

                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }

        }

        // 测试修改书目记录。
        // TODO: 要增加中途 new change delete XML 中 dprms:file 元素的功能。或者可以提出去设计一个单独执行的函数
        // "权限不足"
        // "普通权限 只有 setbiblioinfo"
        // "普通权限 setbiblioinfo getbiblioinfo"
        // "存取定义 只有 setbiblioinfo"
        // "存取定义 setbiblioinfo getbiblioinfo"
        // "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向未修改字段)"
        // "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向修改字段)"
        // "存取定义 order"
        // "存取定义 order 数据库 orderWork 角色"
        // "存取定义 setbiblioinfo=ownerchange"
        // "存取定义 setbiblioinfo=ownerchange 998$z相同"
        // "存取定义 setbiblioinfo=ownerchange 998$z不相同"
        static NormalResult TestChange(
            string name,
            string style,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DataModel.SetMessage($"===> 项目 修改书目记录 {name} 情形, style={style} ...");

            var file = StringUtil.IsInList("file", style);
            var objectright = StringUtil.GetParameterByPrefix(style, "objectright");
            string append_o_right()
            {
                if (objectright == null)
                    return "";
                return "," + objectright.Replace("|", ",");
            }

            string append_o_access()
            {
                if (objectright == null)
                    return "";
                return "|" + objectright;
            }

            var origin_xml = GetXml();
            byte[] origin_timestamp = null;
            byte[] last_timestamp = null;
            string path = "";

            string changed_xml;
            // 修改了原有 XML 中的 title
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(origin_xml);
                Utility.ChangeElementText(dom,
                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']/{http://dp2003.com/UNIMARC}:subfield[@code='a']",
                    // "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                    "changed title"
                    );
                changed_xml = dom.DocumentElement.OuterXml;
            }

            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                if (name.Contains("998$z相同"))
                {
                    record.setFirstSubfield("998", "z", "_test_account");
                }
                else if (name.Contains("998$z不相同"))
                {
                    record.setFirstSubfield("998", "z", "supervisor");
                }
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }

            string PrepareRecord(LibraryChannel c)
            {
                var ret = c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    origin_xml,
                    null,
                    "",
                    out string output_path,
                    out origin_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        origin_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                return output_path;
            }

            long DoChange(LibraryChannel c, string p, out string error)
            {
                return c.SetBiblioInfo(null,
                    "change",
                    p,
                    "xml",
                    changed_xml,
                    null,
                    "",
                    out string output_path,
                    out byte[] timestamp,
                    out error);
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path))
                    return;
                int redo_count = 0;
            REDO:
                var ret = c.SetBiblioInfo(null,
                    "delete",
                    path,
                    "xml",
                    "",
                    last_timestamp,
                    "",
                    out _,
                    out last_timestamp,
                    out string error);
                if (ret == -1)
                {
                    if (redo_count < 2
                        && c.ErrorCode == ErrorCode.TimestampMismatch)
                    {
                        redo_count++;
                        goto REDO;
                    }
                    throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                }
            }

            // 断言记录没有被修改。
            // parameters:
            //      new_timestamp   如果此参数不为 null，则表示比较时候用它而不是 origin_timestamp
            void AssertNotChange(byte[] new_timestamp = null)
            {
                // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                var channel = DataModel.GetChannel();
                try
                {
                    var verify_ret = Utility.VerifyBiblioRecord(channel,
                        path,
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        origin_xml,
                        new_timestamp == null ? origin_timestamp : new_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"发现此时记录 {path} 已经被改变: {verify_ret.ErrorInfo}");
                }
                finally
                {
                    DataModel.ReturnChannel(channel);
                }
            }

            switch (name)
            {
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        Debug.Assert(string.IsNullOrEmpty(path) == false);
                        var ret = DoChange(channel, path, out string error);
                        {
                            Utility.AssertResult(
                "SetBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied,
                error);

                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoChange(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");

                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo,getbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                changed_xml,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo" + append_o_access(),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoChange(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange();

                            /*
                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                origin_xml,
                                origin_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                            */
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                changed_xml,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向未修改字段)":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 300 字段。数据中修改了 200 字段内容
                            Access = $"{_strBiblioDbName}:setbiblioinfo=*(300)|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.PartialDenied,
        error,
        "字段 200 被拒绝修改");

                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange(timestamp);

                            /*
                            // 预期的 XML 中所有字段都没有被修改

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                origin_xml,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                            */
                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 setbiblioinfo getbiblioinfo 字段限制(指向修改字段)":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 200 字段。数据中修改了 200 字段内容
                            Access = $"{_strBiblioDbName}:setbiblioinfo=*(200)|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 预期的 XML 中 200 字段 被修改

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                    //"//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                                },
                                new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                                },
                                changed_xml,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "非工作库");
                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange();

                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", true);
                                    DataModel.SetMessage($"为书目库 {_strBiblioDbName} 添加 orderWork 角色");
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"修改书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError, error);

                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                changed_xml,
                                timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("修改书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo=ownerchange":
                case "存取定义 setbiblioinfo=ownerchange 998$z相同":
                    return Utility._test(
                        (super_channel) =>
                        {
                            path = PrepareRecord(super_channel);

                            return new UserInfo
                            {
                                UserName = "_test_account",
                                Rights = "",
                                Access = $"{_strBiblioDbName}:setbiblioinfo=ownerchange|getbiblioinfo" + append_o_access()
                            };
                        },
                        (channel, user) =>
                        {
                            var ret = channel.SetBiblioInfo(null,
                                "change",
                                path,
                                "xml",
                                changed_xml,
                                origin_timestamp,
                                "",
                                out string output_path,
                                out byte[] timestamp,
                                out string error);
                            {
                                Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        channel.ErrorCode,
                        ErrorCode.NoError,
                        error);

                                // GetBiblioInfo() 验证 XML 记录是否符合预期
                                var verify_ret = Utility.VerifyBiblioRecord(channel,
                                    path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    changed_xml,
                                    timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }

                            DataModel.SetMessage("修改书目记录成功", "green");
                            return new NormalResult();
                        },
                        // clean up
                        (super_channel) =>
                        {
                            DeleteRecord(super_channel);
                        },
                        token);

                case "存取定义 setbiblioinfo=ownerchange 998$z不相同":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo=ownerchange|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = channel.SetBiblioInfo(null,
                            "change",
                            path,
                            "xml",
                            changed_xml,
                            origin_timestamp,
                            "",
                            out string output_path,
                            out byte[] timestamp,
                            out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "创建者");

                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            AssertNotChange();

                            /*
                            // GetBiblioInfo() 验证 XML 记录是否符合预期
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                origin_xml,
                                origin_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                            */
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }
        }

        // "权限不足"
        // "普通权限 只有 setbiblioinfo"
        // "普通权限 setbiblioinfo getbiblioinfo"
        // "存取定义 只有 setbiblioinfo"
        // "存取定义 setbiblioinfo getbiblioinfo"

        // "存取定义 setbiblioinfo=change getbiblioinfo"
        // "存取定义 setbiblioinfo=(否定) getbiblioinfo"

        // "存取定义 setbiblioinfo getbiblioinfo 字段限制"
        // "存取定义 order"
        // "存取定义 order 数据库 orderWork 角色"
        // "存取定义 setbiblioinfo=ownerdelete"
        // "存取定义 setbiblioinfo=ownerdelete 998$z相同"
        // "存取定义 setbiblioinfo=ownerdelete 998$z不相同"
        // "存取定义 order|setbiblioinfo 后者优先"
        static NormalResult TestDelete(
            string name,
            string style,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DataModel.SetMessage($"===> 项目 删除书目记录 {name} 情形, style={style} ...");

            var file = StringUtil.IsInList("file", style);
            var objectright = StringUtil.GetParameterByPrefix(style, "objectright");
            string append_o_right()
            {
                if (objectright == null)
                    return "";
                return "," + objectright.Replace("|", ",");
            }

            string append_o_access()
            {
                if (objectright == null)
                    return "";
                return "|" + objectright;
            }

            var origin_xml = GetXml();
            byte[] origin_timestamp = null;
            byte[] last_timestamp = null;
            string path = "";

            string changed_xml;
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(origin_xml);
                Utility.ChangeElementText(dom,
                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']/{http://dp2003.com/UNIMARC}:subfield[@code='a']",
                    // "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                    "changed title"
                    );
                changed_xml = dom.DocumentElement.OuterXml;
            }

            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                if (name.Contains("998$z相同"))
                {
                    record.setFirstSubfield("998", "z", "_test_account");
                }
                else if (name.Contains("998$z不相同"))
                {
                    record.setFirstSubfield("998", "z", "supervisor");
                }
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }

            string PrepareRecord(LibraryChannel c)
            {
                var ret = c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    origin_xml,
                    null,
                    "",
                    out string output_path,
                    out origin_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
"SetBiblioInfo()",
ret,
0,
c.ErrorCode,
ErrorCode.NoError,
error);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        origin_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                return output_path;
            }

            long DoDelete(LibraryChannel c, string p, out string error)
            {
                var ret = c.SetBiblioInfo(null,
                    "delete",
                    p,
                    "xml",
                    "",
                    origin_timestamp,
                    "",
                    out string output_path,
                    out byte[] timestamp,
                    out error);
                if (ret == -1)
                {
                    // DataModel.SetMessage($"SetBiblioInfo() action delete fail: {error}, ErrorCode:{c.ErrorCode.ToString()}");
                }
                return ret;
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path))
                    return;
                int redo_count = 0;
            REDO:
                var ret = c.SetBiblioInfo(null,
                    "delete",
                    path,
                    "xml",
                    "",
                    last_timestamp,
                    "",
                    out _,
                    out last_timestamp,
                    out string error);
                if (ret == -1)
                {
                    if (redo_count < 2
                        && c.ErrorCode == ErrorCode.TimestampMismatch)
                    {
                        redo_count++;
                        goto REDO;
                    }
                    throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                }
            }

            switch (name)
            {
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        Debug.Assert(string.IsNullOrEmpty(path) == false);
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
                "SetBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied,
                error,
                "被拒绝");
                        }

                        // 验证记录依然存在
                        AssertExist(channel, path);

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "权限安全性规则");
                        }

                        // 验证记录依然存在
                        AssertExist(channel, path);

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "setbiblioinfo,getbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            if (file && objectright == null)
                            {
                                Utility.AssertResult(
"SetBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"权限不足以删除所有MARC字段");
                                AssertExist(channel, path);
                            }
                            else
                            {
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);

                                // 验证记录已经不存在
                                AssertNotExist(channel, path);
                                path = "";
                            }
                        }

                        DataModel.SetMessage("删除书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "权限安全性规则");
                            // 验证记录依然存在
                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            if (file && objectright == null)
                            {
                                Utility.AssertResult(
"SetBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以");
                                AssertExist(channel, path);
                            }
                            else
                            {
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);

                                AssertNotExist(channel, path);
                                path = "";
                            }
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 setbiblioinfo=change getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限中 action 错为 change
                            Access = $"{_strBiblioDbName}:setbiblioinfo=change|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo=(否定) getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限为否定的 setbiblioinfo
                            Access = $"{_strBiblioDbName}:setbiblioinfo=|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 setbiblioinfo getbiblioinfo 字段限制":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 300 字段。数据中修改了 200 字段内容
                            Access = $"{_strBiblioDbName}:setbiblioinfo=*(300)|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不足以");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "非工作库");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", true);
                                    DataModel.SetMessage($"为书目库 {_strBiblioDbName} 添加 orderWork 角色");
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"修改书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            if (file && objectright == null)
                            {
                                Utility.AssertResult(
"SetBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以");

                                AssertExist(channel, path);
                            }
                            else
                            {
                                Utility.AssertResult(
            "SetBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);

                                AssertNotExist(channel, path);
                                path = "";
                            }
                        }

                        DataModel.SetMessage("删除书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 setbiblioinfo=ownerdelete":
                case "存取定义 setbiblioinfo=ownerdelete 998$z相同":
                    return Utility._test(
                        (super_channel) =>
                        {
                            path = PrepareRecord(super_channel);

                            return new UserInfo
                            {
                                UserName = "_test_account",
                                Rights = "",
                                Access = $"{_strBiblioDbName}:setbiblioinfo=ownerdelete|getbiblioinfo" + append_o_access()
                            };
                        },
                        (channel, user) =>
                        {
                            var ret = DoDelete(channel, path, out string error);
                            {
                                if (file && objectright == null)
                                {
                                    Utility.AssertResult(
"SetBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以");

                                    AssertExist(channel, path);
                                }
                                else
                                {
                                    Utility.AssertResult(
                            "SetBiblioInfo()",
                            ret,
                            0,
                            channel.ErrorCode,
                            ErrorCode.NoError,
                            error);

                                    AssertNotExist(channel, path);
                                    path = "";
                                }
                            }

                            DataModel.SetMessage("删除书目记录成功", "green");
                            return new NormalResult();
                        },
                        // clean up
                        (super_channel) =>
                        {
                            DeleteRecord(super_channel);
                        },
                        token);

                case "存取定义 setbiblioinfo=ownerdelete 998$z不相同":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:setbiblioinfo=ownerdelete|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "创建者");
                            AssertErrorInfo(error, "创建者");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order|setbiblioinfo 后者优先":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 因为 order 和 setbiblioinfo 后者优先，所以 *(000) 应该造成 ErrorCode.AccessDenied
                            Access = $"{_strBiblioDbName}:order|setbiblioinfo=*(000)|getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoDelete(channel, path, out string error);
                        {
                            Utility.AssertResult(
        "SetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error);
                            AssertErrorInfo(error, "不足以删除所有MARC字段");

                            AssertExist(channel, path);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }
        }

        // TODO: 验证路径错误，记录不存在，返回 0
        /*
        "权限不足"
        "普通权限 getbiblioinfo"
        "存取定义 getbiblioinfo"
        "存取定义 order"
        "存取定义 getbiblio=(否定)"
        "存取定义 getbiblioinfo 字段限制"
        "存取定义 order 数据库 orderWork 角色"
        "存取定义 order|getbiblioinfo 后者优先"
        */
        // parameters:
        //      style   附加风格。如果包含 file，表示要在测试的书目记录 XML 中包含 dprms:file 元素
        static NormalResult TestGet(
    string name,
    string style,
    CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DataModel.SetMessage($"===> 项目 获取书目记录 {name} 情形, style={style} ...");

            var file = StringUtil.IsInList("file", style);

            var objectright = StringUtil.GetParameterByPrefix(style, "objectright");
            string append_o_right()
            {
                if (objectright == null)
                    return "";
                return "," + objectright.Replace("|", ",");
            }

            string append_o_access()
            {
                if (objectright == null)
                    return "";
                return "|" + objectright;
            }

            var origin_xml = GetXml();
            byte[] origin_timestamp = null;
            string path = "";

            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }

            string PrepareRecord(LibraryChannel c)
            {
                var ret = c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    origin_xml,
                    null,
                    "",
                    out string output_path,
                    out origin_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
"SetBiblioInfo()",
ret,
0,
c.ErrorCode,
ErrorCode.NoError,
error);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        origin_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                return output_path;
            }

            long DoGet(LibraryChannel c,
                string p,
                out string xml,
                out string error)
            {
                xml = "";
                var ret = c.GetBiblioInfos(null,
                    p,
                    "",
                    new string[] { "xml" },
                    out string[] results,
                    out byte[] timestamp,
                    out error);
                if (ret == -1)
                {
                    DataModel.SetMessage($"GetBiblioInfos() fail: ErrorInfo:{error}, ErrorCode:{c.ErrorCode.ToString()}");
                }
                else
                {
                    DataModel.SetMessage($"GetBiblioInfos() return {ret}。ErrorInfo:{error}, ErrorCode:{c.ErrorCode.ToString()}");
                    xml = results[0];
                }
                return ret;
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path))
                    return;
                var last_timestamp = origin_timestamp;
                int redo_count = 0;
            REDO:
                var ret = c.SetBiblioInfo(null,
                    "delete",
                    path,
                    "xml",
                    "",
                    last_timestamp,
                    "",
                    out _,
                    out last_timestamp,
                    out string error);
                if (ret == -1)
                {
                    if (redo_count < 2
                        && c.ErrorCode == ErrorCode.TimestampMismatch)
                    {
                        redo_count++;
                        goto REDO;
                    }
                    throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                }
            }

            switch (name)
            {
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        Debug.Assert(string.IsNullOrEmpty(path) == false);
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
                "GetBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied, error);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "getbiblioinfo" + append_o_right()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.NoError, error);

                            // file 情形，返回的 XML 中没有 dprms:file 元素
                            var verify_ret = Utility.VerifyBiblioRecord(origin_xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
                                });
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("获得书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:getbiblioinfo" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 验证获得了全部内容
                            var verify_ret = Utility.VerifyBiblioRecord(origin_xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
                                });
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("获得书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access(),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 验证获得了全部内容
                            var verify_ret = Utility.VerifyBiblioRecord(origin_xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
                                });
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("获得书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 getbiblio=(否定)":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限为否定的 getbiblioinfo
                            Access = $"{_strBiblioDbName}:getbiblioinfo=" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 getbiblioinfo 字段限制":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能读取 200 字段内容
                            Access = $"{_strBiblioDbName}:getbiblioinfo=*(200)" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.PartialDenied,
        error);
                            AssertErrorInfo(error, "###,690,701");

                            Utility.AssertHasElements(xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']"
                                });

                            Utility.AssertMissingElements(xml,
                                new string[]
                                {
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='690']",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='701']",
                                });
                            if (file && objectright == null)
                                Utility.AssertMissingElements(xml,
                                    new string[] { "//{http://dp2003.com/dprms}:file" }
                                    );

                            AssertNullMarcHeader(xml);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", true);
                                    DataModel.SetMessage($"为书目库 {_strBiblioDbName} 添加 orderWork 角色");
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"修改书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = $"{_strBiblioDbName}:order" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 验证获得了全部内容
                            var verify_ret = Utility.VerifyBiblioRecord(origin_xml,
                                file && objectright == null ? new string[] { "//{http://dp2003.com/dprms}:file" } : null,
                                xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
                                });
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

                        DataModel.SetMessage("获得书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 order|getbiblioinfo 后者优先":
                    return Utility._test(
                    (super_channel) =>
                    {
                        path = PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 因为 order 和 getbiblioinfo 后者优先，所以 *(200) 应该造成 ErrorCode.PartialDenied
                            Access = $"{_strBiblioDbName}:order|getbiblioinfo=*(200)" + append_o_access()
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoGet(channel,
                            path,
                            out string xml,
                            out string error);
                        {
                            Utility.AssertResult(
        "GetBiblioInfo()",
        ret,
        1,
        channel.ErrorCode,
        ErrorCode.PartialDenied,
        error,
        "###,690,701");
                            AssertErrorInfo(error, "###,690,701");

                            Utility.AssertHasElements(xml,
                                new string[]
                                {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']"
                                });

                            Utility.AssertMissingElements(xml,
                                new string[]
                                {
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='690']",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='701']",
                                });
                            if (file && objectright == null)
                                Utility.AssertMissingElements(xml,
                                    new string[] { "//{http://dp2003.com/dprms}:file" }
                                    );

                            AssertNullMarcHeader(xml);
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }
        }

        public static void AssertNullMarcHeader(string xml)
        {
            Debug.Assert(_null_marc_header != null);
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            var nodes = Utility.SelectNodes(dom, "//{http://dp2003.com/UNIMARC}:leader");
            if (nodes.Count == 0)
                throw new Exception("xml 中没有包含 unimarc:leader 元素");
            if (nodes.Count > 1)
                throw new Exception($"xml 中包含的 unimarc:leader 元素多于一个 ({nodes.Count})");

            if (nodes[0].InnerText.Trim() != _null_marc_header)
                throw new Exception($"xml 中包含的 unimarc:leader 元素内容 '{nodes[0].InnerText.Trim()}' 不等于期望的 '{_null_marc_header}'");
        }

        public static void AssertErrorInfo(string error, string part)
        {
            if (error.Contains(part) == false)
                throw new Exception($"错误字符串中期待包含 '{part}'。但现在却是 '{error}'");
        }
        public static void AssertExist(LibraryChannel c, string path)
        {
            var channel = DataModel.GetChannel();
            try
            {
                var ret = channel.GetRes(null,
                    path,
                    0,
                    1,
                    "",
                    out _,
                    out _,
                    out string output_path,
                    out _,
                    out string error);
                if (ret == -1)
                    throw new Exception($"验证记录 {path} 的存在性失败。({error})");
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        public static void AssertNotExist(LibraryChannel c, string path)
        {
            var channel = DataModel.GetChannel();
            try
            {
                var ret = channel.GetRes(null,
                path,
                0,
                1,
                "",
                out _,
                out _,
                out string output_path,
                out _,
                out string error);
                if (ret == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return;
                    throw new Exception($"验证记录 {path} 的不存在性失败。({error})");
                }
                throw new Exception($"验证记录 {path} 的不存在性失败。(记录依然存在)");
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }




        static string _strBiblioDbName = "_测试用书目";

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            try
            {
                {
                    // 利用默认账户进行准备操作
                    LibraryChannel channel = DataModel.GetChannel();
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromMinutes(10);

                    try
                    {
                        // 创建测试所需的书目库

                        // 如果测试用的书目库以前就存在，要先删除。
                        DataModel.SetMessage("正在删除测试用书目库 ...");
                        string strOutputInfo = "";
                        lRet = channel.ManageDatabase(
            null,
            "delete",
            _strBiblioDbName,    // strDatabaseNames,
            "",
            "",
            out strOutputInfo,
            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                                goto ERROR1;
                        }

                        DataModel.SetMessage("正在创建测试用书目库 ...");
                        // 创建一个书目库
                        // parameters:
                        // return:
                        //      -1  出错
                        //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                        //      1   成功创建
                        nRet = ManageHelper.CreateBiblioDatabase(
                            channel,
                            null,
                            _strBiblioDbName,
                            "series",   // _dbType == "issue" ? "series" : "book",
                            "unimarc",
                            "*",
                            "",
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 临时设置 不校验条码号
                        lRet = channel.SetSystemParameter(null,
                            "circulation",
                            "?VerifyBarcode",
                            "false",
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        lRet = channel.GetSystemParameter(null,
                            "utility",
                            "null_marc_header",
                            out string value,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                            goto ERROR1;
                        _null_marc_header = value;

                        // 验证 supervisor 账户是否具备一些基本权限
                        var rights = channel.Rights;
                        if (StringUtil.IsInList("getobject,getbiblioinfo", rights) == false)
                            throw new Exception($"{channel.UserName} 账户缺乏 getobject 或 getbiblioobject 权限");
                        if (StringUtil.IsInList("setobject,setbiblioinfo", rights) == false)
                            throw new Exception($"{channel.UserName} 账户缺乏 setobject 或 setbiblioobject 权限");
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        DataModel.ReturnChannel(channel);
                    }
                }

#if REMOVED

                {
                    // 利用 test_right 账户进行准备操作。评注库库要求原始创建者才能修改评注记录
                    LibraryChannel channel = DataModel.NewChannel("test_rights", "");
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromMinutes(10);
                    try
                    {
                        // 创建书目记录和下级记录
                        {
                            // *** 创建书目记录
                            string path = _strBiblioDbName + "/?";
                            var record = BuildBiblioRecord("测试题名", "");

                            string strXml = "";
                            nRet = MarcUtil.Marc2XmlEx(
                record.Text,
                "unimarc",
                ref strXml,
                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            DataModel.SetMessage("正在创建书目记录");
                            lRet = channel.SetBiblioInfo(null,
                                "new",
                                path,
                                "xml", // strBiblioType
                                strXml,
                                null,
                                null,
                                "", // style
                                out string output_biblio_recpath,
                                out byte[] new_timestamp,
                                out strError);
                            if (lRet == -1)
                                goto ERROR1;

                            _biblio_recpath = output_biblio_recpath;

                            // 创建册记录
                            DataModel.SetMessage($"正在创建书目的下级({_dbType})记录");
                            EntityInfo[] errorinfos = null;
                            var entities = BuildEntityRecords();
                            if (_dbType == "item")
                                lRet = channel.SetEntities(null,
                                    output_biblio_recpath,
                                    entities,
                                    out errorinfos,
                                    out strError);
                            else if (_dbType == "order")
                                lRet = channel.SetOrders(null,
                output_biblio_recpath,
                entities,
                out errorinfos,
                out strError);
                            else if (_dbType == "issue")
                                lRet = channel.SetIssues(null,
                output_biblio_recpath,
                entities,
                out errorinfos,
                out strError);
                            else if (_dbType == "comment")
                            {

                                lRet = channel.SetComments(null,
                output_biblio_recpath,
                entities,
                out errorinfos,
                out strError);

                            }
                            else
                                throw new ArgumentException("_dbType error");
                            if (lRet == -1)
                                goto ERROR1;
                            strError = Utility.GetError(errorinfos, out ErrorCodeValue error_code);
                            if (string.IsNullOrEmpty(strError) == false)
                            {
                                if (_dbType == "comment" && error_code == ErrorCodeValue.PartialDenied)
                                {
                                    // 创建评注记录，XML 中的 creator 元素，会导致返回 PartialDenied 错误码
                                }
                                else
                                    goto ERROR1;
                            }
                            _itemRecordPath = errorinfos[0].NewRecPath;
                        }
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        DataModel.DeleteChannel(channel);
                    }
                }

#endif
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "PrepareEnvironment() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }

        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }




        public static NormalResult Finish()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用书目库 ...");
                string strOutputInfo = "";
                long lRet = channel.ManageDatabase(
    null,
    "delete",
    _strBiblioDbName,    // strDatabaseNames,
    "",
    "",
    out strOutputInfo,
    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        goto ERROR1;
                }

                var ret = Utility.DeleteMemoryUsers(channel);
                if (ret.Value == -1)
                {
                    strError = ret.ErrorInfo;
                    goto ERROR1;
                }

                DataModel.SetMessage("结束");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "Finish() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


    }
}
