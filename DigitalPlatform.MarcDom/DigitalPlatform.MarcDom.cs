using System;
using System.Xml;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.RegularExpressions;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace DigitalPlatform.MarcDom
{
	/// <summary>
	/// 把一条MARC记录表示为层次结构.
	/// </summary>
	public class MarcDocument
	{
		#region 常量定义

		public const char FLDEND  = (char)30;	// 字段结束符
		public const char RECEND  = (char)29;	// 记录结束符
		public const char SUBFLD  = (char)31;	// 子字段指示符

		public const int FLDNAME_LEN =        3;       // 字段名长度
		public const int MAX_MARCREC_LEN =    100000;   // MARC记录的最大长度

		#endregion


		string	m_strMarc = "";	// MARC记录体

		public MarcDocument()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public string Marc
		{
			get 
			{
				return m_strMarc;
			}
			set 
			{
				m_strMarc = value;
			}

		}

		void Parse(FilterDocument filter)
		{

		}


		#region 处理MARC记录各种内部结构的静态函数

		// 以字段/子字段名从记录中得到第一个子字段内容。
		// parameters:
		//		strMARC	机内格式MARC记录
		//		strFieldName	字段名。内容为字符
		//		strSubfieldName	子字段名。内容为1字符
		// return:
		//		""	空字符串。表示没有找到指定的字段或子字段。
		//		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
		static public string GetFirstSubfield(string strMARC,
			string strFieldName,
			string strSubfieldName)
		{
			string strField = "";
			string strNextFieldName = "";


		// return:
		//		-1	error
		//		0	not found
		//		1	found
			int nRet = GetField(strMARC,
                strFieldName,
				0,
				out strField,
				out strNextFieldName);

			if (nRet != 1)
				return "";

			string strSubfield = "";
			string strNextSubfieldName = "";

		// return:
		//		-1	error
		//		0	not found
		//		1	found
			nRet = GetSubfield(strField,
				ItemType.Field,
                strSubfieldName,
				0,
                out strSubfield,
                out strNextSubfieldName);
			if (strSubfield.Length < 1)
				return "";

			return strSubfield.Substring(1);
		}

		// 单元测试!

		// 从记录中得到一个字段
		// parameters:
		//		strMARC		机内格式MARC记录
		//		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
		//		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
		//		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
		//					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
		//		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
		// return:
		//		-1	出错
		//		0	所指定的字段没有找到
		//		1	找到。找到的字段返回在strField参数中
		public static int GetField(string strMARC,
			string strFieldName,
			int nIndex,
			out string strField,
			out string strNextFieldName)
		{
//			LPTSTR p;
			int nFoundCount = 0;
			int nChars = 0;
			string strCurFldName;


			strField = null;
			strNextFieldName = null;


			if (strMARC == null) 
			{
				Debug.Assert(false, "strMARC参数不能为null");
				return -1;
			}


			if (strMARC.Length < 24)
				return -1;

			if (strFieldName != null) 
			{
				if (strFieldName.Length != 3) 
				{
					Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
					return -1;
				}
			}
			else 
			{
				// 表示不关心字段名，依靠nIndex来定位字段
			}

			strField = "";

			char ch;

			// 循环，找特定的字段

			// p = (LPTSTR)pszMARC;
			for(int i=0; i<strMARC.Length;) 
			{
				ch = strMARC[i];

				if (ch == RECEND)
					break;

				// 设置m_strItemName
				if (i == 0) 
				{

					if ( (nIndex == 0 && strFieldName == null)	// 头标区
						|| 
						(strFieldName != null 
						&& String.Compare("hdr",strFieldName, true) == 0) // 有字段名要求，并且要求头标区
						)
					{
						strField = strMARC.Substring(0, 24);

						// 取strNextFieldName
						strNextFieldName = GetNextFldName(strMARC,
							24);
						return 1;	// found
					}
					nChars = 24;

					if (strFieldName == null
						|| (strFieldName != null 
						&& "hdr" == strFieldName)
						)
					{
						nFoundCount ++;
					}

				}
				else 
				{
					nChars = DetectFldLens(strMARC, i);
					if (nChars < 3+1) 
					{
						strCurFldName = "???";	// ???
						goto SKIP;
					}
					Debug.Assert(nChars>=3+1, "");
					strCurFldName = strMARC.Substring(i, 3);
					if (strFieldName == null
						|| (strFieldName != null 
						&& strCurFldName == strFieldName)
						)
					{
						if (nIndex == nFoundCount) 
						{
							strField = strMARC.Substring(i, nChars-1);	// 不包含字段结束符

							// 取strNextFieldName
							strNextFieldName = GetNextFldName(strMARC,
								i + nChars);

							/*
							if (i+nChars < strMARC.Length
								&& strMARC[i+nChars] != RECEND
								&& DetectFldLens(strMARC, i+nChars) >= 3 ) 
							{
								strNextFieldName = strMARC.Substring(i+nChars, 3);
								for(int j=0;j<strNextFieldName.Length;j++) 
								{
									char ch0 = strNextFieldName[j];
									if (ch0 == RECEND
										|| ch0 == SUBFLD 
										|| ch0 == FLDEND)
										strNextFieldName = strNextFieldName.Insert(j, "?").Remove(j+1, 1);

								}
							}
							else
								strNextFieldName = "";
							*/

							return 1;	// found
						}
						nFoundCount ++;
					}
				}

			SKIP:
				i += nChars;
			}
			return 0;	// not found
		}

		// nStart需正好为字段名字符位置
		// 本函数为函数GetField()服务
		static string GetNextFldName(string strMARC,
			int nStart)
		{
			string strNextFieldName = "";

			if (nStart < strMARC.Length
				&& strMARC[nStart] != RECEND
				&& DetectFldLens(strMARC, nStart) >= 3 ) 
			{
				strNextFieldName = strMARC.Substring(nStart, 3);
				for(int j=0;j<strNextFieldName.Length;j++) 
				{
					char ch0 = strNextFieldName[j];
					if (ch0 == RECEND
						|| ch0 == SUBFLD 
						|| ch0 == FLDEND)
						strNextFieldName = strNextFieldName.Insert(j, "?").Remove(j+1, 1);

				}
			}
			else
				strNextFieldName = "";

			return strNextFieldName;
		}

		// 根据字段开始位置探测整个字段长度
		// 返回字段的长度
		// 这是包含记录结束符号在内的长度
		public static int DetectFldLens(string strText,
			int nStart)
		{
			Debug.Assert(strText != null, "strText参数不能为null");
			int nChars = 0;
			// LPTSTR p = (LPTSTR)pFldStart;
			for(;nStart<strText.Length;nStart++) 
			{
				if (strText[nStart] == FLDEND) 
				{
					nChars ++;
					break;
				}
				nChars ++;
			}

			return nChars;
		}

		// 根据字段开始位置探测整个字段长度
		// 返回字段的长度(字符数，不是byte数)
		public static int DetectSubFldLens(string strField,
			int nStart)
		{
			Debug.Assert(strField != null, "strField参数不能为null");
			Debug.Assert(strField[nStart] == SUBFLD, "nStart位置必须是一个子字段符号");
			Debug.Assert(nStart < strField.Length, "nStart不能越过strField长度");
			int nChars = 0;
			nStart ++;
			nChars ++;
			for(;nStart < strField.Length; nStart++) 
			{
				if (strField[nStart] == SUBFLD) 
				{
					break;
				}
				nChars ++;
			}

			return nChars;
		}


		// 得到一个嵌套记录中的字段
		// parameters:
		//		strMARC		字段中嵌套的MARC记录。其实，这本是一个MARC字段，其中用$1隔开各个嵌套字段。例如UNIMARC的410字段就是这样。
		//		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
		//		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个不表示头标区了，因为MARC嵌套记录中无法定义头标区)
		//		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
		//					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
		//		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
		// return:
		//		-1	出错
		//		0	所指定的字段没有找到
		//		1	找到。找到的字段返回在strField参数中
		public static int GetNestedField(string strMARC,
			string strFieldName,
			int nIndex,
			out string strField,
			out string strNextFieldName)
		{
			// LPTSTR p;
			int nFoundCount = 0;
			int nChars = 0;
			string strFldName = "";

			strField = "";
			strNextFieldName = "";

			if (strMARC == null) 
			{
				Debug.Assert(false, "strMARC参数不能为null");
				return -1;
			}

			if (strMARC.Length < 5)
				return -1;

			if (strFieldName != null) 
			{
				if (strFieldName.Length != 3) 
				{
					Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
					return -1;
				}
			}
			else 
			{
				// 表示不关心字段名，依靠nIndex来定位字段
			}

			strField = "";

			// 循环，找特定的字段
			//p = (LPTSTR)pszMARC + 5;
			int nStart = 5;

			// 找到第一个'$1'符号
			for(;nStart<strMARC.Length;)
				// *p&&*p!=FLDEND;) 
			{
				if (strMARC[nStart] == FLDEND)
					break;
				if (strMARC[nStart] == SUBFLD
					&& strMARC[nStart+1] == '1' )
					goto FOUND;
				nStart ++;
			}
			return 0;

			FOUND:

				for(int i=nStart;i<strMARC.Length;) 
				{
					if (strMARC[i] == FLDEND)
						break;

					nChars = DetectNestedFldLens(strMARC, i);
					if (nChars < 2+3+1) 
					{
						strFldName = "???";
						goto SKIP;
					}
					Debug.Assert(nChars>=2+3+1, "error");
					strFldName = strMARC.Substring(i + 2, 3);

					if (strFieldName == null
						|| (strFieldName != null
						&& strFldName == strFieldName) )
					{
						if (nIndex == nFoundCount) 
						{
							strField = strMARC.Substring(i + 2, nChars - 2);

							if (i+nChars < strMARC.Length
								&& strMARC[i+nChars] != RECEND
								&& DetectFldLens(strMARC, i+nChars) >= 2+3)
								strNextFieldName = strMARC.Substring(i+nChars+2, 3);
							else
								strNextFieldName = "";

							return 1;	// found
						}
						nFoundCount ++;
					}

				SKIP:
					i += nChars;
				}
			return 0;	// not found
		}

		// 嵌套字段：根据字段开始位置探测整个字段长度
		// 返回字段的长度(字符数，不是byte数)
		static int DetectNestedFldLens(string strMARC, int nStart)
		{
			Debug.Assert(strMARC != null , "strMARC参数不能为null");

			if (nStart >= strMARC.Length) 
			{
				Debug.Assert(false, "nStart参数值超出strMARC内容长度范围");
				return 0;
			}

			if (strMARC[nStart] != SUBFLD || strMARC[nStart+1] != '1') 
			{
				Debug.Assert(false, "必须用$1开头的位置调用本函数");
			}

			int i = nStart + 1;
			for(;i<strMARC.Length;i++)
			{
				if (strMARC[i] == SUBFLD
					&& strMARC[i + 1] == '1')
					break;
			}

			return i-nStart;
		}


		// 从字段中得到子字段组
		// parameters:
		//		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
		//		nIndex	子字段组序号。从0开始计数。
		//		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
		// return:
		//		-1	出错
		//		0	所指定的子字段组没有找到
		//		1	找到。找到的子字段组返回在strGroup参数中
		public static int GetGroup(string strField,
			int nIndex,
			out string strGroup)
		{
			Debug.Assert(strField != null ,"strField参数不能为null");

			Debug.Assert(nIndex>=0, "nIndex参数必须>=0");

			strGroup = "";

			int nLen = strField.Length;
			if (nLen <= 5) 
			{
				return 0;
			}

			// LPCTSTR lpStart,lpStartSave;
			// LPTSTR pp;
			//int l;
			string strZzd = "a";

			// char zzd[3];

			/*
			zzd[0]=SUBFLD;
			zzd[1]='a';
			zzd[2]=0;
			*/
			strZzd = strZzd.Insert(0, new String(SUBFLD, 1));
	
			// lpStart = pszField;

			// 找到起点
			int nStart = 5;
			int nPos;
			for(int i=0;;i++) 
			{
				nPos = strField.IndexOf(strZzd, nStart);
				if (nPos == -1)
					return 0;

				/*
				pp = _tcsstr(lpStart,zzd);
				if (pp==NULL) 
				{
					return 0; // not found
				}
				*/

				if (i>=nIndex) 
				{
					nStart = nPos;
					break;
				}
				nStart = nPos + 1;
				// lpStart = pp + 1;
			}

			//lpStart = pp;
			//lpStartSave = pp;
			//lpStart ++;
			int nStartSave = nStart;
			nStart ++;

			nPos = strField.IndexOf(strZzd, nStart);
			if (nPos == -1) 
			{
				// 后方没有子字段了
				strGroup = strField.Substring(nStartSave);
				return 1;
			}
			else 
			{
				strGroup = strField.Substring(nStartSave, nPos - nStartSave);
				return 1;
			}
			/*
			pp = _tcsstr(lpStart,zzd);
			if (pp == NULL) 
			{	// 后方没有子字段了
				l = _tcslen(lpStartSave);
				MemCpyToString(lpStartSave, l, strGroup);
				return strGroup.GetLength();
			}
			else 
			{
				l = pp - lpStartSave;	// 注意，是字符数
				MemCpyToString(lpStartSave, l, strGroup);
				return strGroup.GetLength();
			}
			*/

			//	ASSERT(0);
			//   return 0;
		}

		// 从字段或子字段组中得到一个子字段
		// parameters:
		//		strText		字段内容，或者子字段组内容。
		//		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
		//		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
		//					形式为'a'这样的。
		//		nIndex			想要获得同名子字段中的第几个。从0开始计算。
		//		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
		//		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
		// return:
		//		-1	出错
		//		0	所指定的子字段没有找到
		//		1	找到。找到的子字段返回在strSubfield参数中
		public static int GetSubfield(string strText,
			ItemType textType,
			string strSubfieldName,
			int nIndex,
			out string strSubfield,
			out string strNextSubfieldName)
		{

			Debug.Assert(textType == ItemType.Field 
				|| textType == ItemType.Group,
				"textType只能为 ItemType.Field/ItemType.Group之一");
			// LPCTSTR p;
			// int nTextLen;
			int nFoundCount = 0;
			int i;

			strSubfield = "";
			strNextSubfieldName = "";

			Debug.Assert(strText != null, "strText参数不能为null");
			//nFieldLen = strText.Length;

			if (textType == ItemType.Field)
			{
				if (strText.Length < 3)
					return -1;	// 字段内容长度不足3字符

				if (strText.Length == 3) 
					return 0;	// 不存在任何子字段内容
			}
			if (textType == ItemType.Group)
			{
				if (strText.Length < 2)
					return -1;	// 字段内容长度不足3字符

				if (strText.Length == 1) 
					return 0;	// 不存在任何子字段内容
			}


			if (strSubfieldName != null) 
			{
				if (strSubfieldName.Length != 1) 
				{
					Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
					return -1;
				}
			}

			// p = pszField + 3;
			// 找到第一个子字段符号
			for(i=0;i<strText.Length;i++) 
			{
				if (strText[i] == SUBFLD)
					goto FOUND;
			}
			return 0;

			FOUND:
				// 匹配
				for(;i<strText.Length;i++) 
				{
					if (strText[i] == SUBFLD) 
					{
						if (i + 1 >= strText.Length)
							return 0;	// not found

						if ( strSubfieldName == null
							|| 
							(strSubfieldName != null
							&& strText[i + 1] == strSubfieldName[0]) 
							)
						{
							if ( nFoundCount == nIndex) 
							{
								int nChars = DetectSubFldLens(strText, i);
								strSubfield = strText.Substring(i+1, nChars-1);

								// 取下一个子字段名
									if (i + nChars < strText.Length
										&& strText[i + nChars] == SUBFLD
										&& DetectFldLens(strText, i+nChars) >= 2)
									{
										strNextSubfieldName = strText.Substring(i+nChars+1, 1);
									}
									else
										strNextSubfieldName = "";

								return 1;
							}

							nFoundCount ++;
						}

					}

				}

			return 0;
		}


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
		public static int ReplaceField(
			ref string strMARC,
			string strFieldName,
			int nIndex,
			string strField)
		{
			int nInsertOffs = 24;
			int nStartOffs = 24;
			int nLen = 0;
			int nChars = 0;
			string strFldName;
			int nFoundCount = 0;
			bool bFound = false;

			if (strMARC.Length < 24)
				return -1;

			Debug.Assert(strFieldName != null, "");

			/*
			if (strFieldName.Length != 3) 
			{
				Debug.Assert(false, "strFieldName参数内容必须为3字符");
				return -1;
			}
			*/
			if (strFieldName != null) 
			{
				if (strFieldName.Length != 3) 
				{
					Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
					return -1;
				}
			}
			else 
			{
				// 表示不关心字段名，依靠nIndex来定位字段
			}

			bool bIsHeader = false;

			if (strFieldName == null || strFieldName == "") 
			{
				if (nIndex == 0)
					bIsHeader = true;
			}
			else 
			{
				if (strFieldName == "hdr")
					bIsHeader = true;
			}

			// 检查strField参数正确性
			if (bIsHeader == true) 
			{
				if (strField == null
					|| strField == "")
				{
					Debug.Assert(false,"头标区内容只能替换，不能删除");
					return -1;
				}

				if (strField.Length != 24) 
				{
					Debug.Assert(false,"strField中用来替换头标区的内容，必须为24字符");
					return -1;
				}
			}


			// 看看给出的字段内容最后是否有字段结束符，如果没有，则追加一个。
			// 头标区内容不作此处理
			if (strField != null && strField.Length > 0
				&& bIsHeader == false) 
			{
				if (strField[strField.Length - 1] != FLDEND)
					strField += FLDEND;
			}

			bool bInsertOffsOK = false;

			// 循环，找特定的字段
			//p = (LPTSTR)(LPCTSTR)strMARC;
			for(int i=0; i<strMARC.Length;) 
			{
				if (strMARC[i] == RECEND)
					break;


				if (i== 0) 
				{
					if ( (nIndex == 0 && strFieldName == null)	// 头标区
						|| 
						(strFieldName != null 
						&& String.Compare("hdr",strFieldName, true) == 0) // 有字段名要求，并且要求头标区
						)
					{
						if (strField == null
							|| strField == "")
						{
							Debug.Assert(false,"头标区内容只能替换，不能删除");
							return -1;
						}

						if (strField.Length != 24) 
						{
							Debug.Assert(false,"strField中用来替换头标区的内容，必须为24字符");
							return -1;
						}

						strMARC = strMARC.Remove(0, 24);	// 删除原来内容

						strMARC = strMARC.Insert(0, strField);	// 插入新的内容

						return 1;	// found
					}

					nChars = 24;
					strFldName = "hdr";


					if (strFieldName == null
						|| (strFieldName != null 
						&& "hdr" == strFieldName)
						)
					{
						nFoundCount ++;
					}

				}
				else 
				{
					nChars = DetectFldLens(strMARC, i);
					if (nChars < 3+1) 
					{
						strFldName = "???";
						goto SKIP;
					}
					Debug.Assert(nChars>=3+1, "探测到字段长度不能小于3+1");
					strFldName = strMARC.Substring(i, 3);
					// MemCpyToString(p, 3, strFldName);
				}

			SKIP:

				if (strFieldName == null
					|| (strFieldName != null 
					&& strFldName == strFieldName)
					)
				{
					if (nIndex == nFoundCount) 
					{
						nStartOffs = i;
						nLen = nChars;
						bFound = true;
						goto FOUND;
					}
					nFoundCount ++;
				}

				// 想办法留存将来要用的插入位置
				if (strFieldName != null && strFieldName != ""
					&& strFldName != "hdr"
					&& bInsertOffsOK == false) 
				{
					if (String.Compare(strFldName,strFieldName, false) > 0) 
					{
						nInsertOffs = Math.Max(i, 24);
						bInsertOffsOK = true;	// 以后不再设置
					}
					else 
					{
						nInsertOffs = Math.Max(i+nChars, 24);
					}
				}

				i += nChars;
			}

			nStartOffs = nInsertOffs;
			nLen = 0;

			if (strField == null || strField.Length == 0)	// 实际为删除要求
				return 0;
			FOUND:
				if (nLen > 0)
					strMARC = strMARC.Remove(nStartOffs, nLen);	// 删除原来内容

			strMARC = strMARC.Insert(nStartOffs, strField);	// 插入新的内容

			if (bFound == true)
				return 1;

			return 0;
		}


		// 替换字段中的子字段。
		// parameters:
		//		strField	[in,out]待替换的字段
		//		strSubfieldName	要替换的子字段的名，内容为1字符。如果==null，表示任意子字段
		//					形式为'a'这样的。
		//		nIndex		要替换的子字段所在序号。如果为-1，将始终为在字段中追加新子字段内容。
		//		strSubfield	要替换成的新子字段。注意，其中第一字符为子字段名，后面为子字段内容
		// return:
		//		-1	出错
		//		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
		//		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
		public static int ReplaceSubfield(
			ref string strField,
			string strSubfieldName,
			int nIndex,
			string strSubfield)
		{
			if (strField.Length <= 1)
				return -1;

			if (strSubfieldName != null) 
			{
				if (strSubfieldName.Length != 1) 
				{
					Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
					return -1;
				}
			}

			if (nIndex < 0)
				goto APPEND;	// 追加新子字段

			int i = 0;
			int nFoundCount = 0;

			// 找到第一个子字段符号
			for(i=0;i<strField.Length;i++) 
			{
				if (strField[i] == SUBFLD)
					goto FOUNDHEAD;
			}
			goto APPEND;
			FOUNDHEAD:
				// 匹配
				for(;i<strField.Length;i++) 
				{
					if (strField[i] == SUBFLD) 
					{
						if (i + 1 >= strField.Length)
							goto APPEND;	// not found

						if ( strSubfieldName == null
							|| 
							(strSubfieldName != null
							&& strField[i + 1] == strSubfieldName[0]) 
							)
						{
							if ( nFoundCount == nIndex) 
							{
								int nChars = DetectSubFldLens(strField, i);

								// 去除原来的内容
								strField = strField.Remove(i, nChars);
								if (strSubfield != null
									&& strSubfield != "") 
								{
									// 插入新内容
									strField = strField.Insert(i, new string(SUBFLD,1) + strSubfield);
								}
								return 1;
							}

							nFoundCount ++;
						}

					}

				} // end



			APPEND:
				strField +=  new string(SUBFLD,1) + strSubfield;

			return 0;	// inserted

		}

		#endregion



	}



}
