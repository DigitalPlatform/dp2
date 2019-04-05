using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.OldZ3950
{
    public class Record 
    {
        public byte [] m_baRecord = null;   // 原始形态的数据
        public string m_strSyntaxOID = "";
	    public string m_strDBName = "";
        public string m_strElementSetName = ""; // B / F

	    // 诊断信息
	    public int m_nDiagCondition = 0;    // 0表示没有诊断信息
	    public string m_strDiagSetID = "";
	    public string m_strAddInfo = "";

        public string AutoDetectedSyntaxOID = "";   // 自动识别的OID，后期使用
    }


    public class RecordCollection : List<Record>
    {
    }
}
