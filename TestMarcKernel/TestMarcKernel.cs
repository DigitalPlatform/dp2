using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace TestMarcKernel
{
    [TestClass]
    public class MarcKernelUnitTest
    {

        // static string CONTROL_NULL = MarcDiff.GetBlankHeader();
        static string NULL_HEADER = MarcDiff.GetNullHeader();

        // 权限足够时的 null 头标区内容
        // public const string CONTROL_NULL = "*???????????????????????";


        // 测试和 ?????? 头标区内部用法冲突的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_0_0()
        {
            //string strComment = "";
            //string strError = "";
            //int nRet = 0;

            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            /*
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));
            */
            string header = new string ('?', 24);

            MarcRecord new_record = new MarcRecord();
            new_record.Header[0, 24] = header;
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            target_record.Header[0, 24] = header;
            // 结果记录有三个 856 字段
            // 结果记录的第二个 856 字段兑现了修改，增加了 $zZZZ。旧记录的第一个 856 字段得到了保护
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");


        }

        // 保留 old_record 的头标区内容
        [TestMethod]
        public void MarcDiff_MergeOldNew_0_1()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:000-999"; // 除了 ### 头标区，所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            old_record.Header[0, 24] = new string('1', 24);

            MarcRecord new_record = new MarcRecord();
            new_record.Header[0, 24] = new string('?', 24);
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果的头标区等于 old_record 的头标区。因为字段权限里面缺乏 ### 权限
            target_record.Header[0, 24] = old_record.Header[0, 24];
            // 结果记录有三个 856 字段
            // 结果记录的第二个 856 字段兑现了修改，增加了 $zZZZ。旧记录的第一个 856 字段得到了保护
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "字段 ### 被拒绝修改");


        }

        // 保留 old_record 的头标区内容。但碰巧 old_record 和 new_record 的头标区完全一致，strComment 中不会出现 "字段 ### 被拒绝修改"
        [TestMethod]
        public void MarcDiff_MergeOldNew_0_2()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:000-999"; // 除了 ### 头标区，所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            old_record.Header[0, 24] = new string('1', 24);

            MarcRecord new_record = new MarcRecord();
            new_record.Header[0, 24] = new string('1', 24);
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果的头标区等于 old_record 的头标区。因为字段权限里面缺乏 ### 权限
            target_record.Header[0, 24] = old_record.Header[0, 24];
            // 结果记录有三个 856 字段
            // 结果记录的第二个 856 字段兑现了修改，增加了 $zZZZ。旧记录的第一个 856 字段得到了保护
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");


        }

        // old 记录 MARC 为空
        [TestMethod]
        public void MarcDiff_MergeOldNew_0_3()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:000-999"; // 除了 ### 头标区，所有字段都允许操作

            string old_marc = "";   // MARC 完全为空。合并后，会用空值填充头标区

            MarcRecord new_record = new MarcRecord();
            new_record.Header[0, 24] = new string('?', 24);
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果的头标区等于 old_record 的头标区。因为字段权限里面缺乏 ### 权限
            target_record.Header[0, 24] = NULL_HEADER;
            // 结果记录有三个 856 字段
            // 结果记录的第二个 856 字段兑现了修改，增加了 $zZZZ。旧记录的第一个 856 字段得到了保护
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_marc,
            new_record.Text,
            target_record.Text,
            "字段 ### 被拒绝修改");
        }


        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户权限不足，获取到缺少一个 856 字段的记录，然后修改其中一个字段以后保存
        // 要点：要保护好旧记录中的第一个 856 字段，并允许前端修改第二个
        [TestMethod]
        public void MarcDiff_MergeOldNew_1_1()
        {
            //string strComment = "";
            //string strError = "";
            //int nRet = 0;

            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限禁止当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));
#if NO
            string strOldMarc = old_record.Text;

            nRet = LibraryApplication.MaskCantGet856(
    strUserRights,
    ref strOldMarc,
    out strError);
            if (nRet == -1)
                throw new Exception(strError);
            Assert.AreEqual(nRet, 1, "应当是只标记了一个 856 字段");
