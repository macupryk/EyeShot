using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using System.IO;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;


using devDept.Eyeshot.Triangulation;
using Region = devDept.Eyeshot.Entities.Region;
using devDept.Eyeshot.Translators;

namespace WpfApplication1
{
    partial class Draw
    {

        public static void MotherBoard(Model model)
        {
            ReadFile rf = new ReadFile(MainWindow.GetAssetsPath() + "Motherboard_ASRock_A330ION.eye");

            model.StartWork(rf);
        }

        public static void Locomotive(Model model)
        {

            Region r1 = Region.CreateRectangle(110, 38);
#if NURBS
            Region r2 = Region.CreateRectangle(0, 19, 8, 19);
#else
            Ellipse el = new Ellipse(0, 19, 0, 8, 19);
            el.Regen(1);
            Region r2 = new Region(new LinearPath(el.Vertices), Plane.XY, false);
#endif
            Region u1 = Region.Union(r1, r2)[0];

            Region r3 = Region.CreateCircle(17, -6, 9);

            Region u2 = Region.Union(u1, r3)[0];

            r3.Translate(20, 0, 0);

            Region u3 = Region.Union(u2, r3)[0];

            Region r4 = Region.CreateCircle(70, 0, 15);

            Region u4 = Region.Union(u3, r4)[0];

            Region r5 = Region.CreateCircle(50, 38, 10);

            Region u5 = Region.Union(u4, r5)[0];

            Region r6 = Region.CreateRectangle(79, 36, 44, 14);

            Region u6 = Region.Union(u5, r6)[0];

            Region r7 = Region.CreateRectangle(- 11, 14, 10, 10);

            Region u7 = Region.Union(u6, r7)[0];

            Region r8 = Region.CreatePolygon(new Point2D(-15, -8), new Point2D(4, -8), new Point2D(4, 8));

            Region u8 = Region.Union(u7, r8)[0];

            Region r9 = Region.CreatePolygon(new Point2D(20, 20), new Point2D(32, 62), new Point2D(26, 72), new Point2D(14, 72), new Point2D(8, 62));

            Region u9 = Region.Union(u8, r9)[0];

            model.Entities.Add(u9, Color.IndianRed);

        }

        public static void Bunny(Model model)
        {

            ReadFile readFile = new ReadFile(MainWindow.GetAssetsPath() + "Bunny.eye");
            readFile.DoWork();

            // scales file contents by 100
            foreach (Entity entity in readFile.Entities)

                entity.Scale(100, 100, 100);

            readFile.AddToScene(model);

            if (model.Entities.Count > 0 && model.Entities[0] is FastPointCloud)
            {

                FastPointCloud fpc = (FastPointCloud)model.Entities[0];

                fpc.Rotate(Math.PI/2, Vector3D.AxisX, Point3D.Origin);
                
                model.Entities.Regen();

                model.ZoomFit();

                BallPivoting bp = new BallPivoting(fpc);

                model.StartWork(bp);

            }
        

        }

        public static void Pocket(Model model)
        {
            
            Point2D[] pts = new Point2D[]
                                {
                                    new Point2D(0, 0),
                                    new Point2D(40, 0),
                                    new Point2D(40, 20),
                                    new Point2D(60, 20),
                                    new Point2D(60, 10),
                                    new Point2D(100, 10),
                                    new Point2D(100, 60),
                                    new Point2D(60, 60),
                                    new Point2D(60, 30),
                                    new Point2D(40, 30),
                                    new Point2D(40, 80),
                                    new Point2D(0, 80),
                                    new Point2D(0, 0),
                                };

            LinearPath outerContour = new LinearPath(Plane.XY, pts);

            outerContour.LineWeightMethod = colorMethodType.byEntity;
            outerContour.LineWeight = 3;
            
            model.Entities.Add(outerContour, Color.OrangeRed);

            Circle innerContour = new Circle(20, 60, 0, 6);

            innerContour.LineWeightMethod = colorMethodType.byEntity;
            innerContour.LineWeight = 3;
            
            model.Entities.Add(innerContour, Color.OrangeRed);

            Region r1 = new Region(new ICurve[] { outerContour, innerContour }, Plane.XY, true);

            ICurve[] passes = r1.Pocket(4, cornerType.Round, .1);

            const double zStep = 2;
            const int stepCount = 10;

            for (int i = 1; i < stepCount; i++)
            {
                foreach (Entity crv in passes)
                {
                    Entity en = (Entity)crv.Clone();
                    en.Translate(0, 0, -i * zStep);
                    model.Entities.Add(en, Color.DarkBlue);
                }
            }
        }

