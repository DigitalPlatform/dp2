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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup; // IAddChild, ContentPropertyAttribute

namespace _3DTools
{
    /// <summary>
    /// Class causes a Viewport3D to become interactive.  To cause the interactivity,
    /// a hidden visual, corresponding to the Visual being interacted with, is placed
    /// on the PostViewportChildren layer, and is then interacted with, giving the illusion
    /// of interacting with the 3D object.
    /// </summary>
    public class Interactive3DDecorator : Viewport3DDecorator
    {
        /// <summary>
        /// Constructs the InteractiveViewport3D
        /// </summary>
        public Interactive3DDecorator() : base()
        {
            // keep everything within our bounds so that the hidden visuals are only
            // accessable over the Viewport3D
            ClipToBounds = true;
            
            // the offset of the hidden visual and the transform associated with it
            _offsetX = _offsetY = 0.0;
            _scale = 1;

            // set up the hidden visual transforms
            _hiddenVisTranslate = new TranslateTransform(_offsetX, _offsetY);
            _hiddenVisScale = new ScaleTransform(_scale, _scale);
            _hiddenVisTransform = new TransformGroup();
            _hiddenVisTransform.Children.Add(_hiddenVisScale);
            _hiddenVisTransform.Children.Add(_hiddenVisTranslate);            

            // the layer that contains our moving visual
            _hiddenVisual = new Decorator();            
            _hiddenVisual.Opacity = 0.0;
            _hiddenVisual.RenderTransform = _hiddenVisTransform;                       

            // where we store the previous hidden visual so that it can be in the tree
            // after it is removed so any state (i.e. mouse over) can be updated.
            _oldHiddenVisual = new Decorator();
            _oldHiddenVisual.Opacity = 0.0;
            
            // the keyboard focus visual
            _oldKeyboardFocusVisual = new Decorator();
            _oldKeyboardFocusVisual.Opacity = 0.0;

            // add all of the hidden visuals
            PostViewportChildren.Add(_oldHiddenVisual);
            PostViewportChildren.Add(_oldKeyboardFocusVisual);
            PostViewportChildren.Add(_hiddenVisual);
            
            // initialize other member variables to null
            _closestIntersectInfo = null;
            _lastValidClosestIntersectInfo = null;     

            AllowDrop = true;
        }

        /// <summary>
        /// We want our visuals to size themselves to their desired size, so we impose no
        /// constraint on them when measuring.
        /// </summary>
        /// <param name="Constraint"></param>
        protected override void MeasurePostViewportChildren(Size constraint)
        {
            Size noConstraintSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            // measure the post viewport visuals
            foreach (UIElement uiElem in PostViewportChildren)
            {
                uiElem.Measure(noConstraintSize);          
            }
        }

        /// <summary>
        /// The hidden visuals are all set at their desired size.  The passed in 
        /// arrangeSize is ignored.
        /// </summary>
        /// <param name="arrangeSize"></param>
        protected override void ArrangePostViewportChildren(Size arrangeSize)
        {
            // measure the post viewport visual visuals
            foreach (UIElement uiElem in PostViewportChildren)
            {
                uiElem.Arrange(new Rect(uiElem.DesiredSize));          
            }
        }

        /// <summary>
        /// When a drag is in progress we do the same hit testing as in a 
        /// regular mouse move, except we need to scale up the hidden visual
        /// to "correct" for how mouse positions are calculated during a drag
        /// and drop operation.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewDragOver(DragEventArgs e) 
        {
            base.OnPreviewDragOver(e);

            if (Viewport3D != null)
            {
                Point mousePosition = e.GetPosition(Viewport3D);
                ArrangeHiddenVisual(mousePosition, true); 
            }     
        } 

        /// <summary>
        /// Although the InteractiveViewport3D sets the AllowDrop flag to true
        /// so that it can intercept preview drag moves, it doesn't actually
        /// do anything with drag+drop, so if a DragOver event ever reaches us
        /// set the effects to be none.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Although the InteractiveViewport3D sets the AllowDrop flag to true
        /// so that it can intercept preview drag moves, it doesn't actually
        /// do anything with drag+drop, so if a OnDragEnter event ever reaches us
        /// set the effects to be none.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }        

