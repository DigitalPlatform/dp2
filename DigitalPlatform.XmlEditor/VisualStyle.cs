using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Drawing;


namespace DigitalPlatform.Xml
{
	// 样式配置类
	public class VisualCfg
	{
		public string strXml = "";
		public string m_debugInfo = "";

		public Color transparenceColor = Color.FromArgb (127,128,129);
		public string strBackPicUrl = "";
		public BackPicStyle backPicStyle = BackPicStyle.Fill ;

		// 定义四个list
		public CaseList caseList = new CaseList ();
		public VisualStyleList visualStyleList = new VisualStyleList ();
		public FontList fontList = new FontList ();
		public BorderList borderList = new BorderList ();

		// 初始化list
		public void Initial()
		{
			caseList.cfg = this;
			visualStyleList.cfg = this;

			XmlDocument dom = new XmlDocument ();
			dom.LoadXml (strXml);

			//清空旧值
			caseList.Clear ();
			visualStyleList.Clear ();
			fontList.Clear ();
			borderList.Clear ();

			XmlNode root = dom.DocumentElement ;


			XmlNode nodeGlobal = root.SelectSingleNode ("//global");
			string strColor = DomUtil.GetAttrDiff (nodeGlobal,"transparenceColor");
			if (strColor != null && strColor != "")
			{
				transparenceColor = ColorUtil.String2Color (strColor);
			}
		
			string myBackPicUrl = DomUtil.GetAttrDiff (nodeGlobal,"backPicture");
			if (myBackPicUrl != null 
				&& strColor != "")
			{
				strBackPicUrl = myBackPicUrl;
			}

			string strBackPicStyle = DomUtil.GetAttrDiff (nodeGlobal,"backPicStyle");
			if (strBackPicStyle != null
				&& strBackPicStyle != "")
			{
				backPicStyle = (BackPicStyle)(Enum.Parse (typeof(BackPicStyle),strBackPicStyle));
			}

			borderList.Initial (root);
			fontList.Initial (root);
			visualStyleList.Initial (root);
			caseList.Initial (root);

			visualStyleList.InitailBelongList();  //初始化belongList

			//初始化后parallelNodes成员已没有用了，节省空间
			visualStyleList.parallelNodes = null;
			fontList.parallelNodes = null;
			borderList.parallelNodes = null;
		}

		public VisualStyle GetVisualStyle(string strElement,
			string strLevel,
			string strXpath,
			string strRegion)
		{
			ItemRegion region = ItemRegion.No;
			region =  (ItemRegion)Enum.Parse(typeof(ItemRegion), strRegion,true);

			return caseList.GetVisualStyle(strElement,
				strLevel,
				strXpath,
				region);
		}


		public VisualStyle GetVisualStyle(Item item,
			ItemRegion region)
		{
			return caseList.GetVisualStyle(item.Name,
				Convert.ToString (item.GetLevel()),
				"",
				region);
		}


		// 直接从visualStyleList里面找
		public VisualStyle GetVisualStyle(string strName)
		{
			return visualStyleList.GetVisualStyle(strName);
		}


		public string Xml
		{
			get 
			{
				return "";
			}
			set
			{
				strXml = value;
				Initial();
			}
		}


		//供测试用
		public string ShowList(string strListName)
		{
			string strResult = "";
			if (strListName == "case")
				strResult = caseList.Dump();
			else if (strListName == "visualstyle")
				strResult = visualStyleList.Dump();
			else if (strListName == "font")
				strResult = fontList.Dump();
			else if (strListName == "border")
				strResult = borderList.Dump();
			
			return strResult;
		}

	}


	#region Case组

	// CaseList //////////////////////////////////////////
	public class CaseList : CollectionBase
	{
		public VisualCfg cfg = null;   //VisualCfg指针,用于从它的visualStyleList取值

