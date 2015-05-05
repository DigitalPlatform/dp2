using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for BrowseList.
	/// </summary>
    public class BrowseList : DigitalPlatform.GUI.ListViewNF   // System.Windows.Forms.ListView
	{
		public string Lang = "zh";

        public bool IsInsert = false;

		private System.Windows.Forms.ColumnHeader columnHeader_id;
		private System.Windows.Forms.ColumnHeader columnHeader_first;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public BrowseList()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.columnHeader_id = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_first = new System.Windows.Forms.ColumnHeader();
			// 
			// columnHeader_id
			// 
			this.columnHeader_id.Text = "记录路径";
			this.columnHeader_id.Width = 150;
			// 
			// columnHeader_first
			// 
			this.columnHeader_first.Text = "1";
			this.columnHeader_first.Width = 200;
			// 
			// BrowseList
			// 
			this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																			  this.columnHeader_id,
																			  this.columnHeader_first});
			this.FullRowSelect = true;
			this.HideSelection = false;
			this.View = System.Windows.Forms.View.Details;

		}
		#endregion


		// 确保列标题数量足够
		void EnsureColumns(int nCount)
		{
			if (this.Columns.Count >= nCount)
				return;

			for(int i=this.Columns.Count;i<nCount;i++) 
			{
				string strText = "";
				if (i == 0) 
				{
					strText = "记录路径";
				}
				else 
				{
					strText = Convert.ToString(i);
				}

				ColumnHeader col = new ColumnHeader();
				col.Text = strText;
				col.Width = 200;
				this.Columns.Add(col);
			}

		}


		// 在listview最后追加一行
		public void NewLine(string strID, string []others)
		{
			EnsureColumns(others.Length + 1);

			ListViewItem item = new ListViewItem(strID, 0);


			for(int i=0;i<others.Length;i++) 
			{
				item.SubItems.Add(others[i]);
			}

            if (this.IsInsert == true)
            {
                this.Items.Insert(0, item);
                UpdateItem(0);
            }
            else
            {
                this.Items.Add(item);
                UpdateItem(this.Items.Count - 1);
            }


		}


		// 获得当前选择的若干记录路径
		public static string[] GetSelectedRecordPaths(
			ListView listview,
			bool bIncludeFocuedItem)
		{
			int nDelta = 0;
			if (bIncludeFocuedItem == true) 
			{
				if (listview.FocusedItem != null)
					nDelta = 1;
			}
			string [] paths = new string [listview.SelectedItems.Count + nDelta];


			for(int i=0;i<listview.SelectedItems.Count;i++)
			{
				paths[i] = ResPath.GetRegularRecordPath(listview.SelectedItems[i].Text);
			}

			if (nDelta == 1) 
			{
				paths[paths.Length-1] = ResPath.GetRegularRecordPath(listview.FocusedItem.Text);
				listview.FocusedItem.Selected = true;
			}

			return paths;
        }

        bool updating = false;
        //int itemnumber = 0;
        public void UpdateItem(int iIndex)
        {
            updating = true;

            // itemnumber = iIndex;
            this.Update();

            updating = false;
        }

        protected override void WndProc(ref Message messg)
        {
            if (updating)
            {
                // We do not want to erase the background, 
                // turn this message into a null-message
                if ((int)API.WM_ERASEBKGND == messg.Msg)
                    messg.Msg = (int)API.WM_NULL;
                /*
                else if ((int)API.WM_PAINT == messg.Msg)
                {
                    RECT vrect = this.GetWindowRECT();
                    // validate the entire window                
                    API.ValidateRect(this.Handle, ref vrect);

                    //Invalidate only the new item
                    Invalidate(this.Items[itemnumber].Bounds);
                }*/
            }
            base.WndProc(ref messg);
        }

        /*
        private RECT GetWindowRECT()
        {
            RECT rect = new RECT();
            rect.left = this.Left;
            rect.right = this.Right;
            rect.top = this.Top;
            rect.bottom = this.Bottom;
            return rect;
        }*/
    }
}
