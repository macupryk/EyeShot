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
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System.Windows.Controls.Primitives;
using devDept.Eyeshot.Labels;
using Font = System.Drawing.Font;
using devDept.Eyeshot.Translators;
using Region = devDept.Eyeshot.Entities.Region;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Textures = "../../../../../../dataset/Assets/Textures/";

        const string mapleMatName = "Maple";
        const string cherryMatName = "Cherry";
        const string plasticMatName = "Plastic";        

        enum materialEnum { Maple = 0, Cherry = 1 };
        private materialEnum currentFrameMaterial;

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
        }

        protected override void OnContentRendered(EventArgs e)
        {
            model1.GetGrid().Visible = false;
            model1.Backface.ColorMethod = backfaceColorMethodType.Cull;            

            currentFrameMaterial = materialEnum.Maple;

            Material mapleMat = new Material(mapleMatName, System.Drawing.Color.FromArgb(100, 100, 100), System.Drawing.Color.White, 1, new Bitmap(Textures + "Maple.jpg"));

            mapleMat.Density = 0.7 * 1e-3; // set maple density

            model1.Materials.Add(mapleMat);

            Material cherryMat = new Material(cherryMatName, System.Drawing.Color.FromArgb(100, 100, 100), System.Drawing.Color.White, 1, new Bitmap(Textures + "Maple.jpg"));

            cherryMat.Density = 0.8 * 1e-3; // set cherry density

            model1.Materials.Add(cherryMat);

            model1.Layers.Add(plasticMatName, System.Drawing.Color.GreenYellow);

            Material plasticLayerMat = new Material(plasticMatName, System.Drawing.Color.GreenYellow);

            model1.Layers[plasticMatName].MaterialName = plasticMatName;

            plasticLayerMat.Density = 1.4 * 1e-3; // set plastic density

            model1.Materials.Add(plasticLayerMat);

            RebuildChair();

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();

            // refresh the viewport
            model1.Invalidate();

            base.OnContentRendered(e);
        }

        private void  seatColor_Click(object sender, RoutedEventArgs e)
        {            
            ToggleButton rb = (ToggleButton)sender;

            model1.Layers[plasticMatName].Color = RenderContextUtility.ConvertColor(rb.Background); // affects edges color
            model1.Materials[plasticMatName].Diffuse = RenderContextUtility.ConvertColor(rb.Background); // affects faces color

            model1.Invalidate();
        }

        private void woodEssence_Click(object sender, RoutedEventArgs e)
        {
            if (mapleEssenceRadioButton.IsChecked.HasValue && mapleEssenceRadioButton.IsChecked.Value)
            {
                currentFrameMaterial = materialEnum.Maple;
            }
            else if (cherryEssenceRadioButton.IsChecked.HasValue && cherryEssenceRadioButton.IsChecked.Value)
            {
                currentFrameMaterial = materialEnum.Cherry;
            }

            RebuildChair();

            model1.Invalidate();
        }

        private void sizeTrackBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (model1 == null)
                return;
            model1.Entities.Clear();
            RebuildChair();
            model1.Invalidate();
        }

        private void exportStlButton_Click(object sender, RoutedEventArgs e)
        {
            string stlFile = "chair.stl";
            WriteSTL ws = new WriteSTL(new WriteParams(model1), stlFile, true);
            ws.DoWork();

            string fullPath = String.Format(@"{0}\{1}", System.Environment.CurrentDirectory, stlFile);
            MessageBox.Show(String.Format("File saved in {0}", fullPath));
        }

        private void exportObjButton_Click(object sender, RoutedEventArgs e)
        {
            string objFile = "chair.obj";

            var ws = new WriteOBJ(new WriteParamsWithMaterials(model1), objFile);
            ws.DoWork();

            string fullPath = String.Format(@"{0}\{1}", System.Environment.CurrentDirectory, objFile);
            MessageBox.Show(String.Format("File saved in {0}", fullPath));
        }

        private void RebuildChair()
        {
            string currentMatName = (currentFrameMaterial == materialEnum.Cherry) ? cherryMatName : mapleMatName;

            model1.Entities.Clear();
            model1.Labels.Clear();

            double legDepth = 3;
            double legWidth = 3;
            double legHeight = 56.5;

            double seatDepth = 29;
            double seatWidth = sizeTrackBar.Value;
            double seatHeight = 1.6;
            double seatY = 27.4;

            //
            // Build the legs
            //
            Mesh leg1 = MakeBox(legWidth, legDepth, legHeight);
            Mesh leg4 = (Mesh)leg1.Clone();


            Mesh leg2 = MakeBox(legWidth, legDepth, seatY);
            Mesh leg3 = (Mesh)leg2.Clone();

            leg2.Translate(seatDepth - legDepth, 0, 0);
            leg3.Translate(seatDepth - legDepth, seatWidth - legWidth, 0);
            leg4.Translate(0, seatWidth - legWidth, 0);

            AddEntityWithMaterial(ref leg1, currentMatName);
            AddEntityWithMaterial(ref leg2, currentMatName);
            AddEntityWithMaterial(ref leg3, currentMatName);
            AddEntityWithMaterial(ref leg4, currentMatName);

            //
            // Build the seat
            //
            double dx = 0.3;
            double dy = 0.2;
            double delta = 0.1;
            double seatPartDepth = 4.5;
            double seatPartOffset = 0.5;

            Point3D[] seatFirstPartPoints = { new Point3D(legDepth + delta, 0,0), 
                                     new Point3D(seatPartDepth, 0,0),
                                     new Point3D(seatPartDepth, seatWidth ,0),
                                     new Point3D(legDepth + delta, seatWidth,0),
                                     new Point3D(legDepth + delta,seatWidth -legWidth + dy,0),
                                     new Point3D(-dx, seatWidth -legWidth, 0),
                                     new Point3D(-dx, legWidth,0),
                                     new Point3D(legDepth + delta, legWidth + dy,0),
                                     new Point3D(legDepth + delta, 0,0)};

            Region seatFirstPart = new Region(new LinearPath(seatFirstPartPoints));
            Mesh seatPart0 = seatFirstPart.ExtrudeAsMesh(new Vector3D(0, 0, seatHeight), 0.01, Mesh.natureType.Smooth);

            seatPart0.Translate(0, -dy, seatY);
            Mesh seatPart1 = MakeBox(seatWidth + 2 * dx, seatPartDepth, seatHeight);
            seatPart1.Translate(seatPartDepth + seatPartOffset, -dy, seatY);

            Mesh seatPart2 = (Mesh)seatPart1.Clone();
            seatPart2.Translate(seatPartDepth + seatPartOffset, 0, 0);

            Mesh seatPart3 = (Mesh)seatPart2.Clone();
            seatPart3.Translate(seatPartDepth + seatPartOffset, 0, 0);

            Mesh seatPart4 = (Mesh)seatPart3.Clone();
            seatPart4.Translate(seatPartDepth + seatPartOffset, 0, 0);

            Mesh seatPart5 = (Mesh)seatPart4.Clone();
            seatPart5.Translate(seatPartDepth + seatPartOffset, 0, 0);

            model1.Entities.Add(seatPart0, plasticMatName);
            model1.Entities.Add(seatPart1, plasticMatName);
            model1.Entities.Add(seatPart2, plasticMatName);
            model1.Entities.Add(seatPart3, plasticMatName);
            model1.Entities.Add(seatPart4, plasticMatName);
            model1.Entities.Add(seatPart5, plasticMatName);

            //
            // Build the bars under the seat
            //
            double underSeatXBarWidth = legWidth * 0.8;
            double underSeatXBarDepth = seatDepth - 2 * legDepth;
            double underSeatXBarHeight = 5.0;

            double underSeatYBarWidth = seatWidth - 2 * legWidth;
            double underSeatYBarDepth = legDepth * 0.8;
            double underSeatYBarHeight = underSeatXBarHeight;

            Mesh barUnderSeatLeft = MakeBox(underSeatXBarWidth, underSeatXBarDepth, underSeatXBarHeight);
            barUnderSeatLeft.Translate(legDepth, (legWidth - underSeatXBarWidth) / 2, seatY - underSeatXBarHeight);

            Mesh barUnderSeatRight = (Mesh)barUnderSeatLeft.Clone();
            barUnderSeatRight.Translate(0, seatWidth - legWidth, 0);

            Mesh barUnderSeatBack = MakeBox(seatWidth - 2 * legWidth, legDepth * 0.8, underSeatYBarHeight);
            barUnderSeatBack.Translate((legDepth - underSeatYBarDepth) / 2, legWidth, seatY - underSeatYBarHeight);

            Mesh barUnderSeatFront = (Mesh)barUnderSeatBack.Clone();
            barUnderSeatFront.Translate(seatDepth - legDepth, 0, 0);

            AddEntityWithMaterial(ref barUnderSeatLeft, currentMatName);
            AddEntityWithMaterial(ref barUnderSeatRight, currentMatName);
            AddEntityWithMaterial(ref barUnderSeatFront, currentMatName);
            AddEntityWithMaterial(ref barUnderSeatBack, currentMatName);

            //
            // Build the two cylinders on the sides
            //
            double CylinderRadius = legWidth / 3;
            double cylinderY = 14.5;
            Mesh leftCylinder = MakeCylinder(CylinderRadius, seatDepth - 2 * legDepth, 16);
            leftCylinder.ApplyMaterial(currentMatName, textureMappingType.Cylindrical, .25, 1);
            leftCylinder.Rotate(Math.PI / 2, new Vector3D(0, 1, 0));
            leftCylinder.Translate(legDepth, legWidth / 2, cylinderY);

            model1.Entities.Add(leftCylinder);

            Mesh rightCylinder = (Mesh)leftCylinder.Clone();
            rightCylinder.Translate(0, seatWidth - legWidth, 0);

            model1.Entities.Add(rightCylinder);


            //
            //  Build the chair back
            //
            double chairBackHorizHeight = 4;
            double chairBackHorizDepth = 2;
            double horizHeight1 = seatY + seatHeight + 7;
            Mesh chairBackHorizontal1 = MakeBox(seatWidth - 2 * legWidth, chairBackHorizDepth, chairBackHorizHeight);
            chairBackHorizontal1.Translate((legDepth - chairBackHorizDepth) / 2.0, legWidth, horizHeight1);

            double cylinderHeight = 12;
            double horizHeight2 = cylinderHeight + chairBackHorizHeight;
            Mesh chairBackHorizontal2 = (Mesh)chairBackHorizontal1.Clone();
            chairBackHorizontal2.Translate(0, 0, horizHeight2);

            AddEntityWithMaterial(ref chairBackHorizontal1, currentMatName);
            AddEntityWithMaterial(ref chairBackHorizontal2, currentMatName);

            double chairBackCylinderRadius = chairBackHorizDepth / 4.0;
            double chairBackCylinderHeight = horizHeight2 - chairBackHorizHeight;
            Mesh chairBackCylinder = MakeCylinder(chairBackCylinderRadius, chairBackCylinderHeight, 16);
            chairBackCylinder.Translate(legDepth / 2.0, legWidth, horizHeight1 + chairBackHorizHeight);

            double chairBackWidth = seatWidth - 2 * legWidth;
            double cylinderOffset = 7;
            int nCylinders = (int)(chairBackWidth / cylinderOffset);
            double offset = (chairBackWidth - (nCylinders + 1) * cylinderOffset) / 2.0;
            offset += cylinderOffset;

            for (int i = 0; i < nCylinders; i++, offset += cylinderOffset)
            {
                Mesh cyl = (Mesh)chairBackCylinder.Clone();
                cyl.ApplyMaterial(currentMatName, textureMappingType.Cylindrical, .25, 1);
                cyl.Translate(0, offset, 0);
                model1.Entities.Add(cyl);
            }

            //
            // Add the linear dimension
            // 
            Point3D dimCorner = new Point3D(0, 0, legHeight);
            Plane myPlane = Plane.YZ;
            myPlane.Origin = dimCorner;

            LinearDim ad = new LinearDim(myPlane, new Point3D(0, 0, legHeight + 1), new Point3D(0, seatWidth, legHeight + 1), new Point3D(seatDepth + 10, seatWidth / 2, legHeight + 10), 3);

            ad.TextSuffix = " cm";
            model1.Entities.Add(ad);

            model1.Entities.UpdateBoundingBox();

            //
            // Update extents
            //
            widthTextBox.Text = model1.Entities.BoxSize.X.ToString("f2") + " cm";
            depthTextBox.Text = model1.Entities.BoxSize.Y.ToString("f2") + " cm";
            heightTextBox.Text = model1.Entities.BoxSize.Z.ToString("f2") + " cm";

            //
            // Update weight
            //      
            double totalWeight = CalcWeight();

            weightTextBox.Text = totalWeight.ToString("f2") + " kg";

            //
            // Product ID label
            //
            devDept.Eyeshot.Labels.LeaderAndText prodIdLabel = new LeaderAndText(seatDepth / 2, seatWidth, 25, "Product ID goes here", new Font("Tahoma", 8.25f), System.Drawing.Color.Black, new Vector2D(20, 0));
            model1.Labels.Add(prodIdLabel);

        }

        private double CalcWeight()
        {

            double totalWeight = 0;

            foreach (Entity ent in model1.Entities)
            {
                // Volume() method is defined in the IFace interface
                if (ent is Mesh)
                {

                    Mesh mesh = ((Mesh)ent);

                    VolumeProperties mp = new VolumeProperties(mesh.Vertices, mesh.Triangles);

                    if (ent.LayerName == plasticMatName)
                    {
                        // Gets plastic layer material density
                        totalWeight += model1.Materials[plasticMatName].Density * mp.Volume;

                    }
                    else
                    {

                        if (ent is Mesh)
                        {

                            Mesh m = (Mesh)ent;

                            Material mat = model1.Materials[ent.MaterialName];

                            totalWeight += mat.Density * mp.Volume;
                        }

                    }

                }

            }

            return totalWeight;
        }

        private Mesh MakeBox(double Width, double Depth, double height)
        {
            return Mesh.CreateBox(Depth, Width, height, Mesh.natureType.Smooth);
        }

        private Mesh MakeCylinder(double radius, double height, int slices)
        {
            return Mesh.CreateCylinder(radius, height, slices, Mesh.natureType.Smooth);
        }

        private void AddEntityWithMaterial(ref Mesh m, string matName)
        {
            m.ApplyMaterial(matName, textureMappingType.Cubic, 1, 1);
            model1.Entities.Add(m);
        }
    }
}