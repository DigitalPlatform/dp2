using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;

namespace TestDp2Library
{
    // 测试 dp2library 中和密码有关的函数
    [TestClass]
    public class TestPassword
    {
        [TestMethod]
        public void Test_IsSequence_1_1()
        {
            string text = "123456";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(true, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_1_2()
        {
            string text = "654321";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(true, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_1_3()
        {
            string text = "222222";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_IsSequence_1_4()
        {
            string text = "123210";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_1_5()
        {
            string text = "";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_1_6()
        {
            string text = "1";
            // parameters:
            //      direction   -1 逐步减小; 0 相同; 1 逐步增加
            var ret = LibraryApplication.IsSequence(text, 1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, -1);
            Assert.AreEqual(false, ret);

            ret = LibraryApplication.IsSequence(text, 0);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_1()
        {
            string text = "123456";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_2()
        {
            string text = "654321";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_3()
        {
            string text = "222222";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_4()
        {
            string text = "123210";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_5()
        {
            string text = "";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void Test_IsSequence_2_6()
        {
            string text = "1";
            var ret = LibraryApplication.IsSequence(text);
            Assert.AreEqual(false, ret);
        }

        // 风格 1
        /*
1. 8个字符，且不能是顺序、逆序或相同
2. 数字加字母组合
3. 密码和用户名不可以一样
4. 临时密码不可以当做正式密码使用
5. 新旧密码不能一样
         * */

        [TestMethod]
        public void Test_validate_1_1()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "12345";  // 长度不足

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(0, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }

        [TestMethod]
        public void Test_validate_1_2()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "123456789";  // 顺序序列

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(0, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }


        [TestMethod]
        public void Test_validate_1_3()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "123456787";  // 只有数字，没有字母

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(0, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }


        [TestMethod]
        public void Test_validate_1_4()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "123456787a";  // 合法

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(1, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }


        [TestMethod]
        public void Test_validate_1_5()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "test";  // 和旧密码一样

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(0, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }

        [TestMethod]
        public void Test_validate_1_6()
        {
            string xml = @"<account name='supervisor'>
    <password>qUqP5cyxm6YcTAhz05Hph5gvu9M=</password>
</account>";
            string password = "supervisor";  // 和用户名一样

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 验证密码字符串的合法性
            // parameters:
            //      style   风格。style-1 为第一种密码风格
            // return:
            //      -1  出错
            //      0   不合法(原因在 strError 中返回)
            //      1   合法
            var ret = LibraryApplication.ValidatePassword(
                dom.DocumentElement,
                password,
                "style-1",
                out string strError);
            Assert.AreEqual(0, ret);
            Debug.WriteLine($"密码 '{password}' 验证结果: {strError}");
        }

    }

}
