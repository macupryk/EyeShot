using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using System.IO;

using devDept.Geometry;
using devDept.Eyeshot.Labels;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using Environment = devDept.Eyeshot.Environment;
#if NURBS
using Region = devDept.Eyeshot.Entities.Region;
using devDept.Eyeshot.Translators;

namespace WpfApplication1
{
    partial class Draw
    {

        public static void HairDryer(Model model)
        {

            BuildHairDryer bhd = new BuildHairDryer();
            model.StartWork(bhd);

        }

        private class BuildHairDryer : WorkUnit
        {

            double trimTol = 0.001;
            double filletTol = 0.001;
            double offsetTol = 0.1;
            double offsetAmount = -1;

            int totalSteps = 5;

            List<Entity> whiteEntList = new List<Entity>();
            List<Entity> darkEntList = new List<Entity>();
            List<Entity> offsetEntList = new List<Entity>();

            protected override void DoWork(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs doWorkEventArgs)
            {

                // body -------------------------

                if (!UpdateProgressAndCheckCancelled(1, totalSteps, "Body drawing", worker, doWorkEventArgs))

                    return;

                Surface s1 = DrawBody();

                whiteEntList.Add(s1);

                // body closure
                Arc a5 = new Arc(80, 0, 0, 110, Math.PI, 1.11 * Math.PI);
                Surface s7 = a5.RevolveAsSurface(Math.PI, Math.PI, Vector3D.AxisX, Point3D.Origin)[0];
                whiteEntList.Add(s7);

                // fillet
                Surface[] f1;
                Surface.Fillet(s1, s7, 5, filletTol, true, true, true, true, true, false, out f1);

                whiteEntList.AddRange(f1);

                // handle ------------------------

                if (!UpdateProgressAndCheckCancelled(2, totalSteps, "Handle drawing", worker, doWorkEventArgs))

                    return;

                double len1 = 150;
                double ang1 = -Math.PI / 2.8;

                // back
                Arc a1 = new Arc(Plane.YZ, new Point2D(22, 0), 50, 4 * Math.PI / 5, Math.PI);

                Surface s2 = a1.ExtrudeAsSurface(len1, 0, 0)[0];

                s2.Rotate(ang1, Vector3D.AxisZ);

                whiteEntList.Add(s2);

                // front
                Arc a2 = new Arc(Plane.YZ, new Point2D(-30, 0), 30, 0, Math.PI / 5);
                Arc a3 = new Arc(Plane.YZ, new Point2D(-15, -40), 60, 9 * Math.PI / 20, 12 * Math.PI / 20);

                Arc a4;
                Curve.Fillet(a2, a3, 10, false, false, true, true, out a4);

                CompositeCurve cc1 = new CompositeCurve(a2, a4, a3);

                Surface s3 = cc1.ExtrudeAsSurface(len1, 0, 0)[0];

                s3.Rotate(ang1, Vector3D.AxisZ);

                whiteEntList.Add(s3);

                // bottom
                Line ln3 = new Line(0, -125, 100, -125);

                s2.Regen(0.1);

                Surface s6 = ln3.ExtrudeAsSurface(new Vector3D(0, 0, s2.BoxMax.Z), Utility.DegToRad(2), trimTol)[0];

                whiteEntList.Add(s6);


                // fillets ------------------------------
                if (!UpdateProgressAndCheckCancelled(3, totalSteps, "Computing fillets", worker, doWorkEventArgs))

                    return;

                Surface[] f2, f3, f4;

                // rear fillet
                Surface.Fillet(new Surface[] { s2 }, new Surface[] { s6 }, 10, filletTol, true, true, true, true, true, false, out f2);
                whiteEntList.AddRange(f2);



                // along handle fillet
                Surface.Fillet(new Surface[] { s3 }, new Surface[] { s2, s6, f2[0] }, 5, filletTol, true, true, true, true, true, false, out f3);
                whiteEntList.AddRange(f3);

                // handle-body fillet
                Surface.Fillet(new Surface[] { s1 }, new Surface[] { s3, s2, s6, f2[0], f3[0] }, 10, filletTol, false, false, true, true, true, false, out f4);

                foreach (Surface surface in f4)
                {
                    surface.ReverseU();
                }

                whiteEntList.AddRange(f4);

                // nozzle ------------------------
                Surface s8 = DrawNozzle(trimTol);

                darkEntList.Add(s8);

                // offset ------------------------

                if (!UpdateProgressAndCheckCancelled(4, totalSteps, "Computing offset", worker, doWorkEventArgs))

                    return;

                offsetEntList.AddRange(OffsetSurfaces(offsetAmount, offsetTol, whiteEntList));
                offsetEntList.AddRange(OffsetSurfaces(offsetAmount, offsetTol, darkEntList));

                // mirror ------------------------

                if (!UpdateProgressAndCheckCancelled(5, totalSteps, "Computing mirror", worker, doWorkEventArgs))

                    return;

                Mirror m = new Mirror(Plane.XY);

                MirrorEntities(m, whiteEntList);

                if (Cancelled(worker, doWorkEventArgs))
                    return;

                MirrorEntities(m, darkEntList);

                if (Cancelled(worker, doWorkEventArgs))
                    return;

                MirrorEntities(m, offsetEntList);

                if (Cancelled(worker, doWorkEventArgs))
                    return;
            }

