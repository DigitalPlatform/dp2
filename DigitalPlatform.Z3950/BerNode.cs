using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;

namespace DigitalPlatform.Z3950
{
    public class BerNode
    {
        public List<BerNode> ChildrenCollection = new List<BerNode>();

        public BerNode ParentNode = null;   /* 父结点 */

        public byte [] m_baData = null;

        public string m_strDebugInfo = "";

        public UInt16 m_uTag = 0;    /* 标识号 */
	    public char  m_cClass = (char)0; 
        /* tag 类型:
          00 = universal
          01 = application
          10 = context specific
          11 = private */
        public char m_cForm = (char)1;
        /* 
          0 = primitive
          1 = constructed */

        #region 常量

        public const char ASN1_UNIVERSAL   = (char)0;
        public const char ASN1_APPLICATION = (char)1;
        public const char ASN1_CONTEXT = (char)2;
        public const char ASN1_PRIVATE = (char)3;

        public const char ASN1_PRIMITIVE = (char)0;
        public const char ASN1_CONSTRUCTED = (char)1;

        public const char ASN1_BOOLEAN          = (char) 1;
        public const char ASN1_INTEGER          = (char) 2;
        public const char ASN1_BITSTRING        = (char) 3;
        public const char ASN1_OCTETSTRING      = (char) 4;
        public const char ASN1_NULL             = (char) 5;
        public const char ASN1_OBJECTIDENTIFIER = (char) 6;
        public const char ASN1_OBJECTDESCRIPTOR = (char) 7;
        public const char ASN1_EXTERNAL         = (char) 8;
        public const char ASN1_SEQUENCE         = (char)16;
        public const char ASN1_SET              = (char)17;
        public const char ASN1_VISIBLESTRING    = (char)26;
        public const char ASN1_GENERALSTRING    = (char)27;

        #endregion

        public BerNode GetLeftSibling()
        {
            if (this.ParentNode == null)
                return null;

            int nIndex = this.ParentNode.ChildrenCollection.IndexOf(this);

            Debug.Assert(nIndex != -1, "");

            if (nIndex == -1)
                return null;

            if (nIndex == 0)
                return null;

            Debug.Assert(nIndex < this.ParentNode.ChildrenCollection.Count, "");
            return this.ParentNode.ChildrenCollection[nIndex - 1];
        }

        public BerNode GetRightSibling()
        {
            if (this.ParentNode == null)
                return null;

            int nIndex = this.ParentNode.ChildrenCollection.IndexOf(this);

            Debug.Assert(nIndex != -1, "");

            if (nIndex == -1)
                return null;

            if (nIndex >= this.ParentNode.ChildrenCollection.Count - 1)
                return null;

            Debug.Assert(nIndex < this.ParentNode.ChildrenCollection.Count, "");
            return this.ParentNode.ChildrenCollection[nIndex + 1];
        }

        // 在当前结点下方构造一个constructed结构的非叶子结点
        // parameters:
        // return:
        //		null
        //		其他
        public BerNode NewChildConstructedNode(UInt16 uTag,
            char cClass)
        {
	        BerNode node = null;

            if (this.m_cForm == ASN1_PRIMITIVE)
                return null;

            node = new BerNode();
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

            node.m_uTag = uTag;
	        node.m_cClass = cClass;
            node.m_cForm = ASN1_CONSTRUCTED;

            return node;
        }

        // 在当前结点下方构造一个存放字符数据的子结点
        // parameters:
        // return:
        //		null
        //		其他
        public BerNode NewChildCharNode(UInt16 uTag,
            char cClass,
            byte [] baData)
        {
            BerNode node = null;


        	if (this.m_cForm==ASN1_PRIMITIVE)
                return null;

            node = new BerNode();
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

            node.m_baData = baData;
            node.m_uTag = uTag;
            node.m_cClass = cClass;
            node.m_cForm = ASN1_PRIMITIVE;

            return node;
        }

        // 2007/7/16
        public BerNode NewChildBooleanNode(UInt16 uTag,
            char cClass,
            bool bData)
        {
            BerNode node = null;

            if (this.m_cForm == ASN1_PRIMITIVE)
                return null;

            node = new BerNode();
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

            node.m_baData = new byte[1];
            if (bData == true)
                node.m_baData[0] = 1;
            else
                node.m_baData[0] = 0;

            node.m_uTag = uTag;
            node.m_cClass = cClass;
            node.m_cForm = ASN1_PRIMITIVE;

            return node;
        }

