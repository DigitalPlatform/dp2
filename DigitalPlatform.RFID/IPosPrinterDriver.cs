using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public interface IPosPrinterDriver
    {
        NormalResult InitializeDriver(string port, string style);

        NormalResult ReleaseDriver();

        // parameters:
        //      style   附加的子参数 
        NormalResult Print(
            string action,
            string text,
            string style);
    }
}
