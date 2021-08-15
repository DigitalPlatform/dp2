using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class ChatHost
    {
        public ChatForm ChatForm { get; set; }

        // 扩展装入前一日的消息
        public void expand()
        {
            // ChatForm.InsertHtml("<div class='item'>test text</div>");
            // MessageBox.Show(ChatForm, "expand");
            ChatForm.BeginExpandMessage();
        }
    }

}
