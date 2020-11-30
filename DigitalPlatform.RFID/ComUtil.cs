using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public delegate void SerialPortEventHandler(Object sender, SerialPortEventArgs e);

    public class SerialPortEventArgs : EventArgs
    {
        public bool isOpend = false;
        public Byte[] receivedBytes = null;
    }

    public class ComModel
    {
        private SerialPort _sp = new SerialPort();

        public event SerialPortEventHandler comReceiveDataEvent = null;
        public event SerialPortEventHandler comOpenEvent = null;
        public event SerialPortEventHandler comCloseEvent = null;

        private Object thisLock = new Object();

        /// <summary>
        /// When serial received data, will call this method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (_sp.BytesToRead <= 0)
            {
                return;
            }
            //Thread Safety explain in MSDN:
            // Any public static (Shared in Visual Basic) members of this type are thread safe. 
            // Any instance members are not guaranteed to be thread safe.
            // So, we need to synchronize I/O
            lock (thisLock)
            {
                int len = _sp.BytesToRead;
                Byte[] data = new Byte[len];
                try
                {
                    _sp.Read(data, 0, len);
                }
                catch (System.Exception)
                {
                    //catch read exception
                }
                SerialPortEventArgs args = new SerialPortEventArgs();
                args.receivedBytes = data;
                if (comReceiveDataEvent != null)
                {
                    comReceiveDataEvent.Invoke(this, args);
                }
            }
        }

        /// <summary>
        /// Send bytes to device
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool Send(byte[] bytes, out string strError)
        {
            strError = "";
            if (!_sp.IsOpen)
            {
                strError = "_sp 尚未打开";
                return false;
            }

            try
            {
                _sp.Write(bytes, 0, bytes.Length);
            }
            catch (System.Exception ex)
            {
                strError = $"Write() 出现异常: {ex.Message}";
                return false;   //write failed
            }
            return true;        //write successfully
        }

        /// <summary>
        /// Open Serial port
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        /// <param name="parity"></param>
        /// <param name="handshake"></param>
        public bool Open(string portName, 
            String baudRate,
            string dataBits,
            string stopBits, 
            string parity,
            string handshake,
            out string strError)
        {
            strError = "";

            if (_sp.IsOpen)
            {
                Close();
            }
            _sp.PortName = portName;
            _sp.BaudRate = Convert.ToInt32(baudRate);
            _sp.DataBits = Convert.ToInt16(dataBits);

            /**
             *  If the Handshake property is set to None the DTR and RTS pins 
             *  are then freed up for the common use of Power, the PC on which
             *  this is being typed gives +10.99 volts on the DTR pin & +10.99
             *  volts again on the RTS pin if set to true. If set to false 
             *  it gives -9.95 volts on the DTR, -9.94 volts on the RTS. 
             *  These values are between +3 to +25 and -3 to -25 volts this 
             *  give a dead zone to allow for noise immunity.
             *  http://www.codeproject.com/Articles/678025/Serial-Comms-in-Csharp-for-Beginners
             */
            if (handshake == "None")
            {
                //Never delete this property
                _sp.RtsEnable = true;
                _sp.DtrEnable = true;
            }

            SerialPortEventArgs args = new SerialPortEventArgs();
            try
            {
                _sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
                _sp.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                _sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshake);
                _sp.WriteTimeout = 2000; /*Write time out*/
                // _sp.ErrorReceived += _sp_ErrorReceived;
                // _sp.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                _sp.Open();
                args.isOpend = true;
            }
            catch (System.Exception ex)
            {
                args.isOpend = false;
                strError = $"串口打开失败: {ex.Message}";
                return false;
            }

            if (comOpenEvent != null)
            {
                comOpenEvent.Invoke(this, args);
            }

            return true;
        }

        public SerialError SerialError { get; set; }

        private void _sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            this.SerialError = e.EventType;
        }

        /**
         *  Take care to avoid deadlock when calling Close on the SerialPort 
         *  in response to a GUI event.
         *   An app involving the UI and the SerialPort freezes up when closing the SerialPort
         *   Deadlock can occur if Control.Invoke() is used in serial port event handlers
         * 
         *  The typical scenario we encounter is occasional deadlock in an app 
         *  that has a data received handler trying to update the GUI at the 
         *  same time the GUI thread is trying to close the SerialPort (for 
         *  example, in response to the user clicking a Close button).
         * 
         *  The reason deadlock happens is that Close() waits for events to 
         *  finish executing before it closes the port. You can address this 
         *  problem in your apps in two ways:
         * 
         *  (1)In your event handlers, replace every Control.Invoke call with 
         *  Control.BeginInvoke, which executes asynchronously and avoids 
         *  the deadlock condition. This is commonly used for deadlock avoidance 
         *  when working with GUIs.
         *  
         *  (2)Call serialPort.Close() on a separate thread. You may prefer this
         *  because this is less invasive than updating your Invoke calls.
         */
        /// <summary>
        /// Close serial port
        /// </summary>
        public void Close()
        {
            Thread closeThread = new Thread(new ThreadStart(CloseSpThread));
            closeThread.Start();
        }

        /// <summary>
        /// Close serial port thread
        /// </summary>
        private void CloseSpThread()
        {
            SerialPortEventArgs args = new SerialPortEventArgs();
            args.isOpend = false;
            try
            {
                _sp.Close(); //close the serial port
                             // _sp.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
            }
            catch (Exception)
            {
                args.isOpend = true;
            }
            if (comCloseEvent != null)
            {
                comCloseEvent.Invoke(this, args);
            }
        }
    }
}
