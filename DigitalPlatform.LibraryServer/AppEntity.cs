using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;	// Stop��
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// �������ǵ��ҵ��(��)��صĴ���
    /// </summary>
    public partial class LibraryApplication
    {
        // Ҫ��Ԫ�����б�
        static string[] core_entity_element_names = new string[] {
                "parent",
                "barcode",
                "state",
                "publishTime",   // 2007/10/24 new add
                "location",
                "seller",   // 2007/10/24 new add
                "source",   // 2008/2/15 new add ������Դ
                "price",
                "bookType",
                "registerNo",
                "comment",
                "mergeComment",
                "batchNo",
                "volume",    // 2007/10/19 new add
                "refID",    // 2008/4/16 new add
                "accessNo", // 2008/12/12 new add
                "intact",   // 2009/10/11 new add
                "binding",  // 2009/10/11 new add
                "operations", // 2009/10/24 new add
                "bindingCost",  // 2012/6/1 װ����
            };

        // <DoEntityOperChange()���¼�����>
        // �ϲ��¾ɼ�¼
        static int MergeTwoEntityXml(XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            // �㷨��Ҫ����, ��"�¼�¼"�е�Ҫ���ֶ�, ���ǵ�"�Ѵ��ڼ�¼"��

            /*
            // Ҫ��Ԫ�����б�
            string[] element_names = new string[] {
                "parent",
                "barcode",
                "state",
                "location",
                "price",
                "bookType",
                "registerNo",
                "comment",
                "mergeComment",
                "batchNo",
            };
             * */

            for (int i = 0; i < core_entity_element_names.Length; i++)
            {
                /*
                string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                    core_entity_element_names[i]);

                DomUtil.SetElementText(domExist.DocumentElement,
                    core_entity_element_names[i], strTextNew);
                 * */
                // 2009/10/24 changed
                string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                    core_entity_element_names[i]);

                DomUtil.SetElementOuterXml(domExist.DocumentElement,
                    core_entity_element_names[i], strTextNew);
            }

            strMergedXml = domExist.OuterXml;

            return 0;
        }

        // <DoEntityOperChange()���¼�����>
        // �Ƚ�������¼, �����Ͳ��¼�йص��ֶ��Ƿ����˱仯
        // return:
        //      0   û�б仯
        //      1   �б仯
        static int IsRegisterInfoChanged(XmlDocument dom1,
            XmlDocument dom2)
        {
            for (int i = 0; i < core_entity_element_names.Length; i++)
            {
                /*
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    core_entity_element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    core_entity_element_names[i]);
                 * */
                // 2009/10/24 changed ��Ϊ<operator>Ԫ���ڿ�������Ƕ��XML����
                string strText1 = DomUtil.GetElementOuterXml(dom1.DocumentElement,
                    core_entity_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(dom2.DocumentElement,
                    core_entity_element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }


        // ״̬�ַ������Ƿ�������ӹ��С���
        public static bool IncludeStateProcessing(string strStateString)
        {
            if (StringUtil.IsInList("�ӹ���", strStateString) == true)
                return true;
            return false;
        }

        // �������ֵ����0�����ж�ѭ��������
        public delegate int Delegate_checkRecord(string strRecPath,
            XmlDocument dom,
            byte [] baTimestamp,
            object param,
            out string strError);


        // ������Ŀ��¼������ʵ���¼������������Ҫ����Ϣ�������ṩ����ʵ��ɾ��ʱʹ��
        // parameters:
        //      strStyle    check_borrow_info,count_borrow_info,return_record_xml
        //                  ������ check_borrow_info ʱ�����ֵ�һ����ͨ��Ϣ������������������-1
        //                  ������ count_borrow_info ʱ������Ҫͳ��ȫ����ͨ��Ϣ�ĸ���
        // return:
        //      -2  not exist entity dbname
        //      -1  error
        //      >=0 ������ͨ��Ϣ��ʵ���¼����, ��strStyle����count_borrow_infoʱ��
        public int SearchChildEntities(RmsChannel channel,
            string strBiblioRecPath,
            string strStyle,
            Delegate_checkRecord procCheckRecord,
            object param,
            out long lHitCount,
            out List<DeleteEntityInfo> entityinfos,
            out string strError)
        {
            strError = "";
            lHitCount = 0;
            entityinfos = new List<DeleteEntityInfo>();

            int nRet = 0;

            bool bCheckBorrowInfo = StringUtil.IsInList("check_borrow_info", strStyle);
            bool bCountBorrowInfo = StringUtil.IsInList("count_borrow_info", strStyle);
            bool bReturnRecordXml = StringUtil.IsInList("return_record_xml", strStyle);
            bool bOnlyGetCount = StringUtil.IsInList("only_getcount", strStyle);

            if (bCheckBorrowInfo == true
                && bCountBorrowInfo == true)
            {
                strError = "strStyle��check_borrow_info��count_borrow_info����ͬʱ�߱�";
                return -1;
            }

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // �����Ŀ���Ӧ��ʵ�����
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2008/12/5 new add
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return 0;


            // ����ʵ�����ȫ���������ض�id�ļ�¼

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "����¼")       // 2007/9/14 new add
                + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "û���ҵ�������Ŀ��¼ '" + strBiblioRecPath + "' ���κ�ʵ���¼";
                return 0;
            }

            lHitCount = lRet;

            // ��������������
            if (bOnlyGetCount == true)
                return 0;

            int nResultCount = (int)lRet;

            if (nResultCount > 10000)
            {
                strError = "���в��¼�� " + nResultCount.ToString() + " ���� 10000, ��ʱ��֧��������ǵ�ɾ������";
                goto ERROR1;
            }

            int nBorrowInfoCount = 0;

            int nStart = 0;
            int nPerCount = 100;
            for (; ; )
            {
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }

                // ���ÿ����¼
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                    if (lRet == -1)
                    {
                        /*
                        entityinfo.RecPath = aPath[i];
                        entityinfo.ErrorCode = channel.OriginErrorCode;
                        entityinfo.ErrorInfo = channel.ErrorInfo;

                        entityinfo.OldRecord = "";
                        entityinfo.OldTimestamp = null;
                        entityinfo.NewRecord = "";
                        entityinfo.NewTimestamp = null;
                        entityinfo.Action = "";
                         * */
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        strError = "��ȡʵ���¼ '" + aPath[i] + "' ʱ��������: " + strError;
                        goto ERROR1;
                        // goto CONTINUE;
                    }

                    entityinfo.RecPath = strOutputPath;
                    entityinfo.OldTimestamp = timestamp;
                    if (bReturnRecordXml == true)
                        entityinfo.OldRecord = strXml;

                    if (bCheckBorrowInfo == true
                        || bCountBorrowInfo == true
                        || procCheckRecord != null)
                    {
                        // ����Ƿ��н�����Ϣ
                        // �Ѽ�¼װ��DOM
                        XmlDocument domExist = new XmlDocument();

                        try
                        {
                            domExist.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "ʵ���¼ '" + aPath[i] + "' װ�ؽ���DOMʱ��������: " + ex.Message;
                            goto ERROR1;
                        }

                        if (procCheckRecord != null)
                        {
                            nRet = procCheckRecord(strOutputPath, 
                                domExist, 
                                timestamp, 
                                param,
                                out strError);
                            if (nRet != 0)
                                return nRet;
                        }

                        entityinfo.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                            "barcode");

                        // TODO: ����־�ָ��׶ε��ñ�����ʱ���Ƿ��б�Ҫ����Ƿ������ͨ��Ϣ���ƺ���ʱӦǿ��ɾ��Ϊ��

                        // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                        string strDetail = "";
                        bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist, out strDetail);


                        if (bHasCirculationInfo == true)
                        {
                            if (bCheckBorrowInfo == true)
                            {
                                strError = "��ɾ���Ĳ��¼ '" + entityinfo.RecPath + "' �а�������ͨ��Ϣ(" + strDetail + ")(����������ܲ�������һ��)������ɾ�������ȫ��ɾ����������������";
                                goto ERROR1;
                            }
                            if (bCountBorrowInfo == true)
                                nBorrowInfoCount++;
                        }
                    }

                    // CONTINUE:
                    entityinfos.Add(entityinfo);
                }

                nStart += aPath.Count;
                if (nStart >= nResultCount)
                    break;
            }

            return nBorrowInfoCount;
        ERROR1:
            return -1;
        }

#if NO
        // �����Ŀ��¼������ʵ���¼������������Ҫ����Ϣ�������ṩ����ʵ��ɾ��ʱʹ��
        // parameters:
        //      strStyle    return_record_xml
        // return:
        //      -1  error
        //      0   succeed
        public int SearchChildRecords(RmsChannel channel,
            List<string> aPath,
            string strStyle,
            out List<DeleteEntityInfo> entityinfos,
            out string strError)
        {
            strError = "";
            entityinfos = new List<DeleteEntityInfo>();

            int nRet = 0;

            bool bReturnRecordXml = StringUtil.IsInList("return_record_xml", strStyle);

            // ���ÿ����¼
            for (int i = 0; i < aPath.Count; i++)
            {
                string strMetaData = "";
                string strXml = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                long lRet = channel.GetRes(aPath[i],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        continue;   // �Ƿ񱨴�?

                    strError = "��ȡ�¼���¼ '" + aPath[i] + "' ʱ��������: " + strError;
                    goto ERROR1;
                    // goto CONTINUE;
                }

                entityinfo.RecPath = strOutputPath;
                entityinfo.OldTimestamp = timestamp;
                if (bReturnRecordXml == true)
                    entityinfo.OldRecord = strXml;
#if NO
                    if (bCheckBorrowInfo == true
                        || bCountBorrowInfo == true)
                    {
                        // ����Ƿ��н�����Ϣ
                        // �Ѽ�¼װ��DOM
                        XmlDocument domExist = new XmlDocument();

                        try
                        {
                            domExist.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "ʵ���¼ '" + aPath[i] + "' װ�ؽ���DOMʱ��������: " + ex.Message;
                            goto ERROR1;
                        }

                        entityinfo.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                            "barcode");

                        // TODO: ����־�ָ��׶ε��ñ�����ʱ���Ƿ��б�Ҫ����Ƿ������ͨ��Ϣ���ƺ���ʱӦǿ��ɾ��Ϊ��

                        // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                        string strDetail = "";
                        bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist, out strDetail);


                        if (bHasCirculationInfo == true)
                        {
                            if (bCheckBorrowInfo == true)
                            {
                                strError = "��ɾ���Ĳ��¼ '" + entityinfo.RecPath + "' �а�������ͨ��Ϣ(" + strDetail + ")(����������ܲ�������һ��)������ɾ�������ȫ��ɾ����������������";
                                goto ERROR1;
                            }
                            if (bCountBorrowInfo == true)
                                nBorrowInfoCount++;
                        }
                    }
