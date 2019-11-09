using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
using devDept.Graphics;

namespace WpfApplication1
{
    /// <summary>
    /// Contains all methods required to draw different entities interactively
    /// </summary>
    partial class MyModel
    {
        // Draws interactive/elastic lines as per user clicks on mouse move
        private void DrawInteractiveLines()
        {
            if (points.Count == 0)
                return;

            Point2D[] screenPts = GetScreenVertices(points);

            renderContext.DrawLineStrip(screenPts);

            if (ActionMode == actionType.None && !GetToolBar().Contains(mouseLocation) && points.Count > 0)
            {
                // Draw elastic line
                renderContext.DrawLine(screenPts[screenPts.Length - 1], WorldToScreen(current));
            }
        }

        private Point2D[] GetScreenVertices(IList<Point3D> vertices)
        {
            Point2D[] screenPts = new Point2D[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                screenPts[i] = WorldToScreen(vertices[i]);
            }
            return screenPts;
        }

        // Draws interactive circle (rubber-band) on mouse move with fixed center
        private void DrawInteractiveCircle()
        {
            radius = points[0].DistanceTo(current);
            
            if (radius > 1e-3)
            {
                drawingPlane = GetPlane(current);

                DrawPositionMark(points[0]);

                Circle tempCircle = new Circle(drawingPlane, points[0], radius);

                Draw(tempCircle);
            }

        }

        // Draws interactive leader
        private void DrawInteractiveLeader()
        {
            renderContext.EnableXOR(false);
            string text;
            if (points.Count == 0)
                text = "Select the first point";
            else if (points.Count == 1)
                text = "Select the second point";
            else
                text = "Select the third point";

            DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10,
                text, new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);

            renderContext.EnableXOR(true);

            DrawInteractiveLines();
        }

        private Plane GetPlane(Point3D next)
        {
            Vector3D xAxis = new Vector3D(points[0], next);
            xAxis.Normalize();
            Vector3D yAxis = Vector3D.Cross(Vector3D.AxisZ, xAxis);
            yAxis.Normalize();

            Plane plane = new Plane(points[0], xAxis, yAxis);
            
            return plane;
        }

        // Draws interactive arc with selected center point position and two end points
        private void DrawInteractiveArc()
        {
            Point2D[] screenPts = GetScreenVertices(points);

            renderContext.DrawLineStrip(screenPts);

            if (ActionMode == actionType.None && !GetToolBar().Contains(mouseLocation) && points.Count > 0)
            {
                // Draw elastic line
                renderContext.DrawLine(WorldToScreen(points[0]), WorldToScreen(current));

                //draw three point arc
                if (points.Count == 2)
                {

                    radius = points[0].DistanceTo(points[1]);

                    if (radius > 1e-3)
                    {
                        drawingPlane = GetPlane(points[1]);

                        Vector2D v1 = new Vector2D(points[0], points[1]);
                        v1.Normalize();
                        Vector2D v2 = new Vector2D(points[0], current);
                        v2.Normalize();

                        arcSpanAngle = Vector2D.SignedAngleBetween(v1, v2);

                        if (Math.Abs(arcSpanAngle) > 1e-3)
                        {

                            Arc tempArc = new Arc(drawingPlane, drawingPlane.Origin, radius, 0, arcSpanAngle);

                            Draw(tempArc);

                        }

                    }
                }

            }
        }

        // Draws interactive ellipse on mouse move with fixed center and given axis ends
        // Inputs - Ellipse center, End of first axis, End of second axis
        private void DrawInteractiveEllipse()
        {

            if (drawingEllipticalArc && points.Count > 2)
            {
                return;
            }

            if (points.Count == 1)
            {
                // Draw elastic line
                renderContext.DrawLine(WorldToScreen(points[0]), WorldToScreen(current));
            }

            if (points.Count < 2)
            {
                return;
            }

            radius = points[0].DistanceTo(points[1]);
            radiusY = current.DistanceTo(new Segment2D(points[0], points[1]));

            if (radius > 1e-3 && radiusY > 1e-3)
            {
                drawingPlane = GetPlane(points[1]);

                DrawPositionMark(points[0]);

                Ellipse tempEllipse = new Ellipse(drawingPlane, drawingPlane.Origin, radius, radiusY);

                Draw(tempEllipse);
            }

        }

        private void Draw(ICurve theCurve)
        {
            if (theCurve is CompositeCurve)
            {
                CompositeCurve compositeCurve = theCurve as CompositeCurve;
                Entity[] explodedCurves = compositeCurve.Explode();
                foreach (Entity ent in explodedCurves)

                    DrawScreenCurve((ICurve) ent);
            }
            else
            {
                DrawScreenCurve(theCurve);
            }
        }

        private void DrawScreenCurve(ICurve curve)
        {
            const int subd = 100;

            Point3D[] pts = new Point3D[subd + 1];

            for (int i = 0; i <= subd; i++)
            {
                pts[i] = WorldToScreen(curve.PointAt(curve.Domain.ParameterAt((double) i/subd)));
            }

            renderContext.DrawLineStrip(pts);
        }

