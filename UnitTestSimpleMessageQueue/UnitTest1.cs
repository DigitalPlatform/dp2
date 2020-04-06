using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.SimpleMessageQueue;

namespace UnitTestSimpleMessageQueue
{
    [TestClass]
    public class UnitTest1
    {
        // 最简单的冒烟测试
        [TestMethod]
        public void TestMethod1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            MessageQueue queue = new MessageQueue(fileName);

            queue.Push(new List<string> { "1" });

            var message = queue.Pull();
            Assert.AreEqual(message.GetString(), "1");

            message = queue.Pull();
            Assert.AreEqual(message, null);
        }

        // 循环 Push 和 Pull
        [TestMethod]
        public void TestMethod2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            MessageQueue queue = new MessageQueue(fileName);

            for (int i = 0; i < 100; i++)
            {
                queue.Push(new List<string> { $"{i + 1}" });
            }

            for (int i = 0; i < 100; i++)
            {
                var message = queue.Pull();
                Assert.AreEqual(message.GetString(), $"{i + 1}");
            }

            {
                var message = queue.Pull();
                Assert.AreEqual(message, null);
            }
        }

        // 大尺寸的消息测试
        [TestMethod]
        public void TestMethod3()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            MessageQueue queue = new MessageQueue(fileName);
            queue.ChunkSize = 10;

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < queue.ChunkSize * 20; i++)
            {
                bytes.Add((byte)i);
            }
            queue.Push(new List<byte []> { bytes.ToArray() });

            var message = queue.Pull();

            Assert.AreEqual(message.Content.Length, bytes.Count);
            for (int i = 0; i < queue.ChunkSize * 20; i++)
            {
                Assert.AreEqual(bytes[i], message.Content[i]);
            }

            message = queue.Pull();
            Assert.AreEqual(message, null);
        }
    }
}