#endif

            MarcRecord new_record = new MarcRecord();
            // 新记录有两个 856 字段，相当于丢失了旧记录的第一个 856 字段，这模仿了权限不足的用户编辑修改保存的过程。
            // 新记录的第一个 856 字段被修改了，增加了 $zZZZ
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage$zZZZ"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));
#if NO
            string strNewMarc = new_record.Text;

            // 对 strNewMarc 进行过滤，将那些当前用户无法读取的 856 字段删除
            // 对 MARC 记录进行过滤，将那些当前用户无法读取的 856 字段删除
            // return:
            //      -1  出错
            //      其他  滤除的 856 字段个数
            nRet = LibraryApplication.MaskCantGet856(
                strUserRights,
                ref strNewMarc,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            Assert.AreEqual(nRet, 0, "应当没有标记任何 856 字段");
#endif

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            // 结果记录的第二个 856 字段兑现了修改，增加了 $zZZZ。旧记录的第一个 856 字段得到了保护
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage$zZZZ"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");

#if NO
            // 按照字段修改权限定义，合并新旧两个 MARC 记录
            // parameters:
            //      strDefaultOperation   insert/replace/delete 之一或者逗号间隔组合
            // return:
            //      -1  出错
            //      0   成功
            //      1   有部分修改要求被拒绝
            nRet = MarcDiff.MergeOldNew(
                "insert,replace,delete",
                strFieldNameList,
                strOldMarc,
                ref strNewMarc,
                out strComment,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            // 检查 strNewMarc
            if (strNewMarc != target_record.Text)
                throw new Exception("和期望的结果不符合");

            // 检查 strComment
#endif

        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户权限不足，获取到缺少一个 856 字段的记录，然后修改其中二个字段以后保存
        // 要点：要保护好旧记录中的第一个 856 字段，并允许前端修改第二个第三个
        [TestMethod]
        public void MarcDiff_MergeOldNew_1_2()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限禁止当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录有两个 856 字段，相当于丢失了旧记录的第一个 856 字段，这模仿了权限不足的用户编辑修改保存的过程。
            // 新记录的两个字段被修改了
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage$zZZZ2"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage$zZZZ3"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage$zZZZ2"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage$zZZZ3"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户权限不足，获取到缺少一个 856 字段的记录，然后修改最后一个字段以后保存
        // 要点：要保护好旧记录中的第一个 856 字段，并允许前端修改最后一个
        [TestMethod]
        public void MarcDiff_MergeOldNew_1_3()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限禁止当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录有两个 856 字段，相当于丢失了旧记录的第一个 856 字段，这模仿了权限不足的用户编辑修改保存的过程。
            // 新记录的最后一个字段被修改了
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage$zZZZ3"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage$zZZZ3"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户权限不足，获取到缺少一个 856 字段的记录，然后什么都不修改，保存回去
        // 要点：要保护好旧记录中的第一个 856 字段，后两个字段不应有变化
        [TestMethod]
        public void MarcDiff_MergeOldNew_1_4()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限禁止当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录有两个 856 字段，相当于丢失了旧记录的第一个 856 字段，这模仿了权限不足的用户编辑修改保存的过程。
            // 新记录的两个字段都没有被修改
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户修改一个 856 字段，导致权限不足，保存回去
        // 要点：要保护好旧记录中的这个 856 字段，这种情况是不允许修改的
        [TestMethod]
        public void MarcDiff_MergeOldNew_2_1()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限允许当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录的第一个字段被修改，即将导致权限不足
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段。第一个字段没有允许修改
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户删除一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_3_1()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限允许当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录的第一个字段被删除
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户删除一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_3_2()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限允许当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录的第二个字段被删除
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户删除一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_3_3()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录有三个 856 字段，第一个的权限允许当前用户访问
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            old_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录的第三个字段被删除
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_4_1()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加一个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有一个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加二个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_4_2()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加二个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加三个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_4_3()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加三个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 856 字段。原来已有一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_4_4()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加一个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加二个 856 字段。原来已有一个 856 字段
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_4_5()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加二个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有三个 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:level-1"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "{null}");
        }


        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_1()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加一个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录没有 856 字段
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加二个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_2()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加二个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有一个 856 字段。其中一个打算增加的被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加二个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_3()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加二个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage;rights:group"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有一个 856 字段。其中一个打算增加的被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加三个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_4()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加三个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage;rights:group"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段。其中一个打算增加的被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加三个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_5()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加三个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage;rights:group"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段。其中一个打算增加的被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加三个 856 字段，但权限被限制的情况
        // 要点：这是全部字段权限都足够的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_5_6()
        {
            string strUserRights = "level-1";
            string strFieldNameList = "*:***-***"; // 所有字段都允许操作

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 856 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加三个 856 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            new_record.add(new MarcField('$', "85642$3Cover image 3$uURL3$qimage/jpeg$xtype:FrontCover.LargeImage;rights:group"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // target_record.Header[0, 24] = CONTROL_NULL;
            // 结果记录有二个 856 字段。其中一个打算增加的被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "85642$3Cover image 1$uURL1$qimage/jpeg$xtype:FrontCover.SmallImage"));
            target_record.add(new MarcField('$', "85642$3Cover image 2$uURL2$qimage/jpeg$xtype:FrontCover.MediumImage"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "856!");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 205 字段，但权限被限制的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_1()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200";

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加一个 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "205  $a第一版"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果记录没有 205 字段。打算增加 205 字段被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户修改一个 205 字段，但权限被限制的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_2()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200";

            MarcRecord old_record = new MarcRecord();
            // 旧记录有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "205  $a第一版"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录修改 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "205  $a第二版"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果记录 205 字段的修改被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "205  $a第一版"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户删除一个 205 字段，但权限被限制的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_3()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200";

            MarcRecord old_record = new MarcRecord();
            // 旧记录有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "205  $a第一版"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录删除 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果记录 205 字段的删除被拒绝了
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "205  $a第一版"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 205 字段，权限允许的情况
        [TestMethod]
        public void MarcDiff_MergeOldNew_addField()
        {
            string[] fieldnamelist_values = new string[] {
            "*:***-***", // 所有字段都允许操作
            "205",
            "200-205",
            "100-205",
            "200,205",
            };

            foreach (string strFieldNameList in fieldnamelist_values)
            {
                string strUserRights = ""; // "level-1";

                MarcRecord old_record = new MarcRecord();
                // 旧记录没有 205 字段
                old_record.add(new MarcField("001A1234567"));
                old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
                old_record.add(new MarcField('$', "701  $aauthor"));

                MarcRecord new_record = new MarcRecord();
                // 新记录增加一个 205 字段
                new_record.add(new MarcField("001A1234567"));
                new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
                new_record.add(new MarcField('$', "205  $a第一版"));
                new_record.add(new MarcField('$', "701  $aauthor"));

                MarcRecord target_record = new MarcRecord();
                /*
                if (strFieldNameList == "*:***-***")
                    target_record.Header[0, 24] = CONTROL_NULL;
                */
                // 打算增加 205 字段被兑现
                target_record.add(new MarcField("001A1234567"));
                target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
                target_record.add(new MarcField('$', "205  $a第一版"));
                target_record.add(new MarcField('$', "701  $aauthor"));

                MarcDiff_MergeOldNew(
                strUserRights,
                strFieldNameList,
                old_record,
                new_record,
                target_record,
                "");
            }

        }

#if NO
        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户增加一个 205 字段
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_4()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200-205";

            MarcRecord old_record = new MarcRecord();
            // 旧记录没有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录增加一个 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "205  $a第一版"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 打算增加 205 字段被兑现
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "205  $a第一版"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户修改一个 205 字段
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_5()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200-205";

            MarcRecord old_record = new MarcRecord();
            // 旧记录有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "205  $a第一版"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录修改 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "205  $a第二版"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果记录 205 字段的修改被兑现
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "205  $a第二版"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

        // 常规测试
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户删除一个 205 字段
        [TestMethod]
        public void MarcDiff_MergeOldNew_6_6()
        {
            string strUserRights = ""; // "level-1";
            string strFieldNameList = "200-205";

            MarcRecord old_record = new MarcRecord();
            // 旧记录有 205 字段
            old_record.add(new MarcField("001A1234567"));
            old_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            old_record.add(new MarcField('$', "205  $a第一版"));
            old_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord new_record = new MarcRecord();
            // 新记录删除 205 字段
            new_record.add(new MarcField("001A1234567"));
            new_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            new_record.add(new MarcField('$', "701  $aauthor"));

            MarcRecord target_record = new MarcRecord();
            // 结果记录 205 字段的删除被兑现
            target_record.add(new MarcField("001A1234567"));
            target_record.add(new MarcField('$', "2001 $atitle$fauthor"));
            target_record.add(new MarcField('$', "701  $aauthor"));

            MarcDiff_MergeOldNew(
            strUserRights,
            strFieldNameList,
            old_record,
            new_record,
            target_record,
            "");
        }

#endif
        // 测试 MarcDiff.MergeOldNew() 方法
        // parameters:
        //      strCommentCheckList 期望在 strComment 中出现的值，逗号分隔
        void MarcDiff_MergeOldNew(
            string strUserRights,
            string strFieldNameList,
            MarcRecord old_record,
            MarcRecord new_record,
            MarcRecord target_record,
            string strCommentCheckList = "")
        {
            MarcDiff_MergeOldNew(
strUserRights,
strFieldNameList,
old_record.Text,
new_record.Text,
target_record.Text,
strCommentCheckList);
        }

        void MarcDiff_MergeOldNew(
    string strUserRights,
    string strFieldNameList,
    string old_marc,
    string new_marc,
    string target_marc,
    string strCommentCheckList = "")
        {
            string strComment = "";
            string strError = "";
            int nRet = 0;

            string strOldMarc = old_marc;

            LibraryApplication.delegate_checkAccess func =
                (r, n) =>
                {
                    if (r == "download" || r == "preview")
                        return strUserRights;
                    return StringUtil.IsInList(r, strUserRights) ?
                    null : $"strUserRight='{strUserRights}' 中不具备权限 '{r}'";
                };

            nRet = LibraryApplication.MaskCantGet856(
                func,
                true,
                ref strOldMarc,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            // Assert.AreEqual(nRet, 1, "应当是只标记了一个 856 字段");

            string strNewMarc = new_marc;

            // 对 strNewMarc 进行过滤，将那些当前用户无法读取的 856 字段删除
            // 对 MARC 记录进行过滤，将那些当前用户无法读取的 856 字段删除
            // return:
            //      -1  出错
            //      其他  滤除的 856 字段个数
            nRet = LibraryApplication.MaskCantGet856(
                func,   // strUserRights,
                true,
                ref strNewMarc,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            // Assert.AreEqual(nRet, 0, "应当没有标记任何 856 字段");

            // 按照字段修改权限定义，合并新旧两个 MARC 记录
            // parameters:
            //      strDefaultOperation   insert/replace/delete 之一或者逗号间隔组合
            // return:
            //      -1  出错
            //      0   成功
            //      1   有部分修改要求被拒绝
            nRet = MarcDiff.MergeOldNew(
                "insert,replace,delete",
                strFieldNameList,
                null,
                strOldMarc,
                ref strNewMarc,
                out strComment,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            // 检查 strNewMarc
            if (strNewMarc != target_marc)
            {
                var display = CompareDisplay(strNewMarc, target_marc);
                Console.WriteLine(display);
                throw new Exception($"和期望的结果不符合\r\n{display}");
            }

            // 检查 strComment
            if (string.IsNullOrEmpty(strCommentCheckList) == false)
            {
                List<string> texts = StringUtil.SplitList(strCommentCheckList);
                foreach (string text in texts)
                {
                    if (text == "{null}")
                    {
                        if (string.IsNullOrEmpty(strComment) == false)
                            throw new Exception("strComment 字符串 '" + strComment + "' 没有期望的那样为空");
                        continue;
                    }
                    nRet = strComment.IndexOf(text);
                    if (nRet == -1)
                        throw new Exception("strComment 字符串 '" + strComment + "' 中没有包含要求的片段 '" + text + "'");
                }
            }

        }

        static string CompareDisplay(string marc1, string marc2)
        {
            int width = 24;
            List<string> lines1 = new List<string>();
            List<string> lines2 = new List<string>();

            {
                var ret = MarcUtil.CvtJineiToWorksheet(marc1,
    width,
    out lines1,
    out string error);
                if (ret == -1)
                {
                    throw new InternalTestFailureException(error);
                }
            }

            {
                var ret = MarcUtil.CvtJineiToWorksheet(marc2,
    width,
    out lines2,
    out string error);
                if (ret == -1)
                {
                    throw new InternalTestFailureException(error);
                }
            }

            string blank = new string(' ', width);
            StringBuilder result = new StringBuilder();
            for(int i = 0; i< Math.Max(lines1.Count, lines2.Count); i++)
            {
                if (i < lines1.Count)
                    result.Append(lines1[i].PadRight(width, ' '));
                else
                    result.Append(blank);

                // 中间分隔条带
                result.Append(" | ");

                if (i < lines2.Count)
                    result.Append(lines2[i].PadRight(width, ' '));
                else
                    result.Append(blank);

                result.Append("\r\n");
            }

            return result.ToString();
        }

        static string GetWorkSheet(string marc)
        {
            var ret = MarcUtil.CvtJineiToWorksheet(marc,
                30,
                out List<string> lines,
                out string error);
            if (ret == -1)
            {
                throw new InternalTestFailureException(error);
            }

            string strText = "";
            foreach (string line in lines)
            {
                strText += line + "\r\n";
            }

            return strText;
        }
    }
}