        // 在当前结点下方构造一个存放bitstring数据的子结点
        // parameters:
        // return:
        //		null
        //		其他
        public BerNode NewChildBitstringNode(UInt16 uTag,
            char cClass,
            string strValue)
        {
            // CByteArray charray;
            List<byte> charray = new List<byte>();

            byte[] mask = new byte[] {0x80, 0x40, 0x20, 0x10, 
		0x08, 0x04, 0x02, 0x01};

            int nLen = strValue.Length;
            int nByteNum = 1 + (nLen / 8) + ((nLen % 8) != 0 ? 1 : 0);

            int unused = 8 - (nLen % 8);
            if (unused == 8)
                unused = 0;

            byte c = (byte)unused;
            charray.Add(c);

            for (int i = 0; i < nLen; i += 8)
            {

                c = 0;

                for (int j = 0; j < 8 && i + j < nLen; j++)
                {

                    if (strValue[i + j] == 'y' || strValue[i + j] == 'Y'
                        || strValue[i + j] == 't' || strValue[i + j] == 'T'
                        || strValue[i + j] == '1')
                    {
                        c |= mask[j];
                    }
                }

                charray.Add(c);
            }

            byte[] temp = new byte[charray.Count];
            charray.CopyTo(temp);

            return NewChildCharNode(uTag, cClass, temp);
        }

        // 改变整数的高低顺序
        static void ChangeIntegerOrder(ref List<byte> baData)
        {
            List<byte> baTemp = new List<byte>();

            if (baData.Count == 4)
            {
                baTemp.Add(baData[3]);
                baTemp.Add(baData[2]);
                baTemp.Add(baData[1]);
                baTemp.Add(baData[0]);
            }
            else if (baData.Count == 8)
            {
                baTemp.Add(baData[7]);
                baTemp.Add(baData[6]);
                baTemp.Add(baData[5]);
                baTemp.Add(baData[4]);
                baTemp.Add(baData[3]);
                baTemp.Add(baData[2]);
                baTemp.Add(baData[1]);
                baTemp.Add(baData[0]);
            }
            else if (baData.Count == 2)
            {
                baTemp.Add(baData[1]);
                baTemp.Add(baData[0]);
            }
            else
            {
                Debug.Assert(false, "不支持的整数长度 " + baData.Count);
                return;
            }

            baData = baTemp;
        }

        public BerNode NewChildIntegerNode(UInt16 uTag,
             char cClass,
            long lValue)
        {
            byte [] value = BitConverter.GetBytes((long)lValue);

            return NewChildIntegerNode(uTag,
                cClass,
                value);
        }


        // 在当前结点下方构造一个存放整型数据的子结点
        // parameters:
        // return:
        //		null
        //		其他
        public BerNode NewChildIntegerNode(UInt16 uTag,
            char cClass,
            byte[] baData)
        {
            if (this.m_cForm == ASN1_PRIMITIVE)
                return null;

            BerNode node = null;
            int nLen;

            List<byte> charray = new List<byte>();

            node = new BerNode();
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

            nLen = baData.Length;

            charray.AddRange(baData);

            ChangeIntegerOrder(ref charray);


            if (charray[0] == 0)
            {
                while ((charray.Count > 1) &&
                    (charray[0] == 0) && (charray[1] < 128))
                    charray.RemoveAt(0);
            }
            else
            {
                if (charray[0] == 0xff)
                    while ((charray.Count > 1) &&
                        (charray[0] == 0xff) && (charray[1] > 127))
                        charray.RemoveAt(0);
            }

            node.m_baData = new byte[charray.Count];
            charray.CopyTo(node.m_baData);

            node.m_uTag = uTag;
            node.m_cClass = cClass;
            node.m_cForm = ASN1_PRIMITIVE;

            return node;

        }

        string GetNumber(string strText,
            int nStart)
        {
            int nRet = strText.IndexOf(".", nStart);
            if (nRet != -1)
                return strText.Substring(nStart, nRet - nStart);

            return strText.Substring(nStart);
        }


        // 在当前结点下方构造一个存放OIDs数据的结点
        // parameters:
        // return:
        //		NULL
        //		其他
        public BerNode NewChildOIDsNode(UInt16 uTag,
            char cClass,
            string strValue)
        {
            if (this.m_cForm == ASN1_PRIMITIVE)
                return null;

            BerNode node = null;
            // List<byte> charray;

            node = new BerNode();
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

            byte[] place = new byte[100]; /* seems like enough */
            int offset = 0;
            long value;

            int nRet = 0;
            string strTemp = "";

            for (int i = 0; i < strValue.Length; )
            {
                /*
                if (Char.IsDigit(strValue[i]) == false)
                    break;
                 * */

                if (i > 90)
                {
                    Debug.Assert(false, "");
                    return null;
                }


                strTemp = GetNumber(strValue,i);
                
                value = Convert.ToInt64(strTemp);

                if (i == 0)  /* first 2 numbers get special treatment */
                {

                    nRet = strValue.IndexOf('.', i);
                    if (nRet == -1)
                    {
                        Debug.Assert(false, "");
                        return null;
                    }

                    i = nRet + 1;

                    strTemp = GetNumber(strValue, i);
                    value = value * 40 + Convert.ToInt64(strTemp);
                }

                if (value >= 0x80)
                {
                    int count = 0;
                    byte[] bits = new byte[12];  /* Save a 84 (12*7) bit number */

                    while (value != 0)
                    {
                        bits[count++] = (byte)(value & 0x7F );
                        value >>= 7;
                    }

                    /* Now place in the correct order */
                    while (--count > 0)
                        place[offset++] = (byte)(bits[count] | 0x80);

                    place[offset++] = bits[count];
                }
                else
                    place[offset++] = (byte)value;

                nRet = strValue.IndexOf('.', i);
                if (nRet != -1)
                {
                    i = nRet + 1;
                }
                else
                {
                    break;
                }
            }


            node.m_baData = new byte[offset];
            Array.Copy(place, node.m_baData,
                offset);

            node.m_uTag = uTag;
            node.m_cClass = cClass;
            node.m_cForm = ASN1_PRIMITIVE;

            return node;
        }



