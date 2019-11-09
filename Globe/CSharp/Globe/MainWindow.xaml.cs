using System;
using System.Collections.Generic;
using System.Drawing;
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
            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }


        protected override void OnContentRendered(EventArgs e)
        {
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            model1.Rendered.EnvironmentMapping = false;

            string matName = "Globe mat";

            // defines a new material using a texture
            Material mat = new Material(matName, new Bitmap("../../../../../../dataset/Assets/Textures/EarthMap.jpg"));

            // more accurate texture scaling (optional and slower)
            mat.MinifyingFunction = textureFilteringFunctionType.Linear;

            // adds the material to the viewport's master material collection
            model1.Materials.Add(mat);

            // Creates a new RichMesh sphere with earth radius, slices and stacks
            Mesh rm = Mesh.CreateSphere(6356.75, 100, 50, Mesh.natureType.RichSmooth);

            // assigns the material to all triangles and maps the material texture spherically
            rm.ApplyMaterial(matName, textureMappingType.Spherical, 1, 1);

            // adds the mesh to the viewport's master entities array
            model1.Entities.Add(rm);           

            // fits the model in the viewport
            model1.ZoomFit();

            // hides origin symbol
            model1.GetOriginSymbol().Visible = false;
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}