            private static Entity[] OffsetSurfaces(double amount, double tol, IList<Entity> whiteEntList)
            {
                List<Entity> offSurf = new List<Entity>();

                for (int i = 0; i < whiteEntList.Count; i++)
                {
                    Entity entity = whiteEntList[i];
                    if (entity is Surface)
                    {
                        Surface surf = (Surface)entity;

                        Surface offset;
                        if (surf.Offset(amount, tol, out offset))

                            offSurf.Add(offset);
                    }
                }

                return offSurf.ToArray();
            }

            private static void MirrorEntities(Mirror m, IList<Entity> entList)
            {
                int count = entList.Count;

                for (int i = 0; i < count; i++)
                {
                    Entity entity = entList[i];

                    if (entity is Surface)
                    {
                        Surface copy = (Surface)entity.Clone();

                        copy.TransformBy(m);

                        entList.Add(copy);
                    }
                }
            }

            private static Surface DrawNozzle(double trimTol)
            {
                Arc a1 = new Arc(Plane.YZ, Point2D.Origin, 30, 0, Math.PI);
                a1.Translate(81, 0, 0);
                Curve a2 = new Arc(Plane.YZ, Point2D.Origin, 27, 0, Math.PI).GetNurbsForm();
                a2.Scale(1, 1, 1.15);
                a2.Translate(81 + 10, 0, 0);
                Curve a3 = new Arc(Plane.YZ, Point2D.Origin, 34, 0, Math.PI).GetNurbsForm();
                a3.Scale(1, .5, 1);
                a3.Translate(81 + 30, 0, 0);
                Curve a4 = new Arc(Plane.YZ, Point2D.Origin, 34, 0, Math.PI).GetNurbsForm();
                a4.Scale(1, .5, 1);
                a4.Translate(81 + 40, 0, 0);

                Surface s1 = Surface.Loft(new ICurve[] { a1, a2, a3, a4 }, 3)[0];

                s1.ReverseU();

                Arc a5 = new Arc(Plane.ZX, Point2D.Origin, 120, 0, Math.PI);
                a5.Translate(0, -20, 0);

                Surface s2 = a5.ExtrudeAsSurface(0, 40, 0)[0];

                s1.TrimBy(s2, trimTol, false);

                return s1;
            }

            private static Surface DrawBody()
            {
                // simple
                Line ln1 = new Line(-30, 32, 80, 32);

                Surface s1 = ln1.RevolveAsSurface(0, Math.PI, Vector3D.AxisX, Point3D.Origin)[0];

                // advanced
                Arc a1 = new Arc(Plane.YZ, Point2D.Origin, 32, 0, Math.PI);
                a1.Translate(-30, 0, 0);
                Arc a2 = new Arc(Plane.YZ, Point2D.Origin, 35, 0, Math.PI);
                a2.Translate(-20, 0, 0);
                Arc a3 = new Arc(Plane.YZ, Point2D.Origin, 36, 0, Math.PI);
                a3.Translate(30, 0, 0);
                Arc a4 = new Arc(Plane.YZ, Point2D.Origin, 30, 0, Math.PI);
                a4.Translate(80, 0, 0);

                s1 = Surface.Loft(new ICurve[] { a1, a2, a3, a4 }, 3)[0];

                s1.ReverseU();

                return s1;

            }

            protected override void WorkCompleted(Environment model)
            {
                model.Entities.AddRange(whiteEntList, "Default", Color.WhiteSmoke);
                model.Entities.AddRange(darkEntList, "Default", Color.FromArgb(31, 31, 31));
                model.Entities.AddRange(offsetEntList, "Default", Color.DarkGray);
                model.SetView(viewType.Trimetric);
                model.ZoomFit();
            }
        }        
        
