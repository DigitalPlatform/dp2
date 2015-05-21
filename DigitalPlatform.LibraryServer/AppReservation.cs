using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// �������Ǻ���ͨԤԼ(����)������صĴ���
    /// </summary>
    public partial class LibraryApplication
    {
        // ԤԼ
        // Ȩ�ޣ���Ҫ��reservationȨ��
        public LibraryServerResult Reservation(
            SessionInfo sessioninfo,
            string strFunction,
            string strReaderBarcode,
            string strItemBarcodeList)
        {
            LibraryServerResult result = new LibraryServerResult();

            // Ȩ���ַ���
            if (StringUtil.IsInList("reservation", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                // text-level: �û���ʾ
                result.ErrorInfo = this.GetString("ԤԼ�������ܾ������߱�reservationȨ�ޡ�");
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            int nRet = 0;
            string strError = "";

            // 2010/12/31
            if (String.IsNullOrEmpty(this.ArrivedDbName) == true)
            {
                strError = "ԤԼ�������δ����, ԤԼ����ʧ��";
                goto ERROR1;
            }

            if (String.Compare(strFunction, "new", true) != 0
                && String.Compare(strFunction, "delete", true) != 0
                && String.Compare(strFunction, "merge", true) != 0
                && String.Compare(strFunction, "split", true) != 0
                )
            {
                result.Value = -1;

                // text-level: �ڲ�����
                result.ErrorInfo = string.Format(this.GetString("δ֪��strFunction����ֵs"),    // "δ֪��strFunction����ֵ '{0}'"
                    strFunction);
                    // "δ֪��strFunction����ֵ '" + strFunction + "'";
                result.ErrorCode = ErrorCode.InvalidParameter;
                return result;
            }

            // �ڼܲἯ��
            List<string> OnShelfItemBarcodes = new List<string>();

            // ��ɾ�����ѵ���״̬�Ἧ��
            List<string> ArriveItemBarcodes = new List<string>();
            
            // �Ӷ��߼�¼��
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try
            {
                // ������߼�¼
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;

                nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    // text-level: �û���ʾ
                    result.ErrorInfo = string.Format(this.GetString("����֤�����s������"),   // ����֤����� {0} ������
                        strReaderBarcode);
                    // "����֤����� '" + strReaderBarcode + "' ������";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = string.Format(this.GetString("������߼�¼ʱ��������s"), // "������߼�¼ʱ��������: {0}"
                        strError);
                        // "������߼�¼ʱ��������: " + strError;
                    goto ERROR1;
                }

                string strLibraryCode = "";
                // �������߼�¼�����������ݿ⣬�Ƿ��ڲ�����ͨ�Ķ��߿�֮��
                // 2012/9/8
                if (String.IsNullOrEmpty(strOutputReaderRecPath) == false)
                {
                    string strReaderDbName = ResPath.GetDbName(strOutputReaderRecPath);
                    bool bReaderDbInCirculation = true;
                    if (this.IsReaderDbName(strReaderDbName,
                        out bReaderDbInCirculation,
                        out strLibraryCode) == false)
                    {
                        // text-level: �ڲ�����
                        strError = "���߼�¼·�� '" + strOutputReaderRecPath + "' �е����ݿ��� '" + strReaderDbName + "' ��Ȼ���ڶ���Ķ��߿�֮�С�";
                        goto ERROR1;
                    }

                    if (bReaderDbInCirculation == false)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("ԤԼ�������ܾ�������֤�����s���ڵĶ��߼�¼s�������ݿ�s����δ������ͨ�Ķ��߿�"),  // "ԤԼ�������ܾ�������֤����� '{0}' ���ڵĶ��߼�¼ '{1}' �������ݿ� '{2}' ����δ������ͨ�Ķ��߿�"
                            strReaderBarcode,
                            strOutputReaderRecPath,
                            strReaderDbName);

                        goto ERROR1;
                    }

                    // ��鵱ǰ�������Ƿ��Ͻ������߿�
                    // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                    if (this.IsCurrentChangeableReaderPath(strOutputReaderRecPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "���߼�¼·�� '" + strOutputReaderRecPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                        goto ERROR1;
                    }
                }


                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = string.Format(this.GetString("װ�ض��߼�¼����XMLDOMʱ��������s"),   // "װ�ض��߼�¼����XML DOMʱ��������: {0}"
                        strError);
                        // "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                    goto ERROR1;
                }

                if (strFunction == "delete"
                    && sessioninfo.UserType != "reader")
                {
                    // ��������Ա��Ϊ����ʱ, ����delete��������һ�棬�����������(����֤״̬�ͼ���δȡ�����ļ��)
                }
                else
                {
                    // return:
                    //      -1  �����̷����˴���Ӧ�������ܽ���������
                    //      0   ���Խ���
                    //      1   ֤�Ѿ�����ʧЧ�ڣ����ܽ���
                    //      2   ֤�в��ý��ĵ�״̬
                    nRet = CheckReaderExpireAndState(readerdom,
                        out strError);
                    if (nRet != 0)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("ԤԼ�������ܾ���ԭ��s"),   // "ԤԼ�������ܾ���ԭ��: {0}"
                            strError);

                        // "ԤԼ�������ܾ���ԭ��: " + strError;
                        goto ERROR1;
                    }

                    // ��鵽��δȡ�����Ƿ񳬱�
                    XmlNode nodeOutof = readerdom.DocumentElement.SelectSingleNode("outofReservations");
                    if (nodeOutof != null)
                    {
                        string strCount = DomUtil.GetAttr(nodeOutof, "count");
                        int nCount = 0;
                        try
                        {
                            nCount = Convert.ToInt32(strCount);
                        }
                        catch
                        {
                        }
                        if (nCount >= this.OutofReservationThreshold)
                        {
                            strError = string.Format(this.GetString("ԤԼ�������ܾ�����Ϊ��������"),    // "ԤԼ�������ܾ�����Ϊ��ǰ������ǰԤԼ�����δȡ�Ĵ��������� {0} �Σ���ȡ��ԤԼ���������Ҫ�ָ�ԤԼ����������ߵ�ͼ��ݹ�̨������������"
                                this.OutofReservationThreshold.ToString());

                            // "ԤԼ�������ܾ�����Ϊ��ǰ������ǰԤԼ�����δȡ�Ĵ��������� " + this.OutofReservationThreshold.ToString() + " �Σ���ȡ��ԤԼ���������Ҫ�ָ�ԤԼ����������ߵ�ͼ��ݹ�̨������������";
                            goto ERROR1;
                        }
                    }
                }


                // ׼����־DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // �������ڵĹݴ���
                DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "reservation");


                if (String.Compare(strFunction, "new", true) == 0)
                {
                    // �Լ���ԤԼ�Ĳ�����Ž��в���
                    // Ҫ�󱾶�����ǰδ������Щ�����ԤԼ��
                    // return:
                    //      -1  ����
                    //      0   û����
                    //      1   ���� ��ʾ��Ϣ��strError��
                    nRet = this.ReservationCheckDup(
                        strItemBarcodeList,
                        strLibraryCode,
                        ref readerdom,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        result.Value = -1;
                        // text-level: �û���ʾ
                        result.ErrorInfo = string.Format(this.GetString("ԤԼ�������ܾ���ԭ��s"),
                            strError);
                        // result.ErrorInfo = "ԤԼ���󱻾ܾ�: " + strError;

                        result.ErrorCode = ErrorCode.DupItemBarcode;
                        return result;
                    }
                } // end of "new"


                // Ϊд�ض��ߡ����¼��׼��
                // byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
                long lRet = 0;

                // �и�������Ĳ������
                string[] itembarcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < itembarcodes.Length; i++)
                {
                    string strItemBarcode = itembarcodes[i].Trim();

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                        continue;

                    string strItemXml = "";
                    string strOutputItemRecPath = "";

                    // ���¼����
                    this.EntityLocks.LockForWrite(strItemBarcode);

                    try
                    {
                        int nRedoCount = 0;

                    REDO_LOAD:
                        byte[] item_timestamp = null;

                        // ��ò��¼
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   ����1��
                        //      >1  ���ж���1��
                        nRet = this.GetItemRecXml(
                            channel,
                            strItemBarcode,
                            out strItemXml,
                            out strOutputItemRecPath,
                            out item_timestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            result.Value = -1;
                            // text-level: �û���ʾ
                            result.ErrorInfo = string.Format(this.GetString("�������s������"),   // "������� {0} ������"
                                strItemBarcode);
                            // "������� '" + strItemBarcode + "' ������";
                            result.ErrorCode = ErrorCode.ItemBarcodeNotFound;
                            return result;
                        }
                        if (nRet == -1)
                        {
                            // text-level: �ڲ�����
                            strError = string.Format(this.GetString("������¼ʱ��������s"),   // "������¼ʱ��������: {0}"
                                strError);
                                // "������¼ʱ��������: " + strError;
                            goto ERROR1;
                        }

                        if (nRet > 1)
                        {
                            // text-level: �ڲ�����
                            strError = string.Format(this.GetString("�������s���ظ�"),   // "������� '{0}' ���ظ�({1}��)���޷�����ԤԼ������"
                                strItemBarcode,
                                nRet.ToString());
                            // "������� '" + strItemBarcode + "' ���ظ�(" + nRet.ToString() + "��)���޷�����ԤԼ������";
                        }

                        XmlDocument itemdom = null;
                        nRet = LibraryApplication.LoadToDom(strItemXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                        {
                            // text-level: �ڲ�����
                            strError = string.Format(this.GetString("װ�ز��¼����XMLDOMʱ��������s"),   // "װ�ز��¼����XML DOMʱ��������: {0}"
                                strError);
                            // "װ�ز��¼����XML DOMʱ��������: " + strError;
                            goto ERROR1;
                        }

                        // 2012/9/13
                        // ���һ�����¼�Ĺݲصص��Ƿ���Ϲݴ����б�Ҫ��
                        // return:
                        //      -1  �����̳���
                        //      0   ����Ҫ��
                        //      1   ������Ҫ��
                        nRet = CheckItemLibraryCode(itemdom,
                            strLibraryCode,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                        {
                            strError = "���¼ '" + strItemBarcode + "' ��ݲصض����ܽ���ԤԼ: " + strError;
                            goto ERROR1;
                        }

                        // 2011/12/7
                        // �����¼״̬
                        string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                            "state");
                        if (string.IsNullOrEmpty(strState) == false)
                        {
                            // text-level: �û���ʾ
                            strError = string.Format(this.GetString("��״̬Ϊs�޷�ԤԼ"),   // "�� {0} ״̬Ϊ '{1}' ���޷�����ԤԼ������"
                                strItemBarcode,
                                strState,
                                nRet.ToString());
                            goto ERROR1;
                        }

                        bool bOnShelf = false;
                        bool bArrived = false;

                        // �ڲ��¼����ӻ���ɾ��ԤԼ��Ϣ
                        nRet = this.DoReservationItemXml(
                            sessioninfo.Channels,
                            strFunction,
                            strReaderBarcode,
                            sessioninfo.UserID,
                            ref itemdom,
                            out bOnShelf,
                            out bArrived,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (bOnShelf == true)
                            OnShelfItemBarcodes.Add(strItemBarcode);

                        if (bArrived == true)
                            ArriveItemBarcodes.Add(strItemBarcode);


                        // д�ز��¼
                        lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                            itemdom.OuterXml,
                            false,
                            "content", // ,ignorechecktimestamp
                            item_timestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {
                                if (nRedoCount > 10)
                                {
                                    // text-level: �ڲ�����
                                    strError = "д�ز��¼ '" + strOutputItemRecPath + "' ʱ��������ʱ�����ͻ, ����10��������Ȼʧ��";
                                    goto ERROR1;
                                }
                                nRedoCount++;
                                goto REDO_LOAD;
                            }
                            // ��д������������ʱ�������ø���ѧ�Ĵ�ʩ��undo��
                            // ���Ѹղ����ӵ�<request>Ԫ���ҵ���ɾ��
                            goto ERROR1;
                        }


                    }
                    finally
                    {
                        this.EntityLocks.UnlockForWrite(strItemBarcode);
                    }

                } // end of for

                // �ڶ��߼�¼�м����ɾ��ԤԼ��Ϣ
                // parameters:
                //      strFunction "new"����ԤԼ��Ϣ��"delete"ɾ��ԤԼ��Ϣ; "merge"�ϲ�; "split"��ɢ
                // return:
                //      -1  error
                //      0   unchanged
                //      1   changed
                nRet = this.DoReservationReaderXml(
                    strFunction,
                    strItemBarcodeList,
                    sessioninfo.UserID,
                    ref readerdom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                {
                    // Ұ��д��
                    lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action", strFunction);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "readerBarcode", strReaderBarcode);
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "itemBarcodeList", strItemBarcodeList);

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
                        // text-level: �ڲ�����
                        strError = "Reservation() API д����־ʱ��������: " + strError;
                        goto ERROR1;
                    }

                    // д��ͳ��ָ��
                    if (this.Statis != null)
                        this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "����",
                        "ԤԼ��",
                        1);

                }

                // �Ե�ǰ����ͨ�ܵ�ͼ�飬������������֪ͨ
                if (this.CanReserveOnshelf == true
                    && OnShelfItemBarcodes.Count > 0)
                {
                    // ֻ֪ͨ��һ���ڼܵĲ�
                    string strItemBarcode = OnShelfItemBarcodes[0];

                    if (string.IsNullOrEmpty(strItemBarcode) == true)
                    {
                        strError = "�ڲ�����OnShelfItemBarcodes �ĵ�һ��Ԫ��Ϊ�ա�������� '"+StringUtil.MakePathList(OnShelfItemBarcodes)+"'";
                        goto ERROR1;
                    }

                    List<string> DeletedNotifyRecPaths = null;  // ��ɾ����֪ͨ��¼�����á�
                    // ֪ͨԤԼ����Ĳ���
                    // ���ڶԶ��߿��������ı�������, �������˴˺���
                    // return:
                    //      -1  error
                    //      0   û���ҵ�<request>Ԫ��
                    nRet = DoReservationNotify(
                        sessioninfo.Channels,
                        strReaderBarcode,
                        false,  // ����Ҫ�����ڼӶ���������Ϊ�����Ѿ�����
                        strItemBarcode,
                        true,   // ����ͨ��
                        true,   // ��Ҫ�޸ĵ�ǰ���¼��<request>Ԫ��state����
                        out DeletedNotifyRecPaths,
                        out strError);
                    if (nRet == -1)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("ԤԼ�����Ѿ��ɹ�, �����ڼ�����֪ͨ����ʧ��, ԭ��s"),  // "ԤԼ�����Ѿ��ɹ�, �����ڼ�����֪ͨ����ʧ��, ԭ��: {0}"
                            strError);
                            // "ԤԼ�����Ѿ��ɹ�, �����ڼ�����֪ͨ����ʧ��, ԭ��: " + strError;
                        goto ERROR1;
                    }

                    /*
                            if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(
                        strLibraryCode,
                        "����",
                        "ԤԼ�����",
                        1);
                     * */

                    // ����ɹ���ʾ
                    // text-level: �û���ʾ
                    string strMessage = string.Format(this.GetString("��ע�⣬�����ύ��ԤԼ���������͵õ��˶���"), // "��ע�⣬�����ύ��ԤԼ���������͵õ��˶���(ԤԼ����֪ͨ��ϢҲ���������ˣ���ע�����)����ԤԼ�Ĳ� {0} Ϊ�ڼ�״̬����Ϊ������������������Ϳ���ͼ��ݰ������������"
                        strItemBarcode);
                        // "��ע�⣬�����ύ��ԤԼ���������͵õ��˶���(ԤԼ����֪ͨ��ϢҲ���������ˣ���ע�����)����ԤԼ�Ĳ� " + strItemBarcode + " Ϊ�ڼ�״̬����Ϊ������������������Ϳ���ͼ��ݰ������������";
                    if (OnShelfItemBarcodes.Count > 1)
                    {
                        OnShelfItemBarcodes.Remove(strItemBarcode);
                        string [] barcodelist = new string[OnShelfItemBarcodes.Count];
                        OnShelfItemBarcodes.CopyTo(barcodelist);
                        // text-level: �û���ʾ
                        strMessage += string.Format(this.GetString("����ͬһԤԼ������Ҳͬʱ�ύ�������ڼ�״̬�Ĳ�"),   // "����ͬһԤԼ������Ҳͬʱ�ύ�������ڼ�״̬�Ĳ�: {0}����ͬһ�����е�ǰ���� {1} ����Ч����Щ��ͬʱ�����ԡ�(��ȷҪԤԼ����ڼܵĲ������Ƕ�������Ч����ÿ�ι�ѡһ���󵥶��ύ������Ҫ�Ѷ����һ�����ύ��)"
                            String.Join(",", barcodelist),
                            strItemBarcode);
                            // "����ͬһԤԼ������Ҳͬʱ�ύ�������ڼ�״̬�Ĳ�: " + String.Join(",", barcodelist) + "����ͬһ�����е�ǰ���� " + strItemBarcode + " ����Ч����Щ��ͬʱ�����ԡ�(��ȷҪԤԼ����ڼܵĲ������Ƕ�������Ч����ÿ�ι�ѡһ���󵥶��ύ������Ҫ�Ѷ����һ�����ύ��)";
                    }

                    result.ErrorInfo = strMessage;
                }

                if (ArriveItemBarcodes.Count > 0)
                {
                    string[] barcodelist = new string[ArriveItemBarcodes.Count];
                    ArriveItemBarcodes.CopyTo(barcodelist);

                    // text-level: �û���ʾ
                    result.ErrorInfo += string.Format(this.GetString("��s��ɾ��ǰ�Ѿ����ڵ���״̬"),    // "�� {0} ��ɾ��ǰ�Ѿ����ڡ����顱״̬�����ո�ɾ������(Щ)��������ζ�����Ѿ�����ȡ�顣ͼ��ݽ�˳����������Ŷӵȴ���ԤԼ�ߵ����󣬻������������߽��Ĵ��顣(������ͼҪȥͼ�������ȡ�飬��һ����Ҫȥɾ��������״̬Ϊ���ѵ��顱���������������ȡ����Զ�ɾ��)"
                        String.Join(",", barcodelist));
                        // "�� " + String.Join(",", barcodelist) + " ��ɾ��ǰ�Ѿ����ڡ����顱״̬�����ո�ɾ������(Щ)��������ζ�����Ѿ�����ȡ�顣ͼ��ݽ�˳����������Ŷӵȴ���ԤԼ�ߵ����󣬻������������߽��Ĵ��顣(������ͼҪȥͼ�������ȡ�飬��һ����Ҫȥɾ��������״̬Ϊ���ѵ��顱���������������ȡ����Զ�ɾ��)";
                }
            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // �Լ���ԤԼ�Ĳ�����Ž��в���
        // Ҫ�󱾶�����ǰδ������Щ�����ԤԼ��
        // parameters:
        //      strLibraryCode  ���߼�¼���ڶ��߿�Ĺݴ���
        // return:
        //      -1  ����
        //      0   û����
        //      1   ���� ��ʾ��Ϣ��strError��
        public int ReservationCheckDup(
            string strItemBarcodeList,
            string strLibraryCode,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strItemBarcodeList) == true)
            {
                strError = this.GetString("��������б���Ϊ��");    // ��������б���Ϊ��
                return -1;
            }

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            string strParamValue = "";
            MatchResult matchresult;

            // �õ��ö������������������ͼ���"��ԤԼ����"
            // return:
            //      reader��book���;�ƥ�� ��4��
            //      ֻ��reader����ƥ�䣬��3��
            //      ֻ��book����ƥ�䣬��2��
            //      reader��book���Ͷ���ƥ�䣬��1��
            int nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "��ԤԼ����",
                out strParamValue,
                out matchresult,
                out strError);
            if (nRet == -1 || nRet < 3)
            {
                // text-level: �û���ʾ
                strError = string.Format(this.GetString("��������s��δ�����ԤԼ��������"),  // "�������� '{0}' ��δ���� ��ԤԼ���� ����, ԤԼ�������ܾ�"
                    strReaderType);
                    // "�������� '" + strReaderType + "' ��δ���� ��ԤԼ���� ����, ԤԼ�������ܾ�";
                return -1;
            }

            int nMaxReserveItems = 0;
            try
            {
                nMaxReserveItems = Convert.ToInt32(strParamValue);
            }
            catch
            {
                // text-level: �ڲ�����
                strError = "�ݴ��� '" + strLibraryCode + "' �� �������� '" + strReaderType + "' ����� ��ԤԼ���� ����ֵ '" + strParamValue + "' ���Ϸ���Ӧ��Ϊ������";
                return -1;
            }


            string[] newbarcodes = strItemBarcodeList.Split(new char[] { ',' });

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("reservations/request");

            // ����Ƿ񳬹�ÿ�����ߵ����
            if (nodes.Count >= nMaxReserveItems)
            {
                // text-level: �û���ʾ
                strError = string.Format(this.GetString("ԤԼ�����������ֵ"),  // "����ԤԼǰ�Ѿ�ԤԼ���������Ѿ��ﵽ {0}���Ѿ����� �������� '{1}' ����Ŀ�ԤԼ���� {2}��ԤԼ�������ܾ�"
                    nodes.Count,
                    strReaderType,
                    nMaxReserveItems.ToString());
                    // "����ԤԼǰ�Ѿ�ԤԼ���������Ѿ��ﵽ " + nodes.Count + "���Ѿ����� �������� '" + strReaderType + "' ����Ŀ�ԤԼ���� " + nMaxReserveItems.ToString() + "��ԤԼ�������ܾ�";
                return -1;
            }

            // ����Ƿ�����ԤԼ��
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strItems = DomUtil.GetAttr(node, "items");

                string[] barcodes = strItems.Split(new char[] { ',' });
                for (int j = 0; j < barcodes.Length; j++)
                {
                    string strBarcode = barcodes[j].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    for (int k = 0; k < newbarcodes.Length; k++)
                    {
                        string strNewBarcode = newbarcodes[k].Trim();
                        if (String.IsNullOrEmpty(strNewBarcode) == true)
                            continue;

                        if (strNewBarcode == strBarcode)
                        {
                            // text-level: �û���ʾ
                            strError = string.Format(this.GetString("�������s�Ѿ���ԤԼ��"), // "������� '{0}' �Ѿ���ԤԼ��..."
                                strNewBarcode);
                            // "������� '" + strNewBarcode + "' �Ѿ���ԤԼ��...";
                            return 1;
                        }

                    } // end of k

                } // end for j

            } // end for i

            // ����Ƿ�������ǰ���߽���
            nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strItemBarcode = DomUtil.GetAttr(node, "barcode");
                if (String.IsNullOrEmpty(strItemBarcode) == true)
                    continue;

                for (int k = 0; k < newbarcodes.Length; k++)
                {
                    string strNewBarcode = newbarcodes[k].Trim();
                    if (String.IsNullOrEmpty(strNewBarcode) == true)
                        continue;

                    if (strNewBarcode == strItemBarcode)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("��s�Ѿ�����ǰ���߽���"),   // "�� '{0}' �ѱ���ǰ���߽��ģ���˲��ܱ�ԤԼ..."
                            strNewBarcode);
                            // "�� '" + strNewBarcode + "' �ѱ���ǰ���߽��ģ���˲��ܱ�ԤԼ...";
                        return 1;
                    }

                } // end of k


            } // end for i

            return 0;
        }

        // �ڲ��¼�м���ԤԼ��Ϣ
        // parameters:
        //      strFunction "new"����ԤԼ��Ϣ��"delete"ɾ��ԤԼ��Ϣ
        //      bOnShelf    strFunctionΪ"new"������£�����᱾����û���˽��ģ����ҵ�ǰ����ΪԤԼ�ò�ĵ�һ�ˣ���bOnShelf����true
        //      bArrived    strFunciontΪ"delete"������£�ɾ����״̬Ϊ"arrived"��ԤԼ����
        public int DoReservationItemXml(
            RmsChannelCollection channels,
            string strFunction,
            string strReaderBarcode,
            string strOperator,
            ref XmlDocument itemdom,
            out bool bOnShelf,
            out bool bArrived,
            out string strError)
        {
            strError = "";
            bOnShelf = false;
            bArrived = false;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: �û���ʾ
                strError = this.GetString("����֤����Ų���Ϊ��");    // ����֤����Ų���Ϊ��
                return -1;
            }

            XmlNode root = null;

            root = itemdom.DocumentElement.SelectSingleNode("reservations");
            if (root == null)
            {
                root = itemdom.CreateElement("reservations");
                root = itemdom.DocumentElement.AppendChild(root);
            }
            // �����Ƿ��Ѿ�����Ԫ��
            XmlNode nodeRequest = root.SelectSingleNode("request[@reader='" + strReaderBarcode + "']");

            if (String.Compare(strFunction, "new", true) == 0)
            {
                // ����Ƿ�û�б��˽���
                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                    "borrower");
                string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                    "state");
                string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                    "barcode");

                XmlNodeList nodesRequest = itemdom.DocumentElement.SelectNodes("reservations/request");
                if (String.IsNullOrEmpty(strBorrower) == true
                    && nodesRequest.Count == 0)
                {
                    // 2009/10/19 new add
                    // ״̬Ϊ���ӹ��С�
                    if (IncludeStateProcessing(strState) == true
                        && this.CanReserveOnshelf == false)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("����ԤԼ�ӹ��еĲ�s"), // "����ԤԼ״̬Ϊ�ӹ��еĲ� {0}"
                            strItemBarcode);
                        return -1;
                    }

                    if (this.CanReserveOnshelf == false)
                    {
                        // text-level: �û���ʾ
                        strError = string.Format(this.GetString("����ԤԼ�ڼܵĲ�s"), // "����ԤԼ�ڼ�(δ�������)�� {0}"
                            strItemBarcode);
                            // "����ԤԼ�ڼ�(δ�������)�� " + strItemBarcode;
                        return -1;
                    }

                    if (IncludeStateProcessing(strState) == false)   // ֻ�в��������ӹ��С��ģ�������֪ͨ������ֻ�ܵ��Ժ�״̬�ı�ʱ֪ͨ
                    {
                        // ���������û���˽��ģ����ҵ�ǰ�����ǵ�һ��ԤԼ�ò��
                        bOnShelf = true;
                    }
                }

                if (nodeRequest == null)
                {
                    nodeRequest = itemdom.CreateElement("request");
                    nodeRequest = root.AppendChild(nodeRequest);
                    DomUtil.SetAttr(nodeRequest, "reader", strReaderBarcode);
                }

                // ����ʱ��
                DomUtil.SetAttr(nodeRequest, "requestDate", this.Clock.GetClock());
                // ������
                DomUtil.SetAttr(nodeRequest, "operator", strOperator);
            }

            if (String.Compare(strFunction, "delete", true) == 0)
            {
                if (nodeRequest != null)
                {
                    // ɾ��ǰҪ���״̬���ǲ���arrived
                    string strState = DomUtil.GetAttr(nodeRequest, "state");
                    if (strState == "arrived")
                    {
                        // TODO: �Ƿ���Ҫ����֪ͨ��ǰ���ߣ�������ζ���Ѿ����������ȡ��

                        string strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                            "barcode");

                        string strQueueRecXml = "";
                        byte[] baQueueRecTimestamp = null;
                        string strQueueRecPath = "";

                        // ���ԤԼ������м�¼
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   ����1��
                        //      >1  ���ж���1��
                        int nRet = GetArrivedQueueRecXml(
                            channels,
                            strItemBarcode,
                            out strQueueRecXml,
                            out baQueueRecTimestamp,
                            out strQueueRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet >= 1)
                        {
                            XmlDocument queue_rec_dom = new XmlDocument();
                            try
                            {
                                queue_rec_dom.LoadXml(strQueueRecXml);
                            }
                            catch (Exception ex)
                            {
                                // text-level: �ڲ�����
                                strError = "ԤԼ���м�¼XMLװ��DOMʱʧ��: " + ex.Message;
                                return -1;
                            }
                            nRet = DoNotifyNext(
                                channels,
                                strQueueRecPath,
                                queue_rec_dom,
                                baQueueRecTimestamp,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 1)
                            {
                                // ��Ҫ���
                                // ���¼��<location>��Ҫȥ��#reservation�����<request>Ԫ��Ҳ��Ҫɾ��
                                string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                                    "location");
                                // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
                                strLocation = StringUtil.GetPureLocationString(strLocation);
                                DomUtil.SetElementText(itemdom.DocumentElement,
                                    "location", strLocation);

                            }
                        }

                        // ��������״̬
                        bArrived = true;

                    } // end of -- if (strState == "arrived")

                    nodeRequest.ParentNode.RemoveChild(nodeRequest);
                } // end of -- if (nodeRequest != null)
            }

            return 0;
        }

        // �ڶ��߼�¼�м����ɾ��ԤԼ��Ϣ
        // parameters:
        //      strFunction "new"����ԤԼ��Ϣ��"delete"ɾ��ԤԼ��Ϣ; "merge"�ϲ�; "split"��ɢ
        //      strItemBarcodeList  ������ŵ��б�ÿ�����ֿ���ʹ�� @refID: ǰ׺
        // return:
        //      -1  error
        //      0   unchanged
        //      1   changed
        public int DoReservationReaderXml(
            string strFunction,
            string strItemBarcodeList,
            string strOperator,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strItemBarcodeList) == true)
            {
                // text-level: �û���ʾ
                strError = this.GetString("��������б���Ϊ��");    // ��������б���Ϊ��
                return -1;
            }

            XmlNode root = null;

            root = readerdom.DocumentElement.SelectSingleNode("reservations");
            if (root == null)
            {
                root = readerdom.CreateElement("reservations");
                root = readerdom.DocumentElement.AppendChild(root);
            }

            if (String.Compare(strFunction, "new", true) == 0)
            {
                XmlNode node = readerdom.CreateElement("request");
                node = root.AppendChild(node);
                DomUtil.SetAttr(node, "items", strItemBarcodeList);

                // ����ʱ��
                DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                // ������
                DomUtil.SetAttr(node, "operator", strOperator);

                return 1;
            }

            // ɾ��
            // ע���Ѿ���֪ͨ״̬������, ���ܼ򵥱༭ɾ��. �������Ϊ����, ����
            if (String.Compare(strFunction, "delete", true) == 0)
            {
                bool bChanged = false;
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    // �������������У�ֻҪƥ���ϴ�������У���ɾ��
                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        XmlNode node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    }

                    // ��ɾ����״̬Ϊarrived�����items���������Ѿ���������ʾ��Ϣ
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            // �ϲ�
            // ע���Ѿ�����֪ͨ״̬������, ���ܺ���������ϲ�
            if (String.Compare(strFunction, "merge", true) == 0)
            {
                string strMerged = "";
                bool bChanged = false;
                XmlNode node = null;
                // �ҵ�������������, Ȼ��ɾ��, �����γ�һ������
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            string strState = DomUtil.GetAttr(node, "state");
                            if (strState == "arrived")
                            {
                                // text-level: �û���ʾ
                                strError = this.GetString("�ϲ��������ܾ���״̬Ϊ�ѵ�����в��ܲ���ϲ�������");    // "�ϲ��������ܾ���״̬Ϊ�ѵ�����в��ܲ���ϲ�������"
                                return -1;
                            }

                            if (strMerged != "")
                                strMerged += ",";
                            strMerged += strItems;
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    } // end of for j

                } // end of for i

                if (bChanged == true)
                {
                    node = readerdom.CreateElement("request");
                    node = root.InsertBefore(node, root.FirstChild);    // ���뵽��һ��
                    DomUtil.SetAttr(node, "items", strMerged);

                    // ����ʱ��
                    DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                    // ������
                    DomUtil.SetAttr(node, "operator", strOperator);
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            // ��ɢ
            // ע���Ѿ�����֪ͨ״̬������, ���ܱ���ɢ
            if (String.Compare(strFunction, "split", true) == 0)
            {
                string strSplited = "";
                bool bChanged = false;
                XmlNode node = null;
                // �ҵ�������������, Ȼ��ɾ��, ���¼����������
                string[] barcodes = strItemBarcodeList.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;

                    XmlNodeList nodes = root.SelectNodes("request");

                    for (int j = 0; j < nodes.Count; j++)
                    {
                        node = nodes[j];
                        string strItems = DomUtil.GetAttr(node, "items");
                        if (IsInBarcodeList(strBarcode, strItems) == true)
                        {
                            string strState = DomUtil.GetAttr(node, "state");
                            if (strState == "arrived")
                            {
                                // text-level: �û���ʾ
                                strError = this.GetString("��ɢ�������ܾ���״̬Ϊ�ѵ�����в��ܲ����ɢ������");    // "��ɢ�������ܾ���״̬Ϊ�ѵ�����в��ܲ����ɢ������"
                                return -1;
                            }


                            if (strSplited != "")
                                strSplited += ",";
                            strSplited += strItems;
                            node.ParentNode.RemoveChild(node);
                            bChanged = true;
                        }
                    } // end of for j

                } // end of for i

                if (bChanged == true)
                {
                    bool bFirst = true;
                    XmlNode prev = null;
                    barcodes = strSplited.Split(new char[] { ',' });
                    for (int i = 0; i < barcodes.Length; i++)
                    {
                        string strBarcode = barcodes[i].Trim();
                        if (String.IsNullOrEmpty(strBarcode) == true)
                            continue;

                        node = readerdom.CreateElement("request");
                        if (bFirst == true)
                        {
                            node = root.InsertBefore(node, root.FirstChild);    // ���뵽��һ��
                            bFirst = false;
                        }
                        else
                        {
                            node = root.InsertAfter(node, prev);
                        }
                        DomUtil.SetAttr(node, "items", strBarcode);

                        // ����ʱ��
                        DomUtil.SetAttr(node, "requestDate", this.Clock.GetClock());
                        // ������
                        DomUtil.SetAttr(node, "operator", strOperator);
                        prev = node;
                    }
                }

                if (bChanged == true)
                    return 1;

                return 0;
            }

            return 0;
        }

        // text-level: �ڲ�����
        // ֪ͨԤԼ����Ĳ���
        // ���ڶԶ��߿��������ı�������, �������˴˺���
        // ע������������Ҫɾ������֪ͨ��¼
        // parameters:
        //      strItemBarcodeParam  ������š�����ʹ�� "@refID:" ǰ׺
        // return:
        //      -1  error
        //      0   û���ҵ�<request>Ԫ��
        //      1   �ѳɹ�����
        public int DoReservationNotify(
            RmsChannelCollection channels,
            string strReservationReaderBarcode,
            bool bNeedLockReader,
            string strItemBarcodeParam,
            bool bOnShelf,
            bool bModifyItemRecord,
            out List<string> DeletedNotifyRecPaths,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            long lRet = 0;
            DeletedNotifyRecPaths = null;

            // �����ض��߼�¼
            string strReaderXml = "";
            string strOutputReaderRecPath = "";

            // �Ӷ��߼�¼��
            if (bNeedLockReader == true)
                this.ReaderLocks.LockForWrite(strReservationReaderBarcode);
            try
            {
                // ������߼�¼
                nRet = this.GetReaderRecXml(
                    //sessioninfo,
                    channels,
                    strReservationReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "����֤����� '" + strReservationReaderBarcode + "' ������";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "������߼�¼ʱ��������: " + strError;
                    return -1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                    return -1;
                }

                byte[] timestamp = null;
                byte[] output_timestamp = null;
                string strOutputPath = "";

                RmsChannel channel = channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("reservations/request");
                XmlNode readerRequestNode = null;
                string strItems = "";
                for (int i = 0; i < nodes.Count; i++)
                {
                    readerRequestNode = nodes[i];
                    strItems = DomUtil.GetAttr(readerRequestNode, "items");
                    if (IsInBarcodeList(strItemBarcodeParam, strItems) == true)
                        goto FOUND;
                }

                return 0;   // not found request

            FOUND:
                Debug.Assert(readerRequestNode != null, "");

                // ����ز��е�<request>Ԫ�����
                string[] barcodes = strItems.Split(new char[] { ',' });
                for (int i = 0; i < barcodes.Length; i++)
                {
                    string strBarcode = barcodes[i].Trim();
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    if (strBarcode == strItemBarcodeParam
                        && bModifyItemRecord == false)
                    {
                        continue;   // �������¼�Ѿ��������
                    }

                    // ������¼
                    string strItemXml = "";
                    string strOutputItemRecPath = "";
                    // ��ò��¼
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   ����1��
                    //      >1  ���ж���1��
                    nRet = this.GetItemRecXml(
                        //sessioninfo,
                        channel,
                        strBarcode,
                        out strItemXml,
                        out strOutputItemRecPath,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "������� '" + strBarcode + "' ������";
                        continue;
                    }
                    if (nRet == -1)
                    {
                        strError = "������¼'" + strBarcode + "'ʱ��������: " + strError;
                        return -1;
                    }

                    XmlDocument itemdom = null;
                    nRet = LibraryApplication.LoadToDom(strItemXml,
                        out itemdom,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "װ�ز��¼����XML DOMʱ��������: " + strError;
                        return -1;
                    }

                    if (strBarcode == strItemBarcodeParam)
                    {
                        string strTempReservationReaderBarcode = "";
                        // ��������ǵ�ǰ���¼����Ҫ����arrived״̬
                        // ���Ϊ��ʧ�����������ĵ�������Ҫ֪ͨ�ȴ��ߣ����Ѿ���ʧ�ˣ������ٵȴ�
                        // parameters:
                        //      bMaskLocationReservation    ��Ҫ��<location>����#reservation���
                        // return:
                        //      -1  error
                        //      0   û���޸�
                        //      1   ���й��޸�
                        nRet = DoItemReturnReservationCheck(
                            bOnShelf,   // �������ڻ��ڼ��ϵ�ʱ�򣬲��¼�� <location> ��û�� #reservation�������ڽ��黷�ڼ����Ƿ�ԤԼ��ʱ�򣬾Ͳ���ֻ�� <location> ��
                            ref itemdom,
                            out strTempReservationReaderBarcode,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "�޸Ĳ��¼'" + strBarcode + "' ԤԼ����״̬ʱ(DoItemReturnReservationCheck)��������: " + strError;
                            return -1;
                        }
                        // �Բ��¼���û�иĶ�
                        if (nRet == 0)
                            continue;
                    }
                    else
                    {
                        // �����ͬһ�����е��������¼

                        // ɾ����Ӧ��<request>Ԫ��
                        XmlNode itemrequestnode = itemdom.DocumentElement.SelectSingleNode("reservations/request[@reader='" + strReservationReaderBarcode + "']");
                        if (itemrequestnode == null)
                            continue;

                        itemrequestnode.ParentNode.RemoveChild(itemrequestnode);
                    }

                    // д�ز��¼
                    lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                        itemdom.OuterXml,
                        false,
                        "content,ignorechecktimestamp",
                        timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "д�ز��¼'" + strBarcode + "' (��¼·��'" + strOutputItemRecPath + "')ʱ��������: " + strError;
                        return -1;
                    }
                } // end of for

                // ���߼�¼��Ϊ��Ӧ��<request>Ԫ�ش���״̬�Ǻ�
                DomUtil.SetAttr(readerRequestNode, "state", "arrived");
                // ����ʱ��
                DomUtil.SetAttr(readerRequestNode, "arrivedDate", this.Clock.GetClock());
                // ʵ�ʵ����һ��������� 2007/1/18 new add
                DomUtil.SetAttr(readerRequestNode, "arrivedItemBarcode", strItemBarcodeParam);

                // д�ض��߼�¼
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                strReaderXml = readerdom.DocumentElement.OuterXml;
            }
            finally
            {
                if (bNeedLockReader == true)
                    this.ReaderLocks.UnlockForWrite(strReservationReaderBarcode);
            }

            string strLibraryCode = "";
            nRet = this.GetLibraryCode(strOutputReaderRecPath,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                return -1;

            // ����һ��XML��¼, ����"ԤԼ����"��
            // �����¼ԤԼ������Ŀ�ģ���Ϊ���ù����߳̿��Լ�ض����Ƿ���ȡ�飬��������������ޣ�Ҫת��֪ͨ��һ��ԤԼ�˴˲�Ķ��ߡ�
            // ����email֪ͨ����
            nRet = AddNotifyRecordToQueue(
                channels,
                strItemBarcodeParam,
                "", // ��ʱ��ʹ�ô˲���
                bOnShelf,
                strLibraryCode,
                strReservationReaderBarcode,
                strReaderXml,
                out DeletedNotifyRecPaths,
                out strError);
            if (nRet == -1)
                return -1;

            if (this.Statis != null)
                this.Statis.IncreaseEntryValue(strLibraryCode,
    "����",
    "ԤԼ�����",
    1);

            return 1;
        }

        // ̽�⵱ǰ�ĵ�����п��Ƿ�߱� �ο�ID ���������
        bool ArrivedDbKeysContainsRefIDKey()
        {
            if (this.ArrivedDbFroms == null || this.ArrivedDbFroms.Length == 0)
                return false;
            foreach (BiblioDbFromInfo info in this.ArrivedDbFroms)
            {
                if (StringUtil.IsInList("item_refid", info.Style) == true)
                    return true;
            }

            return false;
        }

        // text-level: �ڲ�����
        // �� ԤԼ���� ���У�׷��һ���µļ�¼
        // ����email֪ͨ
        // ע������������Ҫɾ������֪ͨ��¼
        // parameters:
        //      strItemBarcode  ������š������ǲ�����š�����������Ϊ�գ��ο�ID��Ҫʹ�� strRefID ����
        //      strRefID        �ο�ID
        //      bOnShelf    Ҫ֪ͨ�Ĳ��Ƿ��ڼܡ��ڼ�ָ��û���˽��Ĺ���������������ϡ�
        //      strLibraryCode  �������ڵĹݴ���
        //      strReaderXml    ԤԼ��ͼ��Ķ��ߵ�XML��¼��������Ϣ֪ͨ�ӿ�
        int AddNotifyRecordToQueue(
            RmsChannelCollection channels,
            string strItemBarcode,
            string strRefID,
            bool bOnShelf,
            string strLibraryCode,
            string strReaderBarcode,
            string strReaderXml,
            out List<string> DeletedNotifyRecPaths,
            out string strError)
        {
            strError = "";
            DeletedNotifyRecPaths = new List<string>();
            
            // 2010/12/31
            if (String.IsNullOrEmpty(this.ArrivedDbName) == true)
            {
                strError = "ԤԼ�������δ����, AddNotifyRecordToQueue()����ʧ��";
                return -1;
            }

            // ׼��д��¼
            byte[] timestamp = null;
            byte[] output_timestamp = null;
            string strOutputPath = "";
            int nRet = 0;
            long lRet = 0;


            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                // ��������õĲ������Ϊ�գ����϶����н�����������ޣ��Ǿͻ����ϵͳ���ط�æ��
                strError = "����strItemBarcode�еĲ�����Ų���Ϊ�ա�";
                return -1;
            }

            RmsChannel channel = channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

        REDODELETE:
            // ����������Ѿ�����ͬ������ŵļ�¼, Ҫ��ɾ��
            string strNotifyXml = "";
            // ���ԤԼ������м�¼
            // return:
            //      -1  error
            //      0   not found
            //      1   ����1��
            //      >1  ���ж���1��
            nRet = GetArrivedQueueRecXml(
                channels,
                strItemBarcode,
                out strNotifyXml,
                out timestamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                // д�������־?
                this.WriteErrorLog("�ڻ�������У������������Ϊ " + strItemBarcode + " ��ԤԼ������¼ʱ����: " + strError);
            }
            if (nRet >= 1)
            {
                int nRedoDeleteCount = 0;
            // TODO: ��һ��ɾ���������ר�ű�����һ�������У�������ô����ѭ���������Ż�����
            REDO_DELETE:
                lRet = channel.DoDeleteRes(strOutputPath,
                    timestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    // ʱ�����ƥ��
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                        && nRedoDeleteCount < 10)
                    {
                        nRedoDeleteCount++;
                        timestamp = output_timestamp;
                        goto REDO_DELETE;
                    }

                    // д�������־?
                    this.WriteErrorLog("�ڻ�������У�������ԤԼ�����¼ǰ, ɾ���Ѵ��ڵ�ԤԼ������¼ '" + strOutputPath + "' ����: " + strError);
                }

                DeletedNotifyRecPaths.Add(strOutputPath);    // �����Ѿ���ɾ���ļ�¼·�� 2007/7/5 new add

                goto REDODELETE;    // ����ж�����ѭ��ɾ��
            }


            // ����ԤԼ�����¼
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // TODO: �Ժ����� <refID> Ԫ�أ��洢���¼�Ĳο�ID

