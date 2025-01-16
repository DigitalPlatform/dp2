using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    Request ACS Resend
    This message requests the ACS to re-transmit its last message.  
    It is sent by the SC to the ACS when the checksum in a received message does not match the value calculated by the SC.  The ACS should respond by re-transmitting its last message,  This message should never include a “sequence number” field, even when error detection is enabled, (see “Checksums and Sequence Numbers” below) but would include a “checksum” field since checksums are in use.
    97
    */
    public class RequestACSResend_97 : BaseMessage
    {
        public RequestACSResend_97()
        {
            this.CommandIdentifier = "97";

            this.SetDefaultValue();
        }
    }
}
