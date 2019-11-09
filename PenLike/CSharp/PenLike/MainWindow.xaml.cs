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
            // Adds a rectangle
            Quad q = new Quad(0, 0, 0, 20, 0, 0, 20, 30, 0, 0, 30, 0);
            
            model1.Entities.Add(q, System.Drawing.Color.DarkSlateBlue);

            // Sets trimetric view
            model1.SetView(viewType.Trimetric);

            // Fits the model in the viewport            
            model1.ZoomFit();

            // refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }
    }
}