#if NO
            XmlNode nodeItemBarcode = DomUtil.SetElementText(dom.DocumentElement, "itemBarcode", strItemBarcode);

            // ��<itemBarcode>Ԫ��������һ��onShelf���ԣ���ʾ�����ڼ����
            Debug.Assert(nodeItemBarcode != null, "");
            if (bOnShelf == true)
                DomUtil.SetAttr(nodeItemBarcode, "onShelf", "true");
#endif
            // ���� strItemBarcode �к���ǰ׺���÷�
            string strHead = "@refID:";
            if (StringUtil.HasHead(strItemBarcode, strHead, true) == true)
            {
                strRefID = strItemBarcode.Substring(strHead.Length);
                strItemBarcode = "";
            }

            if (this.ArrivedDbKeysContainsRefIDKey() == true)
            {
                DomUtil.SetElementText(dom.DocumentElement, "itemBarcode", strItemBarcode);
                DomUtil.SetElementText(dom.DocumentElement, "refID", strRefID);
            }
            else
            {
                if (string.IsNullOrEmpty(strItemBarcode) == true)
                {
                    if (string.IsNullOrEmpty(strRefID) == true)
                    {
                        strError = "AddNotifyRecordToQueue() ������ strItemBarcode ����Ϊ�յ�ʱ�򣬱����� strRefID ������Ϊ��";
                        return -1;
                    }

                    Debug.Assert(string.IsNullOrEmpty(strRefID) == false, "");
                    // �ɵ��÷����������ʱ��鲻��
                    DomUtil.SetElementText(dom.DocumentElement, "itemBarcode", "@refID:" + strRefID);
                }
                else
                    DomUtil.SetElementText(dom.DocumentElement, "itemBarcode", strItemBarcode); // 2015/5/20 ��ӣ����� BUG
            }

            // ��Ϊ�洢��Ԫ���� 2015/5/7
            if (bOnShelf == true)
                DomUtil.SetElementText(dom.DocumentElement, "onShelf", "true");

            // 2012/10/26
            DomUtil.SetElementText(dom.DocumentElement, "libraryCode", strLibraryCode);

            DomUtil.SetElementText(dom.DocumentElement, "readerBarcode", strReaderBarcode);
            DomUtil.SetElementText(dom.DocumentElement, "notifyDate", this.Clock.GetClock());

            string strPath = this.ArrivedDbName + "/?";

            // д�¼�¼
            lRet = channel.DoSaveTextRes(
                strPath,
                dom.OuterXml,
                false,
                "content,ignorechecktimestamp",
                timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                // д�������־ 2007/1/3 new add
                this.WriteErrorLog("�����µ�ԤԼ������м�¼ʱ����: " + strError);
                return -1;
            }

            string strReaderEmailAddress = "";
            string strName = "";
            nRet = GetReaderNotifyInfo(
                strReaderXml,
                out strName,
                out strReaderEmailAddress,
                out strError);
            if (nRet == -1)
                return -1;

            // ���ͼ��ժҪ��Ϣ
            string strSummary = "";
            string strBiblioRecPath = "";

            nRet = this.GetBiblioSummary(strItemBarcode,
                "", //  strConfirmItemRecPath,
                null,   //  strBiblioRecPathExclude,
                25,
                out strBiblioRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
            {
                strSummary = "ERROR: " + strError;
            }
#if NO
            // ��ʱ��SessionInfo����
            SessionInfo sessioninfo = new SessionInfo(this);
            // ģ��һ���˻�
            Account account = new Account();
            account.LoginName = "CacheBuilder";
            account.Password = "";
            account.Rights = "getbibliosummary";

            account.Type = "";
            account.Barcode = "";
            account.Name = "AddNotifyRecordToQueue";
            account.UserID = "AddNotifyRecordToQueue";
            account.RmsUserName = this.ManagerUserName;
            account.RmsPassword = this.ManagerPassword;

            sessioninfo.Account = account;
            try
            {

                string strBiblioRecPath = "";
                LibraryServerResult result = this.GetBiblioSummary(
                    sessioninfo,
                    strItemBarcode,
                    "", // strConfirmItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1)
                {
                    strSummary = "ERROR: " + result.ErrorInfo;
                }
                else
                {
                    // �ض�
                    if (strSummary.Length > 25)
                        strSummary = strSummary.Substring(0, 25) + "...";
                }
            }
            finally
            {
                sessioninfo.Close();
                sessioninfo = null;
            }
#endif


            // ���Ͷ���Ϣ֪ͨ
            string strTotalError = "";

            // *** dpmail
            if (this.MessageCenter != null)
            {
                string strTemplate = "";
                // ����ʼ�ģ��
                nRet = GetMailTemplate(
                    "dpmail",
                    bOnShelf == false ? "ԤԼ����֪ͨ" : "ԤԼ����֪ͨ(�ڼ�)",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "ԤԼ����֪ͨ<mailTemplate/template>��δ���á�";
                    return -1;
                }

                /*
                %item%  ����Ϣ
                %reservetime%   ��������
                %today% ����email�ĵ���
                %summary% ��ĿժҪ
                %itembarcode% ������� 
                %name% ��������
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(�������Ϊ: " + strItemBarcode + " URLΪ: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strSummary;
                table["%itembarcode%"] = strItemBarcode;
                table["%name%"] = strName;
                string strBody = "";


                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (String.IsNullOrEmpty(this.MessageCenter.MessageDbName) == false)
                {
                    // ������Ϣ
                    nRet = this.MessageCenter.SendMessage(
                        channels,
                        strReaderBarcode,
                        "ͼ���",
                        "ԤԼ����֪ͨ",
                        "text",
                        strBody,
                        false,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "����dpmail��Ϣʱ����: " + strError + "\r\n";
                    }
                }
            }

            // ** email
            if (String.IsNullOrEmpty(strReaderEmailAddress) == false)
            {
                string strTemplate = "";
                // ����ʼ�ģ��
                nRet = GetMailTemplate(
                    "email",
                    bOnShelf == false ? "ԤԼ����֪ͨ" : "ԤԼ����֪ͨ(�ڼ�)",
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "ԤԼ����֪ͨ<mailTemplate/template>��δ���á�";
                    return -1;
                }

                /*
                %item%  ����Ϣ
                %reservetime%   ��������
                %today% ����email�ĵ���
                %summary% ��ĿժҪ
                %itembarcode% ������� 
                %name% ��������
                 * */
                Hashtable table = new Hashtable();
                table["%item%"] = "(�������Ϊ: " + strItemBarcode + " URLΪ: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                table["%today%"] = DateTime.Now.ToString();
                table["%summary%"] = strSummary;
                table["%itembarcode%"] = strItemBarcode;
                table["%name%"] = strName;

                string strBody = "";


                nRet = GetMailText(strTemplate,
                    table,
                    out strBody,
                    out strError);
                if (nRet == -1)
                    return -1;

                {
                    // ����email
                    // return:
                    //      -1  error
                    //      0   not found smtp server cfg
                    //      1   succeed
                    nRet = SendEmail(strReaderEmailAddress,
                        "ԤԼ����֪ͨ",
                        strBody,
                        "text",
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "����email��Ϣʱ����: " + strError + "\r\n";
                    }
                }
            }

            // *** external messageinterfaces
            if (this.m_externalMessageInterfaces != null)
            {
                foreach (MessageInterface message_interface in this.m_externalMessageInterfaces)
                {
                    string strTemplate = "";
                    // ����ʼ�ģ��
                    nRet = GetMailTemplate(
                        message_interface.Type,
                        bOnShelf == false ? "ԤԼ����֪ͨ" : "ԤԼ����֪ͨ(�ڼ�)",
                        out strTemplate,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "ԤԼ����֪ͨ<mailTemplate/template>��δ���á�";
                        return -1;
                    }

                    /*
                    %item%  ����Ϣ
                    %reservetime%   ��������
                    %today% ����email�ĵ���
                %summary% ��ĿժҪ
                %itembarcode% ������� 
                %name% ��������
                     * */
                    Hashtable table = new Hashtable();
                    table["%item%"] = "(�������Ϊ: " + strItemBarcode + " URLΪ: " + this.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + " )";
                    table["%reservetime%"] = this.GetDisplayTimePeriodStringEx(this.ArrivedReserveTimeSpan);
                    table["%today%"] = DateTime.Now.ToString();
                    table["%summary%"] = strSummary;
                    table["%itembarcode%"] = strItemBarcode;
                    table["%name%"] = strName;

                    string strBody = "";

                    nRet = GetMailText(strTemplate,
                        table,
                        out strBody,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // ������Ϣ
                    nRet = message_interface.HostObj.SendMessage(
                        strReaderBarcode,
                        strReaderXml,
                        strBody,
                        strLibraryCode,
                        out strError);
                    if (nRet == -1)
                    {
                        strTotalError += "����"+message_interface.Type+"��Ϣʱ����: " + strError + "\r\n";
                    }
                }
            }

            if (String.IsNullOrEmpty(strTotalError) == false)
            {
                strError = strTotalError;
                return -1;
            }

            return 0;
        }

        // text-level: �ڲ�����
        // ��úͶ���֪ͨ�йص���Ϣ
        int GetReaderNotifyInfo(
            string strReaderXml,
            out string strName,
            out string strReaderEmailAddress,
            out string strError)
        {
            strError = "";
            strReaderEmailAddress = "";
            strName = "";

            XmlDocument readerdom = null;
            int nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                return -1;
            }

            strReaderEmailAddress = DomUtil.GetElementText(readerdom.DocumentElement,
                "email");

            strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");

            return 0;
        }

#if NO
        // text-level: �ڲ�����
        // ��úͶ���֪ͨ�йص���Ϣ
        int GetReaderNotifyInfo(
            RmsChannelCollection channels,
            string strReaderBarcode,
            out string strName,
            out string strReaderEmailAddress,
            out string strError)
        {
            strError = "";
            strReaderEmailAddress = "";
            strName = "";

            // ������߼�¼
            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            byte[] reader_timestamp = null;
            int nRet = this.GetReaderRecXml(
                channels,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "���߼�¼ '" + strReaderBarcode + "' û���ҵ���";
                return -1;
            }
            if (nRet == -1)
            {
                strError = "������߼�¼ʱ��������: " + strError;
                return -1;
            }

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                return -1;
            }

            strReaderEmailAddress = DomUtil.GetElementText(readerdom.DocumentElement,
                "email");

            strName = DomUtil.GetElementText(readerdom.DocumentElement,
                "name");

            return 0;
        }

