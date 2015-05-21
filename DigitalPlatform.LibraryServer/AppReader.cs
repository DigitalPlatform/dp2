using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

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
    /// �������Ƕ�����صĴ���
    /// </summary>
    public partial class LibraryApplication
    {
        // ���߼�¼�� Ҫ��Ԫ�����б�
        static string[] reader_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post", // 2009/7/17 new add
                "address",
                "tel",
                "email",
                "comment",
                "zhengyuan",
                "hire",
                "cardNumber",   // ����֤�š�Ϊ��ԭ����(100$b)���ݣ�ҲΪ�˽�����RFID���� 2008/10/14 new add
                "foregift", // Ѻ��2008/11/11 new add
                "displayName",  // ��ʾ��
                "preference",   // ���Ի�����
                "outofReservations",    // ԤԼδȡ����
                "nation",   // 2011/9/24
                "fingerprint", // 2012/1/15
                "rights", // 2014/7/8
                "personalLibrary", // 2014/7/8
                "friends", // 2014/9/9
                "access",   // 2014/9/10
            };

        // ���߼�¼�� �����Լ����޸ĵ�Ԫ�����б�
        static string[] selfchangeable_reader_element_names = new string[] {
                "displayName",  // ��ʾ��
                "preference",   // ���Ի�����
            };

        // ɾ��target�е�ȫ��<dprms:file>Ԫ�أ�Ȼ��source��¼�е�ȫ��<dprms:file>Ԫ�ز��뵽target��¼��
        public static void MergeDprmsFile(ref XmlDocument domTarget,
            XmlDocument domSource)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // ɾ��target�е�ȫ��<dprms:file>Ԫ��
            XmlNodeList nodes = domTarget.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.ParentNode != null)
                    node.ParentNode.RemoveChild(node);
            }

            // Ȼ��source��¼�е�ȫ��<dprms:file>Ԫ�ز��뵽target��¼��
            nodes = domSource.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                XmlDocumentFragment fragment = domTarget.CreateDocumentFragment();
                fragment.InnerXml = node.OuterXml;

                domTarget.DocumentElement.AppendChild(fragment);
            }
        }

#if NO
        // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList)
        {
            string strDbName = ResPath.GetDbName(strReaderRecPath);
            if (string.IsNullOrEmpty(strDbName) == true)
                return false;

            List<string> dbnames = GetCurrentReaderDbNameList(strAccountLibraryCodeList);
            if (dbnames.IndexOf(strDbName) != -1)
                return true;
            return false;
        }