        //

        // 在此结点下增加一棵子树
        // parameters:
        public void AddSubtree(BerNode sub)
        {
    	    this.ChildrenCollection.Add(sub);
            sub.ParentNode = this;
        }

        // 将本结点以及所有子结点编码为BER包
        public void EncodeBERPackage(ref byte[] baPackage)
        {
            // 1.得到全部子结点的BER包

            BerNode node = null;
            byte[] baTempPackage = null;

            // nMax = m_ChildArray.GetSize();

            if (this.ChildrenCollection.Count != 0)
            {
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    node = this.ChildrenCollection[i];
                    Debug.Assert(node != null);

                    // 递归
                    baTempPackage = null;
                    node.EncodeBERPackage(ref baTempPackage);

                    if (baTempPackage == null)
                    {
                        // 是否有问题?
                        continue;   // 2008/12/17
                    }

                    Debug.Assert(baTempPackage != null, "");    // 2008/12/17
                    baPackage = ByteArray.Add(baPackage, baTempPackage);
                }
            }
            else
            {
                Debug.Assert(this.m_baData != null, "");    // 2007/7/20
                baPackage = ByteArray.Add(baPackage, this.m_baData);
            }


            // 2.根据1.步得到的包长度，最终加入本结点需要的识别信息
            MakeHeadPart(ref baTempPackage, baPackage.Length);

            baPackage = ByteArray.Add(baTempPackage, baPackage);
        }

        // 根据所有下级结点共同构成的包的总长度， 最终构造出本结点的头部
        void MakeHeadPart(ref byte[] baHead,
            int nDataSize)
        {
            baHead = null;
            byte[] baTempPackage = null;

            // 1.构造tag + class + form包
            MakeTagClassFormPart(ref baTempPackage);

            Debug.Assert(baTempPackage.Length != 0, "");

            baHead = baTempPackage;


            // 2.构造length
            baTempPackage = null;
            MakeLengthPart(ref baTempPackage, nDataSize);

            Debug.Assert(baTempPackage.Length != 0, "");
            baHead = ByteArray.Add(baHead, baTempPackage);
        }


        // 构造tag + class + form包
        void MakeTagClassFormPart(ref byte[] baPart)
        {
            byte[] charray = new byte[3];
            int nBytes = 0;

            baPart = null;

            if (this.m_uTag < 31)
            {	/* 如果结点的tag值小于31，则只需一个字节
								存放cForm、cClass及nTag值 */
                charray[0] = (byte)
                (this.m_uTag
                + this.m_cClass * 64
                + this.m_cForm * 32);
                nBytes = 1;
            }

            else						/* 否则需要2-3个字节存放 */
            {

                charray[nBytes] = (byte)
                    (31
                    + this.m_cClass * 64
                    + this.m_cForm * 32);
                nBytes++;
                if (this.m_uTag < 128)
                {
                    charray[nBytes] = (byte)this.m_uTag;
                }
                else
                {
                    /* old 2007/7/20
                    charray[nBytes] = (byte)(128 + this.m_uTag / 8);
                    nBytes++;
                    charray[nBytes] = (byte)(this.m_uTag % 8);
                     * */

                    // 只做了 < 16384 (128*128) 的部分 //zu

                    if (this.m_uTag < 16384)
                    {
                        charray[nBytes] = (byte)(128 + this.m_uTag / 128);
                        nBytes++;
                        charray[nBytes] = (byte)(this.m_uTag % 128);
                    }
                    else
                    {
                        //nBytes--; // 16384 -- 65535 //zu//return (tag == 0)
                    }

                }
                nBytes++;
            }

            baPart = new byte[nBytes];
            Array.Copy(charray, baPart, nBytes);
        }

        public static int LEN_LEN(int nDataSize)
        {
            return (nDataSize < 128 ? 1 : (nDataSize < 256 ? 2 : (nDataSize < 65536L ? 3 : 5)));
        }

#if NOOOOOOOOOOOOOOOOOO

