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
            
            #region Frame drawing

            string jointsLabel = "Joints";
            string barsLabel = "Bars";
            model1.Layers.Add(jointsLabel, System.Drawing.Color.Red);
            model1.Layers.Add(barsLabel, System.Drawing.Color.DimGray);

            model1.Entities.Add(new Joint(-40, -20, 0, 2.5, 2), jointsLabel);
            model1.Entities.Add(new Joint(-40, +20, 0, 2.5, 2), jointsLabel);
                                                                         
            model1.Entities.Add(new Joint(+40, -20, 0, 2.5, 2), jointsLabel);
            model1.Entities.Add(new Joint(+40, +20, 0, 2.5, 2), jointsLabel);

            model1.Entities.Add(new Bar(-40, -20, 0, +40, -20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(-40, +20, 0, +40, +20, 0, 1, 8), barsLabel);

            model1.Entities.Add(new Joint(+40, 0, 40, 2.5, 2), jointsLabel);
            model1.Entities.Add(new Joint(-40, 0, 40, 2.5, 2), jointsLabel);

            model1.Entities.Add(new Bar(-40, 0, 40, +40, 0, 40, 1, 8), barsLabel);

            model1.Entities.Add(new Bar(-40, -20, 0, -40, +20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(-40, -20, 0, -40, 0, 40, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(-40, +20, 0, -40, 0, 40, 1, 8), barsLabel);

            model1.Entities.Add(new Bar(+40, -20, 0, +40, +20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+40, -20, 0, +40, 0, 40, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+40, +20, 0, +40, 0, 40, 1, 8), barsLabel);

            model1.Entities.Add(new Bar(-40, -20, 0, +40, +20, 0, 1, 8), barsLabel);

            model1.Entities.Add(new Bar(+40, -20, 0, +120, -20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+40, +20, 0, +120, +20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+40, 0, 40, +120, +20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+40, 0, 40, +120, -20, 0, 1, 8), barsLabel);
            model1.Entities.Add(new Bar(+120, +20, 0, +120, -20, 0, 1, 8), barsLabel);

            model1.Entities.Add(new Bar(+40, +20, 0, +120, -20, 0, 1, 8), barsLabel);

            model1.Entities.Add(new Joint(120, -20, 0, 2.5, 2), jointsLabel);
            model1.Entities.Add(new Joint(120, +20, 0, 2.5, 2), jointsLabel);

            #endregion

            // adds cable layer and entities            
            model1.LineTypes.Add("Dash", new float[] { 2, -1 });
            string cableLayer = "Cable";
            model1.Layers.Add(cableLayer, System.Drawing.Color.Teal, "Dash");

            Plane xz = new Plane(new Point3D(110, 0, -10), Vector3D.AxisX, Vector3D.AxisZ);

            Line l1 = new Line(-60, 0, -10, 120, 0, -10);                  
            Arc a1 = new Arc(xz, new Point3D(120, 0, -15), 5, 0, Math.PI / 2);
            Line l2 = new Line(125, 0, -15, 125, 0, -50);

            model1.Entities.AddRange(new Entity[] { l1, a1, l2 }, cableLayer);

            // adds pulley layer and entities            
            string pulleyLayer = "Pulley";
            model1.Layers.Add(pulleyLayer, System.Drawing.Color.Magenta);

            Circle c1 = new Circle(xz, new Point3D(120, 0, -15), 5);
            Circle c2 = new Circle(xz, new Point3D(120, 0, -15), 7);
            Circle c3 = new Circle(xz, new Point3D(120, 0, -15), 2);

            model1.Entities.AddRange(new Entity[] { c1, c2, c3 }, pulleyLayer);

            // axes on default layer with their own line style
            Line l3 = new Line(110, 0, -15, 130, 0, -15);

            model1.LineTypes.Add("DashDot", new float[] { 5, -1.5f, 0.25f, -1.5f });

            l3.LineTypeMethod = colorMethodType.byEntity;
            l3.LineTypeName = "DashDot";

            Line l4 = new Line(120, 0, -5, 120, 0, -25);
            l4.LineTypeMethod = colorMethodType.byEntity;
            l4.LineTypeName = "DashDot";

            model1.Entities.Add(l3);
            model1.Entities.Add(l4);
            
            model1.ZoomFit();

            model1.Invalidate();
         
            base.OnContentRendered(e);
        }
    }
}