        /// <summary>
        /// On a mouse move, we hit test the Viewport3D, and arrange the hidden visuals
        /// to be in the correct locations.  This function is the core event that needs
        /// to be handled so that the InteractiveViewport3D works.
        /// </summary>
        /// <param name="e">The mouse event arguments</param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            // quick speedup to avoid hit testing again right after we moved in to the 
            // correct position
            if (_isInPosition)
            {
                _isInPosition = false;
            }
            else
            {
                if (Viewport3D != null)
                {
                    bool needsMouseResync = ArrangeHiddenVisual(e.GetPosition(Viewport3D), false);

                    if (needsMouseResync)
                    {
                        e.Handled = true;

                        _isInPosition = true;

                        // we need to make this InvalidateArrange call so that inking works.
                        // This is potentially a time consuming call, so by default it does not
                        // happen unless ContainsInk is set to be true.
                        if (ContainsInk)
                        {
                            InvalidateArrange();
                        }

                        // resynch the mouse since we just moved things around
                        Mouse.Synchronize();
                    }
                }
            }
        }

        /// <summary>
        /// Arranges the hidden visuals so that interactivity is achieved.
        /// </summary>
        /// <param name="mouseposition">The location of the mouse</param>
        /// <returns>Whether a mouse resynch is necessary</returns>
        private bool ArrangeHiddenVisual(Point mouseposition, bool scaleHiddenVisual)
        {
            bool needMouseResync = false;

            // get the underlying viewport3D we're enhancing
            Viewport3D viewport3D = Viewport3D;

            // if the viewport3D exists - perform a hit test operation on the underlying visuals
            if (viewport3D != null)
            {
                // set up the hit test parameters
                PointHitTestParameters pointparams = new PointHitTestParameters(mouseposition);
                _closestIntersectInfo = null;
                _mouseCaptureInHiddenVisual = _hiddenVisual.IsMouseCaptureWithin;

                // first hit test - this one attempts to hit a visible mesh if possible
                VisualTreeHelper.HitTest(viewport3D, InteractiveMV3DFilter, HTResult, pointparams);

                // perform capture positioning if we didn't hit anything and something has capture
                if (_closestIntersectInfo == null && 
                    _mouseCaptureInHiddenVisual &&
                    _lastValidClosestIntersectInfo != null)
                {
                    HandleMouseCaptureButOffMesh(_lastValidClosestIntersectInfo.InteractiveModelVisual3DHit,
                                                 mouseposition);                    
                } 
                else if (_closestIntersectInfo != null)
                {
                    // save it for if we walk off the mesh and something has capture                  
                    _lastValidClosestIntersectInfo = _closestIntersectInfo;
                }
                
                // update the location if we have positioning information
                needMouseResync = UpdateHiddenVisual(_closestIntersectInfo,
                                                     mouseposition,
                                                     scaleHiddenVisual);                                                         
            }

            return needMouseResync;
        }
        
        /// <summary>
        /// Function to deal with mouse capture when off the mesh.
        /// </summary>
        /// <param name="imv3DHit">The model hit</param>
        /// <param name="mousePos">The location of the mouse</param>
        private void HandleMouseCaptureButOffMesh(InteractiveVisual3D imv3DHit, Point mousePos)
        {
            // process the mouse capture if it exists
            UIElement uie = (UIElement)Mouse.Captured;

            // get the size of the element
            Rect contBounds = VisualTreeHelper.GetDescendantBounds(uie);
            
            // translate to the parent's coordinate system
            GeneralTransform gt = uie.TransformToAncestor(_hiddenVisual);

            Point[] visCorners = new Point[4];

            // get the points relative to the parent
            visCorners[0] = gt.Transform(new Point(contBounds.Left, contBounds.Top));
            visCorners[1] = gt.Transform(new Point(contBounds.Right, contBounds.Top));         
            visCorners[2] = gt.Transform(new Point(contBounds.Right, contBounds.Bottom));
            visCorners[3] = gt.Transform(new Point(contBounds.Left, contBounds.Bottom));

            // get the u,v texture coordinate values of the above points
            Point[] texCoordsOfInterest = new Point[4];
            for (int i = 0; i < visCorners.Length; i++)
            {
                texCoordsOfInterest[i] = VisualCoordsToTextureCoords(visCorners[i], _hiddenVisual);
            }

            // get the edges that map to the given visual
            List<HitTestEdge> edges = imv3DHit.GetVisualEdges(texCoordsOfInterest);

            if (Debug)
            {
                AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (_DEBUGadorner == null) 
                {
                    _DEBUGadorner = new DebugEdgesAdorner(this, edges);
                    myAdornerLayer.Add(_DEBUGadorner);
                }
                else
                {
                    myAdornerLayer.Remove(_DEBUGadorner);
                    _DEBUGadorner = new DebugEdgesAdorner(this, edges);
                    myAdornerLayer.Add(_DEBUGadorner);
                }
            }

            // find the closest intersection of the mouse position and the edge list
            FindClosestIntersection(mousePos, edges, imv3DHit);
        }

