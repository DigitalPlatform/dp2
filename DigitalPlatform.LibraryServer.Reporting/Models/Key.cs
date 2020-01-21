using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    // 检索点
    public class Key
    {
        [MaxLength(256)]
        public string Text { get; set; }

        // 检索点类型。例如 title author class class_clc
        [MaxLength(128)]
        public string Type { get; set; }

        [MaxLength(128)]
        public string BiblioRecPath { get; set; }

        // 检索点在同 Type 检索点中的序号，从 0 开始
        public int Index { get; set; }

        // 该检索点所从属的书目记录
        public virtual Biblio Biblio { get; set; }

        // 在 keys 中定位一个 key 的下标
        public static int IndexOf(List<Key> keys, Key key)
        {
            int i = 0;
            foreach(var current in keys)
            {
                if (IsEqual(current, key))
                    return i;
                i++;
            }

            return -1;
        }

        // 比对两个 Key 的值是否完全相等
        public static bool IsEqual(Key key1, Key key2)
        {
            if (key1.Text == key2.Text
                && key1.Type == key2.Type
                && key1.BiblioRecPath == key2.BiblioRecPath
                && key1.Index == key2.Index)
                return true;
            return false;
        }

        public override string ToString()
        {
            return $"Text={Text},Type={Type},BiblioRecPath={BiblioRecPath},Index={Index}";
        }
    }

}
