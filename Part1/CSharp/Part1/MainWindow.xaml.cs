using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Labels;
using devDept.Graphics;
using devDept.Geometry;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                        
            model1.GetGrid().AutoSize = true;
            model1.GetGrid().Step = 5;

            // for correct volume calculation, during modeling it is useful to have this = true
            model1.ShowNormals = false;


            double tol = 0.25;
            double holeDia = 10;
            double thickness = 5;

            ICurve hole = new Circle(40, 20, 0, holeDia);
            hole.Reverse();

            // Bottom face
            // to see the model at this point UNComment 
            // the two lines just after this code block
            CompositeCurve baseProfile = new CompositeCurve(new Line(0, 0, 40, 0),
                                                            new Arc(new Point3D(40, 20, 0), 20, 6 * Math.PI / 4, 2 * Math.PI),
                                                            new Line(60, 20, 60, 40),
                                                            new Line(60, 40, 0, 40), new Line(0, 40, 0, 0));

            Region faceRegion = new Region(baseProfile, hole);
            Mesh part1 = faceRegion.ConvertToMesh(tol);

            part1.FlipNormal();

            //model1.Entities.Add(part1);
            //return;

            // Extrudes of some profile entities
            // to see the model at this point UNComment 
            // the two lines just after this code block
            Mesh face;

            for (int i = 1; i < 3; i++)
            {
                face = baseProfile.CurveList[i].ExtrudeAsMesh(0, 0, thickness, tol, Mesh.natureType.Smooth);

                part1.MergeWith(face);
            }

            face = hole.ExtrudeAsMesh(new Vector3D(0, 0, thickness), tol, Mesh.natureType.Smooth);
            
            part1.MergeWith(face);

            //model1.Entities.Add(part1);
            //return;        
            
            // Top face
            // to see the model at this point UNComment 
            // the two lines just after this code block
            baseProfile = new CompositeCurve(new Line(thickness, 0, 40, 0),
                                             new Arc(new Point3D(40, 20, 0), 20, 6 * Math.PI / 4, 2 * Math.PI),
                                             new Line(60, 20, 60, 40),
                                             new Line(60, 40, thickness, 40),
                                             new Line(thickness, 40, thickness, 0));

            faceRegion = new Region(baseProfile, hole);
            face = faceRegion.ConvertToMesh(tol);

            // Translates it to Z = 10
            face.Translate(0, 0, thickness);

            part1.MergeWith(face);

            //model1.Entities.Add(part1);
            //return;


            // Top vertical profile
            // to see the model at this point UNComment 
            // the two lines just after this code block
            LinearPath pl = new LinearPath(4);

            pl.Vertices[0] = new Point3D(thickness, 0, thickness);
            pl.Vertices[1] = new Point3D(thickness, 0, 30);
            pl.Vertices[2] = new Point3D(0, 0, 30);
            pl.Vertices[3] = new Point3D(0, 0, 0);

            face = pl.ExtrudeAsMesh(0, 40, 0, tol, Mesh.natureType.Smooth);

            face.FlipNormal();

            part1.MergeWith(face);

            //model1.Entities.Add(part1);
            //return;


            // Front 'L' shaped face
            // to see the model at this point UNComment 
            // the two lines just after this code block
            Point3D[] frontProfile = new Point3D[7];

            frontProfile[0] = Point3D.Origin;
            frontProfile[1] = new Point3D(40, 0, 0);
            frontProfile[2] = new Point3D(40, 0, thickness);
            frontProfile[3] = new Point3D(thickness, 0, thickness);
            frontProfile[4] = new Point3D(thickness, 0, 30);
            frontProfile[5] = new Point3D(0, 0, 30);
            frontProfile[6] = Point3D.Origin;

            // This profile is in the wrong direction, we use true as last parameter
            face = Mesh.CreatePlanar(frontProfile, Mesh.natureType.Smooth);

            // makes a deep copy of this face
            Mesh rearFace = (Mesh)face.Clone();

            part1.MergeWith(face);

            // model1.Entities.Add(part1);
            // return;


            // Rear 'L' shaped face
            // to see the model at this point UNComment 
            // the two lines just after this code block

            // Translates it to Y = 40
            rearFace.Translate(0, 40, 0);

            // Stretches it
            for (int i = 0; i < rearFace.Vertices.Length; i++)

                if (rearFace.Vertices[i].X > 10)

                    rearFace.Vertices[i].X = 60;

            rearFace.FlipNormal();
            part1.MergeWith(rearFace);

            //model1.Entities.Add(part1);
            //return;


            // Set the normal averaging and edge style mode
            part1.NormalAveragingMode = Mesh.normalAveragingType.AveragedByAngle;
            part1.EdgeStyle = Mesh.edgeStyleType.Sharp;
            
            model1.Layers.Add("Brakets",System.Drawing.Color.Crimson);

            // Adds the mesh to the model1
            model1.Entities.Add(part1, "Brakets");

            VolumeProperties mp = new VolumeProperties(part1.Vertices, part1.Triangles);

            // Adds the volume label            
            model1.Labels.Add(new LeaderAndText(60, 40, thickness, "Volume = " + mp.Volume.ToString("f3") + " cubic " + model1.Units, new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.Black, new Vector2D(0, 50)));

            // fits the model in the model1            
            model1.ZoomFit();

            // refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}