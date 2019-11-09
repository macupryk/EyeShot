using System.Drawing;
using devDept.Geometry;
using devDept.Graphics;

namespace WpfApplication1
{
    public partial class MyDrawings
    {
        // current snapped point which is one of the vertex of the view
        private Point3D _snappedPoint;

        private const int SnapQuadSize = 12;

        /// <summary>
        /// Draws the quad that defines the snapped point
        /// </summary>
        private void DrawQuad(System.Drawing.Point onScreen)
        {
            double x1 = onScreen.X - (SnapQuadSize / 2);
            double y1 = onScreen.Y - (SnapQuadSize / 2);
            double x2 = onScreen.X + (SnapQuadSize / 2);
            double y2 = onScreen.Y + (SnapQuadSize / 2);

            Point3D bottomLeftVertex = new Point3D(x1, y1);
            Point3D bottomRightVertex = new Point3D(x2, y1);
            Point3D topRightVertex = new Point3D(x2, y2);
            Point3D topLeftVertex = new Point3D(x1, y2);

            renderContext.DrawLineLoop(new Point3D[]
            {
                bottomLeftVertex,
                bottomRightVertex,
                topRightVertex,
                topLeftVertex
            });
        }

        /// <summary>
        /// Displays the snapped point
        /// </summary>
        private void DisplaySnappedVertex()
        {
            renderContext.SetLineSize(2);

            // blue color
            renderContext.SetColorWireframe(Color.FromArgb(0, 0, 255));
            renderContext.SetState(depthStencilStateType.DepthTestOff);

            Point2D onScreen = WorldToScreen(_snappedPoint);

            DrawQuad(new System.Drawing.Point((int)onScreen.X, (int)(onScreen.Y)));
            renderContext.SetLineSize(1);
        }
    }
}
