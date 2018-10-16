// #define DEBUG_LOCK
// rms

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.rms;
using DigitalPlatform.Text;
using DigitalPlatform.Text.SectionPropertyString;
using DigitalPlatform.Xml;
using DigitalPlatform.ResultSet;
using DigitalPlatform.IO;

namespace DigitalPlatform.rms
{
    // 用户集合
    public class UserCollection : List<User>
    {
        private MyReaderWriterLock m_lock = new MyReaderWriterLock();
        private static int m_nTimeOut = 5000;

        KernelApplication KernelApplication = null;
        // internal DatabaseCollection m_dbs = null; // 数据库集合

        private int GreenMax = 10;   // 绿色尺寸 在这个尺寸以下, 不清除UseCount为0的对象; 在以上才启动清除
        private int YellowMax = 50;  // 黄色尺寸 凡超过这个尺寸, 就开始清理不常用的对象
        private int RedMax = 100;   // 红色尺寸 绝对不允许超过这个尺寸。如果已经达到这个尺寸，必先清除掉出一个空位后，才能允许新对象进入集合

        public TimeSpan MaxLastUse = new TimeSpan(0, 30, 0);

        public DatabaseCollection Dbs
        {
            get
            {
                return this.KernelApplication.Dbs;
            }
        }

        // 初始化用户集合对象
        // parameters:
        //      userDbs     帐户库集合
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：安全的
        public int Initial(
            KernelApplication app,
            //DatabaseCollection dbs,
            out string strError)
        {
            strError = "";

            // this.m_dbs = dbs;
            this.KernelApplication = app;

            //*********对帐户集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("Initial()，对用户集合加写锁。");
#endif
            try
            {
                // 清空成员
                this.Clear();
            }
            finally
            {
                m_lock.ReleaseWriterLock();  //解写锁
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("Initial()，对用户集合解写锁。");
#endif
            }

            return 0;
        }

        public void Close()
        {
            /*
            eventClose.Set();	// 令工作线程退出

            // 等待工作线程真正退出
            // 因为可能正在回写数据库
            eventFinished.WaitOne(5000, false);
             */
        }

        // 只从用户集合中查找用户对象
        // parameters:
        //      strName     用户名
        //      user        out参数，返回用户对象
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   未找到
        //      1   找到
        // 线：安全
        private int GetUserFromCollection(string strName,
            out User user,
            out string strError)
        {
            user = null;
            strError = "";

            //*********对帐户集合加读锁*****************
            this.m_lock.AcquireReaderLock(m_nTimeOut); //加读锁
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("GetUserFromCollection()，对帐户集合加读锁。");
#endif
            try
            {
                foreach (User oneUser in this)
                {
                    if (oneUser.Name == strName)
                    {
                        user = oneUser;
                        return 1;
                    }
                }

                return 0;
            }
            finally
            {
                //*****对帐户集合解读锁*******
                this.m_lock.ReleaseReaderLock();
#if DEBUG_LOCK
                this.m_dbs.WriteDebugInfo("GetUserFromCollection()，对帐户集合解读锁。");
#endif
            }
        }

