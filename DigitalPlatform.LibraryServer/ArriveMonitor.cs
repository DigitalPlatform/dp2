using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.Web;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Collections;


namespace DigitalPlatform.LibraryServer
{
    /// ԤԼ����������
    /// ��������֪ͨҲ���������
    public class ArriveMonitor : BatchTask
    {

        public ArriveMonitor(LibraryApplication app, 
            string strName)
            : base(app, strName)
        {
            this.Loop = true;
        }

        public override string DefaultName
        {
            get
            {
                return "ԤԼ�������";
            }
        }

        // ����ϵ��ַ���
        static string MakeBreakPointString(
            string strRecordID)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // <loop>
            XmlNode nodeLoop = dom.CreateElement("loop");
            dom.DocumentElement.AppendChild(nodeLoop);

            DomUtil.SetAttr(nodeLoop, "recordid", strRecordID);

            return dom.OuterXml;
        }

        // ���� ��ʼ ����
        // parameters:
        //      strStart    �����ַ�������ʽΪXML
        //                  ����Զ��ַ���Ϊ"!breakpoint"����ʾ�ӷ���������Ķϵ���Ϣ��ʼ
        int ParseArriveMonitorStart(string strStart,
            out string strRecordID,
            out string strError)
        {
            strError = "";
            strRecordID = "";

            int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                // strError = "������������Ϊ��";
                // return -1;
                strRecordID = "1";
                return 0;
            }

