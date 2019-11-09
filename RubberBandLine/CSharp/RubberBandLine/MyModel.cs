using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Input;
using devDept.Eyeshot;
using devDept.Graphics;
using System.Collections;
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using System.Diagnostics;

namespace WpfApplication1
{

    public class MyModel : devDept.Eyeshot.Model
    {        
        Point3D p1, p2, p3;
        Plane plane = Plane.XY;

        Point3D current;

        bool firstClick = false;
        
    
        public List<Point3D> points = new List<Point3D>();


        // Set internal p1, p2, p3 and plane members
        public void SetPlane(Point3D p1, Point3D p2, Point3D p3)
        {

            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            plane = new Plane(p1, p2, p3);

        }
        
        // Every click adds a line
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))))
            {
                base.OnMouseUp(e);

                return;
            }

            if (ActionMode == actionType.None && e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {

                if (firstClick == false)
                {
                    points.Clear();
                    firstClick = true;
                }

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, out current);
                points.Add(current);

            }
            else if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {

                ScreenToPlane(RenderContextUtility.ConvertPoint(GetMousePosition(e)), plane, out current);
                points.Add(current);

                lp = new LinearPath(points);

                lp.LineWeightMethod = colorMethodType.byEntity;
                lp.LineWeight = 2;

                Entities.Add(lp, System.Drawing.Color.ForestGreen);
                points.Clear();

                current = null;
    
                Invalidate(); 
            }
         
            base.OnMouseUp(e);
        }

        public LinearPath lp;
        System.Drawing.Point mouseLocation;

        protected override void OnMouseMove(MouseEventArgs e)
        {            
            // save the current mouse position
            mouseLocation = RenderContextUtility.ConvertPoint(GetMousePosition(e));

            // if start is valid and actionMode is None and it's not in the toolbar area

            if (current == null || ActionMode != actionType.None || GetToolBar().Contains(RenderContextUtility.ConvertPoint(GetMousePosition(e))))
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
            ScreenToPlane(mouseLocation, plane, out current);
            
            // draw the elastic line
            renderContext.SetLineSize(1);

            // draw inverted
            renderContext.EnableXOR(true);

            renderContext.SetState(depthStencilStateType.DepthTestOff);

            // entity drawing in 2D
            lp = new LinearPath(points);

            List<Point3D> pts = new List<Point3D>();

            // draw the elastic line
            for (int i = 0; i < lp.Vertices.Length; i++)

                pts.Add(WorldToScreen(lp.Vertices[i]));

            foreach (var pt in pts)

                pt.Z = 0; // Avoid clipping by camera planes

            if (pts.Count > 0)
                renderContext.DrawLineStrip(pts.ToArray());

            if (ActionMode == actionType.None && !GetToolBar().Contains(mouseLocation) && lp.Vertices.Length > 0)
            {
                List<Point3D> pts2 = new List<Point3D>();

                // Draw elastic line
                pts2.Add(WorldToScreen(lp.Vertices[lp.Vertices.Length - 1]));
                pts2.Add(WorldToScreen(current));

                // cross drawing in 3D
                Point3D left = WorldToScreen(current.X - (p2.X - p1.X) / 10, current.Y, current.Z);
                Point3D right = WorldToScreen(current.X + (p2.X - p1.X) / 10, current.Y, current.Z);

                pts2.Add(left);
                pts2.Add(right);

                Point3D bottom = WorldToScreen(current.X, current.Y - (p3.Y - p1.Y) / 10, current.Z - (p3.Z - p1.Z) / 10);
                Point3D top = WorldToScreen(current.X, current.Y + (p3.Y - p1.Y) / 10, current.Z + (p3.Z - p1.Z) / 10);

                pts2.Add(bottom);
                pts2.Add(top);

                foreach (var pt in pts2)

                    pt.Z = 0; // Avoid clipping by camera planes

                renderContext.DrawLines(pts2.ToArray());

                // disables draw inverted
                renderContext.EnableXOR(false);

                // text drawing
                DrawText(mouseLocation.X, 
                    (int)Size.Height - mouseLocation.Y + 10,
                    "Current point: "
                    + current.X.ToString("f2") + ", "
                    + current.Y.ToString("f2") + ", "
                    + current.Z.ToString("f2"), new Font("Tahoma", 8.25f), Color.Black, ContentAlignment.BottomLeft);
            }
            else
            {
                // disables draw inverted
                renderContext.EnableXOR(false);
            }

            base.DrawOverlay(myParams);
        }

    }

}