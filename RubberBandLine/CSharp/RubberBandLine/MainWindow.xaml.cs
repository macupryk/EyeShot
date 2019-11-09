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
            // Adds a beige semi-transparent rectangle
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(100, 0, 0);
            Point3D p3 = new Point3D(100, 80, 40);
            Point3D p4 = new Point3D(0, 80, 40);

            model1.Entities.Add(new Quad(p1, p2, p3, p4), System.Drawing.Color.FromArgb(127, System.Drawing.Color.Beige));

            model1.SetPlane(p1, p2, p4);

            // Sets trimetric view
            model1.SetView(viewType.Trimetric);

            // Fits the model in the viewport
            model1.ZoomFit();
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}