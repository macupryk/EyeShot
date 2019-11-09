using System;
using System.Collections.Generic;
using System.Text;

using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using System.Drawing;
using System.Collections;

namespace WpfApplication1
{
    public class MyModel : devDept.Eyeshot.Model
    {

        Point3D p1 = Point3D.Origin;
        Point3D p2 = Point3D.Origin;
        Point3D p3 = Point3D.Origin;
        Plane plane = Plane.XY;

        double wallHeight;
        Color wallColor = Color.Firebrick;

        Point3D start, end, current;

        bool firstClick = false;

        #region Properties

        public double WallHeight
        {

            get { return wallHeight; }
            set { wallHeight = value; }

        }

        public Color WallColor
        {

            get { return wallColor; }
            set { wallColor = value; }

        } 

        #endregion

        // Set internal p1, p2, p3 and plane members
        public void SetPlane(Point3D p1, Point3D p2, Point3D p3)
        {

            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            plane = new Plane(p1, p2, p3);

        }

        // Every click adds a Quad
        protected override void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            
            if (ActionMode == actionType.None && !GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))) && e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {

                if (firstClick == false)
                {

                    ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, out start);

                    SnapToGrid(ref start);

                    firstClick = true;

                }
                else
                {

                    ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, out end);

                    SnapToGrid(ref end);

                    Line l = new Line(start, end);

                    Entities.Add(new Quad(
                        l.StartPoint, 
                        l.EndPoint, 
                        new Point3D(l.EndPoint.X, l.EndPoint.Y, l.EndPoint.Z + wallHeight), 
                        new Point3D(l.StartPoint.X, l.StartPoint.Y, l.StartPoint.Z + wallHeight)), wallColor) ;

                    start = end;

                    Invalidate();


                }

            }

            base.OnMouseUp(e);

        }

        System.Drawing.Point mouseLocation;

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {                    
            // save the current mouse position
            mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            // if start is valid and actionMode is None
            if (ActionMode != actionType.None || GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))))
            {

                base.OnMouseMove(e);

                return;
            }


            // paint the viewport surface
            PaintBackBuffer();

            // consolidates the drawing
            SwapBuffers();
            
            base.OnMouseMove(e);

        }

        protected override void DrawOverlay(DrawSceneParams myParams)
        {
            if (ActionMode != actionType.None || GetToolBar().Contains(mouseLocation))
            {
                base.DrawOverlay(myParams);
                return;
            }

            ScreenToPlane(mouseLocation, plane, out current);

            SnapToGrid(ref current);

            // draw inverted
            renderContext.EnableXOR(true);

            renderContext.SetState(depthStencilStateType.DepthTestOff);

            if (firstClick)
            {
                renderContext.SetLineSize(1);
                Point3D screenStart = WorldToScreen(start);
                Point3D screenCurrent = WorldToScreen(current);
                screenStart.Z = 0;
                screenCurrent.Z = 0;

                List<Point3D> pts = new List<Point3D>();
                // elastic line
                pts.Add(screenStart);
                pts.Add(screenCurrent);

                renderContext.DrawLines(pts.ToArray());
            }

            // cross drawing in 3D
            renderContext.SetLineSize(3);

            List<Point3D> pts2 = new List<Point3D>();

            Point3D left = WorldToScreen(current.X - (p2.X - p1.X) / 20, current.Y, current.Z);
            Point3D right = WorldToScreen(current.X + (p2.X - p1.X) / 20, current.Y, current.Z);

            pts2.Add(left);
            pts2.Add(right);

            Point3D bottom = WorldToScreen(current.X, current.Y - (p3.Y - p1.Y) / 20, current.Z - (p3.Z - p1.Z) / 20);
            Point3D top = WorldToScreen(current.X, current.Y + (p3.Y - p1.Y) / 20, current.Z + (p3.Z - p1.Z) / 20);

            pts2.Add(bottom);
            pts2.Add(top);

            // Sets the Z to 0 to avoid clipping planes issues
            for (int i = 0; i < pts2.Count; i++)
            {
                pts2[i].Z = 0;
            }

            renderContext.DrawLines(pts2.ToArray());

            // disables draw inverted
            renderContext.EnableXOR(false);

            renderContext.EnableXORForTexture(true, myParams.ShaderParams);

            // text drawing
            DrawText(mouseLocation.X, (int)Size.Height - mouseLocation.Y + 10,
                    "Current point: "
                    + current.X.ToString("f2") + ", "
                    + current.Y.ToString("f2") + ", "
                    + current.Z.ToString("f2"), new Font("Tahoma", 8.25f), Color.Black, ContentAlignment.BottomLeft);

            renderContext.EnableXORForTexture(false, myParams.ShaderParams);

            base.DrawOverlay(myParams);
        }

        void SnapToGrid(ref Point3D p)
        {

            p.X = Math.Round(p.X / 10) * 10;
            p.Y = Math.Round(p.Y / 10) * 10;

        }
        
    }

}