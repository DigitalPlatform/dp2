using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DigitalPlatform.IO
{
	/// <summary>
	/// 操纵 .dp2bak 格式的文件
	/// </summary>
	public class Backup
	{

		// 将第一个Res的全部资源组合成完整的Res数据,写入流中
		// 调用本函数前，注意把文件指针设置到适当位置(例如文件的末尾，或者要覆盖位置的开始)。
		public static long WriteFirstResToBackupFile(Stream outputfile,
			string strMetaData,
			string strBody)
		{
			long lStart = outputfile.Position;

			outputfile.Seek(8, SeekOrigin.Current);

			// 写入metadata的长度, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

			outputfile.LockingWrite(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// 写入metadata内容
			outputfile.LockingWrite(data, 0, data.Length);

			// 准备Body数据
			data = Encoding.UTF8.GetBytes(strBody);

			// 写入body的长度, 8bytes
			long lBodyLength = data.Length;
			outputfile.LockingWrite(BitConverter.GetBytes(lBodyLength), 0, 8);

			// 写入body内容
            outputfile.LockingWrite(data, 0, data.Length);

			long lTotalLength = outputfile.Position - lStart - 8;	// 净长度

			// 最后写开始的总长度
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

            // outputfile.Seek(lStart, SeekOrigin.Begin);     // 文件大了以后这句话的性能会很差
            outputfile.LockingWrite(BitConverter.GetBytes(lTotalLength), 0, 8);

			// 收尾,为后面继续写设置好文件指针
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return lTotalLength + 8;	// 返回毛长度
		}

		// 将第一个Res以外的Res数据,写入流中
		// 调用本函数前，注意把文件指针设置到适当位置(例如文件的末尾，或者要覆盖位置的开始)。
		public static long WriteOtherResToBackupFile(Stream outputfile,
			string strMetaData,
			string strBodyFileName)
		{
			long lStart = outputfile.Position;

			outputfile.Seek(8, SeekOrigin.Current);

			// 写入metadata的长度, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

            outputfile.LockingWrite(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// 写入metadata内容
            outputfile.LockingWrite(data, 0, data.Length);

            using (FileStream fileSource = File.Open(
                strBodyFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                // body的长度, 8bytes
                long lBodyLength = fileSource.Length;
                outputfile.LockingWrite(BitConverter.GetBytes(lBodyLength), 0, 8);

                // body内容
                StreamUtil.LockingDumpStream(fileSource, outputfile, false);
            }

			long lTotalLength = outputfile.Position - lStart - 8;	// 净长度

			// 最后写开始的总长度
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

			// outputfile.Seek(lStart, SeekOrigin.Begin);   // 性能
            outputfile.LockingWrite(BitConverter.GetBytes(lTotalLength), 0, 8);

			// 收尾,为后面继续写设置好文件指针
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return lTotalLength + 8;	// 返回毛长度
		}

		// 写res的头。
		// 如果不能预先确知整个res的长度，可以用随便一个lTotalLength值调用本函数，
		// 但是需要记忆下函数所返回的lStart，最后调用EndWriteResToBackupFile()。
		// 如果能预先确知整个res的长度，则最后不必调用EndWriteResToBackupFile()
		public static long BeginWriteResToBackupFile(Stream outputfile,
			long lTotalLength,
			out long lStart)
		{
			lStart = outputfile.Position;

			outputfile.LockingWrite(BitConverter.GetBytes(lTotalLength), 0, 8);

			return 0;
		}

		public static long EndWriteResToBackupFile(
			Stream outputfile,
			long lTotalLength,
			long lStart)
		{
			// 最后写开始的总长度
            outputfile.Seek(lStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lStart, "");

			// outputfile.Seek(lStart, SeekOrigin.Begin);   // 性能
            outputfile.LockingWrite(BitConverter.GetBytes(lTotalLength), 0, 8);

			// 收尾,为后面继续写设置好文件指针
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return 0;
		}

		public static long WriteResMetadataToBackupFile(Stream outputfile,
			string strMetaData)
		{
			// 写入metadata的长度, 8bytes
			byte[] data = Encoding.UTF8.GetBytes(strMetaData);
			long lMetaDataLength = data.Length;

            outputfile.LockingWrite(BitConverter.GetBytes(lMetaDataLength), 0, 8);

			// 写入metadata内容
            outputfile.LockingWrite(data, 0, data.Length);

			return 0;
		}

		// 写res body的头。
		// 如果不能预先确知body的长度，可以用随便一个lBodyLength值调用本函数，
		// 但是需要记忆下函数所返回的lBodyStart，最后调用EndWriteResBodyToBackupFile()。
		// 如果能预先确知body的长度，则最后不必调用EndWriteResBodyToBackupFile()
		// parameters:
		//		lBodyStart	返回res body尚未写但是即将写的位置，也就是尚未写8byte尺寸的位置
		public static long BeginWriteResBodyToBackupFile(
			Stream outputfile,
			long lBodyLength,
			out long lBodyStart)
		{
			lBodyStart = outputfile.Position;

            outputfile.LockingWrite(BitConverter.GetBytes(lBodyLength), 0, 8);
			return 0;
		}

		// res body收尾
		public static long EndWriteResBodyToBackupFile(
			Stream outputfile,
			long lBodyLength,
			long lBodyStart)
		{
			// 最后写开始的总长度
            outputfile.Seek(lBodyStart - outputfile.Position, SeekOrigin.Current);
            Debug.Assert(outputfile.Position == lBodyStart, "");

			// outputfile.Seek(lBodyStart, SeekOrigin.Begin);  // 性能
            outputfile.LockingWrite(BitConverter.GetBytes(lBodyLength), 0, 8);

			// 收尾,为后面继续写设置好文件指针
			outputfile.Seek(lBodyLength, SeekOrigin.Current);

			return 0;
		}

		public Backup()
		{
			//
			// TODO: Add constructor logic here
			//
		}
	}
}
