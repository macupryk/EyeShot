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

        private MarchingCubes mc;

        protected override void OnContentRendered(EventArgs e)
        {                    
            // adjusts viewport grid size and step
            model1.GetGrid().Min = new Point3D(-5, -5);
            model1.GetGrid().Max = new Point3D(+5, +5);
            model1.GetGrid().Step = .5;           

            // declare the function to be used to evaluate the 3D scalar field
            ScalarField3D func = new ScalarField3D(myScalarField);

            // initialize marching cube algorithm
            mc = new MarchingCubes(new Point3D(0, -2.5, 0), 50, .1f, 25, .1f, 25, .1f, func);

            mc.IsoLevel = trackBar1.Value;

            mc.DoWork();

            // iso surface generation
            Mesh isoSurf = mc.Result;

            // adds the surface to the entities collection with Magenta color            
            model1.Entities.Add(isoSurf, System.Drawing.Color.Magenta);

            // updates the iso level label
            isoLevelLabel.Content = "Iso level = " + mc.IsoLevel;
            
            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();
            
            // refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }

        float myScalarField(float x1, float y1, float z1)
        {

            float den1 = x1 * x1 + y1 * y1 + z1 * z1;
            float den2 = (x1 - 3) * (x1 - 3) + y1 * y1 + z1 * z1;

            float part1 = den1 != 0 ? 100 / den1 : 0;
            float part2 = den2 != 0 ? 100 / den2 : 0;

            return part1 + part2;

        }

        private void trackBar1_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mc == null)
                return;
                        
            model1.Entities.Clear();
            while (model1.Entities.Count > 0)            
                model1.Entities.RemoveAt(0);                            

            mc.IsoLevel = trackBar1.Value;

            mc.DoWork();
            
            model1.Entities.Add(mc.Result, System.Drawing.Color.Magenta);
            isoLevelLabel.Content = "Iso level = " + (int)mc.IsoLevel;

            model1.Invalidate();
        }
    }
}