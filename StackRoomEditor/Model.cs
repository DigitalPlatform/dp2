using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml;
using System.IO;

using System.Windows.Media.Media3D;
using _3DTools;

namespace StackRoomEditor
{
    public static class Model
    {
      /// <summary>
      /// Creates a ModelVisual3D containing a text label.
      /// </summary>
      /// <param name="text">The string to be drawn</param>
      /// <param name="textColor">The color of the text.</param>
      /// <param name="isDoubleSided">Visible from both sides?</param>
      /// <param name="height">Height of the characters</param>
      /// <param name="basePoint">The base point of the label</param>
      /// <param name="isBasePointCenterPoint">if set to <c>true</c> the base point 
      /// is center point of the label.</param>
      /// <param name="vectorOver">Horizontal direction of the label</param>
      /// <param name="vectorUp">Vertical direction of the label</param>
      /// <returns>Suitable for adding to your Viewport3D</returns>
      /// <remarks>Two vectors: vectorOver and vectorUp are creating the surface 
      /// on which we are drawing the text. Both those vectors are used for 
      /// calculation of label size, so it is resonable that each co-ordinate 
      /// should be 0 or 1. e.g. [1,1,0] or [1,0,1], etc...</remarks>
        public static GeometryModel3D CreateTextLabel3D(
          string text,
          Brush textColor,
            Brush backColor,
          bool isDoubleSided,
          double width,
          double height,
          Point3D basePoint,
          bool isBasePointCenterPoint,
          Vector3D vectorOver,
          Vector3D vectorUp,
            out TextBlock textblock)
      {
        // First we need a textbox containing the text of our label
        textblock = new TextBlock(new Run(text));
        textblock.Foreground = textColor; // setting the text color
        textblock.FontFamily = new FontFamily("Arial"); // setting the font to be used
        textblock.FontWeight = FontWeights.Black;
        textblock.VerticalAlignment = VerticalAlignment.Top;
        textblock.HorizontalAlignment = HorizontalAlignment.Left;
        textblock.SnapsToDevicePixels = true;
        textblock.Padding = new Thickness(1);
        // Now use that TextBox as the brush for a material
        DiffuseMaterial mataterialWithLabel = new DiffuseMaterial();
        // Allows the application of a 2-D brush, 
        // like a SolidColorBrush or TileBrush, to a diffusely-lit 3-D model. 
        // we are creating the brush from the TextBlock    
            VisualBrush brush = new VisualBrush(textblock);
            brush.Stretch = Stretch.Uniform;
            //brush.Viewport = new Rect(0,0,width, height);
            //brush.ViewportUnits = BrushMappingMode.Absolute;
            mataterialWithLabel.Brush = brush;
            
        //calculation of text width (assumming that characters are square):
        //// width = text.Length * height;
        // we need to find the four corners
        // p0: the lower left corner;  p1: the upper left
        // p2: the lower right; p3: the upper right
        Point3D p0 = basePoint;
        // when the base point is the center point we have to set it up in different way
        if(isBasePointCenterPoint)
          p0 = basePoint - width / 2 * vectorOver - height / 2 * vectorUp;
        Point3D p1 = p0 + vectorUp * 1 * height;
        Point3D p2 = p0 + vectorOver * width;
        Point3D p3 = p0 + vectorUp * 1 * height + vectorOver * width;
        // we are going to create object in 3D now:
        // this object will be painted using the (text) brush created before
        // the object is rectangle made of two triangles (on each side).
        MeshGeometry3D mg_RestangleIn3D = new MeshGeometry3D();
        mg_RestangleIn3D.Positions = new Point3DCollection();
        mg_RestangleIn3D.Positions.Add(p0);    // 0
        mg_RestangleIn3D.Positions.Add(p1);    // 1
        mg_RestangleIn3D.Positions.Add(p2);    // 2
        mg_RestangleIn3D.Positions.Add(p3);    // 3
        // when we want to see the text on both sides:
        if (isDoubleSided)
        {
          mg_RestangleIn3D.Positions.Add(p0);    // 4
          mg_RestangleIn3D.Positions.Add(p1);    // 5
          mg_RestangleIn3D.Positions.Add(p2);    // 6
          mg_RestangleIn3D.Positions.Add(p3);    // 7
        }
        mg_RestangleIn3D.TriangleIndices.Add(0);
        mg_RestangleIn3D.TriangleIndices.Add(3);
        mg_RestangleIn3D.TriangleIndices.Add(1);
        mg_RestangleIn3D.TriangleIndices.Add(0);
        mg_RestangleIn3D.TriangleIndices.Add(2);
        mg_RestangleIn3D.TriangleIndices.Add(3);
        // when we want to see the text on both sides:
        if (isDoubleSided)
        {
          mg_RestangleIn3D.TriangleIndices.Add(4);
          mg_RestangleIn3D.TriangleIndices.Add(5);
          mg_RestangleIn3D.TriangleIndices.Add(7);
          mg_RestangleIn3D.TriangleIndices.Add(4);
          mg_RestangleIn3D.TriangleIndices.Add(7);
          mg_RestangleIn3D.TriangleIndices.Add(6);
        }
        // texture coordinates must be set to
        // stretch the TextBox brush to cover 
        // the full side of the 3D label.
        mg_RestangleIn3D.TextureCoordinates.Add(new Point(0, 1));
        mg_RestangleIn3D.TextureCoordinates.Add(new Point(0, 0));
        mg_RestangleIn3D.TextureCoordinates.Add(new Point(1, 1));
        mg_RestangleIn3D.TextureCoordinates.Add(new Point(1, 0));
        // when the label is double sided:
        if (isDoubleSided)
        {
          mg_RestangleIn3D.TextureCoordinates.Add(new Point(1, 1));
          mg_RestangleIn3D.TextureCoordinates.Add(new Point(1, 0));
          mg_RestangleIn3D.TextureCoordinates.Add(new Point(0, 1));
          mg_RestangleIn3D.TextureCoordinates.Add(new Point(0, 0));
        }

        MaterialGroup g = new MaterialGroup();
        g.Children.Add(new EmissiveMaterial(backColor));
        g.Children.Add(mataterialWithLabel);
        // g.Children.Add(new SpecularMaterial(Brushes.LightYellow, 100));

        GeometryModel3D result =  new GeometryModel3D();
        result.Geometry = mg_RestangleIn3D;
        result.Material = g;
        result.BackMaterial = new DiffuseMaterial(Brushes.Black);
        return result;

        // return new GeometryModel3D(mg_RestangleIn3D, mataterialWithLabel);
      }
    }
}
