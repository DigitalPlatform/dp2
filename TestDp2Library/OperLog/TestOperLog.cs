using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.IO;

namespace TestDp2Library
{
    [TestClass]

    public class TestOperLog
    {
        // 测试追加写入一个不存在的日志文件
        [TestMethod]
        public void Test_appendOperLog_01()
        {
            // string strBinDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);   //  Environment.CurrentDirectory;
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";
            string xml = "<root />";

            try
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.AppendOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    xml,
                    null,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);

                // 然后尝试读出刚写入的日志记录内容，并加以比对验证
                nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    "",
                    "",
                    null,
                    out long lHintNext,
                    out string output_xml,
                    null,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(xml, output_xml);
                Assert.AreEqual(tail, lHintNext);

                var count = OperLogUtility.GetOperLogCount(
                    strDirectory,
                    strFileName);
                Assert.AreEqual(1, count);
            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }

        // 测试追加到一个已经存在的日志文件的尾部
        [TestMethod]
        public void Test_appendOperLog_02()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName);

            string xml = "<root111 />";

            try
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.AppendOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    xml,
                    null,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);

                // 然后尝试读出刚写入的日志记录内容，并加以比对验证
                nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    1,  // 第二条日志记录
                    -1,
                    "",
                    "",
                    null,
                    out long lHintNext,
                    out string output_xml,
                    null,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(xml, output_xml);
                Assert.AreEqual(tail, lHintNext);

                var count = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
                Assert.AreEqual(2, count);
            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }

        // 测试在一个具有两条日志记录的文件中替换第一条日志记录
        [TestMethod]
        public void Test_replaceOperLog_01()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName, 2);

            try
            {
                // 然后尝试读出刚写入的第一条日志记录内容，并加以比对验证
                int nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    "",
                    "",
                    null,
                    out long lHintNext,
                    out string output_xml,
                    null,
                    out string strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual("<root />", output_xml);

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                nRet = OperLogUtility.ReplaceOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,  // 第一条
                    -1,
                    "",
                    "<root111 />",
                    null,
                    out long tail,
                    out strError);
                Assert.AreEqual(1, nRet);

                // 再次尝试读出刚写入的第一条日志记录内容，验证修改替换是否兑现了
                nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    "",
                    "",
                    null,
                    out long tail1,
                    out output_xml,
                    null,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual("<root111 />", output_xml);
                Assert.AreEqual(tail1, tail);

                // 再次尝试读出第二条日志记录内容，验证内容是否依旧
                nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    1,  // 第二条
                    -1,
                    "",
                    "",
                    null,
                    out long tail2,
                    out output_xml,
                    null,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual("<root />", output_xml);

                string strFilePath = Path.Combine(strDirectory, strFileName);
                var length = FileUtil.GetFileLength(strFilePath);
                Assert.IsTrue(length == tail2);

                var count = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
                Assert.AreEqual(2, count);

            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }

        // 测试在一个已有的日志文件中插入新的日志记录
        [TestMethod]
        public void Test_insertOperLog_01()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName, 2);

            try
            {
                // 统计日志记录条数
                var count1 = OperLogUtility.GetOperLogCount(
                    strDirectory,
                    strFileName);
                Assert.AreEqual(2, count1);

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.InsertOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,  // 第一条
                    -1,
                    "<root111 />",
                    null,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);

                // 再次统计日志记录条数
                var count2 = OperLogUtility.GetOperLogCount(
                    strDirectory,
                    strFileName);
                Assert.AreEqual(3, count2);
            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }

        }

        // 测试从一个日志文件中删除唯一的一条日志记录
        [TestMethod]
        public void Test_deleteOperLog_01()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName);

            try
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.DeleteOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(0, tail);

                string strFilePath = Path.Combine(strDirectory, strFileName);
                var length = FileUtil.GetFileLength(strFilePath);
                Assert.AreEqual(0, length);

                var count = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
                Assert.AreEqual(0, count);

            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }

        // 测试从一个具有两条日志记录的文件中删除第一条日志记录
        [TestMethod]
        public void Test_deleteOperLog_02()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName, 2);

            try
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.DeleteOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(0, tail);

                string strFilePath = Path.Combine(strDirectory, strFileName);
                var length = FileUtil.GetFileLength(strFilePath);
                Assert.IsTrue(length > 0);

                var count = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
                Assert.AreEqual(1, count);

            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }

        // 测试从一个具有两条日志记录的文件中删除第二条日志记录
        [TestMethod]
        public void Test_deleteOperLog_03()
        {
            string strDirectory = Environment.CurrentDirectory;
            string strFileName = "20000101.log";

            BuildLogFile(strDirectory, strFileName, 2);

            try
            {
                // 然后尝试读出刚写入的第一条日志记录内容，并加以比对验证
                int nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    "",
                    "",
                    null,
                    out long lHintNext,
                    out string output_xml,
                    null,
                    out string strError);
                Assert.AreEqual(1, nRet);

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                nRet = OperLogUtility.DeleteOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    1,  // 第二条
                    -1,
                    out long tail,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(lHintNext, tail);

                string strFilePath = Path.Combine(strDirectory, strFileName);
                var length = FileUtil.GetFileLength(strFilePath);
                Assert.IsTrue(length == lHintNext);

                var count = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
                Assert.AreEqual(1, count);

            }
            finally
            {
                string strFilePath = Path.Combine(strDirectory, strFileName);
                File.Delete(strFilePath);
            }
        }


        void BuildLogFile(string strDirectory,
            string strFileName,
            int count = 1)
        {
            string xml = "<root />";

            // 如果同名文件已经存在，先删除它
            string strFilePath = Path.Combine(strDirectory, strFileName);
            File.Delete(strFilePath);

            for (int i = 0; i < count; i++)
            {
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = OperLogUtility.AppendOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    xml,
                    null,
                    out long tail,
                    out string strError);
                Assert.AreEqual(1, nRet);

                /*
                // 然后尝试读出刚写入的日志记录内容，并加以比对验证
                nRet = OperLogUtility.GetOperLog(
                    null,
                    strDirectory,
                    strFileName,
                    0,
                    -1,
                    "",
                    "",
                    null,
                    out long lHintNext,
                    out string output_xml,
                    null,
                    out strError);
                Assert.AreEqual(1, nRet);
                Assert.AreEqual(xml, output_xml);
                Assert.AreEqual(tail, lHintNext);
                */
            }

            var ret = OperLogUtility.GetOperLogCount(
    strDirectory,
    strFileName);
            Assert.AreEqual(count, ret);
        }
    }
}
