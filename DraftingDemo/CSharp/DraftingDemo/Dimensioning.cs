using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;

using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Graphics;

namespace WpfApplication1
{
    /// <summary>
    /// Contains methods required for dimensioning different entities.
    /// Linear, aligned, radial and diametric dimensioning is supported as of now.
    /// </summary>
    partial class MyModel
    {

        // Draws preview of horizontal/vertical dimension for a line
        private void DrawInteractiveLinearDim()
        {
            // We need to have two reference points selected, might be snapped vertices
            if (points.Count < 2)
            {
                return;
            }

            bool verticalDim = (current.X > points[0].X && current.X > points[1].X) || (current.X < points[0].X && current.X < points[1].X);
            
            Vector3D axisX;
            
            if (verticalDim)
            {

                axisX = Vector3D.AxisY;

                extPt1 = new Point3D(current.X, points[0].Y);
                extPt2 = new Point3D(current.X, points[1].Y);

                if (current.X > points[0].X && current.X > points[1].X)
                {
                    extPt1.X += dimTextHeight / 2;
                    extPt2.X += dimTextHeight / 2;
                }
                else
                {
                    extPt1.X -= dimTextHeight / 2;
                    extPt2.X -= dimTextHeight / 2;
                }

            }
            else//for horizontal
            {

                axisX = Vector3D.AxisX;

                extPt1 = new Point3D(points[0].X, current.Y);
                extPt2 = new Point3D(points[1].X, current.Y);

                if (current.Y > points[0].Y && current.Y > points[1].Y)
                {
                    extPt1.Y += dimTextHeight / 2;
                    extPt2.Y += dimTextHeight / 2;
                }
                else
                {
                    extPt1.Y -= dimTextHeight / 2;
                    extPt2.Y -= dimTextHeight / 2;
                }
               
            }

            Vector3D axisY = Vector3D.Cross(Vector3D.AxisZ, axisX);


            List<Point3D> pts = new List<Point3D>();
            
            // Draw extension line1
            pts.Add(WorldToScreen(points[0]));
            pts.Add(WorldToScreen(extPt1));
            
            // Draw extension line2
            pts.Add(WorldToScreen(points[1]));
            pts.Add(WorldToScreen(extPt2));
            
            //Draw dimension line
            Segment3D extLine1 = new Segment3D(points[0], extPt1);
            Segment3D extLine2 = new Segment3D(points[1], extPt2);
            Point3D pt1 = current.ProjectTo(extLine1);
            Point3D pt2 = current.ProjectTo(extLine2);

            pts.Add(WorldToScreen(pt1));
            pts.Add(WorldToScreen(pt2));

            renderContext.DrawLines(pts.ToArray());
            
            //store dimensioning plane
            drawingPlane = new Plane(points[0], axisX, axisY);
           
            //draw dimension text
            renderContext.EnableXOR(false);

            string dimText = "L " + extPt1.DistanceTo(extPt2).ToString("f3");            
            DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, dimText,
                new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
        }
        
        // Draws preview of aligned dimension
        private void DrawInteractiveAlignedDim()
        {
            // We need to have two reference points selected, might be snapped vertices
            if (points.Count < 2)
            {
                return;
            }

            if (points[1].X < points[0].X || points[1].Y < points[0].Y)
            {
                Point3D p0 = points[0];
                Point3D p1 = points[1];

                Utility.Swap(ref p0, ref p1);

                points[0] = p0;
                points[1] = p1;
            }

            Vector3D axisX = new Vector3D(points[0], points[1]);
            Vector3D axisY = Vector3D.Cross(Vector3D.AxisZ, axisX);

            drawingPlane = new Plane(points[0], axisX, axisY);

            Vector2D v1 = new Vector2D(points[0], points[1]);
            Vector2D v2 = new Vector2D(points[0], current);

            double sign = Math.Sign(Vector2D.SignedAngleBetween(v1, v2));
            
            //offset p0-p1 at current
            Segment2D segment = new Segment2D(points[0], points[1]);
            double offsetDist = current.DistanceTo(segment);
            extPt1 = points[0] + sign * drawingPlane.AxisY * (offsetDist + dimTextHeight /2); 
            extPt2 = points[1] + sign * drawingPlane.AxisY * (offsetDist + dimTextHeight /2);
            Point3D dimPt1 = points[0] + sign * drawingPlane.AxisY * offsetDist;
            Point3D dimPt2 = points[1] + sign * drawingPlane.AxisY * offsetDist; 

            List<Point3D> pts = new List<Point3D>();

            // Draw extension line1
            pts.Add(WorldToScreen(points[0]));
            pts.Add(WorldToScreen(extPt1));
            
            // Draw extension line2
            pts.Add(WorldToScreen(points[1]));
            pts.Add(WorldToScreen(extPt2));
            
            //Draw dimension line
            pts.Add(WorldToScreen(dimPt1));
            pts.Add(WorldToScreen(dimPt2));

            renderContext.DrawLines(pts.ToArray());
            
            //draw dimension text
            renderContext.EnableXOR(false);

            string dimText = "L " + extPt1.DistanceTo(extPt2).ToString("f3");            
            DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, dimText,
                new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
        }

