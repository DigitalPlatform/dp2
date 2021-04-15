using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenCvSharp;

namespace TestOpenCvSharp
{
#if REMOVED
    // https://github.com/thohemp/OpenCVSharpCameraDeviceEnumerator/blob/master/OpenCVDeviceEnumerator/OpenCVDeviceEnumerator.cs
    class OpenCVDeviceEnumerator
    {
        public List<CapDriver> drivers;
        static public List<int> camIdList = new List<int>();
        static OpenCVDeviceEnumerator enumerator = new OpenCVDeviceEnumerator();

        public struct CapDriver
        {
            public int enumValue;
            public string enumName;
            public string comment;
        };


        /*
        static void Main(string[] args)
        {

            enumerator.EnumerateCameras(camIdList);

        }
        */

        public bool EnumerateCameras(List<int> camIdx)
        {
            camIdx.Clear();

            // list of all CAP drivers (see highgui_c.h)
            drivers = new List<CapDriver>();

            //  drivers.Add(new CapDriver { enumValue = CaptureDevice., "CV_CAP_MIL", "MIL proprietary drivers" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.VFW, enumName = "VFW", comment = "platform native" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.V4L, enumName = "V4L", comment = "platform native" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Firewire, enumName = "FireWire", comment = "IEEE 1394 drivers" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Fireware, enumName = "Fireware", comment = "IEEE 1394 drivers" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Qt, enumName = "Qt", comment = "Quicktime" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Unicap, enumName = "Unicap", comment = "Unicap drivers" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.DShow, enumName = "DSHOW", comment = "DirectShow (via videoInput)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.PVAPI, enumName = "PVAPI", comment = "PvAPI, Prosilica GigE SDK" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.OpenNI, enumName = "OpenNI", comment = "OpenNI(for Kinect) " });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.OpenNI_ASUS, enumName = "OpenNI_ASUS", comment = "OpenNI(for Asus Xtion) " });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Android, enumName = "Android", comment = "Android" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.XIAPI, enumName = "XIAPI", comment = "XIMEA Camera API" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.AVFoundation, enumName = "AVFoundation", comment = "AVFoundation framework for iOS (OS X Lion will have the same API)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Giganetix, enumName = "Giganetix", comment = "Smartek Giganetix GigEVisionSDK" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.MSMF, enumName = "MSMF", comment = "Microsoft Media Foundation (via videoInput)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.WinRT, enumName = "WinRT", comment = "Microsoft Windows Runtime using Media Foundation" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.IntelPERC, enumName = "IntelPERC", comment = "Intel Perceptual Computing SDK" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.OpenNI2, enumName = "OpenNI2", comment = "OpenNI2 (for Kinect)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.OpenNI2_ASUS, enumName = "OpenNI2_ASUS", comment = "OpenNI2 (for Asus Xtion and Occipital Structure sensors)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.GPhoto2, enumName = "GPhoto2", comment = "gPhoto2 connection" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.GStreamer, enumName = "GStreamer", comment = "GStreamer" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.FFMPEG, enumName = "FFMPEG", comment = "Open and record video file or stream using the FFMPEG library" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Images, enumName = "Images", comment = "OpenCV Image Sequence (e.g. img_%02d.jpg)" });
            drivers.Add(new CapDriver { enumValue = (int)CaptureDevice.Aravis, enumName = "Aravis", comment = "Aravis SDK" });



            string driverName, driverComment;
            int driverEnum;
            Mat frame = new Mat();
            bool found;
            Console.WriteLine("Searching for cameras IDs...");
            for (int drv = 0; drv < drivers.Count; drv++)
            {
                driverName = drivers[drv].enumName;
                driverEnum = drivers[drv].enumValue;
                driverComment = drivers[drv].comment;
                Console.WriteLine("Testing driver " + driverName);
                found = false;

                int maxID = 100; //100 IDs between drivers
                if (driverEnum == (int)CaptureDevice.VFW)
                    maxID = 10; //VWF opens same camera after 10 ?!?


                for (int idx = 0; idx < maxID; idx++)
                {

                    VideoCapture cap = new VideoCapture(driverEnum + idx);  // open the camera
                    if (cap.IsOpened())                  // check if we succeeded
                    {
                        found = true;
                        camIdx.Add(driverEnum + idx);  // vector of all available cameras
                        cap.Read(frame);
                        if (frame.Empty())
                            Console.WriteLine(driverName + "+" + idx + "\t opens: OK \t grabs: FAIL");
                        else
                            Console.WriteLine(driverName + "+" + idx + "\t opens: OK \t grabs: OK");
                    }
                    cap.Release();
                }
                if (!found) Console.WriteLine("Nothing !");

            }
            Console.WriteLine(camIdx.Count() + " camera IDs has been found ");
            Console.WriteLine("Press a key..."); Console.ReadKey();

            return (camIdx.Count() > 0); // returns success
        }
    }

#endif
}
