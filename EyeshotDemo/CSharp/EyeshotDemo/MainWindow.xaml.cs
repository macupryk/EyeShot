using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using devDept.Eyeshot;
using devDept.Eyeshot.Labels;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using Microsoft.Win32;
using Cursors = System.Windows.Input.Cursors;
using MouseButton = System.Windows.Input.MouseButton;
using devDept.Eyeshot.Triangulation;
using devDept.Eyeshot.Translators;
using devDept.Serialization;
using devDept.CustomControls;
using System.Windows.Forms;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private bool _doingTransform = false;
        private bool _yAxisUp = false;

        public ObservableCollection<ListViewModelItem> Layers { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // model1.Unlock(""); // For more details see 'Product Activation' topic in the documentation.  

            // Event handlers                        
            model1.SelectionChanged += model1_SelectionChanged;
            model1.Viewports[0].LabelSelectionChanged += viewportZero_LabelSelectionChanged;
            model1.WorkCompleted += model1_WorkCompleted;
            model1.WorkCancelled += model1_WorkCancelled;
            model1.WorkFailed += model1_WorkFailed;
            model1.CameraMoveBegin += model1_CameraMoveBegin;

#if !PROFESSIONAL
            tabControl1.Items.Remove(primitivesTabPage);
            tabControl1.Items.Remove(triangulationTabPage);
            tabControl1.Items.Remove(meshingTabPage);
            tabControl1.Items.Remove(motherBoardTabPage);
            tabControl1.Items.Remove(bunnyTabPage);
            tabControl1.Items.Remove(pocketTabPage);
            tabControl1.Items.Remove(locomotiveTabPage);
            tabControl1.Items.Remove(bracketTabPage);
            tabControl1.Items.Remove(flangeTabPage);

            magGlassCheckBox.IsEnabled = false;
#endif

#if !NURBS
            tabControl1.Items.Remove(toolpathTabPage);
            tabControl1.Items.Remove(hairDryerTabPage);
            tabControl1.Items.Remove(bracketTabPage);
            tabControl1.Items.Remove(flangeTabPage);

#endif

#if !ULTIMATE
            
#endif

#if !SOLID
            tabControl1.Items.Remove(medalTabPage);
            tabControl1.Items.Remove(houseTabPage);
#endif

            tableTabControl.FocusProperties(null);            
        }        

        protected override void OnContentRendered(EventArgs e)
        {
            model1.MagnifyingGlass.Factor = 3;
            model1.MagnifyingGlass.Size = new System.Drawing.Size(200, 200);
            
            tabControl1.SelectedIndex = 1;
            rendererVersionStatusLabel.Text = model1.RendererVersion.ToString();

            // enables FastZPR when the scene exceeds 3000 objects
            _maxComplexity = model1.Entities.MaxComplexity = 3000;

            model1.Invalidate();
            tableTabControl.Environment = model1;

            base.OnContentRendered(e);         
        }

        private void tabControl1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!model1.IsLoaded) return;

            // every time the selected tab changes ...
            model1.ActionMode = actionType.None; // reset all actions
            model1.Focus();

            perspectiveButton.IsChecked = true;                 // set default projection to perspective
            selectionComboBox.SelectedIndex = 5;                // set default selection to VisibleByPick
            selectionFilterComboBox.SelectedIndex = 3;          // set default selection filter to Entity
            selectButton.IsChecked = false;                   // disable selection mode

            model1.StopAnimation();              // stop any animation

            model1.Clear();                      // clear model (entities, blocks, layers, materials, etc.)

            tableTabControl.FocusProperties(null);               // clear propertyGrid contents

            if (model1.GetLegends().Count > 0)
                model1.GetLegends()[0].Visible = false;
            
            model1.GetGrid().Visible = true;
            model1.GetGrid().Step = 10;


            model1.HiddenLines.Lighting = false;
            model1.HiddenLines.ColorMethod = hiddenLinesColorMethodType.SingleColor;
            model1.HiddenLines.DashedHiddenLines = false;

            model1.AutoHideLabels = true;
            model1.DisplayMode = displayType.Rendered;

            switch (((TabItem)tabControl1.SelectedItem).Header.ToString())
            {
#if SOLID

                case "Medal":
                    Draw.Medal(model1);
                    break;                
                case "House":
                    model1.HiddenLines.DashedHiddenLines = true;
                    model1.HiddenLines.DashedHiddenLinesColor = System.Drawing.Color.FromArgb(200, 200, 200);
                    Draw.House(model1);
                    break;
#endif
                case "Motherboard":
                    Draw.MotherBoard(model1);
                    break;
                case "Locomotive":
                    Draw.Locomotive(model1);
                    break;
                case "Bunny":
                    Draw.Bunny(model1);
                    break;
                case "Pocket 2.5x":
                    Draw.Pocket(model1);
                    break;
                case "Primitives":
                    Draw.Primitives(model1);
                    break;
#if NURBS
                case "Hair dryer":
                    Draw.HairDryer(model1);
                    break;                                
                case "Toolpath":
                    Draw.Toolpath(model1);
                    break;                
                case "Bracket":
                    Draw.Bracket(model1);
                    break;
                case "Flange":
                    Draw.Flange(model1);
                    break;
#endif                    
                case "Jet":
                    Draw.Jet(model1);
                    break;                
            }

            if (model1.IsBusy)
            {
                tabControl1.IsEnabled = false;
                importButton.IsEnabled = false;
                openButton.IsEnabled = false;
                saveButton.IsEnabled = false;
            }

            if (model1.IsLoaded)
            {
                // Sets trimetric view and fits the model in the main viewport
                model1.SetView(viewType.Trimetric, true, model1.AnimateCamera);

                // Refresh the model
                model1.Invalidate();

                tableTabControl.Sync();
                UpdateDisplayModeButtons();
            }

        }

        #region DisplayMode
        private void UpdateDisplayModeButtons()
        {
            // syncs the shading buttons with the current display mode.
            switch (model1.DisplayMode)
            {
                case displayType.Wireframe:
                    wireframeButton.IsChecked = true;
                    SetDisplayModeButtonsChecked(wireframeButton);
                    break;
                case displayType.Shaded:
                    shadedButton.IsChecked = true;
                    SetDisplayModeButtonsChecked(shadedButton);
                    break;
                case displayType.Rendered:
                    renderedButton.IsChecked = true;
                    SetDisplayModeButtonsChecked(renderedButton);
                    break;
                case displayType.Flat:
                    flatButton.IsChecked = true;
                    SetDisplayModeButtonsChecked(flatButton);
                    break;
                case displayType.HiddenLines:
                    hiddenLinesButton.IsChecked = true;
                    SetDisplayModeButtonsChecked(hiddenLinesButton);
                    break;
            }
        }
        private void SetDisplayModeButtonsChecked(ToggleButton checkedButton)
        {            
            if (!checkedButton.Equals(wireframeButton)) wireframeButton.IsChecked = !checkedButton.IsChecked;
            if (!checkedButton.Equals(shadedButton)) shadedButton.IsChecked = !checkedButton.IsChecked;
            if (!checkedButton.Equals(renderedButton)) renderedButton.IsChecked = !checkedButton.IsChecked;
            if (!checkedButton.Equals(hiddenLinesButton)) hiddenLinesButton.IsChecked = !checkedButton.IsChecked;                       
            if (!checkedButton.Equals(flatButton)) flatButton.IsChecked = !checkedButton.IsChecked;
        }

        private void wireframeButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDisplayModeButtonsChecked((ToggleButton) sender);
            SetDisplayMode(model1, displayType.Wireframe);
        }

        private void shadedButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDisplayModeButtonsChecked((ToggleButton)sender);
            SetDisplayMode(model1, displayType.Shaded);            
        }

        private void renderedButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDisplayModeButtonsChecked((ToggleButton)sender);
            SetDisplayMode(model1, displayType.Rendered);            
        }

        private void hiddenLinesButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDisplayModeButtonsChecked((ToggleButton)sender);
            SetDisplayMode(model1, displayType.HiddenLines);            
        }

        private void flatButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetDisplayModeButtonsChecked((ToggleButton)sender);
            SetDisplayMode(model1, displayType.Flat);            
        }

        private void showCurveDirectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ShowCurveDirection = showCurveDirectionButton.IsChecked.Value;            
            model1.Invalidate();
        }

        public static void SetDisplayMode(Model model, displayType displayType)
        {
            model.DisplayMode = displayType;
            SetBackgroundStyleAndColor(model);
            model.Entities.UpdateBoundingBox(); // Updates simplified representation (when available)
            model.Invalidate();
        }

        public static void SetBackgroundStyleAndColor(Model model)
        {
            model.GetCoordinateSystemIcon().Lighting = false;
            model.GetViewCubeIcon().Lighting = false;

            switch (model.DisplayMode)
            {

                case displayType.HiddenLines:
                    model.GetBackground().TopColor = RenderContextUtility.ConvertColor(System.Drawing.Color.FromArgb(0xD2, 0xD0, 0xB9));

                    model.GetCoordinateSystemIcon().Lighting = true;
                    model.GetViewCubeIcon().Lighting = true;

                    break;

                default:
                    model.GetBackground().TopColor = RenderContextUtility.ConvertColor(System.Drawing.Color.FromArgb(0xED, 0xED, 0xED));
                    break;
            }

            model.CompileUserInterfaceElements();
        }

        private void cullingButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (cullingButton.IsChecked.Value)
                model1.Backface.ColorMethod = backfaceColorMethodType.Cull;
            else
                model1.Backface.ColorMethod = backfaceColorMethodType.EntityColor;

            model1.Invalidate();
        }
        #endregion

        #region ZPR
        private void zoomButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ActionMode = actionType.None;

            if (zoomButton.IsChecked.Value)

                model1.ActionMode = actionType.Zoom;
            
            panButton.IsChecked = false;
            rotateButton.IsChecked = false;
            zoomWindowButton.IsChecked = false;
            selectButton.IsChecked = false;
        }

        private void panButton_OnClick(object sender, EventArgs e)
        {
            model1.ActionMode = actionType.None;

            if (panButton.IsChecked.Value)

                model1.ActionMode = actionType.Pan;

            zoomButton.IsChecked = false;
            rotateButton.IsChecked = false;
            zoomWindowButton.IsChecked = false;
            selectButton.IsChecked = false;
        }

        private void rotateButton_OnClick(object sender, EventArgs e)
        {
            model1.ActionMode = actionType.None;

            if (rotateButton.IsChecked.Value)

                model1.ActionMode = actionType.Rotate;

            zoomButton.IsChecked = false;
            panButton.IsChecked = false;
            zoomWindowButton.IsChecked = false;
            selectButton.IsChecked = false;
        }

        private void zoomFitButton_OnClick(object sender, EventArgs e)
        {
            model1.ZoomFit();
            model1.Invalidate();
        }

        private void zoomWindowButton_OnClick(object sender, EventArgs e)
        {
            model1.ActionMode = actionType.None;

            if (zoomWindowButton.IsChecked.Value)

                model1.ActionMode = actionType.ZoomWindow;

            zoomButton.IsChecked = false;
            panButton.IsChecked = false;
            rotateButton.IsChecked = false;
            selectButton.IsChecked = false;
        }

        #endregion

        #region Projection
        private void parallelButton_OnClick(object sender, RoutedEventArgs e)
        {
            perspectiveButton.IsChecked = !parallelButton.IsChecked;

            model1.Camera.ProjectionMode = projectionType.Orthographic;

            model1.AdjustNearAndFarPlanes();

            model1.Invalidate();
        }

        private void perspectiveButton_OnClick(object sender, RoutedEventArgs e)
        {
            parallelButton.IsChecked = !perspectiveButton.IsChecked;

            model1.Camera.ProjectionMode = projectionType.Perspective;

            model1.AdjustNearAndFarPlanes();

            model1.Invalidate();
        }
        #endregion

        #region Zoom/Pan/Rotate

        private bool RotateToFace = false;

        private void rotateToFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ActionMode = actionType.None;

            RotateToFace = false;

            if (rotateToFaceButton.IsChecked.Value)
            {
                RotateToFace = true;
                model1.Cursor = Cursors.Hand;
            }
            else
            {
                RotateToFace = false;                
                model1.Cursor = null;
            }
        }

        #endregion

        #region Zoom
        private void zoomSelectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ZoomFit(true);
            model1.Invalidate();
        }
        #endregion

        #region View

        private void isoViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.SetView(viewType.Isometric, true, model1.AnimateCamera);
            model1.Invalidate();
        }

        private void frontViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.SetView(viewType.Front, true, model1.AnimateCamera);
            model1.Invalidate();
        }

        private void sideViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.SetView(viewType.Right, true, model1.AnimateCamera);
            model1.Invalidate();
        }

        private void topViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.SetView(viewType.Top, true, model1.AnimateCamera);
            model1.Invalidate();
        }

        private void prevViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.PreviousView();
            model1.Invalidate();
        }

        private void nextViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.NextView();
            model1.Invalidate();
        }

        private void animateCameraCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            model1.AnimateCamera = animateCameraCheckBox.IsChecked.Value;
        }
        #endregion

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
            if (selectButton == null)
                return;

            groupButton.IsEnabled = true;

            if (selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
                Selection();
        }

        private void selectCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            groupButton.IsEnabled = true;

            if (selectButton.IsChecked.HasValue && selectButton.IsChecked.Value)
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

                case 5: // visible by pick
                    model1.ActionMode = actionType.SelectVisibleByPick;
                    break;

                case 6: // visible by box
                    model1.ActionMode = actionType.SelectVisibleByBox;
                    break;

                case 7: // visible by poly
                    model1.ActionMode = actionType.SelectVisibleByPolygon;
                    break;

                case 8: // visible by pick dynamic
                    model1.ActionMode = actionType.SelectVisibleByPickDynamic;
                    break;

                case 9: // visible by pick label
                    model1.ActionMode = actionType.SelectVisibleByPickLabel;
                    groupButton.IsEnabled = false;
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

        private void selectionFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionFilter();
        }

        private void SelectionFilter()
        {
            switch (selectionFilterComboBox.SelectedIndex)
            {
                case 0:
                    model1.SelectionFilterMode = selectionFilterType.Vertex;
                    break;

                case 1:
                    model1.SelectionFilterMode = selectionFilterType.Edge;
                    break;

                case 2:
                    model1.SelectionFilterMode = selectionFilterType.Face;
                    break;

                case 3:
                    model1.SelectionFilterMode = selectionFilterType.Entity;
                    break;
            }
        }

        private void setCurrentButton_OnClick(object sender, RoutedEventArgs e)
        {
            for (int index = 0; index < model1.Entities.Count; index++)
            {
                Entity ent = model1.Entities[index];

                if (ent.Selected && ent is BlockReference)
                {
                    model1.Entities.SetCurrent((BlockReference) ent);
                    model1.Invalidate();
                }
            }
        }

        private void clearCurrentButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Entities.SetCurrent(null);
            model1.Invalidate();
        }
        #endregion

        #region Editing
        private void transformButton_OnClick(object sender, RoutedEventArgs e)
        {
            // allows transformation for one entity at a time
            Entity temp = null;
            foreach (Entity ent in model1.Entities)
            {
                if (ent.Selected)
                {
                    temp = ent;
                    break;
                }
            }
            if (temp != null)
            {
                _doingTransform = true;
                Transformation center = new Translation(temp.BoxMin.X, temp.BoxMin.Y, temp.BoxMin.Z);
                model1.ObjectManipulator.Enable(center, false);
                model1.ObjectManipulator.ShowOriginalWhileEditing = false;
                model1.Invalidate();
            }
            else
                return;
        }

        private void duplicateButton_OnClick(object sender, RoutedEventArgs e)
        {
            // counts selected entities
            int count = 0;

            foreach (Entity ent in model1.Entities)

                if (ent.Selected)

                    count++;

            // fills the duplicates array
            Entity[] duplicates = new Entity[count];

            count = 0;

            foreach (Entity ent in model1.Entities)

                if (ent.Selected)
                {

                    duplicates[count] = (Entity)ent.Clone();

                    ent.Selected = false;

                    count++;

                }


            foreach (Entity dup in duplicates)
            {

                dup.Translate(50, 100, 50);

                model1.Entities.Add(dup);

            }

            model1.Invalidate();
        }

        private void deleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.Entities.DeleteSelected();
            model1.Invalidate();
        }

        private void untrimButton_OnClick(object sender, RoutedEventArgs e)
        {
#if NURBS
            foreach (Entity ent in model1.Entities)

                if (ent.Selected)
                {

                    if (ent is Surface)
                    {

                        Surface s = ent as Surface;

                        s.Untrim();

                        model1.Entities.Regen();

                    }

                }

            model1.Invalidate();
#endif
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
#if NURBS
                    else if (ent is Brep)
                    {
                        model1.Entities.RemoveAt(i);

                        Brep sld = (Brep) ent;

                        model1.Entities.AddRange(sld.ConvertToSurfaces());
                    }
#endif
                    else if (ent.GroupIndex > -1)
                    {
                        model1.Ungroup(ent.GroupIndex);
                    }
                }
            }

            model1.Invalidate();
        }

