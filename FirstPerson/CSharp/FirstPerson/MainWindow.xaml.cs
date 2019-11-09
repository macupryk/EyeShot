////////////////////////////////////////////////////////////////////////
// Derived from the demo Camera2 of dhpoware http://www.dhpoware.com/ //
////////////////////////////////////////////////////////////////////////

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
            
            model1.ViewportBorder.Visible = false;
            model1.ViewportBorder.CornerRadius = 0;

            model1.Camera.Location = new Point3D(0, 0, 1);                                       
        }

        protected override void OnContentRendered(EventArgs e)
        {            
            // Disable shadows because they are not yet supported with multitexturing
            model1.Rendered.ShadowMode = shadowType.None;

            bool multiTexture = false;
            //if (model1.Renderer == rendererType.OpenGL)
            //    multiTexture = model1.OpenglExtensions.Contains("ARB_multitexture");

            Floor floor = new Floor(multiTexture);

            model1.Entities.Add(floor);

            model1.Viewports[0].Navigation.Min = new Point3D(floor.BoxMin.X, floor.BoxMin.Y, 0.1);
            model1.Viewports[0].Navigation.Max = new Point3D(floor.BoxMax.X, floor.BoxMax.Y, 4);
            model1.Viewports[0].Navigation.Acceleration = 0.01;
            model1.Viewports[0].Navigation.Speed = 0.05;
            model1.Viewports[0].Navigation.RotationSpeed = 2;           
            model1.Viewports[0].Navigation.Mode = devDept.Eyeshot.Camera.navigationType.Walk;

            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}