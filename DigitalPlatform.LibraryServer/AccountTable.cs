using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 帐户对象集合。可以用来防止一个用户多次登录占用多个session，也可以用来管理每个帐户的token
    /// </summary>
    public class AccountTable : List<Account>
    {
        public Account FindAccount(string strToken)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Account account = this[i];
                if (account.Token == strToken)
                    return account;
            }

            return null;
        }
    }

}
