using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Labels;
using devDept.Geometry;
using devDept.Graphics;

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

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                                
            model1.AutoHideLabels = true;

            model1.GetGrid().AutoSize = true;
            model1.GetGrid().Step = 100;

            string concreteMatName = "Concrete";
            string steelMatName = "Blue steel";
            
            model1.Materials.Add(new Material(concreteMatName, System.Drawing.Color.FromArgb(25, 25, 25), System.Drawing.Color.LightGray, System.Drawing.Color.FromArgb(31, 31, 31), .05f, 0));
            model1.Materials.Add(new Material(steelMatName, System.Drawing.Color.RoyalBlue));

            // square column block
            Block b = new Block("squareCol");

            // creates a gray box
            Mesh m1 = Mesh.CreateBox(30, 30, 270);

            m1.ColorMethod = colorMethodType.byEntity;            
            m1.Color = System.Drawing.Color.Gray;
            m1.MaterialName = concreteMatName;

            // adds it to the block
            b.Entities.Add(m1);

            // creates a new black polyline
            LinearPath steel = new LinearPath();

            steel.Vertices = new Point3D[8];
            steel.Vertices[0] = new Point3D(4, 4, -20);
            steel.Vertices[1] = new Point3D(4, 4, 290);
            steel.Vertices[2] = new Point3D(4, 26, 290);
            steel.Vertices[3] = new Point3D(4, 26, -20);
            steel.Vertices[4] = new Point3D(26, 26, -20);
            steel.Vertices[5] = new Point3D(26, 26, 290);
            steel.Vertices[6] = new Point3D(26, 4, 290);
            steel.Vertices[7] = new Point3D(26, 4, -20);

            steel.ColorMethod = colorMethodType.byEntity;            
            steel.Color = System.Drawing.Color.Black;

            // adds it to the block
            b.Entities.Add(steel);

            // creates a price tag
            devDept.Eyeshot.Entities.Attribute at = new devDept.Eyeshot.Entities.Attribute(new Point3D(0, -15), "Price", "$25,000", 10);

            // adds it to the block
            b.Entities.Add(at);

            // adds the block to the master block dictionary
            model1.Blocks.Add(b);

            // inserts the "SquareCol" block many times: this is a HUGE memory and graphic resources saving for big models
            BlockReference reference;

            for (int k = 0; k < 4; k++)

                for (int j = 0; j < 5; j++)

                    for (int i = 0; i < 5; i++)

                        if (i < 2 && j < 2)

                            System.Diagnostics.Debug.WriteLine("No columns here");

                        else
                        {

                            reference = new BlockReference(i * 500, j * 400, k * 300, "squareCol", 1, 1, 1, 0);

                            // defines a different price for each one
                            reference.Attributes.Add("Price", "$" + i + ",000");

                            model1.Entities.Add(reference);

                        }

            // again as above
            b = new Block("floor");

            double width = 2030;
            double depth = 1630;
            double dimA = 1000;
            double dimB = 800;

            Point2D[] outerPoints = new Point2D[7];

            outerPoints[0] = new Point2D(0, dimB);
            outerPoints[1] = new Point2D(dimA, dimB);
            outerPoints[2] = new Point2D(dimA, 0);
            outerPoints[3] = new Point2D(width, 0);
            outerPoints[4] = new Point2D(width, depth);
            outerPoints[5] = new Point2D(0, depth);
            outerPoints[6] = (Point2D)outerPoints[0].Clone();

            LinearPath outer = new LinearPath(Plane.XY, outerPoints);

            Point2D[] innerPoints = new Point2D[5];

            innerPoints[0] = new Point2D(1530, 800);
            innerPoints[1] = new Point2D(1530, 950);
            innerPoints[2] = new Point2D(1650, 950);
            innerPoints[3] = new Point2D(1650, 800);
            innerPoints[4] = (Point2D)innerPoints[0].Clone();

            LinearPath inner = new LinearPath(Plane.XY, innerPoints);

            devDept.Eyeshot.Entities.Region reg = new devDept.Eyeshot.Entities.Region(outer, inner);

            Mesh m2 = reg.ExtrudeAsMesh(30, 0.1, Mesh.natureType.Plain);

            m2.ColorMethod = colorMethodType.byEntity;            
            m2.Color = System.Drawing.Color.White;
            m2.MaterialName = concreteMatName;

            b.Entities.Add(m2);

            model1.Blocks.Add(b);

            for (int i = 0; i < 4; i++)
            {

                reference = new BlockReference(0, 0, 270 + i * 300, "floor", 1, 1, 1, 0);

                model1.Entities.Add(reference);

            }

            string brickMatName = "Wall bricks";

            model1.Materials.Add(new Material(brickMatName, new Bitmap("../../../../../../dataset/Assets/Textures/Bricks.jpg")));

            b = new Block("brickWall");

            Mesh rm = Mesh.CreateBox(470, 30, 270, Mesh.natureType.RichPlain);

            rm.ApplyMaterial(brickMatName, textureMappingType.Cubic, 1.5, 1.5);

            rm.ColorMethod = colorMethodType.byEntity;            
            rm.Color = System.Drawing.Color.Chocolate;

            b.Entities.Add(rm);

            model1.Blocks.Add(b);

            for (int j = 1; j < 4; j++)

                for (int i = 0; i < 2; i++)
                {

                    reference = new BlockReference(1030 + i * 500, 0, j * 300, "brickWall", 1, 1, 1, 0);

                    model1.Entities.Add(reference);

                }


            // Cylindrical column
            b = new Block("CylindricalCol");

            Mesh m3 = Mesh.CreateCylinder(20, 270, 32, Mesh.natureType.Smooth);

            m3.ColorMethod = colorMethodType.byEntity;            
            m3.Color = System.Drawing.Color.RoyalBlue;
            m3.MaterialName = steelMatName;

            b.Entities.Add(m3);

            model1.Blocks.Add(b);

            for (int j = 0; j < 2; j++)

                for (int i = 0; i < 3; i++)
                {

                    reference = new BlockReference(100 + i * 400, 115 + j * 200, 0, "CylindricalCol", 1, 1, 1, 0);

                    model1.Entities.Add(reference);

                }

            // Roof (not a block this time)
            Mesh roof = Mesh.CreateBox(880, 280, 30);

            // Edits vertices to add a slope
            roof.Vertices[4].Z = 15;
            roof.Vertices[7].Z = 15;

            roof.Translate(60, 75, 270);
            
            model1.Entities.Add(roof, System.Drawing.Color.DimGray);


            // Labels            
            LeaderAndText lat = new LeaderAndText(new Point3D(0, 800, 1200),
                                  "Height = 12 m", new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.White, new Vector2D(0, 20));

            lat.FillColor = System.Drawing.Color.Black;
            model1.Labels.Add(lat);


            LeaderAndText ff;
            
            ff = new LeaderAndText(new Point3D(1000, 1000, 300),
                     "First floor", new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.White, new Vector2D(0, 10));

            ff.FillColor = System.Drawing.Color.Red;
            ff.Alignment = System.Drawing.ContentAlignment.BottomCenter;

            model1.Labels.Add(ff);

            // fits the model in the viewport            
            model1.ZoomFit();

            // refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}