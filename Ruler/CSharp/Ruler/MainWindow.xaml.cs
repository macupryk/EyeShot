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
using devDept.Eyeshot.Entities;
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

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                                
            model1.Camera.ProjectionMode = projectionType.Orthographic;

            // Set the plane of the ruler
            model1.RulerPlaneMode = MyModel.rulerPlaneType.XY;

            // Set the correct orientation
            SetOrientation(model1.RulerPlaneMode);            

            Circle circle = new Circle(Point3D.Origin, 20);            
            model1.Entities.Add(circle, System.Drawing.Color.Red);

            // Ruler is for XY plane, so disable rotation
            model1.Rotate.Enabled = false;

            // Disable the view cube because we don't want the user to change view orientation
            model1.GetViewCubeIcon().Visible = false;
            
            model1.ZoomFit();
            
            model1.Invalidate();  
         
            base.OnContentRendered(e);
        }

        private void SetOrientation(MyModel.rulerPlaneType planeMode)
        {
            switch (planeMode)
            {
                case MyModel.rulerPlaneType.XY:
                    model1.SetView(viewType.Top);
                    break;

                case MyModel.rulerPlaneType.YZ:
                    model1.SetView(viewType.Right);
                    break;

                case MyModel.rulerPlaneType.ZX:
                    model1.SetView(viewType.Front);
                    break;

            }

        }
    }
}