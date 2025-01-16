using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
     Request SC Resend
     This message requests the SC to re-transmit its last message.  It is sent by the ACS to the SC when the checksum in a received message does not match the value calculated by the ACS.  The SC should respond by re-transmitting its last message, This message should never include a “sequence number” field, even when error detection is enabled, (see “Checksums and Sequence Numbers” below) but would include a “checksum” field since checksums are in use.
     96
     */
    public class RequestSCResend_96 : BaseMessage
    {
        public RequestSCResend_96()
        {
            this.CommandIdentifier = "96";
        }
    }
}
