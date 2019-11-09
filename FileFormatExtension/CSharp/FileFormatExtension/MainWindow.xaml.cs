using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Serialization;
using devDept.CustomControls;
using devDept.Eyeshot.Translators;
using Rotation = devDept.Geometry.Rotation;
using Block = devDept.Eyeshot.Block;
using EyeshotExtensions;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public bool AsyncRegen { get { return (bool)regenAsyncChk.IsChecked; } }

        public bool OpenSaveAsync { get { return (bool)asyncCheckBox.IsChecked; } }

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.            

            // Event handlers for async operations
            model1.WorkCompleted += new devDept.Eyeshot.Model.WorkCompletedEventHandler(model1_WorkCompleted);
            model1.WorkCancelled += new devDept.Eyeshot.Model.WorkCancelledEventHandler(model1_WorkCancelled);
            model1.WorkFailed += new devDept.Eyeshot.Model.WorkFailedEventHandler(model1_WorkFailed);
        }

        protected override void OnContentRendered(EventArgs e)
        { // Hides grid
            model1.GetGrid().Visible = false;

            // Sets display mode settings
            model1.DisplayMode = displayType.Rendered;
            model1.Rendered.ShowEdges = false;
            model1.Rendered.ShadowMode = shadowType.None;
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.LastFrame;
            model1.Rendered.PlanarReflections = true;
            model1.Rendered.EnvironmentMapping = true;

            // Adds line types
            string lineTypeDash = "Dash";
            model1.LineTypes.Add(lineTypeDash, new float[] { 0.2f, -0.15f });
            string lineTypeDashDot = "DashDot";
            model1.LineTypes.Add(lineTypeDashDot, new float[] { 0.5f, -0.15f, 0.025f, -0.15f });

            // Adds a block
            model1.Blocks.Add(BuildBlock());

            List<Entity> entities = new List<Entity>();

            // Creates mesh with texture
            entities.Add(BuildMeshWithTexture());

            // Adds a block reference
            BlockReference br = new BlockReference(8, -12, 0, "ArrowWithAttributes", Utility.DegToRad(60));
            br.Attributes["att1"] = new AttributeReference("Plate");
            br.Attributes["att2"] = new AttributeReference("With Holes");
            br.ColorMethod = colorMethodType.byEntity;
            br.Color = System.Drawing.Color.SandyBrown;
#if !OLDVER
            br.EntityData = new CustomData(1, 40) { Description = "This is a BlockRef with price" };
#else
            br.EntityData = new CustomData(1) { Description = "This is BlockRef" };
#endif
            entities.Add(br);

            // Creates MyCircle
            MyCircle myCircle = new MyCircle(Plane.XY, 3);
            myCircle.CustomDescription = "Custom circle with dash-dot";
            myCircle.ColorMethod = colorMethodType.byEntity;
            myCircle.Color = System.Drawing.Color.RoyalBlue;
            myCircle.LineTypeMethod = colorMethodType.byEntity;
            myCircle.LineTypeName = lineTypeDashDot;
            myCircle.TransformBy(new Rotation(Utility.DegToRad(45), Vector3D.AxisY) * new Translation(-5, 10, 0));
#if !OLDVER
            myCircle.EntityData = new CustomData(2, 25) { Description = "This is MyCircle with price" };
#else
            myCircle.EntityData = new CustomData(2) { Description = "This is MyCircle"};
#endif
            entities.Add(myCircle);

            // Creates a linearpath            
            LinearPath lp = new LinearPath(new[]
            {
                new Point3D(0, -1.25),
                new Point3D(1.875, -2.5),
                new Point3D(0.9375, -0.3125),
                new Point3D(2.5, 0.9375),
                new Point3D(0.625, 0.9375),
                new Point3D(0, 2.5),
                new Point3D(-0.625, 0.9375),
                new Point3D(-2.5, 0.9375),
                new Point3D(-0.9375, -0.3125),
                new Point3D(-1.875, -2.5),
                new Point3D(0, -1.25)
            });

            lp.ColorMethod = colorMethodType.byEntity;
            lp.Color = System.Drawing.Color.Green;
            lp.LineTypeMethod = colorMethodType.byEntity;
            lp.LineTypeName = lineTypeDash;
            lp.GlobalWidth = 0.1;
            lp.TransformBy(new Translation(3, 5, 0) * new Rotation(Utility.DegToRad(45), Vector3D.AxisX));
#if !OLDVER
            lp.EntityData = new CustomData(3, 30) { Description = "This is a LinearPath with price" };
#else
            lp.EntityData = new CustomData(3) { Description = "This is a LinearPath" };
#endif
            entities.Add(lp);

#if SOLID
            // Creates solid
            Solid cone = Solid.CreateCone(5, 2, 4, 30);
            cone.ColorMethod = colorMethodType.byEntity;
            cone.Color = System.Drawing.Color.FromArgb(124, System.Drawing.Color.Green);
            cone.Translate(3, 15, 2);
            entities.Add(cone);
#endif

#if NURBS
            // Creates solid3D
            entities.Add(BuildBrep());

            // Creates Surface
            entities.Add(BuildSurface());
#endif

            // Creates region
            entities.Add(BuildRegion());

            // adds all the entities to the scene
            model1.Entities.AddRange(entities);

            // sets trimetric view
            model1.SetView(viewType.Trimetric);

            // fits the model in the viewport
            model1.ZoomFit();

            base.OnContentRendered(e);
        }

        #region Async event handlers                

        private void model1_WorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            // Checks the WorkUnit type, more than one can be present in the same application 
            if (e.WorkUnit is WriteFile)
            {
                ShowLog("WriteFile log", ((WriteFile)e.WorkUnit).Log);
                SetButtonEnabled(true);
            }
            else if (e.WorkUnit is ReadFile)
            {
                ReadFile rf = (ReadFile)e.WorkUnit;
                AddToScene(rf);
                ShowLog(rf);
            }
            else if (e.WorkUnit is Regeneration)
            {
                model1.Entities.UpdateBoundingBox();
                EnableButtonsAndRefresh();
            }
        }

        private void model1_WorkFailed(object sender, WorkFailedEventArgs e)
        {
            SetButtonEnabled(true);
        }

        private void model1_WorkCancelled(object sender, EventArgs e)
        {
            SetButtonEnabled(true);
        }

        #endregion

        #region Helper methods

        private void SetButtonEnabled(bool value)
        {
            openButton.IsEnabled = saveButton.IsEnabled = value;
        }

        private bool _skipZoomFit = false;
        private void AddToScene(ReadFile rfa)
        {
            RegenOptions ro = new RegenOptions();
            ro.Async = AsyncRegen;

            rfa.AddToScene(model1, ro);

            _skipZoomFit = rfa.FileSerializer.FileBody.Camera != null;

            if (!AsyncRegen)
                EnableButtonsAndRefresh();
        }

        private void EnableButtonsAndRefresh()
        {
            if (!_skipZoomFit)
                model1.ZoomFit();
            SetButtonEnabled(true);
            model1.Invalidate();
        }

        private void ShowLog(ReadFile rf)
        {
            if (String.IsNullOrEmpty(rf.Log)) return;
            string msg = String.Format("{0}{1}Log{1}----------------------{1}{2}", rf.GetFileInfo(), System.Environment.NewLine, rf.Log);
            ShowLog("File info", msg);
        }

        private void ShowLog(string title, string log)
        {
            if (!String.IsNullOrEmpty(log))
            {
                var df = new DetailsWindow();

                df.contentTextBox.Text = String.Format("{0}{1}----------------------{1}{2}", title, System.Environment.NewLine, log);

                df.Show();
            }
        }

        #endregion        

        #region Build entities        
        private Mesh BuildMeshWithTexture()
        {
            Material mat = new Material("Marble", new Bitmap("../../../../../../dataset/Assets/Textures/Maple.jpg"));
            model1.Materials.Add(mat);

            const double tol = 0.01;       // chordal error
            const double radius = 0.4;      // holes radius

            // Plate with holes            
            ICurve profile = new CompositeCurve(new Line(0, 0, 8, 0),
                new Arc(new Point3D(8, 2, 0), 2, 6 * Math.PI / 4, 2 * Math.PI),
                new Line(10, 2, 10, 6),
                new Line(10, 6, 0, 6),
                new Line(0, 6, 0, 0));

            ICurve[] holes = new ICurve[20];

            int count = 0;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    holes[count] = new Circle(new Point3D(1 + x * 1, 1 + y * 1, 0), radius);
                    holes[count].Reverse(); // holes have wrong orientation, we must reverse them
                    count++;

                }
            }

            List<ICurve> contours = new List<ICurve>() { profile };
            contours.AddRange(holes);
            devDept.Eyeshot.Entities.Region region = new devDept.Eyeshot.Entities.Region(contours, Plane.XY, false);
            Mesh plate = region.ExtrudeAsMesh(new Vector3D(0, 0, 0.25), tol, Mesh.natureType.RichSmooth);
            plate.FlipNormal();
            plate.NormalAveragingMode = Mesh.normalAveragingType.AveragedByAngle;
            plate.ApplyMaterial(mat.Name, textureMappingType.Cubic, 1, 1);
            plate.Translate(10, 0, 0);
            return plate;
        }