		public Case this[int index]
		{
			get 
			{
				return (Case)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		public void Add(Case item)
		{
			InnerList.Add(item);
		}

		public void Initial(XmlNode root)
		{
			XmlNodeList nodeList = root.SelectNodes("//case");
			for(int i=0;i<nodeList.Count ;i++)
			{
				Case oneCase = new Case();
				oneCase.container = this;
				oneCase.CreateBy (nodeList[i]);

				this.Add (oneCase );
			}
		}

		// 检查是否该对象是否包含指定的区域
		bool isInRegion(ItemRegion region,Case myCase)
		{
			if ((region & ItemRegion.Frame) == ItemRegion.Frame)
			{
				if ((ItemRegion.Frame & myCase.region ) == ItemRegion.Frame)
					return true;
			}
			if ((region & ItemRegion.Label) == ItemRegion.Label)
			{
				if ((ItemRegion.Label & myCase.region) == ItemRegion.Label)
					return true;
			}

			if ((region & ItemRegion.Text   )  == ItemRegion.Text   )
			{
				if ((ItemRegion.Text  & myCase.region) == ItemRegion.Text   )
					return true;
			}
			if ((region & ItemRegion.Content)  == ItemRegion .Content)
			{
				if ((ItemRegion.Content & myCase.region) == ItemRegion.Content)
					return true;
			}	
			if ((region & ItemRegion.Comment)  == ItemRegion.Comment)
			{
				if ((ItemRegion.Comment & myCase.region) == ItemRegion.Comment)
					return true;
			}
			if ((region & ItemRegion.Attributes  )  == ItemRegion.Attributes  )
			{
				if ((ItemRegion.Attributes   & myCase.region) == ItemRegion.Attributes  )
					return true;
			}
			if ((region & ItemRegion.ExpandAttributes )== ItemRegion.ExpandAttributes)
			{
				if ((ItemRegion.ExpandAttributes & myCase.region) == ItemRegion.ExpandAttributes)
					return true;
			}
			if ((region & ItemRegion.ExpandContent)  == ItemRegion.ExpandContent )
			{
				if ((ItemRegion.ExpandContent & myCase.region) == ItemRegion.ExpandContent)
					return true;
			}			
			if ((region & ItemRegion.BoxTotal  )  == ItemRegion.BoxTotal    )
			{
				if ((ItemRegion.BoxTotal  & myCase.region) == ItemRegion.BoxTotal )
					return true;
			}
			if ((region & ItemRegion.BoxAttributes )  == ItemRegion.BoxAttributes )
			{
				if ((ItemRegion.BoxAttributes & myCase.region) == ItemRegion.BoxAttributes)
					return true;
			}
			if ((region & ItemRegion.BoxContent   )  == ItemRegion.BoxContent )
			{
				if ((ItemRegion.BoxContent & myCase.region) == ItemRegion.BoxContent )
					return true;
			}

			return false;
		}

		// 检索给定的文本与式子是否区域
        // parameters:
        //      strText         文本字符串
        //      strExpression   式子
		bool IsMatch(string strText,
            string strExpression)
		{
			if (strExpression == "" || strExpression == "*")
				return true;
			if (strText == strExpression)
				return true;
			if (StringUtil.IsInList (strText,strExpression) == true)
				return true;
			if (RegexCompare(strText,strExpression) == true)
				return true;

			return false;
		}

		// 比较一个字符串实例是否与正则表达式匹配
		public bool RegexCompare(string strInstance,string strPattern)	
		{
			Regex  r = new Regex(strPattern, RegexOptions.IgnoreCase);
			System.Text.RegularExpressions.Match m = r.Match(strInstance);
			if (m.Success)
				return true;
			else
				return false;
		}

		public VisualStyle GetVisualStyle(string strElement,
			string strLevel,
			string strXpath,
			ItemRegion region)
		{
			for(int i=0;i<this.Count ;i++)
			{
				Case myCase = this[i];
				if (IsMatch(strElement,myCase.strElement) == true &&
					IsMatch(strLevel,myCase.strLevel) == true &&
					strXpath == myCase.strXpath  &&
					isInRegion (region,myCase) == true)
				{
					return myCase.Style ;
				}
			}
			return null;
		}

		public string Dump()
		{
			string strResult = "";
			foreach(Case mycase in this)
			{
				strResult += mycase.Dump ();
			}
			return strResult;
		}

	}

	public class Case 
	{
		public CaseList container = null;
		public string strName = null;

		public string strElement = null;
		public string strLevel = null;
		public string strXpath = null;
		//public string strRegion = null;

		public ItemRegion region = ItemRegion.No ;


		public VisualStyle refStyle = null;   //引用的
		public VisualStyle innerStyle = null; //内部定义的

		public Case ()
		{}

		public VisualStyle Style
		{
			get
			{
				if (refStyle != null)
					return refStyle ;
				if (innerStyle != null)
					return innerStyle;
				return null;
			}
		}

		public ItemRegion SplitStr2Enum(string strList)
		{
			ItemRegion regionResult = ItemRegion.No ;

			string[] aList;
			aList = strList.Split(new char[]{','});

			for(int i=0;i<aList.Length;i++)
			{
				if (String.Compare("frame",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Frame ;
				}
				if (String.Compare("label",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Label  ;
				}
				if (String.Compare("text",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Text   ;
				}
				if (String.Compare("content",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Content  ;
				}
				if (String.Compare("comment",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Comment  ;
				}
				if (String.Compare("attributes",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.Attributes     ;
				}
				if (String.Compare("expandAttributes",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.ExpandAttributes;
				}
				if (String.Compare("expandContent",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.ExpandContent;
				}
				if (String.Compare("boxTotal",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.BoxTotal ;
				}
				if (String.Compare("boxAttributes",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.BoxAttributes  ;
				}
				if (String.Compare("boxContent",aList[i],true) == 0) 
				{
					regionResult |= ItemRegion.BoxContent;
				}
			}

			return regionResult;

		}

		public void  CreateBy(XmlNode node)
		{
			strName = DomUtil.GetAttrDiff (node,"name");

			strElement = DomUtil.GetAttrDiff (node,"element");
			strLevel = DomUtil.GetAttrDiff (node,"level");
			strXpath = DomUtil.GetAttrDiff (node,"xpath");
			
			string strRegion = DomUtil.GetAttrDiff (node,"region");
			region = SplitStr2Enum(strRegion);


			string strStyleName = DomUtil.GetAttrDiff (node,"style");
			if (strStyleName != null)
			{
				refStyle = container.cfg.visualStyleList.GetVisualStyle(strStyleName);
			}

			XmlNode nodeStyle = node.SelectSingleNode ("style");
			//对于包含在Case里面的Style注意要新new，并加到集合里
			if (nodeStyle != null)
			{
				innerStyle = container.cfg.visualStyleList.GetVisualStyle(nodeStyle);
			}
		}

		public string Dump()
		{
			string strInfo = "\r\n";
			strInfo += "strName:"+strName + "\r\n";
			strInfo += "strElement:"+strElement+ "\r\n";
			strInfo += "strLevel:"+strLevel+ "\r\n";
			strInfo += "strXpath:"+strXpath+ "\r\n";
			//strInfo += "strRegion:"+strRegion+ "\r\n";

			if (refStyle != null)
				strInfo += "refStyle:" + refStyle.strName + "\r\n";
			else
				strInfo += "refStyle:null\r\n";

			if (innerStyle != null)
				strInfo += "innerStyle:" + innerStyle.strName + "\r\n";
			else
				strInfo += "innerStyle:null\r\n";


			return strInfo;
		}
	}


	#endregion

	#region VisualStyle组

	//VisualStyleList ///////////////////////////////////
	public class VisualStyleList : CollectionBase
	{
		public VisualCfg cfg = null;   //指针,用于找font对象，或border对象

		//用于找对象
		public ArrayList parallelNodes = new ArrayList ();


		public VisualStyle this[int index]
		{
			get 
			{
				return (VisualStyle)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		public void Add(VisualStyle item)
		{
			InnerList.Add(item);
		}

		public void Initial(XmlNode root)
		{
			XmlNodeList nodeList = root.SelectNodes("//style");
			for(int i=0;i<nodeList.Count ;i++)
			{
				VisualStyle style = new VisualStyle();
				style.container = this;
				style.CreateBy (nodeList[i]);
				this.Add (style);

				if (parallelNodes == null)
					parallelNodes = new ArrayList ();
				parallelNodes.Add (nodeList[i]);
			}
		}

		public VisualStyle GetVisualStyle(string strMyName)
		{
			foreach(VisualStyle style in this)
			{
				if (style.strName == strMyName)
					return style;
			}
			return null;
		}

		public VisualStyle GetVisualStyle(XmlNode nodeStyle)
		{
			for(int i=0;i<parallelNodes .Count ;i++)
			{
				XmlNode node = (XmlNode)parallelNodes[i];
				if (node.Equals (nodeStyle) == true)
					return this[i];
			}
			return null;
		}


		public void InitailBelongList()
		{
			foreach(VisualStyle style in this)
			{
				style.InitailBelongList();
			}
		}

		public string Dump()
		{
			string strInfo = "";
			foreach(VisualStyle visualStyle in this)
			{
				strInfo += visualStyle.Dump ();
			}
			return strInfo;
		}

	}

	public class VisualStyle 
	{
		//public string strDebugInfo = "";  //处理信息

		public VisualStyleList container = null;
		public string strName = null;

		object nTopBlank = null;         //int
		object nBottomBlank = null;      //int
		object nLeftBlank = null;        //int
		object nRightBlank = null;       //int

		object cBackColor = null;       //color
		object cTextColor = null;

		DpFont refFont = null;
		Border refBorder = null;
		DpFont innerFont = null;
		Border innerBorder = null;

		string strBelongName = null;
		ArrayList belongList = null;	// 继承的风格

		public VisualStyle()
		{
		}


		public int TopBlank
		{
			get
			{
				object topBlank = GetValue(ValueStyle.TopBlank);
				if (topBlank != null)
					return (int)topBlank;
				else
					return 0 ;
			}
		}

		public int BottomBlank
		{
			get
			{
				object bottomBlank = GetValue(ValueStyle.BottomBlank );
				if (bottomBlank != null)
					return (int)bottomBlank;
				else
					return 0 ;
			}
		}

		public int LeftBlank
		{
			get
			{
				object leftBlank = GetValue(ValueStyle.LeftBlank );
				if (leftBlank != null)
					return (int)leftBlank;
				else
					return 0;
			}
		}

		public int RightBlank
		{
			get
			{
				object rightBlank = GetValue(ValueStyle.RightBlank );
				if (rightBlank != null)
					return (int)rightBlank;
				else
					return 0;
			}
		}

		public Color BackColor
		{
			get
			{
				object backColor = GetValue(ValueStyle.BackColor);
				if (backColor != null)
					return (Color)backColor;
				else
					return container.cfg .transparenceColor;//Color.Gray ;
			}
		}

		public Color TextColor
		{
			get
			{
				object textColor = GetValue(ValueStyle.TextColor );
				if (textColor != null)
					return (Color)textColor;
				else
					return Color.Red  ;
			}
		}

		public string FontFace
		{
			get
			{
				object strFontFace = GetValue(ValueStyle.FontFace);
				if (strFontFace != null)
					return (string)strFontFace;
				else
					return "Arial";
			}
		}

		public int FontSize
		{
			get
			{
				object fontSize = GetValue(ValueStyle.FontSize);
				if (fontSize != null)
					return (int)fontSize;
				else
					return 10;
			}
		}

		public FontStyle FontStyle
		{
			get
			{
				object fontStyle = GetValue(ValueStyle.FontStyle);
				if (fontStyle != null)
					return (FontStyle)fontStyle;
				else 
					return FontStyle.Regular;
			}
		}

		public Font Font
		{
			get
			{
				//先用两个属性找
				return (new Font (FontFace,FontSize,FontStyle));
			}
		}

		public int TopBorderHeight
		{
			get
			{
				object vertWidth = GetValue(ValueStyle.TopBorderHeight);
				if (vertWidth != null)
					return (int)vertWidth;
				else
					return 0;
			}
		}

		public int BottomBorderHeight
		{
			get
			{
				object bottomHeight = GetValue(ValueStyle.BottomBorderHeight);
				if (bottomHeight != null)
					return (int)bottomHeight;
				else
					return 0;
			}
		}

		public int LeftBorderWidth
		{
			get
			{
				object leftWidth = GetValue(ValueStyle.LeftBorderWidth);
				if (leftWidth != null)
					return (int)leftWidth;
				else
					return 0;
			}
		}

		public int RightBorderWidth
		{
			get
			{
				object rightWidth = GetValue(ValueStyle.RightBorderWidth);
				if (rightWidth != null)
					return (int)rightWidth;
				else
					return 0;
			}
		}

		public Color BorderColor
		{
			get
			{
				object color = GetValue(ValueStyle.BorderColor);
				if (color != null)
					return (Color)color;
				else
					return Color.LightGray; // Gray
			}
		}


		public object GetValue(ValueStyle valueStyle)
		{
			//strDebugInfo = ""; //先清空一下，否则会越积越多
			object oResult = null;

			if (valueStyle == ValueStyle.TopBlank )
			{
				oResult = nTopBlank;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的nTopBlank有具体的值为"+Convert.ToString (nTopBlank)+"\r\n" ;
					goto END1;
				}
				//strDebugInfo += this.strName  +"的nTopBlank为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.BottomBlank )
			{
				oResult = nBottomBlank;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的nBottomBlank有具体的值为"+Convert.ToString (nBottomBlank)+"\r\n";
					goto END1;
				}
				//strDebugInfo += this.strName  +"的nBottomBlank为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.LeftBlank )
			{
				oResult = nLeftBlank;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的nLeftBlank有具体的值为"+Convert.ToString (nLeftBlank)+"\r\n";
					goto END1;
				}
				//strDebugInfo += this.strName  +"的nLeftBlank为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.RightBlank )
			{
				oResult = nRightBlank;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的nRightBlank有具体的值为"+Convert.ToString (nRightBlank)+"\r\n";
					goto END1;
				}
				//strDebugInfo += this.strName  +"的nRightBlank为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.BackColor )
			{
				oResult = this.cBackColor;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的cBackColor有具体的值为"+ColorUtil.Color2String((Color)oResult)+"\r\n";
					goto END1;
				}
				//strDebugInfo += this.strName  +"的cBackColor为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.TextColor )
			{
				oResult = this.cTextColor ;
				if (oResult != null)
				{
					//strDebugInfo += this.strName  +"的cTextColor有具体的值为"+ColorUtil.Color2String ((Color)oResult)+"\r\n";
					goto END1;
				}
				//strDebugInfo += this.strName  +"的cTextColor为null,从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.FontFace)
			{
				if (refFont != null)
				{
					//strDebugInfo += this.strName +"的refFont有具体的值:"+ refFont.Dump () + "\r\n";
					oResult = refFont.strFace ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的strFace有具体的值为"+refFont.strFace + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的strFace为null，所以继续找\r\n";
				}
				
				if (innerFont != null)
				{
					//strDebugInfo += this.strName +"的innerFont有具体的值:"+innerFont.Dump () + "\r\n";
					oResult = innerFont.strFace ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的strFace有具体的值为"+innerFont.strFace + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的strFace为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refFont和innerFont都没找到strFace值，从belongList里找\r\n";
			}
			else  if (valueStyle == ValueStyle.FontSize)
			{
				if (refFont != null)
				{
					//strDebugInfo += this.strName +"的refFont有具体的值:"+refFont.Dump () + "\r\n";

					oResult = refFont.nSize  ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nSize有具体的值为"+refFont.nSize + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nSize为null，所以继续找\r\n";
				}
				
				if (innerFont != null)
				{
					//strDebugInfo += this.strName +"的innerFont有具体的值:"+innerFont.Dump () + "\r\n";

					oResult = innerFont.nSize ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nSize有具体的值为"+innerFont.nSize + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nSize为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refFont和innerFont都没找到nSize值，从belongList里找\r\n";
			}
			else  if (valueStyle == ValueStyle.FontStyle )
			{
				if (refFont != null)
				{
					//strDebugInfo += this.strName +"的refFont有具体的值:"+refFont.Dump () + "\r\n";

					oResult = refFont.style;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nSize有具体的值为"+refFont.nSize + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nSize为null，所以继续找\r\n";
				}
				
				if (innerFont != null)
				{
					//strDebugInfo += this.strName +"的innerFont有具体的值:"+innerFont.Dump () + "\r\n";

					oResult = innerFont.style  ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nSize有具体的值为"+innerFont.nSize + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nSize为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refFont和innerFont都没找到nSize值，从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.LeftBorderWidth)
			{
				if (refBorder != null)
				{
					//strDebugInfo += this.strName +"的refBorder有具体的值:"+refBorder.Dump () + "\r\n";
					oResult = refBorder.nLeftWidth;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nVertWidth有具体的值为"+refBorder.nVertWidth + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nVertWidth为null，所以继续找\r\n";
				}
				
				if (innerBorder != null)
				{
					//strDebugInfo += this.strName +"的innerBorder有具体的值:"+innerBorder.Dump () + "\r\n";
					oResult = innerBorder.nLeftWidth;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nVertWidth有具体的值为"+innerBorder.nVertWidth + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nVertWidth为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refBorder和innerBorder都没找到nVertWidth值，从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.RightBorderWidth)
			{
				if (refBorder != null)
				{
					//strDebugInfo += this.strName +"的refBorder有具体的值:"+refBorder.Dump () + "\r\n";
					oResult = refBorder.nRightWidth;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+refBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				
				if (innerBorder != null)
				{
					//strDebugInfo += this.strName +"的innerBorder有具体的值:"+innerBorder.Dump () + "\r\n";
					oResult = innerBorder.nRightWidth;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+innerBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refBorder和innerBorder都没找到nHorzHeight值，从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.TopBorderHeight)
			{
				if (refBorder != null)
				{
					//strDebugInfo += this.strName +"的refBorder有具体的值:"+refBorder.Dump () + "\r\n";
					oResult = refBorder.nTopHeight;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+refBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				
				if (innerBorder != null)
				{
					//strDebugInfo += this.strName +"的innerBorder有具体的值:"+innerBorder.Dump () + "\r\n";
					oResult = innerBorder.nTopHeight;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+innerBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refBorder和innerBorder都没找到nHorzHeight值，从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.BottomBorderHeight)
			{
				if (refBorder != null)
				{
					//strDebugInfo += this.strName +"的refBorder有具体的值:"+refBorder.Dump () + "\r\n";
					oResult = refBorder.nBottomHeight;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+refBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				
				if (innerBorder != null)
				{
					//strDebugInfo += this.strName +"的innerBorder有具体的值:"+innerBorder.Dump () + "\r\n";
					oResult = innerBorder.nBottomHeight;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的nHorzHeight有具体的值为"+innerBorder.nHorzHeight + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的nHorzHeight为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refBorder和innerBorder都没找到nHorzHeight值，从belongList里找\r\n";
			}
			else if (valueStyle == ValueStyle.BorderColor )
			{
				if (refBorder != null)
				{
					//strDebugInfo += this.strName +"的refBorder有具体的值:"+refBorder.Dump () + "\r\n";
					oResult = refBorder.color ;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的color有具体的值为"+ColorUtil.Color2String ((Color)refBorder.color) + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的color为null，所以继续找\r\n";
				}
				
				if (innerBorder != null)
				{
					//strDebugInfo += this.strName +"的innerBorder有具体的值:" + innerBorder.Dump () + "\r\n";
					oResult = innerBorder.color;
					if (oResult != null)
					{
						//strDebugInfo += "而且其中的color有具体的值为" + ColorUtil.Color2String((Color)innerBorder.color) + "\r\n";
						goto END1;
					}
					//strDebugInfo += "但其中的color为null，所以继续找\r\n";
				}
				//strDebugInfo += this.strName +"的refBorder和innerBorder都没找到color值，从belongList里找\r\n";
			}

			oResult = GetValueFormBList(valueStyle);
		
			END1:
				//container.m_strDebugInfo += strDebugInfo;
				//FileUtil.WriteText ("L:debug.txt",strDebugInfo);
			return oResult;
		}


		// 为GetValue私有的函数
		object GetValueFormBList(ValueStyle valueStyle)
		{
			if (belongList == null)
			{
				//strDebugInfo += this.strName  +"的belongList为null\r\n";
				return null;
			}

			object oResult = null;
			for(int i=0;i<belongList.Count ;i++)
			{
				VisualStyle style = (VisualStyle)belongList[i];
				if (valueStyle == ValueStyle.TopBlank )
				{
					if (style.nTopBlank != null)
					{
						oResult = style.nTopBlank ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到nTopBlank了,值为"+Convert.ToString (oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的nTopBlank为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.BottomBlank )
				{
					if (style.nBottomBlank != null)
					{
						oResult = style.nBottomBlank ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到nBottomBlank了,值为"+Convert.ToString (oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的nBottomBlank为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.LeftBlank )
				{
					if (style.nLeftBlank != null)
					{
						oResult = style.nLeftBlank ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到nLeftBlank了,值为"+Convert.ToString (oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的nLeftBlank为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.RightBlank )
				{
					if (style.nRightBlank != null)
					{
						oResult = style.nRightBlank ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到nRightBlank了,值为"+Convert.ToString (oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的nRightBlank为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.BackColor )
				{
					if (style.cBackColor != null)
					{
						oResult = style.cBackColor ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到cBackColor了,值为"+ColorUtil.Color2String ((Color)oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的cBackColor为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.TextColor )
				{
					if (style.cTextColor != null)
					{
						oResult = style.cTextColor ; 
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到cTextColor了,值为"+ ColorUtil.Color2String ((Color)oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的cTextColor为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.FontFace )
				{
					oResult = style.FontFace ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到FontFace了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的FontFace为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.FontSize)
				{
					oResult = style.FontSize ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到FontSize了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的FontSize为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.FontStyle )
				{
					oResult = style.FontStyle  ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到FontSize了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的FontSize为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.TopBorderHeight)
				{
					oResult = style.TopBorderHeight ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到BorderVertWidth了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的BorderVertWidth为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.BottomBorderHeight)
				{
					oResult = style.BottomBorderHeight;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到BorderHorzHeight了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的BorderHorzHeight为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.LeftBorderWidth)
				{
					oResult = style.LeftBorderWidth  ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到BorderHorzHeight了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的BorderHorzHeight为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.RightBorderWidth)
				{
					oResult = style.RightBorderWidth;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到BorderHorzHeight了,值为"+oResult+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的BorderHorzHeight为null,继续从belongList里找\r\n";
				}
				else if (valueStyle == ValueStyle.BorderColor )
				{
					oResult = style.BorderColor    ;
					if (oResult != null)
					{
						//strDebugInfo += "    " + Convert.ToString (i) +": 从"+style.strName +"里找到BorderColor了,值为" + ColorUtil.Color2String ((Color)oResult)+"\r\n";
						break;
					}
					//strDebugInfo += "    " + Convert.ToString (i) +": "+style.strName +"的BorderColor为null,继续从belongList里找\r\n";
				}
			}

			return oResult;
		}


		// 根据配置节点创建本对象
		public void CreateBy(XmlNode node)
		{
			strName = DomUtil.GetAttrDiff (node,"name");

			string strTopBlank = DomUtil.GetAttrDiff (node,"topblank");
			if (strTopBlank != null)
				nTopBlank = ConvertUtil.S2Int32(strTopBlank);

			string strBottomBlank = DomUtil.GetAttrDiff (node,"bottomblank");
			if (strBottomBlank != null)
				nBottomBlank = ConvertUtil.S2Int32(strBottomBlank);

			string strLeftBlank = DomUtil.GetAttrDiff (node,"leftblank");
			if (strLeftBlank != null)
				nLeftBlank = ConvertUtil.S2Int32(strLeftBlank);

			string strRightBlank = DomUtil.GetAttrDiff (node,"rightblank");
			if (strRightBlank != null)
				nRightBlank = ConvertUtil.S2Int32(strRightBlank);

			string strBackColor = DomUtil.GetAttrDiff (node,"backcolor");
			if (strBackColor != null)  
			{
				this.cBackColor = ColorUtil.String2Color(strBackColor);
			}

			string strTextColor = DomUtil.GetAttrDiff (node,"textcolor");
			if (strTextColor != null)  
			{
				this.cTextColor  = ColorUtil.String2Color(strTextColor);
			}

			string strFontName = DomUtil.GetAttrDiff (node,"font");
			if (strFontName != null)
			{
				refFont = container.cfg.fontList.GetFont(strFontName);
			}

			string strBorderName = DomUtil.GetAttrDiff (node,"border");
			if (strBorderName != null)
			{
				refBorder = container.cfg.borderList.GetBorder(strBorderName);
			}

			//记住继承的类名
			strBelongName = DomUtil.GetAttrDiff (node,"belong");

			XmlNode nodeFont = node.SelectSingleNode("font");
			if (nodeFont != null)
			{
				innerFont = container.cfg.fontList.GetFont(nodeFont);
			}

			XmlNode nodeBorder = node.SelectSingleNode("border");
			if (nodeBorder != null)
			{
				innerBorder = container.cfg.borderList.GetBorder(nodeBorder);
			}
		}


		// 将一个style加到belongList里，先检查数组里有没有？
		void Add2BelongList(VisualStyle style)
		{
			if (belongList == null)
				belongList = new ArrayList ();

			for(int i=0;i<belongList.Count ;i++)
			{
				VisualStyle tempStyle = (VisualStyle)belongList[i];
				if (tempStyle.Equals (style) == true)
				{
					throw(new Exception ("belong集合里已存在这个style了,循环继承了"));
				}
			}
			belongList.Add (style);
		}

		// 初始化belongList
		public void InitailBelongList()
		{
			string strName = strBelongName;
			while(true)
			{
				if (strName == null)
					break;

				VisualStyle style = container.GetVisualStyle (strName);
				if (style == null)
					break;

				if (this.Equals (style) == true)
				{
					throw(new Exception ("循环继承了"));
				}
				Add2BelongList(style);
				strName = style.strBelongName;
			}
		}

		public string Dump()
		{
			string strInfo = "\r\n";
			strInfo += "strName:"+strName + "\r\n";

			strInfo += "\r\n";
			strInfo += "TopBlank:"+ TopBlank + "\r\n";
			strInfo += "BottomBlank:"+ BottomBlank+ "\r\n";
			strInfo += "LeftBlank:"+LeftBlank+ "\r\n";
			strInfo += "RightBlank:"+RightBlank+ "\r\n";

			strInfo += "\r\n";
			strInfo += "BackColor:"+ ColorUtil.Color2String(BackColor) + "\r\n";
			strInfo += "TextColor:"+ ColorUtil.Color2String(TextColor) + "\r\n";

			strInfo += "\r\n";
			strInfo += "FontFace:"+FontFace + "\r\n";
			strInfo += "FontSize:"+FontSize + "\r\n";

			strInfo += "\r\n";
			strInfo += "TopBorderHeight:"+ this.TopBorderHeight + "\r\n";
			strInfo += "BottomBorderHeight:"+ this.BottomBorderHeight + "\r\n";
			strInfo += "LeftBorderWidth:"+ this.LeftBorderWidth + "\r\n";
			strInfo += "RightBorderWidth:"+ this.RightBorderWidth + "\r\n";
			strInfo += "BorderColor:" + ColorUtil.Color2String(BorderColor)+ "\r\n";

			return strInfo;
		}
	}


	#endregion

	#region Border组

	// BorderList
	public class BorderList : CollectionBase
	{
		public ArrayList parallelNodes = new ArrayList ();

		public Border this[int index]
		{
			get 
			{
				return (Border)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		public void Add(Border item)
		{
			InnerList.Add(item);
		}

		public void Initial(XmlNode root)
		{
			XmlNodeList nodeList = root.SelectNodes("//border");
			for(int i=0;i<nodeList.Count ;i++)
			{
				Border border = new Border();
				border.CreateBy (nodeList[i]);

				this.Add (border);

				//加到平行数组
				if (parallelNodes == null)
					parallelNodes = new ArrayList ();
				parallelNodes.Add (nodeList[i]);
			}
		}

		public Border GetBorder (string strBorderName)
		{
			foreach(Border border in this)
			{
				if (border.strName == strBorderName)
					return border;
			}
			return null;
		}

		public Border GetBorder (XmlNode nodeBorder)
		{
			for(int i=0;i<parallelNodes.Count ;i++)
			{
				XmlNode node = (XmlNode)parallelNodes[i];
				if (node.Equals (nodeBorder) == true)
					return this[i];
			}
			return null;
		}

		public string Dump()
		{
			string strInfo = "";
			foreach(Border border in this)
			{
				strInfo += border.Dump ();
			}
			return strInfo;
		}

	}

	public class Border
	{
		public string strName = null;

		//public object nHorzHeight = null;  //int
		//public object nVertWidth = null;   //int

		public object nLeftWidth = null; //int
		public object nRightWidth = null;  //int
		public object nTopHeight = null; //int
		public object nBottomHeight = null; //int

		public object color = null ;   //Color

		public Border ()
		{
		}

		//根据Node创建border
		public void CreateBy(XmlNode node)
		{
			strName = DomUtil.GetAttrDiff (node,"name");

/*
			string strHorzHeight = DomUtil.GetAttrDiff(node,"horzheight");
			if (strHorzHeight != null)
				nHorzHeight = ConvertUtil.S2Int32(strHorzHeight);

			string strVertWidth = DomUtil.GetAttrDiff (node,"vertwidth");
			if (strVertWidth != null)
				nVertWidth = ConvertUtil.S2Int32(strVertWidth);
*/

			string strLeftWidth = DomUtil.GetAttrDiff(node,"leftwidth");
			if (strLeftWidth != null)
				this.nLeftWidth = ConvertUtil.S2Int32(strLeftWidth);

			string strRightWidth = DomUtil.GetAttrDiff(node,"rightwidth");
			if (strRightWidth != null)
				this.nRightWidth = ConvertUtil.S2Int32(strRightWidth);

			string strTopHeight = DomUtil.GetAttrDiff(node,"topheight");
			if (strTopHeight != null)
				this.nTopHeight = ConvertUtil.S2Int32(strTopHeight);

			string strBottomHeight = DomUtil.GetAttrDiff(node,"bottomheight");
			if (strBottomHeight != null)
				this.nBottomHeight = ConvertUtil.S2Int32(strBottomHeight);

			string strColor = DomUtil.GetAttrDiff (node,"color");
			if (strColor != null)
			{
				color = ColorUtil.String2Color(strColor);
			}
		}

		public string Dump()
		{
			string strInfo = "\r\n";
			strInfo += "strName:"+strName + "\r\n";
			strInfo += "nTopHeight:"+ this.nTopHeight + "\r\n";
			strInfo += "nBottomHeight:"+ this.nTopHeight + "\r\n";
			strInfo += "nLeftWidth:"+ this.nLeftWidth + "\r\n";
			strInfo += "nRightWidth:"+ this.nRightWidth + "\r\n";
			strInfo += "color:" + ColorUtil.Color2String((Color)color) + "\r\n";

			return strInfo;
		}

	}

	#endregion

	#region Font组
	
    // FontList
	public class FontList : CollectionBase
	{
		public ArrayList parallelNodes = new ArrayList ();

		public DpFont this[int index]
		{
			get 
			{
				return (DpFont)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		public void Add(DpFont item)
		{
			InnerList.Add(item);
		}

		public void Initial(XmlNode root)
		{
			XmlNodeList nodeList = root.SelectNodes("//font");
			for(int i=0;i<nodeList.Count ;i++)
			{
				DpFont font = new DpFont();
				font.CreateBy (nodeList[i]);

				this.Add (font);

				if (parallelNodes == null)
					parallelNodes = new ArrayList ();
				parallelNodes.Add (nodeList[i]);
			}
		}
		public DpFont GetFont (string strFontName)
		{
			foreach(DpFont font in this)
			{
				if (font.strName == strFontName)
					return font;
			}
			return null;
		}

		public DpFont GetFont (XmlNode nodeFont)
		{
			for(int i=0;i<parallelNodes .Count ;i++)
			{
				XmlNode node = (XmlNode)parallelNodes[i];
				if (node.Equals (nodeFont) == true)
					return this[i];
			}
			return null;
		}

		public string Dump()
		{
			string strInfo = "";
			foreach(DpFont font in this)
			{
				strInfo += font.Dump ();
			}
			return strInfo;
		}
	}

	public class DpFont   //避免和System.Drawing.Font引用不必要的麻烦
	{
		public string strName = null;

		public string strFace = null;
		public object nSize = null;
		public object style = null;

		public DpFont()
		{}

		public void CreateBy(XmlNode node)
		{
			strName = DomUtil.GetAttrDiff (node,"name");
			strFace = DomUtil.GetAttrDiff (node,"face");
			string strStyle = DomUtil.GetAttrDiff (node,"style");
			if (strStyle != null)
			{
				if (strStyle == "")
					style = (FontStyle)0;
				else
					style = (FontStyle)Enum.Parse(typeof(FontStyle), strStyle,true);
			}
			string strSize = DomUtil.GetAttrDiff (node,"size");
			if (strSize != null)
				nSize = ConvertUtil.S2Int32(strSize);
		}

		public string Dump()
		{
			string strInfo = "\r\n";
			strInfo += "strName:"+strName + "\r\n";
			strInfo += "face:"+ strFace + "\r\n";
			strInfo += "size:"+ nSize + "\r\n";
			return strInfo;
		}
	}

	#endregion

}
