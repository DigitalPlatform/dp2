using System;

using DigitalPlatform.Library;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;

namespace dp2Batch
{
	/// <summary>
	/// Summary description for Batch.
	/// </summary>
	public class Batch
	{
		public ApplicationInfo ap = null;
		public MainForm MainForm = null;	// 引用

        public SearchPanel SearchPanel = null;

		public string ServerUrl = "";

		private string	m_strXmlRecord = "";
		private bool	m_bXmlRecordChanged = false;	// XML记录是否被脚本改变


		public string	MarcSyntax = "";

		private string	m_strMarcRecord = "";	// MARC记录体。外部接口在MarcRecord {get;set}
		private bool	m_bMarcRecordChanged = false;	// MARC记录是否被脚本改变

		/*
		private string	m_strMarcOutputRecord = "";	// 用于输出的MARC记录体。外部接口在MarcOutputRecord {get;set}
		private bool	m_bMarcOutputRecordChanged = false;	// 用于输出的MARC记录是否被脚本改变
		*/


		public byte[] TimeStamp = null;	// 时间戳
		public string RecPath = "";	// 记录路径
		public int RecIndex = 0;	// 当前记录在一批中的序号。从0开始计数
		public string ProjectDir = "";
		public string DbPath = "";	// 数据库路径

		public bool SkipInput = false;

		public Batch()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public virtual void OnInitial(object sender, BatchEventArgs e)
		{

		}

		public virtual void OnBegin(object sender, BatchEventArgs e)
		{

		}

		public virtual void Outputing(object sender, BatchEventArgs e)
		{

		}

		public virtual void Inputing(object sender, BatchEventArgs e)
		{

		}

		public virtual void Inputed(object sender, BatchEventArgs e)
		{

		}

		public virtual void OnEnd(object sender, BatchEventArgs e)
		{

		}

		public virtual void OnPrint(object sender, BatchEventArgs e)
		{

		}


		// MARC记录体
		public string MarcRecord 
		{
			get 
			{
				return m_strMarcRecord;
			}
			set 
			{
				m_strMarcRecord = value;
				m_bMarcRecordChanged = true;
			}
		}

		// MARC记录体是否被改变过
		public bool MarcRecordChanged
		{
			get 
			{
				return m_bMarcRecordChanged;
			}
			set 
			{
				m_bMarcRecordChanged = value;
			}
		}

		// XML记录体
		public string XmlRecord 
		{
			get 
			{
				return m_strXmlRecord;
			}
			set 
			{
				m_strXmlRecord = value;
				m_bXmlRecordChanged = true;
			}
		}

		// Xml记录体是否被改变过
		public bool XmlRecordChanged
		{
			get 
			{
				return m_bXmlRecordChanged;
			}
			set 
			{
				m_bXmlRecordChanged = value;
			}
		}

		/*
		// 用于输出的MARC记录体
		public string MarcOutputRecord 
		{
			get 
			{
				return m_strMarcOutputRecord;
			}
			set 
			{
				m_strMarcOutputRecord = value;
				m_bMarcOutputRecordChanged = true;
			}
		}

		// 用于输出的MARC记录体是否被改变过
		public bool MarcOutputRecordChanged
		{
			get 
			{
				return m_bMarcOutputRecordChanged;
			}
			set 
			{
				m_bMarcOutputRecordChanged = value;
			}
		}
		*/

		public string RecFullPath
		{
			get 
			{
				return this.ServerUrl + "?" + this.RecPath;
			}
			set 
			{
				ResPath respath = new ResPath(value);

				this.ServerUrl = respath.Url;
				this.RecPath = respath.Path;
			}
		}

	}

	public enum ContinueType
	{
		Yes = 0,
		/*
		SkipBegin = 1,
		SkipMiddle = 2,
		SkipBeginMiddle = 3,
		*/
		SkipAll = 4,
	}

	public class BatchEventArgs : EventArgs
	{
		public ContinueType	Continue = ContinueType.Yes;	// 是否继续循环
	}

}
