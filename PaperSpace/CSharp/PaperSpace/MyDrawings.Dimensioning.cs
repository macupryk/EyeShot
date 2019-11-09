using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using devDept.Geometry;

namespace WpfApplication1
{
    public partial class MyDrawings
    {

        private double _dimTextHeight = 2.5;
        public double DimTextHeight { get { return _dimTextHeight; } set { _dimTextHeight = value; } }
        public bool DrawingLinearDim { get; set; }
        private double _viewScale = 1;

        // Draws preview of horizontal/vertical dimension for a line
        private void DrawInteractiveLinearDim()
        {
            // 2 points needed to draw the interactive LinearDim
            if (_numPoints < 2)
                return;

            bool verticalDim = (_current.X > _points[0].X && _current.X > _points[1].X) || (_current.X < _points[0].X && _current.X < _points[1].X);

            Vector3D axisX;

            double convertedDimTextHeight = DimTextHeight * GetUnitsConversionFactor();

            if (verticalDim)
            {
                axisX = Vector3D.AxisY;

                _extPt1 = new Point3D(_current.X, _points[0].Y);
                _extPt2 = new Point3D(_current.X, _points[1].Y);

                if (_current.X > _points[0].X && _current.X > _points[1].X)
                {
                    _extPt1.X += convertedDimTextHeight / 2;
                    _extPt2.X += convertedDimTextHeight / 2;
                }
                else
                {
                    _extPt1.X -= convertedDimTextHeight / 2;
                    _extPt2.X -= convertedDimTextHeight / 2;
                }

            }
            else // for horizontal LinearDim
            {
                axisX = Vector3D.AxisX;

                _extPt1 = new Point3D(_points[0].X, _current.Y);
                _extPt2 = new Point3D(_points[1].X, _current.Y);

                if (_current.Y > _points[0].Y && _current.Y > _points[1].Y)
                {
                    _extPt1.Y += convertedDimTextHeight / 2;
                    _extPt2.Y += convertedDimTextHeight / 2;
                }
                else
                {
                    _extPt1.Y -= convertedDimTextHeight / 2;
                    _extPt2.Y -= convertedDimTextHeight / 2;
                }
            }

            // defines the Y axis
            Vector3D axisY = Vector3D.Cross(Vector3D.AxisZ, axisX);

            List<Point3D> pts = new List<Point3D>();

            // draws extension line1
            pts.Add(WorldToScreen(_points[0]));
            pts.Add(WorldToScreen(_extPt1));

            // draws extension line2
            pts.Add(WorldToScreen(_points[1]));
            pts.Add(WorldToScreen(_extPt2));

            // draws dimension line
            Segment3D extLine1 = new Segment3D(_points[0], _extPt1);
            Segment3D extLine2 = new Segment3D(_points[1], _extPt2);
            Point3D pt1 = _current.ProjectTo(extLine1);
            Point3D pt2 = _current.ProjectTo(extLine2);

            pts.Add(WorldToScreen(pt1));
            pts.Add(WorldToScreen(pt2));

            renderContext.DrawLines(pts.ToArray());

            // stores dimensioning plane
            _drawingPlane = new Plane(_points[0], axisX, axisY);

            // draws dimension text
            renderContext.EnableXOR(false);

            // calculates the scaled distance
            var scaledDistance = _extPt1.DistanceTo(_extPt2) * (1 / _viewScale);
            string dimText = "L " + scaledDistance.ToString("f3");
            DrawText(_mouseLocation.X, (int) Size.Height - _mouseLocation.Y + 10, dimText,
                new Font("Tahoma", 8.25f), DrawingColor, ContentAlignment.BottomLeft);
        }
    }
}
