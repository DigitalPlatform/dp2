using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;
using DigitalPlatform.Z3950;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2ZServer
{
    public class Session : IDisposable
    {
        // private Socket m_clientSocket = null;    // Referance to client Socket.
        TcpClient _client = null;

        private Service _service = null;    // 

        private string _sessionID = "";      // Holds session ID.

        private DateTime _sessionStartTime;    // Session创建的时间

        private DateTime _activateTime;    // 最近一次使用过的时间

        public DateTime ActivateTime
        {
            get
            {
                return this._activateTime;
            }
        }

        LibraryChannel _channel = new LibraryChannel();

        string _groupId = "";
        string _userName = "";
        string _password = "";

        // 检索词的编码方式
        Encoding _searchTermEncoding = Encoding.GetEncoding(936);    // 缺省为GB2312编码方式
        // MARC记录的编码方式
        Encoding _marcRecordEncoding = Encoding.GetEncoding(936);    // 缺省为GB2312编码方式

        bool _bInitialized = false;    // 是否被Initial初始化成功。如果为false，则Initial()后续的请求都要被拒绝。
        long _lPreferredMessageSize = 500 * 1024;
        long _lExceptionalRecordSize = 500 * 1024;

        const long MaxPreferredMessageSize = 1024 * 1024;
        const long MaxExceptionalRecordSize = 1024 * 1024;

        public void Dispose()
        {
            if (this._client != null)
            {
                try
                {
                    this._client.Close();
                }
                catch
                {
                }
                this._client = null;
            }

            if (this._channel != null)
            {
                this._channel.Close();
                this._channel = null;
            }
        }

        internal Session(TcpClient client,
            Service server,
            string sessionID)
        {
            this._client = client;
            _service = server;
            _sessionID = sessionID;
            _sessionStartTime = DateTime.Now;
            _activateTime = _sessionStartTime;

            this._channel.Url = _service.LibraryServerUrl;

            this._channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this._userName;
                e.Password = this._password;
                e.Parameters = "location=z39.50 server, type=worker,client=dp2ZServer|0.01";
                /*
                e.IsReader = false;
                e.Location = "z39.50 server";
                 * */
                if (String.IsNullOrEmpty(e.UserName) == true)
                {
                    e.ErrorInfo = "没有指定用户名，无法自动登录";
                    e.Failed = true;
                    return;
                }

                return;
            }

            e.ErrorInfo = "first try失败后，无法自动登录";
            e.Failed = true;
            return;
        }

        public string SessionID
        {
            get
            {
                return _sessionID;
            }
        }

        // 接收请求包
        public int RecvTcpPackage(out byte[] baPackage,
            out int nLen,
            out string strError)
        {
            strError = "";

            int nInLen;
            int wRet = 0;
            bool bInitialLen = false;

            Debug.Assert(_client != null, "client为空");

            baPackage = new byte[4096];
            nInLen = 0;
            nLen = 4096; //COMM_BUFF_LEN;

            // long lIdleCount = 0;

            while (nInLen < nLen)
            {
                if (_client == null)
                {
                    strError = "通讯中断";
                    goto ERROR1;
                }

                try
                {
                    wRet = _client.GetStream().Read(baPackage,
                        nInLen,
                        baPackage.Length - nInLen);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10035)
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = "recv出错: " + ex.Message;
                    goto ERROR1;
                }

                if (wRet == 0)
                {
                    strError = "Closed by remote peer";
                    goto ERROR1;
                }

                // 得到包的长度

                if ((wRet >= 1 || nInLen >= 1)
                    && bInitialLen == false)
                {
                    long remainder = 0;
                    bool bRet = BerNode.IsCompleteBER(baPackage,
                        0,
                        nInLen + wRet,
                        out remainder);
                    if (bRet == true)
                    {
                        /*
                        // 正式分配缓冲区尺寸
                        byte[] temp = new byte[nLen];
                        Array.Copy(baPackage, 0, temp, 0, nInLen + wRet);
                        baPackage = temp;

                        bInitialLen = true;
                         * */
                        nLen = nInLen + wRet;
                        break;
                    }
                }

                nInLen += wRet;
                if (nInLen >= baPackage.Length
                    && bInitialLen == false)
                {
                    // 扩大缓冲区
                    byte[] temp = new byte[baPackage.Length + 4096];
                    Array.Copy(baPackage, 0, temp, 0, nInLen);
                    baPackage = temp;
                    nLen = baPackage.Length;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (baPackage.Length > nLen)
            {
                byte[] temp = new byte[nLen];
                Array.Copy(baPackage, 0, temp, 0, nLen);
                baPackage = temp;
            }

            return 0;
        ERROR1:
            this.CloseSocket();
            baPackage = null;
            return -1;
        }

        // 发出响应包
        // return:
        //      -1  出错
        //      0   正确发出
        //      1   发出前，发现流中有未读入的数据
        public int SendTcpPackage(byte[] baPackage,
            int nLen,
            out string strError)
        {
            strError = "";

            if (_client == null)
            {
                strError = "client尚未初始化";
                return -1;
            }

            // DoIdle();

            if (this._client == null)
            {
                strError = "用户中断";
                return -1;
            }

            try
            {

                NetworkStream stream = _client.GetStream();

                if (stream.DataAvailable == true)
                {
                    // Debug.Assert(false, "发送前居然发现有未读的数据" );
                    strError = "发送前发现流中有未读的数据";
                    return 1;
                }

                try
                {
                    stream.Write(baPackage, 0, nLen);
                }
                catch (Exception ex)
                {
                    strError = "send出错: " + ex.Message;
                    this.CloseSocket();
                    return -1;
                }

                // stream.Flush();

                return 0;
            }
            finally
            {

            }
        }

        public void CloseSocket()
        {
            if (_client != null)
            {

                try
                {
                    NetworkStream stream = _client.GetStream();
                    stream.Close();
                }
                catch { }
                try
                {
                    _client.Close();
                }
                catch { }

                _client = null;
            }

            // this.m_bInitialized = false;
        }

        /// <summary>
        /// Session处理轮回
        /// </summary>
        public void Processing()
        {
            int nRet = 0;
            string strError = "";

            try
            {
                byte[] baPackage = null;
                int nLen = 0;
                byte[] baResponsePackage = null;

                for (; ; )
                {
                    _activateTime = DateTime.Now;
                    // 接收前端请求
                    nRet = RecvTcpPackage(out baPackage,
                        out nLen,
                        out strError);
                    if (nRet == -1)
                        goto ERROR_NOT_LOG;

                    // 分析请求包
                    BerTree tree1 = new BerTree();
                    int nTotlen = 0;

                    tree1.m_RootNode.BuildPartTree(baPackage,
                        0,
                        baPackage.Length,
                        out nTotlen);

                    BerNode root = tree1.GetAPDuRoot();

                    switch (root.m_uTag)
                    {
                        case BerTree.z3950_initRequest:
                            {
                                InitRequestInfo info = null;
                                string strDebugInfo = "";
                                nRet = Decode_InitRequest(
                                    root,
                                    out info,
                                    out strDebugInfo,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                                // 可以用groupid来表示字符集信息

                                InitResponseInfo response_info = new InitResponseInfo();

                                // 判断info中的信息，决定是否接受Init请求。

                                if (String.IsNullOrEmpty(info.m_strID) == true)
                                {
                                    // 如果定义了允许匿名登录
                                    if (String.IsNullOrEmpty(this._service.AnonymousUserName) == false)
                                    {
                                        info.m_strID = this._service.AnonymousUserName;
                                        info.m_strPassword = this._service.AnonymousPassword;
                                    }
                                    else
                                    {
                                        response_info.m_nResult = 0;
                                        this._bInitialized = false;

                                        SetInitResponseUserInfo(response_info,
                                            "", // string strOID,
                                            0,  // long lErrorCode,
                                            "不允许匿名登录");
                                        goto DO_RESPONSE;
                                    }
                                }

                                // 进行登录
                                // return:
                                //      -1  error
                                //      0   登录未成功
                                //      1   登录成功
                                nRet = DoLogin(info.m_strGroupID,
                                    info.m_strID,
                                    info.m_strPassword,
                                    out strError);
                                if (nRet == -1 || nRet == 0)
                                {
                                    response_info.m_nResult = 0;
                                    this._bInitialized = false;

                                    SetInitResponseUserInfo(response_info,
                                        "", // string strOID,
                                        0,  // long lErrorCode,
                                        strError);
                                }
                                else
                                {
                                    response_info.m_nResult = 1;
                                    this._bInitialized = true;
                                }

                            DO_RESPONSE:
                                // 填充response_info的其它结构
                                response_info.m_strReferenceId = info.m_strReferenceId;  // .m_strID; BUG!!! 2007/11/2

                                if (info.m_lPreferredMessageSize != 0)
                                    this._lPreferredMessageSize = info.m_lPreferredMessageSize;
                                // 极限
                                if (this._lPreferredMessageSize > MaxPreferredMessageSize)
                                    this._lPreferredMessageSize = MaxPreferredMessageSize;
                                response_info.m_lPreferredMessageSize = this._lPreferredMessageSize;

                                if (info.m_lExceptionalRecordSize != 0)
                                    this._lExceptionalRecordSize = info.m_lExceptionalRecordSize;
                                // 极限
                                if (this._lExceptionalRecordSize > MaxExceptionalRecordSize)
                                    this._lExceptionalRecordSize = MaxExceptionalRecordSize;
                                response_info.m_lExceptionalRecordSize = this._lExceptionalRecordSize;

                                response_info.m_strImplementationId = "Digital Platform";
                                response_info.m_strImplementationName = "dp2ZServer";
                                response_info.m_strImplementationVersion = "1.0";

                                if (info.m_charNego != null)
                                {
                                    /* option
* 
search                 (0), 
present                (1), 
delSet                 (2),
resourceReport         (3),
triggerResourceCtrl    (4),
resourceCtrl           (5), 
accessCtrl             (6),
scan                   (7),
sort                   (8), 
--                     (9) (reserved)
extendedServices       (10),
level-1Segmentation    (11),
level-2Segmentation    (12),
concurrentOperations   (13),
namedResultSets        (14)
15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
19 Query type 104 
* }
*/
                                    response_info.m_strOptions = "yynnnnnnnnnnnnn";

                                    if (info.m_charNego.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
                                    {
                                        BerTree.SetBit(ref response_info.m_strOptions,
                                            17,
                                            true);
                                        response_info.m_charNego = info.m_charNego;
                                        this._searchTermEncoding = Encoding.UTF8;
                                        if (info.m_charNego.RecordsInSelectedCharsets != -1)
                                        {
                                            response_info.m_charNego.RecordsInSelectedCharsets = info.m_charNego.RecordsInSelectedCharsets; // 依从前端的请求
                                            if (response_info.m_charNego.RecordsInSelectedCharsets == 1)
                                                this._marcRecordEncoding = Encoding.UTF8;
                                        }
                                    }
                                }
                                else
                                {
                                    response_info.m_strOptions = "yynnnnnnnnnnnnn";
                                }



                                BerTree tree = new BerTree();
                                nRet = Encode_InitialResponse(response_info,
                                    out baResponsePackage);
                                if (nRet == -1)
                                    goto ERROR1;


                            }
                            break;
                        case BerTree.z3950_searchRequest:
                            {
                                SearchRequestInfo info = null;
                                // 解码Search请求包
                                nRet = Decode_SearchRequest(
                                    root,
                                    out info,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                                if (_bInitialized == false)
                                    goto ERROR_NOT_LOG;


                                // 编码Search响应包
                                nRet = Encode_SearchResponse(info,
                                    out baResponsePackage,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                            }
                            break;

                        case BerTree.z3950_presentRequest:
                            {
                                PresentRequestInfo info = null;
                                // 解码Search请求包
                                nRet = Decode_PresentRequest(
                                    root,
                                    out info,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                                if (_bInitialized == false)
                                    goto ERROR_NOT_LOG;

                                // 编码Present响应包
                                nRet = Encode_PresentResponse(info,
                                    out baResponsePackage);
                                if (nRet == -1)
                                    goto ERROR1;

                            }
                            break;
                        default:
                            break;
                    }


                    // 发出响应包
                    // return:
                    //      -1  出错
                    //      0   正确发出
                    //      1   发出前，发现流中有未读入的数据
                    nRet = SendTcpPackage(baResponsePackage,
                        baResponsePackage.Length,
                        out strError);
                    if (nRet == -1)
                        goto ERROR_NOT_LOG;
                }

            }
            catch (ThreadInterruptedException)
            {
                // string dummy = e.Message;     // Needed for to remove compile warning
            }
            catch (Exception x)
            {
                /*
				if(m_clientSocket.Connected)
				{
					// SendData("421 Service not available, closing transmission channel\r\n");

					// m_pSMTP_Server.WriteErrorLog(x.Message);
				}
				else
				{

				}
                 * */
                strError = "Session Processing()俘获异常: " + ExceptionUtil.GetDebugText(x);
                goto ERROR1;
            }
            finally
            {
                _service.RemoveSession(this.SessionID);
                this.CloseSocket();
            }
            return;
        ERROR1:
            // 将strError写入日志
            this._service.Log.WriteEntry(strError, EventLogEntryType.Error);
            return;
        ERROR_NOT_LOG:
            // 不写入日志
            return;
        }

        void SetInitResponseUserInfo(InitResponseInfo response_info,
            string strOID,
            long lErrorCode,
            string strErrorMessage)
        {
            if (response_info.UserInfoField == null)
                response_info.UserInfoField = new External();

            response_info.UserInfoField.m_strDirectRefenerce = strOID;
            response_info.UserInfoField.m_lIndirectReference = lErrorCode;
            if (String.IsNullOrEmpty(strErrorMessage) == false)
            {
                response_info.UserInfoField.m_octectAligned = Encoding.UTF8.GetBytes(strErrorMessage);
            }
        }

        // 进行登录
        // return:
        //      -1  error
        //      0   登录未成功
        //      1   登录成功
        int DoLogin(string strGroupId,
            string strUserName,
            string strPassword,
            out string strError)
        {
            strError = "";

            // return:
            //      -1  error
            //      0   登录未成功
            //      1   登录成功
            long lRet = this._channel.Login(strUserName,
                strPassword,
                "location=z39.50 server,type=worker,client=dp2ZServer|0.01",
                /*
                "z39.50 server",    // string strLocation,
                false,  // bReader,
                 * */
                out strError);
            if (lRet == -1)
                return -1;

            // 记忆下来，供以后使用
            this._groupId = strGroupId;
            this._userName = strUserName;
            this._password = strPassword;

            return (int)lRet;
        }

        #region BER包处理

        // 解码Initial请求包
        public static int Decode_InitRequest(
            BerNode root,
            out InitRequestInfo info,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";
            info = new InitRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_initRequest)
            {
                strError = "root tag is not z3950_initRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId:
                        info.m_strReferenceId = node.GetCharNodeData();
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ProtocolVersion:
                        info.m_strProtocolVersion = node.GetBitstringNodeData();
                        strDebugInfo += "ProtocolVersion='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_Options:
                        info.m_strOptions = node.GetBitstringNodeData();
                        strDebugInfo += "Options='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_PreferredMessageSize:
                        info.m_lPreferredMessageSize = node.GetIntegerNodeData();
                        strDebugInfo += "PreferredMessageSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ExceptionalRecordSize:
                        info.m_lExceptionalRecordSize = node.GetIntegerNodeData();
                        strDebugInfo += "ExceptionalRecordSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_idAuthentication:
                        {
                            string strGroupId = "";
                            string strUserId = "";
                            string strPassword = "";
                            int nAuthentType = 0;

                            int nRet = DecodeAuthentication(
                                node,
                                out strGroupId,
                                out strUserId,
                                out strPassword,
                                out nAuthentType,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_nAuthenticationMethod = nAuthentType;	// 0: open 1:idPass
                            info.m_strGroupID = strGroupId;
                            info.m_strID = strUserId;
                            info.m_strPassword = strPassword;

                            strDebugInfo += "idAuthentication struct occur\r\n";
                        }
                        break;
                    case BerTree.z3950_ImplementationId:
                        info.m_strImplementationId = node.GetCharNodeData();
                        strDebugInfo += "ImplementationId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ImplementationName:
                        info.m_strImplementationName = node.GetCharNodeData();
                        strDebugInfo += "ImplementationName='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_ImplementationVersion:
                        info.m_strImplementationVersion = node.GetCharNodeData();
                        strDebugInfo += "ImplementationVersion='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case BerTree.z3950_OtherInformationField:
                        info.m_charNego = new CharsetNeogatiation();
                        info.m_charNego.DecodeProposal(node);
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }

        // 解码Search请求包
        public static int Decode_SearchRequest(
            BerNode root,
            out SearchRequestInfo info,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            info = new SearchRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_searchRequest)
            {
                strError = "root tag is not z3950_searchRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId: // 2
                        info.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_smallSetUpperBound: // 13 smallSetUpperBound (Integer)
                        info.m_lSmallSetUpperBound = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_largeSetLowerBound: // 14 largeSetLowerBound  (Integer)         
                        info.m_lLargeSetLowerBound = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_mediumSetPresentNumber: // 15 mediumSetPresentNumber (Integer)      
                        info.m_lMediumSetPresentNumber = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_replaceIndicator: // 16 replaceIndicator, (boolean)
                        info.m_lReplaceIndicator = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_resultSetName: // 17 resultSetName (string)
                        info.m_strResultSetName = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_databaseNames: // 18 dbNames (sequence)
                        /*
                        // sequence is constructed, // have child with case = 105, (string)
                        m_saDBName.RemoveAll();
                        DecodeDBName(pNode, m_saDBName, m_bIsCharSetUTF8);
                         * */
                        {
                            List<string> dbnames = null;
                            nRet = DecodeDbnames(node,
                                out dbnames,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_dbnames = dbnames;
                        }
                        break;
                    case BerTree.z3950_query: // 21 query (query)
                        //			DecodeSearchQuery(pNode, m_strSQLWhere, pRPNStructureRoot);
                        {
                            BerNode rpn_root = GetRPNStructureRoot(node,
                                out strError);
                            if (rpn_root == null)
                                return -1;

                            info.m_rpnRoot = rpn_root;
                        }
                        break;
                    default:
                        break;
                }

            }

            return 0;
        }

        // 编码(构造) Search响应包
        int Encode_SearchResponse(SearchRequestInfo info,
            out byte[] baPackage,
            out string strError)
        {
            baPackage = null;
            int nRet = 0;
            long lRet = 0;
            strError = "";

            DiagFormat diag = null;

            BerTree tree = new BerTree();
            BerNode root = null;

            long lSearchStatus = 0; // 0 失败；1成功
            long lHitCount = 0;

            string strQueryXml = "";
            // 根据逆波兰表进行检索

            // return:
            //      -1  error
            //      0   succeed
            nRet = BuildQueryXml(
                info.m_dbnames,
                info.m_rpnRoot,
                out strQueryXml,
                out strError);
            if (nRet == -1)
            {
                SetPresentDiagRecord(ref diag,
                    2,  // temporary system error
                    strError);
            }

            string strResultSetName = info.m_strResultSetName;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";

            if (diag == null)
            {
                lRet = _channel.Search(null,
                    strQueryXml,
                    strResultSetName,
                    "", // strOutputStyle
                    out strError);

                /*
                // 测试检索失败
                lRet = -1;
                strError = "测试检索失败";
                 * */

                if (lRet == -1)
                {
                    lSearchStatus = 0;  // failed

                    SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                }
                else
                {
                    lHitCount = lRet;
                    lSearchStatus = 1;  // succeed
                }
            }


            root = tree.m_RootNode.NewChildConstructedNode(
                BerTree.z3950_searchResponse,
                BerNode.ASN1_CONTEXT);

            // reference id
            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }


            // resultCount
            root.NewChildIntegerNode(BerTree.z3950_resultCount, // 23
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)lHitCount));

            // numberOfRecordsReturned
            root.NewChildIntegerNode(BerTree.z3950_NumberOfRecordsReturned, // 24
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)0/*info.m_lNumberOfRecordReturned*/));    // 0

            // nextResultSetPosition
            root.NewChildIntegerNode(BerTree.z3950_NextResultSetPosition, // 25
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)1/*info.m_lNextResultSetPosition*/));

            // 2007/11/7 原来本项位置不对，现在移动到这里
            // bool
            // searchStatus
            root.NewChildIntegerNode(BerTree.z3950_searchStatus, // 22
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)lSearchStatus));

            // resultSetStatus OPTIONAL

            // 2007/11/7
            // presentStatus
            root.NewChildIntegerNode(BerTree.z3950_presentStatus, // 27
                BerNode.ASN1_CONTEXT,   // ASNI_PRIMITIVE BUG!!!!
                BitConverter.GetBytes((long)0));


            // 诊断记录
            if (diag != null)
            {
                BerNode nodeDiagRoot = root.NewChildConstructedNode(BerTree.z3950_nonSurrogateDiagnostic,    // 130
                    BerNode.ASN1_CONTEXT);

                diag.BuildBer(nodeDiagRoot);
            }

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        // 解码Present请求包
        public static int Decode_PresentRequest(
            BerNode root,
            out PresentRequestInfo info,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            info = new PresentRequestInfo();

            Debug.Assert(root != null, "");

            if (root.m_uTag != BerTree.z3950_presentRequest)
            {
                strError = "root tag is not z3950_presentRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case BerTree.z3950_ReferenceId: // 2
                        info.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_ResultSetId: // 31 resultSetId (IntenationalString)
                        info.m_strResultSetID = node.GetCharNodeData();
                        break;
                    case BerTree.z3950_resultSetStartPoint: // 30 resultSetStartPoint  (Integer)         
                        info.m_lResultSetStartPoint = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_numberOfRecordsRequested: // 29 numberOfRecordsRequested (Integer)      
                        info.m_lNumberOfRecordsRequested = node.GetIntegerNodeData();
                        break;
                    case BerTree.z3950_ElementSetNames: // 19 ElementSetNames (complicates)
                        {
                            List<string> elementset_names = null;
                            nRet = DecodeElementSetNames(node,
                                out elementset_names,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            info.m_elementSetNames = elementset_names;
                        }
                        break;
                    default:
                        break;
                }
            }

            return 0;
        }

        // 设置present response中的诊断记录
        static void SetPresentDiagRecord(ref DiagFormat diag,
            int nCondition,
            string strAddInfo)
        {
            if (diag == null)
            {
                diag = new DiagFormat();
                diag.m_strDiagSetID = "1.2.840.10003.4.1";
            }

            diag.m_nDiagCondition = nCondition;
            diag.m_strAddInfo = strAddInfo;
        }

        // 编码(构造) Present响应包
        int Encode_PresentResponse(PresentRequestInfo info,
            out byte[] baPackage)
        {
            baPackage = null;
            int nRet = 0;
            string strError = "";

            DiagFormat diag = null;

            BerTree tree = new BerTree();
            BerNode root = null;

            string strResultSetName = info.m_strResultSetID;
            if (String.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            long lStart = info.m_lResultSetStartPoint - 1;
            long lNumber = info.m_lNumberOfRecordsRequested;

            long lPerCount = lNumber;

            long lHitCount = 0;

            List<string> paths = new List<string>();

            int nPresentStatus = 5; // failed

            // 获取结果集中需要部分的记录path
            long lOffset = lStart;
            int nCount = 0;
            for (; ; )
            {
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                long lRet = this._channel.GetSearchResult(
                    null,   // stop,
                    strResultSetName,   // strResultSetName
                    lOffset,
                    lPerCount,
                    "id",
                    "zh",   // this.Lang,
                    out searchresults,
                    out strError);
                /*
                // 测试获取结果集失败的情况，返回非代理诊断记录
                lRet = -1;
                strError = "测试检索错误信息！";
                 * */

                if (lRet == -1)
                {
                    SetPresentDiagRecord(ref diag,
                        2,  // temporary system error
                        strError);
                    break;
                }
                if (lRet == 0)
                {
                    // goto ERROR1 ?
                }

                lHitCount = lRet;   // 顺便得到命中记录总条数

                // 转储
                for (int i = 0; i < searchresults.Length; i++)
                {
                    paths.Add(searchresults[i].Path);
                }

                lOffset += searchresults.Length;
                lPerCount -= searchresults.Length;
                nCount += searchresults.Length;

                if (lOffset >= lHitCount
                    || lPerCount <= 0
                    || nCount >= lNumber)
                {
                    // 
                    break;
                }
            }

            // TODO: 需要注意多个错误是否形成多个diag记录？V2不允许这样，V3允许这样
            if (lHitCount < info.m_lResultSetStartPoint
                && diag == null)
            {
                strError = "start参数值 "
                    + info.m_lResultSetStartPoint
                    + " 超过结果集中记录总数 "
                    + lHitCount;
                // return -1;  // 如果表示错误状态？
                SetPresentDiagRecord(ref diag,
                    13,  // Present request out-of-range
                    strError);
            }

            int MAX_PRESENT_RECORD = 100;

            // 限制每次 present 的记录数量
            if (lNumber > MAX_PRESENT_RECORD)
                lNumber = MAX_PRESENT_RECORD;

            long nNextResultSetPosition = 0;

            // 
            if (lHitCount < (lStart - 1) + lNumber)
            {
                // 是 present 错误，但还可以调整 lNumber
                lNumber = lHitCount - (lStart - 1);
                nNextResultSetPosition = 0;
            }
            else
            {
                //
                nNextResultSetPosition = lStart + lNumber + 1;
            }

            root = tree.m_RootNode.NewChildConstructedNode(
                BerTree.z3950_presentResponse,
                BerNode.ASN1_CONTEXT);

            // reference id
            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }

            List<RetrivalRecord> records = new List<RetrivalRecord>();

            // 获取要返回的MARC记录
            if (diag == null)
            {

                // 记录编码格式为 GRS-1 (generic-record-syntax-1) :
                //		EXTERNAL 
                //			--- OID (Object Identifier)
                //			--- MARC (OCTET STRING)
                //	m_strOID = _T("1.2.840.10003.5.1");  // OID of UNIMARC
                //	m_strOID = _T("1.2.840.10003.5.10"); // OID of USMARC //
                // 需要建立一个数据库名和oid的对照表，方面快速取得数据库MARC syntax OID

                // TODO: 编码过程中，可能会发现记录太多，总尺寸超过Initial中规定的prefered message size。
                // 这样需要减少返回的记录数量。这样，就需要先做这里的循环，后构造另外几个参数
                int nSize = 0;
                for (int i = 0; i < (int)lNumber; i++)
                {
                    // 编码 N 条 MARC 记录
                    //
                    // if (m_bStop) return false;

                    // 取出数据库指针
                    // lStart 不是 0 起点的
                    string strPath = paths[i];

                    // 解析出数据库名和ID
                    string strDbName = Global.GetDbName(strPath);
                    string strRecID = Global.GetRecordID(strPath);

                    // 如果取得的是xml记录，则根元素可以看出记录的marc syntax，进一步可以获得oid；
                    // 如果取得的是MARC格式记录，则需要根据数据库预定义的marc syntax来看出oid了
                    string strMarcSyntaxOID = GetMarcSyntaxOID(strDbName);

                    byte[] baMARC = null;

                    RetrivalRecord record = new RetrivalRecord();
                    record.m_strDatabaseName = strDbName;

                    // 根据书目库名获得书目库属性对象
                    BiblioDbProperty prop = this._service.GetDbProperty(
                        strDbName,
                        false);

                    nRet = GetMARC(strPath,
                        info.m_elementSetNames,
                        prop != null ? prop.AddField901 : false,
                        out baMARC,
                        out strError);

                    /*
                    // 测试记录群中包含诊断记录
                    if (i == 1)
                    {
                        nRet = -1;
                        strError = "测试获取记录错误";
                    }*/
                    if (nRet == -1)
                    {
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 14;  // system error in presenting records
                        record.m_surrogateDiagnostic.m_strAddInfo = strError;
                    }
                    else if (nRet == 0)
                    {
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 1028;  // record deleted
                        record.m_surrogateDiagnostic.m_strAddInfo = strError;
                    }
                    else if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                    {
                        // 根据数据库名无法获得marc syntax oid。可能是虚拟库检索命中记录所在的物理库没有在dp2zserver.xml中配置。
                        record.m_surrogateDiagnostic = new DiagFormat();
                        record.m_surrogateDiagnostic.m_strDiagSetID = "1.2.840.10003.4.1";
                        record.m_surrogateDiagnostic.m_nDiagCondition = 109;  // database unavailable // 似乎235:database dos not exist也可以
                        record.m_surrogateDiagnostic.m_strAddInfo = "根据数据库名 '" + strDbName + "' 无法获得marc syntax oid";
                    }
                    else
                    {
                        record.m_external = new External();
                        record.m_external.m_strDirectRefenerce = strMarcSyntaxOID;
                        record.m_external.m_octectAligned = baMARC;
                    }

                    nSize += record.GetPackageSize();

                    if (i == 0)
                    {
                        // 连一条记录也放不下
                        if (nSize > this._lExceptionalRecordSize)
                        {
                            Debug.Assert(diag == null, "");
                            SetPresentDiagRecord(ref diag,
                                17, // record exceeds Exceptional_record_size
                                "记录尺寸 " + nSize.ToString() + " 超过 Exceptional_record_size " + this._lExceptionalRecordSize.ToString());
                            lNumber = 0;
                            break;
                        }
                    }
                    else
                    {
                        if (nSize >= this._lPreferredMessageSize)
                        {
                            // 调整返回的记录数
                            lNumber = i;
                            break;
                        }
                    }

                    records.Add(record);
                }
            }


            // numberOfRecordsReturned
            root.NewChildIntegerNode(BerTree.z3950_NumberOfRecordsReturned, // 24
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
                BitConverter.GetBytes((long)lNumber));

            if (diag != null)
                nPresentStatus = 5;
            else
                nPresentStatus = 0;

            // nextResultSetPosition
            // if 0, that's end of the result set
            // else M+1, M is 最后一次 present response 的最后一条记录在 result set 中的 position
            root.NewChildIntegerNode(BerTree.z3950_NextResultSetPosition, // 25
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
                BitConverter.GetBytes((long)nNextResultSetPosition));

            // presentStatus
            // success      (0),
            // partial-1    (1),
            // partial-2    (2),
            // partial-3    (3),
            // partial-4    (4),
            // failure      (5).
            root.NewChildIntegerNode(BerTree.z3950_presentStatus, // 27
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE BUG!!!
               BitConverter.GetBytes((long)nPresentStatus));


            // 诊断记录
            if (diag != null)
            {
                BerNode nodeDiagRoot = root.NewChildConstructedNode(BerTree.z3950_nonSurrogateDiagnostic,    // 130
                    BerNode.ASN1_CONTEXT);

                diag.BuildBer(nodeDiagRoot);

                /*
                nodeDiagRoot.NewChildOIDsNode(6,
                    BerNode.ASN1_UNIVERSAL,
                    diag.m_strDiagSetID);   // "1.2.840.10003.4.1"

                nodeDiagRoot.NewChildIntegerNode(2,
                    BerNode.ASN1_UNIVERSAL,
                    BitConverter.GetBytes((long)diag.m_nDiagCondition));

                if (String.IsNullOrEmpty(diag.m_strAddInfo) == false)
                {
                    nodeDiagRoot.NewChildCharNode(26,
                        BerNode.ASN1_UNIVERSAL,
                        Encoding.UTF8.GetBytes(diag.m_strAddInfo));
                }
                 * */
            }


            // 如果 present 是非法的，到这里打包完成，可以返回了
            if (0 != nPresentStatus)
                goto END1;

            // 编码记录BER树

            // 以下为 present 成功时，打包返回记录。
            // present success
            // presRoot records child, constructed (choice of ... ... optional)
            // if present fail, then may be no records 'node'
            // Records ::= CHOICE {
            //		responseRecords              [28]   IMPLICIT SEQUENCE OF NamePlusRecord,
            //		nonSurrogateDiagnostic       [130]  IMPLICIT DefaultDiagFormat,
            //		multipleNonSurDiagnostics    [205]  IMPLICIT SEQUENCE OF DiagRec} 

            // 当 present 成功时，response 选择了 NamePlusRecord (数据库名 +　记录)
            BerNode node = root.NewChildConstructedNode(BerTree.z3950_dataBaseOrSurDiagnostics,    // 28
                            BerNode.ASN1_CONTEXT);

            for (int i = 0; i < records.Count; i++)
            {
                RetrivalRecord record = records[i];

                record.BuildNamePlusRecord(node);
            }

        END1:

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        string GetMarcSyntaxOID(string strBiblioDbName)
        {
            string strSyntax = this._service.GetMarcSyntax(strBiblioDbName);
            if (strSyntax == null)
                return null;
            if (strSyntax == "unimarc")
                return "1.2.840.10003.5.1";
            if (strSyntax == "usmarc")
                return "1.2.840.10003.5.10";

            return null;
        }

        // 获得MARC记录
        // parameters:
        //      bAddField901    是否加入901字段？
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetMARC(string strPath,
            List<string> elementSetNames,
            bool bAddField901,
            out byte[] baMARC,
            out string strError)
        {
            baMARC = null;
            strError = "";

            string strXml = "";
            byte[] timestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetMarcXml(strPath,
                out strXml,
                out timestamp,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            //
            string strMarcSyntax = "";
            string strOutMarcSyntax = "";
            string strMarc = "";

            // 转换为机内格式
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                strMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            if (bAddField901 == true)
            {
                // 901  $p记录路径$t时间戳
                string strField = "901  "
                    + new string(MarcUtil.SUBFLD, 1) + "p" + strPath
                    + new string(MarcUtil.SUBFLD, 1) + "t" + ByteArray.GetHexTimeStampString(timestamp);

                // 替换记录中的字段内容。
                // 先在记录中找同名字段(第nIndex个)，如果找到，则替换；如果没有找到，
                // 则在顺序位置插入一个新字段。
                // parameters:
                //		strMARC		[in][out]MARC记录。
                //		strFieldName	要替换的字段的名。如果为null或者""，则表示所有字段中序号为nIndex中的那个被替换
                //		nIndex		要替换的字段的所在序号。如果为-1，将始终为在记录中追加新字段内容。
                //		strField	要替换成的新字段内容。包括字段名、必要的字段指示符、字段内容。这意味着，不但可以替换一个字段的内容，也可以替换它的字段名和指示符部分。
                // return:
                //		-1	出错
                //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
                //		1	找到了指定的字段，并且也成功用strField替换掉了。
                nRet = MarcUtil.ReplaceField(
                    ref strMarc,
                    "901",
                    0,
                    strField);
                if (nRet == -1)
                    return -1;
            }

            // 转换为ISO2709
            nRet = MarcUtil.CvtJineiToISO2709(
                strMarc,
                strOutMarcSyntax,
                this._marcRecordEncoding,
                out baMARC,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 获得MARC XML记录
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetMarcXml(string strBiblioRecPath,
            out string strXml,
            out byte[] timestamp,
            out string strError)
        {
            strXml = "";
            strError = "";
            timestamp = null;

            try
            {
                string[] formats = new string[1];
                formats[0] = "xml";

                string[] results = null;

                long lRet = _channel.GetBiblioInfos(
                    null,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;   // not found

                strXml = results[0];
            }
            finally
            {
            }

            return 1;
            /*
        ERROR1:
            return -1;
             * */
        }


        // 解析出search请求中的 数据库名列表
        static int DecodeElementSetNames(BerNode root,
            out List<string> elementset_names,
            out string strError)
        {
            elementset_names = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                /*
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
                 * */
                // TODO: 这里需要看一下PDU定义，看看是否需要判断m_uTag
                elementset_names.Add(node.GetCharNodeData());
            }

            return 0;
        }


        // 获得search请求中的RPN根节点
        static BerNode GetRPNStructureRoot(BerNode root,
            out string strError)
        {
            strError = "";

            if (root == null)
            {
                strError = "query root is null";
                return null;
            }

            if (root.ChildrenCollection.Count < 1)
            {
                strError = "no query item";
                return null;
            }

            BerNode RPNRoot = root.ChildrenCollection[0];
            if (1 != RPNRoot.m_uTag) // type-1 query
            {
                strError = "not type-1 query. unsupported query type";
                return null;
            }

            string strAttributeSetId = ""; //attributeSetId OBJECT IDENTIFIER
            // string strQuery = "";


            for (int i = 0; i < RPNRoot.ChildrenCollection.Count; i++)
            {
                BerNode node = RPNRoot.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case 6: // attributeSetId (OBJECT IDENTIFIER)
                        strAttributeSetId = node.GetOIDsNodeData();
                        if (strAttributeSetId != "1.2.840.10003.3.1") // bib-1
                        {
                            strError = "support bib-1 only";
                            return null;
                        }
                        break;
                    // RPNStructure (CHOICE 0, 1)
                    case 0:
                    case 1:
                        return node; // this is RPN Stucture root
                }
            }

            strError = "not found";
            return null;
        }

        // 解析出search请求中的 数据库名列表
        static int DecodeDbnames(BerNode root,
            out List<string> dbnames,
            out string strError)
        {
            dbnames = new List<string>();
            strError = "";

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                if (node.m_uTag == 105)
                {
                    dbnames.Add(node.GetCharNodeData());
                }
            }

            return 0;
        }


        // 解析出init请求中的 鉴别信息
        // parameters:
        //      nAuthentType 0: open(simple) 1:idPass(group)
        static int DecodeAuthentication(
            BerNode root,
            out string strGroupId,
            out string strUserId,
            out string strPassword,
            out int nAuthentType,
            out string strError)
        {
            strGroupId = "";
            strUserId = "";
            strPassword = "";
            nAuthentType = 0;
            strError = "";

            if (root == null)
            {
                strError = "root == null";
                return -1;
            }

            string strOpen = ""; // open mode authentication


            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case BerNode.ASN1_SEQUENCE:

                        nAuthentType = 1;   //  "GROUP";
                        for (int k = 0; k < node.ChildrenCollection.Count; k++)
                        {
                            BerNode nodek = node.ChildrenCollection[k];
                            switch (nodek.m_uTag)
                            {
                                case 0: // groupId
                                    strGroupId = nodek.GetCharNodeData();
                                    break;
                                case 1: // userId
                                    strUserId = nodek.GetCharNodeData();
                                    break;
                                case 2: // password
                                    strPassword = nodek.GetCharNodeData();
                                    break;
                            }
                        }

                        break;
                    case BerNode.ASN1_VISIBLESTRING:
                    case BerNode.ASN1_GENERALSTRING:
                        nAuthentType = 0; //  "SIMPLE";
                        strOpen = node.GetCharNodeData();
                        break;
                }
            }

            if (nAuthentType == 0)
            {
                int nRet = strOpen.IndexOf("/");
                if (nRet != -1)
                {
                    strUserId = strOpen.Substring(0, nRet);
                    strPassword = strOpen.Substring(nRet + 1);
                }
                else
                {
                    strUserId = strOpen;
                }
            }

            return 0;
        }

        // 解码RPN结构中的Attribute + Term结构
        static int DecodeAttributeAndTerm(
            Encoding term_encoding,
            BerNode pNode,
            out long lAttributeType,
            out long lAttributeValue,
            out string strTerm,
            out string strError)
        {
            lAttributeType = 0;
            lAttributeValue = 0;
            strTerm = "";
            strError = "";

            if (pNode == null)
            {
                strError = "node == null";
                return -1;
            }

            if (pNode.ChildrenCollection.Count < 2) //attriblist + term
            {
                strError = "bad RPN query";
                return -1;
            }

            BerNode pAttrib = pNode.ChildrenCollection[0]; // attriblist
            BerNode pTerm = pNode.ChildrenCollection[1]; // term

            if (44 != pAttrib.m_uTag) // Attributes
            {
                strError = "only support Attributes";
                return -1;
            }

            if (45 != pTerm.m_uTag) // Term
            {
                strError = "only support general Term";
                return -1;
            }

            // get attribute type and value
            if (pAttrib.ChildrenCollection.Count < 1) //attribelement
            {
                strError = "bad RPN query";
                return -1;
            }

            pAttrib = pAttrib.ChildrenCollection[0];
            if (16 != pAttrib.m_uTag) //attribelement (SEQUENCE) 
            {
                strError = "only support Attributes";
                return -1;
            }

            for (int i = 0; i < pAttrib.ChildrenCollection.Count; i++)
            {
                BerNode pTemp = pAttrib.ChildrenCollection[i];
                switch (pTemp.m_uTag)
                {
                    case 120: // attributeType
                        lAttributeType = pTemp.GetIntegerNodeData();
                        break;
                    case 121: // attributeValue
                        lAttributeValue = pTemp.GetIntegerNodeData();
                        break;
                }
            }

            // get term
            strTerm = pTerm.GetCharNodeData(term_encoding);

            if (-1 == lAttributeType
                || -1 == lAttributeValue
                || String.IsNullOrEmpty(strTerm) == true)
            {
                strError = "bad RPN query";
                return -1;
            }

            return 0;
        }

        static int DecodeRPNOperator(BerNode pNode)
        {
            if (pNode == null)
                return -1;

            if (46 == pNode.m_uTag)
            {
                if (pNode.ChildrenCollection.Count > 0)
                {
                    return pNode.ChildrenCollection[0].m_uTag;
                }
            }

            return -1;
        }

        #endregion // BER包处理

        // 构造一个检索词的XML检索式局部
        // 本函数不递归
        int BuildOneXml(
            List<string> dbnames,
            string strTerm,
            long lAttritueValue,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            if (dbnames.Count == 0)
            {
                strError = "一个数据库名也未曾指定";
                return -1;
            }

            // string strFrom = "";    // 根据nAttributeType nAttributeValue得到检索途径名


            // 先评估一下，是不是每个数据库都有一样的maxResultCount参数。
            // 如果是，则可以把这些数据库都组合为一个<target>；
            // 如果不是，则把相同的挑选出来成为一个<target>，然后多个<target>用OR组合起来

            // 为此，可以先把数据库属性对象按照maxResultCount参数排序，以便聚合是用<target>。
            // 但是这带来一个问题：最后发生的检索库的先后顺序，就不是用户要求的那个顺序了。
            // 看来，还得按照用户指定的数据库顺序来构造<item>。那么，就不得不降低聚合的可能，
            // 而仅仅聚合相邻的、maxResultCount值相同的那些

            int nPrevMaxResultCount = -1;   // 前一个MaxResultCount参数值
            List<List<BiblioDbProperty>> prop_groups = new List<List<BiblioDbProperty>>();

            List<BiblioDbProperty> props = new List<BiblioDbProperty>();
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                BiblioDbProperty prop = this._service.GetDbProperty(strDbName,
                    true);
                if (prop == null)
                {
                    strError = "数据库 '" + strDbName + "' 不存在";
                    return -1;
                }

                // 如果当前库的MaxResultCount参数和前面紧邻的不一样了，则需要推入当前正在使用的props，新起一个props
                if (prop.MaxResultCount != nPrevMaxResultCount
                    && props.Count != 0)
                {
                    Debug.Assert(props.Count > 0, "不为空的props才能推入 (1)");
                    prop_groups.Add(props);
                    props = new List<BiblioDbProperty>();   // 新增加一个props
                }

                props.Add(prop);

                nPrevMaxResultCount = prop.MaxResultCount;
            }

            Debug.Assert(props.Count > 0, "不为空的props才能推入 (2)");
            prop_groups.Add(props); // 将最后一个props加入到group数组中


            for (int i = 0; i < prop_groups.Count; i++)
            {
                props = prop_groups[i];

                string strTargetListValue = "";
                int nMaxResultCount = -1;
                for (int j = 0; j < props.Count; j++)
                {
                    BiblioDbProperty prop = props[j];

                    string strDbName = prop.DbName;
#if DEBUG
                    if (j != 0)
                    {
                        Debug.Assert(prop.MaxResultCount == nMaxResultCount, "props内的每个数据库都应当有相同的MaxResultCount参数值");
                    }
#endif

                    if (j == 0)
                        nMaxResultCount = prop.MaxResultCount;  // 只取第一个prop的值即可

                    string strOutputDbName = "";
                    string strFrom = this._service.GetFromName(strDbName,
                        lAttritueValue,
                        out strOutputDbName,
                        out strError);
                    if (strFrom == null)
                        return -1;  // 寻找from名的过程发生错误

                    if (strTargetListValue != "")
                        strTargetListValue += ";";

                    Debug.Assert(strOutputDbName != "", "");

                    strTargetListValue += strOutputDbName + ":" + strFrom;
                }

                if (i != 0)
                    strQueryXml += "<operator value='OR' />";

                strQueryXml += "<target list='" + strTargetListValue + "'>"
                + "<item><word>"
                + StringUtil.GetXmlStringSimple(strTerm)
                + "</word><match>left</match><relation>=</relation><dataType>string</dataType>"
                + "<maxCount>" + nMaxResultCount.ToString() + "</maxCount></item>"
                + "<lang>zh</lang></target>";
            }

            // 如果有多个props，则需要在检索XML外面包裹一个<target>元素，以作为一个整体和其他部件进行逻辑操作
            if (prop_groups.Count > 1)
                strQueryXml = "<target>" + strQueryXml + "</target>";

            return 0;
        }