#endif

                // CONTINUE:
                entityinfos.Add(entityinfo);
            }

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // ��������ͬһ��Ŀ��¼��ȫ��ʵ���¼
        // parameters:
        //      strAction   copy / move
        // return:
        //      -2  Ŀ��ʵ��ⲻ���ڣ��޷����и��ƻ���ɾ��
        //      -1  error
        //      >=0  ʵ�ʸ��ƻ����ƶ���ʵ���¼��
        public int CopyBiblioChildEntities(RmsChannel channel,
            string strAction,
            List<DeleteEntityInfo> entityinfos,
            string strTargetBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nOperCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement(strAction == "copy" ? "copyEntityRecords" : "moveEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }

            // ���Ŀ����Ŀ��������ʵ�����
            string strTargetItemDbName = "";
            string strTargetBiblioDbName = ResPath.GetDbName(strTargetBiblioRecPath);
            // return:
            //      -1  ����
            //      0   û���ҵ�
            //      1   �ҵ�
            int nRet = this.GetItemDbName(strTargetBiblioDbName,
                out strTargetItemDbName,
                out strError);
            if (nRet == 0 || string.IsNullOrEmpty(strTargetItemDbName) == true)
            {
                return -2;   // Ŀ��ʵ��ⲻ����
            }

            string strParentID = ResPath.GetRecordId(strTargetBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "Ŀ����Ŀ��¼·�� '"+strTargetBiblioRecPath+"' ����ȷ���޷���ü�¼��";
                return -1;
            }

            List<string> newrecordpaths = new List<string>();
            List<string> oldrecordpaths = new List<string>();
            List<string> parentids = new List<string>();
            List<string> oldrecords = new List<string>();

            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                string strOutputRecPath = "";

                string strNewBarcode = "";  // �������޸ĺ�Ĳ������

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "��¼ '" + info.RecPath + "' װ��XMLDOM��������: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "parent",
                        strParentID);

                    // ���Ƶ������Ҫ������ֲ������������ظ�����
                    if (strAction == "copy")
                    {
                        // �޸Ĳ�����ţ����ⷢ��������ظ�
                        string strOldItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                            "barcode");
                        if (String.IsNullOrEmpty(strOldItemBarcode) == false)
                        {
                            strNewBarcode = strOldItemBarcode + "_" + Guid.NewGuid().ToString();
                            DomUtil.SetElementText(dom.DocumentElement,
                                "barcode",
                                strNewBarcode);
                        }

                        // *** �����⼸���������Ҫ��Ϊ�������

                        // ��� refid
                        DomUtil.SetElementText(dom.DocumentElement,
                            "refID",
                            null);


                        // �ѽ������
                        // (Դʵ���¼������н�����Ϣ������ͨ���������޷�ɾ���˼�¼�ġ�ֻ���ó��ɴ�������й黹��Ȼ�����ɾ��)
                        {
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrower",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowPeriod",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowDate",
                                null);
                        }
                    }

                    // TODO: ����˳��ȷ����û�ж�����Դ�����û�У���ʡ��CopyRecord����

                    long lRet = channel.DoCopyRecord(info.RecPath,
                         strTargetItemDbName + "/?",
                         strAction == "move" ? true : false,   // bDeleteSourceRecord
                         out output_timestamp,
                         out strOutputRecPath,
                         out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;
                        strError = "����ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                        goto ERROR1;
                    }



                    // �޸�xml��¼��<parent>Ԫ�ط����˱仯
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        dom.OuterXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    oldrecordpaths.Add(info.RecPath);
                    newrecordpaths.Add(strOutputRecPath);
                    parentids.Add(strParentID);
                    if (strAction == "move")
                        oldrecords.Add(info.OldRecord);
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // ��������־DOM��
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                    DomUtil.SetAttr(node, "targetRecPath", strOutputRecPath);

                    // 2014/1/5
                    if (string.IsNullOrEmpty(strNewBarcode) == false)
                        DomUtil.SetAttr(node, "newBarcode", strNewBarcode);
                }

                nOperCount++;
            }


            return nOperCount;
        ERROR1:
            // Undo�Ѿ����й��Ĳ���
            if (strAction == "copy")
            {
                string strWarning = "";

                foreach (string strRecPath in newrecordpaths)
                {
                    string strTempError = "";
                    byte[] timestamp = null;
                    byte[] output_timestamp = null;
                REDO_DELETE:
                    long lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strTempError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (timestamp == null)
                            {
                                timestamp = output_timestamp;
                                goto REDO_DELETE;
                            }
                        }
                        strWarning += strTempError + ";";
                    }

                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "����Undo�����У�����������: " + strWarning;
            }
            else if (strAction == "move")
            {
                string strWarning = "";
                for (int i = 0; i < newrecordpaths.Count; i++)
                {
                    byte[] output_timestamp = null;
                    string strOutputRecPath = "";
                    string strTempError = "";
                    // TODO: ���ȷ��û�ж��󣬾Ϳ���ʡ����һ��
                    long lRet = channel.DoCopyRecord(newrecordpaths[i],
         oldrecordpaths[i],
         true,   // bDeleteSourceRecord
         out output_timestamp,
         out strOutputRecPath,
         out strTempError);
                    if (lRet == -1)
                    {
                        strWarning += strTempError + ";";
                    }

                    // �޸�xml��¼��<parent>Ԫ�ط����˱仯
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(oldrecordpaths[i],
                        oldrecords[i],
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strTempError);
                    if (lRet == -1)
                    {
                        strWarning += strTempError + ";";
                    }
                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "����Undo�����У�����������: " + strWarning;
            }
            return -1;
        }

        // ��������ͬһ��Ŀ��¼��ȫ��ʵ���¼
        // parameters:
        //      strAction   copy / move
        // return:
        //      -1  error
        //      >=0  ʵ�ʸ��ƻ����ƶ���ʵ���¼��
        public int CopyBiblioChildRecords(RmsChannel channel,
            string strAction,
            List<DeleteEntityInfo> entityinfos,
            List<string> target_recpaths,
            string strTargetBiblioRecPath,
            List<string> newbarcodes,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            if (entityinfos.Count != target_recpaths.Count)
            {
                strError = "entityinfos.Count (" + entityinfos.Count.ToString() + ") != target_recpaths.Count (" + target_recpaths.Count .ToString()+ ")";
                return -1;
            }

            int nOperCount = 0;

            string strParentID = ResPath.GetRecordId(strTargetBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "Ŀ����Ŀ��¼·�� '" + strTargetBiblioRecPath + "' ����ȷ���޷���ü�¼��";
                return -1;
            }

            List<string> newrecordpaths = new List<string>();
            List<string> oldrecordpaths = new List<string>();
            List<string> parentids = new List<string>();
            List<string> oldrecords = new List<string>();

            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];
                string strTargetRecPath = target_recpaths[i];

                string strNewBarcode = newbarcodes[i];

                byte[] output_timestamp = null;
                string strOutputRecPath = "";

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "��¼ '" + info.RecPath + "' װ��XMLDOM��������: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "parent",
                        strParentID);

                    // ���Ƶ������Ҫ������ֲ������������ظ�����
                    if (strAction == "copy")
                    {
                        // �޸Ĳ�����ţ����ⷢ��������ظ�
                        string strOldItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                            "barcode");
                        if (String.IsNullOrEmpty(strOldItemBarcode) == false)
                        {
                            // 2014/1/5
                            if (string.IsNullOrEmpty(strNewBarcode) == true)
                                strNewBarcode = "temp_" + strOldItemBarcode;
                            DomUtil.SetElementText(dom.DocumentElement,
                                "barcode",
                                strNewBarcode);
                        }

                        // 2014/1/5
                        DomUtil.SetElementText(dom.DocumentElement,
                            "refID",
                            null);

                        // �ѽ������
                        // (Դʵ���¼������н�����Ϣ������ͨ���������޷�ɾ���˼�¼�ġ�ֻ���ó��ɴ�������й黹��Ȼ�����ɾ��)
                        {
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrower",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowPeriod",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowDate",
                                null);
                        }
                    }

                    // TODO: ����˳��ȷ����û�ж�����Դ�����û�У���ʡ��CopyRecord����

                    long lRet = channel.DoCopyRecord(info.RecPath,
                         strTargetRecPath,
                         strAction == "move" ? true : false,   // bDeleteSourceRecord
                         out output_timestamp,
                         out strOutputRecPath,
                         out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;
                        strError = "����ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                        goto ERROR1;
                    }



                    // �޸�xml��¼��<parent>Ԫ�ط����˱仯
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        dom.OuterXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    oldrecordpaths.Add(info.RecPath);
                    newrecordpaths.Add(strOutputRecPath);
                    parentids.Add(strParentID);
                    if (strAction == "move")
                        oldrecords.Add(info.OldRecord);
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                nOperCount++;
            }

            return nOperCount;
        ERROR1:
            // ��ҪUndo
            return -1;
        }

        // ɾ������ͬһ��Ŀ��¼��ȫ��ʵ���¼
        // ������Ҫ�ṩEntityInfo����İ汾
        // return:
        //      -1  error
        //      0   û���ҵ�������Ŀ��¼���κ�ʵ���¼�����Ҳ���޴�ɾ��
        //      >0  ʵ��ɾ����ʵ���¼��
        public int DeleteBiblioChildEntities(RmsChannel channel,
            List<DeleteEntityInfo> entityinfos,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("deletedEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }

            // ����ʵ��ɾ��
            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;

            REDO_DELETE:

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {

                    long lRet = channel.DoDeleteRes(info.RecPath,
                        info.OldTimestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        // ��������ԣ���ʱ�������¶������
                        // ���Ҫ���ԣ�Ҳ�ü������¶�����¼���ж������ж��޽軹��Ϣ����ɾ��

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount > 10)
                            {
                                strError = "������10�λ����С�ɾ��ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                                goto ERROR1;
                            }
                            nRedoCount++;

                            // ���¶����¼
                            string strMetaData = "";
                            string strXml = "";
                            string strOutputPath = "";
                            string strError_1 = "";

                            lRet = channel.GetRes(info.RecPath,
                                out strXml,
                                out strMetaData,
                                out output_timestamp,
                                out strOutputPath,
                                out strError_1);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                    continue;

                                strError = "��ɾ��ʵ���¼ '" + info.RecPath + "' ʱ����ʱ�����ͻ�������Զ����»�ȡ��¼�����ַ�������: " + strError_1;
                                goto ERROR1;
                                // goto CONTINUE;
                            }

                            // ����Ƿ��н�����Ϣ
                            // �Ѽ�¼װ��DOM
                            XmlDocument domExist = new XmlDocument();

                            try
                            {
                                domExist.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "ʵ���¼ '"+info.RecPath+"' XMLװ�ؽ���DOMʱ��������: " + ex.Message;
                                goto ERROR1;
                            }

                            info.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                                "barcode");

                            // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                            string strDetail = "";
                            bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                                out strDetail);
                            if (bHasCirculationInfo == true)
                            {
                                strError = "��ɾ���Ĳ��¼ '" + info.RecPath + "' �а�������ͨ��Ϣ("+strDetail+")(����������ܲ�������һ��)������ɾ����";
                                goto ERROR1;
                            }


                            info.OldTimestamp = output_timestamp;
                            goto REDO_DELETE;
                        }

                        strError = "ɾ��ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // ��������־DOM��
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                }

                nDeletedCount++;
            }


            return nDeletedCount;
        ERROR1:
            return -1;
        }

        // ɾ������ͬһ��Ŀ��¼��ȫ��ʵ���¼��ע�⣬������������¼��·ͨ��Ϣ
        // ���Ǽ�����ɾ��һ�ν��еİ汾
        // return:
        //      -1  error
        //      0   û���ҵ�������Ŀ��¼���κ�ʵ���¼�����Ҳ���޴�ɾ��
        //      >0  ʵ��ɾ����ʵ���¼��
        public int DeleteBiblioChildEntities(RmsChannel channel,
            string strBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            List<DeleteEntityInfo> entityinfos = null;
            long lHitCount = 0;
            // return:
            //      -2  not exist entity dbname
            //      -1  error
            //      >=0 ������ͨ��Ϣ��ʵ���¼����
            int nRet = SearchChildEntities(channel,
                strBiblioRecPath,
                "",   // "check_borrow_info",    // 2011/4/24
                (Delegate_checkRecord)null,
                null,
                out lHitCount,
                out entityinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
            {
                Debug.Assert(entityinfos.Count == 0, "");
            }
            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            nRet = DeleteBiblioChildEntities(channel,
                entityinfos,
                domOperLog,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
        ERROR1:
            return -1;
        }

#if NOOOOOOOOOOOOOOOOOOO
        // ɾ������ͬһ��Ŀ��¼��ȫ��ʵ���¼
        // return:
        //      -1  error
        //      0   û���ҵ�������Ŀ��¼���κ�ʵ���¼�����Ҳ���޴�ɾ��
        //      >0  ʵ��ɾ����ʵ���¼��
        public int DeleteBiblioChildEntities(RmsChannelCollection Channels,
            string strBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // �����Ŀ���Ӧ��ʵ�����
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // ����ʵ�����ȫ���������ض�id�ļ�¼

            string strQueryXml = "<target list='" + strItemDbName + ":" + "����¼" + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "û���ҵ�������Ŀ��¼ '" + strBiblioRecPath + "' ���κ�ʵ���¼";
                return 0;
            }

            int nResultCount = (int)lRet;

            if (nResultCount > 500)
            {
                strError = "���в��¼�� " + nResultCount.ToString() + " ���� 500, ��ʱ��֧��������ǵ�ɾ������";
                goto ERROR1;
            }

            List<EntityInfo> entityinfos = new List<EntityInfo>();

            int nStart = 0;
            int nPerCount = 100;
            for (; ; )
            {
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }

                // ���ÿ����¼
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    EntityInfo entityinfo = new EntityInfo();

                    if (lRet == -1)
                    {
                        /*
                        entityinfo.RecPath = aPath[i];
                        entityinfo.ErrorCode = channel.OriginErrorCode;
                        entityinfo.ErrorInfo = channel.ErrorInfo;

                        entityinfo.OldRecord = "";
                        entityinfo.OldTimestamp = null;
                        entityinfo.NewRecord = "";
                        entityinfo.NewTimestamp = null;
                        entityinfo.Action = "";
                         * */
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        strError = "��ȡʵ���¼ '" + aPath[i] + "' ʱ��������: " + strError;
                        goto ERROR1;
                        // goto CONTINUE;
                    }

                    entityinfo.RecPath = strOutputPath;
                    entityinfo.OldTimestamp = timestamp;
                    /*
                    entityinfo.OldRecord = strXml;
                    entityinfo.NewRecord = "";
                    entityinfo.NewTimestamp = null;
                    entityinfo.Action = "";
                     * */
                    // ����Ƿ��н�����Ϣ
                    // �Ѽ�¼װ��DOM
                    XmlDocument domExist = new XmlDocument();

                    try
                    {
                        domExist.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                        goto ERROR1;
                    }

                    // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                    if (IsEntityHasCirculationInfo(domExist) == true)
                    {
                        strError = "��ɾ���Ĳ��¼ '" + entityinfo.RecPath + "' �а�������ͨ��Ϣ(����������ܲ�������һ��)������ɾ����";
                        goto ERROR1;
                    }

                // CONTINUE:
                    entityinfos.Add(entityinfo);
                }

                nStart += aPath.Count;
                if (nStart >= nResultCount)
                    break;
            }

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("deletedEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }


            // ����ʵ��ɾ��
            for (int i = 0; i < entityinfos.Count; i++)
            {
                EntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;
            REDO_DELETE:
                lRet = channel.DoDeleteRes(info.RecPath,
                    info.OldTimestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        continue;

                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount > 10)
                        {
                            strError = "������10�λ����С�ɾ��ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                            goto ERROR1;
                        }
                        nRedoCount++;
                        info.OldTimestamp = output_timestamp;
                        goto REDO_DELETE;
                    }

                    strError = "ɾ��ʵ���¼ '" + info.RecPath + "' ʱ��������: " + strError;
                    goto ERROR1;
                }

                // ��������־DOM��
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                }

                nDeletedCount++;
            }


            return nDeletedCount;
        ERROR1:
            return -1;
        }
