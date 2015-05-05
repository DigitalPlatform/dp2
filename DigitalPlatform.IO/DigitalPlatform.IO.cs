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

	// 临时文件容器
	public class TempFileCollection : ArrayList
	{
		public TempFileCollection() 
		{
		}

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

	public delegate bool FlushOutput();
	public delegate bool ProgressOutput(long lCur);


	/// <remarks>
	/// StreamUtil类，Stream功能扩展函数
	/// </remarks>
	public class StreamUtil
	{
		public static long DumpStream(Stream streamSource,Stream streamTarget)
		{
			return DumpStream(streamSource, streamTarget, false);
		}

		// 连Flush带看IsConnected状态
		// 如果返回true表示正常，false表示输出流已经切断，没有必要再继续dump了

		public static long DumpStream(Stream streamSource,
			Stream streamTarget,
			FlushOutput flushOutputMethod)
		{
			int nChunkSize = 8192;
			byte[] bytes = new byte[nChunkSize];
			long lLength = 0;
			while (true) 
			{
				int n = streamSource.Read(bytes,0,nChunkSize);

				if (n != 0)	// 2005/6/8
					streamTarget.Write(bytes,0,n);

				if (flushOutputMethod != null) 
				{
					if (flushOutputMethod()  == false)
						break;
				}

				if (n<=0)
					break;

				lLength += n;
				//if (n<1000)
				//	break;
			}

			return lLength;
		}

		public static long DumpStream(Stream streamSource,
			Stream streamTarget,
			ProgressOutput progressOutputMethod)
		{
			int nChunkSize = 8192;
			byte[] bytes = new byte[nChunkSize];
			long lLength = 0;
			while (true) 
			{
				if (progressOutputMethod != null) 
				{
					if (progressOutputMethod(lLength) == false)
						break;
				}

				int n = streamSource.Read(bytes,0,nChunkSize);

				if (n != 0)	// 2005/6/8
					streamTarget.Write(bytes,0,n);


				if (n<=0)
					break;

				lLength += n;
				//if (n<1000)
				//	break;
			}

			if (progressOutputMethod != null) 
			{
				progressOutputMethod(lLength);
			}

			return lLength;
		}
		

		/// <summary>
		/// 将源流输入到目标流
        /// 调用前要确保文件指针在适当的位置。在那个位置，就从那个位置开始dump
		/// </summary>
		/// <param name="streamSource">源流</param>
		/// <param name="streamTarget">目标流</param>
		/// <returns>成功执行返回0或者以上，返回值表明本次写入的长度</returns>
		public static long DumpStream(Stream streamSource,
			Stream streamTarget,
			bool bFlush)
		{
			int nChunkSize = 8192;
			byte[] bytes = new byte[nChunkSize];
			long lLength = 0;
			while (true) 
			{
				int n = streamSource.Read(bytes,0,nChunkSize);

				if (n != 0)	// 2005/6/8
					streamTarget.Write(bytes,0,n);

				if (bFlush == true)
					streamTarget.Flush();

				if (n<=0)
					break;

				lLength += n;
				//if (n<1000)
				//	break;
			}

			return lLength;
		}

		public static long DumpStream(Stream streamSource, 
			Stream streamTarget,
			long lLength)
		{
			return DumpStream(streamSource, streamTarget, lLength, false);
		}


		public static long DumpStream(Stream streamSource, 
			Stream streamTarget,
			long lLength,
			bool bFlush)
		{
			long lWrited = 0;
			long lThisRead = 0;
			int nChunkSize = 8192;
			byte[] bytes = new byte[nChunkSize];
			while (true) 
			{
				long lLeft = lLength - lWrited;
				if (lLeft > nChunkSize)
					lThisRead = nChunkSize;
				else
					lThisRead = lLeft;
				long n = streamSource.Read(bytes,0,(int)lThisRead);

				if (n != 0) // 2005/6/8
				{
					streamTarget.Write(bytes,0,(int)n);
				}

				if (bFlush == true)
					streamTarget.Flush();


				//if (n<nChunkSize)
				//	break;
				if (n <= 0)
					break;

				lWrited += n;
			}

			return lWrited;
		}

		public static long DumpStream(Stream streamSource, 
			Stream streamTarget,
			long lLength,
			FlushOutput flushOutputMethod)
		{
			long lWrited = 0;
			long lThisRead = 0;
			int nChunkSize = 8192;
			byte[] bytes = new byte[nChunkSize];
			while (true) 
			{
				long lLeft = lLength - lWrited;
				if (lLeft > nChunkSize)
					lThisRead = nChunkSize;
				else
					lThisRead = lLeft;
				long n = streamSource.Read(bytes,0,(int)lThisRead);

				if (n != 0)	// 2005/6/8
					streamTarget.Write(bytes,0,(int)n);

				if (flushOutputMethod != null) 
				{
					if (flushOutputMethod()  == false)
						break;
				}


				//if (n<nChunkSize)
				//	break;
				if (n <= 0)
					break;

				lWrited += n;
			}

			return lWrited;
		}

        // 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
        // 如果文件已经存在，则追加在尾部。
        public static void WriteText(string strFileName,
            string strText)
        {
            using (FileStream file = File.Open(
strFileName,
FileMode.Append,	// append
FileAccess.Write,
FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(file,
                    System.Text.Encoding.UTF8))
                {
                    sw.Write(strText);
                }
            }
        }

#if NO
		// 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
		// 如果文件已经存在，则追加在尾部。
		public static void WriteText(string strFileName, 
			string strText)
		{
            using (StreamWriter sw = new StreamWriter(strFileName,
                true,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strText);
            }
		}
#endif



		// 替换文件某一部分
		// 注意，如果当前文件为UNICODE字符集，不要破坏文件开头的UNICODE方向标志
		// parameters:
		//		nStartOffs	被替换区间的开始偏移(以byte计算)
		//		nEndOffs	被替换区间的结束偏移(以byte计算) nEndOffs可以和nStartOffs重合，表示插入内容
		//		baNew		新的内容。如果想删除文件中一段内容，可以用刚构造的新CByteArray对象作为本参数。
		public static int Replace(Stream s,
			long nStartOffs,
			long nEndOffs,
			byte[] baNew)
		{
			long nFileLen;
			long nOldBytes;
			int nNewBytes;
			long nDelta = 0;
			byte[] baBuffer = null;
			int nMaxChunkSize = 4096;
			int nChunkSize;
			long nRightPartLen;
			long nHead;
			// BOOL bRet;
			int nReadBytes/*, nWriteBytes*/;

			// 检测开始与结束部位是否合法
			if (nStartOffs < 0) 
			{
				throw(new Exception("nStartOffs不能小于0"));
			}
			if (nEndOffs < nStartOffs) 
			{
				throw(new Exception("nStartOffs不能大于nEndOffs"));
			}
			nFileLen = s.Length;

			if (nEndOffs > nFileLen) 
			{
				throw(new Exception("nEndOffs不能大于文件长度"));
			}


			// 计算被替换区间原来的长度
			nOldBytes = nEndOffs - nStartOffs;

			// 计算新长度
			nNewBytes = baNew.Length;
	
			if (nNewBytes != nOldBytes)	
			{ // 新旧长度不等，需要移动区间后面的文件内容

				// 根据增加还是减少长度，需要采取不同的移动策略
				// | prev | cur | trail |
				// | prev | cur + delta --> | trial | 先移右方向的块
				// | prev | cur - delta <-- | trial | 先移动左方向的块
				nDelta = nNewBytes - nOldBytes;
				if (nDelta > 0) 
				{	// 整体向右移动， 从右面的块开始作
					nRightPartLen = nFileLen - nStartOffs - nOldBytes;
					nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
					nHead = nFileLen - nChunkSize;
					if (baBuffer == null || baBuffer.Length < nChunkSize) 
					{
						baBuffer = new byte[nChunkSize];
					}
					// baBuffer.SetSize(nChunkSize);
					for(;;) 
					{
						s.Seek(nHead, SeekOrigin.Begin); 
						//SetFilePointer(m_hFile,nHead,NULL,FILE_BEGIN);

						nReadBytes = s.Read(baBuffer, 0, nChunkSize);
						/*
						bRet = ReadFile(m_hFile, baBuffer.GetData(), nChunkSize,
							(LPDWORD)&nReadBytes, NULL);
						if (bRet == FALSE)
							goto ERROR1;
						*/

						s.Seek(nHead+nDelta, SeekOrigin.Begin);
						// SetFilePointer(m_hFile,nHead+nDelta,NULL,FILE_BEGIN);

						s.Write(baBuffer,0, nReadBytes);
						/*
						bRet = WriteFile(m_hFile, baBuffer.GetData(), nReadBytes, 
							(LPDWORD)&nWriteBytes, NULL);
						if (bRet == FALSE)
							goto ERROR1;
						if (nWriteBytes != nReadBytes) 
						{
							// 磁盘满
							m_nErrNo = ERROR_DISK_FULL;
							return -1;
						}
						*/
						// 移动全部完成
						if (nHead <= nStartOffs + nOldBytes)	// < 是为了安全
							break;
						// 移动头位置
						nHead -= nChunkSize;
						if (nHead < nStartOffs + nOldBytes) 
						{
							nChunkSize -= (int)(nStartOffs + nOldBytes - nHead);
							nHead = nStartOffs + nOldBytes;
						}

					}
				}

				if (nDelta < 0) 
				{	// 整体向左移动， 从左面的块开始作
					nRightPartLen = nFileLen - nStartOffs - nOldBytes;
					nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
					if (baBuffer == null || baBuffer.Length < nChunkSize) 
					{
						baBuffer = new byte[nChunkSize];
					}

					//baBuffer.SetSize(nChunkSize);
					nHead = nStartOffs + nOldBytes;
					for(;;) 
					{
						s.Seek(nHead, SeekOrigin.Begin); 
						// SetFilePointer(m_hFile,nHead,NULL,FILE_BEGIN);

						nReadBytes = s.Read(baBuffer, 0, nChunkSize);
						/*
						bRet = ReadFile(m_hFile, baBuffer.GetData(), nChunkSize,
							(LPDWORD)&nReadBytes, NULL);
						if (bRet == FALSE)
							goto ERROR1;
						*/

						s.Seek(nHead+nDelta, SeekOrigin.Begin);
						// SetFilePointer(m_hFile,nHead+nDelta,NULL,FILE_BEGIN);

						s.Write(baBuffer,0, nReadBytes);
						/*
						bRet = WriteFile(m_hFile, baBuffer.GetData(), nReadBytes, 
							(LPDWORD)&nWriteBytes, NULL);
						if (bRet == FALSE)
							goto ERROR1;
						if (nWriteBytes != nReadBytes) 
						{
							// 磁盘满
							m_nErrNo = ERROR_DISK_FULL;
							return -1;
						}
						*/
						// 移动全部完成
						if (nHead + nChunkSize >= nFileLen)
							break;
						// 移动头位置
						nHead += nChunkSize;
						if (nHead + nChunkSize > nFileLen) 
						{ // >是为了安全
							nChunkSize -= (int)(nHead + nChunkSize - nFileLen);
						}

					}
					// 截断文件(因为缩小了文件尺寸)
					//ASSERT(nFileLen+nDelta>=0);
					if (nFileLen + nDelta < 0) 
					{
						throw(new Exception("nFileLen + nDelta < 0"));
					}

					s.SetLength(nFileLen+nDelta);
					/*
					SetFilePointer(m_hFile, nFileLen+nDelta, NULL, FILE_BEGIN);
					SetEndOfFile(m_hFile);
					*/
				}

				//ASSERT(nDelta != 0);
				if (nDelta == 0) 
				{
					throw(new Exception("nDelta == 0"));
				}

			}

			// 将新内容写入

			if (nNewBytes != 0) 
			{
				s.Seek(nStartOffs, SeekOrigin.Begin);
				//SetFilePointer(m_hFile,nStartOffs,NULL,FILE_BEGIN);

				s.Write(baNew,0,nNewBytes);
				/*
				bRet = WriteFile(m_hFile, baNew.GetData(), nNewBytes, 
					(LPDWORD)&nWriteBytes, NULL);
				if (bRet == FALSE)
					goto ERROR1;
				if (nWriteBytes != nNewBytes) 
				{
					// 磁盘满
					m_nErrNo = ERROR_DISK_FULL;
					return -1;
				}
				*/
			}

			/*
			// 恢复写入前的文件指针，并且迫使缓冲区失效
			if (m_nPos > nFileLen + nDelta)
				m_nPos = nFileLen + nDelta;
			SetFilePointer(m_hFile, m_nPos, NULL, FILE_BEGIN);
			m_baCache.RemoveAll();
			m_bEOF = FALSE;
			return 0;
			ERROR1:
				m_nErrNo = GetLastError();
			return -1;
			*/

			return 0;
		}

		// 移动文件某一部分
        // 本函数调用前后，是否保证了文件指针不变?
		// parameters:
		//		nSourceOffs	被移动区间的开始偏移(以byte计算)
		//		nLength		被移动区间的长度(以byte计算)
		//		nTargetOffs	要移动到的目的位置偏移(以byte计算)
		public static int Move(Stream s,
			long nSourceOffs,
			long nLength,
			long nTargetOffs)
		{
			long nFileLen;
			long nDelta = 0;
			byte[] baBuffer = null;
			int nMaxChunkSize = 4096;
			int nChunkSize;
			long nRightPartLen;
			long nHead;
			int nReadBytes;

			// 检测开始与结束部位是否合法
			if (nSourceOffs < 0) 
			{
				throw(new Exception("nSourceOffs不能小于0"));
			}
			if (nTargetOffs < 0) 
			{
				throw(new Exception("nTargetOffs不能小于0"));
			}
			nFileLen = s.Length;

			if (nSourceOffs + nLength > nFileLen) 
			{
				throw(new Exception("移动前区域尾部不能越过文件长度"));
			}


			if (nSourceOffs != nTargetOffs)	
			{

				// 根据增加还是减少长度，需要采取不同的移动策略
				// | prev | block | 
				// | prev | + delta --> | block | 先移右方向的块
				// | prev | - delta <-- | block | 先移动左方向的块
				nDelta = nTargetOffs - nSourceOffs;	//nNewBytes - nOldBytes;
				if (nDelta > 0) 
				{	// 整体向右移动， 从右面的块开始作
					nRightPartLen = nLength;	// nRightMost - nStartOffs - nOldBytes;
					nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
					nHead = nSourceOffs + nLength - nChunkSize;
					if (baBuffer == null || baBuffer.Length < nChunkSize) 
					{
						baBuffer = new byte[nChunkSize];
					}

					for(;;) 
					{
						s.Seek(nHead, SeekOrigin.Begin); 


						nReadBytes = s.Read(baBuffer, 0, nChunkSize);

						s.Seek(nHead+nDelta, SeekOrigin.Begin);


						s.Write(baBuffer,0, nReadBytes);

						// 移动全部完成
						if (nHead <= nSourceOffs)	// < 是为了安全
							break;
						// 移动头位置
						nHead -= nChunkSize;
						if (nHead < nSourceOffs) // 不足一个chunk
						{
							nChunkSize -= (int)(nSourceOffs - nHead);
							if (nChunkSize <= 0) 
							{
								throw(new Exception("nChunkSize小于或等于0!"));
							}
							nHead = nSourceOffs;
						}

					}
				}

				if (nDelta < 0) 
				{	// 整体向左移动， 从左面的块开始作
					nRightPartLen = nLength;	// nRightMost - nStartOffs - nOldBytes;
					nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
					if (baBuffer == null || baBuffer.Length < nChunkSize) 
					{
						baBuffer = new byte[nChunkSize];
					}

					nHead = nSourceOffs;
					for(;;) 
					{
						s.Seek(nHead, SeekOrigin.Begin); 

						nReadBytes = s.Read(baBuffer, 0, nChunkSize);

						s.Seek(nHead+nDelta, SeekOrigin.Begin);


						s.Write(baBuffer,0, nReadBytes);
						// 移动全部完成
						if (nHead + nChunkSize >= nSourceOffs + nLength)
							break;
						// 移动头位置
						nHead += nChunkSize;
						if (nHead + nChunkSize > nSourceOffs + nLength) // 最后一个块，零头
						{ // >是为了安全
							nChunkSize -= (int)(nHead + nChunkSize - (nSourceOffs + nLength));
							if (nChunkSize <= 0) 
							{
								throw(new Exception("nChunkSize小于或等于0!"));
							}
						}

					}


				}

				if (nDelta == 0) 
				{
					throw(new Exception("nDelta == 0"));
				}

			}

			return 0;
		}


	} // end of class StreamUtil


	/// <summary>
	/// Path功能扩展函数
	/// </summary>
	public class PathUtil
	{

        // 获得一个目录下的全部文件的尺寸总和。包括子目录中的
        public static long GetAllFileSize(string strDataDir, ref long count)
        {
            long size = 0;
            DirectoryInfo di = new DirectoryInfo(strDataDir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
                count++;
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                size += GetAllFileSize(subdir.FullName, ref count);
            }

            return size;
        }

        // get clickonce shortcut filename
        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

        public static void DeleteDirectory(string strDirPath)
        {
            try
            {
                Directory.Delete(strDirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            }
        }

        // 移除文件目录内所有文件的 ReadOnly 属性
        public static void RemoveReadOnlyAttr(string strSourceDir)
        {
            string strCurrentDir = Directory.GetCurrentDirectory();

            DirectoryInfo di = new DirectoryInfo(strSourceDir);

            FileSystemInfo[] subs = di.GetFileSystemInfos();

            for (int i = 0; i < subs.Length; i++)
            {

                // 递归
                if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    RemoveReadOnlyAttr(subs[i].FullName);
                }
                else
                    File.SetAttributes(subs[i].FullName, FileAttributes.Normal);

            }
        }

		// 拷贝目录
		public static int CopyDirectory(string strSourceDir,
			string strTargetDir,
			bool bDeleteTargetBeforeCopy,
			out string strError)
		{
			strError = "";

			try 
			{

				DirectoryInfo di = new DirectoryInfo(strSourceDir);

				if (di.Exists == false)
				{
					strError = "源目录 '" + strSourceDir + "' 不存在...";
					return -1;
				}

				if (bDeleteTargetBeforeCopy == true)
				{
					if (Directory.Exists(strTargetDir) == true)
						Directory.Delete(strTargetDir, true);
				}

				CreateDirIfNeed(strTargetDir);


				FileSystemInfo [] subs = di.GetFileSystemInfos();

				for(int i=0;i<subs.Length;i++) 
				{
					// 复制目录
					if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory) 
					{
						int nRet = CopyDirectory(subs[i].FullName,
							strTargetDir + "\\" + subs[i].Name,
							bDeleteTargetBeforeCopy,
							out strError);
						if (nRet == -1)
							return-1;
						continue;
					}
					// 复制文件
					File.Copy(subs[i].FullName, strTargetDir + "\\" + subs[i].Name, true);
				}

			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return -1;
			}


			return 0;
		}

		// 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
		public static bool CreateDirIfNeed(string strDir)
		{
			DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
		}

        // 删除一个目录内的所有文件和目录
        public static bool ClearDir(string strDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDir);
                if (di.Exists == false)
                    return true;

                // 删除所有的下级目录
                DirectoryInfo[] dirs = di.GetDirectories();
                foreach (DirectoryInfo childDir in dirs)
                {
                    Directory.Delete(childDir.FullName, true);
                }

                // 删除所有文件
                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    File.Delete(fi.FullName);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 获得纯文件名部分
		public static string PureName(string strPath)
		{
            // 2012/11/30
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;

			string sResult = "";
			sResult = strPath;
			sResult = sResult.Replace("/", "\\");
			if (sResult.Length > 0) 
			{
				if (sResult[sResult.Length-1] == '\\')
					sResult = sResult.Substring(0, sResult.Length - 1);
			}
			int nRet = sResult.LastIndexOf("\\");
			if (nRet != -1)
				sResult = sResult.Substring(nRet + 1);

			return sResult;
		}

		public static string PathPart(string strPath)
		{
			string sResult = "";
			sResult = strPath;
			sResult = sResult.Replace("/", "\\");
			if (sResult.Length > 0) 
			{
				if (sResult[sResult.Length-1] == '\\')
					sResult = sResult.Substring(0, sResult.Length - 1);
			}
			int nRet = sResult.LastIndexOf("\\");
			if (nRet != -1)
				sResult = sResult.Substring(0, nRet);
			else
				sResult = "";

			return sResult;
		}

		public static string MergePath(string s1, string s2)
		{
			string sResult = "";

			if (s1 != null) 
			{
				sResult = s1;
				sResult = sResult.Replace("/", "\\");
				if (sResult != "") 
				{
					if (sResult[sResult.Length -1] != '\\')
						sResult += "\\";
				}
				else 
				{
					sResult += "\\";
				}
			}
			if (s2 != null) 
			{
				s2 = s2.Replace("/","\\");
				if (s2 != "") 
				{
					if (s2[0] == '\\')
						s2 = s2.Remove(0,1);
					sResult += s2;
				}

			}

			return sResult;
		}

        // 正规化目录路径名。把所有字符'/'替换为'\'，并且为末尾确保有字符'\'
        public static string CanonicalizeDirectoryPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "";

            strPath = strPath.Replace('/', '\\');

            if (strPath[strPath.Length - 1] != '\\')
                strPath += "\\";

            return strPath;
        }

		// 测试strPath1是否为strPath2的下级目录或文件
		//	strPath1正好等于strPath2的情况也返回true
		public static bool IsChildOrEqual(string strPath1, string strPath2)
		{
			FileSystemInfo fi1 = new DirectoryInfo(strPath1);

			FileSystemInfo fi2 = new DirectoryInfo(strPath2);

			string strNewPath1 = fi1.FullName;
			string strNewPath2 = fi2.FullName;

			if (strNewPath1.Length != 0) 
			{
				if (strNewPath1[strNewPath1.Length-1] != '\\')
					strNewPath1 += "\\";
			}
			if (strNewPath2.Length != 0) 
			{
				if (strNewPath2[strNewPath2.Length-1] != '\\')
					strNewPath2 += "\\";
			}

			// 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
			if (strNewPath1.Length < strNewPath2.Length)
				return false;


			// 截取路径1中前面一段进行比较
			string strPart = strNewPath1.Substring(0, strNewPath2.Length);
			strPart.ToUpper();
			strNewPath2.ToUpper();

			if (strPart != strNewPath2)
				return false;

			return true;
		}

		// 测试strPath1是否和strPath2为同一文件或目录
		public static bool IsEqual(string strPath1, string strPath2)
		{
            if (String.IsNullOrEmpty(strPath1) == true
                && String.IsNullOrEmpty(strPath2) == true)
                return true;

            if (String.IsNullOrEmpty(strPath1) == true)
                return false;

            if (String.IsNullOrEmpty(strPath2) == true)
                return false;

            if (strPath1 == strPath2)
                return true;

            FileSystemInfo fi1 = new DirectoryInfo(strPath1);
			FileSystemInfo fi2 = new DirectoryInfo(strPath2);

			string strNewPath1 = fi1.FullName;
			string strNewPath2 = fi2.FullName;

			if (strNewPath1.Length != 0) 
			{
				if (strNewPath1[strNewPath1.Length-1] != '\\')
					strNewPath1 += "\\";
			}
			if (strNewPath2.Length != 0) 
			{
				if (strNewPath2[strNewPath2.Length-1] != '\\')
					strNewPath2 += "\\";
			}

			if (strNewPath1.Length != strNewPath2.Length)
				return false;

			strNewPath1.ToUpper();
			strNewPath2.ToUpper();

			if (strNewPath1 == strNewPath2)
				return true;

			return false;
		}

        // 测试strPath1是否和strPath2为同一文件或目录
        public static bool IsEqualEx(string strPath1, string strPath2)
        {
            string strNewPath1 = strPath1;
            string strNewPath2 = strPath2;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            if (strNewPath1.Length != strNewPath2.Length)
                return false;

            strNewPath1.ToUpper();
            strNewPath2.ToUpper();

            if (strNewPath1 == strNewPath2)
                return true;

            return false;
        }


		public static string EnsureTailBackslash(string strPath)
		{
			if (strPath == "")
				return "\\";

			string sResult = "";

			sResult = strPath.Replace("/", "\\");

			if (sResult[sResult.Length-1] != '\\')
				sResult += "\\";

			return sResult;
		}

		// 末尾是否有'\'。如果具备，表示这是一个目录路径。
		public static bool HasTailBackslash(string strPath)
		{
			if (strPath == "")
				return true;	// 理解为'\'

			string sResult = "";

			sResult = strPath.Replace("/", "\\");

			if (sResult[sResult.Length-1] == '\\')
				return true;

			return false;
		}


		// 测试strPathChild是否为strPathParent的下级目录或文件
		// 如果是下级，则将strPathChild中和strPathParent重合的部分替换为
		// strMacro中的宏字符串返回在strResult中，并且函数返回true。
		// 否则函数返回false，strResult虽返回内容，但不替换。
		//	strPath1正好等于strPath2的情况也返回true
		// 
		// Exception:
		//	System.NotSupportedException
		// Testing:
		//	在testIO.exe中
		public static bool MacroPathPart(string strPathChild,
			string strPathParent,
			string strMacro,
			out string strResult)
		{
			strResult = strPathChild;

			FileSystemInfo fiChild = new DirectoryInfo(strPathChild);

			FileSystemInfo fiParent = new DirectoryInfo(strPathParent);

			string strNewPathChild = fiChild.FullName;
			string strNewPathParent = fiParent.FullName;

			if (strNewPathChild.Length != 0) 
			{
				if (strNewPathChild[strNewPathChild.Length-1] != '\\')
					strNewPathChild += "\\";
			}
			if (strNewPathParent.Length != 0) 
			{
				if (strNewPathParent[strNewPathParent.Length-1] != '\\')
					strNewPathParent += "\\";
			}

			// 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
			if (strNewPathChild.Length < strNewPathParent.Length)
				return false;


			// 截取路径1中前面一段进行比较
			string strPart = strNewPathChild.Substring(0, strNewPathParent.Length);
			strPart.ToUpper();
			strNewPathParent.ToUpper();

			if (strPart != strNewPathParent)
				return false;

			strResult = strMacro + "\\" + fiChild.FullName.Substring(strNewPathParent.Length);
			// fiChild.FullName是尾部未加'\'以前的形式

			return true;
		}

		// 将路径中的%%宏部分替换为具体内容
		// parameters:
		//		macroTable	宏名和内容的对照表
		//		bThrowMacroNotFoundException	是否抛出MacroNotFoundException异常
		// Exception:
		//	MacroNotFoundException
		//	MacroNameException	函数NextMacro()可能抛出
		// Testing:
		//	在testIO.exe中
		public static string UnMacroPath(Hashtable macroTable,
			string strPath,
			bool bThrowMacroNotFoundException)
		{
			int nCurPos = 0;
			string strPart = "";

			string strResult = "";

			for(;;) 
			{
				strPart = NextMacro(strPath, ref nCurPos);
				if (strPart == "")
					break;

				if (strPart[0] == '%') 
				{
					string strValue = (string)macroTable[strPart];
					if (strValue == null) 
					{
						if (bThrowMacroNotFoundException) 
						{
							MacroNotFoundException ex = new MacroNotFoundException("macro " + strPart + " not found in macroTable");
							throw ex;
						}
						else 
						{
							// 将没有找到的宏放回结果中
							strResult += strPart;
							continue;
						}
					}

					strResult += strValue;
				}
				else 
				{
					strResult += strPart;
				}

			}

			return strResult;
		}

		// 本函数为UnMacroPath()的服务函数
		// 顺次得到下一个部分
		// nCurPos在第一次调用前其值必须设置为0，然后，调主不要改变其值
		// Exception:
		//	MacroNameException
		static string NextMacro(string strText,
			ref int nCurPos)
		{
			if (nCurPos >= strText.Length)
				return "";

			string strResult = "";
			bool bMacro = false;	// 本次是否在macro上

			if (strText[nCurPos] == '%')
				bMacro = true;

			int nRet = -1;
			
			if (bMacro == false)
				nRet = strText.IndexOf("%", nCurPos);
			else
				nRet = strText.IndexOf("%", nCurPos+1);

			if (nRet == -1) 
			{
				strResult = strText.Substring(nCurPos);
				nCurPos = strText.Length;
				if (bMacro == true) 
				{
					// 这是异常情况，表明%只有头部一个
					throw(new MacroNameException("macro " + strResult + " format error"));
				}
				return strResult;
			}

			if (bMacro == true) 
			{
				strResult = strText.Substring(nCurPos, nRet - nCurPos + 1);
				nCurPos = nRet + 1;
				return strResult;
			}
			else 
			{
				Debug.Assert(strText[nRet] == '%', "当前位置不是%，异常");
				strResult = strText.Substring(nCurPos, nRet - nCurPos);
				nCurPos = nRet;
				return strResult;
			}

		}

		public static string GetShortFileName(string strFileName)
		{
			StringBuilder shortPath = new StringBuilder(300);
			int nRet = API.GetShortPathName(
				strFileName,
				shortPath,
				shortPath.Capacity);
			if (nRet == 0 || nRet >= 300) 
			{
				// MessageBox.Show("file '" +strFileName+ "' get short error");
				// return strFileName;
				throw(new Exception("GetShortFileName error"));
			}

			return shortPath.ToString(); 
		}


	}

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


	/// <summary>
	/// File功能扩展函数
	/// </summary>
	public class FileUtil
	{
        // 检测字符串是否为纯数字(不包含'-','.'号)
        public static bool IsPureNumber(string s)
        {
            if (s == null)
                return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > '9' || s[i] < '0')
                    return false;
            }
            return true;
        }

        // 文件扩展名是否为 ._1 ._2 ...
        public static bool IsBackupFile(string strFileName)
        {
            string strExt = Path.GetExtension(strFileName);
            if (string.IsNullOrEmpty(strExt) == true)
                return false;
            if (strExt.StartsWith("._") == false)
                return false;
            string strNumber = strExt.Substring(2);
            if (IsPureNumber(strNumber) == true)
                return true;

            return false;
        }
        /// <summary>
        /// 获得文件的长度
        /// </summary>
        /// <param name="strFileName">文件名全路径</param>
        /// <returns>返回文件长度。如果是 -1，表示文件不存在</returns>
        public static long GetFileLength(string strFileName)
        {
            FileInfo fi = new FileInfo(strFileName);
            if (fi.Exists == false)
                return -1;
            return fi.Length;
        }
        // 根据时间戳信息，设置文件的最后修改时间
        public static void SetFileLastWriteTimeByTimestamp(string strFilePath,
            byte[] baTimeStamp)
        {
            if (baTimeStamp == null || baTimeStamp.Length < 8)
                return;
#if NO
            byte [] baTime = new byte[8];
            Array.Copy(baTimeStamp,
    0,
    baTime,
    0,
    8);
#endif
            long lTicks = BitConverter.ToInt64(baTimeStamp, 0);

            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return;
            fileInfo.LastWriteTimeUtc = new DateTime(lTicks);
        }

        // 根据文件的最后修改时间和尺寸, 获得时间戳信息
        public static byte[] GetFileTimestamp(string strFilePath)
        {
            byte[] baTimestamp = null;
            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return baTimestamp;

            long lTicks = fileInfo.LastWriteTimeUtc.Ticks;
            byte[] baTime = BitConverter.GetBytes(lTicks);

            byte[] baLength = BitConverter.GetBytes((long)fileInfo.Length);
            //Array.Reverse(baLength);

            baTimestamp = new byte[baTime.Length + baLength.Length];
            Array.Copy(baTime,
                0,
                baTimestamp,
                0,
                baTime.Length);
            Array.Copy(baLength,
                0,
                baTimestamp,
                baTime.Length,
                baLength.Length);

            return baTimestamp;
        }

        // 写入日志文件。每天创建一个单独的日志文件
        public static void WriteErrorLog(
            object lockObj,
            string strLogDir,
            string strText,
            string strPrefix = "log_",
            string strPostfix = ".txt")
        {

            lock (lockObj)
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFilename = PathUtil.MergePath(strLogDir, strPrefix + DateTimeUtil.DateTimeToString8(now) + strPostfix);
                string strTime = now.ToString();
                StreamUtil.WriteText(strFilename,
                    strTime + " " + strText + "\r\n");
            }
        }

        // 能自动识别文件内容的编码方式的读入文本文件内容模块
        // parameters:
        //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
        // return:
        //      -1  出错 strError中有返回值
        //      0   文件不存在 strError中有返回值
        //      1   文件存在
        //      2   读入的内容不是全部
        public static int ReadTextFileContent(string strFilePath,
            long lMaxLength,
            out string strContent,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            strContent = "";
            encoding = null;

            if (File.Exists(strFilePath) == false)
            {
                strError = "文件 '" + strFilePath + "' 不存在";
                return 0;
            }

            encoding = FileUtil.DetectTextFileEncoding(strFilePath);

            try
            {
                bool bExceed = false;

                using (FileStream file = File.Open(
        strFilePath,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite))
                {
                    // TODO: 这里的自动探索文件编码方式功能不正确，
                    // 需要专门编写一个函数来探测文本文件的编码方式
                    // 目前只能用UTF-8编码方式
                    using (StreamReader sr = new StreamReader(file, encoding))
                    {
                        if (lMaxLength == -1)
                        {
                            strContent = sr.ReadToEnd();
                        }
                        else
                        {
                            long lLoadedLength = 0;
                            StringBuilder temp = new StringBuilder(4096);
                            for (; ; )
                            {
                                string strLine = sr.ReadLine();
                                if (strLine == null)
                                    break;
                                if (lLoadedLength + strLine.Length > lMaxLength)
                                {
                                    strLine = strLine.Substring(0, (int)(lMaxLength - lLoadedLength));
                                    temp.Append(strLine + " ...");
                                    bExceed = true;
                                    break;
                                }

                                temp.Append(strLine + "\r\n");
                                lLoadedLength += strLine.Length + 2;
                                if (lLoadedLength > lMaxLength)
                                {
                                    temp.Append(strLine + " ...");
                                    bExceed = true;
                                    break;
                                }
                            }
                            strContent = temp.ToString();
                        }
                        /*
                    sr.Close();
                    sr = null;
                         * */
                    }
                }

                if (bExceed == true)
                    return 2;
            }
            catch (Exception ex)
            {
                strError = "打开或读入文件 '" + strFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            return 1;
        }

        // 获得一个临时文件名
        // parameters:
        //      strPostFix  如果用于表示文件扩展名，应包含点。否则可以当作一般后缀字符串使用
        public static string NewTempFileName(string strDir,
            string strPrefix,
            string strPostFix)
        {
            if (string.IsNullOrEmpty(strDir) == true)
            {
                return Path.GetTempFileName();
            }

            string strFileName = "";
            for (int i = 0; ; i++)
            {
                strFileName = PathUtil.MergePath(strDir, strPrefix + Convert.ToString(i) + strPostFix);

                FileInfo fi = new FileInfo(strFileName);
                if (fi.Exists == false)
                {
                    // 创建一个0 byte的文件
                    FileStream f = File.Create(strFileName);
                    f.Close();
                    return strFileName;
                }
            }
        }

		// 把一个byte[] 写到指定的文件
		// parameter:
		//		strFileName: 文件名,每次新创建,覆盖原来的文件
		//		data: byte数组
		// 编写者: 任延华
		public static void WriteByteArray(string strFileName,
			byte[] data)
		{
			FileStream s = File.Create(strFileName);
			try
			{
				s.Write(data,
					0,
					data.Length);
			}
			finally
			{
				s.Close();
			}
		}

		// 改动文件的“最后修改时间”属性
		// 注意可能抛出异常
		public static void ChangeFileLastModifyTimeUtc(string strPath,
			string strTime)
		{
			FileInfo fi = new FileInfo(strPath);
			DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
			fi.LastWriteTimeUtc = time;
			fi.CreationTimeUtc = time;
		}

        // 文件是否存在并且是非空
        public static bool IsFileExsitAndNotNull(string strFilename)
        {

            try
            {
                FileStream file = File.Open(
                    strFilename,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

                if (file.Length == 0)
                    return false;

                file.Close();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

		// 文件是否存在?
		public static bool FileExist(string strFilePath)
		{
			FileInfo fi = new FileInfo(strFilePath);
			return fi.Exists;
		}


		// 功能:将一个字符串写到文件,当文件不存在时,创建文件;存在,覆盖文件
		// paramter:
		//		strContent:  字符串
		//		strFileName: 文件名
		// 编写者：任延华
		public static void String2File(string strContent,
			string strFileName)
		{
			StreamWriter s  = File.CreateText(strFileName);
			try
			{
				s.Write(strContent);
			}
			finally
			{
				s.Close();
			}
		}


		// 功能:文件到字符串，使用直接读到尾的方法
		// strFileName: 文件名
		public static string File2StringE(string strFileName)
		{
			if (strFileName == null
				|| strFileName == "")
				return "";
			StreamReader sr = new StreamReader(strFileName, true);
			string strText = sr.ReadToEnd();
			sr.Close();

			return strText;
		}

        // 如果未能探测出来，则当作 936
        public static Encoding DetectTextFileEncoding(string strFilename)
        {
            Encoding encoding = DetectTextFileEncoding(strFilename, null);
            if (encoding == null)
                return Encoding.GetEncoding(936);    // default

            return encoding;
        }

        // 检测文本文件的encoding
        /*
UTF-8: EF BB BF 
UTF-16 big-endian byte order: FE FF 
UTF-16 little-endian byte order: FF FE 
UTF-32 big-endian byte order: 00 00 FE FF 
UTF-32 little-endian byte order: FF FE 00 00 
         * */
        public static Encoding DetectTextFileEncoding(string strFilename,
            Encoding default_encoding)
        {

            byte[] buffer = new byte[4];

            try
            {
                FileStream file = File.Open(
        strFilename,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite);
                try
                {

                    if (file.Length >= 2)
                    {
                        file.Read(buffer, 0, 2);    // 1, 2 BUG

                        if (buffer[0] == 0xff && buffer[1] == 0xfe)
                        {
                            return Encoding.Unicode;    // little-endian
                        }

                        if (buffer[0] == 0xfe && buffer[1] == 0xff)
                        {
                            return Encoding.BigEndianUnicode;
                        }
                    }

                    if (file.Length >= 3)
                    {
                        file.Read(buffer, 2, 1);
                        if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                        {
                            return Encoding.UTF8;
                        }

                    }

                    if (file.Length >= 4)
                    {
                        file.Read(buffer, 3, 1);

                        // UTF-32 big-endian byte order: 00 00 FE FF 
                        // UTF-32 little-endian byte order: FF FE 00 00 

                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
                        {
                            return Encoding.UTF32;    // little-endian
                        }

                        if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
                        {
                            return Encoding.GetEncoding(65006);    // UTF-32 big-endian
                        }
                    }

                }
                finally
                {
                    file.Close();
                }
            }
            catch
            {
            }

            return default_encoding;    // default
        }
	}

}
