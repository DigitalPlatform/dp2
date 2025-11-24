using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    [TestClass]
    public class TestSetBiblioInfoAccess
    {
        // 最简单直白的情况
        [TestMethod]
        public void test_check_01()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 星号 actions
        [TestMethod]
        public void test_check_02()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=*";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }


        // 否定用法
        [TestMethod]
        public void test_check_03()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=";
            string strAction = "change";

            string correct_matched_action = "";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNotNull(error);
            Assert.AreEqual("", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 直接匹配到唯一的 action
        [TestMethod]
        public void test_check_04()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=change";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 直接匹配到唯一的 'ownerchange' action
        [TestMethod]
        public void test_check_11()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=ownerchange";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // ownerchange 和 change 混合在一起。会被认为是 change
        [TestMethod]
        public void test_check_12()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=ownerchange,change";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 将上一个顺序颠倒
        [TestMethod]
        public void test_check_13()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=change,ownerchange";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // * 会被认为是 change
        [TestMethod]
        public void test_check_14()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=*";
            string strAction = "change";

            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // *,ownerchange 这是一种不合法的情况，应该抛出异常
        // 注: 2025/11/12 不再抛出异常
        [TestMethod]
        public void test_check_15()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=*,ownerchange";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            try
            {
                var error = ProcessCheckSetBiblioInfoAccess(
                    normal_rights,
                    access_string,
                    strAction,
                    out _,
                    out string matched_action,
                    out string matched_operation,
                    out bool owner_only);
                // Assert.Fail("期望抛出异常，没有抛出");
                Assert.IsNull(error);
                Assert.AreEqual(correct_matched_action, matched_action);
                Assert.AreEqual(correct_owner_only, owner_only);
            }
            catch (ArgumentException ex)
            {
                // 期望抛出异常
                Assert.IsTrue(ex.Message.IndexOf("不合法") != -1);
            }
        }

        // order 和 setbiblioinfo 混合在一起。优先匹配 setbiblioinfo
        [TestMethod]
        public void test_check_21()
        {
            string normal_rights = "";
            string access_string = "中文图书:order=change|setbiblioinfo=ownerchange";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 顺序颠倒一下。依然优先匹配 setbiblioinfo
        [TestMethod]
        public void test_check_22()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=ownerchange|order=change";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // setbiblioinfo 和 writerecord 混合在一起。优先匹配 setbiblioinfo
        [TestMethod]
        public void test_check_23()
        {
            string normal_rights = "";
            string access_string = "中文图书:writerecord=change|setbiblioinfo=ownerchange";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // order 和 writerecord 混合在一起。优先匹配 writerecord
        [TestMethod]
        public void test_check_24()
        {
            string normal_rights = "";
            string access_string = "中文图书:order=change|writerecord=ownerchange";
            string strAction = "change";

            string correct_matched_action = "ownerchange";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out _,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("writerecord", matched_operation);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 验证 parameters 输出
        // 格式: dbname1=action1(parameters1),action2;dbname2=action1,action2
        [TestMethod]
        public void test_check_31()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=change(200-300)";
            string strAction = "change";

            string correct_matched_parameters = "200-300";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // parameters 为空
        [TestMethod]
        public void test_check_32()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=change";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // parameters 为空
        [TestMethod]
        public void test_check_33()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=change()";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // parameters 为空
        [TestMethod]
        public void test_check_34()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=*";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // parameters 为空
        [TestMethod]
        public void test_check_35()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 匹配 order 以后，要求中文图书库 的 role 包含 orderWork，才允许写入
        [TestMethod]
        public void test_check_41()
        {
            string normal_rights = "";
            string access_string = "中文图书:order";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only/*,
                "orderWork"*/);
            Assert.IsNull(error);
            Assert.AreEqual("order", matched_operation);    // 返回了 order
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        [TestMethod]
        public void test_check_42()
        {
            string normal_rights = "";
            string access_string = "中文图书:order";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only/*,
                ""*/);
            Assert.IsNull(error);    // 应该不报错
            Console.WriteLine(error);
            Assert.AreEqual("order", matched_operation);
            // Assert.IsTrue(error.Contains("orderWork"));
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 数据库名匹配不上
        [TestMethod]
        public void test_check_51()
        {
            string normal_rights = "";
            string access_string = "西文图书:setbiblioinfo";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNotNull(error);
            Assert.AreEqual("", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 多数据库名
        [TestMethod]
        public void test_check_52()
        {
            string normal_rights = "";
            string access_string = "西文图书,中文图书:setbiblioinfo";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "change";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // operation 匹配不上
        [TestMethod]
        public void test_check_53()
        {
            string normal_rights = "";
            string access_string = "中文图书:getbiblioinfo";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Console.WriteLine(error);
            Assert.IsNotNull(error);
            Assert.AreEqual("", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // action 部分匹配不上
        [TestMethod]
        public void test_check_54()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=delete";
            string strAction = "change";

            string correct_matched_parameters = "";
            string correct_matched_action = "";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Console.WriteLine(error);
            Assert.IsNotNull(error);
            Assert.AreEqual("", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 匹配 change 之外的 action，比如 delete
        [TestMethod]
        public void test_check_61()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=delete";
            string strAction = "delete";

            string correct_matched_parameters = "";
            string correct_matched_action = "delete";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        [TestMethod]
        public void test_check_62()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=ownerdelete";
            string strAction = "delete";

            string correct_matched_parameters = "";
            string correct_matched_action = "ownerdelete";
            bool correct_owner_only = true;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // 实际上 new 并不存在对应的 ownernew 存取定义 action
        // 测试误用的情况
        [TestMethod]
        public void test_check_71()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=ownernew";
            string strAction = "new";

            string correct_matched_parameters = "";
            string correct_matched_action = "";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Console.WriteLine(error);
            Assert.IsNotNull(error);
            Assert.AreEqual("", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }

        // new 就是只有 new
        [TestMethod]
        public void test_check_72()
        {
            string normal_rights = "";
            string access_string = "中文图书:setbiblioinfo=new(***)";
            string strAction = "new";

            string correct_matched_parameters = "***";
            string correct_matched_action = "new";
            bool correct_owner_only = false;

            var error = ProcessCheckSetBiblioInfoAccess(
                normal_rights,
                access_string,
                strAction,
                out string matched_parameters,
                out string matched_action,
                out string matched_operation,
                out bool owner_only);
            Assert.IsNull(error);
            Assert.AreEqual("setbiblioinfo", matched_operation);
            Assert.AreEqual(correct_matched_parameters, matched_parameters);
            Assert.AreEqual(correct_matched_action, matched_action);
            Assert.AreEqual(correct_owner_only, owner_only);
        }


        string ProcessCheckSetBiblioInfoAccess(
            string normal_rights,
            string access_string,
            string strAction,
            out string matched_parameters,
            out string matched_action,
            out string matched_operation,
            out bool owner_only,
            string style = "")
        {
            LibraryApplication app = new LibraryApplication();
            var orderWork = StringUtil.IsInList("orderWork", style);
            app.ItemDbs = new List<ItemDbCfg>
            {
                new ItemDbCfg
                {
                    DbName = "中文图书实体",
                    BiblioDbName = "中文图书",
                    Role = orderWork ? "orderWork" : "",
                }
            };
            SessionInfo sessioninfo = new SessionInfo(app);
            sessioninfo.Account = new Account { UserID = "test",
            Rights = normal_rights,
            Access = access_string};
            return app.CheckSetBiblioInfoAccess(sessioninfo,
                "biblio",
                "中文图书",
                strAction,
                false,
                out matched_parameters,
                out matched_action,
                out matched_operation,
                out owner_only);
        }
    }
}
