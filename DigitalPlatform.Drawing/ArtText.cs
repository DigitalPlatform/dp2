using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace DigitalPlatform.Drawing
{
    [Flags]
    public enum ArtEffect
    {
        None = 0x00,
        Shadow = 0x01,
    }

    public class TextInfo
    {
        // public string Text = "";
        public string FontFace = "";
        public float FontSize = 0;
        public FontStyle fontstyle = FontStyle.Regular;
        public Color colorText = Color.Black;
        public Color colorBack = Color.White;
        public Color colorShadow = Color.Gray;
        public ArtEffect effect = ArtEffect.None;
        // public ImageFormat imageformat = ImageFormat.Gif;
        // public int Width = 500;
        // public int Height = 100;

    }

    public class ArtText
    {
        // 把字符串参数翻译为整数
        static int GetValue(string strText,
            int value,
            int width,
            int height)
        {
            if (strText.IndexOf("%") != -1)
            {
                string strValue = strText.Substring(0, strText.Length - 1);
                double v = 0;
                double.TryParse(strValue,
                     out v);
                return (int)((double)value * v / (double)100);
            }

            if (strText == "0")
                return 0;
            if (strText == "width")
                return width;
            if (strText == "height")
                return height;

            {
                int v = 0;
                Int32.TryParse(strText,
                     out v);
                return v;
            }
        }

        //  max_width   100% 50% 
        //  x   "center" "0" "width"
        public static MemoryStream PaintText(
            string strSourceFileName,
            string strText,
            TextInfo info,
            string s_x,
            string s_y,
            string s_max_width,
            string s_max_height,
            ImageFormat imageformat)
        {
            SizeF size;

            using (Image source = Image.FromFile(strSourceFileName))
            {

                int width = source.Width;
                int height = source.Height;

                int x = 0;

                if (s_x != "center")
                    x = GetValue(s_x,
                     width,
                     width,
                     height);

                int y = GetValue(s_y,
        height,
        width,
        height);
                int max_width = GetValue(s_max_width,
        width,
        width,
        height);
                int max_height = GetValue(s_max_height,
    height,
    width,
    height);
                Font font = null;
                try
                {
                    using (Bitmap bitmapTemp = new Bitmap(1, 1))
                    {
                        using (Graphics graphicsTemp = Graphics.FromImage(bitmapTemp))
                        {
                            int text_height = (int)((float)max_height * 0.8);
                            // 第一次测算，按照最大高度
                            font = new Font(info.FontFace, text_height, info.fontstyle, GraphicsUnit.Pixel);

                            size = graphicsTemp.MeasureString(
                                strText,
                                font);

                            int width_delta = (int)size.Width - max_width;
                            if (width_delta > 0)
                            {
                                int nFontHeight = (int)((float)text_height * ((float)max_width / size.Width));
                                if (font != null)
                                    font.Dispose();
                                font = new Font(info.FontFace, nFontHeight, info.fontstyle, GraphicsUnit.Pixel);
                                y += (text_height - nFontHeight) / 2;
                            }

                            if ((info.effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                            {
                                size.Height += 2;
                                size.Width += 2;
                            }
                        }
                    }

                    // 正式的图像
                    using (Bitmap bitmapDest = new Bitmap(
                        source.Width,   //                (int)size.Width + 1,
                        Math.Max(source.Height, y + max_height),  // (int)size.Height + 1,
                        PixelFormat.Format64bppPArgb))
                    {
                        using (Graphics objGraphics = Graphics.FromImage(bitmapDest))
                        {

                            objGraphics.Clear(info.colorBack);// Color.Transparent

                            objGraphics.DrawImageUnscaled(source, new Point(0, 0));


                            // 
                            objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                            // System.Drawing.Text.TextRenderingHint oldrenderhint = objGraphics.TextRenderingHint;
                            //设置高质量,低速度呈现平滑程度 
                            objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                            StringFormat stringFormat = new StringFormat();

                            if (s_x == "center")
                            {
                                stringFormat.Alignment = StringAlignment.Center;
                                x = 0;
                                size.Width = source.Width;
                            }
                            else
                                stringFormat.Alignment = StringAlignment.Near;

                            using (Brush objBrush = new SolidBrush(info.colorText)) // 透明颜色 ' Color.Black
                            {
                                RectangleF rect = new RectangleF(x,
                                y,
                                size.Width,
                                size.Height);

                                if ((info.effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                                {
                                    using (Brush objBrushShadow = new SolidBrush(info.colorShadow))
                                    {
                                        RectangleF rectShadow = new RectangleF(rect.X,
                                            rect.Y, rect.Width, rect.Height);
                                        rectShadow.Offset(2, 2);
                                        objGraphics.DrawString(strText,
                                            font,
                                            objBrushShadow,
                                            rectShadow,
                                            stringFormat);
                                    }
                                }

                                objGraphics.DrawString(strText,
                                    font,
                                    objBrush,
                                    rect,
                                    stringFormat);
                            }

                            MemoryStream stream = new MemoryStream();

                            if (imageformat == ImageFormat.Gif)
                            {
                                bitmapDest.MakeTransparent(
                                info.colorBack);

                                OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
                                quantizer.TransparentColor = info.colorBack;

                                using (Bitmap quantized = quantizer.Quantize(bitmapDest))
                                {
                                    quantized.Save(stream, imageformat);
                                }
                            }
                            else
                            {
                                bitmapDest.Save(stream, imageformat);   // System.Drawing.Imaging.ImageFormat.Jpeg
                            }

                            return stream;
                        }
                    }
                }
                finally
                {
                    if (font != null)
                        font.Dispose();
                }
            }
        }

        // 2016/10/5 改造为利用下级 Bitmap 函数
        // parameters:
        //      nWidth  控制折行的位置
        public static MemoryStream BuildArtText(
            string strText,
            string strFontFace,
            float fFontSize,
            FontStyle fontstyle,
            Color colorText,
            Color colorBack,
            Color colorShadow,
            ArtEffect effect,
            ImageFormat imageformat,
            int nWidth = 500)
        {
            // 正式的图像
            using (Bitmap bitmapDest = BuildArtText(strText,
                strFontFace,
                fFontSize,
                fontstyle,
                colorText,
                colorBack,
                colorShadow,
                effect,
                nWidth))
            {
                MemoryStream stream = new MemoryStream();

                if (imageformat == ImageFormat.Png
                    && colorBack == Color.Transparent)
                {
                    bitmapDest.MakeTransparent(colorBack);
                }

                if (imageformat == ImageFormat.Gif)
                {
                    bitmapDest.MakeTransparent(
                    colorBack);

                    OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
                    quantizer.TransparentColor = colorBack;

                    using (Bitmap quantized = quantizer.Quantize(bitmapDest))
                    {
                        quantized.Save(stream, imageformat);
                    }
                }
                else
                {
                    bitmapDest.Save(stream, imageformat);   // System.Drawing.Imaging.ImageFormat.Jpeg
                }

                return stream;
            }
        }

#if NO
        // parameters:
        //      nWidth  控制折行的位置
        public static MemoryStream BuildArtText(
            string strText,
            string strFontFace,
            float fFontSize,
            FontStyle fontstyle,
            Color colorText,
            Color colorBack,
            Color colorShadow,
            ArtEffect effect,
            ImageFormat imageformat,
            int nWidth = 500)
        {
            SizeF size;
            using (Font font = new Font(strFontFace, fFontSize, fontstyle))
            {
                using (Bitmap bitmapTemp = new Bitmap(1, 1))
                {
                    using (Graphics graphicsTemp = Graphics.FromImage(bitmapTemp))
                    {
                        size = graphicsTemp.MeasureString(
                            strText,
                            font,
                            nWidth);

                        if ((effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                        {
                            size.Height += 2;
                            size.Width += 2;
                        }
                    }
                }

                // 正式的图像
                using (Bitmap bitmapDest = new Bitmap((int)size.Width + 1, (int)size.Height + 1, PixelFormat.Format64bppPArgb))
                {
                    using (Graphics objGraphics = Graphics.FromImage(bitmapDest))
                    {
                        // colorBack = Color.FromArgb(0, colorBack);
                        objGraphics.Clear(colorBack);// Color.Transparent

                        // 
                        objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        // System.Drawing.Text.TextRenderingHint oldrenderhint = objGraphics.TextRenderingHint;
                        //设置高质量,低速度呈现平滑程度 
                        objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        // objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Near;
                        stringFormat.LineAlignment = StringAlignment.Center;    // 2016/5/24

                        // Color.FromArgb(128, 100, 100, 100)
                        using (Brush objBrush = new SolidBrush(colorText)) // 透明颜色 ' Color.Black
                        {
                            RectangleF rect = new RectangleF(0, 0, size.Width, size.Height);

                            if ((effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                            {
                                using (Brush objBrushShadow = new SolidBrush(colorShadow))
                                {
                                    RectangleF rectShadow = new RectangleF(rect.X,
                                        rect.Y, rect.Width, rect.Height);
                                    rectShadow.Offset(2, 2);
                                    objGraphics.DrawString(strText,
                                        font,
                                        objBrushShadow,
                                        rectShadow,
                                        stringFormat);
                                }
                            }

                            objGraphics.DrawString(strText,
                                font,
                                objBrush,
                                rect,
                                stringFormat);
                        }
                    }

                    MemoryStream stream = new MemoryStream();

                    /*
                    stream = SaveGIFWithNewColorTable(
                        bitmapDest,
                        256,
                        true);
                     */
                    if (imageformat == ImageFormat.Png
                        && colorBack == Color.Transparent)
                    {
                        bitmapDest.MakeTransparent(colorBack);
                    }

                    if (imageformat == ImageFormat.Gif)
                    {
                        bitmapDest.MakeTransparent(
                        colorBack);

                        OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
                        quantizer.TransparentColor = colorBack;

                        using (Bitmap quantized = quantizer.Quantize(bitmapDest))
                        {
                            quantized.Save(stream, imageformat);
                        }
                    }
                    else
                    {
                        bitmapDest.Save(stream, imageformat);   // System.Drawing.Imaging.ImageFormat.Jpeg
                    }

                    return stream;
                }
            }
        }

#endif

        //根据#XXXXXX格式字符串得到Color
        public static Color ColorFromHexString(string strColor)
        {
            string strR = strColor.Substring(1, 2);
            int nR = ConvertUtil.S2Int32(strR, 16);

            string strG = strColor.Substring(3, 2);
            int nG = ConvertUtil.S2Int32(strG, 16);

            string strB = strColor.Substring(5, 2);
            int nB = ConvertUtil.S2Int32(strB, 16);

            return Color.FromArgb(nR, nG, nB);
        }

        public static MemoryStream SaveGIFWithNewColorTable(
    Image image,
    uint nColors,
    bool fTransparent
    )
        {

            // GIF codec supports 256 colors maximum, monochrome minimum.
            if (nColors > 256)
                nColors = 256;
            if (nColors < 2)
                nColors = 2;

            // Make a new 8-BPP indexed bitmap that is the same size as the source image.
            int Width = image.Width;
            int Height = image.Height;

            // Always use PixelFormat8bppIndexed because that is the color
            // table-based interface to the GIF codec.
            Bitmap bitmap = new Bitmap(Width,
                                    Height,
                                    PixelFormat.Format8bppIndexed);

            // Create a color palette big enough to hold the colors you want.
            ColorPalette pal = GetColorPalette(nColors);

            // Initialize a new color table with entries that are determined
            // by some optimal palette-finding algorithm; for demonstration 
            // purposes, use a grayscale.
            for (uint i = 0; i < nColors; i++)
            {
                uint Alpha = 0xFF;                      // Colors are opaque.
                uint Intensity = i * 0xFF / (nColors - 1);    // Even distribution. 

                // The GIF encoder makes the first entry in the palette
                // that has a ZERO alpha the transparent color in the GIF.
                // Pick the first one arbitrarily, for demonstration purposes.

                if (i == 0 && fTransparent) // Make this color index...
                    Alpha = 0;          // Transparent

                // Create a gray scale for demonstration purposes.
                // Otherwise, use your favorite color reduction algorithm
                // and an optimum palette for that algorithm generated here.
                // For example, a color histogram, or a median cut palette.
                pal.Entries[i] = Color.FromArgb((int)Alpha,
                                                (int)Intensity,
                                                (int)Intensity,
                                                (int)Intensity);
            }

            // Set the palette into the new Bitmap object.
            bitmap.Palette = pal;


            // Use GetPixel below to pull out the color data of Image.
            // Because GetPixel isn't defined on an Image, make a copy 
            // in a Bitmap instead. Make a new Bitmap that is the same size as the
            // image that you want to export. Or, try to
            // interpret the native pixel format of the image by using a LockBits
            // call. Use PixelFormat32BppARGB so you can wrap a Graphics  
            // around it.
            Bitmap BmpCopy = new Bitmap(Width,
                                    Height,
                                    PixelFormat.Format32bppArgb);
            {
                Graphics g = Graphics.FromImage(BmpCopy);

                g.PageUnit = GraphicsUnit.Pixel;

                // Transfer the Image to the Bitmap
                g.DrawImage(image, 0, 0, Width, Height);

                // g goes out of scope and is marked for garbage collection.
                // Force it, just to keep things clean.
                g.Dispose();
            }

            // Lock a rectangular portion of the bitmap for writing.
            BitmapData bitmapData;
            Rectangle rect = new Rectangle(0, 0, Width, Height);

            bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            // Write to the temporary buffer that is provided by LockBits.
            // Copy the pixels from the source image in this loop.
            // Because you want an index, convert RGB to the appropriate
            // palette index here.
            IntPtr pixels = bitmapData.Scan0;

            unsafe
            {
                // Get the pointer to the image bits.
                // This is the unsafe operation.
                byte* pBits;
                if (bitmapData.Stride > 0)
                    pBits = (byte*)pixels.ToPointer();
                else
                    // If the Stide is negative, Scan0 points to the last 
                    // scanline in the buffer. To normalize the loop, obtain
                    // a pointer to the front of the buffer that is located 
                    // (Height-1) scanlines previous.
                    pBits = (byte*)pixels.ToPointer() + bitmapData.Stride * (Height - 1);
                uint stride = (uint)Math.Abs(bitmapData.Stride);

                for (uint row = 0; row < Height; ++row)
                {
                    for (uint col = 0; col < Width; ++col)
                    {
                        // Map palette indexes for a gray scale.
                        // If you use some other technique to color convert,
                        // put your favorite color reduction algorithm here.
                        Color pixel;    // The source pixel.

                        // The destination pixel.
                        // The pointer to the color index byte of the
                        // destination; this real pointer causes this
                        // code to be considered unsafe.
                        byte* p8bppPixel = pBits + row * stride + col;

                        pixel = BmpCopy.GetPixel((int)col, (int)row);

                        // Use luminance/chrominance conversion to get grayscale.
                        // Basically, turn the image into black and white TV.
                        // Do not calculate Cr or Cb because you 
                        // discard the color anyway.
                        // Y = Red * 0.299 + Green * 0.587 + Blue * 0.114

                        // This expression is best as integer math for performance,
                        // however, because GetPixel listed earlier is the slowest 
                        // part of this loop, the expression is left as 
                        // floating point for clarity.

                        double luminance = (pixel.R * 0.299) +
                            (pixel.G * 0.587) +
                            (pixel.B * 0.114);

                        // Gray scale is an intensity map from black to white.
                        // Compute the index to the grayscale entry that
                        // approximates the luminance, and then round the index.
                        // Also, constrain the index choices by the number of
                        // colors to do, and then set that pixel's index to the 
                        // byte value.
                        *p8bppPixel = (byte)(luminance * (nColors - 1) / 255 + 0.5);

                    } /* end loop for col */
                } /* end loop for row */
            } /* end unsafe */

            // To commit the changes, unlock the portion of the bitmap.  
            bitmap.UnlockBits(bitmapData);

            MemoryStream stream = new MemoryStream();

            bitmap.Save(stream, ImageFormat.Gif);

            // Bitmap goes out of scope here and is also marked for
            // garbage collection.
            // Pal is referenced by bitmap and goes away.
            // BmpCopy goes out of scope here and is marked for garbage
            // collection. Force it, because it is probably quite large.
            // The same applies to bitmap.
            BmpCopy.Dispose();
            bitmap.Dispose();

            return stream;
        }

        static ColorPalette GetColorPalette(uint nColors)
        {
            // Assume monochrome image.
            PixelFormat bitscolordepth = PixelFormat.Format1bppIndexed;
            ColorPalette palette;    // The Palette we are stealing
            Bitmap bitmap;     // The source of the stolen palette

            // Determine number of colors.
            if (nColors > 2)
                bitscolordepth = PixelFormat.Format4bppIndexed;
            if (nColors > 16)
                bitscolordepth = PixelFormat.Format8bppIndexed;

            // Make a new Bitmap object to get its Palette.
            bitmap = new Bitmap(1, 1, bitscolordepth);

            palette = bitmap.Palette;   // Grab the palette

            bitmap.Dispose();           // cleanup the source Bitmap

            return palette;             // Send the palette back
        }

        // parameters:
        //      nWidth  控制折行的位置
        public static Bitmap BuildArtText(
            string strText,
            string strFontFace,
            float fFontSize,
            FontStyle fontstyle,
            Color colorText,
            Color colorBack,
            Color colorShadow,
            ArtEffect effect,
            // ImageFormat imageformat,
            int nWidth = 500)
        {
            Bitmap bitmapDest = null;

            SizeF size;
            using (Font font = new Font(strFontFace, fFontSize, fontstyle))
            {
                using (Bitmap bitmapTemp = new Bitmap(1, 1))
                {
                    using (Graphics graphicsTemp = Graphics.FromImage(bitmapTemp))
                    {
                        size = graphicsTemp.MeasureString(
                            strText,
                            font,
                            nWidth);

                        if ((effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                        {
                            size.Height += 2;
                            size.Width += 2;
                        }
                    }
                }

                // 正式的图像
                bitmapDest = new Bitmap((int)size.Width + 1, (int)size.Height + 1, PixelFormat.Format64bppPArgb);

                using (Graphics objGraphics = Graphics.FromImage(bitmapDest))
                {
                    // colorBack = Color.FromArgb(0, colorBack);
                    objGraphics.Clear(colorBack);// Color.Transparent

                    // 
                    objGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    // System.Drawing.Text.TextRenderingHint oldrenderhint = objGraphics.TextRenderingHint;
                    //设置高质量,低速度呈现平滑程度 
                    objGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    // objGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Near;
                    stringFormat.LineAlignment = StringAlignment.Center;    // 2016/5/24

                    // Color.FromArgb(128, 100, 100, 100)
                    using (Brush objBrush = new SolidBrush(colorText)) // 透明颜色 ' Color.Black
                    {
                        RectangleF rect = new RectangleF(0, 0, size.Width, size.Height);

                        if ((effect & ArtEffect.Shadow) == ArtEffect.Shadow)
                        {
                            using (Brush objBrushShadow = new SolidBrush(colorShadow))
                            {
                                RectangleF rectShadow = new RectangleF(rect.X,
                                    rect.Y, rect.Width, rect.Height);
                                rectShadow.Offset(2, 2);
                                objGraphics.DrawString(strText,
                                    font,
                                    objBrushShadow,
                                    rectShadow,
                                    stringFormat);
                            }
                        }

                        objGraphics.DrawString(strText,
                            font,
                            objBrush,
                            rect,
                            stringFormat);
                    }
                }
            }

            return bitmapDest;
        }
    }
}
