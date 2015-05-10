using System;
using System.Collections;

namespace dp2rms
{
	// 脚本功能名称
	public class ScriptAction
	{
		public string Name = "";
		public string Comment = "";
		public string ScriptEntry = "";	// 脚本入口函数名

		public bool Active = false;


	}

	/// <summary>
	/// 脚本功能名称集合
	/// </summary>
	public class ScriptActionCollection : ArrayList
	{
		public ScriptActionCollection()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// 加入一个新事项
		public ScriptAction NewItem(string strName,
			string strComment,
			string strScriptEntry,
			bool bActive)
		{
			ScriptAction item = new ScriptAction();
			item.Name = strName;
			item.Comment = strComment;
			item.ScriptEntry = strScriptEntry;
			item.Active = bActive;

			this.Add(item);
			return item;
		}
	}
}