#if NOOOOOOOOOOOOOOOOOOO
        // 构造一个检索词的XML检索式局部
        int BuildOneXml(
            List<string> dbnames,
            string strTerm,
            long lAttritueValue,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            if (dbnames.Count == 0)
            {
                strError = "一个数据库名也未曾指定";
                return -1;
            }

            // string strFrom = "";    // 根据nAttributeType nAttributeValue得到检索途径名


            // 先评估一下，是不是每个数据库都有一样的maxResultCount参数。
            // 如果是，则可以把这些数据库都组合为一个<target>；
            // 如果不是，则把相同的挑选出来成为一个<target>，然后多个<target>用OR组合起来

            // 为此，可以先把数据库属性对象按照maxResultCount参数排序，以便聚合是用<target>。
            // 但是这带来一个问题：最后发生的检索库的先后顺序，就不是用户要求的那个顺序了。
            // 看来，还得按照用户指定的数据库顺序来构造<item>。那么，就不得不降低聚合的可能，
            // 而仅仅聚合相邻的、maxResultCount值相同的那些

            int nPrevMaxResultCount = -1;   // 前一个MaxResultCount参数值
            List<List<BiblioDbProperty>> prop_groups = new List<List<BiblioDbProperty>>();

            List<BiblioDbProperty> props = new List<BiblioDbProperty>();
            for (int i = 0; i < dbnames.Count; i++)
            {
                string strDbName = dbnames[i];

                BiblioDbProperty prop = this.m_service.GetDbProperty(strDbName);
                if (prop == null)
                {
                    strError = "数据库 '" + strDbName + "' 不存在";
                    return -1;
                }

                // 如果当前库的MaxResultCount参数和前面紧邻的不一样了，则需要推入当前正在使用的props，新起一个props
                if (prop.MaxResultCount != nPrevMaxResultCount
                    && props.Count != 0)
                {
                    Debug.Assert(props.Count > 0, "不为空的props才能推入 (1)");
                    prop_groups.Add(props);
                    props = new List<BiblioDbProperty>();   // 新增加一个props
                }

                props.Add(prop);
            }

            Debug.Assert(props.Count > 0, "不为空的props才能推入 (2)");
            prop_groups.Add(props); // 将最后一个props加入到group数组中


            for (int i = 0; i < prop_groups.Count; i++)
            {
                props = prop_groups[i];

                string strTargetListValue = "";
                int nMaxResultCount = -1;
                for (int j = 0; j < props.Count; j++)
                {
                    BiblioDbProperty prop = props[j];

                    string strDbName = prop.DbName;
                    /*
                    string strDbName = dbnames[j];

                    BiblioDbProperty prop = this.m_service.GetDbProperty(strDbName);
                    if (prop == null)
                    {
                        strError = "数据库 '" + strDbName + "' 不存在";
                        return -1;
                    }
                     * */

#if DEBUG
                    if (j != 0)
                    {
                        Debug.Assert(prop.MaxResultCount == nMaxResultCount, "props内的每个数据库都应当有相同的MaxResultCount参数值");
                    }
#endif

                    if (j==0)
                        nMaxResultCount = prop.MaxResultCount;  // 只取第一个prop的值即可

                    string strOutputDbName = "";
                    string strFrom = this.m_service.GetFromName(strDbName,
                        lAttritueValue,
                        out strOutputDbName,
                        out strError);
                    if (strFrom == null)
                        return -1;  // 寻找from名的过程发生错误

                    if (strTargetListValue != "")
                        strTargetListValue += ";";

                    Debug.Assert(strOutputDbName != "", "");

                    strTargetListValue += strOutputDbName + ":" + strFrom;
                }

                if (i != 0)
                    strQueryXml += "<operator value='OR' />";

                strQueryXml += "<target list='" + strTargetListValue + "'>"
                + "<item><word>"
                + StringUtil.GetXmlStringSimple(strTerm)
                + "</word><match>left</match><relation>=</relation><dataType>string</dataType>"
                + "<maxCount>" + nMaxResultCount.ToString() + "</maxCount></item>"
                + "<lang>zh</lang></target>";
            }

            // 如果有多个props，则需要在检索XML外面包裹一个<target>元素，以作为一个整体和其他部件进行逻辑操作
            if (prop_groups.Count > 1)
                strQueryXml = "<target>" + strQueryXml + "</target>";

            return 0;
        }

