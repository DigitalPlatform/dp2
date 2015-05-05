using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace DigitalPlatform.LibraryServer
{
    public class Person
    {
        public string ID = "";
        public string LibraryCode = "";

        public DateTime CreateTime = DateTime.Now;
        public DateTime LastUsedTime = DateTime.Now;

        public bool StatisWrited = false;   // 统计信息是否已经写入过了
    }
    /// <summary>
    /// 跟踪读者入馆状态的类
    /// </summary>
    public class Garden : Hashtable
    {
        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        int _nMaxCount = 10000;    // hashtable 中允许的最多对象数目

        // 如果对象存在，则设置最新活动时间；如果对象不存在，则加入一个新对象
        public Person Activate(string strID,
            string strLibraryCode)
        {
            Person person = null;

            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                person = (Person)this[strID];

            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (person == null)
            {
                if (this.Count > _nMaxCount)
                {
                    return null;
                    // throw new ApplicationException("Person 数量超过 " + _nMaxCount.ToString());
                }

                person = new Person();
                person.ID = strID;
                person.LibraryCode = strLibraryCode;
                person.CreateTime = DateTime.Now;
                person.LastUsedTime = DateTime.Now;

                // 修改瞬间需要写锁定
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
                try
                {
                    this[strID] = person;
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }

                return person;
            }

            {
                person.LastUsedTime = DateTime.Now;
                if (person.ID != strID)
                {
                    Debug.Assert(false, "");
                    person.ID = strID;
                }
                return person;
            }
        }

        public bool IsFull
        {
            get
            {
                if (this.Count >= _nMaxCount)
                    return true;

                return false;
            }
        }

        // 清除长期不活动的 Person 对象，顺便写入统计信息
        // 尽早写入统计信息，可以减少意外 Down 掉的丢失写入的损害
        public void CleanPersons(TimeSpan delta, 
            Statis statis)
        {
            // List<string> remove_keys = new List<string>();

            List<Person> remove_persons = new List<Person>();   // 需要清除的对象
            List<Person> flush_persons = new List<Person>();    // 需要写入统计数据的对象

            // 读锁定并不阻碍一般性访问
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                foreach (string key in this.Keys)
                {
                    Person person = (Person)this[key];

                    if (person == null)
                        continue;

                    if ((DateTime.Now - person.LastUsedTime) >= delta)
                    {
                        // remove_keys.Add(key);   // 这里不能删除，因为 foreach 还要用枚举器
                        remove_persons.Add(person);
                    }
                    else if (person.StatisWrited == false)
                    {
                        flush_persons.Add(person);
                    }
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

#if NO
            if (remove_keys.Count == 0)
                return;
#endif
            // 移走过期的对象
            if (remove_persons.Count > 0)
            {

                // 因为要删除某些元素，所以用写锁定
                List<Person> delete_persons = new List<Person>();
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
                try
                {
#if NO
                    foreach (string key in remove_keys)
                    {
                        Person person = (Person)this[key];
                        if (person == null)
                            continue;   // sessionid 没有找到对应的 Session 对象

                        // 和 id 的 hashtable 脱离关系
                        this.Remove(key);

                        delete_persons.Add(person);
                    }
#endif
                    foreach (Person person in remove_persons)
                    {
                        this.Remove(person.ID);
                    }
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }
            }

            // 写入统计信息
            if (statis != null)
            {
                foreach (Person person in remove_persons)
                {
                    // TODO: 可以按照馆代码统计聚类后，减少写入 Statis 的次数
                    if (person.StatisWrited == false)
                    {
                        // 增量统计数
                        statis.IncreaseEntryValue(
                        person.LibraryCode,
                        "出纳",
                        "读者数",
                        1);
                        person.StatisWrited = true;
                        // person.Close();
                    }
                }

                foreach (Person person in flush_persons)
                {
                    if (person.StatisWrited == false)
                    {
                        // 增量统计数
                        statis.IncreaseEntryValue(
                        person.LibraryCode,
                        "出纳",
                        "读者数",
                        1);
                        person.StatisWrited = true;
                        // person.Close();
                    }
                }
            }
        }

    }
}
