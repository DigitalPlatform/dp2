using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using System.Runtime.Serialization;

using System.Runtime.InteropServices;
using System.Collections.Specialized;

using System.Text;
using System.Web;
using System.Threading;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{
	/// <summary>
	/// 适于进行操作的成套环境
	/// </summary>
	public class SearchPanel
	{
        /// <summary>
        /// 浏览记录到达
        /// </summary>
		public event BrowseRecordEventHandler BrowseRecord = null;

        /// <summary>
        /// 应用程序信息
        /// </summary>
		public ApplicationInfo ap = null;	// 引用

        /// <summary>
        /// 在ap中保存窗口外观状态的标题字符串
        /// </summary>
		public string ApCfgTitle = "";

        /// <summary>
        /// 停止管理器
        /// </summary>
		public DigitalPlatform.StopManager	stopManager = new DigitalPlatform.StopManager();

        /// <summary>
        /// 配置文件缓存
        /// </summary>
		public CfgCache cfgCache = null;	// 引用

        /// <summary>
        /// 服务器信息集合
        /// </summary>
		public ServerCollection Servers = null;	// 引用

        /// <summary>
        /// 通道集合
        /// </summary>
		public RmsChannelCollection Channels = new RmsChannelCollection();	// 拥有

        /// <summary>
        /// 用于管理停止操作的对象
        /// </summary>
		DigitalPlatform.Stop stop = null;

        /// <summary>
        /// 通道
        /// </summary>
		RmsChannel channel = null;

		string m_strServerUrl = "";

        /// <summary>
        /// 构造函数
        /// </summary>
		public SearchPanel()
		{
		}

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="servers">服务器信息集合</param>
        /// <param name="cfgcache">配置文件缓存</param>
		public void Initial(ServerCollection servers,
			CfgCache cfgcache)
		{
			this.Servers = servers;

            /*
			this.Channels.procAskAccountInfo = 
				new Delegate_AskAccountInfo(this.Servers.AskAccountInfo);
             */
            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);
            this.Channels.AskAccountInfo += new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);

			this.cfgCache = cfgcache;
		}

        /// <summary>
        /// 初始化停止管理器
        /// </summary>
        /// <param name="buttonStop">停止按钮</param>
        /// <param name="labelMessage">消息标签</param>
		public void InitialStopManager(Button buttonStop,
			Label labelMessage)
		{
			stopManager.Initial(buttonStop,
				labelMessage,
                null);
			stop = new DigitalPlatform.Stop();
            stop.Register(this.stopManager, true);	// 和容器关联
		}

        /// <summary>
        /// 初始化停止管理器
        /// </summary>
        /// <param name="toolbarbuttonstop">工具条上的停止按钮</param>
        /// <param name="statusbar">状态条</param>
		public void InitialStopManager(ToolBarButton toolbarbuttonstop,
			StatusBar statusbar)
		{
			stopManager.Initial(toolbarbuttonstop,
				statusbar,
                null);
			stop = new DigitalPlatform.Stop();
            stop.Register(this.stopManager, true);	// 和容器关联
		}

        /// <summary>
        /// 和停止管理器脱离关联
        /// </summary>
		public void FinishStopManager()
		{
			if (stop != null) // 脱离关联
			{
				stop.Unregister();	// 和容器关联
				stop = null;
			}
		}

        /// <summary>
        /// 从ap中装载Form状态信息
        /// </summary>
        /// <param name="form">Form对象</param>
		public void LoadFormStates(Form form)
		{
			if (ap != null) 
			{
				if (ApCfgTitle != "" && ApCfgTitle != null) 
				{
					ap.SaveFormStates(form,
						ApCfgTitle);
				}
				else 
				{
					Debug.Assert(true, "若要用ap保存和恢复窗口外观状态，必须先设置ApCfgTitle成员");
				}

			}
		}

        /// <summary>
        /// 将Form状态信息保存到ap中
        /// </summary>
        /// <param name="form">Form对象</param>
		public void SaveFormStates(Form form)
		{
			if (ap != null) 
			{
				if (ApCfgTitle != "" && ApCfgTitle != null) 
				{
					ap.SaveFormStates(form,
						ApCfgTitle);
				}
				else 
				{
					Debug.Assert(true, "若要用ap保存和恢复窗口外观状态，必须先设置ApCfgTitle成员");
				}

			}
		}

        /// <summary>
        /// 缺省服务器URL
        /// </summary>
		public virtual string ServerUrl
		{
			get 
			{
				return m_strServerUrl;
			}
			set 
			{
				m_strServerUrl = value;
			}
		}

        /// <summary>
        /// 获得配置文件
        /// </summary>
        /// <param name="strServerUrl">服务器URL。如果为null，则自动使用this.ServerUrl</param>
        /// <param name="strCfgFilePath">配置文件纯路径，不包含ServerUrl部分</param>
        /// <param name="strContent">返回配置文件内容</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
		public int GetCfgFile(
			string strServerUrl,
			string strCfgFilePath,
			out string strContent,
			out string strError)
		{
			strError = "";
			strContent = "";

			if (strServerUrl == "" || strServerUrl == null)
			{
				strServerUrl = this.ServerUrl;
			}

			if (strServerUrl == "")
			{
				strError = "尚未指定服务器URL";
				return -1;
			}

			RmsChannel channelSave = channel;

			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{

				this.BeginLoop("正在下载文件" + strCfgFilePath);

				byte[] baTimeStamp = null;
				string strMetaData;
				string strOutputPath;

				long lRet = channel.GetRes(
					this.cfgCache,
					strCfgFilePath,
					out strContent,
					out strMetaData,
					out baTimeStamp,
					out strOutputPath,
					out strError);

				this.EndLoop();



				if (lRet == -1) 
				{
					if (channel.ErrorCode == ChannelErrorCode.NotFound)
						return 0;	// not found
					return -1;
				}

				return 1;	// found
			}
			finally 
			{
				this.channel = channelSave;
			}
		}

        /// <summary>
        /// 当停止按钮按下时触发的动作
        /// </summary>
		public void DoStopClick()
		{
			if (stopManager != null)
				stopManager.DoStopActive();
		}

        /// <summary>
        /// 终止通讯操作
        /// </summary>
		public void DoStop(object sender, StopEventArgs e)
		{
			if (this.channel != null)
				this.channel.Abort();
		}

		// 
		// return:
		//		-1	error
		//		0	not found
		//		1	found
        /// <summary>
        /// 获得配置文件
        /// </summary>
        /// <param name="strServerUrl">服务器URL。如果为null，则自动使用this.ServerUrl</param>
        /// <param name="strCfgFilePath">配置文件纯路径，不包含ServerUrl部分</param>
        /// <param name="dom">返回装载了配置文件内容的XmlDocument对象</param>
        /// <param name="strError"></param>
        /// <returns>-1出错;0没有找到;1找到</returns>
		public int GetCfgFile(
			string strServerUrl,
			string strCfgFilePath,
			out XmlDocument dom,
			out string strError)
		{
			strError = "";
			dom = null;

			string strContent = "";

			int nRet = GetCfgFile(
				strServerUrl,
				strCfgFilePath,
				out strContent,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			dom = new XmlDocument();

			try 
			{
				dom.LoadXml(strContent);
			}
			catch (Exception ex)
			{
				dom = null;
				strError = "装载配置文件 '"+strCfgFilePath+"' 内容进入dom失败: " + ex.Message;
				return -1;
			}

			return 1;
		}

        /// <summary>
        /// 开始检索循环
        /// </summary>
        /// <param name="strMessage">循环期间要显示在状态行的提示信息</param>
		public void BeginLoop(string strMessage)
		{
			if (stop != null)
			{
                stop.OnStop += new StopEventHandler(this.DoStop);
				stop.Initial(strMessage);
				stop.BeginLoop();
			}
		}

        /// <summary>
        /// 结束检索循环
        /// </summary>
		public void EndLoop()
		{
			if (stop != null)
			{
				stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
				stop.Initial("");
			}
		}


        /// <summary>
        /// 检索一个命中结果
        /// </summary>
        /// <param name="strServerUrl">服务器URL</param>
        /// <param name="strQueryXml">检索式XML</param>
        /// <param name="strPath">返回的记录路径</param>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1	一般错误;0	not found;1	found;>1	命中多于一条</returns>
		public int SearchOnePath(
            string strServerUrl,
			string strQueryXml,
			out string strPath,
			out string strError)
		{
			strPath = "";
			strError = "";

            if (String.IsNullOrEmpty(strServerUrl) == true)
                strServerUrl = this.ServerUrl;

			RmsChannel channelSave = channel;

			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{


				long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
				if (lRet == -1) 
					return -1;

				if (lRet == 0) 
				{
					return 0;	// 没有找到
				}

				long lCount = lRet;

				if (lRet > 1)
				{
					strError = "命中 " + Convert.ToString(lRet) + " 条。";
				}

				List<string> aPath = null;
				lRet = channel.DoGetSearchResult(
                    "default",
                    0,
					1,
					"zh",
					this.stop,
					out aPath,
					out strError);
				if (lRet == -1) 
				{
					strError = "获取检索结果时出错: " + strError;
					return -1;
				}


				strPath = (string)aPath[0];

				return (int)lCount;
			}
			finally 
			{
				channel = channelSave;
			}
		}


        /// <summary>
        /// 检索得到若干命中结果
        /// </summary>
        /// <param name="strServerUrl">服务器URL</param>
        /// <param name="strQueryXml">检索式XML</param>
        /// <param name="nMax">最大结果数</param>
        /// <param name="aPath">返回的记录路径数组</param>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1	一般错误;0	not found;1	found;>1	命中多于一条</returns>
        public int SearchMultiPath(
            string strServerUrl,
            string strQueryXml,
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            aPath = null;
            strError = "";

            if (String.IsNullOrEmpty(strServerUrl) == true)
                strServerUrl = this.ServerUrl;

            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            try
            {


                long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                {
                    return 0;	// 没有找到
                }

                long lCount = lRet;

                lCount = Math.Min(lCount, nMax);

                if (lRet > 1)
                {
                    strError = "命中 " + Convert.ToString(lRet) + " 条。";
                }

                lRet = channel.DoGetSearchResult(
                    "default",
                    0,
                    lCount,
                    "zh",
                    this.stop,
                    out aPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获取检索结果时出错: " + strError;
                    return -1;
                }

                return (int)lCount;
            }
            finally
            {
                channel = channelSave;
            }
        }


        /// <summary>
        /// 获取记录
        /// </summary>
        /// <param name="strServerUrl">服务器URL。如果==null，表示用SearchPannel自己的ServerUrl</param>
        /// <param name="strPath">记录路径</param>
        /// <param name="dom">返回装载了记录内容的XmlDocument对象</param>
        /// <param name="baTimeStamp">返回时间戳</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1	error;0	not found;1	found</returns>
		public int GetRecord(
            string strServerUrl,
            string strPath,
			out XmlDocument dom,
			out byte[] baTimeStamp,
			out string strError)
		{
			dom = null;
			string strXml = "";

            int nRet = GetRecord(
                strServerUrl,
                strPath,
                out strXml,
                out baTimeStamp,
                out strError);
			if (nRet == -1 || nRet == 0)
				return nRet;

			dom = new XmlDocument();
			try 
			{
				dom.LoadXml(strXml);
			}
			catch(Exception ex)
			{
				strError = "装载路径为 '"+strPath+"' 的xml记录时出错: " + ex.Message;
				return -1;
			}

			return 1;
		}


        /// <summary>
        /// 获取浏览记录。触发事件的版本
        /// </summary>
        /// <param name="fullpaths">若干记录全路径</param>
        /// <param name="bReverse">fullpaths中路径是否为反转形式</param>
        /// <param name="strStyle">检索风格</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1	error;0	not found;1	found</returns>
		public int GetBrowseRecord(string [] fullpaths,
			bool bReverse,
			string strStyle,
			out string strError)
		{
			strError = "";
			int nRet = 0;

			int nIndex = 0;

			ArrayList aTempPath = new ArrayList();
			ArrayList aTempRecord = new ArrayList();
			string strLastUrl = "";
			for(int i=0;i<=fullpaths.Length;i++)
			{
				bool bPush = false;
				ResPath respath = null;

				if (i<fullpaths.Length)
				{
					string strFullPath = fullpaths[i];

					if (bReverse == true)
						strFullPath = ResPath.GetRegularRecordPath(strFullPath);

					respath = new ResPath(strFullPath);

					if ( respath.Url != strLastUrl 
						|| aTempPath.Count >= 5)	// 最大5条为一批。减少用户等待感
						bPush = true;

				}
				else 
				{
					bPush = true;
				}

				if ( bPush == true && aTempPath.Count > 0)
				{
							
					string [] temppaths = new string[aTempPath.Count];
					for(int j=0;j<temppaths.Length;j++)
					{
						temppaths[j] = (string)aTempPath[j];
					}
					nRet = GetBrowseRecord(
						strLastUrl,
						temppaths,
						strStyle,
						out aTempRecord,
						out strError);
					if (nRet == -1)
						return -1;

					// 触发事件
					if (this.BrowseRecord != null)
					{
						
						for(int j=0;j<aTempRecord.Count;j++)
						{
							BrowseRecordEventArgs e = new BrowseRecordEventArgs();
							e.SearchCount = 0;
							e.Index = nIndex ++;
							e.FullPath = strLastUrl + "?" + temppaths[j];
							e.Cols = (string[])aTempRecord[j];
							this.BrowseRecord(this, e);
							if (e.Cancel == true)
							{
								if (e.ErrorInfo == "")
									strError = "用户中断";
								else
									strError = e.ErrorInfo;
								return -2;
							}
						}
					}

					aTempRecord.Clear();

					aTempPath.Clear();
				}


				if (i<fullpaths.Length)
				{
					aTempPath.Add(respath.Path);

					strLastUrl = respath.Url;
				}

			} // end of for

			return 0;

		}

	


        /// <summary>
        ///  获取浏览记录
        /// </summary>
        /// <param name="fullpaths">若干记录全路径</param>
        /// <param name="bReverse">fullpaths中路径是否为反转形式</param>
        /// <param name="strStyle">检索风格</param>
        /// <param name="records">返回记录对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1	error;0	not found;1	found</returns>
		public int GetBrowseRecord(string [] fullpaths,
			bool bReverse,
			string strStyle,
			out ArrayList records,
			out string strError)
		{
			strError = "";
			records = new ArrayList();
			int nRet = 0;


			ArrayList aTempPath = new ArrayList();
			ArrayList aTempRecord = new ArrayList();
			string strLastUrl = "";
			for(int i=0;i<=fullpaths.Length;i++)
			{
				bool bPush = false;
				ResPath respath = null;

				if (i<fullpaths.Length)
				{
					string strFullPath = fullpaths[i];

					if (bReverse == true)
						strFullPath = ResPath.GetRegularRecordPath(strFullPath);

					respath = new ResPath(strFullPath);

					if ( respath.Url != strLastUrl )
						bPush = true;

				}
				else 
				{
					bPush = true;
				}

				if ( bPush == true && aTempPath.Count > 0)
				{
							
					string [] temppaths = new string[aTempPath.Count];
					for(int j=0;j<temppaths.Length;j++)
					{
						temppaths[j] = (string)aTempPath[j];
					}
					nRet = GetBrowseRecord(
						strLastUrl,
						temppaths,
						strStyle,
						out aTempRecord,
						out strError);
					if (nRet == -1)
						return -1;

					records.AddRange(aTempRecord);
					aTempRecord.Clear();

					aTempPath.Clear();
				}


				if (i<fullpaths.Length)
				{
					aTempPath.Add(respath.Path);

					strLastUrl = respath.Url;
				}

			} // end of for

			return 0;

		}

        /// <summary>
        /// 获取浏览记录
        /// </summary>
        /// <param name="strServerUrl"></param>
        /// <param name="paths"></param>
        /// <param name="strStyle"></param>
        /// <param name="records"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
		public int GetBrowseRecord(
			string strServerUrl,
			string [] paths,
			string strStyle,
			out ArrayList records,
			out string strError)
		{
			strError = "";
			records = null;

			if (String.IsNullOrEmpty(strServerUrl) == true)
				strServerUrl = this.ServerUrl;

			RmsChannel channelSave = channel;
			
			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{
				// 根据制定的记录路径获得浏览格式记录
				// parameter:
				//		aRecord	返回的浏览记录信息。一个ArrayList数组。每个元素为一个string[]，所包含的内容
				//				根据strStyle而定。如果strStyle中有id，则aRecord每个元素中的string[]第一个字符串就是id，后面是各列内容。
				return channel.GetBrowseRecords(paths,
					strStyle,
					out records,
					out strError);
			}

			finally 
			{
				channel = channelSave;
			}

		}


        /// <summary>
        /// 获取记录
        /// </summary>
        /// <param name="strServerUrl">服务器URL。如果==null，表示用SearchPannel自己的ServerUrl</param>
        /// <param name="strPath"></param>
        /// <param name="strXml"></param>
        /// <param name="baTimeStamp"></param>
        /// <param name="strError"></param>
        /// <returns>-1	error;0	not found;1	found</returns>
		public int GetRecord(
            string strServerUrl,
            string strPath,
			out string strXml,
			out byte[] baTimeStamp,
			out string strError)
		{
			strError = "";
			baTimeStamp = null;
			strXml = "";

            if (String.IsNullOrEmpty(strServerUrl) == true)
                strServerUrl = this.ServerUrl;

			RmsChannel channelSave = channel;
			
			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{

				// 取记录
				string strStyle = "content,data,timestamp";

				string strMetaData;
				string strOutputPath;


				long lRet = channel.GetRes(strPath,
					strStyle,
					out strXml,
					out strMetaData,
					out baTimeStamp,
					out strOutputPath,
					out strError);
				if (lRet == -1) 
				{
					strError = "获取 '" + strPath + "' 记录体时出错: " + strError;
					if (channel.ErrorCode == ChannelErrorCode.NotFound)
					{
						return 0;
					}

					return -1;
				}


				return 1;
			}

			finally 
			{
				channel = channelSave;
			}

		}


        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="strServerUrl">服务器URL</param>
        /// <param name="strPath"></param>
        /// <param name="strXml"></param>
        /// <param name="baTimestamp"></param>
        /// <param name="bForceSaveOnTimestampMismatch"></param>
        /// <param name="baOutputTimestamp"></param>
        /// <param name="strError"></param>
        /// <returns>-2	时间戳不匹配;-1	一般出错;0	正常</returns>
		public int SaveRecord(
            string strServerUrl,
			string strPath,
			string strXml,
			byte [] baTimestamp,
			bool bForceSaveOnTimestampMismatch,
			out byte [] baOutputTimestamp,
			out string strError)
		{
			strError = "";
			baOutputTimestamp = null;

            if (String.IsNullOrEmpty(strServerUrl) == true)
                strServerUrl = this.ServerUrl;

			RmsChannel channelSave = channel;

			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{
				string strOutputPath = "";

			REDO:

				long lRet = channel.DoSaveTextRes(strPath,
					strXml,
					false,	// bInlucdePreamble
					"",	// style
					baTimestamp,
					out baOutputTimestamp,
					out strOutputPath,
					out strError);
				if (lRet == -1)
				{
					if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
					{
						if (bForceSaveOnTimestampMismatch == true)
						{
							baTimestamp = baOutputTimestamp;
							goto REDO;
						}
						else
							return -2;

					}

					return -1;
				}

				strError = channel.ErrorInfo;	// 特殊API风格

				return 0;
			}
			finally 
			{
				channel = channelSave;
			}
		}


        /// <summary>
        /// 模拟创建检索点
        /// </summary>
        /// <param name="strServerUrl">服务器URL。为什么要单给出这个URL，而不采用this.ServerUrl? 因为要模拟检索点的记录，其服务器URL常常和主服务器的URL不同。</param>
        /// <param name="strPath"></param>
        /// <param name="strXml"></param>
        /// <param name="aLine"></param>
        /// <param name="strError"></param>
        /// <returns>-1	一般出错;0	正常</returns>
		public int GetKeys(
			string strServerUrl,
			string strPath,
			string strXml,
            out List<AccessKeyInfo> aLine,
			out string strError)
		{
			strError = "";
			aLine = null;

            if (String.IsNullOrEmpty(strServerUrl) == true)
                strServerUrl = this.ServerUrl;

			
			RmsChannel channelSave = channel;

			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{
				long lRet = channel.DoGetKeys(
                    strPath,
					strXml,
					"zh",	// strLang
					// "",	// strStyle
					null,	// this.stop,
					out aLine,
					out strError);
				if (lRet == -1)
				{
					return -1;
				}
				return 0;
			}
			finally 
			{
				channel = channelSave;
			}
		}

        /// <summary>
        /// 检索实用库
        /// </summary>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="dom"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
		public int SearchUtilDb(
			string strDbName,
			string strFrom,
			string strKey,
			out XmlDocument dom,
			out string strError)
		{
			strError = "";
			dom = null;

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
			string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 new add
                + "'><item><word>"
				+ StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

			string strPath = "";

			// 检索一个命中结果
			// return:
			//		-1	一般错误
			//		0	not found
			//		1	found
			//		>1	命中多于一条
			int nRet = SearchOnePath(
                null,
				strQueryXml,
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			byte [] baTimeStamp = null;
			// 获取记录
			// return:
			//		-1	error
			//		0	not found
			//		1	found
			nRet = this.GetRecord(
                null,
                strPath,
				out dom,
				out baTimeStamp,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			return 1;
		}

        /// <summary>
        /// 检索实用库
        /// </summary>
        /// <param name="strDbName"></param>
        /// <param name="strFrom"></param>
        /// <param name="strKey"></param>
        /// <param name="strValueAttrName"></param>
        /// <param name="strValue"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
		public int SearchUtilDb(
			string strDbName,
			string strFrom,
			string strKey,
			string strValueAttrName,
			out string strValue,
			out string strError)
		{
			strError = "";
			strValue = "";

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
			string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14 new add
                + "'><item><word>"
				+ StringUtil.GetXmlStringSimple(strKey)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

			string strPath = "";

			// 检索一个命中结果
		// return:
		//		-1	一般错误
		//		0	not found
		//		1	found
		//		>1	命中多于一条
			int nRet = SearchOnePath(
                null,
				strQueryXml,
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			byte [] baTimeStamp = null;
			XmlDocument domRecord = null;
			// 获取记录
			// return:
			//		-1	error
			//		0	not found
			//		1	found
			nRet = this.GetRecord(
                null,
                strPath,
				out domRecord,
				out baTimeStamp,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			strValue = DomUtil.GetAttr(domRecord.DocumentElement, strValueAttrName);

			return 1;
		}



        /// <summary>
        /// 检索并获取浏览结果
        /// </summary>
        /// <param name="strServerUrl">服务器URL。如果==""或者==null，表示用this.ServerUrl</param>
        /// <param name="strQueryXml"></param>
        /// <param name="bGetBrowseCols">是否要获得浏览列</param>
        /// <param name="strError"></param>
        /// <returns>-2	用户中断;-1	一般错误;0	未命中;	>=1	正常结束，返回命中条数</returns>
		public long SearchAndBrowse(
			string strServerUrl,
			string strQueryXml,
            bool bGetBrowseCols,
			out string strError)
		{
			strError = "";
			
			if (strServerUrl == null || strServerUrl == null)
				strServerUrl = this.ServerUrl;

			RmsChannel channelSave = channel;


			channel = Channels.GetChannel(strServerUrl);
			if (channel == null)
			{
				strError = "get channel error";
				return -1;
			}

			try 
			{

				// 检索
				long lRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
				if (lRet == -1) 
					return -1;

				if (lRet == 0) 
					return 0;

				// 循环获取结果
				long nHitCount = lRet;
				long nStart = 0;
				long nCount = 10;
				long nIndex = 0;

				for(;;)
				{
					Application.DoEvents();	// 出让界面控制权

					if (stop != null) 
					{
						if (stop.State != 0)
						{
							strError = "用户中断";
							return -2;
						}
					}

					List<string> aPath = null;
                    ArrayList aLine = null;
                    if (bGetBrowseCols == false)
                    {
                        lRet = channel.DoGetSearchResult(
                    "default",
                            nStart,
                            nCount,
                            "zh",
                            this.stop,
                            out aPath,
                            out strError);
                    }
                    else
                    {
                        lRet = channel.DoGetSearchFullResult(
                    "default",
                            nStart,
                            nCount,
                            "zh",
                            this.stop,
                            out aLine,
                            out strError);
                    }
					if (lRet == -1) 
					{
						strError = "获取检索结果时出错: " + strError;
						return -1;
					}

                    if (bGetBrowseCols == false)
					    nStart += aPath.Count;
                    else
                        nStart += aLine.Count;


					// 触发事件
					if (this.BrowseRecord != null)
					{
                        int nThisCount = 0;

                        if (bGetBrowseCols == false)
                            nThisCount = aPath.Count;
                        else
                            nThisCount = aLine.Count;


                        for (int j = 0; j < nThisCount; j++)
						{
							BrowseRecordEventArgs e = new BrowseRecordEventArgs();
							e.SearchCount = nHitCount;
							e.Index = nIndex ++;
                            if (bGetBrowseCols == false)
                            {
                                e.FullPath = strServerUrl + "?" + (string)aPath[j];
                            }
                            else
                            {
                                string[] cols = (string[])aLine[j];
                                e.FullPath = strServerUrl + "?" + cols[0];
                                // 丢掉第一列
                                e.Cols = new string[Math.Max(cols.Length - 1, 0)];
                                Array.Copy(cols, 1, e.Cols, 0, cols.Length - 1);
                            }
							this.BrowseRecord(this, e);
							if (e.Cancel == true)
							{
								if (e.ErrorInfo == "")
									strError = "用户中断";
								else
									strError = e.ErrorInfo;
								return -2;
							}
						}
					}



					if (nStart >= nHitCount)
						break;

                    // 2006/9/24 add 防止nStart + nCount越界
                    if (nStart + nCount > nHitCount)
                        nCount = nHitCount - nStart;
                    else
                        nCount = 10;

				}


				return nHitCount;
			}
			finally 
			{
				channel = channelSave;
			}

		}



        /// <summary>
        /// 从marcdef配置文件中获得marc格式定义
        /// </summary>
        /// <param name="strDbFullPath"></param>
        /// <param name="strMarcSyntax"></param>
        /// <param name="strError"></param>
        /// <returns>-1	出错;0	没有找到;1	找到</returns>
		public int GetMarcSyntax(string strDbFullPath,
			out string strMarcSyntax,
			out string strError)
		{
			strError = "";
			strMarcSyntax = "";

			ResPath respath = new ResPath(strDbFullPath);

			string strCfgFilePath = respath.Path + "/cfgs/marcdef";

			XmlDocument tempdom = null;
			// 获得配置文件
			// return:
			//		-1	error
			//		0	not found
			//		1	found
			int nRet = this.GetCfgFile(
				respath.Url,
				strCfgFilePath,
				out tempdom,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0) 
			{
				strError = "配置文件 '" + strCfgFilePath + "' 没有找到...";
				return 0;
			}

			XmlNode node = tempdom.DocumentElement.SelectSingleNode("//MARCSyntax");
			if (node == null)
			{
				strError = "marcdef文件 "+strCfgFilePath+" 中没有<MARCSyntax>元素";
				return 0;
			}

			strMarcSyntax = DomUtil.GetNodeText(node);

			strMarcSyntax = strMarcSyntax.ToLower();

			return 1;
		}


	}



    /// <summary>
    /// 浏览记录到达
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
	public delegate void BrowseRecordEventHandler(object sender,
	BrowseRecordEventArgs e);

    /// <summary>
    /// 浏览记录到达事件参数
    /// </summary>
	public class BrowseRecordEventArgs: EventArgs
	{
        /// <summary>
        /// 命中总数
        /// </summary>
		public long SearchCount = 0;	// 

        /// <summary>
        /// 当前记录所在偏移
        /// </summary>
		public long Index = 0;	// 

        /// <summary>
        /// 记录路径，正规全路径，例如 http://dp2003.com/rmsservice/rmsservice.asmx?书目库/1
        /// </summary>
		public string FullPath = "";	// 

        /// <summary>
        /// 浏览各列信息
        /// </summary>
		public string [] Cols = null;	// 

        /// <summary>
        /// 是否需要中断
        /// </summary>
		public bool Cancel = false;	// 

        /// <summary>
        /// 回调期间发生的错误信息
        /// </summary>
		public string ErrorInfo = "";	// 
	}

}
