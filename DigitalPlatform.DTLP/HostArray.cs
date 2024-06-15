using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;

using DigitalPlatform.Xml;
using System.Threading.Tasks;

namespace DigitalPlatform.DTLP
{
    public class HostArray : ArrayList
    {
        ApplicationInfo appinfo = null;

        public DtlpChannel Container = null;

        // 从ini文件或者registry装载已经配置的所有主机事项
        public int InitialHostArray(ApplicationInfo appInfoParam)
        {
            int i, nMax;
            HostEntry entry = null;

            this.Clear();

            appinfo = appInfoParam;	// 保存下来备用

            if (appInfoParam == null)   // 2006/11/21
                return 0;

            ArrayList saHost = LoadHosts(appInfoParam);
            nMax = saHost.Count;
            for (i = 0; i < nMax; i++)
            {
                entry = new HostEntry();
                entry.m_strHostName = (string)saHost[i];
                this.Add(entry);
                entry.Container = this;
            }

            return 0;
        }

        // 从ini文件或者registry装载已经配置的所有主机事项
        public static ArrayList LoadHosts(ApplicationInfo appInfo)
        {
            ArrayList saResult = new ArrayList();

            for (int i = 0; ; i++)
            {
                string strEntry = "entry" + Convert.ToString(i + 1);

                string strValue = appInfo.GetString("ServerAddress",
                    strEntry,
                    "");
                if (strValue == "")
                    break;
                saResult.Add(strValue);
            }

            return saResult;
        }

        // 将CStringArray中的主机事项写入ini文件或者registry
        public static void SaveHosts(ApplicationInfo appInfo,
            ArrayList saHost)
        {
            string strEntry = null;
            int i = 0;

            for (i = 0; i < saHost.Count; i++)
            {

                strEntry = "entry" + Convert.ToString(i + 1);

                string strValue = (string)saHost[i];

                appInfo.SetString("ServerAddress",
                    strEntry,
                    strValue);
            }

            // 最后一次，截断
            strEntry = "entry" + Convert.ToString(i + 1);
            appInfo.SetString("ServerAddress",
                strEntry,
                "");

        }

        // 摧毁一个Host事项
        public int DestroyHostEntry(HostEntry entry)
        {
            this.Remove(entry);
            return 0; // not found
        }

        // 以主机名字或者别名寻找主机事项
        public HostEntry MatchHostEntry(string strHostName)
        {

            for (int i = 0; i < this.Count; i++)
            {
                HostEntry entry = (HostEntry)this[i];
                Debug.Assert(entry != null, "HostEntry中出现空元素");

                if ((String.Compare(strHostName, entry.m_strHostName, true) == 0)
                    || (String.Compare(strHostName, entry.m_strAlias, true) == 0))
                    return entry;
            }
            return null;
        }

        public void CloseAllSockets()
        {

            for (int i = 0; i < this.Count; i++)
            {
                HostEntry entry = (HostEntry)this[i];
                Debug.Assert(entry != null, "HostEntry中出现空元素");

                if (entry.client != null)
                {
                    entry.client.Close();
                    entry.client = null;
                }
            }
        }
    }

    /// <summary>
    /// Summary description for HostArray.
    /// </summary>
    public class HostEntry
    {
        // 2024/6/6
        public TimeSpan _sendTimeout = TimeSpan.FromSeconds(10);
        public TimeSpan _recvTimeout = TimeSpan.FromSeconds(10);


        // SOCKET		m_hSocket;

        public TcpClient client = null;

        public string m_strHostName = "";   // IP地址或者域名
        public string m_strAlias = "";      // 别名

        public int m_nDTLPCharset = DtlpChannel.CHARSET_DBCS;

        // int			m_nLock = 0;

        //		bool		m_bWantDel = false;
        public int m_lUsrID = 0;
        public int m_lChannel = -1;

        //		int			m_nStatus = 0;	// 0:空闲 1:发送 2:接收


        public HostArray Container = null;


