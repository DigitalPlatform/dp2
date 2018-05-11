//---------------------------------------------
// ViewportInfo.cs (c) 2007 by Charles Petzold
//---------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class ViewportInfo : Animatable
    {
        public static readonly Matrix3D ZeroMatrix = new Matrix3D(0, 0, 0, 0, 0, 0, 0, 0,
                                                                  0, 0, 0, 0, 0, 0, 0, 0);

        public static readonly DependencyProperty Viewport3DProperty =
            DependencyProperty.Register("Viewport3D",
                typeof(Viewport3D),
                typeof(ViewportInfo),
                new PropertyMetadata(null, Viewport3DChanged));

        public Viewport3D Viewport3D
        {
            set { SetValue(Viewport3DProperty, value); }
            get { return (Viewport3D)GetValue(Viewport3DProperty); }
        }

        static readonly DependencyPropertyKey TransformKey =
            DependencyProperty.RegisterReadOnly("Transform",
                typeof(Matrix3D),
                typeof(ViewportInfo),
                new PropertyMetadata(new Matrix3D()));

        public static readonly DependencyProperty TransformProperty =
            TransformKey.DependencyProperty;

        public Matrix3D Transform
        {
            protected set { SetValue(TransformKey, value); }
            get { return (Matrix3D)GetValue(TransformProperty); }
        }


        // Properties: Total Transform, Camera Transform, Viewport Transform.


        static void Viewport3DChanged(DependencyObject obj,
                                      DependencyPropertyChangedEventArgs args)
        {
            (obj as ViewportInfo).Viewport3DChanged(args);
        }

        void Viewport3DChanged(DependencyPropertyChangedEventArgs args)
        {
            if (Viewport3D == null)
                Transform = ZeroMatrix;

            else
            {
                Transform = CameraInfo.GetTotalTransform(Viewport3D.Camera,
                                        Viewport3D.ActualWidth / Viewport3D.ActualHeight);
            }
        }




        public static Matrix3D GetTotalTransform(Viewport3DVisual vis)
        {
            Matrix3D matx = GetCameraTransform(vis);
            matx.Append(GetViewportTransform(vis));
            return matx;
        }

        public static Matrix3D GetTotalTransform(Viewport3D viewport)
        {
            Matrix3D matx = GetCameraTransform(viewport);
            matx.Append(GetViewportTransform(viewport));
            return matx;
        }

        public static Matrix3D GetCameraTransform(Viewport3DVisual vis)
        {
            return CameraInfo.GetTotalTransform(vis.Camera,
                                vis.Viewport.Size.Width / vis.Viewport.Size.Height);
        }

        public static Matrix3D GetCameraTransform(Viewport3D viewport)
        {
            return CameraInfo.GetTotalTransform(viewport.Camera,
                                viewport.ActualWidth / viewport.ActualHeight);
        }

        public static Matrix3D GetViewportTransform(Viewport3DVisual vis)
        {
            return new Matrix3D(vis.Viewport.Width / 2, 0,                        0, 0,
                                0,                      -vis.Viewport.Height / 2, 0, 0,
                                0,                      0,                        1, 0,
                                         vis.Viewport.X + vis.Viewport.Width / 2,
                                                 vis.Viewport.Y + vis.Viewport.Height / 2, 0, 1);

        }

        public static Matrix3D GetViewportTransform(Viewport3D viewport)
        {
            return new Matrix3D(viewport.ActualWidth / 2, 0, 0, 0,
                                0, -viewport.ActualHeight / 2, 0, 0,
                                                 0, 0, 1, 0,
                                                 viewport.ActualWidth / 2,
                                                 viewport.ActualHeight / 2, 0, 1);
        }




        public static Point Point3DtoPoint2D(Viewport3D viewport, Point3D point)
        {
            Matrix3D matx = GetTotalTransform(viewport);
            Point3D pointTransformed = matx.Transform(point);
            Point pt = new Point(pointTransformed.X, pointTransformed.Y);
            return pt;
        }

        public static bool Point2DtoPoint3D(Viewport3D viewport, Point ptIn, out LineRange range)
        {
            range = new LineRange();

            Point3D pointIn = new Point3D(ptIn.X, ptIn.Y, 0);
            Matrix3D matxViewport = GetViewportTransform(viewport);
            Matrix3D matxCamera = GetCameraTransform(viewport);

            if (!matxViewport.HasInverse)
                return false;

            if (!matxCamera.HasInverse)
                return false;

            matxViewport.Invert();
            matxCamera.Invert();

            Point3D pointNormalized = matxViewport.Transform(pointIn);
            pointNormalized.Z = 0.01;
            Point3D pointNear = matxCamera.Transform(pointNormalized);
            pointNormalized.Z = 0.99;
            Point3D pointFar = matxCamera.Transform(pointNormalized);

            range = new LineRange(pointNear, pointFar);

            return true;
        }

        public static bool Point2DtoPoint3D(Viewport3D viewport, 
            Vector3D vector3D,
            out Vector3D target)
        {
            target = new Vector3D();

            Matrix3D matxViewport = GetViewportTransform(viewport);
            Matrix3D matxCamera = GetCameraTransform(viewport);

            if (!matxViewport.HasInverse)
                return false;

            if (!matxCamera.HasInverse)
                return false;

            matxViewport.Invert();
            matxCamera.Invert();

            Vector3D pointNormalized = matxViewport.Transform(vector3D);
            target = matxCamera.Transform(pointNormalized);
            return true;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ViewportInfo();
        }
    }
}