        // 获得用户对象
        // 先从用户集合中找，没有再从用户库集合中搜索
        // 注意,如果本函数找User对象成功, 那已经为引用计数加一,调用者要留意在以后不用User对象时, 不要忘记将对象的引用计数减一
        // parameters:
        //      bIncreament 是否顺便为计数器加一
        //      strName     用户名
        //      user        out参数，返回用户对象
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   未找到
        //      1   找到
        // 线：安全
        public int GetUserSafety(
            bool bIncreament,
            string strName,
            CancellationToken token,
            out User user,
            out string strError)
        {
            user = null;
            strError = "";

            token.ThrowIfCancellationRequested();

            // 只从用户集合中查找用户对象
            // parameters:
            //      strName     用户名
            //      user        out参数，返回用户对象
            //      strError    out参数，返回出错信息
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到
            // 线：安全
            int nRet = this.GetUserFromCollection(strName,
                out user,
                out strError);
            if (nRet == -1)
                return -1;

            token.ThrowIfCancellationRequested();

            if (nRet == 1)
            {
                if (bIncreament == true)
                    user.PlusOneUse();

                user.Activate();    // 更新最近使用时间

                return 1;
            }

            token.ThrowIfCancellationRequested();

            // return:
            //		-1	出错
            //		0	未找到帐户
            //		1	找到了
            nRet = this.Dbs.ShearchUserSafety(strName,
                out user,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
                return 0;

            Debug.Assert(user != null, "此时user不可能为null");

            // 如果集合根本不让进入
            if (this.RedMax <= 0)
            {
                user.container = this;  // 但是Container指针还是有用的
                return 1;
            }

            // 从数据库中找, 并加入集合

            //*********对帐户集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
            this.m_dbs.WriteDebugInfo("GetUser()，对帐户集合加写锁。");
#endif
            try
            {
                token.ThrowIfCancellationRequested();

                // 达到红色尺寸
                if (this.Count >= this.RedMax)
                {
                    // 必须先清除出空位，才能让新对象进入
                    int delta = this.Count - this.RedMax + 1;

                    this.RemoveUserObjects(token, delta);
                }

                token.ThrowIfCancellationRequested();

                this.Add(user);
                user.PlusOneUse();  // 因为这是对象重返集合，所以无论如何要增加计数。但是被挤走前的计数值已经无从查考，所以象征性给1
                user.container = this;

                user.Activate();    // 更新最近使用时间

                this.KernelApplication.ActivateWorker();  // 通知工作线程，需要整理尺寸了
                return 1;
            }
            finally
            {
                m_lock.ReleaseWriterLock();  //解写锁
#if DEBUG_LOCK
                this.m_dbs.WriteDebugInfo("GetUser()，对帐户集合解写锁。");
#endif
            }
        }

        // 登录
        // parameters:
        //      strUserName 用户名
        //      strPassword 密码
        //      user        out参数，返回用户对象
        //      strError    out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	用户名不存在，或密码不正确
        //      1   成功
        // 线：安全
        public int Login(string strUserName,
            string strPassword,
            out User user,
            out string strError)
        {
            user = null;
            strError = "";

            // 为引用加一
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到
            // 线：安全
            int nRet = this.GetUserSafety(
                true,
                strUserName,
                this.KernelApplication._app_down.Token,
                out user,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                Debug.Assert(user != null, "此时user不可能为null。");
                string strSHA1Password = Cryptography.GetSHA1(strPassword);
                if (user.SHA1Password == strSHA1Password)
                {
                    return 1;
                }
            }

            return 0;
        }

        // 登出
        // return:
        //      -1  出错
        //      0   未找到
        //      1   找到，并从集合中清除
        public int Logout(
            string strName,
            out string strError)
        {
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到，并从集合中清除
            return ReleaseUser(
                strName,
                out strError);
        }

        // 释放一次内存User对象的引用计数
        // 线：安全
        // return:
        //      -1  出错
        //      0   未找到
        //      1   找到，并从集合中清除
        public int ReleaseUser(
            string strName,
            out string strError)
        {
            User user = null;
            strError = "";

            // 只从用户集合中查找用户对象
            // parameters:
            //      strName     用户名
            //      user        out参数，返回用户对象
            //      strError    out参数，返回出错信息
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到
            // 线：安全
            int nRet = this.GetUserFromCollection(strName,
                out user,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                // 集合中不存在
                return 0;
            }

            Debug.Assert(user != null, "此时user不可能为null");

            user.MinusOneUse();
            this.KernelApplication.ActivateWorker();

            return 1;

            /* 如果必要, 立即从集合中删除?
            //*********对帐户集合加写锁****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
            this.m_dbs.WriteDebugInfo("GetUser()，对帐户集合加写锁。");
#endif
            try
            {
             * if ???
                this.Remove(user);
                return 1;
            }
            finally
            {
                m_lock.ReleaseWriterLock();  //解写锁
#if DEBUG_LOCK
                this.m_dbs.WriteDebugInfo("GetUser()，对帐户集合解写锁。");
#endif
            }
             */
        }

        // 更新内存中的帐户对象。
        // 线：安全
        // parameters:
        //      strRecPath  帐户记录路径
        // return:
        //      0   not found
        //      1   found and removed
        public int RefreshUserSafety(
            string strRecPath)
        {
            //***************对帐户集合加写锁*****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("RefreshUser()，对帐户集合加写锁。");
#endif
            try
            {
                foreach (User user in this)
                {
                    if (user.RecPath == strRecPath)
                    {
                        this.Remove(user);
                        return 0;
                    }
                }
                return 0;
            }
            finally
            {
                //***********对帐户集合解写锁*******************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("RefreshUser()，对帐户集合解写锁。");
#endif
            }
        }



        /*
        // 提取数据库中记录，更新User对象
        public int RefreshUser(
            string strRecordPath,
            User user,
            out string strError)
        {
            strError = "";

            // 创建一个DpPsth实例
            DbPath path = new DbPath(strRecordPath);

            // 找到指定帐户数据库,因为数据库名有可能不是id，所以用DatabaseCollection.GetDatabase()
            Database db = this.m_dbs.GetDatabase(path.Name); //this.GetUserDatabaseByID(path.Name);
            if (db == null)
            {
                strError = "未找到名为'" + path.Name + "'帐户库。";
                return -1;
            }

            // 从帐户库中找到记录
            string strXml;
            int nRet = db.GetXmlDataSafety(path.ID,
                out strXml,
                out strError);
            if (nRet <= -1)
                return -1;

            //加载到dom
            XmlDocument dom = new XmlDocument();
            //dom.PreserveWhitespace = true; //设PreserveWhitespace为true
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "加载路径为'" + strRecordPath + "'的帐户记录到dom时出错,原因:" + ex.Message;
                return -1;
            }

            int nOldUseCount = user.UseCount;

            nRet = user.Initial(strRecordPath,
                dom,
                db,
                out strError);
            if (nRet == -1)
                return -1;

            user.m_nUseCount = nOldUseCount;

            return 1;
        }
         */

        // 如果必要，保存内存中的帐户对象到数据库
        // 线：安全
        // return:
        //		-1  出错
        //      -4  记录不存在
        //		0   成功
        public int SaveUserIfNeed(string strRecPath,
            out string strError)
        {
            strError = "";

            //***************对帐户集合加写锁*****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("SaveUserSafety()，对帐户集合加写锁。");
#endif
            try
            {
                foreach (User user in this)
                {
                    if (user.RecPath == strRecPath)
                    {
                        // return:
                        //		-1  出错
                        //      -4  记录不存在
                        //		0   成功
                        int nRet = user.SaveChanges(out strError);
                        if (nRet <= -1)
                            return nRet;
                    }
                }
                return 0;
            }
            finally
            {
                //***********对帐户集合解写锁*******************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("SaveUserSafety()，对帐户集合解写锁。");
#endif
            }
        }

        // 如果内存用户数超出范围，则移出指定的对象。
        // parameters:
        //      user    用 户对象
        //      strError    out参数，返回出错信息
        // 线：安全
        // 异常：可能会抛出异常
        public void Shrink(CancellationToken token)
        {
            if (this.Count < this.GreenMax)
                return; // 小于绿色尺寸，根本不必进入

            int nCount = 0;
            //***************对帐户集合加写锁*****************
            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合加写锁。");
#endif
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    User user = this[i];

                    // usecount==0清除
                    int nRet = Interlocked.Increment(ref user.m_nUseCount);
                    Interlocked.Decrement(ref user.m_nUseCount);
                    if (nRet <= 1)
                    {
                        this.RemoveAt(i);
                        continue;
                    }

                    // 看看最近使用时间是否超过极限
                    TimeSpan delta = DateTime.Now - user.m_timeLastUse;
                    if (delta > this.MaxLastUse)
                    {
                        this.RemoveAt(i);
                        continue;
                    }
                }

                nCount = this.Count;
            }
            finally
            {
                //***********对帐户集合解写锁*******************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合解写锁。");
#endif
            }

