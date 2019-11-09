using devDept.Geometry;

namespace WpfApplication1
{
    public partial class MyDrawings
    {
        /// <summary>
        /// Draws a plus sign (+) at current mouse location
        /// </summary>
        private void DrawPositionMark(Point3D current, double crossSize = 20.0)
        {
            Point3D currentScreen = WorldToScreen(current);

            // computes the horizontal line direction on screen
            Point2D left = WorldToScreen(current.X - 1, current.Y, 0);
            Vector2D dirHorizontal = Vector2D.Subtract(left, currentScreen);
            dirHorizontal.Normalize();

            // computes the horizontal line endpoints position on screen
            left = currentScreen + dirHorizontal * crossSize;
            Point2D right = currentScreen - dirHorizontal * crossSize;

            renderContext.DrawLine(left, right);

            // computes the vertical line direction on screen
            Point2D bottom = WorldToScreen(current.X, current.Y - 1, 0);
            Vector2D dirVertical = Vector2D.Subtract(bottom, currentScreen);
            dirVertical.Normalize();

            // computes  the vertical line endpoints position on screen
            bottom = currentScreen + dirVertical * crossSize;
            Point2D top = currentScreen - dirVertical * crossSize;

            renderContext.DrawLine(bottom, top);

            renderContext.SetLineSize(1);
        }

        /// <summary>
        /// Draws the pick box at the current mouse location
        /// </summary>
        public void DrawSelectionMark(System.Drawing.Point current)
        {
            // takes the size of the pick box
            double size = PickBoxSize;

            double x1 = current.X - (size / 2);
            double y1 = Size.Height - current.Y - (size / 2);
            double x2 = current.X + (size / 2);
            double y2 = Size.Height - current.Y + (size / 2);

            Point3D bottomLeftVertex = new Point3D(x1, y1);
            Point3D bottomRightVertex = new Point3D(x2, y1);
            Point3D topRightVertex = new Point3D(x2, y2);
            Point3D topLeftVertex = new Point3D(x1, y2);

            // draws the box
            renderContext.DrawLineLoop(new Point3D[]
            {
                bottomLeftVertex,
                bottomRightVertex,
                topRightVertex,
                topLeftVertex
            });

            renderContext.SetLineSize(1);
        }
    }
}