#endif

        // text-level: �ڲ�����
        // ֪ͨ��һ��ԤԼ�ߣ�����(��û����һ��ԤԼ����)�����
        // ����ǰ����Ҫ�Ȼ��ԤԼ���м�¼
        // return:
        //      -1  error
        //      0   ����
        //      1   �����Ѿ�û��ԤԼ�ߣ���֪ͨ����Ա���
        public int DoNotifyNext(
            RmsChannelCollection channels,
            string strQueueRecPath,
            XmlDocument queue_rec_dom,
            byte[] baQueueRecTimeStamp,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = null;
            byte[] output_timestamp = null;

            string strState = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "state");

            string strNotifyDate = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "notifyDate");
            // XmlNode nodeItemBarcode = null;
            string strItemBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "itemBarcode"/*, out nodeItemBarcode*/);

            // 2015/5/7
            string strRefID = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
    "refID");

            string strReaderBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "readerBarcode");

            bool bOnShelf = false;
#if NO
            // <itemBarcode>Ԫ���Ƿ���onShelf���ԡ�
            if (nodeItemBarcode != null)
            {
                string strOnShelf = DomUtil.GetAttr(nodeItemBarcode, "onShelf");
                if (strOnShelf.ToLower() == "true"
                    || strOnShelf.ToLower() == "yes"
                    || strOnShelf.ToLower() == "on")
                    bOnShelf = true;
            }
