using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Graphics;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Threading;

namespace PerformanceStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const double COLUMN_B = 30;
        const double COLUMN_H = 30;
        const double COLUMN_L = 300;
        const double BEAM_B = 30;
        const double BEAM_H = 60;
        const double BEAM_L = 500;
        const double SHELL_TICKNESS = 30;
        const double TEXT_PAD = 5;
        const double TEXT_HEIGHT = 10;

        public enum structureType
        {
            Assembly = 0,
            Flattened = 1,
            SingleMesh = 2
        }

        private EntityList _entityList = new EntityList();
        private int _cols, _beams;
        private int _bayXValue = 5;
        private int _bayYValue = 5;
        private int _floorsValue = 5;
        private int _shellSubValue = 3;
        private Mesh _buildingMesh;
        private string _bricks = "Bricks";
        private string _concreteMatName = "Concrete";
        private string _wallMatName = "wallMat";
        private bool _treeModify;

        public MainWindow(string renderer)
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.
            
            switch (renderer)
            {
                case "DirectX":
                    model1.Renderer = rendererType.Direct3D;
                    shadersCheckBox.IsEnabled = false;
                    depthCheckBox.IsEnabled = false;
                    break;
                case "Native":
                    model1.Renderer = rendererType.Native;
                    shadersCheckBox.IsEnabled = false;
                    depthCheckBox.IsEnabled = false;
                    break;
                case "OpenGL":
                    model1.Renderer = rendererType.OpenGL;
                    break;
            }

            model1.AskForAntiAliasing = true;
            model1.AntiAliasing = false;
            model1.DisplayMode = displayType.Rendered;
            model1.GetGrid().Visible = false;
            model1.Rendered.PlanarReflections = false;
            model1.Rendered.ShadowMode = shadowType.Realistic;
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            model1.Shaded.ShadowMode = shadowType.Realistic;
            model1.Shaded.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            model1.ShowFps = true;
            
            floors.ValueChanged += numeric_ValueChanged;
            bayX.ValueChanged += numeric_ValueChanged;
            bayY.ValueChanged += numeric_ValueChanged;
            shellSubdivisions.ValueChanged += numeric_ValueChanged;

            // Listens the events to handle the tree synchronization
            model1.MouseDown += Model1_MouseDown;
            model1.MouseLeftButtonDown += Model1_MouseLeftButtonDown;
            model1.MouseRightButtonDown += Model1_MouseRightButtonDown;
            treeView1.SelectedItemChanged += TreeView1_SelectedItemChanged;

            // Listens the events to handle the deletion of the selected entity
            model1.KeyDown += Model1_KeyDown;
            treeView1.KeyDown += TreeView1_KeyDown;
            // Sets default values
            displayModeEnumButton.Set(displayType.Rendered);
            shadowModeEnumButton.Set(model1.Rendered.ShadowMode);
            structureModeEnumButton.Set(structureType.Assembly);            

            UpdateCounters();

            _clickTimer.Interval = TimeSpan.FromMilliseconds(300);
            _clickTimer.Tick += ClickTimer;
        }
        private enum MouseClickType
        {
            LeftClick,
            LeftDoubleClick,
            RightClick
        }
        protected override void OnContentRendered(EventArgs e)
        {
            _treeModify = false;

            base.OnContentRendered(e);
            model1.ActionMode = actionType.SelectVisibleByPick;

            model1.Materials.Add(new Material(_concreteMatName, System.Drawing.Color.FromArgb(25, 25, 25), System.Drawing.Color.LightGray, System.Drawing.Color.FromArgb(31, 31, 31), .05f, .05f));
            model1.Materials.Add(new Material(_wallMatName, System.Drawing.Color.FromArgb(100, 25, 150, 25)));
            model1.Materials.Add(new Material(_bricks, new Bitmap("../../../../../../dataset/Assets/Textures/Bricks.jpg")));

            UpdateViewport();
            BuildAssembly();

            // fits the model in the viewport
            model1.ZoomFit();
        }

        #region Mouse buttons handlers
        private readonly System.Windows.Threading.DispatcherTimer _clickTimer = new DispatcherTimer();
        private bool _singleClick;
        private System.Drawing.Point _mouseLocation;
        private void Model1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Gets the mouse location
            _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));
        }
        private void ClickTimer(object sender, EventArgs e)
        {
            if (_singleClick)
            {
                StopTimer();
                Debug.WriteLine("Single click");
                // Selects the entity under the mouse     
                Selection(MouseClickType.LeftClick);
            }
        }

        private void StopTimer()
        {
            _clickTimer.Stop();
            _singleClick = false;
        }

        private void Model1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model1.ActionMode != actionType.SelectVisibleByPick) return;

            _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));

            if (model1.GetMouseClicks(e) == 2)
            {
                StopTimer();
                Debug.WriteLine("Double click");
                // Sets the BlockReference as current (so I can select its entities with one click).
                Selection(MouseClickType.LeftDoubleClick);

            }
            else
            {
                _singleClick = true;
                _clickTimer.Start();
                Selection(MouseClickType.LeftClick);
            }
        }

        private void Model1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (model1.ActionMode != actionType.None) return;

            _mouseLocation = devDept.Graphics.RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));

            Debug.WriteLine("Right click");
            // Sets the parent's BlockReference as current (so I can select the parent's entities with one click).
            Selection(MouseClickType.RightClick);
        }

        #endregion

        private Model.SelectedItem lastSelectedItem;
        private void Selection(MouseClickType mouseClickType)
        {
            if (_treeModify)
                return;

            _treeModify = true;

            if (mouseClickType == MouseClickType.RightClick)
            {
                // Sets the parent of the current BlockReference as current.
                model1.Entities.SetParentAsCurrent();
            }
            else
            {
                // Deselects the previously selected item
                if (lastSelectedItem != null)
                {
                    lastSelectedItem.Select(model1, false);
                    lastSelectedItem = null;
                }

                var item = model1.GetItemUnderMouseCursor(_mouseLocation);

                if (item != null)
                {
                    lastSelectedItem = item;

                    TreeViewUtility.CleanCurrent(model1, false);

                    // Marks as selected the entity under the mouse cursor.
                    item.Select(model1, true);
                }
                else
                {
                    // Back to the root level                
                    if (mouseClickType == MouseClickType.LeftDoubleClick)
                        TreeViewUtility.CleanCurrent(model1);
                }
            }

            // An entity in the viewport was selected, so we highlight the corresponding element in the treeview as well                        
            TreeViewUtility.SynchScreenSelection(treeView1, new Stack<BlockReference>(model1.Entities.Parents), lastSelectedItem);

            model1.Invalidate();

            _treeModify = false;
        }
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_treeModify)
            {
                return;
            }

            _treeModify = true;

            //An element of the treeview was selected, so we select the corresponding viewport element as well

            if (lastSelectedItem != null)
                lastSelectedItem.Select(model1, false);

            TreeViewUtility.CleanCurrent(model1);
            lastSelectedItem = TreeViewUtility.SynchTreeSelection(treeView1, model1);

            model1.Invalidate();

            _treeModify = false;
        }

        #region Selected entity deletion
        private void TreeView1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Model1_KeyDown(sender, e);
        }

        private void Model1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                TreeNode selectedNode = (TreeNode)treeView1.SelectedItem;
                if (selectedNode != null)
                {
                    if (lastSelectedItem != null && lastSelectedItem.Item != null && lastSelectedItem.Item is BlockReference)
                    {
                        var br = lastSelectedItem.Item as BlockReference;

                        if (selectedNode.ParentNode != null)
                        {
                            var parent = selectedNode.ParentNode.Tag as BlockReference;

                            var parentBlockName = parent.BlockName;

                            foreach (var b in model1.Blocks)
                            {
                                if (b.Name == parentBlockName)
                                {
                                    Entity toDelete = null;

                                    foreach (var ent in b.Entities)
                                        if (ent == br)
                                            toDelete = ent;

                                    if (toDelete != null)
                                        b.Entities.Remove(toDelete);
                                }
                            }
                        }
                        else // Root entity is to delete
                        {
                            treeView1.Items.Remove(selectedNode);

                            model1.Entities.Remove(br);
                            model1.Invalidate();
                        }
                    }
                    else if (lastSelectedItem.Item is Entity)
                    {
                        var entity = lastSelectedItem.Item as Entity;

                        foreach (var b in model1.Blocks)
                            if (b.Entities.Contains(entity))
                                b.Entities.Remove(entity);

                        model1.Entities.DeleteSelected(); // in case the entity to delete is the root entity
                    }

                    TreeViewUtility.DeleteSelectedNode(treeView1, model1);
                    treeView1.Items.Remove(selectedNode);
                }
            }
        }
        #endregion        

        #region NumericUpDowns Handler
        private void numeric_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _bayXValue = (int)bayX.Value;
            _bayYValue = (int)bayY.Value;
            _floorsValue = (int)floors.Value;
            _shellSubValue = (int)shellSubdivisions.Value;
            UpdateViewport();
            BuildAssembly();
            UpdateCounters();
            model1.Invalidate();
        }
        #endregion

        #region Button Handlers

        private string[] checkBoxesThatChangeAssembly = new string[] {
                "transparencyCheckBox","textureCheckBox","pillarsCheckBox","labelCheckBox","showBeamXCheckBox","showBeamYCheckBox","shellCheckBox","nodesCheckBox"
            };
        private void checkBoxes_CheckedChanged(object sender, EventArgs e)
        {
            UpdateViewport();

            var checkBox = (System.Windows.Controls.Primitives.ToggleButton)sender;

            if (checkBoxesThatChangeAssembly.Contains(checkBox.Name))
                BuildAssembly();
            UpdateCounters();

            model1.Invalidate();
        }

        private void displayModeEnumButton_Click(object sender, EventArgs e)
        {
            model1.DisplayMode = (displayType)displayModeEnumButton.Value;
            model1.Invalidate();
        }

        private void shadowModeEnumButton_Click(object sender, EventArgs e)
        {
            model1.Rendered.ShadowMode = (shadowType)shadowModeEnumButton.Value;
            model1.Shaded.ShadowMode = (shadowType)shadowModeEnumButton.Value;
            model1.Invalidate();
        }

        private void structureModeEnumButton_Click(object sender, EventArgs eventArgs)
        {
            UpdateViewport();
            BuildAssembly();
        }

        private void rendererButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = Process.GetCurrentProcess().StartInfo;

            startInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location; //  Application.Current.StartupUri.LocalPath; //ExecutablePath;
            var exit = typeof(Application).GetMethod("ExitInternal",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Static);

            if (rendererButton.Content.Equals("Native"))
            {
                startInfo.Arguments = "OpenGL";
            }
            else if (rendererButton.Content.Equals("OpenGL"))
            {
                startInfo.Arguments = "DirectX";
            }
            else if (rendererButton.Content.Equals("DirectX"))
            {
                startInfo.Arguments = "Native";
            }
            MessageBoxResult mBoxResult = System.Windows.MessageBox.Show(this, "Switching renderer to " + startInfo.Arguments.ToString() +
                                                 " requires an application restart. Do you wish to proceed?", "Renderer",
                MessageBoxButton.OKCancel);
            if (mBoxResult == MessageBoxResult.OK)
            {
                Application.Current.Shutdown();
                Process.Start(startInfo);
            }
        }

        #endregion

        private void UpdateCounters()
        {
            if (pillarsCheckBox.IsChecked == true)
            {
                _cols = (_bayXValue + 1) * (_bayYValue + 1) * _floorsValue;
            }
            else
            {
                _cols = 0;
            }

            int beamsX = (showBeamXCheckBox.IsChecked == true) ? _bayXValue * (_bayYValue + 1) : 0;
            int beamsY = (showBeamYCheckBox.IsChecked == true) ? _bayYValue * (_bayXValue + 1) : 0;
            _beams = (beamsX + beamsY) * _floorsValue;
            lColumns.Content = _cols.ToString();
            lBeams.Content = _beams.ToString();

            if (nodesCheckBox.IsChecked == true)
            {
                lJoints.Content = ((_bayXValue + 1) * (_bayYValue + 1) * (_floorsValue + 1)).ToString();
            }
            else
            {
                lJoints.Content = "0";
            }

            if (shellCheckBox.IsChecked == true)
            {
                lShell.Content =
                (2 * _floorsValue * _bayXValue * _shellSubValue * _shellSubValue +
                 2 * _floorsValue * _bayYValue * _shellSubValue * _shellSubValue).ToString();
            }
            else
            {
                lShell.Content = "0";
            }
        }

        private void UpdateViewport()
        {
            model1.AntiAliasing = antiAliasingCheckBox.IsChecked == true ? true : false;
            model1.Rendered.PlanarReflections = planarCheckBox.IsChecked == true ? true : false;
            model1.Rendered.EnvironmentMapping = environmentMappingCheckBox.IsChecked == true ? true : false;
            model1.UseShaders = shadersCheckBox.IsChecked == true ? true : false;
            model1.WriteDepthForTransparents = depthCheckBox.IsChecked == true ? true : false;

            model1.Flat.ShowEdges = edgesCheckBox.IsChecked == true ? true : false;
            model1.HiddenLines.ShowEdges = edgesCheckBox.IsChecked == true ? true : false;
            model1.Rendered.ShowEdges = edgesCheckBox.IsChecked == true ? true : false;
            model1.Shaded.ShowEdges = edgesCheckBox.IsChecked == true ? true : false;
            model1.Wireframe.ShowEdges = edgesCheckBox.IsChecked == true ? true : false;
        }

        private void BuildAssembly()
        {
            model1.AntiAliasing = (antiAliasingCheckBox.IsChecked == true) ? true : false;
            model1.Rendered.PlanarReflections = (planarCheckBox.IsChecked == true) ? true : false;
            model1.UseShaders = (shadersCheckBox.IsChecked == true) ? true : false;
            model1.WriteDepthForTransparents = (depthCheckBox.IsChecked == true) ? true : false;
            model1.Flat.ShowEdges = (edgesCheckBox.IsChecked == true) ? true : false;
            model1.HiddenLines.ShowEdges = (edgesCheckBox.IsChecked == true) ? true : false;
            model1.Rendered.ShowEdges = (edgesCheckBox.IsChecked == true) ? true : false;
            model1.Shaded.ShowEdges = (edgesCheckBox.IsChecked == true) ? true : false;
            model1.Wireframe.ShowEdges = (edgesCheckBox.IsChecked == true) ? true : false;

            _entityList.Clear();
            model1.Entities.Clear();
            model1.Blocks.Clear();

            if (model1.Materials.Count > 0)
            {
                if (!transparencyCheckBox.IsChecked == true)
                {
                    model1.Materials[_wallMatName].Diffuse = System.Drawing.Color.FromArgb(25, 150, 25);
                }
                else
                {
                    model1.Materials[_wallMatName].Diffuse = System.Drawing.Color.FromArgb(100, 25, 150, 25);
                }
            }

            // Variables for unique Mesh (SingleMesh)
            List<Point3D> globalVerts = new List<Point3D>();
            List<IndexTriangle> globalTris = new List<IndexTriangle>();
            int offset = globalVerts.Count;

            // Pillar column block
            devDept.Eyeshot.Block column = new devDept.Eyeshot.Block("squareCol");
            // creates a gray box
            Mesh m1 = Mesh.CreateBox(COLUMN_B, COLUMN_H, COLUMN_L);

            // apply texture if Texture is true
            if (textureCheckBox.IsChecked == true)
            {
                m1.ApplyMaterial("Bricks", textureMappingType.Cubic, 1, 1);
            }
            else
            {
                m1.ColorMethod = colorMethodType.byEntity;
                m1.Color = System.Drawing.Color.LightGray;
                m1.MaterialName = _concreteMatName;
            }

            for (int i = 0; i < m1.Vertices.Length; i++)
                globalVerts.Add(m1.Vertices[i]);
            for (int i = 0; i < m1.Triangles.Length; i++)
                globalTris.Add(new ColorTriangle(offset + m1.Triangles[i].V1, offset + m1.Triangles[i].V2,
                    offset + m1.Triangles[i].V3, System.Drawing.Color.Gray));

            Plane p = new Plane(new Vector3D(0, 1, 0));
            devDept.Eyeshot.Entities.Attribute at = new devDept.Eyeshot.Entities.Attribute(p,
                new Point3D(-(TEXT_HEIGHT + TEXT_PAD), COLUMN_B / 2, COLUMN_L / 2), "Name", "Frame", TEXT_HEIGHT);
            at.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BaselineCenter;
            at.UpsideDown = true;
            column.Entities.Add(at);

            column.Entities.Add(m1);

            // adds the block to the master block dictionary
            model1.Blocks.Add(column);

            BlockReference reference;

            // Beam
            devDept.Eyeshot.Block beam = new devDept.Eyeshot.Block("beam");
            // creates a gray box
            Mesh m2 = Mesh.CreateBox(BEAM_B, BEAM_L, BEAM_H);
            m2.ColorMethod = colorMethodType.byEntity;
            m2.Color = System.Drawing.Color.LightGray;
            m2.MaterialName = _concreteMatName;

            offset = globalVerts.Count;
            for (int i = 0; i < m2.Vertices.Length; i++)
                globalVerts.Add(m2.Vertices[i]);
            for (int i = 0; i < m2.Triangles.Length; i++)
                globalTris.Add(new ColorTriangle(offset + m2.Triangles[i].V1, offset + m2.Triangles[i].V2,
                    offset + m2.Triangles[i].V3, System.Drawing.Color.Gray));

            beam.Entities.Add(m2);

            p = new Plane(new Vector3D(1, 0, 0));
            at = new devDept.Eyeshot.Entities.Attribute(p, new Point3D(BEAM_B / 2, BEAM_L / 2, BEAM_H + TEXT_PAD), "Name",
                "Frame", TEXT_HEIGHT);
            at.Alignment = devDept.Eyeshot.Entities.Text.alignmentType.BaselineCenter;
            at.Color = System.Drawing.Color.Green;
            at.ColorMethod = colorMethodType.byEntity;
            beam.Entities.Add(at);

            // adds the block to the master block dictionary
            model1.Blocks.Add(beam);

            // Shell
            devDept.Eyeshot.Block shell = new devDept.Eyeshot.Block("shell");
            double shellB = BEAM_L / _shellSubValue;
            double shellH = COLUMN_L / _shellSubValue;

            // Mesh
            Mesh m3 = Mesh.CreateBox(shellB, SHELL_TICKNESS, shellH);
            m3.ColorMethod = colorMethodType.byEntity;
            m3.Color = System.Drawing.Color.LightGreen;
            m3.MaterialName = "wallMat";
            shell.Entities.Add(m3);

            // adds the block to the master block dictionary
            model1.Blocks.Add(shell);

            for (int k = 0; k < _floorsValue; k++)
            {
                for (int j = 0; j <= _bayYValue; j++)
                {
                    for (int i = 0; i <= _bayXValue; i++)
                    {
                        if (pillarsCheckBox.IsChecked == true)
                        {
                            reference = new BlockReference(i * BEAM_L - COLUMN_B / 2, j * BEAM_L - COLUMN_H / 2, k * COLUMN_L,
                                "squareCol", 1, 1, 1, 0);
                            if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                            {
                                Mesh mm = (Mesh)model1.Blocks["squareCol"].Entities[1].Clone();
                                mm.Translate(i * BEAM_L - COLUMN_B / 2, j * BEAM_L - COLUMN_H / 2, k * COLUMN_L);
                                offset = globalVerts.Count;
                                globalVerts.AddRange(mm.Vertices);
                                for (int n = 0; n < mm.Triangles.Length; n++)
                                {
                                    globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                        offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3, System.Drawing.Color.Gray));
                                }
                            }

                            if (labelCheckBox.IsChecked == true)
                            {
                                reference.Attributes.Add("Name", string.Format("Pillar_{0},{1},{2}", i, j, k));
                            }
                            _entityList.Add(reference);
                        }

                        if (showBeamXCheckBox.IsChecked == true)
                        {
                            if (j <= _bayYValue && i < _bayXValue)
                            {
                                // Parallel beams to X
                                Transformation t = new Transformation();
                                t.Rotation(-Math.PI / 2, Vector3D.AxisZ);
                                Transformation t2 = new Transformation();
                                t2.Translation(i * BEAM_L, j * BEAM_L + BEAM_B / 2, (k + 1) * COLUMN_L - BEAM_H / 2);
                                reference = new BlockReference(t2 * t, "beam");

                                if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                                {
                                    Mesh mm = (Mesh)model1.Blocks["beam"].Entities[0].Clone();
                                    mm.TransformBy(t2 * t);

                                    offset = globalVerts.Count;
                                    globalVerts.AddRange(mm.Vertices);
                                    for (int n = 0; n < mm.Triangles.Length; n++)
                                    {
                                        globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                            offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3, System.Drawing.Color.Gray));
                                    }
                                }

                                if (labelCheckBox.IsChecked == true)
                                {
                                    reference.Attributes.Add("Name", string.Format("Beam_{0},{1},{2}", i, j, k));
                                }
                                _entityList.Add(reference);
                            }
                        }

                        if (showBeamYCheckBox.IsChecked == true)
                        {
                            if (i <= _bayXValue && j < _bayYValue)
                            {
                                // Parallel beams to X
                                Transformation t = new Transformation();
                                t.Translation(i * BEAM_L - BEAM_B / 2, j * BEAM_L, (k + 1) * COLUMN_L - BEAM_H / 2);
                                reference = new BlockReference(t, "beam");

                                if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                                {
                                    Mesh mm = (Mesh)model1.Blocks["beam"].Entities[0].Clone();
                                    mm.TransformBy(t);
                                    offset = globalVerts.Count;
                                    globalVerts.AddRange(mm.Vertices);
                                    for (int n = 0; n < mm.Triangles.Length; n++)
                                    {
                                        globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                            offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3, System.Drawing.Color.Gray));
                                    }
                                }
                                if (labelCheckBox.IsChecked == true)
                                {
                                    reference.Attributes.Add("Name", string.Format("Beam_{0},{1},{2}", i, j, k));
                                }
                                _entityList.Add(reference);
                            }
                        }

                        if (shellCheckBox.IsChecked == true)
                        {
                            if ((j == 0 || j == _bayYValue) && i < _bayXValue)
                            {
                                for (int i1 = 0; i1 < _shellSubValue; i1++)
                                {
                                    for (int j1 = 0; j1 < _shellSubValue; j1++)
                                    {
                                        Transformation t = new Transformation();
                                        t.Translation(i * BEAM_L + i1 * shellB, j * BEAM_L - SHELL_TICKNESS / 2,
                                            k * COLUMN_L + j1 * shellH);
                                        reference = new BlockReference(t, "shell");

                                        if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                                        {
                                            Mesh mm = (Mesh)model1.Blocks["shell"].Entities[0].Clone();
                                            mm.TransformBy(t);
                                            offset = globalVerts.Count;
                                            globalVerts.AddRange(mm.Vertices);
                                            for (int n = 0; n < mm.Triangles.Length; n++)
                                            {
                                                globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                                    offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3,
                                                    System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)));
                                            }
                                        }
                                        _entityList.Add(reference);
                                    }
                                }
                            }
                        }
                        if (nodesCheckBox.IsChecked == true)
                        {
                            Joint joint1 = new Joint(i * BEAM_L, j * BEAM_L, k * COLUMN_L, 40, 2);
                            joint1.Color = System.Drawing.Color.Blue;
                            joint1.ColorMethod = colorMethodType.byEntity;
                            Joint joint2 = new Joint(i * BEAM_L, j * BEAM_L, (k + 1) * COLUMN_L, 40, 2);
                            joint2.Color = System.Drawing.Color.Blue;
                            joint2.ColorMethod = colorMethodType.byEntity;
                            _entityList.Add(joint1);
                            _entityList.Add(joint2);
                        }

                    }

                    if (shellCheckBox.IsChecked == true)
                    {
                        if (j == 0)
                        {
                            for (int l = 0; l < _bayYValue; l++)
                            {
                                for (int i1 = 0; i1 < _shellSubValue; i1++)
                                {
                                    for (int j1 = 0; j1 < _shellSubValue; j1++)
                                    {
                                        Transformation t = new Transformation();
                                        t.Translation(l * BEAM_L + i1 * shellB, j * BEAM_L - SHELL_TICKNESS / 2,
                                            k * COLUMN_L + j1 * shellH);
                                        Transformation t2 = new Transformation();
                                        t2.Rotation(Math.PI / 2, Vector3D.AxisZ);
                                        reference = new BlockReference(t2 * t, "shell");

                                        if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                                        {
                                            Mesh mm = (Mesh)model1.Blocks["shell"].Entities[0].Clone();
                                            mm.TransformBy(t2 * t);
                                            offset = globalVerts.Count;
                                            globalVerts.AddRange(mm.Vertices);
                                            for (int n = 0; n < mm.Triangles.Length; n++)
                                            {
                                                globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                                    offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3,
                                                    System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)));
                                            }
                                        }
                                        _entityList.Add(reference);
                                    }
                                }
                            }
                        }
                        if (j == _bayYValue)
                        {
                            for (int l = 0; l < _bayYValue; l++)
                            {
                                for (int i1 = 0; i1 < _shellSubValue; i1++)
                                {
                                    for (int j1 = 0; j1 < _shellSubValue; j1++)
                                    {
                                        Transformation t = new Transformation();
                                        t.Translation(l * BEAM_L + i1 * shellB, -SHELL_TICKNESS / 2, k * COLUMN_L + j1 * shellH);
                                        Transformation t2 = new Transformation();
                                        t2.Rotation(Math.PI / 2, Vector3D.AxisZ);
                                        Transformation t3 = new Transformation();
                                        t3.Translation(_bayXValue * BEAM_L, 0, 0);
                                        reference = new BlockReference(t3 * t2 * t, "shell");

                                        if ((structureType)structureModeEnumButton.Value == structureType.SingleMesh)
                                        {
                                            Mesh mm = (Mesh)model1.Blocks["shell"].Entities[0].Clone();
                                            mm.TransformBy(t3 * t2 * t);
                                            offset = globalVerts.Count;
                                            globalVerts.AddRange(mm.Vertices);
                                            for (int n = 0; n < mm.Triangles.Length; n++)
                                            {
                                                globalTris.Add(new ColorTriangle(offset + mm.Triangles[n].V1,
                                                    offset + mm.Triangles[n].V2, offset + mm.Triangles[n].V3,
                                                    System.Drawing.Color.FromArgb(123, System.Drawing.Color.LightGreen)));
                                            }
                                        }
                                        _entityList.Add(reference);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _buildingMesh = new Mesh(globalVerts, globalTris);
            _buildingMesh.ColorMethod = colorMethodType.byEntity;
            model1.Entities.AddRange(_entityList);

            switch ((structureType)structureModeEnumButton.Value)
            {
                case structureType.Flattened:
                    {
                        Entity[] entList = model1.Entities.Explode();
                        model1.Entities.Clear();
                        model1.Entities.AddRange(entList);
                        model1.Invalidate();
                        break;
                    }

                case structureType.SingleMesh:
                    {
                        model1.Entities.Clear();
                        model1.Entities.Add(_buildingMesh);
                        model1.Invalidate();
                        break;
                    }

                case structureType.Assembly:
                    {
                        model1.Invalidate();
                        break;
                    }
            }
            TreeViewUtility.PopulateTree(treeView1, model1.Entities.ToList(), model1.Blocks);
        }
    }
}