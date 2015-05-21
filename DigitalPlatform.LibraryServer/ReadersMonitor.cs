using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// ��ض�����Ϣ���߳� �����ؼ��������ĳ���δ����ˢ����ͣ������Ϣ
    /// </summary>
    public class ReadersMonitor : BatchTask
    {
        public ReadersMonitor(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "����֪ͨ";
            }
        }



        // һ�β���ѭ��
        // TODO: �Ƿ���Ҫ�Զ��߼�¼������
        public override void Worker()
        {
            // ϵͳ�����ʱ�򣬲����б��߳�
            // 2007/12/18 new add
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;
            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            string strError = "";
            int nRet = 0;

            bool bPerDayStart = false;  // �Ƿ�Ϊÿ��һ������ģʽ
            string strMonitorName = "readersMonitor";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strMonitorName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "���ļ��л�ȡ "+strMonitorName+" ÿ������ʱ��ʱ��������: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                string strStartTimeDef = "";
                //      bRet    �Ƿ���ÿ������ʱ��
                bool bRet = false;
                string strOldLastTime = strLastTime;

                // return:
                //      -1  error
                //      0   û���ҵ�startTime���ò���
                //      1   �ҵ���startTime���ò���
                nRet = IsNowAfterPerDayStart(
                    strMonitorName,
                    ref strLastTime,
                    out bRet,
                    out strStartTimeDef,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "��ȡ "+strMonitorName+" ÿ������ʱ��ʱ��������: " + strError;
                    this.AppendResultText(strErrorText + "\r\n");
                    this.App.WriteErrorLog(strErrorText);
                    return;
                }

                // ���nRet == 0����ʾû��������ز����������ԭ����ϰ�ߣ�ÿ�ζ���
                if (nRet == 0)
                {

                }
                else if (nRet == 1)
                {
                    bPerDayStart = true;

                    if (bRet == false)
                    {
                        if (this.ManualStart == true)
                            this.AppendResultText("����̽�������� '" + this.Name + "'������û�е�ÿ������ʱ�� " + strStartTimeDef + " ��δ��������(�ϴ����������ʱ��Ϊ " + DateTimeUtil.LocalTime(strLastTime) + ")\r\n");

                        // 2014/3/31
                        if (string.IsNullOrEmpty(strOldLastTime) == true
                            && string.IsNullOrEmpty(strLastTime) == false)
                        {
                            this.AppendResultText("ʷ���״������������Ѱѵ�ǰʱ�䵱���ϴ����������ʱ�� " + DateTimeUtil.LocalTime(strLastTime) + " д���˶ϵ�����ļ�\r\n");
                            WriteLastTime(strMonitorName, strLastTime);
                        }

                        return; // ��û�е�ÿ��ʱ��
                    }
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(��ʱ)" : "(����ʱ)") + strMonitorName + " ������");
            }

            this.AppendResultText("��ʼ��һ��ѭ��\r\n");

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            int nTotalRecCount = 0;

            for (int i = 0; i < this.App.ReaderDbs.Count; i++)
            {
#if NO
                // ϵͳ�����ʱ�򣬲����б��߳�
                // 2008/5/27
                if (this.App.HangupReason == HangupReason.LogRecover)
                    break;
                // 2012/2/4
                if (this.App.PauseBatchTask == true)
                    break;
#endif
                if (this.Stopped == true)
                    break;

                if (this.Stopped == true)
                    break;

                string strReaderDbName = this.App.ReaderDbs[i].DbName;

                AppendResultText("��ʼ������߿� " + strReaderDbName + " ��ѭ��\r\n");

                bool bFirst = true; // 2008/5/27 moved
                string strID = "1";
                int nOnePassRecCount = 0;
                for (; ; nOnePassRecCount++, nTotalRecCount++)
                {
#if NO
                    // ϵͳ�����ʱ�򣬲����б��߳�
                    // 2008/2/4
                    if (this.App.HangupReason == HangupReason.LogRecover)
                        break;
                    // 2012/2/4
                    if (this.App.PauseBatchTask == true)
                        break;
#endif
                    if (this.Stopped == true)
                        break;

                    string strStyle = "";
                    strStyle = "data,content,timestamp,outputpath";

                    if (bFirst == true)
                        strStyle += "";
                    else
                    {
                        strStyle += ",next";
                    }

                    string strPath = strReaderDbName + "/" + strID;

                    string strXmlBody = "";
                    string strMetaData = "";
                    string strOutputPath = "";
                    byte[] baOutputTimeStamp = null;

                    // 
                    SetProgressText((nOnePassRecCount + 1).ToString() + " " + strPath);

                    // �����Դ
                    // return:
                    //		-1	�����������ԭ����this.ErrorCode�С�this.ErrorInfo���г�����Ϣ��
                    //		0	�ɹ�
                    long lRet = channel.GetRes(strPath,
                        strStyle,
                        out strXmlBody,
                        out strMetaData,
                        out baOutputTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            if (bFirst == true)
                            {
                                // ��һ��û���ҵ�, ����Ҫǿ��ѭ������
                                bFirst = false;
                                goto CONTINUE;
                            }
                            else
                            {
                                if (bFirst == true)
                                {
                                    strError = "���ݿ� " + strReaderDbName + " ��¼ " + strID + " �����ڡ����������";
                                }
                                else
                                {
                                    strError = "���ݿ� " + strReaderDbName + " ��¼ " + strID + " ����ĩһ����¼�����������";
                                }
                                break;
                            }

                        }
                        else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                        {
                            bFirst = false;
                            // ��id��������
                            strID = ResPath.GetRecordId(strOutputPath);
                            goto CONTINUE;

                        }

                        goto ERROR1;
                    }

                    string strLibraryCode = "";
                    nRet = this.App.GetLibraryCode(strOutputPath,
                        out strLibraryCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    bFirst = false;

                    this.AppendResultText("���ڴ���" + (nOnePassRecCount + 1).ToString() + " " + strOutputPath + "\r\n");

                    // ��id��������
                    strID = ResPath.GetRecordId(strOutputPath);

                    // ����
                    nRet = DoOneRecord(
                        strOutputPath,
                        strLibraryCode,
                        strXmlBody,
                        baOutputTimeStamp,
                        out strError);
                    if (nRet == -1)
                    {
                        AppendResultText("DoOneRecord() error : " + strError + "��\r\n");
                    }

                CONTINUE:
                    continue;

                } // end of for

                AppendResultText("��Զ��߿� " + strReaderDbName + " ��ѭ�������������� " + nOnePassRecCount.ToString() + " ����¼��\r\n");

            }
            AppendResultText("ѭ�������������� " + nTotalRecCount.ToString() + " ����¼��\r\n");
            SetProgressText("ѭ�������������� " + nTotalRecCount.ToString() + " ����¼��");

            {
                Debug.Assert(this.App != null, "");

                // д���ļ��������Ѿ������ĵ���ʱ��
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime());  // 2007/12/17 changed // DateTime.UtcNow
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(��ʱ)" : "(����ʱ)") + strMonitorName + "�������������¼ " + nTotalRecCount.ToString() + " ����";
                this.App.WriteErrorLog(strErrorText);

            }

            return;
        ERROR1:
            AppendResultText("ReadersMonitor thread error : " + strError + "\r\n");
            this.App.WriteErrorLog("ReadersMonitor thread error : " + strError + "\r\n");
            return;
        }

        // ����һ����¼
        int DoOneRecord(
            string strPath,
            string strLibraryCode,
            string strReaderXml,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);
            int nRedoCount = 0;

            REDO:

            byte[] output_timestamp = null;

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "װ��XML��DOM����: " + ex.Message;
                return -1;
            }

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement,
                "barcode");

            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            Calendar calendar = null;
            nRet = this.App.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out calendar,
                out strError);
            if (nRet == -1)
            {
                strError = "��ö������� '"+strReaderType+"' �������������ʧ��: " + strError;
                return -1;
            }

            bool bChanged = false;

            List<string> bodytypes = new List<string>();
            bodytypes.Add("dpmail");
            bodytypes.Add("email");
            if (this.App.m_externalMessageInterfaces != null)
            {
                foreach(MessageInterface message_interface in this.App.m_externalMessageInterfaces)
                {
                    bodytypes.Add(message_interface.Type);
                }
            }

            // ÿ�� bodytype ��һ��
            for (int i = 0; i < bodytypes.Count; i++)
            {
                string strBodyType = bodytypes[i];

                string strReaderEmailAddress = "";
                if (strBodyType == "email")
                {
                    strReaderEmailAddress = DomUtil.GetElementText(readerdom.DocumentElement,
                        "email");
                    // ���߼�¼��û��email��ַ�����޷�����email��ʽ��֪ͨ��
                    if (String.IsNullOrEmpty(strReaderEmailAddress) == true)
                        continue;
                }

                if (strBodyType == "dpmail")
                {
                    if (this.App.MessageCenter == null)
                    {
                        continue;
                    }
                }

#if NO
                List<string> notifiedBarcodes = new List<string>();


                // ����ض����͵���֪ͨ���Ĳ�������б�
                // return:
                //      -1  error
                //      ����    notifiedBarcodes������Ÿ���
                nRet = GetNotifiedBarcodes(readerdom,
                    strBodyType,
                    out notifiedBarcodes,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif

                int nResultValue = 0;
                string strBody = "";
                // List<string> wantNotifyBarcodes = null;
                string strMime = "";

                // ������ýű�ǰ�Ķ��߼�¼ 
                string strOldReaderXml = readerdom.DocumentElement.OuterXml;

                // ִ�нű�����NotifyReader
                // parameters:
                // return:
                //      -2  not found script
                //      -1  ����
                //      0   �ɹ�
                // nResultValue
                //      -1  ����
                //      0   û�б�Ҫ����
                //      1   ��Ҫ����
                nRet = this.App.DoNotifyReaderScriptFunction(
                        readerdom,
                        calendar,
                        // notifiedBarcodes,
                        strBodyType,
                        out nResultValue,
                        out strBody,
                        out strMime,
                        // out wantNotifyBarcodes,
                        out strError);
                if (nRet == -1)
                {
                    this.AppendResultText("DoNotifyReaderScriptFunction [barcode=" + strReaderBarcode + "] error: " + strError + "\r\n");
                    continue;
                }

                // 2010/12/18
                // �����ܷ�������Ϊ������NotifyReader()������
                if (nRet == -2)
                    break;

                if (nResultValue == -1)
                {
                    this.AppendResultText("DoNotifyReaderScriptFunction [strReaderBarcode=" + strReaderBarcode + "] nResultValue == -1, errorinfo: " + strError + "\r\n");
                    continue;
                }

                if (nResultValue == 0)
                {
                    // ��Ҫ�����ʼ�
                    continue;
                }

                bool bSendMessageError = false;

                if (nResultValue == 1 && nRedoCount == 0)   // 2008/5/27 changed ������ʱ�򣬲��ٷ�����Ϣ��������Ϣ���¼����
                {
                    // �����ʼ�

                    if (strBodyType == "dpmail")
                    {
                        // ������Ϣ
                        // return:
                        //      -1  ����
                        //      0   �ɹ�
                        nRet = this.App.MessageCenter.SendMessage(
                            this.RmsChannels,
                            strReaderBarcode,
                            "ͼ���",
                            "������Ϣ��ʾ",
                            strMime,    // "text",
                            strBody,
                            false,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "����dpmail����: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(strLibraryCode,
                                "����֪ͨ",
                                "dpmail message ����֪ͨ��Ϣ���ʹ�����",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "����֪ͨ",
                                "dpmail����֪ͨ����",
                                1);
                        }
                    }

                    MessageInterface external_interface = this.App.GetMessageInterface(strBodyType);

                    if (external_interface != null)
                    {
                        // ������Ϣ
                        try
                        {
                            // ����һ����Ϣ
                            // parameters:
                            //      strPatronBarcode    ����֤�����
                            //      strPatronXml    ���߼�¼XML�ַ����������Ҫ��֤����������ĳЩ�ֶ���ȷ����Ϣ���͵�ַ�����Դ�XML��¼��ȡ
                            //      strMessageText  ��Ϣ����
                            //      strError    [out]���ش����ַ���
                            // return:
                            //      -1  ����ʧ��
                            //      0   û�б�Ҫ����
                            //      >=1   ���ͳɹ�������ʵ�ʷ��͵���Ϣ����
                            nRet = external_interface.HostObj.SendMessage(
                                strReaderBarcode,
                                readerdom.DocumentElement.OuterXml,
                                strBody,
                                strLibraryCode,
                                out strError);
                        }
                        catch (Exception ex)
                        {
                            strError = external_interface.Type + " ���͵��ⲿ��Ϣ�ӿ�Assembly��SendMessage()�����׳��쳣: " + ex.Message;
                            nRet = -1;
                        }
                        if (nRet == -1)
                        {
                            strError = "����� '"+strReaderBarcode+"' ����" + external_interface.Type + " messageʱ����: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "����֪ͨ",
                                external_interface.Type + " message ����֪ͨ��Ϣ���ʹ�����",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else if (nRet >= 1)
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(strLibraryCode,
                                "����֪ͨ", 
                                external_interface.Type + " message ����֪ͨ����",
                                1);
                        }
                    }

                    if (strBodyType == "email")
                    {
                        // ����email
                        // return:
                        //      -1  error
                        //      0   not found smtp server cfg
                        //      1   succeed
                        nRet = this.App.SendEmail(strReaderEmailAddress,
                            "������Ϣ��ʾ",
                            strBody,
                            strMime,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "����email����: " + strError;
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "����֪ͨ",
                                "email message ����֪ͨ��Ϣ���ʹ�����",
                                1);
                            this.AppendResultText(strError + "\r\n");
                            bSendMessageError = true;
                            // return -1;

                            this.App.WriteErrorLog(strError);
                            readerdom = new XmlDocument();
                            readerdom.LoadXml(strOldReaderXml);
                        }
                        else if (nRet == 1)
                        {
                            if (this.App.Statis != null)
                                this.App.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "����֪ͨ",
                                "email����֪ͨ����", 
                                1);
                        }
                    }

                }

                if (bSendMessageError == false)
                {
#if NO
                    // �ڶ��߼�¼�б�ǳ���Щ�Ѿ����͹�֪ͨ�Ĳᣬ�����Ժ��ظ�֪ͨ
                    // return:
                    //      -1  error
                    //      0   û���޸�
                    //      1   �������޸�
                    nRet = MaskSendItems(
                        ref readerdom,
                        strBodyType,
                        wantNotifyBarcodes,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (nRet == 1)
                        bChanged = true;
#endif

                    bChanged = true;
                }
            } // end of for

            // ˢ����ͣ�������
            // 2007/12/17 new add
            if (StringUtil.IsInList("pauseBorrowing", this.App.OverdueStyle) == true)
            {
                //
                // ������ͣ������
                // return:
                //      -1  error
                //      0   readerdomû���޸�
                //      1   readerdom�������޸�
                nRet = this.App.ProcessPauseBorrowing(
                    strLibraryCode,
                    ref readerdom,
                    strPath,
                    "#readersMonitor",
                    "refresh",
                    "", // ��Ϊ�ǻ����������Բ�����IP��ַ
                    out strError);
                if (nRet == -1)
                {
                    strError = "��refresh��ͣ����Ĺ����з�������: " + strError;
                    this.AppendResultText(strError + "\r\n");
                }

                if (nRet == 1)
                    bChanged = true;
            }

            // �޸Ķ��߼�¼����
            if (bChanged == true)
            {
                string strOutputPath = "";
                lRet = channel.DoSaveTextRes(strPath,
                    readerdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    baTimeStamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    // ʱ�����ͻ
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount > 10)    // 2008/5/27 new add
                        {
                            strError = "ReadersMonitor д�ض��߿��¼ '" + strPath + "' ʱ����ʱ�����ͻ������10�κ��Է���ʱ�����ͻ: " + strError;
                            return -1;
                        }

                        string strStyle = "data,content,timestamp,outputpath";

                        string strMetaData = "";
                        // string strOutputPath = "";
                        lRet = channel.GetRes(strPath,
                            strStyle,
                            out strReaderXml,
                            out strMetaData,
                            out baTimeStamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "д�ض��߿��¼ '" + strPath + "' ʱ����ʱ�����ͻ����װ��¼ʱ�ַ�������: " + strError;
                            return -1;
                        }

                        nRedoCount++;
                        goto REDO;
                    }
                    
                    strError = "д�ض��߿��¼ '" + strPath + "' ʱ��������: " + strError;
                    return -1;
                }
            }

            return 0;
        }

