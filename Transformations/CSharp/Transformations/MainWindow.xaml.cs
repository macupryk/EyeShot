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
using System.Drawing;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
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

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {
 	 

            #region Black wire cube

            model1.Entities.Add(new Line(0, 0, 0, 0, 0, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 0, 0, 100, 0, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 100, 0, 100, 100, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(0, 100, 0, 0, 100, 100), System.Drawing.Color.Black);

            model1.Entities.Add(new Line(0, 0, 0, 100, 0, 0), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 0, 0, 100, 100, 0), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 100, 0, 0, 100, 0), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(0, 100, 0, 0, 0, 0), System.Drawing.Color.Black);

            model1.Entities.Add(new Line(0, 0, 100, 100, 0, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 0, 100, 100, 100, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(100, 100, 100, 0, 100, 100), System.Drawing.Color.Black);
            model1.Entities.Add(new Line(0, 100, 100, 0, 0, 100), System.Drawing.Color.Black);

            #endregion

            #region Front side (with the help of MoveToPlane() method)

            Circle c1 = new Circle(50, 50, 0, 40);
            Text t1 = new Text(50, 50, 0, "Front side", 18);

            t1.Rotate(Math.PI / 4, Vector3D.AxisZ, new Point3D(50, 50, 0));
            t1.Alignment = Text.alignmentType.MiddleCenter;

            List<Entity> myList = new List<Entity>();

            myList.Add(c1);
            myList.Add(t1);

            model1.MoveToPlane(myList, new Plane(Point3D.Origin, new Point3D(100, 0, 0), new Point3D(0, 0, 100)));

            model1.Entities.AddRange(myList, System.Drawing.Color.DarkViolet);

            #endregion

            #region Right side (using sketch plane)

            Plane sketchPlane = new Plane(new Point3D(100, 0, 0), new Point3D(100, 100, 0), new Point3D(100, 0, 100));

            Circle c2 = new Circle(sketchPlane, new Point2D(50, 50), 40);
            Arc a2 = new Arc(sketchPlane, new Point2D(50, 50), 45, Math.PI / 2, 2 * Math.PI);
            Text t2 = new Text(sketchPlane, new Point2D(50, 50), "Right side", 18, Text.alignmentType.MiddleCenter);

            t2.Rotate(Math.PI / 4, Vector3D.AxisX, new Point3D(0, 50, 50));

            model1.Entities.Add(c2, System.Drawing.Color.Red);
            model1.Entities.Add(a2, System.Drawing.Color.Red);
            model1.Entities.Add(t2, System.Drawing.Color.Red);

            #endregion

            #region Rear side (with the help of Entity.Translate() and Rotate() methods)

            Circle c3 = new Circle(Point3D.Origin, 40);

            c3.Translate(50, 50, -100);
            c3.Rotate(Math.PI / 2, Vector3D.AxisX);

            model1.Entities.Add(c3, System.Drawing.Color.Blue);

            Text t3 = new Text(0, 0, 0, "Rear side", 18);

            t3.Alignment = Text.alignmentType.MiddleCenter;

            t3.Rotate(Math.PI / 2, Vector3D.AxisX);
            t3.Rotate(Math.PI, Vector3D.AxisZ);
            t3.Rotate(Math.PI / 4, Vector3D.AxisY);
            t3.Translate(50, 100, 50);

            model1.Entities.Add(t3, System.Drawing.Color.Blue);

            #endregion

            #region Left side (with the help of the Transformation class)

            Transformation frame = new Transformation(new Point3D(0, 50, 50), new Vector3D(0, -1, 0), Vector3D.AxisZ, new Vector3D(-1, 0, 0));

            Circle c4 = new Circle(0, 0, 0, 40);

            c4.TransformBy(frame);

            model1.Entities.Add(c4, System.Drawing.Color.Green);

            Text t4 = new Text(0, 0, 0, "Left side", 18);

            t4.Alignment = Text.alignmentType.MiddleCenter;

            t4.Rotate(Math.PI / 4, Vector3D.AxisZ);

            t4.TransformBy(frame);

            model1.Entities.Add(t4, System.Drawing.Color.Green);

            #endregion

            // set trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}