            if (strStart == "!breakpoint")
            {
                // �Ӷϵ�����ļ��ж�����Ϣ
                // return:
                //      -1  error
                //      0   file not found
                //      1   found
                nRet = this.App.ReadBatchTaskBreakPointFile(
                    this.DefaultName,
                    out strStart,
                    out strError);
                if (nRet == -1)
                {
                    strError = "ReadBatchTaskBreakPointFileʱ����" + strError;
                    this.App.WriteErrorLog(strError);
                    return -1;
                }

                // ���nRet == 0����ʾû�жϵ��ļ����ڣ�Ҳ��û�б�Ҫ�Ĳ����������������
                if (nRet == 0)
                {
                    strError = "��ǰ������û�з��� "+this.DefaultName+" �ϵ���Ϣ���޷���������";
                    return -1;
                }

                Debug.Assert(nRet == 1, "");
                this.AppendResultText("����������� "+this.DefaultName+" �ϴζϵ��ַ���Ϊ: "
                    + HttpUtility.HtmlEncode(strStart)
                    + "\r\n");

            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strStart);
            }
            catch (Exception ex)
            {
                strError = "װ��XML�ַ�������DOMʱ��������: " + ex.Message;
                return -1;
            }

            XmlNode nodeLoop = dom.DocumentElement.SelectSingleNode("loop");
            if (nodeLoop != null)
            {
                strRecordID = DomUtil.GetAttr(nodeLoop, "recordid");
            }

            return 0;
        }

        public static string MakeArriveMonitorParam(
    bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }


        // ����ͨ����������
        // ��ʽ
        /*
         * <root loop='...'/>
         * loopȱʡΪtrue
         * 
         * */
        public static int ParseArriveMonitorParam(string strParam,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bLoop = true;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam����װ��XML DOMʱ����: " + ex.Message;
                return -1;
            }

            // ȱʡΪtrue
            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
    "loop");
            if (strLoop.ToLower() == "no"
                || strLoop.ToLower() == "false")
                bLoop = false;
            else
                bLoop = true;

            return 0;
        }

        // һ�β���ѭ��
        public override void Worker()
        {
            // ϵͳ�����ʱ�򣬲����б��߳�
            // 2007/12/18 new add
            if (this.App.HangupReason == HangupReason.LogRecover)
                return;

            // 2012/2/4
            if (this.App.PauseBatchTask == true)
                return;

            bool bFirst = true;
            string strError = "";
            int nRet = 0;

            /*
            // ��Ϊˢ��
            if (this.App.Statis != null)
            {
                this.App.Statis.Flush();

                // 2008/3/27 new add
                if (this.App.Changed == true)
                {
                    this.App.Flush();
                }
            }*/

            BatchTaskStartInfo startinfo = this.StartInfo;
            if (startinfo == null)
                startinfo = new BatchTaskStartInfo();   // ����ȱʡֵ��

            // ͨ����������
            bool bLoop = true;
            nRet = ParseArriveMonitorParam(startinfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("����ʧ��: " + strError + "\r\n");
                return;
            }

            this.Loop = bLoop;

            string strID = "";
            nRet = ParseArriveMonitorStart(startinfo.Start,
                out strID,
                out strError);
            if (nRet == -1)
            {
                this.AppendResultText("����ʧ��: " + strError + "\r\n");
                this.Loop = false;
                return;
            }

            ////

            //
            bool bPerDayStart = false;  // �Ƿ�Ϊÿ��һ������ģʽ
            string strMonitorName = "arriveMonitor";
            {
                string strLastTime = "";

                nRet = ReadLastTime(
                    strMonitorName,
                    out strLastTime,
                    out strError);
                if (nRet == -1)
                {
                    string strErrorText = "���ļ��л�ȡ " + strMonitorName + " ÿ������ʱ��ʱ��������: " + strError;
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
                    string strErrorText = "��ȡ " + strMonitorName + " ÿ������ʱ��ʱ��������: " + strError;
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

                    bPerDayStart = true;
                }

                this.App.WriteErrorLog((bPerDayStart == true ? "(��ʱ)" : "(����ʱ)") + strMonitorName + " ������");
            }

            this.AppendResultText("��ʼ��һ��ѭ��\r\n");

            RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);

            this._calendarTable.Clear();

            int nRecCount = 0;
            for (; ; nRecCount ++)
            {
#if NO
                // ϵͳ�����ʱ�򣬲����б��߳�
                // 2008/2/4
                if (this.App.HangupReason == HangupReason.LogRecover)
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

                string strPath = this.App.ArrivedDbName + "/" + strID;

                string strXmlBody = "";
                string strMetaData = "";
                string strOutputPath = "";
                byte[] baOutputTimeStamp = null;

                // 
                this.SetProgressText((nRecCount + 1).ToString() + " " + strPath);
                this.AppendResultText("���ڴ��� " + (nRecCount + 1).ToString() + " " + strPath + "\r\n");

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
                                strError = "��¼ " + strID + " �����ڡ����������";
                            }
                            else
                            {
                                strError = "��¼ " + strID + " ����ĩһ����¼�����������";
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

#if NO
                string strLibraryCode = "";
                nRet = this.App.GetLibraryCode(strOutputPath,   // ???? BUG
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#endif


                bFirst = false;

                // ��id��������
                strID = ResPath.GetRecordId(strOutputPath);

                // ����
                nRet = DoOneRecord(
                    // calendar,
                    strOutputPath,
                    strXmlBody,
                    baOutputTimeStamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            CONTINUE:
                continue;

            } // end of for

            this.AppendResultText("ѭ�������������� " + nRecCount.ToString() + " ����¼��\r\n");

            {
                Debug.Assert(this.App != null);

                // д���ļ��������Ѿ������ĵ���ʱ��
                string strLastTime = DateTimeUtil.Rfc1123DateTimeStringEx(this.App.Clock.UtcNow.ToLocalTime()); // 2007/12/17 changed // DateTime.UtcNow // 2012/5/27
                WriteLastTime(strMonitorName,
                    strLastTime);
                string strErrorText = (bPerDayStart == true ? "(��ʱ)" : "(����ʱ)") + strMonitorName + "�������������¼ " + nRecCount.ToString() + " ����";
                this.App.WriteErrorLog(strErrorText);

            }

            return;

        ERROR1:
            this.AppendResultText("arrivethread error : " + strError + "\r\n");
            this.App.WriteErrorLog("arrivethread error : " + strError);
            return;
        }

        // �ж��Ƿ񳬹���������
        // return:
        //      -1  error
        //      0   û�г���
        //      1   �Ѿ�����
        int CheckeOutOfReservation(
            Calendar calendar,
            XmlDocument queue_rec_dom,
            out string strError)
        {
            strError = "";

            string strState = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
    "state");

            // ��֪ͨ��ɺ�ļ�¼, ѭ���в��ش���
            if (StringUtil.IsInList("outof", strState) == true)
                return 0;

            string strNotifyDate = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "notifyDate");

            /*
            string strItemBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "itemBarcode");
            string strReaderBarcode = DomUtil.GetElementText(queue_rec_dom.DocumentElement,
                "readerBarcode");
             * */


            // ��������ֵ
            string strPeriodUnit = "";
            long lPeriodValue = 0;

            int nRet = LibraryApplication.ParsePeriodUnit(
                this.App.ArrivedReserveTimeSpan,
                out lPeriodValue,
                out strPeriodUnit,
                out strError);
            if (nRet == -1)
            {
                strError = "ԤԼ�������� ֵ '" + this.App.ArrivedReserveTimeSpan + "' ��ʽ����: " + strError;
                return -1;
            }

            //
            DateTime notifydate;

            try
            {
                notifydate = DateTimeUtil.FromRfc1123DateTimeString(strNotifyDate);
            }
            catch
            {
                strError = "֪ͨ����ֵ '" + strNotifyDate + "' ��ʽ����";
                return -1;
            }


            DateTime timeEnd = DateTime.MinValue;

            nRet = LibraryApplication.GetOverTime(
                calendar,
                notifydate,
                lPeriodValue,
                strPeriodUnit,
                out timeEnd,
                out strError);
            if (nRet == -1)
            {
                strError = "���㱣���ڹ��̷�������: " + strError;
                return -1;
            }

            DateTime now = this.App.Clock.UtcNow;  //  DateTime.UtcNow;

            // ���滯ʱ��
            nRet = LibraryApplication.RoundTime(strPeriodUnit,
                ref now,
                out strError);
            if (nRet == -1)
                return -1;

            TimeSpan delta = now - timeEnd;

            long lDelta = 0;

            nRet = LibraryApplication.ParseTimeSpan(
                delta,
                strPeriodUnit,
                out lDelta,
                out strError);
            if (nRet == -1)
                return -1;

            if (lDelta > 0)
                return 1;

            return 0;
        }

        // ��ö�������
        // return:
        //      -1  ����
        //      0   û���ҵ����߼�¼
        //      1   �ҵ�
        int GetReaderType(string strReaderBarcode,
            out string strReaderType,
            out string strError)
        {
            strError = "";
            strReaderType = "";

            if (string.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "strReaderBarcode ����Ϊ��";
                return -1;
            }

            // ������߼�¼
            string strReaderXml = "";
            string strOutputReaderRecPath = "";
            byte[] reader_timestamp = null;

            int nRet = this.App.GetReaderRecXml(
                this.RmsChannels,
                strReaderBarcode,
                out strReaderXml,
                out strOutputReaderRecPath,
                out reader_timestamp,
                out strError);
            if (nRet == 0)
            {
                strError = "����֤����� '" + strReaderBarcode + "' ������";
                return 0;
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

            strReaderType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");
            return 1;
        }

        // string m_strCalendarLibraryCode = null;

        // ��������� cache�� �ݴ��� + | + �������� --> ��������
        Hashtable _calendarTable = new Hashtable();

        // ����һ����¼
        // parameters:
        //      strQueueRecPath ԤԼ������м�¼��·��
        int DoOneRecord(
            // Calendar calendar,
            string strQueueRecPath,
            string strQueueRecXml,
            byte[] baQueueRecTimeStamp,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strQueueRecXml);
            }
            catch (Exception ex)
            {
                strError = "װ�ض��м�¼XML��DOM����: " + ex.Message;
                return -1;
            }

            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            // ��֪ͨ��ɺ�ļ�¼, ѭ���в��ش���
            if (StringUtil.IsInList("outof", strState) == true)
                return 0;

            // TODO: ���ߵĹݴ���
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement,
    "libraryCode");
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
    "readerBarcode");

            // 2015/5/20
            // ͨ������֤����Ż�ö�������
            string strReaderType = "";
            // ��ö�������
            // return:
            //      -1  ����
            //      0   û���ҵ����߼�¼
            //      1   �ҵ�
            nRet = GetReaderType(strReaderBarcode,
        out strReaderType,
        out strError);
            if (nRet == -1)
                strReaderType = "";

            string strKey = strLibraryCode + "|" + strReaderType;
            Calendar calendar = (Calendar)_calendarTable[strKey];
            if (calendar == null)
            {
                // return:
                //      -1  ����
                //      0   û���ҵ�����
                //      1   �ҵ�����
                nRet = this.App.GetReaderCalendar(strReaderType,
                    strLibraryCode,
                    out calendar,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    calendar = null;

                if (calendar != null && _calendarTable.Count < 10000)
                    _calendarTable[strKey] = calendar;
            }
#if NO
            if (this.m_calendar == null
                || this.m_strCalendarLibraryCode != strLibraryCode
                )
            {
                // return:
                //      -1  ����
                //      0   û���ҵ�����
                //      1   �ҵ�����
                nRet = this.App.GetReaderCalendar(null,
                    strLibraryCode,
                    out m_calendar,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    this.m_strCalendarLibraryCode = strLibraryCode;
                else
                {
                    m_calendar = null;
                    this.m_strCalendarLibraryCode = "";
                }
            }
#endif

            // �ж��Ƿ񳬹���������
            // return:
            //      -1  error
            //      0   û�г���
            //      1   �Ѿ�����
            nRet = CheckeOutOfReservation(
                    calendar,
                    dom,
                    out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                    "itemBarcode");

                // ֪ͨ��ǰ���ߣ���������ȡ��ı�������
                string strNotifyDate = DomUtil.GetElementText(dom.DocumentElement,
                    "notifyDate");
                nRet = AddReaderOutOfReservationInfo(
                        this.RmsChannels,
                        strReaderBarcode,
                        strItemBarcode,
                        strNotifyDate,
                        out strError);
                if (nRet == -1)
                {
                    this.App.WriteErrorLog("AddReaderOutOfReservationInfo() error: " + strError);
                }

                // �Ѿ������������ޣ�Ҫ֪ͨ��һλԤԼ��
                nRet = this.App.DoNotifyNext(
                        this.RmsChannels,
                        strQueueRecPath,
                        dom,
                        baQueueRecTimeStamp,
                        out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    // ��Ҫ���
                    // ���¼��<location>��Ҫȥ��#reservation�����<request>Ԫ��Ҳ��Ҫɾ��

                    nRet = RemoveEntityReservationInfo(strItemBarcode,
                        strReaderBarcode,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "RemoveEntityReservationInfo() error: " + strError;
                        return -1;
                    }
                }
            }

            return 0;
        }

        // �����߼�¼�����ԤԼ������ڲ�ȡ��״̬
        int AddReaderOutOfReservationInfo(
            RmsChannelCollection channels,
            string strReaderBarcode,
            string strItemBarcode,
            string strNotifyDate,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

            int nRedoCount = 0;

            REDO_MEMO:
            // �Ӷ��߼�¼��
            this.App.ReaderLocks.LockForWrite(strReaderBarcode);

            try // ���߼�¼������Χ��ʼ
            {

                // ������߼�¼
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.App.GetReaderRecXml(
                    channels,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "����֤����� '" + strReaderBarcode + "' ������";
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

                XmlNode root = readerdom.DocumentElement.SelectSingleNode("outofReservations");
                if (root == null)
                {
                    root = readerdom.CreateElement("outofReservations");
                    readerdom.DocumentElement.AppendChild(root);
                }

                // �ۼƴ���
                string strCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strCount) == true)
                    strCount = "0";
                int nCount = 0;
                try
                {
                    nCount = Convert.ToInt32(strCount);
                }
                catch
                {
                }
                nCount++;
                DomUtil.SetAttr(root, "count", nCount.ToString());

                // ׷��<request>Ԫ��
                XmlNode request = readerdom.CreateElement("request");
                root.AppendChild(request);
                DomUtil.SetAttr(request, "itemBarcode", strItemBarcode);
                DomUtil.SetAttr(request, "notifyDate", strNotifyDate);

                byte[] output_timestamp = null;
                string strOutputPath = "";

                RmsChannel channel = channels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                // д�ض��߼�¼
                lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                    readerdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    reader_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "д�ض��߼�¼��ʱ��,����ʱ�����ͻ,���������10��,��ʧ��...";
                            return -1;
                        }
                        goto REDO_MEMO;
                    }
                    return -1;
                }


            } // ���߼�¼������Χ����
            finally
            {
                this.App.ReaderLocks.UnlockForWrite(strReaderBarcode);
            }

            return 0;
        }

        // ȥ�����¼�й�ʱ��ԤԼ��Ϣ
        // ���¼��<location>��Ҫȥ��#reservation�����<request>Ԫ��Ҳ��Ҫɾ��
        // ��������������EntityLocks��������
        int RemoveEntityReservationInfo(string strItemBarcode,
            string strReaderBarcode,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "������Ų���Ϊ�ա�";
                return -1;
            }
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "����֤����Ų���Ϊ�ա�";
                return -1;
            }

            // �Ӳ��¼��
            this.App.EntityLocks.LockForWrite(strItemBarcode);

            try // ���¼������Χ��ʼ
            {
                // �Ӳ�����Ż�ò��¼

                int nRedoCount = 0;

            REDO_CHANGE:
                List<string> aPath = null;
                string strItemXml = "";
                byte[] item_timestamp = null;
                string strOutputItemRecPath = "";

                // ��ò��¼
                // return:
                //      -1  error
                //      0   not found
                //      1   ����1��
                //      >1  ���ж���1��
                nRet = this.App.GetItemRecXml(
                    this.RmsChannels,
                    strItemBarcode,
                    out strItemXml,
                    100,
                    out aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "������� '" + strItemBarcode + "' ������";
                    return -1;
                }
                if (nRet == -1)
                {
                    strError = "������¼ʱ��������: " + strError;
                    return -1;
                }

                if (aPath.Count > 1)
                {
                    strError = "�������Ϊ '" + strItemBarcode + "' �Ĳ��¼�� " + aPath.Count.ToString() + " �����޷������޸Ĳ��¼�Ĳ�����";
                    return -1;
                }
                else
                {

                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");

                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                    }
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

                // �޸Ĳ��¼

                // ���¼��<location>��Ҫȥ��#reservation�����<request>Ԫ��Ҳ��Ҫɾ��
                string strLocation = DomUtil.GetElementText(itemdom.DocumentElement,
                    "location");
                // StringUtil.RemoveFromInList("#reservation", true, ref strLocation);
                strLocation = StringUtil.GetPureLocationString(strLocation);
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "location", strLocation);

                XmlNode nodeRequest = itemdom.DocumentElement.SelectSingleNode("reservations/request[@reader='" + strReaderBarcode + "']");
                if (nodeRequest != null)
                {
                    nodeRequest.ParentNode.RemoveChild(nodeRequest);
                }

                RmsChannel channel = this.RmsChannels.GetChannel(this.App.WsUrl);


                // д�ز��¼
                byte[] output_timestamp = null;
                string strOutputPath = "";
                long lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                    itemdom.OuterXml,
                    false,
                    "content",  // ,ignorechecktimestamp
                    item_timestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        nRedoCount++;
                        if (nRedoCount > 10)
                        {
                            strError = "д�ز��¼��ʱ��,����ʱ�����ͻ,���������10��,��ʧ��...";
                            return -1;
                        }
                        goto REDO_CHANGE;
                    }
                }

            } // ���¼������Χ����
            finally
            {
                // ����¼��
                this.App.EntityLocks.UnlockForWrite(strItemBarcode);
            }


            return 0;
        }





    }
}
