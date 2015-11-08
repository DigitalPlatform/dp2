using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DigitalPlatform.Drawing
{

    public class ColorUtil
    {
        //根据#XXXXXX格式字符串得到Color
        public static Color String2Color(string strColor)
        {
            string strR = strColor.Substring(1, 2);
            int nR = ConvertUtil.S2Int32(strR, 16);

            string strG = strColor.Substring(3, 2);
            int nG = ConvertUtil.S2Int32(strG, 16);

            string strB = strColor.Substring(5, 2);
            int nB = ConvertUtil.S2Int32(strB, 16);

            return Color.FromArgb(nR, nG, nB);
        }

        //将Color转换成#XXXXXX格式字符串
        public static string Color2String(Color color)
        {
            int nR = color.R;
            string strR = Convert.ToString(nR, 16);
            if (strR == "0")
                strR = "00";

            int nG = color.G;
            string strG = Convert.ToString(nG, 16);
            if (strG == "0")
                strG = "00";

            int nB = color.B;
            string strB = Convert.ToString(nB, 16);
            if (strB == "0")
                strB = "00";

            return "#" + strR + strG + strB;
        }
    }

    public class GraphicsUtil
    {
        // 根据Font，得到字符串的宽度
        // parameter:
        //		font	Font对象
        //		strText	字符串
        // return:
        //		字符串的宽度
        public static int GetWidth(
            Font font,
            string strText)
        {
            Size proposedSize = new Size(60000, 1000);
            Size size = TextRenderer.MeasureText(strText,
                font,
                proposedSize,
                TextFormatFlags.SingleLine | TextFormatFlags.TextBoxControl);
            return size.Width;
        }

        // 根据Font，得到字符串的宽度
        // parameter:
        //		g	Graphics对象
        //		font	Font对象
        //		strText	字符串
        // return:
        //		字符串的宽度
        public static int GetWidth(Graphics g,
            Font font,
            string strText)
        {
            SizeF sizef = new SizeF();
            sizef = g.MeasureString(
                strText,
                font);
            return sizef.ToSize().Width + 6;    // 微调
        }

        // 缩小图像
        // parameters:
        //		nNewWidth0	宽度(0表示不变化)
        //		nNewHeight0	高度
        //      bRatio  是否保持纵横比例
        // return:
        //      -1  出错
        //      0   没有必要缩放(objBitmap未处理)
        //      1   已经缩放
        public static int ShrinkPic(ref Image objBitmap,
            int nNewWidth0,
            int nNewHeight0,
            bool bRatio,
            out string strError)
        {
            strError = "";

            int nNewWidth = nNewWidth0;
            int nNewHeight = nNewHeight0;

            // 不必要缩放
            if (nNewHeight0 == 0 && nNewWidth0 == 0)
            {
                return 0;
            }

            if (nNewWidth == 0 && nNewHeight == 0) // 两边都不限制
                goto NONEED;
            else if (nNewWidth == 0) // 宽度不限制
            {
                if (objBitmap.Height <= nNewHeight)
                    goto NONEED;
                float ratio = (float)nNewHeight / (float)objBitmap.Height;

                nNewWidth = (int)(ratio * (float)objBitmap.Width);
                if (bRatio == true)
                    nNewHeight = (int)(ratio * (float)objBitmap.Height);
            }
            else if (nNewHeight == 0)	// 高度不限制
            {
                if (objBitmap.Width <= nNewWidth)
                    goto NONEED;

                float ratio = (float)nNewWidth / (float)objBitmap.Width;
                nNewHeight = (int)(ratio * (float)objBitmap.Height);
                if (bRatio == true)
                    nNewWidth = (int)(ratio * (float)objBitmap.Width);	// 如果锁定纵横比例
            }
            else // 宽度高度都限制
            {
                float wratio = 1.0F;
                float hratio = 1.0F;

                if (objBitmap.Height > nNewHeight)
                {
                    hratio = (float)nNewHeight / (float)objBitmap.Height;
                }
                if (objBitmap.Width > nNewWidth)
                {
                    wratio = (float)nNewWidth / (float)objBitmap.Width;
                }

                if (bRatio == true)
                {
                    float ratio = Math.Min(wratio, hratio);

                    if (ratio != 1.0)
                    {
                        nNewHeight = (int)(ratio * (float)objBitmap.Height);
                        nNewWidth = (int)(ratio * (float)objBitmap.Width);
                    }
                    else
                    {
                        nNewHeight = objBitmap.Height;
                        nNewWidth = objBitmap.Width;
                    }
                }
                else
                {
                    nNewHeight = (int)(hratio * (float)objBitmap.Height);
                    nNewWidth = (int)(wratio * (float)objBitmap.Width);
                }
            }

            Bitmap BitmapDest = new Bitmap(nNewWidth, nNewHeight);

            using (Graphics objGraphics = Graphics.FromImage(BitmapDest))
            {
                Rectangle compressionRectangle = new Rectangle(0,
                    0, nNewWidth, nNewHeight);

                /*
                using (Brush trans_brush = new SolidBrush(Color.White))
                {
                    objGraphics.FillRectangle(trans_brush, compressionRectangle);
                }
                 * */

                // set Drawing Quality 
                objGraphics.InterpolationMode = InterpolationMode.High;
                objGraphics.DrawImage(objBitmap, compressionRectangle);
            }

            objBitmap.Dispose();
            objBitmap = BitmapDest;
            return 1;	//  成功保存
        ERROR1:
            return -1;
        NONEED:
            return 0;
        }

        // 缩小图像
        // parameters:
        //      oFile   传入的文件流
        //      strContentType  文件类型字符串
        //		nNewWidth0	宽度(0表示不变化)
        //		nNewHeight	高度
        //      oTargetFile:目标流
        // return:
        //      -1  出错
        //      0   没有必要缩放(oTarget未处理)
        //      1   已经缩放
        public static int ShrinkPic(Stream oFile,
            string strContentType,
            int nNewWidth0,
            int nNewHeight0,
            bool bRatio,
            Stream oTargetFile,
            out string strError)
        {
            strError = "";

            int nNewWidth = nNewWidth0;
            int nNewHeight = nNewHeight0;

            // 不必要缩放
            if (nNewHeight0 == 0 && nNewWidth0 == 0)
            {
                return 0;
            }

            Bitmap objBitmap = null;

            try
            {
                objBitmap = new Bitmap(oFile);
            }
            catch (Exception ex)
            {
                strError = "创建bitmap出错: " + ex.Message;
                goto ERROR1;
            }

            if (nNewWidth == 0 && nNewHeight == 0) // 两边都不限制
                goto NONEED;
            else if (nNewWidth == 0) // 宽度不限制
            {
                if (objBitmap.Height <= nNewHeight)
                    goto NONEED;
                float ratio = (float)nNewHeight / (float)objBitmap.Height;

                nNewWidth = (int)(ratio * (float)objBitmap.Width);
                if (bRatio == true)
                    nNewHeight = (int)(ratio * (float)objBitmap.Height);
            }
            else if (nNewHeight == 0)	// 高度不限制
            {
                if (objBitmap.Width <= nNewWidth)
                    goto NONEED;

                float ratio = (float)nNewWidth / (float)objBitmap.Width;
                nNewHeight = (int)(ratio * (float)objBitmap.Height);
                if (bRatio == true)
                    nNewWidth = (int)(ratio * (float)objBitmap.Width);	// 如果锁定纵横比例
            }
            else // 宽度高度都限制
            {
                float wratio = 1.0F;
                float hratio = 1.0F;

                if (objBitmap.Height > nNewHeight)
                {
                    hratio = (float)nNewHeight / (float)objBitmap.Height;
                }
                if (objBitmap.Width > nNewWidth)
                {
                    wratio = (float)nNewWidth / (float)objBitmap.Width;
                }

                if (bRatio == true)
                {
                    float ratio = Math.Min(wratio, hratio);

                    if (ratio != 1.0)
                    {
                        nNewHeight = (int)(ratio * (float)objBitmap.Height);
                        nNewWidth = (int)(ratio * (float)objBitmap.Width);
                    }
                    else
                    {
                        goto NONEED;    // 2012/5/23
                        // nNewHeight = objBitmap.Height;
                        // nNewWidth = objBitmap.Width;
                    }
                }
                else
                {
                    nNewHeight = (int)(hratio * (float)objBitmap.Height);
                    nNewWidth = (int)(wratio * (float)objBitmap.Width);
                }
            }

            Bitmap BitmapDest = new Bitmap(nNewWidth, nNewHeight);

            using (Graphics objGraphics = Graphics.FromImage(BitmapDest))
            {
                Rectangle compressionRectangle = new Rectangle(0,
                    0, nNewWidth, nNewHeight);

                // set Drawing Quality 
                objGraphics.InterpolationMode = InterpolationMode.High;
                objGraphics.DrawImage(objBitmap, compressionRectangle);

                try
                {
                    BitmapDest.Save(
                        oTargetFile,
                        GetImageType(strContentType));	// System.drawing.Imaging.ImageFormat.Jpeg)
                }
                catch (Exception ex)
                {
                    BitmapDest.Dispose();
                    objBitmap.Dispose();
                    // 2010/12/29 add
                    strError = "BitmapDest.Save()抛出异常 strContentType='" + strContentType + "' : " + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
            }

            BitmapDest.Dispose();
            objBitmap.Dispose();
            return 1;	//  成功保存
        ERROR1:
            if (objBitmap != null)
                objBitmap.Dispose();
            return -1;
        NONEED:
            if (objBitmap != null)
                objBitmap.Dispose();
            return 0;

        }


        static System.Drawing.Imaging.ImageFormat GetImageType(string strContentType)
        {
            strContentType = strContentType.ToString().ToLower();

            switch (strContentType)
            {
                case "image/pjpeg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case "image/jpeg":
                    return System.Drawing.Imaging.ImageFormat.Jpeg;
                case "image/gif":
                    return System.Drawing.Imaging.ImageFormat.Gif;
                case "image/bmp":
                    return System.Drawing.Imaging.ImageFormat.Bmp;
                case "image/tiff":
                    return System.Drawing.Imaging.ImageFormat.Tiff;
                case "image/x-icon":
                    return System.Drawing.Imaging.ImageFormat.Icon;
                case "image/x-png":
                case "image/png":
                    return System.Drawing.Imaging.ImageFormat.Png;
                case "image/x-emf":
                    return System.Drawing.Imaging.ImageFormat.Emf;
                case "image/x-exif":
                    return System.Drawing.Imaging.ImageFormat.Exif;
                case "image/x-wmf":
                    return System.Drawing.Imaging.ImageFormat.Wmf;
            }
            return System.Drawing.Imaging.ImageFormat.MemoryBmp;
        }

    }

}