        // 构造length
        void MakeLengthPart(ref byte[] baPart,
            int nDataSize)
        {
            byte[] charray = new byte[5];
            byte chTemp;
            int nBytes = 0;
            int unused;
            byte[] chtemparray = new byte[4];

            baPart = null;

            if (nDataSize < 128)
            {			/* 只需一个字节 */

                charray[0] = (byte)nDataSize;
                nBytes = 1;
            }
            else						/* 需要LEN_LEN(struRoot)字节 */
            {
                int nWidth;

                nWidth = LEN_LEN(nDataSize) - 1;
                charray[0] = (byte)(nWidth + 128);
                Debug.Assert(4 == sizeof(int), "");
                unused = sizeof(int) - (nWidth);

                chtemparray = BitConverter.GetBytes((int)nDataSize);

                chTemp = (byte)nDataSize;

                Array.Copy(chtemparray, 0, charray, 1, nWidth);
                nBytes = nWidth + 1;
            }

            baPart = new byte[nBytes];
            Array.Copy(charray, baPart, nBytes);
            /*
            ASSERT(nBytes + 128 <= 255);
            ASSERT(nBytes <= 4);
            baPart.InsertAt(0, (unsigned char)(nBytes + 128));
            */
        }

#endif

        // 构造length
        void MakeLengthPart(ref byte[] baPart,
            int nDataSize)
        {
            /*
            byte[] charray = new byte[5];
            byte chTemp;
            int unused;
            byte[] chtemparray = new byte[4];
            baPart = null;
             * */

            if (nDataSize < 128)
            {	
                /* 只需一个字节 */
                baPart = new byte[1];
                baPart[0] = (byte)nDataSize;
                return;
            }
            else						/* 需要LEN_LEN(struRoot)字节 */
            {
                List<byte> baTemp = new List<byte>();

                byte uc;
		        while (nDataSize != 0)
                {
			        uc = (byte)(nDataSize & 0xff);
                    baTemp.Insert(0, uc);
			        nDataSize >>= 8;
		        }

                Debug.Assert(baTemp.Count <= sizeof(int));
                uc = (byte)(baTemp.Count + 128);
                baTemp.Insert(0, uc);

                baPart = new byte[baTemp.Count];
                baTemp.CopyTo(baPart);

                return;
            }
        }

        // BER包是否完整到达
        // 疑问：虽然本函数能够知道BER包是否完整，但是，如果缓冲区内容比一个BER包
        // 还长，也就是说多个BER包堆积起来，还需要得知当前已经结束的这个BER包在何处结束
        // return:
        //		TRUE	完整到达
        public static bool IsCompleteBER(byte[] baBuffer,
            long start,
            long len_param,
            out long remainder)
        {
            int lenlen, tag, taglen;
            long fieldlen, headerlen;

            long len = len_param;

            remainder = -1;

            if (len == 0)
                return false;

            // 探测tag占据byte数
            taglen = get_tag(out tag, baBuffer, (int)start, len);
            if (taglen == 0)
            {
                /* no tag yet */
                //fnprintf("recv.log","\r\n IsCompleteBER() taglen=[%d] tag[%d] len[%d]",taglen, tag, len);
                return false;
            }

            // 探测长度占据byte数
            int temp = 0;
            lenlen = get_len(out temp, baBuffer, (int)start + taglen, (int)len - taglen);
            fieldlen = temp;

            if (lenlen == 0)
                return false;  /* no len yet */

            // fieldlen为内容长度
            long offs = start;

            headerlen = taglen + lenlen;
            if (lenlen == 1 && fieldlen == -1)  /* indefinite 不确定 length */
            {
                long totlen = 0;
                long fieldlen1 = 0;

                /* loop through the subfields and see if they are complete */
                for (offs += headerlen, len -= headerlen;
                    len > 1 && (baBuffer[offs] != 0 || baBuffer[offs + 1] != 0);
                    offs += fieldlen1, len -= fieldlen1)
                {
                    if (IsCompleteBER(baBuffer, offs, len, out fieldlen1) == false)
                        return false;
                    totlen += fieldlen1;
                }
                if (len > 1 && baBuffer[offs] == 0 && baBuffer[offs + 1] == 0)	// 当发现某个部分开头两个字符为0
                {
                    remainder = headerlen + totlen + 2;  /* + 2 nulls at end */
                    Debug.Assert(remainder <= len_param, "");
                    return true;
                }
                remainder = -1;  /* special flag to indicate indefinite length */
                /* items */
                return false;
            }

            if (fieldlen + headerlen <= len)
            {
                remainder = fieldlen + headerlen;
                Debug.Assert(remainder <= len_param, "");
                return true;
            }
            remainder = fieldlen + headerlen - len;
            return false;
        }