            // 控制最大尺寸
            if (nCount > this.YellowMax)
            {
                // 挑选出delta个清除
                int delta = nCount - this.YellowMax;

                this.RemoveUserObjects(token, delta);

                /*
                m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合加写锁。");
#endif
                try
                {
                    for (int i = 0; i < delta; i++)
                    {
                        this.Remove(users[i]);
                    }
                }
                finally
                {
                    //***********对帐户集合解写锁*******************
                    m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合解写锁。");
#endif
                }
                 */
            }
        }

        // 挑出若干个最不常用的User对象从集合中移除
        // 线程安全
        void RemoveUserObjects(CancellationToken token,
            int nRemoveCount)
        {
            List<User> users = this.SortRecentUse(token);

            m_lock.AcquireWriterLock(m_nTimeOut);
#if DEBUG_LOCK
			this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合加写锁。");
#endif
            try
            {
                for (int i = 0; i < nRemoveCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    this.Remove(users[i]);
                }
            }
            finally
            {
                //***********对帐户集合解写锁*******************
                m_lock.ReleaseWriterLock();
#if DEBUG_LOCK
				this.m_dbs.WriteDebugInfo("Shrink()，对帐户集合解写锁。");
#endif
            }
        }

        // 最后修改时间比较器
        public class UserComparer : IComparer<User>
        {
            DateTime m_now = DateTime.Now;

