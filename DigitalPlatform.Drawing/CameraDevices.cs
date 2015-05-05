using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AForge.Video.DirectShow;

namespace DigitalPlatform.Drawing
{
    internal class CameraDevices
    {
        public FilterInfoCollection Devices { get; private set; }
        public VideoCaptureDevice Current { get; private set; }

        public CameraDevices()
        {
            Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public void SelectCamera(int index)
        {
            if (index >= Devices.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            Current = new VideoCaptureDevice(Devices[index].MonikerString);
            this.m_strMoniker = Devices[index].MonikerString;
        }

        string m_strMoniker = "";
        public string CurrentCameraMonier
        {
            get
            {
                return this.m_strMoniker;
            }
        }

        // 2013/4/10
        public void SelectCamera(string strMonikerString)
        {
            this.m_strMoniker = "";
            Current = new VideoCaptureDevice(strMonikerString);
            this.m_strMoniker = strMonikerString;
        }

        // 2013/4/10
        // 比较两个设备列表是否一致
        public bool IsEqual(CameraDevices other)
        {
            if (this == other)
                return true;
            if (this.Devices == null && other.Devices == null)
                return true;
            if (this.Devices == null || other.Devices == null)
                return false; 
            
            if (this.Devices.Count != other.Devices.Count)
                return false;

            for (int i = 0; i < this.Devices.Count; i++)
            {
                FilterInfo info = this.Devices[i];
                FilterInfo other_info = other.Devices[i];
                if (info.CompareTo(other_info) != 0)
                    return false;
            }

            return true;
        }
    }
}
