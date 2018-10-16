using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public class NormalResult
    {
        public int Value { get; set; }
        public string ErrorInfo { get; set; }

        public NormalResult(int value, string error)
        {
            this.Value = value;
            this.ErrorInfo = error;
        }

        public NormalResult()
        {

        }
    }

}
