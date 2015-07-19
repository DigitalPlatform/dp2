using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Marc;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;

namespace TestMarcKernel
{
    [TestClass]
    public class MarcKernelUnitTest
    {
        // 测试 MarcDiff.MergeOldNew() 方法
        // 模拟前端用户权限不足，获取到缺少一个 856 字段的记录，然后修改其中一个字段以后保存
        // 要点：要保护好旧记录中的第一个 856 字段，并允许前端修改第二个
        [TestMethod]
        public void MarcDiff_MergeOldNew_1_1()
        {
            string strComment = "";
            string strError = "";
            int nRet = 0;

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
            string strComment = "";
            string strError = "";
            int nRet = 0;

            string strOldMarc = old_record.Text;

            nRet = LibraryApplication.MaskCantGet856(
    strUserRights,
    ref strOldMarc,
    out strError);
            if (nRet == -1)
                throw new Exception(strError);
            // Assert.AreEqual(nRet, 1, "应当是只标记了一个 856 字段");

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

    }
}