        // 得到tag值
        // parameters:
        //		tag		[out]返回得到的tag值
        //		s		BER包开始位置指针
        // return:
        //		0	没有得到 
        //		其他	tag占据byte数
        static int get_tag(out int tag,
            byte[] baBuffer,
            int nStart,
            long len)
        {
            byte c;
            int taglen;

            if (len == 0)                    /* nothing to look at */
            {
                tag = -1;
                return 0;
            }

            int i = nStart;

            c = (byte)(baBuffer[i] & 0x1f);              /* the first byte of the tag */
            i++;
            taglen = 1;

            if (c < 0x1f)                  /* if the tag is less than 31, then it */
            {
                tag = c;                 /* is fully contained in the one byte */
            }
            else                        /* otherwise we keep looking at bytes */
            {                           /* until we find 1 with it's sign bit off */
                /* catenating the last 7 bits of each byte */
                if (len == 1)  /* no extra bytes to look at */
                {
                    tag = -1;
                    return 0;
                }
                tag = 0;
                c = baBuffer[i];
                i++;
                taglen += 1;
                while (c > 0x80 && taglen < len)
                {
                    tag += c & 0x7F;
                    tag <<= 7;
                    c = baBuffer[i];
                    i++;
                    taglen += 1;
                }
                if (c > 0x80 && taglen == len)  /* missing part of tag */
                {
                    tag = -1;
                    return 0;
                }
                tag += c;
            }

            return taglen;
        }



        static int get_len(out int fieldlen,
            byte[] baBuffer,
            int nStart,
            int len)
        {
            int i;
            byte c;
            int tlen;

            if (len == 0)
            {
                fieldlen = -1;
                return 0;
            }

            int offs = nStart;

            c = baBuffer[offs];

            offs++;

            /* if the sign bit is turned on in the first byte of the length field, then
               the first byte contains the number of subsequent bytes necessary to contain
               the length, or the length is indefinite if the remaining bits are zero.
               Otherwise, the first byte contains the length */

            if (c < 128)  /* sign bit off */
            {
                fieldlen = c;
                return 1;
            }

            tlen = 0;
            if ((int)c - 128 > 4)  /* paranoia check: no lengths greater than 2 billion */
            {
                fieldlen = -1;
                return 0;
            }

            /* Make sure we got enough length bytes. */
            if ((int)(c - 127) > len)
            {
                fieldlen = -1;
                return 0;
            }

            if (c == 128)  /* indefinite length */
            {
                fieldlen = -1;  /* who knows */
                return 1;     /* the length of the length field IS 1 */
            }

            for (i = 0; i < (int)c - 128; i++)
            {
                tlen <<= 8;
                tlen += baBuffer[offs];
                offs++;
            }

            fieldlen = tlen;
            /*
        #ifdef DEBUG
            printf("in get_len: ASN.1 length field is %d bytes long, *len=%ld\n",
                (int)c-127, *fieldlen);
        #endif
             * */
            return (int)c - 127;
        }


