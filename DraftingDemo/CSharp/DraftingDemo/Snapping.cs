using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Graphics;
using objectSnapType = WpfApplication1.MainWindow.objectSnapType;

namespace WpfApplication1
{
    /// <summary>
    /// Contains utilities required for grid snapping or model vertex snapping.
    /// </summary>
    partial class MyModel
    {
        // Current snapped point, which is one of the vertex from model
        private Point3D snapPoint = null;

        // Flags to indicate current snapping mode
        public bool objectSnapEnabled { get; set; }

        public bool gridSnapEnabled { get; set; }

        public bool waitingForSelection { get; set; }

        public objectSnapType activeObjectSnap = objectSnapType.End;
             
        /// <summary>
        /// Finds closest snap point.
        /// </summary>
        /// <param name="snapPoints">Array of snap points</param>
        /// <returns>Closest snap point.</returns>
        private SnapPoint FindClosestPoint(SnapPoint[] snapPoints)
        {

            double minDist = double.MaxValue;

            int i = 0;
            int index = 0;

            foreach (SnapPoint vertex in snapPoints)
            {
                Point3D vertexScreen = WorldToScreen(vertex);
                Point2D currentScreen = new Point2D(mouseLocation.X, Size.Height - mouseLocation.Y);

                double dist = Point2D.Distance(vertexScreen, currentScreen);

                if (dist < minDist)
                {
                    index = i;
                    minDist = dist;
                }

                i++;
            }

            SnapPoint snap = (SnapPoint)snapPoints.GetValue(index);
            DisplaySnappedVertex(snap);
            
            return snap;
        }

        
        /// <summary>
        /// Displays symbols associated with the snapped vertex type
        /// </summary>
        private void DisplaySnappedVertex(SnapPoint snap)
        {
            renderContext.SetLineSize(2);
            
            // white color
            renderContext.SetColorWireframe(Color.FromArgb(0,0,255));
            renderContext.SetState(depthStencilStateType.DepthTestOff);
            
            Point2D onScreen = WorldToScreen(snap);

            this.snapPoint = snap;

            switch (snap.Type)
            {
                case objectSnapType.Point:
                    DrawCircle(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    DrawCross(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    break;
                case objectSnapType.Center:
                    DrawCircle(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    break;
                case objectSnapType.End:
                    DrawQuad(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    break;
                case objectSnapType.Mid:
                    DrawTriangle(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    break;
                case objectSnapType.Quad:
                    renderContext.SetLineSize(3.0f);
                    DrawRhombus(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
                    break;
            }

            renderContext.SetLineSize(1);            
        }

        /// <summary>
        /// Adds entity to scene on active layer and refresh the screen. 
        /// </summary>
        private void AddAndRefresh(Entity entity, string layerName)
        {
            // increase dimension of points
            if(entity is devDept.Eyeshot.Entities.Point)
            {
                entity.LineWeightMethod = colorMethodType.byEntity;
                entity.LineWeight = 3;
            }

            // avoid dimensions with width bigger than one
            if (entity is Dimension || entity is Leader)
            {
                entity.LayerName = layerName;
                entity.LineWeightMethod = colorMethodType.byEntity;

                Entities.Add(entity);
            }
            else
            {
                Entities.Add(entity, layerName);
            }

            Entities.Regen();
            Invalidate();
        }

        /// <summary>
        /// Tries to snap grid vertex for the current mouse point
        /// </summary>
        private bool SnapToGrid(ref Point3D ptToSnap)
        {
            Point2D gridPoint = new Point2D(Math.Round(ptToSnap.X / 10) * 10, Math.Round(ptToSnap.Y / 10) * 10);

            if (Point2D.Distance(gridPoint, ptToSnap) < magnetRange)
            {
                ptToSnap.X = gridPoint.X;
                ptToSnap.Y = gridPoint.Y;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the nested entity inside the BlockReference under mouse cursor and computes its transformation
        /// </summary>
        private Entity GetNestedEntity(System.Drawing.Point mousePos, IList<Entity> entList, ref Transformation accumulatedParentTransform)
        {
            int[] index;
            Entity ent;
            index = GetCrossingEntities(new Rectangle(mousePos.X-5, mousePos.Y-5, 10, 10), entList, true, true, accumulatedParentTransform);
            if ((index != null && index.Length > 0))
            {
                if (entList[index[0]] is BlockReference)
                {
                    BlockReference br = (BlockReference)entList[index[0]];
                    accumulatedParentTransform = accumulatedParentTransform * br.GetFullTransformation(Blocks);
                    ent = GetNestedEntity(mousePos, Blocks[br.BlockName].Entities, ref accumulatedParentTransform);
                    return ent;
                }
                else
                {
                   
                    return entList[index[0]];
                }
            }
            
            return null;
        }

        /// <summary>
        /// identify snapPoints of the entity under mouse cursor in that moment, using PickBoxSize as tolerance
        /// </summary>
        public SnapPoint[] GetSnapPoints(System.Drawing.Point mouseLocation)
        {
            //changed PickBoxSize to define a range for display snapPoints
            int oldSize = PickBoxSize;
            PickBoxSize = 10;

            //select the entity under mouse cursor
            Transformation accumulatedParentTransform=new Identity();
            Entity ent = GetNestedEntity(mouseLocation,  Entities, ref accumulatedParentTransform);
            PickBoxSize = oldSize;
            SnapPoint[] snapPoints = new SnapPoint[0];

            if (ent!=null)
            {       

                //extract the entity selected with GetEntityUnderMouseCursor
                

                //check which type of entity is it and then,identify snap points
                if (ent is devDept.Eyeshot.Entities.Point)
                {
                    devDept.Eyeshot.Entities.Point point = (devDept.Eyeshot.Entities.Point) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.Point:                    
                            Point3D point3d = point.Vertices[0];
                            snapPoints = new SnapPoint[] { new SnapPoint(point3d, objectSnapType.Point) };
                            break;
                    }
                } 
                else if (ent is Line) //line
                {
                    Line line = (Line) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[]{ new SnapPoint(line.StartPoint, objectSnapType.End), 
                                                new SnapPoint(line.EndPoint, objectSnapType.End) };
                            break;
                        case objectSnapType.Mid:
                            snapPoints = new SnapPoint[] { new SnapPoint(line.MidPoint, objectSnapType.Mid) };
                            break;
                    }               
                }
                else if (ent is LinearPath)//polyline
                {
                    LinearPath polyline = (LinearPath) ent;
                    List<SnapPoint> polyLineSnapPoints = new List<SnapPoint>();

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            foreach (Point3D point in polyline.Vertices)
                                polyLineSnapPoints.Add(new SnapPoint(point, objectSnapType.End));
                            snapPoints = polyLineSnapPoints.ToArray();
                            break;
                    }
                }
                else if (ent is CompositeCurve)//composite
                {
                    CompositeCurve composite = (CompositeCurve)ent;
                    List<SnapPoint> polyLineSnapPoints = new List<SnapPoint>();

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            foreach (ICurve curveSeg in composite.CurveList)
                                polyLineSnapPoints.Add(new SnapPoint(curveSeg.EndPoint, objectSnapType.End));
                            polyLineSnapPoints.Add(new SnapPoint(composite.CurveList[0].StartPoint, objectSnapType.End));
                            snapPoints = polyLineSnapPoints.ToArray();
                            break;
                    }
                }
                else if (ent is Arc) //Arc
                {
                    Arc arc = (Arc) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[] { new SnapPoint(arc.StartPoint, objectSnapType.End),
                                                           new SnapPoint(arc.EndPoint, objectSnapType.End) };
                            break;
                        case objectSnapType.Mid:
                            snapPoints = new SnapPoint[] { new SnapPoint(arc.MidPoint, objectSnapType.Mid) };
                            break;
                        case objectSnapType.Center:
                            snapPoints = new SnapPoint[] { new SnapPoint(arc.Center, objectSnapType.Center) };
                            break;
                    }
                }
                else if (ent is Circle) //Circle
                {
                    Circle circle = (Circle) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[] { new SnapPoint(circle.EndPoint, objectSnapType.End) };
                            break;
                        case objectSnapType.Mid:
                            snapPoints = new SnapPoint[] { new SnapPoint(circle.PointAt(circle.Domain.Mid), objectSnapType.Mid) };
                            break;
                        case objectSnapType.Center:
                            snapPoints = new SnapPoint[] { new SnapPoint(circle.Center, objectSnapType.Center) };
                            break;
                        case objectSnapType.Quad:
                            Point3D quad1 = new Point3D(circle.Center.X, circle.Center.Y + circle.Radius);
                            Point3D quad2 = new Point3D(circle.Center.X + circle.Radius, circle.Center.Y);
                            Point3D quad3 = new Point3D(circle.Center.X, circle.Center.Y - circle.Radius);
                            Point3D quad4 = new Point3D(circle.Center.X - circle.Radius, circle.Center.Y);

                            snapPoints = new SnapPoint[] { new SnapPoint(quad1, objectSnapType.Quad),
                                                           new SnapPoint(quad2, objectSnapType.Quad),
                                                           new SnapPoint(quad3, objectSnapType.Quad),
                                                           new SnapPoint(quad4, objectSnapType.Quad)};
                            break;
                    }
                }
#if NURBS                
                else if (ent is Curve) // Spline
                {
                    Curve curve = (Curve) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[] {new SnapPoint(curve.StartPoint, objectSnapType.End),
                                                          new SnapPoint(curve.EndPoint, objectSnapType.End)};
                            break;
                        case objectSnapType.Mid:
                            snapPoints = new SnapPoint[] { new SnapPoint(curve.PointAt(0.5), objectSnapType.Mid) };
                            break;
                    }
                }
#endif                
                else if (ent is EllipticalArc) //Elliptical Arc
                {
                    EllipticalArc elArc = (EllipticalArc) ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[] {new SnapPoint(elArc.StartPoint, objectSnapType.End),
                                                          new SnapPoint(elArc.EndPoint, objectSnapType.End)};
                            break;
                        case objectSnapType.Center:
                            snapPoints = new SnapPoint[] { new SnapPoint(elArc.Center, objectSnapType.Center) };
                            break;
                    }
                }
                else if (ent is Ellipse) //Ellipse
                {
                    Ellipse ellipse = (Ellipse) ent;
                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:
                            snapPoints = new SnapPoint[] { new SnapPoint(ellipse.EndPoint, objectSnapType.End) };
                            break;
                        case objectSnapType.Mid:
                            snapPoints = new SnapPoint[] { new SnapPoint(ellipse.PointAt(ellipse.Domain.Mid), objectSnapType.Mid) };
                            break;
                        case objectSnapType.Center:
                            snapPoints = new SnapPoint[] { new SnapPoint(ellipse.Center, objectSnapType.Center) };
                            break;
                    }
                }
                else if (ent is Mesh) //Mesh
                {
                    Mesh mesh = (Mesh)ent;

                    switch (activeObjectSnap)
                    {
                        case objectSnapType.End:

                            snapPoints = new SnapPoint[mesh.Vertices.Length];

                            for (int i = 0; i < mesh.Vertices.Length; i++)
                            {
                                Point3D pt = mesh.Vertices[i];
                                snapPoints[i] = new SnapPoint(pt, objectSnapType.End);
                            }
                            break;
                    }
                }
            }