        public static void Primitives(Model model)
        {

            model.GetGrid().Step = 25;
            model.GetGrid().Min.X = -25;
            model.GetGrid().Min.Y = -25;

            model.GetGrid().Max.X = 125;
            model.GetGrid().Max.Y = 175;

            double deltaOffset = 50;
            double offsetX = 0;
            double offsetY = 0;

            // First Row

            // Box
            Mesh mesh = Mesh.CreateBox(40, 40, 30);
            mesh.Translate(-20, -20, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            // Cone
            mesh = Mesh.CreateCone(20, 10, 30, 30, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            mesh = Mesh.CreateCone(20, 0, 30, 30, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            // Second Row
            offsetX = 0;
            offsetY += deltaOffset;

            mesh = Mesh.CreateCone(10, 20, 30, 30, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            mesh = Mesh.CreateCone(20, 10, 30, 3, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            // Sphere
            mesh = Mesh.CreateSphere(20, 3, 3, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);

            // Third Row
            offsetX = 0;
            offsetY += deltaOffset;

            mesh = Mesh.CreateSphere(20, 8, 6, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            mesh = Mesh.CreateSphere(20, 14, 14, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            mesh = Mesh.CreateTorus(18, 5, 15, 17, Mesh.natureType.Smooth);
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            // Fourth Row
            offsetX = 0;
            offsetY += deltaOffset;

            LinearPath lp = LinearPath.CreateHelix(10, 5.3, 10.7, true, 0.25);
            lp.Translate(offsetX, offsetY, 0);
            model.Entities.Add(lp, Draw.Color);
            offsetX += deltaOffset;

            mesh = Mesh.CreateSpring(10, 2, 16, 24, 10, 6, true, true, Mesh.natureType.Smooth);
            mesh.EdgeStyle = Mesh.edgeStyleType.None;
            mesh.Translate(offsetX, offsetY, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
            offsetX += deltaOffset;

            // Sweep
            double z = 30;
            double radius = 15;

            Line l1 = new Line(0, 0, 0, 0, 0, z);
            Arc a1 = new Arc(new Point3D(radius, 0, z), new Point3D(0, 0, z), new Point3D(radius, 0, z + radius));
            Line l2 = new Line(radius, 0, z + radius, 30, 0, z + radius);

            CompositeCurve composite = new CompositeCurve(l1, a1, l2);
            LinearPath lpOuter = new LinearPath(10, 16);
            LinearPath lpInner = new LinearPath(5, 11);
            lpInner.Translate(2.5, 2.5, 0);
            lpInner.Reverse();

            Region reg = new Region(lpOuter, lpInner);

            mesh = reg.SweepAsMesh(composite, .25);
            mesh.Translate(offsetX - 10, offsetY - 8, 0);
            model.Entities.Add(mesh, Color.GreenYellow);
        }

        public static void TerrainTriangulation(Model model)
        {

            MainWindow.SetBackgroundStyleAndColor(model);

            int sideCount = 100;

            int len = sideCount * sideCount;

            Point3D[] pts = new Point3D[len];

            Random rand = new Random(3);

            for (int j = 0; j < sideCount; j++)
            {
                for (int i = 0; i < sideCount; i++)
                {

                    double x = rand.NextDouble() * sideCount;
                    double y = rand.NextDouble() * sideCount;
                    double z = 0;

                    double _x = x / 2 - 15;
                    double _y = y / 2 - 15;

                    double den = Math.Sqrt(_x * _x + _y * _y);

                    if (den != 0)

                        z = 10 * Math.Sin(Math.Sqrt(_x * _x + _y * _y)) / den;

                    int R = (int)(255 * (z + 2) / 12);
                    int B = (int)(2.55 * y);

                    Utility.LimitRange<int>(0, ref R, 255);
                    Utility.LimitRange<int>(0, ref B, 255);

                    PointRGB pt = new PointRGB(x, y, z, (byte)R, 255, (byte)B);

                    pts[i + j * sideCount] = pt;

                }
            }

            Mesh m = UtilityEx.Triangulate(pts);

            model.Entities.Add(m);

            Plane pln = new Plane(new Point3D(0,20,20), new Vector3D(20,-30,10));

            PlanarEntity pe = new PlanarEntity(pln, 25);

            model.Entities.Add(pe, Color.Magenta);

            ICurve[] curves = m.Section(pln, 0);

            foreach (Entity ent in curves)
            {
                model.Entities.Add(ent);
            }
        }

        public static void CompositeCurveMeshing(Model model)
        {

            MainWindow.SetDisplayMode(model, displayType.Shaded);

            CompositeCurve outer = new CompositeCurve();

            outer.CurveList.Add(new Line(0, 0, 10, 0));
            outer.CurveList.Add(new Line(10, 0, 10, 6));
            outer.CurveList.Add(new Line(10, 6, 0, 6));
            outer.CurveList.Add(new Line(0, 6, 0, 0));

            CompositeCurve inner1 = new CompositeCurve();

            inner1.CurveList.Add(new Line(2, 2, 6, 2));
            inner1.CurveList.Add(new Line(6, 2, 2, 3));
            inner1.CurveList.Add(new Line(2, 3, 2, 2));

            CompositeCurve inner2 = new CompositeCurve();

            inner2.CurveList.Add(new Circle(8, 4, 0, 1));

            CompositeCurve inner3 = new CompositeCurve();

            inner3.CurveList.Add(new Circle(6, 4, 0, .75));
            
            Region reg = new Region(outer, inner1, inner2, inner3);

            Mesh m = UtilityEx.Triangulate(reg, .15);

            model.Entities.Add(m, Color.Salmon);              
        }
                      
    }

}