        // return:
        //      false
        //      true
        public bool BuildPartTree(byte[] baBuffer,
            int nHead,
            int nLenParam,
            out int nUsedLen)
        {
            nUsedLen = 0;

            int tag;
            int fieldlen, headerlen;
            int nTempLen;
            BerNode node;

            int nLen = nLenParam;

            if (nLen == 0)
                return false;

            int offs = nHead;


            if (this.ParentNode == null)
            {
                node = new BerNode();
                node.ParentNode = this;
                this.ChildrenCollection.Add(node);

                return node.BuildPartTree(baBuffer,
                    nHead,
                    nLen,
                    out nUsedLen);
            }


            // 探测tag占据byte数
            int taglen = get_tag(out tag, baBuffer, offs, nLen);
            if (taglen == 0)
            { /* no tag yet */
                Debug.Assert(false, "");
                return false;
            }

            if (nLen == taglen)
                return false;

            Debug.Assert(nLen != taglen, "");

            // 探测长度占据byte数
            int lenlen = get_len(out fieldlen, 
                baBuffer,
                offs + taglen,
                nLen - taglen);
            if ((lenlen) == 0)
            {
                Debug.Assert(false, "");
                return false;  /* no len yet */
            }

            Debug.Assert(fieldlen < nLen - taglen, "");
            // fieldlen为内容长度

            headerlen = taglen + lenlen;

            nUsedLen += headerlen;
            Debug.Assert(nUsedLen <= nLenParam, "");

            if (lenlen == 1 && fieldlen == -1)  /* indefinite 不确定 length */
            {
                int fieldlen1 = 0;
                int totlen = 0;

                // 1.给此结点的Tag、Class、Form赋值
                SetTagClassForm(baBuffer, offs);

                /* loop through the subfields and see if they are complete */
                for (offs += headerlen, nLen -= headerlen;
                nLen > 1 && (baBuffer[offs] != 0 || baBuffer[offs + 1] != 0);
                offs += fieldlen1, nLen -= fieldlen1)
                {
                    node = new BerNode();	//生成此结点的一个子结点
                    node.ParentNode = this;
                    this.ChildrenCollection.Add(node);
                    if (node.BuildPartTree(baBuffer, offs, nLen, out nTempLen) == false)
                        return false;

                    totlen += nTempLen;
                    fieldlen1 = nTempLen;
                    nUsedLen += nTempLen;
                    Debug.Assert(nUsedLen <= nLenParam, "");
                }
                if (nLen > 1 && baBuffer[offs] == 0 && baBuffer[offs + 1] == 0)	// 当发现某个部分开头两个字符为0
                {
                    //*remainder=headerlen+totlen+2;  /* + 2 nulls at end */
                    nUsedLen += 2;
                    Debug.Assert(nUsedLen <= nLenParam, "");
                    return true;
                }
                //*remainder = -1;  /* special flag to indicate indefinite length */
                /* items */
                return false;
            }

            if (fieldlen + headerlen <= nLen)
            {
                // 4.给结点的数据赋值
                node = null;
                int nMax, nSubLen;

                // 1.给此结点的Tag、Class、Form赋值
                SetTagClassForm(baBuffer, nHead);

                //!!!!!!!!!!!!!

                if (this.m_cForm == ASN1_CONSTRUCTED)
                {
                    int nStart = 0;
                    nMax = fieldlen;
                    while (nMax > 0)
                    {
                        node = new BerNode();	//生成此结点的一个子结点
                        node.ParentNode = this;
                        this.ChildrenCollection.Add(node);

                        bool bRet = node.BuildPartTree(baBuffer,
                            nHead + headerlen + nStart,
                            fieldlen,
                            out nSubLen);

                        Debug.Assert(nSubLen <= fieldlen, "");


                        // 如果nSubLen永远为0怎么办
                        nMax -= nSubLen;
                        nStart += nSubLen;
                        nUsedLen += nSubLen;
                        Debug.Assert(nUsedLen <= nLenParam, "");


                        Debug.Assert(nMax >= 0, "");
                    }
                }
                else
                {

                    this.m_baData = new byte[fieldlen];
                    Array.Copy(baBuffer, nHead + headerlen,
                        this.m_baData, 0, fieldlen);
                    nUsedLen += fieldlen;
                    Debug.Assert(nUsedLen <= nLenParam, "");
                }
                //*remainder=fieldlen+headerlen;
                return true;
            }

            nUsedLen = fieldlen + headerlen;

            Debug.Assert(nUsedLen <= nLenParam, "");
            if (nUsedLen >= nLenParam)
                return false;   //

            //*remainder=fieldlen+headerlen-len;
            return true;
        }


        int SetTagClassForm(byte[] baBuffer,
            int nStart)
        {
            byte c;
            int nTaglen;
            int nDelta = 0;

            int offs = nStart;

            m_cClass = (char)(baBuffer[offs] >> 6);		// 给class赋值
            Debug.Assert(m_cClass >= 0 && m_cClass < 3, "");

            m_cForm = (char)((baBuffer[offs] >> 5) & 1);    // 给form赋值 
            nTaglen = 1;

            c = (byte)(baBuffer[offs] & 0x1f);
            Debug.Assert(c >= 0, "");
            nDelta++;
            if (c < 0x1f)             //  tag值小于31
                m_uTag = c;
            else
            {
                m_uTag = 0;
                c = baBuffer[offs + nDelta];
                nDelta++;
                while (c > 0x80)
                {
                    nTaglen += 1;
                    m_uTag += (UInt16)(c & 0x7F);
                    m_uTag <<= 7;   // 7 ?
                    // m_uTag &= 0xFF; //
                    c = baBuffer[offs + nDelta];
                    Debug.Assert(c >= 0, "");
                    nDelta++;
                }
                nTaglen += 1;
                m_uTag += c;
            }

            return nTaglen;
        }

#if NOOOOOOOOO
 // 应当用物理根调用本函数。
// 本函数要递归。第一次调用本函数负责创建逻辑根结点。
// 
        int BuildTreeNode(byte [] baPackage,
            out int nDelta,
            out int nTotlen)
{
	int nTaglen,nLength;
	int nDatalen;
	int nRet;
	BerNode node;

	ASSERT(0);


	if (this->m_pParent == NULL) {
		pNode = new CBERNode;
		ASSERT(pNode);
		this->m_ChildArray.Add(pNode);
	
		pNode->m_pParent = this;
		nDelta = 0;
		nTotlen = 0;
		return pNode->BldTreenode(baPackage,
						  nDelta,
						  nTotlen);

	}

	nTotlen = 0;

	// 1.给此结点的Tag、Class、Form赋值
	ASSERT(nDelta<baPackage.GetSize());
	nTaglen =GetTagClassForm(baPackage,nDelta);
	
	// 2.获取此结点数据所占的字节数
	nLength = GetDataBytenum(baPackage.GetData()+nDelta,
		baPackage.GetSize(),
		nDatalen);

	// 3.当记录的长度不确定时，判断其是否为完整的记录
	if (nDatalen==-1)
	{
		ASSERT(0);
		/*
		nRet = IsCompleteBER(baPackage.GetData()+nDelta,
			baPackage.GetSize());
		if(nRet!=0 && nRet!=-1)
		{
			nTotlen += nRet;
			nDatalen = nRet-(nTaglen+nLength);
			nDatalen -= 2;  // 去掉记录最后的两个零字节 
		}
		else
		{
			return -1;
		}

		*/
	}
	else {
		nTotlen += nTaglen+nLength+nDatalen;

		nDelta += nLength; 

	}
 
	// 4.给结点的数据赋值
	nRet = GetNodeData(baPackage,nDelta,nDatalen);
	if (nRet==-1)
		return -1;

	return 0;
}

#endif


