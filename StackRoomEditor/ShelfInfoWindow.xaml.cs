using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace StackRoomEditor
{
    /// <summary>
    /// ShelfInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShelfInfoWindow : Window
    {
        public event ShelfInfoChanged ShelfInfoChanged = null;

        BookShelf m_shelf = null;

        public BookShelf BookShelf
        {
            get
            {
                return this.m_shelf;
            }
        }

        bool m_bChanged = false;

        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;

                this.button_apply.IsEnabled = value;
            }
        }

        public ShelfInfoWindow()
        {
            InitializeComponent();
        }

        bool m_bModel = false;

        // 设置为模式对话框状态
        public void SetModelState()
        {
            this.button_apply.Content = "确定";
            this.button_close.Content = "取消";
            m_bModel = true;
        }

        public void PutInfo(BookShelf shelf)
        {
            if (this.m_shelf != shelf)
            {
                this.RefreshInfo();
                this.m_shelf = null;
            }

            this.m_shelf = shelf;

            this.textBox_roomName.Text = shelf.RoomName;
            this.textBox_shelfNo.Text = shelf.No;
            this.textBox_accessNoRange.Text = shelf.AccessNoRange;
            this.textBox_locationX.Text = shelf.LocationX.ToString();
            this.textBox_locationZ.Text = shelf.LocationZ.ToString();
            this.textBox_direction.Text = shelf.Direction.ToString();
            this.textBox_color.Text = shelf.ColorString;

            this.Changed = false;
        }

        static string GetTextValue(string strText)
        {
            if (strText == "<空>")
                return "";
            if (string.IsNullOrEmpty(strText) == false)
                return strText;
            return null;    // 表示没有发生修改
        }

        // 部分兑现修改值
        public void PartialGetInfo(BookShelf shelf)
        {
            if (GetTextValue(this.textBox_roomName.Text)!= null)
                shelf.RoomName = GetTextValue(this.textBox_roomName.Text);

            if (GetTextValue(this.textBox_shelfNo.Text) != null)
                shelf.No = GetTextValue(this.textBox_shelfNo.Text);

            if (GetTextValue(this.textBox_accessNoRange.Text) != null)
                shelf.AccessNoRange = GetTextValue(this.textBox_accessNoRange.Text);

            if (GetTextValue(this.textBox_color.Text) != null)
            {
                // TODO: 是否要验证一下颜色值?
                shelf.ColorString = GetTextValue(this.textBox_color.Text);
            }

            double v = 0;
            bool bRet = false;

            if (GetTextValue(this.textBox_locationX.Text) != null)
            {
                bRet = double.TryParse(this.textBox_locationX.Text, out v);
                if (bRet == false)
                    throw new Exception("位置 X 值 '" + this.textBox_locationX.Text + "' 格式错误");
                shelf.LocationX = v;
            }

            if (GetTextValue(this.textBox_locationZ.Text) != null)
            {
                v = 0;
                bRet = double.TryParse(this.textBox_locationZ.Text, out v);
                if (bRet == false)
                    throw new Exception("位置 Z 值 '" + this.textBox_locationZ.Text + "' 格式错误");

                shelf.LocationZ = v;
            }

            if (GetTextValue(this.textBox_direction.Text) != null)
            {
                v = 0;
                bRet = double.TryParse(this.textBox_direction.Text, out v);
                if (bRet == false)
                    throw new Exception("方向 值 '" + this.textBox_direction.Text + "' 格式错误");

                shelf.Direction = v;
            }
        }

        public void GetInfo(BookShelf shelf)
        {
            shelf.RoomName = this.textBox_roomName.Text;
            shelf.No = this.textBox_shelfNo.Text;
            shelf.AccessNoRange = this.textBox_accessNoRange.Text;
            shelf.ColorString = this.textBox_color.Text;

            double v = 0;
            bool bRet = double.TryParse(this.textBox_locationX.Text, out v);
            if (bRet == false)
                throw new Exception("位置 X 值 '"+this.textBox_locationX.Text+"' 格式错误");
            shelf.LocationX = v;

            v = 0;
            bRet = double.TryParse(this.textBox_locationZ.Text, out v);
            if (bRet == false)
                throw new Exception("位置 Z 值 '" + this.textBox_locationZ.Text + "' 格式错误");

            shelf.LocationZ= v;

            v = 0;
            bRet = double.TryParse(this.textBox_direction.Text, out v);
            if (bRet == false)
                throw new Exception("方向 值 '" + this.textBox_direction.Text + "' 格式错误");

            shelf.Direction = v;
        }

        // 将面板信息兑现到 m_shelf 对象中
        public void RefreshInfo()
        {
            if (m_shelf == null)
                return;

            if (this.m_bChanged == false)
                return;

            try
            {
                GetInfo(m_shelf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
            this.m_bChanged = false;
            if (this.ShelfInfoChanged != null)
            {
                ShelfInfoChangedEventArgs e = new ShelfInfoChangedEventArgs();
                e.Shelf = this.m_shelf;
                this.ShelfInfoChanged(this, e);
            }
        }

        public void Clear()
        {
            this.m_shelf = null;

            this.textBox_roomName.Text = "";
            this.textBox_shelfNo.Text = "";
            this.textBox_accessNoRange.Text = "";
            this.textBox_locationX.Text = "";
            this.textBox_locationZ.Text = "";
            this.textBox_direction.Text = "";
            this.textBox_color.Text = "";
        }

        private void button_apply_Click(object sender, RoutedEventArgs e)
        {
            this.RefreshInfo();
            if (this.m_bModel == true)
                this.DialogResult = true;
            this.button_apply.IsEnabled = false;
        }

        private void textBox_roomName_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_shelfNo_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_accessNoRange_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_locationX_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_locationZ_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_direction_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_color_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Changed = true;
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            if (this.m_bModel == true)
                this.DialogResult = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.RefreshInfo();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };


        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DwmIsCompositionEnabled() == true)
            {
                this.Background = Brushes.Transparent;

                try
                {
                    // Obtain the window handle for WPF application
                    IntPtr mainWindowPtr = new WindowInteropHelper(this).Handle;
                    HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                    mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                    // Get System Dpi
                    System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                    float DesktopDpiX = desktop.DpiX;
                    float DesktopDpiY = desktop.DpiY;

                    // Set Margins
                    MARGINS margins = new MARGINS();

                    // Extend glass frame into client area
                    // Note that the default desktop Dpi is 96dpi. The  margins are
                    // adjusted for the system Dpi.
                    margins.cxLeftWidth = -1;   //  Convert.ToInt32(5 * (DesktopDpiX / 96));
                    margins.cxRightWidth = -1;  //  Convert.ToInt32(5 * (DesktopDpiX / 96));
                    margins.cyTopHeight = -1;   //  Convert.ToInt32((10 + 5) * (DesktopDpiX / 96));
                    margins.cyBottomHeight = -1;    // Convert.ToInt32(5 * (DesktopDpiX / 96));

                    int hr = DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                    //
                    if (hr < 0)
                    {
                        //DwmExtendFrameIntoClientArea Failed
                    }
                }
                // If not Vista, paint background white.
                catch (DllNotFoundException)
                {
                    Application.Current.MainWindow.Background = Brushes.White;
                }
            }
        }
    }

    public delegate void ShelfInfoChanged(object sender,
ShelfInfoChangedEventArgs e);


    public class ShelfInfoChangedEventArgs : EventArgs
    {
        public BookShelf Shelf = null;  // 指示哪一个书架对象发生了信息改变
    }
}
