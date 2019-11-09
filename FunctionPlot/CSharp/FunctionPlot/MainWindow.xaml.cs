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
            model1.GetGrid().AutoSize = true;
            model1.GetGrid().Step = 1;

            const int rows = 50;
            const int cols = 50;
            const double scale = 4;

            List<PointRGB> vertices = new List<PointRGB>(rows * cols);

            Mesh surface = new Mesh();

            surface.NormalAveragingMode = Mesh.normalAveragingType.Averaged;

            for (int j = 0; j < rows; j++)

                for (int i = 0; i < cols; i++)
                {

                    double x = i / 5.0;
                    double y = j / 5.0;

                    double f = 0;

                    double den = Math.Sqrt(x * x + y * y);

                    if (den != 0)

                        f = scale * Math.Sin(Math.Sqrt(x * x + y * y)) / den;

                    // generates a random color
                    int red = (int)(255 - y * 20);
                    int green = (int)(255 - x * 20);
                    int blue = (int)(-f * 50);

                    // clamps color values lat 0-255
                    Utility.LimitRange<int>(0, ref red, 255);
                    Utility.LimitRange<int>(0, ref green, 255);
                    Utility.LimitRange<int>(0, ref blue, 255);

                    vertices.Add(new PointRGB(x, y, f, (byte)red, (byte)green, (byte)blue));

                }

            List<SmoothTriangle> triangles = new List<SmoothTriangle>((rows - 1) * (cols - 1) * 2);

            for (int j = 0; j < (rows - 1); j++)

                for (int i = 0; i < (cols - 1); i++)
                {

                    triangles.Add(new SmoothTriangle(i + j * cols,
                                                          i + j * cols + 1,
                                                          i + (j + 1) * cols + 1));
                    triangles.Add(new SmoothTriangle(i + j * cols,
                                                          i + (j + 1) * cols + 1,
                                                          i + (j + 1) * cols));
                }

            surface.Vertices = vertices.ToArray();
            surface.Triangles = triangles.ToArray();

            model1.Entities.Add(surface);

            // fits the model in the model1
            model1.ZoomFit();
            model1.Invalidate();
            
 	        base.OnContentRendered(e);
        }
    }
}