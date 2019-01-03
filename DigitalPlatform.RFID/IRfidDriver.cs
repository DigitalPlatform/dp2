using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public interface IRfidDriver
    {
        void InitializeDriver();

        void ReleaseDriver();

        void OpenReader();

        void CloseReader();

        void Inventory();

        void ConnectTag();

        void DisconnectTag();
    }
}