            if (accumulatedParentTransform != new Identity())
            {
                Point3D p_tmp;
                foreach (SnapPoint sp in snapPoints)
                {
                    p_tmp = accumulatedParentTransform * sp;
                    sp.X = p_tmp.X;
                    sp.Y = p_tmp.Y;
                    sp.Z = p_tmp.Z;
                }
            }

            return snapPoints;
        }

        #region SnappingData

        /// <summary>
        /// Represents a 3D point from model vertices and associated snap type.
        /// </summary>
        public class SnapPoint : devDept.Geometry.Point3D
        {
            public objectSnapType Type;

            public SnapPoint()
                : base()
            {
                Type = objectSnapType.None;
            }
            
            public SnapPoint(Point3D point3D, objectSnapType objectSnapType) : base(point3D.X, point3D.Y, point3D.Z)
            {             
                this.Type = objectSnapType;
            }

            public override string ToString()
            {
                return base.ToString() + " | " + Type;
            }
        }
        #endregion

        #region Snapping symbols

        private const int snapSymbolSize = 12;

        /// <summary>
        /// Draw cross. Drawn with a circle identifies a single point
        /// </summary>
        public void DrawCross(System.Drawing.Point onScreen)
        {
            double dim1 = onScreen.X + (snapSymbolSize / 2);
            double dim2 = onScreen.Y + (snapSymbolSize / 2);
            double dim3 = onScreen.X - (snapSymbolSize / 2);
            double dim4 = onScreen.Y - (snapSymbolSize / 2);

            Point3D topLeftVertex   = new Point3D(dim3, dim2);
            Point3D topRightVertex  = new Point3D(dim1, dim2);
            Point3D bottomRightVertex   = new Point3D(dim1, dim4);
            Point3D bottomLeftVertex    = new Point3D(dim3, dim4);

            renderContext.DrawLines(
                new Point3D[]
                {
                    bottomLeftVertex,
                    topRightVertex,

                    topLeftVertex,
                    bottomRightVertex,

                });
        }