#endif

        // 根据RPN创建XML检索式
        // 本函数要递归调用，检索数据库并返回结果集
        // parameters:
        //		node    RPN 结构的根结点
        //		strXml[out] 返回局部XML检索式
        // return:
        //      -1  error
        //      0   succeed
        int BuildQueryXml(
            List<string> dbnames,
            BerNode node,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";
            int nRet = 0;

            if (node == null)
            {
                strError = "node == null";
                return -1;
            }

            if (0 == node.m_uTag)
            {
                // operand node

                // 检索得到 saRecordID
                if (node.ChildrenCollection.Count < 1)
                {
                    strError = "bad RPN structure";
                    return -1;
                }

                BerNode pChild = node.ChildrenCollection[0];

                if (102 == pChild.m_uTag)
                {
                    // AttributesPlusTerm
                    long nAttributeType = -1;
                    long nAttributeValue = -1;
                    string strTerm = "";

                    nRet = DecodeAttributeAndTerm(
                        this._searchTermEncoding,
                        pChild,
                        out nAttributeType,
                        out nAttributeValue,
                        out strTerm,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = BuildOneXml(
                        dbnames,
                        strTerm,
                        nAttributeValue,
                        out strQueryXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    return 0;

                    /*
			// 真的要去检索数据库啦
			SearchDBMulti(pResult, nAttributeValue, strTerm);
                     * */
                }
                else if (31 == pChild.m_uTag)
                {
                    // 是结果集参预了检索
                    string strResultSetID = pChild.GetCharNodeData();

                    strQueryXml = "<item><resultSetName>" + strResultSetID + "</resultSetName></item>";
                    /*
                    //
                    // 为了避免在递归运算时删除了以前保留的结果集，copy 一份
                    if (!FindAndCopyExistResultSet(strResultSetID, pResult)) {
                        throw_exception(0, _T("referred resultset not exist"));
                    }
                    //
                     * */
                }
                else
                {
                    //
                    strError = "Unsurported RPN structure";
                }

            }
            else if (1 == node.m_uTag)
            { // rpnRpnOp
                //
                if (3 != node.ChildrenCollection.Count)
                {
                    strError = "bad RPN structure";
                    return -1;
                }
                //
                string strXmlLeft = "";
                string strXmlRight = "";
                int nOperator = -1;

                nRet = BuildQueryXml(
                    dbnames,
                    node.ChildrenCollection[0],
                    out strXmlLeft,
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = BuildQueryXml(
                    dbnames,
                    node.ChildrenCollection[1],
                    out strXmlRight,
                    out strError);
                if (nRet == -1)
                    return -1;


                //	and     [0] 
                //	or      [1] 
                //	and-not [2] 
                nOperator = DecodeRPNOperator(node.ChildrenCollection[2]);
                if (nOperator == -1)
                {
                    strError = "DecodeRPNOperator() return -1";
                    return -1;
                }

                switch (nOperator)
                {
                    case 0: // and
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='AND' />" + strXmlRight + "</group>";
                        break;
                    case 1: // or 
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='OR' />" + strXmlRight + "</group>";
                        break;
                    case 2: // and-not
                        strQueryXml = "<group>" + strXmlLeft + "<operator value='SUB' />" + strXmlRight + "</group>";
                        break;
                    default:
                        // 不支持的操作符
                        strError = "unsurported operator";
                        return -1;
                }
            }
            else
            {
                strError = "bad RPN structure";
            }

            return 0;
        }

        // 2007/7/18
        //	 build a z39.50 Init response
        public static int Encode_InitialResponse(InitResponseInfo info,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;

            BerTree tree = new BerTree();

            root = tree.m_RootNode.NewChildConstructedNode(BerTree.z3950_initResponse,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(info.m_strReferenceId) == false)
            {
                root.NewChildCharNode(BerTree.z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(info.m_strReferenceId));
            }

            root.NewChildBitstringNode(BerTree.z3950_ProtocolVersion,   // 3
                BerNode.ASN1_CONTEXT,
                "yy");

            /* option
         search                 (0), 
         present                (1), 
         delSet                 (2),
         resourceReport         (3),
         triggerResourceCtrl    (4),
         resourceCtrl           (5), 
         accessCtrl             (6),
         scan                   (7),
         sort                   (8), 
         --                     (9) (reserved)
         extendedServices       (10),
         level-1Segmentation    (11),
         level-2Segmentation    (12),
         concurrentOperations   (13),
         namedResultSets        (14)
            15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
            16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
            17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
            18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
            19 Query type 104 
*/
            root.NewChildBitstringNode(BerTree.z3950_Options,   // 4
                BerNode.ASN1_CONTEXT,
                info.m_strOptions);    // "110000000000001"

            root.NewChildIntegerNode(BerTree.z3950_PreferredMessageSize,    // 5
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_lPreferredMessageSize));

            root.NewChildIntegerNode(BerTree.z3950_ExceptionalRecordSize,   // 6
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_lExceptionalRecordSize));

            // 2007/11/7 原来这个事项曾经位置不对，现在调整到这里
            // bool
            root.NewChildIntegerNode(BerTree.z3950_result,  // 12
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)info.m_nResult));


            root.NewChildCharNode(BerTree.z3950_ImplementationId,   // 110
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationId));

            root.NewChildCharNode(BerTree.z3950_ImplementationName, // 111
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationName));

            root.NewChildCharNode(BerTree.z3950_ImplementationVersion,  // 112
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(info.m_strImplementationVersion));  // "3"


            // userInformationField
            if (info.UserInfoField != null)
            {
                BerNode nodeUserInfoRoot = root.NewChildConstructedNode(BerTree.z3950_UserInformationField,    // 11
                    BerNode.ASN1_CONTEXT);
                info.UserInfoField.Build(nodeUserInfoRoot);
            }

            if (info.m_charNego != null)
            {
                info.m_charNego.EncodeResponse(root);
            }

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }
    }

    // Init请求信息结构
    public class InitRequestInfo
    {
        public string m_strReferenceId = "";
        public string m_strProtocolVersion = "";

        public string m_strOptions = "";

        public long m_lPreferredMessageSize = 0;
        public long m_lExceptionalRecordSize = 0;

        public int m_nAuthenticationMethod = 0;	// 0: open 1:idPass
        public string m_strGroupID = "";
        public string m_strID = "";
        public string m_strPassword = "";

        public string m_strImplementationId = "";
        public string m_strImplementationName = "";
        public string m_strImplementationVersion = "";

        public CharsetNeogatiation m_charNego = null;
    }

    public class InitResponseInfo
    {
        public string m_strReferenceId = "";
        public string m_strOptions = "";
        public long m_lPreferredMessageSize = 0;
        public long m_lExceptionalRecordSize = 0;
        public long m_nResult = 0;

        public string m_strImplementationId = "";
        public string m_strImplementationName = "";
        public string m_strImplementationVersion = "";

        // public long m_lErrorCode = 0;

        // public string m_strErrorMessage = "";
        public External UserInfoField = null;

        public CharsetNeogatiation m_charNego = null;
    }

    // Search请求信息结构
    public class SearchRequestInfo
    {
        public string m_strReferenceId = "";

        public long m_lSmallSetUpperBound = 0;
        public long m_lLargeSetLowerBound = 0;
        public long m_lMediumSetPresentNumber = 0;

        // bool
        public long m_lReplaceIndicator = 0;

        public string m_strResultSetName = "default";
        public List<string> m_dbnames = null;

        public BerNode m_rpnRoot = null;
    }

    // Search响应信息结构
    public class SearchResponseInfo
    {
        public string m_strReferenceId = "";

        public long m_lResultCount = 0;
        public long m_lNumberOfRecordReturned = 0;
        public long m_lNextResultSetPosition = 0;

        // bool
        public long m_lSearchStatus = 0;
    }

    // Present请求信息结构
    public class PresentRequestInfo
    {
        public string m_strReferenceId = "";

        public string m_strResultSetID = "";
        public long m_lResultSetStartPoint = 0;
        public long m_lNumberOfRecordsRequested = 0;
        public List<string> m_elementSetNames = null;
    }

    /*
    // Present响应信息结构
    public class PresentResponseInfo
    {
        public string m_strReferenceId = "";

        public string m_strResultSetID = "default"; // 结果集名。从present请求获得
        public long m_lNumberOfRecordReturned = 0;  // 检索命中的结果总数。从结果集获得

        public long m_lResultSetStartPoint = 0; // 要获取的开始偏移。从present请求获得
        public long m_lNumberOfRecordsRequested = 0;    // 要获取的记录数。从present请求获得



        // nextResultSetPosition
        // if 0, that's end of the result set
        // else M+1, M is 最后一次 present response 的最后一条记录在 result set 中的 position
        public long m_lNextResultSetPosition = 0;

        // presentStatus
        // success      (0),
        // partial-1    (1),
        // partial-2    (2),
        // partial-3    (3),
        // partial-4    (4),
        // failure      (5).
        public long m_lPresentStatus = 0;

        public List<string> m_paths = null; // 要获得的本批记录的路径

        public List<string> m_elementSetNames = null;   // 元素集名们。从present请求获得
    }
     * */

    /*
External is defined in the ASN.1 standard.

EXTERNAL ::= [UNIVERSAL 8] IMPLICIT SEQUENCE
    {direct-reference      OBJECT IDENTIFIER OPTIONAL,
     indirect-reference    INTEGER           OPTIONAL,
     data-value-descriptor ObjectDescriptor  OPTIONAL,
     encoding              CHOICE
        {single-ASN1-type  [0] ANY,
         octet-aligned     [1] IMPLICIT OCTET STRING,
         arbitrary         [2] IMPLICIT BIT STRING}}

In Z39.50, we use the direct-reference option and omit the
indirect-reference and data-value-descriptor.  For the encoding, we use
single-asn1-type if the record has been defined with ASN.1.  Examples would
be GRS-1 and SUTRS records.  We use octet-aligned for non-ASN.1 records.
The most common example of this would be a MARC record.

Hope this helps!

Ralph
     * */
    // 检索命中的记录
    public class RetrivalRecord
    {
        public string m_strDatabaseName = "";    //
        public External m_external = null;
        public DiagFormat m_surrogateDiagnostic = null;

        // 估算数据所占的包尺寸
        public int GetPackageSize()
        {
            int nSize = 0;

            if (String.IsNullOrEmpty(this.m_strDatabaseName) == false)
            {
                nSize += Encoding.UTF8.GetByteCount(this.m_strDatabaseName);
            }

            if (this.m_external != null)
                nSize += this.m_external.GetPackageSize();

            if (this.m_surrogateDiagnostic != null)
                nSize += this.m_surrogateDiagnostic.GetPackageSize();

            return nSize;
        }

        // 构造NamePlusRecord子树
        // parameters:
        //      node    NamePlusRecord的容器节点。也就是Present Response的根节点
        public void BuildNamePlusRecord(BerNode node)
        {
            if (this.m_external == null
                && this.m_surrogateDiagnostic == null)
                throw new Exception("m_external 和 m_surrogateDiagnostic 不能同时为空");

            if (this.m_external != null
                && this.m_surrogateDiagnostic != null)
                throw new Exception("m_external 和 m_surrogateDiagnostic 不能同时为非空。只能有一个为空");


            BerNode pSequence = node.NewChildConstructedNode(
                BerNode.ASN1_SEQUENCE,    // 16
                BerNode.ASN1_UNIVERSAL);

            // 数据库名
            pSequence.NewChildCharNode(0,
                BerNode.ASN1_CONTEXT,   // ASN1_PRIMITIVE, BUG!!!
                Encoding.UTF8.GetBytes(this.m_strDatabaseName));

            // record(一条记录)
            BerNode nodeRecord = pSequence.NewChildConstructedNode(
                1,
                BerNode.ASN1_CONTEXT);


            if (this.m_external != null)
            {
                // extenal
                BerNode nodeRetrievalRecord = nodeRecord.NewChildConstructedNode(
                    1,
                    BerNode.ASN1_CONTEXT);

                // real extenal!
                BerNode nodeExternal = nodeRetrievalRecord.NewChildConstructedNode(
                    8,  // UNI_EXTERNAL
                    BerNode.ASN1_UNIVERSAL);

                // TODO: 和前一条重复的库名和marc syntax oid可以省略？

                Debug.Assert(String.IsNullOrEmpty(this.m_external.m_strDirectRefenerce) == false, "");

                nodeExternal.NewChildOIDsNode(6,   // UNI_OBJECTIDENTIFIER,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_external.m_strDirectRefenerce);

                // 1 条 MARC 记录
                nodeExternal.NewChildCharNode(1,
                    BerNode.ASN1_CONTEXT,
                    this.m_external.m_octectAligned);
            }

            // 如果获得MARC记录出错，则这里要创建SurrogateDiagnostic record
            if (this.m_surrogateDiagnostic != null)
            {
                BerNode nodeSurrogateDiag = nodeRecord.NewChildConstructedNode(
                    2,
                    BerNode.ASN1_CONTEXT);

                BerNode nodeDiagRoot = nodeSurrogateDiag.NewChildConstructedNode(
                    BerNode.ASN1_SEQUENCE, // sequence
                    BerNode.ASN1_UNIVERSAL);

                this.m_surrogateDiagnostic.BuildBer(nodeDiagRoot);

                /*
                nodeDiagRoot.NewChildOIDsNode(6,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_surrogateDiagnostic.m_strDiagSetID);   // "1.2.840.10003.4.1"

                nodeDiagRoot.NewChildIntegerNode(2,
                    BerNode.ASN1_UNIVERSAL,
                    BitConverter.GetBytes((long)this.m_surrogateDiagnostic.m_nDiagCondition));

                if (String.IsNullOrEmpty(this.m_surrogateDiagnostic.m_strAddInfo) == false)
                {
                    nodeDiagRoot.NewChildCharNode(26,
                        BerNode.ASN1_UNIVERSAL,
                        Encoding.UTF8.GetBytes(this.m_surrogateDiagnostic.m_strAddInfo));
                }
                 * */


            }

        }

    }

}