        // Draws interactive elliptical arc 
        // Inputs - Ellipse center, End of first axis, End of second axis, Start and End point
        private void DrawInteractiveEllipticalArc()
        {
            Point3D center = points[0];

            if (points.Count <= 3)
            {
                DrawInteractiveEllipse();
            }

            ScreenToPlane(mouseLocation, plane, out current);

            if (points.Count == 3) // ellipse completed, ask user to select start point
            {
               
                //start position line
                renderContext.DrawLine(WorldToScreen(center), WorldToScreen(points[1]));

                //current position line
                renderContext.DrawLine(WorldToScreen(center), WorldToScreen(current));

                //arc portion
                radius = center.DistanceTo(points[1]);
                radiusY = points[2].DistanceTo(new Segment2D(center, points[1]));

                if (radius > 1e-3 && radiusY > 1e-3)
                {
                    DrawPositionMark(points[0]);

                    drawingPlane = GetPlane(points[1]);

                    Vector2D v1 = new Vector2D(center, points[1]);
                    v1.Normalize();
                    Vector2D v2 = new Vector2D(center, current);
                    v2.Normalize();

                    arcSpanAngle = Vector2D.SignedAngleBetween(v1, v2);

                    if (Math.Abs(arcSpanAngle) > 1e-3)
                    {
                        EllipticalArc tempArc = new EllipticalArc(drawingPlane, drawingPlane.Origin, radius, radiusY, 0, arcSpanAngle, true);
                        
                        Draw(tempArc);
                    }
                }
            }
        }
        
        // Draws interactive/elastic spline curve interpolated from selected points
        private void DrawInteractiveCurve()
        {
#if NURBS
            List<Point3D> plusOne = new List<Point3D>(points);

            plusOne.Add(GetSnappedPoint(mouseLocation, plane, points, 0));

            // Cubic interpolation needs at least 3 points
            if (points.Count > 1)
            {
                Curve tempCurve = Curve.CubicSplineInterpolation(plusOne);

                Draw(tempCurve);
            }
            else

                renderContext.DrawLineStrip(GetScreenVertices(plusOne));            
#endif
        }

        private Point3D GetSnappedPoint(System.Drawing.Point mousePos, Plane plane, IList<Point3D> pts, int indexToCompare)
        {
            // if the mouse in within 10 pixels of the first curve point, return the first point
            if (pts.Count > 0)
            {
                Point3D ptToSnap = pts[indexToCompare];
                Point3D ptSnapScreen = WorldToScreen(ptToSnap);

                Point2D current = new Point2D(mousePos.X, Size.Height - mousePos.Y);

                if (Point2D.Distance(current, ptSnapScreen) < 10)
                    return (Point3D)ptToSnap.Clone();
            }

            Point3D pt;
            ScreenToPlane(mousePos, plane, out pt);
            return pt;
        }

        //Checks if polyline or curve can be closed polygon
        public bool IsPolygonClosed()
        {
            if (points.Count > 0 && (drawingCurve || drawingPolyLine) && (points[0].DistanceTo(current) < magnetRange))
            {                
                return true;
            }

            return false;
        }

        //Draws pickbox at current mouse location
        public void DrawSelectionMark(System.Drawing.Point current)
        {
            double size = PickBoxSize;
            double dim1 = current.X + (size/2);
            double dim2 = Size.Height - current.Y + (size / 2);
            double dim3 = current.X - (size/2);
            double dim4 = Size.Height - current.Y - (size / 2);

            Point3D topLeftVertex = new Point3D(dim3, dim2);
            Point3D topRightVertex = new Point3D(dim1, dim2);
            Point3D bottomRightVertex = new Point3D(dim1, dim4);
            Point3D bottomLeftVertex = new Point3D(dim3, dim4);

            renderContext.DrawLines(
                new Point3D[]
                {
                    bottomLeftVertex,
                    bottomRightVertex,
                    bottomRightVertex,
                    topRightVertex,
                    topRightVertex,
                    topLeftVertex,
                    topLeftVertex,
                    bottomLeftVertex
                });
            

            renderContext.SetLineSize(1);
        }

        // Draws a plus sign (+) at current mouse location
        private void DrawPositionMark(Point3D current, double crossSide = 20.0)
        {            
            if (IsPolygonClosed())
            {
                current = points[0];
            }

            if (gridSnapEnabled)
            {
                if (SnapToGrid(ref current))
                    renderContext.SetLineSize(4);
            }

            Point3D currentScreen = WorldToScreen(current);

            // Compute the direction on screen of the horizontal line
            Point2D left = WorldToScreen(current.X - 1, current.Y, 0);
            Vector2D dirHorizontal = Vector2D.Subtract(left, currentScreen);
            dirHorizontal.Normalize();

            // Compute the position on screen of the line endpoints
            left = currentScreen + dirHorizontal*crossSide;
            Point2D right = currentScreen - dirHorizontal * crossSide;

            renderContext.DrawLine(left, right);

            // Compute the direction on screen of the vertical line
            Point2D bottom = WorldToScreen(current.X, current.Y - 1, 0);
            Vector2D dirVertical = Vector2D.Subtract(bottom, currentScreen);
            dirVertical.Normalize();

            // Compute the position on screen of the line endpoints
            bottom = currentScreen + dirVertical * crossSide;
            Point2D top = currentScreen - dirVertical * crossSide;

            renderContext.DrawLine(bottom, top);

            renderContext.SetLineSize(1);
        }

        #region Flags indicating current drawing mode

        public bool drawingPoints;

        public bool drawingText;

        public bool drawingLeader;

        public bool drawingEllipse;

        public bool drawingEllipticalArc;

        public bool drawingLine;

        public bool drawingCurve;

        public bool drawingCircle;

        public bool drawingArc;

        public bool drawingPolyLine;

        #endregion
    }

}