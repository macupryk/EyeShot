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
using devDept.Eyeshot;
using devDept.Eyeshot.Labels;
using devDept.Geometry;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using Color = System.Drawing.Color;

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
            //model2.Unlock("");
        }

        protected override void OnContentRendered(EventArgs e)
        {                    
            // hides grids
            model1.GetGrid().Visible = false;
            model2.GetGrid().Visible = false;

            // adds entities            
            model1.Entities.Add(new Line(60, 10, 0, 60, 110, 0), System.Drawing.Color.Blue);
            model1.Entities.Add(new Line(10, 60, 0, 110, 60, 0), System.Drawing.Color.Blue);
            model1.Entities.Add(new Circle(60, 60, 0, 40), System.Drawing.Color.Red);
            model1.Entities.Add(new Circle(100, 60, 0, 6), System.Drawing.Color.Red);
            model1.Entities.Add(new Circle(60, 100, 0, 6), System.Drawing.Color.Red);
            model1.Entities.Add(new Circle(60, 20, 0, 6), System.Drawing.Color.Red);
            model1.Entities.Add(new Circle(20, 60, 0, 6), System.Drawing.Color.Red);

            // adds labels
            devDept.Eyeshot.Labels.TextOnly label = new TextOnly(30, 95, 0, "CIRCLE", new System.Drawing.Font("Tahoma", 8.25f), Color.Black);
            label.ColorForSelection = model1.SelectionColor;
            model1.Labels.Add(label);
            devDept.Eyeshot.Labels.TextOnly label2 = new TextOnly(40, 60, 0, "LINE", new System.Drawing.Font("Tahoma", 8.25f), Color.Black);
            label2.ColorForSelection = model1.SelectionColor;
            model1.Labels.Add(label2);

            // sets trimetric view            
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport            
            model1.ZoomFit();

            // refresh the viewports
            model1.Invalidate();
            model2.Invalidate();

            base.OnContentRendered(e);
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            if(selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
                model1.Entities.CopySelection();
            if(selectLabelsButton.IsChecked.HasValue && selectLabelsButton.IsChecked.Value)
                model1.Labels.CopySelection();
        }

        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
            {
                model2.BoundingBox.OverrideSceneExtents = false;
                model2.Entities.Paste();
            }
            if (selectLabelsButton.IsChecked.HasValue && selectLabelsButton.IsChecked.Value)
            {
                model2.Labels.Paste();

                // manually extends BoundingBox to show labels when there are no entities
                if (model2.Entities.Count == 0)
                {
                    model2.BoundingBox.OverrideSceneExtents = true;
                    model2.BoundingBox.Min = new Point3D(10, 10, 0);
                    model2.BoundingBox.Max = new Point3D(110, 110, 0);
                    model2.Entities.UpdateBoundingBox();
                }
            }
            model2.Invalidate();
        }

        private void syncButton_Click(object sender, RoutedEventArgs e)
        {
            // saves the camera from the first model
            Camera savedCamera;
            model1.SaveView(out savedCamera);

            // restores the camera to the second model
            model2.RestoreView(savedCamera);
            model2.Invalidate();
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
            {
                if (selectLabelsButton.IsChecked.HasValue && selectLabelsButton.IsChecked.Value)
                    selectLabelsButton.IsChecked = false;

                model1.ActionMode = actionType.SelectByPick;
            }
            else
            {
                model1.ActionMode = actionType.None;
            }   
        }
        
        private void splitContainer1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            model1.SplitterMoving = true;
            model2.SplitterMoving = true;
        }        

        private void splitContainer1_MouseUp(object sender, MouseButtonEventArgs e)
        {            
            model1.SplitterMoving = false;
            model2.SplitterMoving = false;
        }

        private void selectLabelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectLabelsButton.IsChecked.HasValue && selectLabelsButton.IsChecked.Value)
            {
                if (selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
                    selectButton.IsChecked = false;

                model1.ActionMode = actionType.SelectVisibleByPickLabel;
            }
            else
            {
                model1.ActionMode = actionType.None;
            }
        }
    }
}