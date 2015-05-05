using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace DigitalPlatform.IO
{

	// 数据事项
	public class Item : IComparable
	{
//		public int DataLength = 0;

		public virtual int Length
		{
			get 
			{
				return 0;
			}
			set 
			{
			}
		}

		public virtual void ReadData(Stream stream)
		{
			// 读入Length个bytes的内容

		}


		public virtual void ReadCompareData(Stream stream)
		{
			// 读入Length个bytes的内容

		}

		public virtual void WriteData(Stream stream)
		{
			// 写入Length个bytes的内容

		}

        public virtual void BuildBuffer()
        {
            // 构造m_buffer，准备Length值

        }

		// 实现IComparable接口的CompareTo()方法,
		// 根据ID比较两个对象的大小，以便排序，
		// 按右对齐方式比较
		// obj: An object to compare with this instance
		// 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
		// Less than zero: This instance is less than obj.
		// Zero: This instance is equal to obj.
		// Greater than zero: This instance is greater than obj.
		// 异常: ArgumentException,obj is not the same type as this instance.
		public virtual int CompareTo(object obj)
		{
			Item item = (Item)obj;

			return (this.Length - item.Length);	// 比较谁内容更长
		}


	}


	// 枚举器
	public class ItemFileBaseEnumerator : IEnumerator
	{
		ItemFileBase m_file = null;
		long m_index = -1;

		public ItemFileBaseEnumerator(ItemFileBase file)
		{
			m_file = file;
		}

		public void Reset()
		{
			m_index = -1;
		}

		public bool MoveNext()
		{
			m_index ++;
			if (m_index >= m_file.Count)
				return false;

			return true;
		}

		public object Current
		{
			get
			{
				return (object)m_file[m_index];
			}
		}
	}


	// 拍紧风格
    [Flags]
    public enum CompressStyle 
	{
		Index = 0x01,	// 拍紧索引文件
		Data = 0x02,	// 拍紧数据文件
	}

	public delegate Item Delegate_newItem();

	/// <summary>
	/// 文件中的事项集合。实现事项可排序、自由存取的大文件功能。对内存需求小。
	/// </summary>
	public class ItemFileBase : IEnumerable, IDisposable
	{
        public bool ReadOnly = false;

		public ReaderWriterLock m_lock = new ReaderWriterLock();
		public static int m_nLockTimeout = 5000;	// 5000=5秒


		bool disposed = false;
		// 大文件，大流
		public string m_strBigFileName = "";
		public Stream m_streamBig = null;

		// 小文件，小流
		public string m_strSmallFileName = "";
		public Stream m_streamSmall = null;

		public long m_count = 0;

		bool bDirty = false; //初始值false,表示干净

		public Delegate_newItem procNewItem = null;
		/*
		 * 使用说明
		如果procNewItem已经挂接了delegate，则集合类会在适当时候使用它来创建新Item或其派生类的对象
		这个方法的好处，是不用派生集合类ItemFileBase
		如果procNewItem为空，就使用this.NewItem()函数。基类中有缺省实现，就是返回Item类型对象
		这个方法的缺点，是要派生集合类ItemFileBase
		两个方法最好不要混用，因为容易导致理解混乱。如果混用，procNewItem优先。
		*
		*/


		public ItemFileBase()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method 
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~ItemFileBase()      
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}


		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue 
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the 
		// runtime from inside the finalizer and you should not reference 
		// other objects. Only unmanaged resources can be disposed.
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!this.disposed)
			{
				// If disposing equals true, dispose all managed 
				// and unmanaged resources.
				if(disposing)
				{
					// Dispose managed resources.

                    // 这里有一点问题：可能析构函数调不了Close()
					// this.Close();
				}

                this.Close();   // 2007/6/8 移动到这里的

             
				/*
				// Call the appropriate methods to clean up 
				// unmanaged resources here.
				// If disposing is false, 
				// only the following code is executed.
				CloseHandle(handle);
				handle = IntPtr.Zero;            
				*/
			}
			disposed = true;         
		}

		// 创建新item对象
		// 如果派生了item类并且希望放在本集合中管理，可派生集合类并重载本函数。
		// 也可使用procNewItem接口，这样就不必派生集合类
		// 参见procNewItem定义处说明
		public virtual Item NewItem()
		{
			return new Item();
		}

		// 整数索引器
		public Item this[Int64 nIndex]
		{
			get
			{
				return GetItem(nIndex, false);
			}
		}

		// 记录数
		public Int64 Count
		{
			get
			{
				// 加读锁
				this.m_lock.AcquireReaderLock(m_nLockTimeout);
				try 
				{
					return m_count;
				}
				finally
				{
					this.m_lock.ReleaseReaderLock();
				}

			}
		}

		// 清空内容，但仍然在可用状态
		public void Clear()
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				if (m_streamBig != null)
					m_streamBig.SetLength(0);

				if (m_streamSmall != null)
					m_streamSmall.SetLength(0);

				m_count=0;
				bDirty = false;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}

		}

        // 打开
        // bInitialIndex	初始化index文件，并打开。
        public void Open(bool bInitialIndex)
        {
            // 加写锁
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                if (m_streamBig == null)
                {
                    if (m_strBigFileName == "")
                        m_strBigFileName = this.GetTempFileName();	// 创建临时文件可以用delegate处理

                    m_streamBig = File.Open(m_strBigFileName,
                        FileMode.OpenOrCreate);


                    /*
                    m_streamBig = File.Open(m_strBigFileName,
                        FileMode.OpenOrCreate, 
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite);  // 2007/12/26 new add
                     * */
                }

                if (bInitialIndex == true)
                {
                    string strSaveSmallFileName = "";
                    if (this.m_strSmallFileName != "")
                        strSaveSmallFileName = m_strSmallFileName;

                    RemoveIndexFile();

                    if (strSaveSmallFileName != "")
                        m_strSmallFileName = strSaveSmallFileName;

                    OpenIndexFile();
                }

            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

		// 完整复制一个集合对象
		public void Copy(ItemFileBase file)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				this.Clear();

				foreach(Item item in file)
				{
					this.Add(item);
				}
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// 压缩掉那些已标记删除事项占据的磁盘空间
		// 对于index文件，如果其中没有包含标记删除的事项，可以使用乘法搜索，速度快。
		// 一般要求排序前，Compress一下，否则排序速度很慢。
		public void CompressDeletedItem(CompressStyle style)
		{
			if ((style & CompressStyle.Data) == CompressStyle.Data)
			{
				// 删除标志在index文件中

				throw(new Exception("暂时不支持此功能"));
				/*
				// 清除老index
				RemoveIndexFile();
				OpenIndexFile();
				*/
			}

			if ((style & CompressStyle.Index) == CompressStyle.Index)
			{
				if (m_streamSmall == null)
					return;
				CompressIndex(m_streamSmall);
				bDirty = false;
			}

		}

		// 在尾部追加一个事项
		public virtual void Add(Item item)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				// 将数据文件指针置于尾部
				m_streamBig.Seek(0,
					SeekOrigin.End);

				// 若存在索引文件
				if (m_streamSmall != null)
				{
					// 写入一个新index条目

					m_streamSmall.Seek (0,SeekOrigin.End);
					long nPosition = m_streamBig.Position ;

					byte[] bufferPosition = new byte[8];
					bufferPosition = System.BitConverter.GetBytes((long)nPosition); // 原来缺乏(long), 是一个bug. 2006/10/1 修改
                    Debug.Assert(bufferPosition.Length == 8, "");
                    m_streamSmall.Write(bufferPosition, 0, 8);
				}

                // 2007/7/3 new add
                item.BuildBuffer();

				byte[] bufferLength = System.BitConverter.GetBytes((Int32)item.Length);

                Debug.Assert(bufferLength.Length == 4, "");
				m_streamBig.Write(bufferLength,0,4);

				item.WriteData(m_streamBig);

				m_count++;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// 标记删除一条记录
		public void RemoveAt(int nIndex)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				if (nIndex < 0 || nIndex >= m_count)
				{
					throw(new Exception("下标 " + Convert.ToString(nIndex) + " 越界(Count=" + Convert.ToString(m_count) + ")"));
				}
				int nRet = RemoveAtAuto(nIndex);
				if (nRet == -1)
				{
					throw(new Exception ("RemoveAtAuto fail"));
				}

				m_count --;
				// bDirty = true;	// 表示已经有标记删除的事项了

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		//标记删除多条记录
		public void RemoveAt(int nIndex,
			int nCount)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				if (nIndex < 0 || nIndex + nCount > m_count)
				{
					throw(new Exception("下标 " + Convert.ToString(nIndex) + " 越界(Count=" + Convert.ToString(m_count) + ")"));
				}

				int nRet = 0;
				if (m_streamSmall != null) // 有索引文件时
				{
					// nRet = RemoveAtIndex(nIndex);
					nRet = CompressRemoveAtIndex(nIndex, nCount);
				}
				else 
				{
					throw(new Exception ("暂时还没有编写"));

				}


				if (nRet == -1)
				{
					throw(new Exception ("RemoveAtAuto fail"));
				}

				m_count -= nCount;
				// bDirty = true;	// 表示已经有标记删除的事项了

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// 插入一个事项
		public virtual void Insert(int nIndex,
			Item item)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
                if (m_streamSmall == null
                    && m_streamBig == null)
                    throw (new Exception("内部文件尚未打开或者已经被关闭"));

				// 若不存在索引文件
				if (m_streamSmall == null)
					throw(new Exception("暂不支持无索引文件方式下的插入操作"));


				// 将数据文件指针置于尾部
				m_streamBig.Seek(0,
					SeekOrigin.End);

				// 若存在索引文件
				if (m_streamSmall != null)
				{
					// 插入一个新index条目
					long lStart = (long)nIndex * 8;
					StreamUtil.Move(m_streamSmall,
						lStart, 
						m_streamSmall.Length - lStart,
						lStart + 8);

					m_streamSmall.Seek (lStart,SeekOrigin.Begin);
					long nPosition = m_streamBig.Position;

					byte[] bufferPosition = new byte[8];
                    bufferPosition = System.BitConverter.GetBytes((long)nPosition); // 原来缺乏(long), 是一个bug. 2006/10/1 修改
                    Debug.Assert(bufferPosition.Length == 8, "");
					m_streamSmall.Write (bufferPosition,0,8);
				}

				byte[] bufferLength = System.BitConverter.GetBytes((Int32)item.Length);
                Debug.Assert(bufferLength.Length == 4, "");
				m_streamBig.Write(bufferLength,0,4);

				item.WriteData(m_streamBig);

				m_count++;

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// 关闭。删除所有文件。
		public void Close()
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

			RemoveDataFile();

			RemoveIndexFile();

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// 将外部文件绑定到本对象
		public void Attach(string strFileName,
			string strIndexFileName)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				RemoveIndexFile();
				RemoveDataFile();	// 清除当前正在使用的内部数据文件

				m_strBigFileName = strFileName;
				m_streamBig = File.Open (m_strBigFileName,
					FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);   // 2007/12/26 new add


				bool bCountSeted = false;
				if (strIndexFileName != null)
				{
					if (File.Exists(strIndexFileName) == true)
					{
						this.m_strSmallFileName = strIndexFileName;
						this.OpenIndexFile();
					}
					else
					{
						this.CreateIndexFile(strIndexFileName);
					}

					m_count = GetCountFromIndexFile();
					bCountSeted = true;
				}

				if (bCountSeted == false)
					m_count = GetCountFromDataFile();

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		// 将数据文件整序后输出
		void Resequence(string strOutputFileName)
		{
			this.m_streamSmall.Seek(0, SeekOrigin.Begin);

			for(;;)
			{

			}

			// return;
		}


		// 将数据文件和对象脱钩
		// parameters:
		//		bResequence	是否重新整序?
		// return:
		//	数据文件名
		public void Detach(out string strDataFileName,
			out string strIndexFileName)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				strDataFileName = m_strBigFileName;
				CloseDataFile();

				m_strBigFileName = "";	// 避免析构函数去删除

				strIndexFileName = this.m_strSmallFileName;
				CloseIndexFile();

				this.m_strSmallFileName = "";	// 避免析构函数去删除

				return;
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		// 排序
		public void Sort()
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				if (this.m_strSmallFileName != "")
					CreateIndexFile(this.m_strSmallFileName);
				else 
					CreateIndexFile(null);
				QuickSort();
			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

        // 确保创建了索引
        public void EnsureCreateIndex()
        {
            if (this.m_streamSmall == null)
            {
                this.CreateIndexFile(null);
            }
            else
            {
                // Debug时可以校验一下index文件尺寸和Count的关系是否正确
            }
        }

		// 创建Index
		// parameters:
		//	strIndexFileName	如果==null，表示自动分配临时文件
		public void CreateIndexFile(string strIndexFileName)
		{
			int nLength;

			RemoveIndexFile();

			if (strIndexFileName != null)
				this.m_strSmallFileName = strIndexFileName;

			OpenIndexFile();

			m_streamBig.Seek(0, SeekOrigin.Begin);
			m_streamSmall.Seek(0, SeekOrigin.End);

			int i=0;
			long lPosition = 0;
			for(i=0;;i++)
			{
				//长度字节数组
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n<4)   //表示文件到尾
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //删除项
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}

				byte[] bufferOffset = new byte[8];
				bufferOffset = System.BitConverter.GetBytes((long)lPosition);
                Debug.Assert(bufferOffset.Length == 8, "");
				m_streamSmall.Write (bufferOffset,0,8);

			CONTINUE:

				m_streamBig.Seek (nLength,SeekOrigin.Current);
				lPosition += (4+nLength);
			}
		}


		// 快速排序
		// 修改：建议增加delegate用来探测是否需要中断循环
		// 一般用户不要调用这个函数，调用Sort()即可
		// 如何显示排序进度? 头疼的事情。可否用堆栈深度表示进度?
		// 需要辨别完全排序的部分中，item的数量，将这些部分从总item
		// 数量中去除，就是进度指示的依据。
		// return:
		//  0 succeed
		//  1 interrupted
		public int QuickSort()
		{
            if (this.m_streamSmall == null)
            {
                throw new Exception("调用QuickSort前，需要先创建索引");
            }

			ArrayList stack = new ArrayList (); // 堆栈
			int   nStackTop = 0;
			long   nMaxRow = m_streamSmall.Length /8;  //m_count;
			long   k = 0;
			long j = 0;
			long i = 0;

			if (nMaxRow == 0)
				return 0;

			/*
			if (nMaxRow >= 10) // 调试
			 nMaxRow = 10;
			*/

			Push(stack, 0, nMaxRow - 1, ref nStackTop);
			while(nStackTop>0) 
			{
				Pop(stack, ref k, ref j, ref nStackTop);
				while(k<j) 
				{
					Split(k,j,ref i);
					Push(stack, i+1, j, ref nStackTop);
					j = i - 1;
				}
			}

			return 0;
		}

		#region 基础函数

		// 删除Index文件
		private void RemoveIndexFile()
		{
			// 如果流对象存在，关闭，置空
			if (m_streamSmall != null)
			{
				m_streamSmall.Close();
				m_streamSmall = null;
			}

            if (this.ReadOnly == false)
            {
                // 如果文件名存在，删除文件，置变量值为空
                if (m_strSmallFileName != "" && m_strSmallFileName != null)
                {
                    File.Delete(m_strSmallFileName);
                    m_strSmallFileName = "";
                }
            }

			bDirty = false;
		}

		// 删除data文件
		private void RemoveDataFile()
		{
			// 如果流对象存在，关闭，置空
			if (m_streamBig != null)
			{
				m_streamBig.Close();
				m_streamBig = null;
			}

            if (this.ReadOnly == false)
            {
                // 如果文件名存在，删除文件，置变量值为空
                if (m_strBigFileName != "" && m_strBigFileName != null)
                {
                    File.Delete(m_strBigFileName);
                    m_strBigFileName = "";
                }
            }
		}

		public void CloseDataFile()
		{
			// 如果流对象存在，关闭，置空
			if (m_streamBig != null)
			{
				m_streamBig.Close();
				m_streamBig = null;
			}
		}


		// 是否具有打开的索引文件
		public bool HasIndexed
		{
			get 
			{
				if (m_streamSmall == null)
					return false;
				return true;
			}
		}

        public virtual string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

		// 打开Index文件
		private void OpenIndexFile()
		{
			if (m_streamSmall == null)
			{
				if (m_strSmallFileName == "")
					m_strSmallFileName = this.GetTempFileName();
				// 创建index文件的时候，可以给出角色暗示，以便取有特色的物理文件名

				m_streamSmall = File.Open(m_strSmallFileName,
					FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);  // 2007/12/26 new add

			}

			bDirty = false;
			// m_streamSmall的非空就表明它处于Open状态
		}


		public void CloseIndexFile()
		{
			// 如果流对象存在，关闭，置空
			if (m_streamSmall != null)
			{
				m_streamSmall.Close();
				m_streamSmall = null;
			}
		}


		private Int64 GetCountFromDataFile()
		{
			Debug.Assert(m_streamBig != null, "data流必须先打开");

			m_streamBig.Seek(0, SeekOrigin.Begin);

			Int64 i=0;
			Int32 nLength;

			while(true)
			{
				//长度字节数组
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n < 4)   //表示文件到尾
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //删除项
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}
				i++;

			CONTINUE:
				m_streamBig.Seek(nLength, SeekOrigin.Current);
			}
			return i;
		}

		// 从索引文件得到元素个数
		// 要求bDirty == false
		private Int64 GetCountFromIndexFile()
		{
			Debug.Assert(m_streamSmall != null, "index流必须先打开");

			return m_streamSmall.Length / 8;

			/* 适当时候增加遍历计算个数功能, 排除那些负数偏移值
			m_streamBig.Seek(0, SeekOrigin.Begin);

			Int64 i=0;
			Int32 nLength;

			while(true)
			{
				//长度字节数组
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n < 4)   //表示文件到尾
					break;

				nLength = System.BitConverter.ToInt32(bufferLength,0);
				if (nLength<0)  //删除项
				{
					nLength = (int)GetRealValue(nLength);
					goto CONTINUE;
				}
				i++;

			CONTINUE:
				m_streamBig.Seek(nLength, SeekOrigin.Current);
			}
			return i;
			*/
		}

		// 得到真正的位置或长度
		Int64 GetRealValue(Int64 lPositionOrLength)
		{
			if (lPositionOrLength<0)
			{
				lPositionOrLength = -lPositionOrLength;
				lPositionOrLength--;
			}
			return lPositionOrLength;
		}

		// 得到删除标记使用的位置或长度
		// 负数表示被删除的事项
		Int64 GetDeletedValue(Int64 lPositionOrLength)
		{
			if (lPositionOrLength >= 0)
			{
				lPositionOrLength ++;
				lPositionOrLength = -lPositionOrLength;
			}

			return 	lPositionOrLength;
		}


		// bContainDeleted	是否包含被删除的事项?
		public Item GetItem(Int64 nIndex,
			bool bContainDeleted)
		{
			Item item = null;
			long lBigOffset;

			//自动返回大文件的编移量,小文件存在时，从小文件得到，不存在时，从大文件得到
			//bContainDeleted等于false，忽略已删除的记录，为true,不忽略
			//返回值
			//>=0:正常
			//-1:当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
			lBigOffset = GetDataOffsetAuto(nIndex, bContainDeleted);

			//当bContainDeleted为false时即不包含已删除记录时，返回值-1，表示没找到
			if (bContainDeleted == false)
			{
				if (lBigOffset == -1)
					return null;
			}
			item = GetItemByOffset(lBigOffset);

			return item;
		}

		// bContainDeleted	是否包含被删除的事项?
		public Item GetCompareItem(Int64 nIndex,
			bool bContainDeleted)
		{
			Item item = null;
			long lBigOffset;

			//自动返回大文件的编移量,小文件存在时，从小文件得到，不存在时，从大文件得到
			//bContainDeleted等于false，忽略已删除的记录，为true,不忽略
			//返回值
			//>=0:正常
			//-1:当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
			lBigOffset = GetDataOffsetAuto(nIndex, bContainDeleted);

			//当bContainDeleted为false时即不包含已删除记录时，返回值-1，表示没找到
			if (bContainDeleted == false)
			{
				if (lBigOffset == -1)
					return null;
			}
			item = GetCompareItemByOffset(lBigOffset);

			return item;
		}


		// 自动返回事项在数据文件的编移量。所谓自动，意思是, 小文件存在时，从小文件查得到(快)，不存在时，从大文件顺序得到(慢)
		// parameters:
		//		bContainDeleted等于false，忽略已删除的记录，为true,不忽略
		// 返回值
		//		>=0	正常
		//		-1	当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
		long GetDataOffsetAuto(long nIndex,bool bContainDeleted)
		{
			if (m_streamSmall != null)
			{
				return GetDataOffsetFromIndexFile(nIndex, bContainDeleted);
			}
			else
			{
				return GetDataOffsetFromDataFile(nIndex, bContainDeleted);
			}
		}

		// 在数据文件中直接搜索事项的起点偏移量。
		//	当然，这样速度很慢
		// return:
		//		-1	当bContainDeleted为false时-1表示出错的情况，bContainDeleted为true时表示正常的负值
		long GetDataOffsetFromDataFile(long nIndex,bool bContainDeleted)
		{
			m_streamBig.Seek(0, SeekOrigin.Begin);
			long lBigOffset = 0;

			int nLength;
			int i = 0;
			while(true)
			{
				//读4个字节，得到长度
				byte[] bufferLength = new byte[4];
				int n = m_streamBig.Read(bufferLength,0,4);
				if (n<4)   //表示文件到尾
					break;
				nLength = System.BitConverter.ToInt32(bufferLength,0);

				if (bContainDeleted == false)
				{
					if (nLength<0)
					{
						//转换为实际的长度，再seek
						long lTemp = GetRealValue(nLength);
						m_streamBig.Seek (lTemp,SeekOrigin.Current);

						lBigOffset += (4+lTemp);
						continue;
					}
				}

				if (i == nIndex)
				{
					return  lBigOffset;
				}
				else
				{
					m_streamBig.Seek (nLength,SeekOrigin.Current);
				}

				lBigOffset += (4+nLength);

				i++;
			}

			return -1;
		}

		// 从索引文件中计算返回事项在数据文件中的偏移量
		// return:
		//		-1	当bContainDeleted为false时-1表示出错的情况，bContainDeleted为true时表示正常的负值
		long GetDataOffsetFromIndexFile(
			long nIndex,
			bool bContainDeleted)
		{
			if (m_streamSmall == null)
			{
				Debug.Assert(true, "索引文件流为null, GetDataOffsetFromIndexFile()函数无法执行");
				throw(new Exception ("索引文件流为null, GetDataOffsetFromIndexFile()函数无法执行"));
			}

			// 干净，也就是索引中确保无删除项存在的情况下，可以用乘法运算得到位置，速度快
			if (bDirty == false)
			{
				if (nIndex*8>=m_streamSmall.Length || nIndex<0)
				{
					throw(new Exception("索引 " + Convert.ToString(nIndex) + "经计算超过索引文件当前最大范围"));
				}
				return GetIndexItemValue(nIndex);
			}
			else
			{
				long lBigOffset = 0;

				m_streamSmall.Seek(0, SeekOrigin.Begin);
				int i = 0;
				while(true)
				{
					//读8个字节，得到位置
					byte[] bufferBigOffset = new byte[8];
					int n = m_streamSmall.Read(bufferBigOffset,0,8);
					if (n < 8)   //表示文件到尾
						break;
					lBigOffset = System.BitConverter.ToInt32(bufferBigOffset, 0);
					
					if (bContainDeleted == false)
					{
						//为负数时跳过
						if (lBigOffset<0)
						{
							continue;
						}
					}
					//表示按序号找到
					if (i == nIndex)
					{
						return lBigOffset;
					}
					i++;
				}
			}
			return -1;
		}

		// 用*8的方法算索引事项在索引文件的位置，取得其值
		// ，包含已删除的记录，并取出它所代表的数据事项在数据文件的编移量
		long GetIndexItemValue(long nIndex)
		{
			if( m_streamSmall == null)
			{
				throw(new Exception("m_streamSmall对象为空, GetIndexItemValue()无法进行"));
			}

			if (nIndex*8 >= m_streamSmall.Length || nIndex<0)
			{
				throw(new Exception("索引 " + Convert.ToString(nIndex) + "经计算超过索引文件当前最大范围"));
			}

			m_streamSmall.Seek(nIndex*8, 
				SeekOrigin.Begin);

			byte[] bufferOffset = new byte[8];
			int n = m_streamSmall.Read(bufferOffset, 0, 8);
			if (n <= 0)
			{
				throw(new Exception("GetIndexItemValue()异常：实际流的长度"+Convert.ToString (m_streamSmall.Length )+"\r\n"
					+"希望Seek到的位置"+Convert.ToString (nIndex*8)+"\r\n"
					+"实际读的长度"+Convert.ToString (n)));
			}
			long lOffset = System.BitConverter.ToInt64(bufferOffset,0);

			return lOffset;
		}

		//	lOffset不论正负数都可以找到记录，调用时，注意，如果不需要得到被删除的记录，自已做判断
		Item GetItemByOffset(long lOffset)
		{
			Item item = null;

			if (lOffset <0)
			{
				lOffset = GetRealValue(lOffset);
			}

			if (lOffset >= m_streamBig.Length )
			{
				throw(new Exception ("内部错误，位置大于总长度"));
				//return null;
			}

			m_streamBig.Seek(lOffset, SeekOrigin.Begin);

			//长度字节数组
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //表示文件到尾
			{
				throw(new Exception ("内部错误:Read error"));
				//return null;
			}

			// 如果procNewItem已经挂接了delegate，则使用它来创建新Item或其派生类的对象
			// 这个方法的好处，是不用派生集合类ItemFileBase
			if (this.procNewItem != null)
				item = procNewItem();
			else	// procNewItem为空，就使用this.NewItem()函数。基类中有缺省实现，就是返回Item类型对象
				item = this.NewItem(); // 这个方法的缺点，是要派生集合类ItemFileBase

			item.Length = System.BitConverter.ToInt32(bufferLength, 0);
			item.ReadData(m_streamBig);

			return item;
		}


		//	lOffset不论正负数都可以找到记录，调用时，注意，如果不需要得到被删除的记录，自已做判断
		public Item GetCompareItemByOffset(long lOffset)
		{
			Item item = null;

			if (lOffset <0)
			{
				lOffset = GetRealValue(lOffset);
			}

			if (lOffset >= m_streamBig.Length )
			{
				throw(new Exception ("内部错误，位置大于总长度"));
				//return null;
			}

			m_streamBig.Seek(lOffset, SeekOrigin.Begin);

			//长度字节数组
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //表示文件到尾
			{
				throw(new Exception ("内部错误:Read error"));
				//return null;
			}

			// 如果procNewItem已经挂接了delegate，则使用它来创建新Item或其派生类的对象
			// 这个方法的好处，是不用派生集合类ItemFileBase
			if (this.procNewItem != null)
				item = procNewItem();
			else	// procNewItem为空，就使用this.NewItem()函数。基类中有缺省实现，就是返回Item类型对象
				item = this.NewItem(); // 这个方法的缺点，是要派生集合类ItemFileBase

			item.Length = System.BitConverter.ToInt32(bufferLength, 0);
			item.ReadCompareData(m_streamBig);

			return item;
		}

	
        /*
		// 真正压缩标记删除了的那些事项
		private static int CompressIndex(Stream oStream)
		{
			if (oStream == null)
			{
				return -1;
			}

			long lDeletedStart = 0;  //删除块的起始位置
			long lDeletedEnd = 0;    //删除块的结束位置
			long lDeletedLength = 0;
			bool bDeleted = false;   //是否已出现删除块

			long lUseablePartLength = 0;    //后面正常块的长度
			bool bUserablePart = false;    //是否已出现正常块

			bool bEnd = false;
			long lValue = 0;

			oStream.Seek (0,SeekOrigin.Begin );
			while(true)
			{
				int nRet;
				byte[] bufferValue = new byte[8];
				nRet = oStream.Read(bufferValue,0,8);
				if (nRet != 8 && nRet != 0)  
				{
					throw(new Exception ("内部错误:读到的长度不等于8"));
					//break;
				}
				if (nRet == 0)//表示结束
				{
					if(bUserablePart == false)
						break;

					lValue = -1;
					bEnd = true;
					//break;
				}

				if (bEnd != true)
				{
					lValue = BitConverter.ToInt64(bufferValue,0);
				}
				if (lValue<0)
				{
					if (bDeleted == true && bUserablePart == true)
					{
						lDeletedEnd = lDeletedStart + lDeletedLength;
						//调MovePart(lDeletedStart,lDeletedEnd,lUseablePartLength)

						StreamUtil.Move(oStream,
							lDeletedEnd,
							lUseablePartLength,
							lDeletedStart);

						//重新定位deleted的起始位置
						lDeletedStart = lUseablePartLength-lDeletedLength+lDeletedEnd;
						lDeletedEnd = lDeletedStart+lDeletedLength;

						oStream.Seek (lDeletedEnd+lUseablePartLength,SeekOrigin.Begin);

					}

					bDeleted = true;
					bUserablePart = false;
					lDeletedLength += 8;  //结束位置加8
				}
				else if (lValue>=0)
				{
					//当出现过删除块时，又进入新的有用块时，前方的有用块不计，重新计算长度
					//|  userable  | ........ |  userable |
					//|  ........  | userable |
					if (bDeleted == true && bUserablePart == false)
					{
						lUseablePartLength = 0;
					}

					bUserablePart = true;
					lUseablePartLength += 8;
					
					if (bDeleted == false)
					{
						lDeletedStart += 8;  //当不存在删除块时，删除超始位置加8
					}
				}

				if (bEnd == true)
				{
					break;
				}
			}

			//只剩尾部的被删除记录
			if (bDeleted == true && bUserablePart == false)
			{
				//lDeletedEnd = lDeletedStart + lDeletedLength;
				oStream.SetLength(lDeletedStart);
			}

			// bDirty = false;
			return 0;
		}
        */


        // 真正压缩标记删除了的那些事项
        // 压缩索引
        private static int CompressIndex(Stream oStream)
        {
            if (oStream == null)
            {
                return -1;
            }

            int nRet;
            long lRestLength = 0;
            long lDeleted = 0;
            long lCount = 0;

            oStream.Seek(0, SeekOrigin.Begin);
            lCount = oStream.Length / 8;
            for (long i = 0; i < lCount; i++)
            {
                byte[] bufferValue = new byte[8];
                nRet = oStream.Read(bufferValue, 0, 8);
                if (nRet != 8 && nRet != 0)
                {
                    throw (new Exception("内部错误:读到的长度不等于8"));
                }

                long lValue = BitConverter.ToInt64(bufferValue, 0);

                if (nRet == 0)//表示结束
                {
                    break;
                }

                if (lValue < 0)
                {
                    // 表示需要删除此项目
                    lRestLength = oStream.Length - oStream.Position;

                    Debug.Assert(oStream.Position - 8 >= 0, "");


                    long lSavePosition = oStream.Position;

                    StreamUtil.Move(oStream,
                        oStream.Position,
                        lRestLength,
                        oStream.Position - 8);

                    oStream.Seek(lSavePosition - 8, SeekOrigin.Begin);

                    lDeleted++;
                }
            }

            if (lDeleted > 0)
            {
                oStream.SetLength((lCount - lDeleted) * 8);
            }

            return 0;
        }


		#endregion

	

		#region 排序有关的基础函数

		void Push(ArrayList stack,
			long lStart,
			long lEnd,
			ref int nStackTop)
		{
			if (nStackTop < 0)
			{
				throw(new Exception ("nStackTop不能小于0"));
			}
			if (lStart < 0)
			{
				throw(new Exception ("nStart不能小于0"));
			}

			if (nStackTop*2 != stack.Count )
			{
				throw(new Exception ("nStackTop*2不等于stack.m_count"));
			}


			stack.Add (lStart);
			stack.Add (lEnd);

			nStackTop ++;
		}


		void Pop(ArrayList stack,
			ref long lStart,
			ref long lEnd,
			ref int nStackTop)
		{
			if (nStackTop <= 0)
			{
				throw(new Exception ("pop以前,nStackTop不能小于等于0"));
			}

			if (nStackTop*2 != stack.Count )
			{
				throw(new Exception ("nStackTop*2不等于stack.m_count"));
			}

			lStart = (long)stack[(nStackTop-1) * 2];
			lEnd = (long)stack[(nStackTop-1) * 2+1];

			stack.RemoveRange((nStackTop-1) * 2,2);

			nStackTop --;
		}


		void Split(long nStart,
			long nEnd,
			ref long nSplitPos)
		{
			// 取得中项
			long pStart = 0;
			long pEnd = 0;
			long pMiddle = 0;
			long pSplit = 0;
			long nMiddle;
			long m,n,i,j,k;
			long T = 0;
			int nRet;
			long nSplit;


			nMiddle = (nStart + nEnd) / 2;

			pStart = GetIndexItemValue(nStart);  
			pEnd = GetIndexItemValue(nEnd);   

			// 看起点和终点是否紧密相连
			if (nStart + 1 == nEnd) 
			{
				nRet = Compare(pStart, pEnd);
				if (nRet > 0) 
				{ // 交换
					T = pStart;
					SetRowPtr(nStart, pEnd);
					SetRowPtr(nEnd, T);
				}
				nSplitPos = nStart;
				return;
			}


			pMiddle = GetIndexItemValue(nMiddle);   //GetRowPtr(nMiddle);

			nRet = Compare(pStart, pEnd);
			if (nRet <= 0) 
			{
				nRet = Compare(pStart, pMiddle);
				if (nRet <= 0) 
				{
					pSplit = pMiddle;
					nSplit = nMiddle;
				}
				else 
				{
					pSplit = pStart;
					nSplit = nStart;
				}
			}
			else 
			{
				nRet = Compare(pEnd, pMiddle);
				if (nRet <= 0) 
				{
					pSplit = pMiddle;
					nSplit = nMiddle;
				}
				else 
				{
					pSplit = pEnd;
					nSplit = nEnd;
				}

			}

			// 
			k = nSplit;
			m = nStart;
			n = nEnd;

			T = GetIndexItemValue(k);
			// (m)-->(k)
			SetRowPtr(k, GetIndexItemValue(m));
			i = m;
			j = n;
			while(i!=j) 
			{
				while(true) 
				{
					nRet = Compare(GetIndexItemValue(j), T);
					if (nRet >= 0 && i<j)
						j = j - 1;
					else 
						break;
				}
				if (i<j) 
				{
					// (j)-->(i)
					SetRowPtr(i, GetIndexItemValue(j) /*GetRowPtr(j)*/);
					i = i + 1;
					while(true) 
					{
						nRet = Compare(/*GetRowPtr(i)*/ GetIndexItemValue(i), T);
						if (nRet <=0 && i<j)
							i = i + 1;
						else 
							break;
					}
					if (i<j) 
					{
						// (i)--(j)
						SetRowPtr(j, GetIndexItemValue(i) /*GetRowPtr(i)*/);
						j = j - 1;
					}
				}
			}
			SetRowPtr(i, T);
			nSplitPos = i;
		}


		public void SetRowPtr(long nIndex, long lPtr)
		{
			byte[] bufferOffset ;

			//得到值
			bufferOffset = new byte[8];
			bufferOffset = BitConverter.GetBytes((long)lPtr);
			

			//覆盖值
			m_streamSmall.Seek (nIndex*8,SeekOrigin.Begin);
            Debug.Assert(bufferOffset.Length == 8, "");
			m_streamSmall.Write (bufferOffset,0,8);

		}

		// 本函数是内部使用。若要改变排序行为，可重载Item的CompareTo()函数
		public virtual int Compare(long lPtr1,long lPtr2)
		{
			if (lPtr1<0 && lPtr2<0)
				return 0;
			else if (lPtr1>=0 && lPtr2<0)
				return 1;
			else if (lPtr1<0 && lPtr2>=0)
				return -1;

			Item item1 = GetCompareItemByOffset(lPtr1);
			Item item2 = GetCompareItemByOffset(lPtr2);

			return item1.CompareTo(item2);
		}

		#endregion



		#region 和删除有关的基础函数

		// 自动选择从何处删除
		int RemoveAtAuto(int nIndex)
		{
			int nRet = -1;
			if (m_streamSmall != null) // 有索引文件时
			{
				// nRet = RemoveAtIndex(nIndex);
				nRet = CompressRemoveAtIndex(nIndex, 1);
			}
			else  // 索引文件不存在时， 从数据文件中删除
			{
				nRet = RemoveAtData(nIndex);
			}
			return nRet;
		}

		// 在索引流中定位事项
		public long LocateIndexItem(int nIndex)
		{
			long lPositionS = 0;
			if (bDirty == false)
			{
				lPositionS = nIndex*8;
				if (lPositionS>=m_streamSmall.Length || nIndex<0)
				{
					throw(new Exception("下标越界..."));
				}

				m_streamSmall.Seek(lPositionS, SeekOrigin.Begin);
				return lPositionS;
			}
			else
			{
				m_streamSmall.Seek (0,SeekOrigin.Begin);
				long lBigOffset;
				int i = 0;
				while(true)
				{
					//读8个字节，得到位置
					byte[] bufferBigOffset = new byte[8];
					int n = m_streamSmall.Read(bufferBigOffset,0,8);
					if (n<8)   //表示文件到尾
						break;
					lBigOffset = System.BitConverter.ToInt64(bufferBigOffset,0);
					
					//为负数时跳过
					if (lBigOffset<0)
					{
						goto CONTINUE;
					}

					//表示按序号找到
					if (i == nIndex)
					{
						m_streamSmall.Seek (lPositionS,SeekOrigin.Begin );
						return lPositionS;
					}
					i++;

				CONTINUE:
					lPositionS += 8;
				}
			}
			return -1;
		}

		// 从索引文件中标记删除一个事项
		public int MaskRemoveAtIndex(int nIndex)
		{
			int nRet;

			// lBigOffset表示大文件的编移量，-1表示错误
			long lBigOffset = GetDataOffsetFromIndexFile(nIndex,false);
			if (lBigOffset == -1)
				return -1;

			lBigOffset = GetDeletedValue(lBigOffset);

			byte[] bufferBigOffset = new byte[8];
			bufferBigOffset = BitConverter.GetBytes((long)lBigOffset);

			nRet = (int)LocateIndexItem(nIndex);
			if (nRet == -1)
				return -1;
            Debug.Assert(bufferBigOffset.Length == 8, "");
			m_streamSmall.Write(bufferBigOffset,0,8);

			return 0;
		}


		// 从索引文件中挤压式删除一个事项
		public int CompressRemoveAtIndex(int nIndex,
			int nCount)
		{
			if (m_streamSmall == null)
				throw new Exception("索引文件尚未初始化");

			long lStart = (long)nIndex * 8;
			StreamUtil.Move(m_streamSmall,
					lStart + 8*nCount, 
					m_streamSmall.Length - lStart - 8*nCount,
					lStart);

			m_streamSmall.SetLength(m_streamSmall.Length - 8*nCount);

			return 0;
		}


		//从大文件中删除
		public int RemoveAtData(int nIndex)
		{
			//得到大文件偏移量
			long lBigOffset = GetDataOffsetFromDataFile(nIndex, false);
			if (lBigOffset == -1)
				return -1;

			if (lBigOffset >= m_streamBig.Length )
			{
				throw(new Exception ("内部错误，位置大于总长度"));
				//return null;
			}

			m_streamBig.Seek(lBigOffset,SeekOrigin.Begin);
			//长度字节数组
			byte[] bufferLength = new byte[4];
			int n = m_streamBig.Read(bufferLength,0,4);
			if (n<4)   //表示文件到尾
			{
				throw(new Exception ("内部错误:Read error"));
				//return null;
			}

			int nLength = System.BitConverter.ToInt32(bufferLength,0);
			nLength = (int)GetDeletedValue(nLength);

			bufferLength = BitConverter.GetBytes((Int32)nLength);
			m_streamBig.Seek (-4,SeekOrigin.Current);
            Debug.Assert(bufferLength.Length == 4);
			m_streamBig.Write (bufferLength,0,4);

			return 0;
		}


		#endregion

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new ItemFileBaseEnumerator(this);
		}

	}
}
