using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using devDept.CustomControls;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using devDept.Graphics;
using Microsoft.Win32;
using Block = devDept.Eyeshot.Block;
using Environment = devDept.Eyeshot.Environment;

namespace WpfApplication1
{

    /// <summary>
    /// Interaction logic for DrawingsUserControl.xaml
    /// </summary>
    public partial class DrawingsUserControl : UserControl
    {
        private bool _treeIsDirty = true;

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public Model Model { get; set; }

        public DrawingsUserControl()
        {
            InitializeComponent();

            // sets data for DrawingsPanel control
            drawingsPanel1.drawings = drawings1;

            drawings1.ActionMode = actionType.SelectVisibleByPick;

            drawings1.WorkCompleted += Drawings1OnWorkCompleted;

            drawings1.ProgressBar.Visible = false;
        }

        private void AddDefaultViews(Sheet sheet)
        {
            // this samples uses values in millimeters to add views and it uses this factor to get converted values.
            double unitsConversionFactor = Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, sheet.Units);

            double scaleFactor = drawingsPanel1.GetScaleComboValue();

            // adds Front vector view
            sheet.Entities.Add(new VectorView(70 * unitsConversionFactor, 230 * unitsConversionFactor, viewType.Top, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Top)));
            // adds Trimetric raster view            
            sheet.Entities.Add(new RasterView(150 * unitsConversionFactor, 230 * unitsConversionFactor, viewType.Trimetric, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Trimetric, true)));
            // adds Top vector view
            sheet.Entities.Add(new VectorView(70 * unitsConversionFactor, 130 * unitsConversionFactor, viewType.Front, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Front)));
            // adds Right vector view
            sheet.Entities.Add(new VectorView(150 * unitsConversionFactor, 130 * unitsConversionFactor, viewType.Right, scaleFactor, DrawingsPanel.GetViewName(sheet, viewType.Right)));     
        }


        public Entity[] AddSampleDimSheet(Sheet s, bool addScene)
        {
            var sheet = s;
            double scaleFactor = drawingsPanel1.GetScaleComboValue();
            double unitsConversionFactor = Utility.GetLinearUnitsConversionFactor(linearUnitsType.Millimeters, sheet.Units);
            Point3D linearDimPos = Model.Entities.BoxSize;

            // invalid case
            if (double.IsInfinity(linearDimPos.X) || double.IsInfinity(linearDimPos.Y)|| double.IsInfinity(linearDimPos.Z))
                linearDimPos = new Point3D(0, 0, 0);

            var dim1Plane = new Plane(Point3D.Origin, -1 * Vector3D.AxisY, Vector3D.AxisX); //Plane -Y X;
            var toAdd = new List<Entity>
            {
                // adds linear dims of the box size
                new LinearDim(Plane.XY,
                    new Point2D(70 - (linearDimPos.X * scaleFactor) / 2, 230 - (linearDimPos.Y * scaleFactor) / 2),
                    new Point2D(70 + (linearDimPos.X * scaleFactor) / 2, 230 - (linearDimPos.Y * scaleFactor) / 2),
                    new Point2D(70, 220 - (linearDimPos.Y * scaleFactor) / 2),
                    3),
                new LinearDim(Plane.XY,
                    new Point2D(150 - (linearDimPos.Y * scaleFactor) / 2, 130 - (linearDimPos.Z * scaleFactor) / 2),
                    new Point2D(150 + (linearDimPos.Y * scaleFactor) / 2, 130 - (linearDimPos.Z * scaleFactor) / 2),
                    new Point2D(150, 120 - (linearDimPos.Z * scaleFactor) / 2),
                    3),
                new LinearDim(new Plane(Point3D.Origin, Vector3D.AxisY, new Vector3D(-1,0,0)), 
                    new Point2D(130 - (linearDimPos.Z * scaleFactor) / 2, -70 - (linearDimPos.X * scaleFactor) / 2),
                    new Point2D(130 + (linearDimPos.Z * scaleFactor) / 2, -70 - (linearDimPos.X * scaleFactor) / 2),
                    new Point2D(130, -80  - (linearDimPos.X * scaleFactor) / 2), //move away from expansion
                    3)

               //new devDept.Eyeshot.Entities.Point(new Plane(Point3D.Origin, Vector3D.AxisY, new Vector3D(-1,0,0)), new Point2D(130, -80  - (linearDimPos.X * scaleFactor) / 2)) //center of linearDim text
            };

            return AddLinearDimsToSheet(toAdd, ref sheet, unitsConversionFactor, scaleFactor, addScene);
        }

        private Entity[] AddLinearDimsToSheet(List<Entity> toAdd,ref Sheet sheet, double unitsConversionFactor,double scale, bool addScene)
        {
            foreach (var ent in toAdd)
            {
                if (ent is LinearDim)
                {
                    LinearDim ld = (LinearDim) ent;
                    ld.Scale(unitsConversionFactor);
                    // sets the same layer as wires segments
                    ld.LayerName = drawings1.WiresLayerName;
                    // sets the linear scale as the inverted of the sheet scale factor.
                    ld.LinearScale = 1 / scale;
                }

                if (addScene)
                    sheet.Entities.Add(ent);
            }

            return toAdd.ToArray();
        }
        
        /// <summary>
        /// Create a new sheet with some default views according to the format type.
        /// </summary>
        /// <param name="name">The name for the sheet.</param>
        /// <param name="units">The measurement system type for the sheet.</param>
        /// <param name="formatType">The <see cref="formatType"/>.</param>        
        /// <remarks>
        /// It builds the format block and it adds the created BlockReference to the Sheet and the block to the Drawings Blocks collection.
        /// </remarks>
        public void AddSheet(string name, linearUnitsType units, formatType formatType, bool addDefaultView = true)
        {
            Tuple<double, double> size = DrawingsPanel.GetFormatSize(units, formatType);
            Sheet sheet = new Sheet(units, size.Item1, size.Item2, name);

            Block block;
            BlockReference br = drawingsPanel1.CreateFormatBlock(formatType, sheet, out block);
            drawings1.Blocks.Add(block);

            sheet.Entities.Add(br);  // not possible adding the entity to Drawings because the control handle is not created yet. it will be added when this sheet will be set as the active one.
            drawings1.Sheets.Add(sheet);

            // adds a set of default views.
            if(addDefaultView)
                AddDefaultViews(sheet);
        }

        /// <summary>
        /// Adds a default sheet.
        /// </summary>
        public void AddDefaultSheet()
        {
            AddSheet("Sheet1", linearUnitsType.Millimeters, formatType.A3_ISO, false);
        }

        /// <summary>
        /// Clears the drawings and the treePanel.
        /// </summary>
        public void Clear()
        {
            drawings1.Clear();
            drawingsPanel1.ClearTreeView();
            _treeIsDirty = true;
        }

        /// <summary>
        /// Sets the enable status of the input controls.
        /// </summary>
        /// <param name="status"></param>
        public void EnableUIElements(bool status)
        {
            drawingsPanel1.Enabled = status;
            addLinearDimButton.IsEnabled = status;
            exportSheetButton.IsEnabled = status;
            rebuildButton.IsEnabled = status;
            printButton.IsEnabled = status;
        }

        #region Event Handlers   
        
        private void DrawingsPanel1OnSelectionChanged(object sender, EntityEventArgs e)
        {
            propertyGrid1.SelectedObject = e.Item;
        }

        private void DrawingsPanel1OnViewAdded(object sender, EntityEventArgs e)
        {
            EnableUIElements(false);
            ((View)e.Item).Rebuild(Model, drawings1.GetActiveSheet(), drawings1, true);
        }

        private void AddLinearDimButton_Click(object sender, RoutedEventArgs e)
        {
            if (drawings1.DrawingLinearDim)
            {
                drawings1.DisableDimensioning();
            }
            else
            {
                drawings1.EnableDimensioning();
            }
        }

        /// <summary>
        /// Export the active sheet in the model space of the output file.
        /// </summary>
        private void ExportSheetButton_Click(object sender, RoutedEventArgs e)
        {
            var exportFileDialog = new SaveFileDialog();

            exportFileDialog.Filter = "CAD drawings(*.dwg)| *.dwg|" + "Drawing Exchange Format (*.dxf)|*.dxf";
            exportFileDialog.AddExtension = true;
            exportFileDialog.Title = "Export";
            exportFileDialog.CheckPathExists = true;

            if (exportFileDialog.ShowDialog() == true)
            {
                EnableUIElements(false);

                WriteAutodeskParams wap = new WriteAutodeskParams(drawings1);
                WriteAutodesk wa = new WriteAutodesk(wap, exportFileDialog.FileName);
                drawings1.StartWork(wa);
            }

        }

        public void Drawings1OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                drawingsPanel1.PurgeActiveSheet();

                propertyGrid1.SelectedObject = null;
            }
        }

        public void Drawings1OnSelectionChanged(object sender, Environment.SelectionChangedEventArgs e)
        {
            Entity selected = null;
            foreach (var entity in drawings1.Entities)
            {
                if (entity.Selected)
                    selected = entity; // returns the last object selected
            }

            propertyGrid1.SelectedObject = selected;
        }

        public void Drawings1OnWorkCompleted(object sender, WorkCompletedEventArgs e)
        {
            ViewBuilder vb = e.WorkUnit as ViewBuilder;
            if (vb != null)
            {
                vb.AddToDrawings(drawings1);

                if (drawings1.GetActiveSheet() == null)
                {
                    drawingsPanel1.ActivateSheet(drawings1.Sheets[0].Name);

                    if (_treeIsDirty)
                    {
                        drawingsPanel1.SyncTree();
                        _treeIsDirty = false;
                    }

                    drawings1.ZoomFit();
                }
            }

            EnableUIElements(true);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (drawings1.PageSetup(true, true, 0) == false) return;
            drawings1.PrintPreview(new System.Drawing.Size(800, 600));
        }

        private void PropertyGrid1_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            // updates the entities
            drawings1.Entities.Regen();

            // refresh
            drawings1.Invalidate();
        }

        public void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            EnableUIElements(false);
            drawings1.GetActiveSheet().Rebuild(Model, drawings1, true); //reload partially
        }

        #endregion

    }
}
