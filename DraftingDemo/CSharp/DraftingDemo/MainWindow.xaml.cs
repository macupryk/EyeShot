using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Forms;
using devDept.CustomControls;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using devDept.Serialization;
using Microsoft.Win32;
using AddonWindowLocation = devDept.CustomControls.AddonWindowLocation;
using Cursors = System.Windows.Input.Cursors;
using Environment = devDept.Eyeshot.Environment;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public ObservableCollection<ListViewModelItem> Layers { get; set; }
        private TangentsWindow tangentsWindow;
        public MainWindow()
        {
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

            //model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.  

            // Event handlers            
            model1.SelectionChanged += model1_SelectionChanged;
            model1.WorkCompleted += model1_WorkCompleted;
            model1.WorkCancelled += model1_WorkCancelled;
            model1.WorkFailed += model1_WorkFailed;
            model1.CameraMoveBegin += model1_CameraMoveBegin;
            endRadioButton.Checked += radioButtons_CheckedChanged;
            midRadioButton.Checked += radioButtons_CheckedChanged;
            cenRadioButton.Checked += radioButtons_CheckedChanged;
            pointRadioButton.Checked += radioButtons_CheckedChanged;
            quadRadioButton.Checked += radioButtons_CheckedChanged;
            tableTabControl.LayerListView.SelectedIndexChanged += SelectionChanged;

#if !NURBS
            extendButton.IsEnabled = false;
            trimButton.IsEnabled = false;
            filletButton.IsEnabled = false;
            chamferButton.IsEnabled = false;
            splineButton.IsEnabled = false;
#endif
            tableTabControl.FocusProperties(null);
        }

       

#if SETUP
        private readonly BitnessAgnostic _helper = new BitnessAgnostic();
#endif

        protected override void OnContentRendered(EventArgs e)
        {
            model1.Layers[0].LineWeight = 2;
            model1.Layers[0].Color = MyModel.DrawingColor;
            model1.Layers.TryAdd(new Layer("Dimensions", System.Drawing.Color.ForestGreen));
            model1.Layers.TryAdd(new Layer("Reference geometry", System.Drawing.Color.Red));

            tableTabControl.Environment = model1;
            model1.ActiveLayerName = model1.Layers[0].Name;

            // enables FastZPR when the scene exceeds 3000 objects
            _maxComplexity = model1.Turbo.MaxComplexity = 3000;

            selectionComboBox.SelectedIndex = 0;

            rendererVersionStatusLabel.Text = model1.RendererVersion.ToString();

            model1.SetView(viewType.Top);

            model1.Invalidate();
            
            model1.Focus();
            EnableControls(false);

#if SETUP
            string fileName = String.Format(@"{0}\Eyeshot {1} {2} Samples\dataset\Assets\Misc\app8.dwg", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), _helper.Edition, _helper.Version.Major);            
            ReadFileAsync ra = _helper.GetReadAutodesk(model1, fileName);
#else
            ReadAutodesk ra = new ReadAutodesk("../../../../../../dataset/Assets/Misc/app8.dwg");
            ra.HatchImportMode = ReadAutodesk.hatchImportType.BlockReference;
