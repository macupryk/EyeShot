using System;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using System.IO;

using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot.Labels;
using devDept.Graphics;

using Region = devDept.Eyeshot.Entities.Region;

#if SOLID

namespace WpfApplication1
{
    partial class Draw
    {
        public static void Medal(Model model)
        {

            // materials
            Material alu = Material.Aluminium;

            alu.Diffuse = Color.White;
            alu.Environment = 0.4f;

            string medalMatName = "Alu";

            alu.Name = medalMatName;

            model.Materials.Add(alu);


            string woodMatName = "Wood";

            Material wood = new Material(woodMatName, new Bitmap(MainWindow.GetAssetsPath() + "Textures/Wenge.jpg"));

            model.Materials.Add(wood);



            // medal 
            Solid sphere = Solid.CreateSphere(200, 120, 60);

            sphere.Rotate(Math.PI / 2, Vector3D.AxisY);

            sphere.Translate(0, 0, -190);

            Solid cylinder = Region.CreateCircle(Plane.XY, 0, 0, 50).ExtrudeAsSolid(100, 0.1);

            Solid[] intersection = Solid.Intersection(sphere, cylinder);

            Solid lens = intersection[0];

            Text eyeshotText = new Text(-45.5, -8, 0, "eyeshot", 19);

            eyeshotText.Translate(0, 0, 2);

            List<Solid> solidItems = new List<Solid>();

            solidItems.Add(lens);

            solidItems.AddRange(model.ExtrudeText(eyeshotText, 0.01, new Vector3D(0, 0, 10), true));
            
            Solid medal = Solid.Union(solidItems.ToArray())[0];

            medal.ColorMethod = colorMethodType.byEntity;
            medal.MaterialName = "alu";
            medal.Translate(0, 0, 2);

            model.Entities.Add(medal, Color.White);


            // jewel case
            Solid b1 = Solid.CreateBox(140, 140, 12);
            b1.Translate(-70, -70, 0);

            Solid b2 = Solid.CreateBox(108, 108, 12);
            b2.Translate(-54, -54, 2);

            Solid[] diff1 = Solid.Difference(b1, b2);

            Plane pln = Plane.YZ;

            Line ln1 = new Line(pln, 0, 0, 4, 0);
            Line ln2 = new Line(pln, 4, 0, 4, 4);
            Line ln3 = new Line(pln, 4, 4, 8, 4);
            Arc a1 = new Arc(pln, new Point2D(12, 4), 4, Math.PI / 2, Math.PI);
            Line ln4 = new Line(pln, 12, 8, 12, 12);
            Line ln5 = new Line(pln, 12, 12, 0, 12);
            Line ln6 = new Line(pln, 0, 12, 0, 0);

            CompositeCurve sect = new CompositeCurve(ln1, ln2, ln3, a1, ln4, ln5, ln6);

            sect.Translate(0, -70, 0);

            devDept.Eyeshot.Entities.Region sectReg = new devDept.Eyeshot.Entities.Region(sect);

            LinearPath rail = new LinearPath(new Point3D[] { new Point3D(0, -70, 0), new Point3D(70, -70, 0), new Point3D(70, +70, 0), new Point3D(-70, +70, 0), new Point3D(-70, -70, 0), new Point3D(0, -70, 0) });

            Solid frame = sectReg.SweepAsSolid(rail, 0.1);

            Solid[] diff2 = Solid.Difference(diff1[0], frame);

            Solid jewelCase = diff2[0];

            jewelCase.ApplyMaterial(woodMatName, textureMappingType.Cubic, 1, 1);

            model.Entities.Add(jewelCase, Color.FromArgb(32, 0, 0));

        }

