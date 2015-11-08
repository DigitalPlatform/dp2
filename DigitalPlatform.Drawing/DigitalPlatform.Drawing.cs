using System;
using System.Net;
using System.Drawing;
using System.IO;

namespace DigitalPlatform.Drawing
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class DrawingUtil
	{
		public static MemoryStream MakeTextPic(
			string strText,
			string strFace,
			int nSize,
			Color colorBack)
		{
            SizeF size;
            Font font = null;
            try
            {
                using (Bitmap bitmapTemp = new Bitmap(1, 1))
                {
                    using (Graphics graphicsTemp = Graphics.FromImage(bitmapTemp))
                    {

                        font = new Font(strFace, nSize, FontStyle.Bold);
                        size = graphicsTemp.MeasureString(
                            strText,
                            font);
                        size.Height = (int)((double)size.Height * 1.5F);
                        size.Width = (int)((double)size.Width * 1.2F);
                    }
                }

                // 正式的图像
                using (Bitmap bitmapDest = new Bitmap((int)size.Width, (int)size.Height))
                {
                    using (Graphics objGraphics = Graphics.FromImage(bitmapDest))
                    {
                        objGraphics.Clear(colorBack/*Color.DarkGray*/);// Color.Teal
                        objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                        StringFormat stringFormat = new StringFormat();

                        // 随机产生一个倾斜角度
                        Random random = new Random(unchecked((int)DateTime.Now.Ticks));
                        int angle = random.Next(-10, 10);

                        objGraphics.RotateTransform(angle);

                        stringFormat.Alignment = StringAlignment.Near;
                        if (angle > 0)
                            stringFormat.LineAlignment = StringAlignment.Near;
                        else
                            stringFormat.LineAlignment = StringAlignment.Far;

                        // Color.FromArgb(128, 100, 100, 100)
                        using (Brush objBrush = new SolidBrush(Color.Black)) // 透明颜色 ' Color.Black
                        {
                            RectangleF rect = new RectangleF(0, 0, size.Width, size.Height);
                            objGraphics.DrawString(strText,
                                font,
                                objBrush,
                                rect,
                                stringFormat);
                        }
                    }

                    MemoryStream stream = new MemoryStream();

                    bitmapDest.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return stream;
                }
            }
            finally
            {
                if (font != null)
                    font.Dispose();
            }
        }

		public static int MakeTextPic(
			string strText,
			string strFace,
			int nSize,
			string strOutputFileName)
		{
            SizeF size;
            using (Font font = new Font(strFace, nSize, FontStyle.Bold))
            {
                using (Bitmap bitmapTemp = new Bitmap(1, 1))
                {
                    using (Graphics graphicsTemp = Graphics.FromImage(bitmapTemp))
                    {
                        size = graphicsTemp.MeasureString(
                            strText,
                            font);
                    }
                }

                // 正式的图像
                using (Bitmap bitmapDest = new Bitmap((int)size.Width, (int)size.Height))
                {
                    using (Graphics objGraphics = Graphics.FromImage(bitmapDest))
                    {
                        objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

                        StringFormat stringFormat = new StringFormat();

                        stringFormat.Alignment = StringAlignment.Near;
                        stringFormat.LineAlignment = StringAlignment.Near;

                        using (Brush objBrush = new SolidBrush(Color.FromArgb(128, 100, 100, 100))) // 透明颜色 ' Color.Black
                        {
                            RectangleF rect = new RectangleF(0, 0, size.Width, size.Height);
                            objGraphics.DrawString(strText,
                                font,
                                objBrush,
                                rect,
                                stringFormat);
                        }
                    }

                    bitmapDest.Save(strOutputFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                return 0;
            }
		}


        // 从URI获取图像对象
        // parameters:
        //      strUrl  图像路径
        //      image   out参数，返回Image对象
        //      strError    out参数，返回出错信息
        // return:
        //      -1  出错
        //      0   成功
        public static int GetImageFormUrl(string strUrl,
            out Image image,
            out string strError)
        {
            strError = "";
            image = null;

            try
            {
                // 通过URL获得图像文件的过程可以优化， 可以为Editor建立一个文件cache
                WebRequest request = WebRequest.Create(strUrl);
                WebResponse response = request.GetResponse();

                // Create image.
                image = Image.FromStream(response.GetResponseStream());//.FromFile("SampImag.jpg");
                return 0;
            }
            catch(Exception ex)
            {
                strError = "从'" + strUrl + "'获取图像出错，原因：" + ex.Message;
                return -1;
            }
        }
	}
}