#endif
            model1.StartWork(ra);

            base.OnContentRendered(e);
        }

        #region Hide/Show

        private void showOriginButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.GetOriginSymbol().Visible = showOriginButton.IsChecked.Value;
            model1.Invalidate();            
        }

        private void showExtentsButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.BoundingBox.Visible = showExtentsButton.IsChecked.Value;
            model1.Invalidate();
        }

        private void showVerticesButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ShowVertices = showVerticesButton.IsChecked.Value;
            model1.Invalidate();
        }

        private void showGridButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.GetGrid().Visible = showGridButton.IsChecked.Value;
            model1.Invalidate();
        }
        #endregion

        #region Selection
        private void selectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            groupButton.IsEnabled = true;

            if (selectCheckBox.IsChecked.Value)
                Selection();
        }

        private void selectCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            groupButton.IsEnabled = true;

            if (selectCheckBox.IsChecked != null && selectCheckBox.IsChecked.Value)
            {
                ClearPreviousSelection();
                Selection();
            }
            else
                model1.ActionMode = actionType.None;
        }

        private void Selection()
        {
            switch (selectionComboBox.SelectedIndex)
            {
                case 0: // by pick
                    model1.ActionMode = actionType.SelectByPick;
                    break;

                case 1: // by box
                    model1.ActionMode = actionType.SelectByBox;
                    break;

                case 2: // by poly
                    model1.ActionMode = actionType.SelectByPolygon;
                    break;

                case 3: // by box enclosed
                    model1.ActionMode = actionType.SelectByBoxEnclosed;
                    break;

                case 4: // by poly enclosed
                    model1.ActionMode = actionType.SelectByPolygonEnclosed;
                    break;

                case 5: // visible by pick dynamic
                    model1.ActionMode = actionType.SelectVisibleByPickDynamic;
                    break;

                default:
                    model1.ActionMode = actionType.None;
                    break;
            }
        }

        private void clearSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (model1.ActionMode == actionType.SelectVisibleByPickLabel)

                model1.Viewports[0].Labels.ClearSelection();

            else

                model1.Entities.ClearSelection();

            model1.Invalidate();
        }

        private void invertSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (model1.ActionMode == actionType.SelectVisibleByPickLabel)

                model1.Viewports[0].Labels.InvertSelection();

            else

                model1.Entities.InvertSelection();

            model1.Invalidate();
        }

        private void groupButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.GroupSelection();
        }
        #endregion

        #region Editing

        private void deleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Entities.DeleteSelected();
            model1.Invalidate();
        }

        private void explodeButton_OnClick(object sender, RoutedEventArgs e)
        {
            for (int i = model1.Entities.Count - 1; i >= 0; i--)
            {

                Entity ent = model1.Entities[i];

                if (ent.Selected)
                {
                    if (ent is BlockReference)
                    {

                        model1.Entities.RemoveAt(i);

                        BlockReference br = (BlockReference)ent;

                        Entity[] entList = model1.Entities.Explode(br);

                        model1.Entities.AddRange(entList);

                    }

                    else if (ent is CompositeCurve)
                    {

                        model1.Entities.RemoveAt(i);

                        CompositeCurve cc = (CompositeCurve)ent;

                        model1.Entities.AddRange(cc.Explode());

                    }

                    else if (ent.GroupIndex > -1)
                    {
                        model1.Ungroup(ent.GroupIndex);
                    }
                }
            }

            model1.Invalidate();
        }

        private void trimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingTrim = true;            
            model1.waitingForSelection = true;                                    
        }

        private void extendButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingExtend = true;
            model1.waitingForSelection = true;                                    
        }

        private void offsetButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingOffset = true;
            model1.waitingForSelection = true;
        }

        private void mirrorButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingMirror = true;
            model1.waitingForSelection = true;
        }

        private void filletButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingFillet = true;
            model1.waitingForSelection = true;
        }

        private void chamferButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.doingChamfer = true;
            model1.waitingForSelection = true;
        }
        private void moveButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.selEntities.Clear();

            for (int i = model1.Entities.Count - 1; i > -1; i--)
            {
                Entity ent = model1.Entities[i];
                if (ent.Selected && (ent is ICurve || ent is BlockReference || ent is Text || ent is Leader))
                {
                    model1.selEntities.Add(ent);
                }
            }

            if (model1.selEntities.Count == 0)
                return;

            ClearPreviousSelection();
            model1.doingMove = true;
            foreach (Entity curve in model1.selEntities)
                curve.Selected = true;
        }
        private void rotateButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.selEntities.Clear();

            for (int i = model1.Entities.Count - 1; i > -1; i--)
            {
                Entity ent = model1.Entities[i];
                if (ent.Selected && (ent is ICurve || ent is BlockReference || ent is Text || ent is Leader))
                {
                    model1.selEntities.Add(ent);
                }
            }

            if (model1.selEntities.Count == 0)
                return;

            ClearPreviousSelection();
            model1.doingRotate = true;
            foreach (Entity curve in model1.selEntities)
                curve.Selected = true;
            
        }
        private void scaleButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.selEntities.Clear();

            for (int i = model1.Entities.Count - 1; i > -1; i--)
            {
                Entity ent = model1.Entities[i];
                if (ent.Selected && (ent is ICurve || ent is BlockReference || ent is Text || ent is Leader))
                {
                    model1.selEntities.Add(ent);
                }
            }

            if (model1.selEntities.Count == 0)
                return;

            ClearPreviousSelection();
            model1.doingScale = true;
            foreach (Entity curve in model1.selEntities)
                curve.Selected = true;
            
        }

        #endregion

        #region Inspection

        bool inspectVertex;

        private void pickVertexButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ActionMode = actionType.None;

            inspectVertex = false;

            if (pickVertexButton.IsChecked.Value)
            {
                inspectVertex = true;

                mainStatusLabel.Content = "Click on the entity to retrieve the 3D coordinates";

            }
            else
            {
                mainStatusLabel.Content = "";
            }
        }        
               
        private void Model1_OnMouseDown(object sender, MouseButtonEventArgs e)
        {

            // Checks that we are not using left mouse button for ZPR
            if (model1.ActionMode == actionType.None && e.ChangedButton != System.Windows.Input.MouseButton.Middle)
            {

                Point3D closest;

                if (inspectVertex)
                {

                    if (model1.FindClosestVertex(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), 50, out closest) != -1)

                        model1.Labels.Add(new devDept.Eyeshot.Labels.LeaderAndText(closest, closest.ToString(), new System.Drawing.Font("Tahoma", 8.25f), MyModel.DrawingColor, new Vector2D(0, 50)));

                }

                model1.Invalidate();

            }
        }     

        private void dumpButton_OnClick(object sender, RoutedEventArgs e)
        {            
            for (int i = 0; i < model1.Entities.Count; i++)
            {
                if (model1.Entities[i].Selected)
                {
                    string details = "Entity ID = " + i + System.Environment.NewLine + "----------------------" + System.Environment.NewLine + model1.Entities[i].Dump();
                                        
                    DetailsWindow rf = new DetailsWindow();
                    
                    rf.Title = "Dump";

                    rf.contentTextBox.Text = details;

                    rf.Show();
                    
                    break;
                }
            }
        }

        private void areaButton_OnClick(object sender, RoutedEventArgs e)
        {
            AreaProperties ap = new AreaProperties();

            int count = 0;

            for (int i = 0; i < model1.Entities.Count; i++)
            {

                Entity ent = model1.Entities[i];

                if (ent.Selected)
                {

                    ICurve itfCurve = (ICurve)ent;

                    if (itfCurve.IsClosed)

                        ap.Add(ent.Vertices);

                    count++;
                }

            }

            StringBuilder text = new StringBuilder();
            text.AppendLine(count + " entity(ies) selected");
            text.AppendLine("---------------------");

            if (ap.Centroid != null)
            {

                double x, y, z;
                double xx, yy, zz, xy, zx, yz;
                MomentOfInertia world, centroid;

                ap.GetResults(ap.Area, ap.Centroid, out x, out y, out z, out xx, out yy, out zz, out xy, out zx, out yz, out world, out centroid);

                text.AppendLine("Cumulative area: " + ap.Area + " square " + model1.Units.ToString().ToLower());
                text.AppendLine("Cumulative centroid: " + ap.Centroid);
                text.AppendLine("Cumulative area moments:");
                text.AppendLine(" First moments");
                text.AppendLine("  x: " + x.ToString("g6"));
                text.AppendLine("  y: " + y.ToString("g6"));
                text.AppendLine("  z: " + z.ToString("g6"));
                text.AppendLine(" Second moments");
                text.AppendLine("  xx: " + xx.ToString("g6"));
                text.AppendLine("  yy: " + yy.ToString("g6"));
                text.AppendLine("  zz: " + zz.ToString("g6"));
                text.AppendLine(" Product moments");
                text.AppendLine("  xy: " + xx.ToString("g6"));
                text.AppendLine("  yz: " + yy.ToString("g6"));
                text.AppendLine("  zx: " + zz.ToString("g6"));
                text.AppendLine(" Area Moments of Inertia about World Coordinate Axes");
                text.AppendLine("  Ix: " + world.Ix.ToString("g6"));
                text.AppendLine("  Iy: " + world.Iy.ToString("g6"));
                text.AppendLine("  Iz: " + world.Iz.ToString("g6"));
                text.AppendLine(" Area Radii of Gyration about World Coordinate Axes");
                text.AppendLine("  Rx: " + world.Rx.ToString("g6"));
                text.AppendLine("  Ry: " + world.Ry.ToString("g6"));
                text.AppendLine("  Rz: " + world.Rz.ToString("g6"));
                text.AppendLine(" Area Moments of Inertia about Centroid Coordinate Axes:");
                text.AppendLine("  Ix: " + centroid.Ix.ToString("g6"));
                text.AppendLine("  Iy: " + centroid.Iy.ToString("g6"));
                text.AppendLine("  Iz: " + centroid.Iz.ToString("g6"));
                text.AppendLine(" Area Radii of Gyration about Centroid Coordinate Axes");
                text.AppendLine("  Rx: " + centroid.Rx.ToString("g6"));
                text.AppendLine("  Ry: " + centroid.Ry.ToString("g6"));
                text.AppendLine("  Rz: " + centroid.Rz.ToString("g6"));

            }
                        
            DetailsWindow rf = new DetailsWindow();

            rf.Title = "Area Properties";

            rf.contentTextBox.Text = text.ToString();

            rf.Show();            
        }

        #endregion

        #region File
        private bool _yAxisUp = false;
        private OpenFileAddOn _openFileAddOn;

        private void openButton_OnClick(object sender, EventArgs e)
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
                    _yAxisUp = false;
                    model1.Clear();

                    EnableControls(false);