#if NO
        // ����ض����͵���֪ͨ���Ĳ�������б�
        // return:
        //      -1  error
        //      ����    notifiedBarcodes������Ÿ���
        int GetNotifiedBarcodes(XmlDocument readerdom,
            string strBodyType,
            out List<string> notifiedBarcodes,
            out string strError)
        {
            strError = "";
            notifiedBarcodes = new List<string>();

            // �г�ȫ�����ĵĲ�
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strItemBarcode = DomUtil.GetAttr(node, "barcode");

                if (String.IsNullOrEmpty(strItemBarcode) == true)
                    continue;

                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                bool bNotified = IsNotified(strBodyType,
                    strHistory);
                if (bNotified == false)
                    continue;

                notifiedBarcodes.Add(strItemBarcode);
            }

            return notifiedBarcodes.Count;
        }
#endif

#if NO
        // �ڶ��߼�¼�б�ǳ���Щ�Ѿ����͹�֪ͨ�Ĳᣬ�����Ժ��ظ�֪ͨ
        // return:
        //      -1  error
        //      0   û���޸�
        //      1   �������޸�
        int MaskSendItems(
            ref XmlDocument readerdom,
            string strBodyType,
            List<string> wantNotifyBarcodes,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            bool bChanged = false;

            for (int i = 0; i < wantNotifyBarcodes.Count; i++)
            {
                string strItemBarcode = wantNotifyBarcodes[i];

                XmlNode node = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='"+strItemBarcode+"']");
                if (node == null)
                {
                    strError = "������� '" + strItemBarcode + "' �ڶ��߼�¼�о�Ȼû���ҵ���Ӧ��<borrows/borrow>Ԫ�ء�";
                    return -1;
                }

                string strHistory = DomUtil.GetAttr(node, "notifyHistory");

                nRet = ModifyHistoryString(strBodyType,
                    ref strHistory,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ModifyHistoryString() error : " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    bChanged = true;
                    DomUtil.SetAttr(node, "notifyHistory", strHistory);
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // ���һ�� body type ��ȫ��֪ͨ�ַ�
        public static string GetNotifiedChars(LibraryApplication app,
            string strBodyType,
            string strHistory)
        {
            int nExtendCount = 0;   // ��չ�ӿڵĸ���
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // ÿ��С���ֵĳ���

            int index = -1; // 0: dpmail; 1: email; >=2: �����������Ϣ�ӿڷ�ʽ
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    // strError = "����ʶ��� message type '" + strBodyType + "'";
                    // return -1;
                    return null;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    // strError = "external_interface (type '" + external_interface.Type + "') û���� m_externalMessageInterfaces �������ҵ�";
                    // return -1;
                    return null;
                }
                index += 2;
            }

            string strResult = "";
            for (int i = 0; i < strHistory.Length / nSegmentLength; i++)
            {
                int nStart = i * nSegmentLength;
                int nLength = nSegmentLength;
                if (nStart + nLength > strHistory.Length)
                    nLength = strHistory.Length - nStart;

                string strSegment = strHistory.Substring(nStart, nLength);
                if (index < strSegment.Length)
                    strResult += strSegment[index];
                else
                    strResult += 'n';
            }

            return strResult;
        }

        // �ϲ�����һ�� body type ��ȫ��֪ͨ�ַ�
        // �� strChars �е� 'y' ���õ� strHistory �ж�Ӧ�ﵽλ��'n' ������
        public static int SetNotifiedChars(LibraryApplication app,
            string strBodyType,
            string strChars,
            ref string strHistory,
            out string strError)
        {
            strError = "";

            int nExtendCount = 0;   // ��չ�ӿڵĸ���
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // ÿ��С���ֵĳ���

            int index = -1; // 0: dpmail; 1: email; >=2: �����������Ϣ�ӿڷ�ʽ
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "����ʶ��� message type '" + strBodyType + "'";
                    return -1;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') û���� m_externalMessageInterfaces �������ҵ�";
                    return -1;
                }
                index += 2;
            }

            for (int i = 0; i < strChars.Length; i++)
            {
                char ch = strChars[i];
                if (ch == 'n')
                    continue;

                int nLength = (i + 1) * nSegmentLength;
                if (strHistory.Length < nLength)
                    strHistory = strHistory.PadRight(nLength, 'n');
                int nOffs = i * nSegmentLength + index;
                strHistory = strHistory.Remove(nOffs, 1);
                strHistory = strHistory.Insert(nOffs, "y");
            }

            return 0;
        }

        /// <summary>
        /// �޸�һ���ַ�λ
        /// </summary>
        /// <param name="strText">Ҫ������ַ���</param>
        /// <param name="index">Ҫ���õ�λ�á��� 0 ��ʼ����</param>
        /// <param name="ch">Ҫ���õ��ַ�</param>
        /// <param name="chBlank">�հ��ַ�����չ�ַ������ȵ�ʱ���������ַ�</param>
        public static void SetChar(ref string strText,
            int index,
            char ch,
            char chBlank = 'n')
        {
            if (strText.Length < index + 1)
                strText = strText.PadRight(index + 1, chBlank);

            strText = strText.Remove(index, 1);
            strText = strText.Insert(index, new string(ch,1));
        }