        // Draws preview of ordinate dimension
        private void DrawInteractiveOrdinateDim()
        {
            // We need to have at least one point.
            if (points.Count < 1)
                return;

            List<Point3D> pts = new List<Point3D>();
            Point3D leaderEndPoint;
            Segment3D[] segments = OrdinateDim.Preview(Plane.XY, points[0], current, drawingOrdinateDimVertical, dimTextHeight * 3, dimTextHeight, 0.625, 3.0, 0.625, out leaderEndPoint);            

            foreach (var segment3D in segments)
            {
                pts.Add(WorldToScreen(segment3D.P0));
                pts.Add(WorldToScreen(segment3D.P1));
            }

            //draw the segments
            renderContext.DrawLines(pts.ToArray());

            //draw dimension text
            renderContext.EnableXOR(false);

            double distance = drawingOrdinateDimVertical ? Math.Abs(Plane.XY.Origin.X - points[0].X) : Math.Abs(Plane.XY.Origin.Y - points[0].Y);

            string dimText = "D " + distance.ToString("f3");
            DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, dimText,
                new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
        }

        // Draws preview of radial/diametric dimension with text like R5.25, Ø12.62
        private void DrawInteractiveDiametricDim()
        {
            if (selEntityIndex != -1)
            {
                Entity entity = this.Entities[selEntityIndex];
                if (entity is Circle) //arc is a circle
                {
                    Circle cicularEntity = entity as Circle;

                    //draw center mark
                    DrawPositionMark(cicularEntity.Center);

                    //draw elastic line between center and cursor point
                    renderContext.DrawLine(WorldToScreen(cicularEntity.Center), WorldToScreen(current));
                    
                    // disables draw inverted
                    renderContext.EnableXOR(false);
                    
                    string dimText = "R" + cicularEntity.Radius.ToString("f3");

                    if (drawingDiametricDim)
                    {
                        dimText = "Ø" + cicularEntity.Diameter.ToString("f3");
                    }                    
                    DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, dimText,
                        new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
                }
            }
        }

        // Draws preview of radial/diametric dimension with text like R5.25, Ø12.62
        private void DrawInteractiveAngularDim()
        {
            if (selEntityIndex != -1)
            {
                Entity entity = Entities[selEntityIndex];

                if (entity is Arc && !drawingAngularDimFromLines)
                {
                    Arc selectedArc = entity as Arc;

                    //draw center mark
                    DrawPositionMark(selectedArc.Center);

                    //draw elastic line between center and cursor point
                    renderContext.DrawLine(WorldToScreen(selectedArc.Center), WorldToScreen(current));

                    // disables draw inverted
                    renderContext.EnableXOR(false);

                    string dimText = "A " + Utility.RadToDeg(selectedArc.Domain.Length).ToString("f3") + "°";

                    DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, dimText, new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
                }
            }
            else if (drawingAngularDimFromLines && quadrantPoint != null)
            {
                //draw quadrant point mark
                DrawPositionMark(quadrantPoint);

                //draw elastic line between quadrant Point and cursor point
                renderContext.DrawLine(WorldToScreen(quadrantPoint), WorldToScreen(current));

                // disables draw inverted
                renderContext.EnableXOR(false);

                DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10, "", new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
            }
        }

        #region Dimensioning Flags

        public bool drawingLinearDim;

        public bool drawingAlignedDim;

        public bool drawingRadialDim;

        public bool drawingDiametricDim;

        public bool drawingAngularDim;

        public bool drawingAngularDimFromLines;

        public bool drawingOrdinateDim;
        public bool drawingOrdinateDimVertical;

        public bool drawingQuadrantPoint;

        public double dimTextHeight = 2.5;

        #endregion
    }

}