#if SETUP                    
                    ReadFile readFile = new ReadFile(openFileDialog.FileName, _helper.GetFileSerializerEx((contentType)_openFileAddOn.ContentOption));
#else
                    ReadFile readFile = new ReadFile(openFileDialog.FileName, false, (contentType)_openFileAddOn.ContentOption);
#endif
                    model1.StartWork(readFile);

                }

                _openFileAddOn.EventFileNameChanged -= OpenFileAddOn_EventFileNameChanged;
                _openFileAddOn.Dispose();
                _openFileAddOn = null;
            }
        }

        private void OpenFileAddOn_EventFileNameChanged(IWin32Window sender, string filePath)
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

        private void saveButton_OnClick(object sender, EventArgs e)
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            using (var saveFileAddOn = new SaveFileAddOn())
            {
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye";
                saveFileDialog.AddExtension = true;
                saveFileDialog.CheckPathExists = true;

                if (saveFileDialog.ShowDialog(saveFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    EnableControls(false);

                    WriteFile writeFile = new WriteFile(
                        new WriteFileParams(model1)
                        {
                            Content = (contentType)saveFileAddOn.ContentOption,
                            SerializationMode = (serializationType)saveFileAddOn.SerialOption,
                            SelectedOnly = saveFileAddOn.SelectedOnly,
                            Purge = saveFileAddOn.Purge
                        }, saveFileDialog.FileName,
#if SETUP
                        _helper.GetFileSerializerEx()
#else
                        new FileSerializerEx()
#endif
                        );
                    model1.StartWork(writeFile);
                }
            }
        }

        private void importButton_OnClick(object sender, EventArgs e)
        {
            using (var importFileDialog = new System.Windows.Forms.OpenFileDialog())
            using (var importFileAddOn = new ImportFileAddOn())
            {
                importFileDialog.Filter = "CAD drawings (*.dwg)|*.dwg|Drawing Exchange Format (*.dxf)|*.dxf|All compatible file types (*.*)|*.dwg;*.dxf";
                importFileDialog.Multiselect = false;
                importFileDialog.AddExtension = true;
                importFileDialog.CheckFileExists = true;
                importFileDialog.CheckPathExists = true;

                if (importFileDialog.ShowDialog(importFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    model1.Clear();
                    _yAxisUp = importFileAddOn.YAxisUp;

                    EnableControls(false);
#if SETUP
                    ReadFileAsync ra = _helper.GetReadAutodesk(model1, importFileDialog.FileName);
#else
                    ReadAutodesk ra = new ReadAutodesk(importFileDialog.FileName);
#endif
                    model1.StartWork(ra);
                }
            }
        }

        private void exportButton_OnClick(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CAD drawings (*.dwg)|*.dwg|Drawing Exchange Format (*.dxf)|*.dxf|3D PDF (*.pdf)|*.pdf";
            saveFileDialog.AddExtension = true;
            saveFileDialog.CheckPathExists = true;

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                EnableControls(false);
                WriteFileAsync wfa = null;
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                    case 2:
#if SETUP
                        wfa = _helper.GetWriteAutodesk(model1, saveFileDialog.FileName);
#else
                        wfa = new WriteAutodesk(model1, saveFileDialog.FileName);
#endif
                        break;
                    case 3:
#if SETUP
                        wfa = _helper.GetWritePDF(model1, saveFileDialog.FileName);
#else
                        wfa = new WritePDF(new WritePdfParams(model1, new Size(595, 842), new Rect(10, 10, 575, 822)), saveFileDialog.FileName);
#endif
                        break;
                }

                model1.StartWork(wfa);
            }
        }

        private void EnableControls(bool status)
        {
            rightPanel.IsEnabled = status;
        }        

        #endregion

        #region Event handlers

        private bool _skipZoomFit;

        private void model1_WorkCompleted(object sender, devDept.Eyeshot.WorkCompletedEventArgs e)
        {
            // checks the WorkUnit type, more than one can be present in the same application 
            if (e.WorkUnit is ReadFileAsync)
            {
                ReadFileAsync rfa = (ReadFileAsync)e.WorkUnit;

                ReadFile rf = e.WorkUnit as ReadFile;
                if (rf != null)
                    _skipZoomFit = rf.FileSerializer.FileBody.Camera != null;
                else
                    _skipZoomFit = false;

                if (rfa.Entities != null && _yAxisUp)
                    rfa.RotateEverythingAroundX();

                rfa.AddToScene(model1);
                
                model1.Layers[0].LineWeight = 2;
                model1.Layers[0].Color = MyModel.DrawingColor;
                model1.Layers.TryAdd(new Layer("Dimensions", System.Drawing.Color.ForestGreen));
                model1.Layers.TryAdd(new Layer("Reference geometry", System.Drawing.Color.Red));
                tableTabControl.Sync();
                

                if (System.IO.Path.GetFileName(rfa.FileName) == "app8.dwg")
                {
                    foreach (Entity ent in model1.Entities)
                    {
                        ent.Translate(-170, -400);
                    }

                    model1.Entities.Regen();
                    model1.Camera.Target = new Point3D(75, 3.5, 288);
                    model1.Camera.ZoomFactor = 3;
                    _skipZoomFit = true;
                }

                
                if (!_skipZoomFit)                
                    model1.SetView(viewType.Top, true, false);                
            }

            EnableControls(true);
        }                        
        
        private void model1_WorkFailed(object sender, WorkFailedEventArgs e)
        {
            EnableControls(true);
        }

        private void model1_WorkCancelled(object sender, EventArgs e)
        {
            EnableControls(true);
        }

        private void model1_CameraMoveBegin(object sender, Environment.CameraMoveEventArgs e)
        {
            // refresh FastZPR button according to FastZPR enable status.
            UpdateTurboButton();
        }

        private int _maxComplexity;
        private void turboButton_Checked(object sender, RoutedEventArgs e)
        {
            turboButton_OnClick();
        }

        private void turboButton_Unchecked(object sender, RoutedEventArgs e)
        {
            turboButton_OnClick();
        }
        private void turboButton_OnClick()
        {
            if (model1 == null) return;

            if (turboButton.IsChecked.Value == false)
            {
                _maxComplexity = model1.Turbo.MaxComplexity;
                model1.Turbo.MaxComplexity = int.MaxValue;
            }
            else
            {
                model1.Turbo.MaxComplexity = _maxComplexity;
            }

            model1.Entities.UpdateBoundingBox();
            UpdateTurboButton();
            model1.Invalidate();
        }

        private void UpdateTurboButton()
        {
            if (model1.Turbo.Enabled)
                turboButton.Style = FindResource("TurboToggleButtonStyle") as Style;
            else
                turboButton.Style = FindResource("ToggleButtonStyle") as Style;
        }

        private void websiteButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.devdept.com");
        }

        private void model1_SelectionChanged(object sender, Model.SelectionChangedEventArgs e)
        {

            int count = 0;

            // counts selected entities
            foreach (Entity ent in model1.Entities)

                if (ent.Selected)

                    count++;
            
            // updates count on the status bar
            selectedCountStatusLabel.Text = count.ToString();
            addedCountStatusLabel.Text = e.AddedItems.Count.ToString();
            removedCountStatusLabel.Text = e.RemovedItems.Count.ToString();
            


        }

        private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {            
            if (endRadioButton.IsChecked != null && endRadioButton.IsChecked.Value)

                model1.activeObjectSnap = objectSnapType.End;

            else if (midRadioButton.IsChecked != null && midRadioButton.IsChecked.Value)

                model1.activeObjectSnap = objectSnapType.Mid;

            else if (cenRadioButton.IsChecked != null && cenRadioButton.IsChecked.Value)

                model1.activeObjectSnap = objectSnapType.Center;

            else if (quadRadioButton.IsChecked != null && quadRadioButton.IsChecked.Value)

                model1.activeObjectSnap = objectSnapType.Quad;

            else if (pointRadioButton.IsChecked != null && pointRadioButton.IsChecked.Value)

                model1.activeObjectSnap = objectSnapType.Point;             
        }


        private void filletTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (model1 == null)
                return;

            double val;
            if (Double.TryParse(filletTextBox.Text, out val))
            {
                model1.filletRadius = val;
            }
        }

        private void showCurveDirectionButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            model1.ShowCurveDirection = showCurveDirectionButton.IsChecked.Value;
            model1.Invalidate();
        }

        #endregion

        #region Imaging

        private void printButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Print();
        }

        private void printPreviewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.PrintPreview(new System.Drawing.Size(500, 400));
        }

        private void pageSetupButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.PageSetup();
        }

        private void vectorCopyToClipbardButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.CopyToClipboardVector(false);

            //release mouse capture, otherwise the first mouse click is skipped                        
            vectorCopyToClipbardButton.ReleaseMouseCapture();
        }

        private void vectorSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog mySaveFileDialog = new Microsoft.Win32.SaveFileDialog();
            
            mySaveFileDialog.Filter = "Enhanced Windows Metafile (*.emf)|*.emf";
            mySaveFileDialog.RestoreDirectory = true;

            // Show save file dialog box            
            if (mySaveFileDialog.ShowDialog() == true)
            {                
                // To save as dxf/dwg, see the class HiddenLinesViewOnFileAutodesk available in x86 and x64 dlls                
                model1.WriteToFileVector(false, mySaveFileDialog.FileName);

                //release mouse capture, otherwise the first mouse click is skipped                                
                vectorSaveButton.ReleaseMouseCapture();
            }
        }

        #endregion

        #region Drafting
        private void pointButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingPoints = true;
        }

        private void textButton_OnClick(object sender, RoutedEventArgs e)
        {            
            ClearPreviousSelection();
            model1.drawingText = true;
        }

        private void leaderButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();
            model1.drawingLeader = true;
        }

        private void lineButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingLine = true;
        }

        private void plineButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingPolyLine = true;
        }

        private void arcButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingArc = true;
        }

        private void circleButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingCircle = true;
        }

        private void splineButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingCurve = true;
        }

        private void ellipseButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingEllipse = true;
        }

        private void ellipticalArcButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingEllipticalArc = true;
        }

        private void compositeCurveButton_OnClick(object sender, RoutedEventArgs e)
        {            
            model1.CreateCompositeCurve();
        }

        private void tangentsButton_OnClick(object sender, RoutedEventArgs e)
        {
           
            tangentsWindow= new TangentsWindow();
            tangentsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            tangentsWindow.ShowDialog();
            if (tangentsWindow.DialogResult == true)
            {


                model1.lineTangents = tangentsWindow.LineTangents;
                model1.circleTangents = tangentsWindow.CircleTangents;
                model1.tangentsRadius = tangentsWindow.TangentRadius;
                
                model1.trimTangent = tangentsWindow.TrimTangents;
                model1.flipTangent = tangentsWindow.FlipTangents;
                ClearPreviousSelection();
                model1.doingTangents = true;
                model1.waitingForSelection = true;
            }

        }

        #endregion

        #region Snapping
        private void objectSnapCheckBox_OnClick(object sender, RoutedEventArgs e)
        {            
            if (objectSnapCheckBox.IsChecked.Value)
            {
                model1.objectSnapEnabled = true;
                snapPanel.IsEnabled = true;
            }
            else
            {
                model1.objectSnapEnabled = false;
                snapPanel.IsEnabled = false;
            }
        }

        private void gridSnapCheckBox_OnClick(object sender, RoutedEventArgs e)
        {            
            if (gridSnapCheckBox.IsChecked.Value)
                model1.gridSnapEnabled = true;
            else
                model1.gridSnapEnabled = false;            
        }
        #endregion

        #region Dimensioning
        private void linearDimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingLinearDim = true;
        }

        private void ordinateVerticalButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();
            model1.drawingOrdinateDim = true;
            model1.drawingOrdinateDimVertical = true;
        }

        private void ordinateHorizontalButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();
            model1.drawingOrdinateDim = true;
            model1.drawingOrdinateDimVertical = false;
        }  

        private void radialDimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingRadialDim = true;
            model1.waitingForSelection = true;
        }


        private void diametricDimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingDiametricDim = true;
            model1.waitingForSelection = true;
        }

        private void alignedDimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingAlignedDim = true;
        }

        private void angularDimButton_OnClick(object sender, RoutedEventArgs e)
        {
            ClearPreviousSelection();            
            model1.drawingAngularDim = true;
            model1.waitingForSelection = true;
        }

        private void ClearPreviousSelection()
        {
            model1.SetView(viewType.Top, false, true);            
            model1.ClearAllPreviousCommandData();
        }
        #endregion

        #region Layers

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (tableTabControl.LayerListView.SelectedItems.Count > 0)
            {
                var item = tableTabControl.LayerListView.SelectedItem;
                model1.ActiveLayerName = item.Text;
            }
            else // nothing selected? we force layer zero
            {
                model1.ActiveLayerName = model1.Layers[0].Name;
            }
        }
        #endregion

        /// <summary>
        /// Represents a vetex type from model like center, mid point, etc.
        /// </summary>
        public enum objectSnapType
        {
            None,
            Point,
            End,
            Mid,
            Center,
            Quad
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (Window win in System.Windows.Application.Current.Windows)
                win.Close();
        }
    }

    /// <summary>    
    /// This class represent the Model for Layers List.
    /// </summary>    
    public class ListViewModelItem
    {
        public ListViewModelItem(Layer layer)
        {
            Layer = layer;
            IsChecked = layer.Visible;
            ForeColor = RenderContextUtility.ConvertColor(Layer.Color);
        }

        public Layer Layer { get; set; }

        public string LayerName { get { return Layer.Name; } }

        public float LayerLineWeight { get { return Layer.LineWeight; } }

        public Brush ForeColor { get; set; }

        public bool IsChecked { get; set; }
    }
}