#if NO
        // �۲���ʷ�ַ�����ĳλ�� 'y'/'n' ״̬
        // parameters:
        //      strBodyType ֪ͨ��Ϣ�Ľӿ� (ý��) ����
        //      nTimeIndex  2013/9/24 �߻��Ĵ����±ꡣ0 ��ʾ�Ѿ�����ʱ�Ĵ߻���1 ���Ժ��ֵ��ʾ�����ַ����ж�����ض�����������֪ͨ��Ҳ������δ����ʱ�������
        public static bool IsNotified(
            LibraryApplication app,
            string strBodyType,
            int nTimeIndex,
            string strHistory)
        {
            Debug.Assert(nTimeIndex >= 0, "");

            int nExtendCount = 0;   // ��չ�ӿڵĸ���
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // ÿ��С���ֵĳ���

            int index = -1; // 0: dpmail; 1: email; >=2: �����������Ϣ�ӿڷ�ʽ
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    // strError = "����ʶ��� message type '" + strBodyType + "'";
                    // return -1;
                    return false;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    // strError = "external_interface (type '" + external_interface.Type + "') û���� m_externalMessageInterfaces �������ҵ�";
                    // return -1;
                    return false;
                }
                index += 2; 
            }

            // �����������е�ƫ��
            index = (nSegmentLength * nTimeIndex) + index;

            if (strHistory.Length < index + 1)
                return false;

            if (strHistory[index] == 'y')
                return true;

            return false;
        }

        // ������ʷ�ַ�����ĳλ�� 'y' ״̬
        public static int SetNotified(
            LibraryApplication app,
            string strBodyType,
            int nTimeIndex,
            ref string strHistory,
            out string strError)
        {
            strError = "";
            Debug.Assert(nTimeIndex >= 0, "");

            int nExtendCount = 0;   // ��չ�ӿڵĸ���
            if (app.m_externalMessageInterfaces != null)
                nExtendCount = app.m_externalMessageInterfaces.Count;

            int nSegmentLength = nExtendCount + 2;  // ÿ��С���ֵĳ���

            int index = -1; // 0: dpmail; 1: email; >=2: �����������Ϣ�ӿڷ�ʽ
            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = app.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "����ʶ��� message type '" + strBodyType + "'";
                    return -1;
                }

                index = app.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') û���� m_externalMessageInterfaces �������ҵ�";
                    return -1;
                }
                index += 2;
            }

            // �����������е�ƫ��
            index = (nSegmentLength * nTimeIndex) + index;

            if (strHistory.Length < index + 1)
                strHistory = strHistory.PadRight(index + 1, 'n');

            strHistory = strHistory.Remove(index, 1);
            strHistory = strHistory.Insert(index, "y");
            return 1;
        }
