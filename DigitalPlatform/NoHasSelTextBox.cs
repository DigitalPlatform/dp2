using System;
using System.Windows.Forms;

using DigitalPlatform;

namespace DigitalPlatform.GUI
{
	/// <summary>
	/// 不会被对话框轻易全选的TextBox
	/// </summary>
	public class NoHasSelTextBox : TextBox
	{
		public bool DisableEmSetSelMsg = true;

		protected override void WndProc(ref Message m)
		{
			/*
			base.WndProc( ref m );

			if( m.Msg == (int)API.WM_GETDLGCODE )
			{
				m.Result = new IntPtr( (int)API.DLGC_WANTCHARS |
					(int)API.DLGC_WANTARROWS |
					(int)API.DLGC_WANTTAB |
					m.Result.ToInt32() );
			}
			return;
			*/


			switch (m.Msg) 
			{

				case API.EM_SETSEL:
				{
					if (DisableEmSetSelMsg == true)
						return;
					break;
				}

					
					
					
				case API.WM_GETDLGCODE:
				{

					
					/*
					base.DefWndProc(ref m);
					int temp = (int)m.Result;
					temp |= API.DLGC_WANTTAB;
					// temp &= ~API.DLGC_HASSETSEL;

					// m.Result = new IntPtr(temp);
					*/
					
					

					m.Result = new IntPtr(API.DLGC_WANTALLKEYS);
					return;

				}
					// break;
					
					
				
			}

			base.WndProc(ref m);
		}
	}
}