        // 给此结点的Tag、Class、Form赋值
        // parameters: len已接收字节数的指针
        // return:
        // 存放tag、form、class值所需的字节数
        int GetTagClassForm(byte[] baPackage,
            ref int nDelta)
        {
            byte c;
            int nTaglen;

            m_cClass = (char)(baPackage[nDelta] >> 6);		// 给class赋值
            m_cForm = (char)((baPackage[nDelta] >> 5) & 1);    // 给form赋值 
            nTaglen = 1;

            c = (byte)((baPackage[nDelta]) & 0x1f);
            Debug.Assert(c >= 0, "");
            nDelta++;
            if (c < 0x1f)             //  tag值小于31
                m_uTag = c;
            else
            {
                m_uTag = 0;
                c = baPackage[nDelta];
                nDelta++;
                while (c > 0x80)
                {
                    nTaglen += 1;
                    m_uTag += (UInt16)(c & 0x7F);
                    m_uTag <<= 7;
                    c = baPackage[nDelta];
                    Debug.Assert(c >= 0, "");

                    nDelta++;
                }
                nTaglen += 1;
                m_uTag += c;
            }

            return nTaglen;
        }

#if NOOOOOOOOOOOOO
        // 给结点的m_baData赋值
        int GetNodeData(byte [] baPackage,
            ref int nDelta,
            ref int nFieldlen)
{
	        BerNode node = null;
	        int nSublen;

	        if (m_cForm==ASN1_CONSTRUCTED)
	{
		int nMax = nFieldlen;
		while (nMax>0)
		{
			node = new BerNode();	//生成此结点的一个子结点
            node.ParentNode = this;
            this.ChildrenCollection.Add(node);

			node.BuildTreeNode(baPackage,
                nDelta,
                nSublen);

			nMax -= nSublen;
//			nDelta += nSublen;
			TRACE("%d",nDelta);
		}
	}
	else {
		m_baData = new byte[nFieldlen];

		Debug.Assert(baPackage.Length >= nFieldlen, "");

                Array.Copy(baPackage, nDelta,
                    this.m_baData, 0, nFieldlen);
		nDelta += nFieldlen;
	}

  return 0;
}
#endif 

        // 获取OIDs结点的数据
        // parameters:
        // return:
        //		-1	error
        //		0	succeed
        public string GetOIDsNodeData()
        {
            if (this.m_cForm != ASN1_PRIMITIVE)
                return null;

            string strData = "";
            int i;
            int nvals = 0;
            long len, offset = 0;
            uint[] value = new uint[30];
            string strTemp;


            len = this.m_baData.Length;

            offset = 0;
            while (offset < len)		/* 将编码值转为字符 */
            {
                value[nvals] = 0;
                do
                {
                    value[nvals] <<= 7;
                    value[nvals] |= ((uint)m_baData[offset]) & 0x7f;
                } while ((m_baData[offset++] & 0x80) != 0);
                nvals++;
            }


            for (i = 0; i < nvals; i++)
            {
                if (i == 0)
                {
                    strTemp = (value[0] / 40).ToString() + "." + (value[0] % 40).ToString();
                }
                else
                {
                    strTemp = "." + (value[i]).ToString();
                }
                strData += strTemp;
            }

            return strData;
        }

        // 2007/7/25
        // 获取octect-string结点的数据
        public byte [] GetOctetsData()
        {
            return this.m_baData;
        }


        // 获取此char结点的数据
        // UTF-8版本
        // parameters:
        // return:
        //		NULL
        //		其他
        public string GetCharNodeData()
        {
            return Encoding.UTF8.GetString(this.m_baData);
        }

