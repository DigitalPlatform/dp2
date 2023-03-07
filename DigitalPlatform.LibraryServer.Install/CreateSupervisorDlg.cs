using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 用于创建 supervisor 用户的对话框
    /// </summary>
    public partial class CreateSupervisorDlg : Form
    {
        /// <summary>
        /// 缺省的 supervisor 用户权限值
        /// </summary>
        public static string DefaultRights = "borrow,return,renew,lost,reservation,order,setclock,changereaderpassword,verifyreaderpassword,getbibliosummary,searchreader,getreaderinfo,setreaderinfo,movereaderinfo,changereaderstate,listdbfroms,searchbiblio,getbiblioinfo,searchitem,getiteminfo,setiteminfo,getoperlog,amerce,amercemodifyprice,amercemodifycomment,amerceundo,search,getrecord,getcalendar,changecalendar,newcalendar,deletecalendar,batchtask,clearalldbs,devolvereaderinfo,getuser,changeuser,newuser,deleteuser,changeuserpassword,getsystemparameter,setsystemparameter,urgentrecover,repairborrowinfo,passgate,getres,writeres,setbiblioinfo,hire,foregift,returnforegift,settlement,undosettlement,deletesettlement,searchissue,getissueinfo,setissueinfo,searchorder,getorderinfo,setorderinfo,getcommentinfo,setcommentinfo,searchcomment,setobject,writetemplate,managedatabase,restore,managecache,managecomment,settailnumber,setutilinfo,getpatrontempid,getchannelinfo,managechannel,viewreport,upload,download";

        public CreateSupervisorDlg()
        {
            InitializeComponent();
        }

        private void CreateSupervisorDlg_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_rights.Text) == true)
                this.textBox_rights.Text = DefaultRights;
        }

        private void button_createSupervisor_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_supervisorUserName.Text == "")
            {
                strError = "尚未指定用户名";
                goto ERROR1;
            }

            /*
            if (this.textBox_supervisorUserName.Text.ToLower() == "public"
                || this.textBox_supervisorUserName.Text.ToLower() == "reader"   //?
                || this.textBox_supervisorUserName.Text == "图书馆")
            */
            if (LibraryServerUtil.IsSpecialUserName(this.textBox_supervisorUserName.Text))
            {
                strError = "在这里您不能把用户名取为 '" + this.textBox_supervisorUserName.Text + "'，因为它是被保留的特殊用户名";
                goto ERROR1;
            }

            if (this.textBox_supervisorPassword.Text == "")
            {
                /*
                strError = "尚未指定密码";
                goto ERROR1;
                 * */
                // 2009/10/10 changed
                DialogResult result = MessageBox.Show(this,
                    "超级用户的密码为空。这样会很不安全。你也可以在安装成功后，尽快利用dp2Circulation中的用户窗为超级用户加上密码。\r\n\r\n确实要保持超级用户的密码为空吗?",
                    "setup_dp2library",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;

            }

            if (this.textBox_supervisorPassword.Text != this.textBox_confirmSupervisorPassword.Text)
            {
                strError = "密码 和 确认密码 不一致。请重新输入。";
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string UserName
        {
            get
            {
                return this.textBox_supervisorUserName.Text;
            }
            set
            {
                this.textBox_supervisorUserName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_supervisorPassword.Text;
            }
            set
            {
                this.textBox_supervisorPassword.Text = value;
                this.textBox_confirmSupervisorPassword.Text = value;
            }
        }

        public string Rights
        {
            get
            {
                return this.textBox_rights.Text;
            }
            set
            {
                this.textBox_rights.Text = value;
            }
        }
    }
}