        public static void Toolpath(Model model)
        {
            MainWindow.SetDisplayMode(model, displayType.Shaded);

            #region Surface construction

            List<Point3D> pointList = new List<Point3D>();

            pointList.Add(new Point3D(0, 60, 0));
            pointList.Add(new Point3D(0, 40, +10));
            pointList.Add(new Point3D(0, 20, -5));
            pointList.Add(new Point3D(0, 0, 0));

            Curve first = Curve.GlobalInterpolation(pointList, 3);

            pointList.Clear();
            pointList.Add(new Point3D(40, 55, 0));
            pointList.Add(new Point3D(40, 30, 25));
            pointList.Add(new Point3D(40, 5, 10));

            Curve second = Curve.GlobalInterpolation(pointList, 2);

            pointList.Clear();
            pointList.Add(new Point3D(80, 60, 0));
            pointList.Add(new Point3D(80, 30, 20));
            pointList.Add(new Point3D(80, 0, -10));

            Curve third = Curve.GlobalInterpolation(pointList, 2);

            Surface loft = Surface.Loft(new Curve[] { first, second, third }, 2)[0];

            // flips surface direction
            loft.ReverseU();

#endregion

            // Coarsenes surface tessellation for faster semi-tranparent pre-processing
            loft.Regen(.25);

            model.Entities.Add(loft, Color.FromArgb(200, Color.OrangeRed));

            model.ZoomFit();

            BuildToolpath btp = new BuildToolpath(loft, 0.01, 5);
            model.StartWork(btp);

        }

        private class BuildToolpath : WorkUnit
        {

            private Surface surface;
            private double tolerance;
            private double ballToolRadius;
            private LinearPath toolPath;

            public BuildToolpath(Surface surf, double tol, double toolRadius)
            {
                surface = surf;
                tolerance = tol;
                ballToolRadius = toolRadius;
            }

            protected override void DoWork(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs doWorkEventArgs)
            {

                const int passCount = 50;

                if (!UpdateProgressAndCheckCancelled(100, 100, "Triangulating surface 1/1", worker, doWorkEventArgs))

                    return;

                Mesh m = surface.ConvertToMesh(tolerance);

                // The plane used to slice the surface
                Plane pln = Plane.YZ;

                pln.Rotate(-Math.PI / 4, Vector3D.AxisZ);

                pln.Translate(0, 60, 0);

                List<Point3D> pointList = new List<Point3D>();

                for (int i = 0; i < passCount; i++)
                {

                    pln = pln.Offset(2);

                    ICurve[] sectionCurves = m.Section(pln, 0);

                    if (sectionCurves.Length > 0)
                    {

                        LinearPath pass = (LinearPath)sectionCurves[0];

                        ICurve[] offsetLp = pass.QuickOffset(ballToolRadius, pln);

                        if (offsetLp != null)
                        {

                            if (i % 2 == 1)

                                offsetLp[0].Reverse();

                            pointList.AddRange(((Entity)offsetLp[0]).Vertices);

                        }

                    }

                    if (!UpdateProgressAndCheckCancelled(i, passCount, "Computing passes", worker, doWorkEventArgs))

                        return;

                }

                // raises approach and retract
                Point3D approach = (Point3D)pointList[0].Clone();

                approach.Z += 20;

                pointList.Insert(0, approach);

                Point3D retract = (Point3D)pointList[pointList.Count - 1].Clone();

                retract.Z += 40;

                pointList.Add(retract);

                // return the toolpath as a LinearPath entity
                toolPath = new LinearPath(pointList);

            }

            protected override void WorkCompleted(Environment model)
            {
                model.Entities.Add(toolPath, "Default", Color.DarkBlue);

                #region Tool symbol definition

                Block b1 = new Block("ballTool");

                Circle c1 = new Circle(0, 0, 0, ballToolRadius);
                Circle c2 = new Circle(0, 0, 50, ballToolRadius);
                Arc a1 = new Arc(0, 0, 0, ballToolRadius, Math.PI, 2 * Math.PI);
                a1.Rotate(Math.PI / 2, Vector3D.AxisX);
                Arc a2 = (Arc)a1.Clone();
                a2.Rotate(Math.PI / 2, Vector3D.AxisZ);

                Line l1 = new Line(-ballToolRadius, 0, 0, -ballToolRadius, 0, 50);

                b1.Entities.Add(c1);
                b1.Entities.Add(c2);
                b1.Entities.Add(a1);
                b1.Entities.Add(a2);
                b1.Entities.Add(l1);

                LinearPath lp1 = LinearPath.CreateHelix(ballToolRadius, 50, 1, false, .1);
                b1.Entities.Add(lp1);

                b1.Entities.Add(lp1);
                for (int i = 1; i < 4; i++)
                {
                    Line cloneLn = (Line)l1.Clone();
                    cloneLn.Rotate(i * Math.PI / 2, Vector3D.AxisZ);
                    b1.Entities.Add(cloneLn);

                }

                model.Blocks.Add(b1);

#endregion

                // Adds a reference to the tool symbol
                model.Entities.Add(new BlockReference(toolPath.Vertices[toolPath.Vertices.Length - 1], "ballTool", 1, 1, 1, 0));

                model.ZoomFit();

            }

        }

