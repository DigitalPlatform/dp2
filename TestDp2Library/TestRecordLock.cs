using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestDp2Library
{
    [TestClass]
    public class TestRecordLock
    {
        // 加写锁以后，并不解锁，然后直接 Dispose() locks
        [TestMethod]
        public void Test_recordLock_free_01()
        {
            int count = 10000;
            using (RecordLockCollection locks = new RecordLockCollection())
            {
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();
                    locks.LockForWrite(strID, 1000);
                }

                // Assert.AreEqual(true, locks.IsEmpty());
            }
        }

        // 加读锁以后，并不解锁，然后直接 Dispose() locks
        [TestMethod]
        public void Test_recordLock_free_02()
        {
            int count = 10000;
            using (RecordLockCollection locks = new RecordLockCollection())
            {
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();
                    locks.LockForRead(strID, 1000);
                }

                // Assert.AreEqual(true, locks.IsEmpty());
            }
        }

        // 加写锁然后马上解锁
        [TestMethod]
        public void Test_recordLock_01()
        {
            using (RecordLockCollection locks = new RecordLockCollection())
            {
                for (int i = 0; i < 1000; i++)
                {
                    string strID = i.ToString();
                    locks.LockForWrite(strID, 1000);
                }

                for (int i = 0; i < 1000; i++)
                {
                    string strID = i.ToString();
                    locks.UnlockForWrite(strID);
                }

                Assert.AreEqual(true, locks.IsEmpty());
            }
        }

        // 先加写锁。在解锁之前用另一线程尝试加读锁，应该是加锁失败
        [TestMethod]
        public void Test_recordLock_02()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.LockForWrite(strID, 1000);
            }

            Task.Run(() =>
            {

                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();
                    try
                    {
                        locks.LockForRead(strID, 1000);
                    }
                    catch (Exception ex)
                    {
                        Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                    }
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.UnlockForWrite(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // 先加写锁。在解锁之前用另一线程尝试加写锁，应该是加锁失败
        [TestMethod]
        public void Test_recordLock_03()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.LockForWrite(strID, 1000);
            }

            Task.Run(() =>
            {

                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();
                    try
                    {
                        locks.LockForWrite(strID, 1000);
                    }
                    catch (Exception ex)
                    {
                        Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                    }
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.UnlockForWrite(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // 先加读锁。在解锁之前用另一线程尝试加写锁，应该是加锁失败
        [TestMethod]
        public void Test_recordLock_04()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.LockForRead(strID, 1000);
            }

            Task.Run(() =>
            {

                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();
                    try
                    {
                        locks.LockForWrite(strID, 1000);
                    }
                    catch (Exception ex)
                    {
                        Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                    }
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.UnlockForRead(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // 先加读锁。在解锁之前用另一线程尝试加读锁，应该是加锁成功
        [TestMethod]
        public void Test_recordLock_05()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.LockForRead(strID, 1000);
            }

            Task.Run(() =>
            {
                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = i.ToString();

                    locks.LockForRead(strID, 1000);
                    locks.UnlockForRead(strID);
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = i.ToString();
                locks.UnlockForRead(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // ID 未发生碰撞的加锁。读锁 读锁
        [TestMethod]
        public void Test_recordLock_10()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.LockForRead(strID, 1000);
            }

            Task.Run(() =>
            {
                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = ((i * 2) + 1).ToString();

                    locks.LockForRead(strID, 1000);
                    locks.UnlockForRead(strID);
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.UnlockForRead(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // ID 未发生碰撞的加锁。读锁 写锁
        [TestMethod]
        public void Test_recordLock_11()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.LockForRead(strID, 1000);
            }

            Task.Run(() =>
            {
                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = ((i * 2) + 1).ToString();

                    locks.LockForWrite(strID, 1000);
                    locks.UnlockForWrite(strID);
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.UnlockForRead(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // ID 未发生碰撞的加锁。写锁 写锁
        [TestMethod]
        public void Test_recordLock_12()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.LockForWrite(strID, 1000);
            }

            Task.Run(() =>
            {
                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = ((i * 2) + 1).ToString();

                    locks.LockForWrite(strID, 1000);
                    locks.UnlockForWrite(strID);
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.UnlockForWrite(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // ID 未发生碰撞的加锁。写锁 读锁
        [TestMethod]
        public void Test_recordLock_13()
        {
            int count = 10;
            RecordLockCollection locks = new RecordLockCollection();
            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.LockForWrite(strID, 1000);
            }

            Task.Run(() =>
            {
                // 尝试加读锁
                for (int i = 0; i < count; i++)
                {
                    string strID = ((i * 2) + 1).ToString();

                    locks.LockForRead(strID, 1000);
                    locks.UnlockForRead(strID);
                }
            }).Wait();

            for (int i = 0; i < count; i++)
            {
                string strID = (i * 2).ToString();
                locks.UnlockForWrite(strID);
            }

            Assert.AreEqual(true, locks.IsEmpty());
        }

        // 锁定期间，另一线程尝试加锁失败。解锁以后，另一线程加锁成功
        [TestMethod]
        public void Test_recordLock_20()
        {
            using (RecordLockCollection locks = new RecordLockCollection())
            {
                string strID = "1";
                locks.LockForRead(strID, 1000);

                Task.Run(() =>
                {
                    int count = 10;
                    // string strAnotherID = "2";
                    for (int i = 0; i < count; i++)
                    {
                        try
                        {
                            locks.LockForWrite(strID, 100);
                            throw new Exception("不应该走到这里");
                        }
                        catch (Exception ex)
                        {
                            Assert.AreEqual(typeof(ApplicationException), ex.GetType());
                        }
                    }
                }).Wait();

                locks.UnlockForRead(strID);

                Assert.AreEqual(true, locks.IsEmpty());

                locks.LockForRead(strID, 1000);
            }
        }

        /*
        // 调用私有函数 GetLock()
        // https://stackoverflow.com/questions/9122708/unit-testing-private-methods-in-c-sharp
        // 示范如何测试私有函数。返回类型也是私有的
        static RecordLock CallGetLock(
            RecordLockCollection locks,
            string id,
            bool auto_create)
        {
            PrivateObject obj = new PrivateObject(locks);

            object result = obj.Invoke("GetLock",
            id,
            auto_create);

            PrivateObject obj1 = new PrivateObject(result);
            RecordLock lock_object = (RecordLock)obj1.GetFieldOrProperty("Value");
            return lock_object;
        }
        */
    }
}
