//---------------------------------------------------------------------------
//
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Limited Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedpermissivelicense.mspx
// All other rights reserved.
//
// This file is part of the 3D Tools for Windows Presentation Foundation
// project.  For more information, see:
// 
// http://CodePlex.com/Wiki/View.aspx?ProjectName=3DTools
//
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Composition;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Documents;
using System.Collections;

namespace _3DTools
{
    /// <summary>
    /// Helper class that encapsulates return data needed for the
    /// hit test capture methods.
    /// </summary>
    public class HitTestEdge
    {
        /// <summary>
        /// Constructs a new hit test edge
        /// </summary>
        /// <param name="p1">First edge point</param>
        /// <param name="p2">Second edge point</param>
        /// <param name="uv1">Texture coordinate of first edge point</param>
        /// <param name="uv2">Texture coordinate of second edge point</param>
        public HitTestEdge(Point3D p1,
                           Point3D p2,
                           Point uv1,
                           Point uv2)
        {
            _p1 = p1;
            _p2 = p2;

            _uv1 = uv1;
            _uv2 = uv2;
        }

        /// <summary>
        /// Projects the stored 3D points in to 2D.
        /// </summary>
        /// <param name="objectToViewportTransform">The transformation matrix to use</param>
        public void Project(Matrix3D objectToViewportTransform)
        {
            Point3D projPoint1 = objectToViewportTransform.Transform(_p1);
            Point3D projPoint2 = objectToViewportTransform.Transform(_p2);

            _p1Transformed = new Point(projPoint1.X, projPoint1.Y);
            _p2Transformed = new Point(projPoint2.X, projPoint2.Y);
        }

        public Point3D _p1, _p2;
        public Point _uv1, _uv2;

        // the transformed Point3D value
        public Point _p1Transformed, _p2Transformed;
    }

    /// <summary>
    /// The InteractiveModelVisual3D class represents a model visual 3D that can 
    /// be interacted with.  The class adds some properties that make it easy
    /// to construct an interactive 3D object (geometry and visual), and also makes
    /// it so those Visual3Ds that want to be interactive can explicitly state this
    /// via their type.
    /// </summary>
    public class InteractiveVisual3D : ModelVisual3D
    {
        /// <summary>
        /// Constructs a new InteractiveModelVisual3D
        /// </summary>
        public InteractiveVisual3D()
        {
            InternalVisualBrush = CreateVisualBrush();

            // create holders for the intersection plane and content
            _content = new GeometryModel3D();
            Content = _content;

            GenerateMaterial();
        }

        static InteractiveVisual3D()
        {
            _defaultMaterialPropertyValue = new DiffuseMaterial();
            _defaultMaterialPropertyValue.SetValue(InteractiveVisual3D.IsInteractiveMaterialProperty, true);
            _defaultMaterialPropertyValue.Freeze();

            MaterialProperty = DependencyProperty.Register("Material",
                                                           typeof(Material),
                                                           typeof(InteractiveVisual3D),
                                                           new PropertyMetadata(_defaultMaterialPropertyValue, 
                                                                                new PropertyChangedCallback(OnMaterialPropertyChanged)));
        }

        /// <summary>
        /// When a property of the IMV3D changes we play it safe and invalidate the saved
        /// corner cache.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // invalidate the cache
            _lastVisCorners = null;
        }

        /// <summary>
        /// Gets the visual edges that correspond to the passed in texture coordinates of interest.
        /// </summary>
        /// <param name="texCoordsOfInterest">The texture coordinates whose edges should be found</param>
        /// <returns>The visual edges corresponding to the given texture coordinates</returns>
        internal List<HitTestEdge> GetVisualEdges(Point[] texCoordsOfInterest)
        {
            // get and then cache the edges
            _lastEdges = GrabValidEdges(texCoordsOfInterest);
            _lastVisCorners = texCoordsOfInterest;

            return _lastEdges;
        }

