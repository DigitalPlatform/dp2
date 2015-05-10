using System;

namespace DigitalPlatform.Xml
{
	// 背景图片样式
	public enum BackPicStyle
	{
		Fill = 0,
		Center = 1,
		Tile,
	}

	// 移动方向
	public enum MoveMember
	{
		Front = 0,
		Behind = 1,
		Auto = 2,
	}

	// style属性枚举
	public enum ValueStyle
	{
		TopBlank = 0,	
		BottomBlank = 1,	
			
		LeftBlank = 2,
		RightBlank = 3,
			
		BackColor = 4,
		TextColor = 5,

		FontFace = 6,
		FontSize = 7,
		FontStyle = 8,

		//BorderVertWidth = 9,
		//BorderHorzHeight = 10,
		TopBorderHeight = 9,
		BottomBorderHeight = 10,
		LeftBorderWidth = 11,
		RightBorderWidth = 12,
		BorderColor = 13,
	}


	// 区域枚举
	public enum ItemRegion
	{
		Frame = 0x1,

		Label = 0x2,
		Text = 0x4,
		Comment = 0x8,

		Content = 0x10,
		Attributes = 0x20,

		ExpandAttributes = 0x40,
		ExpandContent = 0x80,
		
		BoxTotal = 0x100,
		BoxAttributes = 0x200,
		BoxContent = 0x400,

		No = 0x0,
	}


	// 绘制成员枚举
	public enum PaintMember
	{
		Border = 0,
		Content = 1,
		Both = 2,
	};

	// 卷滚条枚举
	public enum ScrollBarMember 
	{
		Vert = 0,
		Horz = 1,
		Both = 2,
	};


	// 布局风格枚举
	public enum LayoutStyle
	{
		Vertical = 0,
		Horizontal = 1,
	};

	// 展开按钮风格枚举
	public enum ExpandIconStyle
	{
		Plus  = 0,
		Minus  = 1,
	};

	// Layout值枚举
	public enum LayoutMember  
	{
		CalcuWidth = 0x1,	// 评估宽度。函数应当用比较节省资源的方式尽快算出宽度值返回
		//CalcuBoth = 0x4,

		CalcuHeight = 0x2,
		Layout = 0x4,		// 正式布局。按照给定的宽度，初始化各种内部尺寸参数

		Up = 0x8,

		EnLargeHeight = 0x10,
		EnlargeWidth = 0x20,

		None = 0x0,
	
	}

    public class ElementInitialStyle
    {
        public ExpandStyle attrsExpandStyle = ExpandStyle.None;
        public ExpandStyle childrenExpandStyle = ExpandStyle.None;

        public bool bReinitial = false;	// 是否是: 在内存对象已经存在的基础上重新初始化某些状态。如果==false，表示为首次初始化
    }

    public enum ExpandStyle
    {
        None = 0,
        Expand = 1,
        Collapse = 2,
    }
}
