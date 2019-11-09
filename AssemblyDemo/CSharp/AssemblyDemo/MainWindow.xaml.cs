using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using devDept.Eyeshot;
using devDept.Graphics;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using devDept.Eyeshot.Translators;
using devDept.Serialization;
using devDept.CustomControls;
using Environment = devDept.Eyeshot.Environment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

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

            // sets data for AssemblyBrowser control
            assemblyTreeView1.Model = model1;
            assemblyTreeView1.InitializeContextMenu();

            // connects assemblyBrowser to MyEntityList of MyModel
            ((MyEntityList) model1.Entities).assemblyTree = assemblyTreeView1;

            // Listens the events to handle the deletion of the selected entity
            model1.KeyDown += Model1_KeyDown;
            assemblyTreeView1.KeyDown += assemblyTreeView1_KeyDown;

            // helper for Turbo button color
            model1.CameraMoveBegin += model1_CameraMoveBegin;

            // event needed for asynchronous Read/Write
            model1.WorkCompleted += model1_WorkCompleted;

            model1.ObjectManipulator.ShowOriginalWhileEditing = false;

            // settings to improve performance for heavy geometry
            model1.Rendered.SilhouettesDrawingMode = silhouettesDrawingType.Never;
            model1.Rendered.ShadowMode = shadowType.None;
            model1.Turbo.OperatingMode = operatingType.Boxes;
            
            // set combobox defaults
            operatingModeComboBox.DataContext = Enum.GetValues(typeof(operatingType));
            operatingModeComboBox.SelectedItem = operatingType.Boxes;
            lagLabel.Content = "";

            // to be able to get the center of rotation on not current entities 
            model1.WriteDepthForTransparents = true;
            model1.Rotate.ShowCenter = true;

            model1.Backface.ColorMethod = backfaceColorMethodType.Cull;
        }
        
        protected override void OnContentRendered(EventArgs e)
        {            
            string fileName;
#if SETUP
            Version Version;
            string Product, Title, Company, Edition;

            Environment.GetAssembly(out Product, out Title, out Company, out Version, out Edition);
            
            fileName = String.Format(@"{0}\Eyeshot {1} {2} Samples\dataset\Assets\AssemblyDemo.eye", System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Edition, Version.Major);
#else
            fileName = "../../../../../../dataset/Assets/AssemblyDemo.eye";
#endif
            // Model import    
            model1.OpenFile(fileName);

            assemblyTreeView1.PopulateTree(model1.Entities);
            // sets selection mode
            model1.ActionMode = actionType.SelectVisibleByPick;

            // sets camera orientation
            model1.SetView(viewType.Isometric);

            // enables Turbo when the scene exceeds 3000 objects
            _maxComplexity = model1.Turbo.MaxComplexity = 3000;

            // Fits the model in the viewport
            model1.ZoomFit();
            model1.Invalidate();

            model1.DisplayMode = displayType.Rendered;

            base.OnContentRendered(e);
        }

        #region Selected entity deletion
        private void assemblyTreeView1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            CheckDelete(e.KeyCode);
        }

        private void Model1_KeyDown(object sender, KeyEventArgs e)
        {
            CheckDelete((Keys)KeyInterop.VirtualKeyFromKey(e.Key));
        }

        private void CheckDelete(Keys e)
        {
            if (e == Keys.Delete)
            {
                for (var i = assemblyTreeView1.SelectedNodes.Count - 1; i >= 0; i--)
                {
                    TreeNode selectedNode = assemblyTreeView1.SelectedNodes[i];
                    if (selectedNode != null)
                    {
                        if (assemblyTreeView1.SelectedItems[i] != null && assemblyTreeView1.SelectedItems[i].Item != null)
                        {
                            var entity = assemblyTreeView1.SelectedItems[i].Item as Entity;

                            if (selectedNode.Parent != null && selectedNode.Parent.Tag != null)
                            {
                                var parent =
                                    ((devDept.CustomControls.AssemblyTreeView.NodeTag)selectedNode.Parent.Tag).Entity as BlockReference;

                                var parentBlockName = parent.BlockName;

                                // removes the entity from the block where it's present
                                model1.Blocks[parentBlockName].Entities.Remove(entity);
                            }
                            else
                            {
                                // in case the entity to delete is a root level entity
                                model1.Entities.DeleteSelected();
                            }
                        }

                    }

                }

                assemblyTreeView1.DeleteSelectedNodes();

                // update selection data
                assemblyTreeView1.SelectedNodes.Clear();
                assemblyTreeView1.SelectedItems.Clear();

                model1.Entities.UpdateBoundingBox();
                model1.Invalidate();
            }
        }

        #endregion

        private void leafSelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //invert assembly selection mode
            model1.AssemblySelectionMode = chkLeafSelection.IsEnabled ? Environment.assemblySelectionType.Leaf : Environment.assemblySelectionType.Branch;
            model1.Entities.ClearSelection();
            model1.Invalidate();

            assemblyTreeView1.SelectedNodes.Clear();
            assemblyTreeView1.SelectedItems.Clear();
        }

        #region Read/Write
        private bool _yAxisUp;

        private void model1_WorkCompleted(object sender, devDept.Eyeshot.WorkCompletedEventArgs e)
        {
            if (e.WorkUnit is ReadFileAsync)
            {
                assemblyTreeView1.ClearCurrent(true);

                ReadFileAsync ra = (ReadFileAsync)e.WorkUnit;

                if (_yAxisUp)
                    ra.RotateEverythingAroundX();

                // updates model units and its related combo box
                if (e.WorkUnit is ReadFileAsyncWithBlocks)
                {
                    model1.Units = ((ReadFileAsyncWithBlocks)e.WorkUnit).Units;
                }

                ra.AddToScene(model1, new RegenOptions() { Async = true });
            }
            else if (e.WorkUnit is Regeneration)
            {
                assemblyTreeView1.PopulateTree(model1.Entities);

                model1.Entities.UpdateBoundingBox();
                UpdateFastZprButton();
                model1.ZoomFit();
                model1.Invalidate();
            }

            openButton.IsEnabled = true;
            saveButton.IsEnabled = true;
            importButton.IsEnabled = true;
            exportButton.IsEnabled = true;
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

        private void importButton_Click(object sender, EventArgs e)
        {
            using (var importFileDialog1 = new OpenFileDialog())
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
                    assemblyTreeView1.ClearTree();
                    if (model1.Entities.IsOpenCurrentBlockReference)
                        model1.Entities.CloseCurrentBlockReference();
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
                        exportButton.IsEnabled = false;
                    }
                }
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            string theFilter = "STandard for the Exchange of Product (*.step)|*.step|" + "Initial Graphics Exchange Specification (*.iges)|*.iges";

            saveFileDialog1.Filter = theFilter;

            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;

            var result = saveFileDialog1.ShowDialog();

            if (result.Value)
            {
                WriteFileAsync wfa = null;
                WriteParams dataParams = null;
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

                openButton.IsEnabled = false;
                saveButton.IsEnabled = false;
                importButton.IsEnabled = false;
                exportButton.IsEnabled = false;
            }
        }

        private OpenFileAddOn _openFileAddOn;
        private void openButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
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
                    assemblyTreeView1.ClearTree();
                    if (model1.Entities.IsOpenCurrentBlockReference)
                        model1.Entities.CloseCurrentBlockReference();
                    model1.Clear();

                    _yAxisUp = false;
                    ReadFile readFile = new ReadFile(openFileDialog1.FileName, (contentType)_openFileAddOn.ContentOption);
                    model1.StartWork(readFile);
                    model1.SetView(viewType.Trimetric, true, model1.AnimateCamera);
                    openButton.IsEnabled = false;
                    saveButton.IsEnabled = false;
                    importButton.IsEnabled = false;
                    exportButton.IsEnabled = false;

                    model1.Invalidate();
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

        private void saveButton_Click(object sender, EventArgs e)
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
                    exportButton.IsEnabled = false;
                }
            }
        }
        #endregion

        #region Turbo controls

        private int _maxComplexity = 3000;
        private void turboCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (turboCheckBox.IsChecked.Value == false)
            {
                _maxComplexity = model1.Turbo.MaxComplexity;
                model1.Turbo.MaxComplexity = int.MaxValue;
            }
            else
            {
                model1.Turbo.MaxComplexity = _maxComplexity;
            }

            operatingModeComboBox.IsEnabled = turboCheckBox.IsChecked.Value;
            lagLabel.IsEnabled = turboCheckBox.IsChecked.Value;

            model1.Entities.UpdateBoundingBox();
            UpdateFastZprButton();
        }
        
        private void operatingModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(model1 != null) //with SelectedIndex="3" (see xaml) this is called before the model initialization
            {
                model1.Turbo.OperatingMode = (operatingType) operatingModeComboBox.SelectedIndex;
                model1.Entities.UpdateBoundingBox();
                UpdateFastZprButton();
            }
        }

        private void UpdateFastZprButton()
        {
            if (model1.Turbo.Enabled)
            {
                turboCheckBox.Style = FindResource("FastZprToggleButtonStyle") as Style;
                lagLabel.Content = (model1.Turbo.Lag / 1000.0).ToString("f1") + " s";
            }
            else
            {
                turboCheckBox.Style = FindResource("ToggleButtonStyle") as Style;
                lagLabel.Content = string.Empty;
            }
        }

        private void model1_CameraMoveBegin(object sender, Environment.CameraMoveEventArgs e)
        {
            UpdateFastZprButton();
        }

        #endregion
       
    }
}