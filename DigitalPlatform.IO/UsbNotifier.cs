using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    // 不需要窗口的 USB 变化通知类
    // https://stackoverflow.com/questions/10022794/how-can-i-use-registerdevicenotification-without-a-window-handle
    public class UsbNotifier
    {
        System.Management.WqlEventQuery _q = null;
        ManagementEventWatcher _w = null;

        delegate_action _action = null;

        public delegate void delegate_action(string type);

        public void Start(delegate_action action)
        {
            _action = action;

            _q = new WqlEventQuery("__InstanceOperationEvent", "TargetInstance ISA 'Win32_USBControllerDevice' ");
            _q.WithinInterval = TimeSpan.FromSeconds(1);

            _w = new ManagementEventWatcher(_q);
            _w.EventArrived += new EventArrivedEventHandler(onEventArrived);
            _w.Start();
        }

        public void Stop()
        {
            _w.Stop();
            _w.EventArrived -= new EventArrivedEventHandler(onEventArrived);
        }

        void onEventArrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject _o = e.NewEvent["TargetInstance"] as ManagementBaseObject;

            if (_o != null)
            {
                using (ManagementObject mo = new ManagementObject(_o["Dependent"].ToString()))
                {
                    if (mo != null)
                    {
                        try
                        {
                            if (mo.GetPropertyValue("DeviceID").ToString() != string.Empty)
                            {
                                //connected
                                _action?.Invoke("connected");
                            }
                        }

                        catch (ManagementException ex)
                        {
                            //disconnected
                            _action?.Invoke("disconnected");
                        }
                    }
                }
            }
        }
    }
}