        /// <summary>
        /// Finds the point in edges that is closest ot the mouse position.  Updates closestIntersectionInfo
        /// with the results of this calculation
        /// </summary>
        /// <param name="mousePos">The mouse position</param>
        /// <param name="edges">The edges to test against</param>
        /// <param name="imv3DHit">The model that has the visual on it with capture</param>
        private void FindClosestIntersection(Point mousePos, List<HitTestEdge> edges, InteractiveVisual3D imv3DHit)
        {
            double closestDistance = Double.MaxValue;
            Point closestIntersection = new Point();  // the uv of the closest intersection

            // Find the closest point to the mouse position            
            for (int i=0; i<edges.Count; i++)
            {
                Vector v1 = mousePos - edges[i]._p1Transformed;
                Vector v2 = edges[i]._p2Transformed - edges[i]._p1Transformed;
                
                Point currClosest;
                double distance;

                // calculate the distance from the mouse position to this edge
                // The closest distance can be computed by projecting v1 on to v2.  If the
                // projectiong occurs between _p1Transformed and _p2Transformed, then this is the
                // closest point.  Otherwise, depending on which side it lies, it is either _p1Transformed
                // or _p2Transformed.  
                //
                // The projection equation is given as: (v1 DOT v2) / (v2 DOT v2) * v2.
                // v2 DOT v2 will always be positive.  Thus, if v1 DOT v2 is negative, we know the projection
                // will occur before _p1Transformed (and so it is the closest point).  If (v1 DOT v2) is greater
                // than (v2 DOT v2), then we have gone passed _p2Transformed and so it is the closest point.
                // Otherwise the projection gives us this value.
                //
                double denom = v2 * v2;
                if (denom == 0)
                {
                    currClosest = edges[i]._p1Transformed;
                    distance = v1.Length;
                }
                else
                {
                    double numer = v2 * v1;
                    if (numer < 0)
                    {
                        currClosest = edges[i]._p1Transformed;
                    }
                    else
                    {
                        if (numer > denom)
                        {
                            currClosest = edges[i]._p2Transformed;
                        }
                        else
                        {
                            currClosest = edges[i]._p1Transformed + (numer / denom) * v2;
                        }
                    }

                    distance = (mousePos - currClosest).Length; 
                }

                // see if we found a new closest distance
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    
                    if (denom != 0)
                    {
                        closestIntersection = ((currClosest - edges[i]._p1Transformed).Length / Math.Sqrt(denom) * 
                                               (edges[i]._uv2 - edges[i]._uv1)) + edges[i]._uv1;
                    }
                    else
                    {
                        closestIntersection = edges[i]._uv1;
                    }
                }                
            }

