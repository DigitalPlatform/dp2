using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Text;

using DigitalPlatform;

namespace DigitalPlatform.IO
{
	// 临时文件
	public class TempFileItem 
	{
		public Stream m_stream;
		public string m_strFileName;
	}

#if NO
	// 临时文件容器
	public class TempFileCollection : ArrayList
	{
		public TempFileCollection() 
		{
		}

        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
		~TempFileCollection() 
		{
			Clear();
		}

		public new void Clear() 
		{
			int l;
			for(l=0; l<this.Count; l++) 
			{

				TempFileItem item = (TempFileItem)this[l];
				if (item.m_stream != null) 
				{
					item.m_stream.Close();
					item.m_stream = null;
				}

				try 
				{
					File.Delete(item.m_strFileName);
				}
				catch
				{
				}

			}

			base.Clear();
		}
	}
#endif

	public delegate bool FlushOutput();
	public delegate bool ProgressOutput(long lCur);

	// 在对照表中宏不存在
	public class MacroNotFoundException : Exception
	{

		public MacroNotFoundException (string s) : base(s)
		{
		}

	}

	// 宏名格式错
	public class MacroNameException : Exception
	{

		public MacroNameException (string s) : base(s)
		{
		}

	}

}