        // 获取此char结点的数据
        // 能指定编码方式的版本
        // parameters:
        // return:
        //		NULL
        //		其他
        public string GetCharNodeData(Encoding encoding)
        {
            return encoding.GetString(this.m_baData);
        }

        // 获取bitstring结点的数据
        // parameters:
        // return:
        //		NULL
        //		其他
        public string GetBitstringNodeData()
        {
            int nLastused;
            int nLen;
            int j, i;
            string strTemp;
            // byte[] bitsArray = null;

            byte[] mask = {0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02,
                                  0x01};

            string strResult = "";

            nLastused = 8 - this.m_baData[0];
            nLen = this.m_baData.Length;

            for (i = 1; i < nLen - 1; i++)
                for (j = 0; j < 8; j++)
                {
                    strTemp = ((this.m_baData[i] & mask[j]) != 0) ? "y" : "n";
                    strResult += strTemp;
                }

            // 有问题？
            if (i < this.m_baData.Length)   // 2007/7/16
            {
                for (j = 0; j < nLastused; j++)
                {
                    strTemp = ((this.m_baData[i] & mask[j]) != 0) ? "y" : "n";
                    strResult += strTemp;
                }
            }

            return strResult;
        }


        // 获取Integer结点的数据
        // parameters:
        // return:
        //		NULL
        //		其他
        public long GetIntegerNodeData()
        {
            int count;
            int longlen = sizeof(Int32);
            Debug.Assert(4 == longlen, "");

            long lData = 0;

            /* 数据范围不能超过'long' */
            if (this.m_baData.Length > longlen)
            {
                throw new Exception("m_baData.Length>" + longlen.ToString());
            }


            count = m_baData.Length;
            if (this.m_baData[0] > 127)
            {
                for (count = 0; count < longlen - m_baData.Length; count++)
                {
                    lData |= 0xff;
                    lData <<= 8;
                }
            }


            for (count = 0; count < m_baData.Length; count++)
            {
                lData |= m_baData[count];
                if (count < m_baData.Length - 1)
                    lData <<= 8;
            }
            return lData;
        }

        public void DumpToFile(string strFileName)
        {
            Stream stream = File.Create(strFileName);

            this.DumpToFile(stream);

            stream.Close();
        }

        // 获得一个用于调试的特性显示字符串
        public string GetDebugString()
        {
            string strText = "";

            strText = "tag=[" + this.m_uTag.ToString().PadLeft(3, '0') + "] "
    + "class=[" + ((int)this.m_cClass).ToString() + "] "
    + "form=[" + ((int)this.m_cForm).ToString() + "] ";

            if (this.m_baData != null)
            {
                strText += "datalen=[" + this.m_baData.Length.ToString() + "] "
                    + "content[" + ByteArrayToDispString(this.m_baData) + "] ";
            }
            else
            {
                strText += "datalen=0\r\n";
            }

            if (this.m_strDebugInfo != "")
                strText += " debuginfo[" + this.m_strDebugInfo + "]";

            return strText;
        }

        public void DumpToFile(Stream stream)
        {
            int i;
            string strIndent = "";
            BerNode obj = null;
            int nMax;


            obj = this;
            while (true)
            {
                if (obj.ParentNode == null)
                    break;
                strIndent += "    ";
                obj = obj.ParentNode;
            }

            string strText = "";

            if (this.ParentNode != null)
            {

                strText += strIndent + "{\r\n";
                strText += strIndent
                    + "tag=[" + this.m_uTag.ToString() + "] "
                    + "class=[" + ((int)this.m_cClass).ToString() + "] "
                    + "form=[" + ((int)this.m_cForm).ToString() + "]\r\n";

                if (this.m_baData != null)
                {
                    strText += strIndent
                        + "datalen=[" + this.m_baData.Length.ToString() + "] "
                        + "content[" + ByteArrayToDispString(this.m_baData) + "]\r\n";
                }
                else
                {
                    strText += strIndent
                        + "datalen=0\r\n";
                }

                if (this.m_strDebugInfo != "")
                    strText += "debuginfo[" + this.m_strDebugInfo + "]\r\n";

            }

            nMax = this.ChildrenCollection.Count;
            strText += strIndent + "childrencount=[" + nMax.ToString() + "]\r\n";

            byte[] baText = Encoding.UTF8.GetBytes(strText);
            stream.Write(baText, 0, baText.Length);

            for (i = 0; i < nMax; i++)
            {
                obj = this.ChildrenCollection[i];
                obj.DumpToFile(stream);
            }

            strText = strIndent + "}\r\n";
            baText = Encoding.UTF8.GetBytes(strText);
            stream.Write(baText, 0, baText.Length);
        }

        string ByteArrayToDispString(byte[] baData)
        {
            string strResult = "";
            for (int i = 0; i < baData.Length; i++)
            {
                strResult += Convert.ToString((byte)baData[i], 16).PadLeft(2, '0');
            }

            return strResult;
        }

    }



}