        public HostEntry()
        {
            //m_hSocket = INVALID_SOCKET;
            // m_nDTLPCharset = CHARSET_DBCS;
        }

        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        ~HostEntry()
        {
            if (m_lChannel != -1 && client != null)
            {
                // RmtDestroyChannel();
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        // 远程建立通道
        // 返回 -1 表示失败
        public int RmtCreateChannel(int usrid)
        {

            // send:long usrid
            // recv:
            // return long

            DTLPParam param = new DTLPParam();
            int lRet;
            int nRet;
            int nLen;
            byte[] baSendBuffer = null;
            byte[] baRecvBuffer = null;
            int nErrorNo = 0;

            Debug.Assert(client != null,
                "client为空");

            param.Clear();
            param.ParaLong(usrid);
            lRet = param.ParaToPackage(DtlpChannel.FUNC_CREATECHANNEL,
                nErrorNo,
                out baSendBuffer);

            if (lRet == -1)
            {
                return -1; // error
            }

            nRet = SendTcpPackage(baSendBuffer,
                baSendBuffer.Length,
                out nErrorNo);

            if (nRet < 0)
            {
                return -1;
            }

            nRet = RecvTcpPackage(out baRecvBuffer,
                out nLen,
                out nErrorNo);
            if (nRet < 0)
            {
                return -1;
            }

            param.Clear();
            param.DefPara(Param.STYLE_LONG);


            int nTempFuncNum = 0;
            param.PackageToPara(baRecvBuffer,
                ref nErrorNo,
                out nTempFuncNum);

            lRet = param.lValue(0);

            m_lChannel = lRet;
            m_lUsrID = usrid;

            return lRet;
        }


        // 注销远程通道
        public int RmtDestroyChannel()

        {
            // send:long usrid
            //      long Channel
            // recv:
            // return long

            DTLPParam param = new DTLPParam();
            int lRet;
            int nRet;
            int nLen;
            byte[] baSendBuffer = null;
            byte[] baRecvBuffer = null;
            int nErrorNo = 0;

            Debug.Assert(client != null,
                "client为空");

            param.Clear();
            param.ParaLong(m_lUsrID);
            param.ParaLong(m_lChannel);

            lRet = param.ParaToPackage(DtlpChannel.FUNC_DESTROYCHANNEL,
                nErrorNo,
                out baSendBuffer);

            if (lRet == -1)
            {
                return -1; // error
            }

            nRet = SendTcpPackage(baSendBuffer,
                baSendBuffer.Length,
                out nErrorNo);
            if (nRet < 0)
            {
                return -1;
            }

            nRet = RecvTcpPackage(out baRecvBuffer,
                out nLen,
                out nErrorNo);

            if (nRet < 0)
            {
                return -1;
            }

            param.Clear();
            param.DefPara(Param.STYLE_LONG);


            int nTempFuncNum = 0;
            param.PackageToPara(baRecvBuffer,
                ref nErrorNo,
                out nTempFuncNum);

            lRet = param.lValue(0);

            m_lChannel = -1;

            return lRet;
        }



        // connect()到主机
        // 原来的模块，是先检查空格，如果有，去掉空格右边。
        // 然后，看是否有"()"，如果有，去掉中间的内容(包括括号)。"()"可以多次出现
        public int ConnectSocket(string strHostName,
            out int nErrorNo)
        {
            string strPort = "";
            nErrorNo = 0;

            m_strHostName = strHostName;    // 加工前的字符串

            int nRet = strHostName.IndexOf(":", 0);
            if (nRet != -1)
            {
                strPort = strHostName.Substring(nRet + 1);
                strPort.Trim();
                strHostName = strHostName.Substring(0, nRet);
                strHostName.Trim();
            }

            int nPort = 3001;

            if (strPort != "")
                nPort = Convert.ToInt32(strPort);

            try
            {
                client = new TcpClient(strHostName, nPort);
                //client.ReceiveTimeout;
                //client.SendTimeout;

                /*
                var temp = new TcpClientWithTimeout(strHostName,
                    nPort,
                    Convert.ToInt32(this._sendTimeout.TotalMilliseconds));
                client = temp.Connect();
                */
            }
            catch (SocketException)
            {
                nErrorNo = DtlpChannel.GL_CONNECT;
                // 是否返回错误字符串? 精确区分错误类型
                return -1;
            }

            return 0;
            /*
	
			ERROR1:
				if (client != null)
					client.Close();

			client = null;
			return -1;
			*/
        }


        public int CloseSocket()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

            return 0;
        }


#if OLD
		// 发出请求包
		public int SendTcpPackage(byte []baPackage,
			int nLen,
			out int nErrorNo)
		{
			// nErrorNo = 0;
			nErrorNo = DtlpChannel.GL_INTR;

			if (client == null)
				return -1;

            try
            {

                NetworkStream stream = client.GetStream();

                stream.Write(baPackage, 0, nLen);
            }
            catch (Exception /*ex*/)  // 2006/11/13
            {
                nErrorNo = DtlpChannel.GL_SEND;

                // 2008/10/7
                if (client != null)
                {
                    client.Close();
                    client = null;
                }


                return -1;
            }


			/*
	int nOutLen;
	int wRet;

	ASSERT(m_hSocket != INVALID_SOCKET);
	
	nOutLen = 0;
	while (nOutLen < nLen) {
		
		
		wRet = send (m_hSocket,
			pPackage + nOutLen,
			nLen-nOutLen,
			0);
		
		if ( wRet==0 || wRet == SOCKET_ERROR ) {
			nErrorNo = WSAGetLastError();
			if (nErrorNo == WSAEWOULDBLOCK)
				continue;
			// close socket
			closesocket(m_hSocket);
			m_hSocket = INVALID_SOCKET;
			return -1;
		}
		nOutLen += wRet;
	}
	*/
	
			nErrorNo = 0;
			return 0;
		}
#else

        // 发出请求包
        public int SendTcpPackage(byte[] baPackage,
            int nLen,
            out int nErrorNo)
        {
            // nErrorNo = 0;
            nErrorNo = DtlpChannel.GL_INTR;

            if (client == null)
                return -1;

            try
            {

                // TODO: 是否要关闭 NetworkStream !!!
                NetworkStream stream = client.GetStream();
                var start_time = DateTime.Now;
                IAsyncResult result = stream.BeginWrite(baPackage, 0, nLen,
                    null, null);
                for (; ; )
                {
                    /*
                    int nRet = Container.Container.Container.procIdle(this);
                    if (nRet == 1)
                        return -1;
                    System.Threading.Thread.Sleep(100);
                     * */
                    if (Container.Container.Container.DoIdle(this) == true)
                        return -1;

                    if (result.IsCompleted)
                        break;

                    // 2024/6/6
                    if (DateTime.Now - start_time > _sendTimeout)
                    {
                        nErrorNo = DtlpChannel.GL_SEND;
                        if (client != null)
                        {
                            client.Close();
                            client = null;
                        }
                        return -1;
                    }
                }

                stream.EndWrite(result);
            }
            catch (Exception /*ex*/)
            {
                nErrorNo = DtlpChannel.GL_SEND;

                // 2008/10/7
                if (client != null)
                {
                    client.Close();
                    client = null;
                }
                return -1;
            }

            nErrorNo = 0;
            return 0;
        }

#endif


#if OLD
		// 接收响应包
		// 最少收到4byte，就知道了包的尺寸
		public int RecvTcpPackage(out byte []baPackage,
			out int nLen,
			out int nErrorNo)
		{
			// nErrorNo = 0;
			nErrorNo = DtlpChannel.GL_INTR;

			int nInLen;
			int wRet;
			int l;
			bool bInitialLen = false;

			Debug.Assert(client != null, "client为空");
	
			baPackage = new byte [4096];
			nInLen = 0;
			nLen = 4096; //COMM_BUFF_LEN;
	
			while ( nInLen < nLen ) 
			{

				if (Container.Container.Container.procIdle != null)
				{
					if (client != null && client.GetStream().DataAvailable == false) 
					{
						int nRet = Container.Container.Container.procIdle(this);
						if (nRet == 1)
							goto ERROR1;
						System.Threading.Thread.Sleep(100);
						continue;
					}
				}
					

				if (client == null) 
				{
					goto ERROR1;
				}
		
				wRet = client.GetStream().Read(baPackage, 
					nInLen,
					baPackage.Length - nInLen);

				if ( wRet == 0) 
				{
					goto ERROR1;
				}

				// 得到包的长度
		
				if ((wRet>=4||nInLen>=4)
					&& bInitialLen == false) 
				{

					l = BitConverter.ToInt32(baPackage, 0);
					l = IPAddress.NetworkToHostOrder((Int32)l);
					nLen = (int)l;

                    if (nLen >= (1000 * 1024))  // 2006/11/26
                    {
                        // 长度位出现异常
                        goto ERROR1;
                    }

					// 正式分配缓冲区尺寸
					byte [] temp = new byte [nLen];
					Array.Copy(baPackage, 0, temp, 0, nInLen + wRet);
					baPackage = temp;

					bInitialLen = true;
				}

				nInLen += wRet;
				if (nInLen >= baPackage.Length
					&& bInitialLen == false) // 不太可能发生
				{
					byte [] temp = new byte [baPackage.Length + 4096];
					Array.Copy(baPackage, 0, temp, 0, nInLen);
					baPackage = temp;
				}
			}

			// 最后规整缓冲区尺寸，如果必要的话
			if (baPackage.Length > nLen) 
			{
				byte [] temp = new byte [nLen];
				Array.Copy(baPackage, 0, temp, 0, nLen);
				baPackage = temp;

			}

			nErrorNo = 0;
			return 0;
			ERROR1:
				if (client != null) 
				{
					client.Close();
					client = null;
				}

			return -1;
		}
#else  
        // 接收响应包
        // 最少收到4byte，就知道了包的尺寸
        public int RecvTcpPackage(out byte[] baPackage,
            out int nLen,
            out int nErrorNo)
        {
            nErrorNo = DtlpChannel.GL_INTR;

            int nInLen;
            int wRet;
            int l;
            bool bInitialLen = false;

            Debug.Assert(client != null, "client为空");

            baPackage = new byte[4096];
            nInLen = 0;
            nLen = 4096; //COMM_BUFF_LEN;

            // TODO: 是否要关闭 NetworkStream !!!
            NetworkStream stream = client.GetStream();

            var start_time = DateTime.Now;
            while (nInLen < nLen)
            {
                /*
                if (Container.Container.Container.procIdle != null)
                {
                 * */
                if (client != null && stream.DataAvailable == false)
                {
                    /*
                    int nRet = Container.Container.Container.procIdle(this);
                    if (nRet == 1)
                        goto ERROR1;
                    System.Threading.Thread.Sleep(100);
                     * */
                    if (Container.Container.Container.DoIdle(this) == true)
                        goto ERROR1;

                    // 2024/6/6
                    if (DateTime.Now - start_time > _recvTimeout)
                    {
                        nErrorNo = DtlpChannel.GL_RECV;
                        if (client != null)
                        {
                            client.Close();
                            client = null;
                        }
                        return -1;
                    }

                    continue;
                }
                /*
                }
                 * */


                if (client == null)
                {
                    goto ERROR1;
                }

                IAsyncResult result = stream.BeginRead(baPackage, nInLen, baPackage.Length - nInLen,
                    null, null);
                for (; ; )
                {
                    /*
                    int nRet = Container.Container.Container.procIdle(this);
                    if (nRet == 1)
                        goto ERROR1;
                    System.Threading.Thread.Sleep(100);
                     * */
                    if (Container.Container.Container.DoIdle(this) == true)
                        goto ERROR1;

                    if (result.IsCompleted)
                        break;

                    // 2024/6/6
                    if (DateTime.Now - start_time > _recvTimeout)
                    {
                        nErrorNo = DtlpChannel.GL_RECV;
                        if (client != null)
                        {
                            client.Close();
                            client = null;
                        }
                        return -1;
                    }
                }

                // 2024/6/5
                // 捕获异常
                try
                {
                    wRet = stream.EndRead(result);
                    if (wRet == 0)
                    {
                        goto ERROR1;
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    goto ERROR1;
                }

                // 得到包的长度

                if ((wRet >= 4 || nInLen >= 4)
                    && bInitialLen == false)
                {
                    l = BitConverter.ToInt32(baPackage, 0);
                    l = IPAddress.NetworkToHostOrder((Int32)l);
                    nLen = (int)l;

                    if (nLen >= (1000 * 1024))  // 2006/11/26
                    {
                        // 长度位出现异常
                        goto ERROR1;
                    }

                    // 正式分配缓冲区尺寸
                    byte[] temp = new byte[nLen];
                    Array.Copy(baPackage, 0, temp, 0, nInLen + wRet);
                    baPackage = temp;

                    bInitialLen = true;
                }

                nInLen += wRet;
                if (nInLen >= baPackage.Length
                    && bInitialLen == false) // 不太可能发生
                {
                    byte[] temp = new byte[baPackage.Length + 4096];
                    Array.Copy(baPackage, 0, temp, 0, nInLen);
                    baPackage = temp;
                }
            }

            // 最后规整缓冲区尺寸，如果必要的话
            if (baPackage.Length > nLen)
            {
                byte[] temp = new byte[nLen];
                Array.Copy(baPackage, 0, temp, 0, nLen);
                baPackage = temp;
            }

            nErrorNo = 0;
            return 0;
        ERROR1:
            if (client != null)
            {
                client.Close();
                client = null;
            }
            return -1;
        }

#endif


    }
}
