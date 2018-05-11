//-------------------------------------------
// CameraInfo.cs (c) 2007 by Charles Petzold
//-------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Petzold.Media3D
{
    public class CameraInfo : Animatable
    {
        public static readonly Matrix3D ZeroMatrix = new Matrix3D(0, 0, 0, 0, 0, 0, 0, 0, 
                                                                  0, 0, 0, 0, 0, 0, 0, 0);

        public CameraInfo()
        {
        }

        /// <summary>
        ///     Identifies the Name dependency property.
        /// </summary>

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
            typeof(string),
            typeof(CameraInfo));

        /// <summary>
        ///     Gets or sets the identifying name of the object. ETC.
        ///     This is a dependency property.
        /// </summary>

        public string Name
        {
            set { SetValue(NameProperty, value); }
            get { return (string)GetValue(NameProperty); }
        }

        public static readonly DependencyProperty CameraProperty =
            DependencyProperty.Register("Camera",
            typeof(Camera),
            typeof(CameraInfo),
            new PropertyMetadata(null, CameraPropertyChanged));

        public Camera Camera
        {
            set { SetValue(CameraProperty, value); }
            get { return (Camera)GetValue(CameraProperty); }
        }

        public static readonly DependencyProperty ViewportWidthProperty =
            DependencyProperty.Register("ViewportWidth",
            typeof(double),
            typeof(CameraInfo),
            new PropertyMetadata(1.0, ViewportPropertyChanged));

        public double ViewportWidth
        {
            set { SetValue(ViewportWidthProperty, value); }
            get { return (double)GetValue(ViewportWidthProperty); }
        }

        public static readonly DependencyProperty ViewportHeightProperty =
            DependencyProperty.Register("ViewportHeight",
            typeof(double),
            typeof(CameraInfo),
            new PropertyMetadata(1.0, ViewportPropertyChanged));

        public double ViewportHeight
        {
            set { SetValue(ViewportHeightProperty, value); }
            get { return (double)GetValue(ViewportHeightProperty); }
        }

        static readonly DependencyPropertyKey ViewMatrixKey =
            DependencyProperty.RegisterReadOnly("ViewMatrix",
            typeof(Matrix3D),
            typeof(CameraInfo),
            new PropertyMetadata(new Matrix3D()));

        public static readonly DependencyProperty ViewMatrixProperty =
            ViewMatrixKey.DependencyProperty;

        public Matrix3D ViewMatrix
        {
            private set { SetValue(ViewMatrixKey, value); }
            get { return (Matrix3D)GetValue(ViewMatrixProperty); }
        }

        static readonly DependencyPropertyKey ProjectionMatrixKey =
            DependencyProperty.RegisterReadOnly("ProjectionMatrix",
            typeof(Matrix3D),
            typeof(CameraInfo),
            new PropertyMetadata(new Matrix3D()));

        public static readonly DependencyProperty ProjectionMatrixProperty =
            ProjectionMatrixKey.DependencyProperty;

        public Matrix3D ProjectionMatrix
        {
            private set { SetValue(ProjectionMatrixKey, value); }
            get { return (Matrix3D)GetValue(ProjectionMatrixProperty); }
        }

        public static readonly DependencyProperty TotalTransformProperty =
            DependencyProperty.Register("TotalTransform",
            typeof(Matrix3D),
            typeof(CameraInfo));

        public Matrix3D TotalTransform
        {
            protected set { SetValue(TotalTransformProperty, value); }
            get { return (Matrix3D)GetValue(TotalTransformProperty); }
        }

        public static readonly DependencyProperty InverseTransformProperty =
            DependencyProperty.Register("InverseTransform",
            typeof(Matrix3D),
            typeof(CameraInfo));

        public Matrix3D InverseTransform
        {
            protected set { SetValue(InverseTransformProperty, value); }
            get { return (Matrix3D)GetValue(InverseTransformProperty); }
        }

        static void CameraPropertyChanged(DependencyObject obj,
                                          DependencyPropertyChangedEventArgs args)
        {
            CameraInfo caminfo = obj as CameraInfo;
            caminfo.ViewMatrix = GetViewMatrix(caminfo.Camera);
            ViewportPropertyChanged(obj, args);
        }

        static void ViewportPropertyChanged(DependencyObject obj,
                                            DependencyPropertyChangedEventArgs args)
        {
            CameraInfo caminfo = obj as CameraInfo;
            caminfo.ViewMatrix = GetViewMatrix(caminfo.Camera);

            caminfo.ProjectionMatrix = GetProjectionMatrix(caminfo.Camera,
                                            caminfo.ViewportWidth /
                                            caminfo.ViewportHeight);

            // Can these two be made more efficient -- not getting view and projection again ?????

            caminfo.TotalTransform = GetTotalTransform(caminfo.Camera,
                                            caminfo.ViewportWidth /
                                            caminfo.ViewportHeight);

            caminfo.InverseTransform = GetInverseTransform(caminfo.Camera,
                                            caminfo.ViewportWidth /
                                            caminfo.ViewportHeight);
        }

        /// <summary>
        ///     Obtains the view transform matrix for a camera.
        /// </summary>
        /// <param name="camera">
        ///     Camera to obtain the 
        /// </param>
        /// <returns>
        ///     A Matrix3D objecvt with the camera view transform matrix,
        ///     or a Matrix3D with all zeros if the "camera" is null.
        /// </returns>
        /// <exception cref="TK">
        ///     if the 'camera' is neither of type MatrixCamera nor
        ///     ProjectionCamera.
        /// </exception>

        public static Matrix3D GetViewMatrix(Camera camera)
        {
            Matrix3D matx = Matrix3D.Identity;

            if (camera == null)
            {
                matx = ZeroMatrix;
            }

            else if (camera is MatrixCamera)
            {
                matx = (camera as MatrixCamera).ViewMatrix;
            }
            else if (camera is ProjectionCamera)
            {
                ProjectionCamera projcam = camera as ProjectionCamera;

                Vector3D zAxis = -projcam.LookDirection;
                zAxis.Normalize();

                Vector3D xAxis = Vector3D.CrossProduct(projcam.UpDirection, zAxis);
                xAxis.Normalize();

                Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);
                Vector3D pos = (Vector3D)projcam.Position;

                matx = new Matrix3D(xAxis.X, yAxis.X, zAxis.X, 0,
                                             xAxis.Y, yAxis.Y, zAxis.Y, 0,
                                             xAxis.Z, yAxis.Z, zAxis.Z, 0,
                                             -Vector3D.DotProduct(xAxis, pos),
                                             -Vector3D.DotProduct(yAxis, pos),
                                             -Vector3D.DotProduct(zAxis, pos), 1);

            }

            else if (camera != null)
            {
                throw new ApplicationException("ViewMatrix");
            }
            return matx;
        }

        public static Matrix3D GetProjectionMatrix(Camera cam, double aspectRatio)
        {
            Matrix3D matx = Matrix3D.Identity;

            if (cam == null)
            {
                matx = ZeroMatrix;
            }

            else if (cam is MatrixCamera)
            {
                matx = (cam as MatrixCamera).ProjectionMatrix;
            }

            else if (cam is OrthographicCamera)
            {
                OrthographicCamera orthocam = cam as OrthographicCamera;

                double xScale = 2 / orthocam.Width;
                double yScale = xScale * aspectRatio;
                double zNear = orthocam.NearPlaneDistance;
                double zFar = orthocam.FarPlaneDistance;

                // Hey, check this out!
                if (Double.IsPositiveInfinity(zFar))
                    zFar = 1E10;

                matx = new Matrix3D(xScale, 0, 0, 0,
                                    0,      yScale, 0, 0,
                                    0,      0, 1 / (zNear - zFar), 0,
                                    0,     0, zNear / (zNear - zFar), 1);

            }

            else if (cam is PerspectiveCamera)
            {
                PerspectiveCamera perscam = cam as PerspectiveCamera;

                // The angle-to-radian formula is a little off because only
                //  half the angle enters the calculation.
                double xScale = 1 / Math.Tan(Math.PI * perscam.FieldOfView / 360);
                double yScale = xScale * aspectRatio;
                double zNear = perscam.NearPlaneDistance;
                double zFar = perscam.FarPlaneDistance;
                double zScale = (zFar == double.PositiveInfinity ? -1 : (zFar / (zNear - zFar)));
                double zOffset = zNear * zScale;

                matx = new Matrix3D(xScale, 0,      0,       0,
                                    0,      yScale, 0,       0,
                                    0,      0,      zScale, -1,
                                    0,      0,      zOffset, 0);
            }

            else if (cam != null)
            {
                throw new ApplicationException("ProjectionMatrix");
            }
          
            return matx;
        }


        public static Matrix3D GetTotalTransform(Camera cam, double aspectRatio)
        {
            Matrix3D matx = Matrix3D.Identity;

            if (cam == null)
            {
                matx = ZeroMatrix;
            }

            else
            {
                if (cam.Transform != null)
                {
                    Matrix3D matxCameraTransform = cam.Transform.Value;

                    if (!matxCameraTransform.HasInverse)
                    {
                        matx = ZeroMatrix;
                    }
                    else
                    {
                        matxCameraTransform.Invert();
                        matx.Append(matxCameraTransform);
                    }
                }

                matx.Append(CameraInfo.GetViewMatrix(cam));
                matx.Append(CameraInfo.GetProjectionMatrix(cam, aspectRatio));
            }
            return matx;
        }


        public static Matrix3D GetInverseTransform(Camera cam, double aspectRatio)
        {
            Matrix3D matx = GetTotalTransform(cam, aspectRatio);

            if (matx == ZeroMatrix)
            {
                ;
            }
            else if (!matx.HasInverse)
            {
                matx = ZeroMatrix;
            }
            else
            {
                matx.Invert();
            }
            return matx;
        }


        public override string ToString()
        {
            return String.Format("View Matrix: {0}\nProjection Matrix: {1}\nTotal Transform: {2}", ViewMatrix, ProjectionMatrix, TotalTransform);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new CameraInfo();
        }
    }
}
