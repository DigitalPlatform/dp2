using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Esendex.TokenBucket;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和带宽限制有关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        static ITokenBucket _bucketDownload = null;

        static ITokenBucket _bucketUpload = null;

#if REMOVED
        public long DownloadBandwidth = -1;

        // 初始化限速参数
        public void InitialRateLimit()
        {
            if (DownloadBandwidth == -1)
                _bucketDownload = null;
            else
            {
                /*
                // Create a token bucket with a capacity of 40 kb tokens that refills at a fixed interval of 20 kb tokens per second
                _bucketDownload = TokenBuckets.Construct()
                  .WithCapacity(40960)
                  .WithFixedIntervalRefillStrategy(20480, TimeSpan.FromSeconds(1))
                  .Build();
                */
                _bucketDownload = TokenBuckets.Construct()
  .WithCapacity(DownloadBandwidth)
  .WithFixedIntervalRefillStrategy(DownloadBandwidth, TimeSpan.FromSeconds(1))
  .Build();
            }
        }

#endif

        #region Download Bandwidth

        static long _downloadBandwidth = -1;

        // 初始化限速参数
        public static long DownloadBandwidth
        {
            get
            {
                return _downloadBandwidth;
            }
            set
            {
                _downloadBandwidth = value;

                if (value == -1)
                    _bucketDownload = null;
                else
                {
                    /*
                    // Create a token bucket with a capacity of 40 kb tokens that refills at a fixed interval of 20 kb tokens per second
                    _bucketDownload = TokenBuckets.Construct()
                      .WithCapacity(40960)
                      .WithFixedIntervalRefillStrategy(20480, TimeSpan.FromSeconds(1))
                      .Build();
                    */
                    _bucketDownload = TokenBuckets.Construct()
      .WithCapacity(value)
      .WithFixedIntervalRefillStrategy(value, TimeSpan.FromSeconds(1))
      .Build();
                }
            }
        }


        // 消费
        public ConsumeResult ComsumeDownload(long numTokens,
            TimeSpan timeout,
            CancellationToken token)
        {
            if (_bucketDownload == null)
                return new ConsumeResult { Tokens = numTokens };

            if (numTokens == 0)
                return new ConsumeResult { Tokens = numTokens };

            double ratio = numTokens / DownloadBandwidth;

            /*
            // 检查超时
            if (ratio > timeout.TotalSeconds)
                throw new TimeoutException($"预计延时会超过 {timeout.ToString()} 超时长度");
            */

            if (ratio < 0.5)
                _bucketDownload.Consume(numTokens);
            else
            {
                var start_time = DateTime.Now;
                long rest = numTokens;
                while(rest > 0 && token.IsCancellationRequested == false)
                {
                    if (DateTime.Now - start_time > timeout)
                    {
                        // throw new TimeoutException($"超过 {timeout.ToString()} 超时长度");
                        return new ConsumeResult
                        {
                            Value = -1,
                            ErrorCode = "overflow",
                            Tokens = numTokens - rest,
                            ErrorInfo = $"超过 {timeout.ToString()} 超时长度",
                        };
                    }

                    long step = DownloadBandwidth / 2;
                    if (step == 0)
                        throw new Exception("step 等于零");
                    if (rest - step < 0)
                        step = 0;
                    if (step == 0)
                        break;
                    _bucketDownload.Consume(step);
                    rest -= step;
                }
            }

            return new ConsumeResult { Tokens = numTokens };
        }

#endregion

        public static byte [] GetRange(byte [] source, int count)
        {
            List<byte> content = new List<byte>(source);
            return content.GetRange(0, count).ToArray();
        }

        #region Upload Bandwidth

        static long _uploadBandwidth = -1;

        // 初始化限速参数
        public static long UploadBandwidth
        {
            get
            {
                return _uploadBandwidth;
            }
            set
            {
                _uploadBandwidth = value;

                if (value == -1)
                    _bucketUpload = null;
                else
                {
                    _bucketUpload = TokenBuckets.Construct()
      .WithCapacity(value)
      .WithFixedIntervalRefillStrategy(value, TimeSpan.FromSeconds(1))
      .Build();
                }
            }
        }

        // 消费
        public ConsumeResult ComsumeUpload(long numTokens,
            TimeSpan timeout,
            CancellationToken token)
        {
            if (_bucketUpload == null)
                return new ConsumeResult { Tokens = numTokens };

            if (numTokens == 0)
                return new ConsumeResult { Tokens = numTokens };

            double ratio = numTokens / UploadBandwidth;

            /*
            // 检查超时
            if (ratio > timeout.TotalSeconds)
                throw new TimeoutException($"预计延时会超过 {timeout.ToString()} 超时长度");
            */

            if (ratio < 0.5)
                _bucketUpload.Consume(numTokens);
            else
            {
                var start_time = DateTime.Now;
                long rest = numTokens;
                while (rest > 0 && token.IsCancellationRequested == false)
                {
                    if (DateTime.Now - start_time > timeout)
                    {
                        // throw new TimeoutException($"超过 {timeout.ToString()} 超时长度");
                        return new ConsumeResult
                        {
                            Value = -1,
                            ErrorCode = "overflow",
                            Tokens = numTokens - rest,
                            ErrorInfo = $"超过 {timeout.ToString()} 超时长度",
                        };
                    }

                    long step = UploadBandwidth / 2;
                    if (step == 0)
                        throw new Exception("step 等于零");
                    if (rest - step < 0)
                        step = 0;
                    if (step == 0)
                        break;
                    _bucketUpload.Consume(step);
                    rest -= step;
                }
            }

            return new ConsumeResult { Tokens = numTokens };
        }

        #endregion
    }

    public class ConsumeResult : NormalResult
    {
        public long Tokens { get; set; }
    }
}