        /// <summary>
        /// Function takes the passed in list of texture coordinate points, and then finds the 
        /// visible outline of the rectangle specified by those points and returns it.
        /// </summary>
        /// <param name="tc">The points specifying the rectangle to search for</param>
        /// <returns>The edges of that rectangle</returns>
        private List<HitTestEdge> GrabValidEdges(Point[] tc)
        {
            // our final edge list
            List<HitTestEdge> hitTestEdgeList = new List<HitTestEdge>();
            Dictionary<Edge, EdgeInfo> adjInformation = new Dictionary<Edge, EdgeInfo>();

            // store some important info in local variables for easier access
            MeshGeometry3D contentGeom = ((MeshGeometry3D)_content.Geometry);
            Point3DCollection positions = contentGeom.Positions;
            PointCollection textureCoords = contentGeom.TextureCoordinates;
            Int32Collection triIndices = contentGeom.TriangleIndices;
            Transform3D contentTransform = _content.Transform;

            // we want to map from the camera space to this visual3D's space
            Viewport3DVisual containingVisual;
            bool success;

            // this call actually gets the object to camera transform, but we will invert it later, and because of that
            // the local variable is named cameraToObjecTransform.
            Matrix3D cameraToObjectTransform = MathUtils.TryTransformToCameraSpace(this, out containingVisual, out success);
            if (!success) return new List<HitTestEdge>();

            // take in to account any transform on model
            if (contentTransform != null)
            {
                cameraToObjectTransform.Prepend(contentTransform.Value);
            }

            // also get the object to screen space transform for use later
            Matrix3D objectToViewportTransform = MathUtils.TryTransformTo2DAncestor(this, out containingVisual, out success);
            if (!success) return new List<HitTestEdge>();
            if (contentTransform != null)
            {
                objectToViewportTransform.Prepend(contentTransform.Value);
            }

            // check the cached copy to avoid extra work
            bool sameAsBefore = _lastVisCorners != null;
            if (_lastVisCorners != null)
            {
                for (int i = 0; i < tc.Length; i++)
                {
                    if (tc[i] != _lastVisCorners[i])
                    {
                        sameAsBefore = false;
                        break;
                    }
                }

                if (_lastMatrix3D != objectToViewportTransform)
                {
                    sameAsBefore = false;
                }
            }
            if (sameAsBefore) return _lastEdges;

            // save the matrix that was just used
            _lastMatrix3D = objectToViewportTransform;

            // try to invert so we actually have the camera->object transform
            try
            {
                cameraToObjectTransform.Invert();
            }
            catch (InvalidOperationException)
            {
                return new List<HitTestEdge>();
            }

            Point3D camPosObjSpace = cameraToObjectTransform.Transform(new Point3D(0, 0, 0));

            // get the bounding box around the passed in texture coordinates to help
            // with early rejection tests
            Rect bbox = Rect.Empty;
            for (int i = 0; i < tc.Length; i++)
            {
                bbox.Union(tc[i]);
            }

            // walk through the triangles - and look for the triangles we care about
            int[] indices = new int[3];
            Point3D[] p = new Point3D[3];
            Point[] uv = new Point[3];

            for (int i = 0; i < triIndices.Count; i += 3)
            {
                // get the triangle indices
                Rect triBBox = Rect.Empty;

                for (int j = 0; j < 3; j++)
                {
                    indices[j] = triIndices[i + j];

                    p[j] = positions[indices[j]];
                    uv[j] = textureCoords[indices[j]];

                    triBBox.Union(uv[j]);
                }

                if (bbox.IntersectsWith(triBBox))
                {
                    ProcessTriangle(p, uv, tc, hitTestEdgeList, adjInformation, camPosObjSpace);
                }
            }

            // also handle the case of an edge that doesn't also have a backface - i.e a single plane            
            foreach (Edge edge in adjInformation.Keys)
            {
                EdgeInfo ei = adjInformation[edge];

                if (ei.hasFrontFace && ei.numSharing == 1)
                {
                    HandleSilhouetteEdge(ei.uv1, ei.uv2,
                                         edge._start, edge._end,
                                         tc,
                                         hitTestEdgeList);
                }
            }

            // project all the edges to get at the 2D point of interest
            for (int i = 0; i < hitTestEdgeList.Count; i++)
            {
                hitTestEdgeList[i].Project(objectToViewportTransform);
            }

            return hitTestEdgeList;
        }

        /// <summary>
        /// Processes the passed in triangle by checking to see if it is facing the camera and if
        /// so searches to see if the texture coordinate edges intersect it.  It also looks
        /// to see if there are any silhouette edges and processes these as well.
        /// </summary>
        /// <param name="p">The triangle's vertices</param>
        /// <param name="uv">The texture coordinates for those vertices</param>   
        /// <param name="tc">The texture coordinate edges to intersect with</param>
        /// <param name="edgeList">The edge list that results should be placed on</param>
        /// <param name="adjInformation">The adjacency information for the mesh</param>
        private void ProcessTriangle(Point3D[] p,
                                     Point[] uv,
                                     Point[] tc,
                                     List<HitTestEdge> edgeList,
                                     Dictionary<Edge, EdgeInfo> adjInformation,
                                     Point3D camPosObjSpace)
        {
            // calculate the normal of the mesh and the vector from a point on the mesh to the camera 
            // for back face removal calculations.
            Vector3D normal = Vector3D.CrossProduct(p[1] - p[0], p[2] - p[0]);
            Vector3D dirToCamera = camPosObjSpace - p[0];

            // ignore any triangles that have a normal of (0,0,0)
            if (!(normal.X == 0 && normal.Y == 0 && normal.Z == 0))
            {
                double dotProd = Vector3D.DotProduct(normal, dirToCamera);

                // if the dot product is > 0 then the triangle is visible, otherwise invisible
                if (dotProd > 0.0)
                {
                    // loop over the triangle and update any edge information
                    ProcessTriangleEdges(p, uv, tc, PolygonSide.FRONT, edgeList, adjInformation);

                    // intersect the bounds of the visual with the triangle
                    ProcessVisualBoundsIntersections(p, uv, tc, edgeList);
                }
                else
                {
                    ProcessTriangleEdges(p, uv, tc, PolygonSide.BACK, edgeList, adjInformation);
                }
            }
        }

