using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using devDept.CustomControls;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using devDept.Graphics;
using devDept.Serialization;
using Microsoft.Win32;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using Region = devDept.Eyeshot.Entities.Region;
using MColor = System.Windows.Media.Color;
using Environment = devDept.Eyeshot.Environment;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Textures = "../../../../../../dataset/Assets/Textures/";

        private DeskBuilder _deskBuilder;
        private Brush _oldColor;
        private bool _blockCallback = true;
        private bool isDrawingModified = true;
        private bool isDrawingToReload = false;
        private bool imported = false;

        ToolTip materialsToolTip = new ToolTip();

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            // drawingsUserControl1.drawings1.Unlock("");

            model1.Rendered.EdgeColorMethod = edgeColorMethodType.EntityColor;
            model1.Units = linearUnitsType.Millimeters;

            drawingsUserControl1.Model = model1;

            tabControl1.SelectionChanged += TabControl1_SelectedIndexChanged;

            model1.WorkCompleted += Model1OnWorkCompleted;
            model1.ProgressChanged += Model1_ProgressChanged;
            model1.WorkCancelled += Model1_WorkCancelled;

            drawingsUserControl1.drawings1.WorkCompleted += Model1OnWorkCompleted;
            drawingsUserControl1.drawings1.ProgressChanged += Model1_ProgressChanged;
            drawingsUserControl1.drawings1.WorkCancelled += Model1_WorkCancelled;

            drawingsUserControl1.drawingsPanel1.updateButton.Click += UpdateButton_Click;

            model1.Rendered.PlanarReflections = true;
            model1.Rendered.PlanarReflectionsIntensity = .1f;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (!imported)
            {
                var s = drawingsUserControl1.drawings1.GetActiveSheet() ?? drawingsUserControl1.drawings1.Sheets[0];
                var ld = drawingsUserControl1.AddSampleDimSheet(s, false);
                for (int i = 0; i < drawingsUserControl1.drawings1.Entities.Count; i++)
                {
                    if (drawingsUserControl1.drawings1.Entities[i] is LinearDim)
                    {
                        drawingsUserControl1.drawings1.Entities.RemoveAt(i--);
                    }
                }
                drawingsUserControl1.drawings1.Entities.AddRange(ld);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            model1.ProgressBar.Visible = false;

            model1.GetGrid().Visible = false;
            model1.Backface.ColorMethod = backfaceColorMethodType.Cull;

            AddMaterials();

            PossibleColors();

            EnableImportExportButtons(false);

            _deskBuilder = new DeskBuilder();
            model1.StartWork(_deskBuilder);

            _blockCallback = false;

            base.OnContentRendered(e);
        }

        
        #region Helper

        private static MColor ToMediaColor(Color color)
        {
            return MColor.FromArgb(color.A, color.R, color.G, color.B);
        }

        private static Color ToDrawingColor(MColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void InitializeDrawings()
        {
            //drawingsUserControl1.RebuildButton_Click(null, null); //rebuild all
            drawingsUserControl1.drawings1.Clear();
            drawingsUserControl1.AddSheet("Sheet1", linearUnitsType.Millimeters, formatType.A4_ISO);
            drawingsUserControl1.AddSheet("Sheet2", linearUnitsType.Inches, formatType.A_ANSI);
            
            // updates the pattern for the hidden segments.
            drawingsUserControl1.drawings1.LineTypes.AddOrReplace(new LinePattern(drawingsUserControl1.drawings1.HiddenSegmentsLineTypeName, new float[] { 0.8f, -0.4f }));
        }

        private void AddMaterials()
        {
            var bmp1 = new Bitmap(Textures + "Oak Bordeaux bright.jpg");
            var bmp2 = new Bitmap(Textures + "Lindberg oak.jpg");
            var bmp3 = new Bitmap(Textures + "Lambrate.jpg");
            var bmp4 = new Bitmap(Textures + "Oak dark.jpg");
            var bmp5 = new Bitmap(Textures + "Oak Torino.jpg");
            var bmp6 = new Bitmap(Textures + "Sonoma oak gray.jpg");

            bmp1.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bmp3.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bmp4.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bmp5.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bmp6.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var mat1 = new Material(Mat1MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp1);
            var mat2 = new Material(Mat2MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp2);
            var mat3 = new Material(Mat3MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp3);
            var mat4 = new Material(Mat4MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp4);
            var mat5 = new Material(Mat5MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp5);
            var mat6 = new Material(Mat6MatName, Color.FromArgb(100, 100, 100), Color.White, 1, bmp6);

            model1.Materials.Add(mat1);
            model1.Materials.Add(mat2);
            model1.Materials.Add(mat3);
            model1.Materials.Add(mat4);
            model1.Materials.Add(mat5);
            model1.Materials.Add(mat6);

            //layer for foots
            model1.Layers.Add(new Layer("foots"));
        }

        private void PossibleColors()
        {
            var converter = new System.Windows.Media.BrushConverter();

            materialsToolTip.Content = "R20074";
            comboBoxVeneer.ToolTip = materialsToolTip;

            //set the default color for the frame
            _oldColor = (Brush) converter.ConvertFromString("#8B8C7A");
            //paint color
            greenColorRadioButton.Background = (Brush) converter.ConvertFromString("#8B8C7A"); //Stone gray
            var greenRadioButtonToolTip = new ToolTip {Content = "Stone gray, RAL 7030"};
            greenColorRadioButton.ToolTip = greenRadioButtonToolTip;

            orangeColorRadioButton.Background = (Brush) converter.ConvertFromString("#193737");//Pearly opal green
            var orangeRadioButtonToolTip = new ToolTip{Content = "Pearly opal green, RAL 6036"};
            orangeColorRadioButton.ToolTip = orangeRadioButtonToolTip;

            blueColorRadioButton.Background = (Brush) converter.ConvertFromString("#F6F6F6");//White traffic
            var blueRadioButtonToolTip = new ToolTip{Content = "White traffic, RAL 9016"};
            blueColorRadioButton.ToolTip = blueRadioButtonToolTip;

            pinkColorRadioButton.Background = (Brush) converter.ConvertFromString("#EAE6CA");//White pearl
            var pinkRadioButtonToolTip = new ToolTip{Content = "White pearl, RAL 1013"};
            pinkColorRadioButton.ToolTip = pinkRadioButtonToolTip;

            //end caps color
            var blackRadioButtonToolTip = new ToolTip{Content = "Black, RAL 9005"};
            blackColorRadioButton.Background = (Brush) converter.ConvertFromString("#0A0A0A");//Deep black
            blackColorRadioButton.ToolTip = blackRadioButtonToolTip;

            var whiteRadioButtonToolTip = new ToolTip{Content = "Pure White, RAL 9010"};;
            whiteColorRadioButton.ToolTip = blackRadioButtonToolTip;
        }

        private static Color HexToColor(string hex)
        {
            //Ty stack overflow
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6) throw new Exception("Hex color not valid");

            return Color.FromArgb(
                int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
                int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber));
        }

        //Materials
        private const string Mat1MatName = "Oak Bordeaux bright";
        private const string Mat2MatName = "Lindberg oak";
        private const string Mat3MatName = "Lambrate";
        private const string Mat4MatName = "Oak dark";
        private const string Mat5MatName = "Oak Torino";
        private const string Mat6MatName = "Sonoma oak gray";

        private void ReloadDrawings()
        {

            if (tabControl1.SelectedIndex == 1) //I'm showing the drawings
            {
                if (comboBoxActiveObject.SelectedIndex == 2)
                    drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(3);//1:10
                else if (comboBoxActiveObject.SelectedIndex == 4)
                    drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(2);//1:5
                else
                    drawingsUserControl1.drawingsPanel1.SetScaleComboValueIndex(5);//1:50

                //add drawings
                InitializeDrawings();
                UpdateLinearDim();

                //rebuild
                drawingsUserControl1.drawings1.Rebuild(model1, true, false);
                isDrawingModified = false;
                isDrawingToReload = false;
            }
        }


        //Frame color modifier
        private void ChangeColor(Brush c, bool foots)
        {
            var converter = new System.Windows.Media.BrushConverter();
            if (foots) //get foot color if necessary
                c = blackColorRadioButton.IsChecked != null && (bool) blackColorRadioButton.IsChecked ? (Brush) converter.ConvertFromString("#0A0A0A") : (Brush) converter.ConvertFromString("#FFFFFF");
            foreach (var block in model1.Blocks)
            {
                if (string.CompareOrdinal(block.Name, "Top") == 0) continue;
                foreach (var entity in block.Entities)
                {
#if NURBS
                    if (!(entity is Brep)) continue;
#else
                    if (!(entity is Mesh)) continue;
#endif
                    if (foots && string.CompareOrdinal(entity.LayerName, "foots") == 0 ||
                        !foots && string.CompareOrdinal(entity.LayerName, "foots") != 0)
                    {
                        entity.Color = ToDrawingColor(((SolidColorBrush)c).Color);
                        //brep.Compile(new CompileParams(model1));
                    }
                }
            }

            model1.Entities.Regen();
            //model1.Invalidate();
            model1.Refresh();
        }


        private double[] CalcWeight()
        {
            const double 
                woodDensity = 0.7 * 1e-3,
                plasticDensity = 1.4 * 1e-3,
                steelDensity = 7.8 * 1e-3;

            double
                steel = 0,
                wood = 0,
                plastic = 0,
                totalWeight = 0;
            foreach (var b in model1.Blocks)
            {
                foreach (var ent in b.Entities)
                {
                    
                    var mp = new VolumeProperties();
#if NURBS
                    if (!(ent is Brep)) continue;
                    Brep brep = (Brep) ent;
                    var meshes = brep.GetPolygonMeshes();
                    foreach (var m in meshes)
                        mp.Add(m.Vertices, m.Triangles);
#else
                    if (!(ent is Mesh)) continue;
                    Mesh m = (Mesh) ent;
                    mp.Add(m.Vertices, m.Triangles);
#endif
                    
                    if (b.Name == "Top")
                    {
                        //wood
                        wood += mp.Volume * woodDensity;
                    }
                    else if (b.Name == "Foot")
                    {
                        //plastic
                        plastic += mp.Volume * plasticDensity;
                    }
                    else
                    {
                        //steel
                        steel += mp.Volume * steelDensity;
                    }
                }
            }

            totalWeight = steel + wood + plastic;
            return new[] {totalWeight, plastic, wood, steel};
        }

        private void GetUserDefinedDimensions(DeskBuilder builder)
        {
            if (comboBoxWidth.SelectedItem != null)
            {
                builder.Width = int.Parse(((ComboBoxItem)comboBoxWidth.SelectedItem).Content.ToString());
            }

            if (comboBoxHeigth.SelectedItem != null)
            {
                builder.Height = int.Parse(((ComboBoxItem)comboBoxHeigth.SelectedItem).Content.ToString());
                builder.Height -=
                    4.75 + //foot height 
                    25; //desk top height
            }

            if (comboBoxDepth.SelectedItem != null)
            {
                builder.TableTopDepth = int.Parse(((ComboBoxItem)comboBoxDepth.SelectedItem).Content.ToString());
            }

            builder.Depth = builder.TableTopDepth * 2 + 70;
            if (builder.SingleTable)
            {
                builder.Depth = builder.TableTopDepth;
            }

            var selectedMat = comboBoxVeneer.Text;
            if (selectedMat != null)
                builder.SelMat = selectedMat.ToString();
        }

        //Reload all the scene
        private void ReloadScene()
        {
            //disable buttons
            EnableImportExportButtons(false);
            //set to All
            comboBoxActiveObject.SelectionChanged -= ComboBoxActiveObject_OnSelectionChanged;
            comboBoxActiveObject.SelectedIndex = 0;
            comboBoxActiveObject.SelectionChanged += ComboBoxActiveObject_OnSelectionChanged;
            //store old color
            _oldColor = new SolidColorBrush(ToMediaColor(model1.Blocks[0].Entities[0].Color));
            //remove old blocks
            model1.Blocks.Clear();
            //update user defined fields
            GetUserDefinedDimensions(_deskBuilder);
            //rebuild the desk
            model1.StartWork(_deskBuilder);
        }

        #endregion

        #region MainLogic

        private class DeskBuilder : WorkUnit
        {
            public bool SingleTable = false;

            private Block _frameBlock, _holderBlock, _topBlock, _footBlock;

            public double
                Width = 1600,
                Height = 740 - 4.75 - 25,
                TableTopDepth = 800,
                Depth = 1670,
                LegWidth = 70;

            public string SelMat = "Oak Bordeaux bright";

#if NURBS
            public Brep _top;
#else
            public Mesh _top;
#endif

            #region Logo
#if NURBS
            //Regions for the DevDept logo
            private static devDept.Eyeshot.Entities.Region[] LogoRegions()
            {
                var entList = new List<ICurve>();
                var pw0 = new Point4D[19]{
                new Point4D(335.788091517279,-256.5327,0,1),
                new Point4D(336.053758010568,-256.926267,0,1),
                new Point4D(388.669329681387,-334.872943,0,1),
                new Point4D(391.642592606276,-339.328100500001,0,1),
                new Point4D(394.374609537259,-343.37788,0,1),
                new Point4D(399.12596191723,-341.0402095,0,1),
                new Point4D(399.149889916625,-341.0283,0,1),
                new Point4D(399.352172411515,-340.9385005,0,1),
                new Point4D(439.515229896911,-323.106495,0,1),
                new Point4D(461.831087833165,-312.727500500001,0,1),
                new Point4D(483.959979774142,-302.365381499999,0,1),
                new Point4D(487.123272694231,-287.8552225,0,1),
                new Point4D(487.138687693841,-287.7822,0,1),
                new Point4D(487.179053692822,-287.6075835,0,1),
                new Point4D(495.180139490697,-252.988104,0,1),
                new Point4D(496.935492946353,-243.776402,0,1),
                new Point4D(498.651558903002,-234.727123,0,1),
                new Point4D(498.659192402809,-232.1893875,0,1),
                new Point4D(498.659187402809,-232.1768,0,1),
            };
                var u0 = new double[23]{
                0,
                0,
                0,
                0,
                1,
                1,
                1,
                2,
                2,
                2,
                3,
                3,
                3,
                4,
                4,
                4,
                5,
                5,
                5,
                6,
                6,
                6,
                6,
            };
                entList.Add(new Curve(3, u0, pw0));
                var pw1 = new Point4D[40]{
                new Point4D(498.659187402809,-232.1768,0,1),
                new Point4D(495.910444972248,-244.324762,0,1),
                new Point4D(469.762880132791,-255.1633665,0,1),
                new Point4D(469.630888136126,-255.2178,0,1),
                new Point4D(469.450350640686,-255.286921,0,1),
                new Point4D(433.602130046289,-269.012957,0,1),
                new Point4D(413.265886060025,-277.017522,0,1),
                new Point4D(413.125407563574,-277.073069,0,1),
                new Point4D(405.436501257812,-280.1481525,0,1),
                new Point4D(399.483438408199,-282.5834885,0,1),
                new Point4D(399.449202409064,-282.59769,0,1),
                new Point4D(399.447277409113,-282.5977095,0,1),
                new Point4D(399.431234409518,-282.6043465,0,1),
                new Point4D(397.110549468143,-283.5464345,0,1),
                new Point4D(395.408665511137,-283.6283925,0,1),
                new Point4D(394.678800529575,-283.605612,0,1),
                new Point4D(394.64888303033,-283.604505,0,1),
                new Point4D(394.621097031032,-283.6044855,0,1),
                new Point4D(394.594534031703,-283.6015995,0,1),
                new Point4D(394.537151033153,-283.5986055,0,1),
                new Point4D(394.491709534301,-283.596695,0,1),
                new Point4D(394.448410035395,-283.5936385,0,1),
                new Point4D(394.052800045389,-283.558473,0,1),
                new Point4D(393.677315054874,-283.4978835,0,1),
                new Point4D(393.328320063691,-283.414495,0,1),
                new Point4D(393.302195564351,-283.40919,0,1),
                new Point4D(393.278785064942,-283.4023005,0,1),
                new Point4D(393.252588065604,-283.3950555,0,1),
                new Point4D(392.869759075275,-283.2996585,0,1),
                new Point4D(392.518719084143,-283.175746,0,1),
                new Point4D(392.188243592491,-283.038671,0,1),
                new Point4D(390.840508626538,-282.4383445,0,1),
                new Point4D(389.810468152559,-281.430575,0,1),
                new Point4D(389.255666166575,-280.7951835,0,1),
                new Point4D(389.219777667481,-280.7506475,0,1),
                new Point4D(382.663502833107,-272.5628525,0,1),
                new Point4D(382.630390333943,-272.5215,0,1),
                new Point4D(382.355587840885,-272.17963,0,1),
                new Point4D(327.94469421542,-204.48937,0,1),
                new Point4D(327.669891722362,-204.1475,0,1),
            };
                var u1 = new double[44]{
                0,
                0,
                0,
                0,
                1,
                1,
                1,
                2,
                2,
                2,
                3,
                3,
                3,
                4,
                4,
                4,
                5,
                5,
                5,
                6,
                6,
                6,
                7,
                7,
                7,
                8,
                8,
                8,
                9,
                9,
                9,
                10,
                10,
                10,
                11,
                11,
                11,
                12,
                12,
                12,
                13,
                13,
                13,
                13,
            };
                entList.Add(new Curve(3, u1, pw1));
                var pw2 = new Point4D[13]{
                new Point4D(372.117190599529,-204.1475,0,1),
                new Point4D(372.177061098016,-204.1270895,0,1),
                new Point4D(384.03141979855,-200.0858105,0,1),
                new Point4D(384.193338845552,-200.034106,0,1),
                new Point4D(404.452696451485,-193.817484,0,1),
                new Point4D(415.341845943872,-189.8545415,0,1),
                new Point4D(444.368183826766,-211.630902,0,1),
                new Point4D(473.221427545421,-233.4641235,0,1),
                new Point4D(449.928863133841,-240.712943,0,1),
                new Point4D(449.810488636831,-240.749,0,1),
                new Point4D(449.658775140664,-240.804105,0,1),
                new Point4D(419.619502899519,-251.714895,0,1),
                new Point4D(419.467789403352,-251.77,0,1),
            };
                var u2 = new double[17]{
                0,
                0,
                0,
                0,
                1,
                1,
                1,
                2,
                2,
                2,
                3,
                3,
                3,
                4,
                4,
                4,
                4,
            };
                entList.Add(new Curve(3, u2, pw2));
                var pt3 = new Point3D[2]{
                new Point3D(327.669891722362,-204.1475,0),
                new Point3D(335.788091517279,-256.5327,0),
            };
                entList.Add(new LinearPath(pt3));
                var pt4 = new Point3D[2]{
                new Point3D(419.467789403352,-251.77,0),
                new Point3D(372.117190599529,-204.1475,0),
            };
                entList.Add(new LinearPath(pt4));



                ICurve[] contours = UtilityEx.GetConnectedCurves(entList, 0.1);
                devDept.Eyeshot.Entities.Region[] regions = UtilityEx.DetectRegionsFromContours(contours, 0.1, Plane.XY);
                return regions;
            }
#endif
            #endregion

            #region Builders
#if NURBS
            private static List<Brep> FrameBuilder(double width, double height, double tableTopDepth, bool singleTable)
#else
            private static List<Mesh> FrameBuilder(double width, double height, double tableTopDepth, bool singleTable)
#endif
            
            {
                //70 is the space betwee the two desks
                var depth = tableTopDepth * 2 + 70;
                //with a single table the depth of the desk it's the same as the depth of th table
                if (singleTable)
                {
                    depth = tableTopDepth;
                }

                //leg
                const int legDepth = 40;
                const int legWidth = 70;

                //junction
                const int junDepth = 30;
                const int junHeight = 50;

                //leg
                var legOuter = CompositeCurve.CreateRoundedRectangle(legWidth, legDepth, 5);
                legOuter.Translate(0, -legDepth);
                var legRegion = legOuter.OffsetToRegion(-2, 0, true);
                
                var legRail = new CompositeCurve(new ICurve[] {
                    new Line(Point3D.Origin, new Point3D(0, 0, height - legDepth - 10)),
                    new Line(new Point3D(0, 0, height - legDepth - 10), new Point3D(0, depth - legDepth * 2, height - legDepth - 10)),
                    new Line(new Point3D(0, depth - legDepth * 2, height - legDepth - 10), new Point3D(0, depth - legDepth * 2, 0)),
                });

#if NURBS
                var leg = legRegion.SweepAsBrep(legRail, 0.1);
#else
                var leg = legRegion.SweepAsMesh(legRail, 0.1);
#endif
                leg.Translate(0, legDepth);

                var rightPlane = new Plane(new Point3D(0, depth / 2, 0), Vector3D.AxisX, Vector3D.AxisZ);
                var rightMirr = new Mirror(rightPlane);
                var frontPlane = new Plane(new Point3D(width / 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ);
                var frontMirr = new Mirror(frontPlane);

#if NURBS
                //screw leg junction
                var screwHole = new Circle(Plane.ZY, 5.5);
                var screwReg = new Region(screwHole);
                screwReg.Translate(legWidth + 1, ((tableTopDepth / 4) + 50), height - 30);

                //holes
                leg.ExtrudeRemove(screwReg, 4);
                screwReg.Translate(0, 100, 0);
                leg.ExtrudeRemove(screwReg, 4);
                screwReg.Translate(0, 100, 0);
                leg.ExtrudeRemove(screwReg, 4);
                //if it's not a single table do 3 more holes
                if (!singleTable)
                {
                    screwReg.TransformBy(rightMirr);
                    leg.ExtrudeRemove(screwReg, -4);
                    screwReg.Translate(0, 100, 0);
                    leg.ExtrudeRemove(screwReg, -4);
                    screwReg.Translate(0, 100, 0);
                    leg.ExtrudeRemove(screwReg, -4);
                }

                var legFront = (Brep)leg.Clone();
#else
                var legFront = (Mesh)leg.Clone();
#endif
                legFront.TransformBy(frontMirr);

                //junctions
                var junctionOuter = CompositeCurve.CreateRoundedRectangle(junHeight, junDepth, 5);
                junctionOuter.Rotate(Math.PI / 2, Vector3D.AxisY);
                var junctionRegion = junctionOuter.OffsetToRegion(-2, 0, true);
#if NURBS
                var junction = junctionRegion.ExtrudeAsBrep(width - (legWidth * 2));
#else
                var junction = junctionRegion.ExtrudeAsMesh(width - (legWidth * 2), 0.1, Mesh.natureType.Plain);
#endif
                
                junction.Translate(legWidth, (tableTopDepth / 4) - (junDepth / 2), height);

#if NURBS
                //screw junction junctions
                //2 circles
                var screwHoleJunctionUp = new Circle(Plane.XY, 3.5);
                var screwHoleJunctionDown = (Circle)screwHoleJunctionUp.Offset(5, Vector3D.AxisZ);
                //holes
                var screwRegJunctionUp = new Region(screwHoleJunctionUp);
                var screwRegJunctionDown = new Region(screwHoleJunctionDown);
                //move the regions (1)
                screwRegJunctionUp.Translate(100, (tableTopDepth / 4), height);
                screwRegJunctionDown.Translate(100, (tableTopDepth / 4), height - junHeight);
                //holes
                junction.ExtrudeRemove(screwRegJunctionUp, -2);
                junction.ExtrudeRemove(screwRegJunctionDown, 2);
                //move the regions (2)
                screwRegJunctionUp.Translate(200, 0, 0);
                screwRegJunctionDown.Translate(200, 0, 0);
                //holes
                junction.ExtrudeRemove(screwRegJunctionUp, -2);
                junction.ExtrudeRemove(screwRegJunctionDown, 2);
                //move the regions (3)
                screwRegJunctionUp.TransformBy(frontMirr);
                screwRegJunctionDown.TransformBy(frontMirr);
                //holes
                junction.ExtrudeRemove(screwRegJunctionUp, 2);
                junction.ExtrudeRemove(screwRegJunctionDown, -2);
                //move the regions (4)
                screwRegJunctionUp.Translate(200, 0, 0);
                screwRegJunctionDown.Translate(200, 0, 0);
                //holes
                junction.ExtrudeRemove(screwRegJunctionUp, 2);
                junction.ExtrudeRemove(screwRegJunctionDown, -2);

                var innerJunction = (Brep)junction.Clone();
#else
                var innerJunction = (Mesh)junction.Clone();
#endif
                innerJunction.Translate(0, (tableTopDepth / 4) * 2);

#if NURBS
                var result = new List<Brep>
#else
                var result = new List<Mesh>
#endif
            {
                junction,
                innerJunction,
                leg,
                legFront
            };

                if (singleTable) return result;
                //Mirror for the other two
#if NURBS
                var junctionRx = (Brep)junction.Clone();
                var innerJunctionRx = (Brep)innerJunction.Clone();
#else
                var junctionRx = (Mesh)junction.Clone();
                var innerJunctionRx = (Mesh)innerJunction.Clone();
#endif
                
                junctionRx.TransformBy(rightMirr);
                innerJunctionRx.TransformBy(rightMirr);

                result.Add(junctionRx);
                result.Add(innerJunctionRx);

                return result;
            }

#if NURBS
            private Brep HolderBuilder(double width, double height, double tableTopDepth)
#else
            private Mesh HolderBuilder(double width, double height, double tableTopDepth)
#endif
            {
                double depth = tableTopDepth * 2 + 70;

                //Holder
                //Logo side
                var leftSide = new Line(new Point3D(0, 0, 8), new Point3D(0, 0, 519 - 16));
                var arcTopLeft = new Arc(new Point3D(16, 0, 519 - 16), leftSide.EndPoint, new Point3D(16, 0, 519));
                var topSide = new Line(arcTopLeft.EndPoint, new Point3D(250 - 16, 0, 519));
                var verticalMirr = new Mirror(new Plane(new Point3D(250 / 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ));
                var arcTopRight = (Arc)arcTopLeft.Clone();
                arcTopRight.TransformBy(verticalMirr);
                var rightSide = (Line)leftSide.Clone();
                rightSide.TransformBy(verticalMirr);
                var bottomSide = new Line(leftSide.StartPoint, rightSide.StartPoint);
                var frontPanel = new CompositeCurve(leftSide, arcTopLeft, topSide, arcTopRight, rightSide, bottomSide);
                var frontPanelReg = new Region(frontPanel);
#if NURBS
                var frontPanelModel = frontPanelReg.ExtrudeAsBrep(4);

                //Logo
                foreach (var item in LogoRegions())
                {
                    item.Rotate(Math.PI / 2, Vector3D.AxisX);
                    item.Rotate(Math.PI, Vector3D.AxisZ);
                    item.Scale(.55);
                    item.Translate(350, -3, 300);
                    frontPanelModel.ExtrudeRemove(item, 10);
                }

                //holes
                var topHole = CompositeCurve.CreateRoundedRectangle(40, 10, 2);
                topHole.Rotate(Math.PI / 2, Vector3D.AxisX);
                var topHoleReg = new Region(topHole);
                topHoleReg.Translate((250 / 2) - 20, 0, 519 - 60 - 10);
                frontPanelModel.ExtrudeRemove(topHoleReg, -4);
                topHoleReg.Translate(0, 0, -18);
                frontPanelModel.ExtrudeRemove(topHoleReg, -4);
                //screw holes
                var screwHoleHolder = new Circle(Plane.XZ, 6.5);
                var screwHoleHolderReg = new Region(screwHoleHolder);
                var screwHoleHolderCentralReg = (Region)screwHoleHolderReg.Clone();
                screwHoleHolderReg.Translate(25, 0, 519 - 16);
                frontPanelModel.ExtrudeRemove(screwHoleHolderReg, -4);
                screwHoleHolderReg.TransformBy(verticalMirr);
                frontPanelModel.ExtrudeRemove(screwHoleHolderReg, 4);
                screwHoleHolderCentralReg.Translate(250 / 2, 0, 519 - 16);
                frontPanelModel.ExtrudeRemove(screwHoleHolderCentralReg, -4);
#else
                var frontPanelModel = frontPanelReg.ExtrudeAsMesh(4,0.1,Mesh.natureType.Plain);
#endif


                var basement = new Line(new Point3D(0, -4), new Point3D(250, -4));

                //Base side
                //start from left side
                var baseConj = new Line(new Point3D(0, -4), new Point3D(0, -6));
                var bottArcTopSx = new Arc(new Point3D(-5, -6, 0), 5, -Math.PI / 2);
                var bottArcTopLeftExt = new Line(bottArcTopSx.EndPoint, new Point3D(-15, -10, 0));
                var bottArcTopLeftOut = new Arc(new Point3D(-15, -15, 0), 5, Math.PI / 2, Math.PI);
                var leftSideBott = new Line(bottArcTopLeftOut.EndPoint, new Point3D(-20, -145, 0));
                var bottArcBotLeft = new Arc(new Point3D(-5, -145, 0), 15, Math.PI, 3 * (Math.PI / 2));
                var bottLineBot = new Line(bottArcBotLeft.EndPoint, new Point3D(255, -160, 0));

                //mirrored parts
                var mirrZero = (Arc)bottArcTopSx.Clone();
                mirrZero.TransformBy(verticalMirr);
                var mirrTwo = (Arc)bottArcTopLeftOut.Clone();
                mirrTwo.TransformBy(verticalMirr);
                var mirrFour = (Arc)bottArcBotLeft.Clone();
                mirrFour.TransformBy(verticalMirr);
                var mirrOne = (Line)bottArcTopLeftExt.Clone();
                mirrOne.TransformBy(verticalMirr);
                var mirrThree = (Line)leftSideBott.Clone();
                mirrThree.TransformBy(verticalMirr);
                var mirrFive = (Line)baseConj.Clone();
                baseConj.TransformBy(verticalMirr);

                var botObj = new CompositeCurve(
                    basement,
                    baseConj,
                    bottArcTopSx,
                    bottArcTopLeftExt,
                    bottArcTopLeftOut,
                    leftSideBott,
                    bottArcBotLeft,
                    bottLineBot,
                    mirrZero,
                    mirrOne,
                    mirrTwo,
                    mirrThree,
                    mirrFour,
                    mirrFive);

                var baseBotReg = new Region(botObj);

#if NURBS
                var baseBot = baseBotReg.ExtrudeAsBrep(4);

                //Hole
                var baseHole = CompositeCurve.CreateRoundedRectangle(40, 10, 2);
                var baseHoleReg = new Region(baseHole);
                baseHoleReg.Translate((255 / 2) - 20, -164 + 18);
                baseBot.ExtrudeRemove(baseHoleReg, 4);
#else
                var baseBot = baseBotReg.ExtrudeAsMesh(4, 0.1, Mesh.natureType.Plain);
#endif

                //Junction
                var bottomRect = CompositeCurve.CreateRectangle(250, 4);
                var bottRectReg = new Region(bottomRect);
#if NURBS
                var bottomConj = bottRectReg.RevolveAsBrep(-Math.PI / 2, Vector3D.AxisX, new Point3D(250 / 2, -4, 0));
#else
                var bottomConj = bottRectReg.RevolveAsMesh(0, -Math.PI / 2, Vector3D.AxisX, new Point3D(250 / 2, -4, 0), 20, 0.1, Mesh.natureType.Plain);
#endif
                bottomConj.Translate(0, 0, 8);

                
#if NURBS
                baseBot.Add(bottomConj, frontPanelModel);
#else
                baseBot.MergeWith(bottomConj);
                baseBot.MergeWith(frontPanelModel);
#endif
                return baseBot;
            }

#if NURBS
            private static Brep FootBuilder()
            {
                var footBase = Brep.CreateBox(70, 40, 4.75);
                var cc = CompositeCurve.CreateRoundedRectangle(66, 36, 4);
                var footInnerRegion = cc.OffsetToRegion(-4, 0, false);
                var footInner = footInnerRegion.ExtrudeAsBrep(20);
                footInner.Translate(2, 2, 4.75);
                footBase.Add(footInner);
                return footBase;
            }
#else
            private static List<Mesh> FootBuilder()
            {
                var footBase = Mesh.CreateBox(70, 40, 4.75);
                var cc = CompositeCurve.CreateRoundedRectangle(66, 36, 4);
                var footInnerRegion = cc.OffsetToRegion(-4, 0, false);
                var footInner = footInnerRegion.ExtrudeAsMesh(20, 0.1, Mesh.natureType.Plain);
                footInner.Translate(2, 2, 4.75);
                return new List<Mesh>(){footBase, footInner};
            }
#endif

#if NURBS
            private static Brep TopBuilder(double width, double height, double tableTopDepth)
#else
            private static Mesh TopBuilder(double width, double height, double tableTopDepth)
#endif
            {
                //var depth = tableTopDepth * 2 + 70;
                var tableTopHeigth = 25;
#if NURBS
                
                var frontPlane = new Plane(new Point3D(width / 2, 0, 0), Vector3D.AxisY, Vector3D.AxisZ);
                var frontMirr = new Mirror(frontPlane);
                //Table top
                var tableTopBox = Brep.CreateBox(width, tableTopDepth, tableTopHeigth);

                //screw holes
                var tableTopHole = new Circle(Plane.XY, 4);
                var tableTopHoleReg = new Region(tableTopHole);
                tableTopHoleReg.Translate(100, (tableTopDepth / 4), 0);

                //Extrude remove pattern
                tableTopBox.ExtrudeRemovePattern(tableTopHoleReg, 14, (tableTopDepth / 4), 2, (tableTopDepth / 2), 2);
                tableTopHoleReg.TransformBy(frontMirr);
                tableTopBox.ExtrudeRemovePattern(tableTopHoleReg, -14, (tableTopDepth / 4), 2, (tableTopDepth / 2), 2);
#else
                var tableTopBox = Mesh.CreateBox(width, tableTopDepth, tableTopHeigth);
#endif

                return tableTopBox;
            }

            #endregion

            protected override void DoWork(System.ComponentModel.BackgroundWorker worker, System.ComponentModel.DoWorkEventArgs doWorkEventArgs)
            {
                //initialize progress
                UpdateProgress(0, 100, "Rebuilding desk", worker);

                Color structureColor = HexToColor("8B8C7A"), footColor = Color.White;

                UpdateProgress(20, 100, "Creating blocks", worker);

                _frameBlock = new Block("Frame", linearUnitsType.Millimeters);
                _holderBlock = new Block("Holder", linearUnitsType.Millimeters);
                _topBlock = new Block("Top", linearUnitsType.Millimeters);
                _footBlock = new Block("Foot", linearUnitsType.Millimeters);

                UpdateProgress(40, 100, "Creating Frame", worker);
                //Frame
                foreach (var item in FrameBuilder(Width, Height, TableTopDepth, SingleTable))
                {
                    item.ColorMethod = colorMethodType.byEntity;
                    item.Color = structureColor;
                    _frameBlock.Entities.Add(item);
                }

                UpdateProgress(60, 100, "Creating Holder", worker);
                //Holder
#if NURBS
                Brep holder = HolderBuilder(Width, Height, TableTopDepth);
#else
                Mesh holder = HolderBuilder(Width, Height, TableTopDepth);
#endif
                holder.ColorMethod = colorMethodType.byEntity;
                holder.Color = structureColor;
                _holderBlock.Entities.Add(holder);

                UpdateProgress(80, 100, "Creating Foot", worker);
                //Foot
#if NURBS
                Brep foot = FootBuilder();
                foot.LayerName = "foots";
                foot.ColorMethod = colorMethodType.byEntity;
                foot.Color = footColor;
                _footBlock.Entities.Add(foot);
#else
                List<Mesh> foot = FootBuilder();
                foreach (Mesh m in foot)
                {
                    m.LayerName = "foots";
                    m.ColorMethod = colorMethodType.byEntity;
                    m.Color = footColor;
                    _footBlock.Entities.Add(m);
                }
#endif
                

                UpdateProgressTo100("Creating Top", worker);
                //Top
                _top = TopBuilder(Width, Height, TableTopDepth);
                _top.ColorMethod = colorMethodType.byEntity;
                _topBlock.Entities.Add(_top);

                //selected material
#if NURBS
                _top.MaterialName = SelMat;
#else
                _top.ApplyMaterial(SelMat, textureMappingType.Cubic, .5, .5);
#endif
            }

            protected override void WorkCompleted(Environment environment)
            {
                environment.Blocks.Add(_frameBlock);
                environment.Blocks.Add(_holderBlock);
                environment.Blocks.Add(_topBlock);
                environment.Blocks.Add(_footBlock);

                //nel caso del tavolo singolo non bast un holder ed un top
                if (!SingleTable)
                {
                    environment.Entities.Add(new BlockReference(0, Depth - TableTopDepth, Height + 4.75, "Top", 0));
                    environment.Entities.Add(new BlockReference(LegWidth + 4, Depth - ((((TableTopDepth / 4) + 50)) + 250 - 25), Height - 519 - 10 - 4 + 4.75, "Holder", Math.PI / 2));
                }
                environment.Entities.Add(new BlockReference(0, 0, 4.75, "Frame", 0));
                environment.Entities.Add(new BlockReference(0, 0, Height + 4.75, "Top", 0));
                environment.Entities.Add(new BlockReference(Width - LegWidth - 4, (((TableTopDepth / 4) + 50)) + 250 - 25, Height - 519 - 10 - 4 + 4.75, "Holder", -Math.PI / 2));
                environment.Entities.Add(new BlockReference(0, 0, 0, "Foot", 0));
                environment.Entities.Add(new BlockReference(Width - LegWidth, 0, 0, "Foot", 0));
                environment.Entities.Add(new BlockReference(0, Depth - 40, 0, "Foot", 0));
                environment.Entities.Add(new BlockReference(Width - LegWidth, Depth - 40, 0, "Foot", 0));

                //change texture proportions
                int[] facesToScale = { 0, 2, 4, 5 };
                foreach (var face in facesToScale)
                {
#if NURBS
                    ((Brep.TessellationMesh)_top.Faces[face].Tessellation[0]).TextureScaleV *= 25.0f / (float)TableTopDepth; //y scale
#endif
                }

                //update top data display
                environment.Entities.Regen();

                // sets trimetric view
                environment.SetView(viewType.Trimetric);

                // fits the model in the viewport
                environment.ZoomFit();
            }

        }
        #endregion

        #region UserInput

        public void ChangeWeight()
        {
            var weights = CalcWeight();
            totalTextBox.Text = (Math.Round(weights[0]/1000)) + " kg";
            plasticTextBox.Text = Math.Round(weights[1]) + " g";
            woodTextBox.Text = Math.Round(weights[2]/1000) + " kg";
            steelTextBox.Text = Math.Round(weights[3]/1000) + " kg";
        }

        public void UpdateLinearDim()
        {
            if (tabControl1.SelectedIndex != 1) return;
            foreach (var sheet in drawingsUserControl1.drawings1.Sheets)
            {
                model1.Entities.UpdateBoundingBox();
                var ld1 = drawingsUserControl1.AddSampleDimSheet(sheet, false);
                for (int i = 0; i < sheet.Entities.Count; i++)
                {
                    if (sheet.Entities[i] is LinearDim)
                    {
                        sheet.Entities.RemoveAt(i--);
                    }
                }
                sheet.Entities.AddRange(ld1);
            }

            if (drawingsUserControl1.drawings1.Entities.Count <= 0) return;
            model1.Entities.UpdateBoundingBox();
            var s = drawingsUserControl1.drawings1.GetActiveSheet() ?? drawingsUserControl1.drawings1.Sheets[0];
            var ld = drawingsUserControl1.AddSampleDimSheet(s, false);
            for (int i = 0; i < drawingsUserControl1.drawings1.Entities.Count; i++)
            {
                if (drawingsUserControl1.drawings1.Entities[i] is LinearDim)
                {
                    drawingsUserControl1.drawings1.Entities.RemoveAt(i--);
                }
            }
            drawingsUserControl1.drawings1.Entities.AddRange(ld);
        }

        //Radio button Event Handler
        private void PaintColorClick(object sender, EventArgs e)
        {
            isDrawingToReload = true;
            var rb = (RadioButton)sender;
            ChangeColor(rb.Background, false);
            ReloadDrawings();
        }

        private void PaintColorFootsClick(object sender, EventArgs e)
        {
            isDrawingToReload = true;
            var rb = (RadioButton)sender;
            ChangeColor(rb.Background, true);
            ReloadDrawings();
        }
        
        private void ComboBoxVeneer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isDrawingToReload = true;
            if (_blockCallback) return;
            //change the tooltip
            switch (comboBoxVeneer.SelectedIndex)
            {
                case 1:
                    materialsToolTip.Content = "R20021";
                    break;
                case 2:
                    materialsToolTip.Content = "R20090";
                    break;
                case 3:
                    materialsToolTip.Content = "R20033";
                    break;
                case 4:
                    materialsToolTip.Content = "R20231";
                    break;
                case 5:
                    materialsToolTip.Content = "R20039";
                    break;
                default:
                    materialsToolTip.Content = "R20074";
                    break;
            }
            foreach (var block in model1.Blocks)
            {
                if (string.CompareOrdinal(block.Name, "Top") != 0) continue;
                foreach (var entity in block.Entities)
                {
#if  NURBS
                    if (!(entity is Brep)) continue;
#else
                    if (!(entity is Mesh)) continue;
#endif
                    //selected material
                    var selMat = "Oak Bordeaux bright";
                    var selectedMat = ((ComboBoxItem)e.AddedItems[0]).Content;
                    if (selectedMat != null)
                        selMat = selectedMat.ToString();
                    entity.MaterialName = selMat;
                }
            }

            model1.Entities.Regen();
            model1.Invalidate();
            ReloadDrawings();
        }

        private void ComboBoxFormat_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_blockCallback) return;
            isDrawingModified = true;
            _deskBuilder.SingleTable = comboBoxFormat.SelectedIndex == 1;
            ReloadScene();
        }

        private void ComboBoxDim_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_blockCallback) return;
            isDrawingModified = true;
            ReloadScene();
        }

        private void ComboBoxActiveObject_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_blockCallback) return;
            isDrawingModified = true;
            EnableImportExportButtons(false);
            model1.Entities.Clear();
            switch (comboBoxActiveObject.SelectedIndex)
            {
                case 1 : //Frame
                    model1.Entities.Add(new BlockReference(0, 0, 0, "Frame", 0));
                    break;
                case 2 : //Holder
                    model1.Entities.Add(new BlockReference(270, 10, 0, "Holder", Math.PI));
                    break;
                case 3 : //Top
                    model1.Entities.Add(new BlockReference(0, 0, 0, "Top", 0));
                    break;
                case 4 : //Foot
                    model1.Entities.Add(new BlockReference(0, 0, 0, "Foot", 0));
                    break;
                default : //display all
                    if (!_deskBuilder.SingleTable)
                    {
                        model1.Entities.Add(new BlockReference(0, _deskBuilder.Depth - _deskBuilder.TableTopDepth, _deskBuilder.Height + 4.75, "Top", 0));
                        model1.Entities.Add(new BlockReference(_deskBuilder.LegWidth + 4, _deskBuilder.Depth - ((((_deskBuilder.TableTopDepth / 4) + 50)) + 250 - 25), _deskBuilder.Height - 519 - 10 - 4 + 4.75, "Holder", Math.PI / 2));
                    }
                    model1.Entities.Add(new BlockReference(0, 0, 4.75, "Frame", 0));
                    model1.Entities.Add(new BlockReference(0, 0, _deskBuilder.Height + 4.75, "Top", 0));
                    model1.Entities.Add(new BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth - 4, (((_deskBuilder.TableTopDepth / 4) + 50)) + 250 - 25, _deskBuilder.Height - 519 - 10 - 4 + 4.75, "Holder", -Math.PI / 2));
                    model1.Entities.Add(new BlockReference(0, 0, 0, "Foot", 0));
                    model1.Entities.Add(new BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth, 0, 0, "Foot", 0));
                    model1.Entities.Add(new BlockReference(0, _deskBuilder.Depth - 40, 0, "Foot", 0));
                    model1.Entities.Add(new BlockReference(_deskBuilder.Width - _deskBuilder.LegWidth, _deskBuilder.Depth - 40, 0, "Foot", 0));

                    //change texture proportions
                    int[] facesToScale = { 0, 2, 4, 5 };
                    foreach (var face in facesToScale)
                    {
#if NURBS
                        ((Brep.TessellationMesh)_deskBuilder._top.Faces[face].Tessellation[0]).TextureScaleV *= 25.0f / (float)_deskBuilder.TableTopDepth; //y scale
#endif
                    }
                    break;
            }
            
            model1.Entities.Regen();
            model1.Invalidate();
            if (tabControl1.SelectedIndex == 0)
                model1.ZoomFit();

            //rebuild drawings
            ReloadDrawings();
            //re-enable buttons
            EnableImportExportButtons(true);
        }

        #endregion

        #region Event handlers

        public void EnableImportExportButtons(bool status)
        {
            //disable or enable all buttons, combobox, etc.
            openButton.IsEnabled = status;
            saveButton.IsEnabled = status;
            exportButton.IsEnabled = status;
            importButton.IsEnabled = status;
            explodeViewsCheckBox.IsEnabled = status;
            greenColorRadioButton.IsEnabled = status;
            pinkColorRadioButton.IsEnabled = status;
            blueColorRadioButton.IsEnabled = status;
            orangeColorRadioButton.IsEnabled = status;
            whiteColorRadioButton.IsEnabled = status;
            blackColorRadioButton.IsEnabled = status;
            comboBoxActiveObject.IsEnabled = status;
            comboBoxDepth.IsEnabled = status;
            comboBoxFormat.IsEnabled = status;
            comboBoxHeigth.IsEnabled = status;
            comboBoxVeneer.IsEnabled = status;
            comboBoxWidth.IsEnabled = status;
            tabControl1.IsEnabled = status;
            if (!status)
            {
                drawingsUserControl1.drawings1.ActionMode = actionType.None;
                drawingsUserControl1.drawings1.Cursor = Cursors.Wait;
            }
            else
            {
                drawingsUserControl1.drawings1.ActionMode = actionType.SelectVisibleByPick;
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            var exportFileDialog = new SaveFileDialog();
            exportFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" + "Drawing Exchange Format (*.dxf)|*.dxf";
            exportFileDialog.AddExtension = true;
            exportFileDialog.Title = "Export";
            exportFileDialog.CheckPathExists = true;
            var result = exportFileDialog.ShowDialog();
            if (result == true)
            {
                var explodeViews = explodeViewsCheckBox.IsChecked == true;
                WriteAutodeskParams wap = new WriteAutodeskParams(model1, drawingsUserControl1.drawings1, false, explodeViews);
                WriteAutodesk wa = new WriteAutodesk(wap, exportFileDialog.FileName);
                model1.StartWork(wa);

                EnableImportExportButtons(false);
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            var importFileDialog = new OpenFileDialog();
            importFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" + "Drawing Exchange Format (*.dxf)|*.dxf";
            importFileDialog.Multiselect = false;
            importFileDialog.AddExtension = true;
            importFileDialog.Title = "Import";
            importFileDialog.CheckFileExists = true;
            importFileDialog.CheckPathExists = true;
            var result = importFileDialog.ShowDialog();
            if (result == true)
            {
                model1.Clear();
                drawingsUserControl1.Clear();

                ReadAutodesk ra = new ReadAutodesk(importFileDialog.FileName);
                ra.SkipLayouts = false;
                model1.StartWork(ra);

                EnableImportExportButtons(false);
                imported = true;    
            }
        }

        private void Model1OnWorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            progressBar.Value = 100;

            if (e.WorkUnit is ReadFileAsyncWithDrawings)
            {
                var rfa = (ReadFileAsyncWithDrawings)e.WorkUnit;
                rfa.AddToScene(model1);
                model1.SetView(viewType.Trimetric, true, false);
                Drawings drawings = drawingsUserControl1.drawings1;

                model1.Units = rfa.Units;
                rfa.AddToDrawings(drawings);

                // If there are no sheets adds a default one to have a ready-to-use paper space.
                if (drawings.Sheets.Count == 0)
                    drawingsUserControl1.AddDefaultSheet();

                if (tabControl1.SelectedIndex == 0)
                {
                    model1.ZoomFit();
                    model1.Invalidate();
                }
                else
                {
                    drawings.Rebuild(model1, true, true);
                }

                EnableImportExportButtons(false);
                drawingsUserControl1.EnableUIElements(false);
                openButton.IsEnabled = true;
                saveButton.IsEnabled = true;
                exportButton.IsEnabled = true;
                importButton.IsEnabled = true;
                explodeViewsCheckBox.IsEnabled = true;
                tabControl1.IsEnabled = true;
            }
            else if (e.WorkUnit is DeskBuilder)
            {
                ChangeWeight();
                //set colors
                ChangeColor(_oldColor, false);
                ChangeColor(_oldColor, true);
                //rebuild drawings
                ReloadDrawings();
                //re-enable buttons
                EnableImportExportButtons(true);
                if (tabControl1.SelectedIndex == 0)
                    model1.ZoomFit();
            }

            progressBar.Value = 0;
        }

        private OpenFileAddOn _openFileAddOn;
        private void OpenButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Filter = "Eyeshot (*.eye)|*.eye";
                openFileDialog.Multiselect = false;
                openFileDialog.AddExtension = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;
                openFileDialog.DereferenceLinks = true;

                _openFileAddOn = new OpenFileAddOn();
                _openFileAddOn.EventFileNameChanged += OpenFileAddOn_EventFileNameChanged;

                if (openFileDialog.ShowDialog(_openFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    model1.Clear();
                    drawingsUserControl1.Clear();

                    ReadFile readFile = new ReadFile(openFileDialog.FileName, new FileSerializerEx((contentType)_openFileAddOn.ContentOption));
                    model1.StartWork(readFile);

                    EnableImportExportButtons(false);
                }

                _openFileAddOn.EventFileNameChanged -= OpenFileAddOn_EventFileNameChanged;
                _openFileAddOn.Dispose();
                _openFileAddOn = null;
            }
        }

        private void OpenFileAddOn_EventFileNameChanged(System.Windows.Forms.IWin32Window sender, string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                ReadFile rf = new ReadFile(filePath, true);
                _openFileAddOn.SetFileInfo(rf.GetThumbnail(), rf.GetFileInfo());
            }
            else
            {
                _openFileAddOn.ResetFileInfo();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            using (var saveDialogCtrl = new SaveFileAddOn())
            {
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye";
                saveFileDialog.AddExtension = true;
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.ShowHelp = true;

                if (saveFileDialog.ShowDialog(saveDialogCtrl, null) == System.Windows.Forms.DialogResult.OK)
                {
                    WriteFile writeFile = new WriteFile(new WriteFileParams(model1, drawingsUserControl1.drawings1) { Content = (contentType)saveDialogCtrl.ContentOption, SerializationMode = (serializationType)saveDialogCtrl.SerialOption, SelectedOnly = saveDialogCtrl.SelectedOnly, Purge = saveDialogCtrl.Purge }, saveFileDialog.FileName, new FileSerializerEx());
                    model1.StartWork(writeFile);

                    EnableImportExportButtons(false);
                }
            }
        }

        private void Model1_WorkCancelled(object sender, EventArgs e)
        {
            progressBar.Value = 0;
        }

        private void Model1_ProgressChanged(object sender, devDept.Eyeshot.ProgressChangedEventArgs e)
        {
            progressBar.Value = e.Progress;
        }


        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!imported && isDrawingModified)
                ReloadDrawings();
            else if(!imported && isDrawingToReload)
                drawingsUserControl1.drawings1.Rebuild(model1, true, false);
            else
            if (tabControl1.SelectedIndex == 1 && drawingsUserControl1.drawings1.Sheets.Count > 0)
                drawingsUserControl1.drawings1.Rebuild(model1, true, true);
        }

        #endregion

    }
}