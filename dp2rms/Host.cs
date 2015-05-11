using System;
using System.Reflection;

namespace dp2rms
{
	/// <summary>
	/// Summary description for Host.
	/// </summary>
	public class Host
	{
		public DetailForm DetailForm = null;
		public Assembly Assembly = null;

		public Host()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public void Invoke(string strFuncName)
		{
			Type classType = this.GetType();

			// new一个Host派生对象
			classType.InvokeMember(strFuncName, 
				BindingFlags.DeclaredOnly | 
				BindingFlags.Public | BindingFlags.NonPublic | 
				BindingFlags.Instance | BindingFlags.InvokeMethod
				, 
				null,
				this,
				null);

		}

		public virtual void Main(object sender, HostEventArgs e)
		{

		}
	}


	public class HostEventArgs : EventArgs
	{
		public int i = 0;	// 测试用
	}
}