#endif

#if NO
        // �޸�֪ͨ��ʷ�ַ��������ض���λ����Ϊ'y'
        // return:
        //      -1  error
        //      0   û�з����޸�
        //      1   �������޸�
        int ModifyHistoryString(string strBodyType,
            ref string strHistory,
            out string strError)
        {
            strError = "";
            int index = -1;

            bool bChanged = false;

            if (strBodyType == "dpmail")
            {
                index = 0;
            }
            else if (strBodyType == "email")
            {
                index = 1;
            }
            else
            {
                MessageInterface external_interface = this.App.GetMessageInterface(strBodyType);
                if (external_interface == null)
                {
                    strError = "����ʶ��� message type '" + strBodyType + "'";
                    return -1;
                }

                index = this.App.m_externalMessageInterfaces.IndexOf(external_interface);
                if (index == -1)
                {
                    strError = "external_interface (type '" + external_interface.Type + "') û���� m_externalMessageInterfaces �������ҵ�";
                    return -1;
                }
                index += 2;
            }

            if (strHistory.Length < index + 1)
            {
                strHistory = strHistory.PadRight(index + 1, 'n');
                bChanged = true;
            }

            if (strHistory[index] != 'y')
            {
                strHistory = strHistory.Remove(index);
                strHistory = strHistory.Insert(index, "y");
                // strHistory[index] = 'y';
                bChanged = true;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // ��鵱ǰ�Ƿ���Ǳ�ڵĳ��ڲ�
        // return:
        //      -1  error
        //      0   û�г��ڲ�
        //      1   �г��ڲ�
        int CheckOverdue(
            Calendar calendar,
            ref XmlDocument readerdom,
            out string strError)
        {
            strError = "";
            int nOverCount = 0;
            int nRet = 0;

            LibraryApplication app = this.App;

            string strOverdueItemBarcodeList = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            XmlNode node = null;
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    node = nodes[i];
                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                    string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strOperator = DomUtil.GetAttr(node, "operator");

                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   û�з��ֳ���
                    //      1   ���ֳ���   strError������ʾ��Ϣ
                    //      2   �Ѿ��ڿ������ڣ������׳��� 2009/3/13 new add
                    nRet = app.CheckPeriod(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "���߼�¼�� �йز� '" + strBarcode + "' �Ľ���������Ϣ�����ִ���" + strError;
                    }

                    if (nRet == 1)
                    {
                        if (strOverdueItemBarcodeList != "")
                            strOverdueItemBarcodeList += ",";
                        strOverdueItemBarcodeList += strBarcode;
                        nOverCount++;
                    }


                }

                // ����δ�黹�Ĳ��г����˳������
                if (nOverCount > 0)
                {
                    strError = "�ö��ߵ�ǰ�� " + Convert.ToString(nOverCount) + " ��δ�����ڲ�: " + strOverdueItemBarcodeList + ""; 
                    return 1;
                }
            }

            return 0;
        }
    }
}