#endif
            // 2015/5/7
            // <onShelf> Ԫ��
            {
                string strOnShelf = DomUtil.GetElementText(queue_rec_dom.DocumentElement, "onShelf");
                if (DomUtil.IsBooleanTrue(strOnShelf, false) == true)
                    bOnShelf = true;
            }

            // 2015/5/7
            if (string.IsNullOrEmpty(strItemBarcode) == true && string.IsNullOrEmpty(strRefID) == false)
            {
                strItemBarcode = "@refID:" + strRefID;
            }

            // Ҫ֪ͨ��һλԤԼ��

            string strReservationReaderBarcode = "";

            // ������ߺͲ��¼�е��ѵ�ԤԼ�������ȡ��һ��ԤԼ����֤�����
            // ������������������¼����ǰ������state=arrived��<request>Ԫ��
            nRet = this.ClearArrivedInfo(
                channels,
                strReaderBarcode,
                strItemBarcode,
                bOnShelf,
                out strReservationReaderBarcode,
                out strError);
            if (nRet == -1)
                return -1;


            // 3) ֪ͨԤԼ����Ĳ���
            List<string> DeletedNotifyRecPaths = null;  // ��ɾ����֪ͨ��¼��

            if (String.IsNullOrEmpty(strReservationReaderBarcode) == false)
            {
                // ֪ͨ��һ����

                // ���ڶԶ��߿��������ı�������, �������˴˺���
                // return:
                //      -1  error
                //      0   û���ҵ�<request>Ԫ��
                //      1   �ѳɹ�����
                nRet = this.DoReservationNotify(
                    channels,
                    strReservationReaderBarcode,
                    true,
                    strItemBarcode,
                    bOnShelf,
                    false,   // ��Ҫ�޸ĵ�ǰ���¼��<request> state���ԣ���Ϊǰ��ClearArrivedInfo()���Ѿ�������DoItemReturnReservationCheck(), �޸��˵�ǰ���<request> state���ԡ�
                    out DeletedNotifyRecPaths,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                // outof �ļ�¼��ʱɾ����
                // ���¼�еĹݲصص� #reservation��ʱ������һ�������ھ�������һ�����̵�ģ��ɨ������ʱ������

                // �Ѽ�¼״̬�޸�Ϊ outofreservation
                DomUtil.SetElementText(queue_rec_dom.DocumentElement,
                    "state",
                    "outof");

                channel = channels.GetChannel(this.WsUrl);

                string strOutputPath = "";
                lRet = channel.DoSaveTextRes(strQueueRecPath,
                    queue_rec_dom.OuterXml,
                    false,
                    "content,ignorechecktimestamp",
                    baQueueRecTimeStamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "д��ԤԼ�����¼ '" + strQueueRecPath + "' ʱ��������: " + strError;
                    return -1;
                }

                // TODO: ֪ͨ��Ա�����ϼܲ���
                // ������ϵͳĳĿ¼����׷�ӵ�һ���ı��ļ���������Ա���Բ�ԡ�
                // ��ʽ��ÿ�� ������� ���һ��ԤԼ�Ķ���֤�����
                if (String.IsNullOrEmpty(this.StatisDir) == false)
                {
                    string strLogFileName = this.StatisDir + "\\outof_reservation_" + Statis.GetCurrentDate() + ".txt";
                    StreamUtil.WriteText(strLogFileName, strItemBarcode + " " + strReaderBarcode + "\r\n");
                }

                return 1;
            }

            // 4) ɾ����ǰ֪ͨ��¼

            // 2007/7/5 new add
            bool bAlreadeDeleted = false;
            if (DeletedNotifyRecPaths != null)
            {
                if (DeletedNotifyRecPaths.IndexOf(strQueueRecPath) != -1)
                    bAlreadeDeleted = true;
            }

            if (bAlreadeDeleted == false)
            {
                channel = channels.GetChannel(this.WsUrl);

                lRet = channel.DoDeleteRes(
                    strQueueRecPath,
                    baQueueRecTimeStamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "DoNotifyNext()ɾ��֪ͨ��¼ '" + strQueueRecPath + "' ʱʧ��: " + strError;
                    return -1;
                }
            }

            return 0;
        }
    }
}
