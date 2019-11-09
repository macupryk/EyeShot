using System.Drawing;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Collections.Generic;
using Point = System.Drawing.Point;

namespace WindowsApplication1
{
    class DrawCamera
    {
        private Point3D[] pNear = new Point3D[4];
        private Point3D[] pFar = new Point3D[4];
        private Camera Camera;
        private string LayerName;

        public DrawCamera(Viewport viewport, double controlHeight, string layerName)
        {
            Camera = viewport.Camera;
            LayerName = layerName;

            int[] viewFrame = viewport.GetViewFrame();

            // gets Near Plane vertices
            Point3D[] pts = Camera.ScreenToPlane(new List<Point>
            {
                new Point(0, 0),
                new Point(0, viewport.Size.Height),
                new Point(viewport.Size.Width, viewport.Size.Height),
                new Point(viewport.Size.Width, 0),
            }, Camera.NearPlane.Equation, (int)controlHeight, viewFrame);

            pNear[0] = pts[0];
            pNear[1] = pts[1];
            pNear[2] = pts[2];
            pNear[3] = pts[3];

            // gets Far Plane vertices
            pts = Camera.ScreenToPlane(new List<Point>
            {
                new Point(0, 0),
                new Point(0, viewport.Size.Height),
                new Point(viewport.Size.Width, viewport.Size.Height),
                new Point(viewport.Size.Width, 0),
            }, Camera.FarPlane.Equation, (int)controlHeight, viewFrame);

            pFar[0] = pts[0];
            pFar[1] = pts[1];
            pFar[2] = pts[2];
            pFar[3] = pts[3];
        }

        public void Draw(Model model)
        {
            Point3D origin;
            Vector3D camX, camY, camZ;

            Camera.GetFrame(out origin, out camX, out camY, out camZ);
            if (origin != null)
            {
                // Draws the View Volume
                Point3D[] pts = new Point3D[24];
                int count = 0;

                for (int i = 0; i < 4; i++)
                {
                    pts[count++] = pNear[i];
                    pts[count++] = pNear[(i + 1) % 4];
                    pts[count++] = pFar[(i + 1) % 4];
                    pts[count++] = pFar[i];
                    pts[count++] = origin;
                    pts[count++] = pNear[(i + 1) % 4];
                }

                LinearPath lp1 = new LinearPath(pts);
                lp1.Color = Color.Gray;
                lp1.ColorMethod = colorMethodType.byEntity;
                model.Entities.Add(lp1, LayerName);

                //Draws the Camera
                const double widthB = 3, heightB = 5, depthB = 3, heightC = widthB / 2, radiusC = 1.5;

                Mesh cone = Mesh.CreateCone(radiusC, radiusC / 2, heightC, 10);
                cone.ColorMethod = colorMethodType.byEntity;
                cone.Color = Color.GreenYellow;

                Mesh box = Mesh.CreateBox(widthB, depthB, heightB);
                box.ColorMethod = colorMethodType.byEntity;
                box.Color = Color.GreenYellow;

                // centers the box to the world origin
                box.Translate(-widthB / 2, -depthB / 2, +heightC);

                // Aligns the Camera to the Camera view
                Transformation t = new Align3D(Plane.XY, new Plane(origin, camX, camY));
                box.TransformBy(t);
                cone.TransformBy(t);

                model.Entities.Add(cone, LayerName);
                model.Entities.Add(box, LayerName);
            }
        }

        public void DeletePrevious(Model model)
        {
            model.Layers.Remove(LayerName);
            model.Layers.Add(LayerName);
        }
    }
}
