using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;

namespace FaceDrivers.Baidu
{
    public class Driver : BioUtil
    {
        #region API

        // sdk初始化
        [DllImport("BaiduFaceApi.dll",
            EntryPoint = "sdk_init",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int sdk_init(string model_path);

        // 是否授权
        [DllImport("BaiduFaceApi.dll",
            EntryPoint = "is_auth",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern bool is_auth();

        // sdk销毁
        [DllImport("BaiduFaceApi.dll",
            EntryPoint = "sdk_destroy",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void sdk_destroy();

        // 获取设备指纹
        [DllImport("BaiduFaceApi.dll",
            EntryPoint = "get_device_id",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_device_id();

        // 获取sdk版本号
        [DllImport("BaiduFaceApi.dll",
            EntryPoint = "sdk_version",
            CharSet = CharSet.Ansi,
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr sdk_version();


        #endregion

        public override string DriverName
        {
            get
            {
                return "Baidu";
            }
        }

        // 算法版本号
        public override string AlgorithmVersion
        {
            get
            {
                return "8.4";
            }
        }

        public override string BioTypeName
        {
            get
            {
                return "人脸";
            }
        }

        // ILog Log = null;

        public Driver()
        {
            BrowseStyle = "face";
            SearchFrom = "人脸时间戳";
            ElementName = "face";

            // Log = LogManager.GetLogger("main", "driver1");
        }

        // 初始化
        public override NormalResult Init(int dev_index)
        {
            try
            {
                return InitEngines();
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
        }

        public override NormalResult Free()
        {
            // 使用完毕，销毁sdk，释放内存
            sdk_destroy();

            return new NormalResult();
        }

        // https://stackoverflow.com/questions/8836093/how-can-i-specify-a-dllimport-path-at-runtime
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        private NormalResult InitEngines()
        {
            var is64CPU = Environment.Is64BitProcess;

            // Log.Debug($"is64CPU={is64CPU}");

            /*
            {
                string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string assemblyPath = Path.Combine(folderPath, IntPtr.Size == 8 ? "x64" : "x86");

                SetDllDirectory(assemblyPath);
            }
            */

            string model_path = null;
            // string model_path="d:\\face";
            int n = sdk_init(model_path);

            // Console.WriteLine("sdk init cost {0:D}", time_end - time_begin);
            //若没通过初始化，则n不为0, 返回的错误码及原因可参考说明文档
            if (n != 0)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"sdk init fail and errcode is {n}",
                    ErrorCode = n.ToString()
                };
            }
            // 获取设备指纹
            get_sdk_info();
            // 验证是否授权
            bool authed = is_auth();
            if (authed == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "SDK 尚未授权"
                };
            Console.WriteLine("authed res is:" + authed);

            return new NormalResult();
        }

        // 获取sdk信息如设备指纹，版本号
        static void get_sdk_info()
        {
            // 获取设备指纹
            IntPtr ptr = get_device_id();
            string buf = Marshal.PtrToStringAnsi(ptr);
            Console.WriteLine("device id is:" + buf);
            // 获取sdk版本号
            IntPtr ptr_v = sdk_version();
            string vbuf = Marshal.PtrToStringAnsi(ptr_v);
            Console.WriteLine("sdk version is:" + vbuf);
        }

        class GetFeatureResult : NormalResult
        {
            public IntPtr FeaturePtr { get; set; }
        }

        GetFeatureResult GetFeature(Image image)
        {
            Lock();
            try
            {
                return _getFeature(image);
            }
            finally
            {
                Unlock();
            }
        }

        GetFeatureResult _getFeature(Image srcImage0)
        {
            StringBuilder debugInfo = new StringBuilder();

            Image srcImage = null;
            //调整图像宽度，需要宽度为4的倍数
            if (srcImage0.Width % 4 != 0)
            {
                //srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                srcImage = ImageUtil.ScaleImage(srcImage0, srcImage0.Width - (srcImage0.Width % 4), srcImage0.Height);
            }
            else
                srcImage = ImageUtil.ScaleImage(srcImage0, srcImage0.Width - (srcImage0.Width % 4), srcImage0.Height);

            using (srcImage)
            {
                //调整图片数据，非常重要
                using (ImageInfo imageInfo = ImageUtil.ReadBMP(srcImage))
                {
                    //人脸检测
                    ASF_MultiFaceInfo multiFaceInfo = FaceUtil.DetectFace(pImageEngine,
                        imageInfo);
                    //年龄检测
                    int retCode_Age = -1;
                    ASF_AgeInfo ageInfo = FaceUtil.AgeEstimation(pImageEngine,
                        imageInfo,
                        multiFaceInfo,
                        out retCode_Age);
                    //性别检测
                    int retCode_Gender = -1;
                    ASF_GenderInfo genderInfo = FaceUtil.GenderEstimation(pImageEngine,
                        imageInfo,
                        multiFaceInfo,
                        out retCode_Gender);

                    //3DAngle检测
                    int retCode_3DAngle = -1;
                    ASF_Face3DAngle face3DAngleInfo = FaceUtil.Face3DAngleDetection(pImageEngine,
                        imageInfo,
                        multiFaceInfo,
                        out retCode_3DAngle);

                    // MemoryUtil.Free(imageInfo.imgData);

                    if (multiFaceInfo.faceNum < 1)
                    {
                        /*
                        srcImage = ImageUtil.ScaleImage(srcImage, picImageCompare.Width, picImageCompare.Height);
                        image1Feature = IntPtr.Zero;
                        picImageCompare.Image = srcImage;
                        AppendText(string.Format("{0} - 未检测出人脸!\n\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));
                        AppendText(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                        AppendText("\n");
                        return;
                        */
                        // throw new Exception("未检测出人脸");
                        return new GetFeatureResult
                        {
                            Value = -1,
                            FeaturePtr = IntPtr.Zero,
                            ErrorInfo = "未检测出人脸"
                        };
                        // return IntPtr.Zero;
                    }

                    MRECT temp = new MRECT();
                    int ageTemp = 0;
                    int genderTemp = 0;
                    int rectTemp = 0;

                    //标记出检测到的人脸
                    for (int i = 0; i < multiFaceInfo.faceNum; i++)
                    {
                        // 这里曾经出现过尝试写入受保护的内存异常
                        MRECT rect = MemoryUtil.PtrToStructure<MRECT>(multiFaceInfo.faceRects + MemoryUtil.SizeOf<MRECT>() * i);
                        int orient = MemoryUtil.PtrToStructure<int>(multiFaceInfo.faceOrients + MemoryUtil.SizeOf<int>() * i);
                        int age = 0;

                        if (retCode_Age != 0)
                        {
                            debugInfo.Append($"年龄检测失败，返回{retCode_Age}!\n\n");
                        }
                        else
                        {
                            age = MemoryUtil.PtrToStructure<int>(ageInfo.ageArray + MemoryUtil.SizeOf<int>() * i);
                        }

                        int gender = -1;
                        if (retCode_Gender != 0)
                        {
                            debugInfo.Append(string.Format("性别检测失败，返回{0}!\n\n", retCode_Gender));
                        }
                        else
                        {
                            gender = MemoryUtil.PtrToStructure<int>(genderInfo.genderArray + MemoryUtil.SizeOf<int>() * i);
                        }

                        int face3DStatus = -1;
                        float roll = 0f;
                        float pitch = 0f;
                        float yaw = 0f;
                        if (retCode_3DAngle != 0)
                        {
                            debugInfo.Append(string.Format("3DAngle检测失败，返回{0}!\n\n", retCode_3DAngle));
                        }
                        else
                        {
                            //角度状态 非0表示人脸不可信
                            face3DStatus = MemoryUtil.PtrToStructure<int>(face3DAngleInfo.status + MemoryUtil.SizeOf<int>() * i);
                            //roll为侧倾角，pitch为俯仰角，yaw为偏航角
                            roll = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.roll + MemoryUtil.SizeOf<float>() * i);
                            pitch = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.pitch + MemoryUtil.SizeOf<float>() * i);
                            yaw = MemoryUtil.PtrToStructure<float>(face3DAngleInfo.yaw + MemoryUtil.SizeOf<float>() * i);
                        }

                        int rectWidth = rect.right - rect.left;
                        int rectHeight = rect.bottom - rect.top;

                        //查找最大人脸
                        if (rectWidth * rectHeight > rectTemp)
                        {
                            rectTemp = rectWidth * rectHeight;
                            temp = rect;
                            ageTemp = age;
                            genderTemp = gender;
                        }

                        debugInfo.Append(string.Format("{0} - 人脸坐标:[left:{1},top:{2},right:{3},bottom:{4},orient:{5},roll:{6},pitch:{7},yaw:{8},status:{11}] Age:{9} Gender:{10}\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), rect.left, rect.top, rect.right, rect.bottom, orient, roll, pitch, yaw, age, (gender >= 0 ? gender.ToString() : ""), face3DStatus));
                    }

                    debugInfo.Append(string.Format("{0} - 人脸数量:{1}\n\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), multiFaceInfo.faceNum));

                    DateTime detectEndTime = DateTime.Now;
                    debugInfo.Append(string.Format("------------------------------检测结束，时间:{0}------------------------------\n", detectEndTime.ToString("yyyy-MM-dd HH:mm:ss:ms")));
                    debugInfo.Append("\n");
                    ASF_SingleFaceInfo singleFaceInfo = new ASF_SingleFaceInfo();
                    //提取人脸特征
                    // 注意有可能返回 null
                    var ptr = FaceUtil.ExtractFeature(pImageEngine,
                        srcImage,
                        out singleFaceInfo,
                        out string strError);
                    if (ptr == IntPtr.Zero)
                        return new GetFeatureResult
                        {
                            Value = -1,
                            FeaturePtr = IntPtr.Zero,
                            ErrorInfo = strError,
                            ErrorCode = "extractFeatureError"
                        };
                    return new GetFeatureResult { FeaturePtr = ptr };
                }
            }
        }

        // 旧版本
        // errorCode:
        //      "getFeatureFail"
        public override TextResult GetRegisterString(
            Image image,
            string strExcludeBarcodes)
        {
            try
            {
                _cancelOfRegister = new CancellationTokenSource();
                // 等待一秒
                // Task.Delay(TimeSpan.FromSeconds(1), _cancelOfRegister.Token).Wait();

                _cancelOfRegister.Token.ThrowIfCancellationRequested();

                /*
                IntPtr feature = GetFeature(image);
                if (feature == IntPtr.Zero)
                {
                    return new TextResult
                    {
                        Value = -1,
                        ErrorInfo = "图像特征提取失败",
                        ErrorCode = "getFeatureFail"
                    };
                }
                */
                var result = GetFeature(image);
                if (result.Value == -1)
                    return new TextResult
                    {
                        Value = -1,
                        ErrorInfo = $"图像特征提取失败: {result.ErrorInfo}",
                        ErrorCode = "getFeatureFail"
                    };
                var feature = result.FeaturePtr;

                byte[] bytes = FaceUtil.GetFeatureBytes(feature);
                // Speaking("获取人脸信息成功");
                return new TextResult { Text = Convert.ToBase64String(bytes) };
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Where(e => e is TaskCanceledException).Count() > 0)
                    return new TextResult
                    {
                        Value = -1,
                        ErrorInfo = "放弃获取人脸信息",
                        ErrorCode = "abort"
                    };

                return new TextResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = $"exception:{ex.GetType().ToString()}"
                };
            }
        }

        // 新版本
        public override TextResult GetRegisterString(Image image,
    Image irImage,
    string strExcludeBarcodes)
        {
            return GetRegisterString(image, strExcludeBarcodes);
        }

        public override void StartCapture(CancellationToken token)
        {

#if NO
            Task.Run(() =>
            {
                Recongnition(token);
            });
#endif
            /* https://github.com/ArcsoftEscErd/ArcfaceDemo_CSharp
             * 7.在.Net项目中出现堆栈溢出问题,如何解决？
	.Net平台设置的默认堆栈大小为256KB，SDK中需要的大小为512KB以上，推荐调整堆栈的方法为：
	new Thread(new ThreadStart(delegate {
		ASF_MultiFaceInfo multiFaceInfo = FaceUtil.DetectFace(pEngine, imageInfo);
	}), 1024 * 512).Start();
             * */
            Thread captureThread = new Thread(new ThreadStart(Recongnition), 1024 * 512);
            // captureThread.IsBackground = true;
            captureThread.Start();
            _cancelToken = token;
        }


        CancellationToken _cancelToken = new CancellationToken();


        void Recongnition(
            // CancellationToken token
            )
        {
            CancellationToken token = _cancelToken;

            var threshold = GetIdentityThreshold();
            try
            {
                FeatureItem _prevFeature = null;

                while (token.IsCancellationRequested == false)
                {
                    // 等待一秒
                    Task.Delay(TimeSpan.FromSeconds(1), token).Wait();

                    if (token.IsCancellationRequested)
                        return;

                    IntPtr ftr1 = IntPtr.Zero;

                    // byte[] bytes = null;
                    using (var image = TryGetImage())
                    {
                        if (image == null)
                        {
                            _prevFeature = null;
                            continue;
                        }

                        /*
                        ftr1 = GetFeature(image);
                        if (ftr1 == IntPtr.Zero)
                        {
                            _prevFeature = null;
                            continue;
                        }
                        */
                        var result = GetFeature(image);
                        if (result.Value == -1)
                        {
                            _prevFeature = null;
                            continue;
                        }

                        ftr1 = result.FeaturePtr;
                    }

                    // IntPtr ftr1 = FaceUtil.GetFeaturePtr(bytes);
                    try
                    {
                        // testing
                        // continue;

                        // 和前一个识别过的人脸特征比对
                        if (_prevFeature != null)
                        {
                            float similarity = 0f;
                            int ret = ASFFunctions.ASFFaceFeatureCompare(pImageEngine,
                                _prevFeature.Feature,
                                ftr1,
                                ref similarity);
                            //增加异常值处理
                            if (similarity.ToString().IndexOf("E") > -1)
                            {
                                similarity = 0f;
                            }


                            if (// match == true && 
                            similarity >= threshold)
                                continue;
                        }

                        // 和数组比对
                        LockForRead();
                        try
                        {
                            foreach (FeatureItem feature in _featureArray)
                            {
                                if (token.IsCancellationRequested)
                                    return;

                                // 比对
                                float similarity = 0f;
                                int ret = ASFFunctions.ASFFaceFeatureCompare(pImageEngine,
                                    feature.Feature,
                                    ftr1,
                                    ref similarity);
                                //增加异常值处理
                                if (similarity.ToString().IndexOf("E") > -1)
                                {
                                    similarity = 0f;
                                }

                                if (// match == true &&
                                similarity > threshold)
                                {
                                    _prevFeature = feature; // 保存刚识别过的结果

                                    CapturedEventArgs e1 = new CapturedEventArgs
                                    {
                                        Text = feature.PatronID,
                                        Score = (int)Convert.ToInt64(similarity * 100),
                                    };
                                    TriggerCaptured(null, e1);
                                }
                            }
                        }
                        finally
                        {
                            UnlockForRead();
                        }
                    }
                    finally
                    {
                        if (ftr1 != IntPtr.Zero)
                            FaceUtil.FreeFeaturePtr(ftr1);
                    }
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Where(e => e is TaskCanceledException).Count() > 0)
                {
                }
                else
                    throw ex;
            }
            catch (Exception ex)
            {
                int i = 0;
                i++;
            }
        }

        // ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        List<FeatureItem> _featureArray = new List<FeatureItem>();

        class FeatureItem
        {
            public string PatronID { get; set; }
            public IntPtr Feature { get; set; }

            public void Free()
            {
                if (Feature != IntPtr.Zero)
                {
                    FaceUtil.FreeFeaturePtr(Feature);
                    Feature = IntPtr.Zero;
                }
            }
        }

        // 用一个图象对象对内存存储的特征库进行比对
        // parameters:
        //      style   处理风格。(2023/12/29 新增的此参数)
        //              如果包含 multiple_hits 表示返回多个命中结果
        //              如果包含 max_hits:xxx 表示限制最多返回 xxx 个命中结果。如果没有包含，默认返回 100 个
        public override RecognitionFaceResult RecongnitionFace(Image image,
            Image irImage,
            string style,
            CancellationToken token)
        {
            var multiple_hits = StringUtil.IsInList("multiple_hits", style);
            var max_hits_string = StringUtil.GetParameterByPrefix(style, "max_hits");
            int max_hits = 100;
            if (string.IsNullOrEmpty(max_hits_string) == false)
            {
                if (Int32.TryParse(max_hits_string, out max_hits) == false)
                    return new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorCode = "invalidParameter",
                        ErrorInfo = $"style 参数值中 '{max_hits_string}' 格式错误，应为一个整数"
                    };
            }

            List<RecognitionFaceHit> hits = new List<RecognitionFaceHit>();

            var threshold = GetIdentityThreshold();

            StringBuilder debugInfo = new StringBuilder();
            try
            {
                debugInfo?.Append($"Begin GetFeature()\r\n");

                /*
                IntPtr ftr1 = GetFeature(image);
                if (ftr1 == IntPtr.Zero)
                    return new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = "获得人脸特征失败",
                        ErrorCode = "getFeatureFail"
                    };
                */
                var result = GetFeature(image);
                if (result.Value == -1)
                    return new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = $"获得人脸特征失败: {result.ErrorInfo}",
                        ErrorCode = "getFeatureFail"
                    };
                IntPtr ftr1 = result.FeaturePtr;

                debugInfo?.Append($"End GetFeature()\r\n");

                try
                {
                    // 和数组比对
                    LockForRead();
                    try
                    {
                        ScoreInfo max_score = null;
                        debugInfo?.Append($"featureArray.Count={_featureArray.Count} threshold={threshold}\r\n");
                        foreach (FeatureItem feature in _featureArray)
                        {
                            if (token.IsCancellationRequested)
                                return new RecognitionFaceResult
                                {
                                    Value = -1,
                                    ErrorInfo = "中断"
                                };

                            // 比对
                            float similarity = 0f;
                            int ret = ASFFunctions.ASFFaceFeatureCompare(pImageEngine,
                                feature.Feature,
                                ftr1,
                                ref similarity);
                            //增加异常值处理
                            if (similarity.ToString().IndexOf("E") > -1)
                            {
                                similarity = 0f;
                            }


                            // 记下较大的 FeatureItem 和 分数
                            if (similarity > threshold)
                            {
                                debugInfo?.Append($"compare to {feature.PatronID} similarity={similarity}\r\n");
                                if (max_score == null
                                    || max_score.Score < similarity)
                                    max_score = new ScoreInfo { Score = similarity, Feature = feature };

                                if (multiple_hits && hits.Count < max_hits)
                                    hits.Add(new RecognitionFaceHit
                                    {
                                        Score = (int)Convert.ToInt64(similarity * 100),
                                        Patron = feature.PatronID
                                    });
                            }
                        }

                        if (max_score != null)
                            return new RecognitionFaceResult
                            {
                                Value = 1,
                                Patron = max_score.Feature.PatronID,
                                Score = (int)Convert.ToInt64(max_score.Score * 100),
                                Hits = hits.ToArray(),
                                DebugInfo = debugInfo.ToString()
                            };

                        debugInfo?.Append($"not found\r\n");
                        return new RecognitionFaceResult
                        {
                            Value = 0,
                            ErrorInfo = "无法识别",
                            ErrorCode = "recognitionFail",
                            DebugInfo = debugInfo.ToString()
                        };
                    }
                    finally
                    {
                        UnlockForRead();
                    }
                }
                finally
                {
                    if (ftr1 != IntPtr.Zero)
                        FaceUtil.FreeFeaturePtr(ftr1);
                }
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Where(e => e is TaskCanceledException).Count() > 0)
                {
                    return new RecognitionFaceResult
                    {
                        Value = -1,
                        ErrorInfo = "TaskCanceled"
                    };
                }
                else
                    throw ex;
            }
            catch (Exception ex)
            {
                return new RecognitionFaceResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = "exception"
                };
            }
        }

        class ScoreInfo
        {
            public float Score { get; set; }
            public FeatureItem Feature { get; set; }
        }

        public override int ItemCount
        {
            get
            {
                return _featureArray.Count;
            }
        }

        // parameters:
        //      strPatronID 读者证条码号。如果为 null，表示希望删除全部 FeatureItem
        // return:
        //      返回实际删除的数量
        int RemoveFeature(string strPatronID)
        {
            List<FeatureItem> delete_items = new List<FeatureItem>();
            LockForRead();
            try
            {
                foreach (FeatureItem feature in _featureArray)
                {
                    if (strPatronID == null
                        || feature.PatronID == strPatronID)
                        delete_items.Add(feature);
                }
            }
            finally
            {
                UnlockForRead();
            }

            foreach (FeatureItem feature in delete_items)
            {
                feature.Free();
                _featureArray.Remove(feature);
            }

            return delete_items.Count;
        }


        FeatureItem FindFeature(string patron_id)
        {
            LockForRead();
            try
            {
                foreach (var feature in _featureArray)
                {
                    if (feature.PatronID == patron_id)
                        return feature;
                }

                return null;
            }
            finally
            {
                UnlockForRead();
            }
        }

        // 添加高速缓存事项
        // 如果items == null 或者 items.Count == 0，表示要清除当前的全部缓存内容
        // 如果一个item对象的FingerprintString为空，表示要删除这个缓存事项
        // return:
        //      0   成功
        //      其他  失败。错误码
        public override int AddItems(
            List<FingerprintItem> items,
            ProcessInfo info_param,
            out string strError)
        {
            strError = "";
            if (items == null || items.Count == 0)
            {
                Lock();
                try
                {
                    if (info_param != null)
                        info_param.DeleteCount = _featureArray.Count;
                    _featureArray.Clear();
                }
                finally
                {
                    Unlock();
                }
                return 0;
            }

            ProcessInfo info = new ProcessInfo();

            int delete_count = 0;
            int new_count = 0;
            foreach (FingerprintItem item in items)
            {
                // 2019/7/6
                // 跳过 ReaderBarcode 为空的事项。因为它会造成后面 RemoveFeature() 删除全部事项的意外结果
                if (string.IsNullOrEmpty(item.ReaderBarcode))
                    continue;

                if (string.IsNullOrEmpty(item.FingerprintString))
                {
                    var count = RemoveFeature(item.ReaderBarcode);
                    if (info != null)
                        delete_count += count;
                    continue;
                }

                // 2019/7/3
                var remove_count = RemoveFeature(item.ReaderBarcode);
                delete_count += remove_count;

                byte[] bytes = Convert.FromBase64String(item.FingerprintString);

                FeatureItem feature = new FeatureItem
                {
                    PatronID = item.ReaderBarcode,
                    Feature = FaceUtil.GetFeaturePtr(bytes)
                };
                Lock();
                try
                {
                    _featureArray.Add(feature);
                    new_count++;
                }
                finally
                {
                    Unlock();
                }
            }

            if (info != null)
            {
                // 调整一下数量
                int change_count = 0;
                if (new_count > 0 && delete_count > 0)
                {
                    change_count = Math.Min(new_count, delete_count);
                    new_count -= change_count;
                    delete_count -= change_count;
                }

                info.NewCount = new_count;
                info.DeleteCount = delete_count;
                info.ChangeCount = change_count;
            }

            if (info_param != null)
                ProcessInfo.AddTo(info, info_param);
            return 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            RemoveFeature(null);
            this.Free();
        }

        #region 参数值

        /*
 * 5.SDK人脸比对的阈值设为多少合适？	
推荐值为0.8，用户可根据不同场景适当调整阈值。
 * */
        // public double Threshold = 0.8F;


        // 人脸比对阈值 默认值
        public static float DefaultIdentifyThreshold = 0.8F;

        // 获得 人脸比对的阈值
        float GetIdentityThreshold()
        {
            if (ConfigTable.TryGetValue("identity_threshold", out string count) == false)
                return DefaultIdentifyThreshold;
            if (float.TryParse(count, out float value) == false)
                throw new Exception($"identity_threshold 参数值 '{count}' 格式不正确。应为一个 0~1.0 的小数");

            if (!(value >= 0 && value <= 1.0F))
                throw new Exception($"identity_threshold 参数值 '{value}' 超出合法范围。应为 0~1.0 的小数");

            return value;
        }

        #endregion
    }

}
