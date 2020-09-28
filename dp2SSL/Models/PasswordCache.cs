using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL.Models
{
    /// <summary>
    /// 工作人员密码缓存机制。在一段时间内再次刷卡时可以省去输入密码
    /// </summary>
    public static class PasswordCache
    {
        // key --> PasswordItem
        // key 的构成：UID + userName
        static Hashtable _passwordTable = new Hashtable();

        // 获取一个密码
        // return:
        //      null    没有找到密码
        //      其它  返回密码
        public static string GetPassword(string uid, string userName)
        {
            lock (_passwordTable.SyncRoot)
            {
                string key = PasswordItem.BuildKey(uid, userName);
                PasswordItem item = (PasswordItem)_passwordTable[key];
                if (item == null)
                    return null;
                item.LastTime = DateTime.Now;
                return item.Password;
            }
        }

        // 保存一个密码
        public static void SavePassword(string uid,
            string userName,
            string password)
        {
            lock (_passwordTable.SyncRoot)
            {
                string key = PasswordItem.BuildKey(uid, userName);
                PasswordItem item = (PasswordItem)_passwordTable[key];
                if (item == null)
                {
                    item = new PasswordItem
                    {
                        UID = uid,
                        UserName = userName,
                        Password = password,
                        LastTime = DateTime.Now
                    };
                    _passwordTable[key] = item;
                }
                else
                {
                    item.Password = password;
                    item.LastTime = DateTime.Now;
                }
            }
        }

        // 从缓存中删掉一个密码事项
        public static void DeletePassword(string uid, string userName)
        {
            lock (_passwordTable.SyncRoot)
            {
                string key = PasswordItem.BuildKey(uid, userName);
                _passwordTable.Remove(key);
            }
        }

        // 清除闲置超过指定时间长度的那些对象
        public static void CleanIdlePassword(TimeSpan length)
        {
            lock (_passwordTable.SyncRoot)
            {
                List<string> delete_keys = new List<string>();
                foreach(string key in _passwordTable.Keys)
                {
                    var item = _passwordTable[key] as PasswordItem;
                    if (DateTime.Now - item.LastTime > length)
                        delete_keys.Add(key);
                }

                foreach(string key in delete_keys)
                {
                    _passwordTable.Remove(key);
                }
            }
        }
    }

    class PasswordItem
    {
        public string UID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        // 最近一次使用的时间
        public DateTime LastTime { get; set; }

        public static string BuildKey(string uid, string userName)
        {
            return uid + "_" + userName;
        }
    }
}
