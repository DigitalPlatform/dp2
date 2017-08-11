using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DigitalPlatform.rms.Client
{
    // 2017/5/3
    /// <summary>
    /// 数据库记录枚举器。用于枚举若干个数据库，整个数据库内的所有记录
    /// </summary>
    public class RecordLoader : IEnumerable
    {
        public string ElementType { get; set; } // KernelRecord / Record。默认 KernelRecord

        public List<string> Paths { get; set; } // 数据库路径集合。包含若干个数据库的路径

        public RmsChannelCollection Channels { get; set; }

        public string ResultSetName { get; set; }


        public DigitalPlatform.Stop Stop { get; set; }

        public string FormatList
        {
            get;
            set;
        }

        public string Lang { get; set; }

        // 每批获取最多多少个记录
        public long BatchSize { get; set; }

        public RecordLoader(
            RmsChannelCollection channels,
            DigitalPlatform.Stop stop,
            List<string> paths,
            string resultsetName,
            string formatList,
            string lang = "zh")
        {
            this.Channels = channels;
            this.Stop = stop;
            this.ResultSetName = resultsetName;
            this.FormatList = formatList;
            this.Lang = lang;
            this.Paths = paths;
        }

        RmsChannel cur_channel = null;

        public IEnumerator GetEnumerator()
        {
            string strError = "";
            string strRange = "0-9999999999";
            long lTotalCount = 0;	// 总命中数
            long lExportCount = 0;
            string strTimeMessage = "";

            DigitalPlatform.Stop stop = this.Stop;

            StopStyle old_style = StopStyle.None;
            if (stop != null)
            {
                old_style = stop.Style;
                stop.Style = StopStyle.EnableHalfStop;  // API的间隙才让中断。避免获取结果集的中途，因为中断而导致 Session 失效，结果集丢失，进而无法 Retry 获取
                stop.OnStop += stop_OnStop;
            }
            ProgressEstimate estimate = new ProgressEstimate();

            try
            {
                int i_path = 0;
                foreach (string path in this.Paths)
                {
                    ResPath respath = new ResPath(path);

                    string strQueryXml = "<target list='" + respath.Path
            + ":" + "__id'><item><word>" + strRange + "</word><match>exact</match><relation>range</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                    cur_channel = Channels.CreateTempChannel(respath.Url);
                    Debug.Assert(cur_channel != null, "Channels.GetChannel() 异常");

                    try
                    {
                        long lRet = cur_channel.DoSearch(strQueryXml,
            "default",
            out strError);
                        if (lRet == -1)
                        {
                            strError = "检索数据库 '" + respath.Path + "' 时出错: " + strError;
                            throw new Exception(strError);
                        }

                        if (lRet == 0)
                        {
                            strError = "数据库 '" + respath.Path + "' 中没有任何数据记录";
                            continue;
                        }

                        lTotalCount += lRet;	// 总命中数

                        estimate.SetRange(0, lTotalCount);
                        if (i_path == 0)
                            estimate.StartEstimate();

                        if (stop != null)
                        stop.SetProgressRange(0, lTotalCount);

                        SearchResultLoader loader = new SearchResultLoader(cur_channel,
    stop,
    this.ResultSetName,
    this.FormatList,
    this.Lang);
                        loader.BatchSize = this.BatchSize;

                        foreach (KernelRecord record in loader)
                        {
                            if (stop != null)
                            stop.SetProgressValue(lExportCount + 1);
                            lExportCount++;

                            yield return record;
                        }

                        strTimeMessage = "总共耗费时间: " + estimate.GetTotalTime().ToString();
                    }
                    finally
                    {
                        cur_channel.Close();
                        cur_channel = null;
                    }
                    // MessageBox.Show(this, "位于服务器 '" + respath.Url + "' 上的数据库 '" + respath.Path + "' 内共有记录 " + lTotalCount.ToString() + " 条，本次导出 " + lExportCount.ToString() + " 条。" + strTimeMessage);

                    i_path++;
                }

            }
            finally
            {
                if (stop != null)
                {
                    stop.Style = old_style;
                    stop.OnStop -= stop_OnStop;
                }
            }
        }

        void stop_OnStop(object sender, StopEventArgs e)
        {
            if (cur_channel != null)
                cur_channel.Abort();
        }
    }
}
