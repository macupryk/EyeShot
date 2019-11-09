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
            // adjusts grid extents ad step
            model1.GetGrid().Max = new Point3D(200, 200);            
            model1.GetGrid().Step = 25;

            int nArrows = 13;
            int arcRadius = 100;

            double arcSpan = 120;

            // adds the arc
            model1.Entities.Add(
                new Arc(
                Point3D.Origin,
                arcRadius,
                Utility.DegToRad(340), Utility.DegToRad(340 + 150)
                ));

            for (int i = 0; i < nArrows; i++)
            {

                // angle in rad
                double radAngle = Utility.DegToRad(arcSpan * i / nArrows);

                // Creates a mesh with the arrow shape
                Mesh m = Mesh.CreateArrow(4, 100 - i * 4, 8, 24, 16, Mesh.natureType.Smooth);

                m.EdgeStyle = Mesh.edgeStyleType.Sharp;

                // Translation transformation
                Translation tra = new Translation(arcRadius * Math.Cos(radAngle), arcRadius * Math.Sin(radAngle), 0);

                // Rotation transformation
                devDept.Geometry.Rotation rot = new devDept.Geometry.Rotation(radAngle, Vector3D.AxisZ);

                // Combines the two
                Transformation combined = tra * rot;

                // applies the transformation to the arrow
                m.TransformBy(combined);

                // adds the arrow to the master entity array                
                model1.Entities.Add(m, System.Drawing.Color.FromArgb(120 + i * 10, 255 - i * 10, 0));                

            }           

            // sets trimetric view            
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport            
            model1.ZoomFit();

            //refresh the model control
            model1.Invalidate();            

            base.OnContentRendered(e);  
        }        
    }
}