#endif
        // ��װ��İ汾
        // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList)
        {
            string strLibraryCode = "";
            return IsCurrentChangeableReaderPath(strReaderRecPath,
                strAccountLibraryCodeList,
                out strLibraryCode);
        }

        // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��? ���߻�ö��߿�(strReaderRecPath)�Ĺݴ���
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList,
            out string strLibraryCode)
        {
            strLibraryCode = "";

            string strDbName = ResPath.GetDbName(strReaderRecPath);
            if (string.IsNullOrEmpty(strDbName) == true)
                return false;

            if (IsReaderDbName(strDbName, out strLibraryCode) == false)
                return false;

            if (SessionInfo.IsGlobalUser(strAccountLibraryCodeList) == true)
                return true;

            if (StringUtil.IsInList(strLibraryCode, strAccountLibraryCodeList) == true)
                return true;

            return false;
        }

        // ��õ�ǰ�û��ܹ�Ͻ�Ķ��߿����б�
        public List<string> GetCurrentReaderDbNameList(string strAccountLibraryCodeList)
        {
            List<string> dbnames = new List<string>();
            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                string strDbName = this.ReaderDbs[i].DbName;
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                if (string.IsNullOrEmpty(strAccountLibraryCodeList) == false)
                {
                    string strLibraryCode = this.ReaderDbs[i].LibraryCode;
                    // ƥ��ͼ��ݴ���
                    // parameters:
                    //      strSingle   ����ͼ��ݴ��롣�յ����ǲ���ƥ��
                    //      strList     ͼ��ݴ����б�����"��һ��,�ڶ���"������"*"���ձ�ʾ��ƥ��
                    // return:
                    //      false   û��ƥ����
                    //      true    ƥ����
                    if (LibraryApplication.MatchLibraryCode(strLibraryCode, strAccountLibraryCodeList) == false)
                        continue;
                }

                dbnames.Add(strDbName);
            }

            return dbnames;
        }

        // ƥ��ͼ��ݴ���
        // parameters:
        //      strSingle   ����ͼ��ݴ��롣�յ����ǲ���ƥ��
        //      strList     ͼ��ݴ����б�����"��һ��,�ڶ���"������"*"��һ���ǺŻ��߿ձ�ʾ��ƥ��
        // return:
        //      false   û��ƥ����
        //      true    ƥ����
        public static bool MatchLibraryCode(string strSingle, string strList)
        {
            if (string.IsNullOrEmpty(strSingle) == true
                && SessionInfo.IsGlobalUser(strList) == true)
                return true;

            if (string.IsNullOrEmpty(strSingle) == true)
                return false;
            if (SessionInfo.IsGlobalUser(strList) == true)
                return true;
            string[] parts = strList.Split(new char[] {','});
            foreach (string s in parts)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                string strOne = s.Trim();
                if (string.IsNullOrEmpty(strOne) == true)
                    continue;
                if (strOne == "*")
                    return true;
                if (strOne == strSingle)
                    return true;
            }

            return false;
        }

        // ��Ԫ����<birthday>�滻Ϊ<dateOfBirth>
        static bool RenameBirthday(XmlDocument dom)
        {
            if (dom == null || dom.DocumentElement == null)
                return false;

            bool bChanged = false;
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//birthday");
            foreach (XmlNode node in nodes)
            {
                XmlNode nodeNew = dom.CreateElement("dateOfBirth");
                if (node != dom.DocumentElement)
                {
                    node.ParentNode.InsertBefore(nodeNew, node);

                    nodeNew.InnerXml = node.InnerXml;
                    node.ParentNode.RemoveChild(node);
                    bChanged = true;
                }
            }

            return bChanged;
        }

        // <DoReaderChange()���¼�����>
        // �ϲ��¾ɼ�¼
        static int MergeTwoReaderXml(
            string [] reader_element_names,
            string strAction,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            if (strAction == "change")
            {
                /*
                // Ҫ��Ԫ�����б�
                string[] reader_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post", // 2009/7/17 new add
                "address",
                "tel",
                "email",
                "comment",
                "zhengyuan",
                "hire",
                "cardNumber",   // ����֤�š�Ϊ��ԭ����(100$b)���ݣ�ҲΪ�˽�����RFID����  2008/10/14 new add
            };
                */
                RenameBirthday(domExist);
                RenameBirthday(domNew);

                // �㷨��Ҫ����, ��"�¼�¼"�е�Ҫ���ֶ�, ���ǵ�"�Ѵ��ڼ�¼"��

                for (int i = 0; i < reader_element_names.Length; i++)
                {
                    string strElementName = reader_element_names[i];
                    // <foregift>Ԫ�����ݲ���SetReaderInfo() API��change action�޸�
                    if (strElementName == "foregift")
                        continue;

                    // 2006/11/29 changed
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        strElementName);

                    // 2013/1/15 <fingerprint>Ԫ�ص�������
                    if (strElementName == "fingerprint")
                    {
                        string strTextOld = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                            strElementName);
                        // ���Ԫ���ı��������Է����仯
                        if (strTextNew != strTextOld)
                        {
                            DomUtil.SetElementOuterXml(domExist.DocumentElement,
    strElementName,
    strTextNew);
                            // ˢ��timestamp����
                            XmlNode node = domExist.DocumentElement.SelectSingleNode(strElementName);
                            if (node != null)
                                DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));
                        }
                        continue;
                    }

                    // 2013/6/19 <hire>Ԫ�ص�������
                    // ���� expireDate ���Բ����޸�
                    if (strElementName == "hire")
                    {
                        XmlNode nodeExist = domExist.DocumentElement.SelectSingleNode("hire");
                        // XmlNode nodeNew = domNew.DocumentElement.SelectSingleNode("hire");

                        string strExistExpireDate = "";
                        if (nodeExist != null)
                            strExistExpireDate = DomUtil.GetAttr(nodeExist, "expireDate");

                        DomUtil.SetElementOuterXml(domExist.DocumentElement,
                            strElementName,
                            strTextNew);

                        // �� expireDate ���ǻ�ȥ
                        nodeExist = domExist.DocumentElement.SelectSingleNode("hire");
                        if (nodeExist != null)
                            DomUtil.SetAttr(domExist.DocumentElement, "expireDate", strExistExpireDate);
                        else if (string.IsNullOrEmpty(strExistExpireDate) == false)
                        {
                            XmlNode node = DomUtil.SetElementText(domExist.DocumentElement,
                                strElementName,
                                "");
                            DomUtil.SetAttr(node, "expireDate", strExistExpireDate);
                        }

                        continue;
                    }

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        strElementName,
                        strTextNew);

                }

                // ɾ��target�е�ȫ��<dprms:file>Ԫ�أ�Ȼ��source��¼�е�ȫ��<dprms:file>Ԫ�ز��뵽target��¼��
                MergeDprmsFile(ref domExist,
                    domNew);

            }
            else if (strAction == "changestate")
            {

                string[] element_names_onlystate = new string[] {
                    "state",
                    "comment",
                    };
                for (int i = 0; i < element_names_onlystate.Length; i++)
                {
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        element_names_onlystate[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        element_names_onlystate[i],
                        strTextNew);

                }

                // ���޸�<dprms:file>
            }
            else if (strAction == "changeforegift")
            {
                // 2008/11/11 new add
                string[] element_names_onlyforegift = new string[] {
                    "foregift",
                    "comment",
                    };
                for (int i = 0; i < element_names_onlyforegift.Length; i++)
                {
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        element_names_onlyforegift[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        element_names_onlyforegift[i],
                        strTextNew);

                }

                // ���޸�<dprms:file>
            }
            else
            {
                strError = "strActionֵ����Ϊchange��changestate��changeforegift֮һ��";
                return -1;
            }

            strMergedXml = domExist.OuterXml;
            return 0;
        }

        // ������ʺϱ�����¶��߼�¼
        // ��Ҫ��Ϊ�˰Ѵ��ӹ��ļ�¼�У����ܳ��ֵ����ڡ���ͨ��Ϣ�����ֶ�ȥ����������ְ�ȫ������
        // return:
        //      -1  ����
        //      0   û��ʵ�����޸�
        //      1   ������ʵ�����޸�
        static int BuildNewReaderRecord(XmlDocument domNewRec,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            // ��ͨԪ�����б�
            string[] element_names = new string[] {
                "borrows",
                "overdues",
                "reservations",
                "borrowHistory",
                "outofReservations",
                "hire", // 2008/11/11 new add
                "foregift", // 2008/11/11 new add
            };

            // TODO: ��Ҫ���Ա�����������<hire>Ԫ�ص�����ֵ����ȥ��ô?

            XmlDocument dom = new XmlDocument();

            dom.LoadXml(domNewRec.OuterXml);

            RenameBirthday(dom);

            bool bChanged = false;
            for (int i = 0; i < element_names.Length; i++)
            {
                List<XmlNode> deleted_nodes = DomUtil.DeleteElements(dom.DocumentElement,
                    element_names[i]);
                if (deleted_nodes != null
                    && deleted_nodes.Count > 0)
                    bChanged = true;
            }

            // ������Ѿ�����<fingerprint>Ԫ�أ���������timestamp����
            // ˢ��timestamp����
            XmlNode node = dom.DocumentElement.SelectSingleNode("fingerprint");
            if (node != null)
                DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));

            // TODO: �����״�����
            string strBirthDate = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
            string strNewPassword = "";
            try
            {
                if (string.IsNullOrEmpty(strBirthDate) == false)
                    strNewPassword = DateTimeUtil.DateTimeToString8(DateTimeUtil.FromRfc1123DateTimeString(strBirthDate));
            }
            catch (Exception ex)
            {
                strError = "���������ֶ�ֵ '" + strBirthDate + "' ���Ϸ�: " + ex.Message;
                return -1;
            }

            XmlDocument domOperLog = null;
            // �޸Ķ�������
            // return:
            //      -1  error
            //      0   �ɹ�
            int nRet = ChangeReaderPassword(
                dom,
                strNewPassword,
                ref domOperLog,
                out strError);
            if (nRet == -1)
            {
                strError = "��ʼ�����߼�¼����ʱ����: " + strError;
                return -1;
            }

            strXml = dom.OuterXml;
            if (bChanged == true)
                return 1;

            return 0;
        }

        // <DoReaderChange()���¼�����>
        // �Ƚ�������¼, �����Ͷ��߾�̬��Ϣ�йص��ֶ��Ƿ����˱仯
        // return:
        //      0   û�б仯
        //      1   �б仯
        static int IsReaderInfoChanged(
            string [] reader_element_names,
            XmlDocument dom1,
            XmlDocument dom2)
        {
            for (int i = 0; i < reader_element_names.Length; i++)
            {
                /*
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    element_names[i]);
                 * */
                // 2006/11/29 changed
                string strText1 = DomUtil.GetElementOuterXml(dom1.DocumentElement,
                    reader_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(dom2.DocumentElement,
                    reader_element_names[i]);


                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }


        // �޸Ķ��߼�¼
        // TODO: �Ƿ�Ҫ�ṩ������ص������ǿ��д��Ĺ��ܣ�
        // ��Ҫһ�������ɼ�¼��ԭ��, ��Ϊ�˺����ݿ��е�ǰ�����Ѿ��仯�˵ļ�¼���бȽϣ�
        // ���SetReaderInfo�ܸ��ǵĲ����ֶΣ���һ����û�з���ʵ���Ա仯��������¼������
        // ��ͨʵʱ��Ϣ�����˱仯���������������ʵ��ϲ��󱣴��¼�������᷵�ش�������
        // ��API�Ŀ����ԡ����ʵ�������в������ؾɼ�¼���ɷ������ַ������ͻ���������
        // �����ԣ���ɣ��������ݿ��е�ǰ��¼�ĸı��������Щ�ֶη�Χ����ֻ�ܱ������ˡ�
        // paramters:
        //      strAction    ������new change delete changestate changeforegift forcenew forcechange forcedelete
        //      strRecPath  ϣ�����浽�ļ�¼·��������Ϊ�ա�
        //      strNewXml   ϣ������ļ�¼��
        //      strOldXml   ԭ�Ȼ�õľɼ�¼�塣����Ϊ�ա�
        //      baOldTimestamp  ԭ�Ȼ�þɼ�¼��ʱ���������Ϊ�ա�
        //      strExistringXml ���ǲ���ʧ��ʱ���������ݿ����Ѿ����ڵļ�¼����ǰ�˲ο�
        //      strSavedXml ʵ�ʱ�����¼�¼�����ݿ��ܺ�strNewXml�������졣
        //      strSavedRecPath ʵ�ʱ���ļ�¼·��
        //      baNewTimestamp  ʵ�ʱ�������ʱ���
        // return:
        //      result -1ʧ�� 0 ���� 1�����ֶα��ܾ�(ע������Ƿ�ʵ�֣��ǵû���һ��ר�ŵĴ��������ʹ��)
        // Ȩ�ޣ�
        //      ���߲����޸��κ��˵Ķ��߼�¼���������Լ��ġ�
        //      ������Ա��Ҫ�� setreaderinfoȨ���Ƿ�߱�
        //      ����������ܻ���Ҫ changereaderstate �� changereaderforegift Ȩ��
        // ��־:
        //      Ҫ������־
        public LibraryServerResult SetReaderInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            byte[] baOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode)
        {
            strExistingXml = "";
            strSavedXml = "";
            strSavedRecPath = "";
            baNewTimestamp = null;

            string[] element_names = reader_element_names;

            LibraryServerResult result = new LibraryServerResult();

            LibraryApplication app = this;

            kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            bool bForce = false;
            if (strAction == "forcenew"
                || strAction == "forcechange"
                || strAction == "forcedelete")
            {
                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "�޸Ķ�����Ϣ��" + strAction + "�������ܾ������߱�restoreȨ�ޡ�";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                bForce = true;

                // ��strAction�����޸�Ϊ������forceǰ׺����
                strAction = strAction.Remove(0, "force".Length);
            }
            else
            {
                // Ȩ���ַ���
                if (strAction == "changestate")
                {
                    // ��setreaderinfo��changereaderstate֮һ����
                    if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    {
                        if (StringUtil.IsInList("changereaderstate", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "�޸Ķ�����Ϣ���ܾ������߱�changereaderstateȨ�ޡ�";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }
                if (strAction == "changeforegift")
                {
                    // changereaderforegift
                    if (StringUtil.IsInList("changereaderforegift", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "changeforegift��ʽ�޸Ķ�����Ϣ���ܾ������߱�changereaderforegiftȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "�޸Ķ�����Ϣ���ܾ������߱�setreaderinfoȨ�ޡ�";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            // �Զ�����ݵĸ����ж�
            if (strAction != "change" && sessioninfo.UserType == "reader")
            {
                // ����������޸��������ߵļ�¼,��������ߴ������߼�¼.������������޸��Լ��ļ�¼�е�ĳЩԪ��
                result.Value = -1;
                result.ErrorInfo = "�������ִ�� '" + strAction + "' ���޸Ķ�����Ϣ�����������ܾ�";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            // �������

            if (strAction == "delete")
            {
                if (String.IsNullOrEmpty(strNewXml) == false)
                {
                    strError = "strActionֵΪdeleteʱ, strNewXml��������Ϊ��";
                    goto ERROR1;
                }
                if (baNewTimestamp != null)
                {
                    strError = "strActionֵΪdeleteʱ, baNewTimestamp��������Ϊ��";
                    goto ERROR1;
                }
            }
            else
            {
                // ��delete��� strNewXml����벻Ϊ��
                if (String.IsNullOrEmpty(strNewXml) == true)
                {
                    strError = "strActionֵΪ" + strAction + "ʱ, strNewXml��������Ϊ��";
                    goto ERROR1;
                }
            }

            // 2007/11/12 new add
            if (strAction == "new")
            {
                if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    strError = "strActionֵΪnewʱ, strOldXml��������Ϊ��";
                    goto ERROR1;
                }
                if (baOldTimestamp != null)
                {
                    strError = "strActionֵΪnewʱ, baOldTimestamp��������Ϊ��";
                    goto ERROR1;
                }
            }
            else
            {
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    // �������ģʽ
                    // return:
                    //      -1  �����̳���
                    //      0   ����ͨ��
                    //      1   ������ͨ��
                    nRet = CheckTestModePath(strRecPath,
                        out strError);
                    if (nRet != 0)
                    {
                        strError = "�޸Ķ��߼�¼�Ĳ������ܾ�: " + strError;
                        goto ERROR1;
                    }
                }
            }

            // �Ѿɼ�¼װ�ص�DOM
            XmlDocument domOldRec = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strOldXml) == true)
                    strOldXml = "<root />";

                domOldRec.LoadXml(strOldXml);
            }
            catch (Exception ex)
            {
                strError = "strOldXml XML��¼װ�ص�DOMʱ����: " + ex.Message;
                goto ERROR1;
            }

            // ��Ҫ������¼�¼װ�ص�DOM
            XmlDocument domNewRec = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strNewXml) == true)
                    strNewXml = "<root />";

                domNewRec.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "strNewXml XML��¼װ�ص�DOMʱ����: " + ex.Message;
                goto ERROR1;
            }

            string strOldBarcode = "";
            string strNewBarcode = "";

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
                goto ERROR1;

           // �Զ�����ݵĸ����ж�
            if (strAction == "change" && sessioninfo.UserType == "reader")
            {
                /*
                // ��ʱ����������Լ��޸��κζ��ߵ���Ϣ
                // ����޸�Ϊ������ֻ���޸��Լ��ļ�¼������ֻ���޸�ĳЩ�ֶΣ������޸ı����ԣ���
                result.Value = -1;
                result.ErrorInfo = "�޸Ķ�����Ϣ���ܾ�����Ϊ���߲����޸Ķ��߼�¼";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
                 * */

                if (sessioninfo.Account.Barcode != strNewBarcode)
                {
                    result.Value = -1;
                    result.ErrorInfo = "�޸Ķ�����Ϣ���ܾ�����Ϊ���߲����޸��������ߵĶ��߼�¼";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                element_names = selfchangeable_reader_element_names;
            }

            bool bBarcodeChanged = false;
            if (nRet == 1)
                bBarcodeChanged = true;


            string strOldDisplayName = "";
            string strNewDisplayName = "";

            // return:
            //      -1  ����
            //      0   ���
            //      1   �����
            nRet = CompareTwoDisplayName(domOldRec,
                domNewRec,
                out strOldDisplayName,
                out strNewDisplayName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            bool bDisplayNameChanged = false;
            if (nRet == 1)
                bDisplayNameChanged = true;


            string strLockBarcode = "";

            if (strAction == "new"
                || strAction == "change"
                || strAction == "changestate")
                strLockBarcode = strNewBarcode;
            else if (strAction == "delete")
            {
                // ˳�����һЩ���
                if (String.IsNullOrEmpty(strNewBarcode) == false)
                {
                    strError = "û�б�Ҫ��delete������strNewXml, �����¼�¼����...���෴��ע��һ��Ҫ��strOldXMl�а�������ɾ����ԭ��¼";
                    goto ERROR1;
                }
                strLockBarcode = strOldBarcode;
            }


            // �Ӷ��߼�¼��
            if (String.IsNullOrEmpty(strLockBarcode) == false)
                app.ReaderLocks.LockForWrite(strLockBarcode);

            try
            {
                // 2014/1/10
                // ���������
                if (bBarcodeChanged == true
    && (strAction == "new"
        || strAction == "change"
        || strAction == "changestate"
        || strAction == "changeforegift")
    && String.IsNullOrEmpty(strNewBarcode) == true
    )
                {
                    if (this.AcceptBlankReaderBarcode == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError + "֤����Ų���Ϊ�ա��������ʧ��";
                        result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                        return result;
                    }
                }

                // �Զ���֤����Ų��أ������Ҫ�������strRecPath
                if (bBarcodeChanged == true
                    && (strAction == "new"
                        || strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift")
                    && String.IsNullOrEmpty(strNewBarcode) == false
                    )
                {

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
                            null,
                            sessioninfo.LibraryCodeList,
                            strNewBarcode,
                            out nResultValue,
                            out strError);
                        if (nRet == -2 || nRet == -1 || nResultValue != 1)
                        {
                            if (nRet == -2)
                                strError = "library.xml ��û�������������֤�������޷������������֤";
                            else if (nRet == -1)
                            {
                                strError = "��֤����֤����ŵĹ����г���"
                                    + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                            }
                            else if (nResultValue != 1)
                            {
                                strError = "����� '" + strNewBarcode + "' ����֤���ֲ���һ���Ϸ��Ķ���֤�����"
                                    + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                            }
                            result.Value = -1;
                            result.ErrorInfo = strError + "���������ʧ��";
                            result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                            return result;
                        }
                    }

                    List<string> aPath = null;

                    // ������ֻ�������, ������ü�¼��
                    // return:
                    //      -1  error
                    //      ����    ���м�¼����(������nMax�涨�ļ���)
                    nRet = app.SearchReaderRecDup(
                        sessioninfo.Channels,
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

                        // ������������û��ָ��strRecPath
                        if (String.IsNullOrEmpty(strRecPath) == true)
                        {
                            if (strAction == "new") // 2006/12/23 add
                                bDup = true;
                            else
                                strRecPath = aPath[0];
                        }
                        else
                        {
                            if (aPath[0] == strRecPath) // �������Լ�
                            {
                                bDup = false;
                            }
                            else
                            {
                                // ��ļ�¼���Ѿ�ʹ������������
                                bDup = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(nRet > 1, "");
                        bDup = true;
                    }

                    // ����
                    if (bDup == true)
                    {
                        /*
                        string[] pathlist = new string[aPath.Count];
                        aPath.CopyTo(pathlist);


                        strError = "����� '" + strNewBarcode + "' �Ѿ������ж��߼�¼ʹ����: " + String.Join(",", pathlist) + "������ʧ�ܡ�";
                         * */
                        if (String.IsNullOrEmpty(strNewDisplayName) == false)
                            strError = "����� '" + strNewBarcode + "' �� ��ʾ�� '"+strNewDisplayName+"' �Ѿ������ж��߼�¼ʹ����: " + StringUtil.MakePathList(aPath) + "������ʧ�ܡ�";
                        else
                            strError = "����� '" + strNewBarcode + "' �Ѿ������ж��߼�¼ʹ����: " + StringUtil.MakePathList(aPath) + "������ʧ�ܡ�";

                        // 2008/8/15 changed
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }
                }

                // ����ʾ�����Ͳ���
                if (bDisplayNameChanged == true
                    && (strAction == "new"
                        || strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift")
                    && String.IsNullOrEmpty(strNewDisplayName) == false
                    )
                {
                    {
                        int nResultValue = -1;
                        // ������ֿռ䡣
                        // return:
                        //      -2  not found script
                        //      -1  ����
                        //      0   �ɹ�
                        nRet = this.DoVerifyBarcodeScriptFunction(
                            null,
                            "",
                            strNewDisplayName,
                            out nResultValue,
                            out strError);
                        if (nRet == -2)
                        {
                            // û��У������Ź��ܣ������޷�У���û�������������ֿռ�ĳ�ͻ
                            goto SKIP_VERIFY;
                        }
                        if (nRet == -1)
                        {
                            strError = "У����ʾ�� '" + strNewDisplayName + "' �������(�ռ�)Ǳ�ڳ�ͻ������(���ú���DoVerifyBarcodeScriptFunction()ʱ)��������: " + strError;
                            goto ERROR1;
                        }

                        Debug.Assert(nRet == 0, "");

                        if (nResultValue == -1)
                        {
                            strError = "У����ʾ�� '" + strNewDisplayName + "' �������(�ռ�)Ǳ�ڳ�ͻ�����з�������: " + strError;
                            goto ERROR1;
                        }


                        if (nResultValue == 1)
                        {
                            // TODO: ��Ҫ������
                            strError = "��ʾ�� '" + strNewDisplayName + "' �Ͷ���֤��������ֿռ䷢����ͻ��������Ϊ��ʾ����";
                            goto ERROR1;
                        }
                    }

                SKIP_VERIFY:

                    List<string> aPath = null;

                    // ��ֹ���������ߵ���ʾ�����ظ�
                    // ������ֻ�������, ������ü�¼��
                    // return:
                    //      -1  error
                    //      ����    ���м�¼����(������nMax�涨�ļ���)
                    nRet = app.SearchReaderDisplayNameDup(
                        sessioninfo.Channels,
                        strNewDisplayName,
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

                        // ������������û��ָ��strRecPath
                        if (String.IsNullOrEmpty(strRecPath) == true)
                        {
                            if (strAction == "new")
                                bDup = true;
                            else
                                strRecPath = aPath[0];
                        }
                        else
                        {
                            if (aPath[0] == strRecPath) // �������Լ�
                            {
                                bDup = false;
                            }
                            else
                            {
                                // ��ļ�¼���Ѿ�ʹ������������
                                bDup = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(nRet > 1, "");
                        bDup = true;
                    }

                    // ����
                    if (bDup == true)
                    {
                        strError = "��ʾ�� '" + strNewDisplayName + "' �Ѿ������ж��߼�¼ʹ����: " + StringUtil.MakePathList(aPath) + "������ʧ�ܡ�";

                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }

                    // �Թ�����Ա�ʻ������в��ء���Ȼ����ǿ���Եģ����ǿ��Ա���󲿷����
                    // ע��������Ա��Ȼ���Դ����Ͷ�����ʾ�����ص��ʻ���
                    if (SearchUserNameDup(strNewDisplayName) == true)
                    {
                        strError = "��ʾ�� '" + strNewDisplayName + "' �Ѿ���������Ա�ʻ�ʹ�á�����ʧ�ܡ�";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }
                }

                string strReaderDbName = "";

                if (String.IsNullOrEmpty(strRecPath) == false)
                    strReaderDbName = ResPath.GetDbName(strRecPath);    // BUG. ȱ��'strReaderDbName = ' 2008/6/4 changed

                // ׼����־DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");

                DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "setReaderInfo");
                // 2014/11/17
                if (bForce == true)
                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "style", "force");
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }


                // ����һ������
                if (strAction == "new")
                {
                    // ����¼�¼��·���е�id�����Ƿ���ȷ
                    // �������֣�ǰ���Ѿ�ͳһ������
                    if (String.IsNullOrEmpty(strRecPath) == true)
                    {
                        // ��·������Ϊ�յ�ʱ���Զ�ѡ�õ�һ�����߿�
                        if (String.IsNullOrEmpty(strReaderDbName) == true)
                        {
                            if (app.ReaderDbs.Count == 0)
                            {
                                strError = "dp2Library��δ������߿⣬ ����޷��´������߼�¼��";
                                goto ERROR1;
                            }

                            // ѡ�õ�ǰ�û��ܹ�Ͻ�ĵ�һ�����߿�
                            // strReaderDbName = app.ReaderDbs[0].DbName;
                            List<string> dbnames = app.GetCurrentReaderDbNameList(sessioninfo.LibraryCodeList);
                            if (dbnames.Count > 0)
                                strReaderDbName = dbnames[0];
                            else
                            {
                                strReaderDbName = "";

                                strError = "��ǰ�û�û�й�Ͻ�κζ��߿⣬ ����޷��´������߼�¼��";
                                goto ERROR1;
                            }
                        }

                        strRecPath = strReaderDbName + "/?";
                    }
                    else
                    {
                        string strID = ResPath.GetRecordId(strRecPath);
                        if (String.IsNullOrEmpty(strID) == true)
                        {
                            strError = "RecPath��id����Ӧ��Ϊ'?'";
                            goto ERROR1;
                        }

                        // 2007/11/12
                        // ��������仰���ͽ�ֹ��actionΪnewʱ�Ķ�id���湦�ܡ�������ܱ����Ǳ�����ġ�������ֹ�󣬸��ɱ���������������
                        if (strID != "?")
                        {
                            strError = "��strActionΪnewʱ��strRecPath����Ϊ ���߿���/? ��̬�����(�ձ�ʾȡ��һ�����߿�ĵ�ǰ��β��)��(��ĿǰstrRecPathΪ'" + strRecPath + "')";
                            goto ERROR1;
                        }
                    }

                    // ������ʺϱ�����¶��߼�¼
                    if (bForce == false)
                    {
                        // ��Ҫ��Ϊ�˰Ѵ��ӹ��ļ�¼�У����ܳ��ֵ����ڡ���ͨ��Ϣ�����ֶ�ȥ����������ְ�ȫ������
                        nRet = BuildNewReaderRecord(domNewRec,
                            out strSavedXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 2008/5/29 new add
                        strSavedXml = domNewRec.OuterXml;
                    }

                    string strLibraryCode = "";
                    // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                    if (app.IsCurrentChangeableReaderPath(strRecPath,
                        sessioninfo.LibraryCodeList,
                        out strLibraryCode) == false)
                    {
                        strError = "���߼�¼·�� '" + strRecPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                        goto ERROR1;
                    }

                    // 2014/7/4
                    if (this.VerifyReaderType == true)
                    {
                        XmlDocument domTemp = new XmlDocument();
                        domTemp.LoadXml(strSavedXml);

                        // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
                        // parameters:
                        // return:
                        //      -1  �����̳���
                        //      0   ����Ҫ��
                        //      1   ������Ҫ��
                        nRet = CheckReaderType(domTemp,
                            strLibraryCode,
                            strReaderDbName,
                            out strError);
                        if (nRet == -1 || nRet == 1)
                        {
                            strError = strError + "���������߼�¼����ʧ��";
                            goto ERROR1;
                        }
                    }

                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.DoSaveTextRes(strRecPath,
                        strSavedXml,
                        false,   // include preamble?
                        "content",
                        baOldTimestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strSavedXml = "";
                        strSavedRecPath = strOutputPath;    // 2011/9/6 add
                        baNewTimestamp = output_timestamp;
                        if (channel.OriginErrorCode == ErrorCodeValue.TimestampMismatch)
                        {
                            // 2011/9/6 add
                            strError = "�����¶��߼�¼��ʱ�����ݿ��ں˾��������¼�¼��λ�� '"+strOutputPath+"' ��Ȼ�Ѿ����ڼ�¼����ͨ������Ϊ�����ݿ��β�Ų��������µġ�������ϵͳ����Ա��ʱ����������ϡ�ԭʼ������Ϣ: " + strError;
                        }
                        else
                            strError = "�����¼�¼�Ĳ�����������:" + strError;
                        kernel_errorcode = channel.OriginErrorCode;
                        goto ERROR1;
                    }
                    else // �ɹ�
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // �������ڵĹݴ���

                        DomUtil.SetElementText(domOperLog.DocumentElement, "action", "new");

                        // ������<oldRecord>Ԫ��

                        XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement, "record", strNewXml);
                        DomUtil.SetAttr(node, "recPath", strOutputPath);

                        // �¼�¼����ɹ�����Ҫ������ϢԪ�ء���Ϊ��Ҫ�����µ�ʱ�����ʵ�ʱ���ļ�¼·��
                        strSavedRecPath = strOutputPath;
                        // strSavedXml     // ����������ļ�¼���������б仯, �����Ҫ���ظ�ǰ��
                        baNewTimestamp = output_timestamp;

                        // �ɹ�

                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "�޸Ķ�����Ϣ",
                            "�����¼�¼��",
                            1);

                    }
                }
                else if (strAction == "change"
                    || strAction == "changestate"
                    || strAction == "changeforegift")
                {

                    // ��Ҫ��飬�������������ݿ��¼�У��Ƿ�����ͨ��Ϣ������У������޸Ķ���֤�����

                    // DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue errorcode;
                    // ִ��"change"����
                    // 1) �����ɹ���, NewRecord����ʵ�ʱ�����¼�¼��NewTimeStampΪ�µ�ʱ���
                    // 2) �������TimeStampMismatch����OldRecord���п��з����仯��ġ�ԭ��¼����OldTimeStamp����ʱ���
                    // return:
                    //      -1  ����
                    //      0   �ɹ�
                    nRet = DoReaderChange(
                        sessioninfo.LibraryCodeList,
                        element_names,
                        strAction,
                        bForce,
                        channel,
                        strRecPath,
                        domNewRec,
                        domOldRec,
                        baOldTimestamp,
                        ref domOperLog,
                        out strExistingXml,    // strExistingRecord,
                        out strSavedXml,    // strNewRecord,
                        out baNewTimestamp,
                        out strError,
                        out kernel_errorcode);
                    if (nRet == -1)
                    {
                        // ʧ��
                        domOperLog = null;  // ��ʾ����д����־
                        goto ERROR1;
                    }

                    strSavedRecPath = strRecPath;   // ������̲���ı��¼·��


                }
                else if (strAction == "delete")
                {
                    // return:
                    //      -2  ��¼������ͨ��Ϣ������ɾ��
                    //      -1  ����
                    //      0   ��¼�����Ͳ�����
                    //      1   ��¼�ɹ�ɾ��
                    nRet = DoReaderOperDelete(
                        sessioninfo.LibraryCodeList,
                       element_names,
                       sessioninfo,
                       bForce,
                       channel,
                       strRecPath,
                       strOldXml,
                       baOldTimestamp,
                       strOldBarcode,
                        // strNewBarcode,
                       domOldRec,
                       ref strExistingXml,
                       ref baNewTimestamp,
                       ref domOperLog,
                       ref kernel_errorcode,
                       out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == -2)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.HasCirculationInfo;
                        return result;
                    }

                    // ��¼û���ҵ�
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }


                }
                else
                {
                    // ��֧�ֵ�����
                    strError = "��֧�ֵĲ������� '" + strAction + "'";
                    goto ERROR1;
                }


                // д����־
                if (domOperLog != null)
                {
                    string strOperTime = app.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // ������
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // ����ʱ��

                    nRet = app.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "SetReaderInfo() API д����־ʱ��������: " + strError;
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "�׳��쳣:" + ex.Message;
                return result;
            }
            finally
            {
                if (String.IsNullOrEmpty(strLockBarcode) == false)
                    app.ReaderLocks.UnlockForWrite(strLockBarcode);
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        #region SetReaderInfo() �¼�����

        // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
        // parameters:
        // return:
        //      -1  �����̳���
        //      0   ����Ҫ��
        //      1   ������Ҫ��
        int CheckReaderType(XmlDocument dom,
            string strLibraryCode,
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> values = null;

            // ��̽ ���߿���

            // ���һ��ͼ��ݴ����µ�ֵ�б�
            // parameters:
            //      strLibraryCode  �ݴ���
            //      strTableName    ���������Ϊ�գ���ʾ����name����ֵ��ƥ��
            //      strDbName   ���ݿ��������Ϊ�գ���ʾ����dbname����ֵ��ƥ�䡣
            values = GetOneLibraryValueTable(
                strLibraryCode,
                "readerType",
                strReaderDbName);
            if (values != null && values.Count > 0)
                goto FOUND;

            // ��̽��ʹ�����ݿ���
            values = GetOneLibraryValueTable(
    strLibraryCode,
    "readerType",
    "");
            if (values != null && values.Count > 0)
                goto FOUND;

            return 0;   // ��Ϊû��ֵ�б�ʲôֵ������

            FOUND:
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
    "readerType");

            if (IsInList(strReaderType, values) == true)
                return 0;

            GetPureValue(ref values);
            strError = "�������� '" + strReaderType + "' ���ǺϷ���ֵ��ӦΪ '" + StringUtil.MakePathList(values) + "' ֮һ";
            return 1;
        }

        // ���¾ɶ��߼�¼(���߲��¼)�а���������Ž��бȽ�, �����Ƿ����˱仯(��������Ҫ����)
        // ����Ű�����<barcode>Ԫ����
        // parameters:
        //      strOldBarcode   ˳�㷵�ؾɼ�¼�е������
        //      strNewBarcode   ˳�㷵���¼�¼�е������
        // return:
        //      -1  ����
        //      0   ���
        //      1   �����
        static int CompareTwoBarcode(
    XmlDocument domOldRec,
    XmlDocument domNewRec,
    out string strOldBarcode,
    out string strNewBarcode,
    out string strError)
        {
            return CompareTwoField(
                "barcode",
                domOldRec,
                domNewRec,
                out strOldBarcode,
                out strNewBarcode,
                out strError);
        }

        // ���¾ɼ�¼�а������ֶν��бȽ�, �����Ƿ����˱仯(��������Ҫ����)
        // parameters:
        //      strOldText   ˳�㷵�ؾɼ�¼�е��ֶ�����
        //      strNewText   ˳�㷵���¼�¼�е��ֶ�����
        // return:
        //      -1  ����
        //      0   ���
        //      1   �����
        static int CompareTwoField(
            string strElementName,
            XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldText,
            out string strNewText,
            out string strError)
        {
            strError = "";

            strOldText = "";
            strNewText = "";

            strOldText = DomUtil.GetElementText(domOldRec.DocumentElement, strElementName);

            strNewText = DomUtil.GetElementText(domNewRec.DocumentElement, strElementName);

            if (strOldText != strNewText)
                return 1;   // �����

            return 0;   // ���
        }

        // return:
        //      -1  ����
        //      0   ���
        //      1   �����
        static int CompareTwoDisplayName(XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldDisplayName,
            out string strNewDisplayName,
            out string strError)
        {
            strError = "";

            strOldDisplayName = "";
            strNewDisplayName = "";

            strOldDisplayName = DomUtil.GetElementText(domOldRec.DocumentElement, "displayName");

            strNewDisplayName = DomUtil.GetElementText(domNewRec.DocumentElement, "displayName");

            if (strOldDisplayName != strNewDisplayName)
                return 1;   // �����

            return 0;   // ���
        }

        // ���¾ɶ��߼�¼(���߲��¼)�а�����<state>״̬�ֶν��бȽ�, �����Ƿ����˱仯
        // ״̬������<state>Ԫ����
        // parameters:
        //      strOldState   ˳�㷵�ؾɼ�¼�е�״̬�ַ���
        //      strNewState   ˳�㷵���¼�¼�е�״̬�ַ���
        // return:
        //      -1  ����
        //      0   ���
        //      1   �����
        static int CompareTwoState(XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldState,
            out string strNewState,
            out string strError)
        {
            strError = "";

            strOldState = "";
            strNewState = "";

            strOldState = DomUtil.GetElementText(domOldRec.DocumentElement, "state");
            strOldState = strOldState.Trim();

            strNewState = DomUtil.GetElementText(domNewRec.DocumentElement, "state");
            strNewState = strNewState.Trim();



            if (strOldState != strNewState)
                return 1;   // �����

            return 0;   // ���
        }

        // ɾ�����߼�¼�Ĳ���
        // return:
        //      -2  ��¼������ͨ��Ϣ������ɾ��
        //      -1  ����
        //      0   ��¼�����Ͳ�����
        //      1   ��¼�ɹ�ɾ��
        int DoReaderOperDelete(
            string strCurrentLibraryCode,
            string [] element_names,
            SessionInfo sessioninfo,
            bool bForce,
            RmsChannel channel,
            string strRecPath,
            string strOldXml,
            byte[] baOldTimestamp,
            string strOldBarcode,
            // string strNewBarcode,
            XmlDocument domOldRec,
            ref string strExistingXml,
            ref byte[] baNewTimestamp,
            ref XmlDocument domOperLog,
            ref DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode,
            out string strError)
        {
            strError = "";

            int nRedoCount = 0;
            int nRet = 0;
            long lRet = 0;

            // �����¼·��Ϊ��, ���Ȼ�ü�¼·��
            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                List<string> aPath = null;

                if (String.IsNullOrEmpty(strOldBarcode) == true)
                {
                    strError = "strOldXml�е�<barcode>Ԫ���е�֤����ţ���strRecPath����ֵ������ͬʱΪ�ա�";
                    goto ERROR1;
                }

                // ������ֻ�������, ������ü�¼��
                // return:
                //      -1  error
                //      ����    ���м�¼����(������nMax�涨�ļ���)
                nRet = this.SearchReaderRecDup(
                    sessioninfo.Channels,
                    strOldBarcode,
                    100,
                    out aPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    strError = "֤�����Ϊ '" + strOldBarcode + "' �Ķ��߼�¼�Ѳ�����";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    // goto ERROR1;
                    return 0;   // 2009/7/17 changed
                }


                if (nRet > 1)
                {
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                     * */

                    // 2007/11/22 new add
                    // ��ɾ�������У������ظ����Ǻ�ƽ�������顣ֻҪ
                    // strRecPath�ܹ�������ָ��Ҫɾ������һ�����Ϳ���ִ��ɾ��
                    if (String.IsNullOrEmpty(strRecPath) == false)
                    {
                        if (aPath.IndexOf(strRecPath) == -1)
                        {
                            strError = "֤����� '" + strOldBarcode + "' �Ѿ������ж������߼�¼ʹ����: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'������������strRecPath��ָ��·�� '" + strRecPath + "'��ɾ������ʧ�ܡ�";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "֤����� '" + strOldBarcode + "' �Ѿ������ж������߼�¼ʹ����: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'����δָ����¼·��������£��޷���λ��ɾ����";
                        goto ERROR1;
                    }
                }
                else
                {

                    strRecPath = aPath[0];
                    // strReaderDbName = ResPath.GetDbName(strRecPath);
                }
            }

            // ɾ��������API �� strRecPath ��������Ϊ�գ���������Ҫ�������һ��
            if (this.TestMode == true || sessioninfo.TestMode == true)
            {
                // �������ģʽ
                // return:
                //      -1  �����̳���
                //      0   ����ͨ��
                //      1   ������ͨ��
                nRet = CheckTestModePath(strRecPath,
                    out strError);
                if (nRet != 0)
                {
                    strError = "ɾ�����߼�¼�Ĳ������ܾ�: " + strError;
                    goto ERROR1;
                }
            }

            // Debug.Assert(strReaderDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

        REDOLOAD:


            // �ȶ������ݿ��д�λ�õ����м�¼
            lRet = channel.GetRes(strRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    kernel_errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
                else
                {
                    strError = "ɾ��������������, �ڶ���ԭ�м�¼�׶�:" + strError;
                    kernel_errorcode = channel.OriginErrorCode;
                    goto ERROR1;
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

            string strExistingBarcode = DomUtil.GetElementText(domExist.DocumentElement, "barcode");


            // �۲��Ѿ����ڵļ�¼�У�֤������Ƿ��strOldBarcodeһ��
            if (String.IsNullOrEmpty(strOldBarcode) == false)
            {
                if (strExistingBarcode != strOldBarcode)
                {
                    strError = "·��Ϊ '" + strRecPath + "' �Ķ��߼�¼��<barcode>Ԫ���е�֤����� '" + strExistingBarcode + "' ��strOldXml��<barcode>Ԫ���е�֤����� '" + strOldBarcode + "' ��һ�¡��ܾ�ɾ��(�������ɾ���������ɲ�����ɾ���˱�Ķ��߼�¼��Σ��)��";
                    goto ERROR1;
                }
           }

            // ��� LoginCache
            // this.LoginCache.Remove(strExistingBarcode);
            this.ClearLoginCache(strExistingBarcode);

            // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
            string strDetailInfo = "";
            bool bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                out strDetailInfo);

            if (bForce == false)
            {
                if (bHasCirculationInfo == true)
                {
                    strError = "ɾ���������ܾ�������ɾ���Ķ��߼�¼ '" + strRecPath + "' �а����� " + strDetailInfo + "";
                    goto ERROR2;
                }
            }

            // �Ƚ�ʱ���
            // �۲�ʱ����Ƿ����仯
            nRet = ByteArray.Compare(baOldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 2008/5/29 new add
                if (bForce == true)
                {
                    strError = "���ݿ��м���ɾ���Ķ��߼�¼�Ѿ������˱仯��������װ�ء���ϸ�˶Ժ�����ɾ����";
                    kernel_errorcode = ErrorCodeValue.TimestampMismatch;
                    baNewTimestamp = exist_timestamp;   // ��ǰ��֪�����м�¼ʵ���Ϸ������仯
                    goto ERROR1;
                }

                // �Ƿ񱨴�?
                // �������ľ�ϸһ�㣬��Ҫ�Ƚ�strOldXml��strExistingXml��Ҫ���ֶ��Ƿ񱻸ı��ˣ����û�иı䣬�ǲ��ر����

                // ���ǰ�˸����˾ɼ�¼�����кͿ��м�¼���бȽϵĻ���
                if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    // �Ƚ�������¼, �����Ͷ��߾�̬��Ϣ�йص��ֶ��Ƿ����˱仯
                    // return:
                    //      0   û�б仯
                    //      1   �б仯
                    nRet = IsReaderInfoChanged(
                        element_names,
                        domExist,
                        domOldRec);
                    if (nRet == 1)
                    {

                        strError = "���ݿ��м���ɾ���Ķ��߼�¼�Ѿ������˱仯��������װ�ء���ϸ�˶Ժ�����ɾ����";
                        kernel_errorcode = ErrorCodeValue.TimestampMismatch;

                        baNewTimestamp = exist_timestamp;   // ��ǰ��֪�����м�¼ʵ���Ϸ������仯
                        goto ERROR1;
                    }
                }

                baOldTimestamp = exist_timestamp;
                baNewTimestamp = exist_timestamp;   // ��ǰ��֪�����м�¼ʵ���Ϸ������仯
            }

            string strLibraryCode = "";
            // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out strLibraryCode) == false)
            {
                strError = "���߼�¼·�� '" + strRecPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                goto ERROR1;
            }

            byte[] output_timestamp = null;

            Debug.Assert(strRecPath != "", "");

            lRet = channel.DoDeleteRes(strRecPath,
                baOldTimestamp,
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                // 2009/7/17 new add
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strError = "֤�����Ϊ '" + strOldBarcode + "' �Ķ��߼�¼(��ɾ����ʱ����)�Ѳ�����";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    return 0;
                }

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "����ɾ��������ʱ�����ͻ, ����10��������Ȼʧ��";
                        baNewTimestamp = output_timestamp;
                        kernel_errorcode = channel.OriginErrorCode;
                        goto ERROR1;
                    }
                    // ����ʱ�����ƥ��
                    // �ظ�������ȡ�Ѵ��ڼ�¼\�ȽϵĹ���
                    nRedoCount++;
                    goto REDOLOAD;
                }


                baNewTimestamp = output_timestamp;
                strError = "ɾ��������������:" + strError;
                kernel_errorcode = channel.OriginErrorCode;
                goto ERROR1;
            }
            else
            {
                // �ɹ�
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // �������ڵĹݴ���

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");

                // ������<record>Ԫ��

                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistingXml);
                    DomUtil.SetAttr(node, "recPath", strRecPath);
                }

                // 2014/11/17
                if (bForce == true)
                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "style", "force");
                    if (string.IsNullOrEmpty(strDetailInfo) == false
                        && bHasCirculationInfo == true)
                        DomUtil.SetAttr(node, "description", strDetailInfo);
                }

                // ���ɾ���ɹ����򲻱�Ҫ�������з��ر�ʾ�ɹ�����ϢԪ����

                /// 
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(strLibraryCode,
    "�޸Ķ�����Ϣ",
    "ɾ����¼��",
    1);
            }

            return 1;
        ERROR1:
            kernel_errorcode = ErrorCodeValue.CommonError;
            domOperLog = null;  // ��ʾ����д����־
            return -1;
        ERROR2:
            kernel_errorcode = ErrorCodeValue.CommonError;
            domOperLog = null;  // ��ʾ����д����־
            return -2;
        }


        // ִ��"change"����
        // 1) �����ɹ���, NewRecord����ʵ�ʱ�����¼�¼��NewTimeStampΪ�µ�ʱ���
        // 2) �������TimeStampMismatch����OldRecord���п��з����仯��ġ�ԭ��¼����OldTimeStamp����ʱ���
        // return:
        //      -1  ����
        //      0   �ɹ�
        int DoReaderChange(
            string strCurrentLibraryCode,
            string [] element_names,
            string strAction,
            bool bForce,
            RmsChannel channel,
            string strRecPath,
            XmlDocument domNewRec,
            XmlDocument domOldRec,
            byte[] baOldTimestamp,
            ref XmlDocument domOperLog,
            out string strExistingRecord,
            out string strNewRecord,
            out byte[] baNewTimestamp,
            out string strError,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue errorcode)
        {
            strError = "";
            strExistingRecord = "";
            strNewRecord = "";
            baNewTimestamp = null;
            errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            int nRedoCount = 0;
            bool bExist = true;    // strRecPath��ָ�ļ�¼�Ƿ����?

            int nRet = 0;
            long lRet = 0;

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

        REDOLOAD:

            // �ȶ������ݿ��д�λ�õ����м�¼
            lRet = channel.GetRes(strRecPath,
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
                    strOutputPath = strRecPath;
                }
                else
                {
                    strError = "���������������, �ڶ���ԭ�м�¼�׶�:" + strError;
                    errorcode = channel.OriginErrorCode;
                    return -1;
                }
            }

            // �Ѽ�¼װ��DOM
            XmlDocument domExist = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            string strOldBarcode = "";
            string strNewBarcode = "";

            if (bExist == true) // 2008/5/29 new add
            {

                // �Ƚ��¾ɼ�¼��������Ƿ��иı�
                // return:
                //      -1  ����
                //      0   ���
                //      1   �����
                nRet = CompareTwoBarcode(domExist,
                    domNewRec,
                    out strOldBarcode,
                    out strNewBarcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strDetailInfo = "";  // ���ڶ��߼�¼�����Ƿ�����ͨ��Ϣ����ϸ��ʾ����
                bool bHasCirculationInfo = false;   // ���߼�¼�����Ƿ�����ͨ��Ϣ
                bool bDetectCiculationInfo = false; // �Ƿ��Ѿ�̽������߼�¼�е���ͨ��Ϣ

                if (nRet == 1)  // ����֤������иı�
                {
                    // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                    bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                        out strDetailInfo);
                    bDetectCiculationInfo = true;

                    if (bHasCirculationInfo == true
                        && bForce == false) // 2008/5/28 new add
                    {
                        // TODO: �ɷ���������ͬʱ�޸����������ѽ��Ĳ��¼�޸�����?
                        // ֵ��ע�������μ�¼��������־��������ν���recover������
                        strError = "����߼�¼ '" + strRecPath + "' �а����� " + strDetailInfo + "�������޸���ʱ֤������ֶ����ݲ��ܸı䡣(��ǰ֤����� '" + strOldBarcode + "'����ͼ�޸�Ϊ����� '" + strNewBarcode + "')";
                        goto ERROR1;
                    }
                }

                // ��� LoginCache
#if NO
                this.LoginCache.Remove(strOldBarcode);
                if (strNewBarcode != strOldBarcode)
                    this.LoginCache.Remove(strNewBarcode);
#endif
                this.ClearLoginCache(strOldBarcode);
                if (strNewBarcode != strOldBarcode)
                    this.ClearLoginCache(strNewBarcode);

                // 2009/1/23 new add

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
                    domNewRec,
                    out strOldState,
                    out strNewState,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                {
                    if (strOldState != "ע��" && strNewState == "ע��"
                        && bForce == false)
                    {
                        // �۲��Ѿ����ڵļ�¼�Ƿ�����ͨ��Ϣ
                        if (bDetectCiculationInfo == false)
                        {
                            bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                                out strDetailInfo);
                            bDetectCiculationInfo = true;
                        }

                        if (bHasCirculationInfo == true)
                        {
                            Debug.Assert(bDetectCiculationInfo == true, "");
                            strError = "ע���������ܾ������ⱻע���Ķ��߼�¼ '" + strRecPath + "' �а����� " + strDetailInfo + "��(��ǰ֤״̬ '" + strOldState + "', ��ͼ�޸�Ϊ��״̬ '" + strNewState + "')";
                            goto ERROR1;
                        }
                    }
                }
            }

            // �۲�ʱ����Ƿ����仯
            nRet = ByteArray.Compare(baOldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                if (bForce == true)
                {
                    // 2008/5/29 new add
                    // ��ǿ���޸�ģʽ�£�ʱ�����һ�������ش�ֱ�ӷ��س�����������Ҫ���ֶεıȶ��ж�
                    strError = "���������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + strRecPath + "') �ѷ������޸�";
                    errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
                    return -1;  // timestamp mismatch
                }

                // ʱ����������
                // ��Ҫ��domOldRec��strExistXml���бȽϣ������Ͷ�����Ϣ�йص�Ԫ�أ�Ҫ��Ԫ�أ�ֵ�Ƿ����˱仯��
                // �����ЩҪ��Ԫ�ز�δ�����仯���ͼ������кϲ������Ǳ������

                // �Ƚ�������¼, �����Ͳ��¼�йص��ֶ��Ƿ����˱仯
                // return:
                //      0   û�б仯
                //      1   �б仯
                nRet = IsReaderInfoChanged(
                    element_names,
                    domOldRec,
                    domExist);
                if (nRet == 1)
                {
                    // ������Ϣ��, �������޸Ĺ���ԭ��¼����ʱ���
                    strExistingRecord = strExistXml;
                    baNewTimestamp = exist_timestamp;

                    if (bExist == false)
                        strError = "���������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + strRecPath + "') �ѱ�ɾ����";
                    else
                        strError = "���������������: ���ݿ��е�ԭ��¼ (·��Ϊ'" + strRecPath + "') �ѷ������޸�";

                    errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
                    return -1;  // timestamp mismatch
                }

                // exist_timestamp��ʱ�Ѿ���ӳ�˿��б��޸ĺ�ļ�¼��ʱ���
            }

            // TODO: ��strAction==changestateʱ��ֻ����<state>��<comment>����Ԫ�����ݷ����仯



            // �ϲ��¾ɼ�¼
            string strNewXml = "";

            if (bForce == false)
            {
                nRet = MergeTwoReaderXml(
                    element_names,
                    strAction,
                    domExist,
                    domNewRec,
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 2008/5/29 new add
                strNewXml = domNewRec.OuterXml;
            }

            string strLibraryCode = "";
            // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out strLibraryCode) == false)
            {
                strError = "���߼�¼·�� '" + strRecPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                goto ERROR1;
            }

            // 2014/7/4
            if (this.VerifyReaderType == true)
            {
                string strReaderDbName = "";

                if (String.IsNullOrEmpty(strRecPath) == false)
                    strReaderDbName = ResPath.GetDbName(strRecPath);

                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);

                // ���һ�����¼�Ķ��������Ƿ����ֵ�б�Ҫ��
                // parameters:
                // return:
                //      -1  �����̳���
                //      0   ����Ҫ��
                //      1   ������Ҫ��
                nRet = CheckReaderType(domTemp,
                    strLibraryCode,
                    strReaderDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                {
                    strError = strError + "���޸Ķ��߼�¼����ʧ��";
                    goto ERROR1;
                }
            }

            // �����¼�¼
            byte[] output_timestamp = null;
            lRet = channel.DoSaveTextRes(strRecPath,
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
                        strError = "��������ʱ�����ͻ, ����10��������Ȼʧ��";
                        goto ERROR1;
                    }
                    // ����ʱ�����ƥ��
                    // �ظ�������ȡ�Ѵ��ڼ�¼\�ȽϵĹ���
                    nRedoCount++;
                    goto REDOLOAD;
                }

                strError = "���������������:" + strError;
                errorcode = channel.OriginErrorCode;
                return -1;
            }
            else // �ɹ�
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // �������ڵĹݴ���

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");

                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement, "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", strRecPath);

                node = DomUtil.SetElementText(domOperLog.DocumentElement, "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", strRecPath);

                // ����ɹ�����Ҫ������ϢԪ�ء���Ϊ��Ҫ�����µ�ʱ���
                baNewTimestamp = output_timestamp;
                strNewRecord = strNewXml;

                strError = "��������ɹ���NewTimeStamp�з������µ�ʱ�����NewRecord�з�����ʵ�ʱ�����¼�¼(���ܺ��ύ���¼�¼���в���)��";
                errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                /// 
                {
                    if (strAction == "change")
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "�޸Ķ�����Ϣ",
                            "�޸ļ�¼��",
                            1);
                    }
                    else if (strAction == "changestate")
                    {
                        string strNewState = DomUtil.GetElementText(domNewRec.DocumentElement,
                            "state");
                        if (String.IsNullOrEmpty(strNewBarcode) == true)
                            strNewState = "[����]";
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "�޸Ķ�����Ϣ֮״̬",
                            strNewState, 1);
                    }
                    else if (strAction == "changeforegift")
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            "�޸Ķ�����Ϣ֮Ѻ��",
                            "����",
                            1);
                    }
                }
            }

            return 0;

        ERROR1:
            errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.CommonError;
            return -1;
        }

        // ���߼�¼�Ƿ������ ��ͨ��Ϣ?
        // 2009/1/25 �������һ���Է���ȫ����ϸ��Ϣ
        // parameters:
        //      strDetail   �����ϸ������Ϣ
        static bool IsReaderHasCirculationInfo(XmlDocument dom,
            out string strDetail)
        {
            strDetail = "";
            int nRet = 0;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//borrows/borrow");
            if (nodes.Count > 0)
            {
                Debug.Assert(String.IsNullOrEmpty(strDetail) == true, "");
                strDetail = nodes.Count.ToString() + "���ڽ��";
            }

            nodes = dom.DocumentElement.SelectNodes("//overdues/overdue");
            if (nodes.Count > 0)
            {
                if (String.IsNullOrEmpty(strDetail) == false)
                    strDetail += "��";
                strDetail = nodes.Count.ToString() + "����������";
            }

            string strForegift = DomUtil.GetElementText(dom.DocumentElement,
                "foregift");
            // ����Ѻ��ֵ�������Ƿ�Ϊ0?
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                string strError = "";
                List<string> results = null;
                // ������"-123.4+10.55-20.3"�ļ۸��ַ����鲢����
                nRet = PriceUtil.SumPrices(strForegift,
                    out results,
                    out strError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "��";
                    strDetail = "Ѻ�����(������ַ��� '" + strForegift + "' ��ʽ����:" + strError + ")";
                    goto END1;
                }

                // �������ɸ��۸��ַ����Ƿ񶼱�ʾ��0?
                // return:
                //      -1  ����
                //      0   ��Ϊ0
                //      1   Ϊ0
                nRet = PriceUtil.IsZero(results,
                    out strError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "��";
                    strDetail = "Ѻ�����(���Խ���ַ��� '" + strForegift + "' �����Ƿ�Ϊ���жϵ�ʱ��������: " + strError + ")";
                    goto END1;
                }

                if (nRet == 0)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "��";
                    strDetail = "Ѻ�����";
                    goto END1;
                }
            }

            // TODO: �Ƿ�Ҫ���� //reservations/request ?


            END1:
            if (String.IsNullOrEmpty(strDetail) == false)
                return true;

            return false;
        }


        #endregion


        // Ϊ����XML��Ӹ�����Ϣ
        // parameters:
        //      strLibraryCode  ���߼�¼�������Ķ���߿�Ĺݴ���
        public int GetAdvanceReaderXml(
            SessionInfo sessioninfo,
            string strStyle,
            string strLibraryCode,
            string strReaderXml,
            out string strOutputXml,
            out string strError)
        {
            strOutputXml = "";
            strError = "";
            string strWarning = "";
            int nRet = 0;

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                return -1;
            }

            // �������		
            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            XmlNode nodeInfo = readerdom.CreateElement("info");
            readerdom.DocumentElement.AppendChild(nodeInfo);

            // �ɽ��ܲ���
            int nMaxBorrowItems = 0;
            XmlNode nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "�ɽ��ܲ���");

            string strParamValue = "";
            MatchResult matchresult;
            // return:
            //      reader��book���;�ƥ�� ��4��
            //      ֻ��reader����ƥ�䣬��3��
            //      ֻ��book����ƥ�䣬��2��
            //      reader��book���Ͷ���ƥ�䣬��1��
            nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "�ɽ��ܲ���",
                out strParamValue,
                out matchresult,
                out strError);
            if (nRet == -1 || nRet < 3)
                DomUtil.SetAttr(nodeInfoItem, "error", strError);
            else
            {
                DomUtil.SetAttr(nodeInfoItem, "value", strParamValue);

                try
                {
                    nMaxBorrowItems = System.Convert.ToInt32(strParamValue);
                }
                catch
                {
                    strWarning += "��ǰ���� �ɽ��ܲ��� ���� '" + strParamValue + "' ��ʽ����";
                }
            }

            // �������
            nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "������");


            Calendar calendar = null;
            nRet = this.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out calendar,
                out strError);
            if (nRet == -1)
            {
                strWarning += strError;
                calendar = null;
                DomUtil.SetAttr(nodeInfoItem, "error", strError);
            }
            else
            {
                if (calendar != null)
                    DomUtil.SetElementText(nodeInfoItem, "value", calendar.Name);
                else
                    DomUtil.SetElementText(nodeInfoItem, "value", "");
            }

            // ȫ��<borrow>Ԫ��
            XmlNodeList borrow_nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            int nFreeBorrowCount = Math.Max(0, nMaxBorrowItems - borrow_nodes.Count);

            // ��ǰ���ɽ�
            nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "��ǰ���ɽ�");
            DomUtil.SetAttr(nodeInfoItem, "value", nFreeBorrowCount.ToString());

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < borrow_nodes.Count; i++)
            {
                XmlNode node = borrow_nodes[i];

                /*
                string strNo = DomUtil.GetAttr(node, "no");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strSummary = "";
                 * */
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");

                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");

                if (StringUtil.IsInList("advancexml_borrow_bibliosummary", strStyle) == true)
                {
                    string strSummary = "";
                    string strBiblioRecPath = "";
                    LibraryServerResult result = this.GetBiblioSummary(
                        sessioninfo,
                        channel,
                        strBarcode,
                        strConfirmItemRecPath,
                        null,
                        out strBiblioRecPath,
                        out strSummary);
                    if (result.Value == -1)
                    {
                        // strSummary = result.ErrorInfo;
                    }
                    else
                    {
                        /*
                        // �ض�
                        if (strSummary.Length > 25)
                            strSummary = strSummary.Substring(0, 25) + "...";

                        if (strSummary.Length > 12)
                            strSummary = strSummary.Insert(12, "<br/>");
                         * */
                    }

                    DomUtil.SetAttr(node, "summary", strSummary);
                }

                string strOverdue = "";
                long lOver = 0;
                string strPeriodUnit = "";
                // ��鳬�������
                // return:
                //      -1  ���ݸ�ʽ����
                //      0   û�з��ֳ���
                //      1   ���ֳ���   strError������ʾ��Ϣ
                //      2   �Ѿ��ڿ������ڣ������׳��� 2009/3/13 new add
                nRet = this.CheckPeriod(
                    calendar,
                    strBorrowDate,
                    strPeriod,
                    out lOver,
                    out strPeriodUnit,
                    out strError);
                if (nRet == -1)
                {
                    DomUtil.SetAttr(node, "isOverdue", "error");
                    strOverdue = strError;
                }
                else if (nRet == 1)
                {
                    DomUtil.SetAttr(node, "isOverdue", "yes");
                    strOverdue = strError;	// "�ѳ���";
                }
                else
                {
                    DomUtil.SetAttr(node, "isOverdue", "no");
                    strOverdue = strError;	// ����Ҳ��һЩ��Ҫ����Ϣ������ǹ�����
                }

                DomUtil.SetAttr(node, "overdueInfo", strOverdue);


                {
                    string strOverDue = "";
                    // bool bOverdue = false;  // �Ƿ���

                    DateTime timeReturning = DateTime.MinValue;
                    string strTips = "";

                    DateTime timeNextWorkingDay;
                    lOver = 0;
                    strPeriodUnit = "";

                    // ��û�������
                    // return:
                    //      -1  ���ݸ�ʽ����
                    //      0   û�з��ֳ���
                    //      1   ���ֳ���   strError������ʾ��Ϣ
                    //      2   �Ѿ��ڿ������ڣ������׳��� 
                    nRet = this.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        strTips = strError;
                        if (nRet == 1)
                        {
                            // bOverdue = true;
                            strOverDue = " ("
                                + string.Format(this.GetString("�ѳ���s"),  // �ѳ��� {0}
                                                this.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                + ")";
                        }
                    }

                    DomUtil.SetAttr(node, "overdueInfo1", strOverdue);
                    DomUtil.SetAttr(node, "timeReturning", DateTimeUtil.Rfc1123DateTimeStringEx(timeReturning.ToLocalTime()));  // 2012/6/1 ���� ToLocalTime()
                }

            }

            if (String.IsNullOrEmpty(strWarning) == true)
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "warning", null);
            }
            else
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "warning", strWarning);
            }

            // ȫ��<overdue>Ԫ��
            XmlNodeList overdue_nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            for (int i = 0; i < overdue_nodes.Count; i++)
            {
                XmlNode node = overdue_nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");

                if (StringUtil.IsInList("advancexml_overdue_bibliosummary", strStyle) == true)
                {

                    if (String.IsNullOrEmpty(strBarcode) == false)
                    {
                        string strSummary = "";
                        string strBiblioRecPath = "";
                        LibraryServerResult result = this.GetBiblioSummary(
                            sessioninfo,
                            channel,
                            strBarcode,
                            strConfirmItemRecPath,
                            null,
                            out strBiblioRecPath,
                            out strSummary);
                        if (result.Value == -1)
                        {
                            // strSummary = result.ErrorInfo;
                        }
                        else
                        {
                            /*
                            // �ض�
                            if (strSummary.Length > 25)
                                strSummary = strSummary.Substring(0, 25) + "...";

                            if (strSummary.Length > 12)
                                strSummary = strSummary.Insert(12, "<br/>");
                             * */
                        }

                        DomUtil.SetAttr(node, "summary", strSummary);
                    }
                }

                string strReason = DomUtil.GetAttr(node, "reason");
                string strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                string strID = DomUtil.GetAttr(node, "id");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");


                // ��ͣ����
                string strPauseError = "";
                string strPauseInfo = "";
                if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true
                    && String.IsNullOrEmpty(strOverduePeriod) == false)
                {
                    string strPauseStart = DomUtil.GetAttr(node, "pauseStart");

                    string strUnit = "";
                    long lOverduePeriod = 0;

                    // �������޲���
                    nRet = LibraryApplication.ParsePeriodUnit(strOverduePeriod,
                        out lOverduePeriod,
                        out strUnit,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "�ڷ������޲����Ĺ����з�������: " + strError;
                        strPauseError += strError;
                    }

                    long lResultValue = 0;
                    string strPauseCfgString = "";
                    nRet = this.ComputePausePeriodValue(strReaderType,
                        strLibraryCode,
                            lOverduePeriod,
                            out lResultValue,
                        out strPauseCfgString,
                            out strError);
                    if (nRet == -1)
                    {
                        strError = "�ڼ�����ͣ�������ڵĹ����з�������: " + strError;
                        strPauseError += strError;
                    }

                    // text-level: �û���ʾ
                    /*
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                    {
                        strPauseInfo = "�� " + DateTimeUtil.LocalDate(strPauseStart) + " ��ʼ��";
                    }
                    strPauseInfo += "ͣ���� " + lResultValue.ToString() + app.GetDisplayTimeUnitLang(strUnit) + " (�����������: ���� " + lOverduePeriod.ToString() + app.GetDisplayTimeUnitLang(strUnit) + "���������� " + strReaderType + " �� ��ͣ�������� Ϊ " + strPauseCfgString + ")";
                     * */
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                    {
                        strPauseInfo = string.Format(this.GetString("��s��ʼ��ͣ����s"),
                            // "�� {0} ��ʼ��ͣ���� {1} (�����������: ���� {2}���������� {3} �� ��ͣ�������� Ϊ {4})"
                            DateTimeUtil.LocalDate(strPauseStart),
                            lResultValue.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            lOverduePeriod.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            strReaderType,
                            strPauseCfgString);
                    }
                    else
                    {
                        strPauseInfo = string.Format(this.GetString("ͣ����s"),
                            // "ͣ���� {0} (�����������: ���� {1}���������� {2} �� ��ͣ�������� Ϊ {3})"
                            lResultValue.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            lOverduePeriod.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            strReaderType,
                            strPauseCfgString);
                    }


                }

                if (String.IsNullOrEmpty(strPauseInfo) == false)
                {
                    strPrice = string.Format(this.GetString("ΥԼ�����ͣ����"),    // "{0} -- �� -- {1}"
                        strPrice,
                        strPauseInfo);
                    // " �� "

                    DomUtil.SetAttr(node, "priceString", strPrice);
                }
                else if (String.IsNullOrEmpty(strPauseError) == false)
                {
                    DomUtil.SetAttr(node, "priceString", strPauseError);
                }


            }

            if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true)
            {
                // �������
                strError = "";
                // �㱨��ͣ�������
                string strPauseMessage = "";
                nRet = this.HasPauseBorrowing(
                    calendar,
                    strLibraryCode,
                    readerdom,
                    out strPauseMessage,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strPauseMessage = "�ڼ�����ͣ����Ĺ����з�������: " + strError;
                }
                if (nRet == 1 || String.IsNullOrEmpty(strPauseMessage) == false)
                {
                    XmlNode node = readerdom.DocumentElement.SelectSingleNode("overdues");
                    if (node == null)
                    {
                        node = readerdom.CreateElement("overdues");
                        readerdom.DocumentElement.AppendChild(node);
                    }

                    DomUtil.SetAttr(node, "pauseMessage", strPauseMessage);
                }
            }

            strOutputXml = readerdom.OuterXml;
            return 0;
        }

        // ��ö�����Ϣ
        // parameters:
        //      strBarcode  ����֤����š����ǰ��������"@path:"�����ʾ���߼�¼·������@path�����£�·�����滹���Ը��� "$prev"��"$next"��ʾ����
        //                  ����ʹ�ö���֤�Ŷ�ά��
        //                  TODO: �Ƿ����ʹ�����֤��?
        //      strResultTypeList   ����������� xml/html/text/calendar/advancexml/recpaths/summary
        //              ����calendar��ʾ��ö�������������������advancexml��ʾ���������˵��ṩ�˷ḻ������Ϣ��xml��������г��ں�ͣ���ڸ�����Ϣ
        //      strRecPath  [out] ���߼�¼·����������ж�����߼�¼�������Ƕ��ŷָ���·���б��ַ�������� 100 ��·��
        // Result.Value -1���� 0û���ҵ� 1�ҵ� >1���ж���1��
        // Ȩ��: 
        //		������Ա���߶��ߣ�������getreaderinfoȨ��
        //		���Ϊ����, �������ƻ�ֻ�ܿ������Լ��Ķ�����Ϣ
        public LibraryServerResult GetReaderInfo(
            SessionInfo sessioninfo,
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp)
        {
            results = null;
            baTimestamp = null;
            strRecPath = "";

            List<string> recpaths = null;

            LibraryServerResult result = new LibraryServerResult();

            // ������ի��
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;

            // Ȩ���ж�

            // Ȩ���ַ���
            if (StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "��ȡ������Ϣ���ܾ������߱�getreaderinfoȨ�ޡ�";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            string strError = "";
            // 2007/12/2 new add
            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "strBarcode����ֵ����Ϊ��";
                goto ERROR1;
            }

            // �Զ�����ݵĸ����ж�
            if (sessioninfo.UserType == "reader")
            {
                // TODO: ���ʹ�����֤�ţ��ƺ�����������谭
                if (strBarcode[0] != '@'
                    && StringUtil.HasHead(strBarcode, "PQR:") == false)
                {
                    if (StringUtil.IsIdcardNumber(strBarcode) == true)
                    {
                        // 2013/5/20
                        // �ӳ��ж�
                    }
                    else if (strBarcode != sessioninfo.Account.Barcode 
                        && string.IsNullOrEmpty(strPersonalLibrary) == true)
                    {
                        // ע�����и�����ի�ģ������Լ������ִ��
                        result.Value = -1;
                        result.ErrorInfo = "��ö�����Ϣ���ܾ�����Ϊ����ֻ�ܲ쿴�Լ��Ķ��߼�¼";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // ���滹Ҫ�ж�
            }

            string strIdcardNumber = "";
            string strXml = "";

            string strOutputPath = "";

            int nRet = 0;
            long lRet = 0;

            // ǰ���ṩ��ʱ��¼
            if (strBarcode[0] == '<')
            {
                strXml = strBarcode;
                strRecPath = "?";
                strOutputPath = "?";
                // TODO: ���ݿ�����Ҫ��ǰ�˷�����XML��¼�л�ȡ������Ҫ֪����ǰ�û��Ĺݴ���?
                goto SKIP1;
            }

            bool bOnlyBarcode = false;   // �Ƿ������ ֤�������Ѱ��

            bool bRecordGetted = false; // ��¼�ͷź��Ѿ���ȡ��

            // ����״̬
            if (strBarcode[0] == '@')
            {
                // ��ò��¼��ͨ�����¼·��
                string strLeadPath = "@path:";
                string strLeadDisplayName = "@displayName:";
                string strLeadBarcode = "@barcode:";

                /*
                if (strBarcode.Length <= strLead.Length)
                {
                    strError = "����ļ����ʸ�ʽ: '" + strBarcode + "'";
                    goto ERROR1;
                }
                string strPart = strBarcode.Substring(0, strLead.Length);
                 * */
                if (StringUtil.HasHead(strBarcode, strLeadPath) == true)
                {
                    string strReaderRecPath = strBarcode.Substring(strLeadPath.Length);

                    // 2008/6/20 new add
                    // ���������(����)�����
                    string strCommand = "";
                    nRet = strReaderRecPath.IndexOf("$");
                    if (nRet != -1)
                    {
                        strCommand = strReaderRecPath.Substring(nRet + 1);
                        strReaderRecPath = strReaderRecPath.Substring(0, nRet);
                    }

#if NO
                    string strReaderDbName = ResPath.GetDbName(strReaderRecPath);
                    // ��Ҫ���һ�����ݿ����Ƿ�������Ķ��߿���֮��
                    if (this.IsReaderDbName(strReaderDbName) == false)
                    {
                        strError = "���߼�¼·�� '" + strReaderRecPath + "' �е����ݿ��� '" + strReaderDbName + "' �������õĶ��߿���֮�У���˾ܾ�������";
                        goto ERROR1;
                    }
#endif
                    if (this.IsReaderRecPath(strReaderRecPath) == false)
                    {
                        strError = "��¼·�� '" + strReaderRecPath + "' ������һ�����߿��¼·������˾ܾ�������";
                        goto ERROR1;
                    }

                    string strMetaData = "";

                    RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }

                    // 2008/6/20 changed
                    string strStyle = "content,data,metadata,timestamp,outputpath";

                    if (String.IsNullOrEmpty(strCommand) == false
            && (strCommand == "prev" || strCommand == "next"))
                    {
                        strStyle += "," + strCommand;
                    }

                    // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                    if (this.IsCurrentChangeableReaderPath(strReaderRecPath,
                        sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "���߼�¼·�� '" + strReaderRecPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                        goto ERROR1;
                    }

                    lRet = channel.GetRes(strReaderRecPath,
                        strStyle,
                        out strXml,
                        out strMetaData,
                        out baTimestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.Value = 0;
                            if (strCommand == "prev")
                                result.ErrorInfo = "��ͷ";
                            else if (strCommand == "next")
                                result.ErrorInfo = "��β";
                            else
                                result.ErrorInfo = "û���ҵ�";
                            result.ErrorCode = ErrorCode.NotFound;
                            return result;
                        }

                        nRet = -1;
                    }
                    else
                    {
                        nRet = 1;
                    }

                    bRecordGetted = true;
                }
                else if (StringUtil.HasHead(strBarcode, strLeadDisplayName) == true)
                {
                    // 2011/2/19
                    string strDisplayName = strBarcode.Substring(strLeadDisplayName.Length);

                    // ͨ��������ʾ����ö��߼�¼
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   ����1��
                    //      >1  ���ж���1��
                    nRet = this.GetReaderRecXmlByDisplayName(
                        sessioninfo.Channels,
                        strDisplayName,
                        out strXml,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        result.ErrorInfo = "û���ҵ�";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }
                    // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                    if (this.IsCurrentChangeableReaderPath(strOutputPath,
                        sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "���߼�¼·�� '" + strOutputPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                        goto ERROR1;
                    }
                    bRecordGetted = true;
                }
                else if (StringUtil.HasHead(strBarcode, strLeadBarcode) == true)
                {
                    strBarcode = strBarcode.Substring(strLeadBarcode.Length);
                    bOnlyBarcode = true;
                    bRecordGetted = false;
                }
                else
                {
                    strError = "��֧�ֵļ����ʸ�ʽ: '" + strBarcode + "'��Ŀǰ��֧��'@path:'��'@displayName:'�����ļ�����";
                    goto ERROR1;
                }

                result.ErrorInfo = strError;
                result.Value = nRet;

                //

            }
            
            // ��֤����Ż��
            if (bRecordGetted == false)
            {
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    string strOutputCode = "";
                    // �Ѷ�ά���ַ���ת��Ϊ����֤�����
                    // parameters:
                    //      strReaderBcode  [out]����֤�����
                    // return:
                    //      -1      ����
                    //      0       ���������ַ������Ƕ���֤�Ŷ�ά��
                    //      1       �ɹ�      
                    nRet = this.DecodeQrCode(strBarcode,
                        out strOutputCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        // strQrCode = strBarcode;
                        strBarcode = strOutputCode;
                    }
                }

                // �Ӷ���
                // ���Ա����õ����߼�¼������;����ʱ״̬
                this.ReaderLocks.LockForRead(strBarcode);

                try
                {

                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   ����1��
                    //      >1  ���ж���1��
                    nRet = this.GetReaderRecXml(
                        sessioninfo.Channels,
                        strBarcode,
                        100,
                        sessioninfo.LibraryCodeList,
                        out recpaths,
                        out strXml,
                        // out strOutputPath,
                        out baTimestamp,
                        out strError);

                }
                finally
                {
                    this.ReaderLocks.UnlockForRead(strBarcode);
                }