#endif

        // ��װ��İ汾��������ǰ�Ľű�����
        public LibraryServerResult GetEntities(
    SessionInfo sessioninfo,
    string strBiblioRecPath,
    long lStart,
    long lCount,
    out EntityInfo[] entities)
        {
            return GetEntities(
    sessioninfo,
    strBiblioRecPath,
    lStart,
    lCount,
    "",
    "zh",
    out entities);
        }

        // ��style�ַ����еõ� librarycode:XXXX�Ӵ�
        // ע�⣬���xxxx���Ƕ���ݴ��룬Ҫ���Ϊ "code1|code2"��������̬�����������Զ���'|'�滻Ϊ','
        static string GetLibraryCodeParam(string strStyle)
        {
            string[] parts = strStyle.Split(new char[] { ',' });
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                if (StringUtil.HasHead(strText, "librarycode:") == true)
                    return strText.Substring("librarycode:".Length).Trim().Replace("|",",");
            }

            return null;
        }


        // ��ò���Ϣ
        // parameters:
        //      strBiblioRecPath    ��Ŀ��¼·����������������id���֡������ @path-list: ��������ʾ�����Ǹ��ݸ����Ĳ��¼·������ȡ���¼
        //      lStart  ���شӵڼ�����ʼ    2009/6/7 add
        //      lCount  �ܹ����ؼ�����0��-1����ʾȫ������(0��Ϊ�˼��ݾ�API)
        //      entityinfos ���ص�ʵ����Ϣ����
        //      strStyle    "opac" ��ʵ���¼����OPACҪ����мӹ�������һЩԪ��
        //                  "onlygetpath"   ������ÿ��·��
        //                  "getfirstxml"   �Ƕ�onlygetpath�Ĳ��䣬����õ�һ��Ԫ�ص�XML��¼���������Ȼֻ����·��
        // Ȩ�ޣ���Ҫ��getentitiesȨ��
        // return:
        //      Result.Value    -1���� 0û���ҵ� ���� �ܵ�ʵ���¼�ĸ���(���η��صģ�����ͨ��entities.Count�õ�)
        public LibraryServerResult GetEntities(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,    // 2011/1/21
            string strLang,     // 2011/1/21
            out EntityInfo[] entities)
        {
            entities = null;

            LibraryServerResult result = new LibraryServerResult();

            // Ȩ���ַ���
            if (StringUtil.IsInList("getentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("getiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "��ò���Ϣ �������ܾ������߱�order��getiteminfo��getentitiesȨ�ޡ�";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // �淶������ֵ
            if (lCount == 0)
                lCount = -1;

            int nRet = 0;
            string strError = "";

            // �����¼������
            // ȫ���û���������
            //      ʲô����������ָ��
            // �ֹ��û������ȫ���ֹ�
            //      style��Ҫ���� getotherlibraryitem
            // �ֹ��û���ֻ����Լ���Ͻ�ķֹ�
            //      ʲô����������ָ��
            // ȫ���û���ֻ����ָ���ķֹ�
            //      style��Ҫ���� librarycode:xxxx
            // �ֹ��û���ֻ����ָ���ķֹݡ�ע�⣬�ⲻһ����ָ�ֹ��û���Ͻ�ķֹ�
            //      style��Ҫ���� librarycode:xxxx

            string strLibraryCodeParam = GetLibraryCodeParam(strStyle);
            if (sessioninfo.GlobalUser == false && string.IsNullOrEmpty(strLibraryCodeParam) == true)
                strLibraryCodeParam = sessioninfo.LibraryCodeList;

            bool bGetOtherLibraryItem = StringUtil.IsInList("getotherlibraryitem", strStyle);
            /*
            if (bGetOtherLibraryItem == true)
                strLibraryCodeParam = null;
             * */

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // �����Ŀ���Ӧ��ʵ�����
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            if (String.IsNullOrEmpty(strItemDbName) == true)
            {
                result.Value = -1;
                result.ErrorInfo = "��Ŀ�� '"+strBiblioDbName+"' δ����������ʵ���";
                result.ErrorCode = ErrorCode.ItemDbNotDef;
                return result;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // ����ʵ�����ȫ���������ض�id�ļ�¼

            string strQueryXml = "";

            if ((sessioninfo.GlobalUser == true && string.IsNullOrEmpty(strLibraryCodeParam) == true)
                || bGetOtherLibraryItem == true)
            {
                strQueryXml = "<target list='"
                     + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "����¼")       // 2007/9/14 new add
                     + "'><item><word>"
                     + strBiblioRecId
                     + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
            }
            else
            {
                // ����ȡ�õ�ǰ�û���Ͻ�ķֹݵĲ��¼
                List<string> codes = StringUtil.SplitList(strLibraryCodeParam); // sessioninfo.LibraryCodeList
                foreach (string strCode in codes)
                {
                    string strOneQueryXml = "<target list='"
         + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "����¼+�ݲصص�")
         + "'><item><word>"
         + StringUtil.GetXmlStringSimple(strBiblioRecId + "|" + strCode + "/")
         +"</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
                    if (string.IsNullOrEmpty(strQueryXml) == false)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneQueryXml;
                }
                if (codes.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "û���ҵ�";
                return result;
            }

            int MAXPERBATCH = 100;

            int nResultCount = (int)lRet;

            if (lCount == -1)
                lCount = nResultCount - (int)lStart;

            // lStart�Ƿ�Խ��
            if (lStart >= (long)nResultCount)
            {
                strError = "lStart����ֵ " + lStart.ToString() + " ���������н������β�������н������Ϊ " + nResultCount.ToString();
                goto ERROR1;
            }

            // 2010/12/16
            // ����lCount
            if (lStart + lCount > (long)nResultCount)
            {
                // strError = "lStart����ֵ " + lStart.ToString() + " ��lCount����ֵ " + lCount.ToString() + " ֮�ʹ������н������ " + nResultCount.ToString();
                // goto ERROR1;
                lCount = (long)nResultCount - lStart;
            }

            // �Ƿ񳬹�ÿ�����ֵ
            if (lCount > MAXPERBATCH)
                lCount = MAXPERBATCH;

            /*
            // 2009/6/7 new add
            if (lCount > 0)
                nResultCount = Math.Min(nResultCount-(int)lStart, (int)lCount);
             * */

            /*
            if (nResultCount > 10000)
            {
                strError = "���в��¼�� " + nResultCount.ToString() + " ���� 10000, ��ʱ��֧��";
                goto ERROR1;
            }*/

            List<EntityInfo> entityinfos = new List<EntityInfo>();

            int nStart = (int)lStart;
            int nPerCount = Math.Min(MAXPERBATCH, (int)lCount); // 2009/6/7 changed
            for (; ; )
            {
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    strLang,    // 2012/4/16 // "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }

                bool bOnlyGetPath = StringUtil.IsInList("onlygetpath", strStyle);
                bool bGetFirstXml = StringUtil.IsInList("getfirstxml", strStyle);

                // ���ÿ����¼
                for (int i = 0; i < aPath.Count; i++)
                {
                    EntityInfo entityinfo = new EntityInfo();
                    if (bOnlyGetPath == true)
                    {
                        if (bGetFirstXml == false
                            || i > 0)
                        {
                            entityinfo.OldRecPath = aPath[i];
                            goto CONTINUE;
                        }
                    }

                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);

                    if (lRet == -1)
                    {
                        entityinfo.OldRecPath = aPath[i];
                        entityinfo.ErrorCode = channel.OriginErrorCode;
                        entityinfo.ErrorInfo = channel.ErrorInfo;

                        entityinfo.OldRecord = "";
                        entityinfo.OldTimestamp = null;

                        entityinfo.NewRecPath = "";
                        entityinfo.NewRecord = "";
                        entityinfo.NewTimestamp = null;
                        entityinfo.Action = "";
                        
                        goto CONTINUE;
                    }

                    XmlDocument itemdom = null;

                    // �޸�<borrower>
                    if (sessioninfo.GlobalUser == false) // �ֹ��û�����Ҫ���ˣ���ΪҪ�޸�<borrower>
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        {
                            string strLibraryCode = "";
                            // ���һ�����¼�Ĺݲصص��Ƿ���ϵ�ǰ�û���Ͻ�Ĺݴ����б�Ҫ��
                            // return:
                            //      -1  �����̳���
                            //      0   ����Ҫ��
                            //      1   ������Ҫ��
                            nRet = CheckItemLibraryCode(itemdom,
                                        sessioninfo.LibraryCodeList,
                                        out strLibraryCode,
                                        out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            if (nRet == 1)
                            {
                                // �ѽ����˵�֤����Ÿ���
                                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                                    "borrower");
                                if (string.IsNullOrEmpty(strBorrower) == false)
                                    DomUtil.SetElementText(itemdom.DocumentElement,
                                        "borrower", new string('*', strBorrower.Length));
                                strXml = itemdom.DocumentElement.OuterXml;
                            }
                        }
                    }

                    // ��ʵ���¼����OPACҪ����мӹ�������һЩԪ��
                    if (StringUtil.IsInList("opac", strStyle) == true)
                    {
                        if (itemdom == null)
                        {
                            nRet = LibraryApplication.LoadToDom(strXml,
                                out itemdom,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        nRet = AddOpacInfos(
                            sessioninfo,
                            strLang,
                            ref itemdom,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        strXml = itemdom.DocumentElement.OuterXml;
                    }

                    entityinfo.OldRecPath = strOutputPath;
                    entityinfo.OldRecord = strXml;
                    entityinfo.OldTimestamp = timestamp;

                    entityinfo.NewRecPath = "";
                    entityinfo.NewRecord = "";
                    entityinfo.NewTimestamp = null;
                    entityinfo.Action = "";

                CONTINUE:
                    entityinfos.Add(entityinfo);
                }

                nStart += aPath.Count;
                if (nStart >= nResultCount)
                    break;
                if (entityinfos.Count >= lCount)
                    break;

                // ����nPerCount
                if (entityinfos.Count + nPerCount > lCount)
                    nPerCount = (int)lCount - entityinfos.Count;
            }

            // �ҽӵ������
            entities = new EntityInfo[entityinfos.Count];
            for (int i = 0; i < entityinfos.Count; i++)
            {
                entities[i] = entityinfos[i];
            }

            result.Value = nResultCount;   // entities.Length;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // �����¼������OPAC��Ϣ
        // parameters:
        //      strLibraryCode  ���߼�¼�������Ķ��߿�Ĺݴ���
        int AddOpacInfos(
            SessionInfo sessioninfo,
            string strLang,
            ref XmlDocument item_dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
            string strState = DomUtil.GetElementText(item_dom.DocumentElement, "state");

            // <borrowerRecPath>��2012/9/8�Ժ�Ϊʵ���¼������һ��Ԫ�أ������ǽ��߶��߼�¼��·��
            string strBorrowerRecPath = DomUtil.GetElementText(item_dom.DocumentElement, "borrowerRecPath");


            string strBorrowerLibraryCode = ""; // ��ǰ��Ľ��������ڵĹݴ���
            if (string.IsNullOrEmpty(strBorrowerRecPath) == false)
            {
                nRet = this.GetLibraryCode(strBorrowerRecPath,
                    out strBorrowerLibraryCode,
                    out strError);
                /*
                if (nRet == -1)
                    goto ERROR1;
                 * */
                // TODO: ��α���?
            }


            // �ݲصص�
            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
            // ȥ��#reservation����
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // ���������Ĺݲصص��Ƿ�϶������ڵĹݲصص��Ǻ�
            string strPureLocationName = "";
            string strItemLibraryCode = ""; // ��ǰ�����ڵĹݴ���

            // ����
            ParseCalendarName(strLocation,
        out strItemLibraryCode,
        out strPureLocationName);

            bool bResultValue = false;
            string strMessageText = "";

            // ִ�нű�����ItemCanBorrow
            // parameters:
            // return:
            //      -2  not found script
            //      -1  ����
            //      0   �ɹ�
            nRet = this.DoItemCanBorrowScriptFunction(
                false,
                sessioninfo.Account,
                item_dom,
                out bResultValue,
                out strMessageText,
                out strError);
            if (nRet == -1)
            {
                strMessageText = strError;
            }

            if (nRet == -2)
            {
                // ����״̬�Ƿ�Ϊ��, ����checkbox״̬
                if (string.IsNullOrEmpty(strState) == false)
                {
                    string strText = "�˲���״̬Ϊ " + strState + " ��������衣";
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                        "canBorrow", strText);
                    DomUtil.SetAttr(node, "canBorrow", "false");
                }
                else
                {
                    // ע��ȫ���û��͵�ÿ���ֹݶ��ɽ衣��OPAC items ������ʵ����Ҫ���������֤��������ύԤԼ���֪��Ч��
                    if (sessioninfo.GlobalUser == false
                        && StringUtil.IsInList(strItemLibraryCode, sessioninfo.LibraryCodeList) == false)
                    {
                        string strText = "�˲����������ֹ� " + strItemLibraryCode + " �����ܽ��ġ�";
                        XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                            "canBorrow", strText);
                        DomUtil.SetAttr(node, "canBorrow", "false");
                    }
                    else
                    {
                        // ���ݹݲصص��Ƿ��������, ����checkbox״̬
                        List<string> locations = this.GetLocationTypes(strItemLibraryCode, true);
                        if (locations.IndexOf(strPureLocationName) == -1)
                        {
                            string strText = "�˲������ݲصص� " + strLocation + " ��������衣";
                            XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                                "canBorrow", strText);
                            DomUtil.SetAttr(node, "canBorrow", "false");
                        }
                    }
                }
            }
            else
            {
                if (bResultValue == false)
                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
        "canBorrow", strMessageText);
                    DomUtil.SetAttr(node, "canBorrow", "false");
                }
            }

            if (String.IsNullOrEmpty(strMessageText) == false)
            {
                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
    "stateMessage", strMessageText);
            }

            // ״̬
            // string strState = DomUtil.GetElementText(dom.DocumentElement, "state");



            // ��������
            //string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");

            // �����
            //string strNo = DomUtil.GetElementText(dom.DocumentElement, "no");

            // ��������
            string strBorrowDate = DomUtil.GetElementText(item_dom.DocumentElement, "borrowDate");
            string strTime = strBorrowDate;
            if (String.IsNullOrEmpty(strTime) == false)
            {
                try
                {
                    strTime = DateTimeUtil.LocalTime(strTime);
                }
                catch
                {
                    strTime = "ʱ���ʽ���� -- " + strTime;
                }
            }

            string strClass = "";

            // <borrowerReaderType>��2009/9/18�Ժ�Ϊʵ���¼������һ��Ԫ�أ��ǴӶ��߼�¼��<readerType>�и��ƹ�����
            string strBorrowerReaderType = DomUtil.GetElementText(item_dom.DocumentElement, "borrowerReaderType");

            // ��������
            string strPeriod = DomUtil.GetElementText(item_dom.DocumentElement, "borrowPeriod");

            string strOverDue = ""; // ��������ַ������Ѿ����淶���������ڵ�ʱ������ַ���Ϊ��ֵ
            string strOriginOverdue = "";   // ��������ַ�����û�мӹ���������ǲ����ڵ�ʱ�����˵���ж����쵽��
            long lOver = 0;
            string strPeriodUnit = "";

            if (String.IsNullOrEmpty(strBorrowDate) == false)
            {
                // �������
                Calendar calendar = null;

                if (String.IsNullOrEmpty(strBorrowerReaderType) == false)
                {
                    // return:
                    //      -1  ����
                    //      0   û���ҵ�����
                    //      1   �ҵ�����
                    nRet = this.GetReaderCalendar(strBorrowerReaderType,
                        strBorrowerLibraryCode,
                        out calendar,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        calendar = null;
                    }
                }

                // ��鳬�������
                // return:
                //      -1  ���ݸ�ʽ����
                //      0   û�з��ֳ���
                //      1   ���ֳ���   strError������ʾ��Ϣ
                //      2   �Ѿ��ڿ������ڣ������׳��� 2009/3/13 new add
                nRet = this.CheckPeriod(
                    calendar,   // 2009/9/18 changed
                    strBorrowDate,
                    strPeriod,
                    out lOver,
                    out strPeriodUnit,
                    out strError);

                strOriginOverdue = strError;

                if (nRet == -1)
                    strOverDue = strError;  // ������Ϣ

                if (nRet == 1)
                    strOverDue = this.GetString("�ѳ���");
                else if (nRet == 2) // 2009/9/18 new add
                    strOverDue = this.GetString("���ڿ������ڣ���������");

                /*
                if (nRet == 1 || nRet == 0)
                    strOverDue = strError;	// "�ѳ���";
                 * */
                if (nRet == 1)
                    strClass = "over";
                else if (nRet == 2) // 2009/9/18 new add
                    strClass = "warning";
                else if (nRet == 0 && lOver >= -5)
                    strClass = "warning";

                // strPeriod = this.GetDisplayTimePeriodStringEx(strPeriod);


                // �������
                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                        "overdueInfo", strOverDue);
                    if (String.IsNullOrEmpty(strClass) == false)
                        DomUtil.SetAttr(node, "type", strClass);
                }

                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                            "originOverdueInfo", strOriginOverdue);
                    DomUtil.SetAttr(node, "calendar", calendar != null ? calendar.Name : "");
                    DomUtil.SetAttr(node, "over", lOver.ToString());
                    DomUtil.SetAttr(node, "unit", strPeriodUnit);
                }
            }

            return 0;
        }


        // ����/�������Ϣ
        // parameters:
        //      strBiblioRecPath    ��Ŀ��¼·����������������id���֡�������������ȷ����Ŀ�⣬id���Ա�ʵ���¼��������<parent>Ԫ�����ݡ�������Ŀ������EntityInfo�е�NewRecPath�γ�ӳ�չ�ϵ����Ҫ��������Ƿ���ȷ��Ӧ
        //      entityinfos Ҫ�ύ�ĵ�ʵ����Ϣ����
        // Ȩ�ޣ���Ҫ��setentitiesȨ��
        // TODO: д�����еļ�¼, ��ȱ��<operator>��<operTime>�ֶ�
        // TODO: ��Ҫ�����¼��<parent>Ԫ�������Ƿ�Ϸ�������Ϊ�ʺ�
        public LibraryServerResult SetEntities(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            EntityInfo[] entityinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = new LibraryServerResult();

            // Ȩ���ַ���
            if (StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "�������Ϣ �������ܾ������߱�order��setiteminfo��setentitiesȨ�ޡ�";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            int nRet = 0;
            long lRet = 0;
            string strError = "";

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // �����Ŀ���Ӧ��ʵ�����
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            // ���ʵ����� 2014/9/5
            if (string.IsNullOrEmpty(strBiblioDbName) == false 
                && string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "��Ŀ�� '" + strBiblioDbName + "' ���߱�������ʵ��⣬����ʵ���¼�Ĳ���ʧ��";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            byte[] output_timestamp = null;
            string strOutputPath = "";

            List<EntityInfo> ErrorInfos = new List<EntityInfo>();

            for (int i = 0; i < entityinfos.Length; i++)
            {
                EntityInfo info = entityinfos[i];

                string strAction = info.Action;

                bool bForce = false;    // �Ƿ�Ϊǿ�Ʋ���(ǿ�Ʋ�����ȥ��Դ��¼�е���ͨ��Ϣ�ֶ�����)
                bool bNoCheckDup = false;   // �Ƿ�Ϊ������?
                bool bNoEventLog = false;   // �Ƿ�Ϊ�������¼���־?
                bool bNoOperations = false; // �Ƿ�Ϊ��Ҫ����<operations>����

                string strStyle = info.Style;

                if (info.Action == "forcenew"
                    || info.Action == "forcechange"
                    || info.Action == "forcedelete")
                {
                    bForce = true;

                    // ��strAction�����޸�Ϊ������forceǰ׺����
                    info.Action = info.Action.Remove(0, "force".Length);

                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "�޸Ĳ���Ϣ��" + strAction + "�������ܾ������߱�restoreȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "�޸Ĳ���Ϣ��" + strAction + "�������ܾ���ֻ��ȫ���û����߱�restoreȨ�޲��ܽ��������Ĳ�����";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // �ӹ�style�ַ���������д����־
                    if (StringUtil.IsInList("force", strStyle) == true)
                        StringUtil.SetInList(ref strStyle, "force", true);

                }
                    // 2008/10/6 new add
                else if (StringUtil.IsInList("force", info.Style) == true)
                {
                    bForce = true;

                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "���з�� 'force' ���޸Ĳ���Ϣ��" + strAction + "�������ܾ������߱�restoreȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "�޸Ĳ���Ϣ��" + strAction + "�������ܾ���ֻ��ȫ���û����߱�restoreȨ�޲��ܽ��������Ĳ�����";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // 2008/10/6 new add
                if (StringUtil.IsInList("nocheckdup", info.Style) == true)
                {
                    bNoCheckDup = true;
                }

                if (StringUtil.IsInList("noeventlog", info.Style) == true)
                {
                    bNoEventLog = true;
                }

                if (StringUtil.IsInList("nooperations", info.Style) == true)
                {
                    bNoOperations = true;
                }

                if (bNoCheckDup == true)
                {
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "���з�� 'nocheckdup' ���޸Ĳ���Ϣ��" + strAction + "�������ܾ������߱�restoreȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "���з�� 'nocheckdup' ���޸Ĳ���Ϣ��" + strAction + "�������ܾ���ֻ��ȫ���û����߱�restoreȨ�޲��ܽ��������Ĳ�����";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (bNoEventLog == true)
                {
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "���з�� 'noeventlog' ���޸Ĳ���Ϣ��" + strAction + "�������ܾ������߱�restoreȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "���з�� 'noeventlog' ���޸Ĳ���Ϣ��" + strAction + "�������ܾ���ֻ��ȫ���û����߱�restoreȨ�޲��ܽ��������Ĳ�����";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // ��info�ڵĲ������м�顣
                strError = "";

                // 2008/2/17 new add
                if (entityinfos.Length > 1  // 2013/9/26 ֻ��һ����¼��ʱ�򣬲������� refid ��λ������Ϣ�����Ҳ�Ͳ���Ҫ���Ը������ RefID ��Ա��
                    && String.IsNullOrEmpty(info.RefID) == true)
                {
                    strError = "info.RefID û�и���";
                }

                if (info.NewRecPath != null
                    && info.NewRecPath.IndexOf(",") != -1)
                {
                    strError = "info.NewRecPath ֵ '" + info.NewRecPath + "' �в��ܰ�������";
                }
                else if (info.OldRecPath != null
                    && info.OldRecPath.IndexOf(",") != -1)
                {
                    strError = "info.OldRecPath ֵ '" + info.OldRecPath + "' �в��ܰ�������";
                }


                // ������Ϊ"delete"ʱ���Ƿ��������ֻ����OldRecPath������������NewRecPath
                // ������������ã���Ҫ������Ϊһ�µġ�
                // 2007/11/12 new add
                if (info.Action == "delete")
                {
                    if (String.IsNullOrEmpty(info.NewRecord) == false)
                    {
                        strError = "strActionֵΪdeleteʱ, info.NewRecord��������Ϊ��";
                    }
                    else if (info.NewTimestamp != null)
                    {
                        strError = "strActionֵΪdeleteʱ, info.NewTimestamp��������Ϊ��";
                    }
                        // 2008/6/24 new add
                    else if (String.IsNullOrEmpty(info.NewRecPath) == false)
                    {
                        if (info.NewRecPath != info.OldRecPath)
                        {
                            strError = "strActionֵΪdeleteʱ, ���info.NewRecPath���գ��������ݱ����info.OldRecPathһ�¡�(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                        }
                    }
                }
                else
                {
                    // ��delete��� info.NewRecord����벻Ϊ��
                    if (String.IsNullOrEmpty(info.NewRecord) == true)
                    {
                        strError = "strActionֵΪ" + info.Action + "ʱ, info.NewRecord��������Ϊ��";
                    }
                }

                if (info.Action == "new")
                {
                    if (String.IsNullOrEmpty(info.OldRecord) == false)
                    {
                        strError = "strActionֵΪnewʱ, info.OldRecord��������Ϊ��";
                    }
                    else if (info.OldTimestamp != null)
                    {
                        strError = "strActionֵΪnewʱ, info.OldTimestamp��������Ϊ��";
                    }
                }

                if (strError != "")
                {
                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // ���·���еĿ�������
                if (String.IsNullOrEmpty(info.NewRecPath) == false)
                {
                    strError = "";

                    string strDbName = ResPath.GetDbName(info.NewRecPath);

                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "NewRecPath�����ݿ�����ӦΪ��";
                    }

                    if (string.IsNullOrEmpty(strItemDbName) == false    // �п���ǰ�� strBiblioRecPath Ϊ�գ��� strItemDbName ҲΪ��
                        && strDbName != strItemDbName)
                    {
                        // ����Ƿ�Ϊ�������Եĵ�ͬ����
                        // parameters:
                        //      strDbName   Ҫ�������ݿ���
                        //      strNeutralDbName    ��֪�������������ݿ���
                        if (this.IsOtherLangName(strDbName,
                            strItemDbName) == false)
                        {
                            if (strAction == "copy" || strAction == "move")
                            {
                                // �ٿ�strDbName�Ƿ�������һ��ʵ���
                                if (this.IsItemDbName(strDbName) == false)
                                    strError = "RecPath�����ݿ��� '" + strDbName + "' ����ȷ��ӦΪʵ�����";
                            }
                            else
                                strError = "RecPath�����ݿ��� '" + strDbName + "' ����ȷ��ӦΪ '" + strItemDbName + "'��(��Ϊ��Ŀ����Ϊ '" + strBiblioDbName + "'�����Ӧ��ʵ�����ӦΪ '" + strItemDbName + "' )";
                        }
                    }
                    else if (string.IsNullOrEmpty(strItemDbName) == true)   // 2013/9/26
                    {
                        // Ҫ��鿴�� strDbName �Ƿ�Ϊһ��ʵ�����
                        if (this.IsItemDbName(strDbName) == false)
                            strError = "RecPath�����ݿ��� '" + strDbName + "' ����ȷ��ӦΪʵ�����";
                    }

                    if (strError != "")
                    {
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = strError;
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                        continue;
                    }
                }

                // ��(ǰ�˷�������)�ɼ�¼װ�ص�DOM
                XmlDocument domOldRec = new XmlDocument();
                try
                {
                    // ��strOldRecord��Ŀ���ǲ���ı�info.OldRecord����, ��Ϊ���߿��ܱ����Ƶ������Ϣ��
                    string strOldRecord = info.OldRecord;
                    if (String.IsNullOrEmpty(strOldRecord) == true)
                        strOldRecord = "<root />";

                    domOldRec.LoadXml(strOldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord XML��¼װ�ص�DOMʱ����: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // ��Ҫ������¼�¼װ�ص�DOM
                XmlDocument domNewRec = new XmlDocument();
                try
                {
                    // ��strNewRecord��Ŀ���ǲ���ı�info.NewRecord����, ��Ϊ���߿��ܱ����Ƶ������Ϣ��
                    string strNewRecord = info.NewRecord;

                    if (String.IsNullOrEmpty(strNewRecord) == true)
                        strNewRecord = "<root />";

                    domNewRec.LoadXml(strNewRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.NewRecord XML��¼װ�ص�DOMʱ����: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                string strOldBarcode = "";
                string strNewBarcode = "";

                // �Բ�����ż���?
                string strLockBarcode = "";

                try
                {
                    // ����new��change�Ĺ��в��� -- ����Ų���, Ҳ��Ҫ����
                    // delete����Ҫ����
                    if (info.Action == "new"
                        || info.Action == "change"
                        || info.Action == "delete"
                        || info.Action == "move")
                    {

                        // ����������ȡһ���������
                        // �����¾ɲ�������Ƿ��в���
                        // ��EntityInfo�е�OldRecord��NewRecord�а���������Ž��бȽ�, �����Ƿ����˱仯(��������Ҫ����)
                        // return:
                        //      -1  ����
                        //      0   ���
                        //      1   �����
                        nRet = CompareTwoBarcode(domOldRec,
                            domNewRec,
                            out strOldBarcode,
                            out strNewBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "CompareTwoBarcode() error : " + strError;
                            goto ERROR1;
                        }

                        if (info.Action == "new"
                            || info.Action == "change"
                            || info.Action == "move")
                            strLockBarcode = strNewBarcode;
                        else if (info.Action == "delete")
                        {
                            // ˳�����һЩ���
                            if (String.IsNullOrEmpty(strNewBarcode) == false)
                            {
                                strError = "û�б�Ҫ��delete������EntityInfo��, ����NewRecord����...���෴��ע��һ��Ҫ��OldRecord�а�������ɾ����ԭ��¼";
                                goto ERROR1;
                            }
                            strLockBarcode = strOldBarcode;
                        }


                        // ����
                        if (String.IsNullOrEmpty(strLockBarcode) == false)
                            this.EntityLocks.LockForWrite(strLockBarcode);

#if NO
                        // 2014/1/10
                        // ���������
                        if ((info.Action == "new"
        || info.Action == "change"
        || info.Action == "move")       // delete���������
    && String.IsNullOrEmpty(strNewBarcode) == true)
                        {
                            if (this.AcceptBlankItemBarcode == false)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "������Ų���Ϊ��";
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }
#endif
                        if ((info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "move")       // delete������У���¼
                            && bNoCheckDup == false)
                        {
                            nRet = this.DoVerifyItemFunction(
                                sessioninfo,
                                strAction,
                                domNewRec,
                                out strError);
                            if (nRet != 0)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // ���в�����Ų���
                        // TODO: ���ص�ʱ��Ҫע�⣬�����������Ϊ��move�����������������info.OldRecPath�صģ���Ϊ��������ɾ��
                        if (/*nRet == 1   // �¾�����Ų��ȣ��Ų��ء����������������Ч�ʡ�BUG!!! ����������ɿ�����Ϊǰ�˷�����oldrecord��������
                            &&*/ (info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "move")       // delete����������
                            && String.IsNullOrEmpty(strNewBarcode) == false
                            && bNoCheckDup == false)    // 2008/10/6 new add
                        {
#if NO
                            // ��֤�����
                            if (this.VerifyBarcode == true)
                            {
                                // return:
                                //	0	invalid barcode
                                //	1	is valid reader barcode
                                //	2	is valid item barcode
                                int nResultValue = 0;

                                // return:
                                //      -2  not found script
                                //      -1  ����
                                //      0   �ɹ�
                                nRet = this.DoVerifyBarcodeScriptFunction(
                                    sessioninfo.LibraryCodeList,
                                    strNewBarcode,
                                    out nResultValue,
                                    out strError);
                                if (nRet == -2 || nRet == -1 || nResultValue != 2)
                                {
                                    if (nRet == -2)
                                        strError = "library.xml ��û�������������֤�������޷������������֤";
                                    else if (nRet == -1)
                                    {
                                        strError = "��֤������ŵĹ����г���"
                                           + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                                    }
                                    else if (nResultValue != 2)
                                    {
                                        strError = "����� '" + strNewBarcode + "' ����֤���ֲ���һ���Ϸ��Ĳ������"
                                           + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                                    }

                                    EntityInfo error = new EntityInfo(info);
                                    error.ErrorInfo = strError;
                                    error.ErrorCode = ErrorCodeValue.CommonError;
                                    ErrorInfos.Add(error);
                                    continue;
                                }

                            }
#endif


                            List<string> aPath = null;
                            // ���ݲ�����Ŷ�ʵ�����в���
                            // ������ֻ�������, ������ü�¼��
                            // return:
                            //      -1  error
                            //      ����    ���м�¼����(������nMax�涨�ļ���)
                            nRet = SearchItemRecDup(
                                // sessioninfo.Channels,
                                channel,
                                strNewBarcode,
                                100,
                                out aPath,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            bool bDup = false;
                            if (nRet == 0)
                            {
                                bDup = false;
                            }
                            else if (nRet == 1) // ����һ��
                            {
                                Debug.Assert(aPath.Count == 1, "");

                                if (info.Action == "new")
                                {
                                    if (aPath[0] == info.NewRecPath) // �������Լ�
                                        bDup = false;
                                    else
                                        bDup = true;// ��ļ�¼���Ѿ�ʹ������������

                                }
                                else if (info.Action == "change")
                                {
                                    Debug.Assert(info.NewRecPath == info.OldRecPath, "����������Ϊchangeʱ��info.NewRecPathӦ����info.OldRecPath��ͬ");
                                    if (aPath[0] == info.OldRecPath) // �������Լ�
                                        bDup = false;
                                    else
                                        bDup = true;// ��ļ�¼���Ѿ�ʹ������������
                                }
                                else if (info.Action == "move")
                                {
                                    if (aPath[0] == info.OldRecPath) // ������Դ��¼
                                        bDup = false;
                                    else
                                        bDup = true;// ��ļ�¼���Ѿ�ʹ������������
                                }
                                else
                                {
                                    Debug.Assert(false, "���ﲻ���ܳ��ֵ�info.Actionֵ '" + info.Action + "'");
                                }


                            } // end of if (nRet == 1)
                            else
                            {
                                Debug.Assert(nRet > 1, "");
                                bDup = true;

                                // ��Ϊmove����������Ŀ��λ�ô��ڼ�¼����������Ͳ��ٷ���������
                                // �������move��������Ŀ��λ�ô��ڼ�¼����������Ҫ�жϣ�����Դ����Ŀ��λ�÷���������أ��������ء�
                            }

                            // ����
                            if (bDup == true)
                            {
                                /*
                                string[] pathlist = new string[aPath.Count];
                                aPath.CopyTo(pathlist);
                                 * */

                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "����� '" + strNewBarcode + "' �Ѿ������в��¼ʹ����: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }
                    }

                    // ׼����־DOM
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "setEntity");

                    // ����һ������
                    if (info.Action == "new")
                    {
                        // ����¼�¼��·���е�id�����Ƿ���ȷ
                        // �������֣�ǰ���Ѿ�ͳһ������
                        strError = "";

                        if (String.IsNullOrEmpty(info.NewRecPath) == true)
                        {
                            info.NewRecPath = strItemDbName + "/?";
                        }
                        else
                        {

                            string strID = ResPath.GetRecordId(info.NewRecPath);
                            if (String.IsNullOrEmpty(strID) == true)
                            {
                                strError = "RecPath��id����Ӧ��Ϊ'?'";
                            }

                            if (strError != "")
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // ������ʺϱ�����²��¼
                        // ��Ҫ��Ϊ�˰Ѵ��ӹ��ļ�¼�У����ܳ��ֵ����ڡ���ͨ��Ϣ�����ֶ�ȥ����������ְ�ȫ������
                        // TODO: ���strNewXml�г�������ͨ�ֶΣ��Ƿ���Ҫ����ǰ�ˣ�����ֱ�ӱ�����Ϊ������������ǰ�˵�ע�⣬����ǰ����Ϊ�Լ�ͨ���´���ʵ���¼�ӽ軹��Ϣ���ɹ����ˡ�
                        // ��Ȼ�����������Ҳ������Ҫ��ǰ���Լ��е������
                        string strNewXml = "";
                        if (bForce == false)
                        {
                            nRet = BuildNewEntityRecord(info.NewRecord,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }

                            // 2010/4/8
                            XmlDocument temp = new XmlDocument();
                            temp.LoadXml(strNewXml);
                            if (bForce == false && bNoOperations == false)
                            {
                                // ע��ǿ�ƴ�����¼��ʱ�򣬲�Ҫ����<operations>���������
                                nRet = SetOperation(
                                    ref temp,
                                    "create",
                                    sessioninfo.UserID,
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    EntityInfo error = new EntityInfo(info);
                                    error.ErrorInfo = strError;
                                    error.ErrorCode = ErrorCodeValue.CommonError;
                                    ErrorInfos.Add(error);
                                    continue;
                                }
                            }
                            strNewXml = temp.DocumentElement.OuterXml;
                        }
                        else
                        {
                            // 2008/5/29 new add
                            strNewXml = info.NewRecord;
                        }

                        string strLibraryCode = "";

                        // ע�⣺������ȫ���û���ҲҪ�ú��� CheckItemLibraryCode() ��ùݴ���

                        // �ֹ��û�ֻ�ܱ���ݲصص�Ϊ�Լ���Ͻ��Χ�Ĳ��¼
                        // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
                        // return:
                        //      -1  �����̳���
                        //      0   ����Ҫ��
                        //      1   ������Ҫ��
                        nRet = CheckItemLibraryCode(strNewXml,
                            sessioninfo.LibraryCodeList,
                            out strLibraryCode,
                            out strError);
                        if (nRet == -1)
                        {
                            EntityInfo error = new EntityInfo(info);
                            error.ErrorInfo = "���ֹݴ���ʱ����: " + strError;
                            error.ErrorCode = ErrorCodeValue.CommonError;
                            ErrorInfos.Add(error);
                            domOperLog = null;  // ��ʾ����д����־
                            continue;
                        } 
                        if (sessioninfo.GlobalUser == false)
                        {
                            if (nRet != 0)
                            {
                                EntityInfo error = new EntityInfo(info);
                                /*
                                if (nRet == -1)
                                    error.ErrorInfo = "���ֹݴ���ʱ����: " + strError;
                                else */
                                    error.ErrorInfo = "���������Ĳ��¼�����еĹݲصص㲻����Ҫ��: " + strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                domOperLog = null;  // ��ʾ����д����־
                                continue;
                            }
                        }

                        // 2014/7/3
                        if (this.VerifyBookType == true)
                        {
                            string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                            if (String.IsNullOrEmpty(strEntityDbName) == true)
                            {
                                strError = "��·�� '" + info.NewRecPath + "' �л�����ݿ���ʱʧ��";
                                goto ERROR1;
                            }

                            XmlDocument domTemp = new XmlDocument();
                            domTemp.LoadXml(strNewXml);

                            // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
                            // parameters:
                            // return:
                            //      -1  �����̳���
                            //      0   ����Ҫ��
                            //      1   ������Ҫ��
                            nRet = CheckItemBookType(domTemp,
                                strEntityDbName,
                                out strError);
                            if (nRet == -1 || nRet == 1)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "���������Ĳ��¼�����е�ͼ�����Ͳ�����Ҫ��: " + strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                domOperLog = null;  // ��ʾ����д����־
                                continue;
                            }
                        }


                        lRet = channel.DoSaveTextRes(info.NewRecPath,
                            strNewXml,
                            false,   // include preamble?
                            "content",
                            info.OldTimestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            EntityInfo error = new EntityInfo(info);
                            error.NewTimestamp = output_timestamp;
                            error.ErrorInfo = "�����¼�¼�Ĳ�����������:" + strError;
                            error.ErrorCode = channel.OriginErrorCode;
                            ErrorInfos.Add(error);

                            domOperLog = null;  // ��ʾ����д����־
                        }
                        else // �ɹ�
                        {
                            DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // �����ڵĹݴ���

                            DomUtil.SetElementText(domOperLog.DocumentElement, "action", "new");
                            if (String.IsNullOrEmpty(strStyle) == false)
                                DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                            // ������<oldRecord>Ԫ��

                            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement, 
                                "record", strNewXml);
                            DomUtil.SetAttr(node, "recPath", strOutputPath);

                            // �¼�¼����ɹ�����Ҫ������ϢԪ�ء���Ϊ��Ҫ�����µ�ʱ�����ʵ�ʱ���ļ�¼·��

                            EntityInfo error = new EntityInfo(info);
                            error.NewRecPath = strOutputPath;

                            error.NewRecord = strNewXml;    // ����������ļ�¼���������б仯, �����Ҫ���ظ�ǰ��
                            error.NewTimestamp = output_timestamp;

                            error.ErrorInfo = "�����¼�¼�Ĳ����ɹ���NewTimeStamp�з������µ�ʱ���, RecPath�з�����ʵ�ʴ���ļ�¼·����";
                            error.ErrorCode = ErrorCodeValue.NoError;
                            ErrorInfos.Add(error);
                        }
                    }
                    else if (info.Action == "change")
                    {
                        // ִ��SetEntities API�е�"change"����
                        nRet = DoEntityOperChange(
                            bForce,
                            strStyle,
                            sessioninfo,
                            channel,
                            info,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // ʧ��
                            domOperLog = null;  // ��ʾ����д����־
                        }
                    }
                    else if (info.Action == "move")
                    {
                        // ִ��SetEntities API�е�"move"����
                        nRet = DoEntityOperMove(
                            strStyle,
                            sessioninfo,
                            channel,
                            info,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // ʧ��
                            domOperLog = null;  // ��ʾ����д����־
                        }
                    }
                    else if (info.Action == "delete")
                    {
                        // ɾ�����¼�Ĳ���
                        nRet = DoEntityOperDelete(
                            sessioninfo,
                            bForce,
                            strStyle,
                            channel,
                            info,
                            strOldBarcode,
                            strNewBarcode,
                            domOldRec,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // ʧ��
                            domOperLog = null;  // ��ʾ����д����־
                        }
                    }
                    else
                    {
                        // ��֧�ֵ�����
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = "��֧�ֵĲ������� '" + info.Action + "'";
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                    }

                    // д����־
                    if (domOperLog != null
                        && bNoEventLog == false)    // 2008/10/6 new add
                    {
                        string strOperTime = this.Clock.GetClock();
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                            sessioninfo.UserID);   // ������
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                            strOperTime);   // ����ʱ��

                        nRet = this.OperLog.WriteOperLog(domOperLog,
                            sessioninfo.ClientAddress,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "SetEntities() API д����־ʱ��������: " + strError;
                            goto ERROR1;
                        }
                    }
                }
                finally
                {
                    if (String.IsNullOrEmpty(strLockBarcode) == false)
                        this.EntityLocks.UnlockForWrite(strLockBarcode);
                }
            }

            // ���Ƶ������
            errorinfos = new EntityInfo[ErrorInfos.Count];
            for (int i = 0; i < ErrorInfos.Count; i++)
            {
                errorinfos[i] = ErrorInfos[i];
            }

            result.Value = ErrorInfos.Count;  // ������Ϣ������

            return result;
        ERROR1:
            // ����ı����ǱȽ����صĴ�������������в��ֵ��������Ĵ����������ﱨ������ͨ�����ش�����Ϣ����ķ�ʽ������
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        #region SetEntities() �¼�����

        // ��װ��汾
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        int CheckItemLibraryCode(string strXml,
            string strLibraryCodeList,
            out string strError)
        {
            string strLibraryCode = "";
            return CheckItemLibraryCode(strXml,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
        // parameters:
        //      strLibraryCodeList  ��ǰ�û���Ͻ�Ĺݴ����б�
        //      strLibraryCode  [out]���¼�еĹݴ���
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        int CheckItemLibraryCode(string strXml,
            string strLibraryCodeList,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";

            /*
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 0;
             * */

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "���¼XMLװ��XMLDOMʱ�����: " + ex.Message;
                return -1;
            }

            return CheckItemLibraryCode(dom,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // ��װ��汾
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        int CheckItemLibraryCode(XmlDocument dom,
            string strLibraryCodeList,
            out string strError)
        {
            string strLibraryCode = "";
            return CheckItemLibraryCode(dom,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
        // parameters:
        //      strLibraryCodeList  ��ǰ�û���Ͻ�Ĺݴ����б�
        //      strLibraryCode  [out]���¼�еĹݴ���
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        public int CheckItemLibraryCode(XmlDocument dom,
            string strLibraryCodeList,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";


            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");

#if NO
            // ȥ�� #xxx, ����
            if (strLocation.IndexOf("#") != -1)
            {
                string[] parts = strLocation.Split(new char[] { ',' });
                bool bFound = false;
                foreach (string s in parts)
                {
                    string strText = s.Trim();
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    if (strText[0] == '#')
                        continue;
                    strLocation = strText;
                    break;
                }
                if (bFound == false)
                    strLocation = "";
            }
#endif
            strLocation = StringUtil.GetPureLocationString(strLocation);

            string strPureName = "";

            // ����������
            ParseCalendarName(strLocation,
        out strLibraryCode,
        out strPureName);

            // ������ȫ���û���ҲҪ���õ��ݴ���������ܷ���
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 0;

            if (string.IsNullOrEmpty(strLibraryCode) == true)
                goto NOTMATCH;


            if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == true)
            {
                // �ڹ�Ͻ��Χ��
                return 0;
            }

        NOTMATCH:
            strError = "�ݲصص� '" + strLocation + "' ���� '" + strLibraryCodeList + "' ��Ͻ��Χ��";
            return 1;
        }

        // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
        // parameters:
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        public int CheckItemBookType(XmlDocument dom,
            string strItemDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            string strLibraryCode = "";

            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);

            string strPureName = "";
            // ����������
            ParseCalendarName(strLocation,
        out strLibraryCode,
        out strPureName);

            List<string> values = null;

            // ��̽ ��Ŀ����

            string strBiblioDbName = "";
            // ����ʵ�����, �ҵ���Ӧ����Ŀ����
            // ע�⣬����1��ʱ��strBiblioDbNameҲ�п���Ϊ��
            // return:
            //      -1  ����
            //      0   û���ҵ�
            //      1   �ҵ�
            nRet = GetBiblioDbNameByItemDbName(strItemDbName,
            out strBiblioDbName,
            out strError);
            if (nRet == 0 || nRet == -1)
            {
                strError = "����ʵ����� '" + strItemDbName + "' �л����Ŀ����ʱʧ��";
                return -1;
            }

            values = GetOneLibraryValueTable(
                strLibraryCode,
                "bookType",
                strBiblioDbName);
            if (values.Count > 0)
                goto FOUND;

            // ��̽ ʵ�����

            // ���һ��ͼ��ݴ����µ�ֵ�б�
            // parameters:
            //      strLibraryCode  �ݴ���
            //      strTableName    ���������Ϊ�գ���ʾ����name����ֵ��ƥ��
            //      strDbName   ���ݿ��������Ϊ�գ���ʾ����dbname����ֵ��ƥ�䡣
            values = GetOneLibraryValueTable(
                strLibraryCode,
                "bookType",
                strItemDbName);
            if (values.Count > 0)
                goto FOUND;


            // ��̽��ʹ�����ݿ���

            values = GetOneLibraryValueTable(
    strLibraryCode,
    "bookType",
    "");
            if (values.Count > 0)
                goto FOUND;

            return 0;   // ��Ϊû��ֵ�б�ʲôֵ������

            FOUND:
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
    "bookType");

            if (IsInList(strBookType, values) == true)
                return 0;

            GetPureValue(ref values);
            strError = "ͼ������ '"+strBookType+"' ���ǺϷ���ֵ��ӦΪ '"+StringUtil.MakePathList(values)+"' ֮һ";
            return 1;
        }

        static void GetPureValue(ref List<string> values)
        {
            for(int i = 0;i<values.Count; i++)
            {
                string strText = values[i].Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                values[i] = GetPureSeletedValue(strText);
            }
        }

        static bool IsInList(string strBookType, List<string> values)
        {
            foreach (string strValue in values)
            {
                string strText = strValue.Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                strText = GetPureSeletedValue(strText);
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                if (strBookType == strText)
                    return true;
            }

            return false;
        }

#if NO
        static bool IsInList(string strBookType, List<string> values)
        {
            foreach (string strValue in values)
            {
                string[] parts = strValue.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in parts)
                {
                    string strText = s.Trim();
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    strText = GetPureSeletedValue(strText);
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    if (strBookType == strText)
                        return true;
                }
            }

            return false;
        }
#endif

        /// <summary>
        /// ���˵�������� {} �ַ�
        /// </summary>
        /// <param name="strText">�����˵��ַ���</param>
        /// <returns>���˺���ַ���</returns>
        static string GetPureSeletedValue(string strText)
        {
            for (; ; )
            {
                int nRet = strText.IndexOf("{");
                if (nRet == -1)
                    return strText;
                int nStart = nRet;
                nRet = strText.IndexOf("}", nStart + 1);
                if (nRet == -1)
                    return strText;
                int nEnd = nRet;
                strText = strText.Remove(nStart, nEnd - nStart + 1).Trim();
            }
        }

        /*
         * ��CompareTwoBarcode�����
        // ��EntityInfo�е�OldRecord��NewRecord�а���������Ž��бȽ�, �����Ƿ����˱仯(��������Ҫ����)
        // parameters:
        //      strOldBarcode   ˳�㷵�ؾɼ�¼�е������
        //      strNewBarcode   ˳�㷵���¼�¼�е������
        // return:
        //      -1  ����
        //      0   ���
        //      1   �����
        static int CompareEntityBarcode(EntityInfo info,
            out string strOldBarcode,
            out string strNewBarcode,
            out string strError)
        {
            strError = "";

            strOldBarcode = "";
            strNewBarcode = "";

            if (String.IsNullOrEmpty(info.OldRecord) == false)
            {
                XmlDocument old_dom = new XmlDocument();
                try
                {
                    old_dom.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "װ��info.OldRecord��DOMʱ��������: " + ex.Message;
                    return -1;
                }

                strOldBarcode = DomUtil.GetElementText(old_dom.DocumentElement, "barcode");
            }

            if (String.IsNullOrEmpty(info.NewRecord) == false)
            {
                XmlDocument new_dom = new XmlDocument();
                try
                {
                    new_dom.LoadXml(info.NewRecord);
                }
                catch (Exception ex)
                {
                    strError = "װ��info.NewRecord��DOMʱ��������: " + ex.Message;
                    return -1;
                }

                strNewBarcode = DomUtil.GetElementText(new_dom.DocumentElement, "barcode");
            }

            if (strOldBarcode != strNewBarcode)
                return 1;   // �����

            return 0;   // ���
        }
         */

        // ������ʺϱ�����²��¼
        // ��Ҫ��Ϊ�˰Ѵ��ӹ��ļ�¼�У����ܳ��ֵ����ڡ���ͨ��Ϣ�����ֶ�ȥ����������ְ�ȫ������
        static int BuildNewEntityRecord(string strOriginXml,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strOriginXml);
            }
            catch (Exception ex)
            {
                strError = "װ��strOriginXml��DOMʱ����: " + ex.Message;
                return -1;
            }

            // ��ͨԪ�����б�
            string[] element_names = new string[] {
                "borrower",
                "borrowDate",
                "borrowPeriod",
                "borrowHistory",
            };

            for (int i = 0; i < element_names.Length; i++)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    element_names[i], "");
            }

            strXml = dom.OuterXml;

            return 0;
        }

        // ɾ�����¼�Ĳ���
        int DoEntityOperDelete(
            SessionInfo sessioninfo,
            bool bForce,
            string strStyle,
            RmsChannel channel,
            EntityInfo info,
            string strOldBarcode,
            string strNewBarcode,   // TODO: �������Ƿ���Էϳ�?
            XmlDocument domOldRec,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            // 2008/6/24 new add
            if (String.IsNullOrEmpty(info.NewRecPath) == false)
            {
                if (info.NewRecPath != info.OldRecPath)
                {
                    strError = "actionΪdeleteʱ, ���info.NewRecPath���գ��������ݱ����info.OldRecPathһ�¡�(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                    return -1;
                }
            }
            else
            {
                info.NewRecPath = info.OldRecPath;
            }


            // �����¼·��Ϊ��, ���Ȼ�ü�¼·��
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                List<string> aPath = null;

                if (String.IsNullOrEmpty(strOldBarcode) == true)
                {
                    strError = "info.OldRecord�е�<barcode>Ԫ���еĲ�����ţ���info.RecPath����ֵ������ͬʱΪ�ա�";
                    goto ERROR1;
                }

                // ������ֻ�������, ������ü�¼��
                // return:
                //      -1  error
                //      ����    ���м�¼����(������nMax�涨�ļ���)
                nRet = this.SearchItemRecDup(
                    // sessioninfo.Channels,
                    channel,
                    strOldBarcode,
                    100,
                    out aPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ɾ������������Ų��ؽ׶η�������:" + strError;
                    goto ERROR1;
                }

                if (nRet == 0)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "�������Ϊ '" + strOldBarcode + "' �Ĳ��¼�Ѳ�����";
                    error.ErrorCode = ErrorCodeValue.NotFound;
                    ErrorInfos.Add(error);
                    return -1;
                }

                if (nRet > 1)
                {
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                     * */

                    // ��ɾ�������У������ظ����Ǻ�ƽ�������顣ֻҪ
                    // info.OldRecPath�ܹ�������ָ��Ҫɾ������һ�����Ϳ���ִ��ɾ��
                    if (String.IsNullOrEmpty(info.OldRecPath) == false)
                    {
                        if (aPath.IndexOf(info.OldRecPath) == -1)
                        {
                            strError = "����� '" + strOldBarcode + "' �Ѿ������ж������¼ʹ����: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'������������info.OldRecPath��ָ��·�� '" + info.OldRecPath + "'��ɾ������ʧ�ܡ�";
                            goto ERROR1;
                        }
                        info.NewRecPath = info.OldRecPath;
                    }
                    else
                    {
                        strError = "����� '" + strOldBarcode + "' �Ѿ������ж������¼ʹ����: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'����û��ָ��info.OldRecPath������£��޷���λ��ɾ����";
                        goto ERROR1;
                    }
                }
                else
                {
                    Debug.Assert(nRet == 1, "");

                    info.NewRecPath = aPath[0];
                    // strEntityDbName = ResPath.GetDbName(strRecPath);
                }
            }

            Debug.Assert(String.IsNullOrEmpty(info.NewRecPath) == false, "");
            // Debug.Assert(strEntityDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";
            string strExistingXml = "";

        REDOLOAD:

            // �ȶ������ݿ��д�λ�õ����м�¼
            lRet = channel.GetRes(info.NewRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "�������Ϊ '" + strOldBarcode + "' �Ĳ��¼ '" + info.NewRecPath + "' �Ѳ�����";
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "ɾ��������������, �ڶ���ԭ�м�¼ '" + info.NewRecPath + "' �׶�:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // �Ѽ�¼װ��DOM
            XmlDocument domExist = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistingXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            // �۲��Ѿ����ڵļ�¼�У���������Ƿ��strOldBarcodeһ��
            if (String.IsNullOrEmpty(strOldBarcode) == false)
            {
                string strExistingBarcode = DomUtil.GetElementText(domExist.DocumentElement, "barcode");
                if (strExistingBarcode != strOldBarcode)
                {
                    strError = "·��Ϊ '" + info.NewRecPath + "' �Ĳ��¼��<barcode>Ԫ���еĲ������ '" + strExistingBarcode + "' ��strOldXml��<barcode>Ԫ���еĲ������ '" + strOldBarcode + "' ��һ�¡��ܾ�ɾ��(�������ɾ���������ɲ�����ɾ���˱�Ĳ��¼��Σ��)��";
                    goto ERROR1;
                }
            }


            if (bForce == false)
            {
                // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                string strDetail = "";
                bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                    out strDetail);

                if (bHasCirculationInfo == true)
                {
                    strError = "��ɾ���Ĳ��¼ '" + info.NewRecPath + "' �а�������ͨ��Ϣ(" + strDetail + ")������ɾ����";
                    goto ERROR1;
                }
            }

            // �Ƚ�ʱ���
            // �۲�ʱ����Ƿ����仯
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 2008/5/29 new add
                if (bForce == true)
                {
                    error = new EntityInfo(info);
                    error.NewTimestamp = exist_timestamp;   // ��ǰ��֪�����м�¼ʵ���Ϸ������仯
                    error.ErrorInfo = "���ݿ��м���ɾ���Ĳ��¼�Ѿ������˱仯��������װ�ء���ϸ�˶Ժ�����ɾ����";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // ���ǰ�˸����˾ɼ�¼�����кͿ��м�¼���бȽϵĻ���
                if (String.IsNullOrEmpty(info.OldRecord) == false)
                {
                    // �Ƚ�������¼, �����Ͳ�Ҫ����Ϣ�йص��ֶ��Ƿ����˱仯
                    // return:
                    //      0   û�б仯
                    //      1   �б仯
                    nRet = IsRegisterInfoChanged(domExist,
                        domOldRec);
                    if (nRet == 1)
                    {

                        error = new EntityInfo(info);
                        error.NewTimestamp = exist_timestamp;   // ��ǰ��֪�����м�¼ʵ���Ϸ������仯
                        error.ErrorInfo = "���ݿ��м���ɾ���Ĳ��¼�Ѿ������˱仯��������װ�ء���ϸ�˶Ժ�����ɾ����";
                        error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }

                info.OldTimestamp = exist_timestamp;
                info.NewTimestamp = exist_timestamp;
            }

            // ֻ��orderȨ�޵����
            if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
            {
                // 2009/11/26 changed
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "��·�� '" + info.NewRecPath + "' �л�����ݿ���ʱʧ��";
                    goto ERROR1;
                }

                string strBiblioDbName = "";

                // ����ʵ�����, �ҵ���Ӧ����Ŀ����
                // ע�⣬����1��ʱ��strBiblioDbNameҲ�п���Ϊ��
                // return:
                //      -1  ����
                //      0   û���ҵ�
                //      1   �ҵ�
                nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                out strBiblioDbName,
                out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "����ʵ����� '" + strEntityDbName + "' �л����Ŀ����ʱʧ��";
                    goto ERROR1;
                }

                // BUG !!! string strBiblioDbName = ResPath.GetDbName(info.NewRecPath);

                // �ǹ�����
                if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                {
                    // �ǹ����⡣Ҫ��<state>�������ӹ��С�
                    string strState = DomUtil.GetElementText(domExist.DocumentElement,
                        "state");
                    if (IncludeStateProcessing(strState) == false)
                    {
                        strError = "��ǰ�ʻ�ֻ��orderȨ�޶�û��setiteminfo(��setentities)Ȩ�ޣ�������delete����ɾ�������ڷǹ�����ġ�״̬���������ӹ��С���ʵ���¼ '" + info.NewRecPath + "'";
                        goto ERROR1;    // TODO: ��η���AccessDenied��������?
                    }
                }
            }

            string strLibraryCode = "";
            // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
            // return:
            //      -1  �����̳���
            //      0   ����Ҫ��
            //      1   ������Ҫ��
            nRet = CheckItemLibraryCode(domExist,
                        sessioninfo.LibraryCodeList,
                        out strLibraryCode,
                        out strError);
            if (nRet == -1)
                goto ERROR1;



            // ���ɼ�¼�Ƿ����ڹ�Ͻ��Χ
            if (sessioninfo.GlobalUser == false)
            {
                if (nRet != 0)
                {
                    strError = "������ɾ���Ĳ��¼ '" + info.NewRecPath + "' ��ݲصص㲻����Ҫ��: " + strError;
                    goto ERROR1;
                }
            }

            byte[] output_timestamp = null;

            lRet = channel.DoDeleteRes(info.NewRecPath,
                info.OldTimestamp,
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "����ɾ��������ʱ�����ͻ, ����10��������Ȼʧ��";
                        goto ERROR1;
                    }
                    // ����ʱ�����ƥ��
                    // �ظ�������ȡ�Ѵ��ڼ�¼\�ȽϵĹ���
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.ErrorInfo = "ɾ��������������:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else
            {
                // �ɹ�
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // �����ڵĹݴ���

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");
                if (String.IsNullOrEmpty(strStyle) == false)
                    DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                // ������<record>Ԫ��

                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistingXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);


                // ���ɾ���ɹ����򲻱�Ҫ�������з��ر�ʾ�ɹ�����ϢԪ����
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // TODO: ����ⲿû�н��в��أ�������Ҫ������ݿ����Ѿ����ڵļ�¼������źͼ���������Ƿ����仯�������仯����Ҫ׷�Ӳ���
        // ִ��SetEntities API�е�"change"����
        // 1) �����ɹ���, NewRecord����ʵ�ʱ�����¼�¼��NewTimeStampΪ�µ�ʱ���
        // 2) �������TimeStampMismatch����OldRecord���п��з����仯��ġ�ԭ��¼����OldTimeStamp����ʱ���
        // return:
        //      -1  ����
        //      0   �ɹ�
        int DoEntityOperChange(
            bool bForce,
            string strStyle,
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath��ָ�ļ�¼�Ƿ����?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // ���һ��·��
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                strError = "info.NewRecPath�е�·������Ϊ��";
                goto ERROR1;
            }

            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?")
            {
                strError = "info.NewRecPath·�� '" + strTargetRecId + "' �м�¼ID���ֲ���Ϊ'?'";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strTargetRecId) == true)
            {
                strError = "info.NewRecPath·�� '" + strTargetRecId + "' �м�¼ID���ֲ���Ϊ��";
                goto ERROR1;
            }

            if (info.OldRecPath != info.NewRecPath)
            {
                strError = "��actionΪ\"change\"ʱ��info.NewRecordPath·�� '" + info.NewRecPath + "' ��info.OldRecPath '" +info.OldRecPath+ "' ������ͬ";
                goto ERROR1;
            }

            bool bNoOperations = false; // �Ƿ�Ϊ��Ҫ����<operations>����
            if (StringUtil.IsInList("nooperations", strStyle) == true)
            {
                bNoOperations = true;
            }

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";


            // �ȶ������ݿ��м�������λ�õ����м�¼

        REDOLOAD:

            lRet = channel.GetRes(info.NewRecPath,
                out strExistXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    // �����¼������, ����һ���յļ�¼
                    bExist = false;
                    strExistXml = "<root />";
                    exist_timestamp = null;
                    strOutputPath = info.NewRecPath;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "���������������, �ڶ���ԭ�м�¼�׶�:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }


            // ��������¼װ��DOM

            XmlDocument domExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecordװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            string strSourceLibraryCode = "";

            if (bExist == true)
            {
                // ֻ��orderȨ�޵����
                if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                    && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                    && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
                {
                    // 2009/11/26 changed
                    string strEntityDbName = ResPath.GetDbName(info.OldRecPath);
                    if (String.IsNullOrEmpty(strEntityDbName) == true)
                    {
                        strError = "��·�� '" + info.OldRecPath + "' �л�����ݿ���ʱʧ��";
                        goto ERROR1;
                    }

                    string strBiblioDbName = "";

                    // ����ʵ�����, �ҵ���Ӧ����Ŀ����
                    // ע�⣬����1��ʱ��strBiblioDbNameҲ�п���Ϊ��
                    // return:
                    //      -1  ����
                    //      0   û���ҵ�
                    //      1   �ҵ�
                    nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                    out strBiblioDbName,
                    out strError);
                    if (nRet == 0 || nRet == -1)
                    {
                        strError = "����ʵ����� '" + strEntityDbName + "' �л����Ŀ����ʱʧ��";
                        goto ERROR1;
                    }
                    // BUG !!! string strBiblioDbName = ResPath.GetDbName(info.OldRecPath);

                    // �ǹ�����
                    if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                    {
                        // �ǹ����⡣Ҫ��<state>�������ӹ��С�
                        string strState = DomUtil.GetElementText(domExist.DocumentElement,
                            "state");
                        if (IncludeStateProcessing(strState) == false)
                        {
                            strError = "��ǰ�ʻ�ֻ��orderȨ�޶�û��setiteminfo(��setentities)Ȩ�ޣ�������change�����޸Ĵ����ڷǹ�����ġ�״̬���������ӹ��С���ʵ���¼ '" + info.OldRecPath + "'";
                            goto ERROR1;
                        }
                    }
                }

                // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
                // return:
                //      -1  �����̳���
                //      0   ����Ҫ��
                //      1   ������Ҫ��
                nRet = CheckItemLibraryCode(domExist,
                            sessioninfo.LibraryCodeList,
                            out strSourceLibraryCode,
                            out strError);
                if (nRet == -1)
                    goto ERROR1;



                // ���ɼ�¼�Ƿ����ڹ�Ͻ��Χ
                if (sessioninfo.GlobalUser == false)
                {
                    if (nRet != 0)
                    {
                        strError = "�������޸ĵĲ��¼ '" + info.NewRecPath + "' ��ݲصص㲻����Ҫ��: " + strError;
                        goto ERROR1;
                    }
                }
            }

            string strOldBarcode = "";
            string strNewBarcode = "";

            if (bExist == true) // 2009/3/9 new add
            {
                // �Ƚ��¾ɼ�¼��������Ƿ��иı�
                // return:
                //      -1  ����
                //      0   ���
                //      1   �����
                nRet = CompareTwoBarcode(domExist,
                    domNew,
                    out strOldBarcode,
                    out strNewBarcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                bool bHasCirculationInfo = false;   // ���¼�����Ƿ�����ͨ��Ϣ
                // bool bDetectCiculationInfo = false; // �Ƿ��Ѿ�̽������¼�е���ͨ��Ϣ
                string strDetailInfo = "";  // ���ڲ��¼�����Ƿ�����ͨ��Ϣ����ϸ��ʾ����
                // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                    out strDetailInfo);
                // bDetectCiculationInfo = true;


                if (nRet == 1)  // ��������иı�
                {
                    if (bHasCirculationInfo == true
                        && bForce == false)
                    {
                        // TODO: �ɷ���������ͬʱ�޸����������ѽ��Ķ��߼�¼�޸�����?
                        // ֵ��ע�������μ�¼��������־��������ν���recover������
                        strError = "�޸Ĳ������ܾ�������¼ '" + info.NewRecPath + "' �а�������ͨ��Ϣ(" + strDetailInfo + ")�������޸���ʱ�������Ԫ�����ݲ��ܸı䡣(��ǰ������� '" + strOldBarcode + "'����ͼ�޸�Ϊ����� '" + strNewBarcode + "')";
                        goto ERROR1;
                    }
                }

                if (bHasCirculationInfo == true)
                {
                    string strOldLocation = "";
                    string strNewLocation = "";
                    // �Ƚ��¾ɼ�¼�еĹݲصص��Ƿ��иı�
                    // return:
                    //      -1  ����
                    //      0   ���
                    //      1   �����
                    nRet = CompareTwoField(
                        "location",
                        domExist,
                        domNew,
                        out strOldLocation,
                        out strNewLocation,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)  // �иı�
                    {
                        if (bForce == false)
                        {
                            // ֵ��ע�������μ�¼��������־��������ν���recover������
                            strError = "�޸Ĳ������ܾ�������¼ '" + info.NewRecPath + "' �а�������ͨ��Ϣ(" + strDetailInfo + ")�������޸���ʱ�ݲصص�Ԫ�����ݲ��ܸı䡣(��ǰ�ݲصص� '" + strOldLocation + "'����ͼ�޸�Ϊ�ص� '" + strNewLocation + "')";
                            goto ERROR1;
                        }
                    }
                }

                // �Ƚ��¾ɼ�¼��״̬�Ƿ��иı䣬���������״̬�޸�Ϊ��ע����״̬����Ӧ����ע�⣬����Ҫ���б�Ҫ�ļ��

                string strOldState = "";
                string strNewState = "";

                // parameters:
                //      strOldState   ˳�㷵�ؾɼ�¼�е�״̬�ַ���
                //      strNewState   ˳�㷵���¼�¼�е�״̬�ַ���
                // return:
                //      -1  ����
                //      0   ���
                //      1   �����
                nRet = CompareTwoState(domExist,
                    domNew,
                    out strOldState,
                    out strNewState,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                {
                    if ((strOldState != "ע��" && strOldState != "��ʧ")
                        && (strNewState == "ע��" || strNewState == "��ʧ")
                        && bForce == false)
                    {
#if NO
                        // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                        if (bDetectCiculationInfo == false)
                        {
                            bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                                out strDetailInfo);
                            bDetectCiculationInfo = true;
                        }
#endif

                        if (bHasCirculationInfo == true)
                        {
                            // Debug.Assert(bDetectCiculationInfo == true, "");
                            strError = "ע��(��ʧ)�������ܾ������ⱻע���Ĳ��¼ '" + info.NewRecPath + "' �а�������ͨ��Ϣ(" + strDetailInfo + ")��(��ǰ��״̬ '" + strOldState + "', ��ͼ�޸�Ϊ��״̬ '" + strNewState + "')";
                            goto ERROR1;
                        }
                    }

                    // ����¼�¼״̬û�а������ӹ��С�(���ɼ�¼״̬�����ˡ��ӹ��С�)��Ҫ����ԤԼ���
                    if (bHasCirculationInfo == false
                        && IncludeStateProcessing(strOldState) == true && IncludeStateProcessing(strNewState) == false)
                    {
                        string strReservationReaderBarcode = "";

                        // �쿴����ԤԼ���, �����г�������
                        // TODO: ���Ϊע��������Ҫ֪ͨ�ȴ��ߣ����Ѿ�ע���ˣ������ٵȴ�
                        // return:
                        //      -1  error
                        //      0   û���޸�
                        //      1   ���й��޸�
                        nRet = DoItemReturnReservationCheck(
                            false,
                            ref domNew,
                            out strReservationReaderBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            this.WriteErrorLog("SetEntities()�޸Ĳ��¼ " + info.OldRecPath + " �Ĳ����У����ԤԼ���еĲ���ʧ��(�����޸Ĳ��¼�Ĳ�����һ��ʧ��)����������: " + strError);
                            goto ERROR1;
                        }

                        if (String.IsNullOrEmpty(strReservationReaderBarcode) == false)
                        {
                            List<string> DeletedNotifyRecPaths = null;  // ��ɾ����֪ͨ��¼�����á�
                            // ֪ͨԤԼ����Ĳ���
                            // ���ڶԶ��߿��������ı�������, �������˴˺���
                            // return:
                            //      -1  error
                            //      0   û���ҵ�<request>Ԫ��
                            nRet = DoReservationNotify(
                                channel.Container,
                                strReservationReaderBarcode,
                                true,   // ��Ҫ�����ڼ���
                                strNewBarcode,
                                false,  // ���ڴ��
                                false,  // ����Ҫ���޸ĵ�ǰ���¼����Ϊǰ���Ѿ��޸Ĺ���
                                out DeletedNotifyRecPaths,
                                out strError);
                            if (nRet == -1)
                            {
                                this.WriteErrorLog("SetEntities()�޸Ĳ��¼ " + info.OldRecPath + " �Ĳ����У����ԤԼ���еĲ��������Ѿ��ɹ�, ����ԤԼ����֪ͨ����ʧ��, ԭ��: " + strError);
                            }

                            /*
                            if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "����",
                                "ԤԼ�����",
                                1);
                             * */
                        }

                    }
                    // endif ����¼�¼״̬û�а������ӹ��С�...

                }
            }

            // �۲�ʱ����Ƿ����仯
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // ʱ����������
                // ��Ҫ��info.OldRecord��strExistXml���бȽϣ������Ͳ��¼�йص�Ԫ�أ�Ҫ��Ԫ�أ�ֵ�Ƿ����˱仯��
                // �����ЩҪ��Ԫ�ز�δ�����仯���ͼ������кϲ������Ǳ������

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecordװ�ؽ���DOMʱ��������: " + ex.Message;
                    goto ERROR1;
                }

                if (bForce == false)
                {
                    // �Ƚ�������¼, �����Ͳ��¼�йص��ֶ��Ƿ����˱仯
                    // return:
                    //      0   û�б仯
                    //      1   �б仯
                    nRet = IsRegisterInfoChanged(domOld,
                        domExist);
                }

                if (nRet == 1 || bForce == true) // 2008/5/29 changed
                {
                    error = new EntityInfo(info);
                    // ������Ϣ��, �������޸Ĺ���ԭ��¼����ʱ���
                    error.OldRecord = strExistXml;
                    error.OldTimestamp = exist_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "���������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + info.OldRecPath + "') �ѱ�ɾ����";
                    else
                        error.ErrorInfo = "���������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + info.OldRecPath + "') �ѷ������޸�";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_timestamp��ʱ�Ѿ���ӳ�˿��б��޸ĺ�ļ�¼��ʱ���
            }


            // �ϲ��¾ɼ�¼
            string strNewXml = "";
            if (bForce == false)
            {
                if (bNoOperations == false)
                {
                    // 2010/4/8
                    nRet = SetOperation(
    ref domNew,
    "lastModified",
    sessioninfo.UserID,
    "",
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }


                nRet = MergeTwoEntityXml(domExist,
                    domNew,
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            else
            {
                // 2008/5/29 new add
                strNewXml = domNew.OuterXml;
            }

            string strTargetLibraryCode = "";
            // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
            // return:
            //      -1  �����̳���
            //      0   ����Ҫ��
            //      1   ������Ҫ��
            nRet = CheckItemLibraryCode(strNewXml,
                        sessioninfo.LibraryCodeList,
                        out strTargetLibraryCode,
                        out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2014/7/3
            if (this.VerifyBookType == true)
            {
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "��·�� '" + info.NewRecPath + "' �л�����ݿ���ʱʧ��";
                    goto ERROR1;
                }

                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);

                // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
                // parameters:
                // return:
                //      -1  �����̳���
                //      0   ����Ҫ��
                //      1   ������Ҫ��
                nRet = CheckItemBookType(domTemp,
                    strEntityDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;
            }

            // ����¼�¼�Ƿ����ڹ�Ͻ��Χ
            if (sessioninfo.GlobalUser == false)
            {
                if (nRet != 0)
                {
                    strError = "���¼�������еĹݲصص㲻����Ҫ��: " + strError;
                    goto ERROR1;
                }
            }

            // �����¼�¼
            byte[] output_timestamp = null;
            lRet = channel.DoSaveTextRes(info.NewRecPath,
    strNewXml,
    false,   // include preamble?
    "content",
    exist_timestamp,
    out output_timestamp,
    out strOutputPath,
    out strError);
            if (lRet == -1)
            {

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "�������������ʱ�����ͻ, ����10��������Ȼʧ��";
                        goto ERROR1;
                    }
                    // ����ʱ�����ƥ��
                    // �ظ�������ȡ�Ѵ��ڼ�¼\�ȽϵĹ���
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.ErrorInfo = "���������������:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // �ɹ�
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strSourceLibraryCode + "," + strTargetLibraryCode);    // �����ڵĹݴ���

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");
                if (String.IsNullOrEmpty(strStyle) == false)
                    DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                // �¼�¼
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // �ɼ�¼
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // ����ɹ�����Ҫ������ϢԪ�ء���Ϊ��Ҫ�����µ�ʱ���
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "��������ɹ���NewTimeStamp�з������µ�ʱ�����NewRecord�з�����ʵ�ʱ�����¼�¼(���ܺ��ύ���¼�¼���в���)��";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;

        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // ���ԭ�еĲ�������
        public int ClearOperation(
            ref string strXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML�ַ���װ��DOMʱ��������: " + ex.Message;
                return -1;
            }

            // TODO: �����Ԫ�ؾ���<operations>�أ�
            for (; ; )
            {
                XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
                if (nodeOperations != null)
                    nodeOperations.ParentNode.RemoveChild(nodeOperations);
                else 
                    break;
            }

            strXml = dom.OuterXml;
            return 0;
        }

        // ���û���ˢ��һ����������
        // parameters:
        //      bAppend �Ƿ���׷�ӵķ�ʽ�����µĲ�����Ϣ.���==false����ʾ���һ��ͬstrOperName��ԭ�нڵ�
        //      nMaxCount   <operation>Ԫ�ص������Ŀ.������������Ŀ�����Զ�����ӵڶ�����ʼ�����ɸ�Ԫ�ء���һ��Ԫ��ͨ����create��������Ϣ�������Ᵽ��
        public int SetOperation(
            ref string strXml,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            int nMaxCount,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML�ַ���װ��DOMʱ��������: "+ ex.Message;
                return -1;
            }

            int nRet = SetOperation(
                ref dom,
                strOperName,
                strOperator,
                strComment,
                bAppend,
                nMaxCount,
                out strError);
            if (nRet == -1)
                return -1;

            strXml = dom.OuterXml;
            return nRet;
        }

        // 2011/11/30
        // ��װ��汾���������
        public int SetOperation(
    ref XmlDocument dom,
    string strOperName,
    string strOperator,
    string strComment,
    out string strError)
        {
            return SetOperation(
    ref dom,
    strOperName,
    strOperator,
    strComment,
    false,
    100,
    out strError);
        }

        // ���û���ˢ��һ����������
        // parameters:
        //      bAppend �Ƿ���׷�ӵķ�ʽ�����µĲ�����Ϣ.���==false����ʾ���һ��ͬstrOperName��ԭ�нڵ�
        //      nMaxCount   <operation>Ԫ�ص������Ŀ.������������Ŀ�����Զ�����ӵڶ�����ʼ�����ɸ�Ԫ�ء���һ��Ԫ��ͨ����create��������Ϣ�������Ᵽ��
        public int SetOperation(
            ref XmlDocument dom,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            int nMaxCount,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "dom.DocumentElement == null";
                return -1;
            }

            XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
            if (nodeOperations == null)
            {
                nodeOperations = dom.CreateElement("operations");
                dom.DocumentElement.AppendChild(nodeOperations);
            }

            XmlNode node = nodeOperations.SelectSingleNode("operation[@name='" + strOperName + "']");
            if (node == null || bAppend == true)
            {
                node = dom.CreateElement("operation");
                nodeOperations.AppendChild(node);
                DomUtil.SetAttr(node, "name", strOperName);
            }

            string strTime = this.Clock.GetClock();

            DomUtil.SetAttr(node, "time", strTime);
            DomUtil.SetAttr(node, "operator", strOperator);
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(node, "comment", strComment);

            // ɾ������nMaxCount������<operation>Ԫ��
            XmlNodeList nodes = nodeOperations.SelectNodes("operation");
            if (nodes.Count > nMaxCount)
            {
                for (int i = 0; i < nodes.Count - nMaxCount; i++)
                {
                    if (i + 1 >= nodes.Count)
                        break;
                    XmlNode current = nodes[i+1];
                    current.ParentNode.RemoveChild(current);
                }
            }

            return 0;
        }

        // ִ��SetEntities API�е�"move"����
        // 1) �����ɹ���, NewRecord����ʵ�ʱ�����¼�¼��NewTimeStampΪ�µ�ʱ���
        // 2) �������TimeStampMismatch����OldRecord���п��з����仯��ġ�ԭ��¼����OldTimeStamp����ʱ���
        // return:
        //      -1  ����
        //      0   �ɹ�
        int DoEntityOperMove(
            string strStyle,
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            // int nRedoCount = 0;
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath��ָ�ļ�¼�Ƿ����?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // ���·��
            if (info.OldRecPath == info.NewRecPath)
            {
                strError = "��actionΪ\"move\"ʱ��info.NewRecordPath·�� '" + info.NewRecPath + "' ��info.OldRecPath '" + info.OldRecPath + "' ���벻��ͬ";
                goto ERROR1;
            }

            // ��鼴�����ǵ�Ŀ��λ���ǲ����м�¼������У����������move������
            // ���Ҫ���д�����Ŀ��λ�ü�¼���ܵ�move������ǰ�˿�����ִ��һ��delete������Ȼ����ִ��move������
            // �����涨����Ϊ�˱�����ڸ��ӵ��ж��߼���Ҳ����ǰ�˲�������������ĺ����
            // ��Ϊ�������move���и���Ŀ���¼���ܣ��򱻸��ǵļ�¼��Ԥɾ�����������ڽ�����һ��ע���������Ч�ò����ԣ���ǰ�˲�����Ա׼ȷ�ж���̬���Ժ������(���ҿ�������ע����Ҫ����Ĳ���Ȩ��)������
            bool bAppendStyle = false;  // Ŀ��·���Ƿ�Ϊ׷����̬��
            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                bAppendStyle = true;

            string strOutputPath = "";
            string strMetaData = "";


            if (bAppendStyle == false)
            {
                string strExistTargetXml = "";
                byte[] exist_target_timestamp = null;

                // ��ȡ����Ŀ��λ�õ����м�¼
                lRet = channel.GetRes(info.NewRecPath,
                    out strExistTargetXml,
                    out strMetaData,
                    out exist_target_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        // �����¼������, ˵��������ɸ���̬��
                        /*
                        strExistSourceXml = "<root />";
                        exist_source_timestamp = null;
                        strOutputPath = info.NewRecPath;
                         * */
                    }
                    else
                    {
                        error = new EntityInfo(info);
                        error.ErrorInfo = "�ƶ�������������, �ڶ��뼴�����ǵ�Ŀ��λ�� '" + info.NewRecPath + "' ԭ�м�¼�׶�:" + strError;
                        error.ErrorCode = channel.OriginErrorCode;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }
                else
                {
                    // �����¼���ڣ���Ŀǰ�����������Ĳ���
                    strError = "�ƶ�(move)�������ܾ�����Ϊ�ڼ������ǵ�Ŀ��λ�� '" + info.NewRecPath + "' �Ѿ����ڲ��¼������ɾ��(delete)������¼���ٽ����ƶ�(move)����";
                    goto ERROR1;
                }
            }


            string strExistSourceXml = "";
            byte[] exist_source_timestamp = null;

            // �ȶ������ݿ���Դλ�õ����м�¼
            // REDOLOAD:

            lRet = channel.GetRes(info.OldRecPath,
                out strExistSourceXml,
                out strMetaData,
                out exist_source_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    /*
                    // �����¼������, ����һ���յļ�¼
                    bExist = false;
                    strExistSourceXml = "<root />";
                    exist_source_timestamp = null;
                    strOutputPath = info.NewRecPath;
                     * */
                    // �����������ſ��������صĸ����ã����Բ��÷ſ�
                    strError = "�ƶ�(move)������Դ��¼ '" + info.OldRecPath + "' �����ݿ��в����ڣ������޷������ƶ�������";
                    goto ERROR1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "�ƶ�(move)������������, �ڶ������ԭ��Դ��¼(·����info.OldRecPath) '" + info.OldRecPath + "' �׶�:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // ��������¼װ��DOM

            XmlDocument domSourceExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domSourceExist.LoadXml(strExistSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecordװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }


            // �۲�ʱ����Ƿ����仯
            nRet = ByteArray.Compare(info.OldTimestamp, exist_source_timestamp);
            if (nRet != 0)
            {
                // ʱ����������
                // ��Ҫ��info.OldRecord��strExistXml���бȽϣ������Ͳ��¼�йص�Ԫ�أ�Ҫ��Ԫ�أ�ֵ�Ƿ����˱仯��
                // �����ЩҪ��Ԫ�ز�δ�����仯���ͼ������кϲ������Ǳ������

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecordװ�ؽ���DOMʱ��������: " + ex.Message;
                    goto ERROR1;
                }

                // �Ƚ�������¼, �����Ͳ��¼�йص��ֶ��Ƿ����˱仯
                // return:
                //      0   û�б仯
                //      1   �б仯
                nRet = IsRegisterInfoChanged(domOld,
                    domSourceExist);
                if (nRet == 1)
                {
                    error = new EntityInfo(info);
                    // ������Ϣ��, �������޸Ĺ���ԭ��¼����ʱ���
                    error.OldRecord = strExistSourceXml;
                    error.OldTimestamp = exist_source_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "�ƶ�������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + info.OldRecPath + "') �ѱ�ɾ����";
                    else
                        error.ErrorInfo = "�ƶ�������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + info.OldRecPath + "') �ѷ������޸�";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_source_timestamp��ʱ�Ѿ���ӳ�˿��б��޸ĺ�ļ�¼��ʱ���
            }

            string strSourceLibraryCode = "";
            // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
            // return:
            //      -1  �����̳���
            //      0   ����Ҫ��
            //      1   ������Ҫ��
            nRet = CheckItemLibraryCode(domSourceExist,
                        sessioninfo.LibraryCodeList,
                        out strSourceLibraryCode,
                        out strError);
            if (nRet == -1)
                goto ERROR1;


            // ���ɼ�¼�Ƿ����ڹ�Ͻ��Χ
            if (sessioninfo.GlobalUser == false)
            {
                if (nRet != 0)
                {
                    strError = "�������ƶ��Ĳ��¼��ݲصص㲻����Ҫ��: " + strError;
                    goto ERROR1;
                }
            }

            bool bNoOperations = false; // �Ƿ�Ϊ��Ҫ����<operations>����
            if (StringUtil.IsInList("nooperations", strStyle) == true)
            {
                bNoOperations = true;
            }

            if (bNoOperations == false)
            {
                // 2010/4/8
                // 
                nRet = SetOperation(
    ref domNew,
    "moved",
    sessioninfo.UserID,
    "",
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }


            // �ϲ��¾ɼ�¼
            string strNewXml = "";
            nRet = MergeTwoEntityXml(domSourceExist,
                domNew,
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // ֻ��orderȨ�޵����
            if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
            {
                // 2009/11/26 changed
                string strEntityDbName = ResPath.GetDbName(info.OldRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "��·�� '" + info.OldRecPath + "' �л�����ݿ���ʱʧ��";
                    goto ERROR1;
                }

                string strBiblioDbName = "";

                // ����ʵ�����, �ҵ���Ӧ����Ŀ����
                // ע�⣬����1��ʱ��strBiblioDbNameҲ�п���Ϊ��
                // return:
                //      -1  ����
                //      0   û���ҵ�
                //      1   �ҵ�
                nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                out strBiblioDbName,
                out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "����ʵ����� '" + strEntityDbName + "' �л����Ŀ����ʱʧ��";
                    goto ERROR1;
                }

                // �ǹ�����
                if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                {
                    // �ǹ����⡣Ҫ��<state>�������ӹ��С�
                    string strState = DomUtil.GetElementText(domSourceExist.DocumentElement,
                        "state");
                    if (IncludeStateProcessing(strState) == false)
                    {
                        strError = "��ǰ�ʻ�ֻ��orderȨ�޶�û��setiteminfo(��setentities)Ȩ�ޣ�������move����ɾ�������ڷǹ�����ġ�״̬���������ӹ��С���ʵ���¼ '" + info.OldRecPath + "'";
                        goto ERROR1;
                    }
                }

                // TODO: ���ԭ���ƶ���Ŀ���¼�������޸ģ��ƺ�Ҳ������?
            }

            // �ƶ���¼
            byte[] output_timestamp = null;

            // TODO: Copy��Ҫдһ�Σ���ΪCopy����д���¼�¼��(ע��Copy/Move��ʱ����⣬������¼��<parent>��Ҫ�ı�)
            // ��ʵCopy���������ڴ�����Դ�����򻹲�����Save+Delete
            lRet = channel.DoCopyRecord(info.OldRecPath,
                info.NewRecPath,
                true,   // bDeleteSourceRecord
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "DoCopyRecord() error :" + strError;
                goto ERROR1;
            }

            string strTargetLibraryCode = "";
            // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
            // return:
            //      -1  �����̳���
            //      0   ����Ҫ��
            //      1   ������Ҫ��
            nRet = CheckItemLibraryCode(strNewXml,
                        sessioninfo.LibraryCodeList,
                        out strTargetLibraryCode,
                        out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2014/7/3
            if (this.VerifyBookType == true)
            {
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "��·�� '" + info.NewRecPath + "' �л�����ݿ���ʱʧ��";
                    goto ERROR1;
                }

                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);

                // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
                // parameters:
                // return:
                //      -1  �����̳���
                //      0   ����Ҫ��
                //      1   ������Ҫ��
                nRet = CheckItemBookType(domTemp,
                    strEntityDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;
            }


            // ����¼�¼�Ƿ����ڹ�Ͻ��Χ
            if (sessioninfo.GlobalUser == false)
            {
                if (nRet != 0)
                {
                    strError = "���¼�������еĹݲصص㲻����Ҫ��: " + strError;
                    goto ERROR1;
                }
            }

            // Debug.Assert(strOutputPath == info.NewRecPath);
            string strTargetPath = strOutputPath;

            lRet = channel.DoSaveTextRes(strTargetPath,
                strNewXml,
                false,   // include preamble?
                "content",
                output_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "WriteEntities()API move�����У�ʵ���¼ '" + info.OldRecPath + "' �Ѿ����ɹ��ƶ��� '" + strTargetPath + "' ������д��������ʱ��������: " + strError;

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    // �����з�������
                    // ��ΪԴ�Ѿ��ƶ�������ܸ���
                }

                // ����д�������־���ɡ�û��Undo
                this.WriteErrorLog(strError);

                /*
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "��������(DoCopyRecord())������ʱ�����ͻ, ����10��������Ȼʧ��";
                        goto ERROR1;
                    }
                    // ����ʱ�����ƥ��
                    // �ظ�������ȡ�Ѵ��ڼ�¼\�ȽϵĹ���
                    nRedoCount++;
                    goto REDOLOAD;
                }*/


                error = new EntityInfo(info);
                error.ErrorInfo = "�ƶ�������������:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // �ɹ�
            {
                info.NewRecPath = strOutputPath;    // ���ֱ����λ�ã���Ϊ������׷����ʽ��·��

                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strSourceLibraryCode + "," + strTargetLibraryCode);    // �����ڵĹݴ���

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "move");
                if (String.IsNullOrEmpty(strStyle) == false)
                    DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                // �¼�¼
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // �ɼ�¼
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistSourceXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // ����ɹ�����Ҫ������ϢԪ�ء���Ϊ��Ҫ�����µ�ʱ���
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "�ƶ������ɹ���NewRecPath�з�����ʵ�ʱ����·��, NewTimeStamp�з������µ�ʱ�����NewRecord�з�����ʵ�ʱ�����¼�¼(���ܺ��ύ��Դ��¼���в���)��";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;

        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // ̽����¼���Ƿ�����ͨ��Ϣ?
        // parameters:
        //      strDetail   �����ϸ������Ϣ
        static bool IsEntityHasCirculationInfo(XmlDocument dom,
            out string strDetail)
        {
            strDetail = "";
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower").Trim();

            if (String.IsNullOrEmpty(strBorrower) == true)
                return false;
            strDetail = "������ " + strBorrower + " ����";
            return true;
        }

        #endregion

#if NO
        // ���ݲ�������б��õ���¼·���б�
        // ����������û�����м�¼������Ӧλ�÷��ؿ��ַ������������������ж�����¼������Ӧλ�÷����ַ�'!'��ͷ�ı�����Ϣ
        public int GetItemRecPathList(
            RmsChannelCollection channels,
            string strBarcodeList,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            List<string> results = new List<string>();
            string [] barcodes = strBarcodeList.Split(new char[] {','});
            foreach (string barcode in barcodes)
            {
                string strBarcode = barcode.Trim();

                if (string.IsNullOrEmpty(strBarcode) == true)
                {
                    results.Add("");
                    continue;
                }

                string strXml = "";
                List<string> aPath = null;
                byte[] timestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   ����1��
                //      >1  ���ж���1��
                int nRet = this.GetItemRec(
                    channels,
                    strBarcode,
                    "",
                    out strXml,
                    10,
                    out aPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    results.Add("");
                    continue;
                }

                if (nRet > 1)
                {
                    results.Add("!������� '' ��Ψһ�����м�¼ "+nRet.ToString()+" ��");
                    continue;
                }

                if (aPath == null || aPath.Count == 0)
                {
                    strError = "aPath����";
                    return -1;
                }
                results.Add(aPath[0]);
            }

            strResult = StringUtil.MakePathList(results);
            return 1;
        }
#endif
        static int IndexOf(
            List<string> list,
            string one,
            bool bIgnoreCase)
        {
            int index = 0;
            foreach (string s in list)
            {
                if (string.Compare(s, one, bIgnoreCase) == 0)
                    return index;
                index++;
            }

            return -1;
        }

        // ���ݲ�������б��õ���¼·���б�
        // ����������û�����м�¼������Ӧλ�÷��ؿ��ַ������������������ж�����¼������Ӧλ�÷����ַ�'!'��ͷ�ı�����Ϣ
        public int GetItemRecPathList(
            RmsChannelCollection channels,
            string strDbType,
            string strFrom,
            string strWordList,
            bool bIgnoreCase,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            int nRet = 0;

            string[] words = strWordList.Split(new char[] { ',' });

            // ��������
            List<string> word_list = new List<string>();
            foreach (string word in words)
            {
                string strWord = word.Trim();
                if (string.IsNullOrEmpty(strWord) == true)
                {
                    word_list.Add("");
                    continue;
                }
                word_list.Add(strWord);
            }

            int nMaxCount = Math.Max(word_list.Count * 3, 1000);    // ����Ϊ 1000
            List<Record> records = null;

            // return:
            //      -1  ����
            //      0   һ��Ҳû������
            //      >0  ���е��ܸ�����ע�⣬�ⲻһ����results�з��ص�Ԫ�ظ�����results�з��صĸ�����Ҫ�ܵ�nMax�����ƣ���һ������ȫ�����и���
            nRet = this.GetItemRec(
    channels,
    strDbType,
    strWordList,
    strFrom,
    nMaxCount,
    "keyid,id,key",    // Ҫ����key��������֪���Ƿ�����������ظ�
    out records,
    out strError);
            if (nRet == -1)
                return -1;

            int nHitCount = nRet;

            List<string> results = new List<string>();
            for (int i = 0; i < word_list.Count; i++)
            {
                results.Add("");
            }

            // ����key�鲢?
            foreach (Record record in records)
            {
                if (record.Keys == null || record.Keys.Length == 0)
                {
                    strError = "record.Keys error";
                    return -1;
                }

                string strKey = record.Keys[0].Key;

                int nIndex = IndexOf(word_list, strKey, bIgnoreCase);
                if (nIndex == -1)
                {
                    strError = "����ֳ����� key '" + strKey + "' ��wordlist '" + strWordList + "' ��û��ƥ�����";
                    return -1;
                }

                // �Ƿ��������м������ظ�?
                if (string.IsNullOrEmpty(results[nIndex]) == false)
                    results[nIndex] = "!" + strFrom + " '" + strKey + "' �������в�Ψһ";
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(record.Path) == false, "");
                    results[nIndex] = record.Path;
                }
            }

#if TESTING
            ///
            records = new List<Record>();   // ������
            int nHitCount = 0;  // ������
            List<string> results = new List<string>();
            for (int i = 0; i < word_list.Count; i++)
            {
                results.Add("");
            }
            ///
#endif

            if (nHitCount > records.Count)
            {
                // ���е�ȫ����¼û��ȡ�꣬�����Ϳ��ܷ����еĲ�����ʵ���������˵�û�����ü���������
                // ���Ե�ʱ��ע��Ҫ��������������в���

                // ��û�����еļ������ٴ���֯����
                List<string> temp_words = new List<string>();
                for (int i = 0; i < results.Count; i++)
                {
                    if (string.IsNullOrEmpty(results[i]) == true)
                        temp_words.Add(word_list[i]);
                }

                if (temp_words.Count == 0)
                    goto END1;  // Ŀǰ��û��δ���еļ����ʣ����Բ����ٽ��м����ˡ�����������������һ��������ظ�����̫����ɵ�

                // ����Щ�����ʸ�Ϊһ��һ������
                List<string> temp_results = new List<string>();
                foreach (string temp_word in temp_words)
                {
                    string strXml = "";
                    List<string> aPath = null;
                    byte[] timestamp = null;
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   ����1��
                    //      >1  ���ж���1��
                    nRet = this.GetOneItemRec(
                        channels,
                        strDbType,
                        temp_word,
                        strFrom,
                        "",
                        out strXml,
                        10,
                        out aPath,
                        out timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        temp_results.Add("");   // û������
                    else if (nRet > 1)
                        temp_results.Add("!" + strFrom + " '" + temp_word + "' �������в�Ψһ");
                    else
                    {
                        Debug.Assert(nRet == 1, "");
                        if (aPath == null || aPath.Count < 1)
                        {
                            strError = "aPath error";
                            return -1;
                        }
                        temp_results.Add(aPath[0]);
                    }
                }

                // ���뵱ǰ�����
                if (temp_results.Count != temp_words.Count)
                {
                    strError = "GetOneItemRec() ѭ�����صĽ����Ŀ�ͼ����ʸ���������";
                    return -1;
                }

                Debug.Assert(temp_results.Count == temp_words.Count, "");

                for (int i = 0; i<temp_words.Count; i++)
                {
                    string word = temp_words[i];
                    int nPos = IndexOf(word_list, word, bIgnoreCase);
                    if (nRet == -1)
                    {
                        strError = "����ֳ����� temp_word '" + word + "' ��wordlist '" + strWordList + "' ��û��ƥ�����";
                        return -1;
                    }

                    results[nPos] = temp_results[i];
                }
            }

        END1:
            strResult = StringUtil.MakePathList(results);
            return 1;
        }


    }


    // ʵ����Ϣ
    public class DeleteEntityInfo
    {
        public string RecPath = ""; // ��¼·��

        public string OldRecord = "";   // �ɼ�¼
        public byte[] OldTimestamp = null;  // �ɼ�¼��Ӧ��ʱ���

        public string ItemBarcode = ""; // �������
    }

    // ʵ����Ϣ
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class EntityInfo
    {
        [DataMember]
        public string RefID = "";  // 2008/2/17 new add ǰ�˷���Set...����ʱ������ʶ��id����������������Ӧ�У�����ǰ������Ӧ���������

        [DataMember]
        public string OldRecPath = "";  // ԭ��¼·�� 2007/6/2 new add
        [DataMember]
        public string OldRecord = "";   // �ɼ�¼
        [DataMember]
        public byte[] OldTimestamp = null;  // �ɼ�¼��Ӧ��ʱ���

        [DataMember]
        public string NewRecPath = ""; // �¼�¼·��
        [DataMember]
        public string NewRecord = "";   // �¼�¼
        [DataMember]
        public byte[] NewTimestamp = null;  // �¼�¼��Ӧ��ʱ���

        [DataMember]
        public string Action = "";   // Ҫִ�еĲ���(getʱ��������) ֵΪnew change delete move 4��֮һ��changeҪ��OldRecPath��NewRecPathһ����move��Ҫ������һ������move�������г�������Ҫ��Ϊ����־ͳ�Ƶı�����

        [DataMember]
        public string Style = "";   // 2008/10/6 new add ��񡣳��������ӵ����Բ���������: nocheckdup,noeventlog,force

        [DataMember]
        public string ErrorInfo = "";   // ������Ϣ
        [DataMember]
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;   // �����루��ʾ���ں������͵Ĵ���

        public EntityInfo(EntityInfo info)
        {
            this.RefID = info.RefID;
            this.OldRecPath = info.OldRecPath;
            this.OldRecord = info.OldRecord;
            this.OldTimestamp = info.OldTimestamp;
            this.NewRecPath = info.NewRecPath;
            this.NewRecord = info.NewRecord;
            this.NewTimestamp = info.NewTimestamp;
            this.Action = info.Action;
            this.ErrorInfo = info.ErrorInfo;
            this.ErrorCode = info.ErrorCode;
        }

        public EntityInfo()
        {

        }
    }


}
