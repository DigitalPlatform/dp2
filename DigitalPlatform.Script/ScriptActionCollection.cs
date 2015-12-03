using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitalPlatform.Script
{
    // 脚本功能名称
    public class ScriptAction
    {
        public string Name = "";
        public string Comment = "";
        public string ScriptEntry = "";	// 脚本入口函数名

        public bool Active = false;

        public char ShortcutKey = (char)0;  // 快捷键 2011/8/3
    }

    /// <summary>
    /// Ctrl+A 功能名称集合
    /// </summary>
    public class ScriptActionCollection : List<ScriptAction>
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
            bool bActive,
            char chShortcutKey = (char)0)
        {
            ScriptAction item = new ScriptAction();
            item.Name = strName;
            item.Comment = strComment;
            item.ScriptEntry = strScriptEntry;
            item.Active = bActive;
            item.ShortcutKey = chShortcutKey;

            this.Add(item);
            return item;
        }

        // 2011/8/3
        // 加入一个分割行
        public ScriptAction NewSeperator()
        {
            ScriptAction item = new ScriptAction();
            item.Name = "-";
            this.Add(item);
            return item;
        }
    }
}