#if NO
                if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                    && nRet > 0)
                {
                    // nRet ������
                    nRet = FilterReaderRecPath(ref recpaths,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
#endif

                if (nRet > 0)
                    strOutputPath = recpaths[0];

                if (nRet == 0)
                {
                    if (bOnlyBarcode == true)
                        goto NOT_FOUND;
                    // ��������֤�ţ�����̽���������֤�š�;��
                    if (StringUtil.IsIdcardNumber(strBarcode) == true)
                    {
                        strIdcardNumber = strBarcode;
                        strBarcode = "";

                        // ͨ���ض�����;����ö��߼�¼
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   ����1��
                        //      >1  ���ж���1��
                        /*
                        nRet = this.GetReaderRecXmlByFrom(
                            sessioninfo.Channels,
                            strIdcardNumber,
                            "���֤��",
                            out strXml,
                            out strOutputPath,
                            out baTimestamp,
                            out strError);
                         * */
                        nRet = this.GetReaderRecXmlByFrom(
    sessioninfo.Channels,
    null,
    strIdcardNumber,
    "���֤��",
    100,
    sessioninfo.LibraryCodeList,
    out recpaths,
    out strXml,
                            // out strOutputPath,
    out baTimestamp,
    out strError);
                        if (nRet == -1)
                        {
                            // text-level: �ڲ�����
                            strError = "�����֤�� '" + strIdcardNumber + "' ������߼�¼ʱ��������: " + strError;
                            goto ERROR1;
                        }
#if NO
                        if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                            && nRet > 0)
                        {
                            // nRet ������
                            nRet = FilterReaderRecPath(ref recpaths,
                                sessioninfo.LibraryCodeList,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
#endif

                        if (nRet == 0)
                        {
                            result.Value = 0;
                            // text-level: �û���ʾ
                            result.ErrorInfo = string.Format(this.GetString("���֤��s������"),   // "���֤�� '{0}' ������"
                                strIdcardNumber);
                            result.ErrorCode = ErrorCode.IdcardNumberNotFound;
                            return result;
                        }



                        if (nRet > 0)
                            strOutputPath = recpaths[0];

                        /*
                 * �������Ա���ǰ�˴ӷ���ֵ�Ѿ����Կ�������
                        if (nRet > 1)
                        {
                            // text-level: �û���ʾ
                            result.Value = -1;
                            result.ErrorInfo = "�����֤�� '" + strIdcardNumber + "' �������߼�¼���� " + nRet.ToString() + " ��������޷������֤�������н軹�����������֤����������н軹������";
                            result.ErrorCode = ErrorCode.IdcardNumberDup;
                            return result;
                        }
                        Debug.Assert(nRet == 1, "");
                         * */

                        result.ErrorInfo = strError;
                        result.Value = nRet;
                        goto SKIP0;
                    }
                    else
                    {
                        // �����Ҫ���Ӷ���֤�ŵȸ���;�����м���
                        foreach (string strFrom in this.PatronAdditionalFroms)
                        {
                            nRet = this.GetReaderRecXmlByFrom(
sessioninfo.Channels,
null,
strBarcode,
strFrom,
100,
sessioninfo.LibraryCodeList,
out recpaths,
out strXml,
out baTimestamp,
out strError);
                            if (nRet == -1)
                            {
                                // text-level: �ڲ�����
                                strError = "��" + strFrom + " '" + strBarcode + "' ������߼�¼ʱ��������: " + strError;
                                goto ERROR1;
                            }

#if NO
                            if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                                && nRet > 0)
                            {
                                // nRet ������
                                nRet = FilterReaderRecPath(ref recpaths,
                                    sessioninfo.LibraryCodeList,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }
#endif

                            if (nRet == 0)
                                continue;

                            if (nRet > 0)
                                strOutputPath = recpaths[0];

                            result.ErrorInfo = strError;
                            result.Value = nRet;
                            goto SKIP0;
                        }
                    }

                NOT_FOUND:
                    result.Value = 0;
                    result.ErrorInfo = "û���ҵ�";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }


                // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                if (this.IsCurrentChangeableReaderPath(strOutputPath,
                    sessioninfo.LibraryCodeList) == false)
                {
                    strError = "���߼�¼·�� '" + strOutputPath + "' �Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                    goto ERROR1;
                }


                /*
                 * �������Ա���ǰ�˴ӷ���ֵ�Ѿ����Կ�������
                if (nRet > 1)
                {
                    result.Value = nRet;
                    result.ErrorInfo = "����֤����� '" +strBarcode+ "' ���� " +nRet.ToString() + " ��������һ�����ش�����ϵͳ����Ա�����ų���";
                    result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                    return result;
                }
                 * */

                if (nRet == -1)
                    goto ERROR1;

                result.ErrorInfo = strError;
                result.Value = nRet;
            }

        SKIP0:
            // strRecPath = strOutputPath;
            // 2013/5/21
            if (recpaths != null)
                strRecPath = StringUtil.MakePathList(recpaths);
            else
                strRecPath = strOutputPath;


        SKIP1:
            if (String.IsNullOrEmpty(strResultTypeList) == true)
            {
                results = null; // �������κν��
                return result;
            }

            XmlDocument readerdom = null;
            if (sessioninfo.UserType == "reader")
            {
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "װ�ض��߼�¼���� XML DOM ʱ��������: " + strError;
                    goto ERROR1;
                }


                // �Զ�����ݵĸ����ж�
                if (sessioninfo.UserType == "reader"
                    && string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    string strBarcode1 = DomUtil.GetElementText(readerdom.DocumentElement,
            "barcode");
                    if (strBarcode1 != sessioninfo.Account.Barcode)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "��ö�����Ϣ���ܾ�����Ϊ����ֻ�ܲ쿴�Լ��Ķ��߼�¼";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            string strLibraryCode = "";
            if (strRecPath == "?")
            {
                // �ӵ�ǰ�û���Ͻ�Ĺݴ�����ѡ���һ��
                // TODO: ���������XML��¼���ж��߿����͹ݴ�������ж������
                List<string> librarycodes = StringUtil.FromListString(sessioninfo.LibraryCodeList);
                if (librarycodes != null && librarycodes.Count > 0)
                    strLibraryCode = librarycodes[0];
                else
                    strLibraryCode = "";
            }
            else
            {
                nRet = this.GetLibraryCode(strRecPath,
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string[] result_types = strResultTypeList.Split(new char[] { ',' });
            results = new string[result_types.Length];

            for (int i = 0; i < result_types.Length; i++)
            {
                string strResultType = result_types[i];

                // 2008/4/3 new add
                // if (String.Compare(strResultType, "calendar", true) == 0)
                if (IsResultType(strResultType, "calendar") == true)
                {
                    if (readerdom == null)
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "װ�ض��߼�¼����XML DOMʱ��������: " + strError;
                            goto ERROR1;
                        }
                    }

                    string strReaderType = DomUtil.GetElementText(readerdom, "readerType");

                    // �������
                    DigitalPlatform.LibraryServer.Calendar calendar = null;
                    nRet = this.GetReaderCalendar(strReaderType,
                        strLibraryCode,
                        out calendar,
                        out strError);
                    if (nRet == -1)
                    {
                        calendar = null;
                    }

                    string strCalendarName = "";

                    if (calendar != null)
                        strCalendarName = calendar.Name;

                    results[i] = strCalendarName;
                }
                // else if (String.Compare(strResultType, "xml", true) == 0)
                else if (IsResultType(strResultType, "xml") == true)
                {
                    // results[i] = strXml;
                    string strResultXml = "";
                    nRet = GetItemXml(strXml,
        strResultType,
        out strResultXml,
        out strError);
                    if (nRet == -1)
                    {
                        strError = "��ȡ " + strResultType + " ��ʽ�� XML �ַ���ʱ����: " + strError;
                        goto ERROR1;
                    }
                    results[i] = strResultXml;
                }
                else if (String.Compare(strResultType, "timestamp", true) == 0)
                {
                    // 2011/1/27
                    results[i] = ByteArray.GetHexTimeStampString(baTimestamp);
                }
                else if (String.Compare(strResultType, "recpaths", true) == 0)
                {
                    // 2013/5/21
                    if (recpaths != null)
                        results[i] = StringUtil.MakePathList(recpaths);
                    else
                        results[i] = strOutputPath;
                }
                else if (String.Compare(strResultType, "advancexml_borrow_bibliosummary", true) == 0
                    || String.Compare(strResultType, "advancexml_overdue_bibliosummary", true) == 0)
                {
                    // 2011/1/27
                    continue;
                }
                // else if (String.Compare(strResultType, "summary", true) == 0)
                else if (IsResultType(strResultType, "summary") == true)
                {
                    // 2013/11/15
                    string strSummary = "";
                    XmlDocument dom = new XmlDocument();
                    try {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strSummary = "���� XML װ�� DOM ����: " +ex.Message;
                        results[i] = strSummary;
                        continue;
                    }
                    strSummary = DomUtil.GetElementText(dom.DocumentElement, "name");
                    results[i] = strSummary;
                }
                // else if (String.Compare(strResultType, "advancexml", true) == 0)
                else if (IsResultType(strResultType, "advancexml") == true)
                {
                    // 2008/4/3 new add
                    string strOutputXml = "";
                    nRet = this.GetAdvanceReaderXml(
                        sessioninfo,
                        strResultTypeList,  // strResultType, BUG!!! 2012/4/8
                        strLibraryCode,
                        strXml,
                        out strOutputXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "GetAdvanceReaderXml()����: " + strError;
                        goto ERROR1;
                    }
                    results[i] = strOutputXml;
                }
                // else if (String.Compare(strResultType, "html", true) == 0)
                else if (IsResultType(strResultType, "html") == true)
                {

                    string strReaderRecord = "";
                    // �����߼�¼���ݴ�XML��ʽת��ΪHTML��ʽ
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2html.cs",
                        this.CfgDir + "\\readerxml2html.cs.ref",
                        strLibraryCode,
                        strXml,
                        strOutputPath,  // 2009/10/18 new add
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()����(�ű�����Ϊ" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    // test strReaderRecord = "<html><body><p>test</p></body></html>";
                    results[i] = strReaderRecord;
                }
                // else if (String.Compare(strResultType, "text", true) == 0)
                else if (IsResultType(strResultType, "text") == true)
                {
                    string strReaderRecord = "";
                    // �����߼�¼���ݴ�XML��ʽת��Ϊtext��ʽ
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2text.cs",
                        this.CfgDir + "\\readerxml2text.cs.ref",
                        strLibraryCode,
                        strXml,
                        strOutputPath,  // 2009/10/18 new add
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()����(�ű�����Ϊ" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    results[i] = strReaderRecord;
                }
                else
                {
                    strError = "δ֪�Ľ������ '" + strResultType + "'";
                    goto ERROR1;
                }
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        static bool IsResultType(string strResultType, string strName)
        {
            if (String.Compare(strResultType, strName, true) == 0
                   || StringUtil.HasHead(strResultType, strName + ":") == true)
                return true;
            return false;
        }

        // ���ݹݴ��룬��������Ͻ�� ���߿��¼·���ַ��� ɸѡɾ��
        // return:
        //      -1  ����
        //      ����  recpaths �����Ԫ������
        int FilterReaderRecPath(ref List<string> recpaths,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (recpaths == null)
                return 0;

            List<string> results = new List<string>();
            foreach (string strReaderRecPath in recpaths)
            {
                // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                if (this.IsCurrentChangeableReaderPath(strReaderRecPath,
                    strLibraryCodeList) == true)
                    results.Add(strReaderRecPath);
            }

            recpaths = results;
            return recpaths.Count;
        }

        // ���һ�����ݿ����Ƿ�������Ķ��߿���֮��
        public bool IsReaderRecPath(string strRecPath)
        {
            string strReaderDbName = ResPath.GetDbName(strRecPath);
            return this.IsReaderDbName(strReaderDbName);
        }

        // �ƶ����߼�¼
        // parameters:
        //      strTargetRecPath    [in][out]Ŀ���¼·��
        // return:
        // result.Value:
        //      -1  error
        //      0   �Ѿ��ɹ��ƶ�
        // Ȩ�ޣ�
        //      ��ҪmovereaderinfoȨ��
        // ��־:
        //      Ҫ������־
        public LibraryServerResult MoveReaderInfo(
            SessionInfo sessioninfo,
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte [] target_timestamp)
        {
            string strError = "";
            target_timestamp = null;
            int nRet = 0;
            long lRet = 0;
            // bool bChanged = false;  // �Ƿ�����ʵ���ԸĶ�

            LibraryServerResult result = new LibraryServerResult();

            // Ȩ���ַ���
            if (StringUtil.IsInList("movereaderinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "�ƶ����߼�¼�Ĳ������ܾ������߱�movereaderinfoȨ�ޡ�";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // ���Դ��Ŀ���¼·��������ͬ
            if (strSourceRecPath == strTargetRecPath)
            {
                strError = "Դ��Ŀ����߼�¼·��������ͬ";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strSourceRecPath) == true)
            {
                strError = "Դ���߼�¼·������Ϊ��";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strTargetRecPath) == true)
            {
                strError = "Ŀ����߼�¼·������Ϊ��";
                goto ERROR1;
            }

            // �������·���Ƿ��Ƕ��߿�·��
            if (this.IsReaderRecPath(strSourceRecPath) == false)
            {
                strError = "strSourceRecPath������������Դ��¼·�� '"+strSourceRecPath+"' ������һ�����߿��¼·��";
                goto ERROR1;
            }
            if (this.IsReaderRecPath(strTargetRecPath) == false)
            {
                strError = "strTargetRecPath������������Ŀ���¼·�� '" + strTargetRecPath + "' ������һ�����߿��¼·��";
                goto ERROR1;
            }
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            // ����Դ��¼
            string strExistingSourceXml = "";
            byte[] exist_soutce_timestamp = null;
            string strTempOutputPath = "";
            string strMetaData = "";
            int nRedoCount = 0;

        REDOLOAD:

            // �ȶ������ݿ��д�λ�õ����м�¼
            lRet = channel.GetRes(strSourceRecPath,
                out strExistingSourceXml,
                out strMetaData,
                out exist_soutce_timestamp,
                out strTempOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strError = "Դ��¼ '" + strSourceRecPath + "' ������";
                    // errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
                else
                {
                    strError = "�ƶ�������������, �ڶ���Դ��¼ '" + strSourceRecPath + "' �׶�:" + strError;
                    // errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
            }

            string strSourceLibraryCode = "";
            string strTargetLibraryCode = "";
            // �������߼�¼�������Ķ��߿�Ĺݴ��룬�Ƿ񱻵�ǰ�û���Ͻ
            if (String.IsNullOrEmpty(strTempOutputPath) == false)
            {
                // ��鵱ǰ�������Ƿ��Ͻ������߿�
                // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                if (this.IsCurrentChangeableReaderPath(strTempOutputPath,
        sessioninfo.LibraryCodeList,
        out strSourceLibraryCode) == false)
                {
                    strError = "Դ���߼�¼·�� '" + strTempOutputPath + "' �����Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                    goto ERROR1;
                }
            }

            // �Ѽ�¼װ��DOM
            XmlDocument domExist = new XmlDocument();
            try
            {
                domExist.LoadXml(strExistingSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistingSourceXmlװ�ؽ���DOMʱ��������: " + ex.Message;
                goto ERROR1;
            }

            string strLockBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                "barcode");

            // �Ӷ��߼�¼��
            if (String.IsNullOrEmpty(strLockBarcode) == false)
                this.ReaderLocks.LockForWrite(strLockBarcode);
            try
            {
                // ���������¶���һ��Դ���߼�¼��������Ϊ���ĵ�һ��Ϊ�˻��֤����ŵĶ�ȡ������֮����ڿ��ܱ������ط��޸��˴�����¼�Ŀ���
                byte[] temp_timestamp = null;
                lRet = channel.GetRes(strSourceRecPath,
                    out strExistingSourceXml,
                    out strMetaData,
                    out temp_timestamp,
                    out strTempOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "�ƶ�������������, �����¶���Դ��¼ '" + strSourceRecPath + "' �׶�:" + strError;
                    goto ERROR1;
                }

                nRet = ByteArray.Compare(exist_soutce_timestamp, temp_timestamp);
                if (nRet != 0)
                {
                    // ���°Ѽ�¼װ��DOM
                    domExist = new XmlDocument();
                    try
                    {
                        domExist.LoadXml(strExistingSourceXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistingSourceXmlװ�ؽ���DOMʱ��������(2): " + ex.Message;
                        goto ERROR1;
                    }

                    // ���º˶������
                    if (strLockBarcode != DomUtil.GetElementText(domExist.DocumentElement,
                "barcode"))
                    {
                        if (nRedoCount < 10)
                        {
                            nRedoCount++;
                            goto REDOLOAD;
                        }
                        strError = "�������������з���̫��εĴ������Ժ������ƶ�����";
                        goto ERROR1;
                    }

                    exist_soutce_timestamp = temp_timestamp;
                }


                // ��鼴�����ǵ�Ŀ��λ���ǲ����м�¼������У����������move������
                bool bAppendStyle = false;  // Ŀ��·���Ƿ�Ϊ׷����̬��
                string strTargetRecId = ResPath.GetRecordId(strTargetRecPath);

                if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                {
                    // 2009/11/1 new add
                    if (String.IsNullOrEmpty(strTargetRecId) == true)
                        strTargetRecPath += "/?";

                    bAppendStyle = true;
                }


                if (bAppendStyle == false)
                {
                    string strExistTargetXml = "";
                    byte[] exist_target_timestamp = null;
                    string strOutputPath = "";

                    // ��ȡ����Ŀ��λ�õ����м�¼
                    lRet = channel.GetRes(strTargetRecPath,
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
                        }
                        else
                        {
                            strError = "�ƶ�������������, �ڶ��뼴�����ǵ�Ŀ��λ�� '" + strTargetRecPath + "' ԭ�м�¼�׶�:" + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        // �����¼���ڣ���Ŀǰ�����������Ĳ���
                        strError = "�ƶ��������ܾ�����Ϊ�ڼ������ǵ�Ŀ��λ�� '" + strTargetRecPath + "' �Ѿ����ڼ�¼��������ɾ��(delete)������¼�����ܽ����ƶ�(move)����";
                        goto ERROR1;
                    }
                }

                // �������߼�¼�������Ķ��߿�Ĺݴ��룬�Ƿ񱻵�ǰ�û���Ͻ
                if (String.IsNullOrEmpty(strTargetRecPath) == false)
                {
                    // ��鵱ǰ�������Ƿ��Ͻ������߿�
                    // �۲�һ�����߼�¼·���������ǲ����ڵ�ǰ�û���Ͻ�Ķ��߿ⷶΧ��?
                    if (this.IsCurrentChangeableReaderPath(strTargetRecPath,
            sessioninfo.LibraryCodeList,
            out strTargetLibraryCode) == false)
                    {
                        strError = "Ŀ����߼�¼·�� '" + strTargetRecPath + "' �����Ķ��߿ⲻ�ڵ�ǰ�û���Ͻ��Χ��";
                        goto ERROR1;
                    }
                }

                // �ƶ���¼
                // byte[] output_timestamp = null;
                string strOutputRecPath = "";

                // TODO: Copy��Ҫдһ�Σ���ΪCopy����д���¼�¼��
                // ��ʵCopy���������ڴ�����Դ�����򻹲�����Save+Delete
                lRet = channel.DoCopyRecord(strSourceRecPath,
                     strTargetRecPath,
                     true,   // bDeleteSourceRecord
                     out target_timestamp,
                     out strOutputRecPath,
                     out strError);
                if (lRet == -1)
                {
                    strError = "DoCopyRecord() error :" + strError;
                    goto ERROR1;
                }

                strTargetRecPath = strOutputRecPath;

                /*
                if (String.IsNullOrEmpty(strNewBiblio) == false)
                {
                    this.BiblioLocks.LockForWrite(strOutputRecPath);

                    try
                    {
                        // TODO: ����µġ��Ѵ��ڵ�xmlû�в�ͬ�������µ�xmlΪ�գ����ⲽ�������ʡ��
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strOutputRecPath,
                            strNewBiblio,
                            false,
                            "content", // ,ignorechecktimestamp
                            output_timestamp,
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        this.BiblioLocks.UnlockForWrite(strOutputRecPath);
                    }
                }
                */

            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "�׳��쳣:" + ex.Message;
                return result;
            }
            finally
            {
                if (String.IsNullOrEmpty(strLockBarcode) == false)
                    this.ReaderLocks.UnlockForWrite(strLockBarcode);
            }

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strSourceLibraryCode + "," + strTargetLibraryCode);    // �������ڵĹݴ���
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "setReaderInfo");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "action", "move");

            string strOperTimeString = this.Clock.GetClock();   // RFC1123��ʽ

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTimeString);

            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "record", "");
            DomUtil.SetAttr(node, "recPath", strTargetRecPath);

            node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "oldRecord", strExistingSourceXml);
            DomUtil.SetAttr(node, "recPath", strSourceRecPath);

            nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "MoveReaderInfo() API д����־ʱ��������: " + strError;
                goto ERROR1;
            }

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // Ϊ�������߼�¼������Ӻ��ѹ�ϵ
        public int AddFriends(
            SessionInfo sessioninfo,
            string strReaderBarcode1,
            string strReaderBarcode2,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> barcodes = new List<string>();
            barcodes.Add(strReaderBarcode1);
            barcodes.Add(strReaderBarcode2);

            barcodes.Sort();

            // �Ӷ��߼�¼��
            // �������������Է�ֹ����
            this.ReaderLocks.LockForWrite(barcodes[0]);
            this.ReaderLocks.LockForWrite(barcodes[1]);

            try // ���߼�¼������Χ��ʼ
            {

                // ������߼�¼
                string strReaderXml1 = "";
                byte[] reader_timestamp1 = null;
                string strOutputReaderRecPath1 = "";
                nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    strReaderBarcode1,
                    out strReaderXml1,
                    out strOutputReaderRecPath1,
                    out reader_timestamp1,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = "������߼�¼ '" + strReaderBarcode1 + "' ʱ��������: " + strError;
                    return -1;
                }

                string strReaderXml2 = "";
                byte[] reader_timestamp2 = null;
                string strOutputReaderRecPath2 = "";
                nRet = this.GetReaderRecXml(
                    sessioninfo.Channels,
                    strReaderBarcode2,
                    out strReaderXml2,
                    out strOutputReaderRecPath2,
                    out reader_timestamp2,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = "������߼�¼ '" + strReaderBarcode2 + "' ʱ��������: " + strError;
                    return -1;
                }

                XmlDocument readerdom1 = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml1,
                    out readerdom1,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = "װ�ض��߼�¼ '" + strReaderBarcode1 + "' ����XML DOMʱ��������: " + strError;
                    return -1;
                }

                XmlDocument readerdom2 = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml2,
                    out readerdom2,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: �ڲ�����
                    strError = "װ�ض��߼�¼ '" + strReaderBarcode2 + "' ����XML DOMʱ��������: " + strError;
                    return -1;
                }

                string strFriends1 = DomUtil.GetElementText(readerdom1.DocumentElement, "friends");
                string strFriends2 = DomUtil.GetElementText(readerdom2.DocumentElement, "friends");

                string strNewFriends1 = strFriends1;
                string strNewFriends2 = strFriends2;

                StringUtil.SetInList(ref strNewFriends1, strReaderBarcode2, true);
                StringUtil.SetInList(ref strNewFriends2, strReaderBarcode1, true);

                if (strNewFriends1 == strFriends1)
                    readerdom1 = null;

                if (strNewFriends2 == strFriends2)
                    readerdom2 = null;

                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }

                // д�ض��߼�¼
                if (readerdom1 != null)
                {
                    byte[] output_timestamp1 = null;
                    string strOutputPath1 = "";
                    long lRet = channel.DoSaveTextRes(strOutputReaderRecPath1,
                        readerdom1.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp1,
                        out output_timestamp1,
                        out strOutputPath1,
                        out strError);
                    if (lRet == -1)
                    {
                        // text-level: �ڲ�����
                        strError = "д����߼�¼ '" + strReaderBarcode1 + "' �����У���������: " + strError;
                        return -1;
                    }
                }

                // д�ض��߼�¼
                if (readerdom2 != null)
                {
                    byte[] output_timestamp2 = null;
                    string strOutputPath2 = "";
                    long lRet = channel.DoSaveTextRes(strOutputReaderRecPath2,
                        readerdom2.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp2,
                        out output_timestamp2,
                        out strOutputPath2,
                        out strError);
                    if (lRet == -1)
                    {
                        // text-level: �ڲ�����
                        strError = "д����߼�¼ '" + strReaderBarcode1 + "' �����У���������: " + strError;
                        return -1;
                    }
                }


            } // ���߼�¼������Χ����
            finally
            {
                this.ReaderLocks.UnlockForWrite(barcodes[1]);
                this.ReaderLocks.UnlockForWrite(barcodes[0]);
            }

            return 0;
        }
    }
}