#if NURBS
        private Brep BuildBrep()
        {
            double height = 12;
            double radius = 4;
            double offset = 1.5;

            Point3D[] vertices = new Point3D[4];

            vertices[0] = new Brep.Vertex(radius, 0, 0);
            vertices[1] = new Brep.Vertex(radius, 0, height);

            Brep.Edge[] edges = new Brep.Edge[6];
            Brep.Face[] faces = new Brep.Face[3];

            Circle c1 = new Circle(Plane.XY, radius);
            edges[0] = new Brep.Edge(c1, 0, 0);

            Circle c2 = new Circle(Plane.XY, radius);
            c2.Translate(0, 0, height);
            edges[1] = new Brep.Edge(c2, 1, 1);

            Line l1 = new Line((Point3D)vertices[0].Clone(), (Point3D)vertices[1].Clone());

            edges[2] = new Brep.Edge(l1, 0, 1);

            Brep.OrientedEdge[] bottomLoop = new Brep.OrientedEdge[1];
            bottomLoop[0] = new Brep.OrientedEdge(0, false);


            Brep.OrientedEdge[] sideLoop = new Brep.OrientedEdge[4];

            sideLoop[0] = new Brep.OrientedEdge(0);
            sideLoop[1] = new Brep.OrientedEdge(2);
            sideLoop[2] = new Brep.OrientedEdge(1, false);
            sideLoop[3] = new Brep.OrientedEdge(2, false);


            Brep.OrientedEdge[] topLoop = new Brep.OrientedEdge[1];
            topLoop[0] = new Brep.OrientedEdge(1);


            PlanarSurf top = new PlanarSurf(vertices[1], Vector3D.AxisZ, Vector3D.AxisX);
            CylindricalSurf side = new CylindricalSurf(Point3D.Origin, Vector3D.AxisZ, Vector3D.AxisX, radius);
            PlanarSurf bottom = new PlanarSurf(vertices[0], Vector3D.AxisZ, Vector3D.AxisX);

            faces[0] = new Brep.Face(bottom, new Brep.Loop(bottomLoop), false);
            faces[1] = new Brep.Face(side, new Brep.Loop(sideLoop));
            faces[2] = new Brep.Face(top, new Brep.Loop(topLoop));


            Brep solid3D = new Brep(vertices, edges, faces);

            // Inner void
            vertices[2] = new Brep.Vertex(radius - offset, 0, offset);
            vertices[3] = new Brep.Vertex(radius - offset, 0, height - offset);

            Circle c11 = new Circle(new Point3D(0, 0, offset), radius - offset);
            edges[3] = new Brep.Edge(c11, 2, 2);

            Circle c22 = new Circle(new Point3D(0, 0, height - offset), radius - offset);
            edges[4] = new Brep.Edge(c22, 3, 3);

            Line l7 = new Line((Point3D)vertices[3].Clone(), (Point3D)vertices[2].Clone());
            edges[5] = new Brep.Edge(l7, 2, 3);

            Brep.OrientedEdge[] voidBottomLoop = new Brep.OrientedEdge[]
            {
                new Brep.OrientedEdge(3)
            };

            Brep.OrientedEdge[] voidSideLoop = new Brep.OrientedEdge[]
            {
                new Brep.OrientedEdge(3, false),
                new Brep.OrientedEdge(5),
                new Brep.OrientedEdge(4),
                new Brep.OrientedEdge(5, false)
            };

            Brep.OrientedEdge[] voidTopLoop = new Brep.OrientedEdge[]
            {
                new Brep.OrientedEdge(4, false)
            };

            PlanarSurf voidBottom = new PlanarSurf(vertices[2], Vector3D.AxisZ, Vector3D.AxisX);
            CylindricalSurf voidSide = new CylindricalSurf((Point3D)c11.Center.Clone(), Vector3D.AxisZ, Vector3D.AxisX, radius - offset);
            PlanarSurf voidTop = new PlanarSurf(vertices[3], Vector3D.AxisZ, Vector3D.AxisX);

            Brep.Face[] innerVoid = new Brep.Face[]
            {
                new Brep.Face(voidBottom, new Brep.Loop(voidBottomLoop)),
                new Brep.Face(voidSide, new Brep.Loop(voidSideLoop), false),
                new Brep.Face(voidTop, new Brep.Loop(voidTopLoop), false)
            };

            solid3D.Inners = new Brep.Face[][] { innerVoid };

            solid3D.ColorMethod = colorMethodType.byEntity;
            solid3D.Color = System.Drawing.Color.FromArgb(124, System.Drawing.Color.Red);
            solid3D.Translate(28, 15, -5);

            return solid3D;
        }

        public Surface BuildSurface()
        {
            List<Point3D> array = new List<Point3D>();

            array.Add(new Point3D(0, 0, 0));
            array.Add(new Point3D(0, 2, 1.5));
            array.Add(new Point3D(0, 4, 0));
            array.Add(new Point3D(0, 6, 0.5));

            Curve firstU = Curve.GlobalInterpolation(array, 3);

            array.Clear();
            array.Add(new Point3D(4, 0, 1));
            array.Add(new Point3D(4, 3, 1.5));
            array.Add(new Point3D(4, 5, 0));

            Curve secondU = Curve.GlobalInterpolation(array, 2);

            array.Clear();
            array.Add(new Point3D(8, 0, 0));
            array.Add(new Point3D(8, 3, 2));
            array.Add(new Point3D(8, 6, 0));

            Curve thirdU = Curve.GlobalInterpolation(array, 2);

            array.Clear();
            array.Add(new Point3D(0, 0, 0));
            array.Add(new Point3D(4, 0, 1));
            array.Add(new Point3D(8, 0, 0));

            Curve firstV = Curve.GlobalInterpolation(array, 2);

            array.Clear();
            array.Add(new Point3D(0, 6, 5));
            array.Add(new Point3D(4, 5, 0));
            array.Add(new Point3D(6, 4, 0));
            array.Add(new Point3D(8, 6, 0));

            Curve secondV = Curve.GlobalInterpolation(array, 2);


            Point3D[,] ptGrid = new Point3D[3, 2];

            ptGrid[0, 0] = new Point3D(0, 0, 0);
            ptGrid[0, 1] = new Point3D(0, 6, 0.5);

            ptGrid[1, 0] = new Point3D(4, 0, 1);
            ptGrid[1, 1] = new Point3D(4, 5, 0);

            ptGrid[2, 0] = new Point3D(8, 0, 0);
            ptGrid[2, 1] = new Point3D(8, 6, 0);


            Surface surface = Surface.Gordon(new Curve[] { firstU, secondU, thirdU }, new Curve[] { firstV, secondV }, ptGrid);
            surface.ColorMethod = colorMethodType.byEntity;
            surface.Color = System.Drawing.Color.MediumPurple;

            surface.Translate(24, -4, 0);

            return surface;

        }