        public static void Flange(Model model)
        {
            CompositeCurve cc1 = new CompositeCurve(
                new Line(Plane.XZ, 15, 40, 29, 40),
                new Arc(Plane.XZ, new Point2D(29, 39), 1, 0, Utility.DegToRad(90)),
                new Line(Plane.XZ, 30, 39, 30, 16),
                new Arc(Plane.XZ, new Point2D(36, 16), 6, Math.PI, Utility.DegToRad(270)),
                new Line(Plane.XZ, 36, 10, 79, 10),
                new Arc(Plane.XZ, new Point2D(79, 9), 1, 0, Utility.DegToRad(90)),
                new Line(Plane.XZ, 80, 9, 80, 6),
                new Arc(Plane.XZ, new Point2D(86, 6), 6, Utility.DegToRad(180), Utility.DegToRad(270)),
                new Line(Plane.XZ, 86, 0, 130, 0));

            Region reg = cc1.OffsetToRegion(5, 0, false);

            Brep rev1 = reg.RevolveAsBrep(Math.PI * 2, Vector3D.AxisZ, Point3D.Origin);

            model.Entities.Add(rev1, System.Drawing.Color.Aqua);

            Region cssr1 = Region.CreateCircularSlot(0, Utility.DegToRad(30), 60, 8);

            rev1.ExtrudeRemovePattern(cssr1, new Interval(0, 50), Point3D.Origin, Utility.DegToRad(360) / 3, 3);

            Region rr1 = Region.CreateRectangle(90, -40, 50, 80);

            rev1.ExtrudeRemovePattern(rr1, new Interval(0, 50), Point3D.Origin, Utility.DegToRad(360) / 2, 2);

            Region cr1 = Region.CreateCircle(110, 0, 10);

            const int numHoles = 8;

            rev1.ExtrudeRemovePattern(cr1, 50, Point3D.Origin, Utility.DegToRad(360) / numHoles, numHoles);

            model.Entities.Regen();
        }

        public static void Bracket(Model model)
        {
            CompositeCurve rrscc1 = CompositeCurve.CreateRoundedRectangle(Plane.YZ, 40, 120, 12, true);

            CompositeCurve sscc1 = CompositeCurve.CreateSlot(Plane.YZ, 9, 5.25, true);

            sscc1.Translate(0, 0, 43);

            CompositeCurve sscc2 = CompositeCurve.CreateSlot(Plane.YZ, 9, 5.25, true);

            sscc2.Rotate(Utility.DegToRad(90), Vector3D.AxisX, Point3D.Origin);

            sscc2.Translate(0, 0, -40);

            Circle c1 = new Circle(Plane.YZ, 4.25);

            Region r1 = new Region(rrscc1, sscc1, sscc2, c1);

            Brep ext1 = r1.ExtrudeAsBrep(-4);

            model.Entities.Add(ext1, "Default", Color.YellowGreen);

            CompositeCurve cc1 = new CompositeCurve(
                new Line(Plane.YZ, 8, -10, 11, -10),
                new Arc(Plane.YZ, new Point2D(11, -5), 5, Utility.DegToRad(270), Utility.DegToRad(360)),
                new Line(Plane.YZ, 16, -5, 16, +5),
                new Arc(Plane.YZ, new Point2D(11, +5), 5, Utility.DegToRad(0), Utility.DegToRad(90)),
                new Line(Plane.YZ, 11, 10, -11, 10),
                new Arc(Plane.YZ, new Point2D(-11, +5), 5, Utility.DegToRad(90), Utility.DegToRad(180)),
                new Line(Plane.YZ, -16, +5, -16, -5),
                new Arc(Plane.YZ, new Point2D(-11, -5), 5, Utility.DegToRad(180), Utility.DegToRad(270)),
                new Line(Plane.YZ, -11, -10, -8, -10));

            Region r2 = cc1.OffsetToRegion(-2.5, 0, false);

            ext1.ExtrudeAdd(r2, 275);

            Region ssr2 = Region.CreateSlot(Plane.XY, 12, 5.25);

            ssr2.Translate(9, 0, 0);

            ext1.ExtrudeRemovePattern(ssr2, 10, 35, 8, 0, 1);

            model.Entities.Regen();
        }
    }
}


#endif