        /// <summary>
        /// Draw circle with renderContext to rapresent CENTER snap point
        /// </summary>
        public void DrawCircle(System.Drawing.Point onScreen)
        {
            double radius = snapSymbolSize /2; 

            double x2 = 0, y2 = 0;

            List<Point3D> pts = new List<Point3D>();

            for (int angle = 0; angle < 360; angle += 10)
            {
                double rad_angle = Utility.DegToRad(angle);

                x2 = onScreen.X + radius * Math.Cos(rad_angle);
                y2 = onScreen.Y + radius * Math.Sin(rad_angle);

                Point3D circlePoint = new Point3D(x2, y2);
                pts.Add(circlePoint);
            }

            renderContext.DrawLineLoop(pts.ToArray());
        }

        /// <summary>
        /// Draw quad with renderContext to rapresent END snap point
        /// </summary>
        public void DrawQuad(System.Drawing.Point onScreen)
        {
            double dim1 = onScreen.X + (snapSymbolSize / 2);
            double dim2 = onScreen.Y + (snapSymbolSize / 2);
            double dim3 = onScreen.X - (snapSymbolSize / 2);
            double dim4 = onScreen.Y - (snapSymbolSize / 2);

            Point3D topLeftVertex = new Point3D(dim3, dim2);
            Point3D topRightVertex = new Point3D(dim1, dim2);
            Point3D bottomRightVertex = new Point3D(dim1, dim4);
            Point3D bottomLeftVertex = new Point3D(dim3, dim4);

            renderContext.DrawLineLoop(new Point3D[]
            {
                bottomLeftVertex,
                bottomRightVertex,
                topRightVertex,
                topLeftVertex
            });
        }

