using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace DigitalPlatform.Drawing
{
    public class AForgeImageUtil
    {

#if NO
        public static System.Drawing.Image AforgeAutoCrop(Bitmap selectedImage,
            DetectBorderParam param)
        {
            // 一些参数的默认值
            if (param.MinObjectWidth == 0)
                param.MinObjectWidth = 500;
            if (param.MinObjectHeight == 0)
                param.MinObjectHeight = 500;

            Bitmap autoCropImage = null;
            try
            {

                autoCropImage = selectedImage;
                // create grayscale filter (BT709)
                Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
                Bitmap grayImage = filter.Apply(autoCropImage);
                // create instance of skew checker
                DocumentSkewChecker skewChecker = new DocumentSkewChecker();
                // get documents skew angle
                double angle = skewChecker.GetSkewAngle(grayImage);
                // create rotation filter
                RotateBilinear rotationFilter = new RotateBilinear(-angle);
                rotationFilter.FillColor = Color.White;
                // rotate image applying the filter
                Bitmap rotatedImage = rotationFilter.Apply(grayImage);
                new ContrastStretch().ApplyInPlace(rotatedImage);
                new Threshold(100).ApplyInPlace(rotatedImage);
                BlobCounter bc = new BlobCounter();
                bc.FilterBlobs = true;
                bc.MinWidth = param.MinObjectWidth; //  500;
                bc.MinHeight = param.MinObjectHeight;   // 500;
                bc.ProcessImage(rotatedImage);
                Rectangle[] rects = bc.GetObjectsRectangles();

                if (rects.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("No rectangle found in image ");
                }
                else if (rects.Length == 1)
                {
                    autoCropImage = rotatedImage.Clone(rects[0], rotatedImage.PixelFormat);
                }
                else if (rects.Length > 1)
                {
                    // get largets rect
                    Console.WriteLine("Using largest rectangle found in image ");
                    var r2 = rects.OrderByDescending(r => r.Height * r.Width).ToList();
                    autoCropImage = rotatedImage.Clone(r2[1], rotatedImage.PixelFormat);
                }
                else
                {
                    Console.WriteLine("Huh? on image ");
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                throw ex;
            }

            return autoCropImage;
        }
#endif

        public static Bitmap Apply(Bitmap selectedImage,
            double angle,
            Rectangle rect)
        {
            System.Drawing.Imaging.PixelFormat old_format = selectedImage.PixelFormat;

            RotateBilinear rotationFilter = new RotateBilinear(-angle);
            rotationFilter.FillColor = Color.White;
            rotationFilter.KeepSize = true;
            // 格式必须为其中之一： rotationFilter.FormatTranslations;
            // rotate image applying the filter
            Bitmap temp = selectedImage.Clone(new Rectangle(0, 0, selectedImage.Width, selectedImage.Height),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Bitmap rotatedImage = rotationFilter.Apply(temp);
#if NO
            return rotatedImage.Clone(rect,
                old_format
                // rotatedImage.PixelFormat
                );
#endif
            return rotatedImage;
        }

        public static bool GetSkewParam(Bitmap selectedImage,
            DetectBorderParam param,
            out double angle,
            out Rectangle rect)
        {
            // 一些参数的默认值
            if (param.MinObjectWidth == 0)
                param.MinObjectWidth = 500;
            if (param.MinObjectHeight == 0)
                param.MinObjectHeight = 500;

            Bitmap autoCropImage = null;
            try
            {

                autoCropImage = selectedImage;

#if NO
                // create grayscale filter (BT709)
                Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
                Bitmap grayImage = filter.Apply(autoCropImage);
#endif
                Bitmap grayImage = selectedImage.Clone(new Rectangle(0, 0, selectedImage.Width, selectedImage.Height),
                System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                // create instance of skew checker
                DocumentSkewChecker skewChecker = new DocumentSkewChecker();
                // get documents skew angle
                angle = skewChecker.GetSkewAngle(grayImage);
                // create rotation filter
                RotateBilinear rotationFilter = new RotateBilinear(-angle);
                rotationFilter.FillColor = Color.Black; // .White;
                rotationFilter.KeepSize = true;
                // rotate image applying the filter
                Bitmap rotatedImage = rotationFilter.Apply(grayImage);
                new ContrastStretch().ApplyInPlace(rotatedImage);
                new Threshold(100).ApplyInPlace(rotatedImage);
                BlobCounter bc = new BlobCounter();
                bc.FilterBlobs = true;
                bc.MinWidth = param.MinObjectWidth; //  500;
                bc.MinHeight = param.MinObjectHeight;   // 500;
#if NO
                bc.MinWidth = 500;// grayImage.Width / 10;  // 500
                bc.MinHeight = 500;// grayImage.Height / 10; // 500
#endif
                bc.ProcessImage(rotatedImage);

                Rectangle[] rects = bc.GetObjectsRectangles();

                if (rects.Length == 0)
                {
                    // System.Windows.Forms.MessageBox.Show("No rectangle found in image ");
                    rect = new Rectangle(0, 0, 0, 0);
                    return false;
                }
                else if (rects.Length == 1)
                {
                    rect = rects[0];
                    // autoCropImage = rotatedImage.Clone(rects[0], rotatedImage.PixelFormat);
                }
                else if (rects.Length > 1)
                {
                    // TODO： 应该把这些矩形合并在一起
                    Rectangle first = new Rectangle(0, 0, 0, 0);
                    int i = 0;
                    foreach (Rectangle one in rects)
                    {
                        Debug.WriteLine("one=" + one.ToString());
                        if (i == 0)
                            first = one;
                        else
                            first = Merge(first, one);
                        i++;
                    }
                    rect = first;
                    Debug.WriteLine("result=" + rect.ToString());
#if NO

                    // get largets rect
                    Console.WriteLine("Using largest rectangle found in image ");
                    var r2 = rects.OrderByDescending(r => r.Height * r.Width).ToList();
                    rect = r2[1];
                    // autoCropImage = rotatedImage.Clone(r2[1], rotatedImage.PixelFormat);
#endif
                }
                else
                {
                    // Console.WriteLine("Huh? on image ");
                    rect = new Rectangle(0, 0, 0, 0);
                    return false;
                }

#if NO
                Blob[] blobs = bc.GetObjectsInformation();
                foreach (var blob in blobs)
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blob);
                    List<IntPoint> cornerPoints;

                    // use the shape checker to extract the corner points
                    if (shapeChecker.IsQuadrilateral(edgePoints, out cornerPoints))
                    {
                        // only do things if the corners form a rectangle
                        if (shapeChecker.CheckPolygonSubType(cornerPoints) == PolygonSubType.Rectangle)
                        {
                            // here i use the graphics class to draw an overlay, but you
                            // could also just use the cornerPoints list to calculate your
                            // x, y, width, height values.
                            List<Point> Points = new List<Point>();
                            foreach (var point in cornerPoints)
                            {
                                Points.Add(new Point(point.X, point.Y));
                            }

                            Graphics g = Graphics.FromImage(image);
                            g.DrawPolygon(new Pen(Color.Red, 5.0f), Points.ToArray());

                            image.Save("result.png");
                        }
                    }
                }
#endif

            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                throw ex;
            }
            finally
            {
                if (autoCropImage != null)
                    autoCropImage.Dispose();
            }
            return true;
        }

        // 应该增补一次识别长条形的图像，也纳入合并的范围
        static List<System.Drawing.Point> GetFourPoints(Rectangle rect1)
        {
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            points.Add(new System.Drawing.Point(rect1.X, rect1.Y));
            points.Add(new System.Drawing.Point(rect1.X, rect1.Y + rect1.Height));
            points.Add(new System.Drawing.Point(rect1.X + rect1.Width, rect1.Y));
            points.Add(new System.Drawing.Point(rect1.X + rect1.Width, rect1.Y + rect1.Height));
            return points;
        }

        static Rectangle Merge(Rectangle rect1, Rectangle rect2)
        {
            // 一共 8 个点。找到最左 上 右 下的点?
            int x_min = 0;
            int x_max = 0;
            int y_min = 0;
            int y_max = 0;
            List<System.Drawing.Point> points = new List<System.Drawing.Point>();
            points.AddRange(GetFourPoints(rect1));
            points.AddRange(GetFourPoints(rect2));
            int i = 0;
            foreach (System.Drawing.Point point in points)
            {
                if (i == 0)
                {
                    x_min = point.X;
                    x_max = point.X;
                    y_min = point.Y;
                    y_max = point.Y;
                }
                else
                {
                    if (point.X < x_min)
                        x_min = point.X;
                    if (point.X > x_max)
                        x_max = point.X;

                    if (point.Y < y_min)
                        y_min = point.Y;
                    if (point.Y > y_max)
                        y_max = point.Y;
                }
                i++;
            }

            return new Rectangle(x_min, y_min, x_max - x_min, y_max - y_min);
        }

        // 根据四个顶点，测算出四边形的长边和短边长度
        // TODO: 需要根据梯形两平行边的差值，估算出平面倒伏的程度(角度)，测算出尽可能完美的边长
        static void GetWidthHeight(List<IntPoint> corners,
            out int width,
            out int height)
        {
            // 1 2
            // 4 3

            // 1 --> 2
            double x = corners[1].X - corners[0].X;
            double y = corners[1].Y- corners[0].Y;

            int width1 = (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            // 4 --> 3
            x = corners[3].X - corners[2].X;
            y = corners[3].Y - corners[2].Y;

            int width2 = (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            width = Math.Max(width1, width2);

            // 1 --> 4
            x = corners[3].X - corners[0].X;
            y = corners[3].Y - corners[0].Y;

            int height1 = (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            // 2 --> 3
            x = corners[2].X - corners[1].X;
            y = corners[2].Y - corners[1].Y;

            int height2 = (int)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            height = Math.Max(height1, height2);
        }

        public static Bitmap Clip(Bitmap source,
            List<IntPoint> corners)
        {
#if NO
            // define quadrilateral's corners
            List<IntPoint> corners = new List<IntPoint>();
            corners.Add(new IntPoint(99, 99));
            corners.Add(new IntPoint(156, 79));
            corners.Add(new IntPoint(184, 126));
            corners.Add(new IntPoint(122, 150));
#endif
            int width = 0;
            int height = 0;
            GetWidthHeight(corners, out width, out height);

            // create filter
            QuadrilateralTransformation filter =
                new QuadrilateralTransformation(corners, width, height);
            // apply the filter
            return filter.Apply(source);
        }
    }

    // 用于边界探测的一些参数
    public class DetectBorderParam
    {
        // 最小物体的宽度、高度
        public int MinObjectWidth { get; set; }
        public int MinObjectHeight { get; set; }

        public DetectBorderParam(System.Drawing.Image image)
        {
            // 宽除以 5， 或者高除以 4
            int nShorter = Math.Min(image.Width, image.Height);
            MinObjectWidth = nShorter / 4;
            MinObjectHeight = MinObjectWidth;
        }
    }
}
