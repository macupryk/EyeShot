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

            //myModel1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {                    
            Mesh cylinder = Mesh.CreateCylinder(10, 20, 36);

            cylinder.Translate(20, 20, 0);
            
            myModel1.Entities.Add(cylinder, System.Drawing.Color.DarkGoldenrod);

            Mesh cone = Mesh.CreateCone(15, 0, 10, 36);
            
            myModel1.Entities.Add(cone, System.Drawing.Color.Khaki);

            Mesh box = Mesh.CreateBox(12, 12, 12);

            box.Translate(-10, 20, 0);
            
            myModel1.Entities.Add(box, System.Drawing.Color.DarkRed);

            // sets trimetric view for main viewport            
            myModel1.Viewports[0].SetView(viewType.Trimetric);
            myModel1.Viewports[0].Grid.Visible = false;
            myModel1.Viewports[0].ZoomFit();

            // sets top view for secondary viewport            
            myModel1.Viewports[1].SetView(viewType.Top);
            myModel1.Viewports[1].DisplayMode = displayType.Shaded;
            myModel1.Viewports[1].Camera.ProjectionMode = projectionType.Orthographic;
            myModel1.Viewports[1].ZoomFit();

            myModel1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}