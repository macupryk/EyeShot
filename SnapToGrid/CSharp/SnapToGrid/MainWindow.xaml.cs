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
using devDept.Geometry;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot;
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

            model1.InitializeScene += Model1OnInitializeScene;
        }

        private void Model1OnInitializeScene(object sender, EventArgs eventArgs)
        {
            model1.WallHeight = (double)heightNumericUpDown.Value;

            // Adds a rect
            Point3D p1 = new Point3D(0, 0, 0);
            Point3D p2 = new Point3D(80, 0, 0);
            Point3D p3 = new Point3D(0, 80, 0);

            // Set plane points
            model1.SetPlane(p1, p2, p3);

            model1.SetView(viewType.Trimetric);

            model1.GetGrid().AutoSize = false;

            colorPicker.SelectedColor = System.Windows.Media.Color.FromArgb(model1.WallColor.A, model1.WallColor.R, model1.WallColor.G, model1.WallColor.B);
        }

        private void heightNumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (model1 != null)
                model1.WallHeight = (double)heightNumericUpDown.Value;
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var color = e.NewValue.Value;
            model1.WallColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}