#endregion

#region Inspection

        bool inspectVertex = false;
        bool inspectMesh = false;

        private void pickVertexButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ActionMode = actionType.None;

            inspectVertex = false;
            inspectMesh = false;

            if (pickVertexButton.IsChecked.HasValue && pickVertexButton.IsChecked.Value)
            {
                inspectVertex = true;

                mainStatusLabel.Content = "Click on the entity to retrieve the 3D coordinates";

            }
            else
            {
                mainStatusLabel.Content = "";
            }

            pickFaceButton.IsChecked = false;
        }

        private void pickFaceButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.ActionMode = actionType.None;

            inspectVertex = false;
            inspectMesh = false;

            if (pickFaceButton.IsChecked.HasValue && pickFaceButton.IsChecked.Value)
            {

                inspectMesh = true;

                mainStatusLabel.Content = "Click on the face to retrieve the 3D coordinates";

            }
            else

                mainStatusLabel.Content = "";

            pickVertexButton.IsChecked = false;
        }

        private void Model1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Checks that we are not using left mouse button for ZPR
            if (model1.ActionMode == actionType.None && e.ChangedButton != MouseButton.Middle)
            {
                Point3D closest;

                if (inspectVertex)
                {
                    if (model1.FindClosestVertex(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), 50, out closest) != -1)
                    {
                        Point3D copy = (Point3D)closest.Clone();

                        model1.Labels.Add(new devDept.Eyeshot.Labels.LeaderAndText(copy, closest.ToString(), new System.Drawing.Font("Tahoma", 8.25f), Draw.Color, new Vector2D(0, 50)));
                    }
                }
                else if (inspectMesh)
                {
                    var item = model1.GetItemUnderMouseCursor(RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)));

                    if (item != null)
                    {
                        if (item.Item is IFace)
                        {
                            Point3D pt;
                            int tri;
                            if (model1.FindClosestTriangle(item, RenderContextUtility.ConvertPoint(model1.GetMousePosition(e)), out pt, out tri) > 0)
                            {
                                // adds a label with the point elevation
                                model1.Labels.Add(new devDept.Eyeshot.Labels.LeaderAndText(pt, pt.ToString(),
                                    new System.Drawing.Font("Tahoma", 8.25f), Draw.Color, new Vector2D(0, 50)));
                            }
                        }
                    }
                }

                if (RotateToFace)
                {
                    System.Drawing.Point point = RenderContextUtility.ConvertPoint(model1.GetMousePosition(e));
                    int index = model1.GetEntityUnderMouseCursor(point);
                    if(index != -1)
                        RotateToFaceRecursive(model1.Entities[index], point);

                }

                model1.Invalidate();

            }

            if (RotateToFace)

                rotateToFaceButton.IsChecked = false;

            if (e.ChangedButton == System.Windows.Input.MouseButton.Right)
            {
                // doing trasformation apply the objectManipulator changes
                if (_doingTransform)
                {
                    model1.ObjectManipulator.Apply();
                    model1.ObjectManipulator.Cancel();
                    model1.Entities.Regen();
                    _doingTransform = false;
                }
            }
        }

        private void RotateToFaceRecursive(Entity entity, System.Drawing.Point point)
        {
            if (entity is BlockReference)
            {
                BlockReference blockReference = (BlockReference) entity;
                model1.Entities.SetCurrent(blockReference);
                int index = model1.GetEntityUnderMouseCursor(point);
                RotateToFaceRecursive(model1.Entities[index], point);
            }
            else
            {
                // rotates the view perpendicular to the plane under the mouse cursor
                model1.RotateCamera(point);
                model1.Entities.SetCurrent(null);
            }
        }

        private void dumpButton_OnClick(object sender, RoutedEventArgs e)
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

                        df.Title = "Dump";

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

                    df.Title = "Dump";

                    df.contentTextBox.Text = sb.ToString();

                    df.Show();

                    break;
                }
            }
        }

        private void statisticsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DetailsWindow rf = new DetailsWindow();

            rf.Title = "Statistics";

            rf.contentTextBox.Text = model1.Entities.GetStats(model1.Blocks, true);

            rf.Show();
        }

        private int AddAreaProperty(AreaProperties ap, Entity ent, out bool blockReferenceNotScaled, bool isParentSelected = false)
        {
            int count = 0;
            blockReferenceNotScaled = true;

            if (ent.Selected || isParentSelected)
            {
                if (ent is IFace)
                {
                    IFace itfFace = (IFace)ent;

                    Mesh[] meshes = itfFace.GetPolygonMeshes();

                    foreach (Mesh mesh in meshes)
                    {
                        ap.Add(mesh.Vertices, mesh.Triangles);
                    }
                    count++;
                }
                else if (ent is BlockReference)
                {
                    var br = (BlockReference)ent;

                    if (br.GetScaleFactorX() != 1 &&
                         br.GetScaleFactorY() != 1 &&
                         br.GetScaleFactorZ() != 1)
                    {
                        blockReferenceNotScaled = false;
                        return count;
                    }

                    foreach (var e in br.GetEntities(model1.Blocks))
                    { 
                        count += AddAreaProperty(ap, e, out blockReferenceNotScaled, true);

                        if (!blockReferenceNotScaled) return count;
                    }

            }
                else
                {
                    ICurve itfCurve = (ICurve)ent;

                    if (itfCurve.IsClosed)
                        ap.Add(ent.Vertices);

                    count++;
                }

            }
#if NURBS
            else if (ent is Brep)
            {
                Brep solid3D = (Brep)ent;

                for (int j = 0; j < solid3D.Faces.Length; j++)
                {
                    Brep.Face sf = solid3D.Faces[j];
                    Mesh[] faceTessellation = sf.Tessellation;

                    if (solid3D.GetFaceSelection(j))
                    {
                        foreach (Mesh m in faceTessellation)
                            ap.Add(m.Vertices, m.Triangles);

                        count++;
                    }
                }
            }
#endif
            return count;
        }

        private int AddVolumeProperty(VolumeProperties vp, Entity ent, out bool blockReferenceNotScaled, bool isParentSelected = false)
        {
            int count = 0;
            blockReferenceNotScaled = true;

            if (ent.Selected || isParentSelected)
            {
                if (ent is IFace)
                {
                    IFace itfFace = (IFace)ent;

                    Mesh[] meshes = itfFace.GetPolygonMeshes();

                    foreach (Mesh mesh in meshes)
                    {
                        vp.Add(mesh.Vertices, mesh.Triangles);
                    }
                    count++;
                }
                else if (ent is BlockReference)
                {
                    var br = (BlockReference)ent;

                    if (br.GetScaleFactorX() != 1 &&
                         br.GetScaleFactorY() != 1 &&
                         br.GetScaleFactorZ() != 1)
                    {
                        blockReferenceNotScaled = false;
                        return count;
                    }

                    foreach (var e in br.GetEntities(model1.Blocks))
                    {
                        count += AddVolumeProperty(vp, e, out blockReferenceNotScaled, true);

                        if (!blockReferenceNotScaled) return count;
                    }
                }

            }

            return count;
        }


        private void areaButton_OnClick(object sender, RoutedEventArgs e)
        {
            AreaProperties ap = new AreaProperties();

            int count = 0;
            var blockReferenceNotScaled = true;

            for (int i = 0; i < model1.Entities.Count && blockReferenceNotScaled; i++)
            {

                Entity ent = model1.Entities[i];

                count += AddAreaProperty(ap, ent, out blockReferenceNotScaled);
            }

            StringBuilder text = new StringBuilder();

            if (blockReferenceNotScaled)
            {
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
            }
            else
            {
                text.AppendLine("Error: scaled BlockReference is not supported.");
                text.AppendLine("---------------------");
            }

            DetailsWindow rf = new DetailsWindow();

            rf.Title = "Area Properties";

            rf.contentTextBox.Text = text.ToString();

            rf.Show();
        }

        private void volumeButton_OnClick(object sender, RoutedEventArgs e)
        {
            VolumeProperties vp = new VolumeProperties();

            int count = 0;
            var blockReferenceNotScaled = true;

            for (int i = 0; i < model1.Entities.Count && blockReferenceNotScaled; i++)
            {

                Entity ent = model1.Entities[i];

                count += AddVolumeProperty(vp, ent, out blockReferenceNotScaled);
            }

            StringBuilder text = new StringBuilder();

            if (blockReferenceNotScaled)
            {
                text.AppendLine(count + " entity(ies) selected");
                text.AppendLine("---------------------");

                if (vp.Centroid != null)
                {

                    double x, y, z;
                    double xx, yy, zz, xy, zx, yz;
                    MomentOfInertia world, centroid;

                    vp.GetResults(vp.Volume, vp.Centroid, out x, out y, out z, out xx, out yy, out zz, out xy, out zx, out yz, out world, out centroid);

                    text.AppendLine("Cumulative volume: " + vp.Volume + " cubic " + model1.Units.ToString().ToLower());
                    text.AppendLine("Cumulative centroid: " + vp.Centroid);
                    text.AppendLine("Cumulative volume moments:");
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
                    text.AppendLine(" Volume Moments of Inertia about World Coordinate Axes");
                    text.AppendLine("  Ix: " + world.Ix.ToString("g6"));
                    text.AppendLine("  Iy: " + world.Iy.ToString("g6"));
                    text.AppendLine("  Iz: " + world.Iz.ToString("g6"));
                    text.AppendLine(" Volume Radii of Gyration about World Coordinate Axes");
                    text.AppendLine("  Rx: " + world.Rx.ToString("g6"));
                    text.AppendLine("  Ry: " + world.Ry.ToString("g6"));
                    text.AppendLine("  Rz: " + world.Rz.ToString("g6"));
                    text.AppendLine(" Volume Moments of Inertia about Centroid Coordinate Axes:");
                    text.AppendLine("  Ix: " + centroid.Ix.ToString("g6"));
                    text.AppendLine("  Iy: " + centroid.Iy.ToString("g6"));
                    text.AppendLine("  Iz: " + centroid.Iz.ToString("g6"));
                    text.AppendLine(" Volume Radii of Gyration about Centroid Coordinate Axes");
                    text.AppendLine("  Rx: " + centroid.Rx.ToString("g6"));
                    text.AppendLine("  Ry: " + centroid.Ry.ToString("g6"));
                    text.AppendLine("  Rz: " + centroid.Rz.ToString("g6"));
                }
            }
            else
            {
                text.AppendLine("Error: scaled BlockReference is not supported.");
                text.AppendLine("---------------------");
            }

            DetailsWindow rf = new DetailsWindow();

            rf.Title = "Volume Properties";

            rf.contentTextBox.Text = text.ToString();

            rf.Show();
        }

