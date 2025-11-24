using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static DigitalPlatform.LibraryServer.LibraryApplication;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 测试权限通用函数
    /// </summary>
    [TestClass]
    public class TestRightCommon
    {
        [TestMethod]
        public void test_moveUserRightsToAccess_01()
        {
            string rights = "setiteminfo";
            string access = "";

            string target_rights = "";
            string target_access = "*:setiteminfo(*)";

            LibraryApplication.MoveUserRightsToAccess(ref rights, ref access);
            Assert.AreEqual(target_rights, rights);
            Assert.AreEqual(target_access, access);
        }

        [TestMethod]
        public void test_moveUserRightsToAccess_02()
        {
            string rights = "setiteminfo";
            string access = "中文图书:getbiblioinfo";

            string target_rights = "";
            string target_access = "中文图书:getbiblioinfo;*:setiteminfo(*)";

            LibraryApplication.MoveUserRightsToAccess(ref rights, ref access);
            Assert.AreEqual(target_rights, rights);
            Assert.AreEqual(target_access, access);
        }

        // 存取定义中已经有了一个同类的 API，就不再追加了
        [TestMethod]
        public void test_moveUserRightsToAccess_03()
        {
            string rights = "setiteminfo";
            string access = "中文图书:setiteminfo";

            string target_rights = "";
            string target_access = "中文图书:setiteminfo";

            LibraryApplication.MoveUserRightsToAccess(ref rights, ref access);
            Assert.AreEqual(target_rights, rights);
            Assert.AreEqual(target_access, access);
        }

        // 普通权限字符串中不止一个权限值
        [TestMethod]
        public void test_moveUserRightsToAccess_04()
        {
            string rights = "getbiblioinfo,setiteminfo";
            string access = "";

            string target_rights = "getbiblioinfo";
            string target_access = "*:setiteminfo(*)";

            LibraryApplication.MoveUserRightsToAccess(ref rights, ref access);
            Assert.AreEqual(target_rights, rights);
            Assert.AreEqual(target_access, access);
        }

        // 普通权限字符串中不止一个权限值
        [TestMethod]
        public void test_moveUserRightsToAccess_05()
        {
            string rights = "setiteminfo,getbiblioinfo";
            string access = "";

            string target_rights = "getbiblioinfo";
            string target_access = "*:setiteminfo(*)";

            LibraryApplication.MoveUserRightsToAccess(ref rights, ref access);
            Assert.AreEqual(target_rights, rights);
            Assert.AreEqual(target_access, access);
        }

        [TestMethod]
        public void test_getDbOperRights_01()
        {
            string access = "中文图书:setorderinfo=new(newparam),change(changeparam)|getbiblioinfo=*";
            string dbname = "中文图书";
            string operation = "setorderinfo";
            string correct = "new(newparam),change(changeparam)";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_getDbOperRights_02()
        {
            string access = "中文图书:setorderinfo=*|getbiblioinfo=*";
            string dbname = "中文图书";
            string operation = "setorderinfo";
            string correct = "*";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 否定形态
        [TestMethod]
        public void test_getDbOperRights_03()
        {
            string access = "中文图书:setorderinfo=|getbiblioinfo=*";
            string dbname = "中文图书";
            string operation = "setorderinfo";
            string correct = "";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 没有找到
        [TestMethod]
        public void test_getDbOperRights_04()
        {
            string access = "中文图书:getbiblioinfo=*";
            string dbname = "中文图书";
            string operation = "setorderinfo";
            string correct = null;
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // "中文图书:getbiblioinfo" 等同于 "中文图书:getbiblioinfo=*"
        // 注意 "中文图书:getbiblioinfo=" 表示否定的意思，即 getbiblioinfo 操作不被允许
        [TestMethod]
        public void test_getDbOperRights_05()
        {
            string access = "中文图书:getbiblioinfo";
            string dbname = "中文图书";
            string operation = "getbiblioinfo";
            string correct = "*";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_getDbOperRights_10()
        {
            string access = "中文图书:getbiblioinfo=1;*:getbiblioinfo=2";
            string dbname = "中文图书";
            string operation = "getbiblioinfo";
            string correct = "1";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_getDbOperRights_11()
        {
            string access = "中文图书:getbiblioinfo=1;*:getbiblioinfo=2";
            string dbname = "英文图书";
            string operation = "getbiblioinfo";
            string correct = "2";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 数据库名字为空
        [TestMethod]
        public void test_getDbOperRights_12()
        {
            string access = "中文图书:getbiblioinfo=1;*:getbiblioinfo=2";
            string dbname = "";
            string operation = "getbiblioinfo";
            string correct = "1";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 2025/10/15
        // 验证 strOperation 参数中使用多个权限值的清醒
        [TestMethod]
        public void test_getDbOperRights_21()
        {
            string access = "中文图书:getbiblioinfo=1|order=2";
            string dbname = "中文图书";
            string operation = "getbiblioinfo,order";
            string correct = "1";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 从左到右扫描，先命中 getbiblioinfo=1
        [TestMethod]
        public void test_getDbOperRights_22()
        {
            string access = "中文图书:getbiblioinfo=1|order=2";
            string dbname = "中文图书";
            string operation = "order,getbiblioinfo";
            string correct = "1";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 否定
        [TestMethod]
        public void test_getDbOperRights_23()
        {
            string access = "中文图书:getbiblioinfo=|order=2";
            string dbname = "中文图书";
            string operation = "order,getbiblioinfo";
            string correct = "2";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 否定。两个都是否定
        [TestMethod]
        public void test_getDbOperRights_24()
        {
            string access = "中文图书:getbiblioinfo=|order=";
            string dbname = "中文图书";
            string operation = "order,getbiblioinfo";
            string correct = "";    // 找到了，但是为否定形态
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        // 没有找到
        [TestMethod]
        public void test_getDbOperRights_25()
        {
            string access = "中文图书:getbiblioinfo=|order=";
            string dbname = "中文图书";
            string operation = "borrow,return";
            string correct = null;
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operation);
            Assert.AreEqual(correct, result);
        }

        [TestMethod]
        public void test_getDbOperRightsEx_01()
        {
            string access = "中文图书:getbiblioinfo=1|order=2";
            string dbname = "中文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "order", Rights = "2" , DbNames = "中文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        [TestMethod]
        public void test_getDbOperRightsEx_02()
        {
            string access = "中文图书:getbiblioinfo=1|order=";
            string dbname = "中文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "order", Rights = "" , DbNames = "中文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        [TestMethod]
        public void test_getDbOperRightsEx_03()
        {
            string access = "中文图书:getbiblioinfo=1|order";
            string dbname = "中文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "order", Rights = "*" , DbNames = "中文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        [TestMethod]
        public void test_getDbOperRightsEx_04()
        {
            string access = "中文图书:getbiblioinfo=1|order=2;西文图书:getbiblioinfo=3|order=4";
            string dbname = "西文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "3", DbNames = "西文图书" },
                new OperRights{Operation = "order", Rights = "4" , DbNames = "西文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        [TestMethod]
        public void test_getDbOperRightsEx_05()
        {
            string access = "*:getbiblioinfo=1|order=2;西文图书:getbiblioinfo=3|order=4";
            string dbname = "西文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "西文图书" },
                new OperRights{Operation = "order", Rights = "2" , DbNames = "西文图书"},
                new OperRights{Operation = "getbiblioinfo", Rights = "3", DbNames = "西文图书" },
                new OperRights{Operation = "order", Rights = "4" , DbNames = "西文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }


        [TestMethod]
        public void test_getDbOperRightsEx_06()
        {
            string access = "*:getbiblioinfo=1|order=2;西文图书:getbiblioinfo=3|order=4";
            string dbname = "中文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "order", Rights = "2" , DbNames = "中文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 多个数据库名
        [TestMethod]
        public void test_getDbOperRightsEx_07()
        {
            string access = "中文图书:getbiblioinfo=1|order=2;西文图书:getbiblioinfo=3|order=4";
            string dbname = "中文图书,西文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "order", Rights = "2" , DbNames = "中文图书"},
                new OperRights{Operation = "getbiblioinfo", Rights = "3", DbNames = "西文图书" },
                new OperRights{Operation = "order", Rights = "4" , DbNames = "西文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 条件中多个数据库名，存取定义中数据库名为 *
        [TestMethod]
        public void test_getDbOperRightsEx_08()
        {
            string access = "*:getbiblioinfo=1|order=2;西文图书:getbiblioinfo=3|order=4";
            string dbname = "中文图书,西文图书";
            string operations = "getbiblioinfo,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书,西文图书" },
                new OperRights{Operation = "order", Rights = "2" , DbNames = "中文图书,西文图书"},
                new OperRights{Operation = "getbiblioinfo", Rights = "3", DbNames = "西文图书" },
                new OperRights{Operation = "order", Rights = "4" , DbNames = "西文图书"},
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 数据库名字为空
        [TestMethod]
        public void test_getDbOperRightsEx_09()
        {
            string access = "中文图书:getbiblioinfo=1;*:getbiblioinfo=2";
            string dbname = "";
            string operations = "getbiblioinfo";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "getbiblioinfo", Rights = "2", DbNames = "*" },
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 数据库名字为空
        [TestMethod]
        public void test_getDbOperRightsEx_10()
        {
            string access = "中文图书:getbiblioinfo=1;*:getbiblioinfo=2";
            string dbname = "*";
            string operations = "getbiblioinfo";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "getbiblioinfo", Rights = "1", DbNames = "中文图书" },
                new OperRights{Operation = "getbiblioinfo", Rights = "2", DbNames = "*" },
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 否定的在肯定的前面
        [TestMethod]
        public void test_getDbOperRightsEx_11()
        {
            string access = "中文图书:setiteminfo=new,change|getiteminfo|getbibliosummary1|managedatabase=|getsystemparameter=;*:managedatabase";
            string dbname = "中文图书";
            string operations = "managedatabase,order";
            List<OperRights> correct = new List<OperRights> {
                new OperRights{Operation = "managedatabase", Rights = "", DbNames = "中文图书" },
                new OperRights{Operation = "managedatabase", Rights = "*", DbNames = "中文图书" },
            };
            var result = LibraryApplication.GetDbOperRightsEx(access,
            dbname,
            operations);
            var error = CheckEqual(correct, result);
            if (error != null)
                throw new InternalTestFailureException(error);
        }

        // 否定的在肯定的前面
        [TestMethod]
        public void test_getDbOperRightsEx_12()
        {
            string access = "中文图书:setiteminfo=new,change|getiteminfo|getbibliosummary1|managedatabase=|getsystemparameter=;*:managedatabase";
            string dbname = "中文图书";
            string operations = "managedatabase,order";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operations);
            Assert.AreEqual("", result);    // "" 表示否定
        }

        [TestMethod]
        public void test_getDbOperRightsEx_13()
        {
            // 任意数据库名字，匹配上了最后一截 *:managedatabase。相当于 *:managedatabase=*
            string access = "中文图书:setiteminfo=new,change|getiteminfo|getbibliosummary1|managedatabase=|getsystemparameter=;*:managedatabase";
            string dbname = "";
            string operations = "managedatabase,order";
            var result = LibraryApplication.GetDbOperRights(access,
            dbname,
            operations);
            Console.WriteLine($"result='{result}'");
            Assert.IsTrue(result == "*");    // "" 表示否定
        }

        static string CheckEqual(List<OperRights> list1, List<OperRights> list2)
        {
            if (list1.Count != list2.Count)
                return $"list1.Count={list1.Count} 和 list2.Count={list2.Count} 不相等";
            for (int i = 0; i < list1.Count; i++)
            {
                var s1 = list1[i].ToString();
                var s2 = list2[i].ToString();
                if (s1 != s2)
                    return $"index 位置 {i} 的两个元素不相等。\r\n'{s1}' != \r\n'{s2}'";
            }

            return null;
        }

    }
}
