using System;
using System.Windows;
using System.Windows.Input;
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
        devDept.Eyeshot.Translators.ReadFile _readFile;
        SplitHelper Sh;
        private double _offset;
        private int _originalIndex;
        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {
            _readFile = new devDept.Eyeshot.Translators.ReadFile("../../../../../../dataset/Assets/Piston.eye");
            _readFile.DoWork();
            _readFile.AddToScene(model1, System.Drawing.Color.White);

            // Inizializes original entity index
            _originalIndex = 0;

            Sh = new SplitHelper(model1);
            Sh.DrawNormalDirection(Point3D.Origin, _readFile.Entities[_originalIndex].BoxSize.Diagonal);                   

            // sets trimetric view
            model1.SetView(viewType.Trimetric);
            model1.Camera.ProjectionMode = projectionType.Orthographic;
            model1.GetGrid().Visible = false;

            // fits the model in the viewport
            model1.ZoomFit();

            //refresh the model control
            model1.Invalidate();            

            base.OnContentRendered(e);  
        }

        private void hideOriginalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (model1 != null)
            {
                if ((bool)this.hideOriginalCheckBox.IsChecked)
                    model1.Entities[_originalIndex].Visible = false;
                else
                    model1.Entities[_originalIndex].Visible = true;

                model1.Invalidate();
            }
        }

        private void pullDirectionButton_Click(object sender, EventArgs e)
        {
            if (model1.Entities.Count > 0)
            {
                Enable_Buttons(false);

                Sh.QuickSplit((Mesh)_readFile.Entities[_originalIndex], Sh.Direction);

                Enable_Buttons(true);
            }
        }

        private void model1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                int selEntityIndex = model1.GetEntityUnderMouseCursor(devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)));

                if (selEntityIndex != -1)
                {
                    Entity entity = model1.Entities[selEntityIndex];
                    Point3D pt;
                    int tri;
                    try
                    {
                        if (model1.FindClosestTriangle((IFace)entity, devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), out pt, out tri) > 0)
                        {
                            IndexTriangle it = ((Mesh)entity).Triangles[tri];

                            // calculates normal direction of selected triangle
                            Point3D[] pointEnt = ((Mesh)entity).Vertices;
                            Triangle selT = new Triangle(pointEnt[it.V1], pointEnt[it.V2], pointEnt[it.V3]);
                            selT.Regen(0.1);
                            Sh.Direction = selT.Normal;

                            // shows the normal's direction like an arrow
                            Sh.DrawNormalDirection(pt, _readFile.Entities[_originalIndex].BoxSize.Diagonal);

                            model1.Entities.Regen();
                            model1.Invalidate();
                        }
                    }
                    catch { }
                }
            }
        }

        private void Enable_Buttons(bool enabled)
        {
            translationSlider.IsEnabled = enabled;
            hideOriginalCheckBox.IsEnabled = enabled;
            pullDirectionButton.IsEnabled = enabled;
            translationSlider.Value = translationSlider.Minimum;
            _offset = 0;
        }

        private void translationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Calculates translation offset from entity size
            double size = _readFile.Entities[_originalIndex].BoxSize.Diagonal;
            double tOffset = (translationSlider.Value - _offset) * size / 50;
            _offset = translationSlider.Value;

            Sh.TranslatingSections(tOffset);
        }

    }
}