#endregion

#region Imaging

        private void rasterCopyToClipboardButton_OnClick(object sender, RoutedEventArgs e)
        {
            model1.CopyToClipboardRaster();
        }

        private void rasterSaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog mySaveFileDialog = new Microsoft.Win32.SaveFileDialog();
            
            mySaveFileDialog.Filter = "Bitmap (*.bmp)|*.bmp|" +
                "Portable Network Graphics (*.png)|*.png|" +
                "Windows metafile (*.wmf)|*.wmf|" +
                "Enhanced Windows Metafile (*.emf)|*.emf";

            mySaveFileDialog.FilterIndex = 2;
            mySaveFileDialog.RestoreDirectory = true;
            Nullable<bool> result = mySaveFileDialog.ShowDialog();
            if (result == true)
            {

                switch (mySaveFileDialog.FilterIndex)
                {

                    case 1: model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case 2: model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case 3: model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Wmf);
                        break;
                    case 4: model1.WriteToFileRaster(2, mySaveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Emf);
                        break;

                }

            }
        }

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
            //temporarily disabled
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

#region File
        private void websiteButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("www.devdept.com");
        }
#endregion

#region Event handlers
        int totalSelectedCount = 0;

        private void model1_SelectionChanged(object sender, Model.SelectionChangedEventArgs e)
        {

            int selectedCount = 0;

            // counts selected entities
            object[] selected = new object[e.AddedItems.Count];

            selectedCount = 0;

            // fills selected array
            for (int index = 0; index < e.AddedItems.Count; index++)
            {
                var item = e.AddedItems[index];

                if (item is Model.SelectedFace)
                {
                    var faceItem = ((Model.SelectedFace)item);
                    var ent = faceItem.Item;

                    if (ent is Mesh)
                    {
                        var mesh = (Mesh)ent;
                        selected[selectedCount++] = mesh.Faces[faceItem.Index];
                    }

#if NURBS
                    else if (ent is Brep)
                    {
                        var sol = (Brep)ent;
                        if (faceItem.ShellIndex == 0)
                            selected[selectedCount++] = sol.Faces[faceItem.Index];
                        else
                        {
                            selected[selectedCount++] = sol.Inners[faceItem.ShellIndex - 1][faceItem.Index];
                        }
                    }
#endif

#if SOLID
                    else if (ent is Solid)
                    {
                        var sol = (Solid)ent;
                        selected[selectedCount++] = sol.Faces[faceItem.Index];
                    }
#endif
                }

#if NURBS
                else if (item is Model.SelectedEdge)
                {
                    var edgeItem = ((Model.SelectedEdge)item);
                    var ent = edgeItem.Item;
                    if (ent is Brep)
                    {
                        var sol = (Brep)ent;
                        selected[selectedCount++] = sol.Edges[edgeItem.Index];

                    }
                }
                else if (item is Model.SelectedVertex)
                {
                    var vertexItem = ((Model.SelectedVertex)item);
                    var ent = vertexItem.Item;
                    if (ent is Brep)
                    {
                        var sol = (Brep)ent;
                        selected[selectedCount++] = sol.Vertices[vertexItem.Index];
                    }
                }
#endif
                else
                    selected[selectedCount++] = e.AddedItems[index].Item;

            }


            // updates counters on the status bar
            totalSelectedCount += selectedCount - e.RemovedItems.Count;
            selectedCountStatusLabel.Text = totalSelectedCount.ToString();
            addedCountStatusLabel.Text = e.AddedItems.Count.ToString();
            removedCountStatusLabel.Text = e.RemovedItems.Count.ToString();
        }

        private void viewportZero_LabelSelectionChanged(object sender, Model.SelectionChangedEventArgs e)
        {

            int count = 0;

            // counts selected entities
            foreach (devDept.Eyeshot.Labels.Label lbl in model1.Viewports[0].Labels)

                if (lbl.Selected)

                    count++;
            // updates count on the status bar
            selectedCountStatusLabel.Text = count.ToString();
            addedCountStatusLabel.Text = e.AddedItems.Count.ToString();
            removedCountStatusLabel.Text = e.RemovedItems.Count.ToString();
        }

        private void model1_WorkCancelled(object sender, EventArgs e)
        {
            EnableControls();
        }

        private void EnableControls()
        {
            tabControl1.IsEnabled = true;
            importButton.IsEnabled = true;
            openButton.IsEnabled = true;
            saveButton.IsEnabled = true;
        }

        private bool _skipZoomFit = false;
        private void model1_WorkCompleted(object sender, devDept.Eyeshot.WorkCompletedEventArgs e)
        {
            // checks the WorkUnit type, more than one can be present in the same application 
            if (e.WorkUnit is BallPivoting)
            {
                BallPivoting bp = (BallPivoting)e.WorkUnit;

                Mesh m = bp.Result;

                m.EdgeStyle = Mesh.edgeStyleType.Free;
                model1.Entities.Clear();
                model1.Entities.Add(m, "Default", System.Drawing.Color.Beige);
            }
            else if (e.WorkUnit is ReadFileAsync)
            {
                ReadFileAsync rfa = (ReadFileAsync)e.WorkUnit;

                RegenOptions ro = new RegenOptions();
                ro.Async = _asyncRegen;

                ReadFile rf = e.WorkUnit as ReadFile;
                if (rf != null)
                    _skipZoomFit = rf.FileSerializer.FileBody.Camera != null;
                else
                    _skipZoomFit = false;

                if (rfa.Entities != null && _yAxisUp)
                    rfa.RotateEverythingAroundX();

                #region disable sexy features to get the max FPS on imported models
                model1.Shaded.ShadowMode   = shadowType.None;
                model1.Rendered.ShadowMode = shadowType.None;
                model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
                model1.Rendered.PlanarReflections = false;
                #endregion

                rfa.AddToScene(model1, ro);

                if (!_asyncRegen && !_skipZoomFit)
                    model1.ZoomFit();
            }
            else if (e.WorkUnit is Regeneration)
            {
                model1.Entities.UpdateBoundingBox();
                if (!_skipZoomFit)
                    model1.ZoomFit();
                model1.Invalidate();
                _skipZoomFit = false;
            }

            EnableControls();

            tableTabControl.Sync();
            UpdateDisplayModeButtons();
        }

        private void model1_WorkFailed(object sender, WorkFailedEventArgs e)
        {
           EnableControls();
        }

        private void model1_CameraMoveBegin(object sender, devDept.Eyeshot.Environment.CameraMoveEventArgs e)
        {
            // refresh FastZPR button according to FastZPR enable status.
            UpdateTurboButton();
        }

        #endregion

        private void layerListView_ItemChecked(object sender, RoutedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.CheckBox;
            var item = (ListViewModelItem)cb.DataContext;
            
            if (item.IsChecked)

                model1.Layers.TurnOn(item.LayerName);

            else

                model1.Layers.TurnOff(item.LayerName);

            // updates bounding box, shadow and transparency
            model1.Entities.UpdateBoundingBox();

            model1.Invalidate();
        }

        private OpenFileAddOn _openFileAddOn;
        private void openButton_OnClick(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog1.Filter = "Eyeshot (*.eye)|*.eye";
                openFileDialog1.Multiselect = false;
                openFileDialog1.AddExtension = true;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;
                openFileDialog1.DereferenceLinks = true;

                _openFileAddOn = new OpenFileAddOn();
                _openFileAddOn.EventFileNameChanged += OpenFileAddOn_EventFileNameChanged;

                if (openFileDialog1.ShowDialog(_openFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    _yAxisUp = false;
                    model1.Clear();
                    ReadFile readFile = new ReadFile(openFileDialog1.FileName, (contentType)_openFileAddOn.ContentOption);
                    model1.StartWork(readFile);
                    model1.SetView(viewType.Trimetric, true, model1.AnimateCamera);
                    openButton.IsEnabled = false;
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

        private void eyeSaveButton_OnClick(object sender, EventArgs e)
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            using (var saveFileAddOn = new SaveFileAddOn())
            {
                saveFileDialog.Filter = "Eyeshot (*.eye)|*.eye";
                saveFileDialog.AddExtension = true;
                saveFileDialog.CheckPathExists = true;

                if (saveFileDialog.ShowDialog(saveFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    WriteFile writeFile = new WriteFile(new WriteFileParams(model1) { Content = (contentType)saveFileAddOn.ContentOption, SerializationMode = (serializationType)saveFileAddOn.SerialOption, SelectedOnly = saveFileAddOn.SelectedOnly, Purge = saveFileAddOn.Purge }, saveFileDialog.FileName);
                    model1.StartWork(writeFile);
                    openButton.IsEnabled = false;
                    saveButton.IsEnabled = false;
                    importButton.IsEnabled = false;
                }
            }
        }

        private void importButton_OnClick(object sender, RoutedEventArgs e)
        {
            using (var importFileDialog1 = new System.Windows.Forms.OpenFileDialog())
            using (var importFileAddOn = new ImportFileAddOn())
            {
                string theFilter = "All compatible file types (*.*)|*.asc;*.stl;*.obj;*.las;*.3ds"
#if NURBS 
                               + ";*.igs;*.iges;*.stp;*.step"
#endif

#if SOLID
                               + ";*.ifc;*.ifczip"
#endif
                               + "|Points (*.asc)|*.asc|" + "WaveFront OBJ (*.obj)|*.obj|" + "Stereolithography (*.stl)|*.stl|" + "Laser LAS (*.las)|*.las|" + "3D Studio Max (*.3ds)|*.3ds";
#if NURBS
                theFilter += "|IGES (*.igs; *.iges)|*.igs; *.iges|" + "STEP (*.stp; *.step)|*.stp; *.step";
#endif

#if SOLID
                theFilter += "|IFC (*.ifc; *.ifczip)|*.ifc; *.ifczip";
#endif
                importFileDialog1.Filter = theFilter;

                importFileDialog1.Multiselect = false;
                importFileDialog1.AddExtension = true;
                importFileDialog1.CheckFileExists = true;
                importFileDialog1.CheckPathExists = true;

                if (importFileDialog1.ShowDialog(importFileAddOn, null) == System.Windows.Forms.DialogResult.OK)
                {
                    model1.Clear();
                    _yAxisUp = importFileAddOn.YAxisUp;

                    ReadFileAsync rfa = getReader(importFileDialog1.FileName);

                    if (rfa != null)
                    {
                        model1.StartWork(rfa);

                        model1.SetView(viewType.Trimetric, true, model1.AnimateCamera);

                        openButton.IsEnabled = false;
                        saveButton.IsEnabled = false;
                        importButton.IsEnabled = false;
                    }
                }
            }
        }

        private void exportButton_OnClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            string theFilter = "WaveFront OBJ (*.obj)|*.obj|" + "Stereolithography (*.stl)|*.stl|" + "Laser LAS (*.las)|*.las|" + "WebGL (*.html)|*.html";
#if NURBS
            theFilter += "|STandard for the Exchange of Product (*.step)|*.step|" + "Initial Graphics Exchange Specification (*.iges)|*.iges";
#endif
            saveFileDialog1.Filter = theFilter;

            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WriteFileAsync wfa = null;
                WriteParams dataParams;

                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        dataParams = new WriteParamsWithMaterials(model1);
                        wfa = new WriteOBJ((WriteParamsWithMaterials)dataParams, saveFileDialog1.FileName);
                        break;

                    case 2:
                        dataParams = new WriteParams(model1);
                        wfa = new WriteSTL(dataParams, saveFileDialog1.FileName);
                        break;
                    case 3:
                        dataParams = null;
                        wfa = new WriteLAS(model1.Entities.Where(x => x is FastPointCloud).FirstOrDefault() as FastPointCloud, saveFileDialog1.FileName);
                        break;
                    case 4:
                        dataParams = new WriteParamsWithMaterials(model1);
                        wfa = new WriteWebGL((WriteParamsWithMaterials)dataParams, model1.DefaultMaterial, saveFileDialog1.FileName);
                        break;
#if NURBS
                    case 5:
                        dataParams = new WriteParamsWithUnits(model1);
                        wfa = new WriteSTEP((WriteParamsWithUnits)dataParams, saveFileDialog1.FileName);
                        break;

                    case 6:
                        dataParams = new WriteParamsWithUnits(model1);
                        wfa = new WriteIGES((WriteParamsWithUnits)dataParams, saveFileDialog1.FileName);
                        break;
#endif
                }

                model1.StartWork(wfa);
            }
        }

        private ReadFileAsync getReader(string fileName)
        {
            string ext = System.IO.Path.GetExtension(fileName);

            if (ext != null)
            {
                ext = ext.TrimStart('.').ToLower();

                switch (ext)
                {
                    case "asc":
                        return new ReadASC(fileName);
                    case "stl":
                        return new ReadSTL(fileName);
                    case "obj":
                        return new ReadOBJ(fileName);
                    case "las":
                        return new ReadLAS(fileName);
                    case "3ds":
                        return new Read3DS(fileName);
#if NURBS
                    case "igs":
                    case "iges":
                        return new ReadIGES(fileName);
                    case "stp":
                    case "step":
                        return new ReadSTEP(fileName);
#endif
#if SOLID
                    case "ifc":
                    case "ifczip":
                        return new ReadIFC(fileName);
#endif
                }
            }

            return null;
        }


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


        private void magGlassCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            if ((bool)magGlassCheckBox.IsChecked)
            {                
                model1.ActionMode = actionType.MagnifyingGlass;
            }
            else
            {
                model1.ActionMode = actionType.None;
            }
            model1.Invalidate();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (Window win in System.Windows.Application.Current.Windows)            
                win.Close();            
        }

        private void clipButton_Click(object sender, RoutedEventArgs e)
        {
            if (model1 != null)
            {
                if ((bool)clipButton.IsChecked)
                {
                    // enables a clippingPlane
                    model1.ClippingPlane1.Edit(null);
                    tabControl1.IsEnabled = false;
                }
                else
                {
                    // disables the clippingPlane and its change
                    model1.ClippingPlane1.Cancel();
                    tabControl1.IsEnabled = true;
                }

                model1.Invalidate();
            }
        }

        private void OpenCurrentButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (openCurrentButton.IsChecked.HasValue && openCurrentButton.IsChecked.Value)
                model1.Entities.OpenCurrentBlockReference();
            else
                model1.Entities.CloseCurrentBlockReference();

            model1.Invalidate();
        }

        private bool _asyncRegen;

        private void regenAsyncButton_OnClick(object sender, RoutedEventArgs e)
        {
            _asyncRegen = (regenAsyncButton.IsChecked == true) ? true : false;
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

        private static string _assetsPath = null;
        public static string GetAssetsPath()
        {
            if (String.IsNullOrEmpty(_assetsPath))
            {
                string path = "../../../../../../dataset/Assets/";
                if (System.IO.Directory.Exists(path))
                    _assetsPath = path;
                else
                {
                    string product, title, company, edition;
                    Version version;
                    devDept.Eyeshot.Environment.GetAssembly(out product, out title, out company, out version, out edition);
                    _assetsPath = String.Format("{0}\\{1} {2} {3} Samples\\dataset\\Assets\\", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), product, edition, version.Major);
                }
            }

            return _assetsPath;
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