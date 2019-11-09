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
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Labels;
using devDept.Geometry;
using devDept.Graphics;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string fuselage = "Fuselage", wings = "Wings", tail = "Tail", wires = "Wires";

        private ToolTip tip;
        private int lastIndex = -1;
        private bool cameraIsMoving = false;

        public MainWindow()
        {
            InitializeComponent();

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.

            tip = new ToolTip();
            model1.MouseMove += model1_MouseMove;
            model1.CameraMoveBegin += model1_CameraMoveBegin;
            model1.CameraMoveEnd += model1_CameraMoveEnd;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            // edits default layer
            model1.Layers[0].Name = fuselage;
            model1.Layers[0].Color = System.Drawing.Color.LightGray;

            // additional layers            
            model1.Layers.Add(wings, System.Drawing.Color.CornflowerBlue);
            model1.Layers.Add(tail, System.Drawing.Color.Chartreuse);
            model1.Layers.Add(wires);

            layerListView.SyncLayers();

            #region Jet drawing

            model1.Entities.Add(new Triangle(+15, -30, 8, 0, -30, 23, 0, -60, 8), fuselage, System.Drawing.Color.DeepSkyBlue);
            model1.Entities.Add(new Triangle(0, -60, 8, 0, -30, 23, -15, -30, 8), fuselage, System.Drawing.Color.DeepSkyBlue);
            model1.Entities.Add(new Triangle(-15, -30, 8, 0, -30, 23, 0, +56, 8), fuselage);
            model1.Entities.Add(new Triangle(0, +56, 8, 0, -30, 23, 15, -30, 8), fuselage);
            model1.Entities.Add(new Quad(0, +56, 8, +15, -30, 8, 0, -60, 8, -15, -30, 8), fuselage);


            model1.Entities.Add(new Triangle(0, -27, 10, -60, +8, 10, 60, +8, 10), wings);
            model1.Entities.Add(new Triangle(60, +8, 10, 0, +8, 15, 0, -27, 10), wings);
            model1.Entities.Add(new Triangle(60, +8, 10, -60, +8, 10, 0, +8, 15), wings);
            model1.Entities.Add(new Triangle(0, -27, 10, 0, +8, 15, -60, +8, 10), wings);


            model1.Entities.Add(new Triangle(-30, +57, 7.5, 30, +57, 7.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(0, +40, 7.5, 30, +57, 7.5, 0, +57, 12), tail);
            model1.Entities.Add(new Triangle(0, +57, 12, -30, +57, 7.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(30, +57, 7.5, -30, +57, 7.5, 0, +57, 12), tail);
            model1.Entities.Add(new Triangle(0, +40, 7.5, 3, +57, 8.5, 0, +65, 33), tail);
            model1.Entities.Add(new Triangle(0, +65, 33, -3, +57, 8.5, 0, +40, 7.5), tail);
            model1.Entities.Add(new Triangle(3, +57, 8.5, -3, +57, 8.5, 0, +65, 33), tail);



            Line axis = new Line(-22, 0, 3, 22, 0, 3);

            axis.LineTypeMethod = colorMethodType.byEntity;
            model1.LineTypes.Add("JetAxisPattern", new float[] { 5, -1.5f, 0.25f, -1.5f });
            axis.LineTypeName = "JetAxisPattern";

            model1.Entities.Add(axis, wires);

            // Points
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +12, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +16, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +21, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +27, 10, 4), wires);
            model1.Entities.Add(new devDept.Eyeshot.Entities.Point(-60, +34, 10, 4), wires);

            // Wheels
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(+20, 0, 3), 3), wires);
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(-20, 0, 3), 3), wires);
            model1.Entities.Add(new Circle(Plane.YZ, new Point3D(0, -42, 2), 2), wires);

            // Wheel crosses
            model1.Entities.Add(new Line(-20, 0, 2, -20, 0, 4), wires);
            model1.Entities.Add(new Line(-20, -1, 3, -20, +1, 3), wires);
            model1.Entities.Add(new Line(+20, 0, 2, +20, 0, 4), wires);
            model1.Entities.Add(new Line(+20, -1, 3, +20, +1, 3), wires);
            model1.Entities.Add(new Line(0, -41, 2, 0, -43, 2), wires);
            model1.Entities.Add(new Line(0, -42, 1, 0, -42, 3), wires);

            #endregion

            //// Labels                        
            model1.Labels.Add(new LeaderAndText(+60, +8, 10, "Left wing", new System.Drawing.Font("Tahoma", 8.25f), System.Drawing.Color.Black, new Vector2D(0, 30)));
            model1.Labels.Add(new ImageOnly(0, +65, 33, Properties.Resources.CautionLabel));

            // Dimensions
            Plane dimPlane1 = Plane.XY;
            dimPlane1.Rotate(Math.PI / 2, Vector3D.AxisZ, Point3D.Origin);
            Plane dimPlane2 = Plane.YZ;
            dimPlane2.Rotate(Math.PI / 2, Vector3D.AxisX, Point3D.Origin);

            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, -60, 8), new Point3D(60, 8, 8), new Point3D(70, -26, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, -60, 8), new Point3D(0, 65, 8), new Point3D(80, 0, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane1, new Point3D(0, 57, 8), new Point3D(0, 65, 8), new Point3D(70, 80, 8), 4), wires);
            model1.Entities.Add(new LinearDim(dimPlane2, new Point3D(0, 57, 8), new Point3D(0, 65, 33), new Point3D(0, 75, 20.5), 4), wires);

            model1.ZoomFit();            
            model1.Invalidate();
            layerListView.Environment = model1;
         
            base.OnContentRendered(e);
        }

        private void model1_CameraMoveBegin(object sender, Model.CameraMoveEventArgs e)
        {
            cameraIsMoving = true;            
        }

        private void model1_CameraMoveEnd(object sender, Model.CameraMoveEventArgs e)
        {
            cameraIsMoving = false;         
        }

        private void model1_MouseMove(object sender, MouseEventArgs e)
        {
            if (cameraIsMoving)
                return;
            
            int index = model1.GetEntityUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)));
            if (index != -1 && index != lastIndex)
            {
                //hide the tooltip
                tip.IsOpen = false;

                //get the entity
                Entity ent = model1.Entities[index];

                //get the entity type                
                string entType = ent.GetType().ToString().Split('.').LastOrDefault();

                //show the tooltip with the entity info
                tip.Content = entType + " ID: " + index;                
                ToolTipService.SetToolTip(model1, tip);                                
                tip.IsOpen = true;
                

                lastIndex = index;
            }
        }

        private void Model1_OnMouseLeave(object sender, MouseEventArgs e)
        {
            tip.IsOpen = false;
        }
    }
}