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
using Color = System.Drawing.Color;
using Environment = devDept.Eyeshot.Environment;

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

            // sets selection  as "ByPick"
            selectionComboBox.SelectedIndex = 0;

            model1.SelectionChanged += Model1_SelectionChanged;
            model1.PreviewMouseDown += Model1_MouseDown;
            
            model1.HiddenLines.ColorMethod = hiddenLinesColorMethodType.EntityColor;
            
            model1.GetOriginSymbol().Visible = false;
        }
        
        protected override void OnContentRendered(EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            MeshSpheres();

            model1.Camera.ProjectionMode = projectionType.Orthographic;
            model1.SetView(viewType.Dimetric, true, false);
            model1.Invalidate();

            base.OnContentRendered(e);
        }

        private void Model1_MouseDown(object sender, MouseButtonEventArgs mouseEventArgs)
        {
            // clears the previous selection

            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                foreach (Entity entity in model1.Entities)
                {
                    if (entity is ISelect)
                    {
                        ((ISelect)entity).SelectedSubItems = new List<int>(0);
                    }
                }
            }
        }

        private void Model1_SelectionChanged(object sender, Model.SelectionChangedEventArgs e)
        {
            for (int i = 0; i < model1.Entities.Count; i++)
            {
                var ent = model1.Entities[i];
                if (ent is MyMesh)
                {
                    var m = ((MyMesh)ent);
                    if (m.needsCompileSelected)

                        m.CompileSelected(model1.renderContext);
                }
            }
        }
        private void wireframeButton_Click(object sender, RoutedEventArgs e)
        {
            model1.DisplayMode = displayType.Wireframe;
            model1.Invalidate();
        }

        private void shadedButton_Click(object sender, RoutedEventArgs e)
        {
            model1.DisplayMode = displayType.Shaded;
            model1.Invalidate();
        }

        private void renderedButton_Click(object sender, RoutedEventArgs e)
        {
            model1.DisplayMode = displayType.Rendered;
            model1.Invalidate();
        }

        private void hiddenLinesButton_Click(object sender, RoutedEventArgs e)
        {
            model1.DisplayMode = displayType.HiddenLines;
            model1.Invalidate();
        }
        private void flatButton_Click(object sender, RoutedEventArgs e)
        {
            model1.DisplayMode = displayType.Flat;
            model1.Invalidate();
        }

        private void selectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectCheckBox == null)
                return;

            if (selectCheckBox.IsChecked != null && selectCheckBox.IsChecked.Value)

                Selection();

            else

                model1.ActionMode = actionType.None;
        }

        private void selectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (selectCheckBox.IsChecked != null && selectCheckBox.IsChecked.Value)

                Selection();

            else

                model1.ActionMode = actionType.None;
        }
        private void Selection()
        {
            switch (selectionComboBox.SelectedIndex)
            {
                case 0: // by pick
                    model1.ActionMode = actionType.SelectByPick;
                    // selects only the first triangle 
                    model1.firstOnlyInternal = true;
                    break;

                case 1: // by box
                    model1.ActionMode = actionType.SelectByBox;
                    model1.firstOnlyInternal = false;
                    break;

                case 2: // by poly
                    model1.ActionMode = actionType.SelectByPolygon;
                    model1.firstOnlyInternal = false;
                    break;

                case 3: // by box enclosed
                    model1.ActionMode = actionType.SelectByBoxEnclosed;
                    model1.firstOnlyInternal = false;
                    break;

                case 4: // by poly enclosed
                    model1.ActionMode = actionType.SelectByPolygonEnclosed;
                    model1.firstOnlyInternal = false;
                    break;

                case 5: // visible by pick
                    model1.ActionMode = actionType.SelectVisibleByPick;
                    // selects only the first triangle 
                    model1.firstOnlyInternal = true;
                    break;

                case 6: // visible by box
                    model1.ActionMode = actionType.SelectVisibleByBox;
                    model1.firstOnlyInternal = false;
                    break;

                case 7: // visible by poly
                    model1.ActionMode = actionType.SelectVisibleByPolygon;
                    model1.firstOnlyInternal = false;
                    break;
                    
                default:
                    model1.ActionMode = actionType.None;
                    break;
            }
        }
         private void tabControl1_OnSelectionChanged(object sender, EventArgs e)
        { 
            // every time the selected tab changes ...
            if (!model1.IsLoaded)
                return;

            // reset all actions
            model1.Focus();
            model1.Entities.Clear();  
            
            switch (((TabItem)tabControl1.SelectedItem).Header.ToString())
            {
                case "Triangles":
                    MeshSpheres();
                    break;
                case "Lines":
                    LinearPathSpheres();
                    break;
            }
            model1.SetView(viewType.Dimetric, true, false);
            model1.Invalidate();
        }

        private void MeshSpheres()
        {
            // creates the entities
            MyMesh m1 = new MyMesh(model1, Mesh.CreateSphere(10, 10, 10));
            MyMesh m2 = new MyMesh(model1, Mesh.CreateSphere(10, 10, 10));
           
            m2.Translate(25, 0, 0);

            // Adds entities to the scene
            model1.Entities.Add(m1, Color.FromArgb(255, Color.Green));
            model1.Entities.Add(m2, Color.FromArgb(127, Color.Red));
        }

        private void LinearPathSpheres()
        {
            // creates the entities
            int slices = 20;
            int stacks = 10;
            MyLinearPath m2 = new MyLinearPath(model1, new LinearPath(Mesh.CreateSphere(10, slices, stacks).Vertices));

            //computes group of lines
            IndexLine[] edges = new IndexLine[slices* (stacks -1)];
            for (int i = 0; i < (stacks-1); i++)
            {
                for (int j = 0; j < (slices-1); j++)
                {
                    int v1 = i*slices + j;
                    edges[v1] = new IndexLine(v1, v1+ 1);
                }
                edges[i*slices +(slices-1)] = new IndexLine(i*slices + (slices-1), i*slices);
            }
            m2.Lines = edges;

            MyLinearPath m1 = (MyLinearPath)m2.Clone();
            m2.Translate(25,0, 0);
            m2.LineWeight = 4;

            model1.Entities.Add(m1, Color.FromArgb(255, Color.Green));
            model1.Entities.Add(m2, Color.FromArgb(155, Color.Red));
        }
        
    }
}