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
using devDept.Eyeshot.Translators;

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
        }        

        private Model model1;
        protected override void OnContentRendered(EventArgs e)
        {                    
            model1 = new Model();
            model1.InitializeViewports();
            model1.Viewports[0].Grids.Add(new devDept.Eyeshot.Grid());
            
            model1.Size = new System.Drawing.Size(300, 300);

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            
            model1.CreateControl();

            // hides grid            
            model1.GetGrid().Visible = false;

            // A triangle fan            
            model1.Entities.Add(new Triangle(-10, -10, 0, 10, -10, 0, 0, 0, 5), System.Drawing.Color.Red);
            model1.Entities.Add(new Triangle(+10, -10, 0, 10, +10, 0, 0, 0, 5), System.Drawing.Color.Green);
            model1.Entities.Add(new Triangle(+10, +10, 0, -10, +10, 0, 0, 0, 5), System.Drawing.Color.Cyan);
            model1.Entities.Add(new Triangle(-10, +10, 0, -10, -10, 0, 0, 0, 5), System.Drawing.Color.Blue);
            
            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();
            
            // refresh the viewport
            model1.Invalidate();

            // Update the bounding box, needed by many internal operations
            model1.Entities.UpdateBoundingBox();

            base.OnContentRendered(e);
        }

        private void copyImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.CopyToClipboardRaster();
        }

        private void printPreviewButton_OnClick(object sender, RoutedEventArgs e)
        {            
            model1.PrintPreview(new System.Drawing.Size(400, 500));
        }

        private void saveStlButton_OnClick(object sender, RoutedEventArgs e)
        {
            string stlFile = "test.stl";
            WriteParams wp = new WriteParams(model1);
            WriteSTL ws = new WriteSTL(wp, stlFile);
            ws.DoWork();

            string fullPath = String.Format(@"{0}\{1}", System.Environment.CurrentDirectory, stlFile);
            MessageBox.Show(String.Format("File saved in {0}", fullPath));
        }
    }
}