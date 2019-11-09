using System;
using System.Drawing;

using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot.Labels;
using System.Collections.Generic;


namespace WpfApplication1
{

    partial class Draw
    {
        public static Color Color = Color.Black;
        
        public static void Jet(Model model1)
        {            
            #region Jet drawing

            string fuselage = "Fuselage";
            model1.Layers.Add(fuselage, Color.DarkGray);
            model1.Entities.Add(new Triangle(+15, -30, 8, 0, -30, 23, 0, -60, 8), fuselage, Color.DeepSkyBlue);
            model1.Entities.Add(new Triangle(0, -60, 8, 0, -30, 23, -15, -30, 8), fuselage, Color.DeepSkyBlue);
            model1.Entities.Add(new Triangle(-15, -30, 8, 0, -30, 23, 0, +56, 8), fuselage);
            model1.Entities.Add(new Triangle(0, +56, 8, 0, -30, 23, 15, -30, 8), fuselage);
            model1.Entities.Add(new Quad(0, +56, 8, +15, -30, 8, 0, -60, 8, -15, -30, 8), fuselage);

            string wings = "Wings";
            model1.Layers.Add(wings, Color.CornflowerBlue);
            model1.Entities.Add(new Triangle(0, -27, 10, -60, +8, 10, 60, +8, 10), wings);
            model1.Entities.Add(new Triangle(60, +8, 10, 0, +8, 15, 0, -27, 10), wings);
            model1.Entities.Add(new Triangle(60, +8, 10, -60, +8, 10, 0, +8, 15), wings);
            model1.Entities.Add(new Triangle(0, -27, 10, 0, +8, 15, -60, +8, 10), wings);

            string tail = "Tail";
            model1.Layers.Add(tail, Color.Chartreuse);
            model1.Entities.Add(new Triangle(-30, +57, 7.5, 30, +57, 7.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(0, +40, 7.5, 30, +57, 7.5, 0, +57, 12), tail);
            model1.Entities.Add(new Triangle(0, +57, 12, -30, +57, 7.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(30, +57, 7.5, -30, +57, 7.5, 0, +57, 12), tail);
            model1.Entities.Add(new Triangle(0, +40, 7.5, 3, +57, 8.5, 0, +65, 33), tail);
            model1.Entities.Add(new Triangle(0, +65, 33, -3, +57, 8.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(3, +57, 8.5, -3, +57, 8.5, 0, +65, 33), tail);

            string wires = "Wires";
            model1.Layers.Add(wires);

            Line axis = new Line(-22, 0, 3, 22, 0, 3);

            axis.LineTypeMethod = colorMethodType.byEntity;
            model1.LineTypes.Add("JetAxisPattern", new float[] { 5, -1.5f, 0.25f, -1.5f });
            axis.LineTypeName = "JetAxisPattern";

            model1.Entities.Add(axis, "Wires");

            // Points
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +12, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +16, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +21, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +27, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +34, 10, 4), wires);

            // Wheels
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(+20, 0, 3), 3), wires);
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(-20, 0, 3), 3), wires);
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(0, -42, 2), 2), wires);

            // Wheel crosses
            model1.Entities.Add(new Line(-20, 0, 2, -20, 0, 4), wires);
            model1.Entities.Add(new Line(-20, -1, 3, -20, +1, 3), wires);
            model1.Entities.Add(new Line(+20, 0, 2, +20, 0, 4), wires);
            model1.Entities.Add(new Line(+20, -1, 3, +20, +1, 3), wires);
            model1.Entities.Add(new Line(0, -41, 2, 0, -43, 2), wires);
            model1.Entities.Add(new Line(0, -42, 1, 0, -42, 3), wires);

            #endregion

            // Labels            
            model1.Labels.Add(new LeaderAndText(+60, +8, 10, "Left wing", new Font("Tahoma", 8.25f), Draw.Color, new Vector2D(0, 30)));
            model1.Labels.Add(new ImageOnly(0, +65, 33, WpfApplication1.Properties.Resources.CautionLabel));

            // Dimensions
            Plane dimPlane1 = Plane.XY;
            dimPlane1.Rotate(Math.PI / 2, Vector3D.AxisZ, Point3D.Origin);
            Plane dimPlane2 = Plane.YZ;
            dimPlane2.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin);

            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, -60, 8), new Point3D(60, 8, 8), new Point3D(70, -26, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, -60, 8), new Point3D(0, 65, 8), new Point3D(80, 0, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, 57, 8), new Point3D(0, 65, 8), new Point3D(70, 80, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane2, new Point3D(0, 57, 8), new Point3D(0, 65, 33), new Point3D(0, 75, 20.5), 4), wires);

            model1.ZoomFit();
        }       
        
    }
}