        /// <summary>
        /// Function intersects the edges specified by tc with the texture coordinates
        /// on the passed in triangle.  If there are any intersections, the edges
        /// of these intersections are added to the edgelist
        /// </summary>
        /// <param name="p">The vertices of the triangle</param>
        /// <param name="uv">The texture coordinates for that triangle</param>
        /// <param name="tc">The texture coordinate edges to be intersected against</param>
        /// <param name="edgeList">The list of edges any intersecte edges should be added to</param>
        private void ProcessVisualBoundsIntersections(Point3D[] p,
                                                      Point[] uv,
                                                      Point[] tc,
                                                      List<HitTestEdge> edgeList)
        {
            List<Point3D> pointList = new List<Point3D>();
            List<Point> uvList = new List<Point>();

            // loop over the visual's texture coordinate bounds
            for (int i = 0; i < tc.Length; i++)
            {
                Point visEdgeStart = tc[i];
                Point visEdgeEnd = tc[(i + 1) % tc.Length];

                // clear out anything that used to be there
                pointList.Clear();
                uvList.Clear();

                // loop over triangle edges
                bool skipListProcessing = false;
                for (int j = 0; j < uv.Length; j++)
                {
                    Point uv1 = uv[j];
                    Point uv2 = uv[(j + 1) % uv.Length];
                    Point3D p3D1 = p[j];
                    Point3D p3D2 = p[(j + 1) % p.Length];

                    // initial rejection processing
                    if (!((Math.Max(visEdgeStart.X, visEdgeEnd.X) < Math.Min(uv1.X, uv2.X)) ||
                          (Math.Min(visEdgeStart.X, visEdgeEnd.X) > Math.Max(uv1.X, uv2.X)) ||
                          (Math.Max(visEdgeStart.Y, visEdgeEnd.Y) < Math.Min(uv1.Y, uv2.Y)) ||
                          (Math.Min(visEdgeStart.Y, visEdgeEnd.Y) > Math.Max(uv1.Y, uv2.Y))))
                    {
                        // intersect the two lines
                        bool areCoincident = false;
                        Vector dir = uv2 - uv1;
                        double t = IntersectRayLine(uv1, dir, visEdgeStart, visEdgeEnd, out areCoincident);

                        // if they are coincident then we have two intersections and don't need to
                        // do anymore processing
                        if (areCoincident)
                        {
                            HandleCoincidentLines(visEdgeStart, visEdgeEnd,
                                                  p3D1, p3D2,
                                                  uv1, uv2, edgeList);
                            skipListProcessing = true;
                            break;
                        }
                        else if (t >= 0 && t <= 1)
                        {
                            Point intersUV = uv1 + dir * t;
                            Point3D intersPoint3D = p3D1 + (p3D2 - p3D1) * t;

                            double visEdgeDiff = (visEdgeStart - visEdgeEnd).Length;

                            if ((intersUV - visEdgeStart).Length < visEdgeDiff &&
                                (intersUV - visEdgeEnd).Length < visEdgeDiff)
                            {
                                pointList.Add(intersPoint3D);
                                uvList.Add(intersUV);
                            }
                        }
                    }
                }

                if (!skipListProcessing)
                {
                    if (pointList.Count >= 2)
                    {
                        edgeList.Add(new HitTestEdge(pointList[0], pointList[1],
                                                     uvList[0], uvList[1]));
                    }
                    else if (pointList.Count == 1)
                    {
                        Point3D outputPoint;

                        // To avoid an edge cases caused by generating a point extremely
                        // close to one of the bound points, we test if both points are inside
                        // the bounds to be on the safe side - in the worst case we do 
                        // extra work or generate a small edge
                        if (IsPointInTriangle(visEdgeStart, uv, p, out outputPoint))
                        {
                            edgeList.Add(new HitTestEdge(pointList[0], outputPoint,
                                                         uvList[0], visEdgeStart));
                        }

                        if (IsPointInTriangle(visEdgeEnd, uv, p, out outputPoint))
                        {
                            edgeList.Add(new HitTestEdge(pointList[0], outputPoint,
                                                         uvList[0], visEdgeEnd));
                        }
                    }
                    else
                    {
                        Point3D outputPoint1, outputPoint2;

                        if (IsPointInTriangle(visEdgeStart, uv, p, out outputPoint1) &&
                            IsPointInTriangle(visEdgeEnd, uv, p, out outputPoint2))
                        {
                            edgeList.Add(new HitTestEdge(outputPoint1, outputPoint2,
                                                         visEdgeStart, visEdgeEnd));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function tests to see if the given texture coordinate point p is contained within the 
        /// given triangle.  If it is it returns the 3D point corresponding to that intersection.
        /// </summary>
        /// <param name="p">The point to test</param>
        /// <param name="triUVVertices">The texture coordinates of the triangle</param>
        /// <param name="tri3DVertices">The 3D coordinates of the triangle</param>
        /// <param name="inters3DPoint">The 3D point of intersection</param>
        /// <returns>True if the point is in the triangle, false otherwise</returns>
        private bool IsPointInTriangle(Point p, Point[] triUVVertices, Point3D[] tri3DVertices, out Point3D inters3DPoint)
        {
            double denom = 0.0;
            inters3DPoint = new Point3D();

            double A = triUVVertices[0].X - triUVVertices[2].X;
            double B = triUVVertices[1].X - triUVVertices[2].X;
            double C = triUVVertices[2].X - p.X;
            double D = triUVVertices[0].Y - triUVVertices[2].Y;
            double E = triUVVertices[1].Y - triUVVertices[2].Y;
            double F = triUVVertices[2].Y - p.Y;

            denom = (A * E - B * D);
            if (denom == 0) return false;
            double lambda1 = (B * F - C * E) / denom;

            denom = (B * D - A * E);
            if (denom == 0) return false;
            double lambda2 = (A * F - C * D) / denom;

            if (lambda1 < 0 || lambda1 > 1 || lambda2 < 0 || lambda2 > 1 || (lambda1 + lambda2) > 1) return false;

            inters3DPoint = (Point3D)(lambda1 * (Vector3D)tri3DVertices[0] +
                                      lambda2 * (Vector3D)tri3DVertices[1] +
                                      (1.0f - lambda1 - lambda2) * (Vector3D)tri3DVertices[2]);

            return true;
        }

        /// <summary>
        /// Handles adding an edge when the two line segments are coincident.
        /// </summary>
        /// <param name="visUV1">The texture coordinates of the boundary edge</param>
        /// <param name="visUV2">The texture coordinates of the boundary edge</param>
        /// <param name="tri3D1">The 3D coordinate of the triangle edge</param>
        /// <param name="tri3D2">The 3D coordinates of the triangle edge</param>
        /// <param name="triUV1">The texture coordinates of the triangle edge</param>
        /// <param name="triUV2">The texture coordinates of the triangle edge</param>
        /// <param name="edgeList">The edge list to add to</param>
        private void HandleCoincidentLines(Point visUV1, Point visUV2,
                                           Point3D tri3D1, Point3D tri3D2,
                                           Point triUV1, Point triUV2,
                                           List<HitTestEdge> edgeList)
        {
            Point minVisUV, maxVisUV;

            Point minTriUV, maxTriUV;
            Point3D minTri3D, maxTri3D;

            // to be used in final edge creation
            Point uv1, uv2;
            Point3D p1, p2;

            // order the points and give refs to them for ease of use
            if (Math.Abs(visUV1.X - visUV2.X) > Math.Abs(visUV1.Y - visUV2.Y))
            {
                if (visUV1.X <= visUV2.X)
                {
                    minVisUV = visUV1;
                    maxVisUV = visUV2;
                }
                else
                {
                    minVisUV = visUV2;
                    maxVisUV = visUV1;
                }

                if (triUV1.X <= triUV2.X)
                {
                    minTriUV = triUV1;
                    minTri3D = tri3D1;

                    maxTriUV = triUV2;
                    maxTri3D = tri3D2;
                }
                else
                {
                    minTriUV = triUV2;
                    minTri3D = tri3D2;

                    maxTriUV = triUV1;
                    maxTri3D = tri3D1;
                }

                // now actually create the edge           
                // compute the minimum value
                if (minVisUV.X < minTriUV.X)
                {
                    uv1 = minTriUV;
                    p1 = minTri3D;
                }
                else
                {
                    uv1 = minVisUV;
                    p1 = minTri3D + (minVisUV.X - minTriUV.X) / (maxTriUV.X - minTriUV.X) * (maxTri3D - minTri3D);
                }

                // compute the maximum value
                if (maxVisUV.X > maxTriUV.X)
                {
                    uv2 = maxTriUV;
                    p2 = maxTri3D;
                }
                else
                {
                    uv2 = maxVisUV;
                    p2 = minTri3D + (maxVisUV.X - minTriUV.X) / (maxTriUV.X - minTriUV.X) * (maxTri3D - minTri3D);
                }
            }
            else
            {
                if (visUV1.Y <= visUV2.Y)
                {
                    minVisUV = visUV1;
                    maxVisUV = visUV2;
                }
                else
                {
                    minVisUV = visUV2;
                    maxVisUV = visUV1;
                }

                if (triUV1.Y <= triUV2.Y)
                {
                    minTriUV = triUV1;
                    minTri3D = tri3D1;

                    maxTriUV = triUV2;
                    maxTri3D = tri3D2;
                }
                else
                {
                    minTriUV = triUV2;
                    minTri3D = tri3D2;

                    maxTriUV = triUV1;
                    maxTri3D = tri3D1;
                }

                // now actually create the edge           
                // compute the minimum value
                if (minVisUV.Y < minTriUV.Y)
                {
                    uv1 = minTriUV;
                    p1 = minTri3D;
                }
                else
                {
                    uv1 = minVisUV;
                    p1 = minTri3D + (minVisUV.Y - minTriUV.Y) / (maxTriUV.Y - minTriUV.Y) * (maxTri3D - minTri3D);
                }

                // compute the maximum value
                if (maxVisUV.Y > maxTriUV.Y)
                {
                    uv2 = maxTriUV;
                    p2 = maxTri3D;
                }
                else
                {
                    uv2 = maxVisUV;
                    p2 = minTri3D + (maxVisUV.Y - minTriUV.Y) / (maxTriUV.Y - minTriUV.Y) * (maxTri3D - minTri3D);
                }
            }

            // add the edge
            edgeList.Add(new HitTestEdge(p1, p2, uv1, uv2));
        }

        /// <summary>
        /// Intersects a ray with the line specified by the passed in end points.  The parameterized coordinate along the ray of
        /// intersection is returned.  
        /// </summary>
        /// <param name="o">The ray origin</param>
        /// <param name="d">The ray direction</param>
        /// <param name="p1">First point of the line to intersect against</param>
        /// <param name="p2">Second point of the line to intersect against</param>
        /// <param name="coinc">Whether the ray and line are coincident</param>        
        /// <returns>
        /// The parameter along the ray of the point of intersection.
        /// If the ray and line are parallel and not coincident, this will be -1.
        /// </returns>
        private double IntersectRayLine(Point o, Vector d, Point p1, Point p2, out bool coinc)
        {
            coinc = false;

            // deltas
            double dy = p2.Y - p1.Y;
            double dx = p2.X - p1.X;

            // handle case of a vertical line
            if (dx == 0)
            {
                if (d.X == 0)
                {
                    coinc = (o.X == p1.X);
                    return -1;
                }
                else
                {
                    return (p2.X - o.X) / d.X;
                }
            }

            // now need to do more general intersection
            double numer = (o.X - p1.X) * dy / dx - o.Y + p1.Y;
            double denom = (d.Y - d.X * dy / dx);

            // if denominator is zero, then the lines are parallel
            if (denom == 0)
            {
                double b0 = -o.X * dy / dx + o.Y;
                double b1 = -p1.X * dy / dx + p1.Y;

                coinc = (b0 == b1);
                return -1;
            }
            else
            {
                return (numer / denom);
            }
        }

        /// <summary>
        /// Helper structure to represent an edge
        /// </summary>
        private struct Edge
        {
            public Edge(Point3D s, Point3D e)
            {
                _start = s;
                _end = e;
            }

            public Point3D _start;
            public Point3D _end;
        }

        /// <summary>
        /// Information about an edge such as whether it belongs to a front/back facing
        /// triangle, the texture coordinates for the edge, and how many polygons refer
        /// to that edge.
        /// </summary>
        private class EdgeInfo
        {
            public EdgeInfo()
            {
                hasFrontFace = hasBackFace = false;
                numSharing = 0;
            }

            public bool hasFrontFace;
            public bool hasBackFace;
            public Point uv1;
            public Point uv2;
            public int numSharing;
        }

        /// <summary>
        /// Processes the edges of the given triangle.  It does so by updating
        /// the adjacency information based on the direction the polygon is facing.
        /// If there is a silhouette edge found, then this edge is added to the list
        /// of edges if it is within the texture coordinate bounds passed to the function.
        /// </summary>
        /// <param name="p">The triangle's vertices</param>
        /// <param name="uv">The texture coordinates for those vertices</param>
        /// <param name="tc">The texture coordinate edges being searched for</param>
        /// <param name="polygonSide">Which side the polygon is facing (greateer than 0 front, less than 0 back)</param>
        /// <param name="edgeList">The list of edges comprosing the visual outline</param>
        /// <param name="adjInformation">The adjacency information structure</param>
        private void ProcessTriangleEdges(Point3D[] p,
                                          Point[] uv,
                                          Point[] tc,
                                          PolygonSide polygonSide,
                                          List<HitTestEdge> edgeList,
                                          Dictionary<Edge, EdgeInfo> adjInformation)
        {
            // loop over all the edges and add them to the adjacency list
            for (int i = 0; i < p.Length; i++)
            {
                Point uv1, uv2;
                Point3D p3D1 = p[i];
                Point3D p3D2 = p[(i + 1) % p.Length];

                Edge edge;

                // order the edge points so insertion in to adjInformation is consistent
                if (p3D1.X < p3D2.X ||
                   (p3D1.X == p3D2.X && p3D1.Y < p3D2.Y) ||
                   (p3D1.X == p3D2.X && p3D1.Y == p3D2.Y && p3D1.Z < p3D1.Z))
                {
                    edge = new Edge(p3D1, p3D2);
                    uv1 = uv[i];
                    uv2 = uv[(i + 1) % p.Length];
                }
                else
                {
                    edge = new Edge(p3D2, p3D1);
                    uv2 = uv[i];
                    uv1 = uv[(i + 1) % p.Length];
                }

                // look up the edge information
                EdgeInfo edgeInfo;
                if (adjInformation.ContainsKey(edge))
                {
                    edgeInfo = adjInformation[edge];
                }
                else
                {
                    edgeInfo = new EdgeInfo();
                    adjInformation[edge] = edgeInfo;
                }
                edgeInfo.numSharing++;

                // whether or not the edge has already been added to the edge list
                bool alreadyAdded = edgeInfo.hasBackFace && edgeInfo.hasFrontFace;

                // add the edge to the info list
                if (polygonSide == PolygonSide.FRONT)
                {
                    edgeInfo.hasFrontFace = true;
                    edgeInfo.uv1 = uv1;
                    edgeInfo.uv2 = uv2;
                }
                else
                {
                    edgeInfo.hasBackFace = true;
                }

                // if the sides are different we may need to add an edge
                if (!alreadyAdded && edgeInfo.hasBackFace && edgeInfo.hasFrontFace)
                {
                    HandleSilhouetteEdge(edgeInfo.uv1, edgeInfo.uv2,
                                        edge._start, edge._end,
                                        tc,
                                        edgeList);
                }
            }
        }

        /// <summary>
        /// Handles intersecting a silhouette edge against the passed in texture coordinate 
        /// bounds.  It behaves similarly to the case of intersection the bounds with a triangle 
        /// except the testing order is switched.
        /// </summary>
        /// <param name="uv1">The texture coordinates of the edge</param>
        /// <param name="uv2">The texture coordinates of the edge</param>
        /// <param name="p3D1">The 3D point of the edge</param>
        /// <param name="p3D2">The 3D point of the edge</param>
        /// <param name="bounds">The texture coordinate bounds</param>
        /// <param name="edgeList">The list of edges</param>
        private void HandleSilhouetteEdge(Point uv1, Point uv2,
                                          Point3D p3D1, Point3D p3D2,
                                          Point[] bounds,
                                          List<HitTestEdge> edgeList)
        {
            List<Point3D> pointList = new List<Point3D>();
            List<Point> uvList = new List<Point>();
            Vector dir = uv2 - uv1;

            // loop over object bounds
            for (int i = 0; i < bounds.Length; i++)
            {
                Point visEdgeStart = bounds[i];
                Point visEdgeEnd = bounds[(i + 1) % bounds.Length];

                // initial rejection processing
                if (!((Math.Max(visEdgeStart.X, visEdgeEnd.X) < Math.Min(uv1.X, uv2.X)) ||
                      (Math.Min(visEdgeStart.X, visEdgeEnd.X) > Math.Max(uv1.X, uv2.X)) ||
                      (Math.Max(visEdgeStart.Y, visEdgeEnd.Y) < Math.Min(uv1.Y, uv2.Y)) ||
                      (Math.Min(visEdgeStart.Y, visEdgeEnd.Y) > Math.Max(uv1.Y, uv2.Y))))
                {
                    // intersect the two lines
                    bool areCoincident = false;
                    double t = IntersectRayLine(uv1, dir, visEdgeStart, visEdgeEnd, out areCoincident);

                    // silhouette edge processing will only include non-coincident lines
                    if (areCoincident)
                    {
                        // if it's coincident, we'll let the normal processing handle this edge
                        return;
                    }
                    else if (t >= 0 && t <= 1)
                    {
                        Point intersUV = uv1 + dir * t;
                        Point3D intersPoint3D = p3D1 + (p3D2 - p3D1) * t;

                        double visEdgeDiff = (visEdgeStart - visEdgeEnd).Length;

                        if ((intersUV - visEdgeStart).Length < visEdgeDiff &&
                            (intersUV - visEdgeEnd).Length < visEdgeDiff)
                        {
                            pointList.Add(intersPoint3D);
                            uvList.Add(intersUV);
                        }
                    }
                }
            }

            if (pointList.Count >= 2)
            {
                edgeList.Add(new HitTestEdge(pointList[0], pointList[1],
                                             uvList[0], uvList[1]));
            }
            else if (pointList.Count == 1)
            {
                // for the case that uv1/2 is actually a point on or extremely close to the bounds
                // of the polygon, we do the pointinpolygon test on both to avoid any numerical
                // precision issues - in the worst case we end up with a very small edge and
                // the right edge
                if (IsPointInPolygon(bounds, uv1))
                {
                    edgeList.Add(new HitTestEdge(pointList[0], p3D1,
                                                 uvList[0], uv1));
                }
                if (IsPointInPolygon(bounds, uv2))
                {
                    edgeList.Add(new HitTestEdge(pointList[0], p3D2,
                                                 uvList[0], uv2));
                }
            }
            else
            {
                if (IsPointInPolygon(bounds, uv1) &&
                    IsPointInPolygon(bounds, uv2))
                {
                    edgeList.Add(new HitTestEdge(p3D1, p3D2,
                                                 uv1, uv2));
                }
            }
        }

        /// <summary>
        /// Function tests to see whether the point p is contained within the polygon
        /// specified by the list of points passed to the function.  p is considered within
        /// this polygon if it is on the same side of all the edges.  A point on any of
        /// the edges of the polygon is not considered within the polygon.
        /// </summary>
        /// <param name="polygon">The polygon to test against</param>
        /// <param name="p">The point to be tested against</param>
        /// <returns>Whether the point is in the polygon</returns>
        private bool IsPointInPolygon(Point[] polygon, Point p)
        {
            bool sign = false;

            for (int i = 0; i < polygon.Length; i++)
            {
                double crossProduct = Vector.CrossProduct(polygon[(i + 1) % polygon.Length] - polygon[i],
                                                          polygon[i] - p);

                bool currSign = crossProduct > 0;

                if (i == 0)
                {
                    sign = currSign;
                }
                else
                {
                    if (sign != currSign) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// GenerateMaterial creates the material for the InteractiveModelVisual3D.  The
        /// material is composed of the Visual, which is displayed on a VisualBrush on a 
        /// DiffuseMaterial, as well as any post materials which are also applied.
        /// </summary>
        private void GenerateMaterial()
        {
            Material material;

            // begin order dependent operations            
            InternalVisualBrush.Visual = null;
            InternalVisualBrush = CreateVisualBrush();
            
            material = Material.Clone();
            _content.Material = material;

            InternalVisualBrush.Visual = InternalVisual;

            SwapInVisualBrush(material);
            // end order dependent operations
            
            if (IsBackVisible)
            {
                _content.BackMaterial = material;
            }
        }

        /// <summary>
        /// Creates the VisualBrush that will be used to hold the interactive
        /// 2D content.
        /// </summary>
        /// <returns>The VisualBrush to hold the interactive 2D content</returns>
        private VisualBrush CreateVisualBrush()
        {
            VisualBrush vb = new VisualBrush();
            RenderOptions.SetCachingHint(vb, CachingHint.Cache);
            vb.ViewportUnits = BrushMappingMode.Absolute;
            vb.TileMode = TileMode.None;

            return vb;
        }

        /// <summary>
        /// Replaces any instances of the sentinal brush with the internal visual brush
        /// </summary>
        /// <param name="material">The material to look through</param>
        private void SwapInVisualBrush(Material material)
        {
            bool foundMaterialToSwap = false;
            Stack<Material> materialStack = new Stack<Material>();
            materialStack.Push(material);

            while (materialStack.Count > 0)
            {
                Material currMaterial = materialStack.Pop();

                if (currMaterial is DiffuseMaterial)
                {
                    DiffuseMaterial diffMaterial = (DiffuseMaterial)currMaterial;
                    if ((Boolean)diffMaterial.GetValue(InteractiveVisual3D.IsInteractiveMaterialProperty))
                    {
                        diffMaterial.Brush = InternalVisualBrush;
                        foundMaterialToSwap = true;
                    }
                }
                else if (currMaterial is EmissiveMaterial)
                {
                    EmissiveMaterial emmMaterial = (EmissiveMaterial)currMaterial;
                    if ((Boolean)emmMaterial.GetValue(InteractiveVisual3D.IsInteractiveMaterialProperty))
                    {
                        emmMaterial.Brush = InternalVisualBrush;
                        foundMaterialToSwap = true;
                    }
                }
                else if (currMaterial is SpecularMaterial)
                {
                    SpecularMaterial specMaterial = (SpecularMaterial)currMaterial;
                    if ((Boolean)specMaterial.GetValue(InteractiveVisual3D.IsInteractiveMaterialProperty))
                    {
                        specMaterial.Brush = InternalVisualBrush;
                        foundMaterialToSwap = true;
                    }
                }
                else if (currMaterial is MaterialGroup)
                {
                    MaterialGroup matGroup = (MaterialGroup)currMaterial;
                    foreach (Material m in matGroup.Children)
                    {
                        materialStack.Push(m);
                    }
                }
                else
                {
                    throw new ArgumentException("material needs to be either a DiffuseMaterial, EmissiveMaterial, SpecularMaterial or a MaterialGroup",
                                                "material");
                }
            }

            // make sure there is at least one interactive material
            if (!foundMaterialToSwap)
            {
                throw new ArgumentException("material needs to contain at least one material that has the IsInteractiveMaterial attached property",
                                            "material");
            }
        }

        /// <summary>
        /// The visual applied to the VisualBrush, which is then used on the 3D object
        /// </summary>
        private static DependencyProperty VisualProperty =
            DependencyProperty.Register(
                "Visual",
                typeof(Visual),
                typeof(InteractiveVisual3D),
                new PropertyMetadata(null, new PropertyChangedCallback(OnVisualChanged)));

        public Visual Visual
        {
            get { return (Visual)GetValue(VisualProperty); }
            set { SetValue(VisualProperty, value); }
        }

        /// <summary>
        /// The actual visual being placed on the brush.
        /// so that the patterns on visuals caused by tabbing, etc... work, 
        /// we wrap the Visual DependencyProperty in a AdornerDecorator.
        /// </summary>
        internal UIElement InternalVisual
        {
            get { return _internalVisual; }
        }

        /// <summary>
        /// The visual brush that the internal visual is contained on.
        /// </summary>
        private VisualBrush InternalVisualBrush
        {
            get 
            { 
                return _visualBrush; 
            }

            set
            {
                _visualBrush = value;
            }
        }

        internal static void OnVisualChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            InteractiveVisual3D imv3D = ((InteractiveVisual3D)sender);
            AdornerDecorator ad = null;
            if (imv3D.InternalVisual != null)
            {
                ad = ((AdornerDecorator)imv3D.InternalVisual);
                if (ad.Child is VisualDecorator)
                {
                    VisualDecorator oldVisualDecorator = (VisualDecorator)ad.Child;
                    oldVisualDecorator.Content = null;
                }
            }
            // so that the patterns on visuals caused by tabbing, etc... work, 
            // we put an adorner layer here so that anything adorned gets adorned 
            // within the visual and not at the adorner layer on the window
            if (ad == null)
            {
                ad = new AdornerDecorator();
            }
            UIElement adornerDecoratorChild;
            if (imv3D.Visual is UIElement)
            {
                adornerDecoratorChild = (UIElement)imv3D.Visual;
            }
            else
            {
                VisualDecorator visDecorator = new VisualDecorator();
                visDecorator.Content = imv3D.Visual;
                adornerDecoratorChild = visDecorator;
            }
            ad.Child = null;
            ad.Child = adornerDecoratorChild;
            imv3D._internalVisual = ad;
            imv3D.InternalVisualBrush.Visual = imv3D.InternalVisual;
        }


        /// <summary>
        /// The BackFaceVisibleProperty specifies whether or not the back face of the 3D object
        /// should be considered visible.  If it is then when generating the material, the back material
        /// is also set.
        /// </summary>
        private static readonly DependencyProperty IsBackVisibleProperty =
            DependencyProperty.Register(
                "IsBackVisible",
                typeof(bool),
                typeof(InteractiveVisual3D),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsBackVisiblePropertyChanged)));

        public bool IsBackVisible
        {
            get { return (bool)GetValue(IsBackVisibleProperty); }
            set
            {
                SetValue(IsBackVisibleProperty, value);
            }
        }

        internal static void OnIsBackVisiblePropertyChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            InteractiveVisual3D imv3D = ((InteractiveVisual3D)sender);

            if (imv3D.IsBackVisible)
            {
                imv3D._content.BackMaterial = imv3D._content.Material;
            }
            else
            {
                imv3D._content.BackMaterial = null;
            }
        }



        /// <summary>
        /// The emissive color of the material
        /// </summary>
        private readonly static DiffuseMaterial _defaultMaterialPropertyValue;
        public static readonly DependencyProperty MaterialProperty;

        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }

        internal static void OnMaterialPropertyChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            InteractiveVisual3D imv3D = ((InteractiveVisual3D)sender);

            imv3D.GenerateMaterial();            
        }
       
        /// <summary>
        /// The 3D geometry that the InteractiveModelVisual3D represents
        /// </summary>
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(
                "Geometry",
                typeof(Geometry3D),
                typeof(InteractiveVisual3D),
                new PropertyMetadata(null, new PropertyChangedCallback(OnGeometryChanged)));

        public Geometry3D Geometry
        {
            get { return (Geometry3D)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        internal static void OnGeometryChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            InteractiveVisual3D imv3D = ((InteractiveVisual3D)sender);

            imv3D._content.Geometry = imv3D.Geometry;
        }

        /// <summary>
        /// The attached dependency property used to indicate whether a material should be made
        /// interactive.
        /// </summary>
        public static readonly DependencyProperty IsInteractiveMaterialProperty =
            DependencyProperty.RegisterAttached(
                "IsInteractiveMaterial",
                typeof(Boolean),
                typeof(InteractiveVisual3D),
                new PropertyMetadata(false));

        public static void SetIsInteractiveMaterial(UIElement element, Boolean value)
        {
            element.SetValue(IsInteractiveMaterialProperty, value);
        }
        public static Boolean GetIsInteractiveMaterial(UIElement element)
        {
            return (Boolean)element.GetValue(IsInteractiveMaterialProperty);
        }

        /// <summary>
        /// Done so that the Content property is not serialized and not visible by a visual designer
        /// </summary>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        public new Model3D Content
        {
            get { return base.Content; }
            set { base.Content = value; }
        }

        //------------------------------------------------------------------------
        //
        // PRIVATE DATA
        //
        //------------------------------------------------------------------------

        private enum PolygonSide { FRONT, BACK };

        // the geometry model that represents this visual3D
        internal readonly GeometryModel3D _content;

        // helper functions to cache the last created visual edges and also to tell
        // if we need to recompute these values, or can use the cache
        private Point[] _lastVisCorners = null;
        private List<HitTestEdge> _lastEdges = null;
        private Matrix3D _lastMatrix3D;

        // the actual visual that is created
        private UIElement _internalVisual;

        private VisualBrush _visualBrush;
    }

    /// <summary>
    /// The VisualDecorator class simply holds one Visual as a child.  It is used
    /// to provide a bridge between the AdornerDecorator and the Visual that 
    /// is intended to be placed on the 3D mesh.  The reason being that AdornerDecorator
    /// only takes a UIElement as a child - so in the case that a Visual (non UI/FE) 
    /// is to be placed on the 3D mesh, a VisualDecorator is needed to provide that
    /// bridge.
    /// </summary>
    internal class VisualDecorator : FrameworkElement
    {
        public VisualDecorator()
        {
            _visual = null;
        }

        /// <summary>
        /// The content/child of the VisualDecorator.
        /// </summary>
        public Visual Content
        {
            get
            {
                return _visual;
            }

            set
            {
                // check to make sure we're attempting to set something new
                if (_visual != value)
                {
                    Visual oldVisual = _visual;
                    Visual newVisual = value;

                    // remove the previous child
                    RemoveVisualChild(oldVisual);
                    RemoveLogicalChild(oldVisual);

                    // set the private variable
                    _visual = value;

                    // link in the new child
                    AddLogicalChild(newVisual);
                    AddVisualChild(newVisual);                    
                }
            }
        }
       
        /// <summary>
        /// Returns the number of Visual children this element has.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get 
            {
                return (Content != null ? 1 : 0);
            }
        }

        /// <summary>
        /// Returns the child at the specified index.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && Content != null) return _visual;

            // if we didn't return then the index is out of range - throw an error
            throw new ArgumentOutOfRangeException("index", index, "Out of range visual requested");
        }

        /// <summary> 
        /// Returns an enumertor to this element's logical children
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                Visual[] logicalChildren = new Visual[VisualChildrenCount];
                for (int i = 0; i < VisualChildrenCount; i++)
                {
                    logicalChildren[i] = GetVisualChild(i);
                }               

                return logicalChildren.GetEnumerator();
            }
        }

        // the visual being referenced
        private Visual _visual;
    }
}
