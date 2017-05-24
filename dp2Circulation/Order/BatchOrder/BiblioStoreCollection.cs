using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2Circulation
{
    // 存储书目对象的集合。
    // 可以当作数组遍历，也可以通过记录路径快速获取对象
    public class BiblioStoreCollection : IEnumerable
    {
        List<BiblioStore> _lines = new List<BiblioStore>();

        Hashtable _recPathTable = new Hashtable();  // biblio_recpath --> BiblioStore

        public void Clear()
        {
            _lines.Clear();
            _recPathTable.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return this._lines.GetEnumerator();
        }

        public ICollection Keys
        {
            get
            {
                return this._recPathTable.Keys;
            }
        }

        public BiblioStore GetByRecPath(string strBiblioRecPath)
        {
            return this._recPathTable[strBiblioRecPath] as BiblioStore;
        }

        public bool Remove(BiblioStore biblio)
        {
            this._recPathTable.Remove(biblio.RecPath);
            return this._lines.Remove(biblio);
        }

        // TODO: 遇到 recpath 重复的，是否抛出异常?
        public void Add(BiblioStore biblio)
        {
            if (this._recPathTable.ContainsKey(biblio.RecPath) == true)
                throw new ArgumentException("路径为 '"+biblio.RecPath+"' 的书目记录不能重复添加");

            this._lines.Add(biblio);
            this._recPathTable[biblio.RecPath] = biblio;
        }
    }
}
