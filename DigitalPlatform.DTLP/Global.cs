using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.DTLP
{
    public class Global
    {
        // 把路径加工为缩略或者正规形态
        // parameters:
        //      strCtlnoPart    如果为""，表示加工为缩略的、只用于前端显示形态，如果为"ctlno"或者"记录索引号"，表示加工为正规形态
        public static string ModifyDtlpRecPath(string strPath,
            string strCtlnoPart)
        {
            int nRet = strPath.LastIndexOf("/");

            if (nRet == -1)
                return strPath;

            string strNumber = strPath.Substring(nRet + 1).Trim();

            nRet = strPath.LastIndexOf("/", nRet - 1);
            if (nRet == -1)
                return strPath;

            string strPrevPart = strPath.Substring(0, nRet).Trim();

            return strPrevPart + "/" + strCtlnoPart + "/" + strNumber;
        }

        // 把一个字符串数组去重。调用前，应当已经排序
        public static void RemoveDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }

        }
    }
}