            if (closestDistance != Double.MaxValue)
            {
                UIElement uiElemWCapture = (UIElement)Mouse.Captured;
                UIElement uiElemOnMesh = imv3DHit.InternalVisual;
                
                Rect contBounds = VisualTreeHelper.GetDescendantBounds(uiElemWCapture);                
                Point ptOnVisual = TextureCoordsToVisualCoords(closestIntersection, uiElemOnMesh);
                Point ptRelToCapture = uiElemOnMesh.TransformToDescendant(uiElemWCapture).Transform(ptOnVisual);

                // we want to "ring" around the outside so things like buttons are not pressed
                // this code here does that - the +BUFFER_SIZE and -BUFFER_SIZE are to give a bit of a 
                // buffer for any numerical issues
                if (ptRelToCapture.X <= contBounds.Left + 1) ptRelToCapture.X -= BUFFER_SIZE;
                if (ptRelToCapture.Y <= contBounds.Top + 1) ptRelToCapture.Y -= BUFFER_SIZE;
                if (ptRelToCapture.X >= contBounds.Right - 1) ptRelToCapture.X += BUFFER_SIZE;
                if (ptRelToCapture.Y >= contBounds.Bottom - 1) ptRelToCapture.Y += BUFFER_SIZE;

                Point finalVisualPoint = uiElemWCapture.TransformToAncestor(uiElemOnMesh).Transform(ptRelToCapture);                
                
                _closestIntersectInfo = new ClosestIntersectionInfo(VisualCoordsToTextureCoords(finalVisualPoint, uiElemOnMesh), 
                                                                    imv3DHit.InternalVisual, 
                                                                    imv3DHit);                 
            }
        }
       
        /// <summary>
        /// Filter for hit testing.  In the case that the hidden visual has capture
        /// then all Visual3Ds are skipped except for the one it is on.  This gives the 
        /// same behavior as capture in the 2D case.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public HitTestFilterBehavior InteractiveMV3DFilter(DependencyObject o)
        {
            // by default everything is ok
            HitTestFilterBehavior behavior = HitTestFilterBehavior.Continue;
            
            // if the hidden visual has mouse capture - then we only want to test against
            // the IMV3D that has capture
            if (o is Visual3D && _mouseCaptureInHiddenVisual)
            {
                if (o is InteractiveVisual3D)
                {
                    InteractiveVisual3D imv3D = (InteractiveVisual3D)o;
                
                    if (imv3D.InternalVisual != _hiddenVisual.Child)
                    {
                        behavior = HitTestFilterBehavior.ContinueSkipSelf;
                    }                                                                
                }
                else
                {
                    behavior = HitTestFilterBehavior.ContinueSkipSelf;
                }
            }

            return behavior;
        }

        /// <summary>
        /// This function sets the passed in uiElem as the hidden visual, and aligns
        /// it so that the point the uv coordinates map to on the visual are located
        /// at the same location as mousePos.
        /// </summary>
        /// <param name="uiElem">The UIElement that should be the hidden visual</param>
        /// <param name="uv">The uv coordinates on that UIElement that should be aligned with mousePos</param>
        /// <param name="mousePos">The mouse location</param>
        /// <param name="scaleHiddenVisual">Whether to scale the visual in addition to moving it</param>
        /// <returns></returns>
        private bool UpdateHiddenVisual(ClosestIntersectionInfo isectInfo, Point mousePos, bool scaleHiddenVisual)
        {
            bool needsMouseReSync = false;
            double newOffsetX, newOffsetY;

            // compute positioning information
            if (isectInfo != null)
            {
                UIElement uiElem = isectInfo.UIElementHit;

                // set our UIElement to be the one passed in
                if (_hiddenVisual.Child != uiElem)
                {
                    // we need to replace the old one with this new one
                    UIElement prevVisual = _hiddenVisual.Child;

                    // clear out uiElem from any of our hidden visuals
                    if (_oldHiddenVisual.Child == uiElem) _oldHiddenVisual.Child = null;
                    if (_oldKeyboardFocusVisual.Child == uiElem) _oldKeyboardFocusVisual.Child = null;

                    // also clear out prevVisual
                    if (_oldHiddenVisual.Child == prevVisual) _oldHiddenVisual.Child = null;
                    if (_oldKeyboardFocusVisual.Child == prevVisual) _oldKeyboardFocusVisual.Child = null;

                    // depending on whether or not it has focus, do two different things, either
                    // use the _oldKeyboardFocusVisual or the _oldHiddenVisual
                    Decorator _oldVisToUse = null;
                    if (prevVisual != null && prevVisual.IsKeyboardFocusWithin)
                    {
                        _oldVisToUse = _oldKeyboardFocusVisual;
                    }
                    else
                    {
                        _oldVisToUse = _oldHiddenVisual;
                    }

                    // now safely link everything up
                    _hiddenVisual.Child = uiElem;
                    _oldVisToUse.Child = prevVisual;

                    needsMouseReSync = true;
                }

                Point ptOnVisual = TextureCoordsToVisualCoords(isectInfo.PointHit, _hiddenVisual);
                newOffsetX = mousePos.X - ptOnVisual.X;
                newOffsetY = mousePos.Y - ptOnVisual.Y;
            }
            else
            {
                // because we didn't interesect with anything, we need to move the hidden visual off
                // screen so that it can no longer be interacted with
                newOffsetX = ActualWidth + 1;
                newOffsetY = ActualHeight + 1;                
            }

            // compute the scale needed
            double newScale;
            if (scaleHiddenVisual)
            {
                newScale = Math.Max(Viewport3D.RenderSize.Width,
                                    Viewport3D.RenderSize.Height);
            }
            else
            {
                newScale = 1.0;
            }
            
            // do the actual positioning
            needsMouseReSync |= PositionHiddenVisual(newOffsetX, newOffsetY, newScale, mousePos); 
                      
            return needsMouseReSync;
        }

        /// <summary>
        /// Positions the hidden visual based upon the offset and scale specified.
        /// </summary>
        /// <param name="newOffsetX">The new hidden visual x offset</param>
        /// <param name="newOffsetY">The new hidden visual y offset</param>
        /// <param name="newScale">The new scale to perform on the visual</param>
        /// <param name="mousePosition">The position of the mouse</param>
        /// <returns>Whether the new offset/scale was different than the previous</returns>
        private bool PositionHiddenVisual(double newOffsetX, double newOffsetY, double newScale, Point mousePosition)
        {
            bool positionChanged = false;

            if (newOffsetX != _offsetX || newOffsetY != _offsetY || _scale != newScale)
            {
                _offsetX = newOffsetX;
                _offsetY = newOffsetY;
                _scale = newScale;

                // change where we're putting the object                    
                _hiddenVisTranslate.X = _scale * (_offsetX - mousePosition.X) + mousePosition.X;
                _hiddenVisTranslate.Y = _scale * (_offsetY - mousePosition.Y) + mousePosition.Y;
                _hiddenVisScale.ScaleX = _scale;
                _hiddenVisScale.ScaleY = _scale;

                positionChanged = true;
            }

            return positionChanged;
        }

        /// <summary>
        /// Converts a point given in texture coordinates to the corresponding
        /// 2D point on the UIElement passed in.
        /// </summary>
        /// <param name="uv">The texture coordinate to convert</param>
        /// <param name="uiElem">The UIElement whose coordinate system is to be used</param>
        /// <returns>
        /// The 2D point on the passed in UIElement cooresponding to the
        /// passed in texture coordinate. 
        /// </returns>
        static Point TextureCoordsToVisualCoords(Point uv, UIElement uiElem)
        {
            Rect descBounds = VisualTreeHelper.GetDescendantBounds(uiElem);

            return new Point(uv.X * descBounds.Width + descBounds.Left,
                             uv.Y * descBounds.Height + descBounds.Top);                             
        }

        /// <summary>
        /// Converts a point on the passed in UIElement to the corresponding
        /// texture coordinate for that point.  The function assumes (0, 0)
        /// is the upper-left texture coordinate and (1,1) is the lower-right.
        /// </summary>
        /// <param name="pt">The 2D point on the passed in UIElement to convert</param>
        /// <param name="uiElem">The UIElement whose coordinate system is being used</param>
        /// <returns>
        /// The texture coordinate corresponding to the 2D point on the passed in UIElement
        /// </returns>
        static Point VisualCoordsToTextureCoords(Point pt, UIElement uiElem)
        {
            Rect descBounds = VisualTreeHelper.GetDescendantBounds(uiElem);
            
            return new Point((pt.X - descBounds.Left) / (descBounds.Right - descBounds.Left),
                             (pt.Y - descBounds.Top) / (descBounds.Bottom - descBounds.Top));
        }        


        /// <summary>
        /// we want to keep the _oldKeyboardFocusVisual and _oldHiddenVisual off screen at all 
        /// times so that they can't be interacted with.  This method is overridden to know
        /// when the size of the Viewport3D changes so that we can always keep the above two
        /// hidden visuals off screen
        /// </summary>
        /// <param name="info"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);

            Size newSize = info.NewSize;
            TranslateTransform tt = new TranslateTransform(newSize.Width + 1, 0);
            _oldKeyboardFocusVisual.RenderTransform = tt;
            _oldHiddenVisual.RenderTransform = tt;
        }
        
        /// <summary>
        /// The HTResult function simply takes the intersection closest to the origin and
        /// and stores the intersection info for that closest intersection point.
        /// </summary>
        /// <param name="rawresult"></param>
        /// <returns></returns>
        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;
            HitTestResultBehavior hitTestResultBehavior = HitTestResultBehavior.Continue;

            // since we're hit testing a viewport3D we should be getting the ray hit test result back
            if (rayResult != null)
            {
                _closestIntersectInfo = GetIntersectionInfo(rayResult);
                hitTestResultBehavior = HitTestResultBehavior.Stop;
            }

            return hitTestResultBehavior;
        }

        /// <summary>
        /// Returns the intersection info for the given rayHitResult.  Intersection info
        /// only exists for an InteractiveModelVisual3D, so if an InteractiveModelVisual3D
        /// is not hit, then the return value is null.
        /// </summary>
        /// <param name="rayHitResult"></param>
        /// <returns>
        /// Returns ClosestIntersectionInfo if an InteractiveModelVisual3D is hit, otherwise
        /// returns null.
        /// </returns>
        private ClosestIntersectionInfo GetIntersectionInfo(RayHitTestResult rayHitResult)
        {
            ClosestIntersectionInfo isectInfo = null;

            // try to cast to a RaymeshGeometry3DHitTestResult
            RayMeshGeometry3DHitTestResult rayMeshResult = rayHitResult as RayMeshGeometry3DHitTestResult;
            if (rayMeshResult != null)
            {
                // see if we hit an InteractiveVisual3D
                InteractiveVisual3D imv3D = rayMeshResult.VisualHit as InteractiveVisual3D;
                if (imv3D != null)
                {
                    // we can now extract the mesh and visual for the object we hit
                    MeshGeometry3D geom = rayMeshResult.MeshHit;
                    UIElement uiElem = imv3D.InternalVisual;
                
                    if (uiElem != null)
                    {
                        // pull the barycentric coordinates of the intersection point
                        double vertexWeight1 = rayMeshResult.VertexWeight1;
                        double vertexWeight2 = rayMeshResult.VertexWeight2;
                        double vertexWeight3 = rayMeshResult.VertexWeight3;

                        // the indices in to where the actual intersection occurred
                        int index1 = rayMeshResult.VertexIndex1;
                        int index2 = rayMeshResult.VertexIndex2;
                        int index3 = rayMeshResult.VertexIndex3;

                        // texture coordinates of the three vertices hit
                        // in the case that no texture coordinates are supplied we will simply
                        // treat it as if no intersection occurred
                        if (geom.TextureCoordinates != null &&
                            index1 < geom.TextureCoordinates.Count &&
                            index2 < geom.TextureCoordinates.Count &&
                            index3 < geom.TextureCoordinates.Count)
                        {
                            Point texCoord1 = geom.TextureCoordinates[index1];
                            Point texCoord2 = geom.TextureCoordinates[index2];
                            Point texCoord3 = geom.TextureCoordinates[index3];

                            // get the final uv values based on the barycentric coordinates
                            Point finalPoint = new Point(texCoord1.X * vertexWeight1 +
                                                         texCoord2.X * vertexWeight2 +
                                                         texCoord3.X * vertexWeight3,
                                                         texCoord1.Y * vertexWeight1 +
                                                         texCoord2.Y * vertexWeight2 +
                                                         texCoord3.Y * vertexWeight3);

                            // create and return a valid intersection info
                            isectInfo = new ClosestIntersectionInfo(finalPoint,
                                                                    uiElem,
                                                                    imv3D);
                        }
                    }
                }
            }

            return isectInfo;
        }

        /// <summary>
        /// The following DP allows for the debugging of InteractiveViewport3D by making the
        /// hidden visual no longer transparent, and also draws all of the edges created during 
        /// capture.
        /// </summary>
        public static readonly DependencyProperty DebugProperty =
            DependencyProperty.Register(
                "Debug",
                typeof(bool),
                typeof(Interactive3DDecorator),
                new PropertyMetadata(false, new PropertyChangedCallback(OnDebugPropertyChanged)));

        public bool Debug
        {
            get { return (bool)GetValue(DebugProperty); }
            set { SetValue(DebugProperty, value); }
        }

        internal static void OnDebugPropertyChanged(Object sender, DependencyPropertyChangedEventArgs e)
        {
            Interactive3DDecorator iv3D = ((Interactive3DDecorator)sender);

            if ((bool)e.NewValue == true)
            {
                iv3D._hiddenVisual.Opacity = 0.2;
            }
            else
            {
                iv3D._hiddenVisual.Opacity = 0.0;

                if (iv3D._DEBUGadorner != null)
                {
                    AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(iv3D);                
                    myAdornerLayer.Remove(iv3D._DEBUGadorner);
                    iv3D._DEBUGadorner = null;
                }
            }
        }

        /// <summary>
        /// The following DP indicates whether any of the 3D objects within the
        /// Viewport3D will have 2D visuals with ink on them - special processing
        /// is required in this case.
        /// </summary>
        public static readonly DependencyProperty ContainsInkProperty =
            DependencyProperty.Register(
                "ContainsInk",
                typeof(bool),
                typeof(Interactive3DDecorator),
                new PropertyMetadata(false));

        public bool ContainsInk
        {
            get { return (bool)GetValue(ContainsInkProperty); }
            set { SetValue(ContainsInkProperty, value); }
        }

        /// <summary>
        /// The DebugEdgesAdorner enables the edges returned when the mouse is captured
        /// to be visualized on screen in order to debug where they are, and verify
        /// it is working correctly.
        /// </summary>
        public class DebugEdgesAdorner : Adorner
        {
            /// <summary>
            /// Constructs the DebugEdgesAdorner class
            /// </summary>
            /// <param name="adornedElement">The element being adorned</param>
            /// <param name="edges">The edges that are to be displayed</param>
            public DebugEdgesAdorner(UIElement adornedElement, List<HitTestEdge> edges)
                : base(adornedElement)
            {
                _edges = edges;
            }

            /// <summary>
            /// Draws all of the edges.
            /// </summary>
            /// <param name="drawingContext"></param>
            protected override void OnRender(DrawingContext drawingContext)
            {
                Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);

                for (int i = 0; i < _edges.Count; i++)
                {
                    drawingContext.DrawLine(renderPen, _edges[i]._p1Transformed, _edges[i]._p2Transformed);
                }

            }

            private List<HitTestEdge> _edges;
        }
        
        //------------------------------------------------------
        //
        //  Private data
        //
        //------------------------------------------------------ 
               
        /// <summary>
        /// The ClosestIntersectionInfo class is a wrapper class that contains all the 
        /// information necessary to process an intersection with an InteractiveModelVisual3D
        /// </summary>
        private class ClosestIntersectionInfo
        {
            public ClosestIntersectionInfo(Point p, UIElement v, InteractiveVisual3D iv3D)
            {
                _pointHit = p;
                _uiElemHit = v;
                _imv3DHit = iv3D;
            }

            // the point on the visual that we hit
            private Point _pointHit;
            public Point PointHit
            {
                get
                {
                    return _pointHit;
                }
                set
                {
                    _pointHit = value;
                }
            }

            // the visual hit by the intersection
            private UIElement _uiElemHit;
            public UIElement UIElementHit
            {
                get
                {
                    return _uiElemHit;
                }
                set
                {
                    _uiElemHit = value;
                }
            }

            // the InteractiveModelVisual3D hit by the intersection
            private InteractiveVisual3D _imv3DHit;
            public InteractiveVisual3D InteractiveModelVisual3DHit
            {
                get
                {
                    return _imv3DHit;
                }
                set
                {
                    _imv3DHit = value;
                }
            }
        }

        private Decorator _hiddenVisual;            // the hidden visual that is interacted with
        private Decorator _oldHiddenVisual;         // the previous visual - used so that things like losing
                                                    // focus can occur if two visuals are being interacted with
        private Decorator _oldKeyboardFocusVisual;  // if the old visual has keyboard focus, we place it in here
                                                    // rather than oldHiddenVisual so that it can retain focus
                                                    // as long as is necessary        

        private TranslateTransform _hiddenVisTranslate;         // hidden visual's transform    
        private ScaleTransform _hiddenVisScale;                 // hidden visual's scale
        private TransformGroup _hiddenVisTransform;             // the combined transform

        private bool _mouseCaptureInHiddenVisual;

        private double _offsetX;                 // the offset needed to move the visual so
        private double _offsetY;                 // that hit testing will work on the hidden visual    
        private double _scale;

        private ClosestIntersectionInfo _closestIntersectInfo;
        private ClosestIntersectionInfo _lastValidClosestIntersectInfo = null;

        private DebugEdgesAdorner _DEBUGadorner = null;

        bool _isInPosition = false;             // optimization so that things aren't rechecked after they are moved 

        private const double BUFFER_SIZE = 2.0;  // the "ring" around the element with capture to use in the capture case        
    }
}
