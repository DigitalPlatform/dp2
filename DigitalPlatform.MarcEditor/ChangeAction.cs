using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Marc;

namespace DigitalPlatform.MarcEditor
{
    /// <summary>
    /// 描述一个编辑修改基本动作
    /// </summary>
    public class ChangeAction
    {
        // 动作
        // GlobalInsert 插入 Offs(格式为 field_index:offs) NewText
        // GlobalChange 替换 Offs(格式为 field_index:offs) NewText OldText
        // GlobalDelete 删除 Offs(格式为 field_index:offs) OldText (NewText 为空)
        // FieldInsert 插入一个或者多个连续的字段 Offs(格式为 field_index) NewFields (OldField 为空) 
        // FieldChange 替换一个或者多个连续的字段 Offs(格式为 field_index) Length NewFields OldFields
        // FieldDelete 删除一个或者多个连续的字段 Offs(格式为 field_index) NewFields(空) OldFields(.Count 表明被删除的字段个数)
        // Reset MARC 记录内容全部重设 Text
        // Clear MARC 记录内容全部清除
        public string Action { get; set; }

        public string Offs { get; set; }

        public List<Field> NewFields { get; set; }
        public List<Field> OldFields { get; set; }

        public string NewText { get; set; }
        public string OldText { get; set; }

        // 插入符所在的小 Edit 栏目，和栏目内的偏移
        public int CaretCol { get; set; }
        public int CaretPos { get; set; }

        public int TryGetFieldIndex()
        {
            if (Int32.TryParse(this.Offs, out int value) == true)
                return value;
            return -1;
        }
    }


#if REMOVED
    /// <summary>
    /// 描述一个编辑修改基本动作
    /// </summary>
    public class ChangeAction
    {
        // 动作
        // Insert 插入 Offs Text
        // Replace 替换 Offs Length Text
        // Delete 删除 Offs Length
        // Reset MARC 记录内容全部重设 Text
        // Clear MARC 记录内容全部清除
        public string Action { get; set; }
        public int Offs { get; set; }
        public int Length { get; set; }

        public string Text { get; set; }
    }
#endif
}