        /// <summary>
        /// Draw triangle with renderContext to rapresent MID snap point
        /// </summary>
        void DrawTriangle(System.Drawing.Point onScreen)
        {
            double dim1 = onScreen.X + (snapSymbolSize / 2);
            double dim2 = onScreen.Y + (snapSymbolSize / 2);
            double dim3 = onScreen.X - (snapSymbolSize / 2);
            double dim4 = onScreen.Y - (snapSymbolSize / 2);
            double dim5 = onScreen.X;

            Point3D topCenter = new Point3D(dim5, dim2);

            Point3D bottomRightVertex = new Point3D(dim1, dim4);
            Point3D bottomLeftVertex  = new Point3D(dim3, dim4);

            renderContext.DrawLineLoop(new Point3D[]
            {
                bottomLeftVertex,
                bottomRightVertex,
                topCenter
            });
        }


        void DrawRhombus(System.Drawing.Point onScreen)
        {
            double dim1 = onScreen.X + (snapSymbolSize / 1.5);
            double dim2 = onScreen.Y + (snapSymbolSize / 1.5);
            double dim3 = onScreen.X - (snapSymbolSize / 1.5);
            double dim4 = onScreen.Y - (snapSymbolSize / 1.5);

            Point3D topVertex    = new Point3D(onScreen.X, dim2);
            Point3D bottomVertex = new Point3D(onScreen.X, dim4);
            Point3D rightVertex  = new Point3D(dim1, onScreen.Y);
            Point3D leftVertex   = new Point3D(dim3, onScreen.Y);

            renderContext.DrawLineLoop(new Point3D[]
            {
                bottomVertex,
                rightVertex,
                topVertex,
                leftVertex,
            });
        }

        #endregion
    }

}