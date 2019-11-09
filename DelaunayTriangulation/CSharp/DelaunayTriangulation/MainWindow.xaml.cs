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
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Triangulation;
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
            // defines and add a circle
            Circle c1 = new Circle(0, 0, 0, 8);

            // regen with our own tolerance
            c1.Regen(0.05);

            // defines and adds a rect
            LinearPath r1 = new LinearPath(3, 3);

            r1.Translate(1, -5, 0);


            // creates an array of points ...
            Point3D[] points = new Point3D[100];

            // ... and fills it
            for (int y = 0; y < 10; y++)

                for (int x = 0; x < 10; x++)
                {

                    Point3D p = new Point3D(x, y, 0);

                    points[x + y * 10] = p;

                    // adds the point also to the master entity array                    
                    model1.Entities.Add(new devDept.Eyeshot.Entities.Point(p), System.Drawing.Color.Black);

                }

            // creates an internal constraint
            Arc a1 = new Arc(0, 0, 0, 5, Utility.DegToRad(120), Utility.DegToRad(220));

            a1.Regen(0.05);

            List<Segment3D> segments = new List<Segment3D>();

            for (int i = 0; i < a1.Vertices.Length -1; i++)
            {
                segments.Add(new Segment3D(a1.Vertices[i], a1.Vertices[i+1]));
            }

            // computes triangulation and fill the Mesh entity
            Mesh m = UtilityEx.Triangulate(c1.Vertices, new Point3D[][] { r1.Vertices }, points, segments);

            model1.Entities.Add(c1, System.Drawing.Color.Red);
            model1.Entities.Add(r1, System.Drawing.Color.Blue);
            model1.Entities.Add(a1, System.Drawing.Color.Green);

            m.EdgeStyle = Mesh.edgeStyleType.Free;

            // moves the mesh up
            m.Translate(0, 0, 5);

            // adds the mesh to the master entity array            
            model1.Entities.Add(m, System.Drawing.Color.RoyalBlue);

            // sets the shaded display mode
            model1.DisplayMode = displayType.Shaded;

            // fits the model in the viewport
            model1.ZoomFit();

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // hides origin symbol
            model1.GetOriginSymbol().Visible = false;

            //refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}