#endif

        private devDept.Eyeshot.Entities.Region BuildRegion()
        {
            // Face
            Circle outer = new Circle(Point3D.Origin, 6);

            // Eyes
            Ellipse inner1 = new Ellipse(new Point3D(-2, 2, 0), 0.5, 1);
            Ellipse inner2 = new Ellipse(new Point3D(2, 2, 0), 0.5, 1);

            // Mouth
            Circle circle = new Circle(Point3D.Origin, 4);
            Point3D i1;
            Point3D i2;
            CompositeCurve.IntersectionLineCircle3D(new Line(-10, 0, 10, 0), circle, out i1, out i2);
            Arc arc1 = new Arc(new Point3D(0, 2, 0), i1, i2);
            ICurve[] segments;
            circle.SplitBy(new List<Point3D> { i1, i2 }, out segments);
            CompositeCurve inner3 = new CompositeCurve(new List<ICurve>() { arc1, segments[1] });

            // Smile region
            devDept.Eyeshot.Entities.Region reg = new devDept.Eyeshot.Entities.Region(new List<ICurve> { outer, inner1, inner2, inner3 });
            reg.ColorMethod = colorMethodType.byEntity;
            reg.Color = System.Drawing.Color.FromArgb(255, 180, 30);
            reg.TransformBy(new Translation(12, 25, 0) * new Rotation(Math.PI / 2, Vector3D.AxisX));

            return reg;

        }
        private Block BuildBlock()
        {
            Block myBlock = new Block("ArrowWithAttributes");

            Line l = new Line(0, 0, 10, 0);
            Triangle t = new Triangle(new Point3D(0, -1.5), new Point3D(0, 1.5), new Point3D(1.5, 0));
            t.Translate(10, 0, 0);
            l.ColorMethod = t.ColorMethod = colorMethodType.byParent;

            devDept.Eyeshot.Entities.Attribute a1 = new devDept.Eyeshot.Entities.Attribute(5, 0.2, 0, "att1", "Top Text", 0.8);
            a1.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BottomCenter;

            devDept.Eyeshot.Entities.Attribute a2 = new devDept.Eyeshot.Entities.Attribute(5, -0.4, 0, "att2", "Bottom Text", 0.8);
            a2.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.TopCenter;

            l.ColorMethod =
                t.ColorMethod =
                    a1.ColorMethod =
                        a2.ColorMethod = colorMethodType.byParent;

            myBlock.Entities.Add(l);
            myBlock.Entities.Add(t);
            myBlock.Entities.Add(a1);
            myBlock.Entities.Add(a2);

            return myBlock;
        }

        #endregion

        private OpenFileAddOn _openFileAddOn;
        private void openButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog1.Filter = "Eyeshot (*.eye)|*.eye";
                openFileDialog1.Multiselect = false;
                openFileDialog1.AddExtension = true;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;
                openFileDialog1.DereferenceLinks = true;
                openFileDialog1.ShowHelp = true;


                _openFileAddOn = new OpenFileAddOn();
                _openFileAddOn.EventFileNameChanged += OpenFileAddOn_EventFileNameChanged;

                if (openFileDialog1.ShowDialog(_openFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    model1.Clear();
                    ReadFile readFile = new ReadFile(openFileDialog1.FileName, false, (contentType)_openFileAddOn.ContentOption);
                    if (OpenSaveAsync)
                    {
                        model1.StartWork(readFile);
                        SetButtonEnabled(false);
                    }
                    else
                    {
                        readFile.DoWork();
                        AddToScene(readFile);
                        ShowLog(readFile);
                    }
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

        private void saveButton_Click(object sender, EventArgs e)
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
                    WriteFile writeFile = new WriteFile(
                        new WriteFileParams(model1)
                        {
                            Content = (contentType)saveDialogCtrl.ContentOption,
                            SerializationMode = (serializationType)saveDialogCtrl.SerialOption,
                            SelectedOnly = saveDialogCtrl.SelectedOnly,
                            Purge = saveDialogCtrl.Purge,
                            Tag = MyFileSerializer.CustomTag
                        }, saveFileDialog.FileName, new MyFileSerializer());

                    if (OpenSaveAsync)
                    {
                        model1.StartWork(writeFile);
                        SetButtonEnabled(false);
                    }
                    else
                    {
                        writeFile.DoWork();
                        ShowLog("WriteFile log", writeFile.Log);
                    }
                }
            }
        }

        private actionType _prevAction = actionType.None;
        private void selectChk_CheckedChanged(object sender, EventArgs e)
        {


            if (selectChk.IsChecked == true)
            {
                _prevAction = model1.ActionMode;
                model1.ActionMode = actionType.SelectVisibleByPick;
            }
            else
            {
                model1.ActionMode = _prevAction;
            }
        }

        private void dumpButton_Click(object sender, EventArgs e)
        {
            Entity[] entList = model1.Entities.ToArray();

            for (int i = 0; i < entList.Length; i++)
            {
                Entity ent = entList[i];

                DetailsWindow df;

                StringBuilder sb = new StringBuilder();
#if NURBS
                if (ent is Brep)
                {
                    Brep solid3D = (Brep)ent;

                    switch (model1.SelectionFilterMode)
                    {
                        case selectionFilterType.Vertex:
                            for (int j = 0; j < solid3D.Vertices.Length; j++)
                            {
                                Brep.Vertex sv = (Brep.Vertex)solid3D.Vertices[j];

                                if (solid3D.GetVertexSelection(j))
                                {
                                    sb.AppendLine("Vertex ID: " + j);
                                    sb.AppendLine(sv.ToString());
                                    sb.AppendLine("----------------------");
                                    sb.Append(sv.Dump());
                                    break;
                                }
                            }
                            break;

                        case selectionFilterType.Edge:
                            for (int j = 0; j < solid3D.Edges.Length; j++)
                            {
                                Brep.Edge se = solid3D.Edges[j];

                                if (solid3D.GetEdgeSelection(j))
                                {
                                    sb.AppendLine("Edge ID: " + j);
                                    sb.AppendLine(se.ToString());
                                    sb.AppendLine("----------------------");
                                    sb.Append(se.Dump());
                                    break;
                                }
                            }
                            break;

                        case selectionFilterType.Face:

                            for (int j = 0; j < solid3D.Faces.Length; j++)
                            {
                                Brep.Face sf = solid3D.Faces[j];

                                if (solid3D.GetFaceSelection(j))
                                {
                                    sb.AppendLine("Face ID: " + j);
                                    sb.AppendLine(sf.Surface.ToString());
                                    sb.AppendLine("----------------------");
                                    sb.Append(sf.Dump());
                                    break;
                                }
                            }
                            break;
                    }

                    if (sb.Length > 0)
                    {
                        df = new DetailsWindow();

                        df.contentTextBox.Text = sb.ToString();

                        df.Show();
                        return;
                    }
                }
#endif
                if (ent.Selected)
                {
                    sb.AppendLine("Entity ID: " + i);

                    sb.Append(ent.Dump());

                    df = new DetailsWindow();

                    df.contentTextBox.Text = sb.ToString();

                    df.Show();

                    break;
                }
            }
        }

        private void statsButton_Click(object sender, EventArgs e)
        {
            DetailsWindow rf = new DetailsWindow();
            rf.Title = "Statistics";
            rf.contentTextBox.Text = model1.Entities.GetStats(true, true, true);
            rf.Show();
        }
    }
}