            // 老的在前面
            int IComparer<User>.Compare(User x, User y)
            {
                // 根据最近使用时间
                TimeSpan delta1 = m_now - x.m_timeLastUse;
                TimeSpan delta2 = m_now - y.m_timeLastUse;

                long lRet = delta1.Ticks - delta2.Ticks;
                if (lRet < 0)
                    return -1*-1;
                if (lRet == 0)
                    return 0;
                return -1*1;
            }
        }

        // 为淘汰算法进行排序
        public List<User> SortRecentUse(CancellationToken token)
        {
            // 复制出对象
            List<User> aItem = new List<User>();

            // 加读锁
            this.m_lock.AcquireReaderLock(m_nTimeOut);
            try
            {
                aItem.AddRange(this);
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }

            token.ThrowIfCancellationRequested();

            UserComparer comp = new UserComparer();

            aItem.Sort(comp);

            return aItem;
        }

        /*
        // 从用户集合中清除一个用户
        // parameters:
        //      user    用户对象
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        // 线：不安全安全
        public int RemoveUser(User user,
            out string strError)
        {
            strError = "";

            Debug.Assert(user != null, "RemoveUser()调用错误，user参数值不能为null。");

            int nIndex = this.IndexOf(user);
            if (nIndex == -1)
            {
                strError = "RemoveUser()，user竟然不是集合中的成员，异常。";
                return -1;
            }

            this.RemoveAt(nIndex);

            return 0;
        }
         */

        // 系统管理员修改用户密码
        // parameters:
        //      user        当前帐户
        //      strChangedUserName  被修改用户名
        //      strNewPassword  新密码
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      -4  记录不存在
        //      -6  权限不够
        //		0   成功
        public int ChangePassword(User user,
            string strChangedUserName,
            string strNewPassword,
            out string strError)
        {
            strError = "";

            User changedUser = null;

            // return:
            //		-1	出错
            //		0	未找到帐户
            //		1	找到了
            // 线：安全
            int nRet = this.GetUserSafety(
                false,
                strChangedUserName,
                this.KernelApplication._app_down.Token,
                out changedUser,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = "没有找到名称为'" + strChangedUserName + "'的用户";
                return -1;
            }

            Debug.Assert(changedUser != null, "此时userChanged对象不可能为null,请检查服务器的ChangePassword()函数。");

            DbPath path = new DbPath(changedUser.RecPath);

            Database db = this.Dbs.GetDatabase(path.Name);
            if (db == null)
            {
                strError = "未找到帐户'" + strChangedUserName + "'从属的数据库，异常。";
                return -1;
            }
            // ???????认不认库的其它语言库名
            string strDbName = db.GetCaption("zh-CN");

            string strExistRights = "";
            bool bHasRight = user.HasRights(strDbName,
                ResType.Database,
                "changepassword",
                out strExistRights);
            if (bHasRight == false)
            {
                strError = "您的帐户名为'" + user.Name + "'，对帐户名为'" + strChangedUserName + "'所从属的数据库'" + strDbName + "'没有'修改记录密码(changepassword)'的权限，目前的权限值为'" + strExistRights + "'。";
                return -6;
            }

            // return:
            //      -1  出错
            //      -4  记录不存在
            //		0   成功
            return changedUser.ChangePassword(strNewPassword,
                out strError);
        }
    }

}