        public static void House(Model model)
        {
            Region outer = Region.CreatePolygon(new Point3D[]
                                  {
                                      new Point3D(0, 0),
                                      new Point3D(460, 0),
                                      new Point3D(460, 100),
                                      new Point3D(600, 100),
                                      new Point3D(600, 400),
                                      new Point3D(0, 400)
                                  });

            // House's extruded outer profile
            Solid body = outer.ExtrudeAsSolid(400, 0);

            // Big room at origin
            Solid bigRoom = Solid.CreateBox(400, 340, 400);

            // Moves big room in place
            bigRoom.Translate(30, 30, 0);

            // Cuts the big room from the house's body
            Solid[] firstCut = Solid.Difference(body, bigRoom);

            // Small room
            Solid smallRoom = Solid.CreateBox(130, 240, 400);

            // Moves small room in place
            smallRoom.Translate(440, 130, 0);

            // Cuts the small room from the house's body
            Solid[] secondCut = Solid.Difference(firstCut[0], smallRoom);

            // Draws the main door profile on a vertical plane
            Plane pln = new Plane(new Point3D(100, 40, 0), Vector3D.AxisX, Vector3D.AxisZ);

            Line l1 = new Line(pln, 0, 180, 0, 0);
            Line l2 = new Line(pln, 0, 0, 120, 0);
            Line l3 = new Line(pln, 120, 0, 120, 180);
            Arc a1 = new Arc(pln, new Point2D(60, 155), new Point2D(120, 180), new Point2D(0, 180));

            devDept.Eyeshot.Entities.Region reg = new devDept.Eyeshot.Entities.Region(new CompositeCurve(l1, l2, l3, a1));

            // Cuts the main door profile from the house's body
            secondCut[0].ExtrudeRemove(reg, 50, 1);

            // central horizontal beam
            Solid beam1 = Solid.CreateBox(680, 30, 40);

            // moves in place
            beam1.Translate(-40, 185, 360);

            // cut the house's body
            Solid[] thirdCut = Solid.Difference(secondCut[0], beam1);

            // same for other two horizontal beams
            Solid beam2 = Solid.CreateBox(680, 20, 40);

            beam2.Translate(-40, 0, 280);

            Solid[] fourthCut = Solid.Difference(thirdCut[0], beam2);

            Solid beam3 = Solid.CreateBox(680, 20, 40);

            beam3.Translate(-40, 380, 280);

            Solid[] fifthCut = Solid.Difference(fourthCut[0], beam3);

            // Intersection tool loop
            outer = Region.CreatePolygon(Plane.YZ, new Point2D[]
                                                                    {
                                                                        new Point2D(0, 0),
                                                                        new Point2D(400, 0),
                                                                        new Point2D(400, 300),
                                                                        new Point2D(200, 400),
                                                                        new Point2D(0, 300)
                                                                    });

            // Tool body
            Solid intersectionTool = outer.ExtrudeAsSolid(Vector3D.AxisX * 680, 0);

            // Moves the tool in place
            intersectionTool.Translate(-40, 0, 0);

            // Intersects the house's body with the tool
            Solid[] firstInters = Solid.Intersection(fifthCut[0], intersectionTool);

            // Intersects the horizontal beams with the tool
            Solid[] secondInters = Solid.Intersection(beam1, intersectionTool);
            Solid[] thirdInters = Solid.Intersection(beam2, intersectionTool);
            Solid[] fourthInters = Solid.Intersection(beam3, intersectionTool);

            // Adds beams to the scene
            model.Entities.AddRange(secondInters, Color.SaddleBrown);
            model.Entities.AddRange(thirdInters, Color.SaddleBrown);
            model.Entities.AddRange(fourthInters, Color.SaddleBrown);

            // Basement sweep rail
            LinearPath rail = new LinearPath(new Point3D[]
                                                 {
                                                     new Point3D(220, 0),
                                                     new Point3D(460, 0),
                                                     new Point3D(460, 100),
                                                     new Point3D(600, 100),
                                                     new Point3D(600, 400),
                                                     new Point3D(0, 400),
                                                     new Point3D(0, 0),
                                                     new Point3D(100, 0)
                                                 });

            // Basement sweep section
            Region section = Region.CreatePolygon(new Point3D[]
                                                    {
                                                        new Point3D(220, 0, 0),
                                                        new Point3D(220, -7.5, 0),
                                                        new Point3D(220, 0, 75)
                                                    });

            // Sweep solid
            Solid basement = section.SweepAsSolid(rail, 0);

            // Merges sweep with the house's body
            Solid[] firstUnion = Solid.Union(firstInters[0], basement);

            // Internal door
            Solid door = Solid.CreateBox(30, 80, 210);

            // Moves internal door in place
            door.Translate(420, 140, 0);

            // Cuts the internal door from the house's body
            Solid[] sixthCut = Solid.Difference(firstUnion[0], door);

            Solid beam10 = Solid.CreateBox(10, 120, 20);
            beam10.Translate(430, 120, 210);
            model.Entities.Add(beam10, Color.Gray);

            Solid[] seventhCut = Solid.Difference(sixthCut[0], beam10);

            // Window
            Solid window = Solid.CreateBox(90, 50, 140);

            // Moves window in place
            window.Translate(280, -10, 90);

            // Cuts the window from the house's body
            Solid[] eighthCut = Solid.Difference(seventhCut[0], window);

            Solid windowLedge = Solid.CreateBox(100, 35, 5);
            windowLedge.Translate(275, -5, 85);
            model.Entities.Add(windowLedge, Color.Gray);

            Solid[] sixthCut3 = Solid.Difference(eighthCut[0], windowLedge);

            sixthCut3[0].SmoothingAngle = Utility.DegToRad(1);

            model.Entities.AddRange(sixthCut3, Color.WhiteSmoke);


            // Oblique beam loop
            Region obliqueLoop = Region.CreatePolygon(Plane.YZ, new Point2D[]
                                                                  {
                                                                      new Point2D(200, 0),
                                                                      new Point2D(-60, -130),
                                                                      new Point2D(-60, -150),
                                                                      new Point2D(200, -20)
                                                                  });

            // Oblique beam
            Solid oblique = obliqueLoop.ExtrudeAsSolid(10, 0);

            // Moves in place
            oblique.Translate(-40, 0, 420);

            // A list of entities we need to mirror
            List<Entity> toBeMirrored = new List<Entity>();

            toBeMirrored.Add(oblique);

            // Copies and adds the oblique beam
            for (int i = 0; i < 7; i++)
            {

                Entity clone = (Entity)oblique.Clone();

                clone.Translate((((680 - 8 * 10) / 7.0) + 10) * (i + 1), 0, 0);

                toBeMirrored.Add(clone);

            }

            // Copies and mirrors
            int count = toBeMirrored.Count;

            Plane mirrorPlane = Plane.ZX;

            mirrorPlane.Origin.Y = 200;

            Mirror m = new Mirror(mirrorPlane);

            for (int i = 0; i < count; i++)
            {
                Entity clone = (Entity)toBeMirrored[i].Clone();

                clone.TransformBy(m);

                toBeMirrored.Add(clone);
            }

            // Adds all the array items to the scene
            model.Entities.AddRange(toBeMirrored, Color.SaddleBrown);


        }        
    }

}

#endif