using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestUHF
{
    public class SELECTION
    {
        public Boolean m_enable;
        public Byte m_target;
        public Byte m_action;
        public Byte m_memBank;
        public UInt32 m_pointer;
        public Byte m_maskBitsLength;
        public List<Byte> m_maskBits;
        public SELECTION()
        {
            m_enable = false;
            m_maskBits = new List<Byte>();
            m_target = 0x04;
            m_action = 0x00;
            m_memBank = 0x01; //EPC
            m_pointer = 0x20;
        }
    }

    public class META_FLAGS
    {
        public Boolean m_enable;
        public Boolean m_EPC;
        public Boolean m_antennaID;
        public Boolean m_timestamp;
        public Boolean m_frequency;
        public Boolean m_RSSI;
        public Boolean m_readCnt;
        public Boolean m_tagData;

        public META_FLAGS()
        {
            m_enable = false;
            m_EPC = false;
            m_antennaID = false;
            m_timestamp = false;
            m_frequency = false;
            m_RSSI = false;
            m_readCnt = false;
            m_tagData = false;
        }
    }

    public class INVEN_READ
    {
        public Boolean m_enable;
        public Byte m_memBank;
        public UInt32 m_wordPtr;
        public UInt32 m_wordCnt;
        public INVEN_READ()
        {
            m_enable = false;
        }
    }

    public class EMBEDDED_WRITE
    {
        public Boolean m_enable;
        public Byte m_memBank;
        public UInt32 m_wordPtr;
        public UInt32 m_wordCnt;
        public List<Byte> m_datas;
        public EMBEDDED_WRITE()
        {
            m_enable = false;
            m_datas = new List<Byte>();
            m_wordPtr = 2;
            m_memBank = 01;
        }
    }

    public class EMBEDDED_Lock
    {
        public Boolean m_enable;
        public Boolean m_userMemSelected;
        public Boolean m_TIDMemSelected;
        public Boolean m_EPCMemSelected;
        public Boolean m_accessPwdSelected;
        public Boolean m_killPwdSelected;
        public uint m_userMem;
        public uint m_TIDMem;
        public uint m_EPCMem;
        public uint m_accessPwd;
        public uint m_killPwd;
        public EMBEDDED_Lock()
        {
            m_enable = false;
            m_userMemSelected = false;
            m_TIDMemSelected = false;
            m_EPCMemSelected = false;
            m_accessPwdSelected = false;
            m_killPwdSelected = false;
            m_userMem = 0;
            m_TIDMem = 0;
            m_EPCMem = 0;
            m_accessPwd = 0;
            m_killPwd = 0;
        }
    }

    public class PARAMETERS
    {
        public SELECTION m_sel;
        public META_FLAGS m_metaFlags;
        public UInt32 m_accessPwd;
        public INVEN_READ m_read;
        public EMBEDDED_WRITE m_write;
        public UInt32 m_timeout;
        public EMBEDDED_Lock m_lock;
        public PARAMETERS()
        {
            m_sel = new SELECTION();
            m_metaFlags = new META_FLAGS();
            m_read = new INVEN_READ();
            m_write = new EMBEDDED_WRITE();
            m_lock = new EMBEDDED_Lock();
            m_timeout = 5000;
        }
    }
}
